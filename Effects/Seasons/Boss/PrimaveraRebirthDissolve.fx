// ══════════════════════════════════════════════════════════
// PrimaveraRebirthDissolve.fx — Vivaldi's Spring death dissolve
// Blooming flowers growing at the dissolve boundary,
// green vine tendrils extending from edges, bright petal
// shapes replacing the body, and warm golden glow.
// Parameters set by BossRenderHelper.DrawDissolve()
// ══════════════════════════════════════════════════════════

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

float fbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    float2 shift = float2(100.0, 100.0);
    for (int i = 0; i < 4; i++)
    {
        v += a * noise(p);
        p = p * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

float4 PS_RebirthDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // === ORGANIC DISSOLVE NOISE: vine/growth FBM pattern ===
    float dissolveNoise = fbm(uv * 5.5 + float2(0.0, 1.5));

    // Dissolve threshold
    float threshold = uDissolveProgress;
    float clipMask = step(threshold, dissolveNoise);

    // === BLOOMING EDGE: growth zone at dissolve boundary ===
    float edgeDist = dissolveNoise - threshold;
    float edgeMask = smoothstep(0.0, uEdgeWidth, edgeDist);
    float edgeGlow = (1.0 - edgeMask) * clipMask;

    // Wider outer glow halo (softer bloom around the edge)
    float outerEdgeDist = dissolveNoise - threshold;
    float outerGlow = smoothstep(0.0, uEdgeWidth * 2.5, outerEdgeDist);
    outerGlow = (1.0 - outerGlow) * clipMask * 0.4;

    // === VINE TENDRILS: extending from dissolve edge ===
    float vineAngle = atan2(uv.y - 0.5, uv.x - 0.5);
    float vineNoise = noise(float2(vineAngle * 6.0, dissolveNoise * 12.0));
    float vineTendril = smoothstep(0.55, 0.75, vineNoise) * edgeGlow;

    // === FLOWER PETAL SHAPES at dissolve boundary ===
    float petalSeed = hash(floor(uv * 35.0));
    float petalMask = step(0.85, petalSeed) * edgeGlow;

    // 5-fold petal shape within each cell
    float2 cellUV = frac(uv * 35.0) - 0.5;
    float cellAngle = atan2(cellUV.y, cellUV.x);
    float cellDist = length(cellUV);
    float petalShape = cos(cellAngle * 2.5) * 0.5 + 0.5;
    petalShape = smoothstep(0.3, 0.8, petalShape) * smoothstep(0.5, 0.1, cellDist);
    float flowerMask = petalShape * step(0.88, petalSeed) * edgeGlow;

    // === SPARKLE MOTES: tiny bright dots near dissolve edge ===
    float sparkleSeed = hash(floor(uv * 60.0));
    float sparkle = step(0.96, sparkleSeed) * edgeGlow;
    float sparkleFlicker = sin(sparkleSeed * 50.0 + uDissolveProgress * 20.0) * 0.3 + 0.7;

    // === COLOR COMPOSITION ===
    // Edge: user color (CherryPink) -> pink tips
    float4 greenVine = float4(0.35, 0.75, 0.25, 1.0);
    float4 pinkPetal = float4(1.0, 0.55, 0.7, 1.0);
    float4 whiteBloom = float4(1.0, 1.0, 0.92, 1.0);
    float4 warmGold = float4(1.0, 0.9, 0.6, 1.0);

    // Gradient along edge: green near body -> pink at tips
    float edgeBlend = smoothstep(0.0, uEdgeWidth * 0.7, edgeDist);
    float4 edgeColor = lerp(uEdgeColor, pinkPetal, edgeBlend * 0.5);
    edgeColor = lerp(edgeColor, greenVine, vineTendril * 0.3);

    // Assemble result
    float4 result = base * clipMask;

    // Edge glow (inner bright)
    result.rgb += edgeColor.rgb * edgeGlow * 1.3;
    // Outer halo glow (softer bloom)
    result.rgb += uEdgeColor.rgb * outerGlow * 0.5;
    // Vine tendrils
    result.rgb += greenVine.rgb * vineTendril * 0.8;
    // Flower shapes
    result.rgb += pinkPetal.rgb * flowerMask * 1.8;
    // Petal highlights (bright white centers)
    result.rgb += whiteBloom.rgb * flowerMask * step(0.95, petalSeed) * 2.5;
    // Sparkle motes
    result.rgb += warmGold.rgb * sparkle * sparkleFlicker * 1.5;

    result.a *= clipMask;

    return result;
}

technique Technique1
{
    pass RebirthDissolve
    {
        PixelShader = compile ps_3_0 PS_RebirthDissolve();
    }
}
