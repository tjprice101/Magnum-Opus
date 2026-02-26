// =====================================================================
//  SakuraSwingTrail.fx  ESakura's Blossom vertex trail shader
// =====================================================================
//
//  Purpose: Flowing sakura-themed energy trail for melee swing rendering.
//  Architecture: Standard pixel shader for CalamityStyleTrailRenderer geometry.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (blade tip), 1 = tail (oldest)
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    SakuraTrailFlow   EMain trail body with noise-driven petal flow
//    SakuraTrailGlow   EWider, softer bloom overlay pass
//
//  C# rendering order (3 passes):
//    1. SakuraTrailGlow @ 3x width  (soft underlayer bloom)
//    2. SakuraTrailFlow @ 1x width  (main trail body)
//    3. SakuraTrailGlow @ 1.5x width (bright overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture (ThinGlowLine)
sampler uImage1 : register(s1); // Noise texture (PerlinNoise)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (BloomPink)
float3 uSecondaryColor;   // Secondary trail color (PollenGold)
float  uOpacity;          // Overall opacity
float  uTime;             // Scrolling time value
float  uIntensity;        // Overall brightness multiplier
float  uOverbrightMult;   // Additive overbright for white-hot core

// --- Sakura-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed along trail
float  uNoiseScale;       // Noise distortion intensity
float  uDistortionAmt;    // Perpendicular distortion amount
float  uPhase;            // Combo phase [0-3] for color shifting

// =====================================================================
//  Utility Functions
// =====================================================================

float4 ApplyOverbright(float4 color)
{
    return color * uOverbrightMult;
}

// Smooth bump: 0 at edges, 1 at center  Efor cross-trail falloff
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// =====================================================================
//  SakuraTrailFlow  EMain flowering trail body
// =====================================================================

float4 SakuraFlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Trail edge softening  Efades at top/bottom edges
    float edgeFade = QuadraticBump(coords.y);

    // Head-to-tail opacity falloff (stronger at tip)
    float trailFade = pow(1.0 - coords.x, 1.8);

    // Scroll UV along trail for flowing energy effect
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y);

    // Sample noise for organic distortion
    float2 noiseUV = float2(coords.x * 2.0 - uTime * uScrollSpeed * 0.6, coords.y * 1.5);
    float4 noiseSample = tex2D(uImage1, noiseUV);
    float noiseVal = noiseSample.r;

    // Apply noise distortion perpendicular to trail
    float2 distortedUV = scrolledUV;
    distortedUV.y += (noiseVal - 0.5) * uDistortionAmt * edgeFade;

    // Sample trail body texture
    float4 trailSample = tex2D(uImage0, distortedUV);

    // Dual-frequency petal shimmer  Ecreates embedded petal impressions
    float petalWave1 = sin(coords.x * 12.0 - uTime * 3.0 + noiseVal * 6.0);
    float petalWave2 = cos(coords.x * 8.0 + uTime * 2.0 + noiseVal * 4.0);
    float petalShimmer = saturate(petalWave1 * 0.3 + petalWave2 * 0.2 + 0.5);

    // Color gradient: primary ↁEsecondary along trail length
    float colorT = coords.x * 0.7 + noiseVal * 0.3;
    float3 trailColor = lerp(uColor, uSecondaryColor, colorT);

    // White-hot core at trail center
    float coreMask = pow(edgeFade, 2.0) * trailFade;
    float3 coreWhite = float3(1.0, 0.98, 0.95);
    trailColor = lerp(trailColor, coreWhite, coreMask * 0.4 * petalShimmer);

    // Final composite
    float alpha = trailSample.a * edgeFade * trailFade * uOpacity * petalShimmer;
    float4 finalColor = float4(trailColor * uIntensity, 0.0) * alpha;

    return ApplyOverbright(finalColor) * sampleColor;
}

// =====================================================================
//  SakuraTrailGlow  ESoft bloom overlay pass
// =====================================================================

float4 SakuraGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge profile for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6); // Softer falloff than main pass

    // Gentler head-to-tail fade
    float trailFade = pow(1.0 - coords.x, 1.2);

    // Slow noise scroll for dreamy glow movement
    float2 noiseUV = float2(coords.x * 1.5 - uTime * uScrollSpeed * 0.3, coords.y);
    float4 noiseSample = tex2D(uImage1, noiseUV);
    float noiseVal = noiseSample.r;

    // Soft color  Eblend primary with white for ethereal glow
    float3 glowColor = lerp(uColor, float3(1.0, 0.95, 0.92), 0.35);
    glowColor = lerp(glowColor, uSecondaryColor, coords.x * 0.4);

    // Breathing pulse
    float pulse = 0.85 + 0.15 * sin(uTime * 2.0 + coords.x * 4.0);

    // Final bloom composite
    float alpha = edgeFade * trailFade * uOpacity * 0.35 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.7, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

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
