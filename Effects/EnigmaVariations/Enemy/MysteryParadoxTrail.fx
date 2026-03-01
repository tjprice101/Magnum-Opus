// MysteryParadoxTrail.fx - Paradoxical warping trail
sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

float hash(float2 p) {
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p) {
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_MysteryParadoxTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    if (base.a < 0.01) return float4(0, 0, 0, 0);

    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Paradox distortion — UV warps back on itself
    float warpX = sin(trailProgress * 6.0 + uTime * 2.0) * 0.05;
    float warpY = cos(trailProgress * 8.0 - uTime * 1.5) * 0.03;
    float2 warpedUV = uv + float2(warpX, warpY);

    // Void energy ribbons
    float ribbon1 = noise(warpedUV * 7.0 + float2(-uTime * 1.5, uTime * 0.5));
    float ribbon2 = noise(warpedUV * 12.0 + float2(uTime * 2.0, -uTime * 1.0));
    float ribbons = ribbon1 * 0.6 + ribbon2 * 0.4;

    // Paradox inversion — sections of trail appear to flow backwards
    float inversion = sin(trailProgress * 4.0 + uTime * 1.0) * 0.5 + 0.5;
    float invNoise = noise(uv * 10.0 + float2(uTime * 3.0, 0.0));

    // Arcane glyph sparkle
    float glyphs = noise(uv * 30.0 + float2(-uTime * 5.0, uTime * 2.0));
    glyphs = pow(saturate(glyphs), 6.0) * 2.5;

    // Enigma palette — deep purple / eerie green with void
    float3 voidDark = float3(0.03, 0.02, 0.06);
    float3 enigmaPurple = float3(0.4, 0.1, 0.55);
    float3 eerieGreen = float3(0.15, 0.65, 0.3);
    float3 glyphFlash = float3(0.5, 0.9, 0.55);

    float3 trailColor = lerp(voidDark, enigmaPurple, ribbons);
    trailColor = lerp(trailColor, eerieGreen, inversion * invNoise * 0.4);
    trailColor += glyphFlash * glyphs;

    float edgeFade = 1.0 - smoothstep(0.3, 0.8, trailWidth);
    float tailFade = smoothstep(0.0, 0.12, trailProgress) * smoothstep(1.0, 0.55, trailProgress);

    float alpha = edgeFade * tailFade * uIntensity;
    alpha *= (0.5 + ribbons * 0.5);

    float3 result = lerp(base.rgb, trailColor, alpha * 0.85);

    return float4(saturate(result), base.a * edgeFade * tailFade);
}

technique Technique1
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_MysteryParadoxTrail();
    }
}
