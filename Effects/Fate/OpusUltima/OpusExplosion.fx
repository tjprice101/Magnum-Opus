// =============================================================================
// Opus Ultima — Supernova Explosion Shader
// =============================================================================
// Energy ball detonation. Expanding ring of crimson-gold fire with void core,
// radial shockwave distortion, and golden glyph sparks scattering outward.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: OpusCrimson
float3 uSecondaryColor;  // Secondary: GloryGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;            // Explosion progress (0..1)

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 ExplosionPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 center = coords - 0.5;
    float dist = length(center);
    float angle = atan2(center.y, center.x);
    float phase = saturate(uPhase);

    // Expanding shockwave ring
    float ringRadius = phase * 0.5;
    float ringWidth = 0.08 + phase * 0.04;
    float ring = saturate(1.0 - abs(dist - ringRadius) / ringWidth);
    ring = ring * ring;

    // Central flash (bright at start, fades)
    float flash = saturate(1.0 - dist * 4.0) * saturate(1.0 - phase * 2.0);
    flash = flash * flash;

    // Radial fire tendrils
    float fire = SmoothNoise(float2(angle * 4.0 + uTime, dist * 10.0 - phase * 5.0));
    fire *= saturate(1.0 - dist * 2.5);

    // Glyph sparks scattered radially
    float2 sparkUV = float2(angle * 10.0, dist * 20.0) + float2(uTime * 2.0, -phase * 8.0);
    float spark = HashNoise(sparkUV);
    spark = step(0.96, spark) * saturate(1.0 - dist * 2.0);

    // Color: void center → crimson ring → gold fire → white flash
    float3 voidCol = float3(0.047, 0.02, 0.07);
    float3 crimsonCol = uColor;
    float3 goldCol = uSecondaryColor;
    float3 whiteHot = float3(0.96, 0.94, 1.0);
    float3 sparkCol = float3(1.0, 0.75, 0.16);

    float3 color = voidCol;
    color = lerp(color, crimsonCol, ring);
    color = lerp(color, goldCol, fire * 0.6);
    color = lerp(color, whiteHot, flash);
    color += sparkCol * spark * 3.0;

    float alpha = (ring * 0.5 + flash * 0.3 + fire * 0.15 + spark * 0.05);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    alpha *= saturate(1.0 - phase * 0.6); // Fade out as explosion progresses

    float3 finalColor = color * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

technique OpusExplosionMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ExplosionPS();
    }
}
