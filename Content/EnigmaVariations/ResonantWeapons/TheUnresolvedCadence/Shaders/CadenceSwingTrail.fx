// ============================================================================
// CadenceSwingTrail.fx — TheUnresolvedCadence melee swing trail shader
// Two techniques:
//   CadenceSwingFlow — Main swing trail with dimensional rending jaggedness,
//                      deep violet core + green edge energy + noise crackling
//   CadenceSwingGlow — Soft bloom layer behind the swing trail
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary trail color (cadence violet)
float3 uSecondaryColor;  // Secondary color (dimensional green)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling/animation
float uIntensity;         // Inevitability stack intensity (0-1)
matrix uWorldViewProjection;

struct VertexInput
{
    float2 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TexCoord : TEXCOORD0;  // .xy = UV, .z = width correction
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

// ---------------------------------------------------------------------------
// CadenceSwingFlow — Dimensional rending trail: jagged distortion at edges,
// deep violet core with green edge energy, noise-driven cracking pattern
// ---------------------------------------------------------------------------
float4 PS_SwingFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Scrolling UV for energy flow along the trail
    float2 scrollUV = float2(coords.x - uTime * 0.8, coords.y);

    // Noise sampling — primary distortion layer (dimensional cracking)
    float2 noiseUV1 = float2(coords.x * 4.0 - uTime * 1.2, coords.y * 3.0 + uTime * 0.3);
    float noise1 = tex2D(uImage1, noiseUV1).r;

    // Secondary noise — finer crackling detail
    float2 noiseUV2 = float2(coords.x * 8.0 + uTime * 0.6, coords.y * 5.0 - uTime * 0.9);
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // Combined noise for jagged dimensional cracking
    float crackNoise = noise1 * 0.6 + noise2 * 0.4;

    // Jagged edge distortion — reality cracking at the trail boundary
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float jaggedEdge = smoothstep(0.5, 1.0, edgeDist + crackNoise * 0.3 - 0.15);
    float edgeFade = 1.0 - jaggedEdge;

    // Distort the trail UV with noise for organic crackling feel
    float distortAmt = 0.04 + 0.03 * uIntensity;
    float2 distortedUV = scrollUV + float2(crackNoise * distortAmt, crackNoise * distortAmt * 0.5);

    // Sample the base trail texture
    float4 baseSample = tex2D(uImage0, distortedUV);

    // Color gradient: deep violet core → green edge energy
    float coreFade = smoothstep(0.0, 0.5, 1.0 - edgeDist);
    float3 trailColor = lerp(uSecondaryColor, uColor, coreFade);

    // Crackling brightness along noise ridges
    float crackBright = smoothstep(0.55, 0.7, crackNoise) * 0.5 * uIntensity;
    trailColor += float3(0.3, 1.0, 0.4) * crackBright;

    // Tip fade (the leading edge of the swing)
    float tipFade = smoothstep(0.0, 0.15, coords.x) * smoothstep(0.0, 0.15, 1.0 - coords.x);

    float3 finalColor = trailColor * (baseSample.r * 0.4 + 0.6);
    float finalAlpha = edgeFade * tipFade * uOpacity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

// ---------------------------------------------------------------------------
// CadenceSwingGlow — Soft bloom layer behind the main swing trail
// Broader, gentler, provides depth and ambient glow
// ---------------------------------------------------------------------------
float4 PS_SwingGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Soft edge falloff — broader than the main flow
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float softEdge = 1.0 - smoothstep(0.3, 1.0, edgeDist);

    // Gentle noise modulation for organic feel
    float2 noiseUV = float2(coords.x * 2.0 - uTime * 0.4, coords.y * 1.5);
    float noise = tex2D(uImage1, noiseUV).r;

    // Slow color oscillation between violet and green
    float colorOsc = sin(coords.x * 3.0 + uTime * 1.5) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor, uSecondaryColor, colorOsc * 0.4);

    // Bloom intensity scales with inevitability
    float bloomStrength = 0.25 + 0.15 * uIntensity;

    float tipFade = smoothstep(0.0, 0.2, coords.x) * smoothstep(0.0, 0.2, 1.0 - coords.x);
    float finalAlpha = softEdge * tipFade * bloomStrength * uOpacity * (noise * 0.3 + 0.7) * input.Color.a;

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
