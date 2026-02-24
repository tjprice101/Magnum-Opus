// =============================================================================
// Eroica Funeral Trail Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Somber, smoky flame trail for funeral-themed weapons, enemies, and
// accessories (Funeral Prayer, Funeral Blitzer, Funeral March Insignia).
// A heavier, slower-burning counterpart to HeroicFlameTrail.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   FuneralFlameFlow  - Slow smoldering trail with smoke dissolution at edges
//   FuneralGlowPass   - Soft mournful radial glow with heartbeat pulse
//
// Features:
//   - Vertex shader transforms via uTransformMatrix
//   - Slower scroll speed for somber, funeral-march pacing
//   - Smoky edge dissolution with procedural hash noise
//   - uColor centre -> uSecondaryColor mid -> transparent edge gradient
//   - "Incense rising" vertical wisps peeling off the trail
//   - Reduced flicker (steady, smoldering flames)
//   - Dimmer white-hot core compared to heroic variant
//   - Overbright multiplier for HDR bloom
//   - Designed for multi-pass rendering (C# renders 3 passes)
// =============================================================================

sampler2D uImage0 : register(s0); // Base trail texture / white gradient
sampler2D uImage1 : register(s1); // Noise texture (e.g. NoiseSmoke, PerlinNoise)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (DirgeRed)
float3 uSecondaryColor;  // Secondary color (RequiemGold)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;       // Flame flow scroll rate (default 0.8, slower than heroic)
float uDistortionAmt;     // Smoke turbulence strength (default 0.05)
float uNoiseScale;        // Noise UV repetition (default 2.5)
float uHasSecondaryTex;   // 1.0 if noise texture bound, 0.0 if not
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed
float uSmokeIntensity;    // Smoke overlay strength (0.0-1.0)

// =============================================================================
// VERTEX SHADER
// =============================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

// =============================================================================
// UTILITY
// =============================================================================

// Hash-based procedural noise (PS 2.0 safe, no extra texture reads)
float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

// Smooth value noise from hash (4-tap bilinear interpolation)
float SmoothHash(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);

    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));

    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

// 0->1->0 bump (peak at centre)
float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: FUNERAL FLAME FLOW
// =============================================================================
// Slow, smoldering flame trail that burns with quiet grief. Smoke curls
// upward from the trail edges while the core holds a dim ember glow.
// Uses procedural hash noise for smoke dissolution and wisp patterns.
// Compared to HeroicFlameFlow: slower scroll, heavier smoke, steadier
// burn, and an "incense rising" wisp effect at the upper edge.
// =============================================================================

float4 FuneralFlameFlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Smoke turbulence distortion ---
    // Gentle vertical drift (smoke rises slowly, not heroic licking)
    float drift1 = sin(coords.x * 5.0 + uTime * uScrollSpeed * 2.0) * uDistortionAmt;
    // Slow lateral sway (smoke billows sideways)
    float drift2 = sin(coords.x * 9.0 - uTime * uScrollSpeed * 3.0 + coords.y * 3.0) * uDistortionAmt * 0.5;
    // Low-frequency undulation for heavy smoke movement
    float drift3 = sin(coords.x * 3.0 + uTime * uScrollSpeed * 1.2) * uDistortionAmt * 0.35;

    float2 distortedUV = coords;
    distortedUV.y += drift1 + drift3;
    distortedUV.x += drift2;

    // Sample base texture
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Procedural smoke noise ---
    float2 noiseP = coords * uNoiseScale;
    noiseP.x -= uTime * uScrollSpeed * 0.6;
    noiseP.y -= uTime * 0.15; // Smoke rises upward, slowly
    float procNoise = SmoothHash(noiseP);

    // Blend with optional secondary texture
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll * 0.8;
    secUV.y -= uTime * 0.2;
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(procNoise, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Edge-to-centre fade ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Smoky edge dissolution ---
    float smokeEdgeNoise = lerp(procNoise, noiseTex.g, uHasSecondaryTex * uSmokeIntensity * 0.5);
    float smokeEdge = saturate(edgeFade * 1.3 - (1.0 - smokeEdgeNoise) * uSmokeIntensity * 0.6);

    // --- Incense wisp effect: thin vertical streams peeling upward ---
    float wispMask = saturate(1.0 - coords.y * 2.5); // Top-biased
    float wispPattern = sin(coords.x * 18.0 + uTime * uScrollSpeed * 5.0) * 0.5 + 0.5;
    wispPattern *= sin(coords.x * 25.0 - uTime * 2.0) * 0.3 + 0.7;
    float wisps = wispMask * wispPattern * uSmokeIntensity * 0.25;

    // --- Trail length fade (head glows, tail turns to ash) ---
    float trailFade = saturate(1.0 - coords.x * 1.15);
    float ashTail = saturate(coords.x * 2.0 - 0.85) * 0.10;

    // --- Funeral colour gradient ---
    // uColor centre -> uSecondaryColor mid -> ash gray at tail
    float gradientT = coords.x * 0.65 + noiseVal * 0.35;
    float3 flameColor = lerp(uColor, uSecondaryColor, gradientT);
    float3 ashGray = float3(0.35, 0.30, 0.28);
    float ashBlend = saturate((coords.x - 0.6) * 2.5);
    flameColor = lerp(flameColor, ashGray, ashBlend * 0.5);

    // --- Dimmer white-hot core (somber, not blazing) ---
    float coreMask = saturate((edgeFade - 0.55) * 2.5);
    float3 hotCore = float3(0.92, 0.85, 0.70);
    flameColor = lerp(flameColor, hotCore, coreMask * 0.45);

    // --- Smoke-darkened edges ---
    float edgeMask = saturate((0.55 - edgeFade) * 3.0);
    flameColor *= 1.0 - edgeMask * uSmokeIntensity * 0.3;

    // --- Slow, steady pulse (heartbeat-like, not frantic) ---
    float pulse = sin(uTime * 4.0 + coords.x * 5.0) * 0.04 + 0.96;
    pulse *= sin(uTime * 2.5 + coords.x * 8.0) * 0.03 + 0.97;

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.55 + noiseVal * 0.45;

    float alpha = (smokeEdge * trailFade + ashTail + wisps) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: FUNERAL GLOW PASS
// =============================================================================
// Soft, mournful radial glow overlay for bloom stacking. Renders behind
// the main flame pass to create a dimmer, heavier halo. DirgeRed to
// RequiemGold gradient. Pulse rate is reduced to a gentle heartbeat
// rhythm. Edges dissolve into smoke.
// =============================================================================

float4 FuneralGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // Gentle smoke distortion
    float wave = sin(coords.x * 3.5 + uTime * uScrollSpeed * 1.5) * uDistortionAmt * 0.35;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    // --- Centre-relative coordinates for radial glow ---
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial falloff (mournful, wide)
    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    // Wider edge fade for smoke halo along trail width
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    // Procedural smoke dissolution at glow edges
    float2 smokeP = coords * uNoiseScale * 0.5;
    smokeP.x -= uTime * uScrollSpeed * 0.3;
    smokeP.y -= uTime * 0.1;
    float smokeNoise = SmoothHash(smokeP);
    float smokeDissolve = lerp(1.0, smokeNoise * 0.6 + 0.4, uSmokeIntensity);
    softEdge *= smokeDissolve;

    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.9);

    // --- DirgeRed -> RequiemGold mournful gradient ---
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.45);

    // Add warm but subdued amber tint
    float3 amberTint = float3(0.85, 0.45, 0.15);
    glowColor = lerp(glowColor, amberTint, 0.12);

    // Gentle noise modulation
    float noiseVal = SmoothHash(coords * uNoiseScale * 0.55 - float2(uTime * 0.3, 0.0));
    noiseVal = lerp(0.75, noiseVal, 0.45);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // --- Gentle heartbeat pulse (funeral pace) ---
    float heartbeat = sin(uTime * 2.0 + coords.x * 3.0) * 0.12 + 0.88;

    // Combine radial glow with trail edge shape
    float shape = max(radial, softEdge * 0.6);
    float alpha = shape * trailFade * uOpacity * sampleColor.a * baseTex.a * heartbeat;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FuneralFlameFlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 FuneralFlameFlowPS();
    }
}

technique FuneralGlowPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_2_0 FuneralGlowPS();
    }
}
