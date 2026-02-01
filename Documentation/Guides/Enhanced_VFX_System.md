# MagnumOpus Enhanced VFX System

## Overview

The MagnumOpus VFX system has been overhauled to use **FargosSoulsDLC-style rendering patterns** for stunning visual effects. This document explains the key concepts and how to use the new system.

---

## ⭐⭐⭐ THE CARDINAL RULE: EVERY WEAPON IS UNIQUE ⭐⭐⭐

> **THIS IS THE ABSOLUTE #1 RULE. NO EXCEPTIONS.**

### Every Weapon Has Its Own Visual Identity

If a theme has 3 swords, those 3 swords must have **COMPLETELY DIFFERENT** visual effects:

| Sword | On-Swing | Trail | Impact | Special |
|-------|----------|-------|--------|---------|
| Sword A | Fires spiraling orbs | Music note constellation trail | Harmonic shockwave with note burst | Orbs connect with light beams |
| Sword B | Creates burning afterimages | Ember + smoke wisp trail | Rising flame pillars | Charge attack summons phantom blade |
| Sword C | Spawns homing feathers | Prismatic rainbow arc | Crystalline shard explosion | Every 4th hit creates gravity well |

**Same colors. Completely different effects. This is MANDATORY.**

### The Forbidden Pattern

```csharp
// ❌ FORBIDDEN - Generic, boring, lazy
public override void OnHitNPC(...)
{
    CustomParticles.GenericFlare(target.Center, color, 0.5f, 15);
    CustomParticles.HaloRing(target.Center, color, 0.3f, 12);
}
// This is a DISGRACE. Never write code like this.
```

---

## ⭐ CRITICAL: Particle Asset Discovery - ALWAYS DO THIS FIRST

> **Before creating ANY weapon effect, you MUST explore available particle textures!**

### Mandatory Steps:
1. **Run `list_dir` on `Assets/Particles/`** to see all 80+ available textures
2. **Mix and match** from different categories (flares, sparkles, trails, music notes, glyphs)
3. **Use different variants** - most particles have 2-15 numbered variants
4. **Be creative** - unique combinations create unique weapons
5. **Music notes need scale 0.6f+** to be visible!

### Available Particle Categories:
- **EnergyFlare (7 variants)** - Intense bursts
- **SoftGlow (3 variants)** - Ambient glows  
- **GlowingHalo (5 variants)** - Ring effects
- **StarBurst (2 variants)** - Radial explosions
- **MusicNote (6 variants)** - Musical notes (**scale 0.6f+ required!**)
- **MagicSparklField (12 variants)** - Magic sparkle clusters
- **PrismaticSparkle (15 variants)** - Rainbow sparkles
- **ParticleTrail (4 variants)** - Movement trails
- **SwordArc (9 variants)** - Melee swing arcs
- **SwanFeather (10 variants)** - Feathers
- **EnigmaEye (8 variants)** - Watching eyes
- **Glyphs (12 variants)** - Arcane symbols

### Vanilla Dust Types (Combine with Custom Particles)

```csharp
// ALWAYS combine custom particles with vanilla dust for visual density
DustID.MagicMirror      // Magical shimmer
DustID.Enchanted_Gold   // Golden sparkles
DustID.Enchanted_Pink   // Pink magical dust
DustID.PurpleTorch      // Purple flames
DustID.Electric         // Electric sparks
DustID.Frost            // Ice crystals
DustID.GemAmethyst      // Purple gems
DustID.GemSapphire      // Blue gems
DustID.Pixie            // Fairy dust
DustID.RainbowMk2       // Rainbow particles
```

> **See `Assets/Particles/README.md` for complete catalog with scale recommendations.**

---

## 🎵 MUSIC NOTES MUST BE VISIBLE

> **THIS IS A MUSIC MOD. MUSIC NOTES ARE CURRENTLY INVISIBLE. THAT'S UNACCEPTABLE.**

### The Problem
Music notes spawned at scales 0.25f-0.4f are **completely invisible**. This defeats the purpose of a music-themed mod.

### The Solution

**EVERY music note MUST:**
- Use scale **0.7f - 1.2f** (MINIMUM 0.6f)
- Have **multi-layer bloom** (3-4 draws at increasing scales)
- Include **shimmer animation** (scale pulses)
- Be **accompanied by sparkles** for visibility

```csharp
// ✅ CORRECT - Visible, glowing, shimmering music notes
void SpawnGlowingMusicNote(Vector2 position, Vector2 velocity, Color baseColor)
{
    float scale = Main.rand.NextFloat(0.75f, 1.0f);
    int variant = Main.rand.Next(1, 7); // Use ALL 6 variants
    
    // Shimmer
    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
    scale *= shimmer;
    
    // Bloom layers
    for (int bloom = 0; bloom < 3; bloom++)
    {
        float bloomScale = scale * (1f + bloom * 0.4f);
        float bloomAlpha = 0.5f / (bloom + 1);
        // Draw bloom particle at bloomScale with bloomAlpha
    }
    
    // Sparkle companions
    for (int i = 0; i < 2; i++)
    {
        Vector2 sparkleOffset = Main.rand.NextVector2Circular(8f, 8f);
        CustomParticles.PrismaticSparkle(position + sparkleOffset, baseColor, 0.35f, Main.rand.Next(1, 16));
    }
    
    // The note
    ThemedParticles.MusicNote(position, velocity, baseColor, scale, 35, variant);
}
```

---

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
