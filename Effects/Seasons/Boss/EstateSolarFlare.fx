// L'Estate - Sunspot Eruption Solar Flare
// uProgress 0..1 drives building -> eruption -> dissipating lifecycle
// Radial eruption from center with expanding shockwave ring

sampler uImage0 : register(s0);
float2 uCenter;
float uIntensity;
float uProgress; // 0-0.3 building, 0.3-0.7 eruption, 0.7-1.0 dissipating
float4 uColor;
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

float4 PS_SolarFlare(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 delta = uv - uCenter;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Phase lifecycle
    float buildPhase = saturate(uProgress / 0.3);
    float eruptPhase = saturate((uProgress - 0.3) / 0.4);
    float fadePhase = saturate((uProgress - 0.7) / 0.3);

    // Building: pulsing core gathers energy
    float coreGather = exp(-dist * 8.0) * buildPhase * (1.0 - eruptPhase);
    float gatherPulse = sin(uTime * 8.0 + dist * 20.0) * 0.3 + 0.7;
    coreGather *= gatherPulse;

    // Eruption: expanding shockwave ring + radial rays
    float ringRadius = eruptPhase * 0.5;
    float ring = smoothstep(0.03, 0.0, abs(dist - ringRadius)) * eruptPhase;

    float rayIntensity = eruptPhase * (1.0 - fadePhase);
    float rays = abs(sin(angle * 10.0 + uTime * 6.0));
    rays = pow(rays, 4.0) * smoothstep(ringRadius + 0.1, 0.0, dist) * rayIntensity;

    // Turbulence at eruption core
    float turb = noise2D(float2(angle * 6.0 + uTime * 5.0, dist * 10.0 - uTime * 3.0));
    float turbMask = smoothstep(0.3, 0.7, turb) * eruptPhase * (1.0 - fadePhase);

    // Fade: everything dims
    float lifeMask = 1.0 - fadePhase;

    // Color ramp: white core -> yellow -> orange
    float4 whiteHot = float4(1.0, 0.98, 0.92, 1.0);
    float4 yellowMid = float4(1.0, 0.85, 0.25, 1.0);
    float heatGrad = saturate(dist * 3.0);
    float4 flareColor = lerp(whiteHot, yellowMid, heatGrad);
    flareColor = lerp(flareColor, uColor, saturate(heatGrad * 1.5 - 0.3));

    float alpha = (coreGather * 0.8 + ring * 1.0 + rays * 0.5 + turbMask * 0.3) * uIntensity * lifeMask;

    float4 result = base;
    result.rgb += flareColor.rgb * alpha;
    result.rgb += whiteHot.rgb * ring * uIntensity * lifeMask * 0.8;

    return result;
}

technique Technique1
{
    pass SolarFlare
    {
        PixelShader = compile ps_3_0 PS_SolarFlare();
    }
}
