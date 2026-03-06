// =============================================================================
// Ignition of the Bell  EThrust Trail Shader (Enhanced)
// =============================================================================
// Supersonic directional flame jet for the 3-phase thrust combo. Gaussian
// core with narrowing spearhead profile, shock-diamond compression nodes
// that flash along the jet axis, FBM turbulence for organic fire, and
// afterburn plasma shimmer at the edges. Feels like concentrated
// infernal propulsion  Ea lance of compressed bell-fire.
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

float4 ApplyOverbright(float3 color, float alpha) { return float4(color * uOverbrightMult, alpha); }
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

float4 IgnitionThrustPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    // ---- Jet cross-section: gaussian core with narrowing spearhead ----
    float widthEnvelope = 1.0 - coords.x * 0.65;
    float yDist = abs(coords.y - 0.5) * 2.0;
    float coreProfile = exp(-yDist * yDist / (widthEnvelope * widthEnvelope * 0.18));
    float outerProfile = exp(-yDist * yDist / (widthEnvelope * widthEnvelope * 0.6));

    // ---- FBM turbulence: fast directional scrolling ----
    float2 flameUV = float2(
        coords.x * 3.0 - uTime * uScrollSpeed * 2.5,
        coords.y * 4.0
    );
    float turb = FBM(flameUV * uNoiseScale);

    // Secondary texture detail layer
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll * 1.5;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texDetail = lerp(1.0, noiseTex.r * 0.7 + 0.5, uHasSecondaryTex);
    turb *= texDetail;

    // ---- Shock-diamond nodes: periodic compressions along the jet ----
    float shockPhase = coords.x * 12.0 - uTime * uScrollSpeed * 0.8;
    float shockDiamond = pow(saturate(cos(shockPhase) * 0.5 + 0.5), 4.0);
    shockDiamond *= coreProfile;

    // ---- Afterburn plasma shimmer at edges ----
    float edgeBand = saturate(outerProfile - coreProfile * 0.7);
    float plasmaShimmer = SmoothNoise(float2(coords.x * 8.0 + uTime * 3.0, coords.y * 15.0));
    float plasma = edgeBand * plasmaShimmer * 0.6;

    // ---- 5-stop colour gradient ----
    float heat = coreProfile * turb;
    float3 cSoot   = float3(0.08, 0.03, 0.02);
    float3 cEmber  = uColor * 0.5;
    float3 cFlame  = uColor;
    float3 cBright = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.97, 0.90);

    float3 color = cSoot;
    color = lerp(color, cEmber,  smoothstep(0.0, 0.25, heat));
    color = lerp(color, cFlame,  smoothstep(0.25, 0.50, heat));
    color = lerp(color, cBright, smoothstep(0.50, 0.75, heat));
    color = lerp(color, cWhite,  smoothstep(0.75, 1.0,  heat));

    // Shock-diamond nodes flash white-gold
    color = lerp(color, cWhite, shockDiamond * 0.7);
    // Afterburn plasma tints edges amber
    color += uColor * 0.4 * plasma;

    // ---- Trail fade & dynamics ----
    float trailFade = smoothstep(0.0, 0.08, coords.x) * smoothstep(1.0, 0.15, coords.x);
    float jetFlicker = SmoothNoise(float2(uTime * 12.0, coords.x * 3.0)) * 0.12 + 0.88;

    float alpha = (coreProfile * 0.7 + outerProfile * 0.2 + shockDiamond * 0.15 + plasma * 0.1)
                * trailFade * turb * uOpacity * jetFlicker * baseTex.a;
    float3 finalColor = color * uIntensity * jetFlicker * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique TrailPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 IgnitionThrustPS();
    }
}
