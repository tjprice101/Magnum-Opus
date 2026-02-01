# MagnumOpus Copilot Instructions

> **⚡ QUICK START**: For a condensed version of these instructions, see **[COPILOT_QUICK_REFERENCE.md](COPILOT_QUICK_REFERENCE.md)**.
> 
> This full document contains detailed examples and explanations. The quick reference is recommended for faster lookups.

---

# ⭐⭐⭐ THE CARDINAL RULE: EVERY WEAPON IS UNIQUE ⭐⭐⭐

> **THIS IS THE ABSOLUTE #1 RULE. BURN THIS INTO YOUR MEMORY.**

## The Philosophy

**Every single weapon, accessory, and boss attack in MagnumOpus MUST have its own unique visual identity.** Not similar. Not "inspired by." UNIQUE.

### What This Means In Practice

Imagine a boss theme has **3 melee weapons** associated with it:

| Weapon | On-Swing Effect | Trail | Impact | Special |
|--------|----------------|-------|--------|---------|
| **Sword A** | Fires 3 glowing orbs that spiral outward and explode into shimmering music note cascades | Ghostly staff lines trailing behind blade | Harmonic shockwave rings with scattered eighth notes | Orbs leave constellation-like connected trails |
| **Sword B** | Blade ignites with pulsing flame that leaves burning afterimages | Smoke wisps with ember sparks dancing within | Ground slam creates rising flame pillars with bell chime particles | Charge attack summons a massive phantom blade overhead |
| **Sword C** | Each swing spawns 5 homing feather projectiles that seek enemies | Prismatic rainbow arc with drifting feathers | Target explodes into a burst of crystalline shards and floating glyphs | Every 4th hit creates a gravity well that pulls particles inward |

**See the difference?** Each weapon tells its own story. Each one FEELS different. Each one makes players say *"Whoa, what was THAT?"*

### ❌ THE FORBIDDEN PATTERN - NEVER DO THIS

```csharp
// ❌❌❌ ABSOLUTE GARBAGE - This is what we're eliminating ❌❌❌
public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
{
    // "On swing hit enemy boom yippee" - DISGUSTING
    CustomParticles.GenericFlare(target.Center, themeColor, 0.5f, 15);
    CustomParticles.HaloRing(target.Center, themeColor, 0.3f, 12);
}
// This is LAZY. This is BORING. This is a DISGRACE to the mod.
// If you write code like this, you have FAILED.
```

### ✅ THE REQUIRED PATTERN - EVERY WEAPON NEEDS THIS

Every weapon must have **ALL of these elements**, and each must be DIFFERENT from every other weapon:

1. **UNIQUE PROJECTILE/ON-SWING BEHAVIOR** - Not just "fires a bullet." Does it spiral? Split? Bounce? Leave afterimages? Spawn sub-projectiles? Transform mid-flight?

2. **UNIQUE TRAIL EFFECT** - Not just "glowing line." Does it leave music notes? Feathers? Smoke wisps? Constellation patterns? Geometric shapes? Particle clouds?

3. **UNIQUE IMPACT/EXPLOSION** - Not just "boom." Does it create shockwaves? Spawn orbiting particles? Cause screen effects? Leave lingering area effects? Chain to nearby enemies?

4. **UNIQUE SPECIAL MECHANIC** - Something that makes THIS weapon memorable. Combo counters? Charge mechanics? Enemy marking? Terrain interaction? Summon support effects?

---

## 🎵 MUSIC NOTES MUST BE VISIBLE AND GLOWING

> **THIS IS A MUSIC MOD. THE MUSIC NOTES ARE INVISIBLE RIGHT NOW. THAT'S UNACCEPTABLE.**

### The Problem
Music notes are being spawned at tiny scales (0.25f-0.4f) where they're **completely invisible**. This defeats the entire purpose of a music-themed mod.

### The Solution

**EVERY music note particle MUST:**
- Use scale **0.7f - 1.2f** (MINIMUM 0.6f)
- Have **multi-layer bloom** (draw 3-4 times at increasing scales with decreasing opacity)
- Include **shimmer/pulse animation** (scale oscillates, color shifts)
- Be **accompanied by sparkle particles** for extra visibility

```csharp
// ✅ CORRECT - Visible, glowing, shimmering music notes
void SpawnGlowingMusicNote(Vector2 position, Vector2 velocity, Color baseColor)
{
    // Main note - LARGE SCALE
    float scale = Main.rand.NextFloat(0.75f, 1.0f);
    int variant = Main.rand.Next(1, 7); // Use all 6 variants!
    
    // Bloom layers for glow effect
    for (int bloom = 0; bloom < 3; bloom++)
    {
        float bloomScale = scale * (1f + bloom * 0.4f);
        float bloomAlpha = 0.6f / (bloom + 1);
        Color bloomColor = baseColor * bloomAlpha;
        // Spawn bloom particle
    }
    
    // Sparkle accompaniment
    for (int i = 0; i < 3; i++)
    {
        Vector2 sparkleOffset = Main.rand.NextVector2Circular(8f, 8f);
        CustomParticles.PrismaticSparkle(position + sparkleOffset, baseColor, 0.4f, Main.rand.Next(1, 16));
    }
    
    // The actual note with shimmer
    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
    ThemedParticles.MusicNote(position, velocity, baseColor, scale * shimmer, 35, variant);
}
```

---

## 🎨 USE ALL AVAILABLE PARTICLE ASSETS - MANDATORY

> **You have been given 80+ custom particle PNGs. USE THEM.**

### Available Particle Categories (USE ALL OF THESE)

| Category | Files | Variants | MUST USE FOR |
|----------|-------|----------|--------------|
| **MusicNote** | `MusicNote1-6.png` | 6 | EVERY weapon trail, EVERY impact, EVERY aura |
| **EnergyFlare** | `EnergyFlare1-7.png` | 7 | Impacts, projectile cores, charge effects |
| **SoftGlow** | `SoftGlow2-4.png` | 3 | Ambient auras, bloom bases, soft lighting |
| **GlowingHalo** | `GlowingHalo1-6.png` | 5 | Shockwaves, expansion rings, impact halos |
| **StarBurst** | `StarBurst1-2.png` | 2 | Explosions, death effects, critical hits |
| **MagicSparkleField** | `MagicSparkleField1-12.png` | 12 | Magic trails, enchantment effects, auras |
| **PrismaticSparkle** | `PrismaticSparkle1-15.png` | 15 | EVERYWHERE - add sparkle to everything |
| **ParticleTrail** | `ParticleTrail1-4.png` | 4 | Projectile trails, movement effects |
| **SwordArc** | `SwordArc1-9.png` | 9 | Melee swings, slash effects |
| **SwanFeather** | `SwanFeather1-10.png` | 10 | Swan Lake theme, graceful effects |
| **EnigmaEye** | `EnigmaEye1-8.png` | 8 | Enigma theme, watching/targeting effects |
| **Glyphs** | `Glyphs1-12.png` | 12 | Magic circles, enchantments, Fate theme |
| **ShatteredStarlight** | `ShatteredStarlight.png` | 1 | Shatter effects, broken glass visuals |

### ALSO USE Vanilla Terraria Dust (Required for Visual Density)

```csharp
// Combine custom particles WITH vanilla dust for maximum visual density
DustID.MagicMirror      // Magical shimmer
DustID.Enchanted_Gold   // Golden sparkles
DustID.Enchanted_Pink   // Pink magical dust
DustID.PurpleTorch      // Purple flames
DustID.Electric         // Electric sparks
DustID.Frost            // Ice crystals
DustID.FireworkFountain_Yellow // Bright sparks
DustID.GemAmethyst      // Purple gems
DustID.GemSapphire      // Blue gems
DustID.GemRuby          // Red gems
DustID.GemDiamond       // White sparkle
DustID.Pixie            // Fairy dust
DustID.Clentaminator_Purple // Purple spray
```

### The Particle Mixing Rule

**Every visual effect MUST combine:**
1. At least **2 different custom particle types** from `Assets/Particles/`
2. At least **1 vanilla Dust type** for density
3. At least **1 music-related particle** (MusicNote, staff lines, etc.) where thematic
4. **Bloom/glow layers** using SoftGlow or additive blending

---

## Weapon Uniqueness Within Themes

### The Boss Weapon Example

When a boss/theme has multiple weapons of the same class, they MUST be radically different:

**Example: Eroica Theme has 3 Swords**

| Sword | Unique Identity |
|-------|-----------------|
| **Heroic Anthem** | Swings create expanding sound wave rings made of visible music notes. Every 5th hit plays a chord and releases a burst of sakura petals that home to nearby enemies. Trail leaves golden staff lines in the air. |
| **Valor's Edge** | Blade charges with each swing, glowing brighter (visible bloom layers). At full charge, next swing releases a massive phantom blade projectile that passes through enemies leaving ember trails. Impact creates rising flame pillars. |
| **Triumph's Melody** | Each swing spawns 3 small orbiting note projectiles that circle the player. Using special attack releases all accumulated notes as a spiraling barrage. Notes leave prismatic sparkle trails. |

**See how DIFFERENT these are?** Same theme colors. Same general aesthetic. But completely unique mechanics and visuals.

---

## VFX Design Workflow

When creating ANY new content, follow this process:

### Step 1: Catalog Existing Effects
Before designing, LIST all existing weapons/effects in that theme to ensure no duplication.

### Step 2: Brainstorm Unique Concept
Design something that has NEVER been done in this mod before. Ask:
- What particle combinations haven't been used?
- What movement patterns are fresh?
- What special mechanics would surprise players?

### Step 3: Select Particle Mix
Choose specific particles from the catalog:
- Which MusicNote variants? (1-6)
- Which EnergyFlare variants? (1-7)
- Which special theme particles?
- Which vanilla Dust types?

### Step 4: Design the Layers
Plan the visual layers:
- Core effect (main visual)
- Bloom/glow (multi-layer)
- Sparkle accents
- Trail elements
- Impact explosion
- Special mechanic visuals

### Step 5: Implement with Visible Music Notes
Ensure all music notes use scale 0.7f+ with bloom layers.

---

## ⭐⭐⭐ CRITICAL: PROJECTILE VFX - READ THIS FIRST ⭐⭐⭐

> **THIS IS THE #1 RULE. Every projectile must be a visual masterpiece.**

### EVERY PROJECTILE MUST COMBINE MULTIPLE EFFECTS

**Every weapon projectile, particle trail, and visual effect MUST use multiple effects in combination to look pretty and uniquely brilliant.** A single effect type is never enough—layer them, combine them, make them sing together.

**Always combine these elements:**

**EVERY boss projectile MUST have:**

1. **UNIQUE VISUAL IDENTITY** - Each projectile type must look distinctly different
2. **MULTIPLE PARTICLE TYPES** - Combine flares, sparkles, dust, glyphs, glows
3. **DUST PARTICLES** - Use vanilla `Dust` types (DustID.Torch, DustID.MagicMirror, DustID.PurpleTorch, DustID.Electric, DustID.Frost, etc.)
4. **SPARKLE PARTICLES** - Use `SparkleParticle` for magical shine
5. **LAYERED BLOOM** - Multiple bloom layers at different scales
6. **THEME-SPECIFIC EFFECTS** - Glyphs, music notes, sakura petals, feathers, etc.

```csharp
// ❌ TOO SIMPLE - Single effect lacks visual impact
public override void OnKill(int timeLeft)
{
    CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.5f, 15);
    CustomParticles.HaloRing(Projectile.Center, PrimaryColor, 0.3f, 12);
}

// ✅ BEAUTIFUL - Rich, layered, unique VFX combining multiple effects
public override void OnKill(int timeLeft)
{
    // Central flash cascade
    EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.7f, 18, 4, 1f);
    EnhancedParticles.BloomFlare(Projectile.Center, PrimaryColor, 0.55f, 22, 3, 0.8f);
    
    // Sparkle burst
    for (int i = 0; i < 10; i++)
    {
        float angle = MathHelper.TwoPi * i / 10f;
        Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
        var sparkle = new SparkleParticle(Projectile.Center, burstVel, AccentColor, 0.4f, 25);
        MagnumParticleHandler.SpawnParticle(sparkle);
        
        // VANILLA DUST - Required for visual richness
        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, burstVel * 1.2f, 0, PrimaryColor, 1.1f);
        dust.noGravity = true;
    }
    
    // Glow particles
    for (int i = 0; i < 6; i++)
    {
        var glow = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(5f, 5f),
            SecondaryColor * 0.8f, 0.3f, 22, true);
        MagnumParticleHandler.SpawnParticle(glow);
    }
}
```

### PROJECTILE AI MUST INCLUDE:
- Orbiting visual elements (spark points, runes, glyphs)
- Trail particles with variety (not just one type)
- Dust particles for visual density
- Sparkle accents
- Dynamic lighting that pulses

```csharp
// ❌ TOO SIMPLE - Single-effect trail lacks depth
if (Main.rand.NextBool(3))
{
    var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, PrimaryColor * 0.6f, 0.25f, 15, true);
    MagnumParticleHandler.SpawnParticle(trail);
}

// ✅ BEAUTIFUL - Rich, layered trail combining multiple effects
// Orbiting spark points
if (Projectile.timeLeft % 6 == 0)
{
    for (int i = 0; i < 3; i++)
    {
        float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
        Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 18f;
        EnhancedParticles.BloomFlare(sparkPos, AccentColor, 0.2f, 8, 2, 0.6f);
    }
}

// Sparkle dust trail
if (Main.rand.NextBool(2))
{
    var sparkle = new SparkleParticle(Projectile.Center + dustOffset, velocity, SecondaryColor, 0.35f, 20);
    MagnumParticleHandler.SpawnParticle(sparkle);
    
    // Dust for density
    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, velocity * 0.5f, 0, PrimaryColor, 0.8f);
    dust.noGravity = true;
}

// Gradient glow particles
if (Main.rand.NextBool(3))
{
    Color trailColor = Color.Lerp(PrimaryColor, AccentColor, Main.rand.NextFloat());
    var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor * 0.7f, 0.28f, 18, true);
    MagnumParticleHandler.SpawnParticle(trail);
}
```

---

## 🎵 THE HEART OF MAGNUM OPUS - DESIGN PHILOSOPHY

> *"This mod is based around music. It's based around how it connects to your heart, and it's based around how it impacts the world."*

### The Soul of This Modpack

MagnumOpus is not just a content mod—it is **a symphony made playable**. Every weapon, every effect, every particle should make players *feel* the music. When a sword swings, players should see **trails of music notes dancing in the blade's wake**. When a gun fires, projectiles should leave **lingering musical echoes in the air** before fading like the final note of a crescendo.

### Core Commandments

1. **EVERY WEAPON IS UNIQUE** - Within a theme, if there are 3 swords, each sword has COMPLETELY different effects. Different projectiles, different trails, different impacts, different special mechanics. NO SHARING.

2. **MUSIC NOTES MUST BE VISIBLE** - Scale 0.7f minimum. Multi-layer bloom. Shimmer effects. If players can't see the music notes, you've failed. This is a MUSIC mod.

3. **USE ALL PARTICLE ASSETS** - You have 80+ custom PNGs. MusicNote (6 variants), EnergyFlare (7 variants), PrismaticSparkle (15 variants), Glyphs (12 variants), etc. USE THEM ALL. Mix and match creatively.

4. **LAYER EVERYTHING** - Single-effect particles are LAZY. Every effect needs: core + bloom layers + sparkle accents + theme particles + vanilla dust for density.

5. **THEME COLORS STAY CONSISTENT** - La Campanella is always black/orange/gold. Eroica is always scarlet/gold. But HOW effects work is completely creative and unique per weapon.

6. **NO "ON HIT BOOM DONE"** - That pattern is FORBIDDEN. Every impact must be a visual symphony with multiple phases, particle types, and memorable qualities.

### The Uniqueness Mandate - EVERY WEAPON DIFFERENT

**Within a single boss/theme, weapons of the same class MUST be radically different:**

```
EROICA THEME - 3 MELEE WEAPONS:

Sword A: "Heroic Crescendo"
- On swing: Releases expanding sound wave rings made of visible music notes
- Trail: Golden staff lines that linger in the air
- Impact: Sakura petal explosion with homing note projectiles
- Special: Every 5th hit plays a chord and causes screen-wide harmonic pulse

Sword B: "Valiant Flame"  
- On swing: Blade leaves burning afterimages that deal damage
- Trail: Ember particles with smoke wisps swirling around them
- Impact: Rising flame pillars from the ground
- Special: Charge attack summons phantom blades that orbit and strike

Sword C: "Phoenix Edge"
- On swing: Spawns 3 orbiting flame orbs that grow larger
- Trail: Feather-shaped flames with prismatic sparkle accents
- Impact: Enemy marked with burning glyph, marked enemies chain damage
- Special: At low HP, sword transforms with enhanced visuals
```

**See how NONE of these share effects?** Same theme. Same colors. Completely unique experiences.

### The Visual Test

Before implementing ANY effect, ask yourself:

1. *"Is there ANY other weapon in this mod that does something similar?"* → If yes, redesign.
2. *"Can players SEE the music notes clearly?"* → If scale < 0.7f, increase it.
3. *"Am I using at least 3-4 different particle types?"* → If not, add more.
4. *"Would a player remember this weapon's effect specifically?"* → If it's generic, redesign.

### What Makes Effects Memorable

**BORING (Never Do This):**
- Fires a projectile → hits enemy → small explosion
- Swing sword → trail follows → damage dealt
- Passive buff → occasional sparkle

**MEMORABLE (Always Do This):**
- Fires 3 spiraling projectiles that split into 5 smaller ones, each trailing music notes, then converge on target creating a harmonic explosion with screen shake
- Swing creates visible arc of staff lines and floating notes, blade tip leaves constellation trail, impacts spawn orbiting glyphs that attack nearby enemies
- Passive creates visible aura of slowly orbiting music notes, damage taken causes notes to scatter defensively, healing causes notes to sing (visible pulse effect)

---

## ⭐⭐⭐ CRITICAL: PROJECTILE VFX - VISUAL MASTERPIECES ⭐⭐⭐

> **Every projectile must be a visual masterpiece. Not a glowing dot. A MASTERPIECE.**

### The Projectile Uniqueness Rule

If a theme has 5 weapons that fire projectiles, those 5 projectiles must be **COMPLETELY DIFFERENT**:

| Weapon | Projectile Concept |
|--------|-------------------|
| Weapon 1 | Spinning music note that leaves staff line trails, orbited by 3 smaller sparkles |
| Weapon 2 | Pulsing orb of cosmic energy with glyph particles orbiting, explodes into constellation |
| Weapon 3 | Flame bolt that splits into 3 homing embers, each trailing smoke wisps |
| Weapon 4 | Feather projectile that floats gracefully, leaving prismatic sparkle trail, bounces between enemies |
| Weapon 5 | Sound wave projectile (circular expanding), visible ripples with music notes embedded |

### EVERY Projectile MUST Have:

```csharp
// ✅ THE REQUIRED ELEMENTS FOR EVERY PROJECTILE

public override void AI()
{
    // 1. CORE VISUAL - The main "body" of the projectile (visible, glowing)
    float pulse = 1f + (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.2f;
    CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.5f * pulse, 8);
    
    // 2. ORBITING ELEMENTS - Something that circles the projectile
    float orbitAngle = Main.GameUpdateCount * 0.08f;
    for (int i = 0; i < 3; i++)
    {
        float angle = orbitAngle + MathHelper.TwoPi * i / 3f;
        Vector2 orbitPos = Projectile.Center + angle.ToRotationVector2() * 15f;
        CustomParticles.PrismaticSparkle(orbitPos, AccentColor, 0.35f, Main.rand.Next(1, 16));
    }
    
    // 3. TRAIL PARTICLES - Multiple types, not just one
    if (Main.rand.NextBool(2))
    {
        // Primary trail
        var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, 
            PrimaryColor * 0.7f, 0.3f, 20, true);
        MagnumParticleHandler.SpawnParticle(trail);
        
        // Secondary sparkle trail  
        CustomParticles.MagicSparkleField(Projectile.Center, SecondaryColor, 0.25f, Main.rand.Next(1, 13));
    }
    
    // 4. MUSIC NOTE TRAIL - This is a music mod! (VISIBLE SCALE)
    if (Main.rand.NextBool(3))
    {
        SpawnGlowingMusicNote(Projectile.Center, -Projectile.velocity * 0.05f, PrimaryColor);
    }
    
    // 5. VANILLA DUST - For visual density
    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, 
        -Projectile.velocity * 0.2f, 0, PrimaryColor, 0.8f);
    dust.noGravity = true;
    
    // 6. DYNAMIC LIGHTING - Projectiles should glow
    Lighting.AddLight(Projectile.Center, PrimaryColor.ToVector3() * 0.8f);
}

public override void OnKill(int timeLeft)
{
    // 7. UNIQUE DEATH EFFECT - Not just "explosion"
    
    // Central flash cascade (3 layers)
    CustomParticles.GenericFlare(Projectile.Center, Color.White, 1.0f, 20);
    CustomParticles.GenericFlare(Projectile.Center, PrimaryColor, 0.8f, 25);
    CustomParticles.GenericFlare(Projectile.Center, SecondaryColor, 0.6f, 22);
    
    // Expanding halo rings (gradient)
    for (int i = 0; i < 4; i++)
    {
        Color ringColor = Color.Lerp(PrimaryColor, SecondaryColor, i / 4f);
        CustomParticles.HaloRing(Projectile.Center, ringColor, 0.3f + i * 0.15f, 15 + i * 3);
    }
    
    // Music note burst (VISIBLE)
    for (int i = 0; i < 6; i++)
    {
        float angle = MathHelper.TwoPi * i / 6f;
        Vector2 noteVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
        SpawnGlowingMusicNote(Projectile.Center, noteVel, 
            Color.Lerp(PrimaryColor, SecondaryColor, i / 6f));
    }
    
    // Sparkle scatter
    for (int i = 0; i < 12; i++)
    {
        Vector2 sparkleVel = Main.rand.NextVector2Circular(8f, 8f);
        CustomParticles.PrismaticSparkle(Projectile.Center, PrimaryColor, 0.4f, Main.rand.Next(1, 16));
    }
    
    // Glyph accent
    CustomParticles.GlyphBurst(Projectile.Center, AccentColor, 4, 5f);
    
    // Vanilla dust burst
    for (int i = 0; i < 15; i++)
    {
        Vector2 dustVel = Main.rand.NextVector2Circular(10f, 10f);
        Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Enchanted_Gold, dustVel, 0, PrimaryColor, 1.2f);
        dust.noGravity = true;
    }
}
```

### Projectile Identity Examples

**La Campanella Projectile - "Infernal Bell Toll"**
- Core: Pulsing orange orb with black smoke center
- Orbiting: 4 small flame wisps that leave ember trails
- Trail: Heavy black smoke + orange sparks + occasional bell-shaped flare
- Impact: Smoke explosion with rising flame pillar and bell chime visual (expanding ring)

**Moonlight Sonata Projectile - "Lunar Whisper"**
- Core: Soft purple glow with silver halo
- Orbiting: Crescent moon shapes that rotate slowly
- Trail: Purple mist particles + silver sparkles + fading music notes
- Impact: Gentle bloom explosion, lunar halo expands, soft particle rain

**Fate Projectile - "Cosmic Decree"**
- Core: Black void center with pink/red energy crackling at edges
- Orbiting: Ancient glyphs that grow brighter over time
- Trail: Cosmic cloud nebula + star sparkles + constellation line connections
- Impact: Reality crack effect, glyph explosion, star scatter, chromatic aberration pulse

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

### 🚀 MAGNUMOPUS ENHANCED VFX SYSTEM (IMPLEMENTATION)

The above FargosSoulsDLC patterns have been **IMPLEMENTED** into MagnumOpus's VFX system. **Use this guide for practical implementation:**

| Document | Contents | When to Use |
|----------|----------|-------------|
| [Enhanced_VFX_System.md](../Documentation/Guides/Enhanced_VFX_System.md) | MagnumOpus implementation of FargosSoulsDLC patterns: `{ A = 0 }` alpha removal, multi-layer bloom, theme palettes, EnhancedParticle, UnifiedVFXBloom API | **ALWAYS** - When creating any VFX, particles, bloom effects, or themed visuals in MagnumOpus |

**Key files in `Common/Systems/VFX/`:**
- `VFXUtilities.cs` - `WithoutAlpha()` extension, math utilities
- `BloomRenderer.cs` - `DrawMultiLayerBloom()` for easy bloom stacking  
- `MagnumThemePalettes.cs` - All theme color arrays for gradient lerping
- `EnhancedParticle.cs` - Particle class with built-in bloom support
- `UnifiedVFXBloom.cs` - High-level API: `UnifiedVFXBloom.[Theme].BloomImpact()`

### MANDATORY: Read Before Implementing VFX

**Before implementing ANY of the following, READ the corresponding documents:**

| Task | Required Reading |
|------|------------------|
| **Any VFX in MagnumOpus** | [Enhanced_VFX_System.md](../Documentation/Guides/Enhanced_VFX_System.md) - **START HERE** for MagnumOpus-specific implementation |
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

## ⭐⭐⭐ CRITICAL: PARTICLE ASSET DISCOVERY - MANDATORY ⭐⭐⭐

> **BEFORE creating ANY weapon, projectile, or effect, you MUST explore the `Assets/Particles/` folder to discover ALL available particle textures.**

### MANDATORY: Always Explore Available Assets

**When creating new weapons or effects:**
1. **USE `list_dir` on `Assets/Particles/`** to see ALL available particle textures
2. **MIX AND MATCH** different particle types - never use just one
3. **USE VARIANT NUMBERS** - Most particles have 2-15 variants (e.g., `MusicNote1.png` through `MusicNote6.png`)
4. **BE CREATIVE** - Combine unexpected particle types for unique effects
5. **UPDATE DOCUMENTATION** - When new particles are added, catalog them below

### Why This Matters

The `Assets/Particles/` folder contains **80+ unique particle textures** across many categories. Using only `GenericFlare` or `SoftGlow` creates boring, repetitive weapons. **Every weapon deserves a unique visual identity.**

```csharp
// ❌ BORING - Same generic glow on every weapon
CustomParticles.GenericFlare(pos, color, 0.5f, 15);

// ✅ UNIQUE - Mix of music notes, sparkles, trails, and flares
// First: Check Assets/Particles/ folder for available textures!
CustomParticles.MusicNote(pos, vel, color, 0.8f, 20, noteVariant: 3); // Use variant 3
CustomParticles.PrismaticSparkle(pos, color, 0.5f, sparkleVariant: 7);
CustomParticles.ParticleTrail(pos, vel, color, 0.4f, trailVariant: 2);
CustomParticles.EnergyFlare(pos, color, 0.6f, 18, flareVariant: 5);
```

---

## Custom Particle System Overview

This mod uses a custom particle system located at `Common/Systems/Particles/`. 

### ⭐ COMPLETE Particle Asset Catalog (Assets/Particles/)

> **CRITICAL: This list may be incomplete. ALWAYS run `list_dir` on `Assets/Particles/` to find all current assets.**

#### Flares & Glows (Layer these for bloom effects)
| File Pattern | Variants | Purpose | Recommended Scale |
|--------------|----------|---------|-------------------|
| `EnergyFlare1-7.png` | 7 | Intense bright bursts, each with unique shape | 0.4f - 1.2f |
| `SoftGlow2-4.png` | 3 | Subtle ambient glows, soft edges | 0.3f - 0.8f |
| `GlowingHalo1-6.png` | 5 | Ring/halo effects for impacts | 0.3f - 1.0f |
| `StarBurst1-2.png` | 2 | Radial star explosions | 0.5f - 1.5f |
| `ShatteredStarlight.png` | 1 | Broken star fragments | 0.4f - 1.0f |

#### Music Notes (THIS IS A MUSIC MOD - USE THESE!)
| File Pattern | Variants | Purpose | Recommended Scale |
|--------------|----------|---------|-------------------|
| `MusicNote1-6.png` | 6 | Different musical note shapes | **0.6f - 1.2f** (NOT 0.25f!) |

> ⚠️ **CRITICAL: Music notes at 0.25-0.4f scale are INVISIBLE. Use 0.6f minimum!**

#### Magic & Sparkles (For magical/enchanted effects)
| File Pattern | Variants | Purpose | Recommended Scale |
|--------------|----------|---------|-------------------|
| `MagicSparklField1-12.png` | 12 | Magic sparkle clusters, fields | 0.3f - 0.8f |
| `PrismaticSparkle1-15.png` | 15 | Rainbow/prismatic sparkle points | 0.3f - 0.7f |

#### Trails (For projectile/movement trails)
| File Pattern | Variants | Purpose | Recommended Scale |
|--------------|----------|---------|-------------------|
| `ParticleTrail1-4.png` | 4 | Elongated trail effects | 0.3f - 0.8f |
| `SwordArc1-9.png` | 9 | Melee swing arcs/smears | 0.5f - 1.5f |

#### Theme-Specific Particles
| File Pattern | Variants | Purpose | Recommended Scale |
|--------------|----------|---------|-------------------|
| `SwanFeather1-10.png` | 10 | Feathers for Swan Lake theme | 0.4f - 1.0f |
| `EnigmaEye1-8.png` | 8 | Watching eyes for Enigma theme | 0.4f - 0.8f |
| `Glyphs1-12.png` | 12 | Arcane symbols for magic effects | 0.3f - 0.7f |

### Creativity Guidelines - MAKE EACH WEAPON UNIQUE

**When designing weapon effects, ask yourself:**
1. What particle types would make this weapon feel DIFFERENT from others?
2. Am I using at least 3-4 different particle categories?
3. Have I explored ALL available variants, not just variant 1?
4. Are my music notes VISIBLE (scale 0.6f+)?
5. Does this weapon have its own visual identity?

**Example: Creating a unique magic staff effect**
```csharp
// Explore what's available first!
// Assets/Particles/ contains: MagicSparklField (12 variants), PrismaticSparkle (15 variants),
// MusicNote (6 variants), Glyphs (12 variants), EnergyFlare (7 variants)...

// Now MIX them creatively:
public override void AI()
{
    // Orbiting glyphs (pick random variant 1-12)
    if (Timer % 10 == 0)
        CustomParticles.Glyph(orbitPos, color, 0.5f, glyphIndex: Main.rand.Next(1, 13));
    
    // Sparkle field trail (use variant 7 for this weapon's identity)
    if (Main.rand.NextBool(3))
        CustomParticles.MagicSparkleField(Projectile.Center, color, 0.4f, variant: 7);
    
    // Visible music notes (scale 0.8f, use variant 4)
    if (Main.rand.NextBool(4))
        CustomParticles.MusicNote(Projectile.Center, trailVel, color, 0.8f, 25, variant: 4);
    
    // Prismatic accents (variant 12 for unique look)
    CustomParticles.PrismaticSparkle(Projectile.Center, rainbow, 0.5f, variant: 12);
}
```

### Maintaining This Catalog

> **IMPORTANT: When you add new particle assets to `Assets/Particles/`, UPDATE this documentation!**
> 
> Add new entries to the appropriate category table above, including:
> - File pattern with variant range
> - Number of variants
> - Purpose description  
> - Recommended scale range

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

## VFX Requirements - CREATIVE FREEDOM WITH RESPONSIBILITY

> *"Give the weapons the trails, the projectiles, the music notes, the particles, the lighting, the dynamicism, the shaders, the waving of the screen as it distorts—give them every ounce of creativity that you have."*

### The Golden Rules

1. **EVERY WEAPON IS UNIQUE** - No two weapons share effects. Period.
2. **USE ALL PARTICLE ASSETS** - 80+ custom PNGs. Use them. All of them. Creatively.
3. **MUSIC NOTES ARE VISIBLE** - Scale 0.7f+, multi-layer bloom, shimmer animation.
4. **LAYER EFFECTS** - Minimum 3-4 particle types per effect.
5. **THEME COLORS ONLY** - Consistent palette, but creative implementation.

### Accessory VFX Requirements (Same Philosophy)

**Accessories follow the SAME uniqueness rule.** If a theme has 5 accessories, each one has completely different visual effects.

**Exception:** Chain accessories or set bonuses that are explicitly designed to work together can share visual language, but even then should have variation.

| Accessory | VFX Concept |
|-----------|-------------|
| Accessory A | Orbiting music notes that attack enemies on proc |
| Accessory B | Passive flame aura that intensifies with damage taken |
| Accessory C | Feather shield that appears on hit, feathers scatter when broken |
| Accessory D | Glyph circles that rotate around player, pulse on ability use |
| Accessory E | Constellation pattern that forms between the player and nearby enemies |

### Boss VFX Requirements (Maximum Creativity)

**Boss attacks must be the MOST visually impressive content in the mod.**

Each boss attack should have:
1. **UNIQUE TELEGRAPH** - Player knows what's coming, but it looks amazing
2. **UNIQUE EXECUTION** - The attack itself is visually distinct
3. **UNIQUE IMPACT** - When it hits, it's spectacular
4. **PARTICLE VARIETY** - Multiple custom particle types + vanilla dust

**No two boss attacks should look similar.** A boss with 8 attacks needs 8 completely different visual experiences.

### The Music Note Mandate (CRITICAL)

**THIS IS A MUSIC MOD. MUSIC NOTES MUST BE EVERYWHERE AND VISIBLE.**

```csharp
// ❌ INVISIBLE - Scale too small
ThemedParticles.MusicNote(pos, vel, color, 0.25f, 20, variant: 1); // CAN'T SEE THIS

// ❌ BORING - Only one variant, no glow
ThemedParticles.MusicNote(pos, vel, color, 0.7f, 20, variant: 1);

// ✅ VISIBLE AND BEAUTIFUL - Proper scale, random variants, bloom layers
void SpawnGlowingMusicNote(Vector2 position, Vector2 velocity, Color baseColor)
{
    float scale = Main.rand.NextFloat(0.7f, 1.0f);
    int variant = Main.rand.Next(1, 7); // All 6 variants
    
    // Shimmer effect
    float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f;
    scale *= shimmer;
    
    // Bloom layers (3 layers for glow)
    Color bloom1 = baseColor with { A = 0 } * 0.4f;
    Color bloom2 = baseColor with { A = 0 } * 0.25f;
    // Draw bloom layers at scale * 1.3f, scale * 1.6f
    
    // Sparkle companions
    for (int i = 0; i < 2; i++)
    {
        Vector2 sparkleOffset = Main.rand.NextVector2Circular(10f, 10f);
        CustomParticles.PrismaticSparkle(position + sparkleOffset, baseColor, 0.3f, Main.rand.Next(1, 16));
    }
    
    // The note itself
    ThemedParticles.MusicNote(position, velocity, baseColor, scale, 35, variant);
}
```

**EVERY weapon trail, EVERY impact, EVERY aura should include music notes.**

### Available Vanilla Dust Types (USE THESE FOR DENSITY)

```csharp
// ALWAYS combine custom particles with vanilla dust for visual richness

// Magical/Sparkle
DustID.MagicMirror          // Shimmering magic
DustID.Enchanted_Gold       // Golden sparkles  
DustID.Enchanted_Pink       // Pink magic
DustID.GemDiamond           // White sparkle
DustID.Pixie                // Fairy dust

// Colored Flames
DustID.PurpleTorch          // Purple flames
DustID.BlueTorch            // Blue flames
DustID.YellowTorch          // Yellow flames
DustID.CursedTorch          // Green flames

// Energy/Electric
DustID.Electric             // Electric sparks
DustID.Vortex               // Vortex energy
DustID.SolarFlare           // Solar particles

// Elemental
DustID.Frost                // Ice crystals
DustID.Water                // Water droplets
DustID.Smoke                // Smoke puffs

// Gems (colored sparkles)
DustID.GemAmethyst          // Purple gems
DustID.GemSapphire          // Blue gems
DustID.GemRuby              // Red gems
DustID.GemEmerald           // Green gems
DustID.GemTopaz             // Yellow gems

// Special
DustID.FireworkFountain_Yellow  // Bright fountain sparks
DustID.Clentaminator_Purple     // Purple spray
DustID.RainbowMk2               // Rainbow particles
DustID.Confetti                 // Celebration particles
```

### Particle Mixing Formula

**Every visual effect should follow this formula:**

```csharp
void CreateRichVisualEffect(Vector2 position, Color primaryColor, Color secondaryColor)
{
    // LAYER 1: Core effect (custom particle)
    CustomParticles.GenericFlare(position, primaryColor, 0.8f, 20);
    
    // LAYER 2: Bloom/glow (custom particle with alpha removal)
    CustomParticles.SoftGlow(position, primaryColor with { A = 0 }, 0.6f, 25);
    
    // LAYER 3: Theme particle (varies by theme)
    // La Campanella: HeavySmoke + EnergyFlare
    // Eroica: SakuraPetals + EnergyFlare
    // Swan Lake: SwanFeather + PrismaticSparkle
    // Moonlight: SoftGlow + MagicSparkleField
    // Enigma: EnigmaEye + Glyphs
    // Fate: Glyphs + StarSparkle + CosmicCloud
    
    // LAYER 4: Music notes (VISIBLE SCALE)
    SpawnGlowingMusicNote(position, Vector2.Zero, primaryColor);
    
    // LAYER 5: Vanilla dust (multiple types)
    for (int i = 0; i < 8; i++)
    {
        Vector2 dustVel = Main.rand.NextVector2Circular(6f, 6f);
        Dust dust1 = Dust.NewDustPerfect(position, DustID.MagicMirror, dustVel, 0, primaryColor, 1f);
        dust1.noGravity = true;
        
        Dust dust2 = Dust.NewDustPerfect(position, DustID.Enchanted_Gold, dustVel * 0.7f, 0, secondaryColor, 0.8f);
        dust2.noGravity = true;
    }
    
    // LAYER 6: Sparkle accents
    for (int i = 0; i < 5; i++)
    {
        Vector2 sparklePos = position + Main.rand.NextVector2Circular(20f, 20f);
        CustomParticles.PrismaticSparkle(sparklePos, primaryColor, 0.35f, Main.rand.Next(1, 16));
    }
    
    // LAYER 7: Dynamic lighting
    Lighting.AddLight(position, primaryColor.ToVector3() * 1.2f);
}
```

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

## ⚠️ CRITICAL: ITEM TOOLTIPS - ModifyTooltips MANDATORY

> **THIS IS A CRITICAL RULE. Items without tooltips appear EMPTY to players.**

### The Problem: Empty Tooltips

The localization file (`en-US_Mods.MagnumOpus.hjson`) contains auto-generated `Tooltip: ""` entries for items. **These are placeholders that result in EMPTY tooltips in-game.** The actual tooltips MUST be defined in code using the `ModifyTooltips` method.

### EVERY Item MUST Have ModifyTooltips

**When creating ANY item (weapons, accessories, materials, etc.), you MUST add a `ModifyTooltips` override:**

```csharp
// ❌ WRONG - Item has NO tooltip method (appears empty in-game)
public class MyNewAccessory : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
    }
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.statDefense += 5;
    }
    // NO ModifyTooltips = EMPTY TOOLTIP IN-GAME!
}

// ✅ CORRECT - Item has proper tooltip implementation
public class MyNewAccessory : ModItem
{
    public override void SetDefaults()
    {
        Item.width = 30;
        Item.height = 30;
        Item.accessory = true;
    }
    
    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.statDefense += 5;
    }
    
    public override void ModifyTooltips(List<TooltipLine> tooltips)
    {
        tooltips.Add(new TooltipLine(Mod, "Effect1", "+5 defense"));
        tooltips.Add(new TooltipLine(Mod, "Lore", "'A sturdy shield against the darkness'") 
        { 
            OverrideColor = new Color(100, 180, 255) 
        });
    }
}
```

### Required Using Statement

**Add this import at the top of ANY file that uses ModifyTooltips:**

```csharp
using System.Collections.Generic;
```

### Tooltip Structure Pattern

**Every item should follow this tooltip pattern:**

```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    // Effect lines - describe what the item DOES
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Primary effect description"));
    tooltips.Add(new TooltipLine(Mod, "Effect2", "Secondary effect description"));
    tooltips.Add(new TooltipLine(Mod, "Effect3", "Tertiary effect (if applicable)"));
    
    // Lore line - themed flavor text with colored text
    tooltips.Add(new TooltipLine(Mod, "Lore", "'Poetic flavor text here'") 
    { 
        OverrideColor = ThemeColor // Use appropriate theme color
    });
}
```

### Theme Color Reference for Lore Lines

| Theme | Lore Color |
|-------|------------|
| Spring | `new Color(255, 180, 200)` - Pink |
| Summer | `new Color(255, 140, 50)` - Orange |
| Autumn | `new Color(200, 150, 80)` - Amber |
| Winter | `new Color(150, 200, 255)` - Ice Blue |
| Moonlight Sonata | `new Color(140, 100, 200)` - Purple |
| Eroica | `new Color(200, 50, 50)` - Scarlet |
| La Campanella | `new Color(255, 140, 40)` - Infernal Orange |
| Enigma Variations | `new Color(140, 60, 200)` - Void Purple |
| Swan Lake | `new Color(240, 240, 255)` - Pure White |
| Fate | `new Color(180, 40, 80)` - Cosmic Crimson |

### Tooltip Checklist for New Items

Before considering an item complete, verify:

- [ ] `using System.Collections.Generic;` is at the top of the file
- [ ] `ModifyTooltips(List<TooltipLine> tooltips)` method exists
- [ ] At least one `Effect` line describing what the item does
- [ ] A `Lore` line with themed colored text
- [ ] Tooltip text matches actual item functionality (read UpdateAccessory/SetDefaults)

### Common Tooltip Patterns by Item Type

**Accessories:**
```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Primary bonus (+X stat, etc.)"));
    tooltips.Add(new TooltipLine(Mod, "Effect2", "Special mechanic description"));
    tooltips.Add(new TooltipLine(Mod, "Lore", "'Themed flavor text'") { OverrideColor = ThemeColor });
}
```

**Weapons:**
```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Special attack pattern/mechanic"));
    tooltips.Add(new TooltipLine(Mod, "Effect2", "On-hit effect or bonus damage"));
    tooltips.Add(new TooltipLine(Mod, "Lore", "'Epic weapon lore'") { OverrideColor = ThemeColor });
}
```

**Materials:**
```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    tooltips.Add(new TooltipLine(Mod, "Effect1", "'Material for crafting [Theme] equipment'"));
}
```

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

## ⭐ BOSS ATTACK DESIGN PATTERNS - THE GOLD STANDARD ⭐

> **THIS IS HOW BOSS ATTACKS SHOULD LOOK AND PLAY.**
> Reference: `Content/Eroica/Bosses/EroicasRetribution.cs` - Attack_HeroesJudgment

### Exemplary Attack Pattern: Hero's Judgment

This attack is the **gold standard** for boss attack design. It has:

1. **CHARGE PHASE** - Visual buildup with converging particles
2. **SAFE ZONE INDICATORS** - Player knows WHERE to dodge
3. **MULTI-WAVE RELEASE** - Not just one burst, but escalating waves
4. **SAFE ARC EXEMPTION** - A gap in the projectiles for skilled players to exploit

```csharp
// ✅ GOLD STANDARD - Hero's Judgment Attack Pattern
private void Attack_HeroesJudgment(Player target)
{
    int chargeTime = 90 - difficultyTier * 10;
    int waveCount = 2 + difficultyTier;
    
    // === PHASE 0: CHARGE WITH CONVERGING PARTICLES ===
    if (SubPhase == 0)
    {
        NPC.velocity *= 0.95f; // Slow down during charge
        
        if (Timer == 1)
        {
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, NPC.Center);
            Main.NewText("Witness the Hero's Judgment!", EroicaGold);
        }
        
        float progress = Timer / (float)chargeTime;
        
        // CONVERGING PARTICLE RING - shrinks as charge builds
        if (Timer % 4 == 0)
        {
            int particleCount = (int)(6 + progress * 10);
            for (int i = 0; i < particleCount; i++)
            {
                float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.05f;
                float radius = 200f * (1f - progress * 0.5f); // Shrinks toward boss
                Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                Color color = Color.Lerp(EroicaGold, Color.White, progress);
                CustomParticles.GenericFlare(pos, color, 0.3f + progress * 0.3f, 12);
            }
        }
        
        // SAFE ZONE INDICATOR - cyan flares show player where to stand
        if (Timer > chargeTime / 2)
        {
            float safeRadius = 100f;
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f + Timer * 0.03f;
                Vector2 safePos = target.Center + angle.ToRotationVector2() * safeRadius;
                CustomParticles.GenericFlare(safePos, Color.Cyan * 0.6f, 0.25f, 5);
            }
        }
        
        // Screen shake builds near end of charge
        if (Timer > chargeTime * 0.7f)
        {
            MagnumScreenEffects.AddScreenShake(progress * 5f);
        }
        
        if (Timer >= chargeTime)
        {
            Timer = 0;
            SubPhase = 1;
        }
    }
    // === PHASES 1-N: MULTI-WAVE RADIAL BURST WITH SAFE ARC ===
    else if (SubPhase <= waveCount)
    {
        if (Timer == 1)
        {
            MagnumScreenEffects.AddScreenShake(15f);
            SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f }, NPC.Center);
            
            CustomParticles.GenericFlare(NPC.Center, Color.White, 1.5f, 25);
            
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projectileCount = 32 + difficultyTier * 8;
                float safeAngle = (target.Center - NPC.Center).ToRotation();
                float safeArc = MathHelper.ToRadians(30f); // 30 degree gap toward player
                
                for (int i = 0; i < projectileCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / projectileCount;
                    
                    // SAFE ARC EXEMPTION - skip projectiles aimed at player
                    float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                    if (Math.Abs(angleDiff) < safeArc) continue;
                    
                    float speed = 12f + difficultyTier * 2f + SubPhase;
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 80, EroicaGold, 8f);
                }
            }
            
            // Cascading halo rings for dramatic release
            for (int i = 0; i < 8; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaScarlet, EroicaGold, i / 8f), 
                    0.4f + i * 0.15f, 18 + i * 3);
            }
        }
        
        if (Timer >= 45) // Pause between waves
        {
            Timer = 0;
            SubPhase++;
        }
    }
    else
    {
        if (Timer >= 40)
        {
            EndAttack();
        }
    }
}
```

### Key Design Principles from Hero's Judgment

| Element | Implementation | Why It Works |
|---------|---------------|--------------|
| **Charge Buildup** | Particles converge from 200px radius down to boss | Player sees danger building |
| **Safe Zone Indicators** | Cyan flares around player show "safe spot" | Teaches mechanics visually |
| **Safe Arc Exemption** | 30° gap in projectile spread toward player | Rewards positioning, prevents unavoidable damage |
| **Multi-Wave Escalation** | 2+ waves, each wave faster | Tension increases, not front-loaded |
| **Cascading Halos** | 8 staggered halo rings on release | Visual "explosion" sells the impact |
| **Screen Shake Timing** | Only at 70%+ charge and on release | Builds anticipation, not annoying |

### MANDATORY: Boss Attack Checklist

**Before implementing ANY boss attack, verify:**

- [ ] **Charge/Windup Phase** - Visual buildup that players can read
- [ ] **Safe Zone Indicators** - Players KNOW where safety is
- [ ] **Difficulty Scaling** - Uses `difficultyTier` and `GetAggressionSpeedMult()`
- [ ] **Projectile Variety** - Uses different `BossProjectileHelper` types
- [ ] **Cascading VFX** - Multiple halo rings, gradient colors
- [ ] **Sound Design** - Distinct audio cues for charge and release
- [ ] **Recovery Time** - Player has windows to attack

### Additional Attack Patterns for Reference

#### Golden Rain (Area Denial from Above)
```csharp
// Boss hovers above player, rains projectiles with warning flares
private void Attack_GoldenRain(Player target)
{
    int duration = (int)((120 + difficultyTier * 40) * GetAggressionRateMult());
    int fireInterval = Math.Max(3, (int)((12 - difficultyTier * 2) * GetAggressionRateMult()));
    
    // Boss hovers above target
    Vector2 hoverPos = target.Center + new Vector2(0, -400f);
    Vector2 toHover = hoverPos - NPC.Center;
    if (toHover.Length() > 50f)
    {
        toHover.Normalize();
        NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 10f * GetAggressionSpeedMult(), 0.05f);
    }
    
    // WARNING FLARES - show where projectiles will spawn
    if (Timer % 20 == 0)
    {
        for (int i = 0; i < 3 + difficultyTier; i++)
        {
            float xOffset = Main.rand.NextFloat(-300f, 300f);
            Vector2 warningPos = target.Center + new Vector2(xOffset, -500f);
            CustomParticles.GenericFlare(warningPos, EroicaGold * 0.5f, 0.3f, 15);
        }
    }
    
    // Fire projectiles with variety
    if (Timer % fireInterval == 0 && Timer > 30 && Main.netMode != NetmodeID.MultiplayerClient)
    {
        int count = 2 + difficultyTier;
        for (int i = 0; i < count; i++)
        {
            float xOffset = Main.rand.NextFloat(-350f, 350f);
            Vector2 spawnPos = target.Center + new Vector2(xOffset, -550f);
            float ySpeed = 12f + difficultyTier * 3f + aggressionLevel * 4f;
            Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), ySpeed);
            
            // PROJECTILE VARIETY - mix accelerating bolts and tracking orbs
            if (i % 2 == 0)
                BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.6f, 75, EroicaGold, 20f);
            else
                BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 75, EroicaScarlet, 0.01f);
            
            CustomParticles.GenericFlare(spawnPos, EroicaGold, 0.4f, 10);
        }
    }
    
    if (Timer >= duration) EndAttack();
}
```

#### Valor Cross (8-Arm Star Pattern)
```csharp
// Projects 8-arm star pattern with layered projectiles per arm
private void Attack_ValorCross(Player target)
{
    int patterns = 2 + difficultyTier;
    int patternDelay = 50 - difficultyTier * 8;
    
    NPC.velocity *= 0.92f;
    
    if (SubPhase < patterns)
    {
        // TELEGRAPH: Show arm directions with expanding lines
        if (Timer < 25 && Timer % 3 == 0)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.PiOver4 * i + SubPhase * MathHelper.PiOver4 * 0.5f;
                Vector2 lineEnd = NPC.Center + angle.ToRotationVector2() * (80f + Timer * 4f);
                CustomParticles.GenericFlare(lineEnd, EroicaScarlet * 0.5f, 0.2f, 4);
            }
        }
        
        // FIRE: 8 arms with multiple projectiles per arm
        if (Timer == 25 && Main.netMode != NetmodeID.MultiplayerClient)
        {
            float baseSpeed = 13f + difficultyTier * 3f;
            int projectilesPerArm = 4 + difficultyTier;
            
            for (int arm = 0; arm < 8; arm++)
            {
                float armAngle = MathHelper.PiOver4 * arm + SubPhase * MathHelper.PiOver4 * 0.5f;
                
                for (int p = 0; p < projectilesPerArm; p++)
                {
                    float speed = baseSpeed + p * 2f;
                    Vector2 vel = armAngle.ToRotationVector2() * speed;
                    Color color = arm % 2 == 0 ? EroicaGold : EroicaScarlet;
                    
                    // VARIATION: Outer projectiles have homing, inner are straight
                    float homing = p >= projectilesPerArm - 1 ? 0.02f : 0f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70, color, homing);
                }
            }
            
            // VFX burst on fire
            CustomParticles.GenericFlare(NPC.Center, Color.White, 0.9f, 18);
            for (int i = 0; i < 6; i++)
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaScarlet, EroicaGold, i / 6f), 0.3f + i * 0.1f, 15 + i * 2);
        }
        
        if (Timer >= patternDelay) { Timer = 0; SubPhase++; }
    }
    else if (Timer >= 30) EndAttack();
}
```

#### Sakura Storm (Orbiting Boss + Spiral Projectiles)
```csharp
// Boss orbits player while firing spiral arms of projectiles
private void Attack_SakuraStorm(Player target)
{
    int duration = (int)((100 + difficultyTier * 30) * GetAggressionRateMult());
    int arms = 3 + difficultyTier;
    
    // ORBITAL MOVEMENT - boss circles around target
    float spinSpeed = (0.02f + difficultyTier * 0.005f) * GetAggressionSpeedMult();
    float radius = 350f - aggressionLevel * 50f; // Get closer as aggression builds
    float angle = Timer * spinSpeed;
    Vector2 idealPos = target.Center + angle.ToRotationVector2() * radius;
    Vector2 toIdeal = idealPos - NPC.Center;
    NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 12f, 0.08f);
    
    // SPIRAL FIRE - rotating arms
    int fireInterval = Math.Max(2, (int)((6 - difficultyTier) * GetAggressionRateMult()));
    if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
    {
        float spiralAngle = Timer * 0.15f;
        
        for (int arm = 0; arm < arms; arm++)
        {
            float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
            float speed = 12f + difficultyTier * 3f;
            Vector2 vel = armAngle.ToRotationVector2() * speed;
            
            // PROJECTILE VARIETY - alternate wave and tracking orbs
            if (arm % 2 == 0)
                BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 65, SakuraPink, 3f);
            else
                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.9f, 65, EroicaGold, 0.015f);
        }
        
        CustomParticles.GenericFlare(NPC.Center, SakuraPink, 0.35f, 10);
    }
    
    // AMBIENT: Sakura petals constantly
    if (Timer % 8 == 0) ThemedParticles.SakuraPetals(NPC.Center, 3, 40f);
    
    if (Timer >= duration) EndAttack();
}
```

### Attack Pattern Categories (Mix These!)

| Category | Examples | Key Features |
|----------|----------|--------------|
| **Radial Bursts** | Hero's Judgment, Valor Cross | Safe arc, multi-wave, cascading halos |
| **Area Denial** | Golden Rain | Warning flares, hover positioning |
| **Orbital** | Sakura Storm | Boss circles player, spiral projectiles |
| **Dash** | Triumphant Charge | Multiple dashes, projectiles on movement |
| **Dive** | Phoenix Dive | Aerial dive, ground impact effects |
| **Ultimate** | Ultimate Valor | Multi-phase, all-out spectacle |

**Every boss should have at least one attack from each category!**

---

## 📚 BOSS VFX OPTIMIZER SYSTEM - PERFORMANCE & WARNINGS

> **For detailed boss breakdowns, see: [Curated_Boss_Effects_and_How_To.md](../Documentation/Curated_Boss_Effects_and_How_To.md)**

### BossVFXOptimizer Overview

**File:** `Common/Systems/BossVFXOptimizer.cs`

This system provides:
1. **Performance optimization** - Frame-skip and quality scaling
2. **Warning indicators** - Standardized attack telegraphs
3. **Attack release VFX** - Consistent visual impact

### Warning Type Enum

```csharp
public enum WarningType
{
    Safe,      // Cyan - "Stand here to be safe"
    Caution,   // Yellow - "This area will be dangerous soon"
    Danger,    // Red - "Projectiles incoming on this path"
    Imminent   // White - "Attack is about to hit NOW"
}
```

### Essential Warning Methods

| Method | Usage | Example |
|--------|-------|---------|
| `WarningLine(start, direction, length, markers, type)` | Show projectile trajectory | Dash attacks, beams |
| `SafeZoneRing(center, radius, markers)` | Show where player SHOULD stand | Radial bursts with gaps |
| `DangerZoneRing(center, radius, markers)` | Show where player should NOT be | AOE attacks |
| `ConvergingWarning(center, radius, progress, color, count)` | Charge-up particle ring | Attack windup |
| `SafeArcIndicator(center, safeAngle, arcWidth, radius, markers)` | Show gap in radial attack | Hero's Judgment style |
| `GroundImpactWarning(point, radius, progress)` | Landing zone for dives | Slam/dive attacks |
| `LaserBeamWarning(start, angle, length, intensity)` | Beam trajectory | Laser attacks |
| `ElectricalBuildupWarning(center, color, radius, progress)` | Shock attack charging | Electrical attacks |

### Standard Attack Telegraph Pattern

```csharp
// Phase 0: Telegraph (ALWAYS include warning indicators)
if (SubPhase == 0)
{
    float progress = Timer / (float)chargeTime;
    
    // 1. Converging particles show charge
    BossVFXOptimizer.ConvergingWarning(NPC.Center, 150f, progress, ThemeColor, 8);
    
    // 2. Safe zone shows escape route
    BossVFXOptimizer.SafeZoneRing(target.Center, 100f, 12);
    
    // 3. Warning lines show projectile directions
    for (int i = 0; i < 8; i++)
    {
        float angle = MathHelper.TwoPi * i / 8f;
        BossVFXOptimizer.WarningLine(NPC.Center, angle.ToRotationVector2(), 300f, 10, WarningType.Danger);
    }
    
    if (Timer >= chargeTime) { Timer = 0; SubPhase = 1; }
}

// Phase 1: Execute (use AttackReleaseBurst for consistent VFX)
if (SubPhase == 1 && Timer == 1)
{
    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, PrimaryColor, SecondaryColor, 1.2f);
    // Spawn projectiles...
}
```

### Optimized Particle Methods

```csharp
// Use these instead of raw CustomParticles calls for automatic optimization:
BossVFXOptimizer.OptimizedFlare(pos, color, scale, lifetime, frameInterval);
BossVFXOptimizer.OptimizedHalo(pos, color, scale, lifetime, frameInterval);
BossVFXOptimizer.OptimizedBurst(pos, color, count, speed);
BossVFXOptimizer.OptimizedRadialFlares(center, color, count, radius, scale, lifetime);
BossVFXOptimizer.OptimizedCascadingHalos(center, startColor, endColor, count, scale, lifetime);
BossVFXOptimizer.OptimizedThemedParticles(center, "sakura", count, radius);
```

---

## 🎭 BOSS QUICK REFERENCE - ALL 4 BOSSES

### Eroica, God of Valor
**File:** `Content/Eroica/Bosses/EroicasRetribution.cs`
| Property | Value |
|----------|-------|
| HP | 450,000 |
| Theme | Heroic triumph, scarlet → gold |
| Key Color | `new Color(255, 200, 80)` EroicaGold |
| Signature Attack | HeroesJudgment (radial burst with safe arc) |
| Themed VFX | SakuraPetals |

### La Campanella, Chime of Life
**File:** `Content/LaCampanella/Bosses/LaCampanellaChimeOfLife.cs`
| Property | Value |
|----------|-------|
| HP | 400,000 |
| Theme | Infernal bell, black smoke → orange fire |
| Key Color | `new Color(255, 140, 40)` CampanellaOrange |
| Signature Attack | InfernalJudgment + BellLaserGrid |
| Themed VFX | HeavySmokeParticle, bell chimes |

### Swan Lake, The Monochromatic Fractal
**File:** `Content/SwanLake/Bosses/SwanLakeTheMonochromaticFractal.cs`
| Property | Value |
|----------|-------|
| HP | 950,000 |
| Theme | Ballet elegance, monochrome + rainbow |
| Key Color | White/Black contrast with `Main.hslToRgb()` rainbow |
| Signature Attack | SwanSerenade + MonochromaticApocalypse |
| Themed VFX | SwanFeatherDrift, PrismaticSparkle |

### Enigma, The Hollow Mystery
**File:** `Content/EnigmaVariations/Bosses/EnigmaTheHollowMystery.cs`
| Property | Value |
|----------|-------|
| HP | 380,000 |
| Theme | Void mystery, purple → green |
| Key Color | `new Color(140, 60, 200)` EnigmaPurple |
| Signature Attack | ParadoxJudgment + VoidLaserWeb |
| Themed VFX | EnigmaEyeGaze, GlyphBurst |

### Boss Difficulty Tier System (All Bosses)

```csharp
// Standard 3-tier system based on HP percentage
float hpPercent = (float)NPC.life / NPC.lifeMax;
int difficultyTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : 2);

// Tier 0 (100-70% HP): Core attacks only
// Tier 1 (70-40% HP): +Phase 2 attacks, faster, more projectiles
// Tier 2 (40-0% HP): +Phase 3 attacks, ultimate abilities, maximum aggression
```

---

## ⭐ BOSS PROJECTILE VFX SCALE GUIDELINES - PLAYER-SIZED ⭐

> **CRITICAL: Boss projectile VFX must be PLAYER-SIZED, not screen-filling monstrosities.**

### VFX Scale Reference (PreDraw Bloom Layers)

**For PLAYER-SIZED projectiles (~8x8 to 24x24 hitbox):**

| Layer | Scale | Purpose |
|-------|-------|---------|
| Outer Glow | `0.5f - 0.6f` | Soft ambient bloom |
| Middle Energy | `0.35f - 0.45f` | Main visible body |
| Core Flare | `0.25f - 0.3f` | Bright center |
| White-hot Center | `0.12f - 0.15f` | Intense core point |

**For orbit/spark effects:**

| Element | Radius | Scale |
|---------|--------|-------|
| Orbiting Sparks | `10f - 14f` | `0.1f - 0.15f` |
| Trail Particles | N/A | `0.18f - 0.25f` |

```csharp
// ✅ CORRECT - Player-sized projectile VFX scales
public override bool PreDraw(ref Color lightColor)
{
    // Outer ethereal layer - SMALL
    Main.spriteBatch.Draw(glowTex, pos, null, outerGlow * 0.25f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
    // Middle energy layer
    Main.spriteBatch.Draw(tex, pos, null, midGlow * 0.4f, rot, origin, 0.35f * pulse, SpriteEffects.None, 0f);
    // Core flare layer
    Main.spriteBatch.Draw(tex, pos, null, coreGlow * 0.55f, rot, origin, 0.25f * pulse, SpriteEffects.None, 0f);
    // White-hot center
    Main.spriteBatch.Draw(tex, pos, null, innerGlow * 0.75f, rot, origin, 0.12f * pulse, SpriteEffects.None, 0f);
    
    // Orbiting sparks - small radius, tiny scale
    for (int i = 0; i < 3; i++)
    {
        float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
        Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f - Main.screenPosition;
        Main.spriteBatch.Draw(tex, sparkPos, null, sparkColor * 0.7f, 0f, origin, 0.1f * pulse, SpriteEffects.None, 0f);
    }
    
    return false;
}

// ❌ FORBIDDEN - Oversized "wall of glow" projectiles
Main.spriteBatch.Draw(glowTex, pos, null, color, 0f, origin, 2.0f, ...);  // TOO BIG
Main.spriteBatch.Draw(tex, pos, null, color, 0f, origin, 1.6f, ...);     // TOO BIG
```

### Reference: AggressiveBossProjectiles.cs Projectile Types

**These are the gold-standard boss projectile implementations:**

| Projectile Type | Use Case | Unique Visual Identity |
|----------------|----------|----------------------|
| `HostileOrbProjectile` | Tracking orbs | Pulsing core, 3 orbiting spark points, magic sparkle dust trail |
| `AcceleratingBoltProjectile` | Fast strikes | Velocity-based stretch, 4-point rotating sparks, accelerating speed |
| `ExplosiveOrbProjectile` | Delayed explosions | Warning glow buildup, dramatic 8-flare explosion |
| `WaveProjectile` | Sinusoidal movement | Undulating motion, wave-pattern trail, dual-tone colors |
| `DelayedDetonationProjectile` | Area denial | Countdown glow, rune accents, warning pulsation |
| `BoomerangProjectile` | Returning attacks | Figure-8 spin, prismatic trail, return-phase color shift |

**Use `BossProjectileHelper` to spawn these:**
```csharp
BossProjectileHelper.SpawnHostileOrb(pos, vel, damage, color, homingStrength);
BossProjectileHelper.SpawnAcceleratingBolt(pos, vel, damage, color, acceleration);
BossProjectileHelper.SpawnExplosiveOrb(pos, vel, damage, color, explosionRadius);
BossProjectileHelper.SpawnWaveProjectile(pos, vel, damage, color, waveAmplitude);
BossProjectileHelper.SpawnDelayedDetonation(pos, damage, color, delay);
BossProjectileHelper.SpawnBoomerang(pos, vel, damage, color, returnSpeed);
```

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
