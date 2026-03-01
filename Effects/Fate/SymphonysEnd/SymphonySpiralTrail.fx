// ═══════════════════════════════════════════════════════════════════
//  SymphonySpiralTrail.fx — Symphony's End spiral blade trail
//  FINAL MOVEMENT HELIX: Enhanced helix distortion with nebula wisps,
//  chromatic color separation along the spiral, musical dynamics as
//  intensity modulation, and trailing afterglow with depth layers.
//  Two techniques: SpiralMain (core trail) and SpiralGlow (outer bloom)
//  Profile: ps_3_0 / vs_2_0
// ═══════════════════════════════════════════════════════════════════

float4x4 uTransformMatrix;
float uTime;
float uOpacity;
float3 uColor;
float3 uSecondaryColor;

sampler uImage0 : register(s0);

struct VSInput
{
    float2 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutput
{
    float4 Position : SV_POSITION;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ─── Shared Vertex Shader ─────────────────────────────────────────

VSOutput MainVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(float4(input.Position, 0.0, 1.0), uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color    = input.Color;
    return output;
}

// ─── SpiralMain: Enhanced helix with nebula wisps and chromatic depth ──

float4 SpiralMainPS(VSOutput input) : COLOR0
{
    float2 uv   = input.TexCoord;
    float along  = uv.x;   // 0 = head, 1 = tail
    float across = uv.y;   // 0..1 cross-section

    // --- Multi-frequency helix: primary + harmonic overtone ---
    float helixFreq1 = 12.566;  // ~2π * 2
    float helixFreq2 = 25.132;  // ~2π * 4 (second harmonic)
    float helixSpeed1 = uTime * 3.0;
    float helixSpeed2 = uTime * 5.0;

    float spiral1 = sin(along * helixFreq1 + helixSpeed1) * 0.28;
    float spiral2 = sin(along * helixFreq2 + helixSpeed2) * 0.08; // subtle overtone
    float spiral = spiral1 + spiral2;

    // Helix tightens toward the tail (musical decrescendo)
    float tighten = 1.0 - along * 0.6;
    float centerDist = abs(across - 0.5 + spiral * tighten);

    // --- Nebula wisp layer: domain-warped noise flowing through the helix ---
    float2 wispUV = float2(along * 3.0 - uTime * 0.5, across * 2.0);
    float2 warp = float2(
        SmoothNoise(wispUV + float2(0, uTime * 0.2)),
        SmoothNoise(wispUV + float2(3.7, uTime * 0.15))
    );
    float wisp = SmoothNoise((wispUV + warp * 0.5) * 2.0);
    float wispMask = smoothstep(0.5, 0.0, centerDist) * (1.0 - along * 0.5);
    float wispIntensity = smoothstep(0.35, 0.65, wisp) * wispMask;

    // --- Soft edge falloff with smoother helix following ---
    float edge = 1.0 - smoothstep(0.0, 0.42, centerDist);

    // --- Musical dynamics: intensity modulation like cresc/decresc ---
    // Creates rhythmic brightness pulses along the trail
    float dynamics = sin(along * 18.0 - uTime * 7.0) * 0.5 + 0.5;
    dynamics = dynamics * dynamics; // sharper peaks = staccato brightness
    float dynamicBoost = 0.7 + dynamics * 0.3;

    // --- Chromatic color separation along the helix path ---
    // Slight hue shift between leading and trailing edges of the spiral
    float spiralPhase = frac(along * 2.0 + uTime * 0.3);
    float3 warmShift = uColor * 1.2;                           // hot side
    float3 coolShift = uSecondaryColor;                         // cool side
    float spiralSide = smoothstep(0.4, 0.6, frac(spiral * 2.0 + 0.5));
    float3 chromColor = lerp(warmShift, coolShift, spiralSide);

    // --- Intensity ramps toward the head ---
    float intensity = (1.0 - along) * edge * dynamicBoost;

    // --- White-hot core near center ---
    float coreFactor = smoothstep(0.25, 0.0, centerDist) * (1.0 - along);

    // --- Trailing star sparkles embedded in the spiral ---
    float2 starUV = uv * float2(40.0, 12.0) + uTime * 0.3;
    float starHash = HashNoise(starUV);
    float stars = step(0.95, starHash) * edge * along; // more stars toward tail
    float starTwinkle = sin(uTime * 6.0 + starHash * 25.0) * 0.3 + 0.7;
    stars *= starTwinkle;

    // --- Color composition ---
    float4 baseColor = input.Color;
    float3 coreWhite = float3(1.0, 0.97, 0.98);

    float3 color = baseColor.rgb;
    color = lerp(color, chromColor, edge * 0.6);
    color = lerp(color, coreWhite, coreFactor * 0.8);
    color += uSecondaryColor * wispIntensity * 0.5; // nebula wisps
    color += coreWhite * stars * 1.5;               // embedded sparkles

    float alpha = intensity * uOpacity;
    alpha += wispIntensity * 0.15 * uOpacity;
    alpha += stars * 0.1 * uOpacity;

    return float4(color * alpha, alpha);
}

// ─── SpiralGlow: Enhanced bloom with nebula depth ──────────────────

float4 SpiralGlowPS(VSOutput input) : COLOR0
{
    float2 uv   = input.TexCoord;
    float along  = uv.x;
    float across = abs(uv.y - 0.5);

    // Wide gaussian glow
    float glow = exp(-across * across * 6.0) * (1.0 - along * 0.75);

    // Subtle helix echo in the glow
    float helixEcho = sin(along * 12.566 + uTime * 3.0) * 0.15;
    float echoDist = abs(uv.y - 0.5 + helixEcho * (1.0 - along));
    float echoGlow = exp(-echoDist * echoDist * 10.0) * 0.3;

    // Nebula cloud in glow layer
    float2 cloudUV = uv * float2(3.0, 2.0) - uTime * 0.2;
    float cloud = SmoothNoise(cloudUV * 2.5);
    cloud = smoothstep(0.3, 0.6, cloud) * glow * 0.2;

    // Musical pulse: slow breathing
    float pulse = 0.8 + 0.2 * sin(uTime * 4.0 + along * 6.283);

    float4 color = input.Color;
    float3 glowMix = lerp(color.rgb, uSecondaryColor, 0.3);
    glowMix += uColor * echoGlow;
    glowMix += uSecondaryColor * cloud;

    float totalGlow = (glow + echoGlow) * pulse;
    float alpha = totalGlow * uOpacity * 0.55;

    return float4(glowMix * alpha, alpha);
}

// ─── Techniques ───────────────────────────────────────────────────

technique SpiralMain
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_3_0 SpiralMainPS();
    }
}

technique SpiralGlow
{
    pass Pass0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader  = compile ps_3_0 SpiralGlowPS();
    }
}
