// =============================================================================
// Triumphant Fractal Shader - Living Sacred Geometry
// =============================================================================
// Hexagonal grid with pulsing energy veins between nodes, recursive
// subdivision that breathes with life, and constellation-like node
// connections that light up in sequence.
//
// VISUAL IDENTITY: Like a living crystal lattice of pure mathematical
// energy -- hexagonal cells pulse with golden fire, veins of light
// connect nodes in geometric perfection, sub-grids appear and vanish 
// as the fractal breathes through recursive depth.
//
// Techniques:
//   FractalEnergyTrail  - Trail with pulsing hex grid & energy veins
//   FractalGlowPass     - Rotating geometric bloom with 6-fold symmetry
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (FractalGold)
float3 uSecondaryColor;  // Secondary color (HexagonScarlet)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uNoiseScale;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;
float uFractalDepth;      // Recursion complexity (1.0-3.0)
float uRotationSpeed;     // Geometric rotation rate

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

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// HEXAGONAL GRID with Energy Veins
// =============================================================================
// Returns: x = distance to nearest hex edge (0=centre, 1=edge)
//          y = distance to nearest hex vertex (for node pulses)

float2 HexGridEx(float2 uv, float scale)
{
    float2 p = uv * scale;
    float sqrt3 = 1.732050808;

    float2 hexUV;
    hexUV.x = p.x;
    hexUV.y = p.y * sqrt3;

    float2 cell = float2(floor(hexUV.x + 0.5), floor(hexUV.y + 0.5));
    float2 frac_part = hexUV - cell;

    float ax = abs(frac_part.x);
    float ay = abs(frac_part.y);
    float hexDist = max(ax, (ax + ay * sqrt3) * 0.5);

    // Distance to nearest vertex (hex corner)
    // Hex vertices are at 60-degree intervals at the edge
    float angle = atan2(frac_part.y, frac_part.x);
    float vertexAngle = floor(angle / 1.0472 + 0.5) * 1.0472; // Snap to nearest 60 deg
    float2 nearestVertex = float2(cos(vertexAngle), sin(vertexAngle)) * 0.577; // 1/sqrt(3)
    float vertexDist = length(frac_part - nearestVertex);

    return float2(saturate(hexDist * 2.2), saturate(vertexDist * 3.0));
}

// =============================================================================
// TECHNIQUE 1: FRACTAL ENERGY TRAIL - Living Crystal Lattice
// =============================================================================

float4 FractalEnergyTrailPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Geometric distortion (structured, crystalline) ---
    float geo1 = sin(coords.x * 10.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt;
    float geo2 = sin(coords.x * 16.0 - uTime * uScrollSpeed * 4.5 + coords.y * 6.0) * uDistortionAmt * 0.45;

    float2 distortedUV = coords;
    distortedUV.y += geo1;
    distortedUV.x += geo2;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Primary hexagonal grid with energy veins ---
    float2 gridUV = coords;
    gridUV.x -= uTime * uScrollSpeed * 0.4;
    float hexScale = 4.0 + uFractalDepth * 2.0;
    float2 hexInfo = HexGridEx(gridUV, hexScale);
    float hexEdge = hexInfo.x;
    float hexVertex = hexInfo.y;

    // Grid edge lines (energy veins)
    float gridLine = saturate(hexEdge * 2.5 - 1.2);

    // --- Pulsing energy veins: veins brighten in traveling waves ---
    float veinPulse = sin(coords.x * 6.0 - uTime * 4.0 + hexEdge * 8.0) * 0.5 + 0.5;
    veinPulse = pow(veinPulse, 2.0);
    float energyVein = gridLine * (0.5 + veinPulse * 0.5);

    // --- Node bright spots at hex vertices ---
    float nodeMask = saturate(1.0 - hexVertex * 2.5);
    nodeMask *= nodeMask;

    // Nodes pulse in sequence (traveling wave along trail)
    float nodePhase = sin(coords.x * 8.0 - uTime * 3.0) * 0.5 + 0.5;
    float nodeBrightness = 0.3 + nodePhase * 0.7;
    nodeMask *= nodeBrightness;

    // --- Sub-grid layer (recursive subdivision) ---
    float2 subGridUV = gridUV;
    subGridUV.x += uTime * uScrollSpeed * 0.15;
    float2 subHexInfo = HexGridEx(subGridUV, hexScale * 2.0);
    float subGridLine = saturate(subHexInfo.x * 2.0 - 1.3);
    float subGridBlend = saturate((uFractalDepth - 1.5) * 0.7);

    // Sub-grid breathes in and out (visibility oscillates)
    float subBreath = sin(uTime * 1.5) * 0.5 + 0.5;
    subGridLine *= subGridBlend * (0.3 + subBreath * 0.7);

    float combinedGrid = energyVein + subGridLine * 0.35;

    // --- Tertiary micro-grid (highest depth, faint shimmer) ---
    float2 microUV = gridUV;
    microUV.x -= uTime * uScrollSpeed * 0.08;
    float2 microHex = HexGridEx(microUV, hexScale * 4.0);
    float microLine = saturate(microHex.x * 1.8 - 1.4);
    float microBlend = saturate((uFractalDepth - 2.5) * 1.0);
    combinedGrid += microLine * microBlend * 0.15;

    // --- Noise for organic variation ---
    float procNoise = HashNoise(gridUV * uNoiseScale + float2(uTime * 0.2, 0.0));
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(procNoise * 0.5 + 0.35, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Edge fade with geometric faceting ---
    float edgeFade = QuadraticBump(coords.y);
    float edgeAngle = coords.y * 3.14159 * 3.0;
    float facet = abs(sin(edgeAngle)) * 0.08 + 0.92;
    float facetedEdge = edgeFade * facet;

    // --- Trail length fade ---
    float trailFade = saturate(1.0 - coords.x * 1.08);

    // --- Fractal colour gradient ---
    float gradientT = coords.x * 0.5 + noiseVal * 0.2 + combinedGrid * 0.15;
    float3 trailColor = lerp(uColor, uSecondaryColor, gradientT);

    // Energy veins glow brighter
    trailColor *= 1.0 + combinedGrid * 0.6;

    // --- Node constellation flares (golden-white) ---
    float3 nodeColor = float3(1.0, 0.95, 0.80);
    trailColor = lerp(trailColor, nodeColor, nodeMask * 0.7);

    // --- White-hot core ---
    float coreMask = saturate((edgeFade - 0.52) * 3.0);
    float3 hotCore = float3(1.0, 0.92, 0.75);
    trailColor = lerp(trailColor, hotCore, coreMask * 0.55);

    // --- Geometric pulse (crisp, crystalline) ---
    float pulse = sin(uTime * 6.0 + coords.x * 10.0) * 0.05 + 0.95;

    // --- Final composition ---
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.60 + noiseVal * 0.40;

    float alpha = facetedEdge * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: FRACTAL GLOW PASS - Rotating Geometric Bloom
// =============================================================================

float4 FractalGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;

    // Apply rotation
    float rotAngle = uTime * uRotationSpeed;
    float cosR = cos(rotAngle);
    float sinR = sin(rotAngle);
    float2 rotated;
    rotated.x = centred.x * cosR - centred.y * sinR;
    rotated.y = centred.x * sinR + centred.y * cosR;

    float dist = length(rotated) * 2.0;
    float angle = atan2(rotated.y, rotated.x);

    // 6-fold symmetry faceting
    float hexFacet = cos(angle * 3.0);
    float facetedDist = dist * (1.0 - hexFacet * 0.12);

    float radial = saturate(1.0 - facetedDist * facetedDist);
    radial *= radial;

    // Concentric hex rings that pulse
    float ringPattern = sin(facetedDist * 3.14159 * (2.0 + uFractalDepth * 2.0) - uTime * 2.0) * 0.5 + 0.5;
    float ringMask = saturate((facetedDist - 0.3) * 3.0) * saturate((0.95 - facetedDist) * 4.0);
    float rings = ringPattern * ringMask * saturate(uFractalDepth - 1.0) * 0.25;

    // Color
    float3 glowColor = lerp(uColor, uSecondaryColor, saturate(facetedDist * 1.5));
    float3 goldTint = float3(1.0, 0.88, 0.55);
    glowColor = lerp(glowColor, goldTint, saturate(1.0 - facetedDist * 2.5) * 0.2);

    float colorFacet = cos(angle * 3.0) * 0.05 + 0.95;
    glowColor *= colorFacet;
    glowColor *= 1.0 + rings;

    float procNoise = HashNoise(coords * uNoiseScale + float2(uTime * 0.15, 0.0));
    float noiseVal = lerp(procNoise * 0.3 + 0.55, 0.85, 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    float shimmer = sin(angle * 6.0 + uTime * 3.0) * 0.06 + 0.94;
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
