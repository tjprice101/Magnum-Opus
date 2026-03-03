# 🔔 La Campanella — Resonance Weapons Planning

> *"The ringing bell, virtuosic fire — passion forged in black smoke and orange flame."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Liszt's La Campanella — virtuosic bell etude, fiery and relentless |
| **Emotional Core** | Passion, intensity, burning brilliance |
| **Color Palette** | Black smoke, orange flames, gold highlights |
| **Palette Hex** | Smoky Black `(30, 20, 15)` → Deep Ember `(160, 60, 10)` → Infernal Orange `(255, 140, 40)` → Bell Gold `(255, 210, 80)` → Chime White `(255, 245, 220)` |
| **Lore Color** | `new Color(255, 140, 40)` — Infernal Orange |
| **Lore Keywords** | Bells, chimes, fire, passion, virtuosity, ringing, resonance |
| **VFX Language** | Bell chime ripples, black smoke billowing, orange-gold flame trails, bell-shaped shockwaves, ringing resonance waves, fire virtuosity |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Dual Fated Chime | Melee | Twin chime inferno waltz with bell flame waves |
| 2 | Ignition of the Bell | Melee | Thrust-based with infernal geysers and chime cyclone |
| 3 | Symphonic Bellfire Annihilator | Ranged | Heavy launcher with crescendo waves and bellfire rockets |
| 4 | Piercing Bell's Resonance | Ranged | Precision weapon with staccato bullets and resonant detonation |
| 5 | Grandiose Chime | Ranged | Beam + note mines + kill echo chains |
| 6 | Fang of the Infinite Bell | Magic | Infinite bell orbs with stacking damage buffs |
| 7 | Infernal Chimes' Calling | Summon | Campanella Choir minion with shockwave attacks |

---

## 1. Dual Fated Chime (Melee)

### Identity & Musical Soul
The Dual Fated Chime is the **opening bell-strike of La Campanella** — twin blades that ring like bells with every clash. Each swing should produce a visible shockwave ring emanating from the point of impact, like sound waves from a struck bell. The weapon alternates between the two chimes in an inferno waltz — left-right-left-right in escalating intensity, trailing black smoke and orange fire.

### Lore Line
*"Two bells toll as one — their song turns steel to cinder."*

### Combat Mechanics
- **Inferno Waltz Combo**: 5-phase alternating left/right combo:
  - **Toll 1 — Opening Peal**: Right chime horizontal slash. Bell shockwave ring on contact.
  - **Toll 2 — Answer**: Left chime diagonal slash. Faster.
  - **Toll 3 — Escalation**: Right chime upward arc + flame wave projectile.
  - **Toll 4 — Resonance**: Left chime downward slam. Double shockwave ring + ground fire.
  - **Toll 5 — Grand Toll**: Both chimes cross-slash. BellFlameWaveProj in full circle (12 directional waves). Massive bell chime SFX.
- **Bell Resonance Stacking**: Each hit on the same target within 3s adds a Resonance Ring (visual ring around enemy, max 5). At 5 rings, next hit triggers Bell Shatter — massive damage burst + all rings detonate as AoE waves.
- **Flame Waltz Dodge**: During Phase 2 or 4, the player sways slightly in swing direction (small dash-dodge). Provides 0.2s iframes.

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ChimeShockwave.fx` | Bell strike shockwave rings | Expanding SDF ring with thickness falloff. Color: white-hot center → orange → black smoke edge. Ring thickness decreases as radius grows. Multiple rings at staggered timings for resonance echo. |
| `InfernoWaltzTrail.fx` | Flame trail behind chime swings | UV-scrolled fire texture with FBM noise distortion. Color ramp: black smoke edge → deep ember → bright orange → gold. Tip-to-base gradient via UV.x. |
| `BellFlameWave.fx` | Directional flame wave projectile | Thin flame strip expanding outward. Internal noise scroll for fire turbulence. Orange-gold gradient with black smoke edges. |
| `BellResonanceRing.fx` | Resonance stacking rings on target | Concentric SDF rings around target center. Each ring pulses at slightly different frequency (harmonics). Gold → orange gradient. Additive. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| BellChimeParticle | Expands outward from impact as visible sound wave | Thin gold ring (SDF rendered or animated sprite), expanding, 15 frame life |
| CampanellaEmberParticle | Rises from flame trails with rotation | Orange-gold embers, 3-5px, flickering brightness, 20-35 frame life |
| BlackSmokeParticle | Billows from swing path, slow drift upward | Dark grey-black smoke wisps, large (10-20px), slow movement, 40-60 frame life |
| BellShatterParticle | Radial burst from Bell Shatter detonation | Gold metallic shards + orange fire sparks, 15-25 particles, decelerating |
| InfernoWaltzSparkParticle | Sheds from chime blades during swing | Small bright orange sparks, fast, short trail, 10 frame life |

#### Bloom Layers
1. **Chime glow**: Persistent orange-gold bloom along each blade (pulsing with swing rhythm)
2. **Shockwave ring**: Expanding ring bloom (white-hot center, additive)
3. **Impact burst**: 3-layer (white core + orange mid + smoky outer ring)
4. **Flame wave**: Orange-gold glow trailing flame wave projectiles
5. **Grand Toll**: Triple stacked shockwave rings + full-screen orange vignette flash

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Fire trail texture | `Assets/LaCampanella/DualFatedChime/Trails/InfernoWaltz.png` | "Horizontal flame trail with billowing black smoke edges and bright orange-gold fire center, intense fire energy, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Bell shockwave ring | `Assets/LaCampanella/DualFatedChime/Flare/BellShockwave.png` | "Thin expanding circular shockwave ring, gold-white energy with subtle bell shape at peak, on solid black background, 256x256px --ar 1:1 --style raw" |
| Resonance ring indicator | `Assets/LaCampanella/DualFatedChime/Orbs/ResonanceRing.png` | "Concentric thin golden rings (3-5 nested), bell-themed energy, on solid black background, 128x128px --ar 1:1 --style raw" |
| Bell shatter texture | `Assets/LaCampanella/DualFatedChime/ImpactSlash/BellShatter.png` | "Radial explosion of golden metallic bell fragments with orange fire, on solid black background, 256x256px --ar 1:1 --style raw" |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| BellResonance | Stacking debuff (1-5 rings), visual indicator. At 5, next hit triggers Bell Shatter. | 180 frames per stack |
| InfernalBurn | DoT (fire damage) | 120 frames |

---

## 2. Ignition of the Bell (Melee)

### Identity & Musical Soul
Ignition of the Bell is La Campanella's **percussive power** — where Dual Fated Chime slashes, Ignition *thrusts and slams*. This is the anvil-strike, the hammer-on-bell. A thrust-based weapon that drives the point home with infernal geysers erupting from the ground and chime cyclone vortexes that pull enemies in. Where the Chime dances, Ignition **overwhelms**.

### Lore Line
*"The first spark was all it took. The bell has been burning ever since."*

### Combat Mechanics
- **Bell Thrust Combo**: 3-phase thrust combo:
  - **Phase 1 — Ignition Strike**: Forward thrust + ground geyser pillar at impact point.
  - **Phase 2 — Tolling Frenzy**: Rapid triple thrust (left-center-right). Each thrust spawns a smaller geyser.
  - **Phase 3 — Chime Cyclone**: Spin attack creating a fire cyclone vortex that pulls enemies inward for 2 seconds, then detonates.
- **Infernal Geyser**: Ground-targeted fire pillars that erupt from below. Enemies standing on the geyser point take initial hit + lingering fire damage.
- **Chimequake**: Every 3rd Phase 3 Cyclone detonation triggers a Chimequake — the ground cracks with fire in a large area, dealing persistent damage for 3 seconds.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `InfernalGeyser.fx` | Ground fire pillar eruption | Vertical beam shader with FBM noise for fire edge. Color: black base → deep red → bright orange → gold tips. UV.y scrolls upward (rising fire). Width pulses then narrows (eruption → dissipation). |
| `ChimeCyclone.fx` | Fire tornado vortex | Radial scrolling UV with spiral distortion. Concentric rings rotating at different speeds. Orange-gold inner → black smoke outer. Radial inward pull visualization. |
| `ChimequakeGround.fx` | Ground crack fire effect | Ground-plane projected shader. Voronoi noise for crack pattern. Cracks colored orange-gold (fire), surrounding area dark. Cracks widen then cool over 3s. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| GeyserFlameParticle | Erupts upward, decelerates, drifts to sides | Orange-gold flames, 5-10px, turbulent movement, 20-30 frame life |
| GeyserGroundSparkParticle | Radial burst at geyser base along ground | Orange sparks, flat trajectory, 4-6px, 12 frame life |
| CycloneDebrisParticle | Spiral inward toward cyclone center | Dark grey-orange debris chunks, orbiting, 30-40 frame life |
| ChimequakeCrackParticle | Rises from ground cracks as embers | Small orange-red embers, slow upward, 25 frame life |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Geyser fire texture | `Assets/LaCampanella/IgnitionOfTheBell/Beams/InfernalGeyser.png` | "Vertical fire geyser pillar, dark base erupting through orange to gold flame tips, intense volcanic fire, on solid black background, 64x256px --ar 1:4 --style raw" |
| Cyclone fire texture | `Assets/LaCampanella/IgnitionOfTheBell/Orbs/FireCyclone.png` | "Top-down fire tornado with spiral flame pattern, orange-gold center with black smoke outer, on solid black background, 256x256px --ar 1:1 --style raw" |
| Ground crack texture | `Assets/LaCampanella/IgnitionOfTheBell/Orbs/ChimequakeCracks.png` | "Top-down cracked ground with orange fire glowing through cracks, Voronoi cell pattern, dark stone with bright fire veins, on solid black background, 256x256px --ar 1:1 --style raw" |

---

## 3. Symphonic Bellfire Annihilator (Ranged)

### Identity & Musical Soul
The Annihilator is La Campanella's **fortissimo** — maximum volume, maximum power, maximum fire. A heavy launcher that fires bellfire rockets and crescendo waves. Every shot should feel like detonating a church bell filled with gunpowder. Massive, loud, devastating. This is the weapon for when subtlety has failed.

### Lore Line
*"When the bell toll becomes a bombardment, even silence trembles."*

### Combat Mechanics
- **Grand Crescendo Wave**: Primary fire — launches a slow-moving bell-shaped shockwave that expands as it travels. Pierces enemies but slows on each pierce. Empowered by Crescendo Buff.
- **Bellfire Rocket**: Alt fire — rapid bellfire rockets that arc slightly. On impact, create small fire patches (1.5s). 
- **Buff Stacking System**:
  - **Grand Crescendo Buff**: Stacks on enemy kills with Crescendo Waves (max 5). Each stack: +10% wave size, +8% damage.
  - **Bellfire Crescendo Buff**: Stacks on enemy kills with Bellfire Rockets (max 3). Each stack: rockets fire in bursts of 2, then 3, then 4.
- **Symphonic Overture**: When both buff stacks are at maximum simultaneously, next primary fire shoots a Symphonic Overture — a massive full-screen-width crescendo wave that ignores all piercing slowdown.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CrescendoWaveBody.fx` | Expanding bell-shaped shockwave | SDF bell curve shape (inverted parabola top, flared edges). UV.x scrolls along wave width. Internal flame texture overlay. Gold → orange gradient with black smoke edges. Scale parameter for crescendo buff sizing. |
| `BellfireRocketTrail.fx` | Rocket exhaust trail | Thin strip trail, UV-scroll fast. Orange → red → black gradient. Smoke texture overlay on outer edges. |
| `SymphonicOverture.fx` | Maximum power full-width wave | Enhanced CrescendoWaveBody with chromatic aberration, screen distortion around edges, and multilayer glow. White-hot core → gold → orange cascade. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CrescendoRippleParticle | Expands outward from wave's path | Thin gold ring sections (arc segments), expanding, 12 frame life |
| BellfireExplosionParticle | Radial burst at rocket impact | Orange-gold fire shards + black smoke puffs, 12-18 per impact |
| GroundFireParticle | Lingers on ground fire patches | Small orange flames, slight upward drift, flicker, 20-30 frame life |
| OvertureFlashParticle | Full-screen flash at Symphonic Overture | Screen-wide gold wash, 5 frame flash, white-hot center |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| BellfireIgnition | Stacking fire DoT from rockets | 90 frames per stack, max 3 |
| CrescendoPressure | Hit enemies move 15% slower per wave hit | 120 frames |

---

## 4. Piercing Bell's Resonance (Ranged)

### Identity & Musical Soul
Where the Annihilator is brute force, Piercing Bell's Resonance is **precision percussion** — the crisp staccato notes of La Campanella played exactly right. A precision ranged weapon where well-placed shots create resonant feedback loops that amplify each other. Patient, technical, rewarding.

### Lore Line
*"A single note, perfectly placed, can shatter a fortress."*

### Combat Mechanics
- **Staccato Bullets**: Primary fire — precision shots that embed a Resonant Marker on hit. Markers are golden bell icons visible on enemy.
- **Seeking Crystals**: Every 4th shot fires a Seeking Crystal that homes toward the nearest Resonant Marker.
- **Resonant Detonation**: When 3+ Resonant Markers overlap on the same enemy, the player can alt-fire to trigger Resonant Blast — all markers detonate simultaneously in overlapping bell-shaped shockwaves. Damage scales with marker count.
- **Resonant Note Projectiles**: Each detonation spawns scattered Resonant Notes that linger 3s and damage enemies passing through them (landmine effect).
- **Perfect Pitch**: If you detonate exactly 5 markers (not 3, not 7, exactly 5), the detonation deals 2x damage and applies Resonant Silence (prevents enemy attacks for 1s).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ResonantMarkerGlow.fx` | Golden bell marker on enemy | Small SDF bell shape with pulsing glow. Gold color. Additive. Pulse frequency increases with marker count. |
| `ResonantBlastWave.fx` | Bell-shaped detonation wave | Bell-curve SDF expanding from each marker point. Overlapping markers create interference pattern (multiplicative blending between waves). Gold → white at overlap. |
| `ResonantNoteField.fx` | Lingering damage note zones | Small circular SDF with embedded music note shape. Gentle pulse. Orange-gold. Additive aura around each note. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| StaccatoBulletTrailParticle | Tight trail behind precision shots | Gold sparks, tiny (1-2px), 8 frame life, thin line |
| SeekingCrystalGlintParticle | Orbits seeking crystal projectile | Small gold glints, 3-4 orbiting, tight radius |
| ResonantBlastRippleParticle | Concentric rings expanding from detonation | Gold ring waves, 3-5 rings staggered, 10 frame life each |
| ResonantNoteMineDriftParticle | Drifts near lingering note zones | Tiny gold motes, hovering, minimal movement, 30 frame life |

---

## 5. Grandiose Chime (Ranged)

### Identity & Musical Soul
The Grandiose Chime is the **cathedral organ** of La Campanella's arsenal — grand, resonant, overwhelming in scope. It fires wide beams, deploys note mines, and chain-kills echo through connected enemies. This is the weapon of a conductor commanding a grand orchestra of destruction.

### Lore Line
*"When the grand chime sounds, the world holds its breath."*

### Combat Mechanics
- **Grandiose Beam**: Primary fire — wide golden beam that sweeps through enemies. Short range but covers wide angle.
- **Bellfire Notes**: Alt fire — deploys floating bell-note mines (max 5 deployed). Mines activate when enemy passes near, dealing damage + small shockwave.
- **Kill Echo Chain**: When an enemy is killed by any Grandiose Chime attack, a Kill Echo propagates to the nearest enemy within 15 tiles (deals 60% of killing blow damage). Can chain up to 3 times.
- **Grandiose Crescendo**: After 5 Kill Echoes chain completely (3 kills from one chain), the next beam fire becomes Grandiose — triple width, +50% damage, and deploys 3 note mines along beam path automatically.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `GrandioseBeamBody.fx` | Wide golden beam | Beam quad with UV-scroll. Internal bell-pattern texture overlay. Gold → white core → gold gradient across beam width. Noise distortion on edges for organic shimmer. |
| `BellfireNoteMine.fx` | Floating bell-note mine glow | SDF music note shape with radial glow aura. Gentle pulse (breathing). Gold body with orange aura ring. Activation: rapid pulse + expand ring. |
| `KillEchoChain.fx` | Lightning-like chain between enemies | Bezier curve strip between kill point and next target. Gold with white-hot core. Rapid flash (appears for 5 frames). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| GrandioseBeamSparkParticle | Sheds from beam edges | Gold sparks, perpendicular to beam, 3-4px, 10 frame life |
| NoteMineOrbitalParticle | Orbits deployed mine | Tiny gold dots, 2-3 orbiting per mine, slow rotation |
| KillEchoFlashParticle | Brief flash at chain connection point | White-gold flash, 3 frame burst, bright |
| MineActivationParticle | Radial burst when mine triggers | Gold-orange sparks + sound wave ring, 10-15 particles |

---

## 6. Fang of the Infinite Bell (Magic)

### Identity & Musical Soul
The Fang is La Campanella's **infinite repetition** — the bell that never stops ringing. In the original piece, the bell motif repeats hundreds of times. This weapon channels that infinite resonance as magic — bell orbs that stack endlessly, each empowering the next. The longer you fight, the more devastating the bell becomes. This is patience and persistence as weapons.

### Lore Line
*"Infinity is not a destination; it is a bell that rings without ceasing."*

### Combat Mechanics
- **Infinite Bell Orbs**: Primary fire — launches bell-shaped energy orbs. Each orb bounces between enemies 2 times. On each bounce, it spawns a smaller echo orb (half damage, bounces once).
- **Stacking Damage System**:
  - **Infinite Bell Damage Buff**: Each bell orb that successfully bounces grants +3% magic damage (max 20 stacks = +60%). Stacks decay 1 per second after 3s of no bouncing.
  - **Infinite Bell Empowered Buff**: At 10+ stacks, bell orbs gain lightning arcs between them (visual chain lightning). At 20 stacks, orbs explode on final bounce instead of fading.
- **Empowered Lightning**: At 10+ stacks, when multiple bell orbs are airborne simultaneously, golden lightning arcs between them. Enemies in the arc path take 30% orb damage.
- **Infinite Crescendo**: At max stacks (20), alt-fire to consume all stacks for one massive Infinite Bell — a slow-moving giant bell orb that bounces 10 times, each bounce does full damage + spawns 4 echo orbs. Ground shakes on each bounce.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `InfiniteBellOrb.fx` | Bell orb body shader | SDF bell/circle hybrid shape with internal gold energy swirl. Color ramp: black center → deep orange → gold surface → white hot edges. Scale parameter for echo orb sizing. |
| `BellLightningArc.fx` | Inter-orb lightning chain | Jagged line strip between orb positions (CPU-computed Bézier with randomized offsets). Gold → white color. Sharp bright bolts with glow around them. 2 frame flicker cycle. |
| `InfiniteCrescendoOrb.fx` | Giant max-stack bell orb | Enhanced InfiniteBellOrb with double size, more turbulent internal energy (higher noise frequency), and pulsing screen distortion around it. Ground impact shader overlay at each bounce point. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| BellOrbTrailParticle | Short trail behind bouncing orbs | Gold-white sparkle dots, 3-4px, 8 frame life |
| BounceImpactParticle | Radial burst at each bounce point | Gold sparks + small bell ring ripple, 8-12 per bounce |
| LightningSparkParticle | Sheds from lightning arcs | Bright white-gold sparks, fast random movement, 5 frame life |
| EchoOrbBirthParticle | Brief flash at echo orb spawn | Orange flash ring expanding, 5 frame burst |
| CrescendoGroundShakeParticle | Rises from ground at Infinite Crescendo bounce | Large orange-gold fire embers, erupts upward, 25 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| BellEchoDissonance | Each bounce hit adds +5% damage taken from bell orbs (max 3 stacks) | 180 frames per stack |

---

## 7. Infernal Chimes' Calling (Summon)

### Identity & Musical Soul
The Infernal Chimes' Calling summons a **Campanella Choir** — a ghostly bell orchestra that rings destruction. The minion is not a single entity but a formation of 3-5 spectral bells that hover around the player and attack in coordinated bell-strike patterns. The choir should feel like an infernal orchestra — each bell chiming in turn to create a deadly melody.

### Lore Line
*"The choir sings not hymns of peace, but anthems of annihilation."*

### Combat Mechanics
- **Campanella Choir**: Summons spectral bell minions (3 at base, +1 per additional summon up to 5). Bells hover in arc formation around player.
- **Bell Strike Pattern**: Bells attack in sequence — Bell 1 fires, then Bell 2, then Bell 3, etc. (staggered 0.3s). Each fires a Shockwave Projectile (bell-shaped wave).
- **Harmonic Convergence**: When all bells fire within 1 second of each other (full sequence), their shockwaves overlap at target. Overlapping waves deal 2x damage in the intersection zone.
- **Infernal Crescendo**: Every 12 seconds, all bells charge simultaneously (glow intensifies for 2s), then fire a synchronized Infernal Barrage — 5 simultaneous massive shockwaves that create a grid-like interference pattern.
- **Bell Sacrifice**: Right-click to sacrifice one bell (min 2 remaining). Sacrificed bell detonates in a massive AoE. Respawns after 15 seconds.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CampanellaChoirAura.fx` | Spectral bell entity glow | SDF bell shape with radial glow. Color: translucent gold → orange edge. Gentle pulse at different frequencies per bell (harmonics). |
| `ChoirShockwaveProj.fx` | Individual bell shockwave attack | Expanding bell-curve SDF. Gold → orange → transparent. Narrow → wide expansion. |
| `InfernalBarrageInterference.fx` | Overlapping shockwave grid | Multiplicative blend of 5 expanding wave SDFs. Where waves overlap, intensity doubles creating bright nodes. Gold-white at nodes. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ChoirBellGlintParticle | Orbits each bell entity slowly | Tiny gold sparkles, 2-3 per bell, slow orbit |
| ShockwaveRippleParticle | Expands outward from shockwave edge | Thin gold ring arcs, 10 frame life, expanding |
| InfernalChargeParticle | Spirals toward bells during charge phase | Orange-gold motes, inward spiral, consumed at bell center |
| SacrificeExplosionParticle | Massive radial burst at sacrifice detonation | Large gold-orange fire shards + smoke + bell fragment shapes, 30+ particles |

---

## Cross-Theme Synergy Notes

### La Campanella Theme Unity
All weapons share the black-orange-gold palette with bell/chime motifs:
- **Dual Fated Chime**: Twin bells, waltz rhythm, shockwave rings
- **Ignition of the Bell**: Percussive power, geysers from below, cyclone vortex
- **Symphonic Bellfire Annihilator**: Maximum firepower, crescendo waves, screen-shaking rockets
- **Piercing Bell's Resonance**: Precision, resonant markers, interference patterns
- **Grandiose Chime**: Grand scope, beam + mines + kill chains
- **Fang of the Infinite Bell**: Infinite stacking, bouncing orbs, lightning chains
- **Infernal Chimes' Calling**: Bell choir formation, coordinated attacks, harmonic overlap

### Musical Motifs
- **Bell shockwave rings** appear in EVERY weapon (different sizes, behaviors) — the unifying visual motif
- **Black smoke + orange fire** is the palette foundation — every weapon uses this differently
- **Stacking/crescendo mechanics** reflect La Campanella's continuous build — most weapons have escalating power systems
- **Percussion** over melody — these are impact-focused, percussive weapons. SFX should emphasize bells, chimes, metallic strikes, and resonant tones
