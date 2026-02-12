// =============================================================================
// MagnumOpus Simple Trail Shader - PS 2.0 Compatible
// =============================================================================
// Simplified trail effects within PS 2.0 limits
// 5 passes: Flame, Ice, Lightning, Nature, Cosmic
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
float uProgress; // 0-1 along trail length

// --- New uniforms for Calamity-tier rendering ---
float uOverbrightMult;   // Overbright multiplier (1.0 = normal, 2-7 = HDR glow)
float uGlowThreshold;    // Brightness threshold for conditional glow (0-1, default 0.5)
float uGlowIntensity;    // Extra multiplier when above threshold (default 1.5)
float uSecondaryTexScale; // UV scale for secondary texture (default 1.0)
float uSecondaryTexScroll; // Scroll speed for secondary texture (default 0.5)
float uHasSecondaryTex;  // 1.0 if secondary texture is bound, 0.0 if not

// =============================================================================
// UTILITY: Apply overbright to final color (minimal instruction cost)
// =============================================================================
float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// STYLE 1: FLAME - Fire trail with noise-driven distortion
// =============================================================================
float4 FlameTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Width falloff with flicker
    float edge = abs(coords.y - 0.5) * 2.0;
    float flicker = sin(coords.x * 20.0 + uTime * 10.0) * 0.1 + 0.9;
    float alpha = saturate(1.0 - edge * 1.5) * flicker;
    
    // Orange to red gradient, modulated by noise texture
    float3 flameColor = lerp(uColor, uSecondaryColor, edge);
    flameColor *= baseColor.rgb * uIntensity;
    
    // Inline secondary texture sampling (branch-free)
    float2 scrollUV = coords * uSecondaryTexScale;
    scrollUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, scrollUV);
    flameColor *= lerp(1.0, noiseTex.rgb * 1.5, uHasSecondaryTex * 0.6);
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    alpha *= uOpacity * sampleColor.a * baseColor.a;
    
    return ApplyOverbright(flameColor, alpha);
}

// =============================================================================
// STYLE 2: ICE - Crystalline trail with texture detail
// =============================================================================
float4 IceTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Inline secondary texture sampling
    float2 scrollUV = coords * uSecondaryTexScale;
    scrollUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, scrollUV);
    
    // Sharp crystalline edges enhanced by noise
    float edge = abs(coords.y - 0.5) * 2.0;
    float crystal = step(0.4, sin(coords.x * 30.0 + edge * 10.0) * 0.5 + 0.5);
    float noiseDetail = lerp(1.0, noiseTex.r, uHasSecondaryTex * 0.4);
    float alpha = saturate(1.0 - edge * 1.3) * (0.7 + crystal * 0.3) * noiseDetail;
    
    // Ice blue to white
    float3 iceColor = lerp(uColor, float3(1, 1, 1), edge * 0.5);
    iceColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    alpha *= uOpacity * sampleColor.a * baseColor.a;
    
    return ApplyOverbright(iceColor, alpha);
}

// =============================================================================
// STYLE 3: LIGHTNING - Electric trail with noise jitter
// =============================================================================
float4 LightningTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Inline secondary texture sampling
    float2 scrollUV = coords * uSecondaryTexScale;
    scrollUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, scrollUV);
    
    // Jagged electric pattern enhanced by noise texture
    float edge = abs(coords.y - 0.5) * 2.0;
    float noiseJag = (noiseTex.r - 0.5) * 0.3 * uHasSecondaryTex;
    float jag = sin(coords.x * 50.0 + uTime * 15.0) * 0.15 + noiseJag;
    float bolt = saturate(1.0 - abs(edge - 0.3 + jag) * 5.0);
    
    // Electric flicker
    float flicker = sin(uTime * 20.0) * 0.3 + 0.7;
    float alpha = bolt * flicker;
    
    // White core, colored edge
    float3 lightningColor = lerp(float3(1, 1, 1), uColor, edge);
    lightningColor *= baseColor.rgb * uIntensity;
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x * 0.8);
    alpha *= uOpacity * sampleColor.a * baseColor.a;
    
    return ApplyOverbright(lightningColor, alpha);
}

// =============================================================================
// STYLE 4: NATURE - Organic vine trail with texture flow
// =============================================================================
float4 NatureTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Inline secondary texture sampling
    float2 scrollUV = coords * uSecondaryTexScale;
    scrollUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, scrollUV);
    
    // Flowing organic shape with noise displacement
    float edge = abs(coords.y - 0.5) * 2.0;
    float noiseWave = (noiseTex.r - 0.5) * 0.15 * uHasSecondaryTex;
    float wave = sin(coords.x * 8.0 - uTime * 2.0) * 0.1 + noiseWave;
    float alpha = saturate(1.0 - (edge + wave) * 1.2);
    
    // Green gradient with noise texture color blending
    float3 vineColor = lerp(uColor, uSecondaryColor, coords.x);
    vineColor *= baseColor.rgb * uIntensity;
    vineColor *= lerp(1.0, noiseTex.rgb * 1.3, uHasSecondaryTex * 0.3);
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    alpha *= uOpacity * sampleColor.a * baseColor.a;
    
    return ApplyOverbright(vineColor, alpha);
}

// =============================================================================
// STYLE 5: COSMIC - Starfield trail with nebula texture
// =============================================================================
float4 CosmicTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Inline secondary texture sampling
    float2 scrollUV = coords * uSecondaryTexScale;
    scrollUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, scrollUV);
    
    // Nebula-like gradient
    float edge = abs(coords.y - 0.5) * 2.0;
    float alpha = saturate(1.0 - edge * 1.2);
    
    // Color shift with noise texture modulation
    float shift = coords.x + uTime * 0.2;
    float3 cosmicColor;
    cosmicColor.r = sin(shift * 3.0) * 0.3 + 0.7;
    cosmicColor.g = sin(shift * 3.0 + 2.0) * 0.3 + 0.7;
    cosmicColor.b = sin(shift * 3.0 + 4.0) * 0.3 + 0.7;
    
    cosmicColor *= uColor * baseColor.rgb * uIntensity;
    // Blend in nebula texture for richness
    cosmicColor *= lerp(1.0, noiseTex.rgb * 1.8, uHasSecondaryTex * 0.5);
    
    // Fade toward end
    alpha *= saturate(1.0 - coords.x);
    alpha *= uOpacity * sampleColor.a * baseColor.a;
    
    return ApplyOverbright(cosmicColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FlameTechnique
{
    pass FlamePass
    {
        PixelShader = compile ps_2_0 FlameTrail();
    }
}

technique IceTechnique
{
    pass IcePass
    {
        PixelShader = compile ps_2_0 IceTrail();
    }
}

technique LightningTechnique
{
    pass LightningPass
    {
        PixelShader = compile ps_2_0 LightningTrail();
    }
}

technique NatureTechnique
{
    pass NaturePass
    {
        PixelShader = compile ps_2_0 NatureTrail();
    }
}

technique CosmicTechnique
{
    pass CosmicPass
    {
        PixelShader = compile ps_2_0 CosmicTrail();
    }
}
