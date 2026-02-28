// =============================================================================
// Fractal of the Stars — Orbit Glow Shader
// =============================================================================
// Soft pulsing glow for orbiting spectral star blades.
// Celestial halo with prismatic shimmer and star-point accents.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: StarGold
float3 uSecondaryColor;  // Secondary: NebulaPink
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;

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

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }

float4 OrbitGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Soft halo glow
    float halo = saturate(1.0 - cross);
    halo = halo * halo;

    // Pulsing intensity
    float pulse = sin(uTime * 3.0 + progress * 5.0) * 0.2 + 0.8;

    // Prismatic color shift along trail
    float prismShift = frac(progress * 2.0 + uTime * 0.5);
    float3 prismColor = lerp(uColor, uSecondaryColor, sin(prismShift * 3.14159) * 0.5 + 0.5);

    // Star-point accents
    float2 starUV = coords * float2(12.0, 4.0) + uTime * 0.8;
    float starPoint = HashNoise(floor(starUV));
    starPoint = step(0.88, starPoint) * halo;
    float starTwinkle = sin(uTime * 6.0 + starPoint * 30.0) * 0.4 + 0.6;
    starPoint *= starTwinkle;

    // Subtle inner nebula glow
    float nebula = SmoothNoise(coords * float2(8.0, 4.0) + uTime * 0.3);
    nebula = nebula * nebula * halo * 0.3;

    float3 color = prismColor * halo * pulse;
    color += float3(1.0, 1.0, 0.94) * starPoint * 1.5;
    color += uSecondaryColor * nebula;

    float alpha = (halo * 0.5 + starPoint * 0.3 + nebula * 0.2) * pulse;
    alpha *= (1.0 - progress * 0.3) * uOpacity * sampleColor.a * baseTex.a;

    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique OrbitGlowMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 OrbitGlowPS();
    }
}
