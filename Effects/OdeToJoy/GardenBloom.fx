// ============================================================================
// GardenBloom.fx — Ode to Joy Theme Shared Bloom/Glow Overlay Shader
// ============================================================================
// A warm golden bloom with petal-edge shimmer and jubilant pulse effects.
// Color palette: deep amber → burnished gold → radiant amber → jubilant gold
//                → golden light → pure joy white. Rose pink & verdant green accents.
//
// Techniques:
//   1. GardenBloomTechnique  — Radial soft bloom with 5-petal edge undulations
//   2. JubilantPulseTechnique — Rhythmic pulsing aura (heartbeat of joy)
// ============================================================================

sampler uImage0 : register(s0); // Base texture (if needed)

// --- Uniforms ---
float  uTime;           // Elapsed time in seconds
float3 uColor;          // Primary color (gold)
float3 uSecondaryColor; // Accent color (rose pink)
float  uOpacity;        // Overall opacity [0..1]
float  uIntensity;      // Bloom brightness multiplier
float  uRadius;         // Bloom radius [0..1] in UV space
float  uPulseSpeed;     // Pulse frequency (used by JubilantPulseTechnique)

// ============================================================================
// Shared helpers
// ============================================================================

static const float PI = 3.14159265;

// Soft radial falloff with 5-petal modulation on the edge
float PetalBloom(float2 uv, float radius, float time)
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // 5-petal shape: modulate the effective radius with sin(5*angle)
    // Shift petals slowly over time for gentle rotation
    float petalWave = sin(5.0 * angle + time * 0.4) * 0.5 + 0.5; // [0..1]
    float petalRadius = radius * (0.85 + 0.15 * petalWave);

    // Smooth falloff from center
    float bloom = 1.0 - smoothstep(0.0, petalRadius, dist);

    // Extra shimmer at the petal edges
    float edgeDist = abs(dist - petalRadius * 0.8);
    float shimmer = exp(-edgeDist * 30.0) * petalWave * 0.3;

    return bloom + shimmer;
}

// ============================================================================
// Technique 1: GardenBloomTechnique
// Radial soft bloom with petal-edge shimmer
// ============================================================================

float4 PS_GardenBloom(float2 coords : TEXCOORD0) : COLOR0
{
    float bloom = PetalBloom(coords, uRadius, uTime);

    // Color gradient: rose at edges → gold at center
    float2 centered = coords - 0.5;
    float dist = length(centered);
    float centerFactor = 1.0 - saturate(dist / max(uRadius, 0.001));

    float3 edgeColor  = uSecondaryColor; // Rose pink at edges
    float3 coreColor  = uColor;          // Gold at center
    float3 bloomColor = lerp(edgeColor, coreColor, centerFactor);

    // Add a bright white-gold highlight at the very center
    float hotspot = exp(-dist * dist * 80.0);
    bloomColor += float3(1.0, 0.95, 0.8) * hotspot * 0.5;

    float alpha = bloom * uOpacity * uIntensity;

    return float4(bloomColor * alpha, alpha);
}

technique GardenBloomTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_GardenBloom();
    }
}

// ============================================================================
// Technique 2: JubilantPulseTechnique
// Rhythmic pulsing aura — "heartbeat of joy"
// ============================================================================

float4 PS_JubilantPulse(float2 coords : TEXCOORD0) : COLOR0
{
    // Pulsing: oscillate intensity and effective radius with time
    // Use a combination of two sine waves for an organic heartbeat feel
    float pulse1 = sin(uTime * uPulseSpeed) * 0.5 + 0.5;           // Primary beat
    float pulse2 = sin(uTime * uPulseSpeed * 2.3 + 1.2) * 0.5 + 0.5; // Secondary harmonic
    float pulse  = lerp(pulse1, pulse2, 0.3); // Blend: mostly primary, touch of harmonic

    // Modulate radius and intensity with pulse
    float pulseRadius    = uRadius * (0.7 + 0.3 * pulse);
    float pulseIntensity = uIntensity * (0.6 + 0.4 * pulse);

    float bloom = PetalBloom(coords, pulseRadius, uTime);

    // Color: gold core, shifting slightly warmer on strong beats
    float2 centered = coords - 0.5;
    float dist = length(centered);
    float centerFactor = 1.0 - saturate(dist / max(pulseRadius, 0.001));

    float3 baseColor = lerp(uSecondaryColor, uColor, centerFactor);

    // On strong pulses, push toward bright jubilant white-gold
    float3 joyWhite = float3(1.0, 0.98, 0.85);
    baseColor = lerp(baseColor, joyWhite, pulse * 0.25 * centerFactor);

    // Expanding ring highlight on each pulse crest
    float ringDist = abs(dist - pulseRadius * pulse * 0.6);
    float ring = exp(-ringDist * ringDist * 200.0) * pulse * 0.4;
    baseColor += float3(1.0, 0.9, 0.6) * ring;

    float alpha = bloom * uOpacity * pulseIntensity;

    return float4(baseColor * alpha, alpha);
}

technique JubilantPulseTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_JubilantPulse();
    }
}
