# MagnumOpus VFX Integration - Complete System Documentation

## 🎯 Overview

The MagnumOpus VFX system now provides **automatic, theme-aware visual effects** for ALL bosses and enemies in the mod with **buttery-smooth 144Hz+ rendering**, **primitive trails with shaders**, **atmospheric fog effects**, and **multi-layer bloom**.

---

## 🎉 NEW ENHANCED FEATURES

### 1. **Sub-Pixel Interpolation (144Hz+ Smooth)**
- Uses `InterpolatedRenderer.GetInterpolatedCenter(npc)` for smooth position updates
- Position history tracking for accurate trail rendering
- Eliminates stuttering on high refresh rate monitors

### 2. **Primitive Trail Rendering**
- Multi-pass trails using `EnhancedTrailRenderer.RenderMultiPassTrail()`
- Automatic trail creation for bosses via `AdvancedTrailSystem.CreateThemeTrail()`
- Width functions: `QuadraticBumpWidth()` for tapered trails
- Color functions: `GradientColor()` with theme palette

### 3. **Movement Fog Effects**
- `SpawnMovementFog()` creates atmospheric fog behind fast-moving NPCs
- Theme-specific fog colors and styles
- Automatic triggering when NPC speed exceeds threshold

### 4. **Multi-Layer Bloom Rendering**
- 4-layer additive bloom in PreDraw for professional glow
- `WithoutAlpha()` pattern for proper additive blending
- Boss-specific aura rings with rotation

### 5. **Orbiting Visual Elements**
- Fate/Enigma bosses: Orbiting glyphs with cosmic rotation
- Swan Lake bosses: Orbiting feathers (black/white alternating)
- Dynamic rotation and wobble effects

### 6. **Enhanced Death Effects**
- Fog explosion burst on boss death
- Sky flash using `DynamicSkyboxSystem.TriggerFlash()`
- Trail cleanup via `AdvancedTrailSystem.EndTrail()`

---

## 🔥 Automatic VFX Integration (GlobalNPC Hooks)

### What Gets Applied Automatically

**File:** `Common/Systems/VFX/GlobalNPCVFXHooks.cs`

| Event | Effect | Systems Used |
|-------|--------|--------------|
| **OnSpawn** | Dramatic entrance with fog, light beams, particles | WeaponFogVFX, LightBeamImpactVFX, CustomParticles |
| **AI (Every Frame)** | Ambient particles, attack state detection | Theme detection, ambient particle system |
| **HitEffect** | Impact flashes, fog bursts on damage | WeaponFogVFX, LightBeamImpactVFX |
| **OnKill** | Death spectacle with explosion, halos, theme extras | All particle systems, DynamicSkyboxSystem |

### Theme Detection

The system automatically detects which theme an NPC belongs to by checking:
1. Type name (e.g., `EroicasRetribution`)
2. Full name including namespace (e.g., `MagnumOpus.Content.Fate.Bosses`)

**Supported Themes:** Fate, Eroica, SwanLake, LaCampanella, MoonlightSonata, EnigmaVariations, DiesIrae, ClairDeLune, Spring, Summer, Autumn, Winter, Nachtmusik, OdeToJoy

---

## 🎭 Boss Signature VFX

### What It Provides

**File:** `Common/Systems/VFX/BossSignatureVFX.cs`

Unique signature attack VFX for each specific boss type:

| Boss | Signature Effects |
|------|-------------------|
| **Eroica** | HeroicStrike, SakuraBladeSwing, FlamesOfValorSummon, ValorousCharge, PhoenixRise |
| **Fate** | CosmicPulse, ConstellationStrike, DestinyGlyph, TimeDistortion, UniversalJudgment |
| **Swan Lake** | MonochromaticStrike, FeatherDance, PrismaticBeam, SwanSong |
| **La Campanella** | BellToll, InfernalStrike, FlameEruption, TollOfDoom |
| **Dies Irae** | JudgmentStrike, WrathFire, DivineJudgment, Armageddon |
| **Enigma** | VoidPulse, ParadoxStrike, RealityWarp, MysteryUnveiled |
| **Nachtmusik** | NightStrike, StarfallAttack, LunarPhase |
| **Moonlight** | SonataNote, LunarBeam, CrescentSlash |
| **Seasonal** | SeasonalBurst (Spring blooms, Summer flames, Autumn leaves, Winter crystals) |
| **Ode to Joy** | JubilantStrike, SymphonyFinale |

### Usage in Boss AI

```csharp
// In your boss's attack method:
private void Attack_HeroesJudgment(Player target)
{
    // During charge-up phase
    if (SubPhase == 0 && Timer % 15 == 0)
    {
        BossSignatureVFX.Eroica.ValorousCharge(NPC.Center, Timer / 90f);
    }
    
    // On attack release
    if (SubPhase == 1 && Timer == 1)
    {
        BossSignatureVFX.Eroica.PhoenixRise(NPC.Center, 1.5f);
    }
}
```

---

## 👾 Enemy Signature VFX

### What It Provides

**File:** `Common/Systems/VFX/EnemySignatureVFX.cs`

Theme-specific attack effects for enemies:

| Theme | Enemy Types | Effects |
|-------|-------------|---------|
| **Eroica** | Centurions, Flames of Valor, Behemoths, Blitzers | CenturionStrike, FlameAttack, BehemothStrike, BlitzerDash |
| **Fate** | Heralds of Fate | HeraldAttack, HeraldGaze |
| **Swan Lake** | Shattered Prima | PrimaStrike, PrimaDance |
| **Moonlight Sonata** | Waning Deer, Shards | DeerAttack, DeerGlow, ShardAttack |
| **La Campanella** | Crawlers of the Bell | CrawlerAttack, CrawlerTrail |
| **Enigma** | Mystery's End | MysteryAttack, MysteryAmbient |

### Generic Enemy Effects

```csharp
// Works for any theme:
EnemySignatureVFX.GenericEnemySpawn(position, "Eroica", intensity);
EnemySignatureVFX.GenericEnemyDeath(position, "Fate", intensity);
EnemySignatureVFX.GenericEnemyHit(position, hitDirection, "SwanLake", isCrit, intensity);
```

---

## ⚔️ Boss Attack VFX Helpers

### What It Provides

**File:** `Common/Systems/VFX/BossAttackVFXHelper.cs`

Pre-built VFX patterns for common attack types:

| Attack Type | Telegraph Method | Execution Method |
|-------------|------------------|------------------|
| **Radial Burst** | RadialBurstTelegraph | RadialBurstRelease |
| **Dash** | DashTelegraph | DashAfterimage, DashImpact |
| **Slam** | SlamTelegraph | SlamImpact |
| **Laser** | LaserTelegraph | LaserExecution |
| **Spiral** | SpiralTelegraph | SpiralRelease |
| **Phase Transition** | PhaseTransitionBegin | PhaseTransitionClimax |

### Using Attack Helpers

```csharp
// Get theme-appropriate style
var style = UniqueWeaponVFXStyles.GetStyle("Eroica");

// Telegraph phase (show player where attack will hit)
BossAttackVFXHelper.RadialBurstTelegraph(NPC.Center, style, progress, projectileCount);

// Execution phase (the actual attack VFX)
BossAttackVFXHelper.RadialBurstRelease(NPC.Center, style, 1.2f);
```

---

## 🌫️ Core VFX Systems

### WeaponFogVFX

**File:** `Common/Systems/VFX/WeaponFogVFX.cs`

Dynamic atmospheric fog effects:

```csharp
// Spawn fog around a position
WeaponFogVFX.SpawnAttackFog(position, "Fate", 0.8f, velocity);

// Create ambient fog
WeaponFogVFX.SpawnAmbientFog(position, "Eroica", 0.5f);
```

### LightBeamImpactVFX

**File:** `Common/Systems/VFX/LightBeamImpactVFX.cs`

Divine light beam effects:

```csharp
// Impact flash
LightBeamImpactVFX.SpawnImpact(position, "SwanLake", 1.2f);

// Sustained beam
LightBeamImpactVFX.SpawnBeam(startPos, endPos, "LaCampanella", 0.8f);
```

### UniqueWeaponVFXStyles

**File:** `Common/Systems/VFX/UniqueWeaponVFXStyles.cs`

Theme color palettes and configuration:

```csharp
// Get a theme's complete style
var style = UniqueWeaponVFXStyles.GetStyle("MoonlightSonata");

// Access colors
Color primary = style.Fog.PrimaryColor;    // Theme's main color
Color secondary = style.Fog.SecondaryColor; // Theme's accent color
Color accent = style.Fog.AccentColor;       // Theme's highlight color
```

---

## 🎨 Theme Color Reference

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| **Eroica** | Scarlet (200,50,50) | Gold (255,200,80) | Sakura Pink (255,150,180) |
| **Fate** | Dark Pink (180,50,100) | Bright Red (255,60,80) | White (255,255,255) |
| **Swan Lake** | White (255,255,255) | Black (30,30,40) | Rainbow (cycling) |
| **La Campanella** | Orange (255,140,40) | Black Smoke (30,20,25) | Gold (255,200,80) |
| **Moonlight Sonata** | Dark Purple (75,0,130) | Light Blue (135,206,250) | Silver (220,220,235) |
| **Enigma** | Purple (140,60,200) | Green Flame (50,220,100) | Void Black (15,10,20) |
| **Dies Irae** | Blood Red (150,0,0) | Dark Crimson (80,0,0) | Ember (255,100,50) |

---

## 🔧 Integration Checklist

### For New Bosses

1. ✅ **Automatic VFX** - GlobalNPCVFXHooks applies fog, beams, and particles automatically
2. ✅ **Attack VFX** - Use BossAttackVFXHelper for common attack patterns
3. ✅ **Signature VFX** - Use BossSignatureVFX for unique boss identity
4. ✅ **Theme Colors** - Use UniqueWeaponVFXStyles.GetStyle() for consistent colors

### For New Enemies

1. ✅ **Automatic VFX** - GlobalNPCVFXHooks applies spawn/death/hit effects automatically
2. ✅ **Attack VFX** - Use EnemySignatureVFX for attack effects
3. ✅ **Ambient VFX** - Theme-aware ambient particles applied in AI

### For Projectiles

1. ✅ **Automatic VFX** - GlobalEnemyProjectileVFX applies trail/impact effects
2. ✅ **Boss Projectiles** - Enhanced effects for boss-spawned projectiles

---

## 📁 File Structure

```
Common/Systems/VFX/
├── GlobalNPCVFXHooks.cs         # Auto-applies VFX to all NPCs
├── GlobalEnemyProjectileVFX.cs   # Auto-applies VFX to hostile projectiles
├── BossAttackVFXHelper.cs        # Attack pattern VFX helpers
├── BossSignatureVFX.cs           # Boss-specific signature effects
├── EnemySignatureVFX.cs          # Enemy-specific attack effects
├── WeaponFogVFX.cs               # Fog effect system
├── LightBeamImpactVFX.cs         # Light beam system
├── UniqueWeaponVFXStyles.cs      # Theme style definitions
└── ... (other VFX systems)
```

---

## 🎯 Quick Reference

### Getting Theme Style
```csharp
var style = UniqueWeaponVFXStyles.GetStyle(themeName);
```

### Boss Attack Telegraph
```csharp
BossAttackVFXHelper.RadialBurstTelegraph(center, style, progress, count);
BossAttackVFXHelper.DashTelegraph(start, end, style, progress);
BossAttackVFXHelper.SlamTelegraph(center, style, progress, radius);
```

### Boss Attack Execution
```csharp
BossAttackVFXHelper.RadialBurstRelease(center, style, intensity);
BossAttackVFXHelper.DashAfterimage(position, style, intensity);
BossAttackVFXHelper.SlamImpact(position, style, intensity, radius);
```

### Enemy Attacks
```csharp
EnemySignatureVFX.EroicaCenturionStrike(position, direction, intensity);
EnemySignatureVFX.FateHeraldAttack(position, direction, intensity);
EnemySignatureVFX.SwanLakePrimaStrike(position, direction, intensity);
```

### Boss Signature Attacks
```csharp
BossSignatureVFX.Eroica.HeroicStrike(position, direction, intensity);
BossSignatureVFX.Fate.UniversalJudgment(position, intensity);
BossSignatureVFX.SwanLake.SwanSong(position, intensity);
```

---

## ✅ Build Status

All VFX integration systems compile successfully:
- **0 Errors**
- **0 Warnings**

The VFX systems are ready for use in all boss and enemy content.
