# ⚔️ Eroica — Resonance Weapons Planning

> *"The hero's symphony — courage, sacrifice, triumphant glory."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Symphony No. 3 "Eroica" — the hero's journey |
| **Emotional Core** | Courage, sacrifice, triumphant glory |
| **Color Palette** | Scarlet, crimson, gold, sakura pink |
| **Palette Hex** | Black `(10, 5, 5)` → Scarlet `(200, 50, 50)` → Crimson `(165, 25, 55)` → Gold `(255, 200, 50)` → Sakura `(255, 150, 180)` → HotCore `(255, 245, 220)` |
| **Lore Color** | `new Color(200, 50, 50)` — Scarlet |
| **Lore Keywords** | Heroism, sacrifice, glory, triumph, blaze, sakura petals, valor |
| **VFX Language** | Roaring flames, sakura petals scattering, golden light breaking through scarlet fire, rising embers ascending, sword afterimages, heroic flash |

### Shared Infrastructure (Already Exists)
| System | Purpose |
|--------|---------|
| `EroicaPalette.cs` | 6-color musical dynamic scale + per-weapon blade palettes |
| `EroicaVFXLibrary.cs` | 1449-line shared VFX library — palette interpolation, bloom stacking, shader setup, trail helpers, music notes, dust, impacts |
| `EroicaShaderManager.cs` | 1122-line shader manager — all weapon-specific shaders wrapped |
| `EroicaTextures.cs` | 320-line centralized texture registry |
| `EroicaColorInfo.cs` | Dust color types, per-weapon accent colors, gradient speed constants |

---

## Foundation Weapons Integration Map

Every Eroica weapon MUST layer Foundation Weapon systems into its VFX pipeline. The Eroica theme already has substantial custom shader infrastructure (HeroicFlameTrail, SakuraSwingTrail, etc.), so Foundations serve as the **structural rendering backbone** that these custom shaders build on top of.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Celestial Valor, Sakura's Blossom | Swing arc overlay via SmearDistortShader — scarlet/gold/sakura gradient LUTs |
| **RibbonFoundation** | Celestial Valor, Sakura's Blossom, Funeral Prayer, Blossom of the Sakura | Trail strips — Ember Drift for Valor, Harmonic Wave for Sakura, Energy Surge for Funeral, Basic Trail for Blossom |
| **ThinSlashFoundation** | Celestial Valor, Sakura's Blossom | Impact slash marks — ThinSlashShader SDF line in crimson/gold |
| **XSlashFoundation** | Celestial Valor (Gloria), Sakura's Blossom (Final Bloom) | Finisher cross-impact — XSlashShader fire distortion in scarlet/gold |
| **ImpactFoundation** | All 7 weapons | Hit VFX — RippleShader for shockwaves, DamageZoneShader for flame zones, SlashMarkShader for melee cuts |
| **ExplosionParticlesFoundation** | Celestial Valor, Piercing Light, Finality | Burst detonations — Gloria/Culmination/Final Bloom explosions |
| **SmokeFoundation** | Celestial Valor, Funeral Prayer, Finality | Flame smoke — rising heroic/funeral ash smoke |
| **LaserFoundation** | Funeral Prayer (Requiem Beam) | ConvergenceBeamShader for main beam body |
| **ThinLaserFoundation** | Funeral Prayer (Ricochet) | ThinBeamShader with MaxBounces=5 for chain beams |
| **SparkleProjectileFoundation** | Triumphant Fractal, Piercing Light | SparkleTrailShader for glimmering projectile trails |
| **MaskFoundation** | Triumphant Fractal, Finality | RadialNoiseMaskShader — fractal energy orbs and summoning circle |
| **MagicOrbFoundation** | Triumphant Fractal | OrbBolt pattern for fractal splitting children |
| **AttackAnimationFoundation** | Celestial Valor (Gloria), Sakura's Blossom (Final Bloom) | Cinematic finisher sequences |

---

## Weapons Overview

| # | Weapon | Class | Damage | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Celestial Valor | Melee | 320 | 4-phase Heroic Crescendo combo → Gloria finale |
| 2 | Sakura's Blossom | Melee | 350 | 3-phase Petal Dance + Sakura Meditation |
| 3 | Funeral Prayer | Magic | 340 | Funeral pyre projectiles + ricochet beam chains |
| 4 | Triumphant Fractal | Magic | 518 | Homing fractal bolts + recursive splitting + Triumph Accumulator |
| 5 | Blossom of the Sakura | Ranged | 75 | Heat-reactive homing bullets + Tracer Blossom marking |
| 6 | Piercing Light of the Sakura | Ranged | 155 | Piercing crescendo shots + SakuraLightning AoE |
| 7 | Finality of the Sakura | Summon | 320 | Spectral sakura spirit + petal volleys + Final Bloom supernova |

---

## 1. Celestial Valor (Melee Broadsword)

### Identity & Musical Soul
The Celestial Valor is the **hero's blade** — a weapon that plays a four-movement symphony of combat. Each phase builds in intensity like orchestral movements, culminating in the Gloria — a moment of transcendent fury. Flames, afterimages, and valor gauges.

### Lore Line
*"Rise, even when the world says fall."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CELESTIAL VALOR — Foundation Architecture          │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: ModItem (channeled combo system)       │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Per-phase swing arc smear overlay          │  │
│  │  → Scarlet→Gold→White gradient LUT            │  │
│  │  → distortStrength: 0.06 → 0.12 by phase     │  │
│  │  → 3-layer rendering (outer/main/core)        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 5: Ember Drift)        │  │
│  │  → Blade trail with ember-scatter texture     │  │
│  │  → 40-point ring buffer, width 24→4 taper     │  │
│  │  → Crimson core, gold edges                   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Impact slash marks on-hit                  │  │
│  │  → Crimson / Fire Edge style                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + SlashMark)         │  │
│  │  → RippleShader: heroic shockwave on-hit      │  │
│  │  → SlashMark: directional cut marks           │  │
│  │  → DamageZone: ValorBoom AoE zone             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Gloria only)    │  │
│  │  → Gloria Fortissimo detonation sparks        │  │
│  │  → 70 sparks, sakura petal + ember colors     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (Gloria only)                 │  │
│  │  → Rising heroic flame smoke                  │  │
│  │  → Crimson core → gold body → gray edge       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Gloria only)                │  │
│  │  → Gloria cross-slash detonation              │  │
│  │  → XSlashShader: fireIntensity = 0.12         │  │
│  │  → Scarlet → Gold → White                     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Gloria only)       │  │
│  │  → 270° slam cinematic sequence               │  │
│  │  → Camera pan + multi-slash + screen flash    │  │
│  │  → B&W brightness during slam                 │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on top of foundations):    │
│  → CelestialValorTrail.fx (HeroicTrail/ValorFlare) │
│  → ValorAura.fx (Valor Gauge visual ring)           │
│  UNIQUE SYSTEMS:                                    │
│  → Valor Gauge (0-100, HeroicBurn/ValorStagger)     │
│  → Hero's Resolve (<30% HP → +25% + ember VFX)      │
│  → 4-layer PreDraw pipeline                         │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (4-Phase Heroic Crescendo)
| Phase | Name | Duration | Swings | Projectile | Foundation |
|-------|------|----------|--------|-----------|-----------|
| 0 | Resolute Strike | 20 ticks | 1 overhead | — | SwordSmear + Ribbon |
| 1 | Ascending Valor | 24 ticks | 1 diagonal | 1× ValorBeam | SwordSmear + Ribbon + ThinSlash |
| 2 | Crimson Legion | 30 ticks | 3 rapid | 3× ValorBeam spread | SwordSmear + Ribbon + ThinSlash + Impact |
| 3 | Finale Fortissimo | 26 ticks | 1 slam (270°) | ValorBoom AoE | ALL foundations + AttackAnimation |

**Gloria Trigger**: Valor Gauge ≥95 → Phase 3 becomes Gloria (2× explosion, +screen shake, 18 sakura petals, 12 music notes)

### VFX Architecture — Foundation-Based

#### Swing Arcs → SwordSmearFoundation (SmearDistortShader)
- `gradientTex`: Scarlet→Gold→White LUT per phase (darkens for Phase 0, brightens for Gloria)
- `distortStrength`: 0.06 (Phase 0) → 0.08 (Phase 1) → 0.10 (Phase 2) → 0.12 (Phase 3/Gloria)
- `flowSpeed`: 0.4 → 0.9 scaling with phase intensity
- 3-layer rendering: outer glow (Crimson, α=0.3) → main smear (Scarlet→Gold) → core (White, α=0.8)

#### Trail → RibbonFoundation (Mode 5: Ember Drift)
- 40-point ring buffer, `RibbonWidthHead = 24f`, `RibbonWidthTail = 4f`
- Ember-scatter texture UV, crimson core → gold edges
- Width scales with phase: 1.0× → 1.1× → 1.2× → 1.4× (Gloria)

#### Impacts → ImpactFoundation (Multi-Shader)
- **RippleShader**: `ringCount = 3`, `ringThickness = 0.07` → crimson rings on regular hits
- **SlashMarkShader**: Directional cut marks along swing angle, crimson with gold edge
- **DamageZoneShader**: ValorBoom persistent zone, `circleRadius = 0.5`, gold-crimson scroll

#### Gloria Detonation → ExplosionParticles + Smoke + XSlash + AttackAnimation
1. **AttackAnimationFoundation**: Camera pan → 270° slam → camera return
2. **XSlashFoundation**: `fireIntensity = 0.12`, `scrollSpeed = 0.6`, Scarlet→Gold→White LUT
3. **ExplosionParticlesFoundation**: `SparkCount = 70`, `MaxLifetime = 120`, sakura + ember colors
4. **SmokeFoundation**: `PuffCount = 25`, `MaxLifetime = 50`, heroic crimson smoke ring
5. Screen flash via `DeathHeroicFlash` from EroicaVFXLibrary

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| CelestialValorSwing | **SwordSmear** + **Ribbon** (core rendering) | 4-phase combo projectile |
| CelestialValorProjectile | **RibbonFoundation** (Mode 1: Pure Bloom) | Thrown flame proj, 12-pos trail |
| ValorBeam | **SparkleProjectileFoundation** (shimmer trail) | Golden-crimson energy bolt |
| ValorBoom | **ImpactFoundation** (DamageZone) + **ExplosionParticles** | AoE shockwave detonation |
| ValorSlash | **ThinSlashFoundation** (ThinSlashShader) | Crimson-gold slash arc |

---

## 2. Sakura's Blossom (Melee Katana)

### Identity & Musical Soul
Where Celestial Valor is the hero's fiery charge, Sakura's Blossom is the **warrior's graceful dance** — petals falling from a cherry tree as the blade sweeps through enemies. Flowing, elegant slashes with petal bursts and phantom blades. The katana that makes death beautiful.

### Lore Line
*"A single petal falls. An army follows."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  SAKURA'S BLOSSOM — Foundation Architecture         │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: ModItem (channeled combo system)       │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Petal-themed swing smear overlay           │  │
│  │  → Sakura pink→white gradient LUT             │  │
│  │  → distortStrength: 0.04 (graceful, lighter)  │  │
│  │  → flowSpeed: 0.5 (slower = more graceful)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 4: Harmonic Wave)      │  │
│  │  → 40-point flowing trail with standing wave  │  │
│  │  → Pink → white → transparent taper           │  │
│  │  → `sin(t * π/2)` easing for graceful arcs    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Clean katana cuts on impact                │  │
│  │  → Ice Cyan / Sakura Pink style variants      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + SlashMark)         │  │
│  │  → RippleShader: sakura ring pulse on-hit     │  │
│  │  → SlashMark: clean katana cut lines          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Final Bloom only)           │  │
│  │  → Final Bloom 360° petal cross-slash         │  │
│  │  → fireIntensity = 0.06 (softer than Valor)  │  │
│  │  → Sakura pink → petal white gradient         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Final Bloom only)  │  │
│  │  → Upward flourish + downward finisher        │  │
│  │  → 360° petal burst cinematic                 │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on foundations):            │
│  → SakuraSwingTrail.fx (SakuraTrailFlow/Glow)       │
│  → PetalDissolve.fx (spectral copy dissolve)         │
│  UNIQUE SYSTEMS:                                    │
│  → Sakura Meditation (charge = 2x damage next hit)  │
│  → SakurasBlossomSpectral (8 homing phantoms)        │
│  → BloomRingParticle meditation aura                 │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics (3-Phase Petal Dance)
| Phase | Name | Duration | Action | Foundation |
|-------|------|----------|--------|-----------|
| 0 | First Petal | 18 ticks | Graceful horizontal sweep | SwordSmear + Ribbon |
| 1 | Scattered Petals | 22 ticks | Cross-slash + 8 homing phantoms | SwordSmear + Ribbon + ThinSlash |
| 2 | Final Bloom | 26 ticks | Up-flourish + down-finisher + 360° burst | ALL including XSlash + AttackAnim |

**Sakura Meditation**: Stand still 1.5s near no enemies → charge → next swing: 1.4× arc, 2× damage, 1.15× scale

### VFX Architecture — Foundation-Based

#### Swing Arcs → SwordSmearFoundation
- `distortStrength = 0.04` (lighter than Valor — katana ≠ broadsword)
- `flowSpeed = 0.5` (slower flow = more graceful trailing petals)
- `gradientTex`: Sakura Pink → Petal White → Transparent LUT
- `sin(t * PI/2)` easing on swing motion

#### Trail → RibbonFoundation (Mode 4: Harmonic Wave)
- Standing wave ribbon matching the weapon's meditative resonance
- Pink core → white outer → fade
- `RibbonWidthHead = 18f`, `RibbonWidthTail = 2f` (narrower than Valor = precision)

#### Phantom Blades → SparkleProjectileFoundation
- `SakurasBlossomSpectral` uses SparkleTrailShader for shimmer trail
- Boss-priority homing (acceleration 0.020→0.048 over 1s)
- PetalDissolve.fx overlay for spectral appearance

#### Final Bloom → XSlash + AttackAnimation + ImpactFoundation
1. **AttackAnimationFoundation**: Camera-tracked up-flourish + down-finisher
2. **XSlashFoundation**: `fireIntensity = 0.06` (gentle), sakura pink cross
3. **ImpactFoundation** (RippleShader): `ringCount = 4`, sakura pink rings
4. 360° petal burst (SpawnSakuraPetals from VFXLibrary)

---

## 3. Funeral Prayer (Magic Staff)

### Identity & Musical Soul
The Funeral Prayer is the **hero's requiem** — a magic weapon that channels grief into flame. Funeral pyres launched like prayers into the sky, trailing requiem fire. Ricochet beams that chain between enemies like sorrow spreading through a crowd.

### Lore Line
*"Even heroes kneel before the pyre."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FUNERAL PRAYER — Foundation Architecture           │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → FuneralPrayerBeam body rendering           │  │
│  │  → 4 detail textures, crimson-violet gradient │  │
│  │  → BaseBeamWidth = 40f (mid-range)            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Ricochet beam rendering (5-6 bounces)      │  │
│  │  → MaxBounces = 5, crimson-gold LUT           │  │
│  │  → MaxSegmentLength = 600f                    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Funeral pyre projectile comet trail        │  │
│  │  → EnergySurgeBeam texture, crimson corona    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (funeral ash)                 │  │
│  │  → Rising funeral ash smoke on detonation     │  │
│  │  → Deep crimson core → gray ash exterior      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: pyre detonation shockwave    │  │
│  │  → DamageZone: lingering flame zone           │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on foundations):            │
│  → EroicaFuneralTrail.fx (FuneralFlameFlow/Glow)   │
│  → RequiemBeam.fx (beam body custom)                │
│  → PrayerConvergence.fx (convergence burst)          │
│  UNIQUE SYSTEMS:                                    │
│  → Martyr's Exchange (damage taken → empower pyre)  │
│  → FuneralAshDust (ModDust, rising ash motes)        │
│  → Ricochet chain tracking (RegisterBeamHit)         │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Funeral Pyre Projectile → RibbonFoundation + ExplosionParticles + Smoke
- **Flight**: RibbonFoundation Mode 6 (Energy Surge) comet trail
- **Detonation**: ExplosionParticlesFoundation (`SparkCount = 45`, crimson/funeral ash sparks) + SmokeFoundation (`PuffCount = 20`, deep crimson ash)
- **Impact zone**: ImpactFoundation DamageZoneShader (lingering flame, 120-frame zone)

#### Tracking Beam → LaserFoundation (ConvergenceBeamShader)
- `BaseBeamWidth = 40f`, `AimSpeed = 0.12f`
- 4 detail textures themed with funeral fire patterns
- Crimson→Violet→Ember gradient LUT
- Secondary arc spawns from beam hit-point to nearest other enemy (50% damage)

#### Ricochet Beam → ThinLaserFoundation (ThinBeamShader)
- `MaxBounces = 5`, `BaseBeamWidth = 12f`, `MaxSegmentLength = 600f`
- Boss-priority targeting per bounce
- Crimson→Gold gradient LUT
- Each bounce segment gets progressively dimmer (α decay: 1.0 → 0.6)

#### Hit VFX → ImpactFoundation
- **RippleShader**: `ringCount = 3`, `ringThickness = 0.06`, funeral crimson expansion
- **DamageZoneShader**: `scrollSpeed = 0.25`, `rotationSpeed = 0.15`, lingering burn zone

---

## 4. Triumphant Fractal (Magic Staff)

### Identity & Musical Soul
The triumph of mathematical beauty — fractal bolts that recursively split, sacred geometry that shatters enemies. The intellectual glory of Eroica's third movement — ORDER emerging from chaos.

### Lore Line
*"In every fragment, the whole persists."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  TRIUMPHANT FRACTAL — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Main fractal bolt rendering (5-layer)      │  │
│  │  → SparkleTrailShader + CrystalShimmerShader  │  │
│  │  → Gold core, fractal purple edge             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MagicOrbFoundation (OrbBolt pattern)          │  │
│  │  → Fractal split children (2-gen, 3-way)      │  │
│  │  → RadialNoiseMaskShader rendering            │  │
│  │  → Progressively smaller + dimmer per gen     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Resonance Zone AoE on impact               │  │
│  │  → FBM noise, gold-crimson, pulsing           │  │
│  │  → OrbDrawScale = 0.6f, 120-frame lifetime    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: geometric ring burst         │  │
│  │  → DamageZone: Resonance Zone ground effect   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Triumph burst)  │  │
│  │  → Triumph Accumulator 64-fragment barrage    │  │
│  │  → Massive gold particle detonation           │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on foundations):            │
│  → TriumphantFractalShader.fx (FractalEnergyTrail)  │
│  → SacredGeometry.fx (hexagram burst)                │
│  UNIQUE SYSTEMS:                                    │
│  → Recursive 2-gen, 3-way splitting on impact       │
│  → Triumph Accumulator (10 kills → 64 barrage)      │
│  → Fractal Shield alt-fire (projectile absorption)   │
│  → SeekingCrystalHelper (33% on-hit crystals)        │
│  → Lightning arc generation (every 8 ticks)          │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Main Fractal Bolt → SparkleProjectileFoundation
- **SparkleTrailShader** (VertexStrip): `sparkleSpeed = 4.0`, `sparkleScale = 0.8`, `glitterDensity = 5.0`
- **CrystalShimmerShader** (SpriteBatch): `shimmerSpeed = 3.0`, `flashIntensity = 0.7`
- Colors: FractalGold core, FractalViolet outer
- 5-layer: shader trail → bloom → crystal body → shimmer → fractal energy overlay

#### Fractal Children → MagicOrbFoundation (OrbBolt)
- Generation 0 (parent): Full size, full brightness
- Generation 1: 70% size, 80% brightness, 3-way split at ±30°
- Generation 2: 40% size, 50% brightness, 3-way split at ±45°
- Each generation uses OrbBolt rendering with progressively simpler bloom stacks

#### Resonance Zones → MaskFoundation + ImpactFoundation
- **MaskFoundation** (RadialNoiseMaskShader): FBM noise mode, `OrbDrawScale = 0.6f`, gold-crimson pulse
- **ImpactFoundation** (DamageZoneShader): `circleRadius = 0.45`, gold geometry scroll pattern
- Combined: orb above + zone below = full resonance zone

#### Triumph Barrage → ExplosionParticlesFoundation
- `SparkCount = 64` (matching the barrage count), gold/white
- Radial scatter from player position
- Center flash: SoftGlow + StarFlare + geometry flare

---

## 5. Blossom of the Sakura (Ranged Gun)

### Identity & Musical Soul
A rapid-fire weapon where bullets bloom like cherry blossoms in spring — heat-reactive rounds that glow hotter with intensity, leaving sakura trails. Speed and beauty united.

### Lore Line
*"Every bullet carries a petal. Every petal, a prayer."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  BLOSSOM OF THE SAKURA — Foundation Architecture    │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → Bullet tracer trail rendering              │  │
│  │  → 20-point short trail, thin width           │  │
│  │  → Heat-reactive color (cool pink → hot white)│  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Bullet impact ring on-hit                  │  │
│  │  → Small scale (ringCount = 2)                │  │
│  │  → Sakura pink → white burst                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (Tracer Blossom)  │  │
│  │  → Every 5th shot: marking tracer round       │  │
│  │  → SparkleTrailShader for long shimmer trail  │  │
│  │  → Gold/sakura sparkle                        │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (layered on foundations):            │
│  → TracerTrail.fx (bullet trail)                     │
│  → HeatDistortion.fx (barrel heat shimmer)           │
│  UNIQUE SYSTEMS:                                    │
│  → Heat-reactive homing (hotter = tighter tracking) │
│  → SeekingCrystalHelper (25% on-hit, 3 crystals)    │
│  → Muzzle flash system                              │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Bullet Trail → RibbonFoundation (Mode 3: Basic Trail)
- `TrailLength = 20` (short for bullets), `RibbonWidthHead = 8f`, `RibbonWidthTail = 1f`
- Color interpolation: `Lerp(SakuraPink, WhiteHot, heatProgress)` where heatProgress = `ai[0]`
- Cool bullets: sakura petal particles scatter behind. Hot bullets: ember particles + bright glow

#### Tracer Blossom → SparkleProjectileFoundation
- Every 5th shot renders with full SparkleTrailShader shimmer
- `sparkleSpeed = 5.0`, `glitterDensity = 6.0` (very visible marking round)
- Gold/sakura shimmer, longer lifetime, marks target (10% bonus damage debuff)

#### Bullet Impact → ImpactFoundation (RippleShader)
- `ringCount = 2`, `ringThickness = 0.04` (small, rapid)
- Pink → white fast expand (30-frame lifetime)
- Multiple impacts per second (fast fire rate = rapid small rings)

---

## 6. Piercing Light of the Sakura (Ranged Gun)

### Identity & Musical Soul
The sniper's crescendo — each shot building to the **Culmination**, a piercing round that passes through everything. SakuraLightning explosions spiral at impact like conductors' batons.

### Lore Line
*"The eighth note shatters silence."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  PIERCING LIGHT OF SAKURA — Foundation Architecture │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Main crescendo round (36-frame spritesheet)│  │
│  │  → SparkleTrailShader with lightning shimmer  │  │
│  │  → Gold core, lightning blue edge             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: AoE impact ring              │  │
│  │  → DamageZone: SakuraLightning ground zone    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Culmination)    │  │
│  │  → Culmination round detonation               │  │
│  │  → 40 sparks, gold/lightning blue/white       │  │
│  │  → Spiral burst pattern                       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 1: Pure Bloom)         │  │
│  │  → SakuraLightning spiral trail rendering     │  │
│  │  → 30-point trail, expanding spiral width     │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → CrescendoCharge.fx (charge orbit visual)          │
│  → SakuraLightningTrail.fx (lightning bolt trail)    │
│  UNIQUE SYSTEMS:                                    │
│  → Culmination every 8th shot (pierces infinitely)  │
│  → Hero's Final Light (<20% HP → every 4th shot)    │
│  → SakuraLightning spiral AoE (80×80, 45-tick)      │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Main Projectile → SparkleProjectileFoundation
- 36-frame spritesheet (6×6), animated core
- SparkleTrailShader: `sparkleSpeed = 3.5`, gold shimmer trail
- CrystalShimmerShader on the spritesheet core for facet shimmer
- Culmination rounds: 1.5× scale, brighter trail, lightning arcs per frame

#### SakuraLightning → RibbonFoundation + ImpactFoundation + ExplosionParticles
- **RibbonFoundation** (Mode 1: Pure Bloom): 30-point spiral trail rendering for each lightning arc
- **ImpactFoundation** (RippleShader): `ringCount = 4`, expanding ring at center of explosion
- **ImpactFoundation** (DamageZoneShader): 45-tick spiral lightning zone
- **ExplosionParticlesFoundation**: 16 sparks per SakuraLightning (3 spawned per hit = 48 total sparks)

---

## 7. Finality of the Sakura (Summoner Staff)

### Identity & Musical Soul
The final movement — a summoned spectral sakura spirit (SakuraOfFate) that fights with dark flame and petals. This is the hero's posthumous guardian, fighting on even after death.

### Lore Line
*"The last petal never touches the ground."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FINALITY OF THE SAKURA — Foundation Architecture   │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Summoning circle ritual on cast            │  │
│  │  → Cosmic noise, rotationSpeed = 0.2          │  │
│  │  → Minion dark flame aura rendering           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 5: Ember Drift)        │  │
│  │  → Dark flame projectile trails               │  │
│  │  → Crimson-violet ember texture               │  │
│  │  → SakuraFlameProjectile 12-point trail       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Final Bloom)    │  │
│  │  → Every-15s petal supernova burst            │  │
│  │  → 80 sparks, dark crimson + sakura + gold    │  │
│  │  → Massive AoE detonation                     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (dark flame)                  │  │
│  │  → Dark smoke wisps around minion             │  │
│  │  → Abyssal crimson → gray → transparent       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: minion contact hit ring      │  │
│  │  → DamageZone: sakura shield active area      │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → DarkFlameAura.fx (minion aura)                    │
│  → FateSummonCircle.fx (summoning ritual)            │
│  UNIQUE SYSTEMS:                                    │
│  → SakuraOfFate minion (36-frame animation)          │
│  → 14-position trail for aura rendering              │
│  → Final Bloom timer (every 15s)                     │
│  → Sakura Shield (periodic defense)                  │
│  → Self-Sacrifice (prevent lethal damage)            │
└─────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Summoning Circle → MaskFoundation (RadialNoiseMaskShader)
- `noiseMode = Cosmic`, `rotationSpeed = 0.2`, `scrollSpeed = 0.05`
- `OrbDrawScale = 0.5f` (medium summoning circle)
- Abyssal crimson → emission glow during cast
- Fade in over 30 frames, fade out over 15 frames

#### Minion Aura → MaskFoundation (variant) + SmokeFoundation
- **MaskFoundation**: `OrbDrawScale = 0.15f` (small, tight aura around minion)
- `noiseMode = Nebula`, dark crimson → black gradients
- **SmokeFoundation**: Ambient smoke wisps, `PuffCount = 3` per second (low-key ambient)

#### Dark Flame Projectiles → RibbonFoundation (Mode 5: Ember Drift)
- `SakuraFlameProjectile`: 12-point trail
- `RibbonWidthHead = 12f`, `RibbonWidthTail = 2f`
- Dark crimson-violet ember texture, fast decay (45 ticks)

#### Final Bloom → ExplosionParticlesFoundation
- Every 15 seconds: `SparkCount = 80`, `MaxLifetime = 150`, `DamageRadius = 250f`
- Dark crimson + sakura pink + gold spark colors
- Center flash: massive bloom stack (SoftGlow × 3 scales + StarFlare + LensFlare)
- Screen shake at `Intensity = 6f`

#### Minion Contact → ImpactFoundation
- **RippleShader**: `ringCount = 2`, `ringThickness = 0.05`, quick crimson rings
- **DamageZoneShader**: Sakura Shield zone when active (pink glow, 60-frame duration)

---

## Foundation Coverage Matrix

| Foundation | Celestial Valor | Sakura's Blossom | Funeral Prayer | Triumphant Fractal | Blossom | Piercing Light | Finality |
|-----------|----------------|-----------------|----------------|-------------------|---------|---------------|----------|
| SwordSmearFoundation | ✅ | ✅ | | | | | |
| RibbonFoundation | ✅ M5 | ✅ M4 | ✅ M6 | | ✅ M3 | ✅ M1 | ✅ M5 |
| ThinSlashFoundation | ✅ | ✅ | | | | | |
| XSlashFoundation | ✅ Gloria | ✅ FBloom | | | | | |
| ImpactFoundation | ✅ All3 | ✅ Rip+Slash | ✅ Rip+DZ | ✅ Rip+DZ | ✅ Ripple | ✅ Rip+DZ | ✅ Rip+DZ |
| ExplosionParticles | ✅ Gloria | | | ✅ Triumph | | ✅ Culmin | ✅ FBloom |
| SmokeFoundation | ✅ Gloria | | ✅ | | | | ✅ |
| LaserFoundation | | | ✅ | | | | |
| ThinLaserFoundation | | | ✅ | | | | |
| SparkleProjectile | | | | ✅ | ✅ Tracer | ✅ | |
| MaskFoundation | | | | ✅ | | | ✅ |
| MagicOrbFoundation | | | | ✅ | | | |
| AttackAnimation | ✅ Gloria | ✅ FBloom | | | | | |

### Eroica Lore Consistency
- All lore references heroism, sacrifice, glory, triumph, blaze, sakura, valor
- NEVER moonlight, tides, void, cosmos — those belong to other themes
- Foundation parameters always use scarlet/crimson/gold/sakura gradient LUTs
- Each weapon has UNIQUE identity even within same class (Valor = power, Sakura = grace)
