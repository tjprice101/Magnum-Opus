# 🔔 La Campanella — Resonance Weapons Planning

> *"The ringing bell, virtuosic fire — passion made sound, sound made flame."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Liszt's La Campanella — the ringing bell, virtuosic fire |
| **Emotional Core** | Passion, intensity, burning brilliance |
| **Color Palette** | Black smoke, orange flames, gold highlights |
| **Palette Hex** | SootBlack `(20, 12, 8)` → DeepEmber `(140, 40, 15)` → InfernalOrange `(255, 120, 30)` → FlameYellow `(255, 200, 60)` → BellGold `(255, 220, 120)` → WhiteHot `(255, 245, 220)` |
| **Lore Color** | `new Color(255, 140, 40)` — Infernal Orange |
| **Lore Keywords** | Fire, bells, chimes, resonance, passion, intensity, inferno, virtuosity |
| **VFX Language** | Heavy black smoke billowing, orange flames crackling, bell chime shockwaves, ember scatter, molten trails, geyser pillars, infernal detonations |

### Shared Infrastructure (Already Exists)
| System | Purpose |
|--------|---------|
| `LaCampanellaPalette.cs` | 353-line palette — core 6-stop fire gradient + per-weapon blade palettes |
| `LaCampanellaVFXLibrary.cs` | 904-line VFX library — bloom stacking, shader setup, trail helpers, music notes, dust, impacts |
| `LaCampanellaShaderManager.cs` | 288-line shader manager — 7 weapon presets wrapping shared shaders |
| `LaCampanellaThemeTextures` | Lazy-loaded texture registry (Impact, Beam, Projectile, Trail, Noise, LUT textures) |

---

## Foundation Weapons Integration Map

La Campanella weapons already have extensive self-contained VFX systems (each weapon has 3 dedicated .fx shaders, custom particles, and primitive renderers). Foundations serve as the **structural backbone beneath these custom systems** — providing the mesh construction, blend state management, and rendering pipeline that weapon-specific shaders plug into.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Dual Fated Chime, Ignition of the Bell | Swing arc smear — SmearDistortShader as base, weapon shaders add fire customization on top |
| **RibbonFoundation** | All 7 weapons | Trail strips — various modes per weapon (Ember Drift for melee, Energy Surge for beams, Basic Trail for bullets) |
| **ThinSlashFoundation** | Dual Fated Chime | Thin flame slash marks — ThinSlashShader SDF in infernal orange |
| **XSlashFoundation** | Dual Fated Chime (Grand Toll), Ignition (Chimequake) | Cross-detonation effects — XSlashShader with fire scrolling |
| **ImpactFoundation** | All 7 weapons | Hit VFX — RippleShader for bell shockwaves, DamageZoneShader for fire zones, SlashMarkShader for cuts |
| **InfernalBeamFoundation** | Grandiose Chime, Ignition of the Bell | InfernalBeamBodyShader — golden beam body, infernal geyser pillars |
| **LaserFoundation** | Grandiose Chime | ConvergenceBeamShader for main golden beam with 4 detail textures |
| **ThinLaserFoundation** | Fang of the Infinite Bell | ThinBeamShader for lightning arcs between bouncing orbs |
| **ExplosionParticlesFoundation** | Dual Fated Chime, Ignition, Symphonic Annihilator, Infernal Chimes | Spark bursts — bell shatter, geyser sparks, rocket explosions, sacrifice detonations |
| **SmokeFoundation** | Ignition (Cyclone), Symphonic (rockets), Infernal Chimes | Heavy fire smoke — cyclone smoke ring, rocket exhaust, sacrifice smoke |
| **SparkleProjectileFoundation** | Fang (bell orbs), Piercing Bells (seeking crystals) | SparkleTrailShader for glittering orb/crystal trails |
| **MaskFoundation** | Fang (Empowered Aura), Infernal Chimes (summoning circle) | RadialNoiseMaskShader for empowerment aura and summoning rituals |
| **MagicOrbFoundation** | Fang of the Infinite Bell | OrbBolt pattern for bouncing bell orbs and echo children |
| **AttackAnimationFoundation** | Dual Fated Chime (Grand Toll), Ignition (Chimequake) | Cinematic finisher sequences |

---

## Weapons Overview

| # | Weapon | Class | Damage | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Dual Fated Chime | Melee | 380 | 5-phase Inferno Waltz combo → Grand Toll → Bell Shatter |
| 2 | Ignition of the Bell | Melee | 340 | 3-phase thrust combo → Chime Cyclone → Chimequake |
| 3 | Fang of the Infinite Bell | Magic | 95 | Bouncing bell orbs + echo children + Infinite Crescendo |
| 4 | Grandiose Chime | Ranged | 240 | Golden beam + Kill Echo Chains + bell-note mines |
| 5 | Piercing Bell's Resonance | Ranged | 165 | Staccato marker bullets + seeking crystals + Resonant Detonation |
| 6 | Symphonic Bellfire Annihilator | Ranged | 494 | Bell shockwaves + bellfire rockets + Symphonic Overture |
| 7 | Infernal Chimes' Calling | Summon | 145 | Spectral bell choir (5 minions) + Infernal Crescendo + Bell Sacrifice |

---

## 1. Dual Fated Chime (Melee)

### Identity & Musical Soul
Twin bells of fate — an alternating dual-wield combo that builds resonance stacks like a bell being struck harder and harder. The weapon IS a bell, and every swing RINGS.

### Lore Line
*"Two bells. One fate. Infinite fire."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  DUAL FATED CHIME — Foundation Architecture         │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: ModItem (channeled, 5-phase combo)     │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Per-phase alternating swing smear overlay  │  │
│  │  → InfernalOrange→BellGold→WhiteHot LUT      │  │
│  │  → distortStrength: 0.07 → 0.14 by phase     │  │
│  │  → Left/Right alternation per phase           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 5: Ember Drift)        │  │
│  │  → Flame trail behind each swing              │  │
│  │  → 40-point ring buffer, ember-scatter UV     │  │
│  │  → Width scales with Bell Resonance stacks    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Thin flame slash marks on-hit              │  │
│  │  → Infernal orange / gold edge variants       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Grand Toll only)            │  │
│  │  → Grand Toll cross-detonation + Bell Shatter │  │
│  │  → fireIntensity = 0.15 (very intense)        │  │
│  │  → InfernalOrange → BellGold → WhiteHot       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: bell ring shockwave on-hit   │  │
│  │  → DamageZone: resonance zone from flame waves│  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Bell Shatter)   │  │
│  │  → Bell Shatter at 5 resonance stacks (80+)  │  │
│  │  → 12 directional BellFlameWaveProj sparks    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Grand Toll)        │  │
│  │  → Grand Toll cinematic sequence              │  │
│  │  → Camera pan → slam → 12 flame waves         │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on foundations):            │
│  → BellFlameTrail.fx (flame flow trail)              │
│  → InfernalFlameSlash.fx (flame slash overlay)       │
│  → InfernoWaltzAura.fx (waltz dodge aura)            │
│  UNIQUE SYSTEMS:                                    │
│  → Bell Resonance Stacking (max 5 → Bell Shatter)  │
│  → Flame Waltz Dodge (0.2s iframes on Toll 2 & 4)  │
│  → DualFatedChimePlayer tracker                      │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (5-Phase Inferno Waltz)
| Phase | Name | Duration | Action | Foundation |
|-------|------|----------|--------|-----------|
| 0 | Opening Peal | — | Initial strike | SwordSmear + Ribbon |
| 1 | Answer | — | Counter sweep | SwordSmear + Ribbon |
| 2 | Escalation | — | Rising intensity | SwordSmear + Ribbon + ThinSlash |
| 3 | Resonance | — | Full resonance + InfernoWaltzProj | SwordSmear + Ribbon + Impact |
| 4 | Grand Toll | — | 12 directional flame waves | ALL + XSlash + ExplosionParticles + AttackAnim |

### VFX Architecture — Foundation-Based

#### Swing Arcs → SwordSmearFoundation
- `gradientTex`: InfernalOrange → BellGold → WhiteHot (fire-themed LUT)
- `distortStrength` scales: 0.07 (Opening) → 0.10 (Escalation) → 0.14 (Grand Toll)
- `flowSpeed`: 0.6 → 1.0 with phase intensity
- Alternating left/right swing direction per phase

#### Trail → RibbonFoundation (Mode 5: Ember Drift)
- Ember-scatter texture, infernal orange core → gold edges
- `RibbonWidthHead = 22f × (1 + resonanceStacks * 0.15)` — widens with stacks
- `RibbonWidthTail = 3f`

#### Bell Ring → ImpactFoundation (RippleShader)
- `ringCount = 4`, `ringThickness = 0.08` — bell ring visual
- InfernalOrange → BellGold concentric ripples
- Ring expansion synced to "chime" timing

#### Grand Toll → XSlash + ExplosionParticles + AttackAnimation
1. **AttackAnimationFoundation**: Camera → slam → 12 flame waves → camera return
2. **XSlashFoundation**: `fireIntensity = 0.15`, `scrollSpeed = 0.8`, infernal fire cross
3. **ExplosionParticlesFoundation**: `SparkCount = 60`, infernal ember colors
4. **ImpactFoundation** (DamageZoneShader): Persistent flame zone from overlapping waves

---

## 2. Ignition of the Bell (Melee)

### Identity & Musical Soul
The ignition — a spear/thrust weapon that drives flames INTO the earth. Where Dual Fated Chime swings wide, Ignition thrusts deep. Ground geysers, fire cyclones, chimequakes.

### Lore Line
*"The bell does not ring. It ignites."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  IGNITION OF THE BELL — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: ModItem (channeled, 3-phase)           │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Forward thrust smear (directional)         │  │
│  │  → DeepEmber→InfernalOrange→WhiteHot LUT      │  │
│  │  → distortStrength: 0.08 (focused heat)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (InfernalBeamBody)     │  │
│  │  → Ground geyser pillar rendering             │  │
│  │  → Multi-texture compositing, vertical beam   │  │
│  │  → Infernal gradient, BaseBeamWidth = 50f     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Thrust trail energy surge behind weapon    │  │
│  │  → Short, intense, focused trail              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Chimequake only)            │  │
│  │  → Every 3rd Cyclone triggers Chimequake      │  │
│  │  → Massive ground X-cross detonation          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: geyser eruption ring         │  │
│  │  → DamageZone: cyclone pull zone              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (geysers)        │  │
│  │  → Geyser eruption spark burst (upward bias)  │  │
│  │  → Cyclone detonation radial scatter          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (Cyclone)                     │  │
│  │  → Fire cyclone heavy smoke ring              │  │
│  │  → Rotating smoke puffs, InfernalOrange core  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Chimequake)        │  │
│  │  → Chimequake cinematic ground-slam           │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → IgnitionThrustTrail.fx (thrust trail)             │
│  → InfernalGeyserShader.fx (vertical geyser)        │
│  → CycloneFlameShader.fx (vortex rendering)          │
│  UNIQUE SYSTEMS:                                    │
│  → IgnitionOfTheBellPlayer tracker                   │
│  → Chimequake counter (every 3rd cyclone)            │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (3-Phase Thrust Combo)
| Phase | Name | Projectile | Foundation |
|-------|------|-----------|-----------|
| 1 | Ignition Strike | IgnitionThrustProj + InfernalGeyserProj | SwordSmear + Ribbon + InfernalBeam + ExplosionParticles |
| 2 | Tolling Frenzy | 3× IgnitionThrustProj + smaller geysers | SwordSmear + Ribbon + Impact |
| 3 | Chime Cyclone | ChimeCycloneProj (2s pull + detonation) | Smoke + ImpactFoundation (DamageZone) + ExplosionParticles |

### VFX Architecture — Foundation-Based

#### Thrust Trail → SwordSmearFoundation + RibbonFoundation
- SmearDistortShader in thrust direction (narrower arc than Dual Fated Chime's wide swings)
- RibbonFoundation Mode 6 (Energy Surge) for focused directional trail

#### Ground Geysers → InfernalBeamFoundation
- **InfernalBeamBodyShader** rendered VERTICALLY (rotated 90°)
- `BaseBeamWidth = 50f`, infernal gradient LUT
- Multi-texture compositing: fire noise + ember detail + geyser body + glow
- ExplosionParticlesFoundation at geyser tip: `SparkCount = 25`, upward velocity bias

#### Cyclone → SmokeFoundation + ImpactFoundation
- **SmokeFoundation**: 40 puffs in rotating circular pattern, infernal orange smoke
- **ImpactFoundation** (DamageZoneShader): `circleRadius = 0.6`, pull zone, 2s duration
- Detonation: ExplosionParticlesFoundation `SparkCount = 50` + ImpactFoundation RippleShader

---

## 3. Fang of the Infinite Bell (Magic)

### Identity & Musical Soul
Bell-shaped energy orbs that bounce endlessly between enemies — the bell's echo that never fades. Each bounce amplifies, stacks build, and at maximum resonance the orbs erupt with lightning.

### Lore Line
*"Every echo louder than the last."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FANG OF THE INFINITE BELL — Foundation Architecture│
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MagicOrbFoundation (OrbBolt pattern)          │  │
│  │  → Main bell orb rendering (3-layer bloom)    │  │
│  │  → RadialNoiseMaskShader for orb body         │  │
│  │  → Echo children: half size OrbBolt copies    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Bell orb shimmer trail while bouncing      │  │
│  │  → SparkleTrailShader: sparkleSpeed=4.0       │  │
│  │  → Gold/orange sparkle trail                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Lightning arcs between airborne orbs       │  │
│  │  → MaxBounces=0, BaseBeamWidth=8f             │  │
│  │  → Triggers at 10+ bounce stacks              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Empowered Aura at 10+ stacks              │  │
│  │  → FBM noise, OrbDrawScale=0.3f              │  │
│  │  → InfernalOrange→BellGold glow              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Bounce impact ring (bell chime visual)     │  │
│  │  → ringCount = 2 per bounce                   │  │
│  │  → Orb explosion at 20 stacks (Ripple + DZ)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (orb explosion)  │  │
│  │  → At 20 stacks: orb detonation spark burst  │  │
│  │  → SparkCount = 35, gold/white sparks         │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → ArcaneOrbTrail.fx (orb trail)                     │
│  → EmpoweredAura.fx (stack aura)                     │
│  → EmpoweredLightning.fx (arc rendering)             │
│  UNIQUE SYSTEMS:                                    │
│  → Bounce stacking (+3% magic dmg/stack, max 20)   │
│  → Echo orbs (half dmg children on bounce)           │
│  → Infinite Crescendo alt-fire (10 bounces, 150%)   │
│  → InfiniteBellDamageBuff / EmpoweredBuff            │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Bell Orb → MagicOrbFoundation + SparkleProjectileFoundation
- **MagicOrbFoundation**: RadialNoiseMaskShader for bell-shaped orb body, 3-layer bloom (InfernalOrange core → BellGold mid → WhiteHot center)
- **SparkleProjectileFoundation**: SparkleTrailShader for continuous shimmer trail during flight
- Echo children: 50% scale MagicOrb copies with faster decay

#### Lightning Arcs → ThinLaserFoundation
- Triggers at 10+ bounce stacks
- `MaxBounces = 0`, `BaseBeamWidth = 8f`, `MaxSegmentLength = 400f`
- Gold → White gradient, rapid fade (15-frame lifetime per arc)
- Random arcs between active orbs every 8 ticks

#### Empowered State → MaskFoundation
- Player aura at 10+ stacks: `OrbDrawScale = 0.3f`, FBM noise, InfernalOrange pulse
- EmpoweredBuff visual indicator: +10% attack speed halo

#### Bounce Impact → ImpactFoundation (RippleShader)
- Each bounce: `ringCount = 2`, `ringThickness = 0.04`, small bell chime ring
- At 20 stacks: `ringCount = 5`, `ringThickness = 0.08` + DamageZoneShader for explosion zone

---

## 4. Grandiose Chime (Ranged — Beam)

### Identity & Musical Soul
A grand golden beam weapon — the bell's voice made visible. Wide devastating beam that triggers kill echoes like harmonics spreading from a struck bell.

### Lore Line
*"The bell speaks, and the world answers in fire."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  GRANDIOSE CHIME — Foundation Architecture          │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → Main golden beam body (4 detail textures)  │  │
│  │  → BaseBeamWidth = 80f (wide golden beam)     │  │
│  │  → Infernal gradient LUT                      │  │
│  │  → Triple width at Grandiose Crescendo        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (alt rendering)        │  │
│  │  → Grandiose Crescendo beam body              │  │
│  │  → InfernalBeamBodyShader at 150% scale       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → NoteMineProj orb rendering (floating mines)│  │
│  │  → FBM noise, pulsing gold, OrbDrawScale=0.15│  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: beam impact zone             │  │
│  │  → DamageZone: kill echo chain zones          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Kill Echo chain projectile trails          │  │
│  │  → Short, bright energy surge per chain       │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → GrandioseBeamShader.fx (beam body)                │
│  → MineShader.fx (floating note mine)                │
│  → BarrageShader.fx (crescendo barrage)              │
│  UNIQUE SYSTEMS:                                    │
│  → Kill Echo Chains (3 chains, 60% dmg each)        │
│  → Note Mines (max 5, 80% dmg, alt-fire)            │
│  → Grandiose Crescendo (5 chain kills → triple beam) │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Main Beam → LaserFoundation (ConvergenceBeamShader)
- `BaseBeamWidth = 80f`, 4 detail textures in infernal palette
- Infernal gradient LUT: SootBlack → InfernalOrange → BellGold → WhiteHot
- Normal mode: standard width. Crescendo mode: `BaseBeamWidth = 240f` (triple)

#### Crescendo Beam → InfernalBeamFoundation
- Activates during Grandiose Crescendo (5 chain kills)
- `BaseBeamWidth = 120f`, `MaxBeamLength = 3000f`
- Multi-texture compositing at 150% scale, infernal fire noise

#### Note Mines → MaskFoundation (RadialNoiseMaskShader)
- `OrbDrawScale = 0.15f`, FBM noise mode
- Gold pulsing orbs floating in position
- 3-layer: bloom halo → shader orb → gold core

#### Kill Echo → RibbonFoundation (Mode 6: Energy Surge) + ImpactFoundation
- Chain projectiles: short, bright energy surge trails
- Each chain terminus: ImpactFoundation RippleShader (`ringCount = 2`, gold)

---

## 5. Piercing Bell's Resonance (Ranged — Sniper)

### Identity & Musical Soul
Precision bullets that embed resonant markers like bell tolls — building to a devastating Resonant Detonation that punishes enemies for every toll they've absorbed.

### Lore Line
*"Count the tolls. Each one is a judgment."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  PIERCING BELL'S RESONANCE — Foundation Arch.       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → Staccato bullet tracer trail               │  │
│  │  → 15-point short trail, InfernalOrange       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → SeekingCrystalProj rendering (every 4th)   │  │
│  │  → SparkleTrailShader: gold crystal shimmer   │  │
│  │  → CrystalShimmerShader: facet glow           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: marker embed ring            │  │
│  │  → DamageZone: Resonant Detonation zone       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Detonation)     │  │
│  │  → Resonant Detonation spark burst            │  │
│  │  → SparkCount scales with marker count        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (marker visual on enemies)     │  │
│  │  → RadialNoiseMaskShader for resonant glyph   │  │
│  │  → Visible marker count on enemy              │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → BulletTrailShader.fx (bullet trail)               │
│  → CrystalGlowShader.fx (seeking crystal)           │
│  → ResonantBlastShader.fx (detonation blast)         │
│  UNIQUE SYSTEMS:                                    │
│  → ResonantMarkerNPC (per-enemy marker tracking)    │
│  → Perfect Pitch (exactly 5 markers = 2x + landmines)│
│  → Resonant Note landmines                           │
└─────────────────────────────────────────────────────┘
```

---

## 6. Symphonic Bellfire Annihilator (Ranged — Rockets)

### Identity & Musical Soul
The LOUDEST weapon in the entire mod — bell-shaped shockwaves and arcing rockets that crescendo toward a devastating Symphonic Overture. This is the climax of the concert.

### Lore Line
*"Fortissimo. Always fortissimo."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  SYMPHONIC BELLFIRE ANNIHILATOR — Foundation Arch.   │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → Grand Crescendo Wave expanding ring        │  │
│  │  → RippleShader: ringCount=6, massive bell    │  │
│  │  → Bell-shaped piercing shockwave visual      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Bellfire Rocket exhaust trail              │  │
│  │  → Short, hot, arcing trail with smoke        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (rocket impact)  │  │
│  │  → Rocket detonation spark burst              │  │
│  │  → SparkCount = 35 per rocket, fire colors    │  │
│  │  → Upward spray + radial scatter              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (rocket exhaust + impact)     │  │
│  │  → Rocket exhaust trailing smoke              │  │
│  │  → Impact crater smoke ring                   │  │
│  │  → PuffCount = 15 per impact                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (Symphonic Overture)   │  │
│  │  → Overture massive beam body                 │  │
│  │  → Both crescendos maxed = devastating beam   │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → RocketTrailShader.fx (rocket exhaust)             │
│  → ExplosionShader.fx (detonation flash)             │
│  → CrescendoShader.fx (crescendo wave overlay)       │
│  UNIQUE SYSTEMS:                                    │
│  → Grand Crescendo Buff (max 5, +10% wave/+8% dmg)  │
│  → Bellfire Crescendo Buff (max 3, +rocket split)    │
│  → Symphonic Overture (both maxed → 200% beam)       │
└─────────────────────────────────────────────────────┘
```

---

## 7. Infernal Chimes' Calling (Summon)

### Identity & Musical Soul
A choir of spectral bells — 5 floating minions that attack in musical sequence, their shockwaves overlapping to create harmony. Bell Sacrifice is the ultimate act of devotion — one bell gives its life for devastating damage.

### Lore Line
*"The choir sings in flame. The encore is silence."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  INFERNAL CHIMES' CALLING — Foundation Architecture │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Summoning circle ritual on cast            │  │
│  │  → Cosmic noise, rotationSpeed = 0.25         │  │
│  │  → Infernal orange → gold glow circle         │  │
│  │  → Minion flame aura (small scale per bell)   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 5: Ember Drift)        │  │
│  │  → Minion movement trail rendering            │  │
│  │  → Per-bell ember drift trail                 │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: shockwave per minion attack  │  │
│  │  → DamageZone: Harmonic Convergence overlap   │  │
│  │  → Overlapping shockwaves = 2x damage visual  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Sacrifice)      │  │
│  │  → Bell Sacrifice AoE detonation (3x damage)  │  │
│  │  → SparkCount = 90, massive gold/flame burst  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (Sacrifice)                   │  │
│  │  → Sacrifice smoke mushroom                   │  │
│  │  → PuffCount = 35, rising infernal smoke      │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → ChoirMinionTrail.fx (minion trail)                │
│  → ChoirFlameAura.fx (per-bell aura)                 │
│  → MusicalShockwave.fx (shockwave rendering)         │
│  UNIQUE SYSTEMS:                                    │
│  → Sequential attack pattern (0.3s stagger)          │
│  → Harmonic Convergence (overlap = 2x damage)        │
│  → Infernal Crescendo (every 12s synchronized)       │
│  → Bell Sacrifice (1 bell detonates, 15s respawn)    │
│  → CampanellaChoirBuff persistence                   │
└─────────────────────────────────────────────────────┘
```

---

## Foundation Coverage Matrix

| Foundation | Dual Fated | Ignition | Fang | Grandiose | Piercing | Symphonic | Infernal Chimes |
|-----------|-----------|----------|------|-----------|----------|-----------|----------------|
| SwordSmearFoundation | ✅ | ✅ | | | | | |
| RibbonFoundation | ✅ M5 | ✅ M6 | | ✅ M6 | ✅ M3 | ✅ M6 | ✅ M5 |
| ThinSlashFoundation | ✅ | | | | | | |
| XSlashFoundation | ✅ Grand | ✅ Quake | | | | | |
| ImpactFoundation | ✅ Rip+DZ | ✅ Rip+DZ | ✅ Rip | ✅ Rip+DZ | ✅ Rip+DZ | ✅ Ripple | ✅ Rip+DZ |
| InfernalBeamFoundation | | ✅ Geyser | | ✅ Crescendo | | ✅ Overture | |
| LaserFoundation | | | | ✅ | | | |
| ThinLaserFoundation | | | ✅ | | | | |
| ExplosionParticles | ✅ Shatter | ✅ Geyser | ✅ Orb | | ✅ Detonate | ✅ Rocket | ✅ Sacrifice |
| SmokeFoundation | | ✅ Cyclone | | | | ✅ Rocket | ✅ Sacrifice |
| SparkleProjectile | | | ✅ Orb | | ✅ Crystal | | |
| MaskFoundation | | | ✅ Aura | ✅ Mine | ✅ Marker | | ✅ Summon |
| MagicOrbFoundation | | | ✅ | | | | |
| AttackAnimation | ✅ Grand | ✅ Quake | | | | | |

### La Campanella Lore Consistency
- All lore references fire, bells, chimes, resonance, passion, inferno
- NEVER moonlight, petals, void, cosmos — those belong to other themes
- Foundation parameters always use infernal/fire gradient LUTs (SootBlack → InfernalOrange → BellGold → WhiteHot)
- Bell chime shockwave rings are the signature impact style (RippleShader with high ringCount)
