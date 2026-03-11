// PrismaticSparkleShader.fx — Swan Lake Theme Sparkle Shader
//
// Transforms star sprites into iridescent, feather-light prismatic sparkles.
// Visual identity: elegant, graceful, monochrome core with rainbow prismatic fringes.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Full-spectrum prismatic refraction: strongest rainbow effect of all themes,
//   with iridescent color shifting around the entire sparkle edge
// - Ballet rotation: sparkle facets rotate in graceful 3/4 waltz time
// - Monochrome-to-prismatic gradient: pure silver/white center bleeds into
//   rainbow at edges (like light through a diamond or prismatic feather)
// - Feather-soft edge falloff: extremely gradual, graceful fade
// - Dual-polarity shimmer: alternating bright/dark bands for black/white swan duality

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // PureWhite/Silver
float3 accentColor;     // PrismaticShimmer/IcyBlue
float3 highlightColor;  // RainbowFlash white

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 PrismaticSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float4 texColor = tex2D(uImage0, uv);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // ---- BALLET WALTZ ROTATION (3/4 time) ----
    // Facets rotate in graceful waltz timing — 0.1047 rad/frame ≈ 3/4 time at 60fps
    float waltzAngle = angle + uTime * 0.1047 * flashSpeed + flashPhase;
    
    // ---- 4-POINT STAR WITH GRACEFUL ROTATION ----
    float star4 = sin(waltzAngle * 4.0) * 0.5 + 0.5;
    
    // ---- 6-POINT SECONDARY (crystalline facets like diamond) ----
    float star6 = sin(waltzAngle * 6.0 + uTime * flashSpeed * 0.3) * 0.5 + 0.5;

    // ---- DUAL-POLARITY SHIMMER (black/white swan duality) ----
    // Alternating bright and dark bands at 2-fold symmetry
    float polarity = sin(angle * 2.0 + uTime * flashSpeed * 0.4 + 1.57) * 0.5 + 0.5;
    float dualShimmer = lerp(0.4, 1.0, polarity); // Dark swan → white swan transition

    // ---- COMBINED SHIMMER ----
    float shimmer = (star4 * 0.45 + star6 * 0.35 + dualShimmer * 0.2);
    shimmer = lerp(1.0, shimmer, shimmerIntensity);

    // ---- GRACEFUL FLASH PEAKS ----
    float gracefulFlash = pow(saturate(star4 * star6), flashPower);
    float globalFlash = sin(uTime * flashSpeed * 2.5 + flashPhase);
    globalFlash = pow(saturate(globalFlash), flashPower * 0.5);

    // ---- FULL-SPECTRUM PRISMATIC REFRACTION ----
    // Swan Lake has the STRONGEST rainbow effect of any theme
    // Full hue rotation around the sparkle edge — like a diamond catching light
    float hue = frac((angle + 3.14159) / 6.28318 + uTime * flashSpeed * 0.08 + flashPhase * 0.2);
    float3 rainbowPrism = hsv2rgb(float3(hue, 0.55, 1.0));

    // Rainbow strength increases with distance from center (edge refraction)
    float rainbowStrength = smoothstep(0.08, 0.35, dist) * smoothstep(0.5, 0.32, dist);
    
    // Secondary subtler rainbow at inner zone
    float hue2 = frac(hue + 0.3 + uTime * flashSpeed * 0.03);
    float3 innerRainbow = hsv2rgb(float3(hue2, 0.3, 1.0));
    float innerRainbowStrength = smoothstep(0.0, 0.15, dist) * smoothstep(0.25, 0.12, dist);

    // ---- FEATHER-SOFT EDGE FALLOFF ----
    // Extremely gradual, elegant fade — befitting Swan Lake's grace
    float bloom = smoothstep(0.5, 0.0, dist * 0.9);
    float innerBloom = smoothstep(0.22, 0.0, dist);

    // ---- GRACEFUL BREATHING ----
    float breath = 0.88 + 0.12 * sin(uTime * flashSpeed * 1.2 + flashPhase * 1.8);
    float centerGlow = smoothstep(0.22, 0.0, dist) * breath;

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Monochrome silver core — pure and elegant, like a white swan
    float3 monoCore = lerp(primaryColor, highlightColor, innerBloom * 0.3) * luminance * shimmer;

    // Full rainbow prismatic edge — strongest of any theme
    float3 prismEdge = rainbowPrism * rainbowStrength * luminance * 0.55;
    float3 prismInner = innerRainbow * innerRainbowStrength * luminance * 0.25;

    // Graceful flash diamonds at facet crossings
    float3 dazzleLayer = highlightColor * gracefulFlash * 1.2;

    // Breathing global flash — soft, elegant
    float3 flashLayer = lerp(accentColor, highlightColor, 0.5) * globalFlash * innerBloom * 0.4;

    // Pristine center glow
    float3 glowLayer = highlightColor * centerGlow * 0.4;

    // Swan duality overlay — dark accents at dark-polarity zones
    float3 darkAccent = float3(0.06, 0.06, 0.09) * (1.0 - dualShimmer) * luminance * 0.15;

    float3 finalColor = monoCore + prismEdge + prismInner + dazzleLayer + flashLayer + glowLayer - darkAccent;
    finalColor *= 1.35;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.2 + 0.8);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass PrismaticSparklePass
    {
        PixelShader = compile ps_3_0 PrismaticSparklePS();
    }
}
