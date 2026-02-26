# Glow and Bloom

Soft luminous textures drawn with **additive blending** to create glow, bloom, lens flare, and god ray effects. These are overlaid on top of gameplay elements to add radiance, atmospheric lighting, and visual weight to important moments.

## What Belongs Here

- **Soft glow circles** — Feathered circular orbs with smooth radial falloff for point-light bloom
- **Lens flares** — Multi-element flare textures (4-point, 6-point, complex) for weapon glints and light sources
- **Bokeh arrays** — Hexagonal or circular bokeh patterns for depth-of-field style bloom
- **Anamorphic streaks** — Horizontal light streaks simulating anamorphic lens artifacts
- **God rays / light shafts** — Radial or directional light beam textures for dramatic atmospheric lighting
- **Light cones** — Focused directional light cone shapes for spotlights and beam origins
- **Concentric glow stacks** — Multi-ring nested glow circles for layered bloom depth

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | White/grayscale on solid black background. Color is applied at draw time |
| **Falloff** | Smooth radial or directional falloff — no hard edges. The softness IS the effect |
| **Dimensions** | 64×64 (small accents), 128×128 (standard), 256×256 (large area bloom) |
| **Background** | Solid black (#000000). Additive blending makes black = invisible |
| **Format** | PNG, no alpha channel needed (black = transparent under additive blend) |

## How These Are Used

Bloom and glow textures are drawn as **screen-space quads** positioned at world locations:
- A weapon's blade tip gets a small glow circle scaled by swing speed
- A projectile's core gets a medium bloom that pulses with a sine wave
- An impact point gets a large flash bloom that rapidly fades
- God rays emanate from boss spawn points or musical crescendo moments
- Lens flares appear at weapon glint points, rotating with viewing angle

These are typically drawn in a **dedicated additive render layer** after the main game rendering, so they stack and blend naturally.

## Current Inventory

*This folder is currently empty — awaiting new glow and bloom textures.*

### Needed Asset Types
- Small soft circular bloom (64×64, gentle Gaussian falloff)
- Medium soft circular bloom (128×128)
- Large area bloom (256×256, very soft edges)
- 4-point star flare (64×64, cross pattern)
- 6-point star flare (64×64)
- Horizontal anamorphic streak (128×32)
- Radial god ray circle (256×256, rays emanating from center)
- Single directional light shaft (256×128)
- Multi-ring concentric glow (128×128)
