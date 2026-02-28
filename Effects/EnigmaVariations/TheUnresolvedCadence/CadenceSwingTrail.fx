// ============================================================================
// CadenceSwingTrail.fx — TheUnresolvedCadence melee swing trail
// UNIQUE SIGNATURE: Dimensional tear lines — sharp parallel diagonal cracks
// raked through reality. Bright paradox energy bleeds through hard geometric
// gashes. NOT organic noise — structured, aggressive, deliberate cuts.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

struct VertexOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;
};

VertexOutput VS_Main(VertexInput input)
{
    VertexOutput output;
    output.Position = mul(float4(input.Position, 0, 1), uWorldViewProjection);
    output.Color = input.Color;
    output.TexCoord = input.TexCoord;
    return output;
}

// Sharp diagonal crack lines via domain warping
float crackPattern(float2 p, float time)
{
    // Primary diagonal crack direction (steep angle)
    float line1 = abs(frac((p.x * 2.0 + p.y * 5.0) * 0.3 - time * 0.4) - 0.5);
    float crack1 = smoothstep(0.04, 0.0, line1);

    // Secondary perpendicular cracks (shallower angle)
    float line2 = abs(frac((p.x * 4.0 - p.y * 2.0) * 0.25 + time * 0.3) - 0.5);
    float crack2 = smoothstep(0.03, 0.0, line2);

    // Tertiary micro-cracks — finer detail
    float line3 = abs(frac((p.x * 7.0 + p.y * 1.0) * 0.15 - time * 0.5) - 0.5);
    float crack3 = smoothstep(0.02, 0.0, line3) * 0.5;

    return saturate(crack1 + crack2 * 0.7 + crack3);
}

float4 PS_SwingFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Domain warp to distort crack pattern
    float2 noiseUV = float2(coords.x * 2.0 + uTime * 0.2, coords.y * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;
    float2 warped = coords + (noise - 0.5) * 0.08;

    // Generate the crack pattern
    float2 crackUV = float2(warped.x * 8.0, warped.y * 6.0);
    float cracks = crackPattern(crackUV, uTime);

    // Intensity drives crack visibility — more stacks = more dimensional damage
    cracks *= (0.5 + uIntensity * 0.8);

    // Energy bleeding through cracks — bright paradox green
    float3 crackColor = uSecondaryColor * 2.0 * cracks;
    // Crack edges glow white-hot
    float crackEdge = smoothstep(0.3, 0.8, cracks);
    crackColor += float3(0.6, 0.8, 0.5) * crackEdge * uIntensity;

    // Base void between cracks — deep purple
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float coreFade = smoothstep(0.0, 0.5, 1.0 - edgeDist);
    float3 baseColor = uColor * coreFade * 0.6;

    // Scrolling sub-surface energy visible through cracks
    float2 energyUV = float2(coords.x * 5.0 - uTime * 1.5, coords.y * 2.0);
    float energy = tex2D(uImage1, energyUV).r;
    float energyFlow = sin(energy * 6.28 + uTime * 3.0) * 0.5 + 0.5;
    crackColor += uSecondaryColor * energyFlow * cracks * 0.4;

    // Combine layers
    float3 finalColor = baseColor + crackColor;

    // Aggressive jagged edge — the trail itself has torn borders
    float jaggedEdge = noise * 0.3;
    float edgeFade = 1.0 - smoothstep(0.45 - jaggedEdge, 0.9, edgeDist);
    float tipFade = smoothstep(0.0, 0.08, coords.x) * smoothstep(0.0, 0.1, 1.0 - coords.x);

    float finalAlpha = edgeFade * tipFade * uOpacity * input.Color.a;
    finalAlpha *= saturate(0.3 + cracks * 0.7 + coreFade * 0.3);

    return float4(finalColor, finalAlpha);
}

float4 PS_SwingGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float softEdge = exp(-edgeDist * edgeDist * 1.8);

    // Slow alternation between deep purple and crack green
    float alt = sin(uTime * 2.2 + coords.x * 3.0) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor * 0.5, uSecondaryColor * 0.6, alt * uIntensity);

    float tipFade = smoothstep(0.0, 0.15, coords.x) * smoothstep(0.0, 0.15, 1.0 - coords.x);
    float bloomStr = 0.2 + 0.2 * uIntensity;
    float finalAlpha = softEdge * tipFade * bloomStr * uOpacity * input.Color.a;

    return float4(glowColor, finalAlpha);
}

technique CadenceSwingFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingFlow();
    }
}

technique CadenceSwingGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingGlow();
    }
}
