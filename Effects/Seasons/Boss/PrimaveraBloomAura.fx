// ══════════════════════════════════════════════════════════╁E
// PrimaveraBloomAura.fx  ESeasons/Primavera boss ambient aura
// Spring bloom aura with growing flower patterns and
// fresh green/pink/yellow palette of new life emerging.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Fresh green
float4 uSecondaryColor;  // Blossom pink
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

float4 PS_BloomAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Gentle bloom breathing
    float pulse = sin(uTime * 2.5) * 0.07 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Flower petal shape  E6 petals expanding/rotating
    float petalAngle = angle + uTime * 0.4;
    float petalShape = cos(petalAngle * 3.0) * 0.5 + 0.5;
    petalShape = smoothstep(0.2, 0.8, petalShape);

    // Growing vine noise  Eorganic spreading
    float vine = noise(float2(angle * 4.0 + uTime * 0.6, dist * 10.0 - uTime * 0.5));
    float vinePattern = smoothstep(0.4, 0.7, vine);

    // Pollen mote sparkles  Eyellow dots drifting upward
    float pollenSeed = hash(floor(float2(uv.x * 25.0, uv.y * 30.0 - uTime * 0.8)));
    float pollen = step(0.95, pollenSeed);
    float pollenGlow = sin(uTime * 4.0 + pollenSeed * 30.0) * 0.3 + 0.7;

    // Color: green base, pink petals, yellow pollen highlights
    float colorMix = petalShape * vinePattern;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    float4 yellowPollen = float4(1.0, 0.95, 0.4, 1.0);

    float alpha = falloff * (vinePattern * 0.5 + petalShape * 0.3 + 0.2) * uIntensity;
    float4 color = baseColor * alpha;
    color += yellowPollen * pollen * pollenGlow * falloff * uIntensity * 0.5;

    return color;
}

technique Technique1
{
    pass BloomAura
    {
        PixelShader = compile ps_3_0 PS_BloomAura();
    }
}
