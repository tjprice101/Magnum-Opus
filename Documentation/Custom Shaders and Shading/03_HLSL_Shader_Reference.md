# HLSL Shader Reference

> **Complete reference of all HLSL shaders from FargosSoulsDLC, organized by category with code excerpts.**

---

## Shader Categories

| Category | Path | Count |
|----------|------|-------|
| Primitives (Trails) | `Assets/AutoloadedEffects/Shaders/Primitives/` | 15+ |
| Objects | `Assets/AutoloadedEffects/Shaders/Objects/` | 6+ |
| Overlay Modifiers | `Assets/AutoloadedEffects/Shaders/OverlayModifiers/` | 8+ |
| Global Overlays | `Assets/AutoloadedEffects/Shaders/GlobalOverlays/` | 4+ |
| Filters | `Assets/AutoloadedEffects/Filters/` | 3+ |
| Root Level | `Assets/AutoloadedEffects/Shaders/` | 10+ |

---

## Common HLSL Patterns

### The QuadraticBump Function
**Used in almost every shader for smooth falloffs:**

```hlsl
float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}
// Input 0.0 → Output 0.0
// Input 0.5 → Output 1.0 (peak)
// Input 1.0 → Output 0.0
// Perfect for edge-to-center intensity gradients
```

### Standard Vertex Shader Structure
```hlsl
struct VertexShaderInput
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float3 TextureCoordinates : TEXCOORD0;
};

VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;
    float4 pos = mul(input.Position, uWorldViewProjection);
    output.Position = pos;
    output.Color = input.Color;
    output.TextureCoordinates = input.TextureCoordinates;
    return output;
}
```

### Palette/Gradient Lerping
```hlsl
float gradientCount;
float3 gradient[8];  // Up to 8 colors

float3 PaletteLerp(float interpolant)
{
    int startIndex = clamp(interpolant * gradientCount, 0, gradientCount - 1);
    int endIndex = clamp(startIndex + 1, 0, gradientCount - 1);
    return lerp(gradient[startIndex], gradient[endIndex], frac(interpolant * gradientCount));
}
```

### Pixelation
```hlsl
float2 pixelationFactor = textureSize0 * 0.7;
coords = floor(coords * pixelationFactor) / pixelationFactor;
```

### Primitive UV Distortion Fix
```hlsl
// Account for texture distortion artifacts in accordance with the primitive distortion fixes.
coords.y = (coords.y - 0.5) / coords.z + 0.5;
```

---

## Primitive Trail Shaders

### PrimitiveBloomShader.fx
**Purpose:** Generic bloom trail for soft glow effects

```hlsl
// Key parameters
float innerGlowIntensity;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    // Edge fade using QuadraticBump
    float edgeFade = QuadraticBump(coords.y);
    
    // Inner glow intensification
    float glowFactor = 1 + innerGlowIntensity * edgeFade;
    
    return color * edgeFade * glowFactor;
}
```

**C# Usage:**
```csharp
ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.PrimitiveBloomShader");
shader.TrySetParameter("innerGlowIntensity", 0.45f);
```

---

### BlazingExoLaserbeamShader.fx
**Purpose:** Hot laser beam with scrolling noise

```hlsl
sampler noiseScrollTexture : register(s1);

float globalTime;
float2 laserDirection;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    float4 color = input.Color;
    
    // UV distortion fix
    coords.y = (coords.y - 0.5) / coords.z + 0.5;
    
    // Edge fade
    float edgeFade = QuadraticBump(coords.y);
    
    // Scrolling noise
    float noise = tex2D(noiseScrollTexture, coords * float2(3, 1) + globalTime * float2(-2, 0));
    
    // Combine
    return color * edgeFade * (1 + noise * 0.3);
}
```

**C# Usage:**
```csharp
shader.TrySetParameter("laserDirection", Projectile.velocity.SafeNormalize(Vector2.UnitX).ToVector3());
shader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
```

---

### HadesLaserShader.fx
**Purpose:** Electric/cracked laser with glow

```hlsl
sampler streakHighlightTexture : register(s1);

float glowIntensity;
float noiseScrollOffset;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / coords.z + 0.5;
    
    float4 color = input.Color;
    float edgeFade = QuadraticBump(coords.y);
    
    // Cracked highlight texture
    float highlight = tex2D(streakHighlightTexture, 
        coords * float2(4, 1) + float2(noiseScrollOffset, 0));
    
    // Apply glow
    float glow = 1 + glowIntensity * highlight * edgeFade;
    
    return color * edgeFade * glow;
}
```

**C# Usage:**
```csharp
shader.TrySetParameter("glowIntensity", 1f);
shader.TrySetParameter("noiseScrollOffset", Projectile.identity * 0.3149f);
shader.SetTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Cracks"), 1, SamplerState.LinearWrap);
```

---

### PlasmaFlameJetShader.fx
**Purpose:** Animated plasma/fire jet

```hlsl
sampler noiseTexture : register(s1);

float localTime;
float glowPower;
float edgeFadeThreshold;
float4 glowColor;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / coords.z + 0.5;
    
    float4 color = input.Color;
    
    // Animated noise for flame effect
    float noise1 = tex2D(noiseTexture, coords * float2(2, 1) + float2(localTime * 2, 0));
    float noise2 = tex2D(noiseTexture, coords * float2(3, 0.5) + float2(localTime * 1.5, 0));
    float combinedNoise = (noise1 + noise2) * 0.5;
    
    // Edge fade
    float edgeFade = smoothstep(edgeFadeThreshold, 0.5, QuadraticBump(coords.y));
    
    // Add glow
    float4 glow = glowColor * pow(combinedNoise, glowPower);
    
    return (color + glow) * edgeFade;
}
```

---

### ExothermalDisintegrationRayShader.fx
**Purpose:** Disintegration beam with lightning

```hlsl
sampler noiseScrollTexture : register(s1);
sampler lightningScrollTexture : register(s2);

float edgeGlowIntensity;
float3 edgeColorSubtraction;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / coords.z + 0.5;
    
    float4 color = input.Color;
    float edgeFade = QuadraticBump(coords.y);
    
    // Noise layers
    float noise = tex2D(noiseScrollTexture, coords * float2(2, 1) + globalTime * float2(-1.5, 0));
    float lightning = tex2D(lightningScrollTexture, coords * float2(1, 0.5) + globalTime * float2(-2, 0));
    
    // Edge glow subtraction (creates colored edges)
    float3 edgeSubtract = edgeColorSubtraction * (1 - edgeFade) * edgeGlowIntensity;
    color.rgb -= edgeSubtract;
    
    return color * edgeFade * (1 + lightning * 0.2);
}
```

---

### AresEnergyKatanaShader.fx
**Purpose:** Energy blade slash effect

```hlsl
float opacity;
float verticalFlip;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates;
    coords.y = (coords.y - 0.5) / coords.z + 0.5;
    
    // Handle vertical flip for left/right arms
    if (verticalFlip != 0)
        coords.y = 1 - coords.y;
    
    float4 color = input.Color;
    float edgeFade = QuadraticBump(coords.y);
    
    // Sharp edge cutoff
    float sharpEdge = smoothstep(0.1, 0.3, edgeFade);
    
    return color * sharpEdge * opacity;
}
```

---

## Object Shaders

### TeslaExplosionShader.fx
**Purpose:** Expanding electric explosion

```hlsl
sampler noiseTexture : register(s1);

float lifetimeRatio;
float2 textureSize0;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelate
    float2 pixelationFactor = textureSize0 * 0.7;
    coords = floor(coords * pixelationFactor) / pixelationFactor;
    
    // Calculate expanding ring
    float endingRadius = sqrt(lifetimeRatio) * 0.5;
    float startingRadius = endingRadius - lerp(0.32, 0.29, lifetimeRatio);
    float distanceFromCenter = distance(coords, 0.5);
    
    // Ring mask
    float inRing = smoothstep(startingRadius, startingRadius + 0.05, distanceFromCenter) *
                   smoothstep(endingRadius + 0.05, endingRadius, distanceFromCenter);
    
    // Add noise for electric effect
    float noise = tex2D(noiseTexture, coords * 3 + lifetimeRatio);
    
    return sampleColor * inRing * (1 + noise * 0.3);
}
```

---

### HyperfuturisticPortalShader.fx
**Purpose:** Sci-fi portal with swirling patterns

```hlsl
sampler noiseTexture : register(s1);
sampler distanceTexture : register(s2);

bool useTextureForDistanceField;
float scale;
float biasToMainSwirlColorPower;

float EdgeDistance(float2 coords, float angleFromCenter)
{
    float n = 6;  // Hexagonal shape
    float modAngle = angleFromCenter % (6.283 / n);    
    float polygonEquation = 1 / cos(modAngle - 3.141 / n);
    return polygonEquation * 0.4;
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float2 offsetFromCenter = coords - 0.5;
    float distanceFromCenter = length(offsetFromCenter);
    float angleFromCenter = atan2(offsetFromCenter.y, offsetFromCenter.x);
    
    // Swirling noise
    float swirl = tex2D(noiseTexture, coords + globalTime * 0.1);
    
    // Edge calculation
    float edge = EdgeDistance(coords, angleFromCenter + swirl * 0.5);
    float edgeFade = smoothstep(edge, edge - 0.1, distanceFromCenter);
    
    // Color gradient
    float3 portalColor = PaletteLerp(distanceFromCenter + swirl * 0.2);
    
    return float4(portalColor, 1) * edgeFade * sampleColor.a;
}
```

---

### GaussNukeExplosionShader.fx
**Purpose:** Nuclear explosion with shockwave

Similar to TeslaExplosion but with multiple ring layers and more dramatic falloff.

---

## Overlay Modifier Shaders

### GlitchShader.fx
**Purpose:** Hologram/glitch distortion effect

```hlsl
sampler noiseTexture : register(s1);

float time;
float2 textureSize;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Pixelation
    float2 pixelationFactor = 2 / textureSize;
    coords = floor(coords / pixelationFactor) * pixelationFactor;
    
    // Horizontal glitch bars
    float glitchPixelationFactor = 5;
    float timeStepFactor = 11;
    float glitchCoords = floor(coords.y * glitchPixelationFactor) / glitchPixelationFactor 
                       + floor(time * timeStepFactor) / timeStepFactor;    
    float glitchNoise = tex2D(noiseTexture, glitchCoords);
    
    // Offset horizontally
    coords.x += glitchNoise * 0.05 * step(0.8, glitchNoise);
    
    // Sample with offset
    float4 color = tex2D(baseTexture, coords);
    
    // Add static noise
    float glitchStatic = tex2D(noiseTexture, coords * 20 + time * 60) * 0.333;
    color += glitchStatic * color.a * 0.4;
    
    // Bottom fade
    float bottomFade = pow(smoothstep(1, 0.45, coords.y), 0.4);
    
    return color * sampleColor * bottomFade;
}
```

---

### HologramShader.fx
**Purpose:** Translucent holographic overlay

```hlsl
float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(baseTexture, coords);
    
    // Scanline effect
    float scanline = sin(coords.y * 200 + time * 10) * 0.1 + 0.9;
    
    // Color tint
    color.rgb = lerp(color.rgb, float3(0.3, 0.8, 1), 0.3);  // Cyan tint
    
    // Transparency
    color.a *= 0.7 * scanline;
    
    return color * sampleColor;
}
```

---

### AresSilhouetteShader.fx
**Purpose:** Disintegrating silhouette effect

```hlsl
float dissolveInterpolant;
float2 dissolveCenter;
float2 dissolveDirection;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Dissolve offset based on noise
    float dissolveNoise = tex2D(noiseTexture, screenUV * float2(14, 0));
    float2 dissolveOffset = dissolveDirection * dissolveNoise * dissolveInterpolant * 2.4;
    
    // Pixelate the offset
    dissolveOffset = round(dissolveOffset / pixelationFactor) * pixelationFactor;
    coords += dissolveOffset;
    
    // Distance-based dissolve
    float distDissolve = tex2D(noiseTexture, screenUV * 0.002);
    clip(distDissolve - dissolveInterpolant - distance(coords, dissolveCenter) + 0.15);
    
    // Sample as silhouette (single color)
    float4 color = tex2D(baseTexture, coords);
    return any(color) * sampleColor * color.a;
}
```

---

## Special Effect Shaders

### OldDukeBileMetaballShader.fx
**Purpose:** Slimy bile metaball rendering

```hlsl
sampler overlayTexture : register(s1);
sampler dissolveNoiseTexture : register(s2);
sampler bloodTexture : register(s3);

float dissolvePersistence;
float2 maxDistortionOffset;

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // World UV calculation
    float2 worldUV = (coords + layerOffset) * screenSize / layerSize;
    
    // Sample metaball data (R = lifetime, G = darkness, B = dissolve)
    float4 colorData = tex2D(metaballContents, coords);
    float lifetimeRatio = colorData.r;
    float darkening = colorData.g;
    float dissolveInterpolant = colorData.b;
    
    // Distance from center for effects
    float distanceFromCenter = distance(coords, 0.5);
    
    // Dissolve with noise
    float dissolveNoise = tex2D(dissolveNoiseTexture, worldUV);
    float dissolveOpacity = smoothstep(0, dissolvePersistence, dissolveNoise - dissolveInterpolant);
    
    // Palette color based on lifetime and darkness
    float hue = smoothstep(0.75, 0, lifetimeRatio) + darkening * 0.4;
    float3 finalColor = PaletteLerp(hue);
    
    return float4(finalColor, 1) * dissolveOpacity * colorData.a;
}
```

---

### FireParticleDissolveShader.fx
**Purpose:** Animated fire particles with dissolve

```hlsl
sampler baseTextureA : register(s1);
sampler baseTextureB : register(s2);
sampler noiseTexture : register(s3);

float turbulence;
float initialGlowIntensity;
float initialGlowDuration;

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
    float2 coords = input.TextureCoordinates.xy;
    float lifetime = input.TextureCoordinates.z;
    
    // Pixelate
    coords = floor(coords * pixelationLevel) / pixelationLevel;
    
    // Blend between two fire textures
    float4 texA = tex2D(baseTextureA, coords);
    float4 texB = tex2D(baseTextureB, coords);
    float4 fireColor = lerp(texA, texB, sin(lifetime * 3.14) * 0.5 + 0.5);
    
    // Turbulence distortion
    float noise = tex2D(noiseTexture, coords + globalTime * turbulence);
    
    // Initial glow boost
    float glowBoost = 1 + initialGlowIntensity * 
                      smoothstep(initialGlowDuration, 0, lifetime);
    
    return fireColor * input.Color * glowBoost;
}
```

---

### NuclearHurricaneShaders (Multiple)

A family of shaders for the Old Duke's hurricane attack:

- **NuclearHurricaneCoreShader.fx** - Inner swirling core
- **NuclearHurricaneGlowShader.fx** - Outer glow ring
- **NuclearHurricaneFoamShader.fx** - Frothy foam layer
- **NuclearHurricaneExtremesShader.fx** - Top/bottom caps

All use similar vertex deformation for cylindrical shape:

```hlsl
VertexShaderOutput VertexShaderFunction(in VertexShaderInput input)
{
    float2 coords = input.TextureCoordinates;
    float bump = QuadraticBump(coords.y);
    
    // Wavy distortion
    input.Position.x += sin(coords.y * 6.283 + localTime) * bump * wavinessFactor;
    
    // Squish at bump peak
    input.Position.x *= lerp(1, maxBumpSquish, bump);
    
    // ... standard transform
}
```

---

## Filter Shaders

### HeatDistortionFilter.fx
**Purpose:** Screen-space heat shimmer

```hlsl
sampler heatMetaballsTexture : register(s2);
sampler heatNoiseTexture : register(s3);

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Sample heat intensity from metaball render target
    float heatIntensity = tex2D(heatMetaballsTexture, (coords - 0.5) / screenZoom + 0.5);
    
    // Animated distortion angle
    float heatAngle = tex2D(heatNoiseTexture, coords * 0.5 + float2(0, globalTime * 0.03)).r * 16 
                    + globalTime * 0.5;
    
    // Distortion direction
    float2 heatDirection = float2(cos(heatAngle), sin(heatAngle));
    
    // Sample screen with offset
    return tex2D(screenTexture, coords + heatDirection * heatIntensity * opacity * 0.0024);
}
```

---

### OldDukeRainShader.fx
**Purpose:** Acid rain screen filter

```hlsl
float rainAngle;
float rainOpacity;

float2 RotatedBy(float2 v, float theta)
{
    float s = sin(theta);
    float c = cos(theta);
    return float2(v.x * c - v.y * s, v.x * s + v.y * c);
}

float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    // Rotate and scale for rain streaks
    float2 rainCoords = RotatedBy(coords - 0.5, rainAngle) * float2(0.8, 8.5) / zoom + 0.5;
    
    // Animated rain
    float rain = tex2D(noiseTexture, rainCoords + float2(time * 1.5, 0));
    
    // Apply rain overlay
    float4 baseColor = tex2D(baseTexture, coords);
    return lerp(baseColor, rainColor, rain * rainOpacity);
}
```

---

## C# Shader Loading Patterns

```csharp
// Getting a shader by name
ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.ShaderName");

// Setting float parameters
shader.TrySetParameter("lifetimeRatio", 0.5f);
shader.TrySetParameter("glowIntensity", 1.2f);

// Setting Vector2/3/4 parameters
shader.TrySetParameter("laserDirection", direction.ToVector3());
shader.TrySetParameter("textureSize0", texture.Size());

// Setting array parameters
float[] lengthRatios = { 1f, 0.8f, 0.6f };
shader.TrySetParameter("lengthRatios", lengthRatios);

// Setting gradient arrays
Vector3[] palette = {
    new Color(255, 100, 0).ToVector3(),
    new Color(255, 50, 0).ToVector3(),
    new Color(100, 0, 0).ToVector3()
};
shader.TrySetParameter("gradient", palette);
shader.TrySetParameter("gradientCount", palette.Length);

// Setting textures (slot 1, 2, 3...)
shader.SetTexture(MiscTexturesRegistry.WavyBlotchNoise.Value, 1, SamplerState.LinearWrap);
shader.SetTexture(noiseTexture, 2, SamplerState.LinearClamp);

// Applying shader
shader.Apply();

// Using with SpriteBatch
Main.spriteBatch.PrepareForShaders();
shader.Apply();
// ... draw calls ...
Main.spriteBatch.ResetToDefault();
```

---

## MagnumOpus Shader Adaptation Guide

### Naming Convention
Change `FargowiltasCrossmod.ShaderName` to `MagnumOpus.ShaderName`

### Required Textures to Create
- Noise textures (wavy, dendritic, perlin)
- Bloom circle texture
- Shine flare texture

### Theme-Specific Shader Parameters
Create preset parameter sets for each theme:

```csharp
public static class FateShaderPresets
{
    public static Vector3[] CosmicGradient = {
        new Color(15, 5, 20).ToVector3(),    // Black
        new Color(180, 50, 100).ToVector3(), // Dark pink
        new Color(255, 60, 80).ToVector3(),  // Bright red
    };
    
    public static float GlowIntensity = 1.5f;
    public static float EdgeFadeThreshold = 0.15f;
}
```

---

*Extracted from FargosSoulsDLC for MagnumOpus VFX development reference.*
