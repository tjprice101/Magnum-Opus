// EnigmaShadowTrail.fx  EEnigma boss teleport/shadow dash trail
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_ShadowTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trail = uv.x;
    float width = abs(uv.y - 0.5) * 2.0;
    float shadow = tex2D(uNoiseTex, float2(uv.x * 2.0 - uTime * 3.0, uv.y * 1.5)).r;
    float voidWisp = tex2D(uNoiseTex, float2(uv.x * 4.0 + uTime * 2.0, uv.y * 3.0)).r;
    float edgeMask = smoothstep(1.0, 0.15, width + shadow * 0.4);
    float ageFade = pow(1.0 - trail, uFadeRate * 2.0);
    // Dark void core with green edge glow
    float4 voidBlack = float4(0.02, 0.01, 0.05, 1);
    float4 greenGlow = uColor;
    float edgeGreen = smoothstep(0.3, 0.7, width) * edgeMask;
    float voidWisps = smoothstep(0.6, 0.9, voidWisp) * edgeMask;
    float4 color = lerp(voidBlack, greenGlow, edgeGreen + voidWisps * 0.5);
    float alpha = edgeMask * ageFade * uTrailWidth;
    return color * saturate(alpha);
}

technique Technique1
{
    pass ShadowTrail { PixelShader = compile ps_3_0 PS_ShadowTrail(); }
}
