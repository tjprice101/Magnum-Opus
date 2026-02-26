// =============================================================================
// Valor Aura Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Concentric ember ring aura radiating outward from player during Celestial
// Valor hold phase. Rings expand with combo-phase-driven intensity, with
// rising fire particle impressions and golden crest flashes at ring peaks.
//
// UV Layout:
//   U (coords.x) = horizontal position (0-1), centre = 0.5
//   V (coords.y) = vertical position (0-1), centre = 0.5
//
// Techniques:
//   ValorAuraMain  - Concentric ember rings with fire impressions
//   ValorAuraGlow  - Soft radial bloom overlay for glow stacking
//
// Features:
//   - Procedural concentric rings via polar distance
//   - Ring intensity scales with uPhase (combo phase)
//   - Rising ember impressions baked into ring pattern
//   - FractalGold ↁEScarlet radial gradient
//   - 6-fold angular symmetry on ring brightness
//   - Overbright multiplier for HDR bloom
// =============================================================================

sampler2D uImage0 : register(s0); // Base texture
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Scarlet)
float3 uSecondaryColor;  // Secondary color (Gold)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uPhase;            // Combo phase (0-1)
float uScrollSpeed;       // Ring expansion rate
float uNoiseScale;        // Noise UV repetition
float uHasSecondaryTex;   // 1.0 if noise texture bound

// =============================================================================
// VERTEX SHADER
// =============================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: VALOR AURA MAIN
// =============================================================================

float4 ValorAuraMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Centre-relative coordinates
    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- Concentric rings expanding outward ---
    float ringFreq = 3.0 + uPhase * 4.0; // More rings at higher combo
    float ringScroll = uTime * uScrollSpeed * 0.8;
    float rings = sin((dist * ringFreq - ringScroll) * 3.14159 * 2.0) * 0.5 + 0.5;
    rings = pow(rings, 2.0); // Sharpen ring bands

    // --- 6-fold angular symmetry (heroic crest pattern) ---
    float angularMod = cos(angle * 3.0) * 0.15 + 0.85;
    rings *= angularMod;

    // --- Rising ember impressions ---
    float2 emberUV = float2(dist * 3.0, angle * 0.318 + uTime * 0.3);
    float embers = HashNoise(emberUV * uNoiseScale);
    embers = saturate(embers * 2.0 - 0.8); // Sparse bright sparks
    float emberRise = saturate(1.0 - dist * 1.5) * embers * 0.4;

    // Optional noise texture modulation
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.x += uTime * 0.2;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.75, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Radial falloff ---
    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    // --- Colour gradient: Scarlet core ↁEGold outer rings ---
    float3 auraColor = lerp(uColor, uSecondaryColor, dist * 0.8);

    // Golden crest flash at ring peaks
    float3 crestGold = float3(1.0, 0.92, 0.65);
    auraColor = lerp(auraColor, crestGold, rings * 0.35 * uPhase);

    // White-hot core
    float coreMask = saturate(1.0 - dist * 3.0);
    auraColor = lerp(auraColor, float3(1.0, 0.96, 0.88), coreMask * 0.4 * uPhase);

    // --- Phase-driven intensity ---
    float phaseIntensity = 0.2 + uPhase * 0.8;

    // --- Pulse ---
    float pulse = sin(uTime * 4.0) * 0.06 + 0.94;

    // --- Final composition ---
    float3 finalColor = auraColor * baseTex.rgb * uIntensity * noiseVal * pulse;
    float alpha = (radial * rings + emberRise) * phaseIntensity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: VALOR AURA GLOW
// =============================================================================

float4 ValorAuraGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    // Soft radial falloff
    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    // Warm glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 0.5);
    float3 warmOrange = float3(1.0, 0.65, 0.25);
    glowColor = lerp(glowColor, warmOrange, 0.15);

    glowColor *= uIntensity * baseTex.rgb;

    // Phase intensity
    float phaseIntensity = 0.2 + uPhase * 0.8;

    // Gentle pulse
    float pulse = sin(uTime * 3.0) * 0.08 + 0.92;

    float alpha = radial * phaseIntensity * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique ValorAuraMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ValorAuraMainPS();
    }
}

technique ValorAuraGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ValorAuraGlowPS();
    }
}
