// ══════════════════════════════════════════════════════════╁E
// InvernoFreezeRay.fx  ESeasons/Inverno phase transition
// Freeze ray expanding outward  Eice crystallization spreading
// from center, frost patterns consuming the screen.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_FreezeRay(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Ice crystallization front  Eexpands outward with progress
    float freezeRadius = uTransitionProgress * 0.8;
    float freezeFront = smoothstep(freezeRadius + 0.05, freezeRadius - 0.02, dist);

    // Crystalline edge  Esharp frost boundary
    float edgeNoise = noise(float2(angle * 6.0 + uTime * 0.5, dist * 10.0));
    float frozenEdge = smoothstep(0.0, 0.06, abs(dist - freezeRadius) + edgeNoise * 0.03);
    float edgeBright = (1.0 - frozenEdge) * step(0.01, freezeRadius);

    // Ice facets inside frozen area  Ecrystalline Voronoi-like pattern
    float facetNoise = noise(float2(uv.x * 15.0 + uTime * 0.2, uv.y * 15.0));
    float facets = smoothstep(0.4, 0.6, facetNoise) * freezeFront;

    // Frost fern patterns at the expansion edge
    float fernNoise = noise(float2(angle * 8.0 + uTime * 0.8, dist * 20.0));
    float ferns = smoothstep(0.6, 0.85, fernNoise);
    float fernMask = ferns * smoothstep(0.1, 0.0, abs(dist - freezeRadius));

    // Phase blend
    float phaseMix = smoothstep(0.3, 0.7, uTransitionProgress);
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);

    // Colors: ice blue frozen area, white crystal edge, silver facets
    float4 iceBlue = float4(0.4, 0.65, 0.95, 1.0);
    float4 crystalWhite = float4(0.9, 0.95, 1.0, 1.0);
    float4 silverFrost = float4(0.75, 0.82, 0.92, 1.0);

    float4 result = base;
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.4);
    result.rgb = lerp(result.rgb, iceBlue.rgb, freezeFront * uIntensity * 0.5);
    result.rgb += silverFrost.rgb * facets * uIntensity * 0.3;
    result.rgb += crystalWhite.rgb * edgeBright * uIntensity * 1.2;
    result.rgb += crystalWhite.rgb * fernMask * uIntensity * 0.6;

    return result;
}

technique Technique1
{
    pass FreezeRay
    {
        PixelShader = compile ps_3_0 PS_FreezeRay();
    }
}
