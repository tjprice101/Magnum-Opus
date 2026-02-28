// =============================================================================
// Nocturnal Executioner — Execution Decree Slash Shader
// =============================================================================
// Heavy, commanding slash arc with void-ripping visual.
// The executioner's blade tears through the night sky itself,
// leaving behind a rift of swirling cosmic purple and regal gold.
//
// UV Layout: Trail primitive (U = along trail, V = across width)
//
// Techniques:
//   ExecutionDecreeSlash – Main slash with void-rip distortion
//   ExecutionDecreeGlow  – Softer bloom stacking pass
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Void-rip distortion: aggressive tear effect at the blade edge
float VoidRip(float2 uv, float time)
{
    float ripple = sin(uv.x * 16.0 + time * 6.0) * sin(uv.y * 8.0 - time * 3.0);
    float tear = abs(uv.y - 0.5) * 4.0; // Distance from center
    tear = saturate(1.0 - tear);
    return ripple * tear * 0.5;
}

float4 ExecutionDecreeSlashPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Heavy void-tear distortion
    float rip1 = sin(coords.x * 10.0 + uTime * uScrollSpeed * 5.0) * uDistortionAmt * 1.2;
    float rip2 = sin(coords.x * 18.0 - uTime * uScrollSpeed * 3.0) * uDistortionAmt * 0.6;
    float rip3 = VoidRip(coords, uTime * uScrollSpeed);

    float2 distortedUV = coords;
    distortedUV.y += rip1 + rip3 * uDistortionAmt;
    distortedUV.x += rip2;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // Noise: cosmic nebula variation
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll * 1.2;
    noiseUV.y += uTime * 0.2;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.6, noiseTex.r, uHasSecondaryTex);

    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 1.1);

    // Gradient: deep void at edges → gold decree at core
    float gradientT = coords.x * 0.6 + noiseVal * 0.4;
    float3 slashColor = lerp(uColor, uSecondaryColor, gradientT);

    // White-hot execution core
    float coreMask = saturate((edgeFade - 0.5) * 4.0);
    slashColor = lerp(slashColor, float3(1.0, 0.95, 0.85), coreMask * 0.6);

    // Void-rip edge glow: bright purple at the torn edges
    float edgeMask = saturate((0.5 - edgeFade) * 3.0);
    float edgeFlicker = saturate(rip1 * 5.0 + 0.5);
    slashColor += float3(0.3, 0.1, 0.5) * edgeMask * edgeFlicker * 0.4;

    // Cosmic energy cracks in the void
    float crackMask = saturate(noiseVal * 2.5 - 1.2) * coreMask;
    slashColor += float3(0.4, 0.35, 0.15) * crackMask;

    // Aggressive execution pulse
    float pulse = sin(uTime * 8.0 + coords.x * 6.0) * 0.08 + 0.92;
    pulse *= sin(uTime * 5.0 + coords.x * 10.0) * 0.04 + 0.96;

    float3 finalColor = slashColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.55 + noiseVal * 0.45;

    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

float4 ExecutionDecreeGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.x * 6.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt * 0.5;
    float2 glowUV = coords;
    glowUV.y += wave;

    float4 baseTex = tex2D(uImage0, glowUV);
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.85);

    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.35);
    glowColor = lerp(glowColor, float3(0.2, 0.08, 0.35), 0.12);

    float2 noiseUV = coords * uSecondaryTexScale * 0.6;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.6;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.8, noiseTex.r, uHasSecondaryTex * 0.5);

    glowColor *= uIntensity * noiseVal * baseTex.rgb;
    float pulse = sin(uTime * 3.0 + coords.x * 3.0) * 0.08 + 0.92;
    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;

    return ApplyOverbright(glowColor, alpha);
}

technique ExecutionDecreeSlash
{
    pass P0 { PixelShader = compile ps_3_0 ExecutionDecreeSlashPS(); }
}

technique ExecutionDecreeGlow
{
    pass P0 { PixelShader = compile ps_3_0 ExecutionDecreeGlowPS(); }
}
