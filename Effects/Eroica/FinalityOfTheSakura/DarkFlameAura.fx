// =============================================================================
// Dark Flame Aura Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Inverted flame aura for Finality of the Sakura minion. Dark core with
// bright crimson edges  Ethe inverse of normal fire. Black flame tongues
// with crimson tips, sakura petal silhouettes embedded in the dark fire.
//
// UV Layout:
//   U (coords.x) = along trail/around aura (0-1)
//   V (coords.y) = across width / radial (0 = top, 1 = bottom)
//
// Techniques:
//   DarkFlameAuraMain  - Inverted dark flame with crimson edge glow
//   DarkFlameAuraGlow  - Soft dark bloom overlay
//
// Features:
//   - Inverted luminance: dark centre, bright edges (opposite of normal fire)
//   - Black flame tongues with crimson-gold tips
//   - Sakura petal silhouettes in dark fire via 5-fold angular modulation
//   - Slow, ominous turbulence (funeral pacing)
//   - Smoke noise dissolution for heavy, wraith-like feel
//   - Overbright on edges only for selective HDR bloom
// =============================================================================

sampler2D uImage0 : register(s0); // Base trail texture
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Crimson)
float3 uSecondaryColor;  // Secondary color (Black)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;       // Flame scroll rate (slow)
float uDistortionAmt;     // Dark flame turbulence
float uNoiseScale;        // Noise UV repetition
float uPhase;            // Pulse phase / intensity
float uHasSecondaryTex;

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

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

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

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: DARK FLAME AURA MAIN
// =============================================================================

float4 DarkFlameAuraMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Slow dark flame distortion ---
    float drift1 = sin(coords.x * 5.0 + uTime * uScrollSpeed * 1.5) * uDistortionAmt;
    float drift2 = sin(coords.x * 9.0 - uTime * uScrollSpeed * 2.5 + coords.y * 3.0) * uDistortionAmt * 0.5;
    float drift3 = sin(coords.x * 3.0 + uTime * uScrollSpeed * 0.8) * uDistortionAmt * 0.4;

    float2 distortedUV = coords;
    distortedUV.y += drift1 + drift3;
    distortedUV.x += drift2;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Edge-to-centre fade ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Inverted luminance: dark centre, bright edges ---
    // Normally fire is bright at centre; here we invert
    float invertedCore = 1.0 - pow(edgeFade, 1.5); // Dark at centre, bright at edge

    // --- Dark flame noise ---
    float2 noiseP = coords * uNoiseScale;
    noiseP.x -= uTime * uScrollSpeed * 0.4;
    noiseP.y -= uTime * 0.2;
    float procNoise = SmoothHash(noiseP);

    // Optional noise texture
    float2 secUV = coords * uNoiseScale;
    secUV.x -= uTime * uScrollSpeed * 0.6;
    secUV.y -= uTime * 0.15;
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(procNoise, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Sakura petal silhouettes in dark fire ---
    // 5-fold angular modulation creates petal-like dark patches
    float petalAngle = coords.x * 3.14159 * 5.0 + uTime * uScrollSpeed * 2.0;
    float petalSilhouette = cos(petalAngle) * 0.15 + sin(petalAngle * 0.7 + noiseVal * 3.0) * 0.1;
    petalSilhouette = saturate(petalSilhouette + 0.75); // Subtle dark impressions

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.1);

    // --- Colour: Black core ↁECrimson edges ---
    // Inverted fire gradient
    float3 darkCore = uSecondaryColor; // Black
    float3 brightEdge = uColor; // Crimson
    float3 flameColor = lerp(darkCore, brightEdge, invertedCore);

    // Crimson tips on flame tongues
    float tipMask = saturate(invertedCore * 2.0 - 0.6);
    float3 goldTip = float3(0.8, 0.4, 0.1); // Dark gold
    flameColor = lerp(flameColor, goldTip, tipMask * noiseVal * 0.4);

    // Apply petal silhouette darkening
    flameColor *= petalSilhouette;

    // --- Smoke dissolution at edges ---
    float smokeEdge = saturate(edgeFade * 1.2 - (1.0 - noiseVal) * 0.5);

    // --- Ominous slow pulse ---
    float pulse = sin(uTime * 2.0 + coords.x * 3.0) * 0.06 + 0.94;

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.5 + noiseVal * 0.5;

    float alpha = smokeEdge * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: DARK FLAME AURA GLOW
// =============================================================================

float4 DarkFlameAuraGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Wider, softer edge
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    // Trail fade
    float trailFade = saturate(1.0 - coords.x * 0.9);

    // Dark crimson glow (subtle, not bright)
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.4);
    float3 darkTint = float3(0.3, 0.08, 0.05);
    glowColor = lerp(glowColor, darkTint, 0.3);

    // Noise modulation
    float noiseVal = SmoothHash(coords * uNoiseScale * 0.5 - float2(uTime * 0.2, 0.0));

    glowColor *= uIntensity * noiseVal * baseTex.rgb * 0.6;

    float pulse = sin(uTime * 1.5) * 0.1 + 0.9;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse * 0.3;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique DarkFlameAuraMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 DarkFlameAuraMainPS();
    }
}

technique DarkFlameAuraGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 DarkFlameAuraGlowPS();
    }
}
