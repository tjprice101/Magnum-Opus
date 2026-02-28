// =============================================================================
// Heroic Flame Trail Shader - Rising Ember Crescendo
// =============================================================================
// Shared flame trail shader for Eroica weapons. Multi-octave turbulent
// fire with rising ember particles, fire-crack energy veins, and a
// musical crescendo ramp that builds from smoulder to inferno.
//
// VISUAL IDENTITY: A roaring heroic bonfire that sweeps behind the
// weapon. Multiple octaves of fire noise create turbulent, slightly
// chaotic flame with visible "fire cracks" -- bright veins of energy
// within the flame body. Tiny ember particles rise upward from the
// trail edges. The intensity ramps from the tail (smouldering embers)
// to the head (white-hot inferno) like a musical crescendo.
//
// Techniques:
//   HeroicFlameFlow  - Multi-octave fire with cracks and embers
//   HeroicFlameGlow  - Warm bloom underlay
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
// FIRE-SPECIFIC: Multi-octave turbulence and fire cracks
// =============================================================================

// 4-octave fire turbulence (different from FBM -- summed with abs for flame look)
float FireTurbulence(float2 uv)
{
    float val = 0.0;
    float amp = 0.5;
    float2 p = uv;
    // Octave 1: Large billowing shapes
    val += abs(SmoothHash(p) * 2.0 - 1.0) * amp;
    p *= 2.1; amp *= 0.5;
    // Octave 2: Medium flame tongues
    val += abs(SmoothHash(p) * 2.0 - 1.0) * amp;
    p *= 2.3; amp *= 0.5;
    // Octave 3: Small licks
    val += abs(SmoothHash(p) * 2.0 - 1.0) * amp;
    p *= 2.0; amp *= 0.5;
    // Octave 4: Fine detail
    val += abs(SmoothHash(p) * 2.0 - 1.0) * amp;
    return val;
}

// Fire crack detection: bright veins where noise values align closely
float FireCracks(float2 uv)
{
    float n1 = SmoothHash(uv * 3.0);
    float n2 = SmoothHash(uv * 3.0 + float2(0.5, 0.3));
    float crack = saturate(1.0 - abs(n1 - n2) * 15.0);
    return crack * crack; // Sharpen
}

// =============================================================================
// TECHNIQUE 1: HEROIC FLAME FLOW - Turbulent Inferno
// =============================================================================

float4 HeroicFlameFlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // --- Fire distortion (turbulent, not smooth) ---
    float turb = FireTurbulence(coords * uNoiseScale * 0.5 + float2(-uTime * uScrollSpeed, uTime * 0.3));
    float2 distortedUV = coords;
    distortedUV.x -= uTime * uScrollSpeed * 0.8;
    distortedUV.y += (turb - 0.5) * uDistortionAmt * 1.5;
    distortedUV.x += (turb - 0.5) * uDistortionAmt * 0.5;

    float4 baseTex = tex2D(uImage0, distortedUV);

    // --- Edge shape ---
    float edgeFade = QuadraticBump(coords.y);

    // --- Musical crescendo ramp: tail = smoulder, head = inferno ---
    // coords.x: 0 = head (newest), 1 = tail (oldest)
    float crescendo = saturate(1.0 - coords.x * 0.9);
    crescendo = pow(crescendo, 0.7); // Slightly non-linear buildup

    // --- Fire noise layers ---
    float2 fireUV1 = coords * uNoiseScale;
    fireUV1.x -= uTime * uScrollSpeed * 1.2;
    fireUV1.y -= uTime * 0.4;
    float fire1 = FireTurbulence(fireUV1);

    float2 fireUV2 = coords * uNoiseScale * 1.5;
    fireUV2.x -= uTime * uScrollSpeed * 0.8;
    fireUV2.y += uTime * 0.2;
    float fire2 = FireTurbulence(fireUV2);

    float combinedFire = fire1 * 0.6 + fire2 * 0.4;

    // Optional noise texture blend
    float2 noiseSample = coords * uNoiseScale;
    noiseSample.x -= uTime * uScrollSpeed * 0.5;
    float4 noiseTex = tex2D(uImage1, noiseSample);
    combinedFire = lerp(combinedFire, noiseTex.r, uHasSecondaryTex * 0.4);

    // --- Fire cracks: bright energy veins ---
    float2 crackUV = coords * uNoiseScale * 0.8;
    crackUV.x -= uTime * uScrollSpeed * 0.6;
    crackUV.y -= uTime * 0.15;
    float cracks = FireCracks(crackUV);
    cracks *= edgeFade * crescendo;

    // --- Rising ember particles ---
    // Tiny bright dots that drift upward from trail edges
    float2 emberGrid = coords * float2(30.0, 20.0);
    float2 emberId = floor(emberGrid);
    float2 emberFrac = frac(emberGrid);

    float emberSeed = HashNoise(emberId);
    float emberVisible = step(0.88, emberSeed); // Only ~12% of cells have embers

    // Embers rise upward (positive Y in screen space) and drift
    float emberRise = frac(emberSeed * 5.0 + uTime * (1.0 + emberSeed * 2.0));
    float2 emberPos = float2(0.5 + sin(emberRise * 6.28 + emberId.x) * 0.3, 1.0 - emberRise);
    float emberDist = length(emberFrac - emberPos);
    float ember = saturate(1.0 - emberDist * 12.0) * emberVisible;

    // Embers appear more at trail edges
    float emberEdgeBias = saturate((0.6 - edgeFade) * 3.0);
    ember *= emberEdgeBias;

    // Ember fade-out as they rise
    float emberFade = saturate(1.0 - emberRise * 1.5);
    ember *= emberFade;

    // --- Colour: Dark red -> Scarlet -> Orange -> Gold -> White crescendo ---
    float heatLevel = combinedFire * crescendo;

    float3 coolEmber = float3(0.3, 0.05, 0.02);      // Dark smoulder
    float3 warmFlame = uColor;                          // Scarlet
    float3 hotFlame = lerp(uColor, float3(1.0, 0.5, 0.1), 0.5); // Orange
    float3 whiteHot = float3(1.0, 0.9, 0.7);           // White-gold core

    float3 flameColor;
    if (heatLevel < 0.33)
        flameColor = lerp(coolEmber, warmFlame, heatLevel * 3.0);
    else if (heatLevel < 0.66)
        flameColor = lerp(warmFlame, hotFlame, (heatLevel - 0.33) * 3.0);
    else
        flameColor = lerp(hotFlame, whiteHot, (heatLevel - 0.66) * 3.0);

    // Fire cracks are white-hot
    flameColor = lerp(flameColor, whiteHot, cracks * 0.7);

    // Embers are orange-gold tiny sparks
    float3 emberColor = float3(1.0, 0.6, 0.15);
    flameColor += emberColor * ember * 1.5;

    // --- Dual-frequency heroic pulse ---
    float pulse1 = sin(uTime * 4.0 + coords.x * 5.0) * 0.1 + 0.9;
    float pulse2 = sin(uTime * 2.3 + coords.x * 2.0) * 0.05 + 0.95;
    float pulse = pulse1 * pulse2;

    // --- Trail fade ---
    float trailFade = saturate(1.0 - coords.x * 1.05);

    // --- Final composition ---
    float3 finalColor = flameColor * baseTex.rgb * uIntensity * pulse * crescendo;

    float fireAlpha = combinedFire * 0.5 + 0.5;
    float alpha = (edgeFade * fireAlpha + cracks * 0.3 + ember * 0.2)
                * trailFade * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: HEROIC FLAME GLOW - Warm Bloom
// =============================================================================

float4 HeroicFlameGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 0.85);

    // Crescendo in glow too
    float crescendo = pow(saturate(1.0 - coords.x * 0.9), 0.8);

    // Warm bloom colour
    float3 bloomColor = lerp(uColor, float3(1.0, 0.4, 0.1), 0.3);
    bloomColor *= uIntensity * baseTex.rgb;

    float softEdge = pow(edgeFade, 0.8);
    float pulse = sin(uTime * 2.0) * 0.08 + 0.92;

    float alpha = softEdge * trailFade * crescendo * uOpacity * sampleColor.a * baseTex.a * pulse * 0.3;

    return ApplyOverbright(bloomColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique HeroicFlameFlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 HeroicFlameFlowPS();
    }
}

technique HeroicFlameGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 HeroicFlameGlowPS();
    }
}
