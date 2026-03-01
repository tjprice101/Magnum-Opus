// =============================================================================
// Fang of the Infinite Bell  EArcane Orb Trail Shader (Enhanced)
// =============================================================================
// Mystical orb trail for homing projectiles. Unlike DualFated's raw fire,
// this is ARCANE energy  Eswirling vortex currents with crystalline fractal
// shimmer, bifurcated dual-stream wake, and phosphorescent afterglow.
// The orb's inner energy rotates like a contained magical storm.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float FBM(float2 uv)
{
    float v = 0.0; float a = 0.5; float2 p = uv;
    v += SmoothNoise(p) * a; p *= 2.07; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.03; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.01; a *= 0.5;
    v += SmoothNoise(p) * a;
    return v;
}

float4 ArcaneOrbTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Cross-section: soft arcane glow with double-peaked bifurcation ---
    float center = abs(coords.y - 0.5) * 2.0;
    float softGlow = exp(-center * center * 4.0);          // Wide gaussian
    float coreBeam = exp(-center * center * 18.0);          // Tight bright core

    // Bifurcated dual-stream: two parallel energy filaments
    float stream1 = exp(-pow((coords.y - 0.35) * 6.0, 2.0));
    float stream2 = exp(-pow((coords.y - 0.65) * 6.0, 2.0));
    float dualStream = (stream1 + stream2) * 0.5;

    // Blend between single core and dual stream based on trail position
    float streamMix = saturate(coords.x * 3.0 - 0.3);  // Bifurcates further back
    float profile = lerp(coreBeam, dualStream, streamMix * 0.4) + softGlow * 0.3;

    // --- Counter-rotating vortex energy ---
    float2 vortex1UV = coords * float2(uNoiseScale * 2.0, 3.0);
    vortex1UV.x -= uTime * uScrollSpeed * 0.7;
    vortex1UV.y += sin(coords.x * 5.0 + uTime * 2.5) * 0.10;

    float2 vortex2UV = coords * float2(uNoiseScale * 1.5, 2.5);
    vortex2UV.x += uTime * uScrollSpeed * 0.35;
    vortex2UV.y -= cos(coords.x * 4.0 - uTime * 1.8) * 0.08;

    float vortex1 = FBM(vortex1UV + float2(uTime * 0.4, 0));
    float vortex2 = FBM(vortex2UV + float2(0, uTime * 0.3));
    float vortexEnergy = saturate(vortex1 * 0.55 + vortex2 * 0.55 - 0.05);

    // --- Crystalline fractal shimmer (sharp micro-sparkles) ---
    float2 crystalUV = coords * float2(25.0, 12.0) + uTime * float2(1.5, 0.5);
    float crystal = HashNoise(crystalUV);
    float crystalSpark = pow(crystal, 12.0) * softGlow * 3.0;  // Very sharp, rare sparkles

    // Fractal edge pattern
    float fractalEdge = abs(vortex1 - vortex2);
    fractalEdge = pow(fractalEdge, 0.5) * softGlow;

    // --- Secondary noise texture ---
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, 0.5 + noiseTex.r * 0.7, uHasSecondaryTex);
    vortexEnergy *= texDetail;

    // --- Arcane color gradient (distinct from fire  Emore violet-amber-gold) ---
    float3 voidBase = uColor * 0.25;
    float3 arcaneAmber = uColor;
    float3 brightGold = uSecondaryColor;
    float3 arcaneWhite = float3(1.0, 0.95, 0.85);
    float3 crystalHighlight = float3(1.0, 0.9, 0.65);

    float t = saturate(profile * vortexEnergy * 1.6);
    float3 color = lerp(voidBase, arcaneAmber, saturate(t * 2.5));
    color = lerp(color, brightGold, saturate(t * 2.0 - 0.4));
    color = lerp(color, arcaneWhite, coreBeam * saturate(t - 0.3) * 0.6);

    // Fractal edges glow brighter
    color += arcaneAmber * fractalEdge * 0.4;

    // Crystal sparkles
    color += crystalHighlight * crystalSpark;

    // --- Phosphorescent afterglow at trail tail ---
    float afterglow = saturate(coords.x * 1.5 - 0.3) * softGlow;
    color += voidBase * afterglow * 0.3 * sin(uTime * 2.0 + coords.x * 4.0) * 0.5 + 0.5;

    // --- Pulsing arcane heartbeat (slower than fire flicker) ---
    float heartbeat = sin(uTime * 2.5) * 0.12 + 0.88;
    float microPulse = sin(uTime * 11.0 + coords.x * 7.0) * 0.04 + 0.96;

    // --- Trail fade with arcane lingering ---
    float trailFade = saturate(coords.x * 6.0);              // Quick onset
    trailFade *= exp(-coords.x * 1.0);                        // Slower decay than fire
    trailFade *= 1.0 + crystalSpark * 0.15;

    float alpha = profile * trailFade * vortexEnergy * uOpacity * baseTex.a;
    alpha += crystalSpark * 0.15;
    float3 finalColor = color * uIntensity * heartbeat * microPulse * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha) * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 ArcaneOrbTrailPS();
    }
}
