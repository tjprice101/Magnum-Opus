// =============================================================================
// Dark Flame Aura Shader - Inverted Wraith Fire
// =============================================================================
// Inverted flame aura with void eye center, shadow tendrils, and crimson
// tip fire. Dark core with bright edges -- the inverse of normal fire.
//
// VISUAL IDENTITY: Like peering into a dark portal wreathed in crimson
// flame -- a void eye opens at the centre, shadow tendrils reach outward
// through the dark fire, and sakura petal silhouettes burn in negative
// (dark shapes in bright flame edges). Deeply unsettling, deeply powerful.
//
// Techniques:
//   DarkFlameAuraMain  - Inverted flame with void eye & shadow tendrils
//   DarkFlameAuraGlow  - Dark crimson bloom with tendril halos
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Crimson)
float3 uSecondaryColor;  // Secondary color (Black)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uNoiseScale;
float uPhase;
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
// TECHNIQUE 1: DARK FLAME AURA - Void Eye & Shadow Tendrils
// =============================================================================

float4 DarkFlameAuraMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Slow dark flame distortion (ominous, heavy) ---
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
    float invertedCore = 1.0 - pow(edgeFade, 1.5);

    // --- Void Eye effect at trail centre ---
    // Creates an "eye" shape: dark iris with a bright slit pupil
    float eyeCenterY = saturate(abs(coords.y - 0.5) * 4.0); // Tall oval shape
    float eyeCenterX = saturate(abs(coords.x - 0.35) * 3.0); // Positioned toward head
    float eyeShape = saturate(1.0 - (eyeCenterY * 0.7 + eyeCenterX * 0.3));
    eyeShape = pow(eyeShape, 2.0);

    // Pupil slit: very narrow vertical bright line within the eye
    float pupilSlit = saturate(1.0 - abs(coords.y - 0.5) * 20.0); // Very thin vertical
    pupilSlit *= eyeShape;

    // Eye blink: the eye opens and closes slowly
    float blinkPhase = sin(uTime * 0.8) * 0.5 + 0.5; // Slow blink
    float eyeVisible = saturate(blinkPhase * 2.0 - 0.3) * uPhase;
    eyeShape *= eyeVisible;
    pupilSlit *= eyeVisible;

    // --- Shadow tendrils reaching outward from trail edges ---
    // 5 tendrils at irregular angular positions
    float tendrilAngle = coords.x * 3.14159 * 3.0 + uTime * uScrollSpeed * 0.5;
    float tendril1 = sin(tendrilAngle * 5.0 + coords.y * 8.0) * 0.5 + 0.5;
    float tendril2 = sin(tendrilAngle * 7.0 - coords.y * 6.0 + 2.0) * 0.5 + 0.5;
    float tendril3 = cos(tendrilAngle * 3.0 + coords.y * 12.0 + 4.0) * 0.5 + 0.5;

    // Tendrils appear at the edges only
    float tendrilEdgeMask = saturate((0.5 - edgeFade) * 3.0);
    float tendrils = (pow(tendril1, 3.0) + pow(tendril2, 3.0) + pow(tendril3, 4.0)) * 0.33;
    tendrils *= tendrilEdgeMask * uPhase;

    // --- Dark flame noise ---
    float2 noiseP = coords * uNoiseScale;
    noiseP.x -= uTime * uScrollSpeed * 0.4;
    noiseP.y -= uTime * 0.2;
    float procNoise = SmoothHash(noiseP);

    float2 secUV = coords * uNoiseScale;
    secUV.x -= uTime * uScrollSpeed * 0.6;
    secUV.y -= uTime * 0.15;
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(procNoise, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Sakura petal silhouettes in dark fire (negative shapes) ---
    float petalAngle = coords.x * 3.14159 * 5.0 + uTime * uScrollSpeed * 2.0;
    float petalSilhouette = cos(petalAngle) * 0.15 + sin(petalAngle * 0.7 + noiseVal * 3.0) * 0.1;
    petalSilhouette = saturate(petalSilhouette + 0.75);

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.1);

    // --- Colour: Black core -> Crimson edges ---
    float3 darkCore = uSecondaryColor;
    float3 brightEdge = uColor;
    float3 flameColor = lerp(darkCore, brightEdge, invertedCore);

    // Crimson-gold tips on flame tongues
    float tipMask = saturate(invertedCore * 2.0 - 0.6);
    float3 goldTip = float3(0.8, 0.4, 0.1);
    flameColor = lerp(flameColor, goldTip, tipMask * noiseVal * 0.4);

    // Tendrils are shadow-dark with faint crimson edges
    float3 tendrilColor = lerp(uSecondaryColor, uColor, 0.15);
    flameColor = lerp(flameColor, tendrilColor, tendrils * 0.6);

    // Void eye: deep black iris with crimson pupil slit
    float3 voidBlack = float3(0.02, 0.005, 0.01);
    float3 pupilCrimson = uColor * 2.0; // Overbright crimson pupil
    flameColor = lerp(flameColor, voidBlack, eyeShape * 0.8);
    flameColor = lerp(flameColor, pupilCrimson, pupilSlit * 0.6);

    // Apply petal silhouette darkening
    flameColor *= petalSilhouette;

    // --- Smoke dissolution at edges ---
    float smokeEdge = saturate(edgeFade * 1.2 - (1.0 - noiseVal) * 0.5);

    // --- Ominous pulse (slower, deeper than other weapons) ---
    float pulse = sin(uTime * 1.5 + coords.x * 2.0) * 0.08 + 0.92;
    pulse *= sin(uTime * 0.7) * 0.04 + 0.96; // Very slow breathe

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.5 + noiseVal * 0.5;

    float alpha = (smokeEdge * trailFade + tendrils * 0.2 + eyeShape * 0.1) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: DARK FLAME AURA GLOW - Tendril Halo
// =============================================================================

float4 DarkFlameAuraGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.9);

    // Shadow tendril impressions in glow layer too
    float tendrilGlow = sin(coords.x * 10.0 + uTime * 1.0 + coords.y * 5.0) * 0.5 + 0.5;
    tendrilGlow = pow(tendrilGlow, 3.0) * saturate((0.5 - edgeFade) * 2.5) * 0.15;

    // Dark crimson glow
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.4);
    float3 darkTint = float3(0.3, 0.08, 0.05);
    glowColor = lerp(glowColor, darkTint, 0.3);

    float noiseVal = SmoothHash(coords * uNoiseScale * 0.5 - float2(uTime * 0.2, 0.0));
    glowColor *= uIntensity * noiseVal * baseTex.rgb * 0.6;

    float pulse = sin(uTime * 1.5) * 0.1 + 0.9;

    float alpha = (softEdge + tendrilGlow) * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse * 0.3;

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
