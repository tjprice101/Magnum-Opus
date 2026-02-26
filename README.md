# MagnumOpus Documentation Index

> **Quick navigation to all documentation, guides, and reference materials.**

---

## 📋 QUICK START

| Need to... | Go to... |
|------------|----------|
| **Understand the mod philosophy** | [COPILOT_QUICK_REFERENCE.md](.github/COPILOT_QUICK_REFERENCE.md) |
| **Find VFX/particle methods** | [VFX_PARTICLE_SYSTEM_GUIDE.md](Documentation/Guides/VFX_PARTICLE_SYSTEM_GUIDE.md) |
| **Understand progression order** | [Mod_Progression.txt](Documentation/Mod_Progression.txt) |
| **Generate new art assets** | [MASTER_PROMPTS.md](Midjourney%20Prompts/MASTER_PROMPTS.md) |
| **Implement a boss** | [Boss Design References](#boss-design-references) |

---

## 📁 DIRECTORY STRUCTURE

```
MagnumOpus/
├── .github/
━E  ├── copilot-instructions.md      # Full AI instructions (detailed)
━E  └── COPILOT_QUICK_REFERENCE.md   # ⭁ECONDENSED AI instructions
━E
├── Assets/
━E  ├── Particles/                    # White/grayscale particle textures
━E  └── Music/                        # Theme music files
━E
├── Common/
━E  ├── Systems/
━E  ━E  ├── Particles/               # Core particle system
━E  ━E  ━E  ├── MagnumParticleHandler.cs  # Particle spawning
━E  ━E  ━E  ├── CommonParticles.cs        # Particle classes
━E  ━E  ━E  └── Particle.cs               # Base particle class
━E  ━E  ├── UnifiedVFX.cs            # ⭁EPREFERRED VFX API
━E  ━E  ├── ThemedParticles.cs       # Theme-specific effects
━E  ━E  ├── MagnumVFX.cs             # Lightning, beams, utilities
━E  ━E  ├── MagnumMeleeSwingSystem.cs # Melee weapon rotation
━E  ━E  ├── BossAIUtilities.cs       # Boss helper methods
━E  ━E  └── [Theme]SkyEffect.cs      # Sky flash effects
━E  └── [Theme]Rarity.cs             # Custom item rarities
━E
├── Content/
━E  └── [ThemeName]/                 # Organized by theme
━E      ├── Bosses/
━E      ├── ResonantWeapons/
━E      ├── ResonantOres/
━E      ├── HarmonicCores/
━E      ├── Accessories/
━E      └── SummonItems/
━E
├── Documentation/
━E  ├── Mod_Progression.txt          # Full progression chain
━E  ├── Guides/
━E  ━E  ├── VFX_PARTICLE_SYSTEM_GUIDE.md      # Complete particle API
━E  ━E  ├── PRISMATIC_GEM_EFFECT_GUIDE.txt    # Gem effect patterns
━E  ━E  └── INFERNUM_VFX_AND_BOSS_AI_REFERENCE.md
━E  ├── Design Documents for Inspiration/     # Calamity references
━E  └── AI Prompts/                  # Suno AI music prompts
━E
├── Localization/
━E  └── en-US_Mods.MagnumOpus.hjson  # All item/NPC text
━E
└── Midjourney Prompts/
    ├── MASTER_PROMPTS.md            # ⭁ECONSOLIDATED prompt library
    └── [Individual files]           # Legacy individual prompts
```

---

## 🎨 VFX & PARTICLES

### Primary References
| File | Purpose |
|------|---------|
| [UnifiedVFX.cs](Common/Systems/UnifiedVFX.cs) | **PREFERRED** - High-level theme effects |
| [ThemedParticles.cs](Common/Systems/ThemedParticles.cs) | Theme-specific particle methods |
| [VFX_PARTICLE_SYSTEM_GUIDE.md](Documentation/Guides/VFX_PARTICLE_SYSTEM_GUIDE.md) | Complete API documentation |

### Quick API Reference
```csharp
// Preferred: UnifiedVFX
UnifiedVFX.[Theme].Impact(position, scale);
UnifiedVFX.[Theme].Explosion(position, scale);
UnifiedVFX.[Theme].DeathExplosion(position, scale);
UnifiedVFX.[Theme].Trail(position, velocity, scale);
UnifiedVFX.Generic.FractalBurst(pos, primary, secondary, points, radius, scale);

// Low-level: CustomParticles
CustomParticles.GenericFlare(position, color, scale, lifetime);
CustomParticles.HaloRing(position, color, scale, lifetime);
CustomParticles.ExplosionBurst(position, color, count, speed);
```

---

## 🎼 THEME REFERENCE

### Progression Order
```
Moon Lord ↁEMoonlight Sonata ↁEEroica ↁELa Campanella ↁEEnigma Variations
    ↁESwan Lake ↁEFate ↁEOde to Joy ↁEDies Irae ↁEWinter ↁENachtmusik
    ↁEClair de Lune ↁEMercury ↁEMars
```

### Theme Color Quick Reference
| Theme | Primary RGB | Secondary RGB | Key Visual |
|-------|-------------|---------------|------------|
| Moonlight Sonata | (75,0,130) Purple | (135,206,250) Blue | Lunar, mist |
| Eroica | (139,0,0) Scarlet | (255,215,0) Gold | Sakura, heroic |
| La Campanella | (20,15,20) Black | (255,100,0) Orange | Smoke, fire, bells |
| Enigma | (15,10,20) Black | (140,60,200) Purple ↁE(50,220,100) Green | Eyes, glyphs, void |
| Swan Lake | (255,255,255) White | (20,20,30) Black | Feathers, rainbow |
| Fate | (15,5,20) Black | (180,50,100) Pink ↁE(255,60,80) Red | Reality tears, cosmic |

### Theme Content Locations
| Theme | Content Path |
|-------|--------------|
| Moonlight Sonata | `Content/MoonlightSonata/` |
| Eroica | `Content/Eroica/` |
| La Campanella | `Content/LaCampanella/` |
| Enigma Variations | `Content/EnigmaVariations/` |
| Swan Lake | `Content/SwanLake/` |
| Fate | `Content/Fate/` |
| Dies Irae | `Content/DiesIrae/` |
| Clair de Lune | `Content/ClairDeLune/` |

---

## 🐉 BOSS DESIGN REFERENCES

### Calamity-Inspired Design Documents
| Document | Best For | Key Patterns |
|----------|----------|--------------|
| [Devourer_of_Gods_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Devourer_of_Gods_Design.md) | Worm bosses | Segment linking, portal attacks, laser walls |
| [Yharon_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Yharon_Design.md) | Flying bosses | Dual AI, attack state machine, enrage arena |
| [Exomech_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Exomech_Design.md) | Multi-entity fights | Mech coordination, berserk mode, phase states |
| [Calamity_Inspired_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Calamity_Inspired_VFX_Design.md) | Any VFX | Lasers, trails, smears, bloom |
| [Exo_Weapons_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Exo_Weapons_VFX_Design.md) | Weapon effects | Swing mechanics, combos, homing |

### VFX-Specific Design Documents
| Document | Focus |
|----------|-------|
| [AstrumDeus_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/AstrumDeus_VFX_Design.md) | Cosmic/constellation effects |
| [Yharon_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Yharon_VFX_Design.md) | Fire/infernal effects |
| [Exomech_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/Exomech_VFX_Design.md) | Technological/laser effects |
| [ProfanedGuardian_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/ProfanedGuardian_VFX_Design.md) | Holy/fire beams |
| [SupremeCalamitas_VFX_Design.md](Documentation/Design%20Documents%20for%20Inspiration/SupremeCalamitas_VFX_Design.md) | Brimstone/demonic effects |

---

## 🎨 ART ASSET GENERATION

### Midjourney Prompts
| File | Contents |
|------|----------|
| [MASTER_PROMPTS.md](Midjourney%20Prompts/MASTER_PROMPTS.md) | **⭁ECONSOLIDATED** - All prompts organized by category |
| Individual theme files | Legacy prompts (kept for reference) |

### Asset Placement Rules
| Asset Type | Destination |
|------------|-------------|
| Particle textures (.png) | `Assets/Particles Asset Library/` |
| Item textures | Same folder as item `.cs` file |
| Projectile textures | Same folder as projectile `.cs` file |
| Boss spritesheets | `Content/[Theme]/Bosses/` |
| Music files | `Assets/Music/` |

---

## 🔧 KEY SYSTEM FILES

### Particle System
| File | Purpose |
|------|---------|
| [MagnumParticleHandler.cs](Common/Systems/Particles/MagnumParticleHandler.cs) | Spawns and manages particles |
| [CommonParticles.cs](Common/Systems/Particles/CommonParticles.cs) | Particle class definitions |
| [ParticleTextureGenerator.cs](Common/Systems/Particles/ParticleTextureGenerator.cs) | Runtime texture generation |

### Visual Effects
| File | Purpose |
|------|---------|
| [UnifiedVFX.cs](Common/Systems/UnifiedVFX.cs) | Theme-based VFX API |
| [ThemedParticles.cs](Common/Systems/ThemedParticles.cs) | Theme particles |
| [MagnumVFX.cs](Common/Systems/MagnumVFX.cs) | Lightning, beams |
| [PrimitiveTrailRenderer.cs](Common/Systems/PrimitiveTrailRenderer.cs) | GPU trails |
| [MeleeSmearEffect.cs](Common/Systems/MeleeSmearEffect.cs) | Weapon swing trails |

### Boss Systems
| File | Purpose |
|------|---------|
| [BossAIUtilities.cs](Common/Systems/BossAIUtilities.cs) | Helper methods for boss AI |
| [BossHealthBarUI.cs](Common/Systems/BossHealthBarUI.cs) | Custom boss health bars |
| [[Theme]SkyEffect.cs](Common/Systems/) | Sky flash effects |

### Weapon Systems
| File | Purpose |
|------|---------|
| [MagnumMeleeSwingSystem.cs](Common/Systems/MagnumMeleeSwingSystem.cs) | 360° melee rotation |
| [ChargedMeleeSystem.cs](Common/Systems/ChargedMeleeSystem.cs) | Charged attack handling |

---

## 📝 LOCALIZATION

All item names, descriptions, and tooltips are in:
- [en-US_Mods.MagnumOpus.hjson](Localization/en-US_Mods.MagnumOpus.hjson)

### Tooltip Guidelines
- Use sentence case (not ALL CAPS)
- Keep descriptions concise and informative
- Match vanilla Terraria style
- Lore lines in italics can be more creative

---

## 🎵 AUDIO

### Suno AI Music Prompts
- [SUNO_AI_Prompts.txt](Documentation/AI%20Prompts/SUNO_AI_Prompts.txt)

---

## ⚡ QUICK IMPLEMENTATION PATTERNS

### Standard Weapon Impact
```csharp
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    UnifiedVFX.[Theme].Impact(target.Center, 1.2f);
    Lighting.AddLight(target.Center, themeColor.ToVector3() * 1.5f);
}
```

### Projectile with Trail
```csharp
public override void AI()
{
    if (Projectile.timeLeft % 4 == 0)
        UnifiedVFX.[Theme].Trail(Projectile.Center, Projectile.velocity, 0.5f);
}
```

### Boss Death Animation
```csharp
if (dying && timer == climaxFrame)
{
    UnifiedVFX.[Theme].DeathExplosion(NPC.Center, 2f);
    [Theme]SkyEffect.TriggerFlash(1.5f);
    MagnumScreenEffects.AddScreenShake(20f);
}
```
