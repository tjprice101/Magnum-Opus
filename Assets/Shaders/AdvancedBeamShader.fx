/*
 * ADVANCED BEAM SHADER - Calamity-Style Rendering
 * 
 * Features:
 * - SDF (Signed Distance Function) for sharp beam core
 * - Fractal Brownian Motion (fBM) noise for energy effects
 * - Palette/Gradient color lookup via texture sampling
 * - Multi-pass bloom with additive overdraw
 * - Time-based UV scrolling
 * 
 * Usage: Apply via Effect.Parameters before drawing primitives
 */

// === TEXTURE SAMPLERS ===

sampler2D uImage0 : register(s0);    // Main beam texture (grayscale gradient)
sampler2D uImage1 : register(s1);    // Noise texture (for fBM)
sampler2D uPalette : register(s2);   // Color palette (1D gradient lookup)

// === SHADER PARAMETERS ===

float uTime;                         // Time for animation (seconds or ticks)
float uOpacity;                      // Overall opacity multiplier
float uIntensity;                    // Bloom/glow intensity
float uNoiseScale;                   // Scale of noise sampling
float uNoiseSpeed;                   // Speed of noise scrolling
float uCoreSharpness;                // SDF core sharpness (higher = sharper edge)
float uCoreWidth;                    // Width of the SDF core (0-1, fraction of beam)
float uPaletteOffset;                // Offset for palette lookup animation

float2 uResolution;                  // Render target resolution
float2 uBeamDirection;               // Normalized beam direction
float4 uTintColor;                   // Tint/multiply color

// === UTILITY FUNCTIONS ===

/**
 * QuadraticBump: Classic smooth falloff function
 * Returns 0 at edges, 1 at center
 * Input x: 0 to 1 (edge to edge)
 */
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

/**
 * InverseLerp: Get interpolation factor from value
 * Returns t where value = lerp(min, max, t)
 */
float InverseLerp(float from, float to, float value)
{
    return saturate((value - from) / (to - from));
}

/**
 * Hash function for procedural noise
 */
float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

/**
 * Value noise for smooth random patterns
 */
float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    
    // Smooth interpolation
    float2 u = f * f * (3.0 - 2.0 * f);
    
    // Four corners
    float a = Hash21(i + float2(0.0, 0.0));
    float b = Hash21(i + float2(1.0, 0.0));
    float c = Hash21(i + float2(0.0, 1.0));
    float d = Hash21(i + float2(1.0, 1.0));
    
    // Bilinear interpolation
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

/**
 * Fractal Brownian Motion (fBM)
 * Layers multiple octaves of noise for organic patterns
 * 
 * @param p: Sample position
 * @param octaves: Number of noise layers (3-6 typical)
 */
float fBM(float2 p, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    float totalAmp = 0.0;
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * ValueNoise(p * frequency);
        totalAmp += amplitude;
        amplitude *= 0.5;        // Halve amplitude each octave
        frequency *= 2.0;        // Double frequency each octave
        p = p * 1.7 + float2(0.23, 0.72); // Rotate/offset for variety
    }
    
    return value / totalAmp;
}

/**
 * SDF for a line segment (beam core)
 * Returns signed distance to the line
 */
float SDFLine(float2 p, float2 a, float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;
    float h = saturate(dot(pa, ba) / dot(ba, ba));
    return length(pa - ba * h);
}

/**
 * SDF for beam cross-section (across the width)
 * UV.y = 0 at one edge, 1 at other edge
 * Core is at UV.y = 0.5
 */
float SDFBeamCore(float uvY, float coreWidth)
{
    float distFromCenter = abs(uvY - 0.5) * 2.0;  // 0 at center, 1 at edges
    float coreEdge = 1.0 - coreWidth;
    return InverseLerp(coreEdge, 1.0, distFromCenter);
}

/**
 * Palette lookup with animation
 * Samples a 1D gradient texture
 */
float4 SamplePalette(float t)
{
    float animatedT = frac(t + uPaletteOffset);
    return tex2D(uPalette, float2(animatedT, 0.5));
}

// === MAIN SHADER PASSES ===

/**
 * PASS 1: Core Beam Shader
 * Combines SDF core with fBM noise for energy effect
 */
float4 BeamCoreShader(float2 uv : TEXCOORD0) : COLOR0
{
    // Sample base beam texture
    float4 baseTex = tex2D(uImage0, uv);
    
    // Calculate SDF-based core intensity
    float coreDistance = SDFBeamCore(uv.y, uCoreWidth);
    float coreMask = 1.0 - saturate(coreDistance * uCoreSharpness);
    coreMask = pow(coreMask, 2.0);  // Sharpen the falloff
    
    // Generate fBM noise for energy effect
    float2 noiseUV = uv * uNoiseScale;
    noiseUV.x += uTime * uNoiseSpeed;  // Scroll along beam
    float noise = fBM(noiseUV, 4);
    
    // Modulate core with noise for organic energy look
    float energyMask = lerp(coreMask, coreMask * (0.7 + noise * 0.6), 0.5);
    
    // Edge glow using QuadraticBump
    float edgeGlow = QuadraticBump(saturate(uv.y));
    edgeGlow *= (0.8 + noise * 0.4);  // Add noise variation to edges
    
    // Combine core and edge
    float intensity = max(energyMask, edgeGlow * 0.5);
    
    // Color from palette based on intensity and noise
    float paletteT = intensity * 0.8 + noise * 0.2;
    float4 beamColor = SamplePalette(paletteT);
    
    // Apply tint
    beamColor *= uTintColor;
    
    // Final output
    float alpha = intensity * uOpacity * baseTex.a;
    return float4(beamColor.rgb, alpha);
}

/**
 * PASS 2: Bloom/Glow Pass
 * Softer, wider glow for additive blending
 */
float4 BeamBloomShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    
    // Wider, softer falloff for bloom
    float dist = abs(uv.y - 0.5) * 2.0;
    float bloom = 1.0 - dist;
    bloom = pow(bloom, 0.5);  // Softer falloff than core
    
    // Add noise variation
    float2 noiseUV = uv * uNoiseScale * 0.5;
    noiseUV.x += uTime * uNoiseSpeed * 0.5;
    float noise = fBM(noiseUV, 2);
    bloom *= (0.9 + noise * 0.2);
    
    // Horizontal falloff at beam ends
    float hFade = QuadraticBump(saturate(uv.x));
    bloom *= hFade;
    
    // Color from palette (shifted toward brighter end)
    float4 bloomColor = SamplePalette(0.7 + noise * 0.3);
    bloomColor *= uTintColor;
    
    float alpha = bloom * uOpacity * uIntensity * baseTex.a;
    return float4(bloomColor.rgb, alpha);
}

/**
 * PASS 3: Fresnel Edge Highlight
 * Bright edge effect for "contained energy" look
 */
float4 BeamFresnelShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    
    // Edge detection (high at edges, low at center)
    float dist = abs(uv.y - 0.5) * 2.0;
    float edge = pow(dist, 3.0);  // Strong edge bias
    
    // Only show edge within beam bounds
    float bounds = 1.0 - saturate((dist - 0.8) * 5.0);
    edge *= bounds;
    
    // Noise for crackling effect
    float2 noiseUV = uv * uNoiseScale * 2.0;
    noiseUV += float2(uTime * uNoiseSpeed * 2.0, 0);
    float noise = fBM(noiseUV, 3);
    edge *= (0.3 + noise * 1.4);  // Strong noise modulation
    
    // Bright white/cyan color for Fresnel edge
    float4 fresnelColor = float4(0.8, 0.95, 1.0, 1.0);
    fresnelColor *= uTintColor;
    
    float alpha = edge * uOpacity * baseTex.a;
    return float4(fresnelColor.rgb * 2.0, alpha);  // Extra bright
}

/**
 * PASS 4: Noise Overlay
 * Additional noise layer for extra detail
 */
float4 BeamNoiseOverlay(float2 uv : TEXCOORD0) : COLOR0
{
    // Multi-layer scrolling noise
    float2 noiseUV1 = uv * uNoiseScale;
    noiseUV1.x += uTime * uNoiseSpeed;
    
    float2 noiseUV2 = uv * uNoiseScale * 1.7;
    noiseUV2.x -= uTime * uNoiseSpeed * 0.7;
    noiseUV2.y += uTime * uNoiseSpeed * 0.3;
    
    float noise1 = fBM(noiseUV1, 3);
    float noise2 = fBM(noiseUV2, 2);
    float combinedNoise = (noise1 + noise2) * 0.5;
    
    // Mask to beam shape
    float dist = abs(uv.y - 0.5) * 2.0;
    float mask = 1.0 - pow(dist, 1.5);
    mask *= QuadraticBump(saturate(uv.x));
    
    float intensity = combinedNoise * mask;
    
    // Palette sample
    float4 noiseColor = SamplePalette(0.3 + combinedNoise * 0.4);
    
    float alpha = intensity * uOpacity * 0.5;  // Subtle overlay
    return float4(noiseColor.rgb, alpha);
}

// === TECHNIQUES ===

technique BeamCore
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 BeamCoreShader();
    }
}

technique BeamBloom
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 BeamBloomShader();
    }
}

technique BeamFresnel
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 BeamFresnelShader();
    }
}

technique BeamNoiseOverlay
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 BeamNoiseOverlay();
    }
}

// === COMBINED TECHNIQUE ===
// Single-pass version that combines multiple effects

float4 BeamCombinedShader(float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    
    // === CORE ===
    float coreDistance = SDFBeamCore(uv.y, uCoreWidth);
    float coreMask = 1.0 - saturate(coreDistance * uCoreSharpness);
    coreMask = pow(coreMask, 2.0);
    
    // === NOISE ===
    float2 noiseUV = uv * uNoiseScale;
    noiseUV.x += uTime * uNoiseSpeed;
    float noise = fBM(noiseUV, 4);
    
    // === ENERGY MODULATION ===
    float energy = lerp(coreMask, coreMask * (0.7 + noise * 0.6), 0.5);
    
    // === EDGE GLOW ===
    float edgeGlow = QuadraticBump(saturate(uv.y));
    edgeGlow *= (0.8 + noise * 0.4);
    
    // === FRESNEL EDGE ===
    float dist = abs(uv.y - 0.5) * 2.0;
    float fresnel = pow(dist, 3.0);
    float bounds = 1.0 - saturate((dist - 0.8) * 5.0);
    fresnel *= bounds * (0.3 + noise * 1.4);
    
    // === COMBINE ===
    float intensity = max(energy, edgeGlow * 0.5);
    
    // === COLOR ===
    float paletteT = intensity * 0.8 + noise * 0.2;
    float4 beamColor = SamplePalette(paletteT);
    
    // Add fresnel as white highlight
    beamColor.rgb += fresnel * float3(0.5, 0.6, 0.7);
    
    // Apply tint
    beamColor *= uTintColor;
    
    float alpha = intensity * uOpacity * baseTex.a;
    return float4(beamColor.rgb, alpha);
}

technique BeamCombined
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 BeamCombinedShader();
    }
}
