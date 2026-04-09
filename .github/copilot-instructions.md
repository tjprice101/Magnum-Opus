# MagnumOpus Copilot Instructions

## VFX Agent System

For ALL visual effects work (weapons, bosses, trails, shaders, particles, bloom, screen effects), use the **@vfx-composer** agent. It orchestrates 12 specialized sub-agents across two tiers and has deep knowledge of compositing techniques grounded in reference mod implementations.

### Agent Routing Guide

| Need | Agent | Role |
|------|-------|------|
| Weapon concept & creative direction | `@creative-director` | 30+ attack archetypes, uniqueness enforcement, concept ideation |
| Full VFX pipeline orchestration | `@vfx-composer` | Routes to all sub-agents, manages compositing |
| Trail & beam rendering | `@trail-specialist` | Triangle strips, UV mapping, CurveSegment, RibbonFoundation |
| HLSL shader authoring | `@shader-specialist` | Noise, distortion, SDF, color grading, .fx compilation |
| Particles & bloom | `@particle-specialist` | Dust choreography, bloom stacking, metaballs |
| Boss encounters & arenas | `@boss-vfx-specialist` | Phase transitions, arena BG, attack telegraphs |
| Projectile behavior & rendering | `@projectile-architect` | Movement patterns, spawn patterns, on-hit behaviors |
| Impact & hit effects | `@impact-designer` | Multi-layer impacts, crit flashes, death bursts |
| Combo & mechanic design | `@weapon-mechanic` | Combo systems, resource mechanics, mode switching |
| Dynamic lighting & atmosphere | `@lighting-atmosphere` | God rays, fog, auras, screen tint, pulsing light |
| Dust spawning & ModDust types | `@dust-particles` | Dust physics, spawn patterns, music note choreography |
| Animation & motion | `@motion-animator` | CurveSegment easing, afterimages, smears, dash/teleport VFX |
| Screen distortion & camera | `@screen-effects` | Chromatic aberration, vignette, screen shake, cinematics |

### Workflow Skills

| Skill | Purpose |
|-------|---------|
| `/new-weapon-vfx` | Step-by-step weapon VFX design from scratch (invokes @creative-director first) |
| `/audit-weapon-quality` | Evaluate VFX quality with 6-dimension creativity scoring |
| `/design-boss-phase` | Design boss encounter VFX across all phases |
| `/design-weapon-identity` | Deep weapon concept design with 17+ interactive questions |
| `/design-projectile` | 10-step projectile design from behavior to rendering |
| `/design-impact` | 8-step impact effect design with themed death bursts |
| `/audit-vfx-creativity` | Audit VFX diversity across a theme's weapon set |

### Agent Invocation Rules

1. **Always start with `@creative-director`** for new weapon/boss concepts before implementation
2. **`@vfx-composer` orchestrates** — it delegates to domain and technical agents automatically
3. **Domain agents ask 15+ interactive questions** before proposing any design
4. **All agents live-search reference repos** — they never invent patterns from scratch

**File-specific instructions** auto-load when editing shader files (`Effects/`, `ShaderSource/`), weapon content files (`Content/**/Weapons/`), VFX system files (`Common/Systems/VFX/`, `Common/Systems/Particles/`), projectile files, and screen effect files.

---

## Reference Repositories

Search these local repos before inventing VFX patterns from scratch. Each excels at different aspects:

| Repository | Local Path | Strengths |
|------------|-----------|-----------|
| **Calamity** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo` | Primitive trails, CurveSegment animation, slash shaders, melee VFX, metaballs, boss AI, content structure |
| **Wrath of the Gods** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Wrath of the Gods Repo` | 190+ shaders, screen distortion + exclusion zones, per-phase boss backgrounds, GPU-accelerated particles, reality tear effects |
| **Everglow** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Everglow Repo` | Pipeline-based post-processing, hierarchical bloom, VFXBatch GPU batching, dissolve effects, 35 paired dust types |
| **Coralite** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Coralite Mod Repo` | Shader techniques, particle systems, rendering pipelines |
| **VFX+** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\VFX+ Mod Repo` | Advanced VFX systems, trail rendering, visual polish |

**Always read the local repos directly.** Search and read the actual source files  -- do not rely on memory or assumptions.

---

## Asset Failsafe Protocol

**MANDATORY.** If you need a texture, particle sprite, or any visual asset that does not already exist, **STOP implementation** and tell the user:

1. **What asset is needed** and where it would be used
2. **A detailed Midjourney prompt** with: art style, subject description, color palette (white/grayscale on solid black background for VFX), dimensions (256x256 particles, 512x128 trails), technical requirements
3. **Expected file location** using the SandboxLastPrism folder pattern

**NEVER use placeholder textures.** Missing asset = the effect cannot be implemented yet. This is a hard stop.

### Available Asset Library (200+ textures)

| Location | Contents |
|----------|----------|
| `Assets/VFX Asset Library/BeamTextures/` | 14 beam strip textures |
| `Assets/VFX Asset Library/ColorGradients/` | 12 theme LUT ramps |
| `Assets/VFX Asset Library/GlowAndBloom/` | 8 bloom/flare sprites |
| `Assets/VFX Asset Library/ImpactEffects/` | 8 impact textures |
| `Assets/VFX Asset Library/NoiseTextures/` | 20 noise types (Perlin, Simplex, FBM, Voronoi, Marble, Cosmic, Nebula) |
| `Assets/VFX Asset Library/MasksAndShapes/` | 7 mask textures |
| `Assets/VFX Asset Library/TrailsAndRibbons/` | 4 trail strip textures |
| `Assets/VFX Asset Library/SlashArcs/` | 4 sword arc textures |
| `Assets/VFX Asset Library/Projectiles/` | 7 projectile sprites |
| `Assets/Particles Asset Library/` | 8+ music note variants, 3 star sprites |
| `Assets/SandboxLastPrism/` | Flare, Gradients, Orbs (5), Pixel, Trails (7 incl. Clear/) |
| Theme-specific folders | Each of 10 themes has 6-11 dedicated textures |

**Check these FIRST before requesting new assets.**

### Item-Specific Asset Placement (SandboxLastPrism Pattern)

All item-specific content (weapons, bosses, accessories, etc.) should follow the organizational pattern established by the Sandbox Last Prism. The SandboxLastPrism (and its Exoblade-style self-contained architecture) is a **recommended folder and file structure**  -- it demonstrates a working, well-organized approach for per-weapon content. You do **not** need to copy the exact implementation or code patterns of the Sandbox Exoblade; it is there as a reference for how to organize files and folders so that each weapon's systems (shaders, particles, primitives, utilities, projectiles, buffs) are self-contained and properly structured. Adapt the architecture to suit each weapon's unique needs.

This pattern splits item-specific assets across three root directories by purpose:

#### 1. VFX Textures  -> `Assets/<ThemeName>/<ItemName>/`

VFX texture assets (PNG images used by shaders, trails, particles, bloom) go in theme-scoped subfolders of `Assets/`, organized by texture type:

```
Assets/<ThemeName>/<ItemName>/
笏懌楳笏 Flare/               -- Lens flare, flash, and burst VFX textures
笏懌楳笏 Gradients/           -- Color gradient lookup textures (1D or 2D ramps)
笏懌楳笏 Orbs/                -- Soft glow circles, feathered spheres, bloom orbs
笏懌楳笏 Pixel/               -- Tiny pixel-art particle sprites for ModDust types
笏懌楳笏 Trails/              -- Trail strip textures, energy lines, ribbon UVs
笏・  笏披楳笏 Clear/           -- Trail textures on transparent (not black) backgrounds
笏懌楳笏 WeaponDesign/        -- Midjourney reference prompts for the weapon sprite itself
笏披楳笏 [Class-Specific]/    -- Additional folders based on weapon class needs:
    笏懌楳笏 SlashArcs/       -- (Melee) Swing arc overlay textures
    笏懌楳笏 ImpactSlash/     -- (Melee) Hit impact slash/burst textures
    笏懌楳笏 Beams/           -- (Magic/Summoner) Beam strip textures
    笏懌楳笏 ChannelingEffects/  -- (Magic) Cast circle and channeling textures
    笏懌楳笏 MuzzleFlash/     -- (Ranged) Barrel discharge flash textures
    笏懌楳笏 ImpactBurst/     -- (Ranged) Explosive detonation impact textures
    笏披楳笏 SummonCircle/    -- (Summoner) Summoning ritual circle textures
```

**Reference:** See `Assets/SandboxLastPrism/` for the canonical example (Flare/, Gradients/, Orbs/, Pixel/, Trails/, Trails/Clear/).

#### 2. Custom Shaders  -> `Effects/<ThemeName>/<ItemName>/`

Item-specific `.fx` and `.fxc` shader files go in theme-scoped subfolders of `Effects/`, grouped by shader purpose:

```
Effects/<ThemeName>/<ItemName>/
笏懌楳笏 <ShaderName>.fx          -- Shader source
笏懌楳笏 <ShaderName>.fxc         -- Compiled shader bytecode
笏披楳笏 <SubCategory>/           -- Grouped by shader purpose when multiple exist:
    笏懌楳笏 Radial/              -- Radial/circular effect shaders (sigils, auras)
    笏披楳笏 Scroll/              -- UV-scrolling shaders (beams, trails, lasers)
```

**Reference:** See `Effects/SandboxLastPrism/` for the canonical example (GlowDustShader at root, Radial/, Scroll/ subdirectories).

#### 3. C# Code + Dust Textures  -> `Content/<ThemeName>/<ItemName>/`

Item code, custom ModDust types, and dust sprite textures all co-locate under `Content/`:

```
Content/<ThemeName>/<Category>/<ItemName>/
笏懌楳笏 <ItemName>.cs            -- Main item/weapon class
笏懌楳笏 <ItemName>VFX.cs         -- Item-specific VFX static class
笏懌楳笏 <ItemName>Swing.cs       -- (Melee) Swing projectile
笏懌楳笏 Dusts/                   -- Custom ModDust types for this item
笏・  笏懌楳笏 <DustName>.cs        -- Dust behavior code
笏・  笏披楳笏 Textures/            -- Dust sprite .png files (co-located with dust code)
笏披楳笏 Systems/                 -- Item-specific systems (flash, pixelation, screen shake)
    笏懌楳笏 <System>.cs
    笏披楳笏 ...
```

**Reference:** See `Content/SandboxLastPrism/` for the canonical example (SandboxLastPrism.cs at root, Dusts/ with Textures/ subfolder, Systems/ with flash/pixelation/screen-shake).

#### Quick Placement Cheatsheet

| Asset Type | Location |
|------------|----------|
| VFX texture (.png for shaders/trails/bloom) | `Assets/<Theme>/<Item>/<TextureType>/` |
| Weapon/item sprite (.png) | Same folder as the `.cs` item file in `Content/` |
| Custom shader (.fx/.fxc) | `Effects/<Theme>/<Item>/` |
| ModDust code (.cs) | `Content/<Theme>/<Category>/<Item>/Dusts/` |
| ModDust sprite (.png) | `Content/<Theme>/<Category>/<Item>/Dusts/Textures/` |
| Midjourney prompts | `Assets/<Theme>/<Item>/<TextureType>/MidjourneyPrompts.txt` |
| Item-specific systems (.cs) | `Content/<Theme>/<Category>/<Item>/Systems/` |

---

## Mod Philosophy & Design Heart

> *"This mod is based around music. It's based around how it connects to your heart, and it's based around how it impacts the world."*

MagnumOpus is a music-themed Terraria mod where each weapon, projectile, and boss implements its own unique VFX directly in its .cs file. There are no global VFX systems that automatically apply effects. It is **a symphony made playable** -- every weapon, every effect, every particle should make players *feel* the music.

### Guiding Principles

1. **EVERY WEAPON IS UNIQUE** - Within a theme, if there are 3 swords, each sword should have different effects, trails, impacts, and special mechanics. Avoid sharing VFX between weapons.

2. **MUSICAL IDENTITY IS VISIBLE** - This is a music mod. Music notes, harmonic pulses, resonance waves, and musical motifs should be woven into weapon effects where appropriate. Players should feel the musical identity of each weapon.

3. **CREATIVE USE OF ASSETS** - You have 100+ custom particle PNGs (MusicNote variants, EnergyFlare, PrismaticSparkle, Glyphs, etc.). Use them creatively. Mix and match to create layered, rich effects.

4. **LAYER EFFECTS FOR RICHNESS** - Great effects combine multiple layers: core visuals + bloom + sparkle accents + theme particles. Single-effect particles feel flat by comparison.

5. **THEME COLORS STAY CONSISTENT** - La Campanella is always black/orange/gold. Eroica is always scarlet/gold. Moonlight Sonata is always purple/blue/silver. But HOW effects work within those palettes is completely open to creative interpretation.

6. **IMPACTS SHOULD FEEL SPECIAL** - Every impact should be multi-layered and memorable. Avoid the generic "hit -> small explosion -> done" pattern.

### The Uniqueness Mandate

Within a single boss/theme, weapons of the same class should be meaningfully different. Here's an example of what that looks like:

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

Same theme. Same colors. Completely unique experiences.

### What Makes Effects Memorable

**Generic (avoid):**
- Fires a projectile -> hits enemy -> small explosion
- Swing sword -> trail follows -> damage dealt

**Memorable (aspire to):**
- Fires spiraling projectiles that split into smaller ones trailing music notes, converging on target with a harmonic explosion and screen shake
- Swing creates visible arc of floating notes, blade tip leaves constellation trail, impacts spawn orbiting glyphs that attack nearby enemies

---

## Theme Identity & Uniqueness - THE SOUL OF EACH SCORE

> *"Each score like Moonlight Sonata, Eroica, etc. should all feel vastly unique from one another."*

Each musical score/theme MUST have its own distinct visual AND emotional identity. Do NOT copy effects between themes. Each theme represents a different musical composition with its own story, feeling, and visual language.

### Why Theme Uniqueness Matters
- Players should **instantly recognize** which theme's weapon they're using just from the visuals
- Each score represents a **different emotional journey** that must translate to effects
- Cross-theme copying creates bland, forgettable weapons that betray the music

### Theme Visual & Emotional Identities

| Theme | Musical Soul | Colors | Emotional Core |
|-------|-------------|--------|---------------|
| **La Campanella** | The ringing bell, virtuosic fire | Black smoke, orange flames, gold highlights | Passion, intensity, burning brilliance |
| **Eroica** | The hero's symphony | Scarlet, crimson, gold, sakura pink | Courage, sacrifice, triumphant glory |
| **Swan Lake** | Grace dying beautifully | Pure white, black contrast, prismatic rainbow edges | Elegance, tragedy, ethereal beauty |
| **Moonlight Sonata** | The moon's quiet sorrow | Deep dark purples, vibrant light blues, violet, ice blue | Melancholy, peace, mystical stillness |
| **Enigma Variations** | The unknowable mystery | Void black, deep purple, eerie green flame | Mystery, dread, arcane secrets |
| **Fate** | The celestial symphony of destiny | Black void, dark pink, bright crimson, celestial white | Cosmic inevitability, endgame awe |
| **Clair de Lune** | Shattered time, blazing clocks | Dark red, vibrant gray, white | Temporal destruction, reality's unraveling |
| **Dies Irae** | Hell's retribution flames | White, black, crimson | Divine judgment, heavenly banishment |
| **Nachtmusik** | Starlit melodies, sweet songs | Golden, dark purple | Golden twinkling, nocturnal melody |
| **Ode to Joy** | Eternal symphony garden | Monochromatic black, white, prismatic chromatic | Prismatic radiance, garden of eternal symphony |

### Embracing Each Score's Unique Elements

**La Campanella** - *The Flaming Bell*
Every La Campanella weapon should feel like ringing bells of fire. Heavy black smoke billowing, orange flames crackling and dancing, bell chime sounds on impacts. The intensity of Liszt's virtuosic passion.

**Eroica** - *The Hero's Journey*
Eroica weapons tell the story of heroic sacrifice. Sakura petals scattering like a warrior's final stand, golden light breaking through scarlet flames, rising embers ascending toward the heavens. The triumph and tragedy of Beethoven's symphony.

**Swan Lake** - *Grace in Monochrome*
Swan Lake weapons are elegant even in destruction. White and black feathers drifting gracefully, prismatic rainbow shimmer at the edges, clean graceful arcs and flowing trails. The dying beauty of Tchaikovsky's swans.

**Moonlight Sonata** - *The Moon's Whisper*
Moonlight weapons are soft, mystical, lunar. Soft purple mist rolling gently, silver light like moonbeams through clouds, gentle flowing movements. The quiet melancholy of Beethoven's adagio. Descriptions and lore lines should evoke moonlight, tides, silver, stillness, and sorrow -- never cosmos, stars, or space.

**Enigma Variations** - *The Unknowable*
Enigma weapons are shrouded in dread and mystery. Swirling void, watching eyes, eerie green flames. Effects should feel unknowable and arcane.

**Fate** - *The Celestial Symphony*
Fate weapons are CELESTIAL COSMIC ENDGAME. Ancient glyphs orbiting weapons, star particles streaming in trails, cosmic cloud energy, screen distortions and chromatic aberration. Dark prismatic: black bleeding to pink to crimson with celestial white highlights. The feeling: you are wielding the power of the cosmos itself.

**Clair de Lune** - *Shattered Time*
Clair de Lune weapons are temporal destruction made manifest. Dark reds mixed with vibrant grays and whites -- shattered clock faces, blazing clock fragments, crackling destruction energy tearing through the fabric of time. Every weapon should feel like the tempo of reality is being ripped apart. Broken gears scattering, clock hands fracturing, red energy crackling through shattered glass.

**Dies Irae** - *Hell's Retribution*
Dies Irae weapons are divine punishment incarnate. White, black, and crimson flames of hell's retribution climbing and soaring -- heavenly banishment made visible. The fury is not chaotic but purposeful: judgment rendered, sentence delivered. Infernal fire that burns with righteous wrath.

**Nachtmusik** - *Golden Starlit Melodies*
Nachtmusik weapons glow with the golden twinkling of starlit melodies lighting a dark purple world. Sweet songs made visible -- golden sparkles drifting through deep purple darkness, starlit melody wisps trailing behind every strike. The beauty of music heard on a quiet night.

**Ode to Joy** - *The Eternal Symphony Garden*
Ode to Joy weapons are monochromatic roses shimmering with prismatic radiant light. Black and white chromatic glass refracting into rainbow brilliance -- the garden of eternal symphony. Every effect is a glass rose catching light, every impact a prismatic cascade of chromatic refractions.

### Weapon Uniqueness Within Themes

When a boss/theme has multiple weapons of the same class, they MUST be radically different:

| Sword | Unique Identity |
|-------|-----------------|
| **Heroic Anthem** | Swings create expanding sound wave rings made of visible music notes. Every 5th hit plays a chord and releases a burst of sakura petals that home to nearby enemies. Trail leaves golden staff lines in the air. |
| **Valor's Edge** | Blade charges with each swing, glowing brighter (visible bloom layers). At full charge, next swing releases a massive phantom blade projectile that passes through enemies leaving ember trails. Impact creates rising flame pillars. |
| **Triumph's Melody** | Each swing spawns 3 small orbiting note projectiles that circle the player. Using special attack releases all accumulated notes as a spiraling barrage. Notes leave prismatic sparkle trails. |

Same theme colors. Same general aesthetic. Completely unique experiences.

---

## Musical Naming & Structure Conventions

These are creative conventions that help maintain the mod's musical identity. They're guidelines for inspiration, not rigid requirements -- use them where they enhance the weapon's design.

### Combo Phases as Musical Movements

Think of each combo phase as a movement in the weapon's symphony. A typical combo has 3-5 phases. The musical metaphor helps structure pacing: Allegro -> Vivace -> Andante -> Presto.

Swing arc segments naturally follow a musical phrase:
- **Windup** (the breath before the note -- slow pullback)
- **Main swing** (the note itself -- fast acceleration)
- **Follow-through** (the resonance -- deceleration/overshoot)

### Color Palettes as Musical Scales

A useful pattern is giving each weapon a color gradient that maps to musical dynamics -- darkest tones for the quietest moments, brightest for the most intense:

```csharp
// Example: mapping dynamics to a color gradient
// Pianissimo (shadows) -> Piano (outer glow) -> Mezzo (body) -> Forte (hot) -> Fortissimo (core)
```

### Ensemble Variety

Different combo phases can use different smear types, particle styles, or trail effects -- like different instruments contributing to an ensemble. This creates visual variety within a single weapon.

### Boss Encounters as Grand Performances

Bosses are the climax of each theme's symphony. The musical structure maps naturally to boss design:
- **Spawn** - The Overture
- **Attack Windups** - Building Tension
- **Attack Firing** - The Crescendo
- **Enrage** - The Finale

---

## Music-Themed VFX Integration

This is a music mod -- weapon effects should feel musical where appropriate. Some ways to achieve this:

- Scatter music notes from blade tips or projectile impacts
- Use harmonic sparkle bursts for phase transitions -- like changing key signatures
- Include music note cascades in finisher effects
- Oscillate trail colors for a shimmering, resonant feel
- Consider rhythmic pulsing tied to attack timing

### Melee Weapon Design Considerations

When designing a new melee weapon, consider:

- Multiple combo phases, each feeling like a different musical movement
- A color palette that fits the theme (dark to bright gradient)
- Trail effects with shader-driven gradients
- Musical elements woven into the VFX (notes, harmonic pulses, resonance)
- Escalating hit effects at higher combo steps
- A unique identity that doesn't overlap with other weapons in the same theme

---

## Tooltip and Description Formatting

### Style Guidelines

Item tooltips should follow vanilla Terraria's style -- informative, clean, and readable. Avoid ALL CAPS for emphasis. Use sentence case. Keep descriptions concise.

```csharp
// Avoid capitalized emphasis
tooltips.Add(new TooltipLine(Mod, "Effect", "Fires slow, MASSIVE reality-warping shots"));

// Prefer clean, vanilla-style descriptions
tooltips.Add(new TooltipLine(Mod, "Effect", "Fires slow, massive reality-warping shots"));
```

### Lore Lines Should Match the Theme

Lore quotes should be poetic and evoke the theme's emotional core:
- **Moonlight Sonata** lore speaks of moonlight, tides, silver, sorrow, stillness
- **Eroica** lore speaks of heroism, sacrifice, glory, triumph
- **La Campanella** lore speaks of fire, bells, passion, intensity
- **Swan Lake** lore speaks of grace, feathers, elegance, tragedy
- **Fate** lore speaks of destiny, cosmos, inevitability, celestial power

Never cross-pollinate themes -- a Moonlight Sonata weapon shouldn't reference cosmos or stars in its lore.

---

## Item Tooltips - ModifyTooltips Required

Items without tooltips appear EMPTY to players. The localization file (`en-US_Mods.MagnumOpus.hjson`) contains auto-generated `Tooltip: ""` placeholders. The actual tooltips should be defined in code using `ModifyTooltips`.

### Tooltip Structure

```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    // Effect lines - describe what the item does
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Primary effect description"));
    tooltips.Add(new TooltipLine(Mod, "Effect2", "Secondary effect description"));

    // Lore line - themed flavor text with colored text
    tooltips.Add(new TooltipLine(Mod, "Lore", "'Poetic flavor text here'")
    {
        OverrideColor = ThemeColor
    });
}
```

### Theme Color Reference for Lore Lines

| Theme | Lore Color |
|-------|------------|
| Moonlight Sonata | `new Color(140, 100, 200)` - Purple |
| Eroica | `new Color(200, 50, 50)` - Scarlet |
| La Campanella | `new Color(255, 140, 40)` - Infernal Orange |
| Enigma Variations | `new Color(140, 60, 200)` - Void Purple |
| Swan Lake | `new Color(240, 240, 255)` - Pure White |
| Fate | `new Color(180, 40, 80)` - Cosmic Crimson |
| Clair de Lune | `new Color(150, 200, 255)` - Shattered Clock Gray-Red |
| Dies Irae | `new Color(200, 50, 30)` - Crimson Hellfire |
| Nachtmusik | `new Color(100, 120, 200)` - Golden Starlit Purple |
| Ode to Joy | `new Color(255, 200, 50)` - Chromatic Glass Rose |

### Tooltip Examples by Item Type

**Weapons** -- describe the special mechanic + lore line:
```csharp
tooltips.Add(new TooltipLine(Mod, "Effect1", "Special attack pattern/mechanic"));
tooltips.Add(new TooltipLine(Mod, "Lore", "'Epic weapon lore'") { OverrideColor = ThemeColor });
```

**Accessories** -- describe the bonus + lore line:
```csharp
tooltips.Add(new TooltipLine(Mod, "Effect1", "Primary bonus (+X stat, etc.)"));
tooltips.Add(new TooltipLine(Mod, "Lore", "'Themed flavor text'") { OverrideColor = ThemeColor });
```

**Materials** -- brief description of crafting use:
```csharp
tooltips.Add(new TooltipLine(Mod, "Effect1", "'Material for crafting [Theme] equipment'"));
```

---

## File Structure

- Melee weapons: `WeaponItem.cs`, `WeaponSwing.cs`, optional `WeaponVFX.cs` in their own folder
- Item/projectile textures: same folder as their .cs file
- Music: `Assets/Music/`

### VFX, Shaders & Particle Locations

When implementing or referencing VFX, shaders, particles, trails, or any visual effect, look in these directories:

#### Shader Files

| Directory | Contents |
|-----------|----------|
| `Effects/` | **Compiled shaders** (.fx) ready for use  -- trail shaders (SimpleTrailShader, MoonlightTrail, HeroicFlameTrail, CelestialValorTrail, EroicaFuneralTrail, ScrollingTrailShader), bloom shaders (SimpleBloomShader, SakuraBloom, MotionBlurBloom), beam shaders (BeamGradientFlow, LunarBeam, TerraBladeFlareBeamShader), screen effects (ScreenDistortion, RadialScrollShader), metaball shaders, and more |
| `ShaderSource/` | **Shader source files** (.fx) for development  -- HLSLLibrary.fxh (shared utilities), advanced shaders (AdvancedBeamShader, AdvancedBloomShader, AdvancedTrailShader, AdvancedDistortionShader, AdvancedScreenEffectsShader), Calamity-inspired shaders (CalamityFireShader, ExobladeSlashShader), procedural trails (ProceduralTrailShader), and README_SHADER_COMPILATION.md for build instructions |
| `Assets/Shaders/` | Additional compiled shader assets |
| `Common/Systems/Shaders/` | **C# shader infrastructure**  -- ShaderLoader.cs (loads/manages shaders) and ShaderRenderer.cs (handles shader rendering) |

#### VFX Textures & Sprites

| Directory | Contents |
|-----------|----------|
| `Assets/VFX/` | **Main VFX texture library** with 15 subcategories: `Afterimages/`, `Beams/`, `Blooms/`, `Impacts/`, `Lightning/`, `LightRays/`, `LUT/` (color grading), `Masks/`, `Noise/` (19 noise types  -- cosmic energy, nebula, Voronoi, FBM, marble, perlin, etc.), `Overlays/`, `Ribbons/`, `Screen/`, `Smears/`, `Trails/` (comet trails, spiral trails, energy UV maps, ember scatters, sparkle fields) |
| `Assets/Particles Asset Library/` | **Particle sprites**  -- 107+ sprites including sparkles, glyphs, halos, explosions, lightning bursts, smoke, sword arcs, feathers, music notes, magic sparkle fields, flare spikes, circular masks, and themed particles |

#### VFX Code Systems

| Directory | Contents |
|-----------|----------|
| `Common/Systems/VFX/` | **Core VFX C# systems** with subsystems: `Beams/` (beam rendering), `Bloom/` (lens flares, god rays, glow), `Boss/` (boss arena/cinematic VFX), `Core/` (particle systems, texture registries, rendering utils), `Effects/` (afterimages, glow dust, smoke, screen shake), `Optimization/` (LOD, adaptive quality, batching), `Projectile/` (layered projectile rendering), `Screen/` (skyboxes, distortions, heat effects), `Themes/` (elemental/themed effects), `Trails/` (advanced trails, nebula, Bezier curves), `Weapon/` (glints, lens flares, fog), plus root files SwingShaderSystem.cs and VFXIntegration.cs |
| `Common/Systems/Particles/` | **Particle system code**  -- ParticleTextureGenerator.cs, CommonParticles.cs, DynamicParticles.cs, SmearParticles.cs, MagnumParticleHandler.cs, MagnumParticleDrawLayer.cs, Particle.cs, plus `Textures/` subdirectory |

#### Theme-Specific VFX

| Directory | Contents |
|-----------|----------|
| `Content/<ThemeName>/VFX/` | Per-theme VFX implementations (e.g., `Content/MoonlightSonata/VFX/` has subdirectories for specific bosses/weapons: Accessories, EternalMoon, GoliathOfMoonlight, IncisorOfMoonlight, MoonlightsCalling, ResurrectionOfTheMoon) |

**Always check these directories before creating new shaders or VFX assets.** Reuse existing shaders, textures, and systems where possible.

---

## Foundation Weapon Systems (Recommended Templates)

`Content/FoundationWeapons/` contains standalone, fully working VFX weapon systems demonstrating best-practice implementations for common effect types: attacks, trails, particles, beams, slashes, explosions, ribbons, masks, and more. Browse these before implementing any VFX  -- they provide structural skeletons and rendering patterns to build from.

See the `/new-weapon-vfx` skill for the full Foundation Weapons reference table.

