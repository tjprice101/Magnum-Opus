// =============================================================================
// Fractal of the Stars — Constellation Trail Shader
// =============================================================================
// Constellation-line trail that connects points like star maps.
// Dotted line effect with bright star nodes and thin connecting lines.
// Used for orbit blade trails and constellation-style VFX.
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;           // Primary: ConstellationWhite
float3 uSecondaryColor;  // Secondary: StarfieldBlue
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;

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

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }

float4 ConstellationPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);

    float progress = coords.x;
    float cross = abs(coords.y - 0.5) * 2.0;

    // Thin centre line (constellation connecting line)
    float lineVal = saturate(1.0 - cross / 0.15);
    lineVal = lineVal * lineVal;

    // Dotted pattern along the line
    float dotPattern = sin(progress * 80.0 + uTime * 3.0);
    dotPattern = step(0.3, dotPattern);
    float dottedLine = lineVal * dotPattern;

    // Star nodes: bright points at intervals
    float nodeFreq = 6.0;
    float nodePhase = frac(progress * nodeFreq);
    float nodeProximity = 1.0 - saturate(abs(nodePhase - 0.5) * 4.0);
    nodeProximity = nodeProximity * nodeProximity * nodeProximity;

    // Node glow radius
    float nodeGlow = saturate(1.0 - cross / 0.6) * nodeProximity;

    // Star twinkle on nodes
    float twinkle = sin(uTime * 5.0 + floor(progress * nodeFreq) * 3.7) * 0.3 + 0.7;
    nodeGlow *= twinkle;

    // Cross-shaped star burst at each node
    float crossShape = saturate(1.0 - cross / 0.08) * nodeProximity;
    float horizontalBurst = saturate(1.0 - abs(coords.y - 0.5) / 0.3) * nodeProximity;
    float starBurst = max(crossShape, horizontalBurst * 0.3) * twinkle;

    // Background starfield dust (very faint)
    float dust = SmoothNoise(coords * float2(40.0, 15.0) + uTime * 0.2);
    dust = dust * dust * 0.15 * saturate(1.0 - cross);

    // Color blending
    float3 lineCol = uSecondaryColor;
    float3 nodeCol = uColor;
    float3 burstCol = float3(1.0, 1.0, 0.94);

    float3 color = lineCol * dottedLine;
    color += nodeCol * nodeGlow * 2.0;
    color += burstCol * starBurst * 1.5;
    color += uSecondaryColor * dust;

    float alpha = (dottedLine * 0.3 + nodeGlow * 0.5 + starBurst * 0.15 + dust * 0.05);
    alpha *= (1.0 - progress * 0.3) * uOpacity * sampleColor.a * baseTex.a;

    float3 finalColor = color * uIntensity * baseTex.rgb;

    return ApplyOverbright(finalColor, alpha);
}

technique ConstellationMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 ConstellationPS();
    }
}
