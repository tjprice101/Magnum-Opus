# FargosSoulsDLC VFX System Overview

> **This document provides a comprehensive overview of the visual effects systems used in FargosSoulsDLC, extracted for reference in MagnumOpus development.**

---

## Table of Contents

1. [Core Architecture](#core-architecture)
2. [Key Systems](#key-systems)
3. [Document Index](#document-index)
4. [Quick Reference Patterns](#quick-reference-patterns)

---

## Core Architecture

FargosSoulsDLC uses a sophisticated, layered VFX pipeline built on top of tModLoader and the **Luminance** library:

```
┌─────────────────────────────────────────────────────────────┐
│                    RENDERING PIPELINE                        │
├─────────────────────────────────────────────────────────────┤
│  SpriteBatch Layer (Standard + Additive blending)           │
│      ↓                                                       │
│  Shader Layer (ManagedShader / ShaderManager)               │
│      ↓                                                       │
│  Primitive Layer (PrimitiveRenderer.RenderTrail)            │
│      ↓                                                       │
│  Pixelation Layer (IPixelatedPrimitiveRenderer)             │
│      ↓                                                       │
│  Post-Processing Layer (Screen filters, metaballs)          │
└─────────────────────────────────────────────────────────────┘
```

### Key Dependencies

- **Luminance Library**: Provides `ManagedShader`, `ShaderManager`, `PrimitiveRenderer`, `MiscTexturesRegistry`, `NoiseTexturesRegistry`
- **Calamity Mod**: Source textures from `CalamityMod/ExtraTextures/GreyscaleGradients/`
- **tModLoader**: Base framework for `ModProjectile`, `ModNPC`, `SpriteBatch`

---

## Key Systems

### 1. Shader Management System
```csharp
// Getting a shader
ManagedShader shader = ShaderManager.GetShader("FargowiltasCrossmod.ShaderName");

// Setting parameters
shader.TrySetParameter("parameterName", value);

// Setting textures (slot 1, 2, 3...)
shader.SetTexture(texture, 1, SamplerState.LinearWrap);

// Applying the shader
shader.Apply();
```

### 2. Primitive Trail Rendering
```csharp
// Implement IPixelatedPrimitiveRenderer interface
public class MyProjectile : ModProjectile, IPixelatedPrimitiveRenderer
{
    public void RenderPixelatedPrimitives(SpriteBatch spriteBatch)
    {
        ManagedShader shader = ShaderManager.GetShader("...");
        PrimitiveSettings settings = new(
            WidthFunction,      // float completionRatio => float width
            ColorFunction,      // float completionRatio => Color
            _ => offset,        // Position offset
            Pixelate: true,     // Enable pixelation
            Shader: shader      // Apply shader
        );
        PrimitiveRenderer.RenderTrail(positions, settings, segmentCount);
    }
    
    public float WidthFunction(float completionRatio) => 30f * (1f - completionRatio);
    public Color ColorFunction(float completionRatio) => Color.White * (1f - completionRatio);
}
```

### 3. Bloom Rendering Pattern
```csharp
// Standard bloom drawing
Texture2D bloom = MiscTexturesRegistry.BloomCircleSmall.Value;
Main.spriteBatch.Draw(bloom, position, null, 
    color with { A = 0 } * opacity,  // Remove alpha for additive
    rotation, bloom.Size() * 0.5f, scale, 0, 0f);
```

### 4. SpriteBatch State Management
```csharp
// Prepare for shader use
Main.spriteBatch.PrepareForShaders();  // or PrepareForShaders(BlendState.Additive)

// ... shader operations ...

// Reset to default state
Main.spriteBatch.ResetToDefault();
```

---

## Document Index

| Document | Contents |
|----------|----------|
| [01_Primitive_Trail_Rendering.md](01_Primitive_Trail_Rendering.md) | Trail rendering system, width/color functions, PrimitiveSettings |
| [02_Bloom_And_Glow_Effects.md](02_Bloom_And_Glow_Effects.md) | Bloom circles, flares, shine textures, additive blending |
| [03_HLSL_Shader_Reference.md](03_HLSL_Shader_Reference.md) | All HLSL shaders organized by type with code samples |
| [04_ExoMechs_VFX_Analysis.md](04_ExoMechs_VFX_Analysis.md) | Ares, Apollo, Artemis, Hades/Thanatos visual effects |
| [05_Particle_Systems.md](05_Particle_Systems.md) | Particle classes, bloom pixels, fire particles, metaballs |
| [06_Old_Duke_VFX_Analysis.md](06_Old_Duke_VFX_Analysis.md) | Fire particle system, bile metaballs, nuclear effects |
| [07_Texture_Registries.md](07_Texture_Registries.md) | MiscTexturesRegistry, NoiseTexturesRegistry reference |
| [08_Color_And_Gradient_Techniques.md](08_Color_And_Gradient_Techniques.md) | Palette lerping, multicolor gradients, hue shifting |

---

## Quick Reference Patterns

### The QuadraticBump Function (Used Everywhere in HLSL)
```hlsl
float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}
// Creates a smooth 0→1→0 curve, perfect for:
// - Trail fadeouts (edges to center)
// - Pulse effects
// - Smooth falloffs
```

### Standard Laser/Trail Width Function
```csharp
public float LaserWidthFunction(float completionRatio)
{
    // Thin at start, thick in middle, thin at end
    float baseWidth = 30f;
    float bump = MathF.Sin(completionRatio * MathHelper.Pi);
    return baseWidth * bump * Projectile.scale;
}
```

### Standard Color Gradient Function
```csharp
public Color LaserColorFunction(float completionRatio)
{
    Color baseColor = new Color(1f, 0.5f, 0.2f);  // Orange
    float fadeOut = 1f - completionRatio;  // Fade along length
    return baseColor * fadeOut * Projectile.Opacity;
}
```

### Bloom Layer Stack
```csharp
// Layer 1: Outer soft glow (largest, most transparent)
Main.spriteBatch.Draw(bloom, pos, null, color * 0.3f, 0f, origin, scale * 2f, 0, 0f);

// Layer 2: Middle glow
Main.spriteBatch.Draw(bloom, pos, null, color * 0.5f, 0f, origin, scale * 1.4f, 0, 0f);

// Layer 3: Inner bright core (smallest, most opaque)
Main.spriteBatch.Draw(bloom, pos, null, Color.White * 0.8f, 0f, origin, scale * 0.6f, 0, 0f);
```

### Pixelation Pattern in Shaders
```hlsl
// Standard pixelation
float2 pixelationFactor = textureSize0 * 0.7;
coords = floor(coords * pixelationFactor) / pixelationFactor;
```

---

## Adaptation Notes for MagnumOpus

1. **Replace Luminance dependencies** with MagnumOpus equivalents or implement similar systems
2. **Shader paths** will need to change from `FargowiltasCrossmod.*` to `MagnumOpus.*`
3. **Texture registries** - create MagnumOpus versions of MiscTexturesRegistry/NoiseTexturesRegistry
4. **Color palettes** - adapt to MagnumOpus theme colors (Fate, Eroica, etc.)
5. **Consider pixelation settings** - Fargo uses heavy pixelation; MagnumOpus may prefer smoother

---

*Document extracted from FargosSoulsDLC GitHub repository for MagnumOpus VFX development reference.*
