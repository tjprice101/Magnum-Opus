// BehemothWarAura.fx - Heavy warlike crimson aura with weight
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

float4 PS_BehemothWarAura(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float2 offset = uv - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);

    // Heavy, slow pulsing  Ethe weight of a behemoth
    float heavyPulse = 0.5 + 0.5 * sin(uTime * 0.8 + dist * 3.0);
    heavyPulse = pow(heavyPulse, 2.0);

    // Grinding war energy  Elow-frequency turbulence
    float warTurb = noise(uv * 3.0 + float2(uTime * 0.3, uTime * 0.2));
    float warTurb2 = noise(uv * 6.0 + float2(-uTime * 0.5, uTime * 0.4));
    float warEnergy = warTurb * 0.6 + warTurb2 * 0.4;

    // Cracked earth pattern  Eangular fractures
    float cracks = noise(uv * 15.0 + float2(uTime * 0.1, 0.0));
    cracks = smoothstep(0.45, 0.55, cracks) * 0.6;

    float radialFade = saturate(1.0 - dist / uRadius);
    radialFade = pow(radialFade, 1.2);

    // Deep oppressive crimson palette
    float3 bloodCrimson = float3(0.55, 0.04, 0.04);
    float3 ironDark = float3(0.15, 0.08, 0.06);
    float3 warGold = float3(0.8, 0.55, 0.1);

    float3 auraColor = lerp(ironDark, bloodCrimson, warEnergy);
    auraColor = lerp(auraColor, warGold, cracks * heavyPulse);

    // Gravitational darkening at center
    float gravityDarken = smoothstep(0.5, 0.0, dist / uRadius) * 0.2;

    float auraStrength = radialFade * (0.5 + heavyPulse * 0.5) * uIntensity;
    float3 result = lerp(base.rgb, auraColor, auraStrength * 0.65);
    result -= gravityDarken;
    result += warGold * cracks * radialFade * 0.08;

    return float4(saturate(result), base.a);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_BehemothWarAura();
    }
}
