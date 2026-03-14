// ══════════════════════════════════════════════════════════╕
// InvernoFrostCreep.fx — Seasons/Inverno screen frost creep
// Frost crystallization creeping from screen edges inward.
// Combines noise-driven frost patterns with SDF edge math.
// Used during Phase 4 (Absolute Zero) to compress the arena.
// ══════════════════════════════════════════════════════════╕

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTime;
float uIntensity;
float uCreepDepth;   // 0-1: how far frost has crept from edges

float4 PS_FrostCreep(float2 uv : TEXCOORD0) : COLOR0
{
    // SDF from screen edges — distance to nearest edge (0 at edge, 0.5 at center)
    float edgeDist = min(min(uv.x, 1.0 - uv.x), min(uv.y, 1.0 - uv.y));

    // Frost creep boundary — noise-modulated edge
    float2 noiseUV1 = uv * 4.0 + float2(uTime * 0.015, uTime * 0.01);
    float2 noiseUV2 = uv * 8.0 + float2(-uTime * 0.008, uTime * 0.012);
    float noise1 = tex2D(uNoiseTex, noiseUV1).r;
    float noise2 = tex2D(uNoiseTex, noiseUV2).r;
    float noiseMod = noise1 * 0.6 + noise2 * 0.4;

    // Modulated creep boundary
    float creepBoundary = uCreepDepth * (0.3 + noiseMod * 0.15);
    float frostMask = smoothstep(creepBoundary, creepBoundary - 0.04, edgeDist);

    // Ice crystal pattern within frosted area
    float2 crystalUV = uv * 10.0 + float2(uTime * 0.02, uTime * 0.015);
    float crystalNoise = tex2D(uNoiseTex, crystalUV).r;
    float crystals = smoothstep(0.3, 0.6, crystalNoise);

    // Frost vein network — thin bright lines
    float2 veinUV = uv * 16.0 + float2(-uTime * 0.01, uTime * 0.008);
    float veinNoise = tex2D(uNoiseTex, veinUV).r;
    float veins = smoothstep(0.6, 0.63, veinNoise) * 0.9;

    // Edge glow at the frost boundary — living, advancing edge
    float edgeGlow = smoothstep(creepBoundary - 0.01, creepBoundary, edgeDist)
                   * smoothstep(creepBoundary + 0.05, creepBoundary, edgeDist);
    edgeGlow *= (0.8 + sin(uTime * 3.0 + uv.x * 20.0 + uv.y * 15.0) * 0.2);

    // Depth-based opacity — thicker frost closer to edges
    float depthFade = 1.0 - smoothstep(0.0, creepBoundary, edgeDist);

    // Color composition
    float4 frostBase = uColor * crystals * depthFade;
    float4 veinColor = float4(0.85, 0.92, 1.0, 1.0) * veins * depthFade * 0.8;
    float4 edgeEmission = float4(0.7, 0.9, 1.0, 1.0) * edgeGlow * 1.8;

    float4 color = frostBase + veinColor + edgeEmission;
    float alpha = (crystals * depthFade * 0.6 + veins * depthFade * 0.3 + edgeGlow * 0.8)
                * frostMask * uIntensity;

    return color * saturate(alpha);
}

technique Technique1
{
    pass FrostCreep
    {
        PixelShader = compile ps_3_0 PS_FrostCreep();
    }
}
