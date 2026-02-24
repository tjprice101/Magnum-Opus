// =============================================================================
// MagnumOpus Moonlight Trail Shader - PS 2.0 Compatible
// =============================================================================
// Flowing lunar trail with "moonlight on water" undulating effect.
// Designed for CalamityStyleTrailRenderer primitive geometry.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   MoonlightFlowTrail  — Main flowing trail with sine-wave water distortion
//   MoonlightGlowPass   — Softer additive bloom overlay for glow stacking
//
// Features:
//   - Dual-axis sine distortion for organic "moonlight on water" shimmer
//   - QuadraticBump edge fade (bright center, soft edges)
//   - White-hot core with palette gradient
//   - Noise texture modulation for organic variation
//   - Overbright multiplier for HDR bloom
//   - Designed for multi-pass rendering (C# renders 3 passes with
//     different palette color pairs for full 6-color gradient effect)
// =============================================================================

sampler uImage0 : register(s0); // Base trail texture / white gradient
sampler uImage1 : register(s1); // Noise texture (e.g. SoftCircularCaustics)

float3 uColor;           // Primary color (e.g. DarkPurple)
float3 uSecondaryColor;  // Secondary color (e.g. IceBlue)
float uOpacity;          // Overall opacity (lifecycle fade)
float uTime;             // Animation time (Main.GlobalTimeWrappedHourly)
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier (2-5 for glow)
float uScrollSpeed;       // Water flow scroll rate (default 1.0)
float uNoiseScale;        // Noise UV repetition (default 3.0)
float uDistortionAmt;     // Sine wave distortion strength (default 0.06)
float uHasSecondaryTex;   // 1.0 if noise texture bound, 0.0 if not
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed

// =============================================================================
// UTILITY
// =============================================================================

// 0→1→0 bump (peak at center, 0 at edges)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: MOONLIGHT FLOW TRAIL
// =============================================================================
// Flowing moonlight-on-water effect with dual sine distortion.
// The sine waves create gentle undulating shimmer along the trail,
// evoking moonlight rippling across a dark lake surface.
// =============================================================================

float4 MoonlightFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Dual-axis sine distortion for "moonlight on water" ---
    // Primary wave: slow lateral undulation along trail length
    float wave1 = sin(coords.x * 6.28 + uTime * uScrollSpeed * 2.0) * uDistortionAmt;
    // Secondary wave: faster cross-axis shimmer for sparkle
    float wave2 = sin(coords.x * 12.56 - uTime * uScrollSpeed * 3.5) * uDistortionAmt * 0.4;

    float2 distortedUV = coords;
    distortedUV.y += wave1;
    distortedUV.x += wave2;

    // Sample base texture with distorted UVs
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Noise texture for organic variation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    noiseUV.y += uTime * 0.15; // Gentle vertical drift
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.75, noiseTex.r, uHasSecondaryTex);

    // --- Edge-to-center fade (QuadraticBump) ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Trail length fade (head bright, tail fades) ---
    float trailFade = saturate(1.0 - coords.x * 1.05);

    // --- Palette gradient along trail ---
    // Smooth blend from primary at head to secondary at tail
    float gradientT = coords.x * 0.8 + noiseVal * 0.2;
    float3 trailColor = lerp(uColor, uSecondaryColor, gradientT);

    // --- White-hot core at beam center ---
    float coreMask = saturate((edgeFade - 0.55) * 3.5);
    trailColor = lerp(trailColor, float3(1.0, 0.97, 1.0), coreMask * 0.55);

    // --- Moonlight shimmer: brighter where sine waves overlap ---
    float shimmer = saturate(wave1 * 8.0 + 0.5) * 0.15 + 0.85;

    // --- Subtle pulse ---
    float pulse = sin(uTime * 4.0 + coords.x * 5.0) * 0.06 + 0.94;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * shimmer * pulse;
    finalColor *= 0.7 + noiseVal * 0.3; // Noise modulation

    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: MOONLIGHT GLOW PASS
// =============================================================================
// Softer, wider glow pass for bloom stacking. Renders behind the main trail
// pass to create a soft lunar halo around the core trail geometry.
// Uses wider edge fade, gentler gradient, higher overbright.
// =============================================================================

float4 MoonlightGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Gentle sine distortion (less intense than main pass)
    float wave = sin(coords.x * 4.0 + uTime * uScrollSpeed * 1.5) * uDistortionAmt * 0.5;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    // Wider, softer edge fade for glow halo
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade; // Squared for softer rolloff

    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.9);

    // Softer gradient, biased toward primary color
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.5);

    // Gentle noise modulation
    float2 noiseUV = coords * uSecondaryTexScale * 0.7;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.5;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.6);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // Slow gentle pulse
    float pulse = sin(uTime * 2.5 + coords.x * 3.0) * 0.08 + 0.92;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique MoonlightFlowTrail
{
    pass P0
    {
        PixelShader = compile ps_2_0 MoonlightFlowPS();
    }
}

technique MoonlightGlowPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 MoonlightGlowPS();
    }
}
