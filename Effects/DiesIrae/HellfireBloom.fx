// Hellfire Bloom Shader - Shared Dies Irae bloom effect
// Radial blood-red bloom with pulsing judgment intensity

sampler uImage0 : register(s0);

float uTime;
float3 uColor;          // Bloom color
float uOpacity;
float uIntensity;
float uRadius;           // Bloom radius multiplier
float uPulseSpeed;       // Pulsing rate

float4 HellfireBloomMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    // Soft radial falloff
    float bloom = pow(saturate(1.0 - dist / uRadius), 2.5);
    
    // Pulsing intensity
    float pulse = 1.0 + sin(uTime * uPulseSpeed) * 0.15;
    
    // Color with hot core
    float3 bloomColor = lerp(uColor, float3(1, 0.95, 0.85), pow(bloom, 3.0) * 0.4);
    
    float alpha = bloom * pulse * uOpacity * uIntensity;
    
    return float4(bloomColor, 1.0) * alpha * sampleColor;
}

// Cracked glow variant - for judgment marks
float4 JudgmentMarkGlow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseSample = tex2D(uImage0, coords);
    
    float pulse = 1.0 + sin(uTime * uPulseSpeed * 2.0) * 0.25;
    float3 glowColor = uColor * pulse * uIntensity;
    
    return float4(glowColor, baseSample.a * uOpacity) * sampleColor;
}

technique HellfireBloomTechnique
{
    pass HellfireBloom
    {
        PixelShader = compile ps_3_0 HellfireBloomMain();
    }
}

technique JudgmentMarkTechnique
{
    pass JudgmentMark
    {
        PixelShader = compile ps_3_0 JudgmentMarkGlow();
    }
}
