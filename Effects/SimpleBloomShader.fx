// =============================================================================
// MagnumOpus Bloom Shader - PS 2.0 Compatible (Minimal Instructions)
// =============================================================================
// Ultra-simple bloom effects under 64 instruction limit
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// =============================================================================
// UNIFIED BLOOM - Works for all themes
// =============================================================================
float4 UnifiedBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Simple radial falloff from center
    float dist = length(coords - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist);
    
    // Gentle pulse
    float pulse = sin(uTime * 2.0) * 0.1 + 1.0;
    
    // Gradient color
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 0.5);
    glowColor *= baseColor.rgb * uIntensity * glow * pulse;
    
    return float4(glowColor, glow * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// ETHEREAL - Soft dreamy bloom
// =============================================================================
float4 EtherealBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float dist = length(coords - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist);
    glow = glow * glow; // Softer falloff
    float breathe = sin(uTime * 1.5) * 0.1 + 1.0;
    float3 color = uColor * baseColor.rgb * uIntensity * glow * breathe;
    return float4(color, glow * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// INFERNAL - Harsh fire-like
// =============================================================================
float4 InfernalBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    float dist = length(coords - float2(0.5, 0.5)) * 2.0;
    float glow = saturate(1.0 - dist * 1.5);
    float flicker = sin(uTime * 10.0) * 0.15 + 1.0;
    float3 color = uColor * baseColor.rgb * uIntensity * glow * flicker;
    return float4(color, glow * uOpacity * sampleColor.a * baseColor.a);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique DefaultTechnique
{
    pass DefaultPass
    {
        PixelShader = compile ps_2_0 UnifiedBloom();
    }
}

technique EtherealTechnique
{
    pass EtherealPass
    {
        PixelShader = compile ps_2_0 EtherealBloom();
    }
}

technique InfernalTechnique
{
    pass InfernalPass
    {
        PixelShader = compile ps_2_0 InfernalBloom();
    }
}
