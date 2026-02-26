// =============================================================================
// MagnumOpus Crescent Bloom Shader - PS 2.0 Compatible
// =============================================================================
// Procedural crescent moon bloom overlay that grows across combo phases.
// Renders a crescent shape that transitions from thin sliver to full circle.
//
// Unique to Eternal Moon -- "The Crescent Rises":
//   - Procedural crescent moon shape via two offset circles
//   - Phase parameter controls illumination (0 = new moon, 1 = full moon)
//   - Asymmetric glow with perpendicular blade offset
//   - Soft radial bloom with inner white-hot core
//   - Gentle pulsing aurora at the crescent rim
//
// UV Layout:
//   Centered at (0.5, 0.5) -- radial coordinate system
//   Draws onto a fullscreen quad or sprite
//
// Techniques:
//   CrescentBloomPass -- Main crescent shape with radial bloom
//   CrescentGlowPass -- Wider softer glow for bloom stacking
// =============================================================================

sampler uImage0 : register(s0); // Base texture (soft glow circle)
sampler uImage1 : register(s1); // Noise texture

float3 uColor;           // Primary color (e.g. Violet)
float3 uSecondaryColor;  // Secondary color (e.g. CrescentGlow / ice blue)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Overall brightness multiplier
float uPhase;            // Lunar phase: 0=new moon (sliver), 1=full moon (circle)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Crescent moon shape: intersection of two circles offset by phase
// Returns 0-1 where 1 = illuminated, 0 = shadowed
float CrescentShape(float2 uv, float phase)
{
    // Distance from center
    float dist = length(uv);

    // Outer circle mask (the moon body)
    float outerRadius = 0.42;
    float outerMask = saturate((outerRadius - dist) * 12.0);

    // Shadow circle: offset to the right, creating crescent
    // As phase increases from 0->1, shadow moves further right until it disappears
    float shadowOffset = lerp(0.05, 0.9, phase);
    float2 shadowCenter = float2(shadowOffset, 0.0);
    float shadowDist = length(uv - shadowCenter);
    float shadowRadius = 0.40;
    float shadowMask = saturate((shadowDist - shadowRadius) * 12.0);

    // Crescent = outer circle minus shadow circle
    // At phase=1 (full moon), shadow is so far right it doesn't overlap
    float crescent = outerMask * lerp(shadowMask, 1.0, phase * phase);

    return crescent;
}

// Rim glow: bright edge along the crescent boundary
float RimGlow(float2 uv, float phase)
{
    float dist = length(uv);
    float rimDist = abs(dist - 0.38);
    float rim = exp(-rimDist * rimDist * 80.0);

    // Stronger rim at the illuminated edge
    float angle = atan2(uv.y, uv.x);
    float edgeBias = saturate(sin(angle) * 0.3 + 0.7);

    return rim * edgeBias * (0.3 + phase * 0.7);
}

// =============================================================================
// TECHNIQUE 1: CRESCENT BLOOM PASS
// =============================================================================

float4 CrescentBloomPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Center UV at (0, 0)
    float2 centered = coords - float2(0.5, 0.5);

    // Sample base texture for soft glow envelope
    float4 baseTex = tex2D(uImage0, coords);

    // --- Crescent shape ---
    float crescentMask = CrescentShape(centered, uPhase);

    // --- Rim glow along crescent edge ---
    float rim = RimGlow(centered, uPhase);

    // --- Noise modulation for organic surface detail ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.3;
    noiseUV.y += sin(uTime * 0.5) * 0.1;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex);

    // --- Color gradient: inner core bright, outer edge colored ---
    float dist = length(centered);
    float coreFraction = saturate(1.0 - dist * 3.5);
    float3 crescentColor = lerp(uColor, uSecondaryColor, coreFraction * 0.6 + uPhase * 0.3);

    // White-hot core at center of illuminated area
    float coreHot = coreFraction * coreFraction * 0.5 * uPhase;
    crescentColor = lerp(crescentColor, float3(1.0, 0.98, 1.0), coreHot);

    // Rim highlight: brighter secondary color at the edge
    crescentColor = lerp(crescentColor, uSecondaryColor * 1.3, rim * 0.6);

    // --- Aurora pulse along the crescent rim ---
    float auroraPulse = sin(uTime * 4.0 + atan2(centered.y, centered.x) * 3.0) * 0.1 + 0.9;

    // --- Radial falloff for overall bloom envelope ---
    float radialFalloff = saturate(1.0 - dist * 2.0);
    radialFalloff = radialFalloff * radialFalloff;

    // --- Final composition ---
    float combinedMask = crescentMask + rim * 0.4;
    float3 finalColor = crescentColor * uIntensity * noiseVal * auroraPulse * baseTex.rgb;

    float alpha = combinedMask * radialFalloff * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: CRESCENT GLOW PASS
// =============================================================================

float4 CrescentGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float4 baseTex = tex2D(uImage0, coords);

    float dist = length(centered);

    // Very soft crescent shape for glow
    float crescentMask = CrescentShape(centered, uPhase);

    // Broader radial falloff for glow envelope
    float radialFalloff = saturate(1.0 - dist * 1.5);

    // Simple color blend
    float3 glowColor = lerp(uColor, uSecondaryColor, uPhase * 0.5);

    // Subtle noise for organic feel
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.2;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.4);

    // Breathing pulse
    float pulse = sin(uTime * 2.5) * 0.06 + 0.94;

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    float alpha = crescentMask * radialFalloff * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique CrescentBloomPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentBloomPS();
    }
}

technique CrescentGlowPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentGlowPS();
    }
}
