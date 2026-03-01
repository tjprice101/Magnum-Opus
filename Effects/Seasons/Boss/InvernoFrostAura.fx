// ══════════════════════════════════════════════════════════╁E
// InvernoFrostAura.fx  ESeasons/Inverno boss ambient aura
// Winter frost aura with crystalline ice patterns, blue/
// white/silver cold light and drifting snowflake accents.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Ice blue
float4 uSecondaryColor;  // Frost white/silver
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

float4 PS_FrostAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Cold pulse  Eslow, crystalline breathing
    float pulse = sin(uTime * 1.5) * 0.04 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Ice crystal pattern  E6-fold symmetry like snowflake
    float crystalAngle = angle + uTime * 0.2;
    float crystal = abs(sin(crystalAngle * 3.0));
    crystal = pow(crystal, 3.0);
    float crystalBranch = abs(sin(crystalAngle * 6.0 + dist * 20.0));
    crystalBranch = pow(crystalBranch, 8.0) * smoothstep(radiusNorm, 0.0, dist);

    // Frost creep noise  Eorganic spreading
    float frost = noise(float2(angle * 4.0 + uTime * 0.3, dist * 10.0));
    float frostPattern = smoothstep(0.35, 0.65, frost);

    // Drifting snowflake accents
    float snowSeed = hash(floor(float2(uv.x * 20.0 + uTime * 0.3, uv.y * 25.0 - uTime * 0.5)));
    float snow = step(0.95, snowSeed);
    float snowTwinkle = sin(uTime * 3.0 + snowSeed * 20.0) * 0.3 + 0.7;

    // Color: ice blue base, frost white crystals, silver accents
    float colorMix = frostPattern * crystal;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    float4 iceWhite = float4(0.9, 0.95, 1.0, 1.0);

    float alpha = falloff * (frostPattern * 0.5 + 0.2) * uIntensity;
    float4 color = baseColor * alpha;
    color += iceWhite * crystalBranch * uIntensity * 0.6;
    color += uSecondaryColor * snow * snowTwinkle * falloff * uIntensity * 0.4;

    return color;
}

technique Technique1
{
    pass FrostAura
    {
        PixelShader = compile ps_3_0 PS_FrostAura();
    }
}
