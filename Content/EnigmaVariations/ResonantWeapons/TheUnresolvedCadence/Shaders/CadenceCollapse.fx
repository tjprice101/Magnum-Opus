// ============================================================================
// CadenceCollapse.fx — TheUnresolvedCadence Paradox Collapse shader
// SpriteBatch-style (no custom vertex shader, ps_3_0 only)
// Technique:
//   CadenceCollapseWarp — Vortex-like implosion/collapse visual pulling color
//                         inward toward a singularity point. Radial UV warp +
//                         noise + time animation. The void collapse of Paradox.
// ============================================================================

sampler uImage0 : register(s0);  // Base texture (source sprite / screen capture)
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary color (cadence violet)
float3 uSecondaryColor;  // Secondary color (dimensional green)
float uOpacity;           // Overall opacity
float uTime;              // Animation progress (0→1 over collapse duration)
float uIntensity;         // Effect strength (stack count driven)

float4 PS_CollapseWarp(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Radial coordinates from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Vortex pull — distorts UV inward toward the singularity over time
    float pullStrength = uTime * uIntensity * 0.35;
    float2 pulled = toCenter * (1.0 - pullStrength * smoothstep(0.0, 0.6, 1.0 - dist));

    // Angular warp — spiral distortion tightening over time
    float spiral = uTime * 3.0 * uIntensity;
    float warpAngle = angle + spiral * smoothstep(0.0, 0.5, 1.0 - dist);
    float2 warped = float2(cos(warpAngle), sin(warpAngle)) * length(pulled) + center;

    // Noise-driven turbulence — chaotic dimensional instability
    float2 noiseUV1 = float2(angle * 0.5 + uTime * 2.0, dist * 3.0 - uTime * 1.5);
    float noise1 = tex2D(uImage1, noiseUV1).r;

    float2 noiseUV2 = float2(dist * 2.5 + uTime * 0.8, angle * 0.3 - uTime);
    float noise2 = tex2D(uImage1, noiseUV2).r;
    float combinedNoise = noise1 * 0.55 + noise2 * 0.45;

    // Apply noise distortion to warped coordinates
    float noiseDistort = 0.02 * uIntensity;
    warped += (combinedNoise - 0.5) * noiseDistort;

    // Sample the base texture with warped coordinates
    float4 baseSample = tex2D(uImage0, saturate(warped));

    // Singularity core — bright implosion point at center
    float coreGlow = exp(-dist * 8.0) * uTime * uIntensity;

    // Collapsing ring — the event horizon of the paradox
    float ringRadius = lerp(0.5, 0.05, uTime);
    float ringWidth = 0.08 * (1.0 - uTime * 0.5);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringWidth, 0.0, ringDist) * combinedNoise;

    // Color: violet core bleeding outward through green to void black
    float colorMix = saturate(1.0 - dist * 2.0 + combinedNoise * 0.25);
    float3 collapseColor = lerp(uSecondaryColor, uColor, colorMix);
    collapseColor += float3(1, 1, 1) * coreGlow * 0.6;
    collapseColor += lerp(uSecondaryColor, float3(1, 1, 1), 0.5) * ring * 0.4;

    // Outer fade — paradox bleeds away at edges
    float outerFade = smoothstep(0.55, 0.35, dist);

    float totalAlpha = saturate(outerFade * (baseSample.a * 0.5 + ring * 0.3 + coreGlow * 0.3)) * uOpacity * color.a;

    return float4(collapseColor, totalAlpha);
}

technique CadenceCollapseWarp
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_CollapseWarp();
    }
}
