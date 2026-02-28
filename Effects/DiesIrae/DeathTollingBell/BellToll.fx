// Death Tolling Bell - BellToll Shader
// Expanding concentric shockwave rings with reverb decay

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Noise

float uTime;
float3 uColor;          // CharcoalBlack
float3 uSecondaryColor; // BloodRed
float uOpacity;
float uIntensity;
float uRadius;           // Current expansion radius
float uRingCount;        // Number of concentric rings
float uDecay;            // Ring fade-out rate

float4 BellTollMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    // Multiple concentric rings
    float rings = 0.0;
    for (float i = 0.0; i < 4.0; i += 1.0)
    {
        if (i >= uRingCount) break;
        float ringRadius = uRadius * (1.0 - i * 0.2);
        float ringWidth = 0.04 + i * 0.015;
        float ringDist = abs(dist - ringRadius);
        float ring = smoothstep(ringWidth, 0.0, ringDist);
        ring *= pow(0.7, i); // Each ring is fainter
        rings += ring;
    }
    
    // Noise distortion for organic ring wobble
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float2 noiseUV = float2(angle / 6.28318, dist + uTime * 0.1);
    float noise = tex2D(uImage1, noiseUV).r;
    rings *= 0.7 + noise * 0.3;
    
    // Decay over time
    float decayFactor = exp(-uDecay * uTime);
    rings *= decayFactor;
    
    // Color: inner rings are brighter, outer are darker
    float colorMix = saturate(dist / uRadius);
    float3 ringColor = lerp(uSecondaryColor, uColor, colorMix);
    
    // Hot center flash
    float centerFlash = pow(saturate(1.0 - dist / (uRadius * 0.3)), 4.0) * decayFactor;
    ringColor = lerp(ringColor, float3(1, 0.8, 0.6), centerFlash);
    
    float alpha = saturate(rings + centerFlash * 0.5) * uOpacity * uIntensity;
    
    return float4(ringColor, 1.0) * alpha * sampleColor;
}

// Death knell dark shockwave - second pass
float4 DeathKnellMain(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = distance(coords, center) * 2.0;
    
    // Single expanding dark ring with bright edge
    float ringDist = abs(dist - uRadius);
    float darkRing = smoothstep(0.15, 0.0, ringDist);
    float brightEdge = exp(-ringDist * ringDist * 400.0);
    
    // Noise for organic look
    float angle = atan2(coords.y - 0.5, coords.x - 0.5);
    float2 noiseUV = float2(angle / 6.28318 * 3.0 + uTime * 0.2, dist * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    
    // Dark interior with bright edge
    float3 darkColor = uColor * 0.2 * darkRing;
    float3 edgeColor = uSecondaryColor * brightEdge * 2.0;
    float3 finalColor = darkColor + edgeColor + float3(1, 0.6, 0.3) * brightEdge * noise * 0.5;
    
    float alpha = saturate(darkRing * 0.3 + brightEdge) * uOpacity * uIntensity;
    
    return float4(finalColor, 1.0) * alpha * sampleColor;
}

technique BellTollTechnique
{
    pass BellToll
    {
        PixelShader = compile ps_3_0 BellTollMain();
    }
}

technique DeathKnellTechnique
{
    pass DeathKnell
    {
        PixelShader = compile ps_3_0 DeathKnellMain();
    }
}
