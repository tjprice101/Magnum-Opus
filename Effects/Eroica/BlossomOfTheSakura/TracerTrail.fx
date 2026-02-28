// =============================================================================
// Tracer Trail Shader - Heat-Reactive Assault Rifle Tracers
// =============================================================================
// Barrel-rifling spiral tracers with supersonic shockwave cone for
// Blossom of the Sakura assault rifle. Each bullet leaves a spinning
// helix trail that intensifies with barrel heat.
//
// VISUAL IDENTITY: Military precision meets sakura fire -- tight rifling
// spirals at low heat, chaotic heat-warped helices at high heat, with a
// supersonic shockwave cone visible at the bullet's tip.
//
// Techniques:
//   TracerTrailMain  - Spiral rifling tracer with heat-reactive distortion
//   TracerTrailGlow  - Soft bloom with heat-reactive color halo
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Sakura pink at low heat)
float3 uSecondaryColor;  // Secondary color (Gold at high heat)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uHeatLevel;         // 0 = cool, 1 = overheated
float uScrollSpeed;       // Trail flow speed
float uDistortionAmt;     // Ember turbulence
float uNoiseScale;        // Noise UV repetition
float uHasSecondaryTex;   // 1.0 if noise texture bound

// =============================================================================
// VERTEX SHADER
// =============================================================================

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(VertexShaderInput input)
{
    VertexShaderOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

// =============================================================================
// UTILITY
// =============================================================================

float HashNoise(float2 p)
{
    return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
}

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: TRACER TRAIL - Rifling Spiral & Shockwave
// =============================================================================

float4 TracerTrailMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Barrel rifling spiral pattern ---
    // Spiral displacement that tightens with position along trail
    float spiralFreq = 30.0 + uHeatLevel * 15.0; // Tighter spiral when hot
    float spiralPhase = coords.x * spiralFreq - uTime * uScrollSpeed * 12.0;
    float spiral = sin(spiralPhase) * (0.3 + uHeatLevel * 0.25) * uDistortionAmt;

    // Second spiral strand (double-helix for rifling grooves)
    float spiral2 = sin(spiralPhase + 3.14159) * (0.2 + uHeatLevel * 0.15) * uDistortionAmt;

    // Heat adds chaotic jitter to the spiral
    float heatJitter = sin(coords.x * 60.0 + uTime * 20.0) * uDistortionAmt * uHeatLevel * uHeatLevel * 0.5;

    float2 distortedUV = coords;
    distortedUV.y += spiral + heatJitter;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Razor-thin core with rifling groove brightness modulation ---
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 3.0);

    // Rifling grooves: spiral modulates brightness across trail width
    float grooveMask = sin(spiralPhase + coords.y * 8.0) * 0.5 + 0.5;
    grooveMask = pow(grooveMask, 2.0) * 0.3;

    // --- Trail length fade ---
    float trailFade = pow(1.0 - coords.x, 2.0);

    // --- Supersonic shockwave cone at bullet tip ---
    // V-shaped brightness increase near trail head (coords.x near 0)
    float tipDist = coords.x; // 0 at tip
    float coneAngle = abs(coords.y - 0.5) * 2.0; // 0 at center, 1 at edge
    float coneMask = saturate(1.0 - tipDist * 8.0) * saturate(1.0 - abs(coneAngle - tipDist * 2.0) * 6.0);
    coneMask *= (0.3 + uHeatLevel * 0.7); // More visible at high heat

    // --- Heat-reactive colour shift ---
    float3 tracerColor = lerp(uColor, uSecondaryColor, uHeatLevel);
    float3 hotWhite = float3(1.0, 0.96, 0.88);
    tracerColor = lerp(tracerColor, hotWhite, uHeatLevel * uHeatLevel * 0.5);

    // White-hot core
    float coreMask = pow(edgeFade, 4.0) * trailFade;
    tracerColor = lerp(tracerColor, hotWhite, coreMask * 0.6);

    // Rifling groove brightness (second spiral strand is slightly different color)
    float3 grooveColor = lerp(tracerColor, uSecondaryColor, 0.3);
    // Second helix as a dimmer ghost strand
    float strand2Mask = saturate(sin(spiralPhase + 3.14159 + coords.y * 8.0) * 0.5 + 0.2);
    strand2Mask *= saturate(0.7 - edgeFade) * uHeatLevel * 0.4;

    // --- Ember sparks at high heat ---
    float2 sparkUV = float2(coords.x * 8.0 - uTime * uScrollSpeed * 3.0, coords.y * 4.0);
    float sparkNoise = HashNoise(sparkUV * uNoiseScale);
    float sparks = saturate(sparkNoise * 3.0 - 2.0) * uHeatLevel * uHeatLevel;

    // Optional noise texture
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.x -= uTime * uScrollSpeed;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(sparkNoise, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Rapid flicker at high heat ---
    float flicker = sin(uTime * 15.0 + coords.x * 12.0) * 0.04 * uHeatLevel + 1.0;

    // --- Final composition ---
    float3 finalColor = tracerColor * baseTex.rgb * uIntensity * flicker;
    finalColor += grooveColor * grooveMask * baseTex.rgb * uIntensity;
    finalColor += tracerColor * strand2Mask * baseTex.rgb * uIntensity * 0.5;
    finalColor += uSecondaryColor * sparks * 0.3;
    finalColor += hotWhite * coneMask * uIntensity * 0.5; // Shockwave cone
    finalColor *= 0.7 + noiseVal * 0.3;

    float alpha = (coreFade * trailFade + grooveMask * edgeFade * 0.3 + sparks * 0.15 + coneMask * 0.2) * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: TRACER TRAIL GLOW
// =============================================================================

float4 TracerTrailGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.5);
    float trailFade = pow(1.0 - coords.x, 1.5);

    // Heat-reactive glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, uHeatLevel * 0.7);
    float3 warmTint = float3(1.0, 0.7, 0.4);
    glowColor = lerp(glowColor, warmTint, uHeatLevel * 0.2);

    // Shockwave cone glow at tip
    float tipGlow = saturate(1.0 - coords.x * 6.0) * (0.2 + uHeatLevel * 0.3);

    glowColor *= uIntensity * baseTex.rgb * 0.7;

    float pulse = sin(uTime * 4.0 + coords.x * 6.0) * 0.08 + 0.92;

    float alpha = (edgeFade * trailFade + tipGlow) * uOpacity * sampleColor.a * baseTex.a * pulse * 0.4;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique TracerTrailMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 TracerTrailMainPS();
    }
}

technique TracerTrailGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 TracerTrailGlowPS();
    }
}
