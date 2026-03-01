// ══════════════════════════════════════════════════════════╁E
// InvernoAbsoluteZeroDissolve.fx  ESeasons/Inverno death dissolve
// Frozen shattering into ice crystals  Ecrystalline edge dissolve
// in ice blue/white as the boss fractures into frozen shards.
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

float4 PS_AbsoluteZeroDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Crystalline dissolve noise  Esharp, angular fracture pattern
    float n1 = noise(uv * 10.0);
    float n2 = noise(uv * 20.0 + float2(4.3, 2.1)) * 0.5;
    // Sharpen with abs to create angular fracture look
    float fracture = abs(n1 - 0.5) * 2.0;
    float dissolveNoise = (fracture + n2) / 1.5;

    // Dissolve threshold
    float threshold = uDissolveProgress;
    float clipMask = step(threshold, dissolveNoise);

    // Crystalline edge  Esharp ice-blue glow at fracture boundary
    float edgeDist = dissolveNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Ice shard geometry  Eangular bright spots at edge
    float shardSeed = hash(floor(uv * 35.0));
    float shardMask = step(0.85, shardSeed) * edgeGlow;

    // Frost crystallization spreading from dissolve edge
    float frostSpread = smoothstep(uEdgeWidth, uEdgeWidth * 2.5, edgeDist) * clipMask;
    float frostNoise = noise(uv * 25.0);
    float frost = smoothstep(0.5, 0.8, frostNoise) * (1.0 - frostSpread) * clipMask;

    // Colors: ice blue edge, white crystal highlights, pale frost
    float4 iceEdge = uEdgeColor;
    float4 crystalWhite = float4(0.92, 0.96, 1.0, 1.0);
    float4 paleFrost = float4(0.7, 0.8, 0.95, 1.0);

    float edgeBlend = smoothstep(0.0, uEdgeWidth * 0.5, edgeDist);
    float4 edgeColor = lerp(crystalWhite, iceEdge, edgeBlend);

    float4 result = base * clipMask;
    result.rgb += edgeColor.rgb * edgeGlow * 1.6;
    result.rgb += crystalWhite.rgb * shardMask * 2.0;
    result.rgb += paleFrost.rgb * frost * 0.2;
    result.a *= clipMask;

    return result;
}

technique Technique1
{
    pass AbsoluteZeroDissolve
    {
        PixelShader = compile ps_3_0 PS_AbsoluteZeroDissolve();
    }
}
