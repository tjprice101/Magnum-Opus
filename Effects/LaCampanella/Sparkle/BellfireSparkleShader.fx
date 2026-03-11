// BellfireSparkleShader.fx — La Campanella Theme Sparkle Shader
//
// Transforms star sprites into fiery bell-ring flash sparkles.
// Visual identity: ringing bells of fire, hot ember crackle, virtuosic intensity.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Concentric bell-ring modulation: expanding ring patterns emanate from
//   the sparkle center like sound waves from a struck bell
// - Fire crackle interference: sharp, irregular flickering that mimics
//   crackling flames (higher frequency noise than Eroica's smooth embers)
// - Temperature-driven color ramp: center is white-hot (BellChime),
//   mid is infernal orange, outer edge fades to deep ember red
// - Bell harmonic overtones: multiple frequency shimmer layers at
//   harmonic intervals (1x, 2x, 3x base frequency)
// - Spark ejection pattern: bright points fly outward from center

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // InfernalOrange
float3 accentColor;     // FlameYellow/BellGold
float3 highlightColor;  // WhiteHot/BellChime

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 BellfireSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float4 texColor = tex2D(uImage0, uv);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // ---- CONCENTRIC BELL-RING MODULATION ----
    // Rings expand outward from center, decaying with distance
    // Like the visual representation of a bell's sound wave
    float ring1 = sin(dist * 25.0 - uTime * flashSpeed * 4.0 + flashPhase) * 0.5 + 0.5;
    float ring2 = sin(dist * 40.0 - uTime * flashSpeed * 6.0 + flashPhase * 1.5) * 0.5 + 0.5;
    float bellRing = ring1 * 0.6 + ring2 * 0.4;
    bellRing *= smoothstep(0.5, 0.1, dist); // Decay with distance

    // ---- 4-POINT STAR PATTERN ----
    float star4 = sin(angle * 4.0 + uTime * flashSpeed * 1.8 + flashPhase) * 0.5 + 0.5;

    // ---- FIRE CRACKLE INTERFERENCE ----
    // High-frequency, irregular flickering — sharper and faster than Eroica
    float crackle1 = sin(angle * 7.0 + uTime * flashSpeed * 8.3 + flashPhase * 3.1) * 0.5 + 0.5;
    float crackle2 = sin(angle * 11.0 - uTime * flashSpeed * 6.7 + flashPhase * 1.9) * 0.5 + 0.5;
    float fireCrackle = pow(saturate(crackle1 * crackle2), 3.0);

    // ---- BELL HARMONIC OVERTONES ----
    // Shimmer layers at harmonic frequency intervals (1×, 2×, 3× fundamental)
    float harmonic1 = sin(uTime * flashSpeed * 2.0 + flashPhase) * 0.5 + 0.5;           // Fundamental
    float harmonic2 = sin(uTime * flashSpeed * 4.0 + flashPhase * 2.0 + 0.3) * 0.5 + 0.5; // 2nd harmonic
    float harmonic3 = sin(uTime * flashSpeed * 6.0 + flashPhase * 3.0 + 0.7) * 0.5 + 0.5; // 3rd harmonic
    float harmonicChord = harmonic1 * 0.5 + harmonic2 * 0.3 + harmonic3 * 0.2;

    // ---- COMBINED SHIMMER ----
    float shimmer = (star4 * 0.3 + bellRing * 0.35 + fireCrackle * 0.2 + harmonicChord * 0.15);
    shimmer = lerp(1.0, shimmer, shimmerIntensity);

    // ---- INTENSE FLASH PEAKS ----
    // La Campanella sparkles are sharp and virtuosic — high power for crisp bell-ring flashes
    float bellFlash = pow(saturate(star4 * bellRing * 2.0), flashPower);
    float globalFlash = sin(uTime * flashSpeed * 3.0 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.6);

    // ---- WARM FIRE PRISMATIC ----
    // Restricted orange-yellow-red hue range (infernal fire)
    float hue = frac(0.07 + (angle + 3.14159) / 6.28318 * 0.08 + uTime * flashSpeed * 0.03);
    float3 firePrism = hsv2rgb(float3(hue, 0.6, 1.0));
    float prismStrength = smoothstep(0.1, 0.28, dist) * smoothstep(0.5, 0.35, dist) * 0.3;

    // ---- TEMPERATURE-DRIVEN RADIAL GRADIENT ----
    // White-hot center → Infernal orange mid → Deep ember edge
    float tempGrad = smoothstep(0.5, 0.0, dist);
    float3 tempColor = lerp(
        primaryColor,                          // Outer: infernal orange
        lerp(accentColor, highlightColor, tempGrad * 0.8),  // Inner: yellow → white-hot
        tempGrad
    );

    // ---- RADIAL BLOOM ----
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.2, 0.0, dist);

    // ---- BELL PULSE ----
    float bellPulse = 0.75 + 0.25 * harmonicChord;
    float centerGlow = smoothstep(0.25, 0.0, dist) * bellPulse;

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Temperature-ramped base with bell-ring modulation
    float3 baseLayer = tempColor * luminance * shimmer;

    // Fire prismatic edge
    float3 prismLayer = firePrism * prismStrength * luminance * 0.4;

    // Sharp bell-ring flash bursts
    float3 dazzleLayer = highlightColor * bellFlash * 1.6;

    // Global harmonic flash
    float3 flashLayer = lerp(accentColor, highlightColor, 0.5) * globalFlash * innerBloom * 0.5;

    // White-hot center core
    float3 glowLayer = highlightColor * centerGlow * 0.5;

    // Fire crackle accent
    float3 crackleLayer = primaryColor * fireCrackle * luminance * 0.2;

    float3 finalColor = baseLayer + prismLayer + dazzleLayer + flashLayer + glowLayer + crackleLayer;
    finalColor *= 1.45;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.3 + 0.7);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass BellfireSparklePass
    {
        PixelShader = compile ps_3_0 BellfireSparklePS();
    }
}
