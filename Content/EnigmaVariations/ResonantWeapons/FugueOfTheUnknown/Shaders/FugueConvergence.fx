// ============================================================================
// FugueConvergence.fx — FugueOfTheUnknown convergence detonation shader
// Two techniques:
//   FugueConvergenceWave — Standing wave pattern with contracting concentric
//     rings and interference patterns, like polyphonic voices colliding
//   FugueConvergenceGlow — Soft bloom behind the convergence effect
// ============================================================================

sampler uImage0 : register(s0);  // Base texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary color (fugue cyan)
float3 uSecondaryColor;  // Secondary color (voice purple)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for animation
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

// --- Technique 1: Standing wave with contracting concentric rings ---

float4 PS_ConvergenceWave(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Noise for organic interference
    float2 noiseUV = float2(angle * 0.25 + uTime * 0.15, dist * 2.5 + uTime * 0.3);
    float noise = tex2D(uImage1, noiseUV).r;

    // Standing wave: two waves traveling in opposite directions create nodes
    float waveOut = sin((dist * 20.0 - uTime * 3.0) + noise * 1.5) * 0.5 + 0.5;
    float waveIn = sin((dist * 20.0 + uTime * 2.5) + noise * 1.2) * 0.5 + 0.5;

    // Interference pattern — voices colliding
    float interference = waveOut * waveIn;
    interference = smoothstep(0.15, 0.5, interference);

    // Contracting rings — convergence pulls inward
    float contract = sin((dist * 15.0 + uTime * 4.0) * uIntensity) * 0.5 + 0.5;
    float rings = interference * 0.6 + contract * 0.4;

    // Radial fade — stronger at center, fading at edges
    float radialFade = 1.0 - smoothstep(0.15, 0.5, dist);

    // Color: purple at outer ring edges, cyan at convergence nodes
    float colorMix = saturate(rings * 0.6 + noise * 0.2 + (1.0 - dist) * 0.2);
    float3 waveColor = lerp(uSecondaryColor, uColor, colorMix);

    // Brightness scales with convergence intensity
    float brightness = rings * radialFade * uIntensity * 1.3;

    // Angular harmonic variation — polyphonic voice interference
    float harmonic = sin(angle * 3.0 + uTime * 1.5) * 0.15 + 0.85;
    brightness *= harmonic;

    float3 finalColor = waveColor * brightness;
    float finalAlpha = radialFade * rings * uOpacity * uIntensity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

// --- Technique 2: Soft bloom glow behind the convergence ---

float4 PS_ConvergenceGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    // Gaussian-like soft glow falloff
    float glow = exp(-dist * dist * 4.5);

    // Gentle convergence pulse
    float pulse = sin(uTime * 3.0) * 0.15 + 0.85;

    // Mix colors: cyan at core, purple-tinted at edge
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 1.8) * pulse;

    float glowAlpha = glow * uOpacity * 0.4 * uIntensity * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique FugueConvergenceWave
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_ConvergenceWave();
    }
}

technique FugueConvergenceGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_ConvergenceGlow();
    }
}
