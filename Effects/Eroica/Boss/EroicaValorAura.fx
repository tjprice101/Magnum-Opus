// ======================================================================
// EroicaValorAura.fx — Eroica boss ambient presence aura
// Movement-based phase system: Beethoven's Third Symphony
// M1: Call to Arms | M2: Funeral March | M3: Scherzo | M4: Apotheosis
// ======================================================================

// --- Eroica Palette Constants ---
static const float3 scarlet       = float3(0.78, 0.20, 0.20);
static const float3 crimson       = float3(0.70, 0.12, 0.24);
static const float3 gold          = float3(1.0, 0.78, 0.31);
static const float3 sakuraPink    = float3(1.0, 0.59, 0.71);
static const float3 funeralBlack  = float3(0.03, 0.02, 0.02);
static const float3 phoenixWhite  = float3(1.0, 0.94, 0.86);

// --- Samplers ---
sampler uImage0 : register(s0);

// --- Uniforms (existing) ---
float2 uCenter;
float uRadius;
float uIntensity;
float4 uPrimaryColor;
float4 uSecondaryColor;
float uTime;

// --- Uniforms (new: movement system) ---
float uMovement;       // 1-4 symphony movement
float uHeroIntensity;  // 0-1 heroic energy ramp

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

// Voronoi cell distance for cracked-armor pattern
float voronoi(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    float minDist = 1.0;
    for (int y = -1; y <= 1; y++)
    {
        for (int x = -1; x <= 1; x++)
        {
            float2 neighbor = float2(x, y);
            float2 cellPoint = float2(hash(i + neighbor), hash(i + neighbor + 37.0));
            float2 diff = neighbor + cellPoint - f;
            minDist = min(minDist, length(diff));
        }
    }
    return minDist;
}

// ---- Main Pixel Shader ----

float4 PS_ValorAura(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    float radiusNorm = uRadius / 200.0;
    float falloff = 1.0 - smoothstep(0.0, radiusNorm, dist);

    float3 color = float3(0, 0, 0);
    float alpha = 0.0;

    if (uMovement < 1.5)
    {
        // ============================================================
        // MOVEMENT 1 — CALL TO ARMS
        // Bold radial golden waves, scarlet accents, sakura starbursts.
        // Clean, sharp-edged, heroic, radiant.
        // ============================================================

        float pulse = sin(uTime * 4.0) * 0.08 + 1.0;
        float adjustedFalloff = 1.0 - smoothstep(0.0, radiusNorm * pulse, dist);

        // Clean expanding sin-wave rings
        float wave1 = sin(dist * 35.0 - uTime * 6.0) * 0.5 + 0.5;
        float wave2 = sin(dist * 22.0 - uTime * 4.0 + 1.57) * 0.5 + 0.5;
        float waves = wave1 * 0.65 + wave2 * 0.35;

        // 5-petal sakura starburst noise pattern
        float petalShape = cos(angle * 5.0) * 0.5 + 0.5;
        float n = noise(float2(angle * 2.5 + uTime * 0.4, dist * 12.0 - uTime * 1.5));
        float petals = smoothstep(0.35, 0.65, n) * petalShape;

        // Gold dominant, scarlet accents
        float colorMix = sin(angle * 5.0 + uTime * 2.5) * 0.5 + 0.5;
        colorMix = lerp(colorMix, petals, 0.4);
        color = lerp(gold, scarlet, colorMix * 0.4);
        color += phoenixWhite * waves * 0.15 * uHeroIntensity;

        alpha = adjustedFalloff * (waves * 0.5 + 0.5) * uIntensity;
        alpha *= lerp(1.0, petals * 0.7 + 0.3, 0.5);
    }
    else if (uMovement < 2.5)
    {
        // ============================================================
        // MOVEMENT 2 — FUNERAL MARCH
        // Aura contracts and darkens. Crimson dominates with rare
        // golden breaks like sunlight through storm clouds.
        // Slow heavy pulsing. Smoke creeps into edges.
        // ============================================================

        float contractFactor = 0.7;
        float adjustedFalloff = 1.0 - smoothstep(0.0, radiusNorm * contractFactor, dist);

        // Slow heavy pulse with ease-in
        float pulse = sin(uTime * 1.2) * 0.5 + 0.5;
        pulse = pulse * pulse;

        // Smoke-like FBM noise creeping from edges
        float smoke = fbm(float2(angle * 2.0 + uTime * 0.15, dist * 6.0 - uTime * 0.3), 4);
        float smokeEdge = smoothstep(0.3, 0.7, smoke) * (1.0 - adjustedFalloff * 0.5);

        // Rare golden breaks — sunlight through storm clouds
        float goldenBreak = noise(float2(angle * 1.5 + uTime * 0.2, dist * 8.0));
        goldenBreak = smoothstep(0.82, 0.92, goldenBreak);

        color = lerp(crimson, funeralBlack, smokeEdge * 0.6);
        color = lerp(color, gold * 0.7, goldenBreak * pulse * 0.5);

        alpha = adjustedFalloff * (pulse * 0.4 + 0.3) * uIntensity;
        alpha *= 1.0 - smokeEdge * 0.3;
    }
    else if (uMovement < 3.5)
    {
        // ============================================================
        // MOVEMENT 3 — SCHERZO
        // Rapid staccato flashes. Chaotic electric edges.
        // Angular pulse patterns replace smooth waves.
        // Gold and scarlet alternate rapidly. Unpredictable.
        // ============================================================

        // Staccato flicker
        float staccato = step(0.5, frac(uTime * 12.0));

        // Sharp angular pulse patterns
        float angularPulse = step(0.5, frac(angle * 4.0 / 6.28318 + uTime * 3.0));
        float radialBurst = step(0.6, frac(dist * 15.0 - uTime * 8.0));

        // Chaotic noise distortion
        float chaos = noise(float2(angle * 6.0 + uTime * 5.0, dist * 20.0 + uTime * 7.0));
        float electricEdge = smoothstep(0.4, 0.6, chaos);

        // Alternating scarlet/gold on staccato beat
        float3 flashColor = staccato > 0.5 ? gold : scarlet;
        color = lerp(flashColor, phoenixWhite, electricEdge * 0.3 * uHeroIntensity);
        color *= (angularPulse * 0.5 + radialBurst * 0.5) * 0.8 + 0.2;

        alpha = falloff * uIntensity * (0.5 + electricEdge * 0.5);
        alpha *= lerp(0.6, 1.0, staccato);
    }
    else
    {
        // ============================================================
        // MOVEMENT 4 — APOTHEOSIS / ENRAGE
        // White-hot core expanding outward. Cracked-armor pattern
        // where golden light bleeds through dark crimson cracks.
        // At high uHeroIntensity the cracks widen and pulse phoenix-white.
        // ============================================================

        // White-hot core
        float coreDist = smoothstep(0.3, 0.0, dist);

        // Voronoi cracked-armor
        float cracks = voronoi(uv * 8.0 + uTime * 0.3);
        float crackEdge = smoothstep(0.15 + uHeroIntensity * 0.1, 0.05, cracks);
        float crackPulse = sin(uTime * 6.0 + cracks * 10.0) * 0.3 + 0.7;

        // Cracks widen with heroic intensity
        float wideningFactor = lerp(0.12, 0.25, uHeroIntensity);
        float wideCracks = smoothstep(wideningFactor, 0.02, cracks);

        // Core color: phoenix-white bleeding to gold
        float3 coreColor = lerp(phoenixWhite, gold, dist * 2.0);

        // Surface: dark crimson armor with golden light in cracks
        float3 armorColor = lerp(crimson * 0.4, funeralBlack, 0.3);
        float3 crackLight = lerp(gold, phoenixWhite, uHeroIntensity * crackPulse);
        float3 surfaceColor = lerp(armorColor, crackLight, wideCracks);

        color = lerp(surfaceColor, coreColor, coreDist);
        color += phoenixWhite * crackEdge * uHeroIntensity * 0.5;

        alpha = falloff * uIntensity * (0.7 + coreDist * 0.3 + wideCracks * 0.3);
        alpha = saturate(alpha * (1.0 + uHeroIntensity * 0.5));
    }

    return float4(color, 1.0) * saturate(alpha);
}

technique Technique1
{
    pass ValorAura
    {
        PixelShader = compile ps_3_0 PS_ValorAura();
    }
}
