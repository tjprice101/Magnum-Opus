// ============================================================================
// TacetBulletTrail.fx — TacetsEnigma bullet trail shader
// Fast-scrolling energy trail for bullets and paradox bolts
// Sharp geometric patterns, purple→green edge gradient
// Intensity brightens for paradox bolt variant
// ============================================================================

sampler uImage0 : register(s0);  // Base trail texture
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary trail color (tacet purple)
float3 uSecondaryColor;  // Secondary color (paradox green)
float uOpacity;           // Overall opacity
float uTime;              // Elapsed time for scrolling
float uIntensity;         // Paradox bolt intensity (0 = normal bullet, 1 = paradox bolt)
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

float4 PS_BulletFlow(VertexOutput input) : COLOR0
{
    float2 coords = input.TexCoord.xy;
    
    // Width correction from vertex data
    float widthFactor = input.TexCoord.z;
    coords.y = (coords.y - 0.5) / max(widthFactor, 0.001) + 0.5;
    
    // Fast scrolling UV — faster than beam-type shaders for bullet feel
    float scrollSpeed = lerp(1.2, 2.5, uIntensity);
    float2 scrollUV = float2(coords.x - uTime * scrollSpeed, coords.y);
    
    // Sample noise for geometric distortion
    float2 noiseUV = float2(coords.x * 3.0 - uTime * 0.7, coords.y * 2.0);
    float noise = tex2D(uImage1, noiseUV).r;
    
    // Sharp geometric pattern — stepped noise for angular shard look
    float geometric = floor(noise * 4.0) / 4.0;
    float sharpNoise = lerp(noise, geometric, 0.5 * uIntensity);
    
    // Distort UV with noise
    float distortAmt = 0.03 + 0.04 * uIntensity;
    float2 distortedUV = scrollUV + float2(sharpNoise * distortAmt, sharpNoise * distortAmt * 0.3);
    
    // Sample base trail
    float4 baseSample = tex2D(uImage0, distortedUV);
    
    // Edge fade (bullet trails are sharper than beam trails)
    float edgeDist = abs(coords.y - 0.5) * 2.0;
    float edgeFade = 1.0 - smoothstep(0.5, 1.0, edgeDist);
    
    // Purple→green gradient from center to edges
    float colorMix = saturate(edgeDist * 0.6 + sharpNoise * 0.4);
    float3 trailColor = lerp(uColor, uSecondaryColor, colorMix);
    
    // Paradox bolt brightens and shifts greener
    float brightness = lerp(0.7, 1.5, uIntensity);
    trailColor = lerp(trailColor, uSecondaryColor, uIntensity * 0.3);
    
    // Tip brightening (near the bullet head)
    float tipBright = smoothstep(0.0, 0.3, 1.0 - coords.x) * 0.4;
    
    // Final color
    float3 finalColor = trailColor * brightness * (baseSample.r * 0.5 + 0.5) + tipBright;
    float finalAlpha = edgeFade * uOpacity * input.Color.a;
    
    return float4(finalColor, finalAlpha);
}

technique TacetBulletFlow
{
    pass P0
    {
        VertexShader = compile vs_3_0 VS_Main();
        PixelShader = compile ps_3_0 PS_BulletFlow();
    }
}
