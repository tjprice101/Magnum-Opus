// SwanMonochromeDissolve.fx -- Swan Lake death dissolve (4-phase aware)
//
// Death is the swan's most beautiful moment. Three dissolve stages:
//   0-40%:   Pure monochrome dissolve. Grayscale fragments. Silver/white edges. Dying in monochrome.
//   40-80%:  Prismatic light breaks through dissolving cracks. Rainbow edges emerge.
//   80-100%: Full prismatic eruption. Remaining fragments blaze with rainbow. True beauty at death.
//
// uPhase/uDrain modulate spectacle -- Phase 4 with high drain = maximum prismatic intensity.

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);

float uDissolveProgress;  // 0-1 over the full death sequence
float4 uEdgeColor;
float uEdgeWidth;
float uPhase;   // 1-4
float uDrain;   // 0-1

float3 hsl2rgb(float h, float s, float l)
{
    float3 rgb = clamp(abs(fmod(h * 6.0 + float3(0, 4, 2), 6.0) - 3.0) - 1.0, 0.0, 1.0);
    return l + s * (rgb - 0.5) * (1.0 - abs(2.0 * l - 1.0));
}

float4 PS_MonochromeDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01) return float4(0, 0, 0, 0);

    float progress = uDissolveProgress;

    // Convert sprite to grayscale -- the monochromatic identity
    float grayVal = dot(sprite.rgb, float3(0.299, 0.587, 0.114));
    float3 grayColor = float3(grayVal, grayVal, grayVal);

    // Desaturate based on progress (fully gray by 40%)
    float desatAmount = saturate(progress * 2.5);
    float3 monoSprite = lerp(sprite.rgb, grayColor, desatAmount);

    // Noise-driven dissolve threshold
    float n = tex2D(uNoiseTex, uv * 2.0).r;
    float threshold = progress * 1.2;
    float clipVal = n - threshold;
    if (clipVal < 0.0) return float4(0, 0, 0, 0);

    // Edge detection
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    // --- Stage 1 (0-40%): Pure monochrome dissolve. Silver/white edges. ---
    float3 silverEdge = float3(0.85, 0.85, 0.90);

    // --- Stage 2 (40-80%): Prismatic breaks through the cracks ---
    float prismaticOnset = smoothstep(0.35, 0.5, progress);
    float hue = frac(n * 4.0 + progress * 2.0 + uv.x * 0.5);
    float3 rainbow = hsl2rgb(hue, 0.9 * prismaticOnset, 0.65);

    // --- Stage 3 (80-100%): Full prismatic eruption ---
    float eruptionOnset = smoothstep(0.75, 0.85, progress);
    float3 brilliantRainbow = hsl2rgb(frac(hue + 0.1), 1.0, 0.75);

    // Blend edge color across the three stages
    float3 edgeColor = silverEdge;
    edgeColor = lerp(edgeColor, rainbow, prismaticOnset);
    edgeColor = lerp(edgeColor, brilliantRainbow, eruptionOnset);

    // Wider, more intense edges at later stages
    float edgeWidthMod = 1.0 + prismaticOnset * 0.5 + eruptionOnset * 1.0;
    float wideEdge = 1.0 - smoothstep(0.0, uEdgeWidth * edgeWidthMod, clipVal);

    // Late-stage fragments become prismatic too
    float3 fragmentColor = monoSprite;
    fragmentColor = lerp(fragmentColor, lerp(monoSprite, rainbow, 0.3), eruptionOnset);

    // Phase 4 / drain amplifies the prismatic spectacle
    float drainBoost = saturate(uDrain * 0.3);
    edgeColor += brilliantRainbow * drainBoost * wideEdge;

    // Final composite
    float3 result = lerp(fragmentColor, edgeColor, wideEdge);

    // Brightness boost at eruption -- the swan's true beauty at death
    result += brilliantRainbow * wideEdge * eruptionOnset * 0.4;

    return float4(result, sprite.a);
}

technique Technique1
{
    pass MonochromeDissolve { PixelShader = compile ps_3_0 PS_MonochromeDissolve(); }
}
