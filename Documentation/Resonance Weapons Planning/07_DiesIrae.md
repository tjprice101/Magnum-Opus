# 🔥 Dies Irae — Resonance Weapons Planning

> *"Day of wrath — the world consumed in fury and fire."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Verdi's Dies Irae from the Requiem — primal fury, divine judgment, apocalyptic wrath |
| **Emotional Core** | Fury, judgment, apocalyptic power |
| **Color Palette** | Blood red, dark crimson, ember orange |
| **Palette Hex** | Char Black `(20, 5, 5)` → Blood Crimson `(140, 15, 15)` → Wrath Red `(220, 40, 20)` → Ember Orange `(255, 120, 30)` → Judgment Gold `(255, 200, 80)` → Hellfire White `(255, 240, 220)` |
| **Lore Color** | `new Color(200, 50, 30)` — Blood Red |
| **Lore Keywords** | Wrath, judgment, sin, condemnation, fire, chain, verdict, damnation, penance, inferno |
| **VFX Language** | Raging flames, chains of judgment, verdict glyphs, crystallized wrath, eclipse darkness with fire edges, molten cracks, infernal sigils |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Wrath's Cleaver | Melee | Crystallized flame projectiles + wrath buildup |
| 2 | Executioner's Verdict | Melee | Judgment-themed heavy strikes + verdict system |
| 3 | Chain of Judgment | Melee | Chain-whip with binding judgment chains |
| 4 | Sin Collector | Ranged | Sin-absorbing bullets that collect enemy power |
| 5 | Damnation's Cannon | Ranged | Heavy cannon lobbing ignited wrath balls |
| 6 | Arbiter's Sentence | Ranged | Judgment flame precision shots |
| 7 | Staff of Final Judgement | Magic | Floating ignition projectiles (fire mines) |
| 8 | Grimoire of Condemnation | Magic | Dark condemnation spells |
| 9 | Eclipse of Wrath | Magic | Eclipse orbs splitting into wrath shards |
| 10 | Death Tolling Bell | Summon | Bell minion with toll wave attacks |
| 11 | Harmony of Judgement | Summon | Judgment sigil minion |
| 12 | Wrathful Contract | Summon | Wrath demon bound by contract |

---

## 1. Wrath's Cleaver (Melee)

### Identity & Musical Soul
Dies Irae opens with a thunderous, wrathful declaration — this cleaver IS that opening. Raw, brutal, unrefined wrath condensed into a blade. Every swing cleaves the air and leaves crystallized flame in its wake — wrath so intense it solidifies. The weapon doesn't ask questions. It passes judgment through violence.

### Lore Line
*"The first blow of wrath is always the loudest."*

### Combat Mechanics
- **3-Phase Wrath Combo**:
  - **Phase 1 — Accusation**: Heavy horizontal cleave. Spawns 2 WrathCrystallizedFlame projectiles that arc slightly upward then crash down. On-hit: enemies ignited with Wrathfire (DoT: 20 damage/s for 4s).
  - **Phase 2 — Conviction**: Overhead slam creating ground eruption. Spawns 4 crystallized flames in a spread pattern. On-hit: Wrathfire stacks to Tier 2 (40 damage/s).
  - **Phase 3 — Execution**: Spinning cleave (270° arc). Spawns 6 crystallized flames in a ring around player. On-hit: Wrathfire stacks to Tier 3 (60 damage/s + -20% enemy healing).
- **Wrath Meter**: Each hit builds Wrath (max 100). At 100 Wrath, next swing becomes **Wrath Unleashed** — massive cleave with double range, spawning a line of crystallized flames that travel forward like a flame carpet. Wrath decays at 5/s when not hitting.
- **Crystallized Flame**: Sub-projectile behavior — arcs through air, sticks to ground on landing, persists 3s as ground hazard dealing continuous damage. Creates a battlefield of fire zones.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WrathCleaverTrail.fx` | Heavy swing trail with ember edges | Wide trail strip — core: dark crimson. Body: blood red with rolling FBM noise scroll (fire texture). Edge: ember orange sparks breaking off. Heavy, brutish, thick trail. Not elegant — raw and angry. |
| `CrystallizedFlameBody.fx` | Crystallized flame projectile + ground hazard | Faceted crystal shape (SDF polygon) filled with internal flame scroll. Ember orange → blood crimson. Ground state: stationary with flame wisps rising, slow pulsing. |
| `WrathUnleashedFloor.fx` | Flame carpet from Wrath Unleashed | Ground-level strip effect, fire crawling forward. Internal FBM noise scroll (fast). Blood red → ember → gold at leading edge. Cracks in ground (dark lines) under the fire. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| EmberShedParticle | Sheds from swing trail edges | Orange-red embers, gravity-affected arc upward then down, 15 frame life |
| CrystalizedFlameWispParticle | Rises from grounded crystallized flames | Thin flame wisp, ember-red, upward drift, 20 frame life |
| WrathMeterPulseParticle | Pulses around player at high Wrath | Crimson pulse ring, frequency increases with Wrath level |
| WrathUnleashedGroundCrackParticle | Appears in "cracked ground" under flame carpet | Dark fissure lines with ember glow inside, stationary, 60 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Wrathfire Tier 1 | 20 damage/s. Visual: ember particles. | 240 frames (4s) |
| Wrathfire Tier 2 | 40 damage/s. Visual: more ember + crimson flames. | 240 frames (4s) |
| Wrathfire Tier 3 | 60 damage/s + -20% enemy healing. Visual: intense flame engulfing. | 300 frames (5s) |

---

## 2. Executioner's Verdict (Melee)

### Identity & Musical Soul
The executioner — impartial, inevitable, final. This weapon doesn't rage; it **judges**. Methodical heavy strikes that mark enemies with judgment glyphs. When the verdict is sealed, the punishment is absolute. Controlled wrath, channeled through ritual and ceremony.

### Lore Line
*"The verdict was written before you were born."*

### Combat Mechanics
- **3-Phase Judgment Combo**:
  - **Phase 1 — Arraignment**: Overhead strike. Applies Judgment Mark (glyph appears on enemy).
  - **Phase 2 — Cross-Examination**: Cross-slash (X pattern). Marked enemies take +25% damage. Applies second Mark.
  - **Phase 3 — The Verdict**: Horizontal execution slash. Double-marked enemies trigger Verdict Execution (massive damage burst + brief stun).
- **Verdict System**: Enemies can accumulate up to 3 Judgment Marks. At 3 marks: automatic Execution regardless of Phase 3. Marks are visible as crimson glyph layers around the enemy.
- **Executioner's Eye**: While holding this weapon, the player can see enemy HP percentage (displayed as crimson bar). Enemies below 25% HP take +50% damage (Execute threshold).
- **Ritual Precision**: If all 3 phases hit the same target without missing, the combo resets instantly (no cooldown). Rewards focused single-target play.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `VerdictSlashTrail.fx` | Heavy methodical slash trail | Thick trail strip — less fiery than Wrath's Cleaver, more deliberate. Blood crimson core with sharp edge (no feathered edge — clean cut). Gold glyph pattern embedded in trail texture. |
| `JudgmentMarkGlyph.fx` | Glyph displayed on marked enemies | SDF circle with internal judgment sigil pattern (scales, gavel, or abstract rune). Crimson-gold. Layers: 1 mark = outer ring, 2 marks = ring + inner glyph, 3 marks = full sigil glowing intensely with radial energy. |
| `VerdictExecutionFlash.fx` | Execution burst at 3 marks | Screen-space brief flash: heavy directional slash line across target + expanding crimson-gold ring. Dark background flash (brief screen dim). Deliberate, not explosive — a clean execution. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| JudgmentGlyphChipParticle | Sheds from judgment marks when stacking | Small crimson glyph fragment, drifts down, 10 frame life |
| VerdictSlashSparkParticle | Sparks along verdict slash trail | Blood red sparks, directional, 6 frame life |
| ExecutionFlashParticle | Burst from Verdict Execution | Gold-crimson flash ring, brief, 4 frame burst |
| ExecuteThresholdGlintParticle | Subtle glow on low-HP enemies | Faint crimson glint on enemies below 25% HP |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Judgment Mark | Stacking (1-3). +25% damage taken per mark from this weapon. At 3: Execution triggers. | 360 frames (6s) per mark |
| Verdict Stunned | Brief stun from Execution. | 60 frames (1s) |

---

## 3. Chain of Judgment (Melee)

### Identity & Musical Soul
Chains bind sinners to their fate — this weapon IS that chain. A chain-whip melee weapon with extended reach that latches onto enemies, binding them. The chains are judgment incarnate — once bound, the target cannot escape their sentence. Unique whip-like melee behavior.

### Lore Line
*"No sinner escapes the chain. It finds them in the dark."*

### Combat Mechanics
- **Chain Whip Swing**: Primary attack — sweeping chain whip with extended reach (12 tiles). JudgmentChainProjectile travels in an arc, damaging and latching to the first enemy hit.
- **Binding Chain**: On latch: enemy is tethered to the chain for 3s. Tethered enemies cannot move beyond the chain's length from the player. The player can swing the tethered enemy into other enemies for collision damage.
- **Judgment Pull**: Alt fire while enemy is bound — player yanks the chain, pulling the bound enemy toward them. Enemy takes damage from the pull + slams into other enemies in its path.
- **Chain Link Stacking**: Rapid consecutive hits build Chain Links on the target. At 5 Chain Links: the enemy is Fully Bound — immobilized for 2s, takes +30% damage. Fully Bound enemies who die burst into chain shrapnel (damages nearby enemies).
- **Chain Lightning**: At full combo, the chain crackles with wrathful energy — next hit causes chain lightning to arc from the target to up to 3 nearby enemies (prioritizes bound enemies).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `JudgmentChainBody.fx` | Chain projectile link segments | Chain rendered as connected segments (not smooth trail — visible individual links). Dark iron base with ember-orange glow between links. When binding: links tighten visually, glow intensifies. |
| `ChainLightningArc.fx` | Chain lightning effect between enemies | Branching lightning bolt between positions. Color: ember orange → white at branch points. Jagged path with slight randomization per frame (not static). Brief (0.3s). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ChainEmberParticle | Sheds from chain links during swing | Orange-red embers, 5 frame life, scattered along chain path |
| BindingTightenParticle | Appears when chains tighten on enemy | Small metal spark, white-orange, 3 frame burst |
| ChainShrapnelParticle | Bursts from Fully Bound enemy death | Dark metal shard particles, radial burst, 10-15 per death, 12 frame life |
| ChainLightningSpark | Sparks at each lightning contact point | Bright orange-white flash, 2 frame burst |

---

## 4. Sin Collector (Ranged)

### Identity & Musical Soul
Every enemy carries sins — this weapon **collects** them. Each bullet absorbs a fragment of the enemy's essence. Collected sins power the weapon, creating an escalating feedback loop. The more you kill, the more sinful the weapon becomes, the more devastating it grows.

### Lore Line
*"Your sins are not forgiven. They are collected."*

### Combat Mechanics
- **Sin Bullet**: Primary fire — SinBulletProjectile. Standard shot. On hit, absorbs a Sin Fragment from the enemy (visual: small dark essence pulled from target into player).
- **Sin Collection**: Each absorbed fragment adds to the Sin Counter (visible as a corruption meter on the player, max 30). Each Sin Fragment: +2% damage, +1% crit.
- **Sin Expenditure**: At 10+ Sins, alt fire consumes all collected Sins for a powerful special shot:
  - 10-19 Sins: **Penance Shot** — piercing shot that applies Wrathfire
  - 20-29 Sins: **Absolution Shot** — explosive shot with 6-tile blast radius
  - 30 Sins: **Damnation Shot** — screen-wide beam that annihilates everything in a line. Resets Sin Counter to 0.
- **Cardinal Sins**: Killing 7 different enemy types while collecting Sins grants Cardinal Sin bonus — next expenditure is one tier higher (Penance → Absolution → Damnation → Super Damnation: Damnation + screen shake + persistent fire zones).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SinCorruptionMeter.fx` | Visual corruption overlay on player based on Sin count | Player sprite overlay: dark crimson veins spreading from weapon hand. At 10: subtle veins. At 20: prominent veins reaching torso. At 30: full corruption, player glows blood red with ember sparks. |
| `DamnationBeam.fx` | Full Damnation Shot screen-wide beam | Massive beam at 30 Sins. Dark crimson body → ember orange edges → hellfire white core. Internal screaming face patterns in noise texture (judgment faces). Screen distortion along beam edges. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SinFragmentAbsorbParticle | Pulled from killed enemy toward player | Dark crimson wisp, homing toward player, 15 frame life |
| PenanceShotTrailParticle | Trail behind Penance Shot | Blood red streak, 8 frame life |
| AbsolutionExplosionParticle | Radial burst from Absolution Shot | Ember-orange flash + shrapnel, 20 particles, 12 frame life |
| SinCounterPulseParticle | Ambient pulse near player at high Sin count | Dark crimson pulse, grows brighter with Sin level, continuous |

---

## 5. Damnation's Cannon (Ranged)

### Identity & Musical Soul
A cannon that fires condensed hellfire — each shot is a payload of divine punishment, lobbed in an arc like a mortar. The IgnitedWrathBall is slow but devastating, creating massive fire zones on impact. This is the artillery piece of judgment — slow, heavy, apocalyptic.

### Lore Line
*"This is not a weapon. This is a sentence."*

### Combat Mechanics
- **Ignited Wrath Ball**: Primary fire — IgnitedWrathBallProjectile. Arcing lobbed shot (gravity-affected). Slow travel speed. On impact: massive explosion (8-tile radius) + creates Hellfire Zone (persistent damage area, 5s).
- **Hellfire Zone**: Persistent ground fire that intensifies over time (damage increases by 20% each second). Standing in a Hellfire Zone grants Wrath's Blessing to the player: +15% damage for 5s.
- **Mortar Mode**: Holding fire charges the shot — longer hold = longer arc, more damage (max 3s charge for 2.5x). Charge visible as cannon glowing ember → gold → white.
- **Damnation Barrage**: After 3 direct hits (enemies must be center of explosion, not edge), unlocks Damnation Barrage — next shot is 5 rapid-fire mini wrath balls in quick succession (carpet bombing).
- **Scorched Earth**: Overlapping Hellfire Zones create Scorched Earth — double damage, any enemy entering is Condemned (cannot heal for 5s).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WrathBallArc.fx` | Flaming wrath ball projectile body | SDF sphere with internal fire scroll (FBM noise). Bright ember center → crimson outer → dark smoke trail. Intense glow. The ball itself should look like a condensed sun of wrath. |
| `HellfireZone.fx` | Persistent ground fire zone | Ground-level fire shader. Internal FBM noise scroll (fast, turbulent). Color: ember center → crimson edge → black smoke wisps. Pulsing brightness indicating damage increase. For Scorched Earth: add dark cracks and secondary color layer. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| WrathBallEmberTrailParticle | Dense trail behind wrath ball in flight | Ember particles, gravity-slightly-upward (heat), 10 frame life, dense |
| ExplosionDebrisParticle | Radial burst from impact explosion | Crimson-orange shrapnel, 20+ particles, 15 frame life |
| HellfireSmokeParticle | Rises from hellfire zone | Dark smoke wisps, slow upward drift, 25 frame life |
| ScorchedEarthCrackParticle | Appears in overlapping zones | Dark fissure lines with molten glow, stationary, 120 frame life |

---

## 6. Arbiter's Sentence (Ranged)

### Identity & Musical Soul
The arbiter delivers sentences with cold, burning precision. Each shot is a JudgmentFlame — a flame that burns with the weight of judgment. Not wildfire — focused, directed, punishing. This is the precision counterpart to Damnation's Cannon's area damage.

### Lore Line
*"The arbiter does not miss. The arbiter does not forgive."*

### Combat Mechanics
- **Judgment Flame Shot**: Primary fire — JudgmentFlameProjectile. Fast, accurate, moderate damage. On hit: applies Judgment Flame debuff (15 damage/s for 3s). Subsequent hits refresh and stack intensity (max 5 stacks: 75 damage/s).
- **Sentencing**: At 5 Judgment Flame stacks on a target, they are Sentenced — imprisoned in a brief cage of flame (1s root + 2x damage from next hit).
- **Arbiter's Focus**: Hitting the same target 5 times in a row without hitting anything else grants Arbiter's Focus — next 3 shots are homing + deal +40% damage. Visual: crosshair glyph appears on target.
- **Appeal Denied**: If a Sentenced enemy is hit during their imprisonment, the Sentence is "upheld" — stacks remain at 5 for another full Duration instead of resetting. Permanent lockdown if you maintain combo.
- **Final Judgment**: Killing a Sentenced enemy causes their remaining flame stacks to transfer to the nearest enemy (spreading the sentence).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `JudgmentFlameShot.fx` | Flame bullet trail | Thin, precise trail strip. Clean crimson-orange body (NOT messy fire — focused, directed). Gold edge highlights. Brief, fading fast. Precision aesthetic. |
| `SentenceCageFlame.fx` | Brief cage of flame imprisoning Sentenced enemy | SDF cage bars (vertical lines in a circle) made of flame. Internal fire scroll inside each bar. Crimson-gold color. Tightens briefly then fades. Paired with root effect. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| JudgmentFlameStackParticle | Small flames on enemy per stack (1-5 flames) | Ember-crimson flame wisps, 1 per stack, orbit enemy slowly |
| SentenceImprisonFlashParticle | Flash when Sentence activates | Gold-crimson cage flash, 5 frame burst |
| FlameTransferParticle | Travels from killed enemy to nearest target | Crimson flame wisp, fast homing toward target, 10 frame life |
| FocusCrosshairParticle | Appears on Arbiter's Focus target | Gold crosshair glyph, stationary on target, subtle pulse |

---

## 7. Staff of Final Judgement (Magic)

### Identity & Musical Soul
The staff delivers the final word — FloatingIgnitionProjectiles that serve as fire mines, hovering in place until enemies approach. Judgment lies in wait. The staff transforms the battlefield into a minefield of divine fire, punishing those who dare move.

### Lore Line
*"Judgment does not chase. Judgment waits."*

### Combat Mechanics
- **Floating Ignition**: Primary fire — places a FloatingIgnitionProjectile at cursor position. Ignition floats stationary, arming after 1s. When an enemy comes within 3 tile radius, it detonates in fire explosion.
- **Mine Network**: Up to 8 Ignitions can be placed at once. When one detonates, adjacent Ignitions within 5 tiles have their damage boosted +30% (chain reaction bonus). If 3+ detonate within 1s, triggers Judgment Storm — screen-wide fire rain for 2s.
- **Controlled Detonation**: Alt fire — manually detonates ALL placed Ignitions simultaneously. Useful for ambush setups.
- **Purgatory Field**: 4+ Ignitions placed in a rough line create a Purgatory Field between them — enemies passing through the line take continuous fire damage. Visual: fire connects between Ignitions.
- **Final Judgement**: When the player takes lethal damage while this staff is equipped, all Ignitions detonate simultaneously with 3x damage (one-time save per life, leaves player at 1 HP).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FloatingIgnitionBody.fx` | Hovering fire mine | SDF sphere with internal slow fire scroll. Unarmed: dim crimson glow. Armed: brighter with pulsing glow. Near-trigger: rapid pulse, ember orange → gold. |
| `PurgatoryFieldLine.fx` | Fire connection line between 4+ Ignitions | Line strip between Ignition positions. Internal fire scroll (UV along line). Crimson-orange fire body. Pulsing intensity. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| IgnitionHoverEmberParticle | Slow embers rising from hovering Ignition | Tiny ember, slow upward drift, 15 frame life |
| IgnitionDetonationParticle | Explosion burst from triggered mine | Crimson-orange burst, 15 particles, radial, 10 frame life |
| JudgmentStormRainParticle | Fire rain during Judgment Storm | Fire droplets falling from screen top, scattered, 20 frame life |
| PurgatoryFieldFlameParticle | Flame wisps along Purgatory Field line | Small flames along connection line, 12 frame life |

---

## 8. Grimoire of Condemnation (Magic)

### Identity & Musical Soul
A dark grimoire containing the records of every condemnation ever spoken. Opening it unleashes waves of dark judgment energy. This is the "dark magic" counterpart to the Staff's controlled placement — raw, channeled, continuous condemnation.

### Lore Line
*"Every name written in this book burns twice — once on the page, once in flesh."*

### Combat Mechanics
- **Condemnation Wave**: Primary fire — channels a continuous wave of dark crimson energy forward (cone shape, 10 tiles). Enemies caught in the wave take continuous damage + gradual slow.
- **Written in Flame**: Enemies killed by condemnation leave their name behind (cosmetic: floating crimson text "CONDEMNED" above kill point for 3s). Each written name powers the next condemnation cast (+5% damage, max 10 names = +50%).
- **Dark Sermon**: Holding alt fire reads from the grimoire (channel, 3s). During the reading, crimson sigils appear in a circle around the player (10 tile radius). After 3s, the circle detonates — anything inside takes massive damage + is Condemned (DoT for 8s).
- **Page Turn**: Every 7th cast, the grimoire "turns a page" — next cast is enhanced (wider cone, +30% damage, deeper color). Visual: page-turning animation on the grimoire.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `CondemnationWaveCone.fx` | Channeled cone wave attack | Cone-shaped mesh with internal crimson energy scroll. FBM noise distortion. Core: dark blood crimson. Edge: ember orange. Distortion at cone edges. Continuous while channeling. |
| `DarkSermonCircle.fx` | Ritual circle for Dark Sermon | SDF circle with sigil patterns inside (rotating runes, judgment symbols). Crimson-gold. Starts dim → intensifies over 3s channel → detonation flash. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CondemnationEnergySpark | Sheds from wave cone edges | Dark crimson sparks, 6 frame life |
| WrittenNameParticle | Floating text "CONDEMNED" at kill point | Crimson glowing text, slow upward drift, 180 frame life (3s) |
| SermonSigilParticle | Appears along Dark Sermon circle edge | Rotating rune symbol, crimson-gold, 60 per circle, stationary |
| PageTurnFlashParticle | Brief flash when page turns | Gold-white flash near player hand, 4 frame burst |

---

## 9. Eclipse of Wrath (Magic)

### Identity & Musical Soul
An eclipse — when darkness consumes light, and wrath consumes mercy. Eclipse orbs are spheres of total darkness edged with wrathful fire, and when they split into wrath shards, they scatter destruction like a dying star's final moments. The most visually dramatic magic weapon — eclipses as weapons.

### Lore Line
*"The sun that rises for judgment is not the sun that brings dawn."*

### Combat Mechanics
- **Eclipse Orb**: Primary fire — EclipseOrbProjectile. Slow-moving dark sphere with corona of ember fire. On hitting an enemy or reaching max range, splits into 6 EclipseWrathShards that scatter in all directions.
- **Wrath Shards**: Shards are fast, piercing, and deal 30% of orb's damage each. They ricochet off surfaces once. Shards leave brief fire trails.
- **Eclipse Field**: When an Eclipse Orb is destroyed (impact or range), it creates a brief Eclipse Field (2s) — a dark zone where enemy vision is reduced (cosmetic) and they take +15% damage from all sources.
- **Total Eclipse**: If 3+ Eclipse Orbs' fields overlap, they merge into a Total Eclipse — massive dark zone (3x duration, +30% damage modifier, enemies inside are slowed 30%).
- **Corona Flare**: Critical orb hits (crit chance applies to the orb itself) create a Corona Flare — instead of 6 shards, the orb explodes into 12 shards + a central fire nova.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `EclipseOrbBody.fx` | Dark sphere with fire corona | SDF circle —body is DARK (near-black, subtractive or very dark alpha). Corona: ring of ember-orange fire around the dark body (additive bloom at edge). Fire noise scroll at corona. The orb should look like an actual eclipse — dark disc with bright fiery edge. |
| `EclipseField.fx` | Darkness zone AoE | Circular SDF field — dark overlay (reduces screen brightness inside). Internal: slow dark-cloud noise scroll. Edge: ember-fire rim. For Total Eclipse: doubled intensity + visual "crack" lines. |
| `WrathShardProjectile.fx` | Fast shard projectile after split | Small elongated SDF shard — ember orange with fire trail. Fast-moving, brief afterimage. Ricochet: flash at bounce point. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| EclipseCoronaFlareParticle | Orbit around eclipse orb corona | Ember-orange fire wisps, tight orbit around dark sphere, continuous |
| WrathShardTrailParticle | Brief fire trail behind each shard | Small ember trail, 5 frame life |
| EclipseFieldDarknessParticle | Slow-moving dark motes inside eclipse field | Very dark purple-crimson motes, slow drift, 20 frame life |
| CoronaFlareNovaParticle | Burst from critical Corona Flare explosion | Bright ember-gold burst, 15-20 particles, 10 frame life |

---

## 10. Death Tolling Bell (Summon)

### Identity & Musical Soul
The death bell tolls — and each toll marks another soul claimed. This summon conjures a spectral bell minion that attacks with **toll waves** — concentric sound rings that push outward with wrathful energy. Each toll deals damage and the sound builds in intensity. The bell is inevitable — it tolls not for thee, but thou art caught in its resonance.

### Lore Line
*"Ask not for whom the bell tolls. It tolls for all."*

### Combat Mechanics
- **Bell Tolling Minion**: Floating spectral bell that hovers above the player. Attacks autonomously.
- **Toll Wave Attack**: Every 2 seconds, the bell tolls — BellTollWaveProjectile: expanding ring of crimson-gold energy. Damages enemies in ring. Multiple rings can be active simultaneously.
- **Tolling Resonance**: Each toll that hits an enemy adds a Tolled stack (max 5). At 5 Tolled: the enemy is Death-Marked — takes double damage from the next toll wave.
- **Funeral March**: Every 10th toll is a Funeral March — double-size toll wave + all enemies hit are afflicted with Wrathfire. Visual: the bell glows intensely during Funeral March.
- **Bell Positioning**: The bell can be commanded (right-click a location while summoned) to move to a specific position. Bell stays at that position until re-commanded. AOE waves originate from bell position. Strategic placement.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `TollWaveRing.fx` | Expanding toll wave ring | SDF ring expanding from bell position. Color: crimson-gold gradient along ring circumference. Ring thins as it expands. Internal pulsing. For Funeral March: double width + ember fire at ring edge. |
| `BellEntityGlow.fx` | Bell minion body shader | Bell silhouette with internal fire glow. Dark iron body → crimson glow between tolls → intensifying ember-gold during toll strike → white flash at moment of toll. Resting: subtle sway motion. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| TollWaveSparkParticle | Sheds from toll wave ring as it expands | Crimson-gold sparks, perpendicular drift outward, 6 frame life |
| BellSwayEmberParticle | Slow embers around resting bell | Ember motes, gentle drift, 15 frame life |
| TolledStackMarkParticle | Ring marks on Tolled enemies | Crimson rings, 1 per stack, orbit enemy, persistent |
| FuneralMarchFlashParticle | Flash when Funeral March activates | Gold-white bell silhouette flash above bell, 5 frame burst |

---

## 11. Harmony of Judgement (Summon)

### Identity & Musical Soul
Judgment harmonized — not a single voice but a **chorus of condemnation**. The sigil minion represents the collected will of all judges, arbiters, and executioners. It projects a judgment sigil that processes enemies — tagging, judging, and executing in an automated cycle.

### Lore Line
*"When many voices speak as one, there is no appeal."*

### Combat Mechanics
- **Judgment Sigil Minion**: Floating rotating sigil (crimson-gold judgment glyph) that autonomously targets enemies.
- **Sigil Process**: The sigil cycles through a judgment process for each target:
  - **Scan** (1s): Sigil fixates, crimson beam connects sigil to target
  - **Judge** (1s): Sigil pulses, judgment marks appear on target
  - **Execute** (instant): Burst of judgment energy at target
  - Then moves to next target
- **Collective Judgment**: When multiple enemies are scanned simultaneously (if you have multiple sigils from multiple summons), they share judgment — execute phase hits all scanned targets simultaneously.
- **Harmonized Verdict**: If the sigil processes 5 enemies in rapid succession (all within 15s), it enters Harmonized state — processing speed doubles, damage +50% for 10s.
- **Judgment Aura**: Allies near the sigil gain +5% damage and +3 defense. Passive aura effect.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `JudgmentSigilEntity.fx` | Rotating judgment sigil body | SDF complex glyph (scales of justice or abstract judgment rune) rendered as rotating sigil. Crimson-gold with internal fire glow. Rotation speed increases during processing. |
| `SigilScanBeam.fx` | Beam connecting sigil to target during Scan phase | Thin beam strip. Color: crimson → gold pulsing. Internal data-flow pattern (small dots traveling along beam). Locks on during Scan, intensifies during Judge, flash on Execute. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SigilRotationSparkParticle | Sheds from rotating sigil | Gold spark, centrifugal outward motion, 8 frame life |
| ScanBeamDataParticle | Travels along scan beam | Tiny gold dot, moves from sigil toward target, 10 frame life |
| JudgmentExecuteFlashParticle | Flash at target during Execute phase | Crimson-gold flash ring, 5 frame burst |
| HarmonizedAuraParticle | Ambient sparkles during Harmonized state | Bright gold sparkles orbiting sigil faster, 12 frame life |

---

## 12. Wrathful Contract (Summon)

### Identity & Musical Soul
A contract signed in blood — a wrath demon bound to serve. The demon attacks ferociously but the contract has fine print: the demon siphons a fraction of the player's life force. Risk-reward summoning. The demon is immensely powerful but there's always a cost.

### Lore Line
*"The contract demands payment in blood. Yours or theirs — it cares not which."*

### Combat Mechanics
- **Wrath Demon Minion**: Aggressive floating demon entity that charges at enemies with melee strikes, each dealing heavy damage.
- **Blood Contract**: While the demon is summoned, the player loses 1 HP/s (continuous cost). This represents the contract's toll. The drain stops if no enemies are within 50 tiles.
- **Wrath Frenzy**: The demon enters Frenzy after 3 consecutive kills — attack speed doubles, damage +30%, but player drain increases to 3 HP/s for 5s. High risk, high reward.
- **Contract Clause**: At the player's expense: if the demon kills an enemy during Frenzy, the player heals 5% of the killed enemy's max HP. Skilled play rewards sustaining the Frenzy.
- **Breach of Contract**: If the player's HP drops below 10% while the demon is summoned, the demon turns hostile for 5s (attacks everything including the player). After 5s, it re-binds. This is the contract's penalty clause.
- **Blood Sacrifice**: Alt fire — sacrifices 20% of current HP to empower the demon. Next 5s: demon's attacks deal 3x damage + apply Wrathfire to all hit. Powerful burst window.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WrathDemonBody.fx` | Demon entity body rendering | Dark silhouette body with internal ember-crimson fire. Cracks of molten energy across body. Eyes: bright ember-gold. During Frenzy: cracks widen, fire intensifies, size grows 20%. During Breach: color shifts to blood-crimson, eyes turn red. |
| `BloodContractLink.fx` | Visual tether between player and demon | Thin crimson line between player and demon. Contains blood-red particles flowing from player → demon (representing life drain). During Blood Sacrifice: flow reverses briefly (golden energy player → demon), then surges. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| DemonEmberShedParticle | Sheds from demon body continuously | Dark ember particles, 10 frame life, continuous |
| BloodDrainFlowParticle | Flows along contract link from player to demon | Small crimson droplet, follows link path, 15 frame life |
| FrenzyIntensifyParticle | Burst at Frenzy activation | Bright ember-gold burst, 10 particles, 8 frame burst |
| BreachWarningParticle | Flashing warning around player at low HP | Crimson pulse ring around player, flashing, 3 frames on/3 frames off |
| BloodSacrificeFlashParticle | Flash when sacrifice is used | Dark red screen-edge vignette + gold burst from player hand, 10 frame |

---

## Cross-Theme Synergy Notes

### Dies Irae Theme Unity
All weapons share blood red + dark crimson + ember orange palette with wrath/judgment motifs:
- **Melee trinity**: Raw wrath (Cleaver) → Methodical judgment (Verdict) → Binding chains (Chain)
- **Ranged spectrum**: Escalating sin collection (Collector) → Area denial (Cannon) → Precision punishment (Sentence)
- **Magic trio**: Tactical mine-laying (Staff) → Continuous channeling (Grimoire) → Eclipse mechanics (Eclipse)
- **Summon family**: Toll-wave AOE (Bell) → Automated processing (Sigil) → Risk-reward demon (Contract)

### Mechanical Identity
Dies Irae weapons share common systemic themes:
- **Stacking debuffs**: Almost every weapon builds stacks toward a threshold (Wrathfire tiers, Judgment Marks, Chain Links, Sin stacks, Tolled stacks)
- **Judgment/execution motifs**: Reaching max stacks triggers "execution" — a powerful payoff moment
- **Fire as punishment**: Wrathfire debuff appears across multiple weapons, creating cross-weapon synergy
- **Risk-reward**: Sin Collector and Wrathful Contract both offer power at a cost
- **Area denial**: Multiple weapons create persistent fire zones (Cleaver's crystallized flames, Cannon's hellfire zones, Staff's ignitions, Eclipse's fields)

### Musical Motifs
- **Percussive impacts**: Dies Irae weapons should FEEL heavy — screen shake on impacts, bass visual effects, weighty animations
- **Building intensity**: Like the Dies Irae movement building to fortissimo, weapons escalate (stacks building → threshold → payoff)
- **Tolling bells**: The Death Tolling Bell is literal, but other weapons echo the motif — rhythmic pulses, countdown mechanics, inevitable consequences
- **Fire-as-wrath**: Fire is the physical manifestation of divine anger. Every weapon uses fire differently, but fire is universal to the theme
