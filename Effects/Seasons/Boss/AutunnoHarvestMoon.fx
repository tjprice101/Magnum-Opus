// ══════════════════════════════════════════════════════════╁E
// AutunnoHarvestMoon.fx  ESeasons/Autunno phase transition
// Harvest moon rising  Efading orange-to-deep-brown with
// warm moon glow accent, transitioning between boss phases.
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

float4 PS_HarvestMoon(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Moon glow  Ecircular disc rising from bottom
    float moonY = 0.6 - uTransitionProgress * 0.4;
    float2 moonCenter = float2(0.5, moonY);
    float moonDist = length(uv - moonCenter);
    float moonGlow = smoothstep(0.2, 0.0, moonDist);
    float moonHalo = smoothstep(0.35, 0.15, moonDist) * 0.4;

    // Phase blend with noise-driven edge
    float transNoise = noise(float2(uv.x * 6.0 + uTime * 0.5, uv.y * 6.0));
    float transEdge = uTransitionProgress + transNoise * 0.15;
    float phaseMix = smoothstep(0.4, 0.6, transEdge);

    // Harvest warmth  Eamber radiance spreading
    float warmth = noise(float2(angle * 3.0 + uTime * 0.4, dist * 5.0));
    float warmMask = smoothstep(0.3, 0.7, warmth) * (1.0 - dist * 1.5);

    // Color transition: from orange to deep brown
    float4 phaseColor = lerp(uFromColor, uToColor, phaseMix);

    // Moon disc  Ewarm amber-orange
    float4 moonColor = float4(1.0, 0.75, 0.3, 1.0);
    float4 moonHaloColor = float4(0.9, 0.55, 0.15, 1.0);

    float4 result = base;
    result.rgb = lerp(result.rgb, phaseColor.rgb, phaseMix * uIntensity * 0.6);
    result.rgb += moonColor.rgb * moonGlow * uIntensity;
    result.rgb += moonHaloColor.rgb * moonHalo * uIntensity;
    result.rgb += phaseColor.rgb * warmMask * uIntensity * 0.3;

    return result;
}

technique Technique1
{
    pass HarvestMoon
    {
        PixelShader = compile ps_3_0 PS_HarvestMoon();
    }
}
