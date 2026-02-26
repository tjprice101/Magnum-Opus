// =====================================================================
//  SwanFlareTrail.fx — Call of the Black Swan homing flare trail
// =====================================================================
//
//  Visual: Thin razor tracer for homing BlackSwanFlare sub-projectiles.
//  uPhase determines polarity:
//    0.0 = Black flare (dark core, silver edge, faint dark-purple fringe)
//    1.0 = White flare (white core, dark edge, faint gold-prismatic fringe)
//
//  Deliberately lightweight — procedural hash noise only, no secondary
//  texture sampling. These are small fast projectiles so the trail needs
//  to be quick and clean.
//
//  UV convention:
//    U (coords.x) = along trail: 0 = head (projectile tip), 1 = tail
//    V (coords.y) = across width: 0 = top edge, 1 = bottom edge
//
//  Techniques:
//    SwanFlareMain — Razor-thin polarity tracer with prismatic fringe
//    SwanFlareGlow — Faint soft bloom halo around the tracer
//
//  C# rendering order (3 passes):
//    1. SwanFlareGlow @ 3x width   (barely-visible bloom underlay)
//    2. SwanFlareMain @ 1x width   (sharp polarity core)
//    3. SwanFlareGlow @ 1.5x width (subtle overbright whisper)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Trail body texture
sampler uImage1 : register(s1); // Unused (no noise needed for this lightweight trail)

// --- Standard uniforms ---
float3 uColor;            // Primary color (Black: ObsidianBlack, White: PureWhite)
float3 uSecondaryColor;   // Secondary color (Black: Silver, White: DarkSilver)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // Additive overbright

// --- Flare-specific uniforms ---
float  uScrollSpeed;      // UV scroll speed
float  uNoiseScale;       // Unused
float  uDistortionAmt;    // Unused
float  uPhase;            // Polarity: 0.0 = black flare, 1.0 = white flare
float  uHasSecondaryTex;  // Unused
float  uSecondaryTexScale; // Unused
float  uSecondaryTexScroll; // Unused

// =====================================================================
//  Utility
// =====================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

// Cheap procedural hash noise (no texture dependency)
float HashNoise(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

// HSL hue to RGB for prismatic fringe
float3 HueToRGB(float hue)
{
    float r = abs(hue * 6.0 - 3.0) - 1.0;
    float g = 2.0 - abs(hue * 6.0 - 2.0);
    float b = 2.0 - abs(hue * 6.0 - 4.0);
    return saturate(float3(r, g, b));
}

// =====================================================================
//  SwanFlareMain — Razor polarity tracer with prismatic fringe
// =====================================================================

float4 SwanFlareMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Razor-thin core — very narrow for fast projectiles
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 3.0);

    // Fast trail fade — these are quick projectiles
    float trailFade = saturate(1.0 - coords.x * 1.8);
    trailFade = pow(trailFade, 1.5);

    // Scrolled UV
    float2 scrolledUV = float2(coords.x - uTime * uScrollSpeed, coords.y);
    float4 trailSample = tex2D(uImage0, scrolledUV);

    // === POLARITY COLOR ===
    // Core color = primary (pure polarity), edges blend to secondary
    float coreT = pow(edgeFade, 2.0);
    float3 coreColor = lerp(uSecondaryColor, uColor, coreT);

    // Bright center line
    float centerBright = exp(-pow((coords.y - 0.5) / 0.06, 2.0));
    // Hot center: black flare gets silver center, white flare gets bright white
    float3 hotCenter = lerp(uSecondaryColor, float3(1.0, 1.0, 1.0), 0.5);
    coreColor = lerp(coreColor, hotCenter, centerBright * 0.5 * trailFade);

    // === PRISMATIC FRINGE ===
    // Thin rainbow shimmer at the very outer edge of the trail
    float edgeDist = abs(coords.y - 0.5);
    float fringeMask = smoothstep(0.3, 0.45, edgeDist) * (1.0 - smoothstep(0.45, 0.5, edgeDist));
    float fringeHue = frac(coords.x * 3.0 + uTime * 0.2 + uPhase * 0.5);
    float3 fringeColor = HueToRGB(fringeHue) * 0.7;
    coreColor += fringeColor * fringeMask * trailFade * 0.4;

    // === SPARKLE HASH ===
    // Sparse procedural sparkles
    float sparkle = HashNoise(float2(floor(coords.x * 20.0) + uTime * 3.0, floor(coords.y * 6.0)));
    sparkle = smoothstep(0.85, 0.95, sparkle) * 0.3;
    coreColor += float3(1.0, 1.0, 1.0) * sparkle * trailFade;

    // === FINAL COMPOSITE ===
    float alpha = trailSample.a * coreFade * trailFade * uOpacity;
    float3 finalColor = coreColor * uIntensity * 0.8;
    float4 result = float4(finalColor * alpha, 0.0);

    return result * uOverbrightMult * sampleColor;
}

// =====================================================================
//  SwanFlareGlow — Soft bloom halo
// =====================================================================

float4 SwanFlareGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Wider, softer edge for bloom
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.7);

    // Gentle trail fade
    float trailFade = saturate(1.0 - coords.x * 1.5);

    // Glow color — blend of primary and secondary
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.4);

    // Soft breathing pulse
    float pulse = 0.88 + 0.12 * sin(uTime * 2.0 + coords.x * 4.0);

    // Very low alpha — barely-visible bloom
    float alpha = edgeFade * trailFade * uOpacity * 0.25 * pulse;
    float4 finalColor = float4(glowColor * uIntensity * 0.4, 0.0) * alpha;

    return finalColor * sampleColor;
}

// =====================================================================
//  Techniques
// =====================================================================

technique SwanFlareMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwanFlareMainPS();
    }
}

technique SwanFlareGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwanFlareGlowPS();
    }
}
