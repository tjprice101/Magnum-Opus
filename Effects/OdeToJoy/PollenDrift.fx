// ============================================================================
// PollenDrift.fx — Ode to Joy Ranged Weapon Trail/Burst Shader
// ============================================================================
// Drifting pollen seed trail with scattered petal shapes and golden bloom.
// Used by: ThePollinator, ThornSprayRepeater, PetalStormCannon.
//
// UV convention:
//   coords.x = position along the trail (0 = start, 1 = tip)
//   coords.y = position across the trail width (0 = edge, 0.5 = center, 1 = edge)
//
// Techniques:
//   1. PollenTrailTechnique     — Projectile trail with floating seed shapes
//   2. BloomDetonationTechnique — Expanding floral detonation circle
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float  uTime;
float3 uColor;          // Primary: pollen gold / amber
float3 uSecondaryColor; // Accent: leaf green / rose pink
float  uOpacity;
float  uIntensity;
float  uRadius;         // Used by BloomDetonation

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
// Pollen seed shape — soft circles with slight elliptical distortion
// ============================================================================
float PollenSeed(float2 uv, float2 center, float size, float elongation)
{
    float2 diff = uv - center;
    diff.x *= elongation;
    float d = length(diff);
    return exp(-d * d / (size * size));
}

// ============================================================================
// Technique 1: PollenTrailTechnique
// Scattered pollen seeds drifting along a projectile trail
// ============================================================================
float4 PS_PollenTrail(float2 coords : TEXCOORD0) : COLOR0
{
    // --- Width and length falloff ---
    float centerDist = abs(coords.y - 0.5) * 2.0;
    float core = exp(-centerDist * centerDist * 5.0);
    float outer = 1.0 - smoothstep(0.0, 1.0, centerDist);
    float lengthFade = smoothstep(0.0, 0.2, coords.x) * (1.0 - smoothstep(0.8, 1.0, coords.x));

    // --- Scrolling golden energy base ---
    float2 scrollUV = coords;
    scrollUV.x += uTime * 1.0;
    float energy = ValueNoise(scrollUV * 5.0);
    float energy2 = ValueNoise(scrollUV * 10.0 + float2(uTime * 0.5, 0.0));
    energy = energy * 0.6 + energy2 * 0.4;
    energy = smoothstep(0.2, 0.8, energy);

    // --- Scatter pollen seed shapes ---
    float seeds = 0.0;
    for (int i = 0; i < 6; i++)
    {
        float phase = (float)i * 0.167;
        float seedX = frac(phase + uTime * (0.2 + 0.1 * (float)i));
        float seedY = 0.5 + sin(seedX * PI * 3.0 + (float)i * 1.7) * 0.25;
        float seedSize = 0.03 + 0.015 * sin(uTime * 2.0 + (float)i);
        float elongation = 1.0 + 0.5 * sin(uTime + (float)i * 0.8);
        seeds += PollenSeed(coords, float2(seedX, seedY), seedSize, elongation) * 0.5;
    }
    seeds = saturate(seeds);

    // --- Petal-like wisps at the edges ---
    float petalNoise = ValueNoise(coords * float2(6.0, 4.0) + float2(uTime * 0.3, uTime * 0.15));
    float petalWisps = smoothstep(0.5, 0.7, petalNoise) * outer * 0.4;

    // --- Color gradient: green edge → gold center → amber at tip ---
    float3 edgeColor = uSecondaryColor * 0.8;
    float3 coreColor = uColor * 1.2;
    float3 tipColor  = uColor + float3(0.1, 0.05, 0.0);

    float3 trailColor;
    if (centerDist < 0.4)
        trailColor = lerp(coreColor, tipColor, coords.x);
    else
        trailColor = lerp(coreColor, edgeColor, (centerDist - 0.4) / 0.6);

    // Seed particles get a bright golden highlight
    float3 seedColor = uColor * 1.5 + float3(0.2, 0.15, 0.0);

    // Petal wisps get a soft rose-gold tint
    float3 petalColor = lerp(uColor, uSecondaryColor, 0.4);

    // Composite
    float3 finalColor = trailColor * (core * 0.4 + outer * 0.2) * energy;
    finalColor += seedColor * seeds;
    finalColor += petalColor * petalWisps;

    // Bright sparkle at seed centers
    finalColor += float3(1.0, 0.97, 0.88) * seeds * seeds * 0.6;

    float alpha = (core * 0.4 + outer * 0.2 + seeds * 0.5 + petalWisps) * lengthFade * uOpacity * uIntensity;
    alpha *= (0.6 + 0.4 * energy);

    return float4(finalColor * alpha, alpha);
}

technique PollenTrailTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_PollenTrail();
    }
}

// ============================================================================
// Technique 2: BloomDetonationTechnique
// Expanding floral detonation circle — botanical explosion
// ============================================================================
float4 PS_BloomDetonation(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // --- 6-petal flower shape: radius modulated by cos(3*angle) ---
    float petalWave = cos(3.0 * angle + uTime * 0.5) * 0.5 + 0.5; // 6 petals
    float petalRadius = uRadius * (0.75 + 0.25 * petalWave);

    // Expanding ring — moves outward over time
    float ringPos = frac(uTime * 0.6) * uRadius;
    float ringDist = abs(dist - ringPos);
    float ring = exp(-ringDist * ringDist * 300.0) * (1.0 - frac(uTime * 0.6));

    // Bloom body — fills toward the petal radius
    float bloom = 1.0 - smoothstep(0.0, petalRadius, dist);

    // Scattered seed particles within the bloom area
    float seedNoise = ValueNoise(coords * 12.0 + uTime * 0.5);
    float seedDots = pow(seedNoise, 6.0) * bloom * 1.5;

    // --- Color: gold center → rose petals → green edge ---
    float centerFactor = 1.0 - saturate(dist / max(uRadius, 0.001));
    float3 centerCol = uColor * 1.4;          // Hot gold center
    float3 petalCol  = uSecondaryColor;        // Rose mid-ring
    float3 edgeCol   = uSecondaryColor * 0.5;  // Fading green edge

    float3 bloomColor;
    if (centerFactor > 0.6)
        bloomColor = lerp(centerCol, uColor, (1.0 - centerFactor) / 0.4);
    else
        bloomColor = lerp(uColor, petalCol, (0.6 - centerFactor) / 0.6);

    // Ring gets bright white-gold
    float3 ringColor = float3(1.0, 0.95, 0.8);

    // Hot center spot
    float hotspot = exp(-dist * dist * 60.0);
    float3 hotColor = float3(1.0, 0.97, 0.85);

    float3 finalColor = bloomColor * bloom * 0.6;
    finalColor += ringColor * ring * 0.8;
    finalColor += hotColor * hotspot * 0.5;
    finalColor += uColor * seedDots;

    float alpha = (bloom * 0.5 + ring + hotspot * 0.5 + seedDots * 0.3) * uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique BloomDetonationTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_BloomDetonation();
    }
}
