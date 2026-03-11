// =============================================================================
// Fractal of the Stars — Swing Trail Shader
// =============================================================================
// RECURSIVE CONSTELLATION GEOMETRY: Clean, geometric, precise. Stars connected
// by hard constellation lines that recursively subdivide into smaller patterns.
// Grid-based fractal tessellation with pulsing star nodes. The mathematical
// beauty of the cosmos made blade-sharp.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uGradientLUT : register(s2);

float3 uColor;           // Primary: StarGold
float3 uSecondaryColor;  // Secondary: FractalPurple
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uPhase;            // Combo intensity (0..1)
float uHasSecondaryTex;
float uSecondaryTexScale;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

// --- Fractal grid: returns distance to nearest grid line at multiple scales ---
float FractalGrid(float2 uv, float time)
{
    float minDist = 1.0;
    float totalLine = 0.0;

    // 3 recursive subdivision levels
    [unroll] for (int level = 0; level < 3; level++)
    {
        float scale = 4.0 * pow(2.5, level);
        float2 gridUV = uv * scale;
        float2 gridCell = frac(gridUV);

        // Distance to nearest grid line (horizontal + vertical)
        float dH = min(gridCell.y, 1.0 - gridCell.y);
        float dV = min(gridCell.x, 1.0 - gridCell.x);
        float dLine = min(dH, dV);

        // Each level is thinner and dimmer
        float lineWidth = 0.06 / (1.0 + level * 0.8);
        float lineIntensity = 0.7 / (1.0 + level * 0.5);

        // Phase shift per level for animation
        float phaseOffset = time * (0.3 + level * 0.15);
        float cellHash = HashNoise(floor(gridUV) + level * 37.0);
        float activation = smoothstep(0.3, 0.7, sin(phaseOffset + cellHash * 6.28));

        totalLine += smoothstep(lineWidth, 0.0, dLine) * lineIntensity * activation;
        minDist = min(minDist, dLine);
    }

    return totalLine;
}

// --- Constellation star nodes: bright points at grid intersections ---
float StarNodes(float2 uv, float time)
{
    float totalStars = 0.0;

    [unroll] for (int level = 0; level < 2; level++)
    {
        float scale = 4.0 * pow(2.5, level);
        float2 gridUV = uv * scale;
        float2 gridCell = frac(gridUV);
        float2 cellID = floor(gridUV);

        // Star at intersection (near corners of cell)
        float dCorner = length(gridCell - 0.5);
        float cornerDist = min(min(
            length(gridCell),
            length(gridCell - float2(1, 0))
        ), min(
            length(gridCell - float2(0, 1)),
            length(gridCell - float2(1, 1))
        ));

        // Not every intersection has a star
        float starChance = HashNoise(cellID + level * 17.0);
        float hasStar = step(0.55, starChance);

        // Pulsing: each star pulses at different phase
        float pulse = sin(time * 3.0 + starChance * 12.566) * 0.3 + 0.7;

        float starGlow = smoothstep(0.12, 0.0, cornerDist) * hasStar * pulse;
        float starPoint = smoothstep(0.03, 0.0, cornerDist) * hasStar; // bright core

        totalStars += (starGlow * 0.6 + starPoint * 1.5) / (1.0 + level * 0.4);
    }

    return totalStars;
}

// --- Constellation connecting lines between star nodes ---
float ConstellationLines(float2 uv, float time)
{
    float scale = 4.0;
    float2 gridUV = uv * scale;
    float2 cellID = floor(gridUV);
    float2 cellPos = frac(gridUV);

    // Check if this cell connects to neighbors
    float h1 = HashNoise(cellID);
    float h2 = HashNoise(cellID + float2(1, 0));
    float h3 = HashNoise(cellID + float2(0, 1));

    float lines = 0.0;

    // Horizontal connection
    if (h1 + h2 > 1.0)
    {
        float dLine = abs(cellPos.y - 0.5 * (h1 + h2));
        float traveling = sin(cellPos.x * 6.28 - time * 4.0) * 0.03;
        lines += smoothstep(0.04, 0.0, abs(dLine + traveling)) * 0.5;
    }

    // Diagonal connection
    if (h1 + h3 > 1.1)
    {
        float dDiag = abs(cellPos.y - cellPos.x * (h3 / max(h1, 0.01)));
        float diagPulse = sin(time * 2.5 + cellPos.x * 4.0) * 0.02;
        lines += smoothstep(0.035, 0.0, abs(dDiag + diagPulse)) * 0.4;
    }

    return lines;
}

// Main swing trail: geometric fractal constellation
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float combo = saturate(uPhase);

    // Scrolling UV for the fractal field
    float2 fieldUV = float2(
        progress * uNoiseScale - uTime * uScrollSpeed * 0.4,
        coords.y * 2.0
    );

    // --- Fractal grid lines ---
    float grid = FractalGrid(fieldUV, uTime);

    // --- Star nodes at intersections ---
    float stars = StarNodes(fieldUV, uTime);

    // --- Constellation connecting lines ---
    float connections = ConstellationLines(fieldUV, uTime);

    // --- Nebula backdrop: soft underlying glow ---
    float nebula = smoothstep(1.0, 0.0, cross);
    nebula = nebula * nebula;
    float nebulaFlow = HashNoise(coords * float2(8.0, 3.0) - uTime * 0.3);
    nebula *= (0.5 + nebulaFlow * 0.5);

    // --- Leading edge: brightest at the newest point ---
    float leading = saturate(1.0 - progress * 1.8);
    leading = leading * leading;

    // --- Secondary texture ---
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.2, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.2);

    // --- Color composition: deep space → purple nebula → gold grid → white stars ---
    float3 deepSpace = float3(0.02, 0.01, 0.06);
    float3 purpleNeb = uSecondaryColor;
    float3 goldGrid = uColor;
    float3 whiteHot = float3(1.0, 0.98, 0.92);
    float3 starBlue = float3(0.7, 0.8, 1.0);

    float3 color = lerp(deepSpace, purpleNeb * 0.4, nebula);
    color += goldGrid * grid * (0.6 + combo * 0.8);
    color += starBlue * connections * 0.8;
    color += whiteHot * stars * (1.0 + combo * 0.5);
    color = lerp(color, goldGrid * 1.5, leading * nebula * 0.3);
    color *= detail;

    // The overall brightness is moderate — mostly dark space with sharp bright geometry
    float alpha = (nebula * 0.3 + grid * 0.3 + stars * 0.25 + connections * 0.15);
    alpha *= (1.0 - progress * 0.35);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    // Fate LUT color toning — subtle theme cohesion
    float lum = dot(finalColor, float3(0.299, 0.587, 0.114));
    float3 lutColor = tex2D(uGradientLUT, float2(saturate(lum), 0.5)).rgb;
    finalColor = lerp(finalColor, lutColor * finalColor * 2.0, 0.25);

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow: purple nebula cloud beneath the geometry
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = exp(-cross * cross * 2.0);
    float pulse = sin(uTime * 2.0 + progress * 5.0) * 0.12 + 0.88;

    // Subtle star shimmer in the nebula glow
    float shimmer = HashNoise(coords * float2(20.0, 8.0) + uTime * 0.3);
    shimmer = step(0.93, shimmer) * glow;

    float3 glowColor = lerp(float3(0.02, 0.01, 0.06), uSecondaryColor * 0.35, glow * 0.5);
    glowColor += uColor * shimmer * 2.5; // Occasional gold star sparks in glow

    float alpha = glow * (1.0 - progress * 0.45) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.4;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique FractalSwingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique FractalSwingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
