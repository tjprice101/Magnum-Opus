// =============================================================================
// Sakura Swing Trail Shader - Cherry Blossom Katana
// =============================================================================
// Flowing cherry blossom energy trail with embedded petal silhouettes,
// wind-carried drift animation, and chromatic iridescence at edges.
//
// VISUAL IDENTITY: Like a katana cutting through a sakura storm -- petals
// visibly swirl within the energy trail, wind shears create chromatic
// rainbow edges, the trail breathes and pulses like falling blossoms.
//
// Techniques:
//   SakuraTrailFlow   - Main trail with petal silhouettes & wind drift
//   SakuraTrailGlow   - Wider bloom with chromatic edge iridescence
// =============================================================================

sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Noise texture (PerlinNoise)

float3 uColor;            // Primary trail color (BloomPink)
float3 uSecondaryColor;   // Secondary trail color (PollenGold)
float  uOpacity;
float  uTime;
float  uIntensity;
float  uOverbrightMult;

float  uScrollSpeed;      // UV scroll speed along trail
float  uNoiseScale;       // Noise distortion intensity
float  uDistortionAmt;    // Perpendicular distortion amount
float  uPhase;            // Combo phase [0-3] for color shifting

// =============================================================================
// Utility Functions
// =============================================================================

float4 ApplyOverbright(float4 color)
{
    return color * uOverbrightMult;
}

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// Smooth value noise
float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// =============================================================================
// SakuraTrailFlow - Cherry Blossom Petal Trail
// =============================================================================

float4 SakuraFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float edgeFade = QuadraticBump(coords.y);
    float trailFade = pow(1.0 - coords.x, 1.8);

    // --- Wind drift: petals don't travel straight, they swirl ---
    float windSway = sin(coords.x * 6.0 + uTime * 2.5) * 0.03;
    float windGust = sin(coords.x * 14.0 - uTime * 5.0) * 0.015 * (1.0 + uPhase * 0.3);

    // Sample noise for organic distortion
    float2 noiseUV = float2(coords.x * 2.0 - uTime * uScrollSpeed * 0.6, coords.y * 1.5);
    float4 noiseSample = tex2D(uImage1, noiseUV);
    float noiseVal = noiseSample.r;

    // Wind-distorted UVs
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + windSway + windGust);
    scrolledUV.y += (noiseVal - 0.5) * uDistortionAmt * edgeFade;

    float4 trailSample = tex2D(uImage0, scrolledUV);

    // --- Embedded petal silhouettes (visible 5-petal shapes within trail) ---
    // Use two octaves of polar-cosine to create petal impressions
    float2 petalUV1 = coords * float2(8.0, 4.0) + float2(-uTime * 1.5, uTime * 0.8);
    float petalAngle1 = atan2(frac(petalUV1.y) - 0.5, frac(petalUV1.x) - 0.5);
    float petalDist1 = length(frac(petalUV1) - 0.5) * 4.0;
    float petal1 = cos(petalAngle1 * 5.0) * 0.4 + 0.6; // 5-petal shape
    float petalMask1 = saturate(1.0 - petalDist1 * 1.5) * petal1;

    float2 petalUV2 = coords * float2(5.0, 3.0) + float2(-uTime * 2.2, -uTime * 0.5);
    float petalAngle2 = atan2(frac(petalUV2.y) - 0.5, frac(petalUV2.x) - 0.5);
    float petalDist2 = length(frac(petalUV2) - 0.5) * 4.0;
    float petal2 = cos(petalAngle2 * 5.0 + 0.6) * 0.4 + 0.6;
    float petalMask2 = saturate(1.0 - petalDist2 * 1.5) * petal2;

    float petalComposite = saturate(petalMask1 * 0.6 + petalMask2 * 0.4);
    // Petals more visible at higher combo phases
    float petalVisibility = 0.15 + uPhase * 0.08;

    // --- Color gradient with petal-influenced shifting ---
    float colorT = coords.x * 0.55 + noiseVal * 0.25 + petalComposite * 0.2;
    float3 trailColor = lerp(uColor, uSecondaryColor, colorT);

    // White-hot core at trail center
    float coreMask = pow(edgeFade, 2.0) * trailFade;
    float3 coreWhite = float3(1.0, 0.98, 0.95);
    trailColor = lerp(trailColor, coreWhite, coreMask * 0.4);

    // Petal impressions add a softer sakura-white tint
    float3 petalWhite = float3(1.0, 0.88, 0.92);
    trailColor = lerp(trailColor, petalWhite, petalComposite * petalVisibility);

    // --- Combo phase color escalation ---
    // Higher phases shift toward more intense crimson and gold
    float phaseShift = uPhase * 0.08;
    float3 phaseColor = lerp(float3(0.9, 0.3, 0.4), float3(1.0, 0.85, 0.3), uPhase * 0.33);
    trailColor = lerp(trailColor, phaseColor, phaseShift * coreMask);

    // --- Dual-frequency shimmer ---
    float petalWave1 = sin(coords.x * 12.0 - uTime * 3.0 + noiseVal * 6.0);
    float petalWave2 = cos(coords.x * 8.0 + uTime * 2.0 + noiseVal * 4.0);
    float petalShimmer = saturate(petalWave1 * 0.3 + petalWave2 * 0.2 + 0.5);

    // Final composite
    float alpha = trailSample.a * edgeFade * trailFade * uOpacity * petalShimmer;
    float4 finalColor = float4(trailColor * uIntensity, 0.0) * alpha;

    return ApplyOverbright(finalColor) * sampleColor;
}

// =============================================================================
// SakuraTrailGlow - Chromatic Iridescence Bloom
// =============================================================================

float4 SakuraGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6);
    float trailFade = pow(1.0 - coords.x, 1.2);

    // Slow noise scroll for dreamy glow movement
    float2 noiseUV = float2(coords.x * 1.5 - uTime * uScrollSpeed * 0.3, coords.y);
    float4 noiseSample = tex2D(uImage1, noiseUV);
    float noiseVal = noiseSample.r;

    // --- Chromatic iridescence at trail edges ---
    // At the edges (where edgeFade is low), split the color into 
    // subtly shifted hues to create rainbow shimmer
    float edgeDist = abs(coords.y - 0.5) * 2.0; // 0 at center, 1 at edge
    float iridescentMask = saturate((edgeDist - 0.4) * 3.5); // Only at outer edges

    // Three color channels shift at different rates for chromatic effect
    float timeOsc = uTime * 1.5 + coords.x * 4.0;
    float3 iridescentColor;
    iridescentColor.r = sin(timeOsc) * 0.5 + 0.5;
    iridescentColor.g = sin(timeOsc + 2.09) * 0.5 + 0.5;
    iridescentColor.b = sin(timeOsc + 4.19) * 0.5 + 0.5;
    // Keep it warm-tinted (bias toward sakura/gold range)
    iridescentColor = lerp(iridescentColor, float3(1.0, 0.7, 0.8), 0.5);

    // Soft color base
    float3 glowColor = lerp(uColor, float3(1.0, 0.95, 0.92), 0.35);
    glowColor = lerp(glowColor, uSecondaryColor, coords.x * 0.4);

    // Blend iridescence at edges
    glowColor = lerp(glowColor, iridescentColor, iridescentMask * 0.35);

    // Breathing pulse
    float pulse = 0.85 + 0.15 * sin(uTime * 2.0 + coords.x * 4.0);

    // Final bloom composite
    float alpha = edgeFade * trailFade * uOpacity * 0.35 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.7, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =============================================================================
// Techniques
// =============================================================================

technique SakuraTrailFlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SakuraFlowPS();
    }
}

technique SakuraTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SakuraGlowPS();
    }
}
