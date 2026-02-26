# VFX Asset Library

Central repository of **visual effect texture assets** for MagnumOpus. Every texture in this library is used by shaders, renderers, and VFX systems to create the mod's visual effects  Etrails, bloom, impacts, beams, screen distortions, and more.

> **For AI agents:** Each subfolder has its own README.md describing exactly what kinds of assets it holds, their technical requirements, and what's currently available vs. needed. Read the category README before adding or searching for assets.

---

## Folder Structure

```
VFX Asset Library/
├── NoiseTextures/       ↁEProcedural noise maps sampled by shaders (perlin, voronoi, FBM, etc.)
├── TrailsAndRibbons/    ↁEUV strip textures for trail primitive rendering (comet tails, energy ribbons)
├── GlowAndBloom/        ↁESoft glow orbs, lens flares, god rays, bloom overlays (additive blend)
├── ImpactEffects/       ↁEHit-reaction bursts: shockwaves, star bursts, slash marks, explosions
├── BeamTextures/        ↁETileable beam core/glow segments, muzzle flares, beam endpoints
├── ScreenEffects/       ↁEScreen-space overlays: distortion maps, chromatic aberration, speed lines
├── ColorGradients/      ↁELUT textures, 1D color ramps, theme palette gradient strips
├── MasksAndShapes/      ↁEAlpha masks, intensity masks, shape stencils for shader masking
├── SlashArcs/           ↁEMelee swing arc overlays, crescent slashes, smear effect textures
└── Lightning/           ↁEElectrical arc bolts, branch patterns, spark discharge textures
```

---

## Quick Reference: Where Does This Asset Go?

| If the texture is... | Put it in... |
|---|---|
| A noise pattern used by shaders for distortion/dissolve/fog | `NoiseTextures/` |
| A strip that gets stretched along a trail path | `TrailsAndRibbons/` |
| A soft glowing shape for bloom/flare/god ray overlay | `GlowAndBloom/` |
| A burst/flash drawn at a hit point | `ImpactEffects/` |
| A tileable segment for beam body rendering | `BeamTextures/` |
| A full-screen distortion/aberration/speed line overlay | `ScreenEffects/` |
| A gradient ramp or LUT for shader color mapping | `ColorGradients/` |
| An alpha mask that controls where an effect appears | `MasksAndShapes/` |
| A swing arc or smear texture for melee attacks | `SlashArcs/` |
| A lightning bolt, arc, or electrical discharge | `Lightning/` |

---

## Universal Technical Rules

These rules apply to **all** textures in this library:

1. **Color space**: White/grayscale on solid black background unless the README for that category says otherwise. Color tinting is applied by shaders at runtime using the weapon's theme palette.

2. **Dimensions**: Always power-of-two (64, 128, 256, 512). GPU texture sampling requires this for proper mipmapping and filtering.

3. **Format**: PNG. No JPEG (lossy compression creates artifacts in shader-sampled textures).

4. **No baked-in color**: Textures should be grayscale/white so they work with ANY theme's color palette. The shader multiplies color at draw time.

5. **Background**: Solid black (#000000) for textures drawn with additive blending (bloom, impacts, arcs). Transparency for textures drawn with alpha blending (masks, some trails).

---

## How VFX Textures Relate to Other Asset Directories

| Directory | Relationship to VFX Asset Library |
|---|---|
| `Assets/Particles Asset Library/` | **Particle sprites**  Eindividual images drawn as discrete particles by the CPU particle system. VFX textures are shader-driven data, not individual sprites |
| `Effects/` | **Shader code** (.fx/.fxc)  Ethe shaders that CONSUME these textures. Each shader references specific textures from this library |
| `Content/<Theme>/<Item>/` | **Per-weapon assets**  Eweapon-specific textures live with their weapon code, not here. This library holds SHARED/REUSABLE textures |
| `Assets/SandboxLastPrism/` | **Reference implementation**  Edemonstrates per-weapon asset organization (separate from this shared library) |

---

## Asset Naming Conventions

- Use **PascalCase** descriptive names: `SoftCircularCaustics.png`, `TileableFBMNoise.png`
- Prefix with category hint when ambiguous: `HorizontalEnergyGradient.png`, `CosmicNebulaClouds.png`
- Avoid generic names like `texture1.png` or `new.png`
- Include dimensionality or variant number for sets: `PerlinNoise_256.png`, `RippleRing_3Ring.png`
