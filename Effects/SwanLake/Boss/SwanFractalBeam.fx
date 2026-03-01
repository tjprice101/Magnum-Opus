// SwanFractalBeam.fx  ESwan Lake boss fractal beam attack shader
sampler uImage0 : register(s0);
float4 uColor;
float uIntensity;
float uTime;

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_FractalBeam(float2 uv : TEXCOORD0) : COLOR0
{
    float beamCenter = abs(uv.y - 0.5) * 2.0;
    float beamMask = smoothstep(1.0, 0.0, beamCenter);
    float scroll = frac(uv.x * 3.0 - uTime * 2.0);
    float fractalPattern = sin(uv.x * 20.0 + uv.y * 15.0 - uTime * 5.0);
    fractalPattern *= sin(uv.x * 13.0 - uv.y * 11.0 + uTime * 3.0);
    fractalPattern = fractalPattern * 0.5 + 0.5;
    float hue = frac(uv.x * 0.5 + uTime * 0.15);
    float3 rainbow = hsl2rgb(hue, 1.0, 0.65);
    float3 white = float3(1, 1, 1);
    float3 color = lerp(rainbow, white, beamMask * 0.5);
    color *= (fractalPattern * 0.4 + 0.6);
    float alpha = beamMask * uIntensity * (scroll * 0.3 + 0.7);
    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass FractalBeam { PixelShader = compile ps_3_0 PS_FractalBeam(); }
}
