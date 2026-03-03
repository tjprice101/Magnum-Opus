# ⚔️ Eroica — Resonance Weapons Planning

> *"The hero's symphony — courage in crimson, sacrifice in gold."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Symphony No. 3 "Eroica" — the hero's journey |
| **Emotional Core** | Courage, sacrifice, triumphant glory |
| **Color Palette** | Scarlet, crimson, gold, sakura pink |
| **Palette Hex** | Deep Scarlet `(180, 30, 30)` → Heroic Crimson `(220, 50, 50)` → Ember Gold `(255, 180, 40)` → Sakura Pink `(255, 180, 200)` → Flame White `(255, 240, 230)` |
| **Lore Color** | `new Color(200, 50, 50)` — Scarlet |
| **Lore Keywords** | Heroism, sacrifice, glory, triumph, sakura, warrior, final stand |
| **VFX Language** | Sakura petals scattering, golden light breaking crimson, rising embers, heroic flame trails, funeral ash, triumphant bursts |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Celestial Valor | Melee | Heroic broadsword with valor slash arcs and beam projectiles |
| 2 | Sakura's Blossom | Melee | Sakura-petal-themed swing weapon with cherry blossom trails |
| 3 | Blossom of the Sakura | Ranged | Sakura blossom themed projectiles with tracer effects |
| 4 | Piercing Light of the Sakura | Ranged | Piercing light projectiles with radiant sakura VFX |
| 5 | Triumphant Fractal | Magic | Fractal magic with heroic energy patterns |
| 6 | Funeral Prayer | Magic | Dark heroic sacrifice magic with funeral ash effects |
| 7 | Finality of the Sakura | Summon | Final sakura fall summoner weapon |

---

## 1. Celestial Valor (Melee)

### Identity & Musical Soul
Celestial Valor is the Eroica's **first movement climax** — the heroic theme itself made into a blade. Every swing should feel like a triumphant declaration. This is the Hero's sword — not subtle, not quiet, but **gloriously, defiantly brilliant**. Slash arcs trail golden staff lines. Impacts explode in crimson and gold. The blade glows brighter as the combo advances, building to a crescendo of valor.

### Lore Line
*"To wield valor is to accept that every victory demands sacrifice."*

### Combat Mechanics
- **Heroic Crescendo Combo**: 4-phase combo that builds in intensity:
  - **Phase 1 — Resolute Strike**: Single heavy overhead slash. Clean, powerful.
  - **Phase 2 — Ascending Valor**: Upward diagonal slash + downward return. Fires ValorSlash arcs.
  - **Phase 3 — Crimson Legion**: Triple rapid strikes. Each spawns valor beam projectiles.
  - **Phase 4 — Finale Fortissimo**: 270° heroic slam. ValorBoom AoE detonation. Screen shake.
- **Valor Gauge**: Builds on successive hits (0-100). At max, next Phase 4 becomes *Gloria* — enhanced detonation with 2x AoE, sakura petal hurricane, screen flash.
- **Hero's Resolve**: While below 30% HP, all swings deal +25% damage and trail extra ember particles (the hero's final stand).

### VFX Architecture Plan

#### Custom Shaders (4)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ValorSlashArc.fx` | Heroic slash arc trail | UV-scrolled flame texture with noise distortion on outer edge. Color ramp: deep scarlet edge → gold center → white-hot core. Edge vignetting via smoothstep. |
| `ValorBeamTrail.fx` | Valor beam projectile trail | Thin strip trail with internal gold energy scroll. Additive. Taper at tail via smoothstep on UV.x. |
| `ValorBoomExplosion.fx` | Phase 4 AoE detonation | Radial SDF with expanding ring + FBM noise for chaotic flame edges. Crimson → gold → white cascade. Chromatic aberration at edges for impact. |
| `HeroicEmberAura.fx` | Low-HP hero's resolve aura | Radial particle-like shader around player. Rising ember shapes (noise-masked circles moving upward). Blend: additive. Color: deep crimson → ember gold. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SakuraPetalParticle | Drifts with gentle sine-wave flutter, gravity-affected | Pink-white cherry blossom petals, rotate as they fall, 40-60 frame lifetime |
| HeroicEmberParticle | Rises upward from blade edge with slight randomization | Orange-gold ember dots, 2-4px, gentle flicker, 20-35 frame life |
| ValorBurstParticle | Radial burst from impact, decelerating | Gold-white star shapes, 8-16 per impact, streaked with short trail |
| GoldenStaffLineParticle | Follows swing arc path, lingers 0.5s | Thin gold horizontal lines (like music staff lines), fade over 30 frames |
| FuneralAshParticle | Slow drift downward from defeated enemies | Dark grey-red ash flakes, very slow movement, 50-70 frame life |

#### Bloom Layers
1. **Blade ember aura**: Persistent crimson-gold glow along blade (tighter below 30% HP)
2. **Valor slash arc**: Wide additive bloom behind slash trail (gold, half-moon shape)
3. **Impact burst**: 3-layer bloom (tiny white core + medium gold + wide crimson ambient)
4. **Valor Boom**: 4-layer stacked bloom with screen vignette flash (white → gold → crimson → dark red)
5. **Gloria finale**: Full-screen golden flash (0.15s) + radial gold god-rays

#### Trail Rendering
- **Slash arc mesh**: 50-point semi-circular primitive strip, UV.x along arc, UV.y across width
- **Width function**: Bell curve — narrow at start, widest at mid-swing, narrow at end
- **Color ramp**: Inner hot (white-gold) → outer cool (deep crimson)
- **Afterimage**: 3 ghostly afterimage layers at 3/6/9 frame delays, decreasing opacity

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Heroic flame trail | `Assets/Eroica/CelestialValor/Trails/HeroicFlame.png` | "Horizontal flame energy trail, deep crimson to bright gold gradient, stylized fire with ember particles, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Sakura slash overlay | `Assets/Eroica/CelestialValor/SlashArcs/SakuraSlash.png` | "Curved sword slash arc with scattered cherry blossom petals embedded, pink-gold gradient, on solid black background, 256x128px --ar 2:1 --style raw" |
| Valor burst flare | `Assets/Eroica/CelestialValor/Flare/ValorBurst.png` | "Radiant 6-pointed star burst with golden rays and crimson corona, heroic energy explosion, on solid black background, 256x256px --ar 1:1 --style raw" |
| Gloria god-ray texture | `Assets/Eroica/CelestialValor/Flare/GloriaRays.png` | "Radial god-ray light beams emanating from center, golden light with soft warm glow, on solid black background, 512x512px --ar 1:1 --style raw" |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| ValorMark | +15% damage taken from Eroica weapons | 180 frames |
| HeroicIgnition | DoT (fire damage, scaling with combo phase) | 120 frames |

---

## 2. Sakura's Blossom (Melee)

### Identity & Musical Soul
If Celestial Valor is the Eroica's *triumph*, Sakura's Blossom is its *beauty*. This is the **second movement's funeral march** — the hero's sacrifice, beautiful and inevitable. Each swing should scatter petals like a warrior's final breath. Where Valor burns bright, Blossom **falls gracefully**. The combat should feel like a dance — fluid, elegant, bittersweet.

### Lore Line
*"Every petal that falls is a promise kept."*

### Combat Mechanics
- **Petal Dance Combo**: 3-phase flowing combo:
  - **Phase 1 — First Petal**: Graceful horizontal sweep. Spawns petal trail that lingers.
  - **Phase 2 — Scattered Petals**: Cross-slash (X pattern). Spawns 8 petal projectiles that drift outward and home weakly.
  - **Phase 3 — Final Bloom**: Upward flourish + downward finisher. 360° petal burst on final hit. All lingering petals converge on struck enemy.
- **Blossom Counter**: Perfect-timing mechanic — if enemy projectile passes through petal trail within 0.5s of swing, all petals absorb the projectile and fire back as homing petal-blades (reflect mechanic).
- **Sakura Meditation**: Static hold (hold fire button without target) — player enters petal stance. After 1.5s, next swing has 2x arc range and spawns double petals.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SakuraSwingTrail.fx` | Pink-white flowing swing trail | UV-scrolled petal texture scroll, gentle noise distortion (Perlin, low intensity) for organic movement. Color: sakura pink core → white edge → transparent. Smoothstep Y-axis fade. |
| `SakuraPetalDrift.fx` | Petal projectile body glow | Small quad shader with rotating UV + gentle color oscillation between pink and gold. Additive. |
| `SakuraConvergence.fx` | Petal convergence burst | Radial implosion shader — particles pull INWARD toward center then burst OUT. Pink-white → crimson flash at convergence moment. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CherryPetalParticle | Flutter with sin(x) wobble, slow gravity, air resistance | Pink-white petal shapes, rotating on 2 axes, 50-80 frame lifetime |
| PetalTrailMoteParticle | Stationary where swing passed, gentle drift, slow fade | Tiny pink dots, 2-3px, form line along swing path, 40 frame lifetime |
| BlossomBurstParticle | Radial burst, decelerating, slight upward bias | Full blossom flower shapes (5-petal), gold center, pink petals, 25 frames |
| SakuraGlintParticle | Sparkle at blade edge during meditation stance | 4-pointed star, alternating pink and gold, quick flash |

#### Bloom Layers
1. **Blade petal glow**: Soft pink-white bloom along blade, breathing pulse
2. **Swing trail bloom**: Wide soft bloom following swing arc (sakura pink, additive)
3. **Impact petal burst**: Pink flash + circular bloom ring expanding (2-layer)
4. **Convergence flash**: Bright white implosion point with pink ambient ring
5. **Meditation aura**: Concentric pink rings pulsing around player

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Petal trail texture | `Assets/Eroica/SakurasBlossom/Trails/PetalFlow.png` | "Flowing cherry blossom petal trail with scattered petals, soft pink to white gradient, anime styled, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Sakura burst texture | `Assets/Eroica/SakurasBlossom/ImpactSlash/BloomBurst.png` | "Circular cherry blossom explosion with petals spiraling outward, pink white and gold, on solid black background, 256x256px --ar 1:1 --style raw" |
| Meditation circle | `Assets/Eroica/SakurasBlossom/Orbs/MeditationCircle.png` | "Concentric thin rings of cherry blossom energy, soft pink glow, elegant and simple, on solid black background, 256x256px --ar 1:1 --style raw" |

---

## 3. Blossom of the Sakura (Ranged)

### Identity & Musical Soul
Where Sakura's Blossom fights in melee with dancer's grace, Blossom of the Sakura fights from afar — a **ranged archer raining sakura arrows** from above. This is the Eroica's **third movement scherzo** — playful, rapid, cascading. Every shot should feel like loosed arrows becoming cherry blossoms mid-flight.

### Lore Line
*"The blossoms do not choose where they fall; they trust the wind."*

### Combat Mechanics
- **Blossom Arrows**: Primary fire — arrows that bloom into petal explosions on contact. Each arrow trails petals.
- **Petal Storm**: Alt fire (charges) — charges for 1s, then fires a volley of 12 arrows at spread angles that rain from above (fall toward cursor position). Each arrow leaves a petal trail. On landing, all petals combine into a brief petal storm AoE.
- **Tracer Blossoms**: Every 5th shot is a Tracer Blossom — a homing shot that marks the target. Marked targets take +10% damage from subsequent arrows for 3 seconds.
- **Petal Shield**: If a petal storm AoE is active and the player stands in it, they gain 10% damage reduction for the duration.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `BlossomArrowTrail.fx` | Arrow trail with embedded petals | Thin strip trail, UV-scrolled. Texture has embedded petal silhouettes. Pink-to-transparent gradient along UV.x (head to tail). |
| `PetalStormAoE.fx` | Ground-level AoE storm of petals | Circular SDF with rotating UV-scroll of petal-pattern texture. Multiple layers at different rotation speeds for depth. Pink-white gradient. |
| `TracerBlossomMark.fx` | Enemy mark indicator | Small circular SDF with rotating sakura glyph. Pulsing pink outline. Additive blend. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| BlossomArrowPetalParticle | Shed from arrow trail, flutter downward | Sakura petals (pink), slow drift, 30-50 frame lifetime |
| PetalStormParticle | Swirl in circle within AoE, high density | Rapid-spinning petals (pink+gold), 20 frame lifetime, constant spawn |
| TracerGlintParticle | Orbits marked target at close radius | Small sakura-pink star, 4-6 orbiting, slow rotation |
| ImpactBlossomBurstParticle | Radial burst on arrow impact | Mix of petals + gold sparks, 10-15 per impact |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Arrow petal trail | `Assets/Eroica/BlossomOfTheSakura/Trails/BlossomArrow.png` | "Thin arrow trail with embedded cherry blossom petals, pink energy fading to transparent, on solid black background, 256x32px seamless --ar 8:1 --style raw" |
| Petal storm pattern | `Assets/Eroica/BlossomOfTheSakura/Orbs/PetalStormPattern.png` | "Circular pattern of swirling cherry blossom petals viewed from above, pink and white, dense petal coverage, on solid black background, 256x256px --ar 1:1 --style raw" |
| Tracer mark | `Assets/Eroica/BlossomOfTheSakura/Flare/TracerMark.png` | "Small circular sakura flower symbol, thin pink outline, elegant design, on solid black background, 64x64px --ar 1:1 --style raw" |

---

## 4. Piercing Light of the Sakura (Ranged)

### Identity & Musical Soul
This is the Eroica's **piercing truth** — the hero's light that cannot be stopped or deflected. While Blossom of the Sakura rains petals, Piercing Light focuses them into a concentrated beam of pure sakura radiance. This weapon channels the **finale of the first movement** — that moment when the heroic theme crashes through all opposition with unstoppable force.

### Lore Line
*"The light that pierces is the one that never faltered."*

### Combat Mechanics
- **Sakura Piercer**: Primary fire — fast, narrow projectiles that pierce through 3 enemies. Each pierced enemy creates a small radial burst of petals.
- **Culmination Shot**: Every 8th shot becomes a Culmination — thicker beam that pierces infinitely and leaves a persistent 2s light trail that damages enemies touching it.
- **Radiant Intensification**: Successive hits on the same target within 2 seconds stack Radiant Marks (max 5). Each mark adds a golden light ring around the target. At 5 marks, all marks detonate in a massive Radiant Sakura Burst.
- **Hero's Final Light**: When player is below 20% HP, Culmination shots fire every 4th shot instead of 8th.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `PiercingLightTrail.fx` | Piercing projectile trail | Tight thin trail strip, brilliant white core with gold-pink edges. UV.x scrolls fast. Edge fade via smoothstep. High intensity additive. |
| `CulminationBeam.fx` | Culmination persistent beam | Wider beam body with internal energy scroll. Gold → white → gold internal oscillation. Lingering afterimage trail with slow opacity decay. |
| `RadiantSakuraBurst.fx` | 5-mark detonation explosion | Expanding SDF ring + star pattern overlay. Gold-white center → crimson-pink outer. Chromatic aberration at peak expansion. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PierceBurstParticle | Radial mini-burst at each pierce point | Gold-pink sparks, 5-8 per pierce, fast outward, 15 frame life |
| CulminationTrailParticle | Slowly rises from persistent light trail | Gold motes drifting upward, 3-4px, gentle, 25 frame life |
| RadiantMarkParticle | Orbits marked target in ring formation | Golden light dots, 1 per mark stack, tight orbit radius |
| RadiantBurstSakuraParticle | Massive radial burst at 5-mark detonation | Large sakura petals + gold light shards, 20-30 particles, deceleration |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Piercing trail texture | `Assets/Eroica/PiercingLightOfTheSakura/Trails/PiercingLight.png` | "Narrow high-intensity light beam trail, brilliant gold-white center with pink-crimson edges, on solid black background, 256x32px seamless --ar 8:1 --style raw" |
| Radiant burst texture | `Assets/Eroica/PiercingLightOfTheSakura/Flare/RadiantBurst.png` | "Radial starburst explosion with golden light rays and scattered cherry blossom petals, heroic energy, on solid black background, 256x256px --ar 1:1 --style raw" |

---

## 5. Triumphant Fractal (Magic)

### Identity & Musical Soul
The Triumphant Fractal is the Eroica's **mathematical beauty** — triumphant patterns that repeat at every scale. This is the **development section** of the symphony — where themes are broken apart, recombined, and rebuilt into something greater. Each spell should spawn self-similar patterns: fractals of fire and gold that split and multiply.

### Lore Line
*"In every fragment of heroism, the whole sacrifice echoes."*

### Combat Mechanics
- **Fractal Bolt**: Primary fire — projectile that splits into 3 smaller copies on impact, which each split into 3 again (2 generations). Each generation is smaller and deals 40% of parent damage.
- **Heroic Resonance**: Fractal fragments that overlap create Resonance Zones — brief AoE damage patches where multiple fragments intersect.
- **Fractal Shield**: Alt fire — creates a fractal barrier in front of the player (5s duration, 30s cooldown). The barrier absorbs projectiles. Each absorbed projectile adds 1 fractal charge. Release charges as rapid-fire fractal bolts at held direction.
- **Triumph Accumulator**: After 10 kills with fractal fragments, the next primary fire shoots a Triumphant Fractal — a massive bolt that splits 4 ways, each splits 4 ways (4^2 = 64 tiny fragments).

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FractalBoltBody.fx` | Fractal projectile body | SDF hexagonal shape with internal noise energy. Gold → crimson gradient. Additive glow around edges. Size parameter for generation scaling. |
| `FractalResonanceZone.fx` | Overlap damage zone indicator | Circular SDF with Sierpinski-triangle-like fractal pattern (approximated with UV math). Pulsing gold outline on crimson field. |
| `FractalBarrier.fx` | Fractal shield barrier | Rectangular SDF with fractal edge pattern. Gold-crimson gradient fill. Shimmer oscillation on edges. Absorbed projectile pulse animation. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| FractalShardParticle | Bursts at each split point, outward | Gold-red triangular shards, rotating, 15-20 frame life |
| ResonanceSparkParticle | Rises from resonance zones | Gold sparkles, slow upward drift, gentle pulsing |
| FractalSplitFlashParticle | Brief flash at moment of fractal splitting | White-gold flash, 2-3 frame burst, additive |
| BarrierAbsorbParticle | Ripple from absorption point on barrier | Concentric crimson ring expanding from point, 10 frame life |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Fractal bolt texture | `Assets/Eroica/TriumphantFractal/Orbs/FractalBolt.png` | "Hexagonal fractal energy orb with self-similar triangular patterns inside, gold and crimson colors, on solid black background, 128x128px --ar 1:1 --style raw" |
| Fractal barrier texture | `Assets/Eroica/TriumphantFractal/Orbs/FractalBarrier.png` | "Rectangular energy shield with fractal geometry pattern, gold edges on crimson field, on solid black background, 256x128px --ar 2:1 --style raw" |
| Resonance zone | `Assets/Eroica/TriumphantFractal/Orbs/ResonanceZone.png` | "Circular fractal pattern (Sierpinski triangle approximation), golden lines on transparent, on solid black background, 128x128px --ar 1:1 --style raw" |

---

## 6. Funeral Prayer (Magic)

### Identity & Musical Soul
The Funeral Prayer is the Eroica's **second movement** — *Marcia funebre (Funeral March)*. Where other Eroica weapons celebrate heroism, this weapon mourns its cost. Dark, solemn, powerful. The magic conjured here feels like a prayer for the fallen — ash and embers rising from burning sacrifice. This is the weight of the hero's burden.

### Lore Line
*"We pray not for victory. We pray for those who ensured it."*

### Combat Mechanics
- **Funeral Pyre**: Primary fire — launches a slow-moving funeral pyre projectile (dark red flame with grey ash particles). On contact, creates a lasting ground pyre (4s) that deals persistent AoE damage.
- **Ash Requiem**: Alt fire — channels for 2s, then releases a wave of funeral ash in a cone. Enemies caught gain the Ash Requiem debuff (take 20% more damage for 5s, leave ash trail as they move).
- **Martyr's Exchange**: Every time the player takes damage while this weapon is active, the next Funeral Pyre is empowered (+50% size, +30% damage).
- **Eulogy**: After 3 successive Funeral Pyres overlap on the same enemy simultaneously, they merge into a Eulogy — a massive dark crimson pillar that deals devastating damage and applies silence.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `FuneralPyreFlame.fx` | Funeral pyre ground fire | Animated fire shader using FBM noise for flame shape generation. Color: very dark crimson base → ember gold tips → grey-white ash at edges. Upward UV scroll. |
| `AshRequiemCone.fx` | Ash wave cone attack | Cone-shaped mesh with scrolling ash particle texture. Dark grey → crimson gradient. Noise distortion on edges for organic spread. |
| `EulogyPillar.fx` | Eulogy pillar VFX | Vertical beam shader with rising embers (noise-masked circles) and dark crimson-to-black gradient. Screen darkening aura around base. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| FuneralAshParticle | Drifts upward slowly, slight horizontal wobble | Dark grey ash flakes, 3-6px, rotating, 40-60 frame life |
| PyreEmberParticle | Rises from ground pyre, decelerates, fades | Orange-red embers, 2-4px, gentle flicker, 25-40 frame life |
| AshTrailParticle | Falls from enemies with Ash Requiem debuff | Grey-white ash, slow gravity, small, 20 frame life |
| EulogySoulParticle | Rises rapidly from Eulogy pillar center | Ghostly white wisps, fast upward, elongated shape, 15 frame life |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Pyre flame texture | `Assets/Eroica/FuneralPrayer/Orbs/PyreFlame.png` | "Dark funeral pyre flame texture, very dark crimson base fading through orange embers to grey ash at tips, somber fire, on solid black background, 128x256px --ar 1:2 --style raw" |
| Ash wave texture | `Assets/Eroica/FuneralPrayer/Trails/AshWave.png` | "Wave of funeral ash spreading horizontally, dark grey with crimson undertones, particles and flakes visible, on solid black background, 256x128px seamless --ar 2:1 --style raw" |
| Eulogy pillar texture | `Assets/Eroica/FuneralPrayer/Beams/EulogyPillar.png` | "Vertical dark crimson energy pillar with rising embers and ash particles, somber and powerful, on solid black background, 64x512px --ar 1:8 --style raw" |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| AshRequiemDebuff | +20% damage taken, leaves ash trail | 300 frames (5s) |
| FuneralSilence | Cannot regenerate HP or mana | 120 frames |

---

## 7. Finality of the Sakura (Summon)

### Identity & Musical Soul
Finality of the Sakura is the Eroica's **coda** — the final notes after the finale. This summoner weapon calls forth a **sakura spirit guardian** — the ghost of a fallen hero who fights on beyond death. The minion should feel both beautiful and melancholy — a spectral sakura tree that attacks with petal blade projections.

### Lore Line
*"The sakura does not mourn its own falling; it becomes the wind."*

### Combat Mechanics
- **Sakura Spirit**: Summons a floating spectral cherry blossom tree minion. The minion hovers near the player and attacks targeted enemies.
- **Attack Pattern — Petal Blade Storm**: The tree fires rapid petal-blade projectiles (5 per volley, 2s cooldown). Petals pierce once and deal moderate damage.
- **Sakura Shield**: The tree creates a petal barrier around the player every 8 seconds (absorbs 1 hit, brief 0.5s duration — requires timing).
- **Final Bloom Attack**: Every 15 seconds, the minion charges and releases a massive petal supernova (large AoE, 3x regular petal damage). During charge, the tree glows bright gold for 2 seconds as visual warning/excitement.
- **Death Echo**: When the player takes lethal damage, the sakura spirit sacrifices itself to prevent death once (120s cooldown). Upon sacrifice, all enemies on screen receive massive petal damage.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SakuraSpiritAura.fx` | Spectral tree ambient glow | Radial SDF with organic Perlin edge. Color: translucent sakura pink → ghostly white → transparent. Gentle pulse. Alpha blend for spectral feel. |
| `SakuraSpiritAttack.fx` | Petal blade projectile trail | Thin strip trail, pink-white with petal texture overlay. UV-scrolled along length. Additive blend. |
| `FinalBloomExplosion.fx` | Final Bloom massive petal AoE | Expanding SDF circle with rotating petal-pattern texture overlay. Gold → pink → white gradient cascade. 4-layer render for depth. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SpiritPetalParticle | Orbits minion tree gently, slow outward drift | Translucent pink petals, spectral glow, 60-80 frame lifetime |
| PetalBladeTrailParticle | Sheds from petal blade projectiles | Tiny pink sparks, 2-3px, quick fade, 10 frame life |
| FinalBloomChargeParticle | Spirals inward toward tree during charge | Gold-white motes, logarithmic spiral, consumed at tree center |
| DeathEchoWaveParticle | Expands outward from sacrifice point as full ring | Ghostly white-pink wave ring, expanding rapidly, 20 frame life |

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Spirit tree entity | `Assets/Eroica/FinalityOfTheSakura/Orbs/SakuraSpirit.png` | "Ghostly spectral cherry blossom tree, translucent pink-white, ethereal glow, small floating tree shape about 48x48px, on solid black background --ar 1:1 --style raw" |
| Final bloom burst | `Assets/Eroica/FinalityOfTheSakura/Flare/FinalBloom.png` | "Massive circular cherry blossom explosion supernova, hundreds of petals spiraling outward, gold center fading to pink edge, on solid black background, 256x256px --ar 1:1 --style raw" |
| Petal blade texture | `Assets/Eroica/FinalityOfTheSakura/Trails/PetalBlade.png` | "Sharp petal-shaped blade projectile, pink-white gradient with sharp leading edge, on solid black background, 32x64px --ar 1:2 --style raw" |

---

## Cross-Theme Synergy Notes

### Eroica Theme Unity
All 7 weapons share the scarlet-crimson-gold-sakura palette but each uses it differently:
- **Celestial Valor**: Heavy on gold + crimson flame — the hero triumphant
- **Sakura's Blossom**: Heavy on sakura pink + white — the hero's beauty
- **Blossom of the Sakura**: Pink petal + gold accent — the ranged rain
- **Piercing Light**: Brilliant gold-white + pink edge — the focused light
- **Triumphant Fractal**: Gold + crimson geometric — the mathematical hero
- **Funeral Prayer**: Dark crimson + ash grey + ember gold — the hero's cost
- **Finality of the Sakura**: Translucent pink-white + ghostly — the hero's spirit

### Musical Motifs Across All Weapons
- Sakura petals appear in all weapons but with different behaviors (falling, piercing, orbiting, rising, spiraling)
- Gold light represents heroism — brighter gold = more heroic moment
- Dark crimson represents sacrifice — darker moments use deeper crimson
- Ember particles = the fire of heroic will — present in all weapons during intense moments
- Music notes should take the form of heroic brass/orchestral motifs when spawned via MoonlightVFXLibrary-equivalent helpers
