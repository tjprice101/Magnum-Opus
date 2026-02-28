// =============================================================================
// Sakura Bloom Shader - Wind-Blown Petal Animation
// =============================================================================
// Shared sakura bloom effect for Eroica weapons. Procedural 5-petal
// cherry blossom with wind drift, individual petal wilt and scatter,
// and a full lifecycle (bud -> bloom -> scatter).
//
// VISUAL IDENTITY: A living cherry blossom that blooms, sways in wind,
// and scatters its petals. Unlike the SakuraSwingTrail (which embeds
// petal shapes into a trail strip), this shader renders a standalone
// bloom effect: bud -> full bloom -> petals detach and drift away.
// Individual petals have unique wilt angles and wind response.
//
// Techniques:
//   SakuraPetalBloom  - Full lifecycle bloom with wind scatter
//   SakuraGlowPass    - Soft inner radiance
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
float uPhase;         // 0 = bud, 0.5 = full bloom, 1.0 = scatter
float uPetalCount;    // Number of petals (default 5)
float uRotationSpeed;
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

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// SAKURA PETAL SHAPE
// =============================================================================

// Single petal shape: a heart-like curve using polar coordinates
// Returns 0-1 where 1 = inside petal
float PetalShape(float2 uv, float angle, float openness, float wiltAngle)
{
    // Rotate UV to petal's angle
    float ca = cos(angle + wiltAngle);
    float sa = sin(angle + wiltAngle);
    float2 rotUV = float2(uv.x * ca + uv.y * sa, -uv.x * sa + uv.y * ca);

    // Petal extends in +X direction from centre
    float r = length(rotUV);
    float theta = atan2(rotUV.y, rotUV.x);

    // Petal shape: cardioid-ish curve with notch at tip
    float petalR = openness * 0.35 * (1.0 + cos(theta)) * (1.0 - 0.3 * abs(sin(theta * 2.5)));

    // Soft edge
    float shape = saturate(1.0 - (r - petalR * 0.8) / (petalR * 0.25 + 0.001));
    shape *= step(0.0, rotUV.x); // Only forward half

    return shape;
}

// =============================================================================
// TECHNIQUE 1: SAKURA PETAL BLOOM - Living Flower
// =============================================================================

float4 SakuraPetalBloomPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // Centre the UV
    float2 centre = coords - 0.5;

    // Slow rotation
    float rotAngle = uTime * uRotationSpeed;
    float cr = cos(rotAngle);
    float sr = sin(rotAngle);
    float2 rotCentre = float2(centre.x * cr + centre.y * sr, -centre.x * sr + centre.y * cr);

    // Phase-driven lifecycle
    float budPhase = saturate(1.0 - uPhase * 2.0);       // 1 at bud, 0 at bloom
    float bloomPhase = saturate(1.0 - abs(uPhase - 0.5) * 4.0); // Peak at 0.5
    float scatterPhase = saturate(uPhase * 2.0 - 1.0);    // 0 until 0.5, then rises to 1

    // Openness: how far petals extend
    float openness = lerp(0.2, 1.0, saturate(uPhase * 2.0)); // Grows from bud to bloom

    // --- Petal count (default 5 for cherry blossom) ---
    float petalN = max(uPetalCount, 5.0);
    float angleStep = 6.28318 / petalN;

    float totalPetal = 0.0;
    float3 petalColorAcc = float3(0, 0, 0);

    // Wind gust: affects scatter direction and petal wilt
    float windX = sin(uTime * uScrollSpeed * 0.7) * 0.3 + 0.5;
    float windY = cos(uTime * uScrollSpeed * 0.5) * 0.2;

    // --- Accumulate petals ---
    for (float i = 0; i < 8; i++) // Max 8 petals (loops beyond petalN are masked out)
    {
        if (i >= petalN) break;

        float petalAngle = angleStep * i;

        // Per-petal random seed
        float petalSeed = HashNoise(float2(i, i * 3.7));

        // Wilt: each petal wilts at a different rate during scatter
        float wiltAmount = scatterPhase * (0.3 + petalSeed * 0.7);
        float wiltAngle = wiltAmount * (petalSeed - 0.5) * 2.0; // Random wilt direction

        // Scatter drift: petals detach and drift with wind
        float2 scatterOffset = float2(0, 0);
        if (scatterPhase > 0.01)
        {
            float driftT = scatterPhase * (0.5 + petalSeed * 0.5);
            scatterOffset.x = (windX + petalSeed * 0.3 - 0.15) * driftT * 0.3;
            scatterOffset.y = (windY + petalSeed * 0.2 - 0.1) * driftT * 0.3;
            // Small rotation during drift
            scatterOffset.x += sin(driftT * 6.28 + petalSeed * 3.0) * 0.05;
        }

        float2 petalUV = rotCentre - scatterOffset;

        // Petal shape
        float petal = PetalShape(petalUV, petalAngle, openness, wiltAngle);

        // Fade falling petals
        float scatterFade = 1.0 - scatterPhase * petalSeed * 0.6;
        petal *= scatterFade;

        // --- Per-petal colour variation ---
        // Base sakura pink with slight variation per petal
        float3 innerPink = lerp(uColor, uSecondaryColor, 0.2 + petalSeed * 0.15);
        float3 outerPink = lerp(uSecondaryColor, uColor, petalSeed * 0.3);

        // Petal has darker edges, lighter centre
        float r = length(petalUV);
        float edgeDark = saturate(r * 3.0 - 0.3);
        float3 petalCol = lerp(innerPink, outerPink, edgeDark);

        // Slight vein pattern within each petal
        float veinAngle = atan2(petalUV.y - sin(petalAngle) * 0.1,
                                petalUV.x - cos(petalAngle) * 0.1);
        float veins = abs(sin(veinAngle * 8.0 + petalSeed * 6.0)) * 0.15;
        petalCol = lerp(petalCol, innerPink * 1.3, veins * petal);

        totalPetal = max(totalPetal, petal);
        petalColorAcc = lerp(petalColorAcc, petalCol, petal);
    }

    // --- Centre pistil glow ---
    float pistilDist = length(rotCentre);
    float pistil = saturate(1.0 - pistilDist * 8.0);
    pistil *= openness;
    float3 pistilColor = float3(1.0, 0.9, 0.4); // Golden-yellow pistil
    petalColorAcc = lerp(petalColorAcc, pistilColor, pistil * 0.6);
    totalPetal = max(totalPetal, pistil * 0.8);

    // --- Texture sample for modulation ---
    float4 baseTex = tex2D(uImage0, coords);

    // --- Bud glow: tight central sphere when phase < 0.3 ---
    float budGlow = saturate(1.0 - pistilDist * 12.0) * budPhase;
    float3 budColor = lerp(uColor, float3(0.8, 0.3, 0.3), 0.3);
    petalColorAcc = lerp(petalColorAcc, budColor, budGlow);
    totalPetal = max(totalPetal, budGlow);

    // --- Wind shimmer ---
    float shimmer = SmoothHash(coords * 15.0 + float2(uTime * 2.0, 0.0));
    shimmer = shimmer * 0.15 + 0.85;

    // --- Final ---
    float3 finalColor = petalColorAcc * uIntensity * baseTex.rgb * shimmer;

    float alpha = totalPetal * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: SAKURA GLOW PASS - Inner Radiance
// =============================================================================

float4 SakuraGlowPassPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float2 centre = coords - 0.5;
    float dist = length(centre);

    // Soft radial glow
    float glow = saturate(1.0 - dist * 2.5);
    glow = pow(glow, 1.5);

    // Phase: glow strongest at full bloom
    float bloomPhase = saturate(1.0 - abs(uPhase - 0.5) * 4.0);
    glow *= bloomPhase * 0.5 + 0.5;

    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3) * uIntensity * 0.5;
    glowColor *= baseTex.rgb;

    float pulse = sin(uTime * 3.0) * 0.08 + 0.92;

    float alpha = glow * uOpacity * sampleColor.a * baseTex.a * pulse * 0.25;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique SakuraPetalBloom
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 SakuraPetalBloomPS();
    }
}

technique SakuraGlowPass
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 SakuraGlowPassPS();
    }
}
