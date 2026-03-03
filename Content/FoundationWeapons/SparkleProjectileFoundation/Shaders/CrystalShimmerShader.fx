// CrystalShimmerShader.fx
// Self-contained crystal shimmer shader for SparkleProjectileFoundation.
//
// Applied as a SpriteBatch effect to the crystal body sprites.
// Creates a dazzling prismatic shimmer effect:
// 1. Rotates the sprite's UV for a faceted spinning crystal look
// 2. Samples a gradient LUT for theme-consistent prismatic coloring
// 3. Adds procedural sparkle flashes at facet intersection points
// 4. Pulses brightness with time for living, breathing crystal feel
// 5. Blends between theme color and bright white highlights
//
// Parameters passed from C#:
//   uTime          — drives all animation
//   rotation       — crystal body rotation (radians)
//   shimmerSpeed   — how fast the prismatic shift cycles
//   flashIntensity — brightness of sparkle flash peaks
//   baseAlpha      — overall opacity (for fade-in/out)
//   primaryColor   — theme primary color (vec3)
//   highlightColor — bright highlight color (vec3)

sampler uImage0 : register(s0); // The crystal sprite texture bound by SpriteBatch

float uTime;
float rotation;
float shimmerSpeed;
float flashIntensity;
float baseAlpha;
float3 primaryColor;
float3 highlightColor;

// Gradient LUT for theme coloring
texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = clamp;
};

// ---- HSV UTILITIES ----
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 CrystalShimmerPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;
    
    // ---- SAMPLE BASE SPRITE ----
    float4 texColor = tex2D(uImage0, baseUV);
    
    // Early out for fully transparent pixels
    if (texColor.a < 0.01)
        return float4(0, 0, 0, 0);
    
    // ---- CENTER-RELATIVE COORDINATES ----
    float2 centered = baseUV - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // ---- FACETED SHIMMER ----
    // Create angular facets — like light hitting crystal faces at different angles
    // 6 facets for hexagonal crystal feel
    float facetAngle = angle + rotation;
    float facet = sin(facetAngle * 6.0 + uTime * shimmerSpeed) * 0.5 + 0.5;
    
    // Secondary facet at different frequency for interference pattern
    float facet2 = sin(facetAngle * 4.0 - uTime * shimmerSpeed * 0.7 + 1.5) * 0.5 + 0.5;
    
    // Combined facet shimmer — creates shifting bright patches across the crystal surface
    float shimmer = facet * 0.6 + facet2 * 0.4;
    
    // ---- PRISMATIC COLOR ----
    // Hue shifts around the crystal based on angle + time → rainbow prismatic refraction
    float hue = frac((angle + 3.14159) / 6.28318 + uTime * shimmerSpeed * 0.15);
    float3 prismColor = hsv2rgb(float3(hue, 0.5, 1.0));
    
    // Sample gradient LUT for theme coloring
    float gradCoord = frac(shimmer * 0.7 + uTime * shimmerSpeed * 0.1);
    float3 gradColor = tex2D(samplerGradient, float2(gradCoord, 0.5)).rgb;
    
    // ---- SPARKLE FLASH POINTS ----
    // Brief bright flashes at "facet intersections" — the dazzle points
    float flash1 = sin(facetAngle * 6.0 + uTime * 7.3);
    float flash2 = sin(facetAngle * 4.0 + uTime * 5.1 + 2.0);
    float flashPeak = pow(saturate(flash1 * flash2), 16.0); // Very sharp peaks
    
    // Distance-based flash — only near edges of the crystal for facet-edge highlight
    float edgeFlash = smoothstep(0.15, 0.35, dist) * smoothstep(0.5, 0.4, dist);
    float sparkleFlash = flashPeak * edgeFlash * flashIntensity;
    
    // ---- CENTRAL GLOW PULSE ----
    // Pulsing brightness at center — the crystal's inner light
    float pulse = 0.85 + 0.15 * sin(uTime * 3.5);
    float centerGlow = smoothstep(0.35, 0.0, dist) * pulse;
    
    // ---- COLOR COMPOSITE ----
    // Base: sprite luminance drives theme coloring
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
    
    // Layer 1: Theme color from gradient, modulated by sprite brightness
    float3 themeLayer = gradColor * luminance * 1.2;
    
    // Layer 2: Prismatic shimmer overlay — subtle iridescent shifting
    float3 prismLayer = lerp(primaryColor, prismColor, shimmer * 0.4) * luminance * 0.5;
    
    // Layer 3: Sparkle flash — white/highlight bursts
    float3 flashLayer = highlightColor * sparkleFlash;
    
    // Layer 4: Center glow — warm inner light
    float3 glowLayer = lerp(primaryColor, highlightColor, 0.5) * centerGlow * 0.6;
    
    // Combine all layers
    float3 finalColor = themeLayer + prismLayer + flashLayer + glowLayer;
    
    // Boost overall brightness for the additive blend
    finalColor *= 1.3;
    
    float finalAlpha = texColor.a * baseAlpha;
    
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass CrystalPass
    {
        PixelShader = compile ps_3_0 CrystalShimmerPS();
    }
}
