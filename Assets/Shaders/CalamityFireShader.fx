// =============================================================================
// MagnumOpus CALAMITY-STYLE FIRE SHADER
// =============================================================================
// 
// Full implementation of Ark of the Cosmos-style flaming effects:
// 
// 1. DUAL-SCROLL UV ADVECTION - Two noise samples with multiplicative blending
// 2. UV DISTORTION (WARP) - Noise-driven UV displacement for flame physics
// 3. THRESHOLD EROSION - smoothstep for organic, licking flame edges
// 4. DYNAMIC COLOR RAMPS - Grayscale mask → color gradient mapping
// 5. fBm NOISE - Fractal Brownian Motion for organic turbulence
// 6. SPECTRAL LUT SAMPLING - Realistic chromatic effects via Fraunhofer lines
// 
// TEXTURE SLOTS:
// - uImage0 (s0): Base texture (core beam/trail)
// - uNoiseTex (s1): Tileable noise (TileableFBMNoise.png)
// - uPaletteLUT (s2): Color gradient LUT (RainbowLUT or theme-specific)
// - uMaskTex (s3): Edge/glow mask (EclipseRing.png)
// 
// =============================================================================

// Samplers
sampler uImage0 : register(s0);        // Base/core texture
sampler uNoiseTex : register(s1);      // Noise for advection
sampler uPaletteLUT : register(s2);    // Color ramp/LUT
sampler uMaskTex : register(s3);       // Mask texture

// Standard uniforms
float3 uColor;                         // Primary flame color
float3 uSecondaryColor;                // Secondary color (cool edges)
float3 uHotColor;                      // White-hot center color (usually white)
float uOpacity;                        // Overall opacity
float uTime;                           // Animation time
float uIntensity;                      // Brightness multiplier

// Dual-Scroll Advection Parameters
float uNoiseScale;                     // Scale of noise sampling (default: 2.0)
float uScrollSpeedA;                   // Layer A scroll speed (default: 0.3)
float uScrollSpeedB;                   // Layer B scroll speed (default: 0.2)
float uScrollAngleA;                   // Layer A scroll angle in radians
float uScrollAngleB;                   // Layer B scroll angle in radians

// UV Distortion Parameters
float uDistortionStrength;             // How much UV is warped (default: 0.08)
float uDistortionFrequency;            // Warp pattern frequency (default: 3.0)

// Erosion Parameters
float uErosionThreshold;               // Base cutoff (0-1, default: 0.3)
float uErosionSoftness;                // Edge softness (default: 0.1)
float uTailErosion;                    // Extra erosion at tail (default: 0.4)

// Flicker Parameters
float uFlickerSpeed;                   // How fast flames flicker (default: 8.0)
float uFlickerIntensity;               // Flicker strength (default: 0.15)

// Color Ramp Parameters
float uHeatFalloff;                    // How quickly heat drops off (default: 2.0)
float uLUTOffset;                      // Offset into spectral LUT

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

float QuadraticBump(float x)
{
    // 0 at edges, 1 at center - perfect for trail width profiles
    return x * (4.0 - x * 4.0);
}

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : (1.0 - x) * 2.0;
}

// Proper smoothstep for erosion
float SmoothErosion(float threshold, float softness, float value)
{
    return smoothstep(threshold - softness, threshold + softness, value);
}

// Rotate UV by angle
float2 RotateUV(float2 uv, float angle)
{
    float s = sin(angle);
    float c = cos(angle);
    return float2(
        uv.x * c - uv.y * s,
        uv.x * s + uv.y * c
    );
}

// =============================================================================
// CORE FUNCTION: DUAL-SCROLL UV ADVECTION
// =============================================================================
// This is the "boiling fluid" effect - samples noise twice with different
// scroll directions and multiplies them together for interference patterns.

float2 DualScrollNoise(float2 uv)
{
    // Layer A: Scroll upward-left
    float2 scrollDirA = float2(-0.3, -1.0);
    scrollDirA = RotateUV(scrollDirA, uScrollAngleA);
    float2 uvA = uv * uNoiseScale + scrollDirA * uTime * uScrollSpeedA;
    float noiseA = tex2D(uNoiseTex, uvA).r;
    
    // Layer B: Scroll upward-right at different speed
    float2 scrollDirB = float2(0.3, -1.0);
    scrollDirB = RotateUV(scrollDirB, uScrollAngleB);
    float2 uvB = uv * uNoiseScale * 1.3 + scrollDirB * uTime * uScrollSpeedB;
    float noiseB = tex2D(uNoiseTex, uvB).r;
    
    // Multiplicative blending creates interference/turbulence
    float combinedNoise = noiseA * noiseB;
    
    // Also return a secondary noise value for extra detail
    float detailNoise = tex2D(uNoiseTex, uv * uNoiseScale * 2.7 + float2(uTime * 0.1, 0)).r;
    
    return float2(combinedNoise, detailNoise);
}

// =============================================================================
// CORE FUNCTION: FRACTAL BROWNIAN MOTION (fBm)
// =============================================================================
// Layered noise for organic, multi-frequency turbulence.

float FBM(float2 uv, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    float lacunarity = 2.0;    // Frequency multiplier per octave
    float persistence = 0.5;   // Amplitude decay per octave
    
    [unroll(4)]
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * tex2D(uNoiseTex, uv * frequency).r;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return value;
}

// =============================================================================
// CORE FUNCTION: UV DISTORTION (WARP)
// =============================================================================
// Uses noise to distort UV coordinates - makes straight edges "wobble" like fire.

float2 WarpedUV(float2 uv, float2 noiseValues, float strength)
{
    // Convert noise from 0-1 to -0.5 to 0.5
    float2 displacement = noiseValues - 0.5;
    
    // Apply distortion perpendicular to the trail
    float2 warpedUV = uv;
    warpedUV.y += displacement.x * strength;           // Horizontal warp
    warpedUV.x += displacement.y * strength * 0.5;     // Slight vertical warp
    
    return warpedUV;
}

// =============================================================================
// CORE FUNCTION: THRESHOLD EROSION
// =============================================================================
// Creates the sharp "licking" flame edges by cutting based on noise threshold.
// Higher threshold = more erosion = more transparent areas.

float ErodeEdge(float noiseValue, float progress, float verticalPos)
{
    // Base threshold increases toward the tail (more erosion at end)
    float threshold = uErosionThreshold + progress * uTailErosion;
    
    // Extra erosion at vertical edges
    float edgeDist = abs(verticalPos - 0.5) * 2.0;
    threshold += edgeDist * 0.2;
    
    // Use smoothstep for organic soft edges
    return SmoothErosion(threshold, uErosionSoftness, noiseValue);
}

// =============================================================================
// CORE FUNCTION: COLOR RAMP TINTING
// =============================================================================
// Maps grayscale brightness to a color gradient (fire: black→red→orange→white).

float3 ApplyColorRamp(float heat, float3 coolColor, float3 hotColor, float3 whiteHotColor)
{
    // Heat is 0 (cold) to 1 (hot)
    float3 color;
    
    if (heat < 0.5)
    {
        // Cool to mid: secondary color → primary color
        color = lerp(coolColor, hotColor, heat * 2.0);
    }
    else
    {
        // Mid to hot: primary color → white-hot
        color = lerp(hotColor, whiteHotColor, (heat - 0.5) * 2.0);
    }
    
    return color;
}

// Alternative: Use LUT texture for color ramp
float3 SampleColorRamp(float heat)
{
    return tex2D(uPaletteLUT, float2(heat, 0.5)).rgb;
}

// =============================================================================
// PASS 1: FLUID FIRE TRAIL
// =============================================================================
// Full implementation: Dual-scroll noise + UV warp + erosion + color ramp.
// This is the main flame effect.

float4 FluidFirePass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // coords.x = progress along trail (0 = head, 1 = tail)
    // coords.y = perpendicular position (0 = edge, 0.5 = center, 1 = edge)
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // STEP 1: DUAL-SCROLL UV ADVECTION
    float2 noiseValues = DualScrollNoise(coords);
    float combinedNoise = noiseValues.x;
    float detailNoise = noiseValues.y;
    
    // STEP 2: UV DISTORTION (WARP)
    // Use noise to distort the UV coordinates for flame "wobble"
    float warpStrength = uDistortionStrength * (1.0 - progress * 0.5);
    float2 warpedUV = WarpedUV(coords, noiseValues, warpStrength);
    
    // STEP 3: SAMPLE BASE TEXTURE with warped UV
    float4 baseColor = tex2D(uImage0, warpedUV);
    
    // STEP 4: THRESHOLD EROSION
    // Create organic, flickering edges
    float erosionMask = ErodeEdge(combinedNoise + detailNoise * 0.3, progress, verticalPos);
    
    // STEP 5: VERTICAL PROFILE (width tapering)
    float verticalFade = QuadraticBump(verticalPos);
    
    // STEP 6: HORIZONTAL FADEOUT (tail fades)
    float tailFade = InverseLerp(0.95, 0.2, progress);
    
    // STEP 7: FLICKER ANIMATION
    float flicker = sin(uTime * uFlickerSpeed + progress * 30.0) * uFlickerIntensity + (1.0 - uFlickerIntensity);
    flicker *= sin(uTime * uFlickerSpeed * 1.7 + verticalPos * 20.0) * (uFlickerIntensity * 0.5) + (1.0 - uFlickerIntensity * 0.5);
    
    // STEP 8: HEAT CALCULATION
    // Heat is highest at center, lowest at edges and tail
    float heat = verticalFade * (1.0 - progress);
    heat = pow(heat, 1.0 / uHeatFalloff);
    heat *= combinedNoise * 0.3 + 0.7; // Noise modulation
    
    // STEP 9: COLOR RAMP APPLICATION
    float3 fireColor = ApplyColorRamp(heat, uSecondaryColor, uColor, uHotColor);
    
    // STEP 10: FINAL COMPOSITE
    float3 finalColor = fireColor * baseColor.rgb * flicker * uIntensity;
    float finalAlpha = erosionMask * verticalFade * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 2: SPECTRAL SHIMMER (Fraunhofer LUT)
// =============================================================================
// Uses spectral LUT for prismatic/iridescent effects like Exo Blade.

float4 SpectralShimmerPass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // Sample base texture
    float4 baseColor = tex2D(uImage0, coords);
    
    // SPECTRAL WAVELENGTH MAPPING
    // Map X coordinate to wavelength position in LUT
    float wavelength = frac(progress * 1.5 + uTime * 0.15 + uLUTOffset);
    float3 spectralColor = tex2D(uPaletteLUT, float2(wavelength, 0.5)).rgb;
    
    // Secondary shimmer based on vertical position
    float verticalWave = sin(verticalPos * 6.28318 + uTime * 4.0);
    float wavelength2 = frac(wavelength + verticalWave * 0.08);
    float3 secondarySpectral = tex2D(uPaletteLUT, float2(wavelength2, 0.5)).rgb;
    
    // Blend spectral colors
    float3 shimmerColor = lerp(spectralColor, secondarySpectral, 0.3);
    
    // FRAUNHOFER LINES (absorption masking)
    // The dark lines in the spectral image create natural flicker
    float spectralIntensity = (spectralColor.r + spectralColor.g + spectralColor.b) / 3.0;
    float absorptionMask = smoothstep(0.1, 0.4, spectralIntensity);
    
    // Edge fadeouts
    float verticalFade = QuadraticBump(verticalPos);
    float tailFade = InverseLerp(0.95, 0.15, progress);
    
    // Intensity pulse
    float pulse = sin(uTime * 5.0 + progress * 12.0) * 0.12 + 0.88;
    
    // FINAL COMPOSITE
    float3 finalColor = shimmerColor * baseColor.rgb * pulse * uIntensity * absorptionMask;
    finalColor += shimmerColor * 0.25; // Additive glow overlay
    
    float finalAlpha = verticalFade * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 3: BLOOM OUTER GLOW
// =============================================================================
// Wide, soft glow layer for additive blending.

float4 BloomGlowPass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // Sample base with slight blur approximation
    float4 baseColor = tex2D(uImage0, coords);
    
    // Very soft falloff for bloom
    float bloomFade = pow(QuadraticBump(verticalPos), 0.4);
    float tailFade = pow(InverseLerp(1.0, 0.0, progress), 0.6);
    
    // Desaturated bloom color
    float3 bloomColor = lerp(uColor, float3(1.0, 1.0, 1.0), 0.4);
    
    // FINAL COMPOSITE
    float3 finalColor = bloomColor * baseColor.rgb * uIntensity * 0.5;
    float finalAlpha = bloomFade * tailFade * uOpacity * 0.35 * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 4: WHITE-HOT CORE
// =============================================================================
// The brightest, tightest center line.

float4 CoreGlowPass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // Sample base texture
    float4 baseColor = tex2D(uImage0, coords);
    
    // Very tight center focus
    float coreFade = pow(QuadraticBump(verticalPos), 2.5);
    float tailFade = InverseLerp(0.85, 0.05, progress);
    
    // White-hot color with slight tint
    float3 coreColor = lerp(uHotColor, float3(1.0, 1.0, 1.0), 0.8);
    
    // Subtle flicker
    float flicker = sin(uTime * uFlickerSpeed * 1.5 + progress * 40.0) * 0.08 + 0.92;
    
    // FINAL COMPOSITE
    float3 finalColor = coreColor * baseColor.rgb * uIntensity * flicker * 1.6;
    float finalAlpha = coreFade * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 5: EROSION EDGE GLOW
// =============================================================================
// Uses erosion pattern to create glowing edge highlights.

float4 ErosionEdgePass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // Get noise for erosion
    float2 noiseValues = DualScrollNoise(coords);
    float erosionMask = ErodeEdge(noiseValues.x, progress, verticalPos);
    
    // Edge detection: glow where erosion transitions
    float edgeGlow = smoothstep(0.3, 0.5, erosionMask) * smoothstep(0.8, 0.6, erosionMask);
    edgeGlow = max(edgeGlow, 0);
    
    // Color is brighter at edges
    float3 edgeColor = lerp(uColor, uHotColor, 0.6);
    
    // Fadeouts
    float tailFade = InverseLerp(0.9, 0.2, progress);
    
    // FINAL COMPOSITE
    float3 finalColor = edgeColor * edgeGlow * uIntensity * 1.2;
    float finalAlpha = edgeGlow * tailFade * uOpacity * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 6: CHROMATIC ABERRATION SHIMMER
// =============================================================================
// RGB channel separation for high-energy effects.

float4 ChromaticPass(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float verticalPos = coords.y;
    
    // Calculate aberration offset based on distance from center
    float dist = abs(verticalPos - 0.5);
    float aberrationStrength = dist * 0.02 * uDistortionStrength;
    
    // Sample RGB channels with offset
    float r = tex2D(uImage0, coords + float2(aberrationStrength, 0)).r;
    float g = tex2D(uImage0, coords).g;
    float b = tex2D(uImage0, coords - float2(aberrationStrength, 0)).b;
    
    float4 baseColor = float4(r, g, b, tex2D(uImage0, coords).a);
    
    // Standard fadeouts
    float verticalFade = QuadraticBump(verticalPos);
    float tailFade = InverseLerp(0.95, 0.2, progress);
    
    // Apply color tint
    float3 tintedColor = baseColor.rgb * lerp(uSecondaryColor, uColor, verticalFade);
    
    // FINAL COMPOSITE
    float3 finalColor = tintedColor * uIntensity;
    float finalAlpha = verticalFade * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FluidFire
{
    pass P0
    {
        PixelShader = compile ps_3_0 FluidFirePass();
    }
}

technique SpectralShimmer
{
    pass P0
    {
        PixelShader = compile ps_3_0 SpectralShimmerPass();
    }
}

technique BloomGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 BloomGlowPass();
    }
}

technique CoreGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 CoreGlowPass();
    }
}

technique ErosionEdge
{
    pass P0
    {
        PixelShader = compile ps_3_0 ErosionEdgePass();
    }
}

technique Chromatic
{
    pass P0
    {
        PixelShader = compile ps_3_0 ChromaticPass();
    }
}
