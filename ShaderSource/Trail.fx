// =============================================================================
// MagnumOpus Trail Shader - SM 4.0 Level 9.1 Compatible
// =============================================================================
// Calamity-style primitive trail rendering with smooth gradients and bloom.
// Compile with: mgcb or 2mgfx for MonoGame/FNA compatibility.
// =============================================================================

// Textures and Samplers
Texture2D SpriteTexture;
SamplerState SpriteTextureSampler
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Parameters
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// Vertex Shader Input
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Pixel Shader Input
struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Utility functions
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

// Pass-through Vertex Shader
PixelShaderInput MainVS(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = input.Position;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// =============================================================================
// MAIN TRAIL SHADER
// =============================================================================
float4 TrailShaderPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    
    // Vertical interpolant for trail width (0 at edges, 1 at center)
    float verticalInterpolant = QuadraticBump(input.TexCoord.y);
    
    // Horizontal interpolant for trail length (0 at start, 1 at end)
    float horizontalProgress = input.TexCoord.x;
    
    // Calculate edge fadeout
    float edgeFade = InverseLerp(0.0, 0.15, verticalInterpolant);
    edgeFade *= InverseLerp(0.95, 0.5, horizontalProgress);
    
    // Blend between primary and secondary color
    float3 gradientColor = lerp(uColor, uSecondaryColor, horizontalProgress);
    
    // Apply time-based pulse
    float pulse = 1.0 + sin(uTime * 3.0) * 0.1;
    
    // Final color calculation
    float3 finalColor = gradientColor * uIntensity * pulse;
    float finalAlpha = edgeFade * uOpacity * input.Color.a * baseColor.a;
    
    return float4(finalColor * baseColor.rgb, finalAlpha);
}

// =============================================================================
// BLOOM TRAIL SHADER - Soft outer glow
// =============================================================================
float4 TrailBloomPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    
    // Wider falloff for bloom effect
    float verticalInterpolant = QuadraticBump(input.TexCoord.y);
    float bloomFade = pow(verticalInterpolant, 0.5);
    
    float3 bloomColor = lerp(uColor, uSecondaryColor, input.TexCoord.x) * 0.5;
    float bloomOpacity = bloomFade * uOpacity * 0.4 * input.Color.a;
    
    return float4(bloomColor * baseColor.rgb, bloomOpacity * baseColor.a);
}

// =============================================================================
// TECHNIQUES
// =============================================================================
technique DefaultTechnique
{
    pass DefaultPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 TrailShaderPS();
    }
}

technique BloomTechnique
{
    pass BloomPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 TrailBloomPS();
    }
}
