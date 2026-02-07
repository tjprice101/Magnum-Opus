// =============================================================================
// MagnumOpus Nebula Fog Shader - Exo-Style Constellation Fog Effect
// =============================================================================
// Creates shimmering, shifting fog/gas cloud effects using dual-layer noise
// Inspired by Calamity's Exoblade constellation trails and Ark of the Cosmos fog.
//
// Core Techniques:
// - Dual Perlin noise sampling with different scroll velocities
// - Multiplicative noise combination for interference patterns
// - Radial vertex masking for soft circular cloud shapes
// - Thresholding for "sparkle" star effects within fog
// - HDR simulation via over-brightening and multi-pass bloom
// =============================================================================

// Texture samplers
sampler uImage0 : register(s0);           // Base texture (typically soft glow)
sampler uNoiseSampler : register(s1);     // Perlin/Simplex noise texture
sampler uPaletteSampler : register(s2);   // Color palette texture (horizontal gradient)
sampler uVoronoiSampler : register(s3);   // Optional Voronoi noise for secondary patterns

// Shader parameters
float uTime;                    // GlobalTimeWrappedHourly from C#
float uOpacity;                 // Overall opacity multiplier
float uIntensity;               // Brightness/intensity multiplier (>1 for HDR simulation)
float3 uColor;                  // Primary fog color
float3 uSecondaryColor;         // Secondary fog color for gradients
float uDistortionStrength;      // UV distortion intensity (0.0 - 0.3)
float uSparkleThreshold;        // Noise threshold for sparkle points (0.7 - 0.95)
float uSparkleIntensity;        // How bright sparkles are
float2 uScrollVelocity1;        // Noise scroll direction 1 (e.g., 0.1, 0.05)
float2 uScrollVelocity2;        // Noise scroll direction 2 (e.g., -0.05, 0.1)
float uNoiseScale;              // Noise UV multiplier (1.0 - 3.0)
float uRadialFalloff;           // Power for radial mask (1.0 = linear, 2.0 = smooth)
float uPulseSpeed;              // Breathing pulse speed
float uPulseAmount;             // Breathing pulse intensity (0.0 - 0.3)

// =============================================================================
// Utility Functions
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float smoothstepCustom(float edge0, float edge1, float x)
{
    float t = saturate((x - edge0) / (edge1 - edge0));
    return t * t * (3.0 - 2.0 * t);
}

// Remaps value from one range to another
float remap(float value, float inMin, float inMax, float outMin, float outMax)
{
    return outMin + (value - inMin) * (outMax - outMin) / (inMax - inMin);
}

// =============================================================================
// Main Nebula Fog Shader
// =============================================================================
// Creates the wavering, shifting fog effect using dual noise layers.
float4 NebulaFogShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float time = uTime * 0.5;
    
    // === DUAL NOISE SAMPLING ===
    // Sample noise at two different UV offsets scrolling in different directions
    // This creates the characteristic "interference pattern" shimmer
    float2 noiseUV = coords * uNoiseScale;
    float2 offset1 = uScrollVelocity1 * time;
    float2 offset2 = uScrollVelocity2 * time;
    
    float noise1 = tex2D(uNoiseSampler, noiseUV + offset1).r;
    float noise2 = tex2D(uNoiseSampler, noiseUV + offset2).r;
    
    // Multiplicative combination creates wavering effect
    float combinedNoise = noise1 * noise2;
    
    // === OPTIONAL UV DISTORTION (Constellation Effect) ===
    // Distort the UV based on low-frequency noise for "gravity" feel
    if (uDistortionStrength > 0.001)
    {
        float2 distortNoise = tex2D(uNoiseSampler, coords * 0.5 + time * 0.1).rg;
        distortNoise = (distortNoise - 0.5) * 2.0; // Remap to -1 to 1
        noiseUV += distortNoise * uDistortionStrength;
        
        // Re-sample with distorted coords for final value
        noise1 = tex2D(uNoiseSampler, noiseUV + offset1).r;
        noise2 = tex2D(uNoiseSampler, noiseUV + offset2).r;
        combinedNoise = noise1 * noise2;
    }
    
    // === RADIAL MASK ===
    // Creates soft circular boundary - center is opaque, edges are transparent
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);
    float radialMask = smoothstepCustom(0.5, 0.1, dist);
    radialMask = pow(radialMask, uRadialFalloff);
    
    // === BREATHING PULSE ===
    float pulse = 1.0 + sin(uTime * uPulseSpeed) * uPulseAmount;
    
    // === COLOR MAPPING ===
    // Map the noise value to a color gradient
    float3 fogColor = lerp(uSecondaryColor, uColor, combinedNoise);
    
    // Optional: Sample from palette texture for complex gradients
    // float3 paletteColor = tex2D(uPaletteSampler, float2(combinedNoise, 0.5)).rgb;
    // fogColor = lerp(fogColor, paletteColor, 0.5);
    
    // === SPARKLE/STAR THRESHOLD ===
    // When noise exceeds threshold, create bright sparkle points
    float sparkle = 0.0;
    if (combinedNoise > uSparkleThreshold)
    {
        float sparkleStrength = (combinedNoise - uSparkleThreshold) / (1.0 - uSparkleThreshold);
        sparkle = sparkleStrength * uSparkleIntensity;
        fogColor = lerp(fogColor, float3(1.0, 1.0, 1.0), sparkle * 0.7);
    }
    
    // === HDR SIMULATION ===
    // Multiply intensity > 1.0 for over-brightening effect
    float3 finalColor = fogColor * baseColor.rgb * uIntensity * pulse;
    finalColor += sparkle * float3(1.2, 1.3, 1.5); // Add blue-white sparkle glow
    
    // === FINAL ALPHA ===
    float finalAlpha = combinedNoise * radialMask * uOpacity * sampleColor.a * baseColor.a;
    finalAlpha = saturate(finalAlpha + sparkle * 0.5);
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// Constellation Fog Shader (Advanced with Voronoi)
// =============================================================================
// Enhanced version with Voronoi noise for more organic star patterns
float4 ConstellationFogShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float time = uTime * 0.4;
    
    // === TRIPLE NOISE LAYER ===
    float2 noiseUV = coords * uNoiseScale;
    float noise1 = tex2D(uNoiseSampler, noiseUV + uScrollVelocity1 * time).r;
    float noise2 = tex2D(uNoiseSampler, noiseUV + uScrollVelocity2 * time).r;
    float voronoi = tex2D(uVoronoiSampler, noiseUV * 1.5 + time * 0.2).r;
    
    // Complex combination for nebula-like swirls
    float combinedNoise = noise1 * noise2 * 0.7 + voronoi * 0.3;
    combinedNoise = pow(combinedNoise, 0.8); // Boost contrast
    
    // === RADIAL MASK WITH SOFTNESS ===
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);
    float radialMask = 1.0 - smoothstepCustom(0.2, 0.55, dist);
    
    // === COLOR WITH PALETTE ===
    float3 gradientColor = lerp(uSecondaryColor, uColor, pow(combinedNoise, 0.7));
    
    // Add subtle hue shift based on noise
    float hueShift = sin(combinedNoise * 6.28318 + uTime) * 0.1;
    gradientColor.r += hueShift;
    gradientColor.b -= hueShift * 0.5;
    
    // === CONSTELLATION STARS ===
    // Voronoi creates distinct cell boundaries - use these as star positions
    float starMask = step(0.85, voronoi);
    float starBrightness = voronoi * starMask * 2.0;
    
    // Sparkle animation on stars
    float twinkle = sin(uTime * 8.0 + voronoi * 50.0) * 0.3 + 0.7;
    starBrightness *= twinkle;
    
    float3 starColor = float3(1.0, 0.95, 1.1) * starBrightness;
    
    // === COMBINE FOG AND STARS ===
    float pulse = 1.0 + sin(uTime * uPulseSpeed) * uPulseAmount;
    float3 finalColor = gradientColor * uIntensity * pulse + starColor;
    finalColor *= baseColor.rgb;
    
    float finalAlpha = (combinedNoise * 0.8 + starMask * 0.5) * radialMask * uOpacity * sampleColor.a;
    
    return float4(finalColor, saturate(finalAlpha));
}

// =============================================================================
// Bloom Pass for Fog (Multi-layer additive)
// =============================================================================
float4 FogBloomShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float time = uTime * 0.3;
    
    // Soft noise for bloom variation
    float noise = tex2D(uNoiseSampler, coords * uNoiseScale * 0.5 + time * 0.1).r;
    
    // Very soft radial falloff
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);
    float bloomMask = pow(1.0 - saturate(dist * 1.5), 0.5);
    
    // Bloom color is softer/more saturated version of main color
    float3 bloomColor = lerp(uColor, uSecondaryColor, 0.3) * 0.6;
    bloomColor *= noise * 0.5 + 0.5; // Vary brightness with noise
    
    float bloomAlpha = bloomMask * uOpacity * 0.35 * sampleColor.a * baseColor.a;
    
    return float4(bloomColor * uIntensity * 0.5, bloomAlpha);
}

// =============================================================================
// Trail Fog Shader (For projectile/weapon trails with fog overlay)
// =============================================================================
float4 TrailFogShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float time = uTime;
    
    // coords.x = progress along trail (0 = start, 1 = tip)
    // coords.y = width (0 and 1 = edges, 0.5 = center)
    
    float horizontalProgress = coords.x;
    float verticalCenter = QuadraticBump(coords.y);
    
    // === SCROLLING NOISE (follows trail) ===
    float2 noiseUV = coords * float2(3.0, 1.5); // Stretch along trail
    noiseUV.x -= time * 2.0; // Scroll toward trail start
    
    float noise1 = tex2D(uNoiseSampler, noiseUV + uScrollVelocity1 * time * 0.5).r;
    float noise2 = tex2D(uNoiseSampler, noiseUV * 1.3 + uScrollVelocity2 * time * 0.3).r;
    float combinedNoise = noise1 * noise2;
    
    // === TRAIL SHAPE MASK ===
    float edgeFade = verticalCenter;
    float tipFade = smoothstepCustom(0.0, 0.2, horizontalProgress); // Fade at start
    tipFade *= smoothstepCustom(1.0, 0.7, horizontalProgress); // Slight fade at tip
    
    float trailMask = edgeFade * tipFade;
    
    // === COLOR GRADIENT ALONG TRAIL ===
    float3 trailColor = lerp(uSecondaryColor, uColor, horizontalProgress);
    trailColor = lerp(trailColor, float3(1.0, 1.0, 1.0), combinedNoise * 0.4); // Hot spots
    
    // === APPLY ===
    float pulse = 1.0 + sin(time * 6.0 + horizontalProgress * 4.0) * 0.15;
    float3 finalColor = trailColor * baseColor.rgb * uIntensity * pulse;
    float finalAlpha = combinedNoise * trailMask * uOpacity * sampleColor.a;
    
    return float4(finalColor, finalAlpha);
}

// =============================================================================
// Technique Definitions
// =============================================================================
technique Technique1
{
    pass NebulaFogPass
    {
        PixelShader = compile ps_3_0 NebulaFogShader();
    }
    
    pass ConstellationFogPass
    {
        PixelShader = compile ps_3_0 ConstellationFogShader();
    }
    
    pass FogBloomPass
    {
        PixelShader = compile ps_3_0 FogBloomShader();
    }
    
    pass TrailFogPass
    {
        PixelShader = compile ps_3_0 TrailFogShader();
    }
}
