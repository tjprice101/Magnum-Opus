// =============================================================================
// Symphonic Bellfire Annihilator  ERocket Trail Shader (Enhanced)
// =============================================================================
// Thick, turbulent exhaust plume for rocket projectiles. Full FBM smoke
// billowing with embedded flame pockets. Exhaust ring nodes form periodic
// bright compression rings behind the nozzle. A nozzle shock-diamond
// flares at the base. Ember cascade scatters through the expanding plume.
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

float FBM(float2 p)
{
    float v = 0.0;
    v += SmoothNoise(p) * 0.5;
    v += SmoothNoise(p * 2.03 + 1.7) * 0.25;
    v += SmoothNoise(p * 4.01 + 3.3) * 0.125;
    v += SmoothNoise(p * 7.97 + 5.1) * 0.0625;
    return v / 0.9375;
}

float4 RocketTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // ---- Expanding plume cross-section ----
    float progress = coords.x;
    float plumeWidth = lerp(0.25, 1.0, progress);
    float yDist = abs(coords.y - 0.5) * 2.0;
    float plumeCore = exp(-yDist * yDist / (plumeWidth * plumeWidth * 0.2));
    float plumeEnv  = exp(-yDist * yDist / (plumeWidth * plumeWidth * 0.6));

    // ---- FBM smoke turbulence (heavy billowing) ----
    float2 scrollUV = float2(
        coords.x * uNoiseScale * 2.0 - uTime * uScrollSpeed,
        coords.y * 3.0
    );
    float turb = FBM(scrollUV * 3.0);

    // ---- Flame pockets embedded in smoke ----
    float flameLine = smoothstep(0.55, 0.75, turb) * plumeCore;

    // ---- Exhaust ring nodes (periodic compression brightening) ----
    float rings = pow(saturate(cos(coords.x * 8.0 - uTime * uScrollSpeed * 1.5) * 0.5 + 0.5), 4.0);
    rings *= plumeCore * saturate(1.0 - progress * 0.8);

    // ---- Nozzle shock-diamond at exhaust base ----
    float nozzleFlare = exp(-coords.x * 15.0);
    float nozzleDiamond = nozzleFlare * plumeCore;

    // ---- Ember cascade scatter ----
    float2 emberUV = coords * float2(20.0, 10.0) + float2(uTime * uScrollSpeed * 2.0, 0.0);
    float embers = HashNoise(emberUV);
    embers = pow(saturate(embers - 0.92) * 12.5, 2.0) * plumeEnv * saturate(1.0 - progress * 0.6);

    // Secondary texture for extra detail
    float2 secUV = float2(coords.x * uSecondaryTexScale - uTime * 0.3, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float secVal = lerp(1.0, secTex.r * 0.5 + 0.6, uHasSecondaryTex * 0.3);

    // ---- 5-stop colour gradient: nozzle ↁEflame ↁEsmoke ----
    float3 cWhite  = float3(1.0, 0.96, 0.88);
    float3 cBright = uSecondaryColor;
    float3 cFlame  = uColor;
    float3 cEmber  = uColor * 0.35;
    float3 cSmoke  = float3(0.06, 0.04, 0.03);

    float3 color = cSmoke;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.25, plumeCore * turb));
    color = lerp(color, cFlame,  smoothstep(0.25, 0.5,  plumeCore * turb));

    // Flame pockets burn bright within the smoke
    color = lerp(color, cBright, flameLine * 0.7);
    // Nozzle shock-diamond is white-hot
    color = lerp(color, cWhite, nozzleDiamond * 0.9);
    // Exhaust rings flash gold
    color += cBright * rings * 0.5;
    // Embers flash white-gold
    color += cWhite * embers * 0.6;
    // Smoke darkens at the tail
    color = lerp(color, cSmoke, saturate(progress * 1.2 - 0.4) * (1.0 - flameLine));

    color *= turb * 0.4 + 0.6;
    color *= secVal;

    // ---- Composite ----
    float alpha = (plumeEnv * 0.5 + flameLine * 0.2 + rings * 0.1 + nozzleDiamond * 0.15 + embers * 0.1)
                * (1.0 - progress * 0.25) * turb * uOpacity * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 RocketTrailPS();
    }
}
