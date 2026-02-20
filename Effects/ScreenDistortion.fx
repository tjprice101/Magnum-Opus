// =============================================================================
// MagnumOpus Screen Distortion Shader  —  PS 2.0 Compatible
// =============================================================================
// Three screen-space post-process techniques for impacts, heat, and aberration.
// Designed for tModLoader's Filters.Scene system (ScreenShaderData).
//
// Techniques:
//   RippleTechnique   – Expanding radial sine-wave ring  (impacts, shockwaves)
//   HeatHazeTechnique – Vertical shimmer distortion      (fire, explosions)
//   ChromaticTechnique – RGB channel separation           (powerful hits, tears)
//
// Compile:
//   fxc.exe /T fx_2_0 /O2 /Fo Effects/ScreenDistortion.fxc Effects/ScreenDistortion.fx
// =============================================================================

// Scene render target — provided automatically by Filters.Scene
sampler uImage0 : register(s0);

// ---- Standard ScreenShaderData parameters ----
float  uIntensity;       // Overall effect strength
float2 uTargetPosition;  // Effect centre in screen UV (0-1)
float  uProgress;        // Animation progress (0 = start, 1 = end)
float  uTime;            // Main.GlobalTimeWrappedHourly
float3 uColor;           // Primary colour (unused by distortion, reserved)
float3 uSecondaryColor;  // Secondary colour (unused by distortion, reserved)
float  uOpacity;         // Master opacity (reserved)


// =============================================================================
//  RIPPLE  –  Expanding radial sine-wave ring
// =============================================================================
//  uProgress drives ring expansion (0 → centre, 1 → fully expanded).
//  uIntensity scales displacement amplitude.
//  Good for impacts, shockwaves, and the "TriggerRipple" / "TriggerPulse" paths.
// =============================================================================

float4 RipplePS(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float2 toCenter = uv - uTargetPosition;
    float  dist     = length(toCenter);

    // Ring geometry — expands with progress, thickens slightly
    float ringRadius = uProgress * 0.6;
    float ringWidth  = 0.06 + uProgress * 0.04;
    float ringDist   = abs(dist - ringRadius);
    float ringMask   = saturate(1.0 - ringDist / ringWidth);

    // Sine ripple along ring
    float wave = sin(dist * 40.0 - uTime * 10.0);

    // Radial displacement direction (avoid /0)
    float2 dir = (dist > 0.001) ? (toCenter / dist) : float2(0.0, 0.0);

    // Displacement magnitude
    float disp = wave * ringMask * uIntensity * 0.025;

    // Fade near screen edges to prevent sampling outside [0,1]
    float edgeFade = saturate(min(uv.x, 1.0 - uv.x) * 10.0)
                   * saturate(min(uv.y, 1.0 - uv.y) * 10.0);
    disp *= edgeFade;

    float2 distortedUV = clamp(uv + dir * disp, 0.0, 1.0);
    return tex2D(uImage0, distortedUV);
}


// =============================================================================
//  HEAT HAZE  –  Vertical shimmer distortion
// =============================================================================
//  Single-octave procedural shimmer from 2 sin() calls (2 sincos = 16 slots).
//  No secondary texture sampler needed.
//  Good for fire, explosions, and the "TriggerHeatHaze" / "TriggerWarp" paths.
//
//  Instruction budget: ~35 arithmetic  (limit 64)
// =============================================================================

float4 HeatHazePS(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float2 toCenter = uv - uTargetPosition;
    float  dist     = length(toCenter);

    // Radial falloff (radius ≈ 0.3), fades with progress
    float fade = saturate(1.0 - dist * 3.33);
    fade *= fade * saturate(1.0 - uProgress);

    // Single-octave sin interference — 2 sin calls = 16 slots
    float noise = sin(uv.x * 30.0 + uTime * 3.0)
                * sin(uv.y * 35.0 + uTime * 2.0);

    // Displacement — primarily vertical (heat rises)
    float strength = uIntensity * fade;
    float2 disp;
    disp.x = noise * 0.008 * strength;
    disp.y = noise * 0.015 * strength;

    return tex2D(uImage0, clamp(uv + disp, 0.0, 1.0));
}


// =============================================================================
//  CHROMATIC  –  RGB channel separation
// =============================================================================
//  Radial chromatic aberration centred on uTargetPosition.
//  Good for "TriggerShatter", "TriggerTear", and "TriggerChromaticBurst".
//  3 tex2D reads — reuse center sample for G and A channels.
//
//  Instruction budget: ~25 arithmetic  (limit 64)
// =============================================================================

float4 ChromaticPS(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float2 toCenter = uv - uTargetPosition;
    float  dist     = length(toCenter);

    // Fade radially and with progress
    float fade = saturate(1.0 - dist * 1.5)
               * saturate(1.0 - uProgress);

    // Offset direction — avoid normalize (saves div + branch)
    float2 offset = toCenter * (fade * uIntensity * 0.015 / (dist + 0.001));

    // 3 tex2D reads: red-shifted, center (green + alpha), blue-shifted
    float4 center = tex2D(uImage0, uv);
    float  r = tex2D(uImage0, clamp(uv + offset, 0.0, 1.0)).r;
    float  b = tex2D(uImage0, clamp(uv - offset, 0.0, 1.0)).b;

    return float4(r, center.g, b, center.a);
}


// =============================================================================
//  TECHNIQUES — one per distortion type
// =============================================================================

technique RippleTechnique
{
    pass RipplePass
    {
        PixelShader = compile ps_2_0 RipplePS();
    }
}

technique HeatHazeTechnique
{
    pass HeatHazePass
    {
        PixelShader = compile ps_2_0 HeatHazePS();
    }
}

technique ChromaticTechnique
{
    pass ChromaticPass
    {
        PixelShader = compile ps_2_0 ChromaticPS();
    }
}
