// WaningDeerMoonbeamTrail.fx - Silver moonbeam trail on movement
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

float hash(float2 p) {
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_WaningDeerMoonbeamTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    // Trail flows along X axis, fades along Y
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Moonbeam shimmer  Egentle sine waves
    float shimmer = sin(trailProgress * 12.0 - uTime * 3.0) * 0.5 + 0.5;
    float softEdge = 1.0 - smoothstep(0.3, 0.8, trailWidth);

    // Subtle star-dust sparkle
    float sparkle = noise(uv * 20.0 + float2(uTime * 2.0, 0.0));
    sparkle = pow(sparkle, 4.0) * 2.0;

    // Silver light scattering
    float scatter = noise(uv * 5.0 + float2(uTime * 0.5, uTime * 0.3));

    // Silver-violet moonbeam palette
    float3 silver = float3(0.82, 0.80, 0.90);
    float3 paleLavender = float3(0.65, 0.55, 0.80);
    float3 brightCore = float3(0.92, 0.90, 1.0);

    float3 beamColor = lerp(paleLavender, silver, shimmer);
    beamColor = lerp(beamColor, brightCore, sparkle * softEdge);
    beamColor += scatter * 0.08;

    // Fade trail tail
    float trailFade = smoothstep(0.0, 0.2, trailProgress) * smoothstep(1.0, 0.7, trailProgress);

    float alpha = base.a * softEdge * trailFade * uIntensity;
    alpha *= (0.6 + shimmer * 0.4);

    float3 result = lerp(base.rgb, beamColor, alpha * 0.8);
    result += brightCore * sparkle * softEdge * trailFade * 0.15;

    return float4(result, base.a * softEdge * trailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_WaningDeerMoonbeamTrail();
    }
}
