// ══════════════════════════════════════════════════════════════════╁E
//  CrescendoSummonBloom.fx  ESummoning explosion bloom shader
//  Renders the divine eruption when the cosmic deity is summoned.
//  Profile: ps_3_0 / vs_2_0
// ══════════════════════════════════════════════════════════════════╁E

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float uIntensity;
float3 uColor;
float3 uSecondaryColor;

sampler uImage0 : register(s0);
sampler uNoiseTexture : register(s1);

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

float4 SummonBloomPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float2 d       = uv - 0.5;
    float dist     = saturate(length(d) * 2.0);

    // Explosion radial
    float expand   = 1.0 - dist;
    float flash    = expand * expand;

    // Shockwave ring
    float ringDist  = abs(dist - frac(uTime * 2.0) * 1.2);
    float ring      = saturate(1.0 - ringDist * 12.5) * step(dist, 1.0);

    // Combine
    float intensity = flash * 0.6 + ring * 0.3 * uIntensity;

    // Color gradient
    float3 bloomCol = lerp(float3(uColor), float3(uSecondaryColor), dist * 0.8);

    float4 color    = input.Color;
    color.rgb       = lerp(bloomCol, 1.0, flash * 0.5);
    color.a         = intensity * uOpacity;
    color.rgb      *= intensity;

    return color;
}

technique SummonBloom
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_3_0 SummonBloomPS();
    }
}
