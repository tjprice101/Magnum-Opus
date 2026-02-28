// =============================================================================
// Coda of Annihilation — Zenith Trail Shader
// =============================================================================
// Two techniques: ZenithMain (flying sword trail) and ZenithGlow (bloom overlay).
// Uses scrolling UV with cosmic noise and per-weapon color tinting.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float QuadBump(float x) { return x * (4.0 - x * 4.0); }

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// ---- ZenithMain: Core trail with scrolling cosmic energy ----
float4 ZenithMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    
    // Cross-section edge fade
    float edgeFade = QuadBump(coords.y);
    
    // Scrolling UV for energy flow
    float2 scrollUV = coords;
    scrollUV.x -= uTime * 0.8;
    scrollUV.y += sin(coords.x * 6.0 + uTime * 3.0) * 0.04;
    
    // Noise distortion
    float noise = HashNoise(scrollUV * 5.0 + uTime * 0.5);
    float energyFlow = sin(coords.x * 8.0 - uTime * 6.0) * 0.5 + 0.5;
    energyFlow *= energyFlow;
    
    // Color: weapon tint → cosmic white at tail
    float headiness = 1.0 - coords.x;
    float3 color = lerp(uSecondaryColor, uColor, headiness * headiness);
    color += float3(0.3, 0.25, 0.35) * energyFlow * headiness;
    
    // Alpha: edge fade × length fade × intensity
    float lengthFade = 1.0 - coords.x;
    float alpha = edgeFade * lengthFade * uOpacity * uIntensity * baseTex.a;
    alpha *= saturate(energyFlow + 0.4);
    
    return float4(color * alpha, alpha);
}

// ---- ZenithGlow: Bloom overlay — softer, wider, brighter ----
float4 ZenithGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float edgeFade = QuadBump(coords.y);
    float lengthFade = 1.0 - pow(coords.x, 0.8);
    
    float pulse = sin(uTime * 4.0 + coords.x * 10.0) * 0.15 + 0.85;
    
    float3 glowColor = lerp(uColor, float3(1, 1, 1), 0.3) * pulse;
    float alpha = edgeFade * lengthFade * uOpacity * 0.5;
    
    return float4(glowColor * alpha, alpha);
}

technique ZenithMain
{
    pass ZenithMainPass
    {
        PixelShader = compile ps_2_0 ZenithMainPS();
    }
}

technique ZenithGlow
{
    pass ZenithGlowPass
    {
        PixelShader = compile ps_2_0 ZenithGlowPS();
    }
}
