// =============================================================================
// The Conductor's Last Constellation — Swing Trail Shader
// =============================================================================
// BATON LIGHTNING FIELD: Sharp, jagged, branching lightning arcs — not smooth
// noise threads. Electric field lines converge at conductor nodes. Real zigzag
// bolt geometry via step-displaced paths. The conductor's baton commands the
// electricity of the cosmos itself.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;           // Primary: ConductorCyan
float3 uSecondaryColor;  // Secondary: BatonPurple
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uNoiseScale;
float uPhase;            // Combo intensity (0..1)
float uHasSecondaryTex;
float uSecondaryTexScale;

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

// --- Lightning bolt path: sharp zigzag displacement ---
// Returns distance to a jagged bolt running along the trail's length
float LightningBolt(float2 coords, float seed, float segments, float amplitude)
{
    // Divide the trail into segments; at each junction the bolt snaps to a new y
    float segX = coords.x * segments;
    float segID = floor(segX);
    float segFrac = frac(segX);

    // Sharp y-displacement at each segment boundary (zigzag)
    float y0 = (HashNoise(float2(segID, seed)) - 0.5) * amplitude;
    float y1 = (HashNoise(float2(segID + 1.0, seed)) - 0.5) * amplitude;

    // Linear interpolation within segment (straight line between junctions)
    float boltY = lerp(y0, y1, segFrac);

    // Distance from bolt center to current pixel
    float dist = abs(coords.y - 0.5 - boltY);
    return dist;
}

// --- Electric arc node: bright convergence point ---
float ArcNode(float2 coords, float2 nodePos, float radius)
{
    float d = length(coords - nodePos);
    float core = smoothstep(radius, 0.0, d);
    float halo = smoothstep(radius * 3.0, 0.0, d) * 0.3;
    return core + halo;
}

// Main swing trail: branching lightning bolts
float4 SwingMainPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;
    float combo = saturate(uPhase);

    // --- Time-varying seed for bolt animation (bolts reform rapidly) ---
    float boltTime = floor(uTime * 8.0); // Bolts snap to new paths 8x per second
    float boltLerp = frac(uTime * 8.0);  // Smooth transition between paths
    boltLerp = boltLerp * boltLerp * (3.0 - 2.0 * boltLerp); // smoothstep

    // --- Primary lightning bolt (main arc) ---
    float segments = 12.0 + combo * 6.0;
    float amp = 0.15 + combo * 0.05;
    float dist1a = LightningBolt(coords, boltTime, segments, amp);
    float dist1b = LightningBolt(coords, boltTime + 1.0, segments, amp);
    float dist1 = lerp(dist1a, dist1b, boltLerp);
    float bolt1Core = smoothstep(0.015, 0.0, dist1);   // bright thin core
    float bolt1Glow = smoothstep(0.08, 0.0, dist1);    // wider glow
    float bolt1Haze = smoothstep(0.2, 0.0, dist1);     // ambient scatter

    // --- Secondary bolt (branching fork, offset path) ---
    float dist2a = LightningBolt(coords, boltTime + 47.0, segments * 0.7, amp * 0.7);
    float dist2b = LightningBolt(coords, boltTime + 48.0, segments * 0.7, amp * 0.7);
    float dist2 = lerp(dist2a, dist2b, boltLerp);
    float bolt2Core = smoothstep(0.01, 0.0, dist2) * 0.6;
    float bolt2Glow = smoothstep(0.06, 0.0, dist2) * 0.4;

    // Branch forks away from main bolt at ~40% along trail
    float branchMask = smoothstep(0.3, 0.5, progress) * (1.0 - smoothstep(0.8, 1.0, progress));
    bolt2Core *= branchMask;
    bolt2Glow *= branchMask;

    // --- Tertiary micro-bolts (combo-dependent, adds density) ---
    float microBolt = 0.0;
    if (combo > 0.3)
    {
        float dist3 = LightningBolt(coords, boltTime + 100.0, 20.0, 0.08);
        float dist3b = LightningBolt(coords, boltTime + 101.0, 20.0, 0.08);
        float dist3m = lerp(dist3, dist3b, boltLerp);
        microBolt = smoothstep(0.008, 0.0, dist3m) * (combo - 0.3) * 1.5;
    }

    // --- Arc nodes: bright convergence points along the bolt path ---
    float nodes = 0.0;
    [unroll] for (int n = 0; n < 4; n++)
    {
        float nx = (n + 0.5) / 4.0;
        float ny = 0.5 + (HashNoise(float2(n, boltTime)) - 0.5) * amp;
        float nodePulse = sin(uTime * 6.0 + n * 1.57) * 0.3 + 0.7;
        nodes += ArcNode(coords, float2(nx, ny), 0.025) * nodePulse;
    }

    // --- Electric field ambient: low-frequency crackling in the background ---
    float field = SmoothNoise(coords * float2(6.0, 4.0) + uTime * 1.5);
    field = smoothstep(0.5, 0.7, field) * (1.0 - cross) * 0.25;

    // --- Leading edge flash ---
    float leading = saturate(1.0 - progress * 2.5);
    leading = leading * leading * leading;

    // --- Secondary texture ---
    float2 secUV = float2(progress * uSecondaryTexScale - uTime * 0.5, coords.y * 2.0);
    float4 secTex = tex2D(uImage1, secUV);
    float detail = lerp(1.0, secTex.r, uHasSecondaryTex * 0.2);

    // --- Color: void → purple field → cyan bolt → gold node → white flash ---
    float3 voidCol = float3(0.02, 0.015, 0.04);
    float3 purpleField = uSecondaryColor * 0.5;
    float3 cyanBolt = uColor;
    float3 goldNode = float3(1.0, 0.86, 0.31);
    float3 whiteFlash = float3(0.95, 0.97, 1.0);

    float3 color = voidCol;
    color = lerp(color, purpleField, bolt1Haze + field);
    color += cyanBolt * (bolt1Glow + bolt2Glow) * 1.5;
    color += whiteFlash * (bolt1Core + bolt2Core + microBolt) * 2.0;
    color += goldNode * nodes * 2.5;
    color = lerp(color, whiteFlash, leading * 0.4);
    color *= detail;

    // --- Alpha: mostly dark with sharp bright bolts ---
    float body = saturate(1.0 - cross);
    float alpha = (body * 0.15 + bolt1Haze * 0.15 + bolt1Glow * 0.25 + bolt1Core * 0.25 + nodes * 0.15 + field * 0.05);
    alpha *= (1.0 - progress * 0.3);
    alpha *= uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

// Wide glow: electric purple-cyan haze with flickering
float4 SwingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    float glow = exp(-cross * cross * 2.5);

    // Electric flicker: rapid random brightness variation
    float flicker = HashNoise(float2(floor(uTime * 12.0), progress * 4.0));
    flicker = 0.7 + flicker * 0.3;

    // Bolt echo in glow
    float boltEcho = LightningBolt(coords, floor(uTime * 6.0), 8.0, 0.12);
    boltEcho = smoothstep(0.15, 0.0, boltEcho) * 0.3;

    float3 glowColor = lerp(float3(0.02, 0.015, 0.04), uSecondaryColor * 0.35, glow * 0.5);
    glowColor += uColor * boltEcho * glow;
    glowColor += float3(0.1, 0.6, 0.7) * glow * glow * 0.2; // Ambient cyan

    float alpha = glow * (1.0 - progress * 0.45) * uOpacity * sampleColor.a * baseTex.a * flicker * 0.45;

    return ApplyOverbright(glowColor * uIntensity, alpha);
}

technique ConductorSwingMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingMainPS();
    }
}

technique ConductorSwingGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SwingGlowPS();
    }
}
