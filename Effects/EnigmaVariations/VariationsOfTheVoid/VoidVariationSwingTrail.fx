// ============================================================================
// VoidVariationSwingTrail.fx — VariationsOfTheVoid melee swing trail
// UNIQUE SIGNATURE: Voronoi cell fracture — reality shattering into irregular
// cells of darkness that drift apart at edges, revealing eerie green light
// bleeding through the cracks between cells. Not organic noise — GEOMETRIC.
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

// Hash-based 2D random for Voronoi
float2 hash2(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

// Voronoi distance field — returns (min distance, cell ID)
float2 voronoi(float2 p, float drift)
{
    float2 n = floor(p);
    float2 f = frac(p);
    float minDist = 8.0;
    float cellId = 0.0;

    for (int j = -1; j <= 1; j++)
    {
        for (int i = -1; i <= 1; i++)
        {
            float2 g = float2(i, j);
            float2 o = hash2(n + g);
            // Animate cell centers with slow drift
            o = 0.5 + 0.4 * sin(uTime * 0.6 + 6.2831 * o + drift);
            float2 r = g + o - f;
            float d = dot(r, r);
            if (d < minDist)
            {
                minDist = d;
                cellId = dot(n + g, float2(7.0, 113.0));
            }
        }
    }
    return float2(sqrt(minDist), frac(sin(cellId) * 43758.5453));
}

float4 PS_SwingFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Voronoi cell coordinates — scale controls cell density
    float2 cellUV = float2(coords.x * 6.0, coords.y * 4.0);
    // Scroll cells along trail
    cellUV.x -= uTime * 0.4;

    float2 vor = voronoi(cellUV, coords.x * 2.0);
    float cellDist = vor.x;    // distance to nearest cell center
    float cellId = vor.y;      // unique cell identifier

    // Cell edges — bright cracks between cells (the green light bleeding through)
    float edgeGlow = smoothstep(0.08, 0.0, cellDist - 0.35);

    // Cell interior darkness — each cell is a shard of void
    float cellInterior = smoothstep(0.15, 0.35, cellDist);
    float cellDark = 1.0 - cellInterior;

    // Cells separate further at trail edges (reality breaking apart more)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float separation = smoothstep(0.3, 0.8, edgeDist);
    edgeGlow *= (1.0 + separation * 2.5);

    // Per-cell color variation using cell ID
    float cellHue = frac(cellId * 3.17 + uTime * 0.2);

    // Void cell color — deep purple with subtle per-cell variation
    float3 voidColor = uColor * (0.4 + cellDark * 0.6);
    voidColor *= lerp(0.8, 1.2, cellHue);

    // Crack color — bright eerie green bleeding through
    float3 crackColor = uSecondaryColor * 1.6 * edgeGlow;
    // Cracks pulse with alien rhythm
    crackColor *= 0.85 + 0.15 * sin(uTime * 4.0 + cellId * 12.0);

    // Noise for fine detail within cells
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 0.3, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    voidColor += uColor * noise * 0.15 * cellDark;

    // Combine: dark void cells + bright green cracks
    float3 finalColor = voidColor + crackColor;

    // Edge fade
    float edgeFade = 1.0 - smoothstep(0.55, 1.0, edgeDist + separation * 0.2);
    float tipFade = smoothstep(0.0, 0.1, coords.x) * smoothstep(0.0, 0.1, 1.0 - coords.x);

    // Dissolution at tail — cells fully separate
    float tailDissolve = smoothstep(0.0, 0.3, coords.x);

    float finalAlpha = edgeFade * tipFade * tailDissolve * uOpacity * input.Color.a;
    finalAlpha *= saturate(cellDark + edgeGlow);

    return float4(finalColor * uIntensity, finalAlpha);
}

float4 PS_SwingGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float softEdge = exp(-edgeDist * edgeDist * 2.0);

    // Pulsing between void purple and crack green
    float pulse = sin(uTime * 1.8 + coords.x * 4.0) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor * 0.6, uSecondaryColor * 0.4, pulse);

    float tipFade = smoothstep(0.0, 0.15, coords.x) * smoothstep(0.0, 0.15, 1.0 - coords.x);
    float bloomStr = 0.2 + 0.15 * uIntensity;
    float finalAlpha = softEdge * tipFade * bloomStr * uOpacity * input.Color.a;

    return float4(glowColor, finalAlpha);
}

technique VoidVariationSwingFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingFlow();
    }
}

technique VoidVariationSwingGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingGlow();
    }
}
