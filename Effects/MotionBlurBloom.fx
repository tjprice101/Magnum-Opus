// =============================================================================
// MagnumOpus — Motion Blur Bloom Shader
// =============================================================================
//
// Directional motion blur combined with bloom enhancement for high-speed VFX.
// Three blur kernel shapes, each at three quality tiers:
//
//   Shape            | Best For
//   -----------------|-----------------------------------------
//   Directional      | Projectiles, dashing entities
//   Radial           | Explosions, impacts, radial bursts
//   ArcSweep         | Melee swings, rotational motion
//
//   Quality          | Taps | Shader Model
//   -----------------|------|--------------
//   Standard         |  5   | ps_2_0
//   HQ               |  9   | ps_2_0
//   Ultra            | 13   | ps_3_0
//
// Compile:
//   fxc.exe /T fx_2_0 /O2 /Fo Effects/MotionBlurBloom.fxc Effects/MotionBlurBloom.fx
//
// =============================================================================

sampler uImage0 : register(s0);

// Standard MagnumOpus uniforms (matches SimpleBloomShader convention)
float3 uColor;            // Primary bloom tint color
float3 uSecondaryColor;   // Secondary color for gradient blending
float uOpacity;           // Overall output opacity  (0-1)
float uTime;              // Animation time (Main.GlobalTimeWrappedHourly)
float uIntensity;         // Bloom intensity multiplier (1.0 = standard)

// Motion blur specific uniforms
float2 uVelocityDir;     // Normalized velocity direction
float uBlurStrength;      // Blur spread in UV space (0.01-0.15 typical)


// =============================================================================
//  DIRECTIONAL BLUR — Linear streak along velocity direction
//  Best for: Projectiles, dashing entities
// =============================================================================

float4 DirectionalBlur5(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 step = uVelocityDir * uBlurStrength;

    // 5-tap center-weighted kernel (weights sum to 1.0)
    float4 color = tex2D(uImage0, coords)              * 0.30;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords + step)              * 0.15;
    color += tex2D(uImage0, coords - step)              * 0.15;

    // Bloom: brighten based on luminance
    float lum   = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float bloom  = saturate(lum * uIntensity * 2.0);

    // Velocity-aware gradient tint
    float grad   = saturate(dot(coords - 0.5, uVelocityDir) + 0.5);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 DirectionalBlur9(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 step = uVelocityDir * uBlurStrength;

    // 9-tap smooth kernel (weights sum to 1.0)
    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords - step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords + step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords - step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords + step)              * 0.08;
    color += tex2D(uImage0, coords - step)              * 0.08;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.05 + 1.0;
    float bloom  = saturate(lum * uIntensity * 2.0) * pulse;

    float grad   = saturate(dot(coords - 0.5, uVelocityDir) + 0.5);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 DirectionalBlur13(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 step = uVelocityDir * uBlurStrength;
    float  s    = 1.0 / 6.0;   // step fraction

    // 13-tap ultra-smooth kernel (weights sum to 1.0)
    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * s)          * 0.12;
    color += tex2D(uImage0, coords - step * s)          * 0.12;
    color += tex2D(uImage0, coords + step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords - step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords + step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords - step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords + step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords - step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords + step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords - step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords + step)              * 0.02;
    color += tex2D(uImage0, coords - step)              * 0.02;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.04 + 1.0;
    float bloom  = saturate(lum * uIntensity * 2.5) * pulse;

    float grad   = saturate(dot(coords - 0.5, uVelocityDir) + 0.5);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}


// =============================================================================
//  RADIAL BLUR — Burst from center outward
//  Best for: Explosions, impacts, radial energy bursts
// =============================================================================

float4 RadialBlur5(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    float2 step     = toCenter * invDist * uBlurStrength;

    float4 color = tex2D(uImage0, coords)              * 0.30;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords + step)              * 0.15;
    color += tex2D(uImage0, coords - step)              * 0.15;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float bloom  = saturate((1.0 - dist * 2.0) * lum * uIntensity * 2.0);

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 RadialBlur9(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    float2 step     = toCenter * invDist * uBlurStrength;

    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords - step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords + step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords - step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords + step)              * 0.08;
    color += tex2D(uImage0, coords - step)              * 0.08;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.05 + 1.0;
    float bloom  = saturate((1.0 - dist * 2.0) * lum * uIntensity * 2.0) * pulse;

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 RadialBlur13(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    float2 step     = toCenter * invDist * uBlurStrength;
    float  s        = 1.0 / 6.0;

    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * s)          * 0.12;
    color += tex2D(uImage0, coords - step * s)          * 0.12;
    color += tex2D(uImage0, coords + step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords - step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords + step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords - step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords + step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords - step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords + step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords - step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords + step)              * 0.02;
    color += tex2D(uImage0, coords - step)              * 0.02;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.04 + 1.0;
    float bloom  = saturate((1.0 - dist * 2.0) * lum * uIntensity * 2.5) * pulse;

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}


// =============================================================================
//  ARC SWEEP BLUR — Tangential/rotational motion
//  Best for: Melee swings, spinning attacks, circular motion
// =============================================================================

float4 ArcSweepBlur5(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    // Tangent = perpendicular to radial direction
    float2 tangent  = float2(-toCenter.y, toCenter.x);
    float2 step     = tangent * invDist * uBlurStrength;

    float4 color = tex2D(uImage0, coords)              * 0.30;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.20;
    color += tex2D(uImage0, coords + step)              * 0.15;
    color += tex2D(uImage0, coords - step)              * 0.15;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    // Arc bloom: stronger further from center (blade tip > pommel)
    float bloom  = saturate(dist * 2.0 * lum * uIntensity * 2.0);

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 ArcSweepBlur9(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    float2 tangent  = float2(-toCenter.y, toCenter.x);
    float2 step     = tangent * invDist * uBlurStrength;

    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords - step * 0.25)      * 0.14;
    color += tex2D(uImage0, coords + step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords - step * 0.50)      * 0.12;
    color += tex2D(uImage0, coords + step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords - step * 0.75)      * 0.08;
    color += tex2D(uImage0, coords + step)              * 0.08;
    color += tex2D(uImage0, coords - step)              * 0.08;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.05 + 1.0;
    float bloom  = saturate(dist * 2.0 * lum * uIntensity * 2.0) * pulse;

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}

float4 ArcSweepBlur13(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 toCenter = coords - float2(0.5, 0.5);
    float  dist     = length(toCenter);
    float  invDist  = 1.0 / max(dist, 0.001);
    float2 tangent  = float2(-toCenter.y, toCenter.x);
    float2 step     = tangent * invDist * uBlurStrength;
    float  s        = 1.0 / 6.0;

    float4 color = tex2D(uImage0, coords)              * 0.16;
    color += tex2D(uImage0, coords + step * s)          * 0.12;
    color += tex2D(uImage0, coords - step * s)          * 0.12;
    color += tex2D(uImage0, coords + step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords - step * s * 2.0)    * 0.10;
    color += tex2D(uImage0, coords + step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords - step * s * 3.0)    * 0.08;
    color += tex2D(uImage0, coords + step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords - step * s * 4.0)    * 0.06;
    color += tex2D(uImage0, coords + step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords - step * s * 5.0)    * 0.04;
    color += tex2D(uImage0, coords + step)              * 0.02;
    color += tex2D(uImage0, coords - step)              * 0.02;

    float lum    = dot(color.rgb, float3(0.299, 0.587, 0.114));
    float pulse  = sin(uTime * 3.0) * 0.04 + 1.0;
    float bloom  = saturate(dist * 2.0 * lum * uIntensity * 2.5) * pulse;

    float grad   = saturate(dist * 2.0);
    float3 tint  = lerp(uColor, uSecondaryColor, grad);

    float3 result = color.rgb * tint * (1.0 + bloom);
    return float4(result, color.a * uOpacity * sampleColor.a);
}


// =============================================================================
//  TECHNIQUES — Standard (5-tap ps_2_0)
// =============================================================================

technique DirectionalBlurTechnique
{
    pass DirectionalBlurPass
    {
        PixelShader = compile ps_2_0 DirectionalBlur5();
    }
}

technique RadialBlurTechnique
{
    pass RadialBlurPass
    {
        PixelShader = compile ps_2_0 RadialBlur5();
    }
}

technique ArcSweepBlurTechnique
{
    pass ArcSweepBlurPass
    {
        PixelShader = compile ps_2_0 ArcSweepBlur5();
    }
}

// =============================================================================
//  TECHNIQUES — High Quality (9-tap ps_2_0)
// =============================================================================

technique DirectionalBlurHQTechnique
{
    pass DirectionalBlurHQPass
    {
        PixelShader = compile ps_2_0 DirectionalBlur9();
    }
}

technique RadialBlurHQTechnique
{
    pass RadialBlurHQPass
    {
        PixelShader = compile ps_2_0 RadialBlur9();
    }
}

technique ArcSweepBlurHQTechnique
{
    pass ArcSweepBlurHQPass
    {
        PixelShader = compile ps_2_0 ArcSweepBlur9();
    }
}

// =============================================================================
//  TECHNIQUES — Ultra Quality (13-tap ps_2_0)
// =============================================================================

technique DirectionalBlurUltraTechnique
{
    pass DirectionalBlurUltraPass
    {
        PixelShader = compile ps_2_0 DirectionalBlur13();
    }
}

technique RadialBlurUltraTechnique
{
    pass RadialBlurUltraPass
    {
        PixelShader = compile ps_2_0 RadialBlur13();
    }
}

technique ArcSweepBlurUltraTechnique
{
    pass ArcSweepBlurUltraPass
    {
        PixelShader = compile ps_2_0 ArcSweepBlur13();
    }
}
