// =============================================================================
// MagnumOpus Lunar Beam Shader - PS 2.0 Compatible
// =============================================================================
// Crescent-shaped beam body with asymmetric cross-section.
// Unlike standard circular beams, this creates a crescent moon profile  E
// thicker on one side, tapered on the other  Efor the Moonlight Sonata theme.
//
// UV Layout:
//   U (coords.x) = position along beam length (0 = origin, 1 = tip)
//   V (coords.y) = position across beam width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   CrescentBeam    EMain beam body with crescent cross-section
//   CrescentCorona  EOuter corona glow pass (softer, wider)
//
// Features:
//   - Asymmetric cross-section (crescent moon profile via offset center)
//   - Phase parameter (0-1) for waxing/waning beam width
//   - Inner glow layer + outer corona
//   - UV-scrolling energy flow within beam body
//   - Overbright multiplier for HDR bloom
// =============================================================================

sampler uImage0 : register(s0); // Beam base texture / white gradient
sampler uImage1 : register(s1); // Noise texture for energy flow

float3 uColor;           // Primary beam color (e.g. Violet)
float3 uSecondaryColor;  // Secondary beam color (e.g. MoonWhite)
float uOpacity;          // Overall opacity
float uTime;             // Animation time
float uIntensity;        // Brightness multiplier

// Extended uniforms
float uOverbrightMult;    // HDR bloom multiplier
float uPhase;             // Lunar phase: 0.0 = new moon (thin), 1.0 = full (wide)
float uScrollSpeed;       // Energy flow scroll speed (default 1.5)
float uHasSecondaryTex;   // 1.0 if noise texture bound
float uSecondaryTexScale; // Noise UV scale
float uSecondaryTexScroll; // Noise scroll speed

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

// =============================================================================
// Crescent Cross-Section Function
// =============================================================================
// Creates a crescent moon shape across the beam width (V axis).
// Two circles: outer circle centered at V=0.5, inner circle offset
// to one side. The crescent is the difference between them.
// Phase controls how far the inner circle is offset (more offset = thinner crescent).
// =============================================================================

float CrescentMask(float v, float phase)
{
    // v: 0 = top edge, 1 = bottom edge
    // Map to centered coordinates: -1 to +1
    float centered = (v - 0.5) * 2.0;

    // Outer circle: full beam width, centered
    float outerRadius = 1.0;
    float outerDist = abs(centered) / outerRadius;
    float outerMask = saturate(1.0 - outerDist);

    // Inner circle: offset to create crescent shape
    // Phase 0 (new moon) = large offset = thin crescent
    // Phase 1 (full moon) = zero offset = full circle (no inner cutout)
    float innerOffset = (1.0 - phase) * 0.7; // Max offset 0.7 at new moon
    float innerRadius = 0.85;
    float innerCentered = centered + innerOffset;
    float innerDist = abs(innerCentered) / innerRadius;
    float innerMask = saturate(1.0 - innerDist * 1.2);

    // Crescent = outer minus inner (but clamped to 0)
    float crescent = saturate(outerMask - innerMask * (1.0 - phase * 0.8));

    return crescent;
}

// =============================================================================
// TECHNIQUE 1: CRESCENT BEAM
// =============================================================================
// Main beam body with crescent-shaped cross-section.
// Energy flows within the beam via UV-scrolling noise.
// White-hot spine with colored flanks.
// =============================================================================

float4 CrescentBeamPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Crescent cross-section mask ---
    float crescent = CrescentMask(coords.y, uPhase);

    // --- Scrolling energy flow within beam ---
    float2 flowUV = coords;
    flowUV.x -= uTime * uScrollSpeed;
    float4 noiseTex = tex2D(uImage1, flowUV * uSecondaryTexScale);
    float energyFlow = lerp(0.7, noiseTex.r, uHasSecondaryTex);

    // --- Beam spine (brightest line along crescent peak) ---
    float spineMask = saturate((crescent - 0.5) * 3.0);

    // --- Color gradient: primary at edges, white-hot at spine ---
    float3 beamColor = lerp(uColor, uSecondaryColor, crescent * 0.6);
    beamColor = lerp(beamColor, float3(1.0, 0.97, 1.0), spineMask * 0.7);

    // --- Energy flow modulation ---
    beamColor *= 0.75 + energyFlow * 0.25;

    // --- Length fade (beam tapers toward tip) ---
    float lengthFade = saturate(1.0 - coords.x * 0.3) * saturate(coords.x * 8.0);

    // --- Pulse animation ---
    float pulse = sin(uTime * 5.0 + coords.x * 8.0) * 0.08 + 0.92;

    // --- Final composition ---
    float3 finalColor = beamColor * uIntensity * pulse;
    float alpha = crescent * lengthFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor * baseTex.rgb, alpha);
}

// =============================================================================
// TECHNIQUE 2: CRESCENT CORONA
// =============================================================================
// Outer corona glow pass. Softer, wider than the main beam.
// Renders behind the main beam to create a glowing halo effect.
// The corona follows the crescent shape but is much more diffuse.
// =============================================================================

float4 CrescentCoronaPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // --- Wider, softer crescent for corona ---
    float crescent = CrescentMask(coords.y, uPhase);
    float corona = crescent * crescent; // Squared for softer falloff

    // Add extra soft halo beyond the crescent edge
    float edgeDist = 1.0 - abs(coords.y - 0.5) * 2.0;
    float halo = saturate(edgeDist * 1.2) * 0.4;
    corona = saturate(corona + halo);

    // --- Gentle noise modulation ---
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.3;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Corona color: softer, more diffuse than beam ---
    float3 coronaColor = lerp(uColor, uSecondaryColor, coords.x * 0.3);
    coronaColor *= uIntensity * noiseVal;

    // --- Length fade ---
    float lengthFade = saturate(1.0 - coords.x * 0.25) * saturate(coords.x * 5.0);

    // --- Slow pulse ---
    float pulse = sin(uTime * 3.0 + coords.x * 4.0) * 0.1 + 0.9;

    float alpha = corona * lengthFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(coronaColor * baseTex.rgb, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique CrescentBeam
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentBeamPS();
    }
}

technique CrescentCorona
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescentCoronaPS();
    }
}
