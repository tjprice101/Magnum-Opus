// XSlashShader.fx
// Self-contained blazing X-slash impact shader for XSlashFoundation.
//
// Applied as a SpriteBatch effect to the X-ShapedImpactCross texture.
// The shader makes the X appear to burn with fluid, dynamic fire energy:
//
// Pipeline:
//   1. Sample the X-cross texture (white on black) to get the shape mask
//   2. Apply noise-driven UV distortion for organic fire-like warping
//   3. Scroll a second noise layer along the X arms for directional energy flow
//   4. Map combined intensity through a gradient LUT for theme coloring
//   5. Create hot core vs cooler edge gradient based on distance from center
//   6. Composite with overall fade alpha
//
// Shader Model 2.0 compatible (fx_2_0).

sampler uImage0 : register(s0); // The X-cross sprite texture bound by SpriteBatch

float uTime;
float3 edgeColor;     // Outer glow color
float3 midColor;      // Mid-tone color
float3 coreColor;     // Bright core color
float fadeAlpha;      // Overall fade multiplier (0-1)
float fireIntensity;  // How much the noise distorts the shape (e.g. 0.06)
float scrollSpeed;    // How fast the fire energy scrolls along the arms

// Noise texture for fire distortion + energy flow
texture noiseTex;
sampler2D samplerNoise = sampler_state
{
    texture = <noiseTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Gradient LUT for theme coloring
texture gradientTex;
sampler2D samplerGradient = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

float4 XSlashPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;

    // ---- NOISE-DRIVEN UV DISTORTION ----
    // Sample noise at two scales for organic fire warping
    float2 noiseUV1 = uv * 2.5 + float2(uTime * 0.15, uTime * 0.1);
    float noiseVal1 = tex2D(samplerNoise, noiseUV1).r;

    float2 noiseUV2 = uv * 4.0 + float2(-uTime * 0.2, uTime * 0.25);
    float noiseVal2 = tex2D(samplerNoise, noiseUV2).r;

    // Combined distortion offset
    float2 distortion = float2(
        (noiseVal1 - 0.5) * fireIntensity,
        (noiseVal2 - 0.5) * fireIntensity
    );

    // Apply distortion to sample the X-cross texture
    float2 distortedUV = uv + distortion;
    float4 xSample = tex2D(uImage0, distortedUV);
    float shapeMask = xSample.r; // White X on black background

    // ---- DIRECTIONAL ENERGY FLOW ----
    // Scroll energy along the X arms — both diagonal directions
    float2 centered = uv - 0.5;
    float diagFlow1 = centered.x + centered.y; // Along top-left to bottom-right arm
    float diagFlow2 = centered.x - centered.y; // Along top-right to bottom-left arm

    float2 flowUV1 = float2(diagFlow1 * 3.0 + uTime * scrollSpeed, uv.y * 2.0);
    float2 flowUV2 = float2(diagFlow2 * 3.0 - uTime * scrollSpeed * 0.8, uv.x * 2.0);
    float flow1 = tex2D(samplerNoise, flowUV1).r;
    float flow2 = tex2D(samplerNoise, flowUV2).r;
    float energyFlow = (flow1 + flow2) * 0.5;

    // ---- DISTANCE FROM CENTER (for core-to-edge gradient) ----
    float dist = length(centered) * 2.0;
    float coreIntensity = 1.0 - smoothstep(0.0, 0.5, dist);

    // ---- GRADIENT LUT COLORING ----
    float gradInput = saturate(energyFlow * 0.6 + shapeMask * 0.4 + coreIntensity * 0.2);
    float3 gradColor = tex2D(samplerGradient, float2(gradInput, 0.5)).rgb;

    // ---- COLOR COMPOSITION ----
    // Three-tier coloring: edge -> mid -> core
    float3 baseColor = edgeColor * gradColor;
    baseColor = lerp(baseColor, midColor * gradColor, smoothstep(0.2, 0.5, shapeMask));
    baseColor = lerp(baseColor, coreColor, coreIntensity * shapeMask);

    // Add energy flow highlights
    baseColor += coreColor * energyFlow * shapeMask * 0.4;

    // Add hot bright core at center intersection
    baseColor += coreColor * coreIntensity * shapeMask * 0.6;

    // Boost overall brightness
    baseColor *= 2.2;

    // ---- FINAL COMPOSITE ----
    float finalAlpha = shapeMask * fadeAlpha * saturate(energyFlow * 0.5 + shapeMask * 1.2);
    float3 finalColor = baseColor * shapeMask;

    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass XSlashPass
    {
        PixelShader = compile ps_2_0 XSlashPS();
    }
}
