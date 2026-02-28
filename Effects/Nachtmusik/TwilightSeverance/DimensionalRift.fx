// =============================================================================
// Twilight Severance — Dimensional Rift Slash Shader
// =============================================================================
// Ultra-fast katana trails that leave behind shimmering dimensional tears.
// The boundary between dusk and dawn is severed with each cut,
// creating a sharp-edged trail with flickering twilight colors
// and clean silver edge highlights.
//
// Techniques:
//   DimensionalRiftSlash – Main sharp-edged trail with rift shimmer
//   DimensionalRiftGlow  – Softer bloom pass
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

// Dimensional rift: sharp-edged tear effect
float DimensionalTear(float2 uv, float time)
{
    // Sharp center line (the cut through reality)
    float centerDist = abs(uv.y - 0.5);
    float tearLine = exp(-centerDist * centerDist * 200.0);
    
    // Rippling edges from the tear
    float ripple = sin(uv.x * 20.0 + time * 8.0) * 0.02;
    float edgeRipple = exp(-(centerDist - 0.15 + ripple) * (centerDist - 0.15 + ripple) * 80.0);
    
    return tearLine * 0.6 + edgeRipple * 0.4;
}

float4 DimensionalRiftSlashPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Sharp, fast distortion (katana precision)
    float cut1 = sin(coords.x * 14.0 + uTime * uScrollSpeed * 8.0) * uDistortionAmt * 0.7;
    float cut2 = sin(coords.x * 22.0 - uTime * uScrollSpeed * 5.0) * uDistortionAmt * 0.3;
    
    float2 distortedUV = coords;
    distortedUV.y += cut1;
    distortedUV.x += cut2;
    
    float4 baseTex = tex2D(uImage0, distortedUV);
    
    // Noise
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll * 1.5;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.65, noiseTex.r, uHasSecondaryTex);
    
    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 1.15); // Faster fade (sharp)
    
    // Gradient: dusk violet → golden dawn
    float gradientT = coords.x * 0.7 + noiseVal * 0.3;
    float3 slashColor = lerp(uColor, uSecondaryColor, gradientT);
    
    // Bright silver core (the razor edge)
    float coreMask = saturate((edgeFade - 0.55) * 5.0); // Sharper than normal
    slashColor = lerp(slashColor, float3(0.92, 0.90, 1.0), coreMask * 0.65);
    
    // Dimensional tear highlight
    float tear = DimensionalTear(coords, uTime * uScrollSpeed);
    slashColor = lerp(slashColor, float3(1.0, 0.98, 1.0), tear * 0.4);
    
    // Twilight flicker: rapid oscillation between dusk and dawn colors
    float flicker = sin(uTime * 12.0 + coords.x * 8.0) * 0.5 + 0.5;
    float3 duskColor = uColor;
    float3 dawnColor = uSecondaryColor;
    float3 flickerBlend = lerp(duskColor, dawnColor, flicker * 0.2);
    slashColor = lerp(slashColor, flickerBlend, 0.15);
    
    // Speed shimmer (very rapid pulse for katana feel)
    float pulse = sin(uTime * 15.0 + coords.x * 12.0) * 0.05 + 0.95;
    
    float3 finalColor = slashColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.6 + noiseVal * 0.4;
    
    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 DimensionalRiftGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.x * 8.0 + uTime * uScrollSpeed * 4.0) * uDistortionAmt * 0.3;
    float2 glowUV = coords;
    glowUV.y += wave;
    
    float4 baseTex = tex2D(uImage0, glowUV);
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.95);
    
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.4);
    
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.6;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.4);
    
    glowColor *= uIntensity * noiseVal * baseTex.rgb;
    float pulse = sin(uTime * 3.5 + coords.x * 4.0) * 0.06 + 0.94;
    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    
    return ApplyOverbright(glowColor, alpha);
}

technique DimensionalRiftSlash
{
    pass P0 { PixelShader = compile ps_3_0 DimensionalRiftSlashPS(); }
}

technique DimensionalRiftGlow
{
    pass P0 { PixelShader = compile ps_3_0 DimensionalRiftGlowPS(); }
}
