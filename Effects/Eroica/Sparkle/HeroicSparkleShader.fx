// HeroicSparkleShader.fx — Eroica Theme Sparkle Shader
//
// Transforms star sprites into heroic ember-flash sparkles.
// Visual identity: fiery courage, sakura petals, golden triumph.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Ember pulse modulation: sparkles flicker like burning embers with
//   irregular, flame-like intensity variations
// - Ascending heat shimmer: brightness rises from bottom to top (like flames)
// - Warm scarlet-gold-sakura color ramp driven by intensity
// - Sharp heroic flash peaks (high exponent = dramatic burst)
// - Sakura petal interference pattern: organic five-fold symmetry overlay

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // Scarlet/Crimson
float3 accentColor;     // Gold/Sakura
float3 highlightColor;  // HotCore white-gold

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 HeroicSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float4 texColor = tex2D(uImage0, uv);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // ---- EMBER PULSE MODULATION ----
    // Irregular, flame-like flickering — multiple overlapping sin waves
    // with different frequencies create an organic ember feel
    float ember1 = sin(uTime * flashSpeed * 3.7 + flashPhase * 1.3) * 0.5 + 0.5;
    float ember2 = sin(uTime * flashSpeed * 5.1 + flashPhase * 2.7 + 1.4) * 0.5 + 0.5;
    float ember3 = sin(uTime * flashSpeed * 2.3 + flashPhase * 0.8 + 3.1) * 0.5 + 0.5;
    float emberFlicker = ember1 * 0.4 + ember2 * 0.35 + ember3 * 0.25;

    // ---- 4-POINT STAR WITH HEROIC FLARE ----
    float starPattern = sin(angle * 4.0 + uTime * flashSpeed * 1.5 + flashPhase) * 0.5 + 0.5;
    
    // ---- SAKURA PETAL INTERFERENCE (5-fold organic symmetry) ----
    float sakura5 = sin(angle * 5.0 - uTime * flashSpeed * 0.7 + flashPhase * 2.1) * 0.5 + 0.5;
    float petalOverlay = pow(saturate(sakura5), 2.0);

    // ---- ASCENDING HEAT SHIMMER ----
    // Brightness rises from bottom to top, like flames ascending
    float heatRise = smoothstep(0.7, 0.2, centered.y + 0.5); // Brighter toward top
    float heatShimmer = lerp(0.6, 1.0, heatRise * shimmerIntensity);

    // ---- COMBINED SHIMMER ----
    float shimmer = (starPattern * 0.4 + petalOverlay * 0.3 + emberFlicker * 0.3) * heatShimmer;
    shimmer = lerp(1.0, shimmer, shimmerIntensity);

    // ---- DRAMATIC FLASH PEAKS ----
    // Eroica sparkles have sharp, heroic bursts — high power exponent
    float heroicFlash = pow(saturate(starPattern * emberFlicker), flashPower);
    float globalFlash = sin(uTime * flashSpeed * 3.5 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.7);

    // ---- WARM HEROIC PRISMATIC ----
    // Restricted to scarlet-gold-sakura hue range (warm heroic colors)
    float hue = frac(0.0 + (angle + 3.14159) / 6.28318 * 0.12 + uTime * flashSpeed * 0.04);
    float3 heroicPrism = hsv2rgb(float3(hue, 0.5, 1.0));
    float prismStrength = smoothstep(0.1, 0.3, dist) * smoothstep(0.5, 0.35, dist) * 0.3;

    // ---- RADIAL BLOOM WITH EMBER FALLOFF ----
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.22, 0.0, dist);

    // ---- PULSING CORE (heroic heartbeat) ----
    float heartbeat = 0.8 + 0.2 * sin(uTime * flashSpeed * 2.0 + flashPhase * 2.5);
    float centerGlow = smoothstep(0.28, 0.0, dist) * heartbeat;

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Ember-tinted base: scarlet → gold → sakura based on intensity
    float3 emberColor = lerp(primaryColor, accentColor, shimmer * 0.5 + emberFlicker * 0.3);
    float3 baseLayer = emberColor * luminance * shimmer;

    // Warm prismatic edge
    float3 prismLayer = heroicPrism * prismStrength * luminance * 0.4;

    // Heroic flash bursts
    float3 dazzleLayer = highlightColor * heroicFlash * 1.5;

    // Ember glow flash
    float3 flashLayer = lerp(primaryColor, highlightColor, 0.4) * globalFlash * innerBloom * 0.5;

    // Hot center core
    float3 glowLayer = lerp(accentColor, highlightColor, 0.6) * centerGlow * 0.5;

    float3 finalColor = baseLayer + prismLayer + dazzleLayer + flashLayer + glowLayer;
    finalColor *= 1.4;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.3 + 0.7);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass HeroicSparklePass
    {
        PixelShader = compile ps_3_0 HeroicSparklePS();
    }
}
