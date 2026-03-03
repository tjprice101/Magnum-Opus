# 👁️ Enigma Variations — Resonance Weapons Planning

> *"The unknowable mystery, shrouded in dread and void."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Elgar's Enigma Variations — unknown theme, layered mystery |
| **Emotional Core** | Mystery, dread, arcane secrets |
| **Color Palette** | Void black, deep purple, eerie green flame |
| **Palette Hex** | Void Black `(10, 5, 20)` → Deep Void Purple `(60, 20, 100)` → Enigma Green `(40, 200, 80)` → Spectral Teal `(80, 255, 160)` → Eerie White `(200, 230, 220)` |
| **Lore Color** | `new Color(140, 60, 200)` — Void Purple |
| **Lore Keywords** | Enigma, void, cipher, unknown, watching eyes, riddles, paradox, silence |
| **VFX Language** | Watching eyes opening, void rifts, green-flame ciphers, paradox glitches, dimensional tears, question marks dissolving into darkness |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | The Unresolved Cadence | Melee | Inevitability stacking → Paradox Collapse at 10 stacks |
| 2 | Variations of the Void | Melee | Every 3rd strike summons converging void beams |
| 3 | Dissonance of Secrets | Magic | Void dissonance spells |
| 4 | Fugue of the Unknown | Magic | Echoing fugue projectiles that replay themselves |
| 5 | Cipher Nocturne | Magic | Encrypted cipher spells with decode mechanics |
| 6 | The Silent Measure | Ranged | Silence-themed precision shots |
| 7 | Tacet's Enigma | Ranged | Musical tacet (rest) themed attacks from silence |
| 8 | The Watching Refrain | Summon | Watching eye minion with repeating refrains |

---

## 1. The Unresolved Cadence (Melee)

### Identity & Musical Soul
An unresolved cadence in music is a chord progression that doesn't resolve — it leaves the listener hanging, waiting for something that never comes. This sword embodies that **eternal tension**. Every swing adds to the mystery, stacking Inevitability on everything it touches. At 10 stacks, the tension breaks — Paradox Collapse tears reality around the target. The weapon should feel *wrong* in the best way — slightly glitchy, reality-bending, like the world can't quite decide if the blade is actually there.

### Lore Line
*"The question was never meant to have an answer."*

### Combat Mechanics (Partially Implemented — extends MeleeSwingItemBase)
- **3-Phase Combo** (via MeleeSwingItemBase):
  - **Phase 1 — The Question**: Diagonal slash. Applies 2 Inevitability stacks. Spawns DimensionalSlash sub-projectile.
  - **Phase 2 — The Doubt**: Cross-slash (X pattern). Applies 3 stacks. Two DimensionalSlash projectiles in X pattern.
  - **Phase 3 — The Silence**: Heavy downward slam. Applies 5 stacks. Large DimensionalSlash + ParadoxBrand detonation.
- **Inevitability Stacking**: Each swing applies stacks to hit enemies (visible as eerie green-purple rings). At 10 stacks → Paradox Collapse: massive void implosion centered on target.
- **Paradox Brand**: On-hit special — brands target with a glowing eye glyph. Branded enemies take +20% damage from all Enigma weapons. Brand lasts 8 seconds.
- **Seeking Crystals**: On hit, spawns 2 void-green seeking crystals that home on branded enemies.

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CadenceSlashArc.fx` | Swing trail with reality-glitch effect | UV-scrolled trail with intentional "glitch" artifacts — thin horizontal scan lines, brief UV offset jumps, color channel splitting (chromatic aberration). Deep void purple core → eerie green edge → black. |
| `InevitabilityRings.fx` | Stack indicator rings on enemy | Concentric SDF rings around target. Each ring has subtle green-flame noise at edges. Ring count = stack count. Rings rotate in alternating directions (clockwise/counterclockwise). At 10 stacks, rings pulse rapidly before collapse. |
| `ParadoxCollapse.fx` | 10-stack void implosion | Screen-space distortion shader — pixels are pulled INWARD toward center point (inverse lens distortion). Color: deep void purple → eerie green → white flash at center. Brief (0.5s) then rapid expansion outward. Chromatic aberration during expansion. |
| `DimensionalSlashTear.fx` | DimensionalSlash projectile body | Thin vertical "tear" in reality — black center with green-purple edges that shimmer. The "inside" of the tear shows UV-scrolled void texture (cosmic noise). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| VoidGlitchParticle | Brief flash at random positions near blade during swing | Small rectangular glitch artifacts (2x8px), green or purple, random orientation, 3-5 frame life |
| InevitabilityEyeParticle | Opens and closes at branded enemy position | Eye glyph shape (from Particle Library), green pupil, slow blink animation, stationary on target |
| ParadoxCollapseDebrisParticle | Pulled inward during collapse, then burst outward | Void purple shards, strong inward velocity then reversal, 20-30 particles |
| SeekingCrystalTrailParticle | Trail behind seeking crystal projectiles | Green sparkle trail, 2-3px, 8 frame life |
| DimensionalTearSparkParticle | Sheds from dimensional slash edges | Green-purple sparks that flicker between colors, 10 frame life |

#### Bloom Layers
1. **Blade glitch aura**: Intermittent bloom that flickers on/off (2-frame randomized intervals, eerie green)
2. **Inevitability rings**: Subtle green glow around stacked rings on enemy (grows with stacks)
3. **Paradox Collapse center**: 4-layer bloom (tiny white → small green → medium purple → wide void black with inverted glow)
4. **Dimensional slash**: Thin green-purple glow at tear edges (additive)
5. **Paradox Brand eye**: Soft green glow behind eye glyph on branded enemies

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Void trail texture | `Assets/EnigmaVariations/TheUnresolvedCadence/Trails/VoidGlitch.png` | "Horizontal energy trail with digital glitch artifacts, deep purple and eerie green, scan line distortions, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Dimensional tear texture | `Assets/EnigmaVariations/TheUnresolvedCadence/Trails/DimensionalTear.png` | "Vertical reality tear texture, black void center with shimmering green-purple edges, dark portal look, on solid black background, 32x256px --ar 1:8 --style raw" |
| Paradox collapse mask | `Assets/EnigmaVariations/TheUnresolvedCadence/Orbs/ParadoxCollapse.png` | "Circular void implosion texture, dark center pulling inward with eerie green and purple radiation at edges, on solid black background, 256x256px --ar 1:1 --style raw" |
| Inevitability ring | `Assets/EnigmaVariations/TheUnresolvedCadence/Orbs/InevitabilityRing.png` | "Thin eerie ringof green-purple energy with tiny flame wisps at edge, circular, on solid black background, 128x128px --ar 1:1 --style raw" |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| InevitabilityStack | Stacking (1-10). At 10, triggers Paradox Collapse. Visual: green rings. | 300 frames per stack (5s) |
| ParadoxBrand | +20% damage from Enigma weapons. Visual: eye glyph. | 480 frames (8s) |
| DimensionalDissonance | -10% defense. Applied by Paradox Collapse. | 180 frames |

---

## 2. Variations of the Void (Melee)

### Identity & Musical Soul
Each variation in Elgar's piece is a portrait of a friend — but hidden, encrypted. This sword creates **variations of void** — every third slash, void manifestations appear where the blade passed moments ago, replaying the attack from different angles. Like echoes of a theme, each variation is subtly different from the original.

### Lore Line
*"Every void remembers the shape of what it consumed."*

### Combat Mechanics (Partially Implemented — extends MeleeSwingItemBase)
- **3-Phase Combo** (via MeleeSwingItemBase):
  - **Phase 1**: Horizontal sweep + void echo spawns 0.5s later from opposite direction.
  - **Phase 2**: Upward diagonal + two void echoes from flanking angles.
  - **Phase 3**: Heavy slam + three void beams converge on slam point.
- **Void Beam Convergence**: Three void beams (summoned on 3rd strike) travel from different directions toward the slam point. If all three converge simultaneously (within 0.3s), they create a Resonance Field — AoE that persists 2s, dealing continuous damage.
- **Paradox Brand Application**: All Enigma melee weapons apply Paradox Brand on hit.
- **Variation Memory**: The weapon remembers the last 3 swing directions. If the player varies their swing direction (doesn't aim the same way twice), a Variation bonus spawns an extra void echo per combo.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `VoidEchoSlash.fx` | Void echo afterimage swing | Ghostly version of the original slash trail — color-shifted to deep purple → eerie green, alpha-faded, slight UV distortion. Appears 0.5s after original swing with deliberate desync. |
| `VoidBeamConverge.fx` | Converging void beams | Thin beam strip shader with internal void-noise scroll. Dark purple body → green edge glow. Beam narrows as it approaches convergence point. |
| `VoidResonanceField.fx` | Persistent AoE from beam convergence | Circular SDF with internal void-pattern rotation (Voronoi noise). Dark purple field → green highlights at Voronoi cell edges. Slow rotation. Gentle pulse. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| VoidEchoFlickerParticle | Appears briefly at echo slash start point | Rectangular glitch, purple-green, 3 frame blink |
| VoidBeamSparkParticle | Sheds from beam edges during travel | Green-purple sparks, perpendicular drift, 8 frame life |
| ResonanceFieldMoteParticle | Rises from resonance field AoE | Dark purple motes, slow upward drift, 20-30 frame life |
| VariationBonusFlashParticle | Brief flash when Variation bonus activates | Green flash + exclamation glyph, 5 frame burst |

---

## 3. Dissonance of Secrets (Magic)

### Identity & Musical Soul
Dissonance is the **clash of unresolved harmony** — two notes that refuse to agree. This magic weapon fires projectiles that exist in two states simultaneously, splitting and recombining unpredictably. Enemies hit feel both the question and its absence. Secrets are encoded in every spell.

### Lore Line
*"The truth and the lie travel side by side. Which strikes first is the secret."*

### Combat Mechanics
- **Dual State Projectile**: Primary fire — fires a void orb that oscillates between two states (Phase A: green, fast, pierce | Phase B: purple, slow, explosive). State changes every 0.5s automatically.
- **Secret Encoding**: Holding fire for 1s before release "encodes" the projectile to stay in its current state permanently (player can lock Phase A or Phase B).
- **Dissonant Clash**: If a Phase A projectile and Phase B projectile from the same player collide, they create a Dissonant Clash — a massive AoE vortex that pulls enemies inward (2s).
- **Whispered Secrets**: Every 10th projectile fired is auto-encoded as a "Secret" — invisible to enemies, double damage, no trail. Surprise attack.
- **Harmonic Lock**: If you alternate Phase A and Phase B kills (A, B, A, B...) for 6 kills in sequence, a Harmonic Lock activates — next 5s of projectiles deal +40% damage.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `DualStateOrb.fx` | Oscillating projectile body | SDF circle with internal state color — lerps between green and purple based on state parameter. Internal noise swirl direction reverses on state change. Brief flash at transition moment. |
| `DissonantVortex.fx` | Collision clash vortex AoE | Radial UV-scroll with inward-spiraling distortion. Purple-green interleaved spiral arms. Screen distortion at center. 2s duration with slow fade. |
| `SecretProjectile.fx` | Invisible "Secret" projectile with subtle shimmer | Nearly invisible (alpha: 0.05). Slight heat-haze-like screen distortion around it. On hit: sudden full-visibility flash before impact. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PhaseTransitionParticle | Burst at projectile when state changes | Small sparks — green or purple depending on direction of change, 5-8 per transition |
| DissonantVortexDebrisParticle | Spirals inward toward vortex center | Dark purple-green debris, orbiting, consumed at center, 15-30 frame life |
| SecretRevealParticle | Flash burst when "Secret" projectile becomes visible on hit | Bright green flash + sparkle ring, 3 frame burst |
| HarmonicLockAuraParticle | Orbits player during Harmonic Lock active | Green-purple alternating sparkle dots, 6-8 orbiting |

---

## 4. Fugue of the Unknown (Magic)

### Identity & Musical Soul
A fugue is a musical form where a theme is introduced then repeated, layered, inverted, and transformed across multiple voices. This weapon fires projectiles that **replay themselves** — each spell echoes 1-2 times from different positions, creating a layered fugue of destruction. The unknown element: echoes sometimes manifest in unexpected ways.

### Lore Line
*"The first note was a question. The echoes were answers in a language no one speaks."*

### Combat Mechanics
- **Fugue Bolt**: Primary fire — fires a void bolt. After 0.5s, an Echo of the bolt replays from a position 45° offset. After 1.0s, a second Echo replays from the opposite 45° offset.
- **Subject & Answer**: The original bolt is the Subject. Echoes are the Answer. If Subject and Answers all hit the same target, they trigger Fugue Resolution — massive damage burst + stun 1s.
- **Inversion**: Alt fire — fires an Inverted Fugue Bolt (travels in the opposite direction from aim, then U-turns toward cursor). Echoes also invert. Creates unpredictable attack patterns.
- **Stretto**: When all 3 bolts (Subject + 2 Answers) are airborne simultaneously, they enter Stretto — their trails connect visually with energy lines, forming a triangle of void power. Enemies inside the triangle take continuous damage.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FugueBoltTrail.fx` | Bolt trail with musical resonance | Strip trail, UV-scroll. Color: deep purple (Subject) or eerie green (Answer). Internal wave pattern overlay (standing wave nodes visible). |
| `FugueResolution.fx` | Resolution burst when all 3 bolts converge | Expanding triple-layered ring (one per bolt overlapping). Purple + green + white. Center flash. Musical note shapes scattered in burst. |
| `StrettoTriangle.fx` | Energy triangle connecting 3 airborne bolts | Line strip between 3 dynamic positions. Bright green edges with void-fill inside triangle area. Internal UV-scroll pattern. Alpha blend for area fill. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| FugueSubjectTrailParticle | Trail behind Subject bolt | Purple sparkle motes, 3-4px, 10 frame life |
| FugueAnswerTrailParticle | Trail behind Answer echoes | Green sparkle motes, slightly larger, 12 frame life |
| FugueResolutionNoteParticle | Scattered from resolution burst | Musical note shapes (eigth, quarter), alternating purple/green, radial burst |
| StrettoFieldSparks | Random sparkles inside Stretto triangle | Green-purple flicker sparks, stationary within triangle, 15 frame life |

---

## 5. Cipher Nocturne (Magic)

### Identity & Musical Soul
A cipher conceals a message within a message. This weapon fires spells that are **encrypted** — they appear as scrambled glyphs that "decode" on enemy contact, revealing their true devastating form. The Nocturne aspect means these secrets are revealed only in darkness. Enemies in shadow take more damage.

### Lore Line
*"Every glyph is a locked door. The key is always pain."*

### Combat Mechanics
- **Cipher Glyphs**: Primary fire — fires a scrambled glyph projectile (appears as rotating question mark/cipher symbols). On hitting an enemy, it "decodes" into one of 3 random effects:
  - **Decode A — Burning Cipher**: Explosion of green fire (AoE damage + DoT)
  - **Decode B — Binding Cipher**: Enemy is stunned for 0.8s + tethered to decode point
  - **Decode C — Recursive Cipher**: Spawns 3 smaller cipher glyphs that home on nearby enemies
- **Shadow Bonus**: Enemies in low-light areas (tile lighting < 50%) take +25% damage from decoded effects.
- **Cipher Sequence**: If you decode all 3 types (A→B→C in any order) within 5 seconds, you trigger Nocturne's Revelation — all enemies on screen are briefly "decoded" (marked with cipher glyphs that each detonate for medium damage).
- **Enigma's Eye**: Passive — while holding this weapon, you can see enemy HP bars and debuff timers (like a magical scanner).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CipherGlyphScramble.fx` | Rotating scrambled glyph projectile | SDF glyph/question-mark shape with UV distortion that "scrambles" the appearance (high-frequency noise displacement). Green-purple shift. Rotation speed increases as it approaches target. |
| `CipherDecode.fx` | Decode flash at hit moment | Brief (0.2s) shader effect: the scrambled glyph "snaps" into clarity (noise displacement lerps to 0), then bursts into the decoded effect. Satisfying reveal moment. |
| `NocturneRevelation.fx` | Screen-wide decode event | Screen-space shader that briefly makes all enemies pulse with cipher glyph outlines. Dark overlay + green cipher patterns appearing and detonating. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ScrambledGlyphParticle | Orbits cipher projectile, spinning | Symbol/glyph shapes (question marks, eyes, cipher letters), purple-green, 15 frame life |
| DecodeFlashParticle | Flash burst at decode moment | White-green flash ring expanding, 5 frame burst |
| BurningCipherFlameParticle | Rises from Decode A green fire | Eerie green flames, 5-8px, upward drift, 20 frame life |
| RecursiveCipherSpawnParticle | Brief flash at Decode C spawn points | Green sparkle burst, 3 frame, small |

---

## 6. The Silent Measure (Ranged)

### Identity & Musical Soul
In music, a measure (bar) of silence is powerful — it creates anticipation. This weapon embodies **the power of the rest** — the silent beat between notes. Shots are fired with eerie precision, each one preceded by a beat of perfect silence. The weapon penalizes hasty shooting and rewards measured, rhythmic firing.

### Lore Line
*"The most terrifying sound is the one that comes after silence."*

### Combat Mechanics
- **Measured Shot**: Primary fire — high-damage single shot. However, damage scales with how long you waited since last shot (min 0.5s). At 2s wait: 1.5x damage. At 4s wait: 2.5x damage. Visual: weapon glows brighter the longer you wait.
- **Silent Mark**: Shots apply Silent Mark — marked targets cannot make sounds (suppresses enemy attack SFX, purely atmospheric). Visual: crossed-out speaker glyph on target.
- **Measure Rhythm**: If the player fires in a consistent rhythm (shots spaced at equal intervals ±0.2s), they build Tempo (max 5 stacks). Each Tempo stack: +5% fire rate + shot leaves void trail. At 5 Tempo stacks, shots pierce infinitely.
- **Grand Pause**: Alt fire — creates an area of absolute silence (15 tile radius) for 3 seconds. All enemies in the area are slowed 30% and cannot use attack patterns that involve projectiles. Massive cooldown (30s).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SilentShotTrail.fx` | Precision shot trail (appears at 3+ Tempo) | Ultra-thin strip trail, void-purple core, eerie green edge. Fades rapidly. The trail itself is "quiet" — minimal visual noise. |
| `GrandPauseField.fx` | Silence AoE zone | Circular SDF with ripple pattern inside (like sound waves freezing). Color: very dark purple → transparent. Edge shimmer. Everything inside appears slightly desaturated. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| MeasureChargeGlintParticle | Appears on weapon during charged wait | Tiny green-purple sparkle, growing brighter with charge time, 1-2 particles |
| SilentMarkGlyphParticle | Stationary on marked enemies | Small muted-speaker glyph, purple, subtle pulse, 300 frame life (debuff duration) |
| TempoRhythmRingParticle | Expanding ring at each rhythmic shot | Thin green ring expanding from barrel, 8 frame life, matches firing tempo |
| GrandPauseSilenceParticle | Drifts inward inside silence zone | Dark motes moving toward center, very slow, creating "vacuum" feel |

---

## 7. Tacet's Enigma (Ranged)

### Identity & Musical Soul
"Tacet" is a musical direction meaning "be silent" — the performer is told to rest for an entire section. This weapon weaponizes *absence*. Shots spawn from **where the player ISN'T** — delayed phantom shots that appear from behind cover, from the player's previous position, or from thin air near enemies. Confusion is the weapon.

### Lore Line
*"Where silence gathers, the enigma strikes."*

### Combat Mechanics
- **Phantom Shot**: Primary fire — the player fires nothing visible. After 0.8s delay, a shot spawns at the player's position at the moment of firing (stored previous position). The phantom shot homes loosely toward the original target.
- **Position Echo**: The weapon remembers the last 3 positions where phantom shots spawned. Every 5th shot triggers Echo Cascade — all 3 stored positions fire simultaneously.
- **Void Snare**: Alt fire — deploys an invisible snare at cursor position. When an enemy steps on it (or after 5s), it detonates with a phantom barrage from 4 directions toward the snare center.
- **Tacet's Trick**: If the player stands completely still for 3s with this weapon, they become "Tacet" — invisible to enemies for 3s. First shot from Tacet deals 3x damage.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `PhantomShotReveal.fx` | Phantom shot materializing from nothing | Quick fade-in shader: starts as distorted void shimmer (screen distortion), sharpens into visible projectile over 0.2s. Green-purple color. Invisible → barely visible → fully visible. |
| `VoidSnareActivation.fx` | Snare detonation burst | Quick radial flash from center with 4 directional beam lines extending outward. Green-purple burst. Very brief (0.3s). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PhantomSpawnRippleParticle | Expanding ripple at phantom shot origin | Dark purple ring, thin, expanding, 10 frame life — marks "where silence struck" |
| TacetInvisibilityMoteParticle | Fades around player during Tacet state | Dark motes absorbing into player character, making them look like they're dissolving into shadow |
| EchoCascadeFlashParticle | Flash at each stored position during Echo Cascade | Green flash + phantom silhouette, 3 frame burst each |

---

## 8. The Watching Refrain (Summon)

### Identity & Musical Soul
A refrain is a repeated section — the part you can't escape. The Watching Refrain summons a **watching eye entity** — a void-eye minion that stares at enemies, marking them with its gaze. The eye attacks in repeating patterns (refrains), and its power grows the longer it watches a single target. Unsettling, patient, inevitable.

### Lore Line
*"It does not blink. It does not forget. It simply watches you unravel."*

### Combat Mechanics
- **Watching Eye Minion**: Summons a floating void eye that hovers near the player. The eye fixates on the nearest enemy and begins its Refrain cycle.
- **Refrain Attack Pattern** (repeating every 8 seconds):
  - **Beat 1-2**: Eye stares (marks target with Watched debuff, +10% damage taken)
  - **Beat 3-4**: Eye fires 2 void bolt projectiles
  - **Beat 5-6**: Eye fires 3 void bolts + eye glyph projectile that orbits target
  - **Beat 7-8**: Eye fires concentrated void beam (2s channeled) at target
- **Watched Debuff Stacking**: The longer the eye stares at one target without interruption, the Watched debuff stacks (max 5). Each stack: +10% damage taken, +1 extra void bolt per refrain cycle.
- **The Inevitable Stare**: If all 5 Watched stacks accumulate on one target, the eye triggers Inevitable Stare — a 0.5s freeze-frame effect where the eye opens completely, then fires a devastating single-target beam (5x base damage).
- **Blinking**: If the target dies or moves off-screen, the eye "blinks" (0.5s close animation) and resets Watched stacks. This is the only way to escape the escalation.

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WatchingEyeEntity.fx` | Eye minion body shader | SDF circle (iris) with inner SDF circle (pupil). Pupil tracks toward target position (offset by gaze direction). Green-purple iris gradient. Pulsing glow. Eye opens wider as Watched stacks increase. |
| `WatchedMarkGlow.fx` | Watched debuff indicator on enemy | Circular SDF ring around enemy that narrows (tightens) as stacks increase. Green → purple color shift with stacks. At max stacks, ring closes into solid circle before Inevitable Stare. |
| `InevitableStareBeam.fx` | Devastating max-stack beam | Wide beam from eye to target. Internal eye-pattern texture scroll. Deep purple body → green edge → white core stream. Screen shake during channel. |
| `WatchingEyeAura.fx` | Ambient aura around eye entity | Irregular radial glow. Color shifts between green and purple. Tentacle-like wisps extending from eye toward gazed target (Bezier curves with noise). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| WatchingGazeLineParticle | Connects eye to target as thin line | Very faint green-purple dotted line between eye and target, subtle |
| VoidBoltTrailParticle | Trail behind void bolt attacks | Green-purple sparkle trail, 3-4px, 8 frame life |
| EyeGlyphOrbitParticle | Orbits target created by glyph projectile | Small eye glyph shapes, tight orbit, slow rotation, 120 frame life |
| InevitableStareChargeParticle | Spirals into eye during charge | Bright green motes from all directions, converging on eye center |
| BlinkResetParticle | Burst when eye blinks/resets | Purple flash + brief "eyelid" closing animation (arc shape), 10 frame burst |

---

## Cross-Theme Synergy Notes

### Enigma Variations Theme Unity
All weapons share void black + deep purple + eerie green palette with mystery/cipher motifs:
- **The Unresolved Cadence**: Unstoppable stacking culminating in reality collapse
- **Variations of the Void**: Echoing replays from alternate angles
- **Dissonance of Secrets**: Dual-state spells that clash
- **Fugue of the Unknown**: Self-replaying layered fugue spells
- **Cipher Nocturne**: Encrypted spells that decode on impact
- **The Silent Measure**: Power from patience and rhythm
- **Tacet's Enigma**: Attacks from absence and delay
- **The Watching Refrain**: Inevitable escalation from observation

### Musical Motifs
- **Paradox/duality** — many weapons have dual-state mechanics (Phase A/B, Subject/Answer, encoded/decoded)
- **Stacking inevitability** — most weapons reward patience and accumulation over time
- **Absence as weapon** — silence, invisibility, delayed attacks, phantom shots
- **Eye/watching motifs** — the unresolved mystery watching from the void
- **Green flame** is the signature VFX element — every weapon should use eerie green fire/glow somewhere
- **Glitch aesthetics** — chromatic aberration, UV distortion, scan lines, reality tears distinguish Enigma from all other themes
