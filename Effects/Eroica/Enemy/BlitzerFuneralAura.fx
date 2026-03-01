// BlitzerFuneralAura.fx - Dark funeral glow, black-red somber aura
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

float4 PS_BlitzerFuneralAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Funeral dirge  Eslow, mournful pulsation
    float dirge = 0.5 + 0.5 * sin(uTime * 0.9 + angle * 2.0);
    dirge = pow(dirge, 3.0);

    // Black smoke tendrils rising
    float smoke1 = noise(uv * 4.0 + float2(uTime * 0.2, -uTime * 0.6));
    float smoke2 = noise(uv * 8.0 + float2(-uTime * 0.3, -uTime * 0.9));
    float smoke = smoke1 * 0.65 + smoke2 * 0.35;

    // Dim red heartbeat flicker
    float heartbeat = sin(uTime * 3.0) * 0.5 + 0.5;
    heartbeat = pow(heartbeat, 8.0);

    float radialFade = saturate(1.0 - dist / uRadius);

    // Somber funeral palette  Emostly dark
    float3 abyssBlack = float3(0.03, 0.01, 0.02);
    float3 funeralRed = float3(0.45, 0.03, 0.05);
    float3 dimCrimson = float3(0.6, 0.08, 0.08);

    float3 auraColor = lerp(abyssBlack, funeralRed, smoke * 0.7);
    auraColor = lerp(auraColor, dimCrimson, dirge * 0.4);
    auraColor += dimCrimson * heartbeat * 0.2;

    // Darkness swallowing the edges
    float voidPull = smoothstep(0.6, 0.0, dist / uRadius) * 0.15;

    float auraStrength = radialFade * (0.4 + smoke * 0.6) * uIntensity;
    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.7);
    result -= voidPull;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_BlitzerFuneralAura();
    }
}
