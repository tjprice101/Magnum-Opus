# Foundation Weapons — Comprehensive Technical Summary

> **Purpose**: Reference document for weapon planning. Each foundation below demonstrates a specific VFX rendering pattern that can be used as scaffolding for production weapons. Use the foundation that matches your weapon's primary visual need, then layer additional foundations for secondary effects.

---

## Table of Contents

1. [AttackAnimationFoundation](#1-attackanimationfoundation) — Cinematic full-screen attack sequences
2. [AttackFoundation](#2-attackfoundation) — 5 distinct attack mode archetypes  
3. [ExplosionParticlesFoundation](#3-explosionparticlesfoundation) — CPU-driven spark/particle fields
4. [ImpactFoundation](#4-impactfoundation) — 3 shader-driven impact effect types
5. [InfernalBeamFoundation](#5-infernalbeamfoundation) — VertexStrip beam with spinning ring VFX
6. [LaserFoundation](#6-laserfoundation) — VertexStrip convergence beam with rainbow flares
7. [MagicOrbFoundation](#7-magicorbfoundation) — Shader-driven noise orb + bloom bolts
8. [MaskFoundation](#8-maskfoundation) — Radial noise masked to circle via shader
9. [RibbonFoundation](#9-ribbonfoundation) — 10 trail rendering modes (bloom + texture-strip)
10. [SmokeFoundation](#10-smokefoundation) — Calamity-style smoke puff particle system
11. [SparkleProjectileFoundation](#11-sparkleprojectilefoundation) — Dual-shader crystal + glitter trail
12. [SwordSmearFoundation](#12-swordsmearfoundation) — Melee swing arc with distortion shader
13. [ThinLaserFoundation](#13-thinlaserfoundation) — Ricocheting thin beam via VertexStrip
14. [ThinSlashFoundation](#14-thinslashfoundation) — SDF razor-thin slash line via shader
15. [XSlashFoundation](#15-xslashfoundation) — Blazing X-shaped impact with fire shader

---

## 1. AttackAnimationFoundation

**Effect Type**: Full-screen cinematic attack animation with camera control, B&W impact flash, and orchestrated slash VFX.

### Files
| File | Role |
|------|------|
| `AttackAnimationFoundation.cs` | ModItem — weapon that fires the orchestrator projectile |
| `AttackAnimationProjectile.cs` | ModProjectile — orchestrator that controls the entire animation sequence |
| `SlashVFXProjectile.cs` | ModProjectile — individual slash visual (bloom arc + impact flash) |
| `AAFPlayer.cs` | ModPlayer — camera pan system + ModSystem screen overlay effects |
| `AAFTextures.cs` | Static texture cache (SoftGlow, StarFlare, GlowOrb, SoftRadialBloom, LensFlare, PointBloom, HardCircleMask) |

### Shaders
**None** — entirely SpriteBatch-based bloom stacking.

### Architecture
- **Phased timer-driven sequence** (~110 frames total): Camera Pan → 4 Slashes → Final Slash → Return
- **Camera control**: `AAFPlayer.ModifyScreenPosition()` lerps `Main.screenPosition` between player and target
- **Screen effects**: `AAFScreenEffectSystem : ModSystem` draws in PostDrawTiles:
  - Progressive brightness overlay (additive full-screen SoftGlow)
  - B&W impact frame (AlphaBlend white flash → dark overlay)
  - Noise zone on enemy (layered SoftGlow + HardCircleMask + orbiting PointBloom sparkles)

### Rendering Pattern
```
SlashVFXProjectile.PreDraw (per-slash):
  └─ Additive pass:
      ├─ Directional slash bloom (SoftGlow stretched along slash direction)
      ├─ Center impact flash (StarFlare spinning + GlowOrb)
      └─ Final slash extra layers (SoftRadialBloom + LensFlare)

AttackAnimationProjectile.PreDraw:
  └─ Additive pass:
      ├─ Motion blur trails (SoftGlow stretched along slash direction)
      └─ Dash lines

AAFScreenEffectSystem.PostDrawTiles:
  └─ Overlay passes (full-screen draws)
```

### Key Techniques
- **Bloom stacking**: Multiple SoftGlow draws at different scales/alphas for depth
- **Velocity-stretched sprites**: `new Vector2(scaleX, scaleY)` with directional rotation
- **Phased animation**: Timer-based state machine controlling spawn timing of sub-projectiles
- **Camera interpolation**: Smooth lerp from player → target → player over the animation

### When to Use
Use for **dramatic finisher moves**, **ultimate attacks**, or **boss phase transitions** where you need cinematic camera control, screen overlays, and orchestrated multi-hit sequences.

---

## 2. AttackFoundation

**Effect Type**: 5 distinct attack mode archetypes showcasing different weapon class patterns (melee combo, thrown slam, orbiting geometry, summoner ring, ranged tracer).

### Files
| File | Role |
|------|------|
| `AttackFoundation.cs` | ModItem — right-click cycles between 5 attack modes |
| `AFTextures.cs` | Static texture cache |
| `ComboSwingProjectile.cs` | 3-phase melee combo (downswing → upswing → spin throw) |
| `ThrowSlamProjectile.cs` | 3-phase thrown weapon (rise → hover → dive-bomb) |
| `AstralgraphProjectile.cs` | Orbiting star polygon (7 nodes, skip-3 star pattern) |
| `FlamingRingProjectile.cs` | Summoner fire ring (12 orbiting orbs + AoE damage) |
| `RangerShotProjectile.cs` | Piercing ranged tracer bolt with muzzle flash |

### Shaders
**None** — all SpriteBatch-based.

### Sub-Mode Details

#### ComboSwingProjectile
- **3-phase melee combo**: Downswing (10 frames) → Upswing (8 frames) → Spin Throw (projectile)
- Smoothstep easing for swing arcs, arc-based collision (line segment check)
- Additive bloom: SoftGlow at blade position + PowerEffectRing at player center
- Player animation control: `heldProj`, `itemTime`, `itemAnimation`, `itemRotation`

#### ThrowSlamProjectile  
- **3-phase thrown weapon**: Rise (20f, upward decel) → Hover (15f, target acquisition) → Dive (60f, homing)
- Sword sprite (Katana texture) drawn spinning at center
- Phase-dependent bloom intensity (`trailAlpha` increases through phases)
- Velocity streak during dive phase (SoftGlow stretched along velocity)

#### AstralgraphProjectile
- **Star polygon**: 7 nodes arranged skip-3 (heptagram), connected by glow lines
- Lines drawn as stretched SoftGlow between nodes
- Node blooms: PointBloom + StarFlare at each vertex
- Lifecycle: Expand (25f) → Hold (80f) → Contract (15f)

#### FlamingRingProjectile
- **Summoner ring**: 12 fire orbs orbiting player at configurable radius
- Expand (18f, EaseOutQuad) → Hold (50f, pulsing radius) → Contract (15f, EaseInQuad)
- AoE damage every 10 frames to enemies within ring radius
- Renders: per-orb SoftGlow + GlowOrb, arc connectors (stretched SoftGlow), PowerEffectRing outline, central heat glow
- Applies OnFire debuff

#### RangerShotProjectile
- **Ranged tracer**: Fast piercing bolt, `extraUpdates = 1`
- Ring buffer of 8 trail positions for afterimage
- Muzzle flash: radial dust burst + forward cone dust + SoftGlow bloom + LensFlare (first 6 frames)
- Velocity streak: SoftGlow stretched along velocity direction
- Afterimage trail: fading SoftGlow at historical positions

### Key Techniques
- **Mode cycling**: Right-click cycles `Projectile.ai[0]` through 5 modes via `AltFunctionUse`
- **Phase state machines**: Timer-driven phase transitions with eased interpolation
- **Ring orbit math**: `orbAngle + (TwoPi / OrbCount) * n` for equidistributed points
- **Star polygon math**: `baseAngle + (TwoPi / nodeCount) * n` with skip-3 connection pattern
- **Directional muzzle flash**: SoftGlow + LensFlare at spawn position, fading over 6 frames

### When to Use
Use individual sub-projectiles as **templates for specific weapon behaviors**: melee combos, thrown weapons, orbital/summoner patterns, ranged tracers. Adapt the phase state machine pattern for any weapon with distinct attack phases.

---

## 3. ExplosionParticlesFoundation

**Effect Type**: CPU-driven scattering spark/particle fields with 3 spawn patterns (radial, fountain, spiral).

### Files
| File | Role |
|------|------|
| `ExplosionParticlesFoundation.cs` | ModItem — fires carrier projectile |
| `EPFTextures.cs` | Static texture cache (SolidWhiteLine, Star4Hard, SoftGlow, StarFlare, LensFlare, GlowOrb) |
| `SparkCarrierProjectile.cs` | Travel projectile with additive bloom + velocity streak |
| `SparkExplosionProjectile.cs` | Main effect — manages 55 internal Spark structs |

### Shaders
**None** — entirely CPU-side particle simulation and SpriteBatch rendering.

### Architecture
- **55 internal `Spark` structs** per explosion: Position, Velocity, Rotation, Scale, Alpha, Length, GravityMult, Friction, SparkType
- **3 spawn patterns** (right-click cycles):
  - **Radial Scatter**: Random circular burst (`NextVector2Circular`)
  - **Fountain Cascade**: Upward cone with gravity
  - **Spiral Shrapnel**: Baselined spiral pattern with rotation

### Rendering Pattern
```
SparkExplosionProjectile.PreDraw:
  └─ Additive pass:
      ├─ Per-spark rendering (55 sparks):
      │   ├─ Type 0: Elongated line (SolidWhiteLine stretched by velocity)
      │   ├─ Type 1: 4-pointed star (Star4Hard)
      │   └─ Type 2: Tiny dot (GlowOrb)
      │   └─ Each gets a SoftGlow backdrop bloom
      └─ Center flash:
          ├─ SoftGlow (large, fading)
          ├─ StarFlare (spinning)
          └─ LensFlare
```

### Key Techniques
- **Per-particle physics**: Each spark has individual gravity, friction, and rotation alignment to velocity
- **3 visual spark types**: Lines (velocity-stretched), stars (4-pointed), dots (small orbs)
- **Rotation alignment**: `spark.Rotation = spark.Velocity.ToRotation()` for line sparks
- **Center flash envelope**: Fades over first 8 frames with expanding SoftGlow + spinning StarFlare

### When to Use
Use for **impact explosions**, **death bursts**, **projectile detonations**, or any **radial particle effect** where you want physically-simulated sparks without shader overhead. Combine with ImpactFoundation for shader + particle hybrid impacts.

---

## 4. ImpactFoundation

**Effect Type**: 3 distinct shader-driven impact effects (ripple rings, damage zone, slash mark).

### Files
| File | Role |
|------|------|
| `ImpactFoundation.cs` | ModItem — right-click cycles modes, fires carrier |
| `IFTextures.cs` | Static texture cache (SoftCircle, SoftGlow, GlowOrb, PointBloom, StarFlare + noise textures) |
| `ImpactProjectile.cs` | Homing carrier projectile, spawns impact effect on NPC hit |
| `RippleEffectProjectile.cs` | Animated concentric rings via RippleShader |
| `DamageZoneProjectile.cs` | Persistent radial noise zone via DamageZoneShader |
| `SlashMarkProjectile.cs` | Directional SDF slash arc via SlashMarkShader |

### Shaders

#### RippleShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated | Animation driver |
| `progress` | float | 0→1 | Expansion progress |
| `ringCount` | float | 4 | Number of concentric rings |
| `ringThickness` | float | 0.04 | Width of each ring |
| `primaryColor` | float3 | theme | Outer ring color |
| `secondaryColor` | float3 | theme | Alternating ring color |
| `coreColor` | float3 | theme | Center ring color |
| `fadeAlpha` | float | 0→1→0 | Overall opacity |
| `noiseTex` | Texture2D | PerlinNoise | Ring distortion source |

- **Pipeline**: SpriteBatch with shader as Effect parameter
- **BlendState**: Additive
- **SamplerState**: LinearWrap
- **Technique**: SDF rings in polar coordinates + noise distortion for organic edges

#### DamageZoneShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated | Scroll/rotation driver |
| `scrollSpeed` | float | 0.2 | Radial scroll rate |
| `rotationSpeed` | float | 0.1 | Angular rotation rate |
| `circleRadius` | float | 0.44 | Mask circle size (UV space) |
| `edgeSoftness` | float | 0.06 | Edge fadeoff width |
| `intensity` | float | 1.8 | Brightness multiplier |
| `primaryColor` | float3 | theme | Outer color |
| `coreColor` | float3 | theme | Center color |
| `fadeAlpha` | float | 0→1→0 | Overall opacity |
| `breathe` | float | sin-wave | Pulsing scale modulator |
| `noiseTex` | Texture2D | NoiseFBM | Primary noise source |
| `gradientTex` | Texture2D | theme LUT | Color ramp |

- **Pipeline**: SpriteBatch with shader
- **Duration**: 300 frames (persistent damage zone)
- **Extra VFX**: Orbiting PointBloom sparkles around edge (CPU-side)

#### SlashMarkShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated | Animation driver |
| `slashAngle` | float | velocity angle | Slash direction |
| `primaryColor` | float3 | theme | Outer color |
| `coreColor` | float3 | theme | Center color |
| `fadeAlpha` | float | 0→1→0 | Overall opacity |
| `slashWidth` | float | 0.06 | Arc thickness (UV space) |
| `slashLength` | float | 0.35 | Arc extent (UV space) |
| `noiseTex` | Texture2D | NoiseCosmicVortex | Edge distortion |
| `gradientTex` | Texture2D | theme LUT | Color ramp |

- **Pipeline**: SpriteBatch with shader
- **Technique**: SDF-based directional arc with noise-distorted edges
- **Extra VFX**: Directional bloom (stretched SoftGlow along slash angle)

### Rendering Pattern (per effect type)
```
RippleEffectProjectile.PreDraw:
  ├─ Additive bloom backdrop (SoftGlow multi-scale)
  ├─ Shader pass: RippleShader on SoftCircle quad
  └─ Center flash (StarFlare + GlowOrb)

DamageZoneProjectile.PreDraw:
  ├─ Shader pass: DamageZoneShader on SoftCircle quad
  ├─ Additive bloom halo
  └─ Orbiting edge sparkles (PointBloom at radius)

SlashMarkProjectile.PreDraw:
  ├─ Directional bloom (SoftGlow stretched along slash angle)
  ├─ Shader pass: SlashMarkShader on SoftCircle quad
  └─ Additive flash (StarFlare)
```

### Key Techniques
- **SDF-based shapes**: Rings, arcs, and circles computed mathematically in the shader, not from texture shapes
- **Noise distortion**: FBM/Perlin/CosmicVortex textures warp SDF edges for organic feel
- **Gradient LUT coloring**: Intensity → color via 1D gradient texture sampling
- **Polar coordinate conversion**: UV → (angle, radius) for radial effects
- **Orbiting sparkles**: Angle-offset PointBloom sprites at zone edge (CPU-side, supplements shader)

### When to Use
Use for **hit effects**, **on-impact marks**, **persistent damage zones**, or any **localized shader-driven effect** at a specific position. The 3 sub-types cover rings/shockwaves, area denial, and directional slash marks.

---

## 5. InfernalBeamFoundation

**Effect Type**: Channeled beam with multi-texture VertexStrip body and spinning ring at origin.

### Files
| File | Role |
|------|------|
| `InfernalBeamFoundation.cs` | ModItem — channeled weapon, holds to fire beam |
| `IBFTextures.cs` | Static texture cache |
| `InfernalBeam.cs` | ModProjectile — beam body rendering + collision |

### Shaders

#### InfernalBeamBodyShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `WorldViewProjection` | Matrix | transform | Vertex transform |
| `onTex` | Texture2D | BeamAlphaMask | Alpha/shape mask |
| `gradientTex` | Texture2D | theme LUT | Color ramp |
| `bodyTex` | Texture2D | SoundWaveBeam | Primary body pattern |
| `detailTex1` | Texture2D | EnergyMotion | Detail layer 1 |
| `detailTex2` | Texture2D | EnergySurge | Detail layer 2 |
| `noiseTex` | Texture2D | NoiseFBM | UV distortion source |
| `bodyReps` / `detail1Reps` / `detail2Reps` / `gradientReps` | float | proportional to length | UV repetition per texture |
| `bodyScrollSpeed` | float | 0.8 | Primary scroll rate |
| `detail1ScrollSpeed` | float | 1.2 | Detail 1 scroll rate |
| `detail2ScrollSpeed` | float | -0.6 | Detail 2 scroll rate (reversed) |
| `noiseDistortion` | float | 0.03 | Distortion strength |
| `totalMult` | float | 1.2 | Overall brightness |
| `uTime` | float | animated | Time driver |

- **Pipeline**: VertexStrip (raw vertex mesh, no SpriteBatch)
- **Vertex construction**: 2 endpoint positions (origin + raycast endpoint), strip expanded to beam width
- **Width function**: `BaseBeamWidth * breathePulse` (100px base, sin-wave breathing)
- **Collision**: Tile raycast every 16px (`RaycastBeamLength`), `Collision.CheckAABBvLineCollision` for NPC hits

### Rendering Pattern
```
InfernalBeam.PreDraw:
  ├─ VertexStrip beam body (InfernalBeamBodyShader)
  │   └─ 2-position strip (origin → endpoint), shader does all visual work
  ├─ Additive pass:
  │   ├─ Origin spinning ring (InfernalBeamRing.png × 3 rotation passes)
  │   │   ├─ Main rotation (time × 0.03)
  │   │   ├─ Offset rotation (+PiOver4, time × 0.02, dimmer)
  │   │   └─ Counter-spin (-time × 0.015, even dimmer)
  │   ├─ PointBloom at center
  │   └─ Endpoint flares:
  │       ├─ StarFlare (spinning)
  │       ├─ GlowOrb
  │       └─ LensFlare
  └─ Restore AlphaBlend
```

### Key Techniques
- **Multi-texture beam body**: 3 body textures + noise distortion, each scrolling at different speeds/directions
- **VertexStrip mesh**: `strip.PrepareStrip(positions, rotations, colorFunc, widthFunc, -screenPos, includeBacksides: true)`
- **Proportional UV reps**: `beamLength / 450f` ensures texture tiling scales with beam distance
- **Spinning ring asset**: 3 overlapping draws at different rotations for layered rotation effect
- **Breathing pulse**: `0.95f + 0.05f * sin(time * 0.06f)` modulates beam width
- **Tile raycast**: Steps every 16px until hitting solid tile, determines beam endpoint

### When to Use
Use for **channeled beams**, **continuous laser attacks**, or **energy streams** where the beam body needs rich internal texture animation. The spinning ring at origin is a bonus pattern for charge-up indicators or beam source VFX.

---

## 6. LaserFoundation

**Effect Type**: Channeled convergence beam with 4-detail-texture shader body and rainbow flare endpoints.

### Files
| File | Role |
|------|------|
| `LaserFoundation.cs` | ModItem — channeled weapon |
| `LFTextures.cs` | Static texture cache |
| `LaserFoundationBeam.cs` | ModProjectile — beam body + endpoint rendering |
| `LFEasings.cs` | Easing utility functions |

### Shaders

#### ConvergenceBeamShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `WorldViewProjection` | Matrix | transform | Vertex transform |
| `onTex` | Texture2D | BeamAlphaMask | Shape mask |
| `gradientTex` | Texture2D | theme LUT | Color ramp |
| `baseColor` | float3 | White | Base tint |
| `satPower` | float | 0.8 | Saturation curve |
| `sampleTexture1-4` | Texture2D | Various detail textures | 4 scrolling detail layers |
| `grad1-4Speed` | float | 0.66–1.03 | Per-texture scroll speeds |
| `tex1-4Mult` | float | 1.25–2.5 | Per-texture brightness |
| `totalMult` | float | 1.0 | Overall brightness |
| `gradientReps` / `tex1-4reps` | float | proportional to length/2000 | UV repetition |
| `uTime` | float | animated | Time driver |

- **Detail textures**: DetailThinGlowLine, DetailSpark, DetailExtra, DetailTrailLoop
- **Pipeline**: VertexStrip (same as InfernalBeam)
- **Key difference**: 4 detail textures vs InfernalBeam's 3, no noise distortion

#### FlareRainbowShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `rotation` | float | animated | Flare rotation |
| `rainbowRotation` | float | slow spin | HSV hue offset |
| `intensity` | float | 1.0 | Brightness |
| `fadeStrength` | float | 1.0 | Alpha control |

- **Pipeline**: SpriteBatch with shader passed as Effect parameter
- **Technique**: Converts white flare textures to spinning rainbow gradient in HSV space

### Rendering Pattern
```
LaserFoundationBeam.PreDraw:
  ├─ VertexStrip beam body (ConvergenceBeamShader)
  │   └─ 2-position strip, shader composites 4 detail textures + gradient
  ├─ Additive pass:
  │   └─ Endpoint flares (FlareRainbowShader via SpriteBatch):
  │       ├─ SoftGlow (sigil-stretched, large)
  │       ├─ LensFlare (oscillating scale)
  │       ├─ StarFlare (double-draw: large dim + small bright)
  │       └─ GlowOrb (core)
  └─ Restore AlphaBlend
```

### Key Techniques
- **4-detail-texture compositing**: Each detail texture scrolls at independent speeds, multiplied by individual brightness factors, then combined
- **FlareRainbowShader**: HSV hue rotation applied to white flare sprites — converts any white texture to a rainbow version
- **Proportional UV scaling**: All texture reps = `beamLength / 2000f * multiplier`
- **SpriteBatch.Begin with Effect**: `sb.Begin(SpriteSortMode.Immediate, BlendState.Additive, ..., shader, ...)`

### When to Use
Use for **convergence beams**, **prismatic lasers**, or **multi-layered beam attacks** where you want richer internal detail than InfernalBeam. FlareRainbowShader is independently useful for any rainbow/prismatic flare effect.

---

## 7. MagicOrbFoundation

**Effect Type**: Floating shader-driven noise orb that fires additive bloom bolts.

### Files
| File | Role |
|------|------|
| `MagicOrbFoundation.cs` | ModItem — fires orb on swing |
| `MOFTextures.cs` | Static texture cache |
| `MagicOrb.cs` | ModProjectile — shader-rendered orb with bolt-spawning AI |
| `OrbBolt.cs` | ModProjectile — pure bloom bolt (no shader) |

### Shaders
**Reuses MaskFoundation's RadialNoiseMaskShader.fx** — see [MaskFoundation](#8-maskfoundation) for shader details.

### Architecture
- **MagicOrb**: Two modes:
  - *Normal*: Speed 4, 480-frame life, fires bolt every 30 frames, slow drift
  - *Burst*: Speed 7, 120-frame life, fires bolt every 20 frames, explodes on death
- **OrbBolt**: Pure additive bloom rendering, gentle homing

### Rendering Pattern
```
MagicOrb.PreDraw:
  ├─ Additive pass: Bloom halo (SoftGlow × 3 scales)
  ├─ Shader pass: RadialNoiseMaskShader on SoftCircle quad
  │   └─ SamplerState.LinearWrap, BlendState.Additive
  └─ Additive pass: Core bloom (GlowOrb × 2 scales)

OrbBolt.PreDraw:
  └─ Additive pass (5 layers, no shader):
      ├─ Outer SoftGlow (large, dim, theme-colored)
      ├─ Mid SoftGlow (medium, brighter)
      ├─ Core GlowOrb (tight, intense)
      ├─ StarFlare directional (aligned to velocity)
      └─ StarFlare perpendicular (cross gleam)
```

### Key Techniques
- **Shader reuse**: MagicOrb reuses MaskFoundation's shader, just configures different uniforms
- **5-layer pure bloom**: OrbBolt demonstrates maximum visual impact with zero shader cost — 5 additive sprite draws create a convincing glowing bolt
- **Bolt spawning AI**: Timer-driven projectile spawning from within a projectile's AI

### When to Use
Use MagicOrb as a template for any **orbiting projectile with a shader-driven body** (turrets, sentries, floating weapons). Use OrbBolt as a template for **simple glowing projectiles** where shader overhead isn't justified.

---

## 8. MaskFoundation

**Effect Type**: Homing noise-textured orb masked to a soft circle via custom shader.

### Files
| File | Role |
|------|------|
| `MaskFoundation.cs` | ModItem — fires orb on melee swing |
| `MFTextures.cs` | Static texture cache + mode color/noise/gradient selectors |
| `MaskOrbProjectile.cs` | ModProjectile — shader-rendered homing orb |

### Shaders

#### RadialNoiseMaskShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated + seed | Time driver |
| `scrollSpeed` | float | 0.3 | Radial outward scroll rate |
| `rotationSpeed` | float | 0.15 | Angular rotation rate |
| `circleRadius` | float | 0.45 | Mask circle size (UV space) |
| `edgeSoftness` | float | 0.08 | Smoothstep edge falloff |
| `intensity` | float | 2.0 | Brightness multiplier |
| `primaryColor` | float3 | theme | Outer color |
| `coreColor` | float3 | theme | Center color |
| `noiseTex` | Texture2D | per-mode | Primary noise source |
| `gradientTex` | Texture2D | per-mode | Color ramp LUT |

- **Pipeline**: SpriteBatch with shader as Effect parameter
- **BlendState**: Additive
- **SamplerState**: LinearWrap (critical for noise tiling)
- **Technique**: UV → polar coordinates → scroll/rotate noise → dual noise layer → gradient LUT → circle mask via smoothstep distance

### Rendering Pattern
```
MaskOrbProjectile.PreDraw:
  ├─ Layer 1: Additive bloom halo (SoftGlow × 3 scales, pulsing)
  ├─ Layer 2: Shader pass (RadialNoiseMaskShader on SoftCircle)
  │   └─ sb.Begin(Deferred, Additive, LinearWrap, ..., shader, ...)
  └─ Layer 3: Additive core bloom (GlowOrb × 2, pulsing)
```

### Key Techniques
- **Polar coordinate noise scrolling**: Converts UV to (angle, radius), scrolls noise outward radially AND rotates angularly — creates a vortex-like swirl inside the orb
- **Dual noise layers**: Two noise samples at different scales combined for rich detail
- **Circle masking via SDF**: `smoothstep(radius + softness, radius - softness, dist)` cuts the noise cleanly to a circle
- **Mode-dependent textures**: `MFTextures.GetNoiseForMode()` / `GetGradientForMode()` swap noise/gradient per style
- **SoftCircle quad**: The sprite used is just a soft circle — the shader replaces its visual content entirely

### When to Use
Use for **any circular shader-driven effect** (orbs, auras, shields, portals). The RadialNoiseMaskShader is the most reusable shader in the foundation set — already referenced by MagicOrbFoundation and DamageZoneProjectile.

---

## 9. RibbonFoundation

**Effect Type**: 10 different ribbon/trail rendering modes demonstrating the full range of trail techniques.

### Files
| File | Role |
|------|------|
| `RibbonFoundation.cs` | ModItem — fires ribbon projectile |
| `RBFTextures.cs` | Static texture cache (SoftGlow, SoftGlowBright, PointBloom, StarFlare, PerlinNoise, BasicTrail, HarmonicWaveRibbon, SpiralingVortexStrip, EnergySurgeBeam, CosmicNebulaClouds, MusicalWavePattern, TileableMarbleNoise, LightningSurge, MusicNoteOrb) |
| `RibbonProjectile.cs` | ModProjectile — 10 rendering modes for the trail |

### Shaders
**None** — all CPU-side SpriteBatch rendering (demonstrating what can be achieved without shaders).

### Trail Infrastructure
- **Ring buffer**: 40 positions, `extraUpdates = 1` doubles recording density
- **Buffer unwinding**: `(trailIndex - trailCount + i + TrailLength * 2) % TrailLength` gives oldest→newest ordering

### 10 Rendering Modes

| Mode | Name | Technique |
|------|------|-----------|
| 0 | **PureBloom** | 3-layer bloom sprites (outer/body/core) at each trail position, velocity-stretched |
| 1 | **BloomNoiseFade** | Like PureBloom but with noise erosion — sin-based mathematical noise with erosion threshold that increases toward tail |
| 2 | **BasicTrailStrip** | Texture-strip rendering with BasicTrail texture |
| 3 | **HarmonicWave** | Texture-strip with HarmonicWaveRibbon texture |
| 4 | **SpiralingVortex** | Texture-strip with SpiralingVortexStrip texture |
| 5 | **EnergySurge** | Texture-strip with EnergySurgeBeam texture |
| 6 | **CosmicNebula** | Texture-strip with CosmicNebulaClouds texture |
| 7 | **MusicalWave** | Texture-strip with MusicalWavePattern texture |
| 8 | **MarbleFlow** | Texture-strip with TileableMarbleNoise texture |
| 9 | **LightningRibbon** | Lightning texture-strip + jitter + aggressive bloom flashes |

### Texture-Strip Rendering (Modes 2-9)
```csharp
// For each trail segment:
float uStart = (progress + time * 2f) % 1f;    // UV scrolling
int srcX = (int)(uStart * texW) % texW;          // Source rect X
Rectangle srcRect = new(srcX, 0, srcWidth, texH); // Vertical slice

float scaleX = segLength / (float)srcWidth;      // Stretch to segment
float scaleY = width / (float)texH;              // Match ribbon width

// Origin at left-center for directional stretching
Vector2 origin = new(0, texH / 2f);

sb.Draw(stripTex, pos, srcRect, bodyColor, segAngle, origin,
    new Vector2(scaleX, scaleY), SpriteEffects.None, 0f);
```

### Key Techniques
- **Texture-strip without VertexStrip**: Achieves UV-mapped ribbon look by drawing sequential vertical slices of a texture at each segment, oriented along the trail direction
- **UV scrolling**: `(progress + time * 2f) % 1f` creates internal animation
- **Width tapering**: `Lerp(RibbonWidthTail, RibbonWidthHead, progress)` — 2px tail to 20px head
- **Noise erosion**: Mathematical sin-based pseudo-noise determines which trail points are visible — creates organic dissolve at tail
- **Lightning jitter**: Random sin-based position offsets for electric crackling
- **Bloom overlay**: Separate loop drawing ~15 SoftGlow sprites along trail for ambient glow

### Head + Body
- **Head bloom**: Multi-scale SoftGlow + StarFlare at projectile position
- **Orb body**: MusicNoteOrb (1024×1024) scaled to 36px (`OrbDisplayScale = 0.035f`)

### When to Use
Use for **any trailing effect behind a projectile** — weapon swing afterimages, projectile trails, dash trails. Choose between bloom-based (modes 0-1) for pure glow trails, texture-strip (modes 2-8) for textured ribbons, or lightning (mode 9) for electric effects. The texture-strip technique is the key takeaway — it achieves UV-mapped ribbon rendering without custom vertex meshes.

---

## 10. SmokeFoundation

**Effect Type**: Calamity-style smoke puff particle system with spritesheet animation.

### Files
| File | Role |
|------|------|
| `SmokeFoundation.cs` | ModItem — fires carrier projectile |
| `SKFTextures.cs` | Static texture cache (SmokeGrid 3×6 spritesheet, SoftGlow, StarFlare, style colors) |
| `SmokeCarrierProjectile.cs` | Travel projectile with additive bloom |
| `SmokeRingProjectile.cs` | Smoke explosion — manages 30 SmokePuff structs |

### Shaders
**None** — CPU-side particle system with spritesheet rendering.

### Calamity Supernova Pattern
The smoke ring replicates Calamity's `SupernovaBoom` → `HeavySmokeParticle` pattern:

```csharp
// Spawn pattern (Calamity's offset generation):
Vector2 ring = new Vector2(15f, 15f).RotatedByRandom(100) * rand(0.8f, 1.6f);
// Same vector used as BOTH spawn offset AND initial velocity
```

### SmokePuff Lifecycle
| Phase | Timing | Behavior |
|-------|--------|----------|
| **Expansion** | First 20% of life | Scale grows by +0.01/frame |
| **Contraction** | After 20% | Scale shrinks by ×0.975/frame |
| **Opacity** | Always | ×0.98/frame decay |
| **Velocity** | Always | ×0.85/frame drag (heavy — smoke doesn't travel far) |
| **Final fade** | Last 15% | Additional rapid alpha via `Utils.GetLerpValue(1, 0.85, completion)` |
| **Color shift** | Over lifetime | Core (hot) → body → edge (cool/dark) |

### Rendering Pattern
```
SmokeRingProjectile.PreDraw:
  ├─ Additive pass: Smoke puff bodies
  │   └─ 30 puffs from 3×6 spritesheet grid
  │       ├─ Random frame index (0-17)
  │       ├─ Random flip (H/V) for variety
  │       ├─ Color lifecycle: core→body→edge
  │       └─ Scale: 0.9-2.3 range × 0.3 render scale
  ├─ Additive pass: Center flash + glow accents
  │   ├─ Center flash (first 10 frames): SoftGlow expanding + core
  │   └─ Per-puff glow accents: SoftGlow behind each puff (hot phase only)
  └─ Restore AlphaBlend
```

### Key Techniques
- **Spritesheet frame selection**: `SKFTextures.GetFrameRect(frameIndex)` computes source rect from 3×6 grid
- **Random flip for variety**: `SpriteEffects.FlipHorizontally | FlipVertically` prevents identical puffs
- **Calamity-accurate lifecycle**: Exact replication of expansion/contraction/drag/fade timing
- **Heavy velocity drag**: ×0.85/frame makes smoke feel thick and heavy
- **AoE damage on spawn**: First-frame radial damage with distance falloff

### When to Use
Use for **smoke explosions**, **bomb detonations**, **steam vents**, or any **thick volumetric cloud effect**. The Calamity lifecycle pattern creates realistic smoke behavior. Combine with ExplosionParticlesFoundation for smoke + sparks.

---

## 11. SparkleProjectileFoundation

**Effect Type**: Dual-shader crystal projectile with glitter trail (VertexStrip) and prismatic body (SpriteBatch).

### Files
| File | Role |
|------|------|
| `SparkleProjectileFoundation.cs` | ModItem — spawns 3 crystal shards per swing |
| `SPFTextures.cs` | Static texture cache |
| `SparkleCrystalProjectile.cs` | ModProjectile — 5-layer rendering with 2 shaders |

### Shaders

#### SparkleTrailShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `WorldViewProjection` | Matrix | transform | Vertex transform |
| `uTime` | float | animated + seed | Time driver |
| `sparkleTex` | Texture2D | 4PointedStarHard | Internal sparkle pattern |
| `gradientTex` | Texture2D | theme LUT | Color ramp |
| `glowMaskTex` | Texture2D | SoftCircle | Cross-section shaping |
| `coreColor` | float3 | theme[4] | Hot center color |
| `outerColor` | float3 | theme[0] | Theme outer color |
| `trailIntensity` | float | 1.5 | Brightness |
| `sparkleSpeed` | float | 1.2 | Internal animation speed |
| `sparkleScale` | float | 3.0 | Sparkle pattern scale |
| `glitterDensity` | float | 2.5 | Sparkle point frequency |
| `tipFadeStart` | float | 0.7 | Where tip fadeoff begins |
| `edgeSoftness` | float | 0.4 | Edge fade width |

- **Pipeline**: VertexStrip (raw vertex mesh)
- **Technique**: Star texture UV scrolling + procedural multi-frequency sin-wave interference raised to high power for sharp glitter peaks

#### CrystalShimmerShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated | Time driver |
| `rotation` | float | body rotation | Facet orientation |
| `shimmerSpeed` | float | 2.0 | Flash animation speed |
| `flashIntensity` | float | 1.5 | Flash brightness |
| `baseAlpha` | float | lifeFade × 0.6 | Overall opacity |
| `primaryColor` | float3 | theme[0] | Primary facet color |
| `highlightColor` | float3 | theme[4] | Bright flash color |
| `gradientTex` | Texture2D | theme LUT | Color ramp |

- **Pipeline**: SpriteBatch with shader as Effect parameter
- **Technique**: 6-facet angular shimmer (`sin(angle×6)`) + 4-facet interference + HSV prismatic color at boundaries + pow(16) sparkle peaks

### Rendering Pattern (5 Layers)
```
SparkleCrystalProjectile.PreDraw:
  ├─ Layer 1: SparkleTrailShader via VertexStrip
  │   └─ 24-point ring buffer → triangle strip mesh → glitter pattern
  ├─ Layer 2: Bloom trail (Photoviscerator-style)
  │   └─ Per-position: 3 additive layers (outer/body/core), velocity-stretched
  │       └─ White-hot head → theme-colored tail transition
  ├─ Layer 3: Bloom halo (SoftGlow × 3 scales + SoftRadialBloom)
  ├─ Layer 4: Crystal body
  │   ├─ Pass A: Star sprite directly (no shader) — theme tinted + white-hot core
  │   ├─ Pass B: Counter-rotated overlay for depth
  │   ├─ Pass C: Star flare cross gleam
  │   └─ Pass D: CrystalShimmerShader overlay (smaller, on top)
  └─ Layer 5: Sparkle accents (no shader)
      ├─ 4 orbiting sparkle points (cubic sin-wave flash)
      └─ Central twinkle pulse (pow(6) peaks)
```

### Key Techniques
- **5-layer compositing**: Most complex rendering in the foundation set — shader trail + bloom trail + bloom halo + dual-pass body + sparkle accents
- **Photoviscerator-style bloom trail**: Shrinking bloom sprites with cubic fade and velocity stretching
- **Dual shader rendering**: VertexStrip shader for trail + SpriteBatch shader for body — different pipelines working together
- **Cubic/pow flash timing**: `sin³` and `pow(value, 6)` create brief bright peaks with long dark gaps — simulates sparkling
- **Phase offset**: 3 crystals spawned 120° apart (`CrystalIndex × TwoPi / 3`) for staggered sparkle timing
- **extraUpdates = 1**: Doubles AI calls per frame for smoother trail and movement

### When to Use
Use for **sparkling/crystalline projectiles**, **gem weapons**, **star weapons**, or any effect requiring **dual-shader rendering** (VertexStrip trail + SpriteBatch body). The Photoviscerator bloom trail technique is independently useful for any fast-moving glowing projectile.

---

## 12. SwordSmearFoundation

**Effect Type**: Melee swing with shader-driven distorted smear arc overlay.

### Files
| File | Role |
|------|------|
| `SwordSmearFoundation.cs` | ModItem — fires swing projectile on use |
| `SMFTextures.cs` | Static texture cache + SmearDistortShader loading |
| `SmearSwingProjectile.cs` | ModProjectile — swing arc + smear overlay rendering |

### Shaders

#### SmearDistortShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | real seconds | Time driver |
| `fadeAlpha` | float | envelope | Overall opacity |
| `distortStrength` | float | 0.05–0.08 | Noise distortion amount |
| `flowSpeed` | float | 0.4 | Energy flow animation speed |
| `noiseScale` | float | 2.5 | Noise texture zoom |
| `noiseTex` | Texture2D | FBMNoise | Distortion source |
| `gradientTex` | Texture2D | per-style LUT | Color ramp |

- **Pipeline**: SpriteBatch with shader, `SpriteSortMode.Immediate` (required for per-draw shader changes)
- **BlendState**: Additive
- **SamplerState**: LinearWrap

### Architecture
- **Swing**: 150° arc over 24 frames, smoothstep easing
- **Smear**: SlashArc texture drawn centered on player, rotated to follow swing angle
- **Alpha envelope**: Quick fade-in (0→10%), sustain (10→85%), fade-out (85→100%)

### Rendering Pattern
```
SmearSwingProjectile.PreDraw:
  ├─ Layer 1: Smear arc overlay (SmearDistortShader)
  │   ├─ Sub-layer A: Wide outer glow (distort=0.08, scale×1.15, α×0.5)
  │   ├─ Sub-layer B: Main smear (distort=0.05, scale×1.0, α×0.8)
  │   └─ Sub-layer C: Bright core (distort=0.025, scale×0.85, α×0.65)
  │   ⚠ Uses SpriteSortMode.Immediate for per-draw parameter changes
  ├─ Layer 2: Tip glow (SoftGlow + StarFlare at blade tip)
  ├─ Layer 3: Root glow (SoftGlow at swing origin)
  └─ Layer 4: Blade sprite (Katana texture, normal AlphaBlend)
```

### Key Techniques
- **3-sub-layer distortion smear**: Same texture drawn 3 times with decreasing distortion strength and scale — creates depth from outer glow to core
- **Immediate sort mode**: `SpriteSortMode.Immediate` required so shader parameter changes take effect between consecutive `sb.Draw()` calls
- **Shader + fallback**: Code includes a non-shader fallback path (static colored layers) for when shader isn't available
- **Swing collision**: Line segment from player center to blade tip, `Collision.CheckAABBvLineCollision`
- **Player animation binding**: `owner.heldProj`, `owner.itemTime`, `owner.itemAnimation`, `owner.itemRotation`
- **Arc-based smear texture matching**: `(BladeLength * 2.4f) / maxDim` scales smear texture to match blade reach

### When to Use
Use for **melee weapon swing overlays** — the smear arc texture rotates with the swing and gets noise-distorted for an organic flowing energy feel. The 3-sub-layer pattern (outer/main/core with decreasing distortion) is the key technique for rich smear effects.

---

## 13. ThinLaserFoundation

**Effect Type**: Instant ricochet beam with per-segment VertexStrip rendering and bounce flares.

### Files
| File | Role |
|------|------|
| `ThinLaserFoundation.cs` | ModItem — fires beam on swing |
| `ThinLaserBeam.cs` | ModProjectile — ricochet path computation + per-segment rendering |
| *(Uses `LFTextures.cs` from LaserFoundation)* |

### Shaders

#### ThinBeamShader.fx
| Parameter | Type | Purpose |
|-----------|------|---------|
| `WorldViewProjection` | Matrix | Vertex transform |
| `onTex` | Texture2D | Shape mask (BeamAlphaMask) |
| `gradientTex` | Texture2D | Theme color ramp |
| `baseColor` | float3 | Base tint |
| `satPower` | float | Saturation curve |
| `sampleTexture1-2` | Texture2D | 2 detail textures |
| `grad1-2Speed` | float | Per-texture scroll speeds |
| `tex1-2Mult` | float | Per-texture brightness |
| `totalMult` | float | Overall brightness |
| `gradientReps` / `tex1-2reps` | float | UV repetition (proportional to segment length) |
| `uTime` | float | Time driver |

- **Pipeline**: VertexStrip (per-segment)
- **Simplified vs LaserFoundation**: 2 detail textures instead of 4

### Ricochet System
```csharp
ComputeRicochetPath(startPos, aimDir):
  for bounce = 0 to MaxBounces (3):
    Raycast along current direction (steps of 8px)
    If hit solid tile:
      Record hit position
      Compute reflection normal (GetReflectionNormal)
      Reflect direction: Vector2.Reflect(dir, normal)
      Nudge past surface (+4px)
    Else:
      Record endpoint at MaxSegmentLength (1200px)
```

### Rendering Pattern
```
ThinLaserBeam.PreDraw:
  ├─ Per-segment VertexStrip (ThinBeamShader):
  │   └─ Each segment drawn independently to avoid kink artifacts
  │       ├─ 2-position strip (segStart → segEnd)
  │       ├─ UV reps proportional to segment length / 1500
  │       └─ Alpha decreases per bounce (energy loss visual)
  ├─ Additive pass: Bounce flares
  │   ├─ Origin: GlowOrb
  │   ├─ Bounce points: GlowOrb + spinning StarFlare
  │   └─ Endpoint: GlowOrb (dimmer)
  └─ Restore AlphaBlend
```

### Key Techniques
- **Per-segment rendering**: Each ricochet segment is its own VertexStrip — avoids mesh kinking at bounce points
- **Tile-based raycast**: Steps every 8px, checks `Main.tile[x,y].HasTile && Main.tileSolid`
- **Reflection normal detection**: Compares tile coordinates of pre-hit and post-hit positions to determine surface orientation (horizontal wall vs vertical floor vs corner)
- **Energy loss visualization**: `segAlpha = alphaMultiplier * (1f - i * 0.15f)` — each bounce dims the beam
- **Shared shader uniforms**: Set once for all segments, only UV reps change per segment
- **Multi-segment collision**: `Colliding()` checks all segments including bounces
- **Instant beam**: Entire path computed on spawn, beam exists for only 25 frames

### When to Use
Use for **ricochet beams**, **bouncing lasers**, **reflected energy attacks**. The per-segment VertexStrip pattern and tile-based raycast/reflection system are the key techniques. Also useful as a simpler beam foundation (2 detail textures vs LaserFoundation's 4).

---

## 14. ThinSlashFoundation

**Effect Type**: Razor-thin slash line rendered at impact point via SDF shader.

### Files
| File | Role |
|------|------|
| `ThinSlashFoundation.cs` | ModItem — fires carrier projectile |
| `TSFTextures.cs` | Static texture cache (SoftCircle, SoftGlow, StarFlare, PointBloom, GlowOrb) |
| `ThinSlashProjectile.cs` | ModProjectile — homing carrier, spawns ThinSlashEffect on hit |
| `ThinSlashEffect.cs` | ModProjectile — the shader-rendered slash mark |

### Shaders

#### ThinSlashShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated + seed | Time driver |
| `slashAngle` | float | velocity angle | Line direction |
| `edgeColor` | float3 | theme[0] | Outermost color |
| `midColor` | float3 | theme[1] | Mid-range color |
| `coreColor` | float3 | theme[2] | Hot center color |
| `fadeAlpha` | float | 0→1→0 | Overall opacity |
| `lineWidth` | float | 0.018 | Line thickness (UV space) |
| `lineLength` | float | 0.45 | Line extent (UV space) |

- **Pipeline**: SpriteBatch with shader as Effect parameter
- **BlendState**: Additive
- **Technique**: SDF line math — extremely narrow, bright, crisp
- **Quad**: SoftCircle texture at scale 0.045

### Timing Envelope
| Phase | Frames | Alpha |
|-------|--------|-------|
| Fade in | 0–5 | 0→1 |
| Hold | 5–13 | 1.0 |
| Fade out | 13–35 | 1→0 |

### Rendering Pattern
```
ThinSlashEffect.PreDraw:
  ├─ Layer 1: ThinSlashShader on SoftCircle quad (very thin line)
  ├─ Layer 2: Directional bloom (3 SoftGlow draws along slash angle)
  │   ├─ Wide ambient (~50×4px)
  │   ├─ Main body (~45×2px)
  │   └─ Tight core (~30×1px)
  └─ Layer 3: Endpoint flashes (PointBloom + GlowOrb at line tips)
      └─ Center flash (StarFlare, first 8 frames only)
```

### Key Techniques
- **SDF line rendering**: Mathematical line computed in shader — perfectly thin at any resolution
- **Complementary bloom**: The shader line is extremely thin, so directional bloom sprites (stretched SoftGlow) provide the ambient light halo that sells the cut
- **Perpendicular spark dust**: Sparks fly perpendicular to slash direction — enhances the cutting feel
- **Carrier + effect pattern**: Homing projectile carries the slash to the enemy, then spawns the stationary effect
- **Tiny quad scale**: `ShaderDrawScale = 0.045f` on a 1024px SoftCircle = ~46px shader quad

### When to Use
Use for **thin precision cuts**, **sword strike marks**, **clean slash lines**, or any **razor-sharp linear impact**. Combine with SwordSmearFoundation for a weapon that shows both the swing arc (smear) and the impact mark (thin slash).

---

## 15. XSlashFoundation

**Effect Type**: Blazing X-shaped impact with fire-distorted texture and bloom.

### Files
| File | Role |
|------|------|
| `XSlashFoundation.cs` | ModItem — fires carrier projectile |
| `XSFTextures.cs` | Static texture cache (SoftGlow, StarFlare, PointBloom, XImpactCross, NoiseFBM) |
| `XSlashProjectile.cs` | ModProjectile — homing carrier, spawns XSlashEffect on hit |
| `XSlashEffect.cs` | ModProjectile — the shader-rendered blazing X |

### Shaders

#### XSlashShader.fx
| Parameter | Type | Default | Purpose |
|-----------|------|---------|---------|
| `uTime` | float | animated + seed | Time driver |
| `edgeColor` | float3 | theme[0] | Outer color |
| `midColor` | float3 | theme[1] | Mid color |
| `coreColor` | float3 | theme[2] | Hot center color |
| `fadeAlpha` | float | 0→1→0 | Overall opacity |
| `fireIntensity` | float | 0.06 | Distortion strength |
| `scrollSpeed` | float | 0.3 | UV scroll speed |
| `noiseTex` | Texture2D | NoiseFBM | Fire distortion source |
| `gradientTex` | Texture2D | per-style LUT | Color ramp |

- **Pipeline**: SpriteBatch with shader as Effect parameter
- **BlendState**: Additive
- **SamplerState**: LinearWrap
- **Technique**: Renders X-ShapedImpactCross.png with noise-based fire distortion, UV scrolling along both arms, gradient LUT coloring

### Timing Envelope
| Phase | Frames | Alpha |
|-------|--------|-------|
| Fade in | 0–6 | 0→1 (+ 30% scale punch) |
| Hold | 6–21 | 1.0 |
| Fade out | 21–46 | 1→0 |

### Rendering Pattern
```
XSlashEffect.PreDraw:
  ├─ Layer 1: Bloom foundation (SoftGlow × 2 scales)
  ├─ Layer 2: XSlashShader on XImpactCross texture
  │   └─ Rotated to impact angle, scale-punched on entrance
  ├─ Layer 3: Directional arm bloom
  │   ├─ Arm 1 at impactAngle + π/4 (SoftGlow stretched)
  │   ├─ Arm 2 at impactAngle - π/4 (SoftGlow stretched)
  │   └─ Tighter core lines on each arm
  └─ Layer 4: Center flash (StarFlare + cross StarFlare + PointBloom)
```

### Key Techniques
- **Texture + shader hybrid**: Uses an actual X-shaped texture (not SDF) — shader adds fire distortion and gradient coloring ON TOP of the base shape
- **Scale punch**: `1f + (1f - timer/FadeInFrames) * 0.3f` — slight scale-up on entrance for impact feel
- **Dual-arm bloom**: Elongated SoftGlow along both X arms (±45° from impact angle) for directional emphasis
- **Ember dust on X axes**: Particles fly along both diagonal arms of the X
- **Carrier + effect pattern**: Same as ThinSlash — homing carrier spawns stationary effect at impact

### When to Use
Use for **cross-slash marks**, **X-shaped impacts**, **dual-strike effects**. Good for weapons that deal two simultaneous hits or have crossing attack patterns. The texture + shader approach (applying distortion TO a shaped texture rather than computing the shape in SDF) is a different technique choice from ThinSlashFoundation.

---

## Cross-Reference: Rendering Pipeline Summary

### By Pipeline Type

| Pipeline | Foundations Using It |
|----------|---------------------|
| **SpriteBatch only (no shader)** | AttackAnimation, AttackFoundation, ExplosionParticles, RibbonFoundation, SmokeFoundation |
| **SpriteBatch + shader as Effect** | ImpactFoundation (Ripple, DamageZone, SlashMark), MaskFoundation, MagicOrb, SwordSmear, ThinSlash, XSlash, SparkleProjectile (body shader) |
| **VertexStrip + shader** | InfernalBeam, LaserFoundation, ThinLaser, SparkleProjectile (trail shader) |
| **Both VertexStrip + SpriteBatch shaders** | SparkleProjectile (only foundation using both in one projectile) |

### By SpriteBatch Technique

| Technique | Foundations Demonstrating It |
|-----------|------------------------------|
| **Multi-scale bloom stacking** | ALL foundations (universal technique) |
| **Velocity-stretched sprites** | AttackAnimation, AttackFoundation (Ranger), ExplosionParticles, RibbonFoundation, SparkleProjectile |
| **Spritesheet grid rendering** | SmokeFoundation |
| **Texture-strip trailing** | RibbonFoundation (modes 2-9) |
| **SpriteSortMode.Immediate** | SwordSmearFoundation (per-draw shader param changes) |

### By Shader Technique

| Shader Technique | Foundations / Shaders |
|-----------------|----------------------|
| **SDF shapes** | RippleShader (rings), SlashMarkShader (arc), ThinSlashShader (line), RadialNoiseMaskShader (circle) |
| **Noise distortion** | DamageZoneShader, SlashMarkShader, SmearDistortShader, XSlashShader, RadialNoiseMaskShader |
| **Gradient LUT coloring** | All shaders except FlareRainbow |
| **UV scrolling** | InfernalBeamBody, ConvergenceBeam, ThinBeam, SparkleTrail, RadialNoiseMask, XSlashShader |
| **Multi-texture compositing** | InfernalBeamBody (3 textures), ConvergenceBeam (4 textures), ThinBeam (2 textures) |
| **Polar coordinate conversion** | RadialNoiseMaskShader, RippleShader |
| **HSV color manipulation** | FlareRainbowShader, CrystalShimmerShader |
| **Procedural sparkle math** | SparkleTrailShader, CrystalShimmerShader |

### Shader Reuse Map

| Shader | Used By |
|--------|---------|
| `RadialNoiseMaskShader.fx` | MaskFoundation, MagicOrbFoundation, (potentially DamageZoneProjectile — similar but separate) |
| `ThinBeamShader.fx` | ThinLaserFoundation (simplified ConvergenceBeamShader) |
| `LFTextures` | LaserFoundation, ThinLaserFoundation (shared texture cache) |

---

## Quick Selection Guide

> "I need to implement [X effect]. Which foundation should I start from?"

| Effect Need | Primary Foundation | Combine With |
|-------------|-------------------|--------------|
| **Melee swing arc** | SwordSmearFoundation | ThinSlash or XSlash for impact marks |
| **Melee combo** | AttackFoundation (ComboSwing) | ExplosionParticles for hit effects |
| **Thrown weapon** | AttackFoundation (ThrowSlam) | ImpactFoundation for landing |
| **Channeled beam** | LaserFoundation (rich) or InfernalBeam (spinning ring) | — |
| **Instant ricochet beam** | ThinLaserFoundation | ImpactFoundation at bounce points |
| **Orbiting attack** | AttackFoundation (FlamingRing or Astralgraph) | — |
| **Homing projectile** | MaskFoundation (shader orb) or MagicOrb (orb + bolts) | RibbonFoundation for trail |
| **Sparkle/crystal projectile** | SparkleProjectileFoundation | — |
| **Projectile trail** | RibbonFoundation (10 modes) | — |
| **Impact explosion** | ImpactFoundation (shader) + ExplosionParticles (sparks) | SmokeFoundation for debris |
| **Thin cut mark** | ThinSlashFoundation | — |
| **X-slash mark** | XSlashFoundation | — |
| **Smoke/explosion cloud** | SmokeFoundation | ExplosionParticles for sparks |
| **Ranged tracer** | AttackFoundation (RangerShot) | ImpactFoundation at target |
| **Cinematic attack** | AttackAnimationFoundation | — |
| **Summoner ring/arena** | AttackFoundation (FlamingRing) | — |
| **Circular aura/orb** | MaskFoundation (RadialNoiseMaskShader) | — |
| **Persistent damage zone** | ImpactFoundation (DamageZone) | SmokeFoundation overlay |
