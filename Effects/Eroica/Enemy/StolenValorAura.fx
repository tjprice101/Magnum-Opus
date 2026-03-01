// StolenValorAura.fx - Corrupted golden aura, dimmer than true valor
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

float4 PS_StolenValorAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Corrupted pulse  Eunstable, flickering
    float corruptPulse = sin(uTime * 3.5 + dist * 8.0) * 0.5 + 0.5;
    float glitch = noise(uv * 20.0 + float2(uTime * 5.0, 0.0));
    glitch = step(0.85, glitch);

    // Tarnished gold energy  Edimmer, sickly
    float tarnish = noise(uv * 6.0 + float2(uTime * 0.4, uTime * 0.3));
    float corruption = noise(uv * 10.0 - float2(uTime * 0.6, uTime * 0.5));

    float radialFade = saturate(1.0 - dist / uRadius);

    // Corrupted valor palette  Esickly gold fading to murky dark
    float3 tarnishedGold = float3(0.6, 0.5, 0.1);
    float3 murkyBrown = float3(0.25, 0.15, 0.05);
    float3 corruptRed = float3(0.5, 0.08, 0.08);
    float3 dimGold = float3(0.7, 0.55, 0.15);

    float3 auraColor = lerp(murkyBrown, tarnishedGold, tarnish * 0.7);
    auraColor = lerp(auraColor, corruptRed, corruption * 0.3);

    // Occasional glitch flash  Estolen valor flickers like a broken illusion
    auraColor = lerp(auraColor, dimGold, glitch * 0.6);

    float unsteadyPulse = 0.4 + 0.6 * corruptPulse * (1.0 - glitch * 0.5);
    float auraStrength = radialFade * unsteadyPulse * uIntensity;

    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.55);
    result += tarnishedGold * radialFade * corruption * 0.06;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_StolenValorAura();
    }
}
