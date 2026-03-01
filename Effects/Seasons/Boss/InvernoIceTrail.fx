// ══════════════════════════════════════════════════════════╁E
// InvernoIceTrail.fx  ESeasons/Inverno boss movement trail
// Ice crystal movement trail with frozen shards along path,
// frosty mist dispersal and glinting ice-blue highlights.
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

float4 PS_IceTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Frost mist core  Esoft icy cloud
    float2 frostUV = float2(uv.x * 2.0 - uTime * 0.8, uv.y * 1.5);
    float frostCloud = tex2D(uNoiseTex, frostUV).r;
    float mistCore = smoothstep(0.7, 0.2, trailWidth) * frostCloud;

    // Ice crystal shards  Esharp angular shapes along trail
    float2 shardUV = float2(uv.x * 8.0 - uTime * 1.0, uv.y * 6.0);
    float shardNoise = tex2D(uNoiseTex, shardUV).r;
    // Sharp threshold for crystalline look
    float shards = smoothstep(0.75, 0.78, shardNoise) * smoothstep(0.9, 0.3, trailWidth);

    // Glinting highlights  Ebright specular flashes on ice
    float glintSeed = hash(floor(uv * 30.0 + float2(uTime * 0.5, 0)));
    float glint = step(0.96, glintSeed);
    float glintFlash = pow(sin(uTime * 8.0 + glintSeed * 50.0) * 0.5 + 0.5, 4.0);

    // Frost edge dispersion  Ecrystalline fog at trail edges
    float2 edgeUV = float2(uv.x * 4.0 - uTime * 1.2, uv.y * 5.0 + uTime * 0.3);
    float edgeFrost = tex2D(uNoiseTex, edgeUV).r;
    float edgeIce = smoothstep(0.5, 0.8, trailWidth) * smoothstep(1.0, 0.7, trailWidth) * edgeFrost;

    // Age fade  Efrost lingers longer than fire
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 1.5);

    // Colors: ice blue core, white shards, silver glints, pale blue mist
    float4 iceBlue = uColor;
    float4 frostWhite = float4(0.9, 0.95, 1.0, 1.0);
    float4 silverGlint = float4(1.0, 1.0, 1.0, 1.0);
    float4 paleMist = float4(0.7, 0.8, 0.95, 1.0);

    float4 color = iceBlue * mistCore;
    color += frostWhite * shards * 1.2;
    color += silverGlint * glint * glintFlash * 1.5;
    color += paleMist * edgeIce * 0.4;

    float alpha = (mistCore * 0.5 + shards * 0.7 + glint * glintFlash + edgeIce * 0.3) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass IceTrail
    {
        PixelShader = compile ps_3_0 PS_IceTrail();
    }
}
