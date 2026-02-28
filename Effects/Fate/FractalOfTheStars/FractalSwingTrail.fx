// =============================================================================
// Fractal of the Stars — Swing Trail Shader
// =============================================================================
// Star-themed cosmic swing arc with constellation patterns and golden fire.
// Deep void base → purple star nebula → gold star fire → white supernova core.
// Two techniques: FractalSwingMain (core trail) + FractalSwingGlow (wide bloom).
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: StarGold
float3 uSecondaryColor;  // Secondary: FractalPurple
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

// Main swing trail: star-fire with constellation sparkle
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;  // 0=newest, 1=oldest
    float cross = abs(coords.y - 0.5) * 2.0;

    // Scrolling stellar turbulence
    float2 fireUV = float2(progress * uNoiseScale - uTime * uScrollSpeed, coords.y * 3.0);
    float fire1 = SmoothNoise(fireUV * 4.0);
    float fire2 = SmoothNoise(fireUV * 9.0 + 3.14);
    float fire = fire1 * 0.55 + fire2 * 0.45;

    // Core: hot centre, tapers to edges
    float core = saturate(1.0 - cross / 0.3);
    core = core * core;

    // Body: wider glow region
    float body = saturate(1.0 - cross);
    body = sqrt(body);

    // Leading edge hotspot (newest points brighter)
    float leading = saturate(1.0 - progress * 2.0);
    leading = leading * leading;

    // Constellation star points at intervals along the trail
    float2 starUV = coords * float2(20.0, 6.0) + float2(uTime * 1.5, 0.0);
    float star = HashNoise(starUV);
    star = step(0.92, star) * body;

    // Star node glow at constellation points
    float2 nodeUV = coords * float2(8.0, 3.0);
    float node = HashNoise(floor(nodeUV));
    node = step(0.85, node) * saturate(1.0 - cross * 1.5);
    float nodePulse = sin(uTime * 4.0 + node * 20.0) * 0.3 + 0.7;
    node *= nodePulse;

    // Combo intensity scales fire
    float combo = saturate(uPhase);
    float comboFire = fire * (0.5 + combo * 0.5);

    // Secondary texture detail
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.3, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.25);

    // Color: void → purple → gold fire → white supernova
    float3 voidCol = float3(0.04, 0.02, 0.10);
    float3 purpleCol = uSecondaryColor;
    float3 goldCol = uColor;
    float3 whiteHot = float3(1.0, 1.0, 0.94);
    float3 starCol = float3(0.86, 0.90, 1.0);

    float3 color = lerp(voidCol, purpleCol, body * 0.6);
    color = lerp(color, goldCol, body * comboFire);
    color = lerp(color, whiteHot, core * leading * 0.7);
    color += starCol * star * 2.5;
    color += goldCol * node * 1.5;
    color *= detail;

    float alpha = (body * 0.45 + core * 0.4 + star * 0.1 + node * 0.05) * (1.0 - progress * 0.4);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow underlayer with stellar purple haze
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = saturate(1.0 - cross);
    glow = glow * glow * glow;

    float pulse = sin(uTime * 2.5 + progress * 6.0) * 0.15 + 0.85;

    // Subtle star shimmer in the glow
    float shimmer = SmoothNoise(coords * float2(15.0, 5.0) + uTime * 0.5) * 0.2;

    float3 glowColor = lerp(float3(0.04, 0.02, 0.10), uSecondaryColor * 0.5, glow);
    glowColor += float3(1.0, 0.78, 0.2) * shimmer * glow; // Gold shimmer in glow
    float alpha = glow * (1.0 - progress * 0.5) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.5;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique FractalSwingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique FractalSwingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
