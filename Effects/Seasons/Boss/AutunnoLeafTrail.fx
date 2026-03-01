// ══════════════════════════════════════════════════════════╁E
// AutunnoLeafTrail.fx  ESeasons/Autunno boss movement trail
// Falling leaf movement trail with tumbling leaf shapes,
// golden-orange to brown gradient, wind-scattered debris.
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

float4 PS_LeafTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Wind-blown core  Esinuous path with noise
    float2 windUV = float2(uv.x * 2.0 - uTime * 1.5, uv.y * 1.5 + uTime * 0.3);
    float windNoise = tex2D(uNoiseTex, windUV).r;
    float coreMask = smoothstep(0.8, 0.3, trailWidth + windNoise * 0.2);

    // Tumbling leaf shapes scattered along trail
    float2 leafUV = float2(uv.x * 6.0 - uTime * 2.0, uv.y * 5.0 + sin(uTime * 3.0 + uv.x * 8.0) * 0.2);
    float leafNoise = tex2D(uNoiseTex, leafUV).r;
    float leafShapes = smoothstep(0.7, 0.85, leafNoise);

    // Swirling debris at trail edges
    float2 debrisUV = float2(uv.x * 10.0 - uTime * 4.0, uv.y * 8.0);
    float debris = tex2D(uNoiseTex, debrisUV).r;
    float debrisMask = smoothstep(0.82, 0.92, debris) * smoothstep(1.0, 0.5, trailWidth);

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: warm orange core, golden leaves, brown debris
    float4 orangeCore = uColor;
    float4 goldLeaf = float4(0.95, 0.8, 0.2, 1.0);
    float4 brownDebris = float4(0.45, 0.25, 0.1, 1.0);

    float4 color = orangeCore * coreMask;
    color += goldLeaf * leafShapes * 0.8;
    color += brownDebris * debrisMask * 0.6;

    float alpha = (coreMask * 0.6 + leafShapes * 0.5 + debrisMask * 0.3) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass LeafTrail
    {
        PixelShader = compile ps_3_0 PS_LeafTrail();
    }
}
