// ============================================================================
// TacetParadoxExplosion.fx — TacetsEnigma paradox explosion shader
// Two techniques:
//   TacetParadoxBlast — Expanding shockwave with paradox distortion
//   TacetParadoxRing  — Sharp ring at the explosion's edge
// SpriteBatch-style (no custom vertex shader)
// ============================================================================

sampler uImage0 : register(s0);  // Base texture (explosion sprite / source)
sampler uImage1 : register(s1);  // Noise texture

float3 uColor;           // Primary color (tacet purple)
float3 uSecondaryColor;  // Secondary color (paradox green)
float uOpacity;           // Overall opacity
float uTime;              // Animation progress (0→1)
float uIntensity;         // Effect strength (paradox stack intensity)

float4 PS_ParadoxBlast(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Radial distance from center
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);
    
    // Expanding shockwave ring — grows outward over time
    float ringRadius = lerp(0.0, 0.55, uTime);
    float ringWidth = 0.15 * (1.0 - uTime * 0.6);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringWidth, 0.0, ringDist);
    
    // Noise turbulence for paradox distortion
    float2 noiseUV = float2(angle * 0.4 + uTime * 1.5, dist * 3.0 - uTime);
    float noise = tex2D(uImage1, noiseUV).r;
    
    // Second noise layer for more chaotic paradox feel
    float2 noiseUV2 = float2(dist * 2.0 + uTime, angle * 0.3 - uTime * 0.5);
    float noise2 = tex2D(uImage1, noiseUV2).r;
    float combinedNoise = noise * 0.6 + noise2 * 0.4;
    
    // Modulate ring with noise
    ring *= (combinedNoise * 0.5 + 0.5);
    
    // Inner fill — bright flash that fades with time
    float innerFill = smoothstep(ringRadius, 0.0, dist) * (1.0 - uTime);
    
    // Color: purple outer → green inner, with noise breaking it up
    float colorMix = saturate(1.0 - dist * 2.5 + combinedNoise * 0.3);
    float3 blastColor = lerp(uColor, uSecondaryColor, colorMix);
    
    // Flash white at explosion start
    float flash = exp(-uTime * 6.0) * uIntensity;
    blastColor += float3(1, 1, 1) * flash * 0.4;
    
    float totalAlpha = saturate(ring + innerFill * 0.5) * uOpacity * color.a;
    
    return float4(blastColor, totalAlpha);
}

float4 PS_ParadoxRing(float4 color : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Radial coordinates
    float2 center = float2(0.5, 0.5);
    float2 toCenter = coords - center;
    float dist = length(toCenter);
    float angle = atan2(toCenter.y, toCenter.x);
    
    // Sharp expanding ring at the explosion edge
    float ringRadius = lerp(0.05, 0.5, uTime);
    float ringThickness = 0.02 + 0.01 * (1.0 - uTime);
    float ringDist = abs(dist - ringRadius);
    float ring = smoothstep(ringThickness, 0.0, ringDist);
    
    // Angular noise for jagged ring edges
    float2 noiseUV = float2(angle * 0.8 + uTime * 2.0, dist * 4.0);
    float noise = tex2D(uImage1, noiseUV).r;
    ring *= smoothstep(0.2, 0.5, noise);
    
    // Ring color — bright paradox green with purple shimmer
    float colorOsc = sin(angle * 6.0 + uTime * 8.0) * 0.5 + 0.5;
    float3 ringColor = lerp(uSecondaryColor, uColor, colorOsc * 0.3);
    
    // Brightness boost
    ringColor *= 1.2 * uIntensity;
    
    float alpha = ring * uOpacity * (1.0 - uTime * 0.5) * color.a;
    
    return float4(ringColor, alpha);
}

technique TacetParadoxBlast
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_ParadoxBlast();
    }
}

technique TacetParadoxRing
{
    pass P0
    {
        PixelShader = compile ps_3_0 PS_ParadoxRing();
    }
}
