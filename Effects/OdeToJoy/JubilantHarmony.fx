// ============================================================================
// JubilantHarmony.fx — Ode to Joy Magic/Summoner Weapon Shader
// ============================================================================
// Musical harmonic wave patterns with standing wave nodes and staff-line motifs.
// Used by: AnthemOfGlory, ElysianVerdict, HymnOfTheVictorious,
//          TriumphantChorus, TheStandingOvation, FountainOfJoyousHarmony.
//
// UV convention:
//   coords.x = position along effect (0 = start, 1 = end)
//   coords.y = position across width (0 = edge, 0.5 = center, 1 = edge)
//
// Techniques:
//   1. HarmonicBeamTechnique   — Beam/bolt trail with harmonic wave modulation
//   2. SymphonicAuraTechnique  — Expanding radial aura with musical pulse
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float  uTime;
float3 uColor;          // Primary: brilliant gold
float3 uSecondaryColor; // Accent: rose pink or verdant green
float  uOpacity;
float  uIntensity;
float  uRadius;         // Used by SymphonicAura
float  uHarmonicFreq;   // Musical frequency multiplier (default ~1.0)

static const float PI  = 3.14159265;
static const float TAU = 6.28318530;

// ============================================================================
// Pseudo-noise
// ============================================================================
float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = Hash21(i);
    float b = Hash21(i + float2(1.0, 0.0));
    float c = Hash21(i + float2(0.0, 1.0));
    float d = Hash21(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ============================================================================
// Musical staff lines — horizontal lines like sheet music staff
// ============================================================================
float StaffLines(float y, int lineCount)
{
    float lines = 0.0;
    float spacing = 1.0 / (float)(lineCount + 1);
    for (int i = 1; i <= lineCount; i++)
    {
        float lineY = spacing * (float)i;
        float lineDist = abs(y - lineY);
        lines += exp(-lineDist * lineDist * 2000.0); // Very thin lines
    }
    return saturate(lines);
}

// ============================================================================
// Standing wave pattern — nodes and antinodes of a vibrating string
// ============================================================================
float StandingWave(float x, float y, float time, float freq)
{
    // Multiple harmonics superimposed
    float wave1 = sin(x * PI * freq) * sin(time * 3.0) * 0.15;
    float wave2 = sin(x * PI * freq * 2.0) * sin(time * 4.7 + 0.5) * 0.08;
    float wave3 = sin(x * PI * freq * 3.0) * sin(time * 6.1 + 1.0) * 0.04;

    float waveY = 0.5 + wave1 + wave2 + wave3;
    float waveDist = abs(y - waveY);

    // Glow around the wave
    float waveGlow = exp(-waveDist * waveDist * 80.0);

    // Bright nodes at standing wave positions (where amplitude = 0)
    float nodeX = sin(x * PI * freq);
    float nodeBrightness = 1.0 - abs(nodeX); // Bright at nodes
    nodeBrightness = pow(max(0.0, nodeBrightness), 4.0);
    float nodeGlow = nodeBrightness * exp(-waveDist * waveDist * 200.0) * 0.5;

    return waveGlow + nodeGlow;
}

// ============================================================================
// Technique 1: HarmonicBeamTechnique
// Beam/bolt trail with harmonic wave modulation and staff-line undertone
// ============================================================================
float4 PS_HarmonicBeam(float2 coords : TEXCOORD0) : COLOR0
{
    float freq = max(uHarmonicFreq, 1.0);

    // --- Width and length falloff ---
    float centerDist = abs(coords.y - 0.5) * 2.0;
    float core = exp(-centerDist * centerDist * 8.0);
    float outer = 1.0 - smoothstep(0.0, 1.0, centerDist);
    float lengthFade = smoothstep(0.0, 0.15, coords.x) * (1.0 - smoothstep(0.85, 1.0, coords.x));

    // --- Standing wave pattern across the beam ---
    float wave = StandingWave(coords.x, coords.y, uTime, freq * 3.0);

    // --- Staff lines (very subtle, like ghostly sheet music) ---
    float staff = StaffLines(coords.y, 5) * 0.15;
    staff *= smoothstep(0.0, 0.3, coords.x) * smoothstep(1.0, 0.7, coords.x);

    // --- Scrolling energy ---
    float2 scrollUV = coords;
    scrollUV.x += uTime * 0.8;
    float energy = ValueNoise(scrollUV * 6.0);
    energy = smoothstep(0.25, 0.75, energy);

    // --- Note sparkles: bright points that look like musical notes ---
    float noteNoise = ValueNoise(coords * float2(15.0, 10.0) + uTime * 0.4);
    float noteSparkle = pow(noteNoise, 8.0) * core * 2.0;

    // --- Color gradient: deep gold → brilliant gold → white-gold core ---
    float3 deepColor = uColor * 0.5;
    float3 brightColor = uColor * 1.3;
    float3 coreGlow = float3(1.0, 0.96, 0.85);

    float3 beamColor;
    if (centerDist < 0.3)
        beamColor = lerp(coreGlow, brightColor, centerDist / 0.3);
    else
        beamColor = lerp(brightColor, deepColor, (centerDist - 0.3) / 0.7);

    // Standing wave areas get accent color highlights
    float3 waveColor = lerp(uColor, uSecondaryColor, 0.4) * 1.5;

    // Staff lines get subtle secondary color
    float3 staffColor = uSecondaryColor * 0.6;

    // Composite
    float3 finalColor = beamColor * (core * 0.5 + outer * 0.3) * (0.6 + 0.4 * energy);
    finalColor += waveColor * wave;
    finalColor += staffColor * staff;
    finalColor += coreGlow * noteSparkle;

    // Bright white-hot center line
    float centerLine = exp(-centerDist * centerDist * 30.0);
    finalColor += float3(1.0, 0.98, 0.9) * centerLine * 0.3;

    float alpha = (core * 0.5 + outer * 0.25 + wave * 0.4 + noteSparkle * 0.3) * lengthFade * uOpacity * uIntensity;
    alpha *= (0.7 + 0.3 * energy);

    return float4(finalColor * alpha, alpha);
}

technique HarmonicBeamTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_HarmonicBeam();
    }
}

// ============================================================================
// Technique 2: SymphonicAuraTechnique
// Expanding radial aura with concentric harmonic rings and musical pulse
// ============================================================================
float4 PS_SymphonicAura(float2 coords : TEXCOORD0) : COLOR0
{
    float freq = max(uHarmonicFreq, 1.0);

    float2 centered = coords - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // --- Multiple harmonic rings: each at a different overtone frequency ---
    float rings = 0.0;
    for (int i = 1; i <= 4; i++)
    {
        // Ring positions based on harmonic series: 1, 2, 3, 4
        float harmonic = (float)i;
        float ringRadius = uRadius * harmonic / 5.0;

        // Each ring pulses at its harmonic frequency
        float pulse = sin(uTime * freq * harmonic * 0.5 + harmonic * 0.7) * 0.5 + 0.5;
        ringRadius *= (0.85 + 0.15 * pulse);

        float ringDist = abs(dist - ringRadius);
        float ringWidth = 0.008 + 0.004 / harmonic; // Higher harmonics are thinner
        float ring = exp(-ringDist * ringDist / (ringWidth * ringWidth));

        // Fade higher harmonics
        ring *= 1.0 / harmonic;

        rings += ring;
    }
    rings = saturate(rings);

    // --- Note-shaped modulation: 8 bright spots around the aura ---
    float noteSpots = 0.0;
    for (int j = 0; j < 8; j++)
    {
        float noteAngle = (float)j * TAU / 8.0 + uTime * 0.3;
        float noteRadius = uRadius * 0.4 * (1.0 + 0.2 * sin(uTime * 2.0 + (float)j));
        float2 notePos = float2(cos(noteAngle), sin(noteAngle)) * noteRadius;
        float noteDist = length(centered - notePos);
        float note = exp(-noteDist * noteDist * 500.0);
        noteSpots += note;
    }
    noteSpots = saturate(noteSpots);

    // --- Central glow ---
    float centerGlow = exp(-dist * dist * 40.0) * 0.4;

    // --- Gentle radial gradient color ---
    float centerFactor = 1.0 - saturate(dist / max(uRadius, 0.001));
    float3 centerColor = uColor * 1.3;
    float3 edgeColor = uSecondaryColor * 0.8;
    float3 auraColor = lerp(edgeColor, centerColor, centerFactor);

    // Rings get bright gold
    float3 ringColor = uColor * 1.5 + float3(0.1, 0.08, 0.0);

    // Note spots get white-gold sparkle
    float3 noteColor = float3(1.0, 0.96, 0.85);

    float3 finalColor = auraColor * centerGlow;
    finalColor += ringColor * rings;
    finalColor += noteColor * noteSpots * 0.7;
    finalColor += centerColor * centerGlow;

    float alpha = (centerGlow + rings + noteSpots * 0.5) * uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique SymphonicAuraTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_SymphonicAura();
    }
}
