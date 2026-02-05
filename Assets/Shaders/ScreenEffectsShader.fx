// =============================================================================
// MagnumOpus Screen Effects Shader - Post-processing effects
// =============================================================================
// Provides screen-wide effects like chromatic aberration, screen shake,
// color grading, and boss-specific visual distortions.
// =============================================================================

sampler uImage0 : register(s0);
float2 uScreenResolution;
float2 uTargetPosition; // Screen-space position (0-1)
float uIntensity;
float uTime;
float3 uColor;

// =============================================================================
// CHROMATIC ABERRATION - RGB channel separation for impact effects
// =============================================================================
float4 ChromaticAberration(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Direction from target to pixel
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Aberration strength falls off with distance
    float strength = uIntensity * 0.01 * saturate(1.0 - dist * 2.0);
    
    // Sample RGB channels at slightly offset positions
    float2 redOffset = direction * strength * 1.0;
    float2 greenOffset = direction * strength * 0.0;
    float2 blueOffset = direction * strength * -1.0;
    
    float r = tex2D(uImage0, coords + redOffset).r;
    float g = tex2D(uImage0, coords + greenOffset).g;
    float b = tex2D(uImage0, coords + blueOffset).b;
    float a = tex2D(uImage0, coords).a;
    
    return float4(r, g, b, a) * sampleColor;
}

// =============================================================================
// RADIAL BLUR - Speed/impact blur from center point
// =============================================================================
float4 RadialBlur(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    float4 color = float4(0, 0, 0, 0);
    int samples = 8;
    
    for (int i = 0; i < samples; i++)
    {
        float t = (float)i / (float)(samples - 1);
        float2 sampleCoords = coords - direction * t * uIntensity * 0.1;
        color += tex2D(uImage0, sampleCoords);
    }
    
    return (color / samples) * sampleColor;
}

// =============================================================================
// VIGNETTE - Darken edges for focus effect
// =============================================================================
float4 Vignette(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center) * 1.414; // Normalize to corners
    
    float vignette = 1.0 - pow(dist, 2.0) * uIntensity;
    vignette = saturate(vignette);
    
    return float4(baseColor.rgb * vignette, baseColor.a) * sampleColor;
}

// =============================================================================
// COLOR FLASH - Screen-wide color overlay with fade
// =============================================================================
float4 ColorFlash(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    // Blend base color with flash color
    float3 flashedColor = lerp(baseColor.rgb, uColor, uIntensity);
    
    return float4(flashedColor, baseColor.a) * sampleColor;
}

// =============================================================================
// WAVE DISTORTION - Ripple effect from impact point
// =============================================================================
float4 WaveDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Animated wave pattern
    float wave = sin(dist * 50.0 - uTime * 10.0);
    wave *= saturate(1.0 - dist * 3.0); // Fade at edges
    wave *= uIntensity * 0.01;
    
    // Offset sample position by wave
    float2 offset = normalize(direction) * wave;
    float4 color = tex2D(uImage0, coords + offset);
    
    return color * sampleColor;
}

// =============================================================================
// HEAT DISTORTION - Wavy heat shimmer effect
// =============================================================================
float4 HeatDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Multiple overlapping sine waves for organic look
    float wave1 = sin(coords.y * 100.0 + uTime * 3.0) * 0.003;
    float wave2 = sin(coords.y * 150.0 + uTime * 4.0 + 1.5) * 0.002;
    float wave3 = sin(coords.x * 80.0 + uTime * 2.5) * 0.002;
    
    float2 offset = float2(wave1 + wave2, wave3) * uIntensity;
    
    float4 color = tex2D(uImage0, coords + offset);
    
    return color * sampleColor;
}

// =============================================================================
// FATE REALITY CRACK - Dark prismatic distortion for Fate theme
// =============================================================================
float4 RealityCrack(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Angular crack lines
    float angle = atan2(direction.y, direction.x);
    float crackPattern = abs(sin(angle * 8.0 + dist * 20.0));
    crackPattern = pow(crackPattern, 8.0);
    
    // Invert colors in crack lines
    float crackStrength = crackPattern * uIntensity * saturate(1.0 - dist * 2.0);
    float3 crackedColor = lerp(baseColor.rgb, 1.0 - baseColor.rgb, crackStrength * 0.5);
    
    // Add colored tint
    crackedColor = lerp(crackedColor, uColor, crackStrength * 0.3);
    
    return float4(crackedColor, baseColor.a) * sampleColor;
}

technique Technique1
{
    pass ChromaticAberration
    {
        PixelShader = compile ps_2_0 ChromaticAberration();
    }
    pass RadialBlur
    {
        PixelShader = compile ps_2_0 RadialBlur();
    }
    pass Vignette
    {
        PixelShader = compile ps_2_0 Vignette();
    }
    pass ColorFlash
    {
        PixelShader = compile ps_2_0 ColorFlash();
    }
    pass WaveDistortion
    {
        PixelShader = compile ps_2_0 WaveDistortion();
    }
    pass HeatDistortion
    {
        PixelShader = compile ps_2_0 HeatDistortion();
    }
    pass RealityCrack
    {
        PixelShader = compile ps_2_0 RealityCrack();
    }
}
