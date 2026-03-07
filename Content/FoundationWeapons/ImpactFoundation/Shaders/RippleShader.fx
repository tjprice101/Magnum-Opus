// RippleShader.fx
// Concentric ring ripple shader for ImpactFoundation.
// Optimized for ps_2_0 (max 64 arithmetic instructions).
//
// Draws expanding rings with noise wobble, masked to a circle.
// Supports LUT gradient texture for theme-consistent coloring.

sampler uImage0 : register(s0);

float uTime;
float progress;
float ringCount;
float ringThickness;
float3 primaryColor;
float3 secondaryColor;
float3 coreColor;
float fadeAlpha;
float useGradient; // 0.0 = hardcoded colors, 1.0 = sample gradientTex LUT

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
    mipfilter = NONE;
    AddressU = clamp;
    AddressV = clamp;
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
    
    // Color — LUT gradient or legacy hardcoded lerp
    // Legacy: core at center → secondary in mid → primary at edge (now uses all 3 colors)
    float gradDist = saturate(normDist * 2.0);
    float3 legacyColor = lerp(lerp(coreColor, secondaryColor, gradDist), primaryColor, saturate(gradDist - 0.5) * 2.0);
    
    // Gradient LUT: sample horizontally, center=bright end (u=1), edge=dark end (u=0)
    float3 gradientColor = tex2D(samplerGradient, float2(saturate(1.0 - normDist), 0.5)).rgb;
    
    // Blend between legacy and gradient based on useGradient uniform
    float3 ringColor = lerp(legacyColor, gradientColor, saturate(useGradient));
    
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
