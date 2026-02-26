// =====================================================================
//  CrystalOrbitTrail.fx — Feather of the Iridescent Flock orbit trail
// =====================================================================
//
//  Visual: Prismatic faceted orbit trail for crystal sentinel minions.
//  Instead of smooth shimmer, uses discrete facets along the trail like
//  light through beveled crystal edges. Each facet is a different hue,
//  creating a stained-glass quality.
//
//  uPhase offsets the dominant hue per crystal (0.0 / 0.33 / 0.66)
//  so the three crystals have distinct color identities:
//    Crystal 0 = warm (red/orange/gold)
//    Crystal 1 = green/emerald
//    Crystal 2 = cool (blue/violet)
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head, 1 = tail
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    CrystalOrbitMain — Sharp faceted prismatic core
//    CrystalOrbitGlow — Soft prismatic bloom underlay
//
//  C# rendering order (3 passes):
//    1. CrystalOrbitGlow @ 3x width   (prismatic bloom)
//    2. CrystalOrbitMain @ 1x width   (sharp crystal facets)
//    3. CrystalOrbitGlow @ 1.5x width (overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Noise texture (CosmicEnergyVortex)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (Pearlescent)
float3 uSecondaryColor;   // Secondary trail color (PureWhite)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Crystal-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Facet distortion
float  uPhase;            // Crystal hue offset: 0.0 / 0.33 / 0.66
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

float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 HSLToRGB(float h, float s, float l)
{
    float3 rgb = HueToRGB(frac(h));
    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    return (rgb - 0.5) * c + l;
}

// =====================================================================
//  Crystal facet coloring
// =====================================================================

float3 CrystalFacet(float u, float time, float crystalPhase)
{
    // Discrete facets: each "block" along the trail gets a different hue
    float facetCount = 8.0;
    float facetIndex = floor(u * facetCount + time * 0.3);
    float facetFrac = frac(u * facetCount + time * 0.3);

    // Each facet gets a hue offset by crystal identity
    float hue = frac(facetIndex * 0.15 + crystalPhase);

    // Sharp step between facets — beveled crystal edge
    float edgeSharp = smoothstep(0.0, 0.08, facetFrac) * (1.0 - smoothstep(0.92, 1.0, facetFrac));

    // High saturation for vivid prismatic look
    float saturation = 0.8 + 0.15 * sin(facetIndex * 2.0 + time * 0.5);
    float luminance = 0.7 + edgeSharp * 0.15;

    float3 facetColor = HSLToRGB(hue, saturation, luminance);

    // Bright edge highlight at facet boundaries
    float edgeHighlight = 1.0 - edgeSharp;
    edgeHighlight = pow(edgeHighlight, 4.0) * 0.4;
    facetColor += float3(1.0, 1.0, 1.0) * edgeHighlight;

    return facetColor;
}

// =====================================================================
//  CrystalOrbitMain — Sharp faceted prismatic core
// =====================================================================

float4 CrystalOrbitMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Sharp core profile
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 2.5);

    // Trail fade — moderate for orbit trail
    float trailFade = pow(saturate(1.0 - coords.x), 1.4);

    // Noise-driven distortion
    float noiseOffset = 0.0;
    float vortexHighlight = 0.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 noiseUV = float2(coords.x * uSecondaryTexScale - uTime * uScrollSpeed * 0.4,
                                coords.y * uSecondaryTexScale * 0.5 + uTime * uSecondaryTexScroll * 0.1);
        float noiseSample = tex2D(uImage1, noiseUV).r;

        noiseOffset = (noiseSample - 0.5) * uDistortionAmt;

        // Vortex energy highlight
        float2 vortexUV = noiseUV * 1.4 + float2(uTime * 0.08, 0.0);
        float vortex2 = tex2D(uImage1, vortexUV).r;
        vortexHighlight = smoothstep(0.6, 0.85, noiseSample * vortex2 * 2.0) * 0.2;
    }

    // Scrolled UV
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + noiseOffset);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === CRYSTAL FACET COLORING ===
    float3 facetColor = CrystalFacet(coords.x, uTime, uPhase);

    // Blend with base pearlescent color
    float3 trailColor = lerp(uColor, facetColor, 0.75);

    // === WHITE-HOT CENTER ===
    float centerGlow = exp(-pow((coords.y - 0.5) / 0.06, 2.0));
    trailColor = lerp(trailColor, uSecondaryColor, centerGlow * 0.4 * trailFade);

    // Add vortex highlights
    float3 vortexColor = HueToRGB(frac(coords.x * 2.0 + uPhase + uTime * 0.15));
    trailColor += vortexColor * vortexHighlight * trailFade;

    // === TAIL DISSOLVE ===
    float tailDissolve = 1.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 dissolveUV = float2(coords.x * 2.0 + uTime * 0.2, coords.y * 1.5);
        float dissolveNoise = tex2D(uImage1, dissolveUV).r;
        float threshold = coords.x * 1.15 - 0.05;
        tailDissolve = smoothstep(threshold - 0.12, threshold + 0.08, dissolveNoise);
        tailDissolve = lerp(1.0, tailDissolve, smoothstep(0.5, 0.85, coords.x));
    }

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * tailDissolve * uOpacity;
    float3 finalColor = trailColor * uIntensity;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  CrystalOrbitGlow — Soft prismatic bloom
// =====================================================================

float4 CrystalOrbitGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.2);

    // Prismatic glow per-crystal
    float hue = frac(coords.x * 1.5 + uPhase + uTime * 0.08);
    float3 glowColor = HSLToRGB(hue, 0.5, 0.75);

    // Blend toward base color
    glowColor = lerp(uColor, glowColor, 0.5);

    // Breathing pulse
    float pulse = 0.85 + 0.15 * sin(uTime * 2.0 + coords.x * 3.0);

    // Low opacity bloom
    float alpha = edgeFade * trailFade * uOpacity * 0.3 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique CrystalOrbitMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrystalOrbitMainPS();
    }
}

technique CrystalOrbitGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrystalOrbitGlowPS();
    }
}
