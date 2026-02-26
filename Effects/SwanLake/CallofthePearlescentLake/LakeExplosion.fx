// =====================================================================
//  LakeExplosion.fx — Call of the Pearlescent Lake water-ripple explosion
// =====================================================================
//
//  Visual: Concentric water ripple explosion. Multiple rings expanding
//  outward like stone dropped in a still lake. Inner ring white, outer
//  rings fade to dark silver with pearlescent shimmer between.
//
//  ps_3_0 compatible. Two techniques:
//    LakeExplosionMain — Full radial with concentric water ripples
//    LakeExplosionRing — Single expanding pearlescent ring overlay
// =====================================================================

sampler uImage0 : register(s0);    // Primary texture (soft glow / circular mask)
sampler uImage1 : register(s1);    // Noise texture (SoftCircularCaustics)

float4 uColor;                      // Primary color (PureWhite)
float4 uSecondaryColor;             // Secondary color (DarkSilver)
float  uOpacity;                    // Overall opacity
float  uTime;                       // Animation time
float  uIntensity;                  // Brightness multiplier
float  uOverbrightMult;             // HDR overbright
float  uScrollSpeed;                // Ripple expansion speed
float  uNoiseScale;                 // Noise detail
float  uDistortionAmt;              // Caustic distortion
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

// HSL to RGB for pearlescent shimmer
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

// Water ripple — sinusoidal ring at given radius
float WaterRipple(float r, float ringRadius, float wavelength, float amplitude)
{
    float dist = r - ringRadius;
    float ripple = sin(dist / wavelength * 6.28318) * amplitude;
    // Attenuate away from ring center
    float envelope = exp(-dist * dist / (wavelength * wavelength * 2.0));
    return ripple * envelope;
}

// Single ring mask
float RingMask(float r, float ringRadius, float ringWidth)
{
    float dist = abs(r - ringRadius);
    return 1.0 - smoothstep(0.0, ringWidth, dist);
}

// =====================================================================
//  Pearlescent shimmer gradient
// =====================================================================

float3 PearlescentGradient(float r, float theta, float time)
{
    // Low-saturation HSL cycling for mother-of-pearl
    float hue = frac(r * 2.0 + theta * 0.1 / 6.28318 + time * 0.08);
    float saturation = 0.25 + 0.15 * sin(r * 8.0 + time * 0.3);
    float luminance = 0.85 - r * 0.3;

    return HSLToRGB(hue, saturation, luminance);
}

// =====================================================================
//  Main explosion — concentric water ripples
// =====================================================================

float4 PS_LakeExplosionMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // === CONCENTRIC WATER RIPPLES ===
    // 4 rings expanding outward at different speeds
    float ripple1 = WaterRipple(r, age * 0.35, 0.03, 1.0);           // Primary ring
    float ripple2 = WaterRipple(r, age * 0.25, 0.025, 0.7);          // Secondary ring
    float ripple3 = WaterRipple(r, age * 0.15, 0.02, 0.4);           // Tertiary ring
    float ripple4 = WaterRipple(r, age * 0.08, 0.015, 0.2);          // Inner echo

    float totalRipple = ripple1 + ripple2 + ripple3 + ripple4;

    // Ring masks for distinct visible bands
    float ring1 = RingMask(r, age * 0.35, 0.025 + age * 0.01);
    float ring2 = RingMask(r, age * 0.25, 0.02 + age * 0.008);
    float ring3 = RingMask(r, age * 0.15, 0.018);
    float rings = ring1 * 1.0 + ring2 * 0.6 + ring3 * 0.3;

    // === CENTRAL GLOW ===
    float centerGlow = exp(-r * r * 30.0);
    centerGlow *= (1.0 - age * 0.8); // Fades with age

    // === CAUSTIC TEXTURE ===
    float caustic = 0.0;
    if (uHasSecondaryTex)
    {
        float2 causticUV = uv * uSecondaryTexScale;
        causticUV += float2(uTime * uSecondaryTexScroll * 0.2, sin(theta) * 0.05);
        caustic = tex2D(uImage1, causticUV).r;
        caustic = smoothstep(0.4, 0.7, caustic) * 0.2 * (1.0 - age * 0.5);
    }

    // === COLOR MAPPING ===
    // Pearlescent gradient across the explosion
    float3 pearlColor = PearlescentGradient(r, theta, uTime);

    // Inner = white, transition = pearlescent, outer = dark silver
    float3 innerColor = uColor.rgb; // PureWhite
    float3 outerColor = uSecondaryColor.rgb; // DarkSilver
    float radialBlend = smoothstep(0.0, 0.3, r);
    float3 baseColor = lerp(innerColor, pearlColor, radialBlend * 0.7);
    baseColor = lerp(baseColor, outerColor, smoothstep(0.25, 0.45, r));

    // Ring colors — inner rings brighter
    float3 ringColor1 = lerp(innerColor, pearlColor, 0.4);
    float3 ringColor2 = lerp(pearlColor, outerColor, 0.3);

    // === COMBINE ===
    float combined = centerGlow * 1.5 + caustic;
    float3 finalColor = baseColor * combined * uIntensity;

    // Add ring contributions
    finalColor += ringColor1 * ring1 * uIntensity * 1.2;
    finalColor += ringColor2 * ring2 * uIntensity * 0.6;
    finalColor += outerColor * ring3 * uIntensity * 0.3;

    // Ripple modulation (subtle brightness variation)
    finalColor *= (1.0 + totalRipple * 0.15);

    // Overbright on center
    finalColor *= (1.0 + uOverbrightMult * centerGlow * 0.4);

    // Age-based fade
    float ageFade = 1.0 - age * age;

    float finalAlpha = baseTex.a * uOpacity * color.a * ageFade *
                       saturate(combined + rings * 0.7 + caustic * 0.3);

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Ring-only pass — expanding pearlescent ring overlay
// =====================================================================

float4 PS_LakeExplosionRing(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // Single expanding pearlescent ring
    float ringRadius = age * 0.35;
    float ringWidth = 0.02 + age * 0.01;
    float ring = RingMask(r, ringRadius, ringWidth);

    // Pearlescent ring color
    float3 pearlRing = PearlescentGradient(r, theta, uTime);

    // At early age, ring is white; later becomes pearlescent
    float3 ringColor = lerp(uColor.rgb, pearlRing, smoothstep(0.0, 0.25, age));

    float ageFade = 1.0 - age * age;
    float3 finalColor = ringColor * ring * uIntensity * 0.8;
    float finalAlpha = baseTex.a * uOpacity * color.a * ring * ageFade * 0.7;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Techniques
// =====================================================================

technique LakeExplosionMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_LakeExplosionMain();
    }
}

technique LakeExplosionRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_LakeExplosionRing();
    }
}
