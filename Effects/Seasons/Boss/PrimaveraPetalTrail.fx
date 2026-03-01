// ══════════════════════════════════════════════════════════╁E
// PrimaveraPetalTrail.fx  ESeasons/Primavera boss trail
// Spring petal movement trail with cherry blossom-like petals
// drifting along the path, green-to-pink gradient.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_PetalTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Soft petal cloud core
    float2 cloudUV = float2(uv.x * 2.5 - uTime * 1.5, uv.y * 2.0 + uTime * 0.2);
    float cloud = tex2D(uNoiseTex, cloudUV).r;
    float coreSoft = smoothstep(0.7, 0.2, trailWidth) * cloud;

    // Individual petal shapes floating along trail
    float2 petalUV = float2(uv.x * 8.0 - uTime * 2.5, uv.y * 6.0 + sin(uv.x * 10.0 + uTime) * 0.3);
    float petalNoise = tex2D(uNoiseTex, petalUV).r;
    float petals = smoothstep(0.72, 0.88, petalNoise);

    // Tiny pollen sparkle dots
    float pollenSeed = hash(floor(uv * 40.0 + float2(-uTime * 2.0, uTime * 0.5)));
    float pollen = step(0.94, pollenSeed);

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: soft pink petals, green undertone, yellow pollen
    float4 pinkPetal = uColor;
    float4 greenBase = float4(0.3, 0.65, 0.25, 1.0);
    float4 yellowPollen = float4(1.0, 0.9, 0.4, 1.0);

    float trailMix = smoothstep(0.0, 1.0, trailWidth);
    float4 color = lerp(pinkPetal, greenBase, trailMix * 0.5) * coreSoft;
    color += pinkPetal * petals * 0.9;
    color += yellowPollen * pollen * 0.5;

    float alpha = (coreSoft * 0.5 + petals * 0.6 + pollen * 0.3) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass PetalTrail
    {
        PixelShader = compile ps_3_0 PS_PetalTrail();
    }
}
