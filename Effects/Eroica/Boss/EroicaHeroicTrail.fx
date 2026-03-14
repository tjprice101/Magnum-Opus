// ======================================================================
// EroicaHeroicTrail.fx — Eroica boss dash / movement trail
// Movement-based phase system: Beethoven's Third Symphony
// M1: War Banner | M2: Funeral Smoke | M3: Staccato | M4: Supernova
// ======================================================================

// --- Eroica Palette Constants ---
static const float3 scarlet       = float3(0.78, 0.20, 0.20);
static const float3 crimson       = float3(0.70, 0.12, 0.24);
static const float3 gold          = float3(1.0, 0.78, 0.31);
static const float3 sakuraPink    = float3(1.0, 0.59, 0.71);
static const float3 funeralBlack  = float3(0.03, 0.02, 0.02);
static const float3 phoenixWhite  = float3(1.0, 0.94, 0.86);

// --- Samplers ---
sampler uImage0   : register(s0);
sampler uNoiseTex : register(s1);

// --- Uniforms (existing) ---
float4 uColor;
float uTrailWidth;
float uFadeRate;
float uTime;

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

float fbm(float2 p, int octaves)
{
    float value = 0.0;
    float amplitude = 0.5;
    for (int i = 0; i < octaves; i++)
    {
        value += amplitude * noise(p);
        p *= 2.0;
        amplitude *= 0.5;
    }
    return value;
}

// ---- Main Pixel Shader ----

float4 PS_HeroicTrail(float2 uv : TEXCOORD0) : COLOR0
{
    // Trail UV: x = along trail (0=newest, 1=oldest), y = across width
    float trailProgress = uv.x;
    float trailWidth = abs(uv.y - 0.5) * 2.0;

    float3 color = float3(0, 0, 0);
    float alpha = 0.0;

    if (uMovement < 1.5)
    {
        // ============================================================
        // MOVEMENT 1 — WAR BANNER TRAIL
        // Bold scarlet-to-gold gradient flowing like a banner in wind.
        // Clean fire edges. Embedded bright embers. Heroic, triumphant.
        // ============================================================

        // Banner-in-wind distortion via sine offset on Y
        float2 bannerUV = float2(
            uv.x * 2.0 - uTime * 2.5,
            uv.y * 1.5 + sin(uv.x * 8.0 + uTime * 3.0) * 0.15
        );
        float bannerWave = tex2D(uNoiseTex, bannerUV).r;

        // Clean fire edge shape
        float edgeFlame = smoothstep(0.85, 0.15, trailWidth + bannerWave * 0.2);

        // Bright ember hotspots
        float2 emberUV = float2(uv.x * 6.0 - uTime * 5.0, uv.y * 4.0);
        float embers = tex2D(uNoiseTex, emberUV).r;
        embers = smoothstep(0.75, 0.92, embers);

        float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.0);

        // Scarlet base → gold center → white-hot embers
        float heatGradient = edgeFlame * (1.0 - trailWidth * 0.5);
        color = lerp(scarlet, gold, heatGradient);
        color = lerp(color, phoenixWhite, embers * 0.6);

        alpha = edgeFlame * ageFade * uTrailWidth;
        alpha += embers * ageFade * 0.4;
    }
    else if (uMovement < 2.5)
    {
        // ============================================================
        // MOVEMENT 2 — FUNERAL SMOKE TRAIL
        // Heavy, lingering, slow to fade. Dark crimson base with ashen
        // edges. Billowing FBM smoke. Occasional golden sparks like
        // memories of glory breaking through.
        // ============================================================

        // Billowing smoke via FBM noise
        float2 smokeUV = float2(uv.x * 1.5 - uTime * 0.5, uv.y * 2.0 + uTime * 0.2);
        float smoke = fbm(smokeUV * 3.0, 4);

        // Slow edge dissipation
        float edgeSmoke = smoothstep(0.9, 0.1, trailWidth + smoke * 0.4);

        // Very slow fade — lingering
        float ageFade = pow(1.0 - trailProgress, uFadeRate * 0.8);

        // Rare golden sparks — memory of glory
        float sparkSeed = hash(floor(uv * 25.0 + float2(-uTime * 0.8, uTime * 0.3)));
        float sparks = step(0.965, sparkSeed);
        sparks *= sin(uTime * 4.0 + sparkSeed * 20.0) * 0.4 + 0.6;

        // Ashen edges on dark crimson
        float3 ashColor = lerp(funeralBlack, crimson * 0.5, 0.3);
        color = lerp(ashColor, crimson, edgeSmoke * 0.7);
        color = lerp(color, gold * 0.6, sparks * 0.4);

        alpha = edgeSmoke * ageFade * uTrailWidth * 0.8;
        alpha += sparks * ageFade * 0.3;
    }
    else if (uMovement < 3.5)
    {
        // ============================================================
        // MOVEMENT 3 — STACCATO AFTERIMAGE TRAIL
        // Discrete dash segments, not continuous. Each segment alternates
        // scarlet/gold. Sharp step-function cut-offs between segments.
        // Electric crackling at segment boundaries.
        // ============================================================

        float segmentSize = 0.12;
        float segmentIndex = floor(uv.x / segmentSize);
        float segmentFrac = frac(uv.x / segmentSize);

        // Hard cut-off at segment edges — staccato gaps
        float segmentMask = step(0.15, segmentFrac) * step(segmentFrac, 0.85);

        // Alternate color per segment
        float altColor = frac(segmentIndex * 0.5);
        float3 segColor = altColor > 0.25 ? gold : scarlet;

        // Electric crackling at segment boundaries
        float boundaryDist = min(segmentFrac, 1.0 - segmentFrac);
        float electric = noise(float2(uv.x * 30.0, uv.y * 10.0 + uTime * 15.0));
        float crackle = smoothstep(0.2, 0.0, boundaryDist) * smoothstep(0.4, 0.7, electric);

        float ageFade = pow(1.0 - trailProgress, uFadeRate * 2.5);
        float edgeFade = smoothstep(0.9, 0.2, trailWidth);

        color = segColor * segmentMask;
        color += phoenixWhite * crackle * 0.8;

        alpha = segmentMask * edgeFade * ageFade * uTrailWidth;
        alpha += crackle * ageFade * 0.6;
    }
    else
    {
        // ============================================================
        // MOVEMENT 4 — SUPERNOVA TRAIL
        // White-hot core, gold-to-scarlet gradient radiating outward.
        // Maximum intensity fire noise. WIDE and BRIGHT.
        // Each afterimage bleeds light. Trail of divine fire.
        // ============================================================

        // Dual-layer fire noise at maximum agitation
        float2 fireUV = float2(uv.x * 3.0 - uTime * 4.0, uv.y * 2.0 + uTime * 1.0);
        float fire = tex2D(uNoiseTex, fireUV).r;
        float2 fireUV2 = float2(uv.x * 5.0 - uTime * 6.0, uv.y * 3.0 - uTime * 0.5);
        float fire2 = tex2D(uNoiseTex, fireUV2).r;
        float fireComposite = fire * 0.55 + fire2 * 0.45;

        // Wide trail — expanded edge tolerance
        float wideEdge = smoothstep(1.0, 0.0, trailWidth * 0.7);

        // Aggressively bright fade
        float ageFade = pow(1.0 - trailProgress, uFadeRate * 1.5);

        // Each afterimage bleeds additive light
        float bleed = fireComposite * wideEdge * (1.0 + uHeroIntensity);

        // White-hot core → gold mid → scarlet outer
        float heat = wideEdge * (1.0 - trailWidth);
        color = lerp(scarlet, gold, heat);
        color = lerp(color, phoenixWhite, heat * heat * (0.6 + uHeroIntensity * 0.4));
        color += phoenixWhite * bleed * 0.2;

        alpha = wideEdge * ageFade * uTrailWidth * (1.3 + uHeroIntensity * 0.5);
        alpha += fireComposite * ageFade * 0.4;
    }

    return float4(color, 1.0) * saturate(alpha);
}

technique Technique1
{
    pass HeroicTrail
    {
        PixelShader = compile ps_3_0 PS_HeroicTrail();
    }
}
