// ══════════════════════════════════════════════════════════
// PrimaveraBloomAura.fx — Vivaldi's Spring boss aura
// Phase-reactive bloom with harmonic wave layers, 
// multi-ring pulsing, musical petal shapes, and 
// rich inner-to-outer color gradation.
// Parameters set by BossRenderHelper.DrawShaderAura()
// ══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Phase outer color (CherryPink / FreshGreen / HotPink / ViolentMagenta)
float4 uSecondaryColor;  // Phase accent color (Lavender / CherryPink / CherryPink / DeepCrimson)
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
    for (int i = 0; i < 4; i++)
    {
        v += a * noise(p);
        p = p * 2.0 + shift;
        a *= 0.5;
    }
    return v;
}

float4 PS_BloomAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float radiusNorm = uRadius / 200.0;

    // === HARMONIC BREATHING — musical: 3 layered sine waves like overtones ===
    float fundamental = sin(uTime * 2.0) * 0.06;
    float second = sin(uTime * 4.0) * 0.03;
    float third = sin(uTime * 6.0) * 0.015;
    float breathe = 1.0 + fundamental + second + third;
    float effectRadius = radiusNorm * breathe;

    // Base radial falloff — softer inner, sharper outer
    float innerFalloff = exp(-dist * dist / (effectRadius * effectRadius * 0.5));
    float outerFalloff = 1.0 - smoothstep(0.0, effectRadius * 1.2, dist);
    float falloff = innerFalloff * 0.6 + outerFalloff * 0.4;

    // === PETAL LAYER 1: 5-fold flower (cherry blossom shape) ===
    float petal1Angle = angle + uTime * 0.3;
    float petal1 = cos(petal1Angle * 2.5) * 0.5 + 0.5;
    petal1 = smoothstep(0.25, 0.85, petal1);
    float petal1Mask = petal1 * smoothstep(effectRadius, 0.0, dist);

    // === PETAL LAYER 2: counter-rotating 3-fold (musical contrast) ===
    float petal2Angle = angle - uTime * 0.4 + 0.5;
    float petal2 = cos(petal2Angle * 1.5) * 0.5 + 0.5;
    petal2 = smoothstep(0.3, 0.75, petal2);
    float petal2Mask = petal2 * smoothstep(effectRadius * 0.7, 0.0, dist) * 0.5;

    // === VINE NOISE: organic growth ===
    float vineSpeed = uTime * 0.5;
    float vine = fbm(float2(angle * 2.5 + vineSpeed, dist * 8.0 - vineSpeed * 0.8));
    float vinePattern = smoothstep(0.35, 0.65, vine) * falloff;

    // === HARMONIC RINGS: standing wave ripples (musical!) ===
    float wave1 = sin(dist * 40.0 - uTime * 3.0) * 0.5 + 0.5;
    float wave2 = sin(dist * 25.0 - uTime * 2.0 + 1.0) * 0.5 + 0.5;
    float ringMask = smoothstep(0.6, 0.9, wave1 * wave2) * falloff * 0.3;

    // === POLLEN SPARKLES: drifting upward ===
    float2 pollenUV = float2(uv.x * 30.0, uv.y * 35.0 - uTime * 1.2);
    float pollenSeed = hash(floor(pollenUV));
    float pollen = step(0.96, pollenSeed);
    float pollenPulse = sin(uTime * 5.0 + pollenSeed * 40.0) * 0.35 + 0.65;
    float pollenGlow = pollen * pollenPulse * falloff;

    // === COLOR BLENDING: primary -> secondary with petal modulation ===
    float colorMix = petal1Mask * vinePattern;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);

    // Hot white-ish core
    float coreHeat = exp(-dist * dist / (effectRadius * effectRadius * 0.08));
    float4 coreColor = float4(1.0, 0.97, 0.92, 1.0);

    // Pollen highlight — warm yellow-white
    float4 pollenColor = float4(1.0, 0.95, 0.8, 1.0);

    // Combine layers
    float alpha = (petal1Mask * 0.35 + petal2Mask * 0.2 + vinePattern * 0.25 +
                   ringMask + pollenGlow * 0.3 + coreHeat * 0.4) * uIntensity;

    float4 color = baseColor * (petal1Mask * 0.35 + petal2Mask * 0.2 + vinePattern * 0.25);
    color += baseColor * ringMask;
    color += coreColor * coreHeat * uIntensity * 0.5;
    color += pollenColor * pollenGlow * uIntensity * 0.4;

    color *= uIntensity;
    color.a = saturate(alpha);

    return color;
}

technique Technique1
{
    pass BloomAura
    {
        PixelShader = compile ps_3_0 PS_BloomAura();
    }
}
