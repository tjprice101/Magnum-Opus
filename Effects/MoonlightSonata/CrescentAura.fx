// =============================================================================
// MagnumOpus Crescent Aura Shader - PS 2.0 Compatible
// =============================================================================
// Renders procedural crescent moon shapes from UV coordinates.
// Used as particle overlays, weapon auras, accessory ambient effects,
// and summon circle decorations for the Moonlight Sonata theme.
//
// UV Layout:
//   U,V = standard quad UVs (0,0 top-left → 1,1 bottom-right)
//   Center of crescent at UV (0.5, 0.5)
//
// Techniques:
//   CrescentShape  — Static crescent with soft inner glow, sharp outer edge
//   CrescentPulse  — Animated pulsing crescent with wax/wane cycle
//
// Features:
//   - Procedural crescent from two offset circles (polar-coordinate inspired)
//   - Phase parameter (0-1) for waxing/waning crescent width
//   - Soft inner glow with sharp outer silhouette edge
//   - Rotation support via pre-rotated UV from vertex shader
//   - Time-animated pulsing for ambient effects
//   - Overbright for HDR bloom integration
// =============================================================================

sampler uImage0 : register(s0); // Base texture (can be white pixel for pure procedural)
sampler uImage1 : register(s1); // Noise texture for subtle variation

float3 uColor;           // Primary crescent color (e.g. Lavender)
float3 uSecondaryColor;  // Inner glow color (e.g. MoonWhite)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uPhase;             // Lunar phase: 0.0 = thin crescent, 0.5 = half moon, 1.0 = full
float uSharpness;         // Edge sharpness (1.0 = soft, 5.0 = sharp, default 3.0)
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale; // Noise UV scale
float uSecondaryTexScroll; // Noise scroll speed

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// Procedural Crescent Moon Shape
// =============================================================================
// Two circles in UV space:
//   - Outer circle: centered at (0.5, 0.5), radius 0.45
//   - Inner circle: offset horizontally, radius 0.4
// The crescent is where the outer circle exists but inner does not.
// Phase controls the offset: phase 0 = max offset (thin crescent),
// phase 1 = zero offset (full circle, no cutout).
// =============================================================================

float CrescentShape(float2 uv, float phase)
{
    // Center of quad
    float2 center = float2(0.5, 0.5);

    // Distance from center for outer circle
    float2 outerDelta = uv - center;
    float outerDist = length(outerDelta);
    float outerRadius = 0.44;

    // Outer circle mask with sharp-ish edge
    float outerMask = saturate((outerRadius - outerDist) * uSharpness * 4.0);

    // Inner circle offset (creates the crescent cutout)
    // Phase 0: max offset (thin crescent), Phase 1: no offset (full moon)
    float innerOffset = (1.0 - phase) * 0.32;
    float2 innerCenter = center + float2(innerOffset, 0.0);
    float2 innerDelta = uv - innerCenter;
    float innerDist = length(innerDelta);
    float innerRadius = 0.38;

    // Inner circle mask (what to subtract)
    float innerMask = saturate((innerRadius - innerDist) * uSharpness * 4.0);

    // Crescent = outer minus inner
    float crescent = saturate(outerMask - innerMask);

    return crescent;
}

// =============================================================================
// Inner Glow Function
// =============================================================================
// Soft radial glow emanating from the crescent's inner curve.
// Creates the "lit from within" effect characteristic of moonlight.
// =============================================================================

float InnerGlow(float2 uv, float phase, float crescentMask)
{
    // Glow source: near the inner edge of the crescent
    float innerOffset = (1.0 - phase) * 0.32;
    float2 glowCenter = float2(0.5 + innerOffset * 0.3, 0.5);
    float glowDist = length(uv - glowCenter);

    // Soft radial falloff for inner glow
    float glow = saturate(1.0 - glowDist * 2.5);
    glow *= glow; // Square for softer falloff

    // Only glow within the crescent shape
    return glow * crescentMask;
}

// =============================================================================
// TECHNIQUE 1: CRESCENT SHAPE
// =============================================================================
// Static crescent with soft inner glow and sharp outer edge.
// Used for weapon hold auras, orbit particles, and decoration.
// =============================================================================

float4 CrescentShapePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Procedural crescent mask ---
    float crescent = CrescentShape(coords, uPhase);

    // --- Inner glow ---
    float glow = InnerGlow(coords, uPhase, crescent);

    // --- Color: primary body + white-ish inner glow ---
    float3 bodyColor = uColor * uIntensity;
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.6);
    float3 finalColor = lerp(bodyColor, glowColor, glow);

    // --- Subtle noise modulation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += uTime * uSecondaryTexScroll * 0.1;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    finalColor *= lerp(1.0, 0.85 + noiseTex.r * 0.3, uHasSecondaryTex * 0.3);

    float alpha = crescent * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor * baseTex.rgb, alpha);
}

// =============================================================================
// TECHNIQUE 2: CRESCENT PULSE
// =============================================================================
// Animated pulsing crescent with wax/wane cycle.
// Phase oscillates over time, creating a breathing crescent effect.
// Used for ambient accessory effects and summon circle decorations.
// =============================================================================

float4 CrescentPulsePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Animated phase: oscillate between thin crescent and half moon ---
    float pulsePhase = uPhase + sin(uTime * 3.0) * 0.2;
    pulsePhase = saturate(pulsePhase);

    // --- Procedural crescent with animated phase ---
    float crescent = CrescentShape(coords, pulsePhase);

    // --- Inner glow with pulse ---
    float glow = InnerGlow(coords, pulsePhase, crescent);

    // --- Color with pulsing brightness ---
    float pulse = sin(uTime * 4.0) * 0.15 + 0.85;
    float3 bodyColor = uColor * uIntensity * pulse;
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.7) * (pulse + 0.15);
    float3 finalColor = lerp(bodyColor, glowColor, glow);

    // --- Noise detail ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    finalColor *= lerp(1.0, 0.8 + noiseTex.r * 0.4, uHasSecondaryTex * 0.35);

    // --- Outer edge bloom: subtle glow beyond crescent edge ---
    float2 centerDelta = coords - 0.5;
    float centerDist = length(centerDelta);
    float outerBloom = saturate((0.46 - centerDist) * 3.0) * 0.2;
    finalColor += uColor * outerBloom;

    float alpha = saturate(crescent + outerBloom) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor * baseTex.rgb, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique CrescentShapeTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentShapePS();
    }
}

technique CrescentPulse
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentPulsePS();
    }
}
