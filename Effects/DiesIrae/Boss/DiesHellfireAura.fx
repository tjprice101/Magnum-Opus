// ═══════════════════════════════════════════════════════════
// DiesHellfireAura.fx — Dies Irae boss ambient hellfire aura
// Blood red hellfire with dark crimson smoke billowing and
// ember particles rising from the infernal presence.
// ═══════════════════════════════════════════════════════════

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Blood red
float4 uSecondaryColor;  // Dark crimson
float uTime;

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 PS_HellfireAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Aggressive pulsing — wrath beats like a heart
    float pulse = sin(uTime * 4.0) * 0.12 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Rising hellfire — two octaves scrolling upward
    float fire1 = noise(float2(angle * 3.0, dist * 8.0 + uTime * 2.0));
    float fire2 = noise(float2(angle * 5.0 + 1.0, dist * 12.0 + uTime * 3.5));
    float hellfire = fire1 * 0.6 + fire2 * 0.4;

    // Dark crimson smoke — slower, billowing
    float smoke = noise(float2(angle * 2.0 - uTime * 0.5, dist * 5.0 - uTime * 0.8));
    float smokeMask = smoothstep(0.3, 0.7, smoke) * falloff;

    // Rising ember particles
    float emberSeed = hash(floor(float2(uv.x * 15.0, uv.y * 20.0 - uTime * 3.0)));
    float embers = step(0.96, emberSeed);
    float emberGlow = sin(uTime * 8.0 + emberSeed * 40.0) * 0.3 + 0.7;

    // Color: blood red fire, dark crimson smoke, ember orange tips
    float4 fireColor = lerp(uPrimaryColor, uSecondaryColor, hellfire);
    float4 emberColor = float4(1.0, 0.5, 0.1, 1.0);

    float alpha = falloff * (hellfire * 0.6 + smokeMask * 0.3 + 0.1) * uIntensity;
    float4 color = fireColor * alpha;
    color += uSecondaryColor * smokeMask * uIntensity * 0.4;
    color += emberColor * embers * emberGlow * falloff * uIntensity;

    return color;
}

technique Technique1
{
    pass HellfireAura
    {
        PixelShader = compile ps_3_0 PS_HellfireAura();
    }
}
