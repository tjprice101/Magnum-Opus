// =============================================================================
// Dual-Fated Chime  EBell Flame Trail Shader (Enhanced)
// =============================================================================
// Homing flame-wave sub-projectile trail. Organic fire turbulence via FBM
// with bell-toll standing wave nodes that create rhythmic bright/dark pulses.
// Asymmetric cross-section (flames rise upward). Smoldering ember wake with
// heat-shimmer distortion. Distinct from the slash  Ethis is a flowing river
// of fire, not a cutting arc.
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
matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput TrailVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 ApplyOverbright(float3 color, float alpha) { return float4(color * uOverbrightMult, alpha); }

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

float FBM(float2 uv)
{
    float v = 0.0; float a = 0.5; float2 p = uv;
    v += SmoothNoise(p) * a; p *= 2.07; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.03; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.01; a *= 0.5;
    v += SmoothNoise(p) * a;
    return v;
}

float4 BellFlameTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // Asymmetric cross-section  Eflames lean upward (lower y = top of trail)
    float center = abs(coords.y - 0.48) * 2.0;  // Slightly off-center
    float riseShift = (0.5 - coords.y) * 0.12;   // Upward flame lean
    float body = saturate(1.0 - center * 1.15);
    body = pow(body, 1.5);
    float core = exp(-center * center * 10.0);

    // FBM-driven organic flame turbulence
    float2 fireUV = float2(coords.x * uNoiseScale * 2.0 - uTime * uScrollSpeed * 0.9,
                            coords.y * 2.5 + riseShift - uTime * 0.4);
    float fire = FBM(fireUV);
    float fireDetail = FBM(fireUV * 2.3 + float2(uTime * 0.6, 0.0));
    fire = saturate(fire * 0.7 + fireDetail * 0.4 - 0.05);

    // Secondary noise texture overlay
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texBlend = lerp(1.0, 0.5 + noiseTex.r * 0.7, uHasSecondaryTex);
    fire *= texBlend;

    // Bell-toll standing wave nodes  Ebright pulses at regular intervals
    float bellPhase = coords.x * 8.0 - uTime * 6.0;
    float bellToll = sin(bellPhase * 6.28318);
    bellToll = pow(saturate(bellToll), 4.0);

    // Secondary harmonic for richness
    float bellHarmonic = sin(bellPhase * 2.0 * 6.28318 + 1.57);
    bellHarmonic = pow(saturate(bellHarmonic), 5.0) * 0.4;
    float bellTotal = bellToll + bellHarmonic;

    // Heat shimmer  Esinusoidal UV displacement
    float shimmerX = sin(coords.x * 12.0 + uTime * 5.0) * 0.02 * body;
    float shimmerY = cos(coords.x * 8.0 + uTime * 3.5) * 0.015 * body;

    // 5-stop color gradient driven by fire intensity
    float3 darkSmoke = float3(0.06, 0.02, 0.01);
    float3 deepEmber = uColor * 0.4;
    float3 brightFlame = uColor;
    float3 bellGold = uSecondaryColor;
    float3 whiteHot = float3(1.0, 0.95, 0.82);

    float t = saturate(body * fire * 1.5);
    float3 color = lerp(darkSmoke, deepEmber, saturate(t * 3.5));
    color = lerp(color, brightFlame, saturate(t * 2.5 - 0.3));
    color = lerp(color, bellGold, saturate(t * 2.0 - 0.6));
    color = lerp(color, whiteHot, core * saturate(t - 0.4) * 0.6);

    // Bell-toll pulse injects gold-white fire at standing wave nodes
    color = lerp(color, lerp(bellGold, whiteHot, 0.5), bellTotal * body * 0.5);

    // Smoldering wake embers at trail tail
    float wakeZone = saturate(coords.x * 2.0 - 0.6);
    float wakeEmbers = HashNoise(coords * float2(30.0, 15.0) + uTime * 2.0);
    wakeEmbers = step(0.90, wakeEmbers) * wakeZone * body;
    color += float3(1.0, 0.5, 0.1) * wakeEmbers * 1.5;

    // Trail fade: exponential decay with bell-node brightness bumps
    float trailFade = saturate(coords.x * 6.0) * exp(-coords.x * 1.5);
    trailFade *= 1.0 + bellTotal * 0.2;

    // Compound flicker
    float flicker = sin(uTime * 8.0 + coords.x * 5.0) * 0.06;
    flicker += sin(uTime * 19.0 + coords.x * 11.0) * 0.04;
    flicker = flicker + 0.90;

    float alpha = body * trailFade * fire * uOpacity * baseTex.a;
    alpha += wakeEmbers * 0.2;
    float3 finalColor = color * uIntensity * flicker * baseTex.rgb;

    return ApplyOverbright(finalColor, saturate(alpha) * sampleColor.a);
}

technique BellFlameMain
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 BellFlameTrailPS();
    }
}
