// ══════════════════════════════════════════════════════════════════╁E
//  SymphonyCrackle.fx  EWand-tip crackle aura
//  Flickering core + electrical arcs + outer crackle ring.
//  Profile: ps_3_0 / vs_2_0
// ══════════════════════════════════════════════════════════════════╁E

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float3 uColor;
float uIntensity; // fire-rate intensity scaling (0ↁE)

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

float4 CracklePS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float2 center = float2(0.5, 0.5);
    float2 delta  = uv - center;
    float dist    = length(delta) * 2.0;

    // Flickering core glow (cheap  Eno exp)
    float flicker = 0.5 + 0.5 * sin(uTime * 15.0 + dist * 10.0);
    float core    = saturate(1.0 - dist * 2.0) * flicker;

    // Breathing crackle ring (cheap  Eno atan2/pow)
    float ringCenter = 0.6 + sin(uTime * 6.0) * 0.1;
    float ring = saturate(1.0 - abs(dist - ringCenter) * 10.0) * 0.5;
    ring *= 0.5 + 0.5 * sin(uTime * 8.0);

    float totalIntensity = (core + ring) * uOpacity * uIntensity;

    float4 color = float4(uColor, 1.0);
    color.rgb   *= totalIntensity;
    color.a      = saturate(totalIntensity);

    return color;
}

technique CrackleAura
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_3_0 CracklePS();
    }
}
