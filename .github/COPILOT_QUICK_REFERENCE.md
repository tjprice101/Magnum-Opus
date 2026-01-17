# MagnumOpus AI Quick Reference

> **Purpose**: Condensed instructions for AI assistants working on MagnumOpus - a music-themed Terraria tModLoader mod.

---

## 🎵 CORE PHILOSOPHY (Read First)

**MagnumOpus is a music mod.** Every visual effect should make players *feel* the music:
- **Music notes in trails, impacts, and auras** - This is the signature look
- **Each theme is UNIQUE** - Never copy effects between themes
- **Gradient color fading** - Always fade between theme colors, never single-color
- **Layered, dramatic VFX** - Dense particles (30+ on impacts, 50+ on explosions)
- **No vanilla sprites** - All projectiles need custom visuals or particle-only rendering

---

## 📁 PROJECT STRUCTURE

```
MagnumOpus/
├── Assets/
│   ├── Particles/         # Particle textures (white/grayscale, tinted at runtime)
│   └── Music/             # Theme music files
├── Common/
│   ├── Systems/
│   │   ├── Particles/     # Core particle system
│   │   ├── ThemedParticles.cs    # Theme-specific particle effects
│   │   ├── UnifiedVFX.cs         # PREFERRED VFX API
│   │   ├── MagnumVFX.cs          # Lightning, beams, utilities
│   │   └── [Theme]SkyEffect.cs   # Sky flash effects
│   └── [Theme]Rarity.cs   # Custom rarities per theme
├── Content/
│   └── [ThemeName]/       # Content organized by theme
│       ├── Bosses/
│       ├── ResonantWeapons/
│       ├── ResonantOres/
│       ├── HarmonicCores/
│       └── Accessories/
├── Documentation/
│   ├── Design Documents for Inspiration/  # Calamity reference docs
│   └── Guides/            # Implementation guides
├── Localization/
│   └── en-US_Mods.MagnumOpus.hjson
└── Midjourney Prompts/    # AI art generation prompts
```

---

## 🎨 THEME COLOR PALETTES

| Theme | Primary → Secondary | Unique Elements |
|-------|---------------------|-----------------|
| **La Campanella** | Black (20,15,20) → Orange (255,100,0) | Heavy smoke, bell chimes, fire runes |
| **Eroica** | Scarlet (139,0,0) → Gold (255,215,0) | Sakura petals, rising embers |
| **Swan Lake** | White ↔ Black + Rainbow shimmer | Feathers, prismatic edges |
| **Moonlight Sonata** | Purple (75,0,130) → Light Blue (135,206,250) | Lunar halos, silver mist |
| **Enigma Variations** | Black → Purple → Green (50,220,100) | Watching eyes, glyphs, void swirls |
| **Fate** | Black (15,5,20) → Pink (180,50,100) → Red (255,60,80) | **CELESTIAL**: Ancient glyphs, star particles, cosmic clouds (Ark of the Cosmos style), reality distortions |

---

## ⭐ FATE CELESTIAL COSMIC REQUIREMENTS (MANDATORY)

**ALL Fate content MUST include these elements:**

| Element | Required | What to Use |
|---------|----------|-------------|
| **Ancient Glyphs** | ✅ MANDATORY | `CustomParticles.Glyph`, `GlyphBurst`, `GlyphCircle`, `GlyphOrbit` |
| **Star Particles** | ✅ MANDATORY | White/gold sparkles, twinkles, constellation points |
| **Cosmic Clouds** | ✅ MANDATORY | Billowing nebula trails (Ark of the Cosmos style) |
| **Dark Prismatic** | ✅ MANDATORY | Black → Pink → Red gradient with white star highlights |
| **Screen Distortions** | ✅ Major attacks | Chromatic aberration, screen slice, reality shatter |
| **Constellation Patterns** | ⚡ Recommended | Connect star points with faint lines on big effects |

```csharp
// FATE EFFECT TEMPLATE - Use this pattern for ALL Fate content
public override void OnHitNPC(NPC target, ...)
{
    // 1. Core flares with Fate gradient
    CustomParticles.GenericFlare(target.Center, FateWhite, 1.0f, 25);
    CustomParticles.GenericFlare(target.Center, FateDarkPink, 0.8f, 22);
    
    // 2. GLYPHS - MANDATORY for Fate
    CustomParticles.GlyphBurst(target.Center, FatePurple, 6, 5f);
    
    // 3. STAR PARTICLES - MANDATORY celestial sparkle
    for (int i = 0; i < 8; i++)
    {
        Vector2 starOffset = Main.rand.NextVector2Circular(30f, 30f);
        CustomParticles.GenericFlare(target.Center + starOffset, FateWhite, 0.3f, 18);
    }
    
    // 4. COSMIC CLOUD BURST - Ark of the Cosmos style
    for (int i = 0; i < 12; i++)
    {
        float angle = MathHelper.TwoPi * i / 12f;
        Vector2 cloudVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
        Color cloudColor = Color.Lerp(FateBlack, FatePurple, Main.rand.NextFloat());
        var cloud = new GenericGlowParticle(target.Center, cloudVel, cloudColor * 0.5f, 0.5f, 30, true);
        MagnumParticleHandler.SpawnParticle(cloud);
    }
    
    // 5. Halos and rings
    CustomParticles.HaloRing(target.Center, FateDarkPink, 0.6f, 20);
}
```

---

## 🔧 VFX QUICK REFERENCE

### UnifiedVFX (PREFERRED API)
```csharp
using MagnumOpus.Common.Systems;

// Theme-specific effects
UnifiedVFX.[Theme].Impact(position, scale);
UnifiedVFX.[Theme].Explosion(position, scale);
UnifiedVFX.[Theme].DeathExplosion(position, scale);
UnifiedVFX.[Theme].Trail(position, velocity, scale);
UnifiedVFX.[Theme].Aura(position, radius, scale);

// Generic utilities
UnifiedVFX.Generic.FractalBurst(pos, primary, secondary, points, radius, scale);
```

### CustomParticles (Low-Level)
```csharp
CustomParticles.GenericFlare(position, color, scale, lifetime);
CustomParticles.HaloRing(position, color, scale, lifetime);
CustomParticles.ExplosionBurst(position, color, count, speed);
CustomParticles.GlyphBurst(position, color, count, speed);
```

### Gradient Pattern (MANDATORY)
```csharp
// Always use gradients, never single colors
for (int i = 0; i < count; i++)
{
    float progress = (float)i / count;
    Color gradientColor = Color.Lerp(primaryColor, secondaryColor, progress);
    CustomParticles.GenericFlare(pos, gradientColor, scale, lifetime);
}
```

---

## ✅ IMPLEMENTATION RULES

### DO:
- ✅ Use `UnifiedVFX.[Theme]` for all effects
- ✅ Include music notes in trails/impacts where thematic
- ✅ Layer multiple effect types (flares + halos + particles + glyphs)
- ✅ Use gradient color fading between theme colors
- ✅ Add 30+ particles on impacts, 50+ on explosions
- ✅ Create custom textures OR particle-only projectiles
- ✅ Use vanilla-style tooltips (sentence case, concise)
- ✅ **For Fate: ALWAYS include glyphs, star particles, and cosmic clouds**

### DON'T:
- ❌ Use vanilla projectile textures
- ❌ Copy effects between themes
- ❌ Use single-color explosions
- ❌ Use ALL CAPS in tooltips
- ❌ Add screen shake on normal hits (only charged attacks, boss phases)
- ❌ Create sparse, minimal effects
- ❌ **NEVER create flat concentric ring/disc projectile TEXTURES** (astrological rings, magic circle textures, spinning nested ring sprites) - THE TEXTURE STYLE IS BANNED, not glyph/halo particle effects
- ❌ **For Fate: NEVER skip glyphs, stars, or cosmic cloud effects**

---

## 📄 ASSET PLACEMENT

| Asset Type | Location |
|------------|----------|
| Particle textures | `Assets/Particles/` |
| Item/Projectile textures | Same folder as `.cs` file |
| Music files | `Assets/Music/` |
| Buff icons | Same folder as buff `.cs` |

### Missing Texture Fallback
```csharp
// Use placeholder if texture doesn't exist
public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
// Or for invisible projectiles with particle-only visuals:
public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
```

---

## 🎯 EFFECT MINIMUMS BY TYPE

| Effect Type | Min Particles | Required Elements |
|-------------|---------------|-------------------|
| Weapon Swing | 15+ | Flares, gradient trail, music notes |
| Projectile Trail | 5+/frame | Glow core, fading trail, theme particles |
| Impact/Hit | 30+ | Flares, halos, fractal burst, sparks |
| Explosion | 50+ | Multi-phase, layered rings, smoke, glyphs |
| Boss Death | 80+ | Maximum spectacle, sky flash, screen shake |

---

## 🔗 KEY FILES

| Purpose | File Path |
|---------|-----------|
| VFX API | `Common/Systems/UnifiedVFX.cs` |
| Themed Particles | `Common/Systems/ThemedParticles.cs` |
| Particle Handler | `Common/Systems/Particles/MagnumParticleHandler.cs` |
| Melee Swing System | `Common/Systems/MagnumMeleeSwingSystem.cs` |
| Sky Effects | `Common/Systems/[Theme]SkyEffect.cs` |
| Boss Utilities | `Common/Systems/BossAIUtilities.cs` |
| Localization | `Localization/en-US_Mods.MagnumOpus.hjson` |
| Progression | `Documentation/Mod_Progression.txt` |

---

## 🎼 THEME PROGRESSION ORDER

```
Moon Lord → Moonlight Sonata → Eroica → La Campanella → Enigma Variations
    → Swan Lake → Fate → Ode to Joy → Dies Irae → Winter → Nachtmusik
    → Clair de Lune → Mercury → Mars
```

---

## 💡 QUICK PATTERNS

### Standard Impact Effect
```csharp
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    // Use themed explosion
    UnifiedVFX.[Theme].Impact(target.Center, 1.2f);
    
    // Add fractal burst
    for (int i = 0; i < 6; i++)
    {
        float angle = MathHelper.TwoPi * i / 6f;
        float progress = (float)i / 6f;
        Color color = Color.Lerp(primaryColor, secondaryColor, progress);
        CustomParticles.GenericFlare(target.Center + angle.ToRotationVector2() * 30f, color, 0.4f, 18);
    }
    
    // Add lighting
    Lighting.AddLight(target.Center, primaryColor.ToVector3() * 1.5f);
}
```

### Projectile Trail (Every 3-4 frames)
```csharp
public override void AI()
{
    if (Projectile.timeLeft % 4 == 0)
    {
        UnifiedVFX.[Theme].Trail(Projectile.Center, Projectile.velocity, 0.5f);
        
        if (Main.rand.NextBool(3))
            ThemedParticles.[Theme]MusicNotes(Projectile.Center, 1, 15f);
    }
}
```

### Particle-Only Projectile (No Sprite)
```csharp
public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

public override bool PreDraw(ref Color lightColor)
{
    // Draw layered glows as the "projectile"
    var tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
    var pos = Projectile.Center - Main.screenPosition;
    Main.spriteBatch.Draw(tex, pos, null, primaryColor * 0.6f, 0f, tex.Size()/2, 0.8f, 0, 0);
    Main.spriteBatch.Draw(tex, pos, null, Color.White * 0.4f, 0f, tex.Size()/2, 0.4f, 0, 0);
    return false;
}
```

---

## 📚 DETAILED DOCUMENTATION

For full details, see:
- [VFX_PARTICLE_SYSTEM_GUIDE.md](../Documentation/Guides/VFX_PARTICLE_SYSTEM_GUIDE.md) - Complete particle API
- [Mod_Progression.txt](../Documentation/Mod_Progression.txt) - Full progression chain
- [Design Documents for Inspiration/](../Documentation/Design%20Documents%20for%20Inspiration/) - Calamity reference implementations
