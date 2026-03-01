// ══════════════════════════════════════════════════════════╁E
// OdeGardenAura.fx  EOde to Joy boss ambient garden aura
// Blooming garden ambience with warm gold/amber light and
// petal-shaped noise patterns radiating outward.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Warm gold
float4 uSecondaryColor;  // Radiant amber
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

float4 PS_GardenAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Breathing garden pulse
    float pulse = sin(uTime * 2.0) * 0.06 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Petal shape mask  E8 petals slowly rotating
    float petalAngle = angle + uTime * 0.3;
    float petalShape = abs(sin(petalAngle * 4.0));
    petalShape = smoothstep(0.3, 0.8, petalShape);

    // Garden bloom noise  Eorganic swirling
    float n1 = noise(float2(angle * 3.0 + uTime * 0.5, dist * 8.0));
    float n2 = noise(float2(dist * 12.0 - uTime * 0.3, angle * 2.0 + uTime * 0.7));
    float bloom = n1 * 0.6 + n2 * 0.4;

    // Light motes  Epollen/firefly sparkle
    float moteSeed = hash(floor(uv * 35.0 + float2(0, uTime * 0.5)));
    float motes = step(0.94, moteSeed) * sin(uTime * 5.0 + moteSeed * 30.0) * 0.5 + 0.5;
    motes *= step(0.94, moteSeed);

    // Color: warm gold core to amber edges with green-gold highlights
    float colorMix = bloom * petalShape;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    float4 lightMote = float4(1.0, 0.95, 0.7, 1.0);

    float alpha = falloff * (bloom * 0.5 + 0.3) * petalShape * uIntensity;
    float4 color = baseColor * alpha;
    color += lightMote * motes * falloff * uIntensity * 0.6;

    return color;
}

technique Technique1
{
    pass GardenAura
    {
        PixelShader = compile ps_3_0 PS_GardenAura();
    }
}
