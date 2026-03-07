// StarburstFlashShader.fx
// SpriteBatch-applied shader for dazzling sparkle explosion particles.
//
// Applied as a SpriteBatch effect to individual star/sparkle sprites in the explosion field.
// Creates an enhanced dazzling effect on each sparkle by:
// 1. Adding procedural multi-facet shimmer (angular sin patterns) for crystalline sparkle
// 2. Prismatic hue shifting around the sprite edges for rainbow refraction
// 3. Sharp flash peaks at facet intersections — the "dazzle points"
// 4. Radial bloom falloff for a soft glowing halo around each star point
// 5. Pulsing inner glow that gives each sparkle a breathing, living quality
// 6. Two-tone color blending: theme-tinted body + white-hot core flashes
//
// This shader transforms flat star sprites into shimmering, faceted, living jewels of light.

sampler uImage0 : register(s0); // The star sprite texture bound by SpriteBatch

float uTime;
float flashPhase;       // Per-sparkle phase offset for unique timing
float flashSpeed;       // How fast this sparkle flashes
float flashPower;       // Peak sharpness exponent (3-8)
float baseAlpha;        // Overall opacity (for fade-in/out lifetime control)
float shimmerIntensity; // How strong the angular facet shimmer is
float3 primaryColor;    // Theme primary tint
float3 accentColor;     // Theme accent tint
float3 highlightColor;  // Bright flash color (usually white)

// ---- HSV UTILITY ----
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 StarburstFlashPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    
    // ---- SAMPLE BASE SPRITE ----
    float4 texColor = tex2D(uImage0, uv);
    
    if (texColor.a < 0.01)
        return float4(0, 0, 0, 0);
    
    // ---- CENTER-RELATIVE COORDINATES ----
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // ---- MULTI-FACET ANGULAR SHIMMER ----
    // 4-point angular pattern (matching the 4-point star shape)
    float facet4 = sin(angle * 4.0 + uTime * flashSpeed * 2.0 + flashPhase) * 0.5 + 0.5;
    
    // 8-point secondary pattern for complex light play
    float facet8 = sin(angle * 8.0 - uTime * flashSpeed * 1.3 + flashPhase * 1.7) * 0.5 + 0.5;
    
    // 3-point asymmetric pattern for organic irregularity
    float facet3 = sin(angle * 3.0 + uTime * flashSpeed * 0.8 + 2.1) * 0.5 + 0.5;
    
    // Combined shimmer — creates shifting bright lobes across the star
    float shimmer = facet4 * 0.5 + facet8 * 0.3 + facet3 * 0.2;
    shimmer = lerp(1.0, shimmer, shimmerIntensity);
    
    // ---- FLASH PEAKS AT FACET INTERSECTIONS ----
    // Where facet patterns overlap, create sharp bright dazzle points
    float flashCross = facet4 * facet8;
    float dazzlePoint = pow(saturate(flashCross), flashPower);
    
    // Time-based global flash
    float globalFlash = sin(uTime * flashSpeed * 3.0 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.5);
    
    // ---- PRISMATIC HUE SHIFTING ----
    // Subtle rainbow refraction around edges of the star
    float hue = frac((angle + 3.14159) / 6.28318 + uTime * flashSpeed * 0.1 + flashPhase * 0.3);
    float3 prismColor = hsv2rgb(float3(hue, 0.35, 1.0));
    float prismStrength = smoothstep(0.1, 0.35, dist) * smoothstep(0.5, 0.38, dist);
    
    // ---- RADIAL BLOOM FALLOFF ----
    // Soft glow that extends beyond the star sprite edges
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.25, 0.0, dist);
    
    // ---- PULSING INNER GLOW ----
    float pulse = 0.8 + 0.2 * sin(uTime * flashSpeed * 1.5 + flashPhase * 2.0);
    float centerGlow = smoothstep(0.3, 0.0, dist) * pulse;
    
    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));
    
    // Layer 1: Theme-tinted base — sprite shape with angular shimmer
    float3 baseLayer = lerp(primaryColor, accentColor, shimmer * 0.5) * luminance * shimmer;
    
    // Layer 2: Prismatic edge refraction
    float3 prismLayer = prismColor * prismStrength * luminance * 0.4;
    
    // Layer 3: Dazzle flash points — white-hot bursts at facet crossings
    float3 dazzleLayer = highlightColor * dazzlePoint * 1.5;
    
    // Layer 4: Global flash bloom — breathing brightness
    float3 flashLayer = highlightColor * globalFlash * innerBloom * 0.6;
    
    // Layer 5: Center glow — warm inner light
    float3 glowLayer = lerp(primaryColor, highlightColor, 0.6) * centerGlow * 0.5;
    
    // Combine
    float3 finalColor = baseLayer + prismLayer + dazzleLayer + flashLayer + glowLayer;
    
    // Boost for additive blending
    finalColor *= 1.4;
    
    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.3 + 0.7);
    
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass StarburstPass
    {
        PixelShader = compile ps_3_0 StarburstFlashPS();
    }
}
