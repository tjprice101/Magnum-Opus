// ============================================================================
// WatchingPhantomAura.fx — TheWatchingRefrain phantom aura shader
// Renders the phase-shifting phantom minion's spectral trail and glow.
// Technique 1: WatchingPhantomGhost — flickering opacity with noise-driven holes,
//              green-purple shimmer at edges (phantom phasing in/out of reality)
// Technique 2: WatchingPhantomGlow — soft ghostly bloom glow behind the phantom
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;            // Primary color (refrain purple)
float3 uSecondaryColor;   // Secondary color (gaze green)
float uOpacity;            // Overall opacity
float uTime;               // Elapsed time for animation
float uIntensity;          // Phase shift intensity (0→1)
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

// ── Technique 1: Spectral phase-shifting aura trail ──
float4 PS_Ghost(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Sample noise for phase-shifting holes (phantom phasing in/out)
    float2 noiseUV1 = float2(coords.x * 3.0 - uTime * 0.6, coords.y * 2.0 + uTime * 0.3);
    float2 noiseUV2 = float2(coords.x * 1.5 + uTime * 0.4, coords.y * 3.0 - uTime * 0.2);
    float noise1 = tex2D(uImage1, noiseUV1).r;
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // Combine noise to create flickering holes in the phantom
    float noiseMask = noise1 * 0.6 + noise2 * 0.4;
    float holeCutoff = lerp(0.3, 0.6, uIntensity);
    float holeAlpha = smoothstep(holeCutoff - 0.1, holeCutoff + 0.1, noiseMask);

    // Edge shimmer: green-purple at the trail edges
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.5, 1.0, edgeDist);
    float shimmer = sin(coords.x * 12.0 + uTime * 4.0) * 0.5 + 0.5;

    // Color: purple core → green shimmer at edges
    float colorMix = saturate(edgeDist * 0.6 + shimmer * 0.3 * uIntensity);
    float3 ghostColor = lerp(uColor, uSecondaryColor, colorMix);

    // Base trail sample for texture variation
    float2 scrollUV = float2(coords.x - uTime * 0.5, coords.y);
    float4 baseSample = tex2D(uImage0, scrollUV);
    ghostColor *= (baseSample.r * 0.4 + 0.6);

    // Flicker with time for spectral feel
    float flicker = sin(uTime * 7.0 + coords.x * 5.0) * 0.1 + 0.9;

    float finalAlpha = holeAlpha * edgeFade * uOpacity * flicker * input.Color.a;

    return float4(ghostColor, finalAlpha);
}

// ── Technique 2: Soft ghostly bloom behind phantom ──
float4 PS_Glow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Soft gaussian-like glow falloff
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float glow = exp(-edgeDist * edgeDist * 2.5);

    // Gentle pulse
    float pulse = sin(uTime * 2.5) * 0.12 + 0.88;

    // Color slowly shifts between purple and green
    float colorShift = sin(uTime * 1.5) * 0.5 + 0.5;
    float3 glowColor = lerp(uColor, uSecondaryColor, colorShift * 0.4) * pulse;

    float glowAlpha = glow * uOpacity * 0.35 * uIntensity * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique WatchingPhantomGhost
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Ghost();
    }
}

technique WatchingPhantomGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_Glow();
    }
}
