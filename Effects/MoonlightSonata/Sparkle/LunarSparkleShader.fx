// LunarSparkleShader.fx — Moonlight Sonata Theme Sparkle Shader
//
// Transforms star sprites into soft, lunar crescent-shimmer sparkles.
// Visual identity: moonlit, ethereal, crescent-shaped light play.
//
// UNIQUE FEATURES (vs other theme sparkles):
// - Crescent-phase modulation: sparkle brightness follows a waxing/waning 
//   crescent pattern instead of simple angular facets
// - Tidal wave shimmer: slow, ocean-like brightness undulation
// - Cool purple-blue prismatic fringe: lunar color temperature
// - Soft, dreamy flash peaks (lower exponent = gentler twinkle)
// - Moon-phase rotating shadow mask that sweeps across the sparkle

sampler uImage0 : register(s0);

float uTime;
float flashPhase;
float flashSpeed;
float flashPower;
float baseAlpha;
float shimmerIntensity;
float3 primaryColor;    // Violet/IceBlue
float3 accentColor;     // Lavender/CrescentGlow
float3 highlightColor;  // MoonWhite

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float4 LunarSparklePS(float4 screenspace : TEXCOORD0) : COLOR0
{
    float2 uv = screenspace.xy;
    float4 texColor = tex2D(uImage0, uv);
    if (texColor.a < 0.01) return float4(0, 0, 0, 0);

    float2 centered = uv - 0.5;
    float dist = length(centered);
    float angle = atan2(centered.y, centered.x);

    // ---- CRESCENT-PHASE MODULATION ----
    // Instead of raw angular facets, the sparkle has a crescent shadow
    // that rotates slowly, making brightness wax and wane like moon phases
    float crescentAngle = angle + uTime * flashSpeed * 0.5 + flashPhase;
    float crescentPhase = smoothstep(-0.2, 0.6, sin(crescentAngle));
    
    // 4-point star alignment with crescent overlay
    float starPattern = sin(angle * 4.0 + uTime * flashSpeed * 0.8 + flashPhase) * 0.5 + 0.5;
    float lunarFacet = starPattern * crescentPhase;

    // ---- TIDAL WAVE SHIMMER ----
    // Slow, rolling brightness undulation like moonlit water
    float tide = sin(dist * 8.0 - uTime * flashSpeed * 0.6 + flashPhase * 0.5) * 0.5 + 0.5;
    float tidalShimmer = lerp(0.7, 1.0, tide * shimmerIntensity);

    // ---- COMBINED SHIMMER ----
    float shimmer = lunarFacet * 0.6 + tidalShimmer * 0.4;
    shimmer = lerp(1.0, shimmer, shimmerIntensity);

    // ---- GENTLE FLASH PEAKS ----
    // Moonlight sparkles are softer than other themes — lower power exponent
    float gentleFlash = pow(saturate(starPattern * crescentPhase), max(flashPower * 0.6, 2.0));
    float globalFlash = sin(uTime * flashSpeed * 2.0 + flashPhase);
    globalFlash = pow(saturate(globalFlash), max(flashPower * 0.4, 1.5));

    // ---- COOL PRISMATIC FRINGE ----
    // Restricted to purple-blue-silver hue range (moonlit colors only)
    float hue = frac(0.6 + (angle + 3.14159) / 6.28318 * 0.15 + uTime * flashSpeed * 0.05);
    float3 lunarPrism = hsv2rgb(float3(hue, 0.25, 1.0));
    float prismStrength = smoothstep(0.12, 0.32, dist) * smoothstep(0.5, 0.36, dist) * 0.35;

    // ---- SOFT RADIAL BLOOM ----
    float bloom = smoothstep(0.5, 0.0, dist);
    float innerBloom = smoothstep(0.2, 0.0, dist);

    // ---- BREATHING PULSE (slow, dreamy) ----
    float pulse = 0.85 + 0.15 * sin(uTime * flashSpeed * 0.8 + flashPhase * 1.5);
    float centerGlow = smoothstep(0.25, 0.0, dist) * pulse;

    // ---- COLOR COMPOSITE ----
    float luminance = dot(texColor.rgb, float3(0.299, 0.587, 0.114));

    // Lunar-tinted base with crescent modulation
    float3 baseLayer = lerp(primaryColor, accentColor, shimmer * 0.4 + crescentPhase * 0.2) * luminance * shimmer;

    // Cool prismatic lunar fringe
    float3 prismLayer = lunarPrism * prismStrength * luminance;

    // Gentle dazzle at crescent peaks
    float3 dazzleLayer = highlightColor * gentleFlash * 1.0;

    // Global flash — softer breathing glow
    float3 flashLayer = lerp(highlightColor, accentColor, 0.3) * globalFlash * innerBloom * 0.4;

    // Center glow — cool moonlight
    float3 glowLayer = lerp(primaryColor, highlightColor, 0.5) * centerGlow * 0.4;

    float3 finalColor = baseLayer + prismLayer + dazzleLayer + flashLayer + glowLayer;
    finalColor *= 1.3;

    float finalAlpha = texColor.a * baseAlpha * (shimmer * 0.25 + 0.75);
    return float4(finalColor * finalAlpha, finalAlpha);
}

technique Technique1
{
    pass LunarSparklePass
    {
        PixelShader = compile ps_3_0 LunarSparklePS();
    }
}
