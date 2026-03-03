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

## Weapons Overview

| # | Weapon | Class | Status | Key Mechanic |
|---|--------|-------|--------|-------------|
| 1 | Incisor of Moonlight | Melee | ✅ Implemented | Three Movements of Moonlight (Adagio→Allegretto→Presto→Finale) |
| 2 | Eternal Moon | Melee | 🔧 Needs VFX Polish | Tidal wave swings, ghost projections, tidal detonations |
| 3 | Resurrection of the Moon | Ranged | 🔧 Needs VFX Polish | Supernova shells, comet core projectiles |
| 4 | Moonlight's Calling | Magic | 🔧 Needs VFX Polish | Channeled serenade beam, spectral child beams, prismatic detonation |
| 5 | Staff of the Lunar Phases | Summon | 🔧 Needs VFX Polish | Goliath of Moonlight minion, devastating charged beams |

---

## 1. Incisor of Moonlight (Melee) — ✅ IMPLEMENTED

### Identity & Musical Soul
The Incisor is the opening movement of Moonlight Sonata made physical — a blade that plays the famous three movements as combat phases. Movement I is the iconic rolling triplets (Adagio Sostenuto), Movement II is the deceptively light Allegretto, Movement III is the furious Presto Agitato, and the Grand Finale is a requiem strike that brings all movements together in a 360° cinematic spin.

### Combat Mechanics
- **Movement I — Adagio Sostenuto** (1 swing): Slow, heavy overhead arc. Fires 3 CrescentMoonProj in rolling triplets.
- **Movement II — Allegretto** (2 swings): Fast dual slash combo. 2nd slash fires 5 StaccatoNoteProj in a fan. Bouncing notes detonate if 3+ hit the same enemy.
- **Movement III — Presto Agitato** (5 swings): Rapid flurry. Each slash fires a LunarBeamProj + OrbitingNoteProj. 5th slash creates CrescentWaveProj shockwave (Moonlit Silence debuff).
- **Grand Finale — Requiem Strike** (1 swing): 360° spin. 12 radial CrescentMoonProj. All OrbitingNoteProj converge. Screen flash.
- **Passive — Lunar Resonance**: Standing still for 2 seconds grants +8% damage on next swing, with visual charging particles.

### VFX Architecture (Implemented)
- **Custom Shaders**: IncisorSlashShader (swing arc with Voronoi noise), IncisorSwingSprite (blade rotation), IncisorPierceShader, IncisorResonance (standing-wave trail), ConstellationField (parallax starfield)
- **Primitive Renderer**: IncisorPrimitiveRenderer — 611-line GPU trail system with custom IncisorVertex (Position2D + Color + UV3D), DynamicVertexBuffer/IndexBuffer
- **Particle System**: Self-contained IncisorParticleHandler (ModSystem, On_Main.DrawDust hook, 500 limit) with ConstellationSparkParticle, LunarMoteParticle, MoonlightMistParticle
- **Trail Colors**: Per-movement color variation — cold blue (I), silver-white (II), deep purple (III), brilliant white (Finale)
- **Visual Indicators**: Moon phase icon above player (🌑→🌒→🌕→🌟) during combo

### Sub-Projectiles
| Projectile | Type | Behavior |
|-----------|------|----------|
| CrescentMoonProj | Crescent arc | Shallow arcs, pierce once, applies LunarResonanceDebuff |
| StaccatoNoteProj | Bouncing note | Bounces off tiles (2x), gravity-affected, 3+ same-target detonation via LunarNova |
| OrbitingNoteProj | Orbiting note | Orbits player 3s → homes on enemies → converges during Finale |
| CrescentWaveProj | Expanding ring | EaseOutQuart expansion, applies MoonlitSilenceDebuff (40% slow) |
| LunarBeamProj | Piercing beam | Fast-moving moonlight beam, pierces enemies |
| LunarNova | Detonation | Explosion from staccato convergence — massive burst |

### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| LunarResonanceDebuff | DoT (resonance decay) | 120 frames |
| MoonlitSilenceDebuff | 40% movement speed reduction | 180 frames |
| MoonlitStasis | Full stasis (from crescent wave critical) | 60 frames |

---

## 2. Eternal Moon (Melee) — VFX Polish Required

### Identity & Musical Soul
Where the Incisor represents the three movements of Moonlight Sonata, the Eternal Moon embodies the **eternal recurrence of the moon itself** — tidal forces, the gravitational pull, the inescapable cycle. This is a greatsword of overwhelming lunar weight. Every swing should feel like the tide crashing against the shore. If the Incisor dances, the Eternal Moon **drowns**.

### Lore Line
*"The tide remembers what the shore forgets."*

### Combat Mechanics (Existing — Enhance VFX)
The Eternal Moon already has:
- **Tidal Wave Swings**: Each swing creates expanding wave projections
- **Ghost Projections**: Phantom blades that mimic the player's swing 0.3s later
- **Crescent Slashes**: Thrown crescent projectiles that return
- **Tidal Detonation**: Massive AoE when crescent impacts overlap

#### Proposed Enhancements
- **Tidal Phase Meter**: Visible lunar-tide meter (Low Tide → Flood → High Tide → Tsunami). Meter fills as player swings. At Tsunami, next swing creates massive full-screen tidal wave.
- **Gravitational Pull**: Hits apply a weak vortex — enemies near the target are slowly pulled toward impact point for 1 second.
- **Echoing Tides**: Every 4th swing echoes the previous 3 swings as ghostly afterimage replays (like waves crashing repeatedly).

### VFX Architecture Plan

#### Custom Shaders (3 new + 1 enhanced)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `EternalMoonTidalTrail.fx` | Tidal wave trail on swings | UV-scrolled water/wave texture with Perlin noise distortion. Color ramp from deep purple core → ice blue edge → white foam tip. `smoothstep` edge fading on Y-axis. |
| `EternalMoonGhostProjection.fx` | Ghost swing afterimage | Alpha-faded version of the sword sprite with chromatic blue shift. Uses previous frame positions for temporal offset. Additive blend. |
| `EternalMoonGravityWell.fx` | Gravitational pull VFX around impact | Radial SDF ring that contracts inward over time. Voronoi noise distortion for organic gravitational turbulence. Dark purple with silver streaks. |
| `EternalMoonPhaseAura.fx` (enhance existing) | Tidal phase meter glow on player | Concentric rings pulsing outward from player center. Ring count = tide phase. Colors intensify from faint purple (Low Tide) to brilliant white (Tsunami). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| TidalDropletParticle | Falls with gravity, slight horizontal drift | Soft blue-white water droplets, size 3-6px, alpha fade over 20-30 frames |
| WaveSprayParticle | Burst radially from impact, high velocity, quick fade | White-to-blue spray mist, 10-15 per impact, arc trajectory |
| MoonGlintParticle | Stationary sparkle at blade tip, slow rotation | 4-pointed silver star, gentle pulse, 15-25 frame lifetime |
| GravityWellMoteParticle | Spiral inward toward gravity center | Purple-to-transparent motes, logarithmic spiral path, consumed at center |

#### Bloom Layers
1. **Blade Aura**: Tight soft bloom around sword body (ice blue, scale 1.2x blade size)
2. **Tidal Crest**: Wide rolling bloom at swing arc apex (deep purple → white gradient)
3. **Impact Flash**: 3-layer stacked bloom on hit (small white core + medium blue + wide purple ambient)
4. **Tsunami Flash**: Full-screen white flash (0.1s) with radial bloom rings expanding outward at Tsunami phase

#### Trail Rendering
- **Primitive mesh**: 60-point trail strip, 0.5s trail length
- **UV scrolling**: Wave-form texture scrolling along UV.x at 1.5x swing speed
- **Width function**: `sin(progress * PI) * baseWidth * tidePhaseMultiplier` — wider trail at higher tide phases
- **Color**: LUT ramp sampling — intensity maps to deep purple (cool) → white (hot)

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Tidal wave trail texture | `Assets/MoonlightSonata/EternalMoon/Trails/TidalWave.png` | "Horizontal flowing water wave texture, deep blue to white gradient, stylized anime water with foam crests, on solid black background, 512x64px seamless tiling --ar 8:1 --style raw" |
| Ghost projection glow | `Assets/MoonlightSonata/EternalMoon/Trails/GhostGlow.png` | "Soft ethereal ghost trail, translucent blue-silver wisps flowing horizontally, on solid black background, 256x64px, seamless edges --ar 4:1 --style raw" |
| Gravity well mask | `Assets/MoonlightSonata/EternalMoon/Orbs/GravityWell.png` | "Concentric gravitational distortion rings, dark purple center fading to transparent edge, circular mask, on solid black background, 256x256px --ar 1:1 --style raw" |
| Tidal phase meter icons | `Assets/MoonlightSonata/EternalMoon/Pixel/TidePhase.png` | "4-frame pixel art sprite sheet of moon tide phases, low tide to tsunami, soft blue glow, 64x16px total (16x16 per frame), on solid black background --ar 4:1 --style raw" |

#### Sound Design
- **Swing**: Deep whoosh with underwater reverb quality (pitch deepens with tide phase)
- **Ghost projection**: Ethereal echo of the swing sound, 0.3s delay, pitched up slightly
- **Tidal detonation**: Crashing wave SFX with resonant bass thump
- **Tsunami phase trigger**: Whale-song-like rising tone + glass bell chime

#### File Structure
```
Content/MoonlightSonata/Weapons/EternalMoon/
├── EternalMoon.cs                          — Main item (existing)
├── EternalMoonVFX.cs                       — VFX static class (new/enhance)
├── Projectiles/
│   ├── EternalMoonSwing.cs                 — Swing projectile (existing)
│   ├── EternalMoonWave.cs                  — Tidal wave (existing)
│   ├── EternalMoonGhost.cs                 — Ghost projection (existing)
│   ├── EternalMoonCrescentSlash.cs         — Crescent (existing)
│   └── EternalMoonTidalDetonation.cs       — Detonation (existing)
├── Particles/                              — New self-contained particle system
│   ├── EternalMoonParticleHandler.cs
│   ├── TidalDropletParticle.cs
│   ├── WaveSprayParticle.cs
│   ├── MoonGlintParticle.cs
│   └── GravityWellMoteParticle.cs
├── Primitives/
│   └── EternalMoonPrimitiveRenderer.cs     — Trail mesh builder
├── Shaders/
│   └── EternalMoonShaderLoader.cs          — Shader registration
├── Utilities/
│   ├── EternalMoonPlayer.cs                — Tidal phase meter tracker
│   └── EternalMoonUtils.cs                 — Easing curves, color palettes
└── Buffs/
    └── TidalGraspDebuff.cs                 — Gravitational pull slow

Assets/MoonlightSonata/EternalMoon/
├── Trails/
│   ├── TidalWave.png
│   └── GhostGlow.png
├── Orbs/
│   └── GravityWell.png
└── Pixel/
    └── TidePhase.png

Effects/MoonlightSonata/EternalMoon/
├── EternalMoonTidalTrail.fx
├── EternalMoonGhostProjection.fx
├── EternalMoonGravityWell.fx
└── EternalMoonPhaseAura.fx
```

---

## 3. Resurrection of the Moon (Ranged) — VFX Polish Required

### Identity & Musical Soul
The Resurrection is the **third movement's fury made ranged** — the explosive rebirth of the moon after its death. Where the Incisor plays the Sonata with a blade, the Resurrection plays it with cosmic artillery. Every shot should feel like the moon exploding and reforming. Supernova shells that collapse into new moons. Comet cores that trail silver light.

### Lore Line
*"What dies in moonlight is reborn in starfire."*

### Combat Mechanics (Existing — Enhance VFX)
- **Supernova Shells**: Primary fire — massive slow projectiles that detonate in expanding rings
- **Comet Core**: Alt fire — fast piercing projectiles that leave long trails and apply lunar impact on hit

#### Proposed Enhancements
- **Lunar Cycle Ammo System**: Shots cycle through New Moon (piercing, dark) → Waxing (balanced) → Full Moon (maximum AoE, brilliant white) → Waning (homing, spectral). Each phase has distinct visual identity.
- **Moonrise Charge**: Hold to charge — longer hold = bigger supernova shell. Visual: gun barrel accumulates swirling lunar energy, silver particles spiral inward.
- **Eclipse Synergy**: If a Supernova Shell and Comet Core collide, they create a brief Eclipse event — massive circular AoE with both explosion types overlapping.

### VFX Architecture Plan

#### Custom Shaders (3 new)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ResurrectionShellTrail.fx` | Supernova shell trail | Radial UV-scrolled texture expanding outward from projectile center. FBM noise + Voronoi for turbulent corona effect. Deep purple → ice blue → white gradient via LUT. |
| `ResurrectionCometTrail.fx` | Comet core trail strip | Classic UV-scrolled comet tail with noise distortion. Wider at head, narrow taper. Silver-white core → purple edge. |
| `ResurrectionImpactRing.fx` | Supernova detonation rings | Animated SDF ring expanding from center. Ring thickness narrows as radius grows. Color shifts from white (center) → purple (edge) with chromatic fringing. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SupernovaEmberParticle | Radial burst from detonation, decelerating | Hot white → cool purple, ember-like flicker, 20-30 per detonation |
| CometTrailSparkParticle | Shed from comet tail, slight drift, short life | Silver sparkle points, 1-3px, 10-15 frame lifetime, spawns every 2 frames |
| LunarImpactDebrisParticle | Radial with rotation, gravity-affected | Small lunar rock fragments, purple-tinted, tumble as they fall |
| MoonriseChargeParticle | Spiral inward toward barrel during charge | Soft purple motes, logarithmic spiral, consumed at gun barrel center |

#### Bloom Layers
1. **Barrel glow**: Persistent soft bloom at barrel tip (ice blue, pulses with fire rate)
2. **Shell corona**: Bright bloom ring around supernova shell in flight (3-layer: white core + blue mid + purple outer)
3. **Comet head**: Tight white-hot bloom at comet core (2-layer: white + ice blue)
4. **Detonation flash**: 4-layer stacked bloom — tiny white + small blue + medium purple + wide faint violet ambient

#### Trail Rendering
- **Supernova Shell**: Circular expanding corona behind projectile, 12-point radial mesh, UV scrolls outward
- **Comet Core**: 40-point trail strip, 0.8s trail length, Bézier-smoothed for graceful arcs

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Supernova corona texture | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Trails/SupernovaCrown.png` | "Radial explosion corona texture, bright white center fading to deep purple edges, stylized energy burst, on solid black background, 256x256px --ar 1:1 --style raw" |
| Comet tail trail | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Trails/CometTail.png` | "Horizontal comet tail energy trail, bright silver-white head fading to soft blue-purple tail, anime styled, on solid black background, 512x64px seamless --ar 8:1 --style raw" |
| Lunar impact ring | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Flare/ImpactRing.png` | "Thin glowing ring of lunar energy, ice blue with purple fringe, clean circle on solid black background, 256x256px --ar 1:1 --style raw" |
| Muzzle flash flare | `Assets/MoonlightSonata/ResurrectionOfTheMoon/Flare/MoonMuzzle.png` | "Stylized muzzle flash flare, silver-blue burst with 6 pointed star rays, on solid black background, 128x128px --ar 1:1 --style raw" |

#### Sound Design
- **Supernova shell fire**: Deep cannon boom with crystalline reverb
- **Comet core fire**: Sharp crack followed by singing trailing tone
- **Supernova detonation**: Expanding bass rumble + glass shattering + bell tone
- **Eclipse synergy**: Both sounds overlaid + sub-bass rumble + rising chime

#### File Structure
```
Content/MoonlightSonata/Weapons/ResurrectionOfTheMoon/
├── ResurrectionOfTheMoon.cs
├── ResurrectionVFX.cs
├── Projectiles/
│   ├── ResurrectionProjectile.cs
│   ├── SupernovaShell.cs
│   └── CometCore.cs
├── Particles/
│   ├── ResurrectionParticleHandler.cs
│   ├── SupernovaEmberParticle.cs
│   ├── CometTrailSparkParticle.cs
│   ├── LunarImpactDebrisParticle.cs
│   └── MoonriseChargeParticle.cs
├── Primitives/
│   └── ResurrectionPrimitiveRenderer.cs
├── Shaders/
│   └── ResurrectionShaderLoader.cs
├── Utilities/
│   ├── ResurrectionPlayer.cs
│   └── ResurrectionUtils.cs
└── Buffs/
    └── LunarImpactDebuff.cs

Effects/MoonlightSonata/ResurrectionOfTheMoon/
├── ResurrectionShellTrail.fx
├── ResurrectionCometTrail.fx
└── ResurrectionImpactRing.fx
```

---

## 4. Moonlight's Calling (Magic) — VFX Polish Required

### Identity & Musical Soul
Moonlight's Calling is a **serenade to the moon** — a channeled magic weapon that sings moonbeams into existence. Where Incisor fights and Eternal Moon crushes, Moonlight's Calling *reaches out*. It's the longing, the ache, the quiet desperation of a melody played alone in the dark. The beam should feel like moonlight streaming through clouds — soft at first, building to devastating brilliance.

### Lore Line
*"She called to the moon, and the moon wept silver."*

### Combat Mechanics (Existing — Enhance VFX)
- **Serenade Holdout**: Channel to maintain beam
- **Main Beam**: Central serenade beam that damages continuously
- **Spectral Child Beams**: After 2s channeling, 2-4 smaller beams split off and home on nearby enemies
- **Prismatic Detonation**: On release after 4s+ channeling, massive prismatic burst at cursor position

#### Proposed Enhancements
- **Resonance Building**: The longer you channel, the more harmonics build. Visual layers increase over time:
  - 0-1s: Single thin beam (pianissimo)
  - 1-2s: Beam widens + shimmer particles (piano)
  - 2-3s: Spectral child beams + standing wave nodes visible on main beam (mezzo-forte)
  - 3-4s: Full beam + orbiting music note particles + ground glow (forte)
  - 4s+: Maximum power beam + screen tint + release triggers Prismatic Detonation (fortissimo)
- **Harmonic Nodes**: Standing wave nodes appear on the beam at harmonic intervals. Enemies positioned at nodes take 1.5x damage.
- **Moonlight Puddles**: Where the beam touches the ground, it leaves temporary pools of moonlight that slow enemies (3s duration).

### VFX Architecture Plan

#### Custom Shaders (4 new)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `MoonlightBeamBody.fx` | Main serenade beam body | UV.x scrolls along beam length. Layered: base body (smooth gradient) + standing wave overlay (sin pattern with harmonic nodes highlighted). Color ramp from deep purple edge → ice blue body → white center. Noise distortion on edges for organic shimmer. |
| `MoonlightBeamCore.fx` | Bright inner core of beam | Thinner pass, additive blend, pure white-to-blue. Pulsing intensity tied to harmonics. |
| `MoonlightPrismaticBurst.fx` | Prismatic detonation on release | Radial expanding SDF circle with chromatic aberration — RGB channels expand at slightly different rates. Purple → blue → silver color cascade. |
| `MoonlightPuddleGlow.fx` | Ground moonlight pool | Circular SDF with Perlin edge distortion. Soft purple-white interior. Gentle pulse. Top-down projection shader. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| SerenadeNoteParticle | Orbits beam axis in helical path, travels along beam | Eighth/quarter note shapes (from Particle Asset Library), silver-white, gentle glow |
| HarmonicNodeParticle | Stationary at harmonic nodes, pulses | Bright white-blue star burst, 4-pointed, scale oscillates sinusoidally |
| ChannelingMistParticle | Rises from player during channeling | Soft purple mist, slow upward drift, widens and fades |
| PrismaticShardParticle | Radial burst from detonation | Small prismatic triangular shards, each reflects different palette color, spin as they fly |

#### Bloom Layers
1. **Staff tip glow**: Small persistent bloom at staff holdout point (purple, grows with channeling)
2. **Beam ambient**: Wide soft bloom along beam length (ice blue, opacity = channeling time normalized)
3. **Beam core**: Tight bright bloom along beam center (white, pulsing with standing wave frequency)
4. **Node highlights**: Bloom orbs at each harmonic node (silver, scale-pulse at harmonic frequency)
5. **Detonation**: 5-layer massive bloom at detonation site

#### Trail / Beam Rendering
- **Beam mesh**: Quad strip from staff to cursor, 2 passes (body + core)
- **Width function**: `baseWidth * (1 + 0.3 * sin(time * harmonicFreq))` — gentle breathing
- **UV.x**: Scrolls at beam's resonance speed (increases with channel time)
- **UV.y**: 0-1 across beam width, used for edge fading and color ramping
- **Standing wave overlay**: `sin(UV.x * PI * nodeCount)` multiplied against color for node highlights

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Beam body texture | `Assets/MoonlightSonata/MoonlightsCalling/Beams/SerenadeBeam.png` | "Horizontal energy beam texture with smooth flowing center and soft shimmering edges, silver-blue-purple color gradient, on solid black background, 512x64px seamless tiling --ar 8:1 --style raw" |
| Standing wave overlay | `Assets/MoonlightSonata/MoonlightsCalling/Beams/HarmonicWave.png` | "Standing wave pattern texture with bright nodes at regular intervals, white highlights on translucent blue, horizontal strip, on solid black background, 512x32px seamless --ar 16:1 --style raw" |
| Prismatic burst texture | `Assets/MoonlightSonata/MoonlightsCalling/Flare/PrismaticBurst.png` | "Radial energy burst with chromatic rainbow fringing at edges, bright white center fading to soft iridescent purple, on solid black background, 256x256px --ar 1:1 --style raw" |
| Moonlight puddle mask | `Assets/MoonlightSonata/MoonlightsCalling/Orbs/MoonlightPuddle.png` | "Top-down view of soft circular moonlight pool, gentle ripple edges, silver-blue glow with purple rim, on solid black background, 128x128px --ar 1:1 --style raw" |

#### Sound Design
- **Channeling start**: Soft harp glissando + sustained singing bowl tone
- **Resonance building**: Progressive harmonic overtones layered every 1s
- **Spectral beams**: Ghostly violin harmonics
- **Prismatic detonation**: Glass bell cascade + massive reverb wash
- **Moonlight puddle formation**: Gentle water-drop delay effect

#### File Structure
```
Content/MoonlightSonata/Weapons/MoonlightsCalling/
├── MoonlightsCalling.cs
├── MoonlightsCallingVFX.cs
├── Projectiles/
│   ├── SerenadeHoldout.cs
│   ├── SerenadeBeam.cs
│   ├── SpectralChildBeam.cs
│   └── PrismaticDetonation.cs
├── Particles/
│   ├── SerenadeParticleHandler.cs
│   ├── SerenadeNoteParticle.cs
│   ├── HarmonicNodeParticle.cs
│   ├── ChannelingMistParticle.cs
│   └── PrismaticShardParticle.cs
├── Primitives/
│   └── SerenadeBeamRenderer.cs
├── Shaders/
│   └── SerenadeShaderLoader.cs
└── Utilities/
    ├── SerenadePlayer.cs              — Channel time tracking, harmonic state
    └── SerenadeUtils.cs               — Standing wave math, color palettes

Effects/MoonlightSonata/MoonlightsCalling/
├── MoonlightBeamBody.fx
├── MoonlightBeamCore.fx
├── MoonlightPrismaticBurst.fx
└── MoonlightPuddleGlow.fx
```

---

## 5. Staff of the Lunar Phases (Summon) — VFX Polish Required

### Identity & Musical Soul
The Staff summons the **Goliath of Moonlight** — a massive spectral lunar entity that fights alongside the player. This is the Sonata's **silent accompanist** — the bass notes beneath the melody. While the player fights with other Moonlight weapons, the Goliath provides devastating support fire with moonlight beams. It should feel less like a minion and more like the **moon itself has descended to fight alongside you**.

### Lore Line
*"The moon does not ask permission to illuminate the dark."*

### Combat Mechanics (Existing — Enhance VFX)
- **Goliath Moonlight Beam**: Primary attack — focused beam that sweeps toward targeted enemy
- **Goliath Devastating Beam**: Charged attack (7s cooldown) — massive beam with screen effects

#### Proposed Enhancements
- **Lunar Phase Attacks**: The Goliath cycles through moon phases, each with different attack behavior:
  - **New Moon Phase**: Goliath fires rapid dark bolts (low damage, high fire rate)
  - **Waxing Phase**: Standard beam attack (balanced)
  - **Full Moon Phase**: Devastating beam (maximum damage, long cooldown)
  - **Waning Phase**: Healing aura pulse (restores 3 HP to player per pulse, every 2s)
- **Tidal Influence**: When the Goliath attacks, it generates a subtle gravitational "pull" visual effect — nearby dust particles drift toward the beam path.
- **Summoning Circle**: When the staff is used, a moonlit summoning circle appears on the ground below the player for 2 seconds with rotating lunar glyphs.

### VFX Architecture Plan

#### Custom Shaders (4 new)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `GoliathAuraShader.fx` | Ambient aura around the Goliath entity | Radial SDF with Perlin noise edge distortion. Colors cycle through phase palette. Gentle pulse opacity. Additive blend. |
| `GoliathBeamShader.fx` | Moonlight beam body | Similar to MoonlightBeamBody but wider, more turbulent. FBM noise distortion for raw power feel. Purple-white gradient. |
| `GoliathDevastatingShader.fx` | Devastating charged beam | Maximum power beam — double-width, chromatic aberration, screen distortion around edges. White core → blue body → purple edge → violet ambient. |
| `LunarSummonCircle.fx` | Summoning circle VFX | Top-down projected circle with rotating glyph ring. SDF circle + rotating UV for glyphs. Fade in over 0.5s, persist 2s, fade out 0.5s. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| GoliathOrbitalParticle | Orbits the Goliath entity slowly | Soft purple motes, 6-10 orbiting at various radii, gentle trail |
| GoliathBeamSparkParticle | Sheds from beam edges, perpendicular drift | Silver-blue sparks, 5-8px, 10-15 frame life, spawns every frame during beam |
| SummonCircleGlyphParticle | Rises from summoning circle, drifts upward | Glyph shapes from Particle Library, soft purple, slow ascent, fade over 30 frames |
| LunarHealingParticle | Rises from Goliath toward player during Waning phase | Soft blue-white orbs, gentle arc path, absorbed at player center |

#### Bloom Layers
1. **Goliath body glow**: Persistent large bloom around entity (deep purple, subtle pulse)
2. **Eye glow**: Bright point bloom at Goliath's "eye" area (ice blue, constant)
3. **Beam core**: Tight white bloom along beam path
4. **Devastating beam**: 5-layer bloom stack + screen flash on fire
5. **Summoning circle**: Ground-level bloom ring (purple, expanding on summon)

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Summoning circle texture | `Assets/MoonlightSonata/StaffOfTheLunarPhases/SummonCircle/LunarCircle.png` | "Top-down magic summoning circle with moon phase symbols around the edge, intricate lunar glyphs, soft purple glow on solid black background, 512x512px --ar 1:1 --style raw" |
| Goliath aura texture | `Assets/MoonlightSonata/StaffOfTheLunarPhases/Orbs/GoliathAura.png` | "Soft radial energy aura with irregular wispy edges, deep purple center to transparent edge, on solid black background, 256x256px --ar 1:1 --style raw" |
| Devastating beam texture | `Assets/MoonlightSonata/StaffOfTheLunarPhases/Beams/DevastatingBeam.png` | "Massive power beam texture with turbulent energy edges, white hot center fading to deep purple edges, horizontal strip, on solid black background, 512x128px seamless --ar 4:1 --style raw" |
| Phase indicator icons | `Assets/MoonlightSonata/StaffOfTheLunarPhases/Pixel/PhaseIcons.png` | "4-frame pixel art sprite sheet of moon phases (new, waxing, full, waning), elegant silver-purple style, 64x16px total (16x16 per frame), on solid black background --ar 4:1 --style raw" |

#### Sound Design
- **Summoning**: Deep reverberant gong + ascending crystalline tones
- **Goliath ambient**: Low, sustained bass hum (barely audible, like tidal breathing)
- **Moonlight beam**: Sustained singing bowl tone + laser whine
- **Devastating beam**: Full orchestral sforzando (sudden loud) + sustained power chord
- **Phase transition**: Soft bell chime + whooshing wind sound

#### File Structure
```
Content/MoonlightSonata/Weapons/StaffOfTheLunarPhases/
├── StaffOfTheLunarPhases.cs
├── StaffVFX.cs
├── Projectiles/
│   ├── GoliathMoonlightBeam.cs
│   └── GoliathDevastatingBeam.cs
├── Particles/
│   ├── GoliathParticleHandler.cs
│   ├── GoliathOrbitalParticle.cs
│   ├── GoliathBeamSparkParticle.cs
│   ├── SummonCircleGlyphParticle.cs
│   └── LunarHealingParticle.cs
├── Primitives/
│   └── GoliathBeamRenderer.cs
├── Shaders/
│   └── GoliathShaderLoader.cs
├── Utilities/
│   ├── GoliathPlayer.cs
│   └── GoliathUtils.cs
├── Dusts/
│   ├── GoliathDust.cs
│   └── Textures/
│       └── GoliathDust.png
└── Buffs/
    └── GoliathMinionBuff.cs

Effects/MoonlightSonata/StaffOfTheLunarPhases/
├── GoliathAuraShader.fx
├── GoliathBeamShader.fx
├── GoliathDevastatingShader.fx
└── LunarSummonCircle.fx
```

---

## Cross-Theme Synergy Notes

### Moonlight Sonata Theme Unity
All 5 weapons must feel like movements of the same sonata:
- **Shared color palette**: Deep Resonance → Frequency Pulse → Resonant Silver → Ice Blue → Crystal Edge → Harmonic White
- **Shared particle library**: MoonlightVFXLibrary provides SpawnMusicNotes, DrawBloom, MeleeImpact shared across all weapons
- **Unique per weapon**: Each weapon uses the palette differently. Incisor is constellation-focused, Eternal Moon is tidal, Resurrection is nova-focused, Calling is beam-focused, Staff is entity-focused.
- **Musical motif**: Music notes appear in ALL weapons' effects, but the note types and behaviors differ (rolling triplets for Incisor, scattered spray for Resurrection, helical orbit for Calling, etc.)

### Moonlight Lore Consistency
- All lore references moonlight, tides, silver, stillness, sorrow
- NEVER cosmos, stars, galaxies — those belong to Fate and Nachtmusik
- Moonlight touches the earth, it doesn't come from space. The visual language is earthbound lunar phenomena: tides, reflected light, nocturnal stillness, the ache of something beautiful and unreachable.
