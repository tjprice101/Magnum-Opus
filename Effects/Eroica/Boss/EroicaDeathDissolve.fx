// ══════════════════════════════════════════════════════════╁E
// EroicaDeathDissolve.fx  EEroica boss death dissolve shader
// A heroic dissolve where the body breaks apart into golden
// embers and sakura petals, with a final burst of white light.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);      // Boss sprite
sampler uNoiseTex : register(s1);     // Dissolve noise
float uDissolveProgress;              // 0 = solid, 1 = gone
float4 uEdgeColor;                    // Gold edge glow
float uEdgeWidth;                     // Width of dissolve edge

float4 PS_DeathDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);
    
    // Multi-octave noise for organic dissolve
    float n1 = tex2D(uNoiseTex, uv * 1.5).r;
    float n2 = tex2D(uNoiseTex, uv * 3.0 + 0.5).r;
    float dissolveNoise = n1 * 0.7 + n2 * 0.3;
    
    // Dissolve from edges inward (distance from center drives priority)
    float2 centered = uv - 0.5;
    float distFromCenter = length(centered) * 1.4;
    float dissolveThreshold = uDissolveProgress * 1.3 - (1.0 - distFromCenter) * 0.3;
    
    // Clip dissolved pixels
    float clipVal = dissolveNoise - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);
    
    // Edge glow (the burning edge effect)
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);
    
    // Edge color transitions: gold ↁEwhite-hot ↁEscarlet
    float4 innerEdge = float4(1.0, 0.95, 0.85, 1.0); // White-hot
    float4 outerEdge = uEdgeColor;                      // Gold
    float4 farEdge = float4(0.8, 0.2, 0.1, 1.0);      // Scarlet ember
    
    float4 edgeCol;
    if (edge > 0.6)
        edgeCol = lerp(outerEdge, innerEdge, (edge - 0.6) / 0.4);
    else
        edgeCol = lerp(farEdge, outerEdge, edge / 0.6);
    
    // Final composite
    float4 result = sprite;
    result = lerp(result, edgeCol, edge * edge);
    result.a = sprite.a;
    
    // Slight golden tint as dissolution progresses
    result.rgb = lerp(result.rgb, uEdgeColor.rgb, uDissolveProgress * 0.3);
    
    return result;
}

technique Technique1
{
    pass DeathDissolve
    {
        PixelShader = compile ps_3_0 PS_DeathDissolve();
    }
}
