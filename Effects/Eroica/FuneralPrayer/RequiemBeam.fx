// =============================================================================
// Requiem Beam Shader - Tesla Coil Funeral Beam
// =============================================================================
// Beam shader for Funeral Prayer magic staff. A mourning beam that
// crackles with Tesla arc discharges, wreathed in smoke wisps, with
// a deep mournful pulse that makes the beam breathe with sorrow.
//
// VISUAL IDENTITY: Not a clean laser -- this is a Tesla coil discharge
// wrapped in incense smoke. The beam core is irregular, constantly
// shifting with 3-frequency arc distortion. Smoke wisps curl at the
// beam edges like funeral incense. The beam intensity surges and fades
// with a slow, mournful cadence. Small electrical crackle sparks
// branch off at random points along the beam length.
//
// Techniques:
//   RequiemBeamMain  - Tesla arc beam with smoke wisps
//   RequiemBeamGlow  - Mournful haze bloom
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1);

float4x4 uTransformMatrix;
float uTime;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uNoiseScale;
float uPhase;
float uArcFrequency;
float uArcAmplitude;
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
// TESLA ARC: 3-frequency electrical discharge displacement
// =============================================================================

float TeslaArc(float x, float time)
{
    float baseFreq = uArcFrequency;
    float baseAmp = uArcAmplitude;

    // 3 overlapping frequencies for chaotic electrical feel
    float arc1 = sin(x * baseFreq + time * 12.0) * baseAmp;
    float arc2 = sin(x * baseFreq * 2.3 + time * 18.0 + 1.7) * baseAmp * 0.5;
    float arc3 = sin(x * baseFreq * 5.7 + time * 25.0 + 3.1) * baseAmp * 0.2;

    // Occasional sharp snap: fast jitter at irregular intervals
    float snap = HashNoise(float2(floor(x * 3.0 + time * 5.0), floor(time * 8.0)));
    float snapArc = (snap - 0.5) * baseAmp * step(0.9, snap);

    return arc1 + arc2 + arc3 + snapArc;
}

// Smoke wisp: curling shape for funeral incense
float SmokeWisp(float2 uv, float speed, float phase)
{
    float2 wispUV = uv;
    wispUV.x -= uTime * speed;
    wispUV.y += sin(wispUV.x * 3.0 + uTime * 0.8 + phase) * 0.15; // Curl

    float noise = SmoothHash(wispUV * uNoiseScale * 0.5);

    // Wisp shape: narrow band modulated by noise
    float band = saturate(1.0 - abs(uv.y - 0.5 + sin(uv.x * 2.0 + phase) * 0.1) * 5.0);
    return band * noise;
}

// =============================================================================
// TECHNIQUE 1: REQUIEM BEAM MAIN - Tesla Arc + Smoke
// =============================================================================

float4 RequiemBeamMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Tesla arc displacement (3 frequencies) ---
    float arcDisplace = TeslaArc(coords.x, uTime);

    // Displaced beam centreline
    float beamCenter = 0.5 + arcDisplace;
    float beamDist = abs(coords.y - beamCenter);

    // Multi-layer beam brightness
    float beamCore = saturate(1.0 - beamDist * 30.0);  // Ultra-thin bright core
    float beamBody = saturate(1.0 - beamDist * 10.0);   // Medium body
    float beamField = saturate(1.0 - beamDist * 4.0);   // Wide field

    beamCore = pow(beamCore, 0.6);

    // --- Secondary arc: smaller companion arc at offset ---
    float arcDisplace2 = TeslaArc(coords.x + 0.5, uTime + 1.0) * 0.6;
    float beam2Center = 0.5 + arcDisplace2;
    float beam2Dist = abs(coords.y - beam2Center);
    float beam2 = saturate(1.0 - beam2Dist * 15.0) * 0.3;

    // --- Smoke wisps at beam edges ---
    float wisp1 = SmokeWisp(coords + float2(0.0, 0.15), uScrollSpeed * 0.3, 0.0);
    float wisp2 = SmokeWisp(coords - float2(0.0, 0.15), uScrollSpeed * 0.25, 2.0);
    float wisp3 = SmokeWisp(coords + float2(0.0, 0.25), uScrollSpeed * 0.2, 4.5);

    // Wisps are strongest at beam edges
    float edgeMask = saturate((beamField - beamBody) * 3.0);
    float totalWisps = (wisp1 + wisp2 + wisp3) * 0.33 * (edgeMask * 0.7 + 0.3);

    // --- Electrical crackle branches ---
    // Small arcs that branch off the main beam at irregular intervals
    float branchSeed = HashNoise(float2(floor(coords.x * 5.0 + uTime * 3.0), 0.0));
    float branchVisible = step(0.8, branchSeed);

    // Branch angle from main arc
    float branchAngle = (branchSeed - 0.5) * 1.5;
    float branchLen = branchSeed * 0.08;
    float branchDist = abs(coords.y - beamCenter - branchAngle * (coords.x - floor(coords.x * 5.0 + uTime * 3.0) / 5.0));
    float branch = saturate(1.0 - branchDist * 40.0) * branchVisible * beamField;

    // --- Mournful pulse: slow, sorrowful breathing ---
    float mournPulse = sin(uTime * 1.2) * 0.5 + 0.5;
    mournPulse = pow(mournPulse, 1.5); // Asymmetric: slow fade, faster rise
    float mournIntensity = 0.6 + mournPulse * 0.4;

    // --- Base texture ---
    float2 texUV = coords;
    texUV.y += arcDisplace * 0.3;
    texUV.x -= uTime * uScrollSpeed * 0.5;
    float4 baseTex = tex2D(uImage0, texUV);

    // --- Noise texture blend ---
    float noiseVal = SmoothHash(coords * uNoiseScale + float2(-uTime * 0.5, 0.0));
    float4 noiseTex = tex2D(uImage1, coords * uNoiseScale * 0.3 - float2(uTime * 0.3, 0.0));
    noiseVal = lerp(noiseVal, noiseTex.r, uHasSecondaryTex * 0.4);

    // --- Beam length taper ---
    float lengthFade = saturate(1.0 - coords.x * 0.15); // Very gradual

    // --- Colour: Deep crimson core -> dark smoke edges ---
    float3 coreColor = uColor * 2.0; // Overbright crimson
    float3 bodyColor = lerp(uColor, uSecondaryColor, 0.2);
    float3 smokeColor = uSecondaryColor * 0.3; // Dark smoke at edges

    float3 beamColor = coreColor * beamCore
                      + bodyColor * beamBody * 0.5
                      + smokeColor * totalWisps * 0.5;

    // Secondary arc: slightly different hue
    float3 arc2Color = lerp(uColor, float3(0.8, 0.3, 0.3), 0.3);
    beamColor += arc2Color * beam2;

    // Crackle branches: white-hot
    beamColor += float3(1.0, 0.8, 0.7) * branch * 0.5;

    // Noise modulation for organic feel
    beamColor *= (0.7 + noiseVal * 0.3);

    // --- Phase modulation ---
    float phaseBoost = lerp(0.4, 1.2, uPhase);

    // --- Final ---
    float3 finalColor = beamColor * baseTex.rgb * uIntensity * mournIntensity * phaseBoost;

    float alpha = (beamBody * 0.7 + totalWisps * 0.2 + beam2 * 0.1 + branch * 0.1)
                * lengthFade * uOpacity * sampleColor.a * baseTex.a * mournIntensity;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: REQUIEM BEAM GLOW - Mournful Haze
// =============================================================================

float4 RequiemBeamGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Wide soft beam glow
    float beamDist = abs(coords.y - 0.5);
    float wideGlow = saturate(1.0 - beamDist * 2.5);
    wideGlow = pow(wideGlow, 1.2);

    float lengthFade = saturate(1.0 - coords.x * 0.1);

    // Mournful pulse matches main beam
    float mournPulse = sin(uTime * 1.2) * 0.5 + 0.5;
    mournPulse = pow(mournPulse, 1.5);
    float mournIntensity = 0.7 + mournPulse * 0.3;

    // Smoke haze colour: dark, desaturated
    float3 hazeColor = lerp(uColor, uSecondaryColor, 0.5) * 0.3;
    hazeColor *= uIntensity * baseTex.rgb;

    float alpha = wideGlow * lengthFade * uOpacity * sampleColor.a * baseTex.a * mournIntensity * uPhase * 0.2;

    return ApplyOverbright(hazeColor, alpha);
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
