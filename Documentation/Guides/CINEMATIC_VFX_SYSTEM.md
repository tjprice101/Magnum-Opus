# Cinematic VFX System - Anamorphic Lens Flares, Energy Streaks & Impact Glints

## Overview

The MagnumOpus Cinematic VFX System provides **Calamity-style polished visual effects** using custom noise textures. This system creates:

- **Anamorphic Lens Flares** - Horizontal light streaks for boss ultimates and limit breaks
- **Energy Streaks** - Flowing energy trails for weapon swings and projectile wakes
- **Impact Glints** - Single-frame burst effects for hits and collisions
- **Enhanced Nebula Fog** - Buttery smooth fog using FBM and marble noise textures
- **🔥 Ark-Style Swing Trails** - Triangle strip mesh trails with UV-mapped noise (NEW!)

---

## 🔥 Ark of the Cosmos-Style Swing Trails (NEW!)

### What This Is

Melee weapons now get **triangle strip mesh trails with UV-mapped noise textures**, NOT discrete fog particles. This is the same technique used by Calamity's Ark of the Cosmos for buttery smooth swing trails.

### Why Triangle Strip Mesh > Discrete Particles

| Discrete Particles | Triangle Strip Mesh |
|-------------------|---------------------|
| Visible particle edges | Continuous smooth surface |
| Gaps between particles | No gaps, seamless flow |
| Each particle is separate draw call | Single mesh, efficient |
| Static texture per particle | UV scrolling animation |
| Poor blending at overlap | Proper vertex color blending |

### Automatic Integration

All MagnumOpus melee weapons automatically get Ark-style swing trails via `ArkSwingTrailGlobalItem`. The system:

1. Detects theme from weapon's namespace (Eroica, Fate, SwanLake, etc.)
2. Applies theme-appropriate colors automatically
3. Renders as **4-pass triangle strip mesh**:
   - **Pass 1:** Background fog (2.5x width, 15% alpha, FBM noise, NonPremultiplied)
   - **Pass 2:** Midground nebula (1.6x width, 25% alpha, marble noise, flowing)
   - **Pass 3:** Main trail (1.0x width, 60% alpha, energy gradient, Additive)
   - **Pass 4:** Bright core (0.35x width, 70% alpha, white, Additive)

### Manual API

```csharp
using MagnumOpus.Common.Systems.VFX;

// Update during swing (call every frame while swinging)
ArkSwingTrail.UpdateSwingTrail(
    player,
    bladeLength: 80f,
    primaryColor: new Color(200, 50, 50),
    secondaryColor: new Color(255, 200, 80),
    width: 35f,
    theme: "Eroica"
);

// End swing (call when swing animation ends)
ArkSwingTrail.EndSwingTrail(player);

// Instant arc spawn (for weapons that don't update every frame)
ArkSwingTrail.SpawnSwingArc(
    player,
    startAngle: -MathHelper.PiOver4,
    endAngle: MathHelper.PiOver4,
    bladeLength: 80f,
    primaryColor: new Color(200, 50, 50),
    secondaryColor: new Color(255, 200, 80),
    width: 35f,
    pointCount: 16,
    theme: "Eroica"
);
```

### Theme Colors (Auto-Detected)

| Theme | Primary | Secondary |
|-------|---------|-----------|
| LaCampanella | Orange (255, 100, 0) | Black smoke (30, 20, 25) |
| Eroica | Scarlet (200, 50, 50) | Gold (255, 200, 80) |
| SwanLake | White | Rainbow (cycling) |
| MoonlightSonata | Dark purple (75, 0, 130) | Light blue (135, 206, 250) |
| EnigmaVariations | Purple (140, 60, 200) | Green flame (50, 220, 100) |
| Fate | Dark pink (180, 50, 100) | Bright red (255, 60, 80) |
| Spring | Pink (255, 180, 200) | Pale green (180, 255, 180) |
| Summer | Orange (255, 140, 50) | Gold (255, 215, 0) |
| Autumn | Amber (200, 150, 80) | Crimson (180, 50, 50) |
| Winter | Ice blue (150, 200, 255) | White |

---

## Custom Noise Textures (Assets/VFX/Noise/)

| Texture | Purpose | Usage |
|---------|---------|-------|
| `HorizontalEnergyGradient.png` | Energy streak/trail base | Weapon trails, projectile wakes |
| `HorizontalBlackCoreCenterEnergyGradient.png` | Anamorphic lens flare | Boss limit breaks, ultimates |
| `NebulaWispNoise.png` | Wispy fractal noise | Fog background layer |
| `SparklyNoiseTexture.png` | Sparkle/glint pattern | Impact glints, star effects |
| `TileableFBMNoise.png` | Complex turbulent noise | Fog midground, dynamic clouds |
| `TileableMarbleNoise.png` | Flowing organic noise | Fog organic flow, energy streaks |

### Texture Requirements

- **Power-of-two dimensions** (64, 128, 256, etc.) for optimal GPU performance
- **Grayscale** - Textures are tinted with `Color` multiplication at runtime
- **Seamlessly tiling** (FBM, Marble, Nebula) for scrolling effects

---

## Quick Start API

### Lens Flares (Boss Ultimates)

```csharp
using MagnumOpus.Common.Systems.VFX;

// Basic lens flare
CinematicVFX.SpawnLensFlare(
    worldPosition: boss.Center,
    color: new Color(255, 200, 80),  // Eroica Gold
    scale: 1.5f,
    lifetime: 25,
    useBlackCore: false,
    intensity: 1f
);

// Boss limit break (multi-flare spectacle)
CinematicVFX.SpawnBossLimitBreak(
    worldPosition: boss.Center,
    primaryColor: new Color(255, 80, 50),  // Scarlet
    accentColor: new Color(255, 200, 80),   // Gold
    intensity: 2f
);
```

### Impact Glints (Projectile Hits)

```csharp
// Basic impact glint
CinematicVFX.SpawnImpactGlint(
    worldPosition: hitPosition,
    color: themeColor,
    maxScale: 0.8f,
    lifetime: 12,
    horizontal: true,   // Horizontal streak shape
    sparkly: false      // Use energy texture vs sparkly noise
);

// Critical hit burst (multiple glints)
CinematicVFX.SpawnCriticalHitBurst(
    worldPosition: hitPosition,
    primaryColor: themeColor,
    accentColor: Color.White
);
```

### Energy Streaks (Weapon Trails)

```csharp
// Energy streak on projectile
CinematicVFX.SpawnEnergyStreak(
    worldPosition: projectile.Center,
    velocity: -projectile.velocity * 0.1f,
    primaryColor: startColor,
    secondaryColor: endColor,
    scale: 0.8f,
    lifetime: 15,
    scrollSpeed: 2f
);
```

### Enhanced Nebula Fog

```csharp
// Single nebula cloud
CinematicVFX.SpawnEnhancedNebula(
    worldPosition: pos,
    velocity: vel,
    primaryColor: darkPink,
    secondaryColor: darkPurple,
    scale: 1f,
    lifetime: 45,
    useMarble: false  // false = FBM noise, true = Marble flow noise
);

// Nebula wisps during melee swing
CinematicVFX.SpawnSwingNebulaWisps(
    player: player,
    swingProgress: swingPct,
    primaryColor: Color.White,
    secondaryColor: Color.Gray,
    scale: 1.2f
);

// Fate theme cosmic nebula (dark prismatic)
CinematicVFX.SpawnFateCosmicNebula(
    worldPosition: pos,
    velocity: vel,
    scale: 1f
);
```

---

## Rendering Pipeline

The CinematicVFX system renders in **three passes** for proper layering:

### Pass 1: Nebula Clouds (Background)
- **BlendState**: `NonPremultiplied` (soft alpha)
- Uses FBM/Marble noise for organic texture
- Large scale, low opacity, slow movement
- Then switches to `Additive` for glow highlights

### Pass 2: Energy Streaks
- **BlendState**: `Additive` (glow accumulation)
- Uses horizontal gradient texture
- Marble noise overlay for organic flow
- Color lerp over lifetime

### Pass 3: Lens Flares & Glints
- **BlendState**: `Additive` (screen-space glare)
- Uses energy gradient or sparkly noise
- White core + colored outer glow
- Soft glow halo overlay

---

## Integration with Existing Systems

### LayeredNebulaFog
LayeredNebulaFog.cs now automatically uses:
- `CinematicVFX.NebulaWispNoise` for background noise layer
- `CinematicVFX.MarbleNoise` for midground flow layer
- Falls back to procedural `ParticleTextureGenerator.CloudNoise` if textures fail to load

### Automatic Projectile Effects
`CinematicVFXProjectile` (GlobalProjectile) automatically applies impact glints when MagnumOpus projectiles are killed, detecting theme colors from namespace.

### Automatic Boss Death Effects
`CinematicVFXNPC` (GlobalNPC) triggers `SpawnBossLimitBreak()` on boss death with theme-appropriate colors.

---

## Theme Color Reference

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| Eroica | `(200, 50, 50)` Scarlet | `(255, 200, 80)` Gold | Sakura Pink |
| Fate | `(140, 50, 90)` Dark Pink | `(255, 240, 255)` Cosmic White | Glyphs |
| SwanLake | `White` | `(200, 200, 220)` Silver | Rainbow |
| MoonlightSonata | `(100, 60, 160)` Purple | `(150, 200, 255)` Ice Blue | Silver |
| LaCampanella | `(255, 120, 30)` Orange | `(255, 200, 80)` Gold | Black Smoke |
| Enigma | `(100, 50, 150)` Purple | `(50, 200, 100)` Green | Void |

---

## Performance Notes

- Maximum active effects are capped:
  - 8 lens flares
  - 30 impact glints  
  - 20 energy streaks
  - 40 nebula clouds
- Oldest effects are removed when caps are reached
- Update runs in `PostUpdateProjectiles()` for consistent timing
- Render runs in `PostDrawTiles()` for proper layering

---

## Files

| File | Purpose |
|------|---------|
| `Common/Systems/VFX/CinematicVFX.cs` | Core system with all effect types |
| `Common/Systems/VFX/CinematicVFXSystem.cs` | ModSystem hooks (Update, Render) |
| `Common/Systems/VFX/LayeredNebulaFog.cs` | Three-pass fog system (uses CinematicVFX textures) |
| `Assets/VFX/Noise/*.png` | Custom noise textures |
