// L'Estate - Turbulent Corona Flame
// Multi-layer polar-coordinate FBM fire for the Phase 3 roaring corona
// Flame tongues lick outward from center with violent turbulence

sampler uImage0 : register(s0);
float uTime;
float uIntensity;
float uRadius;
float uTurbulence;
float uFlameSpeed;
float4 uPrimaryColor;
float4 uSecondaryColor;
float4 uTertiaryColor;

float hash12(float2 p)
{
    float3 p3 = frac(float3(p.xyx) * 0.1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return frac((p3.x + p3.y) * p3.z);
}

float noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash12(i);
    float b = hash12(i + float2(1.0, 0.0));
    float c = hash12(i + float2(0.0, 1.0));
    float d = hash12(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float fbm3(float2 p)
{
    float v = 0.0;
    float a = 0.5;
    for (int i = 0; i < 3; i++)
    {
        v += a * noise2D(p);
        p = p * 2.1 + float2(97.0, 131.0);
        a *= 0.5;
    }
    return v;
}

float4 CoronaFlamePS(float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x) / 6.283185;

    // Polar UV with outward radial scroll for flame motion
    float2 polarUV = float2(angle * 4.0, dist * 3.0 - uTime * uFlameSpeed);

    // Multi-octave flame turbulence
    float flames = fbm3(polarUV * uTurbulence);
    float flames2 = fbm3(polarUV * uTurbulence * 1.5 + float2(uTime * 0.3, 0.0));
    float combined = flames * 0.65 + flames2 * 0.35;

    // Radial falloff with noise-driven edge breakup
    float halfRadius = uRadius * 0.5;
    float normalizedDist = dist / halfRadius;
    float falloff = pow(saturate(1.0 - normalizedDist), 1.5);
    float edgeBreak = smoothstep(0.0, 0.2, combined - normalizedDist * 0.5 + 0.3);

    float flameIntensity = combined * falloff * edgeBreak * uIntensity;

    // Color ramp: center = white-hot, mid = gold, edge = orange
    float t = saturate(normalizedDist);
    float4 color;
    float4 innerMix = lerp(uPrimaryColor, uSecondaryColor, smoothstep(0.0, 0.35, t));
    color = lerp(innerMix, uTertiaryColor, smoothstep(0.35, 0.8, t));

    color *= flameIntensity;
    color.a = flameIntensity;

    return color;
}

technique CoronaFlameTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 CoronaFlamePS();
    }
}
