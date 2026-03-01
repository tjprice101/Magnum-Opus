// =============================================================================
// Coda of Annihilation — Zenith Trail Shader
// =============================================================================
// DISINTEGRATING VOID TRAIL: Flying zenith swords leave trails of reality
// breaking down. Digital dissolution at edges, spectral RGB separation,
// hard void core with bleeding crimson phase-shifting.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

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

// ---- ZenithMain: Disintegrating void trail ----
float4 ZenithMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float cross = abs(coords.y - 0.5) * 2.0;
    float headiness = 1.0 - coords.x; // 1 at head, 0 at tail

    // --- Digital dissolution: pixelated breakup that increases toward tail ---
    float pixelScale = lerp(60.0, 12.0, coords.x); // fine at head, coarse at tail
    float2 pixelUV = floor(coords * pixelScale) / pixelScale;
    float dissolveNoise = HashNoise(pixelUV * 7.0 + floor(uTime * 3.0));
    float dissolveThreshold = coords.x * 0.8 + cross * 0.3;
    float dissolved = step(dissolveThreshold, dissolveNoise);

    // --- Void core with crimson bleed ---
    float coreStrip = smoothstep(0.3, 0.0, cross);
    float voidDepth = coreStrip * headiness;

    // --- Spectral RGB separation at edges ---
    float separation = cross * 0.015 * (1.0 + sin(uTime * 6.0) * 0.3);
    float rChan = tex2D(uImage0, coords + float2(separation, 0)).a;
    float gChan = baseTex.a;
    float bChan = tex2D(uImage0, coords - float2(separation, 0)).a;
    float3 spectral = float3(rChan, gChan, bChan);

    // --- Phase-shifting shimmer: the trail flickers between states ---
    float phase = sin(coords.x * 30.0 - uTime * 8.0) * 0.5 + 0.5;
    float flicker = smoothstep(0.3, 0.7, SmoothNoise(coords * float2(15.0, 4.0) - uTime * 2.0));

    // --- Trailing energy wisps ---
    float2 wispUV = coords * float2(6.0, 3.0) - float2(uTime * 1.5, 0);
    float wisp = SmoothNoise(wispUV * 4.0);
    wisp = smoothstep(0.55, 0.75, wisp) * (1.0 - cross) * coords.x;

    // --- Color composition ---
    float3 voidBlack = float3(0.015, 0.005, 0.025);
    float3 crimsonBleed = uColor * 1.5;
    float3 pinkEdge = uSecondaryColor;
    float3 annihilWhite = float3(1.0, 0.9, 0.95);

    float3 color = voidBlack;
    color = lerp(color, crimsonBleed, voidDepth * (0.5 + phase * 0.5));
    color = lerp(color, pinkEdge, wisp * 0.8);
    color += annihilWhite * flicker * coreStrip * 0.3;
    color *= spectral; // chromatic separation

    // --- Edge fade with dissolution ---
    float edgeFade = 1.0 - smoothstep(0.75, 1.0, cross);
    float lengthFade = headiness;

    float alpha = edgeFade * lengthFade * dissolved;
    alpha *= uOpacity * uIntensity * baseTex.a;
    alpha *= saturate(0.4 + coreStrip * 0.4 + wisp * 0.2);

    return float4(color * alpha, alpha);
}

// ---- ZenithGlow: Bleeding void aura around the dissolving trail ----
float4 ZenithGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float cross = abs(coords.y - 0.5) * 2.0;
    float headiness = 1.0 - coords.x;

    // Wider, softer glow that pulses with annihilation energy
    float glow = exp(-cross * cross * 3.0);
    float pulse = sin(uTime * 5.0 + coords.x * 12.0) * 0.2 + 0.8;

    // Void shimmer: dark purple that brightens at cracks
    float shimmer = SmoothNoise(coords * float2(10.0, 3.0) + uTime * 0.8);
    shimmer = smoothstep(0.4, 0.7, shimmer);

    float3 glowColor = lerp(float3(0.03, 0.01, 0.05), uSecondaryColor * 0.4, glow * 0.6);
    glowColor += uColor * shimmer * glow * 0.3;

    float alpha = glow * headiness * uOpacity * pulse * 0.45;

    return float4(glowColor * alpha, alpha);
}

technique ZenithMain
{
    pass ZenithMainPass
    {
        PixelShader = compile ps_3_0 ZenithMainPS();
    }
}

technique ZenithGlow
{
    pass ZenithGlowPass
    {
        PixelShader = compile ps_3_0 ZenithGlowPS();
    }
}
