// Judgment Aura Shader - Radial expanding ring effect
// For Dies Irae weapon auras and impact rings

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise texture

float uTime;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uIntensity;
float uRadius;
float uRingWidth;
float uScrollSpeed;

float4 JudgmentAuraMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    // Create expanding ring
    float ringDist = abs(dist - uRadius);
    float ring = smoothstep(uRingWidth, 0.0, ringDist);
    
    // Noise distortion for organic feel
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float2 noiseUV = float2(angle / 6.28318 + uTime * uScrollSpeed * 0.1, dist);
    float noise = tex2D(uImage1, noiseUV).r;
    
    ring *= 0.7 + noise * 0.3;
    
    // Color gradient: inner ring is hot, outer is cooler
    float colorMix = saturate((dist - uRadius + uRingWidth) / (uRingWidth * 2.0));
    float3 ringColor = lerp(uSecondaryColor, uColor, colorMix);
    
    // Add bright core line
    float coreLine = exp(-ringDist * ringDist * 200.0);
    ringColor = lerp(ringColor, float3(1, 0.95, 0.9), coreLine * 0.5);
    
    float alpha = ring * uOpacity * uIntensity;
    
    return float4(ringColor, 1.0) * alpha * sampleColor;
}

// Inner glow fill for sustained auras
float4 JudgmentAuraFill(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    float fill = pow(saturate(1.0 - dist / uRadius), 3.0);
    
    float2 noiseUV = coords * 2.0 + float2(uTime * 0.2, uTime * 0.15);
    float noise = tex2D(uImage1, noiseUV).r;
    fill *= 0.6 + noise * 0.4;
    
    float3 fillColor = lerp(uColor * 0.3, uSecondaryColor, fill);
    float alpha = fill * uOpacity * uIntensity * 0.3;
    
    return float4(fillColor, 1.0) * alpha * sampleColor;
}

technique JudgmentAuraTechnique
{
    pass JudgmentAura
    {
        PixelShader = compile ps_3_0 JudgmentAuraMain();
    }
}

technique JudgmentAuraFillTechnique
{
    pass JudgmentAuraFill
    {
        PixelShader = compile ps_3_0 JudgmentAuraFill();
    }
}
