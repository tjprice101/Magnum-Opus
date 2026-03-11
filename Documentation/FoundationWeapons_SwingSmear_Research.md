# Foundation Weapons & Swing/Smear Systems — Complete Research

## Table of Contents
1. [Directory Layout](#1-directory-layout)
2. [Two Melee Swing Architectures](#2-two-melee-swing-architectures)
3. [Foundation Weapons Overview](#3-foundation-weapons-overview)
4. [Shader Pipeline](#4-shader-pipeline)
5. [Trail System](#5-trail-system)
6. [Support Systems](#6-support-systems)
7. [Key Code Locations](#7-key-code-locations)

---

## 1. Directory Layout

### Content/FoundationWeapons/ (16 folders)

```
FoundationWeapons/
├── AttackFoundation/           — Base projectile patterns (combo, throw-slam, flaming ring, ranger shot)
│   ├── AFTextures.cs
│   ├── AttackFoundation.cs     — Item: cycles through 5 attack modes
│   ├── ComboSwingProjectile.cs — Held-projectile 3-phase combo swing using MeleeSwingBase
│   ├── AstralgraphProjectile.cs
│   ├── FlamingRingProjectile.cs
│   ├── RangerShotProjectile.cs
│   └── ThrowSlamProjectile.cs
│
├── AttackAnimationFoundation/  — Cinematic multi-slash combo with camera effects
│   ├── AAFPlayer.cs            — ModPlayer for camera pan/zoom/B&W flash
│   ├── AAFTextures.cs          — Texture registry
│   ├── AttackAnimationFoundation.cs — Item
│   ├── AttackAnimationProjectile.cs — Invisible triggerer: 5 rapid slashes
│   └── SlashVFXProjectile.cs   — Individual slash: random angle, progressive blur/brightness
│
├── SwordSmearFoundation/       — Reference smear swing implementation
│   ├── SwordSmearFoundation.cs — Item with 2 smear texture styles
│   ├── SmearSwingProjectile.cs — Angular swing + SmearDistortShader + 3-sub-layer rendering
│   ├── SMFTextures.cs          — Texture registry (2 arc textures, noise, gradient, glow)
│   └── Shaders/
│       ├── SmearDistortShader.fx  — Dual-layer noise distortion + gradient LUT coloring
│       └── SmearDistortShader.fxc
│
├── ThinSlashFoundation/        — SDF razor-thin slash lines
│   ├── ThinSlashFoundation.cs  — Item with 5 color styles
│   ├── ThinSlashProjectile.cs  — Homing projectile spawns slash on death
│   ├── ThinSlashEffect.cs      — Slash VFX: SDF shader on quad, 5 ColorStyle presets
│   ├── TSFTextures.cs
│   └── Shaders/
│       ├── ThinSlashShader.fx  — SDF line math, core/mid/edge gradient, tip taper
│       └── ThinSlashShader.fxc
│
├── XSlashFoundation/           — X-shaped impact crosses
│   ├── XSlashFoundation.cs     — Item with 6 score-themed styles
│   ├── XSlashProjectile.cs     — Homing projectile spawns X on death
│   ├── XSlashEffect.cs         — X VFX: shader on sprite, 6 ScoreStyle presets
│   ├── XSFTextures.cs
│   └── Shaders/
│       ├── XSlashShader.fx     — Noise distortion + diagonal energy flow + gradient LUT
│       └── XSlashShader.fxc
│
├── LaserFoundation/            — Channeled convergence beam
│   ├── LaserFoundation.cs      — Item: right-click cycles 6 themes
│   ├── LaserFoundationBeam.cs  — VertexStrip beam body + ConvergenceBeamShader + rainbow flares
│   ├── LFEasings.cs
│   ├── LFTextures.cs
│   └── Shaders/                — ConvergenceBeamShader.fx, FlareRainbowShader.fx
│
├── ThinLaserFoundation/        — Narrow precision laser
│   ├── ThinLaserFoundation.cs
│   ├── ThinLaserBeam.cs
│   └── Shaders/
│
├── InfernalBeamFoundation/     — Infernal-themed beam with noise/distortion
│   ├── InfernalBeamFoundation.cs
│   ├── InfernalBeam.cs
│   ├── IBFTextures.cs
│   └── Shaders/
│
├── RibbonFoundation/           — 10 ribbon trail rendering techniques
│   ├── RibbonFoundation.cs     — Item: right-click cycles 10 modes
│   ├── RibbonProjectile.cs     — Demonstrates PureBloom, BloomNoiseFade, 7 TextureStrip, Lightning
│   └── RBFTextures.cs
│
├── ImpactFoundation/           — Multi-style impact effects (ripple, damage zone, slash mark)
│   ├── ImpactFoundation.cs
│   ├── ImpactProjectile.cs     — Homing projectile that spawns impact on hit
│   ├── RippleEffectProjectile.cs
│   ├── DamageZoneProjectile.cs
│   ├── SlashMarkProjectile.cs
│   ├── IFTextures.cs
│   └── Shaders/
│
├── ExplosionParticlesFoundation/ — Radial burst particle systems
│   ├── ExplosionParticlesFoundation.cs
│   ├── SparkCarrierProjectile.cs
│   ├── SparkExplosionProjectile.cs
│   └── EPFTextures.cs
│
├── MagicOrbFoundation/         — Glowing sphere projectiles with bloom
│   ├── MagicOrbFoundation.cs
│   ├── MagicOrb.cs
│   ├── OrbBolt.cs
│   └── MOFTextures.cs
│
├── SparkleProjectileFoundation/ — Shimmering sparkle projectiles
│   ├── SparkleProjectileFoundation.cs
│   ├── SparkleCrystalProjectile.cs
│   ├── SPFTextures.cs
│   └── Shaders/
│
├── Foundation4PointSparkle/    — 4-point sparkle star effects
│   ├── Foundation4PointSparkle.cs
│   ├── SparkleStarProjectile.cs
│   ├── SparkleStarExplosion.cs
│   ├── F4PSTextures.cs
│   └── Shaders/
│
├── SmokeFoundation/            — Smoke and mist effects
│   ├── SmokeFoundation.cs
│   ├── SmokeCarrierProjectile.cs
│   ├── SmokeRingProjectile.cs
│   └── SKFTextures.cs
│
└── MaskFoundation/             — Alpha mask shaping techniques
    ├── MaskFoundation.cs
    ├── MaskOrbProjectile.cs
    ├── MFTextures.cs
    └── Shaders/
```

### Common/BaseClasses/

```
BaseClasses/
├── MeleeSwingBase.cs        — 745 lines, Calamity-style held-projectile swing
├── MeleeSwingItemBase.cs    — Abstract item base for combo tracking
└── ExobladeStyleSwing.cs    — 771 lines, Exoblade-architecture with GPU trails + dash
```

### Common/Systems/ (VFX-relevant)

```
Systems/
├── Shaders/
│   ├── ShaderLoader.cs          — 1148 lines, centralized shader+texture registry
│   └── ShaderRenderer.cs
├── VFX/
│   ├── SwingShaderSystem.cs     — 200 lines, TerraBladeSwingVFX shader + SpriteBatch helpers
│   ├── Core/
│   │   └── MotionBlurBloomRenderer.cs — 719 lines, GPU+CPU motion blur bloom
│   └── Trails/
│       ├── CalamityStyleTrailRenderer.cs — 1218 lines, 5-style GPU trail renderer
│       └── ... (19 trail files total)
└── Particles/
    └── Particle.cs              — CurveSegment struct + PiecewiseAnimation easing
```

---

## 2. Two Melee Swing Architectures

### Architecture A: MeleeSwingBase (Calamity-Style)

**Files:** `Common/BaseClasses/MeleeSwingBase.cs` (745 lines) + `MeleeSwingItemBase.cs`

**Concept:** A held projectile that rotates around the player in predefined combo phases. The item spawns the projectile, the projectile handles all rendering.

#### ComboPhase Struct
```csharp
public struct ComboPhase
{
    public CurveSegment[] AnimCurves;   // PiecewiseAnimation segments for swing easing
    public float MaxAngle;              // Total angle swept (radians, e.g. MathHelper.Pi)
    public int Duration;                // Total frames for this phase
    public float BladeLength;           // Visual blade length multiplier
    public bool FlipDirection;          // Swing clockwise vs counterclockwise
    public Vector2 SquishRange;         // X=min, Y=max blade squish (stretch effect)
    public float DamageMultiplier;      // Damage scaling for this phase
}
```

#### State Variables (stored in Projectile.ai/localAI)
- `ai[0]` = comboStep (which phase we're in)
- `ai[1]` = InPostSwingStasis (waiting between phases)
- `localAI[0]` = SwingTime (current frame within phase)
- `localAI[1]` = SquishFactor (blade stretch/squish interpolation)

#### Sealed PreDraw Pipeline (rendering order)
1. **Trail** → `CalamityStyleTrailRenderer.DrawTrailWithBloom()` using 60-point ring buffer
2. **Smear Overlay** → SmearDistortShader (shader path) or 3-sub-layer additive (fallback)
3. **Blade Sprite** → Rotated/scaled blade texture
4. **Glow** → Additive light at tip/root
5. **Lens Flare** → At blade tip
6. **MotionBlurBloom** → ArcSweep motion blur
7. **Custom VFX** → Virtual `DrawAdditionalVFX()` hook

#### Smear Rendering (shader path)
```
SmearDistortShader.fx (from SwordSmearFoundation/Shaders/):
  - Input: Slash arc texture + noise texture + gradient LUT
  - Dual-layer noise distortion for organic warping
  - Internal energy flow via third noise scroll
  - Gradient LUT maps intensity → theme color
  - Output: Fluid, dynamic slash arc
```

#### Key Virtual Methods to Override
```csharp
protected abstract ComboPhase[] GetAllPhases();         // Define all combo phases
protected abstract Color[] GetPalette();                 // Theme colors [primary, secondary, core]
protected abstract int GetTrailStyle();                  // CalamityStyleTrailRenderer.TrailStyle enum
protected virtual string GetSmearTexturePath() => null;  // Path to slash arc PNG
protected virtual string GetSmearGradientPath() => null; // Path to gradient LUT PNG
protected virtual void DrawAdditionalVFX(SpriteBatch sb, Vector2 drawPos, float rotation, float progress) { }
```

#### MeleeSwingItemBase (matching item class)
- Tracks `comboStep` with `comboResetTimer` (45 frames default)
- `CanShoot` gate prevents overlapping swings
- `Shoot()` spawns projectile with `ai[0] = comboStep`, then advances step

---

### Architecture B: ExobladeStyleSwing

**File:** `Common/BaseClasses/ExobladeStyleSwing.cs` (771 lines)

**Concept:** Exoblade-architecture with GPU primitive trail rendering and dash mechanic. Uses `PrimitiveRenderer.RenderTrail()` for GPU mesh trails and `GameShaders.Misc` for Calamity-registered MiscShaderData.

#### SwingState Enum
```csharp
private enum SwingState { Swinging, BonkDash }
```

#### Animation Curves (CurveSegments)
```csharp
// Slow start → fast power swing → gentle end
CurveSegment SlowStart = new(EasingType.PolyIn, 0f, 0f, 0.25f, 3);
CurveSegment SwingFast = new(EasingType.ExpOut, 0.3f, 0.25f, 0.65f);
CurveSegment EndSwing  = new(EasingType.SineOut, 0.65f, 0.9f, 0.10f);
```

#### Swing Parameters
- `MaxSwingAngle = PiOver2 * SwingArcMultiplier` (default 1.8 ≈ 162°)
- Trail uses `PrimitiveRenderer.RenderTrail()` with `GameShaders.Misc["MagnumOpus:ExobladeSlash"]`
- Blade drawn with SwingSprite UV-rotation shader or simple rotation math

#### Dash Mechanic
- `DashLungeSpeed = 60`
- GoBack→AndThrust displacement curves
- PostDashCooldown, BigSlashWindow timing gates

#### Key Virtual Methods
```csharp
protected virtual float SwingArcMultiplier => 1.8f;
protected virtual float DashLungeSpeed => 60f;
protected virtual Color GetTrailColor(float progress);
protected virtual float GetTrailWidth(float progress);
protected virtual void OnSwingComplete() { }
```

---

## 3. Foundation Weapons Overview

### SwordSmearFoundation (Reference Smear Implementation)

**SmearSwingProjectile** renders in this order:
1. **Smear arc** — SmearDistortShader applied to FlamingSwordArc or SwordArcSmear texture
   - 3 sub-layers: outer glow (1.1x scale, 30% opacity), main (100%), core (0.7x scale, 60% opacity + white lerp)
   - Shader binds: noise texture (s1), gradient LUT (s2), time, distortion strength, flow speed
2. **Tip glow** — Additive soft glow at blade tip
3. **Root glow** — Additive soft glow at swing origin
4. **Blade sprite** — The actual weapon, rotated
5. **Dust particles** — RainbowMk2 dust along swing arc

### ThinSlashFoundation (SDF Slash Lines)

**ThinSlashEffect** renders a pure-shader quad:
- No texture needed — entire visual is SDF math in ThinSlashShader.fx
- Shader parameters: `slashAngle`, `lineWidth` (0.015 default), `lineLength` (0.45 default)
- 3 color layers: core (near-white, 0.25x width), mid (0.6x width), edge (full width)
- 5 ColorStyle presets: PureWhite, IceCyan, GoldenEdge, VioletCut, CrimsonSlice

### XSlashFoundation (X-Impact Crosses)

**XSlashEffect** renders shader-enhanced sprite:
- X-cross sprite texture + XSlashShader.fx
- Noise-driven UV distortion for fire warping
- Diagonal energy flow scrolling along X arms
- Gradient LUT for theme coloring
- 6 ScoreStyle presets: Eroica, LaCampanella, MoonlightSonata, SwanLake, EnigmaVariations, Fate

### AttackAnimationFoundation (Cinematic Slash Combo)

**AttackAnimationProjectile** orchestrates:
1. Camera pan toward cursor (AAFPlayer.cs)
2. 5 rapid slashes from random angles (SlashVFXProjectile)
3. Progressive blur and brightness increase per slash
4. B&W impact frame on final slash
5. Chromatic aberration on big hits

### LaserFoundation (Convergence Beam)

**LaserFoundationBeam** demonstrates:
- VertexStrip for beam body mesh (2 positions → rectangular quad)
- ConvergenceBeamShader: 4 scrolling detail textures + gradient LUT
- FlareRainbowShader: converts white flare sprites into spinning rainbow flares
- 6 beam themes cycled via right-click
- Smooth aim tracking toward cursor with clamped turn speed

### RibbonFoundation (10 Trail Rendering Techniques)

**RibbonProjectile** showcases:
- **Mode 1 (PureBloom):** Overlapping bloom sprites at each position — 3 layers (outer glow, body, hot core), velocity-stretched
- **Mode 2 (BloomNoiseFade):** Bloom sprites with noise-based erosion at tail — organic break-apart effect  
- **Modes 3-9 (TextureStrip):** UV-mapped texture strips using various textures (BasicTrail, HarmonicWave, SpiralingVortex, EnergySurge, CosmicNebula, MusicalWave, MarbleFlow)
- **Mode 10 (Lightning):** Specialized lightning ribbon rendering
- Ring buffer of 40 positions, extraUpdates=1 for double density

### ImpactFoundation (Impact Effect Styles)

**ImpactProjectile** → spawns on hit:
- **Ripple:** Expanding concentric rings
- **DamageZone:** Lasting area damage zone
- **SlashMark:** Directional slash mark decal

---

## 4. Shader Pipeline

### ShaderLoader.cs (Centralized Registry)

**100+ named shader constants** organized by theme:
- Core: SimpleTrailShader, SimpleBloomShader, ScrollingTrailShader, MotionBlurBloom, TerraBladeSwingVFX, RadialScrollShader, BeamGradientFlow, MetaballEdgeShader, ScreenDistortion, TerraBladeFlareBeamShader
- Moonlight Sonata: MoonlightTrail, LunarBeam, CrescentAura, CrescentBloom, LunarPhaseAura, TidalTrail, etc.
- Eroica: HeroicFlameTrail, SakuraBloom, EroicaFuneralTrail, SakuraSwingTrail, etc.
- Swan Lake: DualPolaritySwing, SwanFlareTrail, PearlescentRocketTrail, ChromaticTrail, etc.
- Nachtmusik: NachtmusikStarTrail, NachtmusikSerenade, ExecutionDecree, CrescendoRise, DimensionalRift, etc.
- Clair de Lune: ClairDeLuneMoonlit, PearlGlow, TemporalDrill, CrystalLance, GearSwing, etc.
- Enigma Variations: VoidSwingTrail, VoidBeam, CadenceSwingTrail, CipherBeamTrail, etc.
- Boss shaders: 5 per boss across 12 bosses (Aura, Trail, Signature, Transition, Dissolve)
- Enemy shaders: 2 per mini-boss (Aura, Trail)

**Texture Loading:**
- 18 noise textures from `Assets/VFX Asset Library/NoiseTextures/`
- 5 trail textures from `Assets/SandboxLastPrism/Trails/`
- Style→texture mapping: `GetDefaultTrailStyleTexture(int style)` and `GetDefaultScrollStyleTexture(int scrollStyle)`

### HLSL Shaders (Foundation Weapons)

**SmearDistortShader.fx** (ps_3_0):
```
Inputs: uImage0 (arc texture), noiseTex, gradientTex
Params: uTime, fadeAlpha, distortStrength, flowSpeed, noiseScale
Pipeline: Dual-layer noise UV distortion → distorted arc sample → 
          energy flow modulation → gradient LUT coloring → brightness boost
```

**ThinSlashShader.fx** (ps_2_0):
```
Inputs: uImage0 (unused — pure SDF)
Params: slashAngle, edgeColor, midColor, coreColor, fadeAlpha, lineWidth, lineLength
Pipeline: Center UV → rotate to slash direction → SDF line distance →
          tip taper → core/mid/edge gradient → composite
```

**XSlashShader.fx** (ps_2_0):
```
Inputs: uImage0 (X-cross texture), noiseTex, gradientTex
Params: uTime, edgeColor, midColor, coreColor, fadeAlpha, fireIntensity, scrollSpeed
Pipeline: Noise UV distortion → distorted X sample → diagonal energy flow → 
          distance-from-center gradient → gradient LUT → 3-tier color → composite
```

### SwingShaderSystem.cs

Manages TerraBladeSwingVFX shader for blade sprite deformation:
```csharp
public static void ApplySwingShader(Effect shader, float rotation, Color color, float intensity);
public static void BeginAdditive(SpriteBatch sb);       // End + Begin with TrueAdditive
public static void RestoreSpriteBatch(SpriteBatch sb);   // End + Begin with AlphaBlend
public static Color GetExobladeColor(float progress);    // Returns color along exoblade palette
```

---

## 5. Trail System

### CalamityStyleTrailRenderer.cs (1218 lines)

**5 Trail Styles** with distinct width/color functions:

| Style | Width Character | Color Character |
|-------|----------------|-----------------|
| Flame | Wide start, taper + flicker | Hot core (white) → primary → secondary |
| Ice | Crystalline faceted variation | Blue-white shimmer, sin-based sparkle |
| Lightning | Jagged spiky (random per frame) | Bright white-blue with random flash |
| Nature | Organic flowing wave | Soft gradient with gentle pulse |
| Cosmic | Nebula expanding/contracting | HSL hue cycling, sparkle accents |

**Rendering Pipeline:**
1. Filter zero-positions into pre-allocated array (zero-GC)
2. Catmull-Rom spline smoothing
3. Build triangle strip vertex buffer with width/color functions
4. Bind shader (SimpleTrailShader) with noise texture on sampler slot 1
5. `DrawUserIndexedPrimitives()` with `TrueAdditive` blend state
6. Restore SpriteBatch

**Multi-Pass Methods:**
- `DrawTrailWithBloom()` — 4 passes: outer bloom (2.5x width, 25% opacity) → middle → inner → core
- `DrawDualLayerTrail()` — 2 passes: full-width body + narrow 0.4x core (lerped toward white)
- `DrawScrollingTrail()` — Uses ScrollingTrailShader.fx with 5 ScrollStyles (Flame/Cosmic/Energy/Void/Holy)

**Theme Integration:**
- `MagnumThemePalettes` static class provides color arrays per theme
- `DrawThemedTrail()` / `DrawThemedProjectileTrail()` accept theme name string

### Pre-allocated Arrays (Zero-GC Hot Path)
```csharp
private static readonly Vector2[] _filteredPositions = new Vector2[MaxVertices];
private static readonly Vector2[] _smoothedPositions = new Vector2[MaxVertices];
private static readonly VertexPositionColorTexture[] _vertexBuffer = new VertexPositionColorTexture[MaxVertices];
private static readonly short[] _indexBuffer = new short[MaxIndices];
```

---

## 6. Support Systems

### MotionBlurBloomRenderer.cs (719 lines)

**3 Blur Kernel Shapes:**
- `Directional` — Linear streak along velocity (projectiles, dashes)
- `Radial` — Burst outward from center (explosions, impacts)
- `ArcSweep` — Tangential rotational blur (melee swings)

**GPU Path (High+ quality):**
- Render sprite to transient RT → apply MotionBlurBloom.fx shader → composite back additively
- Quality tiers: Ultra (13-tap ps_3_0), High (9-tap ps_2_0), Medium (5-tap ps_2_0)

**CPU Fallback (Medium-):**
- Multi-layer directional ghost copies with decreasing opacity
- Standard bloom layers on top (4 layers at Medium, 2 at Low)

**Factory Methods:**
```csharp
MotionBlurConfig.ForProjectile(velocity, color);          // Speed scales blur strength
MotionBlurConfig.ForMeleeSwing(swingDirection, color);     // ArcSweep kernel, 0.06 blur
MotionBlurConfig.ForExplosion(color);                      // Radial kernel, 0.05 blur
```

### CurveSegment / PiecewiseAnimation

Defined in `Common/Systems/Particles/Particle.cs`:

```csharp
public struct CurveSegment
{
    public EasingType Easing;  // Linear, SineIn, SineOut, SineBump, PolyIn, PolyOut, ExpIn, ExpOut, CircIn, CircOut
    public float StartX;       // Animation progress where this segment begins (0-1)
    public float StartY;       // Output value at segment start
    public float Lift;         // Total change in output over segment duration
    public int Power;          // Exponent for Poly easings (default 1)
}
```

Used by both `MeleeSwingBase` (combo phase timing) and `ExobladeStyleSwing` (swing animation).

### MagnumBlendStates.cs

```csharp
public static BlendState TrueAdditive;    // Src=One, Dst=One (ignores alpha, pure additive)
public static BlendState ShaderAdditive;   // Src=SourceAlpha, Dst=One (shader controls via alpha)
```

### MagnumMeleeGlobalItem.cs (261 lines)

GlobalItem that applies full-rotation swing animation to all mod melee weapons:
- Detects weapon theme by namespace (SwanLake, LaCampanella, Eroica, MoonlightSonata, Enigma, Fate) or rarity
- `UseStyle()` — Overrides item rotation for 360° swing, positions item to orbit player
- `UseItemHitbox()` — Moves hitbox to follow weapon tip position
- Works with `MagnumMeleePlayer` ModPlayer for swing state tracking

---

## 7. Key Code Locations

### For implementing a new melee weapon:

| Task | File | Key Method/Class |
|------|------|-------------------|
| Define combo phases | Subclass `MeleeSwingBase` | `GetAllPhases()` returns `ComboPhase[]` |
| Track combo state | Subclass `MeleeSwingItemBase` | Auto-manages `comboStep`, reset timer |
| Choose trail style | Override in `MeleeSwingBase` | `GetTrailStyle()` returns `TrailStyle` enum |
| Choose smear texture | Override in `MeleeSwingBase` | `GetSmearTexturePath()` returns asset path |
| Choose color palette | Override in `MeleeSwingBase` | `GetPalette()` returns `Color[]` |
| Add custom VFX layer | Override in `MeleeSwingBase` | `DrawAdditionalVFX()` |
| Use Exoblade-style | Subclass `ExobladeStyleSwing` | Override swing params + colors |
| Motion blur on swing | Call from `DrawAdditionalVFX` | `MotionBlurBloomRenderer.DrawMeleeSwing()` |
| Custom trail colors | Call directly | `CalamityStyleTrailRenderer.DrawThemedTrail()` |

### For implementing a new projectile VFX:

| Task | File | Key Method |
|------|------|------------|
| Bloom trail | `CalamityStyleTrailRenderer` | `DrawProjectileTrailWithBloom()` |
| Scrolling trail | `CalamityStyleTrailRenderer` | `DrawProjectileScrollingTrail()` |
| Motion blur | `MotionBlurBloomRenderer` | `DrawProjectile()` |
| Custom shader | `ShaderLoader` | `GetShader(name)` + apply in `PreDraw` |
| Ribbon trail | Study `RibbonProjectile.cs` | 10 rendering techniques demonstrated |

### For implementing a new impact/slash effect:

| Task | Study |
|------|-------|
| Thin slash line | `ThinSlashEffect.cs` + `ThinSlashShader.fx` |
| X-cross impact | `XSlashEffect.cs` + `XSlashShader.fx` |
| Ripple/damage zone | `ImpactFoundation/` subfolder |
| Smear arc | `SmearSwingProjectile.cs` + `SmearDistortShader.fx` |
| Cinematic slash combo | `AttackAnimationProjectile.cs` + `SlashVFXProjectile.cs` |

### Shader Parameter Quick Reference

**SmearDistortShader:**
```csharp
shader.Parameters["uTime"].SetValue(time);
shader.Parameters["fadeAlpha"].SetValue(alpha);
shader.Parameters["distortStrength"].SetValue(0.05f);  // 0.03-0.08
shader.Parameters["flowSpeed"].SetValue(0.4f);          // 0.3-0.6
shader.Parameters["noiseScale"].SetValue(2.5f);         // 2.0-3.5
device.Textures[1] = noiseTex;    // samplerNoise
device.Textures[2] = gradientTex; // samplerGradient
```

**ThinSlashShader:**
```csharp
shader.Parameters["slashAngle"].SetValue(angleRadians);
shader.Parameters["edgeColor"].SetValue(edgeColor.ToVector3());
shader.Parameters["midColor"].SetValue(midColor.ToVector3());
shader.Parameters["coreColor"].SetValue(coreColor.ToVector3());
shader.Parameters["fadeAlpha"].SetValue(alpha);
shader.Parameters["lineWidth"].SetValue(0.015f);
shader.Parameters["lineLength"].SetValue(0.45f);
```

**XSlashShader:**
```csharp
shader.Parameters["uTime"].SetValue(time);
shader.Parameters["edgeColor"].SetValue(edge.ToVector3());
shader.Parameters["midColor"].SetValue(mid.ToVector3());
shader.Parameters["coreColor"].SetValue(core.ToVector3());
shader.Parameters["fadeAlpha"].SetValue(alpha);
shader.Parameters["fireIntensity"].SetValue(0.06f);
shader.Parameters["scrollSpeed"].SetValue(0.3f);
device.Textures[1] = noiseTex;    // samplerNoise
device.Textures[2] = gradientTex; // samplerGradient
```

**SimpleTrailShader (via CalamityStyleTrailRenderer):**
```csharp
shader.Parameters["uTime"].SetValue(time);
shader.Parameters["uColor"].SetValue(primary.ToVector3());
shader.Parameters["uSecondaryColor"].SetValue(secondary.ToVector3());
shader.Parameters["uIntensity"].SetValue(intensity);
shader.Parameters["uOverbrightMult"].SetValue(3f);
shader.Parameters["uGlowThreshold"].SetValue(0.5f);
shader.Parameters["uGlowIntensity"].SetValue(1.5f);
device.Textures[1] = noiseTex;  // Style-specific noise
```
