// =============================================================================
// Eroica Funeral Trail Shader - Somber Requiem Smoke
// =============================================================================
// Multi-octave layered smoke trail with ash ember dissolution and incense
// wisp depth. Heavier, slower, more mournful than the heroic variant.
//
// VISUAL IDENTITY: Like incense burning at a warrior's funeral pyre --
// thick layers of smoke that curl and separate into distinct depth planes,
// glowing embers that cool to gray ash as they drift, each wisp rising
// like a prayer being carried heavenward.
//
// Techniques:
//   FuneralFlameFlow  - Multi-octave smoke with ash dissolution
//   FuneralGlowPass   - Deep mournful radial glow with heartbeat pulse
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1); // Noise texture

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (DirgeRed)
float3 uSecondaryColor;  // Secondary color (RequiemGold)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uNoiseScale;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;
float uSmokeIntensity;

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

// Multi-octave FBM (Fractal Brownian Motion) for rich smoke layering
float FBMSmoke(float2 uv, float time)
{
    float value = 0.0;
    float amplitude = 0.5;
    float2 offset = float2(time * 0.15, -time * 0.08);

    // Octave 1: Large billowy shapes
    value += SmoothHash(uv * 1.0 + offset) * amplitude;
    amplitude *= 0.5;
    // Octave 2: Medium curls
    value += SmoothHash(uv * 2.2 + offset * 1.3) * amplitude;
    amplitude *= 0.5;
    // Octave 3: Fine wispy detail
    value += SmoothHash(uv * 4.7 + offset * 1.8) * amplitude;
    amplitude *= 0.5;
    // Octave 4: Micro turbulence
    value += SmoothHash(uv * 9.5 + offset * 2.5) * amplitude;

    return value;
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
// TECHNIQUE 1: FUNERAL FLAME FLOW - Multi-Octave Smoke
// =============================================================================

float4 FuneralFlameFlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Multi-layered smoke distortion (3 depth planes) ---
    // Background smoke (slow, large billows)
    float bgDrift = sin(coords.x * 3.0 + uTime * uScrollSpeed * 0.8) * uDistortionAmt * 0.8;
    // Mid smoke (medium movement)
    float midDrift = sin(coords.x * 7.0 - uTime * uScrollSpeed * 1.8 + coords.y * 2.0) * uDistortionAmt * 0.5;
    // Foreground wisps (fast, thin)
    float fgDrift = sin(coords.x * 13.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt * 0.25;

    float2 distortedUV = coords;
    distortedUV.y += bgDrift + fgDrift;
    distortedUV.x += midDrift;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Multi-octave FBM smoke noise ---
    float2 smokeUV = coords * uNoiseScale;
    float fbmNoise = FBMSmoke(smokeUV, uTime * uScrollSpeed);

    // Blend with optional secondary texture
    float2 secUV = coords * uSecondaryTexScale;
    secUV.x -= uTime * uSecondaryTexScroll * 0.8;
    secUV.y -= uTime * 0.2;
    float4 noiseTex = tex2D(uImage1, secUV);
    float noiseVal = lerp(fbmNoise, noiseTex.r, uHasSecondaryTex * 0.55);

    // --- Edge-to-centre fade ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Smoky edge dissolution with FBM detail ---
    float smokeEdgeNoise = lerp(fbmNoise, noiseTex.g, uHasSecondaryTex * uSmokeIntensity * 0.4);
    float smokeEdge = saturate(edgeFade * 1.3 - (1.0 - smokeEdgeNoise) * uSmokeIntensity * 0.7);

    // --- Incense wisp depth layers (3 distinct wisp streams) ---
    // Each stream has different frequency, speed, and vertical position
    float wispMask1 = saturate(1.0 - coords.y * 2.2);
    float wispPattern1 = sin(coords.x * 18.0 + uTime * uScrollSpeed * 5.0) * 0.5 + 0.5;
    wispPattern1 *= SmoothHash(coords * 4.0 + float2(uTime * 0.3, 0.0));

    float wispMask2 = saturate(1.0 - abs(coords.y - 0.2) * 5.0);
    float wispPattern2 = sin(coords.x * 12.0 - uTime * uScrollSpeed * 3.5 + 1.0) * 0.5 + 0.5;
    wispPattern2 *= SmoothHash(coords * 6.0 + float2(-uTime * 0.2, uTime * 0.15));

    float wispMask3 = saturate(1.0 - abs(coords.y - 0.35) * 6.0);
    float wispPattern3 = cos(coords.x * 22.0 + uTime * uScrollSpeed * 4.0) * 0.5 + 0.5;

    float wisps = (wispMask1 * wispPattern1 + wispMask2 * wispPattern2 + wispMask3 * wispPattern3) * uSmokeIntensity * 0.2;

    // --- Ash ember dissolution at trail tail ---
    float trailFade = saturate(1.0 - coords.x * 1.15);
    float ashProgress = saturate((coords.x - 0.5) * 2.0); // 0 at midpoint, 1 at tail

    // Ash particles: noise-driven bright spots that dim to gray
    float ashNoise = SmoothHash(coords * 8.0 + float2(uTime * 0.1, 0.0));
    float ashSparks = saturate(ashNoise * 3.0 - 2.0) * ashProgress;
    float ashDim = 1.0 - ashProgress * 0.6; // Dimming factor

    // --- Funeral colour gradient with ash transition ---
    float gradientT = coords.x * 0.65 + noiseVal * 0.35;
    float3 flameColor = lerp(uColor, uSecondaryColor, gradientT);

    // Ash gray at tail
    float3 ashGray = float3(0.35, 0.30, 0.28);
    float3 coolingEmber = float3(0.5, 0.25, 0.1); // Orange-gray cooling ember
    float ashBlend = saturate((coords.x - 0.5) * 2.5);
    flameColor = lerp(flameColor, coolingEmber, ashBlend * 0.4);
    flameColor = lerp(flameColor, ashGray, ashBlend * ashBlend * 0.5);

    // --- Dimmer white-hot core ---
    float coreMask = saturate((edgeFade - 0.55) * 2.5);
    float3 hotCore = float3(0.92, 0.85, 0.70);
    flameColor = lerp(flameColor, hotCore, coreMask * 0.45 * ashDim);

    // --- Smoke-darkened edges ---
    float edgeMask = saturate((0.55 - edgeFade) * 3.0);
    flameColor *= 1.0 - edgeMask * uSmokeIntensity * 0.35;

    // --- Ash spark highlights ---
    float3 emberGlow = float3(0.8, 0.4, 0.1);
    flameColor += emberGlow * ashSparks * 0.3;

    // --- Slow heartbeat pulse ---
    float pulse = sin(uTime * 4.0 + coords.x * 5.0) * 0.04 + 0.96;
    pulse *= sin(uTime * 2.5 + coords.x * 8.0) * 0.03 + 0.97;

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.50 + noiseVal * 0.50;

    float alpha = (smokeEdge * trailFade + wisps + ashSparks * 0.1) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: FUNERAL GLOW PASS - Deep Mournful Halo
// =============================================================================

float4 FuneralGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float wave = sin(coords.x * 3.5 + uTime * uScrollSpeed * 1.5) * uDistortionAmt * 0.35;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;

    float radial = saturate(1.0 - dist * dist);
    radial *= radial;

    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;

    // FBM smoke dissolution on glow edges
    float2 smokeP = coords * uNoiseScale * 0.5;
    smokeP.x -= uTime * uScrollSpeed * 0.3;
    smokeP.y -= uTime * 0.1;
    float smokeNoise = FBMSmoke(smokeP, uTime * uScrollSpeed * 0.5);
    float smokeDissolve = lerp(1.0, smokeNoise * 0.7 + 0.3, uSmokeIntensity);
    softEdge *= smokeDissolve;

    float trailFade = saturate(1.0 - coords.x * 0.9);

    // Mournful gradient with amber undertone
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.45);
    float3 amberTint = float3(0.85, 0.45, 0.15);
    glowColor = lerp(glowColor, amberTint, 0.12);

    float noiseVal = SmoothHash(coords * uNoiseScale * 0.55 - float2(uTime * 0.3, 0.0));
    noiseVal = lerp(0.75, noiseVal, 0.45);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;

    // Heartbeat pulse
    float heartbeat = sin(uTime * 2.0 + coords.x * 3.0) * 0.12 + 0.88;

    float shape = max(radial, softEdge * 0.6);
    float alpha = shape * trailFade * uOpacity * sampleColor.a * baseTex.a * heartbeat;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique FuneralFlameFlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 FuneralFlameFlowPS();
    }
}

technique FuneralGlowPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 FuneralGlowPS();
    }
}
