// ══════════════════════════════════════════════════════════╁E
// AutunnoWitheringWind.fx  ESeasons/Autunno attack flash
// Withering wind gust visual  Eradial gust pattern with
// warm autumn orange/brown/gold swirling outward from center.
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

float4 PS_WitheringWind(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);
    float2 delta = uv - uCenter;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Radial gust streaks  Ewind rushing outward
    float gustAngle = angle + sin(dist * 12.0 - uTime * 6.0) * 0.4;
    float gusts = abs(sin(gustAngle * 5.0 + uTime * 4.0));
    gusts = pow(gusts, 3.0);

    // Radial falloff  Estrongest near center, fading outward
    float radialFade = exp(-dist * 4.0) * uIntensity;

    // Swirling leaf debris noise
    float debris = noise(float2(angle * 4.0 + uTime * 3.0, dist * 8.0 - uTime * 5.0));
    float debrisMask = smoothstep(0.55, 0.8, debris);

    // Wind spiral distortion
    float spiral = sin(angle * 3.0 - dist * 15.0 + uTime * 7.0);
    spiral = max(0.0, spiral) * radialFade;

    // Autumn warmth colors: orange core, brown edges, gold highlights
    float4 orangeGust = uColor;
    float4 brownEdge = float4(0.45, 0.25, 0.08, 1.0);
    float4 goldHighlight = float4(1.0, 0.85, 0.3, 1.0);

    float4 gustColor = lerp(orangeGust, brownEdge, dist * 2.0);
    float alpha = (gusts * 0.6 + spiral * 0.3 + debrisMask * 0.4) * radialFade;

    float4 result = base;
    result.rgb += gustColor.rgb * alpha;
    result.rgb += goldHighlight.rgb * debrisMask * radialFade * 0.5;

    return result;
}

technique Technique1
{
    pass WitheringWind
    {
        PixelShader = compile ps_3_0 PS_WitheringWind();
    }
}
