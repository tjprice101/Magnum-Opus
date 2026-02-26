// =====================================================================
//  FlockAura.fx — Feather of the Iridescent Flock player aura
// =====================================================================
//
//  Visual: Radial aura overlay rendered behind the player when crystals
//  are active. Concentric prismatic rings with 3 formation node positions
//  at 120-degree spacing that glow based on active crystal count.
//  Subtle equilateral triangle geometry visible in the pattern.
//
//  uPhase encodes crystal count: 0.33 = 1 crystal, 0.66 = 2, 1.0 = 3
//  Each formation node lights up progressively.
//
//  ps_3_0 compatible. Two techniques:
//    FlockAuraMain  — Formation nodes + concentric rings
//    FlockAuraGlow  — Soft prismatic bloom underlay
// =====================================================================

sampler uImage0 : register(s0);    // Primary texture (soft glow / circular mask)
sampler uImage1 : register(s1);    // Noise texture (CosmicEnergyVortex)

float4 uColor;                      // Primary color (Silver)
float4 uSecondaryColor;             // Secondary color (PureWhite)
float  uOpacity;                    // Overall opacity
float  uTime;                       // Animation time
float  uIntensity;                  // Brightness multiplier
float  uOverbrightMult;             // HDR overbright
float  uScrollSpeed;                // Ring expansion speed
float  uNoiseScale;                 // Noise detail
float  uDistortionAmt;              // Formation distortion
bool   uHasSecondaryTex;            // Secondary texture bound
float  uSecondaryTexScale;          // Noise UV scale
float  uSecondaryTexScroll;         // Noise scroll
float  uPhase;                      // Crystal count encoded: 0.33/0.66/1.0

// =====================================================================
//  Helpers
// =====================================================================

float2 ToPolar(float2 uv)
{
    float2 centered = uv - 0.5;
    float r = length(centered);
    float theta = atan2(centered.y, centered.x);
    return float2(r, theta);
}

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

// Formation node — bright spot at specific angle
float FormationNode(float theta, float nodeAngle, float nodeWidth)
{
    float angleDist = abs(theta - nodeAngle);
    // Wrap around at pi boundary
    angleDist = min(angleDist, 6.28318 - angleDist);
    return 1.0 - smoothstep(0.0, nodeWidth, angleDist);
}

// =====================================================================
//  Main aura — formation nodes + concentric rings
// =====================================================================

float4 PS_FlockAuraMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y + 3.14159; // Shift so 0 is at top

    float crystalCount = uPhase * 3.0; // 0-3

    // === CONCENTRIC RINGS ===
    // Slowly expanding prismatic rings
    float ringPhase = r * 8.0 - uTime * uScrollSpeed * 0.3;
    float ringPattern = sin(ringPhase) * 0.5 + 0.5;
    ringPattern = pow(ringPattern, 3.0); // Sharpen rings

    // Per-ring hue cycling
    float ringHue = frac(r * 2.0 + uTime * 0.06);
    float3 ringColor = HSLToRGB(ringHue, 0.5, 0.7);

    // === FORMATION NODES ===
    // 3 nodes at 120-degree spacing (equilateral triangle)
    float nodeRadius = 0.18 + 0.02 * sin(uTime * 1.5);
    float nodeWidth = 0.3; // Angular width

    float node1 = FormationNode(theta, 0.0, nodeWidth);              // Top
    float node2 = FormationNode(theta, 2.09440, nodeWidth);           // Bottom-right (120 deg)
    float node3 = FormationNode(theta, 4.18879, nodeWidth);           // Bottom-left (240 deg)

    // Radial mask — nodes glow at specific radius
    float nodeRadialMask = 1.0 - smoothstep(nodeRadius - 0.04, nodeRadius + 0.06, abs(r - nodeRadius));

    // Enable nodes based on crystal count
    float nodeGlow = 0.0;
    if (crystalCount >= 0.9) nodeGlow += node1;
    if (crystalCount >= 1.9) nodeGlow += node2;
    if (crystalCount >= 2.9) nodeGlow += node3;
    nodeGlow *= nodeRadialMask;

    // Per-node colors (warm, green, cool)
    float3 nodeColor1 = HSLToRGB(0.05, 0.8, 0.7);  // Warm gold
    float3 nodeColor2 = HSLToRGB(0.35, 0.8, 0.65);  // Emerald
    float3 nodeColor3 = HSLToRGB(0.7, 0.8, 0.7);    // Violet-blue

    float3 nodeContrib = float3(0, 0, 0);
    if (crystalCount >= 0.9) nodeContrib += nodeColor1 * node1 * nodeRadialMask;
    if (crystalCount >= 1.9) nodeContrib += nodeColor2 * node2 * nodeRadialMask;
    if (crystalCount >= 2.9) nodeContrib += nodeColor3 * node3 * nodeRadialMask;

    // === TRIANGLE GEOMETRY ===
    // Subtle equilateral triangle connecting the 3 nodes
    float2 centered = uv - 0.5;
    float triangleDist = 0.0;
    if (crystalCount >= 2.9)
    {
        // Three lines connecting nodes — use angular distance to edge lines
        float edge1 = abs(centered.y + centered.x * 0.577) - 0.005; // Approximate triangle edges
        float edge2 = abs(centered.y - centered.x * 0.577) - 0.005;
        float edge3 = abs(centered.y - 0.09) - 0.005;
        float minEdge = min(min(edge1, edge2), edge3);
        triangleDist = smoothstep(0.008, 0.0, minEdge) * 0.15;
    }

    // === NOISE MODULATION ===
    float noiseSwirl = 0.0;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale;
        noiseUV += float2(uTime * uSecondaryTexScroll * 0.1, 0.0);
        noiseSwirl = tex2D(uImage1, noiseUV).r;
        noiseSwirl = smoothstep(0.4, 0.7, noiseSwirl) * 0.15;
    }

    // === COLOR COMPOSITE ===
    float radialFade = exp(-r * r * 12.0);
    float3 baseColor = lerp(uColor.rgb, ringColor, ringPattern * 0.5);
    baseColor *= radialFade;

    // Add node glows
    baseColor += nodeContrib * 0.8;

    // Add triangle hint
    baseColor += uSecondaryColor.rgb * triangleDist * radialFade;

    // Add noise swirl
    float3 swirlColor = HSLToRGB(frac(theta / 6.28318 + uTime * 0.05), 0.4, 0.7);
    baseColor += swirlColor * noiseSwirl * radialFade;

    // Breathing pulse — waltz time
    float pulse = 0.9 + 0.1 * sin(uTime * 2.0);
    baseColor *= pulse;

    float3 finalColor = baseColor * uIntensity * 0.6;
    float finalAlpha = baseTex.a * uOpacity * color.a *
                       saturate(radialFade * 0.5 + ringPattern * 0.2 + nodeGlow * 0.4 + triangleDist);
    finalAlpha *= 0.4; // Keep aura subtle

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Glow pass — soft prismatic bloom
// =====================================================================

float4 PS_FlockAuraGlow(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 polar = ToPolar(uv);
    float r = polar.x;
    float theta = polar.y;

    // Soft radial glow
    float radialGlow = exp(-r * r * 8.0);

    // Prismatic tint based on angle
    float hue = frac(theta / 6.28318 + uTime * 0.03);
    float3 glowColor = HSLToRGB(hue, 0.3, 0.75);

    // Blend toward base silver
    glowColor = lerp(uColor.rgb, glowColor, 0.35);

    // Breathing pulse
    float pulse = 0.88 + 0.12 * sin(uTime * 1.5);

    float3 finalColor = glowColor * uIntensity * 0.3;
    float finalAlpha = baseTex.a * uOpacity * color.a * radialGlow * 0.25 * pulse;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// =====================================================================
//  Techniques
// =====================================================================

technique FlockAuraMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_FlockAuraMain();
    }
}

technique FlockAuraGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_FlockAuraGlow();
    }
}
