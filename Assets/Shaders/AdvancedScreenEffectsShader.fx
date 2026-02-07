// =============================================================================
// MagnumOpus Advanced Screen Effects Shader - 5 Unique Distortion Styles
// =============================================================================
// Style 1: RIPPLE - Water ripple waves emanating from impact point
// Style 2: SHATTER - Reality fracture with geometric crack patterns
// Style 3: WARP - Gravitational lens/black hole warping effect
// Style 4: PULSE - Rhythmic heartbeat pulse with color shift
// Style 5: TEAR - Reality tear with dimensional bleeding
// =============================================================================

sampler uImage0 : register(s0);
sampler uNoiseTex : register(s1);

float2 uScreenResolution;
float2 uTargetPosition;
float2 uSecondaryPosition;
float uIntensity;
float uTime;
float uProgress; // 0-1 for animated effects
float3 uColor;
float3 uSecondaryColor;
float uRadius;
float uStyleParam1;
float uStyleParam2;

// =============================================================================
// UTILITY FUNCTIONS
// =============================================================================

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

float2 Rotate(float2 p, float angle)
{
    float c = cos(angle);
    float s = sin(angle);
    return float2(p.x * c - p.y * s, p.x * s + p.y * c);
}

// =============================================================================
// STYLE 1: RIPPLE DISTORTION
// Water-like ripple waves expanding from impact point
// =============================================================================

float4 RippleDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    float2 normalDir = normalize(direction);
    
    // Multiple concentric ripples
    float rippleWidth = 0.03;
    float rippleSpeed = 5.0;
    float numRipples = 3;
    
    float totalOffset = 0.0;
    
    for (int i = 0; i < 3; i++)
    {
        float rippleTime = uTime - i * 0.2;
        float rippleRadius = rippleTime * rippleSpeed * 0.1;
        
        // Ring mask - ripple exists in a narrow band
        float ringDist = abs(dist - rippleRadius);
        float ringMask = smoothstep(rippleWidth, 0.0, ringDist);
        
        // Damping based on distance traveled
        float damping = exp(-rippleRadius * 2.0);
        
        // Sine wave displacement
        float wave = sin(dist * 50.0 - rippleTime * rippleSpeed) * ringMask * damping;
        
        totalOffset += wave;
    }
    
    // Apply displacement
    float2 offset = normalDir * totalOffset * uIntensity * 0.02;
    float2 sampleCoords = coords + offset;
    
    // Sample with slight chromatic separation
    float3 color;
    color.r = tex2D(uImage0, sampleCoords + offset * 0.3).r;
    color.g = tex2D(uImage0, sampleCoords).g;
    color.b = tex2D(uImage0, sampleCoords - offset * 0.3).b;
    
    // Highlight ripple edges
    float highlight = abs(totalOffset) * 0.5;
    color += uColor * highlight * uIntensity;
    
    return float4(color, 1.0) * sampleColor;
}

// =============================================================================
// STYLE 2: SHATTER DISTORTION
// Reality fracture with geometric crack patterns
// =============================================================================

float4 ShatterDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    float angle = atan2(direction.y, direction.x);
    
    // Expanding shatter radius
    float shatterRadius = uProgress * uRadius;
    float inShatter = step(dist, shatterRadius);
    
    // Create angular crack pattern
    float numCracks = 8;
    float crackAngle = floor(angle / (6.28318 / numCracks)) * (6.28318 / numCracks);
    float crackOffset = angle - crackAngle - (6.28318 / numCracks / 2.0);
    
    // Each shard has slightly different offset
    float shardSeed = Hash(float2(crackAngle, 0.0));
    float2 shardOffset = float2(
        sin(crackAngle + shardSeed * 3.14159) * 0.02,
        cos(crackAngle + shardSeed * 5.0) * 0.02
    ) * uIntensity * inShatter;
    
    // Add radial displacement
    float radialOffset = (shardSeed - 0.5) * 0.03 * uIntensity * inShatter;
    float2 displacement = normalize(direction) * radialOffset;
    
    float2 sampleCoords = coords + shardOffset + displacement;
    float4 color = tex2D(uImage0, sampleCoords);
    
    // Crack line highlighting
    float crackLine = smoothstep(0.03, 0.0, abs(crackOffset));
    crackLine *= smoothstep(shatterRadius, shatterRadius - 0.05, dist);
    
    // Edge of shatter zone
    float shatterEdge = smoothstep(0.02, 0.0, abs(dist - shatterRadius));
    
    // Add crack glow
    color.rgb += uColor * crackLine * 0.5 * uIntensity;
    color.rgb += uSecondaryColor * shatterEdge * uIntensity;
    
    // Darken inside shattered area slightly
    color.rgb *= lerp(1.0, 0.95, inShatter * uIntensity * 0.5);
    
    return color * sampleColor;
}

// =============================================================================
// STYLE 3: WARP DISTORTION
// Gravitational lens/black hole warping effect
// =============================================================================

float4 WarpDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Gravitational lens equation simplified
    // Objects are displaced away from center, more strongly close to center
    float warpStrength = uIntensity * 0.1;
    float singularityRadius = 0.05; // Event horizon
    
    // Prevent division by zero and create event horizon
    float safeDist = max(dist, singularityRadius);
    
    // Displacement increases as you get closer (inverse relationship)
    float displacement = warpStrength / (safeDist * safeDist);
    displacement = min(displacement, 0.5); // Cap maximum displacement
    
    // Direction of warp (away from center creates stretching effect)
    float2 warpDir = normalize(direction);
    
    // Swirl component for rotating warp
    float swirlAngle = (1.0 / safeDist) * uIntensity * 0.5;
    float2 swirlDir = Rotate(warpDir, swirlAngle);
    
    // Combine radial push and swirl
    float2 finalWarp = lerp(warpDir, swirlDir, 0.5) * displacement;
    
    float2 sampleCoords = coords + finalWarp;
    
    // Sample with chromatic aberration near the center
    float chromatic = displacement * 0.5;
    float3 color;
    color.r = tex2D(uImage0, sampleCoords + warpDir * chromatic).r;
    color.g = tex2D(uImage0, sampleCoords).g;
    color.b = tex2D(uImage0, sampleCoords - warpDir * chromatic).b;
    
    // Darken at event horizon
    float darkening = smoothstep(singularityRadius * 2.0, singularityRadius, dist);
    color *= 1.0 - darkening * 0.9;
    
    // Add accretion disk glow
    float diskRadius = singularityRadius * 4.0;
    float disk = smoothstep(diskRadius, singularityRadius * 1.5, dist);
    disk *= smoothstep(singularityRadius, singularityRadius * 2.0, dist);
    color += uColor * disk * uIntensity * 2.0;
    
    return float4(color, 1.0) * sampleColor;
}

// =============================================================================
// STYLE 4: PULSE DISTORTION
// Rhythmic heartbeat pulse expanding from center with color shift
// =============================================================================

float4 PulseDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 direction = coords - uTargetPosition;
    float dist = length(direction);
    
    // Heartbeat-style double pulse
    float pulsePhase = frac(uTime * 1.5);
    float pulse1 = smoothstep(0.0, 0.1, pulsePhase) * smoothstep(0.3, 0.1, pulsePhase);
    float pulse2 = smoothstep(0.15, 0.25, pulsePhase) * smoothstep(0.4, 0.25, pulsePhase);
    float pulse = (pulse1 + pulse2 * 0.7);
    
    // Expanding ring
    float ringRadius = pulsePhase * 0.8;
    float ringWidth = 0.1 * (1.0 - pulsePhase * 0.5);
    float ring = smoothstep(ringWidth, 0.0, abs(dist - ringRadius));
    
    // Displacement pushes outward from ring
    float displacement = ring * pulse * uIntensity * 0.03;
    float2 sampleCoords = coords + normalize(direction) * displacement;
    
    float4 color = tex2D(uImage0, sampleCoords);
    
    // Color shift based on pulse
    float3 pulseColor = lerp(uColor, uSecondaryColor, pulse);
    
    // Vignette that pulses
    float vignette = 1.0 - dist * 0.5 * (1.0 + pulse * 0.3);
    
    // Brightness pulse at center
    float centerPulse = (1.0 - dist * 2.0) * pulse * 0.3;
    centerPulse = max(0.0, centerPulse);
    
    color.rgb *= vignette;
    color.rgb += pulseColor * ring * uIntensity * 0.5;
    color.rgb += pulseColor * centerPulse * uIntensity;
    
    return color * sampleColor;
}

// =============================================================================
// STYLE 5: TEAR DISTORTION
// Reality tear with dimensional bleeding between two points
// =============================================================================

float4 TearDistortion(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Line from primary to secondary position
    float2 lineDir = uSecondaryPosition - uTargetPosition;
    float lineLen = length(lineDir);
    float2 lineNorm = lineDir / max(lineLen, 0.001);
    float2 linePerp = float2(-lineNorm.y, lineNorm.x);
    
    // Project point onto line
    float2 toPoint = coords - uTargetPosition;
    float along = dot(toPoint, lineNorm);
    float perp = dot(toPoint, linePerp);
    
    // Distance from the tear line
    float tearProgress = saturate(along / lineLen);
    float inTearRange = step(0.0, along) * step(along, lineLen);
    float tearDist = abs(perp);
    
    // Tear width varies along length (thicker in middle)
    float tearWidth = 0.02 + sin(tearProgress * 3.14159) * 0.03;
    tearWidth *= uIntensity;
    
    // Jagged tear edges using noise
    float2 noiseCoord = float2(tearProgress * 10.0, uTime * 0.5);
    float tearNoise = Noise2D(noiseCoord) * 0.02;
    float adjustedDist = tearDist - tearNoise;
    
    // Inside tear vs edge
    float inTear = smoothstep(tearWidth, tearWidth * 0.5, adjustedDist) * inTearRange;
    float tearEdge = smoothstep(tearWidth * 0.3, 0.0, abs(adjustedDist - tearWidth * 0.7)) * inTearRange;
    
    // Displacement pulls toward the tear
    float displacement = (1.0 - saturate(tearDist / (tearWidth * 3.0))) * sign(perp) * 0.02 * uIntensity * inTearRange;
    float2 sampleCoords = coords + linePerp * displacement;
    
    // Base sample
    float4 color = tex2D(uImage0, sampleCoords);
    
    // Inside tear - show "other dimension" (inverted/tinted)
    float3 otherDimension = 1.0 - color.rgb;
    otherDimension = lerp(otherDimension, uSecondaryColor, 0.5);
    
    // Blend based on tear
    color.rgb = lerp(color.rgb, otherDimension, inTear * 0.8);
    
    // Tear edge glow
    color.rgb += uColor * tearEdge * 2.0;
    
    // Energy crackling along tear
    float crackle = Noise2D(float2(tearProgress * 30.0 + uTime * 5.0, perp * 100.0));
    crackle = smoothstep(0.7, 1.0, crackle) * inTearRange * smoothstep(tearWidth * 2.0, 0.0, tearDist);
    color.rgb += uColor * crackle * uIntensity;
    
    return color * sampleColor;
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
