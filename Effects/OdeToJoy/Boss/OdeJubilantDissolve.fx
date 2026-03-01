// ══════════════════════════════════════════════════════════╁E
// OdeJubilantDissolve.fx  EOde to Joy boss death dissolve
// Boss dissolves into golden petals and warm light rays,
// pixels break away upward like floating flower petals.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;       // Warm gold
float uEdgeWidth;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_JubilantDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);

    // Dissolve noise  Eupward drift pattern
    float n1 = tex2D(uNoiseTex, uv * 1.5 + float2(0, uDissolveProgress * 0.3)).r;
    float n2 = tex2D(uNoiseTex, uv * 3.0 + 0.5).r;
    float dissolveNoise = n1 * 0.7 + n2 * 0.3;

    // Bottom-up dissolve bias (feet dissolve first, rising like joy ascending)
    float upwardBias = uv.y * 0.4;
    float dissolveThreshold = uDissolveProgress * 1.3 - upwardBias;

    float clipVal = dissolveNoise - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);

    // Edge glow  Egolden petal edges
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    // Petal sparkle at dissolve edge
    float petalSeed = hash(floor(uv * 40.0));
    float petalSparkle = step(0.88, petalSeed) * edge;

    // Edge colors: gold -> bright amber -> warm white
    float4 goldEdge = uEdgeColor;
    float4 amberGlow = float4(0.95, 0.7, 0.2, 1.0);
    float4 warmWhite = float4(1.0, 0.95, 0.8, 1.0);

    float4 edgeCol;
    if (edge > 0.6)
        edgeCol = lerp(amberGlow, warmWhite, (edge - 0.6) / 0.4);
    else
        edgeCol = lerp(goldEdge, amberGlow, edge / 0.6);

    float4 result = sprite;
    result = lerp(result, edgeCol, edge * edge);
    result += warmWhite * petalSparkle * 0.5;
    result.rgb = lerp(result.rgb, uEdgeColor.rgb, uDissolveProgress * 0.3);
    result.a = sprite.a;

    return result;
}

technique Technique1
{
    pass JubilantDissolve
    {
        PixelShader = compile ps_3_0 PS_JubilantDissolve();
    }
}
