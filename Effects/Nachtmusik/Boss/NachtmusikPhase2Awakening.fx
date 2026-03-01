// ══════════════════════════════════════════════════════════╁E
// NachtmusikPhase2Awakening.fx  ENachtmusik boss phase 2
// Fake-death rebirth shimmer: starfield coalesces from
// darkness, silver light rebuilds form with nebula swirl.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;  // 0 = dark/dead, 1 = reborn
float4 uFromColor;          // Void darkness
float4 uToColor;            // Starlight silver
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_Phase2Awakening(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Stars coalescing inward  Eappear from edges first
    float2 starCell = floor(uv * 25.0);
    float starSeed = hash(starCell);
    float starDist = length(frac(uv * 25.0) - 0.5);
    float starThreshold = 1.0 - uTransitionProgress * 1.2 + dist * 0.3;
    float starMask = step(starThreshold, starSeed) * smoothstep(0.15, 0.0, starDist);
    float twinkle = sin(uTime * 6.0 + starSeed * 40.0) * 0.3 + 0.7;

    // Nebula swirl rebuilding form
    float n = tex2D(uNoiseTex, float2(angle / 6.28 + uTime * 0.3, dist * 2.0)).r;
    float nebulaReveal = smoothstep(1.0 - uTransitionProgress * 1.1, 1.0 - uTransitionProgress * 1.1 + 0.1, n);

    // Central light gathering  Egrows from center outward
    float centralGlow = smoothstep(uTransitionProgress * 0.5, 0.0, dist);
    centralGlow *= uTransitionProgress;

    // Edge shimmer at reveal boundary
    float edgeDist = abs(n - (1.0 - uTransitionProgress * 1.1));
    float edgeShimmer = smoothstep(0.06, 0.0, edgeDist) * uIntensity;

    // Colors: void to silver to starlight white
    float4 voidColor = uFromColor;
    float4 silverLight = uToColor;
    float4 starWhite = float4(1.0, 0.97, 1.0, 1.0);

    float4 color = voidColor * (1.0 - uTransitionProgress);
    color += silverLight * nebulaReveal * 0.5;
    color += starWhite * starMask * twinkle * uIntensity;
    color += silverLight * centralGlow * uIntensity;
    color += starWhite * edgeShimmer;

    float alpha = (1.0 - uTransitionProgress) * 0.2 + nebulaReveal * 0.4 + starMask * twinkle + centralGlow + edgeShimmer;
    alpha *= sin(uTransitionProgress * 3.14159) + uTransitionProgress * 0.5;

    return color * saturate(alpha);
}

technique Technique1
{
    pass Phase2Awakening
    {
        PixelShader = compile ps_3_0 PS_Phase2Awakening();
    }
}
