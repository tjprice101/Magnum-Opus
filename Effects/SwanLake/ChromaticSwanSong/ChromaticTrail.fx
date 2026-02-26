// =====================================================================
//  ChromaticTrail.fx — Chromatic Swan Song projectile trail
// =====================================================================
//
//  Visual: Rainbow-banded projectile trail whose saturation and band
//  count escalate with uPhase (combo progression). At phase 0 the trail
//  is a desaturated silver-white whisper. At phase 1 it erupts into a
//  vivid full-spectrum HSL sweep with 7+ color bands. A white-hot core
//  runs through the center regardless of phase.
//
//  Musical shimmer pattern gives the trail a "vibrating string" quality
//  appropriate for the Swan Song's aria theme.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (projectile tip), 1 = tail
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    ChromaticTrailMain — Spectral-banded core with musical shimmer
//    ChromaticTrailGlow — Soft prismatic bloom underlay
//
//  C# rendering order (3 passes):
//    1. ChromaticTrailGlow @ 3x width   (soft rainbow bloom)
//    2. ChromaticTrailMain @ 1x width   (sharp spectral bands)
//    3. ChromaticTrailGlow @ 1.5x width (overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Noise texture (SparklyNoiseTexture)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (Silver)
float3 uSecondaryColor;   // Secondary trail color (PureWhite)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Chromatic-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Shimmer distortion amount
float  uPhase;            // Combo progression: 0 = first shot, 0.5 = second, 1.0 = third
float  uHasSecondaryTex;  // 1.0 if noise texture bound
float  uSecondaryTexScale; // Noise repetition
float  uSecondaryTexScroll; // Noise scroll speed

// =====================================================================
//  Utility
// =====================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// HSL hue to RGB for spectral banding
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 HSLToRGB(float h, float s, float l)
{
    float3 rgb = HueToRGB(frac(h));
    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    return (rgb - 0.5) * c + l;
}

// =====================================================================
//  Spectral banding — combo-driven rainbow
// =====================================================================

float3 SpectralBand(float u, float v, float time, float comboPhase)
{
    // Band count increases with combo: 2 bands at phase 0, 7+ at phase 1
    float bandCount = 2.0 + comboPhase * 5.0;

    // Hue cycles along trail
    float hue = frac(u * bandCount + time * 0.2);

    // Saturation scales with combo — desaturated at 0, vivid at 1
    float saturation = 0.1 + comboPhase * 0.85; // 0.1 -> 0.95

    // Luminance — bright core
    float luminance = 0.8 - abs(v - 0.5) * 0.4;

    float3 spectral = HSLToRGB(hue, saturation, luminance);

    // Cross-width polarity tint: slightly darker above center, brighter below
    float polarityShift = smoothstep(0.3, 0.7, v) * 0.15;
    spectral += polarityShift;

    return spectral;
}

// =====================================================================
//  ChromaticTrailMain — Spectral-banded core with musical shimmer
// =====================================================================

float4 ChromaticTrailMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Medium-sharp edge profile
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 2.2);

    // Trail fade — moderate length
    float trailFade = pow(saturate(1.0 - coords.x), 1.5);

    // Noise-driven shimmer distortion
    float shimmerOffset = 0.0;
    float sparkleHighlight = 0.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 noiseUV = float2(coords.x * uSecondaryTexScale - uTime * uScrollSpeed * 0.5,
                                coords.y * uSecondaryTexScale * 0.6 + uTime * uSecondaryTexScroll * 0.15);
        float noiseSample = tex2D(uImage1, noiseUV).r;

        shimmerOffset = (noiseSample - 0.5) * uDistortionAmt;

        // Sparkle highlights — bright convergence points
        float2 sparkleUV = noiseUV * 1.5 + float2(uTime * 0.12, 0.0);
        float sparkle2 = tex2D(uImage1, sparkleUV).r;
        sparkleHighlight = smoothstep(0.65, 0.9, noiseSample * sparkle2 * 2.0) * 0.25;
    }

    // Scrolled UV with shimmer
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + shimmerOffset);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === SPECTRAL BANDING ===
    float3 spectralColor = SpectralBand(coords.x, coords.y, uTime, uPhase);

    // At low combo, blend heavily toward base silver color
    float3 baseColor = lerp(uColor, uSecondaryColor, coords.y * 0.5);
    float comboBlend = uPhase * 0.8; // 0 = mostly silver, 1 = mostly spectral
    float3 trailColor = lerp(baseColor, spectralColor, comboBlend);

    // === WHITE-HOT CORE ===
    // Always present — all colors combined = white at center
    float centerGlow = exp(-pow((coords.y - 0.5) / 0.07, 2.0));
    trailColor = lerp(trailColor, float3(0.98, 0.97, 1.0), centerGlow * 0.4 * trailFade);

    // === MUSICAL SHIMMER ===
    // Vibrating string pattern — standing wave along trail
    float musicalShimmer = sin(coords.x * 12.0 + uTime * 3.0) * sin(coords.y * 8.0 - uTime * 1.5);
    musicalShimmer = musicalShimmer * 0.5 + 0.5; // Normalize to 0-1
    float shimmerStrength = 0.1 + uPhase * 0.15; // Stronger at higher combo
    trailColor += float3(1.0, 1.0, 1.0) * musicalShimmer * shimmerStrength * trailFade;

    // Add sparkle highlights
    trailColor += float3(1.0, 0.98, 0.95) * sparkleHighlight * trailFade;

    // === TAIL DISSOLVE ===
    float tailDissolve = 1.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 dissolveUV = float2(coords.x * 2.5 + uTime * 0.25, coords.y * 1.5);
        float dissolveNoise = tex2D(uImage1, dissolveUV).r;
        float threshold = coords.x * 1.15 - 0.05;
        tailDissolve = smoothstep(threshold - 0.12, threshold + 0.08, dissolveNoise);
        tailDissolve = lerp(1.0, tailDissolve, smoothstep(0.5, 0.85, coords.x));
    }

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * tailDissolve * uOpacity;
    float3 finalColor = trailColor * uIntensity;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  ChromaticTrailGlow — Soft prismatic bloom
// =====================================================================

float4 ChromaticTrailGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.2);

    // Spectral glow — softer version of main banding
    float hue = frac(coords.x * (1.5 + uPhase * 2.0) + uTime * 0.1);
    float saturation = 0.15 + uPhase * 0.4;
    float3 glowColor = HSLToRGB(hue, saturation, 0.8);

    // Blend toward base color at low combo
    glowColor = lerp(lerp(uColor, uSecondaryColor, 0.4), glowColor, uPhase * 0.6);

    // Breathing pulse — musical tempo
    float pulse = 0.85 + 0.15 * sin(uTime * 2.5 + coords.x * 3.0);

    // Low opacity bloom
    float alpha = edgeFade * trailFade * uOpacity * 0.3 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique ChromaticTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ChromaticTrailMainPS();
    }
}

technique ChromaticTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 ChromaticTrailGlowPS();
    }
}
