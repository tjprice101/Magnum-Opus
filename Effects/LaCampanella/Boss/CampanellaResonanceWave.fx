// CampanellaResonanceWave.fx  EAttack shader for bell toll shockwaves
// Concentric rings that expand outward with fire-tinged edges
sampler uImage0 : register(s0);
float4 uColor;
float uIntensity;
float uTime;

float4 PS_ResonanceWave(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float expandTime = frac(uTime * 0.5);
    float ring = abs(dist - expandTime * 0.5);
    float ringMask = smoothstep(0.03, 0.0, ring);
    float ring2 = abs(dist - expandTime * 0.35);
    float ringMask2 = smoothstep(0.02, 0.0, ring2);
    float fade = 1.0 - expandTime;
    float4 color = uColor * ringMask * fade * uIntensity;
    color += uColor * ringMask2 * fade * uIntensity * 0.5;
    color.a = saturate(color.r + color.g + color.b);
    return color;
}

technique Technique1
{
    pass ResonanceWave { PixelShader = compile ps_3_0 PS_ResonanceWave(); }
}
