// L'Estate - Phase-Morphing Solar Aura
// uPhaseIntensity 0..1 drives the aura from gentle shimmer (Phase 1)
// to roaring inferno (Phase 3). Polar noise rays intensify with phase.

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float uPhaseIntensity; // 0 = calm shimmer, 1 = violent inferno
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

float hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + float2(1.0, 0.0));
    float c = hash12(i + float2(0.0, 1.0));
    float d = hash12(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float fbm2(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 2; i++)
    {
        v += a * noise2D(p);
        p = p * 2.1 + float2(97.0, 131.0);
        a *= 0.5;
    }
    return v;
}

float4 PS_SolarAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Breathing pulse scales with phase
    float pulseSpeed = 2.0 + uPhaseIntensity * 4.0;
    float pulseAmp = 0.05 + uPhaseIntensity * 0.12;
    float pulse = sin(uTime * pulseSpeed) * pulseAmp + 1.0;

    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Ray count and sharpness scale with phase
    float rayCount = 6.0 + uPhaseIntensity * 6.0;
    float raySharp = 3.0 + uPhaseIntensity * 5.0;
    float raySpeed = 1.0 + uPhaseIntensity * 2.5;
    float rays = abs(sin(angle * rayCount + uTime * raySpeed));
    rays = pow(rays, raySharp) * smoothstep(radiusNorm, 0.0, dist);

    // Turbulent noise layer adds chaotic flicker at high phases
    float turbulence = fbm2(float2(angle * 4.0, dist * 8.0 - uTime * 1.5));
    float turbMask = smoothstep(0.3, 0.7, turbulence) * uPhaseIntensity;

    // Corona ring tightens and brightens with phase
    float ringWidth = 0.06 - uPhaseIntensity * 0.03;
    float coronaRing = smoothstep(ringWidth, 0.0, abs(dist - radiusNorm * 0.7));
    coronaRing *= noise2D(float2(angle * 5.0, uTime * 2.0));

    // Color: gold core to orange mid to red outer, shifting hotter with phase
    float heatGrad = saturate(dist / max(radiusNorm, 0.001));
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, heatGrad);
    float4 whiteHot = float4(1.0, 0.98, 0.92, 1.0);

    // Phase shifts core toward white-hot
    baseColor = lerp(baseColor, whiteHot, uPhaseIntensity * 0.3 * (1.0 - heatGrad));

    float alpha = falloff * (0.25 + turbMask * 0.3) * uIntensity;
    float4 color = baseColor * alpha;
    color += uPrimaryColor * rays * uIntensity * (0.3 + uPhaseIntensity * 0.4);
    color += whiteHot * coronaRing * uIntensity * (0.8 + uPhaseIntensity * 0.5);

    return color;
}

technique Technique1
{
    pass SolarAura
    {
        PixelShader = compile ps_3_0 PS_SolarAura();
    }
}
