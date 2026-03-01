// ══════════════════════════════════════════════════════════╁E
// DiesFinalJudgmentDissolve.fx  EDies Irae boss death
// Boss dissolves in flames of judgment: body burns away with
// blood red fire edges, crimson ash scattering outward.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;       // Blood red
float uEdgeWidth;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_FinalJudgmentDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);

    // Multi-octave dissolve noise  Echaotic fire burn
    float n1 = tex2D(uNoiseTex, uv * 2.0 + float2(0, uDissolveProgress * 0.5)).r;
    float n2 = tex2D(uNoiseTex, uv * 4.0 + float2(0.3, 0.7)).r;
    float dissolveNoise = n1 * 0.6 + n2 * 0.4;

    // All-directional burn (judgment is total, no bias)
    float dissolveThreshold = uDissolveProgress * 1.3;

    float clipVal = dissolveNoise - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);

    // Burning edge  Ethe fire consuming the form
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    // Ash scatter at dissolve edge
    float ashSeed = hash(floor(uv * 45.0));
    float ashScatter = step(0.9, ashSeed) * edge;

    // Edge colors: blood red outer -> orange fire -> white-hot core
    float4 bloodOuter = uEdgeColor;
    float4 orangeFire = float4(1.0, 0.45, 0.05, 1.0);
    float4 whiteCore = float4(1.0, 0.9, 0.75, 1.0);
    float4 ashGray = float4(0.3, 0.15, 0.1, 1.0);

    float4 edgeCol;
    if (edge > 0.7)
        edgeCol = lerp(orangeFire, whiteCore, (edge - 0.7) / 0.3);
    else if (edge > 0.3)
        edgeCol = lerp(bloodOuter, orangeFire, (edge - 0.3) / 0.4);
    else
        edgeCol = bloodOuter;

    float4 result = sprite;
    result = lerp(result, edgeCol, edge * edge);
    result += ashGray * ashScatter * 0.3;

    // Progressive crimson tint as body burns
    result.rgb = lerp(result.rgb, uEdgeColor.rgb * 0.5, uDissolveProgress * 0.4);
    result.a = sprite.a;

    return result;
}

technique Technique1
{
    pass FinalJudgmentDissolve
    {
        PixelShader = compile ps_3_0 PS_FinalJudgmentDissolve();
    }
}
