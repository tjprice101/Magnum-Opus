// =============================================================================
// MagnumOpus ADVANCED SCREEN DISTORTION SHADER
// =============================================================================
// 
// Screen-space post-processing effects using proper texture lookups:
// 
// 1. RIPPLE DISTORTION - Uses RippleRing texture for wave patterns
// 2. HEAT DISTORTION - Uses noise texture for heat shimmer
// 3. CHROMATIC ABERRATION - RGB channel separation
// 4. ECLIPSE VIGNETTE - Uses EclipseRing for edge darkening
// 5. REALITY TEAR - Combines multiple effects for "spacetime" ripping
// 
// TEXTURE SLOTS:
// - uImage0 (s0): Screen capture (render target)
// - uDistortionMask (s1): Ripple/Eclipse ring texture
// - uNoiseTexture (s2): Noise for heat/turbulence
// 
// USAGE: Include shared utility library for noise, SDFs, color utilities:
// #include "HLSLLibrary.fxh"
// (Uncomment above line after compiling library into your build pipeline)
// 
// =============================================================================

sampler uImage0 : register(s0);           // Screen texture
sampler uDistortionMask : register(s1);   // RippleRing or EclipseRing
sampler uNoiseTexture : register(s2);     // NoiseSmoke

// Uniforms
float2 uScreenResolution;
float2 uTargetPosition;      // Effect center (0-1 screen coords)
float2 uSecondaryPosition;   // Secondary effect point
float3 uColor;
float3 uSecondaryColor;
float uIntensity;
float uTime;
float uRadius;               // Effect radius (0-1 screen units)
float uDistortionStrength;   // How much UV is displaced

// =============================================================================
// UTILITY FUNCTIONS
// Note: These are duplicated from HLSLLibrary.fxh for standalone compilation.
// When integrating with the full library, remove these and use #include.
// =============================================================================

float2 ScreenToUV(float2 screenPos)
{
    return screenPos / uScreenResolution;
}

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// =============================================================================
// EFFECT 1: RIPPLE WAVE DISTORTION
// =============================================================================
// Creates expanding wave rings from an impact point.
// The RippleRing texture provides the wave pattern.

float4 RippleDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Normalize distance to effect radius
    float normalizedDist = dist / uRadius;
    
    if (normalizedDist > 1.0)
    {
        // Outside effect radius - no distortion
        return tex2D(uImage0, coords) * sampleColor;
    }
    
    // Sample ripple texture using distance as X coordinate
    // This gives us concentric rings
    float2 rippleUV = float2(normalizedDist, 0.5);
    float rippleValue = tex2D(uDistortionMask, rippleUV).r;
    
    // Animate the ripple outward
    float animatedRipple = sin((normalizedDist - uTime * 0.5) * 30.0);
    animatedRipple *= rippleValue;
    animatedRipple *= (1.0 - normalizedDist); // Fade at edges
    
    // Apply distortion along the direction from center
    float2 normalizedDir = normalize(direction);
    float2 distortedCoords = coords + normalizedDir * animatedRipple * uDistortionStrength;
    
    // Clamp to valid UV range
    distortedCoords = clamp(distortedCoords, 0.0, 1.0);
    
    return tex2D(uImage0, distortedCoords) * sampleColor;
}

// =============================================================================
// EFFECT 2: HEAT SHIMMER DISTORTION
// =============================================================================
// Uses noise texture for organic heat-haze effect.

float4 HeatShimmer(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    float normalizedDist = dist / uRadius;
    
    if (normalizedDist > 1.0)
    {
        return tex2D(uImage0, coords) * sampleColor;
    }
    
    // Sample noise with time for animated distortion
    float2 noiseUV = coords * 5.0;
    noiseUV.y -= uTime * 0.5; // Heat rises
    
    float noise1 = tex2D(uNoiseTexture, noiseUV).r;
    float noise2 = tex2D(uNoiseTexture, noiseUV * 1.5 + float2(0.3, uTime * 0.3)).r;
    
    // Combine for turbulent effect
    float2 distortion = float2(
        (noise1 - 0.5) * 2.0,
        (noise2 - 0.5) * 2.0
    );
    
    // Falloff from center
    float falloff = 1.0 - normalizedDist;
    falloff = pow(falloff, 0.5);
    
    // Apply distortion
    float2 distortedCoords = coords + distortion * uDistortionStrength * falloff;
    distortedCoords = clamp(distortedCoords, 0.0, 1.0);
    
    return tex2D(uImage0, distortedCoords) * sampleColor;
}

// =============================================================================
// EFFECT 3: CHROMATIC ABERRATION
// =============================================================================
// Separates RGB channels for impact/damage effects.

float4 ChromaticAberration(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Aberration strength falls off with distance
    float strength = uIntensity * 0.02 * saturate(1.0 - dist * 2.0);
    
    // Offset each channel differently
    float2 redOffset = direction * strength;
    float2 greenOffset = float2(0, 0);
    float2 blueOffset = direction * -strength;
    
    float r = tex2D(uImage0, coords + redOffset).r;
    float g = tex2D(uImage0, coords + greenOffset).g;
    float b = tex2D(uImage0, coords + blueOffset).b;
    float a = tex2D(uImage0, coords).a;
    
    return float4(r, g, b, a) * sampleColor;
}

// =============================================================================
// EFFECT 4: ECLIPSE VIGNETTE
// =============================================================================
// Uses EclipseRing texture for dramatic edge darkening.

float4 EclipseVignette(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Sample eclipse mask from center
    float2 centerOffset = coords - float2(0.5, 0.5);
    float dist = length(centerOffset) * 2.0; // 0-1 from center to edge
    
    float2 maskUV = float2(dist, 0.5);
    float maskValue = tex2D(uDistortionMask, maskUV).r;
    
    // Invert for vignette (dark edges)
    float vignette = lerp(1.0, maskValue, uIntensity);
    
    // Optional color tint
    float3 tintedColor = lerp(baseColor.rgb * vignette, uColor * vignette, uIntensity * 0.3);
    
    return float4(tintedColor, baseColor.a) * sampleColor;
}

// =============================================================================
// EFFECT 5: REALITY TEAR (Combined effect for Fate theme)
// =============================================================================
// Combines ripple, chromatic aberration, and noise for "spacetime ripping".

float4 RealityTear(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    float normalizedDist = dist / uRadius;
    
    if (normalizedDist > 1.0)
    {
        return tex2D(uImage0, coords) * sampleColor;
    }
    
    // Noise-based distortion
    float2 noiseUV = coords * 3.0 + float2(uTime * 0.2, uTime * 0.1);
    float noise = tex2D(uNoiseTexture, noiseUV).r;
    
    // Ripple from mask
    float ripple = tex2D(uDistortionMask, float2(normalizedDist, 0.5)).r;
    float animatedRipple = sin((normalizedDist - uTime) * 20.0) * ripple;
    
    // Combined distortion
    float falloff = pow(1.0 - normalizedDist, 2.0);
    float2 distortion = normalize(direction) * (animatedRipple + (noise - 0.5) * 0.5);
    distortion *= uDistortionStrength * falloff;
    
    // Chromatic split
    float chromaStrength = uIntensity * 0.015 * falloff;
    
    float2 redCoords = coords + distortion + direction * chromaStrength;
    float2 greenCoords = coords + distortion;
    float2 blueCoords = coords + distortion - direction * chromaStrength;
    
    redCoords = clamp(redCoords, 0.0, 1.0);
    greenCoords = clamp(greenCoords, 0.0, 1.0);
    blueCoords = clamp(blueCoords, 0.0, 1.0);
    
    float r = tex2D(uImage0, redCoords).r;
    float g = tex2D(uImage0, greenCoords).g;
    float b = tex2D(uImage0, blueCoords).b;
    
    // Color tint toward theme color at center
    float3 finalColor = float3(r, g, b);
    finalColor = lerp(finalColor, uColor, falloff * 0.3);
    
    // Brightness flash at center
    finalColor += uSecondaryColor * falloff * falloff * uIntensity;
    
    return float4(finalColor, 1.0) * sampleColor;
}

// =============================================================================
// EFFECT 6: LINE DISTORTION (for beam/laser effects)
// =============================================================================
// Distorts along a line between two points.

float4 LineDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Line from uTargetPosition to uSecondaryPosition
    float2 lineDir = uSecondaryPosition - uTargetPosition;
    float lineLength = length(lineDir);
    lineDir /= lineLength;
    
    // Project point onto line
    float2 toPoint = coords - uTargetPosition;
    float projection = dot(toPoint, lineDir);
    projection = clamp(projection, 0.0, lineLength);
    
    float2 closestPoint = uTargetPosition + lineDir * projection;
    float distToLine = length(coords - closestPoint);
    
    // Normalize distance
    float normalizedDist = distToLine / uRadius;
    
    if (normalizedDist > 1.0)
    {
        return tex2D(uImage0, coords) * sampleColor;
    }
    
    // Perpendicular direction
    float2 perpDir = float2(-lineDir.y, lineDir.x);
    
    // Wave distortion along line
    float wave = sin(projection * 30.0 - uTime * 10.0);
    wave *= tex2D(uNoiseTexture, float2(projection / lineLength, 0.5)).r;
    
    float falloff = 1.0 - normalizedDist;
    float2 distortion = perpDir * wave * uDistortionStrength * falloff;
    
    float2 distortedCoords = clamp(coords + distortion, 0.0, 1.0);
    
    return tex2D(uImage0, distortedCoords) * sampleColor;
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique Ripple
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 RippleDistortion();
    }
}

technique Heat
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 HeatShimmer();
    }
}

technique Chromatic
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 ChromaticAberration();
    }
}

technique Eclipse
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 EclipseVignette();
    }
}

technique Reality
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 RealityTear();
    }
}

technique Line
{
    pass MainPass
    {
        PixelShader = compile ps_3_0 LineDistortion();
    }
}
