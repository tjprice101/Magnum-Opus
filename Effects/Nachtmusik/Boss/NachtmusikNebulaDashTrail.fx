// ══════════════════════════════════════════════════════════╁E
// NachtmusikNebulaDashTrail.fx  ENachtmusik boss dash trail
// Nebula cloud trail with cosmic blue wisps and silver
// stardust particles dispersing from the movement path.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_NebulaDashTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Nebula cloud layers  Etwo scrolling noise octaves
    float2 cloudUV1 = float2(uv.x * 2.0 - uTime * 1.5, uv.y * 1.5 + uTime * 0.3);
    float cloud1 = tex2D(uNoiseTex, cloudUV1).r;
    float2 cloudUV2 = float2(uv.x * 4.0 - uTime * 2.5, uv.y * 2.0 - uTime * 0.4);
    float cloud2 = tex2D(uNoiseTex, cloudUV2).r;
    float nebula = cloud1 * 0.6 + cloud2 * 0.4;

    // Edge diffusion  Eclouds billow outward
    float edgeSoft = smoothstep(1.0, 0.3, trailWidth + (1.0 - nebula) * 0.3);

    // Stardust sparkle embedded in the trail
    float2 dustUV = float2(uv.x * 8.0 - uTime * 3.0, uv.y * 6.0);
    float dust = tex2D(uNoiseTex, dustUV).r;
    float sparkle = smoothstep(0.85, 0.95, dust);

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: cosmic deep indigo base, cosmic blue mid, silver dust
    float4 indigoBase = uColor;
    float4 cosmicBlue = float4(0.3, 0.5, 0.9, 1.0);
    float4 silverDust = float4(0.85, 0.9, 1.0, 1.0);

    float4 color = lerp(indigoBase, cosmicBlue, nebula);
    color = lerp(color, silverDust, sparkle * 0.8);

    float alpha = edgeSoft * nebula * ageFade * uTrailWidth;
    alpha += sparkle * ageFade * 0.4;

    return color * saturate(alpha);
}

technique Technique1
{
    pass NebulaDashTrail
    {
        PixelShader = compile ps_3_0 PS_NebulaDashTrail();
    }
}
