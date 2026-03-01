// ═══════════════════════════════════════════════════════════
// DiesJudgmentTrail.fx — Dies Irae boss movement trail
// Ember-trailing judgment movement with scorched ground
// effect, burning embers and dark crimson smoke wake.
// ═══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_JudgmentTrail(float2 uv : TEXCOORD0) : COLOR0
{
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    // Scorched core — hot center fading to ash at edges
    float2 fireUV = float2(uv.x * 3.0 - uTime * 3.0, uv.y * 2.0 + uTime * 1.0);
    float fireNoise = tex2D(uNoiseTex, fireUV).r;
    float scorchCore = smoothstep(0.7, 0.2, trailWidth) * fireNoise;

    // Billowing crimson smoke at trail edges
    float2 smokeUV = float2(uv.x * 2.0 - uTime * 1.5, uv.y * 3.0 - uTime * 0.5);
    float smoke = tex2D(uNoiseTex, smokeUV).r;
    float smokeMask = smoothstep(0.4, 0.8, trailWidth) * smoke;
    smokeMask *= smoothstep(1.0, 0.6, trailWidth);

    // Glowing embers scattered along trail
    float2 emberUV = float2(uv.x * 8.0 - uTime * 5.0, uv.y * 4.0);
    float emberNoise = tex2D(uNoiseTex, emberUV).r;
    float embers = smoothstep(0.8, 0.95, emberNoise);
    float emberFlicker = hash(floor(uv * 30.0 + uTime)) * 0.4 + 0.6;

    // Age fade
    float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

    // Colors: blood red core, dark crimson smoke, orange-white embers
    float4 bloodRed = uColor;
    float4 darkCrimson = float4(0.3, 0.02, 0.02, 1.0);
    float4 emberOrange = float4(1.0, 0.5, 0.1, 1.0);

    float4 color = bloodRed * scorchCore;
    color += darkCrimson * smokeMask * 0.6;
    color += emberOrange * embers * emberFlicker;

    float alpha = (scorchCore + smokeMask * 0.4 + embers * 0.6) * ageFade * uTrailWidth;

    return color * saturate(alpha);
}

technique Technique1
{
    pass JudgmentTrail
    {
        PixelShader = compile ps_3_0 PS_JudgmentTrail();
    }
}
