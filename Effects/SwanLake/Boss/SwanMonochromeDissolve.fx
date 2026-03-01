// SwanMonochromeDissolve.fx  EDeath dissolve with grayscale-to-rainbow shatter
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_MonochromeDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01) return float4(0,0,0,0);
    // Convert to grayscale first (monochromatic fractal identity)
    float gray = dot(sprite.rgb, float3(0.299, 0.587, 0.114));
    float3 monoSprite = lerp(sprite.rgb, float3(gray, gray, gray), uDissolveProgress * 0.8);
    float n = tex2D(uNoiseTex, uv * 2.0).r;
    float threshold = uDissolveProgress * 1.2;
    float clipVal = n - threshold;
    if (clipVal < 0.0) return float4(0,0,0,0);
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);
    // Rainbow edge (the fractal's true colors breaking free at death)
    float hue = frac(n * 3.0 + uDissolveProgress);
    float3 rainbow = hsl2rgb(hue, 1.0, 0.7);
    float3 edgeColor = lerp(rainbow, float3(1,1,1), edge * 0.5);
    float3 result = lerp(monoSprite, edgeColor, edge);
    return float4(result, sprite.a);
}

technique Technique1
{
    pass MonochromeDissolve { PixelShader = compile ps_3_0 PS_MonochromeDissolve(); }
}
