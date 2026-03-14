// L'Estate - Localized Heat Haze Trail
// Distortion offset applied to screen pixels near boss trail positions
// uPersistence controls how long distortion lingers, uVerticalBias makes heat rise

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uPersistence;
float uVerticalBias;
float uTime;

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

float4 PS_HeatHazeTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float dist = length(uv - uCenter);
    float mask = smoothstep(uRadius, uRadius * 0.3, dist);

    // Layered rising noise for organic distortion
    float n1 = noise2D(uv * 12.0 + float2(0.0, uTime * 1.5));
    float n2 = noise2D(uv * 24.0 + float2(uTime * 0.3, uTime * 2.5));

    // Vertical bias: heat rises
    float2 offset;
    offset.x = (n1 - 0.5) * 0.008 * mask;
    offset.y = ((n1 + n2) * 0.5 - 0.5) * 0.012 * mask * uVerticalBias;

    // Persistence decay from center outward
    float decay = pow(mask, uPersistence);
    offset *= decay;

    return tex2D(uImage0, uv + offset);
}

technique Technique1
{
    pass HeatHazeTrail
    {
        PixelShader = compile ps_3_0 PS_HeatHazeTrail();
    }
}
