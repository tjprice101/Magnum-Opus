// ══════════════════════════════════════════════════════════╁E
// EstateHeatHazeTrail.fx  ESeasons/Estate boss movement trail
// Heat distortion movement trail with shimmering mirages,
// rising hot air wisps and scorched ground glow.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float4 PS_HeatHazeTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Heat distortion  Ewavy displacement noise
    float2 hazeUV1 = float2(uv.x * 3.0 - uTime * 1.0, uv.y * 4.0 + uTime * 3.0);
    float haze1 = tex2D(uNoiseTex, hazeUV1).r;
    float2 hazeUV2 = float2(uv.x * 5.0 + uTime * 0.5, uv.y * 6.0 + uTime * 4.0);
    float haze2 = tex2D(uNoiseTex, hazeUV2).r;
    float hazeComposite = haze1 * 0.6 + haze2 * 0.4;

    // Rising hot air columns from trail center
    float risingAir = smoothstep(0.6, 0.1, trailWidth) * hazeComposite;

    // Scorched ground glow  Ehot core at trail center
    float scorchGlow = smoothstep(0.4, 0.0, trailWidth);
    float scorchFlicker = haze1 * 0.3 + 0.7;

    // Mirage shimmer at edges  Etransparent heat distortion
    float mirageEdge = smoothstep(0.3, 0.7, trailWidth) * smoothstep(1.0, 0.7, trailWidth);
    float mirage = mirageEdge * hazeComposite * 0.5;

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: golden scorched core, orange rising heat, faint mirage
    float4 scorchGold = uColor;
    float4 orangeHeat = float4(1.0, 0.55, 0.1, 1.0);
    float4 whiteHot = float4(1.0, 0.95, 0.8, 1.0);

    float4 color = scorchGold * scorchGlow * scorchFlicker;
    color += orangeHeat * risingAir * 0.7;
    color += whiteHot * mirage * 0.3;

    float alpha = (scorchGlow * 0.5 + risingAir * 0.4 + mirage * 0.2) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass HeatHazeTrail
    {
        PixelShader = compile ps_3_0 PS_HeatHazeTrail();
    }
}
