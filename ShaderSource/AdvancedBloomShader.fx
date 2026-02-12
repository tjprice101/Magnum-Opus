// =============================================================================
// MagnumOpus Advanced Bloom Shader - 5 Unique Visual Styles
// =============================================================================
// Style 1: ETHEREAL - Soft, dreamy, gossamer glow with subtle color shifts
// Style 2: INFERNAL - Harsh, flickering, fire-like bloom with heat distortion
// Style 3: CELESTIAL - Star-like rays, cosmic shimmer, orbital patterns
// Style 4: CHROMATIC - Rainbow prismatic, color separation, spectrum blend
// Style 5: VOID - Dark inner glow, event horizon effect, inverse luminance
// =============================================================================
//
// USAGE: Include shared utility library for noise, SDFs, color utilities:
// #include "HLSLLibrary.fxh"
// (Uncomment above line after compiling library into your build pipeline)
// =============================================================================

sampler uImage0 : register(s0);
sampler uImage1 : register(s1); // Secondary texture for patterns

float3 uColor;
float3 uSecondaryColor;
float3 uTertiaryColor;
float uOpacity;
float uTime;
float uIntensity;
float uPulseSpeed;
float uPulseIntensity;
float uNoiseScale;
float uStyleParam1;
float uStyleParam2;
float2 uCenter;
float2 uImageSize;

// =============================================================================
// UTILITY FUNCTIONS
// Note: These are duplicated from HLSLLibrary.fxh for standalone compilation.
// When integrating with the full library, remove these and use #include.
// =============================================================================

float QuadraticBump(float x)
{
    return x * (4.0 - x * 4.0);
}

float SineBump(float x)
{
    return sin(x * 3.14159);
}

float Hash(float2 p)
{
    return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453);
}

float Noise2D(float2 p)
{
    float2 i = floor(p);
    float2 f = frac(p);
    f = f * f * (3.0 - 2.0 * f);
    
    float a = Hash(i);
    float b = Hash(i + float2(1.0, 0.0));
    float c = Hash(i + float2(0.0, 1.0));
    float d = Hash(i + float2(1.0, 1.0));
    
    return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
}

float3 HueShift(float3 color, float shift)
{
    float3 p = float3(0.55735, 0.55735, 0.55735);
    float3 u = color - dot(color, p) * p;
    float3 v = cross(p, u);
    return color + u * cos(shift * 6.28318) + v * sin(shift * 6.28318);
}

float3 RGBToHSV(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = c.g < c.b ? float4(c.bg, K.wz) : float4(c.gb, K.xy);
    float4 q = c.r < p.x ? float4(p.xyw, c.r) : float4(c.r, p.yzx);
    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 HSVToRGB(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
}

// =============================================================================
// STYLE 1: ETHEREAL BLOOM
// Soft, dreamy, gossamer glow with subtle breathing animation
// =============================================================================

float4 EtherealBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float dist = length(coords - center) * 2.0;
    
    // Soft ethereal radial falloff with multiple layers
    float layer1 = saturate(1.0 - dist * 0.7);
    float layer2 = saturate(1.0 - dist * 1.2);
    float layer3 = saturate(1.0 - dist * 2.0);
    
    // Smooth exponential falloffs
    layer1 = pow(layer1, 2.5);
    layer2 = pow(layer2, 1.8);
    layer3 = pow(layer3, 1.2);
    
    // Breathing animation with multiple frequencies
    float breathe1 = sin(uTime * 1.2) * 0.15 + 1.0;
    float breathe2 = sin(uTime * 1.8 + 1.0) * 0.1 + 1.0;
    float breathe3 = sin(uTime * 0.7 + 2.5) * 0.2 + 1.0;
    
    // Gossamer noise overlay
    float noise = Noise2D(coords * 8.0 + uTime * 0.3) * 0.15 + 0.85;
    
    // Subtle color shifting
    float hueShift = sin(uTime * 0.5) * 0.05;
    float3 color1 = HueShift(uColor, hueShift);
    float3 color2 = HueShift(uSecondaryColor, -hueShift);
    
    // Layer colors with decreasing saturation toward edges
    float3 finalColor = color1 * layer3 * breathe3;
    finalColor += lerp(color1, color2, 0.5) * layer2 * breathe2;
    finalColor += lerp(color1, float3(1.0, 1.0, 1.0), 0.3) * layer1 * breathe1;
    
    finalColor *= baseColor.rgb * uIntensity * noise;
    float finalOpacity = (layer1 + layer2 * 0.5 + layer3 * 0.25) * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// =============================================================================
// STYLE 2: INFERNAL BLOOM
// Harsh, flickering, fire-like glow with animated ember particles
// =============================================================================

float4 InfernalBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float2 offset = coords - center;
    float dist = length(offset) * 2.0;
    
    // Harsh flame-like falloff
    float coreGlow = saturate(1.0 - dist * 2.5);
    coreGlow = pow(coreGlow, 0.8); // Harder edges
    
    float outerFlame = saturate(1.0 - dist * 1.3);
    outerFlame = pow(outerFlame, 1.5);
    
    // Flickering animation (multiple frequencies for natural fire)
    float flicker1 = sin(uTime * 15.0) * 0.2 + 0.8;
    float flicker2 = sin(uTime * 23.0 + 1.0) * 0.15 + 0.85;
    float flicker3 = sin(uTime * 37.0 + 2.0) * 0.1 + 0.9;
    float flicker = flicker1 * flicker2 * flicker3;
    
    // Flame noise that rises upward
    float2 noiseCoord = coords;
    noiseCoord.y -= uTime * 0.5; // Rising motion
    float flameNoise = Noise2D(noiseCoord * 6.0) * 0.4 + 0.6;
    flameNoise *= Noise2D(noiseCoord * 12.0 + 5.0) * 0.3 + 0.7;
    
    // Ember particles
    float embers = 0.0;
    for (int i = 0; i < 3; i++)
    {
        float2 emberCoord = coords;
        emberCoord.y -= uTime * (0.3 + i * 0.2);
        emberCoord.x += sin(uTime * 2.0 + i * 3.14159) * 0.1;
        float ember = Noise2D(emberCoord * 20.0);
        ember = smoothstep(0.75, 0.9, ember);
        embers += ember * (1.0 - dist * 2.0);
    }
    
    // Fire color gradient (dark red -> orange -> yellow at core)
    float3 fireColor = float3(0.8, 0.1, 0.0) * outerFlame;
    fireColor += float3(1.0, 0.4, 0.0) * coreGlow * 0.7;
    fireColor += float3(1.0, 0.9, 0.4) * coreGlow * coreGlow;
    
    // Apply flicker and noise
    float3 finalColor = fireColor * uColor * flicker * flameNoise;
    finalColor += embers * float3(1.0, 0.5, 0.1) * 2.0;
    finalColor *= baseColor.rgb * uIntensity;
    
    float finalOpacity = (coreGlow + outerFlame * 0.5 + embers) * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, saturate(finalOpacity));
}

// =============================================================================
// STYLE 3: CELESTIAL BLOOM
// Star-like rays, cosmic shimmer, orbital particle patterns
// =============================================================================

float4 CelestialBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float2 offset = coords - center;
    float dist = length(offset) * 2.0;
    float angle = atan2(offset.y, offset.x);
    
    // Core celestial glow
    float coreGlow = saturate(1.0 - dist * 1.5);
    coreGlow = pow(coreGlow, 2.0);
    
    // Star rays (6-pointed with shimmer)
    int numRays = 6;
    float rayPattern = 0.0;
    for (int i = 0; i < numRays; i++)
    {
        float rayAngle = angle - (6.28318 * i / numRays) + uTime * 0.2;
        float ray = abs(cos(rayAngle * numRays / 2.0));
        ray = pow(ray, 20.0); // Sharp rays
        ray *= (1.0 - dist * 0.8); // Fade with distance
        rayPattern = max(rayPattern, ray);
    }
    
    // Secondary ray set (offset for complexity)
    for (int i = 0; i < numRays; i++)
    {
        float rayAngle = angle - (6.28318 * i / numRays) - uTime * 0.15 + 0.5;
        float ray = abs(cos(rayAngle * numRays / 2.0));
        ray = pow(ray, 15.0);
        ray *= (1.0 - dist * 0.9) * 0.5;
        rayPattern = max(rayPattern, ray);
    }
    
    // Orbital ring with traveling sparks
    float ringDist = abs(dist - 0.6);
    float ring = smoothstep(0.1, 0.0, ringDist);
    float sparkle = sin((angle + uTime * 3.0) * 12.0) * 0.5 + 0.5;
    sparkle = pow(sparkle, 4.0);
    ring *= sparkle;
    
    // Cosmic shimmer overlay
    float shimmer = Noise2D(coords * 15.0 + uTime * 0.5);
    shimmer = pow(shimmer, 3.0) * 0.3;
    
    // Color blending
    float3 coreColor = float3(1.0, 1.0, 1.0);
    float3 rayColor = uColor;
    float3 outerColor = uSecondaryColor;
    
    float3 finalColor = coreColor * coreGlow;
    finalColor += rayColor * rayPattern;
    finalColor += outerColor * ring;
    finalColor += uColor * shimmer * (1.0 - dist);
    
    finalColor *= baseColor.rgb * uIntensity;
    float finalOpacity = saturate(coreGlow + rayPattern * 0.7 + ring * 0.5 + shimmer) * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// =============================================================================
// STYLE 4: CHROMATIC BLOOM
// Rainbow prismatic glow with animated spectrum cycling
// =============================================================================

float4 ChromaticBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float2 offset = coords - center;
    float dist = length(offset) * 2.0;
    float angle = atan2(offset.y, offset.x);
    
    // Radial falloff
    float glow = saturate(1.0 - dist);
    glow = pow(glow, 1.5);
    
    // Rainbow hue based on angle and time
    float hue = (angle / 6.28318 + 0.5 + uTime * 0.1);
    hue = frac(hue);
    
    // Convert hue to RGB
    float3 rainbow = HSVToRGB(float3(hue, 1.0, 1.0));
    
    // Secondary rainbow layer (offset)
    float hue2 = frac(hue + 0.5 + sin(uTime * 0.5) * 0.1);
    float3 rainbow2 = HSVToRGB(float3(hue2, 0.8, 0.9));
    
    // Prismatic separation effect (RGB channels at different distances)
    float redGlow = saturate(1.0 - dist * 0.9);
    float greenGlow = saturate(1.0 - dist * 1.0);
    float blueGlow = saturate(1.0 - dist * 1.1);
    
    float3 prismColor;
    prismColor.r = rainbow.r * pow(redGlow, 1.4);
    prismColor.g = rainbow.g * pow(greenGlow, 1.5);
    prismColor.b = rainbow.b * pow(blueGlow, 1.6);
    
    // Add secondary rainbow at outer edge
    float outerRing = smoothstep(0.0, 0.15, dist - 0.5) * smoothstep(0.15, 0.0, dist - 0.7);
    prismColor += rainbow2 * outerRing * 0.6;
    
    // Sparkle overlay
    float sparkle = Noise2D(coords * 25.0 + uTime);
    sparkle = pow(sparkle, 5.0) * 2.0;
    float3 sparkleColor = HSVToRGB(float3(frac(uTime * 0.3), 0.5, 1.0));
    prismColor += sparkleColor * sparkle * glow;
    
    float3 finalColor = prismColor * baseColor.rgb * uIntensity;
    float finalOpacity = glow * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// =============================================================================
// STYLE 5: VOID BLOOM
// Dark inner glow, event horizon effect, inverse luminance
// =============================================================================

float4 VoidBloom(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 baseColor = tex2D(uImage0, coords);
    
    float2 center = float2(0.5, 0.5);
    float2 offset = coords - center;
    float dist = length(offset) * 2.0;
    float angle = atan2(offset.y, offset.x);
    
    // Inverse glow - dark at center, bright ring at edge
    float eventHorizon = smoothstep(0.2, 0.6, dist) * smoothstep(1.2, 0.8, dist);
    eventHorizon = pow(eventHorizon, 0.8);
    
    // Dark core
    float darkCore = saturate(1.0 - dist * 3.0);
    darkCore = pow(darkCore, 2.0);
    
    // Swirling void energy
    float2 swirl = coords - center;
    float swirlAngle = angle + uTime * 0.3 + dist * 2.0;
    float2 swirlCoord = float2(cos(swirlAngle), sin(swirlAngle)) * dist + center;
    float voidEnergy = Noise2D(swirlCoord * 5.0);
    voidEnergy = smoothstep(0.3, 0.7, voidEnergy);
    
    // Tendrils of void energy
    float tendrils = 0.0;
    for (int i = 0; i < 5; i++)
    {
        float tendrilAngle = angle + (6.28318 * i / 5.0) - uTime * 0.2;
        float tendril = sin(tendrilAngle * 5.0 + dist * 10.0 - uTime * 2.0);
        tendril = smoothstep(0.7, 1.0, tendril);
        tendril *= (1.0 - abs(dist - 0.5) * 3.0);
        tendrils += tendril;
    }
    tendrils = saturate(tendrils);
    
    // Void colors - dark purples and deep blacks
    float3 voidColor = uColor * eventHorizon;
    float3 tendrilColor = uSecondaryColor * tendrils * 0.8;
    float3 energyColor = lerp(uColor, uSecondaryColor, voidEnergy) * voidEnergy * 0.5;
    
    // Dark core absorbs light
    float3 finalColor = voidColor + tendrilColor + energyColor;
    finalColor *= (1.0 - darkCore * 0.8); // Darken at center
    finalColor *= baseColor.rgb * uIntensity;
    
    // Opacity includes the ring and tendrils
    float finalOpacity = (eventHorizon + tendrils * 0.5 + voidEnergy * 0.3) * uOpacity * sampleColor.a * baseColor.a;
    
    return float4(finalColor, finalOpacity);
}

// =============================================================================
// TECHNIQUES
// =============================================================================

technique EtherealTechnique
{
    pass EtherealPass
    {
        PixelShader = compile ps_2_0 EtherealBloom();
    }
}

technique InfernalTechnique
{
    pass InfernalPass
    {
        PixelShader = compile ps_2_0 InfernalBloom();
    }
}

technique CelestialTechnique
{
    pass CelestialPass
    {
        PixelShader = compile ps_2_0 CelestialBloom();
    }
}

technique ChromaticTechnique
{
    pass ChromaticPass
    {
        PixelShader = compile ps_2_0 ChromaticBloom();
    }
}

technique VoidTechnique
{
    pass VoidPass
    {
        PixelShader = compile ps_2_0 VoidBloom();
    }
}
