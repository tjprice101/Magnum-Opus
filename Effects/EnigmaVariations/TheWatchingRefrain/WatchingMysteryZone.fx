// ============================================================================
// WatchingMysteryZone.fx — TheWatchingRefrain mystery zone area
// UNIQUE SIGNATURE: Panopticon surveillance grid — multiple small eye shapes
// arranged in a rotating hexagonal formation, all gazing toward the zone center.
// Green gaze-lines connect eyes to center. The zone feels like being watched
// from every angle simultaneously.
// ============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float hash1(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float4 PS_MysteryField(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 center = float2(0.5, 0.5);
    float2 delta = coords - center;
    float dist = length(delta);
    float angle = atan2(delta.y, delta.x);

    // Circular boundary fade
    float zoneFade = 1.0 - smoothstep(0.35, 0.5, dist);

    // Rotating hexagonal grid of eyes
    float rotation = uTime * 0.3;
    float cosR = cos(rotation);
    float sinR = sin(rotation);
    float2 rotDelta = float2(
        delta.x * cosR - delta.y * sinR,
        delta.x * sinR + delta.y * cosR
    );

    // Hex grid
    float gridScale = 6.0;
    float2 hexUV = rotDelta * gridScale;
    // Skew for hexagonal tiling
    float2 skew = float2(hexUV.x + hexUV.y * 0.577, hexUV.y * 1.155);
    float2 cell = floor(skew);
    float2 f = frac(skew) - 0.5;

    // Eye at each hex cell
    float cellHash = hash1(cell);
    float hasEye = step(0.4 - uIntensity * 0.1, cellHash);

    // Eye shape (small, simple)
    float2 eyeScale = float2(1.0, 2.0);
    float eyeDist = length(f * eyeScale);
    float eyeShape = smoothstep(0.4, 0.25, eyeDist) * hasEye;
    float pupil = smoothstep(0.12, 0.06, eyeDist) * hasEye;

    // Eyes blink at different times
    float blinkPhase = sin(uTime * 2.0 + cellHash * 20.0);
    float blink = smoothstep(-0.6, -0.4, blinkPhase);
    eyeShape *= blink;
    pupil *= blink;

    // Gaze lines — thin lines from each eye toward zone center
    // Direction from this pixel toward center in the rotated frame
    float2 toCenter = -rotDelta;
    float2 toCenterNorm = normalize(toCenter);
    // Line parallel to gaze direction through cell center
    float2 cellCenter = (cell + 0.5) / gridScale;
    // Approximate distance from pixel to the gaze line
    float gazeAngle = atan2(toCenterNorm.y, toCenterNorm.x);
    float pixelAngle = atan2(rotDelta.y - cellCenter.y, rotDelta.x - cellCenter.x);
    float gazeLine = smoothstep(0.05, 0.0, abs(sin(pixelAngle - gazeAngle)) * dist * 2.0);
    gazeLine *= hasEye * blink * 0.3 * (1.0 - smoothstep(0.0, 0.15, eyeDist));

    // Swirling vortex base
    float swirlAngle = angle + (0.5 - dist) * 2.0 * sin(uTime * 0.5) + uTime * 0.2;
    float2 swirlUV = float2(cos(swirlAngle) * dist + 0.5, sin(swirlAngle) * dist + 0.5);
    float noise = tex2D(uImage1, swirlUV).r;
    float vortexBase = noise * 0.2 * (1.0 - dist * 2.0);

    // Central all-seeing eye (larger, always open)
    float2 bigEyeScale = float2(1.8, 1.0);
    float bigEyeDist = length(delta * bigEyeScale / 0.12);
    float bigIris = smoothstep(1.2, 0.6, bigEyeDist) * smoothstep(0.0, 0.3, bigEyeDist);
    float bigPupil = smoothstep(0.4, 0.2, bigEyeDist);

    // Color composition
    float3 baseColor = uColor * vortexBase * zoneFade;

    // Small eyes: green iris with dark pupil
    float3 smallEyeColor = uSecondaryColor * 1.2 * eyeShape * (1.0 - pupil);
    smallEyeColor += float3(0.01, 0.005, 0.02) * pupil; // dark pupil

    // Big central eye
    float3 bigEyeColor = lerp(uSecondaryColor, float3(1, 1, 1), 0.3) * bigIris * uIntensity;
    bigEyeColor = lerp(bigEyeColor, float3(0.01, 0.005, 0.02), bigPupil * 0.8);

    // Gaze lines: faint green
    float3 gazeColor = uSecondaryColor * gazeLine * 0.5;

    float3 finalColor = baseColor + smallEyeColor + bigEyeColor + gazeColor;

    // Breathing
    float breath = sin(uTime * 1.5) * 0.08 + 0.92;

    float alpha = zoneFade * breath * uOpacity * color.a;
    alpha *= saturate(vortexBase * 2.0 + eyeShape + bigIris * 0.5 + gazeLine + 0.1);

    return float4(finalColor, alpha);
}

technique WatchingMysteryField
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_MysteryField();
    }
}
