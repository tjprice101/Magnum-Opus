// =============================================================================
// MagnumOpus Simple Screen Effects Shader - PS 2.0 Compatible
// =============================================================================
// Simplified screen distortion effects within PS 2.0 limits
// 5 passes: Ripple, Shatter, Warp, Pulse, Tear
// =============================================================================

sampler uImage0 : register(s0);

float3 uColor;
float uOpacity;
float uTime;
float uIntensity;
float2 uCenter;
float2 uImageSize;

// =============================================================================
// STYLE 1: RIPPLE - Water wave distortion
// =============================================================================
float4 RippleDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - uCenter;
    float dist = length(offset);
    
    // Ripple wave
    float wave = sin(dist * 30.0 - uTime * 5.0) * uIntensity * 0.01;
    wave *= saturate(1.0 - dist * 2.0); // Fade with distance
    
    float2 distortedCoords = coords + normalize(offset) * wave;
    float4 color = tex2D(uImage0, distortedCoords);
    
    return color * sampleColor;
}

// =============================================================================
// STYLE 2: SHATTER - Reality crack effect
// =============================================================================
float4 ShatterDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);
    
    // Angular cracks
    float crack = abs(sin(angle * 8.0)) * uIntensity * 0.02;
    crack *= saturate(1.0 - dist * 1.5);
    
    float2 distortedCoords = coords + float2(cos(angle), sin(angle)) * crack;
    float4 color = tex2D(uImage0, distortedCoords);
    
    // Slight chromatic aberration
    float3 tint = lerp(float3(1,1,1), uColor, crack * 5.0);
    
    return float4(color.rgb * tint, color.a) * sampleColor;
}

// =============================================================================
// STYLE 3: WARP - Gravitational lens effect
// =============================================================================
float4 WarpDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - uCenter;
    float dist = length(offset);
    
    // Gravitational warp toward center
    float warp = uIntensity * 0.1 / (dist + 0.1);
    warp = min(warp, 0.1); // Clamp
    
    float2 distortedCoords = coords - normalize(offset) * warp;
    float4 color = tex2D(uImage0, distortedCoords);
    
    return color * sampleColor;
}

// =============================================================================
// STYLE 4: PULSE - Heartbeat expansion
// =============================================================================
float4 PulseDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - uCenter;
    float dist = length(offset);
    
    // Pulsing expansion
    float pulse = sin(uTime * 4.0) * 0.5 + 0.5;
    float ring = saturate(1.0 - abs(dist - pulse * 0.3) * 10.0);
    
    float2 distortedCoords = coords + normalize(offset) * ring * uIntensity * 0.02;
    float4 color = tex2D(uImage0, distortedCoords);
    
    // Pulse glow overlay
    float3 glowTint = lerp(float3(1,1,1), uColor, ring * uIntensity);
    
    return float4(color.rgb * glowTint, color.a) * sampleColor;
}

// =============================================================================
// STYLE 5: TEAR - Reality tear between points
// =============================================================================
float4 TearDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offset = coords - uCenter;
    float dist = length(offset);
    float angle = atan2(offset.y, offset.x);
    
    // Vertical tear
    float tear = saturate(1.0 - abs(offset.x) * 10.0) * saturate(1.0 - abs(offset.y) * 3.0);
    tear *= uIntensity;
    
    float2 distortedCoords = coords;
    distortedCoords.x += sign(offset.x) * tear * 0.05;
    
    float4 color = tex2D(uImage0, distortedCoords);
    
    // Dark edge on tear
    float3 tearColor = lerp(color.rgb, uColor * 0.5, tear * 0.5);
    
    return float4(tearColor, color.a) * sampleColor;
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique RippleTechnique
{
    pass RipplePass
    {
        PixelShader = compile ps_2_0 RippleDistortion();
    }
}

technique ShatterTechnique
{
    pass ShatterPass
    {
        PixelShader = compile ps_2_0 ShatterDistortion();
    }
}

technique WarpTechnique
{
    pass WarpPass
    {
        PixelShader = compile ps_2_0 WarpDistortion();
    }
}

technique PulseTechnique
{
    pass PulsePass
    {
        PixelShader = compile ps_2_0 PulseDistortion();
    }
}

technique TearTechnique
{
    pass TearPass
    {
        PixelShader = compile ps_2_0 TearDistortion();
    }
}
