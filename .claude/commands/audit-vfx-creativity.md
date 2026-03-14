Audit creative diversity across a theme's weapons — score uniqueness, identify samey effects, suggest creative enhancements, and generate before/after proposals.

Target: $ARGUMENTS

---

## When to Use
- After creating multiple weapons for a theme — check if they're distinct enough
- Before starting new weapons — understand what already exists
- Quality review of a theme's complete weapon roster
- Finding weapons that need VFX upgrades

## Step 1: Select Audit Scope

Ask the user:
1. Which THEME to audit? (Or "all themes" for a cross-theme survey)
2. Audit depth: QUICK (technique comparison only) or DEEP (full creative scoring)?
3. Focus area: ALL VFX aspects, or specific domain (trails, impacts, particles, mechanics)?

## Step 2: Read All Weapons in Scope

For the selected theme, read every weapon file:
- `Content/<Theme>/Weapons/` — All weapons (primary location)
- `Content/<Theme>/Melee/` — Melee weapons (if separate)
- `Content/<Theme>/Magic/` — Magic weapons
- `Content/<Theme>/Ranged/` — Ranged weapons
- `Content/<Theme>/Summoner/` — Summoner weapons

For each weapon, catalog across 7 VFX dimensions:
1. **Primary attack pattern** (combo phases, single swing, hold, channel)
2. **Trail type** (shader trail, primitive, particle trail, afterimage, none)
3. **Particle effects** (dust types, bloom layers, music notes)
4. **Impact effects** (layers, intensity, screen effects)
5. **Special mechanic** (heat, charge, combo, mode switch)
6. **Lighting/atmosphere** (glow, aura, ambient particles)
7. **Screen effects** (shake, flash, distortion, chromatic)

## Step 3: Score Creative Diversity

### Per-Weapon Scores (1-10)

| Dimension | 1-3 (Generic) | 4-6 (Adequate) | 7-9 (Creative) | 10 (Exceptional) |
|-----------|---------------|----------------|----------------|------------------|
| **Attack Pattern** | Same as vanilla | Minor twist on standard | Unique combo/mechanic | Never seen before |
| **Trail VFX** | Default trail or none | Colored trail | Layered/shader trail | Trail IS the weapon identity |
| **Particle Work** | Basic dust | Themed dust + bloom | Choreographed bursts | Musical particle storytelling |
| **Impact Effects** | Single flash | Flash + particles | Multi-layer + screen effects | Cinematic hit moments |
| **Uniqueness** | Could be any mod | Fits theme colors | Unique within theme | Instantly recognizable |
| **Musical Identity** | No musical elements | Color palette matches | Notes/rhythm visible | Music IS the effect |

**Overall Creativity Score** = average of all 6 dimensions.

**Scoring thresholds:**
- Below 4: Needs complete VFX overhaul
- 4-6: Acceptable but underperforming the mod's vision
- 7-8: Strong, creative implementation
- 9-10: Flagship quality, sets the standard

## Step 4: Identify "Samey" Pairs

Compare every weapon pair within the theme:
- Do any two weapons share the SAME trail technique?
- Do any two weapons share the SAME impact pattern?
- Do any two weapons share the SAME special mechanic?
- Do any two weapons have indistinguishable visual identity when viewed side-by-side?

Flag any pair scoring >70% similarity as **"needs differentiation."**

### Overlap Detection Checklist
For each same-class pair (e.g., two melee swords):
- [ ] Different trail rendering approach?
- [ ] Different particle choreography?
- [ ] Different impact layers?
- [ ] Different special mechanic?
- [ ] Different screen-level effects (or lack thereof)?
- [ ] Could a player tell them apart at a glance?

## Step 5: Reference Comparison

Compare theme weapons against the quality bar set by reference mods:
- How does the MOST creative weapon compare to a Calamity signature weapon (Exoblade, Photoviscerator)?
- Are particle effects as choreographed as WotG particle systems?
- Are shaders as polished as Everglow's paired dust types?
- What techniques do reference mods use that NO weapon in this theme employs?

Search reference repos for specific comparison:
- **Calamity:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo`
- **WotG:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Wrath of the Gods Repo`
- **Everglow:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Everglow Repo`

## Step 6: Generate Enhancement Proposals

For each weapon scoring below 7/10 overall or flagged as "samey":

Present BEFORE → AFTER proposals with 2 creative options:

```
WEAPON: [Name] (Score: [N]/10)

BEFORE: [Current VFX description — what it does now]

AFTER Option A: [Creative enhancement proposal]
  - Trail: [new approach]
  - Particles: [new choreography]
  - Mechanic: [new or enhanced]
  - Musical: [how music identity improves]

AFTER Option B: [Alternative enhancement]
  - Trail: [different approach]
  - Particles: [different choreography]
  - Mechanic: [different enhancement]
  - Musical: [alternative music integration]
```

## Step 7: Creative Diversity Report

Produce final report:

```
== VFX CREATIVITY AUDIT: [Theme Name] ==

THEME SCORE CARD:
| Weapon | Class | Attack | Trail | Particles | Impact | Unique | Musical | Overall |
|--------|-------|--------|-------|-----------|--------|--------|---------|---------|
| [Name] | [Class] | [N] | [N] | [N] | [N] | [N] | [N] | [N/10] |
| ...    | ...     | ... | ... | ...       | ...    | ...    | ...     | ...     |

THEME AVERAGE: [N/10]

STRONGEST WEAPONS: (keep as-is)
  - [Name]: [What makes it great]

NEEDS WORK: (enhancement proposals above)
  - [Name]: [Primary issue + proposed fix]

SAMEY PAIRS: (need differentiation)
  - [Weapon A] vs [Weapon B]: [shared elements]

MISSING TECHNIQUES: (not used by ANY weapon in theme)
  - [Technique 1]: [Which weapon could benefit]
  - [Technique 2]: [Opportunity description]

VFX TECHNIQUE DISTRIBUTION:
  Trail types used: [list]
  Trail types NOT used: [list — opportunities]
  Particle approaches: [list]
  Screen effects: [which weapons use them]
  Shader effects: [which weapons use custom shaders]
  Musical integration: [which weapons have it, which don't]

RECOMMENDATIONS: (priority order)
1. [Highest impact improvement]
2. [Second priority]
3. [Third priority]

REFERENCE QUALITY: Theme [meets/falls short of/exceeds] Calamity equivalent tier
```

## Step 8: Implementation (if user approves enhancements)

For each approved enhancement:
- Use `/new-weapon-vfx` workflow for major overhauls
- Use `/design-projectile` for projectile-specific improvements
- Use `/design-impact` for impact-specific improvements
- Direct implementation for minor additions (bloom layers, particle tweaks)
- Tag asset requirements and check existing library first
- Run `dotnet build` after each weapon's changes
