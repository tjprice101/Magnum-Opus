# MagnumOpus Enhanced VFX System

## Overview

The MagnumOpus VFX system has been overhauled to use **FargosSoulsDLC-style rendering patterns** for stunning visual effects. This document explains the key concepts and how to use the new system.

## Key Patterns Implemented

### 1. The `{ A = 0 }` Alpha Removal Pattern

**This is the most important pattern.** When using additive blending, removing the alpha channel prevents the "darkening" effect that occurs with standard blending.

```csharp
// CRITICAL: Remove alpha for proper additive blending
Color bloomColor = baseColor with { A = 0 };

// Then draw with the bloom color
spriteBatch.Draw(texture, position, null, bloomColor * opacity, ...);
```

### 2. Multi-Layer Bloom Stacking

Instead of drawing a single glow sprite, draw 4 layers at different scales and opacities. This creates a professional, soft glow effect.

```csharp
// Bloom layer configuration (from Fargos)
float[] scales = { 2.0f, 1.4f, 0.9f, 0.4f };      // Outer to inner
float[] opacities = { 0.3f, 0.5f, 0.7f, 0.85f };  // Dim to bright

for (int i = 0; i < 4; i++)
{
    float layerScale = baseScale * scales[i];
    float layerAlpha = baseAlpha * opacities[i];
    spriteBatch.Draw(tex, pos, null, bloomColor * layerAlpha, 0, origin, layerScale, ...);
}
```

### 3. Theme Palette Gradients

All themes have defined color palettes for smooth gradient transitions:

| Theme | Primary → Secondary |
|-------|-------------------|
| LaCampanella | Black → Orange → Gold |
| Eroica | Scarlet → Gold |
| MoonlightSonata | Dark Purple → Ice Blue |
| SwanLake | White → Black (rainbow shimmer) |
| EnigmaVariations | Black → Purple → Green |
| Fate | Black → Pink → Red (cosmic white highlights) |

```csharp
// Get gradient color for any theme
Color gradientColor = MagnumThemePalettes.GetThemeColor("Eroica", progress);
```

---

## Using the New System

### Quick Start - Enhanced Effects

For the simplest upgrade path, use the `UnifiedVFXBloom` class:

```csharp
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.VFX;

// Enhanced impact effects (use instead of UnifiedVFX methods)
UnifiedVFXBloom.Eroica.ImpactEnhanced(position, 1.5f);
UnifiedVFXBloom.Fate.CosmicBurst(position);
UnifiedVFXBloom.MoonlightSonata.BloomBurstEnhanced(position);

// Generic bloom effects
UnifiedVFXBloom.Generic.BloomFlare(position, color, 0.5f, 20, 4, 1f);
UnifiedVFXBloom.Generic.BloomBurst(position, primaryColor, secondaryColor, 8, 4f, 0.3f, 25);
```

### Enhanced Particle System

For custom particles with full bloom control:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Create enhanced particle with bloom
var particle = EnhancedParticlePool.GetParticle()
    .Setup(texture, position, velocity, color, scale, lifetime)
    .WithBloom(4, 1.0f)           // 4 layers, full intensity
    .WithShineFlare(0.5f)          // Add star sparkle overlay
    .WithPulse(0.1f, 0.15f)        // Pulsing scale animation
    .WithGradient(secondaryColor)  // Fade to secondary color
    .WithTheme("Fate");            // Use Fate palette automatically

EnhancedParticlePool.SpawnParticle(particle);
```

### Enhanced Themed Particles

For theme-specific effects with full bloom rendering:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Moonlight effects
EnhancedThemedParticles.MoonlightBloomBurstEnhanced(position, intensity);
EnhancedThemedParticles.MoonlightImpactEnhanced(position, intensity);
EnhancedThemedParticles.MoonlightMusicNotesEnhanced(position, count, spread);

// Eroica effects  
EnhancedThemedParticles.EroicaBloomBurstEnhanced(position, intensity);
EnhancedThemedParticles.EroicaImpactEnhanced(position, intensity);
EnhancedThemedParticles.SakuraPetalsEnhanced(position, count, spread);

// La Campanella effects
EnhancedThemedParticles.LaCampanellaBloomBurstEnhanced(position, intensity);
EnhancedThemedParticles.BellChimeEnhanced(position, intensity);

// Fate effects (ENDGAME CELESTIAL)
EnhancedThemedParticles.FateBloomBurstEnhanced(position, intensity);
EnhancedThemedParticles.FateImpactEnhanced(position, intensity);
EnhancedThemedParticles.FateGlyphBurstEnhanced(position, count, speed);
EnhancedThemedParticles.FateStarTrailEnhanced(position, velocity);

// Swan Lake effects
EnhancedThemedParticles.SwanLakeBloomBurstEnhanced(position, intensity);
EnhancedThemedParticles.SwanFeatherBurstEnhanced(position, count, intensity);

// Enigma effects
EnhancedThemedParticles.EnigmaBloomBurstEnhanced(position, intensity);
```

---

## VFX Utilities

### Color Extensions

```csharp
using MagnumOpus.Common.Systems.VFX;

// Remove alpha for additive blending
Color bloomColor = baseColor.WithoutAlpha();

// Palette lerping
Color gradientColor = VFXUtilities.PaletteLerp(colorArray, progress);
```

### Math Utilities

```csharp
// QuadraticBump: 0 → 1 → 0 curve (peaks at 0.5)
float bump = VFXUtilities.QuadraticBump(progress);

// InverseLerp: Get progress between two values
float t = VFXUtilities.InverseLerp(min, max, current);

// Convert01To010: 0→1→0 over 0→1 input
float value = VFXUtilities.Convert01To010(progress);
```

### Bloom Renderer

```csharp
using MagnumOpus.Common.Systems.VFX;

// Draw multi-layer bloom stack
BloomRenderer.DrawBloomStack(spriteBatch, texture, position, color, scale, layers, intensity, rotation);

// Themed presets
BloomRenderer.DrawLaCampanellaBloom(spriteBatch, texture, position, scale, lifetime, maxLifetime);
BloomRenderer.DrawEroicaBloom(spriteBatch, texture, position, scale, lifetime, maxLifetime);
BloomRenderer.DrawFateBloom(spriteBatch, texture, position, scale, lifetime, maxLifetime);
```

---

## File Reference

### New VFX System Files (Common/Systems/VFX/)

| File | Purpose |
|------|---------|
| `MagnumTextureRegistry.cs` | Centralized VFX texture management with fallbacks |
| `VFXUtilities.cs` | Math utilities, color extensions, SpriteBatch helpers |
| `MagnumThemePalettes.cs` | Theme color arrays for all 8 themes |
| `BloomParticles.cs` | New particle classes with built-in bloom |
| `BloomRenderer.cs` | High-level bloom drawing utilities |
| `MagnumVFXDrawLayer.cs` | Render pipeline integration |
| `EnhancedTrailRenderer.cs` | Fargos-style primitive trail rendering |
| `EnhancedParticle.cs` | Enhanced particle class with bloom support |
| `EnhancedThemedParticles.cs` | Theme-specific enhanced particle effects |
| `UnifiedVFXBloom.cs` | Easy API for enhanced bloom effects |

### Updated Existing Files

| File | Changes |
|------|---------|
| `MagnumParticleHandler.cs` | DrawParticle uses `{ A = 0 }` pattern |
| `CommonParticles.cs` | BloomParticle, SparkleParticle, GenericGlowParticle, BloomRingParticle all use multi-layer bloom |

---

## Migration Guide

### Before (Old Style)
```csharp
ThemedParticles.EroicaImpact(position, scale);
CustomParticles.GenericFlare(position, color, scale, lifetime);
```

### After (Enhanced Style)
```csharp
// Option 1: Use UnifiedVFXBloom for drop-in replacement
UnifiedVFXBloom.Eroica.ImpactEnhanced(position, scale);

// Option 2: Use EnhancedThemedParticles directly
EnhancedThemedParticles.EroicaImpactEnhanced(position, scale);

// Option 3: Use EnhancedParticles for custom control
EnhancedParticles.BloomFlare(position, color, scale, lifetime, 4, 1.0f);
```

### Existing Particles Auto-Enhanced

All existing particles using these classes will automatically benefit from the new bloom rendering:
- `BloomParticle` ✓
- `BloomRingParticle` ✓
- `SparkleParticle` ✓
- `GenericGlowParticle` ✓

---

## Best Practices

1. **Always use `{ A = 0 }` for additive blending** - This is the key Fargos pattern
2. **Use 4 bloom layers for important effects** - Fewer for performance-critical areas
3. **Match theme palettes** - Use `MagnumThemePalettes` for consistent gradients
4. **Layer effects** - Combine bloom burst + sparks + halos for maximum impact
5. **Use EnhancedParticlePool** - It handles pooling and lifecycle automatically

---

## References

- [FargosSoulsDLC VFX Documentation](../Documentation/Custom%20Shaders%20and%20Shading/)
- [Copilot Instructions](../.github/copilot-instructions.md)
