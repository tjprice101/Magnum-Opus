// =============================================================================
// MagnumOpus Prismatic Beam Shader - PS 2.0 Compatible
// =============================================================================
// Spectral color-splitting beam trail for Moonlight's Calling  E"The Serenade".
// Creates a prismatic refraction effect where the beam body visually splits
// into spectral components, like light passing through a crystal prism.
//
// Visual concept: Each bounce widens the spectral spread, revealing more
// rainbow colors within the beam body. By the final bounce, the beam
// contains the full visible spectrum  Ea serenade of light.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   PrismaticBeamMain  EFull spectral beam with color splitting
//   PrismaticBeamGlow  ESofter bloom pass for layered stacking
// =============================================================================

sampler uImage0 : register(s0); // Base texture (beam body)
sampler uImage1 : register(s1); // Noise/refraction texture

float3 uColor;           // Primary color (e.g. PrismViolet)
float3 uSecondaryColor;  // Secondary color (e.g. RefractedBlue)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Bounce-based intensity (0.3 - 1.5)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // UV scroll rate along beam
float uNoiseScale;        // Noise UV repetition
float uDistortionAmt;     // Chromatic offset strength
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;
float uPhase;             // Spectral spread phase (0 = narrow purple, 1 = full rainbow)

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

// HSL to RGB conversion for spectral color generation
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

// Full spectral color from position across beam width
// baseHue centers the spectrum, spread controls how much of the rainbow is visible
float3 SpectralColor(float vPos, float baseHue, float spread)
{
    float hue = baseHue + (vPos - 0.5) * spread;
    hue = frac(hue); // Wrap around
    float3 rgb = HueToRGB(hue);
    // Boost saturation and brightness
    return lerp(float3(0.8, 0.8, 1.0), rgb, 0.7 + spread * 0.3);
}

// =============================================================================
// TECHNIQUE 1: PRISMATIC BEAM MAIN
// =============================================================================

float4 PrismaticBeamPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Edge fade (beam width) ---
    float edgeFade = QuadraticBump(coords.y);
    float sharpEdge = pow(edgeFade, 1.2);

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.1);
    float headBright = saturate(1.0 - coords.x * 3.0); // Bright head

    // --- Chromatic splitting ---
    // Offset UV.y differently for red/green/blue channels to create prismatic spread
    float chromaticOffset = uDistortionAmt * uPhase;
    float2 coordsR = coords + float2(0.0, chromaticOffset * 0.5);
    float2 coordsG = coords;
    float2 coordsB = coords - float2(0.0, chromaticOffset * 0.5);

    // Sample base texture with chromatic offsets
    float4 baseTex = tex2D(uImage0, coords);
    float texR = tex2D(uImage0, coordsR).r;
    float texG = tex2D(uImage0, coordsG).g;
    float texB = tex2D(uImage0, coordsB).b;
    float3 chromaticBase = float3(texR, texG, texB);

    // --- Spectral color mapping ---
    // The beam splits into a spectral rainbow across its width
    float baseHue = 0.75; // Start at purple/violet
    float spectralSpread = 0.15 + uPhase * 0.35; // More bounces = wider rainbow
    float3 spectralCol = SpectralColor(coords.y, baseHue, spectralSpread);

    // --- Scrolling energy pattern ---
    float scrollT = uTime * uScrollSpeed;
    float energyWave = sin(coords.x * 12.0 - scrollT * 4.0) * 0.15 + 0.85;
    float energyPulse = sin(coords.x * 6.0 + scrollT * 2.0) * 0.08 + 0.92;

    // --- Standing wave nodes (musical resonance) ---
    float nodePattern = abs(sin(coords.x * 3.14159 * (3.0 + uPhase * 2.0)));
    float antinodeBright = pow(nodePattern, 0.5) * 0.3 + 0.7;

    // --- Noise texture modulation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.4);

    // --- Core beam brightness ---
    float coreBright = sharpEdge * (1.0 + headBright * 0.5);

    // --- Compose final color ---
    // Mix spectral colors with primary/secondary based on position
    float3 primaryMix = lerp(uColor, uSecondaryColor, coords.x * 0.5);
    float3 finalColor = lerp(primaryMix, spectralCol, uPhase * 0.6);

    // Apply chromatic base texture modulation
    finalColor *= lerp(float3(1, 1, 1), chromaticBase * 2.0, 0.3);

    // Apply energy patterns
    finalColor *= energyWave * energyPulse * antinodeBright;

    // Apply noise
    finalColor *= noiseVal;

    // Boost core brightness (center of beam)
    float centerBoost = exp(-pow((coords.y - 0.5) * 4.0, 2.0));
    finalColor += float3(1.0, 0.98, 1.0) * centerBoost * 0.4 * uIntensity;

    float alpha = coreBright * trailFade * uOpacity * sampleColor.a * uIntensity;

    return ApplyOverbright(finalColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUE 2: PRISMATIC BEAM GLOW
// =============================================================================

float4 PrismaticBeamGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Softer version for bloom stacking
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    float trailFade = saturate(1.0 - coords.x * 0.85);

    // Simplified spectral tint
    float baseHue = 0.75;
    float spread = 0.1 + uPhase * 0.25;
    float3 spectralCol = SpectralColor(coords.y, baseHue, spread);

    float3 glowColor = lerp(uColor, spectralCol, uPhase * 0.5);
    glowColor = lerp(glowColor, uSecondaryColor, coords.x * 0.4);

    // Gentle energy pulse
    float pulse = sin(uTime * 3.0 + coords.x * 4.0) * 0.06 + 0.94;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * uIntensity * pulse * 0.6;

    return ApplyOverbright(glowColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique PrismaticBeamMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PrismaticBeamPS();
    }
}

technique PrismaticBeamGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PrismaticBeamGlowPS();
    }
}
