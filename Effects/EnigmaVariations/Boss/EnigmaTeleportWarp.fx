// EnigmaTeleportWarp.fx — Teleport in/out warp visual
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float4 PS_TeleportWarp(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    // Warping distortion that collapses to center (or expands from center)
    float warpDist = dist / (1.0 + uTransitionProgress * 3.0);
    float warpAngle = angle + uTransitionProgress * 5.0 * dist;
    float warpNoise = tex2D(uNoiseTex, float2(warpAngle / 6.283 * 2.0, warpDist * 3.0 + uTime)).r;
    // Sigil rings that appear during teleport
    float sigil1 = abs(dist - 0.2 * (1.0 - uTransitionProgress));
    float sigil2 = abs(dist - 0.35 * (1.0 - uTransitionProgress));
    float sigilMask = smoothstep(0.015, 0.0, sigil1) + smoothstep(0.01, 0.0, sigil2);
    // Void collapse
    float collapse = smoothstep(0.5 * (1.0 - uTransitionProgress), 0.0, dist);
    float4 color = lerp(uFromColor, uToColor, uTransitionProgress);
    color += float4(0.3, 1.0, 0.4, 1) * sigilMask * 0.5;
    color += uToColor * collapse * 0.3;
    float alpha = (warpNoise * 0.3 + sigilMask * 0.7 + collapse * 0.4) * uIntensity;
    alpha *= sin(uTransitionProgress * 3.14159);
    return color * saturate(alpha);
}

technique Technique1
{
    pass TeleportWarp { PixelShader = compile ps_3_0 PS_TeleportWarp(); }
}
