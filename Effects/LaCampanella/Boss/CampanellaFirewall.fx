// CampanellaFirewall.fx  EPhase transition/firewall attack shader
// Creates a wall of fire with bell silhouette cutouts
sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
float uTime;

float4 PS_Firewall(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float fire1 = tex2D(uNoiseTex, float2(uv.x * 3.0, uv.y * 2.0 + uTime * 3.0)).r;
    float fire2 = tex2D(uNoiseTex, float2(uv.x * 2.0 + 0.5, uv.y * 3.0 + uTime * 4.0)).r;
    float fire = fire1 * 0.6 + fire2 * 0.4;
    float wallMask = smoothstep(0.2, 0.0, abs(centered.x) - uTransitionProgress * 0.5);
    float heat = fire * wallMask;
    float4 coolFire = uFromColor;
    float4 hotFire = uToColor;
    float4 white = float4(1, 0.95, 0.85, 1);
    float4 color = lerp(coolFire, hotFire, heat);
    color = lerp(color, white, step(0.85, heat) * 0.6);
    float alpha = wallMask * fire * uIntensity;
    alpha *= sin(uTransitionProgress * 3.14159);
    return color * saturate(alpha);
}

technique Technique1
{
    pass Firewall { PixelShader = compile ps_3_0 PS_Firewall(); }
}
