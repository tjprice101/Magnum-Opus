// =============================================================================
// MagnumOpus Tidal Trail Shader - PS 2.0 Compatible
// =============================================================================
// Flowing tidal wave trail with moonlight-on-water caustic patterns.
// Designed for CalamityStyleTrailRenderer primitive geometry.
//
// Unique to Eternal Moon -- "Moonlight on Water":
//   - Sinusoidal tidal wave distortion simulating water surface ripple
//   - Standing wave crests (bright tidal nodes at regular intervals)
//   - Water caustic highlights via secondary noise layer
//   - Deep purple to ice blue tidal color rhythm
//   - Soft, wide edge profile (fluid feel vs surgical precision)
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   TidalTrailMain -- Core trail with tidal wave pattern + caustic highlights
//   TidalTrailGlow -- Wider glow pass with gentle wave motion
// =============================================================================

sampler uImage0 : register(s0); // Base trail texture
sampler uImage1 : register(s1); // Noise texture

float3 uColor;           // Primary color (e.g. deep purple)
float3 uSecondaryColor;  // Secondary color (e.g. ice blue)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness / combo phase level (0.25 - 1.0)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Wave scroll rate
float uNoiseScale;        // Noise UV repetition
float uDistortionAmt;     // Tidal wave amplitude
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;

// =============================================================================
// UTILITY
// =============================================================================

// Softer, wider bump than Incisor -- fluid water profile
float WaterBump(float x)
{
    // Centered parabola: peaks at 0.5, zero at 0 and 1
    float t = x * 2.0 - 1.0; // remap [0,1] -> [-1,1]
    return saturate(1.0 - t * t);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Tidal standing wave: creates bright crests at regular intervals along the trail
float TidalCrest(float u, float frequency, float time)
{
    // Crests travel slowly along the trail like ocean swells
    float phase = u * 3.14159 * frequency - time * 0.7;
    float wave = abs(sin(phase));
    // Sharpen the crests so they read as distinct wave peaks
    return wave * wave;
}

// Water caustic pattern: bright network of light refracting through water
float CausticPattern(float2 uv, float time)
{
    // Two overlapping sine grids at different angles create caustic-like interference
    float c1 = sin(uv.x * 18.0 + time * 1.3) * sin(uv.y * 14.0 - time * 0.9);
    float c2 = sin(uv.x * 12.0 - time * 1.1 + 1.5) * sin(uv.y * 20.0 + time * 0.7);
    // Combine and bias toward bright highlights
    float caustic = (c1 + c2) * 0.5 + 0.5;
    return caustic * caustic; // Square for sharper bright lines
}

// =============================================================================
// TECHNIQUE 1: TIDAL TRAIL MAIN
// =============================================================================

float4 TidalTrailMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Tidal wave distortion (water surface ripple) ---
    float waveFreq = 3.0 + uIntensity * 3.0; // 3-6 waves depending on combo phase
    float tidalCrestVal = TidalCrest(coords.x, waveFreq, uTime * uScrollSpeed * 2.0);

    // Primary tidal ripple: slow, broad sinusoidal displacement across width
    float tidalRipple = sin(coords.x * waveFreq * 6.28 - uTime * uScrollSpeed * 2.5)
                       * uDistortionAmt * 0.8;

    // Secondary cross-ripple: perpendicular interference pattern
    float crossRipple = sin(coords.x * 10.0 + uTime * uScrollSpeed * 1.5)
                       * uDistortionAmt * 0.25;

    float2 distortedUV = coords;
    distortedUV.y += tidalRipple + crossRipple;
    // Gentle horizontal drift like water current
    distortedUV.x += sin(coords.y * 4.0 + uTime * 1.2) * uDistortionAmt * 0.15;

    // Sample base texture
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Noise modulation (organic fluid turbulence) ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    noiseUV.y += sin(uTime * 0.4 + coords.x * 2.0) * 0.15;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.75, noiseTex.r, uHasSecondaryTex);

    // --- Water caustic highlights (secondary noise layer) ---
    float2 causticUV = coords * uSecondaryTexScale * 1.5;
    causticUV.x += uTime * uSecondaryTexScroll * 0.6;
    causticUV.y -= uTime * 0.3;
    float4 causticNoiseTex = tex2D(uImage1, causticUV);
    float causticNoise = lerp(0.5, causticNoiseTex.g, uHasSecondaryTex);

    // Combine analytical caustic with noise-driven variation
    float causticBase = CausticPattern(coords * 3.0, uTime * uScrollSpeed);
    float causticFinal = causticBase * causticNoise * uIntensity * 0.6;

    // --- Edge fade (soft, wide water profile) ---
    float edgeFade = WaterBump(coords.y);
    // Gentler power curve than Incisor for fluid feel
    float softEdge = edgeFade * edgeFade;

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.05);
    // Soften the tail with a gentle ramp
    trailFade *= trailFade;

    // --- Tidal color rhythm: purple to blue with wave-driven oscillation ---
    float tidalRhythm = sin(coords.x * 3.14159 * 2.0 - uTime * uScrollSpeed * 0.8) * 0.5 + 0.5;
    float gradientT = coords.x * 0.5 + tidalRhythm * 0.3 + tidalCrestVal * 0.2;
    float3 trailColor = lerp(uColor, uSecondaryColor, saturate(gradientT));

    // --- Tidal crest highlights (standing wave bright spots) ---
    float crestBrightness = tidalCrestVal * uIntensity * 0.5;
    float3 crestColor = lerp(uSecondaryColor, float3(0.9, 0.95, 1.0), 0.5);
    trailColor = lerp(trailColor, crestColor, crestBrightness);

    // --- Caustic overlay (bright refractive highlights) ---
    float3 causticColor = lerp(uSecondaryColor, float3(1.0, 1.0, 1.0), 0.6);
    trailColor = lerp(trailColor, causticColor, causticFinal);

    // --- Moonlit core (soft center glow like light through water) ---
    float coreMask = saturate((softEdge - 0.5) * 3.0);
    trailColor = lerp(trailColor, float3(0.92, 0.95, 1.0), coreMask * 0.4);

    // --- Tidal brightness modulation ---
    float waveGlow = 0.65 + tidalCrestVal * 0.35 * uIntensity;

    // --- Gentle tidal pulse (slower, more rhythmic than Incisor) ---
    float tidalPulse = sin(uTime * 3.0 + coords.x * 5.0) * 0.06 * uIntensity + 0.94;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * waveGlow * tidalPulse;
    finalColor *= 0.7 + noiseVal * 0.3;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: TIDAL TRAIL GLOW
// =============================================================================

float4 TidalTrailGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Broad, gentle wave for glow pass
    float wave = sin(coords.x * 3.0 - uTime * uScrollSpeed * 1.5) * uDistortionAmt * 0.25;
    float drift = sin(coords.y * 3.0 + uTime * 0.8) * uDistortionAmt * 0.1;
    float2 glowUV = coords;
    glowUV.y += wave;
    glowUV.x += drift;

    float4 baseTex = tex2D(uImage0, glowUV);

    // Wider, softer edge fade for bloom envelope
    float edgeFade = WaterBump(coords.y);
    // Very soft power: broad glow spread
    float softEdge = edgeFade;

    float trailFade = saturate(1.0 - coords.x * 0.8);
    trailFade *= trailFade;

    // Color: biased toward primary (deep purple) with subtle tidal shift
    float tidalShift = sin(coords.x * 3.14159 - uTime * uScrollSpeed * 0.5) * 0.15 + 0.15;
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.3 + tidalShift);

    // Subtle crest highlights in glow
    float crestVal = TidalCrest(coords.x, 3.0, uTime * uScrollSpeed * 1.5);
    glowColor = lerp(glowColor, float3(0.85, 0.9, 1.0), crestVal * 0.2 * uIntensity);

    // Noise for organic variation
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.3;
    noiseUV.y += uTime * 0.15;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // Slow rhythmic pulse like ocean breathing
    float pulse = sin(uTime * 2.0 + coords.x * 3.0) * 0.05 + 0.95;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique TidalTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 TidalTrailMainPS();
    }
}

technique TidalTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 TidalTrailGlowPS();
    }
}
