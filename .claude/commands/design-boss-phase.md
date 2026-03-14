Design a boss encounter's complete VFX across all phases — arena atmosphere, attack telegraphs, signature attacks, phase transitions, enrage escalation, and cinematic moments.

Boss to design for: $ARGUMENTS

---

## Step 1: Interactive Boss Concept Dialog

**MANDATORY.** Run 12+ questions across 3 rounds before any design work.

### Round 1: Boss Identity (4-5 questions)
1. What THEME/SCORE is this boss from? (Constrains everything — palette, emotion, musical identity)
2. What is the boss's ROLE in the theme's story? (Guardian? Fallen hero? Force of nature? Manifestation of the music itself?)
3. In ONE PHRASE, what should fighting this boss feel like? ("Conducting a storm," "Dueling a dying swan," "Surviving divine judgment")
4. How many PHASES? (2-3 is standard, 4+ for endgame flagships)
5. What MUSICAL STRUCTURE should the fight follow? (Sonata form? Theme and variations? Crescendo to finale? Call and response?)

### Round 2: Visual Identity (3-4 questions)
6. Boss SILHOUETTE — What does it look like? (Humanoid? Beast? Abstract? Instrument-shaped? Cosmic entity?)
7. ARENA ATMOSPHERE — Should the background transform? (Dark void? Themed landscape? Abstract space? Concert hall?)
8. ATTACK STYLE — Elegant and precise? Overwhelming and chaotic? Rhythmic and patterned? Reality-warping?
9. SIGNATURE ATTACK — What's the one attack players will REMEMBER? (Screen-filling beam? Reality tear? Musical crescendo that fills the screen?)

### Round 3: Technical & Creative (3-4 questions)
10. PHASE TRANSITIONS — Dramatic pause + transformation? Sudden shift? Gradual escalation?
11. ENRAGE — How should desperation manifest visually? (Fire? Speed? Screen effects? Arena corruption?)
12. SCREEN PRESENCE — Scale 1-10 for how much the boss should dominate the screen visually
13. Any REFERENCE BOSSES from other games/mods that inspire this fight?

## Step 2: Survey Existing Boss Infrastructure

**MANDATORY reads before designing:**

### Existing Systems
- `Common/Systems/VFX/Boss/BossAttackVFXHelper.cs` — Reusable attack patterns: `RadialBurst()`, `DashTrail()`, `SlamImpact()`, `BeamSweep()`, `SpiralBarrage()`, `SummonCircle()`, `PhaseTransitionFlash()`
- `Common/Systems/VFX/Boss/BossSignatureVFX.cs` — Per-boss signature effects (30+ examples)
- `Common/Systems/VFX/Boss/BossShaderEffects.cs` — Shader-driven boss rendering utilities
- `Common/Systems/VFX/Boss/BossArenaVFX.cs` — Arena ambient effects (floating particles with parallax)
- `Common/Systems/VFX/Boss/TelegraphSystem.cs` — 5 telegraph types: `ThreatLine`, `DangerZone`, `ConvergingRing`, `ImpactPoint`, `SectorCone`
- `Common/Systems/VFX/Boss/BossVFXOptimizer.cs` — Frame-skipped particle rendering
- `Common/Systems/Bosses/BossShaderManager.cs` — Central shader loading + parameter application
- `Common/Systems/Bosses/BossRenderHelper.cs` — `DrawShaderAura()`, `DrawShaderTrail()`, `DrawPhaseTransition()`, `DrawDissolve()`
- `Common/Systems/Bosses/BossSkyRegistrationSystem.cs` — How CustomSky implementations are registered

### Architecture Patterns

| Pattern | What It Is | Where |
|---------|-----------|-------|
| **Static AttackVFX class** | Each boss has a static class with per-attack methods (`SwordDashTelegraph()`, `SwordDashImpact()`, etc.) | `Content/*/Bosses/Systems/*AttackVFX.cs` |
| **Static BossShaderSystem class** | Shader binding per phase — `DrawValorAura()`, `DrawBossGlow()` called from `PreDraw`/`PostDraw` | `Content/*/Bosses/Systems/*BossShaderSystem.cs` |
| **CustomSky inheritance** | Extends `CustomSky` (not `ModSky`), registered via `BossSkyRegistrationSystem`, activated with `SkyManager.Instance.Activate()` | `Content/*/Bosses/Systems/*Sky.cs` |
| **SpriteBatch shader cycle** | End → Begin(Immediate, ShaderAdditive) → Apply → Draw → End → Begin(Deferred, AlphaBlend) | Every shader rendering call |
| **Multi-phase attack enum** | Boss AI uses attack enum with phase-gated subsets, VFX scales with `lifeRatio` / `difficultyTier` | Main boss `.cs` file |

### Read One Complete Boss Example
Pick the closest existing boss to the new design and read ALL its files:
- Main NPC class: `Content/<Theme>/Bosses/<Boss>.cs`
- Attack VFX: `Content/<Theme>/Bosses/Systems/<Theme>AttackVFX.cs`
- Shader system: `Content/<Theme>/Bosses/Systems/<Theme>BossShaderSystem.cs`
- Sky: `Content/<Theme>/Bosses/Systems/<Theme>Sky.cs`

**Only proceed after you've read and understood these patterns.**

## Step 3: Establish Musical Structure

Map boss phases to musical structure:

| Phase | Musical Analog | Intensity | VFX Budget |
|-------|---------------|-----------|------------|
| Phase 1 | Exposition / Opening theme | Medium | Standard attacks 3-4 layers |
| Phase 2 | Development / Building tension | High | Attacks 4-5 layers, signature unlocks |
| Phase 3 | Recapitulation / Climax | Maximum | Attacks 5-7 layers, screen effects |
| Enrage | Coda / Finale | Overwhelming | All layers active, arena transformed |

Each phase should feel like a distinct movement of the musical piece.

## Step 4: Design Arena Background Per Phase

### CustomSky Implementation Pattern
```csharp
public class <Theme>Sky : CustomSky
{
    public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
    {
        // Phase-aware background rendering
        // Ambient particle spawning
        // Atmospheric effects (fog, stars, embers, etc.)
    }

    public override Color OnTileColor(Color inColor)
    {
        // Tint world lighting per phase
        return Color.Lerp(inColor, themeColor, tintIntensity);
    }
}
```

Design per-phase backgrounds:
- **Phase 1:** Subtle atmosphere change (sky tint, ambient particles)
- **Phase 2:** Full arena transformation (custom background, themed particles dense)
- **Phase 3:** Intense atmosphere (screen effects, background reacting to attacks)
- **Enrage:** Arena itself feels hostile (corruption spreading, colors intensifying)

For each phase specify: background type, primary colors (from theme palette), ambient particle type/density/behavior, lighting mood, and special atmospheric effects (fog, rain, heat haze, cosmic dust).

## Step 5: Design Attack Telegraphs

Every boss attack needs a telegraph before firing:

| Attack Type | Telegraph Style | Duration |
|-------------|----------------|----------|
| Fast standard | Brief color flash on boss | 0.2-0.3s |
| Medium attack | Glowing charge + particle convergence | 0.5-0.8s |
| Heavy attack | Full windup animation + screen tint + particles | 0.8-1.2s |
| Signature | Cinematic telegraph (letterbox + zoom + buildup) | 1.0-2.0s |
| Ultimate | Full screen anticipation (desaturation + convergence + pause) | 1.5-3.0s |

Telegraphs use: convergent particles, growing glow/aura, screen tint shift, subtle screen shake buildup, themed warning indicators.

### Telegraph Shader References
- WotG `DeadSunTelegraphBeamShader.fx` — Beam preview
- WotG `SunLaserTelegraphShader.fx` — Laser indicator
- MagnumOpus `TelegraphSystem.cs` — Existing 5-type infrastructure

## Step 6: Design Signature Attacks (Layered VFX)

### Static AttackVFX Pattern
```csharp
public static class <Theme>AttackVFX
{
    public static void DrawSignatureAttack(SpriteBatch spriteBatch, NPC boss, float progress)
    {
        // Layer 1: Core projectile/beam rendering
        // Layer 2: Trail/ribbon behind attack
        // Layer 3: Bloom stacking (3-scale)
        // Layer 4: Particle burst/shower
        // Layer 5: Screen shake + flash
        // Layer 6: Themed accent particles (notes, feathers, embers, etc.)
        // Layer 7: Screen distortion (for signatures only)
    }
}
```

For each signature attack, design:
- Core visual (what IS the attack — beam, projectile barrage, melee slam, area denial?)
- Trail/wake (what it leaves behind)
- Impact (what happens when it connects)
- Screen response (shake, flash, aberration — proportional to damage)
- Musical element (how the attack sounds/feels musical)

### Attack Variety Within a Phase

No two attacks in the same phase should look the same:
- **Attack A**: Beam sweep (continuous laser visual)
- **Attack B**: Projectile barrage (multiple scattered particles)
- **Attack C**: Ground slam (radial shockwave + debris)
- **Attack D**: Summon (magic circle + minion spawn)

## Step 7: Design Phase Transitions

Three intensity levels:

### Standard Transition (phase 1→2)
- Brief invulnerability (0.5-1s)
- Particle burst from boss center
- Color palette shift
- Background transition begins
- Screen shake (medium)

### Major Transition (phase 2→3)
- Longer invulnerability (1-2s)
- Screen flash + chromatic aberration pulse
- Full background swap
- Boss aura changes/intensifies
- Dramatic particle explosion
- Brief letterboxing for cinematic feel

### Ultimate Transition / Enrage
- Reality-level visual disruption
- Screen tear / distortion wave
- Background completely transforms
- All ambient particles intensify 2-3x
- Boss gains visible aura + afterimage trail
- Camera zoom punch effect
- Brief desaturation → color explosion

### Transition Checklist
- [ ] Player clearly understands "the boss just changed"
- [ ] Transition takes 1-3 seconds (not too fast, not dragging)
- [ ] New phase's visual baseline is established immediately after
- [ ] Transition VFX uses the NEW phase's palette, not the old one

## Step 8: Design Enrage / Finale VFX

Enrage must be VISUALLY UNMISTAKABLE:

- [ ] Ambient particle density increases 2-3x
- [ ] Background shader intensifies (more saturated, faster animation)
- [ ] Boss gains visible aura (noise-masked, pulsing)
- [ ] Attack VFX gains additional layers
- [ ] Color palette shifts hotter/more intense
- [ ] New visual elements appear (afterimages, screen effects)
- [ ] Boss trail appears or intensifies
- [ ] Attack telegraphs become shorter but more dramatic

## Step 9: Performance Budget

| Element | Phase 1 | Phase 2 | Phase 3 | Enrage |
|---------|---------|---------|---------|--------|
| Ambient particles | 50-100 | 100-200 | 200-400 | 400-800 |
| Attack particles | 20-50/atk | 50-100/atk | 100-200/atk | 200-400/atk |
| Active trails | 1-2 | 2-3 | 3-5 | 5-8 |
| Screen effects | 0-1 | 1 | 1-2 | 2-3 |
| Bloom layers | 2 | 3 | 3 | 3 |

**Hard limits:** Max 1500 total particles during enrage. Render targets pooled. UI exclusion zones on all screen distortion.

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

## Step 10: Design Screen Effects & Cinematic Moments

### Boss Intro Sequence
- Spawn animation (ascending from ground? Descending from sky? Materializing?)
- Brief cinematic (letterboxing, camera focus, name reveal)
- Arena transition (background shifts to boss arena)

### Mid-Fight Cinematics (phase transitions)
- Use AttackAnimationFoundation patterns for timed VFX sequences
- Camera zoom + desaturation + focus on boss during transformation
- Duration: 1-3 seconds maximum (don't interrupt gameplay too long)

### Death Sequence
- Final dramatic explosion (boss death = 20px screen shake exception)
- Themed dissolution (dissolve shader, particle scatter, light burst)
- Arena gradually returns to normal
- Loot drop with themed particle accent

### Screen Effect Reference
- Screen distortion: heat haze around boss, expanding shockwaves on impacts
- Chromatic aberration: phase transitions, heavy hits, signature attacks
- Vignette: per-theme color, intensity increases with phase
- Screen shake: proportional to attack weight (see CLAUDE.md safety limits)
- Flash: max 70% opacity, max 0.15s duration

### Lighting & Atmosphere Design
For each phase, define:
- **Arena lighting**: Dynamic light sources, color, pulsing patterns
- **Boss aura**: Glow color, radius, noise erosion, per-phase evolution
- **God rays / fog**: If applicable, when they activate and fade
- **Color grading**: Per-phase LUT or tint adjustments

## Step 11: Document the Design

Produce a complete boss VFX specification:

```
== BOSS VFX DESIGN: [BossName] ==

Theme: [Theme]
Musical Structure: [Form]
Phases: [N]

ARENA:
  Phase 1: [description + sky behavior]
  Phase 2: [description + transition]
  Phase 3: [description + intensity]
  Enrage: [description + transformation]

ATTACKS:
  [Per-attack breakdown with layer count and VFX description]

PHASE TRANSITIONS:
  1→2: [description + timing + effects]
  2→3: [description + timing + effects]
  →Enrage: [description + timing + effects]

SIGNATURE MOMENTS:
  [The 2-3 most visually spectacular moments]

CINEMATICS:
  Intro: [description]
  Death: [description]

PERFORMANCE BUDGET:
  [Per-phase particle/effect limits]

FILES TO CREATE:
  Content/<Theme>/Bosses/<Boss>.cs
  Content/<Theme>/Bosses/Systems/<Theme>AttackVFX.cs
  Content/<Theme>/Bosses/Systems/<Theme>BossShaderSystem.cs
  Content/<Theme>/Bosses/Systems/<Theme>Sky.cs
  Effects/<Theme>/Boss/[shaders needed]

ASSET REQUIREMENTS:
  [List of needed textures/sprites with status: exists / needs creation]
```

## Step 12: Implementation Order

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

Run `dotnet build` after each step to verify compilation.
