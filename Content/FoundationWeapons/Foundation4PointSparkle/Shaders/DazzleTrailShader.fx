// DazzleTrailShader.fx
// Shader-driven dazzling sparkle trail for Foundation4PointSparkle.
//
// Renders a VertexStrip trail as a shimmering ribbon of twinkling 4-point stars.
// The shader creates a mesmerizing trail by:
// 1. Sampling a star texture with multi-axis UV scrolling for flowing internal sparkle motion
// 2. Three layers of procedural glitter at different spatial frequencies and speeds
//    — each layer uses sin-wave interference with high-power sharpening for distinct twinkle peaks
// 3. Prismatic hue shifting tied to position along the trail and time
// 4. Smooth edge fade (UV.y) and graceful tail-to-tip fade (UV.x)
// 5. Hot white core at sparkle peaks with a softer colored outer glow
// 6. Standing-wave pattern along the trail length for rhythmic brightness pulsing
//
// UV mapping (from VertexStrip):
//   UV.x = 0..1 along trail length (tail → head)
//   UV.y = 0..1 across trail width (edge → edge, 0.5 = center)

// ---- TRANSFORM ----
matrix WorldViewProjection;

// ---- TIME ----
float uTime;

// ---- TRAIL PARAMETERS ----
float trailIntensity;   // Overall brightness multiplier
float sparkleSpeed;     // How fast sparkle patterns scroll
float sparkleScale;     // UV tile scale for sparkle texture
float glitterDensity;   // Controls sparkle point count (frequency multiplier)
float tipFadeStart;     // UV.x where tip fade begins
float edgeSoftness;     // How soft trail edges are
float pulseRate;        // Standing-wave pulse rate along the trail
float prismaticShift;   // How much hue rotates with position/time

// ---- COLOR ----
float3 coreColor;       // Hot inner core (usually near-white)
float3 outerColor;      // Theme outer glow
float3 accentColor;     // Secondary accent color for prismatic variety

// ---- TEXTURES ----

// Star pattern texture — drives internal sparkle detail
texture sparkleTex;
sampler2D samplerSparkleTex = sampler_state
{
    texture = <sparkleTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Secondary star texture — counter-scrolled for interference shimmer
texture sparkleTexB;
sampler2D samplerSparkleTexB = sampler_state
{
    texture = <sparkleTexB>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Glow mask — soft circle for cross-section shaping
texture glowMaskTex;
sampler2D samplerGlowMask = sampler_state
{
    texture = <glowMaskTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = clamp;
    AddressV = clamp;
};

// ---- HSV UTILITY ----
float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

// ---- VERTEX FORMAT ----
struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

// ---- VERTEX SHADER ----
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    output.Position = mul(input.Position, WorldViewProjection);
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
};

// ---- PIXEL SHADER ----
float4 DazzleTrailPS(VertexShaderOutput input) : COLOR0
{
    float2 UV = input.TextureCoordinates.xy;
    
    // ---- EDGE FADE ----
    float distFromCenter = abs(UV.y - 0.5) * 2.0;
    float edgeFade = smoothstep(1.0, 1.0 - edgeSoftness, distFromCenter);
    
    // ---- LENGTH FADE ----
    float tailFade = smoothstep(0.0, 0.12, UV.x);
    float headFade = smoothstep(1.0, tipFadeStart, UV.x);
    float lengthFade = tailFade * headFade;
    
    // ---- STAR TEXTURE SAMPLE A (forward-scrolling) ----
    float2 sparkleUV_A = float2(
        UV.x * sparkleScale + uTime * sparkleSpeed,
        UV.y * 2.0 + uTime * sparkleSpeed * 0.25
    );
    float starSampleA = tex2D(samplerSparkleTex, sparkleUV_A).r;
    
    // ---- STAR TEXTURE SAMPLE B (counter-scrolling, rotated, different scale) ----
    float2 sparkleUV_B = float2(
        UV.x * sparkleScale * 1.5 - uTime * sparkleSpeed * 0.7 + 0.3,
        UV.y * 3.0 + uTime * sparkleSpeed * 0.4 + 0.5
    );
    float starSampleB = tex2D(samplerSparkleTexB, sparkleUV_B).r;
    
    // Combine star textures — where both are bright, we get intense sparkle points
    float starComposite = starSampleA * 0.6 + starSampleB * 0.4;
    float starPeaks = pow(saturate(starSampleA * starSampleB * 2.0), 3.0);
    
    // ---- PROCEDURAL GLITTER (three frequency layers) ----
    
    // Layer 1: Medium frequency — the main twinkle field
    float g1 = sin(UV.x * glitterDensity * 19.7 + uTime * 4.3)
             * sin(UV.y * glitterDensity * 27.3 + uTime * 3.1);
    
    // Layer 2: High frequency — tiny rapid twinkles
    float g2 = sin(UV.x * glitterDensity * 41.3 + uTime * 6.1 + 1.7)
             * sin(UV.y * glitterDensity * 33.7 + uTime * 5.3 + 0.9);
    
    // Layer 3: Low frequency — broader pulsing glow regions
    float g3 = sin(UV.x * glitterDensity * 11.1 + uTime * 2.7 + 2.8)
             * sin(UV.y * glitterDensity * 13.9 + uTime * 2.1 + 1.4);
    
    // Sharp peak sharpening — higher power = more distinct twinkle points
    float glitter1 = pow(saturate(g1), 10.0);   // Very peaky
    float glitter2 = pow(saturate(g2), 12.0);   // Even peakier (tiny sparkles)
    float glitter3 = pow(saturate(g3), 4.0);    // Broader glow regions
    
    // Weighted combine
    float glitterFlash = glitter1 * 0.5 + glitter2 * 0.35 + glitter3 * 0.15;
    
    // ---- STANDING WAVE (rhythmic pulse along trail length) ----
    float wave = sin(UV.x * 3.14159 * 3.0 + uTime * pulseRate) * 0.5 + 0.5;
    float waveBrightness = 0.7 + wave * 0.3;
    
    // ---- COMBINED SPARKLE INTENSITY ----
    float sparkleIntensity = (starComposite * 0.35 + glitterFlash * 0.65 + starPeaks * 0.5) * waveBrightness;
    
    // ---- PRISMATIC HUE SHIFT ----
    // Hue rotates smoothly along trail and over time for rainbow shimmer
    float hue = frac(UV.x * 0.5 + uTime * prismaticShift + distFromCenter * 0.2);
    float3 prismColor = hsv2rgb(float3(hue, 0.3, 1.0));
    
    // ---- CORE + OUTER COMPOSITE ----
    // Hot core: bright white at high sparkle intensities
    float coreStrength = pow(saturate(sparkleIntensity * 1.5), 2.5);
    float3 coreContrib = coreColor * coreStrength;
    
    // Outer glow: softer, uses themed color blended with prismatic shift
    float outerStrength = sparkleIntensity * 0.5 + 0.08;
    float3 themedOuter = lerp(outerColor, accentColor, UV.x * 0.5 + 0.25);
    float3 outerContrib = lerp(themedOuter, prismColor, 0.25) * outerStrength;
    
    // ---- CENTER WEIGHTING ----
    float centerWeight = 1.0 - distFromCenter * 0.4;
    float3 finalColor = (coreContrib * centerWeight + outerContrib) * trailIntensity;
    
    // ---- FINAL ALPHA ----
    float baseAlpha = (sparkleIntensity * 0.75 + 0.1) * edgeFade * lengthFade;
    float finalAlpha = baseAlpha * input.Color.a;
    
    return float4(finalColor, finalAlpha);
}

technique BasicColorDrawing
{
    pass DazzleTrailPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 DazzleTrailPS();
    }
};
