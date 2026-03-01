// CenturionChargeTrail.fx - Blazing charge trail with ember particles
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

float4 PS_CenturionChargeTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Aggressive forward-rushing energy
    float charge = noise(uv * 8.0 + float2(-uTime * 6.0, uTime * 1.0));
    float chargeWave = sin(trailProgress * 14.0 - uTime * 8.0) * 0.5 + 0.5;

    // Ember sparks  Escattered bright points
    float sparks = noise(uv * 30.0 + float2(-uTime * 10.0, uTime * 3.0));
    sparks = pow(saturate(sparks), 6.0) * 4.0;

    // Heat distortion at core
    float heatCore = 1.0 - smoothstep(0.0, 0.3, trailWidth);
    float heatFlicker = noise(uv * 12.0 + float2(-uTime * 5.0, 0.0));

    // Scarlet-gold battle palette
    float3 darkCrimson = float3(0.5, 0.05, 0.02);
    float3 blazeScarlet = float3(0.95, 0.25, 0.05);
    float3 hotGold = float3(1.0, 0.9, 0.4);

    float3 trailColor = lerp(darkCrimson, blazeScarlet, charge);
    trailColor = lerp(trailColor, hotGold, chargeWave * heatCore * 0.7);
    trailColor += hotGold * sparks * 0.3;

    float edgeFade = 1.0 - smoothstep(0.3, 0.85, trailWidth);
    float tailFade = smoothstep(0.0, 0.08, trailProgress) * smoothstep(1.0, 0.5, trailProgress);

    float alpha = edgeFade * tailFade * uIntensity;
    alpha *= (0.6 + charge * 0.4);

    float3 result = lerp(base.rgb, trailColor, alpha * 0.9);
    result += hotGold * heatCore * heatFlicker * tailFade * 0.2;

    return float4(saturate(result), base.a * edgeFade * tailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_CenturionChargeTrail();
    }
}
