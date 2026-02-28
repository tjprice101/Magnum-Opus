// ═══════════════════════════════════════════════════════════════════
//  CrescendoAuraGlow.fx — Deity ambient aura with radial pulse
//  Renders the cosmic deity's ever-present divine light field.
//  Profile: ps_2_0 / vs_2_0
// ═══════════════════════════════════════════════════════════════════

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

float4 AuraPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float2 d       = uv - 0.5;
    float dist     = saturate(length(d) * 2.0);

    // Radial gradient — bright core, soft falloff
    float radial   = 1.0 - dist;
    radial        *= radial;

    // Pulsing ring
    float rings    = sin(dist * 12.566 - uTime * 6.0) * 0.5 + 0.5;
    rings         *= dist * (1.0 - dist) * 4.0 * 0.25 * uIntensity;

    float intensity = radial * 0.7 + rings;

    float3 auraCol = lerp(float3(uColor), float3(uSecondaryColor), dist);

    float4 color   = input.Color;
    color.rgb      = auraCol * intensity;
    color.a        = intensity * uOpacity;

    return color;
}

technique AuraGlow
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 AuraPS();
    }
}
