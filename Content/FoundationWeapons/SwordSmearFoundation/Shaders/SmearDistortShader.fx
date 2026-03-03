// SmearDistortShader.fx
// Distortion shader for SwordSmearFoundation slash arcs.
//
// Applied as a SpriteBatch pixel shader to the SlashArc textures.
// Makes the arc appear fluid and dynamic by:
//   1. Distorting the texture UVs with scrolling dual-layer noise
//   2. Adding internal energy flow via noise-based intensity modulation
//   3. Coloring through a gradient LUT for theme consistency
//
// The result: the slash arc shape comes alive — fire licks, energy flows,
// the edges breathe and pulse instead of sitting as a static PNG.

sampler uImage0 : register(s0); // The slash arc texture (SpriteBatch bound)

float uTime;
float fadeAlpha;        // Overall fade multiplier (0-1)
float distortStrength;  // UV distortion amount (0.03-0.08 recommended)
float flowSpeed;        // How fast the noise scrolls (0.3-0.6)
float noiseScale;       // UV scale for noise sampling (2.0-3.5)

// Noise texture for UV distortion and energy flow
texture noiseTex;
sampler2D samplerNoise = sampler_state
{
    texture = <noiseTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Gradient LUT for theme coloring
texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

float4 SmearDistortPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;

    // ---- DUAL-LAYER NOISE DISTORTION ----
    // Two noise samples at different scales/directions for organic warping
    float2 n1UV = uv * noiseScale + float2(uTime * flowSpeed, uTime * flowSpeed * 0.7);
    float2 n2UV = uv * noiseScale * 1.4 + float2(-uTime * flowSpeed * 0.6, uTime * flowSpeed * 0.5);

    float2 n1 = tex2D(samplerNoise, n1UV).rg;
    float2 n2 = tex2D(samplerNoise, n2UV).rg;

    // Combined offset centered around zero
    float2 offset = ((n1 + n2) - 1.0) * distortStrength;

    // ---- DISTORTED ARC SAMPLE ----
    float4 arcSample = tex2D(uImage0, uv + offset);
    float mask = arcSample.r * arcSample.a;

    // ---- INTERNAL ENERGY FLOW ----
    // A third noise scroll gives internal movement within the shape
    float2 energyUV = uv * 2.0 + float2(uTime * flowSpeed * 1.5, uTime * flowSpeed * 0.3);
    float energy = tex2D(samplerNoise, energyUV).r;

    // Modulate mask with energy for internal shimmer
    float intensity = saturate(mask * (energy * 0.4 + 0.6));

    // ---- GRADIENT LUT COLORING ----
    float3 gradColor = tex2D(samplerGradient, float2(saturate(intensity), 0.5)).rgb;

    // ---- BRIGHTNESS ----
    // Denser areas of the arc get brighter
    float brightBoost = mask * 0.5 + 0.5;
    float3 finalColor = gradColor * brightBoost * 1.6;

    // ---- FINAL COMPOSITE ----
    float finalAlpha = mask * fadeAlpha;

    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass SmearDistortPass
    {
        PixelShader = compile ps_3_0 SmearDistortPS();
    }
}
