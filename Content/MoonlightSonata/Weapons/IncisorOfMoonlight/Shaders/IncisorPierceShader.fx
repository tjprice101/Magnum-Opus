// =============================================================================
// Incisor of Moonlight — Constellation Pierce Trail Shader
// =============================================================================
// Renders the lunar dash trail with a flowing constellation effect:
// sinusoidal bloom along the width, scrolling star-like brightness peaks,
// and a purple-to-silver color gradient. Moonlight Sonata identity.
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

    // Center distance for band shaping (0 at center, 1 at edge)
    float centerDist = abs(coords.y - 0.5) * 2.0;

    // --- Multi-layer width shaping ---
    // Bright inner core (sharp bell curve)
    float innerCore = exp(-centerDist * centerDist * 8.0);
    // Softer mid band
    float midBand = exp(-centerDist * centerDist * 2.8);
    // Wide outer glow halo
    float outerHalo = exp(-centerDist * centerDist * 0.9);

    // --- Scrolling streak texture (flowing energy) ---
    float streak = tex2D(uImage1, coords * float2(3.0, 1.0) - float2(uTime * 2.5, 0)).r;

    // --- Noise modulation for organic feel ---
    float noise = tex2D(uImage2, coords * float2(2.0, 1.0) - float2(uTime * 1.5, 0)).r;
    float noise2 = tex2D(uImage2, coords * float2(1.2, 0.8) - float2(uTime * 0.8, 0.1)).r;

    // --- Head-to-tail fade ---
    float lengthFade = pow(1.0 - coords.x, 1.6);

    // --- Color gradient: primary → secondary based on noise + position ---
    float colorMix = noise * 0.5 + coords.x * 0.3 + centerDist * 0.2;
    float3 energyColor = lerp(uColor, uSecondaryColor, saturate(colorMix));
    // Brighten toward center
    energyColor = lerp(energyColor, float3(0.92, 0.94, 1.0), innerCore * 0.5);

    // --- Constellation sparkle peaks along the trail ---
    float sparkle = pow(abs(sin(coords.x * 14.0 + uTime * 3.5)), 12.0) * 0.5;
    // Second harmonic offset for visual complexity
    sparkle += pow(abs(sin(coords.x * 9.0 - uTime * 2.0 + 1.5)), 10.0) * 0.25;
    sparkle *= midBand;

    // --- Gentle resonance pulse (breathing effect) ---
    float pulse = sin(uTime * 3.5 + coords.x * 5.0) * 0.05 + 0.95;

    // --- Compose layers ---
    float streakContrib = streak * midBand * 0.4;
    float noiseContrib = noise2 * outerHalo * 0.2;
    float coreBright = (midBand * 0.7 + innerCore * 0.3 + streakContrib + noiseContrib);

    float4 result = float4(energyColor, 1.0) * coreBright * lengthFade * pulse;

    // Add sparkle whitening
    result.rgb += sparkle * float3(0.9, 0.93, 1.0) * lengthFade;

    // Alpha: smooth edges, no hard cutoffs
    result.a = coreBright * lengthFade * color.a;

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
