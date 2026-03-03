// DamageZoneShader.fx
// Damage zone shader for ImpactFoundation.
// Optimized for ps_2_0 (max 64 arithmetic instructions).
//
// Radially scrolling noise masked to a circle with breathing animation.
// Inspired by MaskFoundation's RadialNoiseMaskShader.

sampler uImage0 : register(s0);

float uTime;
float scrollSpeed;
float rotationSpeed;
float circleRadius;
float edgeSoftness;
float intensity;
float3 primaryColor;
float3 coreColor;
float fadeAlpha;
float breathe;

texture noiseTex;
sampler2D samplerNoise = sampler_state
{
    texture = <noiseTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

float4 DamageZonePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float normDist = dist * 2.0;
    
    // Breathing pulse — modulate distance
    float breathedDist = normDist / breathe;
    
    // Scrolling noise UV (use UV-based coords instead of atan2)
    float2 noiseUV = float2(
        uv.x * 1.5 + uTime * rotationSpeed,
        uv.y * 1.5 + uTime * scrollSpeed);
    
    // Single noise sample
    float noiseVal = tex2D(samplerNoise, noiseUV).r;
    
    // Gradient LUT coloring
    float3 gradColor = tex2D(samplerGradient, float2(noiseVal, 0.5)).rgb;
    
    // Color mixing — lerp between primary*gradient and core based on noise
    float3 baseColor = lerp(primaryColor * gradColor, coreColor, noiseVal * noiseVal);
    baseColor *= intensity;
    
    // Sparkle at noise peaks
    baseColor += coreColor * noiseVal * noiseVal * noiseVal * 0.5;
    
    // Circular mask
    float adjustedRadius = circleRadius * breathe;
    float circleMask = 1.0 - smoothstep(adjustedRadius - edgeSoftness, adjustedRadius, normDist);
    
    // Core brightness falloff
    float coreBrightness = saturate(1.0 - normDist * 0.25);
    
    // Final composite
    float3 finalColor = baseColor * circleMask * coreBrightness;
    float finalAlpha = circleMask * saturate(noiseVal * 2.0 + 0.2) * fadeAlpha;
    
    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass DamageZonePass
    {
        PixelShader = compile ps_2_0 DamageZonePS();
    }
}
