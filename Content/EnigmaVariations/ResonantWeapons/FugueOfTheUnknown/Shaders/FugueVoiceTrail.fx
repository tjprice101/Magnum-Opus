// ============================================================================
// FugueVoiceTrail.fx — FugueOfTheUnknown voice projectile trail shader
// Renders flowing spectral waveform patterns along the trail UV
// with frequency-modulated width oscillation, colored teal to purple
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture (spectral waveform source)

float3 uColor;           // Primary trail color (echo teal)
float3 uSecondaryColor;  // Secondary color (voice purple)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling
float uIntensity;         // Trail brightness scaling
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

float4 PS_VoiceFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Spectral waveform: sinusoidal displacement along the trail
    float waveFreq = 8.0;
    float waveAmp = 0.08;
    float wave = sin(coords.x * waveFreq - uTime * 3.0) * waveAmp;
    float2 waveCoords = float2(coords.x, coords.y + wave);

    // Scrolling UV — voices flowing along the trail as spectral echoes
    float2 scrollUV = float2(waveCoords.x * 2.5 - uTime * 1.5, waveCoords.y * 1.5);

    // Sample noise as spectral pattern — two frequencies for harmonic overtones
    float spectral1 = tex2D(uImage1, scrollUV).r;
    float spectral2 = tex2D(uImage1, scrollUV * 0.6 + float2(uTime * 0.4, 0.3)).r;

    // Combine overtones into a rich spectral pattern
    float pattern = saturate(spectral1 * 0.55 + spectral2 * 0.45);

    // Frequency-modulated sharpening — voices becoming clearer at leading edge
    float freqMod = smoothstep(0.25, 0.65, pattern + coords.x * 0.15);

    // Sample base trail texture for structure
    float2 baseUV = float2(coords.x - uTime * 0.6, coords.y);
    float4 baseSample = tex2D(uImage0, baseUV);

    // Edge fade — soft edges along trail width
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.4, 1.0, edgeDist);

    // Color gradient: teal at leading edge (fresh voice) to purple at tail (fading echo)
    float colorMix = saturate(coords.x * 0.7 + freqMod * 0.3);
    float3 trailColor = lerp(uColor, uSecondaryColor, colorMix);

    // Brightness driven by spectral pattern and base texture
    float brightness = (freqMod * 0.65 + 0.35) * (baseSample.r * 0.4 + 0.6) * uIntensity;

    // Core glow — voice sustain at the center of the trail
    float corePulse = sin(uTime * 5.0 + coords.x * 8.0) * 0.12 + 0.88;
    float coreGlow = (1.0 - edgeDist) * 0.25 * corePulse;
    brightness += coreGlow;

    float3 finalColor = trailColor * brightness;
    float finalAlpha = edgeFade * uOpacity * input.Color.a * saturate(freqMod * 0.6 + 0.4);

    return float4(finalColor, finalAlpha);
}

technique FugueVoiceFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_VoiceFlow();
    }
}
