# 🌹 Ode to Joy — Resonance Weapons Planning

> *"Universal brotherhood — the triumph of joy over suffering."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's 9th Symphony "Ode to Joy" — triumph, celebration, universal joy |
| **Emotional Core** | Joy, celebration, triumph of spirit |
| **Color Palette** | Warm gold, radiant amber, jubilant light |
| **Palette Hex** | Rose Shadow `(100, 30, 50)` → Petal Pink `(220, 100, 120)` → Bloom Gold `(255, 200, 50)` → Radiant Amber `(255, 170, 40)` → Jubilant Light `(255, 250, 200)` → Pure Joy White `(255, 255, 240)` |
| **Lore Color** | `new Color(255, 200, 50)` — Warm Gold |
| **Lore Keywords** | Joy, bloom, rose, thorn, garden, anthem, hymn, triumph, ovation, harmony, elysian, fountain |
| **VFX Language** | Rose petals scattering, vine growth, thorn tendrils, golden light bursts, floral bloom explosions, pollen clouds, garden radiance |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Thornbound Reckoning | Melee | Vine waves on swing |
| 2 | The Gardener's Fury | Melee | Botanical fury projectiles |
| 3 | Rose Thorn Chainsaw | Melee | Held chainsaw with thorn projectiles |
| 4 | Thorn Spray Repeater | Ranged | Rapid-fire thorn spray |
| 5 | The Pollinator | Ranged | Pollination projectiles with spreading effect |
| 6 | Petal Storm Cannon | Ranged | Heavy petal storm barrages |
| 7 | Anthem of Glory | Magic | Channeled glorious anthem |
| 8 | Hymn of the Victorious | Magic | Victorious hymn spells |
| 9 | Elysian Verdict | Magic | Elysian judgment magic |
| 10 | Triumphant Chorus | Summon | Chorus minion ensemble |
| 11 | The Standing Ovation | Summon | Crowd-themed ovation minions |
| 12 | Fountain of Joyous Harmony | Summon | Fountain of healing harmony |

---

## 1. Thornbound Reckoning (Melee)

### Identity & Musical Soul
Joy is not passive — it fights for its place. Thornbound Reckoning is the warrior's garden: roses that conceal thorns, beauty that strikes back. Each swing sends vine waves through the earth, erupting with thorns wherever enemies stand. Joy's reckoning for those who would trample the garden.

### Lore Line
*"The garden does not ask permission to defend itself."*

### Combat Mechanics
- **3-Phase Garden Combo**:
  - **Phase 1 — Pruning Strike**: Horizontal slash. Spawns VineWaveProjectile that crawls along the ground 15 tiles, erupting at enemy positions with rose thorn pillars.
  - **Phase 2 — Thorn Bloom**: Rising upper slash. Spawns 3 vine waves in a fan. Each wave creates a Thorn Patch (persistent ground hazard, 3s) where it erupts.
  - **Phase 3 — Reckoning Bloom**: Overhead slam → massive vine wave in all 4 directions + central Bloom Burst (AoE that heals player 3% of damage dealt).
- **Overgrowth**: Thorn Patches persist as ground hazards. If 3+ patches overlap, they merge into an Overgrowth Zone — enemies inside take triple Thorn Patch damage + are entangled (rooted 1s on entry).
- **Root Network**: Vine waves travel faster over Thorn Patches (2x speed). Encourages building a thorny battlefield then leveraging it.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `VineWaveTravel.fx` | Ground-crawling vine wave | Ground-level strip — vine-patterned texture UV-scrolling forward. Green-petal pink body → thorn highlights. Leading edge: rising vine tendrils. |
| `ThornPatchField.fx` | Persistent thorn ground hazard | Ground overlay. Thorn/vine texture fill. Petal pink → rose shadow color. Small thorn protrusions at random positions (SDF spikes). Pulses subtly. For Overgrowth: denser, brighter, with rose bloom centers. |
| `ReckoningBloomBurst.fx` | Central AoE bloom explosion | Expanding SDF circle with petal-shaped edges (not smooth circle — rose-petal scalloped edge). Gold center → petal pink ring → rose shadow fade. Rose petals burst outward. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| VineWaveLeafParticle | Kicked up from vine wave travel | Small green leaves, upward arc, 10 frame life |
| ThornEruptionParticle | Upward burst at thorn eruption points | Thorn spike shapes, petal pink, small upward burst, 8 frame |
| RoseBloomPetalParticle | Scattered from Reckoning Bloom | Rose petals (pink-gold), slow drift, 25 frame life |
| OvergrowthVineParticle | Appears in Overgrowth zones (decorative) | Small vine tendrils, stationary, 90 frame life |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Thorn Pierced | 10 damage/s while in Thorn Patch. | While standing in patch |
| Entangled | Rooted in place. Applied by Overgrowth. | 60 frames (1s) |

---

## 2. The Gardener's Fury (Melee)

### Identity & Musical Soul
The gardener's fury is the wrath of the caretaker — one who nurtures but strikes back with devastating force when the garden is threatened. Botanical fury projectiles are concentrated plant energy — compressed pollen bombs, seed artillery, and barrage of hardened stems.

### Lore Line
*"He who tends the garden knows best where the roots run deep."*

### Combat Mechanics
- **3-Phase Botanical Combo**:
  - **Phase 1 — Seed Scatter**: Quick thrust spawning 5 GardenerFuryProjectiles (seed pods) in a spray. Seeds embed in enemies and burst after 1s (delayed damage + brief root).
  - **Phase 2 — Stem Lash**: Horizontal lash with hardened stem. On hit: enemy marked with Pollen Cloud (all allies have +5% crit against target for 3s).
  - **Phase 3 — Botanical Barrage**: Overhead slam → 8 seed pods scattered in random directions + central Gardener's Wrath explosion (gold-green AoE).
- **Seed Growth**: Seeds embedded in enemies grow for 1s before bursting. If the player hits a seeded enemy again before burst, the seed grows larger → bigger burst damage (max 3x).
- **Compost**: Enemies killed while seeded drop Compost (small health orb, heals 15 HP). Sustain mechanic.
- **Overgrowth Sync**: If Thornbound Reckoning's vine waves pass through enemies with seeds, the seeds burst immediately at 2x damage. Cross-weapon synergy within theme.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SeedPodProjectile.fx` | Seed pod projectile with internal growth | Small SDF oval. Starts dim green → as growth timer progresses, swells slightly and brightens to gold → petal pink. At burst: expanding flower-shaped flash. |
| `GardenersWrathExplosion.fx` | Plant energy AoE from Phase 3 | Expanding circle with leaf/petal-shaped edge pattern. Gold center → green body → petal pink edge. Internal vine pattern. Brief (0.3s). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SeedEmbedSparkParticle | Flash when seed embeds in enemy | Small green spark, 3 frame burst |
| SeedBurstPetalParticle | Burst when embedded seed detonates | Small rose petals + green leaf confetti, 10-12 particles, 10 frame life |
| PollenCloudParticle | Hovering around Pollen Cloud marked enemy | Yellow-gold pollen motes, slow drift, 90 frame life (3s debuff) |
| CompostDropParticle | Brief sparkle at Compost health orb | Green-gold sparkle, 5 frame, marks pickup |

---

## 3. Rose Thorn Chainsaw (Melee — Held)

### Identity & Musical Soul
A held melee weapon — a chainsaw made of intertwined rose thorns, spinning continuously. The beauty of rose meets the brutality of machinery. Held against enemies for continuous damage, it shreds with thorns and flings rose petals. It's joy expressed through pure aggressive contact.

### Lore Line
*"Beauty is not gentle. Beauty is relentless."*

### Combat Mechanics
- **Held Chainsaw**: Primary use — hold weapon against enemies for continuous contact damage (RoseThornChainsawProjectile). High DPS when maintained.
- **Thorn Fling**: While held on enemy, thorns periodically fling outward (every 20 frames) — small thorn projectiles that damage nearby enemies. The chainsaw hits one target but flinging harms those nearby.
- **Rose Shredder**: Continuous contact builds Shredder stacks on target (max 10). Each stack: armor penetration +3. At max: enemy has reduced armor by 30. Visual: enemy covered in thorn marks.
- **Petal Storm**: After 3 seconds of continuous contact, the chainsaw generates a Petal Storm — swirling petal tornado (3 tile radius) that persists for 2s after releasing, damaging enemies caught in it.
- **Thorned Guard**: While actively chainsawing, the player has +10 defense (thorns protect the wielder too). Contact damage to the player is reflected at 30%.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `RoseThornChainBody.fx` | Spinning chainsaw blade body | Spinning blade with thorn-tooth pattern at edges. Petal pink body → dark thorn highlights. Rotation speed reflects attack speed. Blur at high speed. Rose petal trails from teeth. |
| `PetalStormVortex.fx` | Petal tornado AoE after sustained contact | Circular vortex shader. Rose petals UV-scrolling in spiral pattern. Gold center → petal pink spiral → rose shadow edge. Internal rotation. 2s duration with slow decay. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ChainThornFlingParticle | Thorns flung outward from contact | Small dark thorn shapes, radial burst, 10 frame life |
| ChainsawPetalShedParticle | Continuous petal shed during contact | Rose petals, spraying outward from contact point, 12 frame life |
| ShredderMarkParticle | Small marks on shredded enemy | Tiny thorn wounds, stationary on enemy, 60 frame life per stack |
| PetalStormSwirlParticle | Petals inside petal storm vortex | Full-size rose petals, spiraling inside vortex, 40 frame life |

---

## 4. Thorn Spray Repeater (Ranged)

### Identity & Musical Soul
Rapid-fire burst of thorn projectiles — a machine gun of the garden. Thorns spray in a tight cone, shredding everything in their path. The joy of overwhelming firepower expressed through nature's own ammunition.

### Lore Line
*"One thorn is a warning. A thousand thorns is an anthem."*

### Combat Mechanics
- **Thorn Spray**: Primary fire — rapid-fire ThornSprayProjectiles in a slight spread (3-4° cone). High rate of fire (8 useTime). Individual thorn damage is low, but volume is extreme.
- **Thorn Accumulation**: Thorns embed in enemies (cosmetic: enemy gradually covered in thorns). At 20 embedded thorns → Thorn Burst: all thorns detonate simultaneously (bonus damage proportional to thorns embedded). Resets thorn count.
- **Rose Burst Ammo**: Every 15th thorn is a Rose Burst thorn — slightly larger, deals 2x damage, explodes in a small AoE of petal shrapnel on hit.
- **Gardener's Rhythm**: Firing in sustained bursts (continuous fire for 2+ seconds) builds Tempo. At max Tempo: fire rate increases 30%, but accuracy decreases slightly (wider cone). Creates a spray-and-pray dynamic.
- **Bloom Reload**: When you stop firing after sustained burst, a "bloom" animation plays on the weapon — brief rose bloom VFX. The first 5 shots after a bloom deal +25% damage. Rewards burst → pause → burst rhythm.

### VFX Architecture Plan

#### Custom Shaders (1)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ThornSprayTrail.fx` | Brief thorn projectile trail | Ultra-thin strip trail — green-brown thorn color. Very brief (3-4 frames visible). At Tempo mode: trail fades to petal pink. Minimal visual per thorn, cumulative effect creates visual density. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ThornImpactChipParticle | Fragment at thorn impact point | Tiny green-brown chip, 3 frame burst, 2 per hit |
| ThornBurstPetalParticle | Burst from accumulated thorn detonation | Rose petals + thorn shrapnel mixed, 15 particles, 10 frame life |
| RoseBurstExplosionParticle | Small AoE from Rose Burst special thorn | Petal pink flash + petal scatter, 8 particles, 8 frame life |
| BloomReloadFlashParticle | Rose bloom on weapon during reload pause | Gold-pink rose bloom shape opening, 10 frame animation |

---

## 5. The Pollinator (Ranged)

### Identity & Musical Soul
Pollination — the mechanism that spreads life. This weapon fires projectiles that spread **pollen** to enemies, and that pollen chains to create a spreading network of damage and debuff. One shot can eventually affect the entire battlefield through propagation. Joy spreading itself.

### Lore Line
*"Joy, like pollen, cannot be contained. It spreads."*

### Combat Mechanics
- **Pollinator Shot**: Primary fire — PollinatorProjectiles. Moderate speed, moderate damage. On hit: applies Pollinated debuff to target.
- **Pollinated**: Debuff — enemy sheds pollen particles. These particles drift toward nearby unpollinated enemies (5 tile range). If a particle reaches another enemy, they become Pollinated too. Chain spreads.
- **Pollinated Damage**: Pollinated enemies take 8 damage/s and have -10% movement speed. Multiple players' pollination stacks.
- **Bloom Trigger**: If 5+ enemies are simultaneously Pollinated, the player can alt-fire to trigger Mass Bloom — all Pollinated enemies take a burst of damage and spawn homing seed projectiles (3 per enemy) that attack non-Pollinated enemies.
- **Golden Field**: Pollinated enemies who die release a Golden Field (2 tile radius, 3s) that heals the player 5 HP/s while standing in it. Larger groups = more healing fields.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `PollenCloud.fx` | Pollen drift cloud from Pollinated enemies | Volume of floating particles (actually a translucent SDF sphere with internal noise). Yellow-gold, semi-transparent. Drifts toward nearby enemies. Soft falloff edges. |
| `GoldenFieldGlow.fx` | Healing golden field from dead Pollinated enemies | Circular SDF zone. Gold center → warm amber edge → transparent. Internal slow pulsing. Upward-floating golden sparkle motes inside. Healing indicator. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PollenDriftParticle | Drifts from Pollinated enemy toward nearest target | Tiny yellow pollen mote, slow drift with slight randomization, 60 frame life |
| MassBloomExplosionParticle | Burst from each enemy during Mass Bloom | Rose petals + golden pollen in burst, 8-10 particles per enemy, 12 frame life |
| SeedProjectileTrailParticle | Trail behind homing seed from Mass Bloom | Green-gold trail, 6 frame life |
| GoldenFieldSparkParticle | Rising sparkles in Golden Field | Gold sparkle, slow upward, 20 frame life |

---

## 6. Petal Storm Cannon (Ranged)

### Identity & Musical Soul
The heavy artillery of the garden — a cannon that fires compressed petal storms. Each shot is a botanical artillery barrage: the petals are razor-sharp, spinning in a vortex of joyful destruction. This is the moment the orchestra swells to fortissimo — beautiful, overwhelming, unstoppable.

### Lore Line
*"The storm does not discriminate. Joy and ruin travel together."*

### Combat Mechanics
- **Petal Storm Barrage**: Primary fire — PetalStormProjectiles. Fires 3 petal clusters per shot in a spread. Each cluster explodes on contact into a swirling petal vortex (AoE, 2s persistent).
- **Storm Stacking**: Overlapping petal vortexes merge into a larger Storm (radius increases by 50% per merged vortex, max 4x). Massive merged storms devastate groups.
- **Eye of the Storm**: Standing inside your own petal storm grants Eye of the Storm — +8% damage, +5% crit for 3s after leaving the storm. Encourages aggressive positioning.
- **Hurricane Mode**: After 3 consecutive shots, holding fire charges a Hurricane Shot (max 2s charge). Hurricane Shot fires a single massive petal storm that travels forward, persisting as it moves (not stationary), sweeping across the battlefield.
- **Seasonal Petals**: Petals cycle through visual colors matching the shot sequence — first shot: pink, second: gold, third: white. Creates a rainbow of botanical destruction.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `PetalVortex.fx` | Persistent petal storm AoE | Circular vortex with petal-shaped particles UV-scrolling in spiral. Color determined by seasonal cycle (pink/gold/white). Internal rotation, slight screen distortion at center. Merged storms: more layers, faster rotation, brighter. |
| `HurricaneShotBody.fx` | Moving hurricane projectile | Large moving vortex. Petal spiral scroll + forward UV motion. Eye of storm visible as bright center. Trailing petal debris. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PetalVortexParticle | Spiraling inside vortex areas | Full-size rose petals, tight spiral motion, color-matched to season |
| StormMergeFlashParticle | Flash when vortexes merge | Gold-white expanding ring, 5 frame burst |
| HurricaneTrailPetalParticle | Shed from moving hurricane shot | Petals trailing behind, all three seasonal colors mixed, 15 frame life |
| EyeOfStormBuffParticle | Subtle glow around player in Eye of Storm | Gold shimmer around player, 8 frame life |

---

## 7. Anthem of Glory (Magic)

### Identity & Musical Soul
The Anthem — the core melody of Ode to Joy itself. This weapon channels the **pure musical energy** of the anthem, creating beams and waves of golden sound that resonate through enemies. It's the most directly "musical" weapon in the theme — you can hear the anthem in its effects.

### Lore Line
*"Sing, and the world sings with you. Scream, and the world burns."*

### Combat Mechanics
- **Anthem Beam**: Primary fire — channeled golden beam forward. Continuous damage while held. Beam sways slightly side to side (like a conductor's gesture).
- **Glory Notes**: Every 2s of continuous channeling, a Glory Note spawns at a random screen position and fires toward the beam's target. Notes deal bonus damage. Max 6 notes active at once.
- **Crescendo Channel**: Damage increases the longer you channel (starts at 1x, reaches 2x at 5s). Visual: beam intensifies, more notes spawn faster.
- **Anthem's End**: When you stop channeling (release), there's a brief "echoing" effect — the last 1s of beam lingers as a fading afterimage that still deals 50% damage. Enemies hit by both the beam and the echo take +20% bonus.
- **Victory Fanfare**: If channeling kills 3+ enemies within 5s, triggers Victory Fanfare — screen-wide golden flash + all remaining Glory Notes converge simultaneously on cursor in a burst of musical energy.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `AnthemBeamBody.fx` | Channeled golden beam | Strip beam with internal musical-wave pattern (standing wave). Gold core → warm amber body → petal pink edges. Width oscillates gently (sway). UV-scroll: musical staff line pattern flowing along beam. Intensity tied to Crescendo progress. |
| `GloryNoteBody.fx` | Spawned musical note projectile | SDF musical note (quarter note shape). Gold glow. Trail: small golden sparkle. Homes toward target with slight arc. On impact: small golden ring burst. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| AnthemBeamEdgeParticle | Sheds from beam edge during channeling | Gold sparkles, perpendicular drift, 6 frame life |
| GloryNoteTrailParticle | Trail behind Glory Note projectiles | Tiny gold sparkle, 4 frame life |
| VictoryFanfareFlashParticle | Screen-wide flash from Victory Fanfare | Golden flash overlay, 5 frame fade |
| CrescendoIntensityParticle | Ambient around player during crescendo channeling | Gold motes orbiting player, frequency increases with crescendo, 15 frame life |

---

## 8. Hymn of the Victorious (Magic)

### Identity & Musical Soul
A hymn sung by victors — each spell is a verse of triumph. Where Anthem is a continuous beam, Hymn fires discrete powerful shots, each one a verse. Complete the hymn (fire all verses) and the culmination is devastating. Structured, deliberate, building to a climax.

### Lore Line
*"Each verse is a victory. The final verse is annihilation."*

### Combat Mechanics
- **Hymn Verses**: Primary fire — fires a HymnProjectile. Each shot is a "verse" (cycle of 4 unique verse types):
  - **Verse 1 — Exordium**: Gold energy bolt, single target, high damage, piercing.
  - **Verse 2 — Rising**: 3 smaller bolts in fan, moderate damage, applies Jubilant Burn (DoT).
  - **Verse 3 — Apex**: Large orb that hovers at cursor for 1s, then detonates in AoE.
  - **Verse 4 — Gloria**: Massive bolt that splits into 6 homing fragments on contact.
- **Complete Hymn**: Firing all 4 verses in sequence without pause triggers Complete Hymn — next cast fires ALL 4 verse types simultaneously (combined into one super-spell).
- **Hymn Resonance**: Enemies hit by 3+ different verse types within 5s receive Hymn Resonance — take +25% magic damage for 4s.
- **Encore**: If Complete Hymn kills an enemy, the Hymn resets to Verse 4 (instead of Verse 1), allowing repeated Gloria + Complete Hymn cycles for sustained high damage during boss fights.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `HymnVerseOrb.fx` | Verse projectile body (parameterized per verse) | SDF orb with color parameter: V1=pure gold, V2=petal pink, V3=amber, V4=brilliant white-gold. Internal energy scroll unique to each verse type. Glow intensity increases V1→V4. |
| `CompleteHymnDetonation.fx` | Combined Complete Hymn super-spell impact | Layered explosion: gold ring → pink ring → amber ring → white-gold center flash. Each ring represents a verse. Overlapping creates rich, multi-layered burst effect. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| VerseTrailParticle | Trail behind each verse (color coded) | Color-matched sparkle, 6 frame life, different shape per verse |
| HymnResonanceGlowParticle | Aura on Hymn Resonance debuffed enemies | Gold-pink glow, orbiting enemy, 120 frame life (debuff duration) |
| CompleteHymnBurstParticle | Massive burst from Complete Hymn | All 4 verse colors mixed, 25+ particles, 15 frame life |
| EncoreFlashParticle | Brief flash when Encore triggers | White-gold flash at player position, 4 frame burst |

---

## 9. Elysian Verdict (Magic)

### Identity & Musical Soul
Elysium — paradise. But to reach paradise, one must be judged worthy. This weapon delivers the Elysian Verdict: a judgment of joy that is nevertheless devastating to those found wanting. Elysian energy is pure golden light that purifies and burns simultaneously. The most powerful magic weapon in the Ode to Joy arsenal.

### Lore Line
*"Elysium's gates open only for those the light deems worthy. None have been worthy."*

### Combat Mechanics
- **Elysian Judgment**: Primary fire — ElysianProjectiles: golden light orb with prismatic edges. Medium speed, high damage. On hit: Elysian Mark (target is judged).
- **Judgment Tiers**: Elysian Marks stack (max 3):
  - 1 Mark: Target glows faintly gold. +10% magic damage taken.
  - 2 Marks: Target glows stronger. +20% magic damage + takes Elysian Burn (DoT).
  - 3 Marks: **Elysian Verdict** — massive golden explosion centered on target, dealing heavy AoE damage + healing player for 10% of damage dealt.
- **Elysian Radiance**: While this weapon is equipped, the player emits a soft golden aura (5 tile radius). Allies in the aura gain +3% damage.
- **Worthy Judge**: Critical hits apply 2 marks instead of 1. With high crit chance, Mark buildup becomes very fast.
- **Paradise Lost**: If the player drops below 25% HP, Elysian energy becomes corrupted — orbs turn dark gold with crimson edges, damage dealt +50%, but healing from Verdict becomes 0. High-risk high-reward mode.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ElysianOrbBody.fx` | Golden light orb projectile | SDF sphere with internal golden energy (smooth, radiant, not noisy — clean and pure). Gold core → warm amber → prismatic rainbow edge shimmer. Bright additive glow. For Paradise Lost: gold shifts to dark gold-crimson. |
| `ElysianVerdictExplosion.fx` | Massive golden explosion at 3 marks | Large expanding SDF circle with rays extending outward (like a sunburst). Gold center → radiant amber → prismatic rainbow edge. Internal golden energy flash. Screen shake. Healing particles flowing toward player. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ElysianMarkGlowParticle | Aura around marked enemies (intensity per mark) | Gold glow, orbiting, brighter per stack, 120 frame life |
| VerdictSunburstParticle | Rays extending from Verdict explosion | Radial gold rays, extending outward, 10 frame life |
| HealFlowParticle | Flows from judged enemy toward player during Verdict | Green-gold health mote, homing toward player, 20 frame life |
| ParadiseLostEmberParticle | Dark embers around player during Paradise Lost | Dark gold-crimson embers, slow drift, continuous |

---

## 10. Triumphant Chorus (Summon)

### Identity & Musical Soul
A chorus of triumph — multiple vocal minions singing in harmony. Each minion represents a different vocal part (soprano, alto, tenor, bass). Together they create harmonies that damage enemies and buff allies. The chorus grows stronger with more voices.

### Lore Line
*"Alone, a voice. Together, a world remade."*

### Combat Mechanics
- **Chorus Minion**: Summons a floating musical spirit that sings (attacks). Each minion fires ChorusProjectiles (golden sound waves) at enemies.
- **Vocal Parts**: Each additional summon adds a different vocal part:
  - 1 minion: Soprano (fast, light attacks)
  - 2 minions: + Alto (medium attacks with slow effect)
  - 3 minions: + Tenor (strong attacks with knockback)
  - 4+ minions: + Bass (heavy attacks that deal AoE ground pound)
- **Harmony Bonus**: When all 4 vocal parts are active, they achieve Harmony — all minion damage +20%, attacks create golden resonance fields at enemy positions.
- **Ensemble Attacks**: Every 10 seconds, all minions sing together — synchronized ChorusProjectile volley where all minions fire at the same target simultaneously. Synchronized hits deal 1.5x damage.
- **Standing Ovation Sync**: If The Standing Ovation summon is also active, both summon types gain +15% damage (cross-summon synergy within theme).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ChorusMinionBody.fx` | Musical spirit minion body | Ethereal golden humanoid silhouette. Internal golden energy. Each vocal part has slight variation: Soprano (smaller, brighter), Alto (medium, warm amber), Tenor (medium-large, gold), Bass (largest, darker gold). Pulsing glow when singing. |
| `SoundWaveProjectile.fx` | Sound wave projectile from singing | Expanding arc shape (like a sound wave). Color: gold with amber edges. Travels toward target with wave motion. At Harmony: rainbow prismatic edge added. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SongNoteParticle | Music notes floating from singing minions | Musical note shapes (eighth, quarter), gold, upward drift, 15 frame life |
| HarmonyResonanceFieldParticle | Golden sparkles in resonance fields | Gold sparkle, rising, 20 frame life |
| EnsembleAttackFlashParticle | Flash when synchronized volley fires | White-gold flash at each minion, 3 frame burst |
| ChorusLinkParticle | Faint lines connecting chorus members during Harmony | Thin gold lines between minions, pulsing alpha |

---

## 11. The Standing Ovation (Summon)

### Identity & Musical Soul
The audience erupts — standing ovation! This summon conjures crowd-themed minions: enthusiastic phantom spectators that attack with applause shockwaves, thrown roses, and overwhelming adoration. It's joyful absurdity — being attacked by an appreciative audience.

### Lore Line
*"The audience loved the performance. The audience demands an encore."*

### Combat Mechanics
- **Ovation Minion**: Summons a phantom spectator that attacks enemies with:
  - **Applause Wave**: Ranged shockwave from clapping (every 3s)
  - **Thrown Rose**: Tracking rose projectile that applies Thorned debuff
  - **Standing Rush**: Charges at enemy when they're at low HP (below 20%)
- **Crowd Size**: Multiple summons create a crowd. Larger crowds deal more damage per minion (+5% per additional crowd member). Visual: minions cluster together.
- **Ovation Meter**: Killing enemies builds Ovation Meter. At full meter: Standing Ovation Event — all minions rise to their feet, clap together creating a massive shockwave, then rain roses from above for 3s.
- **Encore Encore**: After a Standing Ovation Event, re-summoning immediately (within 5s) grants 2 minions for 1 slot (crowd gets bigger). Stacks once.
- **Triumphant Chorus Sync**: Cross-summon synergy with Triumphant Chorus minions (see above).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `OvationMinionBody.fx` | Phantom spectator body | Semi-transparent golden-human silhouette. Less defined than Chorus minion — more like a cheering crowd member. Pulsing with crowd energy. At Standing Ovation: all minions glow bright gold. |
| `ApplauseShockwave.fx` | Expanding shockwave from applause | Expanding SDF ring — thin, gold, fast expansion. Internal sound-wave-line pattern. Quick (0.5s duration). Multiple overlapping during crowd attacks. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ThrownRoseProjectileParticle | Rose thrown by minion | Red rose sprite, arc trajectory, 30 frame life |
| RoseRainParticle | Roses raining during Standing Ovation Event | Rose petals and whole roses falling from above, scattered, 25 frame life |
| ApplauseSparkParticle | Small spark at each applause clap | Gold flash, 2 frame burst |
| CrowdRushTrailParticle | Trail behind charging crowd minion | Gold streak, 6 frame life |

---

## 12. Fountain of Joyous Harmony (Summon)

### Identity & Musical Soul
A fountain — centerpiece of any grand garden, spraying water upward in celebration. This summon places a stationary fountain that continuously heals allies and damages enemies with joyous energy. The water is liquid harmony — golden light that rises and falls in beautiful arcs.

### Lore Line
*"Where the fountain flows, joy follows. Where joy flows, nothing can stand against it."*

### Combat Mechanics
- **Joyous Fountain Minion**: Summons a stationary fountain at the player's position. The fountain:
  - Continuously heals allies within 15 tiles (5 HP/s)
  - Fires FountainProjectiles upward that arc down onto enemies (homing golden droplets)
  - Creates a Harmony Zone (10 tile radius): +8% all damage for allies inside
- **Fountain Tiers**: Additional summons upgrade the fountain (not multiple fountains):
  - Tier 1: Base healing and damage
  - Tier 2: +3 HP/s healing, +20% droplet damage
  - Tier 3: +5 HP/s (total 13), droplets pierce, Harmony Zone radius +5 tiles
  - Tier 4+: +2 HP/s per tier and +10% damage per tier
- **Joyous Geyser**: Every 15s, the fountain erupts in a Joyous Geyser — massive golden spray that damages all enemies within 20 tiles and heals allies for 30 HP instant. Higher tier = more frequent geysers.
- **Fountain Relocation**: Alt fire moves the fountain to the player's current position (5s cooldown). Strategic placement.
- **Harmony Aura**: Allies standing directly on the fountain (within 3 tiles) gain +15% all damage and rapid heal (triple the normal rate).

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `JoyousFountainEntity.fx` | Fountain body with rising water | Fountain base with golden water arcing upward and falling. UV-scroll for water flow (upward in center, downward at edges). Gold-amber water color. Higher tier: more water streams, taller arcs. Pulsing golden aura at base. |
| `JoyousGeyserBurst.fx` | Massive geyser eruption | Vertical golden burst — tall column of golden light rising rapidly. Top: spray of golden droplets. Expanding ring at base. All warm gold-amber colors. Brief (0.5s) but visually dramatic. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| GoldenDropletParticle | Arcing droplets from fountain (continuous) | Golden water drops, parabolic arc, 20 frame life |
| HarmonyZoneGlintParticle | Ambient sparkles in Harmony Zone | Tiny gold sparkles, scattered throughout zone, 25 frame life |
| GeyserSprayParticle | Burst during Joyous Geyser | Large golden drops spraying outward + upward, 30+ particles, 15 frame life |
| HealFlowParticle | Green-gold flow from fountain toward allies | Flowing heal mote, arcs toward nearby allies, 20 frame life |

---

## Cross-Theme Synergy Notes

### Ode to Joy Theme Unity
All weapons share warm gold + petal pink + radiant amber palette with botanical/triumph motifs:
- **Melee trio**: Garden warfare — vine waves (Thornbound), seed bombs (Gardener), chainsaw fury (Chainsaw)
- **Ranged spectrum**: Rapid thorns (Repeater), spreading pollen (Pollinator), petal artillery (Cannon)
- **Magic trio**: Continuous anthem (Anthem), versed hymn (Hymn), golden judgment (Elysian)
- **Summon family**: Musical chorus (Chorus), audience crowd (Ovation), healing fountain (Fountain)

### Cross-Weapon Synergies
Unlike most themes, Ode to Joy weapons explicitly synergize:
- **Thornbound + Gardener**: Vine waves interact with embedded seeds
- **Triumphant Chorus + Standing Ovation**: Cross-summon +15% damage bonus
- **Pollinator + any AoE**: Pollinated enemies spread debuffs, AoE maximizes spread
- **Fountain of Harmony**: Universal support — all weapons benefit from Harmony Zone damage buff

### Musical Motifs
- **Joy through combat**: Weapons are joyful in their destruction — petals, roses, golden light, fountains, applause. Not grim or serious — exuberant.
- **Growth and nurture**: Botanical themes (seeds, vines, thorns, flowers) tie to the garden-of-joy motif. Growth as power.
- **Musical structure**: Hymn's verse system, Anthem's crescendo, Chorus's vocal parts — combat as musical performance.
- **Community**: Many weapons feature group mechanics (chorus harmony, crowd effects, healing fountain). Joy is communal.
- **Botanical VFX dominant**: Rose petals should be the signature particle — every weapon scatters petals in some form, but each uses them differently (drifting, exploding, spiraling, raining).
