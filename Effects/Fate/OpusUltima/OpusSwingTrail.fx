// =============================================================================
// Opus Ultima — Swing Trail Shader
// =============================================================================
// The Magnum Opus swing arc. Deep void base with crimson fire erupting along
// the arc, golden glory highlights at the leading edge, cosmic rose accents.
// Two techniques: OpusSwingMain (core trail) + OpusSwingGlow (wide bloom).
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: OpusCrimson
float3 uSecondaryColor;  // Secondary: GloryGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uPhase;            // Combo intensity (0..1)
float uHasSecondaryTex;
float uSecondaryTexScale;
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

// Main swing trail: cosmic fire with golden glory
float4 SwingMainPS(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float progress = coords.x;  // 0=newest, 1=oldest
    float cross = abs(coords.y - 0.5) * 2.0;

    // Scrolling fire turbulence
    float2 fireUV = float2(progress * uNoiseScale - uTime * uScrollSpeed, coords.y * 3.0);
    float fire1 = SmoothNoise(fireUV * 5.0);
    float fire2 = SmoothNoise(fireUV * 10.0 + 2.7);
    float fire = fire1 * 0.6 + fire2 * 0.4;

    // Core: hot centre, tapers to edges
    float core = saturate(1.0 - cross / 0.35);
    core = core * core;

    // Body: wider glow region
    float body = saturate(1.0 - cross);
    body = sqrt(body);

    // Leading edge hotspot
    float leading = saturate(1.0 - progress * 2.0);
    leading = leading * leading;

    // Golden constellation sparks at trailing edge
    float2 sparkUV = coords * float2(30.0, 8.0) + float2(uTime * 2.0, 0.0);
    float spark = HashNoise(sparkUV);
    spark = step(0.93, spark) * body * progress;

    // Combo intensity scales fire + width
    float combo = saturate(uPhase);
    float comboFire = fire * (0.6 + combo * 0.4);

    // Secondary texture detail
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.4, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.25);

    // Color: void → crimson → gold → white-hot core
    float3 voidCol = float3(0.047, 0.02, 0.07);
    float3 fireCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 whiteHot = float3(0.96, 0.94, 1.0);
    float3 sparkCol = float3(1.0, 0.75, 0.16);

    float3 color = lerp(voidCol, fireCol, body * comboFire);
    color = lerp(color, goldCol, core * leading * 0.6 * combo);
    color = lerp(color, whiteHot, core * leading * 0.8);
    color += sparkCol * spark * 2.5;
    color *= detail;

    float alpha = (body * 0.5 + core * 0.4 + spark * 0.1) * (1.0 - progress * 0.4);
    alpha *= uOpacity * input.Color.a;
    float3 finalColor = color * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow underlayer with crimson-gold pulsing
float4 SwingGlowPS(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = saturate(1.0 - cross);
    glow = glow * glow * glow;

    float pulse = sin(uTime * 3.0 + progress * 8.0) * 0.15 + 0.85;
    float combo = saturate(uPhase);

    float3 glowColor = lerp(float3(0.047, 0.02, 0.07), uColor * 0.5, glow);
    glowColor = lerp(glowColor, uSecondaryColor * 0.3, combo * glow * 0.4);

    float alpha = glow * (1.0 - progress * 0.5) * uOpacity * input.Color.a * pulse * 0.5;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique OpusSwingMain
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique OpusSwingGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
