// =============================================================================
// MagnumOpus Scrolling Trail Shader - PS 2.0 Compatible
// =============================================================================
// UV-scrolling trails with animated effects for Calamity-tier quality.
// Techniques: ScrollFlame, ScrollCosmic, ScrollEnergy, ScrollVoid, ScrollHoly
//
// Features:
//   - Secondary texture sampler (uImage1) for noise/trail textures
//   - Overbright multiplier for HDR-like bloom (2-7x)
//   - Conditional glow threshold for bright areas
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise/trail texture for detail

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uProgress; // overall animation progress
float uScrollSpeed;    // UV scroll rate (default: 1.0)
float uNoiseScale;     // procedural noise frequency (default: 4.0)

// --- New uniforms for Calamity-tier rendering ---
float uOverbrightMult;   // Overbright multiplier (1.0 = normal, 2-7 = HDR glow)
float uGlowThreshold;    // Brightness threshold for conditional glow (0-1, default 0.5)
float uGlowIntensity;    // Extra multiplier when above threshold (default 1.5)
float uSecondaryTexScale; // UV scale for secondary texture (default 1.0)
float uSecondaryTexScroll; // Extra scroll speed for secondary texture (default 0.5)
float uHasSecondaryTex;  // 1.0 if secondary texture is bound, 0.0 if not

// =============================================================================
// UTILITY: Apply overbright to final color (minimal instruction cost)
// =============================================================================
float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// UTILITY: Procedural noise approximation (PS 2.0 safe)
// Cheap hash-based noise using sine products
// =============================================================================
float PseudoNoise(float2 uv)
{
    // Simple deterministic pseudo-random based on dot product
    return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
}

float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    
    // Smooth interpolation
    float2 u = f * f * (3.0 - 2.0 * f);
    
    // 4 corner samples
    float a = PseudoNoise(i);
    float b = PseudoNoise(i + float2(1.0, 0.0));
    float c = PseudoNoise(i + float2(0.0, 1.0));
    float d = PseudoNoise(i + float2(1.0, 1.0));
    
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// QuadraticBump: 0→1→0 over input 0→1 (peak at 0.5)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// =============================================================================
// STYLE 1: SCROLL FLAME - Scrolling fire with heat distortion
// =============================================================================
float4 ScrollFlameTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Scroll UVs for animated fire effect
    float2 scrollUV = coords;
    scrollUV.x -= uTime * uScrollSpeed * 0.8;
    scrollUV.y += sin(coords.x * 6.0 + uTime * 3.0) * 0.05; // heat shimmer
    
    float4 baseColor = tex2D(uImage0, scrollUV);
    
    // Secondary texture for turbulence detail (inline, branch-free)
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    
    // Edge-to-center intensity (bright center, fades at edges)
    float edgeFade = QuadraticBump(coords.y);
    
    // Flame shape: edgeFade + texture noise for organic look
    float flame = saturate(edgeFade + noiseTex.r * 0.3 * uHasSecondaryTex - 0.05);
    
    // Fire gradient: branch-free white core -> primary -> secondary
    float3 color = lerp(uSecondaryColor, uColor, saturate(edgeFade * 1.43));
    color = lerp(color, float3(1, 1, 1), saturate((edgeFade - 0.7) * 3.33));
    color *= uIntensity;
    
    // Trail fade
    float trailFade = 1.0 - coords.x;
    float alpha = flame * trailFade * uOpacity;
    
    return ApplyOverbright(color * baseColor.rgb, alpha * sampleColor.a);
}

// =============================================================================
// STYLE 2: SCROLL COSMIC - Nebula trail with star sparkles
// =============================================================================
float4 ScrollCosmicTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Scrolling UV for nebula movement
    float2 scrollUV = coords + float2(uTime * uScrollSpeed * 0.3, uTime * 0.1);
    
    float4 baseColor = tex2D(uImage0, scrollUV);
    
    // Secondary texture for nebula depth (inline, branch-free)
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    
    // Edge fade
    float edgeFade = QuadraticBump(coords.y);
    
    // Nebula clouds - blend procedural + texture
    float nebula = PseudoNoise(scrollUV * uNoiseScale + float2(uTime * 0.5, 0.0));
    nebula = nebula * (1.0 - 0.5 * uHasSecondaryTex) + noiseTex.r * 0.5 * uHasSecondaryTex;
    
    // Hue shift via lerp
    float hueShift = frac(coords.x * 0.3 + uTime * 0.15);
    float3 color = lerp(uColor, uSecondaryColor, hueShift * nebula);
    
    // Star sparkle effect - brighter with texture peaks
    float sparkleVal = nebula * lerp(1.0, noiseTex.g, uHasSecondaryTex);
    float sparkle = step(0.88, sparkleVal);
    color += float3(1, 1, 1) * sparkle * 2.0;
    
    // Blend nebula texture color for depth
    color += noiseTex.rgb * nebula * 0.3 * uHasSecondaryTex;
    color *= uIntensity * baseColor.rgb;
    
    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.9);
    float alpha = edgeFade * trailFade * uOpacity * (0.6 + nebula * 0.4);
    
    return ApplyOverbright(color, alpha * sampleColor.a);
}

// =============================================================================
// STYLE 3: SCROLL ENERGY - Pulsing energy beam with electric edges
// =============================================================================
float4 ScrollEnergyTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Fast scroll for energy flow
    float2 scrollUV = coords;
    scrollUV.x -= uTime * uScrollSpeed * 1.5;
    
    float4 baseColor = tex2D(uImage0, scrollUV);
    
    // Secondary texture for electric detail (inline, branch-free)
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    
    // Sharp edge with electric fringe
    float edge = abs(coords.y - 0.5) * 2.0;
    float coreIntensity = saturate(1.0 - edge * 2.5); // Tight bright core
    
    // Electric edge noise from texture (no procedural noise needed)
    float edgeNoise = noiseTex.r * uHasSecondaryTex;
    float electricEdge = saturate(1.0 - abs(edge - 0.35 + edgeNoise * 0.15) * 8.0);
    
    // Pulsing intensity
    float pulse = sin(uTime * 6.0 + coords.x * 4.0) * 0.15 + 0.85;
    
    // White core with colored edges
    float3 color = lerp(uColor, float3(1, 1, 1), coreIntensity * 0.8);
    color += uSecondaryColor * electricEdge * 0.5;
    color *= uIntensity * pulse * baseColor.rgb;
    
    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.85);
    float alpha = max(coreIntensity, electricEdge * 0.6) * trailFade * uOpacity;
    
    return ApplyOverbright(color, alpha * sampleColor.a);
}

// =============================================================================
// STYLE 4: SCROLL VOID - Dark void trail with eerie glow
// =============================================================================
float4 ScrollVoidTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Slow ominous scroll
    float2 scrollUV = coords;
    scrollUV.x -= uTime * uScrollSpeed * 0.4;
    scrollUV.y += sin(coords.x * 3.0 - uTime * 1.5) * 0.08;
    
    float4 baseColor = tex2D(uImage0, scrollUV);
    
    // Secondary texture for void distortion depth (inline, branch-free)
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    
    // Edge fade with void distortion
    float edgeFade = QuadraticBump(coords.y);
    
    // Void noise from texture (branch-free blend)
    float noise = noiseTex.r * 0.5 + baseColor.g * 0.5;
    
    // Inverted: dark center with glowing edges
    float voidCore = saturate(1.0 - edgeFade * 1.5);
    float edgeGlow = saturate(edgeFade * 2.0 - 0.6);
    
    // Eerie color mix - texture enriches the void
    float3 color = uColor * voidCore * 0.3; // Dark center
    color += uSecondaryColor * edgeGlow * (0.7 + noise * 0.5); // Glowing edges
    color += float3(0.1, 0.0, 0.15) * noise * noiseTex.g * uHasSecondaryTex; // Texture-modulated void tint
    color *= uIntensity * baseColor.rgb;
    
    // Trail fade
    float trailFade = saturate(1.0 - coords.x);
    float alpha = (voidCore * 0.4 + edgeGlow * 0.8) * trailFade * uOpacity;
    
    return ApplyOverbright(color, alpha * sampleColor.a);
}

// =============================================================================
// STYLE 5: SCROLL HOLY - Radiant holy trail with golden pulses
// =============================================================================
float4 ScrollHolyTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Gentle upward scroll
    float2 scrollUV = coords;
    scrollUV.x -= uTime * uScrollSpeed * 0.6;
    scrollUV.y -= uTime * 0.3; // Rising effect
    
    float4 baseColor = tex2D(uImage0, scrollUV);
    
    // Secondary texture for radiant texture detail (inline, branch-free)
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    
    // Soft edge fade
    float edgeFade = QuadraticBump(coords.y);
    
    // Golden pulse waves flowing along trail
    float pulse1 = sin(coords.x * 8.0 - uTime * 4.0) * 0.5 + 0.5;
    float pulse2 = sin(coords.x * 12.0 - uTime * 6.0) * 0.3 + 0.7;
    float pulseCombo = pulse1 * pulse2;
    
    // Radiant core with golden shimmer - texture adds subtle variation
    float3 color = lerp(uColor, float3(1.0, 0.95, 0.8), edgeFade * 0.6);
    color += uSecondaryColor * pulseCombo * 0.4;
    color *= lerp(1.0, noiseTex.rgb * 1.2, 0.25 * uHasSecondaryTex); // Subtle texture modulation
    
    // Bright bloom in center
    float bloom = saturate(edgeFade * 1.5 - 0.2);
    color += float3(1, 1, 1) * bloom * 0.3;
    color *= uIntensity * baseColor.rgb;
    
    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.95);
    float alpha = edgeFade * trailFade * uOpacity * (0.7 + pulseCombo * 0.3);
    
    return ApplyOverbright(color, alpha * sampleColor.a);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique ScrollFlameTechnique
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 ScrollFlameTrail();
    }
}

technique ScrollCosmicTechnique
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 ScrollCosmicTrail();
    }
}

technique ScrollEnergyTechnique
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 ScrollEnergyTrail();
    }
}

technique ScrollVoidTechnique
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 ScrollVoidTrail();
    }
}

technique ScrollHolyTechnique
{
    pass Pass0
    {
        PixelShader = compile ps_2_0 ScrollHolyTrail();
    }
}
