// ============================================================================
// RADIAL SCROLL SHADER - MagnumOpus VFX System
// ============================================================================
// Polar-coordinate noise scrolling with shape masking.
// Based on VFX+ NewRadialScroll approach (polar UV conversion for fluid flow).
// See: https://nekotoarts.github.io/blog/Polar-Water-Breakdown
//
// Usage:
//   1. Set causticTexture and distortTexture via shader parameters
//   2. Pass this Effect to sb.Begin() as the effect parameter
//   3. Draw the shape texture (gear, orb, etc.)  Eits alpha masks the output
//
// The drawn texture's alpha channel acts as the shape mask.
// Noise textures are set via shader parameters, NOT device.Textures[].
// ============================================================================

const float TAU = 6.283185;

// SpriteBatch-drawn texture (shape mask  Egear, orb, circle, etc.)
// SpriteBatch binds this automatically to register s0.
sampler2D uImage0 : register(s0);

// --- Parameters ---
float uTime;
float flowSpeed;
float distortStrength;
float colorIntensity;
float vignetteSize;
float vignetteBlend;
float3 uColor;
float3 uSecondaryColor;

// --- Noise texture (the visual pattern to scroll through the shape) ---
texture causticTexture;
sampler2D causticTex = sampler_state
{
    texture = <causticTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// --- Distortion noise (warps UVs for organic, fluid movement) ---
texture distortTexture;
sampler2D distortTex = sampler_state
{
    texture = <distortTexture>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// ============================================================================
// UTILITY FUNCTIONS
// ============================================================================

// GLSL-style mod (always positive, unlike HLSL fmod which preserves sign)
float2 glslmod(float2 ab, float y)
{
    float xA = ab.x - y * floor(ab.x / y);
    float yB = ab.y - y * floor(ab.y / y);
    return float2(xA, yB);
}

// Convert Cartesian UVs to polar coordinates (radius, angle)
// This is the key to creating the radial/fluid flow effect.
float2 polar_coordinates(float2 uv, float2 center, float zoom, float repeat)
{
    float2 dir = uv - center;
    float radius = length(dir) * 2.0;
    float angle = atan2(dir.y, dir.x) / TAU;
    return glslmod(float2(radius * zoom, angle * repeat), 1.0);
}

// ============================================================================
// TECHNIQUE 1: POLAR SCROLL (single layer, shape-masked)
// ============================================================================
// Scrolls noise radially through the drawn texture's shape.
// The drawn texture's alpha channel acts as the shape mask.
// Output: colored noise within shape, transparent outside.
float4 PSPolarScroll(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Read shape texture alpha (gear, orb, etc.)
    float4 baseCol = tex2D(uImage0, uv);

    // Distort UVs with noise for organic motion
    float distort = tex2D(distortTex, uv + uTime * 0.1).r;
    float2 distortedUV = uv + distort * distortStrength - distortStrength / 2.0;

    // Convert to polar coordinates for radial flow
    float2 polarUV = polar_coordinates(distortedUV, float2(0.5, 0.5), 1.0, 1.0);
    polarUV.x -= uTime * flowSpeed;

    // Sample noise at polar coordinates
    float caus = tex2D(causticTex, polarUV).r;

    // Vignette (circular fade from center)
    float cd = distance(uv, float2(0.5, 0.5));
    float vign = 1.0 - smoothstep(vignetteSize, vignetteSize + vignetteBlend, cd);

    // Map noise intensity through color gradient:
    // dark ↁEuColor ↁEuSecondaryColor ↁEwhite-hot
    float intensity = caus * vign;
    float3 finalColor = lerp(uColor * 0.3, uColor, saturate(intensity * 2.0));
    finalColor = lerp(finalColor, uSecondaryColor, saturate(intensity * 2.0 - 0.5));
    finalColor = lerp(finalColor, float3(1.0, 1.0, 0.95), saturate(intensity * 2.0 - 1.2));

    // Shape mask: output alpha = shape texture alpha only (VFX+ pattern)
    return float4(finalColor * colorIntensity, baseCol.a);
}

// ============================================================================
// TECHNIQUE 2: POLAR MULTI-LAYER (3 counter-scrolling layers, shape-masked)
// ============================================================================
// Three noise layers at different scroll speeds for volumetric depth.
// Creates richer, more complex fluid motion.
float4 PSPolarMultiLayer(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Read shape texture alpha
    float4 baseCol = tex2D(uImage0, uv);

    // Distort UVs
    float distort = tex2D(distortTex, uv + uTime * 0.1).r;
    float2 distortedUV = uv + distort * distortStrength - distortStrength / 2.0;

    // Convert to polar space
    float2 polarUV = polar_coordinates(distortedUV, float2(0.5, 0.5), 1.0, 1.0);

    // Layer 1: Slow background scroll
    float2 uv1 = polarUV;
    uv1.x -= uTime * flowSpeed * 0.4;
    float layer1 = tex2D(causticTex, uv1).r * 0.4;

    // Layer 2: Counter-scrolling midground
    float2 uv2 = polarUV + float2(0.33, 0.17);
    uv2.x += uTime * flowSpeed * 0.6;
    float layer2 = tex2D(causticTex, uv2).r * 0.5;

    // Layer 3: Fast foreground scroll
    float2 uv3 = polarUV + float2(0.67, 0.41);
    uv3.x -= uTime * flowSpeed;
    float layer3 = tex2D(causticTex, uv3).r * 0.6;

    float caus = saturate(layer1 + layer2 + layer3);

    // Vignette
    float cd = distance(uv, float2(0.5, 0.5));
    float vign = 1.0 - smoothstep(vignetteSize, vignetteSize + vignetteBlend, cd);

    // Color gradient mapping
    float intensity = caus * vign;
    float3 finalColor = lerp(uColor * 0.3, uColor, saturate(intensity * 2.0));
    finalColor = lerp(finalColor, uSecondaryColor, saturate(intensity * 2.0 - 0.5));
    finalColor = lerp(finalColor, float3(1.0, 1.0, 0.95), saturate(intensity * 2.0 - 1.2));

    return float4(finalColor * colorIntensity, baseCol.a);
}

// ============================================================================
// TECHNIQUE 3: LINEAR SCROLL (non-polar, simple scrolling, shape-masked)
// ============================================================================
// Simpler version without polar conversion  Ejust scrolls noise linearly.
// Good for rectangular shapes or when polar distortion isn't wanted.
float4 PSLinearScroll(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 baseCol = tex2D(uImage0, uv);

    float distort = tex2D(distortTex, uv + uTime * 0.1).r;
    float2 scrollUV = uv + float2(uTime * flowSpeed, uTime * flowSpeed * 0.3);
    scrollUV += distort * distortStrength - distortStrength / 2.0;

    float caus = tex2D(causticTex, scrollUV).r;

    float cd = distance(uv, float2(0.5, 0.5));
    float vign = 1.0 - smoothstep(vignetteSize, vignetteSize + vignetteBlend, cd);

    float intensity = caus * vign;
    float3 finalColor = lerp(uColor * 0.3, uColor, saturate(intensity * 2.0));
    finalColor = lerp(finalColor, uSecondaryColor, saturate(intensity * 2.0 - 0.5));
    finalColor = lerp(finalColor, float3(1.0, 1.0, 0.95), saturate(intensity * 2.0 - 1.2));

    return float4(finalColor * colorIntensity, baseCol.a);
}

// ============================================================================
// TECHNIQUES
// ============================================================================
technique PolarScroll
{
    pass P0
    {
        PixelShader = compile ps_3_0 PSPolarScroll();
    }
}

technique PolarMultiLayer
{
    pass P0
    {
        PixelShader = compile ps_3_0 PSPolarMultiLayer();
    }
}

technique LinearScroll
{
    pass P0
    {
        PixelShader = compile ps_3_0 PSLinearScroll();
    }
}
