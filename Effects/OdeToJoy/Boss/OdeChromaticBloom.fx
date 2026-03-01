// ══════════════════════════════════════════════════════════╁E
// OdeChromaticBloom.fx  EOde to Joy boss phase transition
// Chromatic bloom burst: jubilant golden light expanding
// outward with prismatic rainbow fringe and warm radiance.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;
float4 uFromColor;          // Warm gold
float4 uToColor;            // Jubilant bright light
float uIntensity;
float uTime;

float4 PS_ChromaticBloom(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Expanding bloom radius
    float bloomRadius = uTransitionProgress * 0.6;
    float bloomMask = smoothstep(bloomRadius + 0.1, bloomRadius - 0.05, dist);

    // Chromatic ring at bloom edge  Eprismatic rainbow fringe
    float ringDist = abs(dist - bloomRadius);
    float chromaticRing = smoothstep(0.08, 0.0, ringDist);

    // Rainbow hue shift along the ring
    float hue = frac(angle / 6.28 + uTime * 0.5);
    float4 rainbow;
    rainbow.r = saturate(abs(hue * 6.0 - 3.0) - 1.0);
    rainbow.g = saturate(2.0 - abs(hue * 6.0 - 2.0));
    rainbow.b = saturate(2.0 - abs(hue * 6.0 - 4.0));
    rainbow.a = 1.0;

    // Petal-shaped noise reveal
    float n = tex2D(uNoiseTex, uv * 2.0 + float2(uTime * 0.2, 0)).r;
    float petalReveal = smoothstep(1.0 - uTransitionProgress * 1.2, 1.0, n + bloomMask * 0.5);

    // Central radiance buildup
    float centralGlow = smoothstep(0.2, 0.0, dist) * uTransitionProgress;

    // Color composition
    float4 baseColor = lerp(uFromColor, uToColor, uTransitionProgress);
    float4 whiteLight = float4(1.0, 0.98, 0.9, 1.0);

    float4 color = baseColor * bloomMask * 0.5;
    color += rainbow * chromaticRing * uIntensity;
    color += baseColor * petalReveal * 0.4;
    color += whiteLight * centralGlow * uIntensity;

    float alpha = (bloomMask * 0.3 + chromaticRing + petalReveal * 0.3 + centralGlow);
    alpha *= sin(uTransitionProgress * 3.14159) + uTransitionProgress * 0.3;

    return color * saturate(alpha);
}

technique Technique1
{
    pass ChromaticBloom
    {
        PixelShader = compile ps_3_0 PS_ChromaticBloom();
    }
}
