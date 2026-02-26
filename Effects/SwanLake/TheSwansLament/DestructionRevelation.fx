// =====================================================================
//  DestructionRevelation.fx — The Swan's Lament prismatic explosion
// =====================================================================
//
//  Visual: Radial explosion where monochrome shatters to reveal
//  prismatic color underneath. The sorrow of monochrome bullets
//  finally releases into a burst of all colors — grief transformed.
//
//  Inner core = monochrome gray (the sorrow)
//  Transition zone = cracks appear, saturation bleeds through
//  Outer rim = vivid rainbow shockwave (the revelation)
//
//  ps_3_0 compatible. Two techniques:
//    RevelationBlastMain — Full radial with crack-to-rainbow transition
//    RevelationBlastRing — Ring-only shockwave overlay
// =====================================================================

sampler uImage0 : register(s0);    // Primary texture (soft glow / circular mask)
sampler uImage1 : register(s1);    // Secondary texture (RealityCrackPattern)

float4 uColor;                      // Primary color (Silver monochrome)
float4 uSecondaryColor;             // Secondary color (PureWhite)
float  uOpacity;                    // Overall opacity
float  uTime;                       // Animation time
float  uIntensity;                  // Brightness multiplier
float  uOverbrightMult;             // HDR overbright
float  uScrollSpeed;                // Ring expansion rate
float  uNoiseScale;                 // Noise detail
float  uDistortionAmt;              // Crack distortion intensity
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

float Hash(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

// HSL to RGB conversion for rainbow generation
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

// Shockwave ring
float CraterRing(float r, float ringRadius, float ringWidth)
{
    float dist = abs(r - ringRadius);
    return 1.0 - smoothstep(0.0, ringWidth, dist);
}

// =====================================================================
//  Revelation Gradient — monochrome center -> rainbow rim
// =====================================================================

float3 RevelationGradient(float r, float theta, float age, float time)
{
    // Inner zone: pure monochrome gray
    float3 monoGray = float3(0.45, 0.45, 0.50);
    float3 darkGray = float3(0.23, 0.23, 0.27);

    // The revelation: rainbow hue mapped to radial distance
    // Each ring of the explosion is a different color
    float hue = frac(r * 3.0 + time * 0.1 + theta * 0.05);
    float sat = 0.85;
    float lum = 0.75;
    float3 rainbow = HSLToRGB(hue, sat, lum);

    // Transition: monochrome at center, rainbow at rim
    // The transition zone moves outward with age
    float transitionStart = age * 0.15;
    float transitionEnd = age * 0.35;
    float colorReveal = smoothstep(transitionStart, transitionEnd, r);

    // Saturation ramp: starts desaturated, becomes vivid
    float saturationRamp = smoothstep(0.0, 0.3, r) * (0.5 + age * 0.5);
    float3 desatRainbow = lerp(float3(lum, lum, lum), rainbow, saturationRamp);

    // Blend from monochrome core to revealed rainbow
    float3 innerColor = lerp(darkGray, monoGray, smoothstep(0.0, 0.08, r));
    return lerp(innerColor, desatRainbow, colorReveal);
}

// =====================================================================
//  Crack pattern — noise-driven shattering where color bleeds through
// =====================================================================

float CrackMask(float2 uv, float r, float theta, float age, float time)
{
    float cracks = 0.0;

    if (uHasSecondaryTex)
    {
        // RealityCrackPattern noise — angular distortion for radial cracks
        float2 crackUV = uv * uSecondaryTexScale;
        crackUV += float2(time * uSecondaryTexScroll * 0.3, sin(theta * 2.0) * 0.08);
        float crackNoise = tex2D(uImage1, crackUV).r;

        // Crack threshold decreases with age (more cracks as explosion expands)
        float threshold = 0.7 - age * 0.4;
        cracks = smoothstep(threshold - 0.05, threshold + 0.05, crackNoise);

        // Cracks are strongest in the transition zone
        float crackZone = smoothstep(0.05, 0.2, r) * (1.0 - smoothstep(0.3, 0.45, r));
        cracks *= crackZone;
    }
    else
    {
        // Procedural fallback: angular crack lines
        float crackAngle = frac(theta * 5.0 / 6.28318);
        cracks = 1.0 - smoothstep(0.0, 0.08, abs(crackAngle - 0.5) * 2.0);
        cracks *= smoothstep(0.05, 0.15, r) * (1.0 - smoothstep(0.25, 0.4, r));
        cracks *= step(0.5, Hash(float2(floor(theta * 5.0 / 6.28318), age * 3.0)));
    }

    return cracks * (0.3 + age * 0.7);
}

// =====================================================================
//  Main revelation blast pixel shader
// =====================================================================

float4 PS_RevelationBlastMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // === SHOCKWAVE RINGS ===
    // Primary revelation ring — expands outward, carries the rainbow
    float ringR1 = age * 0.4;
    float ring1 = CraterRing(r, ringR1, 0.025 + age * 0.015);

    // Secondary inner ring — the monochrome echo
    float ringR2 = age * 0.2;
    float ring2 = CraterRing(r, ringR2, 0.02 + age * 0.01);

    // Faint dark echo at half the primary radius
    float ringR3 = age * 0.2;
    float darkEcho = CraterRing(r, ringR3, 0.015);

    float rings = ring1 * 1.2 + ring2 * 0.5;

    // === CENTRAL SORROW GLOW ===
    // The monochrome core — grief holding on
    float sorrowGlow = exp(-r * r * 25.0);
    sorrowGlow *= (1.0 - age * 0.9); // Fades as revelation takes over

    // === CRACK PATTERN ===
    float cracks = CrackMask(uv, r, theta, age, uTime);

    // === NOISE TURBULENCE ===
    float turbulence = 0.0;
    if (uHasSecondaryTex)
    {
        float2 turbUV = uv * uSecondaryTexScale * 0.5;
        turbUV += float2(uTime * uSecondaryTexScroll, 0.0);
        turbulence = tex2D(uImage1, turbUV).r * 0.25 * (1.0 - age * 0.5);
    }
    else
    {
        turbulence = Hash(uv * uNoiseScale + uTime) * 0.2;
    }

    // === COLOR MAPPING ===
    float3 baseColor = RevelationGradient(r, theta, age, uTime);

    // Cracks reveal vivid rainbow beneath the monochrome shell
    float crackHue = frac(theta / 6.28318 + uTime * 0.15);
    float3 crackColor = HSLToRGB(crackHue, 1.0, 0.7);
    baseColor = lerp(baseColor, crackColor, cracks * 0.8);

    // Ring color: outer ring is vivid rainbow, inner is silver
    float ringHue = frac(theta / 6.28318 + r * 2.0 + uTime * 0.12);
    float3 ringColor = HSLToRGB(ringHue, 0.9, 0.8);
    float3 innerRingColor = uColor.rgb; // Silver
    float3 blendedRing = lerp(innerRingColor, ringColor, smoothstep(0.1, 0.3, r));

    // Dark echo ring is desaturated
    float3 echoColor = float3(0.3, 0.3, 0.35);

    // === COMBINE ===
    float combined = sorrowGlow * 1.5 + turbulence;
    float3 finalColor = baseColor * combined * uIntensity;

    // Add ring contribution
    finalColor += blendedRing * ring1 * uIntensity * 1.5;
    finalColor += innerRingColor * ring2 * uIntensity * 0.6;
    finalColor -= echoColor * darkEcho * 0.3; // Dark echo subtracts slightly

    // Overbright on sorrow core (monochrome punch)
    finalColor *= (1.0 + uOverbrightMult * sorrowGlow * 0.5);

    // Overbright on rainbow cracks (the revelation breaking through)
    finalColor += crackColor * cracks * uOverbrightMult * 0.4;

    // Age-based fade
    float ageFade = 1.0 - age * age;

    float finalAlpha = baseTex.a * uOpacity * color.a * ageFade *
                       saturate(combined + rings * 0.8 + cracks * 0.5);

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Ring-only pass — prismatic shockwave ring overlay
// =====================================================================

float4 PS_RevelationBlastRing(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;
    float age = uPhase;

    // Single expanding prismatic ring
    float ringRadius = age * 0.4;
    float ringWidth = 0.02 + age * 0.01;
    float ring = CraterRing(r, ringRadius, ringWidth);

    // Ring hue cycles around circumference — full rainbow
    float hue = frac(theta / 6.28318 + uTime * 0.1);
    float3 ringColor = HSLToRGB(hue, 0.9, 0.8);

    // At very early age, ring is still monochrome (silver)
    float colorReveal = smoothstep(0.0, 0.3, age);
    ringColor = lerp(uColor.rgb, ringColor, colorReveal);

    float ageFade = 1.0 - age * age;
    float3 finalColor = ringColor * ring * uIntensity * 0.9;
    float finalAlpha = baseTex.a * uOpacity * color.a * ring * ageFade * 0.8;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Techniques
// =====================================================================

technique RevelationBlastMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_RevelationBlastMain();
    }
}

technique RevelationBlastRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_RevelationBlastRing();
    }
}
