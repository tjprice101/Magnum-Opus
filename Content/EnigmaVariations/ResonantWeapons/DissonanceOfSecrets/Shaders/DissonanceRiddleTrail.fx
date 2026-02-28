// ============================================================================
// DissonanceRiddleTrail.fx — DissonanceOfSecrets riddlebolt trail shader
// Renders the homing riddlebolt trails as scrolling cipher-text patterns
// flowing along the trail UV with a glowing green-to-purple gradient
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture (doubles as cipher pattern)

float3 uColor;           // Primary trail color (secret purple)
float3 uSecondaryColor;  // Secondary color (cascade green)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling
float uIntensity;         // Trail brightness scaling
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

float4 PS_RiddleFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;

    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Scrolling UV — cipher text flowing along the trail
    float2 scrollUV = float2(coords.x * 3.0 - uTime * 1.2, coords.y * 2.0);

    // Sample noise as cipher-text pattern — two layers at different scales
    float cipher1 = tex2D(uImage1, scrollUV).r;
    float cipher2 = tex2D(uImage1, scrollUV * 0.5 + float2(uTime * 0.3, 0.5)).r;

    // Interlocking pattern: combine two noise reads for complex cipher look
    float pattern = saturate(cipher1 * 0.6 + cipher2 * 0.4);

    // Sharpen the pattern into distinct cipher-like fragments
    pattern = smoothstep(0.3, 0.7, pattern);

    // Sample base trail texture for underlying shape
    float2 baseUV = float2(coords.x - uTime * 0.5, coords.y);
    float4 baseSample = tex2D(uImage0, baseUV);

    // Edge fade — soft edges along trail width
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.5, 1.0, edgeDist);

    // Color gradient: green at the leading edge (fresh riddle) → purple at tail (fading secret)
    float colorMix = saturate(coords.x * 0.8 + pattern * 0.2);
    float3 trailColor = lerp(uSecondaryColor, uColor, colorMix);

    // Brightness: pattern drives intensity, brighter where cipher text appears
    float brightness = (pattern * 0.7 + 0.3) * (baseSample.r * 0.5 + 0.5) * uIntensity;

    // Add a subtle pulsing glow at the core
    float corePulse = sin(uTime * 4.0 + coords.x * 6.0) * 0.15 + 0.85;
    float coreGlow = (1.0 - edgeDist) * 0.3 * corePulse;
    brightness += coreGlow;

    float3 finalColor = trailColor * brightness;
    float finalAlpha = edgeFade * uOpacity * input.Color.a * saturate(pattern * 0.5 + 0.5);

    return float4(finalColor, finalAlpha);
}

technique DissonanceRiddleFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_RiddleFlow();
    }
}
