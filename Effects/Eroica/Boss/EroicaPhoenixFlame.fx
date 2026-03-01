// ══════════════════════════════════════════════════════════╁E
// EroicaPhoenixFlame.fx  EEroica boss attack shader
// Used for PhoenixDive and major attack VFX. Creates a 
// rising phoenix-wing flame pattern with ascending embers.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uIntensity;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_PhoenixFlame(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    
    // Wing shape: two mirrored flame arcs
    float wingX = abs(centered.x);
    float wingY = centered.y;
    
    // Wing curve
    float wingCurve = wingX * 1.5 - wingY * 0.5 - 0.1;
    float wingMask = smoothstep(0.0, 0.1, wingCurve) * smoothstep(0.6, 0.3, wingCurve);
    
    // Rising fire noise
    float2 fireUV = float2(uv.x * 2.0, uv.y * 3.0 + uTime * 3.0);
    float fire = tex2D(uNoiseTex, fireUV).r;
    float2 fireUV2 = float2(uv.x * 4.0 + 0.5, uv.y * 2.0 + uTime * 5.0);
    float fire2 = tex2D(uNoiseTex, fireUV2).r;
    float fireComposite = fire * 0.6 + fire2 * 0.4;
    
    // Ascending embers  Esmall bright dots
    float emberSeed = hash(floor(uv * 20.0 + float2(0, uTime * 2.0)));
    float embers = step(0.95, emberSeed);
    
    // Color gradient: dark scarlet (base) -> orange (mid) -> gold (hot) -> white (core)
    float heat = fireComposite * wingMask * uIntensity;
    float4 darkScarlet = float4(0.5, 0.05, 0.02, 1.0);
    float4 orange = float4(1.0, 0.4, 0.05, 1.0);
    float4 gold = uColor;
    float4 white = float4(1.0, 0.98, 0.9, 1.0);
    
    float4 color;
    if (heat < 0.33)
        color = lerp(darkScarlet, orange, heat / 0.33);
    else if (heat < 0.66)
        color = lerp(orange, gold, (heat - 0.33) / 0.33);
    else
        color = lerp(gold, white, (heat - 0.66) / 0.34);
    
    float alpha = wingMask * fireComposite * uIntensity;
    alpha += embers * 0.8 * uIntensity;
    
    return color * saturate(alpha);
}

technique Technique1
{
    pass PhoenixFlame
    {
        PixelShader = compile ps_3_0 PS_PhoenixFlame();
    }
}
