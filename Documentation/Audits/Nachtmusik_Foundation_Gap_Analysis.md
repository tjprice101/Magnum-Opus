# Nachtmusik vs Foundation Weapons — Comprehensive Gap Analysis

## Executive Summary

**The Nachtmusik weapons have a complete shader infrastructure already built** — 13 custom .fx shaders (2 shared + 11 per-weapon), a fully developed `NachtmusikShaderManager.cs` with Apply/Glow methods per weapon, and noise texture bindings. However, **none of the Nachtmusik weapon code currently calls any of these shaders.** Every weapon relies entirely on:

- `Dust` particles (PurpleTorch, BlueTorch, GoldFlame, etc.)
- `oldPos`/`oldRot` afterimage trails (no GPU primitive mesh rendering)
- Multi-layer additive sprite stacking for glow (same SoftCircle/ImpactEllipse texture at different scales/opacities)
- `NachtmusikVFXLibrary` helper calls (SpawnMusicNotes, SpawnStarBurst, etc.)
- `CustomParticles` (GenericFlare, HaloRing)

**The Foundation Weapons demonstrate the target quality standard:** GPU-driven shaders applied via SpriteBatch.Begin(Immediate) or VertexStrip, UV-scrolling noise distortion, gradient LUT sampling, multi-pass shader rendering, and mesh-constructed trails. The gap is not about missing infrastructure — it's that the infrastructure exists but isn't wired into the actual weapons.

---

## Foundation Weapons — Architectural Patterns Summary

### SwordSmearFoundation (Melee Reference)
| Feature | Implementation |
|---------|---------------|
| **Trail** | Shader-driven arc overlay — `SmearDistortShader.fx` with FBM noise, UV scrolling, gradient LUT |
| **Sub-layers** | 3 shader passes per smear (outer glow, main, core) with different distort strengths |
| **Bloom** | Multi-scale additive sprite stacking (SoftGlow + StarFlare) at tip and root — manual per-draw-call bloom |
| **Blade drawing** | Custom blade sprite draw with rotation/position interpolation |
| **Fallback** | Full non-shader codepath with static colored layers (degrades gracefully) |
| **SpriteBatch** | Switches Immediate+Additive for shader passes, Deferred+Additive for bloom, Deferred+AlphaBlend to restore |
| **Textures** | Dedicated texture registry (`SMFTextures.cs`) with lazy-loaded shader Effect, slash arcs, color gradients, noise |

### RibbonFoundation (Trail/Particle Reference)
| Feature | Implementation |
|---------|---------------|
| **Trail mesh** | Position history ring buffer (40 points), extraUpdates=1 for density |
| **Trail rendering** | UV-mapped texture strips from `Assets/VFX/Trails/` with horizontal slice source rectangles |
| **Bloom** | Per-point bloom sprite stacking (3 layers per trail point) with velocity stretch |
| **Lightning mode** | Mode 10: jittered position offsets + per-segment bloom for electric arc look |
| **Variety** | 10 distinct rendering modes cycling through textures + techniques |

### ImpactFoundation (Impact VFX Reference)
| Feature | Implementation |
|---------|---------------|
| **Slash mark** | Shader-driven SDF slash — `SlashMarkShader.fx` with noise distortion, directional stretch |
| **Lifecycle** | FadeIn → Hold → FadeOut phase envelope controlling opacity |
| **Bloom** | Directional bloom sprites in both additive + alpha-blend passes |
| **Flash** | Central point flash with rapid scale-in decay |

### MagicOrbFoundation (Projectile/Orb Reference)
| Feature | Implementation |
|---------|---------------|
| **Orb core** | Shader-driven radial noise mask — `RadialNoiseMaskShader.fx` with scrolling noise |
| **Bloom halo** | 3-layer additive bloom stack (outer/mid/inner) with pulsing scales |
| **Sub-projectiles** | OrbBolt with 4-layer pure additive bloom (SoftGlow outer, SoftGlow mid, GlowOrb core, StarFlare cross sparkle) |

### LaserFoundation (Beam Reference)
| Feature | Implementation |
|---------|---------------|
| **Beam body** | `VertexStrip` triangle-strip mesh with `ConvergenceBeamShader.fx` — 4 scrolling detail textures + gradient LUT |
| **Endpoint flares** | `FlareRainbowShader.fx` with spinning radial rainbow |
| **Matrix** | Proper `WorldViewProjection` matrix passed to vertex shader |
| **Channeling** | Full charge/sustain/discharge lifecycle |

---

## Per-Weapon Gap Analysis

### KEY: Severity Levels
- 🔴 **CRITICAL** — Core visual identity missing; weapon looks flat/generic
- 🟡 **MAJOR** — Important visual layer absent; weapon is below Foundation quality
- 🟢 **MINOR** — Polish item; weapon is functional but could be elevated

---

### 1. Twilight Severance (Melee — Katana)

**Class:** Melee | **Base:** `MeleeSwingBase` / `MeleeSwingItemBase`
**Shader Exists:** `DimensionalRift.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| Swing trail uses `CalamityStyleTrailRenderer.TrailStyle.Cosmic` — NOT the per-weapon `DimensionalRift` shader | 🔴 | The `TwilightSeveranceSwing.cs` calls `CalamityStyleTrailRenderer.DrawTrail()` with generic Cosmic style. Should use `NachtmusikShaderManager.ApplyDimensionalRift()` for its unique dimensional tear identity. |
| Smear textures are generic VFX library masks (WideSoftEllipse, VerticalEllipse) — not per-weapon slash arcs | 🟡 | Foundation's SmearSwingProjectile uses dedicated slash arc textures with per-weapon shader distortion. Twilight uses shared VFX library masks with no shader processing. |
| No shader-driven smear overlay | 🔴 | Foundation renders each smear with 3 sub-layers of `SmearDistortShader` (noise distortion + gradient LUT). Twilight draws smear textures as flat additive sprites with no shader. |
| Tip glow is single-layer `BloomRenderer.DrawBloomStackAdditive` | 🟡 | Foundation uses multi-texture tip glow (SoftGlow + StarFlare at different scales). Twilight uses a utility function that likely does similar but without shader enhancement. |
| `TwilightSlashProjectile` trail is `oldPos` afterimage rendering | 🟡 | No GPU primitive mesh trail. Just iterates `Projectile.oldPos[]` and draws scaled sprites. Foundation's ribbon trails use position ring buffers with UV-mapped texture strips. |
| VFX file (`TwilightSeveranceVFX.cs`) is pure Dust + NachtmusikVFXLibrary calls | 🟢 | No shader involvement in any VFX helper method. All effects are CPU-side particles. |

**What it should become:** The dimensional rift shader should drive the swing trail with sharp, reality-tearing distortion. The smear overlay should use a dedicated slash arc texture processed through a shader with UV-scrolling void energy. The blade wave projectile needs a GPU primitive trail instead of oldPos sprites.

---

### 2. Nocturnal Executioner (Melee — Greatsword)

**Class:** Melee | **Base:** `MeleeSwingBase` / `MeleeSwingItemBase`
**Shader Exists:** `ExecutionDecree.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| Swing trail uses generic `CalamityStyleTrailRenderer.TrailStyle.Cosmic` | 🔴 | Should use `NachtmusikShaderManager.ApplyExecutionDecree()` — a void-rip slash effect with vortex noise, 3.5x overbright, 0.12 distortion. Currently identical trail rendering to Twilight Severance. |
| No shader-driven smear overlay | 🔴 | Same gap as Twilight — flat additive sprite smears, no shader processing. |
| `NocturnalBladeProjectile` and `ExecutionFanBlade` use `oldPos` afterimage trails | 🟡 | Both sub-projectiles draw scaled sprite afterimages. No primitive mesh, no shader trail. |
| Execution Fan VFX (`ExecutionFanVFX`) is entirely NachtmusikVFXLibrary calls | 🟡 | The marquee alt-fire attack has no shader-driven visuals despite being a major combat moment. |
| VFX file is pure Dust + library calls | 🟢 | Complex layered VFX (constellation circles, orbiting glyphs, fragmented starlight) but all CPU-side. |

**Uniqueness concern:** The swing rendering is currently **identical** to Twilight Severance — same `CalamityStyleTrailRenderer.TrailStyle.Cosmic`, same smear texture approach, same DrawCustomVFX pattern. Given the "Uniqueness Mandate," these two melee weapons need radically different trail rendering.

---

### 3. Midnight's Crescendo (Melee — Rapid Sword)

**Class:** Melee | **Base:** `MeleeSwingBase` / `MeleeSwingItemBase`
**Shader Exists:** `CrescendoRise.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| Swing trail uses generic `CalamityStyleTrailRenderer.TrailStyle.Cosmic` | 🔴 | Should use `NachtmusikShaderManager.ApplyCrescendoRise()` — an intensity-building trail that accepts `crescendoLevel` (0-1) to dynamically change visual intensity. This is the MOST unique shader (it responds to crescendo stacks) and it's completely unused. |
| Crescendo stack scaling is Dust-density-only, not shader-driven | 🔴 | The crescendo stack mechanic (0-15) scales dust count, bloom brightness, and random thresholds. But the trail shader itself doesn't change. Foundation-quality would have the shader intensity/overbright/distortion scale with stacks. |
| `CrescendoWaveProjectile` uses multi-layer additive sprite rendering with ImpactEllipse texture | 🟡 | 4 additive layers + afterimage trail via oldPos. Good layering approach but no shader processing — the expanding wave should have UV-scrolling internal energy or noise distortion. |
| No shader fallback behavior | 🟢 | The `NachtmusikShaderManager.ApplyCrescendoRise()` already has a fallback to `ApplyStarTrail()` if the shader is unavailable. But neither is called from the weapon code. |

**Uniqueness concern:** Same trail renderer and smear approach as Twilight and Nocturnal. Three melee weapons with identical trail rendering technology.

---

### 4. Constellation Piercer (Ranged — Rifle)

**Class:** Ranged | **Base:** Standalone `ModItem`
**Shader Exists:** `StarChainBeam.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `ConstellationBoltProjectile` trail is `oldPos` afterimage rendering | 🔴 | A precision rifle bolt needs a sharp, luminous GPU trail. Currently draws scaled sprite copies at old positions. Should use `NachtmusikShaderManager.ApplyStarChainBeam()` on a primitive trail mesh. |
| No constellation line rendering between star points uses shaders | 🟡 | `ConstellationPiercerVFX.ConstellationLineVFX()` draws dust particles in a line between points. A shader-driven beam connecting star points would be dramatically more impactful. |
| Seeking crystal projectiles have no custom rendering | 🟡 | Every 5th shot spawns seeking crystals — these likely use basic projectile rendering. |
| Muzzle flash is pure Dust + CustomParticles | 🟢 | Decent layered dust effects (crosshair flash lines, multiple dust types) but no shader bloom or flash shader. |

---

### 5. Nebula's Whisper (Ranged — Cannon)

**Class:** Ranged | **Base:** Standalone `ModItem`
**Shader Exists:** `NebulaScatter.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `NebulaWhisperShot` rendering is multi-layer additive sprite stacking only | 🔴 | 4 additive layers of WideSoftEllipse at different scales/colors. Good color gradient (CosmicPurple → Violet → DeepBlue → StarWhite) but no shader processing. The `NebulaScatter.fx` shader with cosmic noise and 0.09 distortion would make the nebula cloud feel alive and organic. |
| No expanding effect driven by shader parameters | 🟡 | The projectile expands via `DynamicScale` but this only changes sprite draw scale. A shader with a `uPhase` parameter could morph the internal noise pattern as the nebula expands. |
| Whisper Storm alt-fire convergence is pure Dust (30 dust particles streaming inward) | 🟡 | The VFX file shows 30 dust particles with `PurpleTorch`/`BlueTorch`/`PinkTorch` converging. Foundation-quality would use a shader-driven convergence effect or at minimum primitive trails on the converging particles. |

---

### 6. Serenade of Distant Stars (Ranged — Homing Rifle)

**Class:** Ranged | **Base:** Standalone `ModItem`
**Shader Exists:** `StarHomingTrail.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `SerenadeStarProjectile` trail is `oldPos` afterimage | 🔴 | A homing star needs a graceful arcing ribbon trail. The `StarHomingTrail.fx` shader exists specifically for this — a flowing arc with starfield noise. Currently just draws fading sprite copies. |
| `StarEchoProjectile` trail is basic `oldPos` afterimage | 🟡 | Secondary echo projectile uses even simpler afterimage rendering. It's secondary, but still needs improvement. |
| Rhythm stack system doesn't affect trail rendering | 🟡 | Higher stacks increase homing strength and add corona rays in PreDraw, but the trail quality doesn't change. Foundation-quality would pass stack count as a shader uniform to intensify the trail. |
| PreDraw rendering is not in additive blend state | 🟢 | The afterimage trail and star core drawings are done in the default blend state (AlphaBlend) — the `with { A = 0 }` colors are used but SpriteBatch isn't switched to Additive. This means the glow colors won't stack properly. |

---

### 7. Starweaver's Grimoire (Magic — Constellation Book)

**Class:** Magic | **Base:** Standalone `ModItem`
**Shader Exists:** `ConstellationWeave.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `StarweaverOrbProjectile` uses generic afterimage trail + 4-layer additive sprite glow | 🔴 | A constellation orb needs shader-driven rendering — the `ConstellationWeave.fx` with starfield noise would create a star map effect on the orb surface. Currently just multi-scale SoftCircle sprite stacking. |
| VFX file (`StarweaversGrimoireVFX.cs`) is the simplest of all — just raw `Dust.NewDust` calls | 🔴 | No use of NachtmusikVFXLibrary, no CustomParticles, no MagnumParticleHandler. Just basic `Dust.NewDust` with manual parameters. This is pre-Foundation quality. |
| Weave charge mechanic doesn't affect orb shader | 🟡 | `NachtmusikShaderManager.ApplyConstellationWeave()` accepts `chargeLevel` parameter. The charge building system exists in the weapon but doesn't modulate any visual. |
| Tapestry Weave alt-fire VFX is basic radial Dust burst | 🟡 | 30 dust particles + 12 motes. No shader, no screen effect, no primitive rendering. |

---

### 8. Requiem of the Cosmos (Magic — Cosmic Orbs)

**Class:** Magic | **Base:** Standalone `ModItem`
**Shader Exists:** `CosmicRequiem.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `CosmicRequiemOrbProjectile` uses generic afterimage trail + 4-layer additive sprite glow | 🔴 | Same pattern as Starweaver orb. Cosmic requiem orbs should have swirling nebula void energy — the `CosmicRequiem.fx` shader with cosmic noise and 0.12 distortion exists for exactly this. |
| Event Horizon (every 10th cast) has no screen-space effects | 🟡 | The Event Horizon is the weapon's marquee mechanic — a massive gravity-pulling void. It only has dust implosion VFX. Foundation-quality would include screen distortion, chromatic aberration, or screen shake. |
| Gravity well visual is just dust particles being pulled inward | 🟡 | The gravity well mode pulls NPCs but the visual is just `MagicMirror` dust drifting. A shader-distorted gravity lens effect would sell the mechanic. |
| VFX file (`RequiemOfTheCosmosVFX.cs`) is basic `Dust.NewDust` calls | 🔴 | Same issue as Starweaver — raw Dust API with no VFX library usage. |

---

### 9. Galactic Overture (Summoner — Celestial Muse)

**Class:** Summoner | **Base:** Standalone `ModItem`
**Shader Exists:** `OvertureAura.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `CelestialMuseMinion` rendering is basic sprite + 2-layer SoftGlow behind | 🟡 | The minion has a gold/violet glow behind its sprite but no shader-driven aura. The `OvertureAura.fx` could drive an orchestral wave effect around the muse. |
| `MuseNoteProjectile` uses `ProceduralProjectileVFX.DrawNachtmusikProjectile` | 🟢 | This is actually a step forward — it uses a centralized rendering system. Need to verify what that system does internally. |
| VFX file properly uses NachtmusikVFXLibrary consistently | 🟢 | Well-structured — uses SpawnMusicNotes, DrawBloom, AddPaletteLighting. Cleaner than Starweaver/Requiem VFX files. |
| No minion aura shader rendering during idle/attack | 🟡 | Foundation doesn't have a summoner reference, but the shader exists. The minion's ambient state should use `ApplyOvertureAura()`. |

---

### 10. Celestial Chorus Baton (Summoner — Nocturnal Guardian)

**Class:** Summoner | **Base:** Standalone `ModItem`
**Shader Exists:** `ChorusSummonAura.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `NocturnalGuardianMinion` rendering is sprite + 2-layer SoftGlow behind | 🟡 | Same minion rendering approach as Galactic Overture — glow sprites behind the main sprite. No shader aura. |
| Dash attack has no trail rendering | 🔴 | The guardian dashes at 22f velocity to attack enemies but has no visual trail during the dash. Only `NachtmusikVFXLibrary.SpawnCloudTrail()` which spawns dust. Foundation's ribbon trail demonstrates what a moving-projectile trail should look like. |
| Item PreDrawInWorld properly uses additive blending for world glow | 🟢 | The item has a 3-layer cosmic glow when dropped in world — one of the only Nachtmusik items to properly switch SpriteBatch to additive for its world rendering. |
| VFX file properly uses library helpers | 🟢 | Clean VFX structure with music note rings on summon, harmonic resonance pulses on hold. |

---

### 11. Conductor of Constellations (Summoner — Stellar Conductor)

**Class:** Summoner | **Base:** Standalone `ModItem`
**Shader Exists:** `StellarConductorAura.fx` ✅ | **Shader Used in Code:** ❌

| Gap | Severity | Detail |
|-----|----------|--------|
| `StellarConductorMinion` rendering is sprite + 3-layer SoftCircle glow behind | 🟡 | Triple glow layers (CosmicPurple, RadianceGold, Violet) — slightly better layering than the other two summoners. No shader. |
| `ConductorStarProjectile` uses `ProceduralProjectileVFX.DrawNachtmusikProjectile` | 🟢 | Uses centralized rendering system like MuseNoteProjectile. |
| Orchestra burst (8-star ring every 180 ticks) has no special rendering | 🟡 | The marquee periodic attack fires 8 stars in a ring pattern with library VFX but no screen effect, no shader-driven constellation web, no special burst rendering. |
| VFX file has good complexity (constellation web, connecting lines, conducting gestures) | 🟢 | Well-structured VFX with constellation point patterns and line rendering via GenericGlowParticle. |

---

## Cross-Cutting Issues

### 1. Three Identical Melee Weapons 🔴

Twilight Severance, Nocturnal Executioner, and Midnight's Crescendo all use:
- `MeleeSwingBase` with identical `CalamityStyleTrailRenderer.TrailStyle.Cosmic`
- Same smear texture approach (VFX library masks drawn additively)
- Same `DrawCustomVFX` pattern (dust + bloom + particles)

**Fix:** Each should use its own shader:
- Twilight → `DimensionalRift` (sharp tear effect)
- Nocturnal → `ExecutionDecree` (heavy void-rip with high distortion)
- Midnight → `CrescendoRise` (intensity-building, stack-responsive)

### 2. No Weapon Uses Its Custom Shader 🔴

All 11 weapon-specific shaders exist as .fx files. `NachtmusikShaderManager.cs` has complete Apply/Glow methods for every single one with proper fallback chains. **Not a single call to `NachtmusikShaderManager` exists in any weapon, projectile, or VFX file.**

The infrastructure gap is zero. The wiring gap is 100%.

### 3. Ranged/Magic/Summoner Projectiles All Use Same Pattern 🟡

Every non-melee projectile follows:
1. `TrailCacheLength` + `TrailingMode = 2`
2. `oldPos` iteration for afterimage trail (sprite-based, no GPU mesh)
3. Switch to Additive → draw 3-4 layered sprites → restore
4. Dust-based particle trail in AI()

Foundation's OrbBolt shows the difference: 4-texture additive bloom stacking with SoftGlow + GlowOrb + StarFlare, each contributing different visual information. The Nachtmusik projectiles use the same texture (SoftCircle or the projectile's own texture) at different scales.

### 4. VFX File Quality Varies Wildly 🟡

| VFX File | Quality Level | Issues |
|----------|---------------|--------|
| `TwilightSeveranceVFX.cs` | Good | Uses NachtmusikVFXLibrary + CustomParticles |
| `NocturnalExecutionerVFX.cs` | Good | Rich layered effects, constellation circles, authority glyphs |
| `MidnightsCrescendoVFX.cs` | Good | Stack-scaling VFX, crescendo storm at max |
| `ConstellationPiercerVFX.cs` | Good | Unique crosshair precision aesthetic |
| `NebulasWhisperVFX.cs` | Good | Atmospheric nebula gas effects |
| `SerenadeOfDistantStarsVFX.cs` | Good | Melodic arc spray, rhythm-scaling |
| `GalacticOvertureVFX.cs` | Good | Orchestral sweep arcs |
| `ConductorOfConstellationsVFX.cs` | Good | Constellation web patterns |
| `CelestialChorusBatonVFX.cs` | Good | Musical staff lines, harmonic resonance |
| **`StarweaversGrimoireVFX.cs`** | **Poor** | Raw `Dust.NewDust`, no VFX library usage |
| **`RequiemOfTheCosmosVFX.cs`** | **Poor** | Raw `Dust.NewDust`, no VFX library usage |

### 5. No Weapon Uses Dedicated VFX Textures 🟡

Foundation weapons have dedicated texture registries (SMFTextures, RBFTextures, etc.) pointing to per-weapon texture assets. All Nachtmusik weapons use:
- Shared VFX library textures (`Assets/VFX Asset Library/`)
- Shared particle textures (`Assets/Particles Asset Library/`)
- No per-weapon texture registries
- No per-weapon gradient LUTs or noise configurations

There are no `Assets/Nachtmusik/<WeaponName>/` folders with weapon-specific VFX textures.

### 6. SpriteBatch State Management Inconsistencies 🟢

Some projectiles don't switch to additive blending when drawing glow:
- `SerenadeStarProjectile` draws `with { A = 0 }` colors in default AlphaBlend mode
- `StarEchoProjectile` same issue
- Most others correctly switch, but inconsistently (some use `spriteBatch.End()/Begin()` inline, others use helpers)

---

## Priority Recommendations

### Phase 1: Wire Up Existing Shaders (Highest Impact)

The shaders and manager already exist. Each weapon's swing/projectile code needs:

1. **Melee swings:** Replace `CalamityStyleTrailRenderer.TrailStyle.Cosmic` with per-weapon `NachtmusikShaderManager.Apply*()` calls in `DrawCustomVFX` / trail rendering.
2. **Ranged/magic projectile trails:** Build primitive trail meshes using a shared trail helper. Apply per-weapon shaders to the mesh.
3. **Summoner auras:** Add shader-driven aura rendering to minion `PreDraw` using per-weapon aura shaders.

### Phase 2: Differentiate the Three Melee Weapons

Each melee weapon needs a distinct visual identity:
- **Twilight Severance:** Sharp, clean dimensional tears. Minimal noise, high contrast edges.
- **Nocturnal Executioner:** Heavy void-rip slashes. High noise distortion, gravitational pull on particles.
- **Midnight's Crescendo:** Stack-building intensity. Trail grows brighter/wider with crescendo stacks via shader uniform.

### Phase 3: Upgrade Projectile Trail Rendering

Replace `oldPos` sprite afterimages with GPU primitive trails for:
- ConstellationBoltProjectile
- SerenadeStarProjectile
- NebulaWhisperShot
- StarweaverOrbProjectile
- CosmicRequiemOrbProjectile
- CrescendoWaveProjectile
- TwilightSlashProjectile
- NocturnalBladeProjectile / ExecutionFanBlade

### Phase 4: Fix the Two Weakest VFX Files

- `StarweaversGrimoireVFX.cs` — Bring up to library standard (use NachtmusikVFXLibrary, CustomParticles, MagnumParticleHandler, bloom helpers)
- `RequiemOfTheCosmosVFX.cs` — Same upgrade needed

### Phase 5: Per-Weapon Texture Assets

Create `Assets/Nachtmusik/<WeaponName>/` folders with:
- Gradient LUT textures (per-weapon color ramps)
- Slash arc textures (melee weapons)
- Trail textures (unique UV patterns per weapon)
- Noise texture configurations (per-weapon noise character)

---

## Quick Reference: Shader Wiring Map

| Weapon | Shader | Manager Method | Where To Wire |
|--------|--------|----------------|---------------|
| Twilight Severance | `DimensionalRift.fx` | `ApplyDimensionalRift()` / `ApplyDimensionalRiftGlow()` | `TwilightSeveranceSwing.DrawCustomVFX` trail rendering |
| Nocturnal Executioner | `ExecutionDecree.fx` | `ApplyExecutionDecree()` / `ApplyExecutionDecreeGlow()` | `NocturnalExecutionerSwing.DrawCustomVFX` trail rendering |
| Midnight's Crescendo | `CrescendoRise.fx` | `ApplyCrescendoRise(time, crescendoLevel)` / `ApplyCrescendoRiseGlow()` | `MidnightsCrescendoSwing.DrawCustomVFX` trail rendering |
| Constellation Piercer | `StarChainBeam.fx` | `ApplyStarChainBeam()` / `ApplyStarChainBeamGlow()` | `ConstellationBoltProjectile.PreDraw` primitive trail |
| Nebula's Whisper | `NebulaScatter.fx` | `ApplyNebulaScatter()` / `ApplyNebulaScatterGlow()` | `NebulaWhisperShot.PreDraw` rendering |
| Serenade of Distant Stars | `StarHomingTrail.fx` | `ApplyStarHomingTrail()` / `ApplyStarHomingTrailGlow()` | `SerenadeStarProjectile.PreDraw` primitive trail |
| Starweaver's Grimoire | `ConstellationWeave.fx` | `ApplyConstellationWeave(time, chargeLevel)` / glow | `StarweaverOrbProjectile.PreDraw` orb rendering |
| Requiem of the Cosmos | `CosmicRequiem.fx` | `ApplyCosmicRequiem(time, phase)` / glow | `CosmicRequiemOrbProjectile.PreDraw` orb rendering |
| Celestial Chorus Baton | `ChorusSummonAura.fx` | `ApplyChorusSummonAura()` | `NocturnalGuardianMinion.PreDraw` aura |
| Galactic Overture | `OvertureAura.fx` | `ApplyOvertureAura()` | `CelestialMuseMinion.PreDraw` aura |
| Conductor of Constellations | `StellarConductorAura.fx` | `ApplyStellarConductorAura()` | `StellarConductorMinion.PreDraw` aura |
| *(shared core)* | `NachtmusikStarTrail.fx` | `ApplyStarTrail()` / `ApplyStarTrailGlow()` | Fallback for all weapons |
| *(shared core)* | `NachtmusikSerenade.fx` | `ApplySerenade()` | Bloom/aura enhancement for all |
