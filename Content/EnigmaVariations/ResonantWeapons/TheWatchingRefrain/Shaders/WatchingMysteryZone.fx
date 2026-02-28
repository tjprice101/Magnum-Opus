// ============================================================================
// WatchingMysteryZone.fx — TheWatchingRefrain mystery zone area shader
// SpriteBatch-style pixel shader (no custom vertex shader).
// Renders a swirling vortex with an eye-like pattern at center,
// deep purple with green veins of light, slowly rotating.
// ============================================================================

sampler uImage0 : register(s0);  // Base texture (circle/mask)
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;            // Primary color (deep purple)
float3 uSecondaryColor;   // Secondary color (gaze green)
float uOpacity;            // Overall opacity
float uTime;               // Elapsed time
float uIntensity;          // Effect strength (0→1)

float4 PS_MysteryField(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Center-relative coordinates
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Swirling vortex: rotate UV based on distance from center
    float swirlAmount = (1.0 - dist) * 3.0;
    float swirlAngle = angle + swirlAmount * sin(uTime * 0.8) + uTime * 0.3;

    // Sample noise with swirled coordinates for veins of light
    float2 swirlUV = float2(
        cos(swirlAngle) * dist + 0.5 + uTime * 0.1,
        sin(swirlAngle) * dist + 0.5 - uTime * 0.15
    );
    float noise = tex2D(uImage1, swirlUV).r;

    // Second noise layer for more complexity
    float2 noiseUV2 = float2(dist * 2.0 - uTime * 0.2, angle * 0.5 + uTime * 0.3);
    float noise2 = tex2D(uImage1, noiseUV2).r;

    // Green veins: thin bright lines from noise
    float veins = smoothstep(0.55, 0.65, noise * 0.7 + noise2 * 0.3);
    veins *= (1.0 - smoothstep(0.0, 0.45, dist)); // Fade veins at edges

    // Eye-like pattern at center: elliptical bright core
    float2 eyeScale = float2(1.8, 1.0); // Wider than tall
    float eyeDist = length(delta * eyeScale);
    float eyeCore = smoothstep(0.12, 0.0, eyeDist); // Bright center pupil
    float eyeIris = smoothstep(0.25, 0.1, eyeDist) * (1.0 - eyeCore); // Surrounding iris

    // Pupil constricts with intensity
    float pupilSize = lerp(0.08, 0.04, uIntensity);
    float pupil = smoothstep(pupilSize + 0.02, pupilSize, eyeDist);

    // Vortex pull darkness (darker toward center outside eye)
    float vortexDark = smoothstep(0.0, 0.4, dist);

    // Color composition
    float3 baseColor = uColor * vortexDark; // Deep purple base, darker toward center
    float3 veinColor = uSecondaryColor * veins * 1.5; // Bright green veins
    float3 eyeColor = lerp(uSecondaryColor, float3(1, 1, 1), 0.4) * eyeIris * uIntensity;
    float3 pupilColor = float3(0.02, 0.01, 0.05) * pupil; // Near-black pupil

    float3 finalColor = baseColor + veinColor + eyeColor;
    finalColor = lerp(finalColor, pupilColor, pupil * 0.8);

    // Edge fade (circular boundary)
    float edgeFade = 1.0 - smoothstep(0.35, 0.5, dist);

    // Slow rotation modulated breathing
    float breath = sin(uTime * 1.5) * 0.08 + 0.92;

    float finalAlpha = edgeFade * uOpacity * breath * color.a;

    return float4(finalColor, finalAlpha);
}

technique WatchingMysteryField
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_MysteryField();
    }
}
