// FermataTemporalWave.fx — Temporal distortion expanding wave on cast.
// Renders an expanding ring of temporal energy when the staff is used.
// Target: ps_2_0  |  Standard uniforms.

sampler uImage0 : register(s0);

float uTime;
float4 uColor;
float uOpacity;
float uIntensity;
float uProgress;     // 0 = just cast, 1 = fully expanded

// Palette
float4 uPrimaryColor;    // FermataPurple
float4 uSecondaryColor;  // TimeGold

float4 PS_TemporalWave(float2 coords : TEXCOORD0) : COLOR0
{
    // Center UV
    float2 center = coords * 2.0 - 1.0;
    float dist = length(center);
    
    // Expanding ring radius
    float ringRadius = uProgress * 0.9;
    float ringWidth = 0.08 + uProgress * 0.04;
    
    // Ring shape
    float ringDist = abs(dist - ringRadius);
    float ring = exp(-ringDist * ringDist / (ringWidth * ringWidth));
    
    // Inner fill: fading temporal void
    float innerFill = smoothstep(ringRadius, ringRadius - 0.15, dist) * (1.0 - uProgress);
    
    // Edge distortion ripples
    float angle = atan2(center.y, center.x);
    float ripple = sin(angle * 8.0 + uTime * 4.0 - dist * 20.0) * 0.5 + 0.5;
    ripple *= ring * 0.4;
    
    // Time shard sparkles along the ring
    float sparkle = pow(max(0.0, sin(angle * 16.0 - uTime * 6.0)), 16.0) * ring;
    
    // Color gradient: purple core -> gold edge
    float4 coreColor = uPrimaryColor;
    float4 edgeColor = uSecondaryColor;
    float4 baseColor = lerp(coreColor, edgeColor, dist / max(ringRadius, 0.01));
    
    // Combine layers
    float brightness = ring * 0.8 + innerFill * 0.3 + ripple + sparkle * 0.5;
    
    float4 result = baseColor * brightness;
    result.a *= uOpacity * (1.0 - uProgress * 0.7); // Fade as it expands
    result.rgb *= uIntensity;
    
    return result;
}

technique TemporalWave
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_TemporalWave();
    }
}
