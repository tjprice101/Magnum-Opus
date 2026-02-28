// =============================================================================
// Valor Aura Shader - Ember Ring Constellation
// =============================================================================
// Aura effect for Celestial Valor greatsword. Concentric ember rings
// with 6-fold heroic crest symmetry, constellation spark nodes at
// intersections, and a pulsing heartbeat rhythm.
//
// VISUAL IDENTITY: Rings of golden-scarlet embers orbit the weapon in
// concentric circles. Where rings intersect with the 6-fold symmetry
// lines, bright constellation nodes flare up. The whole aura breathes
// with a slow heartbeat pulse. Small ember motes drift outward through
// the rings. It feels like a shield of heroic fire surrounding the blade.
//
// Techniques:
//   ValorAuraMain  - Ember rings with constellation nodes
//   ValorAuraGlow  - Soft warm outer bloom
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

float4 ApplyOverbright(float3 color, float alpha)
{
    return float4(color * uOverbrightMult, alpha);
}

// =============================================================================
// TECHNIQUE 1: VALOR AURA MAIN - Ember Ring Constellation
// =============================================================================

float4 ValorAuraMainPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    // Centre the coordinates
    float2 centre = coords - 0.5;
    float r = length(centre);
    float angle = atan2(centre.y, centre.x);

    // --- Concentric ember rings ---
    // Multiple rings at different radii, each expanding outward over time
    float ringExpand = uTime * uScrollSpeed * 0.3;

    float ring1 = abs(sin((r * 12.0 - ringExpand) * 3.14159));
    ring1 = pow(ring1, 8.0); // Sharp thin rings

    float ring2 = abs(sin((r * 12.0 - ringExpand + 1.57) * 3.14159));
    ring2 = pow(ring2, 10.0); // Even thinner secondary rings

    float ring3 = abs(sin((r * 8.0 - ringExpand * 0.7 + 0.78) * 3.14159));
    ring3 = pow(ring3, 12.0); // Third set, slower expansion

    float rings = ring1 * 0.6 + ring2 * 0.3 + ring3 * 0.2;

    // --- 6-fold heroic crest symmetry ---
    float symAngle = abs(frac(angle / (3.14159 / 3.0)) * 2.0 - 1.0);
    float crestLine = pow(saturate(1.0 - symAngle * 5.0), 2.0);

    // Crest line intensity fades with distance
    crestLine *= saturate(1.0 - abs(r - 0.3) * 3.0); // Brightest at mid-radius

    // --- Constellation nodes: where rings meet crest lines ---
    float nodeIntensity = rings * crestLine * 3.0;
    nodeIntensity = saturate(nodeIntensity);

    // Nodes twinkle
    float twinkle = sin(uTime * 6.0 + angle * 3.0 + r * 20.0) * 0.3 + 0.7;
    nodeIntensity *= twinkle;

    // --- Drifting ember motes ---
    float2 moteGrid = float2(angle * 4.0, r * 15.0);
    float2 moteId = floor(moteGrid);
    float2 moteFrac = frac(moteGrid);

    float moteSeed = HashNoise(moteId);
    float moteVisible = step(0.85, moteSeed);

    // Motes drift outward
    float moteDrift = frac(moteSeed * 3.0 + uTime * (0.3 + moteSeed * 0.5));
    float2 motePos = float2(0.5 + sin(moteDrift * 6.28 + moteId.x) * 0.2, moteDrift);
    float moteDist = length(moteFrac - motePos);
    float mote = saturate(1.0 - moteDist * 10.0) * moteVisible;

    // Mote fade at extremes
    mote *= saturate(1.0 - moteDrift * 1.3);

    // --- Noise turbulence on rings ---
    float noise = SmoothHash(float2(angle * 2.0 + uTime * 0.5, r * 5.0 - uTime * 0.3));
    rings *= (0.6 + noise * 0.4);

    // --- Radial fade ---
    float radialFade = saturate(1.0 - r * 2.2) * saturate(r * 5.0); // Fade at centre and edges

    // --- Heartbeat pulse ---
    // Two-beat pattern: tha-THUMP ... tha-THUMP
    float heartbeat = uTime * 2.0;
    float beat1 = pow(saturate(1.0 - abs(frac(heartbeat) - 0.1) * 8.0), 2.0);
    float beat2 = pow(saturate(1.0 - abs(frac(heartbeat) - 0.25) * 10.0), 2.0);
    float pulse = 0.85 + (beat1 * 0.1 + beat2 * 0.15);

    // --- Phase: aura intensity ramps with phase ---
    float phaseBoost = lerp(0.3, 1.0, uPhase);

    // --- Base texture ---
    float4 baseTex = tex2D(uImage0, coords);

    // --- Colour ---
    // Rings: scarlet -> gold gradient based on radius
    float3 innerColor = uColor;                           // Scarlet at centre
    float3 outerColor = lerp(uColor, uSecondaryColor, 0.4); // More golden outward
    float3 ringColor = lerp(innerColor, outerColor, saturate(r * 2.5));

    // Constellation nodes: white-gold
    float3 nodeColor = float3(1.0, 0.9, 0.6);

    // Crest lines: dim gold
    float3 crestColor = uSecondaryColor * 0.6;

    // Motes: bright orange sparks
    float3 moteColor = float3(1.0, 0.6, 0.2);

    float3 finalColor = ringColor * rings * 0.8
                       + nodeColor * nodeIntensity * 1.5
                       + crestColor * crestLine * 0.4
                       + moteColor * mote * 1.2;

    finalColor *= baseTex.rgb * uIntensity * pulse * phaseBoost;

    float alpha = (rings * 0.6 + nodeIntensity * 0.3 + crestLine * 0.15 + mote * 0.15)
                * radialFade * uOpacity * sampleColor.a * baseTex.a * phaseBoost;

    return ApplyOverbright(finalColor, alpha);
}

// =============================================================================
// TECHNIQUE 2: VALOR AURA GLOW - Soft Warm Outer Bloom
// =============================================================================

float4 ValorAuraGlowPS(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;

    float4 baseTex = tex2D(uImage0, coords);

    float2 centre = coords - 0.5;
    float r = length(centre);

    // Wide soft radial glow
    float glow = saturate(1.0 - r * 2.0);
    glow = pow(glow, 1.2);

    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3) * 0.4;
    glowColor *= uIntensity * baseTex.rgb;

    // Heartbeat in glow
    float heartbeat = uTime * 2.0;
    float beat = pow(saturate(1.0 - abs(frac(heartbeat) - 0.2) * 6.0), 2.0);
    float pulse = 0.9 + beat * 0.2;

    float alpha = glow * uOpacity * sampleColor.a * baseTex.a * pulse * uPhase * 0.2;

    return ApplyOverbright(glowColor, alpha);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique ValorAuraMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ValorAuraMainPS();
    }
}

technique ValorAuraGlow
{
    pass P0
    {
        VertexShader = compile vs_2_0 MainVS();
        PixelShader = compile ps_3_0 ValorAuraGlowPS();
    }
}
