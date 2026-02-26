// =============================================================================
// Incisor of Moonlight — Constellation Pierce Trail Shader
// =============================================================================
// Renders the lunar dash trail with a flowing constellation effect:
// sinusoidal bloom along the width, scrolling star-like brightness peaks,
// and a purple-to-silver color gradient.
//
// Vertex format: IncisorVertex (Position2D + Color + TextureCoordinates3D)
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Trail/streak texture
sampler uImage2 : register(s2); // Noise overlay

float3 uColor;            // Primary trail color (FrequencyPulse purple)
float3 uSecondaryColor;   // Secondary trail color (Crystal Edge silver)
float uOpacity;
float uTime;
matrix uWorldViewProjection;

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
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates.xy;

    // Width correction
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    // Sinusoidal bloom across width (bright center, fade edges)
    float bloomOpacity = pow(sin(coords.y * 3.14159), 4.8);

    // Scrolling streak texture (flowing energy)
    float streak = tex2D(uImage1, coords * 3 - float2(uTime * 2.2, 0)).r;

    // Noise modulation for organic feel
    float noise = tex2D(uImage2, coords * float2(2, 1) - float2(uTime * 1.5, 0)).r;
    float combined = streak + noise * bloomOpacity * 0.6;

    // Color gradient: primary → secondary based on noise
    float4 energyColor = float4(lerp(uColor, uSecondaryColor, noise * 0.7 + coords.x * 0.3), 1);

    // Constellation sparkle peaks along the trail
    float sparkle = pow(abs(sin(coords.x * 12.0 + uTime * 3.0)), 8.0) * 0.4;
    energyColor.rgb += sparkle * float3(0.9, 0.93, 1.0);

    // Gentle resonance pulse
    float pulse = sin(uTime * 4.0 + coords.x * 6.0) * 0.06 + 0.94;

    // Compose: bloom shape + energy + streak, fading along trail length
    float4 result = (energyColor * bloomOpacity + combined * bloomOpacity * 0.5) * color.a;
    result *= pow(1 - coords.x, 1.4) * pulse;

    return result;
}

technique Technique1
{
    pass IncisorPiercePass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
