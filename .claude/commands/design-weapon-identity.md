Design a complete weapon identity from blank slate — 15+ interactive questions across multiple rounds, exploring musical soul, attack patterns, VFX vision, uniqueness, and full implementation spec.

Context: $ARGUMENTS

---

## When to Use
- Starting a new weapon from scratch
- Redesigning a weapon that feels generic
- Exploring what a theme's next weapon should be
- Creating a signature/boss-drop weapon that needs to be exceptional

## Step 1: Deep Interactive Dialog (15+ questions, back-and-forth)

Each answer shapes the next question. This is a conversation, not a form.

### Round 1: Foundation (4-5 questions)
1. What THEME/SCORE? (La Campanella, Eroica, Swan Lake, Moonlight Sonata, Enigma Variations, Fate, Clair de Lune, Dies Irae, Nachtmusik, Ode to Joy)
2. What WEAPON CLASS? (Melee sword, melee spear, melee boomerang, ranged bow, ranged gun, magic staff, magic tome, summoner, whip, other?)
3. What TIER/PROGRESSION POINT? (Early game, mid-game, hardmode, post-plantera, post-golem, post-moon lord, endgame?)
4. What EXISTING WEAPONS are already in this theme for this class? (Search `Content/<Theme>/` to find out — must differentiate!)
5. In ONE PHRASE, what should wielding this weapon feel like? (Let intuition guide: "conducting a thunderstorm," "ringing a cathedral bell," "painting moonlight")

### Round 2: Musical Identity (3-4 questions)
6. What aspect of the theme's MUSIC does this weapon embody? (A passage? A mood? An instrument? A dynamic level — pianissimo vs fortissimo?)
7. Should the weapon feel like the BEGINNING of the piece (gentle, mysterious), the MIDDLE (building, developing), or the CLIMAX (overwhelming, peak)?
8. What TEMPO fits the attack rhythm? (Adagio=slow+heavy, Allegro=bright+brisk, Presto=lightning-fast, Rubato=variable, Staccato=sharp+punctuated)
9. Should music notes/symbols be VISIBLE in effects, or expressed purely through FEEL (pacing, rhythm, dynamics)?

### Round 3: Attack Character (3-4 questions)
10. PRIMARY ATTACK PATTERN: Simple repeating? Combo chain (how many phases)? Charged release? Hold-and-channel? Mode switching?
11. SPECIAL MECHANIC: What makes this weapon UNIQUE beyond visuals? (See mechanic catalog below)
12. SIGNATURE MOMENT: What's the ONE spectacular thing this weapon does?
13. How much SCREEN PRESENCE should attacks have? Scale 1-10.

### Round 4: Visual Identity (3-4 questions)
14. TRAIL CHARACTER: Sharp clean arc? Flowing energy ribbon? Scattered particles? Thick smear? Thin elegant line?
15. IMPACT FEEL: Light touch, solid thwack, explosive burst, elegant cut, earth-shaking pound?
16. AMBIENT PRESENCE: When held but not attacking — floating particles? Pulsing glow? Idle animation? Orbiting accents? Dormant?
17. Any specific VISUAL REFERENCES from games, anime, movies?

---

## Mechanic Catalogs (reference during Round 3)

### Combo Systems
| Type | Description | VFX Hooks |
|------|-------------|-----------|
| **Phase Chain** | Fixed sequence A→B→C→D, each unique | Different trail per phase, escalating particles |
| **Branching** | After phase 1, hold=heavy / tap=light | Visual telegraph for which path |
| **Music Movement** | Allegro→Vivace→Presto→Fortissimo | Each movement has increasing VFX density |
| **Infinite Loop** | Loops back to start with escalating bonuses | Loop counter visual, VFX intensify per cycle |
| **Context** | Different attack if moving/standing/airborne/after-dash | Unique VFX per movement context |

### Resource Systems
| Resource | Builds From | Spends On | Visual Feedback |
|----------|------------|-----------|----------------|
| **Resonance** | Landing hits | Empowered next attack | Orbiting note count, weapon glow intensifies |
| **Heat** | Continuous use | Auto-vents at cap | Barrel glow (blue→orange→red overheat smoke) |
| **Tempo** | Hits in rhythm | Faster attacks, more projectiles | Metronome visual, beat indicator pulse |
| **Harmony Stacks** | Using different attack types | Consume all for burst | Colored orbs orbiting (one per stack) |
| **Crescendo** | Dealing damage | Auto-triggers at threshold | Building screen tint, growing aura |

### Mode Switching
| Type | Trigger | VFX Transition |
|------|---------|----------------|
| **Manual Toggle** | Right-click / special key | Flash + particle burst, sprite swap |
| **Automatic Cycle** | Every N attacks | Particle burst between modes |
| **Threshold Transform** | Damage dealt / HP / kills | Dramatic transformation (flash + particles + invuln) |
| **Environmental** | Biome / time of day / boss presence | Gradual visual morph |
| **Stance Switch** | Alternating attack button | Swift transition, distinct idle poses |

### Special Triggers
| Trigger | Description |
|---------|-------------|
| **Nth Hit** | Every Nth consecutive hit triggers special (screen pulse, burst) |
| **Perfect Timing** | Rhythmic window bonus (visual + damage reward) |
| **Kill Chain** | Consecutive kills within window stack bonuses |
| **Damage Threshold** | Total damage triggers automatic release |
| **HP Reactive** | Weapon changes behavior at low HP |
| **Synergy** | Bonus when paired with specific accessories or other weapons |
| **Environmental** | Bonus effects during rain, blood moon, etc. |
| **Combo Finisher** | Final hit in combo chain is always special |

---

## Step 2: Generate Weapon Concept Proposals

Based on dialog answers, generate 2-3 complete concepts:

Each proposal includes:
- **Name** (musical/thematic)
- **One-line identity** ("A flame-wreathed bell that tolls with each swing, building to a crescendo of fire")
- **Attack pattern** (full combo breakdown if applicable)
- **Special mechanic** (what makes it unique)
- **Signature moment** (the WOW effect)
- **VFX vision** (trail, particles, impact, lighting, screen effects)
- **Musical integration** (how music identity manifests visually)

Present for user selection or refinement.

## Step 3: Refine Selected Concept

Ask 3-4 follow-up questions to sharpen the chosen concept:
- Specific visual details of the signature moment
- Exact feel of the trail type (organic vs geometric, heavy vs light)
- Crescendo pacing (linear buildup vs exponential)
- Any elements to add or remove

## Step 4: Design Complete Attack Pattern Set

For each attack phase/mode:
- Animation timing (CurveSegment breakdown: windup, swing, follow-through)
- VFX active during this phase
- Sound/visual cue that identifies the phase
- Damage/knockback profile
- How this phase differs visually from adjacent phases

### CurveSegment Musical Pacing Reference
```csharp
// Each combo phase as a musical movement
// Allegro — bright and brisk
new CurveSegment(EasingType.ExpOut, 0f, 0f, -MathHelper.PiOver2, 1)  // Quick swing

// Andante — walking pace, measured
new CurveSegment(EasingType.SineOut, 0f, 0f, -MathHelper.PiOver2, 2)  // Moderate swing

// Presto — lightning fast
new CurveSegment(EasingType.ExpOut, 0f, 0f, -MathHelper.Pi, 0.5f)  // Rapid sweep

// Fortissimo — maximum power, deliberate
new CurveSegment(EasingType.PolyInOut, 0f, 0f, -MathHelper.TwoPi, 3) // Dramatic windup + release
```

## Step 5: Design Complete VFX Layer Set

For each visual layer:
- **Trail system:** Type, shader, width/color function
- **Particle choreography:** Dust types, bloom layers, music notes
- **Impact effects:** Per damage tier (see `/design-impact`)
- **Lighting:** Held glow, attack flash, ambient
- **Screen effects:** For signature moments only
- **Motion:** Afterimages, smears, hit-stop

## Step 6: Uniqueness Audit

Read ALL other weapons in the same theme:
- Verify NO overlap in primary attack pattern
- Verify NO overlap in special mechanic
- Verify NO overlap in VFX approach (even with same colors, techniques must differ)
- If overlap found: redesign the overlapping aspect

## Step 7: Asset Audit

Catalog all required assets:
- Weapon sprite (needs Midjourney prompt if new)
- VFX textures (check existing library first)
- Shader requirements (reuse existing or new?)
- Particle sprites (check catalog)
- ModDust sprites (if custom dust needed)

For any missing asset: **STOP** and provide Midjourney prompt.

## Step 8: Full Implementation Spec

Produce complete implementation plan:
- File list (which .cs files to create/modify)
- Folder structure following SandboxLastPrism pattern
- Code architecture (items, projectiles, dusts, systems)
- Dependency order (what to build first)
- Foundation Weapons to reference for each subsystem

## Step 9: Implement

Build the weapon following the spec:
- Item class with `ModifyTooltips` (effect descriptions + themed lore + correct OverrideColor)
- Projectile/swing classes with full VFX rendering
- Custom ModDust types if needed
- Shader registration in ShaderLoader.cs if new shaders
- Run `dotnet build` to verify
