// =============================================================================
// The Conductor's Last Constellation — Lightning Cascade Shader
// =============================================================================
// Renders electric lightning cascade effect — jagged branching energy
// with cyan-gold color and bright white core. Used for lightning strikes
// called down on enemy hit.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: LightningGold
float3 uSecondaryColor;  // Secondary: ConductorCyan
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
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

float4 LightningMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float lengthProg = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Jagged lightning bolt pattern
    float2 boltUV = float2(lengthProg * uNoiseScale * 2.0, coords.y * 8.0 + uTime * 3.0);
    float bolt1 = SmoothNoise(boltUV * 3.0);
    float bolt2 = SmoothNoise(boltUV * 7.0 + 1.618);
    float bolt = bolt1 * 0.6 + bolt2 * 0.4;

    // Lightning core — thin bright center
    float boltCenter = abs(coords.y - 0.5 + (bolt - 0.5) * 0.3);
    float core = saturate(1.0 - boltCenter / 0.08);
    core = core * core;

    // Body glow around bolt
    float body = saturate(1.0 - boltCenter / 0.3);
    body = sqrt(body);

    // Branch sparks
    float2 branchUV = coords * float2(25.0, 12.0) + uTime * 2.0;
    float branch = HashNoise(floor(branchUV));
    branch = step(0.88, branch) * body;

    // Flicker
    float flicker = sin(uTime * 15.0 + lengthProg * 20.0) * 0.2 + 0.8;

    // Color: deep void → cyan body → gold core → white-hot center
    float3 voidCol = float3(0.03, 0.02, 0.06);
    float3 cyanCol = uSecondaryColor;
    float3 goldCol = uColor;
    float3 whiteHot = float3(0.96, 0.98, 1.0);

    float3 color = lerp(voidCol, cyanCol, body * 0.6);
    color = lerp(color, goldCol, core * 0.7);
    color = lerp(color, whiteHot, core * 0.5);
    color += cyanCol * branch * 1.5;
    color *= flicker;

    float alpha = (body * 0.4 + core * 0.5 + branch * 0.1);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ConductorLightningMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 LightningMainPS();
    }
}
