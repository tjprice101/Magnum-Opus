// ============================================================================
// VoidVariationSwingTrail.fx — VariationsOfTheVoid melee swing trail shader
// Two techniques:
//   VoidVariationSwingFlow — Main swing trail with dark matter void energy,
//                            wisps bleeding off edges, noise dissolution at tail,
//                            purple core with teal edge highlights
//   VoidVariationSwingGlow — Soft bloom layer behind the swing trail
// vs_3_0 + ps_3_0, width correction, uWorldViewProjection
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary trail color (variation violet)
float3 uSecondaryColor;  // Secondary color (rift teal)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling/animation
float uIntensity;         // Void intensity buildup (0-1)
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
// VoidVariationSwingFlow — Dark matter void energy trail: flowing wisps of
// darkness bleeding off the edges, noise-driven dissolution at the tail,
// deep purple core with teal edge highlights
// ---------------------------------------------------------------------------
float4 PS_SwingFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Scrolling UV for dark matter flow along the trail
    float2 scrollUV = float2(coords.x - uTime * 0.65, coords.y);

    // Noise sampling — primary void wisp layer
    float2 noiseUV1 = float2(coords.x * 3.5 - uTime * 1.0, coords.y * 2.5 + uTime * 0.4);
    float noise1 = tex2D(uImage1, noiseUV1).r;

    // Secondary noise — finer void dissolution detail
    float2 noiseUV2 = float2(coords.x * 7.0 + uTime * 0.5, coords.y * 4.0 - uTime * 0.7);
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // Combined noise for wisping dark matter pattern
    float wispNoise = noise1 * 0.55 + noise2 * 0.45;

    // Edge distance — wisps bleeding off the trail boundary
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float wispEdge = smoothstep(0.45, 1.0, edgeDist + wispNoise * 0.35 - 0.1);
    float edgeFade = 1.0 - wispEdge;

    // Distort the trail UV with noise for organic flowing feel
    float distortAmt = 0.035 + 0.025 * uIntensity;
    float2 distortedUV = scrollUV + float2(wispNoise * distortAmt, wispNoise * distortAmt * 0.6);

    // Sample the base trail texture
    float4 baseSample = tex2D(uImage0, distortedUV);

    // Color gradient: deep purple core → teal edge highlights
    float coreFade = smoothstep(0.0, 0.5, 1.0 - edgeDist);
    float3 trailColor = lerp(uSecondaryColor, uColor, coreFade);

    // Dissolution at the tail — dark matter evaporating
    float tailDissolution = smoothstep(0.0, 0.25, coords.x);
    float dissolveMask = step(wispNoise * 0.6 + 0.4, tailDissolution + 0.3);

    // Teal edge highlight brightening at wisp boundaries
    float edgeBright = smoothstep(0.35, 0.5, edgeDist) * (1.0 - smoothstep(0.5, 0.8, edgeDist));
    trailColor += uSecondaryColor * edgeBright * 0.6 * uIntensity;

    // Tip fade (leading and trailing edges)
    float tipFade = smoothstep(0.0, 0.12, coords.x) * smoothstep(0.0, 0.12, 1.0 - coords.x);

    float3 finalColor = trailColor * (baseSample.r * 0.35 + 0.65);
    float finalAlpha = edgeFade * tipFade * dissolveMask * uOpacity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

// ---------------------------------------------------------------------------
// VoidVariationSwingGlow — Soft bloom layer behind the main swing trail
// Broader, gentler, provides depth and ambient void glow
// ---------------------------------------------------------------------------
float4 PS_SwingGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Soft edge falloff — broader than the main flow
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float softEdge = 1.0 - smoothstep(0.25, 1.0, edgeDist);

    // Gentle noise modulation for organic feel
    float2 noiseUV = float2(coords.x * 2.0 - uTime * 0.35, coords.y * 1.5);
    float noise = tex2D(uImage1, noiseUV).r;

    // Slow color oscillation between purple and teal
    float colorOsc = sin(coords.x * 2.5 + uTime * 1.2) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor, uSecondaryColor, colorOsc * 0.45);

    // Bloom intensity scales with void intensity
    float bloomStrength = 0.22 + 0.18 * uIntensity;

    float tipFade = smoothstep(0.0, 0.18, coords.x) * smoothstep(0.0, 0.18, 1.0 - coords.x);
    float finalAlpha = softEdge * tipFade * bloomStrength * uOpacity * (noise * 0.3 + 0.7) * input.Color.a;

    return float4(glowColor, finalAlpha);
}

technique VoidVariationSwingFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingFlow();
    }
}

technique VoidVariationSwingGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_SwingGlow();
    }
}
