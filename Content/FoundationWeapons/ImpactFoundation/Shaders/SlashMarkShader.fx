// SlashMarkShader.fx
// Self-contained directional slash mark shader for ImpactFoundation.
//
// Draws a fluid slash arc at the impact point using SDF math with noise
// distortion for organic, dynamic edges. The slash appears to carve
// through space with a hot bright core and cooler outer glow.
//
// Pipeline:
//   1. Convert sprite UVs to center-relative coordinates
//   2. Rotate UV space to align with the slash direction (slashAngle)
//   3. Compute SDF for an elongated ellipse (the slash shape)
//   4. Apply noise distortion to the SDF for fluid, organic edges
//   5. Create hot core + cooler edge gradient using the SDF distance
//   6. Apply gradient LUT for theme-consistent color
//   7. Add energy flow along the slash length via UV scrolling
//   8. Fade with overall alpha
//
// Shader Model 2.0 compatible (fx_2_0).

sampler uImage0 : register(s0);

float uTime;
float slashAngle;    // Direction of the slash in radians
float3 primaryColor;
float3 coreColor;
float fadeAlpha;
float slashWidth;    // Half-width of the slash in normalized space (e.g. 0.12)
float slashLength;   // Half-length of the slash in normalized space (e.g. 0.4)

// Noise texture for edge distortion
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

static const float PI = 3.14159265;

float4 SlashMarkPS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    
    // ---- CENTER-RELATIVE COORDINATES ----
    float2 centered = uv - 0.5;
    
    // ---- ROTATE TO SLASH DIRECTION ----
    float cosA = cos(-slashAngle);
    float sinA = sin(-slashAngle);
    float2 rotated;
    rotated.x = centered.x * cosA - centered.y * sinA;
    rotated.y = centered.x * sinA + centered.y * cosA;
    
    // ---- NOISE DISTORTION ----
    // Sample noise to distort the slash edges for fluid organic feel
    float2 noiseUV = float2(rotated.x * 2.0 + uTime * 0.2, rotated.y * 3.0 + uTime * 0.1);
    float noiseVal = tex2D(samplerNoise, noiseUV).r;
    float distortion = (noiseVal - 0.5) * 0.05;
    
    // Apply distortion to the y axis (perpendicular to slash) for wobbly edges
    float2 distorted = float2(rotated.x, rotated.y + distortion);
    
    // ---- SDF: ELONGATED ELLIPSE (SLASH SHAPE) ----
    // Ellipse SDF: (x/a)^2 + (y/b)^2 where a=length, b=width
    float ex = distorted.x / slashLength;
    float ey = distorted.y / slashWidth;
    float ellipseDist = sqrt(ex * ex + ey * ey);
    
    // ---- SLASH SHAPE MASK ----
    // Soft edge for the slash shape
    float slashMask = 1.0 - smoothstep(0.8, 1.0, ellipseDist);
    
    // ---- CORE-TO-EDGE GRADIENT ----
    // Hot bright core, cooler outer region
    float coreIntensity = 1.0 - smoothstep(0.0, 0.6, ellipseDist);
    float edgeGlow = smoothstep(0.5, 0.9, ellipseDist) * (1.0 - smoothstep(0.9, 1.0, ellipseDist));
    
    // ---- ENERGY FLOW ALONG SLASH ----
    // Scrolling energy along the length of the slash
    float2 flowUV = float2(rotated.x * 3.0 + uTime * 0.5, rotated.y * 2.0);
    float flowNoise = tex2D(samplerNoise, flowUV).r;
    float energyFlow = flowNoise * slashMask;
    
    // ---- GRADIENT LUT COLORING ----
    float gradInput = saturate(energyFlow * 0.8 + coreIntensity * 0.5);
    float3 gradColor = tex2D(samplerGradient, float2(gradInput, 0.5)).rgb;
    
    // ---- COLOR COMPOSITION ----
    float3 baseColor = lerp(primaryColor * gradColor, coreColor, coreIntensity);
    
    // Add energy flow highlights
    baseColor += coreColor * energyFlow * 0.4;
    
    // Edge glow — add a softer rim light at the slash edge
    baseColor += primaryColor * edgeGlow * 0.5;
    
    // Boost overall brightness
    baseColor *= 2.0;
    
    // ---- FINAL COMPOSITE ----
    float finalAlpha = slashMask * fadeAlpha * saturate(energyFlow + coreIntensity * 1.5);
    float3 finalColor = baseColor * slashMask;
    
    return float4(finalColor * fadeAlpha, finalAlpha);
}

technique Technique1
{
    pass SlashMarkPass
    {
        PixelShader = compile ps_2_0 SlashMarkPS();
    }
}
