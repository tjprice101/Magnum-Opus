// =============================================================================
// Constellation Piercer — Star Chain Beam Shader
// =============================================================================
// Sharp, precise beam trail for bullet projectiles that creates
// constellation-line effects between chained enemies.
// Clean starlit energy with precision pinpoint highlights.
//
// Techniques:
//   StarChainBeam     – Main bullet trail with constellation points
//   StarChainBeamGlow – Softer bloom pass
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

// Constellation point pattern
float ConstellationPoints(float2 uv, float time)
{
    float2 cell = floor(uv * 5.0);
    float hash = frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
    
    float2 cellCenter = (cell + 0.5) / 5.0;
    float dist = length(uv - cellCenter) * 5.0;
    
    // Bright point stars
    float star = exp(-dist * dist * 20.0);
    float twinkle = sin(hash * 6.28 + time * (3.0 + hash * 4.0)) * 0.5 + 0.5;
    
    return star * twinkle * step(0.6, hash);
}

float4 StarChainBeamPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Precision distortion (minimal, clean)
    float wave1 = sin(coords.x * 8.0 + uTime * uScrollSpeed * 4.0) * uDistortionAmt * 0.5;
    
    float2 distortedUV = coords;
    distortedUV.y += wave1;
    
    float4 baseTex = tex2D(uImage0, distortedUV);
    
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll * 1.5;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.75, noiseTex.r, uHasSecondaryTex);
    
    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 1.2); // Fast fade for bullet trails
    
    // Cool stellar gradient
    float gradientT = coords.x * 0.8 + noiseVal * 0.2;
    float3 beamColor = lerp(uColor, uSecondaryColor, gradientT);
    
    // Bright white core (precision laser)
    float coreMask = saturate((edgeFade - 0.6) * 5.0);
    beamColor = lerp(beamColor, float3(0.98, 0.96, 1.0), coreMask * 0.7);
    
    // Constellation star points along the beam
    float stars = ConstellationPoints(distortedUV * 2.0, uTime);
    beamColor = lerp(beamColor, float3(1.0, 0.98, 0.95), stars * 0.5);
    
    // Clean precision pulse
    float pulse = sin(uTime * 6.0 + coords.x * 8.0) * 0.04 + 0.96;
    
    float3 finalColor = beamColor * baseTex.rgb * uIntensity * pulse;
    finalColor *= 0.7 + noiseVal * 0.3;
    
    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 StarChainBeamGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.x * 5.0 + uTime * uScrollSpeed * 2.0) * uDistortionAmt * 0.3;
    float2 glowUV = coords;
    glowUV.y += wave;
    
    float4 baseTex = tex2D(uImage0, glowUV);
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.85);
    
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.5);
    
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.5;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.9, noiseTex.r, uHasSecondaryTex * 0.4);
    
    glowColor *= uIntensity * noiseVal * baseTex.rgb;
    float pulse = sin(uTime * 3.0) * 0.05 + 0.95;
    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    
    return ApplyOverbright(glowColor, alpha);
}

technique StarChainBeam
{
    pass P0 { PixelShader = compile ps_3_0 StarChainBeamPS(); }
}

technique StarChainBeamGlow
{
    pass P0 { PixelShader = compile ps_3_0 StarChainBeamGlowPS(); }
}
