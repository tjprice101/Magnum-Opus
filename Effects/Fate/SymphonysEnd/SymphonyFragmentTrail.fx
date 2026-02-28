// ═══════════════════════════════════════════════════════════════════
//  SymphonyFragmentTrail.fx — Blade fragment scatter trail
//  Sharp, jagged trail that dissolves rapidly.
//  Profile: ps_2_0 / vs_2_0
// ═══════════════════════════════════════════════════════════════════

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float3 uColor;

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

float4 FragmentPS(VSOutput input) : COLOR0
{
    float2 uv   = input.TexCoord;
    float along  = uv.x;   // 0 = head, 1 = tail
    float across = abs(uv.y - 0.5);

    // Jagged edge distortion that increases toward the tail
    float jitter = sin(along * 25.132 + uTime * 8.0) * 0.15 * along;
    float edge   = 1.0 - smoothstep(0.0, 0.38 + jitter, across);

    // Quadratic decay — fragments burn out fast
    float fade = (1.0 - along) * (1.0 - along);

    // Hot core color push
    float4 color     = input.Color;
    float hotFactor  = saturate((1.0 - along) * (1.0 - across * 2.0));
    color.rgb        = lerp(color.rgb, float3(uColor), hotFactor * 0.5);

    color.a    = edge * fade * uOpacity;
    color.rgb *= edge * fade;

    return color;
}

technique FragmentTrail
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 FragmentPS();
    }
}
