// RadialNoiseMaskShader.fx
// Self-contained radial noise mask shader for MaskFoundation.
//
// Applied as a SpriteBatch effect to draw a vibrant noise-mapped circle.
// The noise texture is sampled in polar (radial) UV space so it scrolls
// outward/inward and rotates around the center, creating a living orb.
//
// Pipeline:
//   1. Convert sprite UVs to polar coordinates (angle + radius from center)
//   2. Scroll the angle and radius over time for radial animation
//   3. Sample the noise texture with the scrolled polar UVs
//   4. Apply a gradient LUT for theme-consistent vibrant coloring
//   5. Mask to a soft circle using smoothstep distance falloff
//   6. Boost brightness for a vivid, clear result
//
// Shader Model 2.0 compatible (fx_2_0).
//
// Parameters passed from C#:
//   uTime           - accumulated time for animation
//   scrollSpeed     - how fast the noise scrolls radially
//   rotationSpeed   - how fast the noise rotates around center
//   circleRadius    - normalized radius of the visible circle (0-0.5)
//   edgeSoftness    - how soft the circle edge falloff is
//   intensity       - overall brightness multiplier
//   primaryColor    - primary tint color (vec3)
//   coreColor       - bright core color (vec3)

sampler uImage0 : register(s0); // Sprite quad texture bound by SpriteBatch

float uTime;
float scrollSpeed;
float rotationSpeed;
float circleRadius;
float edgeSoftness;
float intensity;
float3 primaryColor;
float3 coreColor;

// Noise texture — the selected noise pattern to mask onto the circle
texture noiseTex;
sampler2D samplerNoise = sampler_state
{
    texture = <noiseTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Gradient LUT for theme coloring
texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

// PI constant
static const float PI = 3.14159265;

float4 RadialNoiseMaskPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;

    // ---- CENTER-RELATIVE COORDINATES ----
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Normalize angle from [-PI, PI] to [0, 1]
    float angleFrac = (angle + PI) / (PI * 2.0);

    // Normalized distance from center (0 = center, 1 = very edge of UV quad)
    float normDist = dist * 2.0; // 0 to ~1.41 for corners, 1.0 at edge of inscribed circle

    // ---- RADIAL SCROLLING UV ----
    // Scroll angle and radius over time for radial motion
    float scrolledAngle = angleFrac + uTime * rotationSpeed;
    float scrolledRadius = normDist * 1.5 + uTime * scrollSpeed;

    // Build scrolled UV in polar space
    float2 noiseUV = float2(scrolledAngle, scrolledRadius);

    // ---- SAMPLE NOISE ----
    float4 noiseSample = tex2D(samplerNoise, noiseUV);
    float noiseVal = noiseSample.r; // Use red channel as intensity

    // Sample a second noise layer at different scale/speed for detail
    float2 noiseUV2 = float2(scrolledAngle * 2.0 + 0.37, scrolledRadius * 0.8 - uTime * scrollSpeed * 0.3);
    float noiseVal2 = tex2D(samplerNoise, noiseUV2).r;

    // Combine noise layers
    float combinedNoise = noiseVal * 0.7 + noiseVal2 * 0.3;

    // ---- GRADIENT LUT COLORING ----
    // Map noise intensity to theme color via gradient LUT
    float3 gradColor = tex2D(samplerGradient, float2(combinedNoise, 0.5)).rgb;

    // ---- VIBRANT COLOR MIX ----
    // Blend between outer theme color and hot core color based on noise brightness
    float3 baseColor = lerp(primaryColor * gradColor, coreColor, combinedNoise * combinedNoise);

    // Boost brightness — make it vibrant and clear
    baseColor *= intensity;

    // Add extra brightness at noise peaks for sparkle
    float sparkle = pow(saturate(combinedNoise), 3.0) * 0.5;
    baseColor += coreColor * sparkle;

    // ---- CIRCULAR MASK ----
    // Soft circle mask using smoothstep
    float innerEdge = circleRadius - edgeSoftness;
    float outerEdge = circleRadius;
    float circleMask = 1.0 - smoothstep(innerEdge, outerEdge, normDist);

    // Extra brightness falloff toward edge — bright core, slightly dimmer edge
    float coreBrightness = 1.0 - normDist * 0.3;
    coreBrightness = saturate(coreBrightness);

    // ---- FINAL COMPOSITE ----
    float3 finalColor = baseColor * circleMask * coreBrightness;
    float finalAlpha = circleMask * saturate(combinedNoise * 2.0 + 0.3);

    return float4(finalColor, finalAlpha);
}

technique Technique1
{
    pass RadialNoiseMaskPass
    {
        PixelShader = compile ps_2_0 RadialNoiseMaskPS();
    }
}
