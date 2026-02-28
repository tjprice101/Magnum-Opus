// ============================================================================
// TacetParadoxExplosion.fx — TacetsEnigma paradox explosion
// UNIQUE SIGNATURE: Multi-ring moiré cascade — 3 rings expand at different
// rates, and where they overlap creates bright constructive interference
// patterns. The visual is of overlapping wave fronts creating complex
// moiré geometry. Distinctly different from any single-ring explosion.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float4 PS_ParadoxBlast(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Three expanding rings at different speeds
    float ring1Radius = lerp(0.0, 0.55, uTime);
    float ring2Radius = lerp(0.0, 0.45, uTime * 1.3); // faster
    float ring3Radius = lerp(0.0, 0.50, uTime * 0.8); // slower

    float ringWidth = 0.12 * (1.0 - uTime * 0.4);

    // Each ring as a gaussian-ish band
    float r1 = exp(-pow((dist - ring1Radius) / ringWidth, 2.0));
    float r2 = exp(-pow((dist - ring2Radius) / (ringWidth * 0.8), 2.0));
    float r3 = exp(-pow((dist - ring3Radius) / (ringWidth * 1.2), 2.0));

    // Constructive interference — where rings overlap, brightness multiplies
    float interference = r1 * r2 + r1 * r3 + r2 * r3;
    interference = saturate(interference * 3.0);

    // Individual ring visibility
    float rings = saturate(r1 + r2 * 0.8 + r3 * 0.6);

    // Moiré angular pattern — rings interact differently at each angle
    float moireAngle = sin(angle * 8.0 + uTime * 3.0) * 0.5 + 0.5;
    float moire = interference * moireAngle * 0.5 + interference * 0.5;

    // Noise for organic feel
    float2 noiseUV = float2(angle * 0.4 + uTime * 1.5, dist * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Inner fill — bright flash fading
    float innerFill = smoothstep(ring1Radius * 0.5, 0.0, dist) * (1.0 - uTime) * 0.6;

    // Color: base rings in purple, interference peaks in bright green
    float3 ringColor = uColor * rings;
    float3 interferenceColor = uSecondaryColor * 2.0 * moire;
    // Bright nodes at peak interference
    float3 nodeColor = float3(1, 1, 1) * 0.5 * smoothstep(0.5, 0.8, interference);

    float3 blastColor = ringColor + interferenceColor + nodeColor;
    blastColor += lerp(uColor, uSecondaryColor, noise) * innerFill;

    // Flash at explosion start
    float flash = exp(-uTime * 6.0) * uIntensity;
    blastColor += float3(1, 1, 1) * flash * 0.4;

    float totalAlpha = saturate(rings * 0.6 + moire * 0.4 + innerFill) * uOpacity * color.a;
    totalAlpha *= (1.0 - uTime * 0.3);

    return float4(saturate(blastColor), totalAlpha);
}

float4 PS_ParadoxRing(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Outer sharp ring — composite of the 3 rings' leading edges
    float r1 = lerp(0.05, 0.52, uTime);
    float r2 = lerp(0.05, 0.42, uTime * 1.3);
    float r3 = lerp(0.05, 0.47, uTime * 0.8);

    float thickness = 0.015 + 0.01 * (1.0 - uTime);
    float edge1 = smoothstep(thickness, 0.0, abs(dist - r1));
    float edge2 = smoothstep(thickness, 0.0, abs(dist - r2));
    float edge3 = smoothstep(thickness, 0.0, abs(dist - r3));

    float edges = saturate(edge1 + edge2 * 0.8 + edge3 * 0.6);

    // Angular noise for jagged edges
    float2 noiseUV = float2(angle * 0.8 + uTime * 2.0, dist * 4.0);
    float noise = tex2D(uImage1, noiseUV).r;
    edges *= smoothstep(0.15, 0.5, noise);

    // Intersection brightness — where ring edges cross
    float intersection = edge1 * edge2 + edge1 * edge3 + edge2 * edge3;
    intersection = saturate(intersection * 5.0);

    // Color: rings in green, intersections in bright white
    float3 edgeColor = uSecondaryColor * edges * 1.3;
    edgeColor += float3(1, 1, 1) * intersection * uIntensity;

    float alpha = edges * uOpacity * (1.0 - uTime * 0.4) * color.a;

    return float4(edgeColor, alpha);
}

technique TacetParadoxBlast
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_ParadoxBlast();
    }
}

technique TacetParadoxRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_ParadoxRing();
    }
}
