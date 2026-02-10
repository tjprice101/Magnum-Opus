# PER-WEAPON VFX STANDARDS - Calamity-Style Visual Effects

> **THIS DOCUMENT REFLECTS THE PER-WEAPON VFX ARCHITECTURE.**
> 
> **IMPORTANT:** Global VFX systems are **DISABLED** and have been **DELETED**.
> Each weapon, projectile, and boss must implement its OWN unique VFX directly 
> in its .cs file, like Calamity's Ark of the Cosmos.

---

## ✅ THE VFX ARCHITECTURE

### 🔥 KEY INSIGHT: PER-WEAPON VFX (Like Ark of the Cosmos)

The global VFX systems have been **REMOVED** from the codebase. The philosophy is:

| Old Approach (DELETED) | New Approach (CURRENT) |
|------------------------|------------------------|
| Global systems auto-applied generic VFX | Each weapon has its OWN unique VFX code |
| Cookie-cutter effects on everything | Every weapon feels unique and memorable |
| `GlobalVFXOverhaul.cs` (deleted) | Implement in each projectile's .cs file |
| `GlobalWeaponVFXOverhaul.cs` (deleted) | Implement in each weapon's .cs file |
| `GlobalBossVFXOverhaul.cs` (deleted) | Implement in each boss's .cs file |

### VFX Utility Classes (Use As Libraries)

These utility classes are available for building per-weapon VFX:

| Technology | File | What It Does |
|------------|------|--------------|
| **Multi-Layer Bloom** | `BloomRenderer.cs` | `DrawBloomStack()`, `DrawSimpleBloom()` |
| **Primitive Trails** | `EnhancedTrailRenderer.cs` | Multi-pass trails with `PrimitiveSettings` |
| **Interpolated Rendering** | `InterpolatedRenderer.cs` | 144Hz+ smoothness via `PartialTicks` |
| **Theme Palettes** | `MagnumThemePalettes.cs` | `GetThemePalette()`, `GetThemeColor()` |
| **Light Rays** | `GodRaySystem.cs` | `CreateBurst()` with `GodRayStyle` |
| **Impact Flares** | `ImpactLightRays.cs` | `SpawnImpactRays()` |
| **Screen Distortion** | `ScreenDistortionManager.cs` | `TriggerRipple()` |
| **Bézier Paths** | `BezierProjectileSystem.cs` | Curved projectile paths |
| **Elemental Effects** | `UniversalElementalVFX.cs` | Flames, lightning, petals, frost, void, cosmic |
| **Boss Arena VFX** | `BossArenaVFX.cs` | Persistent ambient particles with parallax |

---

## 🚀 HOW TO IMPLEMENT PER-WEAPON VFX

### Every Weapon Implements Its Own VFX

Each weapon's .cs file must contain its own VFX logic:

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

### Using VFX Utility Classes

```csharp
using MagnumOpus.Common.Systems.VFX;

// Bloom effects
BloomRenderer.DrawBloomStack(spriteBatch, position, color, 0.5f, layers: 4, intensity: 1f);
BloomRenderer.DrawSimpleBloom(spriteBatch, pos, color, scale);

// Primitive trails
var settings = new EnhancedTrailRenderer.PrimitiveSettings(
    width: EnhancedTrailRenderer.LinearTaper(20f),
    color: EnhancedTrailRenderer.GradientColor(startColor, endColor),
    smoothen: true
);
EnhancedTrailRenderer.RenderMultiPassTrail(oldPos, oldRot, settings, passes: 3);

// Interpolated rendering for 144Hz+
float partialTicks = InterpolatedRenderer.PartialTicks;
Vector2 smoothPos = Vector2.Lerp(previousPosition, currentPosition, partialTicks);

// Light rays
GodRaySystem.CreateBurst(center, direction, color, rayCount: 8, 
    length: 100f, width: 10f, lifetime: 30, GodRayStyle.Explosion);

// Impact flares
ImpactLightRays.SpawnImpactRays(position, color, rayCount: 6, baseLength: 60f, lifetime: 20);

// Screen effects
ScreenDistortionManager.TriggerRipple(worldPosition, intensity: 0.5f, duration: 20);

// Universal elemental effects
UniversalElementalVFX.InfernalEruption(pos, primaryColor, secondaryColor, 1.5f, true);

// Boss arena ambience
BossArenaVFX.Activate("LaCampanella", bossCenter, 600f, intensity: 1f);
```

---

## 🎵 MUSIC NOTE INTEGRATION

Music notes must be spawned manually in per-weapon VFX:

```csharp
// ThemedParticles for manual spawning
ThemedParticles.MusicNoteBurst(position, themeColor, count: 6, speed: 4f);

// Orbiting music notes (THE CORRECT WAY)
float orbitAngle = Main.GameUpdateCount * 0.08f;
for (int i = 0; i < 3; i++)
{
    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 15f;
    ThemedParticles.MusicNote(notePos, Projectile.velocity * 0.8f, themeColor, 0.75f, 30);
}
```

### Music Note Visibility Rules

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
ScreenDistortionManager.TriggerRipple(worldPosition, intensity: 0.5f, duration: 20);

// Sky flash (screen-wide color flash)
DynamicSkyboxSystem.TriggerFlash(Color.White, intensity: 1.2f);
```

---

## 🎨 THEME COLOR PALETTES

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

## 📋 CHECKLIST: Per-Weapon VFX Implementation

### For Projectiles:
- [ ] Implement unique AI() with dust/particle trail
- [ ] Implement PreDraw() with BloomRenderer calls
- [ ] Add theme-appropriate music notes (scale 0.7f+)
- [ ] Use EnhancedTrailRenderer for persistent trails
- [ ] Include orbiting elements for visual interest

### For Weapons:
- [ ] Implement unique swing effects in UseItemFrame()
- [ ] Add muzzle flash in Shoot() for ranged
- [ ] Use SwordArc textures for melee waves
- [ ] Include theme-appropriate particles

### For Bosses:
- [ ] Implement unique attack windups with converging particles
- [ ] Add attack release VFX with bloom cascade
- [ ] Include phase transition spectacles
- [ ] Add death explosion finale

---

## 📚 FILE REFERENCE

### Core VFX Utilities

| File | Purpose |
|------|---------|
| `Common/Systems/VFX/Core/BloomRenderer.cs` | Multi-layer bloom stacking |
| `Common/Systems/VFX/Core/InterpolatedRenderer.cs` | Sub-pixel interpolation for 144Hz+ |
| `Common/Systems/VFX/Core/MagnumThemePalettes.cs` | Theme color arrays |
| `Common/Systems/VFX/Core/VFXUtilities.cs` | QuadraticBump, PaletteLerp, math utilities |
| `Common/Systems/VFX/Core/VFXTextureRegistry.cs` | Noise, LUTs, beams, masks |

### Trail Systems

| File | Purpose |
|------|---------|
| `Common/Systems/VFX/Trails/EnhancedTrailRenderer.cs` | Multi-pass primitive trails |
| `Common/Systems/VFX/Trails/PixelatedTrailRenderer.cs` | FargosSoulsDLC-style pixelated trails |

### Effect Systems

| File | Purpose |
|------|---------|
| `Common/Systems/VFX/Screen/ScreenDistortionManager.cs` | Screen ripple effects |
| `Common/Systems/VFX/Screen/DynamicSkyboxSystem.cs` | Sky flash effects |
| `Common/Systems/VFX/Core/GodRaySystem.cs` | Light ray bursts |
| `Common/Systems/VFX/Core/ImpactLightRays.cs` | Impact flares |
| `Common/Systems/VFX/Core/UniversalElementalVFX.cs` | Elemental effects library |
| `Common/Systems/VFX/Boss/BossArenaVFX.cs` | Boss arena ambient particles |

---

## 🕹️ THE GOLD STANDARD: Iridescent Wingspan

**Study this weapon's VFX patterns for inspiration:**

**Key Patterns (Implement in YOUR weapon):**
- Heavy dust trails (2+ particles per frame, scale 1.5f+)
- Contrasting sparkles (opposite colors for visual pop)
- Frequent flares (1-in-2 chance, not 1-in-10)
- Color oscillation (Main.hslToRgb for dynamic hue shifts)
- Multi-layer PreDraw (4+ glow layers with different scales/rotations)
- Orbiting music notes (locked to projectile position, scale 0.7f+)

```csharp
// Example: Dense dust trail
for (int i = 0; i < 2; i++)
{
    Dust d = Dust.NewDustPerfect(Projectile.Center, dustType, vel, 100, color, 1.8f);
    d.noGravity = true;
    d.fadeIn = 1.4f;
}

// Example: Multi-layer bloom in PreDraw
float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f;
spriteBatch.Draw(tex, pos, null, color * 0.5f, rot, origin, scale * 1.4f * pulse, ...);
spriteBatch.Draw(tex, pos, null, color * 0.3f, rot, origin, scale * 1.2f * pulse, ...);
spriteBatch.Draw(tex, pos, null, Color.White, rot, origin, scale * pulse, ...);
```

**Every weapon deserves this level of VFX polish.**
