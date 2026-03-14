// ======================================================================
// EroicaPhoenixFlame.fx — Eroica boss attack flash effect
// PhoenixDive and major attacks. Movement-based phase system.
// M1: Rising Phoenix | M2: Funeral Pyre | M3: Starburst | M4: Supernova
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
float uIntensity;
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

float4 PS_PhoenixFlame(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    float3 color = float3(0, 0, 0);
    float alpha = 0.0;

    if (uMovement < 1.5)
    {
        // ============================================================
        // MOVEMENT 1 — RISING PHOENIX WING FIRE
        // Mirrored arcs of gold-to-scarlet flame rising upward.
        // Ascending embers. Classical beauty, symmetrical.
        // ============================================================

        float wingX = abs(centered.x);
        float wingY = centered.y;

        // Symmetrical wing curve
        float wingCurve = wingX * 1.5 - wingY * 0.6 - 0.08;
        float wingMask = smoothstep(0.0, 0.08, wingCurve) * smoothstep(0.55, 0.25, wingCurve);

        // Rising fire — scrolls upward
        float2 fireUV = float2(uv.x * 2.5, uv.y * 3.0 + uTime * 3.5);
        float fire = tex2D(uNoiseTex, fireUV).r;
        float2 fireUV2 = float2(uv.x * 4.0 + 0.3, uv.y * 2.5 + uTime * 5.5);
        float fire2 = tex2D(uNoiseTex, fireUV2).r;
        float fireComposite = fire * 0.6 + fire2 * 0.4;

        // Ascending embers — small bright dots
        float emberSeed = hash(floor(uv * 22.0 + float2(0, uTime * 2.5)));
        float embers = step(0.94, emberSeed);

        // Heat gradient: dark scarlet → scarlet → gold → phoenix-white
        float heat = fireComposite * wingMask * uIntensity;
        if (heat < 0.33)
            color = lerp(scarlet * 0.6, scarlet, heat / 0.33);
        else if (heat < 0.66)
            color = lerp(scarlet, gold, (heat - 0.33) / 0.33);
        else
            color = lerp(gold, phoenixWhite, (heat - 0.66) / 0.34);

        alpha = wingMask * fireComposite * uIntensity;
        alpha += embers * 0.7 * uIntensity;
    }
    else if (uMovement < 2.5)
    {
        // ============================================================
        // MOVEMENT 2 — SMOLDERING FUNERAL PYRE
        // Fire compresses and darkens. Crimson base, few golden sparks.
        // Heavy smoke distortion. Fire falls instead of rises. Mourning.
        // ============================================================

        float pyreMask = smoothstep(0.7, 0.0, dist);

        // Falling fire — scrolls downward (negative Y direction)
        float2 smokeUV = float2(uv.x * 2.0, uv.y * 2.0 - uTime * 1.2);
        float smoke = tex2D(uNoiseTex, smokeUV).r;
        float2 smokeUV2 = float2(uv.x * 3.0 + 0.7, uv.y * 1.5 - uTime * 0.8);
        float smoke2 = tex2D(uNoiseTex, smokeUV2).r;
        float smokeComposite = smoke * 0.5 + smoke2 * 0.5;

        // Heavy billowing smoke distortion
        float distortion = fbm(uv * 4.0 + uTime * 0.3, 3) * 0.15;
        float distortedSmoke = tex2D(uNoiseTex, uv * 2.0 + distortion).r;

        // Few golden sparks — rare memories
        float sparkSeed = hash(floor(uv * 18.0 + float2(uTime * 0.3, 0)));
        float sparks = step(0.975, sparkSeed);

        // Dark crimson, compressed and heavy
        float heat = smokeComposite * pyreMask * uIntensity * 0.6;
        color = lerp(funeralBlack, crimson, heat);
        color = lerp(color, gold * 0.5, sparks * 0.6);
        color += crimson * distortedSmoke * 0.15;

        alpha = pyreMask * smokeComposite * uIntensity * 0.7;
        alpha += sparks * 0.4;
    }
    else if (uMovement < 3.5)
    {
        // ============================================================
        // MOVEMENT 3 — EXPLOSIVE STARBURST
        // Sharp radial lines of alternating gold/scarlet pulsing fast.
        // Lightning-crack patterns. Electric and chaotic.
        // ============================================================

        // Radial line star pattern
        float radialLines = abs(sin(angle * 8.0 + uTime * 6.0));
        radialLines = smoothstep(0.3, 0.8, radialLines);

        // High-frequency lightning cracks
        float crackNoise = noise(float2(angle * 10.0 + uTime * 8.0, dist * 25.0 + uTime * 12.0));
        float cracks = smoothstep(0.55, 0.85, crackNoise);

        // Rapid alternating pulse
        float rapidPulse = sin(uTime * 18.0) * 0.5 + 0.5;
        float burstFalloff = smoothstep(0.6, 0.0, dist);

        // Alternating gold/scarlet on rapid pulse
        float3 pulseColor = lerp(scarlet, gold, rapidPulse);
        color = pulseColor * radialLines * burstFalloff;
        color += phoenixWhite * cracks * 0.6;
        color += gold * burstFalloff * 0.2;

        alpha = (radialLines * burstFalloff + cracks * 0.5) * uIntensity;
        alpha *= 0.7 + rapidPulse * 0.3;
    }
    else
    {
        // ============================================================
        // MOVEMENT 4 — SUPERNOVA EXPLOSION
        // Concentric rings: white → gold → scarlet → crimson expanding.
        // Maximum brightness at center. Screen-burning intensity.
        // The hero's final flame.
        // ============================================================

        float ringWidth = 0.08 + uHeroIntensity * 0.06;

        // Three concentric expanding rings offset by 1/3 phase
        float ring1Radius = frac(uTime * 1.5);
        float ring1 = smoothstep(ring1Radius + ringWidth, ring1Radius, dist)
                     - smoothstep(ring1Radius, ring1Radius - ringWidth, dist);

        float ring2Radius = frac(uTime * 1.5 + 0.33);
        float ring2 = smoothstep(ring2Radius + ringWidth, ring2Radius, dist)
                     - smoothstep(ring2Radius, ring2Radius - ringWidth, dist);

        float ring3Radius = frac(uTime * 1.5 + 0.66);
        float ring3 = smoothstep(ring3Radius + ringWidth, ring3Radius, dist)
                     - smoothstep(ring3Radius, ring3Radius - ringWidth, dist);

        float rings = saturate(ring1 + ring2 + ring3);

        // Nuclear-bright core
        float coreBright = smoothstep(0.3, 0.0, dist);
        coreBright = coreBright * coreBright;

        // Fire noise overlay
        float fireNoise = tex2D(uNoiseTex, uv * 3.0 + uTime * 2.0).r;

        // Color by distance: white core → gold → scarlet → crimson outer
        if (dist < 0.15)
            color = lerp(phoenixWhite, gold, dist / 0.15);
        else if (dist < 0.3)
            color = lerp(gold, scarlet, (dist - 0.15) / 0.15);
        else
            color = lerp(scarlet, crimson, saturate((dist - 0.3) / 0.3));

        color += phoenixWhite * coreBright * (0.5 + uHeroIntensity * 0.5);
        color += gold * rings * 0.4;

        // Screen-burning intensity at maximum heroic energy
        float intensity = uIntensity * (1.0 + uHeroIntensity * 0.8);
        alpha = (coreBright + rings * 0.7 + fireNoise * 0.2) * intensity;
    }

    return float4(color, 1.0) * saturate(alpha);
}

technique Technique1
{
    pass PhoenixFlame
    {
        PixelShader = compile ps_3_0 PS_PhoenixFlame();
    }
}
