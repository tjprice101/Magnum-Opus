// SwanPrismaticAura.fx -- Swan Lake boss presence aura (4-phase aware)
//
// Phase 1 (White Swan):   Clean geometric concentric waves. Pure white on void black. No rainbow.
// Phase 2 (Black Swan):   Aura CRACKS. Fracture lines appear. Prismatic bleed ONLY at crack seams. Black counter-pattern.
// Phase 3 (Duality War):  Rapid black/white alternation. Pulse-swapping. Intensified fractures.
// Phase 4 (Death of Swan): Color drains to gray via uDrain. Ghostly remnant. Rare prismatic flicker.

sampler uImage0 : register(s0);

float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // White (0.96, 0.96, 1.0)
float4 uSecondaryColor;  // Black (0.02, 0.02, 0.03)
float uTime;
float uPhase;  // 1-4
float uDrain;  // 0-1 (color drain in Phase 4)

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_PrismaticAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float radiusNorm = uRadius / 200.0;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Palette constants
    float3 pureWhite = float3(0.96, 0.96, 1.0);
    float3 voidBlack = float3(0.02, 0.02, 0.03);
    float3 deadGray  = float3(0.45, 0.45, 0.46);

    // Geometric wave pattern -- ballet-like flowing arcs with sharp edges
    float wave1 = sin(angle * 4.0 + dist * 20.0 - uTime * 3.0);
    float wave2 = sin(angle * 6.0 - dist * 15.0 + uTime * 2.0);
    float wave = smoothstep(-0.2, 0.2, wave1) * 0.6 + smoothstep(-0.3, 0.3, wave2) * 0.4;

    // Fracture pattern -- glass-crack lines (emerge Phase 2+)
    float crack1 = sin(angle * 11.0 + dist * 30.0 + uTime * 1.5);
    float crack2 = sin(angle * 7.0 - dist * 25.0 - uTime * 2.2);
    float fractureLine = abs(crack1 * crack2);
    fractureLine = 1.0 - smoothstep(0.0, 0.08, fractureLine);

    float3 color = pureWhite;
    float alpha = 0.0;

    if (uPhase < 1.5)
    {
        // Phase 1: Clean white geometric waves -- no rainbow, no fractures
        color = pureWhite * wave;
        alpha = falloff * wave * uIntensity;
    }
    else if (uPhase < 2.5)
    {
        // Phase 2: Cracks appear, black counter-pattern, prismatic at seams
        float3 whiteWave = pureWhite * wave;
        float3 blackWave = voidBlack * (1.0 - wave);
        color = lerp(whiteWave, blackWave, (1.0 - wave) * 0.6);

        // Prismatic ONLY at fracture seams
        float hue = frac(angle / 6.283 + dist * 2.0 + uTime * 0.15);
        float3 rainbow = hsl2rgb(hue, 0.9, 0.65);
        color = lerp(color, rainbow, fractureLine * 0.7);

        alpha = falloff * (wave * 0.7 + fractureLine * 0.5) * uIntensity;
    }
    else if (uPhase < 3.5)
    {
        // Phase 3: Rapid black/white alternation with intensified fractures
        float oscillation = sin(uTime * 8.0) * 0.5 + 0.5;
        float3 swapA = lerp(pureWhite, voidBlack, oscillation);
        float3 swapB = lerp(voidBlack, pureWhite, oscillation);
        color = lerp(swapA, swapB, wave);

        // Intensified prismatic at fracture lines
        float hue = frac(angle / 6.283 + uTime * 0.3);
        float3 rainbow = hsl2rgb(hue, 1.0, 0.7);
        color = lerp(color, rainbow, fractureLine * 0.9);

        alpha = falloff * (0.5 + fractureLine * 0.6) * uIntensity;
    }
    else
    {
        // Phase 4: Drain to gray. Ghostly remnant. Rare prismatic flicker.
        float3 drainedWhite = lerp(pureWhite, deadGray, uDrain);
        color = drainedWhite * wave * (1.0 - uDrain * 0.7);

        // Intermittent prismatic -- only triggers on rare sin peaks
        float flicker = sin(uTime * 12.0) * sin(uTime * 7.3);
        flicker = smoothstep(0.7, 1.0, flicker);
        float hue = frac(angle / 6.283 + uTime * 0.1);
        float3 rainbow = hsl2rgb(hue, 0.8, 0.6);
        color = lerp(color, rainbow, fractureLine * flicker * (1.0 - uDrain * 0.5));

        alpha = falloff * wave * uIntensity * (1.0 - uDrain * 0.85);
    }

    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass PrismaticAura { PixelShader = compile ps_3_0 PS_PrismaticAura(); }
}
