# CALAMITY-STYLE VFX STANDARDS - Buttery Smooth Visual Effects

> **THIS DOCUMENT REFLECTS THE NEW PER-WEAPON VFX ARCHITECTURE.**
> 
> **IMPORTANT:** Global VFX systems are **DISABLED**. Each weapon, projectile, and boss
> must implement its OWN unique VFX directly in its .cs file, like Calamity's Ark of the Cosmos.

---

## ✅ THE NEW VFX ARCHITECTURE

### 🔥 KEY INSIGHT: PER-WEAPON VFX (Like Ark of the Cosmos)

The global VFX systems are **DISABLED** via `VFXMasterToggle.GlobalSystemsEnabled = false`:

| System | Status | What To Do Instead |
|--------|--------|-------------------|
| `GlobalVFXOverhaul.cs` | **DISABLED** | Implement projectile VFX in each projectile's .cs file |
| `GlobalWeaponVFXOverhaul.cs` | **DISABLED** | Implement weapon VFX in each weapon's .cs file |
| `GlobalBossVFXOverhaul.cs` | **DISABLED** | Implement boss VFX in each boss's .cs file |

### Core Technologies (Use As Libraries)

These utility classes are still available for building per-weapon VFX:

| Technology | File | What It Does |
|------------|------|--------------|
| **Sub-Pixel Interpolation** | `InterpolatedRenderer.cs` | 144Hz+ smoothness via `GetInterpolatedCenter()` |
| **Bézier Curve Paths** | `BezierProjectileSystem.cs` | Curved homing arcs, snaking paths, spiral approaches |
| **Primitive Trail Rendering** | `EnhancedTrailRenderer.cs` | Multi-pass trails with `PrimitiveSettings` (width/color functions) |
| **Advanced Trail System** | `AdvancedTrailSystem.cs` | Theme-based trail creation via `CreateThemeTrail()` |
| **🔥 Ark-Style Swing Trails** | `ArkSwingTrail.cs` | **Triangle strip mesh with UV-mapped noise textures** |
| **Screen Effects** | `ScreenDistortionManager.cs` | Distortion via `TriggerThemeEffect()` |
| **Dynamic Skybox** | `DynamicSkyboxSystem.cs` | Sky flashes via `TriggerFlash()` |
| **Procedural VFX** | `ProceduralProjectileVFX.cs` | PNG-free rendering via `DrawProceduralProjectile()` |
| **Cinematic VFX** | `CinematicVFX.cs` | Lens flares, energy streaks, impact glints |
| `Eroica` | Eroica | Scarlet → Gold, Sakura accents |
| `Fate` | Fate | Black → Pink → Red, cosmic white |
| `SwanLake` | SwanLake | White/Black, rainbow shimmer |
| `MoonlightSonata` | MoonlightSonata | Purple → Ice Blue |
| `LaCampanella` | LaCampanella | Black smoke → Orange flame |
| `Enigma` | EnigmaVariations | Void purple → Green flame |
| `Spring` | Spring | Pink → Green pastels |
| `Summer` | Summer | Orange → Gold warmth |
| `Autumn` | Autumn | Amber → Crimson |
| `Winter` | Winter | Ice Blue → White |

---

## 🚀 HOW TO IMPLEMENT PER-WEAPON VFX

### Every Weapon Implements Its Own VFX

Since global systems are disabled, each weapon's .cs file must contain its own VFX logic:

```csharp
// ✅ CORRECT: Implement unique VFX directly in the weapon
public class MyEroicaProjectile : ModProjectile
{
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.timeLeft = 120;
    }
    
    public override void AI()
    {
        // Implement YOUR unique trail for THIS projectile
        if (Main.rand.NextBool(2))
        {
            Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, 
                -Projectile.velocity * 0.2f, 0, Color.Orange, 1.5f);
            d.noGravity = true;
        }
    }
    
    public override bool PreDraw(ref Color lightColor)
    {
        // Use BloomRenderer for multi-layer glow unique to THIS projectile
        BloomRenderer.DrawBloomStack(Main.spriteBatch, 
            Projectile.Center - Main.screenPosition, Color.Orange, 0.5f, 4, 1f);
        return true;
    }
}
```

### For Custom Unique Effects: Use CalamityStyleVFX

When you want effects BEYOND the automatic ones, call `CalamityStyleVFX` directly:

```csharp
using MagnumOpus.Common.Systems.VFX;

// In a weapon's Shoot method - add extra wing effect
CalamityStyleVFX.EtherealWingEffect(player, "SwanLake", 1.2f);

// In a projectile's AI - add wave effect
CalamityStyleVFX.WaveProjectileEffect(Projectile.Center, Projectile.velocity, "Eroica", 1f);

// In a boss's attack windup
CalamityStyleVFX.BossAttackWindup(NPC.Center, progress, "LaCampanella");

// On attack release
CalamityStyleVFX.BossAttackRelease(NPC.Center, "Fate", 1.5f);

// Spectacular death explosion
CalamityStyleVFX.SpectacularDeath(position, "Eroica");

// Boss phase transition
CalamityStyleVFX.BossPhaseTransition(NPC.Center, "Fate", 1.5f);
```

---

## 🎵 MUSIC NOTE INTEGRATION

Music notes are automatically spawned by GlobalVFXOverhaul, but for custom control:

```csharp
// ThemedParticles still works for manual spawning
ThemedParticles.MusicNoteBurst(position, themeColor, count: 6, speed: 4f);

// Phase10Integration for themed musical effects
Phase10Integration.Universal.MusicalProjectileTrail(position, velocity, themeColor, noteColor);
Phase10Integration.Universal.DramaticImpact(position, primaryColor, secondaryColor, intensity);
Phase10Integration.Universal.DeathFinale(position, colorArray, intensity);
```

### Music Note Visibility Rules (Still Apply)

| Scale | Visibility | Use Case |
|-------|------------|----------|
| < 0.5f | TOO SMALL | Never use |
| 0.6f - 0.8f | Visible | Trail notes |
| 0.8f - 1.0f | Bold | Impact notes |
| 1.0f+ | Very Bold | Finale notes |

---

## 🌟 SCREEN EFFECTS

For dramatic moments, trigger screen effects:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Screen distortion (ripple effect)
ScreenDistortionManager.TriggerThemeEffect("Fate", worldPosition, intensity: 0.5f, duration: 20);

// Sky flash (screen-wide color flash)
DynamicSkyboxSystem.TriggerFlash(Color.White, intensity: 1.2f);

// Combined for boss phase transitions
CalamityStyleVFX.BossPhaseTransition(bossCenter, "Eroica", scale: 1.5f);
// Automatically does: distortion + sky flash + particle cascade + screen shake
```

---

## 📦 ADVANCED TRAIL SYSTEM

For managed trails that persist across frames:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Create a theme-based trail
int trailId = AdvancedTrailSystem.CreateThemeTrail(
    theme: "Fate",
    width: 22f,
    maxPoints: 25,
    intensity: 1f
);

// Update trail each frame
AdvancedTrailSystem.UpdateTrail(trailId, Projectile.Center, Projectile.rotation);

// Destroy when done
AdvancedTrailSystem.DestroyTrail(trailId);
```

---

## 🎨 PROCEDURAL VFX (PNG-FREE)

For projectiles that should render without texture files:

```csharp
using MagnumOpus.Common.Systems.VFX;

public override bool PreDraw(ref Color lightColor)
{
    // Use procedural rendering instead of textures
    ProceduralProjectileVFX.DrawProceduralProjectile(
        Main.spriteBatch,
        Projectile.Center - Main.screenPosition,
        Projectile.rotation,
        primaryColor,
        secondaryColor,
        scale: 1f,
        "Eroica"  // Theme preset
    );
    return false;
}
```

Available theme presets:
- `DrawEroicaProjectile`, `DrawFateProjectile`, `DrawSwanLakeProjectile`
- `DrawMoonlightSonataProjectile`, `DrawLaCampanellaProjectile`, `DrawEnigmaProjectile`
- `DrawSpringProjectile`, `DrawSummerProjectile`, `DrawAutumnProjectile`, `DrawWinterProjectile`

---

## 📋 CHECKLIST: Does My Content Need Custom VFX Code?

### For Projectiles:
- [ ] Is it in a theme folder? → **NO CODE NEEDED**, GlobalVFXOverhaul handles it
- [ ] Does it need speed-based effects? → Override AI for custom logic
- [ ] Does it need phase-based rendering? → Override PreDraw with state checks
- [ ] Does it need curved paths? → Use BezierProjectileSystem in AI

### For Weapons:
- [ ] Is it a standard melee/ranged/magic? → **NO CODE NEEDED**, GlobalWeaponVFXOverhaul handles it
- [ ] Does it need unique swing style? → Use CalamityStyleVFX.SmoothMeleeSwing with custom MeleeSwingVariation
- [ ] Does it need special shoot effects? → Call CalamityStyleVFX methods in Shoot()

### For Bosses:
- [ ] Basic rendering? → **NO CODE NEEDED**, GlobalBossVFXOverhaul handles it
- [ ] Attack windups? → Call CalamityStyleVFX.BossAttackWindup()
- [ ] Attack releases? → Call CalamityStyleVFX.BossAttackRelease()
- [ ] Phase transitions? → Call CalamityStyleVFX.BossPhaseTransition()
- [ ] Death explosion? → Call CalamityStyleVFX.SpectacularDeath()

---

## 🔧 THEME COLOR PALETTES

All themes have defined color arrays in `MagnumThemePalettes`:

```csharp
using MagnumOpus.Common.Systems.VFX;

// Get gradient color for any theme
Color gradientColor = MagnumThemePalettes.GetThemeColor("Eroica", progress);

// Or access palette directly
Color[] eroicaPalette = MagnumThemePalettes.Eroica; // Scarlet → Crimson → Gold
Color[] fatePalette = MagnumThemePalettes.Fate;     // Black → Pink → Red → White
```

| Theme | Palette Progression |
|-------|-------------------|
| Eroica | Scarlet → Crimson → Gold |
| Fate | Black → DarkPink → BrightRed → White |
| SwanLake | White → Black (rainbow shimmer) |
| MoonlightSonata | DarkPurple → Violet → LightBlue → Silver |
| LaCampanella | Black → Orange → Gold |
| EnigmaVariations | Black → DeepPurple → Purple → GreenFlame |

---

## 🎯 FINAL SUMMARY

### The Old Way (DON'T DO THIS ANYMORE):
```csharp
// ❌ OLD: Manual dust spawning in AI
for (int i = 0; i < 2; i++)
{
    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 0, color, 1.5f);
    d.noGravity = true;
}

// ❌ OLD: Manual flare drawing in PreDraw
Main.spriteBatch.Draw(flareTex, drawPos, null, color * 0.5f, rot, origin, scale, ...);
```

### The New Way (DO THIS):
```csharp
// ✅ NEW: Let GlobalVFXOverhaul handle it automatically
// Just define your projectile, the system does the rest

// ✅ NEW: For custom effects, call the VFX systems
CalamityStyleVFX.SpectacularDeath(position, "Eroica");
BezierProjectileSystem.QuadraticBezier(p0, p1, p2, t);
EnhancedTrailRenderer.RenderMultiPassTrail(positions, rotations, settings, 3);
```

**The systems handle the complexity. You focus on gameplay.**

---

## 📚 FILE REFERENCE

| File | Purpose |
|------|---------|
| `Common/Systems/VFX/GlobalVFXOverhaul.cs` | Auto-applies VFX to all projectiles |
| `Common/Systems/VFX/GlobalWeaponVFXOverhaul.cs` | Auto-applies VFX to all weapons |
| `Common/Systems/VFX/GlobalBossVFXOverhaul.cs` | Auto-applies VFX to all bosses |
| `Common/Systems/VFX/CalamityStyleVFX.cs` | Central VFX library with all methods |
| `Common/Systems/VFX/InterpolatedRenderer.cs` | Sub-pixel interpolation for 144Hz+ |
| `Common/Systems/VFX/BezierProjectileSystem.cs` | Curved projectile paths |
| `Common/Systems/VFX/EnhancedTrailRenderer.cs` | Multi-pass primitive trail rendering |
| `Common/Systems/VFX/AdvancedTrailSystem.cs` | Theme-based trail management |
| `Common/Systems/VFX/ScreenDistortionManager.cs` | Screen ripple effects |
| `Common/Systems/VFX/DynamicSkyboxSystem.cs` | Sky flash effects |
| `Common/Systems/VFX/ProceduralProjectileVFX.cs` | PNG-free procedural rendering |
| `Common/Systems/VFX/MagnumThemePalettes.cs` | Theme color arrays |
| `Common/Systems/VFX/BloomRenderer.cs` | Multi-layer bloom stacking: `DrawBloomStack()`, `DrawSimpleBloom()` |
| `Common/Systems/VFX/GodRaySystem.cs` | Light ray bursts: `CreateBurst()` with `GodRayStyle` |
| `Common/Systems/VFX/ImpactLightRays.cs` | Impact flares: `SpawnImpactRays()` |
| `Common/Systems/VFX/UniversalElementalVFX.cs` | **NEW** - Universal elemental effects (flames, lightning, petals, etc.) |
| `Common/Systems/VFX/BossArenaVFX.cs` | **NEW** - Persistent boss arena ambient particles |

---

## 🕹️ LEGACY: MANUAL VFX PATTERNS (For Reference Only)

> **NOTE:** The patterns below are from the old system. They are kept for reference
> when you need to understand what the Global systems do internally, or when creating
> truly unique signature effects that can't use the automatic systems.

### The Gold Standard: Iridescent Wingspan (Legacy Reference)

This weapon's VFX patterns were the inspiration for the automatic systems:

**Key Patterns (Now Automated):**
- Heavy dust trails (2+ particles per frame, scale 1.5f+)
- Contrasting sparkles (opposite colors for visual pop)
- Frequent flares (1-in-2 chance, not 1-in-10)
- Color oscillation (Main.hslToRgb for dynamic hue shifts)
- Multi-layer PreDraw (4+ glow layers with different scales/rotations)
- Orbiting music notes (locked to projectile position)

These patterns are now built into `GlobalVFXOverhaul` and apply automatically.

### When To Study Legacy Patterns

Study the old manual code when:
1. Creating a signature weapon with truly unique identity
2. Debugging why automatic VFX aren't appearing correctly
3. Understanding the math behind the systems
4. Extending the automatic systems with new features
