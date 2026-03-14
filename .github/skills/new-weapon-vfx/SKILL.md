---
description: "Design a new weapon's complete VFX from scratch — guides through creative direction, asset auditing, technique selection, layer planning, reference repo research, and implementation following the SandboxLastPrism folder pattern. Use when creating any new weapon's visual effects."
---

# New Weapon VFX Design Workflow

This skill guides you through designing and implementing a complete VFX suite for a new weapon in MagnumOpus.

## Step 1: Invoke @creative-director FIRST

**MANDATORY.** Before any technical VFX work, invoke the @creative-director agent to run a full interactive concept dialog. The creative-director will:
- Ask 15+ interactive questions across 4-5 rounds (back-and-forth, each answer shapes next)
- Generate 2-3 weapon concept proposals with full VFX vision
- Ensure the concept is unique within its theme
- Produce a locked creative direction before technical work begins

If this is not a complete new weapon (e.g., just adding trails or improving existing VFX), skip to Step 2 but still ask the user 3-4 creative direction questions.

## Step 2: Identify the Weapon

Gather these details (may already be answered by creative-director dialog):
- **Weapon class**: Melee, Magic, Ranged, or Summoner
- **Theme**: Which musical score (Moonlight Sonata, Eroica, La Campanella, etc.)
- **Weapon name**: For folder naming and identity
- **Musical soul**: What aspect of the theme does this weapon embody?

## Step 3: Ask Creative Direction Questions (if not already covered by Step 1)

Before designing anything, have an interactive back-and-forth dialog with the user:

**Round 1: Visual Identity (3-4 questions)**
1. "Should this weapon feel sharp/crystalline, organic/flowing, ethereal/ghostly, violent/explosive, or graceful/sweeping?"
2. "Should its effects be subtle ambient, dramatic screen-commanding, understated elegant, or raw overwhelming power?"
3. "Is this a flagship weapon for the theme, or a supporting weapon? This determines VFX complexity budget."
4. "Are there any reference weapons/effects (from MagnumOpus, Calamity, WotG, or other games) you want to draw from?"

**Round 2: Technical Preferences (based on Round 1 answers)**
5. "Based on your answers, I'm thinking [technique A] or [technique B] — do either resonate?"
6. "Any techniques to USE or AVOID? (Noise distortion vs clean edges, shader-heavy vs particle-heavy?)"
7. "Music note visibility: should notes be physically present in the effects, or is musical identity expressed purely through rhythm and dynamics?"

**Round 3: Uniqueness Check (share findings, ask for direction)**
8. "I found N existing [class] weapons in [theme]. Here's what they do: [summary]. Which direction feels MOST different from these?"

## Step 4: Check Uniqueness Within Theme
```
Content/<ThemeName>/
```

For each existing weapon of the same class, note:
- What trail type they use (ribbon, afterimage, primitive, none)
- What particle effects they use (bloom, sparks, notes, themed particles)
- What special mechanics they have (charge, orbit, chain, burst)
- What shader effects they use (distortion, color ramp, UV scroll)

**The new weapon MUST be meaningfully different from all existing weapons in the same theme + class.**

## Step 5: Audit Available Assets

Search these locations systematically:

### VFX Textures
- `Assets/VFX Asset Library/BeamTextures/` — 14 beam strips
- `Assets/VFX Asset Library/ColorGradients/` — 12 theme LUT ramps
- `Assets/VFX Asset Library/GlowAndBloom/` — 8 bloom/flare sprites
- `Assets/VFX Asset Library/ImpactEffects/` — 8 impact textures
- `Assets/VFX Asset Library/NoiseTextures/` — 20 noise types
- `Assets/VFX Asset Library/MasksAndShapes/` — 7 masks
- `Assets/VFX Asset Library/TrailsAndRibbons/` — 4 trail strips
- `Assets/VFX Asset Library/SlashArcs/` — 4 sword arc textures
- `Assets/VFX Asset Library/Projectiles/` — 7 projectile sprites

### Particle Sprites
- `Assets/Particles Asset Library/` — Music notes, stars
- Theme-specific particle folders

### Existing Shaders
- `Effects/` — 30+ compiled shaders
- `Effects/<ThemeName>/` — Theme-specific shaders

### Foundation Weapons
- `Content/FoundationWeapons/` — Reference implementations by VFX type

**If ANY needed asset doesn't exist** — STOP. Provide a detailed Midjourney prompt and expected file location. Do not continue until assets are confirmed.

## Step 6: Design Effect Layers

Plan the VFX as distinct layers. A standard weapon should have at least 3 layers; a flagship weapon should have 5+.

### Per Weapon Class

**Melee Weapons:**
| Layer | Purpose | Example Technique |
|-------|---------|------------------|
| Swing trail | Primary visual identity | Primitive trail with shader, smear overlay |
| Impact effect | Hit feedback | Particle burst + bloom + screen shake (small) |
| Ambient/hold | Passive presence | Gentle glow, orbiting particles, subtle aura |
| Special mechanic | Unique gameplay moment | Charged finisher, combo escalation, projectile release |
| Accents | Polish layer | Music notes trailing, sparkle edges, theme particles |

**Magic Weapons:**
| Layer | Purpose | Example Technique |
|-------|---------|------------------|
| Projectile rendering | Primary visual identity | Layered bloom + trail + themed particles |
| Cast/channel effect | Feedback during use | Cast circle, hand glow, channeling particles |
| Impact/explosion | Hit feedback | Multi-layered burst + screen flash |
| Ambient/hold | Passive presence | Orb glow, magic circle, floating particles |
| Accents | Polish layer | Color ramp shifts, music notes, prismatic sparkle |

**Ranged Weapons:**
| Layer | Purpose | Example Technique |
|-------|---------|------------------|
| Projectile trail | Primary visual identity | Comet trail, ribbon, energy streak |
| Muzzle flash | Fire feedback | Burst + light spike at barrel |
| Impact/explosion | Hit feedback | Radial burst + debris particles |
| Barrel glow | Passive presence | Subtle glow at weapon tip |
| Accents | Polish layer | Shell casings, smoke wisps, theme particles |

**Summoner Weapons:**
| Layer | Purpose | Example Technique |
|-------|---------|------------------|
| Summon circle | Summoning ritual | Radial shader, ascending particles |
| Minion rendering | Minion visual identity | Glow + trail + themed particles on the minion |
| Minion attacks | Attack variety | Beams, projectiles, melee swings (varied VFX each) |
| Staff hold effect | Passive presence | Subtle glow, connection line to minion |
| Accents | Polish layer | Music notes on summon, harmonic pulse on attacks |

## Step 7: Check Foundation Weapons

Browse `Content/FoundationWeapons/` for systems matching your effect needs:

| Foundation | Use For |
|-----------|---------|
| AttackFoundation | Base projectile patterns, 5-mode attack cycling |
| AttackAnimationFoundation | Cinematic timed sequences, camera + VFX sync |
| SwordSmearFoundation | 3-layer melee sword smear VFX |
| ThinSlashFoundation | Precision slash arcs (SDF-based) |
| XSlashFoundation | Cross-slash patterns |
| LaserFoundation | Full-width laser beams |
| ThinLaserFoundation | Thin precision laser lines |
| InfernalBeamFoundation | Fire/energy beams |
| MagicOrbFoundation | Glowing sphere projectiles (shader-masked) |
| SparkleProjectileFoundation | Bloom-rendered projectiles (Photoviscerator-style) |
| RibbonFoundation | 10 flowing ribbon trail modes |
| ImpactFoundation | Multi-layered hit effects (3 shader types) |
| ExplosionParticlesFoundation | CPU physics radial burst particles |
| SmokeFoundation | Soft billowing smoke lifecycle |
| MaskFoundation | Alpha mask shaping |
| Foundation4PointSparkle | 4-pointed sparkle particles |
| FoundationIncisorOrbs | Orbiting sub-projectiles |

Use these as structural skeletons — customize colors, textures, timing, and layering to match the weapon's unique identity.

## Step 8: Research Reference Repos

For the specific techniques you plan to use, search the reference repositories:

| Technique Needed | Search In |
|-----------------|-----------|
| Primitive trails | Calamity: `ExobladeProj.cs`, `PrimitiveRenderer.cs` |
| Slash shaders | Calamity: `ExobladeSlashShader.fx` |
| Screen distortion | WotG: `Core/Graphics/ArbitraryScreenDistortion/` |
| Hierarchical bloom | Everglow: `BloomPipeline.cs` |
| GPU particles | WotG: `Core/Graphics/FastParticleSystems/` |
| Metaballs | Calamity: `Graphics/Metaballs/`, MagnumOpus: `Common/Systems/Metaballs/` |

Read the FULL implementation — C# rendering code, .fx shader source, how they wire together. Then adapt for MagnumOpus.

## Step 9: Request Missing Assets

For EVERY texture/sprite the weapon needs that doesn't exist:

1. Describe what asset is needed and where it would be used
2. Provide a detailed **Midjourney prompt**:
   - Art style and medium
   - Subject description with specific visual details
   - Color palette (white/grayscale on solid black background for VFX textures)
   - Dimensions (256x256 particles, 512x128 trails, etc.)
   - Technical requirements (seamless tiling, transparent edges, etc.)
3. Expected file location:
   - VFX textures: `Assets/<ThemeName>/<WeaponName>/<TextureType>/`
   - Shaders: `Effects/<ThemeName>/<WeaponName>/`
   - Dust sprites: `Content/<ThemeName>/<Category>/<WeaponName>/Dusts/Textures/`

## Step 10: Creative Asset Reuse Check

Before requesting ANY new assets, apply creative reuse thinking:
- Can an existing bloom sprite produce this effect at a different scale/tint/rotation?
- Can an existing noise texture + color ramp create this look via shader?
- Can particle choreography (spawn pattern, velocity, lifetime) make existing sprites look entirely new?

Only request new assets when creative reuse genuinely cannot achieve the desired result.

## Step 11: Implement

Create files following the SandboxLastPrism folder pattern:

```
Content/<ThemeName>/<Category>/<WeaponName>/
├── <WeaponName>.cs           — Main item class
├── <WeaponName>VFX.cs        — VFX static helpers
├── <WeaponName>Swing.cs      — Swing projectile (melee)
├── Dusts/
│   ├── <DustName>.cs         — Custom ModDust
│   └── Textures/             — Dust sprites
└── Systems/                  — Weapon-specific systems
```

## Step 10: Quality Checklist

Before considering the weapon complete, verify:

- [ ] **Theme colors** — Effect colors match the theme palette consistently
- [ ] **Edge quality** — No hard texture cutoffs. Smoothstep fading, feathered edges, or mask shaping.
- [ ] **Motion/life** — Something scrolls, pulses, oscillates, or evolves. No static effects.
- [ ] **Layer count** — At least 3 distinct visual layers (core + glow + accents minimum)
- [ ] **Musical identity** — Music notes, harmonic pulses, or rhythmic timing woven in where appropriate
- [ ] **Uniqueness** — No other weapon in the same theme uses the same VFX approach
- [ ] **Correct blend modes** — Glow uses additive. Smoke uses alpha blend. No dark edges on glow effects.
- [ ] **Impact feedback** — Hits feel multi-layered and memorable, not just "small explosion → done"
- [ ] **ModifyTooltips** — Tooltips describe the weapon's special mechanics + themed lore line with correct color
- [ ] **Asset completeness** — No placeholder textures. Every visual asset exists.
