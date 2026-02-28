// ResonanceBladeTrail.fx
// Spectral blade slash trail — ghostly arc with edge glow and shimmer.

sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float uOpacity;

struct VSOutput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 BladeTrailPS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(uImage0, uv);

    // Trail fade (head to tail)
    float trailFade = 1.0 - uv.x;
    trailFade = trailFade * trailFade;

    // Edge glow — bright at edges, softer in center
    float edgeDist = abs(uv.y - 0.5) * 2.0;
    float edgeGlow = pow(max(edgeDist, 0.0), 2.0) * 0.6;
    float centerFill = 1.0 - edgeDist;

    // Spectral shimmer
    float shimmer = sin(uTime * 4.0 + uv.x * 8.0) * 0.15 + 0.85;

    // Ghostly core + bright edges
    float3 col = uColor * (centerFill + edgeGlow) * shimmer;
    float alpha = texColor.a * trailFade * (centerFill * 0.7 + edgeGlow) * uOpacity;

    return float4(col, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_2_0 BladeTrailPS();
    }
}
