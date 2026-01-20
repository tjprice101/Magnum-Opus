# VFX Assets for MagnumOpus

This folder contains textures used by the enhanced VFX system based on FargosSoulsDLC patterns.

## Required Textures

### Bloom Textures (`/VFX/`)
All bloom textures should be **white/grayscale** with smooth radial falloff.

| Filename | Size | Description |
|----------|------|-------------|
| `BloomCircleSmall.png` | 64x64 | Small circular bloom, soft edge falloff |
| `BloomCircleMedium.png` | 128x128 | Medium circular bloom |
| `BloomCircleLarge.png` | 256x256 | Large soft bloom for backgrounds |

### Flare Textures (`/VFX/`)
| Filename | Size | Description |
|----------|------|-------------|
| `ShineFlare4Point.png` | 64x64 | 4-pointed star/cross flare |
| `ShineFlare6Point.png` | 64x64 | 6-pointed star flare |
| `StreakFlare.png` | 64x16 | Horizontal streak for motion |
| `LensFlare.png` | 128x128 | Complex lens flare |

### Line/Trail Textures (`/VFX/`)
| Filename | Size | Description |
|----------|------|-------------|
| `BloomLine.png` | 128x8 | Horizontal glow line for beams |
| `TaperedLine.png` | 128x8 | Line that fades toward one end |
| `Pixel.png` | 1x1 | Single white pixel |
| `InvisiblePixel.png` | 1x1 | Transparent pixel |

### Noise Textures (`/VFX/Noise/`)
Used for shader effects and distortions.

| Filename | Size | Description |
|----------|------|-------------|
| `PerlinNoise.png` | 256x256 | Smooth perlin noise, tileable |
| `WavyBlotchNoise.png` | 256x256 | Organic wavy patterns |
| `DendriticNoise.png` | 256x256 | Branching/lightning patterns |
| `CellularNoise.png` | 256x256 | Voronoi/cellular patterns |
| `TurbulenceNoise.png` | 256x256 | Smoke/cloud turbulence |

### Gradient Textures (`/VFX/`)
| Filename | Size | Description |
|----------|------|-------------|
| `LinearGradient.png` | 256x1 | White to black horizontal |
| `RadialGradient.png` | 128x128 | White center to black edge |

### Special Effect Textures (`/VFX/`)
| Filename | Size | Description |
|----------|------|-------------|
| `SmokePuff.png` | 64x64 | Soft cloud/smoke shape |
| `GlowySpark.png` | 32x32 | Diamond/square spark |

## Texture Guidelines

1. **Use white/grayscale**: All bloom and flare textures are tinted at runtime
2. **Soft edges**: Use smooth gradients, no hard edges on bloom textures
3. **Power-of-two sizes**: 32, 64, 128, 256 for better GPU performance
4. **Alpha channel**: Use alpha for transparency, not black backgrounds
5. **Tileable noise**: Noise textures should tile seamlessly

## Fallback Behavior

The `MagnumTextureRegistry` will generate procedural fallback textures if any of these files are missing. However, custom textures will look significantly better.

## Creating Bloom Textures

For a basic radial bloom in any image editor:
1. Create new image with transparent background
2. Use radial gradient tool from center
3. White at center, transparent at edges
4. Apply Gaussian blur for softness
5. Export as PNG with alpha

## Midjourney Prompts for Particle Textures

See `/Midjourney Prompts/` folder for prompts to generate these textures.
