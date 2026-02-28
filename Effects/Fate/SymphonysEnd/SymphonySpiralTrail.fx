// ═══════════════════════════════════════════════════════════════════
//  SymphonySpiralTrail.fx — Symphony's End spiral blade trail
//  Two techniques: SpiralMain (core trail) and SpiralGlow (outer bloom)
//  Profile: ps_2_0 / vs_2_0
// ═══════════════════════════════════════════════════════════════════

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float3 uColor;
float3 uSecondaryColor;

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

// ─── Shared Vertex Shader ─────────────────────────────────────────

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 0.0, 1.0), uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color    = input.Color;
    return output;
}

// ─── SpiralMain: core trail with helix distortion ─────────────────

float4 SpiralMainPS(VSOutput input) : COLOR0
{
    float2 uv   = input.TexCoord;
    float along  = uv.x;   // 0 = head, 1 = tail
    float across = uv.y;   // 0..1 cross-section

    // Sinusoidal helix wobble that tightens toward the tail
    float spiral     = sin(along * 12.566 + uTime * 3.0) * 0.3;
    float centerDist = abs(across - 0.5 + spiral * (1.0 - along));

    // Soft edge falloff
    float edge = 1.0 - smoothstep(0.0, 0.48, centerDist);

    // Intensity ramps toward the head
    float intensity = (1.0 - along) * edge;

    // White-hot core near center
    float4 baseColor  = input.Color;
    float coreFactor  = smoothstep(0.3, 0.0, centerDist) * (1.0 - along);
    float4 coreColor  = float4(uColor, 1.0);
    float4 finalColor = lerp(baseColor, coreColor, coreFactor);

    finalColor.a    = intensity * uOpacity;
    finalColor.rgb *= intensity;

    return finalColor;
}

// ─── SpiralGlow: wider softer bloom overlay ───────────────────────

float4 SpiralGlowPS(VSOutput input) : COLOR0
{
    float2 uv   = input.TexCoord;
    float along  = uv.x;
    float across = abs(uv.y - 0.5);

    // Wide gaussian-ish glow
    float glow = exp(-across * across * 8.0) * (1.0 - along * 0.8);

    // Subtle pulse
    float pulse = 0.8 + 0.2 * sin(uTime * 5.0 + along * 6.283);

    float4 color = input.Color;
    color.rgb    = lerp(color.rgb, float3(uSecondaryColor), 0.3);
    color.a      = glow * pulse * uOpacity * 0.6;
    color.rgb   *= glow * pulse * 0.6;

    return color;
}

// ─── Techniques ───────────────────────────────────────────────────

technique SpiralMain
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 SpiralMainPS();
    }
}

technique SpiralGlow
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_2_0 SpiralGlowPS();
    }
}
