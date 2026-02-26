// =============================================================================
// MagnumOpus Refraction Ripple Shader - PS 2.0 Compatible
// =============================================================================
// Screen-space prismatic ripple effect for Moonlight's Calling  E"The Serenade".
// Creates expanding concentric rings with spectral chromatic aberration,
// simulating the visual distortion of light refracting through water or crystal.
//
// Visual concept: When a beam bounces off a surface, the impact point
// sends out prismatic ripples  Elike dropping a prism into still water.
// Each ring separates light into spectral components.
//
// UV Layout:
//   U (coords.x) = horizontal position on quad
//   V (coords.y) = vertical position on quad
//   Center of effect at (0.5, 0.5)
//
// Techniques:
//   RefractionRippleMain    EFull prismatic ripple with spectral splitting
//   RefractionRippleSubtle  ELighter version for ambient/hold effects
// =============================================================================

sampler uImage0 : register(s0); // Base texture (glow circle / soft glow)
sampler uImage1 : register(s1); // Optional noise texture

float3 uColor;           // Primary ring color
float3 uSecondaryColor;  // Secondary ring color
float uOpacity;          // Overall opacity (fades as ripple expands)
float uTime;             // Animation time
float uIntensity;        // Ring brightness (scales with bounce count)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Ring expansion speed
float uNoiseScale;        // Noise modulation scale
float uDistortionAmt;     // Chromatic separation strength
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;
float uPhase;             // Ripple age (0 = just created, 1 = fully expanded/faded)

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// HSL hue to RGB
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

// =============================================================================
// TECHNIQUE 1: REFRACTION RIPPLE MAIN
// =============================================================================

float4 RefractionRipplePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Distance from center
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);
    float2 dir = normalize(delta + 0.001);

    // --- Expanding ring pattern ---
    float expansionTime = uTime * uScrollSpeed;

    // Multiple concentric rings expanding outward
    float ringCount = 3.0 + uIntensity * 2.0;
    float ringSpacing = 0.12 + uPhase * 0.08;

    float ringAccum = 0.0;
    float spectralAccum = 0.0;

    // Ring 1: Primary expanding ring
    float ring1Dist = frac(dist * 4.0 - expansionTime * 1.5);
    float ring1 = exp(-pow((ring1Dist - 0.5) * 8.0, 2.0));
    ringAccum += ring1 * 0.8;

    // Ring 2: Secondary ring (slightly delayed)
    float ring2Dist = frac(dist * 3.5 - expansionTime * 1.2 + 0.3);
    float ring2 = exp(-pow((ring2Dist - 0.5) * 6.0, 2.0));
    ringAccum += ring2 * 0.5;

    // Ring 3: Tertiary ring (more delayed)
    float ring3Dist = frac(dist * 3.0 - expansionTime * 0.9 + 0.6);
    float ring3 = exp(-pow((ring3Dist - 0.5) * 5.0, 2.0));
    ringAccum += ring3 * 0.3;

    // --- Chromatic aberration per ring ---
    // Each ring separates red/green/blue at different radial offsets
    float chromaticStr = uDistortionAmt * (1.0 - uPhase * 0.5);

    float2 uvR = coords + dir * chromaticStr * 0.5;
    float2 uvG = coords;
    float2 uvB = coords - dir * chromaticStr * 0.5;

    float4 baseR = tex2D(uImage0, uvR);
    float4 baseG = tex2D(uImage0, uvG);
    float4 baseB = tex2D(uImage0, uvB);

    float3 chromaticBase = float3(baseR.r, baseG.g, baseB.b);

    // --- Spectral color mapping along ring circumference ---
    float angle = atan2(delta.y, delta.x);
    float normalizedAngle = (angle / 3.14159 + 1.0) * 0.5; // 0 to 1 around circle
    float spectralHue = 0.7 + normalizedAngle * uPhase * 0.4; // Spectral spread increases with phase
    float3 spectralColor = HueToRGB(frac(spectralHue));

    // --- Color composition ---
    // Rings colored with spectral gradient
    float3 ringColor = lerp(uColor, spectralColor, uPhase * 0.5);
    ringColor = lerp(ringColor, uSecondaryColor, dist * 1.5);

    // Apply chromatic base
    ringColor *= lerp(float3(1, 1, 1), chromaticBase * 1.5, 0.3);

    // --- Noise modulation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += float2(cos(uTime * 0.3), sin(uTime * 0.4)) * 0.05;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.3);

    // --- Edge and distance fade ---
    float edgeFade = 1.0 - smoothstep(0.35, 0.5, dist);
    float centerClear = smoothstep(0.0, 0.08, dist); // No ring at dead center

    // --- Final composition ---
    float3 finalColor = ringColor * ringAccum * noiseVal;

    // Add bright center flash (fades with phase)
    float centerFlash = exp(-dist * dist * 60.0) * (1.0 - uPhase) * 0.5;
    finalColor += float3(1.0, 0.98, 1.0) * centerFlash;

    float alpha = ringAccum * edgeFade * centerClear * uOpacity * sampleColor.a * uIntensity;

    return ApplyOverbright(finalColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUE 2: REFRACTION RIPPLE SUBTLE
// =============================================================================

float4 RefractionRippleSubtlePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Lighter version for ambient hold effects
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);

    // Single gentle expanding ring
    float ringDist = frac(dist * 3.0 - uTime * uScrollSpeed * 0.8);
    float ring = exp(-pow((ringDist - 0.5) * 5.0, 2.0));

    // Gentle spectral tint
    float angle = atan2(delta.y, delta.x);
    float normalizedAngle = (angle / 3.14159 + 1.0) * 0.5;
    float hue = 0.7 + normalizedAngle * 0.15;
    float3 spectralColor = HueToRGB(frac(hue));

    float3 glowColor = lerp(uColor, spectralColor, 0.3);
    glowColor = lerp(glowColor, uSecondaryColor, dist);

    float edgeFade = 1.0 - smoothstep(0.3, 0.5, dist);
    float centerClear = smoothstep(0.0, 0.1, dist);

    float pulse = sin(uTime * 3.0) * 0.08 + 0.92;

    float alpha = ring * edgeFade * centerClear * uOpacity * sampleColor.a * uIntensity * pulse * 0.5;

    return ApplyOverbright(glowColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique RefractionRippleMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 RefractionRipplePS();
    }
}

technique RefractionRippleSubtle
{
    pass P0
    {
        PixelShader = compile ps_3_0 RefractionRippleSubtlePS();
    }
}
