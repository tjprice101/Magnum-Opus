// =============================================================================
// Dual-Fated Chime  EInferno Waltz Aura Shader (Enhanced)
// =============================================================================
// Radial inferno aura for the spinning Waltz attack. Concentric bell-toll
// shockwave rings expand outward while fire spokes whirl in a pinwheel.
// FBM-driven flame turbulence, bell-shaped radial energy bands, and
// ember particle scatter at the outer perimeter. The waltz intensifies
// with uPhase (spin progress)  Emore spokes, brighter fire, tighter rings.
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float4x4 uTransformMatrix;
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uOverbrightMult;
float uPhase;
float uScrollSpeed;
float uNoiseScale;
float uHasSecondaryTex;

struct VSInput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

struct VSOutput
{
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0;
    float4 Color    : COLOR0;
};

VSOutput WaltzVS(VSInput input)
{
    VSOutput output;
    output.Position = mul(input.Position, uTransformMatrix);
    output.TexCoord = input.TexCoord;
    output.Color = input.Color;
    return output;
}

float HashNoise(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

float SmoothNoise(float2 uv)
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

float FBM(float2 uv)
{
    float v = 0.0; float a = 0.5; float2 p = uv;
    v += SmoothNoise(p) * a; p *= 2.07; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.03; a *= 0.5;
    v += SmoothNoise(p) * a; p *= 2.01; a *= 0.5;
    v += SmoothNoise(p) * a;
    return v;
}

float4 WaltzAuraPS(VSOutput input) : COLOR0
{
    float2 coords = input.TexCoord;
    float4 sampleColor = input.Color;
    float4 baseTex = tex2D(uImage0, coords);

    float2 centred = coords - 0.5;
    float dist = length(centred) * 2.0;
    float angle = atan2(centred.y, centred.x);

    // --- Bell-toll concentric rings (expand outward, sharpen with phase) ---
    float ringSpeed = uTime * uScrollSpeed * 2.0;
    float ringFreq = lerp(4.0, 8.0, uPhase);
    float rings = sin((dist * ringFreq - ringSpeed) * 6.28318) * 0.5 + 0.5;
    rings = pow(rings, lerp(4.0, 2.5, uPhase));  // Sharper at start, broader at full spin

    // Secondary harmonic ring (octave above)
    float rings2 = sin((dist * ringFreq * 2.0 - ringSpeed * 1.5) * 6.28318) * 0.5 + 0.5;
    rings2 = pow(rings2, 5.0) * 0.3;

    // --- Fire spokes (pinwheel, count increases with phase) ---
    float spokeCount = lerp(3.0, 6.0, uPhase);
    float spokeAngle = angle + uTime * 3.5 * (0.3 + uPhase * 0.7);
    float spokes = cos(spokeAngle * spokeCount) * 0.5 + 0.5;
    spokes = pow(spokes, 2.5);

    // Spoke flames with FBM turbulence
    float2 spokeFBMuv = float2(angle * 0.5 + uTime * 0.3, dist * uNoiseScale * 2.0);
    float spokeFire = FBM(spokeFBMuv);
    spokes *= saturate(spokeFire * 1.3 + 0.2);

    // --- Radial FBM flame turbulence ---
    float2 flameUV = float2(dist * 3.0 + uTime * 0.5, angle * 1.5 + uTime * 0.3);
    float flame = FBM(flameUV * uNoiseScale);
    flame = saturate(flame * 1.4 - 0.1);

    // Optional noise texture
    float2 secUV = float2(angle * 0.318 + uTime * 0.2, dist + uTime * 0.15);
    float4 noiseTex = tex2D(uImage1, secUV);
    float texNoise = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.5);

    // --- Radial falloff: bell-shaped curve (bright rim + hot center) ---
    float outerRim = exp(-pow((dist - 0.75) * 4.0, 2.0)) * 0.5;
    float innerGlow = exp(-dist * dist * 6.0);
    float radial = innerGlow + outerRim * uPhase;

    // --- Ember scatter at outer perimeter ---
    float emberZone = saturate(dist - 0.55) * saturate(1.15 - dist);
    float2 emberUV = float2(angle * 8.0, dist * 15.0) + uTime * float2(2.0, 0.5);
    float embers = HashNoise(emberUV);
    embers = step(0.91, embers) * emberZone * uPhase;

    // --- 5-stop color gradient ---
    float3 voidBlack = float3(0.02, 0.01, 0.005);
    float3 deepEmber = uColor * 0.35;
    float3 flameOrange = uColor;
    float3 bellGold = uSecondaryColor;
    float3 whiteHot = float3(1.0, 0.92, 0.7);

    float t = saturate(radial * flame * 1.4);
    float3 auraColor = lerp(voidBlack, deepEmber, saturate(t * 3.0));
    auraColor = lerp(auraColor, flameOrange, saturate(t * 2.2 - 0.25));
    auraColor = lerp(auraColor, bellGold, (rings + rings2) * 0.45);
    auraColor = lerp(auraColor, whiteHot, spokes * rings * 0.35);

    // Hot center punch
    auraColor = lerp(auraColor, whiteHot, innerGlow * uPhase * 0.55);

    // Ember sparks
    auraColor += float3(1.0, 0.65, 0.15) * embers * 2.5;

    // --- Phase-reactive intensity & pulse ---
    float phaseIntensity = 0.2 + uPhase * 0.8;
    float pulse = sin(uTime * 7.0) * 0.06 + 0.94;
    pulse *= sin(uTime * 17.0) * 0.03 + 0.97;

    float3 finalColor = auraColor * uIntensity * texNoise * pulse * baseTex.rgb;
    float alpha = (radial * (rings * 0.5 + spokes * 0.35 + rings2 * 0.15) + flame * 0.1 + embers * 0.3)
                  * phaseIntensity * uOpacity * sampleColor.a * baseTex.a;

    return ApplyOverbright(finalColor, saturate(alpha));
}

technique WaltzAuraMain
{
    pass P0
    {
        VertexShader = compile vs_2_0 WaltzVS();
        PixelShader = compile ps_3_0 WaltzAuraPS();
    }
}
