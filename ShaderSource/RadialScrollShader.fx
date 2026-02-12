// ============================================================================
// RADIAL SCROLL SHADER - MagnumOpus VFX System
// ============================================================================
// Multi-technique radial scrolling shader with theme presets.
// Supports: Orbs, Portals, Auras, Energy Fields, Vortexes
//
// COMPILE: fxc /T ps_2_0 /E PSRadialScroll /Fo RadialScrollShader.fxb RadialScrollShader.fx
// ============================================================================

// --- SAMPLERS ---
sampler2D uImage0 : register(s0);  // Primary noise/caustic texture (MUST tile seamlessly)
sampler2D uImage1 : register(s1);  // Secondary noise (optional, for dual-layer)
sampler2D uImage2 : register(s2);  // Gradient LUT (optional, for color mapping)

// --- PARAMETERS ---
float uTime;                        // Animated time (Main.GlobalTimeWrappedHourly)
float uOpacity;                     // Master opacity (0-1)
float4 uColor;                      // Primary tint color
float4 uSecondaryColor;             // Secondary color for gradients

// Radial scroll parameters
float uFlowSpeed;                   // Angular scroll speed (default: 0.5)
float uRadialSpeed;                 // Radial scroll speed (default: 0.2)
float uDistortStrength;             // Noise distortion amount (0-0.1)
float uZoom;                        // UV zoom factor (default: 1.0)
float uRepeat;                      // Angle repeat count (default: 1.0)

// Vignette parameters
float uVignetteSize;                // Vignette start radius (0-1)
float uVignetteBlend;               // Vignette blend width (0-1)

// Additional controls
float uPulseSpeed;                  // Pulsing animation speed
float uPulseAmount;                 // Pulsing intensity (0-0.5)
float uLayerOffset;                 // Layer separation for multi-layer

// ============================================================================
// CONSTANTS
// ============================================================================
#define TAU 6.28318530718
#define PI 3.14159265359

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

// GLSL-compatible modulo (handles negatives correctly)
float glslmod(float x, float y)
{
    return x - y * floor(x / y);
}

float2 glslmod2(float2 x, float2 y)
{
    return x - y * floor(x / y);
}

// Convert Cartesian UV to Polar coordinates
// Returns: float2(radius, angle) where angle is 0-1 (normalized from 0-TAU)
float2 ToPolar(float2 uv, float2 center, float zoom, float repeat)
{
    float2 offset = uv - center;
    
    // Radius: distance from center (affected by zoom)
    float radius = length(offset) * zoom;
    
    // Angle: 0 to TAU, normalized to 0-1, with repeat
    float angle = atan2(offset.y, offset.x);
    angle = glslmod(angle, TAU);  // Ensure positive
    angle = (angle / TAU) * repeat;  // Normalize and repeat
    
    return float2(radius, angle);
}

// Smooth vignette falloff
float Vignette(float radius, float size, float blend)
{
    return 1.0 - smoothstep(size - blend, size + blend, radius);
}

// QuadraticBump: 0→1→0 curve (peaks at 0.5)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// ============================================================================
// TECHNIQUE 1: BASIC RADIAL SCROLL
// ============================================================================
// Simple single-layer radial scrolling with vignette
float4 PSRadialBasic(float2 uv : TEXCOORD0) : COLOR0
{
    // Convert to polar coordinates
    float2 polar = ToPolar(uv, float2(0.5, 0.5), uZoom, uRepeat);
    
    // Animate: scroll angle (θ) and optionally radius
    float2 scrollUV = float2(
        polar.x + uTime * uRadialSpeed,     // Radial scroll
        polar.y + uTime * uFlowSpeed        // Angular scroll
    );
    
    // Sample noise texture
    float4 noise = tex2D(uImage0, scrollUV);
    
    // Apply vignette
    float vignette = Vignette(polar.x, uVignetteSize, uVignetteBlend);
    
    // Color and output
    float4 result = noise * uColor;
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 2: DUAL-PHASE RADIAL SCROLL (Seamless)
// ============================================================================
// Uses Catlike Coding dual-phase blending to eliminate seam artifacts
float4 PSDualPhase(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 polar = ToPolar(uv, center, uZoom, uRepeat);
    
    // Phase 0: Normal time
    float2 uv0 = float2(
        polar.x + uTime * uRadialSpeed,
        polar.y + uTime * uFlowSpeed
    );
    
    // Phase 1: Offset by half period
    float2 uv1 = float2(
        polar.x + uTime * uRadialSpeed + 0.5,
        polar.y + uTime * uFlowSpeed + 0.5
    );
    
    // Sample both phases
    float4 sample0 = tex2D(uImage0, uv0);
    float4 sample1 = tex2D(uImage0, uv1);
    
    // Blend factor oscillates 0→1→0
    float blend = abs(frac(uTime * uFlowSpeed) * 2.0 - 1.0);
    
    // Seamless interpolation
    float4 noise = lerp(sample0, sample1, blend);
    
    // Vignette
    float vignette = Vignette(polar.x, uVignetteSize, uVignetteBlend);
    
    // Apply color
    float4 result = noise * uColor;
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 3: DISTORTION-ENHANCED RADIAL
// ============================================================================
// Uses secondary noise to distort the primary sample
float4 PSDistorted(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 polar = ToPolar(uv, center, uZoom, uRepeat);
    
    // Sample distortion noise (slower, different direction)
    float2 distortUV = float2(
        polar.x * 2.0 - uTime * uRadialSpeed * 0.5,
        polar.y - uTime * uFlowSpeed * 0.3
    );
    float2 distortion = tex2D(uImage1, distortUV).rg * 2.0 - 1.0;
    
    // Apply distortion to polar coordinates
    float2 distortedPolar = polar + distortion * uDistortStrength;
    
    // Main sample with distortion
    float2 scrollUV = float2(
        distortedPolar.x + uTime * uRadialSpeed,
        distortedPolar.y + uTime * uFlowSpeed
    );
    
    float4 noise = tex2D(uImage0, scrollUV);
    
    // Vignette with slight expansion from distortion
    float vignette = Vignette(polar.x, uVignetteSize, uVignetteBlend);
    
    float4 result = noise * uColor;
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 4: MULTI-LAYER RADIAL (Depth Effect)
// ============================================================================
// Three layers at different speeds for parallax-like depth
float4 PSMultiLayer(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 polar = ToPolar(uv, center, uZoom, uRepeat);
    
    // Layer 1: Slow background
    float2 uv1 = float2(
        polar.x * 0.8 + uTime * uRadialSpeed * 0.3,
        polar.y + uTime * uFlowSpeed * 0.5
    );
    float4 layer1 = tex2D(uImage0, uv1) * 0.3;
    
    // Layer 2: Medium midground
    float2 uv2 = float2(
        polar.x + uTime * uRadialSpeed * 0.7,
        polar.y + uTime * uFlowSpeed
    );
    float4 layer2 = tex2D(uImage0, uv2 + float2(0.33, 0.17)) * 0.5;
    
    // Layer 3: Fast foreground
    float2 uv3 = float2(
        polar.x * 1.2 + uTime * uRadialSpeed,
        polar.y + uTime * uFlowSpeed * 1.5
    );
    float4 layer3 = tex2D(uImage0, uv3 + float2(0.67, 0.41)) * 0.7;
    
    // Combine layers (additive for glow)
    float4 combined = layer1 + layer2 + layer3;
    combined = saturate(combined);
    
    // Vignette
    float vignette = Vignette(polar.x, uVignetteSize, uVignetteBlend);
    
    float4 result = combined * uColor;
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 5: GRADIENT-MAPPED RADIAL
// ============================================================================
// Uses LUT texture for theme-based color mapping
float4 PSGradientMapped(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 polar = ToPolar(uv, center, uZoom, uRepeat);
    
    // Animated radial scroll
    float2 scrollUV = float2(
        polar.x + uTime * uRadialSpeed,
        polar.y + uTime * uFlowSpeed
    );
    
    // Sample noise (use R channel as lookup)
    float noiseValue = tex2D(uImage0, scrollUV).r;
    
    // Add radial gradient influence
    float radialInfluence = 1.0 - saturate(polar.x / uVignetteSize);
    float lookupValue = saturate(noiseValue * 0.7 + radialInfluence * 0.3);
    
    // Sample gradient LUT (horizontal 1D lookup)
    float4 gradientColor = tex2D(uImage2, float2(lookupValue, 0.5));
    
    // Blend with primary color
    float4 result = lerp(uColor, gradientColor, noiseValue);
    
    // Vignette
    float vignette = Vignette(polar.x, uVignetteSize, uVignetteBlend);
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 6: PULSING ENERGY ORB
// ============================================================================
// Animated pulsing with breathing effect
float4 PSPulsingOrb(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    
    // Pulsing zoom
    float pulse = 1.0 + sin(uTime * uPulseSpeed) * uPulseAmount;
    float2 polar = ToPolar(uv, center, uZoom * pulse, uRepeat);
    
    // Dual-phase for seamless animation
    float2 uv0 = float2(polar.x + uTime * uRadialSpeed, polar.y + uTime * uFlowSpeed);
    float2 uv1 = uv0 + float2(0.5, 0.5);
    
    float4 s0 = tex2D(uImage0, uv0);
    float4 s1 = tex2D(uImage0, uv1);
    float blend = abs(frac(uTime * uFlowSpeed) * 2.0 - 1.0);
    float4 noise = lerp(s0, s1, blend);
    
    // Pulsing brightness
    float brightness = 1.0 + sin(uTime * uPulseSpeed * 2.0) * 0.2;
    
    // Edge glow intensifies with pulse
    float edgeGlow = QuadraticBump(polar.x / uVignetteSize);
    
    // Vignette
    float vignette = Vignette(polar.x, uVignetteSize * pulse, uVignetteBlend);
    
    // Combine
    float4 result = noise * uColor * brightness;
    result.rgb += uSecondaryColor.rgb * edgeGlow * 0.5;
    result.a *= vignette * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUE 7: VORTEX / PORTAL
// ============================================================================
// Stronger angular distortion for swirling vortex effect
float4 PSVortex(float2 uv : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 offset = uv - center;
    float radius = length(offset);
    
    // Vortex twist: angle increases with distance from center
    float twist = radius * uDistortStrength * 10.0;
    float angle = atan2(offset.y, offset.x) + twist + uTime * uFlowSpeed;
    
    // Reconstruct polar with twisted angle
    float2 polar = float2(
        radius * uZoom,
        glslmod(angle, TAU) / TAU * uRepeat
    );
    
    // Inward radial flow (negative = toward center)
    float2 scrollUV = float2(
        polar.x - uTime * uRadialSpeed,  // Inward
        polar.y + uTime * uFlowSpeed
    );
    
    float4 noise = tex2D(uImage0, scrollUV);
    
    // Dark center (portal depth)
    float centerDark = smoothstep(0.0, 0.3, radius);
    
    // Edge glow
    float edgeGlow = smoothstep(uVignetteSize, uVignetteSize - 0.1, radius);
    
    float4 result = noise * uColor * centerDark;
    result.rgb += uSecondaryColor.rgb * edgeGlow * 0.7;
    result.a *= Vignette(polar.x, uVignetteSize, uVignetteBlend) * uOpacity;
    
    return result;
}

// ============================================================================
// TECHNIQUES
// ============================================================================
technique RadialBasic
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSRadialBasic();
    }
}

technique DualPhase
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSDualPhase();
    }
}

technique Distorted
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSDistorted();
    }
}

technique MultiLayer
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSMultiLayer();
    }
}

technique GradientMapped
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSGradientMapped();
    }
}

technique PulsingOrb
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSPulsingOrb();
    }
}

technique Vortex
{
    pass P0
    {
        PixelShader = compile ps_2_0 PSVortex();
    }
}
