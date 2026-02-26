// =============================================================================
// Tracer Trail Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Heat-reactive bullet tracer trail for Blossom of the Sakura assault rifle.
// Thin, laser-sharp trail that shifts from cool sakura pink (low heat) to
// white-hot gold (high heat), with noise-driven ember sparks at high heat.
//
// UV Layout:
//   U (coords.x) = along trail (0 = head, 1 = tail)
//   V (coords.y) = across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   TracerTrailMain  - Sharp tracer trail with heat-reactive colour shift
//   TracerTrailGlow  - Soft bloom overlay for additive glow stacking
//
// Features:
//   - Razor-thin core with rapid falloff
//   - Heat-reactive colour: pink ↁEscarlet ↁEwhite-hot gold
//   - Ember sparks via procedural noise at high heat
//   - Fast scroll speed for bullet velocity feel
//   - Overbright multiplier for HDR bloom punch
// =============================================================================

sampler2D uImage0 : register(s0); // Base trail texture
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Sakura pink at low heat)
float3 uSecondaryColor;  // Secondary color (Gold at high heat)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uHeatLevel;         // 0 = cool, 1 = overheated
float uScrollSpeed;       // Trail flow speed
float uDistortionAmt;     // Ember turbulence
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

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: TRACER TRAIL MAIN
// =============================================================================

float4 TracerTrailMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Razor-thin core profile ---
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 3.0); // Very sharp center peak

    // --- Trail length fade ---
    float trailFade = pow(1.0 - coords.x, 2.0);

    // --- Heat-reactive minor distortion ---
    float heatDistort = sin(coords.x * 20.0 + uTime * uScrollSpeed * 8.0) * uDistortionAmt * uHeatLevel;
    float2 distortedUV = coords;
    distortedUV.y += heatDistort;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Heat-reactive colour shift ---
    // Low heat: primary (sakura pink), High heat: secondary (gold) ↁEwhite-hot
    float3 tracerColor = lerp(uColor, uSecondaryColor, uHeatLevel);
    float3 hotWhite = float3(1.0, 0.96, 0.88);
    tracerColor = lerp(tracerColor, hotWhite, uHeatLevel * uHeatLevel * 0.5);

    // White-hot core
    float coreMask = pow(edgeFade, 4.0) * trailFade;
    tracerColor = lerp(tracerColor, hotWhite, coreMask * 0.6);

    // --- Ember sparks at high heat ---
    float2 sparkUV = float2(coords.x * 8.0 - uTime * uScrollSpeed * 3.0, coords.y * 4.0);
    float sparkNoise = HashNoise(sparkUV * uNoiseScale);
    float sparks = saturate(sparkNoise * 3.0 - 2.0) * uHeatLevel * uHeatLevel;
    // Optional noise texture
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.x -= uTime * uScrollSpeed;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(sparkNoise, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Rapid flicker at high heat ---
    float flicker = 1.0;
    flicker = sin(uTime * 15.0 + coords.x * 12.0) * 0.04 * uHeatLevel + 1.0;

    // --- Final composition ---
    float3 finalColor = tracerColor * baseTex.rgb * uIntensity * flicker;
    finalColor += uSecondaryColor * sparks * 0.3;
    finalColor *= 0.7 + noiseVal * 0.3;

    float alpha = (coreFade * trailFade + sparks * 0.15) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: TRACER TRAIL GLOW
// =============================================================================

float4 TracerTrailGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Wider, softer edge profile
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.5); // Extra wide

    // Trail fade
    float trailFade = pow(1.0 - coords.x, 1.5);

    // Heat-reactive glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, uHeatLevel * 0.7);
    float3 warmTint = float3(1.0, 0.7, 0.4);
    glowColor = lerp(glowColor, warmTint, uHeatLevel * 0.2);

    glowColor *= uIntensity * baseTex.rgb * 0.7;

    float pulse = sin(uTime * 4.0 + coords.x * 6.0) * 0.08 + 0.92;

    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse * 0.4;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique TracerTrailMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 TracerTrailMainPS();
    }
}

technique TracerTrailGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 TracerTrailGlowPS();
    }
}
