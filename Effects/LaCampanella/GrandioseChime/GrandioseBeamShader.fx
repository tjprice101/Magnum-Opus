// =============================================================================
// Grandiose Chime — Beam Shader (Enhanced)
// =============================================================================
// Radiant concentrated beam with true standing wave harmonic interference.
// The beam vibrates with visible antinodes and nodes — like a plucked string
// made of fire. Spectral dispersion at beam edges splits the gold into
// component hues. Bell-toll pulse pressure nodes brighten periodically.
// Unique identity: a MUSICAL beam — you can "see" the sound waves.
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
float uPhase;
float uHasSecondaryTex;
float uSecondaryTexScale;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float4 GrandioseBeamPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float yDist = abs(coords.y - 0.5) * 2.0;
    float charge = saturate(uPhase);
    float scrollX = coords.x - uTime * uScrollSpeed;

    // --- Standing wave harmonic series (fundamental + 3 overtones) ---
    float fundamental = sin(scrollX * 16.0 * 6.28318) * 0.5 + 0.5;
    float overtone2 = sin(scrollX * 32.0 * 6.28318 + 1.57) * 0.5 + 0.5;
    float overtone3 = sin(scrollX * 48.0 * 6.28318 + 3.14) * 0.5 + 0.5;
    float overtone4 = sin(scrollX * 64.0 * 6.28318 + 4.71) * 0.5 + 0.5;

    // Weighted harmonic sum (fundamental dominates, overtones add detail)
    float harmonic = fundamental * 0.45 + overtone2 * 0.25 + overtone3 * 0.18 + overtone4 * 0.12;
    float harmonicSharp = pow(harmonic, 1.5);

    // --- Antinode/node visualization: beam "breathes" wider at antinodes ---
    float antinodeWidth = 0.30 + harmonicSharp * 0.15 * charge;
    float envelopeWidth = antinodeWidth + 0.15;

    float core = exp(-pow(yDist / max(antinodeWidth * 0.4, 0.01), 2.0) * 8.0);
    float body = exp(-pow(yDist / max(envelopeWidth, 0.01), 2.0) * 3.0);
    float outerHaze = exp(-yDist * yDist * 1.5) * 0.3;

    // --- Bell-toll pressure nodes (bright pulses at intervals) ---
    float bellNode = cos(scrollX * 6.0 * 6.28318) * 0.5 + 0.5;
    bellNode = pow(bellNode, 5.0);
    float bellBright = bellNode * body * 0.4 * charge;

    // --- Spectral dispersion at beam edges ---
    // Fire splits into ember-red outer, orange mid, gold-white inner
    float edgeZone = saturate(yDist - antinodeWidth * 0.5) / max(envelopeWidth - antinodeWidth * 0.5, 0.01);
    edgeZone = saturate(edgeZone);
    float3 spectralShift = lerp(float3(0.0, 0.0, 0.0), float3(0.3, -0.1, -0.15), edgeZone);

    // --- Lateral harmonic shimmer (perpendicular waves) ---
    float lateralWave = sin(coords.y * 24.0 + uTime * 8.0) * 0.5 + 0.5;
    float lateralDetail = sin(coords.y * 48.0 - uTime * 12.0) * 0.5 + 0.5;
    float shimmer = lateralWave * 0.7 + lateralDetail * 0.3;
    shimmer = shimmer * 0.2 + 0.8;

    // --- Smooth turbulence for organic fire feel ---
    float2 turbUV = float2(scrollX * uNoiseScale * 2.0, coords.y * 3.0);
    float turb = SmoothNoise(turbUV);
    float turb2 = SmoothNoise(turbUV * 2.3 + float2(1.7, 3.1));
    float turbulence = turb * 0.6 + turb2 * 0.4;
    turbulence = turbulence * 0.3 + 0.7;

    // --- Secondary noise texture ---
    float2 secUV = float2(scrollX * uSecondaryTexScale, coords.y * 2.0);
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(1.0, 0.6 + noiseTex.r * 0.6, uHasSecondaryTex * 0.35);

    // --- 5-stop beam color gradient ---
    float3 darkEmber = uSecondaryColor * 0.3;
    float3 burnOrange = uColor;
    float3 brightGold = uSecondaryColor;
    float3 whiteGold = float3(1.0, 0.95, 0.82);
    float3 bellFlash = float3(1.0, 0.88, 0.55);

    float3 color = lerp(darkEmber, burnOrange, body);
    color = lerp(color, brightGold, saturate(body * 1.5 - 0.3));
    color = lerp(color, whiteGold, core * 0.75);
    color += spectralShift * outerHaze;  // Spectral dispersion

    // Standing wave modulation
    color *= harmonicSharp * 0.35 + 0.65;

    // Bell-toll highlights
    color = lerp(color, bellFlash, bellBright);

    color *= shimmer * turbulence * noiseVal;

    // --- Charge-reactive intensity ---
    float chargeAlpha = 0.3 + charge * 0.7;

    float alpha = (body * 0.4 + core * 0.45 + outerHaze * 0.15 + bellBright * 0.15)
                  * uOpacity * sampleColor.a * baseTex.a * chargeAlpha;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha));
}

technique AutoPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 GrandioseBeamPS();
    }
}
