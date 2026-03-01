// ResonanceEchoBloom.fx
// Echo / memory ripple bloom  Eexpanding ring of cosmic memory.

sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float uOpacity;
float uProgress; // 0 = just spawned, 1 = fully expanded

struct VSOutput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 EchoBloomPS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(uImage0, uv);

    // Ring effect at expanding radius
    float2 center = uv - float2(0.5, 0.5);
    float dist = length(center) * 2.0;

    float ringRadius = uProgress * 0.8;
    float ringWidth = 0.15;
    float ring = 1.0 - saturate(abs(dist - ringRadius) / ringWidth);
    ring = ring * ring;

    // Inner fill glow (fades as ring expands)
    float innerGlow = saturate(1.0 - dist / max(ringRadius, 0.01)) * (1.0 - uProgress);

    // Combine ring + inner fill
    float brightness = ring + innerGlow * 0.3;
    float3 col = uColor * brightness;
    float alpha = texColor.a * brightness * uOpacity * (1.0 - uProgress * 0.5);

    return float4(col, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 EchoBloomPS();
    }
}
