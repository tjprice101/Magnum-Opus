// =============================================================================
// Starweaver's Grimoire — Constellation Weave Shader
// =============================================================================
// Constellation-pattern magic orb shader — star points slowly connect via
// bright lines as the charge builds. Creates a living star map that completes
// itself, then bursts when the constellation is finished.
//
// UV Layout: Centered at (0.5, 0.5) — radial for orb
//
// Techniques:
//   ConstellationWeaveOrb – Star map pattern orb
//   ConstellationWeaveGlow – Astral glow halo
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uPhase; // 0..1 charge level — constellation completeness

float uOverbrightMult;
float uScrollSpeed;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Generate fixed star positions on a circle
float2 StarPosition(int index, int total, float radius)
{
    float angle = 6.28318 * (float)index / (float)total;
    return float2(cos(angle), sin(angle)) * radius;
}

// Distance from a point to a line segment
float DistToSegment(float2 p, float2 a, float2 b)
{
    float2 pa = p - a;
    float2 ba = b - a;
    float t = saturate(dot(pa, ba) / dot(ba, ba));
    return length(pa - ba * t);
}

float4 ConstellationWeaveOrbPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // Noise backdrop
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += float2(uTime * uSecondaryTexScroll * 0.15, -uTime * uSecondaryTexScroll * 0.1);
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.5, noiseTex.r, uHasSecondaryTex);
    
    // 7-star constellation (musical note: 7 notes in a scale)
    float starField = 0.0;
    float lineField = 0.0;
    float constRadius = 0.28;
    
    // Draw star points
    for (int i = 0; i < 7; i++)
    {
        float2 sp = StarPosition(i, 7, constRadius);
        float d = length(centered - sp);
        
        // Star pulse — each star twinkles at its own rate
        float twinkle = sin(uTime * uScrollSpeed * 3.0 + (float)i * 1.5) * 0.3 + 0.7;
        float starBright = exp(-d * d * 800.0) * twinkle;
        
        // Stars only appear as charge builds
        float starThreshold = (float)i / 7.0;
        starBright *= smoothstep(starThreshold - 0.05, starThreshold + 0.05, uPhase);
        
        starField += starBright;
    }
    
    // Draw connecting lines between adjacent stars (and some cross-links)
    for (int j = 0; j < 7; j++)
    {
        int next = (j + 1) % 7;
        float2 a = StarPosition(j, 7, constRadius);
        float2 b = StarPosition(next, 7, constRadius);
        
        float d = DistToSegment(centered, a, b);
        float lineBright = exp(-d * d * 3000.0);
        
        // Lines draw themselves progressively with charge
        float lineThreshold = ((float)j + 0.5) / 7.0;
        lineBright *= smoothstep(lineThreshold - 0.05, lineThreshold + 0.05, uPhase);
        
        lineField += lineBright * 0.6;
    }
    
    // Cross-links for constellation complexity (every other)
    for (int k = 0; k < 7; k += 2)
    {
        float2 a2 = StarPosition(k, 7, constRadius);
        float2 b2 = StarPosition((k + 3) % 7, 7, constRadius);
        
        float d2 = DistToSegment(centered, a2, b2);
        float crossBright = exp(-d2 * d2 * 2000.0);
        crossBright *= smoothstep(0.7, 0.9, uPhase); // Only at high charge
        
        lineField += crossBright * 0.3;
    }
    
    // Central glow (grows with charge)
    float centralGlow = exp(-dist * dist * 20.0) * uPhase * 0.5;
    
    // Radial falloff
    float orbFalloff = exp(-dist * dist * 6.0);
    
    float pattern = (starField + lineField + centralGlow) * orbFalloff;
    pattern *= noiseVal * 0.4 + 0.6;
    
    // Color: silver-blue constellation lines, gold star points
    float3 lineColor = uColor; // Deep indigo
    float3 starColor = uSecondaryColor; // Gold
    float3 finalC = lerp(lineColor, starColor, starField / max(starField + lineField, 0.01));
    
    // At full charge, everything brightens to white-gold
    float fullChargePulse = smoothstep(0.9, 1.0, uPhase);
    float3 burstColor = float3(1.0, 0.95, 0.8);
    finalC = lerp(finalC, burstColor, fullChargePulse * 0.6);
    
    float alpha = pattern * uOpacity * sampleColor.a * baseTex.a;
    float3 finalColor = finalC * uIntensity * baseTex.rgb;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 ConstellationWeaveGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // Soft astral halo
    float glow = exp(-dist * dist * 4.0);
    float pulse = sin(uTime * 2.5) * 0.15 * uPhase + 0.85;
    
    float alpha = glow * uOpacity * 0.35 * uPhase * sampleColor.a * baseTex.a * pulse;
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.4) * uIntensity * 0.5;
    
    return ApplyOverbright(glowColor * baseTex.rgb, alpha);
}

technique ConstellationWeaveOrb
{
    pass P0 { PixelShader = compile ps_3_0 ConstellationWeaveOrbPS(); }
}

technique ConstellationWeaveGlow
{
    pass P0 { PixelShader = compile ps_3_0 ConstellationWeaveGlowPS(); }
}
