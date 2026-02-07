# MagnumOpus Quick Reference - VFX Design Rules

> **BURN THESE RULES INTO YOUR MEMORY. NO EXCEPTIONS.**
> 
> **🔥 FULL GUIDE**: See **[TRUE_VFX_STANDARDS.md](../Documentation/Guides/TRUE_VFX_STANDARDS.md)** for complete examples.

---

## 🎉 NEW: AUTOMATIC CALAMITY-STYLE VFX SYSTEM 🎉

**The mod now has GLOBAL VFX systems that auto-apply effects to ALL content!**

| System | What It Does |
|--------|--------------|
| `GlobalVFXOverhaul.cs` | Projectiles: Primitive trails, 4-layer bloom, orbiting notes, spectacular deaths |
| `GlobalWeaponVFXOverhaul.cs` | Weapons: Smooth swing arcs, muzzle flash, magic circles |
| `GlobalBossVFXOverhaul.cs` | Bosses: Interpolated rendering, dash trails, entrance/death spectacles |
| `CalamityStyleVFX.cs` | Manual VFX library for unique effects |

### Core Technologies

| Tech | Description |
|------|-------------|
| **Bézier Curves** | Curved projectile paths (`BezierProjectileSystem.cs`) |
| **Primitive Trails** | Multi-pass shader trails with bloom (`EnhancedTrailRenderer.cs`) |
| **Interpolation** | 144Hz+ sub-pixel smoothness (`InterpolatedRenderer.cs`) |
| **Multi-Layer Bloom** | 4-layer additive glow stacking (`BloomRenderer.cs`) |
| **God Rays** | Light ray bursts (`GodRaySystem.cs`) |
| **Impact Rays** | Impact flares (`ImpactLightRays.cs`) |
| **Screen Effects** | Screen distortion (`ScreenDistortionManager.cs`) |

### For Unique Effects, Use These Systems:

```csharp
using MagnumOpus.Common.Systems.VFX;

// High-level APIs
CalamityStyleVFX.SmoothMeleeSwing(player, "Eroica", swingProgress, direction);
CalamityStyleVFX.SpectacularDeath(position, "LaCampanella");
UniversalElementalVFX.LaCampanellaFlames(pos, vel, 1f);  // NEW
BossArenaVFX.Activate("Fate", center, 800f, 1f);         // NEW

// Core renderers (use these for custom effects)
BloomRenderer.DrawBloomStack(spriteBatch, pos, color, scale, 4, 1f);
EnhancedTrailRenderer.RenderMultiPassTrail(positions, rotations, settings, 3);
GodRaySystem.CreateBurst(center, direction, color, 8, 100f, 10f, 30, GodRayStyle.Explosion);
ImpactLightRays.SpawnImpactRays(position, color, 6, 60f, 20);
```

---

## 🚨 LEGACY: THE #1 PROBLEM (NOW AUTO-HANDLED)

**"Slapping a flare" on PreDraw is NOT a visual effect. The Global systems now handle this automatically!**

| ❌ WRONG | ✅ CORRECT (NOW AUTOMATIC) |
|----------|-----------|
| Single flare on PreDraw | **Layer 4+ flares** spinning at different speeds |
| Sparse dust trail | **Dense dust** (2+ per frame, scale 1.5f+) |
| No color oscillation | **Main.hslToRgb** for color shimmer |
| Static music notes | **Orbiting music notes** that lock to projectile |
| Basic "puff" impact | **Glimmer cascade** with rings + sparkles |
| Rigid straight trails | **Curved trails** (Ark of the Cosmos style) |

---

## ⭐ THE GOLD STANDARD: Iridescent Wingspan

**STUDY THIS WEAPON. COPY ITS PATTERNS.**

```csharp
// Trail: HEAVY DUST (every frame, 2+ particles, scale 1.8f!)
for (int i = 0; i < 2; i++)
{
    Dust d = Dust.NewDustPerfect(pos, dustType, vel, 100, color, 1.8f);
    d.noGravity = true;
    d.fadeIn = 1.4f;
}

// Sparkles: 1-in-2, not 1-in-10!
if (Main.rand.NextBool(2))
    CustomParticles.GenericFlare(pos + offset, color, 0.5f, 18);

// Color shift: Main.hslToRgb
float hue = Main.rand.NextFloat();
Color rainbow = Main.hslToRgb(hue, 1f, 0.7f);

// PreDraw: MULTIPLE SPINNING LAYERS
Main.EntitySpriteDraw(tex, pos, null, color * 0.5f, rot, origin, scale * 1.4f, ...);
Main.EntitySpriteDraw(tex, pos, null, color * 0.3f, rot, origin, scale * 1.2f, ...);
Main.EntitySpriteDraw(tex, pos, null, Color.White, rot, origin, scale, ...);
```

---

## ⭐⭐⭐ RULE #1: EVERY WEAPON IS UNIQUE ⭐⭐⭐

**If a theme has 3 swords, those 3 swords have COMPLETELY DIFFERENT effects.**

| Sword | On-Swing | Trail | Impact | Special |
|-------|----------|-------|--------|---------|
| A | Spiraling orbs | Music note constellation | Harmonic shockwave | Orbs connect with beams |
| B | Burning afterimages | Ember + smoke wisps | Rising flame pillars | Charge summons phantom blade |
| C | Homing feathers | Prismatic rainbow arc | Crystalline explosion | 4th hit = gravity well |

**Same colors. DIFFERENT effects. ALWAYS.**

---

## ❌ THE FORBIDDEN PATTERN

```csharp
// NEVER DO THIS - This is GARBAGE
public override void OnHitNPC(...)
{
    CustomParticles.GenericFlare(target.Center, color, 0.5f, 15);
    CustomParticles.HaloRing(target.Center, color, 0.3f, 12);
}
// "On swing hit enemy boom yippee" is DISGUSTING.
```

---

## 🎵 MUSIC NOTES MUST BE VISIBLE + ORBIT

| ❌ WRONG | ✅ CORRECT |
|----------|-----------|
| Scale 0.25f-0.4f | Scale **0.7f-1.2f** |
| Random spawn | **Orbit projectile** |
| No bloom | **Multi-layer bloom** |
| Static | **Shimmer/pulse animation** |
| Alone | **With sparkle companions** |

```csharp
// ✅ CORRECT - Visible, ORBITING music notes
float orbitAngle = Main.GameUpdateCount * 0.08f;
for (int i = 0; i < 3; i++)
{
    float noteAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 15f;
    ThemedParticles.MusicNote(notePos, Projectile.velocity * 0.8f, color, 0.75f, 30);
}
```

---

## ⚔️ MELEE: USE SWORDARC TEXTURES!

**We have 9 SwordArc PNGs. USE THEM.**

```csharp
// Wave projectiles: Layer arcs with glows, NOT png copy-paste!
Texture2D arc = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc2").Value;
// Draw in PreDraw with additive blending, multiple layers
```

---

## 🎨 USE ALL PARTICLE ASSETS

**You have 100+ custom PNGs. USE THEM.**

| Category | Variants | USE FOR |
|----------|----------|---------|
| **MusicNote** | 6 | EVERY trail, impact, aura |
| **EnergyFlare** | 2 | Projectile cores - **LAYER MULTIPLE!** |
| **PrismaticSparkle** | 3 | Sparkle accents |
| **SwordArc** | 9 | Melee swing effects - **USE THESE!** |
| **Glyphs** | 12 | Magic circles, Fate theme |
| **SwanFeather** | 10 | Swan Lake theme |
| **SoftGlow** | 3 | Bloom bases - layer under flares |
| **GlowingHalo** | 5 | Shockwaves, expansion rings |
| **EnigmaEye** | 8 | Enigma watching effects |

**ALSO USE vanilla Dust (DENSE - 2+ per frame!):**
- `DustID.MagicMirror` - scale 1.5f+
- `DustID.Enchanted_Gold` - scale 1.4f+
- `DustID.WhiteTorch` - for contrast

---

## 📋 PROJECTILE CHECKLIST

- [ ] PreDraw has **4+ layered flares** spinning
- [ ] Trail has **dense dust** (2+ per frame, scale 1.5f+)
- [ ] Trail has **contrasting sparkles** (1-in-2)
- [ ] Trail has **flares littering air** (1-in-2)
- [ ] Colors **oscillate** with Main.hslToRgb
- [ ] Music notes **orbit** projectile (scale 0.7f+)
- [ ] Impact is **glimmer cascade**, not puff
- [ ] Lighting is **bright** (1.0f+ intensity)

---

## 🎭 THEME COLORS (STAY CONSISTENT)

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| **La Campanella** | Black smoke | Orange flames | Gold |
| **Eroica** | Scarlet | Crimson | Gold, Sakura |
| **Swan Lake** | White | Black | Rainbow shimmer |
| **Moonlight Sonata** | Dark purple | Light blue | Silver |
| **Enigma** | Black/Purple | Green flame | Void |
| **Fate** | Black void | Pink→Red | White stars, Glyphs |

---

## ✅ BEFORE IMPLEMENTING, ASK:

1. *"Am I just slapping one flare on PreDraw?"* → **ADD 3+ MORE LAYERS**
2. *"Is my dust trail sparse?"* → **2+ PARTICLES PER FRAME**
3. *"Are colors static?"* → **ADD Main.hslToRgb OSCILLATION**
4. *"Do music notes randomly spawn?"* → **MAKE THEM ORBIT**
5. *"Is my impact a puff?"* → **MAKE IT A GLIMMER CASCADE**
6. *"Am I using SwordArc for melee?"* → **YES YOU SHOULD BE**

