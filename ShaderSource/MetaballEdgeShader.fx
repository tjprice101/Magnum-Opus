// =============================================================================
// MetaballEdgeShader.fx - SM 4.0 Level 9.1 Compatible (ENHANCED)
// =============================================================================
// Based on Calamity Mod's metaball edge detection shader.
// This shader detects edges of metaball shapes (where alpha transitions to 0)
// and applies an edge color while compositing layer textures on top.
//
// ENHANCED FEATURES:
// - Multi-sample Sobel edge detection for smoother edges
// - Glow rim effect with configurable falloff
// - Inner glow/bloom support
// - Animated edge pulsing
//
// USAGE: Include shared utility library for noise, SDFs, color utilities:
// #include "HLSLLibrary.fxh"
// (Uncomment above line after compiling library into your build pipeline)
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

// Enhanced parameters (set defaults to maintain backward compatibility)
float edgeThickness;    // Edge detection thickness multiplier (default: 2.0)
float glowIntensity;    // Outer glow intensity (0-2, default: 0.5)
float glowFalloff;      // How quickly glow fades (1-5, default: 2.0)
float innerGlowIntensity; // Inner bloom intensity (0-1, default: 0.3)
float time;             // Animation time for pulsing effects
float pulseSpeed;       // Edge pulse animation speed (0 = disabled)
float pulseIntensity;   // Edge pulse intensity multiplier

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

// =============================================================================
// ENHANCED EDGE DETECTION FUNCTIONS
// =============================================================================

// Sample alpha at offset position
float SampleAlpha(float2 coords, float2 pixelOffset)
{
    float2 sampleCoords = ConvertFromScreenCoords(ConvertToScreenCoords(coords) + pixelOffset);
    return MetaballContents.Sample(MetaballContentsSampler, sampleCoords).a;
}

// Sobel-based edge detection for smoother edges
// Returns edge strength (0 = no edge, 1 = strong edge)
float SobelEdgeDetection(float2 coords, float thickness)
{
    // Sample 3x3 neighborhood
    float tl = SampleAlpha(coords, float2(-thickness, -thickness));
    float t  = SampleAlpha(coords, float2(0, -thickness));
    float tr = SampleAlpha(coords, float2(thickness, -thickness));
    float l  = SampleAlpha(coords, float2(-thickness, 0));
    float r  = SampleAlpha(coords, float2(thickness, 0));
    float bl = SampleAlpha(coords, float2(-thickness, thickness));
    float b  = SampleAlpha(coords, float2(0, thickness));
    float br = SampleAlpha(coords, float2(thickness, thickness));
    
    // Sobel kernels for gradient calculation
    float gx = (tr + 2*r + br) - (tl + 2*l + bl);
    float gy = (bl + 2*b + br) - (tl + 2*t + tr);
    
    // Gradient magnitude (edge strength)
    return saturate(sqrt(gx*gx + gy*gy));
}

// Calculate distance-based glow falloff
// Returns glow intensity based on distance from edge
float CalculateGlowFalloff(float2 coords, float maxDistance, float falloffPower)
{
    float minAlphaDistance = 9999.0;
    
    // Sample in expanding rings to find distance to edge
    [unroll]
    for (int ring = 1; ring <= 4; ring++)
    {
        float dist = ring * 2.0;
        
        // Sample 8 directions
        float a1 = SampleAlpha(coords, float2(dist, 0));
        float a2 = SampleAlpha(coords, float2(-dist, 0));
        float a3 = SampleAlpha(coords, float2(0, dist));
        float a4 = SampleAlpha(coords, float2(0, -dist));
        float a5 = SampleAlpha(coords, float2(dist * 0.707, dist * 0.707));
        float a6 = SampleAlpha(coords, float2(-dist * 0.707, dist * 0.707));
        float a7 = SampleAlpha(coords, float2(dist * 0.707, -dist * 0.707));
        float a8 = SampleAlpha(coords, float2(-dist * 0.707, -dist * 0.707));
        
        // If any sample has alpha, we found interior
        float hasInterior = step(0.01, a1 + a2 + a3 + a4 + a5 + a6 + a7 + a8);
        minAlphaDistance = lerp(minAlphaDistance, dist, step(minAlphaDistance, dist) * hasInterior);
    }
    
    // Calculate falloff based on distance
    float normalizedDist = saturate(minAlphaDistance / maxDistance);
    return pow(1.0 - normalizedDist, falloffPower);
}

float4 MetaballEdgePS(PixelShaderInput input) : SV_TARGET
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    
    // Sample the base color from the metaball render target
    float4 baseColor = MetaballContents.Sample(MetaballContentsSampler, coords);
    
    // If there's no metaball content here (alpha is 0), calculate glow for outer regions
    if (baseColor.a < 0.01)
    {
        // Only compute outer glow if intensity is set
        if (glowIntensity > 0.01)
        {
            // Find distance to nearest metaball content
            float glowAmount = 0.0;
            float searchRadius = 8.0 * max(1.0, edgeThickness);
            
            // Sample in expanding rings
            [unroll]
            for (int ring = 1; ring <= 4; ring++)
            {
                float dist = ring * 2.0;
                float ringGlow = 0.0;
                
                // Sample 8 directions
                ringGlow += SampleAlpha(coords, float2(dist, 0));
                ringGlow += SampleAlpha(coords, float2(-dist, 0));
                ringGlow += SampleAlpha(coords, float2(0, dist));
                ringGlow += SampleAlpha(coords, float2(0, -dist));
                ringGlow += SampleAlpha(coords, float2(dist * 0.707, dist * 0.707));
                ringGlow += SampleAlpha(coords, float2(-dist * 0.707, dist * 0.707));
                ringGlow += SampleAlpha(coords, float2(dist * 0.707, -dist * 0.707));
                ringGlow += SampleAlpha(coords, float2(-dist * 0.707, -dist * 0.707));
                
                // Falloff based on distance
                float falloff = pow(1.0 - (dist / searchRadius), max(1.0, glowFalloff));
                glowAmount += (ringGlow / 8.0) * falloff;
            }
            
            glowAmount = saturate(glowAmount * glowIntensity);
            
            // Apply pulse if enabled
            if (pulseSpeed > 0.01)
            {
                float pulse = sin(time * pulseSpeed) * 0.5 + 0.5;
                glowAmount *= lerp(1.0, pulse, pulseIntensity);
            }
            
            // Return outer glow color
            float4 glowColor = edgeColor * sampleColor * glowAmount;
            glowColor.a *= glowAmount;
            return glowColor;
        }
        
        return float4(0, 0, 0, 0);
    }
    
    // Use enhanced Sobel edge detection
    float thickness = max(2.0, edgeThickness);
    float edgeStrength = SobelEdgeDetection(coords, thickness);
    
    // Also check basic neighbor sampling for sharper edges (original method)
    float alphaOffset = (1.0 - any(baseColor.a));
    float left = SampleAlpha(coords, float2(-thickness, 0)) + alphaOffset;
    float right = SampleAlpha(coords, float2(thickness, 0)) + alphaOffset;
    float top = SampleAlpha(coords, float2(0, -thickness)) + alphaOffset;
    float bottom = SampleAlpha(coords, float2(0, thickness)) + alphaOffset;
    
    float leftHasNoAlpha = step(left, 0);
    float rightHasNoAlpha = step(right, 0);
    float topHasNoAlpha = step(top, 0);
    float bottomHasNoAlpha = step(bottom, 0);
    
    float basicEdge = saturate(leftHasNoAlpha + rightHasNoAlpha + topHasNoAlpha + bottomHasNoAlpha);
    
    // Combine edge detection methods for best results
    float finalEdge = saturate(max(basicEdge, edgeStrength));
    
    // Apply edge pulse animation
    if (pulseSpeed > 0.01)
    {
        float pulse = sin(time * pulseSpeed) * 0.5 + 0.5;
        float4 pulseColor = edgeColor * (1.0 + pulse * pulseIntensity);
        // Mix edge color with pulse
        finalEdge *= lerp(1.0, 1.0 + pulse * 0.3, pulseIntensity);
    }
    
    // Calculate the layer texture color with scrolling offset
    float2 layerCoords = (coords + layerOffset + singleFrameScreenOffset) * screenSize / layerSize;
    float4 layerCalcedColor = OverlayTexture.Sample(OverlayTextureSampler, layerCoords);
    
    // Combine layer color with metaball contents and per-layer tint
    float4 defaultColor = layerCalcedColor * baseColor * sampleColor * layerColor;
    
    // Add inner glow/bloom effect
    if (innerGlowIntensity > 0.01)
    {
        // Inner glow based on distance from edge (brighter toward center)
        float innerGlow = (1.0 - finalEdge) * innerGlowIntensity;
        float4 innerGlowColor = edgeColor * innerGlow * 0.5;
        defaultColor = defaultColor + innerGlowColor;
    }
    
    // Final output:
    // - At edges: blend edge color based on edge strength
    // - Not at edges: use layer-textured color with optional inner glow
    float conditionOpacityFactor = 1.0 - finalEdge;
    return (defaultColor * conditionOpacityFactor) + (edgeColor * sampleColor * finalEdge);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

// Default technique - backward compatible with existing code
technique DefaultTechnique
{
    pass ParticlePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MetaballEdgePS();
    }
}

// Enhanced technique with all new features enabled
technique EnhancedTechnique
{
    pass ParticlePass
    {
        VertexShader = compile vs_4_0_level_9_1 MainVS();
        PixelShader = compile ps_4_0_level_9_1 MetaballEdgePS();
    }
}
