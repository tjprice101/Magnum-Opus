// =====================================================================
//  AriaExplosion.fx — Chromatic Swan Song 3-hit combo detonation
// =====================================================================
//
//  Visual: The prismatic aria's culmination. Radial distance maps to
//  hue in a full ROYGBIV spectrum — white-hot center, distinct spectral
//  color bands radiating outward (like light through a prism), fading
//  to obsidian black at the very outer edge (framing rainbow in darkness).
//
//  Multiple spectral rings separate as explosion expands, each ring a
//  different dominant hue. Noise swirls in transition zones between bands.
//
//  ps_3_0 compatible. Two techniques:
//    AriaExplosionMain — Full spectral radial with prismatic bands
//    AriaExplosionRing — Expanding rainbow ring shockwave
// =====================================================================

sampler uImage0 : register(s0);    // Primary texture (soft glow / circular mask)
sampler uImage1 : register(s1);    // Noise texture (SparklyNoiseTexture)

float4 uColor;                      // Primary color (PureWhite)
float4 uSecondaryColor;             // Secondary color (ObsidianBlack — outer frame)
float  uOpacity;                    // Overall opacity
float  uTime;                       // Animation time
float  uIntensity;                  // Brightness multiplier
float  uOverbrightMult;             // HDR overbright
float  uScrollSpeed;                // Spectral band scroll
float  uNoiseScale;                 // Noise detail
float  uDistortionAmt;              // Band distortion
bool   uHasSecondaryTex;            // Secondary texture bound
float  uSecondaryTexScale;          // Noise UV scale
float  uSecondaryTexScroll;         // Noise scroll
float  uPhase;                      // Explosion age (0 = detonated, 1 = faded)

// =====================================================================
//  Helpers
// =====================================================================

float2 ToPolar(float2 uv)
{
    float2 centered = uv - 0.5;
    float r = length(centered);
    float theta = atan2(centered.y, centered.x);
    return float2(r, theta);
}

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

// Spectral ring mask
float SpectralRingMask(float r, float ringRadius, float ringWidth)
{
    float dist = abs(r - ringRadius);
    return 1.0 - smoothstep(0.0, ringWidth, dist);
}

// =====================================================================
//  Spectral gradient — maps radial distance to full ROYGBIV
// =====================================================================

float3 SpectralGradient(float r, float theta, float time, float age)
{
    // Map radius to hue: center = no hue (white), outer = full spectrum
    // The spectrum bands sharpen as explosion matures (age increases)
    float hueRange = r * 3.0; // Multiple cycles across radius
    float hue = frac(hueRange + theta * 0.05 / 6.28318 + time * 0.05);

    // Saturation: center = 0 (white), increasing outward
    float saturation = smoothstep(0.0, 0.15, r) * (0.8 + age * 0.2);

    // Luminance: bright center, moderate bands, dark outer edge
    float luminance = 0.9 - r * 1.2;
    luminance = max(luminance, 0.15);

    return HSLToRGB(hue, saturation, luminance);
}

// =====================================================================
//  Main explosion — spectral radial with prismatic bands
// =====================================================================

float4 PS_AriaExplosionMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // === SPECTRAL COLOR MAPPING ===
    float3 spectralColor = SpectralGradient(r, theta, uTime, age);

    // Inner white-hot core — always pure white
    float centerGlow = exp(-r * r * 40.0);
    centerGlow *= (1.0 - age * 0.7);

    // === SPECTRAL RINGS ===
    // Multiple rings at different radii, each a different dominant hue
    float ring1 = SpectralRingMask(r, age * 0.30, 0.025 + age * 0.012);
    float ring2 = SpectralRingMask(r, age * 0.22, 0.020 + age * 0.008);
    float ring3 = SpectralRingMask(r, age * 0.14, 0.018);
    float ring4 = SpectralRingMask(r, age * 0.08, 0.015);

    // Per-ring hues
    float3 ringColor1 = HueToRGB(frac(0.0 + uTime * 0.08));  // Red-ish
    float3 ringColor2 = HueToRGB(frac(0.2 + uTime * 0.08));  // Yellow-green
    float3 ringColor3 = HueToRGB(frac(0.5 + uTime * 0.08));  // Cyan-blue
    float3 ringColor4 = HueToRGB(frac(0.75 + uTime * 0.08)); // Violet

    float rings = ring1 + ring2 * 0.7 + ring3 * 0.4 + ring4 * 0.2;

    // === NOISE SWIRL IN TRANSITION ZONES ===
    float noiseSwirl = 0.0;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale;
        noiseUV += float2(sin(theta) * 0.08, cos(theta) * 0.08);
        noiseUV += float2(uTime * uSecondaryTexScroll * 0.15, uTime * 0.1);
        noiseSwirl = tex2D(uImage1, noiseUV).r;
        noiseSwirl = smoothstep(0.35, 0.7, noiseSwirl) * 0.25 * (1.0 - age * 0.4);
    }

    // === OBSIDIAN BLACK OUTER FRAME ===
    // Rainbow framed in darkness at the outer edge
    float outerDarkness = smoothstep(0.3, 0.45, r);
    float3 outerFrame = uSecondaryColor.rgb; // ObsidianBlack

    // === COLOR COMPOSITE ===
    float3 baseColor = spectralColor;

    // Add white-hot center
    baseColor = lerp(baseColor, uColor.rgb, centerGlow * 1.2);

    // Add ring contributions with their per-ring colors
    baseColor += ringColor1 * ring1 * uIntensity * 1.0;
    baseColor += ringColor2 * ring2 * uIntensity * 0.7;
    baseColor += ringColor3 * ring3 * uIntensity * 0.4;
    baseColor += ringColor4 * ring4 * uIntensity * 0.2;

    // Add noise swirl
    float3 swirlColor = HueToRGB(frac(r * 2.0 + uTime * 0.1));
    baseColor += swirlColor * noiseSwirl;

    // Fade toward obsidian at outer edge
    baseColor = lerp(baseColor, outerFrame, outerDarkness * 0.7);

    // Overbright on center
    baseColor *= (1.0 + uOverbrightMult * centerGlow * 0.5);

    float3 finalColor = baseColor * uIntensity;

    // Age-based fade
    float ageFade = 1.0 - age * age;

    float finalAlpha = baseTex.a * uOpacity * color.a * ageFade *
                       saturate(centerGlow + rings * 0.8 + noiseSwirl * 0.3 + 0.1);

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Ring-only pass — expanding rainbow ring shockwave
// =====================================================================

float4 PS_AriaExplosionRing(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // Single expanding spectral ring
    float ringRadius = age * 0.35;
    float ringWidth = 0.025 + age * 0.015;
    float ring = SpectralRingMask(r, ringRadius, ringWidth);

    // Ring cycles through full spectrum along its circumference
    float ringHue = frac(theta / 6.28318 + age * 0.5 + uTime * 0.1);
    float3 ringColor = HueToRGB(ringHue);

    // High saturation — this is the most vivid part
    float3 vividRing = HSLToRGB(ringHue, 0.95, 0.65);

    // At early age, ring is white; later becomes full rainbow
    float3 finalRingColor = lerp(uColor.rgb, vividRing, smoothstep(0.0, 0.2, age));

    float ageFade = 1.0 - age * age;
    float3 finalColor = finalRingColor * ring * uIntensity * 1.0;
    float finalAlpha = baseTex.a * uOpacity * color.a * ring * ageFade * 0.8;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Techniques
// =====================================================================

technique AriaExplosionMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_AriaExplosionMain();
    }
}

technique AriaExplosionRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_AriaExplosionRing();
    }
}
