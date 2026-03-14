// ══════════════════════════════════════════════════════════
// PrimaveraGrowthPulse.fx — Vivaldi's Spring attack flash
// Dramatic blooming burst with multiple expanding rings,
// 5-fold flower symmetry, harmonic radial cascade, and
// bright bloom energy at the center.
// Parameters set by BossRenderHelper.DrawAttackFlash()
// ══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
float2 uCenter;
float uIntensity;
float4 uColor;
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

float4 PS_GrowthPulse(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 center = uCenter;
    // Fallback: if uCenter not set, use screen center
    if (length(center) < 0.001) center = float2(0.5, 0.5);

    float2 delta = uv - center;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // === MULTI-RING EXPANSION: 3 rings at different speeds ===
    float ringPos1 = frac(uTime * 1.2) * 0.6;
    float ring1 = smoothstep(0.025, 0.0, abs(dist - ringPos1));
    ring1 *= smoothstep(0.6, 0.0, dist);

    float ringPos2 = frac(uTime * 0.9 + 0.33) * 0.55;
    float ring2 = smoothstep(0.02, 0.0, abs(dist - ringPos2));
    ring2 *= smoothstep(0.55, 0.0, dist);

    float ringPos3 = frac(uTime * 1.5 + 0.66) * 0.5;
    float ring3 = smoothstep(0.015, 0.0, abs(dist - ringPos3));
    ring3 *= smoothstep(0.5, 0.0, dist);

    float rings = ring1 * 0.8 + ring2 * 0.5 + ring3 * 0.3;

    // === 5-FOLD FLOWER SYMMETRY: cherry blossom burst ===
    float petalAngle = angle + uTime * 1.5;
    float petals = abs(sin(petalAngle * 2.5));
    petals = pow(petals, 1.5);
    float petalShape = petals * smoothstep(0.45, 0.0, dist);

    // === SECONDARY 3-FOLD: counter-rotating accent ===
    float petal2Angle = angle - uTime * 2.0 + 1.0;
    float petal2 = abs(sin(petal2Angle * 1.5));
    petal2 = pow(petal2, 2.0);
    float petal2Shape = petal2 * smoothstep(0.3, 0.0, dist) * 0.4;

    // === RADIAL FALLOFF with exponential core ===
    float radialCore = exp(-dist * 5.0) * uIntensity;
    float radialSoft = exp(-dist * 2.5) * uIntensity * 0.5;

    // === BLOOMING NOISE: organic FBM spread ===
    float n1 = noise(float2(angle * 3.0 + uTime * 2.0, dist * 12.0 - uTime * 3.5));
    float n2 = noise(float2(angle * 5.0 - uTime * 1.5, dist * 8.0 + uTime * 2.0)) * 0.5;
    float bloomNoise = (n1 + n2) / 1.5;
    float bloomMask = smoothstep(0.3, 0.7, bloomNoise) * radialSoft;

    // === HARMONIC RADIAL RIPPLE: musical interference ===
    float harmonic = sin(dist * 50.0 - uTime * 5.0) * sin(dist * 30.0 - uTime * 3.0);
    float harmonicMask = smoothstep(0.5, 1.0, harmonic * 0.5 + 0.5) * radialSoft * 0.25;

    // === COLOR: growth green core → pink tips → white highlights ===
    float petalMix = smoothstep(0.1, 0.35, dist);
    float4 growthCore = uColor;
    float4 pinkTip = float4(1.0, 0.55, 0.7, 1.0);
    float4 whiteHot = float4(1.0, 1.0, 0.95, 1.0);

    float4 burstColor = lerp(growthCore, pinkTip, petalMix * 0.6);

    float alpha = (petalShape * 0.5 + petal2Shape +
                   bloomMask * 0.4 + rings * 0.7 + harmonicMask +
                   radialCore * 0.3) * uIntensity;

    float4 result = base;
    result.rgb += burstColor.rgb * (petalShape * 0.5 + petal2Shape + bloomMask * 0.4);
    result.rgb += burstColor.rgb * rings * 0.7;
    result.rgb += whiteHot.rgb * (rings * radialCore * 0.6 + harmonicMask);
    result.rgb += whiteHot.rgb * radialCore * 0.4;

    return result;
}

technique Technique1
{
    pass GrowthPulse
    {
        PixelShader = compile ps_3_0 PS_GrowthPulse();
    }
}
