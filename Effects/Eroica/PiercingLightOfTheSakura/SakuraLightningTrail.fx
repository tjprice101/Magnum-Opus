// =============================================================================
// Sakura Lightning Trail Shader - VS 2.0 + PS 2.0 Compatible
// =============================================================================
// Zigzagging lightning bolt trail for Piercing Light of the Sakura crescendo
// projectiles. Sharp angular trail with sakura-fire colours, noise-driven
// branching forks, and charge-driven intensity.
//
// UV Layout:
//   U (coords.x) = along trail (0 = head, 1 = tail)
//   V (coords.y) = across trail width (0 = top edge, 1 = bottom edge)
//
// Techniques:
//   LightningTrailMain  - Sharp zigzag bolt with electrical crackling
//   LightningTrailGlow  - Wider soft bloom for electrical aura
//
// Features:
//   - High-frequency zigzag displacement via sin waves
//   - Noise-driven branching fork impressions
//   - Charge-reactive intensity (weak spark ↁEfull bolt)
//   - Sakura pink ↁEgold ↁEwhite-hot colour escalation
//   - Rapid electrical flicker for crackling feel
//   - Overbright multiplier for HDR bloom
// =============================================================================

sampler2D uImage0 : register(s0); // Base trail texture
sampler2D uImage1 : register(s1); // Noise texture (optional)

float4x4 uTransformMatrix;
float uTime;
float3 uColor;           // Primary color (Sakura pink)
float3 uSecondaryColor;  // Secondary color (Gold)
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;       // Lightning scroll rate
float uDistortionAmt;     // Zigzag displacement amount
float uNoiseScale;        // Noise UV repetition
float uPhase;            // Charge level (0 = weak spark, 1 = full bolt)
float uBranchIntensity;   // Fork branching strength
float uHasSecondaryTex;

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
// TECHNIQUE 1: LIGHTNING TRAIL MAIN
// =============================================================================

float4 LightningTrailMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Zigzag displacement (sharp, angular, not smooth) ---
    // Primary bolt zigzag
    float zigTime = uTime * uScrollSpeed * 8.0;
    float zig1 = sin(coords.x * 25.0 + zigTime) * uDistortionAmt;
    // Secondary high-freq jitter for electric crackle
    float zig2 = sin(coords.x * 50.0 - zigTime * 1.5 + 2.0) * uDistortionAmt * 0.4;
    // Low-freq wander for bolt path variation
    float zig3 = sin(coords.x * 8.0 + uTime * 3.0) * uDistortionAmt * 0.6;

    float2 distortedUV = coords;
    distortedUV.y += zig1 + zig2 + zig3;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Sharp core profile (lightning is very thin and bright) ---
    float edgeFade = QuadraticBump(coords.y);
    float coreFade = pow(edgeFade, 3.5);

    // --- Trail length fade (bolt dissipates at tail) ---
    float trailFade = pow(1.0 - coords.x, 1.5);

    // --- Branching fork impressions ---
    float2 branchUV = float2(coords.x * 6.0 + uTime * uScrollSpeed * 2.0, coords.y * 3.0);
    float branchNoise = HashNoise(branchUV * uNoiseScale);
    float forks = saturate(branchNoise * 3.0 - 2.0) * uBranchIntensity;
    // Forks extend from the bolt edge
    float forkMask = saturate(0.6 - edgeFade) * 2.0;
    forks *= forkMask;

    // Optional noise texture
    float2 noiseUV = coords * uNoiseScale;
    noiseUV.x -= uTime * uScrollSpeed;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(branchNoise, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Charge-reactive colour ---
    float3 boltColor = lerp(uColor, uSecondaryColor, uPhase);
    float3 electricWhite = float3(1.0, 0.97, 0.92);
    boltColor = lerp(boltColor, electricWhite, coreFade * 0.6 * uPhase);

    // Branch colour (slightly different from core)
    float3 branchColor = lerp(uColor, float3(0.9, 0.8, 1.0), 0.3);

    // --- Electrical crackling flicker ---
    float crackle1 = sin(uTime * 20.0 + coords.x * 30.0) * 0.12;
    float crackle2 = sin(uTime * 35.0 + coords.x * 50.0) * 0.06;
    float crackle = 0.82 + crackle1 + crackle2;
    crackle *= 0.8 + uPhase * 0.2; // More stable at full charge

    // --- Charge-driven visibility ---
    float chargeVis = 0.3 + uPhase * 0.7;

    // --- Final composition ---
    float3 coreContrib = boltColor * coreFade * trailFade;
    float3 forkContrib = branchColor * forks * 0.5;
    float3 finalColor = (coreContrib + forkContrib) * baseTex.rgb * uIntensity * crackle;
    finalColor *= 0.65 + noiseVal * 0.35;

    float alpha = (coreFade * trailFade + forks * 0.3) * chargeVis * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: LIGHTNING TRAIL GLOW
// =============================================================================

float4 LightningTrailGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Wider, softer profile
    float edgeFade = QuadraticBump(coords.y);
    edgeFade = pow(edgeFade, 0.4);

    float trailFade = pow(1.0 - coords.x, 1.2);

    // Glow colour
    float3 glowColor = lerp(uColor, uSecondaryColor, uPhase * 0.5);
    float3 electricTint = float3(0.85, 0.80, 1.0);
    glowColor = lerp(glowColor, electricTint, 0.15);

    glowColor *= uIntensity * baseTex.rgb * 0.6;

    float chargeVis = 0.3 + uPhase * 0.7;

    float pulse = sin(uTime * 5.0 + coords.x * 8.0) * 0.1 + 0.9;

    float alpha = edgeFade * trailFade * chargeVis * uOpacity * sampleColor.a * baseTex.a * pulse * 0.35;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique LightningTrailMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 LightningTrailMainPS();
    }
}

technique LightningTrailGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 LightningTrailGlowPS();
    }
}
