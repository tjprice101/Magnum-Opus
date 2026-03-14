---
applyTo: "Common/Systems/VFX/**,Common/Systems/Particles/**,Common/Systems/Shaders/**,Common/Systems/Metaballs/**,Content/**/Bosses/**"
---

# VFX System Conventions — MagnumOpus

## SpriteBatch State Management

**Critical rule:** Always restore SpriteBatch state after modifying it. Failing to do so corrupts rendering for everything drawn after.

### Pattern: Save → Modify → Restore
```csharp
// Save current state by ending the active batch
spriteBatch.End();

// Begin with your desired state
spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearWrap,
    DepthStencilState.None, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

// Draw your effect
// ...

// Restore original state
spriteBatch.End();
spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState,
    DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
```

### Common Blend States

| Blend State | Use For |
|------------|---------|
| `BlendState.Additive` | Glow, energy, fire, bloom, trails, any luminous effect |
| `BlendState.AlphaBlend` | Smoke, solid shapes, UI elements, anything that should occlude |
| `MagnumBlendStates.*` | Custom blend states defined in `Common/MagnumBlendStates.cs` |

**Additive makes black invisible.** VFX textures on black backgrounds must use Additive blending. Using AlphaBlend on such textures shows ugly dark rectangle edges.

### Sampler States

| Sampler | Use For |
|---------|---------|
| `SamplerState.LinearWrap` | UV-scrolling textures that tile (trails, beams) |
| `SamplerState.LinearClamp` | Non-tiling textures (sprites, bloom) |
| `SamplerState.PointClamp` | Pixel-art textures (preserve sharp edges) |

## Draw Layer Ordering

Effects render in specific layers. Using the wrong layer causes visual ordering issues:

1. **PreDraw** hooks — Before the entity draws (behind it)
2. **Draw** — The entity itself
3. **PostDraw** hooks — After the entity draws (in front of it)
4. **DrawBehind** — Explicitly behind all entities
5. **Screen-space effects** — Applied to the entire screen after all world drawing

Bloom/glow typically goes in PreDraw (behind the entity). Particles and trails typically go in PostDraw.

## Particle System Usage

### MagnumParticleHandler
- Max 2000 active particles
- Use `CommonParticles.cs` for shared particle types
- Separate blend mode lists — register particles with correct blend mode
- 3000px frustum culling — particles beyond this are auto-removed

### Creating New ModDust
Place in the weapon's folder: `Content/<Theme>/<Category>/<Weapon>/Dusts/`
- Sprite PNG in `Dusts/Textures/` subfolder
- Each dust needs: `SetStaticDefaults()`, `OnSpawn()`, `Update()`, `Draw()` if custom rendering
- Set `UpdateType` for particle behavior inheritance
- Clean up properly — ensure particles die (don't leak)

## Performance Guidelines

### Texture Loading
- Textures are loaded via `ModContent.Request<Texture2D>(path)`
- Cache texture references — don't load every frame
- Use `Asset<Texture2D>` fields assigned once in `SetStaticDefaults()`

### Render Target Management
- Reuse render targets — don't allocate new ones per frame
- Clear targets before use
- Dispose targets properly in `Unload()`
- For intensive effects, consider half-resolution targets

### Particle Budgets
- Normal gameplay: 200-400 active particles
- Boss fights: up to 1500 max
- Frustum cull aggressively for offscreen particles

## Lighting & Dust Conventions

### Dynamic Lighting

Every visible VFX element that glows should call `Lighting.AddLight()`:
- Match light color to the effect's dominant color
- Animate intensity with the effect (pulse, fade, flash)
- Scale radius to the visual size — don't over-light small effects

### Dust Spawning Standards

| Context | Dust Count | Velocity Range | Scale Range |
|---------|-----------|---------------|-------------|
| Idle glow | 1-2/frame | 0.5-1.5 | 0.4-0.8 |
| Swing trail | 2-4/frame | 1-3 | 0.6-1.2 |
| Impact burst | 8-15 (one-shot) | 2-6 | 0.8-1.6 |
| Explosion | 15-30 (one-shot) | 3-8 | 1.0-2.0 |
| Boss death | 30-60 (one-shot) | 4-12 | 1.0-3.0 |

### Dust Behavior Patterns

- **Orbital**: Dust orbits a center point with decaying radius
- **Radial burst**: Evenly distributed outward from center
- **Directional spray**: Cone-shaped emission in a direction
- **Rising embers**: Upward drift with slight horizontal wander
- **Gravity fall**: Spawn upward, arc down naturally

### Music Note Particle Integration

Music note particles from `Assets/Particles Asset Library/` should be used creatively:
- Scale variation: 0.3x–1.5x for depth (smaller = further away)
- Rotation: Gentle spin (±0.02 rad/frame) for floating feel
- Opacity fade: Spawn at 80%, fade to 0% over lifetime
- Color tinting: Use theme palette colors, not white

## Bloom Stacking Standards

Rich glow requires multiple additive layers at different scales:

```
Layer 1: Core (0.3-0.5x scale, 80-100% opacity) — bright hot center
Layer 2: Inner glow (1.0x scale, 40-60% opacity) — main body
Layer 3: Outer glow (2.0-3.0x scale, 15-30% opacity) — soft ambient
```

- All bloom layers use `BlendState.Additive`
- Color should desaturate toward white at the core
- Bloom should pulse subtly with the effect's animation

## Atmosphere Standards

### Auras & Ambient Glow
- Draw behind the entity (PreDraw layer)
- Use noise masking for organic edges
- Rotate slowly (0.01 rad/frame) for visual interest
- Pulse scale ±5-10% for breathing effect

### Screen-Space Effects Routing
For screen distortion, chromatic aberration, vignette, and camera effects, see `screen-effects.instructions.md`. For god rays, fog, and environmental atmosphere, see `lighting-effects.instructions.md`.
- Use LOD — reduce counts at lower quality settings

## Existing Systems — Check Before Building New

| System | Location | Purpose |
|--------|----------|---------|
| ShaderLoader | `Common/Systems/Shaders/ShaderLoader.cs` | Loads and manages all shaders |
| ShaderRenderer | `Common/Systems/Shaders/ShaderRenderer.cs` | Shader rendering utilities |
| MagnumParticleHandler | `Common/Systems/Particles/` | Particle lifecycle management |
| MetaballManager | `Common/Systems/Metaballs/` | Metaball fusion rendering |
| VFX/Trails/* | `Common/Systems/VFX/Trails/` | 7+ trail rendering systems |
| VFX/Bloom/* | `Common/Systems/VFX/Bloom/` | Bloom, lens flare, god rays |
| VFX/Boss/* | `Common/Systems/VFX/Boss/` | Boss arena, telegraphs, cinematics |
| VFX/Screen/* | `Common/Systems/VFX/Screen/` | Skybox, distortion, heat effects |
| VFX/Effects/* | `Common/Systems/VFX/Effects/` | Afterimages, glow dust, smoke, screen shake |
| VFX/Optimization/* | `Common/Systems/VFX/Optimization/` | LOD, adaptive quality, batching |

Always check these before writing new infrastructure. Reuse and extend existing systems wherever possible.
