// =====================================================================
//  DualPolaritySwing.fx — Call of the Black Swan melee swing trail
// =====================================================================
//
//  Visual: Dual-polarity greatsword swing trail. Obsidian black on one
//  edge fading to pure white on the other, with a thin prismatic shimmer
//  band at the collision line where black meets white.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (blade tip), 1 = tail (oldest)
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  uPhase = combo step (0.0 = first swing, 0.5 = second, 1.0 = third/finisher)
//    - Band width and prismatic intensity scale with combo progression
//    - Third swing has maximum rainbow revelation at the collision line
//
//  Techniques:
//    DualPolarityFlow  — Sharp polarity core with prismatic collision band
//    DualPolarityGlow  — Soft bloom underlay with gentle polarity tint
//
//  C# rendering order (3 passes):
//    1. DualPolarityGlow  @ 3x width   (soft underlayer bloom)
//    2. DualPolarityFlow  @ 1x width   (sharp polarity core)
//    3. DualPolarityGlow  @ 1.5x width (bright overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture (ThinGlowLine)
sampler uImage1 : register(s1); // Noise texture (SoftCircularCaustics)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (ObsidianBlack)
float3 uSecondaryColor;   // Secondary trail color (PureWhite)
float  uOpacity;          // Overall opacity
float  uTime;             // Scrolling time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Black Swan-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Width distortion amount
float  uPhase;            // Combo step (0.0 / 0.5 / 1.0)
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

// HSL to RGB for prismatic band
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

// Noise sample with fallback
float SampleNoise(float2 uv)
{
    if (uHasSecondaryTex > 0.5)
        return tex2D(uImage1, uv).r;
    // Procedural fallback
    float h = dot(uv, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

// Prismatic collision band — rainbow shimmer where black meets white
float3 PrismaticBand(float v, float u, float time, float comboPhase)
{
    // Distance from center line (v = 0.5)
    float distFromCenter = abs(v - 0.5);

    // Band width scales with combo: narrow on first swing, wide on finisher
    float bandWidth = 0.04 + comboPhase * 0.06; // 0.04 -> 0.10

    // Band intensity mask — sharp falloff from center
    float bandMask = 1.0 - smoothstep(0.0, bandWidth, distFromCenter);

    // Rainbow hue cycles along trail and time
    float hue = frac(u * 2.0 + time * 0.15 + v * 0.5);
    float3 rainbow = HueToRGB(hue);

    // Saturation scales with combo — first swing is subtle, finisher is vivid
    float saturation = 0.4 + comboPhase * 0.6; // 0.4 -> 1.0
    float3 desatRainbow = lerp(float3(0.85, 0.85, 0.9), rainbow, saturation);

    return desatRainbow * bandMask;
}

// =====================================================================
//  DualPolarityFlow — Sharp polarity core with prismatic collision
// =====================================================================

float4 DualPolarityFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Edge fade — sharp swing profile
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 2.5);

    // Head-to-tail fade — elegant decay
    float trailFade = pow(saturate(1.0 - coords.x), 1.8);

    // Noise-driven distortion
    float2 noiseUV = float2(coords.x * uSecondaryTexScale - uTime * uScrollSpeed,
                            coords.y * uSecondaryTexScale * 0.5);
    float noise = SampleNoise(noiseUV);
    float distortion = (noise - 0.5) * uDistortionAmt;

    // Scrolled UV for trail body texture
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + distortion);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === DUAL POLARITY GRADIENT ===
    // Top half (v < 0.5) = obsidian black, Bottom half (v > 0.5) = pure white
    // Smooth transition across the center
    float polarityT = smoothstep(0.3, 0.7, coords.y);
    float3 polarityColor = lerp(uColor, uSecondaryColor, polarityT);

    // === PRISMATIC COLLISION BAND ===
    float3 bandColor = PrismaticBand(coords.y, coords.x, uTime, uPhase);
    float bandStrength = 0.6 + uPhase * 0.4; // 0.6 -> 1.0

    // Blend polarity base with prismatic band
    float3 trailColor = polarityColor + bandColor * bandStrength;

    // === WHITE-HOT CORE AT COLLISION LINE ===
    float coreGlow = exp(-pow((coords.y - 0.5) / 0.03, 2.0));
    float coreStrength = 0.3 + uPhase * 0.5; // Combo-scaled
    trailColor += float3(0.95, 0.95, 1.0) * coreGlow * coreStrength * trailFade;

    // === WALTZ-TIME PULSE (3/4 rhythm) ===
    float waltzPulse = 0.92 + 0.08 * sin(uTime * 3.14159);
    trailColor *= waltzPulse;

    // === FEATHER DISSOLVE AT TAIL ===
    float dissolveThreshold = coords.x * 1.2 - 0.1;
    float dissolveNoise = SampleNoise(float2(coords.x * 3.0 + uTime * 0.2, coords.y * 2.0));
    float dissolveMask = smoothstep(dissolveThreshold - 0.1, dissolveThreshold + 0.1, dissolveNoise);
    float tailDissolve = lerp(1.0, dissolveMask, smoothstep(0.4, 0.8, coords.x));

    // === SECONDARY NOISE SHIMMER ===
    float2 shimmerUV = float2(coords.x * 5.0 - uTime * 1.5, coords.y * 3.0 + uTime * 0.3);
    float shimmer = SampleNoise(shimmerUV);
    shimmer = smoothstep(0.6, 0.85, shimmer) * 0.15 * trailFade;
    float3 shimmerColor = HueToRGB(frac(coords.x + uTime * 0.1));
    trailColor += shimmerColor * shimmer * uPhase; // Shimmer only on later combos

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * tailDissolve * uOpacity;
    float3 finalColor = trailColor * uIntensity;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  DualPolarityGlow — Soft bloom underlay with polarity tint
// =====================================================================

float4 DualPolarityGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6);

    // Gentler trail fade
    float trailFade = pow(saturate(1.0 - coords.x), 1.2);

    // Polarity gradient — softer blend for glow
    float polarityT = smoothstep(0.2, 0.8, coords.y);
    float3 glowColor = lerp(uColor, uSecondaryColor, polarityT);

    // Blend slightly toward center (prismatic hint in glow)
    float centerDist = abs(coords.y - 0.5);
    float centerGlow = 1.0 - smoothstep(0.0, 0.2, centerDist);
    float3 centerTint = lerp(float3(0.7, 0.7, 0.8), float3(0.9, 0.9, 1.0), uPhase);
    glowColor = lerp(glowColor, centerTint, centerGlow * 0.3);

    // Breathing pulse — waltz time
    float pulse = 0.85 + 0.15 * sin(uTime * 2.0 + coords.x * 3.0);

    // Low opacity glow
    float alpha = edgeFade * trailFade * uOpacity * 0.35 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique DualPolarityFlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 DualPolarityFlowPS();
    }
}

technique DualPolarityGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 DualPolarityGlowPS();
    }
}
