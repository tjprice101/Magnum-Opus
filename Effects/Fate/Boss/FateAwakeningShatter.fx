// ══════════════════════════════════════════════════════════╁E
// FateAwakeningShatter.fx  EFate boss True Form awakening
// Screen shatters like glass with cosmic energy bleeding
// through the cracks, dark pink/crimson/celestial white.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;  // 0 = intact, 1 = fully shattered
float4 uFromColor;          // Dark pink pre-awakening
float4 uToColor;            // Celestial white post-awakening
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_AwakeningShatter(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);

    // Voronoi-style glass shard cells
    float2 cellUV = uv * 8.0;
    float2 cellID = floor(cellUV);
    float2 cellFrac = frac(cellUV);
    float minDist = 1.0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(x, y);
            float2 cellPoint = float2(hash(cellID + neighbor), hash(cellID + neighbor + 99.0));
            float2 diff = neighbor + cellPoint - cellFrac;
            float d = length(diff);
            minDist = min(minDist, d);
        }
    }

    // Crack lines at cell edges
    float crackMask = smoothstep(0.05, 0.0, minDist);
    float crackGrow = smoothstep(0.0, 0.5, uTransitionProgress);
    crackMask *= crackGrow;

    // Shard displacement  Eeach shard shifts outward as progress increases
    float shardSeed = hash(cellID);
    float2 shardOffset = (centered * 0.1 + float2(shardSeed, hash(cellID + 50.0)) * 0.05) * uTransitionProgress;

    // Cosmic energy bleeding through cracks
    float cosmicNoise = tex2D(uNoiseTex, uv * 2.0 + float2(uTime * 0.5, uTime * 0.3)).r;
    float cosmicBleed = crackMask * cosmicNoise * uIntensity;

    // Color: cracks glow crimson -> celestial white, shards tinted pink
    float4 crackColor = lerp(uFromColor, uToColor, uTransitionProgress);
    float4 cosmicWhite = float4(1.0, 0.95, 1.0, 1.0);
    float4 shardTint = lerp(float4(1,1,1,1), uFromColor, uTransitionProgress * 0.3);

    float4 base = tex2D(uImage0, uv + shardOffset) * shardTint;
    base += crackColor * crackMask * uIntensity * 2.0;
    base += cosmicWhite * cosmicBleed * 0.8;

    // Bright flash at peak transition
    float flash = pow(sin(uTransitionProgress * 3.14159), 4.0) * uIntensity;
    base += cosmicWhite * flash * 0.3;

    return base;
}

technique Technique1
{
    pass AwakeningShatter
    {
        PixelShader = compile ps_3_0 PS_AwakeningShatter();
    }
}
