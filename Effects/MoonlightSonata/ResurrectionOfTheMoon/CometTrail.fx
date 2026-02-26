// ============================================================================
//  CometTrail.fx  EBurning comet tail shader for Resurrection of the Moon
//
//  Visual: A blazing comet ember trail that shifts from white-hot at the core
//  through gold and violet to deep space black at the tail. Scrolling ember
//  particles and cooling gradient simulate a streaking celestial body.
//
//  ps_2_0 compatible. Two techniques:
//    CometTrailMain   EFull burning comet tail with ember scrolling and cooling gradient
//    CometTrailGlow   ESoft wide glow pass for bloom overlay
// ============================================================================

sampler uImage0 : register(s0);    // Primary texture (trail body)
sampler uImage1 : register(s1);    // Secondary texture (noise/ember pattern)

float4 uColor;                      // Primary comet color (CometCore white-gold)
float4 uSecondaryColor;             // Secondary color (CometTrail violet)
float uOpacity;                     // Overall opacity
float uTime;                        // Scrolling time
float uIntensity;                   // Brightness multiplier (escalates with ricochets)
float uOverbrightMult;              // HDR overbright factor
float uScrollSpeed;                 // Ember scroll rate
float uNoiseScale;                  // Noise detail scale
float uDistortionAmt;               // Heat distortion amount
bool uHasSecondaryTex;              // Whether secondary texture is bound
float uSecondaryTexScale;           // Secondary texture UV scale
float uSecondaryTexScroll;          // Secondary texture scroll rate
float uPhase;                       // Comet intensity phase (0 = cold first shot, 1 = white-hot max ricochets)

// ============================================================================
//  Ember noise  Eprocedural hash for ember particle simulation
// ============================================================================
float Hash(float2 p)
{
    float h = dot(p, float2(127.1, 311.7));
    return frac(sin(h) * 43758.5453);
}

float EmberNoise(float2 uv)
{
    float2 i = floor(uv);
    float2 f = frac(uv);
    f = f * f * (3.0 - 2.0 * f);

    float a = Hash(i);
    float b = Hash(i + float2(1, 0));
    float c = Hash(i + float2(0, 1));
    float d = Hash(i + float2(1, 1));

    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ============================================================================
//  Cooling gradient  Emaps trail position to comet color temperature
//  0 = head (white-hot), 1 = tail (deep space violet/black)
// ============================================================================
float3 CoolingGradient(float coolProgress, float phase)
{
    // Temperature bands shift hotter with phase
    float hotThreshold = 0.15 + phase * 0.1;
    float warmThreshold = 0.4 + phase * 0.1;
    float coolThreshold = 0.7;

    float3 whiteHot = float3(0.95, 0.92, 1.0);      // Supernova white
    float3 goldCore = uColor.rgb;                      // CometCore white-gold
    float3 violetTrail = uSecondaryColor.rgb;           // CometTrail violet
    float3 deepSpace = float3(0.2, 0.08, 0.4);        // DeepSpaceViolet

    float3 result;
    if (coolProgress < hotThreshold)
    {
        float t = coolProgress / hotThreshold;
        result = lerp(whiteHot, goldCore, t);
    }
    else if (coolProgress < warmThreshold)
    {
        float t = (coolProgress - hotThreshold) / (warmThreshold - hotThreshold);
        result = lerp(goldCore, violetTrail, t);
    }
    else if (coolProgress < coolThreshold)
    {
        float t = (coolProgress - warmThreshold) / (coolThreshold - warmThreshold);
        result = lerp(violetTrail, deepSpace, t);
    }
    else
    {
        float t = (coolProgress - coolThreshold) / (1.0 - coolThreshold);
        result = lerp(deepSpace, float3(0.05, 0.02, 0.1), t);
    }

    return result;
}

// ============================================================================
//  Main comet trail pixel shader
// ============================================================================
float4 PS_CometTrailMain(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    // === COOLING GRADIENT along trail length (UV.x = 0 head, 1 tail) ===
    float coolProgress = uv.x;

    // === EMBER PARTICLE SCROLLING ===
    float2 emberUV = uv * float2(uNoiseScale * 3.0, uNoiseScale * 1.5);
    emberUV.x -= uTime * uScrollSpeed;
    float emberPattern = EmberNoise(emberUV);

    // Second faster ember layer
    float2 emberUV2 = uv * float2(uNoiseScale * 5.0, uNoiseScale * 2.5);
    emberUV2.x -= uTime * uScrollSpeed * 1.8;
    float emberPattern2 = EmberNoise(emberUV2);

    float embers = lerp(emberPattern, emberPattern2, 0.4);

    // Embers are brighter near the head, dimmer in tail
    float emberBrightness = (1.0 - coolProgress * 0.7) * embers;

    // === HEAT DISTORTION  Eslight UV warp near head ===
    float distortion = sin(uv.y * 15.0 + uTime * 4.0) * uDistortionAmt * (1.0 - coolProgress);
    float2 distortedUV = uv + float2(distortion * 0.02, 0);

    // === SECONDARY TEXTURE OVERLAY (noise pattern) ===
    float noiseOverlay = 1.0;
    if (uHasSecondaryTex)
    {
        float2 noiseUV = distortedUV * uSecondaryTexScale;
        noiseUV.x -= uTime * uSecondaryTexScroll;
        noiseOverlay = tex2D(uImage1, noiseUV).r;
        noiseOverlay = lerp(0.7, 1.0, noiseOverlay);
    }

    // === COMBINE: Cooling gradient + ember brightness ===
    float3 cometColor = CoolingGradient(coolProgress, uPhase);

    // Add ember glow
    float3 emberGlow = cometColor * (1.0 + emberBrightness * (0.8 + uPhase * 0.8));

    // Core brightness along center line (UV.y = 0.5 center)
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float coreBright = 1.0 - centerDist * centerDist;
    coreBright = coreBright * coreBright; // Sharper falloff

    float3 finalColor = emberGlow * coreBright * noiseOverlay;

    // Intensity and overbright
    finalColor *= uIntensity * (1.0 + uOverbrightMult * (1.0 - coolProgress * 0.5));

    // Tail fade
    float tailFade = 1.0 - smoothstep(0.6, 1.0, coolProgress);

    float finalAlpha = baseTex.a * uOpacity * color.a * tailFade;
    return float4(finalColor * finalAlpha, finalAlpha);
}

// ============================================================================
//  Glow pass  Esoft wide bloom for comet body
// ============================================================================
float4 PS_CometTrailGlow(float2 uv : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, uv);
    if (baseTex.a < 0.01)
        return float4(0, 0, 0, 0);

    float coolProgress = uv.x;

    // Soft glow uses cooling gradient but much softer
    float3 glowColor = CoolingGradient(coolProgress * 0.7, uPhase);

    // Broad gaussian-like falloff
    float centerDist = abs(uv.y - 0.5) * 2.0;
    float glowFalloff = exp(-centerDist * centerDist * 2.0);

    // Head-heavy glow
    float headGlow = 1.0 - coolProgress * 0.5;

    float3 finalColor = glowColor * glowFalloff * headGlow * uIntensity * 0.5;
    float finalAlpha = baseTex.a * uOpacity * color.a * (1.0 - coolProgress * 0.4) * 0.6;

    return float4(finalColor * finalAlpha, finalAlpha);
}

// ============================================================================
//  Techniques
// ============================================================================
technique CometTrailMain
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_CometTrailMain();
    }
}

technique CometTrailGlow
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_CometTrailGlow();
    }
}
