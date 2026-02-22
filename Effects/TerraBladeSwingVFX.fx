// =============================================================================
// Terra Blade Swing VFX Shader - PS 2.0 Compatible
// =============================================================================
// Calamity-tier per-weapon shader for the Sandbox Terra Blade swing.
// Three techniques in a single file for efficient single-load usage:
//
//   1. EnergyTrail   – Noise-distorted afterimage energy trail
//                      (green → cyan gradient, QuadraticBump fade, noise turbulence)
//
//   2. BladeBloom    – Multi-layer radial bloom with directional bias
//                      (4-layer falloff emulation in a single pass,
//                       inner white-hot core → green fringe, sine pulse)
//
//   3. SlashSmear    – Arc-shaped swing smear overlay
//                      (angular gradient, noise displacement, sharp edge glow)
//
// Uniform naming follows MagnumOpus convention (uColor, uSecondaryColor, etc.)
// so ShaderLoader.GetShader() + standard parameter calls Just Work.
// =============================================================================

sampler uImage0 : register(s0);       // Primary texture (blade sprite / glow tex)
sampler uImage1 : register(s1);       // Noise texture (PerlinNoise recommended)

// --- Standard MagnumOpus uniforms ---
float3 uColor;                         // Primary color  (green:  0.39, 1.0, 0.47)
float3 uSecondaryColor;                // Secondary color (cyan:  0.20, 0.78, 0.59)
float uOpacity;                        // Master opacity (0-1)
float uTime;                           // Main.GlobalTimeWrappedHourly
float uIntensity;                      // Brightness multiplier (1-5)
float uProgress;                       // Swing progression (0-1)

// --- Overbright / noise uniforms ---
float uOverbrightMult;                 // HDR-like multiplier (2-5)
float uHasSecondaryTex;                // 1.0 if uImage1 is bound, else 0.0
float uSecondaryTexScale;              // UV scale for noise sampling
float uSecondaryTexScroll;             // UV scroll speed for noise

// --- Weapon-specific uniforms ---
float uSwingSpeed;                     // Angular velocity intensity (0-1)
float uDirection;                      // Swing direction (-1 or 1)

// =============================================================================
// UTILITY
// =============================================================================

// 0 → 1 → 0  (peak at x = 0.5)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// Smooth hermite-like step
float SmoothFade(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: ENERGY TRAIL
// =============================================================================
// Draws each afterimage blade sprite with:
// - QuadraticBump edge-to-centre fade for soft width falloff
// - Green→Cyan gradient along the blade (UV.y = pommel→tip)
// - Noise-based turbulence for organic energy look
// - Sine pulse synchronised to time + blade position
// - Progress-based tail fade for trail dissipation
// =============================================================================

float4 EnergyTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Edge fade: bright along centre of the blade silhouette ---
    float edgeFade = QuadraticBump(coords.x);

    // --- Trail dissipation: older images (higher uProgress) fade more ---
    float trailFade = saturate(1.0 - uProgress * 1.15);

    // --- Color gradient along blade length (pommel → tip) ---
    float3 energyColor = lerp(uColor, uSecondaryColor, coords.y);

    // --- White-hot core in the very centre of the blade ---
    float coreMask = saturate((edgeFade - 0.6) * 4.5);
    energyColor = lerp(energyColor, float3(1.0, 1.0, 0.95), coreMask * 0.7);

    // --- Noise turbulence (branch-free when unbound) ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll;
    noiseUV.y -= uTime * uSecondaryTexScroll * 0.7;
    float4 noise = tex2D(uImage1, noiseUV);
    float noiseMod = lerp(1.0, noise.r * 1.6 + 0.2, uHasSecondaryTex * 0.5);
    energyColor *= noiseMod;

    // --- Sine pulse: gentle throb along blade ---
    float pulse = 0.90 + sin(uTime * 7.0 + coords.y * 14.0) * 0.10;

    // --- Combine ---
    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    float3 finalColor = energyColor * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: BLADE BLOOM
// =============================================================================
// Emulates a 4-layer bloom stack in a single shader pass.
// Each "layer" is a concentric radial ring with different opacity/scale.
// The shader takes the glow texture and applies:
//   Layer 1 (outer):  wide, dim halo
//   Layer 2 (mid):    medium ring
//   Layer 3 (inner):  concentrated glow
//   Layer 4 (core):   white-hot pinpoint
// All summed additively with directional bias toward the blade tip.
// =============================================================================

float4 BladeBloomPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Radial distance from centre (0 = centre, 1 = edge)
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // --- 4-layer bloom emulation via stepped falloff ---
    // Each layer has: scale, opacity, color blend
    float outerGlow  = saturate(1.0 - dist * 0.55) * 0.30;      // Layer 1: wide, dim
    float midGlow    = saturate(1.0 - dist * 0.85) * 0.50;      // Layer 2: medium
    float innerGlow  = saturate(1.0 - dist * 1.30) * 0.70;      // Layer 3: concentrated
    float coreGlow   = saturate(1.0 - dist * 2.50) * 0.90;      // Layer 4: hot core

    // Combined glow intensity
    float totalGlow = outerGlow + midGlow + innerGlow + coreGlow;

    // --- Color gradient: green outer → white core ---
    float3 bloomColor = lerp(uColor, float3(1.0, 1.0, 1.0), coreGlow / max(totalGlow, 0.01));
    bloomColor = lerp(bloomColor, uSecondaryColor, outerGlow * 0.6);

    // --- Directional bias: bloom stretches toward blade tip ---
    float directionalBias = saturate(centred.y * uDirection * -1.5 + 0.6);
    totalGlow *= directionalBias;

    // --- Pulse animation ---
    float pulse = 0.88 + sin(uTime * 5.0) * 0.12;
    totalGlow *= pulse;

    // --- Swing speed intensity: bloom brightens during fast swing ---
    totalGlow *= (0.6 + uSwingSpeed * 0.4);

    float alpha = totalGlow * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = bloomColor * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 3: SLASH SMEAR
// =============================================================================
// Arc-shaped energy smear drawn over the swing path.
// Angular gradient from swing start to current position.
// Noise-displaced edges for organic energy turbulence.
// Sharp bright edge at the leading front of the swing.
// =============================================================================

float4 SlashSmearPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Arc gradient: bright at leading edge (UV.x = 0), fading behind ---
    float arcFade = saturate(1.0 - coords.x * 1.2);
    arcFade = arcFade * arcFade;   // Sharper leading edge

    // --- Width falloff: bright centre, fading edges ---
    float widthFade = QuadraticBump(coords.y);

    // --- Sharp leading edge highlight ---
    float leadingEdge = saturate((1.0 - coords.x) * 5.0 - 3.5);
    float3 edgeHighlight = lerp(uColor, float3(1.0, 1.0, 0.95), leadingEdge);

    // --- Noise displacement for organic turbulence ---
    float2 noiseUV = coords * uSecondaryTexScale * 1.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 2.0;
    noiseUV.y += uTime * uSecondaryTexScroll * 0.5;
    float4 noise = tex2D(uImage1, noiseUV);

    // Displace width slightly with noise for wispy edges
    float noiseBias = lerp(0.0, (noise.r - 0.5) * 0.3, uHasSecondaryTex);
    float displacedWidth = saturate(widthFade + noiseBias);

    // --- Color: green → cyan gradient across the smear arc ---
    float3 smearColor = lerp(uSecondaryColor, uColor, coords.x * 0.7);
    smearColor = lerp(smearColor, edgeHighlight, leadingEdge * 0.8);

    // --- Noise color modulation for extra richness ---
    smearColor *= lerp(1.0, noise.r * 1.3 + 0.4, uHasSecondaryTex * 0.35);

    // --- Swing speed: smear is more visible during fast swing ---
    float speedAlpha = saturate(uSwingSpeed * 1.5);

    // --- Progression window: ramp in at 15%, ramp out at 90% ---
    float progressIn  = SmoothFade(0.12, 0.25, uProgress);
    float progressOut = SmoothFade(0.92, 0.80, uProgress);
    float progressWindow = progressIn * progressOut;

    // --- Combine ---
    float alpha = arcFade * displacedWidth * speedAlpha * progressWindow
                * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = smearColor * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 4: SHIMMER OVERLAY
// =============================================================================
// Combined scrolling noise + iridescent color cycling overlay.
// Scrolling noise controls WHERE shimmer appears; position+time drives WHAT COLOR.
//
// Noise layers: Two counter-scrolling samples from uImage1 (SparklyNoiseTexture)
//   create organic energy flow patterns with sparkle hot-spots.
//
// Iridescence: Hue rotation based on UV.y (blade position) + time + noise,
//   blended 65/35 with the weapon's green palette for theme consistency.
//
// Masking: Blade alpha (from uImage0) confines shimmer to blade silhouette.
//   QuadraticBump edge fade + tip bias prevent harsh boundaries.
//
// Instruction budget: ~55 ALU + 3 TEX (within ps_2_0 64+32 limit)
// =============================================================================

float4 ShimmerOverlayPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float bladeMask = baseTex.a;

    // --- Dual-layer scrolling noise (organic energy flow) ---
    float2 noise1UV = coords * uSecondaryTexScale;
    noise1UV.x += uTime * uSecondaryTexScroll;
    noise1UV.y -= uTime * uSecondaryTexScroll * 0.7;
    float noise1 = tex2D(uImage1, noise1UV).r;

    float2 noise2UV = coords * uSecondaryTexScale * 1.3;
    noise2UV.x -= uTime * uSecondaryTexScroll * 0.8;
    noise2UV.y += uTime * uSecondaryTexScroll * 0.4;
    float noise2 = tex2D(uImage1, noise2UV).g;

    float combinedNoise = noise1 * 0.7 + noise2 * 0.3;

    // --- Sparkle peaks (white-hot points at noise maxima) ---
    float sparkle = saturate((combinedNoise - 0.65) * 5.0);
    float sparklePulse = sin(uTime * 12.0 + coords.x * 8.0 + coords.y * 6.0) * 0.3 + 0.7;
    sparkle *= sparklePulse;

    // --- Iridescent color cycling (hue rotation in PS 2.0) ---
    float wavelength = frac(
        coords.y * 0.5 +
        coords.x * 0.3 +
        uTime * uSecondaryTexScroll * 0.3 +
        combinedNoise * 0.15
    );

    float hue = wavelength * 6.0;
    float3 rainbow;
    rainbow.r = saturate(abs(hue - 3.0) - 1.0);
    rainbow.g = saturate(2.0 - abs(hue - 2.0));
    rainbow.b = saturate(2.0 - abs(hue - 4.0));

    // Blend rainbow 35% with theme palette 65% for consistency
    float3 themeColor = lerp(uColor, uSecondaryColor, wavelength);
    float3 iridescentColor = lerp(themeColor, rainbow, 0.20);

    // --- Edge fade + tip bias ---
    float edgeFade = QuadraticBump(coords.x);
    float tipBias = lerp(0.4, 1.0, coords.y);

    // --- Swing speed modulation ---
    float speedMod = lerp(0.5, 1.2, uSwingSpeed);

    // --- Shimmer visibility mask ---
    float shimmerMask = saturate(combinedNoise * edgeFade * tipBias * speedMod * 2.2);

    // --- Final composite ---
    float3 shimmerColor = iridescentColor * shimmerMask;
    float3 sparkleColor = float3(1.0, 1.0, 0.95) * sparkle * edgeFade * tipBias;
    float3 finalColor = (shimmerColor + sparkleColor) * baseTex.rgb * uIntensity;
    float finalAlpha = shimmerMask * bladeMask * uOpacity * sampleColor.a;

    return ApplyOverbright(finalColor, finalAlpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique EnergyTrail
{
    pass P0
    {
        PixelShader = compile ps_2_0 EnergyTrailPS();
    }
}

technique BladeBloom
{
    pass P0
    {
        PixelShader = compile ps_2_0 BladeBloomPS();
    }
}

technique SlashSmear
{
    pass P0
    {
        PixelShader = compile ps_2_0 SlashSmearPS();
    }
}

technique ShimmerOverlay
{
    pass P0
    {
        PixelShader = compile ps_2_0 ShimmerOverlayPS();
    }
}
