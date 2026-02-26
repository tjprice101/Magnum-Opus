// ============================================================================
//  SupernovaBlast.fx  ERadial crater explosion shader for Resurrection of the Moon
//
//  Visual: Expanding radial shockwave with crater ring geometry, radial lances
//  of white-hot energy, and a cooling central crater. Used for ricochet impacts,
//  on-hit detonations, and the grand finale supernova.
//
//  ps_2_0 compatible. Two techniques:
//    SupernovaBlastMain    EFull radial explosion with crater rings and radial lances
//    SupernovaBlastRing    ERing-only pass for shockwave overlay
// ============================================================================

sampler uImage0 : register(s0);    // Primary texture (soft glow / circular mask)
sampler uImage1 : register(s1);    // Secondary texture (noise pattern)

float4 uColor;                      // Primary color (CometCore white-gold)
float4 uSecondaryColor;             // Secondary color (ImpactCrater blue)
float uOpacity;                     // Overall opacity
float uTime;                        // Animation time
float uIntensity;                   // Brightness multiplier
float uOverbrightMult;              // HDR overbright
float uScrollSpeed;                 // Ring expansion rate
float uNoiseScale;                  // Noise detail
float uDistortionAmt;               // Radial distortion
bool uHasSecondaryTex;              // Secondary texture bound
float uSecondaryTexScale;           // Noise UV scale
float uSecondaryTexScroll;          // Noise scroll
float uPhase;                       // Explosion age (0 = just detonated, 1 = fully expanded/faded)

// ============================================================================
//  Helper: Polar coordinates
// ============================================================================
float2 ToPolar(float2 uv)
{
    float2 centered = uv - 0.5;
    float r = length(centered);
    float theta = atan2(centered.y, centered.x);
    return float2(r, theta);
}

// ============================================================================
//  Helper: Simple hash noise
// ============================================================================
float Hash(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

// ============================================================================
//  Radial lance pattern  Espokes of energy radiating outward
// ============================================================================
float RadialLances(float theta, float r, float lanceCount, float time)
{
    // Main lances
    float lanceAngle = theta * lanceCount / 6.28318;
    float lance = abs(frac(lanceAngle) - 0.5) * 2.0;
    lance = 1.0 - smoothstep(0.0, 0.15, lance);

    // Lances are brighter near center, fade outward
    float radialFade = 1.0 - smoothstep(0.05, 0.45, r);

    // Slight rotation over time
    float rotatedTheta = theta + time * 0.5;
    float secondaryLance = abs(frac(rotatedTheta * (lanceCount * 0.5) / 6.28318) - 0.5) * 2.0;
    secondaryLance = 1.0 - smoothstep(0.0, 0.2, secondaryLance);
    secondaryLance *= 0.3;

    return (lance + secondaryLance) * radialFade;
}

// ============================================================================
//  Crater ring  Eexpanding shockwave ring
// ============================================================================
float CraterRing(float r, float ringRadius, float ringWidth)
{
    float dist = abs(r - ringRadius);
    return 1.0 - smoothstep(0.0, ringWidth, dist);
}

// ============================================================================
//  Main supernova blast pixel shader
// ============================================================================
float4 PS_SupernovaBlastMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;

    float age = uPhase; // 0 = just detonated, 1 = fully faded

    // === EXPANDING CRATER RINGS ===
    // Primary shockwave ring  Eexpands outward over time
    float ringRadius1 = age * 0.4;
    float ring1 = CraterRing(r, ringRadius1, 0.03 + age * 0.02);

    // Secondary inner ring  Eslower expansion
    float ringRadius2 = age * 0.25;
    float ring2 = CraterRing(r, ringRadius2, 0.025 + age * 0.015);

    // Tertiary fine ring
    float ringRadius3 = age * 0.15;
    float ring3 = CraterRing(r, ringRadius3, 0.02);

    float rings = ring1 + ring2 * 0.6 + ring3 * 0.3;

    // === RADIAL ENERGY LANCES ===
    float lanceCount = 8.0 + uIntensity * 4.0;
    float lances = RadialLances(theta, r, lanceCount, uTime);

    // Lances fade as explosion ages
    lances *= (1.0 - age * 0.7);

    // === CENTRAL CRATER GLOW ===
    float craterGlow = exp(-r * r * 20.0);
    craterGlow *= (1.0 - age * 0.8); // Fades with age

    // === NOISE TURBULENCE ===
    float noise = 0.0;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale;
        noiseUV += float2(uTime * uSecondaryTexScroll, sin(theta * 3.0) * 0.05);
        noise = tex2D(uImage1, noiseUV).r;
    }
    else
    {
        noise = Hash(uv * uNoiseScale + uTime);
    }
    float turbulence = noise * 0.3 * (1.0 - age * 0.5);

    // === COLOR MAPPING ===
    // Central hot zone: white ↁEgold
    // Ring zones: gold ↁEimpact blue
    // Outer: impact blue ↁEdeep violet fade

    float3 hotColor = float3(0.95, 0.92, 1.0);   // Supernova white
    float3 goldColor = uColor.rgb;                  // CometCore
    float3 craterColor = uSecondaryColor.rgb;        // ImpactCrater
    float3 deepColor = float3(0.2, 0.08, 0.4);     // DeepSpaceViolet

    float3 coreCol = lerp(hotColor, goldColor, smoothstep(0.0, 0.1, r));
    float3 midCol = lerp(goldColor, craterColor, smoothstep(0.1, 0.3, r));
    float3 outerCol = lerp(craterColor, deepColor, smoothstep(0.3, 0.5, r));

    float3 baseColor = r < 0.1 ? coreCol : (r < 0.3 ? midCol : outerCol);

    // === COMBINE ALL ELEMENTS ===
    float combined = craterGlow * 2.0 + rings * 1.5 + lances * 0.8 + turbulence;

    // Age-based overall fade
    float ageFade = 1.0 - age * age;

    float3 finalColor = baseColor * combined * uIntensity * (1.0 + uOverbrightMult * craterGlow);
    float finalAlpha = baseTex.a * uOpacity * color.a * ageFade * saturate(combined);

    return float4(finalColor * finalAlpha, finalAlpha);
}

// ============================================================================
//  Ring-only pass  Eclean shockwave ring overlay
// ============================================================================
float4 PS_SupernovaBlastRing(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float age = uPhase;

    // Single clean expanding ring
    float ringRadius = age * 0.4;
    float ringWidth = 0.025 + age * 0.01;
    float ring = CraterRing(r, ringRadius, ringWidth);

    // Color shifts from hot to cool along ring
    float3 ringColor = lerp(uColor.rgb, uSecondaryColor.rgb, age * 0.7);

    // Ring brightness
    float ageFade = 1.0 - age * age;
    float3 finalColor = ringColor * ring * uIntensity * 0.8;
    float finalAlpha = baseTex.a * uOpacity * color.a * ring * ageFade * 0.7;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// ============================================================================
//  Techniques
// ============================================================================
technique SupernovaBlastMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_SupernovaBlastMain();
    }
}

technique SupernovaBlastRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_SupernovaBlastRing();
    }
}
