# 🌙 Nachtmusik — Resonance Weapons Planning

> *"A Little Night Music — every weapon is a serenade to the infinite dark, every star a note in the cosmos's final symphony."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Mozart's Eine kleine Nachtmusik — nocturnal serenade, playful yet profound, the night sky as orchestra |
| **Emotional Core** | Nocturnal wonder, stellar beauty, cosmic awe |
| **Color Palette** | Night void, deep indigo, cosmic blue, starlight silver, moon pearl, stellar white |
| **Palette Hex** | Night Void `(10, 10, 30)` → Deep Indigo `(40, 30, 100)` → Cosmic Blue `(60, 80, 180)` → Starlight Silver `(180, 200, 230)` → Moon Pearl `(220, 225, 245)` → Stellar White `(240, 245, 255)` |
| **Lore Color** | `new Color(100, 120, 200)` — Starlight Indigo |
| **Lore Keywords** | Night, stars, twilight, constellation, cosmic, serenade, midnight, nebula, stellar, nocturnal, celestial |
| **VFX Language** | Starlight trails, constellation patterns, cosmic dust, nebula clouds, twilight gradients, aurora-like ribbons, moon glow, stellar explosions, gravitational distortion |

---

## Foundation Weapons Integration Map

| # | Weapon | Class | Primary Foundation | Secondary Foundations | Noise Textures | Key Technique |
|---|--------|-------|-------------------|----------------------|----------------|---------------|
| 1 | Nocturnal Executioner | Melee | SwordSmearFoundation | AttackFoundation, MaskFoundation, ImpactFoundation, ExplosionParticlesFoundation | CosmicNebula (nebula trail), TileableFBMNoise (cosmic cloud) | FBM-distorted cosmic smear arcs + noise-masked execution aura |
| 2 | Midnight's Crescendo | Melee | SwordSmearFoundation | ThinSlashFoundation, MaskFoundation, ImpactFoundation | TileableFBMNoise (wave energy), PerlinFlow (crescendo flow) | Stack-scaling smear width + ThinSlash crescendo wave arcs |
| 3 | Twilight Severance | Melee | ThinSlashFoundation | XSlashFoundation, AttackFoundation, ImpactFoundation | CellularCrack (dimensional rift), TileableFBMNoise (twilight) | Sharp ThinSlash katana trails + XSlash Dimension Sever |
| 4 | Constellation Piercer | Ranged | SparkleProjectileFoundation | RibbonFoundation, MaskFoundation, ImpactFoundation | StarField (constellation pattern), PerlinFlow (field fill) | 5-layer sparkle star projectiles + ribbon constellation lines |
| 5 | Nebula's Whisper | Ranged | SmokeFoundation | MaskFoundation, MagicOrbFoundation, ImpactFoundation | CosmicNebula (nebula cloud), TileableFBMNoise (residue) | Nebula smoke clouds + noise-masked expanding residue fields |
| 6 | Serenade of Distant Stars | Ranged | SparkleProjectileFoundation | RibbonFoundation, AttackFoundation, ImpactFoundation | PerlinFlow (starlight glow), StarField (trail pattern) | Extreme-range sparkle stars + ribbon connection lines |
| 7 | Starweaver's Grimoire | Magic | MagicOrbFoundation | RibbonFoundation, MaskFoundation, ImpactFoundation | StarField (node glow), TileableFBMNoise (tapestry fill) | Orb weave nodes + ribbon thread networks + noise-masked tapestry |
| 8 | Requiem of the Cosmos | Magic | MagicOrbFoundation | MaskFoundation, ImpactFoundation, ExplosionParticlesFoundation, AttackAnimationFoundation | CosmicNebula (orb interior), CosmicVortex (singularity) | Massive cosmic orbs + vortex-noise singularity + screen effects |
| 9 | Celestial Chorus Baton | Summon | MagicOrbFoundation | LaserFoundation, AttackFoundation, RibbonFoundation | PerlinFlow (singer energy), TileableFBMNoise (harmonic) | Orb singer bodies + laser harmonic beam + ribbon formation links |
| 10 | Galactic Overture | Summon | MaskFoundation | AttackFoundation, LaserFoundation, ImpactFoundation | CosmicVortex (galaxy spiral), CosmicNebula (galaxy fill) | Noise-masked evolving galaxy disc + laser cosmic jet beam |
| 11 | Conductor of Constellations | Summon | MagicOrbFoundation | LaserFoundation, ImpactFoundation, AttackFoundation, AttackAnimationFoundation | StarField (constellation body), PerlinFlow (finale aura) | 4-instrument constellation entities + laser strings + finale screen |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Nocturnal Executioner | Melee | 3-phase cosmic combo + Execution Charge (0-100) + 5-blade fan |
| 2 | Midnight's Crescendo | Melee | Rapid combo + Crescendo Stacks (max 15) + crescendo wave arcs |
| 3 | Twilight Severance | Melee | Ultra-fast katana + Twilight Charge + Dimension Sever X-slash |
| 4 | Constellation Piercer | Ranged | Piercing constellation shots + star points + zodiac pattern bonuses |
| 5 | Nebula's Whisper | Ranged | Expanding nebula shots + residue fields + Whisper Storm convergence |
| 6 | Serenade of Distant Stars | Ranged | Extreme-range homing stars + Star Memory + Serenade Rhythm |
| 7 | Starweaver's Grimoire | Magic | Starlight weave nodes + thread networks + pattern casting |
| 8 | Requiem of the Cosmos | Magic | Cosmic burst orbs + Stellar Collapse singularity + Event Horizon |
| 9 | Celestial Chorus Baton | Summon | Conductor baton + celestial singer chorus + harmonic beam |
| 10 | Galactic Overture | Summon | Evolving galaxy minion + stellar projectiles + cosmic jet |
| 11 | Conductor of Constellations | Summon | 4-instrument constellation orchestra + Orchestral Sync + Finale |

---

## 1. Nocturnal Executioner (Melee)

### Identity & Musical Soul
The night's executioner — a cosmic greatsword that channels the finality of midnight. Devastating 3-phase combo builds Execution Charge through combat. At high charge, the weapon unleashes a devastating fan of spectral blades that cut through the night sky like falling stars, converging in a gravitational cosmic explosion. The heaviest, most impactful weapon in Nachtmusik — every swing carries the weight of the void.

### Lore Line
*"At midnight, the executioner does not knock. The stars simply go dark."*

### Combat Mechanics (1850 damage, extends MeleeSwingItemBase)
- **3-Phase Cosmic Combo**:
  - **Phase 1**: Heavy horizontal sweep. Cosmic trailing energy. Spawns NocturnalBladeProjectile forward. Screen shake.
  - **Phase 2**: Rising uppercut. Two blades in V-pattern. Cosmic dust cloud on hit.
  - **Phase 3**: Devastating overhead slam. Three blades in fan + ground impact shockwave (expanding cosmic ring).
- **Execution Charge** (0-100): +5/swing, +10/hit, +15/kill. Decays -2/s.
  - 50+ Charge: Right-click fires 5 blades in fan (2.5x damage). Screen shake + brief screen darkening.
  - 100 Charge: 5 blades at 3.5x + brief homing + massive cosmic explosion at convergence. Resets to 0.
- **Cosmic Presence**: At high charge, player pulses with indigo energy. Orbiting constellation particles. Enemies within 8 tiles take passive aura damage.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              NOCTURNAL EXECUTIONER                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ExplosionParticlesFoundation                  │
│           → Cosmic convergence explosion debris           │
│           → RadialScatter, 55 stellar fragments           │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: ground impact cosmic rings      │
│           → DamageZoneShader: convergence detonation zone │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Cosmic Presence aura  │
│           → CosmicNebula noise for nebula cloud interior │
│           → Charge-scaled intensity (0 at 0, max at 100) │
│  Layer 2: AttackFoundation                              │
│           → Mode 3 (Spread): spectral blade projectiles  │
│           → Mode 5 (Burst): 5-blade execution fan         │
│  Layer 1: SwordSmearFoundation (PRIMARY)                │
│           → SmearDistortShader: cosmic greatsword arcs   │
│           → distortStrength: 0.07 (heavy cosmic warp)    │
│           → noiseTex: CosmicNebula (nebula distortion)   │
│           → gradientTex: Nachtmusik cosmic LUT           │
│           → 3-layer additive rendering                   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Primary Swing: SwordSmearFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `distortStrength` | 0.07 | Heavy cosmic warping for greatsword weight |
| `flowSpeed` | 0.3 | Moderate flow — heavy but cosmic |
| `noiseScale` | 3.5 | Large nebula cloud patterns in smear |
| `noiseTex` | CosmicNebula | Nebula-cloud distortion for cosmic feel |
| `gradientTex` | Nachtmusik_Cosmic_LUT | Night Void → Deep Indigo → Cosmic Blue → Starlight Silver edge |
| Blend | Additive | 3-layer: void core → indigo mid → silver edge |
| Phase scaling | Phase 1: 1.0x, Phase 2: 1.1x, Phase 3: 1.4x | Escalating cosmic intensity |

#### Cosmic Presence Aura: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | CosmicNebula (primary) + TileableFBMNoise (secondary) | Cosmic cloud aura with organic flow |
| `scrollSpeed` | 0.2 | Slow cosmic drift |
| `circleRadius` | 0.25 (charge 0) → 0.48 (charge 100) | Aura scales linearly with Execution Charge |
| `edgeSoftness` | 0.1 | Moderate nebula edge |
| `intensity` | 0.0 (charge 0) → 2.5 (charge 100) | Invisible at 0, brilliant at max |
| Gradient LUT | Nachtmusik_Aura_LUT | Deep Indigo → Cosmic Blue → Starlight Silver |

#### Ground Impact: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Phase 3 overhead slam ground ring | 10 rings, Deep Indigo → Cosmic Blue → Starlight Silver, 0.8s |
| DamageZoneShader | 100-charge convergence detonation | Cosmic Blue → Stellar White, noiseDistortion: 0.05 with CosmicVortex |

#### Convergence Explosion: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | RadialScatter | Stellar fragment burst |
| Spark Count | 55 | Dense cosmic debris |
| Lifetime | 90 frames | Long stellar drift |
| Colors | Deep Indigo, Cosmic Blue, Starlight Silver, Moon Pearl, Stellar White | Full nocturnal palette |
| Gravity | 0.1 (very low) | Fragments float in cosmic drift |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| NocturnalBladeProjectile | AttackFoundation (Mode 1/3) | Spectral blade forward; V-pattern; fan spread | Indigo blade silhouette, SparkleProjectileFoundation shimmer |
| ExecutionFanBlade (x5) | AttackFoundation (Mode 5) | 5-blade fan at 50+ charge, brief homing at 100 | Silver-edged ghost blade, RibbonFoundation Mode 1 aftertrail |
| CosmicConvergence | ImpactFoundation + ExplosionParticlesFoundation | 100-charge convergence point explosion | DamageZoneShader + 55 stellar fragments |
| GroundShockwave | ImpactFoundation (RippleShader) | Phase 3 expanding cosmic ring | 10-ring indigo → silver ripple |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Cosmic Presence Damage | Passive damage to enemies within 8 tiles at high charge | Continuous while in range |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Cosmic_LUT | `Assets/Nachtmusik/NocturnalExecutioner/Gradients/` | "Horizontal color gradient strip, deep void black left through indigo through cosmic blue to starlight silver right, nocturnal cosmic depth, game LUT texture, 256x16px --ar 16:1 --style raw" |
| CosmicSmearArc | `Assets/Nachtmusik/NocturnalExecutioner/SlashArcs/` | "Heavy cosmic greatsword slash arc with embedded nebula cloud patterns and starlight silver edge, deep indigo and cosmic blue energy on solid black background, game melee VFX texture, 512x256px --ar 2:1 --style raw" |
| SpectralBladeSprite | `Assets/Nachtmusik/NocturnalExecutioner/Trails/` | "Ghostly spectral blade silhouette with starlight silver edge glow and deep indigo body, ethereal phantom sword, on solid black background, game projectile sprite, 128x32px --ar 4:1 --style raw" |

---

## 2. Midnight's Crescendo (Melee)

### Identity & Musical Soul
A crescendo building from midnight's silence — rapid alternating slashes that build stacking momentum. Each hit adds to the crescendo, growing the trail wider and brighter. At 8+ stacks, the weapon begins releasing crescendo wave arcs that extend reach. At maximum 15 stacks, the weapon is a blinding storm of starlight and cosmic energy. The rhythm of the night, building ever louder until silence shatters.

### Lore Line
*"The night starts quiet. It does not end that way."*

### Combat Mechanics (extends MeleeSwingItemBase)
- **Rapid 3-Phase Combo**: Fast alternating left-right-overhead. Speed over weight.
- **Crescendo Stacks** (max 15): +1 per hit, +12% damage, +2% crit per stack. Decay after 1.5s without hitting.
  - 5 stacks: Trail more vibrant, sparkle density increases
  - 8+: Each swing releases crescendo wave — expanding arc extending reach by 8 tiles
  - 15: Maximum Crescendo — waves deal double damage, trail massive and brilliant
- **Celestial Harmony**: On hit applies +10% damage from all Nachtmusik weapons (theme debuff).
- **Momentum Preservation**: 10+ stacks for 5s extends decay to 3s.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              MIDNIGHT'S CRESCENDO                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: crescendo wave arc impacts     │
│           → SlashMarkShader: rapid swing contact marks   │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: stack indicator ring  │
│           → 15 segments, filling as stacks increase      │
│           → PerlinFlow noise for luminous energy fill     │
│  Layer 2: ThinSlashFoundation                           │
│           → ThinSlashShader: crescendo wave arc shapes   │
│           → Expanding crescent arcs at 8+ stacks         │
│           → Width + brightness scaled by stack count      │
│  Layer 1: SwordSmearFoundation (PRIMARY)                │
│           → SmearDistortShader: stack-scaling swing trail │
│           → distortStrength: 0.04 (low) → 0.08 (max)    │
│           → Width: 0.6x (stack 0) → 1.8x (stack 15)     │
│           → noiseTex: TileableFBMNoise                   │
│           → gradientTex shifts: Indigo → Silver → White  │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Primary Swing: SwordSmearFoundation (stack-scaling)
| Parameter | Stack 0-4 | Stack 5-7 | Stack 8-14 | Stack 15 |
|-----------|-----------|-----------|------------|----------|
| `distortStrength` | 0.04 | 0.05 | 0.06 | 0.08 |
| `flowSpeed` | 0.4 | 0.5 | 0.6 | 0.8 |
| Width multiplier | 0.6x | 0.9x | 1.2x | 1.8x |
| `gradientTex` | Dark indigo LUT | Indigo-blue LUT | Blue-silver LUT | Silver-white brilliant LUT |
| Bloom intensity | Subtle | Moderate | Strong | Blinding |
| `noiseTex` | TileableFBMNoise | TileableFBMNoise | PerlinFlow | PerlinFlow + star sparkles |

#### Crescendo Wave Arcs: ThinSlashFoundation (at 8+ stacks)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | Crescent arc (Style 2: smooth arc) | Musical wave shape |
| `lineWidth` | 0.02 (stack 8) → 0.04 (stack 15) | Expanding with intensity |
| `lineLength` | 0.35 → 0.50 | Larger reach at higher stacks |
| `edgeColor` | Cosmic Blue | Outer wave edge |
| `midColor` | Starlight Silver | Wave body |
| `coreColor` | Stellar White (at 15: pulsing) | Brilliant wave center |
| Timing | 25 frames (faster than standard 35) | Quick crescendo wave |
| Spawn | Each swing at 8+ stacks | One wave per swing |

#### Stack Indicator: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | PerlinFlow | Luminous energy fill |
| Visual | SDF ring with 15 arc segments | Segmented crescendo meter |
| Fill | Segments illuminate as stacks increase | Clear stack count visual |
| Colors | Deep Indigo (empty) → Cosmic Blue (partial) → Starlight Silver (8+) → Stellar White (15) | Progressive color escalation |
| `intensity` | 0.5 (low stacks) → 3.0 (stack 15) | Dramatic brightness at max |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| CrescendoWaveArc | ThinSlashFoundation | Expanding crescent arc at 8+ stacks, 8-tile extension | SDF crescent, Cosmic Blue → Stellar White |
| CrescendoStackSpark | ExplosionParticlesFoundation (micro) | Brief burst per new stack gained | Cosmic blue spark, 4 particles, 4-frame burst |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Celestial Harmony | +10% damage from all Nachtmusik weapons | 180 frames (3s), refreshes |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Crescendo_LUT_Set | `Assets/Nachtmusik/MidnightsCrescendo/Gradients/` | "Four horizontal gradient strips stacked: dark indigo top, indigo-blue second, blue-silver third, brilliant silver-white bottom, nocturnal crescendo progression, game LUT texture set, 256x64px --ar 4:1 --style raw" |
| CrescendoWaveArc | `Assets/Nachtmusik/MidnightsCrescendo/SlashArcs/` | "Expanding crescent sound wave arc with internal standing wave vibration pattern, cosmic blue to starlight silver energy, on solid black background, game VFX texture, 256x128px --ar 2:1 --style raw" |

---

## 3. Twilight Severance (Melee)

### Identity & Musical Soul
Twilight — the boundary between day and night, light and dark. This katana cuts through that boundary with extreme speed and impossible precision. Ultra-fast 3-phase combo fires perpendicular blade waves every third slash, and the Dimension Sever tears through reality itself with a cross-slash that leaves visible rifts in space. Speed and precision over brute force — every cut is clean, sharp, and absolute.

### Lore Line
*"Between light and dark, the blade finds every truth."*

### Combat Mechanics (1450 damage, 25% crit, extends MeleeSwingItemBase)
- **Ultra-Fast 3-Phase Combo**: Fastest melee in Nachtmusik. Quick diagonal → reverse → horizontal. Minimal windup.
- **Twilight Charge** (0-100): +5/swing, -3/s decay.
  - Every 3rd slash fires perpendicular blade waves (crescent projectiles at 90° to swing).
  - 100 Charge right-click: **Dimension Sever** — massive cross-slash firing 5-blade fan at 3x damage. Enemies hit marked with Dimensional Rift (3s continuous damage).
- **Celestial Harmony**: Shares Nachtmusik theme debuff.
- **Twilight Shift**: 50+ charge gives +15% movement speed.
- **Blade Wave Combo**: Consecutive perpendicular waves crossing creates Twilight Cross — AoE burst at intersection.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              TWILIGHT SEVERANCE                           │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Twilight Cross intersection     │
│           → SlashMarkShader: Dimensional Rift wound marks │
│  Layer 3: AttackFoundation                              │
│           → Mode 2 (Spread): perpendicular blade waves   │
│           → Mode 5 (Burst): Dimension Sever blade fan    │
│  Layer 2: XSlashFoundation                              │
│           → XSlashShader: Dimension Sever cross-slash    │
│           → fireIntensity: 0.04 (subtle cosmic fire)     │
│           → CellularCrack noise (dimensional rift tears) │
│           → Chromatic aberration along slash lines        │
│  Layer 1: ThinSlashFoundation (PRIMARY)                 │
│           → ThinSlashShader: razor-sharp katana trails   │
│           → lineWidth: 0.015 (ultra-thin precision)      │
│           → lineLength: 0.50 (extended katana reach)     │
│           → Style 1: clean hard-edge (no feathering)     │
│           → Indigo core → Silver edge                    │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Primary Swing: ThinSlashFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | 1 (clean hard-edge) | Razor-sharp precision — NO soft feathering |
| `lineWidth` | 0.015 | Ultra-thin katana trail (thinnest in Nachtmusik) |
| `lineLength` | 0.50 | Extended reach |
| `edgeColor` | Deep Indigo | Dark sharp edge |
| `midColor` | Cosmic Blue | Twilight body |
| `coreColor` | Starlight Silver | Brilliant cutting edge |
| Timing | 25 frames (fastest swing) | Speed expressed through brevity |

#### Dimension Sever: XSlashFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `fireIntensity` | 0.04 | Subtle cosmic fire distortion (not overwhelming) |
| `scrollSpeed` | 0.4 | Moderate rift energy scroll |
| `noiseTex` | CellularCrack | Dimensional rift tear pattern (cracked spacetime) |
| Gradient LUT | Nachtmusik_Rift_LUT | Deep Indigo → Cosmic Blue → Starlight Silver → Chromatic fringe |
| Timing | 46 frames | Standard X-slash timing |
| Style | 3 (cosmic rift) | Dimensional tear aesthetic |
| Post-effect | 0.3s chromatic aberration along both slash lines | Spatial distortion indicator |

#### Blade Wave Projectiles: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 2 (Spread) | Perpendicular crescent waves |
| Shape | Crescent arc (ThinSlashFoundation derivative) | Thin sharp arcs at 90° to swing |
| Color | Starlight Silver core → Cosmic Blue edge | Silver precision |
| Spawn | Every 3rd slash | Rhythmic wave cadence |

#### Twilight Cross: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Intersection burst when blade waves cross | 6 rings, Cosmic Blue → Stellar White, 0.5s, fast expansion |
| Screen flash | Brief silver-white flash, 4 frames, 20% opacity | Intersection impact weight |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| BladeWaveCrescent | AttackFoundation (Mode 2) | Perpendicular to swing, every 3rd slash | ThinSlash-derived crescent, Starlight Silver |
| DimensionSeverSlash | XSlashFoundation | Cross-slash at 100 charge, 5-blade fan | CellularCrack rift X pattern + chromatic aberration |
| DimensionSeverBlade (x5) | AttackFoundation (Mode 5) | Fan from Dimension Sever, 3x damage | Ghost blades, Indigo body, Silver edge glow |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Celestial Harmony | +10% damage from all Nachtmusik weapons | 180 frames (3s) |
| Dimensional Rift | Continuous rift damage (4% weapon damage/s) | 180 frames (3s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Rift_LUT | `Assets/Nachtmusik/TwilightSeverance/Gradients/` | "Horizontal color gradient strip, deep indigo left through cosmic blue to starlight silver right with subtle chromatic rainbow fringe at far right, dimensional rift energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| DimensionSeverRift | `Assets/Nachtmusik/TwilightSeverance/SlashArcs/` | "Cross-slash dimensional rift pattern with cracked spacetime and cosmic energy bleeding through tears, deep indigo and silver with chromatic aberration edges, on solid black background, game VFX texture, 256x256px --ar 1:1 --style raw" |

---

## 4. Constellation Piercer (Ranged)

### Identity & Musical Soul
Every shot connects stars — this weapon fires piercing star projectiles that mark enemies as Star Points on the battlefield. After enough stars, constellation lines automatically form between them, creating geometric patterns that deal continuous AoE damage. Special zodiac patterns trigger bonus effects. A weapon of geometry and starlight, rewarding precise aim and spatial awareness.

### Lore Line
*"Each star is an enemy. Each line of light between them is a death sentence."*

### Combat Mechanics
- **Piercing Constellation Shot**: Fast piercing projectile through multiple enemies. Each pierced enemy becomes a Star Point.
- **Constellation Formation**: 3+ Star Points from one shot auto-connect with constellation lines. Enemies within polygon take continuous AoE (3s).
- **Star Chain**: Alt fire — Star Chain shot connects to existing points, extending constellations.
- **Zodiac Patterns**: Triangle = damage burst, Square = slowing field, Pentagon = 50-damage player shield.
- **Cosmic Scope**: While aiming, starfield scope overlay shows points and potential connections.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CONSTELLATION PIERCER                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Star Point creation flash       │
│           → DamageZoneShader: constellation polygon AoE  │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: constellation field    │
│           → StarField noise for embedded star sparkles    │
│           → PerlinFlow secondary for cosmic field fill    │
│  Layer 2: RibbonFoundation                              │
│           → Mode 1 PureBloom: constellation line segments│
│           → Thin luminous lines connecting Star Points    │
│           → Pulsing brightness along connections          │
│  Layer 1: SparkleProjectileFoundation (PRIMARY)         │
│           → SparkleTrailShader: piercing star trail       │
│           → CrystalShimmerShader: star body shimmer       │
│           → 5-layer rendering with dot-spaced trail       │
│           → Theme: Stellar (Nachtmusik 5-color palette)   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Star Projectile: SparkleProjectileFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Theme | Custom "Stellar" | Night Void → Deep Indigo → Cosmic Blue → Starlight Silver → Stellar White |
| SparkleTrailShader | Active — dot-spaced mode | Trail leaves regularly-spaced dots (like connecting constellation dots) |
| CrystalShimmerShader | Active | 4-point star shimmer on projectile body |
| Ring Buffer | 24 positions | Full constellation trail length |
| Trail persistence | 120 frames (2s) — much longer than normal | Trails persist to show constellation paths |
| Piercing | Continues through enemies, marking each | Multi-hit constellation building |

#### Constellation Lines: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean luminous starlight lines |
| Width | 3px | Thin constellation lines |
| Color | Starlight Silver core → Cosmic Blue edge | Star connection energy |
| Pulsing | Brightness oscillates 0.8-1.0 at 1Hz | Living cosmic energy |
| Persistence | Matches constellation duration (3s) | Lines last as long as the formation |

#### Constellation Field: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | StarField (primary) + PerlinFlow (secondary) | Star sparkles within cosmic field fill |
| `scrollSpeed` | 0.1 | Slow cosmic drift |
| `circleRadius` | Polygon-fitted (varies per formation) | Fills constellation polygon area |
| `edgeSoftness` | 0.06 | Defined constellation boundary |
| `intensity` | 1.4 | Subtle cosmic field (damage indicator, not overwhelming) |
| Gradient LUT | Nachtmusik_Constellation_LUT | Deep Indigo → Cosmic Blue |

#### Star Point Creation: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Star Point creation flash | 4 rings, Starlight Silver, 0.3s, small radius |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ConstellationPiercerShot | SparkleProjectileFoundation | Piercing, marks enemies as Star Points | 5-layer stellar sparkle, 2s persistent dot trail |
| StarChainShot | SparkleProjectileFoundation (variant) | Connects to existing Star Points | Golden-tinted sparkle, connects automatically |
| ConstellationLine | RibbonFoundation (persistent) | Auto-forms between 3+ Star Points | PureBloom silver lines, 3s duration |
| ConstellationField | MaskFoundation (persistent) | Fills polygon between connected points | StarField-noise cosmic field, 3s AoE |
| ZodiacBonusBurst | ImpactFoundation | Triangle/Square/Pentagon bonus trigger | Pattern-specific colored ripple burst |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Constellation_LUT | `Assets/Nachtmusik/ConstellationPiercer/Gradients/` | "Horizontal color gradient strip, deep indigo left through cosmic blue to subtle starlight silver right, nocturnal constellation field energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| StarPointGlyph | `Assets/Nachtmusik/ConstellationPiercer/Orbs/` | "Bright 4-pointed star glyph with radiant silver center and cosmic blue corona, constellation star point marker, on solid black background, game VFX sprite, 32x32px --ar 1:1 --style raw" |

---

## 5. Nebula's Whisper (Ranged)

### Identity & Musical Soul
Nebulae whisper in light that takes millennia to arrive — soft, diffuse, overwhelming in aggregate. This weapon fires nebula-cloud projectiles that expand as they travel, leaving persistent residue fields behind. The Whisper Storm converts all residue into a massive concentrated damage zone. Patient, spreading, inevitable — the night sky's gentlest and most devastating breath.

### Lore Line
*"The nebula does not shout. It barely breathes. But entire stars are born in its exhale."*

### Combat Mechanics
- **Nebula Whisper Shot**: Starts tight, expands into wide cloud as it travels. Close: focused. Max range: massive AoE, reduced per-target damage.
- **Nebula Residue**: Lingering cloud trail (2s). Enemies in residue take minor DoT + slow.
- **Accumulation**: Multiple shots through same area layer residue (3+ layers: significant damage + Cosmic Confusion — random enemy movement 1s).
- **Whisper Storm**: After 5 shots, alt fire converges all residue on cursor — massive concentrated nebula cloud (4s damage zone).
- **Silent Approach**: Shots phase through first 3 tiles of walls.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              NEBULA'S WHISPER                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → DamageZoneShader: Whisper Storm convergence  │
│           → RippleShader: residue convergence inward pull│
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: nebula residue fields │
│           → CosmicNebula noise for authentic nebula look │
│           → TileableFBMNoise secondary for organic drift │
│           → Density parameter scales with accumulation   │
│  Layer 2: MagicOrbFoundation                            │
│           → RadialNoiseMaskShader: expanding nebula body │
│           → CosmicNebula noise, expanding circleRadius   │
│           → Soft heavy smoothstep for diffuse edges      │
│  Layer 1: SmokeFoundation (PRIMARY)                     │
│           → Nebula cloud trail + residue rendering        │
│           → Style: Billowing (cosmic indigo-blue tint)    │
│           → 30 puffs per cloud formation                  │
│           → CosmicNebula noise coloring                   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Nebula Cloud Trail: SmokeFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | Billowing | Thick nebula cloud formations |
| Spritesheet | 3x6 cosmic nebula dissipation animation | Nebula-specific cloud behavior |
| Puffs per Formation | 30 | Dense nebula volume |
| Color | Deep Indigo base → Cosmic Blue accents → traces of Starlight Silver sparkle | Authentic nebula palette |
| Opacity | 60% (single layer) → 95% (3+ layers) | Density builds with accumulation |
| Lifetime | 120 frames (2s) | Persistent residue |

#### Expanding Nebula Body: MagicOrbFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Style | CosmicNebula | Authentic nebula cloud interior |
| `scrollSpeed` | 0.3 | Internal nebula drift |
| `circleRadius` | 0.15 (spawn) → 0.48 (max expansion) | Expands as projectile travels |
| `edgeSoftness` | 0.2 | Very soft diffuse edge (nebula nature) |
| `intensity` | 2.0 (close) → 1.2 (expanded) | Dimmer when expanded (spread thin) |
| Gradient LUT | Nachtmusik_Nebula_LUT | Deep Indigo → Cosmic Blue → hints of purple |

#### Residue Field: MaskFoundation (RadialNoiseMaskShader)
| Accumulation | circleRadius | intensity | edgeSoftness | Visual |
|-------------|-------------|-----------|--------------|--------|
| 1 layer | 0.30 | 0.8 | 0.2 | Faint indigo haze |
| 2 layers | 0.38 | 1.2 | 0.15 | Visible nebula field |
| 3+ layers | 0.45 | 1.8 | 0.10 | Dense nebula, silver sparkles, confusion indicator |

#### Whisper Storm: ImpactFoundation + MaskFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| Convergence visual | Residue particles streaming inward (ImpactFoundation RippleShader, reversed) | Residue converges on cursor |
| Storm zone | MaskFoundation at maximum density (CosmicNebula + FBM, intensity 2.8) | Massive concentrated damage zone |
| Duration | 4s | Extended devastation field |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| NebulaWhisperShot | MagicOrbFoundation (expanding) | Expands as travels, phases through 3 tiles | CosmicNebula-noise orb, circleRadius grows over time |
| NebulaResidue | MaskFoundation (persistent) + SmokeFoundation | Lingers 2s along shot path | Nebula cloud field, accumulation-scaled |
| WhisperStormZone | MaskFoundation (max density) | 4s concentrated damage zone | Dense cosmic nebula, intensity 2.8 |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Nebula Slow | 25% movement slow in residue fields | While in field + 30 frames |
| Cosmic Confusion | Random movement direction (3+ layer residue) | 60 frames (1s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Nebula_LUT | `Assets/Nachtmusik/NebulasWhisper/Gradients/` | "Horizontal color gradient strip, deep indigo left through cosmic blue center to faint purple-silver right, deep space nebula colors, game LUT texture, 256x16px --ar 16:1 --style raw" |
| NebulaClouldSpritesheet | `Assets/Nachtmusik/NebulasWhisper/Pixel/` | "Cosmic nebula cloud spritesheet 3x6 grid showing dissipation from dense blue-indigo cluster to scattered cosmic motes with star sparkles, on solid black background, game particle sheet, 384x192px --ar 2:1 --style raw" |

---

## 6. Serenade of Distant Stars (Ranged)

### Identity & Musical Soul
A serenade — a song of devotion played at distance. This weapon fires homing star projectiles across impossible distances, each carrying the patient inevitability of starlight crossing the void. Star Memory remembers every enemy passed, replaying damage echoes on arrival. The Serenade Rhythm rewards consistent timing with perfect homing. Romantic, patient, but inexorable — a love letter written in light.

### Lore Line
*"The light left a star ages ago, just to find you. And it never missed."*

### Combat Mechanics
- **Distant Star Shot**: Homing star with 80-tile range, moderate homing. Starlight silver trail.
- **Star Memory**: Stars remember enemies passed within 5 tiles. After hit/max range, fires Star Echo projectiles back at remembered enemies.
- **Serenade Rhythm**: Firing every 1.0s ±0.2s builds stacks (max 5). +10% homing/stack. At 5: perfect homing (irresistible).
- **Distant Connection**: Two airborne stars passing within 3 tiles form a Connection — brief damage line between them.
- **Starlight Sonata**: Killing with perfect-homing star creates mini-star at kill position firing 4 Star Echoes (cascade).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              SERENADE OF DISTANT STARS                    │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Starlight Sonata nova burst    │
│           → SlashMarkShader: Star Memory echo marks       │
│  Layer 3: AttackFoundation                              │
│           → Mode 4 (Targeted): Star Echo return fire     │
│           → Mode 1 (Direct): main star firing             │
│  Layer 2: RibbonFoundation                              │
│           → Mode 1 PureBloom: Distant Connection lines   │
│           → Brief curved energy lines between stars       │
│           → Elegant slight curve (not straight)           │
│  Layer 1: SparkleProjectileFoundation (PRIMARY)         │
│           → SparkleTrailShader: starlight constellation   │
│           → CrystalShimmerShader: 4-point star shimmer   │
│           → 5-layer rendering, extreme persistence        │
│           → Theme: Starlight Serenade                     │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Star Projectile: SparkleProjectileFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Theme | "Starlight Serenade" | Deep Indigo → Cosmic Blue → Starlight Silver → Moon Pearl → Stellar White |
| SparkleTrailShader | Active — long persistence | Extended shimmer trail across 80-tile range |
| CrystalShimmerShader | Active — 4-point star pattern | Star-shaped shimmer instead of crystal facets |
| Ring Buffer | 24 positions | Full trail buffer |
| Serenade scaling | At 5 stacks: star gains corona rays (4 extending light beams from points) | Visual homing perfection indicator |
| Homing visual | Trail curves visibly toward target | Homing trajectory visible in trail path |

#### Star Echo: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 4 (Targeted) | Homes toward remembered enemies |
| Visual | SparkleProjectileFoundation (dimmer variant) | Smaller, fainter echo star |
| Count | 1 per remembered enemy | Memory replay |

#### Distant Connection: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean luminous energy arc |
| Width | 3px | Thin elegant line |
| Color | Starlight Silver → Stellar White | Pure starlight |
| Duration | 18 frames (0.3s) | Brief but visible connection |
| Curvature | Slight bezier curve (not straight line) | Elegant, serenade-like |

#### Starlight Sonata Nova: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Cascade kill nova burst | 8 rings, Starlight Silver → Stellar White, 0.6s |
| 4 Star Echo spawns | AttackFoundation Mode 4 from kill position | At kill position, 4 echoes radiate outward |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| DistantStarShot | SparkleProjectileFoundation | 80-tile range homing, remembers enemies | 5-layer stellar sparkle, 4-point star shimmer |
| StarEchoProjectile | AttackFoundation (Mode 4) + SparkleProjectileFoundation (dim) | Returns to remembered enemies | Smaller/dimmer sparkle star |
| DistantConnection | RibbonFoundation (brief) | Damage line between passing stars | PureBloom curved silver line, 0.3s |
| StarlightSonataNova | ImpactFoundation + AttackFoundation | Cascade kill → 4 echoes | 8-ring silver nova + 4 outward echo stars |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| SerenadeStarSprite | `Assets/Nachtmusik/SerenadeOfDistantStars/Orbs/` | "Brilliant 4-pointed star with radiant silver-white corona rays extending from each point, romantic starlight energy, on solid black background, game projectile sprite, 48x48px --ar 1:1 --style raw" |
| StarEchoSprite | `Assets/Nachtmusik/SerenadeOfDistantStars/Pixel/` | "Small fading echo star with gentle cosmic blue glow, dimmer version of a bright star, memory of light, on solid black background, game particle sprite, 24x24px --ar 1:1 --style raw" |
---

## 7. Starweaver's Grimoire (Magic)

### Identity & Musical Soul
A grimoire that weaves starlight into spell patterns — nets, webs, and tapestries of interconnected light that trap and damage enemies. Each cast creates a Weave Node that auto-connects to nearby nodes via threads. Dense networks amplify through resonance, and the Celestial Tapestry special creates a massive grid of lethal starlight. The weapon rewards spatial awareness and patient network building.

### Lore Line
*"The stars are threads. The night is the loom. And this book knows every pattern."*

### Combat Mechanics
- **Starweave Bolt**: Fires starlight bolt creating Weave Node at impact (5s). Bolts near existing nodes auto-connect with Weave Threads (damage lines).
- **Weave Network**: Connected nodes form networks. Enemies touching threads take damage. 3+ connections → Nexus Node (AoE).
- **Pattern Casting** (alt fire cycles):
  - **Star Web**: 5 nodes in pentagonal pattern
  - **Starlight Net**: Net projectile creating node cluster around target (binding)
  - **Celestial Tapestry**: 12 nodes in grid (3s channel)
- **Thread Resonance**: Damaged threads vibrate, triggering nearby thread chain reactions. Dense networks amplify massively.
- **Unravel**: Enemy death on a node unravels connected threads — burst of energy along each.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              STARWEAVER'S GRIMOIRE                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Nexus Node AoE activation      │
│           → SlashMarkShader: unravel energy burst marks  │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Celestial Tapestry    │
│           → StarField noise for star-filled grid cells   │
│           → TileableFBMNoise for cosmic grid fill         │
│  Layer 2: RibbonFoundation                              │
│           → Mode 1 PureBloom: Weave Thread connections   │
│           → Thin pulsing starlight connection lines       │
│           → Vibration animation on damage                 │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: Weave Node bodies     │
│           → StarField noise for star-cluster appearance  │
│           → Nexus variant: brighter, corona effect        │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Weave Nodes: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| State | circleRadius | intensity | Gradient LUT | Unique Feature |
|-------|-------------|-----------|-------------|----------------|
| Normal Node | 0.25 | 1.6 | Starlight Silver → Cosmic Blue | Star-cluster appearance |
| Nexus Node (3+ connections) | 0.32 | 2.4 | Starlight Silver → Stellar White corona | Brighter with extended corona rays |

Common parameters:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Style | StarField | Star-cluster interior |
| `scrollSpeed` | 0.15 | Slow stellar drift |
| `edgeSoftness` | 0.06 | Defined node boundary |
| Lifespan | 300 frames (5s) with brightness decay in final 1s | Timed node persistence |

#### Weave Threads: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean starlight thread lines |
| Width | 2px (base) → 4px (dense network) | Thickens with more connections |
| Color | Starlight Silver core → Cosmic Blue edge | Starlight thread energy |
| Pulsing | Energy traveling along thread at 2 pixels/frame | Visible flow direction |
| Vibration | On damage: lateral offset oscillation, 4 frames, amplitude 2px | Resonance chain reaction visual |

#### Celestial Tapestry: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | StarField (primary) + TileableFBMNoise (secondary) | Star-filled cells with cosmic nebula fill |
| `scrollSpeed` | 0.1 | Slow cosmic drift in grid |
| `circleRadius` | 0.48 (full tapestry coverage) | Spans the entire 12-node grid |
| `intensity` | 2.0 | Vivid cosmic grid |
| Grid rendering | 12 nodes + connecting threads rendered as unified mesh | Efficient tapestry visualization |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| StarweaveBolt | MagicOrbFoundation (small, fast) | Direct fire, creates Weave Node at impact | StarField-noise small orb, sparkle trail |
| WeaveNode | MagicOrbFoundation (persistent) | Stationary 5s, auto-connects nearby | Star-cluster orb, Nexus variant at 3+ connections |
| WeaveThread | RibbonFoundation (persistent) | Connecting line between nodes, deals damage | PureBloom silver thread, energy pulse, vibration on hit |
| CelestialTapestryGrid | MaskFoundation (large) | 12-node grid, 3s channel | StarField+FBM filled cosmic grid |
| UnravelBurst | ImpactFoundation | Travels along threads when node destroyed | Energy burst along each thread path |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| StarfieldNoise | `Assets/Nachtmusik/StarweaversGrimoire/Trails/` | "Dense starfield pattern with scattered bright star points of varying sizes against cosmic dark background, tileable star cluster texture, white on solid black background, game VFX noise texture, 256x256px --ar 1:1 --style raw" |
| WeaveNodeGlyph | `Assets/Nachtmusik/StarweaversGrimoire/Orbs/` | "Starlight weave node with internal star cluster and faint connecting radial filaments, silver-blue cosmic node, on solid black background, game VFX sprite, 48x48px --ar 1:1 --style raw" |

---

## 8. Requiem of the Cosmos (Magic)

### Identity & Musical Soul
A requiem for the cosmos itself — the grandest, most devastating magic weapon in Nachtmusik. Massive cosmic orbs detonate as supernovae, charged shots create gravitational singularities that pull and explode, and the 10th-cast Event Horizon warps the entire screen. You are channeling forces too vast for mortal comprehension — dying stars, collapsing singularities, cosmic events that reshape reality.

### Lore Line
*"The cosmos sings its own requiem. You merely conduct the final movement."*

### Combat Mechanics
- **Cosmic Burst**: Large cosmic orb detonates on impact — supernova explosion (6 tile radius). Slow but devastating.
- **Stellar Collapse** (2s charge): Gravitational singularity on impact — pulls enemies within 10 tiles for 2s, then explodes outward.
- **Cosmic Event Cycle** (10 casts):
  - Casts 1-3: Normal cosmic bursts
  - Casts 4-6: Bursts leave persistent nebula fields (lingering damage)
  - Casts 7-9: Bursts spawn orbiting starlets at impact (mini-damage satellites)
  - Cast 10: **Event Horizon** — auto Stellar Collapse with double radius + screen distortion
- **Cosmic Awareness**: While equipped, enemies on screen have 5% weakened resistances.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              REQUIEM OF THE COSMOS                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: AttackAnimationFoundation                     │
│           → Event Horizon screen-wide cinematic effect   │
│           → Phase 4 (Climax): cosmic screen overlay       │
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Supernova radial debris                       │
│           → RadialScatter, 55 cosmic fragments            │
│  Layer 3: ImpactFoundation                              │
│           → RippleShader: supernova expansion rings       │
│           → DamageZoneShader: singularity pull zone       │
│  Layer 2: MaskFoundation                                │
│           → RadialNoiseMaskShader: persistent nebula      │
│           → CosmicVortex noise for singularity rendering  │
│           → CosmicNebula noise for nebula fields          │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: cosmic burst orb body │
│           → CosmicNebula noise for nebula cloud interior │
│           → Large circleRadius (2x normal projectile)    │
│           → Intense corona glow                           │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Cosmic Orb: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Style | CosmicNebula | Authentic cosmic interior |
| `scrollSpeed` | 0.35 | Active internal nebula movement |
| `rotationSpeed` | 0.2 | Swirling cosmic energy |
| `circleRadius` | 0.48 (2x normal) | Massive projectile |
| `edgeSoftness` | 0.06 | Defined cosmic boundary with corona |
| `intensity` | 2.8 | Brilliant cosmic energy |
| Gradient LUT | Nachtmusik_Cosmic_LUT | Night Void → Deep Indigo → Cosmic Blue → Starlight Silver highlights |
| Corona | 3-layer additive bloom (Indigo → Blue → Silver) at 1.5x, 2.0x, 2.5x scale | Massive corona presence |

#### Stellar Collapse Singularity: MaskFoundation (RadialNoiseMaskShader) — 2 phases
| Phase | Noise | scrollSpeed | circleRadius | intensity | Visual |
|-------|-------|------------|-------------|-----------|--------|
| Pull (2s) | CosmicVortex | 0.8 (fast inward pull) | 0.45 shrinking to 0.15 | 3.0 → 4.0 | Vortex pulling inward, screen distortion |
| Explode | CosmicNebula | 0.0 → 1.0 burst | 0.0 → 0.48 rapid expansion | 4.0 → 0.0 | Expanding cosmic ring, fading |

#### Supernova: ImpactFoundation + ExplosionParticlesFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ImpactFoundation RippleShader | 12 rings, Deep Indigo → Cosmic Blue → Starlight Silver → Stellar White, 1.0s | Expanding supernova rings |
| ExplosionParticlesFoundation | RadialScatter, 55 cosmic fragments, full Nachtmusik palette | Stellar debris burst |
| Screen shake | Intensity 10, duration 20 frames | Cosmic detonation weight |

#### Event Horizon: AttackAnimationFoundation
| Phase | Duration | Visual |
|-------|----------|--------|
| Buildup (Phase 1-2) | 30 frames | Screen edges darken, cosmic vignette |
| Singularity (Phase 3) | 40 frames | Screen wobble, gravitational lens distortion |
| Climax (Phase 4) | 20 frames | Cosmic flash + double-radius Stellar Collapse |
| Resolution | 20 frames | Stars briefly visible in background, fade back |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| CosmicBurstOrb | MagicOrbFoundation | Slow, massive, detonates on impact | CosmicNebula orb, 3-layer corona bloom |
| StellarCollapseOrb | MagicOrbFoundation (charged) | 2s charge, singularity on impact | Same orb + CosmicVortex pull → explode cycle |
| SupernovaDetonation | ImpactFoundation + ExplosionParticlesFoundation | 6-tile radius explosion | 12-ring ripple + 55 cosmic fragments |
| NebulaFieldPersistent | MaskFoundation | Lingers from casts 4-6, deals AoE DoT | CosmicNebula field, 4s duration |
| OrbitingStarlet | SparkleProjectileFoundation (mini) | Orbits impact point from casts 7-9, mini-damage | Tiny silver star, tight orbit, 8s duration |
| EventHorizonEffect | AttackAnimationFoundation | Cast 10 auto-trigger, screen-wide | Full 4-phase cinematic + double Stellar Collapse |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_CosmicVortex_LUT | `Assets/Nachtmusik/RequiemOfTheCosmos/Gradients/` | "Horizontal color gradient strip, void black left through deep indigo through cosmic blue to brilliant stellar white right, gravitational singularity energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| CosmicOrbFlare | `Assets/Nachtmusik/RequiemOfTheCosmos/Flare/` | "Massive cosmic orb with internal nebula cloud swirl and brilliant corona rays extending outward, deep indigo blue and starlight silver, on solid black background, game VFX flare texture, 256x256px --ar 1:1 --style raw" |
| SupernovaImpact | `Assets/Nachtmusik/RequiemOfTheCosmos/Flare/` | "Supernova explosion with expanding ring of cosmic energy and scattered stellar fragments, indigo blue and silver with white-hot center, on solid black background, game VFX impact texture, 512x512px --ar 1:1 --style raw" |

---

## 9. Celestial Chorus Baton (Summon)

### Identity & Musical Soul
A conductor's baton that summons a celestial chorus — starlight entities that sing cosmic sound waves at enemies. The conductor directs them with cursor gestures for focused fire, and during Harmonic Phase all singers synchronize into a devastating combined beam. A conducting fantasy made real — you direct the stars themselves.

### Lore Line
*"The baton asks no permission. The stars obey or cease to shine."*

### Combat Mechanics
- **Celestial Chorus Minions**: 3 starlight singers in formation. Fire cosmic sound wave projectiles autonomously.
- **Conductor's Direction**: Rapid cursor movement commands focused fire in that direction (1.5x damage).
- **Harmonic Phase**: Every 15s, all singers synchronize — combined beam for 3s (channeled multi-source converging).
- **Additional Summons**: Up to 6 singers total. At 6: Grand Harmonic — beam 2x wide, 2x damage.
- **Celestial Shield**: When idle, singers orbit player providing +5 defense and +3% damage reduction per singer.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CELESTIAL CHORUS BATON                       │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Harmonic Phase beam impacts    │
│  Layer 3: RibbonFoundation                              │
│           → Mode 1 PureBloom: singer formation links     │
│           → Shield formation orbital connection lines     │
│  Layer 2: LaserFoundation                               │
│           → ConvergenceBeamShader: Harmonic Phase beam   │
│           → Multi-source converging on single target      │
│           → Nachtmusik gradient LUT                       │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: singer entity bodies  │
│           → PerlinFlow noise for ethereal starlight form │
│           → 3 parameterized variants + brightness scaling│
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Singer Bodies: MagicOrbFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Style | PerlinFlow | Ethereal starlight energy |
| `scrollSpeed` | 0.3 | Active internal motion |
| `circleRadius` | 0.30 (compact singer form) | Humanoid-ish entity |
| `edgeSoftness` | 0.08 | Soft ethereal edge |
| `intensity` | 1.8 (idle) → 2.8 (singing/Harmonic Phase) | Brightens during attack |
| Gradient LUT | Nachtmusik_Singer_LUT | Cosmic Blue → Starlight Silver → Stellar White |
| Singer differentiation | Slight size variation (1-3: 0.28, 0.30, 0.32) | Visual distinction |

#### Harmonic Beam: LaserFoundation (ConvergenceBeamShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Detail Textures | 4: harmonic wave pattern, stellar energy flow, cosmic particles, standing wave | Musical beam interior |
| Gradient LUT | Nachtmusik_Harmonic_LUT | Deep Indigo → Cosmic Blue → Starlight Silver → Stellar White |
| Width | 60px (3 singers) → 120px (6 singers Grand Harmonic) | Scales with singer count |
| Multi-source | 3-6 individual beams converging on target | Multiple origin points |
| Duration | 3s sustained | Harmonic Phase duration |
| Endpoint | FlareRainbowShader → adapted stellar flare | Starlight bloom flare at target |

#### Formation Links: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean starlight connections |
| Width | 2px | Thin formation lines |
| Color | Cosmic Blue → Starlight Silver | Celestial formation energy |
| Visibility | Active during shield formation (idle) | Visual shield indicator |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| CelestialSoundWave | AttackFoundation (Mode 1) | Expanding arc from singer to target | Sound wave arc shape, Starlight Silver → Cosmic Blue |
| HarmonicBeam | LaserFoundation | 3-6 converging beams, 3s sustained | ConvergenceBeamShader, stellar gradient |
| FormationLink | RibbonFoundation (persistent) | Connects singers during shield formation | PureBloom Cosmic Blue lines |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Singer_LUT | `Assets/Nachtmusik/CelestialChorusBaton/Gradients/` | "Horizontal color gradient strip, cosmic blue left through starlight silver center to stellar white right, ethereal starlight entity energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| CelestialSingerSprite | `Assets/Nachtmusik/CelestialChorusBaton/Orbs/` | "Ethereal starlight humanoid entity made of silver-blue cosmic energy with visible singing posture and sound wave emanation, celestial chorus singer, on solid black background, game minion sprite, 48x64px --ar 3:4 --style raw" |
| SoundWaveArcCosmic | `Assets/Nachtmusik/CelestialChorusBaton/Trails/` | "Expanding cosmic sound wave arc with internal stellar vibration pattern, silver and cosmic blue energy arc, on solid black background, game projectile VFX texture, 128x64px --ar 2:1 --style raw" |

---

## 10. Galactic Overture (Summon)

### Identity & Musical Soul
An overture is the grand introduction. This summon conjures a miniature galaxy that orbits the player, launching stellar projectiles. The galaxy evolves over time from Simple → Spiral → Barred → Active, gaining visual complexity and combat power. At Active Galaxy it fires cosmic jets from its center beam. Galaxy Collisions between multiple summons create massive projectile bursts. A crescendo of cosmic scale.

### Lore Line
*"Before the symphony begins, the galaxy must announce its arrival."*

### Combat Mechanics
- **Galaxy Minion**: Small rotating galaxy firing stellar projectiles autonomously.
- **Galaxy Evolution** (every 20s):
  - Simple (0-20s): Single projectiles. Sparse, dim.
  - Spiral (20-40s): Spiral patterns (2/volley). Visible arms.
  - Barred (40-60s): Bar endpoint fire (3/volley). +30% damage.
  - Active (60s+): Rapid barrages + occasional cosmic jet beam. +60% damage. Full complexity.
- **Galaxy Collision**: 2+ galaxies passing through each other → 15 stellar projectiles in all directions.
- **Cosmic Background**: Subtle background star/nebula shift while galaxy active (cosmetic).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              GALACTIC OVERTURE                            │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Galaxy Collision burst rings    │
│           → RippleShader: evolution stage-up flash        │
│  Layer 3: LaserFoundation                               │
│           → ThinBeamShader: cosmic jet beam (Active)     │
│           → Thin beam from galaxy center to target        │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): stellar projectile fire     │
│           → Mode 3 (Spread): spiral/barred volleys       │
│           → Mode 5 (Burst): galaxy collision burst        │
│  Layer 1: MaskFoundation (PRIMARY)                      │
│           → RadialNoiseMaskShader: galaxy disc body      │
│           → CosmicVortex noise for spiral arm rotation   │
│           → CosmicNebula noise for nebula fill            │
│           → Parameterized per evolution stage              │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Galaxy Body: MaskFoundation (PRIMARY — RadialNoiseMaskShader)
| Stage | Noise | scrollSpeed | circleRadius | intensity | Visual |
|-------|-------|------------|-------------|-----------|--------|
| Simple | CosmicVortex (slow) | 0.15 | 0.30 | 1.2 | Dim 2-arm disc |
| Spiral | CosmicVortex (moderate) | 0.25 | 0.35 | 1.6 | Clear spiral arms + detail |
| Barred | CosmicVortex + FBM secondary | 0.3 | 0.40 | 2.0 | Bar structure + spiral arms, brighter |
| Active | CosmicVortex + CosmicNebula | 0.4 | 0.45 | 2.8 | Full galaxy, jets visible, constant fire |

All stages share: `edgeSoftness: 0.08`, Gradient LUT: Nachtmusik_Galaxy_LUT (Night Void → Deep Indigo → Cosmic Blue → Starlight Silver)

#### Cosmic Jet: LaserFoundation (ThinBeamShader — Active stage only)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Width | 10px | Thin focused jet |
| Length | Variable (to target) | Reaches current target |
| Color | Cosmic Blue core → Starlight Silver edge | Cosmic jet energy |
| Duration | 60 frames (1s) per jet burst | Brief sustained beam |
| Interval | Every 8s during Active stage | Periodic jet fire |

#### Galaxy Collision: ImpactFoundation + AttackFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| AttackFoundation Mode 5 | 15 stellar projectiles in all directions | Collision burst |
| ImpactFoundation RippleShader | 8 rings, multi-color Nachtmusik palette, 0.8s | Collision flash rings |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| StellarProjectile | AttackFoundation (Mode 1/3) | Direct/spread fire per evolution stage | Silver streak, 2-layer bloom |
| CosmicJetBeam | LaserFoundation (ThinBeamShader) | Active-stage beam from galaxy center | Thin cosmic blue-silver beam, 1s duration |
| CollisionBurst (x15) | AttackFoundation (Mode 5) | All-direction burst during collision | Mixed stellar projectiles |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| Nachtmusik_Galaxy_LUT | `Assets/Nachtmusik/GalacticOverture/Gradients/` | "Horizontal color gradient strip, void black left through deep indigo through cosmic blue to starlight silver right, galaxy disc energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| GalaxyDiscSprite | `Assets/Nachtmusik/GalacticOverture/Orbs/` | "Miniature spiral galaxy with visible spiral arms and bright center, deep indigo and cosmic blue with starlight silver highlights, on solid black background, game summon entity sprite, 96x96px --ar 1:1 --style raw" |

---

## 11. Conductor of Constellations (Summon)

### Identity & Musical Soul
The ultimate Nachtmusik summoner — you become the conductor of the night sky. This weapon summons constellation entities representing different orchestral sections: Strings (beam attacks), Percussion (AoE slams), Winds (sweeping waves), Brass (heavy bursts). The Orchestral Sync combines all four, and the Finale Performance is a 5-second burst of maximum orchestral power. Every star is an instrument. You are the conductor.

### Lore Line
*"All the stars are instruments. The night sky is the concert hall. You are the conductor."*

### Combat Mechanics
- **Constellation Orchestra** (type cycles each summon):
  - **Strings** (1st): Thin sustained beam attacks (sweeping)
  - **Percussion** (2nd): Ground slam AoE attacks (rhythmic)
  - **Winds** (3rd): Wide wave attacks (sweeping arcs)
  - **Brass** (4th+): Heavy burst projectiles (powerful)
- **Orchestral Sync**: All 4 types present → simultaneous enhanced attack on same target. Massive combined damage.
- **Conductor's Cue**: Right-click marks "soloist target" — all focus with +30% damage.
- **Finale Performance**: After 90s with all 4 types, 5s burst at double speed with enhanced effects. 60s cooldown.
- **Standing Formation**: Idle entities arrange in constellation patterns (cosmetic).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              CONDUCTOR OF CONSTELLATIONS                  │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: AttackAnimationFoundation                     │
│           → Finale Performance screen-wide aura          │
│           → 5s cinematic enhanced combat state            │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Percussion slam AoE rings      │
│           → RippleShader: Orchestral Sync convergence    │
│  Layer 3: LaserFoundation                               │
│           → ThinBeamShader: Strings sustained beams      │
│           → Thin sweeping beams from Strings entities    │
│  Layer 2: AttackFoundation                              │
│           → Mode 1: Winds wave attacks                    │
│           → Mode 5: Brass heavy burst projectiles         │
│           → Mode 3: multi-instrument synchronized fire   │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: constellation bodies  │
│           → StarField noise for star-cluster appearance  │
│           → 4 parameterized variants per instrument       │
│           → PerlinFlow secondary for energy flow          │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Constellation Entity Bodies: MagicOrbFoundation (RadialNoiseMaskShader)
| Instrument | circleRadius | intensity | Shape Modifier | Color Bias |
|-----------|-------------|-----------|---------------|------------|
| Strings | 0.28 (linear arrangement: 3 nodes in line) | 1.6 | Elongated cluster | Starlight Silver dominant |
| Percussion | 0.35 (compact cluster: tight group) | 2.0 | Dense compact | Deep Indigo → Cosmic Blue |
| Winds | 0.30 (spread arrangement: wide spacing) | 1.4 | Dispersed arc | Cosmic Blue dominant |
| Brass | 0.32 (triangular: 3-point formation) | 2.2 | Triangular formation | Cosmic Blue → Starlight Silver |

All share: `noiseTex: StarField, scrollSpeed: 0.2, edgeSoftness: 0.07, PerlinFlow secondary for energy flow`

#### Strings Beam: LaserFoundation (ThinBeamShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Width | 8px | Thin sustained beam (violin-like precision) |
| Length | Variable (sweeping across enemies) | Sustained beam sweep |
| Color | Starlight Silver core → Cosmic Blue edge | String instrument: silver precision |
| Duration | Sustained while attacking | Continuous beam |
| Sweep | Lateral sweep across targets | Bow-stroke motion |

#### Percussion Slam: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Ground slam AoE | 8 rings, Deep Indigo → Cosmic Blue → Starlight Silver, 0.6s |
| Screen shake | Minor shake per slam | Intensity 2, duration 4 frames |

#### Orchestral Sync: ImpactFoundation (combined)
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| Strings beam + Percussion slam + Winds wave + Brass burst | All converge on single target simultaneously | Layered multi-ring impact |
| ImpactFoundation RippleShader | 4 ring groups in different Nachtmusik sub-colors | Each instrument = one ring color |

#### Finale Performance: AttackAnimationFoundation
| Phase | Duration | Visual |
|-------|----------|--------|
| Declaration (Phase 1) | 15 frames | All entities glow brightly, conductor cue flash |
| Performance (Phase 2-3) | 255 frames (4.25s) | Double-speed attacks, enhanced VFX, starfield visible in background |
| Crescendo (Phase 4) | 30 frames | Final massive Orchestral Sync + screen flash |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| StringsBeam | LaserFoundation (ThinBeamShader) | Sustained sweeping beam | Thin silver beam, bow-stroke motion |
| PercussionSlam | ImpactFoundation | Ground AoE slam | 8-ring indigo-silver ripple + screen shake |
| WindsWaveArc | AttackFoundation (Mode 1) | Wide sweeping wave attack | Expanding cosmic arc, Cosmic Blue → Silver |
| BrassBurstShot | AttackFoundation (Mode 5) | Heavy single projectile | Large cosmic bolt, Indigo → Silver, 3-layer bloom |
| OrchestraSyncImpact | All 4 instruments + ImpactFoundation | Combined convergence attack | 4-color layered rings + all attack types simultaneously |
| FinaleScreenEffect | AttackAnimationFoundation | 5s enhanced combat screen state | Starfield background, all entities enhanced |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| ConstellationEntitySprites | `Assets/Nachtmusik/ConductorOfConstellations/Orbs/` | "Four constellation entity variants showing star-cluster formations in different arrangements: linear, compact, spread, and triangular, each made of connected star points in silver-blue cosmic energy, on solid black background, game minion sprite sheet, 256x64px --ar 4:1 --style raw" |
| OrchestraSyncFlare | `Assets/Nachtmusik/ConductorOfConstellations/Flare/` | "Massive multi-layered cosmic convergence point with concentric rings of different blue-silver tones, orchestral cosmic combined energy, on solid black background, game VFX impact texture, 256x256px --ar 1:1 --style raw" |

---

## Cross-Theme Synergy Notes

### Nachtmusik Theme Unity — Foundation Coverage
All 11 weapons built on Foundation Weapons scaffolding with consistent cosmic noise masking:
- **SwordSmearFoundation** (2 weapons): CosmicNebula and FBM noise for heavy/scaling cosmic smear arcs
- **ThinSlashFoundation** (2 weapons): Clean precision katana trails + crescendo wave arcs
- **XSlashFoundation** (1 weapon): Dimensional rift X-slash with CellularCrack spacetime tears
- **MagicOrbFoundation** (6 weapons): RadialNoiseMaskShader with CosmicNebula, StarField, and PerlinFlow for cosmic bodies
- **MaskFoundation** (6 weapons): Persistent zone rendering for constellation fields, nebula residue, galaxy discs, cosmic auras
- **SparkleProjectileFoundation** (3 weapons): 5-layer stellar star projectiles with extended persistence
- **LaserFoundation** (3 weapons): ConvergenceBeamShader harmonic beams + ThinBeamShader cosmic jets/strings
- **RibbonFoundation** (4 weapons): Constellation lines, connection arcs, formation links
- **SmokeFoundation** (1 weapon): Nebula cloud trail rendering
- **ImpactFoundation** (9 weapons): Cosmic ripple rings across nearly all weapons
- **ExplosionParticlesFoundation** (3 weapons): Stellar fragment bursts for cosmic detonations
- **AttackFoundation** (8 weapons): Star projectiles, blade waves, minion attacks, collision bursts
- **AttackAnimationFoundation** (2 weapons): Event Horizon and Finale Performance screen effects

### Noise Texture Strategy — Nocturnal Identity
| Noise Texture | Usage | Cosmic Feel |
|---------------|-------|------------|
| CosmicNebula | Orb interiors, nebula clouds, smear distortion, galaxy fill | Deep space nebula — vast, colorful, swirling |
| CosmicVortex | Singularity pull zones, galaxy spiral rotation | Gravitational phenomena — pulling, spinning, inevitable |
| StarField | Weave nodes, constellation entity bodies, tapestry cells | Star cluster patterns — precise, geometric, stellar |
| TileableFBMNoise | Cosmic cloud auras, secondary organic detail | Organic cosmic drift — living, breathing space |
| PerlinFlow | Singer energy, starlight glow, gentle cosmic flow | Ethereal ambient energy — soft, flowing, luminous |
| CellularCrack | Dimensional rift tears (Twilight Severance only) | Shattered spacetime — cracked, sharp, dimensional |

### Shared Debuff: Celestial Harmony
Multiple melee weapons inflict +10% damage from all Nachtmusik weapons. Creates cross-weapon synergy encouraging mixed Nachtmusik loadouts.

### Visual Distinction
Despite shared palette:
- **Executioner**: Heavy, wide, screen-darkening — cosmic weight
- **Crescendo**: Scaling 0.6x→1.8x visual intensity — building momentum
- **Severance**: Ultra-thin precision, X-slash dimensional rifts — speed
- **Constellation Piercer**: Geometric star-point networks — spatial awareness
- **Nebula's Whisper**: Soft, expanding, accumulating clouds — patience
- **Serenade**: Individual bright stars, romantic curvature — devotion
- **Starweaver**: Interconnected webs of light — intricate complexity
- **Requiem**: Massive supernova/singularity — cosmic devastation
- **Celestial Chorus**: Singing entities, converging beams — musical harmony
- **Galactic Overture**: Evolving galaxy disc, cosmic jets — building grandeur
- **Conductor**: Multi-instrument entities, orchestral convergence — total command