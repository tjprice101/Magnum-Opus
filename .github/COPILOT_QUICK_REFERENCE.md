# MagnumOpus Quick Reference - VFX Design Rules

> **BURN THESE RULES INTO YOUR MEMORY. NO EXCEPTIONS.**

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

## 🎵 MUSIC NOTES MUST BE VISIBLE

| ❌ WRONG | ✅ CORRECT |
|----------|-----------|
| Scale 0.25f-0.4f | Scale 0.7f-1.2f |
| No bloom | Multi-layer bloom |
| Static | Shimmer/pulse animation |
| Alone | With sparkle companions |

```csharp
// ✅ CORRECT - Visible, glowing music notes
float scale = Main.rand.NextFloat(0.75f, 1.0f);
int variant = Main.rand.Next(1, 7); // Use ALL 6 variants
float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
ThemedParticles.MusicNote(pos, vel, color, scale * shimmer, 35, variant);
```

---

## 🎨 USE ALL PARTICLE ASSETS

**You have 80+ custom PNGs. USE THEM.**

| Category | Variants | USE FOR |
|----------|----------|---------|
| **MusicNote** | 6 | EVERY trail, impact, aura |
| **EnergyFlare** | 7 | Impacts, projectile cores |
| **PrismaticSparkle** | 15 | Sparkle accents EVERYWHERE |
| **Glyphs** | 12 | Magic circles, Fate theme |
| **SwanFeather** | 10 | Swan Lake theme |
| **EnigmaEye** | 8 | Enigma watching effects |
| **MagicSparkleField** | 12 | Magic trails, auras |
| **SwordArc** | 9 | Melee swing effects |
| **GlowingHalo** | 5 | Shockwaves, impacts |
| **SoftGlow** | 3 | Bloom bases, ambient |

**ALSO USE vanilla Dust:**
- `DustID.MagicMirror` - Magic shimmer
- `DustID.Enchanted_Gold` - Golden sparkles
- `DustID.PurpleTorch` - Purple flames
- `DustID.Electric` - Electric sparks
- `DustID.GemAmethyst/Sapphire/Ruby` - Gem sparkles

---

## 📋 EFFECT REQUIREMENTS

### Every Weapon Effect MUST Have:

1. **UNIQUE PROJECTILE/SWING** - Different from all others
2. **UNIQUE TRAIL** - Music notes, sparkles, theme particles
3. **UNIQUE IMPACT** - Layered explosion with multiple phases
4. **UNIQUE SPECIAL** - Combo, charge, mark, or mechanic

### Every Effect MUST Combine:

1. At least **2 custom particle types**
2. At least **1 vanilla Dust type**
3. At least **1 music-related particle** (where thematic)
4. **Multi-layer bloom**

---

## 🎭 THEME COLORS (STAY CONSISTENT)

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| **La Campanella** | Black smoke | Orange flames | Gold |
| **Eroica** | Scarlet | Crimson | Gold, Sakura pink |
| **Swan Lake** | White | Black | Rainbow shimmer |
| **Moonlight Sonata** | Dark purple | Light blue | Silver |
| **Enigma** | Black/Purple | Green flame | Void |
| **Fate** | Black void | Pink→Red | White stars, Glyphs |

---

## ✅ BEFORE IMPLEMENTING, ASK:

1. *"Is there ANY other weapon that does something similar?"* → If yes, REDESIGN.
2. *"Can players SEE the music notes?"* → If scale < 0.7f, INCREASE.
3. *"Am I using 3-4+ particle types?"* → If not, ADD MORE.
4. *"Would players REMEMBER this effect?"* → If generic, REDESIGN.

---

## 🚨 QUICK CHECKS

- [ ] Music notes scale ≥ 0.7f
- [ ] Using multiple particle types
- [ ] Using vanilla dust for density
- [ ] Effect is UNIQUE from all others
- [ ] Bloom layers for glow
- [ ] Theme colors consistent
