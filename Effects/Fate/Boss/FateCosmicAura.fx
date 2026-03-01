// ══════════════════════════════════════════════════════════╁E
// FateCosmicAura.fx  EFate boss ambient cosmic nebula aura
// Orbiting glyph patterns over swirling dark pink/crimson
// nebula clouds with celestial white star highlights.
// ══════════════════════════════════════════════════════════╁E

sampler uImage0 : register(s0);
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;    // Dark pink
float4 uSecondaryColor;  // Crimson
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

float4 PS_CosmicAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // Pulsing cosmic radius
    float pulse = sin(uTime * 2.0) * 0.08 + 1.0;
    float radiusNorm = uRadius / 200.0 * pulse;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    // Swirling nebula clouds  Etwo octaves rotating opposite directions
    float n1 = noise(float2(angle * 2.0 + uTime * 0.4, dist * 8.0 - uTime * 0.7));
    float n2 = noise(float2(angle * 3.0 - uTime * 0.6, dist * 12.0 + uTime * 0.3));
    float nebula = n1 * 0.6 + n2 * 0.4;

    // Orbiting glyph ring  Eangular hotspots that rotate
    float glyphAngle = fmod(angle + uTime * 1.5 + 3.14159, 6.28318);
    float glyphSlot = fmod(glyphAngle, 6.28318 / 6.0);
    float glyphMask = smoothstep(0.15, 0.0, abs(glyphSlot - 3.14159 / 6.0));
    float glyphRing = smoothstep(0.02, 0.0, abs(dist - radiusNorm * 0.6));
    float glyphs = glyphMask * glyphRing;

    // Star sparkle highlights
    float stars = hash(floor(uv * 40.0 + float2(uTime * 0.2, 0)));
    stars = step(0.97, stars) * sin(uTime * 8.0 + stars * 50.0) * 0.5 + 0.5;

    // Color: dark pink nebula core, crimson edges, celestial white glyphs
    float colorMix = nebula * 0.7 + sin(angle * 2.0 + uTime) * 0.3;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, colorMix);
    float4 white = float4(1.0, 0.97, 1.0, 1.0);

    float alpha = falloff * (nebula * 0.6 + 0.4) * uIntensity;
    float4 color = baseColor * alpha;
    color += white * glyphs * uIntensity * 1.5;
    color += white * stars * falloff * 0.4;

    return color;
}

technique Technique1
{
    pass CosmicAura
    {
        PixelShader = compile ps_3_0 PS_CosmicAura();
    }
}
