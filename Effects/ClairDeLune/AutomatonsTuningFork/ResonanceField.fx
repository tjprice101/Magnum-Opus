// ============================================================
//  ResonanceField.fx — Automaton's Tuning Fork (Summon Whip)
//  Clair de Lune — "Harmonic Resonance Pulse"
//
//  The tuning fork strikes create expanding waves of harmonic
//  resonance — concentric rings of soft blue energy that pulse
//  outward, with standing wave interference patterns where
//  they overlap.
//  Two techniques: ResonanceFieldPulse, ResonanceFieldHarmonic
// ============================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4 uColor;
float4 uSecondaryColor;
float  uOpacity;
float  uTime;
float  uIntensity;
float  uOverbrightMult;
float  uScrollSpeed;
float  uDistortionAmt;
bool   uHasSecondaryTex;
float  uSecondaryTexScale;
float2 uSecondaryTexScroll;

float4 ResonanceFieldPulsePS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float2 center = float2(0.5, 0.5);
    float dist = length(uv - center) * 2.0;

    // Expanding concentric pulse rings
    float ringPhase = dist * 8.0 - uTime * uScrollSpeed * 3.0;
    float rings = pow(max(sin(ringPhase), 0), 4.0);
    float ringFade = exp(-dist * dist * 1.5); // Fade with distance

    // Standing wave interference — abs() reveals ALL interference nodes,
    // not just positive ones. Two counterpropagating waves at different
    // frequencies create a visible moiré of reinforcement nodes.
    float interference = sin(dist * 16.0 - uTime * uScrollSpeed * 6.0) *
                         sin(dist * 12.0 + uTime * uScrollSpeed * 2.0);
    interference = abs(interference) * ringFade * 0.35;

    // Harmonic shimmer from noise
    float harmonic = 0.5;
    if (uHasSecondaryTex)
    {
        float2 harmUV = uv * uSecondaryTexScale + float2(0, -uTime * 0.1);
        harmonic = tex2D(uImage1, harmUV).r;
    }

    // Soft blue core → pearl white rings → faint dream haze edges
    float3 softBlue = uColor.rgb;
    float3 pearlWhite = uSecondaryColor.rgb;
    float3 dreamHaze = float3(0.47, 0.59, 0.82); // DreamHaze

    float3 color = softBlue * rings * ringFade * 0.6;
    color += pearlWhite * interference * 0.5;
    color += dreamHaze * harmonic * ringFade * 0.15;
    color += pearlWhite * exp(-dist * dist * 8.0) * 0.3; // Core glow

    float3 finalColor = color * uIntensity;
    finalColor *= (1.0 + uOverbrightMult * ringFade * 0.3);

    float alpha = base.a * uOpacity * saturate(rings * ringFade + interference + exp(-dist * 4.0) * 0.3);

    return float4(finalColor, alpha);
}

float4 ResonanceFieldHarmonicPS(float2 uv : TEXCOORD0) : COLOR0
{
    float4 base = tex2D(uImage0, uv);

    float dist = length(uv - 0.5) * 2.0;

    // Harmonic overtone pattern — true musical frequency ratios
    // Fundamental (root), octave (2x freq), perfect fifth (1.5x freq)
    float fundamental = sin(dist * 6.0 - uTime * 2.0);
    float octave = sin(dist * 12.0 - uTime * 4.0) * 0.5;
    float fifth = sin(dist * 9.0 - uTime * 3.0) * 0.3;
    // Use abs() to show all harmonic nodes — both constructive & destructive
    float harmonics = abs(fundamental + octave + fifth);
    harmonics = pow(harmonics / 1.8, 2.0) * exp(-dist * 1.5);

    // Visible standing wave nodes at interference peaks
    float nodePattern = pow(abs(sin(dist * 6.0)), 8.0); // Tight peaks at nodes
    float nodes = nodePattern * exp(-dist * 2.0) * 0.15;

    float3 harmColor = lerp(uColor.rgb, uSecondaryColor.rgb, harmonics * 0.5) * uIntensity * uOverbrightMult * 0.6;
    harmColor += uSecondaryColor.rgb * nodes * uIntensity; // Accent at nodes
    float alpha = base.a * uOpacity * (harmonics * 0.35 + nodes);

    return float4(harmColor, alpha);
}

technique ResonanceFieldPulse
{
    pass P0 { PixelShader = compile ps_3_0 ResonanceFieldPulsePS(); }
}

technique ResonanceFieldHarmonic
{
    pass P0 { PixelShader = compile ps_3_0 ResonanceFieldHarmonicPS(); }
}
