// =============================================================================
// Requiem of Reality — Swing Trail Shader
// =============================================================================
// Cosmic funeral march swing arc. Deep void base with bright crimson fire
// erupting along the arc, pink constellation sparks at the trailing edge.
// Two techniques: RequiemSwingMain (core trail) + RequiemSwingGlow (wide bloom).
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: BrightCrimson
float3 uSecondaryColor;  // Secondary: DarkPink
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uPhase;            // Combo intensity (0..1)
float uHasSecondaryTex;
float uSecondaryTexScale;

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

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }

// Main swing trail: cosmic fire
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

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

    // Leading edge hotspot (newest points brighter)
    float leading = saturate(1.0 - progress * 2.0);
    leading = leading * leading;

    // Constellation sparks at trailing edge
    float2 sparkUV = coords * float2(30.0, 8.0) + float2(uTime * 2.0, 0.0);
    float spark = HashNoise(sparkUV);
    spark = step(0.94, spark) * body * progress;

    // Combo intensity scales fire + width
    float combo = saturate(uPhase);
    float comboFire = fire * (0.6 + combo * 0.4);

    // Secondary texture detail
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.4, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.25);

    // Color: void base → crimson fire → pink constellation → white-hot core
    float3 voidCol = float3(0.06, 0.02, 0.08);
    float3 fireCol = uColor;
    float3 pinkCol = uSecondaryColor;
    float3 whiteHot = float3(1.0, 0.95, 0.92);
    float3 sparkCol = float3(0.78, 0.82, 0.94);

    float3 color = lerp(voidCol, fireCol, body * comboFire);
    color = lerp(color, pinkCol, body * (1.0 - comboFire) * 0.4);
    color = lerp(color, whiteHot, core * leading * 0.8);
    color += sparkCol * spark * 2.0;
    color *= detail;

    float alpha = (body * 0.5 + core * 0.4 + spark * 0.1) * (1.0 - progress * 0.4);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow underlayer
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = saturate(1.0 - cross);
    glow = glow * glow * glow;

    float pulse = sin(uTime * 3.0 + progress * 8.0) * 0.15 + 0.85;

    float3 glowColor = lerp(float3(0.06, 0.02, 0.08), uColor * 0.5, glow);
    float alpha = glow * (1.0 - progress * 0.5) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.5;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique RequiemSwingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique RequiemSwingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
