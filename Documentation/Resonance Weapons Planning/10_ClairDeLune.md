# 10. Clair de Lune — Resonant Weapons Technical Documentation

> *"Moonlit reverie. Clockwork precision. Time itself bends to the melody."*

## Theme Identity

### Musical Soul
Clair de Lune is Debussy's gentle moonlit dreamscape — but here it's fused with clockwork precision and temporal mechanics. Every weapon operates through the metaphor of time: gears turning, clocks ticking, pendulums swinging, orreries spinning. The visual language is mechanical moonlight — soft blue radiance filtered through brass gears and crystal clockwork. Where Nachtmusik gazes outward at the cosmos, Clair de Lune gazes inward at the mechanisms of time itself.

### Color Palette (Clockwork Moonlight Scale)
```
Temporal Void (15,20,45) → Night Mist Blue (40,60,110) → Soft Moonblue (100,140,200)
→ Pearl Frost (180,200,230) → Clockwork Gold (200,170,80) → Pearl White (235,240,250)
```

### Lore Color
`new Color(150, 200, 255)` — Ice Blue

### Visual Language
- **Clockwork elements**: Gears, cogs, pendulums, clock hands, tick marks — mechanical precision
- **Temporal distortion**: Time Slow Fields, temporal echoes, frozen moments — time manipulation VFX
- **Moonlit radiance**: Soft, diffuse blue-white glow with gold mechanical accents
- **Noise identity**: VoronoiCell for gear/crystal patterns, PerlinFlow for temporal energy, TileableFBMNoise for moonlit mist, MarbleSwirl for clock face patterns

---

## Foundation Weapons Integration Map

```
Foundation Coverage Across All 12 Weapons:

SwordSmearFoundation ──────► [1] Chronologicality (temporal smear arcs, 3-phase combo)
ThinSlashFoundation ───────► [2] Temporal Piercer (precise rapier piercing trails)
XSlashFoundation ──────────► (none — precision over chaos in this theme)
LaserFoundation ───────────► [7] Clockwork Grimoire (Hour Mode sustained beam)
                              [10] Lunar Phylactery (moonlight beam sentinel)
ThinLaserFoundation ───────► [10] Lunar Phylactery (secondary thin beam crossing)
InfernalBeamFoundation ────► (none — no infernal energy in clockwork theme)
MagicOrbFoundation ────────► [4] Starfall Whisper (crystal bolt projectiles)
                              [6] Cog and Hammer (bomb orb bodies)
                              [7] Clockwork Grimoire (Minute/Second/Pendulum modes)
                              [8] Orrery of Dreams (Dream Sphere bodies)
                              [10] Lunar Phylactery (sentinel body)
SparkleProjectileFoundation► [4] Starfall Whisper (Temporal Fracture replays)
                              [8] Orrery of Dreams (Dream Alignment chain launch)
MaskFoundation ────────────► [1] Chronologicality (Time Slow Field zones)
                              [3] Clockwork Harmony (gear mesh zone)
                              [5] Midnight Mechanism (tick mark→Midnight Strike indicator)
                              [7] Clockwork Grimoire (Pendulum Mode temporal zone)
                              [9] Requiem of Time (Forward/Reverse temporal fields)
                              [11] Gear-Driven Arbiter (Temporal Judgment indicator)
                              [12] Automaton's Tuning Fork (resonance zone)
RibbonFoundation ──────────► [3] Clockwork Harmony (gear mesh connector arcs)
                              [5] Midnight Mechanism (bullet stream trails)
                              [8] Orrery of Dreams (orbital path trails)
ImpactFoundation ──────────► [1] Chronologicality (temporal echo detonations)
                              [2] Temporal Piercer (Frozen Moment impact)
                              [3] Clockwork Harmony (Gear Recall mesh explosion)
                              [4] Starfall Whisper (Shattered Time fracture bursts)
                              [6] Cog and Hammer (clockwork bomb detonation)
                              [9] Requiem of Time (Temporal Paradox overlap)
                              [11] Gear-Driven Arbiter (Arbiter's Verdict detonation)
                              [12] Automaton's Tuning Fork (Perfect Resonance burst)
ExplosionParticlesFoundation► [5] Midnight Mechanism (Mechanism Eject shrapnel)
                              [6] Cog and Hammer (gear shrapnel scatter)
                              [11] Gear-Driven Arbiter (Clockwork Court sync burst)
SmokeFoundation ───────────► [9] Requiem of Time (temporal mist in field zones)
AttackFoundation ──────────► [3] Clockwork Harmony (3 gear types launched)
                              [5] Midnight Mechanism (rapid-fire projectiles)
                              [6] Cog and Hammer (bomb lobbing + shrapnel)
                              [7] Clockwork Grimoire (mode-specific projectiles)
                              [8] Orrery of Dreams (Dream Alignment chain launch)
                              [11] Gear-Driven Arbiter (gear projectile fire)
                              [12] Automaton's Tuning Fork (resonance wave projectiles)
AttackAnimationFoundation ─► [1] Chronologicality (Clockwork Overflow screen effect)
                              [9] Requiem of Time (Temporal Paradox screen distortion)
```

---

## Weapons Overview

| # | Weapon | Class | Primary Foundation | Signature VFX |
|---|--------|-------|-------------------|---------------|
| 1 | Chronologicality | Melee (Broadsword) | SwordSmearFoundation | 3-phase clock-hand combo, temporal echoes, Time Slow Fields |
| 2 | Temporal Piercer | Melee (Rapier) | ThinSlashFoundation | Precise puncture marks, Frozen Moment stun, time-pierce boomerang |
| 3 | Clockwork Harmony | Ranged (Launcher) | AttackFoundation | 3 gear sizes meshing for chain reactions, Gear Recall |
| 4 | Starfall Whisper | Ranged (Bow) | SparkleProjectileFoundation | Temporal Fracture replays, Shattered Time alt fire |
| 5 | Midnight Mechanism | Ranged (Gun) | AttackFoundation | 5-phase gatling spin-up, tick marks→Midnight Strike |
| 6 | Cog and Hammer | Ranged (Launcher) | MagicOrbFoundation | Clockwork bomb bodies, gear shrapnel, Chain Detonation |
| 7 | Clockwork Grimoire | Magic | LaserFoundation | 4-mode cycle (Hour beam/Minute orbs/Second bolts/Pendulum zone) |
| 8 | Orrery of Dreams | Magic | MagicOrbFoundation | 3 orbiting Dream Spheres, Dream Alignment chain launch |
| 9 | Requiem of Time | Magic | MaskFoundation | Forward/Reverse temporal fields, Temporal Paradox overlap |
| 10 | Lunar Phylactery | Summon | LaserFoundation | Moonlight beam sentinel, Soul-Link HP scaling |
| 11 | Gear-Driven Arbiter | Summon | AttackFoundation | Clockwork gear projectiles, Temporal Judgment, Arbiter's Verdict |
| 12 | Automaton's Tuning Fork | Summon | ImpactFoundation | Resonance waves, 4 frequencies (A/C/E/G), Perfect Resonance overlap |

---

## 1. Chronologicality (Melee — Broadsword)

### Identity & Musical Soul
A broadsword forged from crystallized time — its three-phase combo mimics the three hands of a clock. Hour Hand (heavy, slow cleave), Minute Hand (mid sweep), Second Hand (rapid flurry). Each phase leaves temporal echoes that replay their damage. Time Slow Fields linger at impact points, and Clockwork Overflow triggers after a perfect 3-phase cycle — freezing time briefly in a massive radius. Every swing is a tick. Every combo is a minute passing.

### Lore Line
*"Every swing is a second spent. Every combo is a minute passing. And when the hour strikes — time itself holds its breath."*

### Combat Mechanics
- **3-Phase Clock Combo**:
  - **Hour Hand** (Phase 1): Slow, wide 270° cleave. 2x base damage. Leaves temporal echo for 1s replay.
  - **Minute Hand** (Phase 2): Mid-speed 180° sweep. 1.5x base damage. Temporal echo.
  - **Second Hand** (Phase 3): Rapid 3-strike flurry (90° each). 0.8x per strike. Temporal echoes.
- **Temporal Echoes**: Translucent replays of each swing that deal 30% original damage after a 0.5s delay.
- **Time Slow Field**: Hit enemies leave lingering Time Slow zones (3s, 3-tile radius) slowing enemies 40%.
- **Clockwork Overflow**: Perfect 3-phase combo → 8-tile radius temporal detonation, 4x damage, brief screen time-freeze effect.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CHRONOLOGICALITY                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: AttackAnimationFoundation                     │
│           → Clockwork Overflow screen time-freeze effect │
│           → Clock face overlay → detonation → resume     │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: temporal echo detonation rings │
│           → DamageZoneShader: Clockwork Overflow zone    │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Time Slow Field zones │
│           → VoronoiCell noise for crystallized time look │
│           → MarbleSwirl for clock-face pattern in zone   │
│  Layer 2: SwordSmearFoundation (PRIMARY)                │
│           → SmearDistortShader: temporal smear arcs      │
│           → Hour: wide 270° arc, heavy distortion        │
│           → Minute: mid 180° arc, moderate distortion    │
│           → Second: rapid 3x 90° arcs, light distortion │
│           → PerlinFlow noise for time-energy distortion  │
│  Layer 1: SparkleProjectileFoundation                   │
│           → Temporal echo rendering (30% opacity replay) │
│           → Ghost trail positions from original swing     │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Clock-Hand Smears: SwordSmearFoundation (PRIMARY — SmearDistortShader)
| Phase | Arc | distortStrength | flowSpeed | noiseScale | Width | Unique Feature |
|-------|-----|----------------|-----------|-----------|-------|----------------|
| Hour Hand | 270° | 0.07 | 0.25 | 2.0 | 2.0x (massive) | Heavy temporal weight, screen shake 4 |
| Minute Hand | 180° | 0.05 | 0.4 | 2.5 | 1.2x | Mid-speed sweep, balanced |
| Second Hand | 90° x3 | 0.03 | 0.6 | 3.0 | 0.8x | Rapid triple flurry, sharp |

Common parameters:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `noiseTex` | PerlinFlow | Temporal energy distortion |
| `gradientTex` | ClairDeLune_Temporal_LUT | Night Mist Blue → Soft Moonblue → Pearl Frost → Clockwork Gold highlights |
| Blend | Additive | Luminous temporal arcs |
| Layers | 3 (body + glow + gold clock-tick accent) | Depth with clockwork detail |
| Clock-tick accent | Clockwork Gold spark at arc tip per 30° interval | Tick marks along arc |

#### Temporal Echoes: SparkleProjectileFoundation (adapted)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Opacity | 30% base → 0% over 0.5s | Ghost replay fading |
| Color | Pearl Frost tint with Soft Moonblue edge | Temporal ghost coloring |
| Trail positions | Recorded from original swing, replayed 0.5s later | Exact swing replay |
| Ring buffer | 24 positions (captures full swing arc) | Complete arc fidelity |
| Bloom layers | 2 (reduced from standard 5 for ghost subtlety) | Understated but visible |

#### Time Slow Fields: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell (primary) + MarbleSwirl (secondary) | Crystallized time + clock-face pattern |
| `scrollSpeed` | 0.08 (very slow — time is slowed) | Nearly frozen appearance |
| `circleRadius` | 0.38 (3-tile radius) | Time Slow zone coverage |
| `edgeSoftness` | 0.10 | Soft temporal boundary |
| `intensity` | 1.4 | Subtle but visible zone |
| Gradient LUT | ClairDeLune_TimeSlow_LUT | Temporal Void → Night Mist Blue → Soft Moonblue (cool, frozen) |
| Duration | 180 frames (3s) with 1s fade-out | Lingering temporal zone |

#### Clockwork Overflow: AttackAnimationFoundation + ImpactFoundation
| Phase | Duration | Visual |
|-------|----------|--------|
| Trigger (Phase 1) | 10 frames | Clock face overlay appears centered on player |
| Freeze (Phase 2) | 20 frames | Screen tints Pearl Frost, all motion appears frozen |
| Detonation (Phase 3) | 15 frames | ImpactFoundation: 10 rings Night Mist → Soft Moonblue → Pearl Frost → Clockwork Gold, 8-tile radius |
| Resume (Phase 4) | 15 frames | Clock face shatters into gear fragments, normal color resumes |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| HourHandSwing | SwordSmearFoundation | 270° slow heavy cleave | Wide temporal smear, PerlinFlow distortion, Clockwork Gold ticks |
| MinuteHandSwing | SwordSmearFoundation | 180° mid sweep | Moderate temporal smear |
| SecondHandFlurry (x3) | SwordSmearFoundation | 90° rapid strike x3 | Thin fast arcs, sharp |
| TemporalEcho | SparkleProjectileFoundation (ghost) | 30% damage replay after 0.5s delay | 30% opacity swing replay with 2-layer ghost bloom |
| TimeSlowField | MaskFoundation (persistent) | 3s AoE slow zone at impact | VoronoiCell + MarbleSwirl frozen zone |
| ClockworkOverflow | AttackAnimationFoundation + ImpactFoundation | 8-tile temporal detonation after perfect combo | Screen freeze → clock face → 10-ring detonation → gear shatter |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Temporal_LUT | `Assets/ClairDeLune/Chronologicality/Gradients/` | "Horizontal color gradient strip, night mist blue left through soft moonblue through pearl frost to warm clockwork gold accents right, temporal clock energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ClockFaceOverlay | `Assets/ClairDeLune/Chronologicality/Flare/` | "Ornate clock face with Roman numerals and three clock hands (hour minute second) in pearl white and clockwork gold, detailed mechanical clockwork border, on solid black background, game VFX overlay texture, 256x256px --ar 1:1 --style raw" |
| TemporalEchoGhost | `Assets/ClairDeLune/Chronologicality/Trails/` | "Fading translucent sword swing arc with ghostly afterimage effect and faint clock-tick marks along the arc, pearl frost blue with moonblue tint, on solid black background, game VFX trail texture, 256x128px --ar 2:1 --style raw" |

---

## 2. Temporal Piercer (Melee — Rapier)

### Identity & Musical Soul
A rapier that punctures through time. Ultra-precise thrusts leave Temporal Puncture marks on enemies (max 5). Each mark ticks like a clock, and at 5 marks the Frozen Moment triggers — a brief stun + massive damage burst. The time-pierce boomerang alt fire travels through marked enemies dealing bonus damage per mark. It's a fencer's weapon — precision, patience, and the devastating riposte.

### Lore Line
*"Five marks upon the hours. And when the fifth chimes — the moment freezes."*

### Combat Mechanics
- **Temporal Thrust**: Precise rapier thrust with extremely fast windup. Inflicts Temporal Puncture (max 5 stacks, 8s each).
- **Temporal Puncture**: Each mark shows a clock-face indicator (⊙) on enemy. Visual clock hand advances per stack. At 5 stacks → Frozen Moment.
- **Frozen Moment**: 5-stack trigger: 1.5s stun + burst damage (sum of all accumulated puncture damage × 2). All marks consumed. Clock shatters VFX.
- **Time-Pierce Boomerang** (alt fire): Rapier launches as boomerang through marked enemies. +30% damage per mark on each enemy hit. Returns to player.
- **Piercing Through Time**: Thrusts have 15% chance to pierce through enemy and hit the enemy behind them.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              TEMPORAL PIERCER                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Frozen Moment shatter rings    │
│           → SlashMarkShader: puncture mark VFX           │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: clock-face indicators │
│           → MarbleSwirl noise for clock-face pattern     │
│           → Tracks 1-5 stack progression                 │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): time-pierce boomerang       │
│           → Returns to player after pierce-through       │
│  Layer 1: ThinSlashFoundation (PRIMARY)                 │
│           → ThinSlashShader: ultra-precise thrust trails │
│           → lineWidth 0.012 (thinnest possible)          │
│           → Pearl Frost → Soft Moonblue → Clockwork Gold │
│           → Style 1 (straight): precision rapier line    │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Rapier Thrust: ThinSlashFoundation (PRIMARY — ThinSlashShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `lineWidth` | 0.012 | Ultra-thin precision (thinnest in mod) |
| `lineLength` | 0.55 | Extended reach (rapier lunge) |
| Colors | Edge: Night Mist Blue, Mid: Soft Moonblue, Core: Pearl Frost | Temporal precision coloring |
| Style | 1 (straight line) | Clean rapier thrust |
| Duration | 25 frames (fast) | Quick thrust animation |
| Clockwork Gold tip | Gold sparkle at thrust tip point | Puncture mark creation indicator |

#### Temporal Puncture Marks: MaskFoundation (RadialNoiseMaskShader) — per-enemy indicator
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | MarbleSwirl | Clock-face veined pattern |
| `circleRadius` | 0.15 (small, per-enemy) | Clock indicator above enemy |
| `scrollSpeed` | 0.0 (static) | Frozen clock face — not moving |
| `intensity` | 1.0 + 0.3 per stack (1.0 → 2.5 at 5 stacks) | Brightens with stacks |
| Clock hand overlay | Rotates: 1 stack = 12 o'clock, 5 stacks = full rotation | Visual stack progression |
| Gradient LUT | ClairDeLune_Puncture_LUT | Night Mist Blue → Clockwork Gold (urgency at high stacks) |

#### Frozen Moment: ImpactFoundation
| Shader | Parameters | Purpose |
|--------|-----------|---------|
| RippleShader | 6 rings, Pearl Frost → Clockwork Gold → Pearl White, 0.6s | Clock shatter expanding rings |
| SlashMarkShader | 5 radial slash marks (one per consumed puncture) | Puncture marks shattering outward |
| Screen freeze | 10-frame brief screen tint (Pearl Frost 30% opacity) | Temporal freeze moment |

#### Time-Pierce Boomerang: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (Direct) | Straight-line boomerang (returns) |
| Speed | 14 pixels/frame out, 16 pixels/frame return | Fast pierce-through |
| Trail | SparkleProjectileFoundation: 12-position ring buffer, Pearl Frost sparkle | Clean precision trail |
| Damage bonus | +30% per Temporal Puncture mark on each enemy hit | Rewards mark stacking |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| TemporalThrust | ThinSlashFoundation | Ultra-fast precise thrust | 0.012 width, Pearl Frost→Moonblue→Gold, straight Style 1 |
| TemporalPunctureIndicator | MaskFoundation (per-enemy) | Clock-face stack indicator (1-5) | MarbleSwirl clock face, advancing hand, brightening |
| FrozenMomentBurst | ImpactFoundation | 5-stack trigger, stun + damage | 6 rings + 5 radial slash marks + screen freeze |
| TimePierceBoomerang | AttackFoundation + SparkleProjectileFoundation | Alt fire boomerang through marked enemies | Sparkle trail boomerang, +30% per mark |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Puncture_LUT | `Assets/ClairDeLune/TemporalPiercer/Gradients/` | "Horizontal color gradient strip, cool night mist blue left transitioning through soft moonblue center to warm clockwork gold right, precision clock energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ClockFaceIndicatorSmall | `Assets/ClairDeLune/TemporalPiercer/Orbs/` | "Small ornate clock face with single rotating hand, Roman numerals XII III VI IX visible, pearl white and clockwork gold on solid black background, game VFX indicator sprite, 32x32px --ar 1:1 --style raw" |

---

## 3. Clockwork Harmony (Ranged — Launcher)

### Identity & Musical Soul
A launcher that fires clockwork gears of three sizes — small (fast), medium (bouncing), and drive gears (heavy). When gears of different sizes collide mid-air, they mesh together, creating chain reactions: meshed gears spin rapidly dealing sustained AoE damage. Gear Recall pulls all gears back creating a vortex. The weapon rewards skill-shot placement — positioning gears to mesh creates devastating clockwork machinery in mid-air.

### Lore Line
*"Harmony isn't found. It's engineered."*

### Combat Mechanics
- **Small Gear**: Fast, direct fire. Bounces once. Meshes with Medium or Drive on contact.
- **Medium Gear** (alt fire): Arc-lobbed, bounces 3 times. Meshes with Small or Drive.
- **Drive Gear** (hold + release): Slow, heavy. 2x damage. Meshes with either type. Anchors mesh point.
- **Gear Mesh**: When 2+ different-size gears collide, they mesh — spinning AoE for 3s dealing sustained damage. 3 gears = Harmony Mesh (1.5x AoE, +50% damage).
- **Gear Recall** (special): All deployed gears fly back to player, damaging enemies in path. Meshed gears detonate on recall.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CLOCKWORK HARMONY                           │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Gear Mesh activation flash     │
│           → RippleShader: Gear Recall detonation         │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Gear Mesh AoE zone    │
│           → VoronoiCell noise for gear-teeth pattern     │
│  Layer 2: RibbonFoundation                              │
│           → Mode 1 PureBloom: mesh connector arcs        │
│           → Visible interlocking gear teeth lines         │
│  Layer 1: AttackFoundation (PRIMARY)                    │
│           → Mode 1 (Direct): Small Gear fire             │
│           → Mode 3 (Spread): Medium Gear arc-lob         │
│           → Mode 5 (Burst): Drive Gear heavy launch      │
│           → 3 distinct gear projectile types              │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Gear Projectiles: AttackFoundation (PRIMARY)
| Gear Type | Mode | Speed | Size | Bloom Layers | Color |
|-----------|------|-------|------|-------------|-------|
| Small Gear | 1 (Direct) | 16 px/f | 20px radius | 2 (Soft Moonblue core + Pearl Frost glow) | Clockwork Gold gear tint |
| Medium Gear | 3 (Spread) | 10 px/f, arc | 32px radius | 3 | Clockwork Gold → Pearl Frost |
| Drive Gear | 5 (Burst, hold) | 6 px/f | 48px radius | 4 (largest presence) | Clockwork Gold core + Soft Moonblue corona |

All gears share:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Rotation | Constant spin (Small: 15°/f, Medium: 10°/f, Drive: 5°/f) | Clockwork animation |
| Gear-teeth rendering | 8/12/16 teeth per size (drawn as edge bumps) | Mechanical gear look |
| Trail | RibbonFoundation Mode 1: thin PureBloom trail behind gear | Motion path visibility |

#### Gear Mesh Zone: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Gear-teeth / mechanical crystal pattern |
| `scrollSpeed` | 0.4 (fast, spinning) | Active meshing animation |
| `circleRadius` | 0.25 (2-gear mesh) → 0.35 (3-gear Harmony Mesh) | Zone scales with gear count |
| `intensity` | 2.0 (2-gear) → 2.8 (Harmony Mesh) | Brighter with more gears |
| `rotationSpeed` | Matched to gear spin speed | Rotating mesh zone |
| Gradient LUT | ClairDeLune_Clockwork_LUT | Clockwork Gold → Soft Moonblue → Pearl Frost |

#### Mesh Connectors: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean mechanical connection |
| Width | 3px | Gear-teeth interlock lines |
| Color | Clockwork Gold | Mechanical precision |
| Animation | Pulsing at gear rotation frequency | Synchronized with gear spin |

#### Gear Recall + Detonation: ImpactFoundation
| Phase | Visual |
|-------|--------|
| Recall (all gears fly back) | Trail lines from each gear converging on player |
| Mesh detonation (meshed gears explode) | RippleShader: 6 rings, Clockwork Gold → Pearl Frost, gear fragment particles |
| Single gear arrival | Small flash at player position per gear |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| SmallGear | AttackFoundation (Mode 1) | Fast direct, 1 bounce | 20px spinning gear, Clockwork Gold, 2-layer bloom |
| MediumGear | AttackFoundation (Mode 3) | Arc-lob, 3 bounces | 32px spinning gear, 3-layer bloom |
| DriveGear | AttackFoundation (Mode 5) | Slow heavy hold-release | 48px spinning gear, 4-layer bloom, heavy |
| GearMeshZone | MaskFoundation (persistent) | 3s AoE at collision point | VoronoiCell spinning zone, scales with gear count |
| MeshConnectorArc | RibbonFoundation | Visual link between meshed gears | PureBloom Clockwork Gold teeth lines |
| GearRecallDetonation | ImpactFoundation | Meshed gears explode on recall | 6-ring ripple + gear fragment particles |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Clockwork_LUT | `Assets/ClairDeLune/ClockworkHarmony/Gradients/` | "Horizontal color gradient strip, warm clockwork gold left through soft moonblue center to pearl frost white right, mechanical clockwork energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| GearSpritesSheet | `Assets/ClairDeLune/ClockworkHarmony/Pixel/` | "Three clockwork gear sprites of different sizes: small 8-tooth, medium 12-tooth, large 16-tooth cog, warm clockwork gold with pearl frost highlights, mechanical precision design, on solid black background, game pixel art sprite sheet, 128x48px --ar 8:3 --style raw" |

---

## 4. Starfall Whisper (Ranged — Bow)

### Identity & Musical Soul
A bow that fires crystal arrows through time. Hits create Temporal Fracture points that replay the impact 0.5s later. The alt fire triggers Shattered Time — five fractures firing simultaneously in a spread. Temporal Refraction bends arrows through Time Slow Fields from Chronologicality. It whispers because its arrows are barely visible until they arrive — then time shatters.

### Lore Line
*"You hear the whisper only after the arrow has already arrived."*

### Combat Mechanics
- **Temporal Arrow**: Crystal arrow, fast, creates Temporal Fracture at impact point (0.5s delayed replay dealing 40% damage).
- **Temporal Fracture**: Persistent 1s fracture point — a crack in time that replays the hit.
- **Shattered Time** (alt fire): Fires 5 fracture arrows simultaneously in spread pattern. Each creates its own fracture. 3s cooldown.
- **Temporal Refraction**: Arrows passing through Time Slow Fields (from Chronologicality) refract — splitting into 2 and changing angle by 30°.
- **Fracture Resonance**: 3+ fractures within 5 tiles of each other → chain reaction, 50% bonus damage burst.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              STARFALL WHISPER                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: fracture creation rings         │
│           → RippleShader: Fracture Resonance chain burst │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: fracture point VFX    │
│           → VoronoiCell noise for cracked-time fracture  │
│  Layer 2: SparkleProjectileFoundation                   │
│           → CrystalShimmerShader: fracture replay arrows │
│           → 5-layer crystal arrow rendering               │
│  Layer 1: SparkleProjectileFoundation (PRIMARY)         │
│           → SparkleTrailShader: crystal arrow flight     │
│           → Ring buffer trail with temporal shimmer      │
│           → Pearl Frost → Soft Moonblue palette           │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Crystal Arrow: SparkleProjectileFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| SparkleTrailShader | 24-position ring buffer | Arrow flight trail |
| 5-layer rendering | Core: Pearl White, Inner: Pearl Frost, Mid: Soft Moonblue, Outer: Night Mist Blue, Ambient: Temporal Void | Full crystal arrow depth |
| CrystalShimmerShader | Temporal shimmer (clock-tick flash every 8 frames) | Time-themed shimmer |
| Speed | 18 pixels/frame (barely visible — per identity) | Extremely fast arrow |
| Size | Small (16px head) | Whisper-like subtlety |

#### Temporal Fracture Points: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Cracked-time fracture pattern |
| `scrollSpeed` | 0.0 (frozen) → 0.8 (replay burst) | Static until replay triggers |
| `circleRadius` | 0.2 | Small fracture point |
| `intensity` | 1.2 (idle) → 3.0 (replay burst) | Flashes bright during replay |
| Duration | 60 frames (1s) | Persistent fracture |
| Gradient LUT | ClairDeLune_Fracture_LUT | Temporal Void → Night Mist Blue → Pearl Frost (cold crack) |

#### Shattered Time (5-fracture spread): SparkleProjectileFoundation x5
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Count | 5 simultaneous arrows | Full Shattered Time pattern |
| Spread | 15° between each (total 60° cone) | Wide coverage |
| Each creates fracture | 5 fracture points appear | Massive fracture field |

#### Fracture Resonance: ImpactFoundation
| Trigger | 3+ fractures within 5 tiles | Chain reaction |
| RippleShader | 4 rings per fracture, Night Mist Blue → Clockwork Gold, 0.4s | Fracture detonation |
| Chain visual | Energy arcs between resonating fractures before burst | Connection visibility |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| TemporalArrow | SparkleProjectileFoundation | Fast crystal arrow, creates fracture on impact | 5-layer crystal rendering, temporal shimmer |
| TemporalFracturePoint | MaskFoundation (persistent) | 1s cracke at impact point, replays hit at 0.5s | VoronoiCell frozen crack, bright flash on replay |
| ShatteredTimeArrow (x5) | SparkleProjectileFoundation | Spread-fire alt, each creates own fracture | Same crystal arrows in 60° cone |
| FractureResonanceBurst | ImpactFoundation | 3+ nearby fractures chain-react | Per-fracture 4-ring ripple + connecting arcs |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Fracture_LUT | `Assets/ClairDeLune/StarfallWhisper/Gradients/` | "Horizontal color gradient strip, deep temporal void left through night mist blue center to pearl frost right, cracked frozen time energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| TemporalFractureCrack | `Assets/ClairDeLune/StarfallWhisper/Flare/` | "Shattered time fracture crack in space with VoronoiCell crystalline pattern and faint clock markings visible through the cracks, pearl frost and soft moonblue with night mist blue edges, on solid black background, game VFX texture, 64x64px --ar 1:1 --style raw" |

---

## 5. Midnight Mechanism (Ranged — Gun)

### Identity & Musical Soul
A gatling gun that embodies the relentless ticking of a clock approaching midnight. Five-phase spin-up escalates from slow deliberate shots to a screaming 24-round-per-second barrage. Tick marks accumulate visually around the weapon's barrel. At 12 tick marks, Midnight Strike triggers — a single devastating shot with screen-wide clock-chime effect. Gear Jam and Mechanism Eject add identity to downtime. It's a countdown to destruction.

### Lore Line
*"The clock does not care if you are ready. Midnight comes regardless."*

### Combat Mechanics
- **5-Phase Spin-Up**:
  - Phase 1 (0-2s): 3 shots/s. Precise, slow.
  - Phase 2 (2-4s): 6 shots/s. Building.
  - Phase 3 (4-6s): 12 shots/s. Aggressive.
  - Phase 4 (6-8s): 18 shots/s. Relentless.
  - Phase 5 (8s+): 24 shots/s. Maximum barrage.
- **Tick Mark Accumulation**: Every 50 hits = 1 tick mark (visual, around barrel). 12 tick marks = Midnight.
- **Midnight Strike** (12 tick marks): Massive single shot — 10x damage, screen-wide clock chime + brief time-stop visual. Consumes all marks.
- **Gear Jam**: Stopping fire during Phase 3+ → weapon jams for 1s (can't fire). Resets to Phase 1.
- **Mechanism Eject** (on jam): Ejects gears outward dealing radial damage. Silver lining for jams.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              MIDNIGHT MECHANISM                           │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Mechanism Eject: gear shrapnel on jam        │
│           → SpiralShrapnel, 35 gear fragments             │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: tick mark indicator    │
│           → MarbleSwirl: clock face around barrel         │
│           → 12-segment progression to Midnight            │
│  Layer 2: RibbonFoundation                              │
│           → Mode 1 PureBloom: bullet stream trail         │
│           → Visible at Phase 3+, thickens each phase     │
│  Layer 1: AttackFoundation (PRIMARY)                    │
│           → Mode 1 (Direct): individual shots             │
│           → Fire rate scales 3→24 per second              │
│           → Bloom scales with phase (1 layer→4 layers)   │
│           → Muzzle flash: Clockwork Gold + Pearl Frost   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Bullet Fire: AttackFoundation (PRIMARY)
| Phase | Fire Rate | Bullet Size | Bloom Layers | Muzzle Flash | Screen Shake |
|-------|-----------|-------------|-------------|-------------|-------------|
| 1 | 3/s | 8px | 1 (subtle) | Small Clockwork Gold flash | None |
| 2 | 6/s | 8px | 2 | Moderate CG flash | None |
| 3 | 12/s | 10px | 2 | Bright CG flash | Subtle (0.5) |
| 4 | 18/s | 10px | 3 | Intense CG + Pearl Frost | Light (1.0) |
| 5 | 24/s | 12px | 4 (full) | Massive CG + PF + screen glow | Moderate (2.0) |

| Parameter | Value | Purpose |
|-----------|-------|---------|
| Color | Clockwork Gold core → Pearl Frost trail | Mechanical bullet energy |
| Sound integration | Tick-tick-tick accelerating (implied by fire rate) | Counting to midnight |

#### Bullet Stream Trail: RibbonFoundation (Phase 3+)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Visible bullet stream |
| Width | 2px (Phase 3) → 4px (Phase 4) → 6px (Phase 5) | Intensifying stream |
| Color | Clockwork Gold | Consistent stream color |
| Activation | Only visible at Phase 3+ fire rates | Indicates momentum |

#### Tick Mark Indicator: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | MarbleSwirl | Clock face pattern around barrel |
| `circleRadius` | 0.30 (barrel-surrounding ring) | Tick mark display area |
| `scrollSpeed` | 0.0 (static until Midnight) | Fixed clock face |
| `intensity` | 0.5 + (0.15 × tick_count) → 2.3 at 12 marks | Brightens toward midnight |
| Segments | 12 divisions (each tick mark lights up one segment) | Clock face to midnight |
| Gradient LUT | ClairDeLune_Midnight_LUT | Night Mist Blue (empty) → Clockwork Gold (full) → Pearl White (Midnight) |

#### Midnight Strike: Custom screen effect + AttackFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| Single shot | AttackFoundation Mode 1 at 10x scale, 10x damage | Massive single projectile |
| Screen flash | Pearl White 50% opacity, 15 frames | Clock chime visual |
| ImpactFoundation | 12 rings (one per consumed tick), Clockwork Gold → Pearl White | Midnight detonation |
| Clock chime overlay | 12 tick marks flash simultaneously then shatter | All marks consumed |

#### Mechanism Eject: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | SpiralShrapnel | Gear fragments spiral outward |
| Spark count | 35 | Moderate gear debris |
| Colors | Clockwork Gold (80%), Night Mist Blue (20%) | Mechanical debris palette |
| Lifetime | 45 frames | Brief eject burst |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| MechanismBullet | AttackFoundation (Mode 1) | Rapid fire, rate scales with phase | CG→PF bullet, scaling bloom layers |
| BulletStream | RibbonFoundation | Visual trail at Phase 3+ | PureBloom CG stream, widening |
| MidnightStrikeShot | AttackFoundation (10x) | Single massive shot at 12 marks | Giant CG→PW projectile, 12-ring detonation, screen flash |
| MechanismEjectGears | ExplosionParticlesFoundation | Gear shrapnel on jam | SpiralShrapnel 35 CG fragments |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Midnight_LUT | `Assets/ClairDeLune/MidnightMechanism/Gradients/` | "Horizontal color gradient strip, cool night mist blue left through warm clockwork gold center to brilliant pearl white right, midnight countdown energy progression, game LUT texture, 256x16px --ar 16:1 --style raw" |
| TickMarkClockRing | `Assets/ClairDeLune/MidnightMechanism/Orbs/` | "Circular clock face ring with 12 evenly-spaced tick mark segments, ornate clockwork design with Roman numerals, pearl white and clockwork gold, on solid black background, game VFX indicator ring, 64x64px --ar 1:1 --style raw" |

---

## 6. Cog and Hammer (Ranged — Launcher)

### Identity & Musical Soul
A launcher that lobs clockwork bombs — spherical cog-encrusted devices that tick 3 times before detonating. Detonation sprays gear shrapnel in all directions. Variants include Sticky (attaches to enemies/surfaces), Chain Detonation (sequential timed explosions), and the Master Mechanism every 8th shot — a massive bomb that spawns 4 smaller bombs on detonation. It's the demolitions expert of the clockwork symphony.

### Lore Line
*"The precision of a watchmaker. The philosophy of a demolitions expert."*

### Combat Mechanics
- **Clockwork Bomb**: Lobbed arc, ticks 3 times (0.5s intervals), detonates after 1.5s. 4-tile radius, sprays 8 gear shrapnel.
- **Sticky Bomb** (alt fire): Attaches to first surface/enemy hit. Same tick + detonate pattern. Attached to enemies: follows them.
- **Chain Detonation**: 3+ bombs within 6 tiles → chain detonation (sequential 0.2s intervals). Visual chain arcs between bomb locations.
- **Master Mechanism** (every 8th shot): Giant bomb — 2x explosion radius, spawns 4 regular-sized bombs on detonation (which themselves detonate after 1s).
- **Shrapnel**: Each detonation sprays gear shrapnel projectiles dealing 30% bomb damage.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              COG AND HAMMER                               │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Gear shrapnel spray from each detonation     │
│           → RadialScatter, 8-16 gear fragments per bomb  │
│  Layer 3: ImpactFoundation                              │
│           → RippleShader: bomb detonation rings           │
│           → DamageZoneShader: Chain Detonation zone      │
│  Layer 2: AttackFoundation                              │
│           → Mode 3 (Spread): shrapnel gear scatter       │
│           → Mode 5 (Burst): Master Mechanism bomb launch │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: bomb body rendering   │
│           → VoronoiCell noise for cog-textured surface   │
│           → Tick animation: 3 brightness pulses          │
│           → Master Mechanism: 2x scale, gear-toothed rim│
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Bomb Body: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| Type | circleRadius | intensity (per tick) | Size | Unique Feature |
|------|-------------|---------------------|------|----------------|
| Regular Bomb | 0.28 | 1.2 → 1.8 → 2.8 → DETONATE | 24px | 3-tick countdown glow, VoronoiCell cog surface |
| Sticky Bomb | 0.28 | Same ticking pattern | 24px | Attaches to surface/enemy, follows if attached |
| Master Mechanism | 0.38 | 1.5 → 2.2 → 3.2 → DETONATE | 40px | 2x scale, visible gear-teeth rim, spawns 4 bombs |

Common parameters:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Cog-textured mechanical surface |
| `scrollSpeed` | 0.15 | Slow gear rotation |
| `edgeSoftness` | 0.05 | Sharp mechanical edge |
| Gradient LUT | ClairDeLune_Cog_LUT | Clockwork Gold → Pearl Frost → Night Mist Blue rim |
| Tick animation | 3 intensity pulses over 90 frames (30f each) | Visual countdown |

#### Detonation: ImpactFoundation + ExplosionParticlesFoundation
| Component | Regular Bomb | Master Mechanism |
|-----------|-------------|-----------------|
| ImpactFoundation rings | 6 rings, CG → PF, 0.5s | 10 rings, CG → PF → PW, 0.8s |
| ExplosionParticlesFoundation | RadialScatter, 8 gear fragments | RadialScatter, 16 gear fragments |
| Shrapnel | 8 gear pieces (AttackFoundation Mode 3) | 16 gear pieces + 4 sub-bomb spawns |
| Screen shake | Intensity 3 | Intensity 6 |
| Explosion radius | 4 tiles | 8 tiles |

#### Chain Detonation Visual: ImpactFoundation (sequential)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Chain arcs | RibbonFoundation Mode 1: connecting lines between chain bombs | Chain connection visual |
| Sequential timing | 0.2s between each detonation | Cascading explosion |
| DamageZoneShader | Breathing zone connecting all chain detonation points | Chain damage field |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ClockworkBomb | MagicOrbFoundation | Lobbed arc, 3-tick, detonates | VoronoiCell orb, 3-pulse countdown, CG gradient |
| StickyBomb | MagicOrbFoundation | Attaches, same tick pattern | Same orb + attachment indicator |
| MasterMechanismBomb | MagicOrbFoundation (2x) | Every 8th, spawns 4 bombs on det | 2x orb, gear-teeth rim, 10-ring detonation |
| GearShrapnel (x8-16) | ExplosionParticlesFoundation + AttackFoundation | Radial scatter from detonation | CG gear fragments, 30% bomb damage each |
| ChainDetonationArc | RibbonFoundation | Visual chain connector between bombs | PureBloom CG connecting lines |
### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Cog_LUT | `Assets/ClairDeLune/CogAndHammer/Gradients/` | "Horizontal color gradient strip, warm clockwork gold left through pearl frost center to cool night mist blue right, mechanical bomb energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ClockworkBombSprite | `Assets/ClairDeLune/CogAndHammer/Orbs/` | "Spherical clockwork bomb with visible cog-teeth around circumference and internal mechanical detail, warm clockwork gold with pearl frost highlights, ticking indicator on surface, on solid black background, game VFX projectile sprite, 48x48px --ar 1:1 --style raw" |
| GearShrapnelSprites | `Assets/ClairDeLune/CogAndHammer/Pixel/` | "Scattered broken clockwork gear fragments of various sizes, sharp mechanical debris, warm clockwork gold, on solid black background, game pixel particle sprites, 128x32px --ar 4:1 --style raw" |

---

## 7. Clockwork Grimoire (Magic)

### Identity & Musical Soul
A grimoire that cycles through 4 temporal modes — each representing a different aspect of timekeeping. Hour Mode fires a sustained beam. Minute Mode launches 12 ticking orbs. Second Mode rapid-fires precise bolts. Pendulum Mode creates a swinging temporal zone that alternates damage and healing. Temporal Synergy activates when all 4 modes are used in sequence — next cast is enhanced. The complete clockwork library.

### Lore Line
*"Hours of patience. Minutes of precision. Seconds of fury. And the pendulum swings eternal."*

### Combat Mechanics
- **4-Mode Cycle** (advances on alt fire):
  - **Hour Mode**: Sustained beam (like LaserFoundation). Slow, steady, massive damage per second.
  - **Minute Mode**: Fires 12 ticking orbs that detonate after 3s. Scattered area denial.
  - **Second Mode**: Rapid-fire precise bolts (20/s). Each bolt is small but piercing.
  - **Pendulum Mode**: Creates a pendulum zone — swings left/right dealing AoE damage on each swing.
- **Temporal Synergy**: Using all 4 modes in sequence (H→M→S→P) → next mode cast is 50% enhanced.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CLOCKWORK GRIMOIRE                           │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: MaskFoundation                                │
│           → RadialNoiseMaskShader: Pendulum Mode zone    │
│           → MarbleSwirl noise for temporal zone pattern  │
│           → Swinging pendulum oscillation                 │
│  Layer 3: AttackFoundation                              │
│           → Mode 1 (Direct): Second Mode rapid bolts     │
│           → Mode 3 (Spread): Minute Mode ticking orbs   │
│  Layer 2: MagicOrbFoundation                            │
│           → RadialNoiseMaskShader: Minute Mode orb bodies│
│           → VoronoiCell noise for ticking clock orbs     │
│           → 12 ticking orbs with countdown pulse          │
│  Layer 1: LaserFoundation (PRIMARY — Hour Mode)         │
│           → ConvergenceBeamShader: sustained hour beam   │
│           → ClairDeLune gradient LUT                      │
│           → 80px width, full temporal energy               │
│           → FlareRainbowShader → adapted for clock flare │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Hour Mode Beam: LaserFoundation (PRIMARY — ConvergenceBeamShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Detail Textures | 4: clock gear pattern, temporal flow, moonlight wave, tick mark texture | Clockwork beam interior |
| Gradient LUT | ClairDeLune_Hour_LUT | Temporal Void → Night Mist Blue → Soft Moonblue → Clockwork Gold → Pearl White |
| Width | 80px | Heavy sustained beam |
| Max Length | 2400px | Full screen reach |
| Endpoint flare | FlareRainbowShader adapted: Clockwork Gold + Pearl Frost radial flare | Impact point bloom |
| Duration | Sustained while held | Continuous hour beam |

#### Minute Mode Orbs: MagicOrbFoundation (RadialNoiseMaskShader) x12
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Ticking clock orb surface |
| `circleRadius` | 0.22 (small, many) | 12 scattered orbs |
| `scrollSpeed` | 0.05 (nearly static until detonation) | Ticking stillness |
| `intensity` | 1.0 → 1.5 → 2.5 over 3s, then DETONATE | 3-step countdown |
| Gradient LUT | ClairDeLune_Minute_LUT | Night Mist Blue → Clockwork Gold at detonation |
| Detonation | ImpactFoundation: 4 rings per orb | 12 small detonations |
| Tick visual | Brightness pulse every second (60 frames) | Audible-feeling countdown |

#### Second Mode Bolts: AttackFoundation (Mode 1)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Fire rate | 20/second | Rapid sustained fire |
| Bolt size | 6px | Small precise bolts |
| Bloom | 1 layer (Soft Moonblue) | Minimal but visible |
| Color | Pearl Frost core → Soft Moonblue trail | Temporal bolt energy |
| Piercing | True (pierces 1 enemy) | Second hand precision |

#### Pendulum Mode Zone: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | MarbleSwirl | Flowing temporal pendulum pattern |
| `scrollSpeed` | 0.3 (oscillating direction) | Pendulum swing motion |
| `circleRadius` | 0.40 | Wide pendulum zone |
| `intensity` | 2.0 base, pulses 1.5 → 2.5 with swing rhythm | Rhythmic intensity |
| Pendulum offset | UV offset oscillates ±0.15 on x-axis at 1Hz | Left-right pendulum swing |
| Gradient LUT | ClairDeLune_Pendulum_LUT | Soft Moonblue → Pearl Frost → Clockwork Gold |
| Damage timing | Damage dealt at swing extremes (left and right) | Pendulum hit timing |

#### Temporal Synergy (H→M→S→P sequence bonus)
| Visual | 50% larger next cast, Clockwork Gold corona overlay, brief gold flash on mode change |
| Indicator | 4-segment progress arc (MaskFoundation mini-ring at grimoire position, fills per mode used) |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| HourBeam | LaserFoundation | Sustained beam (Hour Mode) | ConvergenceBeamShader, 80px, CG→PW gradient, clock interior |
| MinuteOrb (x12) | MagicOrbFoundation | 3s ticking orbs, detonate | VoronoiCell, 3-tick countdown, small detonation each |
| SecondBolt (rapid) | AttackFoundation (Mode 1) | 20/s rapid-fire piercing bolts | 6px PF→SMB bolts, 1-layer bloom |
| PendulumZone | MaskFoundation (persistent) | Oscillating AoE zone, damage at extremes | MarbleSwirl pendulum, UV oscillation, rhythmic pulse |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Hour_LUT | `Assets/ClairDeLune/ClockworkGrimoire/Gradients/` | "Horizontal color gradient strip, temporal void left through night mist blue through soft moonblue through clockwork gold to pearl white right, sustained clockwork beam energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ClockTickOrbSprite | `Assets/ClairDeLune/ClockworkGrimoire/Orbs/` | "Small clockwork ticking orb with VoronoiCell crystalline surface and subtle internal clock mechanism visible, pearl frost and clockwork gold, on solid black background, game VFX projectile sprite, 32x32px --ar 1:1 --style raw" |

---

## 8. Orrery of Dreams (Magic)

### Identity & Musical Soul
An orrery — a mechanical model of celestial orbits. This weapon summons 3 Dream Spheres that orbit the player at different distances (like planets). Each sphere fires its own projectile type. The Dream Alignment special triggers when all 3 spheres align — they launch a focused chain blast through the alignment axis. Moonlight from Clair de Lune's palette augments sphere damage at night. A dreaming clockwork cosmos in miniature.

### Lore Line
*"A clock that counts not hours, but worlds."*

### Combat Mechanics
- **3 Dream Spheres** (orbiting player at different distances/speeds):
  - **Inner Sphere** (close, fast): Rapid small projectiles.
  - **Middle Sphere** (mid, moderate): Homing orbs.
  - **Outer Sphere** (far, slow): Heavy slow projectiles with AoE.
- **Dream Alignment** (every 12s): All 3 spheres align on cursor direction → chain launch: combined projectile traveling through all 3 sphere positions dealing accumulated damage.
- **Orrery Adjustment** (alt fire): Reverses orbit direction. During reversal moment, spheres freeze 0.3s.
- **Moonlight Augmentation**: At night (7:30 PM–4:30 AM), sphere damage +15%, Dream Alignment +30%.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              ORRERY OF DREAMS                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Dream Alignment chain impact   │
│  Layer 3: RibbonFoundation                              │
│           → Mode 1 PureBloom: orbital path trails         │
│           → 3 concentric orbit visualization rings        │
│  Layer 2: SparkleProjectileFoundation                   │
│           → Dream Alignment chain launch projectile      │
│           → 5-layer combined sphere energy rendering     │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: 3 Dream Sphere bodies │
│           → PerlinFlow noise for dreamlike energy         │
│           → 3 variants: Inner/Middle/Outer sizing        │
│           → Moonlight boost: +20% intensity at night     │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Dream Spheres: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| Sphere | circleRadius | intensity | Orbit Radius | Orbit Speed | Color Bias |
|--------|-------------|-----------|-------------|-------------|------------|
| Inner | 0.22 | 1.8 | 40px | 3°/frame | Clockwork Gold dominant |
| Middle | 0.28 | 1.6 | 80px | 2°/frame | Soft Moonblue balanced |
| Outer | 0.35 | 1.4 | 120px | 1°/frame | Pearl Frost dominant |

All share:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | PerlinFlow | Dreamlike internal energy |
| `scrollSpeed` | 0.25 | Flowing dream currents |
| `edgeSoftness` | 0.08 | Soft dreamlike boundary |
| Night boost | `intensity *= 1.2` during nighttime | Moonlight Augmentation visual |
| Gradient LUT | ClairDeLune_Dream_LUT | Night Mist Blue → Soft Moonblue → Pearl Frost → Clockwork Gold |

#### Orbital Paths: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean orbital track |
| Rings | 3 concentric orbits at 40/80/120px radius | Orrery orbital display |
| Width | 1px (subtle) | Thin orbit lines |
| Color | Night Mist Blue (30% opacity) | Subtle background orbital tracks |
| Sphere position indicator | Bright pulse at each sphere position on the ring | Track active sphere positions |

#### Dream Alignment Chain: SparkleProjectileFoundation + ImpactFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| SparkleProjectileFoundation | 5-layer rendering, combined palette of all 3 spheres | Combined sphere energy |
| Chain path | Travels through Inner → Middle → Outer sphere positions in sequence | Alignment path |
| Accumulated damage | Sum of all 3 sphere base damages × 2 | Combined power |
| ImpactFoundation (at final target) | RippleShader: 8 rings, full ClairDeLune palette, 0.6s | Alignment detonation |
| Moonlight bonus | +30% at night | Dream Alignment augmentation |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| InnerSphereShot | AttackFoundation (Mode 1) | Rapid small projectiles from Inner Sphere | 8px CG bolts, 2-layer bloom |
| MiddleSphereOrb | MagicOrbFoundation (mini) | Homing orbs from Middle Sphere | Small PerlinFlow orb, SMB tint, homing |
| OuterSphereBomb | MagicOrbFoundation (heavy) | Slow AoE from Outer Sphere | Large PF orb, 3-tile AoE on impact |
| DreamAlignmentChain | SparkleProjectileFoundation | Chain through all 3 sphere positions | 5-layer combined rendering, heavy bloom |
| AlignmentDetonation | ImpactFoundation | Final target explosion | 8-ring full palette ripple |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Dream_LUT | `Assets/ClairDeLune/OrreryOfDreams/Gradients/` | "Horizontal color gradient strip, night mist blue left through soft moonblue through pearl frost to warm clockwork gold right, dreaming celestial energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| DreamSphereSprites | `Assets/ClairDeLune/OrreryOfDreams/Orbs/` | "Three dreamlike celestial spheres of different sizes with internal flowing PerlinFlow energy, soft moonblue and pearl frost with clockwork gold accents, each sphere has a distinct planetary quality, on solid black background, game summon entity sprites, 128x48px --ar 8:3 --style raw" |

---

## 9. Requiem of Time (Magic)

### Identity & Musical Soul
A requiem for time itself. This weapon creates temporal fields — Forward (speeds allies) and Reverse (slows enemies). When Forward and Reverse fields overlap, a Temporal Paradox erupts dealing massive damage. The weapon costs health to use (temporal cost), making it a high-risk high-reward tool. The screen warps within temporal fields. Time is not a resource — it's a sacrifice.

### Lore Line
*"Time asks a price for every second borrowed."*

### Combat Mechanics
- **Forward Field**: Place a zone that speeds all allies inside by 30% and boosts their attack speed 15%. Lasts 6s. 12-tile diameter.
- **Reverse Field** (alt fire): Place a zone that slows all enemies inside by 40% and reduces their damage 20%. Lasts 6s. 12-tile diameter. Costs 5% current HP.
- **Temporal Paradox**: Forward and Reverse fields overlapping → Paradox zone. Deals massive damage per second to enemies inside. Screen distortion within zone. 4s duration.
- **Temporal Exhaustion**: After Paradox, player takes 2x damage for 3s. Risk/reward.
- **Time's Toll**: Each field cast costs a small amount of HP (2% current for Forward, 5% for Reverse).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              REQUIEM OF TIME                              │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: AttackAnimationFoundation                     │
│           → Temporal Paradox screen distortion overlay   │
│           → Chromatic aberration in Paradox zone          │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Paradox detonation rings        │
│           → DamageZoneShader: Paradox sustained AoE      │
│  Layer 3: SmokeFoundation                               │
│           → Temporal mist within Forward/Reverse fields  │
│           → Billowing style, slow drift                    │
│  Layer 2: MaskFoundation (PRIMARY)                      │
│           → RadialNoiseMaskShader: Forward Field zone    │
│           → RadialNoiseMaskShader: Reverse Field zone    │
│           → PerlinFlow noise for Forward (flowing time)  │
│           → MarbleSwirl noise for Reverse (frozen time)  │
│           → VoronoiCell noise for Paradox (shattered)    │
│  Layer 1: AttackFoundation                              │
│           → Field placement projectile (mark position)   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Forward Field: MaskFoundation (PRIMARY — RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | PerlinFlow | Flowing, accelerating time |
| `scrollSpeed` | 0.5 (fast — time speeds up) | Fast internal flow |
| `circleRadius` | 0.45 (12-tile diameter) | Large field zone |
| `edgeSoftness` | 0.12 | Soft temporal boundary |
| `intensity` | 1.6 | Visible but not overwhelming |
| Gradient LUT | ClairDeLune_Forward_LUT | Soft Moonblue → Pearl Frost → Clockwork Gold (warm, accelerating) |
| SmokeFoundation | Billowing style, 15 puffs, rapid cycle | Temporal mist inside field |

#### Reverse Field: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | MarbleSwirl | Frozen, coagulated time |
| `scrollSpeed` | 0.05 (nearly frozen) | Time slowed to near-stop |
| `circleRadius` | 0.45 | Same size as Forward |
| `edgeSoftness` | 0.06 (sharper boundary) | More defined frozen edge |
| `intensity` | 1.8 | Slightly more ominous |
| Gradient LUT | ClairDeLune_Reverse_LUT | Temporal Void → Night Mist Blue → Soft Moonblue (cold, frozen) |
| SmokeFoundation | Billowing style, 15 puffs, very slow drift | Nearly frozen mist |

#### Temporal Paradox: MaskFoundation (VoronoiCell) + ImpactFoundation + AttackAnimationFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| MaskFoundation | VoronoiCell noise, `scrollSpeed: 0.6`, `intensity: 3.0`, Gradient: Temporal Void → Night Mist Blue → Clockwork Gold → Pearl White | Shattered time overlay in overlap zone |
| ImpactFoundation DamageZoneShader | Breathing zone at overlap, pulsing damage | Sustained Paradox AoE |
| AttackAnimationFoundation | Screen chromatic aberration within zone view, Phase 2-3 distortion, 4s | Reality-warping screen effect |
| ImpactFoundation RippleShader | 8 rings, full palette, 0.8s (on Paradox creation) | Paradox birth rings |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ForwardFieldZone | MaskFoundation (persistent) | 6s speed boost zone | PerlinFlow flowing, SMB→PF→CG gradient, temporal mist |
| ReverseFieldZone | MaskFoundation (persistent) | 6s slow zone, costs 5% HP | MarbleSwirl frozen, TV→NMB→SMB gradient, frozen mist |
| TemporalParadox | MaskFoundation + ImpactFoundation | Overlap zone, massive AoE | VoronoiCell shattered + chromatic aberration + 8-ring birth ripple |
| FieldPlacement | AttackFoundation (Mode 1) | Marks field position | Small moon-blue bolt, no damage |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Forward_LUT | `Assets/ClairDeLune/RequiemOfTime/Gradients/` | "Horizontal color gradient strip, soft moonblue left through pearl frost center to warm clockwork gold right, accelerating forward time energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ClairDeLune_Reverse_LUT | `Assets/ClairDeLune/RequiemOfTime/Gradients/` | "Horizontal color gradient strip, deep temporal void left through night mist blue center to soft moonblue right, frozen reverse time energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| TemporalParadoxVFX | `Assets/ClairDeLune/RequiemOfTime/Flare/` | "Temporal paradox zone with shattered VoronoiCell reality fractures and chromatic aberration distortion effect, mixed warm clockwork gold and cold night mist blue tearing apart, on solid black background, game VFX overlay texture, 256x256px --ar 1:1 --style raw" |

---

## 10. Lunar Phylactery (Summon)

### Identity & Musical Soul
A moonlight sentinel phylactery — a crystalline vessel that houses a fragment of the moon's soul. Summons a sentinel that fires sustained moonlight beams. The sentinel's power scales with the player's HP (Soul-Link). Phylactery Pulse periodically releases healing moonlight to nearby allies. When multiple sentinels cross beams, the intersection creates an amplified AoE. You protect the phylactery. The phylactery protects you.

### Lore Line
*"The moon does not die. It merely waits inside the crystal for someone worthy to carry it."*

### Combat Mechanics
- **Moonlight Sentinel**: Floating crystal sentinel fires sustained moonlight beam at nearest enemy. Beam deals continuous damage.
- **Soul-Link**: Sentinel damage scales with player's current HP% (80%+ HP = 100% damage, 50% HP = 75%, 25% = 50%). Encourages staying healthy.
- **Phylactery Pulse** (every 10s): Sentinel releases a moonlight pulse healing allies within 8 tiles for 3% max HP.
- **Beam Crossing**: 2+ sentinels with crossing beams → intersection point deals 2x AoE damage in 3-tile radius.
- **Crystal Resonance**: Sentinel gains +10% damage for each other Clair de Lune summon active.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              LUNAR PHYLACTERY                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Phylactery Pulse healing rings │
│           → RippleShader: Beam Crossing AoE              │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Beam Crossing AoE zone│
│           → PerlinFlow noise for moonlight energy field  │
│  Layer 2: LaserFoundation                               │
│           → ConvergenceBeamShader: primary moonlight beam│
│           → Soft moonblue gradient, 40px width            │
│           → ThinBeamShader: secondary thin crossing beams│
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: sentinel crystal body │
│           → VoronoiCell noise for crystalline surface    │
│           → Intensity scales with player HP% (Soul-Link)│
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Sentinel Body: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Crystalline phylactery surface |
| `scrollSpeed` | 0.1 | Slow crystal shimmer |
| `circleRadius` | 0.30 | Crystal sentinel size |
| `edgeSoftness` | 0.04 | Sharp crystal edges |
| `intensity` | 1.0 + (playerHP% × 1.5) → max 2.5 at full HP | Soul-Link brightness scaling |
| Gradient LUT | ClairDeLune_Phylactery_LUT | Night Mist Blue → Soft Moonblue → Pearl Frost → Pearl White |
| Idle float | Gentle sine-wave bob, 2px amplitude, 0.02 Hz | Floating crystal animation |

#### Moonlight Beam: LaserFoundation (ConvergenceBeamShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Detail Textures | 4: moonlight wave, temporal flow, crystal shimmer, pearl mist | Moonlit beam interior |
| Gradient LUT | ClairDeLune_Beam_LUT | Night Mist Blue → Soft Moonblue → Pearl Frost |
| Width | 40px | Mid-weight sustained beam |
| Damage | Continuous, scales with HP% | Soul-Link damage scaling |
| Color tone | Cooler than other ClairDeLune beams — more moonlight, less clockwork | Moonlit identity |

#### Beam Crossing: ThinLaserFoundation + MaskFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ThinBeamShader | 10px secondary beams | Thin crossing beam visuals |
| MaskFoundation | PerlinFlow, `circleRadius: 0.25`, `intensity: 2.5` | Amplified AoE at intersection |
| ImpactFoundation | 4 rings Pearl Frost → Soft Moonblue, pulsing at beam overlap | Crossing AoE damage zone |
| Damage | 2x base beam damage, 3-tile radius | Beam intersection bonus |

#### Phylactery Pulse: ImpactFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| RippleShader | 4 rings, Soft Moonblue → Pearl Frost → Pearl White, 0.8s, 8-tile range | Healing pulse expansion |
| Healing | 3% max HP to allies within range | Phylactery support function |
| Interval | Every 600 frames (10s) | Periodic healing |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| MoonlightSentinel | MagicOrbFoundation (persistent) | Floating crystal, fires beam | VoronoiCell crystal, Soul-Link intensity scaling |
| MoonlightBeam | LaserFoundation | Sustained beam to nearest enemy | ConvergenceBeamShader moonlight, 40px |
| BeamCrossingZone | MaskFoundation + ImpactFoundation | AoE at beam intersection | PerlinFlow zone + 4-ring pulsing ripple |
| PhylacteryPulse | ImpactFoundation | 10s healing burst | 4-ring SMB→PF→PW expansion, 8-tile |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Phylactery_LUT | `Assets/ClairDeLune/LunarPhylactery/Gradients/` | "Horizontal color gradient strip, night mist blue left through soft moonblue through pearl frost to pearl white right, crystalline moonlight energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| PhylacteryCrystalSprite | `Assets/ClairDeLune/LunarPhylactery/Orbs/` | "Floating crystalline phylactery vessel with VoronoiCell faceted surface and internal soft moonlight glow, pearl frost and soft moonblue with night mist blue deep interior, sacred magical container, on solid black background, game summon entity sprite, 48x48px --ar 1:1 --style raw" |

---

## 11. Gear-Driven Arbiter (Summon)

### Identity & Musical Soul
A summon that deploys a clockwork arbitration construct — a floating judicial automaton that fires gear projectiles and delivers Temporal Judgment. Hits apply ticking debuff marks. At max marks, Arbiter's Verdict triggers — a massive focused detonation. Multiple Arbiters sync for Clockwork Court — coordinated judgment barrages. Mechanical justice, inescapable and precise.

### Lore Line
*"The gears turn. The verdict is predetermined. The only variable is when."*

### Combat Mechanics
- **Clockwork Arbiter Minion**: Floating construct fires gear projectiles at enemies. Medium fire rate, decent damage.
- **Temporal Judgment**: Hits apply Temporal Judgment debuff (stacks to 8, 6s each). Each stack shows a clock-tick mark on the enemy. At 8 stacks → timer appears counting down 3s.
- **Arbiter's Verdict** (8 stacks + 3s countdown): Massive focused detonation — 5x base damage in 4-tile radius. Consumes all marks.
- **Arbiter's Focus** (alt fire): Target a specific enemy. Arbiter fires exclusively at that target with +20% fire rate.
- **Clockwork Court**: 3+ Arbiters active → synchronized barrages every 8s (all fire simultaneously + 30% bonus).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              GEAR-DRIVEN ARBITER                          │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Clockwork Court synchronized burst debris    │
│           → RadialScatter, 45 gear fragments              │
│  Layer 3: ImpactFoundation                              │
│           → RippleShader: Arbiter's Verdict detonation   │
│           → DamageZoneShader: Verdict AoE zone            │
│  Layer 2: MaskFoundation                                │
│           → RadialNoiseMaskShader: Temporal Judgment      │
│           → MarbleSwirl: clock-face indicator per enemy  │
│           → 8-segment tick progression + 3s countdown    │
│  Layer 1: AttackFoundation (PRIMARY)                    │
│           → Mode 1 (Direct): gear projectile fire         │
│           → Gear-shaped projectiles with spin animation  │
│           → Clockwork Gold→Pearl Frost palette            │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Arbiter Body: MagicOrbFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | VoronoiCell | Mechanical construct surface |
| `circleRadius` | 0.32 | Judicial construct size |
| `scrollSpeed` | 0.1 | Slow internal gear shimmer |
| `intensity` | 1.8 | Solid mechanical presence |
| Gradient LUT | ClairDeLune_Arbiter_LUT | Night Mist Blue → Clockwork Gold → Pearl Frost |
| Gear rim | 8 gear teeth rendered at entity edge | Mechanical identity |

#### Gear Projectiles: AttackFoundation (PRIMARY — Mode 1)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Speed | 12 px/frame | Moderate fire speed |
| Size | 16px | Gear-sized projectile |
| Spin | 12°/frame constant rotation | Gear spinning in flight |
| Bloom | 2 layers (CG core + PF glow) | Clockwork energy |
| Fire rate | Mid (base) → +20% focused (alt fire) | Arbiter's Focus boost |
| Clockwork Court | Synchronized volley every 8s from 3+ Arbiters | Court barrage |

#### Temporal Judgment Indicator: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | MarbleSwirl | Clock-face judicial indicator |
| `circleRadius` | 0.18 (per-enemy overhead) | Small judgment clock |
| `scrollSpeed` | 0.0 (static, ticking) | Frozen until countdown |
| `intensity` | 0.6 + (0.2 × stack_count) → 2.2 at 8 stacks | Builds urgency |
| Segments | 8 divisions (each hit lights one segment) | Dual-function: damage+progress |
| At 8 stacks | 3s countdown timer overlay, clock hands spin rapidly, intensity 3.0 | Verdict countdown |
| Gradient LUT | ClairDeLune_Judgment_LUT | Night Mist Blue → Clockwork Gold (urgency) → Pearl White (Verdict) |

#### Arbiter's Verdict: ImpactFoundation + ExplosionParticlesFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ImpactFoundation RippleShader | 10 rings, CG → PF → PW, 0.8s, 4-tile radius | Judgment detonation rings |
| DamageZoneShader | Brief flash zone at verdict target | Verdict AoE indicator |
| ExplosionParticlesFoundation | RadialScatter, 20 gear fragments | Judgment shrapnel |
| Clock shatter | 8 tick-mark segments shatter outward | All marks consumed VFX |
| Screen shake | Intensity 5 | Verdict weight |

#### Clockwork Court Barrage: AttackFoundation (synchronized)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Sync timing | All 3+ Arbiters fire simultaneously every 8s | Coordinated volley |
| Bonus | +30% damage per projectile | Court synchronization bonus |
| Visual | All Arbiters flash CG simultaneously before volley | Visual court activation |
| ExplosionParticlesFoundation | RadialScatter 45 fragments if 4+ Arbiters | Court debris burst |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| GearProjectile | AttackFoundation (Mode 1) | Spinning gear fire | 16px CG gear, 12°/f spin, 2-layer bloom |
| TemporalJudgmentMark | MaskFoundation (per-enemy) | 8-stack clock indicator + 3s countdown | MarbleSwirl, 8-segment fill, urgency build |
| ArbiterVerdict | ImpactFoundation + ExplosionParticlesFoundation | 5x detonation at 8+3 | 10-ring ripple + 20 gear fragments + clock shatter |
| CourtBarrageVolley | AttackFoundation (synchronized x3+) | Coordinated burst every 8s | All Arbiters flash + simultaneous volley |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Judgment_LUT | `Assets/ClairDeLune/GearDrivenArbiter/Gradients/` | "Horizontal color gradient strip, cool night mist blue left through warm clockwork gold center to brilliant pearl white right, judicial countdown energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ArbiterConstructSprite | `Assets/ClairDeLune/GearDrivenArbiter/Orbs/` | "Floating clockwork judicial construct with VoronoiCell faceted armor and 8 gear teeth around circumference, night mist blue and clockwork gold with pearl frost judicial glow, authoritative mechanical minion, on solid black background, game summon entity sprite, 48x48px --ar 1:1 --style raw" |

---

## 12. Automaton's Tuning Fork (Summon)

### Identity & Musical Soul
The pinnacle of Clair de Lune's clockwork symphony — a tuning fork that creates resonance waves at 4 frequencies. Each frequency (A, C, E, G) applies a different buff/debuff. Resonance zones persist and can overlap — where 2+ frequencies overlap, Perfect Resonance occurs (amplified effect + damage). Conductor's Final Note is the ultimate: all 4 frequencies simultaneously for a devastating harmonic convergence. Music made clockwork. Clockwork made lethal.

### Lore Line
*"A-C-E-G. Four notes. Four frequencies. When they align — even time trembles."*

### Combat Mechanics
- **Resonance Minion**: Automaton strikes its tuning fork, creating resonance wave rings.
- **4 Frequency Cycle** (auto-cycles):
  - **A (La)**: Attack boost zone (+12% damage to allies inside)
  - **C (Do)**: Defense zone (-15% enemy damage inside)
  - **E (Mi)**: Speed zone (+20% move speed to allies inside)
  - **G (Sol)**: Damage zone (deals direct resonance damage to enemies inside)
- **Perfect Resonance**: 2+ overlapping frequency zones → amplified combined effect + 2x damage.
- **Conductor's Final Note** (every 30s): All 4 frequencies emitted simultaneously — massive 15-tile AoE combining all effects, 4s duration.
- **Harmonic Memory**: Each successive use of same frequency in a row → +5% per stack (max 4 stacks, 20% bonus). Resets on frequency change.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              AUTOMATON'S TUNING FORK                      │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: AttackAnimationFoundation                     │
│           → Conductor's Final Note screen-wide overlay   │
│           → Harmonic convergence screen effect, 4s        │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: frequency zones        │
│           → 4 parameterized LUTs per frequency            │
│           → PerlinFlow noise for resonance wave pattern  │
│           → VoronoiCell for Perfect Resonance overlap    │
│  Layer 2: AttackFoundation                              │
│           → Mode 1: resonance wave ring projectiles      │
│           → 4 color variants per frequency                │
│  Layer 1: ImpactFoundation (PRIMARY)                    │
│           → RippleShader: expanding resonance wave rings │
│           → 4 color-coded ring sets per frequency         │
│           → DamageZoneShader: G frequency damage zone    │
│           → Perfect Resonance burst ripple                │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Resonance Wave Rings: ImpactFoundation (PRIMARY — RippleShader)
| Frequency | Ring Color | Ring Count | Special |
|-----------|-----------|------------|---------|
| A (La — Attack) | Clockwork Gold → Pearl Frost | 4 rings | Gold-dominant, aggressive feel |
| C (Do — Defense) | Night Mist Blue → Pearl Frost | 4 rings | Blue-dominant, protective feel |
| E (Mi — Speed) | Soft Moonblue → Pearl White | 4 rings | Light, airy, fast expansion |
| G (Sol — Damage) | Clockwork Gold → Night Mist Blue → Pearl White | 6 rings (more intense) | Mixed, damage-dealing |

All share: `0.8s expansion, 8-tile radius base`

#### Frequency Zones: MaskFoundation (RadialNoiseMaskShader) — persistent regions
| Frequency | Noise | scrollSpeed | intensity | Gradient LUT |
|-----------|-------|------------|-----------|-------------|
| A (Attack) | PerlinFlow | 0.3 | 1.4 | CG→PF (warm boost) |
| C (Defense) | PerlinFlow | 0.15 | 1.2 | NMB→PF (cool shield) |
| E (Speed) | PerlinFlow | 0.5 (fastest — speed zone) | 1.0 | SMB→PW (light, airy) |
| G (Damage) | PerlinFlow | 0.35 | 1.8 (most intense) | CG→NMB→PW (mixed aggressive) |

All share: `circleRadius: 0.40, edgeSoftness: 0.10, duration: 300 frames (5s)`

#### Perfect Resonance (2+ zone overlap): MaskFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| MaskFoundation (overlap) | VoronoiCell noise, `scrollSpeed: 0.4`, `intensity: 2.8` | Crystalline resonance amplification |
| ImpactFoundation burst | 8 rings, combined colors of overlapping frequencies, 0.6s | Resonance burst at overlap trigger |
| Damage | 2x base resonance damage | Amplified overlap effect |
| Combined buff/debuff | Both frequency effects amplified 50% | Stacking buffs |

#### Conductor's Final Note: AttackAnimationFoundation
| Phase | Duration | Visual |
|-------|----------|--------|
| Build (Phase 1) | 20 frames | Automaton raises tuning fork, 4-color aura builds around it |
| Emission (Phase 2) | 15 frames | 4 simultaneous resonance ring sets emanate outward (all frequencies) |
| Convergence (Phase 3) | 180 frames (3s of active zone) | Combined 15-tile zone with all 4 effects, VoronoiCell overlay |
| Resolution (Phase 4) | 25 frames | All frequencies fade with harmonic shimmer, brief screen-wide Pearl Frost flash |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ResonanceWaveA | ImpactFoundation | Expanding Attack boost rings | CG→PF 4 rings, warm boost zone |
| ResonanceWaveC | ImpactFoundation | Expanding Defense rings | NMB→PF 4 rings, cool shield zone |
| ResonanceWaveE | ImpactFoundation | Expanding Speed rings | SMB→PW 4 rings, fast airy expansion |
| ResonanceWaveG | ImpactFoundation | Expanding Damage rings | CG→NMB→PW 6 rings, intense damage zone |
| FrequencyZone (per type) | MaskFoundation (persistent) | 5s buff/debuff/damage zone | PerlinFlow noise, frequency-specific LUT |
| PerfectResonanceOverlap | MaskFoundation + ImpactFoundation | 2+ zone overlap trigger | VoronoiCell overlay + 8-ring burst |
| ConductorFinalNote | AttackAnimationFoundation | 30s cooldown, 4s all-frequency zone | Full 4-phase cinematic + 15-tile combined zone |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ClairDeLune_Frequency_LUTs | `Assets/ClairDeLune/AutomatonstuningFork/Gradients/` | "Four horizontal color gradient strips stacked vertically representing four musical frequencies: A (clockwork gold to pearl frost), C (night mist blue to pearl frost), E (soft moonblue to pearl white), G (clockwork gold through night mist blue to pearl white), game LUT texture set, 256x64px --ar 4:1 --style raw" |
| TuningForkAutomaton | `Assets/ClairDeLune/AutomatonsTuningFork/Orbs/` | "Mechanical automaton holding a large tuning fork with visible resonance waves emanating from the fork tips, clockwork body with gear joints, clockwork gold and pearl frost with night mist blue energy accents, on solid black background, game summon entity sprite, 48x64px --ar 3:4 --style raw" |
| ResonanceWaveRing | `Assets/ClairDeLune/AutomatonsTuningFork/Trails/` | "Expanding circular resonance wave ring with visible frequency oscillation pattern along the ring, clean sharp concentric wave lines, pearl frost and soft moonblue, on solid black background, game VFX ring texture, 128x128px --ar 1:1 --style raw" |

---

## Cross-Theme Synergy Notes

### Clair de Lune Theme Unity — Foundation Coverage
All 12 weapons built on Foundation Weapons scaffolding with consistent clockwork-temporal identity:
- **SwordSmearFoundation** (1 weapon): PerlinFlow temporal smear arcs for the signature broadsword
- **ThinSlashFoundation** (1 weapon): Ultra-thin rapier precision at 0.012 lineWidth (thinnest in mod)
- **MagicOrbFoundation** (5 weapons): VoronoiCell crystals, PerlinFlow dreams, bomb/sentinel/sphere bodies
- **SparkleProjectileFoundation** (2 weapons): Crystal arrows and temporal echo ghosts
- **MaskFoundation** (7 weapons): The backbone — Time Slow Fields, tick indicators, gear meshes, frequency zones, temporal fields, midnight clocks, pendulums
- **LaserFoundation** (3 weapons): Hour Mode beam, moonlight sentinel beam, thin crossing beams
- **ThinLaserFoundation** (1 weapon): Beam crossing thin beams
- **RibbonFoundation** (3 weapons): Gear mesh connectors, bullet streams, orbital paths
- **ImpactFoundation** (8 weapons): Temporal ripples, judgment detonations, resonance waves, fracture bursts
- **ExplosionParticlesFoundation** (3 weapons): Gear shrapnel, mechanism ejects, court bursts
- **SmokeFoundation** (1 weapon): Temporal mist in time fields
- **AttackFoundation** (7 weapons): Gear launches, bullets, bolts, resonance waves
- **AttackAnimationFoundation** (3 weapons): Clockwork Overflow, Temporal Paradox, Conductor's Final Note

### Noise Texture Strategy — Clockwork Identity
| Noise Texture | Usage | Clockwork Feel |
|---------------|-------|---------------|
| VoronoiCell | Crystalline surfaces, cracked time, gear-textured bombs, construct armor | Faceted, mechanical, crystalline — the core "clockwork" texture |
| MarbleSwirl | Clock faces, pendulum zones, judgment indicators | Veined, ornate, clock-intricate — timekeeping surfaces |
| PerlinFlow | Temporal energy, dream spheres, forward time fields, resonance zones | Flowing, continuous — the "temporal flow" of time's movement |
| TileableFBMNoise | Clock dial detail (secondary use), cosmic nebula in dream contexts | Organic detail contrast against mechanical primary textures |

### Cross-Weapon Synergies
| Synergy | Weapons | Mechanic |
|---------|---------|----------|
| **Temporal Refraction** | Chronologicality + Starfall Whisper | Arrows bend through Time Slow Fields |
| **Clockwork Court** | Multiple Gear-Driven Arbiters | Synchronized judgment volleys |
| **Crystal Resonance** | Lunar Phylactery + any Clair summon | +10% damage per Clair summon active |
| **Harmonic Memory** | Automaton's Tuning Fork (internal) | Same-frequency stacking bonus |
| **Beam Crossing** | Multiple Lunar Phylacteries | Intersection AoE amplification |

### Visual Distinction (Despite Shared Palette)
Despite all weapons sharing the Night Mist Blue → Soft Moonblue → Pearl Frost → Clockwork Gold palette:
- **Chronologicality**: Heavy temporal weight, wide clock-hand arcs — broadsword gravitas
- **Temporal Piercer**: Ultra-thin precision, clinical puncture marks — rapier elegance
- **Clockwork Harmony**: Spinning gears, meshing collisions — mechanical dynamism
- **Starfall Whisper**: Cracked time fractures, barely-visible arrows — fragile whispers
- **Midnight Mechanism**: Escalating tempo barrage, tick countdown — relentless pressure
- **Cog and Hammer**: Ticking bombs, spray shrapnel — demolitions chaos
- **Clockwork Grimoire**: 4 distinct modes, each its own identity — versatile mastery
- **Orrery of Dreams**: Orbiting spheres, planetary alignment — celestial clockwork
- **Requiem of Time**: Forward/Reverse field duality, Paradox tears — temporal sacrifice
- **Lunar Phylactery**: Floating crystal sentinel, sustained beams — moonlight guardian
- **Gear-Driven Arbiter**: Judicial precision, ticking judgment countdown — mechanical justice
- **Automaton's Tuning Fork**: 4-frequency resonance, harmonic overlap — musical clockwork