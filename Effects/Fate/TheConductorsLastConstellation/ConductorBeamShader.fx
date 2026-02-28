// =============================================================================
// The Conductor's Last Constellation — Beam Shader
// =============================================================================
// Renders homing sword beam projectiles with conductor cyan-gold energy flow,
// electric pulsing edges, and starlight core.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: ConductorCyan
float3 uSecondaryColor;  // Secondary: LightningGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 BeamMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float lengthProg = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Scrolling energy flow
    float2 flowUV = float2(lengthProg * uNoiseScale - uTime * uScrollSpeed, coords.y * 2.0);
    float flow1 = SmoothNoise(flowUV * 5.0);
    float flow2 = SmoothNoise(flowUV * 11.0 + 2.71);
    float flow = flow1 * 0.6 + flow2 * 0.4;

    // Core beam body — bright center, soft edges
    float core = saturate(1.0 - cross / 0.25);
    core = core * core;

    float body = saturate(1.0 - cross * 0.8);
    body = sqrt(body);

    // Electric edge crackle
    float2 edgeUV = float2(lengthProg * 20.0 + uTime * 4.0, coords.y * 6.0);
    float edge = SmoothNoise(edgeUV);
    float edgeLine = smoothstep(0.6, 0.85, edge) * (1.0 - body) * 2.0;

    // Pulsing intensity along beam
    float beamPulse = sin(lengthProg * 12.0 - uTime * 6.0) * 0.15 + 0.85;

    // Color: void base → cyan body → gold edge crackle → white core
    float3 voidCol = float3(0.03, 0.02, 0.06);
    float3 cyanCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 whiteHot = float3(0.94, 0.96, 1.0);

    float3 color = lerp(voidCol, cyanCol, body * flow);
    color = lerp(color, goldCol, edgeLine);
    color = lerp(color, whiteHot, core * 0.8);
    color *= beamPulse;

    float alpha = (body * 0.5 + core * 0.35 + edgeLine * 0.15);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ConductorBeamMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 BeamMainPS();
    }
}
