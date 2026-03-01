// =============================================================================
// Infernal Chimes Calling  EChoir Minion Trail (Enhanced)
// =============================================================================
// Trail for summoned choir minion as it glides. Three parallel ghost-echo
// trails overlap like choir voices singing in harmony. FBM ember turbulence
// adds warmth. Resonance nodes brighten where the voices align. The trail
// breathes with a gentle choral rhythm and has a soft ethereal mist edge.
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

float FBM(float2 p)
{
    float v = 0.0;
    v += SmoothNoise(p) * 0.5;
    v += SmoothNoise(p * 2.03 + 1.7) * 0.25;
    v += SmoothNoise(p * 4.01 + 3.3) * 0.125;
    v += SmoothNoise(p * 7.97 + 5.1) * 0.0625;
    return v / 0.9375;
}

float4 ChoirMinionTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // ---- Three parallel choir voice trails ----
    float yDist0 = abs(coords.y - 0.50) * 2.0; // Soprano (centre)
    float yDist1 = abs(coords.y - 0.38) * 2.0; // Alto (upper)
    float yDist2 = abs(coords.y - 0.62) * 2.0; // Tenor (lower)

    float voice0 = exp(-yDist0 * yDist0 / 0.08);
    float voice1 = exp(-yDist1 * yDist1 / 0.04) * 0.5;
    float voice2 = exp(-yDist2 * yDist2 / 0.04) * 0.5;

    // ---- Choral harmony overtones (musical wave interference) ----
    float overtone1 = sin(coords.x * 8.0 - uTime * 4.0) * 0.5 + 0.5;
    float overtone2 = sin(coords.x * 12.0 - uTime * 6.0 + 1.047) * 0.5 + 0.5;
    float overtone3 = sin(coords.x * 20.0 - uTime * 10.0 + 2.094) * 0.5 + 0.5;
    float harmony = overtone1 * 0.5 + overtone2 * 0.3 + overtone3 * 0.2;

    // Resonance nodes: where all voices align, brightness peaks
    float resonance = voice0 * voice1 * voice2;
    resonance = pow(saturate(resonance * 20.0), 0.5) * 0.8;

    // ---- FBM ember turbulence ----
    float2 driftUV = float2(
        coords.x * 2.0 - uTime * uScrollSpeed * 0.7,
        coords.y * 4.0 + sin(coords.x * 3.0 + uTime) * 0.06
    );
    float turb = FBM(driftUV * uNoiseScale);

    // Secondary texture
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    turb = lerp(turb, turb * (0.5 + noiseTex.r * 0.6), uHasSecondaryTex);

    // ---- Ethereal mist at trail edges ----
    float envelope = exp(-yDist0 * yDist0 / 0.5);
    float mistEdge = saturate(envelope - (voice0 + voice1 + voice2) * 0.4);
    float mist = mistEdge * SmoothNoise(float2(coords.x * 4.0 + uTime, coords.y * 6.0)) * 0.35;

    // ---- 5-stop colour gradient ----
    float composite = (voice0 + voice1 + voice2) * turb * harmony;
    float3 cDark   = float3(0.08, 0.03, 0.01);
    float3 cEmber  = uColor * 0.4;
    float3 cWarm   = uColor;
    float3 cGold   = uSecondaryColor;
    float3 cBright = float3(1.0, 0.93, 0.78);

    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.15, composite));
    color = lerp(color, cWarm,   smoothstep(0.15, 0.35, composite));
    color = lerp(color, cGold,   smoothstep(0.35, 0.6,  composite));
    color = lerp(color, cBright, smoothstep(0.6,  0.9,  composite));

    // Resonance nodes glow white-gold
    color = lerp(color, cBright, resonance);
    // Mist tints edges with warm amber
    color += uColor * 0.25 * mist;

    // ---- Trail fade & choral breathing ----
    float trailFade = smoothstep(0.0, 0.06, coords.x) * smoothstep(1.0, 0.15, coords.x);
    float breathe = sin(uTime * 3.5) * 0.08 + 0.92;

    float alpha = ((voice0 + voice1 + voice2) * 0.5 + harmony * 0.2 + resonance * 0.15 + mist * 0.15)
                * trailFade * turb * uOpacity * breathe * baseTex.a;
    float3 finalColor = color * uIntensity * breathe * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        PixelShader = compile ps_3_0 ChoirMinionTrailPS();
    }
}
