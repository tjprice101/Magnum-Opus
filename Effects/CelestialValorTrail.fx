// =============================================================================
// Celestial Valor Trail Shader - PS 2.0 Compatible
// =============================================================================
// Unique heroic trail for Celestial Valor sword projectiles.
// Scarlet → Gold gradient with pulsing core energy and sakura-soft edges.
//
// Techniques:
//   HeroicTrail  – Main projectile / swing trail
//   ValorFlare   – Bright impact / bloom overlay pass
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;           // Primary: scarlet (0.78, 0.20, 0.20)
float3 uSecondaryColor;  // Secondary: gold  (1.0, 0.84, 0.0)
float uOpacity;
float uTime;
float uIntensity;        // Overall brightness multiplier
float uProgress;         // 0-1 along trail length

// Overbright / glow uniforms (shared with other MagnumOpus shaders)
float uOverbrightMult;   // HDR multiplier (2-5 recommended)
float uGlowThreshold;    // Glow cutoff (default 0.5)
float uGlowIntensity;    // Extra glow boost (default 1.5)
float uHasSecondaryTex;  // 1.0 if uImage1 is bound
float uSecondaryTexScale; // UV scale for noise
float uSecondaryTexScroll; // Scroll speed for noise

// =============================================================================
// UTILITY
// =============================================================================

// 0→1→0 bump (peak at centre)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: HEROIC TRAIL
// =============================================================================
// Scarlet-to-gold gradient along UV.x (progress).
// Bright white-hot core along UV.y centre.
// Soft sakura-pink tint at edges before fade.
// Optional noise turbulence via uImage1.
// =============================================================================

float4 HeroicTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Edge-to-centre intensity (bright centre, fades at edges) ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Progress fade (trail fades toward tail) ---
    float progressFade = saturate(1.0 - coords.x * 1.1);

    // --- Heroic gradient: scarlet at head → gold at tail ---
    float3 heroGrad = lerp(uColor, uSecondaryColor, coords.x);

    // --- White-hot core in the very centre ---
    float coreMask = saturate((edgeFade - 0.65) * 4.0);          // narrow bright strip
    float3 coreColor = lerp(heroGrad, float3(1, 1, 1), coreMask);

    // --- Sakura-pink tint at soft outer edge ---
    float outerMask = saturate((0.45 - edgeFade) * 3.0);         // only at edge fringe
    float3 sakuraTint = float3(1.0, 0.59, 0.71);                 // (255, 150, 180) normalised
    coreColor = lerp(coreColor, sakuraTint, outerMask * 0.35);

    // --- Noise turbulence (branch-free, zero-cost when unbound) ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noise = tex2D(uImage1, noiseUV);
    coreColor *= lerp(1.0, noise.r * 1.4 + 0.3, uHasSecondaryTex * 0.45);

    // --- Pulse: subtle sine throb synced to time ---
    float pulse = 0.92 + sin(uTime * 8.0 + coords.x * 12.0) * 0.08;

    // --- Combine ---
    float alpha = edgeFade * progressFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    float3 finalColor = coreColor * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: VALOR FLARE
// =============================================================================
// Additive bloom / impact overlay. White-gold radial glow, very bright centre.
// Used for lens-flare passes and hit-flash overlays.
// =============================================================================

float4 ValorFlarePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Radial distance from centre of quad (0 = centre, 1 = edge)
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0; // 0→1 from centre to edge

    // Soft radial falloff
    float radial = saturate(1.0 - dist * dist);

    // Gold core → scarlet fringe gradient
    float3 flareColor = lerp(uSecondaryColor, uColor, dist);
    flareColor = lerp(flareColor, float3(1, 1, 1), radial * radial); // white-hot centre

    // Pulsing intensity
    float pulse = 0.85 + sin(uTime * 6.0) * 0.15;

    float alpha = radial * uOpacity * sampleColor.a * baseTex.a * pulse;
    float3 finalColor = flareColor * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique HeroicTrail
{
    pass P0
    {
        PixelShader = compile ps_2_0 HeroicTrailPS();
    }
}

technique ValorFlare
{
    pass P0
    {
        PixelShader = compile ps_2_0 ValorFlarePS();
    }
}
