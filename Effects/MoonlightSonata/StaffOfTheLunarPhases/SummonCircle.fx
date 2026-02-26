// ============================================================
// SummonCircle.fx  EStaff of the Lunar Phases
// Rotating sigil with lunar phase symbols for summoning ritual.
//
// Creates a glowing magical circle that rotates and pulses,
// with concentric rings and evenly-spaced node points that
// represent the 8 lunar phases. Used during the Goliath of
// Moonlight summoning sequence and Conductor Mode activation.
//
// Techniques:
//   SummonCircleMain  ECore sigil body with rotating rings + phase nodes
//   SummonCircleGlow  ESofter wider bloom pass for aura halo
//
// FNA/XNA compatible  Eps_2_0 profile.
// ============================================================

sampler uImage0 : register(s0); // Primary sprite texture
sampler uImage1 : register(s1); // Optional noise texture

float3 uColor;            // Primary color (nebula purple)
float3 uSecondaryColor;   // Secondary color (energy tendril)
float  uTime;             // Global time for animation
float  uOpacity;          // Master opacity
float  uIntensity;        // Circle intensity (0.2 faint ↁE1.0 full ritual)
float  uOverbrightMult;   // Overbright multiplier for bloom
float  uScrollSpeed;      // Ring rotation speed
float  uDistortionAmt;    // Edge distortion strength
float  uPhase;            // Ritual phase (0 = dormant, 1 = fully active)
float  uHasSecondaryTex;  // 1.0 if uImage1 is bound
float  uSecondaryTexScale; // UV scale for noise sampling
float  uSecondaryTexScroll; // Noise scroll speed
float  uNoiseScale;       // Procedural noise frequency

// ------ helpers ------
float hash21(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

// ------ MAIN CIRCLE BODY ------
float4 SummonCircleMainPS(float2 uv : TEXCOORD0, float4 col : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 center = uv - 0.5;
    float dist = length(center);
    float angle = atan2(center.y, center.x);

    // Rotation animation
    float rotSpeed = uTime * uScrollSpeed;

    // Concentric rings  E3 rings at different radii
    float ring1 = saturate(1.0 - abs(dist - 0.15) * 25.0);
    float ring2 = saturate(1.0 - abs(dist - 0.28) * 20.0);
    float ring3 = saturate(1.0 - abs(dist - 0.40) * 16.0);

    // Rotating ring modulation  Egaps in rings that sweep around
    float ringMod1 = sin(angle * 6.0 + rotSpeed * 2.0) * 0.5 + 0.5;
    float ringMod2 = sin(angle * 4.0 - rotSpeed * 1.5) * 0.5 + 0.5;
    float ringMod3 = sin(angle * 8.0 + rotSpeed * 1.0) * 0.5 + 0.5;

    ring1 *= 0.5 + ringMod1 * 0.5;
    ring2 *= 0.4 + ringMod2 * 0.6;
    ring3 *= 0.3 + ringMod3 * 0.7;

    // Phase node points  E8 evenly spaced glowing nodes on middle ring
    float nodePattern = 0.0;
    float nodeAngle = angle + rotSpeed * 0.5;
    float nodeIndex = floor((nodeAngle / 6.28318 + 0.5) * 8.0);
    float nodeFrac = frac((nodeAngle / 6.28318 + 0.5) * 8.0);
    float nodeProximity = saturate(1.0 - abs(nodeFrac - 0.5) * 6.0);
    float nodeRadial = saturate(1.0 - abs(dist - 0.28) * 12.0);
    nodePattern = nodeProximity * nodeRadial * uPhase;

    // Connector lines between nodes  Ethin radial spokes
    float spokeAngle = frac(angle / 6.28318 * 8.0 + rotSpeed * 0.07);
    float spoke = saturate(1.0 - abs(spokeAngle - 0.5) * 40.0);
    spoke *= saturate(dist * 3.0) * saturate(1.0 - (dist - 0.42) * 5.0);
    spoke *= uPhase * 0.4;

    // Center glow  Ethe singularity core
    float coreGlow = saturate(1.0 - dist * 5.0);
    coreGlow = pow(coreGlow, 1.5) * uPhase;

    // Noise modulation
    float noise = 0.5;
    if (uHasSecondaryTex > 0.5)
    {
        float2 noiseUV = uv * uSecondaryTexScale + float2(uTime * uSecondaryTexScroll * 0.5, uTime * uSecondaryTexScroll * 0.3);
        noise = tex2D(uImage1, noiseUV).r;
    }
    else
    {
        noise = hash21(uv * uNoiseScale + uTime * 0.2);
    }

    // Edge distortion from noise
    float edgeNoise = (noise - 0.5) * uDistortionAmt;

    // Combine all elements
    float pattern = (ring1 + ring2 + ring3) * 0.35
                  + nodePattern * 0.8
                  + spoke
                  + coreGlow * 0.6;

    // Noise shimmer
    pattern *= 0.8 + noise * 0.2 + edgeNoise;

    // Pulse with ritual phase
    float pulse = 0.9 + sin(uTime * 4.0 * uPhase) * 0.1;
    pattern *= pulse;

    // Color: rings get primary, nodes get secondary, core is white
    float colorMix = saturate(nodePattern * 0.6 + coreGlow * 0.4);
    float3 circleColor = lerp(uColor, uSecondaryColor, colorMix);

    // White-hot core overlay
    circleColor = lerp(circleColor, float3(1, 1, 1), coreGlow * 0.3);

    float3 finalColor = circleColor * pattern * uIntensity * uOverbrightMult;

    float alpha = baseTex.a * pattern * uOpacity * uIntensity;
    alpha = saturate(alpha);

    return float4(finalColor * alpha, alpha) * col;
}

// ------ GLOW PASS (wider, softer halo) ------
float4 SummonCircleGlowPS(float2 uv : TEXCOORD0, float4 col : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 center = uv - 0.5;
    float dist = length(center);

    // Soft radial glow  Ewider than main pass
    float glowFalloff = saturate(1.0 - dist * 1.6);
    glowFalloff = pow(glowFalloff, 0.7);

    // Subtle ring pattern in glow
    float ringGlow = sin(dist * 20.0 - uTime * uScrollSpeed) * 0.15 + 0.85;

    // Pulse
    float pulse = 0.85 + sin(uTime * 3.0) * 0.15;

    float pattern = glowFalloff * ringGlow * pulse;

    // Color shifted toward primary for soft aura
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3);
    float3 finalColor = glowColor * pattern * uIntensity * uOverbrightMult * 0.5;

    float alpha = baseTex.a * pattern * uOpacity * uIntensity * 0.4;
    alpha = saturate(alpha);

    return float4(finalColor * alpha, alpha) * col;
}

// ------ TECHNIQUES ------
technique SummonCircleMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 SummonCircleMainPS();
    }
}

technique SummonCircleGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 SummonCircleGlowPS();
    }
}
