/*
 * SwingSpriteShader.fx
 * 
 * SWING SPRITE SHADER - Blade Deformation Effect
 * 
 * This shader distorts a blade sprite during swing animations, creating the
 * effect of the blade stretching and bending as it moves through the swing arc.
 * 
 * Based on Calamity Mod's SwingSprite shader used for the Exo Blade.
 * 
 * KEY PARAMETERS:
 * - rotation: The current swing angle displacement
 * - pommelToOriginPercent: How much of the blade is behind the pivot point (0-1)
 * - color: Color tint applied to the sprite
 * 
 * The shader works by:
 * 1. Taking the UV coordinates of the sprite
 * 2. Rotating them around a pivot point (near the handle)
 * 3. Applying the rotation based on the current swing angle
 * 4. Sampling the texture at the rotated coordinates
 * 
 * IMPORTANT: This shader works best with SQUARE textures where the blade
 * is positioned diagonally from corner to corner.
 */

sampler uImage0 : register(s0);
float4 uSourceRect;
float2 uImageSize0;
matrix uWorldViewProjection;

// Custom parameters for swing deformation
float rotation;           // Current swing angle displacement
float pommelToOriginPercent;  // Portion of blade behind pivot (0.05 default)
float4 color;            // Color tint

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
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

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    
    // Calculate the pivot point (near the handle/pommel of the blade)
    // For a diagonal blade in a square texture, the pivot is near one corner
    float2 pivot = float2(pommelToOriginPercent, 1.0 - pommelToOriginPercent);
    
    // Translate coordinates so pivot is at origin
    float2 translatedCoords = coords - pivot;
    
    // Apply rotation matrix
    float cosR = cos(rotation);
    float sinR = sin(rotation);
    float2 rotatedCoords;
    rotatedCoords.x = translatedCoords.x * cosR - translatedCoords.y * sinR;
    rotatedCoords.y = translatedCoords.x * sinR + translatedCoords.y * cosR;
    
    // Translate back
    rotatedCoords += pivot;
    
    // Check if coordinates are still within valid texture bounds
    if (rotatedCoords.x < 0 || rotatedCoords.x > 1 || rotatedCoords.y < 0 || rotatedCoords.y > 1)
    {
        return float4(0, 0, 0, 0); // Transparent if outside bounds
    }
    
    // Sample the texture at the rotated coordinates
    float4 texColor = tex2D(uImage0, rotatedCoords);
    
    // Apply color tint and input color
    return texColor * color * input.Color;
}

technique Technique1
{
    pass SwingSpritePass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
