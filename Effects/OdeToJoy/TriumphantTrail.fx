// ============================================================================
// TriumphantTrail.fx — Ode to Joy Theme Shared Trail Shader
// ============================================================================
// Golden energy trail with gradient coloring and blossom-wind variant.
// Color palette: verdant green edge → amber center → bright gold core.
//
// UV convention:
//   coords.x = position along the trail (0 = start, 1 = tip)
//   coords.y = position across the trail width (0 = edge, 0.5 = center, 1 = edge)
//
// Techniques:
//   1. TriumphantTrailTechnique  — Golden energy trail with UV-scrolling flow
//   2. BlossomWindTrailTechnique — Dappled petal/particle scatter along trail
// ============================================================================

sampler uImage0 : register(s0); // Trail base texture / gradient map
sampler uImage1 : register(s1); // Noise texture (used by BlossomWindTrailTechnique)

// --- Uniforms ---
float  uTime;           // Elapsed time in seconds
float3 uColor;          // Primary trail color (gold / amber)
float3 uSecondaryColor; // Edge accent color (verdant green)
float  uOpacity;        // Overall opacity [0..1]
float  uIntensity;      // Brightness multiplier

static const float PI = 3.14159265;

// ============================================================================
// Simple pseudo-noise for when we don't want to sample uImage1
// ============================================================================
float Hash21(float2 p)
{
    p = frac(p * float2(123.34, 456.21));
    p += dot(p, p + 45.32);
    return frac(p.x * p.y);
}

// Smooth value noise
float ValueNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f); // smoothstep

    float a = Hash21(i);
    float b = Hash21(i + float2(1.0, 0.0));
    float c = Hash21(i + float2(0.0, 1.0));
    float d = Hash21(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ============================================================================
// Technique 1: TriumphantTrailTechnique
// Golden energy trail with flowing UV-scroll
// ============================================================================

float4 PS_TriumphantTrail(float2 coords : TEXCOORD0) : COLOR0
{
    // --- Width falloff: brightest at center (y=0.5), fading to edges ---
    float centerDist = abs(coords.y - 0.5) * 2.0; // [0..1] from center
    float widthFalloff = 1.0 - smoothstep(0.0, 1.0, centerDist);

    // Sharper bright core, softer outer glow
    float core  = exp(-centerDist * centerDist * 8.0);  // Tight bright center
    float outer = widthFalloff;                           // Soft falloff

    // --- Length falloff: fade toward trail start, sharp at tip ---
    float lengthFade = smoothstep(0.0, 0.3, coords.x) * (1.0 - smoothstep(0.85, 1.0, coords.x));

    // --- UV-scrolling energy pattern ---
    float2 scrollUV = coords;
    scrollUV.x += uTime * 0.8;  // Scroll along trail
    scrollUV.y *= 2.0;          // Stretch across width

    // Two octaves of procedural noise for energy flow
    float energy1 = ValueNoise(scrollUV * 6.0);
    float energy2 = ValueNoise(scrollUV * 12.0 + float2(uTime * 0.3, 0.0));
    float energy  = energy1 * 0.6 + energy2 * 0.4;
    energy = smoothstep(0.25, 0.75, energy); // Contrast boost

    // --- Color gradient: green edge → amber → bright gold core ---
    //   centerDist: 0 = center, 1 = edge
    float3 coreColor  = uColor * 1.3;                          // Bright gold at center
    float3 midColor   = uColor;                                 // Amber in the middle
    float3 edgeColor  = uSecondaryColor;                        // Verdant green at edges

    float3 trailColor;
    if (centerDist < 0.4)
        trailColor = lerp(coreColor, midColor, centerDist / 0.4);
    else
        trailColor = lerp(midColor, edgeColor, (centerDist - 0.4) / 0.6);

    // Energy modulation adds shimmer
    trailColor += float3(0.15, 0.12, 0.0) * energy * core;

    // White-hot highlight at the very center
    trailColor += float3(1.0, 0.95, 0.8) * core * core * 0.4;

    float alpha = (core * 0.6 + outer * 0.4) * lengthFade * uOpacity * uIntensity;
    alpha *= (0.7 + 0.3 * energy); // Energy modulates opacity slightly

    return float4(trailColor * alpha, alpha);
}

technique TriumphantTrailTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_TriumphantTrail();
    }
}

// ============================================================================
// Technique 2: BlossomWindTrailTechnique
// Dappled petal/light scatter along the trail, like petals in warm wind
// ============================================================================

float4 PS_BlossomWindTrail(float2 coords : TEXCOORD0) : COLOR0
{
    // --- Base width and length falloff (same as above) ---
    float centerDist = abs(coords.y - 0.5) * 2.0;
    float widthFalloff = 1.0 - smoothstep(0.0, 1.0, centerDist);
    float lengthFade = smoothstep(0.0, 0.2, coords.x) * (1.0 - smoothstep(0.8, 1.0, coords.x));

    // --- Sample noise texture for petal shapes ---
    float2 noiseUV1 = coords * float2(4.0, 3.0) + float2(uTime * 0.5, uTime * 0.15);
    float2 noiseUV2 = coords * float2(6.0, 4.5) + float2(uTime * 0.3, -uTime * 0.2);

    float noise1 = tex2D(uImage1, frac(noiseUV1)).r;
    float noise2 = tex2D(uImage1, frac(noiseUV2)).g;

    // Create petal-like dappled shapes by thresholding noise
    float petals = smoothstep(0.45, 0.65, noise1) * smoothstep(0.4, 0.6, noise2);

    // Softer scattered sparkles from a different noise frequency
    float2 sparkleUV = coords * float2(10.0, 7.0) + float2(uTime * 0.7, uTime * 0.1);
    float sparkle = tex2D(uImage1, frac(sparkleUV)).b;
    sparkle = pow(sparkle, 4.0) * 1.5; // Sharp bright sparkles

    // --- Color: warm gold base with rose petal accents ---
    float3 baseColor = uColor * (0.8 + 0.2 * widthFalloff);

    // Petals get a rose-pink tint
    float3 petalColor = lerp(uColor, uSecondaryColor, 0.5);

    float3 finalColor = baseColor * widthFalloff * 0.3;     // Soft base glow
    finalColor += petalColor * petals * 0.7;                  // Petal shapes
    finalColor += float3(1.0, 0.97, 0.85) * sparkle * 0.4;  // Golden sparkles

    // Slight green tint at edges
    finalColor += uSecondaryColor * 0.15 * (1.0 - widthFalloff);

    float alpha = (widthFalloff * 0.3 + petals * 0.5 + sparkle * 0.2) * lengthFade;
    alpha *= uOpacity * uIntensity;

    return float4(finalColor * alpha, alpha);
}

technique BlossomWindTrailTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_BlossomWindTrail();
    }
}
