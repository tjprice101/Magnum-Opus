# MagnumOpus — Claude Code Project Guide

## What This Mod Is

MagnumOpus is a music-themed Terraria tModLoader mod. 10 musical scores/themes, each with its own weapons, bosses, accessories, and visual identity. Every weapon implements its own unique VFX directly in its .cs files — there are no global VFX systems that auto-apply effects. It is **a symphony made playable**.

## Guiding Principles

1. **EVERY WEAPON IS UNIQUE** — Within a theme, no two weapons of the same class share VFX approaches. Different trails, impacts, mechanics, shaders.
2. **MUSICAL IDENTITY IS VISIBLE** — Music notes, harmonic pulses, resonance waves, rhythmic timing woven into effects where appropriate.
3. **CREATIVE USE OF ASSETS** — 200+ custom textures in the library. Mix, layer, and reuse creatively. 1 texture → infinite effects through different scale/tint/blend/spawn patterns.
4. **LAYER EFFECTS FOR RICHNESS** — Core + bloom + accents minimum. Single-pass flat rendering is never acceptable.
5. **THEME COLORS STAY CONSISTENT** — Each theme has a strict palette. HOW effects work within that palette is open to creative interpretation.
6. **IMPACTS FEEL SPECIAL** — Every hit is multi-layered and memorable, not "small explosion → done."

## Implementation Standards

- Always implement by editing files directly — write actual working C# and HLSL code
- After implementation, run `dotnet build` to verify compilation
- Always search existing systems before building new infrastructure
- Always search reference repositories before inventing VFX patterns from scratch

## Interactive Design Protocol

For new weapon/boss content: ask 15+ questions across multiple rounds before proposing any design. Each answer shapes the next question. Present 2-3 creative options for user selection. Never skip the dialog — the user WANTS extensive questions.

## Theme Identities

| Theme | Musical Soul | Colors | Emotional Core | Lore Color |
|-------|-------------|--------|---------------|------------|
| **La Campanella** | The ringing bell, virtuosic fire | Black smoke, orange flames, gold | Passion, intensity, burning brilliance | `new Color(255, 140, 40)` |
| **Eroica** | The hero's symphony | Scarlet, crimson, gold, sakura pink | Courage, sacrifice, triumphant glory | `new Color(200, 50, 50)` |
| **Swan Lake** | Grace dying beautifully | Pure white, black contrast, prismatic edges | Elegance, tragedy, ethereal beauty | `new Color(240, 240, 255)` |
| **Moonlight Sonata** | The moon's quiet sorrow | Deep purples, vibrant blues, violet, ice blue | Melancholy, peace, mystical stillness | `new Color(140, 100, 200)` |
| **Enigma Variations** | The unknowable mystery | Void black, deep purple, eerie green | Mystery, dread, arcane secrets | `new Color(140, 60, 200)` |
| **Fate** | Celestial symphony of destiny | Black void, dark pink, bright crimson, celestial white | Cosmic inevitability, endgame awe | `new Color(180, 40, 80)` |
| **Clair de Lune** | Shattered time, blazing clocks | Dark red, vibrant gray, white | Temporal destruction, reality's unraveling | `new Color(150, 200, 255)` |
| **Dies Irae** | Hell's retribution flames | White, black, crimson | Divine judgment, heavenly banishment | `new Color(200, 50, 30)` |
| **Nachtmusik** | Starlit melodies, sweet songs | Golden, dark purple | Golden twinkling, nocturnal melody | `new Color(100, 120, 200)` |
| **Ode to Joy** | Eternal symphony garden | Monochromatic black, white, prismatic chromatic | Prismatic radiance, garden of eternal symphony | `new Color(255, 200, 50)` |

**Moonlight Sonata lore must NEVER reference cosmos, stars, or space — only moonlight, tides, silver, stillness, sorrow.**

## Reference Repositories

Search these local repos before inventing patterns from scratch:

| Repository | Local Path | Strengths |
|------------|-----------|-----------|
| **Calamity** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Calamity Mod Repo` | Primitive trails, CurveSegment animation, slash shaders, melee VFX, metaballs, boss AI |
| **Wrath of the Gods** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Wrath of the Gods Repo` | 190+ shaders, screen distortion + exclusion zones, per-phase boss backgrounds, GPU particles, reality tear effects |
| **Everglow** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Everglow Repo` | Pipeline post-processing, hierarchical bloom, VFXBatch GPU batching, dissolve effects, 35 paired dust types |
| **Coralite** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\Coralite Mod Repo` | Shader techniques, particle systems, rendering pipelines |
| **VFX+** | `C:\Users\creat\Downloads\Terraria Magnum Opus Mod Assets\VFX+ Mod Repo` | Advanced VFX systems, trail rendering, visual polish |

**Always read actual source files — never rely on memory or assumptions.**

## Asset Failsafe Protocol

**MANDATORY.** If you need a texture/sprite that does not exist: **STOP implementation** and tell the user:
1. What asset is needed and where it would be used
2. A detailed **Midjourney prompt** (art style, subject, color palette — white/grayscale on black for VFX — dimensions, technical requirements)
3. Expected file location using the SandboxLastPrism folder pattern

**NEVER use placeholder textures.** Missing asset = effect cannot be implemented yet.

### Available Asset Library (200+ textures)

| Location | Contents |
|----------|----------|
| `Assets/VFX Asset Library/BeamTextures/` | 14 beam strip textures |
| `Assets/VFX Asset Library/ColorGradients/` | 12 theme LUT ramps |
| `Assets/VFX Asset Library/GlowAndBloom/` | 8 bloom/flare sprites (GlowOrb, LensFlare, PointBloom, SoftGlow, StarFlare) |
| `Assets/VFX Asset Library/ImpactEffects/` | 8 impact textures |
| `Assets/VFX Asset Library/NoiseTextures/` | 20 noise types (Perlin, Simplex, FBM, Voronoi, Marble, Cosmic, Nebula) |
| `Assets/VFX Asset Library/MasksAndShapes/` | 7 mask textures |
| `Assets/VFX Asset Library/TrailsAndRibbons/` | 4 trail strip textures |
| `Assets/VFX Asset Library/SlashArcs/` | 4 sword arc textures |
| `Assets/VFX Asset Library/Projectiles/` | 7 projectile sprites |
| `Assets/Particles Asset Library/` | 107+ sprites — music notes, stars, sparkles, glyphs, halos, explosions |
| `Assets/SandboxLastPrism/` | Flare, Gradients, Orbs (5), Pixel, Trails (7 incl. Clear/) |
| Theme-specific folders | Each of 10 themes has 6-11 dedicated textures |

## File Structure (SandboxLastPrism Pattern)

| Asset Type | Location |
|------------|----------|
| VFX texture (.png for shaders/trails/bloom) | `Assets/<Theme>/<Item>/<TextureType>/` |
| Weapon/item sprite (.png) | Same folder as the `.cs` item file in `Content/` |
| Custom shader (.fx/.fxc) | `Effects/<Theme>/<Item>/` |
| ModDust code (.cs) | `Content/<Theme>/<Category>/<Item>/Dusts/` |
| ModDust sprite (.png) | `Content/<Theme>/<Category>/<Item>/Dusts/Textures/` |
| Item-specific systems (.cs) | `Content/<Theme>/<Category>/<Item>/Systems/` |

```
Content/<ThemeName>/<Category>/<ItemName>/
├── <ItemName>.cs           — Main item/weapon class
├── <ItemName>.png          — Item sprite
├── <ItemName>VFX.cs        — VFX static helper class (optional)
├── <ItemName>Swing.cs      — Swing projectile (melee)
├── Dusts/
│   ├── <DustName>.cs       — Custom ModDust types
│   └── Textures/           — Dust sprite PNGs
└── Systems/                — Item-specific systems
```

## Existing Systems — Check Before Building New

| System | Location | Purpose |
|--------|----------|---------|
| ShaderLoader | `Common/Systems/Shaders/ShaderLoader.cs` | Loads and manages all shaders |
| ShaderRenderer | `Common/Systems/Shaders/ShaderRenderer.cs` | Shader rendering utilities |
| MagnumParticleHandler | `Common/Systems/Particles/` | Particle lifecycle (2000 max, blend mode lists, 3000px frustum culling) |
| MetaballManager | `Common/Systems/Metaballs/` | Metaball fusion rendering |
| VFX/Trails | `Common/Systems/VFX/Trails/` | 7+ trail systems (Bezier, Calamity-style, Primitive, Enhanced, Nebula, Afterimage) |
| VFX/Bloom | `Common/Systems/VFX/Bloom/` | Bloom, lens flare, god rays |
| VFX/Boss | `Common/Systems/VFX/Boss/` | Boss arena, telegraphs, attack VFX helpers, cinematics |
| VFX/Screen | `Common/Systems/VFX/Screen/` | Skybox, distortion, heat effects |
| VFX/Effects | `Common/Systems/VFX/Effects/` | Afterimages, glow dust, smoke, screen shake |
| VFX/Optimization | `Common/Systems/VFX/Optimization/` | LOD, adaptive quality, batching |
| BossShaderManager | `Common/Systems/Bosses/BossShaderManager.cs` | Central shader loading + param application |
| BossSkyRegistration | `Common/Systems/Bosses/BossSkyRegistrationSystem.cs` | CustomSky registration |
| BossRenderHelper | `Common/Systems/Bosses/BossRenderHelper.cs` | DrawShaderAura, DrawShaderTrail, DrawPhaseTransition, DrawDissolve |

## Coding Conventions

### SpriteBatch State Management

**Critical rule: Always restore SpriteBatch state after modifying it.**

```csharp
// Save → Modify → Restore
spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
    DepthStencilState.None, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);
// Draw your effect...
spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
```

| Blend State | Use For |
|------------|---------|
| `BlendState.Additive` | Glow, energy, fire, bloom, trails — any luminous effect. **Black = invisible.** |
| `BlendState.AlphaBlend` | Smoke, solid shapes, UI elements — anything that should occlude |
| `MagnumBlendStates.*` | Custom blend states in `Common/MagnumBlendStates.cs` — includes ShaderAdditive, Multiply (Src=DestColor, Dest=Zero), Screen (Src=One, Dest=InvSrcColor) |

| Sampler | Use For |
|---------|---------|
| `SamplerState.LinearWrap` | UV-scrolling textures that tile (trails, beams) |
| `SamplerState.LinearClamp` | Non-tiling textures (sprites, bloom) |
| `SamplerState.PointClamp` | Pixel-art textures (preserve sharp edges) |

### Shader Development

- **File org:** Compiled `.fx`+`.fxc` in `Effects/<Theme>/<Item>/`, source in `ShaderSource/`, shared utilities in `ShaderSource/HLSLLibrary.fxh`
- **Registration:** All shaders registered in `Common/Systems/Shaders/ShaderLoader.cs`
- **Pixel shader signature:** `float4 PixelShaderFunction(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0`
- **Naming:** Uniforms `u`-prefixed (`uTime`, `uColor`, `uIntensity`), functions PascalCase, samplers `s0`-`s7`, techniques named `Technique1`
- **Primitive UV correction (WotG):** `coords.y = (coords.y - 0.5) / input.TextureCoordinates.z + 0.5;`
- **Edge fading (every trail/beam):** `smoothstep(0.0, 0.15, coords.y) * smoothstep(1.0, 0.85, coords.y)` + tip fade: `smoothstep(1.0, 0.7, coords.x)`
- **LUT color:** `float4 themed = tex2D(colorRamp, float2(tex2D(baseTex, coords).r, 0));`
- **Additive-safe:** Ensure black background where no effect — black = invisible in additive blend
- **Before writing a new shader:** (1) Check `Effects/` for existing shaders, (2) Check `ShaderSource/HLSLLibrary.fxh` for shared utilities, (3) Check theme-specific shader dirs, (4) Search reference repos
- **Compilation:** See `ShaderSource/README_SHADER_COMPILATION.md`

**SDF Shape Reference:**

| Shape | SDF |
|-------|-----|
| Circle | `length(p) - radius` |
| Ring | `abs(length(p) - radius) - thickness` |
| Box | `max(abs(p.x) - w, abs(p.y) - h)` |
| Hexagon | `max(abs(p.x) * 0.866 + p.y * 0.5, abs(p.y)) - radius` |
| Star | Polar: `cos(N * atan2(p.y, p.x)) * radius` blended with circle |

Use `smoothstep(0.01, -0.01, sdf)` for anti-aliased SDF edges.

**Noise + Ramp + Mask Reuse Matrix:**

One well-parameterized shader serves dozens of effects. Change the noise texture and color ramp, not the shader code.

| Noise | Color Ramp | Mask | Result |
|-------|-----------|------|--------|
| Perlin | Fire | Circle SDF | Fireball aura |
| Perlin | Ice | Circle SDF | Frost orb |
| Voronoi | Void | Ring SDF | Portal/rift |
| FBM | Energy | Screen UV | Energy field |
| Marble | Blood | Radial fade | Pulsing blood aura |
| Simplex | Rainbow | Star SDF | Prismatic sigil |

### Weapon Content

- Every weapon MUST implement `ModifyTooltips` with effect descriptions + themed lore line with correct `OverrideColor`
- Tooltip style: vanilla Terraria — informative, clean. No ALL CAPS emphasis. Sentence case.
- Lore text wrapped in single quotes, matching theme emotional core
- Each weapon fully responsible for its own VFX (trail, particles, bloom, impacts)
- Lore themes per theme: La Campanella=fire/bells/passion, Eroica=heroism/sacrifice/glory, Swan Lake=grace/feathers/elegance, Moonlight Sonata=moonlight/tides/silver/sorrow, Enigma=mystery/dread/arcane, Fate=destiny/cosmos/inevitability, Clair de Lune=shattered time/blazing clocks/destruction, Dies Irae=hellfire/retribution/divine judgment, Nachtmusik=golden twinkling/starlit melodies/sweet songs, Ode to Joy=chromatic glass roses/prismatic radiance/eternal symphony
- Uniqueness mandate dimensions: attack pattern, trail/swing VFX, impact effect, special mechanic — all 4 must differ within same theme+class

### Projectile Content

- **Minimum:** Additive bloom layer OR custom shader (no bare `Main.EntitySpriteDraw`)
- **Standard:** Bloom + trail (afterimage, primitive, or particle wake)
- **Quality:** Bloom + trail + impact particles + screen response
- Trail required for any projectile traveling >5px/frame
- Trail must fade at tail (no abrupt cutoff), width should taper
- Trail type matches character: energy=bloom trail, physical=afterimage, magical=particle wake
- Impact minimum: 5+ particle burst + brief bloom flash
- Never hardcode colors outside the established theme palette

| Projectile Tier | Max Active | Particles/Proj | Trail Points |
|----------------|-----------|---------------|--------------|
| Bullet/fast | 20+ | 0-2 | 8-12 |
| Standard | 10-15 | 3-5 | 15-20 |
| Complex shader | 5-8 | 5-10 | 20-30 |
| Boss signature | 1-3 | 10-20 | 30-50 |

### Lighting & Atmosphere

- `Lighting.AddLight()` for all glowing elements, animated to match VFX pulses
- Pulsing light: `float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * speed);` (idle speed 2-4)
- **Bloom stacking** (3 layers minimum for rich glow):
  - Core: 0.3-0.5x scale, 80-100% opacity (desaturate toward white at core)
  - Inner: 1.0x scale, 40-60% opacity
  - Outer: 2.0-3.0x scale, 15-30% opacity
  - Bloom should pulse subtly with the effect's animation
- All bloom drawn with `BlendState.Additive`
- Auras: draw behind entity (PreDraw), noise-masked edges, slow rotation (0.01 rad/frame), ±5-10% scale pulse
- Max 3 bloom layers per entity, max 1 god ray, max 1 screen tint
- Screen tinting: max 30% opacity combat, 60% cinematic; always lerp in/out (never snap); durations 0.3-0.5s impacts, 2-5s transitions
- God rays: reserve for dramatic moments, emanate from logical source, fade within 1-2s
- Fog/mist: scroll via UV offset (not particles), layer 2-3 planes at different speeds for parallax, 10-25% opacity, max 2 fog layers per screen region
- Vignette: per-theme edge color (never default black for all), 20-40% intensity for atmosphere, 50-70% for critical moments

### Draw Layer Ordering

1. **DrawBehind** — Behind all entities (arena backgrounds, ambient fog)
2. **PreDraw** — Behind specific entity (auras, bloom/glow)
3. **Draw** — Entity itself
4. **PostDraw** — In front of entity (particles, trails, overlays)
5. **Screen-space** — After all world drawing (distortion, aberration, vignette)

### Screen Effects Safety

- Flash max opacity: **70%** (never pure white fullscreen)
- Flash max duration: **0.15s** (3 frames at 60fps)
- Min gap between flashes: **0.5s**
- Chromatic aberration max: **8px** impacts, **4px** ambient; always centered on effect source, duration 0.1-0.3s impact, continuous 1-2px boss phases
- Screen shake max: **12px** (boss death = 20px exception)
- All screen effects decay smoothly — use dampened oscillation (`amplitude * sin(t * freq) * exp(-decay * t)`), not random jitter
- **Distortion exclusion zones** mandatory for UI (health/mana top-left, hotbar bottom-center, minimap top-right, boss bar)
- Max simultaneous screen effects: 3 (4 fullscreen shader passes allowed during cinematics only)
- Graceful off-switch below 30 FPS
- Cinematic timing: anticipation (0.3-0.5s) → impact (0.1-0.2s) → aftermath (0.5-1.0s) — never skip anticipation
- Render targets: max 2 full-screen RTs simultaneously, prefer half-resolution for blur, dispose when effects end, re-create on screen resize

**Screen Shake Patterns:**

| Context | Intensity | Duration | Notes |
|---------|-----------|----------|-------|
| Light hit | 2-3px | 0.1s | Single impulse |
| Strong hit | 4-6px | 0.15s | Quick decay |
| Explosion | 6-10px | 0.3s | Rapid frequency |
| Boss phase change | 8-12px | 0.5-1.0s | Slow rumble |
| Boss death | 12-20px | 1.0-2.0s | Escalating then fade |

### Particle System

- MagnumParticleHandler: 2000 max, separate blend mode lists, 3000px frustum culling
- Texture loading: use `ModContent.Request<Texture2D>(path)`, cache references in `Asset<Texture2D>` fields assigned in `SetStaticDefaults()` — never load every frame
- Particle budgets: 200-400 active during normal gameplay, up to 1500 max during boss fights
- Dust spawning standards:

| Context | Count | Velocity | Scale |
|---------|-------|----------|-------|
| Idle glow | 1-2/frame | 0.5-1.5 | 0.4-0.8 |
| Swing trail | 2-4/frame | 1-3 | 0.6-1.2 |
| Impact burst | 8-15 (one-shot) | 2-6 | 0.8-1.6 |
| Explosion | 15-30 (one-shot) | 3-8 | 1.0-2.0 |
| Boss death | 30-60 (one-shot) | 4-12 | 1.0-3.0 |

- Music note particles: scale variation 0.3-1.5x, gentle spin ±0.02 rad/frame, spawn at 80% opacity fading to 0%, tinted with theme colors
- Music note sprites: CursiveMusicNote, MusicNote, MusicNoteWithSlashes, QuarterNote, TallMusicNote, WholeNote
- Dust behavior patterns: Orbital (decaying radius around center), Radial burst (evenly distributed outward), Directional spray (cone emission), Rising embers (upward drift + horizontal wander), Gravity fall (spawn upward, arc down)
- ModDust alpha convention: in Terraria, alpha 0=visible, 255=invisible

## Foundation Weapons

Browse `Content/FoundationWeapons/` for reusable rendering patterns:

| Foundation | Use For |
|-----------|---------|
| AttackFoundation | Base projectile patterns, 5-mode attack cycling |
| AttackAnimationFoundation | Cinematic timed sequences, camera + VFX sync |
| SwordSmearFoundation | 3-layer melee sword smear VFX |
| ThinSlashFoundation | Precision slash arcs (SDF-based) |
| XSlashFoundation | Cross-slash patterns |
| LaserFoundation | Full-width laser beams |
| ThinLaserFoundation | Thin precision laser lines |
| InfernalBeamFoundation | Fire/energy beams |
| MagicOrbFoundation | Glowing sphere projectiles (shader-masked) |
| SparkleProjectileFoundation | Bloom-rendered projectiles (Photoviscerator-style) |
| RibbonFoundation | 10 flowing ribbon trail modes |
| ImpactFoundation | Multi-layered hit effects (3 shader types) |
| ExplosionParticlesFoundation | CPU physics radial burst particles |
| SmokeFoundation | Soft billowing smoke lifecycle |
| MaskFoundation | Alpha mask shaping |
| Foundation4PointSparkle | 4-pointed sparkle particles |
| FoundationIncisorOrbs | Orbiting sub-projectiles |

## VFX Compositing Vocabulary

### Noise Types

| Noise | Character | Use For |
|-------|-----------|---------|
| Perlin | Smooth, flowing, cloud-like | Gentle wind, calm water, soft magic |
| FBM | Layered, turbulent | Fire, roiling energy, storm clouds |
| Voronoi | Cell-like, crystalline | Ice fractures, shattered glass, geometric energy |
| Marble | Veined, directional | Liquid, blood flow, cosmic swirls |
| Cosmic/Nebula | Space-like, vast | Celestial effects, astral energy |

### Quality Standards

1. **Visual Depth** — Perceivable difference between core and edge. Not flat.
2. **Theme Coloring** — Colors read as belonging to their theme at a glance.
3. **Edge Quality** — Intentional edges (smoothstep, masks, SDF, feathered). No raw texture cutoffs.
4. **Motion/Life** — Something moves, pulses, scrolls, oscillates. Static = dead.
5. **Proportional Complexity** — Passive < swing < finisher < boss transition.

### Anti-Patterns

- Single-pass flatness (one texture, one tint)
- Noise on everything (crystalline effects should NOT be noise-distorted)
- Wrong blend mode (glow in AlphaBlend = dark edges; smoke in Additive = too ethereal)
- Copy-paste VFX (identical effects + color swap = bland)
- Inventing from scratch without checking reference repos
- Over-engineering simple moments; under-engineering signature moments

## Musical Conventions

- **Combo phases as musical movements:** Allegro → Vivace → Andante → Presto
- **Swing phrasing:** Windup (breath) → Main swing (note) → Follow-through (resonance)
- **Color as dynamics:** Darkest = pianissimo, brightest = fortissimo
- **Boss encounters as performances:** Spawn=Overture, Windups=Tension, Attack=Crescendo, Enrage=Finale

## Custom Slash Commands

| Command | Purpose |
|---------|---------|
| `/new-weapon-vfx` | Full weapon VFX design from scratch (creative direction → implementation) |
| `/audit-weapon-quality` | Evaluate weapon/boss VFX quality with 6-dimension creativity scoring |
| `/design-boss-phase` | Design boss encounter VFX across all phases |
| `/design-impact` | Design multi-layered hit effects, crit flashes, death bursts |
| `/design-projectile` | 10-step projectile design from behavior to rendering |
| `/design-weapon-identity` | Deep weapon concept design with 15+ interactive questions |
| `/audit-vfx-creativity` | Audit VFX diversity across a theme's weapon set |
