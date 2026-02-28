// ============================================================================
// CadenceCollapse.fx — TheUnresolvedCadence Paradox Collapse shader
// UNIQUE SIGNATURE: Recursive geometric mandala — concentric pentagons
// spiraling inward with kaleidoscopic symmetry. Event horizon ring with
// inverted color zone. Mathematical precision, not organic chaos.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// Kaleidoscope function — fold angle into repeating sector
float kaleido(float angle, float sectors)
{
    float sector = 6.2831 / sectors;
    float a = abs(fmod(angle + sector * 0.5, sector) - sector * 0.5);
    return a;
}

float4 PS_CollapseWarp(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Kaleidoscope symmetry — fold into 5 sectors (pentagonal mandala)
    float kAngle = kaleido(angle + uTime * 0.5 * uIntensity, 5.0);

    // Recursive rings — concentric pentagons collapsing inward
    float collapse = uTime; // 0→1 over collapse duration
    float pullDist = dist / (1.0 - collapse * 0.6); // stretch radially

    // Geometric edge pattern from kaleidoscoped polar coordinates
    float geo1 = abs(sin(kAngle * 5.0 + pullDist * 20.0 - uTime * 4.0));
    float geo2 = abs(sin(kAngle * 3.0 - pullDist * 15.0 + uTime * 3.0));
    float mandala = smoothstep(0.92, 1.0, geo1) + smoothstep(0.94, 1.0, geo2) * 0.7;
    mandala = saturate(mandala);

    // Event horizon ring — the collapse boundary
    float horizonRadius = lerp(0.45, 0.03, collapse);
    float horizonWidth = 0.03 + 0.02 * (1.0 - collapse);
    float horizonDist = abs(dist - horizonRadius);
    float horizon = smoothstep(horizonWidth, 0.0, horizonDist);

    // Inside event horizon — inverted colors, reality breakdown
    float insideHorizon = smoothstep(horizonRadius + 0.02, horizonRadius - 0.02, dist);

    // Noise turbulence
    float2 noiseUV = float2(kAngle * 0.5 + uTime, pullDist * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Singularity core — white-hot collapse point
    float coreGlow = exp(-dist * dist * 40.0) * collapse * uIntensity * 2.0;

    // Color composition
    // Outside horizon: purple mandala geometry
    float3 outsideColor = uColor * mandala * 1.2;
    outsideColor += uSecondaryColor * noise * 0.2 * mandala;

    // Inside horizon: inverted — green dominates, reality is broken
    float3 insideColor = uSecondaryColor * (1.0 - mandala * 0.5) * 1.5;
    insideColor += uColor * mandala * 0.4;
    // Inversion effect
    insideColor = lerp(insideColor, 1.0 - insideColor * 0.6, insideHorizon * 0.3);

    // Event horizon ring — bright white-green
    float3 horizonColor = lerp(uSecondaryColor, float3(1, 1, 1), 0.5) * horizon * 2.0;

    // Blend inside/outside based on horizon
    float3 collapseColor = lerp(outsideColor, insideColor, insideHorizon);
    collapseColor += horizonColor;
    collapseColor += float3(1, 1, 1) * coreGlow;

    // Flash at start
    float flash = exp(-uTime * 5.0) * uIntensity * 0.4;
    collapseColor += float3(1, 1, 1) * flash;

    // Outer fade
    float outerFade = smoothstep(0.55, 0.3, dist);
    float alphaContent = mandala * 0.4 + horizon * 0.5 + coreGlow * 0.3 + insideHorizon * 0.3;
    float totalAlpha = saturate(outerFade * alphaContent) * uOpacity * color.a;

    return float4(saturate(collapseColor), totalAlpha);
}

technique CadenceCollapseWarp
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_CollapseWarp();
    }
}
