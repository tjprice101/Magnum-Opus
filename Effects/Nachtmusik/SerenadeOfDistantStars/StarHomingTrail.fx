// =============================================================================
// Serenade of Distant Stars — Star Homing Trail Shader
// =============================================================================
// Homing star projectile trail — graceful arcing streaks with twinkling
// stars embedded in a flowing ribbon. Each star projectile leaves a unique
// ephemeral trail showing its flight path through the night sky.
//
// UV Layout: Standard trail strip [0..1] x [0..1]
//
// Techniques:
//   StarHomingTrail – Arcing star-studded ribbon trail
//   StarHomingGlow – Soft starlight halo bloom
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1);

float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uTime;
float uIntensity;

float uOverbrightMult;
float uScrollSpeed;
float uDistortionAmt;
float uHasSecondaryTex;
float uSecondaryTexScale;
float uSecondaryTexScroll;

float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Twinkling star function — sharp, bright points
float StarTwinkle(float2 uv, float time, float seed)
{
    float x = frac(sin(dot(uv * 30.0 + seed, float2(12.9898, 78.233))) * 43758.5453);
    float y = frac(sin(dot(uv * 30.0 + seed, float2(39.346, 11.135))) * 43758.5453);
    
    float2 starPos = float2(x, y);
    float d = length(frac(uv * 5.0) - starPos);
    float twinkle = sin(time * 4.0 + x * 20.0) * 0.5 + 0.5;
    float star = exp(-d * d * 200.0) * twinkle;
    
    return star;
}

float4 StarHomingTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = coords.y - 0.5;
    float absCross = abs(cross);
    
    // Noise for organic flow
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV += float2(uTime * uSecondaryTexScroll, -uTime * uSecondaryTexScroll * 0.5);
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.6, noiseTex.r, uHasSecondaryTex);
    
    // Core ribbon — thin, graceful
    float ribbon = exp(-absCross * absCross * 30.0);
    
    // Slight wave distortion for graceful arcing
    float wave = sin(progress * 10.0 + uTime * uScrollSpeed * 3.0) * 0.02 * uDistortionAmt;
    float ribbonOffset = absCross + wave;
    ribbon = exp(-ribbonOffset * ribbonOffset * 30.0);
    
    // Embedded twinkling stars along the trail
    float stars = StarTwinkle(coords, uTime * uScrollSpeed, 0.0);
    stars += StarTwinkle(coords * 1.3, uTime * uScrollSpeed * 0.8, 5.0) * 0.6;
    
    // Trail fade: quick bright head, graceful taper
    float headFade = smoothstep(0.0, 0.08, progress);
    float tailFade = pow(smoothstep(1.0, 0.15, progress), 0.8);
    
    float alpha = (ribbon + stars * 0.5) * headFade * tailFade * uOpacity * sampleColor.a * baseTex.a;
    alpha *= noiseVal * 0.5 + 0.5;
    
    // Color: starlight gold core fading to night-sky blue edges
    float3 coreColor = uSecondaryColor; // Gold
    float3 edgeColor = uColor; // Indigo/blue
    float colorT = absCross * 3.0;
    float3 trailColor = lerp(coreColor, edgeColor, saturate(colorT));
    
    // Stars are pure white
    float3 starWhite = float3(1.0, 0.98, 0.95);
    trailColor = lerp(trailColor, starWhite, stars * 0.7);
    
    float3 finalColor = trailColor * uIntensity * baseTex.rgb;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 StarHomingGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = abs(coords.y - 0.5);
    
    // Broad soft starlight halo
    float glow = exp(-cross * cross * 5.0);
    float fade = smoothstep(0.0, 0.1, progress) * smoothstep(1.0, 0.3, progress);
    
    float alpha = glow * fade * uOpacity * 0.4 * sampleColor.a * baseTex.a;
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.3) * uIntensity * 0.5;
    
    return ApplyOverbright(glowColor * baseTex.rgb, alpha);
}

technique StarHomingTrail
{
    pass P0 { PixelShader = compile ps_3_0 StarHomingTrailPS(); }
}

technique StarHomingGlow
{
    pass P0 { PixelShader = compile ps_3_0 StarHomingGlowPS(); }
}
