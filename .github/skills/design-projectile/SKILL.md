---
description: "Design a projectile's behavior, visual rendering, trail, and impact — step-by-step interactive workflow from concept to implementation spec."
---

# /design-projectile — Projectile VFX Design Workflow

A step-by-step interactive workflow for designing a complete projectile — behavior, rendering, trail, and impact — from concept to implementation.

## When to Use This Skill
- Creating a new weapon projectile
- Redesigning an existing projectile's visuals
- Adding special projectile behaviors (homing, splitting, spiraling)
- Designing beam/laser endpoints and rendering

## Workflow Steps

### Step 1: Interactive Context Dialog
Ask the user:
1. **What fires this projectile?** (Weapon class, theme, weapon name if known)
2. **Movement behavior:** Straight, homing, spiraling, boomerang, splitting, orbiting, wave-pattern, gravity-affected?
3. **Visual impression:** How should it LOOK in motion? (Fast streak, glowing orb, spinning blade, musical note, energy lance, cascading sparks?)
4. **Speed tier:** Bullet-fast, moderate tracking, slow heavy, variable (accelerating/decelerating)?
5. **What happens on hit?** (Explode, pierce, split, ricochet, stick, chain to nearby enemies, leave mark?)

### Step 2: Search Reference Mods
Before designing, search for similar projectiles:
- **Calamity:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo` — Search `Projectiles/` for similar archetypes
- **WotG:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Wrath of the Gods Repo` — Search for divine/cosmic projectile patterns
- **Everglow:** `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Everglow Repo` — VFXBatch projectile rendering

Read at least 2-3 concrete implementations. Note what works well and what to adapt.

### Step 3: Map to Foundation Weapon Template
Check these Foundation Weapons for starting points:
| Foundation | Projectile Type |
|-----------|----------------|
| `SparkleProjectileFoundation` | Bloom-rendered projectiles (Photoviscerator-style) |
| `MagicOrbFoundation` | Shader-masked spherical projectiles |
| `InfernalBeamFoundation` | Beam/laser primitives |
| `LaserFoundation` | Thin laser lines |
| `ThinLaserFoundation` | Very thin precision lasers |
| `FoundationIncisorOrbs` | Orbiting sub-projectiles |

### Step 4: Design Render Pipeline
Based on Steps 1-3, design the visual rendering:
- **Sprite-based:** Simple texture draw + bloom layers + trail
- **Shader-driven:** Custom .fx with noise/glow/distortion
- **Primitive-based:** VertexStrip triangle mesh (for beams/lasers)
- **Hybrid:** Shader body + particle accents + primitive trail

Present 2-3 rendering approaches to the user.

### Step 5: Design Trail System
Every projectile needs a trail decision:
- **No trail** (only for instant-hit or very fast projectiles)
- **Afterimage trail** (previous positions, fading copies)
- **Primitive trail** (smooth triangle strip with shader)
- **Particle wake** (scattered dust behind)
- **Bloom trail** (glow orbs along path)
- **Combined** (primitive + particle accents)

### Step 6: Design Impact Behavior
Invoke @impact-designer patterns or design inline:
- Impact particle burst (radial, themed)
- Screen shake (scaled to projectile importance)
- Flash/glow at hit point
- Status effect application visuals
- On-kill escalation (bigger explosion if killing blow)

### Step 7: Asset Check
Check existing assets before requesting new ones:
- `Assets/VFX Asset Library/Projectiles/` — 7 projectile sprites
- `Assets/Particles Asset Library/` — 107+ particle sprites
- `Assets/VFX Asset Library/TrailsAndRibbons/` — 4 trail textures
- `Assets/VFX Asset Library/GlowAndBloom/` — 8 bloom sprites
- Theme-specific asset folders

If assets are missing: **STOP** and provide Midjourney prompt.

### Step 8: Performance Budget
| Projectile Complexity | Active Limit | Particles per Proj | Trail Points |
|----------------------|-------------|-------------------|-------------|
| Simple (bullet) | 20+ | 0-2 | 8-12 |
| Medium (orb+trail) | 10-15 | 3-5 | 15-20 |
| Complex (shader+particles) | 5-8 | 5-10 | 20-30 |
| Boss signature | 1-3 | 10-20 | 30-50 |

### Step 9: Final Specification
Present complete projectile spec:
- Movement behavior + AI code pattern
- Render technique + shader/sprite details
- Trail system + shader choice
- Impact behavior + particle burst details
- Color palette (from theme)
- Performance notes
- Asset dependencies

### Step 10: Implementation
Delegate to appropriate agents:
- @projectile-architect for behavior code
- @trail-specialist for trail rendering
- @shader-specialist for custom shaders
- @particle-specialist for impact/wake particles
- @impact-designer for hit effects
