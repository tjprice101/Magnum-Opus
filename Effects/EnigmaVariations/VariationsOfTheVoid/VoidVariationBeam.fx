// ============================================================================
// VoidVariationBeam.fx — VariationsOfTheVoid tri-beam convergence shader
// UNIQUE SIGNATURE: Spiral UV warping toward convergence point with chromatic
// separation — the red and blue channels diverge at the edges creating a
// reality-distortion look. Three visible energy streams twist into one.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PS_BeamFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Three visible beam streams that converge at UV.x = 1.0
    // Each stream has a sinusoidal path offset by 120 degrees
    float convergeFactor = coords.x * coords.x; // quadratic convergence
    float streamWidth = lerp(0.12, 0.02, convergeFactor);

    float stream1Y = 0.5 + sin(coords.x * 8.0 - uTime * 3.0) * 0.15 * (1.0 - convergeFactor);
    float stream2Y = 0.5 + sin(coords.x * 8.0 - uTime * 3.0 + 2.094) * 0.15 * (1.0 - convergeFactor);
    float stream3Y = 0.5 + sin(coords.x * 8.0 - uTime * 3.0 + 4.189) * 0.15 * (1.0 - convergeFactor);

    float s1 = exp(-pow((coords.y - stream1Y) / streamWidth, 2.0));
    float s2 = exp(-pow((coords.y - stream2Y) / streamWidth, 2.0));
    float s3 = exp(-pow((coords.y - stream3Y) / streamWidth, 2.0));

    float streams = max(s1, max(s2, s3));

    // Chromatic separation at edges — offset the color channels
    float chromaOffset = 0.02 * uIntensity * (1.0 - convergeFactor);
    float2 coordsR = float2(coords.x, coords.y + chromaOffset);
    float2 coordsB = float2(coords.x, coords.y - chromaOffset);

    float streamR = exp(-pow((coordsR.y - stream1Y) / streamWidth, 2.0));
    float streamB = exp(-pow((coordsB.y - stream3Y) / streamWidth, 2.0));

    // Noise for energy turbulence
    float2 noiseUV = float2(coords.x * 4.0 - uTime * 1.5, coords.y * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Convergence crescendo — brightness peaks at UV.x = 1.0
    float convergenceGlow = convergeFactor * uIntensity * 1.5;

    // Build color with chromatic separation
    float3 baseColor = lerp(uColor, uSecondaryColor, streams * 0.5);
    // Red channel gets extra from offset stream
    baseColor.r += streamR * uSecondaryColor.r * 0.3 * uIntensity;
    // Blue channel gets extra from offset stream
    baseColor.b += streamB * uColor.b * 0.3 * uIntensity;

    // Energy flow pattern along streams
    float flow = sin(coords.x * 20.0 - uTime * 6.0 + noise * 3.0) * 0.5 + 0.5;
    baseColor += float3(0.15, 0.3, 0.1) * flow * streams * uIntensity;

    // Convergence point white-hot glow
    float tipGlow = smoothstep(0.7, 1.0, coords.x) * exp(-abs(coords.y - 0.5) * 8.0);
    baseColor += float3(0.8, 1.0, 0.9) * tipGlow * convergenceGlow;

    // Fractal noise erosion at beam edges
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float erosion = smoothstep(0.4, 0.9, edgeDist + noise * 0.25);

    float tipFade = smoothstep(0.0, 0.06, coords.x);
    float alpha = (streams * 0.8 + tipGlow * 0.6) * (1.0 - erosion) * tipFade;
    alpha *= uOpacity * input.Color.a * (0.6 + convergenceGlow * 0.4);

    return float4(saturate(baseColor), saturate(alpha));
}

technique VoidVariationBeamFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamFlow();
    }
}
