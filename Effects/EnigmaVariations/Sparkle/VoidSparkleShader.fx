// VoidSparkleShader.fx — Enigma Variations Theme Sparkle Shader
//
// Transforms star sprites into eerie, void-distorted glitch sparkles.
// Visual identity: unknowable mystery, arcane dread, void-shattered light.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Voronoi-like shatter pattern: sparkle brightness fragments into
//   cell-like regions that shift independently, as if reality is cracking
// - Temporal glitch: periodic sudden brightness jumps (stuttering flicker)
//   that break the smooth sin-wave pattern used by other themes
// - Eerie green-purple color inversion: at random moments, the sparkle's
//   color inverts between green and purple (the two Enigma colors)
// - Void distortion: UV coordinates themselves are slightly warped outward
//   from center, making the sparkle appear to bend space
// - Watching eye pulse: a subtle radial pulse pattern that evokes a blinking eye

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // Purple/GreenFlame
float3 accentColor;     // BrightGreen/VoidFlame
float3 highlightColor;  // WhiteGreenFlash

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// Pseudo-random hash for Voronoi-like patterns
float hash12(float2 p)
{
    float3 p3 = frac(float3(p.x, p.y, p.x) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float4 VoidSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    
    // ---- VOID DISTORTION (UV warp) ----
    // Slightly push UVs outward from center — space bends around the sparkle
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float voidWarp = 0.015 * sin(uTime * flashSpeed * 2.0 + dist * 12.0 + flashPhase);
    float2 warpedUV = uv + normalize(centered + 0.001) * voidWarp;
    
    float4 texColor = tex2D(uImage0, warpedUV);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    // ---- VORONOI-LIKE SHATTER PATTERN ----
    // Fragment the sparkle into cell-like regions with independent brightness
    float2 cellUV = centered * 6.0;
    float2 cellID = floor(cellUV);
    float2 cellFrac = frac(cellUV) - 0.5;
    
    float cellBright = hash12(cellID + floor(uTime * flashSpeed * 0.5));
    float cellFlicker = sin(uTime * flashSpeed * 3.0 + cellBright * 6.28) * 0.5 + 0.5;
    float shatterPattern = cellFlicker * 0.5 + 0.5;

    // ---- 4-POINT STAR WITH VOID TWIST ----
    float star4 = sin(angle * 4.0 + uTime * flashSpeed * 1.2 + flashPhase + voidWarp * 20.0) * 0.5 + 0.5;
    
    // ---- 7-POINT ARCANE PATTERN (prime number for unsettling asymmetry) ----
    float arcane7 = sin(angle * 7.0 - uTime * flashSpeed * 0.9 + flashPhase * 1.7) * 0.5 + 0.5;

    // ---- TEMPORAL GLITCH ----
    // Periodic sudden brightness jumps that break smooth animation
    float glitchTimer = frac(uTime * flashSpeed * 0.3 + flashPhase * 0.7);
    float glitch = step(0.92, glitchTimer); // Brief bright flash 8% of the time
    float antiGlitch = step(0.85, glitchTimer) * (1.0 - glitch); // Dim just before flash

    // ---- WATCHING EYE PULSE ----
    // Radial pulse that expands and contracts — like an eye blinking
    float eyeOpen = sin(uTime * flashSpeed * 1.5 + flashPhase * 2.0) * 0.5 + 0.5;
    float eyeRing = smoothstep(0.2, 0.15, abs(dist - eyeOpen * 0.3)) * 0.4;

    // ---- COMBINED SHIMMER ----
    float shimmer = (star4 * 0.3 + arcane7 * 0.2 + shatterPattern * 0.3 + eyeRing * 0.2);
    shimmer = lerp(1.0, shimmer, shimmerIntensity);
    shimmer = shimmer * (1.0 - antiGlitch * 0.5) + glitch * 0.8; // Apply glitch

    // ---- UNSETTLING FLASH PEAKS ----
    float voidFlash = pow(saturate(star4 * arcane7 * 2.0), flashPower);
    float globalFlash = sin(uTime * flashSpeed * 2.8 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.5);
    globalFlash += glitch * 0.6; // Glitch amplifies flash

    // ---- EERIE GREEN-PURPLE COLOR INVERSION ----
    // Periodically swap between green and purple dominance
    float colorSwap = sin(uTime * flashSpeed * 0.7 + flashPhase * 3.0) * 0.5 + 0.5;
    float3 enigmaColor = lerp(primaryColor, accentColor, colorSwap);

    // ---- VOID PRISMATIC (eerie green-purple only) ----
    float hue = frac(0.33 + (angle + 3.14159) / 6.28318 * 0.2 + uTime * flashSpeed * 0.06);
    float3 voidPrism = hsv2rgb(float3(hue, 0.45, 0.9));
    float prismStrength = smoothstep(0.1, 0.3, dist) * smoothstep(0.5, 0.35, dist) * 0.35;

    // ---- RADIAL BLOOM ----
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.22, 0.0, dist);

    // ---- VOID CENTER ----
    float voidPulse = 0.7 + 0.3 * eyeOpen;
    float centerGlow = smoothstep(0.25, 0.0, dist) * voidPulse;

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Enigma-tinted base with color inversion and shatter
    float3 baseLayer = enigmaColor * luminance * shimmer;

    // Eerie prismatic edge
    float3 prismLayer = voidPrism * prismStrength * luminance;

    // Void flash dazzle — greenish-white
    float3 dazzleLayer = lerp(highlightColor, accentColor, 0.2) * voidFlash * 1.3;

    // Global flash with glitch accent
    float3 flashLayer = highlightColor * globalFlash * innerBloom * 0.45;

    // Void center glow — eerie green
    float3 glowLayer = lerp(accentColor, highlightColor, 0.4) * centerGlow * 0.4;

    // Eye ring accent
    float3 eyeLayer = accentColor * eyeRing * luminance * 0.3;

    float3 finalColor = baseLayer + prismLayer + dazzleLayer + flashLayer + glowLayer + eyeLayer;
    finalColor *= 1.35;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.3 + 0.7);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass VoidSparklePass
    {
        PixelShader = compile ps_3_0 VoidSparklePS();
    }
}
