# Lightning

Electrical arc, bolt, and discharge textures for **lightning-based visual effects**. These textures depict branching electrical energy, jagged bolts, and crackling discharge patterns.

## What Belongs Here

- **Lightning bolt strips** — Horizontal bolt textures UV-mapped along a path for rendering lightning between two points
- **Arc branch patterns** — Branching fractal lightning trees for area-of-effect electrical discharge
- **Spark discharge bursts** — Small radial electrical spark patterns for impact moments
- **Chain lightning connectors** — Thinner bolt segments for rendering chain lightning between multiple targets
- **Electrical aura crackling** — Looping electrical noise patterns for persistent crackling aura effects

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | White/grayscale on solid black background. Lightning color (blue, purple, gold, etc.) is applied at runtime |
| **Bolt strips** | Horizontal orientation, tileable on X axis for variable-length bolts |
| **Branch patterns** | Centered, square textures. Branching should radiate from center |
| **Dimensions** | Strips: 256×32 or 256×64. Bursts/branches: 128×128 or 256×256 |
| **Background** | Solid black (#000000) for additive blending |
| **Format** | PNG |

## How These Are Used

Lightning rendering approaches:
1. **Point-to-point bolts**: A strip texture is UV-mapped along a segmented path between source and target. Each frame, the path's control points are randomly offset for a flickering, jagged look
2. **Area discharge**: A branch pattern texture is drawn at an impact point, scaled up and rapidly fading, possibly with random rotation each frame
3. **Chain lightning**: Multiple thin bolt strips connect a sequence of targets, each segment independently animated
4. **Aura crackling**: A looping pattern is drawn around an entity, with UV scrolling and random intensity pulses

Lightning textures are often combined with **glow textures** (from `GlowAndBloom/`) for bright flash at the bolt origin and termination points.

## Current Inventory

*This folder is currently empty — awaiting new lightning textures.*

### Needed Asset Types
- Lightning bolt strip (256×64, jagged bolt, tileable X)
- Arc branch pattern (128×128, branching from center)
- Small spark discharge burst (64×64)
