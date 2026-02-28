// =============================================================================
// Conductor of Constellations — Stellar Conductor Aura Shader
// =============================================================================
// Grand orbiting constellation ring that surrounds the Stellar Conductor.
// Constellations slowly rotate around the minion, with bright star-points
// connected by faint lines, creating a living star map.
//
// UV Layout: Centered at (0.5, 0.5) — radial coordinate system
//
// Techniques:
//   StellarConductorAura – Orbiting constellation ring aura
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uPhase;

float uOverbrightMult;
float uScrollSpeed;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Constellation line pattern: radial lines connecting star points
float ConstellationWeb(float2 centered, float time)
{
    float angle = atan2(centered.y, centered.x);
    float dist = length(centered);
    
    // Star points at specific angular positions (rotating)
    float rotAngle = angle - time * 0.3;
    float starPattern = sin(rotAngle * 6.0) * sin(rotAngle * 4.0);
    starPattern = abs(starPattern);
    starPattern = pow(starPattern, 4.0);
    
    // Radial connecting lines
    float radialLines = abs(sin(rotAngle * 8.0));
    radialLines = pow(radialLines, 12.0); // Very thin lines
    
    // Ring structure
    float ring1 = exp(-(dist - 0.25) * (dist - 0.25) * 100.0);
    float ring2 = exp(-(dist - 0.35) * (dist - 0.35) * 80.0);
    
    return starPattern * 0.4 + radialLines * ring1 * 0.3 + ring2 * 0.3;
}

float4 StellarConductorAuraPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // Constellation web pattern
    float web = ConstellationWeb(centered, uTime * uScrollSpeed);
    
    // Noise for nebula backdrop
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.2;
    noiseUV.y -= uTime * uSecondaryTexScroll * 0.15;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.7, noiseTex.r, uHasSecondaryTex);
    
    web *= noiseVal;
    
    // Radial falloff
    float innerCutout = saturate(dist * 5.0);
    float outerFalloff = exp(-dist * dist * 7.0);
    float radialMask = innerCutout * outerFalloff;
    
    // Color: inner gold (conductor's baton), outer cosmic blue
    float colorT = saturate(dist * 2.5);
    float3 auraColor = lerp(uSecondaryColor, uColor, colorT);
    
    // Star-point highlights
    float3 starHighlight = float3(0.98, 0.96, 1.0);
    auraColor = lerp(auraColor, starHighlight, web * 0.5);
    
    // Rotation glow (brighter in direction of rotation)
    float rotGlow = sin(angle * 2.0 - uTime * uScrollSpeed * 1.5) * 0.5 + 0.5;
    auraColor *= 0.8 + rotGlow * 0.2;
    
    // Conductor's pulse (grand, commanding)
    float breathe = sin(uTime * 2.0) * 0.1 * uPhase + 0.9;
    float phaseBrightness = 0.3 + uPhase * 0.7;
    
    float3 finalColor = auraColor * uIntensity * phaseBrightness * breathe * baseTex.rgb;
    float alpha = web * radialMask * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

technique StellarConductorAura
{
    pass P0 { PixelShader = compile ps_3_0 StellarConductorAuraPS(); }
}
