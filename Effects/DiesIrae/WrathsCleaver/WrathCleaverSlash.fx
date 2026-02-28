// Wrath's Cleaver Slash Shader - Dies Irae theme
// Blood-red to hellfire-gold slash arc with ember distortion

sampler uImage0 : register(s0); // Trail UV texture
sampler uImage1 : register(s1); // Noise texture (NoiseSmoke)

float uTime;
float3 uColor;          // Primary color (BloodRed)
float3 uSecondaryColor; // Secondary color (HellfireGold)
float uOpacity;
float uIntensity;
float uScrollSpeed;
float uDistortionAmt;
float uOverbrightMult;
float2 uImageSize0;

// Creates a blazing slash effect with flowing ember noise
float4 WrathSlashMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Scroll UV with time for flowing fire effect
    float2 scrolledUV = coords;
    scrolledUV.x += uTime * uScrollSpeed * 0.3;
    
    // Sample noise for distortion
    float2 noiseUV = coords * 2.0 + float2(uTime * 0.4, uTime * 0.2);
    float4 noise = tex2D(uImage1, noiseUV);
    
    // Apply distortion from noise
    float2 distortedUV = coords + (noise.rg - 0.5) * uDistortionAmt;
    distortedUV.x += uTime * uScrollSpeed * 0.15;
    
    // Sample base trail texture
    float4 baseSample = tex2D(uImage0, distortedUV);
    
    // Create fire gradient along trail width (coords.y = 0 at edge, 1 at center)
    float edgeFade = smoothstep(0.0, 0.15, coords.y) * smoothstep(1.0, 0.85, coords.y);
    float tipFade = smoothstep(0.0, 0.1, coords.x) * smoothstep(1.0, 0.7, coords.x);
    
    // Blend primary and secondary colors based on position + noise
    float colorMix = saturate(coords.y + noise.r * 0.4 - 0.2);
    float3 fireColor = lerp(uColor, uSecondaryColor, colorMix);
    
    // Add white-hot core
    float coreIntensity = pow(saturate(1.0 - abs(coords.y - 0.5) * 2.0), 2.0);
    float3 coreColor = lerp(fireColor, float3(1, 0.95, 0.85), coreIntensity * 0.6);
    
    // Overbright for bloom-like glow
    float3 finalColor = coreColor * uOverbrightMult * baseSample.r;
    
    float alpha = baseSample.a * edgeFade * tipFade * uOpacity * uIntensity;
    
    return float4(finalColor, 1.0) * alpha * sampleColor;
}

// Glow pass - softer, wider version for bloom layer
float4 WrathSlashGlow(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseUV = coords * 1.5 + float2(uTime * 0.3, uTime * 0.15);
    float4 noise = tex2D(uImage1, noiseUV);
    
    float2 distortedUV = coords + (noise.rg - 0.5) * uDistortionAmt * 0.5;
    float4 baseSample = tex2D(uImage0, distortedUV);
    
    float edgeFade = smoothstep(0.0, 0.3, coords.y) * smoothstep(1.0, 0.7, coords.y);
    float tipFade = smoothstep(0.0, 0.15, coords.x) * smoothstep(1.0, 0.5, coords.x);
    
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.y) * uOverbrightMult * 0.5;
    float alpha = baseSample.a * edgeFade * tipFade * uOpacity * 0.5;
    
    return float4(glowColor, 1.0) * alpha * sampleColor;
}

technique WrathSlashTechnique
{
    pass WrathSlashMain
    {
        PixelShader = compile ps_2_0 WrathSlashMain();
    }
}

technique WrathSlashGlowTechnique
{
    pass WrathSlashGlow
    {
        PixelShader = compile ps_2_0 WrathSlashGlow();
    }
}
