// ============================================================
//  JudgmentMark.fx — Gear-Driven Arbiter (Summon Sentry)
//  Clair de Lune — "Clockwork Judgment Sigil"
//
//  The sentry brands targets with a clockwork judgment mark —
//  a rotating brass sigil of interlocking gear rings that
//  tightens over time before detonating.
//  Two techniques: JudgmentMarkSigil, JudgmentMarkDetonate
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
float  uPhase; // 0=placed → 1=detonating

float4 JudgmentMarkSigilPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center) * 2.0;
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);

    // Outer gear ring (12 teeth)
    float outerRadius = 0.8 - uPhase * 0.15; // Tightens as phase increases
    float outerRing = exp(-pow((dist - outerRadius) * 12.0, 2.0));
    float teeth = step(0.5, abs(sin(angle * 12.0 + uTime * uScrollSpeed)));
    float outerGear = outerRing * (0.6 + teeth * 0.4);

    // Inner gear ring (8 teeth, counter-rotating)
    float innerRadius = 0.45 - uPhase * 0.1;
    float innerRing = exp(-pow((dist - innerRadius) * 14.0, 2.0));
    float innerTeeth = step(0.5, abs(sin(angle * 8.0 - uTime * uScrollSpeed * 1.5)));
    float innerGear = innerRing * (0.6 + innerTeeth * 0.4);

    // Center judgment eye
    float eye = exp(-dist * dist * 20.0);
    float eyePulse = 0.5 + 0.5 * sin(uTime * 4.0 + uPhase * 10.0);

    // Noise for ethereal fill
    float sigNoise = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(sin(uTime * 0.3) * 0.1, cos(uTime * 0.3) * 0.1);
        sigNoise = tex2D(uImage1, noiseUV).r;
    }

    // Clockwork brass (gears) + temporal crimson (judgment) + white (core)
    float3 brassColor = uColor.rgb;
    float3 crimsonColor = uSecondaryColor.rgb;
    float3 whiteHot = float3(0.96, 0.97, 1.0);

    float3 color = brassColor * (outerGear + innerGear) * 0.5;
    color += crimsonColor * eye * eyePulse * 0.6;
    color += whiteHot * eye * pow(uPhase, 2.0) * 0.4;
    color += brassColor * sigNoise * (outerRing + innerRing) * 0.1;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * (eye * 0.4 + uPhase * 0.3));

    float alpha = base.a * uOpacity * saturate(outerGear + innerGear + eye * 0.8);

    return float4(finalColor, alpha);
}

float4 JudgmentMarkDetonatePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float dist = length(uv - 0.5) * 2.0;
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);

    // Expanding shockwave ring
    float ringRadius = uPhase * 0.9;
    float ring = exp(-pow((dist - ringRadius) * 10.0, 2.0));

    // Radial blast rays
    float rays = pow(abs(sin(angle * 8.0 + uTime * 6.0)), 6.0);
    float blast = ring * (0.5 + rays * 0.5);

    float3 blastColor = lerp(uSecondaryColor.rgb, float3(0.96, 0.97, 1.0), ring * 0.5) * uIntensity * uOverbrightMult;
    float alpha = base.a * uOpacity * blast * (1.0 - uPhase * 0.3);

    return float4(blastColor, alpha);
}

technique JudgmentMarkSigil
{
    pass P0 { PixelShader = compile ps_3_0 JudgmentMarkSigilPS(); }
}

technique JudgmentMarkDetonate
{
    pass P0 { PixelShader = compile ps_3_0 JudgmentMarkDetonatePS(); }
}
