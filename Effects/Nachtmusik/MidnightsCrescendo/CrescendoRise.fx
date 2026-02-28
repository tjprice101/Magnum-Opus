// =============================================================================
// Midnight's Crescendo — Rising Crescendo Trail Shader
// =============================================================================
// A trail that visually intensifies along its length — starts dim and
// builds to a blazing climax, mirroring the musical crescendo.
// The trail literally gets brighter, wider-feeling, and more energetic
// toward the head, with ascending star sparks embedded in the flow.
//
// Techniques:
//   CrescendoRise      – Main trail with intensity-building gradient
//   CrescendoRiseGlow  – Softer bloom pass
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

float QuadraticBump(float x) { return x * (4.0 - x * 4.0); }
float4 ApplyOverbright(float3 c, float a) { return float4(c * uOverbrightMult, a); }

// Ascending spark field: bright points that rise upward
float AscendingSparks(float2 uv, float time)
{
    float2 scrollUV = uv;
    scrollUV.y += time * 0.8; // Sparks rise upward
    
    float2 cell = floor(scrollUV * 6.0);
    float hash = frac(sin(dot(cell, float2(127.1, 311.7))) * 43758.5453);
    
    // Quick flash sparks
    float spark = sin(hash * 6.28 + time * (4.0 + hash * 6.0));
    spark = saturate(spark * 2.0 - 1.0); // Only the peaks
    spark *= step(0.75, hash); // 25% of cells
    
    return spark;
}

float4 CrescendoRisePS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Crescendo effect: intensity increases from tail (x=1) to head (x=0)
    float crescendoT = saturate(1.0 - coords.x); // 0 at tail, 1 at head
    crescendoT = crescendoT * crescendoT; // Exponential rise
    
    // Gentle musical wave distortion (more intense at head)
    float wave1 = sin(coords.x * 6.0 + uTime * uScrollSpeed * 3.0) * uDistortionAmt * crescendoT;
    float wave2 = sin(coords.x * 12.0 - uTime * uScrollSpeed * 2.0) * uDistortionAmt * 0.4 * crescendoT;
    
    float2 distortedUV = coords;
    distortedUV.y += wave1;
    distortedUV.x += wave2;
    
    float4 baseTex = tex2D(uImage0, distortedUV);
    
    // Noise
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x -= uTime * uSecondaryTexScroll;
    noiseUV.y -= uTime * 0.15; // Rising motion
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.7, noiseTex.r, uHasSecondaryTex);
    
    float edgeFade = QuadraticBump(coords.y);
    float trailFade = saturate(1.0 - coords.x * 1.05);
    
    // Color: dark at tail, brilliant at head
    float gradientT = coords.x * 0.5 + (1.0 - crescendoT) * 0.3 + noiseVal * 0.2;
    float3 trailColor = lerp(uSecondaryColor, uColor, gradientT);
    
    // Brilliant white-gold core at head (the climax)
    float coreMask = saturate((edgeFade - 0.5) * 3.5);
    float climaxCore = coreMask * crescendoT;
    trailColor = lerp(trailColor, float3(1.0, 0.97, 0.9), climaxCore * 0.7);
    
    // Ascending sparks embedded in the trail
    float sparks = AscendingSparks(distortedUV * 2.0, uTime);
    trailColor = lerp(trailColor, float3(1.0, 0.95, 0.85), sparks * 0.5 * crescendoT);
    
    // Crescendo brightening: overall intensity rises toward head
    float crescendoBright = 0.6 + crescendoT * 0.4;
    
    // Musical rhythm pulse
    float pulse = sin(uTime * 5.0 + coords.x * 4.0) * 0.06 * crescendoT + 0.94;
    
    float3 finalColor = trailColor * baseTex.rgb * uIntensity * crescendoBright * pulse;
    finalColor *= 0.65 + noiseVal * 0.35;
    
    float alpha = edgeFade * trailFade * uOpacity * sampleColor.a * baseTex.a;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 CrescendoRiseGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float crescendoT = saturate(1.0 - coords.x);
    crescendoT *= crescendoT;
    
    float wave = sin(coords.x * 4.0 + uTime * uScrollSpeed * 1.5) * uDistortionAmt * 0.3;
    float2 glowUV = coords;
    glowUV.y += wave;
    
    float4 baseTex = tex2D(uImage0, glowUV);
    float edgeFade = QuadraticBump(coords.y);
    float softEdge = edgeFade * edgeFade;
    float trailFade = saturate(1.0 - coords.x * 0.9);
    
    float3 glowColor = lerp(uColor, uSecondaryColor, coords.x * 0.5);
    
    float2 noiseUV = coords * uSecondaryTexScale * 0.6;
    noiseUV.x -= uTime * uSecondaryTexScroll * 0.4;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.85, noiseTex.r, uHasSecondaryTex * 0.4);
    
    glowColor *= uIntensity * noiseVal * baseTex.rgb * (0.7 + crescendoT * 0.3);
    
    float pulse = sin(uTime * 2.5) * 0.06 + 0.94;
    float alpha = softEdge * trailFade * uOpacity * sampleColor.a * baseTex.a * pulse;
    
    return ApplyOverbright(glowColor, alpha);
}

technique CrescendoRise
{
    pass P0 { PixelShader = compile ps_3_0 CrescendoRisePS(); }
}

technique CrescendoRiseGlow
{
    pass P0 { PixelShader = compile ps_3_0 CrescendoRiseGlowPS(); }
}
