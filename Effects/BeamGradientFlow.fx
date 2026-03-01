// =============================================================================
// MagnumOpus Beam Gradient Flow Shader - VS 2.0 + PS 2.0
// =============================================================================
// Flowing energy beam shader with scrolling noise textures.
// Designed for DrawUserIndexedPrimitives with VertexPositionColorTexture.
//
// UV Layout:
//   U (coords.x) = position along beam (0ↁE) + time scroll offset
//   V (coords.y) = position across beam (0 = top edge, 1 = bottom edge, 0.5 = center)
//
// Features:
//   - 2 scrolling noise texture layers for organic flowing energy
//   - V-based edge falloff for soft beam edges
//   - White-hot core effect at beam center
//   - Pulse animation along beam length
//   - Overbright multiplier for HDR bloom
//   - Color mixing driven by noise for organic variation
// =============================================================================

float4x4 uWorldViewProjection;

sampler uImage0 : register(s0); // Primary noise texture (e.g. SoftCircularCaustics)
sampler uImage1 : register(s1); // Secondary noise texture (e.g. TileableFBMNoise)

float3 uColor;           // Primary beam color (TerraBlade green)
float3 uSecondaryColor;  // Secondary beam color (TerraBlade cyan)
float uOpacity;          // Overall opacity (lifecycle fade)
float uTime;             // Animation time (Main.GlobalTimeWrappedHourly)
float uIntensity;        // Base brightness multiplier

float uNoiseSpeed1;      // Primary noise scroll speed (negative = scroll outward)
float uNoiseSpeed2;      // Secondary noise scroll speed
float uNoiseScale1;      // Primary noise UV repetition count
float uNoiseScale2;      // Secondary noise UV repetition count
float uEdgeSoftness;     // Edge fade range (0.1 = sharp, 0.5 = very soft)
float uPulseSpeed;       // Pulse animation rate
float uOverbrightMult;   // HDR bloom multiplier (1.0 = normal, 2-4 = bloom)

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
// PIXEL SHADER  EGradient Flow Beam
// =============================================================================
float4 GradientFlowPS(VS_OUTPUT input) : COLOR0
{
    float2 uv = input.TexCoord;
    float u = uv.x; // Along beam (includes scroll offset from vertex generation)
    float v = uv.y; // Across beam (0 = top edge, 1 = bottom edge)

    // --- Edge Distance & Falloff ---
    // edgeDist: 0 at edges, 1 at center
    float edgeDist = 1.0 - abs(v - 0.5) * 2.0;
    float edgeFade = smoothstep(0.0, uEdgeSoftness, edgeDist);

    // --- Scrolling Noise Layers ---
    // Layer 1: Primary flow (large-scale organic patterns)
    float2 noiseUV1 = float2(u * uNoiseScale1 + uTime * uNoiseSpeed1, v * 0.8 + 0.1);
    float noise1 = tex2D(uImage0, noiseUV1).r;

    // Layer 2: Secondary detail (finer turbulence, counter-scroll)
    float2 noiseUV2 = float2(u * uNoiseScale2 - uTime * uNoiseSpeed2 * 0.7, v * 0.6 + 0.2);
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // --- Color Gradient ---
    // Smooth noise for gentle color blending (no harsh banding)
    float smoothNoise1 = smoothstep(0.15, 0.85, noise1);
    float smoothNoise2 = smoothstep(0.15, 0.85, noise2);
    float colorMix = smoothNoise1 * 0.5 + smoothNoise2 * 0.5;
    // Bias toward center for seamless flowing gradients
    colorMix = 0.25 + colorMix * 0.5;
    float3 baseColor = lerp(uColor, uSecondaryColor, colorMix);

    // --- White-Hot Core ---
    // Blend toward white at the beam center
    float coreFactor = smoothstep(0.25, 0.7, edgeDist);
    float3 coreColor = lerp(baseColor, float3(1.0, 1.0, 1.0), coreFactor * 0.4);

    // --- Pulse Animation ---
    float pulse = sin(uTime * uPulseSpeed + u * 4.0) * 0.08 + 0.92;

    // --- Noise-modulated intensity ---
    // Creates the flowing energy feel  Ebrighter where noise peaks
    float noiseIntensity = 0.65 + noise1 * 0.25 + noise2 * 0.10;

    // --- Final Composition ---
    float3 finalColor = coreColor * uIntensity * pulse * noiseIntensity;
    float alpha = edgeFade * uOpacity * input.Color.a;

    return float4(finalColor * uOverbrightMult * alpha, alpha);
}

// =============================================================================
// TECHNIQUE
// =============================================================================
technique GradientBeamTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 GradientFlowPS();
    }
}
