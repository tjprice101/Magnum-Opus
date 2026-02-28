// =============================================================================
// Ignition of the Bell — Infernal Geyser Shader (Enhanced)
// =============================================================================
// Concentrated column of bell fire for the alt-fire charge attack. The beam
// widens and intensifies with charge level (uPhase). Features rising ember
// streaks, FBM fire turbulence, heat-shimmer edge ripple, and charge-
// reactive pressure nodes that brighten at high charge.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;              // Charge level 0-1
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

float4 InfernalGeyserPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // ---- Beam cross-section: gaussian core that widens with charge ----
    float coreWidth = 0.22 + uPhase * 0.28;
    float yDist = abs(coords.y - 0.5) * 2.0;
    float coreProfile = exp(-yDist * yDist / (coreWidth * coreWidth * 0.25));
    float edgeProfile = exp(-yDist * yDist / (coreWidth * coreWidth * 0.8));

    // ---- FBM fire turbulence ----
    float2 flowUV = float2(
        coords.x * 2.5 - uTime * uScrollSpeed * 1.8,
        coords.y * 5.0 + sin(coords.x * 4.0 + uTime * 2.0) * 0.05 * (1.0 + uPhase)
    );
    float turb = FBM(flowUV * uNoiseScale);

    // Secondary texture detail
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, noiseTex.r * 0.6 + 0.5, uHasSecondaryTex);
    turb *= texDetail;

    // ---- Rising ember streaks (vertical upward drift) ----
    float2 emberUV = float2(coords.x * 8.0, coords.y * 25.0 + uTime * 4.0);
    float embers = HashNoise(emberUV);
    embers = pow(saturate(embers - 0.85) * 6.67, 2.0);
    embers *= edgeProfile * (0.5 + uPhase * 0.5);

    // ---- Charge-reactive pressure nodes ----
    float nodeFreq = lerp(6.0, 12.0, uPhase);
    float nodes = pow(saturate(cos(coords.x * nodeFreq - uTime * uScrollSpeed) * 0.5 + 0.5), 3.0);
    nodes *= coreProfile * uPhase;

    // ---- Heat shimmer at beam edges ----
    float shimmerBand = saturate(edgeProfile - coreProfile * 0.8);
    float shimmer = SmoothNoise(float2(coords.x * 6.0 + uTime * 4.0, coords.y * 12.0));
    float heatShimmer = shimmerBand * shimmer * 0.5;

    // ---- 5-stop charge-reactive colour gradient ----
    float heat = coreProfile * turb * (0.4 + uPhase * 0.6);
    float3 cDark   = float3(0.15, 0.04, 0.02);
    float3 cEmber  = uColor * 0.5;
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.97, 0.88);

    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.2,  heat));
    color = lerp(color, cFlame,  smoothstep(0.2,  0.45, heat));
    color = lerp(color, cBright, smoothstep(0.45, 0.7,  heat));
    color = lerp(color, cWhite,  smoothstep(0.7,  1.0,  heat));

    // Pressure nodes flash bright gold
    color = lerp(color, cWhite, nodes * 0.6);
    // Embers flash white-gold
    color += float3(1.0, 0.85, 0.40) * embers;
    // Heat shimmer tints edges amber
    color += uColor * 0.3 * heatShimmer;

    // ---- Length fade & dynamics ----
    float lengthFade = smoothstep(0.0, 0.06, coords.x) * smoothstep(1.0, 0.4, coords.x);
    float chargeIntensity = 0.35 + uPhase * 0.65;
    float pulse = sin(uTime * 7.0 + coords.x * 4.0) * 0.06 + 0.94;

    float alpha = (coreProfile * 0.6 + edgeProfile * 0.15 + nodes * 0.15 + embers * 0.1 + heatShimmer * 0.1)
                * lengthFade * turb * chargeIntensity * uOpacity * pulse * baseTex.a;
    float3 finalColor = color * uIntensity * pulse * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 InfernalGeyserPS();
    }
}
