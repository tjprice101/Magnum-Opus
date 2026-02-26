// =============================================================================
// Heroic Flame Trail Shader - PS 2.0 Compatible
// =============================================================================
// Burning valor flame trail with turbulent fire distortion.
// Designed for CalamityStyleTrailRenderer primitive geometry.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   HeroicFlameFlow  - Main flame trail with fire turbulence
//   HeroicFlameGlow  - Softer additive bloom overlay for glow stacking
//
// Features:
//   - Turbulent fire distortion with flickering edges
//   - QuadraticBump edge fade with ember-hot center
//   - White-hot core ↁEscarlet ↁEgold gradient
//   - Noise texture modulation for organic flame flicker
//   - Overbright multiplier for HDR bloom
//   - Designed for multi-pass rendering (C# renders 3 passes)
// =============================================================================

sampler uImage0 : register(s0); // Base trail texture / white gradient
sampler uImage1 : register(s1); // Noise texture (e.g. PerlinNoise, NoiseSmoke)

float3 uColor;           // Primary color (e.g. Scarlet)
float3 uSecondaryColor;  // Secondary color (e.g. Gold)
float uOpacity;          // Overall opacity (lifecycle fade)
float uTime;             // Animation time (Main.GlobalTimeWrappedHourly)
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier (2-5 for glow)
float uScrollSpeed;       // Flame flow scroll rate (default 1.5)
float uNoiseScale;        // Noise UV repetition (default 3.0)
float uDistortionAmt;     // Fire turbulence strength (default 0.08)
float uHasSecondaryTex;   // 1.0 if noise texture bound, 0.0 if not
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed

// =============================================================================
// UTILITY
// =============================================================================

// 0->1->0 bump (peak at centre)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: HEROIC FLAME FLOW
// =============================================================================
// Turbulent flame trail that burns with valor. Fire licks upward from
// the trail edges while the core remains white-hot. Flame distortion
// creates organic flicker, evoking a sword wreathed in heroic fire.
// =============================================================================

float4 HeroicFlameFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Fire turbulence distortion ---
    // Primary: aggressive vertical flicker (flames lick upward)
    float flicker1 = sin(coords.x * 8.0 + uTime * uScrollSpeed * 4.0) * uDistortionAmt;
    // Secondary: lateral flame tongues
    float flicker2 = sin(coords.x * 14.0 - uTime * uScrollSpeed * 6.0 + coords.y * 5.0) * uDistortionAmt * 0.6;
    // Tertiary: rapid micro-flicker for heat shimmer
    float flicker3 = sin(coords.x * 20.0 + uTime * uScrollSpeed * 10.0) * uDistortionAmt * 0.2;

    float2 distortedUV = coords;
    distortedUV.y += flicker1 + flicker3;
    distortedUV.x += flicker2;

    // Sample base texture with distorted UVs
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Noise texture for organic flame variation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll * 1.5;
    noiseUV.y -= uTime * 0.4; // Flames rise upward
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.65, noiseTex.r, uHasSecondaryTex);

    // --- Edge-to-centre fade (QuadraticBump) ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Trail length fade (head bright, tail embers) ---
    float trailFade = saturate(1.0 - coords.x * 1.1);
    // Ember tail: instead of full fadeout, embers glow dimly at the end
    float emberTail = saturate(coords.x * 2.0 - 0.8) * 0.15;

    // --- Flame gradient along trail ---
    // Head: white-hot ↁEscarlet body ↁEgold embers at tail
    float gradientT = coords.x * 0.7 + noiseVal * 0.3;
    float3 flameColor = lerp(uColor, uSecondaryColor, gradientT);

    // --- White-hot core at beam centre ---
    float coreMask = saturate((edgeFade - 0.50) * 3.0);
    float3 hotCore = float3(1.0, 0.94, 0.78); // Warm white
    flameColor = lerp(flameColor, hotCore, coreMask * 0.65);

    // --- Flame edge: brighter, flickering edge glow ---
    float edgeMask = saturate((0.50 - edgeFade) * 3.5);
    float edgeFlicker = saturate(flicker1 * 6.0 + 0.5);
    flameColor *= 1.0 + edgeMask * edgeFlicker * 0.3;

    // --- Fire cracks: noise-driven bright spots in the flame ---
    float crackMask = saturate(noiseVal * 2.0 - 0.8) * coreMask;
    flameColor += float3(0.3, 0.2, 0.05) * crackMask;

    // --- Rapid pulse (fire flicker rhythm) ---
    float pulse = sin(uTime * 12.0 + coords.x * 8.0) * 0.06 + 0.94;
    pulse *= sin(uTime * 7.0 + coords.x * 15.0) * 0.03 + 0.97; // Dual-freq

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.6 + noiseVal * 0.4; // Noise modulation

    float alpha = (edgeFade * trailFade + emberTail) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: HEROIC FLAME GLOW
// =============================================================================
// Softer, wider glow pass for bloom stacking. Renders behind the
// main flame pass to create a warm fire halo around the core trail.
// =============================================================================

float4 HeroicFlameGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Gentle fire distortion (less intense than main pass)
    float wave = sin(coords.x * 5.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt * 0.4;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    // Wider, softer edge fade for fire halo
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade; // Squared for softer rolloff

    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.85);

    // Warmer gradient, biased toward primary flame color
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.4);

    // Add warm orange tint to glow
    float3 warmTint = float3(1.0, 0.6, 0.2);
    glowColor = lerp(glowColor, warmTint, 0.15);

    // Gentle noise modulation
    float2 noiseUV = coords * uSecondaryTexScale * 0.6;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.7;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.80, noiseTex.r, uHasSecondaryTex * 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // Slow warm pulse
    float pulse = sin(uTime * 3.5 + coords.x * 4.0) * 0.10 + 0.90;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique HeroicFlameFlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 HeroicFlameFlowPS();
    }
}

technique HeroicFlameGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 HeroicFlameGlowPS();
    }
}
