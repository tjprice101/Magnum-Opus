// =============================================================================
// Fang of the Infinite Bell  EEmpowered Lightning Shader (Enhanced)
// =============================================================================
// Arcane lightning bolt with branching fractal structure. Unlike raw fire,
// this is channeled magical energy  Ea contained infernal thunderbolt with
// multi-layered jagged paths, phosphorescent afterglow corridor, branching
// sub-arcs, and bell-shaped strike intensification at the terminus.
// The bolt feels alive  Econstantly shifting, crackling, splitting.
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

// Stepped noise for jagged bolt paths (not smooth  Eintentionally blocky)
float JaggedNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    // Hard step transitions for angular lightning look
    float2 u = step(0.5, f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float4 EmpoweredLightningPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float edgeDist = abs(coords.y - 0.5) * 2.0;

    // --- Primary bolt path: jagged displacement with multi-scale layers ---
    float timeBase = uTime * uScrollSpeed;

    // Large-scale jagging (main bolt direction)
    float jag1 = JaggedNoise(float2(coords.x * uNoiseScale * 6.0, timeBase * 4.0)) * 2.0 - 1.0;
    // Medium-scale (secondary deviations)
    float jag2 = JaggedNoise(float2(coords.x * uNoiseScale * 14.0, timeBase * 7.0 + 3.7)) * 2.0 - 1.0;
    // Fine-scale (crackling detail)
    float jag3 = HashNoise(float2(coords.x * uNoiseScale * 30.0, timeBase * 12.0)) * 2.0 - 1.0;

    float totalJag = jag1 * 0.15 + jag2 * 0.07 + jag3 * 0.03;

    // Primary bolt core  Eextremely tight gaussian along displaced centerline
    float boltCenter = coords.y - 0.5 + totalJag;
    float primaryBolt = exp(-boltCenter * boltCenter * 80.0);  // Razor-thin core
    float primaryGlow = exp(-boltCenter * boltCenter * 12.0);   // Wider corona

    // --- Branching sub-arcs (3 independent branches) ---
    float branch1Jag = JaggedNoise(float2(coords.x * 18.0, timeBase * 6.0 + 1.0)) * 2.0 - 1.0;
    float branch1Y = coords.y - (0.5 + totalJag + branch1Jag * 0.2);
    float branch1 = exp(-branch1Y * branch1Y * 40.0);
    float branch1Mask = step(0.35, coords.x) * saturate(1.0 - (coords.x - 0.35) * 3.0);

    float branch2Jag = JaggedNoise(float2(coords.x * 15.0, timeBase * 5.0 + 5.0)) * 2.0 - 1.0;
    float branch2Y = coords.y - (0.5 + totalJag - branch2Jag * 0.18);
    float branch2 = exp(-branch2Y * branch2Y * 50.0);
    float branch2Mask = step(0.55, coords.x) * saturate(1.0 - (coords.x - 0.55) * 4.0);

    float branch3Jag = HashNoise(float2(coords.x * 22.0, timeBase * 9.0 + 8.0)) * 2.0 - 1.0;
    float branch3Y = coords.y - (0.5 + totalJag + branch3Jag * 0.12);
    float branch3 = exp(-branch3Y * branch3Y * 60.0);
    float branch3Mask = step(0.15, coords.x) * saturate(1.0 - (coords.x - 0.15) * 5.0);

    float branches = branch1 * branch1Mask * 0.5 + branch2 * branch2Mask * 0.4 + branch3 * branch3Mask * 0.3;

    // --- Phosphorescent afterglow corridor ---
    float afterglow = exp(-edgeDist * edgeDist * 2.0);
    float afterPulse = sin(uTime * 2.0 + coords.x * 3.0) * 0.3 + 0.7;
    afterglow *= afterPulse * 0.15;

    // --- Bell-shaped terminus intensification ---
    float terminus = exp(-pow((coords.x - 0.9) * 5.0, 2.0));
    float terminusBell = terminus * primaryGlow * 1.5;

    // --- Secondary texture ---
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll * 2.0;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, 0.6 + noiseTex.r * 0.6, uHasSecondaryTex);

    // --- Arcane lightning color (amber-gold, not blue-white) ---
    float3 voidEdge = uColor * 0.15;
    float3 amberArc = uColor;
    float3 goldCore = uSecondaryColor;
    float3 whiteArc = float3(1.0, 0.98, 0.92);

    float boltT = primaryBolt + primaryGlow * 0.3;
    float3 color = voidEdge * afterglow;
    color += amberArc * primaryGlow * 0.5;
    color = lerp(color, goldCore, primaryBolt * 0.85);
    color = lerp(color, whiteArc, primaryBolt * 0.5);

    // Branch colors (dimmer, more amber)
    color += amberArc * branches * 0.6;

    // Terminus bell effect
    color += goldCore * terminusBell * 0.4;

    color *= texDetail;

    // --- Rapid compound flicker (electric crackling) ---
    float flicker = sin(uTime * 35.0 + coords.x * 25.0) * 0.15 + 0.85;
    flicker *= sin(uTime * 21.0 + coords.x * 12.0) * 0.10 + 0.90;
    flicker *= sin(uTime * 53.0 + coords.x * 40.0) * 0.05 + 0.95;

    // --- Length fade ---
    float lengthFade = saturate(coords.x * 5.0) * saturate(1.0 - coords.x * 0.5);
    lengthFade *= 1.0 + terminus * 0.3;

    float alpha = (primaryBolt * 0.7 + primaryGlow * 0.25 + branches * 0.3 + afterglow + terminusBell * 0.2)
                  * lengthFade * uOpacity * flicker * baseTex.a;
    float3 finalColor = color * uIntensity * flicker * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha) * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 EmpoweredLightningPS();
    }
}
