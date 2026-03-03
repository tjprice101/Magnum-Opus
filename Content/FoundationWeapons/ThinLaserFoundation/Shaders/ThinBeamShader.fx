// ThinBeamShader.fx
// Self-contained thin beam shader for ThinLaserFoundation.
// Simplified variant of ConvergenceBeamShader — uses 2 detail textures instead of 4.
//
// Designed for thin, fast beams that ricochet off surfaces.
// The shader renders a 2-layer scrolling beam with a white-hot core and
// gradient LUT coloring. Fewer layers = snappier, cleaner look for thin beams.
//
// UV mapping (same convention as ConvergenceBeamShader):
//   UV.x = 0..1 along beam length (start -> tip)
//   UV.y = 0..1 across beam width (edge -> edge, 0.5 = center)

// ---- TRANSFORM ----
matrix WorldViewProjection;

// ---- INTENSITY CONTROLS ----
float totalMult;       // Overall brightness multiplier for combined detail
float satPower;        // Controls gradient color vs base color blend (0 = base only, 1 = full gradient)

// ---- UV REPETITION ----
float gradientReps;    // How many times the gradient repeats along beam length
float tex1reps;        // Detail texture 1 UV.x repetition
float tex2reps;        // Detail texture 2 UV.x repetition

// ---- DETAIL INTENSITY ----
float tex1Mult;        // Brightness multiplier for detail texture 1
float tex2Mult;        // Brightness multiplier for detail texture 2

// ---- SCROLL SPEEDS ----
float grad1Speed;      // Gradient + detail 1 scroll speed
float grad2Speed;      // Detail 2 scroll speed

// ---- COLOR ----
float3 baseColor;      // Base tint color (typically white)

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
// Sampled at UV.x offsets to create scrolling color bands
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

// Detail texture 1 (e.g., ThinLinearGlow — thin energy line)
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

// Detail texture 2 (e.g., LightningSurge — crackling energy)
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
    float4 pos = mul(input.Position, WorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
};

// ---- PIXEL SHADER ----
// Simplified 2-layer beam compositing with white-hot core:
// 1. Sample gradient LUT at 2 different scroll speeds → 2 color values
// 2. Sample alpha mask for beam cross-section shape
// 3. Sample 2 detail textures scrolling at different speeds
// 4. Tint each detail texture by its corresponding gradient color
// 5. Add a white-hot core at the beam center (smoothstep on alpha mask)
// 6. Composite with premultiplied alpha
float4 ThinBeamPS(VertexShaderOutput input) : COLOR0
{
    float2 UV = input.TextureCoordinates.xy;

    // Step 1: Sample gradient LUT at 2 different scroll speeds
    float4 gradColor1 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad1Speed), UV.y));
    float4 gradColor2 = tex2D(samplerGradientTex, float2(UV.x * gradientReps + (uTime * grad2Speed), UV.y));

    // Step 2: Alpha mask — shapes the cross-section
    float alpha = tex2D(samplerOnTex, float2(UV.x + (1.0f * uTime), UV.y)).a;
    float4 input_color = float4(baseColor, alpha);

    // Step 3: Sample 2 detail textures at different scroll speeds
    float4 col1 = tex2D(samplerTex1, float2(frac(UV.x * tex1reps + (0.75f * uTime)), UV.y)) * float4(1, 1, 1, 0);
    float4 col2 = tex2D(samplerTex2, float2(frac(UV.x * tex2reps + (1.2f * uTime)), UV.y)) * float4(1, 1, 1, 0);

    // Step 4: Tint each detail by its gradient color
    col1 *= gradColor1 * tex1Mult;
    col2 *= gradColor2 * tex2Mult;

    // Step 5: Composite — desaturated core + squared color detail
    // The core uses the length of combined colors to create a white center
    float4 combined = col1 + col2;
    float luminance = length(combined);
    float4 core = luminance * float4(input_color.rgb * 0.4f, satPower) * input_color.a;
    float4 detail = pow(combined, float4(2, 2, 2, 2));

    // Step 6: White-hot center highlight — subtle brightness at beam center
    float centerBright = smoothstep(0.5, 0.7, alpha);
    float4 hotCore = centerBright * float4(1, 1, 1, 0) * 0.2 * input_color.a;

    float4 result = (core * totalMult) + detail + hotCore;

    // Multiply by vertex alpha for fade control
    float input_alpha = input.Color.a;
    return result * input_alpha;
}

technique BasicColorDrawing
{
    pass MainPS
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ThinBeamPS();
    }
};
