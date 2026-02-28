// ============================================================================
// TacetBulletTrail.fx — TacetsEnigma bullet trail
// UNIQUE SIGNATURE: Crystalline fracture shards — the trail looks like
// shattered glass or crystal fragments flying apart. Hard triangular edges
// with bright white-green edge lines at fracture boundaries. Fragments
// separate at trail edges. Geometric and sharp, NOT organic noise.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// Simple hash for triangular grid
float2 triHash(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

float4 PS_BulletFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Fast scroll for bullet speed
    float speed = lerp(1.5, 3.0, uIntensity);
    float2 scrollCoords = float2(coords.x * 10.0 - uTime * speed, coords.y * 6.0);

    // Triangular grid — creates angular shard pattern
    // Skew coordinates for triangular tiling
    float2 skew = float2(scrollCoords.x + scrollCoords.y * 0.5, scrollCoords.y * 0.866);
    float2 cell = floor(skew);
    float2 f = frac(skew);

    // Determine which triangle we're in (upper or lower)
    float triSelect = step(f.x + f.y, 1.0);

    // Hash for this triangle
    float2 h = triHash(cell + float2(triSelect, 1.0 - triSelect));

    // Distance to triangle edges — creates the fracture lines
    float edge1 = abs(f.x);
    float edge2 = abs(f.y);
    float edge3 = abs(f.x + f.y - 1.0) * 0.707;
    float minEdge = min(edge1, min(edge2, edge3));

    // Sharp edge highlight — bright fracture lines
    float fractureLine = smoothstep(0.06, 0.0, minEdge);

    // Shard interior — each triangle has its own brightness
    float shardBright = h.x * 0.6 + 0.4;
    float shardInterior = saturate(1.0 - fractureLine);

    // Shards separate at trail edges (fragments flying apart)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float separation = smoothstep(0.3, 0.8, edgeDist);
    // Add gap between shards at edges
    float shardGap = smoothstep(0.05, 0.1, minEdge + separation * 0.15);

    // Colors
    // Shard body: deep purple, per-shard variation
    float3 shardColor = uColor * shardBright * shardInterior * shardGap;
    // Fracture lines: bright white-green
    float3 lineColor = lerp(uSecondaryColor, float3(1, 1, 1), 0.4) * fractureLine * 1.5;
    lineColor *= (1.0 - separation * 0.5); // Lines dim as shards separate

    // Paradox bolt enhancement — greener, brighter for special shots
    float paradoxShift = uIntensity * 0.3;
    shardColor = lerp(shardColor, uSecondaryColor * shardBright, paradoxShift);
    lineColor *= (1.0 + uIntensity * 0.5);

    // Tip glow (bullet head)
    float tipBright = smoothstep(0.3, 0.0, 1.0 - coords.x) * 0.5;
    float3 tipColor = float3(0.5, 1.0, 0.6) * tipBright * uIntensity;

    float3 finalColor = shardColor + lineColor + tipColor;

    // Edge fade
    float edgeFade = 1.0 - smoothstep(0.6, 1.0, edgeDist);
    float finalAlpha = edgeFade * (shardInterior * shardGap + fractureLine) * uOpacity * input.Color.a;

    return float4(finalColor, saturate(finalAlpha));
}

technique TacetBulletFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BulletFlow();
    }
}
