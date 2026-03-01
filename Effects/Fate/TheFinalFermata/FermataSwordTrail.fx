// =============================================================================
// The Final Fermata — Sword Trail Shader
// =============================================================================
// TEMPORAL FERMATA DISTORTION: Time-freeze aesthetic for orbiting spectral swords.
// Concentric resonance rings ripple outward like a held fermata note.
// UV warping creates a gravity-well / time-dilation visual. Chromatic color
// shifting based on the distortion field. The moment between notes, stretched
// into eternity.
// =============================================================================

sampler uImage0 : register(s0);

float uTime;
float4 uColor;
float uOpacity;
float uSaturation;
float uIntensity;

float4 uPrimaryColor;    // FermataPurple
float4 uSecondaryColor;  // FermataCrimson

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }

float SmoothNoise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// === MAIN TRAIL: Temporal distortion with concentric resonance ===

float4 PS_SwordTrailMain(float2 coords : TEXCOORD0) : COLOR0
{
    float4 texSample = tex2D(uImage0, coords);

    float along = coords.x;  // 0=tip, 1=tail
    float across = coords.y; // 0..1 cross-section
    float crossDist = abs(across - 0.5) * 2.0;

    // --- Concentric resonance rings emanating from the sword center ---
    // Rings propagate outward along the trail, like ripples from a fermata hold
    float ringFreq = 18.0;
    float ringSpeed = uTime * 4.0;
    float ringPhase = along * ringFreq - ringSpeed;
    float ring = sin(ringPhase) * 0.5 + 0.5;
    ring = pow(ring, 8.0); // Sharp bright rings with dark gaps
    
    // Rings modulated by cross-section (brightest at center)
    float ringMask = 1.0 - smoothstep(0.0, 0.6, crossDist);
    ring *= ringMask;

    // --- Temporal warp field: UV distortion creating gravity-well feel ---
    float warpStrength = 0.04 * (1.0 - along);
    float2 warpDir = float2(
        sin(along * 10.0 + uTime * 2.0) * warpStrength,
        cos(across * 8.0 + uTime * 1.5) * warpStrength * 0.5
    );
    float2 warpedUV = coords + warpDir;
    float warpedTex = tex2D(uImage0, saturate(warpedUV)).a;

    // --- Time dilation noise: frozen-in-time crackling ---
    float2 frozenUV = coords * float2(12.0, 5.0);
    // Time moves in discrete steps - the "frozen" feel of a fermata
    float frozenTime = floor(uTime * 2.0) * 0.5;
    float crackle = SmoothNoise(frozenUV + frozenTime);
    float timeCrack = smoothstep(0.6, 0.7, crackle) * (1.0 - crossDist);

    // --- Fermata symbol echo: the arc of the fermata blended into the trail ---
    // An arc shape near the midpoint of the trail
    float arcCenter = 0.3; // where along the trail the fermata arc appears
    float arcDist = abs(along - arcCenter);
    float arcCurve = 1.0 - smoothstep(0.0, 0.2, arcDist);
    float arcBow = abs(across - 0.5 - arcCurve * 0.15);
    float fermataArc = smoothstep(0.03, 0.0, arcBow - 0.12 * arcCurve) * arcCurve;
    // Dot of the fermata
    float dotDist = length(float2(along - arcCenter, across - 0.5));
    float fermataDot = smoothstep(0.04, 0.02, dotDist) * 0.8;

    // --- Chromatic color shift based on distortion field ---
    float distortionField = length(warpDir) * 25.0;
    float4 coolColor = uPrimaryColor;   // purple at low distortion
    float4 warmColor = uSecondaryColor; // crimson at high distortion
    float4 baseGradient = lerp(coolColor, warmColor, saturate(distortionField + along * 0.3));

    // White-hot where all rings converge
    float convergence = ring * (1.0 - along) * 0.6;
    float4 whiteHot = float4(1.0, 0.95, 0.98, 1.0);

    // --- Compose ---
    float4 color = baseGradient;
    color = lerp(color, whiteHot, convergence);
    color.rgb += uPrimaryColor.rgb * timeCrack * 1.5;
    color.rgb += float3(0.9, 0.85, 0.95) * fermataArc * 0.8;
    color.rgb += float3(1.0, 1.0, 1.0) * fermataDot;

    // Width fade
    float edgeFade = 1.0 - smoothstep(0.5, 0.85, crossDist);
    // Length fade
    float lengthFade = 1.0 - along * along;

    float alpha = edgeFade * lengthFade * texSample.a;
    alpha *= saturate(0.3 + ring * 0.4 + timeCrack * 0.2 + fermataArc * 0.1);
    alpha *= uOpacity;

    color.rgb *= uIntensity;
    color.a = alpha;
    color.rgb *= alpha;

    return color;
}

// === GLOW TRAIL: Temporal distortion aura ===

float4 PS_SwordTrailGlow(float2 coords : TEXCOORD0) : COLOR0
{
    float4 texSample = tex2D(uImage0, coords);

    float along = coords.x;
    float crossDist = abs(coords.y - 0.5) * 2.0;

    // Wide temporal glow with ring echo
    float glow = exp(-crossDist * crossDist * 3.0);

    // Faint concentric rings in the glow (echo of the main rings)
    float ringEcho = sin(along * 12.0 - uTime * 3.0) * 0.5 + 0.5;
    ringEcho = pow(ringEcho, 4.0) * glow * 0.4;

    // Pulsing with the fermata hold
    float holdPulse = sin(uTime * 1.5) * 0.15 + 0.85; // slow, held pulse

    // Time-freeze flicker
    float flicker = SmoothNoise(coords * float2(8.0, 3.0) + floor(uTime * 3.0) * 0.3);
    flicker = smoothstep(0.5, 0.6, flicker) * 0.2;

    float3 glowColor = lerp(float3(0.04, 0.02, 0.06), uPrimaryColor.rgb * 0.4, glow * 0.6);
    glowColor += uSecondaryColor.rgb * ringEcho;
    glowColor += float3(0.8, 0.7, 0.9) * flicker * glow;

    float alpha = glow * (1.0 - along) * holdPulse * uOpacity * 0.5;
    alpha *= texSample.a;

    float3 finalColor = glowColor * uIntensity * 1.3;

    return float4(finalColor * alpha, alpha);
}

technique SwordTrailMain
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_SwordTrailMain();
    }
}

technique SwordTrailGlow
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_SwordTrailGlow();
    }
}
