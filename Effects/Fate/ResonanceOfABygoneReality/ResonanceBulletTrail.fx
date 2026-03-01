// ResonanceBulletTrail.fx
// Cosmic bullet trail with gradient color and glow pass.
// Two techniques: BulletMain (core trail) + BulletGlow (soft bloom halo).

sampler uImage0 : register(s0);

float uTime;
float3 uColor;
float uOpacity;

struct VSOutput
{
    float4 Position : POSITION0;
    float4 Color    : COLOR0;
    float2 TexCoord : TEXCOORD0;
};

float4 BulletMainPS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(uImage0, uv);

    // Gradient along trail length (head bright, tail dark)
    float grad = 1.0 - uv.x;

    // Hot-to-palette color shift
    float3 col = lerp(uColor, float3(1, 1, 1), grad * 0.5);

    // Center brightness ridge
    float centerBright = 1.0 - abs(uv.y - 0.5) * 2.0;
    centerBright = pow(max(centerBright, 0.0), 1.5);

    // Temporal pulse
    float pulse = 0.9 + sin(uTime * 6.0 + uv.x * 10.0) * 0.1;

    float alpha = texColor.a * grad * centerBright * uOpacity * pulse;
    return float4(col * centerBright * pulse, alpha);
}

float4 BulletGlowPS(VSOutput input) : COLOR0
{
    float2 uv = input.TexCoord;
    float4 texColor = tex2D(uImage0, uv);

    float grad = 1.0 - uv.x;

    // Wider, softer center glow
    float centerBright = 1.0 - abs(uv.y - 0.5) * 2.0;
    centerBright = pow(max(centerBright, 0.0), 0.8);

    float3 glowCol = uColor * 1.5;
    float alpha = texColor.a * grad * centerBright * uOpacity * 0.6;

    return float4(glowCol * centerBright, alpha);
}

technique BulletMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 BulletMainPS();
    }
}

technique BulletGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 BulletGlowPS();
    }
}
