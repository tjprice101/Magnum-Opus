---
description: "Design a boss encounter's VFX across all phases — maps musical movements to boss phases, designs arena backgrounds, attack telegraphs, phase transitions, cinematic moments, and enrage escalation. Use when planning or implementing boss fight visual effects."
---

# Design Boss Phase VFX Workflow

This skill guides you through designing the complete visual effects suite for a boss encounter in MagnumOpus, structured as a musical performance.

## Step 0: Interactive Boss Concept Dialog

**MANDATORY.** Before survey or design, have an interactive dialog with the user.

**Round 1: Boss Identity (4-5 questions)**
1. "What theme/score does this boss represent?"
2. "What is the boss's narrative role? (Guardian, destroyer, trickster, tragic figure, cosmic force?)"
3. "How many phases? What triggers transitions?"
4. "What's the emotional arc across all phases?"
5. "In one phrase, what should FIGHTING this boss feel like? ('Surviving a thunderstorm,' 'Dancing with death,' 'Witnessing the end of the world')"

**Round 2: Arena & Atmosphere (3-4 questions based on Round 1)**
6. "Should the arena atmosphere feel claustrophobic (enclosed, oppressive) or vast (open, overwhelming scale)?"
7. "Ambient effects during the fight: persistent weather (rain/embers/petals), floating objects, musical elements, or clean empty space?"
8. "Screen-level mood: always dark and dramatic, or starts calm and escalates? Should the sky/background change per phase?"
9. "Reference boss encounters from any game that capture the feel you want?"

**Round 3: Attack VFX Philosophy (2-3 questions)**
10. "Attack readability vs spectacle balance: scale 1-10 where 1=pure clarity and 10=screen-filling chaos?"
11. "Should attacks have theatrical telegraphs (dramatic windups, ground markings) or feel sudden and reactive?"
12. "What's the ONE signature attack that players will remember and tell their friends about?"

After dialog, proceed to survey and design. Answers from this dialog inform ALL subsequent steps.

## Step 0b: Survey Existing Boss Infrastructure

**Before designing ANYTHING, read the existing boss systems.** This prevents reinventing patterns that already exist and ensures your design fits the established architecture.

### Mandatory Reads

1. **Shared boss utilities** — read these first to understand what's already built:
   - `Common/Systems/VFX/Boss/BossAttackVFXHelper.cs` — Reusable attack patterns: `RadialBurst()`, `DashTrail()`, `SlamImpact()`, `BeamSweep()`, `SpiralBarrage()`, `SummonCircle()`, `PhaseTransitionFlash()`
   - `Common/Systems/VFX/Boss/BossSignatureVFX.cs` — Per-boss signature effects (30+ examples)
   - `Common/Systems/VFX/Boss/BossShaderEffects.cs` — Shader-driven boss rendering utilities
   - `Common/Systems/VFX/Boss/BossArenaVFX.cs` — Arena ambient effects (floating particles with parallax)
   - `Common/Systems/VFX/Boss/TelegraphSystem.cs` — 5 telegraph types: `ThreatLine`, `DangerZone`, `ConvergingRing`, `ImpactPoint`, `SectorCone`
   - `Common/Systems/VFX/Boss/BossVFXOptimizer.cs` — Frame-skipped particle rendering
   - `Common/Systems/Bosses/BossShaderManager.cs` — Central shader loading + parameter application
   - `Common/Systems/Bosses/BossRenderHelper.cs` — `DrawShaderAura()`, `DrawShaderTrail()`, `DrawPhaseTransition()`, `DrawDissolve()`
   - `Common/Systems/Bosses/BossSkyRegistrationSystem.cs` — How CustomSky implementations are registered

2. **One complete boss example** — read all 4 files of a fully implemented boss to understand the pattern:
   - Pick the boss closest to your theme, e.g., for a fire theme read Eroica:
   - `Content/Eroica/Bosses/EroicasRetribution.cs` — Main boss AI + attack state machine
   - `Content/Eroica/Bosses/Systems/EroicaAttackVFX.cs` — Static attack VFX helper (per-attack methods)
   - `Content/Eroica/Bosses/Systems/EroicaBossShaderSystem.cs` — Shader-driven rendering (aura, trail, glow)
   - `Content/Eroica/Bosses/Systems/EroicaSky.cs` — CustomSky implementation with phase-aware rendering

### Architecture Patterns You'll Discover

| Pattern | What It Is | Where |
|---------|-----------|-------|
| **Static AttackVFX class** | Each boss has a static class with per-attack methods (`SwordDashTelegraph()`, `SwordDashImpact()`, etc.) | `Content/*/Bosses/Systems/*AttackVFX.cs` |
| **Static BossShaderSystem class** | Shader binding per phase — `DrawValorAura()`, `DrawBossGlow()` called from `PreDraw`/`PostDraw` | `Content/*/Bosses/Systems/*BossShaderSystem.cs` |
| **CustomSky inheritance** | Extends `CustomSky` (not `ModSky`), registered via `BossSkyRegistrationSystem`, activated with `SkyManager.Instance.Activate()` | `Content/*/Bosses/Systems/*Sky.cs` |
| **SpriteBatch shader cycle** | End → Begin(Immediate, ShaderAdditive) → Apply → Draw → End → Begin(Deferred, AlphaBlend) | Every shader rendering call |
| **Multi-phase attack enum** | Boss AI uses attack enum with phase-gated subsets, VFX scales with `lifeRatio` / `difficultyTier` | Main boss `.cs` file |

**Only proceed to Step 1 after you've read and understood these patterns.**

## Step 1: Establish the Musical Structure

Every boss is a musical performance. Map the boss's phases to musical movements:

| Question | Purpose |
|----------|---------|
| What theme is this boss from? | Determines palette, emotional core, musical identity |
| How many phases? | Each phase = a musical movement |
| What's the emotional arc? | Tension → climax → resolution, or sustained intensity? |
| What's the boss's musical soul? | The specific aspect of the theme this boss embodies |

### Phase-to-Music Mapping Template

```
Phase 1 (Allegro) — Introduction
  Musical feel: [establishing the theme, moderate intensity]
  Arena: [initial background, ambient particles]

Phase 2 (Vivace) — Escalation  
  Musical feel: [tempo increases, new instruments join]
  Arena: [background intensifies, new particle types]

Phase 3 (Adagio) — The Quiet Before the Storm
  Musical feel: [dramatic pause, tension building]
  Arena: [subdued, ominous, anticipatory effects]

Phase 4 (Presto/Fortissimo) — Finale
  Musical feel: [maximum intensity, all instruments firing]
  Arena: [fully transformed, screen-commanding effects]
```

## Step 2: Design Arena Background Per Phase

Each phase should have a visually distinct arena atmosphere.

### Background Techniques (from reference repos)

| Technique | Complexity | Source |
|-----------|-----------|--------|
| Color tint + ambient particles | Simple | MagnumOpus `BossArenaVFX.cs` |
| Custom ModSkyEffect with shader | Medium | Calamity `DoGPortalShader.fx` |
| Per-phase procedural sky shader | Complex | WotG Avatar boss (raymarching, fractals) |
| Render-target-based composition | Advanced | WotG `Core/Graphics/RenderTargets/` |

### Per-Phase Background Design Template

For each phase, specify:
- **Background type**: Color shift / custom sky / procedural shader
- **Primary colors**: From theme palette
- **Ambient particles**: Type, density, behavior
- **Lighting mood**: Bright/dim, warm/cool, saturated/desaturated
- **Special atmospheric effects**: Fog, rain, heat haze, cosmic dust

## Step 3: Design Attack Telegraphs

Every attack needs a visual warning. Telegraph design:

### Telegraph Timing
- **Danger zone appears**: 0.5-1.0 seconds before attack lands
- **Warning opacity**: 30-50% (reads as "warning" not "damage")
- **Flash before impact**: 2-3 frames of higher opacity or pulse

### Telegraph Types by Attack Pattern

| Attack Type | Telegraph Visual |
|------------|----------------|
| **Projectile barrage** | Faint lines showing trajectory paths |
| **Beam sweep** | Thin line that grows to beam width |
| **Ground slam** | Expanding ring/circle at impact point |
| **Dash attack** | Directional arrow or streak preview |
| **Area denial** | Highlighted zone with pulsing boundary |
| **Summon attack** | Magic circle at spawn point |

### Telegraph Shader References
- WotG `DeadSunTelegraphBeamShader.fx` — Beam preview
- WotG `SunLaserTelegraphShader.fx` — Laser indicator
- MagnumOpus `TelegraphSystem.cs` — Existing infrastructure

## Step 4: Design Signature Attacks

Each phase should have 1-2 signature attacks with screen-commanding VFX.

### Attack VFX Layers (scale by attack importance)

**Standard attacks (3-4 layers):**
1. Projectile/hitbox visual
2. Trail/motion behind it
3. Impact effect on contact
4. Brief screen flash or shake

**Signature attacks (5-7 layers):**
1. Wind-up visual (charge glow, gathering particles)
2. Release burst (explosion of particles outward)
3. Projectile/beam with full shader rendering
4. Trail with UV-scrolled texture
5. Impact with multi-layered burst
6. Screen distortion wave
7. Camera shake + brief flash

**Ultimate attack / phase finisher (7+ layers):**
All of the above, plus:
- Anime focus lines
- Reality punch overlay
- Chromatic aberration
- Background temporarily changes
- Cinematic letterboxing

### Attack Variety Within a Phase

No two attacks in the same phase should look the same:
- **Attack A**: Beam sweep (continuous laser visual)
- **Attack B**: Projectile barrage (multiple scattered particles)
- **Attack C**: Ground slam (radial shockwave + debris)
- **Attack D**: Summon (magic circle + minion spawn)

## Step 5: Design Phase Transitions

Phase transitions are KEY CHANGES — the most dramatic visual moments.

### Transition Intensity Levels

**Minor transition (between sub-phases):**
- Background color shift over 30-60 frames
- Particle burst from boss center
- Brief screen flash (5-10 frames)

**Major transition (between main phases):**
- Screen white-out / black-out (15-30 frames)
- Chromatic aberration pulse expanding outward
- Background shader swaps completely
- Reality distortion wave from boss center
- All ambient particles change type
- Cinematic letterboxing during transition

**Final phase transition:**
- Screen tear effect (WotG `NamelessScreenSplitShader.fx`)
- Full reality punch (WotG `StrongRealityPunchOverlayShader.fx`)
- Background completely transforms
- Maximum particle intensity
- Anime focus lines + extreme blur
- Potential brief gameplay freeze (dramatic pause)

### Transition Checklist
- [ ] Player clearly understands "the boss just changed"
- [ ] Transition takes 1-3 seconds (not too fast, not dragging)
- [ ] New phase's visual baseline is established immediately after
- [ ] Transition VFX uses the NEW phase's palette, not the old one

## Step 6: Design Enrage / Finale VFX

The enrage is the FORTISSIMO — maximum visual intensity.

### Enrage Escalation Techniques
- **Ambient particle density** increases 2-3x
- **Background shader intensity** increases (more contrast, more movement)
- **Attack telegraphs** become shorter (less warning, more intensity)
- **Screen effects** become more frequent (shake, flash, distortion)
- **Boss aura** becomes visible (glow, orbiting particles, chromatic edge)
- **Color palette** shifts hotter/more intense within the theme
- **New visual types** appear that weren't present earlier (e.g., splitting reality, afterimages on the boss)

## Step 7: Performance Budget

Boss fights are the most GPU-intensive moments. Plan the budget:

### Per-Phase Particle Budget
| Phase | Max Particle Count | Rationale |
|-------|-------------------|-----------|
| Phase 1 | 200-400 | Establishing, moderate intensity |
| Phase 2 | 400-800 | Escalating, more visual elements |
| Phase 3 | 300-500 | Quiet before storm, but ominous |
| Enrage | 800-1500 | Maximum intensity |

### Performance Techniques
- **Downscale** full-screen effects to 0.5x resolution (WotG `DownscaleOptimizedScreenTarget`)
- **LOD** system — reduce particle count at lower quality settings
- **Frustum culling** — don't render particles offscreen
- **Shader complexity** — keep full-screen shaders under 20 texture samples
- **Render target reuse** — pool RTs, don't allocate per frame
- **GPU particle batching** — use FastParticleSystem for high counts

### Optimization Checkpoints
- [ ] Maintains 60fps on GTX 1060 / equivalent
- [ ] Phase transitions don't cause frame drops
- [ ] Enrage phase stays within particle budget
- [ ] Full-screen shaders have acceptable per-frame cost

## Step 8: Design Screen Effects & Cinematic Moments

Route to specialized agents for screen-level and cinematic design:

### Screen Effects (invoke @screen-effects)
For each phase, define:
- **Persistent screen mood**: Vignette color/intensity, desaturation level, color grading LUT
- **Attack screen responses**: Which attacks trigger shake, distortion, flash, or chromatic aberration
- **Transition screen effects**: Shockwave, white-out, reality tear, or fracture
- **Exclusion zones**: Boss and UI MUST remain readable through all distortion

### Lighting & Atmosphere (invoke @lighting-atmosphere)
For each phase, define:
- **Arena lighting**: Dynamic light sources, color, pulsing patterns
- **Boss aura**: Glow color, radius, noise erosion, per-phase evolution
- **God rays / fog**: If applicable, when they activate and fade
- **Color grading**: Per-phase LUT or tint adjustments

### Cinematic Sequences (reference AttackAnimationFoundation)
Design timed cinematic moments:
- **Boss intro**: Camera control, screen darken, reveal flash, nameplate timing
- **Phase transitions**: Slow-mo, camera zoom, VFX burst, new phase reveal
- **Signature attack cinematics**: Brief gameplay pause, anime focus lines, dramatic camera
- **Death sequence**: Multi-beat death animation with escalating visual spectacle

Reference: `Content/FoundationWeapons/AttackAnimationFoundation/` for frame-by-frame cinematic timing patterns.

## Step 9: Document the Design

Before implementing, write up the full boss VFX design as a reference document:

```markdown
# [Boss Name] — VFX Design Document

## Theme: [Theme Name]
## Musical Structure: [Overview of phase-to-music mapping]

### Phase 1: [Name] — [Musical Analogy]
- Arena: [background description]
- Attacks: [list with VFX approach for each]
- Ambient: [particle types and density]
- Signature Move: [description + VFX layers]

### Transition 1→2: [description]

### Phase 2: [Name] — [Musical Analogy]
[...]

### Final Phase / Enrage
[...]

## Asset Requirements
- [List every texture/sprite needed]
- [Midjourney prompts for any missing assets]

## Shader Requirements
- [List new shaders needed]
- [Reference implementations to adapt from]

## Performance Notes
- [Budget per phase]
- [Optimization strategies]
```

## Step 10: Request Missing Assets

For EVERY visual asset the boss fight needs that doesn't exist:

1. What asset is needed and where
2. Detailed **Midjourney prompt** with:
   - Art style and medium
   - Subject description
   - Color palette (theme-appropriate, on black background for VFX)
   - Dimensions and technical requirements
3. Expected file location

**HARD STOP if assets are missing.** Boss fights require complete asset sets before implementation.

## Step 11: Implementation Order

Implement in this order for iterative testing:

1. **Arena background** (Phase 1 only) — sets the visual stage
2. **Boss ambient effects** (aura, idle particles) — establishes presence
3. **Standard attacks** (VFX per attack type) — core gameplay visuals
4. **Signature attacks** (full layered VFX) — the showpieces
5. **Phase transitions** — connecting the movements
6. **Subsequent phases** (repeat 1-4 for each phase)
7. **Enrage escalation** — maximum intensity overlay
8. **Cinematic moments** (opening, death, special events)
9. **Performance optimization** pass
10. **Polish** (edge cases, edge quality, timing tweaks)
