// ══════════════════════════════════════════════════════════╕
// InvernoFrostFloor.fx — Seasons/Inverno frost floor spread
// Frost spreading across the ground plane. UV-scrolled noise
// creates creeping ice crystallization with glinting highlights.
// Used during Phase 2+ as environmental hazard visualization.
// ══════════════════════════════════════════════════════════╕

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTime;
float uIntensity;
float uSpreadProgress; // 0-1: how far frost has spread
float2 uOrigin;        // Normalized origin point of frost spread (0-1)

float hash21(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_FrostFloor(float2 uv : TEXCOORD0) : COLOR0
{
    // Distance from frost origin
    float2 delta = uv - uOrigin;
    float dist = length(delta);

    // Frost spreads outward from origin
    float spreadEdge = uSpreadProgress * 1.2;
    float frostMask = smoothstep(spreadEdge, spreadEdge - 0.15, dist);

    // Crystalline frost pattern — layered noise
    float2 frostUV1 = uv * 6.0 + float2(uTime * 0.03, uTime * 0.02);
    float2 frostUV2 = uv * 12.0 + float2(-uTime * 0.02, uTime * 0.04);
    float noise1 = tex2D(uNoiseTex, frostUV1).r;
    float noise2 = tex2D(uNoiseTex, frostUV2).r;
    float crystalPattern = noise1 * 0.6 + noise2 * 0.4;

    // Sharp crystalline edges
    float crystals = smoothstep(0.35, 0.55, crystalPattern);

    // Ice vein pattern — thinner, brighter lines
    float veins = smoothstep(0.62, 0.65, crystalPattern) * 0.8;

    // Glinting highlights — sparkle on frost surface
    float glintSeed = hash21(floor(uv * 40.0 + float2(uTime * 0.3, 0)));
    float glint = step(0.97, glintSeed);
    float glintPulse = pow(sin(uTime * 6.0 + glintSeed * 40.0) * 0.5 + 0.5, 6.0);

    // Edge glow where frost is actively spreading
    float edgeGlow = smoothstep(spreadEdge - 0.02, spreadEdge, dist)
                   * smoothstep(spreadEdge + 0.08, spreadEdge, dist);
    edgeGlow *= 1.5;

    // Color composition
    float4 frostBase = uColor * crystals * 0.7;
    float4 veinColor = float4(0.9, 0.95, 1.0, 1.0) * veins;
    float4 glintColor = float4(1.0, 1.0, 1.0, 1.0) * glint * glintPulse * 2.0;
    float4 edgeColor = float4(0.65, 0.85, 0.92, 1.0) * edgeGlow;

    float4 color = frostBase + veinColor + glintColor + edgeColor;
    float alpha = (crystals * 0.5 + veins * 0.3 + glint * glintPulse + edgeGlow * 0.6) * frostMask * uIntensity;

    return color * saturate(alpha);
}

technique Technique1
{
    pass FrostFloor
    {
        PixelShader = compile ps_3_0 PS_FrostFloor();
    }
}
