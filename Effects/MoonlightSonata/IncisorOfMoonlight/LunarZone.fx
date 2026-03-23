// LunarZone.fx
// Moonlight Sonata persistent lunar zone shader.
// Dual-noise UV-scrolling masked to a circle with breathing animation.
// Based on DamageZoneShader pattern, enhanced with second noise layer.

sampler uImage0 : register(s0);

float uTime;
float scrollSpeed1;
float scrollSpeed2;
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

texture noise2Tex;
sampler2D samplerNoise2 = sampler_state
{
    texture = <noise2Tex>;
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

float4 LunarZonePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float normDist = dist * 2.0;

    // Breathing pulse
    float breathedDist = normDist / breathe;

    // Primary noise: scrolling diagonally
    float2 noiseUV1 = float2(
        uv.x * 1.5 + uTime * rotationSpeed,
        uv.y * 1.5 + uTime * scrollSpeed1);
    float noise1 = tex2D(samplerNoise, noiseUV1).r;

    // Secondary noise: scrolling opposite direction
    float2 noiseUV2 = float2(
        uv.x * 2.0 - uTime * scrollSpeed2 * 0.7,
        uv.y * 2.0 + uTime * rotationSpeed * 0.5);
    float noise2 = tex2D(samplerNoise2, noiseUV2).r;

    // Blend both noise layers
    float combinedNoise = noise1 * 0.6 + noise2 * 0.4;

    // Gradient LUT coloring
    float3 gradColor = tex2D(samplerGradient, float2(combinedNoise, 0.5)).rgb;

    // Color mixing
    float3 baseColor = lerp(primaryColor * gradColor, coreColor, combinedNoise * combinedNoise);
    baseColor *= intensity;

    // Sparkle at noise peaks
    baseColor += coreColor * pow(saturate(combinedNoise), 3.0) * 0.6;

    // Circular mask with breathing
    float adjustedRadius = circleRadius * breathe;
    float circleMask = 1.0 - smoothstep(adjustedRadius - edgeSoftness, adjustedRadius, breathedDist);

    // Core brightness
    float coreBrightness = saturate(1.0 - breathedDist * 0.2);

    // Final composite
    float3 finalColor = baseColor * circleMask * coreBrightness;
    float finalAlpha = circleMask * saturate(combinedNoise * 2.0 + 0.15) * fadeAlpha;

    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass LunarZonePass
    {
        PixelShader = compile ps_2_0 LunarZonePS();
    }
}
