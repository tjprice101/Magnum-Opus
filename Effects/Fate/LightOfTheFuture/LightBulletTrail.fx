// =============================================================================
// Light of the Future — Bullet Trail Shader (ps_2_0 optimized)
// =============================================================================
// Accelerating cosmic bullet trail. Two passes: main trail + glow aura.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;

// Main bullet trail: core beam with speed-scaled colour
float4 BulletTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float speed = saturate(uPhase);

    float core = saturate(1.0 - cross / 0.3);
    core = core * core;
    float body = saturate(1.0 - cross);

    float leading = saturate(1.0 - progress * 2.5);
    leading = leading * leading;

    float3 color = lerp(uSecondaryColor, uColor, core * (0.5 + speed * 0.5));
    color = lerp(color, float3(0.9, 0.96, 1.0), core * leading * speed * 0.6);

    float alpha = (body * 0.4 + core * 0.5) * (1.0 - progress * 0.3);
    alpha *= uOpacity * sampleColor.a * baseTex.a;

    return float4(color * uIntensity * uOverbrightMult * baseTex.rgb, alpha);
}

// Wide glow aura that intensifies with speed
float4 AccelGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float speed = saturate(uPhase);

    float glow = saturate(1.0 - cross);
    glow = glow * glow * glow;
    float speedGlow = glow * (0.3 + speed * 0.7);

    float pulse = sin(uTime * 4.0 + progress * 10.0) * 0.1 + 0.9;
    float3 glowColor = lerp(float3(0.04, 0.01, 0.07), uColor * 0.6, speedGlow);
    float alpha = speedGlow * (1.0 - progress * 0.4) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.6;

    return float4(glowColor * uIntensity * uOverbrightMult, alpha);
}

technique BulletTrailMain
{
    pass P0
    {
        PixelShader = compile ps_2_0 BulletTrailPS();
    }
}

technique AccelGlowPass
{
    pass P0
    {
        PixelShader = compile ps_2_0 AccelGlowPS();
    }
}
