// =============================================================================
// MagnumOpus Trail Shader - Calamity-style primitive trail rendering
// =============================================================================
// This shader provides smooth trails with bloom falloff and gradient coloring.
// Apply using SpriteBatch.Begin with custom Effect parameter.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Optional noise texture
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// Utility functions
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float InverseLerp(float from, float to, float x)
{
    return saturate((x - from) / (to - from));
}

float Convert01To010(float x)
{
    return x < 0.5 ? x * 2.0 : 2.0 - x * 2.0;
}

// Main trail pixel shader
float4 TrailShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Get base texture color
    float4 baseColor = tex2D(uImage0, coords);
    
    // Vertical interpolant for trail width (0 at edges, 1 at center)
    float verticalInterpolant = QuadraticBump(coords.y);
    
    // Horizontal interpolant for trail length (0 at start, 1 at end)
    float horizontalProgress = coords.x;
    
    // Calculate edge fadeout using InverseLerp for smooth edges
    float edgeFade = InverseLerp(0.0, 0.15, verticalInterpolant);
    edgeFade *= InverseLerp(0.95, 0.5, horizontalProgress);
    
    // Blend between primary and secondary color based on progress
    float3 gradientColor = lerp(uColor, uSecondaryColor, horizontalProgress);
    
    // Add subtle color pulsing
    float pulse = sin(uTime * 3.0 + coords.x * 6.28318) * 0.15 + 0.85;
    gradientColor *= pulse;
    
    // Apply intensity and opacity
    float finalOpacity = edgeFade * uOpacity * uIntensity * sampleColor.a;
    
    return float4(gradientColor * baseColor.rgb, finalOpacity);
}

// Bloom pass - creates soft outer glow
float4 TrailBloomShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Wider falloff for bloom effect
    float verticalInterpolant = QuadraticBump(coords.y);
    float bloomFade = pow(verticalInterpolant, 0.5); // Softer falloff
    
    float3 bloomColor = lerp(uColor, uSecondaryColor, coords.x) * 0.5;
    float bloomOpacity = bloomFade * uOpacity * 0.4 * sampleColor.a;
    
    return float4(bloomColor, bloomOpacity);
}

technique Technique1
{
    pass TrailPass
    {
        PixelShader = compile ps_2_0 TrailShader();
    }
    pass BloomPass
    {
        PixelShader = compile ps_2_0 TrailBloomShader();
    }
}
