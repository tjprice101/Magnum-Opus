---
description: "Audit creative diversity across a theme's weapons — score uniqueness, identify samey effects, suggest creative enhancements, generate before/after proposals."
---

# /audit-vfx-creativity — Creative Diversity Audit Workflow

Audit how creatively diverse a theme's weapons are. Score attack pattern variety, VFX technique diversity, and visual uniqueness. Identify weapons that feel samey and propose creative enhancements.

## When to Use This Skill
- After creating multiple weapons for a theme — check if they're distinct enough
- Before starting new weapons — understand what already exists
- Quality review of a theme's complete weapon roster
- Finding weapons that need VFX upgrades

## Workflow Steps

### Step 1: Select Audit Scope
Ask the user:
1. Which THEME to audit? (Or "all themes" for a cross-theme survey)
2. Audit depth: QUICK (technique comparison only) or DEEP (full creative scoring)?
3. Focus area: ALL VFX aspects, or specific domain (trails, impacts, particles, mechanics)?

### Step 2: Read All Weapons in Scope
For the selected theme, read every weapon file:
- `Content/<Theme>/Melee/` — All melee weapons
- `Content/<Theme>/Weapons/` — All weapons
- `Content/<Theme>/Magic/` — Magic weapons
- `Content/<Theme>/Ranged/` — Ranged weapons
- `Content/<Theme>/Summoner/` — Summoner weapons

For each weapon, catalog:
- Primary attack pattern (combo phases, single swing, hold, channel)
- Trail type (shader trail, primitive, particle trail, none)
- Particle effects (dust types, bloom layers, music notes)
- Impact effects (layers, intensity, screen effects)
- Special mechanic (heat, charge, combo, mode switch)
- Lighting/atmosphere effects
- Screen effects used

### Step 3: Score Creative Diversity

**Per-Weapon Scores (1-10):**

| Dimension | 1-3 (Generic) | 4-6 (Adequate) | 7-9 (Creative) | 10 (Exceptional) |
|-----------|---------------|----------------|----------------|------------------|
| **Attack Pattern** | Same as vanilla | Minor twist on standard | Unique combo/mechanic | Never seen before |
| **Trail VFX** | Default trail or none | Colored trail | Layered/shader trail | Trail IS the weapon identity |
| **Particle Work** | Basic dust | Themed dust + bloom | Choreographed bursts | Musical particle storytelling |
| **Impact Effects** | Single flash | Flash + particles | Multi-layer + screen effects | Cinematic hit moments |
| **Uniqueness** | Could be any mod | Fits theme colors | Unique within theme | Instantly recognizable |
| **Musical Identity** | No musical elements | Color palette matches | Notes/rhythm visible | Music IS the effect |

**Overall Creativity Score** = average of all dimensions.

### Step 4: Identify "Samey" Pairs
Compare every weapon pair within the theme:
- Do any two weapons share the SAME trail technique?
- Do any two weapons share the SAME impact pattern?
- Do any two weapons share the SAME special mechanic?
- Do any two weapons have indistinguishable visual identity when viewed side-by-side?

Flag any pair scoring >70% similarity as "needs differentiation."

### Step 5: Reference Comparison
Compare theme weapons against the quality bar set by reference mods:
- How does the most creative weapon compare to a Calamity signature weapon?
- Are particle effects as choreographed as WotG particle systems?
- Are shaders as polished as Everglow's paired dust types?

### Step 6: Generate Enhancement Proposals
For each weapon scoring below 7/10 overall or flagged as "samey":

Present BEFORE → AFTER proposals:
> **Weapon: Moonlit Requiem (Score: 4.5/10)**
> - BEFORE: Simple purple trail, basic dust on hit, no special mechanic
> - AFTER Proposal A: Trail shifts between 3 purple hues based on combo phase. Impact spawns cascading music notes that drift upward like moonlit snowflakes. Special: every 3rd swing creates a moonbeam that lingers as a damage zone.
> - AFTER Proposal B: No visible trail — instead, blade leaves constellation dot pattern. Impact creates silver ripple ring (SDF shader). Special: holding attack charges moonlight, release creates expanding silver crescent wave.

### Step 7: Creative Diversity Report
Produce final report:
- **Theme Score Card:** Per-weapon scores in table format
- **Strongest Weapons:** Best VFX in the theme (keep as-is)
- **Needs Work:** Weapons below quality bar with enhancement proposals
- **Missing Techniques:** VFX techniques NOT used by any weapon in the theme (opportunities)
- **Recommendations:** Priority order for enhancement work

### Step 8: Implementation Routing (if user approves enhancements)
For each approved enhancement:
- Route to @creative-director for concept refinement
- Route to appropriate domain agents for implementation
- Tag with asset requirements
