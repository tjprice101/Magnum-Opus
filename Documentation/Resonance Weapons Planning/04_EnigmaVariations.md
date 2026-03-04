# 🔮 Enigma Variations — Resonance Weapons Planning

> *"The unknowable mystery — void black, deep purple, eerie green flame."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Elgar's Enigma Variations — the unknowable mystery |
| **Emotional Core** | Mystery, dread, arcane secrets |
| **Color Palette** | Void black, deep purple, eerie green flame |
| **Palette Hex** | VoidBlack `(10, 5, 15)` → DeepPurple `(60, 20, 100)` → Purple `(140, 60, 200)` → GreenFlame `(50, 220, 100)` → BrightGreen `(120, 255, 160)` → WhiteGreenFlash `(200, 255, 220)` |
| **Lore Color** | `new Color(140, 60, 200)` — Void Purple |
| **Lore Keywords** | Mystery, paradox, riddle, cipher, void, unknown, watching, silence, enigma |
| **VFX Language** | Swirling void, watching eyes, eerie green flames, dimensional tears, paradox glyphs, question marks, shadow mist, arcane convergence |

### Shared Infrastructure (Already Exists)
| System | Purpose |
|--------|---------|
| `EnigmaPalette.cs` | 426-line palette — 6 core colors + 8 per-weapon swing palettes + gradient helpers |
| `ParadoxBrand` debuff + `ParadoxBrandNPC` | Shared stacking debuff applied by ALL 8 weapons |
| `SeekingCrystalHelper.SpawnEnigmaCrystals` | Homing enigma crystals spawned on special events |
| `EnigmaShaderHelper` | Shader binding helpers (`DrawShaderOverlay`) |
| Per-weapon: 2 shaders, custom dust, particles, primitives, player tracker | Self-contained weapon architecture |

---

## Foundation Weapons Integration Map

Enigma Variations weapons each have 2 dedicated shaders and full self-contained VFX pipelines. Foundations provide the **mesh construction, blend management, and rendering scaffolding** that weapon-specific shaders plug into. The theme's identity is defined by void-mystery aesthetics — everything should feel unknowable and arcane.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Unresolved Cadence, Variations of the Void | Swing arc smear — SmearDistortShader base with void purple/green LUT |
| **RibbonFoundation** | All 8 weapons | Trail strips — various modes: Energy Surge for beams, Harmonic Wave for orbiters, Basic Trail for bullets |
| **ThinSlashFoundation** | Unresolved Cadence, Variations of the Void | Dimensional slash impact marks — ThinSlashShader SDF in eerie green/purple |
| **XSlashFoundation** | Unresolved Cadence (Paradox Collapse) | Paradox Collapse cross-tear — XSlashShader void distortion |
| **ImpactFoundation** | All 8 weapons | Hit VFX — RippleShader for paradox shockwaves, DamageZoneShader for mystery zones |
| **LaserFoundation** | Cipher Nocturne, Variations of the Void | ConvergenceBeamShader for channeled beam / convergence beam set |
| **ThinLaserFoundation** | Fang (lightning arcs), Silent Measure (chain lightning) | ThinBeamShader for chain lightning between targets |
| **InfernalBeamFoundation** | Variations of the Void | InfernalBeamBodyShader for converging void beams |
| **SparkleProjectileFoundation** | Fugue (voices), Dissonance (riddlebolts), Silent Measure (seekers) | SparkleTrailShader for glittering homing projectile trails |
| **MaskFoundation** | Dissonance (orb aura), Watching Refrain (mystery zone), Fugue (convergence) | RadialNoiseMaskShader for void orbs, zones, convergence effects |
| **MagicOrbFoundation** | Dissonance of Secrets | OrbBolt pattern for growing void orb rendering |
| **ExplosionParticlesFoundation** | Unresolved Cadence, Variations of the Void, Cipher Nocturne, Tacet | Paradox detonation sparks — snap-back bursts, convergence explosions |
| **SmokeFoundation** | Unresolved Cadence, Variations of the Void | Void smoke wisps from dimensional slashes |
| **AttackAnimationFoundation** | Unresolved Cadence (Paradox Collapse) | Cinematic Collapse sequence |

---

## Weapons Overview

| # | Weapon | Class | Damage | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Cipher Nocturne | Magic (Beam) | 290 | Channeled beam + damage ramp + snap-back detonations |
| 2 | Dissonance of Secrets | Magic (Orb) | 275 | Growing void orb + aura damage + cascade explosion |
| 3 | Fugue of the Unknown | Magic (Orbiter) | 252 | Orbiting voices + release + EchoMark → Harmonic Convergence |
| 4 | Tacet's Enigma | Ranged (Gun) | 265 | Every 4th shot Paradox Bolt + 10-stack paradox explosion |
| 5 | The Silent Measure | Ranged (Bow) | 245 | Arrow split → 3 homing seekers + every 5th Paradox Bolt |
| 6 | The Unresolved Cadence | Melee | 600 | 3-phase combo + Inevitability stacks → Paradox Collapse AoE |
| 7 | The Watching Refrain | Summon | 220 | Phantom minion + crowd-control MysteryZones |
| 8 | Variations of the Void | Melee | 380 | 3-phase combo + converging triple beams → VoidResonanceExplosion |

---

## 1. Cipher Nocturne (Magic — Channeled Beam)

### Identity & Musical Soul
The cipher — a coded message projected as a beam that unravels reality along its path. The longer you channel, the more devastation it stores, and releasing it triggers cascading detonations at every point the beam touched.

### Lore Line
*"The answer was always in the silence between the notes."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CIPHER NOCTURNE — Foundation Architecture          │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → Channeled beam body (4 detail textures)    │  │
│  │  → BaseBeamWidth = 35f → scales to 60f        │  │
│  │  → Void purple/green gradient LUT             │  │
│  │  → Damage ramp: 1x→3x over 2 seconds         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: snap-back detonation rings   │  │
│  │  → DamageZone: unravel point lingering zones  │  │
│  │  → ringCount=3 per snap-back point            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (snap-back)      │  │
│  │  → Snap-back detonation sparks at each point  │  │
│  │  → SparkCount = 20 per unravel point          │  │
│  │  → Green/purple void sparks                   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Beam core energy trail                     │  │
│  │  → UV-scrolled void energy texture            │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → CipherBeamTrail.fx (CipherBeamFlow)               │
│  → CipherSnapBack.fx (CipherSnapBackMain)            │
│  UNIQUE SYSTEMS:                                    │
│  → Unravel points recorded every 15 frames          │
│  → Snap-back: release → detonate all stored points  │
│  → SeekingCrystals (3 per detonation, 25% damage)   │
│  → ParadoxBrand applied on every beam hit            │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Channeled Beam → LaserFoundation (ConvergenceBeamShader)
- 4 detail textures themed with void energy patterns (Simplex noise overlay)
- `BaseBeamWidth = 35f` base → `35f + 25f * min(channelTime/120, 1)` = max 60f at full ramp
- Void gradient LUT: VoidBlack → DeepPurple → GreenFlame → WhiteGreenFlash
- Unravel point markers rendered as MaskFoundation mini-orbs along beam

#### Snap-Back → ExplosionParticlesFoundation + ImpactFoundation
- On beam release: all stored unravel points detonate simultaneously
- Per point: `SparkCount = 20`, eerie green sparks + void purple accents
- ImpactFoundation (RippleShader): `ringCount = 3`, purple→green expansion per detonation
- DamageZoneShader: brief lingering zone at each point (30-frame duration)

---

## 2. Dissonance of Secrets (Magic — Growing Orb)

### Identity & Musical Soul
A riddle that grows larger the longer it exists — a void orb that swells and consumes, periodically releasing riddlebolts like forbidden knowledge escaping containment.

### Lore Line
*"Some things grow when you don't look at them."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  DISSONANCE OF SECRETS — Foundation Architecture    │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MagicOrbFoundation (core orb rendering)       │  │
│  │  → Growing void orb body (0.5→2.0 scale)     │  │
│  │  → RadialNoiseMaskShader at orb center        │  │
│  │  → 3-layer bloom scaling with growth          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Aura damage zone visualization             │  │
│  │  → Voronoi noise, OrbDrawScale = growth×0.8  │  │
│  │  → Pulsing purple→green→black                │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (riddlebolts)     │  │
│  │  → Homing riddlebolt shimmer trail            │  │
│  │  → SparkleTrailShader: sparkleSpeed=3.0       │  │
│  │  → Mild green sparkle                         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (cascade)        │  │
│  │  → Cascade explosion on orb death             │  │
│  │  → SparkCount = 50×scale, void/green sparks   │  │
│  │  → 4 SeekingCrystals spawned                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → Cascade expansion rings                    │  │
│  │  → ringCount = 4, scales with orb size        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 1: Pure Bloom)         │  │
│  │  → Orb flight trail (short, ominous glow)     │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → DissonanceOrbAura.fx (DissonanceOrbAuraMain)      │
│  → DissonanceRiddleTrail.fx (DissonanceRiddleFlow)   │
│  UNIQUE SYSTEMS:                                    │
│  → Growing scale (0.5→2.0 over 5s lifetime)         │
│  → Velocity decay (*0.985/frame)                     │
│  → Aura damage every 15 frames (30%, radius×scale)  │
│  → Riddlebolt spawns every 60 frames                 │
└─────────────────────────────────────────────────────┘
```

---

## 3. Fugue of the Unknown (Magic — Orbiter)

### Identity & Musical Soul
A fugue — multiple voices weaving independently, then converging. Orbiting voice projectiles that the player accumulates and releases, building EchoMark stacks toward devastating Harmonic Convergence.

### Lore Line
*"Five voices. One question. No answer."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FUGUE OF THE UNKNOWN — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation (voice rendering) │  │
│  │  → FugueVoice orbit/release shimmer trail     │  │
│  │  → SparkleTrailShader: subtle green shimmer   │  │
│  │  → CrystalShimmerShader for voice body        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Harmonic Convergence burst effect          │  │
│  │  → FBM noise, void purple → green → white    │  │
│  │  → OrbDrawScale = 0.6f, triggered at 5 marks │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: EchoMark accumulation ring   │  │
│  │  → DamageZone: Convergence zone (chain dmg)   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 4: Harmonic Wave)      │  │
│  │  → Voice orbit path visualization             │  │
│  │  → Standing wave pattern matching fugue theme │  │
│  │  → Purple→green color alternation             │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → FugueVoiceTrail.fx (FugueVoiceFlow)               │
│  → FugueConvergence.fx (FugueConvergenceWave)        │
│  UNIQUE SYSTEMS:                                    │
│  → Max 5 orbiting voices                            │
│  → Right-click releases all voices (homing + spiral) │
│  → EchoMark debuff stacking (5 marks → Convergence) │
│  → Convergence: 5x dmg to primary, 3x chain to all  │
└─────────────────────────────────────────────────────┘
```

---

## 4. Tacet's Enigma (Ranged — Gun)

### Identity & Musical Soul
"Tacet" — silence in a musical score. A gun that fires in silence, building invisible paradox stacks until the target is overwhelmed. Every 4th shot shatters the silence with a Paradox Bolt.

### Lore Line
*"The most dangerous note is the one you never hear."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  TACET'S ENIGMA — Foundation Architecture           │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → Bullet tracer trail (short, subtle)        │  │
│  │  → 15-point trail, void purple with green tip │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (Paradox Bolt)    │  │
│  │  → Every 4th shot enhanced Paradox Bolt       │  │
│  │  → SparkleTrailShader: spinning glyph trail   │  │
│  │  → 2.5x damage, penetrate=4                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: paradox explosion (10 stacks)│  │
│  │  → DamageZone: explosion zone, 200 radius     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (paradox burst)  │  │
│  │  → 10-stack paradox explosion spark burst     │  │
│  │  → SparkCount = 40, purple/green void sparks  │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → TacetBulletTrail.fx (TacetBulletFlow)             │
│  → TacetParadoxExplosion.fx (TacetParadoxBlast)      │
│  UNIQUE SYSTEMS:                                    │
│  → Every 4th shot = Paradox Bolt (2.5x, 4 pierce)  │
│  → 10-stack paradox explosion (200 radius, 50% AoE) │
│  → Chain damage (3 chains, 250 range, 30% dmg)      │
└─────────────────────────────────────────────────────┘
```

---

## 5. The Silent Measure (Ranged — Bow)

### Identity & Musical Soul
A measure of silence — arrows that ask questions and seek answers. Each arrow splits into 3 homing seekers on first impact, and every 5th shot is a paradox-piercing bolt of revelation.

### Lore Line
*"The question has weight. The answer, none."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE SILENT MEASURE — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → Arrow tracer trail rendering               │  │
│  │  → 20-point trail, QuestionViolet → green tip │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (seekers + bolt)  │  │
│  │  → HomingQuestionSeeker shimmer trail          │  │
│  │  → ParadoxPiercingBolt enhanced trail          │  │
│  │  → SparkleTrailShader: enigma green sparkle   │  │
│  │  → CursiveMusicNote texture on seekers        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (chain lightning)         │  │
│  │  → Chain lightning between hit targets         │  │
│  │  → MaxBounces=1, BaseBeamWidth=6f             │  │
│  │  → Green→white gradient, fast fade             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Arrow impact ring + seeker impact ring     │  │
│  │  → ringCount=2, green/purple pulse            │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → SilentSeekerTrail.fx (SilentSeekerFlow)          │
│  → SilentQuestionBurst.fx (SilentQuestionBlast)      │
│  UNIQUE SYSTEMS:                                    │
│  → Arrow split: first hit → 3 homing seekers (50%) │
│  → Every 5th shot = Paradox Piercing Bolt (2x, p5)  │
│  → Chain lightning (200-300 range, 30-40% dmg)       │
│  → SeekingCrystals (2 per arrow, 20% dmg)           │
└─────────────────────────────────────────────────────┘
```

---

## 6. The Unresolved Cadence (Melee — Ultimate)

### Identity & Musical Soul
A cadence that never resolves — the tension that builds and builds until reality collapses. The **ultimate Enigma melee weapon** (600 base damage). Each swing adds Inevitability stacks to ALL on-screen enemies, and when any target reaches 10 stacks, a Paradox Collapse obliterates everything.

### Lore Line
*"Resolution is an illusion. There is only the next question."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE UNRESOLVED CADENCE — Foundation Architecture   │
├─────────────────────────────────────────────────────┤
│  BASE: MeleeSwingItemBase + MeleeSwingBase          │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Per-phase escalating void smear            │  │
│  │  → DeepPurple→GreenFlame→WhiteGreenFlash LUT  │  │
│  │  → distortStrength: 0.06→0.08→0.14 by phase  │  │
│  │  → Phase 2 uses FullCircleSwordArcSlash smear │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → CalamityStyleTrailRenderer.TrailStyle.Cosmic│  │
│  │  → 40-point trail, void purple/green          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Dimensional slash marks on-hit             │  │
│  │  → Eerie green / void purple variants         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Paradox Collapse)           │  │
│  │  → Paradox Collapse cross-tear                │  │
│  │  → XSlashShader: fireIntensity = 0.10         │  │
│  │  → Void distortion, purple→green→white        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (All 3 shaders)              │  │
│  │  → RippleShader: paradox rings on swing hit   │  │
│  │  → SlashMarkShader: dimensional cut marks     │  │
│  │  → DamageZone: Collapse lingering void zone   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Collapse)       │  │
│  │  → Paradox Collapse detonation sparks         │  │
│  │  → SparkCount = 80, void/green/white sparks   │  │
│  │  → 400→800 expanding hitbox visual            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (dimensional slash)           │  │
│  │  → Void smoke wisps from DimensionalSlash     │  │
│  │  → Dark purple, slow dissipation              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Collapse)          │  │
│  │  → Camera → The Silence phase → Collapse AoE  │  │
│  │  → Screen effects: void tint, chromatic        │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → CadenceSwingTrail.fx (CadenceSwingGlow)           │
│  → CadenceCollapse.fx (CadenceCollapseWarp)          │
│  UNIQUE SYSTEMS:                                    │
│  → Inevitability stacking (P0=2, P1=3, P2=5 stacks) │
│  → 10 stacks → Paradox Collapse (400x400, 3x dmg)   │
│  → DimensionalSlash sub-projectiles (P1: 35%, P2: 50%)│
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (3-Phase Combo)
| Phase | Name | Duration | BladeLength | DamageMult | DimensionalSlash | Foundation |
|-------|------|----------|-------------|------------|-----------------|-----------|
| 0 | The Question | 18 | 150 | 0.85× | — | SwordSmear + Ribbon |
| 1 | The Doubt | 20 | 155 | 1.0× | 1 (35% dmg) | SwordSmear + Ribbon + ThinSlash |
| 2 | The Silence | 25 | 168 | 1.3× | 1+2 perp (50%) | ALL foundations + XSlash + AttackAnim |

---

## 7. The Watching Refrain (Summon)

### Identity & Musical Soul
The watcher — a phantom that observes, judges, and creates mystery zones of crowd control. Playing the refrain that never changes but always disturbs.

### Lore Line
*"It watches. It always watches."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE WATCHING REFRAIN — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → MysteryZone rendering (160×160 AoE zone)   │  │
│  │  → Simplex noise, rotationSpeed=0.15          │  │
│  │  → OrbDrawScale = 0.8f per zone               │  │
│  │  → Purple→green→black pulsing                 │  │
│  │  → Slow (0.85×) + pull enemies to center      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (phantom bolt)    │  │
│  │  → PhantomBolt shimmer trail                  │  │
│  │  → SparkleTrailShader: ghostly green sparkle  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: phantom bolt impact ring     │  │
│  │  → DamageZone: PhantomRift lingering zone     │  │
│  │  → 60-frame lingering void damage area        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 1: Pure Bloom)         │  │
│  │  → Phantom minion movement trail              │  │
│  │  → Ghostly, low-opacity, eerie green glow     │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → WatchingPhantomAura.fx (WatchingPhantomGhost)     │
│  → WatchingMysteryZone.fx (WatchingMysteryField)     │
│  UNIQUE SYSTEMS:                                    │
│  → UnsolvedPhantomMinion (1 slot, 150u hover)       │
│  → PhantomBolt attacks every 40 frames               │
│  → MysteryZone every 300 frames (slow + pull + dmg) │
│  → PhantomRift on bolt hit (lingering AoE)           │
└─────────────────────────────────────────────────────┘
```

---

## 8. Variations of the Void (Melee)

### Identity & Musical Soul
The variations — a weapon that reimagines the same attack through different lenses. 3-phase combo that escalates, culminating in converging triple void beams that align into a Void Resonance Explosion.

### Lore Line
*"The void does not vary. You do."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  VARIATIONS OF THE VOID — Foundation Architecture   │
├─────────────────────────────────────────────────────┤
│  BASE: MeleeSwingItemBase + MeleeSwingBase          │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Per-phase void smear overlay               │  │
│  │  → VoidBlack→Purple→GreenFlame gradient LUT   │  │
│  │  → distortStrength: 0.05→0.07→0.12            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (triple void beams)    │  │
│  │  → VoidConvergenceBeamSet rendering           │  │
│  │  → 3 beams, 30° spread → convergence          │  │
│  │  → InfernalBeamBodyShader, void gradient      │  │
│  │  → MaxBeamLength = 600f per beam              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → CalamityStyleTrailRenderer.TrailStyle.Cosmic│  │
│  │  → 40-point void purple/green trail           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → DimensionalSlash impact marks              │  │
│  │  → Green/purple void cut marks                │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: VoidResonanceExplosion rings │  │
│  │  → DamageZone: convergence blast zone          │  │
│  │  → Expanding hitbox (100→300) visual           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (convergence)    │  │
│  │  → VoidResonanceExplosion sparks              │  │
│  │  → SparkCount = 60, void/green/white sparks   │  │
│  │  → Rotating X-shaped beam arms visual         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (dimensional slash)           │  │
│  │  → Void smoke from DimensionalSlash sub-projs │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → VoidVariationSwingTrail.fx (VoidVariationSwingGlow)│
│  → VoidVariationBeam.fx (VoidVariationBeamFlow)      │
│  UNIQUE SYSTEMS:                                    │
│  → Shared DimensionalSlash proj with Cadence        │
│  → VoidConvergenceBeamSet (3 beams, 120-frame align) │
│  → Full alignment → VoidResonanceExplosion (3x, AoE) │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (3-Phase Combo)
| Phase | Name | Duration | BladeLength | DamageMult | Projectiles | Foundation |
|-------|------|----------|-------------|------------|------------|-----------|
| 0 | HorizontalSweep | 16 | 145 | 0.85× | — | SwordSmear + Ribbon |
| 1 | DiagonalSlash | 18 | 150 | 1.0× | 1 DimensionalSlash (33%) | SwordSmear + Ribbon + ThinSlash |
| 2 | HeavySlamFinisher | 24 | 165 | 1.25× | DimSlash×3 + VoidBeamSet | ALL foundations + InfernalBeam |

---

## Foundation Coverage Matrix

| Foundation | Cipher | Dissonance | Fugue | Tacet | Silent | Cadence | Watching | Void |
|-----------|--------|-----------|-------|-------|--------|---------|---------|------|
| SwordSmearFoundation | | | | | | ✅ | | ✅ |
| RibbonFoundation | ✅ M6 | ✅ M1 | ✅ M4 | ✅ M3 | ✅ M3 | ✅ M7 | ✅ M1 | ✅ M7 |
| ThinSlashFoundation | | | | | | ✅ | | ✅ |
| XSlashFoundation | | | | | | ✅ Collapse | | |
| ImpactFoundation | ✅ Rip+DZ | ✅ Ripple | ✅ Rip+DZ | ✅ Rip+DZ | ✅ Ripple | ✅ All3 | ✅ Rip+DZ | ✅ Rip+DZ |
| LaserFoundation | ✅ | | | | | | | |
| ThinLaserFoundation | | | | | ✅ | | | |
| InfernalBeamFoundation | | | | | | | | ✅ |
| ExplosionParticles | ✅ SnapBk | ✅ Cascade | | ✅ Paradox | | ✅ Collapse | | ✅ Converg |
| SmokeFoundation | | | | | | ✅ | | ✅ |
| SparkleProjectile | | ✅ Riddle | ✅ Voice | ✅ PBolt | ✅ Seeker | | ✅ PBolt | |
| MaskFoundation | | ✅ Aura | ✅ Converg | | | | ✅ Zone | |
| MagicOrbFoundation | | ✅ | | | | | | |
| AttackAnimation | | | | | | ✅ Collapse | | |

### Enigma Lore Consistency
- All lore references mystery, paradox, questions, ciphers, void, watching, silence
- NEVER moonlight, fire, sakura, cosmos — those belong to other themes
- Foundation parameters always use void purple/eerie green gradient LUTs
- ParadoxBrand is the UNIVERSAL debuff — every weapon applies it
- Voronoi noise preferred for melee weapons, Simplex for magic/ranged (different void textures)
