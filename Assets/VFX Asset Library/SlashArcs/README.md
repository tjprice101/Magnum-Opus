# Slash Arcs

Swing arc and smear textures for **melee weapon visual effects**. These textures represent the visible trail left by a blade's swing — the sweeping arc of light, energy, or elemental force that follows the weapon's path.

## What Belongs Here

- **Full crescent arcs** — Wide curved arc textures spanning 90–180° for standard sword swings
- **Thin edge slashes** — Narrow, sharp arc lines for precise or fast cuts
- **Double-layer smears** — Two overlapping arc layers (bright core + soft outer glow) for depth
- **Full rotation arcs** — 360° arc textures for spinning attacks or special moves
- **Smear effect strips** — Motion-blur-style smeared textures for speed emphasis
- **Musical arc variants** — Arcs incorporating musical motifs (staff lines, note shapes, frequency waves woven into the arc form)

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | White/grayscale on solid black background. Weapon theme color is applied at runtime |
| **Orientation** | Arc should curve from BOTTOM-LEFT to TOP-RIGHT (standard right-hand swing). Mirroring is done in code |
| **Dimensions** | 256×256 or 512×512 for full arcs. Must be square for rotation |
| **Arc center** | The pivot point (player's hand position) should be near the bottom-center of the texture |
| **Background** | Solid black (#000000) for additive blending |
| **Format** | PNG |

## How These Are Used

Slash arc rendering pipeline:
1. A melee weapon's swing projectile determines the current swing angle and progress
2. A screen-space quad is drawn at the player's position with this arc texture
3. The quad is **rotated** to match the swing's current angle
4. **Opacity fades** over the lifetime of the swing (bright at start, fading at follow-through)
5. Multiple arcs may layer: a thin bright core arc + a wide soft glow arc for richness
6. The texture is tinted with the weapon's theme color palette

Arc textures pair well with **trail strip textures** (from `TrailsAndRibbons/`) — the arc texture handles the broad swing visual while trails handle the blade tip's path.

## Current Inventory

*This folder is currently empty — awaiting new slash arc textures.*

### Needed Asset Types
- Wide crescent arc slash (256×256, ~120° sweep)
- Thin sharp edge slash (256×256, narrow bright line)
- Double-layer smear (256×256, bright core + soft outer)
- Full 360° rotation arc (256×256)
- Musical staff-line arc variant (256×256, arc with staff lines woven in)
