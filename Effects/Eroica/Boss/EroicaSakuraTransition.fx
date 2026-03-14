// ======================================================================
// EroicaSakuraTransition.fx — Eroica boss phase transition effect
// Sakura petal transitions between symphony movements.
// To M2: Farewell tears | To M3: Petals ignite | To M4: Apotheosis ring
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
float uTransitionProgress;  // 0 = not started, 1 = complete
float4 uFromColor;
float4 uToColor;
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

float4 PS_SakuraTransition(float2 uv : TEXCOORD0) : COLOR0
{
    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);
    float progress = uTransitionProgress;

    // 5-petal sakura silhouette mask (shared across all transitions)
    float petalAngle = fmod(abs(angle + 3.14159), 6.28318 / 5.0) - 3.14159 / 5.0;
    float petalDist = cos(petalAngle * 2.5) * 0.4;
    float petalMask = smoothstep(petalDist + 0.12, petalDist - 0.02, dist);

    float3 color = float3(0, 0, 0);
    float alpha = 0.0;

    if (uMovement < 2.5)
    {
        // ============================================================
        // TRANSITION TO MOVEMENT 2 — FAREWELL
        // Sakura petals scatter and drift slowly downward (not upward).
        // Golden edge glow fading to crimson. Petals falling like tears.
        // ============================================================

        // Dissolve noise with slow downward drift
        float n = tex2D(uNoiseTex, uv * 2.0 + float2(uTime * 0.1, uTime * 0.4)).r;
        float dissolveThreshold = progress * 1.3;
        float revealed = step(n, dissolveThreshold);

        // Falling petal particles — drift downward with gentle sway
        float fallSeed = hash(floor(uv * 15.0));
        float fallOffset = uTime * 0.8 + fallSeed * 2.0;
        float2 petalUV = uv + float2(sin(fallOffset * 1.5) * 0.03, frac(fallOffset * 0.2));
        float fallingPetals = hash(floor(petalUV * 20.0));
        fallingPetals = step(0.93, fallingPetals) * progress;

        // Edge glow: gold fading to crimson as transition progresses
        float edgeDist = abs(n - dissolveThreshold);
        float edgeGlow = smoothstep(0.1, 0.0, edgeDist);
        float3 edgeColor = lerp(gold, crimson, progress);

        // Sakura pink → crimson farewell
        color = lerp(sakuraPink, crimson * 0.7, progress * 0.6);
        color = lerp(color, edgeColor, edgeGlow * 0.8);
        color += sakuraPink * fallingPetals * 0.6;

        alpha = (revealed * 0.25 + edgeGlow * uIntensity * 1.5 + fallingPetals * 0.5) * petalMask;
        alpha *= sin(progress * 3.14159); // Fade in then out
    }
    else if (uMovement < 3.5)
    {
        // ============================================================
        // TRANSITION TO MOVEMENT 3 — PETALS IGNITE
        // Each petal catches fire. Noise burns through petal shapes.
        // Violent and sudden. Gold lightning cracks.
        // ============================================================

        // Aggressive burn noise — eating through petal shapes
        float burnNoise = tex2D(uNoiseTex, uv * 3.0 + float2(uTime * 1.5, uTime * 0.8)).r;
        float burnNoise2 = tex2D(uNoiseTex, uv * 5.0 - float2(uTime * 2.0, uTime * 1.2)).r;
        float burn = burnNoise * 0.6 + burnNoise2 * 0.4;

        // Burn threshold advances violently fast
        float burnThreshold = progress * 1.5;
        float burning = smoothstep(burnThreshold, burnThreshold - 0.15, burn);

        // Fire edge — hot white-gold boundary
        float burnEdge = abs(burn - burnThreshold);
        float fireEdge = smoothstep(0.08, 0.0, burnEdge);

        // Gold lightning cracks radiating outward
        float crackNoise = noise(float2(angle * 6.0 + uTime * 10.0, dist * 15.0));
        float cracks = smoothstep(0.7, 0.9, crackNoise) * progress;

        // Igniting petal color: pink → scarlet as they burn
        color = lerp(sakuraPink, scarlet, burn * progress);
        color = lerp(color, gold, fireEdge * 0.9);
        color += phoenixWhite * cracks * 0.7;
        color += gold * fireEdge * 0.3;

        alpha = (burning * 0.4 + fireEdge * uIntensity * 2.0 + cracks * 0.5) * petalMask;
        alpha *= smoothstep(0.0, 0.15, progress) * smoothstep(1.0, 0.85, progress);
    }
    else
    {
        // ============================================================
        // TRANSITION TO MOVEMENT 4 — APOTHEOSIS
        // Expanding ring of white-gold fire consuming the screen from
        // center. Inside: pure phoenix-white. Outside: crimson darkness.
        // The hero transcends.
        // ============================================================

        // Expanding ring driven by transition progress
        float ringRadius = progress * 0.8;
        float ringWidth = 0.06 + progress * 0.04;

        float ring = smoothstep(ringRadius + ringWidth, ringRadius, dist)
                   - smoothstep(ringRadius, max(0.001, ringRadius - ringWidth), dist);

        // Inside the ring: pure phoenix-white radiance
        float insideRing = smoothstep(ringRadius + 0.02, ringRadius - 0.05, dist);

        // Outside: crimson darkness
        float outsideDarkness = smoothstep(ringRadius - 0.02, ringRadius + 0.15, dist);

        // Fire noise on ring edge for organic feel
        float fireNoise = tex2D(uNoiseTex, uv * 4.0 + uTime * 2.0).r;
        float ringFire = ring * (0.7 + fireNoise * 0.3);

        // Color layers: inside transcendence → ring fire → outside darkness
        float3 insideColor = lerp(phoenixWhite, gold, dist * 1.5);
        float3 ringColor = lerp(gold, phoenixWhite, 0.5 + uHeroIntensity * 0.5);
        float3 outsideColor = lerp(crimson * 0.3, funeralBlack, outsideDarkness);

        color = outsideColor;
        color = lerp(color, ringColor, ringFire);
        color = lerp(color, insideColor, insideRing * progress);

        alpha = insideRing * progress * 0.6 + ringFire * uIntensity * 2.0 + outsideDarkness * 0.1;
        alpha *= smoothstep(0.0, 0.1, progress);
    }

    return float4(color, 1.0) * saturate(alpha);
}

technique Technique1
{
    pass SakuraTransition
    {
        PixelShader = compile ps_3_0 PS_SakuraTransition();
    }
}
