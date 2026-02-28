// ============================================================================
// VoidVariationBeam.fx — VariationsOfTheVoid tri-beam convergence beam shader
// Technique:
//   VoidVariationBeamFlow — Scrolling energy pattern along beam with ripple
//                           distortion, intensity peaks at convergence point
//                           (UV.x = 1.0 end), fractal-like noise at edges
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection
// ============================================================================

sampler uImage0 : register(s0);  // Base beam texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary beam color (variation violet)
float3 uSecondaryColor;  // Secondary color (rift teal / void surge blend)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling/animation
float uIntensity;         // Convergence intensity (0-1)
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;  // .xy = UV, .z = width correction
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

// ---------------------------------------------------------------------------
// VoidVariationBeamFlow — Tri-beam convergence energy: scrolling void
// pattern along beam body, ripple distortion waves, intensity crescendo
// at convergence (UV.x → 1.0), fractal noise erosion at beam edges
// ---------------------------------------------------------------------------
float4 PS_BeamFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Scrolling UV — energy flowing toward the convergence point
    float2 scrollUV = float2(coords.x * 3.0 - uTime * 1.5, coords.y);

    // Ripple distortion — waves pulsing along the beam toward convergence
    float ripple = sin(coords.x * 20.0 - uTime * 6.0) * 0.015 * uIntensity;
    coords.y += ripple;

    // Primary noise — fractal-like void energy pattern
    float2 noiseUV1 = float2(coords.x * 5.0 - uTime * 0.9, coords.y * 3.0 + uTime * 0.3);
    float noise1 = tex2D(uImage1, noiseUV1).r;

    // Secondary noise — finer fractal detail for edge erosion
    float2 noiseUV2 = float2(coords.x * 10.0 + uTime * 0.4, coords.y * 6.0 - uTime * 0.6);
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // Combined fractal noise
    float fractalNoise = noise1 * 0.6 + noise2 * 0.4;

    // Edge distance with fractal erosion — beam edges crumble into void
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeErosion = smoothstep(0.4, 1.0, edgeDist + fractalNoise * 0.3 - 0.15);
    float edgeFade = 1.0 - edgeErosion;

    // Sample the base beam texture with distorted UV
    float2 distortedUV = scrollUV + float2(fractalNoise * 0.03, fractalNoise * 0.02);
    float4 baseSample = tex2D(uImage0, distortedUV);

    // Convergence intensity crescendo — brightness peaks at UV.x = 1.0
    float convergence = smoothstep(0.3, 1.0, coords.x);
    float convergenceGlow = convergence * convergence * uIntensity;

    // Color: purple body → teal-white at convergence tip
    float coreFade = smoothstep(0.0, 0.4, 1.0 - edgeDist);
    float3 beamColor = lerp(uSecondaryColor, uColor, coreFade);

    // Convergence point brightens toward white
    beamColor = lerp(beamColor, float3(0.85, 1.0, 0.92), convergenceGlow * 0.5);

    // Energy pulse ripples visible as brightness bands
    float pulse = sin(coords.x * 15.0 - uTime * 4.0) * 0.5 + 0.5;
    beamColor += uSecondaryColor * pulse * 0.15 * uIntensity;

    // Tip fade at beam origin (not at convergence — that's the bright end)
    float tipFade = smoothstep(0.0, 0.08, coords.x);

    float3 finalColor = beamColor * (baseSample.r * 0.3 + 0.7 + convergenceGlow * 0.3);
    float finalAlpha = edgeFade * tipFade * uOpacity * input.Color.a * (0.7 + convergenceGlow * 0.3);

    return float4(saturate(finalColor), saturate(finalAlpha));
}

technique VoidVariationBeamFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamFlow();
    }
}
