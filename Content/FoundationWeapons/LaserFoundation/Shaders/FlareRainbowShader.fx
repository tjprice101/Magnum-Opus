// FlareRainbowShader.fx
// Self-contained radial rainbow shader for LaserFoundation endpoint flares.
// Adapted from SandboxLastPrism's RainbowSigil shader.
//
// What this shader does:
// 1. ROTATES the texture UVs around center — creates a spinning flare effect
// 2. RECOLORS the texture with radial rainbow via HSV→RGB conversion
//    - Hue is based on the pixel's angle from center (atan2) → rainbow wheel
//    - Saturation is based on distance from center → white core, colorful edges
//    - The rainbow rotation parameter shifts the entire color wheel over time
// 3. FADES alpha along UV.x — useful for directional flares aligned with the beam
//
// Without this shader, flares are plain white. With it, they become vibrant
// spinning rainbow flares that match the beam's multicolor aesthetic.
//
// Parameters:
//   rotation       — UV rotation angle (radians). Drives the spinning visual.
//   rainbowRotation — Hue offset angle (radians). Shifts the rainbow wheel over time.
//   intensity       — RGB brightness multiplier. Higher = brighter rainbow.
//   fadeStrength    — How quickly alpha fades along UV.x (1.0 = full fade across texture).

sampler uImage0 : register(s0);

float rotation;
float rainbowRotation;
float intensity;
float fadeStrength;

const float TWO_PI = 6.28318530718;

// Standard HSV → RGB conversion
// Input: float3(Hue 0..1, Saturation 0..1, Value 0..1)
// Output: float3(R, G, B) in 0..1
float3 hsv2rgb(float3 _c)
{
    float4 _K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 _p = abs(frac(_c.xxx + _K.xyz) * 6.0 - _K.www);
    return _c.z * lerp(_K.xxx, clamp(_p - _K.xxx, 0.0, 1.0), _c.y);
}

float4 RainbowFlarePixel(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 baseUV = screenspace.xy;

    // Fade alpha along UV.x (directional fade for beam-aligned flares)
    float alpha = 1.0 - clamp(baseUV.x * fadeStrength, 0.0, 1.0);

    // Rotate UVs around center (0.5, 0.5) for spinning effect
    float2x2 rotMatrix = float2x2(
        cos(rotation), -sin(rotation),
        sin(rotation),  cos(rotation)
    );
    float4 color = tex2D(uImage0, mul((baseUV - 0.5), rotMatrix) + 0.5);
    color.a = alpha * color.a;

    // Apply radial rainbow coloring via HSV
    float2 pos = float2(0.5, 0.5) - baseUV;
    color.rgb = intensity * hsv2rgb(float3(
        (atan2(pos.y, pos.x) + rainbowRotation) / TWO_PI + 0.5,  // Hue = angle from center
        length(pos) * 2.0,                                         // Saturation = distance from center
        1.0                                                         // Value = max brightness
    ));

    return color;
}

technique Technique1
{
    pass Aura
    {
        PixelShader = compile ps_3_0 RainbowFlarePixel();
    }
}
