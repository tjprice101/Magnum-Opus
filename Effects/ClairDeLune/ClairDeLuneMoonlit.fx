// ============================================================
//  ClairDeLuneMoonlit.fx — Theme-wide dreamy moonlit pearl trail
//  Clair de Lune — "Moonlit Reverie"
//
//  Soft flowing water-like ribbon with pearl shimmer and moonbeam
//  highlights. Used as the shared base trail for all weapons.
//  Two techniques: MoonlitFlow (main trail), MoonlitGlow (bloom pass)
// ============================================================

sampler uImage0 : register(s0);   // primary trail texture
sampler uImage1 : register(s1);   // noise texture (SoftCircularCaustics)

// Common uniform block (matches all MagnumOpus theme shaders)
float4 uColor;              // primary color (SoftBlue default)
float4 uSecondaryColor;     // secondary color (PearlWhite default)
float  uOpacity;            // overall alpha [0..1]
float  uTime;               // elapsed time for animation
float  uIntensity;          // general brightness multiplier
float  uOverbrightMult;     // HDR bloom push
float  uScrollSpeed;        // UV scroll rate
float  uDistortionAmt;      // distortion strength
bool   uHasSecondaryTex;    // whether noise texture is bound
float  uSecondaryTexScale;  // noise UV tiling
float2 uSecondaryTexScroll; // noise UV scroll direction

// ========================  HELPERS  ========================

float2 ScrollUV(float2 uv, float speed, float2 dir)
{
    return uv + dir * uTime * speed;
}

float SoftPulse(float t, float freq, float phaseOffset)
{
    return 0.5 + 0.5 * sin(t * freq + phaseOffset);
}

// ========================  MAIN PASS  ========================

float4 MoonlitFlowPS(float2 uv : TEXCOORD0) : COLOR0
{
    // Sample primary trail texture
    float4 base = tex2D(uImage0, uv);

    // Gentle UV distortion from noise (water-like caustics)
    float2 distortedUV = uv;
    float noiseVal = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + uSecondaryTexScroll * uTime * uScrollSpeed;
        float4 noiseSample = tex2D(uImage1, noiseUV);
        noiseVal = noiseSample.r;
        distortedUV += (noiseSample.rg - 0.5) * uDistortionAmt * 0.03;
        base = tex2D(uImage0, distortedUV);
    }

    // Dreamy pearl gradient — interpolate from primary to secondary along trail length
    float gradientT = saturate(uv.x);
    float pearlShimmer = SoftPulse(uTime, 2.0, uv.y * 6.28);
    float3 dreamColor = lerp(uColor.rgb, uSecondaryColor.rgb, gradientT * 0.7 + pearlShimmer * 0.15);

    // Moonbeam highlight band through center of trail
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float moonbeam = exp(-centerDist * centerDist * 8.0);
    float3 moonbeamColor = lerp(dreamColor, float3(0.96, 0.97, 1.0), moonbeam * 0.5);

    // Soft water ripple modulation
    float ripple = SoftPulse(uTime, 3.0, uv.x * 12.0 + uv.y * 4.0) * 0.15 + 0.85;

    float3 finalColor = moonbeamColor * ripple * uIntensity;
    float alpha = base.a * uOpacity * (0.7 + moonbeam * 0.3);

    // Overbright push for bloom
    finalColor *= (1.0 + uOverbrightMult * moonbeam * 0.5);

    return float4(finalColor, alpha);
}

// ========================  GLOW PASS  ========================

float4 MoonlitGlowPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Expanded soft glow version — wider, softer
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float glowFalloff = exp(-centerDist * centerDist * 3.0);

    float pearlPulse = SoftPulse(uTime, 1.5, uv.x * 4.0);
    float3 glowColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.6 + pearlPulse * 0.2);

    float alpha = base.a * uOpacity * glowFalloff * 0.5;
    float3 finalGlow = glowColor * uIntensity * uOverbrightMult;

    return float4(finalGlow, alpha);
}

// ========================  TECHNIQUES  ========================

technique MoonlitFlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 MoonlitFlowPS();
    }
}

technique MoonlitGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 MoonlitGlowPS();
    }
}
