// ══════════════════════════════════════════════════════════╁E
// FateCosmicDeathRift.fx  EFate boss death dissolve
// Boss dissolves into a cosmic rift/black hole, pixels spiral
// inward with crimson/pink energy and celestial white sparks.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;       // Crimson/pink
float uEdgeWidth;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_CosmicDeathRift(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Spiral inward distortion  Epixels twist toward center as dissolve progresses
    float spiralStrength = uDissolveProgress * 3.0;
    float2 spiralUV = uv;
    spiralUV -= 0.5;
    float cosA = cos(spiralStrength * dist);
    float sinA = sin(spiralStrength * dist);
    spiralUV = float2(spiralUV.x * cosA - spiralUV.y * sinA,
                      spiralUV.x * sinA + spiralUV.y * cosA);
    spiralUV += 0.5;

    // Dissolve from outer edges inward (black hole pull)
    float n = tex2D(uNoiseTex, spiralUV * 2.0).r;
    float dissolveThreshold = uDissolveProgress * 1.4 - dist * 0.8;
    float clipVal = n - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);

    // Edge glow  Ecosmic rift energy
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    // Colors: crimson outer edge, pink mid, celestial white core
    float4 outerEdge = uEdgeColor;
    float4 midEdge = float4(0.9, 0.3, 0.5, 1.0);
    float4 innerEdge = float4(1.0, 0.97, 1.0, 1.0);

    float4 edgeCol;
    if (edge > 0.7)
        edgeCol = lerp(midEdge, innerEdge, (edge - 0.7) / 0.3);
    else
        edgeCol = lerp(outerEdge, midEdge, edge / 0.7);

    // Void darkening at center
    float voidPull = smoothstep(0.3, 0.0, dist) * uDissolveProgress;

    float4 result = lerp(sprite, edgeCol, edge * edge);
    result.rgb = lerp(result.rgb, float3(0, 0, 0), voidPull * 0.5);
    result.rgb = lerp(result.rgb, uEdgeColor.rgb, uDissolveProgress * 0.2);
    result.a = sprite.a;

    return result;
}

technique Technique1
{
    pass CosmicDeathRift
    {
        PixelShader = compile ps_3_0 PS_CosmicDeathRift();
    }
}
