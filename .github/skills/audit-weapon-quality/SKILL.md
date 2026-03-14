---
description: "Audit a weapon or boss's VFX quality against reference mod standards — evaluates layer count, theme consistency, edge quality, motion, musical identity, and uniqueness. Use when reviewing existing weapons, bosses, or checking implementation quality."
---

# Audit Weapon Quality Workflow

This skill evaluates an existing weapon's VFX against the quality standards set by reference mods (Calamity, Wrath of the Gods, Everglow) and MagnumOpus's own musical identity requirements. Includes creativity scoring, attack pattern diversity assessment, and asset reuse creativity audit.

## Step 1: Read the Full Implementation

Gather all files for the weapon:
- Item class (`.cs` file in `Content/<Theme>/<Category>/<Weapon>/`)
- Projectile/swing class
- VFX helper class (if exists)
- Custom ModDust types (in `Dusts/` subfolder)
- Any weapon-specific systems (in `Systems/` subfolder)
- Associated shaders (in `Effects/<Theme>/<Weapon>/`)

## Step 2: Layer Count Assessment

Count the distinct visual layers the weapon produces:

| Rating | Layers | Verdict |
|--------|--------|---------|
| **Insufficient** | 1 | Single-pass flat rendering. Needs major VFX overhaul. |
| **Bare Minimum** | 2 | Body + one overlay. Acceptable for minor weapons only. |
| **Standard** | 3-4 | Core + glow + accents. Acceptable for most weapons. |
| **High Quality** | 5-6 | Multi-layered composition. Expected for theme flagships. |
| **Exceptional** | 7+ | Full VFX suite. Boss weapons, endgame gear. |

**What counts as a layer:**
- Trail rendering (primitive mesh, afterimage, ribbon)
- Bloom/glow (multi-scale additive stacking or shader bloom)
- Particle effects (dust motes, sparks, themed particles)
- Screen effects (shake, flash, distortion)
- Shader overlays (aura, smear, color grading)
- Impact effects (burst, slash mark, shockwave)
- Special mechanic visuals (charge indicator, combo counter, orbital)

## Step 3: Theme Palette Consistency

Check all color values used in the weapon against the theme's established palette:

| Theme | Required Palette |
|-------|-----------------|
| Moonlight Sonata | Deep dark purples, vibrant light blues, violet, ice blue |
| Eroica | Scarlet, crimson, gold, sakura pink |
| La Campanella | Black smoke, orange flames, gold highlights |
| Swan Lake | Pure white, black contrast, prismatic rainbow edges |
| Enigma Variations | Void black, deep purple, eerie green flame |
| Fate | Black void, dark pink, bright crimson, celestial white |
| Clair de Lune | Night mist blue, soft blue, pearl white |
| Dies Irae | Blood red, dark crimson, ember orange |
| Nachtmusik | Deep indigo, starlight silver, cosmic blue |
| Ode to Joy | Warm gold, radiant amber, jubilant light |

**Check for:**
- Colors outside the theme palette (cross-theme contamination)
- Hardcoded Color values that don't match theme
- Color ramp/LUT usage — is it using the correct theme gradient from `Assets/VFX Asset Library/ColorGradients/`?

## Step 4: Edge Quality Check

Examine how effects terminate at their boundaries:

**Good edges:**
- `smoothstep()` fading in shaders
- Alpha falloff on trail tips and edges
- Mask textures shaping boundaries
- Feathered sprite edges
- SDF soft boundaries

**Bad edges (flag these):**
- Hard texture cutoffs (rectangular sprite visible)
- Trails that end abruptly without taper
- Bloom circles with visible hard circles
- Particles that pop in/out without fade

## Step 5: Motion & Life Check

Every visual element should have some form of animation:

**Check for:**
- UV scrolling on trails/beams (internal texture movement)
- Pulsing/oscillation on bloom/glow (breathing effect)
- Particle rotation/drift (not frozen in place)
- Color shift over time or lifetime (temperature fade, palette cycling)
- Scale animation (growth, decay, breathing)

**Flag as issues:**
- Static bloom overlays (flat circles that don't change)
- Trails without internal UV movement (painted stripes)
- Particles that spawn and just fade without moving
- Color that never changes frame-to-frame

## Step 6: Reference Comparison

Compare the weapon's visual quality against equivalent weapons in reference mods:

### Calamity Equivalents
- **Melee**: Compare against Exoblade, Celestus, Ark of the Cosmos, Galaxia
- **Magic**: Compare against Subsuming Vortex, Vivid Clarity
- **Ranged**: Compare against Photoviscerator, Magnomaly Cannon, Heavenly Gale
- **Summoner**: Compare against Cosmic Immaterializer

### Questions to Ask
- Does this weapon have comparable visual complexity to reference weapons?
- Does the trail rendering quality match Calamity's smooth interpolation?
- Are particle effects as choreographed and varied as Calamity's?
- Are shader effects as polished as WotG's implementations?

## Step 7: Musical Identity Check

MagnumOpus is a music mod. Assess musical integration:

**Present (good):**
- Music note particles scattered from blade tips or impacts
- Harmonic pulse effects tied to attack rhythm
- Standing wave patterns in trail shaders
- Frequency-based color oscillation
- Rhythmic timing in particle spawns

**Absent (flag for improvement):**
- No musical visual elements at all
- Generic energy/fire effects that could belong to any mod
- No rhythmic or harmonic timing patterns

## Step 8: Uniqueness Check

Search for other weapons in the same theme:
```
Content/<ThemeName>/
```

Compare this weapon against every other weapon of the same class in the same theme:

**Unique (good):** Different trail type, different particle effects, different special mechanic, different shader approach
**Overlapping (flag):** Same trail type with different color, same particle effects, same special mechanic pattern

## Step 9: Creativity Scoring (1-10)

Score the weapon's creative innovation across these dimensions:

| Dimension | 1-3 (Generic) | 4-6 (Adequate) | 7-9 (Creative) | 10 (Exceptional) |
|-----------|---------------|----------------|----------------|------------------|
| **Attack Pattern** | Vanilla copy | Minor twist | Unique mechanic | Never-seen-before |
| **Trail VFX** | Default/none | Colored standard | Multi-layer/shader | Trail IS the identity |
| **Particle Work** | Basic dust | Themed particles | Choreographed bursts | Musical storytelling |
| **Impact Effects** | Single flash | Multi-layer | Screen-level response | Cinematic moments |
| **Asset Reuse Creativity** | Uses 1 asset 1 way | Uses assets normally | Creative combinations | 1 asset → 5+ effects |
| **Musical Integration** | None | Color palette only | Notes/rhythm visible | Music IS the effect |

**Overall Creativity Score** = average of all 6 dimensions.

**Scoring thresholds:**
- Below 4: Needs complete VFX overhaul
- 4-6: Acceptable but underperforming the mod's vision
- 7-8: Strong, creative implementation
- 9-10: Flagship quality, sets the standard

## Step 10: Attack Pattern Diversity Check

For weapons with multiple attack modes/combos:
- Does each combo phase feel visually DIFFERENT? (Not just speed changes)
- Does each phase use different VFX techniques? (Trail vs particles vs smear vs shader)
- Are there escalating visuals across combo steps? (More intensity, new elements appearing)
- Is there a distinct FINISHER that feels like a musical climax?

For same-class weapons within the theme:
- List every weapon's PRIMARY attack pattern
- Identify any overlap in attack type (two swords both doing the same swing arc)
- Flag weapons that could be confused for each other during gameplay

## Step 11: Asset Reuse Creativity Audit

Check how creatively existing assets are used:
- Are bloom sprites used at different scales/tints/contexts? (Creative reuse = good)
- Are noise textures combined with different color ramps for variety?
- Are particle sprites choreographed differently per usage context?
- Could existing assets produce MORE varied effects with creative application?

**Flag as underperforming:**
- Assets used once in one way (each texture appears in exactly one spot)
- Identical bloom stacking across multiple weapons (same scale/tint/layer count)
- Particle sprites used only for their "obvious" purpose (4PointedStar only as star)

## Step 12: Reference Mod Quality Bar Comparison

Compare against the quality standard set by reference mods:
- Read 1-2 equivalent weapons from Calamity/WotG at the same progression tier
- Score MagnumOpus weapon vs reference on: visual depth, technique diversity, polish, impact
- Identify specific techniques the reference mod uses that MagnumOpus weapon is missing

## Step 13: Technical Check

- **Blend modes**: Glow effects use `BlendState.Additive`? Smoke uses `BlendState.AlphaBlend`?
- **SpriteBatch state**: Proper Begin/End management? No nested SpriteBatch issues?
- **Texture loading**: All textures loaded correctly? No `ContentNotFound` risks?
- **ModifyTooltips**: Does the item have proper tooltips with effect descriptions + lore line?
- **Asset existence**: Do all referenced texture paths actually exist?

## Step 14: Generate Report

Produce a structured quality report:

```
== WEAPON QUALITY AUDIT: [WeaponName] ==

Theme: [Theme]
Class: [Melee/Magic/Ranged/Summoner]

LAYER COUNT: [N] layers — [Rating]
  ✓ [Layer 1 description]
  ✓ [Layer 2 description]
  ✗ Missing: [Suggested additional layer]

THEME PALETTE: [Pass/Fail]
  [Details of any off-palette colors]

EDGE QUALITY: [Pass/Fail]
  [Details of any hard cutoffs found]

MOTION/LIFE: [Pass/Fail]
  [Details of any static elements]

MUSICAL IDENTITY: [Strong/Present/Weak/Absent]
  [Details of musical elements found or missing]

UNIQUENESS: [Unique/Overlapping]
  [Comparison with same-theme weapons]

CREATIVITY SCORE: [N/10]
  Attack Pattern: [N/10]
  Trail VFX: [N/10]
  Particle Work: [N/10]
  Impact Effects: [N/10]
  Asset Reuse: [N/10]
  Musical Integration: [N/10]

ATTACK DIVERSITY: [Pass/Needs Work]
  [Details of combo variety and same-class overlap]

REFERENCE MOD COMPARISON: [Below/Meets/Exceeds] reference standard
  [Specific techniques missing vs reference]

TECHNICAL: [Pass/Fail]
  [Any technical issues found]

RECOMMENDATIONS:
1. [Highest priority improvement]
2. [Second priority]
3. [Third priority]

REFERENCE QUALITY: [Below/Meets/Exceeds] Calamity equivalent
```

---

## Boss Audit Mode

When auditing a **boss encounter** instead of a weapon, use Steps 1-10 above for any boss-dropped weapons, then perform these additional boss-specific checks:

### Boss Step A: Read All Boss Files

Gather the complete boss file set:
- Main boss NPC class (`Content/<Theme>/Bosses/<BossName>.cs`)
- Attack VFX helper (`Content/<Theme>/Bosses/Systems/<Theme>AttackVFX.cs`)
- Boss shader system (`Content/<Theme>/Bosses/Systems/<Theme>BossShaderSystem.cs`)
- Sky effect (`Content/<Theme>/Bosses/Systems/<Theme>Sky.cs`)
- Any boss projectile files

### Boss Step B: Arena Atmosphere Assessment

| Rating | Arena Quality | Verdict |
|--------|-------------|---------|
| **Absent** | No custom sky, default Terraria background | Needs complete arena implementation |
| **Minimal** | Color tint only, no custom sky class | Needs sky shader + ambient particles |
| **Standard** | CustomSky with basic gradient + some particles | Acceptable for mid-game bosses |
| **High Quality** | Per-phase sky transitions, atmospheric particles, lighting tint | Expected for theme bosses |
| **Exceptional** | Procedural sky shaders, phase-aware backgrounds, full atmosphere | Endgame / flagship bosses |

**Check for:**
- Does the CustomSky respond to boss HP / phase changes?
- Does `OnTileColor()` tint world lighting appropriately?
- Are ambient particles present and themed?
- Does the atmosphere escalate across phases?

### Boss Step C: Attack VFX Layering

For each boss attack, count VFX layers:
- **Standard attacks** should have **3-4 layers** (telegraph + projectile + trail + impact)
- **Signature attacks** should have **5-7 layers** (add screen effects, burst particles, bloom)
- **Ultimate / phase finisher** should have **7+ layers** (add cinematics, reality effects)

**Flag:** Any attack with fewer than 3 layers. Any signature attack without screen effects.

### Boss Step D: Phase Transition Quality

| Rating | Transition Quality |
|--------|-------------------|
| **Absent** | No visual indication of phase change |
| **Minimal** | Brief flash or particle burst only |
| **Standard** | Screen effect + particle burst + palette shift |
| **High Quality** | Full background swap + chromatic aberration + cinematic moment |
| **Exceptional** | Reality punch + screen tear + letterboxing + dramatic pause |

### Boss Step E: Enrage Escalation

Check that enrage is visually distinct from normal phases:
- [ ] Ambient particle density increases (2-3x)
- [ ] Background shader intensifies
- [ ] Boss aura/glow becomes visible or intensifies
- [ ] Attack VFX gains additional layers
- [ ] Color palette shifts hotter/more intense
- [ ] New visual elements appear (afterimages, screen effects, etc.)

### Boss Step F: Performance Check

- [ ] Phase transitions don't stutter
- [ ] Enrage particle count stays within budget (800-1500 max)
- [ ] Full-screen shaders have exclusion zones for UI readability
- [ ] Render targets are pooled, not allocated per-frame

### Boss Audit Report Template

```
== BOSS VFX AUDIT: [BossName] ==

Theme: [Theme]
Phases: [N] phases

ARENA ATMOSPHERE: [Rating]
  Phase 1: [description]
  Phase 2: [description]
  Enrage:  [description]

ATTACK VFX:
  Standard attacks: [N] layers avg — [Rating]
  Signature attacks: [N] layers avg — [Rating]
  Ultimate: [N] layers — [Rating]

PHASE TRANSITIONS: [Rating]
  1→2: [description]
  2→3: [description]
  →Enrage: [description]

ENRAGE ESCALATION: [N/6 checks passed]
  [Details]

MUSICAL IDENTITY: [Strong/Present/Weak/Absent]
  [Does the fight feel like its musical theme?]

PERFORMANCE: [Pass/Fail]
  [Any frame drops, missing exclusion zones, etc.]

RECOMMENDATIONS:
1. [Highest priority]
2. [Second priority]
3. [Third priority]

REFERENCE QUALITY: [Below/Meets/Exceeds] WotG/Calamity boss equivalent
```
