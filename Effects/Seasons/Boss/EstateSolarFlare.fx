// ══════════════════════════════════════════════════════════╁E
// EstateSolarFlare.fx  ESeasons/Estate attack flash
// Solar corona burst  Eradial sunburst with orange-yellow-
// white intensity radiating outward like a solar flare.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uIntensity;
float4 uColor;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_SolarFlare(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 delta = uv - uCenter;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Solar ray spikes  Esharp radial beams
    float rays = abs(sin(angle * 8.0 + uTime * 5.0));
    rays = pow(rays, 6.0);
    float rayMask = rays * smoothstep(0.5, 0.0, dist);

    // Corona ring  Ebright ring at blast edge
    float ringRadius = frac(uTime * 2.0) * 0.4;
    float corona = smoothstep(0.025, 0.0, abs(dist - ringRadius));
    corona *= smoothstep(0.5, 0.0, dist);

    // Radial intensity falloff
    float radialFade = exp(-dist * 5.0) * uIntensity;

    // Solar turbulence  Echaotic energy at the core
    float turb = noise(float2(angle * 5.0 + uTime * 6.0, dist * 12.0 - uTime * 3.0));
    float turbMask = smoothstep(0.3, 0.7, turb) * radialFade;

    // Colors: white-hot center, yellow mid, orange outer
    float4 whiteHot = float4(1.0, 0.98, 0.9, 1.0);
    float4 yellowMid = float4(1.0, 0.85, 0.2, 1.0);
    float4 orangeOuter = uColor;

    float heatGrad = saturate(dist * 3.0);
    float4 flareColor = lerp(whiteHot, yellowMid, heatGrad);
    flareColor = lerp(flareColor, orangeOuter, saturate(heatGrad * 1.5 - 0.3));

    float alpha = (rayMask * 0.6 + corona * 0.8 + turbMask * 0.4) * radialFade;

    float4 result = base;
    result.rgb += flareColor.rgb * alpha;
    result.rgb += whiteHot.rgb * corona * radialFade * 1.2;

    return result;
}

technique Technique1
{
    pass SolarFlare
    {
        PixelShader = compile ps_3_0 PS_SolarFlare();
    }
}
