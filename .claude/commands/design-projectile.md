Design a projectile from behavior to rendering — movement pattern, visual rendering, trail system, impact effects, and performance budget.

Context: $ARGUMENTS

---

## When to Use
- Creating a new weapon projectile
- Redesigning an existing projectile's visuals
- Adding special projectile behaviors (homing, splitting, spiraling)
- Designing beam/laser endpoints and rendering

## Step 1: Interactive Context Dialog

Ask the user:
1. **What weapon fires this projectile?** (Theme, class, weapon identity)
2. **What FEELING should the projectile evoke?** (Swift arrow, lumbering bomb, ethereal wisp, piercing lance, chaotic scatter?)
3. **Movement behavior?** (Straight, homing, spiraling, splitting, boomerang, orbiting, channeled beam?)
4. **How many active at once?** (Single precision shot, moderate spread, bullet hell swarm?)
5. **Special on-hit behavior?** (Pierce, explode, split, chain, mark, bounce, stick, create zone?)

## Step 2: Search Reference Mods

Search for similar projectile implementations:
- **Calamity:** Search by projectile type (homing → `ExoFlareProjectile`, beam → `DeathrayBase`, split → `SplitTypeProjectile`)
- **WotG:** Search for divine projectiles, screen-level rendering
- **MagnumOpus:** Check Foundation Weapons for matching patterns

Read 2-3 full implementations: the projectile .cs, its rendering code, and any associated shaders.

## Step 3: Select Movement Behavior

| Behavior | Description | VFX Implications |
|----------|------------|-----------------|
| **Linear** | Straight line, constant speed | Clean trail, simple rendering |
| **Homing** | Curves toward target | Curving trail needs smooth interpolation |
| **Spiraling** | Helical path around axis | Ribbon trail showcases the spiral |
| **Splitting** | Divides into sub-projectiles | Each sub needs lighter VFX budget |
| **Boomerang** | Returns to player | Trail must look good in both directions |
| **Orbiting** | Circles around player/point | Circular trail, connection line to center |
| **Channeled beam** | Continuous from weapon to cursor | Strip mesh, UV scrolling, endpoint effects |
| **Chain lightning** | Jumps between targets | Segmented line rendering, impact at each node |
| **Accelerating** | Starts slow, speeds up | Trail thickens/intensifies over time |
| **Decelerating** | Starts fast, slows | Trail fades as projectile slows |
| **Gravity-affected** | Arcing trajectory | Natural trail curve, ground impact |
| **Teleporting** | Blink between positions | Afterimage at origin, flash at destination |
| **Wave** | Sinusoidal path | Flowing ribbon trail |
| **Seeking swarm** | Group of projectiles converge | Each needs minimal VFX, convergence burst |

### Spawn Patterns (for multi-projectile weapons)

| Pattern | Description |
|---------|------------|
| **Fan spread** | N projectiles in arc from weapon |
| **Burst ring** | N projectiles in 360° from point |
| **Sequential** | Rapid-fire stream, slight spread |
| **Converging** | Multiple angles meeting at cursor |
| **Spiral release** | Rotating spawn angle each shot |
| **Random scatter** | Random angles within cone |
| **Formation** | Structured grid/pattern traveling together |

### On-Hit Behaviors

| Behavior | Description |
|----------|------------|
| **Pierce** | Passes through N enemies |
| **Explode** | Area damage + particle burst |
| **Split** | Spawns sub-projectiles on contact |
| **Chain** | Jumps to nearby enemy |
| **Bounce** | Ricochets off surfaces |
| **Stick** | Embeds in enemy, deals DoT |
| **Zone** | Creates lingering damage area |
| **Mark** | Tags enemy for bonus damage |

## Step 4: Select Rendering Pipeline

| Approach | Best For | Foundation |
|----------|---------|------------|
| **Bloom-rendered** | Energy orbs, magic missiles, bullets | SparkleProjectileFoundation |
| **Shader-masked orb** | Glowing spheres with internal detail | MagicOrbFoundation |
| **Primitive trail mesh** | Fast projectiles needing smooth trails | RibbonFoundation (10 modes) |
| **Beam strip** | Continuous laser/beam weapons | LaserFoundation, ThinLaserFoundation |
| **Fire/energy beam** | Fire/energy beams with spinning rings | InfernalBeamFoundation (3-texture body) |
| **Afterimage chain** | High-speed projectiles | Draw previous positions with fade |
| **Particle cluster** | Swarm/cloud projectiles | MagnumParticleHandler |
| **Orbiting sub-projectiles** | Circling/satellite projectiles | FoundationIncisorOrbs |

### Bloom Creativity Guide

One GlowOrb texture produces 10+ distinct projectile types through creative application:

| Variation | How |
|-----------|-----|
| Energy bullet | Small scale, high opacity, fast |
| Magic missile | Medium scale, pulsing opacity, trail |
| Wisp/spirit | Large scale, low opacity, drifting |
| Star projectile | Star-shaped mask over bloom |
| Charged shot | Growing scale over charge time |
| Cluster bomb | Multiple small overlapping blooms |
| Beam endpoint | Elongated bloom at beam tip |
| Shield orb | Large bloom + ring SDF overlay |
| Healing pulse | Soft bloom + ascending particles |
| Musical note | Note-shaped mask over bloom |

## Step 5: Design Trail System

| Trail Type | Technique | Best For |
|-----------|-----------|---------|
| **Ribbon** | RibbonFoundation primitive mesh | Flowing energy, magic |
| **Afterimage** | Previous positions with decreasing opacity | Speed, physical projectiles |
| **Particle wake** | Spawned particles along path | Fire, ice, nature |
| **Shader trail** | Custom .fx on primitive mesh | Unique/complex trails |
| **Beam strip** | UV-scrolling textured strip | Continuous beams |
| **None** | No trail (rare — most projectiles need one) | Very fast bullets, swarm members |

### Trail Requirements
- **Projectiles >5px/frame MUST have a trail**
- Trail fades at tail (no abrupt cutoff)
- Width tapers (thick at projectile, thin at tail)
- UV scrolling for internal movement
- Edge fading via smoothstep in shader

### Trail Rendering Reference
- RibbonFoundation has 10 modes — read `Content/FoundationWeapons/RibbonFoundation/` for full catalog
- Calamity's PrimitiveRenderer: search `PrimitiveRenderer.cs` for triangle strip construction
- WotG's BasePrimitiveLaserbeam: search for beam-specific primitive rendering

## Step 6: Design Impact Behavior

Reference `/design-impact` for full impact layer design. At minimum:
- **Particle burst:** 5+ particles themed to weapon
- **Bloom flash:** Brief additive flash at hit point
- **Screen shake:** Proportional to damage (light=2px, standard=4px, heavy=8px)

For special on-hit (explode, split, chain): design the secondary effect too.

## Step 7: Asset Check

Verify all needed assets exist:
- `Assets/VFX Asset Library/Projectiles/` — 7 projectile sprites
- `Assets/VFX Asset Library/GlowAndBloom/` — bloom/flare for rendering
- `Assets/VFX Asset Library/TrailsAndRibbons/` — 4 trail strips
- `Assets/VFX Asset Library/BeamTextures/` — 14 beam textures (for beams)
- `Assets/VFX Asset Library/NoiseTextures/` — for shader-based trails
- `Assets/VFX Asset Library/ColorGradients/` — theme LUT ramps
- `Assets/Particles Asset Library/` — impact particles, themed sprites

If missing: **STOP** and provide Midjourney prompt.

## Step 8: Performance Budget

| Tier | Max Active | Particles/Proj | Trail Points | Bloom Layers |
|------|-----------|---------------|--------------|-------------|
| **Bullet/fast** | 20+ | 0-2 | 8-12 | 1 |
| **Standard** | 10-15 | 3-5 | 15-20 | 2 |
| **Complex shader** | 5-8 | 5-10 | 20-30 | 2-3 |
| **Boss signature** | 1-3 | 10-20 | 30-50 | 3 |

If the weapon fires many simultaneous projectiles, each individual projectile needs LIGHTER VFX.

## Step 9: Final Projectile Spec

Present complete specification:
- Movement behavior + speed + lifetime
- Rendering approach (foundation reference)
- Trail system (type, shader, width/color functions)
- Bloom/glow layers (scales, opacities)
- Impact effects (layers, particles, screen response)
- Special mechanics (pierce, split, chain, etc.)
- Musical integration (note particles, harmonic timing)
- Performance budget (max active, particles per, trail points)
- Color palette (from theme)

## Step 10: Implement

Create projectile file following SandboxLastPrism pattern:

```
Content/<Theme>/<Category>/<Weapon>/Projectiles/
├── <ProjectileName>.cs     — Projectile behavior + rendering
├── <ProjectileName>.png    — Projectile sprite (if sprite-based)
```

Key implementation points:
- Override `PreDraw` / `PostDraw` for custom rendering
- SpriteBatch state management (End → Begin Immediate/Additive → Draw → End → Restore)
- `Lighting.AddLight()` for glow
- Trail position tracking in `AI()` (store old positions in `Projectile.oldPos`)
- Impact in `OnHitNPC` / `Kill`

Run `dotnet build` to verify.
