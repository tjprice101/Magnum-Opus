// SwanFractalBeam.fx -- Swan Lake MonochromaticApocalypse beam (4-phase aware)
//
// Phase 1 (White Swan):   Pure white geometric beam. Hard smoothstep edges. Fractal interference interior.
// Phase 2 (Black Swan):   White body, black edge corona. Prismatic line at dark/light boundary.
// Phase 3 (Duality War):  Segments alternate white/black along length. Prismatic at each boundary.
// Phase 4 (Death of Swan): Drains to gray. Structure dissolves. Edges soften. Intermittent prismatic.

sampler uImage0 : register(s0);

float4 uColor;
float uIntensity;
float uTime;
float uPhase;  // 1-4
float uDrain;  // 0-1

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_FractalBeam(float2 uv : TEXCOORD0) : COLOR0
{
    float beamCenter = abs(uv.y - 0.5) * 2.0;

    // Palette constants
    float3 pureWhite = float3(0.96, 0.96, 1.0);
    float3 voidBlack = float3(0.02, 0.02, 0.03);
    float3 deadGray  = float3(0.45, 0.45, 0.46);

    // Hard-edged beam mask (geometric, not soft bloom)
    float beamMask = smoothstep(1.0, 0.85, beamCenter);
    float beamHard = smoothstep(1.0, 0.92, beamCenter);

    // Internal fractal interference -- overlapping sine waves
    float f1 = sin(uv.x * 20.0 + uv.y * 15.0 - uTime * 5.0);
    float f2 = sin(uv.x * 13.0 - uv.y * 11.0 + uTime * 3.0);
    float fractal = f1 * f2 * 0.5 + 0.5;

    float scroll = frac(uv.x * 3.0 - uTime * 2.0);

    float3 color = pureWhite;
    float alpha = 0.0;

    if (uPhase < 1.5)
    {
        // Phase 1: Pure white geometric beam, hard edges, fractal interior
        color = pureWhite * (fractal * 0.3 + 0.7);
        alpha = beamHard * uIntensity * (scroll * 0.2 + 0.8);
    }
    else if (uPhase < 2.5)
    {
        // Phase 2: White body, black edge corona, prismatic at boundary
        float corona = smoothstep(0.7, 1.0, beamCenter) * (1.0 - smoothstep(1.0, 1.15, beamCenter));
        float3 body = pureWhite * (fractal * 0.3 + 0.7);

        // Boundary zone between white core and black halo
        float boundaryZone = smoothstep(0.65, 0.8, beamCenter) * (1.0 - smoothstep(0.8, 0.95, beamCenter));
        float hue = frac(uv.x * 1.5 + uTime * 0.2);
        float3 rainbow = hsl2rgb(hue, 0.95, 0.65);

        color = lerp(body, voidBlack, corona);
        color = lerp(color, rainbow, boundaryZone * 0.8);

        alpha = (beamMask + corona * 0.6) * uIntensity * (scroll * 0.2 + 0.8);
    }
    else if (uPhase < 3.5)
    {
        // Phase 3: Alternating white/black segments along beam length
        float segVal = frac(uv.x * 5.0 + uTime * 3.0);
        float segment = step(0.5, segVal);
        float3 segColor = lerp(pureWhite, voidBlack, segment);

        // Prismatic line at each black/white boundary
        float nearHalf = abs(segVal - 0.5);
        float nearEdge = min(segVal, 1.0 - segVal);
        float boundDist = min(nearHalf, nearEdge);
        float boundary = 1.0 - smoothstep(0.0, 0.08, boundDist);

        float hue = frac(uv.x * 2.0 + uTime * 0.4);
        float3 rainbow = hsl2rgb(hue, 1.0, 0.7);

        color = segColor * (fractal * 0.3 + 0.7);
        color = lerp(color, rainbow, boundary * 0.9);

        alpha = beamHard * uIntensity;
    }
    else
    {
        // Phase 4: Drain to gray. Edges soften. Structure dissolves.
        float softBeam = smoothstep(1.0, 0.6, beamCenter);
        float3 drainedColor = lerp(pureWhite, deadGray, uDrain);
        float flatFractal = lerp(fractal, 0.5, uDrain * 0.8);

        color = drainedColor * (flatFractal * 0.2 + 0.8);

        // Intermittent prismatic at edges
        float flicker = sin(uTime * 10.0) * sin(uTime * 6.7);
        flicker = smoothstep(0.6, 1.0, flicker);
        float edgeZone = smoothstep(0.5, 0.8, beamCenter) * (1.0 - smoothstep(0.8, 1.0, beamCenter));
        float hue = frac(uv.x + uTime * 0.15);
        float3 rainbow = hsl2rgb(hue, 0.7, 0.6);
        color = lerp(color, rainbow, edgeZone * flicker * (1.0 - uDrain * 0.6));

        alpha = softBeam * uIntensity * (1.0 - uDrain * 0.9);
    }

    return float4(color, 1) * saturate(alpha);
}

technique Technique1
{
    pass FractalBeam { PixelShader = compile ps_3_0 PS_FractalBeam(); }
}
