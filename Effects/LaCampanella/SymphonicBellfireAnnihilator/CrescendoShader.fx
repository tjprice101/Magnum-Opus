// =============================================================================
// Symphonic Bellfire Annihilator  ECrescendo Shader (Enhanced)
// =============================================================================
// Building musical crescendo for the charged special attack. Energy
// intensifies from a quiet ember hum (pianissimo) to a searing blast
// (fortissimo). FBM turbulence scales with charge. Musical staff lines
// materialise as charge builds. Radial burst rays emerge at peak charge.
// Concentric pressure waves multiply and accelerate with intensity.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uNoiseScale;
float uPhase;         // 0=pianissimo -> 1=fortissimo
float uHasSecondaryTex;

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

float FBM(float2 p)
{
    float v = 0.0;
    v += SmoothNoise(p) * 0.5;
    v += SmoothNoise(p * 2.03 + 1.7) * 0.25;
    v += SmoothNoise(p * 4.01 + 3.3) * 0.125;
    v += SmoothNoise(p * 7.97 + 5.1) * 0.0625;
    return v / 0.9375;
}

float4 CrescendoPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    float dyn = saturate(uPhase);
    float dynSq = dyn * dyn;

    // ---- Core glow: expands with crescendo ----
    float coreSize = lerp(0.10, 0.55, dynSq);
    float core = exp(-dist * dist / (coreSize * coreSize * 0.3));

    // ---- Pressure waves: multiply and accelerate with charge ----
    float waveCount = lerp(2.0, 7.0, dyn);
    float waveSpeed = lerp(1.5, 5.0, dyn);
    float waveSharpness = lerp(5.0, 2.0, dyn);
    float waves = pow(saturate(sin(dist * waveCount * 6.283 - uTime * waveSpeed) * 0.5 + 0.5), waveSharpness);
    waves *= saturate(1.0 - dist * 0.9);

    // ---- FBM energy turbulence (scales with charge) ----
    float2 turbUV = float2(angle * 0.318 + uTime * 0.15, dist * uNoiseScale * 3.0);
    float turb = FBM(turbUV * 2.0);
    float turbMask = turb * (0.4 + dyn * 0.6);

    // ---- Musical staff lines (materialise as charge builds) ----
    float staffY = frac(coords.y * 5.0);
    float staffLine = exp(-pow(staffY - 0.5, 2.0) / 0.0004);
    staffLine *= saturate(1.0 - dist * 1.2) * smoothstep(0.2, 0.6, dyn);

    // ---- Fortissimo burst rays (emerge at high charge) ----
    float rays = pow(saturate(cos(angle * 8.0) * 0.5 + 0.5), 8.0);
    rays *= saturate(1.0 - dist * 1.1) * smoothstep(0.5, 0.9, dyn);

    // ---- Spark scatter at peak charge ----
    float2 sparkUV = float2(angle * 5.0 + uTime, dist * 15.0);
    float sparks = HashNoise(sparkUV);
    sparks = pow(saturate(sparks - 0.9) * 10.0, 2.0) * dynSq * saturate(1.0 - dist);

    // Secondary texture
    float4 secTex = tex2D(uImage1, coords * 2.0);
    float secVal = lerp(1.0, secTex.r * 0.5 + 0.6, uHasSecondaryTex * 0.2 * dyn);

    // ---- 5-stop colour gradient (dynamics -> colour intensity) ----
    float3 cDark   = float3(0.08, 0.03, 0.01);
    float3 cEmber  = uColor * 0.35;
    float3 cFlame  = uColor;
    float3 cGold   = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.96, 0.86);

    float heat = core * turbMask + waves * 0.3;
    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.15, heat));
    color = lerp(color, cFlame,  smoothstep(0.15, 0.35, heat));
    color = lerp(color, cGold,   smoothstep(0.35, 0.6,  heat));
    color = lerp(color, cWhite,  smoothstep(0.6,  0.9,  heat));

    // Staff lines glow gold
    color += uSecondaryColor * staffLine * 0.6;
    // Burst rays flash white at fortissimo
    color += cWhite * rays * 0.5;
    // Sparks flash bright
    color += cWhite * sparks * 0.7;
    // Waves add subtle colour
    color += uColor * waves * 0.15;

    color *= turb * 0.3 + 0.7;
    color *= secVal;

    // ---- Composite ----
    float crescendoAlpha = lerp(0.25, 1.0, dyn);
    float bellToll = pow(saturate(cos(uTime * lerp(3.0, 8.0, dyn)) * 0.5 + 0.5), 3.0) * 0.1 + 0.9;

    float alpha = (core * 0.4 + waves * 0.25 + rays * 0.15 + staffLine * 0.1 + sparks * 0.1)
                * crescendoAlpha * uOpacity * bellToll * baseTex.a;
    float3 finalColor = color * uIntensity * bellToll * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 CrescendoPS();
    }
}
