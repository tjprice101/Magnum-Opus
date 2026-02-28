// =============================================================================
// Celestial Chorus Baton — Chorus Summon Aura Shader
// =============================================================================
// Musical summoning aura — concentric rings of music notes orbiting the
// NocturnalGuardian minion. Creates a visible "choir" of floating note
// symbols that harmonize visually.
//
// UV Layout: Centered at (0.5, 0.5) — radial coordinate system
//
// Techniques:
//   ChorusSummonAura – Musical note constellation aura
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

// Music note symbol approximation via math (treble clef shape)
float MusicNoteShape(float2 uv)
{
    // Stem
    float stem = exp(-abs(uv.x) * 80.0) * step(-0.15, uv.y) * step(uv.y, 0.2);
    
    // Note head (tilted ellipse)
    float2 headPos = uv - float2(0.02, -0.15);
    float headAngle = 0.3;
    float2 rotHead = float2(
        headPos.x * cos(headAngle) + headPos.y * sin(headAngle),
        -headPos.x * sin(headAngle) + headPos.y * cos(headAngle)
    );
    float head = 1.0 - smoothstep(0.0, 0.035, length(rotHead * float2(1.0, 1.8)));
    
    return saturate(stem * 0.5 + head);
}

float4 ChorusSummonAuraPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 centered = coords - float2(0.5, 0.5);
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    
    float4 baseTex = tex2D(uImage0, coords);
    
    // Noise background
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll * 0.1;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.6, noiseTex.r, uHasSecondaryTex);
    
    // Inner ring of music notes (6 notes, rotating clockwise)
    float innerNotes = 0.0;
    for (int i = 0; i < 6; i++)
    {
        float noteAngle = 6.28318 * (float)i / 6.0 + uTime * uScrollSpeed * 0.8;
        float2 notePos = float2(cos(noteAngle), sin(noteAngle)) * 0.18;
        float2 relUV = (centered - notePos) * 12.0;
        innerNotes += MusicNoteShape(relUV) * 0.7;
    }
    
    // Outer ring of music notes (8 notes, rotating counter-clockwise)
    float outerNotes = 0.0;
    for (int j = 0; j < 8; j++)
    {
        float noteAngle2 = 6.28318 * (float)j / 8.0 - uTime * uScrollSpeed * 0.5;
        float2 notePos2 = float2(cos(noteAngle2), sin(noteAngle2)) * 0.32;
        float2 relUV2 = (centered - notePos2) * 14.0;
        outerNotes += MusicNoteShape(relUV2) * 0.5;
    }
    
    // Harmonic rings (concentric circles pulsing outward)
    float ring1 = exp(-(dist - 0.18) * (dist - 0.18) * 200.0);
    float ring2 = exp(-(dist - 0.32) * (dist - 0.32) * 150.0);
    float ringPulse = sin(uTime * 3.0) * 0.3 + 0.7;
    float rings = (ring1 + ring2) * ringPulse * 0.3;
    
    float pattern = innerNotes + outerNotes + rings;
    pattern *= noiseVal * 0.4 + 0.6;
    
    // Radial falloff
    float falloff = exp(-dist * dist * 6.0);
    pattern *= falloff;
    
    // Color: indigo-to-gold gradient based on radius
    float colorT = saturate(dist * 3.0);
    float3 auraColor = lerp(uSecondaryColor, uColor, colorT);
    
    // Notes glow brighter
    float noteHighlight = saturate(innerNotes + outerNotes);
    float3 noteColor = float3(0.95, 0.9, 1.0);
    auraColor = lerp(auraColor, noteColor, noteHighlight * 0.4);
    
    float breathe = sin(uTime * 2.0) * 0.1 * uPhase + 0.9;
    float alpha = pattern * uOpacity * breathe * sampleColor.a * baseTex.a;
    float3 finalColor = auraColor * uIntensity * baseTex.rgb;
    
    return ApplyOverbright(finalColor, alpha);
}

technique ChorusSummonAura
{
    pass P0 { PixelShader = compile ps_3_0 ChorusSummonAuraPS(); }
}
