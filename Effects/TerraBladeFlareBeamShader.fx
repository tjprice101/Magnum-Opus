// =============================================================================
// MagnumOpus — Terra Blade Flare Beam Shader
// =============================================================================
//
// Flowing energy beam shader with heavy wave distortion and directional
// motion-blur feel. Designed for the Sandbox TerraBlade's flare beam
// projectile rendered as a vertex-mesh beam strip.
//
// Based on BeamGradientFlow.fx architecture with two key additions:
//   1. Multi-frequency wave distortion (V-axis sinusoidal displacement)
//   2. Elongated directional noise sampling for motion-blur streaking
//
// UV Layout:
//   U (coords.x) = position along beam (0→1) + time scroll offset
//   V (coords.y) = position across beam (0 = top edge, 1 = bottom edge)
//
// =============================================================================

float4x4 uWorldViewProjection;

sampler uImage0 : register(s0); // Primary noise texture (SoftCircularCaustics)
sampler uImage1 : register(s1); // Secondary noise texture (TileableFBMNoise)

float3 uColor;           // Primary beam color
float3 uSecondaryColor;  // Secondary beam color
float uOpacity;          // Overall opacity (lifecycle fade)
float uTime;             // Animation time (Main.GlobalTimeWrappedHourly)
float uIntensity;        // Base brightness multiplier

float uNoiseSpeed1;      // Primary noise scroll speed
float uNoiseSpeed2;      // Secondary noise scroll speed
float uNoiseScale1;      // Primary noise UV scale along beam
float uNoiseScale2;      // Secondary noise UV scale along beam
float uEdgeSoftness;     // Edge fade range (0.1 = sharp, 0.5 = very soft)
float uPulseSpeed;       // Pulse animation rate
float uOverbrightMult;   // HDR bloom multiplier

// Wave distortion parameters
float uWaveAmplitude;    // V displacement strength (0.03-0.15 typical)
float uWaveFrequency;    // Primary wave cycles along beam (4.0-8.0 typical)

// =============================================================================
// VERTEX SHADER
// =============================================================================
struct VS_INPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

struct VS_OUTPUT
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VS_OUTPUT MainVS(VS_INPUT input)
{
    VS_OUTPUT output;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// =============================================================================
// PIXEL SHADER — Wave Distortion + Motion Blur Beam
// =============================================================================
float4 WaveMotionBlurPS(VS_OUTPUT input) : COLOR0
{
    float2 uv = input.TexCoord;
    float u = uv.x; // Along beam (includes scroll from vertex generation)
    float v = uv.y; // Across beam (0 = top, 1 = bottom, 0.5 = center)

    // --- Wave Distortion ---
    // Multi-frequency sinusoidal displacement of V coordinate.
    // Tapered at beam endpoints so the beam stays anchored at head/tail.
    float endpointTaper = sin(saturate(u * 0.3) * 3.14159); // Soft fade at ends
    float wave1 = sin(u * uWaveFrequency + uTime * 3.5) * uWaveAmplitude;
    float wave2 = sin(u * uWaveFrequency * 1.73 - uTime * 5.2) * uWaveAmplitude * 0.45;
    float wave3 = sin(u * uWaveFrequency * 2.91 + uTime * 7.1) * uWaveAmplitude * 0.2;
    float totalWave = (wave1 + wave2 + wave3) * endpointTaper;
    float vDistorted = v + totalWave;

    // --- Edge Distance & Falloff (using distorted V) ---
    float edgeDist = 1.0 - abs(vDistorted - 0.5) * 2.0;
    float edgeFade = smoothstep(0.0, uEdgeSoftness, edgeDist);

    // --- Elongated Noise: Directional Motion Blur ---
    // Primary noise is sampled with high U stretch and compressed V,
    // creating elongated streaks that simulate directional motion blur.
    float2 noiseUV1 = float2(
        u * uNoiseScale1 + uTime * uNoiseSpeed1,
        vDistorted * 0.3 + 0.35  // Compressed V = horizontal streaking
    );
    float noise1 = tex2D(uImage0, noiseUV1).r;

    // Secondary noise with different stretch and counter-scroll
    float2 noiseUV2 = float2(
        u * uNoiseScale2 - uTime * uNoiseSpeed2 * 0.7,
        vDistorted * 0.4 + 0.3
    );
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // --- Color Gradient ---
    float smoothNoise1 = smoothstep(0.1, 0.9, noise1);
    float smoothNoise2 = smoothstep(0.1, 0.9, noise2);
    float colorMix = smoothNoise1 * 0.5 + smoothNoise2 * 0.5;
    colorMix = 0.2 + colorMix * 0.6;
    float3 baseColor = lerp(uColor, uSecondaryColor, colorMix);

    // --- White-Hot Core ---
    float coreFactor = smoothstep(0.2, 0.75, edgeDist);
    float3 coreColor = lerp(baseColor, float3(1.0, 1.0, 1.0), coreFactor * 0.5);

    // --- Pulse Animation ---
    float pulse = sin(uTime * uPulseSpeed + u * 5.0) * 0.1 + 0.9;

    // --- Noise-Modulated Intensity ---
    // Stronger noise influence for more dramatic flowing energy
    float noiseIntensity = 0.5 + noise1 * 0.35 + noise2 * 0.15;

    // --- Final Composition ---
    float3 finalColor = coreColor * uIntensity * pulse * noiseIntensity;
    float alpha = edgeFade * uOpacity * input.Color.a;

    return float4(finalColor * uOverbrightMult * alpha, alpha);
}

// =============================================================================
// TECHNIQUE
// =============================================================================
technique WaveMotionBlurBeam
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 WaveMotionBlurPS();
    }
}
