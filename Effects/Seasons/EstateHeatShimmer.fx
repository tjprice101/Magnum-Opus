// L'Estate - Full-screen Heat Shimmer Distortion
// Applied as post-process every frame during boss fight
// Intensity scales from barely-visible (0.003) to violent (0.025)

sampler uImage0 : register(s0);
float uTime;
float uIntensity;
float uFrequency;
float uScrollSpeed;

float hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);

    float a = hash12(i);
    float b = hash12(i + float2(1.0, 0.0));
    float c = hash12(i + float2(0.0, 1.0));
    float d = hash12(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 HeatShimmerPS(float2 coords : TEXCOORD0) : COLOR0
{
    // Layered noise for organic variation
    float n = noise2D(coords * 8.0 + float2(0.0, uTime * uScrollSpeed * 0.5));
    float n2 = noise2D(coords * 16.0 + float2(uTime * 0.1, uTime * uScrollSpeed * 0.3));

    // Rising sine wave modulated by noise
    float wave = sin(coords.y * uFrequency + uTime * uScrollSpeed + n * 6.283);
    float detail = sin(coords.y * uFrequency * 2.3 + uTime * uScrollSpeed * 1.4) * 0.3;

    // Vertical bias: horizontal lines shimmer more than vertical
    float2 offset;
    offset.y = (wave + detail) * (n * 0.7 + n2 * 0.3) * uIntensity;
    offset.x = offset.y * 0.3;

    // Edge fadeout to prevent visible seams
    float edgeFade = smoothstep(0.0, 0.05, coords.x) * smoothstep(1.0, 0.95, coords.x)
                   * smoothstep(0.0, 0.05, coords.y) * smoothstep(1.0, 0.95, coords.y);
    offset *= edgeFade;

    return tex2D(uImage0, coords + offset);
}

technique HeatShimmerTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 HeatShimmerPS();
    }
}
