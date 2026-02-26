// =====================================================================
//  EtherealWing.fx — Iridescent Wingspan procedural wing silhouette
// =====================================================================
//
//  Visual: Procedural ethereal wing silhouettes rendered as a quad
//  behind the player. Two mirrored parabolic curves with diagonal
//  feather barb lines within the wing shape. Slow pearlescent HSL
//  cycling fill. uPhase controls unfurl state (0 = subtle idle,
//  1 = full cast burst). Waltz-time breathing pulse.
//
//  UV convention:
//    U (uv.x) = horizontal: 0 = left, 1 = right
//    V (uv.y) = vertical:   0 = top, 1 = bottom
//    Center (0.5, 0.5) = player attachment point
//
//  Techniques:
//    EtherealWingMain — Sharp feather-line wing overlay
//    EtherealWingGlow — Soft prismatic bloom underlay
//
//  C# rendering order (2 passes):
//    1. EtherealWingGlow @ 2.5x scale  (prismatic bloom)
//    2. EtherealWingMain @ 2.0x scale   (sharp feather lines)
// =====================================================================

// --- Samplers ---
sampler uImage0 : register(s0); // Base texture (soft glow / circular mask)
sampler uImage1 : register(s1); // Noise texture (SoftCircularCaustics)

// --- Standard uniforms ---
float4 uColor;            // Primary wing color (PureWhite)
float4 uSecondaryColor;   // Secondary color (PrismaticShimmer)
float  uOpacity;          // Overall opacity
float  uTime;             // Animation time
float  uIntensity;        // Brightness multiplier
float  uOverbrightMult;   // HDR overbright
float  uScrollSpeed;      // Feather line scroll
float  uNoiseScale;       // Noise UV scale
float  uDistortionAmt;    // Feather distortion
bool   uHasSecondaryTex;  // Noise texture bound
float  uSecondaryTexScale; // Noise repetition
float  uSecondaryTexScroll; // Noise scroll speed
float  uPhase;            // Unfurl state: 0 = subtle idle, 1 = full cast burst

// =====================================================================
//  Utility
// =====================================================================

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
//  Wing Shape — mirrored parabolic curves
// =====================================================================

/// Returns wing mask (0 = outside, 1 = inside wing shape).
/// Wings extend outward from center (0.5, 0.5).
/// Each wing is an upward-curving parabola.
float WingShape(float2 uv, float unfurl)
{
    float2 centered = uv - 0.5;

    // Mirror horizontally — both wings use same math
    float ax = abs(centered.x);
    float ay = centered.y;

    // Wing span scales with unfurl (0.15 idle, 0.45 full burst)
    float span = 0.15 + unfurl * 0.30;

    // Wing starts at ax > 0.03 (gap at center for body)
    float startDist = smoothstep(0.03, 0.06, ax);

    // Parabolic upper edge: curves upward then back down
    float wingProgress = saturate((ax - 0.03) / span);
    float upperEdge = -0.18 * wingProgress * (1.0 - wingProgress * 0.6) * (1.0 + unfurl * 0.5);

    // Lower edge: gentle downward curve
    float lowerEdge = upperEdge + 0.06 + wingProgress * 0.12 * (1.0 + unfurl * 0.3);

    // Vertical mask
    float inWing = smoothstep(upperEdge - 0.012, upperEdge + 0.008, ay) *
                   (1.0 - smoothstep(lowerEdge - 0.008, lowerEdge + 0.012, ay));

    // Horizontal mask — fade at wing tips
    float tipFade = 1.0 - smoothstep(span * 0.7, span, ax - 0.03);

    // Combine
    return inWing * startDist * tipFade;
}

// =====================================================================
//  Feather Barb Lines — diagonal sine patterns within wing
// =====================================================================

/// Diagonal feather lines at varying angles within the wing.
/// Returns feather line intensity (0-1).
float FeatherLines(float2 uv, float time, float unfurl)
{
    float2 centered = uv - 0.5;
    float ax = abs(centered.x);
    float ay = centered.y;

    // Primary feather barbs — angled from center outward
    // Angle increases toward wing tip
    float angle = 0.6 + ax * 1.2;
    float rotX = ax * cos(angle) - ay * sin(angle);

    float barbs = sin(rotX * 45.0 + time * 0.5) * 0.5 + 0.5;
    barbs = smoothstep(0.3, 0.7, barbs); // Sharpen to lines

    // Secondary finer barbs at different angle
    float angle2 = 0.3 + ax * 0.8;
    float rotX2 = ax * cos(angle2) - ay * sin(angle2);
    float fineBarbs = sin(rotX2 * 80.0 + time * 0.3) * 0.5 + 0.5;
    fineBarbs = smoothstep(0.4, 0.6, fineBarbs) * 0.4;

    // Combine — stronger toward tips
    float tipStrength = smoothstep(0.0, 0.25, ax);
    return (barbs * 0.6 + fineBarbs) * tipStrength * (0.5 + unfurl * 0.5);
}

// =====================================================================
//  Pearlescent Fill — slow HSL cycling
// =====================================================================

float3 PearlescentFill(float2 uv, float time, float featherLines)
{
    float ax = abs(uv.x - 0.5);

    // Base hue cycles slowly along wing span
    float hue = frac(ax * 1.5 + time * 0.04);
    float saturation = 0.25 + featherLines * 0.3;
    float luminance = 0.85 - featherLines * 0.15;

    float3 pearlColor = HSLToRGB(hue, saturation, luminance);

    // Feather line prismatic accent — more saturated on the barb lines
    float barbHue = frac(hue + 0.3 + featherLines * 0.2);
    float3 barbColor = HSLToRGB(barbHue, 0.6, 0.7);

    return lerp(pearlColor, barbColor, featherLines * 0.5);
}

// =====================================================================
//  EtherealWingMain — Sharp feather-line wing overlay
// =====================================================================

float4 PS_EtherealWingMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float unfurl = uPhase;

    // === WING SHAPE MASK ===
    float wingMask = WingShape(uv, unfurl);
    if (wingMask < 0.01)
        return float4(0, 0, 0, 0);

    // === FEATHER LINES ===
    float feathers = FeatherLines(uv, uTime, unfurl);

    // === NOISE MODULATION ===
    float noiseDetail = 0.0;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale;
        noiseUV += float2(uTime * uSecondaryTexScroll * 0.05, uTime * 0.03);
        noiseDetail = tex2D(uImage1, noiseUV).r;
        noiseDetail = smoothstep(0.3, 0.7, noiseDetail) * 0.2;
    }

    // === PEARLESCENT COLOR ===
    float3 wingColor = PearlescentFill(uv, uTime, feathers);

    // Blend with base colors
    wingColor = lerp(uColor.rgb, wingColor, 0.6);

    // Add prismatic feather-line highlights
    float barbHighlight = feathers * 0.3;
    float3 prismColor = HueToRGB(frac(abs(uv.x - 0.5) * 3.0 + uTime * 0.06));
    wingColor += prismColor * barbHighlight;

    // Add noise texture variation
    float3 noiseColor = HSLToRGB(frac(uv.y * 2.0 + uTime * 0.05), 0.3, 0.7);
    wingColor += noiseColor * noiseDetail;

    // === WALTZ-TIME PULSE ===
    // 3/4 time breathing: emphasis on beat 1
    float waltzPulse = 0.88 + 0.12 * sin(uTime * 3.14159);
    wingColor *= waltzPulse;

    // === EDGE GLOW ===
    // Bright edge at wing boundary
    float edgeGlow = wingMask * (1.0 - wingMask) * 4.0; // Peaks at boundary
    edgeGlow = pow(edgeGlow, 1.5) * 0.3;
    wingColor += uSecondaryColor.rgb * edgeGlow;

    // === UNFURL FLASH ===
    // During cast (unfurl near 1), extra brightness
    float flashBoost = unfurl * unfurl * 0.4;

    // === ATTACHMENT FADE ===
    // Fade near center (body attachment point)
    float2 centered = uv - 0.5;
    float centerDist = length(centered);
    float attachFade = smoothstep(0.02, 0.08, centerDist);

    // === FINAL COMPOSITE ===
    float3 finalColor = wingColor * uIntensity * (1.0 + flashBoost);
    float finalAlpha = baseTex.a * uOpacity * color.a * wingMask * attachFade;
    finalAlpha *= 0.35 + unfurl * 0.35; // Subtle when idle, stronger at cast

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  EtherealWingGlow — Soft prismatic bloom underlay
// =====================================================================

float4 PS_EtherealWingGlow(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float unfurl = uPhase;

    // Expanded soft wing mask (wider than main for bloom effect)
    float wingMask = WingShape(uv, unfurl + 0.1); // Slightly larger

    // Soft radial glow from center
    float2 centered = uv - 0.5;
    float radialGlow = exp(-dot(centered, centered) * 8.0);

    // Prismatic tint based on horizontal position
    float hue = frac(abs(centered.x) * 2.0 + uTime * 0.03);
    float3 glowColor = HSLToRGB(hue, 0.3, 0.8);

    // Blend toward base white
    glowColor = lerp(uColor.rgb, glowColor, 0.35);

    // Waltz pulse
    float pulse = 0.85 + 0.15 * sin(uTime * 3.14159);

    // Bloom contribution — soft, wide, low alpha
    float3 finalColor = glowColor * uIntensity * 0.4;
    float bloom = wingMask * radialGlow * pulse;
    float finalAlpha = baseTex.a * uOpacity * color.a * bloom;
    finalAlpha *= 0.2 + unfurl * 0.25; // Very subtle idle, moderate at cast

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Techniques
// =====================================================================

technique EtherealWingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_EtherealWingMain();
    }
}

technique EtherealWingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_EtherealWingGlow();
    }
}
