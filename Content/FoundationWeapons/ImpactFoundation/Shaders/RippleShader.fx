// RippleShader.fx
// Concentric ring ripple shader for ImpactFoundation.
// Optimized for ps_2_0 (max 64 arithmetic instructions).
//
// Draws expanding rings with noise wobble, masked to a circle.

sampler uImage0 : register(s0);

float uTime;
float progress;
float ringCount;
float ringThickness;
float3 primaryColor;
float3 secondaryColor;
float3 coreColor;
float fadeAlpha;

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

static const float PI = 3.14159265;

float4 RipplePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float normDist = dist * 2.0;
    
    // Noise distortion — use UV directly (skip atan2 to save instructions)
    float2 noiseUV = float2(uv.x + uTime * 0.1, uv.y + uTime * 0.05);
    float noiseVal = tex2D(samplerNoise, noiseUV).r;
    float distortedDist = normDist + (noiseVal - 0.5) * 0.06;
    
    // Ring generation — sin wave pattern expanding outward
    float expandOffset = progress * 1.2;
    float ringPhase = (distortedDist - expandOffset) * ringCount * PI * 2.0;
    float ringWave = sin(ringPhase);
    float ringMask = smoothstep(1.0 - ringThickness * 8.0, 1.0, ringWave);
    
    // Distance fade + circle mask combined
    float circleMask = (1.0 - smoothstep(0.0, 0.5, normDist)) * (1.0 - smoothstep(0.42, 0.5, normDist));
    
    // Color — lerp between core and primary based on distance
    float3 ringColor = lerp(coreColor, primaryColor, saturate(normDist * 2.0));
    
    // Composite
    float finalIntensity = ringMask * circleMask;
    float3 finalColor = ringColor * finalIntensity * 2.0;
    float finalAlpha = finalIntensity * fadeAlpha;
    
    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass RipplePass
    {
        PixelShader = compile ps_2_0 RipplePS();
    }
}
