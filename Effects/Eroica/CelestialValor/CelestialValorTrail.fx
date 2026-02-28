// =============================================================================
// Celestial Valor Trail Shader - Heroic Greatsword Energy
// =============================================================================
// The great hero's blade leaves a trail of standing-wave harmonic energy.
// Scarlet -> Gold gradient with constellation spark nodes, harmonic ripple
// waves that pulse like a struck chord, and an energy afterimage cascade.
//
// VISUAL IDENTITY: Like a hero's battle hymn made visible -- standing waves
// of golden light with scarlet fire edges, star-bright nodes where harmonics
// converge, each swing resonates with the echo of triumphant chords.
//
// Techniques:
//   HeroicTrail   - Main trail with harmonic standing waves & spark nodes
//   ValorFlare    - Radial impact overlay with 4-fold heroic crest pattern
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;           // Primary: scarlet (0.78, 0.20, 0.20)
float3 uSecondaryColor;  // Secondary: gold  (1.0, 0.84, 0.0)
float uOpacity;
float uTime;
float uIntensity;        // Overall brightness multiplier
float uProgress;         // 0-1 along trail length

// Overbright / glow uniforms
float uOverbrightMult;   // HDR multiplier (2-5 recommended)
float uGlowThreshold;    // Glow cutoff (default 0.5)
float uGlowIntensity;    // Extra glow boost (default 1.5)
float uHasSecondaryTex;  // 1.0 if uImage1 is bound
float uSecondaryTexScale; // UV scale for noise
float uSecondaryTexScroll; // Scroll speed for noise

// =============================================================================
// UTILITY
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// =============================================================================
// TECHNIQUE 1: HEROIC TRAIL - Standing Wave Harmonics
// =============================================================================
// Standing-wave harmonic pattern: multiple sine frequencies overlap to create
// bright "harmonic nodes" along the trail -- like the resonant points on a
// vibrating string. Constellation spark dots appear at node peaks.
// =============================================================================

float4 HeroicTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Edge-to-centre intensity ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Progress fade (trail fades toward tail) ---
    float progressFade = saturate(1.0 - coords.x * 1.1);

    // --- Standing wave harmonics (3 overlapping frequencies) ---
    // Fundamental frequency
    float wave1 = sin(coords.x * 18.0 - uTime * 6.0) * 0.5 + 0.5;
    // Second harmonic (2x freq, different phase)
    float wave2 = sin(coords.x * 36.0 - uTime * 12.0 + 1.57) * 0.5 + 0.5;
    // Third harmonic (3x freq)
    float wave3 = sin(coords.x * 54.0 - uTime * 18.0 + 3.14) * 0.5 + 0.5;

    // Combine harmonics -- peaks where all three converge create "nodes"
    float harmonic = wave1 * 0.5 + wave2 * 0.3 + wave3 * 0.2;
    harmonic = pow(harmonic, 1.5); // Sharpen the peaks

    // --- Harmonic node sparks (constellation points at wave peaks) ---
    float nodePeak = saturate((harmonic - 0.65) * 5.0);
    // Sparkle at node positions using hash noise for star-like twinkle
    float sparkle = HashNoise(float2(floor(coords.x * 12.0), floor(uTime * 3.0)));
    float nodeSpark = nodePeak * step(0.6, sparkle) * 2.0;

    // --- Heroic gradient with harmonic modulation ---
    float gradT = coords.x * 0.6 + harmonic * 0.25;
    float3 heroGrad = lerp(uColor, uSecondaryColor, gradT);

    // --- White-hot core at centre, widened at harmonic peaks ---
    float coreWidth = 0.65 - harmonic * 0.12; // Core widens at wave peaks
    float coreMask = saturate((edgeFade - coreWidth) * 4.0);
    float3 coreColor = lerp(heroGrad, float3(1.0, 0.98, 0.92), coreMask);

    // --- Sakura-pink tint at soft outer edge ---
    float outerMask = saturate((0.45 - edgeFade) * 3.0);
    float3 sakuraTint = float3(1.0, 0.59, 0.71);
    coreColor = lerp(coreColor, sakuraTint, outerMask * 0.35);

    // --- Energy ripple perpendicular to trail (cross-wave shimmer) ---
    float crossWave = sin(coords.y * 16.0 + coords.x * 8.0 - uTime * 10.0);
    crossWave = crossWave * 0.5 + 0.5;
    float rippleGlow = crossWave * edgeFade * 0.15;

    // --- Constellation spark nodes (white-gold star flashes) ---
    float3 sparkColor = float3(1.0, 0.95, 0.82);
    coreColor += sparkColor * nodeSpark * edgeFade;

    // --- Noise turbulence ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noise = tex2D(uImage1, noiseUV);
    coreColor *= lerp(1.0, noise.r * 1.2 + 0.4, uHasSecondaryTex * 0.35);

    // --- Harmonic pulse (resonant throb at standing wave frequency) ---
    float pulse = 0.90 + sin(uTime * 5.0 + coords.x * 8.0) * 0.06;
    pulse += harmonic * 0.08; // Brighter at harmonic peaks

    // --- Afterimage cascade (fading energy echoes offset from main trail) ---
    float echo1 = QuadraticBump(saturate(coords.y + 0.06));
    float echo2 = QuadraticBump(saturate(coords.y - 0.06));
    float echoMask = (echo1 + echo2) * 0.12 * progressFade;

    // --- Combine ---
    float alpha = (edgeFade + echoMask + rippleGlow) * progressFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    float3 finalColor = coreColor * baseTex.rgb * uIntensity * (1.0 + harmonic * 0.2);

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: VALOR FLARE - Heroic Crest Bloom
// =============================================================================
// 4-fold heroic crest pattern impact overlay instead of plain radial.
// Creates a shield-shaped cross burst with golden rays and scarlet corona.
// =============================================================================

float4 ValorFlarePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // Soft radial falloff base
    float radial = saturate(1.0 - dist * dist);

    // --- 4-fold heroic crest pattern (cross shape with rounded lobes) ---
    float crest = cos(angle * 2.0) * 0.5 + 0.5; // 4-fold symmetry
    crest = pow(crest, 0.6); // Soften the lobes
    float crestMask = radial * (0.7 + crest * 0.3);

    // --- 8 golden rays radiating outward ---
    float rays = cos(angle * 4.0 + uTime * 2.0) * 0.5 + 0.5;
    rays = pow(rays, 4.0); // Very thin ray lines
    float rayMask = rays * saturate(dist * 2.0) * saturate(1.0 - dist * 1.3) * 0.4;

    // --- Colour: Gold core -> scarlet corona -> sakura fringe ---
    float3 flareColor = lerp(uSecondaryColor, uColor, dist * 0.8);
    flareColor = lerp(flareColor, float3(1.0, 0.98, 0.90), radial * radial); // white-hot centre

    // Ray colour (slightly different from core)
    float3 rayColor = lerp(uSecondaryColor, float3(1.0, 0.92, 0.70), 0.5);

    // --- Pulsing intensity with heroic rhythm ---
    float pulse = 0.85 + sin(uTime * 6.0) * 0.10;
    pulse += sin(uTime * 3.0) * 0.05; // Slower secondary pulse

    float alpha = (crestMask + rayMask) * uOpacity * sampleColor.a * baseTex.a * pulse;
    float3 finalColor = (flareColor * crestMask + rayColor * rayMask) * baseTex.rgb * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique HeroicTrail
{
    pass P0
    {
        PixelShader = compile ps_3_0 HeroicTrailPS();
    }
}

technique ValorFlare
{
    pass P0
    {
        PixelShader = compile ps_3_0 ValorFlarePS();
    }
}
