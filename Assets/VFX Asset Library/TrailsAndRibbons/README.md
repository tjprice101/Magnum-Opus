# Trails and Ribbons

UV-mapped strip textures designed for **primitive trail rendering systems**. These textures are stretched along a series of connected vertices (a "trail strip") that follow a moving object — a swinging blade, a flying projectile, or a dashing player.

## What Belongs Here

- **Comet / fade trails** — Gradient strips that taper from bright to transparent along the trail length
- **Energy ribbon strips** — Glowing energy patterns tiled along the trail's UV for scrolling effects
- **Dissolving / particle trails** — Strips with scattered particle-like elements that break apart toward the tail
- **Spiral / rotation trails** — Strips encoding spiral or rotation patterns for spinning projectile trails
- **Ember / sparkle scatter strips** — Trail textures with embedded point-light scatter patterns
- **Afterimage / motion blur strips** — Horizontal motion blur streaks used for speed afterimage trails

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Orientation** | Horizontal — left edge = trail origin (hot/bright), right edge = trail tail (faded/transparent) |
| **Color space** | White/grayscale on black or transparent background. Color tinting is applied by shaders at runtime |
| **Dimensions** | Width should be power-of-two (128, 256, 512). Height is typically 32–128px depending on trail thickness |
| **UV mapping** | U axis = along trail length, V axis = across trail width. Design the texture to tile or clamp accordingly |
| **Format** | PNG with alpha transparency where appropriate |

## How These Are Used

The trail rendering pipeline:
1. C# code tracks recent positions of an object, building a strip mesh of connected quads
2. UV coordinates map U=0 at the newest point and U=1 at the oldest
3. A trail shader samples THIS texture using those UVs, often scrolling or distorting them
4. The shader multiplies the texture by a color gradient (from a LUT or theme palette)
5. The result is drawn with additive or alpha blending for the final glowing trail

Trail textures are combined with **noise textures** (from `NoiseTextures/`) for distortion, and **color gradients** (from `ColorGradients/`) for theme-specific palette mapping.

## Current Inventory

*This folder is currently empty — awaiting new trail strip textures.*

### Needed Asset Types
- Basic soft-fade comet trail (128×64, white gradient left-to-right)
- Energy ribbon with inner glow pattern (256×64)
- Dissolving particle scatter trail (256×64)
- Musical staff lines trail strip (256×64, for music-themed weapons)
- Harmonic wave trail pattern (256×64, sine wave modulation)
