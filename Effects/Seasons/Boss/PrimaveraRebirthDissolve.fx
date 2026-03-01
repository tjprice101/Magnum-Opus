// ══════════════════════════════════════════════════════════╁E
// PrimaveraRebirthDissolve.fx  ESeasons/Primavera death dissolve
// Blooming flowers replacing the body  Edissolve where edges
// sprout green-pink growth as the boss fades into spring.
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

float4 PS_RebirthDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Organic dissolve noise  Evine/growth pattern
    float n1 = noise(uv * 7.0 + float2(0.0, 2.3));
    float n2 = noise(uv * 14.0 + float2(5.1, 0.0)) * 0.5;
    float n3 = noise(uv * 28.0 + float2(1.7, 8.4)) * 0.25;
    float dissolveNoise = (n1 + n2 + n3) / 1.75;

    // Dissolve threshold
    float threshold = uDissolveProgress;
    float clipMask = step(threshold, dissolveNoise);

    // Blooming edge  Egrowth zone at dissolve boundary
    float edgeDist = dissolveNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Flower petal accents at the edge
    float petalSeed = hash(floor(uv * 40.0));
    float petalMask = step(0.88, petalSeed) * edgeGlow;

    // Colors: green growth edge, pink petal highlights, white bloom center
    float4 greenEdge = uEdgeColor;
    float4 pinkPetal = float4(1.0, 0.55, 0.7, 1.0);
    float4 whiteBloom = float4(1.0, 1.0, 0.9, 1.0);

    float edgeBlend = smoothstep(0.0, uEdgeWidth * 0.6, edgeDist);
    float4 edgeColor = lerp(greenEdge, pinkPetal, edgeBlend * 0.5);

    float4 result = base * clipMask;
    result.rgb += edgeColor.rgb * edgeGlow * 1.4;
    result.rgb += pinkPetal.rgb * petalMask * 1.5;
    result.rgb += whiteBloom.rgb * petalMask * step(0.96, petalSeed) * 2.0;
    result.a *= clipMask;

    return result;
}

technique Technique1
{
    pass RebirthDissolve
    {
        PixelShader = compile ps_3_0 PS_RebirthDissolve();
    }
}
