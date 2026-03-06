# Swan Lake Resonant Weapons — Comprehensive VFX Audit

> Generated from full source code review of all 6 weapons, 12 projectiles, 6 shader loaders, 6 primitive renderers, shared infrastructure (SwanLakePalette, SwanLakeShaderManager, SwanLakeVFXLibrary, SwanLakeThemeTextures), and all utility files.

---

## EXECUTIVE SUMMARY

### The Critical Problem

Every Swan Lake weapon has **three layers of VFX infrastructure that are fully built but completely disconnected from the actual rendering code**:

| Infrastructure Layer | Status | Used in Rendering? |
|---|---|---|
| **12 custom HLSL shaders** (.fx + .fxc compiled) | ✅ Exist & compile | ❌ **NEVER CALLED** from any projectile |
| **6 GPU primitive trail renderers** (triangle-strip mesh builders) | ✅ Exist & complete | ❌ **NEVER INSTANTIATED** by any projectile |
| **6 custom particle systems** (handler + types + base class per weapon) | ✅ Exist | ❌ **NEVER SPAWNED** from any projectile PreDraw¹ |
| **SwanLakeShaderManager** (12 preset methods, noise textures, fallback chains) | ✅ 636 lines of infrastructure | ❌ **NEVER CALLED** from any projectile |

¹ Exception: IridescentWingspan's item HoldItem uses WingspanParticleHandler for hold effects.

**All 12 projectile PreDraw methods use identical "foundation-pattern rendering"** — manually managed SpriteBatch state switches to additive blending, drawing MagnumTextureRegistry bloom textures (SoftGlow, RadialBloom, PointBloom, Star4Soft) as stacked sprites along `oldPos` arrays. No GPU primitives. No shaders. No custom particles in rendering.

### Quality Assessment Summary

| Weapon | Class | VFX Rating | Key Issue |
|---|---|---|---|
| Call of the Black Swan | Melee | ⭐⭐⭐ 3/5 | Best of the set. Multi-layer swing trail + tip bloom + theme accents. Still no shaders. |
| Call of the Pearlescent Lake | Ranged | ⭐⭐½ 2.5/5 | Decent projectile bloom + splash zone. Rocket trail is just bloom dots on oldPos. |
| Chromatic Swan Song | Magic | ⭐⭐⭐ 3/5 | Aria detonation is rich (multi-ring, Opus variant). Bolt itself is simple spectrum cycling. |
| Feather of the Iridescent Flock | Summoner | ⭐⭐½ 2.5/5 | Formation lines are nice detail. Individual crystal bloom is standard. |
| Iridescent Wingspan | Magic | ⭐⭐½ 2.5/5 | Good HoldItem particles (only weapon using custom particles). Bolt rendering is standard. |
| The Swan's Lament | Ranged | ⭐⭐ 2/5 | Uses BeamStreak texture (unique). Otherwise minimal bloom layers. |

**For comparison, a "properly polished" Calamity-tier weapon would rate 4.5-5/5** with shader-driven trails, multi-pass GPU primitives, screen effects on critical hits, and layered particle choreography on top.

---

## SHARED INFRASTRUCTURE

### SwanLakePalette.cs (488 lines)
**Quality: ✅ Excellent — well-structured, comprehensive**

- **6 core theme colors**: ObsidianBlack → DarkSilver → Silver → PureWhite → PrismaticShimmer → RainbowFlash
- **Extended palette**: FeatherWhite, FeatherBlack, LakeSurface, Pearlescent, BalletPink, CurseViolet, ShadowCore, GracefulArc, DyingBeauty
- **7 per-weapon 6-color palettes** (BlackSwanBlade, ChromaticSong, SwansLament, IridescentFlock, Wingspan, PearlescentLake, BlackSwanSwing)
- Gradient helpers: `GetGradient`, `GetDualPolarityGradient`, `GetSwanLakeGradient`, `GetRevelationGradient`, `GetBlackSwanGradient`, `GetWhiteSwanGradient`, `GetRainbow`, `GetVividRainbow`
- Item PreDraw bloom presets: `DrawItemBloom` (3-layer), `DrawItemBloomEnhanced` (4-layer with prismatic)

### SwanLakeShaderManager.cs (636 lines)
**Quality: ✅ Excellent infrastructure... ❌ completely unused**

- Defines **12 dedicated shader presets** (2 per weapon): DualPolaritySwing, SwanFlareTrail, PearlescentRocketTrail, LakeExplosion, ChromaticTrail, AriaExplosion, CrystalOrbitTrail, FlockAura, EtherealWing, WingspanFlareTrail, LamentBulletTrail, DestructionRevelation
- Each has Main + Glow techniques with full parameter binding (`uColor`, `uSecondaryColor`, `uTime`, `uOpacity`, `uIntensity`, `uOverbrightMult`, `uScrollSpeed`, `uDistortionAmt`, `uNoiseScale`, `uPhase`, `uHasSecondaryTex`)
- Binds noise textures: SoftCircularCaustics, CosmicEnergyVortex, SparklyNoiseTexture, RealityCrackPattern, TileableFBMNoise
- Fallback chain per weapon: dedicated shader → SwanLakeTrail → ScrollingTrailShader
- SpriteBatch state helpers: `BeginShaderAdditive`, `RestoreSpriteBatch`, `BeginAdditive`
- **No projectile ever calls any of these methods.**

### SwanLakeVFXLibrary.cs (811 lines)
**Quality: ✅ Good — partially used**

Used features:
- `SpawnMusicNotes` — called from BlackSwanSwingProj, BlackSwanFlareProj, AriaDetonestionProj hit events
- `SpawnPrismaticSparkles` — called from BlackSwanSwingProj hit events
- `SpawnFeatherDrift` — called from FlareProj OnKill
- `MeleeImpact` — called from BlackSwanSwingProj crit hits
- `DrawThemeCrystalAccent` / `DrawThemeImpactRing` — called via Utils.DrawThemeAccents from some PreDraw methods

Unused features:
- `DrawSwanBloomStack` (4-layer) — never called
- `DrawBloomSandwichLayer`, `DrawCounterRotatingFlares` — never called
- `DrawSwanGlow` / `SwanGlowProfile` (GlowRenderer integration) — never called
- `SwanTrailWidth`, `PrecisionTrailWidth`, `GracefulTrailWidth` — never called
- `SpawnOrbitingNotes` — never called
- `SpawnDualPolarityDust`, `SpawnRadialDustBurst`, `SpawnRainbowBurst`, `SpawnRainbowShimmer` — never called
- `SpawnFeatherBurst`, `SpawnFeatherDuality` — never called
- `SpawnPrismaticSwirl`, `SpawnRainbowExplosion` — never called
- `SpawnGradientHaloRings`, `SpawnRainbowHaloRings` — never called
- `ProjectileImpact`, `SwingFrameVFX`, `FinisherSlam` — never called
- `DrawThemeImpactFull` — never called

### SwanLakeThemeTextures.cs (92 lines)
**Quality: ⚠️ Loaded but mostly unused**

Lazy-loads from `Assets/VFX Asset Library/Theme Specific/Swan Lake/`:
- SLEnergyMotionBeam, SLEnergySurgeBeam, SLHarmonicImpact, SLPowerEffectRing, SLCrystalShard, SLBasicTrail, SLHarmonicRibbon, SLGradient

These textures are only referenced through `DrawThemeAccents` utility methods — not directly from any projectile rendering.

---

## WEAPON 1: CALL OF THE BLACK SWAN

### Stats
| Property | Value |
|---|---|
| **Class** | MeleeNoSpeed |
| **Damage** | 400 |
| **Use Time** | 28 |
| **Crit** | 15% |
| **Knockback** | 7 |
| **Style** | Exoblade-style channel+hold greatsword |

### Attack Mechanics
- **3-phase combo**: Entrechat (155px blade, 20 ticks) → Fouetté (160px, 24 ticks) → Grand Jeté (175px, 28 ticks)
- **Swan's Grace stacks**: Max 5, +8% attack speed each, gained on hit
- **Dark Mirror stacks**: +15% damage, -5% speed, gained when Grace = 5
- **Empowered state**: Dark Mirror stacks ≥3, enhances Phase 2 effects
- **Sub-projectiles**: Phase 0 = 3 flare fan, Phase 1 = 5-8 radial flares, Phase 2 = shockwave + 5 feather rain

### VFX Breakdown

#### HoldItem (CalloftheBlackSwan.cs)
- **Grace feedback**: Orbiting WhiteTorch/RainbowTorch dust around player (1 per Grace stack)
- **Dark Mirror feedback**: Orbiting Shadowflame dust (1 per DarkMirror stack), plus RainbowTorch if empowered
- **Base glow**: Single WhiteTorch dust shower

#### PostDrawInWorld (CalloftheBlackSwan.cs)
- Simple 2-layer offset glow: black offset shadow + white offset highlight, both using item texture

#### BlackSwanSwingProj.cs (852 lines)
**State machine**: Piecewise CurveSegment animation (Calamity-style). Direction management, squish factor, combo phase tracking. 40 trail points computed per frame along arc.

**DoSwingVFX** (per-frame particles):
- Only vanilla Dust: Alternating `Shadowflame` (black) / `WhiteTorch` (white), some `RainbowTorch` accents
- No custom particles spawned during swing animation
- Dust emitted from blade length with velocity perpendicular to blade

**SpawnFlares** (phase transition events):
- Phase 0: 3 BlackSwanFlareProj in fan spread
- Phase 1: 5-8 radial flares (empowered = rainbow burst with 12 RainbowTorch)
- Phase 2: Shockwave + 5 feather rain projectiles

**OnHitNPC**:
- 20 vanilla Dust sparks (Shadowflame/WhiteTorch)
- `SwanLakeVFXLibrary.SpawnMusicNotes` (5 notes)
- `SwanLakeVFXLibrary.SpawnPrismaticSparkles` (3 sparkles)
- On crit: `SwanLakeVFXLibrary.MeleeImpact`

**PreDraw** (rendering):
- **Layer 1 — Swing arc trail**: SpriteBatch → Additive. Draws SoftGlow bloom circles at every 2nd trail point along the arc. Each point gets:
  - Outer halo (polarity-colored, 2.5× scale, 0.4 opacity)
  - Mid glow (silver, 1.5× scale, 0.5 opacity)
  - White core (0.8× scale, 0.6 opacity)
  - Rainbow accent (cycling hue, 3.0× scale, varies with Grace stacks)
  - Fades in (0.20-0.35 progression) and out (0.85-1.0 progression)
- **Layer 2 — Tip bloom**: 4-layer at blade tip:
  - Wide polarity halo (RadialBloom if available, else SoftGlow, 0.6× scale)
  - Silver mid glow (0.35× scale)
  - White-hot core (PointBloom, 0.15× scale)
  - Rainbow star accent (Star4Soft, rotating, 0.12× scale)
  - Empowered Phase 2: additional prismatic outer halo
- **Layer 3 — Blade sprite**: Shadow offset + main texture + polarity overlay glow
- **Layer 4 — Theme accents**: `BlackSwanUtils.DrawThemeAccents` → VFXLibrary crystal accent + impact ring

#### BlackSwanFlareProj.cs
Homing sub-projectile, dual polarity (random black/white), HomingRange 350, HomingStrength 0.08.
- **Trail**: SoftGlow on oldPos with polarity color + white core
- **Core bloom**: 4-layer — RadialBloom outer → RadialBloom mid → PointBloom white core → Star4Soft rainbow accent
- **Empowered**: additional rainbow outer ring
- **OnKill**: SpawnMusicNotes + SpawnFeatherDrift

### What's Missing

| Missing Element | Impact |
|---|---|
| **DualPolaritySwing.fx shader** (compiled, registered, never called) | The swing trail should use GPU primitive mesh + this shader for proper UV-scrolled energy patterns instead of bloom dot stamps |
| **SwanFlareTrail.fx shader** (compiled, registered, never called) | Flare projectile trail should be a smooth GPU primitive ribbon, not bloom dots on oldPos |
| **BlackSwanPrimitiveRenderer** (complete class, never instantiated) | 230+ lines of triangle-strip mesh builder with Catmull-Rom smoothing, completely dead code |
| **BlackSwanParticle system** (3 files, never used) | Custom particles with unique behavior, never spawned |
| **Screen effects** on empowered finisher | Phase 2 Grand Jeté = most powerful attack, gets no screen shake or flash |
| **Afterimage system** | No motion trail / afterimage during fast swing phases |
| **Swing smear texture** | No slash arc overlay texture — just bloom dots along arc positions |

### Quality Assessment: ⭐⭐⭐ 3/5
**Best of the six** thanks to multi-layer trail + tip bloom + theme accents + homing sub-projectiles. But the swing rendering is still bloom-dot-dots-along-a-line rather than a proper GPU primitive slash arc with shader-driven energy patterns. The DualPolaritySwing.fx shader was specifically designed for this weapon's black↔white polarity and sits unused.

---

## WEAPON 2: CALL OF THE PEARLESCENT LAKE

### Stats
| Property | Value |
|---|---|
| **Class** | Ranged |
| **Damage** | 380 |
| **Use Time** | 8 |
| **Crit** | 12% |
| **Knockback** | 3 |
| **Style** | Rapid-fire gun with special rockets |

### Attack Mechanics
- **Rapid fire**: useTime 8, uses ammo, fires PearlescentRocketProj
- **Every 8th shot**: Tidal Rocket variant (1.4× damage, stronger homing)
- **Still Waters**: Standing still for 1.5s creates a pearlescent zone (StillWaters variant)
- **Muzzle flash**: 3 WhiteTorch dust per shot

### VFX Breakdown

#### PearlescentRocketProj.cs
- **Flight**: Sine-wave wobble (3px amplitude, ≈0.1 frequency), position offset perpendicular to velocity
- **AI dust**: WhiteTorch (pearlescent-tinted), TintableDustLighted for shimmer
- **Custom trail array**: 18 points, manually tracked each frame
- **OnKill**: Spawns SplashZoneProj + 3 concentric dust rings (inner 6 + mid 10 + outer 8) + SpawnFeatherDrift

**PreDraw**:
- **Trail**: SoftGlow on custom trail array. Each point gets rainbow-tinted pearlescent + white core. Simple opacity fade along trail length.
- **Core bloom**: 3-layer — RadialBloom outer (pearlescent opal) → SoftGlow mid (opalescent shimmer) → PointBloom white core + 6-dot rainbow shimmer ring
- **Theme accents**: `PearlescentUtils.DrawThemeAccents` in separate additive pass

#### SplashZoneProj.cs
5s AoE damage field, max 120px radius, 25% slow debuff.
- **AI**: Edge dust ring + interior shimmer (duration-based spawning)
- **PreDraw**: Foundation pattern:
  - RadialBloom backing (pearlescent tint, scale = radius)
  - SoftGlow shimmer layer
  - PointBloom edge ring (24 dots around circumference)
  - 8-dot rainbow accent ring

### What's Missing

| Missing Element | Impact |
|---|---|
| **PearlescentRocketTrail.fx shader** | Rocket should have flowing opal-shimmer energy trail, not bloom dot stamps |
| **LakeExplosion.fx shader** | Splash zone should render with concentric water-ripple shader |
| **PearlescentPrimitiveRenderer** | Never instantiated — rocket trail should be GPU primitive ribbon |
| **PearlescentParticle system** (3 files) | Never spawned from rendering code |
| **Splash zone shader rendering** | AoE zone is just a scaled bloom sprite — should be animated ripple shader |
| **Muzzle flash VFX** | Only 3 vanilla dust particles. No bloom flash, no additive flare sprite |
| **Impact explosion** | OnKill is only dust rings. No bloom explosion, no screen distortion |
| **Tidal rocket visual distinction** | Tidal variant has slightly different colors but same rendering |

### Quality Assessment: ⭐⭐½ 2.5/5
The splash zone is a visible circular area (good for gameplay readability), and the rocket has a recognizable multi-point trail. But everything is bloom-texture sprites — no flowing energy, no water ripple effects, no muzzle flash presence. For a 380-damage rapid-fire weapon, the per-shot VFX feels minimal.

---

## WEAPON 3: CHROMATIC SWAN SONG

### Stats
| Property | Value |
|---|---|
| **Class** | Magic |
| **Damage** | 290 |
| **Use Time** | 12 |
| **Mana** | 8 |
| **Crit** | 8% |
| **Style** | Magic pistol with chromatic scale mechanic |

### Attack Mechanics
- **Chromatic scale system**: Each shot advances through C-D-E-F-G-A-B. After 7 casts = Opus Detonation (2× damage, larger radius)
- **Harmonic stacks**: Gained on hit, 5 = charged (enhanced detonations)
- **Dying Breath**: <30% HP = double speed + expanded radius
- **AriaDetonationProj**: Spawns on EVERY hit (3 modes: normal, harmonic, opus)

### VFX Breakdown

#### ChromaticBoltProj.cs
- **Flight**: Gentle spiral wobble, hue offset cycling per scale position
- **AI dust**: RainbowTorch colored by scale position
- **OnHitNPC**: Spawns AriaDetonationProj every hit

**PreDraw**:
- **Trail**: SoftGlow on oldPos with spectrum color shifting (hue cycles along trail). White core per point.
- **Core bloom**: 4-layer — RadialBloom chromatic halo → RadialBloom shifted-hue secondary → PointBloom white core → Star4Hard rainbow rotating star
- **Theme accents**: `ChromaticSwanUtils.DrawThemeAccents`

#### AriaDetonationProj.cs
3-ring expanding explosion. 3 modes with different radii and dust counts.
- **AI tick 1**: Massive dust burst — 12-36 RainbowTorch shards colored by scale position. Harmonic mode adds extra notes. Opus mode adds VFXLibrary feather/sparkle/music calls.
- **Expanding hitbox**: Collision radius grows over lifetime

**PreDraw** (most complex rendering of all 6 weapons):
- **Opus mode**: 7 stacked chromatic bloom rings, each at different hue offset, expanding over time
- **Normal mode**: 3 distinct ring layers:
  - Outer ring: 16 RadialBloom circles arranged in expanding ring
  - Mid ring: 12 SoftGlow circles, slightly smaller ring
  - Core: 8 PointBloom dots, tight ring
- **White-hot core**: SoftGlow at center, full brightness
- **Radiating Star4Hard**: Rotating star accent at peak expansion
- **Edge accent**: RainbowTorch-colored SoftGlow dots around expanding edge

### What's Missing

| Missing Element | Impact |
|---|---|
| **ChromaticTrail.fx shader** | Bolt trail should have flowing rainbow-shift energy via GPU shader |
| **AriaExplosion.fx shader** | Detonation should be a radial shader explosion, not manually placed bloom sprites in ring formations |
| **ChromaticPrimitiveRenderer** | Never instantiated |
| **ChromaticParticle system** (3 files) | Never used |
| **Scale-specific visual identity** | All 7 musical notes render identically — each scale degree should have subtly distinct visuals |
| **Opus vs normal bolt distinction** | The bolt itself looks the same regardless of opus charge — should glow/pulse more as you approach 7th cast |
| **Harmonic resonance visual** | Harmonic stacks have no visual presence until detonation |

### Quality Assessment: ⭐⭐⭐ 3/5
The AriaDetonationProj is the most visually complex rendering in the Swan Lake set — the Opus 7-ring chromatic stacked bloom is genuinely interesting. The spectrum color cycling on the bolt trail is thematically appropriate. But it's still all SpriteBatch bloom sprites — the AriaExplosion.fx shader would make the detonation dramatically better with proper radial UV distortion and animated color sweeps.

---

## WEAPON 4: FEATHER OF THE IRIDESCENT FLOCK

### Stats
| Property | Value |
|---|---|
| **Class** | Summon |
| **Damage** | 260 |
| **Use Time** | 30 |
| **Mana** | 20 |
| **Knockback** | 3 |
| **Style** | Summoner staff (V-formation crystal minions) |

### Attack Mechanics
- **V-formation**: Crystal swans fly in V-formation around player with bobbing animation
- **4-state cycle**: FormationFlight → ShardVolley (3-burst CrystalShardProj) → DiveAttack → Return
- **Scaling**: +5% damage per swan beyond first
- **Crystal Resonance**: 4+ swans = +3% crit

### VFX Breakdown

#### IridescentCrystalProj.cs (minion)
- **Formation lines**: PointBloom dots drawn between formation positions (connecting crystals)
- **Movement bloom trail**: SoftGlow iridescent dots on oldPos during movement
- **Core bloom**: 5-layer — RadialBloom oil-sheen outer → 6-dot iridescent orbit ring → SoftGlow core → PointBloom white center → Star4Soft sparkle
- **Dive attack**: Stronger white glow during dive state
- **Theme accents**: `FlockUtils.DrawThemeAccents`
- **Summoning dust**: 12 RainbowTorch dust ring on summon (from item file)

#### CrystalShardProj.cs (shard bullet)
Small homing shard (0.03 homing factor).
- **AI dust**: Iridescent shimmer (RainbowTorch)
- **Trail**: SoftGlow iridescent bloom on oldPos
- **Core**: 2-layer — SoftGlow outer iridescent + PointBloom white core

### What's Missing

| Missing Element | Impact |
|---|---|
| **CrystalOrbitTrail.fx shader** | Minion trail should have oil-sheen UV-shift effect, not solid colored bloom dots |
| **FlockAura.fx shader** | 3+ crystal formation should generate a visible aura connecting them — shader exists, never called |
| **FlockPrimitiveRenderer** | Never instantiated |
| **FlockParticle system** (3 files) | Never used |
| **Formation aura** | The most unique visual opportunity — V-formation crystals generating a visible energy field between them — completely absent |
| **Dive attack impact** | Dive attack has no special impact VFX despite being the strongest attack |
| **Crystal shard trail** | Shards are tiny and barely visible — need stronger trail |
| **Summoning circle** | Staff summon is just a dust ring — should have an animated summoning circle |

### Quality Assessment: ⭐⭐½ 2.5/5
The formation-line rendering (PointBloom dots between crystals) is a clever detail that communicates the V-formation. The 5-layer crystal bloom is adequate. But the FlockAura.fx shader — designed to create a visible energy field when 3+ crystals are active — would be the single biggest visual improvement. The formation aura is THIS weapon's unique identity and it's completely missing.

---

## WEAPON 5: IRIDESCENT WINGSPAN

### Stats
| Property | Value |
|---|---|
| **Class** | Magic |
| **Damage** | 420 |
| **Use Time** | 18 |
| **Mana** | 16 |
| **Knockback** | 6 |
| **Style** | Fan-pattern magic staff (5 wing bolts) |

### Attack Mechanics
- **5-bolt fan**: Spread angles -24°, -10°, 0°, +10°, +24°
- **Cursor convergence**: Bolts curve toward mouse position
- **Wing charge**: 8 charge per hit, 100 = empowered (3× damage, pen 5, noclip, 1.5× scale)
- **Prismatic Convergence**: 3+ bolts hitting same area = bonus burst

### VFX Breakdown

#### IridescentWingspan.cs (item)
**HoldItem** — **UNIQUE: Only weapon using custom particle system**:
- `WingBurstParticle`: Triggered on use
- `WingSparkParticle`: Ambient sparks during hold
- `PrismaticMoteParticle`: Ambient motes
- `EtherealFeatherParticle`: Feather particles during hold
- Uses `WingspanParticleHandler.SpawnParticle()`

#### WingspanBoltProj.cs
- **Flight**: Cursor convergence (velocity adjusts toward mouse pos each frame)
- **Empowered**: penetrate 5, noclip, 1.5× visual scale
- **AI dust**: Prismatic edge colors (cycling hue)

**PreDraw**:
- **Trail**: SoftGlow on oldPos with prismatic edge colors blended with ethereal white. Opacity fades along trail.
- **Core bloom**: 3-layer — SoftGlow prismatic outer → SoftGlow ethereal mid → PointBloom white core
- **Star accent**: Star4Soft with prismatic tint
- **Empowered wing accents**: Side bloom accents mimicking wing shapes (2 SoftGlow draws offset 45° either side of velocity)

### What's Missing

| Missing Element | Impact |
|---|---|
| **EtherealWing.fx shader** | Bolt should have ethereal wing-shaped energy via shader, not just offset bloom dots |
| **WingspanFlareTrail.fx shader** | Trail should be a flowing prismatic ribbon shader |
| **WingspanPrimitiveRenderer** | Never instantiated |
| **Empowered visual escalation** | Charge builds from 0→100 but has no progressive visual indicator on the weapon or player |
| **Prismatic Convergence VFX** | The multi-hit bonus mechanic has no dedicated visual event |
| **Fan-pattern visual cohesion** | 5 bolts fire independently — no visual element connecting them as a "wingspan" |
| **Wing-shape emphasis** | The weapon's identity is WINGS — the empowered "wing accents" are just 2 offset bloom dots, not wing-shaped |

### Quality Assessment: ⭐⭐½ 2.5/5
The HoldItem custom particles are the standout — this is the ONLY weapon actually using its custom particle system. The empowered wing-accent idea is good but the execution (2 offset SoftGlow sprites) doesn't read as "wings" at all. The EtherealWing.fx shader would presumably create proper wing-shaped energy overlays. The cursor convergence mechanic is cool but invisible to the player — needs visual telegraphing.

---

## WEAPON 6: THE SWAN'S LAMENT

### Stats
| Property | Value |
|---|---|
| **Class** | Ranged |
| **Damage** | 180 |
| **Use Time** | 35 |
| **Crit** | 10% |
| **Knockback** | 5 |
| **Style** | Shotgun with destruction halo |

### Attack Mechanics
- **Shotgun spread**: 10-16 bullet volley per shot
- **Every 6th shot**: Destruction Halo (expanding ring projectile) + 4 side bullets
- **Lamentation stacks**: Max 5, built on hit, enhance damage
- **Lament's Echo**: Fire rate boost on kill

### VFX Breakdown

#### TheSwansLament.cs (item)
- **No PostDrawInWorld VFX** — weapon has no world glow
- **No HoldItem VFX** — no visual feedback during hold
- **Muzzle flash**: Just dust (Smoke + WhiteTorch) + screenshake

#### LamentBulletProj.cs
**Unique element: Uses BeamStreak texture** instead of SoftGlow for trail backbone.
- **Trail**: BeamStreak texture drawn along oldPos (black→grey→white gradient). Additional SoftGlow bloom overlay per trail point.
- **Core**: SoftGlow grey outer + PointBloom white core
- **Empowered**: 4-dot gold accent ring around core
- **OnHitNPC**: 6 feather shrapnel dust burst (white/grey). Empowered = gold flash.

#### DestructionHaloProj.cs
Expanding ring: 20→180px over 2s, EaseOutQuart. Ring-shaped collision (edge only, 28px). Applies MournfulGaze debuff.
- **PreDraw**: 
  - Attempts HaloRing texture (with gold accent layer if available)
  - Fallback: 32-segment bloom dot ring (SoftGlow along circumference)
  - RadialBloom backdrop centered
  - 4 rotating PointBloom cardinal accents
  - 6-dot gold shimmer ring

### What's Missing

| Missing Element | Impact |
|---|---|
| **LamentBulletTrail.fx shader** | Grief-streak trail should have shader-driven dissolve/fade patterns |
| **DestructionRevelation.fx shader** | Expanding halo should be a radial shader — not 32 bloom dots in a circle |
| **LamentPrimitiveRenderer** | Never instantiated — CatmullRom-smoothed trail renderer, dead code |
| **LamentParticle system** (3 files) | Never used |
| **Weapon world glow** | Only Swan Lake weapon with NO PostDrawInWorld or HoldItem visual presence |
| **Lamentation stack visual** | Stacks have no visual representation on player or weapon |
| **Destruction halo shader** | The signature attack renders as manually-placed bloom dots — should be a beautiful expanding energy ring |
| **Shotgun muzzle flash** | Only dust particles, no bloom flash |
| **Bullet variety** | All 10-16 bullets look identical — could vary size/brightness/color slightly |

### Quality Assessment: ⭐⭐ 2/5
Weakest of the set. The BeamStreak texture usage is a positive distinction from the other weapons, and the expanding ring concept is unique. But the ring renders as 32 bloom dots in a circle (the fallback path!), the weapon itself has zero visual presence in the world, and the Lamentation stack mechanic has no visual feedback. The DestructionRevelation.fx shader was specifically designed for this weapon's signature attack and goes unused.

---

## CROSS-CUTTING ISSUES

### 1. The Shader Gap (CRITICAL)

**14 compiled HLSL shaders** exist for Swan Lake weapons (12 weapon + 2 SwanLakeShaderManager presets). Zero are used in rendering.

Shaders that exist with .fx source AND .fxc compiled bytecode:

| Shader | Purpose | Called? |
|---|---|---|
| DualPolaritySwing.fx | Black↔white swing arc trail | ❌ |
| SwanFlareTrail.fx | Flare projectile ribbon trail | ❌ |
| PearlescentRocketTrail.fx | Opal-shimmer rocket trail | ❌ |
| LakeExplosion.fx | Concentric water-ripple explosion | ❌ |
| ChromaticTrail.fx | Rainbow-shifting bolt trail | ❌ |
| AriaExplosion.fx | Chromatic aria detonation burst | ❌ |
| CrystalOrbitTrail.fx | Oil-sheen crystal minion trail | ❌ |
| FlockAura.fx | V-formation energy aura | ❌ |
| EtherealWing.fx | Wing-shaped ethereal energy | ❌ |
| WingspanFlareTrail.fx | Prismatic wing bolt trail | ❌ |
| LamentBulletTrail.fx | Grief-streak dissolving trail | ❌ |
| DestructionRevelation.fx | Expanding destruction halo | ❌ |

### 2. The Primitive Renderer Gap (CRITICAL)

6 complete GPU triangle-strip mesh builders exist (one per weapon). Each has:
- Custom vertex struct (Position + Color + TexCoord3)
- Dynamic vertex/index buffers (2048 verts / 6144 indices)
- Arc-length parameterized UV mapping
- Resampling (some with Catmull-Rom smoothing)
- Orthographic projection matrix + shader application
- Proper disposal pattern

**None are instantiated or called from any projectile.** They are completely dead code.

### 3. The Particle System Gap

6 complete custom particle systems (3 files each = 18 files total):
- BlackSwanParticle + BlackSwanParticleHandler + BlackSwanParticleTypes
- PearlescentParticle + PearlescentParticleHandler + PearlescentParticleTypes
- ChromaticParticle + ChromaticParticleHandler + ChromaticParticleTypes
- FlockParticle + FlockParticleHandler + FlockParticleTypes
- WingspanParticle + WingspanParticleHandler + WingspanParticleTypes
- LamentParticle + LamentParticleHandler + LamentParticleTypes

**Only WingspanParticleHandler is used** (from IridescentWingspan's HoldItem). The other 5 systems are dead code.

### 4. Foundation-Pattern Homogeneity

All 12 projectile PreDraw methods follow the exact same pattern:

```csharp
// 1. End default SpriteBatch
sb.End();
// 2. Begin additive
sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, ...);
// 3. Draw bloom textures from MagnumTextureRegistry on oldPos[]
foreach (oldPosition) {
    sb.Draw(SoftGlow/RadialBloom/PointBloom, screenPos, color * opacity, scale);
}
// 4. Restore SpriteBatch
sb.End();
sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

This makes all 6 weapons feel visually similar despite their radically different mechanical identities. The texture choice (SoftGlow vs RadialBloom vs PointBloom) and color palette vary, but the rendering technique is identical across all weapons.

### 5. Missing Screen Effects
No weapon uses:
- Screen shake on impact (except muzzle shake on Lament)
- Screen flash on critical moments
- Screen distortion (ScreenDistortion.fx exists in the mod)
- Chromatic aberration

### 6. Missing Visual Feedback for Mechanics
Every weapon has a stack/charge/special mechanic. None have visual representation:

| Weapon | Mechanic | Visual Feedback |
|---|---|---|
| Black Swan | Swan's Grace stacks (5) | Some orbiting dust particles |
| Black Swan | Dark Mirror stacks | Shadowflame orbiting dust |
| Pearlescent Lake | Still Waters zone | None until zone spawns |
| Chromatic Swan Song | Scale position (7-step) | None (bolt color varies slightly) |
| Chromatic Swan Song | Harmonic stacks (5) | None |
| Iridescent Flock | Crystal Resonance (4+ swans) | None |
| Wingspan | Wing charge (0→100) | None |
| Swan's Lament | Lamentation stacks (5) | None |
| Swan's Lament | Lament's Echo | None |

---

## PRIORITY RECOMMENDATIONS

### Tier 1: Wire Existing Shaders to Rendering (Highest Impact, Lowest Effort)
The shaders are written, compiled, and registered. The primitive renderers are built. The connection is missing.

1. **BlackSwanSwingProj**: Instantiate `BlackSwanPrimitiveRenderer`, call `RenderTrail()` with `DualPolaritySwing` shader. Replace the bloom-dot swing trail.
2. **DestructionHaloProj**: Replace the 32-dot ring fallback with `DestructionRevelation` shader radial render.
3. **AriaDetonationProj**: Replace manually-placed ring bloom sprites with `AriaExplosion` shader radial render.
4. **PearlescentRocketProj**: Instantiate `PearlescentPrimitiveRenderer`, call `RenderTrail()` with `PearlescentRocketTrail` shader.
5. **ChromaticBoltProj**: Instantiate `ChromaticPrimitiveRenderer` + `ChromaticTrail` shader.
6. **IridescentCrystalProj**: Wire `FlockAura` shader when 3+ crystals active. Wire `CrystalOrbitTrail` for minion trail.
7. **WingspanBoltProj**: Instantiate `WingspanPrimitiveRenderer` + `EtherealWing` / `WingspanFlareTrail` shaders.
8. **LamentBulletProj**: Instantiate `LamentPrimitiveRenderer` + `LamentBulletTrail` shader.

### Tier 2: Activate Particle Systems
Wire the 5 unused particle systems into their weapons' VFX code. These should supplement (not replace) the bloom rendering as accent layers.

### Tier 3: Mechanical Visual Feedback
Add progressive visual indicators for charge/stack mechanics — bloom intensity scaling, orbit particle count, color temperature shifts, pulsing frequency changes.

### Tier 4: Screen Effects for Signature Moments
- Screen shake on Black Swan Phase 2 finisher
- Screen flash on Opus Detonation
- Screen distortion on Destruction Halo center
- Chromatic aberration on empowered Wingspan volley

---

## FILE INVENTORY

### Per-Weapon File Counts

| Weapon | Projectiles | Shaders (.fx/.fxc) | Primitives | Particles | Utilities | Total Files |
|---|---|---|---|---|---|---|
| Call of the Black Swan | 2 | 2+2 | 2 (.cs + vertex) | 3 | 2 | 13 |
| Call of the Pearlescent Lake | 2 | 2+2 | 1 (.cs + vertex) | 3 | 1 | 13 |
| Chromatic Swan Song | 2 | 2+2 | 1 (.cs + vertex) | 3 | 2 | 14 |
| Feather of the Iridescent Flock | 2 | 2+2 | 1 (.cs + vertex) | 3 | 1 | 13 |
| Iridescent Wingspan | 1 | 2+2 | 1 (.cs + vertex) | 3 | 2 | 13 |
| The Swan's Lament | 2 | 2+2 | 1 (.cs + vertex) | 3 | 2 | 14 |

### Infrastructure Usage Matrix

| System | Black Swan | Pearlescent | Chromatic | Flock | Wingspan | Lament |
|---|---|---|---|---|---|---|
| SwanLakePalette | ✅ (PreDraw) | ⚠️ (partial) | ⚠️ (partial) | ⚠️ (partial) | ⚠️ (partial) | ⚠️ (partial) |
| SwanLakeShaderManager | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| SwanLakeVFXLibrary | ✅ (OnHit) | ⚠️ (theme only) | ✅ (OnHit) | ⚠️ (theme only) | ⚠️ (theme only) | ⚠️ (theme only) |
| SwanLakeThemeTextures | ⚠️ (via utils) | ⚠️ (via utils) | ⚠️ (via utils) | ⚠️ (via utils) | ⚠️ (via utils) | ❌ |
| Per-weapon shader | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Per-weapon primitives | ❌ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Per-weapon particles | ❌ | ❌ | ❌ | ❌ | ✅ (HoldItem) | ❌ |
| MagnumTextureRegistry | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ |
