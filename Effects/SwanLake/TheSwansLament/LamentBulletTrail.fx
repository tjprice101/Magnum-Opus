// =====================================================================
//  LamentBulletTrail.fx — The Swan's Lament sorrowful bullet trail
// =====================================================================
//
//  Purpose: Deliberately muted, narrow monochrome trail for shotgun
//  bullets. Intentionally dim to create maximum contrast with the
//  prismatic destruction revelation on hit.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (bullet tip), 1 = tail (oldest)
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    LamentTrailMain  — Razor-narrow dark tracer with silver whispers
//    LamentTrailGlow  — Faint sorrowful bloom underlay
//
//  C# rendering order (3 passes):
//    1. LamentTrailGlow  @ 3x width   (barely-visible underlay)
//    2. LamentTrailMain  @ 1x width   (sharp muted core)
//    3. LamentTrailGlow  @ 1.5x width (subtle overbright whisper)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture (ThinGlowLine)
sampler uImage1 : register(s1); // Noise texture (SoftCircularCaustics)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (SwanDarkGray)
float3 uSecondaryColor;   // Secondary trail color (FeatherWhite)
float  uOpacity;          // Overall opacity
float  uTime;             // Scrolling time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Lament-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Width trembling amount
float  uPhase;            // Unused (reserved for future sorrow variation)
float  uHasSecondaryTex;  // 1.0 if noise texture bound
float  uSecondaryTexScale; // Noise repetition
float  uSecondaryTexScroll; // Noise scroll speed

// =====================================================================
//  Utility
// =====================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// Silver whisper highlights — faint, irregular sine pulses
float SilverWhisper(float u, float time)
{
    float w1 = sin(u * 17.0 - time * 1.3) * 0.5 + 0.5;
    float w2 = sin(u * 7.3 + time * 0.7) * 0.5 + 0.5;
    float combined = w1 * w2;
    // Only show whisper where combined is high — makes them sparse
    return smoothstep(0.6, 0.85, combined) * 0.15;
}

// Sorrow trembling — subtle width variation via noise
float SorrowTremble(float2 coords, float time)
{
    float2 noiseUV = float2(coords.x * 3.0 - time * uScrollSpeed * 0.4, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    return (noise - 0.5) * uDistortionAmt;
}

// =====================================================================
//  LamentTrailMain — Muted razor-narrow dark tracer
// =====================================================================

float4 LamentMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Razor-narrow core — deliberately thin for sorrowful feel
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 3.5);

    // Very fast trail fade — bullets leave only brief impressions
    float trailFade = saturate(1.0 - coords.x * 1.5);
    trailFade = pow(trailFade, 1.5);

    // Subtle width trembling from noise
    float tremble = 0.0;
    if (uHasSecondaryTex > 0.5)
        tremble = SorrowTremble(coords, uTime);

    float2 distortedUV = float2(coords.x - uTime * uScrollSpeed, coords.y + tremble);
    float4 trailSample = tex2D(uImage0, distortedUV);

    // Silver whisper highlights — faint flickers of light in the darkness
    float whisper = SilverWhisper(coords.x, uTime);

    // Color: deliberately desaturated, dark
    // Dark gray core -> faint silver at tail
    float colorT = coords.x * 0.6 + whisper;
    float3 trailColor = lerp(uColor, uSecondaryColor, colorT);

    // Extremely subtle center brightening (NOT white-hot, just lighter gray)
    float coreBright = pow(edgeFade, 2.5) * trailFade * 0.2;
    trailColor = lerp(trailColor, uSecondaryColor, coreBright);

    // Add silver whisper as additive highlight
    trailColor += uSecondaryColor * whisper * trailFade;

    // Final composite — intentionally low intensity
    float alpha = trailSample.a * coreFade * trailFade * uOpacity;
    float4 finalColor = float4(trailColor * uIntensity * 0.7, 0.0) * alpha;

    return finalColor * uOverbrightMult * sampleColor;
}

// =====================================================================
//  LamentTrailGlow — Barely-visible sorrowful bloom
// =====================================================================

float4 LamentGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for faint bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.7);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.3);

    // Slow sorrowful pulse — like labored breathing
    float pulse = 0.85 + 0.15 * sin(uTime * 1.5 + coords.x * 3.0);

    // Glow color — slightly lighter than main, blended toward silver
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3);

    // Very low opacity — this is meant to be barely visible
    float alpha = edgeFade * trailFade * uOpacity * 0.2 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique LamentTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 LamentMainPS();
    }
}

technique LamentTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 LamentGlowPS();
    }
}
