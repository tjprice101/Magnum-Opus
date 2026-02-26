// =============================================================================
// Sakura Bloom Shader - PS 2.0 Compatible
// =============================================================================
// Procedural sakura petal bloom overlay with wilt/bloom animation.
// Renders soft petal shapes from UV coordinates with radial glow.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1)
//   V (coords.y) = vertical position (0-1)
//   Centre (0.5, 0.5) = bloom core
//
// Techniques:
//   SakuraPetalBloom  - Main petal bloom with procedural petal shapes
//   SakuraGlowPass    - Soft radial glow for bloom stacking
//
// Features:
//   - Procedural 5-petal flower shape via polar coordinates
//   - Phase parameter: 0=bud, 0.5=full bloom, 1=petal scatter
//   - Soft inner glow with sakura-to-gold gradient
//   - Spinning petal animation synced to time
//   - Noise modulation for organic variation
// =============================================================================

sampler uImage0 : register(s0); // Base texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;           // Primary color (e.g. Sakura pink)
float3 uSecondaryColor;  // Secondary color (e.g. Pollen gold)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uPhase;             // Bloom phase: 0=bud, 0.5=full bloom, 1=scatter
float uPetalCount;        // Number of petals (default 5.0)
float uRotationSpeed;     // Petal rotation rate (default 0.5)
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed
float uDistortionAmt;     // Petal edge softness (default 0.1)

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: SAKURA PETAL BLOOM
// =============================================================================
// Procedural 5-petal flower shape rendered via polar coordinates.
// The petal count creates a flower-like pattern using cosine in polar space.
// Phase controls bloom state: bud (tight, dim), full bloom (wide, bright),
// scatter (petals drift outward, fading).
// =============================================================================

float4 SakuraPetalBloomPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Centre-relative coordinates
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0; // 0 at centre, 1 at edge
    float angle = atan2(centred.y, centred.x);

    // --- Petal rotation ---
    float rotation = uTime * uRotationSpeed;
    float rotatedAngle = angle + rotation;

    // --- Petal shape via polar cosine ---
    // cos(petalCount * angle) creates petal-like lobes
    float petalShape = cos(uPetalCount * rotatedAngle);

    // --- Phase-dependent petal radius ---
    // Bud (0): petals tight (small radius, dim)
    // Full bloom (0.5): petals wide open (large radius, bright)
    // Scatter (1.0): petals drift outward (very large radius, fading)
    float bloomRadius = lerp(0.15, 0.5, saturate(uPhase * 2.0)); // 0ↁE.5 phase
    float scatterExpand = saturate((uPhase - 0.5) * 2.0) * 0.4;  // 0.5ↁE.0 phase
    bloomRadius += scatterExpand;

    // Petal boundary: smooth falloff from petal centre to petal edge
    float petalDist = dist - (petalShape * 0.5 + 0.5) * bloomRadius;
    float petalMask = saturate(1.0 - petalDist * (4.0 - uDistortionAmt * 20.0));

    // --- Inner glow: radial falloff ---
    float innerGlow = saturate(1.0 - dist * 1.8) * 0.6;
    innerGlow *= saturate(uPhase * 4.0); // Only visible once blooming starts

    // --- Color gradient: sakura centre ↁEgold edges ---
    float colourT = saturate(dist * 1.5);
    float3 petalColor = lerp(uColor, uSecondaryColor, colourT * 0.6);

    // White-hot core at very centre
    float coreMask = saturate((1.0 - dist * 3.0));
    petalColor = lerp(petalColor, float3(1.0, 0.96, 0.92), coreMask * 0.5);

    // --- Phase brightness ---
    // Bud: dim, Full bloom: max, Scatter: fading
    float phaseBright = 1.0;
    if (uPhase < 0.5)
        phaseBright = 0.3 + uPhase * 1.4; // 0.3ↁE.0
    else
        phaseBright = 1.0 - (uPhase - 0.5) * 1.2; // 1.0ↁE.4
    phaseBright = saturate(phaseBright);

    // --- Noise modulation for organic variation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += float2(uTime * uSecondaryTexScroll * 0.3, uTime * 0.15);
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.4);

    // --- Gentle petal shimmer ---
    float shimmer = sin(uTime * 3.0 + dist * 8.0) * 0.08 + 0.92;

    // --- Final composition ---
    float combinedMask = saturate(petalMask + innerGlow);
    float3 finalColor = petalColor * baseTex.rgb * uIntensity * noiseVal * shimmer;

    float alpha = combinedMask * phaseBright * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: SAKURA GLOW PASS
// =============================================================================
// Pure radial soft glow for bloom stacking behind the petal layer.
// Simple, soft, warm  Ecreates the sakura "aura" around the flower.
// =============================================================================

float4 SakuraGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Radial distance from centre
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial falloff
    float radial = saturate(1.0 - dist * dist);
    radial *= radial; // Extra soft

    // Warm sakura glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 0.3);

    // Add warm pink tint
    float3 warmPink = float3(1.0, 0.75, 0.80);
    glowColor = lerp(glowColor, warmPink, 0.2);

    glowColor *= uIntensity * baseTex.rgb;

    // Phase brightness (same as main pass)
    float phaseBright = 1.0;
    if (uPhase < 0.5)
        phaseBright = 0.3 + uPhase * 1.4;
    else
        phaseBright = 1.0 - (uPhase - 0.5) * 1.2;
    phaseBright = saturate(phaseBright);

    // Slow pulse
    float pulse = sin(uTime * 2.0) * 0.1 + 0.9;

    float alpha = radial * phaseBright * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique SakuraPetalBloom
{
    pass P0
    {
        PixelShader = compile ps_3_0 SakuraPetalBloomPS();
    }
}

technique SakuraGlowPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 SakuraGlowPS();
    }
}
