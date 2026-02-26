// =============================================================================
// MagnumOpus Lunar Phase Aura Shader - PS 2.0 Compatible
// =============================================================================
// Radial aura effect with expanding concentric rings.
// Creates a "gravitational tidal field" around the player during hold/swing.
//
// Unique to Eternal Moon -- "The Tidal Field":
//   - Concentric rings that scroll outward (tidal rhythm)
//   - Ring density and brightness scale with lunar phase
//   - Radial falloff: exponential decrease from center
//   - Noise-broken symmetry for organic, nebulous look
//   - Color pulses between deep tide purple and crescent ice blue
//   - Breathing/pulsing intensity from tidal rhythm
//
// UV Layout:
//   Centered at (0.5, 0.5) -- radial coordinate system
//   Draws onto a fullscreen quad or sprite
//
// Techniques:
//   LunarPhaseAuraPass -- Concentric expanding ring aura
// =============================================================================

sampler uImage0 : register(s0); // Base texture (soft glow circle)
sampler uImage1 : register(s1); // Noise texture

float3 uColor;           // Primary color (e.g. Violet)
float3 uSecondaryColor;  // Secondary color (e.g. IceBlue)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Overall brightness (scales with combo phase)
float uPhase;            // Lunar phase 0-1 (controls ring density + brightness)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Ring expansion rate
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Concentric ring pattern: returns brightness for a distance-from-center value
float ConcentricRings(float dist, float ringCount, float time, float scrollSpeed)
{
    // Rings scroll outward over time
    float ringPhase = dist * ringCount - time * scrollSpeed;
    // Sharp ring lines using sin^power
    float ring = sin(ringPhase * 6.28318);
    ring = ring * 0.5 + 0.5; // Remap to 0-1
    ring = ring * ring * ring; // Sharpen to thin bright lines
    return ring;
}

// Angular variation: makes rings subtly brighter in some directions
float AngularModulation(float2 centered, float time)
{
    float angle = atan2(centered.y, centered.x);
    // Slowly rotating modulation pattern
    float mod1 = sin(angle * 3.0 + time * 0.5) * 0.15;
    float mod2 = sin(angle * 5.0 - time * 0.3) * 0.08;
    return 0.77 + mod1 + mod2;
}

// =============================================================================
// TECHNIQUE: LUNAR PHASE AURA
// =============================================================================

float4 LunarPhaseAuraPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Center UV at (0, 0)
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);

    // Sample base texture for soft envelope
    float4 baseTex = tex2D(uImage0, coords);

    // --- Ring count scales with phase (2 rings at new moon, 8 at full) ---
    float ringCount = 2.0 + uPhase * 6.0;

    // --- Primary concentric rings ---
    float rings = ConcentricRings(dist, ringCount, uTime, uScrollSpeed);

    // --- Secondary ring layer (offset phase for depth) ---
    float rings2 = ConcentricRings(dist, ringCount * 0.7, uTime * 0.8, uScrollSpeed * 0.6);

    // Blend the two ring layers
    float combinedRings = rings * 0.7 + rings2 * 0.3;

    // --- Noise modulation to break perfect symmetry ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.2;
    noiseUV.y -= uTime * uSecondaryTexScroll * 0.15;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.7, noiseTex.r, uHasSecondaryTex);

    // Apply noise to rings for organic, nebulous look
    combinedRings *= noiseVal;

    // --- Angular variation (directional brightness) ---
    float angularMod = AngularModulation(centered, uTime);
    combinedRings *= angularMod;

    // --- Radial falloff (exponential decrease from center) ---
    float innerCutout = saturate(dist * 5.0); // Slight hole at dead center
    float outerFalloff = exp(-dist * dist * 8.0); // Gaussian envelope
    float radialMask = innerCutout * outerFalloff;

    // --- Color gradient: inner rings violet, outer rings ice blue ---
    float colorT = saturate(dist * 2.5);
    float3 auraColor = lerp(uColor, uSecondaryColor, colorT);

    // Ring highlights: brighter at ring crests
    float3 ringHighlight = lerp(uSecondaryColor, float3(0.92, 0.96, 1.0), 0.4);
    auraColor = lerp(auraColor, ringHighlight, combinedRings * 0.5);

    // --- Tidal breathing pulse ---
    float breathe = sin(uTime * 2.5) * 0.12 * uPhase + 0.88;

    // --- Phase-dependent overall brightness ---
    float phaseBrightness = 0.3 + uPhase * 0.7;

    // --- Final composition ---
    float3 finalColor = auraColor * uIntensity * phaseBrightness * breathe * baseTex.rgb;

    float alpha = combinedRings * radialMask * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique LunarPhaseAuraPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 LunarPhaseAuraPS();
    }
}
