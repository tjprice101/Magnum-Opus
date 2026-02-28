// =============================================================================
// Incisor of Moonlight — Slash Arc Trail Shader
// =============================================================================
// Renders the swing arc trail with standing-wave resonance patterns,
// constellation node highlights, and a moonlight color palette.
//
// Palette: Deep Resonance purple → Frequency Pulse lavender → Resonant Silver
//          → Ice Blue Clarity → Crystal Edge white
//
// Vertex format: IncisorVertex (Position2D + Color + TextureCoordinates3D)
// where TextureCoordinates.z = width correction factor.
//
// UV Layout:
//   X (coords.x) = completion along trail (0 = current, 1 = tail)
//   Y (coords.y) = across trail width (0 = top edge, 1 = bottom edge)
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise texture (VoronoiNoise)

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

    // --- Width-based band structure ---
    // Center distance (0 at center, 1 at edge)
    float centerDist = abs(coords.y - 0.5) * 2.0;
    // Smooth bell-curve core brightness
    float coreBand = exp(-centerDist * centerDist * 3.5);
    // Soft outer glow falloff
    float outerGlow = exp(-centerDist * centerDist * 1.2);

    // Standing wave resonance pattern
    float waveFreq = 4.0 + uIntensity * 4.0;
    float standingWave = StandingWave(coords.x, waveFreq, uTime);

    // Noise sampling with sine-wrapped X to prevent hard cutoffs
    float2 noiseCoords = coords * float2(0.5, 2.0) - float2(uTime * 0.35, 0);
    noiseCoords.x = sin(noiseCoords.x * 5.4) * 0.5 + 0.5;
    float noise1 = tex2D(uImage1, noiseCoords).r;
    float noise2 = pow(tex2D(uImage1, noiseCoords * 2.1).r, 1.5);
    float noise3 = pow(tex2D(uImage1, noiseCoords * 1.15).r, 1.3);

    // --- Trail opacity: smooth fade along length, noise-modulated ---
    // Head-to-tail fade: strong at head, fades to tail
    float lengthFade = pow(saturate(1.0 - coords.x), 1.6);
    // Noise breathing for organic feel
    float noiseFade = saturate(noise1 * 0.6 + 0.5);
    float opacity = lengthFade * noiseFade * outerGlow;

    // --- Color layering ---
    // Base: vertex color (passed from C#, typically Frequency Pulse lavender)
    // Core silver-white along the center band
    float silverWeight = coreBand * pow(1.0 - coords.x, 0.8) * (0.6 + noise2 * 0.4);
    color.rgb = lerp(color.rgb, uColor, silverWeight);

    // Dark purple base toward edges and tail
    float darkWeight = saturate((1.0 - coreBand) * 0.8 + coords.x * 0.4 + noise1 * 0.08);
    color.rgb = lerp(color.rgb, uSecondaryColor, darkWeight * 0.7);

    // Ice blue leading edge (bright strip at the swing's cutting edge)
    float iceEdge = InverseLerp(0.3, 0.0, coords.y) * pow(1.0 - coords.x, 1.8);
    color.rgb = lerp(color.rgb, fireColor, iceEdge * 0.85);

    // Constellation node highlights
    float nodeCount = 3.0 + uIntensity * 4.0;
    float nodeGlow = ConstellationNodes(coords.x, nodeCount) * standingWave;

    // Standing wave brightness modulation
    float waveBright = 0.8 + standingWave * 0.2 * uIntensity;

    // High-frequency shimmer (harmonic overtone)
    float shimmer = sin(coords.x * 20.0 - uTime * 6.0) * 0.03 * uIntensity + 1.0;

    // --- Compose final color ---
    float brightMult = (noise3 * 1.5 + 1.8) * waveBright * shimmer;
    float4 finalColor = float4(color.rgb * brightMult, 1.0) * opacity;

    // Constellation node whitening: sharp silver-white star spots
    finalColor.rgb = lerp(finalColor.rgb, float3(0.93, 0.95, 1.0) * brightMult * opacity, nodeGlow * 0.6);

    // Final alpha: smooth edges, no hard cutoff
    finalColor.a = opacity * color.a * (0.85 + coreBand * 0.15);

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
