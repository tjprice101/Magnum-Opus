// =============================================================================
// Galactic Overture — Overture Aura Shader
// =============================================================================
// Sweeping orchestral aura for the CelestialMuse minion — broad, 
// dramatic waves of starlight that rise and fall like an orchestral
// crescendo. Layered wave patterns create visual "music" around the minion.
//
// UV Layout: Centered at (0.5, 0.5) — radial
//
// Techniques:
//   OvertureAura – Orchestral wave aura
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

float OrchestraWave(float2 centered, float time)
{
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    // Multiple wave fronts radiating outward at different speeds
    float wave1 = sin(dist * 40.0 - time * 4.0 + angle * 2.0) * 0.5 + 0.5;
    wave1 *= exp(-dist * 8.0);
    
    float wave2 = sin(dist * 30.0 - time * 3.0 - angle * 3.0) * 0.5 + 0.5;
    wave2 *= exp(-dist * 6.0);
    
    float wave3 = sin(dist * 20.0 - time * 5.0) * 0.5 + 0.5;
    wave3 *= exp(-dist * 10.0);
    
    // Combine with orchestral layering
    return wave1 * 0.4 + wave2 * 0.35 + wave3 * 0.25;
}

float4 OvertureAuraPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // Noise for organic texture
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += float2(uTime * uSecondaryTexScroll * 0.2, -uTime * uSecondaryTexScroll * 0.15);
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.6, noiseTex.r, uHasSecondaryTex);
    
    // Orchestral wave pattern
    float waves = OrchestraWave(centered, uTime * uScrollSpeed);
    
    // Musical staff lines (horizontal arcs around the minion)
    float staffAngle = angle + uTime * uScrollSpeed * 0.4;
    float staffLines = 0.0;
    for (int i = 0; i < 5; i++)
    {
        float lineRadius = 0.12 + (float)i * 0.06;
        float lineDist = abs(dist - lineRadius);
        float line = exp(-lineDist * lineDist * 800.0);
        
        // Staff lines wave gently
        float lineWave = sin(angle * 4.0 + uTime * uScrollSpeed * 2.0 + (float)i * 1.2) * 0.01;
        float lineDistWaved = abs(dist - lineRadius - lineWave);
        line = exp(-lineDistWaved * lineDistWaved * 800.0);
        
        staffLines += line * 0.4;
    }
    
    float pattern = waves * 0.6 + staffLines * 0.4;
    pattern *= noiseVal * 0.3 + 0.7;
    
    // Radial falloff — wide dramatic aura
    float falloff = exp(-dist * dist * 5.0);
    pattern *= falloff;
    
    // Color: deep cosmic blue base with silver highlights where waves peak
    float3 baseColor = uColor;
    float3 peakColor = uSecondaryColor;
    float colorT = waves;
    float3 auraColor = lerp(baseColor, peakColor, colorT);
    
    // Staff lines are bright silver
    float3 silverLine = float3(0.9, 0.92, 1.0);
    auraColor = lerp(auraColor, silverLine, staffLines * 0.6);
    
    // Crescendo effect — everything brightens with phase
    float phaseBright = 0.4 + uPhase * 0.6;
    float breathe = sin(uTime * 1.8) * 0.08 + 0.92;
    
    float alpha = pattern * uOpacity * phaseBright * breathe * sampleColor.a * baseTex.a;
    float3 finalColor = auraColor * uIntensity * baseTex.rgb;
    
    return ApplyOverbright(finalColor, alpha);
}

technique OvertureAura
{
    pass P0 { PixelShader = compile ps_3_0 OvertureAuraPS(); }
}
