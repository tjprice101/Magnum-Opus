// =============================================================================
// Sakura Lightning Trail Shader - Fractal Arc Storm
// =============================================================================
// Lightning bolt trail for Piercing Light of the Sakura (bow).
// Fractal branching arcs with charge accumulation glow, electric
// bright nodes, and sakura-pink lightning with scarlet core flash.
//
// VISUAL IDENTITY: Not a smooth energy beam -- this is LIGHTNING.
// The trail zigzags violently with fractal fork branches splitting
// off at random angles. Charge accumulation causes bright nodes to
// swell along the main bolt. Each fork terminates in sakura-pink
// sparks. The overall feel: a thunderbolt wrapped in cherry blossoms.
//
// Techniques:
//   LightningTrailMain  - Fractal zigzag bolt with fork branches
//   LightningTrailGlow  - Electric haze bloom around bolt path
// =============================================================================

sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1);

float4x4 uTransformMatrix;
float uTime;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uIntensity;
float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uNoiseScale;
float uPhase;
float uBranchIntensity;
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

float SmoothHash(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    float2 u = f * f * (3.0 - 2.0 * f);
    float a = HashNoise(i);
    float b = HashNoise(i + float2(1.0, 0.0));
    float c = HashNoise(i + float2(0.0, 1.0));
    float d = HashNoise(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
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
// LIGHTNING-SPECIFIC: Multi-frequency zigzag displacement
// =============================================================================

// Creates jagged zigzag displacement (not smooth sine -- ANGULAR lightning)
float ZigzagDisplacement(float x, float freq, float timeOffset)
{
    float phase = x * freq + uTime * uScrollSpeed * 8.0 + timeOffset;
    // Triangle wave for sharp zigzag instead of smooth sine
    float tri = abs(frac(phase * 0.159) * 2.0 - 1.0) * 2.0 - 1.0;
    return tri;
}

// Fractal branching: each fork gets smaller and more jagged
float ForkBranch(float2 coords, float forkAngle, float forkStart, float forkLen, float thickness)
{
    // Compute distance from a branching line segment
    float2 forkOrigin = float2(forkStart, 0.5);
    float2 forkDir = float2(cos(forkAngle), sin(forkAngle));
    float2 delta = coords - forkOrigin;
    float proj = dot(delta, forkDir);
    proj = saturate(proj / forkLen);
    float2 closestPt = forkOrigin + forkDir * proj * forkLen;
    float dist = length(coords - closestPt);

    // Zigzag along the fork
    float zigzag = ZigzagDisplacement(proj * 5.0, 4.0, forkAngle * 10.0) * 0.02 * (1.0 - proj);
    dist += abs(zigzag);

    // Thin bright line
    float brightness = saturate(1.0 - dist / thickness) * (1.0 - proj * 0.8);
    return brightness;
}

// =============================================================================
// TECHNIQUE 1: LIGHTNING TRAIL MAIN - Fractal Bolt
// =============================================================================

float4 LightningTrailMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Main bolt: 3-frequency zigzag displacement ---
    float zigFreq1 = ZigzagDisplacement(coords.x, 3.0, 0.0) * uDistortionAmt * 1.2;
    float zigFreq2 = ZigzagDisplacement(coords.x, 7.0, 3.14) * uDistortionAmt * 0.6;
    float zigFreq3 = ZigzagDisplacement(coords.x, 15.0, 1.57) * uDistortionAmt * 0.25;

    float totalDisplace = zigFreq1 + zigFreq2 + zigFreq3;

    // Displaced bolt centreline
    float boltCenter = 0.5 + totalDisplace;
    float boltDist = abs(coords.y - boltCenter);

    // Main bolt brightness: extremely bright core, sharp falloff
    float boltCore = saturate(1.0 - boltDist * 25.0); // Very thin bright core
    float boltGlow = saturate(1.0 - boltDist * 8.0);  // Wider soft glow
    float boltField = saturate(1.0 - boltDist * 4.0);  // Diffuse field

    boltCore = pow(boltCore, 0.5); // Soften slightly for smoother falloff

    // --- Fractal fork branches ---
    float branch1 = 0.0;
    float branch2 = 0.0;
    float branch3 = 0.0;
    float branch4 = 0.0;

    // Branch parameters derived from noise for variety
    float forkSeed = HashNoise(float2(floor(uTime * 4.0), 0.0));

    // 4 fork branches at different positions along the bolt
    float forkAngle1 = 0.5 + forkSeed * 0.8;
    branch1 = ForkBranch(coords, forkAngle1, 0.2 + forkSeed * 0.1, 0.12, 0.015);

    float forkAngle2 = -0.4 - forkSeed * 0.6;
    branch2 = ForkBranch(coords, forkAngle2, 0.4 + forkSeed * 0.05, 0.1, 0.012);

    float forkAngle3 = 0.7 + forkSeed * 0.5;
    branch3 = ForkBranch(coords, forkAngle3, 0.6 - forkSeed * 0.1, 0.08, 0.01);

    float forkAngle4 = -0.6 - forkSeed * 0.4;
    branch4 = ForkBranch(coords, forkAngle4, 0.75 + forkSeed * 0.05, 0.06, 0.008);

    float totalBranches = (branch1 + branch2 + branch3 + branch4) * uBranchIntensity;

    // --- Charge accumulation nodes ---
    // Bright swelling nodes at irregular intervals along the bolt
    float nodeSpacing = 0.18;
    float nodePhase = frac(coords.x / nodeSpacing + uTime * 0.3);
    float nodeBrightness = pow(saturate(1.0 - abs(nodePhase - 0.5) * 4.0), 2.0);
    nodeBrightness *= boltGlow; // Only visible near the bolt
    nodeBrightness *= (sin(uTime * 6.0 + coords.x * 20.0) * 0.3 + 0.7); // Flicker

    // --- Electric crackle noise ---
    float crackle = SmoothHash(coords * uNoiseScale * 2.0 + float2(uTime * 5.0, 0.0));
    float crackleFlicker = step(0.85, crackle) * boltField * 0.4;

    // Flicker: lightning should flicker rapidly
    float flicker = HashNoise(float2(floor(uTime * 15.0), floor(uTime * 7.0)));
    float flickerMask = 0.7 + flicker * 0.3;

    // --- Trail fade ---
    float trailFade = saturate(1.0 - coords.x * 1.05);
    float edgeFade = QuadraticBump(coords.y);

    // --- Base texture ---
    float2 distortedUV = coords;
    distortedUV.y += totalDisplace * 0.3;
    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Colour: Sakura-pink bolt with scarlet core ---
    float3 coreColor = uColor * 2.5; // Overbright scarlet core
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3); // Sakura-pink mid
    float3 fieldColor = uSecondaryColor * 0.6; // Faint pink field

    float3 boltColor = coreColor * boltCore
                      + glowColor * boltGlow * 0.6
                      + fieldColor * boltField * 0.2;

    // Branches are slightly more pink
    float3 branchColor = lerp(uSecondaryColor, float3(1.0, 0.7, 0.8), 0.3);
    boltColor += branchColor * totalBranches;

    // Charge nodes are white-hot
    float3 nodeColor = float3(1.0, 0.95, 0.9);
    boltColor += nodeColor * nodeBrightness * 0.8;

    // Crackle sparks
    boltColor += float3(1.0, 0.8, 0.9) * crackleFlicker;

    // --- Phase modulation: charge intensity ---
    float chargeBoost = lerp(0.5, 1.5, uPhase);

    // --- Final composition ---
    float3 finalColor = boltColor * baseTex.rgb * uIntensity * flickerMask * chargeBoost;

    float alpha = (boltGlow * 0.8 + totalBranches * 0.3 + nodeBrightness * 0.2 + crackleFlicker)
                * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: LIGHTNING TRAIL GLOW - Electric Haze
// =============================================================================

float4 LightningTrailGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    // Wider glow around main bolt path
    float zigDisp = ZigzagDisplacement(coords.x, 3.0, 0.0) * uDistortionAmt;
    float boltCenter = 0.5 + zigDisp;
    float dist = abs(coords.y - boltCenter);
    float wideGlow = saturate(1.0 - dist * 3.0);
    wideGlow = pow(wideGlow, 1.5);

    float trailFade = saturate(1.0 - coords.x * 0.8);

    // Electric haze shimmer
    float shimmer = SmoothHash(coords * 5.0 + float2(uTime * 3.0, uTime * 1.5));
    shimmer = shimmer * 0.3 + 0.7;

    float3 hazeColor = lerp(uSecondaryColor, uColor, 0.2) * 0.5;
    hazeColor *= uIntensity * baseTex.rgb * shimmer;

    float flicker = 0.8 + HashNoise(float2(floor(uTime * 12.0), 0.0)) * 0.2;

    float alpha = wideGlow * trailFade * uOpacity * sampleColor.a * baseTex.a * flicker * 0.25;

    return ApplyOverbright(hazeColor, alpha);
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
