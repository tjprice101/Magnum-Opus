// =============================================================================
// Fate Summon Circle Shader - PS 2.0 Compatible
// =============================================================================
// Dark summoning ritual circle for Finality of the Sakura. Concentric rings
// with crimson runes, rotating inner/outer rings in opposite directions,
// dark flame energy rising from circle edge, fate-black center void.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1), centre = 0.5
//   V (coords.y) = vertical position (0-1), centre = 0.5
//
// Techniques:
//   FateSummonMain  - Dark ritual circle with counter-rotating rings
//   FateSummonGlow  - Crimson bloom overlay for circle aura
//
// Features:
//   - Counter-rotating concentric rings
//   - Procedural rune impressions via angular noise
//   - Phase-driven ritual activation (0 = dormant, 1 = fully active)
//   - Dark void centre that deepens with ritual progress
//   - Crimson ↁEscarlet gradient with dark flame edge
//   - Overbright for HDR bloom around circle edges
// =============================================================================

sampler uImage0 : register(s0); // Base texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;            // Primary color (Crimson)
float3 uSecondaryColor;   // Secondary color (DeepScarlet)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;             // Ritual phase (0 = dormant, 1 = fully active)
float uScrollSpeed;        // Ring rotation speed
float uDistortionAmt;      // Dark flame distortion
float uNoiseScale;         // Noise UV repetition
float uHasSecondaryTex;

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float SmoothHash(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: FATE SUMMON MAIN
// =============================================================================

float4 FateSummonMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- Outer ring: clockwise rotation ---
    float outerAngle = angle + uTime * uScrollSpeed;
    float outerRingDist = abs(dist - 0.75);
    float outerRing = saturate(1.0 - outerRingDist * 15.0);

    // Rune impressions on outer ring (8 angular segments)
    float runePattern = sin(outerAngle * 8.0) * 0.5 + 0.5;
    runePattern = pow(runePattern, 3.0); // Sharpen into rune-like marks
    float outerRunes = outerRing * runePattern * 0.6;

    // --- Middle ring: counter-clockwise ---
    float midAngle = angle - uTime * uScrollSpeed * 1.3;
    float midRingDist = abs(dist - 0.50);
    float midRing = saturate(1.0 - midRingDist * 18.0);

    // Different rune pattern on middle ring (5 segments for pentagram)
    float midRunes = sin(midAngle * 5.0) * 0.5 + 0.5;
    midRunes = pow(midRunes, 2.0);
    midRing *= (0.6 + midRunes * 0.4);

    // Middle ring only visible as ritual progresses
    midRing *= saturate(uPhase * 2.0);

    // --- Inner ring: fast clockwise ---
    float innerAngle = angle + uTime * uScrollSpeed * 2.0;
    float innerRingDist = abs(dist - 0.28);
    float innerRing = saturate(1.0 - innerRingDist * 22.0);
    innerRing *= saturate(uPhase * 3.0 - 0.5); // Appears later in ritual

    // --- Dark void centre ---
    float voidMask = saturate(1.0 - dist * 5.0);
    voidMask *= uPhase; // Deepens with ritual

    // --- Dark flame energy at circle edge ---
    float2 flameP = float2(angle * 0.318 + uTime * 0.3, dist * 2.0 - uTime * uScrollSpeed * 0.5);
    float flameNoise = SmoothHash(flameP * uNoiseScale);
    float edgeMask = saturate((dist - 0.6) * 4.0) * saturate((0.95 - dist) * 4.0);
    float darkFlame = edgeMask * flameNoise * uPhase * 0.4;

    // Optional noise texture
    float2 noiseUV = coords * uNoiseScale;
    noiseUV += float2(uTime * 0.1, -uTime * 0.15);
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(flameNoise, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Colour: Crimson rings, dark void centre ---
    float3 ringColor = lerp(uColor, uSecondaryColor, dist * 0.5);

    // Rune segments brighter (golden-crimson)
    float3 runeColor = lerp(uColor, float3(0.9, 0.6, 0.3), 0.4);

    // Void is near-black with slight crimson tint
    float3 voidColor = float3(0.05, 0.01, 0.02);

    // --- Phase-driven intensity ---
    float phaseIntensity = 0.1 + uPhase * 0.9;

    // --- Ominous pulse (slow, dreadful) ---
    float pulse = sin(uTime * 2.0 + dist * 4.0) * 0.08 + 0.92;

    // --- Final composition ---
    float totalRings = saturate(outerRing + midRing + innerRing);
    float3 finalColor = ringColor * totalRings * uIntensity;
    finalColor += runeColor * outerRunes * uIntensity;
    finalColor += uColor * darkFlame * uIntensity * 0.6;
    finalColor = lerp(finalColor, voidColor, voidMask * 0.7);
    finalColor *= pulse * (0.6 + noiseVal * 0.4);

    float alpha = (totalRings * 0.7 + outerRunes * 0.3 + darkFlame + voidMask * 0.2) *
        phaseIntensity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: FATE SUMMON GLOW
// =============================================================================

float4 FateSummonGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial glow biased toward ring radius
    float ringGlow = saturate(1.0 - abs(dist - 0.6) * 3.0);
    float radial = saturate(1.0 - dist * dist) * 0.4;
    float combined = ringGlow + radial;

    // Dark crimson glow
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3);
    float3 darkTint = float3(0.4, 0.1, 0.08);
    glowColor = lerp(glowColor, darkTint, 0.2);

    glowColor *= uIntensity * baseTex.rgb * 0.7;

    float phaseIntensity = 0.1 + uPhase * 0.9;

    float pulse = sin(uTime * 1.5) * 0.1 + 0.9;

    float alpha = combined * phaseIntensity * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FateSummonMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 FateSummonMainPS();
    }
}

technique FateSummonGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 FateSummonGlowPS();
    }
}
