# 🌹 Ode to Joy — Resonance Weapons Planning

> *"Joy, beautiful spark of divinity — every petal, every thorn, every golden note is a celebration of being alive."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Ode to Joy — triumphant, communal, a hymn to universal brotherhood through botanical fury |
| **Emotional Core** | Joy, celebration, triumph of spirit, the garden in full bloom |
| **Color Palette** | Rose shadow, petal pink, bloom gold, radiant amber, jubilant light |
| **Palette Hex** | Rose Shadow `(100, 30, 50)` → Petal Pink `(220, 100, 120)` → Bloom Gold `(255, 200, 50)` → Radiant Amber `(255, 170, 40)` → Jubilant Light `(255, 250, 200)` → Pure Joy White `(255, 255, 240)` |
| **Lore Color** | `new Color(255, 200, 50)` — Warm Gold |
| **Lore Keywords** | Joy, garden, bloom, petal, thorn, vine, seed, rose, triumph, hymn, anthem, chorus, golden, radiant, botanical, harmony, ovation, fountain |
| **VFX Language** | Rose petal cascades, vine wave distortion, thorn eruptions, golden bloom bursts, seed pod detonations, pollen cloud fields, botanical artillery, hymn verse impacts, elysian radiance, chorus harmony waves, standing ovation shockwaves, fountain geyser sprays |

---

## Foundation Weapons Integration Map

| # | Weapon | Class | Primary Foundation | Secondary Foundations | Noise Textures | Key Technique |
|---|--------|-------|-------------------|----------------------|----------------|---------------|
| 1 | Thornbound Reckoning | Melee | SwordSmearFoundation | RibbonFoundation, MaskFoundation, ImpactFoundation, ExplosionParticlesFoundation | TileableFBMNoise (vine distortion), CellularCrack (thorn structure) | FBM-distorted vine smear arcs + noise-masked thorn wall zones |
| 2 | Gardener's Fury | Melee | SwordSmearFoundation | MagicOrbFoundation, AttackFoundation, ExplosionParticlesFoundation, SmokeFoundation | TileableFBMNoise (organic pod texture), PerlinFlow (pollen cloud) | Seed pod orbs with FBM noise + botanical explosion cascades |
| 3 | Rose Thorn Chainsaw | Melee (Held) | RibbonFoundation | SwordSmearFoundation, ImpactFoundation, ExplosionParticlesFoundation, AttackFoundation | TileableFBMNoise (chainsaw fire distortion), VoronoiCell (petal vein) | Continuous ribbon chainsaw body + petal storm smear arcs |
| 4 | Thorn Spray Repeater | Ranged | SparkleProjectileFoundation | AttackFoundation, ImpactFoundation, ExplosionParticlesFoundation, SmokeFoundation | CellularCrack (thorn crystalline), PerlinFlow (bloom mist) | Crystalline thorn projectiles with 5-layer sparkle rendering |
| 5 | The Pollinator | Ranged | SmokeFoundation | AttackFoundation, MaskFoundation, ExplosionParticlesFoundation, ImpactFoundation | PerlinFlow (pollen drift), TileableFBMNoise (golden field) | Pollen smoke clouds + noise-masked golden healing fields |
| 6 | Petal Storm Cannon | Ranged | MaskFoundation | AttackFoundation, ExplosionParticlesFoundation, SmokeFoundation, RibbonFoundation | CosmicVortex (swirling vortex), TileableFBMNoise (storm turbulence) | Noise-masked persistent petal vortex zones + hurricane ribbons |
| 7 | Anthem of Glory | Magic | LaserFoundation | SparkleProjectileFoundation, ImpactFoundation, AttackAnimationFoundation, MaskFoundation | PerlinFlow (beam energy), TileableFBMNoise (crescendo aura) | ConvergenceBeamShader golden anthem beam + sparkle glory notes |
| 8 | Hymn of the Victorious | Magic | MagicOrbFoundation | AttackFoundation, ImpactFoundation, ExplosionParticlesFoundation | TileableFBMNoise (verse energy), VoronoiCell (Gloria fracture) | RadialNoiseMaskShader verse orbs with 4 parameterized styles |
| 9 | Elysian Verdict | Magic | MagicOrbFoundation | MaskFoundation, ImpactFoundation, ExplosionParticlesFoundation | PerlinFlow (pure radiance), TileableFBMNoise (judgment aura) | Radiant orb bodies + noise-masked mark auras + sunburst impacts |
| 10 | Triumphant Chorus | Summon | MagicOrbFoundation | AttackFoundation, RibbonFoundation, ImpactFoundation | PerlinFlow (vocal energy), TileableFBMNoise (harmony field) | Orb-rendered vocal minion bodies + harmony link ribbons |
| 11 | The Standing Ovation | Summon | AttackFoundation | ImpactFoundation, ExplosionParticlesFoundation, SmokeFoundation | PerlinFlow (crowd fog), CellularCrack (shockwave fracture) | Multi-mode applause attacks + rose rain explosion particles |
| 12 | Fountain of Joyous Harmony | Summon | MaskFoundation | AttackFoundation, SparkleProjectileFoundation, ImpactFoundation, ExplosionParticlesFoundation | TileableFBMNoise (water flow), PerlinFlow (harmony zone) | Noise-masked harmony zone + sparkle fountain spray + geyser bursts |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Thornbound Reckoning | Melee | Vine wave arcs + thorn wall zone denial + botanical burst finisher |
| 2 | Gardener's Fury | Melee | Seed pod projectiles + botanical barrage detonation chain |
| 3 | Rose Thorn Chainsaw | Melee (Held) | Continuous chainsaw ribbon + petal storm + thorn fling |
| 4 | Thorn Spray Repeater | Ranged | Rapid crystalline thorn spray + bloom reload burst |
| 5 | The Pollinator | Ranged | Pollen spread + Pollinated debuff + Mass Bloom detonation |
| 6 | Petal Storm Cannon | Ranged | Persistent petal vortex AoE + storm merging + Hurricane Mode |
| 7 | Anthem of Glory | Magic | Channeled golden beam + Glory Notes + Crescendo scaling + Victory Fanfare |
| 8 | Hymn of the Victorious | Magic | 4-verse spell cycle + Complete Hymn super-spell + Encore |
| 9 | Elysian Verdict | Magic | Judgment Marks (3 tiers) + Elysian Verdict explosion + Paradise Lost |
| 10 | Triumphant Chorus | Summon | 4 vocal parts + Harmony bonus + Ensemble synchronized attacks |
| 11 | The Standing Ovation | Summon | Crowd minions + Ovation Meter + rose rain + applause shockwaves |
| 12 | Fountain of Joyous Harmony | Summon | Stationary fountain + Harmony Zone + Joyous Geyser eruptions |

---

## 1. Thornbound Reckoning (Melee)

### Identity & Musical Soul
The opening chord of Ode to Joy's garden — a greatsword wreathed in living thorns that channels nature's triumphant fury. Each swing sends vine waves rippling across the battlefield, and at full combo the sword plants a thorn wall that denies space to enemies. The botanical burst finisher erupts with seeds, petals, and golden pollen in a celebration of destructive growth. This is the garden's judgment: beautiful, thorned, and absolute.

### Lore Line
*"The vine does not ask permission to grow. It simply overcomes."*

### Combat Mechanics
- **3-Phase Botanical Combo**:
  - **Phase 1 — Vine Wave**: Horizontal sweep — spawns a traveling vine wave projectile that damages and slows enemies in its path. Wave leaves thorn residue on the ground (2s).
  - **Phase 2 — Thorn Lash**: Rising diagonal — 2 thorn lash projectiles in a V-pattern. Each lash embeds in enemies, dealing bleed DoT.
  - **Phase 3 — Botanical Burst**: Overhead slam — massive impact spawns expanding thorn wall (arc shape, 10 tiles wide) that persists 4s. Enemies touching the wall take continuous damage + knockback. Thorn wall has visible thorns, roses, and pulsing golden pollen.
- **Reckoning Charge** (0-100): Builds through vine wave hits (+8) and thorn embeds (+12). At full charge, next Phase 3 creates double-width thorn wall + golden botanical burst explosion (8 tile radius AoE).
- **Vine Synergy**: Vine wave residue on ground amplifies thorn wall damage by 25% where they overlap. Rewards combo sequencing.
- **Rose Thorn Bleed**: Embedded thorns deal 3% weapon damage/s for 4s. Max 5 embeds per enemy.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              THORNBOUND RECKONING                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ExplosionParticlesFoundation                  │
│           → Botanical burst petal/seed debris            │
│           → RadialScatter pattern, 55 sparks             │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: vine wave concentric rings     │
│           → DamageZoneShader: thorn wall persistent zone │
│           → SlashMarkShader: thorn lash arc marks        │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: thorn wall zone       │
│           → FBM noise for organic thorn growth pattern   │
│           → CellularCrack noise for thorn structure      │
│  Layer 2: RibbonFoundation                              │
│           → Mode 2 BloomNoiseFade: vine trail ribbons    │
│           → 40-position ring buffer, Additive blend      │
│  Layer 1: SwordSmearFoundation (PRIMARY)                │
│           → SmearDistortShader: vine-distorted arcs      │
│           → distortStrength: 0.06 (organic vine warp)    │
│           → noiseTex: TileableFBMNoise (vine pattern)    │
│           → gradientTex: OdeToJoy botanical LUT          │
│           → 3-layer additive rendering                   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Primary Swing: SwordSmearFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `distortStrength` | 0.06 | Organic vine-wave warping in smear arc |
| `flowSpeed` | 0.35 | Slower flow for creeping vine feel |
| `noiseScale` | 3.0 | Larger scale for thick vine patterns |
| `noiseTex` | TileableFBMNoise | FBM creates organic vine-like distortion |
| `gradientTex` | OdeToJoy_Botanical_LUT | Rose Shadow → Petal Pink → Bloom Gold gradient |
| Blend | Additive | 3-layer rendering: dark vine core → mid bloom → bright petal edge |
| Phase scaling | Phase 1: 0.8x width, Phase 2: 1.0x, Phase 3: 1.4x | Escalating visual intensity |

#### Vine Wave Trail: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 2 (BloomNoiseFade) | Organic fade for vine trail dissipation |
| Ring Buffer | 40 positions | Full vine trail length |
| Width | 18px base, 12px tip | Tapered vine ribbon |
| Color | Bloom Gold core → Petal Pink edge | Warm botanical gradient |
| Blend | Additive | Luminous vine energy |

#### Thorn Wall Zone: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | FBM + CellularCrack dual layer | Organic growth with thorn-crack structure |
| `scrollSpeed` | 0.15 | Slow organic growth animation |
| `circleRadius` | 0.48 | Full zone fill |
| `edgeSoftness` | 0.12 | Soft organic edge (not geometric cutoff) |
| `intensity` | 2.0 | Vivid botanical field |
| Gradient LUT | OdeToJoy_ThornWall_LUT | Rose Shadow (thorns) → Bloom Gold (pollen) → Jubilant Light (bloom) |

#### Impact: ImpactFoundation
| Shader | Usage | Parameters |
|--------|-------|-----------|
| RippleShader | Vine wave concentric rings on swing impact | 8 rings, Rose Shadow → Bloom Gold, 0.6s duration |
| DamageZoneShader | Thorn wall persistent zone indicator | Petal Pink → Bloom Gold breathing pulse, noiseDistortion: 0.04 with FBM |
| SlashMarkShader | Thorn lash arc marks (Phase 2) | arcAngle: 70°, Petal Pink → Radiant Amber |

#### Botanical Burst: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | RadialScatter | Seeds, petals, pollen in radial burst |
| Spark Count | 55 | Dense botanical debris |
| Lifetime | 90 frames | Long-lasting petal drift |
| Colors | Petal Pink, Bloom Gold, Radiant Amber, Jubilant Light | Full botanical palette |
| Gravity | 0.3 (low) | Petals float and drift, not fall |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| VineWaveProjectile | ImpactFoundation (RippleShader) | Travels forward 12 tiles, expanding rings damage + slow | Concentric vine rings, Bloom Gold → Petal Pink, 2s residue trail |
| ThornLashProjectile | AttackFoundation (Mode 2: Spread) | V-pattern, embeds in enemies on contact | CellularCrack-textured thorn sprite, Petal Pink trailing sparks |
| ThornWallSegment | MaskFoundation (persistent) | Stationary 4s, continuous damage + knockback | FBM-masked zone with visible thorn sprites, golden pollen particles |
| BotanicalBurstShard | ExplosionParticlesFoundation | Radial burst from Phase 3 at full charge | Mixed petal/seed sprites, RadialScatter, 8-tile radius |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Rose Thorn Bleed | 3% weapon damage/s, stacks up to 5x | 240 frames (4s), refreshes per embed |
| Vine Root | 40% movement slow from vine wave residue | 120 frames (2s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_Botanical_LUT | `Assets/OdeToJoy/ThornboundReckoning/Gradients/` | "Horizontal color gradient strip, dark rose left through warm gold center to bright cream right, smooth botanical warmth, game color lookup texture, 256x16px, no background detail --ar 16:1 --style raw" |
| VineSmearArc | `Assets/OdeToJoy/ThornboundReckoning/SlashArcs/` | "Sweeping vine slash arc with thorned edges and small rose blooms along the curve, warm gold and petal pink energy on solid black background, game melee VFX texture, 512x256px --ar 2:1 --style raw" |
| ThornWallMask | `Assets/OdeToJoy/ThornboundReckoning/Trails/` | "Dense organic thorn wall pattern with interlocking thorny vines and small golden flowers, white on solid black background, game VFX alpha mask, 256x256px, tileable --ar 1:1 --style raw" |
| BotanicalBurstFlare | `Assets/OdeToJoy/ThornboundReckoning/Flare/` | "Bright botanical burst flare with petal shapes radiating outward and golden pollen center, cream and gold tones on solid black background, game impact VFX, 256x256px --ar 1:1 --style raw" |

---

## 2. Gardener's Fury (Melee)

### Identity & Musical Soul
The gardener's wrath made manifest — a weapon that plants seeds of destruction with every swing. Each strike embeds seed pods in the ground and in enemies, and the gardener can detonate them in a chain of botanical explosions. The fury is patient at first (planting), then devastating (harvest). Growth → bloom → explosion. The garden's fury is not immediate — it is cultivated.

### Lore Line
*"Plant in silence. Harvest in thunder."*

### Combat Mechanics
- **3-Phase Planting Combo**:
  - **Phase 1 — Sow**: Horizontal sweep — embeds 2 SeedPodProjectiles in the ground at swing endpoints. Pods glow faintly.
  - **Phase 2 — Cultivate**: Diagonal strike — embeds 3 pods in a wider arc. Existing ground pods pulse brighter.
  - **Phase 3 — Harvest**: Overhead slam — all embedded pods detonate in sequence (cascade chain, 0.1s between each). Each detonation: 4-tile AoE botanical explosion.
- **Seed pod Types** (cycle with combo repetitions):
  - **Bloom Pod**: Detonates in rose petal burst (damage + brief blind).
  - **Thorn Pod**: Detonates in thorn shrapnel spray (damage + bleed).
  - **Pollen Pod**: Detonates in golden pollen cloud (damage + slow for 3s).
- **Botanical Barrage**: After 3 full combo cycles (9 phases), right-click triggers Botanical Barrage — all pods on field detonate simultaneously regardless of Phase + 8 bonus pods rain from above and detonate on landing. Massive devastation.
- **Growth Acceleration**: Pods left undetonated for 2+ seconds grow — their detonation radius increases by 30% and damage by 20%. Patient gardeners are rewarded.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              GARDENER'S FURY                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: SmokeFoundation                               │
│           → Pollen cloud from pod detonations            │
│           → Style: Billowing (warm gold tint)            │
│           → 30 puffs per detonation cloud                │
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Cascade detonation debris                    │
│           → FountainCascade pattern (upward seed burst)  │
│           → 55 sparks per detonation                     │
│  Layer 3: MagicOrbFoundation                            │
│           → RadialNoiseMaskShader: seed pod bodies       │
│           → FBM noise for organic pod texture            │
│           → circleRadius: 0.38, edgeSoftness: 0.1       │
│  Layer 2: AttackFoundation                              │
│           → Mode 3 (Spread): seed pod firing pattern     │
│           → Botanical Barrage bonus pod rain             │
│  Layer 1: SwordSmearFoundation (PRIMARY)                │
│           → SmearDistortShader: botanical swing arcs     │
│           → distortStrength: 0.05 (organic flow)         │
│           → noiseTex: TileableFBMNoise                   │
│           → gradientTex: OdeToJoy_Gardener_LUT           │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Primary Swing: SwordSmearFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `distortStrength` | 0.05 | Moderate organic flow for gardener sweep |
| `flowSpeed` | 0.4 | Standard flow velocity |
| `noiseScale` | 2.8 | Natural organic pattern scale |
| `noiseTex` | TileableFBMNoise | Organic botanical distortion |
| `gradientTex` | OdeToJoy_Gardener_LUT | Bloom Gold → Radiant Amber → Jubilant Light |
| Phase scaling | Phase 1: 0.7x, Phase 2: 0.9x, Phase 3: 1.3x | Restrained → cultivated → devastating |

#### Seed Pod Bodies: MagicOrbFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Style | FBM (organic pod texture) | Living, pulsing seed appearance |
| `scrollSpeed` | 0.25 | Slow internal organic motion |
| `rotationSpeed` | 0.1 | Gentle rotation (growing) |
| `circleRadius` | 0.38 | Compact pod shape |
| `edgeSoftness` | 0.1 | Organic soft edge |
| `intensity` | 1.8 | Warm glow |
| Gradient LUT | Per pod type: Bloom=Petal Pink, Thorn=Rose Shadow, Pollen=Bloom Gold | Pod type visual identity |
| Growth state | After 2s: radius +30%, intensity +0.5, edgeSoftness +0.05 | Visual growth indication |

#### Cascade Detonation: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | FountainCascade | Seeds/petals erupt upward then scatter |
| Spark Count | 55 per pod detonation | Dense botanical debris |
| Lifetime | 75 frames | Moderate drift time |
| Cascade Delay | 0.1s between sequential detonations | Visible chain reaction timing |
| Colors by Type | Bloom: Petal Pink/Jubilant Light, Thorn: Rose Shadow/Radiant Amber, Pollen: Bloom Gold/Pure Joy White | Type-coded debris |

#### Pollen Cloud: SmokeFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | Billowing | Thick organic cloud |
| Spritesheet | 3x6 (18 frames) pollen cloud animation | Golden pollen dissipation |
| Puffs per Ring | 25 | Dense but not overwhelming |
| Color | Bloom Gold → Radiant Amber with 60% opacity | Warm golden haze |
| Lifetime | 120 frames (2s) | Lingering pollen field |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| BloomSeedPod | MagicOrbFoundation | Embeds in ground/enemy, detonates on Phase 3 or Barrage | FBM-noise orb, Petal Pink core, rose petal burst on detonate |
| ThornSeedPod | MagicOrbFoundation | Same embed behavior, thorn shrapnel on detonate | FBM-noise orb, Rose Shadow core, CellularCrack thorn fragments |
| PollenSeedPod | MagicOrbFoundation | Same embed, golden pollen cloud on detonate | FBM-noise orb, Bloom Gold core, SmokeFoundation cloud on detonate |
| BarrageRainPod | AttackFoundation (Mode 4: Targeted) | Falls from above, detonates on landing | All 3 pod types mixed, downward trajectory, larger detonation |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_Gardener_LUT | `Assets/OdeToJoy/GardenersFury/Gradients/` | "Horizontal color gradient strip, warm gold left through bright amber center to jubilant cream right, botanical warmth, game LUT texture, 256x16px --ar 16:1 --style raw" |
| SeedPodOrb | `Assets/OdeToJoy/GardenersFury/Orbs/` | "Organic glowing seed pod with visible internal energy veins and soft outer membrane, warm gold and green tones on solid black background, game projectile texture, 128x128px --ar 1:1 --style raw" |
| BotanicalExplosionFlare | `Assets/OdeToJoy/GardenersFury/Flare/` | "Explosive botanical burst with petals seeds and pollen radiating outward in a golden starburst pattern, warm amber and pink, on solid black background, game VFX impact texture, 256x256px --ar 1:1 --style raw" |

---

## 3. Rose Thorn Chainsaw (Melee — Held)

### Identity & Musical Soul
Pure botanical aggression — a chainsaw made of interlocking rose thorns that runs continuously while held. The saw body is a ribbon of thorns and roses spinning at furious speed, flinging thorn shrapnel at nearby enemies and periodically erupting in petal storms. This is joy expressed as overwhelming, beautiful violence — a garden that refuses to be tamed.

### Lore Line
*"Every rose has its chainsaw."*

### Combat Mechanics
- **Continuous Chainsaw**: Hold fire — chainsaw runs continuously. Deals rapid contact damage to enemies in front. Consumes mana slowly while held.
- **Thorn Fling**: Every 0.5s while running, flings 2 ThornShrapnelProjectiles at random nearby enemies within 8 tiles. Shrapnel deals moderate damage + embeds (bleed DoT).
- **Petal Storm Buildup**: Continuous operation builds Petal Storm meter (fills in 4s). At full: automatic Petal Storm eruption — 360° burst of razor petals (12 PetalProjectiles in all directions). Meter resets.
- **Revving**: Holding longer increases RPM. At 2s: +20% damage, thicker ribbon trail. At 4s: +40% damage, petal storm triggers, maximum visual intensity.
- **Rose Garden**: Enemies killed by the chainsaw leave a Rose Garden patch (3 tile radius, 5s). Rose Garden heals player for 2 HP/s while standing in it. Beautiful flowers grow from carnage.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              ROSE THORN CHAINSAW                         │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ExplosionParticlesFoundation                  │
│           → Petal Storm 360° eruption                    │
│           → SpiralShrapnel pattern, 55 petals            │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: chainsaw contact impact rings  │
│           → SlashMarkShader: thorn fling arc marks       │
│  Layer 3: AttackFoundation                              │
│           → Mode 2 (Spread): thorn fling projectiles     │
│           → Mode 1 (Direct): petal storm projectiles     │
│  Layer 2: SwordSmearFoundation                          │
│           → SmearDistortShader: petal storm smear arcs   │
│           → distortStrength: 0.07 (violent distortion)   │
│           → noiseTex: VoronoiCell (petal vein pattern)   │
│  Layer 1: RibbonFoundation (PRIMARY)                    │
│           → Mode 2 BloomNoiseFade: chainsaw body ribbon  │
│           → Continuous high-speed ribbon rendering        │
│           → 40-position ring buffer at max RPM            │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Chainsaw Body: RibbonFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 2 (BloomNoiseFade) | Organic thorn texture with noise fade at edges |
| Ring Buffer | 40 positions | Full chainsaw ribbon length |
| Width | 14px (rev 0) → 22px (max RPM) | Widens with rev speed |
| Update Rate | Every frame (continuous weapon) | Smooth continuous ribbon |
| Color | Rose Shadow core → Petal Pink mid → Bloom Gold edge → Radiant Amber tips | Full botanical gradient along ribbon |
| Blend | Additive | Luminous chainsaw energy |
| RPM Visual | Width, brightness, particle density all scale with rev time | Visual rev feedback |

#### Petal Storm Smear: SwordSmearFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `distortStrength` | 0.07 | Aggressive violent distortion for petal eruption |
| `flowSpeed` | 0.6 | Fast chaotic flow |
| `noiseScale` | 2.0 | Tight petal-scale patterns |
| `noiseTex` | VoronoiCell | Creates petal-vein-like distortion structure |
| `gradientTex` | OdeToJoy_PetalStorm_LUT | Petal Pink → Bloom Gold → Pure Joy White |
| Usage | Triggered during Petal Storm eruption only | Burst arc overlay during 360° eruption |

#### Thorn Fling: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 2 (Spread) | Flings thorns at random nearby targets |
| Projectiles per Volley | 2 | Sustained but not overwhelming |
| Interval | 30 frames (0.5s) | Rhythmic fling cadence |
| Bloom layers | Core (Radiant Amber) + glow (Petal Pink) | 2-layer additive glow |

#### Petal Storm Eruption: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | SpiralShrapnel | Petals spiral outward in beautiful pattern |
| Spark Count | 55 | Dense petal burst |
| Lifetime | 60 frames | Moderate petal flight time |
| Colors | Petal Pink, Bloom Gold, Jubilant Light, Pure Joy White | Full bright palette |
| Directional | 12 directions (360° / 12 = 30° spacing) | Even radial coverage |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ThornShrapnelProjectile | AttackFoundation (Mode 2) | Flung at random nearby enemies, embeds on contact | CellularCrack-textured thorn sprite, Rose Shadow trail |
| PetalProjectile | AttackFoundation (Mode 1) | 360° burst, travels 12 tiles outward | Petal sprite with VoronoiCell vein pattern, Petal Pink glow |
| RoseGardenZone | MaskFoundation (passive) | Stationary 5s healing zone from kills | FBM-noise masked flower field, Bloom Gold → Jubilant Light |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Thorn Embed Bleed | 2% weapon damage/s per embed, max 8 embeds | 180 frames (3s), refreshes per embed |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_PetalStorm_LUT | `Assets/OdeToJoy/RoseThornChainsaw/Gradients/` | "Horizontal color gradient strip, warm petal pink left through bright bloom gold center to pure cream white right, floral energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ChainsawBodyRibbon | `Assets/OdeToJoy/RoseThornChainsaw/Trails/` | "Continuous thorned vine chainsaw ribbon with interlocking rose thorns and small bloom accents, warm rose and gold energy on solid black background, game trail VFX texture, 512x64px, tileable horizontal --ar 8:1 --style raw" |
| PetalStormSmear | `Assets/OdeToJoy/RoseThornChainsaw/SlashArcs/` | "360-degree petal storm burst arc with razor rose petals spiraling outward in beautiful destructive pattern, pink and gold on solid black background, game melee VFX texture, 512x512px --ar 1:1 --style raw" |

---

## 4. Thorn Spray Repeater (Ranged)

### Identity & Musical Soul
Rapid-fire botanical artillery — a repeater that sprays crystalline thorns at blistering speed. Each thorn is a tiny jewel of pain, trailing sparkle energy as it flies. The weapon rewards sustained fire with Thorn Accumulation on enemies, and rewards brief pauses with a spectacular Bloom Reload burst that scatters petals and resets the cycle. Fast, rhythmic, and dazzling — a staccato passage of botanical destruction.

### Lore Line
*"A thousand thorns. A thousand tiny joys. A thousand reasons to stay down."*

### Combat Mechanics
- **Rapid Thorn Spray**: Primary fire — fires crystalline thorn projectiles at high speed (12/s). Each thorn deals moderate damage. Tight spread that widens slightly with sustained fire.
- **Thorn Accumulation**: Each thorn hit on the same enemy stacks Thorn Accumulation (max 25). At milestones:
  - 10 stacks: Enemy bleeds (Thorn Bleed DoT)
  - 15 stacks: Enemy slowed 20% (thorns weigh them down)
  - 20 stacks: Enemy takes +15% damage from all sources (thorns compromise defenses)
  - 25 stacks: **Thorn Detonation** — all embedded thorns explode simultaneously (massive burst damage). Resets stacks.
- **Bloom Reload**: After firing 36 thorns (3 seconds of sustained fire), weapon enters Bloom Reload — 1s pause where the repeater flowers bloom open, releasing a burst of healing pollen (heals player 15 HP) + visual petal scatter. After reload: first 6 shots are Bloom Thorns (50% more damage, golden trail).
- **Precision Spray**: Crouching tightens spread dramatically and increases thorn velocity by 20%. Rewards stationary precision.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              THORN SPRAY REPEATER                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: SmokeFoundation                               │
│           → Bloom Reload pollen burst cloud              │
│           → Style: Dispersing (golden pollen)            │
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Thorn Detonation at 25 stacks                │
│           → RadialScatter, 55 crystalline thorn shards   │
│  Layer 3: ImpactFoundation                              │
│           → RippleShader: thorn embed impacts            │
│           → DamageZoneShader: Thorn Detonation AoE       │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): rapid thorn firing          │
│           → 12 projectiles/s sustained fire rate         │
│  Layer 1: SparkleProjectileFoundation (PRIMARY)         │
│           → SparkleTrailShader: crystalline thorn trail  │
│           → CrystalShimmerShader: thorn body shimmer     │
│           → 5-layer rendering, 24-position ring buffer   │
│           → Theme: Botanical Crystal (custom 5-color)    │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Thorn Projectile: SparkleProjectileFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Theme | Custom "Botanical Crystal" | Rose Shadow → Petal Pink → Bloom Gold → Radiant Amber → Jubilant Light |
| SparkleTrailShader | Active | Crystalline sparkle wake behind each thorn |
| CrystalShimmerShader | Active | Faceted shimmer on thorn body |
| Ring Buffer | 24 positions | Full sparkle trail length |
| 5 Visual Layers | Core + inner glow + shimmer + trail + ambient sparkles | Maximum sparkle complexity |
| Bloom Thorn variant | Golden palette override: Bloom Gold core → Pure Joy White | Post-reload enhanced thorns |

#### Rapid Fire: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (Direct) | Straight-line rapid fire |
| Fire Rate | 12/s (every 5 frames) | Sustained barrage |
| Spread | 3° base, +0.5° per 12 shots, reset on reload | Widening spray |
| Bloom Layers | Core (Radiant Amber, scale 0.6) + glow (Bloom Gold, scale 1.2) | 2-layer additive thorn glow |

#### Thorn Detonation: ExplosionParticlesFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ExplosionParticlesFoundation | RadialScatter, 55 sparks, CellularCrack thorn textures | Crystalline thorn shard burst |
| ImpactFoundation DamageZoneShader | 6-tile radius, Petal Pink → Bloom Gold, noiseDistortion: 0.03 with CellularCrack noise | Thorn detonation zone indicator |
| Screen shake | Intensity 4, duration 8 frames | Impact weight |

#### Bloom Reload: SmokeFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | Dispersing | Quick golden pollen burst |
| Puffs | 15 (smaller burst) | Brief healing cloud |
| Color | Bloom Gold → Jubilant Light | Golden healing warmth |
| Lifetime | 45 frames | Quick dissipation |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| CrystallineThornProjectile | SparkleProjectileFoundation | Rapid fire, embeds on contact, stacks accumulation | 5-layer sparkle rendering, CellularCrack texture, Petal Pink trail |
| BloomThornProjectile | SparkleProjectileFoundation (golden variant) | First 6 post-reload, 50% bonus damage | Golden palette override, brighter shimmer, Bloom Gold core |
| ThornDetonationBurst | ExplosionParticlesFoundation | Triggers at 25 stacks, massive radial burst | 55 crystalline shards, RadialScatter, Rose Shadow → Radiant Amber |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Thorn Accumulation | Stacking: 10=bleed, 15=slow, 20=vuln, 25=detonate | Persistent while stacking, resets on detonation |
| Thorn Bleed (10+) | 2% weapon damage/s | 180 frames (3s), refreshes per hit |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| CrystallineThorn | `Assets/OdeToJoy/ThornSprayRepeater/Pixel/` | "Tiny crystalline rose thorn with faceted edges and internal golden glow, botanical crystal jewel, white and gold on solid black background, game pixel particle sprite, 32x32px --ar 1:1 --style raw" |
| BloomReloadFlare | `Assets/OdeToJoy/ThornSprayRepeater/Flare/` | "Soft golden pollen bloom flare with gentle petals opening outward and healing warmth, warm gold and cream on solid black background, game VFX flare texture, 128x128px --ar 1:1 --style raw" |

---

## 5. The Pollinator (Ranged)

### Identity & Musical Soul
A weapon of patient, spreading destruction — the Pollinator fires pollen-laden shots that don't kill immediately but spread the Pollinated debuff like a botanical plague. Pollinated enemies become time bombs: when they die, they explode in a Mass Bloom that spreads pollen to nearby enemies. The golden field left behind heals allies. Joy through contamination — the garden grows through everything it touches.

### Lore Line
*"The pollen does not hate. The pollen simply is. And soon, everything else simply was."*

### Combat Mechanics
- **Pollen Shot**: Primary fire — fires a pollen-laden projectile. On hit: applies Pollinated debuff (enemy glows with golden pollen aura, takes 1% HP/s DoT).
- **Pollen Spread**: Pollinated enemies passively spread pollen to nearby enemies within 4 tiles (applies Pollinated to them after 2s proximity). Chain spreading.
- **Mass Bloom**: When a Pollinated enemy dies, it triggers Mass Bloom — golden explosion (5 tile radius) that applies Pollinated to all enemies caught + fires 3 homing seed projectiles at the nearest non-Pollinated enemies. Chain bloom potential.
- **Golden Field**: Mass Bloom detonation sites become Golden Fields (5s, 3 tile radius). Allies in Golden Fields heal 3 HP/s + gain +5% damage. Fields stack visually but not mechanically.
- **Harvest Season**: After 5 Mass Blooms within 10 seconds, triggers Harvest Season — all Pollinated enemies take 3x DoT for 5s + their next Mass Bloom detonations are doubled in radius. Rewards patient setup.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              THE POLLINATOR                              │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ImpactFoundation                              │
│           → RippleShader: Mass Bloom detonation rings    │
│           → DamageZoneShader: Golden Field indicator      │
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Mass Bloom petal/pollen burst                │
│           → RadialScatter, 55 botanical particles        │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Golden Field zone      │
│           → FBM noise for organic golden field texture    │
│           → PerlinFlow noise for drifting pollen feel     │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): pollen shot firing          │
│           → Mode 4 (Targeted): homing seed projectiles   │
│  Layer 1: SmokeFoundation (PRIMARY)                     │
│           → Pollen cloud rendering on impacts             │
│           → Style: Billowing (golden pollen haze)         │
│           → 30 puffs per cloud, PerlinFlow noise tint     │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Pollen Cloud: SmokeFoundation (PRIMARY)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Style | Billowing | Thick, drifting pollen cloud |
| Spritesheet | 3x6 golden pollen cloud animation | Pollen-specific dissipation |
| Puffs per Cloud | 30 | Dense visible pollen |
| Color | Bloom Gold base → Radiant Amber accents, 70% opacity | Warm golden haze |
| Lifetime | 150 frames (2.5s) | Lingers for spreading mechanic |

#### Golden Field: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | FBM + PerlinFlow dual layer | Organic golden field with drifting pollen |
| `scrollSpeed` | 0.2 | Gentle organic drift |
| `circleRadius` | 0.45 | Zone fill |
| `edgeSoftness` | 0.15 | Very soft organic edge (inviting, not threatening) |
| `intensity` | 1.6 | Warm golden glow (healing field) |
| Gradient LUT | OdeToJoy_GoldenField_LUT | Bloom Gold → Jubilant Light → Pure Joy White |

#### Mass Bloom: ExplosionParticlesFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ExplosionParticlesFoundation | RadialScatter, 55 particles, petal + pollen sprites | Golden bloom explosion |
| ImpactFoundation RippleShader | 6 rings, Bloom Gold → Jubilant Light, 0.8s duration | Expanding golden bloom rings |
| Homing Seeds | AttackFoundation Mode 4, 3 seeds per bloom | Seed projectiles seeking non-Pollinated targets |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| PollenShotProjectile | AttackFoundation (Mode 1) | Direct fire, applies Pollinated on hit | Golden pollen orb, SmokeFoundation trail cloud |
| HomingSeedProjectile | AttackFoundation (Mode 4) | Homing from Mass Bloom, applies Pollinated | Small seed sprite, SparkleProjectileFoundation shimmer |
| GoldenFieldZone | MaskFoundation (persistent) | Stationary 5s healing zone from Mass Bloom | FBM+PerlinFlow masked golden field, healing particle motes |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Pollinated | 1% HP/s DoT, spreads to nearby enemies after 2s, Mass Bloom on death | 600 frames (10s), refreshes on re-application |
| Harvest Season | 3x Pollinated DoT + doubled Mass Bloom radius | 300 frames (5s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_GoldenField_LUT | `Assets/OdeToJoy/ThePollinator/Gradients/` | "Horizontal color gradient strip, bright bloom gold left through jubilant cream center to pure warm white right, healing golden warmth, game LUT texture, 256x16px --ar 16:1 --style raw" |
| PollenCloudSpritesheet | `Assets/OdeToJoy/ThePollinator/Pixel/` | "Golden pollen cloud spritesheet 3x6 grid showing dissipation animation from dense golden cluster to scattered drifting motes, warm gold on solid black background, game particle animation sheet, 384x192px --ar 2:1 --style raw" |
| GoldenFieldMask | `Assets/OdeToJoy/ThePollinator/Trails/` | "Soft organic golden field pattern with drifting pollen particles and gentle flower shapes, white on solid black background, game VFX alpha mask, 256x256px, tileable --ar 1:1 --style raw" |

---

## 6. Petal Storm Cannon (Ranged)

### Identity & Musical Soul
The heavy artillery of the garden — a cannon that fires compressed petal storms. Each shot is a botanical artillery barrage: the petals are razor-sharp, spinning in a vortex of joyful destruction. Persistent vortex AoE zones that merge when overlapping, and at full charge a Hurricane Shot sweeps the entire battlefield. This is the fortissimo moment — beautiful, overwhelming, unstoppable.

### Lore Line
*"The storm does not discriminate. Joy and ruin travel together."*

### Combat Mechanics
- **Petal Storm Barrage**: Primary fire — 3 petal cluster projectiles per shot in a spread. Each cluster explodes on contact into a swirling petal vortex (AoE, 2s persistent).
- **Storm Stacking**: Overlapping petal vortexes merge into larger Storms (radius +50% per merge, max 4x). Massive merged storms devastate groups.
- **Eye of the Storm**: Standing inside your own petal storm grants +8% damage, +5% crit for 3s after leaving. Encourages aggressive positioning.
- **Hurricane Mode**: After 3 consecutive shots, holding fire charges Hurricane Shot (max 2s charge). Fires a single massive petal storm that travels forward, persisting as it moves (not stationary), sweeping across the battlefield.
- **Seasonal Petals**: Petals cycle colors per shot — first: pink, second: gold, third: white. Rainbow of botanical destruction.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              PETAL STORM CANNON                          │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: RibbonFoundation                              │
│           → Mode 5 CosmicNebula (adapted): hurricane     │
│           → ribbon trail behind moving Hurricane Shot     │
│  Layer 4: SmokeFoundation                               │
│           → Storm atmosphere and debris haze              │
│           → Style: Swirling (warm petal tint)             │
│  Layer 3: ExplosionParticlesFoundation                  │
│           → Petal cluster impact burst                    │
│           → SpiralShrapnel pattern (spiraling petals)     │
│  Layer 2: AttackFoundation                              │
│           → Mode 3 (Spread): 3-cluster firing pattern    │
│           → Charged Mode: Hurricane Shot single fire      │
│  Layer 1: MaskFoundation (PRIMARY)                      │
│           → RadialNoiseMaskShader: persistent vortex zone │
│           → CosmicVortex noise (swirling vortex pattern)  │
│           → FBM noise secondary (storm turbulence)        │
│           → Rotation via scrollSpeed for spinning effect   │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Petal Vortex Zone: MaskFoundation (PRIMARY — RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | CosmicVortex (primary) + FBM (secondary) | Swirling vortex with turbulent internal detail |
| `scrollSpeed` | 0.5 | Fast rotation for spinning vortex |
| `rotationSpeed` | 0.4 | Additional rotational animation |
| `circleRadius` | 0.4 (base), grows to 0.48 per merge | Expandable zone |
| `edgeSoftness` | 0.08 | Moderate edge (storm boundary visible) |
| `intensity` | 2.4 (base), +0.3 per merge | Intensifies when storms merge |
| Gradient LUT | Per seasonal color: Pink/Gold/White variations | Color-coded per shot cycle |
| Merge behavior | Adjacent vortexes: combined radius, shared gradient, increased scrollSpeed | Visual storm merging |

#### Hurricane Trail: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 5 (CosmicNebula adapted → PetalNebula) | Swirling nebula-like petal debris trail |
| Ring Buffer | 40 positions | Full hurricane trail |
| Width | 30px (massive hurricane trail) | Hurricane scale |
| Color | All 3 seasonal colors mixed: Petal Pink + Bloom Gold + Pure Joy White | Multi-color hurricane |

#### Impact Burst: ExplosionParticlesFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Pattern | SpiralShrapnel | Petals spiral outward beautifully |
| Spark Count | 40 per cluster impact | Dense petal shower |
| Lifetime | 45 frames | Brief but vivid |
| Colors | Seasonal color of that shot's petals | Color-coded burst |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| PetalClusterProjectile | AttackFoundation (Mode 3) | 3-way spread, detonates on contact | Spinning petal cluster, ExplosionParticlesFoundation on impact |
| PetalVortexZone | MaskFoundation (persistent) | Stationary 2s, continuous AoE damage | CosmicVortex noise-masked spinning zone, seasonal coloring |
| HurricaneShotProjectile | MaskFoundation (moving) + RibbonFoundation trail | Travels forward, persistent AoE along path | Moving vortex zone + petal nebula ribbon trail |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_PetalVortex_LUT | `Assets/OdeToJoy/PetalStormCannon/Gradients/` | "Horizontal color gradient strip, deep rose left through petal pink center through bright bloom gold to pure white right, swirling floral energy, game LUT texture, 256x16px --ar 16:1 --style raw" |
| PetalVortexNoise | `Assets/OdeToJoy/PetalStormCannon/Trails/` | "Swirling vortex pattern with spiral petal shapes embedded in turbulent flow, organic storm pattern, white on solid black background, game VFX noise mask, 256x256px, tileable --ar 1:1 --style raw" |
| HurricaneTrailStrip | `Assets/OdeToJoy/PetalStormCannon/Trails/Clear/` | "Wide flowing hurricane trail strip with scattered rose petals and golden debris in turbulent wind pattern, warm pink gold and white on transparent background, game trail VFX texture, 512x64px --ar 8:1 --style raw" |
---

## 7. Anthem of Glory (Magic)

### Identity & Musical Soul
The Anthem — the core melody of Ode to Joy itself made weapon. This staff channels the pure musical energy of the anthem, creating a sustained golden beam of resonant sound that sways like a conductor's gesture. Glory Notes spawn from the music itself, seeking targets independently. The longer you channel, the louder the crescendo — until the Victory Fanfare silences everything in golden radiance. This is the most directly musical weapon in the theme.

### Lore Line
*"Sing, and the world sings with you. Scream, and the world burns."*

### Combat Mechanics
- **Anthem Beam**: Primary fire — channeled golden beam forward. Continuous damage while held. Beam sways gently side to side (conductor gesture).
- **Glory Notes**: Every 2s of continuous channeling, a Glory Note spawns at a random screen position and fires toward the beam's target. Notes deal bonus damage. Max 6 notes active.
- **Crescendo Channel**: Damage increases the longer you channel (1x → 2x at 5s). Visual: beam intensifies, more notes spawn faster.
- **Anthem's End**: When you stop channeling, the last 1s of beam lingers as a fading afterimage dealing 50% damage. Enemies hit by both beam and echo take +20% bonus.
- **Victory Fanfare**: If channeling kills 3+ enemies within 5s, triggers screen-wide golden flash + all remaining Glory Notes converge simultaneously in a burst of musical energy.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              ANTHEM OF GLORY                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: AttackAnimationFoundation                     │
│           → Crescendo visual escalation system           │
│           → 4-phase brightness/width scaling             │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Victory Fanfare convergence    │
│           → SlashMarkShader: beam endpoint flare         │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: crescendo aura        │
│           → FBM noise for musical energy field            │
│  Layer 2: SparkleProjectileFoundation                   │
│           → Glory Note projectile rendering               │
│           → SparkleTrailShader: golden note trail         │
│           → CrystalShimmerShader: note body shimmer       │
│  Layer 1: LaserFoundation (PRIMARY)                     │
│           → ConvergenceBeamShader: golden anthem beam     │
│           → 4 detail textures, OdeToJoy gradient LUT     │
│           → 100px width, sway animation parameter         │
│           → FlareRainbowShader: beam endpoint flares      │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Anthem Beam: LaserFoundation (PRIMARY — ConvergenceBeamShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Detail Textures | 4: standing wave pattern, musical staff lines, harmonic node highlights, golden energy flow | Musical beam interior |
| Gradient LUT | OdeToJoy_Anthem_LUT | Rose Shadow (edge) → Petal Pink → Bloom Gold → Radiant Amber (core) → Jubilant Light (hot center) |
| Width | 80px base → 120px at max crescendo | Beam widens with crescendo |
| Max Length | 2400px | Full beam reach |
| Sway | Sinusoidal lateral offset: amplitude 8px, frequency 0.5Hz | Conductor gesture sway |
| Endpoint | FlareRainbowShader → FlareGoldenShader (adapted) | Golden bloom flare at beam tip |
| Crescendo scaling | Width: 1.0x→1.5x, brightness: 1.0x→2.0x, LUT shift toward hotter colors | Progressive visual intensification |

#### Glory Notes: SparkleProjectileFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Theme | Custom "Golden Anthem" | Bloom Gold → Radiant Amber → Jubilant Light → Pure Joy White → Bloom Gold |
| SparkleTrailShader | Active | Golden sparkle wake behind each note |
| CrystalShimmerShader | Active | Musical note body shimmer |
| Ring Buffer | 16 positions | Shorter trail for small projectiles |
| Sprite Override | Musical note shape (quarter note) | Music-themed projectile shape |
| Homing | Moderate strength toward beam target | Notes converge on channeled target |

#### Crescendo Aura: MaskFoundation (RadialNoiseMaskShader)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | FBM (musical energy field) | Warm organic energy surrounding caster |
| `scrollSpeed` | 0.3 | Moderate pulsing energy |
| `circleRadius` | 0.35 (base) → 0.48 (max crescendo) | Aura expands with crescendo |
| `intensity` | 1.5 → 2.5 (scales with crescendo) | Brightens during sustained channel |
| Gradient LUT | OdeToJoy_Crescendo_LUT | Bloom Gold → Radiant Amber → Jubilant Light |

#### Victory Fanfare: ImpactFoundation + AttackAnimationFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ImpactFoundation RippleShader | 12 rings, Bloom Gold → Pure Joy White, 1.0s duration, screen-wide | Massive golden ripple expansion |
| AttackAnimationFoundation | Phase 4 (Climax): screen golden flash, 15 frames | Screen-wide fanfare flash |
| Note convergence | All active Glory Notes → cursor position, SparkleProjectileFoundation trails | Musical notes spiral inward for final burst |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| AnthemBeam | LaserFoundation | Channeled continuous beam, sways, crescendo-scaled | ConvergenceBeamShader, 4 musical detail textures, golden LUT |
| GloryNoteProjectile | SparkleProjectileFoundation | Spawns every 2s, homes toward target, max 6 | Musical note shape, golden sparkle trail, shimmer body |
| AnthemEchoBeam | LaserFoundation (faded) | 1s afterimage at 50% opacity/damage | Same beam params at 50% width and brightness |
| VictoryFanfareImpact | ImpactFoundation | Screen-wide golden flash on 3+ kills | 12-ring ripple + screen flash overlay |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_Anthem_LUT | `Assets/OdeToJoy/AnthemOfGlory/Gradients/` | "Horizontal color gradient strip, dark rose edge left through petal pink through bright bloom gold center to radiant amber to jubilant cream right, musical golden warmth, game beam LUT texture, 256x16px --ar 16:1 --style raw" |
| MusicalStaffDetail | `Assets/OdeToJoy/AnthemOfGlory/Beams/` | "Musical staff lines with treble clef and flowing notes as a horizontal beam detail texture, golden notes on dark background, game beam detail texture, 512x64px, tileable horizontal --ar 8:1 --style raw" |
| GloryNoteSprite | `Assets/OdeToJoy/AnthemOfGlory/Pixel/` | "Glowing golden musical quarter note with radiant warmth and soft bloom, warm gold on solid black background, game projectile sprite, 64x64px --ar 1:1 --style raw" |
| VictoryFanfareFlare | `Assets/OdeToJoy/AnthemOfGlory/Flare/` | "Massive golden radiant sunburst with musical note shapes embedded in the rays, triumphant warm gold and cream, on solid black background, game VFX flare texture, 512x512px --ar 1:1 --style raw" |

---

## 8. Hymn of the Victorious (Magic)

### Identity & Musical Soul
A hymn sung by victors — each spell is a verse of triumph. Where Anthem is a continuous beam, Hymn fires discrete powerful verse-shots, each building toward a culminating Complete Hymn super-spell. Four verse types create variety, the Complete Hymn combines all four in devastating unity, and the Encore mechanic rewards kills with sustained high damage. Structured, deliberate, building to a climax — a four-movement symphony for a single weapon.

### Lore Line
*"Each verse is a victory. The final verse is annihilation."*

### Combat Mechanics
- **Hymn Verses**: Primary fire — cycle of 4 unique verse types:
  - **Verse 1 — Exordium**: Gold energy bolt, single target, high damage, piercing.
  - **Verse 2 — Rising**: 3 smaller bolts in fan, moderate damage, applies Jubilant Burn (DoT).
  - **Verse 3 — Apex**: Large orb that hovers at cursor 1s, then detonates in AoE.
  - **Verse 4 — Gloria**: Massive bolt that splits into 6 homing fragments on contact.
- **Complete Hymn**: Firing all 4 in sequence without pause fires ALL 4 simultaneously (combined super-spell).
- **Hymn Resonance**: Enemies hit by 3+ verse types within 5s take +25% magic damage for 4s.
- **Encore**: If Complete Hymn kills an enemy, Hymn resets to Verse 4 (repeated Gloria + Complete Hymn cycles).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              HYMN OF THE VICTORIOUS                      │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ExplosionParticlesFoundation                  │
│           → Complete Hymn combined detonation            │
│           → SpiralShrapnel: 4-color layered burst        │
│  Layer 3: ImpactFoundation                              │
│           → RippleShader: verse impact rings             │
│           → DamageZoneShader: Verse 3 Apex hover zone    │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): V1 Exordium bolt           │
│           → Mode 3 (Spread): V2 Rising fan              │
│           → Mode 4 (Targeted): V4 Gloria homing frags   │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: all verse bodies      │
│           → 4 parameterized LUT variations per verse     │
│           → FBM noise for internal energy                │
│           → VoronoiCell noise for V4 Gloria fracture     │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Verse Orbs: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| Verse | Noise Style | Gradient LUT | circleRadius | intensity | Unique Feature |
|-------|-------------|-------------|--------------|-----------|----------------|
| V1 Exordium | FBM (smooth golden) | Pure Gold → Radiant Amber | 0.40 | 2.0 | Piercing aftertrail, clean lines |
| V2 Rising | FBM (flowing) | Petal Pink → Bloom Gold | 0.35 | 1.8 | 3-way fan spread, warmer |
| V3 Apex | PerlinFlow (hovering) | Bloom Gold → Jubilant Light | 0.48 | 2.5 | Largest orb, hover state, AoE detonation |
| V4 Gloria | VoronoiCell (fracturing) | Radiant Amber → Pure Joy White | 0.43 | 2.8 | Fracture lines visible, splits into 6 fragments |

All verses share:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `scrollSpeed` | 0.3 | Moderate internal motion |
| `rotationSpeed` | 0.15 | Gentle rotation |
| `edgeSoftness` | 0.08 | Clean but soft edge |
| Bloom layers | Core (verse color) + glow (Bloom Gold, 1.5x scale) | 2-layer additive rendering |

#### Complete Hymn: ExplosionParticlesFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ExplosionParticlesFoundation | SpiralShrapnel, 55 sparks in 4 color groups (gold/pink/amber/white) | 4-layered ring explosion representing all verses |
| ImpactFoundation RippleShader | 4 concentric ring colors: Gold → Pink → Amber → White, 1.0s duration | Each ring = one verse detonating |
| Screen shake | Intensity 6, duration 12 frames | Complete Hymn weight |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ExordiumBolt | MagicOrbFoundation + AttackFoundation (Mode 1) | Direct piercing bolt, high damage | FBM gold orb, clean piercing trail |
| RisingBolt (x3) | MagicOrbFoundation + AttackFoundation (Mode 3) | 3-way fan, applies Jubilant Burn | FBM pink-gold orbs, spread pattern |
| ApexOrb | MagicOrbFoundation (large) | Hovers at cursor 1s, then AoE detonation | Largest orb, PerlinFlow, ImpactFoundation detonation |
| GloriaBolt | MagicOrbFoundation (fracturing) | Splits into 6 homing fragments on contact | VoronoiCell fracture orb, fragments use AttackFoundation Mode 4 |
| GloriaFragment (x6) | AttackFoundation (Mode 4) | Homing fragments from Gloria split | Smaller FBM orbs, golden sparkle trail |
| CompleteHymnSuper | All 4 verses combined | Simultaneous launch of all verse types | 4-verse visual layers combined, massive impact |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Jubilant Burn | 4% weapon damage/s DoT from Verse 2 Rising | 240 frames (4s), refreshes |
| Hymn Resonance | +25% magic damage taken (3+ verse types landed) | 240 frames (4s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_HymnVerse_LUT_Set | `Assets/OdeToJoy/HymnOfTheVictorious/Gradients/` | "Four horizontal color gradient strips stacked vertically: pure gold top, petal pink second, warm amber third, brilliant white bottom, each smooth and distinct, game LUT texture set, 256x64px --ar 4:1 --style raw" |
| VerseOrbFlare | `Assets/OdeToJoy/HymnOfTheVictorious/Flare/` | "Brilliant warm golden orb flare with internal energy veins radiating outward, triumphant radiant energy, on solid black background, game VFX flare texture, 128x128px --ar 1:1 --style raw" |

---

## 9. Elysian Verdict (Magic)

### Identity & Musical Soul
Elysium — paradise. But to reach paradise, one must be judged worthy. This weapon delivers the Elysian Verdict: a judgment of pure golden light that purifies and burns simultaneously. Three tiers of Judgment Marks build to the devastating Elysian Verdict detonation, and the Paradise Lost mode at low HP corrupts the golden radiance into dark crimson-gold for maximum damage. The most powerful magic weapon in the Ode to Joy arsenal — joy as divine judgment.

### Lore Line
*"Elysium's gates open only for those the light deems worthy. None have been worthy."*

### Combat Mechanics
- **Elysian Judgment**: Primary fire — golden light orb with prismatic edges. On hit: applies Elysian Mark.
- **Judgment Tiers** (Elysian Marks stack, max 3):
  - 1 Mark: Target glows faintly gold. +10% magic damage taken.
  - 2 Marks: Target glows stronger. +20% magic damage + Elysian Burn DoT.
  - 3 Marks: **Elysian Verdict** — massive golden explosion, heavy AoE + heals player 10% of damage dealt.
- **Elysian Radiance**: While equipped, player emits soft golden aura (5 tile radius). Allies gain +3% damage.
- **Worthy Judge**: Critical hits apply 2 marks instead of 1.
- **Paradise Lost**: Below 25% HP, energy corrupts — orbs turn dark gold with crimson edges, +50% damage, but Verdict healing becomes 0.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              ELYSIAN VERDICT                             │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ExplosionParticlesFoundation                  │
│           → Elysian Verdict sunburst particles           │
│           → RadialScatter, 55 golden + prismatic shards  │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Verdict explosion rings         │
│           → DamageZoneShader: mark aura indicator         │
│  Layer 3: MaskFoundation                                │
│           → RadialNoiseMaskShader: Elysian Radiance aura │
│           → PerlinFlow noise for pure radiant energy      │
│           → Paradise Lost: FBM noise for corrupted aura  │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): Elysian orb firing          │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: Elysian orb body      │
│           → PerlinFlow noise for pure golden radiance     │
│           → Prismatic edge shimmer via edgeSoftness       │
│           → Paradise Lost: VoronoiCell corrupted texture  │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Elysian Orb: MagicOrbFoundation (PRIMARY — RadialNoiseMaskShader)
| State | Noise Style | Gradient LUT | intensity | Unique Feature |
|-------|-------------|-------------|-----------|----------------|
| Normal | PerlinFlow (pure, clean radiance) | Bloom Gold → Radiant Amber → Jubilant Light → Pure Joy White | 2.4 | Prismatic rainbow edge shimmer (edgeSoftness: 0.04, tight) |
| Paradise Lost | VoronoiCell (corrupted fractures) | Rose Shadow → Petal Pink → Dark Gold → Crimson Edge | 3.0 | Crimson crack lines visible, aggressive distortion |

Common parameters:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `scrollSpeed` | 0.35 | Active internal motion |
| `rotationSpeed` | 0.2 | Gentle rotation |
| `circleRadius` | 0.43 | Standard orb size |
| `edgeSoftness` | 0.04 (normal), 0.12 (Paradise Lost) | Normal: tight prismatic edge. PL: rough corrupted edge |

#### Judgment Mark Aura: MaskFoundation (RadialNoiseMaskShader)
| Mark Tier | circleRadius | intensity | Gradient LUT |
|-----------|-------------|-----------|-------------|
| 1 Mark | 0.25 | 1.0 | Faint Bloom Gold → transparent |
| 2 Marks | 0.35 | 1.6 | Bloom Gold → Radiant Amber → Jubilant Light |
| 3 Marks (pre-detonation) | 0.45 | 2.8 | Full OdeToJoy gold → brilliant white, pulsing 2Hz |

#### Elysian Verdict Explosion: ImpactFoundation + ExplosionParticlesFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ImpactFoundation RippleShader | 10 rings, Bloom Gold → Jubilant Light → Pure Joy White, 1.2s | Expanding golden sunburst rings |
| ExplosionParticlesFoundation | RadialScatter, 55 sparks: golden + prismatic rainbow shards | Sunburst particle debris |
| Heal flow particles | Custom: 8 green-gold motes homing toward player | Visual healing indicator |
| Screen shake | Intensity 8, duration 15 frames | Verdict impact weight |
| Screen flash | Bloom Gold overlay, 10 frames, 40% opacity | Brief golden divine flash |

#### Elysian Radiance: MaskFoundation (passive aura)
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | PerlinFlow (gentle radiance) | Soft ambient golden field |
| `scrollSpeed` | 0.1 | Very slow, peaceful |
| `circleRadius` | 0.48 | Full aura fill |
| `edgeSoftness` | 0.2 | Very soft edge (ambient, not aggressive) |
| `intensity` | 1.0 | Subtle background aura |
| Gradient LUT | Bloom Gold → Jubilant Light (very faded) | Warm golden ambient |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ElysianOrbProjectile | MagicOrbFoundation | Direct fire, applies Elysian Mark on hit | PerlinFlow golden orb, prismatic edge, 2-layer bloom |
| ElysianOrbCorrupted | MagicOrbFoundation (Paradise Lost) | Same behavior, +50% damage, no heal | VoronoiCell fracture orb, dark gold-crimson |
| ElysianVerdictExplosion | ImpactFoundation | Triggers at 3 marks, massive AoE + heal | 10-ring golden ripple + sunburst particles |
| HealFlowMote | Custom (no foundation) | Homes from detonation site to player | Green-gold sparkle, curved arc path |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Elysian Mark (1) | +10% magic damage taken, faint gold glow | 600 frames (10s), stacks to 3 |
| Elysian Mark (2) | +20% magic damage + Elysian Burn DoT | 600 frames, refreshes |
| Elysian Mark (3) | Triggers Elysian Verdict explosion | Instant detonation |
| Elysian Burn | 3% max HP/s DoT | 300 frames (5s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_Elysian_LUT | `Assets/OdeToJoy/ElysianVerdict/Gradients/` | "Horizontal color gradient strip, bright bloom gold left through radiant amber through jubilant cream to pure brilliant white right, divine golden radiance, game LUT texture, 256x16px --ar 16:1 --style raw" |
| ElysianVerdictSunburst | `Assets/OdeToJoy/ElysianVerdict/Flare/` | "Massive divine golden sunburst explosion with extending radiant rays and prismatic rainbow edges, triumphant judgment energy, on solid black background, game VFX impact texture, 512x512px --ar 1:1 --style raw" |
| ElysianMarkGlyph | `Assets/OdeToJoy/ElysianVerdict/Orbs/` | "Simple golden judgment glyph circle with inner radiance and tiered glow rings, clean divine symbol, warm gold on solid black background, game debuff indicator, 64x64px --ar 1:1 --style raw" |

---

## 10. Triumphant Chorus (Summon)

### Identity & Musical Soul
A chorus of triumph — multiple vocal spirit minions singing in harmony. Each minion represents a different vocal part (soprano, alto, tenor, bass), and together they create harmonies that damage enemies through pure sound energy. The chorus grows stronger with more voices, and the Ensemble synchronized attack creates devastating musical convergence. Community as power — joy is stronger together.

### Lore Line
*"Alone, a voice. Together, a world remade."*

### Combat Mechanics
- **Chorus Minion**: Summons floating musical spirits firing golden sound waves at enemies.
- **Vocal Parts** (each additional summon adds a different part):
  - 1: Soprano (fast, light attacks)
  - 2: + Alto (medium attacks with slow)
  - 3: + Tenor (strong attacks with knockback)
  - 4+: + Bass (heavy AoE ground pound)
- **Harmony Bonus**: All 4 vocal parts active = +20% damage + golden resonance fields at enemy positions.
- **Ensemble Attacks**: Every 10s, all minions fire synchronized volley at same target (1.5x damage).
- **Standing Ovation Sync**: If The Standing Ovation summon is also active, both gain +15% damage (cross-summon synergy).

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              TRIUMPHANT CHORUS                           │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: Ensemble attack convergence    │
│           → DamageZoneShader: resonance field indicator  │
│  Layer 3: RibbonFoundation                              │
│           → Mode 1 PureBloom: harmony links between      │
│             singers during Harmony state                  │
│  Layer 2: AttackFoundation                              │
│           → Mode 1 (Direct): sound wave attacks          │
│           → Mode 5 (Burst): Ensemble synchronized volley │
│  Layer 1: MagicOrbFoundation (PRIMARY)                  │
│           → RadialNoiseMaskShader: vocal minion bodies   │
│           → PerlinFlow noise for ethereal singing energy │
│           → 4 parameterized variants per vocal part      │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Vocal Minion Bodies: MagicOrbFoundation (RadialNoiseMaskShader)
| Vocal Part | circleRadius | intensity | Gradient LUT Bias | Unique Feature |
|-----------|-------------|-----------|-------------------|----------------|
| Soprano | 0.30 (smallest) | 2.2 (brightest) | Jubilant Light → Pure Joy White | Rapid internal scroll, compact |
| Alto | 0.36 | 1.8 | Bloom Gold → Radiant Amber | Warm amber tones, medium size |
| Tenor | 0.40 | 2.0 | Radiant Amber → Bloom Gold | Strong presence, golden body |
| Bass | 0.45 (largest) | 1.6 (deeper) | Rose Shadow → Petal Pink → Bloom Gold | Largest, deepest color range |

All share: `scrollSpeed: 0.25, rotationSpeed: 0.15, edgeSoftness: 0.1, noiseTex: PerlinFlow`

#### Harmony Links: RibbonFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (PureBloom) | Clean luminous lines between singers |
| Width | 4px | Thin harmonic connections |
| Color | Bloom Gold → Jubilant Light | Golden harmony energy |
| Visibility | Only during Harmony state (all 4 parts active) | Visual Harmony indicator |

#### Sound Wave Attacks: AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Mode | 1 (Direct) per singer | Individual targeting |
| Projectile Shape | Expanding arc (sound wave) | Musical wave shape |
| Color | Per vocal part (see table above) | Part-coded attacks |
| Ensemble Mode | Mode 5 (Burst): all fire simultaneously | Synchronized volley |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| SoundWaveProjectile | AttackFoundation (Mode 1) | Arc shape, travels toward target | Expanding golden arc, per-part coloring |
| EnsembleConvergence | AttackFoundation (Mode 5) + ImpactFoundation | All minions fire simultaneously, convergence burst | 4-color wave convergence, RippleShader impact |
| HarmonyResonanceField | MaskFoundation (brief) | Spawns at enemy positions during Harmony | FBM-masked golden zone, 1s lifetime |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| VocalMinionSprite | `Assets/OdeToJoy/TriumphantChorus/Orbs/` | "Ethereal golden singing spirit entity with visible sound waves emanating from its form, warm luminous humanoid silhouette made of musical energy, on solid black background, game minion sprite, 64x64px --ar 1:1 --style raw" |
| SoundWaveArc | `Assets/OdeToJoy/TriumphantChorus/Trails/` | "Expanding golden sound wave arc with internal musical vibration lines, warm gold energy arc on solid black background, game projectile VFX texture, 128x64px --ar 2:1 --style raw" |

---

## 11. The Standing Ovation (Summon)

### Identity & Musical Soul
The audience erupts — standing ovation! This summon conjures phantom spectator minions that attack with applause shockwaves, thrown roses, and overwhelming adoration. The Ovation Meter builds toward a climactic event where the entire crowd rises for a devastating synchronized shockwave and rose rain. Joyful absurdity — being attacked by an appreciative audience. Joy as collective violent enthusiasm.

### Lore Line
*"The audience loved the performance. The audience demands an encore."*

### Combat Mechanics
- **Ovation Minion**: Summons phantom spectators attacking with:
  - **Applause Wave** (every 3s): ranged shockwave from clapping
  - **Thrown Rose**: tracking rose projectile that applies Thorned debuff
  - **Standing Rush**: charges at low HP enemies (<20%)
- **Crowd Size**: Multiple summons = larger crowd. +5% damage per additional member.
- **Ovation Meter**: Kills build meter. Full meter: Standing Ovation Event — massive synchronized shockwave + rose rain for 3s.
- **Encore**: Re-summoning within 5s of Standing Ovation Event grants 2 minions for 1 slot (stacks once).
- **Triumphant Chorus Sync**: Cross-summon +15% damage with Chorus minions.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              THE STANDING OVATION                        │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 4: SmokeFoundation                               │
│           → Crowd energy atmospheric fog                  │
│           → Style: Dispersing (warm golden tint)          │
│  Layer 3: ExplosionParticlesFoundation                  │
│           → Rose rain during Standing Ovation Event       │
│           → FountainCascade pattern (roses from above)    │
│  Layer 2: ImpactFoundation                              │
│           → RippleShader: applause shockwave rings        │
│           → SlashMarkShader: standing rush charge mark    │
│  Layer 1: AttackFoundation (PRIMARY)                    │
│           → Mode 2 (Spread): applause wave attacks        │
│           → Mode 4 (Targeted): thrown rose homing         │
│           → Mode 1 (Direct): standing rush charge         │
│           → Multi-mode minion attack system               │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Minion Attacks: AttackFoundation (PRIMARY — multi-mode)
| Attack | Mode | Parameters | VFX |
|--------|------|-----------|-----|
| Applause Wave | Mode 2 (Spread) | 3-way arc pattern, interval 3s per minion | Expanding golden arc (adapted SoundWave shape), Bloom Gold |
| Thrown Rose | Mode 4 (Targeted) | Tracking rose projectile, applies debuff | Red-gold rose sprite, curved arc trajectory |
| Standing Rush | Mode 1 (Direct) | Charge at low-HP enemy, high damage | Gold streak trail, RibbonFoundation Mode 1 behind charging minion |

#### Applause Shockwave: ImpactFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| RippleShader | 4 rings, Bloom Gold → Jubilant Light, 0.4s duration | Fast expanding applause ring |
| Multiple overlapping | During crowd attacks, multiple shockwaves overlap | Dense crowd energy |

#### Standing Ovation Event: ExplosionParticlesFoundation + SmokeFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ExplosionParticlesFoundation | FountainCascade, 55 rose sprites raining from above | Rose rain shower |
| ImpactFoundation RippleShader | 8 rings, all Bloom Gold → Pure Joy White, screen-wide | Massive synchronized shockwave |
| SmokeFoundation | Dispersing style, 15 puffs, Bloom Gold, 60 frames | Crowd energy fog during event |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| ApplauseWaveProjectile | AttackFoundation (Mode 2) + ImpactFoundation | 3-way expanding arcs, fast | Golden arc shockwaves, RippleShader rings |
| ThrownRoseProjectile | AttackFoundation (Mode 4) | Arcing homing rose, applies Thorned | Red rose sprite, curved trajectory, Petal Pink trail |
| StandingRushCharge | AttackFoundation (Mode 1) | Direct charge at low-HP enemy | Gold streak, RibbonFoundation trail |
| RoseRainPetal | ExplosionParticlesFoundation | Falls from above during event | Rose petal sprites, FountainCascade pattern |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Thorned | 2% weapon damage/s bleed from thrown roses | 180 frames (3s) |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| PhantomSpectatorSprite | `Assets/OdeToJoy/TheStandingOvation/Orbs/` | "Semi-transparent golden phantom audience member silhouette clapping with visible enthusiasm and golden energy, cheering crowd spirit, on solid black background, game minion sprite, 48x64px --ar 3:4 --style raw" |
| ThrownRoseSprite | `Assets/OdeToJoy/TheStandingOvation/Pixel/` | "Beautiful red and gold rose with slight golden glow being thrown gracefully, game thrown projectile sprite, on solid black background, 32x32px --ar 1:1 --style raw" |
| RoseRainPetal | `Assets/OdeToJoy/TheStandingOvation/Pixel/` | "Falling rose petal in warm pink and gold with gentle tumbling motion blur, game particle sprite, on solid black background, 16x16px --ar 1:1 --style raw" |

---

## 12. Fountain of Joyous Harmony (Summon)

### Identity & Musical Soul
A grand fountain — centerpiece of the garden of joy, spraying liquid harmony upward in eternal celebration. This stationary summon continuously heals allies and damages enemies with joyous golden energy. The water is liquid joy — golden light that rises in beautiful arcs and falls as healing rain. The Harmony Zone around the fountain amplifies all allies, and the periodic Joyous Geyser eruption is a spectacular vertical burst of pure celebration.

### Lore Line
*"Where the fountain flows, joy follows. Where joy flows, nothing can stand against it."*

### Combat Mechanics
- **Joyous Fountain Minion**: Stationary fountain at player's position:
  - Heals allies within 15 tiles (5 HP/s)
  - Fires arcing golden droplet projectiles at enemies (homing)
  - Creates Harmony Zone (10 tile radius): +8% all damage for allies
- **Fountain Tiers** (additional summons upgrade, not multiple fountains):
  - Tier 1: Base healing and damage
  - Tier 2: +3 HP/s, +20% droplet damage
  - Tier 3: +5 HP/s (13 total), droplets pierce, Harmony Zone +5 tiles
  - Tier 4+: +2 HP/s and +10% damage per tier
- **Joyous Geyser**: Every 15s, massive golden spray — damages all enemies within 20 tiles + heals allies 30 HP instant.
- **Fountain Relocation**: Alt fire moves fountain to player position (5s cooldown).
- **Harmony Aura**: Allies within 3 tiles of fountain gain +15% damage + triple healing rate.

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────────┐
│              FOUNTAIN OF JOYOUS HARMONY                  │
│              Foundation Stack                            │
├─────────────────────────────────────────────────────────┤
│  Layer 5: ExplosionParticlesFoundation                  │
│           → Joyous Geyser spray particles                │
│           → FountainCascade pattern (upward burst)        │
│           → 55 golden droplet sparks                      │
│  Layer 4: ImpactFoundation                              │
│           → RippleShader: geyser eruption rings           │
│           → DamageZoneShader: Harmony Aura inner zone     │
│  Layer 3: SparkleProjectileFoundation                   │
│           → Golden droplet projectile rendering           │
│           → SparkleTrailShader: droplet sparkle trail     │
│           → Parabolic arc trajectory                      │
│  Layer 2: AttackFoundation                              │
│           → Mode 4 (Targeted): homing droplet firing      │
│           → Continuous autonomous attack pattern           │
│  Layer 1: MaskFoundation (PRIMARY)                      │
│           → RadialNoiseMaskShader: Harmony Zone           │
│           → FBM noise for flowing water energy field      │
│           → PerlinFlow noise secondary (golden drift)     │
│           → Tier-scaled radius and intensity              │
└─────────────────────────────────────────────────────────┘
```

### VFX Architecture — Foundation-Based

#### Harmony Zone: MaskFoundation (PRIMARY — RadialNoiseMaskShader)
| Tier | circleRadius | intensity | Additional |
|------|-------------|-----------|-----------|
| Tier 1 | 0.40 | 1.4 | Base warm golden field |
| Tier 2 | 0.42 | 1.6 | Slightly brighter |
| Tier 3 | 0.48 | 1.8 | Extended radius + visible water stream lines |
| Tier 4+ | 0.48 + 0.02/tier | 1.8 + 0.2/tier | Progressive enhancement |

Common parameters:
| Parameter | Value | Purpose |
|-----------|-------|---------|
| Noise Mode | FBM (primary) + PerlinFlow (secondary) | Flowing water energy with golden drift |
| `scrollSpeed` | 0.2 | Gentle flowing animation |
| `edgeSoftness` | 0.18 | Very soft inviting edge |
| Gradient LUT | OdeToJoy_Harmony_LUT | Bloom Gold → Jubilant Light → Pure Joy White |

#### Golden Droplets: SparkleProjectileFoundation + AttackFoundation
| Parameter | Value | Purpose |
|-----------|-------|---------|
| AttackFoundation Mode | 4 (Targeted) | Homing toward enemies |
| SparkleProjectileFoundation Theme | "Joyous Water" | Bloom Gold → Radiant Amber → Jubilant Light → Pure Joy White → Bloom Gold |
| Trajectory | Parabolic arc (fountain spray) | Water-like arcing path |
| Ring Buffer | 16 positions | Shorter sparkle trail |
| Fire Rate | 1 droplet/s base, scales with tier | Continuous fountain spray |

#### Joyous Geyser: ExplosionParticlesFoundation + ImpactFoundation
| Component | Parameters | Purpose |
|-----------|-----------|---------|
| ExplosionParticlesFoundation | FountainCascade, 55 golden droplet sparks, upward bias | Vertical geyser spray |
| ImpactFoundation RippleShader | 6 rings, Bloom Gold → Pure Joy White, 1.0s, expanding from fountain base | Geyser eruption base rings |
| Screen shake | Intensity 3, duration 6 frames | Moderate geyser weight |
| Vertical column | RibbonFoundation Mode 1 (PureBloom), vertical, 20 frames | Geyser column of light |

### Sub-Projectiles (Foundation-Based)
| Projectile | Foundation | Behavior | VFX |
|------------|-----------|----------|-----|
| GoldenDropletProjectile | SparkleProjectileFoundation + AttackFoundation (Mode 4) | Arcing homing droplet, continuous fire | Golden sparkle droplet, parabolic arc, 5-layer rendering |
| JoyousGeyserBurst | ExplosionParticlesFoundation + ImpactFoundation | 15s interval vertical burst, 20-tile damage | FountainCascade upward spray + golden ripple rings |
| HarmonyZoneField | MaskFoundation (persistent) | Stationary zone around fountain, tier-scaled | FBM+PerlinFlow golden field, soft inviting edge |

### Asset Requirements
| Asset | Location | Midjourney Prompt |
|-------|----------|-------------------|
| OdeToJoy_Harmony_LUT | `Assets/OdeToJoy/FountainOfJoyousHarmony/Gradients/` | "Horizontal color gradient strip, warm bloom gold left through jubilant cream center to pure warm white right, healing fountain warmth, game LUT texture, 256x16px --ar 16:1 --style raw" |
| FountainEntitySprite | `Assets/OdeToJoy/FountainOfJoyousHarmony/Orbs/` | "Ornate golden garden fountain with water arcing upward in graceful streams, warm gold and cream tones, classical garden centerpiece, on solid black background, game summon entity sprite, 64x96px --ar 2:3 --style raw" |
| GoldenDropletSprite | `Assets/OdeToJoy/FountainOfJoyousHarmony/Pixel/` | "Tiny golden water droplet with internal warm glow and gentle sparkle, liquid joy, on solid black background, game particle sprite, 16x16px --ar 1:1 --style raw" |
| GeyserColumnStrip | `Assets/OdeToJoy/FountainOfJoyousHarmony/Trails/` | "Vertical golden water geyser column with rising spray and internal golden light, fountain eruption energy, on solid black background, game VFX trail texture, 64x512px --ar 1:8 --style raw" |

---

## Cross-Theme Synergy Notes

### Ode to Joy Theme Unity — Foundation Coverage
All 12 weapons built on Foundation Weapons scaffolding with consistent noise masking:
- **SwordSmearFoundation** (2 weapons): FBM vine/botanical distortion for organic melee arcs
- **MagicOrbFoundation** (4 weapons): RadialNoiseMaskShader with FBM/PerlinFlow/VoronoiCell for verse orbs, minion bodies, and judgment orbs
- **MaskFoundation** (5 weapons): Persistent zone rendering with FBM+PerlinFlow dual noise for thorn walls, golden fields, petal vortexes, and harmony zones
- **SparkleProjectileFoundation** (3 weapons): Crystalline thorn projectiles, glory notes, and fountain droplets
- **LaserFoundation** (1 weapon): ConvergenceBeamShader for Anthem of Glory's channeled beam
- **ImpactFoundation** (8 weapons): RippleShader and DamageZoneShader across nearly all weapons for botanical impacts
- **ExplosionParticlesFoundation** (9 weapons): RadialScatter, FountainCascade, and SpiralShrapnel for botanical debris
- **RibbonFoundation** (4 weapons): Vine trails, chainsaw body, harmony links, hurricane trails
- **SmokeFoundation** (4 weapons): Pollen clouds, storm atmosphere, crowd fog
- **AttackFoundation** (8 weapons): Multi-mode firing patterns across all weapon classes

### Noise Texture Strategy — Botanical Identity
| Noise Texture | Usage | Botanical Feel |
|---------------|-------|---------------|
| TileableFBMNoise | Vine distortion, pod texture, field zones, storm turbulence | Organic growth, living botanical energy |
| PerlinFlow | Pollen drift, golden radiance, harmony fields, vocal energy | Soft flowing warmth, gentle beauty |
| CellularCrack | Thorn structure, crystalline thorn projectiles | Sharp thorned edges within organic softness |
| VoronoiCell | Petal vein patterns, fracture lines (Gloria/Paradise Lost) | Organic cellular structure, destructive beauty |
| CosmicVortex | Petal storm vortex zones | Swirling destructive botanical storms |

### Cross-Weapon Synergies (Foundation Interactions)
- **Thornbound + Gardener**: Both use SwordSmearFoundation with FBM noise — vine wave residue amplifies seed pod detonation zones
- **Triumphant Chorus + Standing Ovation**: Both use AttackFoundation — cross-summon +15% damage bonus, harmonized attack patterns
- **Pollinator + any AoE weapon**: SmokeFoundation pollen clouds spread Pollinated; MaskFoundation golden fields from Mass Bloom support all allies
- **Fountain of Harmony**: MaskFoundation Harmony Zone universally buffs all allies — cornerstone support

### Musical Motifs
- **Joy through combat**: Weapons celebrate destruction — petals, roses, golden light, fountains, applause. Exuberant, not grim.
- **Growth and nurture**: SmokeFoundation pollen + MaskFoundation fields + MagicOrbFoundation seed pods — botanical growth as combat power.
- **Musical structure**: LaserFoundation anthem beam crescendo, MagicOrbFoundation verse system, vocal chorus harmony — combat as musical performance.
- **Community**: AttackFoundation multi-minion systems, MaskFoundation zone buffs, RibbonFoundation harmony links — joy is communal.
- **Rose petal signature**: ExplosionParticlesFoundation with SpiralShrapnel/RadialScatter petal debris appears across nearly every weapon — the signature visual of Ode to Joy.