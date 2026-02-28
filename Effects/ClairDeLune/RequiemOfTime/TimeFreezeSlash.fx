// ============================================================
//  TimeFreezeSlash.fx — Requiem Of Time (Magic Ultimate)
//  Clair de Lune — "Reality Fracture Sweep"
//
//  When time itself is the weapon — the slash rips a seam in
//  space that fragments reality into shattered glass panels,
//  each shard reflecting a different moment frozen in time.
//  Two techniques: TimeFreezeSlash, TimeFreezeCrack
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

float4 TimeFreezeSlashPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Main slash energy — sharp diagonal sweep
    float sweepAngle = uv.x * 2.0 - uv.y;
    float slashCore = exp(-sweepAngle * sweepAngle * 8.0);

    // Fracture lines — Voronoi-inspired shattering from noise
    float fracture = 0.5;
    float shardPattern = 0.5;
    if (uHasSecondaryTex)
    {
        float2 fracUV = uv * uSecondaryTexScale * 1.5;
        fracture = tex2D(uImage1, fracUV).r;

        // Scrolling energy within cracks
        float2 energyUV = uv * uSecondaryTexScale + float2(-uTime * uScrollSpeed, uTime * uScrollSpeed * 0.3);
        shardPattern = tex2D(uImage1, energyUV).r;
    }

    // Edge detection on fracture = crack lines
    float crackIntensity = pow(1.0 - fracture, 6.0) * slashCore;

    // Temporal crimson at crack edges, pearl blue in shard bodies
    float3 crackColor = float3(0.7, 0.31, 0.47); // TemporalCrimson
    float3 shardColor = uColor.rgb;
    float3 whiteFlash = float3(0.96, 0.97, 1.0); // WhiteHot

    float3 color = shardColor * slashCore * shardPattern * 0.5;
    color += crackColor * crackIntensity * 0.8;
    color += whiteFlash * pow(slashCore, 3.0) * 0.4;

    // Distortion hint at edges
    float edgeDistort = (1.0 - abs(uv.y - 0.5) * 2.0) * slashCore * uDistortionAmt;
    color += uSecondaryColor.rgb * edgeDistort * 0.2;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * pow(slashCore, 2.0) * 0.6);

    float alpha = base.a * uOpacity * saturate(slashCore * 0.7 + crackIntensity * 0.3);

    return float4(finalColor, alpha);
}

float4 TimeFreezeCrackPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Spreading crack network from center
    float dist = length(uv - 0.5) * 2.0;
    float radialCrack = 0.5;
    if (uHasSecondaryTex)
    {
        float2 crackUV = uv * uSecondaryTexScale * 2.0;
        radialCrack = tex2D(uImage1, crackUV).r;
    }

    float cracks = pow(1.0 - radialCrack, 8.0) * exp(-dist * 2.0);
    float3 crackGlow = float3(0.7, 0.31, 0.47) * uIntensity * uOverbrightMult;
    float alpha = base.a * uOpacity * cracks * 0.5;

    return float4(crackGlow, alpha);
}

technique TimeFreezeSlash
{
    pass P0 { PixelShader = compile ps_3_0 TimeFreezeSlashPS(); }
}

technique TimeFreezeCrack
{
    pass P0 { PixelShader = compile ps_3_0 TimeFreezeCrackPS(); }
}
