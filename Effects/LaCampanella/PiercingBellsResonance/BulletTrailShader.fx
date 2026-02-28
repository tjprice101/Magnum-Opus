// =============================================================================
// Piercing Bells Resonance — Bullet Trail Shader (Enhanced)
// =============================================================================
// High-speed bullet tracer trail. Supersonic compression-wave brightening
// near the head, phosphorus-burn afterimage that decays along length.
// Parallel speed-line echoes create a staccato repeating-fire impression.
// The tight hot core is surrounded by a warm diffused glow envelope.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1, 0));
    float c = HashNoise(i + float2(0, 1));
    float d = HashNoise(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 BulletTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // ---- Tight gaussian core + wider glow envelope ----
    float yDist = abs(coords.y - 0.5) * 2.0;
    float core = exp(-yDist * yDist / 0.02);
    float glow = exp(-yDist * yDist / 0.14);

    // ---- Supersonic compression wave at bullet head ----
    float headFlare = exp(-coords.x * 20.0);

    // ---- Speed-line echoes (staccato repeating impressions) ----
    float speedLines = SmoothNoise(float2(coords.x * 8.0 - uTime * uScrollSpeed * 4.0, coords.y * 6.0));
    speedLines = pow(saturate(speedLines), 1.5);

    // ---- Phosphorus afterburn decay ----
    float afterburn = SmoothNoise(float2(
        coords.x * 3.0 - uTime * uScrollSpeed * 2.0,
        coords.y * 10.0
    ));
    afterburn *= glow * saturate(coords.x * 2.0);

    // Secondary texture for turbulence detail
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll * 2.0;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, noiseTex.r * 0.5 + 0.6, uHasSecondaryTex);

    // ---- 5-stop colour gradient (tracer incandescence) ----
    float heat = core * (0.5 + speedLines * 0.5) + headFlare * 0.5;
    float3 cDark   = float3(0.10, 0.03, 0.01);
    float3 cEmber  = uColor * 0.5;
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.97, 0.92);

    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.2,  heat));
    color = lerp(color, cFlame,  smoothstep(0.2,  0.45, heat));
    color = lerp(color, cBright, smoothstep(0.45, 0.7,  heat));
    color = lerp(color, cWhite,  smoothstep(0.7,  1.0,  heat));

    // Head compression flash
    color = lerp(color, cWhite, headFlare * 0.8);
    // Afterburn phosphorus glow
    color += uColor * 0.25 * afterburn;

    color *= texDetail;

    // ---- Very fast trail fade (bullets are short-lived) ----
    float trailFade = smoothstep(0.0, 0.03, coords.x) * smoothstep(1.0, 0.2, coords.x);
    float staccato = sin(uTime * 20.0 + coords.x * 15.0) * 0.06 + 0.94;

    float alpha = (core * 0.5 + glow * 0.2 + headFlare * 0.2 + afterburn * 0.1)
                * trailFade * speedLines * uOpacity * staccato * baseTex.a;
    float3 finalColor = color * uIntensity * staccato * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 BulletTrailPS();
    }
}
