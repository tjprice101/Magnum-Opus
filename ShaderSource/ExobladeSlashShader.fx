/*
 * ExobladeSlashShader.fx
 * 
 * EXOBLADE SLASH TRAIL SHADER
 * 
 * This shader creates the characteristic "flaming slash" effect seen on the Exo Blade.
 * It uses noise textures and UV coordinates to create a dynamic, animated trail.
 * 
 * Based on Calamity Mod's ExobladeSlash shader.
 * 
 * KEY FEATURES:
 * - Voronoi noise sampling for organic-looking flames
 * - Three-color gradient (primary, secondary, fire edge)
 * - Time-based animation for flowing effect
 * - White-hot edge effect near the blade
 * - Fade-out toward trail end
 * 
 * REQUIRED TEXTURES:
 * - uImage0: Not used (trail is procedural)
 * - uImage1: Noise texture (e.g., VoronoiShapes or GreyscaleGradients)
 */

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
float3 uColor;             // Primary color (e.g., cyan/aqua)
float3 uSecondaryColor;    // Dark/shadow color (e.g., dark purple)
float3 fireColor;          // Fire edge color (e.g., orange)
float uOpacity;
float uSaturation;
float uRotation;
float uTime;               // Time for animation
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;
matrix uWorldViewProjection;
float4 uShaderSpecificData;
bool flipped;              // Whether to flip the trail vertically

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

// Utility function for smooth 0-1 clamping with fade
float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates.xy;
    
    // Account for texture distortion artifacts from primitive rendering
    // The z coordinate contains the width scaling factor
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;
    
    // Flip if needed (for direction changes)
    if (flipped)
        coords.y = 1 - coords.y;
    
    // ========================================
    // NOISE SAMPLING
    // ========================================
    
    // Calculate base noise sampling coordinates
    // Squish factors control the "sameness" vs "detail" of the pattern
    float2 noiseDetail = float2(2, 2);
    float2 noiseCoords = coords * float2(1.0 / noiseDetail.x, noiseDetail.y) - float2(uTime * 0.45, 0);
    
    // Use sine to prevent sudden texture cutoffs (creates smooth looping)
    noiseCoords.x = sin(noiseCoords.x * 5.4) * 0.5 + 0.5;
    
    // Sample multiple noise layers with different scales and powers
    float noise = tex2D(uImage1, noiseCoords).r;
    float noise2 = pow(tex2D(uImage1, noiseCoords * 2.2).r, 1.6);
    float noise3 = pow(tex2D(uImage1, noiseCoords * 1.1).r, 1.3);
    
    // ========================================
    // OPACITY CALCULATION
    // ========================================
    
    // Combine noise with position-based fading
    // - X coordinate (trail progress): fades out further along the trail
    // - Y coordinate (edge distance): bottom of trail fades more
    float opacity = noise * pow(saturate((1 - coords.x) - noise * coords.y * 0.54), 3);
    
    // ========================================
    // COLOR MIXING
    // ========================================
    
    // Fade to primary color based on noise
    color = lerp(color, float4(uColor, 1), noise2);
    
    // Dark color weight: stronger further along trail and near bottom
    float darkColorWeight = saturate(coords.y * 1.8 + coords.x * 0.45 + noise * 0.1);
    color = lerp(color, float4(uSecondaryColor, 1), darkColorWeight);
    
    // Fire edge effect: applies to top 30% of trail, fades with trail progress
    float fireColorWeight = InverseLerp(0.3, 0, coords.y) * pow(1 - coords.x, 1.56);
    color = lerp(color, float4(fireColor, 1), fireColorWeight);
    
    // ========================================
    // FINAL OUTPUT
    // ========================================
    
    // Combine all effects
    float4 noiseColor = color * opacity * (noise3 * 2.4 + 2.4);
    
    // Fire edge has reduced alpha for additive-like blend
    noiseColor.a = lerp(noiseColor.a, 0, 1 - fireColorWeight);
    
    // Apply input alpha
    return noiseColor * input.Color.a;
}

technique Technique1
{
    pass TrailPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
