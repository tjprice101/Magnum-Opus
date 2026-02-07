// =============================================================================
// MagnumOpus PROCEDURAL TRAIL SHADER - Calamity-Style VFX
// =============================================================================
// 
// This shader implements the full Calamity/Ark of the Cosmos VFX pipeline:
// 
// 1. UV ADVECTION - Noise-based distortion for fluid fire effects
// 2. SPECTRAL LUT SAMPLING - Rainbow/prism shimmer via lookup table
// 3. PRIMITIVE MESH INTEGRATION - Works with triangle strip vertex data
// 4. MULTI-PASS RENDERING - Background fog, main trail, bright core
// 5. VERTEX DISPLACEMENT - Optional wave/flicker effects
// 
// TEXTURE SLOTS:
// - uImage0 (s0): Main trail texture (BeamStreak1, etc.)
// - uNoiseTexture (s1): Noise for UV advection (NoiseSmoke)
// - uPaletteLUT (s2): Rainbow/spectral color lookup (RainbowLUT)
// - uMaskTexture (s3): Edge glow mask (EclipseRing)
// 
// =============================================================================

// Texture Samplers
sampler uImage0 : register(s0);        // Main trail texture
sampler uNoiseTexture : register(s1);   // Noise for advection
sampler uPaletteLUT : register(s2);     // Rainbow LUT
sampler uMaskTexture : register(s3);    // Edge mask

// Standard uniforms
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// Advanced parameters
float uNoiseScale;        // Noise sampling scale (default: 3.0)
float uAdvectionStrength; // How much UV is distorted (default: 0.1)
float uLUTOffset;         // Offset into rainbow LUT (default: 0)
float uFlickerSpeed;      // Flame flicker rate (default: 8.0)
float uTrailProgress;     // 0 = start of trail, 1 = end

// =============================================================================
// UTILITY FUNCTIONS (matching VFXUtilities.cs)
// =============================================================================

float QuadraticBump(float x)
{
    // 0 at edges, 1 at center
    // Input 0.0 → 0.0, Input 0.5 → 1.0, Input 1.0 → 0.0
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

float SmoothStep(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

// =============================================================================
// NOISE FUNCTIONS
// =============================================================================

// Fractal Brownian Motion - layered noise for organic detail
float FBM(float2 uv, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 1.0;
    float lacunarity = 2.0;    // Frequency multiplier per octave
    float persistence = 0.5;   // Amplitude decay per octave
    
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * tex2D(uNoiseTexture, uv * frequency).r;
        amplitude *= persistence;
        frequency *= lacunarity;
    }
    
    return value;
}

// =============================================================================
// PASS 1: FLUID FIRE TRAIL (UV Advection + Noise Erosion)
// =============================================================================
// This creates the "living fire" effect where flames appear to flow and flicker.
// The key is NOT moving the texture, but moving the UV LOOKUP with noise.

float4 FluidFireTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // coords.x = progress along trail (0 = head, 1 = tail)
    // coords.y = perpendicular position (0.5 = center)
    
    float progress = coords.x;
    float width = coords.y;
    
    // STEP 1: UV ADVECTION (the secret sauce)
    // Sample noise texture with time offset to create flowing motion
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.y -= uTime * 0.3;  // Flames rise/flow backward
    
    float noiseValue = tex2D(uNoiseTexture, noiseUV).r;
    float noiseValue2 = tex2D(uNoiseTexture, noiseUV * 2.1 + float2(0.5, uTime * 0.2)).r;
    
    // Combine noise layers for turbulent flow
    float combinedNoise = noiseValue * 0.6 + noiseValue2 * 0.4;
    
    // Apply advection - distort the main texture UV
    float2 advectedUV = coords;
    advectedUV += (combinedNoise - 0.5) * uAdvectionStrength;
    
    // STEP 2: SAMPLE MAIN TEXTURE with advected UV
    float4 baseColor = tex2D(uImage0, advectedUV);
    
    // STEP 3: EDGE EROSION (flames have irregular edges)
    float verticalFade = QuadraticBump(width);
    float noisyEdge = SmoothStep(1.0, 0.5 - combinedNoise * 0.3, abs(width - 0.5) * 2.0);
    
    // STEP 4: HORIZONTAL FADEOUT (tail fades)
    float tailFade = InverseLerp(0.9, 0.3, progress);
    
    // STEP 5: FLICKERING (rapid intensity variation)
    float flicker = sin(uTime * uFlickerSpeed + progress * 30.0) * 0.15 + 0.85;
    flicker *= sin(uTime * uFlickerSpeed * 1.7 + coords.y * 20.0) * 0.1 + 0.9;
    
    // STEP 6: COLOR GRADIENT (hot core to cool edges)
    // Use progress and width to determine heat
    float heat = verticalFade * (1.0 - progress);
    float3 fireColor = lerp(uSecondaryColor, uColor, heat);
    fireColor = lerp(fireColor, float3(1.0, 1.0, 0.9), heat * heat * 0.5); // White-hot core
    
    // FINAL COMPOSITE
    float3 finalColor = fireColor * baseColor.rgb * flicker * uIntensity;
    float finalAlpha = noisyEdge * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 2: SPECTRAL SHIMMER (Rainbow LUT Sampling)
// =============================================================================
// Creates prismatic/iridescent effects like the Exo Blade.
// Samples a rainbow gradient based on position and time for shimmer.

float4 SpectralShimmer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float width = coords.y;
    
    // Sample main texture
    float4 baseColor = tex2D(uImage0, coords);
    
    // SPECTRAL LOOKUP
    // The key: sample rainbow LUT using position + time offset
    float wavelength = frac(progress + uTime * 0.2 + uLUTOffset);
    float3 spectralColor = tex2D(uPaletteLUT, float2(wavelength, 0.5)).rgb;
    
    // Apply secondary shimmer based on vertical position
    float verticalWave = sin(width * 6.28318 + uTime * 3.0);
    wavelength = frac(wavelength + verticalWave * 0.1);
    float3 secondarySpectral = tex2D(uPaletteLUT, float2(wavelength, 0.5)).rgb;
    
    // Blend spectral colors
    float3 shimmerColor = lerp(spectralColor, secondarySpectral, 0.3);
    
    // Edge fadeout
    float verticalFade = QuadraticBump(width);
    float tailFade = InverseLerp(0.95, 0.2, progress);
    
    // Intensity pulse
    float pulse = sin(uTime * 4.0 + progress * 10.0) * 0.1 + 0.9;
    
    // FINAL COMPOSITE
    float3 finalColor = shimmerColor * baseColor.rgb * pulse * uIntensity;
    finalColor += shimmerColor * 0.3; // Additive glow
    
    float finalAlpha = verticalFade * tailFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 3: BLOOM GLOW (Soft outer layer)
// =============================================================================
// Wide, soft glow for additive blending pass.

float4 BloomGlow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float width = coords.y;
    
    // Sample main texture
    float4 baseColor = tex2D(uImage0, coords);
    
    // Very soft falloff for bloom
    float bloomFade = pow(QuadraticBump(width), 0.5);
    float tailFade = pow(InverseLerp(1.0, 0.0, progress), 0.5);
    
    // Softer color (less saturated)
    float3 bloomColor = lerp(uColor, float3(1.0, 1.0, 1.0), 0.3);
    
    // FINAL COMPOSITE
    float3 finalColor = bloomColor * baseColor.rgb * uIntensity * 0.6;
    float finalAlpha = bloomFade * tailFade * uOpacity * 0.4 * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 4: WHITE-HOT CORE (Inner bright line)
// =============================================================================
// The brightest center line of the trail.

float4 CoreGlow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float width = coords.y;
    
    // Sample main texture
    float4 baseColor = tex2D(uImage0, coords);
    
    // Very tight center focus
    float coreFade = pow(QuadraticBump(width), 2.0);
    float tailFade = InverseLerp(0.8, 0.1, progress);
    
    // White-hot color
    float3 coreColor = lerp(uColor, float3(1.0, 1.0, 1.0), 0.7);
    
    // FINAL COMPOSITE
    float3 finalColor = coreColor * baseColor.rgb * uIntensity * 1.5;
    float finalAlpha = coreFade * tailFade * uOpacity * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// PASS 5: EDGE GLOW WITH MASK (Using EclipseRing)
// =============================================================================
// Uses the edge mask texture for smooth edge glow.

float4 MaskedEdgeGlow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float progress = coords.x;
    float width = coords.y;
    
    // Sample main texture and mask
    float4 baseColor = tex2D(uImage0, coords);
    float maskValue = tex2D(uMaskTexture, float2(width, progress)).r;
    
    // Color blend
    float3 glowColor = lerp(uSecondaryColor, uColor, maskValue);
    
    // Fadeouts
    float tailFade = InverseLerp(0.95, 0.3, progress);
    
    // FINAL COMPOSITE
    float3 finalColor = glowColor * baseColor.rgb * maskValue * uIntensity;
    float finalAlpha = maskValue * tailFade * uOpacity * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FluidFire
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 FluidFireTrail();
    }
}

technique Spectral
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 SpectralShimmer();
    }
}

technique Bloom
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 BloomGlow();
    }
}

technique Core
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 CoreGlow();
    }
}

technique MaskedEdge
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 MaskedEdgeGlow();
    }
}
