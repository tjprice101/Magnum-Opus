// ============================================================
//  CelestialOrbit.fx — Orrery Of Dreams (Magic Staff/Orb)
//  Clair de Lune — "Dream Planetarium"
//
//  Orbiting dream-spheres trace luminous elliptical paths —
//  like a clockwork orrery projecting constellations of light.
//  Two techniques: CelestialOrbitPath, CelestialOrbitCore
// ============================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 uColor;
float4 uSecondaryColor;
float  uOpacity;
float  uTime;
float  uIntensity;
float  uOverbrightMult;
float  uScrollSpeed;
float  uDistortionAmt;
bool   uHasSecondaryTex;
float  uSecondaryTexScale;
float2 uSecondaryTexScroll;

float4 CelestialOrbitPathPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Multiple concentric orbit rings
    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center) * 2.0;

    float ring1 = abs(dist - 0.35);
    float ring2 = abs(dist - 0.6);
    float ring3 = abs(dist - 0.85);

    float orbit1 = exp(-ring1 * ring1 * 200.0);
    float orbit2 = exp(-ring2 * ring2 * 300.0);
    float orbit3 = exp(-ring3 * ring3 * 400.0);

    // Orbiting body positions
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);
    float body1 = exp(-pow(angle - fmod(uTime * 1.5, 6.2832) + 3.14159, 2.0) * 3.0) * orbit1 * 5.0;
    float body2 = exp(-pow(angle - fmod(uTime * 1.0 + 2.0, 6.2832) + 3.14159, 2.0) * 3.0) * orbit2 * 5.0;
    float body3 = exp(-pow(angle - fmod(uTime * 0.7 + 4.0, 6.2832) + 3.14159, 2.0) * 3.0) * orbit3 * 5.0;

    float bodies = saturate(body1 + body2 + body3);
    float orbits = (orbit1 + orbit2 * 0.7 + orbit3 * 0.5) * 0.6;

    // Dream haze noise
    float haze = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(uTime * 0.05, -uTime * 0.03);
        haze = tex2D(uImage1, noiseUV).r;
    }

    // Color: dream haze (base) + starlight silver (orbits) + pearl white (bodies)
    float3 dreamColor = uColor.rgb;
    float3 starlightColor = uSecondaryColor.rgb;
    float3 pearlWhite = float3(0.86, 0.90, 0.96);

    float3 color = dreamColor * haze * 0.15;
    color += starlightColor * orbits * 0.4;
    color += pearlWhite * bodies * 0.8;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * bodies * 0.5);

    float alpha = base.a * uOpacity * saturate(orbits + bodies * 0.8 + haze * 0.1);

    return float4(finalColor, alpha);
}

float4 CelestialOrbitCorePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float dist = length(uv - 0.5) * 2.0;
    float core = exp(-dist * dist * 8.0);
    float pulse = 0.5 + 0.5 * sin(uTime * 3.0);

    float3 coreColor = lerp(uColor.rgb, float3(0.96, 0.97, 1.0), core * 0.5) * uIntensity * uOverbrightMult;
    float alpha = base.a * uOpacity * core * (0.4 + pulse * 0.2);

    return float4(coreColor, alpha);
}

technique CelestialOrbitPath
{
    pass P0 { PixelShader = compile ps_3_0 CelestialOrbitPathPS(); }
}

technique CelestialOrbitCore
{
    pass P0 { PixelShader = compile ps_3_0 CelestialOrbitCorePS(); }
}
