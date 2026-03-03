# 🌙 Nachtmusik — Resonance Weapons Planning

> *"A Little Night Music — nocturnal wonder and stellar beauty."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Mozart's Eine kleine Nachtmusik — a nocturnal serenade, playful yet profound |
| **Emotional Core** | Nocturnal wonder, stellar beauty |
| **Color Palette** | Deep indigo, starlight silver, cosmic blue |
| **Palette Hex** | Night Void `(10, 10, 30)` → Deep Indigo `(40, 30, 100)` → Cosmic Blue `(60, 80, 180)` → Starlight Silver `(180, 200, 230)` → Moon Pearl `(220, 225, 245)` → Stellar White `(240, 245, 255)` |
| **Lore Color** | `new Color(100, 120, 200)` — Starlight Indigo |
| **Lore Keywords** | Night, stars, twilight, constellation, cosmic, serenade, midnight, nebula, stellarr nocturnal, celestial |
| **VFX Language** | Starlight trails, constellation patterns, cosmic dust, nebula clouds, twilight gradients, aurora-like ribbons, moon glow, stellar explosions |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Nocturnal Executioner | Melee | Execution Charge (0-100) + 5-blade fan at 50+ |
| 2 | Midnight's Crescendo | Melee | Crescendo Stacks (max 15) + crescendo waves at 8+ |
| 3 | Twilight Severance | Melee | Twilight Charge + blade waves + Dimension Sever |
| 4 | Constellation Piercer | Ranged | Constellation-themed piercing shots |
| 5 | Nebula's Whisper | Ranged | Nebula-whisper projectiles with smoke trails |
| 6 | Serenade of Distant Stars | Ranged | Star-serenade long-range homing shots |
| 7 | Starweaver's Grimoire | Magic | Starlight weaving spells |
| 8 | Requiem of the Cosmos | Magic | Cosmic requiem channeled magic |
| 9 | Celestial Chorus Baton | Summon | Conductor baton with celestial chorus minions |
| 10 | Galactic Overture | Summon | Galactic-themed overture minions |
| 11 | Conductor of Constellations | Summon | Constellation orchestra conductor |

---

## 1. Nocturnal Executioner (Melee)

### Identity & Musical Soul
The night's executioner — a cosmic greatsword that channels the finality of midnight. Devastating 3-phase combo with an Execution Charge system that rewards patient buildup. At high charge, the weapon unleashes a devastating fan of blades that cut through the night sky like falling stars. The heaviest, most impactful weapon in Nachtmusik.

### Lore Line
*"At midnight, the executioner does not knock. The stars simply go dark."*

### Combat Mechanics (Existing: 1850 damage, extends MeleeSwingItemBase)
- **3-Phase Cosmic Combo**:
  - **Phase 1**: Heavy horizontal sweep with cosmic trailing energy. Spawns NocturnalBladeProjectile (spectral blade forward). Screen shake on impact.
  - **Phase 2**: Rising uppercut swing. Two NocturnalBladeProjectiles in a V-pattern. Cosmic dust cloud on hit.
  - **Phase 3**: Devastating overhead slam. Three blades in a fan + ground impact shockwave (expanding cosmic ring).
- **Execution Charge System** (0-100):
  - Builds through combat (+5 per swing, +10 per hit, +15 per kill)
  - Decays at -2/s when not swinging
  - At 50+ Charge: Right-click fires 5 NocturnalBladeProjectiles in a fan (2.5x base damage per blade). Screen shake + brief screen darkening.
  - At 100 Charge: Right-click fires 5 blades at 3.5x damage + all blades home briefly + massive cosmic explosion at convergence point. Charge resets to 0.
- **Cosmic Presence**: At high charge, the player pulses with indigo-cosmic energy. Orbiting purple-gold constellation particles. Enemies within 8 tiles take passive aura damage (subtle).

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `NocturnalSwingTrail.fx` | Heavy greatsword swing trail | Wide, imposing trail strip. Night void center → deep indigo mid → cosmic blue → starlight silver edge. Internal nebula noise scroll (cosmic cloud pattern). Star point sparkles embedded. Each phase's trail has increasing width and brightness. |
| `NocturnalBladeProjectile.fx` | Spectral blade projectile body | Ghost blade silhouette — indigo body with silver-starlight edge glow. Internal cosmic energy scroll. Afterimage trail (3 fading copies). At high charge levels: blade gains constellation–line pattern inside. |
| `ExecutionFanRelease.fx` | 5-blade fan special attack | Brief screen darkening effect (0.2s dim) + 5 blades fanning outward with converging trails. At 100 charge: convergence point creates gravitational lens distortion before cosmic explosion. |
| `CosmicPresenceAura.fx` | Charge-based aura around player | SDF circle aura with internal cosmic cloud. Intensity scales linearly with charge (invisible at 0, vibrant at 100). Deep indigo → cosmic blue. Orbiting constellation star particles at edge. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CosmicDustTrailParticle | Sheds from swing trail and blade projectiles | Cosmic blue-silver dust motes, slow drift, 12 frame life |
| ExecutionChargeOrbParticle | Orbits player proportional to charge level | Indigo-gold constellation dots, orbit radius shrinks at higher charge, continuous |
| ImpactShockwaveParticle | Expanding cosmic ring at ground impacts | Indigo-silver ring expanding, thin, 10 frame burst |
| ExecutionConvergenceParticle | Spirals into convergence point at 100 charge | Starlight particles spiraling inward, 15 frame life |

#### Bloom Layers
1. **Blade edge**: Starlight silver glow along cutting edge
2. **Swing trail**: Deep indigo-blue ambient glow behind trail
3. **Execution fan**: Intense white-silver bloom at each blade during fan attack
4. **Convergence point**: 4-layer bloom (white → silver → cosmic blue → indigo) at 100-charge convergence

---

## 2. Midnight's Crescendo (Melee)

### Identity & Musical Soul
A crescendo building from midnight's silence — rapid combo strikes that build stacking momentum. Each hit adds to the crescendo, and the more stacks accumulated, the more devastating each subsequent hit becomes. At 8+ stacks, the weapon begins releasing crescendo waves — visible sound energy that extends the weapon's reach. The rhythm of the night, building ever louder.

### Lore Line
*"The night starts quiet. It does not end that way."*

### Combat Mechanics (Existing: extends MeleeSwingItemBase)
- **Rapid 3-Phase Combo**: Fast alternating slashes (left-right-overhead). Each phase faster and tighter than Nocturnal Executioner. Prioritizes speed over weight.
- **Crescendo Stack System** (max 15):
  - Each hit adds 1 stack (+12% damage, +2% crit per stack)
  - Stacks decay after 1.5 seconds without hitting
  - At 5 stacks: Swing trail becomes more vibrant, sparkle density increases
  - At 8+ stacks: Each swing releases a crescendo wave — expanding arc projectile that extends reach by 8 tiles
  - At 15 stacks: Maximum Crescendo — waves deal double damage, trail is massive and brilliant
- **Inflicts Celestial Harmony**: On hit, applies Celestial Harmony debuff (shared Nachtmusik debuff — enemies take +10% damage from all Nachtmusik weapons).
- **Momentum Preservation**: If you maintain stacks above 10 for 5+ seconds, the decay timer extends to 3 seconds (easier to maintain peak stacks during boss fights).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CrescendoSwingTrail.fx` | Rapid swing trail that scales with stacks | Thin-to-wide trail strip. Low stacks: subtle indigo with starlight edges. 8+: wider with cosmic blue body + internal UV-scroll wave pattern. 15: massive, brilliant, pulsing silver-white with cosmic streaks. Width parameterized by stack count. |
| `CrescendoWaveArc.fx` | Expanding crescendo wave projectile | Crescent arc shape — advancing forward like a sound wave. Color: cosmic blue → starlight silver. Internal standing-wave pattern (musical). Width and brightness tied to stack count. |
| `CrescendoStackIndicator.fx` | Stack count visual indicator around player | SDF ring around player with segmented fill (15 segments for 15 possible stacks, filling as stacks increase). Indigo → cosmic blue → silver as stacks increase. At 15: all segments glow brilliantly. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CrescendoStackSparkParticle | Burst with each new stack gained | Cosmic blue spark, small, 4 frame burst |
| CrescendoWaveEdgeParticle | Sheds from crescendo wave edges | Silver sparkle, perpendicular drift, 6 frame life |
| MaximumCrescendoAuraParticle | Intense aura at 15 stacks | Brilliant silver-white orbiting particles, 15 frame life |
| CelestialHarmonyDebuffParticle | Indicator on Celestial Harmony debuffed enemies | Small cosmic-blue star glyph orbiting enemy, 60 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Celestial Harmony | +10% damage from all Nachtmusik weapons. | 180 frames (3s), refreshes on hit |

---

## 3. Twilight Severance (Melee)

### Identity & Musical Soul
Twilight — the boundary between day and night, light and dark. This katana-style weapon cuts through that boundary with extreme speed. Ultra-fast 3-phase combo with a Twilight Charge system. Every third slash fires perpendicular blade waves, and at full charge a devastating Dimension Sever tears through reality. Speed and precision over brute force.

### Lore Line
*"Between light and dark, the blade finds every truth."*

### Combat Mechanics (Existing: 1450 damage, 25% crit, extends MeleeSwingItemBase)
- **Ultra-Fast 3-Phase Combo**: Fastest melee in Nachtmusik. Quick diagonal → reverse diagonal → horizontal. Minimal windup, maximum speed.
- **Twilight Charge System** (0-100):
  - Builds on swing (+5 per swing)
  - Decays when idle (-3/s)
  - Every 3rd slash fires perpendicular blade waves — crescent projectiles that travel at 90° to swing direction
  - Right-click at full charge (100): **Dimension Sever** — massive cross-slash that fires a 5-blade fan at 3x damage. Enemies hit are marked with Dimensional Rift (continuous damage from the rift in their body, 3s).
- **Inflicts Celestial Harmony**: Shares the Nachtmusik theme debuff.
- **Twilight Shift**: At 50+ charge, the player's movement speed is increased by 15%. Encourages aggressive, mobile play.
- **Blade Wave Combo**: If perpendicular blade waves from consecutive swings cross paths, their intersection creates a Twilight Cross — small AoE burst at intersection point.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `TwilightSwingTrail.fx` | Fast, sharp katana swing trail | Thin, razor-sharp trail strip — NOT wide and heavy. Clean edge (no feathered soft edge — hard cutoff). Deep indigo core → bright starlight silver edge. The speed should be implied by how thin and sharp the trail is. |
| `BladeWaveProjectile.fx` | Perpendicular crescent blade wave | Crescent arc projectile — thin, sharp, fast. Starlight silver body → cosmic blue edge glow. Very clean lines — this is precision, not power. Brief trail behind. |
| `DimensionSeverSlash.fx` | Cross-slash screen effect for Dimension Sever | Screen-space effect: two crossing slash lines (X pattern) with slow-healing seam (indigo rift visible in the slash lines for 0.5s). Chromatic aberration along both lines. Brief screen shake. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| TwilightSpeedSparkParticle | Brief sparks from fast katana swings | Silver sparks, very brief (3 frame), directional |
| BladeWaveTrailParticle | Thin trail behind blade waves | Cosmic blue streak, 4 frame life |
| TwilightCrossBurstParticle | Burst at Twilight Cross intersection | Silver-blue flash ring, 6 frame burst |
| DimensionalRiftParticle | Sheds from Dimensional Rift debuffed enemies | Indigo-black rift sparks, slow, 15 frame life |
| TwilightShiftSpeedParticle | Speed lines around player at 50+ charge | Silver streak lines trailing from player, 8 frame life |

---

## 4. Constellation Piercer (Ranged)

### Identity & Musical Soul
A piercing shot that travels through enemies like light connecting stars in a constellation. Each hit point becomes a "star" and after enough stars, the constellation pattern forms between them — a connected geometric shape that deals bonus damage to everything within it.

### Lore Line
*"Each star is an enemy. Each line of light between them is a death sentence."*

### Combat Mechanics
- **Piercing Constellation Shot**: Primary fire — fast piercing projectile that passes through multiple enemies. Each enemy pierced becomes a "Star Point" on the battlefield (marked visually).
- **Constellation Formation**: After 3+ Star Points are created from a single shot, they automatically connect with constellation lines. Enemies within the formed constellation polygon take continuous AoE damage for 3s.
- **Star Chain**: Alt fire — fires a Star Chain shot that connects to existing Star Points, extending the constellation. Allows building complex shapes over multiple shots.
- **Zodiac Patterns**: If a constellation forms specific patterns (triangle = Aries, square = Libra, pentagon = Aquarius), bonus effects trigger:
  - Triangle: Damage burst
  - Square: Enemy slowing field
  - Pentagon: Player shield (absorbs 50 damage)
- **Cosmic Scope**: While aiming (holding fire before release), a starfield scope overlay appears showing Star Points and potential connections. Rewards thoughtful aim.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ConstellationPiercerTrail.fx` | Piercing shot trail connecting stars | Thin bright trail strip — starlight silver core with cosmic blue glow. Trail persists longer than most (2s) to form constellation lines. Clean, geometric, precise. |
| `ConstellationLine.fx` | Lines connecting Star Points | Beam strip between positions. Starlight silver with pulsing intensity. Internal constellation-glyph texture. Brightest at Star Points, dimming at midpoints. |
| `ConstellationFieldEffect.fx` | AoE damage field within constellation polygon | Alpha-blended fill of the polygon area. Deep indigo, semi-transparent, with embedded star sparkles. Internal cosmic noise scroll (subtle). Edge brightness along constellation lines. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| StarPointCreateParticle | Flash when a new Star Point is created | Bright silver flash expanding ring, 5 frame burst |
| ConstellationLinePulseParticle | Travels along constellation lines | Silver sparkle mote, moves between Star Points, 10 frame life |
| ConstellationFieldMoteParticle | Ambient particles inside constellation area | Cosmic dust, slow drift, 20 frame life |
| ZodiacBonusFlashParticle | Flash when Zodiac pattern triggers | Pattern-specific colored burst + glyph shape, 8 frame burst |

---

## 5. Nebula's Whisper (Ranged)

### Identity & Musical Soul
Nebulae whisper in light that takes millennia to arrive — soft, diffuse, overwhelming in aggregate. This weapon fires nebula-cloud projectiles that expand as they travel, starting focused and growing into massive AoE clouds. Whispered attacks that become devastating.

### Lore Line
*"The nebula does not shout. It barely breathes. But entire stars are born in its exhale."*

### Combat Mechanics
- **Nebula Whisper Shot**: Primary fire — starts as a tight projectile. As it travels, it expands into a wider and wider cloud. At close range: focused damage. At max range: massive AoE but reduced per-target damage.
- **Nebula Residue**: Where the shot travels, it leaves Nebula Residue — a lingering cloud trail (2s). Enemies in residue take minor DoT + movement slow.
- **Accumulation**: Firing multiple shots through the same area layers Nebula Residue, increasing its density. Dense residue (3+ layers): deals significantly more damage + applies Cosmic Confusion (random enemy movement direction for 1s).
- **Whisper Storm**: After 5 consecutive shots, alt fire creates a Whisper Storm — all existing Nebula Residue converges on cursor position, creating a massive concentrated nebula cloud. Cloud persists 4s as a devastating damage zone.
- **Silent Approach**: Shots travel through walls for the first 3 tiles (phasing). Allows shooting through cover.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `NebulaClouldExpanding.fx` | Expanding nebula projectile body | SDF circle that grows in radius over time. Internal nebula noise scroll (deep indigo + cosmic blue + traces of purple). Soft edges (heavy smoothstep falloff). More detail as it expands. |
| `NebulaResidueField.fx` | Lingering nebula cloud trail/convergence | Semi-transparent nebula cloud overlay. Color: deep indigo → cosmic blue → traces of silver sparkle. Slow internal drift. Density parameter controls opacity and damage-indicating brightness. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| NebulaDustParticle | Sheds from expanding nebula shot | Cosmic blue motes, slow outward drift, 15 frame life |
| ResidueSparkleParticle | Ambient sparkle in residue fields | Starlight silver twinkle, slow, 20 frame life |
| WhisperStormConvergeParticle | Residue particles moving toward convergence point | Cosmic dust streaming inward, 10 frame life |
| CosmicConfusionSpiralParticle | Spiraling effect on confused enemies | Indigo spiral around enemy head, 3 rotations, 30 frame life |

---

## 6. Serenade of Distant Stars (Ranged)

### Identity & Musical Soul
A serenade — a song of devotion played at distance. This weapon fires homing star projectiles that travel impossible distances, seeking targets with the patience of starlight crossing the void. Long-range, gentle, but inexorable. The romantic aspect of Nachtmusik — a love letter written in light.

### Lore Line
*"The light left a star ages ago, just to find you. And it never missed."*

### Combat Mechanics
- **Distant Star Shot**: Primary fire — homing star projectile with extreme range (80 tiles) and moderate homing strength. Each star glows with starlight silver and leaves a constellation trail.
- **Star Memory**: Stars remember every enemy they pass near (within 5 tiles). After hitting or reaching max range, the star replays its memory — firing small Star Echo projectiles back at every enemy it passed. Rewards shooting through crowds.
- **Serenade Rhythm**: Firing at a consistent rhythm (every 1.0s ± 0.2s) builds Serenade stacks (max 5). Each stack: +10% homing strength + stars glow brighter. At 5 stacks: stars become irresistible (perfect homing, always hit).
- **Distant Connection**: If two stars are airborne simultaneously and pass within 3 tiles of each other, they form a Connection — a brief energy line between them that damages everything it crosses.
- **Starlight Sonata**: Killing an enemy with a perfect-homing star (5 Serenade stacks) creates a Starlight Sonata — that enemy's position becomes a mini-star that fires 4 Star Echoes at nearby enemies (cascade kill potential).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `DistantStarProjectile.fx` | Glowing star projectile body | SDF 4-point star. Starlight silver core → cosmic blue edge glow. Internal twinkle effect (brightness oscillation). At high Serenade stacks: star gains corona rays (extending light beams from points). |
| `StarConnectionLine.fx` | Energy line between two passing stars | Thin beam strip between star positions. Starlight silver with pulsing brightness. Brief (0.3s) but visually linking the stars. Curved slightly (not straight — more elegant). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| DistantStarTrailParticle | Constellation trail behind star | Silver dot trail (like connecting dots), regularly spaced, 30 frame life |
| StarEchoProjectileParticle | Small returning echo projectile trail | Smaller cosmic blue trail, 6 frame life |
| SerenaadeRhythmGlintParticle | Glint on weapon at each rhythmic shot | Silver glint, 3 frame burst |
| StarlightSonataNovaParticle | Nova burst from cascade kill | Silver-blue expanding ring + 4 directional star shapes, 10 frame burst |

---

## 7. Starweaver's Grimoire (Magic)

### Identity & Musical Soul
A grimoire that weaves starlight into spell patterns. Each cast creates woven light structures — nets, webs, and tapestries of starlight that trap and damage enemies. The weaving motif: spells create geometric patterns that interconnect and strengthen each other.

### Lore Line
*"The stars are threads. The night is the loom. And this book knows every pattern."*

### Combat Mechanics
- **Starweave Bolt**: Primary fire — fires a starlight bolt that creates a brief Weave Node at impact (lingers 5s). Subsequent bolts that pass within 3 tiles of a Weave Node automatically connect to it with a Weave Thread (damage line).
- **Weave Network**: Multiple Weave Nodes connected by threads form a Starweave Network. Enemies touching threads take damage. More connections = stronger threads. A node connected to 3+ other nodes becomes a Nexus Node (brightens, deals AoE).
- **Pattern Casting**: Alt fire cycles through special patterns:
  - **Star Web**: Places 5 nodes in a pentagonal pattern simultaneously (trap setup)
  - **Starlight Net**: Fires a net projectile — on hit, creates a small node cluster around the target (binding)
  - **Celestial Tapestry**: Massive pattern — 12 nodes in a grid. Requires 3s channel.
- **Thread Resonance**: When enemies are damaged by threads, the thread vibrates — nearby threads also vibrate (chain reaction). Dense networks amplify damage through resonance.
- **Unravel**: If an enemy destroys a Weave Node (by dying on it), the destruction unravels connected threads — brief burst of energy along each thread (bonus damage).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WeaveNodeGlow.fx` | Starlight node body | SDF circle — starlight silver glow. Nexus nodes: brighter + corona effect. Connected nodes: faint radial lines extending toward connections. 5s lifespan with slow brightness decay near end. |
| `WeaveThreadLine.fx` | Connecting thread between nodes | Thin beam strip between positions. Starlight silver with internal pulse (energy traveling along thread). Thickness increases with more connections. Vibration: brief lateral offset wave when activated. |
| `CelestialTapestryGrid.fx` | Large grid pattern special cast | Grid of connected nodes — rendered efficiently as a single mesh. Each grid cell has internal starlight fill. The full tapestry is a cosmic grid of light spanning the battlefield. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| WeaveNodeSpawnParticle | Flash at node creation | Silver sparkle ring, 5 frame burst |
| WeaveThreadSparkParticle | Travels along threads (showing energy flow) | Silver dot, moves between connected nodes, 8 frame life |
| NexusNodeRadianceParticle | Ambient glow at Nexus Nodes | Brighter silver sparkles, 15 frame life |
| UnravelBurstParticle | Burst along threads when unraveling | Silver energy burst traveling along each thread path, 10 frame life |

---

## 8. Requiem of the Cosmos (Magic)

### Identity & Musical Soul
A requiem for the cosmos itself — the grandest, most devastating magic weapon in Nachtmusik. Channels the raw power of dying stars and cosmic events. Massive projectiles, screen-affecting effects, and the sense that you are channeling forces too vast for mortal comprehension.

### Lore Line
*"The cosmos sings its own requiem. You merely conduct the final movement."*

### Combat Mechanics
- **Cosmic Burst**: Primary fire — fires a large cosmic orb that detonates on impact into a supernova-style explosion (6 tile radius). Slow but devastating.
- **Stellar Collapse**: Charged fire (hold 2s) — fires a Stellar Collapse orb. On impact, creates a temporary gravitational singularity — pulls all enemies within 10 tiles toward center for 2s, then explodes outward.
- **Cosmic Event Cycle**: Weapon cycles through "cosmic events" every 10 casts:
  - Casts 1-3: Normal cosmic bursts (standard)
  - Casts 4-6: Cosmic bursts leave persistent nebula fields (lingering damage)
  - Casts 7-9: Cosmic bursts spawn small orbiting starlets at impact (mini-damage satellites)
  - Cast 10: **Event Horizon** — automatic charged Stellar Collapse with double radius + screen distortion
- **Cosmic Awareness**: While equipped, enemies on screen have their damage resistances weakened by 5% (passive). The cosmos reveals all weaknesses.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CosmicBurstOrb.fx` | Large cosmic orb projectile | SDF sphere with internal nebula cloud scroll. Deep indigo → cosmic blue → starlight silver highlights. Large (2x normal projectile size). Intense glow corona. Pulsing. |
| `StellarCollapseEffect.fx` | Gravitational singularity + explosion | Two-phase shader: Phase 1 (pull): screen-space gravitational lens (pixels pulled inward to center point). Phase 2 (explode): expanding cosmic ring with nebula debris and star sparks. Chromatic aberration during pull phase. |
| `EventHorizonScreen.fx` | Screen-wide effect for 10th cast Event Horizon | Screen overlay: brief cosmic aura at screen edges. Stars visible in background. Deep indigo vignette. Brief gravitational pulse (screen wobble). Most dramatic single-cast in Nachtmusik's magic arsenal. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CosmicBurstDebrisParticle | Radial burst from orb detonation | Cosmic blue-silver debris, 20+ particles, 15 frame life |
| StellarCollapseInwardParticle | Pull toward singularity during pull phase | Starlight motes spiraling inward, 20 frame life |
| NebulaFieldPersistentParticle | Lingers in nebula field AoE | Indigo-blue motes, slow drift, 60 frame life |
| OrbitingStarletParticle | Orbits impact point (mini-satellites) | Tiny silver stars, tight orbit, 120 frame life (persistent mini-damage) |
| EventHorizonPulseParticle | Massive pulse at Event Horizon trigger | Expanding cosmic ring, screen-wide, 15 frame life |

---

## 9. Celestial Chorus Baton (Summon)

### Identity & Musical Soul
A conductor's baton that summons a celestial chorus — a group of starlight entities that sing in cosmic harmony. The chorus attacks through sound-waves of celestial energy, and the conductor (player) can direct them with baton gestures (cursor movement). A conducting fantasy made real.

### Lore Line
*"The baton asks no permission. The stars obey or cease to shine."*

### Combat Mechanics
- **Celestial Chorus Minions**: Summons 3 starlight singer entities that hover in formation. They autonomously fire cosmic sound wave projectiles at enemies.
- **Conductor's Direction**: Moving the cursor rapidly in a direction commands all chorus members to focus fire in that direction. Focused fire from all 3 = 1.5x damage.
- **Harmonic Phase**: Every 15s, the chorus enters Harmonic Phase — all 3 singers synchronize, firing a combined beam for 3s (channeled multi-source beam converging on target).
- **Additional Summons**: More summon slots = more singers (up to 6 total). At 6 singers, Harmonic Phase becomes Grand Harmonic — beam is twice as wide and deals 2x damage.
- **Celestial Shield**: When not attacking, chorus members orbit the player in a protective formation. Their starlight presence provides +5 defense and +3% damage reduction per singer.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CelestialSingerBody.fx` | Starlight singer entity | Ethereal humanoid silhouette made of starlight. Silver-cosmic blue body. Internal star-sparkle pattern. During Harmonic Phase: body brightens, connecting beams between singers visible. |
| `CelestialSoundWave.fx` | Sound wave attack projectile | Expanding arc (sound wave shape). Starlight silver → cosmic blue. Internal standing wave pattern. Musical note shapes embedded at wave nodes. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SingerAuraParticle | Ambient starlight around each singer | Silver sparkles, slow orbit, 20 frame life |
| SoundWaveEdgeParticle | Sheds from sound wave edges | Cosmic blue sparks, 5 frame life |
| HarmonicBeamSparkParticle | Sheds from combined beam during Harmonic Phase | Bright silver sparks, perpendicular, 8 frame life |
| ShieldFormationGlintParticle | Sparkles at orbital positions during shield formation | Silver glint, 10 frame life |

---

## 10. Galactic Overture (Summon)

### Identity & Musical Soul
An overture is the opening piece — the grand introduction. This summon weapon conjures the grandest introduction to night: a miniature galaxy that orbits the player, launching stellar projectiles. The galaxy grows more complex and powerful over time, mirroring how an overture builds from a simple theme to full orchestral complexity.

### Lore Line
*"Before the symphony begins, the galaxy must announce its arrival."*

### Combat Mechanics
- **Galaxy Minion**: Summons a small rotating galaxy near the player. The galaxy fires stellar projectiles (starlight bullets) at enemies autonomously.
- **Galaxy Evolution**: Over time (every 20s), the galaxy evolves:
  - **Simple Galaxy** (0-20s): Fires single stellar projectiles. Sparse, dim.
  - **Spiral Galaxy** (20-40s): Fires in spiral patterns (2 projectiles per volley). Visible spiral arms.
  - **Barred Galaxy** (40-60s): Fires from bar endpoints (3 projectiles per volley). Bar structure visible. +30% damage.
  - **Active Galaxy** (60s+): Fires rapid barrages + occasional cosmic jet (beam from center). +60% damage. Full visual complexity — bright, dynamic, constantly firing.
- **Galaxy Collision**: If 2+ galaxies are summoned, they occasionally "collide" (pass through each other) — collision creates a massive projectile burst (15 stellar projectiles in all directions).
- **Cosmic Background**: While the galaxy minion is active, the background behind the player subtly shifts to show cosmic elements (stars, nebulae). Cosmetic.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `GalaxyMinionBody.fx` | Rotating galaxy entity body | Flat disc with spiral arm pattern UV-scrolled (rotation). Different parameters per evolution: Simple = dim, 2 arms. Spiral = moderate, 2 arms + detail. Barred = bright, bar + arms. Active = brilliant, complex structure + jets. Internal cosmic noise for nebula fill. |
| `CosmicJetBeam.fx` | Beam projected from Active Galaxy center | Thin beam from galaxy center to target. Cosmic blue → starlight silver. Internal energy scroll. Brief duration (1s per jet). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| StellarProjectileTrailParticle | Trail behind stellar projectiles | Silver streak, 5 frame life |
| GalaxyEvolutionFlashParticle | Flash when galaxy evolves to next stage | Cosmic blue expanding ring, 8 frame burst |
| GalaxyCollisionBurstParticle | Burst during galaxy collision event | Multi-color cosmic debris, 20+ particles, 12 frame life |
| GalaxySpiralArmParticle | Ambient particles tracing spiral arms | Cosmic blue motes following spiral path, continuous |

---

## 11. Conductor of Constellations (Summon)

### Identity & Musical Soul
The ultimate Nachtmusik summoner — you become the conductor of the entire night sky. This weapon summons a constellation orchestra: multiple cosmic entities, each playing a different role (violin = beam attacks, percussion = AoE slams, winds = sweeping waves). The conductor (player) orchestrates their movements with cursor.

### Lore Line
*"All the stars are instruments. The night sky is the concert hall. You are the conductor."*

### Combat Mechanics
- **Constellation Orchestra**: Summons a constellation entity (type cycles with each summon):
  - **Strings** (1st summon): Fires thin beam attacks (sustained, sweeping)
  - **Percussion** (2nd): Ground slam AoE attacks (heavy, rhythmic)
  - **Winds** (3rd): Wide wave attacks (sweeping, covering large arc)
  - **Brass** (4th+): Heavy burst projectiles (powerful single shots)
- **Orchestral Sync**: When all 4 instrument types are present, they can perform Orchestral Sync — all attack the same target simultaneously with enhanced attacks. Massive combined damage.
- **Conductor's Cue**: Right-click marks an enemy as the "soloist target." All minions focus this target with enhanced damage (+30%) until the target dies or a new cue is given.
- **Finale Performance**: After 90 seconds of all 4 types being active, they can perform a Finale — 5-second burst where all minions attack at double speed with enhanced effects. 60s cooldown after.
- **Standing Formation**: When idle, constellation entities arrange themselves in recognizable constellation patterns (visual — Orion, Ursa Major, etc.). Cosmetic but beautiful.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ConstellationEntityBody.fx` | Constellation entity body (parameterized per instrument) | Star-cluster body — multiple star points connected by constellation lines. Strings: linear arrangement. Percussion: compact cluster. Winds: spread arrangement. Brass: triangular formation. Each has unique silver-cosmic blue palette variation. |
| `OrchestraSyncExplosion.fx` | Combined orchestral sync attack | Multi-layered attack: beam (from strings) + AoE ring (from percussion) + wave arc (from winds) + heavy burst (from brass) all simultaneously targeting one point. Layered rings in different Nachtmusik sub-colors. |
| `FinalePerformanceAura.fx` | Screen-wide aura during Finale | Brief cosmic overlay — starfield visible, all constellation entities glow brilliantly. Musical note shapes scattered. Grand, awe-inspiring screen effect. 5s duration. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| StringsBeamParticle | Thin line from strings entity to target | Silver beam, sustained, 3 frame life per segment |
| PercussionImpactParticle | Ground slam shockwave | Cosmic blue ring expanding from slam, 10 frame |
| WindsSweepParticle | Wide particles from wind wave | Silver arc particles, sweeping, 8 frame life |
| BrassBurstParticle | Burst from brass heavy shot | Indigo-gold burst, 12 frame life |
| ConductorCueGlyphParticle | Indicator on cue-targeted enemy | Music conductor baton glyph, gold, stationary on target |

---

## Cross-Theme Synergy Notes

### Nachtmusik Theme Unity
All weapons share deep indigo + cosmic blue + starlight silver palette with nocturnal/stellar motifs:
- **Melee trio**: Power spectrum — heavy executioner → momentum crescendo → speed katana
- **Ranged trio**: Space spectrum — constellation geometry → nebula clouds → distant homing stars
- **Magic duo**: Scale spectrum — networked weaving light → cosmic devastation
- **Summon trio**: Orchestra spectrum — chorus singers → evolving galaxy → full constellation orchestra

### Shared Debuff: Celestial Harmony
Multiple melee weapons inflict Celestial Harmony (+10% damage from all Nachtmusik weapons). Creates cross-weapon synergy encouraging mixed Nachtmusik loadouts.

### Visual Distinction
Despite shared palette:
- **Executioner**: Heavy, wide trails with screen darkening — weight and gravity
- **Crescendo**: Scaling intensity visuals — thin at start, brilliant at max
- **Severance**: Sharp, thin, precise trails — speed and clean cuts
- **Constellation Piercer**: Geometric, connected patterns — nodes and lines
- **Nebula's Whisper**: Soft, diffuse, cloudy — expanding and enveloping
- **Serenade**: Individual bright stars against dark sky — pinpoints of light
- **Starweaver**: Woven nets and webs — intricate interconnection
- **Requiem**: Massive cosmic events — supernovae, singularities
- **Summons**: Each has distinct entity shapes — singers, galaxies, constellations

### Musical Motifs
- **Serenade/nocturne**: Nachtmusik IS night music — weapons should feel like the night sky. Stars, constellations, nebulae, galaxies.
- **Rhythmic play**: Crescendo stacking, Serenade rhythm, Orchestral Sync — many weapons reward rhythmic, musical play patterns
- **Building complexity**: Galaxy evolution, Weave networks, constellation formation — weapons grow more complex the longer you use them
- **Cosmic scale**: Nachtmusik's magic weapons operate at cosmic scale — singularities, supernovae, event horizons. The night sky is VAST, and these weapons channel that vastness
