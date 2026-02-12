# TestWeapons — Self-Contained Melee Swing Testbed

## Architecture

Each weapon lives in its own folder with **exactly 3 files**:

```
XX_WeaponName/
├── WeaponNameItem.cs        ← ModItem (held weapon, spawns projectile)
├── WeaponNameSwing.cs       ← ModProjectile (IS the swing — held projectile pattern)
└── WeaponNameVFX.cs         ← Static VFX helper (trail drawing, particles, colors)
```

**No global hooks.** No ModPlayer, no GlobalProjectile, no GlobalItem.  
Each weapon is 100% self-contained. Copy a folder → get a working weapon.

## Pattern

1. **Item** uses `useStyle = Swing, noMelee = true, noUseGraphic = true, channel = true`
2. **Item.Shoot** spawns the swing projectile
3. **Swing projectile** uses `PiecewiseAnimation` with `CurveSegment` for the swing arc
4. **Swing projectile** draws itself: blade sprite + trail + lens flare
5. **VFX helper** encapsulates all trail/particle/color logic as static methods

## Key Infrastructure Used

| System | File | What We Use |
|--------|------|-------------|
| `CurveSegment` / `PiecewiseAnimation` | `Common/Systems/Particles/Particle.cs` | Swing arc easing |
| `SwingShaderSystem` | `Common/Systems/VFX/SwingShaderSystem.cs` | `BeginAdditive()`, `RestoreSpriteBatch()`, `GetExobladeColor()` |
| `CalamityStyleTrailRenderer` | `Common/Systems/VFX/Trails/CalamityStyleTrailRenderer.cs` | Shader-based trails (Ice, Fire, Cosmic, etc.) |
| `ShaderLoader` | `Common/Systems/Shaders/ShaderLoader.cs` | Compiled `.fxc` shaders + noise/trail textures |
| `EnhancedTrailRenderer` | `Common/Systems/VFX/Trails/EnhancedTrailRenderer.cs` | Multi-pass primitive trails |
| `BloomRenderer` | `Common/Systems/VFX/Bloom/BloomRenderer.cs` | Multi-layer bloom stacking |
| `MagnumThemePalettes` | `Common/Systems/VFX/Core/MagnumThemePalettes.cs` | Theme color arrays |

## The 5 Test Weapons

| # | Name | VFX Focus | Trail System |
|---|------|-----------|--------------|
| 01 | Infernal Cleaver | 🔥 Fire smoke trail, ember particles, heat-up glow | Manual Exoblade-style (SpriteBatch segment drawing) |
| 02 | Frostbite Edge | ❄️ Ice crystal trail, frost dust, cold mist | `CalamityStyleTrailRenderer` with `TrailStyle.Ice` |
| 03 | Cosmic Rend Blade | 🌌 Constellation trails, screen distortion, star sparkles | `CalamityStyleTrailRenderer` with `TrailStyle.Cosmic` |
| 04 | Verdant Crescendo | 🌿 Leaf particles, vine trail, nature glow | `CalamityStyleTrailRenderer` with `TrailStyle.Nature` |
| 05 | Arcane Harmonics | 🎵 Music note orbits, multi-pass rainbow trail, god rays | `EnhancedTrailRenderer` multi-pass |

## Reference

See `Documentation/Reference/DebugSWINGProj_Reference.cs` for the original gold-standard implementation this architecture is based on.
