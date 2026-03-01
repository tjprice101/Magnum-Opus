// MysteryVoidAura.fx - Void-tinged mystery aura, deep purple-green
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

float4 PS_MysteryVoidAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Enigmatic swirling void  Erotating distortion
    float swirl = noise(float2(angle * 3.0 + uTime * 0.5, dist * 8.0 - uTime * 1.0));
    float deepVoid = noise(uv * 4.0 + float2(sin(uTime * 0.3) * 0.5, cos(uTime * 0.4) * 0.5));

    // Eerie watching-eye pattern  Econcentric focus
    float eyeFocus = 1.0 - smoothstep(0.0, 0.25, dist / uRadius);
    float eyePulse = 0.5 + 0.5 * sin(uTime * 1.5);

    // Mystery flickers  Erandom arcane sparks
    float arcane = noise(uv * 18.0 + float2(uTime * 2.0, -uTime * 1.5));
    arcane = pow(saturate(arcane), 4.0);

    float radialFade = saturate(1.0 - dist / uRadius);

    // Deep purple-green void palette
    float3 voidBlack = float3(0.02, 0.01, 0.04);
    float3 deepPurple = float3(0.35, 0.08, 0.5);
    float3 eerieGreen = float3(0.1, 0.55, 0.2);
    float3 arcaneFlash = float3(0.4, 0.8, 0.45);

    float3 auraColor = lerp(voidBlack, deepPurple, swirl * 0.8);
    auraColor = lerp(auraColor, eerieGreen, deepVoid * 0.35);
    auraColor += arcaneFlash * arcane * 0.25;

    // Void pull  Ecenter darkens ominously
    float voidPull = eyeFocus * eyePulse * 0.25;

    float auraStrength = radialFade * (0.4 + swirl * 0.6) * uIntensity;
    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.65);
    result -= voidPull;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_MysteryVoidAura();
    }
}
