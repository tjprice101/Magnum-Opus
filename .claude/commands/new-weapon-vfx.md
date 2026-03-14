Design and implement a complete VFX suite for a new weapon in MagnumOpus.

If the user specified a weapon: $ARGUMENTS

---

## Step 1: Creative Direction Dialog

**MANDATORY.** Before any technical work, run a full interactive concept dialog.

### Round 1: Broad Context (3-4 questions)
- What theme/score is this for? What's the weapon class?
- What emotional feeling should this weapon evoke? (Not just "powerful" — dig deeper: triumphant? melancholy? frenzied? serene? wrathful?)
- Is this a flagship weapon (complex VFX budget) or supporting weapon?
- What inspired this weapon idea? A musical passage? A visual image? A gameplay mechanic?

### Round 2: Identity Drill-Down (3-4 questions based on Round 1)
Explore specifics. Examples:
- "You said 'melancholy elegance' — should the sadness come through gentle fading trails, or sharp beautiful strikes that leave lingering marks?"
- "For a flagship melee weapon — noble knight's blade (deliberate, weighty) or berserker's fury (rapid, overwhelming)?"
- "What should the weapon's SIGNATURE MOMENT be? Charged finisher? Transformation? Screen-filling ultimate?"
- "How should it feel when NOT attacking? Glowing idle? Subtle particles? Dormant?"

### Round 3: Creative Options (present 2-3 proposals)
Each proposal includes: attack pattern concept, VFX vision, signature moment, musical integration.

### Round 4: Technical Detail (3-4 questions based on selection)
- Combo style: distinct animation phases or fluid continuous?
- Signature burst: screen shake? flash? chromatic aberration? or contained/elegant?
- Trail rendering: sharp/crystalline (SDF) or organic/flowing (noise)?
- Projectile style for special: homing? expanding ring? converging star? linear devastation?

### Round 5: Final Proposal (confirmation)
Complete weapon concept: full attack breakdown, VFX layer list, asset requirements, musical integration, signature moment, uniqueness comparison.

If this is NOT a complete new weapon (just adding trails or improving existing VFX), skip to Step 2 but still ask 3-4 creative direction questions.

## Step 2: Identify the Weapon

Gather: weapon class, theme, weapon name, musical soul.

## Step 3: Check Uniqueness Within Theme

Read `Content/<ThemeName>/` and catalog every existing weapon of the same class:
- Trail type (ribbon, afterimage, primitive, none)
- Particle effects (bloom, sparks, notes, themed)
- Special mechanics (charge, orbit, chain, burst)
- Shader effects (distortion, color ramp, UV scroll)

**The new weapon MUST be meaningfully different in at least 3 of 5 categories.**

## Step 4: Audit Available Assets

Search systematically:
- `Assets/VFX Asset Library/` (all subdirectories — beams, gradients, bloom, impacts, noise, masks, trails, slashes, projectiles)
- `Assets/Particles Asset Library/` (107+ sprites)
- `Assets/SandboxLastPrism/` (flare, gradients, orbs, trails)
- Theme-specific asset folders
- `Effects/` (existing shaders)
- `Content/FoundationWeapons/` (reference implementations)

**If ANY needed asset doesn't exist** — STOP. Provide Midjourney prompt + expected file location.

## Step 5: Creative Asset Reuse Check

Before requesting new assets:
- Can an existing bloom sprite work at different scale/tint/rotation?
- Can an existing noise texture + color ramp create this via shader?
- Can particle choreography (spawn pattern, velocity, lifetime) make existing sprites look new?

Only request new assets when creative reuse genuinely cannot achieve the goal.

## Step 6: Design Effect Layers

Plan VFX as distinct layers. Standard weapon: 3+ layers. Flagship: 5+.

**Melee:**
| Layer | Purpose | Technique |
|-------|---------|-----------|
| Swing trail | Primary identity | Primitive trail with shader, smear overlay |
| Impact | Hit feedback | Particle burst + bloom + screen shake |
| Ambient/hold | Passive presence | Gentle glow, orbiting particles, aura |
| Special mechanic | Unique moment | Charged finisher, combo escalation, projectile release |
| Accents | Polish | Music notes, sparkle edges, theme particles |

**Magic:**
| Layer | Purpose | Technique |
|-------|---------|-----------|
| Projectile | Primary identity | Layered bloom + trail + themed particles |
| Cast/channel | Use feedback | Cast circle, hand glow, channeling particles |
| Impact | Hit feedback | Multi-layered burst + screen flash |
| Ambient/hold | Passive presence | Orb glow, magic circle, floating particles |
| Accents | Polish | Color ramp shifts, music notes, prismatic sparkle |

**Ranged:**
| Layer | Purpose | Technique |
|-------|---------|-----------|
| Projectile trail | Primary identity | Comet trail, ribbon, energy streak |
| Muzzle flash | Fire feedback | Burst + light spike at barrel |
| Impact | Hit feedback | Radial burst + debris particles |
| Barrel glow | Passive presence | Subtle glow at weapon tip |
| Accents | Polish | Shell casings, smoke wisps, theme particles |

**Summoner:**
| Layer | Purpose | Technique |
|-------|---------|-----------|
| Summon circle | Ritual | Radial shader, ascending particles |
| Minion rendering | Minion identity | Glow + trail + themed particles |
| Minion attacks | Attack variety | Beams, projectiles, melee (varied VFX each) |
| Staff hold | Passive presence | Subtle glow, connection line to minion |
| Accents | Polish | Music notes on summon, harmonic pulse on attacks |

## Step 7: Check Foundation Weapons

Browse `Content/FoundationWeapons/` for starting points:

| Foundation | Use For |
|-----------|---------|
| AttackFoundation | Base projectile patterns, 5-mode cycling |
| AttackAnimationFoundation | Cinematic timed sequences |
| SwordSmearFoundation | 3-layer melee smear VFX |
| ThinSlashFoundation | Precision slash arcs (SDF) |
| XSlashFoundation | Cross-slash patterns |
| LaserFoundation | Full-width laser beams |
| ThinLaserFoundation | Thin precision lasers |
| InfernalBeamFoundation | Fire/energy beams |
| MagicOrbFoundation | Glowing sphere projectiles |
| SparkleProjectileFoundation | Bloom-rendered projectiles |
| RibbonFoundation | 10 flowing ribbon trail modes |
| ImpactFoundation | Multi-layered hits (3 shader types) |
| ExplosionParticlesFoundation | CPU physics radial burst |
| SmokeFoundation | Soft billowing smoke |
| MaskFoundation | Alpha mask shaping |
| Foundation4PointSparkle | 4-pointed sparkles |
| FoundationIncisorOrbs | Orbiting sub-projectiles |

## Step 8: Research Reference Repos

For the specific techniques planned:

| Technique | Search In |
|-----------|-----------|
| Primitive trails | Calamity: `ExobladeProj.cs`, `PrimitiveRenderer.cs` |
| Slash shaders | Calamity: `ExobladeSlashShader.fx` |
| Screen distortion | WotG: `Core/Graphics/ArbitraryScreenDistortion/` |
| Hierarchical bloom | Everglow: `BloomPipeline.cs` |
| GPU particles | WotG: `Core/Graphics/FastParticleSystems/` |
| Metaballs | Calamity: `Graphics/Metaballs/`, MagnumOpus: `Common/Systems/Metaballs/` |

Read FULL implementations — C# rendering + .fx shader + how they wire together.

## Step 9: Request Missing Assets

For every texture/sprite that doesn't exist:
1. Describe what's needed and where
2. **Midjourney prompt** with art style, subject, color palette (white/grayscale on black for VFX), dimensions, technical requirements
3. Expected file location: VFX textures → `Assets/<Theme>/<Weapon>/<Type>/`, shaders → `Effects/<Theme>/<Weapon>/`, dust sprites → `Content/<Theme>/<Category>/<Weapon>/Dusts/Textures/`

## Step 10: Implement

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

## Step 11: Quality Checklist

Before considering the weapon complete:

- [ ] **Theme colors** — All effect colors match theme palette
- [ ] **Edge quality** — No hard texture cutoffs (smoothstep, feathered edges, mask shaping)
- [ ] **Motion/life** — Something scrolls, pulses, oscillates. No static effects.
- [ ] **Layer count** — At least 3 distinct visual layers (core + glow + accents)
- [ ] **Musical identity** — Music notes, harmonic pulses, or rhythmic timing where appropriate
- [ ] **Uniqueness** — No other weapon in the same theme uses the same VFX approach
- [ ] **Blend modes** — Glow uses Additive, smoke uses AlphaBlend, no dark edges on glow
- [ ] **Impact feedback** — Hits are multi-layered and memorable
- [ ] **ModifyTooltips** — Effect descriptions + themed lore line with correct OverrideColor
- [ ] **Asset completeness** — No placeholder textures
- [ ] **Builds** — `dotnet build` passes
