# MagnumOpus Copilot Instructions

## Reference Repositories (Primary Implementation Sources)

When implementing VFX, shaders, trails, bloom, particles, melee swings, projectile rendering, boss AI, content systems, or any visual effect, **you MUST read the local cloned repositories first** as your primary reference. These are your authoritative sources for implementation patterns, shader techniques, and content architecture. Do not guess or improvise -- find the real implementation in these repos and adapt it.

### Local Repository Paths (USE THESE DIRECTLY)

| Repository | Local Path | Use For |
|------------|-----------|---------|
| **CalamityMod** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo` | Melee swing architecture (Exoblade, Ark of the Cosmos, Galaxia), primitive trail shaders, bloom stacking, boss AI, projectile VFX, CurveSegment animation, content structure, damage systems |
| **Coralite** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Coralite Mod Repo` | Shader techniques, particle systems, visual effect implementations, rendering pipelines, advanced shader patterns |
| **VFX+** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\VFX+ Mod Repo` | Advanced VFX systems, shader implementations, trail rendering, visual polish techniques |

### GitHub Links (for browsing only)

| Repository | Link |
|------------|------|
| **CalamityMod** | [CalamityModPublic (1.4.4 branch)](https://github.com/CalamityTeam/CalamityModPublic/tree/1.4.4) |
| **VFX+** | [VFXPlus](https://github.com/GreatFriend129/VFXPlus/tree/main) |
| **Coralite** | [Coralite-Mod](https://github.com/CoraIite/Coralite-Mod) |

### How to Use These References

1. **Always read the local repos directly** using the file paths above. Search and read the actual source files -- do not rely on memory or assumptions about how these mods work.
2. **For VFX/shaders/trails**: Search Calamity and Coralite for the specific effect type you need. Read the full implementation, understand the shader pipeline, then adapt for MagnumOpus.
3. **For content patterns** (weapons, projectiles, bosses): Search Calamity for similar content to understand stat balance, AI patterns, and projectile behavior.
4. **For particle systems**: Check Coralite first for advanced particle techniques, then Calamity for particle usage in context.
5. **For shader code** (.fx/.hlsl files): Check all three repos -- each has different shader approaches worth understanding.

**Do NOT invent VFX patterns from scratch.** Read how these mods implement the specific effect you need, then adapt their approach for MagnumOpus using MagnumOpus's existing utility classes where available.

---

## Asset Workflow

If you need a texture, particle sprite, or any visual asset that does not already exist in `Assets/`, **STOP implementation** and tell the user:

1. **What asset is needed** and where it would be used
2. **A detailed Midjourney prompt** to generate it, following this format:
   - Art style and medium
   - Subject description with specific visual details
   - Color palette (white/grayscale on solid black background for particles)
   - Dimensions and technical requirements
   - Example: *"White soft circular glow with gentle falloff on solid black background, game particle texture, 256x256px, seamless edges, no background detail --ar 1:1 --style raw"*
   - Midjourney prompts for weapons, bosses, anything like that CAN take inspiration from existing Terraria or Calamity assets, Coralite assets, etc. but should be adapted to fit the musical themes of MagnumOpus. For example, a sword slash effect for a Moonlight Sonata weapon might be described as: *"A sweeping arc of deep purple and ice blue light with shimmering star particles, on solid black background, game VFX texture, 512x512px, dynamic motion blur effect --ar 1:1 --style raw"*
3. **Expected file location** using the SandboxLastPrism layout pattern described below.

Do not use placeholder textures or skip VFX because an asset is missing. Ask for it. Also, do not create fallback textures or assets as, if the shaders or other VFX fail, it is imperative to know this upfront for the visual quality of the mod. Always request the correct asset with a detailed prompt if you do not have them readily available.

### Item-Specific Asset Placement (SandboxLastPrism Pattern)

All item-specific content (weapons, bosses, accessories, etc.) should follow the organizational pattern established by the Sandbox Last Prism. The SandboxLastPrism (and its Exoblade-style self-contained architecture) is a **recommended folder and file structure**  Eit demonstrates a working, well-organized approach for per-weapon content. You do **not** need to copy the exact implementation or code patterns of the Sandbox Exoblade; it is there as a reference for how to organize files and folders so that each weapon's systems (shaders, particles, primitives, utilities, projectiles, buffs) are self-contained and properly structured. Adapt the architecture to suit each weapon's unique needs.

This pattern splits item-specific assets across three root directories by purpose:

#### 1. VFX Textures ↁE`Assets/<ThemeName>/<ItemName>/`

VFX texture assets (PNG images used by shaders, trails, particles, bloom) go in theme-scoped subfolders of `Assets/`, organized by texture type:

```
Assets/<ThemeName>/<ItemName>/
├── Flare/               ELens flare, flash, and burst VFX textures
├── Gradients/           EColor gradient lookup textures (1D or 2D ramps)
├── Orbs/                ESoft glow circles, feathered spheres, bloom orbs
├── Pixel/               ETiny pixel-art particle sprites for ModDust types
├── Trails/              ETrail strip textures, energy lines, ribbon UVs
━E  └── Clear/           ETrail textures on transparent (not black) backgrounds
├── WeaponDesign/        EMidjourney reference prompts for the weapon sprite itself
└── [Class-Specific]/    EAdditional folders based on weapon class needs:
    ├── SlashArcs/       E(Melee) Swing arc overlay textures
    ├── ImpactSlash/     E(Melee) Hit impact slash/burst textures
    ├── Beams/           E(Magic/Summoner) Beam strip textures
    ├── ChannelingEffects/  E(Magic) Cast circle and channeling textures
    ├── MuzzleFlash/     E(Ranged) Barrel discharge flash textures
    ├── ImpactBurst/     E(Ranged) Explosive detonation impact textures
    └── SummonCircle/    E(Summoner) Summoning ritual circle textures
```

**Reference:** See `Assets/SandboxLastPrism/` for the canonical example (Flare/, Gradients/, Orbs/, Pixel/, Trails/, Trails/Clear/).

#### 2. Custom Shaders ↁE`Effects/<ThemeName>/<ItemName>/`

Item-specific `.fx` and `.fxc` shader files go in theme-scoped subfolders of `Effects/`, grouped by shader purpose:

```
Effects/<ThemeName>/<ItemName>/
├── <ShaderName>.fx          EShader source
├── <ShaderName>.fxc         ECompiled shader bytecode
└── <SubCategory>/           EGrouped by shader purpose when multiple exist:
    ├── Radial/              ERadial/circular effect shaders (sigils, auras)
    └── Scroll/              EUV-scrolling shaders (beams, trails, lasers)
```

**Reference:** See `Effects/SandboxLastPrism/` for the canonical example (GlowDustShader at root, Radial/, Scroll/ subdirectories).

#### 3. C# Code + Dust Textures ↁE`Content/<ThemeName>/<ItemName>/`

Item code, custom ModDust types, and dust sprite textures all co-locate under `Content/`:

```
Content/<ThemeName>/<Category>/<ItemName>/
├── <ItemName>.cs            EMain item/weapon class
├── <ItemName>VFX.cs         EItem-specific VFX static class
├── <ItemName>Swing.cs       E(Melee) Swing projectile
├── Dusts/                   ECustom ModDust types for this item
━E  ├── <DustName>.cs        EDust behavior code
━E  └── Textures/            EDust sprite .png files (co-located with dust code)
└── Systems/                 EItem-specific systems (flash, pixelation, screen shake)
    ├── <System>.cs
    └── ...
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
| **Clair de Lune** | Moonlit reverie | Night mist blue, soft blue, pearl white | Dreamlike calm, gentle luminescence |
| **Dies Irae** | Day of wrath | Blood red, dark crimson, ember orange | Fury, judgment, apocalyptic power |
| **Nachtmusik** | A little night music | Deep indigo, starlight silver, cosmic blue | Nocturnal wonder, stellar beauty |
| **Ode to Joy** | Universal brotherhood | Warm gold, radiant amber, jubilant light | Joy, celebration, triumph of spirit |

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
| Clair de Lune | `new Color(150, 200, 255)` - Ice Blue |
| Dies Irae | `new Color(200, 50, 30)` - Blood Red |
| Nachtmusik | `new Color(100, 120, 200)` - Starlight Indigo |
| Ode to Joy | `new Color(255, 200, 50)` - Warm Gold |

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
| `Effects/` | **Compiled shaders** (.fx) ready for use  Etrail shaders (SimpleTrailShader, MoonlightTrail, HeroicFlameTrail, CelestialValorTrail, EroicaFuneralTrail, ScrollingTrailShader), bloom shaders (SimpleBloomShader, SakuraBloom, MotionBlurBloom), beam shaders (BeamGradientFlow, LunarBeam, TerraBladeFlareBeamShader), screen effects (ScreenDistortion, RadialScrollShader), metaball shaders, and more |
| `ShaderSource/` | **Shader source files** (.fx) for development  EHLSLLibrary.fxh (shared utilities), advanced shaders (AdvancedBeamShader, AdvancedBloomShader, AdvancedTrailShader, AdvancedDistortionShader, AdvancedScreenEffectsShader), Calamity-inspired shaders (CalamityFireShader, ExobladeSlashShader), procedural trails (ProceduralTrailShader), and README_SHADER_COMPILATION.md for build instructions |
| `Assets/Shaders/` | Additional compiled shader assets |
| `Common/Systems/Shaders/` | **C# shader infrastructure**  EShaderLoader.cs (loads/manages shaders) and ShaderRenderer.cs (handles shader rendering) |

#### VFX Textures & Sprites

| Directory | Contents |
|-----------|----------|
| `Assets/VFX/` | **Main VFX texture library** with 15 subcategories: `Afterimages/`, `Beams/`, `Blooms/`, `Impacts/`, `Lightning/`, `LightRays/`, `LUT/` (color grading), `Masks/`, `Noise/` (19 noise types  Ecosmic energy, nebula, Voronoi, FBM, marble, perlin, etc.), `Overlays/`, `Ribbons/`, `Screen/`, `Smears/`, `Trails/` (comet trails, spiral trails, energy UV maps, ember scatters, sparkle fields) |
| `Assets/Particles Asset Library/` | **Particle sprites**  E107+ sprites including sparkles, glyphs, halos, explosions, lightning bursts, smoke, sword arcs, feathers, music notes, magic sparkle fields, flare spikes, circular masks, and themed particles |

#### VFX Code Systems

| Directory | Contents |
|-----------|----------|
| `Common/Systems/VFX/` | **Core VFX C# systems** with subsystems: `Beams/` (beam rendering), `Bloom/` (lens flares, god rays, glow), `Boss/` (boss arena/cinematic VFX), `Core/` (particle systems, texture registries, rendering utils), `Effects/` (afterimages, glow dust, smoke, screen shake), `Optimization/` (LOD, adaptive quality, batching), `Projectile/` (layered projectile rendering), `Screen/` (skyboxes, distortions, heat effects), `Themes/` (elemental/themed effects), `Trails/` (advanced trails, nebula, Bezier curves), `Weapon/` (glints, lens flares, fog), plus root files SwingShaderSystem.cs and VFXIntegration.cs |
| `Common/Systems/Particles/` | **Particle system code**  EParticleTextureGenerator.cs, CommonParticles.cs, DynamicParticles.cs, SmearParticles.cs, MagnumParticleHandler.cs, MagnumParticleDrawLayer.cs, Particle.cs, plus `Textures/` subdirectory |

#### Theme-Specific VFX

| Directory | Contents |
|-----------|----------|
| `Content/<ThemeName>/VFX/` | Per-theme VFX implementations (e.g., `Content/MoonlightSonata/VFX/` has subdirectories for specific bosses/weapons: Accessories, EternalMoon, GoliathOfMoonlight, IncisorOfMoonlight, MoonlightsCalling, ResurrectionOfTheMoon) |

**Always check these directories before creating new shaders or VFX assets.** Reuse existing shaders, textures, and systems where possible.

---

## Suggestions & Inspiration (Recommendation Only)

The following are **non-mandatory recommendations** for VFX and shader reference. These are not rules  Ethey are starting points for inspiration when designing weapon effects, trails, bloom, and shader-driven visuals.

### Calamity's Miracle Matter Weapons (Exoblade, etc.)

The **Exoblade** and the other endgame Calamity weapons crafted from **Miracle Matter** are exceptional references for high-quality VFX, shader usage, and asset pipelines. They demonstrate:

- **Multi-layered primitive trail rendering** with shader-driven color gradients and UV scrolling
- **Slash VFX arcs** with procedural vertex meshes and custom fragment shaders
- **Bloom stacking techniques** for weapon glow, projectile cores, and impact flashes
- **Afterimage / motion blur systems** that convey speed and weight
- **Per-weapon visual identity**  Eeach Miracle Matter weapon has a completely distinct look despite sharing the same crafting material, achieved through unique shaders, color palettes, and particle choreography
- **Screen effects on critical moments**  Escreen shake, flash, distortion timed to gameplay impact

**Where to find them locally:**
```
C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo
```

Key files to study (search within the repo):
- `Exoblade.cs`  EThe weapon item and its swing projectile logic
- `ExobladeSlashShader.fx`  ECustom slash arc shader
- Any files referencing `MiracleMatter` or `Exo` prefix  Erelated weapons share the pipeline
- `Galaxia.cs`, `ArkoftheCosmos.cs`, `ArkoftheElements.cs`  EOther top-tier melee weapons with distinct VFX approaches

### How This Applies to MagnumOpus

MagnumOpus is a **music-based mod**. When adapting these references, always apply a musical twist:

- Where Calamity uses raw energy VFX, MagnumOpus should weave in **musical motifs**  Emusic note particles, rhythmic pulsing tied to beat timing, harmonic resonance waves, conductor's baton flourishes
- Where Calamity uses elemental color palettes, MagnumOpus should use the **theme-specific palettes** defined in `MoonlightSonataPalette.cs` and each weapon's VFX class
- Where Calamity uses generic impact effects, MagnumOpus should use **thematically unique custom dusts**  Eeach weapon family has its own ModDust types (LunarMote, StarPointDust, GravityWellDust, etc.)
- Trail shaders should incorporate **standing wave patterns, frequency oscillation, and harmonic node highlights** rather than simple energy flows
- The goal is: take the technical quality and layered complexity of Calamity's best weapons, but make every visual element sing with the mod's musical identity

### VFX Asset Recommendations by Weapon Class

Each weapon's VFX asset folder (Midjourney prompts, textures) should focus on effects that are natural for that weapon class. These are starting points  Efeel free to invent new effect types when inspiration strikes.

- **Melee weapons**: Prioritize swing slash arcs, impact slash effects, blade trail textures, hit explosion flares, and projectile textures for any thrown or launched attacks.
- **Magic weapons**: Focus on projectile orbs, beams, lightning bolts, arcane flames, channeling effects, and other dazzling visual phenomena that sell the feeling of raw magical power.
- **Ranged weapons**: Emphasize muzzle flash flares, bullet/projectile trail textures, beam textures, explosive impact bursts, and shell casing / debris effects.
- **Summoner weapons**: Consider all of the above  Esummoned minions can melee, shoot projectiles, cast beams, and create explosions. Design assets that support the full range of exciting summoner attack patterns.

### Calamity Design References by Weapon Class

When designing a new weapon, these Calamity weapons are good references for the visual quality and complexity to aim for. Search the local Calamity repo to study their implementations.

- **Melee**: Exoblade, Celestus, Ark of the Cosmos, Galaxia  Estudy their swing arcs, layered trails, slash shaders, and impact choreography
- **Magic**: Subsuming Vortex, Vivid Clarity  Estudy their projectile rendering, beam effects, channeling visuals, and particle cascades
- **Ranged**: Photoviscerator, Magnomaly Cannon, Heavenly Gale  Estudy their muzzle flash systems, projectile trails, explosive impacts, and screen effects
- **Summoner**: Cosmic Immaterializer  Estudy how the summoned entity attacks with varied VFX (beams, projectiles, slams) and how the staff itself has summoning ritual effects

These are recommendations, not constraints. The best MagnumOpus weapons will take inspiration from these references while developing their own unique musical identity.

---

## Shader-Driven VFX (Recommended Foundation)

Custom HLSL shaders are the recommended foundation for advanced visual effects in MagnumOpus. While not every weapon needs custom shaders, weapons that aspire to high visual quality should aim for **multiple shader systems working together**  Etrails, bloom overlays, auras, beam rendering, screen distortions  Elayered to create a rich, cohesive look.

### Workflow: Reference Repos First, Then Shaders

Before writing any new shader or VFX system, **always study how the reference repositories solve the same problem**:

1. **Search Calamity, Coralite, and VFX+** for the specific effect type you need (trail, beam, bloom, distortion, etc.). Read the full implementation  Ethe C# rendering code, the `.fx` shader source, and how they wire together.
2. **Understand the technique**  Ewhat shader uniforms drive the effect? How are vertices constructed? What blend states are used? How do they handle fallback when shaders aren't available?
3. **Adapt for MagnumOpus**  Ebuild your version using MagnumOpus's existing shader infrastructure (`ShaderLoader.cs`, `ShaderRenderer.cs`, `MoonlightSonataShaderManager.cs`, etc.) and the mod's musical identity.
4. **Layer multiple shaders** where it makes sense  Ea weapon that combines a custom trail shader, a bloom overlay shader, and an aura shader will look dramatically more polished than one relying on particles alone.

### What Shaders Are Good For

Shaders excel at effects that are difficult or expensive to achieve with CPU-side particles alone:

- **Trail rendering**: UV-scrolling energy patterns, tidal wave textures, gradient-sampled ribbons that flow smoothly along a path (see `TidalTrail.fx`, `ScrollingTrailShader.fx`)
- **Bloom and glow overlays**: Soft procedural glow shapes, crescent bloom, lens flares rendered as screen-space quads (see `CrescentBloom.fx`, `SimpleBloomShader.fx`)
- **Auras and overlays**: Radial effects around the player or weapon  Econcentric rings, sigil rotations, gravity distortions (see `LunarPhaseAura.fx`, `GravitationalRift.fx`, `SummonCircle.fx`)
- **Beam effects**: Gradient-mapped beam bodies with noise distortion, spectral splitting, intensity pulsing (see `LunarBeam.fx`, `BeamGradientFlow.fx`)
- **Screen-space effects**: Distortion, chromatic aberration, heat haze, screen slice (see `ScreenDistortion.fx`)

### Recommended Shader Coverage Per Weapon

A well-developed weapon might include some or all of these shader layers:

| Shader Layer | Purpose | Example |
|-------------|---------|---------|
| Trail shader | Renders the weapon's swing trail or projectile trail with GPU-driven patterns | `TidalTrail.fx` for Eternal Moon's tidal wake |
| Bloom/glow shader | Adds soft glow overlays to the weapon, projectiles, or impact points | `CrescentBloom.fx` for crescent-shaped bloom |
| Aura shader | Provides ambient visual presence (hold effects, charge indicators, summon circles) | `LunarPhaseAura.fx`, `SummonCircle.fx` |
| Beam shader | Powers beam-type attacks with smooth gradient flow and noise | `LunarBeam.fx` for moonlight beams |
| Distortion shader | Screen-space distortion for high-impact moments (finishers, boss phase transitions) | `ScreenDistortion.fx` |

Not every weapon needs all five layers. A simpler weapon might use just a trail shader and rely on particles for everything else. A flagship weapon might use all five plus additional custom effects. Use your judgment based on the weapon's importance and visual ambitions.

### Shader + Particle Synergy

Shaders and particles are complementary, not competing:

- Use **shaders** for smooth, continuous effects (trails, beams, glowing overlays, distortions)
- Use **particles** for discrete, scattered accents (dust motes, sparks, music notes, impact debris)
- Use **custom ModDust types** for theme-specific particles that have unique behavior (homing, orbiting, fading gradients)
- The best effects combine all three  Ea shader-driven trail with particle sparks flying off its edges and custom dust motes drifting in its wake

### Existing Shader Infrastructure

Before creating new shaders, check what's already available:

- **`ShaderLoader.cs`** (`Common/Systems/Shaders/`)  ELoads and manages all shader instances. Add new shaders here.
- **`ShaderRenderer.cs`**  EProvides rendering utilities for shader-driven effects.
- **Per-theme shader managers** (e.g., `MoonlightSonataShaderManager.cs`)  ETheme-specific helpers for binding palettes, time uniforms, and texture parameters.
- **`Effects/` directory**  EContains all compiled `.fx` and `.fxc` files, organized by theme and weapon.
- **`ShaderSource/` directory**  EContains shader source files and `HLSLLibrary.fxh` with shared HLSL utilities.

### Shader Placement (SandboxLastPrism Pattern)

New shaders follow the same folder structure as all other assets:

```
Effects/<ThemeName>/<WeaponName>/
├── <ShaderName>.fx          EShader source
├── <ShaderName>.fxc         ECompiled shader bytecode
└── <SubCategory>/           EGrouped by purpose when multiple exist
```

See `Effects/SandboxLastPrism/` and `Effects/MoonlightSonata/` for canonical examples.

## VFX Compositing — Toolkit, Techniques & Decision-Making

> *Creating polished effects from minimal assets is about understanding how textures, shaders, blend modes, and layering combine. This section teaches the visual vocabulary — not rigid recipes.*

### Core Philosophy: Ask First, Build Second

**Before implementing ANY visual effect, ASK the user these questions:**

1. **What is the visual identity of this specific weapon/effect?** (Sharp and crystalline? Organic and flowing? Ethereal and ghostly? Violent and explosive? Graceful and sweeping?)
2. **What emotional weight should the effect carry?** (Subtle ambient glow? Dramatic screen-commanding presence? Understated elegance? Raw overwhelming power?)
3. **What existing effects in MagnumOpus or the reference repos are closest to what they want?** (Get a concrete anchor point before designing.)
4. **How important is this effect in the weapon's hierarchy?** (Is this the signature move that defines the weapon, or a secondary accent? This determines how many layers and how much complexity to invest.)
5. **Are there specific techniques they want used or avoided?** (Maybe they want noise distortion, maybe they explicitly don't. Maybe they want SDF math instead of texture masks. Ask.)
6. **What assets already exist that could serve this effect?** (Check `Assets/VFX/` subfolders and the weapon's own asset folder before requesting new ones.)

**Do NOT assume the answers.** Different weapons within the same theme should use different techniques — that's what makes them unique. The user's creative direction determines the approach.

### The Compositing Vocabulary

These are the fundamental building blocks for constructing visual effects. Each one is a tool with strengths and trade-offs. Understanding them means knowing WHEN to reach for each one — not applying all of them every time.

#### Texture-Based Techniques

**UV-Scrolled Textures**
Animates a texture along a mesh (trail strip, beam quad) by offsetting UV coordinates over time. The texture tiles or stretches along the mesh, creating the illusion of flowing energy, fire, water, or any continuous moving surface.

```hlsl
// The basic idea — scroll a texture along UV.x over time
float2 scrolledUV = float2(uv.x + time * scrollSpeed, uv.y);
float4 color = tex2D(bodyTex, scrolledUV);
```

- **Best for**: Trails, beams, ribbons, any long continuous shape that needs internal movement
- **Character depends entirely on the texture**: A smooth gradient texture scrolls cleanly. A noisy texture scrolls chaotically. A structured texture (staff lines, wave patterns) scrolls rhythmically.
- **Reference textures**: `Assets/VFX/Trails/`, `Assets/VFX/Beams/`, `Assets/VFX/Ribbons/`

**Noise Distortion**
Offsets UV coordinates using values sampled from a noise texture, warping the visual output of another texture or shape. Creates organic, living movement.

```hlsl
// Sample noise to get a distortion offset
float2 noiseUV = uv * noiseScale + time * noiseScrollSpeed;
float2 distortion = (tex2D(noiseTex, noiseUV).rg - 0.5) * distortStrength;
// Apply distortion to the main texture's UVs
float4 color = tex2D(mainTex, uv + distortion);
```

- **Best for**: Organic energy, fire, magical auras, anything that should feel alive and unpredictable
- **NOT good for**: Clean geometric effects, sharp crystalline visuals, precise mechanical shapes
- **Different noise textures create radically different feels**:

| Noise Type | Visual Character | Feels Like |
|-----------|-----------------|------------|
| Perlin | Smooth, flowing, cloud-like | Gentle wind, calm water, soft magic |
| FBM (Fractal Brownian Motion) | Layered, detailed, turbulent | Fire, roiling energy, storm clouds |
| Voronoi / Cellular | Cell-like, cracked, crystalline | Ice fractures, shattered glass, geometric energy |
| Marble / Swirl | Veined, flowing, directional | Liquid, blood flow, cosmic swirls |
| Cosmic / Nebula | Space-like, vast, colorful | Celestial effects, astral energy, cosmic power |

- **Reference textures**: `Assets/VFX/Noise/` (19 types available — read the filenames to understand their character)

**Alpha Masking**
Multiplies a mask texture against an effect to control where it's visible. Shapes, feathers, or cuts the effect without changing the underlying technique.

```hlsl
// Mask controls visibility — white = fully visible, black = invisible
float maskValue = tex2D(maskTex, uv).a;
finalColor *= maskValue;
```

- **Best for**: Shaping falloff (soft edges, tapered tips), creating rings/crescents from solid shapes, feathering hard edges, vignetting
- **Reference textures**: `Assets/VFX/Masks/`

**Color Ramp / LUT Sampling**
Maps a grayscale intensity value to a color gradient by sampling a 1D texture. Instead of coloring effects with uniform tints, this lets intensity drive color — hot cores are one color, cool edges are another.

```hlsl
// intensity is 0.0 (cold/edge) to 1.0 (hot/core)
float intensity = baseTex.r * edgeFade * tipFade;
float4 themedColor = tex2D(colorRampTex, float2(intensity, 0));
```

- **Best for**: Consistent theme coloring across different effect types, temperature-mapped effects (hot core → cool edge), ensuring multiple weapons in a theme share a cohesive palette without looking identical
- **Reference textures**: `Assets/VFX/LUT/`

#### Shader Math Techniques (No Texture Required)

**Smoothstep Edge Fading**
Uses `smoothstep()` to create soft transitions based on UV coordinates. Essential for trail edges, beam edges, and tip fade-outs.

```hlsl
// Soft edge fade — uv.y is 0 at one edge, 1 at the other, with 0.5 at center
float edgeFade = smoothstep(0.0, 0.15, uv.y) * smoothstep(1.0, 0.85, uv.y);
// Tip fade — uv.x is 0 at start, 1 at tip
float tipFade = smoothstep(1.0, 0.7, uv.x);
```

- **Best for**: Every trail and beam needs edge handling. This is a fundamental building block.

**SDF (Signed Distance Field) Math**
Computes shape boundaries mathematically rather than from textures. Gives perfectly sharp or perfectly smooth edges at any resolution.

```hlsl
// Circle SDF — distance from center
float dist = length(uv - 0.5) * 2.0;
float circle = smoothstep(radius + softness, radius - softness, dist);

// Ring SDF
float ring = smoothstep(outerRadius + soft, outerRadius - soft, dist)
           - smoothstep(innerRadius + soft, innerRadius - soft, dist);
```

- **Best for**: Clean geometric shapes, expanding rings/shockwaves, procedural auras, anything that should feel precise and mathematical rather than organic
- **Pairs well with**: Noise distortion applied to the distance field for organic-geometric hybrids

**Procedural Animation (sin, cos, frac, etc.)**
Shader-computed oscillation, pulsing, wave patterns without any texture dependencies.

```hlsl
// Pulsing glow
float pulse = 0.8 + 0.2 * sin(time * pulseSpeed);
// Standing wave pattern (musical!)
float wave = sin(uv.x * frequency + time * speed) * amplitude;
// Harmonic node highlights
float harmonic = abs(sin(uv.x * PI * nodeCount));
```

- **Best for**: Rhythmic pulsing, standing waves, harmonic patterns, anything that should feel like it has a heartbeat or musical timing
- **Especially relevant for MagnumOpus**: Standing wave math, harmonic nodes, and frequency-based patterns are naturally musical

#### Blend Modes

How draw calls combine with what's already on screen. Choosing the wrong blend mode is one of the most common reasons an effect looks wrong.

| Blend Mode | SpriteBatch State | What It Does Visually | When To Use |
|-----------|------------------|----------------------|-------------|
| **Additive** | `BlendState.Additive` | Adds light. Colors stack and brighten. Black = invisible. | Glow, energy, fire, bloom, anything luminous. THE default for VFX overlays. |
| **Alpha Blend** | `BlendState.AlphaBlend` | Standard transparency. Can darken and occlude. | Smoke, solid shapes, anything that should block what's behind it. |
| **Multiply** | Custom: `Src=DestColor, Dest=Zero` | Darkens. White = no change, black = full darken. | Shadows, dark overlays, screen vignettes. Rarely used for weapon VFX. |
| **Screen** | Custom: `Src=One, Dest=InvSrcColor` | Lightens softly without the blowout of additive. | Subtle glow, soft light, effects that should brighten without becoming blindingly white. |

**Critical insight**: Additive blending makes black pixels invisible. This is why VFX textures are typically bright shapes on black backgrounds — the black disappears, leaving only the glow. If your texture has a non-black background and you use additive blending, the background will add unwanted light to the scene.

#### Multi-Layer Compositing

The difference between a flat effect and a rich one is almost always layering. But the NUMBER, TYPE, and STYLE of layers depends entirely on the effect's identity.

**What layering means in practice:**

```
An effect has visual DEPTH when you can perceive:
- A hot/bright core vs. a cooler/dimmer outer region
- Internal detail or movement vs. an overall shape
- Scattered accents that break the silhouette
- Interaction with the environment (glow on nearby surfaces, screen effects)
```

**Ways to achieve layering:**
- **Multiple draw passes** at different scales/opacities (stacked bloom sprites)
- **Multiple shader passes** with different parameters (body pass + core pass)
- **Shader-internal layering** (one shader samples multiple textures and combines them)
- **Mixed GPU + CPU** (shader trail + particle accents spawned along edges)
- **Temporal layering** (afterimages from previous frames create depth over time)

**There is no correct number of layers.** A subtle ambient glow might need 2 layers. A boss's ultimate attack might need 8. The weapon's importance and the moment's dramatic weight determine the investment.

**Multi-scale bloom stacking** (a common and effective technique):

```csharp
// Drawing the same soft texture at multiple scales with decreasing opacity
// creates a convincing glow without any shader at all.
// The specific colors, scales, and opacities depend on the effect.

// Tight bright core
spriteBatch.Draw(bloomTex, pos, null, brightColor * coreOpacity, 0f, origin, smallScale, SpriteEffects.None, 0f);
// Medium glow
spriteBatch.Draw(bloomTex, pos, null, midColor * midOpacity, 0f, origin, mediumScale, SpriteEffects.None, 0f);
// Wide soft ambient
spriteBatch.Draw(bloomTex, pos, null, outerColor * outerOpacity, 0f, origin, largeScale, SpriteEffects.None, 0f);
```

This works because each layer contributes different visual information: the core says "this is the light source," the mid layer says "this is the glow radius," and the outer layer says "this light affects the surrounding area." Together they create depth.

#### Primitive Mesh Construction

For trails and beams, the C# side builds a triangle strip mesh that the GPU renders with a shader. The mesh is constructed from a series of positions (recorded over time for trails, or computed geometrically for beams) expanded into quads.

**How trail meshes work conceptually:**

```
Each frame, record the trail emitter's position.
For each recorded position, create two vertices offset perpendicular to the trail direction.
UV.x maps along the trail length (0 = oldest, 1 = newest — or reversed).
UV.y maps across the trail width (0 = one edge, 1 = other edge).
The shader receives these UVs and does all the visual work.
```

- **Study Calamity's PrimitiveTrail implementations** for production-quality mesh construction
- **MagnumOpus's existing trail systems** in `Common/Systems/VFX/Trails/` may already handle mesh construction — check before writing new code

#### Screen-Space Effects

Effects that modify the entire screen or a region of it. High-impact but should be used sparingly — overuse causes visual fatigue.

- **Screen distortion**: Warps the screen behind/around an effect (heat haze, gravity, spatial tears)
- **Chromatic aberration**: Splits RGB channels for prismatic fringing (cosmic effects, high-energy impacts)
- **Screen flash**: Brief white/colored overlay (big impacts, phase transitions)
- **Screen shake**: Camera offset (impacts, detonations) — technically not a shader but part of the VFX toolkit

**Reserve these for moments that DESERVE them.** A basic projectile impact doesn't need screen shake. A boss phase transition does.

### Quality Standards (Outcomes, Not Methods)

These define what a finished effect should achieve. HOW you achieve them is creative freedom guided by the weapon's identity.

1. **Visual Depth**: Important effects should not look flat. There should be a perceivable difference between core and edge, between the effect and its ambient influence. The technique for achieving this varies — it could be layered draw calls, shader-internal compositing, temporal afterimages, or something else entirely.

2. **Theme-Consistent Coloring**: The effect's colors must read as belonging to its theme at a glance. How you apply color — LUT ramps, vertex colors, shader uniforms, hardcoded values, procedural math — is your choice. The result matters.

3. **Edge Quality**: Effects should have intentional edges, not raw texture cutoffs. Whether that's a shader `smoothstep`, a mask texture, vertex alpha fade, feathered sprite edges, or SDF soft boundaries depends on the effect.

4. **Motion and Life**: Static effects feel dead. Something should move, pulse, scroll, oscillate, or evolve. A trail that doesn't scroll internally looks like a painted stripe. A bloom that doesn't pulse looks like a flat circle.

5. **Proportional Complexity**: Scale visual investment to the moment's importance. A passive hold effect is simpler than a swing trail, which is simpler than a finisher, which is simpler than a boss phase transition. Don't over-invest in ambient effects or under-invest in signature moments.

6. **Technical Cleanliness**: Effects should use the correct blend mode (additive for glow, alpha for smoke). Textures designed for black backgrounds should be drawn additively. Glow shouldn't occlude things behind it. Trails should fade at their tips, not cut off abruptly.

### Anti-Patterns (What To Avoid)

- **Single-pass flatness**: Drawing one texture with one tint and calling it a finished effect. Even two layers (body + glow) dramatically improve visual quality.
- **Noise on everything**: Noise distortion creates organic movement, but not every effect is organic. A crystalline ice slash, a precise laser beam, or a clean geometric sigil should NOT be noise-distorted. Choose techniques that match the visual identity.
- **Wrong blend mode**: Drawing glow/energy effects in alpha blend (they'll have visible dark edges from the black background). Drawing smoke/solid shapes in additive (they'll look ethereal instead of solid).
- **Copy-paste VFX between weapons**: Two weapons using identical effects with only color changes. Even within a theme, each weapon should use different techniques or apply shared techniques differently.
- **Inventing from scratch**: Always check reference repos (`Calamity`, `Coralite`, `VFX+`) and existing MagnumOpus code before writing new rendering systems. Someone has almost certainly solved a similar problem.
- **Over-engineering simple moments**: A subtle ambient particle doesn't need a custom shader with 5 texture inputs. Match complexity to importance.
- **Under-engineering signature moments**: A weapon's defining attack should not be a single `Main.spriteBatch.Draw()` call. This is where layering, shaders, and full compositing pay off.

### Asset Library Quick Reference

Before designing any effect, check what's already available. These are real directories in the project:

| Folder | What's Inside | Visual Character |
|--------|--------------|-----------------|
| `Assets/VFX/Trails/` | Strip textures for UV-mapped trail meshes | Comet trails, spiral energy, ember scatters, sparkle fields — various clean and noisy styles |
| `Assets/VFX/Beams/` | Strip textures for beam rendering | Designed for UV.x = along beam, UV.y = across beam |
| `Assets/VFX/Blooms/` | Soft glow shapes | Circular, feathered, radial falloff — for additive bloom stacking |
| `Assets/VFX/Masks/` | Alpha shapes for masking | Crescents, circles, rings, feathered edges — multiply against effects to shape them |
| `Assets/VFX/Noise/` | 19 noise texture types | Cosmic, nebula, Voronoi, FBM, marble, perlin, cellular — each has distinct character (see noise table above) |
| `Assets/VFX/Smears/` | Motion smear textures | Directional streaks, swing arc overlays |
| `Assets/VFX/Ribbons/` | Flowing ribbon textures | Graceful, curved, flowing continuous shapes |
| `Assets/VFX/Lightning/` | Bolt and arc textures | Electrical, branching, jagged energy |
| `Assets/VFX/Impacts/` | Burst and explosion textures | Radial impact shapes |
| `Assets/VFX/LightRays/` | God ray / radial light textures | Dramatic directional light, divine/cosmic moments |
| `Assets/VFX/LUT/` | Color gradient ramps | Sample with `tex2D(lut, float2(intensity, 0))` for themed color mapping |
| `Assets/VFX/Afterimages/` | Afterimage effect textures | Motion trails, ghosting, speed visualization |
| `Assets/VFX/Overlays/` | Screen overlay textures | Full-screen or regional overlay effects |
| `Assets/VFX/Screen/` | Screen-space effect textures | Distortion maps, transition masks |
| `Assets/Particles Asset Library/` | 107+ particle sprites | Sparkles, glyphs, halos, explosions, lightning, smoke, sword arcs, feathers, music notes, magic fields, flare spikes, circular masks |

**If an effect needs a texture that doesn't exist, STOP and request it with a Midjourney prompt.** Do not use placeholders or skip the effect.

### Decision Process (How To Approach a New Effect)

This is a thinking framework, not a checklist. Work through these considerations when designing any new visual effect:

1. **Understand the weapon's identity.** What does it feel like to wield? What's its musical soul? Is it aggressive or graceful? Chaotic or precise? This determines which techniques are appropriate.

2. **Identify the effect type.** Is this a trail, a beam, a bloom/glow, an impact, a projectile, an aura, a screen effect? Each has different technical foundations.

3. **Check existing implementations.** Search MagnumOpus's codebase for similar effects. Search Calamity, Coralite, and VFX+ for reference implementations. Read the actual code — don't guess.

4. **Check available assets.** Browse `Assets/VFX/` and the weapon's own asset folder. What textures already exist that could serve this effect? What's missing?

5. **Choose techniques that match the identity.** Organic flowing energy → noise distortion + scrolled textures. Sharp crystalline power → SDF math + clean edges. Ethereal ghostly presence → afterimages + soft bloom + low opacity. Explosive violent impact → radial burst + screen shake + particle shower. Musical resonance → standing wave math + harmonic pulsing.

6. **Determine layer count by importance.** Ambient/passive → 1-2 layers. Active effect → 2-4 layers. Signature attack → 3-6+ layers. Boss phase transition → as many as needed.

7. **Ask the user when uncertain.** If you're not sure whether an effect should be sharp or soft, noisy or clean, subtle or dramatic — ASK. Don't guess. The user's creative vision determines the direction.

### When In Doubt, Ask These Questions

If at any point during VFX implementation you are uncertain about direction, **stop and ask the user**. Specific questions are better than vague ones:

- "Should this trail feel organic and flowing (noise-distorted, soft edges) or clean and precise (sharp edges, geometric)?"
- "For the bloom on this projectile, should it be a tight focused glow or a wide ambient haze?"
- "This weapon's impact — should it feel like a detonation (radial burst, screen shake) or a slice (directional smear, clean cut)?"
- "I see [X noise texture] and [Y noise texture] in Assets/VFX/Noise/. Which character fits better — the cellular Voronoi look or the smooth Perlin look?"
- "Should this effect use a color ramp from Assets/VFX/LUT/ for its coloring, or is a simpler uniform tint more appropriate here?"
- "How visually prominent should this be? Is it a background accent or a center-stage moment?"
- "I found [reference implementation] in Calamity/Coralite — is this the direction you want, or should we diverge?"
- "This effect could work as a pure shader solution or as layered sprite draws. Do you have a preference for the rendering approach?"

**Asking questions is not a failure — it's how we ensure every effect matches the creative vision.** The goal is collaborative iteration, not autonomous guessing.