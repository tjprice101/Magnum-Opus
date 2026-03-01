// ══════════════════════════════════════════════════════════╁E
// NachtmusikStellarDissolve.fx  ENachtmusik boss death
// Boss dissolves into cascading stardust: pixels break into
// tiny star points that drift upward and twinkle out.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;       // Starlight silver
float uEdgeWidth;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_StellarDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);

    // Dissolve noise  Erising pattern (top dissolves first for upward drift)
    float n1 = tex2D(uNoiseTex, uv * 2.0).r;
    float n2 = tex2D(uNoiseTex, uv * 4.0 + 0.3).r;
    float dissolveNoise = n1 * 0.6 + n2 * 0.4;

    // Top-first dissolve bias
    float verticalBias = (1.0 - uv.y) * 0.4;
    float dissolveThreshold = uDissolveProgress * 1.3 - verticalBias;

    float clipVal = dissolveNoise - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);

    // Edge glow  Estellar energy at dissolve boundary
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    // Star sparkle at dissolve edge
    float sparkSeed = hash(floor(uv * 50.0));
    float sparkle = step(0.9, sparkSeed) * edge;
    float twinkle = sin(uDissolveProgress * 20.0 + sparkSeed * 30.0) * 0.5 + 0.5;

    // Edge colors: silver -> white-hot center -> indigo outer fade
    float4 silverEdge = uEdgeColor;
    float4 whiteHot = float4(1.0, 0.98, 1.0, 1.0);
    float4 indigoFade = float4(0.2, 0.15, 0.5, 1.0);

    float4 edgeCol;
    if (edge > 0.6)
        edgeCol = lerp(silverEdge, whiteHot, (edge - 0.6) / 0.4);
    else
        edgeCol = lerp(indigoFade, silverEdge, edge / 0.6);

    float4 result = sprite;
    result = lerp(result, edgeCol, edge * edge);
    result += whiteHot * sparkle * twinkle * 0.6;
    result.rgb = lerp(result.rgb, uEdgeColor.rgb, uDissolveProgress * 0.25);
    result.a = sprite.a;

    return result;
}

technique Technique1
{
    pass StellarDissolve
    {
        PixelShader = compile ps_3_0 PS_StellarDissolve();
    }
}
