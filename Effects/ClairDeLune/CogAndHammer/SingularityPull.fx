// ============================================================
//  SingularityPull.fx — Cog And Hammer (Ranged Launcher)
//  Clair de Lune — "Temporal Singularity Collapse"
//
//  Launches a gravitational singularity that warps light around
//  itself — a swirling vortex of deep night with a white-hot
//  core that distorts everything near it.
//  Two techniques: SingularityVortex, SingularityCore
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

float4 SingularityVortexPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center) * 2.0;
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);

    // Gravitational spiral — UV distortion toward center
    float spiral = fmod(angle / 6.2832 + dist * 2.0 - uTime * uScrollSpeed * 0.5, 1.0);
    float spiralArm = pow(abs(sin(spiral * 6.2832 * 3.0)), 4.0);

    // Accretion disk
    float diskRadius = 0.55;
    float diskWidth = 0.15;
    float diskRing = exp(-pow((dist - diskRadius) / diskWidth, 2.0) * 4.0);

    // Noise for chaotic matter
    float chaos = 0.5;
    if (uHasSecondaryTex)
    {
        float spiralDistort = angle + dist * 3.0 - uTime * 0.5;
        float2 noiseUV = float2(cos(spiralDistort) * 0.3 + 0.5, sin(spiralDistort) * 0.3 + 0.5);
        noiseUV = noiseUV * uSecondaryTexScale;
        chaos = tex2D(uImage1, noiseUV).r;
    }

    // Deep night (outer) → pearl blue (disk) → white hot (core)
    float3 deepNight = uColor.rgb;
    float3 pearlBlue = uSecondaryColor.rgb;
    float3 whiteHot = float3(0.96, 0.97, 1.0);

    float3 color = deepNight * spiralArm * (1.0 - diskRing) * chaos * 0.5;
    color += pearlBlue * diskRing * 0.7;
    color += whiteHot * exp(-dist * dist * 12.0) * 0.5;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * exp(-dist * 4.0) * 0.8);

    float alpha = base.a * uOpacity * saturate(spiralArm * 0.3 + diskRing * 0.5 + exp(-dist * 6.0) * 0.5);

    return float4(finalColor, alpha);
}

float4 SingularityCorePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float dist = length(uv - 0.5) * 2.0;

    // Intense core glow — tight central singularity
    float core = exp(-dist * dist * 16.0);

    // Event horizon ring — softened for visibility at game resolution
    float horizon = exp(-pow((dist - 0.3) * 5.0, 2.0)) * 0.6;

    // Secondary accretion ring — fainter outer shimmer
    float outerRing = exp(-pow((dist - 0.55) * 4.0, 2.0)) * 0.2;

    float pulse = 0.5 + 0.5 * sin(uTime * 5.0);
    float breathe = 0.5 + 0.5 * sin(uTime * 1.8);

    float total = core + horizon + outerRing * breathe;

    // Gradient from secondary color (outer) → white-hot (core)
    float3 whiteHot = float3(0.96, 0.97, 1.0);
    float3 coreColor = lerp(uSecondaryColor.rgb, whiteHot, core);
    coreColor = lerp(coreColor, uColor.rgb, outerRing * 0.3);
    coreColor *= uIntensity * uOverbrightMult;

    float alpha = base.a * uOpacity * total * (0.5 + pulse * 0.2);

    return float4(coreColor, alpha);
}

technique SingularityVortex
{
    pass P0 { PixelShader = compile ps_3_0 SingularityVortexPS(); }
}

technique SingularityCore
{
    pass P0 { PixelShader = compile ps_3_0 SingularityCorePS(); }
}
