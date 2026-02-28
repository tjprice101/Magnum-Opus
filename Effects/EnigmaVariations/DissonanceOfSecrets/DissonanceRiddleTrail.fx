// ============================================================================
// DissonanceRiddleTrail.fx — DissonanceOfSecrets riddlebolt trail
// UNIQUE SIGNATURE: Alternating encrypted/decrypted segments — the trail is
// composed of discrete blocks that alternate between scrambled noise (encrypted,
// dark, chaotic) and resolved clean regions (decrypted, bright, orderly).
// A visible transition wave sweeps along the trail, decrypting as it goes.
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

float4 PS_RiddleFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Segment the trail into discrete blocks
    float segmentSize = 0.1;
    float segmentIndex = floor(coords.x / segmentSize);
    float segmentFrac = frac(coords.x / segmentSize);

    // Decryption wave — sweeps along trail at varying speed
    float wavePos = frac(uTime * 0.6);
    float decryptionWave = smoothstep(wavePos - 0.15, wavePos, coords.x) *
                           smoothstep(wavePos + 0.15, wavePos, coords.x);

    // Per-segment state: encrypted or decrypted (alternates with time influence)
    float segHash = frac(sin(segmentIndex * 73.1 + floor(uTime * 1.5) * 37.3) * 43758.5);
    float isDecrypted = step(0.5 - uIntensity * 0.15, segHash);

    // Transition state at the decryption wave boundary
    float isTransitioning = decryptionWave;

    // ENCRYPTED appearance: noisy, dark, scrambled
    float2 scrambleUV = float2(coords.x * 8.0 + uTime * 2.0, coords.y * 6.0 - uTime);
    float scramble = tex2D(uImage1, scrambleUV).r;
    float2 scrambleUV2 = float2(coords.x * 12.0 - uTime * 1.5, coords.y * 4.0 + uTime * 0.7);
    float scramble2 = tex2D(uImage1, scrambleUV2).r;
    float encryptedPattern = scramble * 0.6 + scramble2 * 0.4;
    encryptedPattern = smoothstep(0.3, 0.7, encryptedPattern);

    // DECRYPTED appearance: clean, structured, bright bands
    float bandCount = 5.0;
    float band = sin(coords.y * bandCount * 3.14159) * 0.5 + 0.5;
    band = smoothstep(0.3, 0.7, band);
    float decryptedPattern = band * 0.8 + 0.2;

    // TRANSITION appearance: scan line sweeping across segment
    float scanLine = smoothstep(0.45, 0.5, segmentFrac) * smoothstep(0.55, 0.5, segmentFrac);
    float transitionPattern = lerp(encryptedPattern, decryptedPattern, segmentFrac);
    transitionPattern += scanLine * 0.5;

    // Blend based on state
    float pattern;
    float3 segColor;

    float encrypted = (1.0 - isDecrypted) * (1.0 - isTransitioning);
    float decrypted = isDecrypted * (1.0 - isTransitioning);
    float trans = isTransitioning;

    pattern = encrypted * encryptedPattern +
              decrypted * decryptedPattern +
              trans * transitionPattern;

    // Colors
    // Encrypted: deep purple, noisy
    float3 encColor = uColor * encryptedPattern * 0.6;
    // Decrypted: bright green, clean
    float3 decColor = uSecondaryColor * decryptedPattern * 1.2;
    // Transitioning: bright white flash with both colors
    float3 transColor = lerp(uColor, uSecondaryColor, segmentFrac) * transitionPattern;
    transColor += float3(0.4, 0.6, 0.4) * scanLine;

    segColor = encColor * encrypted + decColor * decrypted + transColor * trans;

    // Segment borders — thin lines between segments
    float border = smoothstep(0.03, 0.0, segmentFrac) + smoothstep(0.97, 1.0, segmentFrac);
    border *= 0.3;
    segColor += uSecondaryColor * border;

    // Edge fade
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.5, 1.0, edgeDist);

    // Core glow pulse
    float corePulse = sin(uTime * 3.0 + coords.x * 5.0) * 0.1 + 0.9;
    float coreGlow = (1.0 - edgeDist) * 0.15 * corePulse;
    segColor += uSecondaryColor * coreGlow;

    float finalAlpha = edgeFade * uOpacity * input.Color.a * saturate(pattern + border + 0.1);
    finalAlpha *= uIntensity;

    return float4(segColor, saturate(finalAlpha));
}

technique DissonanceRiddleFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_RiddleFlow();
    }
}
