// L'Estate - Solar Collapse Dissolve
// Inward dissolution: edges burn away first, converging to white-hot point
// Burning edge glow with procedural ember scatter

sampler uImage0 : register(s0);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

float hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + float2(1.0, 0.0));
    float c = hash12(i + float2(0.0, 1.0));
    float d = hash12(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_SupernovaDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Inward dissolve: edges go first, center last (reversed from original)
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float radialBias = (1.0 - dist) * 0.6; // Center has higher threshold = dissolves last

    // Turbulent dissolve noise (3-octave)
    float n1 = noise2D(uv * 6.0 + float2(1.3, 4.7));
    float n2 = noise2D(uv * 12.0 + float2(8.2, 0.9)) * 0.5;
    float n3 = noise2D(uv * 24.0 + float2(3.6, 6.1)) * 0.25;
    float dissolveNoise = (n1 + n2 + n3) / 1.75;

    // Edges dissolve first (low radialBias), center last (high radialBias)
    float adjustedNoise = dissolveNoise * 0.6 + radialBias * 0.4;

    float threshold = uDissolveProgress;
    float clipMask = step(threshold, adjustedNoise);

    // Burning edge with temperature gradient
    float edgeDist = adjustedNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Colors: white-hot inner edge, yellow mid, ember orange outer
    float4 whiteHot = float4(1.0, 0.98, 0.92, 1.0);
    float4 yellowBurn = float4(1.0, 0.8, 0.2, 1.0);
    float4 emberEdge = uEdgeColor;

    float burnBlend = smoothstep(0.0, uEdgeWidth * 0.4, edgeDist);
    float4 edgeColor = lerp(whiteHot, yellowBurn, burnBlend);
    edgeColor = lerp(edgeColor, emberEdge, smoothstep(uEdgeWidth * 0.4, uEdgeWidth, edgeDist));

    // Convergence glow: as dissolve progresses, center glows brighter
    float convergence = uDissolveProgress * (1.0 - dist * 2.0);
    convergence = saturate(convergence) * uDissolveProgress;
    float4 convergeGlow = whiteHot * convergence * 0.5;

    // Ember spark scatter at dissolve edge
    float spark = hash12(floor(uv * 60.0 + dissolveNoise * 8.0));
    float sparkMask = step(0.92, spark) * edgeGlow;

    float4 result = base * clipMask;
    result.rgb += edgeColor.rgb * edgeGlow * 2.0;
    result.rgb += whiteHot.rgb * sparkMask * 2.5;
    result.rgb += convergeGlow.rgb;
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
