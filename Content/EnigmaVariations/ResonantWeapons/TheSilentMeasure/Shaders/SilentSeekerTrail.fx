// ============================================================================
// SilentSeekerTrail.fx — TheSilentMeasure seeker trail shader
// Renders a dotted/dashed energy trail for homing seekers — creates a periodic
// on/off visibility pattern along UV.x (like morse code), glowing
// violet→emerald, with noise-distorted edges.
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;            // Primary trail color (Question Violet)
float3 uSecondaryColor;   // Secondary color (Enigma Emerald)
float uOpacity;            // Overall opacity
float uTime;               // Elapsed time for scrolling
float uIntensity;          // Effect strength (0→1)
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

float4 PS_SeekerFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Morse code / dotted pattern along trail length
    // Frequency creates periodic on/off dashes
    float dashFreq = 12.0;
    float scrolled = coords.x * dashFreq - uTime * 3.0;
    float dashPattern = smoothstep(0.3, 0.35, frac(scrolled)) * smoothstep(0.85, 0.8, frac(scrolled));

    // Sample noise for edge distortion
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 0.4, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Distort edges with noise
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float distortedEdge = edgeDist + (noise - 0.5) * 0.15 * uIntensity;
    float edgeFade = 1.0 - smoothstep(0.5, 0.9, distortedEdge);

    // Color: violet core → emerald outer, modulated by position and noise
    float colorMix = saturate(coords.x * 0.7 + noise * 0.3);
    float3 trailColor = lerp(uColor, uSecondaryColor, colorMix);

    // Brightness pulse along trail
    float pulse = sin(coords.x * 6.28 - uTime * 4.0) * 0.2 + 0.8;
    trailColor *= pulse * uIntensity;

    // Combined visibility: dash pattern * edge fade
    float visibility = dashPattern * edgeFade;

    // Sample base texture for additional modulation
    float4 baseSample = tex2D(uImage0, coords);
    trailColor *= (baseSample.r * 0.4 + 0.6);

    float finalAlpha = visibility * uOpacity * input.Color.a;
    return float4(trailColor, finalAlpha);
}

technique SilentSeekerFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SeekerFlow();
    }
}
