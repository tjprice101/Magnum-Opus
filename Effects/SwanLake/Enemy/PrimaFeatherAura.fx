// PrimaFeatherAura.fx - Prismatic feather shimmer aura, white with rainbow edges
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

float3 rainbow(float t) {
    float3 c;
    c.r = 0.5 + 0.5 * cos(6.2832 * (t + 0.0));
    c.g = 0.5 + 0.5 * cos(6.2832 * (t + 0.333));
    c.b = 0.5 + 0.5 * cos(6.2832 * (t + 0.667));
    return c;
}

float4 PS_PrimaFeatherAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Feather shimmer  Egentle directional waves
    float featherWave = sin(angle * 6.0 + dist * 10.0 - uTime * 2.0) * 0.5 + 0.5;
    float softGlow = noise(uv * 5.0 + float2(uTime * 0.4, uTime * 0.2));

    float radialFade = saturate(1.0 - dist / uRadius);
    float edgeZone = smoothstep(0.3, 0.7, dist / uRadius);

    // Pure white core with prismatic rainbow at edges
    float3 pureWhite = float3(0.95, 0.95, 1.0);
    float3 softBlack = float3(0.1, 0.1, 0.12);
    float3 prism = rainbow(angle / 6.2832 + uTime * 0.15);

    float3 auraColor = lerp(pureWhite, softBlack, edgeZone * 0.3);
    auraColor = lerp(auraColor, prism, edgeZone * featherWave * 0.5);

    float shimmerPulse = 0.7 + 0.3 * sin(uTime * 1.5 + softGlow * 4.0);
    float auraStrength = radialFade * shimmerPulse * uIntensity;

    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.55);
    result += pureWhite * featherWave * radialFade * 0.15;
    result += prism * edgeZone * radialFade * 0.08;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_PrimaFeatherAura();
    }
}
