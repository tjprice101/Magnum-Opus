# Beam Textures

Textures for rendering **continuous energy beams, lasers, and channeled streams**. These are UV-mapped along beam geometry — a series of quads stretching from a source point (weapon/hand) to a target point or max range.

## What Belongs Here

- **Beam core segments** — Bright inner core textures, tileable horizontally for beam body
- **Beam outer glow segments** — Softer, wider glow textures layered around the core for beam halo
- **Muzzle flare origins** — Bright burst textures drawn at the beam's origin point (weapon tip)
- **Beam endpoint splashes** — Impact textures drawn where the beam hits a surface or NPC
- **Beam noise / distortion overlays** — Scrolling noise patterns layered onto beams for turbulence

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Orientation** | Horizontal — beam flows left to right. Left = origin, right = endpoint |
| **Tiling** | Core and outer glow segments MUST tile seamlessly on the X axis (they repeat along beam length) |
| **Color space** | White/grayscale on black. Beam color comes from shader tinting |
| **Dimensions** | Width: 128–512px (tiling length). Height: 32–128px (beam thickness) |
| **Background** | Solid black (#000000) for additive blending |
| **Format** | PNG |

## How These Are Used

Beam rendering pipeline:
1. C# code defines beam start/end positions and constructs a strip mesh between them
2. The **core texture** is drawn first — tiled along the beam length with UV scrolling for animation
3. The **outer glow texture** is drawn slightly larger around the core for soft halo
4. A **muzzle flare** is drawn at the origin point, scaled and rotated
5. An **endpoint splash** is drawn where the beam terminates
6. Noise textures (from `NoiseTextures/`) are used to distort the beam's UVs for organic movement

Beam textures are drawn in **additive blend mode** and typically use the weapon's theme color palette.

## Current Inventory

*This folder is currently empty — awaiting new beam textures.*

### Needed Asset Types
- Tileable beam core (256×64, bright center line with falloff)
- Tileable beam outer glow (256×128, soft and wide)
- Muzzle flare origin sprite (64×64, bright radial burst)
- Beam endpoint splash (64×64, impact dispersal)
