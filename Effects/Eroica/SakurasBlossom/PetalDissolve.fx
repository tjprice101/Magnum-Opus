// =============================================================================
// Petal Dissolve Shader - PS 2.0 Compatible
// =============================================================================
// Noise-driven petal-shaped dissolution mask for Sakura's Blossom spectral
// copy fade-out effect. Eats away at the sprite from edges inward, leaving
// sakura petal silhouettes before fully fading.
//
// UV Layout:
//   U (coords.x) = horizontal sprite position (0-1)
//   V (coords.y) = vertical sprite position (0-1)
//
// Techniques:
//   PetalDissolveMain  - Dissolving sprite with petal edge glow
//   PetalDissolveGlow  - Soft bloom overlay for dissolving edges
//
// Features:
//   - Procedural petal-shaped dissolution via polar noise mask
//   - uDissolveProgress controls dissolution (0=solid, 1=gone)
//   - Bright sakura edge glow at dissolution boundary
//   - Sakura pink ↁEgold gradient on dissolving edges
//   - Overbright multiplier for HDR bloom on edges
// =============================================================================

sampler uImage0 : register(s0); // Sprite texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;             // Primary color (Sakura pink)
float3 uSecondaryColor;    // Secondary color (Pollen gold)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uDissolveProgress;   // 0 = solid, 1 = fully dissolved
float uNoiseScale;          // Noise frequency (default 4.0)
float uHasSecondaryTex;     // 1.0 if noise texture bound

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float SmoothHash(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: PETAL DISSOLVE MAIN
// =============================================================================

float4 PetalDissolveMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Petal-shaped dissolution noise ---
    // Centre-relative for radial petal pattern
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // 5-petal modulation on noise
    float petalMod = cos(5.0 * angle + uTime * 0.5) * 0.15 + 0.85;

    // Procedural noise for dissolution mask
    float2 noiseP = coords * uNoiseScale;
    noiseP += float2(uTime * 0.15, uTime * 0.1);
    float procNoise = SmoothHash(noiseP);

    // Optional secondary texture noise
    float2 secUV = coords * uNoiseScale * 0.8;
    secUV += float2(uTime * 0.1, 0.0);
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(procNoise, noiseTex.r, uHasSecondaryTex * 0.6);

    // Combine noise with petal modulation and edge bias
    float dissolveMask = noiseVal * petalMod;
    dissolveMask = dissolveMask * 0.7 + dist * 0.3; // Edges dissolve first

    // --- Dissolution threshold ---
    float threshold = uDissolveProgress * 1.3; // Slightly overshoot for full dissolve
    float dissolved = step(dissolveMask, threshold);

    // --- Edge glow at dissolution boundary ---
    float edgeBand = saturate(1.0 - abs(dissolveMask - threshold) * 12.0);
    edgeBand *= dissolved; // Only glow where still visible

    // Edge glow colour: sakura ↁEgold
    float3 edgeColor = lerp(uColor, uSecondaryColor, uDissolveProgress);
    float3 brightEdge = lerp(edgeColor, float3(1.0, 0.96, 0.90), 0.4);

    // --- Composite ---
    float3 spriteColor = baseTex.rgb * sampleColor.rgb;
    float3 finalColor = lerp(spriteColor, brightEdge * uIntensity, edgeBand * 0.7);

    float alpha = dissolved * baseTex.a * sampleColor.a * uOpacity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: PETAL DISSOLVE GLOW
// =============================================================================

float4 PetalDissolveGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // Petal modulation
    float petalMod = cos(5.0 * angle + uTime * 0.5) * 0.15 + 0.85;

    // Noise
    float procNoise = SmoothHash(coords * uNoiseScale + float2(uTime * 0.15, uTime * 0.1));
    float dissolveMask = procNoise * petalMod * 0.7 + dist * 0.3;
    float threshold = uDissolveProgress * 1.3;

    // Wider edge band for glow
    float edgeBand = saturate(1.0 - abs(dissolveMask - threshold) * 6.0);

    // Soft glow colour
    float3 glowColor = lerp(uColor, float3(1.0, 0.85, 0.88), 0.3);
    glowColor *= uIntensity * baseTex.rgb;

    // Radial falloff
    float radial = saturate(1.0 - dist * 0.8);

    float pulse = sin(uTime * 2.5) * 0.08 + 0.92;

    float alpha = edgeBand * radial * uOpacity * sampleColor.a * baseTex.a * pulse * 0.5;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique PetalDissolveMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PetalDissolveMainPS();
    }
}

technique PetalDissolveGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PetalDissolveGlowPS();
    }
}
