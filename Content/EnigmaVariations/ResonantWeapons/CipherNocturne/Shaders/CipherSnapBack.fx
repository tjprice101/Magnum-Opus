// ============================================================================
// CipherSnapBack.fx — CipherNocturne snap-back implosion shader
// Renders the reality snap-back effect as a contracting distortion ring
// with void-purple to bright-green energy collapse
// ============================================================================

sampler uImage0 : register(s0);  // Base texture (explosion sprite)
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary color (purple)
float3 uSecondaryColor;  // Secondary color (green)
float uOpacity;           // Overall opacity
float uTime;              // Animation progress (0→1)
float uIntensity;         // Effect strength

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
    output.Position = float4(input.Position, 0, 1);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 PS_SnapBack(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    
    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);
    
    // Contracting ring effect
    float ringRadius = lerp(0.5, 0.0, uTime);
    float ringWidth = 0.12 * (1.0 - uTime);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringWidth, 0.0, ringDist);
    
    // Noise distortion on the ring
    float2 noiseUV = float2(angle * 0.5 + uTime, dist * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    ring *= (noise * 0.4 + 0.6);
    
    // Color: purple outer → bright green inner
    float colorMix = saturate(1.0 - dist * 2.0 + uTime);
    float3 snapColor = lerp(uColor, uSecondaryColor, colorMix);
    
    // Brightness flash at the moment of collapse
    float flash = exp(-uTime * 4.0) * uIntensity;
    snapColor += float3(1, 1, 1) * flash * 0.3;
    
    float alpha = ring * uOpacity * input.Color.a;
    
    return float4(snapColor, alpha);
}

technique CipherSnapBackMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_SnapBack();
    }
}
