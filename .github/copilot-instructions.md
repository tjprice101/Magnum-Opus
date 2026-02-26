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

All item-specific content (weapons, bosses, accessories, etc.) **must** follow the same organizational pattern established by the Sandbox Last Prism. This splits item-specific assets across three root directories by purpose:

#### 1. VFX Textures → `Assets/<ThemeName>/<ItemName>/`

VFX texture assets (PNG images used by shaders, trails, particles, bloom) go in theme-scoped subfolders of `Assets/`, organized by texture type:

```
Assets/<ThemeName>/<ItemName>/
├── Flare/              — Lens flare, flash, and burst VFX textures
├── Gradients/          — Color gradient lookup textures (1D or 2D ramps)
├── Orbs/               — Soft glow circles, feathered spheres, bloom orbs
├── Pixel/              — Tiny pixel-art particle sprites for ModDust types
├── Trails/             — Trail strip textures, energy lines, ribbon UVs
│   └── Clear/          — Trail textures on transparent (not black) backgrounds
├── WeaponDesign/       — Midjourney reference prompts for the weapon sprite itself
└── [Class-Specific]/   — Additional folders based on weapon class needs:
    ├── SlashArcs/      — (Melee) Swing arc overlay textures
    ├── ImpactSlash/    — (Melee) Hit impact slash/burst textures
    ├── Beams/          — (Magic/Summoner) Beam strip textures
    ├── ChannelingEffects/ — (Magic) Cast circle and channeling textures
    ├── MuzzleFlash/    — (Ranged) Barrel discharge flash textures
    ├── ImpactBurst/    — (Ranged) Explosive detonation impact textures
    └── SummonCircle/   — (Summoner) Summoning ritual circle textures
```

**Reference:** See `Assets/SandboxLastPrism/` for the canonical example (Flare/, Gradients/, Orbs/, Pixel/, Trails/, Trails/Clear/).

#### 2. Custom Shaders → `Effects/<ThemeName>/<ItemName>/`

Item-specific `.fx` and `.fxc` shader files go in theme-scoped subfolders of `Effects/`, grouped by shader purpose:

```
Effects/<ThemeName>/<ItemName>/
├── <ShaderName>.fx         — Shader source
├── <ShaderName>.fxc        — Compiled shader bytecode
└── <SubCategory>/          — Grouped by shader purpose when multiple exist:
    ├── Radial/             — Radial/circular effect shaders (sigils, auras)
    └── Scroll/             — UV-scrolling shaders (beams, trails, lasers)
```

**Reference:** See `Effects/SandboxLastPrism/` for the canonical example (GlowDustShader at root, Radial/, Scroll/ subdirectories).

#### 3. C# Code + Dust Textures → `Content/<ThemeName>/<ItemName>/`

Item code, custom ModDust types, and dust sprite textures all co-locate under `Content/`:

```
Content/<ThemeName>/<Category>/<ItemName>/
├── <ItemName>.cs           — Main item/weapon class
├── <ItemName>VFX.cs        — Item-specific VFX static class
├── <ItemName>Swing.cs      — (Melee) Swing projectile
├── Dusts/                  — Custom ModDust types for this item
│   ├── <DustName>.cs       — Dust behavior code
│   └── Textures/           — Dust sprite .png files (co-located with dust code)
└── Systems/                — Item-specific systems (flash, pixelation, screen shake)
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
| `Effects/` | **Compiled shaders** (.fx) ready for use — trail shaders (SimpleTrailShader, MoonlightTrail, HeroicFlameTrail, CelestialValorTrail, EroicaFuneralTrail, ScrollingTrailShader), bloom shaders (SimpleBloomShader, SakuraBloom, MotionBlurBloom), beam shaders (BeamGradientFlow, LunarBeam, TerraBladeFlareBeamShader), screen effects (ScreenDistortion, RadialScrollShader), metaball shaders, and more |
| `ShaderSource/` | **Shader source files** (.fx) for development — HLSLLibrary.fxh (shared utilities), advanced shaders (AdvancedBeamShader, AdvancedBloomShader, AdvancedTrailShader, AdvancedDistortionShader, AdvancedScreenEffectsShader), Calamity-inspired shaders (CalamityFireShader, ExobladeSlashShader), procedural trails (ProceduralTrailShader), and README_SHADER_COMPILATION.md for build instructions |
| `Assets/Shaders/` | Additional compiled shader assets |
| `Common/Systems/Shaders/` | **C# shader infrastructure** — ShaderLoader.cs (loads/manages shaders) and ShaderRenderer.cs (handles shader rendering) |

#### VFX Textures & Sprites

| Directory | Contents |
|-----------|----------|
| `Assets/VFX/` | **Main VFX texture library** with 15 subcategories: `Afterimages/`, `Beams/`, `Blooms/`, `Impacts/`, `Lightning/`, `LightRays/`, `LUT/` (color grading), `Masks/`, `Noise/` (19 noise types — cosmic energy, nebula, Voronoi, FBM, marble, perlin, etc.), `Overlays/`, `Ribbons/`, `Screen/`, `Smears/`, `Trails/` (comet trails, spiral trails, energy UV maps, ember scatters, sparkle fields) |
| `Assets/Particles/` | **Particle sprites** — 107+ sprites including sparkles, glyphs, halos, explosions, lightning bursts, smoke, sword arcs, feathers, music notes, magic sparkle fields, flare spikes, circular masks, and themed particles |

#### VFX Code Systems

| Directory | Contents |
|-----------|----------|
| `Common/Systems/VFX/` | **Core VFX C# systems** with subsystems: `Beams/` (beam rendering), `Bloom/` (lens flares, god rays, glow), `Boss/` (boss arena/cinematic VFX), `Core/` (particle systems, texture registries, rendering utils), `Effects/` (afterimages, glow dust, smoke, screen shake), `Optimization/` (LOD, adaptive quality, batching), `Projectile/` (layered projectile rendering), `Screen/` (skyboxes, distortions, heat effects), `Themes/` (elemental/themed effects), `Trails/` (advanced trails, nebula, Bezier curves), `Weapon/` (glints, lens flares, fog), plus root files SwingShaderSystem.cs and VFXIntegration.cs |
| `Common/Systems/Particles/` | **Particle system code** — ParticleTextureGenerator.cs, CommonParticles.cs, DynamicParticles.cs, SmearParticles.cs, MagnumParticleHandler.cs, MagnumParticleDrawLayer.cs, Particle.cs, plus `Textures/` subdirectory |

#### Theme-Specific VFX

| Directory | Contents |
|-----------|----------|
| `Content/<ThemeName>/VFX/` | Per-theme VFX implementations (e.g., `Content/MoonlightSonata/VFX/` has subdirectories for specific bosses/weapons: Accessories, EternalMoon, GoliathOfMoonlight, IncisorOfMoonlight, MoonlightsCalling, ResurrectionOfTheMoon) |

**Always check these directories before creating new shaders or VFX assets.** Reuse existing shaders, textures, and systems where possible.

---

## Suggestions & Inspiration (Recommendation Only)

The following are **non-mandatory recommendations** for VFX and shader reference. These are not rules — they are starting points for inspiration when designing weapon effects, trails, bloom, and shader-driven visuals.

### Calamity's Miracle Matter Weapons (Exoblade, etc.)

The **Exoblade** and the other endgame Calamity weapons crafted from **Miracle Matter** are exceptional references for high-quality VFX, shader usage, and asset pipelines. They demonstrate:

- **Multi-layered primitive trail rendering** with shader-driven color gradients and UV scrolling
- **Slash VFX arcs** with procedural vertex meshes and custom fragment shaders
- **Bloom stacking techniques** for weapon glow, projectile cores, and impact flashes
- **Afterimage / motion blur systems** that convey speed and weight
- **Per-weapon visual identity** — each Miracle Matter weapon has a completely distinct look despite sharing the same crafting material, achieved through unique shaders, color palettes, and particle choreography
- **Screen effects on critical moments** — screen shake, flash, distortion timed to gameplay impact

**Where to find them locally:**
```
C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo
```

Key files to study (search within the repo):
- `Exoblade.cs` — The weapon item and its swing projectile logic
- `ExobladeSlashShader.fx` — Custom slash arc shader
- Any files referencing `MiracleMatter` or `Exo` prefix — related weapons share the pipeline
- `Galaxia.cs`, `ArkoftheCosmos.cs`, `ArkoftheElements.cs` — Other top-tier melee weapons with distinct VFX approaches

### How This Applies to MagnumOpus

MagnumOpus is a **music-based mod**. When adapting these references, always apply a musical twist:

- Where Calamity uses raw energy VFX, MagnumOpus should weave in **musical motifs** — music note particles, rhythmic pulsing tied to beat timing, harmonic resonance waves, conductor's baton flourishes
- Where Calamity uses elemental color palettes, MagnumOpus should use the **theme-specific palettes** defined in `MoonlightSonataPalette.cs` and each weapon's VFX class
- Where Calamity uses generic impact effects, MagnumOpus should use **thematically unique custom dusts** — each weapon family has its own ModDust types (LunarMote, StarPointDust, GravityWellDust, etc.)
- Trail shaders should incorporate **standing wave patterns, frequency oscillation, and harmonic node highlights** rather than simple energy flows
- The goal is: take the technical quality and layered complexity of Calamity's best weapons, but make every visual element sing with the mod's musical identity

### VFX Asset Recommendations by Weapon Class

Each weapon's VFX asset folder (Midjourney prompts, textures) should focus on effects that are natural for that weapon class. These are starting points — feel free to invent new effect types when inspiration strikes.

- **Melee weapons**: Prioritize swing slash arcs, impact slash effects, blade trail textures, hit explosion flares, and projectile textures for any thrown or launched attacks.
- **Magic weapons**: Focus on projectile orbs, beams, lightning bolts, arcane flames, channeling effects, and other dazzling visual phenomena that sell the feeling of raw magical power.
- **Ranged weapons**: Emphasize muzzle flash flares, bullet/projectile trail textures, beam textures, explosive impact bursts, and shell casing / debris effects.
- **Summoner weapons**: Consider all of the above — summoned minions can melee, shoot projectiles, cast beams, and create explosions. Design assets that support the full range of exciting summoner attack patterns.

### Calamity Design References by Weapon Class

When designing a new weapon, these Calamity weapons are good references for the visual quality and complexity to aim for. Search the local Calamity repo to study their implementations.

- **Melee**: Exoblade, Celestus, Ark of the Cosmos, Galaxia — study their swing arcs, layered trails, slash shaders, and impact choreography
- **Magic**: Subsuming Vortex, Vivid Clarity — study their projectile rendering, beam effects, channeling visuals, and particle cascades
- **Ranged**: Photoviscerator, Magnomaly Cannon, Heavenly Gale — study their muzzle flash systems, projectile trails, explosive impacts, and screen effects
- **Summoner**: Cosmic Immaterializer — study how the summoned entity attacks with varied VFX (beams, projectiles, slams) and how the staff itself has summoning ritual effects

These are recommendations, not constraints. The best MagnumOpus weapons will take inspiration from these references while developing their own unique musical identity.

---

## Shader-Driven VFX (Recommended Foundation)

Custom HLSL shaders are the recommended foundation for advanced visual effects in MagnumOpus. While not every weapon needs custom shaders, weapons that aspire to high visual quality should aim for **multiple shader systems working together** — trails, bloom overlays, auras, beam rendering, screen distortions — layered to create a rich, cohesive look.

### Workflow: Reference Repos First, Then Shaders

Before writing any new shader or VFX system, **always study how the reference repositories solve the same problem**:

1. **Search Calamity, Coralite, and VFX+** for the specific effect type you need (trail, beam, bloom, distortion, etc.). Read the full implementation — the C# rendering code, the `.fx` shader source, and how they wire together.
2. **Understand the technique** — what shader uniforms drive the effect? How are vertices constructed? What blend states are used? How do they handle fallback when shaders aren't available?
3. **Adapt for MagnumOpus** — build your version using MagnumOpus's existing shader infrastructure (`ShaderLoader.cs`, `ShaderRenderer.cs`, `MoonlightSonataShaderManager.cs`, etc.) and the mod's musical identity.
4. **Layer multiple shaders** where it makes sense — a weapon that combines a custom trail shader, a bloom overlay shader, and an aura shader will look dramatically more polished than one relying on particles alone.

### What Shaders Are Good For

Shaders excel at effects that are difficult or expensive to achieve with CPU-side particles alone:

- **Trail rendering**: UV-scrolling energy patterns, tidal wave textures, gradient-sampled ribbons that flow smoothly along a path (see `TidalTrail.fx`, `ScrollingTrailShader.fx`)
- **Bloom and glow overlays**: Soft procedural glow shapes, crescent bloom, lens flares rendered as screen-space quads (see `CrescentBloom.fx`, `SimpleBloomShader.fx`)
- **Auras and overlays**: Radial effects around the player or weapon — concentric rings, sigil rotations, gravity distortions (see `LunarPhaseAura.fx`, `GravitationalRift.fx`, `SummonCircle.fx`)
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
- The best effects combine all three — a shader-driven trail with particle sparks flying off its edges and custom dust motes drifting in its wake

### Existing Shader Infrastructure

Before creating new shaders, check what's already available:

- **`ShaderLoader.cs`** (`Common/Systems/Shaders/`) — Loads and manages all shader instances. Add new shaders here.
- **`ShaderRenderer.cs`** — Provides rendering utilities for shader-driven effects.
- **Per-theme shader managers** (e.g., `MoonlightSonataShaderManager.cs`) — Theme-specific helpers for binding palettes, time uniforms, and texture parameters.
- **`Effects/` directory** — Contains all compiled `.fx` and `.fxc` files, organized by theme and weapon.
- **`ShaderSource/` directory** — Contains shader source files and `HLSLLibrary.fxh` with shared HLSL utilities.

### Shader Placement (SandboxLastPrism Pattern)

New shaders follow the same folder structure as all other assets:

```
Effects/<ThemeName>/<WeaponName>/
├── <ShaderName>.fx         — Shader source
├── <ShaderName>.fxc        — Compiled shader bytecode
└── <SubCategory>/          — Grouped by purpose when multiple exist
```

See `Effects/SandboxLastPrism/` and `Effects/MoonlightSonata/` for canonical examples.

