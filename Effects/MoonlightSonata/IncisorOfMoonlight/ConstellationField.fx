// =============================================================================
// MagnumOpus Constellation Field Shader - PS 2.0 Compatible
// =============================================================================
// Parallax starfield overlay for Incisor of Moonlight  E"The Stellar Scalpel".
// Creates a layered, slowly-rotating star field that overlays the swing arc,
// giving the impression of cutting through a constellation map.
//
// Visual concept: Each precision swing reveals stars beneath the surface of
// reality, as if the scalpel blade can slice open the night sky itself.
//
// UV Layout:
//   U (coords.x) = position along trail / arc (0 = head, 1 = tail)
//   V (coords.y) = position across width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   ConstellationFieldMain  EFull parallax starfield with connecting lines
//   ConstellationFieldGlow  ESofter bloom pass for layered stacking
// =============================================================================

sampler uImage0 : register(s0); // Base texture (bloom/trail body)
sampler uImage1 : register(s1); // Star field scatter texture

float3 uColor;           // Primary color (e.g. ConstellationBlue)
float3 uSecondaryColor;  // Secondary color (e.g. HarmonicWhite)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Combo resonance level (0.3 - 1.0)

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uScrollSpeed;       // Field rotation / scroll rate
float uNoiseScale;        // Star field UV repetition
float uDistortionAmt;     // Parallax depth shift amplitude
float uHasSecondaryTex;   // 1.0 if star scatter texture bound
float uSecondaryTexScale;
float uSecondaryTexScroll;
float uPhase;             // Constellation phase (0-1): controls star density/brightness

// =============================================================================
// UTILITY
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Procedural star function  Ecreates sharp bright points at pseudo-random positions
// Uses a hash-based approach to generate star positions within grid cells
float StarField(float2 uv, float density, float brightness)
{
    // Grid-based star placement
    float2 gridUV = uv * density;
    float2 cellID = floor(gridUV);
    float2 cellUV = frac(gridUV) - 0.5;

    // Pseudo-random star position within cell using hash
    float hash1 = frac(sin(dot(cellID, float2(127.1, 311.7))) * 43758.5453);
    float hash2 = frac(sin(dot(cellID, float2(269.5, 183.3))) * 43758.5453);
    float hash3 = frac(sin(dot(cellID, float2(419.2, 371.9))) * 43758.5453);

    // Star presence probability (not every cell has a star)
    float starPresent = step(0.55 - uPhase * 0.2, hash3); // More stars at higher phase

    // Star position offset within cell
    float2 starPos = float2(hash1 - 0.5, hash2 - 0.5) * 0.6;

    // Distance from pixel to star center
    float dist = length(cellUV - starPos);

    // Sharp Gaussian point light
    float star = exp(-dist * dist * (200.0 + brightness * 300.0)) * starPresent;

    // Twinkle based on time + cell hash
    float twinkle = 0.6 + 0.4 * sin(uTime * (3.0 + hash1 * 4.0) + hash2 * 6.28);
    star *= twinkle;

    return star;
}

// Constellation line detection  Edraws faint lines between nearby star cells
float ConstellationLines(float2 uv, float density)
{
    float2 gridUV = uv * density;
    float2 cellID = floor(gridUV);
    float2 cellUV = frac(gridUV);

    float lineAccum = 0.0;

    // Check connections to neighboring cells (simplified for ps_3_0)
    float hash0 = frac(sin(dot(cellID, float2(419.2, 371.9))) * 43758.5453);
    float star0 = step(0.55 - uPhase * 0.2, hash0);

    if (star0 > 0.5)
    {
        // Check right neighbor
        float2 rightCell = cellID + float2(1.0, 0.0);
        float hashR = frac(sin(dot(rightCell, float2(419.2, 371.9))) * 43758.5453);
        float starR = step(0.55 - uPhase * 0.2, hashR);

        if (starR > 0.5)
        {
            // Line segment between star positions
            float posY0 = frac(sin(dot(cellID, float2(269.5, 183.3))) * 43758.5453) * 0.6;
            float posYR = frac(sin(dot(rightCell, float2(269.5, 183.3))) * 43758.5453) * 0.6;
            float expectedY = lerp(posY0, posYR, cellUV.x);
            float lineDist = abs(cellUV.y - 0.5 - (expectedY - 0.5) * 0.3);
            lineAccum += exp(-lineDist * lineDist * 400.0) * 0.3;
        }

        // Check bottom neighbor
        float2 downCell = cellID + float2(0.0, 1.0);
        float hashD = frac(sin(dot(downCell, float2(419.2, 371.9))) * 43758.5453);
        float starD = step(0.55 - uPhase * 0.2, hashD);

        if (starD > 0.5)
        {
            float posX0 = frac(sin(dot(cellID, float2(127.1, 311.7))) * 43758.5453) * 0.6;
            float posXD = frac(sin(dot(downCell, float2(127.1, 311.7))) * 43758.5453) * 0.6;
            float expectedX = lerp(posX0, posXD, cellUV.y);
            float lineDist = abs(cellUV.x - 0.5 - (expectedX - 0.5) * 0.3);
            lineAccum += exp(-lineDist * lineDist * 400.0) * 0.3;
        }
    }

    return saturate(lineAccum);
}

// =============================================================================
// TECHNIQUE 1: CONSTELLATION FIELD MAIN
// =============================================================================

float4 ConstellationFieldPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // --- Edge fade (surgical precision) ---
    float edgeFade = QuadraticBump(coords.y);
    float sharpEdge = pow(edgeFade, 1.3);

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.05);

    // --- Parallax field UV with scrolling rotation ---
    float scrollT = uTime * uScrollSpeed * 0.3;
    float cosT = cos(scrollT * 0.2);
    float sinT = sin(scrollT * 0.2);

    // Layer 1: Background stars (slow, dim)
    float2 bgUV = coords;
    bgUV.x += scrollT * 0.15;
    bgUV.y += sin(uTime * 0.2) * 0.02;
    float bgStars = StarField(bgUV, 8.0, 0.5);

    // Layer 2: Midground stars (medium speed, medium brightness)
    float2 midUV = coords;
    midUV.x += scrollT * 0.35;
    midUV.y += cos(uTime * 0.3) * 0.03;
    float midStars = StarField(midUV * 1.3 + 0.5, 6.0, 1.0);

    // Layer 3: Foreground stars (fast, bright)
    float2 fgUV = coords;
    fgUV.x += scrollT * 0.6;
    fgUV.y += sin(uTime * 0.5) * 0.04;
    float fgStars = StarField(fgUV * 0.7 + 1.3, 4.0, 2.0);

    // Constellation connecting lines on midground layer
    float conLines = ConstellationLines(midUV * 1.3 + 0.5, 6.0) * uPhase;

    // --- Combine star layers ---
    float starBrightness = bgStars * 0.3 + midStars * 0.6 + fgStars * 1.0;
    starBrightness *= uIntensity;

    // --- Color mapping ---
    // Stars: blend from secondary (white/bright) at center to primary (blue) at edges
    float3 starColor = lerp(uSecondaryColor, uColor, saturate(1.0 - starBrightness * 2.0));
    starColor = lerp(starColor, float3(1.0, 0.98, 1.0), saturate(starBrightness * 1.5));

    // Constellation lines: primary color with subtle glow
    float3 lineColor = uColor * 1.2;

    // --- Base texture sampling ---
    float4 baseTex = tex2D(uImage0, coords);

    // --- Noise texture modulation ---
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Resonance pulse ---
    float pulse = sin(uTime * 5.0 + coords.x * 6.0) * 0.06 * uIntensity + 0.94;

    // --- Final composition ---
    float3 finalColor = starColor * starBrightness + lineColor * conLines;
    finalColor += baseTex.rgb * uColor * 0.2; // Subtle base texture tinting
    finalColor *= noiseVal * pulse;

    float alpha = sharpEdge * trailFade * uOpacity * sampleColor.a
                * (starBrightness + conLines * 0.5 + 0.15); // Minimum alpha for field glow

    return ApplyOverbright(finalColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUE 2: CONSTELLATION FIELD GLOW
// =============================================================================

float4 ConstellationFieldGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Softer version for bloom stacking
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    float trailFade = saturate(1.0 - coords.x * 0.85);

    // Single star layer (simpler for glow pass)
    float2 starUV = coords;
    starUV.x += uTime * uScrollSpeed * 0.25;
    float stars = StarField(starUV, 6.0, 0.8);

    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.3);
    glowColor = lerp(glowColor, float3(1.0, 0.98, 1.0), stars * 0.5);

    // Gentle pulse
    float pulse = sin(uTime * 3.0 + coords.x * 4.0) * 0.05 + 0.95;

    float alpha = softEdge * trailFade * uOpacity * sampleColor.a
                * (stars * 0.6 + 0.15) * uIntensity * pulse;

    return ApplyOverbright(glowColor, saturate(alpha));
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique ConstellationFieldMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConstellationFieldPS();
    }
}

technique ConstellationFieldGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConstellationFieldGlowPS();
    }
}
