// =============================================================================
// Heat Distortion Shader - PS 2.0 Compatible
// =============================================================================
// Barrel heat mirage effect for Blossom of the Sakura assault rifle.
// Creates rising heat shimmer that intensifies with heat level,
// distorting the sprite with sinusoidal displacement.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1)
//   V (coords.y) = vertical position (0-1)
//
// Techniques:
//   HeatShimmerMain  - Rising heat distortion with color tint
//
// Features:
//   - Sinusoidal UV displacement that increases with heat
//   - Rising shimmer waves (upward scroll)
//   - Heat-reactive warm color tinting at high levels
//   - Procedural noise for organic shimmer variation
//   - Fully transparent at heat=0, visible shimmer at heat=1
// =============================================================================

sampler uImage0 : register(s0); // Sprite texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;            // Primary color (Flame orange)
float3 uSecondaryColor;   // Secondary color (HotCore white)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uHeatLevel;          // 0 = cool, 1 = overheated
float uDistortionAmt;      // Maximum distortion strength
float uScrollSpeed;         // Heat shimmer scroll rate

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: HEAT SHIMMER MAIN
// =============================================================================

float4 HeatShimmerMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Heat-scaled distortion ---
    float heatFactor = uHeatLevel * uHeatLevel; // Quadratic ramp for feel

    // Rising wave displacement (heat rises upward)
    float wave1 = sin(coords.y * 12.0 + uTime * uScrollSpeed * 3.0 + coords.x * 4.0);
    float wave2 = sin(coords.y * 20.0 - uTime * uScrollSpeed * 5.0 + coords.x * 8.0);
    float wave3 = cos(coords.y * 8.0 + uTime * uScrollSpeed * 2.0);

    float distortX = (wave1 * 0.5 + wave2 * 0.3 + wave3 * 0.2) * uDistortionAmt * heatFactor;
    float distortY = sin(coords.x * 15.0 + uTime * uScrollSpeed * 4.0) * uDistortionAmt * heatFactor * 0.3;

    float2 distortedUV = coords;
    distortedUV.x += distortX;
    distortedUV.y += distortY;

    // Sample distorted sprite
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Heat-reactive warm tint ---
    float3 heatTint = lerp(float3(1.0, 1.0, 1.0), uColor, heatFactor * 0.3);
    heatTint = lerp(heatTint, uSecondaryColor, heatFactor * heatFactor * 0.2);

    // --- Rising shimmer overlay ---
    float shimmerMask = saturate(1.0 - coords.y * 1.5); // Top-biased
    float shimmerPattern = sin(coords.x * 25.0 + uTime * uScrollSpeed * 6.0) * 0.5 + 0.5;
    shimmerPattern *= sin(coords.x * 15.0 - uTime * 3.0) * 0.3 + 0.7;
    float shimmer = shimmerMask * shimmerPattern * heatFactor * 0.15;

    // --- Procedural noise variation ---
    float noise = HashNoise(coords * 8.0 + float2(uTime * 0.3, uTime * 0.2));

    // --- Final composition ---
    float3 finalColor = baseTex.rgb * heatTint * uIntensity;
    finalColor += uSecondaryColor * shimmer * noise;

    float alpha = baseTex.a * sampleColor.a * uOpacity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique HeatShimmerMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 HeatShimmerMainPS();
    }
}
