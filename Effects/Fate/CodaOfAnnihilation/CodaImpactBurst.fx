// =============================================================================
// Coda of Annihilation — Impact Burst Shader
// =============================================================================
// Radial impact explosion with shockwave rings and cosmic energy dispersion.
// Used when flying swords or held swing strikes an enemy.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float4 ImpactBurstMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);
    
    // Shockwave ring expanding outward
    float ringRadius = uTime * 0.6;
    float ringWidth = 0.08;
    float ring = 1.0 - saturate(abs(dist - ringRadius) / ringWidth);
    ring *= ring;
    
    // Inner flash — bright core
    float innerFlash = saturate(1.0 - dist * 4.0) * (1.0 - uTime);
    innerFlash *= innerFlash;
    
    // Energy dispersion — simplified radial pattern
    float rays = sin(delta.x * 14.0 + uTime * 3.0) * 0.5 + 0.5;
    rays *= saturate(1.0 - dist * 2.0);
    
    // Composite
    float intensity = (ring * 0.8 + innerFlash * 1.2 + rays * 0.3) * uIntensity;
    
    float3 color = lerp(uColor, uSecondaryColor, dist) + innerFlash * 0.5;
    
    float alpha = intensity * uOpacity * (1.0 - saturate(uTime));
    
    return float4(color * alpha, alpha);
}

technique ImpactBurstMain
{
    pass ImpactBurstMainPass
    {
        PixelShader = compile ps_2_0 ImpactBurstMainPS();
    }
}
