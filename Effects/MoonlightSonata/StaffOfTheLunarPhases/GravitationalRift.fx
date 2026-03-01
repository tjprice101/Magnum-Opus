// ============================================================
// GravitationalRift.fx  EStaff of the Lunar Phases
// Spiral gravity well distortion shader.
//
// Creates a swirling vortex pattern that simulates gravitational
// lensing around the Goliath of Moonlight. Particles appear to
// spiral inward toward a central singularity.
//
// Techniques:
//   GravitationalRiftMain  ECore vortex body with spiral arms
//   GravitationalRiftGlow  EWider soft bloom pass for the rift halo
//
// FNA/XNA compatible  Eps_3_0 profile.
// ============================================================

sampler uImage0 : register(s0); // Primary sprite texture
sampler uImage1 : register(s1); // Optional noise texture

float3 uColor;            // Primary color (gravity well purple)
float3 uSecondaryColor;   // Secondary color (star core white)
float  uTime;             // Global time for animation
float  uOpacity;          // Master opacity
float  uIntensity;        // Rift intensity (0.2 ambient ↁE1.0 full charge)
float  uOverbrightMult;   // Overbright multiplier for bloom effect
float  uScrollSpeed;      // Spiral rotation speed
float  uDistortionAmt;    // Gravitational distortion strength
float  uPhase;            // Rift phase (0 = dormant, 1 = fully open)
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

// ------ MAIN RIFT BODY ------
float4 GravitationalRiftMainPS(float2 uv : TEXCOORD0, float4 col : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    // Center UV for radial calculations
    float2 center = uv - 0.5;
    float dist = length(center);
    float angle = atan2(center.y, center.x);

    // Spiral distortion  EUV warped by gravitational pull
    float spiralPhase = uTime * uScrollSpeed * 2.0;
    float spiralWarp = sin(angle * 3.0 + dist * 12.0 - spiralPhase) * uDistortionAmt;
    float radialWarp = cos(angle * 2.0 - spiralPhase * 0.7) * uDistortionAmt * 0.5;

    float2 warpedUV = uv + float2(spiralWarp, radialWarp) * uPhase;

    // Sample noise for additional distortion
    float noise = 0.5;
    if (uHasSecondaryTex > 0.5)
    {
        float2 noiseUV = warpedUV * uSecondaryTexScale + float2(uTime * uSecondaryTexScroll, -uTime * uSecondaryTexScroll * 0.7);
        noise = tex2D(uImage1, noiseUV).r;
    }
    else
    {
        noise = hash21(warpedUV * uNoiseScale + uTime * 0.3);
    }

    // Spiral arm pattern  E3 arms rotating with gravity
    float spiralArms = sin(angle * 3.0 - dist * 8.0 + spiralPhase * 1.5) * 0.5 + 0.5;
    spiralArms = pow(spiralArms, 1.5);

    // Radial falloff  Ebright center fading outward (gravity well)
    float coreBright = saturate(1.0 - dist * 2.5);
    coreBright = pow(coreBright, 1.2);

    // Event horizon ring  Ebright ring at specific radius
    float ringDist = abs(dist - 0.25 * uPhase);
    float eventHorizon = saturate(1.0 - ringDist * 8.0) * uPhase;

    // Combine patterns
    float pattern = coreBright * 0.6
                  + spiralArms * 0.3 * uPhase
                  + eventHorizon * 0.4;

    // Noise modulation
    pattern *= 0.7 + noise * 0.3;

    // Color gradient: deep void at outer edge ↁEbright star core at center
    float colorMix = saturate(coreBright + eventHorizon * 0.5);
    float3 riftColor = lerp(uColor, uSecondaryColor, colorMix);

    // Overbright for bloom
    float3 finalColor = riftColor * pattern * uIntensity * uOverbrightMult;

    float alpha = baseTex.a * pattern * uOpacity * uIntensity;
    alpha = saturate(alpha);

    return float4(finalColor * alpha, alpha) * col;
}

// ------ GLOW PASS (wider, softer bloom halo) ------
float4 GravitationalRiftGlowPS(float2 uv : TEXCOORD0, float4 col : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float2 center = uv - 0.5;
    float dist = length(center);
    float angle = atan2(center.y, center.x);

    // Softer spiral for glow pass
    float spiralPhase = uTime * uScrollSpeed * 1.5;
    float spiralGlow = sin(angle * 3.0 + dist * 6.0 - spiralPhase) * 0.5 + 0.5;
    spiralGlow = pow(spiralGlow, 2.0);

    // Wider radial falloff
    float glowFalloff = saturate(1.0 - dist * 1.8);
    glowFalloff = pow(glowFalloff, 0.8);

    // Pulse with time
    float pulse = 0.85 + sin(uTime * 3.0) * 0.15;

    float pattern = glowFalloff * (0.5 + spiralGlow * 0.5) * pulse;

    // Color shifted slightly toward secondary for glow
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.4 + glowFalloff * 0.3);
    float3 finalColor = glowColor * pattern * uIntensity * uOverbrightMult * 0.6;

    float alpha = baseTex.a * pattern * uOpacity * uIntensity * 0.5;
    alpha = saturate(alpha);

    return float4(finalColor * alpha, alpha) * col;
}

// ------ TECHNIQUES ------
technique GravitationalRiftMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 GravitationalRiftMainPS();
    }
}

technique GravitationalRiftGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 GravitationalRiftGlowPS();
    }
}
