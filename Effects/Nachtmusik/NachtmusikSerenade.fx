// =============================================================================
// MagnumOpus Nachtmusik Serenade Shader - PS 3.0
// =============================================================================
// Soft nocturnal bloom overlay with musical wave modulation.
// Creates a "serenade under the stars" radial glow that pulses
// with harmonic rhythm, evoking the gentle beauty of night music.
//
// Used as a weapon-aura/bloom overlay for magic and ranged weapons.
//
// UV Layout:
//   Centered at (0.5, 0.5) — radial coordinate system
//
// Techniques:
//   NachtmusikSerenadePass  – Main radial bloom with harmonic waves
//   NachtmusikSerenadeGlow  – Wider softer glow for bloom stacking
// =============================================================================

sampler uImage0 : register(s0); // Base texture (soft glow circle)
sampler uImage1 : register(s1); // Noise texture

float3 uColor;           // Primary color (deep indigo)
float3 uSecondaryColor;  // Secondary color (starlight silver)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness multiplier
float uPhase;            // Intensity phase (0 = quiet, 1 = forte)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Wave animation speed
float uHasSecondaryTex;   
float uSecondaryTexScale;
float uSecondaryTexScroll;

// =============================================================================
// UTILITY
// =============================================================================

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Musical harmonic wave pattern — overlapping sine waves creating
// a visual representation of musical harmonics
float HarmonicWaves(float2 centered, float time, float speed)
{
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // Fundamental frequency (the base note)
    float fundamental = sin(dist * 12.0 - time * speed * 2.0) * 0.5 + 0.5;
    
    // Second harmonic (octave)
    float harmonic2 = sin(dist * 24.0 - time * speed * 4.0 + angle * 2.0) * 0.3 + 0.5;
    
    // Third harmonic (perfect fifth)
    float harmonic3 = sin(dist * 18.0 - time * speed * 3.0 - angle) * 0.2 + 0.5;
    
    // Combine harmonics (like musical overtones)
    float combined = fundamental * 0.5 + harmonic2 * 0.3 + harmonic3 * 0.2;
    
    return combined;
}

// =============================================================================
// TECHNIQUE 1: NACHTMUSIK SERENADE
// =============================================================================

float4 NachtmusikSerenadePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // --- Harmonic wave pattern ---
    float harmonics = HarmonicWaves(centered, uTime, uScrollSpeed);
    
    // --- Noise for organic variation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.3;
    noiseUV.y += sin(uTime * 0.4) * 0.08;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.8, noiseTex.r, uHasSecondaryTex);
    
    // Apply noise to break perfect symmetry
    harmonics *= noiseVal;
    
    // --- Radial falloff ---
    float innerCutout = saturate(dist * 4.0);
    float outerFalloff = exp(-dist * dist * 6.0);
    float radialMask = innerCutout * outerFalloff;
    
    // --- Color gradient: inner warm gold, outer deep indigo ---
    float colorT = saturate(dist * 2.5);
    float3 serenadeColor = lerp(uSecondaryColor, uColor, colorT);
    
    // Harmonic highlights: brighter at wave crests
    float3 waveHighlight = lerp(uSecondaryColor, float3(0.95, 0.93, 1.0), 0.3);
    serenadeColor = lerp(serenadeColor, waveHighlight, harmonics * 0.4);
    
    // --- Angular star points (6-pointed star pattern) ---
    float starPoints = abs(sin(angle * 3.0 + uTime * 0.5));
    starPoints = pow(starPoints, 8.0) * 0.3;
    serenadeColor += starPoints * uSecondaryColor;
    
    // --- Musical breathing (gentle rhythmic pulse) ---
    float breathe = sin(uTime * 3.0) * 0.1 * uPhase + 0.9;
    
    // --- Phase brightness ---
    float phaseBrightness = 0.4 + uPhase * 0.6;
    
    // --- Final ---
    float3 finalColor = serenadeColor * uIntensity * phaseBrightness * breathe * baseTex.rgb;
    
    float alpha = harmonics * radialMask * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: SERENADE GLOW
// =============================================================================

float4 NachtmusikSerenadeGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    float radialFalloff = saturate(1.0 - dist * 1.4);
    radialFalloff = radialFalloff * radialFalloff;
    
    float3 glowColor = lerp(uColor, uSecondaryColor, uPhase * 0.4);
    
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.2;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.4);
    
    float pulse = sin(uTime * 2.0) * 0.05 + 0.95;
    
    glowColor *= uIntensity * noiseVal * baseTex.rgb;
    
    float alpha = radialFalloff * uOpacity * sampleColor.a * baseTex.a * pulse;
    
    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique NachtmusikSerenadePass
{
    pass P0
    {
        PixelShader = compile ps_3_0 NachtmusikSerenadePS();
    }
}

technique NachtmusikSerenadeGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 NachtmusikSerenadeGlowPS();
    }
}
