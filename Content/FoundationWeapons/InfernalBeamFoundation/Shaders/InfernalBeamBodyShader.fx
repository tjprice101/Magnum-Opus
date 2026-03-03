// InfernalBeamBodyShader.fx
// VertexStrip beam shader for InfernalBeamFoundation.
//
// Renders a multi-layered scrolling beam body effect:
// - Primary body texture (SoundWaveBeam) scrolls along UV.x
// - Two secondary detail textures scroll at different rates
// - Light noise distortion warps all UVs for organic movement
// - Alpha mask shapes the beam cross-section
// - Gradient LUT maps intensity -> theme color
//
// UV.x = 0..1 along beam length; UV.y = 0..1 across beam width (center=0.5)

// ---- TRANSFORM ----
matrix WorldViewProjection;

// ---- UV REPETITION ----
float bodyReps;
float detail1Reps;
float detail2Reps;
float gradientReps;

// ---- SCROLL SPEEDS ----
float bodyScrollSpeed;
float detail1ScrollSpeed;
float detail2ScrollSpeed;

// ---- INTENSITY ----
float totalMult;
float noiseDistortion;

// ---- TIME ----
float uTime;

// ---- TEXTURES ----
texture onTex;
sampler2D samplerOnTex = sampler_state
{
    texture = <onTex>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

texture bodyTex;
sampler2D samplerBody = sampler_state
{
    texture = <bodyTex>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

texture detailTex1;
sampler2D samplerDetail1 = sampler_state
{
    texture = <detailTex1>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

texture detailTex2;
sampler2D samplerDetail2 = sampler_state
{
    texture = <detailTex2>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

texture noiseTex;
sampler2D samplerNoise = sampler_state
{
    texture = <noiseTex>;
    magfilter = LINEAR; minfilter = LINEAR; mipfilter = LINEAR;
    AddressU = wrap; AddressV = wrap;
};

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 InfernalBeamPS(VertexShaderOutput input) : COLOR0
{
    float2 UV = input.TextureCoordinates.xy;

    // ---- NOISE DISTORTION ----
    float2 noiseUV = UV * 3.0 + float2(uTime * 0.15, uTime * 0.1);
    float2 noise = (tex2D(samplerNoise, noiseUV).rg - 0.5) * noiseDistortion;
    float2 distortedUV = UV + noise;

    // ---- ALPHA MASK ----
    float alpha = tex2D(samplerOnTex, float2(distortedUV.x + uTime * 0.5, distortedUV.y)).a;

    // ---- PRIMARY BODY (SoundWaveBeam) ----
    float2 bodyUV = float2(frac(distortedUV.x * bodyReps + uTime * bodyScrollSpeed), distortedUV.y);
    float4 bodyColor = tex2D(samplerBody, bodyUV);

    // ---- DETAIL LAYER 1 (EnergyMotion) ----
    float2 d1UV = float2(frac(distortedUV.x * detail1Reps + uTime * detail1ScrollSpeed), distortedUV.y);
    float4 detail1Color = tex2D(samplerDetail1, d1UV);

    // ---- DETAIL LAYER 2 (EnergySurge) ----
    float2 d2UV = float2(frac(distortedUV.x * detail2Reps + uTime * detail2ScrollSpeed), distortedUV.y);
    float4 detail2Color = tex2D(samplerDetail2, d2UV);

    // ---- COMBINE BODY + DETAILS ----
    // SoundWaveBeam is the dominant layer; details add energy and complexity
    float4 combined = bodyColor * 1.2 + detail1Color * 0.6 + detail2Color * 0.4;
    float intensity = saturate(length(combined.rgb) / 1.7);

    // ---- GRADIENT LUT COLORING ----
    float2 gradUV = float2(frac(distortedUV.x * gradientReps + uTime * 0.66), 0.5);
    float4 gradColor = tex2D(samplerGradient, gradUV);

    // ---- EDGE SOFTNESS ----
    float centerDist = abs(UV.y - 0.5) * 2.0;
    float edgeFade = smoothstep(1.0, 0.6, centerDist);

    // ---- HOT CORE ----
    float coreBright = smoothstep(0.5, 0.0, centerDist);

    // ---- FINAL COLOR ----
    // Gradient tints the body; core is boosted to white-hot
    float3 tintedBody = combined.rgb * gradColor.rgb * 1.5;
    float3 hotCore = float3(1.0, 1.0, 1.0) * coreBright * intensity;
    float3 finalRGB = (tintedBody + hotCore) * totalMult;

    float finalAlpha = alpha * edgeFade * saturate(intensity * 2.0);

    return float4(finalRGB, finalAlpha) * input.Color.a;
}

technique BasicColorDrawing
{
    pass MainPS
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 InfernalBeamPS();
    }
}
