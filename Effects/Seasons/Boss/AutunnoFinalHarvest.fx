// ══════════════════════════════════════════════════════════╁E
// AutunnoFinalHarvest.fx  ESeasons/Autunno death dissolve
// Leaves crumbling into decay  Enoise-based dissolve where
// edges glow warm orange as the boss disintegrates.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float uDissolveProgress;
float4 uEdgeColor;
float uEdgeWidth;

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

float4 PS_FinalHarvest(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Multi-octave dissolve noise  Eorganic leaf-crumble feel
    float n1 = noise(uv * 8.0);
    float n2 = noise(uv * 16.0 + float2(3.7, 1.2)) * 0.5;
    float n3 = noise(uv * 32.0 + float2(7.1, 5.3)) * 0.25;
    float dissolveNoise = (n1 + n2 + n3) / 1.75;

    // Dissolve threshold  Epixels below are removed
    float threshold = uDissolveProgress;
    float clipMask = step(threshold, dissolveNoise);

    // Glowing edge  Ewarm orange at the dissolve boundary
    float edgeDist = dissolveNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Leaf-like crumble: darker inner edge, bright outer edge
    float4 innerEdge = float4(0.4, 0.2, 0.05, 1.0);
    float4 outerEdge = uEdgeColor;
    float edgeBlend = smoothstep(0.0, uEdgeWidth * 0.5, edgeDist);
    float4 edgeColor = lerp(outerEdge, innerEdge, edgeBlend);

    // Ember sparkle at dissolve edge
    float sparkle = hash(floor(uv * 50.0 + dissolveNoise * 10.0));
    float sparkleMask = step(0.92, sparkle) * edgeGlow;

    float4 result = base * clipMask;
    result.rgb += edgeColor.rgb * edgeGlow * 1.5;
    result.rgb += float3(1.0, 0.85, 0.4) * sparkleMask * 2.0;
    result.a *= clipMask;

    return result;
}

technique Technique1
{
    pass FinalHarvest
    {
        PixelShader = compile ps_3_0 PS_FinalHarvest();
    }
}
