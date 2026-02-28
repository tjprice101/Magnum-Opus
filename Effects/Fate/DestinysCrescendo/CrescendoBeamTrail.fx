// ═══════════════════════════════════════════════════════════════════
//  CrescendoBeamTrail.fx — Cosmic deity beam trail shader
//  Two techniques: BeamMain (core beam) + BeamGlow (outer glow layer)
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

// ─── BeamMain: Core cosmic beam with pulsing energy ───────────────

float4 BeamMainPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float along    = uv.x;    // 0 = head, 1 = tail
    float across   = abs(uv.y - 0.5);

    // Scrolling energy distortion
    float scroll   = sin(along * 18.85 + uTime * 12.0) * 0.08 * (1.0 - along);
    float edge     = 1.0 - smoothstep(0.0, 0.35 + scroll, across);

    // Core hotspot — bright center line
    float core     = 1.0 - smoothstep(0.0, 0.12, across);

    // Fade along length — keeps head bright, tail vanishes
    float fade     = pow(1.0 - along, 1.6);

    // Pulse
    float pulse    = 1.0 + sin(uTime * 8.0 + along * 6.28) * 0.15 * uIntensity;

    // Color: gradient from primary to secondary along beam
    float3 beamCol = lerp(float3(uColor), float3(uSecondaryColor), along);
    float3 hotCol  = float3(1.0, 1.0, 1.0);

    float4 color   = input.Color;
    color.rgb      = lerp(beamCol, hotCol, core * 0.7);
    color.a        = edge * fade * uOpacity * pulse;
    color.rgb     *= edge * fade * pulse;

    return color;
}

// ─── BeamGlow: Outer bloom layer (wider, softer) ──────────────────

float4 BeamGlowPS(VSOutput input) : COLOR0
{
    float2 uv     = input.TexCoord;
    float along    = uv.x;
    float across   = abs(uv.y - 0.5);

    // Wider, softer edge
    float edge     = 1.0 - smoothstep(0.0, 0.5, across);
    float fade     = pow(1.0 - along, 1.2);

    // Gentle pulse
    float pulse    = 1.0 + sin(uTime * 5.0) * 0.1;

    float4 color   = input.Color;
    color.a        = edge * fade * uOpacity * 0.4 * pulse;
    color.rgb     *= edge * fade * 0.4 * pulse;

    return color;
}

// ─── Techniques ───────────────────────────────────────────────────

technique BeamMain
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 BeamMainPS();
    }
}

technique BeamGlow
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 BeamGlowPS();
    }
}
