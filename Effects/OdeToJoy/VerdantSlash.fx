// ============================================================================
// VerdantSlash.fx — Ode to Joy Melee Weapon Trail Shader
// ============================================================================
// Vine-entwined slash trail with growing thorns and tendril weave patterns.
// Used by: TheGardenersFury, ThornboundReckoning, RoseThornChainsaw.
//
// UV convention:
//   coords.x = position along the trail (0 = start/old, 1 = tip/new)
//   coords.y = position across the trail width (0 = edge, 0.5 = center, 1 = edge)
//
// Techniques:
//   1. VerdantSlashTechnique   — Main swing trail with vine tendrils and thorn pulses
//   2. ThornImpactTechnique    — Radial thorn-burst impact effect for on-hit VFX
// ============================================================================

sampler uImage0 : register(s0); // Trail base texture / gradient map
sampler uImage1 : register(s1); // Noise texture for vine generation

// --- Uniforms ---
float  uTime;
float3 uColor;          // Primary: verdant green or forest green
float3 uSecondaryColor; // Accent: golden pollen or rose pink
float  uOpacity;
float  uIntensity;
float  uComboProgress;  // 0..1 representing combo buildup (optional, for Gardener's Fury)

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
// Vine tendril pattern — sinusoidal fibers that weave through the trail
// ============================================================================
float VineTendril(float2 coords, float time, float freq, float phase)
{
    // Multiple vine strands at different frequencies
    float vine1 = sin(coords.x * freq * PI + time * 1.5 + phase) * 0.12;
    float vine2 = sin(coords.x * freq * PI * 1.7 + time * 0.8 + phase * 2.3) * 0.08;
    float vine3 = sin(coords.x * freq * PI * 2.3 + time * 2.1 + phase * 0.7) * 0.05;

    float vineCenter = 0.5 + vine1 + vine2 + vine3;

    // Sharp vine stroke
    float vineDist = abs(coords.y - vineCenter);
    float vineWidth = 0.025 + 0.015 * sin(coords.x * 8.0 + time);
    float vine = exp(-vineDist * vineDist / (vineWidth * vineWidth));

    return vine;
}

// ============================================================================
// Thorn shape — small triangular protrusions along the vine
// ============================================================================
float ThornShape(float2 coords, float time)
{
    // Thorns appear at regular intervals along the trail
    float thornSpacing = 0.12;
    float thornPhase = frac(coords.x / thornSpacing + time * 0.3);

    // Sharp triangular shape
    float thornX = abs(thornPhase - 0.5) * 2.0;
    float thornY = abs(coords.y - 0.5) * 2.0;

    // Only show thorns near the vine center
    float thornMask = smoothstep(0.6, 0.3, thornY);
    float thorn = smoothstep(0.5, 0.0, thornX) * thornMask;
    thorn *= step(thornY, 0.3 + thornX * 0.2); // Triangular falloff

    // Pulse thorns with time for a "growing" effect
    float pulse = sin(thornPhase * PI) * 0.5 + 0.5;
    return thorn * pulse * 0.6;
}

// ============================================================================
// Technique 1: VerdantSlashTechnique
// ============================================================================
float4 PS_VerdantSlash(float2 coords : TEXCOORD0) : COLOR0
{
    // --- Width falloff ---
    float centerDist = abs(coords.y - 0.5) * 2.0;
    float core = exp(-centerDist * centerDist * 6.0);
    float outer = 1.0 - smoothstep(0.0, 1.0, centerDist);

    // --- Length falloff ---
    float lengthFade = smoothstep(0.0, 0.15, coords.x) * (1.0 - smoothstep(0.85, 1.0, coords.x));

    // --- Multiple vine tendrils ---
    float vine1 = VineTendril(coords, uTime, 4.0, 0.0);
    float vine2 = VineTendril(coords, uTime, 5.5, 1.5);
    float vine3 = VineTendril(coords, uTime, 3.2, 3.0);
    float vines = max(vine1, max(vine2, vine3));

    // --- Thorns along the vine fibers ---
    float thorns = ThornShape(coords, uTime);

    // --- Scrolling energy underneath the vines ---
    float2 scrollUV = coords;
    scrollUV.x += uTime * 0.6;
    float energy = ValueNoise(scrollUV * 8.0);
    energy = smoothstep(0.3, 0.7, energy);

    // --- Color gradient: deep green → verdant → gold tips ---
    float3 deepGreen  = uColor * 0.4;
    float3 verdant    = uColor;
    float3 goldenTips = uSecondaryColor * 1.2;

    // Trail body color: green at base, gold toward tip
    float3 bodyColor;
    if (coords.x < 0.6)
        bodyColor = lerp(deepGreen, verdant, coords.x / 0.6);
    else
        bodyColor = lerp(verdant, goldenTips, (coords.x - 0.6) / 0.4);

    // Vine strands get brighter green
    float3 vineColor = uColor * 1.5;

    // Thorn protrusions get a gold-rose highlight
    float3 thornColor = lerp(uSecondaryColor, float3(1.0, 0.95, 0.85), 0.3);

    // Composite layers
    float3 finalColor = bodyColor * (core * 0.3 + outer * 0.2) * energy;
    finalColor += vineColor * vines;
    finalColor += thornColor * thorns;

    // White-hot highlight at the very center of the core
    finalColor += float3(1.0, 0.98, 0.88) * core * core * 0.3;

    // Combo progress enhances brightness (for Gardener's Fury stacking)
    float comboBoost = 1.0 + uComboProgress * 0.5;
    finalColor *= comboBoost;

    float alpha = (core * 0.4 + outer * 0.3 + vines * 0.5 + thorns * 0.3) * lengthFade * uOpacity * uIntensity;
    alpha *= (0.7 + 0.3 * energy);

    return float4(finalColor * alpha, alpha);
}

technique VerdantSlashTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_VerdantSlash();
    }
}

// ============================================================================
// Technique 2: ThornImpactTechnique
// Radial thorn-burst impact for melee on-hit VFX
// ============================================================================
float4 PS_ThornImpact(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Radial thorn spikes: 8 directions with jagged edges
    float spikes = 0.0;
    for (int i = 0; i < 8; i++)
    {
        float spikeAngle = (float)i * TAU / 8.0;
        float angleDiff = abs(angle - spikeAngle);
        angleDiff = min(angleDiff, TAU - angleDiff);

        float spikeWidth = 0.15 + 0.05 * sin(uTime * 3.0 + (float)i);
        float spike = exp(-angleDiff * angleDiff / (spikeWidth * spikeWidth * 0.01));

        // Spike length varies with time — expanding outward
        float spikeLength = 0.35 + 0.1 * sin(uTime * 5.0 + (float)i * 0.7);
        spike *= smoothstep(spikeLength, spikeLength * 0.3, dist);

        spikes += spike;
    }

    spikes = saturate(spikes);

    // Central green glow
    float centerGlow = exp(-dist * dist * 30.0);

    // Color: green center → golden spikes → rose tips
    float3 centerColor = uColor * 1.3;
    float3 spikeColor = lerp(uColor, uSecondaryColor, dist * 2.0);
    spikeColor += float3(1.0, 0.95, 0.85) * spikes * 0.3; // White-hot tips

    float3 finalColor = centerColor * centerGlow + spikeColor * spikes;

    float alpha = (centerGlow + spikes) * uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique ThornImpactTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_ThornImpact();
    }
}
