// =============================================================================
// Infernal Chimes Calling  EMusical Shockwave Shader (Enhanced)
// =============================================================================
// Radial expanding shockwave for the 5th-hit AoE. A true bell-curve
// gaussian wavefront expands outward, leaving harmonically-decaying echo
// rings behind. Scalloped angular edges give a musical bell-mouth shape.
// Fire-tinted afterglow fills the region behind the wavefront.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;              // Expansion progress 0-1
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;
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

float4 MusicalShockwavePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // ---- Primary wavefront: gaussian bell-curve profile ----
    float wavefrontR = uPhase * 0.95;
    float wavefrontW = 0.06 + uPhase * 0.03;
    float waveDelta = dist - wavefrontR;
    float wavefront = exp(-waveDelta * waveDelta / (wavefrontW * wavefrontW * 0.5));

    // ---- Harmonic echo rings with exponential decay ----
    float echo1R = wavefrontR * 0.60;
    float echo1 = exp(-(dist - echo1R) * (dist - echo1R) / 0.003) * 0.55;
    float echo2R = wavefrontR * 0.30;
    float echo2 = exp(-(dist - echo2R) * (dist - echo2R) / 0.002) * 0.30;
    float echo3R = wavefrontR * 0.12;
    float echo3 = exp(-(dist - echo3R) * (dist - echo3R) / 0.0015) * 0.15;

    // ---- Angular scalloped bell-mouth edge ----
    float scallop = cos(angle * 6.0) * 0.06 + cos(angle * 12.0) * 0.02;
    float wavefrontScalloped = exp(-(waveDelta + scallop) * (waveDelta + scallop) / (wavefrontW * wavefrontW * 0.5));

    // ---- Fire-tinted afterglow behind wavefront ----
    float afterglow = saturate(wavefrontR - dist) * saturate(dist * 4.0);
    afterglow *= afterglow;
    afterglow *= saturate(1.0 - uPhase * 0.7);

    // ---- Turbulence at wavefront edge ----
    float2 turbUV = float2(angle * 0.5 + uTime * 0.4, dist * uNoiseScale * 3.0);
    float turb = SmoothNoise(turbUV * 3.0);
    turb = turb * 0.3 + 0.7;

    // Secondary texture
    float2 secUV = coords * 2.0;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texVal = lerp(1.0, noiseTex.r * 0.6 + 0.5, uHasSecondaryTex * 0.4);

    // ---- 5-stop colour gradient ----
    float3 cDark   = float3(0.10, 0.04, 0.01);
    float3 cEmber  = uColor * 0.5;
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.96, 0.85);

    // Wavefront is bright, echoes progressively dimmer
    float3 color = cDark * afterglow;
    color += cWhite * wavefrontScalloped * 0.8;
    color += cBright * wavefront * 0.5;
    color += cFlame * echo1;
    color += cEmber * echo2;
    color += cDark * 2.0 * echo3;
    // Afterglow warmth
    color += uColor * 0.4 * afterglow;

    color *= turb * texVal;

    // ---- Energy dissipation ----
    float energyFade = saturate(1.0 - uPhase * 0.45);
    float bellToll = pow(saturate(cos(uTime * 8.0) * 0.5 + 0.5), 3.0) * 0.1 + 0.9;

    float alpha = (wavefrontScalloped * 0.7 + echo1 * 0.5 + echo2 * 0.3 + echo3 * 0.15 + afterglow * 0.3)
                * energyFade * uOpacity * bellToll * baseTex.a;
    float3 finalColor = color * uIntensity * bellToll * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 MusicalShockwavePS();
    }
}
