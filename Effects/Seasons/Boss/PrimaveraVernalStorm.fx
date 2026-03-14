// ══════════════════════════════════════════════════════════
// PrimaveraVernalStorm.fx — Vivaldi's Spring phase transition
// Dramatic spring storm: spiraling vortex carrying petals,
// diagonal rain streaks, phase color sweep, and
// lightning-like energy flashes across the screen.
// Parameters set by BossRenderHelper.DrawPhaseTransition()
// ══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float fbm(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    float2 shift = float2(100.0, 100.0);
    for (int i = 0; i < 3; i++)
    {
        v += a * noise(p);
        p = p * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

float4 PS_VernalStorm(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // === STORM VORTEX: spiraling wind pattern ===
    float swirlAngle = angle - dist * 10.0 + uTime * 3.5;
    float swirl = sin(swirlAngle * 5.0) * 0.5 + 0.5;
    float swirlMask = swirl * smoothstep(0.65, 0.08, dist);

    // Second layer — tighter, faster inner swirl
    float swirl2Angle = angle + dist * 14.0 - uTime * 5.0;
    float swirl2 = sin(swirl2Angle * 3.0) * 0.5 + 0.5;
    float swirl2Mask = swirl2 * smoothstep(0.4, 0.05, dist) * 0.4;

    // === PETAL SCATTER: wind-driven cherry blossoms ===
    float2 petalUV = float2(
        uv.x + sin(uv.y * 10.0 + uTime * 3.0) * 0.08,
        uv.y - uTime * 0.8 + cos(uv.x * 6.0 + uTime * 2.0) * 0.04);
    float petalNoise = noise(petalUV * 14.0);
    float petals = smoothstep(0.72, 0.88, petalNoise);

    // Larger floating petals (slower, more visible)
    float2 bigPetalUV = float2(
        uv.x + sin(uv.y * 4.0 + uTime * 1.5) * 0.12,
        uv.y - uTime * 0.4);
    float bigPetal = noise(bigPetalUV * 6.0);
    float bigPetals = smoothstep(0.78, 0.9, bigPetal) * 0.7;

    // === PHASE TRANSITION WAVE: sweeps across screen ===
    float waveNoise = noise(float2(uv.y * 6.0, uTime * 0.5)) * 0.12;
    float transWave = uv.x + waveNoise;
    float phaseMix = smoothstep(0.35, 0.65, uTransitionProgress + (1.0 - transWave) * 0.35);

    // === RAIN STREAKS: diagonal spring rain ===
    float rainUV = uv.x * 2.0 + uv.y * 18.0 - uTime * 10.0;
    float rain = noise(float2(rainUV, uv.y * 3.0));
    float rainStreaks = smoothstep(0.88, 0.97, rain) * 0.35;

    // Second rain layer — thinner, faster (upper harmonics)
    float rain2UV = uv.x * 3.0 + uv.y * 22.0 - uTime * 14.0;
    float rain2 = noise(float2(rain2UV, uv.y * 4.0));
    float rainStreaks2 = smoothstep(0.90, 0.98, rain2) * 0.2;

    // === LIGHTNING-LIKE FLASH: bright energy branches ===
    float lightning = fbm(float2(uv.x * 4.0 + uTime * 8.0, uv.y * 8.0));
    float lightningMask = smoothstep(0.70, 0.85, lightning) *
                          sin(uTime * 15.0 + hash(floor(uv * 5.0)) * 20.0);
    lightningMask = max(0.0, lightningMask) * 0.4;

    // === HARMONIC RING: expanding circular wave from center ===
    float ringPos = frac(uTime * 0.8) * 0.7;
    float harmonicRing = smoothstep(0.02, 0.0, abs(dist - ringPos));
    harmonicRing *= smoothstep(0.7, 0.0, dist) * 0.5;

    // === COLOR COMPOSITION ===
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);
    float4 petalPink = lerp(uFromColor, float4(1.0, 0.65, 0.75, 1.0), 0.6);
    float4 rainWhite = float4(0.85, 0.92, 1.0, 1.0);
    float4 flashWhite = float4(1.0, 0.98, 0.95, 1.0);

    float4 result = base;
    // Phase color tint
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.45);
    // Storm swirl
    result.rgb += phaseColor.rgb * (swirlMask * 0.25 + swirl2Mask * 0.15) * uIntensity;
    // Petals
    result.rgb += petalPink.rgb * petals * uIntensity * 0.75;
    result.rgb += petalPink.rgb * bigPetals * uIntensity * 0.6;
    // Rain
    result.rgb += rainWhite.rgb * (rainStreaks + rainStreaks2) * uIntensity;
    // Lightning flash
    result.rgb += flashWhite.rgb * lightningMask * uIntensity;
    // Harmonic ring
    result.rgb += phaseColor.rgb * harmonicRing * uIntensity;

    return result;
}

technique Technique1
{
    pass VernalStorm
    {
        PixelShader = compile ps_3_0 PS_VernalStorm();
    }
}
