// ResonanceMuzzleFlash.fx
// Muzzle flash / impact flash shader  Eradial burst with rapid falloff.

sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float uOpacity;
float uIntensity;

struct VSOutput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 MuzzleFlashPS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(uImage0, uv);

    // Radial falloff from center
    float2 center = uv - float2(0.5, 0.5);
    float dist = length(center) * 2.0;
    float radial = saturate(1.0 - dist);
    radial = pow(radial, 1.5);

    // Flash pulse (rapid decay driven by uIntensity)
    float flash = saturate(uIntensity) * radial;

    // Hot core to palette color at edges
    float3 col = lerp(float3(1, 1, 1), uColor, dist);

    float alpha = texColor.a * flash * uOpacity;
    return float4(col * flash, alpha);
}

technique Technique1
{
    pass P0
    {
        PixelShader = compile ps_3_0 MuzzleFlashPS();
    }
}
