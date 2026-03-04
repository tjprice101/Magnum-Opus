# 🔥 Dies Irae — Resonance Weapons Planning

> *"Day of wrath — the world consumed in fury and fire."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Verdi's Dies Irae from the Requiem — primal fury, divine judgment, apocalyptic wrath |
| **Emotional Core** | Fury, judgment, apocalyptic power |
| **Color Palette** | Blood red, dark crimson, ember orange |
| **Palette Hex** | Char Black `(20, 5, 5)` → Blood Crimson `(140, 15, 15)` → Wrath Red `(220, 40, 20)` → Ember Orange `(255, 120, 30)` → Judgment Gold `(255, 200, 80)` → Hellfire White `(255, 240, 220)` |
| **Lore Color** | `new Color(200, 50, 30)` — Blood Red |
| **Lore Keywords** | Wrath, judgment, sin, condemnation, fire, chain, verdict, damnation, penance, inferno |
| **VFX Language** | Raging flames, chains of judgment, verdict glyphs, crystallized wrath, eclipse darkness with fire edges, molten cracks, infernal sigils |

---

## Foundation Weapons Integration Map

Every Dies Irae weapon MUST build its VFX systems on top of existing Foundation Weapons. Below is the master mapping of which Foundations each weapon uses as scaffolding. **No weapon should implement rendering from scratch** — it extends and re-themes Foundation systems.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Wrath's Cleaver, Executioner's Verdict, Chain of Judgment | Swing arc smear overlays with SmearDistortShader — re-themed to crimson/ember gradient LUT with heavy FBM noise distortion |
| **RibbonFoundation** | Wrath's Cleaver, Chain of Judgment, Eclipse of Wrath | Trail rendering — Mode 6 (Energy Surge) for Cleaver trails, Mode 10 (Lightning) for Chain lightning, Mode 5 (Cosmic Nebula) for Eclipse corona |
| **ThinSlashFoundation** | Executioner's Verdict | Razor-thin verdict slash marks at judgment strikes — ThinSlashShader SDF line in crimson/gold |
| **XSlashFoundation** | Executioner's Verdict, Eclipse of Wrath | Cross-shaped verdict execution impact — XSlashShader fire distortion in blood-crimson/gold; Eclipse orb split detonation |
| **ImpactFoundation** | All 12 weapons | Impact VFX on hit — RippleShader for expanding wrath rings, DamageZoneShader for hellfire ground zones, SlashMarkShader for judgment slash scars |
| **ExplosionParticlesFoundation** | Damnation's Cannon, Eclipse of Wrath, Grimoire of Condemnation | Massive detonation sparks — RadialScatter for hellfire explosions, FountainCascade for judgment fire rain, SpiralShrapnel for eclipse shard bursts |
| **SmokeFoundation** | Wrath's Cleaver, Damnation's Cannon, Staff of Final Judgement | Heavy crimson-black smoke — 30-puff rings themed InfernalEmber and CrimsonVeil for infernal atmosphere |
| **SparkleProjectileFoundation** | Sin Collector, Arbiter's Sentence | Bullet/projectile rendering — SparkleTrailShader + CrystalShimmerShader for Sin bullets, judgment flame shots with 5-layer visual depth |
| **LaserFoundation** | Grimoire of Condemnation | Channeled condemnation wave — ConvergenceBeamShader with crimson-gold detail textures, widening cone rendering |
| **ThinLaserFoundation** | Arbiter's Sentence, Chain of Judgment | Precision beam effects — ThinBeamShader for focused judgment flame trails, chain lightning arcs |
| **InfernalBeamFoundation** | Sin Collector (Damnation Shot), Grimoire of Condemnation (Dark Sermon) | Devastating full-screen beams — InfernalBeamBodyShader with FBM noise distortion for the ultimate punishment effects |
| **MaskFoundation** | Eclipse of Wrath, Staff of Final Judgement, Death Tolling Bell | RadialNoiseMaskShader for eclipse orb bodies (Voronoi + FBM combo), floating ignition mines, toll wave centers |
| **MagicOrbFoundation** | Eclipse of Wrath, Staff of Final Judgement, Damnation's Cannon | Orb projectile bodies with RadialNoiseMaskShader — eclipse spheres, ignition mines, wrath balls |
| **AttackAnimationFoundation** | Executioner's Verdict (Verdict Execution), Wrathful Contract (Blood Sacrifice) | Cinematic execution sequences — camera control, screen darkening, multi-slash judgment cuts |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Wrath's Cleaver | Melee | Crystallized flame projectiles + wrath buildup |
| 2 | Executioner's Verdict | Melee | Judgment-themed heavy strikes + verdict system |
| 3 | Chain of Judgment | Melee | Chain-whip with binding judgment chains |
| 4 | Sin Collector | Ranged | Sin-absorbing bullets that collect enemy power |
| 5 | Damnation's Cannon | Ranged | Heavy cannon lobbing ignited wrath balls |
| 6 | Arbiter's Sentence | Ranged | Judgment flame precision shots |
| 7 | Staff of Final Judgement | Magic | Floating ignition projectiles (fire mines) |
| 8 | Grimoire of Condemnation | Magic | Dark condemnation spells |
| 9 | Eclipse of Wrath | Magic | Eclipse orbs splitting into wrath shards |
| 10 | Death Tolling Bell | Summon | Bell minion with toll wave attacks |
| 11 | Harmony of Judgement | Summon | Judgment sigil minion |
| 12 | Wrathful Contract | Summon | Wrath demon bound by contract |

---

## 1. Wrath's Cleaver (Melee)

### Identity & Musical Soul
Dies Irae opens with a thunderous, wrathful declaration — this cleaver IS that opening. Raw, brutal, unrefined wrath condensed into a blade. Every swing cleaves the air and leaves crystallized flame in its wake — wrath so intense it solidifies. The weapon doesn't ask questions. It passes judgment through violence.

### Lore Line
*"The first blow of wrath is always the loudest."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  WRATH'S CLEAVER — Foundation Architecture          │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: MeleeSwingItemBase + MeleeSwingBase    │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Heavy wrath smear per combo phase          │  │
│  │  → distortStrength: 0.10 (raw / brutish)      │  │
│  │  → flowSpeed: 0.6 base → 0.9 at Wrath max    │  │
│  │  → noiseTex: TileableFBMNoise (fire churn)    │  │
│  │  → gradientTex: DiesIrae LUT (char→crimson    │  │
│  │    →ember→gold→hellfire white)                 │  │
│  │  → 3-layer: outer 1.15x, main, core 0.85x    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → 40-point ring buffer heavy trail           │  │
│  │  → EnergySurgeBeam UV-scroll for fire body    │  │
│  │  → Width: 32f head → 6f tail (thick, heavy)   │  │
│  │  → Colors: blood crimson → ember orange        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZoneShader)           │  │
│  │  → Crystallized Flame ground hazard zones     │  │
│  │  → noiseTex: FBMNoise (rolling fire)          │  │
│  │  → gradientTex: DiesIrae LUT                  │  │
│  │  → circleRadius: 0.40, edgeSoftness: 0.06    │  │
│  │  → breathe: linked to Wrathfire damage tick   │  │
│  │  → 180-frame (3s) persistence                 │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (InfernalEmber style)         │  │
│  │  → 30-puff smoke ring per heavy swing         │  │
│  │  → body: blood crimson, core: ember orange,   │  │
│  │    edge: char black (inverted — dark edges)   │  │
│  │  → Dense black smoke billowing from blade     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Wrath shockwave on Phase 3 spin attack    │  │
│  │  → ringCount: 5, ringThickness: 0.06         │  │
│  │  → Heavy screen shake on trigger              │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → WrathMeter HUD system (0-100 Wrath gauge)       │
│  → CrystallizedFlame sub-projectile (DamageZone)   │
│  → WrathUnleashed: carpet of DamageZones forward    │
│  → Per-phase intensity scaling on all foundations   │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **3-Phase Wrath Combo**:
  - **Phase 1 — Accusation**: Heavy horizontal cleave. Spawns 2 CrystallizedFlame projectiles (arcing, gravity-affected) that arc upward then crash down, each using **ImpactFoundation DamageZoneShader** for ground persistence. On-hit: enemies ignited with Wrathfire Tier 1 (20 damage/s for 4s).
  - **Phase 2 — Conviction**: Overhead slam creating ground eruption. Spawns 4 crystallized flames in a spread pattern. **SmokeFoundation** erupts 30-puff ring at slam point. On-hit: Wrathfire stacks to Tier 2 (40 damage/s).
  - **Phase 3 — Execution**: Spinning cleave (270° arc). **SwordSmearFoundation** at max distortion (0.12). Spawns 6 crystallized flames in ring around player. **ImpactFoundation RippleShader** expands from player center. On-hit: Wrathfire Tier 3 (60 damage/s + -20% enemy healing). Screen shake.
- **Wrath Meter**: Each hit builds Wrath (max 100). All Foundation parameters scale with Wrath: SwordSmear `distortStrength` += Wrath * 0.0003, RibbonFoundation width += Wrath * 0.1, DamageZone `intensity` += Wrath * 0.01. At 100 Wrath, next swing becomes **Wrath Unleashed**.
- **Wrath Unleashed**: Massive cleave with double-width SwordSmear (1.4x `BladeLength`). Carpets forward with 8 DamageZone crystallized flames in a line. **ExplosionParticlesFoundation** RadialScatter (55 sparks) at impact point. Wrath resets to 0. Decays at -5/s when not hitting.

### VFX Architecture — Foundation-Based

#### Swing Arc → SwordSmearFoundation (SmearDistortShader)
- `noiseTex`: TileableFBMNoise (aggressive, churning fire distortion)
- `gradientTex`: DiesIrae LUT — Char Black → Blood Crimson → Ember Orange → Judgment Gold → Hellfire White
- `distortStrength`: Phase 1 = 0.08, Phase 2 = 0.10, Phase 3 = 0.12, Wrath Unleashed = 0.15
- `flowSpeed`: 0.5 base, scales +0.4 at max Wrath
- `noiseScale`: 3.0 (coarser than default 2.5 for more aggressive churn)
- 3-layer rendering: outer glow (1.15x scale, 40% opacity) → main smear → bright core (0.85x, 100% opacity)
- **Masking Enhancement**: Apply secondary FBM noise mask over outer layer — `smoothstep(0.3, 0.7, noiseSample)` erodes edges into ragged fire wisps, preventing clean silhouette edges. The smear should look torn apart by its own fury.

#### Trail → RibbonFoundation (Mode 6: Energy Surge)
- 40-point ring buffer, `RibbonWidthHead = 32f`, `RibbonWidthTail = 6f`
- EnergySurgeBeam texture UV-scrolled at 1.8x base speed (aggressive flow)
- Colors: blood crimson body → ember orange edge → char black smoke fringe
- Width function: `sin(progress * PI) * baseWidth * (1 + wrathMeter * 0.005)`
- **Additional layer**: Under the main ribbon, draw a secondary pass with SmokeFoundation's CPU noise erosion (Mode 2: BloomNoiseFade) in char black — creates a dark underlayer that makes the fire ribbon pop against blackness.

#### Crystallized Flame Ground → ImpactFoundation (DamageZoneShader)
- `noiseTex`: FBMNoise (rolling fire simulation)
- `gradientTex`: DiesIrae LUT
- `scrollSpeed`: 0.35 (faster than default 0.2 for visible fire motion)
- `rotationSpeed`: 0.15 (slow rotational churn)
- `circleRadius`: 0.40, `edgeSoftness`: 0.06, `intensity`: 2.5
- `breathe`: linked to Wrathfire damage tick — pulses bright on each damage application
- 6 orbiting edge sparkles (PointBloom from ImpactFoundation) in ember orange
- **Noise Masking on edges**: Boost `edgeSoftness` to 0.10 and increase noise scroll speed so the edges visibly roil with fire — not a clean circle but a burning, shifting boundary.
- Secondary outer glow layer: SoftGlow at 1.3x zone radius, blood crimson, 20% opacity — hellish ambient illumination.

#### Wrath Shockwave → ImpactFoundation (RippleShader)
- `ringCount`: 5, `ringThickness`: 0.06
- `noiseTex`: PerlinNoise for ring edge distortion
- Colors: Ember orange → Wrath red → Blood crimson fade
- Scale expansion: 0.08 → 0.6 over 60 frames (EaseOutQuad)
- 10px screen shake on trigger, decaying ×0.85/frame

#### Smoke Eruption → SmokeFoundation (InfernalEmber style)
- 30-puff ring per Phase 2 slam and Wrath Unleashed
- Colors: body = Char Black `(20,5,5)`, core = Ember Orange `(255,120,30)`, edge = Blood Crimson `(140,15,15)`
- Heavy inverted palette — dark smoke with fire cores, reads as billowing hellsmoke
- Scale accelerated decay (×0.96 instead of ×0.975) — violent, fast dissipation
- Per-puff glow accent (SoftGlow at 0.6× puff scale, ember orange, fading quickly)

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| CrystallizedFlame | **ImpactFoundation** (DamageZoneShader) | Persistent fire zone (3s), arcing flight, ground latch |
| WrathShockwave | **ImpactFoundation** (RippleShader) | 5-ring shockwave, Phase 3 only |
| WrathUnleashedCarpet | **ImpactFoundation** (DamageZone × 8) | 8 zones in a forward line, each 3s |
| WrathUnleashedBurst | **ExplosionParticlesFoundation** (RadialScatter) | 55 sparks, ember + crimson 4-color palette |
| WrathSmokeRing | **SmokeFoundation** (InfernalEmber) | 30 puffs, Phase 2 + Unleashed |

### Debuffs

| Debuff | Effect | Duration |
|--------|--------|----------|
| Wrathfire Tier 1 | 20 damage/s. Visual: ember particles via single-frame SoftGlow + GlowOrb sparks. | 240 frames (4s) |
| Wrathfire Tier 2 | 40 damage/s. Visual: above + ImpactFoundation mini-Ripple every tick. | 240 frames (4s) |
| Wrathfire Tier 3 | 60 damage/s + -20% enemy healing. Visual: DamageZoneShader wrapping the enemy model. | 300 frames (5s) |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| DiesIrae LUT (gradient) | `Assets/DiesIrae/WrathsCleaver/Gradients/` | "Horizontal color gradient strip, leftmost black transitioning through deep blood crimson then bright ember orange then golden amber to white-hot on rightmost edge, 256x16px, game asset texture, solid black background, clean sharp color transitions --ar 16:1 --style raw" |
| Crystallized Flame Sprite | `Assets/DiesIrae/WrathsCleaver/Orbs/` | "Crystallized flame shard, angular faceted fire crystal with internal orange-red glow, jagged edges, dark crimson core with ember orange highlights, game item sprite, 64x64px, on solid black background --ar 1:1 --style raw" |
| Dense Fire Smoke Trail | `Assets/DiesIrae/WrathsCleaver/Trails/` | "Dense fire smoke trail texture strip, dark black smoke with bright ember orange fire cores breaking through, horizontal strip for UV scrolling, 256x64px, seamless horizontal tile, on solid black background --ar 4:1 --style raw" |

---

## 2. Executioner's Verdict (Melee)

### Identity & Musical Soul
The executioner — impartial, inevitable, final. This weapon doesn't rage; it **judges**. Methodical heavy strikes that mark enemies with judgment glyphs. When the verdict is sealed, the punishment is absolute. Controlled wrath, channeled through ritual and ceremony.

### Lore Line
*"The verdict was written before you were born."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  EXECUTIONER'S VERDICT — Foundation Architecture    │
├─────────────────────────────────────────────────────┤
│  BASE CLASS: MeleeSwingItemBase + MeleeSwingBase    │
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Methodical, deliberate swing arcs          │  │
│  │  → distortStrength: 0.06 (controlled, not     │  │
│  │    chaotic — this is judicial precision)       │  │
│  │  → gradientTex: DiesIraeVerdict LUT (blood    │  │
│  │    crimson → judgment gold → sharp white edge) │  │
│  │  → Embedded glyph texture in smear arc        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader SDF)     │  │
│  │  → Verdict slash marks — each phase impact    │  │
│  │  → CrimsonSlice style (140,15,15 edge →       │  │
│  │    255,200,80 mid → 255,240,220 core)         │  │
│  │  → lineWidth: 0.024 (wider than default for   │  │
│  │    heavy cleaving judgment)                    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (XSlashShader)               │  │
│  │  → Verdict Execution cross-impact at 3 marks  │  │
│  │  → fireIntensity: 0.04 (controlled burn)      │  │
│  │  → scrollSpeed: 0.2 (slow, deliberate)        │  │
│  │  → Colors: blood crimson → gold → white       │  │
│  │  → ShaderDrawScale: 0.30 (massive X)          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (SlashMarkShader)            │  │
│  │  → Judgment Mark scars on marked enemies      │  │
│  │  → slashWidth: 0.08, slashLength: 0.40       │  │
│  │  → noiseTex: CosmicVortex (swirling glyph)   │  │
│  │  → Persistent on enemy for mark duration      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Judgment Glyph rendering on marked enemies │  │
│  │  → noiseTex: VoronoiCell (fractured sigil)    │  │
│  │  → circleRadius: 0.35 → 0.45 with marks      │  │
│  │  → Intensity scales from dim to blinding      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Verdict Execution) │  │
│  │  → Cinematic execution at 3 Judgment Marks    │  │
│  │  → Camera pan to target → X-slash → screen    │  │
│  │    darkening → bright gold-crimson flash       │  │
│  │  → 110-frame sequence with progressive bloom  │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Judgment Mark stacking visual (1-3 sigil layers)│
│  → Executioner's Eye HP overlay (crimson bar)      │
│  → Execute threshold glow (<25% HP enemies)        │
│  → Ritual Precision combo reset reward             │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **3-Phase Judgment Combo**:
  - **Phase 1 — Arraignment**: Overhead strike. **SwordSmearFoundation** at `distortStrength = 0.06`. Applies Judgment Mark — **MaskFoundation** renders a Voronoi-cracked sigil on the enemy. First mark = outer ring only.
  - **Phase 2 — Cross-Examination**: Cross-slash (X pattern via **ThinSlashFoundation** double-slash spawned at ±45°). Marked enemies take +25% damage. **ImpactFoundation SlashMarkShader** scars the target. Applies second Mark — MaskFoundation adds inner glyph layer.
  - **Phase 3 — The Verdict**: Horizontal execution slash. Double-marked enemies trigger Verdict Execution via **AttackAnimationFoundation** (cinematic camera + **XSlashFoundation** cross-impact + screen darkening). Triple-marked enemies get automatic execution regardless of phase.
- **Verdict System**: **MaskFoundation** renders layered sigils — 1 mark: outer VoronoiCell ring (`intensity = 1.5`). 2 marks: + inner FBM glyph layer (`intensity = 2.0`). 3 marks: full sigil blazing (`intensity = 3.0`) + orbiting ember sparks + screen edge vignette.
- **Executioner's Eye**: Player sees enemy HP % as a crimson bar. Enemies below 25% HP emit a **MaskFoundation** glow overlay (`VoronoiEdge` noise, blood crimson, `intensity = 1.0`). +50% damage to these targets.
- **Ritual Precision**: All 3 phases hitting same target without miss → instant combo reset. Reward is a brief **ImpactFoundation RippleShader** pulse (gold, 3 rings, tight, fast — satisfying feedback).

### VFX Architecture — Foundation-Based

#### Swing Arc → SwordSmearFoundation (SmearDistortShader)
- `noiseTex`: TileableFBMNoise (controlled, not chaotic)
- `gradientTex`: DiesIraeVerdict LUT — Blood Crimson → Judgment Gold → Hellfire White (sharper transitions than Wrath's Cleaver)
- `distortStrength`: 0.06 constant (controlled fury, not escalating)
- `flowSpeed`: 0.35 (slower, more deliberate than Cleaver's 0.5)
- `noiseScale`: 2.0 (finer detail for cleaner smear)
- **Key difference from Wrath's Cleaver**: The smear has sharp, clean edges — no additional noise erosion on outer layer. This executioner's blade cuts clean. But within the smear body, embed a judgment glyph texture via UV-mapped overlay (faint gold scales-of-justice pattern visible inside the arc).

#### Judgment Mark Glyph → MaskFoundation (RadialNoiseMaskShader)
- Drawn on enemy as a persistent overlay
- `noiseTextures` change with mark count:
  - 1 Mark: VoronoiCell (outer ring only, blood crimson, low intensity 1.5)
  - 2 Marks: VoronoiCell outer + FBMNoise inner layer (gold highlights, intensity 2.0)
  - 3 Marks: Full VoronoiCell + FBM + StarField combo (blazing gold-white, intensity 3.0)
- `circleRadius`: 0.30 at 1 mark → 0.45 at 3 marks (sigil grows)
- `edgeSoftness`: 0.04 (sharp boundary — judgment is absolute)
- `scrollSpeed`: 0.1 at 1 mark → 0.3 at 3 marks (accelerating intensity)
- **Noise-on-noise masking**: The inner glyph layer uses FBMNoise as the primary shape, but applies a secondary VoronoiCell mask over it with `smoothstep(0.25, 0.75, voronoi)` — creates a fractured, cracked sigil look like engraved judgment runes shattering through the enemy.

#### Verdict Impact → ThinSlashFoundation (ThinSlashShader SDF)
- Style: CrimsonSlice variant — `edgeColor = (140,15,15)`, `midColor = (255,200,80)`, `coreColor = (255,240,220)`
- `lineWidth`: 0.024, `lineLength`: 0.50 (wider and longer for heavy judicial cuts)
- 3-layer directional bloom: SoftGlow outer, mid, core in crimson-gold
- Spawned in pairs at ±45° during Phase 2 Cross-Examination

#### Verdict Execution → AttackAnimationFoundation + XSlashFoundation
- **AttackAnimationFoundation** provides the cinematic sequence:
  - Camera pan from player to marked target (15 frames)
  - **XSlashFoundation** cross-slash (XSlashShader with `fireIntensity = 0.04`, `scrollSpeed = 0.2`, gold-crimson gradient)
  - Screen darkening overlay (not brightening — this is a solemn execution, not celebration)
  - Single powerful gold-crimson flash at execution moment
  - Camera return (25 frames)
- **XSlashFoundation** parameters: `ShaderDrawScale = 0.30`, 6-frame fade-in + 20 hold + 30 fadeOut
- Post-execution: **ImpactFoundation RippleShader** expanding ring (gold → crimson → transparent, 4 rings)
- Brief Verdict Stun applied (60 frames)

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| JudgmentSlash | **ThinSlashFoundation** (CrimsonSlice) | Paired ±45° slashes, Phase 2 |
| VerdictExecution | **XSlashFoundation** + **AttackAnimationFoundation** | Cinematic X-impact at 3 marks |
| JudgmentMark | **MaskFoundation** (VoronoiCell) | Persistent enemy overlay, 1-3 layers |
| RitualResetPulse | **ImpactFoundation** (RippleShader) | Brief gold 3-ring pulse feedback |

### Debuffs

| Debuff | Effect | Duration |
|--------|--------|----------|
| Judgment Mark (1-3) | +25% damage per mark from this weapon. At 3: auto-Execution. Visual: MaskFoundation layers. | 360 frames (6s) per mark |
| Verdict Stunned | Brief stun from Execution. Visual: screen-edge crimson vignette. | 60 frames (1s) |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| DiesIraeVerdict LUT | `Assets/DiesIrae/ExecutionersVerdict/Gradients/` | "Horizontal color gradient strip, leftmost deep blood crimson transitioning sharply through rich judgment gold to pure white at rightmost edge, hard transitions not soft blending, 256x16px, game asset, solid black background --ar 16:1 --style raw" |
| Judgment Glyph Texture | `Assets/DiesIrae/ExecutionersVerdict/Orbs/` | "Ancient judgment sigil glyph, scales of justice motif with ornate runic border, glowing crimson and gold lines on solid black background, thin line art style, game VFX overlay texture, 128x128px --ar 1:1 --style raw" |

---

## 3. Chain of Judgment (Melee)

### Identity & Musical Soul
Chains bind sinners to their fate — this weapon IS that chain. A chain-whip melee weapon with extended reach that latches onto enemies, binding them. The chains are judgment incarnate — once bound, the target cannot escape their sentence. Unique whip-like melee behavior.

### Lore Line
*"No sinner escapes the chain. It finds them in the dark."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CHAIN OF JUDGMENT — Foundation Architecture        │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 10: Lightning)         │  │
│  │  → Chain body rendering as segmented trail    │  │
│  │  → LightningSurge texture for arcing links    │  │
│  │  → Jitter offsets simulate chain rattling     │  │
│  │  → Width: 16f head → 8f tail (thick chain)   │  │
│  │  → Aggressive bloom overlay per-segment       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → Swing arc for the chain whip motion        │  │
│  │  → Wider arc (200°) for whip sweep            │  │
│  │  → distortStrength: 0.07 (metallic grind)     │  │
│  │  → gradientTex: DiesIraeChain LUT (iron       │  │
│  │    grey → ember orange → judgment gold)        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Chain Lightning arc between enemies        │  │
│  │  → Up to 3 bounces (ricochet enabled)         │  │
│  │  → ember orange → white at branch points      │  │
│  │  → Width: 6px, brief 0.3s lifetime            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (SpiralShrapnel) │  │
│  │  → Chain shrapnel on Fully Bound death        │  │
│  │  → Metal-themed sparks: iron grey + ember     │  │
│  │  → 40 sparks, tangential velocity spread      │  │
│  │  → Line type sparks for metal debris look     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Chain impact ring on each latch            │  │
│  │  → Tight rings (ringCount: 3)                 │  │
│  │  → Iron grey → ember orange → gold            │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Chain Link rendering (segmented, not smooth)    │
│  → Binding tether system (player ↔ enemy)          │
│  → Chain Link Stack counter (0-5)                  │
│  → Judgment Pull yank animation                    │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Chain Whip Swing**: Sweeping chain whip (12 tile reach). **SwordSmearFoundation** renders the arc. **RibbonFoundation** Mode 10 Lightning renders the chain body — jittering lightning segments give the impression of individual chain links rattling. Each link segment between ring buffer nodes = one chain link.
- **Binding Chain**: On latch: **ImpactFoundation RippleShader** at contact (3 tight rings, metallic colors). Tethered enemies connected by **ThinLaserFoundation** beam (iron + ember colors). Enemy cannot move beyond chain length from player. Swing tethered enemy into others for collision damage.
- **Judgment Pull**: Alt fire yanks chain — **ThinLaserFoundation** beam briefly brightens to white. Enemy dragged toward player. Path collision damages enemies in their trajectory.
- **Chain Link Stacking**: Rapid hits build Chain Links (max 5). Visual: each stack adds an orbiting ember spark around target (via SoftGlow + GlowOrb from ImpactFoundation per-node sparkle pattern). At 5: Fully Bound — immobilized 2s, +30% damage taken.
- **Chain Shrapnel**: Fully Bound enemies who die: **ExplosionParticlesFoundation** SpiralShrapnel (40 sparks) with metal-themed debris + **SmokeFoundation** 15-puff mini smoke burst (iron grey/ember).
- **Chain Lightning**: At full combo, next hit arcs **ThinLaserFoundation** beam to 3 nearby enemies. Ricochet enabled (ring buffer path bounces). Each bounce: `alpha × 0.85` decrease. Prioritizes bound enemies.

### VFX Architecture — Foundation-Based

#### Chain Body → RibbonFoundation (Mode 10: Lightning)
- 40-position ring buffer with deliberate **jitter offsets** — each segment displaced ±3px perpendicular to create "rattling chain links" feel
- LightningSurge texture UV-scrolled aggressively (2.0x speed)
- Width: 16f → 8f (thick, heavy chain feel)
- Colors: dark iron `(60,55,50)` body → ember orange `(255,120,30)` inter-link glow
- Aggressive bloom overlay per segment: SoftGlow at 0.02 scale, ember orange, cycling intensity (sin-wave pulse simulating heat between links)
- **Noise masking on edges**: Apply FBM noise erosion to the ribbon alpha using CPU-side sin-based hash (BloomNoiseFade technique from Mode 2). This breaks the ribbon into distinct link-shaped segments rather than a smooth strip — crucial for the chain aesthetic.

#### Chain Lightning → ThinLaserFoundation (ThinBeamShader)
- Per-segment `VertexStrip` with ricochet support (up to 3 bounces)
- `gradientTex`: DiesIraeChain LUT
- `baseColor`: ember orange, brightening to white at junction points
- Width: 6px, brief 25-frame lifetime
- Bounce flares: Additive GlowOrb + StarFlare at each bounce point (from DrawBounceFlares)
- `alpha` decreases per bounce: `1 → 0.85 → 0.70 → 0.55`

#### Chain Shrapnel → ExplosionParticlesFoundation (SpiralShrapnel)
- 40 sparks, tangential velocity (chain links flying outward in spiral)
- Spark types: 70% Line (metal shards), 20% Star (ember sparks), 10% Dot (hot points)
- 4-color palette: Iron Grey `(80,75,70)`, Ember Orange `(255,120,30)`, Judgment Gold `(255,200,80)`, Char Black `(20,5,5)`
- Light gravity (0.01-0.06) — shrapnel hangs in the air briefly
- 90-frame lifetime

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| ChainBody | **RibbonFoundation** (Mode 10: Lightning) | Segmented chain whip body |
| ChainLightningArc | **ThinLaserFoundation** (ThinBeamShader) | Ricochet beam to 3 targets |
| ChainShrapnelBurst | **ExplosionParticlesFoundation** (SpiralShrapnel) | 40 metal sparks on Fully Bound death |
| BindingImpactRing | **ImpactFoundation** (RippleShader) | 3-ring metallic contact pulse |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| DiesIraeChain LUT | `Assets/DiesIrae/ChainOfJudgment/Gradients/` | "Horizontal color gradient strip, leftmost dark iron grey transitioning through red-hot ember orange to bright judgment gold at rightmost, metallic heated metal feel, 256x16px, game asset, solid black background --ar 16:1 --style raw" |
| Chain Link Texture | `Assets/DiesIrae/ChainOfJudgment/Trails/` | "Repeating chain link texture strip for UV scroll, dark iron chain links with glowing orange heat between them, sharp metallic detail, horizontal seamless tile, 256x32px, on solid black background --ar 8:1 --style raw" |

---

## 4. Sin Collector (Ranged)

### Identity & Musical Soul
Every enemy carries sins — this weapon **collects** them. Each bullet absorbs a fragment of the enemy's essence. Collected sins power the weapon, creating an escalating feedback loop. The more you kill, the more sinful the weapon becomes, the more devastating it grows.

### Lore Line
*"Your sins are not forgiven. They are collected."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  SIN COLLECTOR — Foundation Architecture            │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Sin Bullet projectile (5-layer rendering)  │  │
│  │  → SparkleTrailShader for bullet trail        │  │
│  │  → CrystalShimmerShader for bullet body       │  │
│  │  → Ring buffer: 24 positions                  │  │
│  │  → Palette: blood crimson primary, ember      │  │
│  │    orange secondary, judgment gold accent,     │  │
│  │    char black dark, hellfire white highlight   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (Damnation Shot)       │  │
│  │  → Full Damnation beam at 30 Sins             │  │
│  │  → InfernalBeamBodyShader with FBM noise      │  │
│  │  → noiseDistortion: 0.05 (extreme warp)       │  │
│  │  → Width: 120px (massive devastating beam)    │  │
│  │  → Origin ring: 3 spinning InfernalBeamRing   │  │
│  │    passes in blood crimson + gold              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (RadialScatter)  │  │
│  │  → Absolution Shot explosion (20-29 Sins)     │  │
│  │  → 55 sparks in radial burst                  │  │
│  │  → 6-tile blast radius                        │  │
│  │  → Center flash: SoftGlow + StarFlare + Lens  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (CrimsonVeil style)           │  │
│  │  → Smoke puffs on Absolution/Damnation impact │  │
│  │  → Blood crimson body, ember core, char edge  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZoneShader)           │  │
│  │  → Sin corruption zone at Damnation impact    │  │
│  │  → Persistent dark crimson fire zone          │  │
│  │  → noiseTex: VoronoiCell (corruption cracks)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Sin Corruption overlay on player sprite    │  │
│  │  → VoronoiEdge noise (veins spreading)        │  │
│  │  → Intensity scales with Sin count: 0→30      │  │
│  │  → At 10: subtle veins, At 20: prominent,     │  │
│  │    At 30: full corruption glow                │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Sin Counter (0-30) with visual escalation       │
│  → SinFragment homing absorb particle system       │
│  → Cardinal Sins 7-type kill tracker               │
│  → 3-tier expenditure system                       │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Sin Bullet**: Primary fire — **SparkleProjectileFoundation** renders the bullet with full 5-layer visual (SparkleTrail shader + CrystalShimmer body + bloom halo). On hit, absorbs Sin Fragment (dark crimson wisp homing from enemy to player).
- **Sin Collection**: Each fragment adds to Sin Counter (max 30). **MaskFoundation** renders corruption overlay on player — VoronoiEdge noise creating vein patterns that grow with Sin count. At 10: `intensity = 1.0` (subtle). At 20: `intensity = 2.0` + ember particle accents. At 30: `intensity = 3.0` + full blood-red glow.
- **Sin Expenditure (alt fire)**: Consumes all Sins:
  - 10-19 Sins: **Penance Shot** — enhanced **SparkleProjectileFoundation** with piercing + Wrathfire (2x trail intensity, larger bloom halo)
  - 20-29 Sins: **Absolution Shot** — **ExplosionParticlesFoundation** RadialScatter (55 sparks) + **SmokeFoundation** 30-puff ring at impact. 6-tile blast radius.
  - 30 Sins: **Damnation Shot** — **InfernalBeamFoundation** full devastating beam. 120px width, FBM noise warping, 3 spinning origin rings, massive screen shake. Everything in the beam line is annihilated.
- **Cardinal Sins**: Kill 7 different enemy types → next expenditure is one tier higher. Super Damnation: Damnation + **ImpactFoundation DamageZoneShader** persistent fire zones along beam path + 16px screen shake.

### VFX Architecture — Foundation-Based

#### Sin Bullet → SparkleProjectileFoundation
- **Layer 1 (SparkleTrail)**: 24-position ring buffer. `trailIntensity = 1.5`, `sparkleSpeed = 1.0`, `sparkleScale = 2.5`, `glitterDensity = 2.0`. Colors: blood crimson outer, ember orange core. `gradientTex`: DiesIrae LUT.
- **Layer 2 (Bloom trail)**: Photoviscerator-style velocity-stretched SoftGlow from ring buffer. 3 sub-layers: wide 1.6x (blood crimson), main (ember orange), tight 0.4x (judgment gold). `BloomTrailStretch = 3.0`.
- **Layer 3 (Bloom halo)**: SoftGlow at 3 scales + SoftRadialBloom
- **Layer 4 (Crystal body)**: BrightStarProjectile1 (additive) + CrystalShimmerShader overlay (blood crimson primary, ember orange highlight) + counter-rotated BrightStarProjectile2 + StarFlare cross gleam
- **Layer 5 (Sparkle accents)**: 4 orbiting 4PointedStarHard sparks + central twinkle
- **At high Sin count**: All layers intensify — `trailIntensity` scales to 2.5, bloom scales increase 1.3x, body pulse frequency increases

#### Damnation Beam → InfernalBeamFoundation
- `bodyTex`: SoundWaveBeam, `detailTex1`: EnergyMotion, `detailTex2`: EnergySurgeBeam
- `noiseTex`: TileableFBMNoise
- `noiseDistortion`: 0.05 (extreme warping — the beam writhes with fury)
- `bodyReps`: 1.8, `detail1Reps`: 2.5, `detail2Reps`: 1.5
- Scroll speeds: 1.0, 1.5, -0.8 (aggressive counter-scroll)
- Width: 120px (massive)
- `gradientTex`: DiesIrae LUT (blood crimson body → ember core → hellfire white center)
- Origin ring: 3 passes — main ring (0.40 scale), offset +0.4rad at 1.15x, counter-spin at 0.9x. All blood crimson + judgment gold.
- Endpoint: StarFlare (0.06 scale) + GlowOrb (0.04) + LensFlare (0.03). Ember orange.
- **Noise masking**: Layer a secondary VoronoiCell noise mask over the beam edges: `beamAlpha *= smoothstep(0.15, 0.65, voronoiSample)`. This creates the appearance of the beam being made of molten, fractured energy rather than smooth fire. Cracks of darkness tear through the beam body.

#### Sin Corruption Overlay → MaskFoundation (RadialNoiseMaskShader)
- Drawn on player sprite as a persistent overlay
- `noiseTex`: VoronoiEdge (creates spreading crack/vein patterns)
- `scrollSpeed`: 0.08 (slow creep, corruption spreading gradually)
- `rotationSpeed`: 0.05
- `circleRadius`: 0.20 at 0 Sins → 0.50 at 30 Sins (veins spread from weapon hand)
- `edgeSoftness`: 0.12 (soft, creeping boundary)
- `intensity`: `sinCount / 15.0` (0.0 at 0 → 2.0 at 30)
- `primaryColor`: blood crimson, `coreColor`: ember orange
- **Double-layer masking**: Draw the VoronoiEdge pass first, then overlay a second pass with FBMNoise at lower intensity (`0.3x`) and faster scroll (`0.15`). The FBM noise breaks up the clean Voronoi veins into organic, corrupted tendrils — like sin literally corrupting flesh.

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| SinBullet | **SparkleProjectileFoundation** | 5-layer bullet, intensity scales with Sin count |
| PenanceShot | **SparkleProjectileFoundation** (enhanced) | Pierce + Wrathfire, 2x trail, larger halo |
| AbsolutionExplosion | **ExplosionParticlesFoundation** (RadialScatter) | 55 sparks, 6-tile blast |
| DamnationBeam | **InfernalBeamFoundation** | 120px beam, FBM + Voronoi noise mask |
| SinCorruptionZone | **ImpactFoundation** (DamageZoneShader) | VoronoiCell corruption fire, Super Damnation |
| SinFragment | Custom (homing wisp) | Crimson wisp → player, simple bloom + velocity trail |
| SinCorruptionOverlay | **MaskFoundation** | Player overlay, VoronoiEdge + FBM double-layer |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| Sin Fragment Wisp | `Assets/DiesIrae/SinCollector/Pixel/` | "Small dark crimson wisp particle, ghostly tendril of red-black smoke energy, minimal detail, game particle sprite 32x32px, on solid black background --ar 1:1 --style raw" |
| Corruption Vein Texture | `Assets/DiesIrae/SinCollector/Orbs/` | "Dark spreading vein corruption texture, red-black organic veins branching outward from center, player overlay texture, circular, 128x128px, on solid black background --ar 1:1 --style raw" |

---

## 5. Damnation's Cannon (Ranged)

### Identity & Musical Soul
A cannon that fires condensed hellfire — each shot is a payload of divine punishment, lobbed in an arc like a mortar. Slow but devastating, creating massive fire zones on impact. This is the artillery piece of judgment — slow, heavy, apocalyptic.

### Lore Line
*"This is not a weapon. This is a sentence."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  DAMNATION'S CANNON — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MagicOrbFoundation (RadialNoiseMaskShader)    │  │
│  │  → Ignited Wrath Ball projectile body         │  │
│  │  → noiseTex: FBMNoise (internal fire churn)   │  │
│  │  → Heavy 3-layer bloom halo (SoftGlow at     │  │
│  │    0.25/0.15/0.08 — BIG orb)                  │  │
│  │  → circleRadius: 0.43, intensity: 2.8        │  │
│  │  → Bright ember center → crimson outer         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 2: BloomNoiseFade)     │  │
│  │  → Dense wrath ball flight trail              │  │
│  │  → CPU noise erosion creates fire wisps       │  │
│  │  → Width: 24f head → 10f tail (thick trail)   │  │
│  │  → Colors: ember → crimson → char black smoke │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (FountainCascade)│  │
│  │  → Wrath Ball impact detonation               │  │
│  │  → 55 sparks upward cone + strong gravity     │  │
│  │  → Creates cascading fire rain effect          │  │
│  │  → Center flash: SoftGlow (massive, 0.15      │  │
│  │    scale) + StarFlare + LensFlare             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (InfernalEmber style × 2)     │  │
│  │  → Double smoke ring on impact (60 puffs!)    │  │
│  │  → First ring: standard InfernalEmber         │  │
│  │  → Second ring delayed 5 frames: CrimsonVeil  │  │
│  │  → Layered smoke reads as MASSIVE explosion   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZoneShader)           │  │
│  │  → Hellfire Zone persistent ground fire       │  │
│  │  → 8-tile radius, intensifying damage         │  │
│  │  → noiseTex: FBMNoise (fast turbulent scroll) │  │
│  │  → circleRadius: 0.48 (BIG zone)             │  │
│  │  → breathe synced to damage escalation         │  │
│  │  → 300-frame (5s) persistence                 │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Impact shockwave rings                     │  │
│  │  → ringCount: 6, ringThickness: 0.08         │  │
│  │  → Massive screen shake (12px base)           │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Mortar arc trajectory rendering                 │
│  → Charge mechanic (hold for longer arc + damage)  │
│  → Damnation Barrage: 5 rapid-fire mini balls      │
│  → Scorched Earth: overlapping zones = 2x damage   │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Ignited Wrath Ball**: Primary fire — arcing lobbed shot (gravity-affected). **MagicOrbFoundation** renders the ball with RadialNoiseMaskShader (FBM internal fire). **RibbonFoundation** Mode 2 BloomNoiseFade creates dense, fire-eroded trail. On impact: 8-tile radius explosion.
- **Explosion**: Triple-foundation impact — **ExplosionParticlesFoundation** FountainCascade (55 sparks raining down) + **SmokeFoundation** double ring (60 total puffs, staggered 5 frames) + **ImpactFoundation RippleShader** (6 rings, massive). 12px screen shake.
- **Hellfire Zone**: **ImpactFoundation DamageZoneShader** at impact point. 8-tile radius, 5s persistence. Damage increases 20% per second. Standing in it grants Wrath's Blessing (+15% damage, 5s). FBM noise scrolling fast (0.4 speed) for visible fire animation. `intensity` pulse linked to damage escalation.
- **Mortar Mode**: Hold fire charges shot (max 3s). Charge visible as **MaskFoundation** glow over weapon sprite: at 1s = ember, 2s = gold, 3s = white. Charged shots arc higher, travel further, deal up to 2.5x.
- **Damnation Barrage**: After 3 direct center hits, next shot becomes 5 rapid-fire mini wrath balls (smaller MagicOrbFoundation, each 0.6x scale, faster travel). Carpet bombing.
- **Scorched Earth**: Overlapping Hellfire Zones merge — DamageZone `intensity` doubles, secondary crack texture overlay (VoronoiCell noise masked over the FBM). Enemies entering are Condemned (cannot heal, 5s).

### VFX Architecture — Foundation-Based

#### Wrath Ball Body → MagicOrbFoundation (RadialNoiseMaskShader)
- **Shader**: `scrollSpeed = 0.45` (fast churn), `rotationSpeed = 0.25`
- `circleRadius = 0.43`, `edgeSoftness = 0.07`, `intensity = 2.8`
- `noiseTex`: FBMNoise (aggressive fire look)
- `gradientTex`: DiesIrae LUT
- `primaryColor`: Wrath Red `(220,40,20)`, `coreColor`: Hellfire White `(255,240,220)`
- 3-layer bloom halo: SoftGlow at 0.25/0.15/0.08 scales (LARGE — the ball looks like a condensed sun of wrath)
- Core bloom: GlowOrb layers in judgment gold
- **Noise masking on corona**: Multiply the outer bloom layer's alpha by a sampled FBM noise texture (screen-space): `bloomAlpha *= smoothstep(0.2, 0.8, fbm(screenUV * 4 + time))`. Creates a roiling fire corona — wisps of fire breaking off the edges. The orb looks like churning contained hellfire, not a clean sphere.

#### Flight Trail → RibbonFoundation (Mode 2: BloomNoiseFade)
- 40-position ring buffer
- CPU noise erosion breaks smooth trail into fire wisps: sin-based hash sampling with threshold
- Width: 24f → 10f (thick, heavy — cannonball trail)
- Colors: Ember Orange body → Blood Crimson edge → Char Black outer fringing
- Velocity-stretched bloom sub-layers: 3 passes (wide 1.6x, main, tight 0.4x)
- Additive blending throughout

#### Impact Detonation → ExplosionParticlesFoundation (FountainCascade) + SmokeFoundation × 2
- **FountainCascade**: 55 sparks in upward cone, strong gravity (0.12-0.28). Cascading fire rain. Spark types: 40% Line, 30% Star, 30% Dot. 4-color: Ember Orange, Wrath Red, Judgment Gold, Char Black. Center flash: SoftGlow at 0.15 scale (massive) + StarFlare + LensFlare.
- **SmokeFoundation Ring 1** (frame 0): 30 puffs, InfernalEmber — Char Black body, Ember Orange core, Blood Crimson edge.
- **SmokeFoundation Ring 2** (frame 5): 30 puffs, CrimsonVeil — Blood Crimson body, Judgment Gold core, Char Black edge. 1.15x velocity. Staggered double-ring = layered mushroom-cloud effect.

#### Hellfire Zone → ImpactFoundation (DamageZoneShader)
- `scrollSpeed`: 0.40, `rotationSpeed`: 0.2
- `circleRadius`: 0.48, `edgeSoftness`: 0.08, `intensity`: starts at 2.0, increases +0.4/s
- `breathe`: synced to 20% damage increase intervals
- `noiseTex`: FBMNoise, `gradientTex`: DiesIrae LUT
- 6 orbiting edge sparkles (PointBloom, ember orange)
- **Scorched Earth variant**: Overlapping zones switch secondary noise to VoronoiCell — visible cracks in ground fire. `edgeSoftness` drops to 0.04. `intensity` doubles. Double-noise (FBM + Voronoi) = ground fracturing under hellfire.

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| IgnitedWrathBall | **MagicOrbFoundation** | FBM fire orb, 3-layer bloom, noise corona |
| WrathBallTrail | **RibbonFoundation** (Mode 2) | Noise-eroded fire trail |
| ImpactDetonation | **ExplosionParticlesFoundation** (FountainCascade) | 55 sparks, fire rain |
| ImpactSmokeDouble | **SmokeFoundation** × 2 (staggered) | 60 puffs total, mushroom cloud |
| ImpactShockwave | **ImpactFoundation** (RippleShader) | 6 rings, 12px screen shake |
| HellfireZone | **ImpactFoundation** (DamageZoneShader) | 5s persistence, escalating damage |
| MiniWrathBall | **MagicOrbFoundation** (0.6x scale) | Damnation Barrage rapid fire |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| Wrath Ball Corona | `Assets/DiesIrae/DamnationsCannon/Orbs/` | "Roiling sphere of condensed hellfire, bright ember orange center with dark crimson fire tendrils extending outward, churning internal energy, game projectile sprite 96x96px, on solid black background --ar 1:1 --style raw" |
| Hellfire Ground Texture | `Assets/DiesIrae/DamnationsCannon/Trails/` | "Ground fire texture, top-down view of spreading flames, dark crimson base with ember orange hot spots and black char cracks, circular, 128x128px, on solid black background --ar 1:1 --style raw" |

---

## 6. Arbiter's Sentence (Ranged)

### Identity & Musical Soul
The arbiter delivers sentences with cold, burning precision. Each shot is a focused flame of judgment — not wildfire, but directed punishment. This is the precision counterpart to Damnation's Cannon's area devastation.

### Lore Line
*"The arbiter does not miss. The arbiter does not forgive."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  ARBITER'S SENTENCE — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Judgment Flame bullet (5-layer rendering)  │  │
│  │  → SparkleTrailShader + CrystalShimmerShader  │  │
│  │  → Precision aesthetic: tight trail, clean     │  │
│  │  → trailIntensity: 1.2 (controlled, not wild) │  │
│  │  → sparkleScale: 2.0, glitterDensity: 1.5    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Homing beam at Arbiter's Focus             │  │
│  │  → 3 precision shots with slight homing       │  │
│  │  → Width: 4px (razor focused)                 │  │
│  │  → No ricochet (direct, punishing)            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (SlashMarkShader)            │  │
│  │  → Flame scar marks per hit stack             │  │
│  │  → 1-5 visible flame scars on enemy           │  │
│  │  → slashAngle: randomized per stack           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Sentence Cage visual at 5 stacks          │  │
│  │  → noiseTex: VoronoiCell (cage bars)          │  │
│  │  → Tight circle wrapping enemy (cage)         │  │
│  │  → circleRadius: 0.30 (tight imprisonment)   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Flame transfer ripple when sentence spreads│  │
│  │  → ringCount: 2 (subtle, precise)             │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Judgment Flame debuff (1-5 stacks visual)       │
│  → Arbiter's Focus crosshair glyph                 │
│  → Final Judgment flame transfer on kill            │
│  → Sentence Cage imprisonment effect               │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Judgment Flame Shot**: **SparkleProjectileFoundation** fast precision bullet. `trailIntensity = 1.2`, `sparkleSpeed = 0.8`. Clean, focused aesthetic — not messy fire but directed judgment. On hit: Judgment Flame debuff (15 damage/s, 3s). Stacks to 5 (75 damage/s).
- **Sentencing**: At 5 stacks: **MaskFoundation** renders Sentence Cage over enemy — VoronoiCell noise creates cage-bar patterns. `circleRadius = 0.30`, `edgeSoftness = 0.03`, `intensity = 2.5`. Enemy rooted 1s, next hit deals 2x.
- **Arbiter's Focus**: 5 consecutive hits on same target: crosshair glyph via **MaskFoundation** (StarField noise, judgment gold, low intensity). Next 3 shots home using **ThinLaserFoundation** beams (4px, no ricochet) + 40% bonus.
- **Final Judgment**: Kill a Sentenced enemy → flame transfers to nearest enemy. Transfer visual: **ImpactFoundation RippleShader** burst at death + homing ember wisp (SparkleProjectile scaled to 0.3x, rapid travel to new target).

### VFX Architecture — Foundation-Based

#### Judgment Flame Bullet → SparkleProjectileFoundation
- 5-layer rendering at precision settings:
  - Layer 1 (SparkleTrail): 24-pos ring buffer, `trailIntensity = 1.2`, `glitterDensity = 1.5`, tight taper (10px → 1px)
  - Layer 2 (Bloom trail): Compact — `BloomTrailStretch = 2.0`
  - Layer 3 (Bloom halo): SoftGlow at smaller scales (0.08/0.05/0.03) — tight, focused
  - Layer 4 (Crystal body): CrystalShimmerShader crimson/gold
  - Layer 5 (Sparkle accents): 2 orbiting sparks (cleaner, more precise than default 4)

#### Sentence Cage → MaskFoundation (RadialNoiseMaskShader)
- `noiseTex`: VoronoiCell — cage bar patterns at tight radius + high contrast
- `circleRadius`: 0.30, `edgeSoftness`: 0.03
- `scrollSpeed`: 0.05 (barely moving — cage is static, imprisoning)
- `intensity`: 2.5, `primaryColor`: Blood Crimson, `coreColor`: Ember Orange
- **Noise masking technique**: Apply secondary radial `smoothstep` that only renders Voronoi between radii 0.25-0.35 (ring shape). Creates cage bars in a circle around enemy — flame cage, not filled orb.

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| JudgmentFlameBullet | **SparkleProjectileFoundation** | Precision 5-layer, tight trail |
| FocusBeam | **ThinLaserFoundation** | 4px homing beam, 3 shots |
| FlameScar | **ImpactFoundation** (SlashMarkShader) | Randomized angle per stack |
| SentenceCage | **MaskFoundation** (VoronoiCell) | Tight ring cage on enemy |
| FlameTransferWisp | **SparkleProjectileFoundation** (0.3x) | Homing wisp to nearby target |
| TransferRipple | **ImpactFoundation** (RippleShader) | 2-ring death burst |

---

## 7. Staff of Final Judgement (Magic)

### Identity & Musical Soul
The staff delivers the final word — floating ignition mines that hover in wait. Judgment lies in wait. The staff transforms the battlefield into a minefield of divine fire, punishing those who dare approach.

### Lore Line
*"Judgment does not chase. Judgment waits."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  STAFF OF FINAL JUDGEMENT — Foundation Architecture │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MagicOrbFoundation (RadialNoiseMaskShader)    │  │
│  │  → Floating Ignition mine body                │  │
│  │  → noiseTex: CosmicVortex → FBMNoise on arm  │  │
│  │  → Unarmed: dim glow (intensity 1.0)         │  │
│  │  → Armed: brighter (intensity 1.8)            │  │
│  │  → Near-trigger: pulsing (intensity 2.5-3.0)  │  │
│  │  → 3-layer bloom halo scaling with state       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Purgatory Field connecting lines           │  │
│  │  → Fire connection between 4+ mines           │  │
│  │  → Width: 8px, persistent while connected     │  │
│  │  → Ember → crimson color, internal scroll     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (RadialScatter)  │  │
│  │  → Mine detonation spark burst                │  │
│  │  → 40 sparks per mine                         │  │
│  │  → Chain reaction bonus: +30% if adjacent     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (InfernalEmber style)         │  │
│  │  → 20-puff smoke ring per detonation          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Detonation shockwave per mine              │  │
│  │  → ringCount: 3, fast expansion               │  │
│  │  → Judgment Storm: 7-ring screen-wide ripple  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Purgatory Field zone rendering             │  │
│  │  → Semi-transparent fire field between mines  │  │
│  │  → noiseTex: FBMNoise, low intensity          │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Mine placement system (up to 8)                 │
│  → Mine state machine (unarmed→armed→triggered)    │
│  → Chain reaction damage bonus                     │
│  → Judgment Storm: 3+ simultaneous = fire rain     │
│  → Final Judgement: lethal save mechanic            │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Floating Ignition**: Primary fire — places mine at cursor. **MagicOrbFoundation** renders mine body with state-dependent intensity. Unarmed (1s arm delay): CosmicVortex noise, `intensity = 1.0`. Armed: FBMNoise, `intensity = 1.8`. Near-trigger (enemy within 3 tiles): pulsing `intensity = 2.5-3.0`, ember → gold shift.
- **Detonation**: **ExplosionParticlesFoundation** RadialScatter (40 sparks) + **SmokeFoundation** 20-puff ring + **ImpactFoundation RippleShader** (3 rings). Adjacent mines within 5 tiles chain-detonate with +30% bonus.
- **Judgment Storm**: 3+ mines detonating within 1s → screen-wide **ImpactFoundation RippleShader** (7 rings). Fire rain: **ExplosionParticlesFoundation** FountainCascade from top of screen (2s duration).
- **Purgatory Field**: 4+ mines in a line → **ThinLaserFoundation** renders beams between them. **MaskFoundation** renders semi-transparent fire field. Enemies crossing take continuous damage.
- **Final Judgement**: Lethal damage while equipped → ALL mines detonate at 3x power (55 sparks each) + screen-wide 7-ring ripple. Player survives at 1 HP.

### VFX Architecture — Foundation-Based

#### Ignition Mine Body → MagicOrbFoundation (RadialNoiseMaskShader)
- **Unarmed**: `noiseTex`: CosmicVortex, `scrollSpeed`: 0.15, `intensity`: 1.0, dim ember glow
- **Armed**: `noiseTex` transitions to FBMNoise, `scrollSpeed`: 0.25, `intensity`: 1.8
- **Near-trigger**: `intensity` oscillates 2.5-3.0 via `sin(time * 8)` rapid pulse, ember → gold color shift
- `circleRadius`: 0.38, `edgeSoftness`: 0.05
- 3-layer bloom: scales with state — `0.10/0.06/0.03` (unarmed) → `0.18/0.12/0.06` (armed) → pulsing scales (trigger)
- **Noise masking on state transition**: During the unarmed→armed transition, both noise textures blend by lerping between CosmicVortex and FBMNoise samples over 30 frames. The visual shift reads as the mine's internal fire intensifying from a calm swirl to aggressive churning.

#### Purgatory Field → ThinLaserFoundation + MaskFoundation
- **ThinLaserFoundation** beams (8px) connecting mine pairs. Ember orange body with internal scroll.
- **MaskFoundation** fills the field between beam lines: FBMNoise at `intensity = 0.8` (semi-transparent fire field). `scrollSpeed = 0.3`. `edgeSoftness = 0.15` (very soft edges — the field bleeds into surrounding air).
- Enemies inside: continuous 10 damage/s + Wrathfire Tier 1 application.

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| FloatingIgnition | **MagicOrbFoundation** | State-driven orb (3 states) |
| PurgatoryBeam | **ThinLaserFoundation** | 8px connecting beam |
| PurgatoryField | **MaskFoundation** | Semi-transparent fire area |
| MineDetonation | **ExplosionParticlesFoundation** (RadialScatter) | 40 sparks per mine |
| MineRipple | **ImpactFoundation** (RippleShader) | 3-ring per mine, 7-ring storm |
| JudgmentFireRain | **ExplosionParticlesFoundation** (FountainCascade) | Screen-wide fire rain |

---

## 8. Grimoire of Condemnation (Magic)

### Identity & Musical Soul
A dark grimoire containing the words of every condemnation. Opening it unleashes waves of dark judgment energy — raw, channeled, continuous condemnation. This is sustained channeled destruction.

### Lore Line
*"Every name written in this book burns twice — once on the page, once in flesh."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  GRIMOIRE OF CONDEMNATION — Foundation Architecture │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ LaserFoundation (ConvergenceBeamShader)       │  │
│  │  → Condemnation Wave channeled beam           │  │
│  │  → 4 detail textures: ThinLinearGlow,         │  │
│  │    LightningSurge, EnergyMotion, EnergySurge  │  │
│  │  → gradientTex: DiesIrae LUT                  │  │
│  │  → Width: 80px → 120px (widens over channel)  │  │
│  │  → MaxLength: 600px (10 tile cone range)      │  │
│  │  → AimSpeed: 0.12 rad/frame (sweepable)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ InfernalBeamFoundation (Dark Sermon circle)   │  │
│  │  → Channeled sermon ritual ring attack        │  │
│  │  → InfernalBeamBodyShader: FBM noise warped   │  │
│  │  → Rendered as circular ring (not straight)   │  │
│  │  → Width: 60px ring thickness                 │  │
│  │  → Origin ring: 3 spinning passes             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Sermon sigil circle rendering              │  │
│  │  → noiseTex: VoronoiCell (runic patterns)     │  │
│  │  → circleRadius: 0.45, builds over 3s channel │  │
│  │  → Sigil rotation during channel              │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Sermon circle detonation rings             │  │
│  │  → ringCount: 8, massive expansion            │  │
│  │  → Blood crimson → gold → white flash         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (CrimsonVeil style)           │  │
│  │  → Dark smoke during channeling               │  │
│  │  → Slow continuous puffs (not bursts)         │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Written in Flame: "CONDEMNED" text apparitions   │
│  → Page Turn enhanced cast every 7th use           │
│  → Dark Sermon: 3s channel ritual circle           │
│  → Name counter powering damage (+5% per name)     │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Condemnation Wave**: **LaserFoundation** channeled beam — sweepable cone. 4 detail textures scroll with DiesIrae palette. Continuous damage + gradual slow. Width widens over channel (80px → 120px at 3s). Every 7th cast: Page Turn = +30% damage, wider cone.
- **Dark Sermon**: Alt fire (3s channel) — **MaskFoundation** renders ritual circle (10-tile radius, VoronoiCell sigils). Circle intensifies over 3s. At completion: **ImpactFoundation RippleShader** 8-ring detonation + **InfernalBeamFoundation** circular beam ring at boundary + massive condemnation damage inside.
- **Written in Flame**: Kills leave floating "CONDEMNED" text apparitions — custom UI rendering (blood crimson glow text, slow upward drift, 3s). Each name powers next cast (+5%, max 10 names = +50%).

### VFX Architecture — Foundation-Based

#### Condemnation Wave → LaserFoundation (ConvergenceBeamShader)
- 4 detail textures scrolling: ThinLinearGlow (+0.8/s), LightningSurge (-1.2/s counter-scroll), EnergyMotion (+0.6/s), EnergySurgeBeam (+1.0/s)
- `gradientTex`: DiesIrae LUT
- `beamWidth`: 80px base, `widthGrowth`: +13.3px/s over 3s channel → 120px max
- AimSpeed: 0.12 rad/frame
- **Noise masking**: Apply FBM noise to the beam edge: `edgeAlpha *= smoothstep(0.1, 0.5, fbm(beamEdge + time * 0.8))`. The condemnation wave looks like dark fire eating along its edges — the beam boundary is living, ragged, consuming.

#### Dark Sermon Sigil → MaskFoundation + InfernalBeamFoundation
- **MaskFoundation** sigil circle: `VoronoiCell` noise, `circleRadius` grows from 0.20 → 0.45 over 3s channel. `rotationSpeed = 0.20`. `intensity` ramps 1.0 → 3.0. At completion: flash to white.
- **InfernalBeamFoundation** ring: rendered as a circular beam (vertex strip bent into circle at the sigil radius). `noiseDistortion = 0.04`, FBM noise warping the ring body. Width 60px. 3 spinning origin ring passes.
- **Combined**: The MaskFoundation sigil is the "floor" of the ritual. The InfernalBeam ring is the "wall" of fire at the boundary. Together they create a 3D-looking ritual circle.

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| CondemnationWave | **LaserFoundation** (ConvergenceBeamShader) | 80-120px sweepable beam |
| DarkSermonSigil | **MaskFoundation** (VoronoiCell) | Growing ritual circle floor |
| DarkSermonRing | **InfernalBeamFoundation** | Circular fire wall at boundary |
| SermonDetonation | **ImpactFoundation** (RippleShader) | 8-ring massive burst |
| CondemnedText | Custom UI rendering | Floating blood-crimson text |

---

## 9. Eclipse of Wrath (Magic)

### Identity & Musical Soul
An eclipse — when darkness consumes light, and wrath consumes mercy. Eclipse orbs are spheres of absolute darkness edged with wrathful fire. When they split, they scatter destruction like a dying star's final moments. The most visually dramatic magic weapon in Dies Irae.

### Lore Line
*"The sun that rises for judgment is not the sun that brings dawn."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  ECLIPSE OF WRATH — Foundation Architecture         │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Eclipse Orb dark body rendering            │  │
│  │  → INVERTED RENDERING: dark center, bright    │  │
│  │    corona ring (additive bloom at edge only)   │  │
│  │  → noiseTex: VoronoiCell + FBMNoise layered   │  │
│  │  → circleRadius: 0.35 (dark core area)        │  │
│  │  → edgeSoftness: 0.02 (sharp eclipse edge)    │  │
│  │  → The corona is a SEPARATE additive bloom     │  │
│  │    ring drawn around the masked dark disc      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 5: Cosmic Nebula)      │  │
│  │  → Eclipse Orb corona trail                   │  │
│  │  → CosmicNebulaClouds texture for fire wisp   │  │
│  │  → Width: 20f → 6f, ember orange colors       │  │
│  │  → The trail is the corona fire bleeding off  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation                   │  │
│  │  → Wrath Shard projectiles (post-split)       │  │
│  │  → 6 shards per orb (12 on Corona Flare)     │  │
│  │  → Fast, piercing, ember orange + fire trail  │  │
│  │  → Scaled to 0.5x (small, rapid shards)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (XSlashShader)               │  │
│  │  → Corona Flare crit explosion                │  │
│  │  → 12 shards + central fire nova              │  │
│  │  → fireIntensity: 0.08 (aggressive burst)     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (DamageZoneShader)           │  │
│  │  → Eclipse Field darkness zone (2s)           │  │
│  │  → Special: DARK overlay (dims brightness)    │  │
│  │  → noiseTex: CosmicNebula (dark cloud scroll) │  │
│  │  → Ember fire rim at edges                    │  │
│  │  → Total Eclipse: 3x duration, 2x radius      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (SpiralShrapnel) │  │
│  │  → Eclipse orb split burst                    │  │
│  │  → 30 sparks in spiral (debris)               │  │
│  │  → Dark-themed: char black + ember accents    │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Dual-render eclipse technique (dark disc +       │
│    additive corona — two draw passes)               │
│  → Eclipse Field screen darkening overlay           │
│  → Total Eclipse: 3+ overlapping fields merge       │
│  → Corona Flare crit mechanic                      │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Eclipse Orb**: Primary fire — slow-moving dark sphere with fire corona. Dual-render: **MaskFoundation** dark disc (AlphaBlend, dark center) + additive bloom corona ring drawn separately. **RibbonFoundation** Mode 5 CosmicNebula for trailing fire wisps.
- **Split**: On impact or max range, orb splits into 6 **SparkleProjectileFoundation** Wrath Shards. Fast, piercing, 0.5x scale. Ricochet off surfaces once. Brief fire trail.
- **Eclipse Field**: Destroyed orbs create **ImpactFoundation DamageZoneShader** zones with inverted purpose — darken, not burn. CosmicNebula noise scroll = dark cloud. Fire rim at edges (additive pass). Enemies inside: -vision, +15% damage from all sources. 2s duration.
- **Total Eclipse**: 3+ overlapping fields merge. 3x duration, +30% damage mod, 30% enemy slow. Doubled noise layers.
- **Corona Flare**: Crit hits → **XSlashFoundation** fire nova + 12 shards + **ExplosionParticlesFoundation** SpiralShrapnel 30 sparks.

### VFX Architecture — Foundation-Based

#### Eclipse Orb → MaskFoundation (INVERTED) + Additive Corona
- **Pass 1 (Dark Disc)**: MaskFoundation with **AlphaBlend** (not Additive). `noiseTex`: VoronoiCell. `circleRadius`: 0.35, `edgeSoftness`: 0.02 (sharp). Near-black disc with internal void texture.
- **Pass 2 (Fire Corona)**: Separate additive pass — SoftGlow bloom ring at disc's edge radius. Ember orange → Judgment Gold. FBM noise mask on corona: `coronaAlpha *= smoothstep(0.2, 0.9, fbm(polarUV + time * 0.5))`. Fire wisps break off the corona — alive, flaring and dying.
- **Combined**: True eclipse — impenetrable dark center, ragged living fire at the boundary. Two render passes are essential.

#### Eclipse Field → ImpactFoundation (DamageZoneShader, INVERTED)
- `noiseTex`: CosmicNebula (dark cloud scroll)
- `scrollSpeed`: 0.20, `rotationSpeed`: 0.10
- `circleRadius`: 0.42, `edgeSoftness`: 0.10
- `intensity`: 1.5 (lower than combat zones — this darkens, not burns)
- Rendered with custom blend state: dims background rather than adding light
- Ember orange fire rim: separate additive ring drawn at `circleRadius` edge with FBM noise masking
- **Total Eclipse**: `noiseTex` doubles (CosmicNebula + VoronoiCell overlay). `circleRadius` × 2. Duration × 3. Extra SmokeFoundation 15-puff ring in char black.

#### Wrath Shards → SparkleProjectileFoundation (0.5x scale)
- All 5 layers scaled to 0.5x: shorter trails, tighter bloom, smaller body
- `trailIntensity = 2.0` (bright despite small size — they streak like burning fragments)
- `sparkleSpeed = 1.5` (fast trailing sparks)
- 6 per split (12 on Corona Flare crit)
- Each shard: radial direction from orb center + slight random spread

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| EclipseOrb | **MaskFoundation** (inverted) + Additive corona | Dual-render dark disc + fire corona |
| EclipseTrail | **RibbonFoundation** (Mode 5) | Cosmic Nebula fire wisps |
| WrathShard | **SparkleProjectileFoundation** (0.5x) | 6-12 fast piercing shards |
| CoronaFlare | **XSlashFoundation** | Fire nova on crit |
| EclipseField | **ImpactFoundation** (DamageZone, inverted) | 2s darkening zone |
| TotalEclipse | **ImpactFoundation** (enhanced DamageZone) | Merged 3+ fields, 6s |
| EclipseShrapnel | **ExplosionParticlesFoundation** (SpiralShrapnel) | 30 dark + ember sparks |

### Asset Requirements

| Asset | Location | Midjourney Prompt |
|-------|----------|------------------|
| Eclipse Corona Ring | `Assets/DiesIrae/EclipseOfWrath/Orbs/` | "Eclipse corona ring texture, bright ember orange and gold fire ring with ragged wisps extending outward, dark void center cutout, game VFX overlay, 128x128px, on solid black background --ar 1:1 --style raw" |
| Dark Nebula Cloud | `Assets/DiesIrae/EclipseOfWrath/Trails/` | "Dark cosmic nebula cloud texture, very dark purple-black swirling clouds with faint ember orange edges, subtle internal structure, game VFX noise texture, 256x256px, seamless tile, on solid black background --ar 1:1 --style raw" |

---

## 10. Death Tolling Bell (Summon)

### Identity & Musical Soul
The death bell tolls and each toll marks another soul claimed. A spectral bell that attacks with concentric sound rings. Each toll is inevitable, rhythmic, building — just like the Dies Irae percussion building to fortissimo.

### Lore Line
*"Ask not for whom the bell tolls. It tolls for all."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  DEATH TOLLING BELL — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Toll Wave expanding ring attack            │  │
│  │  → ringCount: 3 per toll, ringThickness: 0.06 │  │
│  │  → crimson → gold gradient                    │  │
│  │  → Funeral March: ringCount: 6, 2x thickness  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Bell entity body rendering                 │  │
│  │  → noiseTex: CosmicVortex (internal fire)     │  │
│  │  → State cycling: rest→charge→toll→flash      │  │
│  │  → intensity: 1.0 rest → 2.5 toll moment      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (CrimsonVeil style)           │  │
│  │  → Funeral March smoke burst                  │  │
│  │  → 15 puffs, blood crimson + ember            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 4: Harmonic Wave)      │  │
│  │  → Tether between bell and command position   │  │
│  │  → Standing wave = bell resonance visual      │  │
│  │  → Colors: crimson → gold oscillating         │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Toll Wave attack cycle (2s interval)            │
│  → Tolled stacks (1-5) on enemies                  │
│  → Death-Mark at 5 Tolled (2x damage)             │
│  → Funeral March (every 10th toll, enhanced)       │
│  → Bell positioning command system                 │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Toll Wave**: Every 2s, bell tolls — **ImpactFoundation RippleShader** (3 rings, crimson-gold). Each hit: Tolled +1 stack. At 5: Death-Mark (2x damage from next wave).
- **Funeral March**: Every 10th toll — 6-ring doubled-thickness RippleShader. Wrathfire on all hit. **SmokeFoundation** 15-puff burst. Bell **MaskFoundation** flashes white (`intensity = 3.0`).
- **Bell Body**: **MaskFoundation** CosmicVortex noise. State-driven: idle `1.0` → charging `1.5` → toll `2.5` → flash. Color cycles: crimson → ember → gold → white flash.
- **Positioning**: Right-click commands bell location. **RibbonFoundation** Mode 4 tether renders standing wave between player and bell — crimson-gold oscillation.

### VFX Architecture — Foundation-Based

#### Toll Wave → ImpactFoundation (RippleShader)
- Standard toll: `ringCount = 3`, `ringThickness = 0.06`. Colors: Ember Orange outer → Blood Crimson inner → Judgment Gold core ring.
- Expansion: 0.05 → 0.80 over 90 frames (EaseOutCubic — fast initial bloom, slow fade)
- **Each ring gets noise-masked edges**: `ringAlpha *= smoothstep(0.2, 0.8, perlin(circumferenceUV + time * 2))`. The rings look like sound waves made of fire — not smooth circles but ragged, vibrating rings of force.
- Funeral March toll: `ringCount = 6`, `ringThickness = 0.12`. Same noise masking but heavier. 6px screen shake.

#### Bell Body → MaskFoundation (RadialNoiseMaskShader)
- `noiseTex`: CosmicVortex (swirling internal fire aesthetic)
- `circleRadius`: 0.36, `edgeSoftness`: 0.05
- Idle: `scrollSpeed = 0.10`, `intensity = 1.0`, crimson palette
- Charging (0.5s before toll): `scrollSpeed = 0.25`, `intensity = 1.5`, color shifts to ember
- Toll moment (flash frame): `intensity = 2.5`, color = gold, scale pulse 1.0 → 1.15 → 1.0
- **Double-noise spectral layer**: Overlay a secondary StarField noise at low `intensity` (0.3) — creates sparkling fire motes inside the swirling bell body, adding depth.

#### Bell Tether → RibbonFoundation (Mode 4: Harmonic Wave)
- 40-position ring buffer, but with sinusoidal perpendicular offsets — standing wave visualization
- HarmonicWaveBeam texture UV-scrolled at 1.0x speed
- Width: 8f → 4f (visible but not dominant)
- Color oscillation: crimson → gold → crimson cycling at 2Hz
- PureBloom sub-layer (Mode 1) underneath at 0.3x opacity for soft glow

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| TollWave | **ImpactFoundation** (RippleShader) | 3-ring standard, 6-ring Funeral March |
| BellEntity | **MaskFoundation** (CosmicVortex) | State-driven body rendering |
| BellTether | **RibbonFoundation** (Mode 4) | Harmonic standing wave |
| FuneralSmoke | **SmokeFoundation** (CrimsonVeil) | 15-puff enhanced toll burst |

---

## 11. Harmony of Judgement (Summon)

### Identity & Musical Soul
Judgment harmonized — a chorus of condemnation. The sigil minion represents the collective will of all judges, cycling through an automated Scan→Judge→Execute process on each target.

### Lore Line
*"When many voices speak as one, there is no appeal."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  HARMONY OF JUDGEMENT — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Sigil entity body (rotating glyph)         │  │
│  │  → noiseTex: VoronoiCell (fractured sigil)    │  │
│  │  → rotationSpeed: 0.20 (visible rotation)     │  │
│  │  → Processing: 0.40 (faster spin)             │  │
│  │  → crimson-gold palette                       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Scan beam: sigil → target                  │  │
│  │  → Width: 4px scan → 6px judge → 10px execute │  │
│  │  → Color intensifies through cycle            │  │
│  │  → Crimson → gold → white flash               │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Execute burst at target                    │  │
│  │  → ringCount: 4, crimson-gold                 │  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (XSlashShader)               │  │
│  │  → Harmonized Verdict: enhanced execution     │  │
│  │  → After 5 rapid processings                  │  │
│  │  → fireIntensity: 0.06, judgment gold         │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → Judgment Process cycle (Scan→Judge→Execute)     │
│  → Collective Judgment (multi-sigil syncing)       │
│  → Harmonized Verdict (2x speed, +50% dmg)        │
│  → Judgment Aura passive (+5% damage allies)       │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Judgment Process**: Autonomous cycle per target — **Scan** (1s): **ThinLaserFoundation** 4px crimson beam + **MaskFoundation** `rotationSpeed = 0.40`. → **Judge** (0.5s): beam widens to 6px, color shifts gold, sigil `intensity` ramps. → **Execute** (instant): beam flashes to 10px white, **ImpactFoundation RippleShader** 4-ring burst at target, damage applied.
- **Collective Judgment**: Multiple sigils scanning same target → synced execution = 2x damage each.
- **Harmonized Verdict**: After 5 rapid executions within 10s → sigil enters enhanced state (2x process speed, +50% damage). **XSlashFoundation** judgment X on every Execute. `MaskFoundation` `intensity += 1.0`, brighter gold.
- **Judgment Aura**: Passive 5-tile radius — allies deal +5% damage. Visual: very faint **MaskFoundation** ring (StarField noise, `intensity = 0.3`, judgment gold, barely visible).

### VFX Architecture — Foundation-Based

#### Sigil Body → MaskFoundation (RadialNoiseMaskShader)
- `noiseTex`: VoronoiCell (fractured sigil aesthetic)
- `circleRadius`: 0.32, `edgeSoftness`: 0.04 (sharp sigil edges)
- Normal: `scrollSpeed = 0.10`, `rotationSpeed = 0.20`, `intensity = 1.8`. Crimson body, gold highlights.
- Processing: `rotationSpeed = 0.40` (faster spin indicates active judgment). `intensity = 2.2`.
- Harmonized: `rotationSpeed = 0.50`, `intensity = 2.8`, gold dominant.
- **Noise-on-noise layering**: VoronoiCell primary + StarField secondary at `0.4x intensity`. The StarField adds glittering inner detail that makes the sigil look like it contains contained fire-light behind fractured glass.

#### Judgment Beam → ThinLaserFoundation (ThinBeamShader)
- Phase-driven width: Scan 4px → Judge 6px → Execute 10px
- `gradientTex`: DiesIrae LUT
- Scan: `alpha = 0.6`, crimson body
- Judge: `alpha = 0.8`, gold body, `scrollSpeed = 0.3`
- Execute: `alpha = 1.0`, white flash (1 frame) → fade over 8 frames
- No ricochet — direct judgment, single target per beam

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| SigilEntity | **MaskFoundation** (VoronoiCell) | State-driven rotating sigil body |
| JudgmentBeam | **ThinLaserFoundation** | Phase-driven width/color beam |
| ExecuteBurst | **ImpactFoundation** (RippleShader) | 4-ring crimson-gold burst |
| HarmonizedX | **XSlashFoundation** | Judgment gold X at enhanced Execute |
| JudgmentAura | **MaskFoundation** (StarField) | Faint gold passive aura ring |

---

## 12. Wrathful Contract (Summon)

### Identity & Musical Soul
A contract signed in blood — a wrath demon bound to serve. Immensely powerful but constantly draining player HP. Risk and reward incarnate. The demon IS wrath given form.

### Lore Line
*"The contract demands payment in blood. Yours or theirs — it cares not which."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  WRATHFUL CONTRACT — Foundation Architecture        │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → Demon body rendering                       │  │
│  │  → noiseTex: FBMNoise (internal fire body)    │  │
│  │  → circleRadius: 0.40 → 0.48 in Frenzy       │  │
│  │  → Normal: ember cracks on dark silhouette    │  │
│  │  → Frenzy: widened, brighter, faster scroll   │  │
│  │  → Breach: blood-crimson shift, hostile glow  │  │
│  │  → Demon eyes: 2x StarFlare (ember-gold)      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (ThinBeamShader)          │  │
│  │  → Blood Contract tether (player → demon)    │  │
│  │  → Width: 3px (thin, thread-like)             │  │
│  │  → Crimson body, life drain particles flowing │  │
│  │  → Blood Sacrifice: briefly reverses to gold  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (RippleShader)               │  │
│  │  → Frenzy activation burst                    │  │
│  │  → Breach warning pulse                       │  │
│  │  → Blood Sacrifice flash ring                 │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (RadialScatter)  │  │
│  │  → Demon Frenzy kill effects                  │  │
│  │  → 30 sparks per kill (reward visual)         │  │
│  │  → Ember + crimson + gold palette             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (InfernalEmber style)         │  │
│  │  → Demon ambient smoke (continuous)           │  │
│  │  → 5 puffs/s (normal) → 15 puffs/s (Frenzy)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Blood Sacrifice)   │  │
│  │  → Cinematic sacrifice sequence               │  │
│  │  → Camera zoom to demon + crimson vignette    │  │
│  │  → 3x empowerment flash                      │  │
│  └───────────────────────────────────────────────┘  │
│  UNIQUE ADDITIONS:                                  │
│  → HP drain mechanic (1 HP/s → 3 HP/s Frenzy)    │
│  → Frenzy state (3 kills → 2x speed, +30% dmg)   │
│  → Breach of Contract (<10% HP → hostile)          │
│  → Blood Sacrifice (20% HP for 3x demon damage)   │
│  → Contract Clause healing (5% enemy max HP)       │
└─────────────────────────────────────────────────────┘
```

### Combat Mechanics
- **Demon Body**: **MaskFoundation** FBMNoise = molten cracks across dark silhouette. `primaryColor`: Char Black, `coreColor`: Ember Orange. 2x StarFlare eyes (ember-gold, 0.02 scale). Frenzy: `circleRadius` 0.40 → 0.48, `intensity` +1.0. Breach: color shift to blood crimson, red eyes.
- **Blood Contract Tether**: **ThinLaserFoundation** 3px beam. Crimson. Life drain visualized as particles flowing along beam (player → demon). Blood Sacrifice: brief gold reversal surge.
- **Demon Attacks**: Melee charges. Frenzy kills: **ExplosionParticlesFoundation** 30-spark RadialScatter + **SmokeFoundation** 10-puff InfernalEmber burst. Ambient: 5 puffs/s → 15 puffs/s Frenzy.
- **Blood Sacrifice**: **AttackAnimationFoundation** mini-sequence — camera focus demon, crimson vignette, gold flash. 3x damage 5s.
- **Breach of Contract**: Below 10% HP: **ImpactFoundation RippleShader** warning pulse (fast crimson 2-ring). Demon hostile 5s. Re-bind ripple after.

### VFX Architecture — Foundation-Based

#### Demon Body → MaskFoundation (RadialNoiseMaskShader)
- `noiseTex`: FBMNoise (molten cracks across dark form)
- `circleRadius`: 0.40 normal, 0.48 Frenzy
- `edgeSoftness`: 0.06 (semi-soft — the demon's form bleeds smoke at edges)
- `scrollSpeed`: 0.20 normal, 0.35 Frenzy (cracks shift faster)
- `intensity`: 1.5 normal, 2.5 Frenzy, 3.0 Breach
- `primaryColor`: Char Black `(20,5,5)`, `coreColor`: Ember Orange `(255,120,30)`
- **Double-noise for depth**: FBMNoise primary (large cracks) + VoronoiCell secondary at `0.3x intensity` (fine surface fractures). Combined = a demon that looks like it's barely holding together, molten inside with cracked obsidian skin.
- 2x StarFlare eyes: position offset from center by `(±0.08, -0.05)`. Scale 0.02. Ember-gold normal, blood-red in Breach.
- Frenzy aura: additional SoftGlow at 1.3x `circleRadius`, ember orange at 15% opacity, pulsing.

#### Blood Tether → ThinLaserFoundation (ThinBeamShader)
- Width: 3px (thin, intimate, thread-like — a leash)
- `gradientTex`: DiesIrae LUT
- Base color: Blood Crimson
- Life drain particles: sample beam at 6 evenly-spaced points, draw 6 tiny SoftGlow orbs (2px, crimson) flowing from player toward demon at 4px/frame. Gives the impression of life being drawn along the thread.
- Blood Sacrifice: beam color flash to gold for 30 frames, particles reverse direction for 10 frames, then surge all toward demon.
- Breach: beam color shifts to angry red, pulsing width (3px → 5px → 3px at 4Hz) — the contract is straining.

#### Blood Sacrifice → AttackAnimationFoundation
- Mini-sequence (60 frames total, shortened from standard 110):
  - Camera shifts 20% toward demon (10 frames)
  - Screen crimson vignette ramps from 0 → 40% coverage (20 frames)
  - Gold flash at demon position: SoftGlow 0.12 scale in judgment gold (1 frame peak)
  - **MaskFoundation** demon body `intensity` temporarily spikes to 3.5 for 15 frames
  - Camera returns (15 frames), vignette fades
- Post-sacrifice: demon leaves ember trail sparks (SoftGlow 0.01 scale, ember orange) at feet for 5s duration

### Sub-Projectiles (Foundation-Based)

| Projectile | Foundation | Configuration |
|-----------|-----------|---------------|
| DemonEntity | **MaskFoundation** (FBMNoise + VoronoiCell) | State-driven demon body (Normal/Frenzy/Breach) |
| BloodTether | **ThinLaserFoundation** | 3px crimson tether with drain particles |
| FrenzyKillBurst | **ExplosionParticlesFoundation** (RadialScatter) | 30 ember sparks per kill |
| FrenzyActivation | **ImpactFoundation** (RippleShader) | Activation ring burst |
| BreachWarning | **ImpactFoundation** (RippleShader) | Fast pulsing crimson 2-ring |
| BloodSacrifice | **AttackAnimationFoundation** | 60-frame cinematic sequence |
| AmbientSmoke | **SmokeFoundation** (InfernalEmber) | Continuous 5-15 puffs/s |

---

## Cross-Theme Synergy Notes

### Dies Irae Theme Unity
All weapons share blood red + dark crimson + ember orange palette with wrath/judgment motifs:
- **Melee trinity**: Raw wrath (Cleaver) → Methodical judgment (Verdict) → Binding chains (Chain)
- **Ranged spectrum**: Escalating sin collection (Collector) → Area denial (Cannon) → Precision punishment (Sentence)
- **Magic trio**: Tactical mine-laying (Staff) → Continuous channeling (Grimoire) → Eclipse mechanics (Eclipse)
- **Summon family**: Toll-wave AOE (Bell) → Automated processing (Sigil) → Risk-reward demon (Contract)

### Foundation Usage Patterns
Every weapon uses at least 3 foundations. Most common combinations:
- **SwordSmearFoundation + ImpactFoundation**: All 3 melee weapons — swing arc + impact is the universal melee pattern
- **ExplosionParticlesFoundation + SmokeFoundation**: Paired for all explosive effects — sparks + smoke together create rich detonations
- **MaskFoundation**: Used by 8/12 weapons — RadialNoiseMaskShader with VoronoiCell + FBM noise layering is Dies Irae's **signature rendering technique** for entity bodies, sigils, cages, and corruption overlays
- **Noise masking everywhere**: FBM noise masks on bloom edges, Voronoi cracks on fire zones, double-noise layering for corruption. Dies Irae is defined by aggressive noise masking that makes every effect look **torn, cracked, and furious**.

### Noise Masking as Identity
Dies Irae uses noise masking more aggressively than any other theme:
- **FBMNoise**: Rolling fire, smoke churn, beam warping — organic fury
- **VoronoiCell**: Fractured cracks, cage bars, judgment sigils, ground fissures — structured wrath
- **Combined FBM + Voronoi**: Corruption overlays, Scorched Earth, eclipse bodies, demon forms — layered complexity
- `smoothstep` masks with low thresholds (0.15-0.35) create ragged, torn edges on everything. Nothing in Dies Irae is clean — every effect looks consumed by its own fire.

### Musical Motifs
- **Percussive impacts**: Screen shake on impacts (8-16px), heavy bloom flash stacking, bass-heavy visual weight
- **Building intensity**: Wrath Meter, Sin Counter, Chain Links, Tolled stacks — everything escalates toward threshold payoffs
- **Tolling bells**: Death Tolling Bell is literal, but rhythmic pulsing appears in mine arming, demon smoke, and judgment mark layering
- **Fire-as-wrath**: Fire is divine anger made physical. Every weapon uses fire differently: crystallized (Cleaver), focused (Sentence), eclipsed (Eclipse), tolling (Bell), contractual (Contract)