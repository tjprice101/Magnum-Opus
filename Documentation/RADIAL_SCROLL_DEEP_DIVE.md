# Radial Scroll Deep Dive - Expert-Level Implementation Guide

> **COMPREHENSIVE TECHNICAL DOCUMENTATION**
> 
> This document provides expert-level coverage of radial scroll effects, polar coordinate transformations, flow mapping, and advanced VFX techniques for use in tModLoader/MonoGame projects.

---

## 📖 Table of Contents

1. [Theoretical Foundations](#1-theoretical-foundations)
2. [Polar Coordinate Mathematics](#2-polar-coordinate-mathematics)
3. [Core Radial Scroll Technique](#3-core-radial-scroll-technique)
4. [HLSL Implementation Patterns](#4-hlsl-implementation-patterns)
5. [Advanced Flow Mapping](#5-advanced-flow-mapping)
6. [Seamless Animation Techniques](#6-seamless-animation-techniques)
7. [C# Integration for tModLoader](#7-c-integration-for-tmodloader)
8. [Performance Optimization](#8-performance-optimization)
9. [Use Cases & Applications](#9-use-cases--applications)
10. [Complete Reference Implementation](#10-complete-reference-implementation)

---

## 1. Theoretical Foundations

### 1.1 What is Radial Scrolling?

Radial scrolling is a UV manipulation technique that converts standard Cartesian (X,Y) texture coordinates into polar (radius, angle) space, then animates along one or both axes. This creates circular, spiral, vortex, and radially-expanding effects that are impossible to achieve with simple X/Y scrolling.

**Key Insight:** In polar space:
- **Radius axis** controls distance from center → animating creates inward/outward flow
- **Angle axis** controls rotational position → animating creates spinning/vortex effects
- **Combining both** creates spiraling effects

### 1.2 Coordinate Systems

```
CARTESIAN (Standard UV)           POLAR (Radial)
─────────────────────            ─────────────────
        ▲ Y                        ▲ angle (θ)
        │                          │  ╱╲
        │                          │ ╱  ╲ radius (r)
        │                          │╱    ╲
   ─────┼─────▶ X              ────●──────▶ radius
        │                     (center)
        │
        ▼

UV Space: (0,0)──────(1,0)      Polar Space: 
              │          │       r: 0 (center) → 0.707+ (corners)
              │          │       θ: -π (left) → +π (right)
         (0,1)──────(1,1)           (wraps at ±π)
```

### 1.3 The Mathematical Transform

**Cartesian → Polar:**
$$r = \sqrt{(x - c_x)^2 + (y - c_y)^2}$$
$$\theta = \text{atan2}(y - c_y, x - c_x)$$

**Polar → Cartesian:**
$$x = r \cdot \cos(\theta) + c_x$$
$$y = r \cdot \sin(\theta) + c_y$$

Where $(c_x, c_y)$ is the center point (typically 0.5, 0.5 for UV space).

---

## 2. Polar Coordinate Mathematics

### 2.1 The atan2 Function

The `atan2(y, x)` function is **critical** for polar conversion. Unlike `atan(y/x)`, it handles all four quadrants correctly:

```hlsl
// atan2 returns angle in radians: [-π, +π]
// Maps full 360° rotation unambiguously
float angle = atan2(y, x);
```

**Quadrant Mapping:**
| Quadrant | x sign | y sign | atan2 range |
|----------|--------|--------|-------------|
| I (top-right) | + | + | [0, π/2] |
| II (top-left) | - | + | [π/2, π] |
| III (bottom-left) | - | - | [-π, -π/2] |
| IV (bottom-right) | + | - | [-π/2, 0] |

### 2.2 Normalization for UV Space

For texture sampling, we need to map polar coordinates back to [0, 1] range:

```hlsl
// TAU = 2π = 6.283185307 (full rotation in radians)
#define TAU 6.283185307

float2 polar_coordinates(float2 uv, float2 center, float zoom, float repeat)
{
    // Step 1: Center the coordinate system
    float2 dir = uv - center;
    
    // Step 2: Calculate radius (distance from center)
    // Multiply by 2 so radius reaches ~1.0 at UV edges (instead of 0.5)
    float radius = length(dir) * 2.0;
    
    // Step 3: Calculate angle, normalize to [0, 1] range
    // atan2 returns [-π, π], divide by TAU to get [-0.5, 0.5]
    // The 1.0/TAU normalizes full rotation to unit range
    float angle = atan2(dir.y, dir.x) * (1.0 / TAU);
    
    // Step 4: Apply zoom (scales radius) and repeat (tiles angle)
    // Zoom > 1: Texture appears smaller (zoomed out)
    // Repeat > 1: Texture repeats around the circle
    return float2(radius * zoom, angle * repeat);
}
```

### 2.3 Seamless Angular Tiling

A critical issue: angle ranges from -0.5 to 0.5, creating a seam at the left side. To tile seamlessly:

```hlsl
// Option 1: Use frac() - wraps to [0, 1]
float angle_tiled = frac(angle);

// Option 2: GLSL-compatible mod for symmetry
float glslmod(float x, float y)
{
    return x - y * floor(x / y);
}
float angle_seamless = glslmod(angle, 1.0);

// Option 3: Shift angle to [0, 1] before any operations
float angle_normalized = angle * 0.5 + 0.5; // [-0.5, 0.5] → [0, 1]
```

---

## 3. Core Radial Scroll Technique

### 3.1 Basic Radial Scroll Shader

```hlsl
// =============================================================================
// RADIAL SCROLL SHADER - Foundational Implementation
// =============================================================================
// Creates spinning/flowing radial effects by scrolling UV in polar space
// =============================================================================

sampler uImage0 : register(s0);  // Main texture

float uTime;           // Animation time (Main.GlobalTimeWrappedHourly)
float uFlowSpeed;      // Radial flow speed (positive = outward)
float uSpinSpeed;      // Rotational speed (positive = counter-clockwise)
float uZoom;           // Texture zoom (1.0 = normal)
float uRepeat;         // Angular repetitions (1.0 = once around)

#define TAU 6.283185307
#define CENTER float2(0.5, 0.5)

// GLSL-compatible modulo (handles negatives correctly)
float glslmod(float x, float y)
{
    return x - y * floor(x / y);
}

float2 polar_coordinates(float2 uv, float2 center, float zoom, float repeat)
{
    float2 dir = uv - center;
    float radius = length(dir) * 2.0;
    float angle = atan2(dir.y, dir.x) / TAU;
    
    return float2(radius * zoom, angle * repeat);
}

float4 PSMain(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Convert to polar coordinates
    float2 polar = polar_coordinates(uv, CENTER, uZoom, uRepeat);
    
    // Animate in polar space
    polar.x -= uTime * uFlowSpeed;  // Radial flow (outward when positive)
    polar.y += uTime * uSpinSpeed;  // Angular rotation
    
    // Wrap coordinates for seamless tiling
    polar = float2(glslmod(polar.x, 1.0), glslmod(polar.y, 1.0));
    
    // Sample texture with polar UVs
    float4 texColor = tex2D(uImage0, polar);
    
    return texColor * color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PSMain();
    }
}
```

### 3.2 Understanding the Animation Axes

| Parameter | Effect | Visual Result |
|-----------|--------|---------------|
| `polar.x -= time` | Decrease radius over time | **Inward flow** (sucking in) |
| `polar.x += time` | Increase radius over time | **Outward flow** (expanding) |
| `polar.y += time` | Increase angle over time | **Counter-clockwise spin** |
| `polar.y -= time` | Decrease angle over time | **Clockwise spin** |
| Both += time | Outward + CCW | **Spiral outward CCW** |
| x += time, y -= time | Outward + CW | **Spiral outward CW** |

### 3.3 Adding Visual Enhancements

```hlsl
float4 PSEnhanced(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Polar coordinates
    float2 polar = polar_coordinates(uv, CENTER, uZoom, uRepeat);
    
    // Animate
    polar.x -= uTime * uFlowSpeed;
    polar.y += uTime * uSpinSpeed;
    polar = float2(glslmod(polar.x, 1.0), glslmod(polar.y, 1.0));
    
    // Sample base texture
    float4 texColor = tex2D(uImage0, polar);
    
    // === ENHANCEMENT 1: Distance-based fade (vignette) ===
    float2 centered = uv - CENTER;
    float dist = length(centered);
    float vignette = smoothstep(0.7, 0.0, dist);  // Fade at edges
    
    // === ENHANCEMENT 2: Intensity based on distance ===
    float intensity = 1.0 - dist * 0.5;  // Brighter at center
    
    // === ENHANCEMENT 3: Color tinting based on angle ===
    float angle01 = atan2(centered.y, centered.x) / TAU + 0.5;
    float3 tint = lerp(uColor1.rgb, uColor2.rgb, angle01);
    
    // Combine
    float4 result = texColor;
    result.rgb *= tint * intensity;
    result.a *= vignette;
    
    return result * color;
}
```

---

## 4. HLSL Implementation Patterns

### 4.1 Multi-Layer Radial Scroll (Depth Effect)

Layering multiple radial scrolls at different speeds creates organic, complex visuals:

```hlsl
float4 PSMultiLayer(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    float4 result = float4(0, 0, 0, 0);
    
    // Layer 1: Slow background
    float2 polar1 = polar_coordinates(uv, CENTER, 0.5, 1.0);
    polar1.x -= uTime * 0.1;
    polar1.y += uTime * 0.05;
    float4 layer1 = tex2D(uImage0, frac(polar1)) * 0.3;
    
    // Layer 2: Medium middle
    float2 polar2 = polar_coordinates(uv, CENTER, 1.0, 2.0);
    polar2.x -= uTime * 0.25;
    polar2.y += uTime * 0.1;
    float4 layer2 = tex2D(uImage0, frac(polar2)) * 0.5;
    
    // Layer 3: Fast foreground
    float2 polar3 = polar_coordinates(uv, CENTER, 2.0, 4.0);
    polar3.x -= uTime * 0.5;
    polar3.y += uTime * 0.2;
    float4 layer3 = tex2D(uImage0, frac(polar3)) * 0.7;
    
    // Additive blend (glow effect)
    result = layer1 + layer2 + layer3;
    result.a = max(max(layer1.a, layer2.a), layer3.a);
    
    return result * color;
}
```

### 4.2 Distortion-Enhanced Radial Scroll

Adding noise-based distortion creates organic, fluid motion:

```hlsl
sampler uDistortTexture : register(s1);  // Noise texture

float uDistortStrength;  // 0.0 - 0.2 typical

float4 PSDistorted(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Sample distortion texture (animated)
    float2 distortUV = uv * 2.0 + float2(uTime * 0.1, uTime * 0.15);
    float2 distortion = tex2D(uDistortTexture, frac(distortUV)).rg;
    distortion = (distortion - 0.5) * 2.0;  // Remap [0,1] → [-1,1]
    
    // Apply distortion to UV before polar conversion
    float2 distortedUV = uv + distortion * uDistortStrength;
    
    // Convert to polar (now with organic warping)
    float2 polar = polar_coordinates(distortedUV, CENTER, uZoom, uRepeat);
    
    // Animate
    polar.x -= uTime * uFlowSpeed;
    polar.y += uTime * uSpinSpeed;
    
    // Sample
    float4 texColor = tex2D(uImage0, frac(polar));
    
    return texColor * color;
}
```

### 4.3 Gradient-Mapped Radial Scroll

Using a 1D gradient texture for color mapping:

```hlsl
sampler uCausticTexture : register(s0);   // Caustic/noise pattern (grayscale works)
sampler uGradientTexture : register(s1);  // 1D gradient (horizontal color ramp)

float4 PSGradientMapped(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Polar scroll for caustic pattern
    float2 polar = polar_coordinates(uv, CENTER, uZoom, uRepeat);
    polar.x -= uTime * uFlowSpeed;
    polar.y += uTime * uSpinSpeed;
    
    // Sample caustic as grayscale intensity
    float intensity = tex2D(uCausticTexture, frac(polar)).r;
    
    // Use intensity to sample gradient texture
    // Gradient is horizontal, so sample at (intensity, 0.5)
    float4 gradientColor = tex2D(uGradientTexture, float2(intensity, 0.5));
    
    // Apply color intensity and vignette
    float dist = length(uv - CENTER);
    float vignette = 1.0 - smoothstep(0.3, 0.7, dist);
    
    return gradientColor * intensity * vignette * color;
}
```

---

## 5. Advanced Flow Mapping

### 5.1 Flow Map Concept

A **flow map** is a texture where the RG channels encode 2D flow direction vectors. This allows **variable flow direction** across the surface.

```
Flow Map Encoding:
  R channel = X velocity (-1 to +1, stored as 0-255)
  G channel = Y velocity (-1 to +1, stored as 0-255)
  
  Red (1,0) = Flow right
  Green (0,1) = Flow up
  Purple (0.5, 0.5) = Neutral (no flow)
  Teal (0, 1) = Flow up
```

### 5.2 Basic Flow Map Implementation

```hlsl
sampler uFlowMap : register(s1);

float4 PSFlowMap(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Sample flow direction from flow map
    float2 flowDir = tex2D(uFlowMap, uv).rg;
    flowDir = (flowDir - 0.5) * 2.0;  // Remap [0,1] → [-1,1]
    
    // Apply flow to UV
    float2 flowedUV = uv + flowDir * uTime * uFlowSpeed;
    
    // Sample texture with flowed UVs
    return tex2D(uImage0, frac(flowedUV)) * color;
}
```

### 5.3 Dual-Phase Blending (Catlike Coding Pattern)

**Problem:** Simple flow creates visible "pulse" when animation loops.

**Solution:** Blend two phases offset by 0.5, using triangle wave weights.

```hlsl
// Triangle wave weight function: 0→1→0 over 0→1 input
float TriangleWave(float x)
{
    return 1.0 - abs(1.0 - 2.0 * frac(x));
}

// Flow UV function with phase
float2 FlowUV(float2 uv, float2 flowDir, float time, float phase)
{
    float progress = frac(time + phase);
    return uv - flowDir * progress;
}

float4 PSDualPhaseFlow(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Sample flow direction
    float2 flowDir = tex2D(uFlowMap, uv).rg * 2.0 - 1.0;
    
    // Calculate phase A and B (offset by 0.5)
    float time = uTime * uFlowSpeed;
    float2 uvA = FlowUV(uv, flowDir, time, 0.0);
    float2 uvB = FlowUV(uv, flowDir, time, 0.5);
    
    // Sample both phases
    float4 texA = tex2D(uImage0, frac(uvA));
    float4 texB = tex2D(uImage0, frac(uvB));
    
    // Blend weights (triangle waves, 180° out of phase)
    float weightA = TriangleWave(time);
    float weightB = TriangleWave(time + 0.5);
    
    // Normalize weights
    float totalWeight = weightA + weightB;
    weightA /= totalWeight;
    weightB /= totalWeight;
    
    // Blend
    float4 result = texA * weightA + texB * weightB;
    
    return result * color;
}
```

### 5.4 Time Offset via Noise

Adding per-pixel time offset breaks up uniform animation:

```hlsl
sampler uNoiseTexture : register(s2);

float4 PSTimeOffsetFlow(float4 color : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Sample noise for time offset (static noise texture)
    float timeOffset = tex2D(uNoiseTexture, uv * 0.5).r;
    
    // Each pixel has slightly different animation phase
    float localTime = (uTime + timeOffset * uTimeOffsetStrength) * uFlowSpeed;
    
    // Flow direction from flow map
    float2 flowDir = tex2D(uFlowMap, uv).rg * 2.0 - 1.0;
    
    // Dual-phase with local time
    float2 uvA = uv - flowDir * frac(localTime);
    float2 uvB = uv - flowDir * frac(localTime + 0.5);
    
    float4 texA = tex2D(uImage0, frac(uvA));
    float4 texB = tex2D(uImage0, frac(uvB));
    
    float weight = TriangleWave(localTime);
    
    return lerp(texB, texA, weight) * color;
}
```

---

## 6. Seamless Animation Techniques

### 6.1 The Seam Problem

When scrolling in polar space, seams appear when:
1. The texture doesn't tile
2. The angle wraps at ±π (left side)
3. The radius approaches center (singularity)

### 6.2 Solutions

#### A. Texture Preparation (External)
- Ensure textures tile seamlessly in both X and Y
- Use tileable noise/caustic generators
- For center singularity: fade opacity to 0 at center

#### B. Shader-Side Fixes

```hlsl
// Fix 1: Center fade (hide singularity)
float centerFade = smoothstep(0.0, 0.1, length(uv - CENTER));

// Fix 2: Seamless angle wrapping
float angle = atan2(dir.y, dir.x);
angle = angle / TAU + 0.5;  // [0, 1] instead of [-0.5, 0.5]

// Fix 3: Soft edge blending
float2 seamBlend = smoothstep(0.0, 0.1, min(polar, 1.0 - polar));
float seamMask = seamBlend.x * seamBlend.y;
```

#### C. Derivative Maps Instead of Normal Maps

For effects requiring surface normals (lighting), use derivative maps computed on-the-fly:

```hlsl
// Compute derivatives for normal reconstruction
float height = tex2D(uHeightMap, uv).r;
float2 derivatives = float2(ddx(height), ddy(height));

// Reconstruct normal from derivatives
float3 normal = normalize(float3(-derivatives.x, -derivatives.y, 1.0));
```

### 6.3 UV Jumping (Advanced)

To prevent looping artifacts over long play sessions:

```hlsl
// "Jump" UV offset at fixed intervals to prevent precision loss
float jumpInterval = 10.0;  // Seconds
float jumpPhase = floor(uTime / jumpInterval);
float localTime = frac(uTime / jumpInterval) * jumpInterval;

// Use localTime for animation, jumpPhase for seeding random offset
float2 jumpOffset = Hash2D(float2(jumpPhase, jumpPhase * 0.7)) * 1000.0;
float2 animatedUV = uv + flowDir * localTime + jumpOffset;
```

---

## 7. C# Integration for tModLoader

### 7.1 Effect Loading and Parameter Binding

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;

public class RadialScrollRenderer
{
    private Effect _effect;
    private Texture2D _causticTexture;
    private Texture2D _gradientTexture;
    private Texture2D _distortTexture;
    
    public float FlowSpeed { get; set; } = 0.3f;
    public float SpinSpeed { get; set; } = 0.1f;
    public float Zoom { get; set; } = 1.0f;
    public float Repeat { get; set; } = 1.0f;
    public float DistortStrength { get; set; } = 0.05f;
    public float ColorIntensity { get; set; } = 1.5f;
    public float VignetteSize { get; set; } = 0.3f;
    
    public void Load(Mod mod)
    {
        // Load shader effect
        _effect = ModContent.Request<Effect>(
            "MagnumOpus/Assets/Shaders/RadialScroll",
            AssetRequestMode.ImmediateLoad
        ).Value;
        
        // Load textures
        _causticTexture = ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX/Caustic",
            AssetRequestMode.ImmediateLoad
        ).Value;
        
        _gradientTexture = ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX/FireGradient",
            AssetRequestMode.ImmediateLoad
        ).Value;
        
        _distortTexture = ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX/PerlinNoise",
            AssetRequestMode.ImmediateLoad
        ).Value;
    }
    
    public void ApplyEffect()
    {
        if (_effect == null) return;
        
        // Set time (wrapped to prevent precision issues)
        _effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
        
        // Set animation parameters
        _effect.Parameters["uFlowSpeed"]?.SetValue(FlowSpeed);
        _effect.Parameters["uSpinSpeed"]?.SetValue(SpinSpeed);
        _effect.Parameters["uZoom"]?.SetValue(Zoom);
        _effect.Parameters["uRepeat"]?.SetValue(Repeat);
        _effect.Parameters["uDistortStrength"]?.SetValue(DistortStrength);
        _effect.Parameters["uColorIntensity"]?.SetValue(ColorIntensity);
        _effect.Parameters["uVignetteSize"]?.SetValue(VignetteSize);
        
        // Set textures
        Main.graphics.GraphicsDevice.Textures[1] = _gradientTexture;
        Main.graphics.GraphicsDevice.Textures[2] = _distortTexture;
        
        // Apply effect
        _effect.CurrentTechnique.Passes[0].Apply();
    }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Texture2D baseTexture, 
                     Color color, float rotation, float scale)
    {
        // End current SpriteBatch (to change effect)
        spriteBatch.End();
        
        // Begin with shader effect
        spriteBatch.Begin(
            SpriteSortMode.Immediate,
            BlendState.Additive,
            SamplerState.LinearWrap,  // IMPORTANT: Wrap for seamless tiling
            DepthStencilState.None,
            RasterizerState.CullNone,
            _effect,
            Main.GameViewMatrix.TransformationMatrix
        );
        
        // Apply parameters
        ApplyEffect();
        
        // Draw
        Vector2 origin = baseTexture.Size() * 0.5f;
        spriteBatch.Draw(
            baseTexture,
            position - Main.screenPosition,
            null,
            color,
            rotation,
            origin,
            scale,
            SpriteEffects.None,
            0f
        );
        
        // Restore SpriteBatch to default
        spriteBatch.End();
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            Main.DefaultSamplerState,
            DepthStencilState.None,
            Main.Rasterizer,
            null,
            Main.GameViewMatrix.TransformationMatrix
        );
    }
}
```

### 7.2 Integration with Projectile PreDraw

```csharp
public class RadialOrbProjectile : ModProjectile
{
    private RadialScrollRenderer _renderer;
    
    public override void SetStaticDefaults()
    {
        ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
        ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
    }
    
    public override void SetDefaults()
    {
        Projectile.width = 48;
        Projectile.height = 48;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.tileCollide = false;
        Projectile.timeLeft = 300;
        Projectile.alpha = 0;
        
        // Initialize renderer
        _renderer = new RadialScrollRenderer
        {
            FlowSpeed = 0.4f,
            SpinSpeed = 0.15f,
            Zoom = 1.2f,
            ColorIntensity = 2.0f
        };
        _renderer.Load(Mod);
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        SpriteBatch spriteBatch = Main.spriteBatch;
        Texture2D orbTexture = ModContent.Request<Texture2D>(
            "MagnumOpus/Assets/VFX/RadialOrb"
        ).Value;
        
        // Pulsing scale
        float pulse = 1f + MathF.Sin(Main.GlobalTimeWrappedHourly * 8f) * 0.1f;
        float scale = 1.5f * pulse;
        
        // Color cycling
        float hue = (Main.GlobalTimeWrappedHourly * 0.5f) % 1f;
        Color orbColor = Main.hslToRgb(hue, 1f, 0.8f);
        
        // Draw with radial scroll effect
        _renderer.Draw(
            spriteBatch,
            Projectile.Center,
            orbTexture,
            orbColor,
            Projectile.rotation,
            scale
        );
        
        return false;  // Skip vanilla drawing
    }
}
```

### 7.3 PixelationSystem Integration (VFX+ Style)

For compatibility with VFX+ mod's layered rendering:

```csharp
using VFXPlus;

public class RadialScrollOverride : GlobalItem
{
    public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
    {
        // Register with VFX+ pixelation layers
        if (item.type == ItemID.MagnetSphere)
        {
            PixelationSystem.AddToQueue(
                RenderLayer.Dusts,
                () => DrawRadialEffect()
            );
        }
    }
    
    private void DrawRadialEffect()
    {
        Effect radialEffect = ModContent.Request<Effect>(
            "VFXPlus/Effects/Radial/NewRadialScroll",
            AssetRequestMode.ImmediateLoad
        ).Value;
        
        radialEffect.Parameters["uTime"].SetValue(Main.GlobalTimeWrappedHourly * 0.5f);
        radialEffect.Parameters["flowSpeed"].SetValue(0.03f);
        radialEffect.Parameters["distortStrength"].SetValue(0.4f);
        radialEffect.Parameters["colorIntensity"].SetValue(1.5f);
        radialEffect.Parameters["vignetteSize"].SetValue(1.2f);
        radialEffect.Parameters["vignetteBlend"].SetValue(1.5f);
        
        // Bind textures
        Main.graphics.GraphicsDevice.Textures[1] = _gradientTexture;
        Main.graphics.GraphicsDevice.Textures[2] = _causticTexture;
        Main.graphics.GraphicsDevice.Textures[3] = _distortTexture;
        
        radialEffect.CurrentTechnique.Passes[0].Apply();
        
        // Draw at projectile positions
        foreach (var proj in Main.projectile.Where(p => p.active && p.type == myType))
        {
            DrawRadialAtPosition(proj.Center);
        }
    }
}
```

---

## 8. Performance Optimization

### 8.1 Shader Model Considerations

**tModLoader Target: ps_2_0 / vs_2_0**

| Feature | ps_2_0 Limit | Optimization |
|---------|--------------|--------------|
| Instruction count | 64 arithmetic | Simplify math, precompute |
| Texture samples | 8 per pass | Combine textures into atlases |
| Registers | 32 constant, 12 temp | Minimize temporaries |
| Dynamic branching | Limited | Use step()/lerp() instead |

### 8.2 Optimization Techniques

```hlsl
// 1. PRECOMPUTE CONSTANTS
// BAD: Calculate TAU every pixel
float angle = atan2(y, x) / 6.283185;

// GOOD: Define as constant
#define INV_TAU 0.159154943  // 1.0 / TAU
float angle = atan2(y, x) * INV_TAU;


// 2. USE MAD (MULTIPLY-ADD) OPERATIONS
// Compiler optimizes a * b + c to single MAD instruction
float result = a * b + c;  // Fast!


// 3. AVOID DEPENDENT TEXTURE READS
// BAD: Sample, then use result for next sample UV
float4 noise = tex2D(noiseSampler, uv);
float4 main = tex2D(mainSampler, uv + noise.rg);  // Stalls!

// BETTER: Precalculate offsets or use separate passes


// 4. MINIMIZE BRANCHES
// BAD: Dynamic branch
if (dist > threshold) discard;

// GOOD: Predication
float mask = step(threshold, dist);
color *= mask;


// 5. USE SATURATE INSTEAD OF CLAMP
// BAD
clamp(x, 0.0, 1.0);

// GOOD (free on many GPUs)
saturate(x);


// 6. VECTORIZE OPERATIONS
// BAD: Scalar operations
float r = a.r * b.r;
float g = a.g * b.g;
float b = a.b * b.b;

// GOOD: Vector operation (single instruction)
float3 result = a.rgb * b.rgb;
```

### 8.3 Texture Recommendations

| Texture | Format | Size | Notes |
|---------|--------|------|-------|
| Caustic | DXT1/BC1 (RGB) | 256×256 | Tileable, 8 bits per channel sufficient |
| Gradient | Uncompressed RGBA | 256×1 | 1D lookup, smooth gradients need precision |
| Noise | DXT1/BC1 (RGB) | 128×128 | Perlin/Simplex, tileable |
| Flow Map | Uncompressed RG | 128×128 | Precision needed for vectors |

### 8.4 LOD Considerations

For distant effects, reduce quality:

```csharp
float distance = Vector2.Distance(Main.LocalPlayer.Center, effectPosition);
float lodFactor = MathHelper.Clamp(1f - distance / 1000f, 0.3f, 1f);

_effect.Parameters["uDetailLevel"]?.SetValue(lodFactor);
_effect.Parameters["uLayerCount"]?.SetValue((int)(4 * lodFactor));
```

---

## 9. Use Cases & Applications

### 9.1 Magic Orbs & Spheres

**Settings:**
- FlowSpeed: 0.3-0.5 (inward for absorption, outward for emission)
- SpinSpeed: 0.1-0.2 (subtle rotation)
- DistortStrength: 0.05-0.1 (organic wobble)
- Vignette: Strong center fade

**Effect Stack:**
1. Base radial scroll (caustic pattern)
2. Additive glow layer (bloom shader)
3. Particle halo (standard particles)

### 9.2 Portals & Vortexes

**Settings:**
- FlowSpeed: 0.5-1.0 (strong inward pull)
- SpinSpeed: 0.3-0.5 (noticeable rotation)
- Repeat: 2-4 (multiple spiral arms)
- DistortStrength: 0.1-0.2 (warped reality)

**Effect Stack:**
1. Background void (dark radial)
2. Energy spiral (bright radial scroll)
3. Edge glow (rim lighting)
4. Particle suction (particles with radial velocity)

### 9.3 Auras & Shields

**Settings:**
- FlowSpeed: 0.1-0.2 (slow ambient motion)
- SpinSpeed: 0.05 (barely perceptible)
- Vignette: Inverted (bright edges, transparent center)
- Opacity: 0.3-0.5 (subtle overlay)

### 9.4 Energy Weapons (Charging)

**Settings:**
- FlowSpeed: Start at 0.1, ramp to 1.0 during charge
- Scale: Start at 0.5, grow to 2.0
- ColorIntensity: Ramp with charge level
- Distortion: Increase with power

```csharp
float chargeProgress = currentCharge / maxCharge;
_effect.Parameters["uFlowSpeed"]?.SetValue(0.1f + chargeProgress * 0.9f);
_effect.Parameters["uScale"]?.SetValue(0.5f + chargeProgress * 1.5f);
_effect.Parameters["uColorIntensity"]?.SetValue(1f + chargeProgress * 2f);
```

### 9.5 Water/Liquid Surfaces

**Settings:**
- FlowSpeed: 0.05-0.1 (gentle movement)
- SpinSpeed: 0 (no rotation for water)
- DistortStrength: 0.02-0.05 (subtle ripples)
- Use dual-phase blending for seamless loops

---

## 10. Complete Reference Implementation

### 10.1 Full-Featured Radial Scroll Shader

```hlsl
// =============================================================================
// MagnumOpus Advanced Radial Scroll Shader
// Complete implementation with all features
// =============================================================================

// Samplers
sampler uImage0 : register(s0);        // Base texture (caustic/noise)
sampler uGradientTex : register(s1);   // Color gradient lookup
sampler uDistortTex : register(s2);    // Distortion noise

// Parameters
float uTime;
float uFlowSpeed;       // Radial flow (+ = outward)
float uSpinSpeed;       // Angular rotation
float uZoom;            // Texture scale
float uRepeat;          // Angular repetitions
float uDistortStrength; // Distortion intensity
float uColorIntensity;  // Color brightness multiplier
float uVignetteSize;    // Vignette distance
float uVignetteBlend;   // Vignette softness
float3 uTint;           // Color tint
float uAlpha;           // Overall opacity

// Constants
#define TAU 6.283185307
#define INV_TAU 0.159154943
#define CENTER float2(0.5, 0.5)

// Utility functions
float glslmod(float x, float y)
{
    return x - y * floor(x / y);
}

float2 glslmod2(float2 v, float2 m)
{
    return float2(glslmod(v.x, m.x), glslmod(v.y, m.y));
}

float2 polar_coordinates(float2 uv, float2 center, float zoom, float repeat)
{
    float2 dir = uv - center;
    float radius = length(dir) * 2.0;
    float angle = atan2(dir.y, dir.x) * INV_TAU;
    return float2(radius * zoom, angle * repeat);
}

// Main pixel shader
float4 PSRadialScroll(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // === DISTORTION PASS ===
    float2 distortUV = uv * 3.0;
    distortUV.x += uTime * 0.1;
    distortUV.y += uTime * 0.07;
    float2 distortion = tex2D(uDistortTex, frac(distortUV)).rg;
    distortion = (distortion - 0.5) * 2.0 * uDistortStrength;
    
    float2 distortedUV = uv + distortion;
    
    // === POLAR CONVERSION ===
    float2 polar = polar_coordinates(distortedUV, CENTER, uZoom, uRepeat);
    
    // === ANIMATION ===
    polar.x -= uTime * uFlowSpeed;  // Radial scroll
    polar.y += uTime * uSpinSpeed;  // Angular rotation
    
    // Wrap for seamless tiling
    polar = glslmod2(polar, float2(1.0, 1.0));
    
    // === TEXTURE SAMPLING ===
    float4 caustic = tex2D(uImage0, polar);
    
    // === GRADIENT MAPPING ===
    float intensity = caustic.r;
    float4 gradientColor = tex2D(uGradientTex, float2(intensity, 0.5));
    
    // === VIGNETTE ===
    float2 centered = uv - CENTER;
    float dist = length(centered);
    float vignette = 1.0 - smoothstep(uVignetteSize, uVignetteSize + uVignetteBlend, dist);
    
    // === CENTER FADE (hide singularity) ===
    float centerFade = smoothstep(0.0, 0.08, dist);
    
    // === COMPOSITING ===
    float3 finalColor = gradientColor.rgb * uTint * intensity * uColorIntensity;
    float finalAlpha = vignette * centerFade * caustic.a * uAlpha;
    
    return float4(finalColor, finalAlpha) * sampleColor;
}

// Dual-phase version for seamless animation
float4 PSDualPhaseRadial(float4 sampleColor : COLOR0, float2 uv : TEXCOORD0) : COLOR0
{
    // Distortion
    float2 distortUV = uv * 3.0 + uTime * float2(0.1, 0.07);
    float2 distortion = (tex2D(uDistortTex, frac(distortUV)).rg - 0.5) * 2.0 * uDistortStrength;
    float2 distortedUV = uv + distortion;
    
    // Phase timing
    float time = uTime * uFlowSpeed;
    float phaseA = frac(time);
    float phaseB = frac(time + 0.5);
    
    // Phase A
    float2 polarA = polar_coordinates(distortedUV, CENTER, uZoom, uRepeat);
    polarA.x -= phaseA;
    polarA.y += uTime * uSpinSpeed;
    polarA = glslmod2(polarA, float2(1.0, 1.0));
    float4 sampleA = tex2D(uImage0, polarA);
    
    // Phase B
    float2 polarB = polar_coordinates(distortedUV, CENTER, uZoom, uRepeat);
    polarB.x -= phaseB;
    polarB.y += uTime * uSpinSpeed;
    polarB = glslmod2(polarB, float2(1.0, 1.0));
    float4 sampleB = tex2D(uImage0, polarB);
    
    // Triangle wave blending
    float weightA = 1.0 - abs(1.0 - 2.0 * phaseA);
    float weightB = 1.0 - abs(1.0 - 2.0 * phaseB);
    float totalWeight = weightA + weightB;
    
    float4 blended = (sampleA * weightA + sampleB * weightB) / totalWeight;
    
    // Gradient mapping
    float4 gradientColor = tex2D(uGradientTex, float2(blended.r, 0.5));
    
    // Vignette
    float dist = length(uv - CENTER);
    float vignette = 1.0 - smoothstep(uVignetteSize, uVignetteSize + uVignetteBlend, dist);
    float centerFade = smoothstep(0.0, 0.08, dist);
    
    // Output
    float3 finalColor = gradientColor.rgb * uTint * blended.r * uColorIntensity;
    float finalAlpha = vignette * centerFade * blended.a * uAlpha;
    
    return float4(finalColor, finalAlpha) * sampleColor;
}

// Technique definitions
technique BasicRadialScroll
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PSRadialScroll();
    }
}

technique SeamlessRadialScroll
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PSDualPhaseRadial();
    }
}
```

### 10.2 Complete C# Manager Class

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using ReLogic.Content;
using System;

namespace MagnumOpus.Common.Systems.VFX
{
    /// <summary>
    /// Complete radial scroll effect manager with all features.
    /// Supports multiple rendering modes and seamless animation.
    /// </summary>
    public class RadialScrollManager : IDisposable
    {
        #region Fields
        
        private Effect _effect;
        private Texture2D _causticTexture;
        private Texture2D _gradientTexture;
        private Texture2D _distortTexture;
        private bool _isLoaded;
        
        #endregion
        
        #region Properties
        
        /// <summary>Radial flow speed. Positive = outward, negative = inward.</summary>
        public float FlowSpeed { get; set; } = 0.3f;
        
        /// <summary>Angular rotation speed. Positive = counter-clockwise.</summary>
        public float SpinSpeed { get; set; } = 0.1f;
        
        /// <summary>Texture zoom level. Higher = smaller pattern.</summary>
        public float Zoom { get; set; } = 1.0f;
        
        /// <summary>Number of pattern repetitions around the circle.</summary>
        public float Repeat { get; set; } = 1.0f;
        
        /// <summary>Distortion strength. 0 = none, 0.1 = subtle, 0.3 = heavy.</summary>
        public float DistortStrength { get; set; } = 0.05f;
        
        /// <summary>Color intensity multiplier.</summary>
        public float ColorIntensity { get; set; } = 1.5f;
        
        /// <summary>Vignette size (0.2-0.6 typical).</summary>
        public float VignetteSize { get; set; } = 0.3f;
        
        /// <summary>Vignette edge softness.</summary>
        public float VignetteBlend { get; set; } = 0.2f;
        
        /// <summary>Color tint applied to effect.</summary>
        public Color Tint { get; set; } = Color.White;
        
        /// <summary>Overall opacity.</summary>
        public float Alpha { get; set; } = 1.0f;
        
        /// <summary>Use seamless dual-phase animation.</summary>
        public bool UseSeamlessMode { get; set; } = true;
        
        #endregion
        
        #region Initialization
        
        public void Load(Mod mod)
        {
            if (_isLoaded) return;
            
            try
            {
                // Load shader
                _effect = ModContent.Request<Effect>(
                    $"{mod.Name}/Assets/Shaders/AdvancedRadialScroll",
                    AssetRequestMode.ImmediateLoad
                ).Value;
                
                // Load textures with fallbacks
                _causticTexture = SafeLoadTexture(mod, "Assets/VFX/Caustic", GenerateFallbackCaustic);
                _gradientTexture = SafeLoadTexture(mod, "Assets/VFX/FireGradient", GenerateFallbackGradient);
                _distortTexture = SafeLoadTexture(mod, "Assets/VFX/PerlinNoise", GenerateFallbackNoise);
                
                _isLoaded = true;
            }
            catch (Exception ex)
            {
                mod.Logger.Error($"Failed to load RadialScrollManager: {ex.Message}");
            }
        }
        
        private Texture2D SafeLoadTexture(Mod mod, string path, Func<Texture2D> fallbackGenerator)
        {
            try
            {
                return ModContent.Request<Texture2D>($"{mod.Name}/{path}", AssetRequestMode.ImmediateLoad).Value;
            }
            catch
            {
                return fallbackGenerator();
            }
        }
        
        private Texture2D GenerateFallbackCaustic()
        {
            int size = 128;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Random rand = new Random(12345);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float value = (float)(Math.Sin(x * 0.1) * Math.Cos(y * 0.1) * 0.5 + 0.5);
                    value += (float)rand.NextDouble() * 0.2f;
                    byte b = (byte)(MathHelper.Clamp(value, 0, 1) * 255);
                    data[y * size + x] = new Color(b, b, b, 255);
                }
            }
            
            tex.SetData(data);
            return tex;
        }
        
        private Texture2D GenerateFallbackGradient()
        {
            int width = 256;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, width, 1);
            Color[] data = new Color[width];
            
            for (int x = 0; x < width; x++)
            {
                float t = (float)x / (width - 1);
                data[x] = Color.Lerp(Color.DarkRed, Color.Orange, t);
            }
            
            tex.SetData(data);
            return tex;
        }
        
        private Texture2D GenerateFallbackNoise()
        {
            int size = 64;
            Texture2D tex = new Texture2D(Main.graphics.GraphicsDevice, size, size);
            Color[] data = new Color[size * size];
            
            Random rand = new Random(54321);
            for (int i = 0; i < data.Length; i++)
            {
                byte v = (byte)rand.Next(256);
                data[i] = new Color(v, v, v, 255);
            }
            
            tex.SetData(data);
            return tex;
        }
        
        #endregion
        
        #region Rendering
        
        public void ApplyEffect()
        {
            if (!_isLoaded || _effect == null) return;
            
            // Time
            _effect.Parameters["uTime"]?.SetValue(Main.GlobalTimeWrappedHourly);
            
            // Animation
            _effect.Parameters["uFlowSpeed"]?.SetValue(FlowSpeed);
            _effect.Parameters["uSpinSpeed"]?.SetValue(SpinSpeed);
            _effect.Parameters["uZoom"]?.SetValue(Zoom);
            _effect.Parameters["uRepeat"]?.SetValue(Repeat);
            _effect.Parameters["uDistortStrength"]?.SetValue(DistortStrength);
            
            // Visual
            _effect.Parameters["uColorIntensity"]?.SetValue(ColorIntensity);
            _effect.Parameters["uVignetteSize"]?.SetValue(VignetteSize);
            _effect.Parameters["uVignetteBlend"]?.SetValue(VignetteBlend);
            _effect.Parameters["uTint"]?.SetValue(Tint.ToVector3());
            _effect.Parameters["uAlpha"]?.SetValue(Alpha);
            
            // Textures
            GraphicsDevice device = Main.graphics.GraphicsDevice;
            device.Textures[1] = _gradientTexture;
            device.Textures[2] = _distortTexture;
            device.SamplerStates[1] = SamplerState.LinearWrap;
            device.SamplerStates[2] = SamplerState.LinearWrap;
            
            // Select technique
            string technique = UseSeamlessMode ? "SeamlessRadialScroll" : "BasicRadialScroll";
            _effect.CurrentTechnique = _effect.Techniques[technique];
            _effect.CurrentTechnique.Passes[0].Apply();
        }
        
        public void Draw(SpriteBatch spriteBatch, Vector2 worldPosition, float scale, Color color)
        {
            if (!_isLoaded) return;
            
            // End current batch
            spriteBatch.End();
            
            // Begin with shader
            spriteBatch.Begin(
                SpriteSortMode.Immediate,
                BlendState.Additive,
                SamplerState.LinearWrap,
                DepthStencilState.None,
                RasterizerState.CullNone,
                _effect,
                Main.GameViewMatrix.TransformationMatrix
            );
            
            ApplyEffect();
            
            // Draw
            Vector2 screenPos = worldPosition - Main.screenPosition;
            Vector2 origin = _causticTexture.Size() * 0.5f;
            
            spriteBatch.Draw(
                _causticTexture,
                screenPos,
                null,
                color,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );
            
            // Restore batch
            spriteBatch.End();
            spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                Main.DefaultSamplerState,
                DepthStencilState.None,
                Main.Rasterizer,
                null,
                Main.GameViewMatrix.TransformationMatrix
            );
        }
        
        #endregion
        
        #region Disposal
        
        public void Dispose()
        {
            // Textures managed by content system, don't dispose
            _effect = null;
            _causticTexture = null;
            _gradientTexture = null;
            _distortTexture = null;
            _isLoaded = false;
        }
        
        #endregion
    }
}
```

---

## 📖 Related Documentation

| Document | Purpose |
|----------|---------|
| **[HLSL_GRAPHICS_DEEP_DIVE.md](HLSL_GRAPHICS_DEEP_DIVE.md)** | Complete HLSL reference, noise functions, SDFs |
| **[VFX_MASTERY_RESEARCH_COMPLETE.md](VFX_MASTERY_RESEARCH_COMPLETE.md)** | MonoGame API, BlendStates, bloom stacking |
| **[VFX_CORE_CONCEPTS_PART2.md](VFX_CORE_CONCEPTS_PART2.md)** | Bezier curves, particle systems |
| **[HLSLLibrary.fxh](../ShaderSource/HLSLLibrary.fxh)** | Reusable utility functions |

---

## 🔗 External Resources

| Resource | URL | Description |
|----------|-----|-------------|
| Catlike Coding - Texture Distortion | https://catlikecoding.com/unity/tutorials/flow/texture-distortion/ | Dual-phase blending, flow maps |
| Godot Shaders - Polar Coordinates | https://godotshaders.com/snippet/polar-coordinates/ | Concise polar transform snippet |
| Inigo Quilez - 2D SDFs | https://iquilezles.org/articles/distfunctions2d/ | SDF primitives and operations |
| The Book of Shaders - Polar | https://thebookofshaders.com/06/ | Color theory in polar space |
| Simon Schreibt - Render Hell | https://simonschreibt.de/gat/renderhell/ | GPU architecture fundamentals |

---

*Last Updated: Expert-Level Radial Scroll Documentation Complete*
