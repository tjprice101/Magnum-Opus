// =============================================================================
// Requiem of Reality — Swing Trail Shader
// =============================================================================
// INK DISSOLUTION DIRGE: The Requiem's swing feels like dark ink spreading
// through water — flowing, heavy, mournful. Domain-warped FBM creates organic
// fluid tendrils. Music staff lines ghost through the dissolution. The funeral
// march made visible: slow, dark, inevitable.
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

// Domain-warped FBM for organic ink tendrils
float InkFBM(float2 p)
{
    // First pass: warp the domain
    float2 warp = float2(
        SmoothNoise(p + float2(0.0, uTime * 0.3)),
        SmoothNoise(p + float2(5.2, uTime * 0.25 + 1.3))
    );
    p += warp * 1.2;

    // Second pass: domain warp again for deeper organic feel
    float2 warp2 = float2(
        SmoothNoise(p + float2(1.7, uTime * 0.15 + 9.2)),
        SmoothNoise(p + float2(8.3, uTime * 0.2 + 2.8))
    );
    p += warp2 * 0.8;

    // FBM accumulation
    float val = 0.0;
    float amp = 0.5;
    float freq = 1.0;
    [unroll] for (int i = 0; i < 4; i++)
    {
        val += amp * SmoothNoise(p * freq);
        freq *= 2.17;
        amp *= 0.49;
    }
    return val;
}

// Music staff lines: faint horizontal ruled lines in the ink
float StaffLines(float2 coords)
{
    // 5 staff lines across the trail width
    float lineSpacing = 0.2;
    float lineY = frac(coords.y / lineSpacing);
    float staffLine = smoothstep(0.02, 0.0, abs(lineY - 0.5));
    // Lines distort slightly with ink flow
    float distort = SmoothNoise(coords * float2(6.0, 2.0) + uTime * 0.4) * 0.03;
    float lineDistorted = smoothstep(0.03, 0.0, abs(lineY - 0.5 + distort));
    return lineDistorted * 0.6;
}

// Main swing trail: ink dissolution with funeral dirge weight
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float combo = saturate(uPhase);

    // --- Ink dissolution: flowing, spreading, organic tendrils ---
    float2 inkUV = float2(
        progress * uNoiseScale - uTime * uScrollSpeed * 0.6,
        coords.y * 2.5
    );
    float ink = InkFBM(inkUV * 2.0);

    // Ink spreads wider at the tail, tighter at the head
    float inkMask = smoothstep(1.0, 0.0, cross - (0.5 + ink * 0.4));
    float inkDensity = smoothstep(0.2, 0.6, ink) * inkMask;

    // --- Bleeding edge tendrils: ink reaches beyond the main body ---
    float tendrilReach = ink * 0.35 * (0.5 + progress * 0.5);
    float tendrils = smoothstep(0.0, 0.1, tendrilReach - cross + 0.6);
    tendrils *= smoothstep(1.0, 0.7, cross); // fade at far edge

    // --- Dark void core: heavy center line ---
    float core = smoothstep(0.25, 0.0, cross);
    float coreInk = core * saturate(ink * 1.3);

    // --- Music staff lines ghosting through the flow ---
    float staff = StaffLines(coords + float2(-uTime * 0.3, 0));
    staff *= inkMask * (0.3 + combo * 0.4);

    // --- Crimson bleed: color seeps through where ink is thinnest ---
    float bleedMask = smoothstep(0.45, 0.55, ink) * inkMask;
    float crimsonBleed = bleedMask * (1.0 - core * 0.5);

    // --- Fading note silhouettes at the trailing edge ---
    float2 noteUV = coords * float2(12.0, 5.0) + float2(uTime * 0.5, 0);
    float noteHash = HashNoise(floor(noteUV));
    float notes = step(0.92, noteHash) * saturate(progress - 0.3) * inkMask;
    float notePulse = sin(uTime * 3.0 + noteHash * 20.0) * 0.3 + 0.7;
    notes *= notePulse;

    // --- Secondary texture detail ---
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.2, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.3);

    // --- Color: dominated by darkness with crimson bleeding through ---
    float3 inkBlack = float3(0.03, 0.01, 0.04);
    float3 deepVoid = float3(0.06, 0.02, 0.08);
    float3 crimson = uColor;
    float3 dirtyPink = uSecondaryColor * 0.7;
    float3 staffGhost = float3(0.5, 0.4, 0.55);

    float3 color = inkBlack;
    color = lerp(color, deepVoid, tendrils);
    color = lerp(color, crimson * 0.8, crimsonBleed * (0.5 + combo * 0.5));
    color = lerp(color, dirtyPink, notes * 1.5);
    color += staffGhost * staff;
    color = lerp(color, float3(0.9, 0.85, 0.92), coreInk * 0.15 * combo);
    color *= detail;

    // The overall feel should be DARK — this is a funeral march
    float alpha = (inkDensity * 0.5 + coreInk * 0.35 + tendrils * 0.1 + notes * 0.05);
    alpha *= (1.0 - progress * 0.3);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow underlayer: dark ink haze
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Wide ink haze with organic flow
    float glow = exp(-cross * cross * 2.5);
    float inkFlow = SmoothNoise(coords * float2(4.0, 2.0) - uTime * 0.4);
    glow *= saturate(0.6 + inkFlow * 0.4);

    float pulse = sin(uTime * 2.0 + progress * 5.0) * 0.1 + 0.9;

    // Dark, heavy glow — more void than light
    float3 glowColor = lerp(float3(0.02, 0.008, 0.03), uColor * 0.2, glow * 0.4);
    // Faint crimson veining in the haze
    float vein = smoothstep(0.55, 0.65, inkFlow);
    glowColor += uSecondaryColor * vein * glow * 0.15;

    float alpha = glow * (1.0 - progress * 0.5) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.4;

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
