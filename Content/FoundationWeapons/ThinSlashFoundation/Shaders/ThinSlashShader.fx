// ThinSlashShader.fx
// Self-contained razor-thin slash line shader for ThinSlashFoundation.
//
// Draws an extremely thin, crisp, bright line using SDF math.
// The line has a hot white/bright core with subtle colored edge glow.
// Optimized for ps_2_0 (under 64 instructions).
//
// Pipeline:
//   1. Center UV coordinates
//   2. Rotate UV space to align with slash direction
//   3. Compute distance from the center line (perpendicular distance)
//   4. Apply length cutoff with tapered endpoints
//   5. Layer core brightness + edge color
//   6. Output with fade alpha

sampler uImage0 : register(s0);

float uTime;
float slashAngle;    // Direction of the slash in radians
float3 edgeColor;    // Outermost glow color
float3 midColor;     // Middle glow color
float3 coreColor;    // Brightest center color (usually near-white)
float fadeAlpha;     // Overall alpha (fade in/out)
float lineWidth;     // Half-width in normalized UV space (e.g. 0.015 for razor thin)
float lineLength;    // Half-length in normalized UV space (e.g. 0.45)

float4 ThinSlashPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;

    // ---- CENTER COORDINATES ----
    float2 centered = uv - 0.5;

    // ---- ROTATE TO SLASH DIRECTION ----
    float cosA = cos(-slashAngle);
    float sinA = sin(-slashAngle);
    float2 rotated;
    rotated.x = centered.x * cosA - centered.y * sinA;
    rotated.y = centered.x * sinA + centered.y * cosA;

    // ---- LINE SDF ----
    // Perpendicular distance from the horizontal center line
    float perpDist = abs(rotated.y);

    // Length cutoff — normalized distance along the line
    float alongLine = abs(rotated.x) / lineLength;

    // Tapered endpoints: slash gets thinner toward the tips
    float tipTaper = 1.0 - smoothstep(0.6, 1.0, alongLine);

    // Effective line width (tapered at ends)
    float effectiveWidth = lineWidth * tipTaper;

    // ---- CORE-TO-EDGE GRADIENT ----
    // Sharp bright core (very narrow)
    float coreWidth = effectiveWidth * 0.25;
    float coreMask = 1.0 - smoothstep(0.0, coreWidth, perpDist);

    // Mid glow (slightly wider)
    float midWidth = effectiveWidth * 0.6;
    float midMask = 1.0 - smoothstep(0.0, midWidth, perpDist);

    // Edge glow (full width — still very thin)
    float edgeMask = 1.0 - smoothstep(0.0, effectiveWidth, perpDist);

    // Length mask — cut off beyond the line endpoints
    float lengthMask = 1.0 - smoothstep(0.85, 1.0, alongLine);

    // ---- COLOR COMPOSITION ----
    float3 color = edgeColor * edgeMask * 0.6;
    color += midColor * midMask * 0.8;
    color += coreColor * coreMask * 2.0;

    // Boost brightness at the very core for that crisp bright line look
    color += coreColor * coreMask * coreMask * 1.5;

    // ---- FINAL COMPOSITE ----
    float totalMask = edgeMask * lengthMask;
    float finalAlpha = totalMask * fadeAlpha;

    return float4(color * lengthMask * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass ThinSlashPass
    {
        PixelShader = compile ps_2_0 ThinSlashPS();
    }
}
