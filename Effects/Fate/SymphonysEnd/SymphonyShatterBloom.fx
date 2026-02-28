// ═══════════════════════════════════════════════════════════════════
//  SymphonyShatterBloom.fx — Shatter impact bloom effect
//  Expanding ring + central flash + spiky rays.
//  Profile: ps_2_0 / vs_2_0
// ═══════════════════════════════════════════════════════════════════

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float3 uColor;
float uProgress; // 0→1 shatter animation lifetime

sampler uImage0 : register(s0);

struct VSInput
{
    float2 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 0.0, 1.0), uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color    = input.Color;
    return output;
}

float4 ShatterPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float2 center = float2(0.5, 0.5);
    float dist    = length(uv - center) * 2.0;

    // Expanding ring (cheap — no smoothstep)
    float ringRadius = uProgress * 1.5;
    float ringWidth  = 0.15 * (1.0 - uProgress) + 0.05;
    float ring       = saturate(1.0 - abs(dist - ringRadius) / ringWidth);

    // Central flash (cheap — no exp)
    float flash = saturate(1.0 - dist * 2.0) * (1.0 - uProgress);

    float intensity = (ring + flash) * uOpacity;

    float4 color = float4(uColor, 1.0);
    color.rgb   *= intensity;
    color.a      = saturate(intensity);

    return color;
}

technique ShatterBloom
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 ShatterPS();
    }
}
