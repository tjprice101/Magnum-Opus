// =============================================================================
// Nebula's Whisper — Nebula Scatter Shader
// =============================================================================
// Splitting nebula projectile trail — soft gaseous clouds that fracture
// and scatter as projectiles split apart. Dreamy, atmospheric feel.
//
// UV Layout: Standard trail strip [0..1] x [0..1]
//
// Techniques:
//   NebulaScatterTrail – Gaseous nebula cloud trail
//   NebulaScatterGlow – Soft nebula bloom overlay
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

float NebulaCloud(float2 uv, float time)
{
    // Multi-octave cloud pattern
    float cloud = 0.0;
    float2 p = uv;
    
    // Layer 1: slow-rolling base cloud
    float2 uv1 = p * 2.0 + float2(time * 0.12, time * 0.08);
    cloud += (sin(uv1.x * 3.0 + sin(uv1.y * 2.7)) * 0.5 + 0.5) * 0.5;
    
    // Layer 2: medium wispy detail
    float2 uv2 = p * 4.0 + float2(-time * 0.18, time * 0.14);
    cloud += (sin(uv2.x * 5.0 + cos(uv2.y * 3.8)) * 0.5 + 0.5) * 0.3;
    
    // Layer 3: fine scatter detail
    float2 uv3 = p * 8.0 + float2(time * 0.25, -time * 0.2);
    cloud += (sin(uv3.x * 7.0 + sin(uv3.y * 6.1)) * 0.5 + 0.5) * 0.2;
    
    return cloud;
}

float4 NebulaScatterTrailPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = coords.y - 0.5;
    
    // Secondary noise for distortion
    float2 noiseUV = coords * uSecondaryTexScale;
    noiseUV.x += uTime * uSecondaryTexScroll;
    noiseUV.y -= uTime * uSecondaryTexScroll * 0.7;
    float4 noiseTex = tex2D(uImage1, noiseUV);
    float noiseVal = lerp(0.5, noiseTex.r, uHasSecondaryTex);
    
    // Apply UV distortion (gaseous billow)
    float2 distortedUV = coords;
    distortedUV.x += sin(coords.y * 8.0 + uTime * uScrollSpeed) * uDistortionAmt * 0.5;
    distortedUV.y += cos(coords.x * 6.0 + uTime * uScrollSpeed * 0.8) * uDistortionAmt * 0.3;
    
    // Nebula cloud field
    float cloud = NebulaCloud(distortedUV, uTime * uScrollSpeed);
    cloud *= noiseVal + 0.3;
    
    // Fade: head is bright, tail wisps away
    float headFade = smoothstep(0.0, 0.2, progress);
    float tailFade = smoothstep(1.0, 0.4, progress);
    float edgeFade = 1.0 - abs(cross) * 2.0;
    edgeFade = saturate(edgeFade);
    edgeFade *= edgeFade; // Soft edge
    
    float alpha = cloud * headFade * tailFade * edgeFade * uOpacity * sampleColor.a * baseTex.a;
    
    // Color: whispered mix of deep indigo and nebula pink
    float colorT = cloud * 0.5 + progress * 0.3;
    float3 nebulaColor = lerp(uColor, uSecondaryColor, colorT);
    
    // Bright wisps at cloud centers
    float wispBright = pow(saturate(cloud), 3.0);
    float3 wispColor = float3(0.85, 0.8, 1.0); // Soft starlight
    nebulaColor = lerp(nebulaColor, wispColor, wispBright * 0.4);
    
    float3 finalColor = nebulaColor * uIntensity * baseTex.rgb;
    
    return ApplyOverbright(finalColor, alpha);
}

float4 NebulaScatterGlowPS(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseTex = tex2D(uImage0, coords);
    float progress = coords.x;
    float cross = abs(coords.y - 0.5);
    
    float cloud = NebulaCloud(coords, uTime * uScrollSpeed * 0.5);
    
    // Wide soft glow around nebula clouds
    float glow = exp(-cross * cross * 6.0);
    float trailFade = smoothstep(0.0, 0.15, progress) * smoothstep(1.0, 0.5, progress);
    
    float alpha = glow * trailFade * cloud * uOpacity * 0.5 * sampleColor.a * baseTex.a;
    float3 glowColor = lerp(uColor, uSecondaryColor, 0.5) * uIntensity * 0.7;
    
    return ApplyOverbright(glowColor * baseTex.rgb, alpha);
}

technique NebulaScatterTrail
{
    pass P0 { PixelShader = compile ps_3_0 NebulaScatterTrailPS(); }
}

technique NebulaScatterGlow
{
    pass P0 { PixelShader = compile ps_3_0 NebulaScatterGlowPS(); }
}
