// WaningDeerLunarAura.fx - Soft lunar mist aura with purple-blue pulsing glow
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

float4 PS_WaningDeerLunarAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Slow lunar pulse  Ewaxing and waning like the moon
    float pulse = 0.5 + 0.5 * sin(uTime * 1.2 + dist * 6.0);
    float mistLayer1 = noise(uv * 4.0 + float2(uTime * 0.3, uTime * 0.15));
    float mistLayer2 = noise(uv * 7.0 - float2(uTime * 0.2, uTime * 0.4));
    float mist = (mistLayer1 * 0.6 + mistLayer2 * 0.4);

    // Radial falloff  Esofter near edges
    float radialGlow = saturate(1.0 - dist / uRadius);
    radialGlow = radialGlow * radialGlow;

    // Crescent highlight sweeping around the deer
    float crescent = saturate(sin(angle * 2.0 + uTime * 0.8) * 0.5 + 0.3);

    // Purple-blue moonlight palette
    float3 deepPurple = float3(0.28, 0.12, 0.52);
    float3 softBlue = float3(0.35, 0.55, 0.85);
    float3 silverMist = float3(0.72, 0.68, 0.82);

    float3 auraColor = lerp(deepPurple, softBlue, mist);
    auraColor = lerp(auraColor, silverMist, crescent * pulse * 0.4);

    float auraStrength = radialGlow * (0.5 + mist * 0.5) * uIntensity;
    auraStrength *= (0.7 + pulse * 0.3);

    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.6);
    result += auraColor * auraStrength * 0.25;

    return float4(result, base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_WaningDeerLunarAura();
    }
}
