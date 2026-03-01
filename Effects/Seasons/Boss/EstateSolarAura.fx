// ══════════════════════════════════════════════════════════╁E
// EstateSolarAura.fx  ESeasons/Estate boss ambient aura
// Summer solar aura with heat haze shimmer, blazing gold/
// orange/red sun energy radiating outward in waves.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Blazing gold
float4 uSecondaryColor;  // Solar orange-red
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

float4 PS_SolarAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Solar pulse  Eintense, rhythmic
    float pulse = sin(uTime * 3.0) * 0.1 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Solar flare rays  Esharp radial spikes
    float rays = abs(sin(angle * 6.0 + uTime * 1.5));
    rays = pow(rays, 5.0) * smoothstep(radiusNorm, 0.0, dist);

    // Heat haze shimmer  Ehigh-frequency distortion noise
    float haze1 = noise(float2(uv.x * 20.0 + uTime * 2.0, uv.y * 20.0 + uTime * 3.0));
    float haze2 = noise(float2(uv.x * 30.0 - uTime * 1.5, uv.y * 30.0));
    float hazeShimmer = (haze1 + haze2) * 0.5;
    hazeShimmer = smoothstep(0.3, 0.7, hazeShimmer) * falloff * 0.4;

    // Solar corona ring
    float coronaRing = smoothstep(0.04, 0.0, abs(dist - radiusNorm * 0.7));
    coronaRing *= noise(float2(angle * 5.0, uTime * 2.0));

    // Color: gold core, orange mid, red outer
    float heatGrad = dist / radiusNorm;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, heatGrad);
    float4 whiteHot = float4(1.0, 0.97, 0.85, 1.0);

    float alpha = falloff * (0.3 + hazeShimmer) * uIntensity;
    float4 color = baseColor * alpha;
    color += uPrimaryColor * rays * uIntensity * 0.5;
    color += whiteHot * coronaRing * uIntensity;

    return color;
}

technique Technique1
{
    pass SolarAura
    {
        PixelShader = compile ps_3_0 PS_SolarAura();
    }
}
