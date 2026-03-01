// ============================================================
//  SoulBeam.fx — Lunar Phylactery (Summon Staff)
//  Clair de Lune — "Moonlit Soul Tether"
//
//  The phylactery channels soul energy as luminous beams that
//  connect the summoned entity to its targets — pearl shimmer
//  tethers with soft moonlit frost pulsing through them.
//  Two techniques: SoulBeamTether, SoulBeamAura
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

float4 SoulBeamTetherPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Beam core — tight center band
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float beamCore = exp(-centerDist * centerDist * 10.0);
    float beamOuter = exp(-centerDist * centerDist * 3.0);

    // Soul energy pulses traveling along the beam
    float pulse = sin(uv.x * 15.0 - uTime * uScrollSpeed * 4.0);
    float soulPulse = pow(max(pulse, 0), 4.0);

    // Ethereal shimmer from noise
    float shimmer = 0.5;
    if (uHasSecondaryTex)
    {
        float2 shimUV = uv * uSecondaryTexScale + float2(-uTime * uScrollSpeed * 0.5, uTime * 0.05);
        shimmer = tex2D(uImage1, shimUV).r;
    }

    // Pearl shimmer core → moonlit frost outer
    float3 pearlShimmer = uColor.rgb;
    float3 moonlitFrost = uSecondaryColor.rgb;
    float3 whiteHot = float3(0.96, 0.97, 1.0);

    float3 color = moonlitFrost * beamOuter * 0.4;
    color += pearlShimmer * beamCore * 0.6;
    color += whiteHot * beamCore * soulPulse * 0.4;
    color += pearlShimmer * shimmer * beamOuter * 0.15;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * beamCore * soulPulse * 0.5);

    float alpha = base.a * uOpacity * (beamOuter * 0.5 + beamCore * soulPulse * 0.3 + 0.1);

    return float4(finalColor, alpha);
}

float4 SoulBeamAuraPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Soft radial aura around the phylactery
    float dist = length(uv - 0.5) * 2.0;
    float aura = exp(-dist * dist * 2.0);
    float breathe = 0.5 + 0.5 * sin(uTime * 1.5);

    // Soul wisp tendrils — anisotropic falloff creates wispy upward shapes
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);
    float upBias = max(0, -sin(angle)); // Stronger upward
    float tendril = pow(max(sin(angle * 3.0 + uTime * 2.0), 0.0), 6.0) * upBias;
    float wispFade = exp(-dist * 1.5) * tendril * 0.4;

    // Gentle moonlit shimmer
    float shimmer = 0.5;
    if (uHasSecondaryTex)
    {
        float2 shimUV = uv * uSecondaryTexScale + float2(uTime * 0.02, -uTime * 0.04);
        shimmer = tex2D(uImage1, shimUV).r;
    }

    float3 auraColor = lerp(uColor.rgb, uSecondaryColor.rgb, shimmer * 0.4);
    auraColor *= uIntensity * uOverbrightMult * 0.5;

    // Wisp tendrils use a slightly warmer pearl tint
    float3 wispColor = uSecondaryColor.rgb * uIntensity * uOverbrightMult;

    float3 finalColor = auraColor * aura * (0.15 + breathe * 0.1) + wispColor * wispFade;
    float alpha = base.a * uOpacity * (aura * (0.18 + breathe * 0.12) + wispFade);

    return float4(finalColor, alpha);
}

technique SoulBeamTether
{
    pass P0 { PixelShader = compile ps_3_0 SoulBeamTetherPS(); }
}

technique SoulBeamAura
{
    pass P0 { PixelShader = compile ps_3_0 SoulBeamAuraPS(); }
}
