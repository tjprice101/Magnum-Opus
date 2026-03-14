// L'Estate - Persistent Afterburn Screen Marks
// Two techniques:
//   BurnFadeTechnique  - applied to burn RT each frame to fade marks
//   BurnCompositeTechnique - composites burn RT over game (additive)

sampler uImage0 : register(s0);
float uTime;
float uFadeRate;
float4 uBurnColor;
float uIntensity;

// Technique 1: Gradually fade existing burn marks
float4 BurnFadePS(float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);
    color *= uFadeRate;
    return color;
}

// Technique 2: Composite burn RT with temperature-based color ramp
float4 BurnCompositePS(float2 coords : TEXCOORD0) : COLOR0
{
    float4 burnSample = tex2D(uImage0, coords);
    float brightness = dot(burnSample.rgb, float3(0.299, 0.587, 0.114));

    // Procedural temperature color ramp
    // 1.0=white-hot, 0.7=sun gold, 0.4=blazing orange, 0.2=deep red, 0.0=gone
    float4 rampColor;
    float t;

    if (brightness > 0.7)
    {
        t = (brightness - 0.7) / 0.3;
        rampColor = lerp(float4(1.0, 0.78, 0.2, 1.0), float4(1.0, 0.98, 0.94, 1.0), t);
    }
    else if (brightness > 0.4)
    {
        t = (brightness - 0.4) / 0.3;
        rampColor = lerp(float4(1.0, 0.55, 0.16, 1.0), float4(1.0, 0.78, 0.2, 1.0), t);
    }
    else if (brightness > 0.15)
    {
        t = (brightness - 0.15) / 0.25;
        rampColor = lerp(float4(0.59, 0.12, 0.04, 1.0), float4(1.0, 0.55, 0.16, 1.0), t);
    }
    else
    {
        t = brightness / 0.15;
        rampColor = lerp(float4(0.0, 0.0, 0.0, 0.0), float4(0.59, 0.12, 0.04, 0.8), t);
    }

    rampColor *= uIntensity;
    rampColor.a = saturate(brightness * uIntensity * 1.5);

    return rampColor;
}

technique BurnFadeTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 BurnFadePS();
    }
}

technique BurnCompositeTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 BurnCompositePS();
    }
}
