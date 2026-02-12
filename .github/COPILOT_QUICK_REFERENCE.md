# MagnumOpus Quick Reference — Calamity-Grounded VFX Design

> **BURN THESE RULES INTO YOUR MEMORY. NO EXCEPTIONS.**
>
> **🔥 FULL GUIDE**: See **[copilot-instructions.md](copilot-instructions.md)** for detailed examples.

---

## 🚨 ARCHITECTURE: PER-WEAPON VFX (Global Systems Disabled)

```csharp
// Common/Systems/VFX/VFXMasterToggle.cs
public static bool GlobalSystemsEnabled = false;  // DISABLED — use per-weapon VFX
public static bool ScreenShadersEnabled = true;
public static bool SkyEffectsEnabled = true;
public static bool ParticleRenderingEnabled = true;
```

**Each weapon, projectile, and boss implements its OWN unique VFX directly in its .cs file.**
Utility classes (BloomRenderer, EnhancedTrailRenderer, etc.) are still available as libraries.

---

## ⭐ GOLD STANDARD = CALAMITY MOD SOURCE CODE

> **Study Calamity's source. These weapons are the benchmark, NOT our own production weapons.**

| Calamity Weapon | Why It Matters | Study For |
|-----------------|----------------|-----------|
| **Exoblade** | Held-projectile swing, 4-phase combo, `CurveSegment` easing | Melee swing architecture |
| **Ark of the Cosmos** | Constellation trails, cosmic cloud particles, curved bezier paths | Trail rendering, curved motion |
| **Galaxia** | Mode-switching combos, per-mode color palettes, sub-projectile spawns | Combo variety, palette systems |
| **Photoviscerator** | Metaball rendering, multi-pass bloom, `{ A = 0 }` alpha removal | Bloom stacking, additive blending |
| **Profaned Guardians** | Multi-entity coordination, phase state machines, arena VFX | Boss design, coordinated attacks |
| **The Oracle** | Primitive trail shaders, width/color functions, shader-based rendering | HLSL trails, GPU rendering |

---

## 🔬 5 CALAMITY PATTERNS THAT MATTER MOST

### 1. Multi-Layer Bloom Stack (`{ A = 0 }` pattern)
```csharp
Color c = baseColor with { A = 0 }; // CRITICAL: remove alpha for additive
sb.Draw(bloom, pos, null, c * 0.30f, 0f, origin, scale * 2.0f, SpriteEffects.None, 0f); // Outer
sb.Draw(bloom, pos, null, c * 0.50f, 0f, origin, scale * 1.4f, SpriteEffects.None, 0f); // Mid
sb.Draw(bloom, pos, null, c * 0.70f, 0f, origin, scale * 0.9f, SpriteEffects.None, 0f); // Inner
sb.Draw(bloom, pos, null, Color.White with { A = 0 } * 0.85f, 0f, origin, scale * 0.4f, SpriteEffects.None, 0f); // Core
```

### 2. 3-Pass Trail Rendering
```csharp
var settings = new EnhancedTrailRenderer.PrimitiveSettings(
    width: progress => baseWidth * (1f - progress),           // Taper
    color: progress => Color.Lerp(startColor, endColor, progress) with { A = 0 },
    smoothen: true
);
EnhancedTrailRenderer.RenderMultiPassTrail(oldPositions, oldRotations, settings, passes: 3);
```

### 3. CurveSegment Piecewise Animation (Swing Arcs)
```csharp
new CurveSegment(EasingType.PolyOut, 0f, -1f, 0.25f, 2),     // Windup (breath)
new CurveSegment(EasingType.PolyIn, 0.25f, -0.75f, 1.65f, 3), // Main swing (note)
new CurveSegment(EasingType.PolyOut, 0.85f, 0.9f, 0.1f, 2),   // Follow-through (resonance)
```

### 4. Sub-Pixel Interpolation (144Hz+)
```csharp
float partialTicks = InterpolatedRenderer.PartialTicks;
Vector2 smoothPos = Vector2.Lerp(previousPosition, currentPosition, partialTicks);
```

### 5. Velocity-Based VFX (Stretch + Spin)
```csharp
float speed = Projectile.velocity.Length();
float stretch = 1f + speed * 0.02f;   // Stretch with speed
float spin = speed * 0.05f;            // Faster = more spin
```

---

## 🎵 THIS IS A MUSIC MOD

- Music notes **MUST** be visible: scale **0.7f–1.2f** (never 0.25f)
- Notes **orbit** projectiles, not spawn randomly
- Multi-layer bloom on every note
- Every combo phase is a **movement in a symphony**
- 6-color palette = **musical dynamics** (pianissimo → sforzando)

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

- [ ] PreDraw has **4+ layered bloom layers** (Calamity bloom stack pattern)
- [ ] Trail uses **EnhancedTrailRenderer** or **CalamityStyleTrailRenderer**
- [ ] Trail has **dense dust** (2+ per frame, scale 1.5f+)
- [ ] Colors use **palette gradient** via `MagnumThemePalettes`
- [ ] Music notes **orbit** projectile (scale 0.7f+)
- [ ] Impact uses **multi-layer bloom cascade**, not single puff
- [ ] Lighting is **bright** (1.0f+ intensity)
- [ ] Uses `{ A = 0 }` for all additive blending

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

1. *"Am I using the `{ A = 0 }` pattern for bloom?"* → **YES, ALWAYS**
2. *"Am I using 4-layer bloom stacks like Calamity?"* → **YES**
3. *"Are my trails using EnhancedTrailRenderer multi-pass?"* → **3 PASSES MINIMUM**
4. *"Do music notes randomly spawn?"* → **MAKE THEM ORBIT**
5. *"Is my impact a puff?"* → **MAKE IT A BLOOM CASCADE**
6. *"Am I studying Calamity's source for reference?"* → **YES YOU SHOULD BE**

---

## 🗡️ MELEE SWING ARCHITECTURE

**ALL melee weapons use held-projectile pattern. NEVER use vanilla `useStyle = Swing`.**
Every swing is a measure of music — each combo phase a different note in the melody.

### 3-File Structure (MANDATORY)
```
WeaponNameItem.cs   → ModItem (channel=true, noMelee=true, noUseGraphic=true)
WeaponNameSwing.cs  → ModProjectile (IS the swing, draws blade+trail+smear+flare)
WeaponNameVFX.cs    → Static helper (optional: palette, trail funcs, particle logic)
```

### Copy From Test Weapons
```
Content/TestWeapons/01_InfernalCleaver/  → Fire (TrailStyle.Flame)
Content/TestWeapons/02_FrostbiteEdge/    → Ice (TrailStyle.Ice)
Content/TestWeapons/03_CosmicRendBlade/  → Cosmic (TrailStyle.Cosmic)
Content/TestWeapons/04_VerdantCrescendo/ → Nature (TrailStyle.Nature)
Content/TestWeapons/05_ArcaneHarmonics/  → Arcane (EnhancedTrailRenderer)
```

### Combo Phase Requirements
- **4+ phases** with different CurveSegment arrays (each phase = a movement in the weapon's symphony)
- **Phase 2**: Spawn sub-projectiles at ~70% progress (blade tip)
- **Phase 3**: Finisher effects at ~85% (screen shake, ground effects, music note cascade)
- **Each phase**: Different smear type, scaling damage/intensity
- **Music notes** scattered from blade tip during swings (scale 0.7f+)

### PreDraw Order (ALWAYS — like reading a score)
```
1. Trail (CalamityStyleTrailRenderer.DrawTrailWithBloom) — the sustained harmony
2. Smear overlay (3-layer additive) — the crescendo blur
3. Blade sprite (normal + additive glow pass) — the melody
4. Lens flare (3-layer at tip using Extra[98]) — the staccato accent
```

### Key Systems
| System | File | Purpose |
|--------|------|---------|
| `CurveSegment` + `PiecewiseAnimation` | `Particle.cs` | Swing arc easing |
| `CalamityStyleTrailRenderer` | `Trails/CalamityStyleTrailRenderer.cs` | 5 shader trail styles |
| `EnhancedTrailRenderer` | `Trails/EnhancedTrailRenderer.cs` | Multi-pass primitive trails |
| `SwingShaderSystem` | `VFX/SwingShaderSystem.cs` | `BeginAdditive()`, `RestoreSpriteBatch()` |
| `SmearTextureGenerator` | `Particles/SmearParticles.cs` | Procedural fallback textures |

### 6-Color Palette (EVERY melee weapon needs one — its musical scale)
```csharp
Color[] Palette = { DarkShadow, DarkMid, Primary, BrightMid, Bright, WhiteHot };
//                  Pianissimo   Piano   Mezzo    Forte   Fortissimo  Sforzando
// Interpolate: GetPaletteColor(t) where t = 0 (dark) to 1 (white-hot)
```

### Standard Palettes
| Element | [0] Dark | [2] Primary | [5] Brightest |
|---------|----------|-------------|---------------|
| 🔥 Fire | (80,10,0) | (255,120,20) | (255,250,220) |
| ❄️ Ice | (20,40,80) | (80,160,220) | (240,250,255) |
| 🌌 Cosmic | (40,10,60) | (160,60,200) | (255,220,255) |
| 🌿 Nature | (20,60,10) | (60,180,70) | (230,250,180) |
| 🎵 Arcane | (40,10,80) | (140,60,210) | (250,220,255) |

### ✅ BEFORE CREATING MELEE WEAPON, ASK:
1. *"Am I using the held-projectile pattern?"* → **YES, ALWAYS**
2. *"Do I have 4+ combo phases?"* → **MINIMUM (like a 4-movement sonata)**
3. *"Does each phase have different smear type?"* → **YES**
4. *"Am I spawning sub-projectiles in Phase 2?"* → **AT ~70% PROGRESS**
5. *"Does Phase 3 have screen shake + finisher?"* → **AT ~85% PROGRESS**
6. *"Am I drawing trail → smear → blade → flare in order?"* → **ALWAYS**
7. *"Are music notes visible from the blade tip?"* → **SCALE 0.7f+, YES**
8. *"Does this weapon sound like a unique musical composition?"* → **EVERY WEAPON IS A DIFFERENT SONG**

