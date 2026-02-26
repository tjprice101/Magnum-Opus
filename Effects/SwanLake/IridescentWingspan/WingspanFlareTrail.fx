// =====================================================================
//  WingspanFlareTrail.fx — Iridescent Wingspan homing flare trail
// =====================================================================
//
//  Visual: Homing projectile trail with feather-dissolve tail. Elegant
//  mid-width white core with pearlescent secondary color shifting.
//  At coords.x > 0.5 (tail half), FBM noise threshold creates
//  progressive dissolution where the trail scatters into feather-shaped
//  fragments. Each of the 3 projectiles has a slightly different hue
//  bias via uPhase (0.0 / 0.33 / 0.66).
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head, 1 = tail
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    WingspanFlareMain — Mid-width core with feather dissolve
//    WingspanFlareGlow — Soft pearlescent bloom underlay
//
//  C# rendering order (3 passes):
//    1. WingspanFlareGlow @ 2.5x width  (pearlescent bloom)
//    2. WingspanFlareMain @ 1x width    (feather-dissolve core)
//    3. WingspanFlareGlow @ 1.5x width  (overbright halo)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Noise texture (TileableFBMNoise)

// --- Standard uniforms ---
float3 uColor;            // Primary trail color (PureWhite)
float3 uSecondaryColor;   // Secondary trail color (Rainbow via uPhase)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Feather distortion amount
float  uPhase;            // Per-projectile hue offset: 0.0 / 0.33 / 0.66
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

// =====================================================================
//  Feather Dissolve — progressive breakup at trail tail
// =====================================================================

/// Returns dissolve mask (1 = solid, 0 = dissolved).
/// Dissolution increases toward tail (coords.x -> 1).
/// Uses FBM noise for organic feather-shaped fragments.
float FeatherDissolve(float2 coords, float time)
{
    if (uHasSecondaryTex < 0.5)
        return 1.0; // No dissolve without noise

    // Primary dissolve noise — large-scale feather shapes
    float2 dissolveUV = float2(
        coords.x * uSecondaryTexScale * 0.8 + time * 0.15,
        coords.y * uSecondaryTexScale * 1.2 + time * 0.05
    );
    float primaryNoise = tex2D(uImage1, dissolveUV).r;

    // Secondary finer noise — feather barb detail
    float2 fineUV = float2(
        coords.x * uSecondaryTexScale * 2.0 - time * 0.1,
        coords.y * uSecondaryTexScale * 3.0 + time * 0.08
    );
    float fineNoise = tex2D(uImage1, fineUV).r;

    // Blend noise layers (FBM-like)
    float combinedNoise = primaryNoise * 0.65 + fineNoise * 0.35;

    // Dissolve threshold — increases along trail toward tail
    // No dissolve at head (coords.x < 0.35), full dissolve at tail
    float dissolveStart = 0.35;
    float dissolveProgress = saturate((coords.x - dissolveStart) / (1.0 - dissolveStart));
    dissolveProgress = pow(dissolveProgress, 1.5); // Accelerate toward tail

    // Threshold: noise must exceed this to remain visible
    float threshold = dissolveProgress * 1.1 - 0.05;

    // Soft dissolve edge for feather-like fragmentation
    float dissolveMask = smoothstep(threshold - 0.1, threshold + 0.06, combinedNoise);

    // At the very head, always solid
    dissolveMask = lerp(1.0, dissolveMask, smoothstep(0.2, 0.5, coords.x));

    return dissolveMask;
}

// =====================================================================
//  Pearlescent Color Shift — per-projectile hue identity
// =====================================================================

float3 PearlescentShift(float2 coords, float time, float projectilePhase)
{
    // Slow hue cycling along trail length with per-projectile offset
    float hue = frac(coords.x * 0.8 + time * 0.06 + projectilePhase);

    // Low saturation = pearlescent / opalescent feel
    float saturation = 0.25 + 0.15 * sin(coords.x * 4.0 + time * 0.3);
    float luminance = 0.85;

    return HSLToRGB(hue, saturation, luminance);
}

// =====================================================================
//  WingspanFlareMain — Mid-width core with feather dissolve
// =====================================================================

float4 WingspanFlareMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Mid-width core profile
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 1.8);

    // Trail fade
    float trailFade = pow(saturate(1.0 - coords.x), 1.2);

    // Noise-driven distortion
    float noiseOffset = 0.0;
    float textureDetail = 0.0;
    if (uHasSecondaryTex > 0.5)
    {
        float2 noiseUV = float2(
            coords.x * uSecondaryTexScale - uTime * uScrollSpeed * 0.3,
            coords.y * uSecondaryTexScale * 0.6 + uTime * uSecondaryTexScroll * 0.08
        );
        float noiseSample = tex2D(uImage1, noiseUV).r;
        noiseOffset = (noiseSample - 0.5) * uDistortionAmt;

        // Texture detail for subtle shimmer
        float2 detailUV = noiseUV * 1.5 + float2(uTime * 0.05, 0.0);
        float detail = tex2D(uImage1, detailUV).r;
        textureDetail = smoothstep(0.5, 0.8, noiseSample * detail * 2.0) * 0.15;
    }

    // Scrolled UV for base texture
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y + noiseOffset);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === PEARLESCENT COLOR ===
    float3 pearlColor = PearlescentShift(coords, uTime, uPhase);

    // Blend with base white color
    float3 trailColor = lerp(uColor, pearlColor, 0.5);

    // === WHITE-HOT CENTER ===
    float centerGlow = exp(-pow((coords.y - 0.5) / 0.08, 2.0));
    trailColor = lerp(trailColor, float3(1.0, 1.0, 1.0), centerGlow * 0.5 * trailFade);

    // === PER-PROJECTILE HUE ACCENT ===
    // Prismatic fringe at edges, biased by projectile identity
    float edgeProximity = 1.0 - edgeFade;
    float3 hueAccent = HueToRGB(frac(coords.x * 2.0 + uPhase + uTime * 0.1));
    trailColor += hueAccent * edgeProximity * 0.15 * trailFade;

    // Add texture detail shimmer
    float3 shimmerColor = HSLToRGB(frac(coords.x * 1.5 + uPhase + uTime * 0.08), 0.4, 0.75);
    trailColor += shimmerColor * textureDetail * trailFade;

    // === FEATHER DISSOLVE ===
    float dissolveMask = FeatherDissolve(coords, uTime);

    // Bright edge at dissolve boundary (feather-fragment glow)
    float dissolveEdge = 0.0;
    if (dissolveMask > 0.01 && dissolveMask < 0.95)
    {
        dissolveEdge = (1.0 - abs(dissolveMask - 0.5) * 2.0) * 0.3;
        float3 edgeCol = HueToRGB(frac(uPhase + coords.x * 1.0 + uTime * 0.12));
        trailColor += edgeCol * dissolveEdge;
    }

    // === WALTZ PULSE ===
    float waltzPulse = 0.92 + 0.08 * sin(uTime * 3.14159 + coords.x * 2.0);
    trailColor *= waltzPulse;

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * dissolveMask * uOpacity;
    float3 finalColor = trailColor * uIntensity;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  WingspanFlareGlow — Soft pearlescent bloom
// =====================================================================

float4 WingspanFlareGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wide, soft edge profile for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.7);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.1);

    // Pearlescent bloom color per-projectile
    float hue = frac(coords.x * 0.6 + uPhase + uTime * 0.05);
    float3 glowColor = HSLToRGB(hue, 0.3, 0.8);

    // Blend toward base white
    glowColor = lerp(uColor, glowColor, 0.4);

    // Breathing pulse
    float pulse = 0.85 + 0.15 * sin(uTime * 2.5 + coords.x * 2.0);

    // Feather dissolve — apply to glow too for coherent fragmentation
    float dissolveMask = 1.0;
    if (uHasSecondaryTex > 0.5)
    {
        // Softer dissolve for glow (wider transition)
        float2 dissolveUV = float2(
            coords.x * uSecondaryTexScale * 0.8 + uTime * 0.15,
            coords.y * uSecondaryTexScale * 1.0 + uTime * 0.05
        );
        float noise = tex2D(uImage1, dissolveUV).r;
        float dissolveProgress = saturate((coords.x - 0.5) / 0.5);
        dissolveProgress = pow(dissolveProgress, 2.0);
        float threshold = dissolveProgress * 0.9;
        dissolveMask = smoothstep(threshold - 0.15, threshold + 0.1, noise);
        dissolveMask = lerp(1.0, dissolveMask, smoothstep(0.3, 0.6, coords.x));
    }

    // Low opacity bloom
    float alpha = edgeFade * trailFade * uOpacity * 0.3 * pulse * dissolveMask;
    float4 finalColor = float4(glowColor * uIntensity * 0.5, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique WingspanFlareMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 WingspanFlareMainPS();
    }
}

technique WingspanFlareGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 WingspanFlareGlowPS();
    }
}
