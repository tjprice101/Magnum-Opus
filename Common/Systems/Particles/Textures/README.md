# Particle Texture Requirements

This folder should contain the following texture assets for the particle system.
All textures should be PNG format with transparency (alpha channel).

## Required Textures

### BloomCircle.png
- **Size**: 64x64 pixels (recommended)
- **Description**: A soft, circular glow/bloom effect. Should be a white circle with gradual fade from center (fully opaque) to edges (fully transparent). This creates the classic "bloom" or "glow orb" effect.
- **Usage**: Energy particles, glowing effects, light orbs

### BloomRing.png
- **Size**: 64x64 pixels (recommended)
- **Description**: A ring/donut shape with soft edges. White color, transparent center and outer edges. Only the ring itself should be visible.
- **Usage**: Expanding ring effects, impact waves, aura effects

### Sparkle.png
- **Size**: 32x32 pixels (recommended)
- **Description**: A 4-pointed or 8-pointed star/sparkle shape. White color with soft edges.
- **Usage**: Twinkling stars, magical sparkles, pickup effects

### GlowSpark.png
- **Size**: 48x48 pixels (recommended)
- **Description**: An elongated spark or flame-like shape pointing upward. Should have a bright core with soft glow falloff.
- **Usage**: Sparks, fire particles, energy trails

### SoftGlow.png
- **Size**: 32x32 pixels (recommended)
- **Description**: Similar to BloomCircle but smaller and possibly with a sharper core. A general-purpose glow texture.
- **Usage**: Generic glowing particles, ambient effects

### Point.png
- **Size**: 8x8 pixels (recommended)
- **Description**: A simple, small bright dot. Can be a tiny soft circle or a sharp point.
- **Usage**: Distant particles, subtle ambient dust, small stars

### HeavySmoke.png
- **Size**: 64x64 pixels (recommended)
- **Description**: A soft, cloud-like smoke/fog shape. Should be irregular/organic looking, not perfectly circular.
- **Usage**: Smoke effects, dust clouds, explosion debris

## Color Notes

All textures should be **white or grayscale**. The particle system applies color tinting at runtime, so white textures allow for the most flexibility in coloring effects.

## Additional Tips

1. **Anti-aliasing**: Use soft edges and anti-aliasing for smooth rendering
2. **Power of 2**: Sizes like 32x32, 64x64, 128x128 are most efficient
3. **Premultiplied alpha**: Consider using premultiplied alpha for additive blending compatibility
4. **Center origin**: Most particles assume the texture origin is at center

## Temporary Fallback

If textures are missing, the particle system will fall back to using Terraria's built-in `MagicPixel` texture, which will display as simple colored squares instead of the intended effects.

## Creating Simple Textures

You can create basic versions of these textures using:
- Paint.NET (free)
- GIMP (free)
- Aseprite (pixel art focused)
- Photoshop
- Any image editor with gradient/blur tools
