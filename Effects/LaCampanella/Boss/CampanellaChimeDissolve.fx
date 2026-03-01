// CampanellaChimeDissolve.fx  EDeath dissolve for La Campanella boss
// The body shatters like cracking ceramic/bell metal, with orange fire in the cracks
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

float4 PS_ChimeDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01) return float4(0,0,0,0);
    float n = tex2D(uNoiseTex, uv * 2.0).r;
    float n2 = tex2D(uNoiseTex, uv * 4.0 + 0.3).r;
    float voronoi = abs(n - n2);
    float crackNoise = n * 0.5 + voronoi * 0.5;
    float threshold = uDissolveProgress * 1.2;
    float clipVal = crackNoise - threshold;
    if (clipVal < 0.0) return float4(0,0,0,0);
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth * 1.5, clipVal);
    float4 crackGlow = uEdgeColor;
    float4 whiteHot = float4(1, 0.9, 0.7, 1);
    float4 edgeCol = lerp(crackGlow, whiteHot, edge * edge);
    float4 result = lerp(sprite, edgeCol, edge);
    result.a = sprite.a;
    return result;
}

technique Technique1
{
    pass ChimeDissolve { PixelShader = compile ps_3_0 PS_ChimeDissolve(); }
}
