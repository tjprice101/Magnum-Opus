// =============================================================================
// Incisor of Moonlight — Slash Arc Trail Shader
// =============================================================================
// Renders the swing arc trail with standing-wave resonance patterns,
// constellation node highlights, and a moonlight color palette.
//
// Vertex format: IncisorVertex (Position2D + Color + TextureCoordinates3D)
// where TextureCoordinates.z = width correction factor.
//
// UV Layout:
//   X (coords.x) = completion along trail (0 = current, 1 = tail)
//   Y (coords.y) = across trail width (0 = top edge, 1 = bottom edge)
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise texture (VoronoiNoise or StarFieldScatter)

float3 uColor;            // Primary: Resonant Silver
float3 uSecondaryColor;   // Dark base: Deep Resonance purple
float3 fireColor;         // Edge color: Ice Blue Clarity
float uOpacity;
float uTime;
float uIntensity;          // Combo resonance level (0.3–1.0)
matrix uWorldViewProjection;
bool flipped;

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

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

// Standing wave: tuning-fork resonance visualization
float StandingWave(float u, float freq, float time)
{
    return abs(sin(u * 3.14159 * freq + time * 2.0));
}

// Constellation nodes: sharp Gaussian peaks at regular intervals
float ConstellationNodes(float u, float nodeCount)
{
    float spacing = 1.0 / (nodeCount + 1.0);
    float minDist = 1.0;
    for (int i = 1; i <= 7; i++)
    {
        if ((float)i > nodeCount) break;
        float dist = abs(u - spacing * (float)i);
        minDist = min(minDist, dist);
    }
    return exp(-minDist * minDist * 600.0);
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float4 color = input.Color;
    float2 coords = input.TextureCoordinates.xy;

    // Correct for width distortion from primitive renderer
    coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;

    if (flipped)
        coords.y = 1 - coords.y;

    // Standing wave resonance pattern
    float waveFreq = 4.0 + uIntensity * 4.0;
    float standingWave = StandingWave(coords.x, waveFreq, uTime);

    // Noise sampling with sine-wrapped X to prevent hard cutoffs
    float2 noiseCoords = coords * float2(0.5, 2.0) - float2(uTime * 0.35, 0);
    noiseCoords.x = sin(noiseCoords.x * 5.4) * 0.5 + 0.5;
    float noise1 = tex2D(uImage1, noiseCoords).r;
    float noise2 = pow(tex2D(uImage1, noiseCoords * 2.1).r, 1.5);
    float noise3 = pow(tex2D(uImage1, noiseCoords * 1.15).r, 1.3);

    // Trail opacity: noise-driven with fade along length and width
    float opacity = noise1 * pow(saturate((1 - coords.x) - noise1 * coords.y * 0.45), 2.8);

    // Primary color blend (silver/white core)
    color = lerp(color, float4(uColor, 1), noise2);

    // Dark purple base toward bottom and tail of trail
    float darkWeight = saturate(coords.y * 1.7 + coords.x * 0.5 + noise1 * 0.12);
    color = lerp(color, float4(uSecondaryColor, 1), darkWeight);

    // Ice blue edge at the top of the trail (like fire streak but moonlight)
    float iceWeight = InverseLerp(0.28, 0, coords.y) * pow(1 - coords.x, 1.5);
    color = lerp(color, float4(fireColor, 1), iceWeight);

    // Constellation node highlights
    float nodeCount = 3.0 + uIntensity * 4.0;
    float nodeGlow = ConstellationNodes(coords.x, nodeCount) * standingWave;

    // Standing wave brightness modulation
    float waveBright = 0.7 + standingWave * 0.3 * uIntensity;

    // High-frequency shimmer
    float shimmer = sin(coords.x * 20.0 - uTime * 6.0) * 0.04 * uIntensity + 1.0;

    // Compose final color
    float4 finalColor = color * opacity * (noise3 * 2.2 + 2.2) * waveBright * shimmer;

    // Apply constellation node whitening
    finalColor.rgb = lerp(finalColor.rgb, float3(0.93, 0.95, 1.0), nodeGlow * 0.65);

    // Transparent edge for fire streak
    finalColor.a = lerp(finalColor.a, 0, 1 - iceWeight);

    return finalColor * input.Color.a;
}

technique Technique1
{
    pass IncisorSlashPass
    {
        VertexShader = compile vs_3_0 VertexShaderFunction();
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
