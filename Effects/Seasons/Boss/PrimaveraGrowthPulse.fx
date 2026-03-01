// ══════════════════════════════════════════════════════════╁E
// PrimaveraGrowthPulse.fx  ESeasons/Primavera attack flash
// Blooming growth burst  Egreen-pink petal burst pattern
// radiating outward with spring energy and floral accents.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uIntensity;
float4 uColor;
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

float4 PS_GrowthPulse(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 delta = uv - uCenter;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Petal burst  E5-fold symmetry like a flower
    float petalAngle = angle + uTime * 2.0;
    float petals = abs(sin(petalAngle * 2.5));
    petals = pow(petals, 2.0);
    float petalShape = petals * smoothstep(0.4, 0.0, dist);

    // Growth pulse ring expanding outward
    float ringPos = frac(uTime * 1.5) * 0.6;
    float ring = smoothstep(0.03, 0.0, abs(dist - ringPos));
    ring *= smoothstep(0.6, 0.0, dist);

    // Radial falloff
    float radialFade = exp(-dist * 3.5) * uIntensity;

    // Blooming noise  Eorganic spread
    float bloom = noise(float2(angle * 3.0 + uTime * 2.5, dist * 10.0 - uTime * 4.0));
    float bloomMask = smoothstep(0.4, 0.75, bloom) * radialFade;

    // Colors: green core, pink petal tips, white highlights
    float4 greenGrowth = uColor;
    float4 pinkPetal = float4(1.0, 0.5, 0.65, 1.0);
    float4 whiteHighlight = float4(1.0, 1.0, 0.95, 1.0);

    float petalMix = smoothstep(0.15, 0.35, dist);
    float4 burstColor = lerp(greenGrowth, pinkPetal, petalMix);

    float alpha = (petalShape * 0.5 + bloomMask * 0.4 + ring * 0.6) * radialFade;

    float4 result = base;
    result.rgb += burstColor.rgb * alpha;
    result.rgb += whiteHighlight.rgb * ring * radialFade * 0.8;

    return result;
}

technique Technique1
{
    pass GrowthPulse
    {
        PixelShader = compile ps_3_0 PS_GrowthPulse();
    }
}
