// ══════════════════════════════════════════════════════════╁E
// NachtmusikStarfieldAura.fx  ENachtmusik boss ambient aura
// Deep indigo starfield with twinkling silver stars and
// shooting star streaks across the nocturnal void.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Deep indigo
float4 uSecondaryColor;  // Starlight silver
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

float4 PS_StarfieldAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Radius falloff with gentle pulse
    float pulse = sin(uTime * 1.5) * 0.05 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Indigo nebula base  Eslow swirl
    float n = noise(float2(angle * 2.0 + uTime * 0.2, dist * 6.0 - uTime * 0.3));
    float nebulaBase = n * 0.5 + 0.5;

    // Twinkling star grid
    float2 starCell = floor(uv * 30.0);
    float starSeed = hash(starCell);
    float starMask = step(0.92, starSeed);
    float twinkle = sin(uTime * (4.0 + starSeed * 8.0) + starSeed * 50.0) * 0.4 + 0.6;
    float stars = starMask * twinkle;

    // Shooting star streaks  Erare diagonal lines
    float shootSeed = hash(floor(float2(uv.x * 5.0 + uTime * 2.0, uv.y * 3.0)));
    float shootMask = step(0.985, shootSeed);
    float shootTrail = smoothstep(0.0, 0.05, frac(uv.x * 5.0 + uTime * 2.0));
    float shootingStar = shootMask * shootTrail;

    // Color composition
    float colorMix = nebulaBase;
    float4 baseColor = lerp(uPrimaryColor, uPrimaryColor * 1.3, colorMix);
    float4 silverStar = uSecondaryColor;

    float alpha = falloff * (nebulaBase * 0.4 + 0.2) * uIntensity;
    float4 color = baseColor * alpha;
    color += silverStar * stars * falloff * uIntensity;
    color += float4(1.0, 0.95, 0.9, 1.0) * shootingStar * falloff * uIntensity * 1.5;

    return color;
}

technique Technique1
{
    pass StarfieldAura
    {
        PixelShader = compile ps_3_0 PS_StarfieldAura();
    }
}
