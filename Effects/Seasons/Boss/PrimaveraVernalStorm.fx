// ══════════════════════════════════════════════════════════╁E
// PrimaveraVernalStorm.fx  ESeasons/Primavera phase transition
// Spring storm with flower petals  Eswirling wind carrying
// petals across the screen during boss phase change.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float uTransitionProgress;
float4 uFromColor;
float4 uToColor;
float uIntensity;
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

float4 PS_VernalStorm(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Storm swirl  Erotating wind pattern
    float swirlAngle = angle - dist * 8.0 + uTime * 3.0;
    float swirl = sin(swirlAngle * 4.0) * 0.5 + 0.5;
    swirl *= smoothstep(0.6, 0.1, dist);

    // Petal scatter  Esmall petal shapes carried by wind
    float2 petalUV = float2(uv.x + sin(uv.y * 8.0 + uTime * 2.5) * 0.1, uv.y - uTime * 0.6);
    float petalNoise = noise(petalUV * 12.0);
    float petals = smoothstep(0.75, 0.88, petalNoise);

    // Phase transition wave  Esweeps across screen
    float transWave = uv.x + noise(float2(uv.y * 5.0, uTime)) * 0.15;
    float phaseMix = smoothstep(0.4, 0.6, uTransitionProgress + (1.0 - transWave) * 0.3);

    // Rain streaks  Ediagonal spring rain
    float rain = noise(float2(uv.x * 3.0 + uv.y * 15.0 - uTime * 8.0, uv.y * 2.0));
    float rainStreaks = smoothstep(0.85, 0.95, rain) * 0.3;

    // Colors: spring green base, pink petals, white rain highlights
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);
    float4 petalPink = float4(1.0, 0.6, 0.7, 1.0);
    float4 rainWhite = float4(0.85, 0.9, 1.0, 1.0);

    float4 result = base;
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.5);
    result.rgb += phaseColor.rgb * swirl * uIntensity * 0.3;
    result.rgb += petalPink.rgb * petals * uIntensity * 0.8;
    result.rgb += rainWhite.rgb * rainStreaks * uIntensity;

    return result;
}

technique Technique1
{
    pass VernalStorm
    {
        PixelShader = compile ps_3_0 PS_VernalStorm();
    }
}
