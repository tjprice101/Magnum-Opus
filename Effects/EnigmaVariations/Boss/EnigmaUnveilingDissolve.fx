// EnigmaUnveilingDissolve.fx  EDeath dissolve: the mystery is finally unveiled
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

float4 PS_UnveilingDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01) return float4(0,0,0,0);
    float n = tex2D(uNoiseTex, uv * 2.5).r;
    // Spiral dissolve pattern (mystery unwinding)
    float2 centered = uv - 0.5;
    float angle = atan2(centered.y, centered.x);
    float spiralN = frac(angle / 6.283 + length(centered) * 3.0);
    float combined = n * 0.6 + spiralN * 0.4;
    float threshold = uDissolveProgress * 1.3;
    float clipVal = combined - threshold;
    if (clipVal < 0.0) return float4(0,0,0,0);
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);
    // Green-purple edge glow revealing what was hidden
    float4 greenGlow = float4(0.2, 0.9, 0.35, 1);
    float4 purpleGlow = uEdgeColor;
    float4 edgeCol = lerp(purpleGlow, greenGlow, edge);
    float4 result = lerp(sprite, edgeCol, edge);
    result.a = sprite.a;
    return result;
}

technique Technique1
{
    pass UnveilingDissolve { PixelShader = compile ps_3_0 PS_UnveilingDissolve(); }
}
