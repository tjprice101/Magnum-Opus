// ============================================================================
// CipherBeamTrail.fx — CipherNocturne channeled beam trail
// UNIQUE SIGNATURE: Digital data stream — the beam is composed of discrete
// quantized bands that scroll at different speeds like encoded data channels.
// Bands flicker between "encrypted" (noisy/dark) and "decrypted" (bright/clean).
// Matrix-rain inspired vertical data flow within the horizontal beam.
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

float4 PS_BeamFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    // Quantize the beam into discrete horizontal bands (data channels)
    float bandCount = 8.0;
    float bandIndex = floor(coords.y * bandCount);
    float bandFrac = frac(coords.y * bandCount);
    float bandCenter = (bandIndex + 0.5) / bandCount;

    // Each band scrolls at a different speed (based on band index)
    float bandSpeed = 0.8 + frac(sin(bandIndex * 127.1) * 43758.5) * 1.5;
    float scrollX = coords.x - uTime * bandSpeed;

    // Quantize along X too — creating block-like data cells
    float cellSize = 0.08;
    float cellX = floor(scrollX / cellSize);
    float cellFrac = frac(scrollX / cellSize);

    // Hash for per-cell randomness (encrypted/decrypted state)
    float cellHash = frac(sin(cellX * 73.1 + bandIndex * 191.7 + floor(uTime * 2.0) * 37.3) * 43758.5);

    // Encrypted cells: noisy, dark, scrambled
    // Decrypted cells: clean, bright, resolved
    float decrypted = step(0.4 - uIntensity * 0.2, cellHash); // more intensity = more decrypted

    // Noise for encrypted cells
    float2 noiseUV = float2(scrollX * 6.0, coords.y * 4.0 + uTime * 0.5);
    float noise = tex2D(uImage1, noiseUV).r;

    // Encrypted appearance: noisy scrambled data
    float encrypted = (1.0 - decrypted);
    float scramble = noise * encrypted * 0.8;

    // Decrypted appearance: clean bright bands
    float clean = decrypted * 0.9;
    // Subtle vertical scan line within decrypted cells (matrix rain)
    float scanLine = smoothstep(0.4, 0.5, abs(bandFrac - 0.5));
    clean *= (0.8 + scanLine * 0.2);

    // Data cell edge highlight
    float cellEdgeX = smoothstep(0.0, 0.1, cellFrac) * smoothstep(1.0, 0.9, cellFrac);
    float cellEdgeY = smoothstep(0.0, 0.15, bandFrac) * smoothstep(1.0, 0.85, bandFrac);
    float cellMask = cellEdgeX * cellEdgeY;

    // Color composition
    // Encrypted: deep purple, noisy
    float3 encryptedColor = uColor * (0.3 + scramble * 0.7) * cellMask;
    // Decrypted: bright green, clean
    float3 decryptedColor = uSecondaryColor * clean * cellMask * 1.2;
    // Bright white flash at decryption moment
    float decryptFlash = step(0.38 - uIntensity * 0.2, cellHash) * step(cellHash, 0.42 - uIntensity * 0.2);
    float3 flashColor = float3(1, 1, 1) * decryptFlash * 0.5;

    float3 beamColor = encryptedColor + decryptedColor + flashColor;

    // Channel intensity ramp along beam length
    float lengthRamp = lerp(0.5, 1.3, coords.x) * uIntensity;
    beamColor *= lengthRamp;

    // Edge fade (beam body)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.7, 1.0, edgeDist);

    float finalAlpha = edgeFade * cellMask * uOpacity * input.Color.a * (scramble + clean + 0.1);

    return float4(beamColor, saturate(finalAlpha));
}

float4 PS_BeamGlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;

    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float glow = exp(-edgeDist * edgeDist * 2.5);

    // Data pulse along beam
    float pulse = sin(uTime * 4.0 + coords.x * 10.0) * 0.15 + 0.85;
    float3 glowColor = lerp(uColor, uSecondaryColor, edgeDist * 0.8) * pulse;

    float glowAlpha = glow * uOpacity * 0.35 * uIntensity * input.Color.a;

    return float4(glowColor, glowAlpha);
}

technique CipherBeamFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamFlow();
    }
}

technique CipherBeamGlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BeamGlow();
    }
}
