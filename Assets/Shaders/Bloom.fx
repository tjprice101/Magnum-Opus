// =============================================================================
// MagnumOpus Bloom Shader - SM 4.0 Level 9.1 Compatible
// =============================================================================
// Multi-layer additive bloom with pulsing and gradient support.
// =============================================================================

Texture2D SpriteTexture;
SamplerState SpriteTextureSampler
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

PixelShaderInput MainVS(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = input.Position;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// =============================================================================
// UNIFIED BLOOM - Standard glow with radial falloff
// =============================================================================
float4 UnifiedBloomPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    
    // Radial falloff from center
    float dist = length(input.TexCoord - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist);
    
    // Gentle pulse
    float pulse = sin(uTime * 2.0) * 0.1 + 1.0;
    
    // Gradient color
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 0.5);
    glowColor *= baseColor.rgb * uIntensity * glow * pulse;
    
    return float4(glowColor, glow * uOpacity * input.Color.a * baseColor.a);
}

// =============================================================================
// ETHEREAL BLOOM - Soft dreamy glow
// =============================================================================
float4 EtherealBloomPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    float dist = length(input.TexCoord - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist);
    glow = glow * glow; // Softer falloff
    float breathe = sin(uTime * 1.5) * 0.1 + 1.0;
    float3 color = uColor * baseColor.rgb * uIntensity * glow * breathe;
    return float4(color, glow * uOpacity * input.Color.a * baseColor.a);
}

// =============================================================================
// INFERNAL BLOOM - Harsh fire-like flicker
// =============================================================================
float4 InfernalBloomPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    float dist = length(input.TexCoord - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist * 1.5);
    float flicker = sin(uTime * 10.0) * 0.15 + 1.0;
    float3 color = uColor * baseColor.rgb * uIntensity * glow * flicker;
    return float4(color, glow * uOpacity * input.Color.a * baseColor.a);
}

// =============================================================================
// TECHNIQUES
// =============================================================================
technique DefaultTechnique
{
    pass DefaultPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 UnifiedBloomPS();
    }
}

technique EtherealTechnique
{
    pass EtherealPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 EtherealBloomPS();
    }
}

technique InfernalTechnique
{
    pass InfernalPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 InfernalBloomPS();
    }
}
