// =============================================================================
// Crescendo Charge Shader - PS 2.0 Compatible
// =============================================================================
// Charge orbit indicator for Piercing Light of the Sakura's 10-shot crescendo
// mechanic. 9 small sakura-fire orbs orbit the weapon tip in a circle, each
// orb "fills in" as the shot counter increases (0/9 to 9/9). All converge
// inward on the 10th (crescendo) shot.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1), centre = 0.5
//   V (coords.y) = vertical position (0-1), centre = 0.5
//
// Techniques:
//   CrescendoChargeMain  - Orbiting charge orbs with fill-in animation
//   CrescendoChargeGlow  - Soft radial bloom for orbit glow
//
// Features:
//   - 9 procedural orbs positioned via polar coordinates
//   - Each orb fills based on uChargeProgress (0-1)
//   - Converging animation at full charge (all rush to centre)
//   - Sakura pink ↁEscarlet ↁEgold colour escalation
//   - Spinning orbit with uTime-driven rotation
//   - Overbright for HDR bloom punch
// =============================================================================

sampler uImage0 : register(s0); // Base texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;             // Primary color (Sakura pink)
float3 uSecondaryColor;    // Secondary color (Gold)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uChargeProgress;     // 0 = empty, 1 = fully charged (9/9)
float uOrbCount;           // Number of orbs (default 9)
float uScrollSpeed;         // Orbit rotation speed
float uPhase;              // Convergence phase (ramps 0ↁE during crescendo)
float uHasSecondaryTex;

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: CRESCENDO CHARGE MAIN
// =============================================================================

float4 CrescendoChargeMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    float orbCount = max(uOrbCount, 1.0);

    // --- Orbit radius (shrinks during convergence) ---
    float baseRadius = 0.55;
    float convergeRadius = lerp(baseRadius, 0.0, uPhase * uPhase);

    // --- Orbit rotation ---
    float rotation = uTime * uScrollSpeed * 2.0;

    // --- Sum contribution from all orbs ---
    float totalOrbMask = 0.0;
    float totalFillMask = 0.0;
    float3 orbColorAccum = float3(0.0, 0.0, 0.0);

    // PS 2.0: unrolled loop (max 9 iterations)
    float orbAngleStep = 6.28318 / orbCount;

    // Orb 0
    float orbAngle0 = 0.0 * orbAngleStep + rotation;
    float2 orbPos0 = float2(cos(orbAngle0), sin(orbAngle0)) * convergeRadius * 0.5;
    float orbDist0 = length(centred - orbPos0) * 2.0;
    float orbMask0 = saturate(1.0 - orbDist0 * 8.0);
    float fill0 = step(0.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask0;
    totalFillMask += orbMask0 * fill0;

    // Orb 1
    float orbAngle1 = 1.0 * orbAngleStep + rotation;
    float2 orbPos1 = float2(cos(orbAngle1), sin(orbAngle1)) * convergeRadius * 0.5;
    float orbDist1 = length(centred - orbPos1) * 2.0;
    float orbMask1 = saturate(1.0 - orbDist1 * 8.0);
    float fill1 = step(1.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask1;
    totalFillMask += orbMask1 * fill1;

    // Orb 2
    float orbAngle2 = 2.0 * orbAngleStep + rotation;
    float2 orbPos2 = float2(cos(orbAngle2), sin(orbAngle2)) * convergeRadius * 0.5;
    float orbDist2 = length(centred - orbPos2) * 2.0;
    float orbMask2 = saturate(1.0 - orbDist2 * 8.0);
    float fill2 = step(2.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask2;
    totalFillMask += orbMask2 * fill2;

    // Orb 3
    float orbAngle3 = 3.0 * orbAngleStep + rotation;
    float2 orbPos3 = float2(cos(orbAngle3), sin(orbAngle3)) * convergeRadius * 0.5;
    float orbDist3 = length(centred - orbPos3) * 2.0;
    float orbMask3 = saturate(1.0 - orbDist3 * 8.0);
    float fill3 = step(3.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask3;
    totalFillMask += orbMask3 * fill3;

    // Orb 4
    float orbAngle4 = 4.0 * orbAngleStep + rotation;
    float2 orbPos4 = float2(cos(orbAngle4), sin(orbAngle4)) * convergeRadius * 0.5;
    float orbDist4 = length(centred - orbPos4) * 2.0;
    float orbMask4 = saturate(1.0 - orbDist4 * 8.0);
    float fill4 = step(4.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask4;
    totalFillMask += orbMask4 * fill4;

    // Orb 5
    float orbAngle5 = 5.0 * orbAngleStep + rotation;
    float2 orbPos5 = float2(cos(orbAngle5), sin(orbAngle5)) * convergeRadius * 0.5;
    float orbDist5 = length(centred - orbPos5) * 2.0;
    float orbMask5 = saturate(1.0 - orbDist5 * 8.0);
    float fill5 = step(5.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask5;
    totalFillMask += orbMask5 * fill5;

    // Orb 6
    float orbAngle6 = 6.0 * orbAngleStep + rotation;
    float2 orbPos6 = float2(cos(orbAngle6), sin(orbAngle6)) * convergeRadius * 0.5;
    float orbDist6 = length(centred - orbPos6) * 2.0;
    float orbMask6 = saturate(1.0 - orbDist6 * 8.0);
    float fill6 = step(6.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask6;
    totalFillMask += orbMask6 * fill6;

    // Orb 7
    float orbAngle7 = 7.0 * orbAngleStep + rotation;
    float2 orbPos7 = float2(cos(orbAngle7), sin(orbAngle7)) * convergeRadius * 0.5;
    float orbDist7 = length(centred - orbPos7) * 2.0;
    float orbMask7 = saturate(1.0 - orbDist7 * 8.0);
    float fill7 = step(7.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask7;
    totalFillMask += orbMask7 * fill7;

    // Orb 8
    float orbAngle8 = 8.0 * orbAngleStep + rotation;
    float2 orbPos8 = float2(cos(orbAngle8), sin(orbAngle8)) * convergeRadius * 0.5;
    float orbDist8 = length(centred - orbPos8) * 2.0;
    float orbMask8 = saturate(1.0 - orbDist8 * 8.0);
    float fill8 = step(8.0 / orbCount, uChargeProgress);
    totalOrbMask += orbMask8;
    totalFillMask += orbMask8 * fill8;

    totalOrbMask = saturate(totalOrbMask);
    totalFillMask = saturate(totalFillMask);

    // --- Colour: filled orbs are bright, unfilled are dim outlines ---
    float3 filledColor = lerp(uColor, uSecondaryColor, uChargeProgress);
    float3 hotCore = float3(1.0, 0.96, 0.90);
    filledColor = lerp(filledColor, hotCore, uChargeProgress * 0.3);

    float3 dimColor = uColor * 0.25;

    float3 orbColor = lerp(dimColor, filledColor, totalFillMask / max(totalOrbMask, 0.001));

    // --- Centre convergence flash at full charge ---
    float centreMask = saturate(1.0 - dist * 4.0) * uPhase * uPhase;
    orbColor = lerp(orbColor, hotCore, centreMask);

    // --- Pulse on filled orbs ---
    float pulse = sin(uTime * 5.0 + uChargeProgress * 10.0) * 0.08 + 0.92;

    // --- Final composition ---
    float3 finalColor = orbColor * baseTex.rgb * uIntensity * pulse;

    float alpha = (totalOrbMask * 0.8 + centreMask * 0.4) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: CRESCENDO CHARGE GLOW
// =============================================================================

float4 CrescendoChargeGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial glow
    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    // Charge-reactive glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, uChargeProgress * 0.6);
    float3 warmPink = float3(1.0, 0.75, 0.82);
    glowColor = lerp(glowColor, warmPink, 0.2);

    glowColor *= uIntensity * baseTex.rgb;

    // Charge-scaled brightness
    float chargeBright = 0.2 + uChargeProgress * 0.8;

    // Convergence flash
    float convergeMask = saturate(1.0 - dist * 3.0) * uPhase * uPhase;
    glowColor += float3(1.0, 0.95, 0.88) * convergeMask * 0.5;

    float pulse = sin(uTime * 3.0) * 0.1 + 0.9;

    float alpha = (radial * chargeBright + convergeMask * 0.3) * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique CrescendoChargeMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescendoChargeMainPS();
    }
}

technique CrescendoChargeGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescendoChargeGlowPS();
    }
}
