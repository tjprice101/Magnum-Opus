---
description: "Design multi-layered hit effects, crit flashes, death bursts, and screen shake — step-by-step interactive workflow from punch feel to implementation."
---

# /design-impact — Impact Effect Design Workflow

A step-by-step interactive workflow for designing what happens when things HIT — from the initial flash to the final particle settling.

## When to Use This Skill
- Designing weapon hit effects (melee, ranged, magic, summon)
- Creating crit/super-crit escalation visuals
- Designing enemy death burst effects
- Adding screen shake and hit-stop to attacks
- Creating boss attack impact effects on the player

## Workflow Steps

### Step 1: Interactive Context Dialog
Ask the user:
1. **What is hitting?** (Melee swing, projectile, beam endpoint, summon attack, boss attack, environmental hazard?)
2. **Theme/score?** (Constrains color palette and emotional tone)
3. **How should it FEEL?** (Heavy and earth-shaking? Quick and precise? Explosive and chaotic? Elegant and clean?)
4. **Damage tier:** Minor tick, standard hit, heavy, critical, killing blow?
5. **Any special on-hit mechanic?** (Apply debuff, chain damage, mark enemy, spawn secondary projectile?)

### Step 2: Search Reference Mods
Search for similar impact patterns:
- **Calamity:** Search `Particles/` for CritSpark, CircularSmear, SemiCircularSmear, PulseRing
- **WotG:** Search for divine impact effects, screen-level hit responses
- **MagnumOpus:** Check `Content/FoundationWeapons/ImpactFoundation/` (3 shader modes) and `Content/FoundationWeapons/ExplosionParticlesFoundation/` (CPU physics)

Read 2-3 implementations. Note the layering pattern.

### Step 3: Design Impact Layer Stack
Every quality impact has multiple layers. Design which layers this impact uses:

| Layer | Description | Include? |
|-------|------------|---------|
| 1. Flash | Brief bright overlay at hit point (1-3 frames) | Almost always |
| 2. Ring/wave | Expanding circle or shockwave from hit point | Medium+ hits |
| 3. Particle burst | Themed particles scattered from impact | Almost always |
| 4. Screen shake | Camera offset for weight | Medium+ hits |
| 5. Hit-stop | Brief freeze (1-3 frames) for heavy feel | Heavy hits, crits |
| 6. Dust/debris | Small physical particles (gravity-affected) | Grounded hits |
| 7. Persistent mark | Lingering glow/symbol at hit location | Special hits |
| 8. Screen effect | Flash, vignette pulse, chromatic aberration | Crits, kills |

Present layer selection to user for approval.

### Step 4: Design Damage Tier Escalation
How the impact scales with damage importance:

| Tier | Layers | Intensity | Duration |
|------|--------|-----------|----------|
| Minor | Flash + 3-5 particles | Low | 5-10 frames |
| Standard | Flash + ring + 8-12 particles + light shake | Medium | 10-15 frames |
| Heavy | Flash + ring + 15-20 particles + shake + dust | High | 15-20 frames |
| Critical | All layers + hit-stop + screen flash + chromatic | Maximum | 20-30 frames |
| Kill | All layers + death burst + persistent mark + screen pulse | Spectacular | 30-60 frames |

### Step 5: Design Themed Death Burst
Enemy death effects should reflect the weapon's theme:
- **La Campanella:** Ember explosion + smoke billow + bell chime visual ring
- **Eroica:** Sakura petal scatter + golden light burst + ascending embers
- **Swan Lake:** White feather drift + prismatic flash + elegant dissolve
- **Moonlight Sonata:** Purple mist exhale + silver sparkle settle + moonbeam fade
- **Enigma:** Void implosion + eerie green flame remnants + watching eye flash
- **Fate:** Cosmic particle scatter + star field brief + chromatic shatter
- **Clair de Lune:** Soft blue mist release + pearl sparkle + gentle dissipation
- **Dies Irae:** Blood explosion + ember rain + ground scorch mark
- **Nachtmusik:** Starlight fragment scatter + indigo mist + constellation brief
- **Ode to Joy:** Golden light burst + warm amber particles + musical note cascade

### Step 6: Asset Check
Check existing assets:
- `Assets/VFX Asset Library/ImpactEffects/` — 8 impact textures
- `Assets/Particles Asset Library/` — sparkles, explosions, lightning bursts, smoke
- `Assets/VFX Asset Library/GlowAndBloom/` — flash/flare sprites
- `Content/FoundationWeapons/ImpactFoundation/` — 3 shader types (ripple, damage zone, slash mark)
- `Content/FoundationWeapons/ExplosionParticlesFoundation/` — CPU physics burst

If missing: **STOP** and provide Midjourney prompt.

### Step 7: Final Impact Spec
Present complete impact specification:
- Layer stack with timing (per-frame breakdown)
- Particle types, counts, velocities, lifetimes
- Screen shake pattern and intensity
- Hit-stop duration (if applicable)
- Screen effect details (if applicable)
- Death burst variant
- Color palette (from theme)
- Performance budget

### Step 8: Implementation
- @impact-designer for full system code
- @particle-specialist for burst particles and bloom
- @screen-effects for screen shake, flash, chromatic aberration
- @shader-specialist if custom impact shader needed
