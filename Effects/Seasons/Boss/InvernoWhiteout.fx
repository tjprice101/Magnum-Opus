// ══════════════════════════════════════════════════════════╕
// InvernoWhiteout.fx — Seasons/Inverno blizzard whiteout
// Full-screen whiteout effect for Phase 3 blizzard.
// Pulsing white fog with noise-driven density variation,
// wind-streaked patterns, and breathing intensity.
// ══════════════════════════════════════════════════════════╕

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTime;
float uIntensity;     // 0-1: overall whiteout strength
float uWindDirection; // Radians: direction of wind streaking

float4 PS_Whiteout(float2 uv : TEXCOORD0) : COLOR0
{
    // Wind-streaked UV offset
    float windX = cos(uWindDirection);
    float windY = sin(uWindDirection);

    // Large-scale fog movement
    float2 fogUV1 = uv * 2.0 + float2(uTime * 0.08 * windX, uTime * 0.03 * windY);
    float2 fogUV2 = uv * 4.0 + float2(uTime * 0.12 * windX, uTime * 0.05 * windY);
    float2 fogUV3 = uv * 1.0 + float2(uTime * 0.04, -uTime * 0.02);
    float fog1 = tex2D(uNoiseTex, fogUV1).r;
    float fog2 = tex2D(uNoiseTex, fogUV2).r;
    float fog3 = tex2D(uNoiseTex, fogUV3).r;

    // Layered fog density
    float fogDensity = fog1 * 0.5 + fog2 * 0.3 + fog3 * 0.2;

    // Wind streaks — elongated along wind direction
    float2 streakUV = uv * float2(1.0, 8.0);
    float streakRotX = streakUV.x * windX - streakUV.y * windY;
    float streakRotY = streakUV.x * windY + streakUV.y * windX;
    float2 rotatedStreakUV = float2(streakRotX, streakRotY) + float2(uTime * 0.2, 0);
    float streaks = tex2D(uNoiseTex, rotatedStreakUV * 0.5).r;
    float streakMask = smoothstep(0.4, 0.7, streaks) * 0.4;

    // Breathing intensity — pulsing whiteout
    float breathe = sin(uTime * 1.5) * 0.15 + 0.85;

    // Edge vignette — slightly clearer at center
    float2 centerDist = uv - 0.5;
    float vignette = 1.0 - dot(centerDist, centerDist) * 0.5;

    // Final composition
    float baseWhiteout = fogDensity * uIntensity * breathe;
    float totalAlpha = saturate(baseWhiteout * vignette + streakMask * uIntensity * 0.5);

    float4 whiteColor = uColor;
    float4 streakColor = float4(0.92, 0.95, 1.0, 1.0);

    float4 color = lerp(whiteColor, streakColor, streakMask / max(totalAlpha, 0.01));

    return color * totalAlpha;
}

technique Technique1
{
    pass Whiteout
    {
        PixelShader = compile ps_3_0 PS_Whiteout();
    }
}
