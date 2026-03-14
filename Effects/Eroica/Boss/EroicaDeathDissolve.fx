// ======================================================================
// EroicaDeathDissolve.fx — Eroica boss death dissolve animation
// The hero's final moment. Three-stage dissolve:
// 0-35%: Defiance | 35-70%: Fire Returns | 70-100%: Full Rebirth
// ======================================================================

// --- Eroica Palette Constants ---
static const float3 scarlet       = float3(0.78, 0.20, 0.20);
static const float3 crimson       = float3(0.70, 0.12, 0.24);
static const float3 gold          = float3(1.0, 0.78, 0.31);
static const float3 sakuraPink    = float3(1.0, 0.59, 0.71);
static const float3 funeralBlack  = float3(0.03, 0.02, 0.02);
static const float3 phoenixWhite  = float3(1.0, 0.94, 0.86);

// --- Samplers ---
sampler uImage0   : register(s0);  // Boss sprite
sampler uNoiseTex : register(s1);  // Dissolve noise

// --- Uniforms (existing) ---
float uDissolveProgress;  // 0 = solid, 1 = gone
float4 uEdgeColor;
float uEdgeWidth;

// --- Uniforms (new: movement system) ---
float uMovement;
float uHeroIntensity;

// ---- Utility Functions ----

float hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float noise(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    float a = hash(i);
    float b = hash(i + float2(1.0, 0.0));
    float c = hash(i + float2(0.0, 1.0));
    float d = hash(i + float2(1.0, 1.0));
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

// ---- Main Pixel Shader ----

float4 PS_DeathDissolve(float2 uv : TEXCOORD0) : COLOR0
{
    float4 sprite = tex2D(uImage0, uv);
    if (sprite.a < 0.01)
        return float4(0, 0, 0, 0);

    float progress = uDissolveProgress;

    // Multi-octave dissolve noise with sakura petal modulation
    float n1 = tex2D(uNoiseTex, uv * 1.5).r;
    float n2 = tex2D(uNoiseTex, uv * 3.0 + 0.5).r;
    float dissolveNoise = n1 * 0.65 + n2 * 0.35;

    // Sakura petal shape baked into dissolve pattern
    float2 centered = uv - 0.5;
    float angle = atan2(centered.y, centered.x);
    float petalModulation = sin(angle * 5.0) * 0.08;
    dissolveNoise += petalModulation;

    // Dissolve from edges inward (center-biased survival)
    float distFromCenter = length(centered) * 1.3;
    float dissolveThreshold = progress * 1.4 - (1.0 - distFromCenter) * 0.3;

    // Clip fully dissolved pixels
    float clipVal = dissolveNoise - dissolveThreshold;
    if (clipVal < 0.0)
        return float4(0, 0, 0, 0);

    // Edge proximity for glow effects
    float edge = 1.0 - smoothstep(0.0, uEdgeWidth, clipVal);

    float4 result = sprite;

    if (progress < 0.35)
    {
        // ============================================================
        // STAGE 1: HERO STANDS DEFIANT (0% – 35%)
        // Sprite desaturates toward gold-tinted grayscale. Warm gold
        // edge glow. Sakura petal shapes in dissolve noise. Dignified.
        // ============================================================

        float stageProgress = progress / 0.35;

        // Desaturation toward golden grayscale
        float gray = dot(sprite.rgb, float3(0.299, 0.587, 0.114));
        float3 goldGray = lerp(float3(gray, gray, gray), gold * gray * 1.4, 0.4);
        result.rgb = lerp(sprite.rgb, goldGray, stageProgress * 0.7);

        // Warm gold edge glow — dignified, not violent
        float3 edgeCol = lerp(gold, phoenixWhite * 0.8, edge * 0.3);
        result.rgb = lerp(result.rgb, edgeCol, edge * edge * 0.8);

        // Subtle sakura petal shapes scattered in the dissolve
        float sakuraDots = hash(floor(uv * 30.0));
        sakuraDots = step(0.97, sakuraDots) * stageProgress;
        result.rgb += sakuraPink * sakuraDots * 0.3;
    }
    else if (progress < 0.70)
    {
        // ============================================================
        // STAGE 2: THE FIRE RETURNS (35% – 70%)
        // Dissolve edges ignite with scarlet → gold → phoenix-white
        // gradient. Embers rise from dissolving pixels. The music swells.
        // ============================================================

        float stageProgress = (progress - 0.35) / 0.35;

        // Igniting edge: three-zone gradient
        float3 edgeCol;
        if (edge > 0.6)
            edgeCol = lerp(gold, phoenixWhite, (edge - 0.6) / 0.4);
        else if (edge > 0.25)
            edgeCol = lerp(scarlet, gold, (edge - 0.25) / 0.35);
        else
            edgeCol = lerp(crimson, scarlet, edge / 0.25);

        // Rising ember particles from dying pixels
        float emberSeed = hash(floor(uv * 25.0 + float2(0, stageProgress * 3.0)));
        float embers = step(0.93, emberSeed) * stageProgress;

        // Progressive golden warming of surviving sprite
        result.rgb = lerp(sprite.rgb, gold * 0.8 + sprite.rgb * 0.3, stageProgress * 0.5);
        result.rgb = lerp(result.rgb, edgeCol, edge * edge);
        result.rgb += phoenixWhite * embers * 0.6;
    }
    else
    {
        // ============================================================
        // STAGE 3: FULL REBIRTH (70% – 100%)
        // Entire dissolving edge becomes blinding phoenix-white. Inside
        // the boundary the sprite shifts to pure golden luminance.
        // Final fragments scatter as golden particles. The hero ascends.
        // ============================================================

        float stageProgress = (progress - 0.70) / 0.30;

        // Blinding phoenix-white dissolve edge
        float3 edgeCol = lerp(phoenixWhite, float3(1.0, 1.0, 1.0), stageProgress * 0.5);

        // Sprite shifts to pure golden luminance
        float luminance = dot(sprite.rgb, float3(0.299, 0.587, 0.114));
        float3 goldenLuminance = gold * luminance * 2.0;
        goldenLuminance = lerp(goldenLuminance, phoenixWhite * luminance * 2.5, stageProgress);
        result.rgb = lerp(sprite.rgb, goldenLuminance, 0.5 + stageProgress * 0.5);

        // Blinding edge overlay
        result.rgb = lerp(result.rgb, edgeCol, edge * (0.8 + stageProgress * 0.2));

        // Golden scatter particles from dissolving fragments
        float scatterSeed = hash(floor(uv * 35.0 + float2(stageProgress * 5.0, 0)));
        float scatter = step(0.9, scatterSeed) * stageProgress;
        result.rgb += gold * scatter * 0.8;
        result.rgb += phoenixWhite * scatter * stageProgress * 0.5;

        // Final intensification — the hero burns brightest
        result.rgb *= 1.0 + stageProgress * uHeroIntensity * 0.6;
    }

    result.a = sprite.a;
    return result;
}

technique Technique1
{
    pass DeathDissolve
    {
        PixelShader = compile ps_3_0 PS_DeathDissolve();
    }
}
