// =============================================================================
// Light of the Future — Muzzle Flash Shader (ps_2_0 optimized)
// =============================================================================
// Radial gold-white flash burst at muzzle point.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;

// Muzzle flash: simple radial burst with central flash
float4 MuzzleFlashPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float2 toCenter = coords - float2(0.5, 0.5);
    float dist = length(toCenter);
    float expand = saturate(uPhase);

    // Central flash
    float flash = saturate(1.0 - dist * 3.0 / max(expand, 0.01));
    flash = flash * flash;

    // Expanding ring
    float ringDist = abs(dist - expand * 0.4);
    float ring = saturate(1.0 - ringDist * 15.0) * expand;

    // Color: gold core + white flash
    float3 color = uColor * flash + uSecondaryColor * flash * 0.5 + uColor * ring * 0.4;

    float alpha = (flash + ring * 0.2) * uOpacity * sampleColor.a * baseTex.a;

    return float4(color * uIntensity * uOverbrightMult, saturate(alpha));
}

technique MuzzleFlashMain
{
    pass P0
    {
        PixelShader = compile ps_2_0 MuzzleFlashPS();
    }
}
