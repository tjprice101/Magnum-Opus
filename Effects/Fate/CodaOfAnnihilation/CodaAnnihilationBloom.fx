// =============================================================================
// Coda of Annihilation — Annihilation Bloom Shader
// =============================================================================
// Ultimate bloom/flash for critical moments: spawn flashes, finisher explosions,
// convergence events. Expanding radial glow with chromatic shift and cosmic pulse.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float4 AnnihilationBloomMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);
    
    // Soft radial glow — Gaussian-like falloff
    float glow = exp(-dist * dist * 8.0);
    
    // Cosmic chromatic ring
    float ringDist = abs(dist - 0.25 - uTime * 0.2);
    float ring = exp(-ringDist * ringDist * 60.0);
    
    // Pulse breathing
    float pulse = sin(uTime * 6.0) * 0.1 + 0.9;
    
    // Color shift: center is bright white/gold, edges are weapon-colored
    float3 color = lerp(uColor, uSecondaryColor, dist * 2.0);
    color += float3(1.0, 0.95, 0.9) * glow * 0.5; // White-hot center
    color *= pulse;
    
    float alpha = (glow * 0.8 + ring * 0.5) * uOpacity * uIntensity;
    alpha *= saturate(1.2 - uTime * 0.8);
    
    return float4(color * alpha, alpha);
}

technique AnnihilationBloomMain
{
    pass AnnihilationBloomMainPass
    {
        PixelShader = compile ps_2_0 AnnihilationBloomMainPS();
    }
}
