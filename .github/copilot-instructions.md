# MagnumOpus Copilot Instructions

> **⚡ QUICK START**: For a condensed version of these instructions, see **[COPILOT_QUICK_REFERENCE.md](COPILOT_QUICK_REFERENCE.md)**.
> 
> This full document contains detailed examples and explanations. The quick reference is recommended for faster lookups.

---

## 🎵 THE HEART OF MAGNUM OPUS - DESIGN PHILOSOPHY

> *"This mod is based around music. It's based around how it connects to your heart, and it's based around how it impacts the world."*

### The Soul of This Modpack

MagnumOpus is not just a content mod—it is **a symphony made playable**. Every weapon, every effect, every particle should make players *feel* the music. When a sword swings, players should see **trails of music notes dancing in the blade's wake**. When a gun fires, projectiles should leave **lingering musical echoes in the air** before fading like the final note of a crescendo.

### Core Commandments

1. **UNIQUENESS ABOVE ALL** - Each score (Moonlight Sonata, Eroica, La Campanella, etc.) must feel **vastly different** from one another. Never reuse effects across themes. Each weapon deserves its own identity.

2. **PLAYER-DRIVEN AWE** - Effects should be **powerful, awe-inspiring, and make players feel like conductors of destruction**. Players should look at their weapons and think *"This is beautiful."*

3. **MUSIC NOTES EVERYWHERE** - This is a music mod! Trails, impacts, auras—**weave music notes and musical symbols into everything**. A melee swing should scatter notes. A projectile should leave a melodic trail. An explosion should burst with symphonic energy.

4. **DYNAMIC, LIVING COLORS** - No flat, static colors. Effects should **breathe, pulse, shimmer, and transition**. Use gradients that flow from one hue to another. Make colors feel alive.

5. **EMBRACE THE SCORE'S SOUL** - Each theme has a story:
   - **Moonlight Sonata** → The moon's ethereal glow, soft purple mist, silver light
   - **Eroica** → Heroic triumph, fragile sakura petals, golden-tinged scarlet embers
   - **La Campanella** → The flaming bell of music, infernal chimes, smoke and fire
   - **Swan Lake** → Graceful elegance, feathers drifting, monochrome with prismatic edges
   - **Fate** → CELESTIAL cosmic power, ancient glyphs orbiting, star particles streaming, cosmic clouds billowing like Ark of the Cosmos, reality bending to cosmic will

6. **CREATIVE FREEDOM** - If you want a sword that **slams into the ground and casts waves of symphonic energy**, DO IT. If you want a gun that **fires into the sky and rains musical notes and flame onto enemies**, BE MY GUEST. But above all—**BE UNIQUE**.

### What This Means In Practice

```csharp
// ❌ WRONG - Boring, generic, forgettable
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    CustomParticles.GenericFlare(target.Center, Color.Red, 0.5f, 10);
}

// ✅ RIGHT - A symphony of destruction that tells a story
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    // Central impact - the crescendo
    CustomParticles.GenericFlare(target.Center, EroicaGold, 0.9f, 25);
    
    // Sakura petals scatter like a hero's final stand
    ThemedParticles.SakuraPetals(target.Center, 8, 50f);
    
    // Music notes spiral outward - the melody of victory
    for (int i = 0; i < 6; i++)
    {
        float angle = MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.05f;
        Vector2 notePos = target.Center + angle.ToRotationVector2() * 35f;
        Color noteColor = Color.Lerp(EroicaScarlet, EroicaGold, (float)i / 6f);
        ThemedParticles.MusicNote(notePos, angle.ToRotationVector2() * 2f, noteColor, 0.4f, 25);
    }
    
    // Rising embers - the phoenix ascends
    CustomParticles.ExplosionBurst(target.Center, EroicaCrimson, 12, 6f);
    
    // Golden halo - triumphant radiance
    CustomParticles.HaloRing(target.Center, EroicaGold * 0.8f, 0.6f, 20);
}
```

### The Ultimate Question

Before implementing ANY effect, ask yourself:

> *"Would a player see this and feel inspired? Would they feel the music?"*

If the answer is no—**think harder, dig deeper, and create something magnificent.**

---

## ⛔ ABSOLUTELY FORBIDDEN PROJECTILE TYPES - NEVER CREATE

> **THIS IS A HARD RULE. DO NOT VIOLATE UNDER ANY CIRCUMSTANCES.**

### BANNED: Rotating Concentric Ring/Disc Projectile TEXTURES

**NEVER, EVER create projectile TEXTURES that look like:**
- Large flat disc textures with concentric red/pink rings
- "Astrological ring" style images with eye-like centers
- Spinning disc projectile sprites with nested circle patterns
- Any texture that resembles a magic circle drawn as a flat disc
- Textures that show multiple rings emanating from a center point

**This ban is specifically about the TEXTURE/IMAGE itself, not about glyph particle effects or ring-burst VFX.**

**Why this texture style is banned:**
- These textures are visually obnoxious and clash with the mod's aesthetic
- They obscure gameplay and obstruct the player's view
- They look generic and don't fit the musical theme of MagnumOpus
- The creator specifically hates this visual style

**If you see this projectile texture style in existing code - DELETE IT IMMEDIATELY.**

### BANNED: GlowingHalo3.png Texture (Concentric Rings) - FILE DELETED

**The texture `Assets/Particles/GlowingHalo3.png` HAS BEEN PERMANENTLY DELETED from the mod.**

This texture created concentric expanding ring effects and is forbidden. NEVER:
- Recreate or add back this texture file
- Use `CustomParticleSystem.GlowingHalos[2]` (the index is now invalid)
- Create ANY concentric ring particle textures

**The `RandomHalo()` function excludes index 2 from selection. Only use indices 0, 1, 3, 4, 5.**

```csharp
// ❌ BANNED - GlowingHalo3 has been DELETED
CustomParticleSystem.GlowingHalos[2]  // WILL CAUSE ERRORS - file deleted

// ✅ ALLOWED - Use these halo indices instead
CustomParticleSystem.GlowingHalos[0]  // OK
CustomParticleSystem.GlowingHalos[1]  // OK  
CustomParticleSystem.GlowingHalos[3]  // OK (GlowingHalo4.png)
CustomParticleSystem.GlowingHalos[4]  // OK
CustomParticleSystem.GlowingHalos[5]  // OK
CustomParticleSystem.RandomHalo()     // OK - excludes index 2
```

```csharp
// ❌ ABSOLUTELY FORBIDDEN - NEVER CREATE ANYTHING LIKE THIS
public class FateAstrologicalRing : ModProjectile  // BANNED
{
    // Drawing concentric rings around player as a projectile - FORBIDDEN
    private void DrawAstrologicalRing() { }
    private void DrawFullRing() { }
    private void DrawRingArc() { }
    // Multiple nested rotating circles as a single projectile - BANNED
    for (int ring = 0; ring < InnerRings; ring++) { }
}

// ❌ ANY variation of rotating disc/ring projectile patterns - BANNED
public void DrawConcentricRings() { }  // NO
public void SpawnAstrologicalCircle() { }  // NO
public void CreateMagicCircle() { }  // NO
```

**What IS allowed:**
- ✅ Glyph particles scattered around effects (CustomParticles.Glyph, CustomParticles.GlyphBurst)
- ✅ Halo ring particles that expand and fade (CustomParticles.HaloRing)
- ✅ Explosion burst VFX that emanate outward and dissipate
- ✅ Musical elements (notes, glyphs, themed particles)
- ✅ Effects that flow naturally with movement (trails, waves, arcs)
- ✅ Particles that don't create large stationary visual obstructions

---

## CRITICAL: Asset File Handling - MANDATORY

**Any file given (image, asset, texture, audio, etc.) should be moved to its correct location within the modpack:**

| File Type | Target Location |
|-----------|-----------------|
| Particle textures (.png) | `Assets/Particles/` |
| Music files (.ogg, .mp3) | `Assets/Music/` |
| Item textures (.png) | Same folder as the item's .cs file |
| Projectile textures (.png) | Same folder as the projectile's .cs file |
| Boss spritesheets (.png) | `Content/[Theme]/Bosses/` (named exactly as the boss class) |
| NPC textures (.png) | Same folder as the NPC's .cs file |
| Buff icons (.png) | Same folder as the buff's .cs file |
| Documentation (.txt, .md) | `Documentation/` or `Documentation/Guides/` |
| AI prompts (.txt) | `Documentation/AI Prompts/` |
| Midjourney prompts | `Midjourney Prompts/` |

**Always verify the expected texture path in the code and place the file accordingly.**

---

## 📚 DESIGN DOCUMENTS FOR INSPIRATION

The `Documentation/Design Documents for Inspiration/` folder contains comprehensive reference documents analyzing advanced techniques from Calamity Mod. **Always consult these when implementing complex systems:**

| Document | Contents | When to Use |
|----------|----------|-------------|
| [Calamity_Inspired_VFX_Design.md](../Documentation/Design%20Documents%20for%20Inspiration/Calamity_Inspired_VFX_Design.md) | Laser effects, constellation rendering, melee smears, primitive trails, Profaned Guardian effects | Creating any VFX, trails, or visual systems |
| [Devourer_of_Gods_Design.md](../Documentation/Design%20Documents%20for%20Inspiration/Devourer_of_Gods_Design.md) | Worm segment architecture, laser wall attacks, portal teleportation, phase transitions, Cosmic Guardians | Building multi-segment bosses, geometric attack patterns, phase-based difficulty |
| [Yharon_Design.md](../Documentation/Design%20Documents%20for%20Inspiration/Yharon_Design.md) | Dual AI system, attack state machine, enrage/arena system, fire attacks (charges, fireballs, tornadoes) | Boss attack variety, enrage mechanics, telegraphed attacks |
| [Exomech_Design.md](../Documentation/Design%20Documents%20for%20Inspiration/Exomech_Design.md) | Multi-boss coordination, SecondaryPhase states, berserk mode, arm weapon systems, HP linking | Multi-entity fights, coordinated bosses, complex phase management |
| [Exo_Weapons_VFX_Design.md](../Documentation/Design%20Documents%20for%20Inspiration/Exo_Weapons_VFX_Design.md) | Ark of the Cosmos swing mechanics, Exoblade dash attacks, Photoviscerator metaballs, primitive trail shaders, CurveSegment animation | Advanced weapon VFX, combo systems, homing projectiles, particle layering |

### How to Use These Documents

1. **Before implementing a new boss**: Read Devourer_of_Gods_Design.md for segment architecture, Yharon_Design.md for attack patterns, and Exomech_Design.md for multi-boss coordination
2. **Before creating weapon effects**: Read Exo_Weapons_VFX_Design.md for swing mechanics, trails, and particle layering techniques
3. **Before adding any VFX**: Read Calamity_Inspired_VFX_Design.md for particle systems, shaders, and visual polish
4. **When stuck on implementation**: These documents contain code examples and adaptation tips specifically for MagnumOpus

### Key Concepts from These Documents

- **Piecewise Animation (CurveSegment)**: Complex multi-phase animations for swings and dashes
- **Primitive Trail Rendering**: Shader-based trails using PrimitiveRenderer
- **Phase State Machines**: SecondaryPhase enum for boss behavior modes
- **Multi-Layer Bloom**: Multiple bloom draws at different scales for depth
- **Color Palette Cycling**: Time-based hue shifts for dynamic visuals (adapt Exo Palette to theme colors)
- **Worm Segment Linking**: AI array communication between head/body/tail

---

## 🎨 ADVANCED VFX REFERENCE - FARGOS SOULS DLC ANALYSIS

The `Documentation/Custom Shaders and Shading/` folder contains **comprehensive VFX documentation extracted from FargosSoulsDLC** - one of the most visually impressive Terraria mods. **ALWAYS consult these documents when implementing any visual effects, shaders, particles, or rendering systems.**

| Document | Contents | When to Use |
|----------|----------|-------------|
| [00_FargosSoulsDLC_VFX_Overview.md](../Documentation/Custom%20Shaders%20and%20Shading/00_FargosSoulsDLC_VFX_Overview.md) | Master overview, architecture diagram, quick reference patterns | **START HERE** - Understanding the overall VFX pipeline |
| [01_Primitive_Trail_Rendering.md](../Documentation/Custom%20Shaders%20and%20Shading/01_Primitive_Trail_Rendering.md) | `IPixelatedPrimitiveRenderer`, `PrimitiveSettings`, width/color functions | Laser beams, weapon trails, projectile trails |
| [02_Bloom_And_Glow_Effects.md](../Documentation/Custom%20Shaders%20and%20Shading/02_Bloom_And_Glow_Effects.md) | Multi-layer bloom stacking, shine flares, the `with { A = 0 }` pattern | Any glowing effect, impacts, explosions |
| [03_HLSL_Shader_Reference.md](../Documentation/Custom%20Shaders%20and%20Shading/03_HLSL_Shader_Reference.md) | 40+ shader files with full code: `QuadraticBump`, `PaletteLerp`, pixelation | Custom shaders, advanced rendering |
| [04_ExoMechs_VFX_Analysis.md](../Documentation/Custom%20Shaders%20and%20Shading/04_ExoMechs_VFX_Analysis.md) | Ares (katanas, tesla, portals), Apollo (plasma), Artemis (lasers), Hades (worm, super laser) | Boss VFX, complex attack visuals |
| [05_Particle_Systems.md](../Documentation/Custom%20Shaders%20and%20Shading/05_Particle_Systems.md) | `BloomPixelParticle`, `GlowySquareParticle`, `StrongBloom`, metaballs, FastParticle | All particle implementations |
| [06_Old_Duke_VFX_Analysis.md](../Documentation/Custom%20Shaders%20and%20Shading/06_Old_Duke_VFX_Analysis.md) | Fire particles, bile metaballs, nuclear hurricane, environmental effects | Fire/flame effects, screen filters, environmental VFX |
| [07_Texture_Registries.md](../Documentation/Custom%20Shaders%20and%20Shading/07_Texture_Registries.md) | `MiscTexturesRegistry`, `NoiseTexturesRegistry`, all texture documentation | Texture management, noise textures for shaders |
| [08_Color_And_Gradient_Techniques.md](../Documentation/Custom%20Shaders%20and%20Shading/08_Color_And_Gradient_Techniques.md) | `Color.Lerp` patterns, HLSL gradients, HSL manipulation, theme palettes | Color systems, gradients, palette management |

### MANDATORY: Read Before Implementing VFX

**Before implementing ANY of the following, READ the corresponding documents:**

| Task | Required Reading |
|------|------------------|
| Any glowing/bloom effect | [02_Bloom_And_Glow_Effects.md](../Documentation/Custom%20Shaders%20and%20Shading/02_Bloom_And_Glow_Effects.md) |
| Trail/beam rendering | [01_Primitive_Trail_Rendering.md](../Documentation/Custom%20Shaders%20and%20Shading/01_Primitive_Trail_Rendering.md) |
| Custom particle types | [05_Particle_Systems.md](../Documentation/Custom%20Shaders%20and%20Shading/05_Particle_Systems.md) |
| Boss attack visuals | [04_ExoMechs_VFX_Analysis.md](../Documentation/Custom%20Shaders%20and%20Shading/04_ExoMechs_VFX_Analysis.md) |
| Fire/flame effects | [06_Old_Duke_VFX_Analysis.md](../Documentation/Custom%20Shaders%20and%20Shading/06_Old_Duke_VFX_Analysis.md) |
| Color gradients/palettes | [08_Color_And_Gradient_Techniques.md](../Documentation/Custom%20Shaders%20and%20Shading/08_Color_And_Gradient_Techniques.md) |
| HLSL shaders | [03_HLSL_Shader_Reference.md](../Documentation/Custom%20Shaders%20and%20Shading/03_HLSL_Shader_Reference.md) |

### Critical Patterns from FargosSoulsDLC

These patterns should be used throughout MagnumOpus:

```csharp
// ✅ CORRECT: Remove alpha for additive blending
Color glowColor = baseColor with { A = 0 };
Main.spriteBatch.Draw(bloom, pos, null, glowColor * 0.5f, ...);

// ✅ CORRECT: Multi-layer bloom stack
for (int i = 0; i < 4; i++)
{
    float scale = 1f + i * 0.3f;
    float opacity = 0.5f / (i + 1);
    Main.spriteBatch.Draw(bloom, pos, null, color with { A = 0 } * opacity, 
        0f, bloom.Size() * 0.5f, scale, 0, 0f);
}

// ✅ CORRECT: Palette gradient lerping
public static Color GetThemeColor(Color[] palette, float progress)
{
    float scaledProgress = progress * (palette.Length - 1);
    int startIndex = (int)scaledProgress;
    int endIndex = Math.Min(startIndex + 1, palette.Length - 1);
    return Color.Lerp(palette[startIndex], palette[endIndex], scaledProgress - startIndex);
}
```

### HLSL QuadraticBump - The Universal Edge Fade

```hlsl
// Used in nearly every FargosSoulsDLC shader
float QuadraticBump(float x)
{
    return x * (4 - x * 4);
}
// Input 0.0 → 0.0, Input 0.5 → 1.0 (peak), Input 1.0 → 0.0
// Perfect for edge-to-center intensity in trails and beams
```

---

## Custom Particle System Overview

This mod uses a custom particle system located at `Common/Systems/Particles/`. 

### Core Particle Textures (Assets/Particles/)

| File | Purpose | Usage |
|------|---------|-------|
| `EnergyFlare.png` | Intense, bright burst | Impacts, explosions, boss attacks, dramatic moments |
| `SoftGlow.png` | Subtle ambient glow | Trails, auras, ambient effects, soft lighting |
| `MusicNote.png` | Musical note | Thematic musical effects - perfect for this music-themed mod! |

**All textures are WHITE/GRAYSCALE and get tinted at runtime to any color.**

### Theme Color Tinting Examples:
- **Eroica theme**: Scarlet, Crimson, Gold
- **Moonlight Sonata**: Deep Purple, Violet, Lavender, Silver
- **Swan Lake**: Pure White, Icy Blue, Pale Cyan
- **Dies Irae**: Blood Red, Dark Crimson, Ember
- **Clair de Lune**: Soft Blue, Moonbeam, Pearl
- **Enigma Variations**: Eerie Green, Deep Purple, Black
- **Fate**: White, Dark Pink, Purple, Crimson

---

## Enigma Eyes & Arcane Glyphs - NEW PARTICLE ASSETS

### EnigmaEye Textures (8 variants - Assets/Particles/EnigmaEye1-8.png)

**Mysterious watching eyes for the Enigma theme.** These eyes represent the unknown observing, arcane awareness, and reality questioning itself.

**CRITICAL: MEANINGFUL PLACEMENT ONLY**
- ❌ **NEVER** scatter eyes randomly around effects
- ✅ Place eyes at impact points, watching struck targets
- ✅ Position eyes to look at specific entities (enemies, players)
- ✅ Use for AOE centers where all eyes watch inward
- ✅ Create formations where eyes orbit meaningfully

```csharp
// ❌ WRONG - Random scattered eyes
for (int i = 0; i < 10; i++)
    CustomParticles.EnigmaEyeGaze(pos + Main.rand.NextVector2Circular(50, 50), color);

// ✅ CORRECT - Meaningful placement watching the target
CustomParticles.EnigmaEyeImpact(impactPos, targetNPC.Center, color, 0.6f);

// ✅ CORRECT - Formation watching a central point
CustomParticles.EnigmaEyeFormation(explosionCenter, color, count: 4, radius: 50f);

// ✅ CORRECT - Orbiting eyes always looking outward
CustomParticles.EnigmaEyeOrbit(player.Center, color, count: 3, radius: 40f);
```

### Available EnigmaEye Methods:
| Method | Purpose |
|--------|---------|
| `EnigmaEyeGaze(pos, color, scale, lookDirection?)` | Single eye at position, optionally facing direction |
| `EnigmaEyeImpact(impactPos, targetPos, color, scale)` | Eye at impact watching the target |
| `EnigmaEyeFormation(center, color, count, radius)` | Multiple eyes watching central point |
| `EnigmaEyeTrail(pos, velocity, color, scale)` | Sparse eyes along projectile path |
| `EnigmaEyeExplosion(pos, color, count, speed)` | Eyes burst outward, looking in movement direction |
| `EnigmaEyeOrbit(center, color, count, radius)` | Rotating orbit watching outward |

---

### Glyph Textures (12 variants - Assets/Particles/Glyphs1-12.png)

**Universal arcane symbols usable for ANY theme.** Glyphs represent arcane power, enchantments, debuff/buff stacking, magic circles, and mysterious runes.

**USE GLYPHS FOR:**
- Debuff stack visualization (more stacks = more glyphs)
- Magic circle effects (rotating glyph formations)
- Enchantment activation bursts
- Impact markers for magical attacks
- Ambient magical auras
- Buff indicators

```csharp
// Show debuff stacks visually
int stacks = target.GetModPlayer<DebuffPlayer>().ParadoxStacks;
CustomParticles.GlyphStack(target.Center, EnigmaColors.Purple, stacks, baseScale: 0.3f);

// Magic circle for channeling/summon
CustomParticles.GlyphCircle(summonPos, color, count: 8, radius: 50f, rotationSpeed: 0.02f);

// Impact with supporting glyphs
CustomParticles.GlyphImpact(hitPos, primaryColor, secondaryColor, scale: 0.6f);

// Ambient aura for magical entities
CustomParticles.GlyphAura(entity.Center, color, radius: 40f, count: 2);
```

### Available Glyph Methods:
| Method | Purpose |
|--------|---------|
| `Glyph(pos, color, scale, glyphIndex)` | Single arcane glyph (-1 for random) |
| `GlyphStack(pos, color, stackCount, baseScale)` | Multi-layered stack visualization |
| `GlyphCircle(pos, color, count, radius, rotationSpeed)` | Rotating magic circle |
| `GlyphBurst(pos, color, count, speed)` | Exploding arcane symbols |
| `GlyphTrail(pos, velocity, color, scale)` | Glyphs left behind projectiles |
| `GlyphAura(center, color, radius, count)` | Floating ambient glyphs |
| `GlyphImpact(pos, primary, secondary, scale)` | Impact with supporting glyphs |
| `GlyphTower(pos, color, layers, baseScale)` | Vertical stacking for powerful effects |

### Theme-Specific Glyph Usage:

| Theme | Glyph Colors | Usage Style |
|-------|--------------|-------------|
| **Enigma Variations** | Purple → Green Flame | Heavy use, mysterious, questioning reality |
| **Fate** | White → Crimson | Cosmic circles, destiny runes, reality marks |
| **Moonlight Sonata** | Purple → Silver | Ethereal circles, lunar symbols |
| **La Campanella** | Orange → Gold | Fire runes, bell patterns |
| **Any Theme** | Theme gradient | Debuff stacking, enchantment effects |

---

## VFX Requirements - THE ART OF MUSICAL DESTRUCTION

> *"Give the weapons the trails, the projectiles, the music notes, the particles, the lighting, the dynamicism, the shaders, the waving of the screen as it distorts—give them every ounce of creativity that you have."*

ALL attacks, projectiles, boss abilities, weapon effects, and enemy spawns MUST use the custom particle system. Never use only vanilla Dust effects alone.

### Core Principles: RADIANCE, UNIQUENESS, and MUSICAL SOUL

**Every effect in MagnumOpus should be:**
1. **RADIANT** - Bold, saturated colors that **shine and glow**. Effects should illuminate the battlefield like a spotlight on a stage.
2. **UNIQUE** - Each weapon/ability should have a distinct visual identity. No two weapons should look identical.
3. **MUSICAL** - Incorporate **music notes, staff lines, treble clefs, and musical symbols** wherever thematically appropriate.
4. **DYNAMIC** - Colors should **flow, pulse, breathe, and transition**. Never static, always alive.
5. **AWE-INSPIRING** - Players should pause and think *"That was beautiful."*

### The Music Note Mandate

**This is a MUSIC MOD.** Music notes should appear:
- In **melee swing trails** - notes dancing in the blade's wake
- In **projectile trails** - lingering notes that fade like echoes
- In **explosion bursts** - notes scattering like a chord struck
- In **auras and ambient effects** - floating notes orbiting the player
- In **impact effects** - the crescendo of contact

```csharp
// MELEE SWING - Leave music notes in the blade's trail
public override void MeleeEffects(Player player, Rectangle hitbox)
{
    // Every swing should sing
    if (Main.rand.NextBool(2))
    {
        Vector2 notePos = hitbox.Center.ToVector2() + Main.rand.NextVector2Circular(15f, 15f);
        Vector2 noteVel = (player.direction * Vector2.UnitX).RotatedByRandom(0.5f) * 2f;
        ThemedParticles.MusicNote(notePos, noteVel, themeColor, 0.35f, 30);
    }
}

// PROJECTILE TRAIL - Echoes of melody left behind
public override void AI()
{
    // Every 4 frames, leave a note in the air
    if (Projectile.timeLeft % 4 == 0)
    {
        ThemedParticles.MusicNote(Projectile.Center, -Projectile.velocity * 0.1f, themeColor * 0.7f, 0.25f, 25);
    }
}
```

**AVOID:**
- Generic, copy-pasted effects between weapons
- Bland, single-color explosions without gradient fading
- Weapons that feel "silent" - no musical elements
- Effects that don't match the theme's emotional tone
- Low-impact, barely-visible particles that fail to inspire

**EMBRACE:**
- Creative combinations of existing particle systems
- Theme-appropriate variations that still feel fresh
- Layered effects (particles + halos + lightning + flares)
- Color gradients that transition smoothly within theme palettes

### Design Philosophy - UNIQUE FRACTAL EFFECTS

**Every effect should be unique and include fractal-like geometric patterns when possible.** The signature look is demonstrated by Feather's Call's left-click attack:

```csharp
// FRACTAL FLARE BURST - the signature geometric look
for (int i = 0; i < 6; i++)
{
    float angle = MathHelper.TwoPi * i / 6f;
    Vector2 flareOffset = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 25f;
    float hue = (Main.GameUpdateCount * 0.02f + i * 0.16f) % 1f;
    Color fractalColor = Main.hslToRgb(hue, 1f, 0.85f);
    CustomParticles.GenericFlare(position + flareOffset, fractalColor, 0.4f, 18);
}
```

**Key principles:**
- Use radial geometric patterns (6-8 point star bursts)
- Layer multiple effect types for depth
- Include mini lightning fractals for high-impact moments
- Use prismatic/rainbow color cycling where thematically appropriate
- Combine themed particles with geometric flare arrangements

---

## CRITICAL: Theme Identity & Uniqueness - THE SOUL OF EACH SCORE

> *"Each score like Moonlight Sonata, Eroica, etc. should all feel vastly unique from one another."*

**Each musical score/theme MUST have its own distinct visual AND emotional identity.** Do NOT copy effects between themes. Each theme represents a different musical composition with its own story, feeling, and visual language.

### Why Theme Uniqueness Matters
- Players should **instantly recognize** which theme's weapon they're using just from the visuals
- Each score represents a **different emotional journey** that must translate to effects
- Cross-theme copying creates bland, forgettable weapons that betray the music

### Theme Visual & Emotional Identities (EMBRACE FULLY)

| Theme | Musical Soul | Visual Language | Emotional Core |
|-------|--------------|-----------------|----------------|
| **La Campanella** | The ringing bell, virtuosic fire | Heavy smoke, bell chimes, infernal orange flames | Passion, intensity, burning brilliance |
| **Eroica** | The hero's symphony | Sakura petals, golden-tinged scarlet embers, rising triumph | Courage, sacrifice, triumphant glory |
| **Swan Lake** | Grace dying beautifully | Feathers drifting, monochrome elegance, prismatic edges | Elegance, tragedy, ethereal beauty |
| **Moonlight Sonata** | The moon's quiet sorrow | Soft purple mist, silver light, lunar halos | Melancholy, peace, mystical stillness |
| **Enigma Variations** | The unknowable mystery | Swirling void, watching eyes, eerie green flames | Mystery, dread, arcane secrets |
| **Fate** | The celestial symphony of destiny | **Ancient glyphs orbiting**, **star particles streaming**, **cosmic cloud energy like Ark of the Cosmos**, chromatic aberration, reality distortions | Celestial inevitability, cosmic power, endgame awe |

### Embracing Each Score's Unique Elements

**La Campanella** - *The Flaming Bell*
```csharp
// Every La Campanella weapon should feel like ringing bells of fire
// - Heavy black smoke billowing
// - Orange flames crackling and dancing
// - Bell chime sounds on impacts
// - The intensity of Liszt's virtuosic passion
```

**Eroica** - *The Hero's Journey*
```csharp
// Eroica weapons tell the story of heroic sacrifice
// - Sakura petals scattering like a warrior's final stand
// - Golden light breaking through scarlet flames
// - Rising embers ascending toward the heavens
// - The triumph and tragedy of Beethoven's symphony
```

**Swan Lake** - *Grace in Monochrome*
```csharp
// Swan Lake weapons are elegant even in destruction
// - White and black feathers drifting gracefully
// - Prismatic rainbow shimmer at the edges
// - Clean, graceful arcs and flowing trails
// - The dying beauty of Tchaikovsky's swans
```

**Moonlight Sonata** - *The Moon's Whisper*
```csharp
// Moonlight weapons are soft, mystical, lunar
// - Soft purple mist rolling gently
// - Silver light like moonbeams through clouds
// - Gentle, flowing movements
// - The quiet melancholy of Beethoven's adagio
```

**Fate** - *The Celestial Symphony of Destiny*
```csharp
// Fate weapons are CELESTIAL COSMIC ENDGAME - think Ark of the Cosmos meets dark celestial power
// MANDATORY VISUAL ELEMENTS:
// - ANCIENT GLYPHS orbiting weapons, projectiles, and impacts (use Glyph particles heavily)
// - STAR PARTICLES streaming and twinkling in trails and explosions
// - COSMIC CLOUD ENERGY billowing like Ark of the Cosmos constellation trails
// - Screen distortions and chromatic aberration
// - Dark prismatic: black bleeding to pink to crimson with celestial white highlights
// - Temporal echoes and sharp afterimage trails with star sparkles
// - Constellation-like patterns connecting impacts
// - Cosmic nebula cloud effects swirling around attacks
// The feeling: You are wielding the power of the cosmos itself
```

### FORBIDDEN Cross-Theme Copying

```csharp
// ❌ WRONG - Using La Campanella effects on Fate weapon
UnifiedVFX.LaCampanella.DeathExplosion(position, scale); // NO! Wrong theme!
ThemedParticles.LaCampanellaSparks(position, direction, count, speed); // NO!

// ✅ CORRECT - Use the weapon's own theme
UnifiedVFX.Fate.Explosion(position, scale);
// Or create unique Fate-specific effects with reality distortions
```

### Creating Unique Theme Effects

**Instead of copying, create VARIATIONS that match the theme:**

```csharp
// La Campanella explosion - smoky, fiery, bell-like
UnifiedVFX.LaCampanella.Explosion(pos, scale);
// Includes: HeavySmokeParticle, orange/black gradient, bell chime sounds

// Eroica explosion - triumphant, petal-filled, golden
UnifiedVFX.Eroica.Explosion(pos, scale);  
// Includes: SakuraPetals, scarlet/gold gradient, rising embers

// Swan Lake explosion - elegant, feathered, prismatic
UnifiedVFX.SwanLake.Explosion(pos, scale);
// Includes: SwanFeatherBurst, black/white contrast, rainbow sparkles

// Moonlight Sonata explosion - ethereal, misty, lunar
UnifiedVFX.MoonlightSonata.Explosion(pos, scale);
// Includes: Soft bloom, purple/silver mist, moon-like halos

// Enigma Variations explosion - mysterious, void-touched, arcane
UnifiedVFX.EnigmaVariations.Explosion(pos, scale);
// Includes: Void swirls, green flame accents, mystery particles

// Fate explosion - reality-shattering, cosmic, UNIQUE
UnifiedVFX.Fate.Explosion(pos, scale);
// Includes: Screen distortions, chromatic aberration, temporal echoes
// FATE MUST BE THE MOST VISUALLY DISTINCT - it's endgame content!
```

### Fate-Specific Requirements (ENDGAME) - CELESTIAL COSMIC AESTHETIC

Fate is the endgame theme and MUST feel like wielding CELESTIAL COSMIC POWER. Think Ark of the Cosmos from Calamity - billowing cosmic clouds, constellation trails, but with MagnumOpus's dark Fate color palette.

**MANDATORY CELESTIAL ELEMENTS (Include in ALL Fate effects):**
- **Ancient Glyphs** - Orbiting glyph particles around weapons, projectiles, and on impacts. Use `CustomParticles.Glyph`, `GlyphBurst`, `GlyphCircle`, `GlyphOrbit`
- **Star Particles** - Twinkling stars in trails, sparkle bursts on impacts, star field backgrounds for major attacks
- **Cosmic Cloud Energy** - Billowing nebula-like particle clouds trailing behind attacks (like Ark of the Cosmos constellation effects)
- **Constellation Patterns** - Connect impacts with faint starry lines, create constellation shapes in explosions

**SCREEN/VISUAL DISTORTION EFFECTS:**
- **Screen slice effects** - visual "cuts" across the screen
- **Reality shatter** - screen fragments briefly
- **Chromatic aberration** - RGB color separation
- **Temporal echoes** - sharp afterimage trails with star sparkles
- **Color inversion pulses** - brief negative flashes

**ARK OF THE COSMOS INSPIRATION:**
Study how Ark of the Cosmos creates its constellation chains and cosmic cloud trails:
- Particles spawn along movement paths creating nebula-like clouds
- Star points connect with faint glowing lines
- Colors shift and shimmer through the cosmic gradient
- Effects feel like tearing through the fabric of space itself

```csharp
// Fate weapons are CELESTIAL - they channel the power of the cosmos
// Every attack should feel like commanding the stars themselves
// Glyphs orbit, stars stream, cosmic clouds billow, reality bends
// The player should feel like a god wielding celestial destruction
```

### Checklist Before Implementing Any Effect

1. ✅ Am I using the CORRECT theme's UnifiedVFX/ThemedParticles?
2. ✅ Does this effect feel different from other themes?
3. ✅ Am I using this theme's unique color palette?
4. ✅ For Fate: Did I include reality-distortion effects?
5. ✅ Would a player recognize the theme just from the visuals?

---

## 📝 TOOLTIP AND DESCRIPTION FORMATTING - MANDATORY

### NO Capitalized Emphasis in Descriptions

**Item tooltips and descriptions should follow vanilla Terraria's style** - informative, clean, and professional. Do NOT use capitalized words for emphasis.

```csharp
// ❌ WRONG - Capitalized emphasis looks unprofessional
tooltips.Add(new TooltipLine(Mod, "Effect", "Every 5th strike unleashes FATE SEVER - a REALITY-CLEAVING slash"));
tooltips.Add(new TooltipLine(Mod, "Effect", "Fires slow, MASSIVE reality-warping shots"));
tooltips.Add(new TooltipLine(Mod, "Effect", "Enemies at 5 Paradox stacks EXPLODE"));

// ✅ CORRECT - Clean, informative, vanilla-style descriptions
tooltips.Add(new TooltipLine(Mod, "Effect", "Every 5th strike unleashes a reality-cleaving slash"));
tooltips.Add(new TooltipLine(Mod, "Effect", "Fires slow, massive reality-warping shots"));
tooltips.Add(new TooltipLine(Mod, "Effect", "Enemies at 5 Paradox stacks trigger an explosion"));
```

### Description Guidelines

1. **Use sentence case** - Only capitalize the first word and proper nouns
2. **Be concise** - Keep descriptions short and informative
3. **No shouting** - Avoid ALL CAPS for emphasis
4. **Match vanilla style** - Look at vanilla Terraria item descriptions for reference
5. **Lore lines can be poetic** - The italic lore quote can be more creative

```csharp
// Good tooltip structure
tooltips.Add(new TooltipLine(Mod, "Effect1", "Swings create temporal echoes that damage enemies"));
tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 5th hit triggers a devastating slash"));
tooltips.Add(new TooltipLine(Mod, "Lore", "'The blade that severs destiny itself'") 
{ 
    OverrideColor = ThemeColor 
});
```

---

## Asset Requirements - PLACEHOLDERS

**If any texture or buff icon doesn't have an image file, use a placeholder:**

```csharp
// For items/projectiles without textures:
public override string Texture => "Terraria/Images/Item_" + ItemID.DirtBlock;
// OR use an existing mod texture:
public override string Texture => "MagnumOpus/Assets/Particles/Placeholder";

// For buffs without icons:
public override string Texture => "Terraria/Images/Buff_" + BuffID.Confused;
```

**Never leave a texture path pointing to a non-existent file.** Always fall back to a vanilla texture or existing mod asset.

---

## ⚠️ CRITICAL: NO VANILLA PROJECTILE SPRITES - MANDATORY

> *"For EVERY weapon in this modpack—ALL OF THEM—do NOT use vanilla Terraria art for the projectiles. Make your OWN unique projectiles."*

**This is a NON-NEGOTIABLE rule.** Every single projectile in MagnumOpus MUST have custom visual identity, NOT vanilla Terraria sprites.

### The Problem with Vanilla Sprites
```csharp
// ❌ ABSOLUTELY FORBIDDEN - Using vanilla projectile texture
public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.RocketI;
public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.MoonlordArrow;
public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.NebulaBlaze1;

// ❌ ALSO FORBIDDEN - Just hiding bad projectiles
Projectile.alpha = 255; // Making it invisible is NOT a solution!
```

### The Solution: Custom Projectile Visuals

**Option 1: Create custom projectile textures**
```csharp
// ✅ CORRECT - Use a custom mod texture
public override string Texture => "MagnumOpus/Content/Fate/Projectiles/DestinyBolt";
public override string Texture => "MagnumOpus/Content/EnigmaVariations/Projectiles/ParadoxOrb";
```

**Option 2: Make projectiles visually invisible and rely ENTIRELY on particle effects**
```csharp
// ✅ CORRECT - Invisible projectile with HEAVY particle-based visuals
public override string Texture => "MagnumOpus/Assets/Particles/Invisible"; // 1x1 transparent pixel

public override void AI()
{
    // The projectile IS the particles - make them DENSE and beautiful
    // This MUST be visually spectacular to compensate for no sprite
    
    // Core glow - the "body" of the projectile
    CustomParticles.GenericFlare(Projectile.Center, primaryColor, 0.6f, 8);
    
    // Heavy trailing particles every frame
    for (int i = 0; i < 3; i++)
    {
        var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(2f, 2f),
            GetThemeGradient(Main.rand.NextFloat()), 0.35f, 18, true);
        MagnumParticleHandler.SpawnParticle(trail);
    }
    
    // Music notes in trail
    if (Main.rand.NextBool(3))
        ThemedParticles.[Theme]MusicNotes(Projectile.Center, 2, 15f);
    
    // Glyph accents
    if (Main.rand.NextBool(5))
        CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, themeColor, 0.3f);
}
```

**Option 3: Custom PreDraw rendering (draw the projectile yourself)**
```csharp
// ✅ CORRECT - Custom-drawn projectile with full control
public override bool PreDraw(ref Color lightColor)
{
    // Draw multiple layered glows as the "projectile"
    SpriteBatch spriteBatch = Main.spriteBatch;
    Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
    Vector2 drawPos = Projectile.Center - Main.screenPosition;
    Vector2 origin = glowTex.Size() / 2f;
    
    // Outer glow layer
    spriteBatch.Draw(glowTex, drawPos, null, primaryColor * 0.4f, 0f, origin, 1.2f, SpriteEffects.None, 0f);
    // Middle glow layer
    spriteBatch.Draw(glowTex, drawPos, null, secondaryColor * 0.6f, 0f, origin, 0.8f, SpriteEffects.None, 0f);
    // Inner core
    spriteBatch.Draw(glowTex, drawPos, null, Color.White * 0.8f, 0f, origin, 0.4f, SpriteEffects.None, 0f);
    
    return false; // Don't draw the default sprite
}
```

### Projectile Folder Structure

Each theme should have its own Projectiles folder:
```
Content/
├── EnigmaVariations/
│   └── Projectiles/
│       ├── ParadoxOrb.cs
│       ├── ParadoxOrb.png
│       ├── RiddleBolt.cs
│       └── RiddleBolt.png
├── Fate/
│   └── Projectiles/
│       ├── DestinyBolt.cs
│       ├── DestinyBolt.png
│       └── CosmicShard.cs
└── ...
```

### The Golden Rule

**If a projectile exists, it MUST look unique to MagnumOpus.** Players should NEVER see a vanilla Terraria projectile coming from a MagnumOpus weapon.

---

## 🔥 GO ABSOLUTELY CRAZY WITH VISUAL EFFECTS - MANDATORY

> *"These weapons should shine just as brightly and be given just as much visual love as the Swan Lake weapons. Go NUTS with visual effects and abilities."*

### The Problem: Bland, Boring Weapons

Some weapons currently have minimal visual effects - just a few particles here and there. This is **UNACCEPTABLE** for MagnumOpus. Every weapon should be a visual spectacle.

### What "Going Crazy" Actually Means

```csharp
// ❌ WRONG - Minimal, boring, forgettable effects
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    CustomParticles.GenericFlare(target.Center, themeColor, 0.5f, 15);
    CustomParticles.HaloRing(target.Center, themeColor, 0.3f, 12);
}

// ✅ RIGHT - A SYMPHONY OF DESTRUCTION
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    // === PHASE 1: THE IMPACT CORE ===
    // Central white flash - the moment of contact
    CustomParticles.GenericFlare(target.Center, Color.White, 1.2f, 25);
    
    // Theme-colored bloom
    CustomParticles.GenericFlare(target.Center, primaryColor, 0.9f, 22);
    CustomParticles.GenericFlare(target.Center, secondaryColor, 0.7f, 20);
    
    // === PHASE 2: THE EXPANDING SHOCKWAVE ===
    // Multiple gradient halo rings
    for (int ring = 0; ring < 5; ring++)
    {
        float progress = ring / 5f;
        Color ringColor = Color.Lerp(primaryColor, secondaryColor, progress);
        float scale = 0.3f + ring * 0.2f;
        int lifetime = 14 + ring * 4;
        CustomParticles.HaloRing(target.Center, ringColor, scale, lifetime);
    }
    
    // === PHASE 3: THE GEOMETRIC FRACTAL BURST ===
    // 8-point star pattern with gradient
    for (int i = 0; i < 8; i++)
    {
        float angle = MathHelper.TwoPi * i / 8f;
        float progress = (float)i / 8f;
        Vector2 offset = angle.ToRotationVector2() * 35f;
        Color fractalColor = Color.Lerp(primaryColor, secondaryColor, progress);
        CustomParticles.GenericFlare(target.Center + offset, fractalColor, 0.55f, 18);
    }
    
    // === PHASE 4: THE RADIAL PARTICLE SPRAY ===
    // Sparks flying outward
    for (int i = 0; i < 16; i++)
    {
        float angle = MathHelper.TwoPi * i / 16f + Main.rand.NextFloat(-0.2f, 0.2f);
        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
        Color sparkColor = GetThemeGradient((float)i / 16f);
        var spark = new GenericGlowParticle(target.Center, vel, sparkColor, 0.4f, 25, true);
        MagnumParticleHandler.SpawnParticle(spark);
    }
    
    // === PHASE 5: THE MUSICAL NOTES ===
    // Because this is a MUSIC mod!
    ThemedParticles.[Theme]MusicNoteBurst(target.Center, 10, 6f);
    
    // === PHASE 6: THE GLYPH MAGIC ===
    // Arcane symbols for that extra mystical feel
    CustomParticles.GlyphImpact(target.Center, primaryColor, secondaryColor, 0.6f);
    CustomParticles.GlyphBurst(target.Center, secondaryColor, 6, 4f);
    
    // === PHASE 7: DYNAMIC LIGHTING ===
    // Make the impact GLOW
    Lighting.AddLight(target.Center, primaryColor.ToVector3() * 1.5f);
}
```

### The Visual Effect Density Rule

**Minimum requirements for EVERY weapon effect:**

| Effect Type | Minimum Particle Count | Required Elements |
|-------------|----------------------|-------------------|
| **Weapon Swing** | 15+ particles per swing | Flares, gradient trail, music notes |
| **Projectile Trail** | 5+ particles per frame | Glowing core, fading trail, theme particles |
| **Impact/Hit** | 30+ particles | Flares, halos, fractal burst, sparks, glyphs, notes |
| **Explosion** | 50+ particles | Multi-phase, layered rings, radial spray, smoke, glyphs |
| **Ambient/Aura** | 8+ particles per second | Orbiting flares, floating notes, subtle glyphs |
| **Death/Kill** | 80+ particles | Maximum spectacle, screen shake, all the above |

### Layer Your Effects - The Onion Principle

Every major effect should have multiple LAYERS:

1. **Core Layer** - Bright white/primary flash at center
2. **Secondary Layer** - Theme-colored bloom around core
3. **Geometric Layer** - Fractal patterns, star bursts, symmetry
4. **Particle Layer** - Sparks, embers, trailing particles
5. **Halo Layer** - Expanding rings, shockwaves
6. **Musical Layer** - Music notes, accidentals, staff lines
7. **Arcane Layer** - Glyphs, runes, magic circles
8. **Lighting Layer** - Dynamic lighting that pulses

### The Swan Lake Standard

Look at the Swan Lake weapons as the **QUALITY BAR**. Every theme's weapons should match this level of visual polish:

- Dense, constant particle trails
- Multiple layered effects on every action
- Rainbow/gradient color transitions
- Feathers/theme-specific particles everywhere
- Ambient auras while holding
- Dramatic explosions on impacts
- Custom PreDraw with glow effects

**If your weapon doesn't look as spectacular as Swan Lake weapons, it needs more work.**

---

### Required VFX Calls

Every impact, explosion, projectile spawn, attack windup, and attack firing should include a combination of:

```csharp
// Core flares (always include at least one)
CustomParticles.GenericFlare(position, themeColor, scale, lifetime);
CustomParticles.GenericGlow(position, velocity, color, scale, lifetime, fade);

// Halo/ring effects (for impacts and explosions)
CustomParticles.HaloRing(position, color, scale, lifetime);
ThemedParticles.[Theme]HaloBurst(position, scale);

// Explosion bursts (for impacts and deaths)
CustomParticles.ExplosionBurst(position, color, count, speed);

// Theme-specific effects
ThemedParticles.[Theme]Impact(position, scale);
ThemedParticles.[Theme]Shockwave(position, scale);
ThemedParticles.[Theme]Sparkles(position, count, radius);
ThemedParticles.[Theme]Sparks(position, direction, count, speed);
```

### Standard VFX Pattern

**Attack Windup:**
```csharp
// Pulsing flares that grow with charge progress
float chargeProgress = Timer / WindupTime;
CustomParticles.GenericFlare(NPC.Center, themeColor, 0.3f + chargeProgress * 0.5f, 20);
CustomParticles.HaloRing(NPC.Center, themeColor, 0.2f + chargeProgress * 0.3f, 15);
```

**Attack Firing / Projectile Spawn:**
```csharp
ThemedParticles.[Theme]HaloBurst(spawnPos, 1.2f);
CustomParticles.GenericFlare(spawnPos, primaryColor, 0.7f, 20);
CustomParticles.GenericFlare(spawnPos, secondaryColor, 0.5f, 15);
CustomParticles.HaloRing(spawnPos, primaryColor, 0.4f, 18);
```

**Impact / Explosion:**
```csharp
ThemedParticles.[Theme]Impact(position, 1.5f);
ThemedParticles.[Theme]HaloBurst(position, 1.5f);
CustomParticles.GenericFlare(position, primaryColor, 0.8f, 25);
CustomParticles.GenericFlare(position, Color.White, 0.6f, 20);
CustomParticles.HaloRing(position, primaryColor, 0.5f, 20);
CustomParticles.ExplosionBurst(position, primaryColor, 12, 10f);

// ADD FRACTAL PATTERN for unique look
for (int i = 0; i < 6; i++)
{
    float angle = MathHelper.TwoPi * i / 6f;
    Vector2 offset = angle.ToRotationVector2() * 30f;
    CustomParticles.GenericFlare(position + offset, secondaryColor, 0.4f, 15);
}
```

**Projectile Trail (periodic, every 3-5 frames):**
```csharp
if (Projectile.timeLeft % 4 == 0)
{
    CustomParticles.GenericFlare(Projectile.Center, themeColor, 0.4f, 15);
}
```

---

## MANDATORY: Gradient Color Fading Effects

**ALL weapon effects, particles, flares, explosions, and accessory effects MUST use gradient color fading.** Never use single-color explosions. Every effect should fade from one theme color to another within the same palette.

### Gradient Fading Pattern (REQUIRED)
```csharp
// USE THIS - Gradient fading from primary to secondary color
float progress = (float)i / count; // or use lifetime progress
Color gradientColor = Color.Lerp(primaryThemeColor, secondaryThemeColor, progress);
CustomParticles.GenericFlare(position, gradientColor, scale, lifetime);

// For particles with lifetime, fade over time:
var particle = new GenericGlowParticle(position, velocity, primaryColor, scale, lifetime, true)
    .WithGradient(secondaryColor); // Fades from primary to secondary over lifetime

// For radial bursts, gradient across the burst:
for (int i = 0; i < 8; i++)
{
    float progress = (float)i / 8f;
    Color gradientColor = Color.Lerp(primaryColor, secondaryColor, progress);
    float angle = MathHelper.TwoPi * i / 8f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 30f, gradientColor, 0.5f, 18);
}
```

### Theme Gradient Examples
```csharp
// Moonlight Sonata: Dark Purple → Light Blue
Color.Lerp(new Color(75, 0, 130), new Color(135, 206, 250), progress)

// Eroica: Deep Scarlet → Bright Gold
Color.Lerp(new Color(139, 0, 0), new Color(255, 215, 0), progress)

// La Campanella: Black → Orange (with smoky effects)
Color.Lerp(CampanellaBlack, CampanellaOrange, progress)
// ALWAYS include HeavySmokeParticle for smoky atmosphere

// Swan Lake: Pure White → Iridescent Rainbow
Color.Lerp(Color.White, Main.hslToRgb(progress, 1f, 0.8f), progress * 0.6f)

// Enigma Variations: Black → Purple → Green Flame
Color.Lerp(Color.Lerp(new Color(20, 10, 30), new Color(120, 40, 180), progress * 2f), new Color(50, 200, 80), Math.Max(0, progress * 2f - 1f))

// Fate: White → Dark Pink → Deep Purple → Crimson (cosmic)
// Use multi-step gradient for cosmic amorphous look
```

---

## Theme Color Palettes

### La Campanella (Infernal Bell)
**Gradient: Black → Orange (with smoky effects)**
```csharp
ThemedParticles.CampanellaBlack   // (20, 15, 20) - Primary (start) - smoky darkness
ThemedParticles.CampanellaOrange  // (255, 100, 0) - Secondary (end) - infernal flames
ThemedParticles.CampanellaYellow  // (255, 200, 50) - Accent - flame highlights
ThemedParticles.CampanellaGold    // (218, 165, 32) - Accent - golden shimmer
ThemedParticles.CampanellaRed     // (200, 50, 30) - Intense/enrage
// Gradient: Color.Lerp(CampanellaBlack, CampanellaOrange, progress)
// MANDATORY: Include HeavySmokeParticle for smoky atmosphere in ALL La Campanella effects
```

### Eroica (Heroic/Epic)
**Gradient: Scarlet → Crimson → Gold**
```csharp
ThemedParticles.EroicaScarlet     // (139, 0, 0) - Primary (start)
ThemedParticles.EroicaCrimson     // (220, 50, 50) - Secondary (mid)
ThemedParticles.EroicaGold        // (255, 215, 0) - Accent (end)
ThemedParticles.EroicaSakura      // (255, 150, 180) - Sakura pink
ThemedParticles.EroicaBlack       // (30, 20, 25) - Smoke
// Gradient: Color.Lerp(EroicaScarlet, EroicaGold, progress)
```

### Swan Lake (Graceful/Ethereal)
**Gradient: Pure White → Black with Rainbow Shimmer**
```csharp
ThemedParticles.SwanWhite         // (255, 255, 255) - Primary
ThemedParticles.SwanBlack         // (20, 20, 30) - Contrast
ThemedParticles.SwanIridescent    // Rainbow cycling - use Main.hslToRgb()
ThemedParticles.SwanSilver        // (220, 225, 235) - Accent
// Gradient: Alternate white/black with rainbow shimmer overlay
```

### Moonlight Sonata (Lunar/Mystical)
**Gradient: Dark Purple → Light Blue**
```csharp
ThemedParticles.MoonlightDarkPurple  // (75, 0, 130) - Primary (start)
ThemedParticles.MoonlightViolet      // (138, 43, 226) - Mid
ThemedParticles.MoonlightLightBlue   // (135, 206, 250) - Secondary (end)
ThemedParticles.MoonlightSilver      // (220, 220, 235) - Accent
// Gradient: Color.Lerp(MoonlightDarkPurple, MoonlightLightBlue, progress)
```

### Clair de Lune (Celestial)
**Gradient: Night Mist → Pearl White**
```csharp
ThemedParticles.ClairNightMist    // (100, 120, 160) - Primary (start)
ThemedParticles.ClairSoftBlue     // (140, 170, 220) - Mid
ThemedParticles.ClairPearl        // (240, 240, 250) - Secondary (end)
// Gradient: Color.Lerp(ClairNightMist, ClairPearl, progress)
```

### Enigma Variations (Mysterious/Arcane) - NEW
**Gradient: Black → Deep Purple → Eerie Green Flame**
**Design: Mysteries, question marks, swirling unknowns**
```csharp
ThemedParticles.EnigmaBlack       // (15, 10, 20) - Primary (start) - void darkness
ThemedParticles.EnigmaDeepPurple  // (80, 20, 120) - Mid - arcane mystery
ThemedParticles.EnigmaPurple      // (140, 60, 200) - Secondary
ThemedParticles.EnigmaGreenFlame  // (50, 220, 100) - Accent (end) - eerie flame
ThemedParticles.EnigmaDarkGreen   // (30, 100, 50) - Dark green accent
// Gradient: Black → Purple → Green flame transition
// Color.Lerp(Color.Lerp(EnigmaBlack, EnigmaPurple, progress * 2f), EnigmaGreenFlame, Math.Max(0, progress * 2f - 1f))

// SPECIAL: Include question mark and mystery symbol particles
// Swirling void effects with green flame accents
// Effects should feel unknowable and arcane
```

### Fate (Celestial Cosmic Endgame) - DARK PRISMATIC CELESTIAL THEME
**Gradient: Black → Dark Pink → Bright Red (Dark Prismatic) with Celestial White Highlights**
**Design: CELESTIAL, COSMIC, SHARP, FLASHY - Like wielding constellation power with Ark of the Cosmos-style cosmic clouds**
**REQUIRES: Ancient glyphs, star particles, cosmic cloud energy, visual distortions, screen effects**
**PRIMARY: Black cosmic void with dark pink highlights bleeding to bright red, punctuated by celestial white star sparkles**
```csharp
ThemedParticles.FateBlack         // (15, 5, 20) - PRIMARY (base) - cosmic void darkness
ThemedParticles.FateDarkPink      // (180, 50, 100) - Secondary - destiny's edge
ThemedParticles.FateBrightRed     // (255, 60, 80) - Accent (end) - bright crimson highlight
ThemedParticles.FatePurple        // (120, 30, 140) - Mid accent - fate's weave / nebula purple
ThemedParticles.FateWhite         // (255, 255, 255) - Star sparkles, celestial highlights, glyph glow
ThemedParticles.FateStarGold      // (255, 230, 180) - Warm star glow accent

// Gradient: Dark Prismatic Celestial - BLACK cosmic void is the primary
// Step 1: Black → Dark Pink (progress 0-0.4)
// Step 2: Dark Pink → Bright Red (progress 0.4-0.8)
// Step 3: Bright Red → White celestial flash accents (progress 0.8-1.0)

// MANDATORY CELESTIAL ELEMENTS FOR ALL FATE CONTENT:
// - ANCIENT GLYPHS orbiting (CustomParticles.Glyph, GlyphCircle, GlyphOrbit)
// - STAR PARTICLES streaming and twinkling (use white/gold star sparkle particles)
// - COSMIC CLOUD ENERGY billowing (like Ark of the Cosmos nebula trails)
// - CONSTELLATION PATTERNS connecting effects with faint starry lines

// MANDATORY VISUAL DISTORTIONS:
// - Screen slice effects (reality cuts)
// - Color channel separation (chromatic aberration)
// - Screen fragment shattering
// - Temporal distortion (sharp afterimage trails with star sparkles)
// - Inverse color flashes
// - Reality tear effects with cosmic energy bleeding through
```

---

## Fate Visual Distortion Effects (ENDGAME EXCLUSIVE)

Fate weapons are endgame content and MUST include dramatic visual distortions:

### Screen Slice Effect
```csharp
// Creates a visual "cut" across the screen
public static void FateScreenSlice(Vector2 start, Vector2 end, float intensity)
{
    // Draw sharp white line with dark pink/purple edges
    // Chromatic aberration along the cut
    // Brief screen displacement effect
}
```

### Reality Shatter Effect
```csharp
// Screen appears to break into fragments briefly
public static void FateRealityShatter(Vector2 center, int fragments, float duration)
{
    // Screen divided into triangular fragments
    // Fragments briefly offset/rotate
    // Cosmic gradient on fragment edges
    // Sharp, precise, geometric
}
```

### Color Inversion Pulse
```csharp
// Brief negative/inverse color flash
// White becomes black, colors invert momentarily
// Creates "reality breaking" feel
```

### Temporal Echo Trail
```csharp
// Sharp afterimages with color gradient
for (int i = 0; i < 8; i++)
{
    float echoProgress = i / 8f;
    Color echoColor = Color.Lerp(FateWhite, FateCrimson, echoProgress) * (1f - echoProgress);
    // Draw sharp, precise afterimage at historical position
}
```

---

## ⭐ FATE CELESTIAL COSMIC DESIGN - MANDATORY IMPLEMENTATION GUIDE

> **CRITICAL: ALL Fate weapons, accessories, bosses, and effects MUST follow this guide.**

### The Fate Aesthetic: Celestial Cosmic Power

Fate is the **ENDGAME celestial cosmic theme**. Every Fate item should feel like wielding the power of the stars themselves. Think **Ark of the Cosmos from Calamity** - but with MagnumOpus's dark Fate color palette (black → pink → crimson with white star highlights).

### MANDATORY Elements for ALL Fate Content

**1. ANCIENT GLYPHS (Use Extensively)**
```csharp
// Glyphs MUST appear in ALL Fate weapons/effects:
// - Orbiting glyphs around held weapons
// - Glyph bursts on impacts
// - Glyph circles during charge-up
// - Glyph trails behind projectiles

// Weapon hold effect - glyphs orbit the weapon
public override void HoldItem(Player player)
{
    // Spawn orbiting glyphs around the held weapon
    if (Main.rand.NextBool(6))
    {
        float angle = Main.GameUpdateCount * 0.04f;
        for (int i = 0; i < 3; i++)
        {
            float glyphAngle = angle + MathHelper.TwoPi * i / 3f;
            Vector2 glyphPos = player.Center + glyphAngle.ToRotationVector2() * 45f;
            CustomParticles.Glyph(glyphPos, FateDarkPink, 0.4f, -1); // -1 for random glyph
        }
    }
}

// Impact - glyph burst
public override void OnHitNPC(NPC target, ...)
{
    CustomParticles.GlyphBurst(target.Center, FatePurple, 6, 4f);
    CustomParticles.GlyphCircle(target.Center, FateDarkPink, 8, 40f, 0.02f);
}
```

**2. STAR PARTICLES (The Celestial Sparkle)**
```csharp
// Stars MUST appear in ALL Fate effects:
// - Twinkling stars in trails
// - Star bursts on impacts/explosions
// - Star sparkles in auras
// - Constellation-like star patterns

// Projectile trail with star particles
public override void AI()
{
    // Core cosmic cloud trail
    for (int i = 0; i < 3; i++)
    {
        Vector2 cloudOffset = Main.rand.NextVector2Circular(10f, 10f);
        CustomParticles.CosmicCloud(Projectile.Center + cloudOffset, -Projectile.velocity * 0.1f, 
            FatePurple, 0.5f, 20);
    }
    
    // Star sparkles scattered in trail
    if (Main.rand.NextBool(3))
    {
        Vector2 starOffset = Main.rand.NextVector2Circular(15f, 15f);
        CustomParticles.StarSparkle(Projectile.Center + starOffset, FateWhite, 0.3f, 15);
    }
    
    // Occasional glyph in trail
    if (Main.rand.NextBool(8))
    {
        CustomParticles.GlyphTrail(Projectile.Center, Projectile.velocity, FateDarkPink, 0.35f);
    }
}
```

**3. COSMIC CLOUD ENERGY (Ark of the Cosmos Style)**
```csharp
// Billowing cosmic clouds MUST trail behind Fate attacks:
// - Nebula-like particle clouds following projectiles
// - Cosmic energy bursting from impacts
// - Swirling cloud vortexes during charge-ups
// - Cloud dissipation effects on weapon swings

// Cosmic cloud trail (inspired by Ark of the Cosmos)
void SpawnCosmicCloudTrail(Vector2 position, Vector2 velocity)
{
    // Multiple layered cloud particles for nebula effect
    for (int layer = 0; layer < 3; layer++)
    {
        float layerProgress = layer / 3f;
        Color cloudColor = Color.Lerp(FateBlack, FatePurple, layerProgress);
        float scale = 0.4f + layer * 0.15f;
        
        Vector2 offset = Main.rand.NextVector2Circular(8f, 8f);
        Vector2 cloudVel = -velocity * (0.05f + layer * 0.03f) + Main.rand.NextVector2Circular(1f, 1f);
        
        var cloud = new GenericGlowParticle(position + offset, cloudVel, cloudColor * 0.6f, scale, 25, true);
        MagnumParticleHandler.SpawnParticle(cloud);
    }
    
    // Star points in the cloud
    if (Main.rand.NextBool(4))
    {
        CustomParticles.GenericFlare(position + Main.rand.NextVector2Circular(12f, 12f), 
            FateWhite, 0.25f, 12);
    }
}
```

**4. CONSTELLATION PATTERNS (Connect the Stars)**
```csharp
// Major Fate attacks should create constellation-like patterns:
// - Lines connecting star points in explosions
// - Star formations in death effects
// - Constellation chains between multi-hits

// Constellation burst on major impact
void SpawnConstellationBurst(Vector2 center, int starCount, float radius)
{
    List<Vector2> starPositions = new List<Vector2>();
    
    // Place stars in a pattern
    for (int i = 0; i < starCount; i++)
    {
        float angle = MathHelper.TwoPi * i / starCount + Main.rand.NextFloat(-0.3f, 0.3f);
        float dist = radius * Main.rand.NextFloat(0.6f, 1f);
        Vector2 starPos = center + angle.ToRotationVector2() * dist;
        starPositions.Add(starPos);
        
        // Spawn star
        CustomParticles.GenericFlare(starPos, FateWhite, 0.5f, 25);
        CustomParticles.Glyph(starPos, FateDarkPink, 0.3f, -1);
    }
    
    // Draw faint lines connecting stars (constellation effect)
    for (int i = 0; i < starPositions.Count; i++)
    {
        int next = (i + 1) % starPositions.Count;
        // Draw line between star points (use MagnumVFX or custom line drawing)
        DrawConstellationLine(starPositions[i], starPositions[next], FatePurple * 0.4f);
    }
}
```

### Fate Effect Checklist (Use Before Every Implementation)

**For EVERY Fate weapon/accessory/boss effect, verify:**

| Element | Required | Implementation |
|---------|----------|----------------|
| Ancient Glyphs | ✅ MANDATORY | Orbiting, bursts, circles, trails |
| Star Particles | ✅ MANDATORY | Sparkles, twinkles, constellation points |
| Cosmic Clouds | ✅ MANDATORY | Billowing nebula trails (Ark of the Cosmos style) |
| Dark Prismatic Gradient | ✅ MANDATORY | Black → Pink → Red with white highlights |
| Screen Distortions | ✅ For major attacks | Chromatic aberration, screen slice, shatter |
| Constellation Patterns | ⚡ Recommended | Connect stars with faint lines on big effects |

### Fate vs Other Themes - Visual Comparison

```csharp
// ❌ WRONG - This looks like Eroica, not Fate
public override void OnHitNPC(...)
{
    CustomParticles.GenericFlare(target.Center, FateDarkPink, 0.8f, 20);
    CustomParticles.ExplosionBurst(target.Center, FateBrightRed, 12, 8f);
    // Missing: Glyphs, stars, cosmic clouds!
}

// ✅ CORRECT - Proper celestial cosmic Fate effect
public override void OnHitNPC(...)
{
    // Core impact
    CustomParticles.GenericFlare(target.Center, FateWhite, 1.0f, 25);
    CustomParticles.GenericFlare(target.Center, FateDarkPink, 0.8f, 22);
    
    // GLYPHS - mandatory for Fate
    CustomParticles.GlyphBurst(target.Center, FatePurple, 6, 5f);
    
    // STAR PARTICLES - the celestial sparkle
    for (int i = 0; i < 8; i++)
    {
        Vector2 starOffset = Main.rand.NextVector2Circular(30f, 30f);
        CustomParticles.GenericFlare(target.Center + starOffset, FateWhite, 0.3f, 18);
    }
    
    // COSMIC CLOUD BURST - Ark of the Cosmos style
    for (int i = 0; i < 12; i++)
    {
        float angle = MathHelper.TwoPi * i / 12f;
        Vector2 cloudVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
        Color cloudColor = Color.Lerp(FateBlack, FatePurple, Main.rand.NextFloat());
        var cloud = new GenericGlowParticle(target.Center, cloudVel, cloudColor * 0.5f, 0.5f, 30, true);
        MagnumParticleHandler.SpawnParticle(cloud);
    }
    
    // Halos with Fate gradient
    CustomParticles.HaloRing(target.Center, FateDarkPink, 0.6f, 20);
    CustomParticles.HaloRing(target.Center, FateBrightRed, 0.4f, 18);
    
    // Screen effects for major hits
    if (hit.Crit)
    {
        // Add chromatic aberration pulse
        FateScreenEffects.ChromaticPulse(target.Center, 0.5f);
    }
}
```

### Boss Fights - Celestial Spectacle

Fate bosses should feel like fighting a **cosmic entity**:

```csharp
// Boss attack windups should have:
// - Orbiting glyph circles growing in intensity
// - Star particles gathering at charge point
// - Cosmic clouds swirling inward
// - Reality distortions intensifying

// Boss death should have:
// - Massive constellation explosion
// - Screen-wide chromatic aberration
// - Glyph cascade
// - Star supernova effect
// - Cosmic cloud dissipation wave
```

### Weapon Categories - Specific Guidance

**Melee Weapons:**
- Swing trails with cosmic cloud wisps
- Glyph particles scattered along swing arc
- Star sparkles at blade tip
- Impact creates mini constellation burst

**Ranged Weapons:**
- Projectiles trail cosmic clouds (dense, billowing)
- Glyphs orbit the projectile
- Star particles in wake
- Muzzle flash with glyph burst

**Magic Weapons:**
- Channeling creates glyph circles
- Cosmic energy gathers during charge
- Release sends cosmic wave with stars
- Impact creates reality tear with constellation

**Summon Weapons:**
- Minions have glyph auras
- Attack trails leave cosmic clouds
- Star sparkles on minion attacks
- Summon animation with glyph circle

**Accessories:**
- Passive glyph orbit around player
- Star sparkle ambient effect
- Cosmic cloud wisps when moving fast
- Proc effects include constellation bursts

---

## UnifiedVFX System - PREFERRED API

**The UnifiedVFX system is the PREFERRED way to create visual effects.** It provides a consolidated API that combines all particle systems with theme-specific effects.

### Basic Usage
```csharp
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

// Theme-based effects - PREFERRED
UnifiedVFX.[Theme].[Effect](position, scale);

// Examples:
UnifiedVFX.LaCampanella.Impact(position, 1.5f);     // Infernal bell impact
UnifiedVFX.Eroica.DeathExplosion(position, 2f);    // Heroic boss death
UnifiedVFX.SwanLake.Trail(position, velocity);      // Feather trail
UnifiedVFX.Fate.Explosion(position, 1.5f);          // Cosmic explosion
UnifiedVFX.Generic.FractalBurst(pos, color1, color2, 8, 40f, 1f); // Geometric burst
```

### Available Theme Classes

#### UnifiedVFX.LaCampanella
**Infernal Bell Theme - Black → Orange**
```csharp
// Colors
UnifiedVFX.LaCampanella.Black    // (20, 15, 20)
UnifiedVFX.LaCampanella.Orange   // (255, 100, 0)
UnifiedVFX.LaCampanella.Yellow   // (255, 200, 50)
UnifiedVFX.LaCampanella.Gold     // (218, 165, 32)

// Effects
UnifiedVFX.LaCampanella.Impact(position, scale);        // Standard impact with smoke
UnifiedVFX.LaCampanella.Explosion(position, scale);     // Major explosion
UnifiedVFX.LaCampanella.BellChime(position, scale);     // Musical bell chime
UnifiedVFX.LaCampanella.SwingAura(pos, dir, scale);     // Weapon swing
UnifiedVFX.LaCampanella.Trail(pos, vel, scale);         // Projectile trail
UnifiedVFX.LaCampanella.DeathExplosion(position, scale);// Boss death
UnifiedVFX.LaCampanella.Aura(position, radius, scale);  // Ambient aura
```

#### UnifiedVFX.Eroica
**Heroic Theme - Scarlet → Gold**
```csharp
// Colors
UnifiedVFX.Eroica.Scarlet   // (139, 0, 0)
UnifiedVFX.Eroica.Crimson   // (220, 50, 50)
UnifiedVFX.Eroica.Flame     // (255, 100, 50)
UnifiedVFX.Eroica.Gold      // (255, 215, 0)
UnifiedVFX.Eroica.Sakura    // (255, 150, 180)

// Effects
UnifiedVFX.Eroica.Impact(position, scale);
UnifiedVFX.Eroica.Explosion(position, scale);       // Includes sakura petals
UnifiedVFX.Eroica.SwingAura(pos, dir, scale);
UnifiedVFX.Eroica.Trail(pos, vel, scale);
UnifiedVFX.Eroica.PhaseTransition(position, scale); // Boss phase change
UnifiedVFX.Eroica.DeathExplosion(position, scale);
UnifiedVFX.Eroica.Aura(position, radius, scale);
```

#### UnifiedVFX.MoonlightSonata
**Lunar Theme - Purple → Blue**
```csharp
// Colors
UnifiedVFX.MoonlightSonata.DarkPurple   // (75, 0, 130)
UnifiedVFX.MoonlightSonata.MediumPurple // (138, 43, 226)
UnifiedVFX.MoonlightSonata.LightBlue    // (135, 206, 250)
UnifiedVFX.MoonlightSonata.Silver       // (220, 220, 235)

// Effects - same pattern as other themes
UnifiedVFX.MoonlightSonata.Impact(position, scale);
UnifiedVFX.MoonlightSonata.Explosion(position, scale);
// ... etc
```

#### UnifiedVFX.SwanLake
**Graceful Theme - White/Black + Rainbow**
```csharp
// Colors
UnifiedVFX.SwanLake.White   // (255, 255, 255)
UnifiedVFX.SwanLake.Black   // (20, 20, 30)
UnifiedVFX.SwanLake.Silver  // (220, 225, 235)
UnifiedVFX.SwanLake.GetRainbow(offset) // Rainbow color cycling

// Effects include feathers and prismatic effects
UnifiedVFX.SwanLake.Impact(position, scale);  // Includes feather burst
// ... etc
```

#### UnifiedVFX.EnigmaVariations
**Mysterious Theme - Black → Purple → Green**
```csharp
// Colors
UnifiedVFX.EnigmaVariations.Black       // (15, 10, 20)
UnifiedVFX.EnigmaVariations.DeepPurple  // (80, 20, 120)
UnifiedVFX.EnigmaVariations.Purple      // (140, 60, 200)
UnifiedVFX.EnigmaVariations.GreenFlame  // (50, 220, 100)

// Effects with eerie green flames
UnifiedVFX.EnigmaVariations.Impact(position, scale);
// ... etc
```

#### UnifiedVFX.Fate
**Cosmic Endgame Theme - White → Pink → Purple → Crimson**
```csharp
// Colors
UnifiedVFX.Fate.White     // (255, 255, 255)
UnifiedVFX.Fate.DarkPink  // (200, 80, 120)
UnifiedVFX.Fate.Purple    // (140, 50, 160)
UnifiedVFX.Fate.Crimson   // (180, 30, 60)

// Helper for cosmic gradient
UnifiedVFX.Fate.GetCosmicGradient(progress) // Returns gradient color 0-1

// Effects - maximum spectacle for endgame
UnifiedVFX.Fate.Impact(position, scale);
UnifiedVFX.Fate.Explosion(position, scale);    // Reality-shattering
UnifiedVFX.Fate.SwingAura(pos, dir, scale);    // Chromatic aberration
UnifiedVFX.Fate.Trail(pos, vel, scale);        // Cosmic afterimages
UnifiedVFX.Fate.DeathExplosion(position, scale); // Ultimate spectacle
```

#### UnifiedVFX.Generic
**Theme-agnostic utilities**
```csharp
UnifiedVFX.Generic.Impact(pos, primary, secondary, scale);
UnifiedVFX.Generic.Explosion(pos, primary, secondary, scale);
UnifiedVFX.Generic.DeathExplosion(pos, primary, secondary, scale);
UnifiedVFX.Generic.Teleport(departure, arrival, color, scale);
UnifiedVFX.Generic.ChargeWindup(pos, color, progress, scale);
UnifiedVFX.Generic.AttackRelease(pos, primary, secondary, scale);

// Signature MagnumOpus geometric burst
UnifiedVFX.Generic.FractalBurst(position, primary, secondary, points, radius, scale);

// Orbiting aura particles
UnifiedVFX.Generic.OrbitingAura(center, primary, secondary, radius, count, scale);
```

### Upgrade Pattern - Converting Old Effects to UnifiedVFX

**Before (old style):**
```csharp
ThemedParticles.LaCampanellaImpact(position, 1.5f);
CustomParticles.GenericFlare(position, ThemedParticles.CampanellaOrange, 0.5f, 20);
CustomParticles.HaloRing(position, ThemedParticles.CampanellaOrange, 0.4f, 18);
```

**After (UnifiedVFX - PREFERRED):**
```csharp
UnifiedVFX.LaCampanella.Impact(position, 1.5f);
// Single call does all the work with proper gradient colors and effects
```

### Boss Death Animation Example
```csharp
private void UpdateDeathAnimation()
{
    deathTimer++;
    float progress = (float)deathTimer / DeathAnimationDuration;
    
    // Phase 1: Building intensity
    if (deathTimer < 120)
    {
        float intensity = (float)deathTimer / 120f;
        
        // Fractal flare pattern with gradient
        if (deathTimer % 5 == 0)
        {
            int points = 6 + (int)(intensity * 4);
            for (int i = 0; i < points; i++)
            {
                float angle = MathHelper.TwoPi * i / points + deathTimer * 0.05f;
                Vector2 offset = angle.ToRotationVector2() * (30f + intensity * 40f);
                float gradientProgress = (float)i / points;
                Color flareColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.4f + intensity * 0.4f, 15);
            }
        }
        
        MagnumScreenEffects.AddScreenShake(intensity * 3f);
    }
    // Phase 2: Climax
    else if (deathTimer == 150)
    {
        // UnifiedVFX themed death explosion
        UnifiedVFX.Eroica.DeathExplosion(NPC.Center, 1.5f);
        
        // Extra spiral galaxy effect for heroic finale
        for (int arm = 0; arm < 6; arm++)
        {
            float armAngle = MathHelper.TwoPi * arm / 6f;
            for (int point = 0; point < 8; point++)
            {
                float spiralAngle = armAngle + point * 0.4f;
                float spiralRadius = 25f + point * 18f;
                Vector2 spiralPos = NPC.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                float gradientProgress = (arm * 8 + point) / 48f;
                Color galaxyColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, gradientProgress);
                CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.5f + point * 0.05f, 25);
            }
        }
    }
}
```

### Weapon Effect Example
```csharp
public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, ...)
{
    // UnifiedVFX themed impact
    UnifiedVFX.Eroica.Impact(position, 1.2f);
    
    // Fractal flare burst with gradient
    for (int i = 0; i < 8; i++)
    {
        float angle = MathHelper.TwoPi * i / 8f;
        Vector2 flareOffset = angle.ToRotationVector2() * 35f;
        float progress = (float)i / 8f;
        Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
        CustomParticles.GenericFlare(position + flareOffset, fractalColor, 0.55f, 22);
    }
    
    // Sakura petals for Eroica theme
    ThemedParticles.SakuraPetals(position, 4, 30f);
    
    return false;
}
```

### Accessory Ambient Effect Example
```csharp
public override void UpdateAccessory(Player player, bool hideVisual)
{
    if (!hideVisual)
    {
        // UnifiedVFX themed aura
        UnifiedVFX.Eroica.Aura(player.Center, 35f, 0.3f);
        
        // Orbiting gradient flares - signature geometric look
        if (Main.rand.NextBool(8))
        {
            float baseAngle = Main.GameUpdateCount * 0.025f;
            for (int i = 0; i < 3; i++)
            {
                float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                float radius = 30f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 8f;
                Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
                float progress = (float)i / 3f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 16);
            }
        }
    }
}
```

---

## Available Particle Types

### CustomParticles (Common/Systems/Particles/)
- `GenericFlare(position, color, scale, lifetime)` - Bright point glow
- `GenericGlow(position, velocity, color, scale, lifetime, fade)` - Moving glow
- `HaloRing(position, color, scale, lifetime)` - Expanding ring
- `ExplosionBurst(position, color, count, speed)` - Radial particle spray

### ThemedParticles (Common/Systems/ThemedParticles.cs)
Each theme has:
- `[Theme]BloomBurst` - Soft bloom effect
- `[Theme]Sparkles` - Scattered sparkle particles
- `[Theme]Shockwave` - Expanding shockwave ring
- `[Theme]Sparks` - Directional spark particles
- `[Theme]Impact` - Standard impact effect
- `[Theme]Trail` - Trail effect for projectiles
- `[Theme]Aura` - Ambient aura particles
- `[Theme]HaloBurst` - Bright halo explosion
- `[Theme]BellChime` (La Campanella) - Bell-specific effect
- `[Theme]MusicNotes` - Floating music note particles

### MagnumVFX (Common/Systems/MagnumVFX.cs)
- `DrawLaCampanellaLightning(start, end, segments, amplitude, branches, branchChance)`
- `DrawEroicaLightning(...)` 
- Other themed lightning/beam effects

### Particle Classes (for custom behavior)
- `GenericGlowParticle` - Glowing particle with fade
- `GlowSparkParticle` - Spark with stretch and gravity
- `HeavySmokeParticle` - Smoke with drift

---

## Boss Requirements - THE GRAND PERFORMANCE

Bosses are the **climax of each theme's symphony**. Every boss fight should feel like an orchestral performance building to a crescendo.

### Spawn - The Overture
- Boss summon items must spawn boss above ground accounting for NPC.height
- Spawn position should use tile collision checks
- **Dramatic entrance VFX** - The audience must know the conductor has arrived

### Attack Windups - Building Tension
- **Progressive VFX that scales with charge progress** - The longer the buildup, the more spectacular the visual
- Sound cues that match the theme's musical identity
- **Pulsing, breathing effects** that create anticipation

### Attack Firing - The Crescendo
- **Full VFX burst on attack release** - This is the moment players remember
- Sky flash for major attacks (via LaCampanellaSkyEffect.TriggerFlash, etc.)
- **Screen shake ONLY for charged attacks and phase transitions** - Use sparingly for maximum impact

### Enrage - The Finale
- **Massive VFX explosion** with multiple layered effects
- Color shift to more intense theme variants
- Sky effect that makes the whole world feel the boss's power

---

## Projectile Requirements - THE FLYING MELODY

Every projectile is a note in the symphony of combat. Make each one sing.

### OnSpawn / First Frame
- **Spawn flare and halo at origin** - Announce the projectile's birth
- Consider music note spawn effects

### AI (periodic)
- **Trail effects every 3-5 frames** - The melody lingers in the air
- **Music notes in trails** where thematically appropriate
- Periodic flares for visibility and radiance

### Kill / OnHit
- **Full impact VFX suite** - The note's final chord
- HaloBurst, GenericFlare, ExplosionBurst
- **Theme-appropriate effects** - Not generic explosions

---

## Weapon Requirements - EVERY WEAPON SHOULD SING

> *"If you want to make a sword that slams itself into the ground before casting out waves of symphonic energy, DO it! If you want to make a gun that fires a bullet into the air and it rains down musical notes and flaming projectiles onto the enemies, be my guest."*

### Melee - The Dancing Blade
- **Swing trails with music notes** - Every swing leaves musical echoes in the blade's wake
- Use MeleeSmearEffect for elegant, flowing trails
- **Impact VFX that tells a story** - Not just an explosion, but a crescendo of the theme
- Consider unique attack patterns: ground slams, charged releases, combo finishers

### Ranged - The Singing Storm
- **Muzzle flash that announces the shot** - GenericFlare, HaloRing, theme particles
- **Projectile trails with musical elements** - Notes lingering in the air like echoes
- On impact: **Themed explosions, not generic bursts**
- Consider: Splitting projectiles, homing notes, rain-down effects, chain reactions

### Magic - The Conductor's Art
- **Cast VFX at player/weapon** - The magic circle, the gathering power
- **Themed particles matching spell element** - Fire runes for La Campanella, lunar symbols for Moonlight
- **Channeling effects that build anticipation** - Orbiting glyphs, intensifying glows
- Consider: Area denial, mark-and-detonate, cosmic judgment

### Summon - The Orchestra Manifested
- **Spawn VFX for minion appearance** - A dramatic entrance worthy of a performer
- **Ambient particles on minion** - Auras, orbiting notes, theme trails
- **Attack effects that match the theme** - The minion is an extension of the music
- Consider: Minion synergies, formation attacks, conductor-like player interaction

---

## ADVANCED VFX COMBINATIONS - SIGNATURE EFFECTS

> *"Give them every ounce of creativity that you have."*

### Ambient Fractal Orbit Pattern (HoldItem)
Use for weapons held by the player to create a magical aura:

```csharp
// Orbiting fractal flares - creates celestial/magical presence
if (Main.rand.NextBool(6))
{
    float baseAngle = Main.GameUpdateCount * 0.025f;
    for (int i = 0; i < 5; i++)
    {
        float angle = baseAngle + MathHelper.TwoPi * i / 5f;
        float radius = 35f + (float)Math.Sin(Main.GameUpdateCount * 0.05f + i * 0.7f) * 12f;
        Vector2 flarePos = player.Center + angle.ToRotationVector2() * radius;
        Color fractalColor = Color.Lerp(primaryColor, secondaryColor, (float)i / 5f);
        CustomParticles.GenericFlare(flarePos, fractalColor, 0.32f, 18);
    }
}
```

### Spiral Galaxy Burst (Ultimate Attacks)
```csharp
// Creates a swirling galaxy explosion effect
for (int arm = 0; arm < 6; arm++)
{
    float armAngle = MathHelper.TwoPi * arm / 6f;
    for (int point = 0; point < 8; point++)
    {
        float spiralAngle = armAngle + point * 0.4f;
        float spiralRadius = 20f + point * 15f;
        Vector2 spiralPos = position + spiralAngle.ToRotationVector2() * spiralRadius;
        float hue = (arm / 6f + point * 0.05f) % 1f;
        Color galaxyColor = Main.hslToRgb(hue, 1f, 0.8f);
        CustomParticles.GenericFlare(spiralPos, galaxyColor, 0.3f + point * 0.05f, 20 + point * 2);
    }
}
CustomParticles.HaloRing(position, Color.White, 1.2f, 30);
ThemedParticles.[Theme]Shockwave(position, 1.5f);
```

### Layered Halo Cascade (Boss Phase Transitions)
```csharp
// Multiple expanding halos with staggered timing
for (int ring = 0; ring < 8; ring++)
{
    float hue = (Main.GameUpdateCount * 0.02f + ring * 0.12f) % 1f;
    Color ringColor = Main.hslToRgb(hue, 0.9f, 0.75f);
    float scale = 0.3f + ring * 0.15f;
    int lifetime = 15 + ring * 5;
    CustomParticles.HaloRing(position, ringColor, scale, lifetime);
}
// Central white flash
CustomParticles.GenericFlare(position, Color.White, 1.5f, 10);
```

### Fractal Lightning Web (Channeled/Chain Attacks)
```csharp
// Draw lightning fractals between multiple points
List<NPC> targets = FindNearbyEnemies(range, maxTargets);
Vector2 lastPoint = sourcePosition;
foreach (var target in targets)
{
    // Main lightning
    MagnumVFX.Draw[Theme]Lightning(lastPoint, target.Center, 12, 40f, 5, 0.6f);
    
    // Mini fractal branches at impact
    for (int i = 0; i < 4; i++)
    {
        float branchAngle = MathHelper.TwoPi * i / 4f;
        Vector2 branchEnd = target.Center + branchAngle.ToRotationVector2() * 60f;
        MagnumVFX.Draw[Theme]Lightning(target.Center, branchEnd, 4, 15f, 1, 0.3f);
    }
    
    // Explosion at each node
    ThemedParticles.[Theme]Impact(target.Center, 1.2f);
    lastPoint = target.Center;
}
```

### Chromatic Distortion Pulse (Reality-Bending Effects)
```csharp
// Creates a visual distortion effect with prismatic edges
for (int layer = 0; layer < 3; layer++)
{
    // RGB separation effect - offset each color slightly
    Vector2[] offsets = { new Vector2(-3, 0), Vector2.Zero, new Vector2(3, 0) };
    Color[] colors = { Color.Red * 0.7f, Color.Green * 0.7f, Color.Blue * 0.7f };
    
    CustomParticles.HaloRing(position + offsets[layer], colors[layer], 0.8f, 25);
}

// Central prismatic burst
CustomParticles.PrismaticSparkleRainbow(position, 12);
CustomParticles.GenericFlare(position, Color.White, 1.0f, 15);
```

### Resonance Wave (Musical Theme Effects)
```csharp
// Sound-wave style expanding rings with music notes
for (int wave = 0; wave < 5; wave++)
{
    float waveDelay = wave * 0.15f;
    Color waveColor = Color.Lerp(primaryColor, secondaryColor, wave / 5f) * (1f - wave * 0.15f);
    CustomParticles.HaloRing(position, waveColor, 0.4f + wave * 0.2f, 18 + wave * 4);
}

// Floating music notes spiral outward
ThemedParticles.[Theme]MusicNotes(position, 8, 50f);
ThemedParticles.[Theme]Accidentals(position, 4, 35f);

// Central chime flare
CustomParticles.GenericFlare(position, Color.White, 0.8f, 25);
```

### Vortex Pull Effect (Gravity/Vacuum Abilities)
```csharp
// Particles spiral inward toward a point
float vortexTimer = (Main.GameUpdateCount * 0.1f) % MathHelper.TwoPi;
for (int particle = 0; particle < 20; particle++)
{
    float angle = vortexTimer + MathHelper.TwoPi * particle / 20f;
    float radius = 150f - (particle * 5f); // Spiral inward
    Vector2 particlePos = center + angle.ToRotationVector2() * radius;
    Vector2 velocity = (center - particlePos).SafeNormalize(Vector2.Zero) * 3f;
    
    float hue = (particle / 20f + Main.GameUpdateCount * 0.01f) % 1f;
    Color vortexColor = Main.hslToRgb(hue, 0.8f, 0.7f);
    
    var glow = new GenericGlowParticle(particlePos, velocity, vortexColor, 0.3f, 15, true);
    MagnumParticleHandler.SpawnParticle(glow);
}
```

### Phoenix Rebirth Burst (Death/Respawn Effects)
```csharp
// Fiery explosion that reforms into a shape
// Phase 1: Explosion outward
CustomParticles.ExplosionBurst(position, primaryColor, 30, 15f);
CustomParticles.ExplosionBurst(position, secondaryColor, 20, 12f);

// Phase 2: Feather/flame rise
for (int i = 0; i < 12; i++)
{
    float angle = MathHelper.TwoPi * i / 12f;
    Vector2 riseVel = new Vector2(0, -4f).RotatedBy(angle * 0.2f);
    CustomParticles.SwanFeatherDrift(position + Main.rand.NextVector2Circular(20f, 20f), primaryColor, 0.4f);
}

// Phase 3: Radiant halo formation
for (int ring = 0; ring < 4; ring++)
{
    CustomParticles.HaloRing(position + new Vector2(0, -ring * 25f), primaryColor * (1f - ring * 0.2f), 0.5f, 20 + ring * 5);
}
```

### Dual-Polarity Contrast (Black/White Swan Lake Effects)
```csharp
// Alternating black and white effects for monochromatic theme
for (int i = 0; i < 12; i++)
{
    float angle = MathHelper.TwoPi * i / 12f;
    Vector2 offset = angle.ToRotationVector2() * 35f;
    
    // Alternate black and white
    Color flareColor = i % 2 == 0 ? Color.White : new Color(20, 20, 30);
    CustomParticles.GenericFlare(position + offset, flareColor, 0.5f, 18);
    
    // Opposite color halo at offset
    Color haloColor = i % 2 == 0 ? new Color(20, 20, 30) : Color.White;
    CustomParticles.HaloRing(position + offset * 0.5f, haloColor * 0.5f, 0.2f, 12);
}

// Rainbow shimmer in the spaces
for (int i = 0; i < 8; i++)
{
    float hue = i / 8f;
    Color rainbow = Main.hslToRgb(hue, 1f, 0.75f);
    CustomParticles.PrismaticSparkle(position + Main.rand.NextVector2Circular(30f, 30f), rainbow, 0.3f);
}
```

### Geometric Mandala Pattern (Meditation/Charge Effects)
```csharp
// Complex layered geometric pattern
float rotationSpeed = Main.GameUpdateCount * 0.02f;

// Inner triangle
for (int i = 0; i < 3; i++)
{
    float angle = rotationSpeed + MathHelper.TwoPi * i / 3f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 20f, primaryColor, 0.4f, 15);
}

// Middle hexagon (opposite rotation)
for (int i = 0; i < 6; i++)
{
    float angle = -rotationSpeed * 0.7f + MathHelper.TwoPi * i / 6f;
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 40f, secondaryColor, 0.35f, 15);
}

// Outer nonagon
for (int i = 0; i < 9; i++)
{
    float angle = rotationSpeed * 0.5f + MathHelper.TwoPi * i / 9f;
    float hue = (i / 9f + Main.GameUpdateCount * 0.01f) % 1f;
    Color outerColor = Main.hslToRgb(hue, 0.9f, 0.8f);
    CustomParticles.GenericFlare(position + angle.ToRotationVector2() * 65f, outerColor, 0.3f, 15);
}

// Pulsing center
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.3f + 0.7f;
CustomParticles.GenericFlare(position, Color.White, pulse, 20);
```

---

## CUSTOM LIGHTING EFFECTS

### Pulsing Aura Light
```csharp
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
Vector3 lightColor = primaryColor.ToVector3();
Lighting.AddLight(position, lightColor * pulse * intensity);
```

### Rainbow Cycling Light
```csharp
float hue = (Main.GameUpdateCount * 0.015f) % 1f;
Vector3 rainbowLight = Main.hslToRgb(hue, 0.8f, 0.6f).ToVector3();
Lighting.AddLight(position, rainbowLight * intensity);
```

### Flickering Fire Light
```csharp
float flicker = Main.rand.NextFloat(0.7f, 1.0f);
Lighting.AddLight(position, 1f * flicker, 0.5f * flicker, 0.2f * flicker);
```

### Dual-Tone Alternating Light
```csharp
bool alternate = (Main.GameUpdateCount / 10) % 2 == 0;
Vector3 lightColor = alternate ? primaryColor.ToVector3() : secondaryColor.ToVector3();
Lighting.AddLight(position, lightColor * intensity);
```

---

## SPECIAL SHADER/VISUAL DISTORTION TECHNIQUES

### Additive Bloom Layering (PreDraw)
```csharp
spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, 
    DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

// Draw multiple scaled layers for bloom effect
float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.15f + 1f;
spriteBatch.Draw(texture, drawPos, null, outerGlowColor * 0.4f, rotation, origin, scale * pulse * 1.4f, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, middleGlowColor * 0.3f, rotation, origin, scale * pulse * 1.2f, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, innerGlowColor * 0.25f, rotation, origin, scale * pulse * 1.05f, SpriteEffects.None, 0f);

spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

### Chromatic Aberration Effect
```csharp
// Draw RGB channels offset for distortion
Vector2 offset = Main.rand.NextVector2Circular(2f, 2f);
spriteBatch.Draw(texture, drawPos + new Vector2(-2, 0), null, Color.Red * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos, null, Color.Green * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
spriteBatch.Draw(texture, drawPos + new Vector2(2, 0), null, Color.Blue * 0.3f, rotation, origin, scale, SpriteEffects.None, 0f);
```

### Trail Echo Effect (Projectile Drawing)
```csharp
// Draw fading afterimages
for (int i = 0; i < ProjectileID.Sets.TrailCacheLength[Projectile.type]; i++)
{
    float progress = (float)i / ProjectileID.Sets.TrailCacheLength[Projectile.type];
    float trailAlpha = 1f - progress;
    float trailScale = 1f - progress * 0.3f;
    Color trailColor = Color.Lerp(primaryColor, secondaryColor) * trailAlpha * 0.6f;
    
    spriteBatch.Draw(texture, Projectile.oldPos[i] - Main.screenPosition, null, 
        trailColor, Projectile.oldRot[i], origin, scale * trailScale, SpriteEffects.None, 0f);
}
```

### Screen Shake Integration - CRITICAL RULES

**⚠️ SCREEN SHAKE IS RESTRICTED - DO NOT USE LIBERALLY**

Screen shake should ONLY be used for:
- ✅ **Weapon charge-up completion** (releasing a charged attack)
- ✅ **Boss phase transitions**
- ✅ **Boss deaths / major enemy deaths**
- ✅ **Ultimate abilities with long cooldowns**

**DO NOT use screen shake for:**
- ❌ Regular weapon swings
- ❌ Normal projectile impacts
- ❌ Standard attack hits
- ❌ Ambient effects
- ❌ Every explosion

```csharp
// ❌ WRONG - Shaking on every hit
public override void OnHitNPC(...) {
    MagnumScreenEffects.AddScreenShake(5f); // NO!
}

// ✅ CORRECT - Only shake on charged release or special trigger
if (chargeComplete) {
    MagnumScreenEffects.AddScreenShake(8f); // Yes, charged attack release
}

// ✅ CORRECT - Boss phase transition
if (phaseTransition) {
    MagnumScreenEffects.AddScreenShake(15f); // Yes, dramatic moment
}
```

When screen shake IS appropriate:
```csharp
// Charged weapon release
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(8f, 15);

// Boss phase transition
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(15f, 30);

// Boss death
player.GetModPlayer<ScreenShakePlayer>()?.AddShake(20f, 40);
```

### Sky Flash Effect (Major Impacts)
```csharp
// Trigger sky flash for dramatic moments
LaCampanellaSkyEffect.TriggerFlash(intensity);
// OR
EroicaSkyEffect.TriggerFlash(intensity);

// Combine with particle burst for full effect
ThemedParticles.[Theme]GrandImpact(position, 2f);
```

---

## PROJECTILE TRAIL COMBINATIONS

### Basic Themed Trail
```csharp
if (Projectile.timeLeft % 3 == 0)
{
    ThemedParticles.[Theme]Trail(Projectile.Center, Projectile.velocity);
    CustomParticles.GenericFlare(Projectile.Center, primaryColor, 0.3f, 12);
}
```

### Heavy Smoke + Spark Trail (Fire Weapons)
```csharp
// Black smoke with golden sparks
if (Main.rand.NextBool(2))
{
    var smoke = new HeavySmokeParticle(
        Projectile.Center, -Projectile.velocity * 0.15f, 
        Color.Black, Main.rand.Next(25, 40), 0.3f, 0.6f, 0.02f, false);
    MagnumParticleHandler.SpawnParticle(smoke);
}
CustomParticles.GenericGlow(Projectile.Center, -Projectile.velocity * 0.1f, primaryColor, 0.25f, 10, true);
```

### Prismatic Sparkle Trail (Magic/Rainbow Weapons)
```csharp
if (Main.rand.NextBool(3))
{
    float hue = (Main.GameUpdateCount * 0.02f + Main.rand.NextFloat()) % 1f;
    Color sparkleColor = Main.hslToRgb(hue, 1f, 0.8f);
    CustomParticles.PrismaticSparkle(Projectile.Center, sparkleColor, 0.25f);
}
```

### Feather Drift Trail (Swan Lake Weapons)
```csharp
if (Main.rand.NextBool(4))
{
    CustomParticles.SwanFeatherDrift(Projectile.Center, Color.White, 0.3f);
    CustomParticles.SwanFeatherDrift(Projectile.Center, Color.Black, 0.25f);
}
```
