# Screen Effects

Full-screen or large-area overlay textures used for **post-processing and screen-space effects**. These textures modify or distort what's already rendered on screen — they don't represent objects in the game world.

## What Belongs Here

- **Distortion maps** — Normal-map-style textures that offset screen pixels for heat haze, shockwave ripple, and spatial distortion
- **Chromatic aberration maps** — Textures that drive RGB channel separation for dramatic moments
- **Speed / zoom lines** — Radial line patterns overlaid during dashes, charges, or boss phase transitions
- **Vignette overlays** — Edge-darkening or edge-coloring frames for mood and atmosphere
- **Screen flash textures** — Full-screen flash patterns for critical hits, phase changes, and finishers
- **Color grading overlays** — Screen-tint textures for temporary mood shifts (damage flash, power-up glow)

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Dimensions** | Typically match screen aspect: 256×256 or 512×512 for distortion maps. Radial effects should be square |
| **Distortion maps** | RGB channels encode XY offset direction. R = X offset, G = Y offset. Neutral = (128, 128, 128) |
| **Speed lines** | White lines on black background. Radial from center |
| **Vignettes** | Black edges fading to transparent center |
| **Format** | PNG |

## How These Are Used

Screen effects are applied as a **post-processing pass** after all game rendering:
1. The game renders normally to a render target
2. A screen-effect shader samples THIS texture to determine how to modify each pixel
3. **Distortion**: Offset each pixel's sample position by the texture's RG values → creates ripple/haze
4. **Chromatic aberration**: Split R/G/B channels by different offsets → creates prismatic fringing
5. **Speed lines**: Draw as additive overlay, scaled/rotated from screen center → creates rush feeling
6. **Vignette**: Multiply screen by this texture → darkens edges for focus

These effects are typically **brief and animated** — a shockwave distortion that expands and fades over 15 frames, a flash that lasts 3 frames, speed lines during a 0.5-second dash.

## Current Inventory

*This folder is currently empty — awaiting new screen effect textures.*

### Needed Asset Types
- Radial distortion ring map (256×256, for shockwave ripple)
- Chromatic aberration base layer (256×256)
- Radial speed/zoom lines (256×256, white lines from center on black)
- Soft vignette overlay (256×256)
