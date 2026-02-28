// Eclipse Orb Shader - Eclipse of Wrath cursor-tracking orb
// Dark sun corona with blood-red plasma tendrils

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise

float uTime;
float3 uColor;          // Core color (DarkBlood)
float3 uSecondaryColor; // Corona color (InfernalRed)
float uOpacity;
float uIntensity;
float uRadius;
float uCoronaSize;

float4 EclipseOrbMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    // Dark inner core
    float core = smoothstep(uRadius * 0.6, uRadius * 0.3, dist);
    
    // Corona tendrils using noise
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float2 noiseUV1 = float2(angle / 6.28318 + uTime * 0.15, dist * 2.0 - uTime * 0.3);
    float2 noiseUV2 = float2(angle / 6.28318 - uTime * 0.1, dist * 1.5 + uTime * 0.2);
    float noise1 = tex2D(uImage1, noiseUV1).r;
    float noise2 = tex2D(uImage1, noiseUV2).r;
    float coronaNoise = (noise1 * 0.6 + noise2 * 0.4);
    
    // Corona extends beyond core radius with noise-driven tendrils
    float coronaDist = dist - uRadius * 0.5;
    float maxExtend = uCoronaSize * (0.5 + coronaNoise * 0.5);
    float corona = smoothstep(maxExtend, 0.0, coronaDist) * (1.0 - core);
    
    // Dark core color (nearly black center, bleeding to blood red)
    float3 coreColor = lerp(float3(0.05, 0.0, 0.0), uColor, smoothstep(0.0, uRadius * 0.5, dist));
    
    // Corona color
    float3 coronaColor = lerp(uSecondaryColor, float3(1, 0.4, 0.1), coronaNoise * 0.5);
    
    float3 finalColor = coreColor * core + coronaColor * corona;
    
    // Bright rim at core edge
    float rim = exp(-pow((dist - uRadius * 0.5) * 8.0, 2.0)) * 0.8;
    finalColor += float3(1, 0.6, 0.2) * rim;
    
    float alpha = saturate(core + corona + rim) * uOpacity * uIntensity;
    
    return float4(finalColor, 1.0) * alpha * sampleColor;
}

// Wrath shard trailing glow
float4 WrathShardTrail(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 1.2, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    
    float lengthFade = pow(1.0 - coords.x, 2.0);
    float widthFade = smoothstep(0.0, 0.25, coords.y) * smoothstep(1.0, 0.75, coords.y);
    
    float heatMap = pow(saturate(1.0 - abs(coords.y - 0.5) * 2.5), 2.0);
    float3 trailColor = lerp(uColor, uSecondaryColor, heatMap + noise * 0.3);
    trailColor = lerp(trailColor, float3(1, 0.8, 0.5), heatMap * lengthFade * 0.4);
    
    float alpha = lengthFade * widthFade * (0.7 + noise * 0.3) * uOpacity;
    
    return float4(trailColor * uIntensity, 1.0) * alpha * sampleColor;
}

technique EclipseOrbTechnique
{
    pass EclipseOrb
    {
        PixelShader = compile ps_3_0 EclipseOrbMain();
    }
}

technique WrathShardTrailTechnique
{
    pass WrathShardTrail
    {
        PixelShader = compile ps_3_0 WrathShardTrail();
    }
}
