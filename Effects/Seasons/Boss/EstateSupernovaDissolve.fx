// ══════════════════════════════════════════════════════════╁E
// EstateSupernovaDissolve.fx  ESeasons/Estate death dissolve
// Supernova-style burning out  Ewhite-hot center dissolving
// outward to ember edges as the boss collapses in solar fury.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_SupernovaDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Radial dissolve from center outward  Esupernova expansion
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float radialBias = dist * 0.6;

    // Turbulent dissolve noise
    float n1 = noise(uv * 6.0 + float2(1.3, 4.7));
    float n2 = noise(uv * 12.0 + float2(8.2, 0.9)) * 0.5;
    float n3 = noise(uv * 24.0 + float2(3.6, 6.1)) * 0.25;
    float dissolveNoise = (n1 + n2 + n3) / 1.75;

    // Center dissolves first, edges last
    float adjustedNoise = dissolveNoise * 0.7 + (1.0 - radialBias) * 0.3;

    float threshold = uDissolveProgress;
    float clipMask = step(threshold, adjustedNoise);

    // Burning edge  Ewhite-hot inner, orange-ember outer
    float edgeDist = adjustedNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Colors: white-hot core edge, yellow mid, ember orange outer
    float4 whiteHot = float4(1.0, 0.98, 0.9, 1.0);
    float4 yellowBurn = float4(1.0, 0.8, 0.2, 1.0);
    float4 emberEdge = uEdgeColor;

    float burnBlend = smoothstep(0.0, uEdgeWidth * 0.4, edgeDist);
    float4 edgeColor = lerp(whiteHot, yellowBurn, burnBlend);
    edgeColor = lerp(edgeColor, emberEdge, smoothstep(uEdgeWidth * 0.4, uEdgeWidth, edgeDist));

    // Ember spark scatter
    float spark = hash(floor(uv * 60.0 + dissolveNoise * 8.0));
    float sparkMask = step(0.9, spark) * edgeGlow;

    float4 result = base * clipMask;
    result.rgb += edgeColor.rgb * edgeGlow * 2.0;
    result.rgb += whiteHot.rgb * sparkMask * 2.5;
    result.a *= clipMask;

    return result;
}

technique Technique1
{
    pass SupernovaDissolve
    {
        PixelShader = compile ps_3_0 PS_SupernovaDissolve();
    }
}
