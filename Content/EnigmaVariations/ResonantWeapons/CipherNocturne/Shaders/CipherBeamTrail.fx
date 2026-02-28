// ============================================================================
// CipherBeamTrail.fx — CipherNocturne beam trail shader
// Renders the channeled beam as a scrolling void-distortion strip
// with purple-to-green color transition and reality-fraying noise
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary beam color (purple)
float3 uSecondaryColor;  // Secondary color (green)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling
float uIntensity;         // Channel intensity (ramps 0→1)
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

float4 PS_BeamFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    
    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;
    
    // Scrolling UV for flow effect
    float2 scrollUV = float2(coords.x - uTime * 0.8, coords.y);
    
    // Sample noise for distortion
    float2 noiseUV = float2(coords.x * 2.0 - uTime * 0.5, coords.y * 1.5);
    float noise = tex2D(uImage1, noiseUV).r;
    
    // Distort the main UV with noise
    float distortAmt = 0.05 * uIntensity;
    float2 distortedUV = scrollUV + float2(noise * distortAmt, noise * distortAmt * 0.5);
    
    // Sample base trail
    float4 baseSample = tex2D(uImage0, distortedUV);
    
    // Edge fade (soft edges along beam width)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.6, 1.0, edgeDist);
    
    // Color mixing: purple core → green edges, modulated by noise
    float colorMix = saturate(noise * 0.6 + edgeDist * 0.4);
    float3 beamColor = lerp(uColor, uSecondaryColor, colorMix);
    
    // Brightness intensifies along beam length (further = more unraveled)
    float lengthBright = lerp(0.6, 1.3, coords.x) * uIntensity;
    
    // Final color
    float3 finalColor = beamColor * lengthBright * (baseSample.r * 0.6 + 0.4);
    float finalAlpha = edgeFade * uOpacity * input.Color.a;
    
    return float4(finalColor, finalAlpha);
}

float4 PS_BeamGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;
    
    // Soft glow — wider and more diffuse than the main beam
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float glow = exp(-edgeDist * edgeDist * 3.0);
    
    // Pulse with time
    float pulse = sin(uTime * 3.0) * 0.15 + 0.85;
    
    float3 glowColor = lerp(uColor, uSecondaryColor, edgeDist) * pulse;
    float glowAlpha = glow * uOpacity * 0.4 * uIntensity * input.Color.a;
    
    return float4(glowColor, glowAlpha);
}

technique CipherBeamFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamFlow();
    }
}

technique CipherBeamGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamGlow();
    }
}
