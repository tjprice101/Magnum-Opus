// ============================================================================
// SilentQuestionBurst.fx — TheSilentMeasure "?" explosion
// UNIQUE SIGNATURE: Visible question mark silhouette — the explosion geometry
// warps into a recognizable "?" hook shape via polar coordinate manipulation.
// The hook curve sweeps closed over the explosion duration. A dot pulses below.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float4 PS_QuestionBlast(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.45); // shifted up for ? shape
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Question mark hook: cardioid-like polar curve
    // r(theta) = a(1 - sin(theta)) — modified to create ? hook shape
    float hookAngle = angle + 1.57; // rotate so hook opens downward
    float hookR = 0.25 * (1.0 - 0.5 * sin(hookAngle)) * (1.0 - uTime * 0.3);
    // Only the upper arc portion (the hook curve)
    float hookMask = smoothstep(-0.5, 0.5, sin(hookAngle * 0.5 + 0.3));
    float hookDist = abs(dist - hookR);
    float hookWidth = 0.04 * (1.0 - uTime * 0.5);
    float hook = smoothstep(hookWidth, 0.0, hookDist) * hookMask;

    // Question mark dot (below the hook)
    float2 dotCenter = float2(0.5, 0.7);
    float dotDist = length(coords - dotCenter);
    float dotSize = 0.04 * (1.0 - uTime * 0.3);
    float dot = smoothstep(dotSize, dotSize * 0.3, dotDist);
    float dotPulse = sin(uTime * 8.0) * 0.3 + 0.7;
    dot *= dotPulse;

    // Expanding shockwave behind the ? shape
    float ringRadius = lerp(0.05, 0.5, uTime);
    float ringWidth = 0.08 * (1.0 - uTime * 0.6);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringWidth, 0.0, ringDist) * (1.0 - uTime);

    // Noise texture for energy detail
    float2 noiseUV = float2(angle * 0.4 + uTime, dist * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;
    ring *= (noise * 0.4 + 0.6);

    // Radial inner glow
    float innerGlow = smoothstep(ringRadius, 0.0, dist) * (1.0 - uTime) * 0.5;

    // Color: hook is bright green, ring is violet, dot pulses between
    float3 hookColor = uSecondaryColor * 1.8 * hook;
    hookColor += float3(0.3, 0.5, 0.3) * hook * uIntensity; // extra brightness

    float3 dotColor = lerp(uColor, uSecondaryColor, dotPulse) * dot * 2.0;

    float3 ringColor = uColor * ring;
    ringColor += uSecondaryColor * noise * ring * 0.3;

    float3 glowColor = lerp(uColor, uSecondaryColor, noise * 0.5) * innerGlow;

    float3 finalColor = hookColor + dotColor + ringColor + glowColor;

    // Initial flash
    float flash = exp(-uTime * 5.0) * uIntensity;
    finalColor += float3(1, 1, 1) * flash * 0.4;

    float alpha = saturate(hook + dot + ring + innerGlow * 0.5);
    alpha *= uOpacity * color.a;

    return float4(saturate(finalColor), alpha);
}

float4 PS_QuestionGlow(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    float glow = exp(-dist * dist * 5.0);
    float pulse = sin(uTime * 4.0) * 0.12 + 0.88;
    float3 glowColor = lerp(uSecondaryColor, uColor, dist * 1.5) * pulse;

    float timeFade = 1.0 - saturate(uTime);
    float alpha = glow * uOpacity * 0.3 * uIntensity * timeFade * color.a;

    return float4(glowColor, alpha);
}

technique SilentQuestionBlast
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_QuestionBlast();
    }
}

technique SilentQuestionGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_QuestionGlow();
    }
}
