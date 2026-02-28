// ============================================================
//  ArcanePages.fx — Clockwork Grimoire (Magic Book)
//  Clair de Lune — "Flowing Arcane Script"
//
//  Pages of luminous script pour from the grimoire — flowing
//  columns of light-text that scroll and shimmer like a
//  moonlit manuscript being written in real time.
//  Two techniques: ArcanePageFlow, ArcanePageGlow
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

float4 ArcanePageFlowPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Scrolling text-line pattern — horizontal rows that scroll vertically
    float linePattern = pow(abs(sin(uv.y * 30.0 - uTime * uScrollSpeed * 2.0)), 8.0);
    float wordGaps = abs(sin(uv.x * 12.0 + uv.y * 6.0 + uTime));
    wordGaps = step(0.3, wordGaps);

    float scriptLine = linePattern * wordGaps;

    // Page surface from noise
    float pageNoise = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(0, -uTime * uScrollSpeed * 0.3);
        pageNoise = tex2D(uImage1, noiseUV).r;
    }

    // Page glow center
    float pageSurface = 1.0 - abs(uv.y - 0.5) * 2.0;
    pageSurface = pow(max(pageSurface, 0), 1.2);

    // Soft blue→pearl white gradient
    float3 inkColor = uColor.rgb;
    float3 pageColor = uSecondaryColor.rgb;
    float3 scriptGlow = float3(0.86, 0.90, 0.96); // PearlWhite tint

    float3 color = pageColor * pageSurface * 0.5;
    color += inkColor * scriptLine * 0.6;
    color += scriptGlow * scriptLine * pageNoise * 0.3;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * scriptLine * 0.4);

    float alpha = base.a * uOpacity * (pageSurface * 0.4 + scriptLine * 0.4 + 0.1);

    return float4(finalColor, alpha);
}

float4 ArcanePageGlowPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float dist = length(uv - float2(0.5, 0.5)) * 2.0;
    float glow = exp(-dist * dist * 2.0);
    float pulse = 0.5 + 0.5 * sin(uTime * 2.5);

    float3 glowColor = lerp(uColor.rgb, uSecondaryColor.rgb, 0.6) * uIntensity * uOverbrightMult;
    float alpha = base.a * uOpacity * glow * (0.15 + pulse * 0.1);

    return float4(glowColor, alpha);
}

technique ArcanePageFlow
{
    pass P0 { PixelShader = compile ps_3_0 ArcanePageFlowPS(); }
}

technique ArcanePageGlow
{
    pass P0 { PixelShader = compile ps_3_0 ArcanePageGlowPS(); }
}
