// ============================================================
//  CrystalLance.fx — Temporal Piercer (Melee Lance/Spear)
//  Clair de Lune — "Frost-Crystal Pierce"
//
//  Sharp crystalline frost lance trail — icy geometric shards
//  that form along the thrust path then shatter outward.
//  Two techniques: CrystalLanceThrust, CrystalLanceShatter
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

float4 CrystalLanceThrustPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Crystalline shard facets — angular pattern via abs-sin combo
    float facet1 = abs(sin(uv.x * 15.0 + uv.y * 7.0 + uTime * uScrollSpeed));
    float facet2 = abs(sin(uv.x * 23.0 - uv.y * 11.0 - uTime * uScrollSpeed * 0.7));
    float crystal = pow(max(facet1, facet2), 4.0);

    // Ice-frost caustic from noise
    float frost = 0.5;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(-uTime * 0.2, uTime * 0.05);
        frost = tex2D(uImage1, noiseUV).r;
    }

    // Lance core — sharp taper toward the tip (u=1)
    float taper = uv.x * uv.x;
    float centerSharpness = 1.0 - abs(uv.y - 0.5) * 2.0;
    centerSharpness = pow(max(centerSharpness, 0), 1.5);
    float lance = taper * centerSharpness;

    // Moonlit frost palette: pearl blue core → soft blue edges → moonlit frost accents
    float3 coreColor = uColor.rgb;
    float3 edgeColor = uSecondaryColor.rgb;
    float3 frostAccent = float3(0.75, 0.86, 0.98); // MoonlitFrost

    float3 color = lerp(edgeColor, coreColor, lance);
    color += frostAccent * crystal * 0.3;
    color += float3(0.8, 0.9, 1.0) * frost * lance * 0.2;

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * lance * 0.5);

    float alpha = base.a * uOpacity * (lance * 0.7 + crystal * 0.15 + 0.15);

    return float4(finalColor, alpha);
}

float4 CrystalLanceShatterPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    // Shatter fragments radiate outward from center
    float dist = length(uv - 0.5) * 2.0;
    float angle = atan2(uv.y - 0.5, uv.x - 0.5);
    float shatterRays = pow(abs(sin(angle * 6.0 + uTime * 3.0)), 8.0);
    float radialFade = exp(-dist * dist * 3.0);

    float3 shatterColor = lerp(uColor.rgb, uSecondaryColor.rgb, shatterRays * 0.5) * uIntensity;
    shatterColor *= uOverbrightMult;
    float alpha = base.a * uOpacity * radialFade * (0.2 + shatterRays * 0.3);

    return float4(shatterColor, alpha);
}

technique CrystalLanceThrust
{
    pass P0 { PixelShader = compile ps_3_0 CrystalLanceThrustPS(); }
}

technique CrystalLanceShatter
{
    pass P0 { PixelShader = compile ps_3_0 CrystalLanceShatterPS(); }
}
