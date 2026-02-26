// =====================================================================
//  PearlescentRocketTrail.fx — Call of the Pearlescent Lake bullet trail
// =====================================================================
//
//  Visual: Pearlescent mother-of-pearl shimmer trail. Low-saturation HSL
//  color banding that shifts slowly like light on a nacre surface. Subtle
//  caustic highlights from noise give a water-surface quality.
//
//  The Pearlescent Lake: frozen beauty — opalescent shots with water
//  caustic highlights and soft edges.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (projectile tip), 1 = tail
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    PearlescentTrailMain  — Medium-width opal shimmer core
//    PearlescentTrailGlow  — Soft pearlescent bloom underlay
//
//  C# rendering order (3 passes):
//    1. PearlescentTrailGlow @ 3x width   (soft pearlescent bloom)
//    2. PearlescentTrailMain @ 1x width   (opal shimmer core)
//    3. PearlescentTrailGlow @ 1.5x width (overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Noise texture (SoftCircularCaustics)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (LakeSurface)
float3 uSecondaryColor;   // Secondary trail color (Pearlescent)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Lake-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Caustic distortion amount
float  uPhase;            // Reserved
float  uHasSecondaryTex;  // 1.0 if noise texture bound
float  uSecondaryTexScale; // Noise repetition
float  uSecondaryTexScroll; // Noise scroll speed

// =====================================================================
//  Utility
// =====================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// HSL to RGB for pearlescent color cycling
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

float3 HSLToRGB(float h, float s, float l)
{
    float3 rgb = HueToRGB(frac(h));
    float c = (1.0 - abs(2.0 * l - 1.0)) * s;
    return (rgb - 0.5) * c + l;
}

// Pearlescent color shift — low saturation, slow cycling
float3 PearlescentShift(float u, float time)
{
    // Multiple sine waves for organic mother-of-pearl shimmer
    float hue1 = sin(u * 6.0 + time * 0.5) * 0.5 + 0.5;
    float hue2 = sin(u * 3.5 - time * 0.3) * 0.5 + 0.5;
    float hue = frac(hue1 * 0.6 + hue2 * 0.4);

    // Low saturation for pearlescent feel (0.2 to 0.4)
    float saturation = 0.2 + 0.2 * sin(u * 4.0 + time * 0.2);

    // High luminance for pearl quality
    float luminance = 0.82;

    return HSLToRGB(hue, saturation, luminance);
}

// =====================================================================
//  PearlescentTrailMain — Opal shimmer core
// =====================================================================

float4 PearlescentMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Medium-width edge profile (softer than razor tracers)
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 2.0);

    // Moderate trail fade
    float trailFade = pow(saturate(1.0 - coords.x), 1.6);

    // Caustic distortion from noise
    float causticOffset = 0.0;
    float causticHighlight = 0.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 causticUV = float2(coords.x * uSecondaryTexScale - uTime * uScrollSpeed * 0.5,
                                  coords.y * uSecondaryTexScale * 0.7 + uTime * uSecondaryTexScroll * 0.2);
        float causticSample = tex2D(uImage1, causticUV).r;

        // Gentle UV distortion
        causticOffset = (causticSample - 0.5) * uDistortionAmt;

        // Caustic highlight — bright spots where caustics converge
        float2 causticUV2 = causticUV * 1.3 + float2(uTime * 0.1, 0.0);
        float caustic2 = tex2D(uImage1, causticUV2).r;
        causticHighlight = smoothstep(0.6, 0.85, causticSample * caustic2 * 2.0) * 0.2;
    }

    // Scrolled UV with caustic distortion
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + causticOffset);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === PEARLESCENT COLOR ===
    float3 pearlColor = PearlescentShift(coords.x, uTime);

    // Blend between primary lake surface and pearlescent shimmer
    float blendT = coords.x * 0.5 + sin(coords.y * 3.14159 + uTime * 0.5) * 0.15;
    float3 baseColor = lerp(uColor, uSecondaryColor, blendT);
    float3 trailColor = lerp(baseColor, pearlColor, 0.6);

    // White-hot center highlight
    float centerGlow = exp(-pow((coords.y - 0.5) / 0.08, 2.0));
    trailColor = lerp(trailColor, float3(0.98, 0.97, 1.0), centerGlow * 0.35 * trailFade);

    // Add caustic highlights as bright spots
    trailColor += float3(1.0, 0.98, 0.95) * causticHighlight * trailFade;

    // Soft tail dissolve via noise
    float tailDissolve = 1.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 dissolveUV = float2(coords.x * 2.0 + uTime * 0.3, coords.y * 1.5);
        float dissolveNoise = tex2D(uImage1, dissolveUV).r;
        float threshold = coords.x * 1.1 - 0.05;
        tailDissolve = smoothstep(threshold - 0.15, threshold + 0.05, dissolveNoise);
        tailDissolve = lerp(1.0, tailDissolve, smoothstep(0.5, 0.85, coords.x));
    }

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * tailDissolve * uOpacity;
    float3 finalColor = trailColor * uIntensity;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  PearlescentTrailGlow — Soft pearlescent bloom
// =====================================================================

float4 PearlescentGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.6);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.2);

    // Pearlescent glow color — slightly desaturated
    float3 pearlGlow = PearlescentShift(coords.x * 0.5, uTime * 0.5);
    float3 glowColor = lerp(uColor, pearlGlow, 0.4);

    // Breathing pulse — lake surface rhythm
    float pulse = 0.85 + 0.15 * sin(uTime * 1.8 + coords.x * 2.5);

    // Low opacity bloom
    float alpha = edgeFade * trailFade * uOpacity * 0.3 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique PearlescentTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PearlescentMainPS();
    }
}

technique PearlescentTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PearlescentGlowPS();
    }
}
