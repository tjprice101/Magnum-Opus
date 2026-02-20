// =============================================================================
// AdditiveMetaballEdgeShader.fx - ps_2_0 Compatible (FNA/MojoShader)
// =============================================================================
// Downported from SM4.0 source (ShaderSource/AdditiveMetaballEdgeShader.fx).
// Additive blending shader for fire/plasma metaball effects.
//
// Samples the center pixel plus 4 cardinal neighbors, averages them,
// finds the darkest channel, and brightens toward white based on intensity.
//
// Only 5 texture samples — well within ps_2_0 instruction limits.
// =============================================================================

// Metaball render target (auto-bound by SpriteBatch to slot 0)
sampler uImage0 : register(s0);

// Overlay texture (set via GraphicsDevice.Textures[1])
sampler uImage1 : register(s1);

// --- Standard tModLoader MiscShaderData parameters ---
// These are set automatically by MiscShaderData.Apply() even if not all are used.
float4 uColor;
float4 uSecondaryColor;
float  uOpacity;
float  uSaturation;
float  uTime;
float2 uTargetPosition;
float2 uDirection;

// --- Custom parameters ---
float4 screenArea;              // Screen area rectangle
float2 layerOffset;             // Parallax layer offset
float2 singleFrameScreenOffset; // Per-frame camera offset compensation


float4 AdditiveMetaballPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Apply offsets (parallax + camera movement)
    coords += layerOffset + singleFrameScreenOffset;

    float2 pixelOffset = float2(3.0 / screenArea.z, 3.0 / screenArea.w);

    // Sample center + 4 cardinal neighbors
    float4 center = tex2D(uImage0, coords);
    float4 right  = tex2D(uImage0, coords + float2( pixelOffset.x, 0));
    float4 left   = tex2D(uImage0, coords + float2(-pixelOffset.x, 0));
    float4 up     = tex2D(uImage0, coords + float2(0, -pixelOffset.y));
    float4 down   = tex2D(uImage0, coords + float2(0,  pixelOffset.y));

    // Average samples (divisor 4.7 from original for softer blending)
    float4 color = (center + right + left + up + down) / 4.7;

    // Find darkest channel — indicates how far from white
    float lowestChannel = min(color.r, min(color.g, color.b));

    // Push toward white based on how dark the darkest channel is
    float whiten = pow(lowestChannel, 0.5);
    color.rgb = lerp(color.rgb, float3(1, 1, 1), whiten);

    // Preserve alpha from average
    color.a *= color.a;

    return color;
}


technique DefaultTechnique
{
    pass ParticlePass
    {
        PixelShader = compile ps_2_0 AdditiveMetaballPS();
    }
}
