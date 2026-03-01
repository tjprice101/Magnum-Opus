// CenturionValorAura.fx - Scarlet/gold heroic aura with warmth
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

float4 PS_CenturionValorAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Heroic radiance  Esteady upward flow with warmth
    float radiance = noise(uv * 5.0 + float2(0.0, -uTime * 1.5));
    float warmPulse = 0.6 + 0.4 * sin(uTime * 2.0 + dist * 5.0);

    // Shield-like angular segments
    float segments = sin(angle * 4.0 + uTime * 0.6) * 0.5 + 0.5;
    segments = smoothstep(0.3, 0.7, segments);

    float radialFade = saturate(1.0 - dist / uRadius);
    radialFade = pow(radialFade, 1.5);

    // Scarlet-gold heroic palette
    float3 deepScarlet = float3(0.7, 0.1, 0.05);
    float3 brightGold = float3(1.0, 0.8, 0.25);
    float3 warmCrimson = float3(0.85, 0.2, 0.1);

    float3 auraColor = lerp(deepScarlet, warmCrimson, radiance);
    auraColor = lerp(auraColor, brightGold, segments * warmPulse * 0.5);

    // Rising ember glow at top
    float risingHeat = saturate(1.0 - (uv.y - uCenter.y + 0.2) * 3.0);
    auraColor += brightGold * risingHeat * 0.15;

    float auraStrength = radialFade * warmPulse * uIntensity;
    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.6);
    result += brightGold * segments * radialFade * 0.12;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_CenturionValorAura();
    }
}
