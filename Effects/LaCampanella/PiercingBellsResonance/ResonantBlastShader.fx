// =============================================================================
// Piercing Bells Resonance  EResonant Blast Shader (Enhanced)
// =============================================================================
// The 20th-shot resonant blast explosion. Multiple concentric bell-shaped
// wavefronts expand with exponentially decaying amplitude  Elike a struck
// bell reverberating. Harmonic interference patterns create brilliant
// nodes where wavefronts overlap. Central detonation core flashes white-hot
// then decays through the fire palette.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;              // Explosion progress 0-1
float uNoiseScale;
float uHasSecondaryTex;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }
float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

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

matrix uWorldViewProjection;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput TrailVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput)0;
    output.Position = mul(input.Position, uWorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}

float4 ResonantBlastPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // ---- Multi-ring resonance wavefronts (struck bell rings) ----
    float expand = saturate(uPhase);
    float ring1R = expand * 0.90;
    float ring2R = expand * 0.60;
    float ring3R = expand * 0.35;

    // Gaussian bell-shaped wavefronts with decaying amplitude
    float ring1 = exp(-(dist - ring1R) * (dist - ring1R) / 0.008);
    float ring2 = exp(-(dist - ring2R) * (dist - ring2R) / 0.005) * 0.6;
    float ring3 = exp(-(dist - ring3R) * (dist - ring3R) / 0.003) * 0.35;

    // ---- Harmonic interference (where rings overlap, brightness peaks) ----
    float interference = ring1 * ring2 + ring2 * ring3 + ring1 * ring3;
    interference = saturate(interference * 3.0);

    // ---- Central detonation core (white-hot flash decaying) ----
    float coreFlash = exp(-dist * dist / (0.01 + expand * 0.04));
    coreFlash *= saturate(1.0 - expand * 1.5);

    // ---- Radial debris ray streaks ----
    float rays = pow(saturate(cos(angle * 10.0) * 0.5 + 0.5), 6.0);
    rays *= exp(-dist * 2.0) * saturate(expand * 3.0);

    // ---- Angular noise for organic wavefront edges ----
    float2 noiseUV = float2(angle * 0.5 + uTime * 0.3, dist * uNoiseScale * 3.0);
    float turb = SmoothNoise(noiseUV * 3.0);
    turb = turb * 0.3 + 0.7;

    // Secondary texture
    float2 secUV = coords * 2.0;
    float4 noiseTex = tex2D(uImage1, secUV);
    float texVal = lerp(1.0, noiseTex.r * 0.5 + 0.6, uHasSecondaryTex * 0.4);

    // ---- 5-stop colour gradient ----
    float3 cDark   = float3(0.08, 0.03, 0.01);
    float3 cEmber  = uColor * 0.5;
    float3 cFlame  = uColor;
    float3 cGold   = uSecondaryColor;
    float3 cWhite  = float3(1.0, 0.96, 0.86);

    float3 color = float3(0, 0, 0);
    // Core flash dominates early
    color += cWhite * coreFlash;
    // Rings contribute fire colours
    color += cGold * ring1 * 0.7;
    color += cFlame * ring2 * 0.6;
    color += cEmber * ring3;
    // Interference nodes flash bright
    color += cWhite * interference * 0.5;
    // Debris rays
    color += float3(1.0, 0.85, 0.45) * rays * 0.5;

    color *= turb * texVal;

    // ---- Energy dissipation ----
    float energyFade = saturate(1.0 - expand * 0.5);
    float bellRing = pow(saturate(cos(uTime * 10.0) * 0.5 + 0.5), 4.0) * 0.08 + 0.92;

    float alpha = (ring1 * 0.35 + ring2 * 0.2 + ring3 * 0.1 + coreFlash * 0.5 + interference * 0.2 + rays * 0.1)
                * energyFade * uOpacity * bellRing * baseTex.a;
    float3 finalColor = color * uIntensity * bellRing * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha * sampleColor.a);
}

technique AutoPass
{
    pass P0
    {
        VertexShader = compile vs_3_0 TrailVS();
        PixelShader = compile ps_3_0 ResonantBlastPS();
    }
}
