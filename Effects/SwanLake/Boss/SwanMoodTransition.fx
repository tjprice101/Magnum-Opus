// SwanMoodTransition.fx  EMood transition (Graceful→Tempest→DyingSwan)
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float4 PS_MoodTransition(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float n = tex2D(uNoiseTex, uv * 2.5 + float2(uTime * 0.2, 0)).r;
    float featherN = tex2D(uNoiseTex, float2(angle / 6.283 * 3.0, dist * 5.0 + uTime * 0.5)).r;
    float reveal = step(n, uTransitionProgress * 1.1);
    float edge = smoothstep(0.06, 0.0, abs(n - uTransitionProgress * 1.1));
    float feathers = smoothstep(0.6, 0.9, featherN) * edge;
    float4 color = lerp(uFromColor, uToColor, reveal);
    float4 edgeWhite = float4(1, 1, 1, 1);
    color = lerp(color, edgeWhite, edge * 0.8);
    float alpha = (reveal * 0.2 + edge * uIntensity + feathers * 0.5);
    alpha *= sin(uTransitionProgress * 3.14159);
    return color * saturate(alpha);
}

technique Technique1
{
    pass MoodTransition { PixelShader = compile ps_3_0 PS_MoodTransition(); }
}
