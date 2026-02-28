// ============================================================================
// CelebrationAura.fx — Ode to Joy Theme Shared Aura/Overlay Shader
// ============================================================================
// Screen-space aura effects: expanding rings of joy and floral sigil pattern.
// Color palette: warm gold, radiant amber, golden light, with green highlights.
//
// Techniques:
//   1. CelebrationAuraTechnique — Concentric golden rings expanding outward
//   2. FloralSigilTechnique     — Rotating flower-of-life botanical sigil
// ============================================================================

sampler uImage0 : register(s0); // Base texture (scene or overlay)

// --- Uniforms ---
float  uTime;           // Elapsed time in seconds
float3 uColor;          // Primary color (gold)
float3 uSecondaryColor; // Accent color (verdant green)
float  uOpacity;        // Overall opacity [0..1]
float  uIntensity;      // Brightness multiplier
float  uRadius;         // Effect radius in UV space [0..1]
float  uRingCount;      // Number of concurrent rings (CelebrationAura)
float  uRotation;       // Base rotation angle in radians (FloralSigil)

static const float PI  = 3.14159265;
static const float TAU = 6.28318530;

// ============================================================================
// Technique 1: CelebrationAuraTechnique
// Concentric golden rings expanding outward like sound waves of joy
// ============================================================================

float4 PS_CelebrationAura(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - 0.5;
    float dist = length(centered);

    // Accumulate multiple expanding rings
    float ringAccum = 0.0;
    float glowAccum = 0.0;

    // Ring count clamped to a reasonable range
    int rings = clamp((int)uRingCount, 1, 8);

    // Each ring has a different phase offset and speed
    for (int i = 0; i < rings; i++)
    {
        float phase = (float)i / (float)rings;       // [0..1) evenly spaced
        float speed = 0.3 + 0.15 * (float)i;         // Each ring expands at different speed

        // Ring position: expands outward, wraps around
        float ringPos = frac(uTime * speed + phase) * uRadius;

        // Ring thickness and fade
        float ringDist = abs(dist - ringPos);
        float ringWidth = 0.012 + 0.006 * (float)i;  // Outer rings slightly thicker

        // Ring intensity: fades as it expands outward
        float ageFade = 1.0 - frac(uTime * speed + phase); // 1 at birth, 0 at death
        ageFade = ageFade * ageFade; // Quadratic fade for nicer falloff

        // Sharp ring line
        float ring = exp(-ringDist * ringDist / (ringWidth * ringWidth)) * ageFade;

        // Soft glow around ring
        float glow = exp(-ringDist * ringDist / (ringWidth * ringWidth * 16.0)) * ageFade * 0.3;

        ringAccum += ring;
        glowAccum += glow;
    }

    // Clamp to avoid over-bright
    ringAccum = saturate(ringAccum);
    glowAccum = saturate(glowAccum);

    // Color: gold rings with slightly warmer (amber) glow
    float3 ringColor = uColor * 1.2;                        // Bright gold ring lines
    float3 glowColor = uColor * 0.8 + float3(0.1, 0.05, 0.0); // Warm amber glow

    // Subtle green shimmer at ring intersections
    float greenShimmer = ringAccum * glowAccum * 2.0;
    float3 accentColor = uSecondaryColor * greenShimmer * 0.3;

    float3 finalColor = ringColor * ringAccum + glowColor * glowAccum + accentColor;

    // Central hotspot: gentle golden glow at the origin
    float centerGlow = exp(-dist * dist * 40.0) * 0.2;
    finalColor += uColor * centerGlow;

    float alpha = (ringAccum + glowAccum + centerGlow) * uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique CelebrationAuraTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_CelebrationAura();
    }
}

// ============================================================================
// Technique 2: FloralSigilTechnique
// Rotating botanical sigil — overlapping circles forming flower-of-life pattern
// ============================================================================

// Draws a soft circle at a given center, returns intensity
float SoftCircle(float2 uv, float2 center, float radius, float softness)
{
    float d = length(uv - center);
    // Ring shape: bright at the edge, transparent in center and outside
    float ring = exp(-(d - radius) * (d - radius) / (softness * softness));
    return ring;
}

float4 PS_FloralSigil(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - 0.5;

    // Apply rotation
    float rot = uRotation + uTime * 0.2; // Slow continuous rotation
    float cosR = cos(rot);
    float sinR = sin(rot);
    float2 rotated;
    rotated.x = centered.x * cosR - centered.y * sinR;
    rotated.y = centered.x * sinR + centered.y * cosR;

    float outerRadius = uRadius * 0.4;
    float circleRadius = outerRadius * 0.5; // Radius of each petal circle
    float softness = 0.008;

    // Flower-of-life: 1 center circle + 6 surrounding circles
    float pattern = 0.0;

    // Center circle
    pattern += SoftCircle(rotated, float2(0.0, 0.0), circleRadius, softness);

    // 6 surrounding circles arranged in a hexagonal ring
    for (int i = 0; i < 6; i++)
    {
        float angle = (float)i * TAU / 6.0;
        float2 offset = float2(cos(angle), sin(angle)) * circleRadius;
        pattern += SoftCircle(rotated, offset, circleRadius, softness);
    }

    // Second ring of 6 circles (rotated 30 degrees, larger radius) for complexity
    for (int j = 0; j < 6; j++)
    {
        float angle2 = ((float)j + 0.5) * TAU / 6.0; // Offset by 30 degrees
        float2 offset2 = float2(cos(angle2), sin(angle2)) * circleRadius * 1.73; // sqrt(3)
        pattern += SoftCircle(rotated, offset2, circleRadius, softness * 1.2);
    }

    // Normalize intensity
    pattern = saturate(pattern * 0.35);

    // Color: gold base with green highlights where circles overlap
    float3 baseColor = uColor;

    // Detect overlaps (where pattern is brighter = more circles overlapping)
    float overlapIntensity = saturate(pattern * 2.0 - 0.4);
    float3 highlightColor = lerp(uColor, uSecondaryColor, overlapIntensity * 0.5);

    float3 finalColor = lerp(baseColor, highlightColor, 0.5);

    // Add subtle inner glow with time-varying warmth
    float breathe = sin(uTime * 1.5) * 0.5 + 0.5;
    finalColor += float3(0.15, 0.1, 0.0) * breathe * pattern;

    // Outer fade: sigil fades at edges of radius
    float distFromCenter = length(centered);
    float outerFade = 1.0 - smoothstep(uRadius * 0.3, uRadius * 0.5, distFromCenter);

    float alpha = pattern * outerFade * uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique FloralSigilTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_FloralSigil();
    }
}
