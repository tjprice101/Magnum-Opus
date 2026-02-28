// ============================================================
//  StarfallTrail.fx — Starfall Whisper (Ranged Sniper)
//  Clair de Lune — "Falling Star Bolt"
//
//  Each sniper bolt is a falling star — a luminous streak with
//  a bright pearl head that leaves a twinkling star-dust wake
//  trailing behind it like a comet crossing a moonlit sky.
//  Two techniques: StarfallBolt, StarfallWake
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

float4 StarfallBoltPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Bolt head — bright concentrated point at leading edge (uv.x → 1)
    float headBrightness = pow(uv.x, 4.0);
    float centerStreak = exp(-pow((uv.y - 0.5) * 2.0, 2.0) * 12.0);

    // Twinkling star dust in the wake
    float twinkle = 0.5;
    if (uHasSecondaryTex)
    {
        float2 starUV = uv * uSecondaryTexScale + float2(-uTime * uScrollSpeed, uTime * 0.1);
        twinkle = tex2D(uImage1, starUV).r;
    }
    float starPoints = pow(twinkle, 3.0) * (1.0 - uv.x); // More stars in the wake

    // Trail fade — bright at head, dimming toward tail
    float trailGrad = pow(uv.x, 1.5);

    // Moonlit frost + pearl blue core
    float3 headColor = float3(0.96, 0.97, 1.0); // WhiteHot
    float3 trailColor = uColor.rgb;
    float3 starColor = uSecondaryColor.rgb;

    float3 color = headColor * headBrightness * centerStreak;
    color += trailColor * centerStreak * trailGrad * 0.6;
    color += starColor * starPoints * 0.4;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * headBrightness * centerStreak * 0.7);

    float alpha = base.a * uOpacity * saturate(centerStreak * trailGrad + starPoints * 0.3);

    return float4(finalColor, alpha);
}

float4 StarfallWakePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Wide soft wake behind the bolt
    float wakeFade = pow(1.0 - uv.x, 2.0);
    float wakeWidth = exp(-pow((uv.y - 0.5) * 2.0, 2.0) * 3.0);
    float sparkle = 0.5;
    if (uHasSecondaryTex)
    {
        float2 sparkleUV = uv * uSecondaryTexScale * 0.5 + float2(-uTime * 0.4, 0);
        sparkle = tex2D(uImage1, sparkleUV).r;
    }

    float wake = wakeFade * wakeWidth * (0.3 + sparkle * 0.3);
    float3 wakeColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.5) * uIntensity * uOverbrightMult * 0.5;
    float alpha = base.a * uOpacity * wake * 0.3;

    return float4(wakeColor, alpha);
}

technique StarfallBolt
{
    pass P0 { PixelShader = compile ps_3_0 StarfallBoltPS(); }
}

technique StarfallWake
{
    pass P0 { PixelShader = compile ps_3_0 StarfallWakePS(); }
}
