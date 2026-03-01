// CrawlerBellAura.fx - Fiery bell resonance aura with orange-black flicker
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

float4 PS_CrawlerBellAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);

    // Bell resonance  Econcentric rings that pulse outward
    float resonance = sin(dist * 18.0 - uTime * 5.0) * 0.5 + 0.5;
    resonance *= saturate(1.0 - dist / uRadius);

    // Fire flicker  Echaotic high-frequency noise
    float flicker = noise(uv * 10.0 + float2(uTime * 4.0, uTime * 2.5));
    float flicker2 = noise(uv * 15.0 - float2(uTime * 3.0, uTime * 5.0));
    float fireNoise = flicker * 0.6 + flicker2 * 0.4;

    // Black smoke wisps
    float smoke = noise(uv * 3.0 + float2(0.0, uTime * 0.8));
    float smokeDarken = smoothstep(0.4, 0.7, smoke) * 0.4;

    // Infernal orange-gold palette
    float3 deepBlack = float3(0.05, 0.02, 0.01);
    float3 burntOrange = float3(0.85, 0.35, 0.05);
    float3 brightGold = float3(1.0, 0.75, 0.2);

    float3 fireColor = lerp(deepBlack, burntOrange, fireNoise);
    fireColor = lerp(fireColor, brightGold, resonance * 0.6);

    float radialFade = saturate(1.0 - dist / uRadius);
    float auraStrength = radialFade * (0.4 + fireNoise * 0.6) * uIntensity;

    float3 result = lerp(base.rgb, fireColor, auraStrength * 0.7);
    result -= smokeDarken * radialFade;
    result += brightGold * resonance * radialFade * 0.2;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_CrawlerBellAura();
    }
}
