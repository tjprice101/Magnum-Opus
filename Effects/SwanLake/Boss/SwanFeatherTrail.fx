// SwanFeatherTrail.fx  ESwan Lake boss elegant movement trail
// White feather-like wisps with rainbow edge shimmer
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_FeatherTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trail = uv.x;
    float width = abs(uv.y - 0.5) * 2.0;
    float featherNoise = tex2D(uNoiseTex, float2(uv.x * 3.0 - uTime, uv.y * 2.0)).r;
    float featherShape = smoothstep(1.0, 0.1, width + featherNoise * 0.3);
    float vane = sin(uv.y * 40.0 + uv.x * 10.0 - uTime * 5.0);
    vane = smoothstep(0.5, 1.0, vane) * featherShape;
    float ageFade = pow(1.0 - trail, uFadeRate);
    // White core with rainbow edges
    float3 white = uColor.rgb;
    float edgeHue = frac(trail * 2.0 + uTime * 0.2);
    float3 rainbow = hsl2rgb(edgeHue, 0.9, 0.7);
    float3 color = lerp(white, rainbow, width * 0.6);
    color += vane * 0.2;
    float alpha = featherShape * ageFade * uTrailWidth;
    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass FeatherTrail { PixelShader = compile ps_3_0 PS_FeatherTrail(); }
}
