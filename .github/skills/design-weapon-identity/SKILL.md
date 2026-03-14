---
description: "Design a complete weapon identity from blank slate — 15+ interactive questions, musical soul, attack patterns, VFX vision, uniqueness audit, full implementation spec."
---

# /design-weapon-identity — Full Weapon Concept Workflow

The deepest creative workflow: design a complete weapon from nothing. This skill runs 15+ interactive questions across multiple rounds, explores creative directions, and produces a full weapon identity with attack patterns, VFX vision, and implementation plan.

## When to Use This Skill
- Starting a new weapon from scratch
- Redesigning a weapon that feels generic
- Exploring what a theme's next weapon should be
- Creating a signature/boss-drop weapon that needs to be exceptional

## Workflow Steps

### Step 1: Deep Interactive Dialog (15+ questions, back-and-forth)

**Round 1: Foundation (4-5 questions)**
1. What THEME/SCORE? (La Campanella, Eroica, Swan Lake, Moonlight Sonata, Enigma Variations, Fate, Clair de Lune, Dies Irae, Nachtmusik, Ode to Joy)
2. What WEAPON CLASS? (Melee sword, melee spear, melee boomerang, ranged bow, ranged gun, magic staff, magic tome, summoner, whip, other?)
3. What TIER/PROGRESSION POINT? (Early game, mid-game, hardmode, post-plantera, post-golem, post-moon lord, endgame?)
4. What EXISTING WEAPONS are already in this theme for this class? (Must differentiate!)
5. In ONE PHRASE, what should wielding this weapon feel like? (Let intuition guide: "conducting a thunderstorm," "ringing a cathedral bell," "painting moonlight")

**Round 2: Musical Identity (3-4 questions)**
6. What aspect of the theme's MUSIC does this weapon embody? (A particular passage? A mood? An instrument? A dynamic level — pianissimo vs fortissimo?)
7. Should the weapon feel like the BEGINNING of the piece (gentle, mysterious), the MIDDLE (building, developing), or the CLIMAX (overwhelming, peak)?
8. What musical TEMPO fits the attack rhythm? (Adagio=slow+heavy, Allegro=bright+brisk, Presto=lightning-fast, Rubato=variable, Staccato=sharp+punctuated)
9. Should music notes/symbols be VISIBLE in the effects, or should the musical identity be purely in the FEEL (pacing, rhythm, dynamics)?

**Round 3: Attack Character (3-4 questions)**
10. PRIMARY ATTACK PATTERN: Simple repeating? Combo chain (how many phases)? Charged release? Hold-and-channel? Mode switching?
11. SPECIAL MECHANIC: What makes this weapon UNIQUE beyond visuals? (Heat buildup, resonance stacking, harmonic combos, phase transformation, rhythmic timing bonus?)
12. SIGNATURE MOMENT: What's the ONE spectacular thing this weapon does? (Every 5th hit? Full charge release? Combo finisher? Kill effect? Special input?)
13. How much SCREEN PRESENCE should attacks have? Scale 1-10 where 1=subtle and 10=screen-filling.

**Round 4: Visual Identity (3-4 questions)**
14. TRAIL CHARACTER: What trails suit this weapon? (Sharp clean arc, flowing energy ribbon, scattered particles, thick smear, thin elegant line, no trail?)
15. IMPACT FEEL: Light touch, solid thwack, explosive burst, elegant cut, earth-shaking pound?
16. AMBIENT PRESENCE: When held but not attacking, should the weapon have visual activity? (Floating particles, pulsing glow, idle animation, orbiting accents?)
17. Any specific VISUAL REFERENCES from games, anime, movies? ("Like the Monado from Xenoblade," "Like Vergil's Yamato," "Like a conductor's baton that controls elements")

### Step 2: Generate Weapon Concept Proposals
Based on dialog answers, generate 2-3 complete weapon concepts:

Each concept includes:
- **Name** (musical/thematic)
- **One-line identity** ("A flame-wreathed bell that tolls with each swing, building to a crescendo of fire")
- **Attack pattern** (full combo breakdown if applicable)
- **Special mechanic** (what makes it unique)
- **Signature moment** (the WOW effect)
- **VFX vision** (trail, particles, impact, lighting, screen effects)
- **Musical integration** (how music identity manifests visually)

Present to user for selection or refinement.

### Step 3: Refine Selected Concept
Ask 3-4 follow-up questions to sharpen the chosen concept:
- "You chose the bell-striker concept — should the bell chime visual be a full-screen ring or localized to the weapon area?"
- "The fire trails — organic flickering or structured geometric flames?"
- "Crescendo mechanic — linear build (steady increase) or exponential (slow start, explosive at max)?"

### Step 4: Design Complete Attack Pattern Set
For each attack phase/mode:
- Animation timing (CurveSegment breakdown)
- VFX active during this phase
- Sound/visual cue that identifies the phase
- Damage/knockback profile

### Step 5: Design Complete VFX Layer Set
For each visual layer:
- Trail system (type, shader, width/color function)
- Particle choreography (dust, bloom, notes)
- Impact effects (per damage tier)
- Lighting (held glow, attack flash, ambient)
- Screen effects (for signature moments only)
- Motion (afterimages, smears, hit-stop)

### Step 6: Uniqueness Audit
Read ALL other weapons in the same theme:
- Verify NO overlap in primary attack pattern
- Verify NO overlap in special mechanic
- Verify NO overlap in VFX approach (even with same colors, techniques must differ)
- If overlap found: redesign the overlapping aspect

### Step 7: Asset Audit
Catalog all required assets:
- Weapon sprite (needs Midjourney prompt if new)
- VFX textures (check existing library first)
- Shader requirements (reuse existing or new?)
- Particle sprites (check catalog)
- ModDust sprites (if custom dust needed)

For any missing asset: **STOP** and provide Midjourney prompt.

### Step 8: Full Implementation Spec
Produce a complete implementation plan:
- File list (which .cs files to create/modify)
- Folder structure following SandboxLastPrism pattern
- Code architecture (items, projectiles, dusts, systems)
- Agent routing (which agents implement which parts)
- Dependency order (what to build first)

### Step 9: Implementation
Route to agents:
- @creative-director validates concept coherence
- @weapon-mechanic implements combo/special systems
- @trail-specialist implements trails
- @particle-specialist implements particles/bloom
- @impact-designer implements hit effects
- @shader-specialist implements custom shaders
- @motion-animator implements afterimages/smears
- @lighting-atmosphere implements glow/aura
- @screen-effects implements screen-level effects (if any)
