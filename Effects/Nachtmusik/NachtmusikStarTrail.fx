// =============================================================================
// MagnumOpus Nachtmusik Star Trail Shader - PS 3.0
// =============================================================================
// Twinkling stellar trail with constellation-point shimmer and cosmic drift.
// Designed for CalamityStyleTrailRenderer primitive geometry.
//
// "Eine kleine Nachtmusik" — A Little Night Music
// The night sky itself flows behind each swing and projectile.
// Stars twinkle along the trail, constellations form and dissolve,
// and the cosmic void breathes with nocturnal wonder.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   NachtmusikStarFlow  – Main stellar trail with twinkling star points
//   NachtmusikStarGlow  – Softer additive bloom overlay for glow stacking
//
// Features:
//   - Procedural twinkling star points along trail using noise
//   - Cosmic drift distortion (slow, majestic, like drifting through space)
//   - QuadraticBump edge fade with starlit core
//   - Deep indigo → silver → starlight gradient
//   - Constellation point highlights (bright pinpoints)
//   - Overbright multiplier for HDR bloom
//   - Multi-pass rendering support (3 passes for full 6-color gradient)
// =============================================================================

sampler uImage0 : register(s0); // Base trail texture
sampler uImage1 : register(s1); // Noise texture (StarFieldScatter recommended)

float3 uColor;           // Primary color (deep indigo)
float3 uSecondaryColor;  // Secondary color (starlight silver)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Cosmic drift rate
float uNoiseScale;        // Noise UV repetition
float uDistortionAmt;     // Cosmic drift distortion strength
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed

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

// Procedural twinkling: bright points that appear and fade rhythmically
float TwinkleField(float2 uv, float time)
{
    // Hash-like function for pseudo-random star placement
    float2 cell = floor(uv * 8.0);
    float hash = frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
    
    // Each star twinkles at its own phase
    float twinklePhase = hash * 6.28 + time * (2.0 + hash * 3.0);
    float twinkle = sin(twinklePhase) * 0.5 + 0.5;
    twinkle = twinkle * twinkle * twinkle; // Sharpen to brief bright flashes
    
    // Only some cells have visible stars
    float starMask = step(0.7, hash);
    
    return twinkle * starMask;
}

// =============================================================================
// TECHNIQUE 1: NACHTMUSIK STAR FLOW
// =============================================================================

float4 NachtmusikStarFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Cosmic drift distortion (slow, majestic) ---
    float drift1 = sin(coords.x * 4.0 + uTime * uScrollSpeed * 1.5) * uDistortionAmt;
    float drift2 = sin(coords.x * 8.0 - uTime * uScrollSpeed * 0.8 + coords.y * 3.0) * uDistortionAmt * 0.5;
    // Slow breathing wave (nocturnal rhythm)
    float drift3 = sin(coords.x * 2.0 + uTime * uScrollSpeed * 0.4) * uDistortionAmt * 0.3;

    float2 distortedUV = coords;
    distortedUV.y += drift1 + drift3;
    distortedUV.x += drift2;

    // Sample base texture
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Noise texture for cosmic nebula variation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    noiseUV.y += uTime * 0.1;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.7, noiseTex.r, uHasSecondaryTex);

    // --- Edge-to-center fade ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.05);

    // --- Stellar gradient along trail ---
    float gradientT = coords.x * 0.75 + noiseVal * 0.25;
    float3 trailColor = lerp(uColor, uSecondaryColor, gradientT);

    // --- White-hot starlit core ---
    float coreMask = saturate((edgeFade - 0.55) * 3.5);
    trailColor = lerp(trailColor, float3(0.95, 0.93, 1.0), coreMask * 0.5);

    // --- Twinkling constellation points ---
    float twinkle = TwinkleField(distortedUV * 3.0 + float2(uTime * 0.3, 0), uTime);
    // Stars are brighter where noise is high (constellation clustering)
    twinkle *= noiseVal;
    float3 starHighlight = float3(0.98, 0.96, 1.0); // Pure starlight
    trailColor = lerp(trailColor, starHighlight, twinkle * 0.6);

    // --- Cosmic drift shimmer ---
    float shimmer = saturate(drift1 * 6.0 + 0.5) * 0.12 + 0.88;

    // --- Nocturnal pulse (slow, like breathing under the stars) ---
    float pulse = sin(uTime * 2.5 + coords.x * 3.0) * 0.08 + 0.92;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * shimmer * pulse;
    finalColor *= 0.65 + noiseVal * 0.35;

    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: NACHTMUSIK STAR GLOW
// =============================================================================

float4 NachtmusikStarGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.x * 3.0 + uTime * uScrollSpeed * 1.2) * uDistortionAmt * 0.4;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    float trailFade = saturate(1.0 - coords.x * 0.9);

    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.45);

    // Add deep indigo tint to the glow (nocturnal sky)
    float3 nightTint = float3(0.15, 0.1, 0.35);
    glowColor = lerp(glowColor, nightTint, 0.1);

    float2 noiseUV = coords * uSecondaryTexScale * 0.6;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.5;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    float pulse = sin(uTime * 2.0 + coords.x * 2.5) * 0.07 + 0.93;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique NachtmusikStarFlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 NachtmusikStarFlowPS();
    }
}

technique NachtmusikStarGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 NachtmusikStarGlowPS();
    }
}
