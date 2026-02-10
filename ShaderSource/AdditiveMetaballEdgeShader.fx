// =============================================================================
// AdditiveMetaballEdgeShader.fx - SM 4.0 Level 9.1 Compatible
// =============================================================================
// Based on Calamity Mod's additive metaball shader.
// Unlike the standard edge shader, this one doesn't do edge detection.
// Instead, it averages neighboring pixels to create smooth blending
// and brightens toward white for intense additive effects.
//
// Used for fire, plasma, energy effects that should glow and merge additively.
// Compile with: mgcb /platform:Windows /profile:HiDef /importer:EffectImporter /processor:EffectProcessor
// =============================================================================

// The render target containing the metaball contents
// t0/s0: SpriteBatch automatically binds the texture being drawn here
Texture2D MetaballContents : register(t0);
SamplerState MetaballContentsSampler : register(s0)
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// Layer texture (optional overlay)
// t1/s1: Set via GraphicsDevice.Textures[1] in C# code
Texture2D OverlayTexture : register(t1);
SamplerState OverlayTextureSampler : register(s1)
{
    Filter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Standard shader parameters
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float4 uShaderSpecificData;

// Screen dimensions for calculating pixel offsets
float2 screenArea;

// UV offset for parallax scrolling
float2 layerOffset;

// Frame-to-frame screen position offset
float2 singleFrameScreenOffset;

// Vertex shader input
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Pixel shader input
struct PixelShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

// Pass-through vertex shader (required for SM 4.0)
PixelShaderInput MainVS(VertexShaderInput input)
{
    PixelShaderInput output;
    output.Position = input.Position;
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 AdditiveMetaballPS(PixelShaderInput input) : SV_TARGET
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    
    float2 originalCoords = coords;
    coords += layerOffset + singleFrameScreenOffset;
    
    // Sample offset in pixels (3 pixels in each direction)
    float2 offset = 3.0 / screenArea;
    
    // Sample this pixel and its neighbors
    float4 color = MetaballContents.Sample(MetaballContentsSampler, coords);
    float4 leftColor = MetaballContents.Sample(MetaballContentsSampler, coords + float2(-offset.x, 0));
    float4 rightColor = MetaballContents.Sample(MetaballContentsSampler, coords + float2(offset.x, 0));
    float4 topColor = MetaballContents.Sample(MetaballContentsSampler, coords + float2(0, -offset.y));
    float4 bottomColor = MetaballContents.Sample(MetaballContentsSampler, coords + float2(0, offset.y));
    
    // Average all colors for smooth blending
    float4 averageColor = (color + leftColor + rightColor + topColor + bottomColor) / 4.7;
    
    // Find the darkest channel value
    float lowestColorValue = min(averageColor.r, averageColor.g);
    lowestColorValue = min(lowestColorValue, averageColor.b);
    
    // Push toward white based on how dark the color is (inverse relationship)
    // This makes bright areas even brighter/whiter for that intense glow
    averageColor = lerp(averageColor, float4(1, 1, 1, 1), pow(lowestColorValue, 0.5)) * averageColor.a;
    
    return averageColor;
}

// =============================================================================
// TECHNIQUES
// =============================================================================
technique DefaultTechnique
{
    pass ParticlePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 AdditiveMetaballPS();
    }
}
