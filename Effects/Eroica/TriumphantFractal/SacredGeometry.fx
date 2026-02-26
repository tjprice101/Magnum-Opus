// =============================================================================
// Sacred Geometry Shader - PS 2.0 Compatible
// =============================================================================
// Procedural 6-pointed star geometry (hexagram) with recursive inner triangles
// for Triumphant Fractal cast/impact bursts. Golden energy fills the shape
// from center outward, with rotating sub-elements.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1), centre = 0.5
//   V (coords.y) = vertical position (0-1), centre = 0.5
//
// Techniques:
//   SacredGeometryMain  - Geometric hexagram with recursive triangles
//   SacredGeometryGlow  - Soft bloom for geometric aura
//
// Features:
//   - 6-pointed star via polar triangle overlay
//   - Phase-driven fill (0 = appear, 1 = fully expanded/fading)
//   - Counter-rotating inner and outer elements
//   - Golden energy fill from centre outward
//   - Recursive depth controls inner subdivision brightness
//   - Overbright for HDR geometric glow
// =============================================================================

sampler uImage0 : register(s0); // Base texture
sampler uImage1 : register(s1); // Noise texture (optional)

float3 uColor;            // Primary color (Gold)
float3 uSecondaryColor;   // Secondary color (Scarlet)
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;             // Expansion phase (0 = appear, 1 = fully expanded)
float uRotationSpeed;      // Geometric rotation rate
float uRecursionDepth;     // Sub-division complexity (1-3)
float uScrollSpeed;        // Energy flow speed
float uHasSecondaryTex;

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// Hexagram distance: creates a 6-pointed star shape
float HexagramDist(float2 centred, float rotation)
{
    float angle = atan2(centred.y, centred.x) + rotation;
    float dist = length(centred);

    // Two overlapping triangles rotated 60 degrees
    float tri1 = cos(angle * 3.0);
    float tri2 = cos((angle + 1.0472) * 3.0); // +60 degrees

    // Combine for hexagram envelope
    float hexagram = max(tri1, tri2);
    return dist / (hexagram * 0.3 + 0.7); // Modulate radius by hexagram shape
}

// =============================================================================
// TECHNIQUE 1: SACRED GEOMETRY MAIN
// =============================================================================

float4 SacredGeometryMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- Outer hexagram: counter-clockwise rotation ---
    float outerRot = uTime * uRotationSpeed;
    float outerHex = HexagramDist(centred, outerRot);

    // Hexagram edge lines
    float outerEdge = saturate(1.0 - abs(outerHex - 0.45) * 15.0);
    outerEdge *= saturate(1.0 - dist * 1.2); // Fade at far edges

    // --- Inner hexagram: clockwise rotation, smaller ---
    float innerRot = -uTime * uRotationSpeed * 1.5;
    float2 innerCentred = centred * 1.6; // Scale down
    float innerHex = HexagramDist(innerCentred, innerRot);

    float innerEdge = saturate(1.0 - abs(innerHex - 0.35) * 18.0);
    innerEdge *= saturate(1.0 - dist * 1.8);
    // Inner only visible at higher recursion depth
    innerEdge *= saturate(uRecursionDepth - 1.0);

    // --- Tertiary micro-geometry (highest depth only) ---
    float microRot = uTime * uRotationSpeed * 2.5;
    float2 microCentred = centred * 2.8;
    float microHex = HexagramDist(microCentred, microRot);
    float microEdge = saturate(1.0 - abs(microHex - 0.3) * 22.0);
    microEdge *= saturate(1.0 - dist * 2.5);
    microEdge *= saturate(uRecursionDepth - 2.0) * 0.5;

    // --- Radial energy fill from centre ---
    float fillRadius = uPhase * 0.8;
    float fillMask = saturate(1.0 - (dist - fillRadius) * 5.0);
    fillMask = saturate(fillMask);

    // --- 6-fold angular lines (connecting vertices) ---
    float angularLine = abs(sin(angle * 3.0 + outerRot));
    float lineMask = saturate(1.0 - angularLine * 6.0) * saturate(dist * 3.0) * saturate(1.0 - dist * 1.5);

    // --- Colour: Gold body ↁEScarlet edges ---
    float3 geoColor = lerp(uColor, uSecondaryColor, dist * 0.7);

    // White-hot centre
    float3 hotCore = float3(1.0, 0.95, 0.80);
    float coreMask = saturate(1.0 - dist * 3.5);
    geoColor = lerp(geoColor, hotCore, coreMask * 0.5 * fillMask);

    // Edge lines are brighter
    float3 lineColor = lerp(uColor, hotCore, 0.4);

    // --- Noise for organic variation ---
    float noise = HashNoise(coords * 4.0 + float2(uTime * 0.15, 0.0));

    // --- Phase opacity (fades as it expands) ---
    float phaseOpacity = saturate(1.0 - uPhase * 0.5);

    // --- Pulse ---
    float pulse = sin(uTime * 5.0 + dist * 6.0) * 0.06 + 0.94;

    // --- Final composition ---
    float totalEdge = saturate(outerEdge + innerEdge + microEdge);
    float totalFill = fillMask * (1.0 - totalEdge); // Fill between lines

    float3 finalColor = geoColor * totalFill * uIntensity;
    finalColor += lineColor * totalEdge * uIntensity * 1.3;
    finalColor += lineColor * lineMask * uIntensity * 0.5;
    finalColor *= pulse * (0.7 + noise * 0.3);

    float alpha = (totalEdge + totalFill * 0.5 + lineMask * 0.3 + coreMask * 0.3) *
        phaseOpacity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: SACRED GEOMETRY GLOW
// =============================================================================

float4 SacredGeometryGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial glow with 6-fold faceting
    float angle = atan2(centred.y, centred.x) + uTime * uRotationSpeed * 0.5;
    float hexFacet = cos(angle * 3.0) * 0.1 + 0.9;

    float radial = saturate(1.0 - dist * dist * hexFacet);
    radial *= radial;

    // Golden glow
    float3 glowColor = lerp(uColor, float3(1.0, 0.92, 0.70), 0.3);
    glowColor *= uIntensity * baseTex.rgb;

    float phaseOpacity = saturate(1.0 - uPhase * 0.4);

    float pulse = sin(uTime * 3.0) * 0.1 + 0.9;

    float alpha = radial * phaseOpacity * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique SacredGeometryMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SacredGeometryMainPS();
    }
}

technique SacredGeometryGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SacredGeometryGlowPS();
    }
}
