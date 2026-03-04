# 🌙 Moonlight Sonata — Resonance Weapons Planning

> *"The moon's quiet sorrow, played in silver and shadow."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Piano Sonata No. 14 — the moon's quiet sorrow |
| **Emotional Core** | Melancholy, peace, mystical stillness |
| **Color Palette** | Deep dark purples, vibrant light blues, violet, ice blue |
| **Palette Hex** | Deep Resonance `(90, 50, 160)` → Frequency Pulse `(170, 140, 255)` → Resonant Silver `(230, 235, 255)` → Ice Blue `(135, 206, 250)` → Crystal Edge `(220, 230, 255)` → Harmonic White `(235, 240, 255)` |
| **Lore Color** | `new Color(140, 100, 200)` — Purple |
| **Lore Keywords** | Moonlight, tides, silver, stillness, sorrow — NEVER cosmos, stars, or space |
| **VFX Language** | Soft purple mist, silver moonbeams, gentle flowing arcs, constellation sparkles, standing-wave resonance, lunar phases |

---

## Foundation Weapons Integration Map

Every Moonlight Sonata weapon MUST build its VFX systems on top of existing Foundation Weapons. Below is the master mapping of which Foundations each weapon uses as scaffolding. **No weapon should implement rendering from scratch** — it extends and re-themes Foundation systems.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Incisor of Moonlight, Eternal Moon | Swing arc smear overlays with SmearDistortShader — re-themed to lunar purple/blue gradient LUT |
| **RibbonFoundation** | Incisor of Moonlight, Eternal Moon, Resurrection | Trail rendering (10 modes available) — Harmonic Wave for Incisor, Basic Trail Strip for Eternal Moon, Energy Surge for Resurrection |
| **ThinSlashFoundation** | Incisor of Moonlight, Eternal Moon | Razor-thin slash marks at impact — ThinSlashShader SDF line in ice-blue/silver |
| **XSlashFoundation** | Eternal Moon | X-shaped cross impact on Tidal Detonation — XSlashShader fire distortion re-themed to tidal wave colors |
| **ImpactFoundation** | All 5 weapons | Impact VFX on hit — RippleShader for expanding rings, DamageZoneShader for lingering areas |
| **ExplosionParticlesFoundation** | Resurrection of the Moon | Supernova shell detonation — SparkCarrier + SparkExplosion with RadialScatter mode |
| **SmokeFoundation** | Resurrection of the Moon | Smoke ring for supernova detonations — SmokeRingProjectile with lunar-themed 3×6 spritesheet |
| **SparkleProjectileFoundation** | Resurrection of the Moon | Comet core projectiles — SparkleCrystal with SparkleTrailShader + CrystalShimmerShader in lunar palette |
| **LaserFoundation** | Moonlight's Calling, Staff of Lunar Phases | Main serenade beam / standard Goliath beam — ConvergenceBeamShader with 4 detail textures |
| **ThinLaserFoundation** | Moonlight's Calling | Spectral child beams — ThinBeamShader with ricochet disabled |
| **InfernalBeamFoundation** | Staff of the Lunar Phases | Goliath devastating beam — InfernalBeamBodyShader re-themed with lunar textures |
| **MaskFoundation** | Moonlight's Calling, Staff of Lunar Phases | Prismatic detonation orb, Goliath aura — RadialNoiseMaskShader |
| **MagicOrbFoundation** | Moonlight's Calling, Staff of Lunar Phases | Harmonic node orbs, New Moon bolts — RadialNoiseMaskShader + OrbBolt pattern |
| **AttackAnimationFoundation** | Incisor of Moonlight (Grand Finale) | Cinematic 360° spin finale — camera pan, multi-slash, screen effects |

---

## Weapons Overview

| # | Weapon | Class | Status | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Incisor of Moonlight | Melee | ✅ Implemented | Three Movements of Moonlight (Adagio→Allegretto→Presto→Finale) |
| 2 | Eternal Moon | Melee | 🔧 Needs VFX Rework | Tidal wave swings, ghost projections, tidal detonations |
| 3 | Resurrection of the Moon | Ranged | 🔧 Needs VFX Rework | Supernova shells, comet core projectiles |
| 4 | Moonlight's Calling | Magic | 🔧 Needs VFX Rework | Channeled serenade beam, spectral child beams, prismatic detonation |
| 5 | Staff of the Lunar Phases | Summon | 🔧 Needs VFX Rework | Goliath of Moonlight minion, devastating charged beams |

---

## 1. Incisor of Moonlight (Melee) — ✅ IMPLEMENTED

### Identity & Musical Soul
The Incisor is the opening movement of Moonlight Sonata made physical — a blade that plays the famous three movements as combat phases. Movement I is the iconic rolling triplets (Adagio Sostenuto), Movement II is the deceptively light Allegretto, Movement III is the furious Presto Agitato, and the Grand Finale is a requiem strike that brings all movements together.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  INCISOR OF MOONLIGHT — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: MeleeSwingItemBase + MeleeSwingBase    │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Swing arc smear per-movement               │  │
│  │  → Purple/blue gradient LUT per combo phase   │  │
│  │  → 3-layer distortion (outer/main/core)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 4: Harmonic Wave)      │  │
│  │  → 40-point position ring buffer trail        │  │
│  │  → Standing wave ribbon strip texture         │  │
│  │  → Per-movement color variation               │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Sub-projectile impact slash marks          │  │
│  │  → Ice Cyan / Violet Cut style selection      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Expanding concentric ring ripple on-hit    │  │
│  │  → Deep purple → ice blue → white color ramp  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Grand Finale only) │  │
│  │  → 360° cinematic spin sequence               │  │
│  │  → Camera pan + multi-slash + screen effects  │  │
│  │  → Bloom stacking (SoftGlow, StarFlare, etc)  │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS (on top of foundations):           │
│  → IncisorPrimitiveRenderer (GPU trail, existing)   │
│  → IncisorParticleHandler (constellation sparks)     │
│  → Per-movement moon phase indicator (🌑→🌒→🌕→🌟)  │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Movement I — Adagio Sostenuto** (1 swing): Slow, heavy overhead arc. Fires 3 CrescentMoonProj in rolling triplets.
- **Movement II — Allegretto** (2 swings): Fast dual slash combo. 2nd slash fires 5 StaccatoNoteProj in a fan. Bouncing notes detonate if 3+ hit the same enemy.
- **Movement III — Presto Agitato** (5 swings): Rapid flurry. Each slash fires LunarBeamProj + OrbitingNoteProj. 5th slash creates CrescentWaveProj shockwave.
- **Grand Finale — Requiem Strike** (1 swing): 360° spin via AttackAnimationFoundation. 12 radial CrescentMoonProj. All OrbitingNoteProj converge. Screen flash.
- **Passive — Lunar Resonance**: Standing still for 2 seconds grants +8% damage on next swing, with visual charging particles.

### VFX Architecture — Foundation-Based

#### Swing Arc Rendering → SwordSmearFoundation
Extends `SmearSwingProjectile` pattern with **SmearDistortShader**:
- `noiseTex`: Perlin noise from `Assets/VFX/Noise/`
- `gradientTex`: Custom lunar gradient LUT (deep purple → ice blue → white)
- `distortStrength`: 0.05 (main), 0.08 (outer), 0.025 (core)
- `flowSpeed`: Varies per movement (0.3 Adagio → 0.8 Presto)
- 3-layer rendering: outer glow → main smear → bright core

#### Trail Rendering → RibbonFoundation (Mode 4: Harmonic Wave)
- 40-point ring buffer, `RibbonWidthHead = 20f`, `RibbonWidthTail = 2f`
- Standing wave ribbon strip texture UV-mapped along position history
- Colors per movement: cold blue (I), silver-white (II), deep purple (III), brilliant white (Finale)

#### Impact VFX → ImpactFoundation (RippleShader) + ThinSlashFoundation
- **RippleShader**: `ringCount = 3`, `ringThickness = 0.06`, noise-distorted edges
- **ThinSlashShader**: `lineWidth = 0.018`, `lineLength = 0.45`, Ice Cyan/Violet Cut styles
- 3-layer directional bloom along slash direction

#### Grand Finale → AttackAnimationFoundation
- 4-phase: camera pan → 12 radial slashes → OrbitingNote convergence → camera return
- Bloom stacking: SoftGlow, StarFlare, GlowOrb, LensFlare in lunar palette
- Screen effects: B&W brightness shift during slash phase

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| CrescentMoonProj | **RibbonFoundation** (Mode 1: Pure Bloom) | Bloom trail, pierce once |
| StaccatoNoteProj | **SparkleProjectileFoundation** (shimmer trail) | Bouncing, gravity-affected |
| OrbitingNoteProj | **MagicOrbFoundation** (orb rendering) | Orbits player, then homes |
| CrescentWaveProj | **ImpactFoundation** (RippleShader) | Expanding ring |
| LunarBeamProj | **ThinLaserFoundation** (ThinBeamShader) | Fast, piercing |
| LunarNova | **ExplosionParticlesFoundation** (RadialScatter) | Staccato convergence burst |

---

## 2. Eternal Moon (Melee) — VFX Rework Required

### Identity & Musical Soul
The Eternal Moon embodies the **eternal recurrence of the moon itself** — tidal forces, the gravitational pull, the inescapable cycle. This is a greatsword of overwhelming lunar weight. If the Incisor dances, the Eternal Moon **drowns**.

### Lore Line
*"The tide remembers what the shore forgets."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  ETERNAL MOON — Foundation Architecture             │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: MeleeSwingItemBase + MeleeSwingBase    │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Tidal wave smear overlay per swing         │  │
│  │  → distortStrength scaled by Tidal Phase      │  │
│  │  → flowSpeed: 0.4 base → 0.8 at Tsunami      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → 40-point trail with tidal wave texture     │  │
│  │  → Width scales with tide phase multiplier    │  │
│  │  → UV-scrolled at 1.5x swing speed            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (XSlashShader)               │  │
│  │  → Tidal Detonation cross-impact effect       │  │
│  │  → Re-themed: purple → blue → white foam      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZone + Ripple)        │  │
│  │  → DamageZone: Gravitational Pull zone        │  │
│  │  → Ripple: Tidal wave shockwave rings         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader)         │  │
│  │  → Impact slash marks on heavy hits           │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE: Ghost Projection system, Tidal Phase Meter │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Tidal Wave Swings**: Each swing creates expanding wave projections via **ImpactFoundation** RippleShader
- **Ghost Projections**: Phantom blades that mimic the player's swing 0.3s later (custom system)
- **Crescent Slashes**: Thrown crescent projectiles with **RibbonFoundation** Mode 1 bloom trails
- **Tidal Detonation**: Massive AoE via **XSlashFoundation** when crescent impacts overlap
- **Tidal Phase Meter**: Low Tide → Flood → High Tide → Tsunami. Scales all Foundation parameters.
- **Gravitational Pull**: **ImpactFoundation** DamageZoneShader creates persistent pull zone on heavy hits

### VFX Architecture — Foundation-Based

#### Swing Arcs → SwordSmearFoundation
- **SmearDistortShader**: `distortStrength = 0.08` base (heavier than Incisor's 0.05), scales to 0.12 at Tsunami
- `FlowSpeed`: 0.4 → 0.8 by tide phase
- `BladeLength = 100f`, `SwingArcDeg = 170°` for heavy greatsword feel
- Gradient LUT: deep purple → ice blue → white foam tip

#### Trail → RibbonFoundation (Mode 3: Basic Trail Strip)
- `TrailLength = 40`, `RibbonWidthHead = 28f`, `RibbonWidthTail = 4f`
- Width function: `sin(progress * PI) * baseWidth * tidePhaseMultiplier`
- UV.x scrolls at 1.5x swing speed
- Tidal wave texture replaces default BasicTrail.png

#### Shockwave Rings → ImpactFoundation (RippleShader)
- `ringCount = 4`, `ringThickness = 0.08`
- EaseOutQuad expansion, scales with Tidal Phase level
- Deep purple → ice blue → white → transparent

#### Gravity Zone → ImpactFoundation (DamageZoneShader)
- `scrollSpeed = 0.2`, `rotationSpeed = 0.1`
- `circleRadius = 0.4`, `edgeSoftness = 0.1`
- Purple with silver streaks, enemies pulled to center

#### Tidal Detonation → XSlashFoundation (XSlashShader)
- `fireIntensity = 0.08`, `scrollSpeed = 0.4` (water-like UV flow)
- Gradient LUT re-themed: deep purple → ice blue → white
- `ShaderDrawScale = 0.28f` for massive detonation
- 5-layer render: bloom → blazing X → arm bloom → center flash → spray

### Asset Requirements
| Asset | Path | Prompt |
|-------|------|--------|
| Tidal wave trail | `Assets/MoonlightSonata/EternalMoon/Trails/TidalWave.png` | "Horizontal flowing water wave texture, deep blue to white gradient, stylized anime water with foam crests, on solid black background, 512x64px seamless tiling --ar 8:1 --style raw" |
| Ghost glow | `Assets/MoonlightSonata/EternalMoon/Trails/GhostGlow.png` | "Soft ethereal ghost trail, translucent blue-silver wisps flowing horizontally, on solid black background, 256x64px, seamless edges --ar 4:1 --style raw" |
| Gravity well mask | `Assets/MoonlightSonata/EternalMoon/Orbs/GravityWell.png` | "Concentric gravitational distortion rings, dark purple center fading to transparent edge, circular mask, on solid black background, 256x256px --ar 1:1 --style raw" |
| Tide phase icons | `Assets/MoonlightSonata/EternalMoon/Pixel/TidePhase.png` | "4-frame pixel art sprite sheet of moon tide phases, low tide to tsunami, soft blue glow, 64x16px total (16x16 per frame), on solid black background --ar 4:1 --style raw" |

---

## 3. Resurrection of the Moon (Ranged) — VFX Rework Required

### Identity & Musical Soul
The Resurrection is the **third movement's fury made ranged** — the explosive rebirth of the moon. Every shot should feel like the moon exploding and reforming. Supernova shells that collapse, comet cores that trail silver light.

### Lore Line
*"What dies in moonlight is reborn in starfire."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  RESURRECTION OF THE MOON — Foundation Architecture │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ ExplosionParticlesFoundation (RadialScatter)  │  │
│  │  → Supernova detonation sparks (55 sparks)    │  │
│  │  → Center flash bloom stacking                │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (lunar-themed)                │  │
│  │  → Supernova smoke ring (30 puffs)            │  │
│  │  → Purple core → blue body → white edge       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Comet Core projectile (5-layer rendering)  │  │
│  │  → SparkleTrailShader + CrystalShimmerShader  │  │
│  │  → Homing, piercing, shimmer trail            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Supernova detonation expanding rings       │  │
│  │  → White center → purple edge shift           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Supernova shell flight trail               │  │
│  │  → EnergySurgeBeam texture corona             │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE: Lunar Cycle Ammo, Eclipse Synergy,         │
│          Moonrise Charge barrel energy               │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Supernova Shells**: Slow projectiles → detonation via **ExplosionParticles** + **Smoke** + **Impact** Foundations
- **Comet Core**: Fast piercing via **SparkleProjectileFoundation** 5-layer rendering
- **Lunar Cycle Ammo**: New Moon (piercing) → Waxing → Full Moon (max AoE) → Waning (homing)
- **Eclipse Synergy**: Shell + Core collision → combined **ExplosionParticles** + **ImpactFoundation** mega-burst

### VFX Architecture — Foundation-Based

#### Supernova Detonation → ExplosionParticlesFoundation + SmokeFoundation + ImpactFoundation
Three Foundations fire simultaneously on shell impact:

1. **ExplosionParticlesFoundation** (RadialScatter mode):
   - `SparkCount = 55`, `MaxLifetime = 90`, `DamageRadius = 100f`
   - SolidWhiteLine + 4PointedStarHard/Soft spark types
   - Colors: lunar purple → ice blue → white
   - Center flash: SoftGlow + StarFlare + LensFlare

2. **SmokeFoundation** (lunar smoke):
   - `PuffCount = 30`, `MaxLifetime = 60`, `RenderScale = 0.3f`
   - Custom 3×6 spritesheet in lunar blues/purples
   - Calamity-style lifecycle: expand → contract → fade

3. **ImpactFoundation** (RippleShader):
   - `ringCount = 5`, `ringThickness = 0.05`
   - White → ice blue → deep purple outward shift

#### Comet Core → SparkleProjectileFoundation
Full 5-layer rendering pipeline:
- **SparkleTrailShader** (VertexStrip): `sparkleSpeed = 3.0`, `sparkleScale = 0.6`, `glitterDensity = 4.0`
- **CrystalShimmerShader** (SpriteBatch): `shimmerSpeed = 2.0`, `flashIntensity = 0.6`
- Colors: Ice Blue core, Resonant Silver outer
- `TrailLength = 24`, `HomingStrength = 0.06f`, `TargetSpeed = 11f`
- Layers: shader trail → bloom trail → bloom halo → crystal body → sparkle accents

#### Shell Flight Trail → RibbonFoundation (Mode 6: Energy Surge)
- EnergySurgeBeam texture as ribbon fill
- 40-point position history
- `RibbonWidthHead = 24f`, `RibbonWidthTail = 3f`
- Corona appearance trailing behind shell

### Asset Requirements
| Asset | Path | Prompt |
|-------|------|--------|
| Supernova corona | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Trails/SupernovaCrown.png` | "Radial explosion corona texture, bright white center fading to deep purple edges, stylized energy burst, on solid black background, 256x256px --ar 1:1 --style raw" |
| Comet tail trail | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Trails/CometTail.png` | "Horizontal comet tail energy trail, bright silver-white head fading to soft blue-purple tail, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Muzzle flash | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Flare/MoonMuzzle.png` | "Stylized muzzle flash flare, silver-blue burst with 6 pointed star rays, on solid black background, 128x128px --ar 1:1 --style raw" |
| Lunar smoke sheet | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Pixel/LunarSmoke.png` | "3x6 grid smoke puff spritesheet, soft blue-purple watercolor smoke, each cell unique, on solid black background, 384x192px --ar 2:1 --style raw" |

---

## 4. Moonlight's Calling (Magic) — VFX Rework Required

### Identity & Musical Soul
A **serenade to the moon** — a channeled magic weapon that sings moonbeams into existence. The beam should feel like moonlight streaming through clouds — soft at first, building to devastating brilliance.

### Lore Line
*"She called to the moon, and the moon wept silver."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  MOONLIGHT'S CALLING — Foundation Architecture      │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → Main serenade beam (4 detail textures)     │  │
│  │  → VertexStrip, UV reps prop. to length       │  │
│  │  → Beam width breathes with channel time      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Spectral child beam rendering              │  │
│  │  → MaxBounces=0, BaseBeamWidth=10f            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Prismatic Detonation on-release burst      │  │
│  │  → Cosmic noise, 3-layer orb rendering        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MagicOrbFoundation (orb + bolt rendering)     │  │
│  │  → Harmonic Node orbs along beam              │  │
│  │  → OrbBolt sub-projectiles from nodes         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZoneShader)           │  │
│  │  → Moonlight Puddle ground zones              │  │
│  │  → 180-frame persistent AoE, slows enemies    │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE: Resonance Building (VFX escalation over    │
│  channel time), Standing wave math on beam UV       │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Serenade Holdout**: Channel to maintain beam
- **Main Beam**: Continuous damage via **LaserFoundation** ConvergenceBeamShader
- **Spectral Child Beams**: After 2s, 2-4 beams via **ThinLaserFoundation**
- **Prismatic Detonation**: On release after 4s+, **MaskFoundation** burst at cursor
- **Harmonic Nodes**: **MagicOrbFoundation** orbs at standing wave positions on beam; 1.5x damage at nodes
- **Moonlight Puddles**: **ImpactFoundation** DamageZone where beam touches ground

### Resonance Building (Visual Escalation)
| Channel Time | VFX Layer | Foundation | Intensity |
|-------------|-----------|-----------|-----------|
| 0–1s | Single thin beam | LaserFoundation at 0.3x width | Pianissimo |
| 1–2s | Beam widens + shimmer | LaserFoundation at 0.6x width | Piano |
| 2–3s | + Child beams + nodes | + ThinLaserFoundation + MagicOrbFoundation | Mezzo-forte |
| 3–4s | + Full beam + ground glow | LaserFoundation at 1.0x + ImpactFoundation | Forte |
| 4s+ | Maximum + screen tint | All systems max + MaskFoundation detonation ready | Fortissimo |

### VFX Architecture — Foundation-Based

#### Main Beam → LaserFoundation (ConvergenceBeamShader)
- 4 detail textures: ThinGlowLine + Spark + Extra (standing wave) + TrailLoop
- Lunar gradient LUT
- `BaseBeamWidth = 100f` × channel time scalar (0.3→1.3)
- `AimSpeed = 0.08f`

#### Child Beams → ThinLaserFoundation (ThinBeamShader)
- `MaxBounces = 0`, `BaseBeamWidth = 10f`, `MaxSegmentLength = 800f`
- Silver gradient LUT, alpha 0.6 (ghostly)

#### Harmonic Nodes → MagicOrbFoundation
- **RadialNoiseMaskShader** orbs at `sin(UV.x * PI * nodeCount)` positions
- `OrbDrawScale = 0.1f`, `DetectionRadius = 200f`
- Nebula noise texture, purple-blue appearance

#### Prismatic Detonation → MaskFoundation
- `MaskOrbProjectile` with Cosmic noise, `OrbDrawScale = 0.8f`, `MaxLifetime = 60`
- 3-layer: bloom halo → shader orb → core bloom
- Scale-up entrance, then fade

#### Moonlight Puddles → ImpactFoundation (DamageZoneShader)
- 180-frame persistent zone
- `scrollSpeed = 0.15`, `rotationSpeed = 0.1`, `circleRadius = 0.35`
- Purple-white, 25% enemy slow

### Asset Requirements
| Asset | Path | Prompt |
|-------|------|--------|
| Beam body | `Assets/MoonlightSonata/MoonlightsCalling/Beams/SerenadeBeam.png` | "Horizontal energy beam texture with smooth flowing center and soft shimmering edges, silver-blue-purple color gradient, on solid black background, 512x64px seamless tiling --ar 8:1 --style raw" |
| Harmonic wave | `Assets/MoonlightSonata/MoonlightsCalling/Beams/HarmonicWave.png` | "Standing wave pattern texture with bright nodes at regular intervals, white highlights on translucent blue, horizontal strip, on solid black background, 512x32px seamless --ar 16:1 --style raw" |
| Prismatic burst | `Assets/MoonlightSonata/MoonlightsCalling/Flare/PrismaticBurst.png` | "Radial energy burst with chromatic rainbow fringing at edges, bright white center fading to soft iridescent purple, on solid black background, 256x256px --ar 1:1 --style raw" |
| Puddle mask | `Assets/MoonlightSonata/MoonlightsCalling/Orbs/MoonlightPuddle.png` | "Top-down view of soft circular moonlight pool, gentle ripple edges, silver-blue glow with purple rim, on solid black background, 128x128px --ar 1:1 --style raw" |

---

## 5. Staff of the Lunar Phases (Summon) — VFX Rework Required

### Identity & Musical Soul
The Staff summons the **Goliath of Moonlight** — a massive spectral lunar entity. This is the Sonata's **silent accompanist** — the bass notes beneath the melody.

### Lore Line
*"The moon does not ask permission to illuminate the dark."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  STAFF OF THE LUNAR PHASES — Foundation Architecture│
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ InfernalBeamFoundation (InfernalBeamBody)     │  │
│  │  → Goliath Devastating Beam (VertexStrip)     │  │
│  │  → Multi-texture compositing, lunar gradient  │  │
│  │  → 3-layer spinning origin ring               │  │
│  ├───────────────────────────────────────────────┤  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → Goliath Standard Beam (BaseWidth=60)       │  │
│  │  → Lower-intensity 4-texture setup            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Goliath aura + Summoning circle            │  │
│  │  → Nebula noise, phase-cycling colors         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Waning Phase healing pulse visual          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MagicOrbFoundation (OrbBolt pattern)          │  │
│  │  → New Moon Phase rapid dark bolts            │  │
│  │  → 5-layer bloom bolt rendering               │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE: Phase cycling, Goliath entity sprites,     │
│          Summoning circle glyph ring                 │
└─────────────────────────────────────────────────────┘
```

### Lunar Phase Attack Modes
| Phase | Foundation | Configuration |
|-------|-----------|---------------|
| New Moon | **MagicOrbFoundation** (OrbBolt) | Rapid dark bolts, low damage, 5-layer bloom in dark purple |
| Waxing | **LaserFoundation** (ConvergenceBeam) | Standard beam, BaseWidth=60f, balanced |
| Full Moon | **InfernalBeamFoundation** (InfernalBeamBody) | Devastating beam, max damage, multi-texture compositing |
| Waning | **ImpactFoundation** (RippleShader) | Healing pulse, expanding ring per heal, blue-white colors |

### VFX Architecture — Foundation-Based

#### Devastating Beam → InfernalBeamFoundation
- **InfernalBeamBodyShader** via VertexStrip
- Re-themed: `noiseTex = Perlin` (gentle, not FBM), `noiseDistortion = 0.03`
- Lunar gradient LUT: deep purple → ice blue → white
- `BaseBeamWidth = 90f`, `MaxBeamLength = 2400f`
- 3-layer spinning ring at Goliath origin, re-tinted purple-blue

#### Standard Beam → LaserFoundation
- **ConvergenceBeamShader** at 0.6x texture multipliers
- `BaseBeamWidth = 60f`, lunar gradient LUT

#### Goliath Aura → MaskFoundation
- **RadialNoiseMaskShader** with Nebula noise
- `scrollSpeed = 0.15`, `rotationSpeed = 0.08`, `OrbDrawScale = 0.4f`
- Colors cycle through phase palette

#### Summoning Circle → MaskFoundation (variant)
- **RadialNoiseMaskShader** with `rotationSpeed = 0.2`, `scrollSpeed = 0.0`
- Custom circle texture, fade in/out lifecycle

### Asset Requirements
| Asset | Path | Prompt |
|-------|------|--------|
| Summoning circle | `Assets/MoonlightSonata/StaffOfTheLunarPhases/SummonCircle/LunarCircle.png` | "Top-down magic summoning circle with moon phase symbols around the edge, intricate lunar glyphs, soft purple glow on solid black background, 512x512px --ar 1:1 --style raw" |
| Goliath aura | `Assets/MoonlightSonata/StaffOfTheLunarPhases/Orbs/GoliathAura.png` | "Soft radial energy aura with irregular wispy edges, deep purple center to transparent edge, on solid black background, 256x256px --ar 1:1 --style raw" |
| Devastating beam | `Assets/MoonlightSonata/StaffOfTheLunarPhases/Beams/DevastatingBeam.png` | "Massive power beam texture with turbulent energy edges, white hot center fading to deep purple edges, horizontal strip, on solid black background, 512x128px seamless --ar 4:1 --style raw" |

---

## Foundation Coverage Matrix

| Foundation | Incisor | Eternal Moon | Resurrection | Calling | Staff |
|-----------|---------|-------------|-------------|---------|-------|
| SwordSmearFoundation | ✅ | ✅ | | | |
| RibbonFoundation | ✅ M4 | ✅ M3 | ✅ M6 | | |
| ThinSlashFoundation | ✅ | ✅ | | | |
| XSlashFoundation | | ✅ | | | |
| ImpactFoundation | ✅ Ripple | ✅ Both | ✅ Ripple | ✅ DamageZone | ✅ Ripple |
| ExplosionParticles | | | ✅ | | |
| SmokeFoundation | | | ✅ | | |
| SparkleProjectile | | | ✅ | | |
| LaserFoundation | | | | ✅ | ✅ |
| ThinLaserFoundation | | | | ✅ | |
| InfernalBeamFoundation | | | | | ✅ |
| MaskFoundation | | | | ✅ | ✅ |
| MagicOrbFoundation | | | | ✅ | ✅ |
| AttackAnimation | ✅ | | | | |

### Moonlight Lore Consistency
- All lore references moonlight, tides, silver, stillness, sorrow
- NEVER cosmos, stars, galaxies — those belong to Fate and Nachtmusik
- Foundation parameters always use lunar gradient LUTs, never fire/cosmic gradients
