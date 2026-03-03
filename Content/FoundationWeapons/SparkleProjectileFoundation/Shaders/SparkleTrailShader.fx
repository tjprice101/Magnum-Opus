// SparkleTrailShader.fx
// Self-contained glitter trail shader for SparkleProjectileFoundation.
//
// Renders a VertexStrip trail as a sparkling, glittery ribbon behind each crystal.
// The shader creates a shimmering effect by:
// 1. Sampling a star/noise texture with UV scrolling for internal sparkle motion
// 2. Applying a gradient LUT for theme-consistent coloring
// 3. Computing procedural sparkle flashes using sin waves at multiple frequencies
// 4. Soft edge fade on UV.y and tip fade on UV.x
// 5. Combining a bright core pass with a wider soft glow pass
//
// UV mapping (from VertexStrip):
//   UV.x = 0..1 along trail length (tail → head)
//   UV.y = 0..1 across trail width (edge → edge, 0.5 = center)

// ---- TRANSFORM ----
matrix WorldViewProjection;

// ---- TIME ----
float uTime;           // Accumulated time drives sparkle animation

// ---- TRAIL PARAMETERS ----
float trailIntensity;  // Overall brightness multiplier
float sparkleSpeed;    // How fast sparkle pattern scrolls
float sparkleScale;    // UV tile scale for the sparkle texture
float glitterDensity;  // Controls how many sparkle points appear (frequency multiplier)
float tipFadeStart;    // UV.x value where tip fade begins (0.6 = fade last 40%)
float edgeSoftness;    // How soft the trail edges are (higher = softer)

// ---- COLOR ----
float3 coreColor;      // Bright inner core color (usually white/highlight)
float3 outerColor;     // Outer glow color (theme primary)

// ---- TEXTURES ----

// Sparkle pattern texture — provides internal detail (star or noise texture)
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

// Gradient LUT — theme color ramp sampled by intensity
texture gradientTex;
sampler2D samplerGradientTex = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = clamp;
};

// Glow mask texture — soft circle for bloom falloff
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
// Creates a sparkling glitter trail effect with procedural twinkle and texture-driven detail.
float4 SparkleTrailPS(VertexShaderOutput input) : COLOR0
{
    float2 UV = input.TextureCoordinates.xy;
    
    // ---- EDGE FADE ----
    // Soft fade at trail edges (UV.y = 0 and 1 are edges, 0.5 is center)
    float distFromCenter = abs(UV.y - 0.5) * 2.0; // 0 at center, 1 at edge
    float edgeFade = smoothstep(1.0, 1.0 - edgeSoftness, distFromCenter);
    
    // ---- TIP FADE ----
    // Trail fades at the tail end (UV.x near 0) and slightly at head (UV.x near 1)
    float tailFade = smoothstep(0.0, 0.15, UV.x);      // Fade in from the oldest point
    float headFade = smoothstep(1.0, tipFadeStart, UV.x); // Fade toward tip
    float lengthFade = tailFade * headFade;
    
    // ---- SPARKLE TEXTURE SAMPLE ----
    // Scroll the sparkle texture along the trail for internal motion
    float2 sparkleUV = float2(
        UV.x * sparkleScale + uTime * sparkleSpeed,
        UV.y * 2.0 + uTime * sparkleSpeed * 0.3
    );
    float sparkleTexSample = tex2D(samplerSparkleTex, sparkleUV).r;
    
    // ---- PROCEDURAL GLITTER ----
    // Multiple sin waves at different frequencies create scattered sparkle flashes
    // Each "sparkle" is a brief bright flash at a particular UV position
    float glitter1 = sin(UV.x * glitterDensity * 17.3 + uTime * 4.1) 
                    * sin(UV.y * glitterDensity * 23.7 + uTime * 3.3);
    float glitter2 = sin(UV.x * glitterDensity * 31.1 + uTime * 5.7 + 1.5) 
                    * sin(UV.y * glitterDensity * 19.3 + uTime * 2.9 + 0.8);
    float glitter3 = sin(UV.x * glitterDensity * 43.7 + uTime * 3.5 + 3.1)
                    * sin(UV.y * glitterDensity * 37.1 + uTime * 4.7 + 2.1);
    
    // Raise to high power → brief bright peaks, long dark valleys = sparkle points
    float glitterBase = max(max(glitter1, glitter2), glitter3);
    float glitterFlash = pow(saturate(glitterBase), 8.0); // Very peaky = sharp sparkles
    
    // Combine texture detail with procedural glitter
    float sparkleIntensity = sparkleTexSample * 0.4 + glitterFlash * 0.8;
    
    // ---- GRADIENT LUT COLORING ----
    // Map sparkle intensity → gradient color for theme tinting
    float gradientCoord = saturate(sparkleIntensity * 0.8 + UV.x * 0.2);
    float3 gradientColor = tex2D(samplerGradientTex, float2(gradientCoord, 0.5)).rgb;
    
    // ---- CORE + OUTER COMPOSITE ----
    // Bright core: white/highlight color at high sparkle intensities
    float coreStrength = pow(saturate(sparkleIntensity), 2.0);
    float3 coreContrib = coreColor * coreStrength;
    
    // Outer glow: softer, wider, uses gradient color for theme identity
    float outerStrength = sparkleIntensity * 0.5 + 0.1; // Always some base glow
    float3 outerContrib = lerp(outerColor, gradientColor, 0.6) * outerStrength;
    
    // Blend: core + outer, weighted by proximity to center
    float centerWeight = 1.0 - distFromCenter * 0.5;
    float3 finalColor = (coreContrib * centerWeight + outerContrib) * trailIntensity;
    
    // ---- FINAL ALPHA ----
    float baseAlpha = (sparkleIntensity * 0.7 + 0.15) * edgeFade * lengthFade;
    float finalAlpha = baseAlpha * input.Color.a;
    
    return float4(finalColor, finalAlpha);
}

technique BasicColorDrawing
{
    pass SparkleTrailPass
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 SparkleTrailPS();
    }
};
