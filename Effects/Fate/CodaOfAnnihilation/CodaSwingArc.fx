// =============================================================================
// Coda of Annihilation — Swing Arc Shader
// =============================================================================
// REALITY-TEARING VOID RIP: The Coda's swing looks like it's shearing space
// apart. Voronoi cracking at the edges, chromatic bleed, a hard void center
// with crimson energy bleeding through the cracks.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

// --- Voronoi cell distance for cracking patterns ---
float2 VoronoiHash(float2 p)
{
    p = float2(dot(p, float2(127.1, 311.7)), dot(p, float2(269.5, 183.3)));
    return frac(sin(p) * 43758.5453);
}

float VoronoiCrack(float2 uv, float scale)
{
    float2 g = floor(uv * scale);
    float2 f = frac(uv * scale);
    float minDist = 1.0;
    float secondDist = 1.0;
    [unroll] for (int y = -1; y <= 1; y++)
    [unroll] for (int x = -1; x <= 1; x++)
    {
        float2 offset = float2(x, y);
        float2 cellCenter = VoronoiHash(g + offset);
        cellCenter = 0.5 + 0.5 * sin(uTime * 1.5 + 6.2831 * cellCenter);
        float d = length(f - offset - cellCenter);
        if (d < minDist) { secondDist = minDist; minDist = d; }
        else if (d < secondDist) { secondDist = d; }
    }
    return secondDist - minDist; // crack width (thin = near edge)
}

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float4 SwingArcMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float cross = abs(coords.y - 0.5) * 2.0;
    float progress = coords.x;

    // --- Void core: hard-edged central darkness ---
    float coreWidth = 0.25 + sin(uTime * 4.0) * 0.03;
    float voidCore = smoothstep(coreWidth + 0.05, coreWidth, cross);

    // --- Voronoi cracking around the arc edges ---
    float2 crackUV = coords + float2(-uTime * 0.6, 0.0);
    float crack = VoronoiCrack(crackUV, 8.0);
    float crackLine = smoothstep(0.02, 0.0, crack); // thin bright cracks
    float crackGlow = smoothstep(0.15, 0.0, crack);  // wider glow around cracks

    // --- Chromatic edge bleed: split RGB slightly at tear boundaries ---
    float2 chromOffset = float2(0.008, 0.004) * (1.0 - voidCore);
    float rShift = tex2D(uImage0, coords + chromOffset).a;
    float bShift = tex2D(uImage0, coords - chromOffset).a;
    float chromatic = abs(rShift - bShift) * 2.0;

    // --- Annihilation energy bleeding through cracks ---
    float energyPulse = sin(progress * 20.0 - uTime * 12.0) * 0.5 + 0.5;
    energyPulse *= energyPulse;
    float crackEnergy = crackGlow * (0.6 + energyPulse * 0.4);

    // --- Edge dissolution: arc frays at outer boundary ---
    float edgeFade = 1.0 - smoothstep(0.7, 1.0, cross);
    float dissolve = HashNoise(coords * float2(40.0, 12.0) + uTime);
    edgeFade *= smoothstep(0.3 * cross, 0.5 * cross, dissolve);

    // --- Color composition ---
    float3 voidBlack = float3(0.02, 0.005, 0.03);
    float3 crackCrimson = uColor * 1.8;
    float3 tearPink = uSecondaryColor;
    float3 annihilWhite = float3(1.0, 0.92, 0.96);

    float3 color = voidBlack;
    color = lerp(color, tearPink * 0.6, crackGlow * (1.0 - voidCore));
    color = lerp(color, crackCrimson, crackLine * crackEnergy);
    color += annihilWhite * chromatic * 0.5;
    color = lerp(color, annihilWhite, voidCore * 0.15 * energyPulse);

    // --- Length falloff: strongest at leading edge ---
    float lengthFade = 1.0 - progress * progress;

    float alpha = edgeFade * lengthFade * uOpacity * uIntensity * baseTex.a;
    alpha *= saturate(0.3 + crackGlow * 0.5 + voidCore * 0.4);

    return float4(color * alpha * uIntensity, alpha);
}

technique SwingArcMain
{
    pass SwingArcMainPass
    {
        PixelShader = compile ps_3_0 SwingArcMainPS();
    }
}
