# ✨ Fate — Resonance Weapons Planning

> *"The celestial symphony of destiny — you wield the power of the cosmos itself."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Symphony No. 5 "Fate" — the knock of destiny, cosmic inevitability |
| **Emotional Core** | Cosmic inevitability, endgame awe, celestial power |
| **Color Palette** | Black void, dark pink, bright crimson, celestial white |
| **Palette Hex** | Cosmic Void `(5, 0, 15)` → Dark Nebula Pink `(160, 30, 80)` → Fate Crimson `(220, 40, 60)` → Stellar White `(255, 250, 255)` → Constellation Gold `(255, 220, 140)` |
| **Lore Color** | `new Color(180, 40, 80)` — Cosmic Crimson |
| **Lore Keywords** | Destiny, cosmos, constellation, star, fate, inevitability, celestial, symphony, annihilation, conductor |
| **VFX Language** | Ancient constellation glyphs, star particle streams, cosmic cloud energy, nebula trails, chromatic aberration, gravitational lensing, screen distortions |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Requiem of Reality | Melee | Spectral blades + cosmic note attacks |
| 2 | The Conductor's Last Constellation | Melee | Held swing + sword beam release |
| 3 | Coda of Annihilation | Melee | Zenith-style ultimate sword |
| 4 | Opus Ultima | Melee | THE Magnum Opus — 3-movement master weapon |
| 5 | Fractal of the Stars | Melee | Orbiting spectral star blades with Star Fracture |
| 6 | Resonance of a Bygone Reality | Ranged | Spectral blade + rapid bullet fusion |
| 7 | Light of the Future | Ranged | Accelerating light bullets + cosmic rockets |
| 8 | The Final Fermata | Magic | Spectral swords + slash waves |
| 9 | Symphony's End | Magic | Spiraling blades that shatter on contact |
| 10 | Destiny's Crescendo | Summon | Crescendo Deity minion with cosmic beams |

---

## 1. Requiem of Reality (Melee)

### Identity & Musical Soul
A requiem for reality itself — this blade cuts not just flesh but the fabric of existence. Each swing tears a small rift where spectral blades emerge, and cosmic notes scatter like stars being born and dying in the blade's wake. The first of the Fate melee weapons — powerful but not the pinnacle.

### Lore Line
*"Reality sang its last note when this blade was forged."*

### Combat Mechanics
- **3-Phase Combo**:
  - **Phase 1 — Opening Motif**: Horizontal slash spawning 2 RequiemSpectralBlades that fly forward (pass through enemies).
  - **Phase 2 — Development**: Rising diagonal slash spawning 3 RequiemCosmicNotes (homing projectiles shaped like musical notes, crimson energy).
  - **Phase 3 — Recapitulation**: Heavy slam spawning 4 spectral blades in a fan + a gravity pull at slam point (draws enemies inward for 1s).
- **Reality Tear**: On-hit effect — 15% chance to spawn a small reality tear behind the enemy (lingering 2s), dealing continuous damage to anything inside.
- **Spectral Resonance**: Spectral blades that pass through enemies leave Spectral Resonance stacks (max 3). At 3 stacks, enemy takes a delayed burst of cosmic damage after 1 second.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `RequiemSlashTrail.fx` | Swing trail with cosmic energy | UV-scrolled trail strip. Core: deep crimson → cosmic void. Edge: dark pink → stellar white sparks. Internal nebula noise scroll (cosmic noise texture). |
| `RealityTearShader.fx` | Small lingering reality rift | Vertical SDF tear — black void center with chromatic aberration at edges (RGB channel splitting). Edge: crimson-pink shimmering. Brief gravitational distortion. |
| `SpectralBladeBody.fx` | Spectral blade sub-projectile | Transparent ghostly blade shape — cosmic void body → crimson edge with afterimage trail (multiple fading copies behind). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CosmicNoteParticle | Homing toward target, rotating | Music note shape, crimson-gold, glowing trail, 60 frame life |
| SpectralResonancePulseParticle | Expands from enemy at 3 stacks | Crimson ring expanding, thin, 10 frame burst |
| RealityTearSparkParticle | Sheds from tear edges | Chromatic sparks (color shifts rapidly), 5-8 frame life |
| NebulaTrailMoteParticle | Drifts from swing trail | Dark pink-crimson motes, slow drift, 15 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| SpectralResonance | Stacking (1-3). At 3: delayed cosmic burst. Visual: crimson rings. | 120 frames per stack |
| RealityFrayed | DoT inside reality tear. 15 damage/s. | While in tear zone |

---

## 2. The Conductor's Last Constellation (Melee)

### Identity & Musical Soul
The conductor raises the baton one final time — and the last constellation blazes to life. This weapon channels the conductor's dying art: a held swing that charges power from the heavens, then releases it as a devastating sword beam. The constellation theme means star-map visuals, connected star points, and the geometry of the night sky.

### Lore Line
*"The last constellation he drew was a blade aimed at eternity."*

### Combat Mechanics
- **Held Swing**: Primary use — channels a held swing projectile. The blade glows brighter the longer held (max 2s). During hold, constellation lines and stars appear around the player (cosmetic, building intensity).
- **Constellation Sword Beam**: On release — fires a ConductorSwordBeam. Beam power scales with hold time (0.5s: 1x, 1.0s: 1.5x, 2.0s: 2.5x damage). Beam pierces through enemies.
- **Star Map Overlay**: While charging, a star map overlay appears behind the player — constellation lines connecting star points. The pattern changes per charge level, culminating in a full constellation at max charge.
- **Conductor's Finale**: If the fully charged beam kills an enemy, the constellation "shatters" — all star points become projectiles that fly toward nearby enemies. 8-12 star projectiles per shatter.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ConstellationMapOverlay.fx` | Star map background appearing during charge | Screen-space overlay. Star points (bright dots) with connecting lines. Fades in progressively — more stars/lines at higher charge. Dark background with crimson-gold stars. |
| `ConductorBeamBody.fx` | Sword beam projectile | Wide beam strip with internal cosmic energy scroll. Nebula noise texture inside. Core: stellar white → crimson body → dark pink edge → void falloff. Chromatic aberration at high power. |
| `ConstellationShatterProj.fx` | Star projectiles from shattered constellation | Small star SDF (4-point star) with golden-white glow. Trail: thin constellation-line behind (straight line trail, not curved). Impact: small star burst. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ChargeStarPointParticle | Appears at star positions during charge | Bright stellar white dot, twinkle effect, stationary, appears one by one as charge builds |
| ConstellationLineParticle | Connects star points during charge (lines) | Thin white-gold line segments, drawn between star points, 1px width |
| BeamEdgeSparkParticle | Sheds from beam edges during travel | Crimson-gold sparks, perpendicular drift, 8 frame life |
| ShatterStarTrailParticle | Trail behind shattered constellation projectiles | Gold sparkle, thin trail, 6 frame life |

---

## 3. Coda of Annihilation (Melee)

### Identity & Musical Soul
The coda — the final passage of a musical composition. The Coda of Annihilation is a **Zenith-style weapon** — the pinnacle of melee crafting for the Fate theme. It summons phantom swords that fly toward the cursor, each representing a different Fate weapon incorporated into it. Multiple blades, multiple identities, one devastating symphony of annihilation.

### Lore Line
*"All melodies find their end here. This is the final bar."*

### Combat Mechanics
- **Zenith-Style Projectiles**: Using the weapon fires CodaZenithSword projectiles — phantom versions of Fate melee weapons that fly toward the cursor in overlapping arcs. 3-5 swords airborne at once.
- **Held Swing Anchor**: While using, CodaHeldSwing keeps a physical swing active — combining direct melee with ranged phantom swords.
- **Annihilation Stacks**: Each phantom sword hit builds Annihilation on the target (max 10). At 10 stacks, the target suffers Annihilation — single massive damage burst equal to 50% of all damage dealt during stack buildup.
- **Coda Finale**: After 10 seconds of continuous use, the weapon enters Coda Finale — all phantom swords converge simultaneously on the cursor in a spiral pattern, creating a gravitational singularity (brief) that implodes then explodes.

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CodaZenithSwordBody.fx` | Phantom flying sword rendering | Ghost-blade with cosmic energy fill. Each phantom sword tinted slightly differently (representing different source weapons). Afterimage trail. Crimson-pink with constellation patterns inside. |
| `CodaSwingTrail.fx` | Held swing trail for the anchor | Massive wide trail — cosmic void with star particles embedded. Deep crimson → black → stellar white sparks at edges. |
| `AnnihilationSingularity.fx` | Gravitational singularity at Coda Finale | Screen-space gravitational lens distortion pulling pixels inward. Crimson ring at event horizon. Chromatic aberration intensifying toward center. Brief implosion → massive expansion. |
| `CodaPhantomAura.fx` | Ambient cosmic aura during use | Player surrounded by swirling cosmic cloud energy. Dark pink nebula with embedded star points. Rotates slowly. Intensifies during Finale countdown. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PhantomSwordTrailParticle | Trail behind each phantom sword | Crimson-gold sparkle streak, 8 frame life, color varies per phantom |
| AnnihilationStackRingParticle | Concentric rings on stacked target | Crimson rings tightening with stacks, 1 ring per stack |
| SingularityDebrisParticle | Pulled inward during singularity, then explodes out | Cosmic debris — star fragments, void shards, 20-40 particles |
| CodaFinaleStarBurstParticle | Massive burst from singularity explosion | All Fate-theme colors, 50+ particles, radial explosion, 25 frame life |

---

## 4. Opus Ultima (Melee)

### Identity & Musical Soul
**THE Magnum Opus.** The culmination of all musical training — the final masterwork. This weapon has a **3-movement combo system** like the Incisor of Moonlight, but grander in every way. Each movement represents a complete section of the symphony: Exposition reveals the theme, Development transforms it, and Recapitulation brings it all together in devastating conclusion. This is the mod's signature weapon.

### Lore Line
*"This is not a weapon. This is the masterwork — the opus that was always meant to be."*

### Combat Mechanics (Existing partial implementation — 720 damage, 3-movement system)
- **Movement I — Exposition** (SubSteps: 3):
  - Step 1: Sweeping horizontal arc, wide. Spawns 2 energy ball projectiles.
  - Step 2: Rising diagonal. Spawns 3 energy balls.
  - Step 3: Overhead slam. All energy balls detonate → each explodes into 5 homing seekers.
- **Movement II — Development** (SubSteps: 4):
  - Steps 1-3: Rapid alternating slashes (left-right-left), each faster and narrower. No sub-projectiles — pure melee + on-hit effects.
  - Step 4: Charged thrust: On hit → DestinyCollapse (massive screen-filling effect) + 8 seeking crystal shards orbit impact point.
- **Movement III — Recapitulation** (SubSteps: 2):
  - Step 1: Massive circular sweep (360°, slow, devastating). During sweep, ALL previously spawned homing seekers and crystal shards reactivate and converge on the sweep's center.
  - Step 2: The Grand Finale — weapon plants into ground. Celestial beam descends from above through player + expands outward. All Opus effects detonate simultaneously. Screen distortion + shake.
- **Opus Resonance**: Passive — while equipped, the player pulses with cosmic energy. Each completed movement grants a permanent Opus Resonance stack for the current fight (+5% all damage, max 9 stacks for 3 complete cycles). Stacks visible as orbiting constellation stars.

### VFX Architecture Plan

#### Custom Shaders (6)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `OpusSwingTrail.fx` | Grand swing trail — the richest trail in the mod | Multi-texture layered trail. Layer 1: cosmic void body. Layer 2: nebula noise scroll. Layer 3: constellation star points embedded. Color: deep void → crimson → pink → stellar white at tip. Each movement has different trail parameters (wider, brighter, more layers). |
| `OpusEnergyBall.fx` | Energy ball projectile body | Pulsing SDF sphere with cosmic cloud fill (UV-scrolled nebula). Dark void center → crimson glow → pink edge. Orbiting small star sparkles around each ball. |
| `DestinyCollapse.fx` | Screen-filling cosmic collapse effect | Full-screen shader effect: gravitational lens pulling screen edges inward → chromatic aberration → cosmic void circle expanding → bright crimson ring → white flash. 0.8s duration. The most dramatic single-effect in the mod. |
| `CelestialBeam.fx` | Vertical beam from the heavens in Recapitulation | Full-screen-height beam from top of screen down through player. Internal: constellation patterns scrolling downward. Nebula colors flowing. Massive, awe-inspiring. |
| `OpusResonanceAura.fx` | Passive aura with orbiting constellation stars | Player aura effect. Base: subtle cosmic shimmer. Per Opus Resonance stack: +1 orbiting star point with constellation line connecting them. At 9 stacks: full constellation pattern orbiting player. |
| `OpusFinaleExplosion.fx` | Grand Finale all-effects detonation | Everything detonates. Multiple overlapping SDF rings in all Fate colors. Internal cosmic cloud expansion. Star particle supernova. Screen distortion. The visual climax of the weapon. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| OpusStarSeekerParticle | Trail behind homing seeker projectiles | Gold-crimson sparkle, 6 frame life |
| CrystalShardOrbitParticle | Orbits around impact point (crystal shard behavior) | Faceted crimson-pink crystal, tight orbit, 180 frame life |
| DestinyCollapseDebrisParticle | Pulled inward then explodes | Cosmic debris — void shards, star fragments, reality shards, 30+ |
| CelestialBeamMoteParticle | Descends inside celestial beam | Stellar white motes falling downward inside beam column, 20 frame life |
| OpusResonanceStarParticle | Orbits player per Resonance stack | Gold star point, slow orbit, persists until stack fades |
| GrandFinaleCosmicBurstParticle | Massive burst from finale | 80+ particles, all Fate-theme colors, radial explosion, 30 frame life |

---

## 5. Fractal of the Stars (Melee)

### Identity & Musical Soul
Stars have fractal nature — zoom into a nebula and you find smaller nebulae, smaller stars, smaller constellations in infinite recursion. This blade is **forged from shattered constellations**, and each swing spawns orbital remnants — spectral star blades that circle the player like a private galaxy. The fractal motif: on-hit effects create smaller versions of themselves.

### Lore Line
*"Every star contains a universe. Every universe, another blade."*

### Combat Mechanics (Existing: 850 damage, 3-phase combo)
- **3-Phase Combo**:
  - **Phase 1 — Horizontal Sweep**: Wide slash. On hit: spawns 1 FractalOrbitBlade (spectral star blade that orbits player at medium range).
  - **Phase 2 — Rising Uppercut**: Vertical upward slash. On hit: spawns 2 orbit blades.
  - **Phase 3 — Gravity Slam**: Heavy downward slam. On hit: Star Fracture — geometric explosion (fractal pattern expanding outward).
- **Orbiting Star Blades**: Max 6 orbit blades at once. They circle the player, damaging enemies they pass through. They fire prismatic beams at enemies every 3 seconds.
- **Star Fracture**: Every 3rd hit triggers Star Fracture — geometric explosion with fractal branching pattern (center → 4 branches → each branch splits into 2 → each sub-branch splits into 2). Visual and damaging.
- **Fractal Recursion**: Star Fracture damage itself can trigger mini-Star Fractures on hit (1/3 size, 1/3 damage), which can trigger micro-fractures (1/9 size, 1/9 damage), creating cascading fractal destruction.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FractalSwingTrail.fx` | Swing trail with embedded star points | Trail strip with star-point UV pattern embedded (not just solid color — visible star shapes along the trail). Cosmic void body → gold-crimson edge. Fractal branching pattern at trail tip. |
| `StarFractureGeometric.fx` | Fractal geometric explosion | Expanding fractal pattern: straight lines branching at 90° angles. Each generation smaller and dimmer. Gold lines on void background. Brief but visually complex. Recursive nesting. |
| `OrbitBladeBody.fx` | Orbiting spectral star blade | SDF 4-point star shape. Cosmic fill with internal nebula scroll. Crimson-gold glow. Bright trail behind as it orbits. Prismatic beam shader for ranged attacks. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| OrbitBladeSparkParticle | Sheds from orbit blade as it circles | Gold-crimson spark, 6 frame life, continuous shedding |
| StarFractureBranchParticle | Appears along fractal branch lines | Tiny gold flash at each branch point, 3 frame burst |
| PrismaticBeamSparkParticle | Sheds from orbit blade's ranged beam | Crimson sparkle, perpendicular drift, 5 frame life |
| MicroFractureFlashParticle | Flash at recursive micro-fracture locations | Tiny gold flash, very brief (2 frame), marks recursion |

---

## 6. Resonance of a Bygone Reality (Ranged)

### Identity & Musical Soul
Resonance from a reality that no longer exists — echoes of a cosmic event that happened eons ago. This weapon fires spectral blades and rapid bullets simultaneously, blending melee aesthetics with ranged delivery. The "bygone reality" aspect means visual distortions — things slightly out of phase, ghostly afterimages, temporal echoes.

### Lore Line
*"What you hear is the echo of a universe that no longer exists."*

### Combat Mechanics
- **Dual Fire Mode**: Primary fire simultaneously shoots:
  - 1 ResonanceSpectralBlade (slow, piercing, high damage, ghostly)
  - 3 ResonanceRapidBullets (fast, moderate damage, spread)
- **Temporal Afterimage**: Spectral blades leave temporal afterimages — ghost copies of themselves that appear 0.3s behind, dealing 30% damage.
- **Bygone Resonance**: If a spectral blade AND a rapid bullet both hit the same enemy within 0.5s, they trigger Bygone Resonance on the target — a delayed explosion (1s fuse) that deals bonus damage equal to the combined hit.
- **Reality Fade**: Every 10th combined hit, the player briefly phases (0.3s invulnerability + visual: player becomes semi-transparent with cosmic distortion). Brief window to reposition.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `BygoneSpectralBlade.fx` | Ghostly spectral blade projectile | Transparent blade silhouette with afterimage duplication (shader draws 3 copies: current + 2 faded behind). Color: crimson → pink → cosmic void fade. Chromatic aberration on afterimages. |
| `TemporalPhaseEffect.fx` | Player phasing transparency during Reality Fade | Screen-space effect on player sprite: alpha oscillation (flicker) + cosmic noise distortion + chromatic aberration. Brief (0.3s). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| RapidBulletTrailParticle | Brief trail behind rapid bullets | Crimson-pink streak, thin, 4 frame life |
| BygoneResonanceRingParticle | Expanding ring at resonance trigger | Gold-crimson ring expanding, 15 frame burst |
| TemporalGhostParticle | Faint particles around phasing player | Semi-transparent cosmic motes, 10 frame life |

---

## 7. Light of the Future (Ranged)

### Identity & Musical Soul
Where Bygone Reality looks backward, Light of the Future looks forward — accelerating toward destiny. Bullets fired from this weapon **accelerate** over distance, starting slow and becoming devastatingly fast. Cosmic rockets represent the grand celestial events yet to come — supernovae, gamma ray bursts, the heat death of the universe.

### Lore Line
*"Aim not where they are, but where fate decrees they shall be."*

### Combat Mechanics
- **Accelerating Bullet**: Primary fire — LightAcceleratingBullet starts at 50% base speed, accelerates to 400% by max range. Damage also scales with current speed (faster = more damage at impact). Trail grows brighter as speed increases.
- **Cosmic Rocket**: Alt fire (10s cooldown) — LightCosmicRocket. Slow-traveling rocket that draws in nearby bullets (gravitational pull). On detonation: massive cosmic explosion. All bullets absorbed by the rocket's gravity increase the explosion damage.
- **Future Sight**: While holding fire for 2+ seconds without releasing, a targeting reticle appears at cursor showing where bullets will reach peak speed. Enemies in the "kill zone" have their silhouettes highlighted.
- **Cascade**: If an accelerating bullet kills an enemy at peak speed, it spawns 2 smaller bullets that continue at peak speed in a slight fan. Cascade can chain.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `AcceleratingBulletTrail.fx` | Trail that intensifies with speed | Trail strip that gets wider and brighter as bullet accelerates. Color transitions: dim crimson (slow) → bright stellar white (fast). Internal energy scroll speed also tied to velocity. |
| `CosmicRocketExplosion.fx` | Massive cosmic explosion from rocket detonation | Expanding nebula-cloud sphere. Multi-layer: void center → crimson ring → pink cloud → gold highlights → stellar white outer ring. Internal star points flash randomly. Brief gravitational lens distortion before explosion. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| AccelerationGlintParticle | Appears along bullet trail, more frequent at higher speed | Gold-white sparkle, 4 frame life, spacing decreases with speed |
| RocketGravityPullParticle | Spirals into rocket during bullet absorption | Crimson motes spiraling inward, 15 frame life |
| CosmicExplosionDebrisParticle | Radial burst from rocket explosion | Multi-color cosmic debris, 30+ particles, 20 frame life |
| CascadeSpawnFlashParticle | Flash at cascade spawn point | Gold burst, 3 frame, marks chain kill |

---

## 8. The Final Fermata (Magic)

### Identity & Musical Soul
A fermata is a musical hold — a note sustained beyond its normal duration, at the performer's discretion. The Final Fermata holds magic in suspended animation before releasing it. Spectral swords hover in place, accumulating power, then slash through targets on command. It's the dramatic pause before the devastating conclusion.

### Lore Line
*"Time holds its breath. The fermata decides when it exhales."*

### Combat Mechanics
- **Spectral Sword Placement**: Primary fire — places a FermataSpectralSwordNew at the cursor position. Sword hovers stationary, glowing. Up to 5 swords can be placed simultaneously.
- **Slash Wave Release**: Alt fire — ALL placed swords simultaneously slash toward the nearest enemy, launching FermataSlashWaves. Each sword fires independently. Waves pierce.
- **Fermata Power**: Swords gain +10% damage per second they're held in place (max 5x at 5s). Visual: swords glow brighter, pulsing faster. Incentivizes strategic placement then dramatic release.
- **Harmonic Alignment**: If 3+ swords are placed in a line or triangle formation, they become Harmonically Aligned — connected by cosmic energy lines. Aligned swords' slash waves converge to the same point (crossfire), and the intersection deals bonus damage.
- **Sustained Note**: If a single sword is held for 10s without being released, it transforms into a Sustained Note — an autonomous minion that follows the player and fires slash waves every 2s. Max 1 Sustained Note.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FermataSwordHover.fx` | Hovering spectral sword body | Ghost sword — semi-transparent with cosmic fill. Pulsing glow that intensifies with hold time. Crimson-pink base → stellar white at max charge. Subtle rotation oscillation. |
| `FermataSlashWave.fx` | Slash wave projectile from sword release | Crescent-arc shape (like a blade wave). Cosmic energy body with nebula scroll. Pink-crimson core → void edge. Fast-moving, brief afterimage. |
| `HarmonicAlignmentLines.fx` | Energy lines connecting aligned swords | Thin beam strips between sword positions. Internal energy pulse traveling along line. Crimson-gold color. Brighter at midpoints. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SwordHoverGlintParticle | Orbits each hovering sword | Tiny crimson-gold sparkle, slow orbit, 30 frame life |
| SlashWaveTrailParticle | Trail behind slash wave projectile | Pink-crimson streak, 6 frame life |
| AlignmentPulseParticle | Travels along harmonic alignment lines | Gold pulse mote, moves between swords, 15 frame life |
| SustainedNoteAuraParticle | Ambient aura around sustained note minion | Cosmic shimmer particles, slow orbit, continuous, 20 frame life |

---

## 9. Symphony's End (Magic)

### Identity & Musical Soul
Where all melodies find their conclusion. This is the Fate theme's rapid-fire magic weapon — a torrential barrage of spectral blades corkscrewing toward the cursor. Where The Final Fermata is deliberate and strategic, Symphony's End is the **fortissimo cascade** — unrelenting, overwhelming, the final measures played at maximum intensity.

### Lore Line
*"The symphony does not fade. It ENDS."*

### Combat Mechanics (Existing: 500 damage, 8 useTime)
- **Spiraling Spectral Blades**: Primary fire — rapid-fire (8 useTime = very fast). Each shot fires a spectral blade that corkscrews toward the cursor (helical spiral path, not straight line).
- **Contact Shatter**: On contact with enemy, blade shatters into 4 fragments. Fragments fly in cardinal directions from impact, dealing 25% of main damage each.
- **Crescendo Mode**: After 3 seconds of continuous fire, enters Crescendo Mode — fire rate increases 50%, blades grow larger, fragments increase to 6. Visual: player gains cosmic aura, blades trail more intensely.
- **Diminuendo**: If you stop firing, there's a 2s "cooldown" period. During Diminuendo, accuracy decreases (blades wobble) but damage increases by 20% (slamming the brakes intensifies each hit).
- **Final Note**: If you fire for exactly 10s continuously then stop, the last blade is a Final Note — giant blade (5x size) that doesn't shatter but instead passes through all enemies and detonates at max range in a cosmic burst.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SpiralBladeBody.fx` | Corkscrewing spectral blade | Ghost blade with internal cosmic energy. Rotation driven by shader (blade visually spins on its axis during spiral flight). Trail: tight helix sparkles. Color: crimson → pink → stellar white core. |
| `FinalNoteDetonation.fx` | Giant final blade cosmic detonation | Massive expanding ring at max range. All Fate colors: void → crimson → pink → gold → white in concentric rings. Internal star burst. Screen shake. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| HelixTrailParticle | Helix pattern trail behind spiraling blade | Crimson-gold sparkle following helix path, 8 frame life |
| ShatterFragmentParticle | Brief flash at shatter point | Pink flash + 4 directional streaks, 4 frame burst |
| CrescendoAuraParticle | Cosmic aura around player during Crescendo Mode | Dark pink cosmic motes, orbiting player, 20 frame life |
| FinalNoteShockwaveParticle | Expanding ring from Final Note detonation | Gold-white ring, expanding, 15 frame life |

---

## 10. Destiny's Crescendo (Summon)

### Identity & Musical Soul
A crescendo is the building of volume and intensity — and this weapon summons a **Crescendo Deity** that embodies that escalation. The minion starts calm and grows more devastating over time, entering escalating phases of power. It builds from prayer to proclamation to divine wrath. The ultimate summoner weapon of the Fate theme.

### Lore Line
*"It began as a whisper. It will end as a decree from the stars themselves."*

### Combat Mechanics
- **Crescendo Deity Minion**: Summons a floating deity entity (cosmic being with constellation-pattern body) that attacks automatically.
- **Escalation Phases** (deity phases up every 15 seconds of combat):
  - **Phase 1 — Pianissimo** (0-15s): Fires single CrescendoCosmicBeam at nearest enemy. Slow rate of fire. Subtle visual presence.
  - **Phase 2 — Piano** (15-30s): Fires 2 beams in alternating targets. Faster rate. Deity grows slightly larger, brighter.
  - **Phase 3 — Forte** (30-45s): Fires 3 beams simultaneously. Spawns small orbiting star shields (block 1 projectile each). Deity noticeably larger.
  - **Phase 4 — Fortissimo** (45s+): Fires 5 beams in rapid succession. Orbiting shields become offensive (launch at enemies). Deity at full size with cosmic aura. 
- **Cosmic Beam**: CrescendoCosmicBeam — channeled beam that sweeps across enemies. Damage scales with phase. Visual intensity scales dramatically.
- **Crescendo Reset**: If the player takes heavy damage (>20% HP in one hit), deity resets to Phase 1. Must be protected.
- **Deity Presence**: The deity provides a passive effect based on phase: P1: +3% damage, P2: +5% damage + regen 1/s, P3: +8% damage + regen 2/s + 5 defense, P4: +12% damage + regen 3/s + 10 defense.

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CrescendoDeityBody.fx` | The deity entity body shader | Cosmic humanoid silhouette. Body made of constellation patterns (star points + lines). Color: void body → crimson constellation lines → gold star points. Body scale increases per phase. At Phase 4: full cosmic nebula fill with star map overlay. |
| `CrescendoCosmicBeam.fx` | The deity's beam attack | Channeled beam strip. Phase 1: thin, dim crimson. Phase 2: medium, brighter. Phase 3: wide, intense. Phase 4: massive, all Fate colors cycling, internal star burst pattern. UV-scroll speed tied to phase. |
| `CrescendoStarShield.fx` | Orbiting star shield around deity | SDF 4-point star with gold-white glow. Orbits deity. On breaking: shatters into sparks. At Phase 4: star transforms from shield to projectile (trail added). |
| `CrescendoPresenceAura.fx` | Passive aura around player based on deity phase | Subtle cosmic shimmer extending from player. Phase 1: barely visible. Phase 4: dramatic cosmic cloud with constellation lines extending from player. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| DeityConstellationGlintParticle | Twinkles at star points on deity body | Gold-white twinkle, periodic flash, 5 frame per twinkle |
| CosmicBeamEdgeParticle | Sheds from beam edges | Crimson-pink sparks, perpendicular drift, 6 frame life |
| StarShieldOrbitParticle | Trail behind orbiting star shields | Gold sparkle, tight orbit trail, 8 frame life |
| PhaseTransitionBurstParticle | Burst when deity escalates phase | All Fate colors in expanding ring, 10-15 particles, 10 frame burst |
| DeityResetFlashParticle | Flash burst when deity resets to Phase 1 | Dark red flash + falling star particles (sad visual), 15 frame life |

---

## Cross-Theme Synergy Notes

### Fate Theme Unity
All weapons share cosmic void + crimson + pink + stellar white palette with celestial/destiny motifs:
- **Requiem of Reality**: Foundational — cosmic tears and spectral blades
- **Conductor's Last Constellation**: Star map charging + devastating beam release
- **Coda of Annihilation**: Zenith-style multi-sword + gravitational singularity
- **Opus Ultima**: THE masterwork — 3-movement symphony of cosmic destruction
- **Fractal of the Stars**: Recursive fractal geometry + orbital mechanics
- **Resonance of a Bygone Reality**: Temporal echoes + dual fire mode (melee+ranged fusion)
- **Light of the Future**: Acceleration mechanics + future-sight targeting
- **The Final Fermata**: Strategic placement + dramatic simultaneous release
- **Symphony's End**: Overwhelming rapid-fire with crescendo escalation
- **Destiny's Crescendo**: Phase-escalating deity with crescendo power buildup

### Power Hierarchy
Fate is an endgame theme. Weapons are ordered by power:
1. **Opus Ultima** (THE weapon — 720 damage, 3-movement system)
2. **Fractal of the Stars** (850 damage, recursive destruction)
3. **Coda of Annihilation** (Zenith-style pinnacle)
4. **Destiny's Crescendo** (escalating deity)
5. **The remaining weapons** (each powerful in their niche)

### Visual Escalation
Fate weapons should feel increasingly cosmic and dramatic:
- Lower-tier Fate weapons: Crimson energy, star sparkles, subtle cosmic FX
- Mid-tier: Constellation patterns, nebula trails, screen-edge effects
- Top-tier (Opus Ultima, Coda): Full screen distortions, gravitational lensing, cosmic singularities, multi-layer shader-driven effects that command the entire screen

### Musical Motifs
- **Musical structure as combat structure**: Exposition/Development/Recapitulation (Opus Ultima), Crescendo phases (Destiny's Crescendo), Fermata hold-and-release (Final Fermata), Coda finale (Coda of Annihilation)
- **Conductor imagery**: The player is the conductor. Weapons respond to their timing, their rhythm, their intent.
- **Constellation geometry**: Star maps, connected star points, geometric patterns in the night sky
- **Destiny/inevitability**: Effects feel cosmic and unavoidable — gravitational pulls, homing projectiles, cascading chain reactions
