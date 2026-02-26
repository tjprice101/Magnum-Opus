# Impact Effects

Burst and hit-reaction textures for **moments of contact** — when a projectile hits, a sword connects, an explosion detonates, or a shockwave expands. These textures convey force, energy release, and dramatic impact.

## What Belongs Here

- **Shockwave rings** — Expanding concentric ring textures for hit ripples and ground pounds
- **Star burst impacts** — Multi-pointed star/cross patterns for bright hit flashes
- **Slash impact marks** — Directional cut/slash marks for melee hit feedback
- **Explosion flash bursts** — Bright radial explosion textures for detonation moments
- **Ripple rings** — Softer concentric ripples for magical/harmonic impact waves
- **Combo finisher arcs** — Large dramatic arc textures for final combo hit payoffs
- **X-slash / cross impacts** — Crossed slash patterns for dual-hit or critical strike visuals

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | White/grayscale on solid black background. Tinted at runtime |
| **Orientation** | Centered — the impact point should be at the texture center |
| **Dimensions** | 64×64 (small hits), 128×128 (standard impacts), 256×256 (finisher bursts) |
| **Directionality** | Slash marks should face RIGHT by default — rotation is applied in code |
| **Background** | Solid black (#000000) for additive blending |
| **Format** | PNG |

## How These Are Used

Impact textures are spawned at the moment of collision:
1. A hit is detected (projectile → NPC, melee swing → NPC, etc.)
2. Code spawns a **screen-space quad** at the hit position with this texture
3. The quad rapidly scales up while fading out (typically 5–15 frames lifetime)
4. Multiple impact layers stack: a shockwave ring + a star burst + particle sparks
5. Color is determined by the weapon's theme palette

The best impacts layer **2–3 textures** from this folder simultaneously for richness — e.g., a shockwave ring underneath, a star burst on top, and a directional slash mark oriented to the swing angle.

## Current Inventory

*This folder is currently empty — awaiting new impact effect textures.*

### Needed Asset Types
- Expanding shockwave ring (128×128)
- 4-point star impact burst (128×128)
- Directional hit slash mark (128×64, horizontal, facing right)
- Bright explosion flash (128×128, radial)
- Concentric ripple rings (128×128, 3+ rings)
- Combo finisher arc (256×128)
- X-shaped slash cross (128×128)
