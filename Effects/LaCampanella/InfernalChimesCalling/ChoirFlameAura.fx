// =============================================================================
// Infernal Chimes Calling  EChoir Flame Aura (Enhanced)
// =============================================================================
// Ambient aura around the summoned choir minion. Counter-rotating concentric
// harmonic rings pulse outward like resonating sound. Musical-notation-shaped
// angular distortion creates ephemeral glyph impressions. The aura breathes
// with a warm heartbeat and has an ethereal fire mist at its boundary.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4x4 uTransformMatrix;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;
float uNoiseScale;
float uHasSecondaryTex;

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

VSOutput ChoirAuraVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1, 0));
    float c = HashNoise(i + float2(0, 1));
    float d = HashNoise(i + float2(1, 1));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float4 ChoirFlameAuraPS(VSOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // ---- Counter-rotating harmonic rings ----
    float ring1 = sin(dist * 10.0 - uTime * 2.5 + angle * 0.5) * 0.5 + 0.5;
    float ring2 = sin(dist * 14.0 + uTime * 1.8 - angle * 0.3) * 0.5 + 0.5;
    float rings = ring1 * 0.6 + ring2 * 0.4;
    rings = pow(rings, 2.0);

    // ---- Radial gaussian with warm rim ----
    float coreGlow = exp(-dist * dist / 0.15);
    float rimGlow  = exp(-(dist - 0.65) * (dist - 0.65) / 0.03) * 0.4;
    float radial = coreGlow + rimGlow;

    // ---- Musical notation angular ghosts (4-fold + 8-fold) ----
    float glyphs4 = pow(saturate(cos(angle * 4.0 + uTime * 1.5) * 0.5 + 0.5), 6.0);
    float glyphs8 = pow(saturate(cos(angle * 8.0 - uTime * 2.0) * 0.5 + 0.5), 8.0);
    float glyphs = (glyphs4 * 0.6 + glyphs8 * 0.4) * saturate(dist * 2.0 - 0.3) * saturate(1.0 - dist);

    // ---- Ember flicker with SmoothNoise ----
    float2 flickerUV = float2(angle * 0.5 + uTime * 0.4, dist * uNoiseScale * 3.0);
    float flicker = SmoothNoise(flickerUV * 3.0);
    flicker = flicker * 0.35 + 0.65;

    // Secondary texture
    float2 secUV = coords * 2.0 + float2(uTime * 0.1, uTime * 0.08);
    float4 noiseTex = tex2D(uImage1, secUV);
    float texVal = lerp(1.0, noiseTex.r * 0.5 + 0.6, uHasSecondaryTex * 0.4);

    // ---- 5-stop colour gradient ----
    float intensity = radial * flicker;
    float3 cDark   = float3(0.06, 0.02, 0.01);
    float3 cEmber  = uColor * 0.35;
    float3 cWarm   = uColor;
    float3 cGold   = uSecondaryColor;
    float3 cBright = float3(1.0, 0.93, 0.78);

    float3 color = cDark;
    color = lerp(color, cEmber,  smoothstep(0.0,  0.15, intensity));
    color = lerp(color, cWarm,   smoothstep(0.15, 0.35, intensity));
    color = lerp(color, cGold,   smoothstep(0.35, 0.6,  intensity));
    color = lerp(color, cBright, smoothstep(0.6,  0.9,  intensity));

    // Glyphs flash with gold tint
    color += uSecondaryColor * glyphs * 0.5;
    // Ring highlights
    color += uColor * 0.2 * rings * radial;

    color *= texVal;

    // ---- Breathing heartbeat & dynamics ----
    float phaseIntensity = 0.35 + uPhase * 0.65;
    float breathe = sin(uTime * 3.0) * 0.12 + 0.88;
    float outerFade = smoothstep(1.0, 0.7, dist);

    float alpha = (radial * 0.5 + rings * radial * 0.2 + glyphs * 0.2 + flicker * 0.1)
                * outerFade * phaseIntensity * uOpacity * breathe * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * breathe * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique TrailPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 ChoirAuraVS();
        PixelShader = compile ps_3_0 ChoirFlameAuraPS();
    }
}
