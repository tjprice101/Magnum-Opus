// EnigmaParadoxRift.fx — Attack shader for paradox/reality rift effects
sampler uImage0 : register(s0);
float4 uColor;
float uIntensity;
float uTime;

float4 PS_ParadoxRift(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    // Reality fracture lines
    float fracture1 = abs(sin(angle * 7.0 + uTime * 3.0));
    fracture1 = smoothstep(0.95, 1.0, fracture1);
    float fracture2 = abs(sin(angle * 11.0 - uTime * 2.0 + dist * 10.0));
    fracture2 = smoothstep(0.94, 1.0, fracture2);
    float fractures = max(fracture1, fracture2);
    // Void center pulling inward
    float voidPull = smoothstep(0.3, 0.0, dist);
    float voidRing = abs(dist - 0.15 - sin(uTime * 2.0) * 0.03);
    float ringMask = smoothstep(0.02, 0.0, voidRing);
    // Color
    float4 voidPurple = uColor;
    float4 eerieGreen = float4(0.2, 0.9, 0.3, 1);
    float4 white = float4(1,1,1,1);
    float4 color = voidPurple * voidPull;
    color += eerieGreen * fractures * 0.8;
    color += white * ringMask * 0.6;
    float alpha = (voidPull * 0.4 + fractures * 0.6 + ringMask * 0.8) * uIntensity;
    float radialFade = 1.0 - smoothstep(0.0, 0.5, dist);
    return color * saturate(alpha * radialFade);
}

technique Technique1
{
    pass ParadoxRift { PixelShader = compile ps_3_0 PS_ParadoxRift(); }
}
