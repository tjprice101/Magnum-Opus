# Foundation Weapons — Complete VFX Survey

> Comprehensive technical survey of all 15 Foundation weapon systems in `Content/FoundationWeapons/`.
> Each entry documents: rendering architecture, shaders used, blend states, trail construction, bloom techniques, and reusable patterns.

---

## Table of Contents

1. [AttackAnimationFoundation](#1-attackanimationfoundation)
2. [AttackFoundation](#2-attackfoundation)
3. [ExplosionParticlesFoundation](#3-explosionparticlesfoundation)
4. [ImpactFoundation](#4-impactfoundation)
5. [InfernalBeamFoundation](#5-infernalbeamfoundation)
6. [LaserFoundation](#6-laserfoundation)
7. [MagicOrbFoundation](#7-magicorbfoundation)
8. [MaskFoundation](#8-maskfoundation)
9. [RibbonFoundation](#9-ribbonfoundation)
10. [SmokeFoundation](#10-smokefoundation)
11. [SparkleProjectileFoundation](#11-sparkleprojectilefoundation)
12. [SwordSmearFoundation](#12-swordsmeerfoundation)
13. [ThinLaserFoundation](#13-thinlaserfoundation)
14. [ThinSlashFoundation](#14-thinslashfoundation)
15. [XSlashFoundation](#15-xslashfoundation)
16. [Cross-Cutting Reusable Infrastructure](#cross-cutting-reusable-infrastructure)

---

## 1. AttackAnimationFoundation

**Purpose:** Cinematic multi-slash melee attack animation with camera control, screen shake, and progressive post-processing.

**Files:** `AttackAnimationFoundation.cs`, `AttackAnimationProjectile.cs` (411 lines), `SlashVFXProjectile.cs`, `AAFPlayer.cs` (391 lines), `AAFTextures.cs`

### Shaders
- **None.** All rendering is CPU-side SpriteBatch.

### Rendering Architecture (PreDraw)
```
sb.End() → sb.Begin(Additive, LinearClamp) → [motion blur trails + slash lines] → sb.End() → sb.Begin(AlphaBlend) restore
```

### VFX Layers
| Layer | Technique | Textures |
|-------|-----------|----------|
| Motion blur trails | Stretched SoftGlow along velocity direction | SoftGlow (1024px) |
| Slash lines | Velocity-direction slash streaks | SoftGlow, StarFlare |
| Impact flashes | Multi-layer bloom stacking (outer/mid/core) | SoftGlow, StarFlare |
| Final slash bloom | Expanding radial bloom | SoftRadialBloom, LensFlare |

### Unique Systems
- **AAFPlayer (ModPlayer):** Camera pan, screen shake, progressive blur/brightness, black-and-white impact frame
- **Color arrays:** `SlashColors[3]` (ice-blue outer → blue-white mid → white core), `ZoneColors[3]`

### Key Technique
Stretched SoftGlow textures along velocity vectors for motion blur — no shader needed, just rotation + anisotropic scale.

---

## 2. AttackFoundation

**Purpose:** 5-mode weapon demonstrating different projectile attack patterns: ThrowSlam, ComboSwing, Astralgraph, FlamingRing, RangerShot.

**Files:** `AttackFoundation.cs`, `ThrowSlamProjectile.cs`, `ComboSwingProjectile.cs`, `AstralgraphProjectile.cs`, `FlamingRingProjectile.cs`, `RangerShotProjectile.cs`, `AFTextures.cs`

### Shaders
- **None.** All CPU-side additive bloom.

### Rendering Architecture (PreDraw — consistent across all 5 projectiles)
```
sb.End() → sb.Begin(Additive) → [outer/mid/core bloom layers] → sb.End() → sb.Begin(AlphaBlend) restore
```

### VFX Layers (per projectile type)

| Projectile | Key VFX | Special Technique |
|------------|---------|-------------------|
| ThrowSlam | 3-layer bloom halo | Standard bloom stacking |
| ComboSwing | 3-layer bloom halo | Standard bloom stacking |
| Astralgraph | Star polygon + connecting glow lines | **`DrawGlowLine()`** — stretched SoftGlow between two points |
| FlamingRing | Orbiting glow orbs + arcs between them | **`DrawFireArc()`** + PowerEffectRing ring outline |
| RangerShot | Ring-buffer trail (8 positions) + velocity streak + muzzle flash | Afterimage bloom trail with progressive fade |

### Key Reusable Methods
- **`DrawGlowLine(Vector2 a, Vector2 b)`** — Draws a line between two screen-space points using a stretched SoftGlow texture. Computes distance, midpoint, rotation, and scale automatically.
- **`AttackMode` enum** with `GetModeColors()` and `GetModeName()` pattern.

### Texture Registry (AFTextures)
Includes **gradient LUT textures** per theme (Moonlight, Eroica, LaCampanella, Fate, Enigma), **noise textures** (Perlin, FBM, CosmicVortex, MusicalWave, StarField), standard bloom set, and `PowerEffectRing`.

---

## 3. ExplosionParticlesFoundation

**Purpose:** CPU-managed spark particle explosion system with multiple burst patterns.

**Files:** `ExplosionParticlesFoundation.cs`, `SparkCarrierProjectile.cs`, `SparkExplosionProjectile.cs` (497 lines), `EPFTextures.cs`

### Shaders
- **None.** All CPU-side SpriteBatch.

### Rendering Architecture
```
sb.End() → sb.Begin(Additive, LinearClamp) → [55 Spark structs drawn per frame] → [central flash] → sb.End() → sb.Begin(AlphaBlend) restore
```

### Particle System
- **55 internal `Spark` structs** managed manually (no external particle system)
- Each has: Position, Velocity, Rotation, Scale, Alpha, Gravity, Friction
- 3 spark patterns: **RadialScatter**, **FountainCascade**, **SpiralShrapnel** — each varies velocity distribution, gravity, spin
- 3 spark visual types: 0=line (elongated SoftGlow), 1=star (StarFlare), 2=dot (PointBloom)

### VFX Layers
| Layer | Technique |
|-------|-----------|
| Spark bodies | Stretched SoftGlow/StarFlare/PointBloom per spark type |
| Central flash | SoftGlow + StarFlare + LensFlare with quadratic alpha falloff |

### Key Technique
`SamplerState.LinearClamp` used explicitly (instead of default) for the additive pass. Spark carrier projectile uses the standard outer glow + core glow + velocity streak pattern.

---

## 4. ImpactFoundation

**Purpose:** 3-mode impact effects (Ripple, DamageZone, SlashMark) — demonstrates SDF-driven shader effects at impact points.

**Files:** `ImpactFoundation.cs`, `ImpactProjectile.cs`, `RippleEffectProjectile.cs`, `DamageZoneProjectile.cs`, `SlashMarkProjectile.cs`, `IFTextures.cs`, `Shaders/`

### Shaders (3 custom)

| Shader | Path | Technique |
|--------|------|-----------|
| **RippleShader** | `ImpactFoundation/Shaders/RippleShader` | Concentric expanding rings via SDF math + noise distortion |
| **DamageZoneShader** | `ImpactFoundation/Shaders/DamageZoneShader` | Radial noise pattern masked to soft circle with breathing pulse |
| **SlashMarkShader** | `ImpactFoundation/Shaders/SlashMarkShader` | SDF-based directional slash arc with noise distortion for organic edges |

### Shader Parameter Catalog

**RippleShader:**
`uTime`, `progress`, `ringCount`(4), `ringThickness`(0.04), `primaryColor`, `secondaryColor`, `coreColor`, `fadeAlpha`, `noiseTex`

**DamageZoneShader:**
`uTime`, `scrollSpeed`, `rotationSpeed`, `circleRadius`(0.44), `edgeSoftness`(0.06), `intensity`(1.8), `primaryColor`, `coreColor`, `fadeAlpha`, `breathe`, `noiseTex`(NoiseFBM), `gradientTex`

**SlashMarkShader:**
`uTime`, `slashAngle`, `primaryColor`, `coreColor`, `fadeAlpha`, `slashWidth`(0.06), `slashLength`(0.35), `noiseTex`(NoiseCosmicVortex), `gradientTex`

### Rendering Architecture
```
sb.End() → sb.Begin(Additive, LinearWrap, shaderEffect) → draw SoftCircle quad → sb.End() → sb.Begin(AlphaBlend) restore
```

### Key Pattern
All three shaders draw onto a **SoftCircle quad** with `SamplerState.LinearWrap` + `BlendState.Additive`. The shader receives the quad's UVs and computes all visual detail procedurally. The `IFTextures.GetGradientForMode()` method returns mode-specific gradient LUTs.

---

## 5. InfernalBeamFoundation

**Purpose:** Channeled beam with spinning ring at origin and flares at endpoint.

**Files:** `InfernalBeamFoundation.cs`, `InfernalBeam.cs`, `IBFTextures.cs`, `Shaders/`

### Shaders (1 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **InfernalBeamBodyShader** | `InfernalBeamFoundation/Shaders/InfernalBeamBodyShader` | VertexStrip |

### Shader Parameters
`WorldViewProjection`, `onTex`(BeamAlphaMask), `gradientTex`, `bodyTex`(SoundWaveBeam), `detailTex1`(EnergyMotion), `detailTex2`(EnergySurge), `noiseTex`(NoiseFBM), `bodyReps`, `detail1Reps`, `detail2Reps`, `gradientReps`, `bodyScrollSpeed`, `detail1ScrollSpeed`, `detail2ScrollSpeed`, `noiseDistortion`, `totalMult`, `uTime`

**Pass name:** `"MainPS"` — followed by `Main.pixelShader.CurrentTechnique.Passes[0].Apply()` to reset.

### Rendering Architecture
```
1. VertexStrip.PrepareStrip() → shader.Passes["MainPS"].Apply() → strip.DrawTrail()
2. Reset pixel shader
3. sb.Begin(Additive) → [origin ring layers] → [endpoint flares] → sb.Begin(AlphaBlend) restore
```

### VFX Layers
| Layer | Technique | Textures |
|-------|-----------|----------|
| Beam body | VertexStrip + InfernalBeamBodyShader | BeamAlphaMask, SoundWaveBeam, EnergyMotion, EnergySurge, NoiseFBM |
| Origin ring | Multiple spinning sprite layers at different rotations/scales | InfernalBeamRing, PointBloom |
| Endpoint flares | Stacked additive sprites spinning at endpoint | StarFlare, GlowOrb, LensFlare |

### Key Pattern
**VertexStrip beam pipeline:** Build 2-position strip (start/end), configure shader uniforms, apply named pass, draw, reset pixel shader. Theme-specific via `GetDustColorsForTheme()` and `GetGradientForTheme()` per `InfernalBeamTheme`.

---

## 6. LaserFoundation

**Purpose:** Convergence beam — the most complex beam rendering system. Demonstrates multi-texture scrolling beam + rainbow endpoint flares.

**Files:** `LaserFoundation.cs`, `LaserFoundationBeam.cs` (600 lines), `LFEasings.cs`, `LFTextures.cs`, `Shaders/`

### Shaders (2 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **ConvergenceBeamShader** | `LaserFoundation/Shaders/ConvergenceBeamShader` | VertexStrip |
| **FlareRainbowShader** | `LaserFoundation/Shaders/FlareRainbowShader` | SpriteBatch Effect |

### Shader Parameters

**ConvergenceBeamShader:**
`WorldViewProjection`, `onTex`(BeamAlphaMask), `gradientTex`, `baseColor`, `satPower`, `sampleTexture1-4`(DetailThinGlowLine, DetailSpark, DetailExtra, DetailTrailLoop), `grad1-4Speed`, `tex1-4Mult`, `totalMult`, `gradientReps`, `tex1-4reps`, `uTime`
**Pass name:** `"MainPS"`

**FlareRainbowShader:**
`rotation`, `rainbowRotation`, `intensity`, `fadeStrength`
Applied as SpriteBatch Effect parameter — converts white flare sprites into spinning radial rainbow flares.

### Rendering Architecture
```
1. VertexStrip beam body: strip.PrepareStrip() → ConvergenceBeamShader.Passes["MainPS"].Apply() → strip.DrawTrail() → reset pixel shader
2. sb.Begin(Additive, EffectMatrix, FlareRainbowShader) → [all endpoint flare sprites get rainbow treatment] → restore
```

### Key Technique
- Uses `Main.GameViewMatrix.EffectMatrix` (not `TransformationMatrix`) for the flare SpriteBatch — important distinction for shader-driven sprite batches.
- **FlareRainbowShader** applies as a batch-level Effect, so ALL sprites drawn in that batch get the rainbow treatment automatically.
- `BeamTheme` enum with `GetDustColorsForTheme()` and `GetGradientForTheme()`.
- Custom textures: DetailThinGlowLine, DetailSpark, DetailExtra, DetailTrailLoop, BeamAlphaMask.

---

## 7. MagicOrbFoundation

**Purpose:** Floating noise orb that fires bloom-trail bolts. Demonstrates shader reuse across foundations.

**Files:** `MagicOrbFoundation.cs`, `MagicOrb.cs`, `OrbBolt.cs`, `MOFTextures.cs`

### Shaders (1 reused)

| Shader | Source | Pipeline |
|--------|--------|----------|
| **RadialNoiseMaskShader** | `MaskFoundation/Shaders/RadialNoiseMaskShader` | SpriteBatch Effect |

### Rendering Architecture — MagicOrb
```
sb.Begin(Additive) → [bloom halo: outer/mid/inner SoftGlow] → sb.End()
sb.Begin(Additive, LinearWrap, RadialNoiseMaskShader) → [SoftCircle quad] → sb.End()
sb.Begin(Additive) → [core bloom: GlowOrb] → sb.End() → sb.Begin(AlphaBlend) restore
```

### Rendering Architecture — OrbBolt (NO shader)
```
sb.Begin(Additive) → [5-layer bloom stacking] → sb.Begin(AlphaBlend) restore
```

**OrbBolt's 5 layers:**
1. Outer SoftGlow (dim, large)
2. Mid SoftGlow (brighter, medium)
3. Core GlowOrb (intense, small)
4. StarFlare aligned to velocity (directional gleam)
5. StarFlare perpendicular (cross flare)

### Key Pattern
- **Cross-foundation shader reuse:** MagicOrb loads `RadialNoiseMaskShader` from MaskFoundation.
- Two spawn modes: Normal (slow, fires bolts) vs Burst (fast, explodes into 6 bolts).
- `OrbNoiseStyle` enum with `GetStyleColors()`, `GetNoiseForStyle()`, `GetGradientForStyle()`.

---

## 8. MaskFoundation

**Purpose:** Origin of the RadialNoiseMaskShader — the most reused shader in the Foundation system.

**Files:** `MaskFoundation.cs`, `MaskOrbProjectile.cs`, `MFTextures.cs`, `Shaders/`

### Shaders (1 custom — reused by others)

| Shader | Path | Reused By |
|--------|------|-----------|
| **RadialNoiseMaskShader** | `MaskFoundation/Shaders/RadialNoiseMaskShader` | MagicOrbFoundation, ImpactFoundation (DamageZone) |

### Shader Parameters
`uTime`, `scrollSpeed`(0.3), `rotationSpeed`(0.15), `circleRadius`(0.45), `edgeSoftness`(0.08), `intensity`(2.0), `primaryColor`, `coreColor`, `noiseTex`, `gradientTex`

### How It Works
1. Converts sprite UVs to polar coordinates
2. Scrolls noise texture radially over time
3. Maps noise intensity through a gradient LUT for theme coloring
4. Masks result to a soft circle with configurable radius and edge softness

### Rendering Architecture
```
sb.Begin(Additive) → [bloom halo: outer/mid/inner SoftGlow] → sb.End()
sb.Begin(Additive, LinearWrap, RadialNoiseMaskShader) → [SoftCircle quad] → sb.End()
sb.Begin(Additive) → [core bloom: GlowOrb] → sb.End() → sb.Begin(AlphaBlend) restore
```

### Key Pattern
- `NoiseMode` enum with `GetModeColors()`, `GetNoiseForMode()`, `GetGradientForMode()`.
- 3-layer rendering: **bloom halo → shader orb → core bloom**.

---

## 9. RibbonFoundation

**Purpose:** 10-mode ribbon trail system — the most comprehensive trail foundation. CPU-side rendering with multiple trail techniques.

**Files:** `RibbonFoundation.cs`, `RibbonProjectile.cs` (610 lines), `RBFTextures.cs`

### Shaders
- **None.** All CPU-side SpriteBatch rendering.

### Trail System
- **Ring buffer:** 40 positions, `extraUpdates = 1` for double density
- Positions recorded each AI tick into circular buffer

### 10 Ribbon Modes

| Mode | Technique | Key Feature |
|------|-----------|-------------|
| **PureBloom** | Overlapping bloom sprites (SoftGlowBright + PointBloom) | 3 layers per point, velocity-stretched |
| **BloomNoiseFade** | Like PureBloom + CPU-side noise erosion | `sin()`-based approximation for organic tail breakup |
| **BasicTrail** | UV-mapped texture strip | `DrawTextureStripRibbon()` generic renderer |
| **HarmonicWave** | UV-mapped strip | HarmonicWaveRibbon texture |
| **SpiralingVortex** | UV-mapped strip | SpiralingVortexStrip texture |
| **EnergySurge** | UV-mapped strip | EnergySurgeBeam texture |
| **CosmicNebula** | UV-mapped strip | CosmicNebulaClouds texture |
| **MusicalWave** | UV-mapped strip | MusicalWavePattern texture |
| **TileableMarble** | UV-mapped strip | TileableMarbleNoise texture |
| **LightningRibbon** | UV-mapped strip + aggressive bloom + random jitter | LightningSurge texture with electric feel |

### Key Reusable Methods
- **`DrawTextureStripRibbon(positions, colors, texture, lifeFade, intensity)`** — Generic texture-strip renderer. Takes ANY texture and draws it as UV-mapped ribbon along position history using source rectangles that sample sequential horizontal slices.
- **`DrawRibbonBloomOverlay(positions, colors, lifeFade, intensity)`** — Shared bloom overlay for all texture strip modes. Draws ~15 evenly-spaced bloom sprites along the trail.
- **`DrawHeadBloom()`** — 4-layer bloom at projectile head (outer/mid/inner SoftGlow + StarFlare cross).
- **`DrawOrbBody()`** — Pulsating Music Note Orb texture scaled to ~36px.

### Rendering Architecture
```
sb.End() → sb.Begin(Additive) → [ribbon body via mode-specific method] → [bloom overlay] → [head bloom] → sb.End()
sb.Begin(AlphaBlend) → [orb body] → restore
```

---

## 10. SmokeFoundation

**Purpose:** Smoke cloud explosion system — demonstrates spritesheet-based particle rendering.

**Files:** `SmokeFoundation.cs`, `SmokeCarrierProjectile.cs`, `SmokeRingProjectile.cs`, `SKFTextures.cs`

### Shaders
- **None.** All CPU-side SpriteBatch.

### Particle System
- **30 `SmokePuff` structs** — self-contained particle pool
- **3×6 smoke spritesheet grid** — each puff randomly picks a frame via `SKFTextures.GetFrameRect()`
- Lifecycle modeled on Calamity's `HeavySmokeParticle`:
  - First 20%: Scale grows +0.01/frame (expansion)
  - After 20%: Scale shrinks ×0.975/frame (dissipation)
  - Opacity decays ×0.98/frame throughout
  - Velocity decays ×0.85/frame (heavy drag)
  - Last 15%: Additional rapid alpha fade
  - Color shifts from core (hot) → body → edge (cool) over lifetime

### Rendering Architecture
```
sb.End() → sb.Begin(Additive, LinearClamp) → [smoke puff bodies from spritesheet] → sb.End()
sb.Begin(Additive, LinearClamp) → [center flash + puff glow accents] → sb.End()
sb.Begin(AlphaBlend) restore
```

### Key Techniques
- **Additive blend for smoke** — because textures are bright shapes on black backgrounds (black becomes invisible)
- **Random flip:** `SpriteEffects.FlipHorizontally | FlipVertically` for visual variety
- Calamity-style ring pattern: `new Vector2(15, 15).RotatedByRandom(100) * rand(0.8, 1.6)` (same vector for offset AND velocity)
- `RenderScale = 0.3f` — frame pixels are ~142×157px, scaled down for gameplay

---

## 11. SparkleProjectileFoundation

**Purpose:** Homing crystal shards with shader-driven glitter trail and prismatic shimmer body. The most shader-dense projectile system.

**Files:** `SparkleProjectileFoundation.cs`, `SparkleCrystalProjectile.cs` (702 lines), `SPFTextures.cs`, `Shaders/`

### Shaders (2 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **SparkleTrailShader** | `SparkleProjectileFoundation/Shaders/SparkleTrailShader` | VertexStrip |
| **CrystalShimmerShader** | `SparkleProjectileFoundation/Shaders/CrystalShimmerShader` | SpriteBatch Effect |

### Shader Parameters

**SparkleTrailShader:**
`WorldViewProjection`, `uTime`, `sparkleTex`(4PointedStarHard), `gradientTex`, `glowMaskTex`(SoftCircle), `coreColor`, `outerColor`, `trailIntensity`(1.5), `sparkleSpeed`(1.2), `sparkleScale`(3.0), `glitterDensity`(2.5), `tipFadeStart`(0.7), `edgeSoftness`(0.4)
**Pass name:** `"SparkleTrailPass"`

**CrystalShimmerShader:**
`uTime`, `rotation`, `shimmerSpeed`(2.0), `flashIntensity`(1.5), `baseAlpha`, `primaryColor`, `highlightColor`, `gradientTex`
Applied as SpriteBatch Effect for crystal body overlay.

### Rendering Architecture (5 layers)
```
1. sb.End() → VertexStrip trail (SparkleTrailShader) → reset pixel shader → sb.Begin(AlphaBlend)
2. sb.End() → sb.Begin(Additive) → [Photoviscerator-style bloom trail: 3 layers per point] 
3. [Crystal glow halo: outer/mid/inner SoftGlow+SoftRadialBloom]
4. [Crystal body sprites: star body + white-hot core + overlay] → sb.End()
   → sb.Begin(Additive, CrystalShimmerShader) → [shimmer overlay] → sb.End() → sb.Begin(Additive)
5. sb.End() → sb.Begin(Additive) → [4 orbiting sparkle accents with cubic sin-wave flash] → restore
```

### Key Techniques
- **Photoviscerator-style bloom trail:** Velocity-stretched, fading bloom sprites at historical positions. 3 layers per point (wide outer glow, main body, tight hot core). White-hot at head → theme-colored at tail.
- **Crystal shimmer:** 6-facet angular shimmer via `sin(angle * 6)`, HSV prismatic color at facet boundaries, `pow(16)` sparkle peaks.
- **Orbiting sparkle accents:** 4 points with cubic sin-wave flash timing + central twinkle with `pow(6)` peaks.
- Ring buffer: 24 positions, `extraUpdates = 1`.
- Named shader pass: `"SparkleTrailPass"` (not `"MainPS"` like beams).

---

## 12. SwordSmearFoundation

**Purpose:** Melee sword swing with slash arc texture overlay + shader-driven distortion.

**Files:** `SwordSmearFoundation.cs`, `SmearSwingProjectile.cs`, `SMFTextures.cs`, `Shaders/`

### Shaders (1 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **SmearDistortShader** | (loaded via `SMFTextures.SmearDistortShader`) | SpriteBatch Effect |

### Shader Parameters
`uTime`, `fadeAlpha`, `distortStrength`(0.05-0.08), `flowSpeed`(0.4), `noiseScale`(2.5), `noiseTex`(FBMNoise), `gradientTex`

### Rendering Architecture (4 layers)
```
1. sb.End() → sb.Begin(Immediate, Additive, LinearWrap, EffectMatrix):
   - Sub-layer A: Wide outer glow (distortStrength=0.08, scale×1.15, alpha×0.5)
   - Sub-layer B: Main smear (distortStrength=0.05, scale×1.0, alpha×0.8)  
   - Sub-layer C: Bright core (distortStrength=0.025, scale×0.85, alpha×0.65)
   → sb.End()
2. sb.Begin(Additive, EffectMatrix) → [tip glow: SoftGlow + StarFlare at blade tip]
3. [root glow: SoftGlow at swing origin]
   → sb.End() → sb.Begin(AlphaBlend, TransformationMatrix)
4. [blade sprite: vanilla Katana texture rotated to swing angle]
```

### Key Techniques
- **3 sub-layers of the same smear texture** at different distortion strengths and scales — creates depth (outer glow → main → core).
- **Shader fallback:** If shader is null, falls back to static colored layers without distortion.
- Uses `Main.GameViewMatrix.EffectMatrix` for shader passes, `TransformationMatrix` for blade sprite.
- Smoothstep-eased swing: `progress² × (3 - 2×progress)`.
- Fade envelope: fade in first 10%, sustain, fade out last 15%.

---

## 13. ThinLaserFoundation

**Purpose:** Thin ricocheting beam that bounces off tiles up to 3 times. Each segment drawn independently.

**Files:** `ThinLaserFoundation.cs`, `ThinLaserBeam.cs` (513 lines), `Shaders/`

### Shaders (1 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **ThinBeamShader** | `ThinLaserFoundation/Shaders/ThinBeamShader` | VertexStrip |

### Shader Parameters
`WorldViewProjection`, `onTex`(BeamAlphaMask), `gradientTex`, `baseColor`, `satPower`(0.85), `sampleTexture1`(DetailThinGlowLine), `sampleTexture2`(DetailSpark), `grad1Speed`(0.9), `grad2Speed`(1.3), `tex1Mult`(1.5), `tex2Mult`(1.8), `totalMult`(1.0), `gradientReps`, `tex1reps`, `tex2reps`, `uTime`
**Pass name:** `"MainPS"`

### Rendering Architecture
```
For each beam segment (up to 4):
  VertexStrip(2 positions) → ThinBeamShader.Passes["MainPS"].Apply() → strip.DrawTrail()
Reset pixel shader
sb.End() → sb.Begin(Additive) → [bounce flares: GlowOrb + StarFlare at each node] → restore
```

### Key Techniques
- **Per-segment rendering:** Each beam segment (origin → bounce → bounce → endpoint) is drawn as an independent VertexStrip to avoid kinking at bounces.
- **Ricochet system:** Manual raycasting at 8px step intervals, `Vector2.Reflect()` for bounce direction, surface normal computation from tile grid.
- **UV repetition scales with segment length:** `repVal = segLen / 1500f` — longer segments get more texture repetitions.
- **Alpha decreases per bounce:** `segAlpha = alphaMultiplier * (1 - i * 0.15)` — beam loses energy with each ricochet.
- Reuses `LFTextures` from LaserFoundation (BeamAlphaMask, DetailThinGlowLine, DetailSpark, gradient LUTs).

---

## 14. ThinSlashFoundation

**Purpose:** Razor-thin slash line impact effect using SDF shader math.

**Files:** `ThinSlashFoundation.cs`, `ThinSlashProjectile.cs`, `ThinSlashEffect.cs`, `TSFTextures.cs`, `Shaders/`

### Shaders (1 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **ThinSlashShader** | `ThinSlashFoundation/Shaders/ThinSlashShader` | SpriteBatch Effect |

### Shader Parameters
`uTime`, `slashAngle`, `edgeColor`, `midColor`, `coreColor`, `fadeAlpha`, `lineWidth`(0.018), `lineLength`(0.45)

### Rendering Architecture
```
sb.End() → sb.Begin(Additive, LinearWrap, ThinSlashShader) → [SoftCircle quad] → sb.End()
sb.Begin(Additive) → [directional bloom: 3 elongated SoftGlow layers along slash] → [endpoint flashes: PointBloom + GlowOrb at tips] → [center StarFlare] → restore
```

### VFX Layers
| Layer | Technique |
|-------|-----------|
| Thin slash line | SDF math in shader — extremely narrow, bright, crisp |
| Directional bloom | Anisotropic SoftGlow: `scale = (0.045, 0.002)` along slash direction |
| Endpoint flashes | Small PointBloom + GlowOrb at both slash tips |
| Center flash | StarFlare at intersection, 18-frame quadratic fade |
| Spark dust | Perpendicular sparks on either side of slash |

### Key Techniques
- **SDF-based line rendering** — no texture strip, pure math: `lineWidth` and `lineLength` in normalized UV space.
- **Anisotropic stretching** of SoftGlow for directional bloom: very wide along slash, extremely narrow perpendicular.
- Timing: 5-frame fade in, 8-frame hold, 22-frame fade out (total 35 frames — instant and sharp).
- Carrier projectile (`ThinSlashProjectile`) uses standard homing + bloom pattern, spawns `ThinSlashEffect` on hit.

---

## 15. XSlashFoundation

**Purpose:** Blazing X-shaped impact cross with fire distortion shader.

**Files:** `XSlashFoundation.cs`, `XSlashProjectile.cs`, `XSlashEffect.cs`, `XSFTextures.cs`, `Shaders/`

### Shaders (1 custom)

| Shader | Path | Pipeline |
|--------|------|----------|
| **XSlashShader** | `XSlashFoundation/Shaders/XSlashShader` | SpriteBatch Effect |

### Shader Parameters
`uTime`, `edgeColor`, `midColor`, `coreColor`, `fadeAlpha`, `fireIntensity`(0.06), `scrollSpeed`(0.3), `noiseTex`(NoiseFBM), `gradientTex`

### Rendering Architecture
```
sb.End() → sb.Begin(Additive) → [bloom foundation: 2-layer SoftGlow]
sb.End() → sb.Begin(Additive, LinearWrap, XSlashShader) → [X-ShapedImpactCross texture] → sb.End()
sb.Begin(Additive) → [arm bloom: elongated SoftGlow along both X arms] → [center flash: StarFlare + PointBloom] → restore
```

### VFX Layers
| Layer | Technique |
|-------|-----------|
| Bloom foundation | 2-scale SoftGlow (wide ambient + mid glow) |
| Blazing X | X-ShapedImpactCross texture + XSlashShader (noise fire + UV scroll + gradient LUT) |
| Arm bloom | Anisotropic SoftGlow along both X arms (±45° from impact angle) |
| Center flash | StarFlare starburst + cross flare + PointBloom, 18-frame fade |
| Ember dust | Sparks along both diagonal axes |

### Key Techniques
- **Texture-driven impact shape:** Unlike ThinSlash's pure SDF, XSlash applies shader effects TO an existing X-shaped texture (`X-ShapedImpactCross`).
- **Scale-up entrance:** During fade-in, scale is boosted by up to 30% for a punchy appearance: `scaleBoost = 1 + (1 - fadeProgress) * 0.3`.
- Timing: 6-frame fade in, 15-frame hold, 25-frame fade out (total 46 frames).
- Carrier projectile (`XSlashProjectile`) uses standard homing + bloom pattern, spawns `XSlashEffect` on hit.

---

## Cross-Cutting Reusable Infrastructure

### Universal SpriteBatch Swap Pattern
Every Foundation weapon uses the same pattern for additive rendering:
```csharp
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
    Main.DefaultSamplerState, DepthStencilState.None,
    RasterizerState.CullCounterClockwise, null,
    Main.GameViewMatrix.TransformationMatrix);
// ... draw additive layers ...
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
    Main.DefaultSamplerState, DepthStencilState.None,
    RasterizerState.CullCounterClockwise, null,
    Main.GameViewMatrix.TransformationMatrix);
```

**Variant for shaders:** Use `SamplerState.LinearWrap` and pass the Effect:
```csharp
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive,
    SamplerState.LinearWrap, DepthStencilState.None,
    RasterizerState.CullCounterClockwise, shaderEffect,
    Main.GameViewMatrix.TransformationMatrix);
```

**Variant for EffectMatrix:** Shader sprite batches that need GPU transformation use `Main.GameViewMatrix.EffectMatrix` instead of `TransformationMatrix`.

### Multi-Layer Bloom Stacking (Universal)
Nearly every Foundation uses this pattern:
```csharp
// Outer (dim, large) → Mid (brighter, medium) → Core (intense, small)
sb.Draw(softGlow, pos, null, outerColor * 0.2f, 0f, origin, 0.2f, ...);
sb.Draw(softGlow, pos, null, midColor * 0.4f, 0f, origin, 0.08f, ...);
sb.Draw(softGlow, pos, null, coreColor * 0.6f, 0f, origin, 0.03f, ...);
```

### VertexStrip Beam Pipeline (3 foundations)
Used by InfernalBeam, LaserFoundation, ThinLaser, and SparkleProjectile:
```csharp
VertexStrip strip = new VertexStrip();
strip.PrepareStrip(positions, rotations, StripColor, StripWidth, -Main.screenPosition, includeBacksides: true);
shader.Parameters["WorldViewProjection"].SetValue(Main.GameViewMatrix.NormalizedTransformationmatrix);
// ... configure other uniforms ...
shader.CurrentTechnique.Passes["MainPS"].Apply();
strip.DrawTrail();
Main.pixelShader.CurrentTechnique.Passes[0].Apply(); // RESET
```

### Texture Registry Pattern
Every Foundation has its own Textures class (e.g., `AFTextures`, `LFTextures`, `SPFTextures`) with:
- `Asset<Texture2D>` fields for each texture (lazy-loaded via `ModContent.Request`)
- `GetStyleColors(style)` / `GetModeColors(mode)` → `Color[]` array for theme coloring
- `GetGradientForStyle(style)` / `GetGradientForTheme(theme)` → Gradient LUT `Texture2D`
- `GetNoiseForStyle(style)` / `GetNoiseForMode(mode)` → Noise `Texture2D`

### Common VFX Textures (from VFX Asset Library)

| Texture | Size | Primary Use |
|---------|------|-------------|
| SoftGlow | 1024px | Universal bloom sprite, motion blur via stretching |
| SoftGlowBright | ~1024px | Brighter bloom variant for trails |
| GlowOrb | 1024px | Tight circular glow, core bloom |
| StarFlare | 1024px | 4-pointed star gleam, cross flares |
| PointBloom | 2160px | Intense point light, very sharp |
| SoftRadialBloom | varies | Softer radial variant |
| LensFlare | varies | Cinematic lens flare accent |
| SoftCircle | varies | **Quad texture for shader rendering** (shader draws onto this) |
| HardCircleMask | varies | Sharp-edged circle mask |
| PowerEffectRing | varies | Ring outline for orbital effects |
| BeamAlphaMask | varies | Alpha mask for beam VertexStrip shaders |

### Shader Summary Table

| Shader | Foundation | Pipeline | Key Technique |
|--------|-----------|----------|---------------|
| RippleShader | Impact | SpriteBatch | SDF concentric rings + noise |
| DamageZoneShader | Impact | SpriteBatch | Radial noise + circle mask + breathing |
| SlashMarkShader | Impact | SpriteBatch | SDF slash arc + noise distortion |
| **RadialNoiseMaskShader** | Mask | SpriteBatch | **Polar-coord noise + gradient LUT + circle mask** (MOST REUSED) |
| InfernalBeamBodyShader | InfernalBeam | VertexStrip | Multi-texture scrolling beam body |
| ConvergenceBeamShader | Laser | VertexStrip | 4-texture scrolling beam with saturation control |
| FlareRainbowShader | Laser | SpriteBatch | Batch-level rainbow flare coloring |
| SparkleTrailShader | SparkleProjectile | VertexStrip | Star texture + procedural glitter peaks + gradient LUT |
| CrystalShimmerShader | SparkleProjectile | SpriteBatch | 6-facet angular shimmer + HSV prismatic + pow(16) peaks |
| SmearDistortShader | SwordSmear | SpriteBatch | Noise-driven UV distortion + gradient flow |
| ThinBeamShader | ThinLaser | VertexStrip | Simplified 2-texture scrolling beam |
| ThinSlashShader | ThinSlash | SpriteBatch | SDF thin line + 3-color gradient |
| XSlashShader | XSlash | SpriteBatch | Noise fire distortion on X texture + UV scroll + gradient |

### Reusable Utility Methods

| Method | Foundation | What It Does |
|--------|-----------|-------------|
| `DrawGlowLine(a, b)` | Attack | Stretched SoftGlow texture between two screen-space points |
| `DrawTextureStripRibbon(positions, colors, texture, ...)` | Ribbon | Generic UV-mapped trail from any texture using source rectangles |
| `DrawRibbonBloomOverlay(positions, colors, ...)` | Ribbon | Shared bloom overlay along any position array |
| `DrawHeadBloom()` | Ribbon | 4-layer bloom halo at projectile head |
| `GetAlphaMultiplier()` | ThinSlash, XSlash | Fade in → hold → fade out timing envelope |

### Design Patterns

1. **Carrier → Effect pattern:** ThinSlash and XSlash use a homing carrier projectile that spawns a separate VFX-only effect projectile on impact. Separates travel behavior from impact rendering.

2. **Style/Mode enum pattern:** Every foundation defines an enum (`SmearStyle`, `NoiseMode`, `RibbonMode`, etc.) with corresponding `Get*Colors()`, `Get*Gradient()`, and `Get*Noise()` methods for easy theme switching.

3. **Ring buffer trails:** Used by Ribbon (40 positions), SparkleProjectile (24 positions), and RangerShot (8 positions) for smooth trail history.

4. **extraUpdates = 1:** Used by Ribbon and SparkleProjectile for double-density trail recording (AI runs twice per frame).

5. **Shader quad pattern:** Impact, ThinSlash, XSlash, and Mask foundations all render shaders onto a SoftCircle/texture quad — the shader computes all visual detail from the quad's UVs.

6. **Pixel shader reset:** After any VertexStrip shader draw, always call `Main.pixelShader.CurrentTechnique.Passes[0].Apply()` to restore Terraria's default pixel shader.
