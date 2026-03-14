Audit a weapon or boss's VFX quality against reference mod standards. Evaluate layer count, theme consistency, edge quality, motion, musical identity, uniqueness, and creativity.

Target to audit: $ARGUMENTS

---

## Step 1: Read the Full Implementation

Gather all files for the weapon:
- Item class (`.cs` file in `Content/<Theme>/<Category>/<Weapon>/`)
- Projectile/swing class
- VFX helper class (if exists)
- Custom ModDust types (in `Dusts/` subfolder)
- Weapon-specific systems (in `Systems/` subfolder)
- Associated shaders (in `Effects/<Theme>/<Weapon>/`)

## Step 2: Layer Count Assessment

Count distinct visual layers:

| Rating | Layers | Verdict |
|--------|--------|---------|
| **Insufficient** | 1 | Single-pass flat. Needs major overhaul. |
| **Bare Minimum** | 2 | Body + one overlay. Minor weapons only. |
| **Standard** | 3-4 | Core + glow + accents. Most weapons. |
| **High Quality** | 5-6 | Multi-layered. Expected for flagships. |
| **Exceptional** | 7+ | Full VFX suite. Boss weapons, endgame. |

**What counts as a layer:** Trail rendering, bloom/glow, particle effects, screen effects, shader overlays, impact effects, special mechanic visuals.

## Step 3: Theme Palette Consistency

Check all colors against the theme's palette:

| Theme | Required Palette |
|-------|-----------------|
| Moonlight Sonata | Deep purples, vibrant blues, violet, ice blue |
| Eroica | Scarlet, crimson, gold, sakura pink |
| La Campanella | Black smoke, orange flames, gold highlights |
| Swan Lake | Pure white, black contrast, prismatic rainbow edges |
| Enigma Variations | Void black, deep purple, eerie green flame |
| Fate | Black void, dark pink, bright crimson, celestial white |
| Clair de Lune | Night mist blue, soft blue, pearl white |
| Dies Irae | Blood red, dark crimson, ember orange |
| Nachtmusik | Deep indigo, starlight silver, cosmic blue |
| Ode to Joy | Warm gold, radiant amber, jubilant light |

Flag: colors outside palette, hardcoded off-theme values, missing theme LUT usage.

## Step 4: Edge Quality Check

**Good edges:** `smoothstep()` fading, alpha falloff on trail tips, mask textures, feathered sprites, SDF soft boundaries.

**Bad edges (flag):** Hard texture cutoffs, trails ending abruptly without taper, visible hard bloom circles, particles that pop in/out without fade.

## Step 5: Motion & Life Check

Every visual element should animate:
- UV scrolling on trails/beams
- Pulsing/oscillation on bloom/glow
- Particle rotation/drift
- Color shift over lifetime
- Scale animation (growth, decay, breathing)

**Flag:** Static bloom overlays, trails without UV movement, particles that spawn and just fade without moving, colors that never change.

## Step 6: Reference Comparison

Compare against equivalent Calamity weapons:
- **Melee:** Exoblade, Celestus, Ark of the Cosmos, Galaxia
- **Magic:** Subsuming Vortex, Vivid Clarity
- **Ranged:** Photoviscerator, Magnomaly Cannon, Heavenly Gale
- **Summoner:** Cosmic Immaterializer

Questions: Comparable visual complexity? Trail quality matches Calamity's smooth interpolation? Particle effects as varied? Shader effects as polished as WotG?

## Step 7: Musical Identity Check

**Present (good):** Music note particles, harmonic pulse effects, standing wave patterns, frequency-based oscillation, rhythmic timing in spawns.

**Absent (flag):** No musical elements at all, generic energy/fire that could be any mod, no rhythmic patterns.

## Step 8: Uniqueness Check

Search `Content/<ThemeName>/` for other weapons of same class. Compare:
- **Unique:** Different trail type, particle effects, special mechanic, shader approach
- **Overlapping:** Same trail type with different color, same effects, same mechanic pattern

## Step 9: Creativity Scoring (1-10)

| Dimension | 1-3 (Generic) | 4-6 (Adequate) | 7-9 (Creative) | 10 (Exceptional) |
|-----------|---------------|----------------|----------------|------------------|
| **Attack Pattern** | Vanilla copy | Minor twist | Unique mechanic | Never-seen-before |
| **Trail VFX** | Default/none | Colored standard | Multi-layer/shader | Trail IS the identity |
| **Particle Work** | Basic dust | Themed particles | Choreographed bursts | Musical storytelling |
| **Impact Effects** | Single flash | Multi-layer | Screen-level response | Cinematic moments |
| **Asset Reuse** | 1 asset 1 way | Normal usage | Creative combos | 1 asset → 5+ effects |
| **Musical Integration** | None | Color palette only | Notes/rhythm visible | Music IS the effect |

**Overall = average of 6 dimensions.** Below 4 = needs overhaul. 4-6 = underperforming. 7-8 = strong. 9-10 = flagship.

## Step 10: Attack Pattern Diversity

For weapons with combos:
- Does each phase feel visually DIFFERENT?
- Different VFX techniques per phase?
- Escalating visuals across combo steps?
- Distinct FINISHER that feels like a climax?

For same-class weapons in theme:
- List every weapon's primary attack pattern
- Identify overlap
- Flag weapons that could be confused during gameplay

## Step 11: Asset Reuse Creativity

- Bloom sprites used at different scales/tints/contexts?
- Noise textures combined with different ramps?
- Particle sprites choreographed differently per context?
- Could existing assets produce MORE variety with creative application?

**Flag:** Assets used once in one way, identical bloom stacking across weapons, sprites used only for "obvious" purpose.

## Step 12: Reference Mod Quality Bar

Read 1-2 equivalent weapons from Calamity/WotG at the same tier. Score MagnumOpus weapon vs reference on: visual depth, technique diversity, polish, impact. Identify techniques the reference uses that MagnumOpus is missing.

## Step 13: Technical Check

- Blend modes correct? (Glow = Additive, smoke = AlphaBlend)
- SpriteBatch state properly managed? No nested issues?
- Textures loaded correctly? No `ContentNotFound` risks?
- ModifyTooltips implemented? Effect descriptions + lore line?
- All referenced texture paths exist?

## Step 14: Generate Report

```
== WEAPON QUALITY AUDIT: [WeaponName] ==

Theme: [Theme]
Class: [Melee/Magic/Ranged/Summoner]

LAYER COUNT: [N] layers — [Rating]
  ✓ [Layer descriptions]
  ✗ Missing: [Suggested layers]

THEME PALETTE: [Pass/Fail]
  [Details]

EDGE QUALITY: [Pass/Fail]
  [Details]

MOTION/LIFE: [Pass/Fail]
  [Details]

MUSICAL IDENTITY: [Strong/Present/Weak/Absent]
  [Details]

UNIQUENESS: [Unique/Overlapping]
  [Comparison]

CREATIVITY SCORE: [N/10]
  Attack Pattern: [N/10]
  Trail VFX: [N/10]
  Particle Work: [N/10]
  Impact Effects: [N/10]
  Asset Reuse: [N/10]
  Musical Integration: [N/10]

ATTACK DIVERSITY: [Pass/Needs Work]
  [Details]

REFERENCE COMPARISON: [Below/Meets/Exceeds]
  [Specific missing techniques]

TECHNICAL: [Pass/Fail]
  [Issues]

RECOMMENDATIONS:
1. [Highest priority]
2. [Second priority]
3. [Third priority]

REFERENCE QUALITY: [Below/Meets/Exceeds] Calamity equivalent
```

---

## Boss Audit Mode

When auditing a **boss** instead of a weapon, use Steps 1-14 above for any boss-dropped weapons, then perform these additional checks:

### Boss Step A: Read All Boss Files
- Main NPC class (`Content/<Theme>/Bosses/<Boss>.cs`)
- Attack VFX helper (`Content/<Theme>/Bosses/Systems/<Theme>AttackVFX.cs`)
- Boss shader system (`Content/<Theme>/Bosses/Systems/<Theme>BossShaderSystem.cs`)
- Sky effect (`Content/<Theme>/Bosses/Systems/<Theme>Sky.cs`)
- Boss projectile files

### Boss Step B: Arena Atmosphere

| Rating | Arena Quality | Verdict |
|--------|-------------|---------|
| **Absent** | No custom sky, default Terraria background | Needs complete arena implementation |
| **Minimal** | Color tint only, no custom sky class | Needs sky shader + ambient particles |
| **Standard** | CustomSky with gradient + some particles | Acceptable for mid-game bosses |
| **High Quality** | Per-phase transitions, atmospheric particles, lighting tint | Expected for theme bosses |
| **Exceptional** | Procedural sky shaders, phase-aware backgrounds, full atmosphere | Endgame / flagship bosses |

Check: Does sky respond to HP/phase? Does `OnTileColor()` tint appropriately? Ambient particles themed? Atmosphere escalates?

### Boss Step C: Attack VFX Layering
- Standard attacks: 3-4 layers (telegraph + projectile + trail + impact)
- Signature attacks: 5-7 layers (add screen effects, burst particles, bloom)
- Ultimate/finisher: 7+ layers (add cinematics, reality effects)

Flag any attack with fewer than 3 layers. Any signature without screen effects.

### Boss Step D: Phase Transitions

| Rating | Quality |
|--------|---------|
| **Absent** | No visual phase change |
| **Minimal** | Flash or particle burst only |
| **Standard** | Screen effect + burst + palette shift |
| **High Quality** | Full background swap + chromatic aberration + cinematic |
| **Exceptional** | Reality punch + screen tear + letterboxing + dramatic pause |

### Boss Step E: Enrage Escalation
- [ ] Particle density increases 2-3x
- [ ] Background shader intensifies
- [ ] Boss aura visible/intensifies
- [ ] Attack VFX gains layers
- [ ] Color palette shifts hotter/intense
- [ ] New visual elements appear

### Boss Step F: Performance
- [ ] Phase transitions don't stutter
- [ ] Enrage particles within budget (800-1500 max)
- [ ] Screen shaders have UI exclusion zones
- [ ] Render targets pooled, not per-frame

### Boss Audit Report

```
== BOSS VFX AUDIT: [BossName] ==

Theme: [Theme]
Phases: [N]

ARENA ATMOSPHERE: [Rating]
  Phase 1: [description]
  Phase 2: [description]
  Enrage: [description]

ATTACK VFX:
  Standard attacks: [N] layers avg — [Rating]
  Signature attacks: [N] layers avg — [Rating]
  Ultimate: [N] layers — [Rating]

PHASE TRANSITIONS: [Rating]
  [Per-transition details]

ENRAGE ESCALATION: [N/6 checks]

MUSICAL IDENTITY: [Strong/Present/Weak/Absent]

PERFORMANCE: [Pass/Fail]

RECOMMENDATIONS:
1-3 priorities

REFERENCE QUALITY: [Below/Meets/Exceeds] WotG/Calamity boss equivalent
```
