// =============================================================================
// The Conductor's Last Constellation — Swing Trail Shader
// =============================================================================
// Conductor/baton-themed cosmic swing arc with electric cyan lightning threads,
// deep void base, golden flashes, and silver star dust.
// Two techniques: ConductorSwingMain (core trail) + ConductorSwingGlow (wide bloom).
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: ConductorCyan
float3 uSecondaryColor;  // Secondary: BatonPurple
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

// Main swing trail: conductor lightning with electric cyan threads
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Scrolling electric turbulence
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

    // Leading edge hotspot
    float leading = saturate(1.0 - progress * 2.0);
    leading = leading * leading;

    // Lightning thread pattern — jagged horizontal lines
    float2 threadUV = coords * float2(30.0, 8.0) + float2(uTime * 3.0, 0.0);
    float thread = SmoothNoise(threadUV);
    float threadLine = step(0.7, thread) * body;
    float threadGlow = smoothstep(0.5, 0.8, thread) * body * 0.4;

    // Electric arc nodes
    float2 nodeUV = coords * float2(10.0, 4.0);
    float node = HashNoise(floor(nodeUV));
    node = step(0.82, node) * saturate(1.0 - cross * 1.5);
    float nodePulse = sin(uTime * 5.0 + node * 25.0) * 0.3 + 0.7;
    node *= nodePulse;

    // Combo intensity scales fire
    float combo = saturate(uPhase);
    float comboFire = fire * (0.5 + combo * 0.5);

    // Secondary texture detail
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.3, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.25);

    // Color: void → purple → cyan conductor → gold lightning → white celestial
    float3 voidCol = float3(0.03, 0.02, 0.06);
    float3 purpleCol = uSecondaryColor;
    float3 cyanCol = uColor;
    float3 goldCol = float3(1.0, 0.86, 0.31);
    float3 whiteHot = float3(0.94, 0.96, 1.0);

    float3 color = lerp(voidCol, purpleCol, body * 0.5);
    color = lerp(color, cyanCol, body * comboFire);
    color = lerp(color, goldCol, core * leading * 0.5);
    color = lerp(color, whiteHot, core * leading * 0.3);
    color += cyanCol * threadLine * 2.0;
    color += goldCol * threadGlow;
    color += whiteHot * node * 1.5;
    color *= detail;

    float alpha = (body * 0.45 + core * 0.4 + threadLine * 0.1 + node * 0.05) * (1.0 - progress * 0.4);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow underlayer with conductor purple-cyan haze
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = saturate(1.0 - cross);
    glow = glow * glow * glow;

    float pulse = sin(uTime * 2.5 + progress * 6.0) * 0.15 + 0.85;

    // Electric shimmer in the glow
    float shimmer = SmoothNoise(coords * float2(15.0, 5.0) + uTime * 0.5) * 0.2;

    float3 glowColor = lerp(float3(0.03, 0.02, 0.06), uSecondaryColor * 0.5, glow);
    glowColor += float3(0.16, 0.78, 0.86) * shimmer * glow; // Cyan shimmer
    float alpha = glow * (1.0 - progress * 0.5) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.5;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique ConductorSwingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique ConductorSwingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
