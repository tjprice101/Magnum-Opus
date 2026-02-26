// =============================================================================
// Requiem Beam Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Electric tracking beam body for Funeral Prayer magic staff.
// Funeral-fire colored beam with flickering arc distortion (like Tesla
// coil), somber deep scarlet core fading to requiem gold at edges,
// smoke wisps emanating from the beam body.
//
// UV Layout:
//   U (coords.x) = along beam (0 = source, 1 = target)
//   V (coords.y) = across beam width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   RequiemBeamMain  - Electric arc beam body with smoke wisps
//   RequiemBeamGlow  - Soft mournful bloom for beam halo
//
// Features:
//   - Tesla coil arc distortion via high-freq sine modulation
//   - Somber scarlet ↁErequiem gold gradient
//   - Smoke wisp impressions at beam edges
//   - Steady, mournful pulse (not frantic)
//   - Procedural hash noise for arc variation
//   - Overbright multiplier for HDR bloom
// =============================================================================

sampler2D uImage0 : register(s0); // Base beam texture
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (DeepScarlet)
float3 uSecondaryColor;  // Secondary color (RequiemGold)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;       // Arc scroll rate
float uDistortionAmt;     // Arc displacement strength
float uNoiseScale;        // Noise UV repetition
float uArcFrequency;      // Arc oscillation frequency
float uArcAmplitude;       // Arc lateral amplitude
float uHasSecondaryTex;

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

float SmoothHash(float2 uv)
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

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: REQUIEM BEAM MAIN
// =============================================================================

float4 RequiemBeamMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Tesla coil arc distortion ---
    // Primary arc: sweeping lateral displacement
    float arcTime = uTime * uScrollSpeed * 4.0;
    float arc1 = sin(coords.x * uArcFrequency + arcTime) * uArcAmplitude;
    // Secondary arc: higher freq for electrical jitter
    float arc2 = sin(coords.x * uArcFrequency * 2.3 - arcTime * 1.5) * uArcAmplitude * 0.4;
    // Tertiary: rapid micro-jitter
    float arc3 = sin(coords.x * uArcFrequency * 5.0 + arcTime * 3.0) * uArcAmplitude * 0.15;

    float2 distortedUV = coords;
    distortedUV.y += arc1 + arc2 + arc3;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Beam core profile ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Beam length (source bright, target dimmer) ---
    float beamFade = 0.85 + 0.15 * (1.0 - coords.x);

    // --- Smoke wisp impressions at edges ---
    float edgeMask = saturate((0.55 - edgeFade) * 3.0);
    float2 smokeP = float2(coords.x * 3.0 - uTime * uScrollSpeed * 0.5, coords.y * 2.0 - uTime * 0.2);
    float smokeNoise = SmoothHash(smokeP * uNoiseScale);
    float wisps = edgeMask * smokeNoise * 0.35;

    // Optional noise texture
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.x -= uTime * uScrollSpeed * 0.6;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(smokeNoise * 0.5 + 0.35, noiseTex.r, uHasSecondaryTex * 0.6);

    // --- Funeral colour gradient ---
    // Scarlet core ↁEgold edges ↁEash at far end
    float gradientT = coords.x * 0.5 + (1.0 - edgeFade) * 0.3 + noiseVal * 0.2;
    float3 beamColor = lerp(uColor, uSecondaryColor, gradientT);

    // Dimmer white core (somber, not blazing)
    float coreMask = saturate((edgeFade - 0.55) * 2.5);
    float3 dimWhite = float3(0.92, 0.85, 0.72);
    beamColor = lerp(beamColor, dimWhite, coreMask * 0.5);

    // Smoke-darkened edges
    beamColor *= 1.0 - wisps * 0.4;

    // --- Electrical arc bright nodes ---
    float arcNode = saturate(abs(arc1) * 12.0 - 0.3);
    beamColor *= 1.0 + arcNode * 0.2;

    // --- Slow mournful pulse ---
    float pulse = sin(uTime * 3.0 + coords.x * 4.0) * 0.05 + 0.95;
    pulse *= sin(uTime * 2.0 + coords.x * 7.0) * 0.03 + 0.97;

    // --- Final composition ---
    float3 finalColor = beamColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.6 + noiseVal * 0.4;

    float alpha = (edgeFade * beamFade + wisps) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: REQUIEM BEAM GLOW
// =============================================================================

float4 RequiemBeamGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // Gentle arc distortion
    float wave = sin(coords.x * uArcFrequency * 0.5 + uTime * uScrollSpeed * 2.0) * uArcAmplitude * 0.4;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    // Wider, softer edge
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    // Beam fade
    float beamFade = saturate(1.0 - coords.x * 0.4);

    // Mournful glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.4);
    float3 amberTint = float3(0.85, 0.50, 0.20);
    glowColor = lerp(glowColor, amberTint, 0.1);

    glowColor *= uIntensity * baseTex.rgb * 0.7;

    // Heartbeat pulse
    float heartbeat = sin(uTime * 2.0 + coords.x * 3.0) * 0.10 + 0.90;

    float alpha = softEdge * beamFade * uOpacity * sampleColor.a * baseTex.a * heartbeat * 0.35;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique RequiemBeamMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 RequiemBeamMainPS();
    }
}

technique RequiemBeamGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 RequiemBeamGlowPS();
    }
}
