// =============================================================================
// Light of the Future  EImpact Bloom Shader (ps_3_0 optimized)
// =============================================================================
// Expanding impact shockwave ring with colour split.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;

// Impact bloom: expanding ring + central flash
float4 ImpactBloomPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float dist = length(coords - float2(0.5, 0.5));
    float expand = saturate(uPhase);
    float fadeOut = 1.0 - expand;

    // Central flash
    float flash = saturate(1.0 - dist * 4.0) * fadeOut * fadeOut;

    // Shockwave ring
    float ringRadius = expand * 0.45;
    float ring = saturate(1.0 - abs(dist - ringRadius) * 20.0) * fadeOut;

    // Color: white flash + crimson ring + cyan outer hint
    float3 color = float3(0.9, 0.96, 1.0) * flash;
    color += uColor * ring * 0.6;
    color += uSecondaryColor * ring * 0.3;

    float alpha = (flash * 0.5 + ring * 0.3) * uOpacity * sampleColor.a * baseTex.a;

    return float4(color * uIntensity * uOverbrightMult, saturate(alpha));
}

technique ImpactBloomMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ImpactBloomPS();
    }
}
