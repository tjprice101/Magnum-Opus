// =============================================================================
// MagnumOpus Incisor Resonance Trail Shader - PS 2.0 Compatible
// =============================================================================
// Frequency-based resonance trail with standing wave patterns.
// Designed for CalamityStyleTrailRenderer primitive geometry.
//
// Unique to Incisor of Moonlight  E"The Stellar Scalpel":
//   - Standing wave pattern (tuning-fork resonance visualization)
//   - Constellation node bright spots at regular intervals
//   - Frequency escalation with combo step (via uIntensity)
//   - Surgical precision feel: sharp center, clean edges
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   IncisorResonanceTrail  EMain trail with standing wave + constellation nodes
//   IncisorResonanceGlow   EWider glow pass for bloom stacking
// =============================================================================

sampler uImage0 : register(s0); // Base trail texture
sampler uImage1 : register(s1); // Noise texture

float3 uColor;           // Primary color (e.g. ResonantSilver)
float3 uSecondaryColor;  // Secondary color (e.g. FrequencyPulse)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness / combo resonance level (0.3 - 1.0)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Wave scroll rate
float uNoiseScale;        // Noise UV repetition
float uDistortionAmt;     // Resonance wave amplitude
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;

// =============================================================================
// UTILITY
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Standing wave: creates nodes (bright spots) at regular intervals
float StandingWave(float u, float frequency, float time)
{
    // Node pattern  Ebright at antinode positions, dim at nodes
    float wave = abs(sin(u * 3.14159 * frequency + time * 0.5));
    return wave;
}

// Constellation node brightness  Esharp peaks at regular positions
float ConstellationNode(float u, int nodeCount)
{
    float spacing = 1.0 / (float)(nodeCount + 1);
    float minDist = 1.0;
    for (int i = 1; i <= nodeCount; i++)
    {
        float nodePos = spacing * (float)i;
        float dist = abs(u - nodePos);
        minDist = min(minDist, dist);
    }
    // Sharp Gaussian peak at each node
    return exp(-minDist * minDist * 800.0);
}

// =============================================================================
// TECHNIQUE 1: INCISOR RESONANCE TRAIL
// =============================================================================

float4 IncisorResonancePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Standing wave distortion (tuning-fork resonance) ---
    float waveFreq = 4.0 + uIntensity * 4.0; // 4-8 waves depending on combo
    float standingWaveVal = StandingWave(coords.x, waveFreq, uTime * uScrollSpeed * 3.0);

    // Perpendicular displacement from standing wave
    float resonanceDisp = sin(coords.x * waveFreq * 6.28 + uTime * uScrollSpeed * 4.0)
                         * uDistortionAmt * standingWaveVal;

    float2 distortedUV = coords;
    distortedUV.y += resonanceDisp;

    // High-frequency shimmer overlay
    float shimmerDisp = sin(coords.x * 25.0 - uTime * 8.0) * uDistortionAmt * 0.2;
    distortedUV.x += shimmerDisp;

    // Sample base texture
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Noise modulation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    noiseUV.y += sin(uTime * 0.3) * 0.1;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.8, noiseTex.r, uHasSecondaryTex);

    // --- Edge fade (sharper than MoonlightTrail  Eprecision feel) ---
    float edgeFade = QuadraticBump(coords.y);
    float sharpEdge = pow(edgeFade, 1.5); // Sharper falloff for surgical precision

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.1);

    // --- Palette gradient with standing wave coloring ---
    float gradientT = coords.x * 0.7 + standingWaveVal * 0.3;
    float3 trailColor = lerp(uColor, uSecondaryColor, gradientT);

    // --- Constellation node highlights ---
    int nodeCount = 3 + (int)(uIntensity * 4.0); // 3-7 nodes based on resonance
    float nodeBrightness = ConstellationNode(coords.x, nodeCount);
    trailColor = lerp(trailColor, float3(1.0, 0.98, 1.0), nodeBrightness * 0.7);

    // --- White-hot core (sharper than moonlight) ---
    float coreMask = saturate((sharpEdge - 0.6) * 4.0);
    trailColor = lerp(trailColor, float3(1.0, 0.97, 1.0), coreMask * 0.6);

    // --- Standing wave brightness modulation ---
    float waveGlow = 0.7 + standingWaveVal * 0.3 * uIntensity;

    // --- Resonance pulse overlay ---
    float resonancePulse = sin(uTime * 6.0 + coords.x * 8.0) * 0.08 * uIntensity + 0.92;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * waveGlow * resonancePulse;
    finalColor *= 0.75 + noiseVal * 0.25;

    float alpha = sharpEdge * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: INCISOR RESONANCE GLOW
// =============================================================================

float4 IncisorResonanceGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Gentler wave for glow pass
    float wave = sin(coords.x * 4.0 + uTime * uScrollSpeed * 2.0) * uDistortionAmt * 0.3;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    // Wider, softer edge fade
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    float trailFade = saturate(1.0 - coords.x * 0.85);

    // Color: biased toward primary with constellation highlights
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.4);

    // Constellation nodes in glow pass too (subtler)
    int nodeCount = 3 + (int)(uIntensity * 4.0);
    float nodeBrightness = ConstellationNode(coords.x, nodeCount);
    glowColor = lerp(glowColor, float3(1.0, 0.98, 1.0), nodeBrightness * 0.3);

    // Noise
    float2 noiseUV = coords * uSecondaryTexScale * 0.6;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.4;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // Gentle pulse
    float pulse = sin(uTime * 3.0 + coords.x * 4.0) * 0.06 + 0.94;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique IncisorResonanceTrail
{
    pass P0
    {
        PixelShader = compile ps_3_0 IncisorResonancePS();
    }
}

technique IncisorResonanceGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 IncisorResonanceGlowPS();
    }
}
