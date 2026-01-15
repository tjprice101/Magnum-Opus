# MagnumOpus Visual Effects & Particle System Guide

This document provides a comprehensive overview of the VFX and particle system used in MagnumOpus.
Use this guide when creating new content or enhancing existing content with visual effects.

---

## Table of Contents

1. [Overview](#overview)
2. [Score Color Palettes](#score-color-palettes)
3. [ThemedParticles API](#themedparticles-api)
4. [Core Particle System](#core-particle-system)
5. [MagnumVFX Utilities](#magnumvfx-utilities)
6. [Implementation Examples](#implementation-examples)
7. [Best Practices](#best-practices)

---

## Overview

The MagnumOpus VFX system is inspired by the Calamity Mod's approach to visual effects. It consists of several components:

| Component | File | Purpose |
|-----------|------|---------|
| **ThemedParticles** | `Common/Systems/ThemedParticles.cs` | High-level themed particle effects for Moonlight & Eroica |
| **Particle System** | `Common/Systems/Particles/` | Core custom particle classes with GPU-batched rendering |
| **MagnumVFX** | `Common/Systems/MagnumVFX.cs` | General VFX utilities, blend modes, lightning effects |
| **ParticleTextureGenerator** | `Common/Systems/Particles/ParticleTextureGenerator.cs` | Procedural texture generation (no PNG files needed) |
| **PrimitiveTrailRenderer** | `Common/Systems/PrimitiveTrailRenderer.cs` | GPU-rendered smooth trails |

### Architecture

```
ThemedParticles (Easy-to-use API)
        ↓
    Particle System (BloomParticle, SparkleParticle, etc.)
        ↓
MagnumParticleHandler (Batched rendering)
        ↓
ParticleTextureGenerator (Runtime texture generation)
```

---

## Score Color Palettes

### 🌙 Moonlight Sonata

**Theme**: Ethereal, mysterious, lunar, melancholic

| Color Name | Hex Code | RGB | Usage |
|------------|----------|-----|-------|
| **Dark Purple** (Primary) | `#4B0082` | `75, 0, 130` | Core energy, main effects |
| **Medium Purple** | `#8A2BE2` | `138, 43, 226` | Transition color |
| **Light Purple** | `#B496FF` | `180, 150, 255` | Outer glow, accents |
| **Light Blue** | `#87CEFA` | `135, 206, 250` | Secondary accent |
| **Ice Blue** | `#C8E6FF` | `200, 230, 255` | Sparkles, highlights |
| **Silver** | `#DCDCEB` | `220, 220, 235` | Metallic accents |
| **White** | `#F0EBFF` | `240, 235, 255` | Core highlights |

**Visual Style**:
- Gradient from dark purple → light blue
- Silver/white sparkle accents
- Flowing, ethereal movements
- Soft bloom effects
- Moon-like luminescence

### 🔥 Eroica (Symphony of Valor)

**Theme**: Heroic, triumphant, fiery, passionate

| Color Name | Hex Code | RGB | Usage |
|------------|----------|-----|-------|
| **Scarlet** (Primary) | `#8B0000` | `139, 0, 0` | Core fire, main effects |
| **Crimson** | `#DC3232` | `220, 50, 50` | Bright flames |
| **Flame Orange** | `#FF6432` | `255, 100, 50` | Fire accents |
| **Gold** (Secondary) | `#FFD700` | `255, 215, 0` | Heroic accents, sparkles |
| **Amber** | `#FFBF64` | `255, 191, 100` | Warm highlights |
| **Black** | `#1E1419` | `30, 20, 25` | Smoke, dark accents |
| **Sakura Pink** | `#FF96B4` | `255, 150, 180` | Sakura petal effects |

**Visual Style**:
- Deep scarlet with gold accents
- Rising ember/fire particles
- Black smoke wisps
- Sharp, energetic sparks
- Sakura petal trails (for sakura-themed items)

---

## ThemedParticles API

`ThemedParticles` provides the easiest way to add themed effects. All methods are static.

### Required Using Statement

```csharp
using MagnumOpus.Common.Systems;
```

### Moonlight Sonata Methods

```csharp
// === BLOOM BURSTS ===
// Creates a burst of glowing bloom particles
ThemedParticles.MoonlightBloomBurst(Vector2 position, float intensity = 1f);

// === SPARKLES ===
// Spawns twinkling sparkle particles
ThemedParticles.MoonlightSparkles(Vector2 position, int count = 8, float spreadRadius = 30f);

// === SHOCKWAVES ===
// Creates expanding ring effect
ThemedParticles.MoonlightShockwave(Vector2 position, float scale = 1f);

// === DIRECTIONAL SPARKS ===
// Sparks that follow a direction (good for slashes/hits)
ThemedParticles.MoonlightSparks(Vector2 position, Vector2 direction, int count = 6, float speed = 5f);

// === COMBINED IMPACT ===
// Full impact effect (bloom + shockwave + sparks + sparkles)
ThemedParticles.MoonlightImpact(Vector2 position, float intensity = 1f);

// === TRAIL PARTICLES ===
// Use in AI() for projectile trails
ThemedParticles.MoonlightTrail(Vector2 position, Vector2 velocity);

// === AMBIENT AURA ===
// Use in UpdateAccessory/AI for persistent aura
ThemedParticles.MoonlightAura(Vector2 center, float radius = 40f);
```

### Eroica Methods

```csharp
// === BLOOM BURSTS ===
ThemedParticles.EroicaBloomBurst(Vector2 position, float intensity = 1f);

// === SPARKLES ===
ThemedParticles.EroicaSparkles(Vector2 position, int count = 8, float spreadRadius = 30f);

// === SHOCKWAVES ===
ThemedParticles.EroicaShockwave(Vector2 position, float scale = 1f);

// === DIRECTIONAL SPARKS ===
ThemedParticles.EroicaSparks(Vector2 position, Vector2 direction, int count = 6, float speed = 6f);

// === COMBINED IMPACT ===
ThemedParticles.EroicaImpact(Vector2 position, float intensity = 1f);

// === TRAIL PARTICLES ===
ThemedParticles.EroicaTrail(Vector2 position, Vector2 velocity);

// === AMBIENT AURA ===
ThemedParticles.EroicaAura(Vector2 center, float radius = 40f);
```

### Special Effects

```csharp
// === SAKURA PETALS ===
// Pink floating petals (Eroica sakura items)
ThemedParticles.SakuraPetals(Vector2 center, int count = 6, float radius = 35f);

// === DODGE TRAIL ===
// Trail effect for dodge abilities
ThemedParticles.DodgeTrail(Vector2 position, Vector2 velocity, bool isMoonlight = true);

// === TELEPORT BURST ===
// Flash effect for teleportation
ThemedParticles.TeleportBurst(Vector2 position, bool isMoonlight = true);
```

### 🎵 Musical Particle Effects (NEW!)

Since MagnumOpus is themed around classical music, special musical particle effects are available!

#### Moonlight Musical Effects

```csharp
// === MUSIC NOTES ===
// Floating purple/silver notes that drift upward with gentle wobble
ThemedParticles.MoonlightMusicNotes(Vector2 position, int count = 5, float spreadRadius = 30f);

// === CLEF SYMBOL ===
// Dramatic treble or bass clef for major effects
ThemedParticles.MoonlightClef(Vector2 position, bool useTrebleClef = true, float scale = 1f);

// === MUSIC STAFF ===
// Glowing 5-line staff for casting/charging effects
ThemedParticles.MoonlightMusicStaff(Vector2 position, float scale = 1f);

// === ACCIDENTALS ===
// Sharp (#) and flat (b) symbols
ThemedParticles.MoonlightAccidentals(Vector2 position, int count = 3, float spreadRadius = 20f);

// === COMBINED MUSICAL IMPACT ===
// Full musical burst (notes + sparkles + accidentals + optional clef)
ThemedParticles.MoonlightMusicalImpact(Vector2 position, float intensity = 1f, bool includeClef = false);

// === MUSICAL TRAIL ===
// Use in AI() for projectiles that shed musical notes
ThemedParticles.MoonlightMusicTrail(Vector2 position, Vector2 velocity);
```

#### Eroica Musical Effects

```csharp
// === MUSIC NOTES ===
// Fiery gold/crimson notes for heroic effects
ThemedParticles.EroicaMusicNotes(Vector2 position, int count = 5, float spreadRadius = 30f);

// === CLEF SYMBOL ===
// Bold golden clef for dramatic moments
ThemedParticles.EroicaClef(Vector2 position, bool useTrebleClef = true, float scale = 1f);

// === MUSIC STAFF ===
// Fiery staff lines
ThemedParticles.EroicaMusicStaff(Vector2 position, float scale = 1f);

// === ACCIDENTALS ===
// Bold gold/crimson sharps and flats
ThemedParticles.EroicaAccidentals(Vector2 position, int count = 3, float spreadRadius = 20f);

// === COMBINED MUSICAL IMPACT ===
// Heroic musical burst with optional clef
ThemedParticles.EroicaMusicalImpact(Vector2 position, float intensity = 1f, bool includeClef = false);

// === MUSICAL TRAIL ===
// Fiery musical note trail
ThemedParticles.EroicaMusicTrail(Vector2 position, Vector2 velocity);
```

#### Generic Musical Effects

```csharp
// === CUSTOM COLOR NOTES ===
// Notes with any color (for non-themed items)
ThemedParticles.MusicNotes(Vector2 position, Color color, int count = 5, float spreadRadius = 30f);

// === NOTE CASCADE ===
// Notes falling like rain (great for abilities)
ThemedParticles.MusicNoteCascade(Vector2 position, Color color, int count = 10, float width = 100f);

// === NOTE BURST ===
// Circular explosion of notes (AOE effects)
ThemedParticles.MusicNoteBurst(Vector2 position, Color color, int count = 12, float speed = 4f);

// === NOTE RING ===
// Swirling ring of notes (charging effects)
ThemedParticles.MusicNoteRing(Vector2 center, Color color, float radius = 50f, int count = 8);
```

---

## Core Particle System

Located in `Common/Systems/Particles/`

### Particle Classes

| Class | File | Description |
|-------|------|-------------|
| `BloomParticle` | `CommonParticles.cs` | Soft glowing orb |
| `BloomRingParticle` | `CommonParticles.cs` | Expanding ring effect |
| `SparkleParticle` | `CommonParticles.cs` | Twinkling star |
| `GlowSparkParticle` | `CommonParticles.cs` | Elongated directional spark |
| `GenericGlowParticle` | `CommonParticles.cs` | Simple glowing dot |
| `HeavySmokeParticle` | `CommonParticles.cs` | Animated smoke (7-frame) |

### Creating Custom Particles

```csharp
using MagnumOpus.Common.Systems.Particles;

// Bloom particle (soft glow)
var bloom = new BloomParticle(
    position,           // Vector2 spawn position
    velocity,           // Vector2 initial velocity
    color,              // Color
    startScale,         // float starting size
    endScale,           // float ending size
    lifetime            // int frames to live
);
MagnumParticleHandler.SpawnParticle(bloom);

// Sparkle particle (twinkling)
var sparkle = new SparkleParticle(
    position, velocity,
    mainColor,          // Primary color
    bloomColor,         // Glow color
    scale,              // float size
    lifetime,           // int frames
    rotationSpeed,      // float rotation per frame
    pulseSpeed          // float twinkle speed
);
MagnumParticleHandler.SpawnParticle(sparkle);

// Glow spark (directional)
var spark = new GlowSparkParticle(
    position, velocity,
    affectedByGravity,  // bool
    lifetime,           // int
    scale,              // float
    color,              // Color
    scaleRange,         // Vector2 (min, max scale during life)
    fadeIn,             // bool
    fadeOut             // bool
);
MagnumParticleHandler.SpawnParticle(spark);
```

### Texture Types (Procedurally Generated)

All textures are generated at runtime - no PNG files needed!

| Texture | Size | Description |
|---------|------|-------------|
| BloomCircle | 64x64 | Soft gaussian blur circle |
| BloomRing | 64x64 | Hollow ring for shockwaves |
| Sparkle | 32x32 | 4-point star shape |
| GlowSpark | 48x64 | Elongated spark |
| SoftGlow | 32x32 | Faded edge glow |
| Point | 8x8 | Tiny glow point |
| HeavySmoke | 64x448 | 7-frame animated smoke |
| Star4Point | 32x32 | 4-pointed star |
| Star6Point | 48x48 | 6-pointed star |

#### 🎵 Musical Textures

| Texture | Size | Description |
|---------|------|-------------|
| MusicNoteQuarter | 32x32 | Quarter note (♩) |
| MusicNoteEighth | 32x32 | Eighth note (♪) with flag |
| MusicNoteSixteenth | 32x32 | Sixteenth note with double flag |
| MusicNoteDouble | 40x40 | Double eighth notes (♫) |
| TrebleClef | 32x48 | Treble clef (𝄞) |
| BassClef | 32x32 | Bass clef (𝄢) |
| MusicStaff | 64x32 | Five horizontal staff lines |
| MusicSharp | 24x24 | Sharp symbol (♯) |
| MusicFlat | 20x28 | Flat symbol (♭) |

---

## MagnumVFX Utilities

Located in `Common/Systems/MagnumVFX.cs`

### Blend Mode Helpers

```csharp
// Switch to additive blending (for glow effects)
MagnumVFX.BeginAdditiveBlend(Main.spriteBatch);

// Draw your glow effects here...

// Return to normal blending
MagnumVFX.EndAdditiveBlend(Main.spriteBatch);
```

### Lightning Effects

```csharp
// Moonlight-themed fractal lightning
MagnumVFX.DrawMoonlightLightning(
    start,          // Vector2
    end,            // Vector2
    segments,       // int (more = more jagged)
    maxOffset,      // float (displacement amount)
    branchDepth,    // int (0 = no branches)
    branchChance    // float (0-1)
);

// Eroica-themed fractal lightning
MagnumVFX.DrawFractalLightning(
    start, end, color, segments, maxOffset, branchDepth, branchChance
);
```

### Burst Effects

```csharp
// Musical note burst (purple/blue)
MagnumVFX.CreateMusicalBurst(position, primaryColor, secondaryColor, intensity);

// Eroica spark burst (scarlet/gold)
MagnumVFX.CreateEroicaSparkBurst(position, count, radius);

// General bloom burst
MagnumVFX.CreateBloomBurst(position, color, count, minScale, maxScale, minLife, maxLife);

// Dust-based shockwave ring
MagnumVFX.CreateShockwaveRing(center, color, radius, thickness, dustCount);

// Expanding particle shockwave
MagnumVFX.CreateExpandingShockwave(position, color, startScale, expansionRate, lifetime);
```

### Prismatic Gem Effects (NEW!)

Based on the HeroicSpiritMinion's brilliant diamond-like visual effect. Creates multi-layered
radiant gem effects using additive blending.

**IMPORTANT**: These methods MUST be called within additive blend mode!

```csharp
// === EROICA PRISMATIC GEM ===
// Scarlet/pink/gold/white diamond glow - heroic flame aesthetic
// MUST be in additive blend mode (use BeginAdditiveBlend first)
MagnumVFX.DrawEroicaPrismaticGem(spriteBatch, position, scale, alpha, pulsePhase);

// === MOONLIGHT PRISMATIC GEM ===
// Purple/blue/silver/white diamond glow - ethereal lunar aesthetic
MagnumVFX.DrawMoonlightPrismaticGem(spriteBatch, position, scale, alpha, pulsePhase);

// === CUSTOM PRISMATIC GEM ===
// Custom colors for unique weapon effects (outer to core colors)
MagnumVFX.DrawCustomPrismaticGem(spriteBatch, position, 
    outerColor, midColor, innerColor, coreColor, scale, alpha, pulsePhase);

// === PRISMATIC GEM BURST ===
// Radial burst of gems - great for explosions and impacts
MagnumVFX.DrawPrismaticGemBurst(spriteBatch, position, isEroica, 
    gemCount, radius, gemScale, alpha, pulsePhase);

// === PRISMATIC GEM TRAIL ===
// Trail of gems at oldPos - perfect for projectiles
MagnumVFX.DrawPrismaticGemTrail(spriteBatch, oldPositions, isEroica, 
    baseScale, pulsePhase);
```

**Usage Example:**
```csharp
public override bool PreDraw(ref Color lightColor)
{
    SpriteBatch spriteBatch = Main.spriteBatch;
    
    // Switch to additive blending
    MagnumVFX.BeginAdditiveBlend(spriteBatch);
    
    // Draw prismatic gem trail
    MagnumVFX.DrawPrismaticGemTrail(spriteBatch, Projectile.oldPos, true, 0.5f, Projectile.timeLeft);
    
    // Draw central gem
    MagnumVFX.DrawEroicaPrismaticGem(spriteBatch, Projectile.Center, 0.8f, 1f, Projectile.timeLeft);
    
    // Return to normal blending
    MagnumVFX.EndAdditiveBlend(spriteBatch);
    
    return true;
}
```

### Utility Methods

```csharp
// Pulsing value (for breathing effects)
float pulse = MagnumVFX.GetPulse(speed, minValue, maxValue);

// Random color between two colors
Color c = MagnumVFX.RandomColor(color1, color2);
```

---

## Implementation Examples

### Example 1: Projectile with Trail

```csharp
using MagnumOpus.Common.Systems;

public class MyMoonlightProjectile : ModProjectile
{
    public override void AI()
    {
        // Add moonlight trail
        ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
        
        // Occasional sparkles
        if (Main.rand.NextBool(4))
        {
            ThemedParticles.MoonlightSparkles(Projectile.Center, 2, 10f);
        }
    }
    
    public override void OnKill(int timeLeft)
    {
        // Impact explosion
        ThemedParticles.MoonlightImpact(Projectile.Center, 1.5f);
    }
    
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        // Hit sparks
        ThemedParticles.MoonlightSparks(target.Center, target.velocity);
    }
}
```

### Example 2: Accessory with Aura

```csharp
using MagnumOpus.Common.Systems;

public class MyEroicaAccessory : ModItem
{
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        if (!hideVisual)
        {
            // Ambient fire aura
            ThemedParticles.EroicaAura(player.Center, 35f);
            
            // Occasional sparkles
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.EroicaSparkles(player.Center, 4, 30f);
            }
        }
    }
}
```

### Example 3: Boss with Full Effects

```csharp
using MagnumOpus.Common.Systems;

public class MyBoss : ModNPC
{
    public override void AI()
    {
        // Ambient aura
        ThemedParticles.EroicaAura(NPC.Center, NPC.width * 0.6f);
        
        // Phase transition
        if (transitioningToPhase2)
        {
            ThemedParticles.EroicaImpact(NPC.Center, 3f);
            ThemedParticles.EroicaShockwave(NPC.Center, 2.5f);
            ThemedParticles.SakuraPetals(NPC.Center, 20, NPC.width);
        }
    }
    
    public override void OnKill()
    {
        // Death explosion
        ThemedParticles.EroicaImpact(NPC.Center, 5f);
        for (int i = 0; i < 3; i++)
        {
            ThemedParticles.EroicaShockwave(NPC.Center, 2f + i * 0.5f);
        }
    }
}
```

### Example 4: Wings with Dodge Effect

```csharp
using MagnumOpus.Common.Systems;

public class MyWings : ModItem
{
    private void PerformDodge(Player player)
    {
        // Dodge initiation burst
        ThemedParticles.MoonlightImpact(player.Center, 1.5f);
        ThemedParticles.TeleportBurst(player.Center, isMoonlight: true);
    }
    
    private void UpdateDodgeTrail(Player player)
    {
        // While dodging, leave a trail
        ThemedParticles.DodgeTrail(player.Center, player.velocity, isMoonlight: true);
    }
}
```

---

## Best Practices

### 1. Performance Considerations

```csharp
// ✅ GOOD - Occasional spawning
if (Main.rand.NextBool(4))
{
    ThemedParticles.MoonlightSparkles(position, 3, 15f);
}

// ❌ BAD - Every frame spawning too many
for (int i = 0; i < 20; i++)
{
    ThemedParticles.MoonlightSparkles(position, 10, 50f);
}
```

### 2. Intensity Scaling

```csharp
// Scale effects based on damage/importance
float intensity = hit.Damage / 100f;
intensity = MathHelper.Clamp(intensity, 0.5f, 3f);
ThemedParticles.MoonlightImpact(position, intensity);
```

### 3. Combining with Vanilla Dust

```csharp
// ThemedParticles + reduced vanilla dust = best visuals
ThemedParticles.EroicaTrail(Projectile.Center, Projectile.velocity);

// Keep some vanilla dust for variety
if (Main.rand.NextBool(3))
{
    Dust d = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
        DustID.CrimsonTorch, 0, 0, 100, default, 1.5f);
    d.noGravity = true;
}
```

### 4. Theme Consistency

```csharp
// ✅ GOOD - Consistent theme
// Moonlight content uses Moonlight particles
ThemedParticles.MoonlightImpact(position, 1f);
ThemedParticles.MoonlightSparkles(position, 6, 25f);

// ❌ BAD - Mixed themes (unless intentional crossover)
ThemedParticles.MoonlightImpact(position, 1f);
ThemedParticles.EroicaSparks(position, velocity);  // Wrong theme!
```

### 5. Server-Side Safety

The particle system automatically handles client/server:
- Particles only spawn on clients
- Safe to call on server (no-ops automatically)
- No need for `if (Main.netMode != NetmodeID.Server)` checks

---

## File Reference

| File Path | Description |
|-----------|-------------|
| `Common/Systems/ThemedParticles.cs` | High-level themed particle API |
| `Common/Systems/Particles/Particle.cs` | Base particle class |
| `Common/Systems/Particles/MagnumParticleHandler.cs` | Particle management/rendering |
| `Common/Systems/Particles/CommonParticles.cs` | Standard particle types |
| `Common/Systems/Particles/ParticleTextureGenerator.cs` | Runtime texture generation |
| `Common/Systems/MagnumVFX.cs` | VFX utilities and helpers |
| `Common/Systems/PrimitiveTrailRenderer.cs` | GPU trail rendering |
| `Common/Systems/MagnumDrawingUtils.cs` | SpriteBatch extensions |

---

## Credits

VFX system architecture inspired by:
- **Calamity Mod** - Primitive rendering, particle batching concepts
- **Spirit Mod** - Particle effect patterns
- **Terraria Community** - Various visual effect techniques

---

*Last Updated: January 2026*
*MagnumOpus Mod - VFX System v1.0*
