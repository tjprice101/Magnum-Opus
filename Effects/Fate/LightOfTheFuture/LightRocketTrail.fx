// =============================================================================
// Light of the Future — Rocket Trail Shader (ps_2_0 optimized)
// =============================================================================
// Spiraling homing rocket trail. Crimson-gold fire with violet edges.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;

// Rocket trail: crimson-gold fire core with spiral wobble
float4 RocketTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Spiral wobble
    float spiral = sin(progress * 12.0 + uPhase + uTime * 3.0) * 0.08;
    float adjCross = abs((coords.y + spiral) - 0.5) * 2.0;

    float core = saturate(1.0 - adjCross / 0.35);
    core = core * core;
    float body = saturate(1.0 - adjCross);

    // Color: violet edge → crimson body → gold core
    float3 color = lerp(float3(0.35, 0.08, 0.5), uColor, body);
    color = lerp(color, uSecondaryColor, core * 0.7);
    color = lerp(color, float3(1.0, 0.95, 0.9), core * (1.0 - progress) * 0.4);

    float alpha = (body * 0.4 + core * 0.5) * (1.0 - progress * 0.5);
    alpha *= uOpacity * sampleColor.a * baseTex.a;

    return float4(color * uIntensity * uOverbrightMult * baseTex.rgb, alpha);
}

technique RocketTrailMain
{
    pass P0
    {
        PixelShader = compile ps_2_0 RocketTrailPS();
    }
}
