// ============================================================================
// FugueVoiceTrail.fx — FugueOfTheUnknown voice projectile trail
// UNIQUE SIGNATURE: Audio spectrum visualization — multiple stacked sine waves
// at different frequencies, each in a slightly different color channel. The trail
// looks like a real-time audio EQ/waveform display. Polyphonic counterpoint
// made visible — multiple independent "voice" waveforms layered together.
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

float4 PS_VoiceFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // 5 voice waveforms at different frequencies (polyphonic fugue)
    float phase = coords.x * 6.28 - uTime;

    // Voice 1: Bass — slow, wide amplitude
    float v1Freq = 3.0;
    float v1Amp = 0.12;
    float v1Y = 0.5 + sin(coords.x * v1Freq - uTime * 2.0) * v1Amp;
    float v1 = exp(-pow((coords.y - v1Y) * 10.0, 2.0));

    // Voice 2: Tenor — medium frequency
    float v2Freq = 5.0;
    float v2Amp = 0.09;
    float v2Y = 0.5 + sin(coords.x * v2Freq - uTime * 2.8 + 1.0) * v2Amp;
    float v2 = exp(-pow((coords.y - v2Y) * 12.0, 2.0));

    // Voice 3: Alto — higher frequency
    float v3Freq = 8.0;
    float v3Amp = 0.06;
    float v3Y = 0.5 + sin(coords.x * v3Freq - uTime * 3.5 + 2.5) * v3Amp;
    float v3 = exp(-pow((coords.y - v3Y) * 14.0, 2.0));

    // Voice 4: Soprano — highest, tight oscillation
    float v4Freq = 12.0;
    float v4Amp = 0.04;
    float v4Y = 0.5 + sin(coords.x * v4Freq - uTime * 4.2 + 4.0) * v4Amp;
    float v4 = exp(-pow((coords.y - v4Y) * 16.0, 2.0));

    // Voice 5: Counter-subject — opposing motion
    float v5Freq = 6.0;
    float v5Amp = 0.08;
    float v5Y = 0.5 - sin(coords.x * v5Freq - uTime * 2.5 + 3.14) * v5Amp;
    float v5 = exp(-pow((coords.y - v5Y) * 11.0, 2.0));

    // Each voice gets a distinct color
    // Bass: deep purple
    float3 v1Color = uColor * 1.0 * v1;
    // Tenor: mixed purple-green
    float3 v2Color = lerp(uColor, uSecondaryColor, 0.3) * 0.9 * v2;
    // Alto: green
    float3 v3Color = uSecondaryColor * 0.8 * v3;
    // Soprano: bright cyan-green
    float3 v4Color = lerp(uSecondaryColor, float3(0.5, 1.0, 0.8), 0.4) * 0.7 * v4;
    // Counter-subject: pale white-purple
    float3 v5Color = lerp(uColor, float3(0.8, 0.7, 0.9), 0.5) * 0.6 * v5;

    // Harmonic convergence — where multiple voices overlap, brightness spikes
    float overlap2 = v1 * v2 + v2 * v3 + v3 * v4 + v4 * v5 + v1 * v5;
    float overlap3 = v1 * v2 * v3 + v2 * v3 * v4 + v3 * v4 * v5;
    float harmonicNode = saturate(overlap2 * 2.0 + overlap3 * 4.0);

    // Harmonic nodes glow bright
    float3 harmonicColor = float3(0.8, 1.0, 0.9) * harmonicNode * uIntensity;

    // Combine all voices
    float3 totalVoice = v1Color + v2Color + v3Color + v4Color + v5Color + harmonicColor;

    // Noise for subtle texture
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 0.3, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    totalVoice += uColor * noise * 0.05;

    // Intensity scaling
    totalVoice *= uIntensity;

    // Edge fade
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.6, 1.0, edgeDist);

    // Alpha from combined voice strength
    float totalStrength = v1 + v2 + v3 + v4 + v5 + harmonicNode;
    float finalAlpha = edgeFade * saturate(totalStrength) * uOpacity * input.Color.a;

    return float4(totalVoice, finalAlpha);
}

technique FugueVoiceFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_VoiceFlow();
    }
}
