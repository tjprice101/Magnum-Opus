// HeraldCosmicAura.fx - Cosmic destiny aura, dark pink-crimson-white
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

float4 PS_HeraldCosmicAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Celestial rotation  Eslow cosmic orbit
    float cosmicSpin = noise(float2(angle * 2.0 + uTime * 0.3, dist * 6.0));
    float nebula = noise(uv * 3.0 + float2(sin(uTime * 0.2) * 0.8, cos(uTime * 0.15) * 0.8));

    // Star field twinkle
    float stars = noise(uv * 40.0 + float2(uTime * 0.5, uTime * 0.3));
    stars = pow(saturate(stars), 8.0) * 3.0;

    // Destiny pulse  Erings of fate
    float fateRing = sin(dist * 12.0 - uTime * 1.5) * 0.5 + 0.5;
    fateRing = pow(fateRing, 3.0);

    float radialFade = saturate(1.0 - dist / uRadius);
    radialFade = pow(radialFade, 1.3);

    // Fate cosmic palette  Eblack void to dark pink to crimson to celestial white
    float3 cosmicVoid = float3(0.02, 0.01, 0.03);
    float3 darkPink = float3(0.6, 0.12, 0.35);
    float3 brightCrimson = float3(0.85, 0.15, 0.2);
    float3 celestialWhite = float3(0.95, 0.9, 1.0);

    float3 auraColor = lerp(cosmicVoid, darkPink, nebula * 0.8);
    auraColor = lerp(auraColor, brightCrimson, cosmicSpin * 0.5);
    auraColor = lerp(auraColor, celestialWhite, fateRing * radialFade * 0.3);
    auraColor += celestialWhite * stars;

    // Cosmic inevitability  Ecentral gravity darkening
    float gravityWell = smoothstep(0.4, 0.0, dist / uRadius) * 0.12;

    float cosmicPulse = 0.6 + 0.4 * sin(uTime * 1.2 + nebula * 3.0);
    float auraStrength = radialFade * cosmicPulse * uIntensity;

    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.65);
    result -= gravityWell;
    result += darkPink * fateRing * radialFade * 0.1;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_HeraldCosmicAura();
    }
}
