---
applyTo: "Content/**/Projectiles/**,Content/**/*Projectile*.cs,Content/**/*Beam*.cs,Content/**/*Laser*.cs,Content/**/*Orb*.cs"
---

# Projectile Content Conventions

## Rendering Requirements

Every projectile must have visual rendering beyond a plain sprite draw:
1. **At minimum**: Additive bloom layer (multi-scale glow stacking) OR custom shader
2. **Standard**: Bloom + trail (afterimage, primitive, or particle wake)
3. **Quality**: Bloom + trail + impact particles + screen response (shake/flash)

**No bare `Main.EntitySpriteDraw()` without additional layers.** A single unlit sprite is not acceptable for MagnumOpus projectiles.

## Trail Standards

- Projectiles traveling more than 5px/frame MUST have a visual trail
- Trail type should match projectile character: energy = bloom trail, physical = afterimage, magical = particle wake
- Trails must fade at the tail end (no abrupt cutoff)
- Trail width should taper (thick at projectile, thin at tail)

## Impact Requirements

All projectiles must have on-hit visual feedback:
- **Minimum**: Particle burst (5+ particles) + brief bloom flash
- **Standard**: Particle burst + bloom flash + small screen shake
- **Quality**: Multi-layer burst + flash + shake + themed particles

## Performance Budgets

| Projectile Tier | Max Active | Particles/Proj | Trail Points |
|----------------|-----------|---------------|--------------|
| Bullet/fast | 20+ | 0-2 | 8-12 |
| Standard | 10-15 | 3-5 | 15-20 |
| Complex shader | 5-8 | 5-10 | 20-30 |
| Boss signature | 1-3 | 10-20 | 30-50 |

## SpriteBatch State Management

When drawing projectile VFX with shaders:
```csharp
// 1. End current SpriteBatch
Main.spriteBatch.End();
// 2. Begin with Immediate + Additive for shader application
Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, ...);
// 3. Apply shader
shader.CurrentTechnique.Passes[0].Apply();
// 4. Draw
Main.spriteBatch.Draw(...);
// 5. End and restore
Main.spriteBatch.End();
Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, ...);
```

## Theme Color Compliance

Projectile colors MUST use the theme palette. Never hardcode colors outside the established palette. Reference `copilot-instructions.md` for theme color tables.

## Musical Integration

Where appropriate, projectiles should incorporate musical elements:
- Music note particles in trail or on impact
- Harmonic pulse on projectile body
- Rhythmic spawn patterns for multi-projectile weapons
