// =============================================================================
// Requiem of the Cosmos — Cosmic Requiem Beam Shader
// =============================================================================
// Intense channeled beam with building cosmic intensity.
// The ultimate magic beam — starts as a focused ray of starlight
// and builds into a roaring cosmic torrent. Nebula clouds swirl
// within the beam, with periodic star-burst flashes.
//
// Techniques:
//   CosmicRequiemBeam     – Main beam with nebula swirl
//   CosmicRequiemBeamGlow – Softer bloom pass
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uPhase;           // Channel intensity 0-1

float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Nebula swirl: rotating cloud pattern within the beam
float NebulaClouds(float2 uv, float time, float phase)
{
    float2 centered = uv - float2(0.5, 0.5);
    float angle = atan2(centered.y, centered.x);
    float dist = length(centered);
    
    // Swirling distortion increases with phase
    float swirl = sin(angle * 3.0 + dist * 8.0 - time * 2.0 * (1.0 + phase));
    float swirl2 = sin(angle * 5.0 - dist * 12.0 + time * 3.0);
    
    return (swirl * 0.6 + swirl2 * 0.4) * 0.5 + 0.5;
}

float4 CosmicRequiemBeamPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Increasing distortion with channel intensity
    float distMult = 1.0 + uPhase * 1.5;
    float wave1 = sin(coords.x * 8.0 + uTime * uScrollSpeed * 4.0) * uDistortionAmt * distMult;
    float wave2 = sin(coords.x * 16.0 - uTime * uScrollSpeed * 6.0) * uDistortionAmt * 0.4 * distMult;
    float wave3 = sin(coords.x * 24.0 + uTime * uScrollSpeed * 8.0) * uDistortionAmt * 0.15 * uPhase;
    
    float2 distortedUV = coords;
    distortedUV.y += wave1 + wave3;
    distortedUV.x += wave2;
    
    float4 baseTex = tex2D(uImage0, distortedUV);
    
    // Noise: cosmic nebula clouds
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll * 1.5;
    noiseUV.y += uTime * 0.25;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.6, noiseTex.r, uHasSecondaryTex);
    
    // Nebula swirl within the beam body
    float nebula = NebulaClouds(distortedUV, uTime, uPhase);
    
    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 0.95);
    
    // Gradient intensifies with phase
    float gradientT = coords.x * 0.5 + noiseVal * 0.3 + (1.0 - uPhase) * 0.2;
    float3 beamColor = lerp(uColor, uSecondaryColor, gradientT);
    
    // Nebula color variation
    float3 nebulaColor = lerp(uColor, float3(0.6, 0.3, 0.8), nebula * 0.3);
    beamColor = lerp(beamColor, nebulaColor, uPhase * 0.3);
    
    // White-hot core (widens with phase)
    float coreWidth = 0.5 + uPhase * 0.15;
    float coreMask = saturate((edgeFade - coreWidth) * (3.0 + uPhase * 2.0));
    beamColor = lerp(beamColor, float3(1.0, 0.97, 1.0), coreMask * (0.5 + uPhase * 0.3));
    
    // Star-burst flashes at peak intensity
    float burstFlash = sin(uTime * 10.0 + coords.x * 15.0) * uPhase;
    burstFlash = saturate(burstFlash * 3.0 - 2.0); // Only the peaks
    beamColor += float3(0.3, 0.2, 0.4) * burstFlash * coreMask;
    
    // Cosmic pulse
    float pulse = sin(uTime * 6.0 + coords.x * 5.0) * 0.06 * (0.5 + uPhase * 0.5) + 0.94;
    
    // Phase brightness
    float phaseBright = 0.6 + uPhase * 0.4;
    
    float3 finalColor = beamColor * baseTex.rgb * uIntensity * pulse * phaseBright;
    finalColor *= 0.55 + noiseVal * 0.45;
    
    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 CosmicRequiemBeamGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float wave = sin(coords.x * 4.0 + uTime * uScrollSpeed * 2.0) * uDistortionAmt * 0.4;
    float2 glowUV = coords;
    glowUV.y += wave;
    
    float4 baseTex = tex2D(uImage0, glowUV);
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.85);
    
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.4);
    
    float2 noiseUV = coords * uSecondaryTexScale * 0.5;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.4;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);
    
    glowColor *= uIntensity * noiseVal * baseTex.rgb * (0.7 + uPhase * 0.3);
    float pulse = sin(uTime * 2.5) * 0.06 + 0.94;
    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    
    return ApplyOverbright(glowColor, alpha);
}

technique CosmicRequiemBeam
{
    pass P0 { PixelShader = compile ps_3_0 CosmicRequiemBeamPS(); }
}

technique CosmicRequiemBeamGlow
{
    pass P0 { PixelShader = compile ps_3_0 CosmicRequiemBeamGlowPS(); }
}
