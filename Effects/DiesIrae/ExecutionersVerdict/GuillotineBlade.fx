// Executioner's Verdict - GuillotineBlade Shader
// Heavy downward slash with screen-distortion weight and darkness edge

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise

float uTime;
float3 uColor;          // DarkBlood
float3 uSecondaryColor; // InfernalRed
float uOpacity;
float uIntensity;
float uScrollSpeed;
float uDistortionAmt;
float uOverbrightMult;
float uExecuteThreshold; // Glow intensity scales with enemy HP%

float4 GuillotineSlashMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Heavy, dark slash - less fire, more impact weight
    float2 noiseUV = float2(coords.x * 2.5 - uTime * uScrollSpeed * 0.2, coords.y * 1.8);
    float noise = tex2D(uImage1, noiseUV).r;
    
    float2 distortedUV = coords + (noise - 0.5) * uDistortionAmt * float2(0.5, 1.0);
    float4 baseSample = tex2D(uImage0, distortedUV);
    
    // Hard edge fade for guillotine feel
    float edgeFade = smoothstep(0.0, 0.08, coords.y) * smoothstep(1.0, 0.92, coords.y);
    float tipFade = smoothstep(0.0, 0.05, coords.x) * smoothstep(1.0, 0.6, coords.x);
    
    // Dark blade body with blood-red edge glow
    float centerDark = pow(saturate(1.0 - abs(coords.y - 0.5) * 2.5), 1.5);
    float3 bladeColor = lerp(uColor, float3(0.02, 0.0, 0.0), centerDark * 0.7);
    
    // Blood-red edge lines
    float edgeGlow = pow(saturate(abs(coords.y - 0.5) * 2.5 - 0.3), 2.0);
    bladeColor = lerp(bladeColor, uSecondaryColor, edgeGlow * 0.8);
    
    // Execute enhancement: brighter when execute threshold is higher
    float executePower = saturate(uExecuteThreshold);
    bladeColor += float3(0.3, 0.05, 0.0) * executePower * centerDark;
    
    float3 finalColor = bladeColor * uOverbrightMult * baseSample.r;
    float alpha = baseSample.a * edgeFade * tipFade * uOpacity * uIntensity;
    
    return float4(finalColor, 1.0) * alpha * sampleColor;
}

// Execution Mark - skull sigil shader for low-HP enemies
float4 ExecutionMarkMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseSample = tex2D(uImage0, coords);
    
    float pulse = 1.0 + sin(uTime * 4.0) * 0.3 * uExecuteThreshold;
    float3 markColor = lerp(uColor, float3(1, 0.2, 0.0), uExecuteThreshold) * pulse;
    
    float alpha = baseSample.a * uOpacity * uIntensity * (0.5 + uExecuteThreshold * 0.5);
    
    return float4(markColor * uOverbrightMult, alpha) * sampleColor;
}

technique GuillotineSlashTechnique
{
    pass GuillotineSlash
    {
        PixelShader = compile ps_2_0 GuillotineSlashMain();
    }
}

technique ExecutionMarkTechnique
{
    pass ExecutionMark
    {
        PixelShader = compile ps_2_0 ExecutionMarkMain();
    }
}
