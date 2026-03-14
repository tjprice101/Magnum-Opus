// L'Estate - Solar Eclipse Screen Darkening
// Full-screen post-process: boss IS the light source
// Everything darkens based on distance from boss position

sampler uImage0 : register(s0);
float2 uBossPos;
float uNearRadius;
float uFarRadius;
float uMinBrightness;
float4 uTintColor;
float uIntensity;

float4 EclipsePS(float2 coords : TEXCOORD0) : COLOR0
{
    float4 screenColor = tex2D(uImage0, coords);

    float dist = length(coords - uBossPos);
    float brightness = smoothstep(uFarRadius, uNearRadius, dist);
    brightness = brightness * (1.0 - uMinBrightness) + uMinBrightness;

    // Tint dark regions toward deep amber
    float darkness = 1.0 - brightness;
    float4 eclipsed = screenColor * brightness + uTintColor * darkness * 0.12;

    // Lerp between original screen and eclipsed version
    float4 result = lerp(screenColor, eclipsed, uIntensity);
    result.a = screenColor.a;

    return result;
}

technique EclipseTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 EclipsePS();
    }
}
