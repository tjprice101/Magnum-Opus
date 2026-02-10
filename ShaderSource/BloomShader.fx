// =============================================================================
// MagnumOpus Bloom Shader - Multi-layer additive bloom effects
// =============================================================================
// Creates vibrant neon glow effects using the FargosSoulsDLC bloom pattern.
// Key features:
// - Multi-layer bloom stacking
// - Pulsing animation support
// - Color gradient blending
// =============================================================================

sampler uImage0 : register(s0);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uPulseSpeed;
float uPulseIntensity;

// Utility functions
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float SineBump(float x)
{
    return sin(x * 3.14159);
}

// Standard bloom shader - applies soft glow with radial falloff
float4 BloomShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Radial distance from center for circular falloff
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center) * 2.0;
    
    // Smooth radial falloff
    float radialFade = saturate(1.0 - dist);
    radialFade = pow(radialFade, 1.5); // Softer edges
    
    // Pulsing animation
    float pulse = 1.0;
    if (uPulseSpeed > 0)
    {
        pulse = 1.0 + sin(uTime * uPulseSpeed) * uPulseIntensity;
    }
    
    // Apply color and intensity
    float3 finalColor = uColor * baseColor.rgb * uIntensity * pulse;
    float finalOpacity = radialFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// Gradient bloom - blends from center color to edge color
float4 GradientBloomShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center) * 2.0;
    float radialFade = saturate(1.0 - dist);
    radialFade = pow(radialFade, 1.5);
    
    // Gradient from center (uColor) to edge (uSecondaryColor)
    float3 gradientColor = lerp(uSecondaryColor, uColor, radialFade);
    
    float pulse = 1.0 + sin(uTime * uPulseSpeed) * uPulseIntensity;
    
    float3 finalColor = gradientColor * baseColor.rgb * uIntensity * pulse;
    float finalOpacity = radialFade * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// Sharp flare shader - creates 4-point star effect
float4 FlareShader(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float2 offset = coords - center;
    
    // Calculate distance and angle from center
    float dist = length(offset) * 2.0;
    float angle = atan2(offset.y, offset.x);
    
    // 4-point star pattern
    float starPattern = abs(cos(angle * 2.0));
    starPattern = pow(starPattern, 4.0);
    
    // Combine radial fade with star pattern
    float radialFade = saturate(1.0 - dist);
    float fade = radialFade * (0.5 + starPattern * 0.5);
    
    float pulse = 1.0 + sin(uTime * 6.0) * 0.1;
    
    float3 finalColor = uColor * baseColor.rgb * uIntensity * pulse;
    float finalOpacity = fade * uOpacity * sampleColor.a;
    
    return float4(finalColor, finalOpacity);
}

technique Technique1
{
    pass BloomPass
    {
        PixelShader = compile ps_2_0 BloomShader();
    }
    pass GradientBloomPass
    {
        PixelShader = compile ps_2_0 GradientBloomShader();
    }
    pass FlarePass
    {
        PixelShader = compile ps_2_0 FlareShader();
    }
}
