// ============================================================
//  TemporalDrill.fx — Chronologicality (Melee Drill)
//  Clair de Lune — "Time-Bore Spiral"
//
//  Spiraling temporal drill bore trail — concentric time-rip rings
//  that spin outward as the drill tears through reality.
//  Two techniques: TemporalDrillBore, TemporalDrillGlow
// ============================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 uColor;
float4 uSecondaryColor;
float  uOpacity;
float  uTime;
float  uIntensity;
float  uOverbrightMult;
float  uScrollSpeed;
float  uDistortionAmt;
bool   uHasSecondaryTex;
float  uSecondaryTexScale;
float2 uSecondaryTexScroll;

float4 TemporalDrillBorePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Spiral bore pattern — rings that scroll along the drill axis
    float spiralAngle = uv.x * 20.0 + uv.y * 8.0 - uTime * uScrollSpeed * 6.0;
    float spiralRing = pow(abs(sin(spiralAngle)), 3.0);

    // Temporal energy crackle from noise
    float noiseVal = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(-uTime * 0.3, uTime * 0.1);
        noiseVal = tex2D(uImage1, noiseUV).r;
    }

    // Core bore brightness at center
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float coreBrightness = exp(-centerDist * centerDist * 6.0);

    // Temporal crimson flare at edges
    float3 boreColor = lerp(uColor.rgb, uSecondaryColor.rgb, spiralRing * 0.6);
    float3 temporalFlare = float3(0.7, 0.3, 0.47) * (1.0 - coreBrightness) * noiseVal * 0.4;

    float3 finalColor = (boreColor * (spiralRing * 0.5 + coreBrightness * 0.8) + temporalFlare) * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * coreBrightness * 0.6);

    float alpha = base.a * uOpacity * (0.6 + spiralRing * 0.2 + coreBrightness * 0.2);

    return float4(finalColor, alpha);
}

float4 TemporalDrillGlowPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float glow = exp(-centerDist * centerDist * 2.5);

    // Faint spiral rings echo the bore pattern — ghostly afterimage of the drill
    float spiralEcho = pow(abs(sin(uv.x * 12.0 + uv.y * 4.0 - uTime * uScrollSpeed * 3.0)), 3.0) * 0.2;

    float pulse = 0.5 + 0.5 * sin(uTime * 4.0 + uv.x * 8.0);

    float3 glowColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.4) * uIntensity * uOverbrightMult;
    float3 spiralTint = float3(0.7, 0.3, 0.47) * spiralEcho * glow * uIntensity; // Temporal crimson echo
    float alpha = base.a * uOpacity * glow * (0.3 + pulse * 0.15 + spiralEcho);

    return float4(glowColor * glow + spiralTint, alpha);
}

technique TemporalDrillBore
{
    pass P0 { PixelShader = compile ps_3_0 TemporalDrillBorePS(); }
}

technique TemporalDrillGlow
{
    pass P0 { PixelShader = compile ps_3_0 TemporalDrillGlowPS(); }
}
