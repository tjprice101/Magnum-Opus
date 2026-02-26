// =============================================================================
// Triumphant Fractal Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Geometric, recursive-pattern shader for the Triumphant Fractal weapon.
// Creates hexagonal and triangular geometric overlays within trail geometry.
//
// UV Layout:
//   U (coords.x) = position along trail (0 = head, 1 = tail)
//   V (coords.y) = position across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   FractalEnergyTrail  - Trail body with hexagonal grid pattern overlay
//   FractalGlowPass     - Radial bloom with 6-fold symmetry and rotation
//
// Features:
//   - Vertex shader transforms via uTransformMatrix
//   - Hexagonal grid pattern within trail body (UV-space tiling)
//   - Grid lines glow brighter than fill areas
//   - FractalGold centre -> HexagonScarlet edge gradient
//   - Noise distortion modulated by hexagonal grid geometry
//   - Geometric (faceted) edge fade instead of smooth
//   - 6-fold symmetry in glow pass
//   - Rotating geometric pattern via uRotationSpeed
//   - Fractal depth controls sub-division complexity
//   - Overbright multiplier for HDR bloom
//   - Designed for multi-pass rendering
// =============================================================================

sampler2D uImage0 : register(s0); // Base trail texture / white gradient
sampler2D uImage1 : register(s1); // Noise texture (optional geometric noise)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (FractalGold)
float3 uSecondaryColor;  // Secondary color (HexagonScarlet)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;       // Grid scroll rate (default 1.2)
float uDistortionAmt;     // Geometric distortion strength (default 0.04)
float uNoiseScale;        // Noise UV repetition (default 3.0)
float uHasSecondaryTex;   // 1.0 if noise texture bound, 0.0 if not
float uSecondaryTexScale; // Noise texture UV scale
float uSecondaryTexScroll; // Noise scroll speed
float uFractalDepth;      // Recursion complexity (1.0-3.0)
float uRotationSpeed;     // Geometric rotation rate (default 0.3)

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
// HEXAGONAL GRID
// =============================================================================
// Produces a hexagonal tiling pattern from UV coordinates.
// Returns a value near 0 at hex cell centres and near 1 at cell edges,
// creating glowing grid lines when used as a brightness multiplier.
//
// The hexagonal distance is approximated using the standard hex metric:
//   d = max(|x|, (|x| + sqrt(3)*|y|) / 2)
// which produces a regular hexagonal Voronoi cell.
// =============================================================================

float HexGrid(float2 uv, float scale)
{
    float2 p = uv * scale;
    float sqrt3 = 1.732050808;

    // Convert to axial hex coordinates with row stagger
    float2 hexUV;
    hexUV.x = p.x;
    hexUV.y = p.y * sqrt3;

    // Round to nearest hex centre
    float2 cell = float2(floor(hexUV.x + 0.5), floor(hexUV.y + 0.5));
    float2 frac_part = hexUV - cell;

    // Hex distance: max of axial distances
    float ax = abs(frac_part.x);
    float ay = abs(frac_part.y);
    float hexDist = max(ax, (ax + ay * sqrt3) * 0.5);

    // Normalise: 0 at centre, 1 at edge
    return saturate(hexDist * 2.2);
}

// =============================================================================
// TECHNIQUE 1: FRACTAL ENERGY TRAIL
// =============================================================================
// Trail body with hexagonal grid pattern overlay. Grid lines glow
// brighter than fill areas, and intersection nodes have small bright
// spots. Noise distortion is modulated by the geometric grid rather
// than being purely organic, creating a structured, crystalline feel.
// =============================================================================

float4 FractalEnergyTrailPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Geometric distortion (structured, not chaotic) ---
    // Angular displacement following hex rhythm
    float geo1 = sin(coords.x * 10.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt;
    // Perpendicular geometric sway
    float geo2 = sin(coords.x * 16.0 - uTime * uScrollSpeed * 4.5 + coords.y * 6.0) * uDistortionAmt * 0.45;

    float2 distortedUV = coords;
    distortedUV.y += geo1;
    distortedUV.x += geo2;

    // Sample base texture with distorted UVs
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Hexagonal grid pattern ---
    // Scroll the grid along the trail for energy flow effect
    float2 gridUV = coords;
    gridUV.x -= uTime * uScrollSpeed * 0.4;
    float hexScale = 4.0 + uFractalDepth * 2.0; // More depth = denser grid
    float hexEdge = HexGrid(gridUV, hexScale);

    // Grid lines: bright where hexEdge is high (near cell boundary)
    float gridLine = saturate(hexEdge * 2.5 - 1.2);

    // Node bright spots: bright where hexEdge is very low (cell centres)
    float nodeMask = saturate(1.0 - hexEdge * 4.0);
    nodeMask *= nodeMask; // Sharpen the node spots

    // Sub-grid layer (fractal subdivision) at higher depth
    float2 subGridUV = gridUV;
    subGridUV.x += uTime * uScrollSpeed * 0.15;
    float subHexEdge = HexGrid(subGridUV, hexScale * 2.0);
    float subGridLine = saturate(subHexEdge * 2.0 - 1.3);
    // Blend sub-grid based on fractal depth (only visible at depth > 1.5)
    float subGridBlend = saturate((uFractalDepth - 1.5) * 0.7);
    gridLine = gridLine + subGridLine * subGridBlend * 0.4;

    // --- Noise for organic variation within hex cells ---
    // Procedural hash noise modulated by geometry
    float procNoise = HashNoise(gridUV * uNoiseScale + float2(uTime * 0.2, 0.0));
    // Blend with optional secondary texture
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(procNoise * 0.5 + 0.35, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Edge-to-centre fade with geometric faceting ---
    float edgeFade = QuadraticBump(coords.y);
    // Hexagonal influence on edge shape: 3 facets across width
    float edgeAngle = coords.y * 3.14159 * 3.0;
    float facet = abs(sin(edgeAngle)) * 0.08 + 0.92;
    float facetedEdge = edgeFade * facet;

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.08);

    // --- Fractal colour gradient along trail ---
    // FractalGold -> HexagonScarlet, modulated by grid
    float gradientT = coords.x * 0.6 + noiseVal * 0.2 + gridLine * 0.2;
    float3 trailColor = lerp(uColor, uSecondaryColor, gradientT);

    // --- Grid line glow: lines are brighter than fill ---
    trailColor *= 1.0 + gridLine * 0.5;

    // --- Node bright spots: golden-white flares at intersections ---
    float3 nodeColor = float3(1.0, 0.95, 0.80);
    trailColor = lerp(trailColor, nodeColor, nodeMask * 0.6);

    // --- White-hot core at beam centre ---
    float coreMask = saturate((edgeFade - 0.52) * 3.0);
    float3 hotCore = float3(1.0, 0.92, 0.75);
    trailColor = lerp(trailColor, hotCore, coreMask * 0.55);

    // --- Geometric pulse (crisp, not organic) ---
    float pulse = sin(uTime * 6.0 + coords.x * 10.0) * 0.05 + 0.95;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.65 + noiseVal * 0.35;

    float alpha = facetedEdge * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: FRACTAL GLOW PASS
// =============================================================================
// Radial bloom with hexagonal faceting and 6-fold symmetry.
// The glow pattern rotates slowly via uRotationSpeed, and fractal
// sub-divisions appear at the edges based on uFractalDepth. Creates a
// structured, crystalline energy aura with rotating geometric patterns
// rather than a smooth organic glow.
// =============================================================================

float4 FractalGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // --- Centre-relative coordinates ---
    float2 centred = coords - 0.5;

    // --- Apply rotation ---
    float rotAngle = uTime * uRotationSpeed;
    float cosR = cos(rotAngle);
    float sinR = sin(rotAngle);
    float2 rotated;
    rotated.x = centred.x * cosR - centred.y * sinR;
    rotated.y = centred.x * sinR + centred.y * cosR;

    float dist = length(rotated) * 2.0; // 0 at centre, 1 at edge
    float angle = atan2(rotated.y, rotated.x);

    // --- 6-fold symmetry faceting ---
    // Modulate radial distance by hexagonal envelope
    float hexFacet = cos(angle * 3.0); // 6-fold: cos(3*theta) pattern
    float facetedDist = dist * (1.0 - hexFacet * 0.12);

    // --- Radial falloff with hex faceting ---
    float radial = saturate(1.0 - facetedDist * facetedDist);
    radial *= radial; // Soft rolloff

    // --- Hex ring pattern at edge ---
    // Concentric hex-influenced rings at higher fractal depth
    float ringPattern = sin(facetedDist * 3.14159 * (2.0 + uFractalDepth * 2.0)) * 0.5 + 0.5;
    float ringMask = saturate((facetedDist - 0.3) * 3.0) * saturate((0.95 - facetedDist) * 4.0);
    float rings = ringPattern * ringMask * saturate(uFractalDepth - 1.0) * 0.25;

    // --- Colour: FractalGold core -> HexagonScarlet ring -> transparent ---
    float3 glowColor = lerp(uColor, uSecondaryColor, saturate(facetedDist * 1.5));

    // Add warm golden tint to core
    float3 goldTint = float3(1.0, 0.88, 0.55);
    glowColor = lerp(glowColor, goldTint, saturate(1.0 - facetedDist * 2.5) * 0.2);

    // Hexagonal faceting on colour gradient
    float colorFacet = cos(angle * 3.0) * 0.05 + 0.95;
    glowColor *= colorFacet;

    // Ring pattern adds brightness
    glowColor *= 1.0 + rings;

    // --- Gentle noise modulation ---
    float procNoise = HashNoise(coords * uNoiseScale + float2(uTime * 0.15, 0.0));
    float noiseVal = lerp(procNoise * 0.3 + 0.55, 0.85, 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // --- Rotating geometric shimmer ---
    float shimmer = sin(angle * 6.0 + uTime * 3.0) * 0.06 + 0.94;

    // --- Slow pulse ---
    float pulse = sin(uTime * 3.0) * 0.08 + 0.92;

    float alpha = (radial + rings * 0.3) * uOpacity * sampleColor.a * baseTex.a * pulse * shimmer;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FractalEnergyTrail
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 FractalEnergyTrailPS();
    }
}

technique FractalGlowPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 FractalGlowPS();
    }
}
