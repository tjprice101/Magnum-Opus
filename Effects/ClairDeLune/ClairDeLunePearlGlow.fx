// ============================================================
//  ClairDeLunePearlGlow.fx — Theme-wide pearl bloom/glow overlay
//  Clair de Lune — "Pearl Luminescence"
//
//  Soft dreamy radial glow with pearl-like iridescent shimmer.
//  Used for bloom overlays on weapons, projectiles, and impacts.
//  Two techniques: PearlBloom (soft bloom), PearlShimmer (iridescent)
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

// ========================  PEARL BLOOM  ========================

float4 PearlBloomPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Radial falloff from center
    float2 center = uv - 0.5;
    float dist = length(center);
    float radial = exp(-dist * dist * 6.0);

    // Pearl color with gentle time-based hue shift
    float hueShift = sin(uTime * 1.2 + dist * 4.0) * 0.08;
    float3 pearlColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.5 + hueShift);

    // Moonbeam ray pattern (soft star-like rays)
    float angle = atan2(center.y, center.x);
    float rays = pow(abs(cos(angle * 3.0 + uTime * 0.5)), 8.0) * 0.3;

    float3 finalColor = pearlColor * (radial + rays) * uIntensity;
    float alpha = base.a * uOpacity * (radial * 0.8 + rays * 0.2);

    finalColor *= (1.0 + uOverbrightMult * radial);

    return float4(finalColor, alpha);
}

// ========================  PEARL SHIMMER  ========================

float4 PearlShimmerPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float2 center = uv - 0.5;
    float dist = length(center);

    // Iridescent shimmer — subtle color cycling like nacre
    float iridescentPhase = uTime * 0.8 + dist * 10.0;
    float3 iridescent;
    iridescent.r = 0.5 + 0.5 * sin(iridescentPhase);
    iridescent.g = 0.5 + 0.5 * sin(iridescentPhase + 2.094);
    iridescent.b = 0.5 + 0.5 * sin(iridescentPhase + 4.189);

    // Keep it subtle — blend mostly with the base pearl color
    float3 blended = lerp(uColor.rgb, iridescent, 0.15) * uIntensity;

    // Radial glow
    float glow = exp(-dist * dist * 4.0);
    float alpha = base.a * uOpacity * glow * 0.4;

    blended *= (1.0 + uOverbrightMult * 0.3);

    return float4(blended, alpha);
}

// ========================  TECHNIQUES  ========================

technique PearlBloom
{
    pass P0
    {
        PixelShader = compile ps_3_0 PearlBloomPS();
    }
}

technique PearlShimmer
{
    pass P0
    {
        PixelShader = compile ps_3_0 PearlShimmerPS();
    }
}
