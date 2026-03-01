// CampanellaInfernalTrail.fx  ELa Campanella boss movement trail
// Black smoke trails with embedded infernal orange fire cores
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_InfernalTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;
    float2 smokeUV = float2(uv.x * 2.0 - uTime * 1.5, uv.y * 1.5 + uTime * 0.3);
    float smoke = tex2D(uNoiseTex, smokeUV).r;
    float2 fireUV = float2(uv.x * 4.0 - uTime * 4.0, uv.y * 2.0);
    float fire = tex2D(uNoiseTex, fireUV).r;
    float edgeMask = smoothstep(1.0, 0.2, trailWidth + smoke * 0.4);
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 1.5);
    float4 smokeColor = float4(0.05, 0.03, 0.02, 1.0);
    float4 fireColor = uColor;
    float fireIntensity = smoothstep(0.5, 0.9, fire) * edgeMask;
    float4 color = lerp(smokeColor, fireColor, fireIntensity);
    float whiteHot = step(0.88, fire) * edgeMask;
    color = lerp(color, float4(1, 0.9, 0.7, 1), whiteHot * 0.6);
    float alpha = edgeMask * ageFade * uTrailWidth;
    return color * saturate(alpha);
}

technique Technique1
{
    pass InfernalTrail { PixelShader = compile ps_3_0 PS_InfernalTrail(); }
}
