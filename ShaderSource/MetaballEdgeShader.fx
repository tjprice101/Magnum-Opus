// =============================================================================
// MetaballEdgeShader.fx - SM 4.0 Level 9.1 Compatible
// =============================================================================
// Based on Calamity Mod's metaball edge detection shader.
// This shader detects edges of metaball shapes (where alpha transitions to 0)
// and applies an edge color while compositing layer textures on top.
//
// Compile with: mgcb /platform:Windows /profile:HiDef /importer:EffectImporter /processor:EffectProcessor
// =============================================================================

// The render target containing the drawn metaball circles (white circles on black)
// t0/s0: SpriteBatch automatically binds the texture being drawn here
Texture2D MetaballContents : register(t0);
SamplerState MetaballContentsSampler : register(s0)
{
    Filter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

// The layer texture to overlay on the metaball shapes  
// t1/s1: Set via GraphicsDevice.Textures[1] in C# code
Texture2D OverlayTexture : register(t1);
SamplerState OverlayTextureSampler : register(s1)
{
    Filter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

// Parameters
float2 screenSize;      // Screen dimensions in pixels
float2 layerSize;       // Layer texture dimensions in pixels  
float2 layerOffset;     // UV offset for scrolling the layer texture (parallax effect)
float4 edgeColor;       // The color to draw at edges of metaballs
float2 singleFrameScreenOffset;  // Single frame screen offset (for smooth scrolling)
float4 layerColor;      // Optional per-layer color tint

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

// Convert normalized UV coords to screen pixel coords
float2 ConvertToScreenCoords(float2 coords)
{
    return coords * screenSize;
}

// Convert screen pixel coords back to normalized UV coords
float2 ConvertFromScreenCoords(float2 coords)
{
    return coords / screenSize;
}

float4 MetaballEdgePS(PixelShaderInput input) : SV_TARGET
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    
    // Sample the base color from the metaball render target
    float4 baseColor = MetaballContents.Sample(MetaballContentsSampler, coords);
    
    // If there's no metaball content here (alpha is 0), output transparent
    // This offset trick prevents false edge detection at fully empty pixels
    float alphaOffset = (1.0 - any(baseColor.a));
    
    // Check neighboring pixels (2 pixels away in each direction) for alpha
    // If any neighbor has 0 alpha, we're at an edge
    float left = MetaballContents.Sample(MetaballContentsSampler, 
        ConvertFromScreenCoords(ConvertToScreenCoords(coords) + float2(-2, 0))).a + alphaOffset;
    float right = MetaballContents.Sample(MetaballContentsSampler, 
        ConvertFromScreenCoords(ConvertToScreenCoords(coords) + float2(2, 0))).a + alphaOffset;
    float top = MetaballContents.Sample(MetaballContentsSampler, 
        ConvertFromScreenCoords(ConvertToScreenCoords(coords) + float2(0, -2))).a + alphaOffset;
    float bottom = MetaballContents.Sample(MetaballContentsSampler, 
        ConvertFromScreenCoords(ConvertToScreenCoords(coords) + float2(0, 2))).a + alphaOffset;
    
    // Determine if each neighbor has zero alpha (step returns 1 if value <= 0)
    float leftHasNoAlpha = step(left, 0);
    float rightHasNoAlpha = step(right, 0);
    float topHasNoAlpha = step(top, 0);
    float bottomHasNoAlpha = step(bottom, 0);
    
    // If ANY neighbor has no alpha, we're at an edge (use saturate to clamp 0-1)
    // conditionOpacityFactor = 1 means NOT at edge, = 0 means AT edge
    float conditionOpacityFactor = 1.0 - saturate(leftHasNoAlpha + rightHasNoAlpha + topHasNoAlpha + bottomHasNoAlpha);
    
    // Calculate the layer texture color with scrolling offset
    float2 layerCoords = (coords + layerOffset + singleFrameScreenOffset) * screenSize / layerSize;
    float4 layerCalcedColor = OverlayTexture.Sample(OverlayTextureSampler, layerCoords);
    
    // Combine layer color with metaball contents and per-layer tint
    float4 defaultColor = layerCalcedColor * MetaballContents.Sample(MetaballContentsSampler, coords) * sampleColor * layerColor;
    
    // Final output:
    // - At edges (conditionOpacityFactor = 0): use edge color
    // - Not at edges (conditionOpacityFactor = 1): use layer-textured color
    return (defaultColor * conditionOpacityFactor) + (edgeColor * sampleColor * (1.0 - conditionOpacityFactor));
}

// =============================================================================
// TECHNIQUES
// =============================================================================
technique DefaultTechnique
{
    pass ParticlePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MetaballEdgePS();
    }
}
