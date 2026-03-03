// ConvergenceBeamShader.fx
// Self-contained beam shader for LaserFoundation.
// Adapted from SandboxLastPrism's ComboLaserVertexGradient shader.
//
// This shader renders a multi-layered scrolling beam effect:
// - 4 detail textures scroll along the beam at different speeds
// - Each detail texture is tinted by a color gradient LUT sampled at different scroll offsets
// - An alpha mask (onTex) shapes the beam cross-section profile
// - The final output composites a desaturated luminance core with a squared saturated detail layer
//
// UV mapping:
//   UV.x = 0..1 along beam length (start -> tip)
//   UV.y = 0..1 across beam width (edge -> edge, 0.5 = center)

// ---- TRANSFORM ----
matrix WorldViewProjection;

// ---- INTENSITY CONTROLS ----
float totalMult;       // Overall brightness multiplier for the desaturated core
float satPower;        // Controls white core vs colored detail balance (0 = pure color, 1 = strong white core)

// ---- UV REPETITION ----
float gradientReps;    // How many times the gradient repeats along the beam length
float tex1reps;        // Detail texture 1 UV.x repetition
float tex2reps;        // Detail texture 2 UV.x repetition
float tex3reps;        // Detail texture 3 UV.x repetition
float tex4reps;        // Detail texture 4 UV.x repetition

// ---- DETAIL INTENSITY ----
float tex1Mult;        // Brightness multiplier for detail texture 1
float tex2Mult;        // Brightness multiplier for detail texture 2
float tex3Mult;        // Brightness multiplier for detail texture 3
float tex4Mult;        // Brightness multiplier for detail texture 4

// ---- GRADIENT SCROLL SPEEDS ----
float grad1Speed;      // Gradient scroll speed for detail texture 1's color
float grad2Speed;      // Gradient scroll speed for detail texture 2's color
float grad3Speed;      // Gradient scroll speed for detail texture 3's color
float grad4Speed;      // Gradient scroll speed for detail texture 4's color

// ---- COLOR ----
float3 baseColor;      // Base color tint (typically white)

// ---- TIME ----
float uTime;           // Accumulated time value (drives all UV scrolling)

// ---- TEXTURES ----

// Alpha/shape mask — defines the beam's cross-section profile
// Bright center = full opacity, dark edges = transparent
texture onTex;
sampler2D samplerOnTex = sampler_state
{
    texture = <onTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Color gradient LUT — 1D horizontal gradient texture
// Sampled at different UV.x offsets per layer to create shifting rainbow bands
texture gradientTex;
sampler2D samplerGradientTex = sampler_state
{
    texture = <gradientTex>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Detail texture 1 (e.g., thin glow line)
texture sampleTexture1;
sampler2D samplerTex1 = sampler_state
{
    texture = <sampleTexture1>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Detail texture 2 (e.g., spark/energy)
texture sampleTexture2;
sampler2D samplerTex2 = sampler_state
{
    texture = <sampleTexture2>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Detail texture 3 (e.g., vanilla extra texture)
texture sampleTexture3;
sampler2D samplerTex3 = sampler_state
{
    texture = <sampleTexture3>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
};

// Detail texture 4 (e.g., looping trail strip)
texture sampleTexture4;
sampler2D samplerTex4 = sampler_state
{
    texture = <sampleTexture4>;
    magfilter = LINEAR;
    minfilter = LINEAR;
    mipfilter = LINEAR;
    AddressU = wrap;
    AddressV = wrap;
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
// Transforms vertices from world space to screen space using the game's view matrix.
// Passes through vertex color and UVs for the pixel shader.
VertexShaderOutput MainVS(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
};

// ---- PIXEL SHADER ----
// Multi-layer beam compositing:
// 1. Sample the gradient LUT at 4 different scroll speeds → 4 color values
// 2. Sample the alpha mask for beam cross-section shape
// 3. Sample 4 detail textures scrolling at different speeds 
// 4. Tint each detail texture by its corresponding gradient color
// 5. Composite: desaturated luminance core + squared saturated detail
float4 ConvergenceBeam(VertexShaderOutput input) : COLOR0
{
    float2 UV = input.TextureCoordinates.xy;

    // Step 1: Sample gradient LUT at 4 different scroll speeds
    // Each creates a different color band pattern that shifts at its own rate
    float4 gradColor1 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad1Speed), UV.y));
    float4 gradColor2 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad2Speed), UV.y));
    float4 gradColor3 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad3Speed), UV.y));
    float4 gradColor4 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad4Speed), UV.y));

    // Step 2: Alpha mask — scrolls along beam, shapes the cross-section
    float alpha = tex2D(samplerOnTex, float2(UV.x + (1.0f * uTime), UV.y)).a;
    float4 input_color = float4(baseColor, alpha);

    // Step 3: Sample 4 detail textures, each at different scroll speeds
    // Multiply by float4(1,1,1,0) to zero out their alpha (we use the mask's alpha instead)
    float4 col1 = tex2D(samplerTex1, float2(frac(UV.x * tex1reps + (0.75f * uTime)), UV.y)) * float4(1, 1, 1, 0);
    float4 col2 = tex2D(samplerTex2, float2(frac(UV.x * tex2reps + (1.0f * uTime)), UV.y)) * float4(1, 1, 1, 0);
    float4 col3 = tex2D(samplerTex3, float2(frac(UV.x * tex3reps + (1.25f * uTime)), UV.y)) * float4(1, 1, 1, 0);
    float4 col4 = tex2D(samplerTex4, float2(frac(UV.x * tex4reps + (1.5f * uTime)), UV.y)) * float4(1, 1, 1, 0);

    // Step 4: Tint each detail texture by its gradient color and intensity multiplier
    col1 *= gradColor1 * tex1Mult;
    col2 *= gradColor2 * tex2Mult;
    col3 *= gradColor3 * tex3Mult;
    col4 *= gradColor4 * tex4Mult;

    // Step 5: Composite
    // combined1 = desaturated luminance core: length of all colors * base * satPower
    // This creates a bright white center whose strength is controlled by satPower
    float4 combined1 = length(col1 + col2 + col3 + col4) * float4(input_color.rgb * 0.3f, satPower) * input_color.a;

    // combined2 = core + squared color detail (squaring increases contrast — brights get brighter, darks darker)
    float4 combined2 = (combined1 * totalMult) + (pow(col1 + col2 + col3 + col4, float4(2, 2, 2, 2)));

    // Multiply by vertex alpha (vertex color is White with alpha=1, but this allows the strip to fade vertices)
    float input_alpha = input.Color.a;
    return combined2 * input_alpha;
}

technique BasicColorDrawing
{
    pass MainPS
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ConvergenceBeam();
    }
};
