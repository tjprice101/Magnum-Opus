// ============================================================================
// CipherSnapBack.fx — CipherNocturne snap-back implosion
// UNIQUE SIGNATURE: Clock-face sector starburst — the circle divides into
// angular wedge segments that light up in rapid sequence (like a cipher wheel
// spinning). Each sector contracts independently with slight time offset.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float4 PS_SnapBack(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x); // -PI to PI

    // Divide circle into sectors (cipher wheel segments)
    float sectorCount = 12.0;
    float sectorAngle = 6.2831 / sectorCount;
    float sectorIndex = floor((angle + 3.14159) / sectorAngle);
    float sectorFrac = frac((angle + 3.14159) / sectorAngle);

    // Each sector contracts with a time offset based on its index
    // Sectors activate in sequence — rapid spinning illumination
    float sectorDelay = sectorIndex / sectorCount * 0.4;
    float localTime = saturate((uTime - sectorDelay) / (1.0 - sectorDelay * 0.5));

    // Sector contraction — each wedge's outer ring collapses at its own pace
    float sectorRadius = lerp(0.48, 0.0, localTime);
    float sectorWidth = 0.1 * (1.0 - localTime);
    float sectorRing = smoothstep(sectorWidth, 0.0, abs(dist - sectorRadius));

    // Sector edge lines — bright borders between wedges
    float sectorEdge = smoothstep(0.06, 0.0, min(sectorFrac, 1.0 - sectorFrac));
    sectorEdge *= smoothstep(0.0, 0.3, dist) * (1.0 - localTime);

    // Active sector illumination — sectors light up in sequence
    float activePhase = frac(uTime * 3.0);
    float sectorActive = smoothstep(0.08, 0.0, abs(frac(sectorIndex / sectorCount) - activePhase));
    sectorActive *= (1.0 - uTime); // Fade as collapse completes

    // Inner glyph flash — cipher decoded at center
    float glyphFlash = exp(-dist * dist * 30.0) * localTime * uIntensity;

    // Noise for texture
    float2 noiseUV = float2(angle * 0.5 + uTime * 2.0, dist * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;
    sectorRing *= (noise * 0.4 + 0.6);

    // Color: sectors alternate between purple and green cipher colors
    float sectorParity = fmod(sectorIndex, 2.0);
    float3 sectorColor = lerp(uColor, uSecondaryColor, sectorParity * 0.7);

    // Active sector is brighter
    sectorColor += float3(0.3, 0.5, 0.3) * sectorActive * uIntensity;

    float3 finalColor = sectorColor * sectorRing;
    finalColor += lerp(uSecondaryColor, float3(1, 1, 1), 0.4) * sectorEdge * 0.6;
    finalColor += float3(1, 1, 1) * glyphFlash;

    // Initial flash
    float flash = exp(-uTime * 6.0) * uIntensity * 0.3;
    finalColor += float3(1, 1, 1) * flash;

    float alpha = saturate(sectorRing + sectorEdge * 0.4 + glyphFlash) * uOpacity * color.a;

    return float4(saturate(finalColor), alpha);
}

technique CipherSnapBackMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_SnapBack();
    }
}
