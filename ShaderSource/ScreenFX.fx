// =============================================================================
// MagnumOpus Screen Effects Shader - SM 4.0 Level 9.1 Compatible
// =============================================================================
// Screen-wide post-processing effects for boss fights and impacts.
// =============================================================================

Texture2D SpriteTexture;
SamplerState SpriteTextureSampler
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float2 uScreenResolution;
float2 uTargetPosition; // Normalized screen-space position (0-1)
float uIntensity;
float uTime;
float3 uColor;

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
// CHROMATIC ABERRATION - RGB channel separation
// =============================================================================
float4 ChromaticAberrationPS(PixelShaderInput input) : SV_TARGET
{
    float2 direction = input.TexCoord - uTargetPosition;
    float dist = length(direction);
    
    // Aberration strength falls off with distance
    float strength = uIntensity * 0.01 * saturate(1.0 - dist * 2.0);
    
    // Sample RGB channels at offset positions
    float2 redOffset = direction * strength;
    float2 blueOffset = direction * strength * -1.0;
    
    float r = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord + redOffset).r;
    float g = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord).g;
    float b = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord + blueOffset).b;
    float a = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord).a;
    
    return float4(r, g, b, a) * input.Color;
}

// =============================================================================
// RADIAL BLUR - Impact blur from center point
// =============================================================================
float4 RadialBlurPS(PixelShaderInput input) : SV_TARGET
{
    float2 direction = input.TexCoord - uTargetPosition;
    
    float4 color = float4(0, 0, 0, 0);
    
    [unroll]
    for (int i = 0; i < 8; i++)
    {
        float t = (float)i / 7.0;
        float2 sampleCoords = input.TexCoord - direction * t * uIntensity * 0.1;
        color += SpriteTexture.Sample(SpriteTextureSampler, sampleCoords);
    }
    
    return (color / 8.0) * input.Color;
}

// =============================================================================
// WAVE DISTORTION - Ripple effect
// =============================================================================
float4 WaveDistortionPS(PixelShaderInput input) : SV_TARGET
{
    float2 direction = input.TexCoord - uTargetPosition;
    float dist = length(direction);
    
    // Animated wave pattern
    float wave = sin(dist * 50.0 - uTime * 10.0);
    wave *= saturate(1.0 - dist * 3.0) * uIntensity * 0.01;
    
    float2 distortedCoords = input.TexCoord + normalize(direction) * wave;
    float4 color = SpriteTexture.Sample(SpriteTextureSampler, distortedCoords);
    
    return color * input.Color;
}

// =============================================================================
// VIGNETTE - Edge darkening for focus
// =============================================================================
float4 VignettePS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    
    float2 center = float2(0.5, 0.5);
    float dist = length(input.TexCoord - center) * 1.414;
    
    float vignette = 1.0 - pow(dist, 2.0) * uIntensity;
    vignette = saturate(vignette);
    
    return float4(baseColor.rgb * vignette, baseColor.a) * input.Color;
}

// =============================================================================
// COLOR FLASH - Screen-wide color overlay
// =============================================================================
float4 ColorFlashPS(PixelShaderInput input) : SV_TARGET
{
    float4 baseColor = SpriteTexture.Sample(SpriteTextureSampler, input.TexCoord);
    float3 flashedColor = lerp(baseColor.rgb, uColor, uIntensity);
    return float4(flashedColor, baseColor.a) * input.Color;
}

// =============================================================================
// TECHNIQUES
// =============================================================================
technique DefaultTechnique
{
    pass DefaultPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 ChromaticAberrationPS();
    }
}

technique RadialBlurTechnique
{
    pass RadialBlurPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 RadialBlurPS();
    }
}

technique WaveDistortionTechnique
{
    pass WaveDistortionPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 WaveDistortionPS();
    }
}

technique VignetteTechnique
{
    pass VignettePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 VignettePS();
    }
}

technique ColorFlashTechnique
{
    pass ColorFlashPass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 ColorFlashPS();
    }
}
