# 🕰️ Clair de Lune — Resonance Weapons Planning

> *"Moonlit reverie — shattered clocks against time's wake."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Debussy's Clair de Lune — dreamlike, impressionistic, a reverie suspended between time and memory |
| **Emotional Core** | Dreamlike calm, gentle luminescence, time's fragility |
| **Color Palette** | Night mist blue, soft blue, pearl white, clockwork gold accents |
| **Palette Hex** | Temporal Void `(15, 20, 45)` → Night Mist Blue `(40, 60, 110)` → Soft Moonblue `(100, 140, 200)` → Pearl Frost `(180, 200, 230)` → Clockwork Gold `(200, 170, 80)` → Pearl White `(235, 240, 250)` |
| **Lore Color** | `new Color(150, 200, 255)` — Ice Blue |
| **Lore Keywords** | Time, clocks, gears, moonlight, dream, mist, frost, memory, reverie, clockwork, shattered, echo, temporal |
| **VFX Language** | Clockwork gears, shattered clock faces, temporal distortions, frozen time fragments, moonlit mist, ticking rhythms, pendulum swings, hourglass sand, clock hands sweeping, reversed time echoes |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Chronologicality | Melee | Temporal gear-based combo with time slow fields |
| 2 | Temporal Piercer | Melee | Time-piercing thrusts with delayed damage echo |
| 3 | Clockwork Harmony | Melee | Gear wave projectiles with interlocking combo |
| 4 | Starfall Whisper | Ranged | Time-piercing crystal bolts with temporal fracture |
| 5 | Midnight Mechanism | Ranged | Clockwork gatling with spin-up momentum |
| 6 | Cog and Hammer | Ranged | Clockwork explosive bombs with gear shrapnel |
| 7 | Clockwork Grimoire | Magic | 4-mode spell cycle (hour/minute/second/pendulum) |
| 8 | Orrery of Dreams | Magic | Orbiting celestial spheres with gravitational magic |
| 9 | Requiem of Time | Magic | Channeled time-reversal / forward-jump magic |
| 10 | Lunar Phylactery | Summon | Beam-firing moonlight minion with soul-link |
| 11 | Gear-Driven Arbiter | Summon | Gear-flinging clockwork minion with Temporal Judgment |
| 12 | Automaton's Tuning Fork | Summon | Tuning fork automaton that resonates and amplifies |

---

## 1. Chronologicality (Melee)

### Identity & Musical Soul
The word itself — Chronologicality — is the nature of time's passage. This greatsword embeds temporal gears into its strikes, creating pockets of slowed time where the blade has already passed. Each swing leaves behind a frozen echo of the blade path that detonates moments later. Fighting with this weapon feels like watching combat through a grandfather clock's mechanism — deliberate, inevitable, perfectly timed.

### Lore Line
*"What the clock hand touches, it has already destroyed. Time merely waits for matter to notice."*

### Combat Mechanics
- **3-Phase Temporal Combo**:
  - **Phase 1 — Hour Hand**: Slow, sweeping horizontal arc. Wide reach. Leaves a temporal echo (frozen blade silhouette) that detonates after 0.8s for 50% of swing damage.
  - **Phase 2 — Minute Hand**: Faster overhead to diagonal strike. Moderate reach. Temporal echo detonates in 0.5s for 60%.
  - **Phase 3 — Second Hand**: Fast thrust. Short range but precise. Temporal echo detonates in 0.3s for 75%. Punctuated by a clock-tick sound.
- **Time Slow Field**: Where temporal echoes detonate, they leave a brief Time Slow Field (2s) — enemies within are slowed by 40%. Multiple overlapping fields stack duration, not intensity.
- **Chronological Mastery**: Hitting an enemy with both the swing AND its temporal echo (requires positioning so the enemy stays in the echo path) deals a Temporal Resonance bonus (additional 30% damage burst + 1s freeze).
- **Clockwork Overflow**: Every 12th swing (full clock cycle), the weapon enters Clockwork Overflow — next 3 swings leave 3 temporal echoes each instead of 1 (massive area denial).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ChronologicalSwingTrail.fx` | Swing trail with embedded gear motif | Trail strip: night mist blue body → soft moonblue edge. Internal gear-tooth pattern UV-scroll (interlocking cog shapes scrolling along trail). Clockwork gold accent lines. Trail has a "ticking" brightness pulse (brightness oscillates at clock-tick rate). |
| `TemporalEchoSilhouette.fx` | Frozen blade echo waiting to detonate | Ghost blade silhouette — semi-transparent, pearl frost color. Stuttering opacity (flickers between visible/transparent at high frequency, like a glitching time echo). Embedded clock-face fragment in the blade. Countdown indicator (subtle circular fill around echo). On detonation: sharp flash + expanding gear ring. |
| `TimeSlowField.fx` | Time slow zone VFX | SDF circle with clockwork interior. Slow-rotating gear patterns inside. Sepia-tinged (slight color desaturation). Edges: ticking clock marks around circumference. Semi-transparent overlay. Enemies inside rendered with brief afterimage stuttering (possible screen-space approach). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| GearFragmentParticle | Burst from temporal echo detonation | Clockwork gold gear fragments, 8-12 per burst, tumbling rotation, 10 frame life |
| ClockTickSparkParticle | Tick at each swing phase transition | Pearl white spark, single bright flash, 2 frame life |
| TimeSlowFieldMoteParticle | Ambient particles in time slow field | Night mist blue motes, extremely slow drift (visually demonstrating time slow), 30 frame life |
| ChronologicalOverflowGearParticle | Orbiting gears during Clockwork Overflow | Gold mini-gears orbiting weapon, 3 per orbit, continuous during overflow |

#### Bloom Layers
1. **Swing trail edge**: Soft moonblue glow along trail
2. **Temporal echo**: Pearl frost pulsing glow around frozen echo
3. **Detonation burst**: Bright clockwork gold flash at detonation
4. **Time slow field edge**: Subtle indigo glow at field boundary

---

## 2. Temporal Piercer (Melee)

### Identity & Musical Soul
A rapier/lance that pierces through time itself. Each thrust doesn't just damage — it creates a temporal puncture at the hit point. The future damage of the weapon "leaks through" these punctures, hitting again moments later. The weapon's identity is precision and inevitability — every thrust has consequences that echo forward in time.

### Lore Line
*"A wound that remembers its own future. The blade only needs to touch you once."*

### Combat Mechanics
- **3-Phase Temporal Thrust Combo**:
  - **Phase 1 — Present Thrust**: Quick forward thrust. Standard damage. Creates Temporal Puncture mark on hit enemy.
  - **Phase 2 — Future Echo Thrust**: Faster thrust. On hit: triggers all existing Temporal Puncture marks on that enemy (each mark detonates for 40% of original thrust damage).
  - **Phase 3 — Temporal Cascade Thrust**: Powerful thrust. Creates Temporal Puncture + immediately detonates all marks + creates new marks at detonation points of old marks (cascade).
- **Temporal Puncture**: Marks persist on enemies for 4 seconds. Max 5 marks per enemy. Each mark is visible as a small clock-face wound on the enemy. Marks from Phase 3 cascade: old marks detonate and new marks appear at detonation points — if the enemy is still there, marks stack rapidly.
- **Time Piercing**: Thrusts have 5% chance to "pierce time" — the thrust passes through the enemy and hits a second time from behind 0.3s later (temporal boomerang). +15% crit = 15% chance.
- **Frozen Moment**: When an enemy has 5 Temporal Puncture marks, they enter Frozen Moment — 2s stun (time freeze) + all marks detonate simultaneously. Devastating combo finisher.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `TemporalPierceTrail.fx` | Thrust trail — thin and sharp | Very thin, precise trail strip. Pearl white → soft moonblue. Internal clock-hand line pattern (single thin line running down center, like a clock hand sweeping). Brief afterimage at thrust endpoints. Not wide — rapier precision. |
| `TemporalPunctureWound.fx` | Clock-face mark on enemy hit point | Small SDF clock-face (circle with 12 tick marks + 2 hands). Pearl frost → clockwork gold. Hands spin at different speeds (hour/minute). At 5 marks (Frozen Moment): all clock-faces glow brilliant white, hands freeze at 12:00, then shatter. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PunctureMarkCreateParticle | Flash at puncture mark creation | Clock-face fragment, clockwork gold, 4 frame burst |
| PunctureDetonateParticle | Burst at puncture mark detonation | Time-frost shards, pearl blue, 6 per burst, 8 frame life |
| TimePierceEchoParticle | Visual for "pierce time" boomerang hit | Ghost blade silhouette appearing behind enemy, 5 frame burst |
| FrozenMomentCrystalParticle | Shards from Frozen Moment trigger | Clock-face fragments shattering outward, 12+ particles, clockwork gold, 15 frame life |

---

## 3. Clockwork Harmony (Melee)

### Identity & Musical Soul
Harmony in clockwork — the beauty of gears meshing perfectly. This weapon fires interlocking gear projectiles with each swing, and the gears interact with each other on the battlefield. When gears from different combo phases mesh, they create amplified damage zones. The weapon rewards understanding how its different phases work together — like appreciating how every gear in a clock contributes to the whole.

### Lore Line
*"A thousand gears turning in silence. You hear nothing until you hear everything."*

### Combat Mechanics
- **3-Phase Interlocking Combo**:
  - **Phase 1 — Small Gear**: Fires 2 small, fast gear projectiles. They spin forward and stick to the first enemy/block hit, spinning in place for 3s. Deal minor ongoing damage.
  - **Phase 2 — Medium Gear**: Fires 1 medium gear projectile. Slower but larger. If it passes near a spinning small gear, it meshes — both gears amplify, dealing 2x damage.
  - **Phase 3 — Drive Gear**: Fires 1 large drive gear. If it passes near ANY spinning gear (small or medium), it meshes with all of them — chain reaction of interlocking. All meshed gears explode for 3x damage.
- **Gear Persistence**: Small gears last 3s, medium gears last 4s. Smart combo play: fire Phase 1 to plant small gears, Phase 2 to mesh medium, Phase 3 to trigger chain.
- **Harmony Bonus**: If all three gear types are meshed simultaneously (small + medium + large connected), it triggers a Clockwork Harmony — massive AoE explosion + all enemies hit receive "Temporal Dissonance" debuff (+20% damage taken for 5s).
- **Gear Recall**: Alt fire recalls all spinning gears back to the player. Gears deal damage along their return path. If gears cross paths on recall, they mesh briefly (bonus damage).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ClockworkGearProjectile.fx` | Spinning gear projectile body (parameterized by size) | SDF gear shape — teeth, shaft hole, spokes visible. Clockwork gold body → pearl white highlights on teeth. Spin animation (rotation). Size parameter: small (8px), medium (16px), large (24px). When meshed: bright connectivity lines between meshed gears visible. |
| `GearMeshConnector.fx` | Line connecting meshed gears | Beam strip between meshed gear positions. Clockwork gold → pearl white. Internal energy flow (energy traveling from small → to large). Pulsing brightness. When all 3 types connected: lines brighten + golden particle shower. |
| `ClockworkHarmonyExplosion.fx` | Massive AoE from full 3-gear mesh | Expanding gear ring — concentric rings of gear teeth expanding outward. Clockwork gold core → night mist blue outer. Gear fragments embedded in the expansion ring. Screen shake. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SmallGearSpinParticle | Orbiting sparkles around spinning small gears | Clockwork gold sparks, tight orbit, 15 frame life |
| MediumGearMeshParticle | Burst when medium gear meshes with small | Gold spark burst + small gear teeth, 8 frame life |
| DriveGearImpactParticle | Heavy burst when drive gear triggers chain | Heavy gear fragments, clockwork gold, 12+ particles, 12 frame life |
| HarmonyExplosionGearParticle | Gear fragments from Clockwork Harmony | Multi-size gear fragments, clock-face pieces, clock hands, 15 frame life |
| GearRecallTrailParticle | Trail behind recalled gears | Gold streak, 5 frame life |

---

## 4. Starfall Whisper (Ranged)

### Identity & Musical Soul
A crossbow/rifle that fires crystal bolts infused with temporal energy. Each bolt whispers through time — on impact, it creates a temporal fracture that echoes the damage forward in time. The weapon is elegant, precise, and its shots feel like they carry the weight of frozen moments. Every shot is a star falling in slow motion.

### Lore Line
*"The crystal remembers the moonlight. And moonlight, as we know, is just old light."*

### Combat Mechanics
- **Crystal Bolt**: Primary fire — fires a pearl-blue crystal bolt. Fast, accurate, moderate damage. On hit: creates a Temporal Fracture at impact point.
- **Temporal Fracture**: The fracture lasts 2s. After 2s, it "replays" — the bolt re-appears at the fracture and fires forward again (original trajectory). If this replay bolt hits an enemy, it creates another (smaller) fracture. Max 2 replays per original bolt.
- **Shattered Time**: Alt fire — fires a Shattered Time bolt (uses 3 ammo). On impact: creates 5 small Temporal Fractures in a star pattern. All replay simultaneously after 1.5s. Massive burst potential if enemies cluster near fractures.
- **Moonlit Precision**: Critical hits create brighter, larger fractures that replay at 1.5x damage. Encourages precision play.
- **Temporal Refraction**: Bolts that pass through Time Slow Fields (from other Clair de Lune weapons) refract — splitting into 2 bolts at slightly different angles. Cross-weapon synergy.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CrystalBoltTrail.fx` | Crystal bolt projectile trail | Thin bright trail strip. Pearl white core → soft moonblue edge. Internal crystalline refraction pattern (faceted look). Brief, clean — precision weapon. |
| `TemporalFracturePortal.fx` | Temporal fracture at impact point | SDF clock-face crack — like a crack in glass with clock numerals visible through the crack. Pearl frost → clockwork gold numerals. Pulsing (ticking countdown to replay). On replay trigger: crack shatters, bolt emerges. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CrystalBoltSparkParticle | Sparks from crystal bolt in flight | Pearl blue crystal shards, small, 4 frame life |
| TemporalFractureTickParticle | Ticking indicator near fracture | Clock hand rotating around fracture, clockwork gold, continuous until replay |
| FractureReplayFlashParticle | Flash when fracture replays bolt | Bright pearl white flash + glass-shatter sound, 5 frame burst |
| ShatteredTimeStarParticle | Star pattern indicator for Shattered Time fractures | 5 pearl-blue dots in star pattern, connecting briefly, 4 frame burst |

---

## 5. Midnight Mechanism (Ranged)

### Identity & Musical Soul
A clockwork gatling gun — a mechanism of midnight that spins up, fires faster and faster, and becomes an unstoppable hail of temporal bullets. The weapon's identity is mechanical precision at ludicrous speed. The spinning mechanism IS the visual: interlocking gears, spinning barrels, clockwork precision. Raw firepower channeled through clockwork perfection.

### Lore Line
*"The mechanism cares nothing for mercy. It counts only bullets and the distance between them and your skull."*

### Combat Mechanics
- **Clockwork Spin-Up**: Hold fire to spin the mechanism. Takes 2 seconds to reach full speed.
  - **Phase 1 (0-0.5s)**: Slow single shots. 3 shots/s. Each shot is a clockwork bullet — small, precise.
  - **Phase 2 (0.5-1.0s)**: Moderate fire. 6 shots/s. Tighter spread.
  - **Phase 3 (1.0-1.5s)**: Rapid fire. 12 shots/s. Very tight grouping.
  - **Phase 4 (1.5-2.0s)**: Overdrive. 20 shots/s. Tiny bullets but massive volume. Slight screen vibration.
  - **Phase 5 (2.0s+)**: Maximum Mechanism. 24 shots/s. Bullets become piercing. Gears visibly spinning on weapon. Screen shakes.
- **Gear Jam**: If you stop firing before Phase 3, the mechanism resets. If you fire continuously to Phase 5 for 5+ seconds, overheating begins — fire rate decreases back to Phase 3 over 3s, then jams for 1s cooldown.
- **Mechanism Eject**: Alt fire (only during Phase 3+) — ejects all spinning gears as a burst of clockwork shrapnel (15 gear projectiles in a cone). Powerful but resets spin-up.
- **Ticking DPS**: Each bullet hit leaves a "tick mark" on the enemy. At 50 tick marks: the enemy takes a Midnight Strike — all accumulated tick damage is replayed as a burst. Resets marks.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `MidnightMechanismBarrel.fx` | Spinning barrel/gear visual on weapon | Clockwork gear assembly overlay on weapon sprite. Rotation speed increases with phase. Clockwork gold gears, pearl blue inner mechanisms. At Phase 5: hot glow (warm gold → orange, overheating indicator). |
| `ClockworkBulletStream.fx` | Rapid-fire bullet trail optimization | Optimized trail for 20+ bullets/s — instead of individual trails, renders a continuous stream strip from barrel to furthest bullet. Clockwork gold → pearl blue. Width and brightness increase with phase. Internal rapid-tick pattern. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ClockworkBulletImpactParticle | Small impact per bullet hit | Clockwork gold spark, tiny, 2 frame life |
| SpinUpGearParticle | Gears spinning up around weapon at higher phases | Gold mini-gears orbiting barrel, more at higher phases, continuous |
| MechanismEjectShrapnelParticle | Burst from alt-fire gear eject | Heavy gear fragments in cone, 15+ particles, 10 frame life |
| MidnightStrikeAccumulationParticle | Burst when 50 tick marks trigger Midnight Strike | Clock-face explosion + tick-mark number scatter, 15 frame burst |
| OverheatSteamParticle | Steam from overheating | Warm gold steam wisps rising from barrel at Phase 5+, 15 frame life |

---

## 6. Cog and Hammer (Ranged)

### Identity & Musical Soul
An explosive ranged weapon that fires clockwork bombs — intricate mechanisms of gears and springs that detonate on impact or after a timed fuse. The "hammer" strikes the "cog" to launch each bomb, and the resulting explosion scatters gear shrapnel. Methodical, devastating, and satisfyingly mechanical.

### Lore Line
*"Every gear was placed with care. Every explosion was calculated."*

### Combat Mechanics
- **Clockwork Bomb**: Primary fire — lobs an arcing clockwork bomb. Moderate arc (grenade-like). On impact or after 2s fuse: detonates in a 5-tile radius gear explosion.
- **Gear Shrapnel**: Detonation scatters 6 small gear shrapnel projectiles outward. Shrapnel bounces off blocks once before despawning.
- **Sticky Mechanism**: Hold fire for 0.5s before releasing — bomb becomes Sticky. Sticks to enemies/blocks and detonates after 1.5s. Sticky bombs deal 20% more damage.
- **Chain Detonation**: If a bomb detonation's shrapnel hits another undetonated bomb (yours or from Clockwork Harmony gear projectiles), it triggers that bomb's detonation too. Planned chain explosions are devastating.
- **Master Mechanism**: Every 8th bomb is a Master Mechanism — larger, deal 2x damage, scatter 12 shrapnel, and shrapnel homes slightly toward enemies. Indicated by golden glow on the bomb.
- **Disassemble**: Alt fire — if any of your bombs are on the field, remotely detonates all at once. Useful for detonating sticky bombs simultaneously.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ClockworkBombBody.fx` | Animated clockwork bomb projectile | SDF bomb shape with visible internal gears (rotating). Clockwork gold body → pearl blue clock-face on front. Ticking animation (internal gear rotation). Sticky variant: adhesive glow at attachment point. Master Mechanism: larger, golden corona. |
| `GearExplosion.fx` | Gear-based detonation explosion | Expanding ring of gear-tooth shapes. Clockwork gold → warm orange at core → night mist blue at edges. Gear fragments tumbling outward embedded in the ring. Screen shake proportional to explosion size. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| BombArcTrailParticle | Smoke trail behind lobbed bomb | Night mist blue smoke wisps, 10 frame life |
| GearShrapnelTrailParticle | Trail behind bouncing shrapnel | Gold streak, 4 frame life |
| ExplosionGearFragmentParticle | Gear fragments from detonation | Multi-size clockwork gold gears tumbling, 15 frame life |
| StickyBombTickParticle | Ticking indicator on stuck bomb | Clock-hand rotation, clockwork gold, continuous until detonate |
| MasterMechanismCoronaParticle | Golden aura on Master Mechanism bomb | Bright gold sparkles, 10 frame life |
| ChainDetonationLinkParticle | Brief flash between chain-detonated bombs | Gold energy line, 3 frame burst |

---

## 7. Clockwork Grimoire (Magic)

### Identity & Musical Soul
A grimoire of temporal clockwork magic — the keeper of time spells. The book cycles through four spell modes like clock positions: Hour (heavy, slow), Minute (moderate, balanced), Second (fast, light), and Pendulum (oscillating, special). Each mode feels fundamentally different while sharing the clockwork aesthetic. The grimoire itself is a time-keeping device as much as a spell-book.

### Lore Line
*"Four hands. Four tempos. One inevitable conclusion."*

### Combat Mechanics
- **4-Mode Spell Cycle**: Alt fire cycles between modes. Each mode has distinct behavior:
  - **Hour Mode (12 o'clock)**: Fires a massive, slow-moving clock-hand beam that sweeps a wide arc. High damage, 2s cooldown. The beam lingers momentarily.
  - **Minute Mode (3 o'clock)**: Fires 3 clock-hand projectiles in a fan. Moderate speed, moderate damage. Balanced all-purpose.
  - **Second Mode (6 o'clock)**: Rapid-fire tiny clock-tick bolts. 8 bolts/s, low individual damage, high DPS. Very fast.
  - **Pendulum Mode (9 o'clock)**: Fires an oscillating pendulum projectile that swings back and forth while traveling forward (sine wave path). Hits multiple times as it oscillates through enemies.
- **Temporal Synergy**: Using all 4 modes within 10 seconds triggers Temporal Synergy — next cast in any mode deals 2x damage + visual temporal burst.
- **Clock Alignment**: If used precisely at :00, :15, :30, or :45 seconds of real-world clock, spells gain 25% damage boost ("on the mark"). Subtle but rewarding for rhythmic players.
- **Grimoire Autonomy**: When not actively casting, the grimoire hovers beside the player, slowly rotating its clock-face. The current mode is displayed on the hovering book.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `HourModeBeamSweep.fx` | Massive sweeping clock-hand beam | Wide beam strip rendered as a giant clock hand sweeping an arc. Night mist blue body → clockwork gold edge → pearl white core. Internal slow UV-scroll (time-crawling feel). Heavy bloom. |
| `MinuteModeFanProjectile.fx` | Clock-hand fan projectiles | Thin clock-hand shaped projectiles (rendered as elongated sprites with clock-hand shape). Pearl blue → clockwork gold. Clean, moderate, balanced visually. |
| `PendulumOscillation.fx` | Oscillating pendulum projectile with sine path | Pendulum bob shape (SDF circle with rod). Sine wave trail showing oscillation path. Night mist blue rod → clockwork gold bob. Trail shows the wave path in pearl frost. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| HourModeBeamDustParticle | Dust from massive beam sweep | Night mist blue dust motes, 15 frame life |
| MinuteModeImpactParticle | Impact burst from fan projectile | Clock-hand fragments, gold, 6 per burst, 8 frame life |
| SecondModeBoltSparkParticle | Tiny spark from rapid-fire bolts | Pearl blue micro-sparks, 2 frame life |
| PendulumSwingParticle | Trail sparkles following pendulum oscillation | Pearl frost sparkle at each oscillation peak, 6 frame life |
| TemporalSynergyBurstParticle | Burst when Temporal Synergy triggers | Clock-face explosion: 4 miniature clock-hands (one for each mode) bursting outward, clockwork gold, 10 frame burst |
| GrimoireHoverParticle | Ambient around hovering grimoire | Soft blue page-flutter particles, 20 frame life |

---

## 8. Orrery of Dreams (Magic)

### Identity & Musical Soul
An orrery — a mechanical model of celestial bodies — but one that captures dreams instead of planets. Orbiting spheres of dream-energy circle the player, and casting sends them spiraling toward enemies. The weapon blurs the line between celestial mechanics and dream logic. Planetary motion meets moonlit reverie.

### Lore Line
*"The spheres don't orbit a sun. They orbit a dream that forgot to end."*

### Combat Mechanics
- **Dream Spheres**: When equipped, 3 Dream Spheres begin orbiting the player. They orbit in an orrery pattern (different orbital distances and speeds, like planets).
- **Sphere Launch**: Primary fire launches the nearest sphere toward cursor. Sphere deals damage on every enemy hit and returns to orbit after 2s. While a sphere is away, it's missing from the orbit (visual gap).
- **Gravitational Pull**: Spheres in orbit passively pull small enemies slightly toward the player (very minor gravitational effect within 5 tiles).
- **Orbital Resonance**: When all 3 spheres are in orbit simultaneously, they create Orbital Resonance — AoE damage ring around player (3 tile radius, minor damage). Resonance breaks when any sphere is launched.
- **Dream Alignment**: Every 30s, the 3 spheres align (form a line). During alignment (3s window), launching a sphere sends ALL 3 in a chain (like a billiard break). First sphere launches, second follows 0.3s later, third 0.6s later. Each successive sphere deals more damage.
- **Sphere Augmentation**: Sphere power is augmented by moonlight — during nighttime, spheres deal +20% damage and orbit faster. Clair de Lune is moonlight's theme.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `DreamSphereBody.fx` | Orbiting dream sphere entity | SDF sphere with internal pearlescent swirl (dream imagery — misty, soft). Pearl white → soft moonblue. Internal slow noise distortion (dreamy, organic). Each sphere has slightly different hue (sphere 1: blue, sphere 2: pearl, sphere 3: gold). Brightness pulses with orbit cycle. |
| `OrbitalResonanceRing.fx` | AoE resonance ring from 3-sphere orbit | SDF ring at player's orbital radius. Soft moonblue with internal celestial glow. Rotating orrery arm lines connecting ring to player center. Pearl frost edge. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SphereOrbitTrailParticle | Trailing sparkles behind orbiting spheres | Pearl frost motes following orbit path, 10 frame life |
| SphereLaunchTrailParticle | Trail behind launched sphere | Soft moonblue streak, 8 frame life |
| OrbitalResonanceWaveParticle | Pulsing waves from resonance ring | Expanding thin ring, pearl blue, 10 frame life |
| DreamAlignmentFlashParticle | Flash when spheres align | Golden line connecting 3 sphere positions, 5 frame burst |
| SphereReturnParticle | Sparkles as sphere returns to orbit | Pearl frost sparkles, decelerating, 8 frame life |

---

## 9. Requiem of Time (Magic)

### Identity & Musical Soul
The grandest magic of Clair de Lune — a requiem for time itself. This weapon can manipulate time forward and backward. Channel forward to accelerate projectiles and ally buffs. Channel backward to reverse recent enemy movement and undo their progress. Temporal manipulation at its most powerful and conceptually ambitious.

### Lore Line
*"Time has always moved in one direction. Until the requiem began, and time forgot which way was forward."*

### Combat Mechanics
- **Temporal Channel**: Hold fire to channel temporal energy. A visible time-field expands around the player.
- **Forward Requiem**: Release while channeling forward (cursor above player) — time acceleration field. For 3s:
  - Player and ally projectiles move 50% faster in the field
  - Player attack speed +30%
  - Allies in field gain +15% speed
  - Enemies in field: their projectiles also move faster (risk!)
- **Reverse Requiem**: Release while channeling backward (cursor below player) — time reversal field. For 3s:
  - Enemies in field slide back to their position from 2s ago (undoes their movement)
  - Enemy projectiles in field reverse direction
  - Enemy attack speed -30%
  - Player in field: can't fire (risk — you're also affected)
- **Temporal Paradox**: If Forward and Reverse fields overlap (requires two casts in quick succession), the overlap zone creates a Temporal Paradox — 2s of extreme time distortion (heavy damage to everything in the zone, allies included). High risk, high reward.
- **Requiem Toll**: Each channel costs health instead of mana (3% max HP/s while channeling). The power of time manipulation has a cost.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `TemporalChannelField.fx` | Expanding time-field during channel | SDF expanding circle from player. Internal clock-face pattern (full clock visible). Night mist blue → pearl frost. Forward channel: clock hands spin clockwise rapidly. Backward channel: clock hands spin counter-clockwise. Ticking visual pulse. |
| `ForwardRequiemAcceleration.fx` | Forward time acceleration screen overlay | Screen-space blue-shift effect — slight screen tone shift toward blue-pearl. speed-line overlays radiating outward from center. Everything feels faster visually. Subtle chromatic aberration stretching forward. |
| `ReverseRequiemReversal.fx` | Reverse time screen overlay | Screen-space sepia-shift — warm, aged tone. Motion trails going backward (afterimages preceding enemies, not following). Rewind VHS-like scan lines (subtle). Everything feels like it's un-happening. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| TemporalChannelEnergyParticle | Spiraling energy during channel | Night mist blue energy spiraling into weapon, clockwise/counterclockwise based on direction, continuous |
| ForwardAccelerationLineParticle | Speed lines in forward field | Pearl blue lines radiating outward, 5 frame life |
| ReverseRetraceParticle | Enemy retrace ghost trail | Ghost silhouette of enemy sliding backward along old path, night mist blue, 15 frame life |
| TemporalParadoxDistortionParticle | Chaotic particles in Paradox zone | Multi-color (blue + gold + white) chaotic flickering particles, 8 frame life |
| RequiemTollBloodParticle | Health cost indicator | Small red-pearl particles drifting from player during channel, 10 frame life |

---

## 10. Lunar Phylactery (Summon)

### Identity & Musical Soul
A phylactery — a vessel containing a fragment of the moon's soul. When summoned, the phylactery releases a Lunar Sentinel that fires focused moonlight beams at enemies. The sentinel is tethered to the phylactery (and thus to the player's life force). The soul-link means the sentinel's power scales with the player's current health — a living weapon tied to its master.

### Lore Line
*"The moon does not care that you borrowed a piece of it. The moon barely notices. That terrifies me more."*

### Combat Mechanics
- **Lunar Sentinel**: Summons a moonlight entity — crystalline, hovering, with a single glowing eye. Fires moonlight beams at the nearest enemy (sustained beam, not projectiles).
- **Beam Behavior**: The beam is continuous while the sentinel has a target. Deals moderate DPS. Beam color: pearl white → soft moonblue.
- **Soul-Link**: Sentinel damage scales with player HP percentage. At 100% HP: 100% damage. At 50% HP: 130% damage (stronger as desperation grows). At 25% HP: 170% damage. Below 10%: 200% damage. Risk-reward scaling.
- **Phylactery Pulse**: Every 10s, the phylactery pulses — sentinel fires an enhanced beam (2x width, 1.5x damage) for 2s. During pulse, the phylactery glows brightly.
- **Moonlit Tether**: A visible tether (thin beam) connects the sentinel to the player. If the tether passes through enemies, they take minor tether damage.
- **Resonant Link**: If multiple Lunar Sentinels are summoned, their beams can cross. When beams cross, the intersection point creates an AoE damage zone.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `LunarSentinelBody.fx` | Moonlight entity body | Crystalline form — SDF octahedron/crystal shape. Pearl white → soft moonblue inner glow. Single bright eye (clockwork gold pupil). Glow intensity scales with player HP % (brighter at low HP). Rotation animation. |
| `MoonlightBeam.fx` | Sustained moonlight beam | Beam strip from sentinel to target. Pearl white core → soft moonblue edge. Internal UV-scroll moonlight pattern (soft, flowing, impressionistic — Debussy). Width parameter for Phylactery Pulse (2x during pulse). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SentinelAuraParticle | Ambient glow around sentinel | Pearl frost sparkles, slow orbit, 15 frame life |
| BeamImpactParticle | Dust at beam-enemy contact point | Soft moonblue sparks, 5 frame life |
| PhylacteryPulseRingParticle | Expanding ring from phylactery pulse | Pearl blue ring, expanding, 8 frame life |
| TetherShimmerParticle | Shimmer along soul-link tether | Pearl frost sparkle traveling along tether, 10 frame life |
| BeamIntersectionAoEParticle | Indicator at beam crossing point | Moonblue glow zone, continuous while beams cross |

---

## 11. Gear-Driven Arbiter (Summon)

### Identity & Musical Soul
A clockwork judge — an automaton that arbitrates with spinning gears. The Arbiter minion flings clockwork gears at enemies and can deliver Temporal Judgment — a debuff that ticks like a clock and detonates when the timer runs out. The minion is mechanical, precise, and inexorable. Every gear it flings is a verdict.

### Lore Line
*"The Arbiter does not deliberate. The gears have already decided."*

### Combat Mechanics
- **Clockwork Arbiter Minion**: Summons a floating clockwork automaton — gears, springs, a single judicial eye. Flings clockwork gear projectiles at enemies.
- **Gear Projectile**: Each gear is a spinning clockwork disc. Moderate damage, moderate speed. Gears bounce off walls once.
- **Temporal Judgment**: Every 5th gear hit applies Temporal Judgment debuff to the target. Temporal Judgment: a visible ticking clock on the enemy. After 3s (the ticking timer), it detonates for 200% of the gear's damage. If the enemy dies before detonation, the detonation triggers immediately (execution damage).
- **Arbiter's Verdict**: When the Arbiter delivers a Temporal Judgment, it briefly stops attacking and hovers (1s) — during this hover, it scans all nearby enemies within 10 tiles and marks the most damaged one (lowest HP %). The Arbiter then focuses this target until it dies. Focus fire increases Arbiter attack speed by 30%.
- **Clockwork Court**: Multiple Arbiters form a Court. When 3+ Arbiters are present, they coordinate — their Temporal Judgments synchronize (all detonate simultaneously for chain AoE).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ArbiterBody.fx` | Clockwork automaton body | Mechanical form — visible gears, springs, pistons. Clockwork gold primary → night mist blue accents. Central judicial eye (pearl white, observing). Gears rotate when attacking (visual feedback). State-based animation (attacking vs scanning vs idle). |
| `TemporalJudgmentClock.fx` | Ticking clock debuff indicator on enemy | SDF clock-face overlay on enemy. Visible hands ticking down from 12 to 3 (3 seconds of countdown). Clockwork gold → pearl white. As timer approaches 0: clock glows brighter, ticking visual flash at each second. On detonation: clock shatters outward in gear fragments. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ArbiterGearProjectileTrailParticle | Trail behind flung gears | Clockwork gold streak, 4 frame life |
| TemporalJudgmentTickParticle | Tick flash at each second of timer | Gold clock-hand flash, 2 frame burst (once per second) |
| JudgmentDetonationParticle | Burst when Temporal Judgment detonates | Clock fragments + gear shrapnel, clockwork gold, 15+ particles, 12 frame life |
| ArbiterScanParticle | Line from Arbiter to scanned target during verdict | Thin pearl blue scanning line, 5 frame life |
| CourtSyncPulseParticle | Pulse between synchronized Arbiters | Gold pulse traveling between Arbiter positions, 8 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Temporal Judgment | Ticking clock visible on enemy. Detonates for 200% gear damage after timer. | 180 frames (3s). Triggers early on enemy death. |

---

## 12. Automaton's Tuning Fork (Summon)

### Identity & Musical Soul
A tuning fork wielded by a clockwork automaton — the purest expression of Clair de Lune's musical-clockwork fusion. The tuning fork resonates at specific frequencies, and the automaton strikes it to create resonance fields that amplify all nearby allied minions and damage enemies through pure vibration. A support summon that makes every other summon better.

### Lore Line
*"Perfect pitch. Perfect time. The fork does not tolerate imperfection."*

### Combat Mechanics
- **Tuning Fork Automaton**: Summons a clockwork automaton holding a tuning fork. The automaton doesn't attack directly — instead, it strikes the fork periodically, creating resonance waves.
- **Resonance Wave**: Every 3 seconds, the automaton strikes the fork. A visible resonance wave expands outward (8 tile radius). Effects:
  - Allied minions in range: +15% damage for 3s (refreshing on each strike = permanent while nearby)
  - Enemies in range: take 5% of their current HP as vibration damage (caps at weapon damage)
  - Projectiles in range: move 10% faster (ally projectiles only)
- **Harmonic Frequency**: The tuning fork cycles through frequencies: A440, C523, E659, G784. Each frequency buffs a different stat for nearby minions:
  - A (440Hz): Attack speed +20%
  - C (523Hz): Knockback +30%
  - E (659Hz): Range +25%
  - G (784Hz): Critical chance +10%
  Frequency changes every 5 resonance strikes (15s).
- **Perfect Resonance**: If 2 Tuning Fork automatons are present, their waves can create Perfect Resonance when waves overlap — doubled effects in the overlap zone.
- **Conductor's Final Note**: Right-click causes ALL Tuning Fork automatons to strike simultaneously — massive resonance wave (12 tile radius) that applies all 4 frequency buffs at once for 5s. 30s cooldown.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `TuningForkResonanceWave.fx` | Expanding resonance wave from fork strike | SDF expanding ring (3 concentric rings for visual richness). Color tinted by current frequency: A=soft moonblue, C=pearl white, E=clockwork gold, G=night mist blue. Internal standing wave pattern visible in each ring. Ring width thins as it expands. |
| `TuningForkStrike.fx` | Visual impact of fork being struck | Brief flash at fork tines — bright pearl white flash expanding into wave. Fork tine vibration (lateral oscillation visible on the sprite). Musical note glyph appears briefly at strike point. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ResonanceWaveExpandParticle | Particles riding the resonance wave front | Pearl frost motes riding expanding ring, 10 frame life |
| ForkStrikeFlashParticle | Impact flash at fork strike | Bright pearl white flash, 3 frame burst |
| FrequencyNoteParticle | Musical note showing current frequency (A, C, E, G) | Clockwork gold miniature note glyph, floating near automaton, 15 frame life |
| PerfectResonanceOverlapParticle | Intense particles in overlap zone of 2 automatons | Brilliant pearl-gold sparkles, intense, 12 frame life |
| ConductorFinalNoteParticle | Massive burst from Conductor's Final Note. | multi-color (all 4 frequency colors) expanding note glyphs, 15 frame life |

---

## Cross-Theme Synergy Notes

### Clair de Lune Theme Unity
All weapons share night mist blue + pearl frost + clockwork gold palette with temporal/clockwork motifs:
- **Melee trio**: Temporal manifestation — time echoes (Chronologicality) → puncture wounds (Temporal Piercer) → interlocking gears (Clockwork Harmony)
- **Ranged trio**: Mechanical precision — crystal bolt echoes (Starfall Whisper) → clockwork gatling (Midnight Mechanism) → clockwork bombs (Cog and Hammer)
- **Magic trio**: Time magic spectrum — 4-mode spellbook (Clockwork Grimoire) → orbital dream mechanics (Orrery of Dreams) → raw time manipulation (Requiem of Time)
- **Summon trio**: Support spectrum — moonlight beam sentinel (Lunar Phylactery) → clockwork judge (Gear-Driven Arbiter) → resonance amplifier (Automaton's Tuning Fork)

### Temporal Mechanics Cross-Reference
Several weapons create shared temporal fields:
- Chronologicality creates Time Slow Fields
- Starfall Whisper bolts refract through Time Slow Fields
- Requiem of Time creates acceleration/reversal fields affecting all temporally-sensitive weapons
- Temporal Piercer and Gear-Driven Arbiter both use time-delayed detonation (puncture marks / Temporal Judgment)

### Visual Distinction
Despite shared palette:
- **Chronologicality**: Heavy, deliberate, frozen-echo afterimages — grandfather clock weight
- **Temporal Piercer**: Sharp, precise, small clock-face wounds — rapier precision
- **Clockwork Harmony**: Interlocking mechanical gears — satisfying mesh/chain reactions
- **Starfall Whisper**: Crystal fractures in spacetime — elegant precision
- **Midnight Mechanism**: Spinning barrels, bullet streams — raw mechanical firepower
- **Cog and Hammer**: Arcing bombs, gear shrapnel — explosive clockwork
- **Clockwork Grimoire**: Four distinct spell modes — clock-position variety
- **Orrery of Dreams**: Orbiting spheres, celestial mechanics — dreamy orrery
- **Requiem of Time**: Screen-wide temporal manipulation — forward/reverse duality
- **Lunar Phylactery**: Sustained beams, crystal sentinel — moonlight focus
- **Gear-Driven Arbiter**: Judicial clockwork, ticking judgments — inevitable verdicts
- **Automaton's Tuning Fork**: Resonance waves, frequency buffs — harmonic support

### Musical-Clockwork Fusion
Clair de Lune uniquely blends two metaphors:
1. **Clockwork**: Gears, springs, mechanisms, ticking, precision
2. **Impressionist music**: Debussy's dreamy, flowing, soft moonlight

The weapons that lean clockwork (Midnight Mechanism, Cog and Hammer, Gear-Driven Arbiter) should still have soft, moonlit coloring.
The weapons that lean musical/dreamy (Orrery of Dreams, Requiem of Time, Automaton's Tuning Fork) should still have clockwork gears in their aesthetic.
Every weapon should feel like both a timepiece AND a reverie.
