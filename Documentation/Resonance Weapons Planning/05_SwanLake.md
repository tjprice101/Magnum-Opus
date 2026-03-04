# 🦢 Swan Lake — Resonance Weapons Planning

> *"Grace dying beautifully — pure white, black contrast, prismatic rainbow edges."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Tchaikovsky's Swan Lake — grace in monochrome, dying beauty |
| **Emotional Core** | Elegance, tragedy, ethereal beauty |
| **Color Palette** | Pure white, obsidian black, silver, prismatic rainbow edges |
| **Core Hex** | ObsidianBlack `(20, 20, 30)` → DarkSilver `(80, 80, 100)` → Silver `(180, 185, 200)` → PureWhite `(240, 240, 250)` → PrismaticShimmer `(220, 230, 255)` → RainbowFlash `(255, 255, 255)` |
| **Extended Colors** | SwanBlack `(15, 15, 25)`, IcyBlue `(180, 220, 255)`, BalletPink `(255, 200, 220)`, CurseViolet `(60, 30, 80)`, Pearlescent `(240, 230, 245)`, LakeSurface `(160, 180, 210)` |
| **Lore Color** | `new Color(240, 240, 255)` — Pure White |
| **Lore Keywords** | Grace, feathers, swan, lake, reflection, duality, monochrome, elegance, tragedy, dance |
| **VFX Language** | White/black feathers drifting gracefully, prismatic rainbow shimmer at edges, clean graceful arcs, flowing trails, ballet movements, dual polarity, pearlescent sheen |

### Shared Infrastructure (Already Exists)
| System | Lines | Purpose |
|--------|-------|---------|
| `SwanLakePalette.cs` | 488 | 6 core colors + 17 extended + 7 per-weapon blade palettes + 13 gradient helpers + item bloom |
| `SwanLakeVFXLibrary.cs` | 742 | All bloom stacks, trail widths, music notes, dust, feather VFX, prismatic, halos, impact composites, lighting |
| `SwanLakeShaderManager.cs` | 636 | 14 shader keys (12 per-weapon + 3 legacy), 5 noise textures, 12 preset methods, fallback chain |
| `SwansMark` debuff | 121 | -10 defense, universal (all 6 weapons) |
| `MournfulGaze` debuff | 110 | -15% movement speed (Destruction Halo) |
| `FlameOfTheSwan` debuff | 295 | +10% vulnerability + 0.1% HP DoT (Pearlescent Rockets) |

---

## Foundation Weapons Integration Map

Swan Lake weapons are the mod's most elegant — clean monochrome aesthetics with sudden prismatic reveals. Foundations provide **mesh construction, blend management, and rendering scaffolding** while per-weapon shaders handle the unique white-black-prismatic visual identity.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Call of the Black Swan | Swing arc smear — SmearDistortShader with dual-polarity (black/white) LUT |
| **RibbonFoundation** | All 6 weapons | Trail strips — Mode 3 (Basic) for bullets/arrows, Mode 6 (Energy Surge) for bolts/rockets, Mode 1 (Pure Bloom) for minion movement |
| **ImpactFoundation** | All 6 weapons | Hit VFX — RippleShader for swan mark rings, DamageZoneShader for splash/mystery zones |
| **MaskFoundation** | Pearlescent Lake (splash zone), Iridescent Flock (formation aura) | RadialNoiseMaskShader for persistent AoE zones and crystal formation rings |
| **ExplosionParticlesFoundation** | Black Swan, Pearlescent Lake, Chromatic Swan Song, Swan's Lament | Feather/spark bursts on major impacts — white/prismatic sparks |
| **SparkleProjectileFoundation** | Iridescent Wingspan, Iridescent Flock, Chromatic Swan Song | SparkleTrailShader for bolt shimmer trails, crystal shard trails |
| **MagicOrbFoundation** | Chromatic Swan Song (Aria Detonation), Iridescent Wingspan (convergence) | OrbBolt rendering pattern for expanding ring detonations |
| **LaserFoundation** | Iridescent Wingspan (convergence laser bolts) | ConvergenceBeamShader for prismatic convergence laser burst |
| **SmokeFoundation** | Swan's Lament (grief smoke), Black Swan (monochromatic smoke) | Smoke particle rendering for dark atmospheric wisps |
| **AttackAnimationFoundation** | Black Swan (Grand Jeté slam) | Cinematic slam sequence with screen effects |

---

## Weapons Overview

| # | Weapon | Class | Damage | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Call of the Black Swan | Melee (Exoblade) | 400 | 3-phase ballet combo + Grace/Dark Mirror duality |
| 2 | Call of the Pearlescent Lake | Ranged (Gun) | 380 | Rapid-fire + Tidal rockets + Still Waters + Splash Zones |
| 3 | Chromatic Swan Song | Magic (Pistol) | 290 | Chromatic scale cycling + Aria detonations + Opus finale |
| 4 | Feather of the Iridescent Flock | Summon | 260 | V-formation crystals + synchronized dive + oil-sheen iridescence |
| 5 | Iridescent Wingspan | Magic (Staff) | 420 | 5-bolt fans + cursor convergence → Prismatic Burst |
| 6 | The Swan's Lament | Ranged (Shotgun) | 180 | Grief flash shotgun + Lamentation stacks + Destruction Halo |

---

## 1. Call of the Black Swan (Melee — Exoblade-Style)

### Identity & Musical Soul
The black swan — duality incarnate. An Exoblade-style greatsword whose visuals shift between black (Dark Mirror) and white (Grace) depending on whether the player takes damage while swinging. Each combo phase is a ballet movement: Entrechat, Fouetté, Grand Jeté.

### Lore Line
*"Every swan carries both shadow and light. Which one dances depends on whether you flinch."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CALL OF THE BLACK SWAN — Foundation Architecture   │
├─────────────────────────────────────────────────────┤
│  BASE: Exoblade-style held greatsword               │
│  (40-point trail arc, CurveSegment animation)       │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Dual-polarity swing smear                  │  │
│  │  → Grace: ShadowCore→Silver→PureWhite LUT    │  │
│  │  → Dark Mirror: ObsidianBlack→DarkSilver LUT │  │
│  │  → distortStrength: 0.04→0.06→0.10 by phase  │  │
│  │  → Phase 2 (Grand Jeté): overhead slam curve  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → 40-point trail arc rendering               │  │
│  │  → Polarity-dependent color (dark/light)      │  │
│  │  → 2-pass: main trail + glow overlay (×0.55)  │  │
│  │  → BlackSwanSlash shader: Voronoi noise       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (flare projs)     │  │
│  │  → BlackSwanFlareProj homing sub-projectiles  │  │
│  │  → SparkleTrailShader: polarity-colored       │  │
│  │  → Dual polarity (random B/W per instance)    │  │
│  │  → P0: 3 fan, P1: 5/8 radial, P2: shockwave  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + SlashMark)         │  │
│  │  → RippleShader: Grace/polarity impact rings  │  │
│  │  → SlashMarkShader: phase hit marks           │  │
│  │  → Phase 2 crit → full MeleeImpact() composite│  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Grand Jeté)     │  │
│  │  → Phase 2: 12 radial DualitySparks          │  │
│  │  → 5 feather rain from above (×0.3 dmg)      │  │
│  │  → 4 MonochromaticSmoke clouds               │  │
│  │  → Max Grace: Prismatic Swan rainbow burst    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (monochromatic smoke)         │  │
│  │  → MonochromaticSmokeParticle from Phase 2   │  │
│  │  → Expands 20% then shrinks, slow drift       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Grand Jeté)        │  │
│  │  → Phase 2 overhead slam: windup→fast slam    │  │
│  │  → CurveSegment: SineIn→ExpOut→bounce         │  │
│  │  → Player upward velocity boost               │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (3 keys, 2 .fx):                   │
│  → DualPolaritySwing.fx (BlackSwanSlash P0)         │
│  → SwanFlareTrail.fx (BlackSwanFlareTrail P0)       │
│  → DualPolaritySwing.fx (BlackSwanSwingSprite P0)   │
│  UNIQUE SYSTEMS:                                    │
│  → Grace stacks (0-5): +8% speed per stack          │
│  → Dark Mirror stacks (0-5): +15% dmg, -5% speed   │
│  → Grace→Dark Mirror on hit while swinging          │
│  → Max Grace → Prismatic Swan release (rainbow AoE) │
│  → Legacy Empowerment (3 flare hits → 5s, 8 flares)│
│  → SwansMark on all flare hits                      │
│  4-LAYER BLOOM (blade tip, additive):               │
│  → Outer halo (×1.4, 20%) + Mid ring (×0.8, 35%)   │
│  → Core (×0.35, 55%) + Prismatic star (×0.25, 30%) │
│  3 PARTICLE TYPES:                                  │
│  → FeatherDriftParticle (Alpha, tumbling feather)   │
│  → DualitySparkParticle (Additive, velocity-squish) │
│  → MonochromaticSmokeParticle (Alpha, expanding)    │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics — 3-Phase Ballet Combo
| Phase | Ballet Move | Duration | Blade | DmgMult | Flares Spawned | Foundation |
|-------|-----------|----------|-------|---------|----------------|-----------|
| 0 | Entrechat | 20 | 155px | ×0.85 | 3 fan (×0.4) | SwordSmear + Ribbon |
| 1 | Fouetté | 24 | 160px | ×1.0 | 5 radial (or 8 emp, ×2) | SwordSmear + Ribbon + Sparkle |
| 2 | Grand Jeté | 28 | 175px | ×1.4 | Shockwave + 5 feather rain (×0.3) | ALL foundations |

---

## 2. Call of the Pearlescent Lake (Ranged — Rapid-Fire Gun)

### Identity & Musical Soul
The lake itself — rapid-fire pearl bullets that shimmer like droplets on a still lake. Standing still creates a zone of calm that empowers the rockets. Kills leave persistent splash zones like ripples spreading across water.

### Lore Line
*"The lake remembers every stone that breaks its surface."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CALL OF THE PEARLESCENT LAKE — Foundation Arch.    │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → 3-pass rocket trail rendering              │  │
│  │  → Pass 1: Bloom underlay (×3 width, 20%)    │  │
│  │  → Pass 2: Core trail (×1 width, full color)  │  │
│  │  → Pass 3: Overbright halo (×1.5, 15%)       │  │
│  │  → PearlescentRocketTrail shader              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → SplashZone persistent AoE (5s, 64px×mult) │  │
│  │  → 25% slow on enemies inside                 │  │
│  │  → Pearlescent pulsing, rainbow shimmer edge  │  │
│  │  → 3-layer: wide bloom + inner glow + ring    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: concentric ripple rings      │  │
│  │  → DamageZone: SplashZone rendering           │  │
│  │  → On-kill: 3 staggered RippleRings           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (on-kill)        │  │
│  │  → PearlDropletParticles (8-12 scattered)     │  │
│  │  → LakeMistParticles (5-8 rising)             │  │
│  │  → PrismaticFeatherParticles (Tidal only: 6)  │  │
│  │  → Music notes + feather burst via VFXLibrary │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → PearlescentRocketTrail.fx (opal-shimmer 3-pass)  │
│  → LakeExplosion.fx (concentric water-ripple)        │
│  UNIQUE SYSTEMS:                                    │
│  → Tidal Rocket (every 8th shot): 1.4× dmg, 2× rad │
│  → Still Waters: stand still 1.5s → mild homing     │
│  → SplashZone (on-kill): 5s persistent AoE, 25% slow│
│  → Sine-wave wobble travel (±0.5 amplitude)          │
│  → FlameOfTheSwan debuff (+10% vuln + HP DoT)       │
│  5-LAYER BLOOM (rocket core):                       │
│  → Lake (×0.5, 25%) + Pearl (×0.3, 40%)            │
│  → Core (×0.15, 60%) + Star glimmer (×0.2, 30%)    │
│  → Tidal extra ring (×0.7, 20%, IcyBlue)            │
│  4 PARTICLE TYPES:                                  │
│  → RippleRingParticle (48-seg expanding ring)        │
│  → LakeMistParticle (rising, slowly expanding)       │
│  → PearlDropletParticle (falling, 3-layer bloom)     │
│  → PrismaticFeatherParticle (tumbling rainbow edge)  │
└─────────────────────────────────────────────────────┘
```

---

## 3. Chromatic Swan Song (Magic — Musical Scale Pistol)

### Identity & Musical Soul
The swan's final song — a chromatic scale of seven distinct colors, each cast cycling through C→D→E→F→G→A→B. Completing an octave unlocks the Opus — a devastating seven-ring chromatic detonation. Every hit triggers an Aria explosion.

### Lore Line
*"The last song of the swan contains every color that ever was."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CHROMATIC SWAN SONG — Foundation Architecture      │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation (bolt trails)     │  │
│  │  → ChromaticBoltProj shimmer trail            │  │
│  │  → SparkleTrailShader: per-note spectrum color│  │
│  │  → CrystalShimmerShader: bolt body jewel      │  │
│  │  → 2-pass: main trail + bloom underlay (×2.5) │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MagicOrbFoundation (Aria detonation)          │  │
│  │  → AriaDetonationProj rendering pattern       │  │
│  │  → 3-ring expanding explosion (Inner/Mid/Outer)│  │
│  │  → Normal: 120px, Harmonic: 200px, Opus: 300px│  │
│  │  → Opus: 7 overlapping chromatic ring layers  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: per-ring expansion           │  │
│  │  → Inner (white) → Mid (scale color) → Outer  │  │
│  │  → Scale with Harmonic/Opus mode multiplier   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Aria bursts)    │  │
│  │  → AriaBurstParticle (64-seg expanding rings) │  │
│  │  → PrismaticShardParticle (falling, spinning) │  │
│  │  → HarmonicNoteParticle (floating upward)     │  │
│  │  → ChromaticSparkParticle (fast decay)        │  │
│  │  → Scale: Normal→Harmonic→Opus particle count │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → Bolt tracer, per-note rainbow color        │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → ChromaticTrail.fx (rainbow-shifting bolt trail)   │
│  → AriaExplosion.fx (chromatic aria detonation)      │
│  UNIQUE SYSTEMS:                                    │
│  → Chromatic Scale (C-D-E-F-G-A-B): hue per note   │
│  → Opus Detonation: 7 casts → 3× dmg, 7-ring 300px │
│  → Harmonic Stack: 5 different targets → enhanced   │
│  → Dying Breath (<30% HP): 2× speed, +50% radius   │
│  → Every bolt hit → AriaDetonation (always explodes)│
│  4-LAYER BLOOM (bolt core):                         │
│  → Scale color (×0.4, 25%) + Shifted hue (×0.25, 35%)│
│  → Core (×0.12, 55%) + Rainbow star (×0.18, 25%)    │
│  4 PARTICLE TYPES:                                  │
│  → ChromaticSparkParticle (Additive, spectrum shift) │
│  → HarmonicNoteParticle (Alpha, floating notes)     │
│  → AriaBurstParticle (Additive, 64-seg rings)       │
│  → PrismaticShardParticle (Additive, falling shards)│
└─────────────────────────────────────────────────────┘
```

### Chromatic Scale → Aria → Opus Pipeline
| Scale Pos | Note | Hue | Bolt Color | Aria Mode | Particles |
|-----------|------|-----|-----------|-----------|-----------|
| 0 | C | 0/7 | Red | Normal (×0.5, 120px) | 3 ring + 12 shard + 8 note + 20 spark |
| 1 | D | 1/7 | Orange | Normal | Same |
| 2 | E | 2/7 | Yellow | Normal | Same |
| 3 | F | 3/7 | Green | If 3 consec hits: Harmonic (×2, 200px) | 3 ring + 24 shard + 14 note + 30 spark |
| 4 | G | 4/7 | Cyan | Normal/Harmonic | Varies |
| 5 | A | 5/7 | Blue | Normal/Harmonic | Varies |
| 6 | B | 6/7 | Purple | Normal/Harmonic | Varies |
| OPUS | All | Full | Rainbow | Opus (×3, 300px, 7 rings) | 7 ring + 36 shard + 14 note + 30 spark |

---

## 4. Feather of the Iridescent Flock (Summon)

### Identity & Musical Soul
The flock — minion crystals that fly in V-formation behind the player like swans in flight. Each crystal has an oil-sheen iridescent shimmer. They coordinate dive attacks and connect with formation lines when enough gather.

### Lore Line
*"One feather drifts. A flock transforms the sky."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FEATHER OF THE IRIDESCENT FLOCK — Foundation Arch. │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation (crystal shards)  │  │
│  │  → CrystalShardProj shimmer trail             │  │
│  │  → SparkleTrailShader: iridescent oil-sheen   │  │
│  │  → 8×8px, pen=1, mild homing (0.04, 500u)    │  │
│  │  → 3 per volley, 8° spread, 8-tick delay      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → FlockAura when 3+ crystals active          │  │
│  │  → Iridescent color cycling per frame         │  │
│  │  → Formation ring connecting adjacent crystals │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 1: Pure Bloom)         │  │
│  │  → Crystal dive trail during attack phase     │  │
│  │  → CrystalOrbitTrail shader, oil-sheen color  │  │
│  │  → No trail during passive V-formation        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: dive impact rings            │  │
│  │  → 5 CrystalShardParticles + 3 IridescentFeathers│
│  │  → Music notes + prismatic sparkles           │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → CrystalOrbitTrail.fx (oil-sheen dive trail)       │
│  → FlockAura.fx (formation aura when 3+ crystals)   │
│  UNIQUE SYSTEMS:                                    │
│  → V-Formation: even=left arm, odd=right arm        │
│  → 4-phase cycle: Formation(2s)→Volley→Dive→Return │
│  → Synchronized Dive (3+): FindUnclaimedNPC() spread│
│  → +5% damage per swan beyond first                 │
│  → Crystal Resonance: 4+ crystals → OilShimmer      │
│  → Formation lines between adjacent crystals         │
│  → 0.34 minion slots per crystal                     │
│  4-LAYER BLOOM (crystal core):                      │
│  → Iridescent halo (×0.35, 25%) + White spark core  │
│  → Prismatic star (×0.15, 30%) + Dive bloom (×0.5)  │
│  4 PARTICLE TYPES:                                  │
│  → IridescentFeatherParticle (Alpha, oil-sheen sway)│
│  → CrystalShardParticle (Additive, fast-fall spin)  │
│  → FormationGlowParticle (Additive, 48-seg ring)    │
│  → OilShimmerParticle (Additive, drifting glow mote)│
└─────────────────────────────────────────────────────┘
```

### Attack Cycle Per Crystal
| Phase | Duration | Behavior | Foundation |
|-------|----------|----------|-----------|
| Formation Flight | 120 ticks (2s) | V-formation drift, bobbing | RibbonFoundation (no trail) |
| Shard Volley | ~24 ticks | 3 CrystalShardProj, 8° spread | SparkleProjectile |
| Dive Attack | ≤50 ticks | Charge through target at speed 20 | Ribbon (trail active) + Impact |
| Return | Variable | Fly back to V-formation position | Ribbon (trail fading) |

---

## 5. Iridescent Wingspan (Magic — Convergence Staff)

### Identity & Musical Soul
The wingspan — five bolts fired in a fan that curve toward the cursor. When all five converge, a prismatic burst erupts. The feeling of wings spreading wide then folding inward with devastating precision.

### Lore Line
*"Each feather seeks its place. Together, they become flight."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  IRIDESCENT WINGSPAN — Foundation Architecture      │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation (wing bolts)      │  │
│  │  → WingspanBoltProj shimmer trail             │  │
│  │  → SparkleTrailShader: per-bolt hue offset    │  │
│  │  → 5 bolts: -24°, -10°, 0°, +10°, +24° fan   │  │
│  │  → Each bolt has unique hue (index/5)          │  │
│  │  → 2-pass: main + bloom underlay (×2.5, 15%)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ LaserFoundation (convergence laser burst)     │  │
│  │  → Prismatic Convergence: all 5 bolts meet    │  │
│  │  → 8 rainbow laser bolts in cardinal dirs     │  │
│  │  → ConvergenceBeamShader: 2.5× dmg per laser  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: bolt hit rings               │  │
│  │  → Convergence burst ring (scaled up)         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (convergence)    │  │
│  │  → WingBurstParticle (expanding wing flash)   │  │
│  │  → 10 prismatic sparkles + 5 music notes      │  │
│  │  → 5 EtherealFeathers drifting upward         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Wing bolt trails, spectral blue/prismatic  │  │
│  │  → WingspanFlareTrail shader                 │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → EtherealWing.fx (ethereal wing display)           │
│  → WingspanFlareTrail.fx (spectral bolt trail)       │
│  UNIQUE SYSTEMS:                                    │
│  → Ethereal Flight Charge: +8/hit, max 100          │
│  → At 100 charge: 1 empowered bolt (3×, pen=5)     │
│  → Convergence: bolts curve to cursor after 30 ticks│
│  → All 5 meet → Prismatic Convergence (8 lasers)    │
│  → Wingspan Resonance: 60-tick +10% dmg after conv  │
│  → HoldItem: ethereal wing particle display          │
│  4-LAYER BLOOM (+1 empowered):                      │
│  → Ethereal halo (×1.8, 25%) + Mid glow (×1.0, 40%)│
│  → Core (×0.4, 60%) + Star (×0.35, 30%)             │
│  → Empowered: Golden aura (×2.5, 20%, WingGold)     │
│  4 PARTICLE TYPES:                                  │
│  → EtherealFeatherParticle (Additive, rising sway)  │
│  → WingSparkParticle (Additive, fast decay)          │
│  → WingBurstParticle (Additive, expanding flash)    │
│  → PrismaticMoteParticle (Additive, drifting glow)  │
└─────────────────────────────────────────────────────┘
```

### Convergence Pipeline
| Stage | Trigger | Result | Foundation |
|-------|---------|--------|-----------|
| Fan Fire | Use item | 5 bolts in 58° spread fan | SparkleProjectile |
| Flight | First 30 ticks | Straight line travel | Ribbon (trail) |
| Convergence | After 30 ticks | Bolts curve toward cursor (0.02-0.08 strength) | SparkleProjectile |
| Arrival | Bolt within 20px of cursor | Registers convergence on WingspanPlayer | — |
| Prismatic Burst | All 5 converge | 8 cardinal laser bolts (2.5× each) + massive VFX | Laser + Explosion + Impact |
| Resonance Window | 60 ticks post-burst | +10% damage bonus on next fan | — |

---

## 6. The Swan's Lament (Ranged — Grief Shotgun)

### Identity & Musical Soul
The lament — mourning expressed as violence. A dark shotgun that fires salvos of grief, building Lamentation stacks on enemies. Most of the time it's dark and monochrome, but sudden "revelation" flashes of prismatic color break through — like beauty piercing through sorrow.

### Lore Line
*"Grief is not darkness. It is the memory of light."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE SWAN'S LAMENT — Foundation Architecture        │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → LamentBulletProj tracer trail (18-point)   │  │
│  │  → Width: 8→1px with ×(1-p*0.5) taper        │  │
│  │  → CatharsisWhite→grey, GriefFlash gold flicker│
│  │  → LamentBulletTrail shader                   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → DestructionHaloProj ring rendering         │  │
│  │  → Expanding: 20→180px, EaseOutQuart          │  │
│  │  → Ring-shaped collision (30px band at edge)  │  │
│  │  → 2-layer: bloom + ring + inner hollow       │  │
│  │  → DestructionRevelation shader               │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: bullet/halo hit rings        │  │
│  │  → 3 feather shrapnel (cone, mixed B/W)       │  │
│  │  → Applies MournfulGaze debuff (Halo hits)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (grief smoke)                 │  │
│  │  → GriefSmoke trailing behind bullets         │  │
│  │  → Heavy, slow, squared opacity falloff       │  │
│  │  → NoiseSmoke texture                         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Finale Lament)  │  │
│  │  → 16 WhiteTorch nova on Halo kill            │  │
│  │  → Prismatic sparkles + music notes           │  │
│  │  → 12 PrismaticFlash at MaxRadius             │  │
│  │  → 8 GriefSmoke at cardinal positions         │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS:                                    │
│  → LamentBulletTrail.fx (grief-flash bullet streak)  │
│  → DestructionRevelation.fx (destruction halo ring)  │
│  UNIQUE SYSTEMS:                                    │
│  → 10-16 bullets per shot in 22° spread cone        │
│  → Destruction Halo (every 6th): 1.5× dmg, ring AoE │
│  → Lamentation Stacks (per-NPC, max 5): +10% vuln  │
│  → Max stacks → enemy "weeps" (teardrop particles)  │
│  → Lament's Echo: kills boost fire rate (1.0→0.5×)  │
│  → Echo widens spread (1.0→1.8×)                    │
│  → Finale Lament: Halo kill → 5 stacks to all 200px │
│  → GriefFlash: pow(sin(t*6π), 8) → narrow peaks     │
│  4-LAYER BLOOM (bullet core, +flash):               │
│  → Grief halo (×0.35, 25%) + Revelation (×0.2, 40%)│
│  → Core (×0.12, 65%) + Flash star (if flash>0.4)    │
│  4 PARTICLE TYPES:                                  │
│  → GriefSmoke (Alpha, heavy slow, NoiseSmoke tex)   │
│  → PrismaticFlashParticle (Additive, 6-10f burst)   │
│  → DestructionRingParticle (Additive, expanding ring)│
│  → LamentEmberParticle (Additive, B/W flicker)      │
└─────────────────────────────────────────────────────┘
```

### Lamentation → Finale Pipeline
| Stage | Trigger | Effect | Foundation |
|-------|---------|--------|-----------|
| Stack Building | Any Lament projectile hit | +1 Lamentation Stack (per-NPC, max 5) | — |
| Weeping | 5 stacks on single NPC | +10% incoming damage, teardrop particles | ImpactFoundation |
| Destruction Halo | Every 6th shot | Expanding ring, MournfulGaze debuff | MaskFoundation |
| Finale Lament | Halo kills weeping enemy | ALL enemies within 200px → 5 stacks instantly | ExplosionParticles |
| Echo Cascade | Any kill during Echo window | Fire rate boost (×0.5) + spread widen (×1.8) | — |

---

## Foundation Coverage Matrix

| Foundation | BlackSwan | PearlLake | ChromSong | IridFlock | IridWing | Lament |
|-----------|----------|----------|----------|----------|---------|--------|
| SwordSmearFoundation | ✅ | | | | | |
| RibbonFoundation | ✅ M6 | ✅ M6 | ✅ M3 | ✅ M1 | ✅ M6 | ✅ M3 |
| ImpactFoundation | ✅ Rip+SM | ✅ Rip+DZ | ✅ Ripple | ✅ Ripple | ✅ Ripple | ✅ Ripple |
| MaskFoundation | | ✅ Splash | | ✅ Aura | | ✅ Halo |
| ExplosionParticles | ✅ GrandJ | ✅ OnKill | ✅ Aria | | ✅ Converg | ✅ Finale |
| SparkleProjectile | ✅ Flares | | ✅ Bolts | ✅ Shards | ✅ WingBolt | |
| MagicOrbFoundation | | | ✅ Aria | | | |
| LaserFoundation | | | | | ✅ Converg | |
| SmokeFoundation | ✅ Mono | | | | | ✅ Grief |
| AttackAnimation | ✅ GrandJ | | | | | |

### Swan Lake Lore Consistency
- All lore references grace, elegance, feathers, swans, lakes, reflections, dance, duality, beauty, tragedy
- NEVER fire, cosmos, void, mystery, bells — those belong to other themes
- Foundation parameters always use monochrome (black↔white) gradient LUTs with prismatic rainbow accents at edges
- SwansMark is the UNIVERSAL debuff — all 6 weapons apply it
- Each weapon has exactly 2 dedicated .fx shaders + 4 custom particle types (consistent architecture)
- Bloom stacks are universally 4-layer (SoftRadialBloom outer → SoftRadialBloom mid → PointBloom core → Star accent)
