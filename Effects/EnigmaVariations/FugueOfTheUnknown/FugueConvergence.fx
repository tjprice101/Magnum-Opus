// ============================================================================
// FugueConvergence.fx — FugueOfTheUnknown convergence detonation
// UNIQUE SIGNATURE: Dual-source standing wave interference — two ripple
// sources create visible constructive/destructive interference nodes.
// Bright dots at constructive peaks, dark voids at destructive nodes.
// The visual metaphor of multiple musical voices colliding into harmony.
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

float4 PS_ConvergenceWave(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float2 center = float2(0.5, 0.5);

    // Two wave sources that converge toward center over time
    float sourceSpread = lerp(0.35, 0.0, uIntensity); // sources move together
    float2 source1 = center + float2(-sourceSpread, 0.0);
    float2 source2 = center + float2(sourceSpread, 0.0);

    // Distance from each source
    float d1 = length(coords - source1);
    float d2 = length(coords - source2);

    // Waves from each source (traveling outward)
    float waveFreq = 25.0;
    float waveSpeed = 4.0;
    float wave1 = sin(d1 * waveFreq - uTime * waveSpeed) * 0.5 + 0.5;
    float wave2 = sin(d2 * waveFreq - uTime * waveSpeed) * 0.5 + 0.5;

    // Interference pattern — add waves
    // Constructive: both positive → bright (sum near 2)
    // Destructive: one positive, one negative → dark (sum near 0)
    float sumWave = (wave1 + wave2) * 0.5;
    float productWave = wave1 * wave2; // emphasizes overlap

    // Standing wave nodes — where interference creates stable bright/dark points
    float pathDiff = abs(d1 - d2); // path length difference
    float constructive = pow(cos(pathDiff * waveFreq * 0.5), 2.0);
    float destructive = 1.0 - constructive;

    // Bright nodes at constructive interference
    float nodeGlow = constructive * sumWave;
    nodeGlow = smoothstep(0.3, 0.8, nodeGlow);

    // Dark voids at destructive interference
    float voidDark = destructive * (1.0 - sumWave);
    voidDark = smoothstep(0.3, 0.7, voidDark);

    // Noise for organic variability
    float2 noiseUV = float2(coords.x * 3.0 + uTime * 0.2, coords.y * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;

    // Radial fade
    float dist = length(coords - center);
    float radialFade = 1.0 - smoothstep(0.2, 0.5, dist);

    // Color composition
    // Constructive nodes: bright cyan-green
    float3 nodeColor = uSecondaryColor * 1.8 * nodeGlow;
    nodeColor += float3(0.5, 0.8, 0.6) * smoothstep(0.6, 0.9, nodeGlow) * uIntensity;

    // Destructive voids: deep purple (darker than base)
    float3 voidColor = uColor * 0.3 * (1.0 - voidDark);

    // Wave body: purple-green gradient based on interference state
    float3 waveColor = lerp(uColor, uSecondaryColor, constructive) * sumWave * 0.5;

    // Convergence center: white-hot as sources meet
    float centerGlow = exp(-dist * dist * 20.0) * uIntensity;
    float3 centerColor = lerp(float3(0.8, 1.0, 0.9), float3(1, 1, 1), centerGlow) * centerGlow;

    float3 finalColor = (voidColor + waveColor + nodeColor + centerColor) * uIntensity;
    finalColor *= (noise * 0.2 + 0.8); // subtle texture

    float alphaContent = sumWave * 0.3 + nodeGlow * 0.5 + centerGlow * 0.4;
    float finalAlpha = saturate(alphaContent) * radialFade * uOpacity * input.Color.a;

    return float4(finalColor, finalAlpha);
}

float4 PS_ConvergenceGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    float glow = exp(-dist * dist * 4.0);
    float pulse = sin(uTime * 3.5 + dist * 10.0) * 0.12 + 0.88;
    float3 glowColor = lerp(uColor, uSecondaryColor, dist * 1.5) * pulse;

    float glowAlpha = glow * uOpacity * 0.35 * uIntensity * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique FugueConvergenceWave
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_ConvergenceWave();
    }
}

technique FugueConvergenceGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_ConvergenceGlow();
    }
}
