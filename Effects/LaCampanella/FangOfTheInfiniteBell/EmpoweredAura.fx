// =============================================================================
// Fang of the Infinite Bell  EEmpowered Aura Shader (Enhanced)
// =============================================================================
// Arcane sigil field around the player during empowerment. Distinct from
// DualFated's fire aura: this is a contained magical resonance field with
// rotating concentric sigil rings, 6-fold crystalline symmetry, orbiting
// glyph bands, and a breathing arcane core. The aura pulses at harmonic
// intervals like a magical heartbeat.
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
float uScrollSpeed;
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

VSOutput AuraVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float SmoothNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
}

float4 EmpoweredAuraPS(VSOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- Concentric sigil rings (counter-rotating pairs) ---
    float ringScroll1 = uTime * uScrollSpeed * 0.4;
    float ringScroll2 = uTime * uScrollSpeed * -0.25;
    float rings1 = sin((dist * 6.0 - ringScroll1) * 6.28318) * 0.5 + 0.5;
    float rings2 = sin((dist * 10.0 - ringScroll2) * 6.28318) * 0.5 + 0.5;
    rings1 = pow(rings1, 3.0);
    rings2 = pow(rings2, 4.0) * 0.5;
    float rings = rings1 + rings2;

    // --- 6-fold crystalline symmetry (arcane sigil geometry) ---
    float hexAngle = angle * 3.0 + uTime * 0.4;
    float hexPattern = cos(hexAngle) * 0.5 + 0.5;
    hexPattern = pow(hexPattern, 1.5);

    // Sigil lines radiating outward at 6 angles
    float sigilAngle = angle * 3.0 + uTime * 0.2;
    float sigilLines = abs(cos(sigilAngle));
    sigilLines = pow(sigilLines, 8.0);  // Very thin radial lines
    sigilLines *= saturate(dist - 0.2) * saturate(1.0 - dist);

    // --- Orbiting glyph band (a ring of arcane symbols at mid-radius) ---
    float glyphRadius = 0.55;
    float glyphBand = exp(-pow((dist - glyphRadius) * 8.0, 2.0));
    float glyphAngle = angle * 6.0 + uTime * 1.5;
    float glyphs = cos(glyphAngle) * 0.5 + 0.5;
    glyphs = pow(glyphs, 3.0) * glyphBand;

    // --- Arcane shimmer (FBM-lite via layered smooth noise) ---
    float2 shimmerUV = float2(dist * 3.0 + uTime * 0.15, angle * 0.5 + uTime * 0.1);
    float shimmer = SmoothNoise(shimmerUV * uNoiseScale * 2.0);
    float shimmer2 = SmoothNoise(shimmerUV * uNoiseScale * 4.0 + float2(3.7, 1.2));
    float arcaneShimmer = saturate(shimmer * 0.6 + shimmer2 * 0.4);

    // Secondary noise texture
    float2 secUV = float2(angle * 0.318 + uTime * 0.08, dist + uTime * 0.05);
    float4 noiseTex = tex2D(uImage1, secUV);
    float texVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Radial falloff: bright core + glowing ring at glyph band ---
    float coreGlow = exp(-dist * dist * 8.0);
    float bandGlow = glyphBand * 0.3;
    float edgeFade = saturate(1.0 - dist * 0.9);
    edgeFade *= edgeFade;
    float radial = coreGlow + bandGlow + edgeFade * 0.3;

    // --- Breathing arcane heartbeat ---
    float breath = sin(uTime * 2.0) * 0.15 + 0.85;
    float microPulse = sin(uTime * 7.0 + dist * 5.0) * 0.05 + 0.95;

    // --- Crystal sparkles in the glyph band ---
    float2 sparkUV = float2(angle * 10.0, dist * 20.0) + uTime * float2(1.0, 0.3);
    float sparkle = HashNoise(sparkUV);
    sparkle = pow(sparkle, 16.0) * glyphBand * 4.0;

    // --- 5-stop arcane color gradient ---
    float3 voidAmber = uColor * 0.2;
    float3 arcaneBody = uColor;
    float3 brightGold = uSecondaryColor;
    float3 arcaneWhite = float3(1.0, 0.95, 0.82);
    float3 sparkColor = float3(1.0, 0.92, 0.7);

    float t = saturate(radial * arcaneShimmer * 1.3);
    float3 auraColor = lerp(voidAmber, arcaneBody, saturate(t * 2.5));
    auraColor = lerp(auraColor, brightGold, (rings + glyphs * 0.5) * 0.4);
    auraColor = lerp(auraColor, arcaneWhite, coreGlow * uPhase * 0.5);

    // Sigil line highlights
    auraColor += brightGold * sigilLines * 0.5 * uPhase;

    // Crystal sparkles
    auraColor += sparkColor * sparkle;

    float phaseIntensity = 0.25 + uPhase * 0.75;

    float3 finalColor = auraColor * uIntensity * texVal * breath * microPulse * baseTex.rgb;
    float alpha = (radial * (rings * 0.4 + hexPattern * 0.2 + glyphs * 0.15) + sigilLines * 0.15 + arcaneShimmer * 0.08 + sparkle * 0.15)
                  * phaseIntensity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, saturate(alpha));
}

technique TrailPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 AuraVS();
        PixelShader = compile ps_3_0 EmpoweredAuraPS();
    }
}
