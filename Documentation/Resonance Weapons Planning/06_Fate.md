# ⭐ Fate — Resonance Weapons Planning

> *"The celestial symphony of destiny — black void, dark pink, bright crimson, celestial white."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Beethoven's Symphony No. 5 (Fate) — the celestial symphony of destiny |
| **Emotional Core** | Cosmic inevitability, endgame awe |
| **Color Palette** | Black void, dark pink, bright crimson, celestial white |
| **Core Hex** | CosmicVoid `(15, 5, 20)` → DarkPink `(180, 50, 100)` → BrightCrimson `(255, 60, 80)` → StarGold `(255, 230, 180)` → WhiteCelestial `(255, 255, 255)` → SupernovaWhite `(255, 255, 250)` |
| **Extended Colors** | FatePurple `(120, 30, 140)`, NebulaPurple `(160, 80, 200)`, FateCyan `(100, 200, 255)`, ConstellationSilver `(200, 210, 240)`, DestinyFlame `(255, 120, 60)`, CosmicDawn `(220, 100, 80)`, CosmicRose `(220, 80, 130)` |
| **Lore Color** | `new Color(180, 40, 80)` — Cosmic Crimson |
| **Lore Keywords** | Destiny, cosmos, stars, constellations, fate, symphony, finale, celestial, supernova, annihilation |
| **VFX Language** | Ancient glyphs, star particles, cosmic cloud energy, screen distortions, chromatic aberration, constellation patterns, gravitational vortex, reality tears |

### Shared Infrastructure (Already Exists)
| System | Lines | Purpose |
|--------|-------|---------|
| `FatePalette.cs` | 473 | 6 core + 9 extended colors, 10 per-weapon palette arrays, gradient/shimmer helpers |
| `FateVFXLibrary.cs` | 832 | Bloom stacking, dust helpers, cosmic clouds, music notes, trail functions (NOT referenced by weapons) |
| `FateShaderManager.cs` | 350 | 10 trail presets, noise textures (NOT referenced by weapons) |
| `FateCosmicVFX.cs` | 725 | Older cosmic VFX with EnhancedParticles (NOT referenced by weapons) |
| `FateAstrographSystem.cs` | 599 | 10 constellation patterns (Orion, Scorpius, Leo, etc.) |
| `DestinyCollapse` debuff | 667 | 8-stack gravitational collapse, Singularity→Supernova→Cosmic Revisit (ALL 10 weapons) |
| `SpectralResonance` debuff | 124 | 3-stack burst (Requiem only) |
| `FateWeaponDebuffs` | — | RealityFrayed (Requiem), AnnihilationMark (Coda), BygoneEcho (Bygone Reality) |

### Architecture Note
All 10 Fate weapons are **fully self-contained** (~12-14 .cs files + 4 .fx shaders each). Each weapon duplicates its own palette, easing, SpriteBatch helpers, particle system, and trail renderer internally. The ONLY shared runtime dependency is `Content/Fate/Debuffs/`. Foundations provide the **mesh construction, blend management, and rendering scaffolding patterns** that each weapon's self-contained systems build upon.

---

## Foundation Weapons Integration Map

Fate is the endgame cosmic theme — every weapon should feel like wielding the power of the cosmos itself. Foundations provide technical scaffolding for rendering at maximum visual quality.

| Foundation | Used By | Purpose |
|-----------|---------|---------|
| **SwordSmearFoundation** | Requiem, Opus, Fractal, Conductor, Coda | Swing arc smear — SmearDistortShader with cosmic pink/crimson/gold LUT |
| **RibbonFoundation** | All 10 weapons | Trail strips — Mode 7 (Cosmic) for melee, Mode 6 (Energy Surge) for beams/projectiles, Mode 3 (Basic) for bullets |
| **ImpactFoundation** | All 10 weapons | Hit VFX — RippleShader for destiny rings, DamageZoneShader for reality tears, SlashMarkShader for impact marks |
| **ExplosionParticlesFoundation** | All 10 weapons | Cosmic detonations — supernova bursts, star fractures, blade shatters |
| **SparkleProjectileFoundation** | Most weapons | SparkleTrailShader for cosmic notes, seekers, crystal shards, homing star particles |
| **LaserFoundation** | Conductor (beam salvos), Fermata (slash waves) | ConvergenceBeamShader for homing cosmic beams |
| **ThinLaserFoundation** | Conductor (lightning), Fractal (prismatic beam) | ThinBeamShader for chain lightning and beam strikes |
| **InfernalBeamFoundation** | Final Fermata (temporal wave) | InfernalBeamBodyShader for sustained cosmic beams |
| **MaskFoundation** | Crescendo (deity aura), Requiem (combo aura), Fermata (orbit ring) | RadialNoiseMaskShader for cosmic auras and formation rings |
| **MagicOrbFoundation** | Symphony's End (spiral blades), Opus (energy balls) | OrbBolt rendering for cosmic projectile bodies |
| **ThinSlashFoundation** | Requiem (spectral blade slash), Fractal (star fracture) | ThinSlashShader SDF for dimensional cut marks |
| **XSlashFoundation** | Coda (Annihilation detonation) | XSlashShader for cross-tear ultimate VFX |
| **SmokeFoundation** | Requiem (reality tear), Light of Future (smoke wisps) | Nebula smoke wisps from cosmic impacts |
| **AttackAnimationFoundation** | Coda (Finale), Fractal (Star Fracture), Conductor (Convergence) | Cinematic sequences with screen effects |
| **AttackFoundation** | All projectile-spawning weapons | Base projectile pattern for cosmic notes, beams, seekers |

---

## Weapons Overview

| # | Weapon | Class | Damage | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Requiem of Reality | Melee | 740 | 4-movement combo + Spectral Blade + Reality Tears |
| 2 | The Final Fermata | Magic | 520 | 6 orbiting spectral swords + synchronized slash + Fermata Power (5×) |
| 3 | Destiny's Crescendo | Summon | 400 | Deity minion + 4-phase Musical Escalation (Pianissimo→Fortissimo) |
| 4 | Symphony's End | Magic | 500 | Rapid-fire spiral blades + Crescendo Mode + Final Note ultimate |
| 5 | Resonance of a Bygone Reality | Ranged | 400 | Rapid-fire + spectral blade every 5th hit + Bygone Resonance dual-hit |
| 6 | Light of the Future | Ranged | 680 | Accelerating railgun (6→42 speed) + cosmic rockets + Cascade kills |
| 7 | Coda of Annihilation | Melee | 1350 | **Zenith-equivalent** — throws 14 theme melee weapon copies |
| 8 | Opus Ultima | Melee | 720 | 3-movement combo + energy balls→seekers + Opus Resonance stacks |
| 9 | Fractal of the Stars | Melee | 850 | 3-phase geometric combo + orbiting star blades + Star Fracture recursion |
| 10 | The Conductor's Last Constellation | Melee | 780 | 3-phase orchestral combo + homing beams + lightning + Convergence |

---

## 1. Requiem of Reality (Melee — 4-Movement Combo)

### Identity & Musical Soul
The requiem — a mass for the dead. A blade that plays four movements: Adagio (slow, mournful), Allegro (quick, desperate), Scherzo (wild, spinning), Finale (the final, devastating stroke). Reality tears open in its wake.

### Lore Line
*"The cosmos does not mourn. It simply ends, and begins again."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  REQUIEM OF REALITY — Foundation Architecture       │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → 4-movement escalating cosmic smear          │  │
│  │  → CosmicVoid→FatePurple→BrightCrimson LUT    │  │
│  │  → distortStrength: 0.05→0.07→0.09→0.14       │  │
│  │  → 5-layer rendering pipeline                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → 24-point trail arc, cosmic pink/crimson     │  │
│  │  → RequiemSwingTrail shader + noise texture    │  │
│  │  → RequiemSwingGlow shader (wide glow layer)   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (cosmic notes)    │  │
│  │  → RequiemCosmicNote homing music notes        │  │
│  │  → 3-5 per swing, 40% dmg, 2-phase AI         │  │
│  │  → SparkleTrailShader: pink-red hue band       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackFoundation (Spectral Blade)             │  │
│  │  → RequiemSpectralBlade: 6-phase autonomous AI │  │
│  │  → Rise→Orbit→Detonate→Seek→Slash→Return      │  │
│  │  → Spawns on 4th swing (Finale), 2× dmg       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → RippleShader: destiny expansion rings       │  │
│  │  → DamageZone: RequiemRealityTear (lingering)  │  │
│  │  → RequiemImpactBloom shader (directional)     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → RequiemComboAura at combo ≥ 3              │  │
│  │  → Cosmic nebula noise, pulsing crimson/gold   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (reality tears)               │  │
│  │  → Nebula smoke from RequiemRealityTear        │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (5 keys, 4 files):                  │
│  → RequiemSwingTrail.fx (SwingMain + SwingGlow)      │
│  → RequiemNoteTrail.fx (NoteTrailMain)               │
│  → RequiemComboAura.fx (ComboAuraMain)               │
│  → RequiemImpactBloom.fx (ImpactBloomMain)           │
│  DEBUFFS: SpectralResonance (3-stack burst 2.5×)    │
│  + RealityFrayed (15 DPS) + DestinyCollapse          │
│  5 PARTICLE TYPES: BloomFlare, Spark, Glyph, Mote, Note │
└─────────────────────────────────────────────────────┘
```

### 4-Movement Musical Combo
| Movement | Name | Arc | Duration | DmgMult | Easing | Foundation |
|----------|------|-----|----------|---------|--------|-----------|
| 0 | Adagio | 160° | 30f | 1.0× | SineInOut | SwordSmear + Ribbon |
| 1 | Allegro | 120° | 22f | 0.9× | QuadOut | SwordSmear + Ribbon + Sparkle |
| 2 | Scherzo | 270° | 18f | 0.8× | ExpOut | SwordSmear + Ribbon |
| 3 | Finale | 100° | 26f | 1.5× | BackOut overshoot | ALL foundations + Attack |

---

## 2. The Final Fermata (Magic — Orbiting Swords)

### Identity & Musical Soul
A fermata — the held note at the end. Spectral swords orbit the caster, building in number and power the longer held. The ultimate expression of sustained magical pressure.

### Lore Line
*"Hold the note. Hold it until the stars remember what silence is."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE FINAL FERMATA — Foundation Architecture        │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SparkleProjectileFoundation (spectral swords) │  │
│  │  → FermataSpectralSwordNew orbiting swords     │  │
│  │  → SparkleTrailShader: crimson cosmic shimmer  │  │
│  │  → 3→6 swords, equilateral→hexagonal formation│  │
│  │  → FermataSwordTrail shader (orbit trail)      │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → FermataOrbitRing: formation ring visual     │  │
│  │  → Cosmic noise, 60px orbit radius             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader)         │  │
│  │  → FermataSlashWave: synchronized slash marks  │  │
│  │  → Every 90 frames: ALL swords slash at once   │  │
│  │  → FermataSyncSlash shader (arc trail)         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: synchronized slash impact     │  │
│  │  → FermataTemporalWave shader (temporal wave)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → SlashWave trail rendering                   │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (5 keys, 4 files):                  │
│  → FermataSwordTrail.fx (Sword + Glow techniques)   │
│  → FermataOrbitRing.fx (formation ring)              │
│  → FermataTemporalWave.fx (temporal wave)            │
│  → FermataSyncSlash.fx (synchronized slash arc)      │
│  UNIQUE SYSTEMS:                                    │
│  → Fermata Power: +10%/s sustained hold, max 5×     │
│  → Harmonic Alignment: 3+ swords focus same target  │
│  → 10s sustained → autonomous Sustained Note minion │
│  → Max 6 swords = 1.5× sync slash damage            │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 3. Destiny's Crescendo (Summon — Deity Minion)

### Identity & Musical Soul
A crescendo — building from silence to overwhelming power. The deity minion grows in size, strength, and visual intensity over 45 seconds. Taking massive damage resets the crescendo.

### Lore Line
*"The symphony of fate plays softly at first. By the finale, it shakes the heavens."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  DESTINY'S CRESCENDO — Foundation Architecture      │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MaskFoundation (RadialNoiseMaskShader)        │  │
│  │  → CrescendoAuraGlow: deity ambient aura       │  │
│  │  → Scales with escalation phase (1.0→1.5×)    │  │
│  │  → Cosmic noise, crimson→gold gradient         │  │
│  │  → CrescendoSummonBloom shader (summon flash)  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (cosmic beams)    │  │
│  │  → CrescendoCosmicBeam homing beam projectiles │  │
│  │  → SparkleTrailShader: fire→gold gradient      │  │
│  │  → 4PointedStarHard texture                    │  │
│  │  → Phase-scales: 1→2→3→5 beams per volley     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: deity slash impact rings      │  │
│  │  → CrescendoSlashArc shader (melee arc)        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 1: Pure Bloom)         │  │
│  │  → Deity movement trail (cosmic glow)          │  │
│  │  → CrescendoBeamTrail shader (beam trails)     │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → CrescendoBeamTrail.fx, CrescendoAuraGlow.fx     │
│  → CrescendoSummonBloom.fx, CrescendoSlashArc.fx   │
│  MUSICAL ESCALATION (4 phases):                     │
│  → Pianissimo (0s): 1 beam, 120f CD, 1.0× scale    │
│  → Piano (15s): 2 beams, 100f CD, 1.15× scale      │
│  → Forte (30s): 3 beams, 80f CD, 1.3× + buffs      │
│  → Fortissimo (45s): 5 beams, 60f CD, 1.5× + all   │
│  → Heavy hit (>200 dmg) → RESET to Pianissimo       │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 4. Symphony's End (Magic — Rapid Spiral Blades)

### Identity & Musical Soul
The symphony ends — not with a quiet note but with a cascade of spiraling blades. Rapid-fire corkscrewing projectiles that shatter into fragments on impact, building to a Final Note ultimate.

### Lore Line
*"Every symphony must end. This one ends the world."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  SYMPHONY'S END — Foundation Architecture           │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ MagicOrbFoundation (spiral blades)            │  │
│  │  → SymphonySpiralBlade helix-steering body     │  │
│  │  → OrbBolt rendering: corkscrewing flight      │  │
│  │  → SymphonySpiralTrail shader (helix trail)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (blade shatter)  │  │
│  │  → 4 SymphonyBladeFragment per shatter         │  │
│  │  → SymphonyShatterBloom shader (bloom)         │  │
│  │  → Gravity-affected scatter fragments          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (fragments)       │  │
│  │  → SymphonyBladeFragment shimmer trail         │  │
│  │  → SymphonyFragmentTrail shader                │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → Spiral blade energy trail                   │  │
│  │  → SymphonyCrackle shader (crackle overlay)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: blade shatter rings           │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → SymphonySpiralTrail.fx, SymphonyShatterBloom.fx  │
│  → SymphonyFragmentTrail.fx, SymphonyCrackle.fx     │
│  UNIQUE SYSTEMS:                                    │
│  → Helix steering: perpendicular oscillation sin(θ) │
│  → Crescendo Mode (3s fire): +50% rate, +size       │
│  → Diminuendo (stop after Crescendo): +20% dmg 2s   │
│  → Final Note (10s fire): 5× size, full pierce      │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 5. Resonance of a Bygone Reality (Ranged — Cosmic Gun)

### Identity & Musical Soul
A resonance from a reality that once was — rapid-fire cosmic bullets that summon ghostly spectral blades from the past. When blade and bullet strike the same enemy, they trigger a Bygone Resonance — reality remembering what it was.

### Lore Line
*"The past does not stay buried. It echoes through every bullet."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  RESONANCE OF A BYGONE REALITY — Foundation Arch.   │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 3: Basic Trail Strip)  │  │
│  │  → ResonanceRapidBullet tracer trail           │  │
│  │  → ResonanceBulletTrail shader (cosmic streak) │  │
│  │  → 8×8, pen=1, extraUpdates=2 (fast travel)   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackFoundation (Spectral Blade)             │  │
│  │  → ResonanceSpectralBlade: 3-phase AI          │  │
│  │  → Approach→Slash(35f)→Explode                 │  │
│  │  → Every 5th hit spawns blade, 2× dmg          │  │
│  │  → ResonanceBladeTrail shader (slash trail)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → ResonanceEchoBloom shader (Bygone Resonance)│  │
│  │  → RippleShader: dual-hit explosion ring       │  │
│  │  → 12-particle resonance ring burst            │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Resonance burst)│  │
│  │  → Bygone Resonance dual-hit explosion sparks  │  │
│  │  → ResonanceMuzzleFlash shader (muzzle VFX)    │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → ResonanceBladeTrail.fx, ResonanceMuzzleFlash.fx  │
│  → ResonanceEchoBloom.fx, ResonanceBulletTrail.fx   │
│  UNIQUE SYSTEMS:                                    │
│  → 40% ammo conservation                            │
│  → Spectral Blade every 5th hit (2× dmg)           │
│  → Bygone Resonance: blade+bullet same enemy <0.5s │
│  → BygoneEcho debuff marks targets                  │
│  → Reality Fade: every 10th combined hit → 0.3s inv │
│  DEBUFFS: DestinyCollapse + BygoneEcho              │
└─────────────────────────────────────────────────────┘
```

---

## 6. Light of the Future (Ranged — Cosmic Railgun)

### Identity & Musical Soul
The light that hasn't arrived yet — a railgun whose bullets accelerate from a crawl to blinding speed, growing more powerful and visually intense the faster they travel. The future is bright, and it hits very hard.

### Lore Line
*"The fastest light is the one that hasn't arrived yet."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  LIGHT OF THE FUTURE — Foundation Architecture      │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ RibbonFoundation (Mode 6: Energy Surge)       │  │
│  │  → LightAcceleratingBullet speed-reactive trail│  │
│  │  → LightBulletTrail shader (speed→color grad)  │  │
│  │  → Void→violet→cyan→plasma white gradient      │  │
│  │  → VFX intensity scales with speed ratio       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (cosmic rockets)  │  │
│  │  → LightCosmicRocket spiral homing rockets     │  │
│  │  → SparkleTrailShader: spiral offset trail     │  │
│  │  → Every 3rd shot: 3 rockets ±15° spread       │  │
│  │  → 600f range, 0.08f turn, spiral flight       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (impact + cascade)│  │
│  │  → LightImpactBloom shader (impact explosion)  │  │
│  │  → Cascade: peak-speed kills → 2 new bullets   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: rocket AoE explosion ring     │  │
│  │  → LightMuzzleFlash shader (railgun muzzle)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SmokeFoundation (high-speed wisps)            │  │
│  │  → Smoke wisps at >60% speed ratio             │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → LightRocketTrail.fx, LightMuzzleFlash.fx        │
│  → LightImpactBloom.fx, LightBulletTrail.fx        │
│  UNIQUE SYSTEMS:                                    │
│  → Acceleration: 6f→42f at +1.2f/frame             │
│  → Speed-based damage: up to +100% at max speed    │
│  → VFX layers unlock at speed thresholds:           │
│  │  → >30%: speed line tracers                     │
│  │  → >50%: star sparks                            │
│  │  → >60%: smoke wisps                            │
│  → Cascade: peak-speed kill → 2 new full-speed      │
│  → 50% ammo conservation                            │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 7. Coda of Annihilation (Melee — **The Zenith**, 1350 dmg)

### Identity & Musical Soul
The coda — the final passage. **MagnumOpus's Zenith equivalent.** Throws spectral copies of ALL 14 theme melee weapons from across the entire mod, cycling through every theme's identity. The culmination of everything.

### Lore Line
*"Every blade. Every theme. Every note. The final coda plays them all."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  CODA OF ANNIHILATION — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → CodaHeldSwing orbital arc                   │  │
│  │  → CodaSwingArc shader (swing arc trail)       │  │
│  │  → 65f orbit radius, ±144° arc, 0.12 rad/frame│  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (Zenith swords)   │  │
│  │  → CodaZenithSword flying homing swords        │  │
│  │  → 14 weapon textures cycling per swing         │  │
│  │  → 600f homing range, 0.25f max turn speed     │  │
│  │  → CodaZenithTrail shader (Zenith + Glow)      │  │
│  │  → 2-3 per swing, 60×60 hitbox, infinite pierce│  │
│  ├───────────────────────────────────────────────┤  │
│  │ XSlashFoundation (Annihilation detonation)    │  │
│  │  → XSlashShader: Coda Finale cross-tear VFX    │  │
│  │  → Triggered after 10s continuous use           │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple + DamageZone)        │  │
│  │  → CodaImpactBurst shader (impact explosion)   │  │
│  │  → CodaAnnihilationBloom shader (finale flash) │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (AnnihilationMark)│  │
│  │  → 10-stack AnnihilationMark detonation burst  │  │
│  │  → 50% accumulated damage as explosion          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Coda Finale)       │  │
│  │  → 10s continuous use → Coda Finale cinematic  │  │
│  │  → 5s cooldown between Finales                  │  │
│  │  → Enhanced VFX, intensified spawn rate         │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → 12-point trail on each Zenith sword          │  │
│  │  → Color tinted per weapon index (14 themes)   │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (5 keys, 4 files):                  │
│  → CodaZenithTrail.fx (ZenithMain + ZenithGlow)     │
│  → CodaSwingArc.fx (SwingArcMain)                   │
│  → CodaImpactBurst.fx (ImpactBurstMain)             │
│  → CodaAnnihilationBloom.fx (AnnihilationBloomMain)  │
│  14-WEAPON CYCLE:                                   │
│  → 0-1: Moonlight Sonata (purple, light blue)       │
│  → 2-3: Eroica (scarlet, gold)                      │
│  → 4-5: La Campanella (orange, golden orange)        │
│  → 6-7: Enigma (purple, green)                      │
│  → 8: Swan Lake (white)                              │
│  → 9-13: Fate (dark pink, crimson, purple, rose, rose)│
│  DEBUFFS: AnnihilationMark (10 stack→50% detonation)│
│  + DestinyCollapse                                   │
└─────────────────────────────────────────────────────┘
```

---

## 8. Opus Ultima (Melee — Energy Ball Combo)

### Identity & Musical Soul
The ultimate opus — a blade whose swings release energy balls that shatter into homing seekers. Each completed 3-movement cycle builds Opus Resonance stacks, strengthening all damage.

### Lore Line
*"The final work. The magnum opus. Written in starfire."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  OPUS ULTIMA — Foundation Architecture              │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → 3-movement cosmic swing smear               │  │
│  │  → OpusSwingTrail shader + noise texture       │  │
│  │  → 5-layer rendering pipeline                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ MagicOrbFoundation (energy balls)             │  │
│  │  → OpusEnergyBallProjectile, 3 modes           │  │
│  │  → Mode 0: forward ball → explodes to 5 seekers│  │
│  │  → Mode 1: homing seeker                       │  │
│  │  → Mode 2: crystal shard (40% dmg, on-hit)    │  │
│  │  → OpusEnergyBall shader (body glow)           │  │
│  │  → OpusSeekerTrail shader (seeker trail)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (ball explosion)  │  │
│  │  → Energy ball → 5 seekers burst               │  │
│  │  → OpusExplosion shader (explosion bloom)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: energy ball detonation ring   │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → 24-point swing trail                       │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → OpusSwingTrail.fx, OpusSeekerTrail.fx            │
│  → OpusExplosion.fx, OpusEnergyBall.fx              │
│  3-MOVEMENT COMBO:                                  │
│  → Exposition (150°, 28f, 1.0×): 1 energy ball     │
│  → Development (120°, 20f, 0.9×): 2 twin balls     │
│  → Recapitulation (180°, 32f, 1.2×): 1 massive ball│
│  OPUS RESONANCE: +5% all dmg per cycle (max 9, +45%)│
│  On melee hit: 3-5 crystal shards (Mode 2, 40%)     │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 9. Fractal of the Stars (Melee — Geometric Star Fracture)

### Identity & Musical Soul
Fractal geometry made blade — each hit spawns orbiting star blades, and the third strike triggers a Star Fracture that recursively explodes into smaller fractures. The mathematical beauty of the cosmos expressed as devastation.

### Lore Line
*"The stars do not scatter randomly. They fracture in self-similar patterns, infinitely deep."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  FRACTAL OF THE STARS — Foundation Architecture     │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → 3-phase geometric swing smear               │  │
│  │  → FractalSwingTrail shader + constellation    │  │
│  │  → 5-layer rendering pipeline                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ SparkleProjectileFoundation (orbit blades)    │  │
│  │  → FractalOrbitBlade: orbiting spectral stars  │  │
│  │  → Max 6 active, 120px orbit, 0.04 rad/frame  │  │
│  │  → Fires prismatic beam every 60 frames         │  │
│  │  → FractalOrbitGlow shader (ambient glow)      │  │
│  │  → FractalConstellationTrail shader (trail)    │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinSlashFoundation (ThinSlashShader)         │  │
│  │  → Star Fracture geometric cut marks           │  │
│  │  → Phase 2 Gravity Slam trigger                │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ExplosionParticlesFoundation (Star Fracture)  │  │
│  │  → FractalStarFracture shader (geometric VFX)  │  │
│  │  → RECURSIVE: sub-fractures at 1/3 size + 1/3 dmg│
│  │  → Cascading geometric explosion pattern        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Gravity Slam)      │  │
│  │  → Phase 2: ExpIn easing (slow→fast slam)     │  │
│  │  → Screen shake on Star Fracture trigger       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → RippleShader: fracture detonation rings     │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → 24-point swing trail + orbit blade trails   │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (4):                                │
│  → FractalSwingTrail.fx, FractalStarFracture.fx     │
│  → FractalOrbitGlow.fx, FractalConstellationTrail.fx│
│  3-PHASE COMBO:                                     │
│  → Horizontal Sweep (170°, 22f, 1.0×): sparks      │
│  → Rising Uppercut (130°, 16f, 0.95×): stars rise   │
│  → Gravity Slam (120°, 24f, 1.4×): STAR FRACTURE   │
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## 10. The Conductor's Last Constellation (Melee — Orchestral Baton)

### Identity & Musical Soul
The conductor's baton — each swing directs an orchestra of cosmic beams and lightning. The 3-phase combo builds like an orchestral performance: a decisive Downbeat, a building Crescendo, then a thunderous Forte with convergence.

### Lore Line
*"The last constellation is the one the conductor draws with their final baton stroke."*

### Foundation Weapons Stack
```
┌─────────────────────────────────────────────────────┐
│  THE CONDUCTOR'S LAST CONSTELLATION — Foundation    │
├─────────────────────────────────────────────────────┤
│  ┌───────────────────────────────────────────────┐  │
│  │ SwordSmearFoundation (SmearDistortShader)     │  │
│  │  → 3-phase orchestral swing smear              │  │
│  │  → ConductorSwingTrail shader (+Glow technique)│  │
│  │  → Electric/lightning spark accents             │  │
│  │  → 5-layer rendering pipeline                  │  │
│  ├───────────────────────────────────────────────┤  │
│  │ LaserFoundation (beam salvos)                 │  │
│  │  → 3 ConductorSwordBeam per swing, 18° spread │  │
│  │  → Aggressive homing: 700f range, 0.12f turn   │  │
│  │  → ConductorBeamShader (beam trail)            │  │
│  │  → Phase 3: crystal shard mode (25% dmg)       │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ThinLaserFoundation (chain lightning)         │  │
│  │  → ConductorLightningShader (lightning bolt)   │  │
│  │  → 3 cosmic lightning strikes per hit          │  │
│  ├───────────────────────────────────────────────┤  │
│  │ ImpactFoundation (Ripple)                     │  │
│  │  → ConductorConvergence shader (convergence)   │  │
│  │  → RippleShader: beam impact rings             │  │
│  ├───────────────────────────────────────────────┤  │
│  │ AttackAnimationFoundation (Convergence)       │  │
│  │  → Phase 2 (Forte) 3rd combo hit               │  │
│  │  → All active beams converge on cursor          │  │
│  │  → Cosmic lightning storm accompaniment        │  │
│  ├───────────────────────────────────────────────┤  │
│  │ RibbonFoundation (Mode 7: Cosmic Flow)        │  │
│  │  → 24-point swing trail + beam trails          │  │
│  └───────────────────────────────────────────────┘  │
│  CUSTOM SHADERS (5 keys, 4 files):                  │
│  → ConductorSwingTrail.fx (Trail + Glow)             │
│  → ConductorLightningShader.fx (bolt VFX)            │
│  → ConductorConvergence.fx (convergence beam)        │
│  → ConductorBeamShader.fx (sword beam trail)         │
│  3-PHASE ORCHESTRAL COMBO:                          │
│  → Downbeat (150°, 22f, 1.0×, QuadIn): 3 beams down│
│  → Crescendo (130°, 18f, 0.95×, SineInOut): wider   │
│  → Forte (180°, 26f, 1.35×, ExpIn→explosion):       │
│    → Lightning cascade + beam Convergence on cursor  │
│    → Constellation shatters → 8-12 homing stars      │
│  On hit: DestinyCollapse(5s) + 3 lightning + 5 shards│
│  DEBUFFS: DestinyCollapse                           │
└─────────────────────────────────────────────────────┘
```

---

## Foundation Coverage Matrix

| Foundation | Requiem | Fermata | Cresc. | Symph. | Bygone | Light | Coda | Opus | Fractal | Conductor |
|-----------|---------|---------|--------|--------|--------|-------|------|------|---------|-----------|
| SwordSmear | ✅ | | | | | | ✅ | ✅ | ✅ | ✅ |
| Ribbon | ✅ M7 | ✅ M6 | ✅ M1 | ✅ M6 | ✅ M3 | ✅ M6 | ✅ M7 | ✅ M7 | ✅ M7 | ✅ M7 |
| Impact | ✅ Rip+DZ | ✅ Rip | ✅ Rip | ✅ Rip | ✅ Rip | ✅ Rip | ✅ Rip+DZ | ✅ Rip | ✅ Rip | ✅ Rip |
| Explosion | | | | ✅ Shatter | ✅ Resonance | ✅ Cascade | ✅ Annihil | ✅ Ball→5 | ✅ StarFrac | |
| SparklePrj | ✅ Notes | ✅ Swords | ✅ Beams | ✅ Frags | | ✅ Rockets | ✅ Zenith | | ✅ Orbits | |
| Laser | | | | | | | | | | ✅ Beams |
| ThinLaser | | | | | | | | | ✅ Prismatic | ✅ Lightning |
| InfernalBeam | | ✅ Temporal | | | | | | | | |
| Mask | ✅ Aura | ✅ Ring | ✅ Aura | | | | | | | |
| MagicOrb | | | | ✅ Spiral | | | | ✅ EBall | | |
| ThinSlash | | ✅ Slash | | | | | | | ✅ Fracture | |
| XSlash | | | | | | | ✅ Finale | | | |
| Smoke | ✅ Rift | | | | | ✅ >60% | | | | |
| AttackAnim | | | | | | | ✅ Finale | | ✅ Slam | ✅ Converge |
| Attack | ✅ SpecBlade | | | | ✅ SpecBlade | | | | | |

### Fate Lore Consistency
- All lore references destiny, cosmos, stars, constellations, symphony, finale, celestial, supernova, annihilation
- NEVER moonlight, bells, feathers, void/mystery, fire/bells — those belong to other themes
- DestinyCollapse is the UNIVERSAL debuff — all 10 weapons apply it
- Dark prismatic palette: black→pink→crimson with celestial white highlights
- Each weapon: ~12-14 self-contained .cs files + 4 dedicated .fx shaders
- 5 melee / 2 magic / 2 ranged / 1 summon — melee-dominant endgame theme
- Coda of Annihilation (1350 dmg) = mod Zenith, cycles all 14 theme melee weapons
