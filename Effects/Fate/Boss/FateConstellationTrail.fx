// ══════════════════════════════════════════════════════════╁E
// FateConstellationTrail.fx  EFate boss movement trail
// Star constellation trail with connected star points flowing
// through deep space void with cosmic thread connections.
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

float4 PS_ConstellationTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Deep void base with subtle nebula swirl
    float2 noiseUV = float2(uv.x * 2.0 - uTime * 0.5, uv.y * 1.5);
    float voidNoise = tex2D(uNoiseTex, noiseUV).r;
    float voidBase = voidNoise * 0.3;

    // Star points along the trail  Egrid hash
    float2 starGrid = floor(uv * float2(16.0, 4.0));
    float starSeed = hash(starGrid + float2(0, uTime * 0.1));
    float starMask = step(0.88, starSeed);
    float starBright = sin(uTime * 6.0 + starSeed * 40.0) * 0.3 + 0.7;

    // Constellation lines  Ehorizontal connections between star cells
    float lineY = abs(uv.y - 0.5);
    float lineAlpha = smoothstep(0.02, 0.0, lineY) * 0.3;
    float2 lineNoiseUV = float2(uv.x * 4.0 - uTime, 0.5);
    float linePulse = tex2D(uNoiseTex, lineNoiseUV).r;
    lineAlpha *= smoothstep(0.3, 0.6, linePulse);

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);
    float edgeFade = smoothstep(1.0, 0.4, trailWidth);

    // Colors: void purple base, celestial white stars, pink thread lines
    float4 voidColor = float4(0.05, 0.0, 0.08, 1.0);
    float4 starColor = float4(1.0, 0.95, 1.0, 1.0);
    float4 lineColor = uColor;

    float4 color = voidColor * voidBase;
    color += starColor * starMask * starBright * 1.5;
    color += lineColor * lineAlpha;

    float alpha = (voidBase * 0.4 + starMask * starBright + lineAlpha) * ageFade * edgeFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass ConstellationTrail
    {
        PixelShader = compile ps_3_0 PS_ConstellationTrail();
    }
}
