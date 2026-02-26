// =============================================================================
// Prayer Convergence Shader - PS 2.0 Compatible
// =============================================================================
// "Prayer Answered" convergence pulse for Funeral Prayer when all 5 beams
// connect. 5-pointed star radial burst expanding outward, golden energy
// lines converging to center, hexagonal halo ring.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1), centre = 0.5
//   V (coords.y) = vertical position (0-1), centre = 0.5
//
// Techniques:
//   ConvergenceMain  - 5-pointed star burst with convergence lines
//   ConvergenceGlow  - Soft radial bloom overlay
//
// Features:
//   - 5-fold symmetry star pattern via polar coordinates
//   - Phase-driven expansion (0 = explosion start, 1 = fully expanded)
//   - Golden convergence lines radiating from centre
//   - Hexagonal halo ring at expansion edge
//   - Opacity fades with expansion for natural dissolve
//   - Overbright for dramatic HDR bloom
// =============================================================================

sampler uImage0 : register(s0); // Base texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;            // Primary color (Gold)
float3 uSecondaryColor;   // Secondary color (Scarlet)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;             // Expansion phase (0 = burst, 1 = fully expanded)
float uScrollSpeed;        // Pattern rotation speed
float uPointCount;         // Star points (default 5)
float uHasSecondaryTex;

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: CONVERGENCE MAIN
// =============================================================================

float4 ConvergenceMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- 5-pointed star envelope ---
    float starAngle = angle + uTime * uScrollSpeed;
    float starShape = cos(uPointCount * starAngle);
    float starMask = saturate(starShape * 0.4 + 0.6); // 5-pointed brightness modulation

    // --- Expanding ring at explosion edge ---
    float ringRadius = uPhase * 0.8 + 0.1; // Expands with phase
    float ringDist = abs(dist - ringRadius);
    float ringMask = saturate(1.0 - ringDist * 10.0);

    // Hexagonal faceting on ring
    float hexFacet = cos(angle * 3.0) * 0.08 + 0.92;
    ringMask *= hexFacet;

    // --- Convergence lines (energy rays from edge to centre) ---
    float angleQuantized = frac(starAngle / 6.28318 * uPointCount);
    float rayDist = abs(angleQuantized * 2.0 - 1.0);
    float rayMask = saturate(1.0 - rayDist * 8.0);
    rayMask *= saturate(dist * 3.0); // No rays at very centre
    rayMask *= saturate(1.0 - dist * 1.3); // Fade at edges
    rayMask *= (1.0 - uPhase); // Rays dissipate during expansion

    // --- Centre flash (bright at burst start, dims during expansion) ---
    float centreMask = saturate(1.0 - dist * 4.0);
    centreMask *= saturate(1.0 - uPhase * 1.5); // Fades quickly

    // --- Colour gradient: Gold centre ↁEScarlet ring ---
    float3 burstColor = lerp(uColor, uSecondaryColor, dist * 0.8);

    // White-hot centre flash
    float3 whiteHot = float3(1.0, 0.95, 0.85);
    burstColor = lerp(burstColor, whiteHot, centreMask * 0.7);

    // Ring colour (golden)
    float3 ringColor = lerp(uColor, whiteHot, 0.3);

    // --- Noise modulation ---
    float noise = HashNoise(coords * 5.0 + float2(uTime * 0.2, 0.0));

    // --- Phase-driven opacity (fades as it expands) ---
    float phaseOpacity = saturate(1.0 - uPhase * 0.7);

    // --- Pulse ---
    float pulse = sin(uTime * 6.0 + dist * 8.0) * 0.06 + 0.94;

    // --- Final composition ---
    float3 finalColor = burstColor * starMask * uIntensity * pulse;
    finalColor += ringColor * ringMask * uIntensity * 0.4;
    finalColor *= 0.7 + noise * 0.3;

    float alpha = (centreMask + ringMask * 0.5 + rayMask * 0.3) *
        phaseOpacity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: CONVERGENCE GLOW
// =============================================================================

float4 ConvergenceGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial glow
    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    // Expanding ring glow
    float ringRadius = uPhase * 0.8 + 0.1;
    float ringDist = abs(dist - ringRadius);
    float ringGlow = saturate(1.0 - ringDist * 4.0) * 0.4;

    // Warm golden glow
    float3 glowColor = lerp(uColor, float3(1.0, 0.90, 0.70), 0.3);
    glowColor *= uIntensity * baseTex.rgb;

    float phaseOpacity = saturate(1.0 - uPhase * 0.6);

    float pulse = sin(uTime * 4.0) * 0.08 + 0.92;

    float alpha = (radial + ringGlow) * phaseOpacity * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique ConvergenceMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConvergenceMainPS();
    }
}

technique ConvergenceGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConvergenceGlowPS();
    }
}
