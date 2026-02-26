# Masks and Shapes

Alpha masks, intensity masks, and shape textures used by shaders to **control where and how strongly an effect is visible**. These textures act as stencils — white areas = full effect, black areas = no effect, gray = partial.

## What Belongs Here

- **Arc smear masks** — Crescent or arc-shaped alpha masks for constraining swing trail rendering to an arc region
- **Radial intensity masks** — Circular masks with radial falloff for controlling beam width, aura density, or glow distribution
- **Eclipse / ring masks** — Ring-shaped masks for halo effects, orbital paths, and eclipse corona rendering
- **Distortion overlay masks** — Normal-map-style masks that control distortion direction and strength in localized areas
- **Ripple ring masks** — Concentric ring patterns for shockwave and water-ripple shaped masking
- **Trail ribbon masks** — Width-profile masks that shape trail primitives (thin at edges, thick in center)
- **Shape cutouts** — Hard-edged geometric shapes (circles, crescents, stars) used as stencil masks

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | Grayscale. White (255) = full visibility. Black (0) = fully masked/hidden |
| **Dimensions** | Power-of-two: 64×64, 128×128, 256×256 |
| **Edges** | Smooth anti-aliased edges for masks used with blending. Hard edges only for stencil cutouts |
| **Distortion maps** | R channel = X offset, G channel = Y offset. Neutral center = (128, 128) |
| **Format** | PNG |

## How These Are Used

Masks are multiplied against effect textures in shaders:
1. An effect texture (glow, trail, beam) provides the base visual
2. A mask texture is sampled at the same UV coordinates
3. The mask's value **multiplies the effect's opacity** — masking it into a specific shape
4. This is how you get: a circular beam intensity falloff, a crescent-shaped bloom, an arc-constrained trail

Example: A melee swing trail would use an **arc smear mask** to clip the trail texture into a crescent shape matching the swing angle, combined with a **radial intensity mask** to fade it toward the outer edge.

## Current Inventory

*This folder is currently empty — awaiting new mask and shape textures.*

### Needed Asset Types
- Arc smear alpha mask (128×128, crescent/arc shape)
- Soft radial intensity mask (128×128, white center fading to black edge)
- Eclipse ring mask (128×128, dark center with bright ring)
- Ripple ring distortion map (128×128, concentric rings as RG offset)
- Trail width profile mask (256×32, shaped width falloff)
