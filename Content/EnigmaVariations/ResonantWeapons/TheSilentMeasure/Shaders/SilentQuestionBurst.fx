// ============================================================================
// SilentQuestionBurst.fx — TheSilentMeasure "?" explosion shader
// Technique 1: Expanding burst with question-mark-like radial distortion
// Technique 2: Soft background glow for the burst
// SpriteBatch-style (ps_3_0 only, no custom vertex shader)
// ============================================================================

sampler uImage0 : register(s0);  // Base texture (sprite)
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;            // Primary color (Question Violet)
float3 uSecondaryColor;   // Secondary color (Enigma Emerald)
float uOpacity;            // Overall opacity
float uTime;               // Animation progress (0→1)
float uIntensity;          // Effect strength

float4 PS_QuestionBlast(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Radial distance and angle from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);

    // Question mark motif — create an asymmetric radial distortion
    // The "?" shape: a curved hook in the upper portion + a dot below
    // Achieved by warping the radial ring with angle-dependent offset
    float hookWarp = sin(angle * 1.0 + 1.57) * 0.12 * (1.0 - uTime);        // Hook curve
    float dotWarp = smoothstep(0.08, 0.0, length(toCenter - float2(0.0, 0.15))); // Dot below

    // Expanding ring
    float ringRadius = lerp(0.05, 0.5, uTime) + hookWarp;
    float ringWidth = 0.1 * (1.0 - uTime * 0.7);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringWidth, 0.0, ringDist);

    // Add the question dot
    ring = saturate(ring + dotWarp * (1.0 - uTime));

    // Noise distortion on the ring edges
    float2 noiseUV = float2(angle * 0.318 + uTime * 0.5, dist * 3.0);
    float noise = tex2D(uImage1, noiseUV).r;
    ring *= (noise * 0.5 + 0.5);

    // Color: violet outer → emerald inner → white flash at peak
    float colorMix = saturate(1.0 - dist * 2.5 + uTime * 0.5);
    float3 burstColor = lerp(uColor, uSecondaryColor, colorMix);

    // Bright flash at the start that fades
    float flash = exp(-uTime * 5.0) * uIntensity;
    burstColor += float3(1, 1, 1) * flash * 0.4;

    // Sample base texture for shape modulation
    float4 baseSample = tex2D(uImage0, coords);

    float alpha = ring * uOpacity * color.a * baseSample.a;
    return float4(burstColor, alpha);
}

float4 PS_QuestionGlow(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Soft radial glow — wider and more diffuse than the blast
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center);

    // Gaussian-like glow falloff
    float glow = exp(-dist * dist * 6.0);

    // Pulse subtly
    float pulse = sin(uTime * 4.0) * 0.1 + 0.9;

    // Color blend from secondary (emerald) to primary (violet)
    float3 glowColor = lerp(uSecondaryColor, uColor, dist * 1.5) * pulse;

    // Fade over time
    float timeFade = 1.0 - saturate(uTime);

    float alpha = glow * uOpacity * 0.35 * uIntensity * timeFade * color.a;
    return float4(glowColor, alpha);
}

technique SilentQuestionBlast
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_QuestionBlast();
    }
}

technique SilentQuestionGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_QuestionGlow();
    }
}
