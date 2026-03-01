// ══════════════════════════════════════════════════════════╁E
// AutunnoDecayAura.fx  ESeasons/Autunno boss ambient aura
// Autumn decay aura with falling leaf silhouettes, warm
// orange-brown-gold decay palette, swirling wind currents.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Warm orange
float4 uSecondaryColor;  // Brown/gold
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

float4 PS_DecayAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Gentle autumn wind pulse
    float pulse = sin(uTime * 1.8) * 0.06 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Swirling wind current  Eleaves carried in circular gusts
    float wind = noise(float2(angle * 3.0 + uTime * 0.8, dist * 6.0 - uTime * 0.5));
    float windSwirl = smoothstep(0.3, 0.7, wind);

    // Falling leaf pattern  Edescending noise cells
    float leafY = uv.y + uTime * 0.4;
    float leafX = uv.x + sin(leafY * 5.0 + uTime) * 0.1;
    float leafNoise = noise(float2(leafX * 10.0, leafY * 8.0));
    float leaves = smoothstep(0.7, 0.85, leafNoise) * falloff;

    // Decay particles  Esubtle dark spots
    float decayNoise = noise(float2(uv.x * 15.0, uv.y * 15.0 + uTime * 0.2));
    float decay = smoothstep(0.8, 0.9, decayNoise) * falloff * 0.3;

    // Color: warm orange core, brown swirl, gold leaf highlights
    float colorMix = windSwirl * 0.6 + leaves * 0.4;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    float4 goldHighlight = float4(1.0, 0.85, 0.3, 1.0);

    float alpha = falloff * (windSwirl * 0.4 + 0.2) * uIntensity;
    float4 color = baseColor * alpha;
    color += goldHighlight * leaves * uIntensity * 0.6;
    color -= float4(0.1, 0.08, 0.05, 0) * decay;

    return color;
}

technique Technique1
{
    pass DecayAura
    {
        PixelShader = compile ps_3_0 PS_DecayAura();
    }
}
