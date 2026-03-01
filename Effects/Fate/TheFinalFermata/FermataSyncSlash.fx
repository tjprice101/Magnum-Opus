// FermataSyncSlash.fx  ESynchronized slash burst effect.
// Renders the visual burst when all swords slash simultaneously.
// Target: ps_3_0  |  Standard uniforms.

sampler uImage0 : register(s0);

float uTime;
float4 uColor;
float uOpacity;
float uIntensity;
float uProgress;     // 0 = slash start, 1 = slash complete
float uSlashAngle;   // Direction of the synchronised slash (radians)

float4 uPrimaryColor;    // FermataCrimson
float4 uSecondaryColor;  // FlashWhite

float4 PS_SyncSlash(float2 coords : TEXCOORD0) : COLOR0
{
    // Center UV
    float2 center = coords * 2.0 - 1.0;
    float dist = length(center);
    float angle = atan2(center.y, center.x);
    
    // Slash arc  Edirectional energy burst
    float slashDiff = angle - uSlashAngle;
    // Wrap to [-PI, PI]
    slashDiff = slashDiff - floor(slashDiff / 6.28318530 + 0.5) * 6.28318530;
    
    // Arc width narrows with progress (starts wide, snaps to thin line)
    float arcWidth = 0.8 - uProgress * 0.6;
    float arc = exp(-slashDiff * slashDiff / (arcWidth * arcWidth));
    
    // Radial expansion
    float maxDist = uProgress * 1.2;
    float radialFade = smoothstep(maxDist, maxDist - 0.3, dist);
    float radialEdge = exp(-(dist - maxDist) * (dist - maxDist) * 20.0);
    
    // Energy slash line
    float slashLine = arc * radialFade;
    
    // Burst sparks at the leading edge
    float sparks = pow(max(0.0, sin(dist * 30.0 - uTime * 8.0)), 8.0) * radialEdge * arc;
    
    // Color: crimson base -> white at peak energy
    float energyPeak = (1.0 - uProgress) * arc * radialFade;
    float4 baseColor = lerp(uPrimaryColor, uSecondaryColor, energyPeak);
    
    // Combine
    float brightness = slashLine * 0.7 + radialEdge * arc * 0.3 + sparks * 0.4;
    
    float4 result = baseColor * brightness;
    result.a *= uOpacity * (1.0 - uProgress * 0.5);
    result.rgb *= uIntensity;
    
    return result;
}

technique SyncSlash
{
    pass Pass0
    {
        PixelShader = compile ps_3_0 PS_SyncSlash();
    }
}
