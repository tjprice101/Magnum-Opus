// =============================================================================
// Requiem of Reality — Combo Aura Shader
// =============================================================================
// Radial aura that erupts when the spectral blade combo triggers.
// Concentric rings of destiny expanding outward, with cosmic glyph-like
// angular segments and a pulsing void core. Phase drives expansion.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // BrightCrimson
float3 uSecondaryColor;  // FatePurple
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uNoiseScale;
float uPhase;            // 0=dormant, 1=fully expanded
float uHasSecondaryTex;

float4x4 uTransformMatrix;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

struct VSInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

VSOutput ComboAuraVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 0.0, 1.0), uTransformMatrix);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord.xy;
    return output;
}

float4 ComboAuraPS(VSOutput input) : COLOR0
{
    float4 baseTex = tex2D(uImage0, input.TexCoord);

    float2 centred = input.TexCoord - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    float expand = saturate(uPhase);

    // Concentric destiny rings
    float ringRadius = expand * 0.8;
    float rings = sin(dist * 20.0 - uTime * 5.0) * 0.5 + 0.5;
    rings = pow(rings, 5.0);
    rings *= saturate(1.0 - abs(dist - ringRadius) / 0.2);

    // Angular glyph segments (8-fold symmetry)
    float glyphAngle = cos(angle * 4.0) * 0.5 + 0.5;
    glyphAngle = pow(glyphAngle, 3.0);

    // Core void glow
    float core = saturate(1.0 - dist / max(expand * 0.3, 0.01));
    core = core * core;

    // Outer fire ring
    float outerRing = abs(dist - expand * 0.9);
    float fireRing = saturate(1.0 - outerRing / 0.08);
    fireRing = fireRing * fireRing;

    // Noise for organic feel
    float2 noiseUV = float2(angle * 0.318 + uTime * 0.15, dist * uNoiseScale);
    float noise = HashNoise(noiseUV * 4.0);

    // Color: void core → crimson fire → purple outer → silver ring edge
    float3 voidCore = float3(0.06, 0.02, 0.08);
    float3 silverEdge = float3(0.78, 0.82, 0.94);

    float3 color = voidCore;
    color = lerp(color, uColor, rings * 0.7 + core * 0.5);
    color = lerp(color, uSecondaryColor, glyphAngle * (1.0 - dist) * 0.4);
    color += silverEdge * fireRing * 0.6;
    color *= noise * 0.3 + 0.7;

    float alpha = (core * 0.4 + rings * 0.3 + fireRing * 0.2 + glyphAngle * (1.0 - dist) * 0.1);
    alpha *= expand * uOpacity * input.Color.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ComboAuraMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 ComboAuraVS();
        PixelShader = compile ps_3_0 ComboAuraPS();
    }
}
