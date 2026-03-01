// =============================================================================
// Opus Ultima — Swing Trail Shader
// =============================================================================
// PRISMATIC COSMIC CONVERGENCE: The Magnum Opus. Every theme's essence distilled
// into one blade. Multi-layered: cosmic nebula clouds, prismatic hue cycling,
// convergence waves where all colors merge into white, and a supernova intensity
// that builds with combo. The grand finale of the entire mod's VFX language.
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: OpusCrimson
float3 uSecondaryColor;  // Secondary: GloryGold
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uPhase;            // Combo intensity (0..1)
float uHasSecondaryTex;
float uSecondaryTexScale;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

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

// --- Domain-warped nebula clouds ---
float NebulaClouds(float2 uv)
{
    float2 warp = float2(
        SmoothNoise(uv * 2.0 + float2(0, uTime * 0.2)),
        SmoothNoise(uv * 2.0 + float2(5.2, uTime * 0.15))
    );
    uv += warp * 0.6;

    float val = 0.0;
    float amp = 0.5;
    float freq = 1.0;
    [unroll] for (int i = 0; i < 4; i++)
    {
        val += amp * SmoothNoise(uv * freq + uTime * 0.1 * (i + 1));
        freq *= 2.3;
        amp *= 0.45;
    }
    return val;
}

// --- Prismatic hue cycling: maps a phase angle to a full spectrum ---
float3 PrismaticColor(float phase)
{
    // Smooth spectrum: red → gold → green → cyan → blue → magenta → red
    float3 a = float3(0.5, 0.5, 0.5);
    float3 b = float3(0.5, 0.5, 0.5);
    float3 c_cos = float3(1.0, 1.0, 1.0);
    float3 d = float3(0.0, 0.33, 0.67);
    return a + b * cos(6.28318 * (c_cos * phase + d));
}

// --- Convergence wave: traveling wavefront where all colors merge to white ---
float ConvergenceWave(float2 coords, float time)
{
    // Waves propagate from head to tail
    float wave = sin(coords.x * 15.0 - time * 6.0) * 0.5 + 0.5;
    wave = pow(wave, 12.0); // Very sharp bright peaks
    // Modulated by cross section
    float crossMask = 1.0 - abs(coords.y - 0.5) * 2.0;
    crossMask = crossMask * crossMask;
    return wave * crossMask;
}

// Main swing trail: prismatic cosmic convergence
float4 SwingMainPS(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float combo = saturate(uPhase);

    // --- Nebula cloud layer: flowing cosmic background ---
    float2 nebulaUV = float2(
        progress * uNoiseScale * 0.8 - uTime * uScrollSpeed * 0.3,
        coords.y * 2.0
    );
    float nebula = NebulaClouds(nebulaUV);
    float nebulaMask = saturate(1.0 - cross * 0.8);
    nebulaMask = sqrt(nebulaMask);

    // --- Prismatic hue cycling: color shifts based on position + time ---
    float huePhase = progress * 0.8 + uTime * 0.4 + nebula * 0.3;
    float3 prismatic = PrismaticColor(huePhase);
    // Desaturate toward the weapon's identity colors at low combo
    float saturation = 0.3 + combo * 0.7;
    float3 weaponColor = lerp(uColor, uSecondaryColor, progress);
    float3 blendedPrismatic = lerp(weaponColor, prismatic, saturation);

    // --- Convergence wave: white-hot traveling peaks ---
    float convergence = ConvergenceWave(coords, uTime);
    convergence *= (0.5 + combo * 0.5); // Stronger at high combo

    // --- Star field scatter: tiny bright points like a galaxy ---
    float2 starUV = coords * float2(50.0, 15.0) + float2(uTime * 0.5, 0);
    float starHash = HashNoise(starUV);
    float stars = step(0.96, starHash) * nebulaMask;
    float starTwinkle = sin(uTime * 5.0 + starHash * 30.0) * 0.3 + 0.7;
    stars *= starTwinkle;

    // --- Energy core: hot center line with combo-dependent width ---
    float coreWidth = 0.2 + combo * 0.15;
    float core = smoothstep(coreWidth, 0.0, cross);
    core = core * core;

    // --- Leading edge supernova ---
    float leading = saturate(1.0 - progress * 2.0);
    leading = pow(leading, 3.0);

    // --- Chromatic aberration at edges (high combo only) ---
    float aberration = cross * 0.012 * combo;
    float3 chromShift = float3(
        SmoothNoise(coords * 10.0 + float2(aberration, 0) + uTime),
        SmoothNoise(coords * 10.0 + uTime),
        SmoothNoise(coords * 10.0 - float2(aberration, 0) + uTime)
    );
    chromShift = chromShift * 0.15 * combo * (1.0 - core);

    // --- Secondary texture detail ---
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.3, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.2);

    // --- Color composition ---
    float3 voidCol = float3(0.025, 0.01, 0.04);
    float3 whiteHot = float3(1.0, 0.97, 0.95);

    float3 color = lerp(voidCol, blendedPrismatic * 0.6, nebulaMask * nebula);
    color += blendedPrismatic * core * 0.8;
    color = lerp(color, whiteHot, convergence * 0.9);
    color = lerp(color, whiteHot, core * leading * 0.7);
    color += prismatic * stars * 2.0;
    color += chromShift;
    color *= detail;

    float alpha = (nebulaMask * 0.3 + core * 0.35 + convergence * 0.2 + stars * 0.1 + leading * 0.05);
    alpha *= (1.0 - progress * 0.35);
    alpha *= uOpacity * input.Color.a;
    float3 finalColor = color * uIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow: prismatic nebula haze with convergence echoes
float4 SwingGlowPS(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float combo = saturate(uPhase);

    float glow = exp(-cross * cross * 2.0);

    // Prismatic hue in the glow, slower cycling
    float hue = progress * 0.5 + uTime * 0.2;
    float3 prisGlow = PrismaticColor(hue);
    float prisAmount = 0.2 + combo * 0.4;

    // Convergence pulse echo
    float convergePulse = sin(progress * 10.0 - uTime * 4.0) * 0.5 + 0.5;
    convergePulse = pow(convergePulse, 6.0) * 0.3 * combo;

    float3 baseGlow = lerp(float3(0.025, 0.01, 0.04), uColor * 0.35, glow * 0.5);
    float3 glowColor = lerp(baseGlow, prisGlow * 0.5, prisAmount * glow);
    glowColor += float3(1.0, 0.97, 0.95) * convergePulse * glow;

    float pulse = sin(uTime * 2.5 + progress * 6.0) * 0.1 + 0.9;
    float alpha = glow * (1.0 - progress * 0.45) * uOpacity * input.Color.a * pulse * 0.5;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique OpusSwingMain
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique OpusSwingGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
