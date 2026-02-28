// ═══════════════════════════════════════════════════════════════════
//  CrescendoSlashArc.fx — Deity slash impact arc shader
//  Renders crescent-shaped slash arcs for the deity's melee attacks.
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

float4 SlashArcPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float along    = uv.x;
    float across   = abs(uv.y - 0.5);

    // Crescent edge
    float edge     = 1.0 - saturate(across * 3.333);

    // Hot core line
    float core     = 1.0 - saturate(across * 12.5);

    // Arc falloff 
    float arcFade  = sin(along * 3.14159);

    // Color gradient
    float3 slashCol = lerp(float3(uColor), float3(uSecondaryColor), along);
    float3 hotCol   = float3(1.0, 0.95, 0.9);

    float4 color    = input.Color;
    float combined  = edge * arcFade + core * 0.4;
    color.rgb       = lerp(slashCol, hotCol, core * 0.8);
    color.a         = combined * uOpacity;
    color.rgb      *= combined;

    return color;
}

technique SlashArc
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 SlashArcPS();
    }
}
