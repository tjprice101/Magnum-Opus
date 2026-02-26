# Color Gradients

Lookup textures (LUTs) and gradient ramps that shaders sample to **map intensity values to colors**. Instead of hardcoding colors in shader code, the shader reads a grayscale value (0.0–1.0) and uses it as a UV coordinate to sample a color from these textures.

## What Belongs Here

- **1D color ramps** — Single-row (Nx1) textures where left = cold/dark and right = hot/bright. Shaders sample horizontally based on intensity
- **2D color grading LUTs** — Grid-format lookup tables for full color correction / grading
- **Theme palette strips** — Per-theme color gradients that encode each musical theme's palette progression (shadow → midtone → highlight → core)
- **Gradient masks** — Grayscale gradients (linear, radial) used to control falloff, fade, or intensity distribution

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **1D color ramps** | Width: 256px. Height: 1px (single row). Left pixel = intensity 0 (darkest), right pixel = intensity 1 (brightest) |
| **2D LUTs** | Standard unwrapped 3D LUT format (e.g., 256×16 or 1024×32 grids) |
| **Theme palettes** | Should follow the musical dynamics mapping: Pianissimo (shadow) → Piano (outer glow) → Mezzo (body) → Forte (hot) → Fortissimo (core) |
| **Filtering** | Point/nearest-neighbor filtering for crisp color bands, or linear for smooth gradients |
| **Format** | PNG, sRGB color space |

## How These Are Used

Color gradient workflow in shaders:
1. A shader computes an **intensity value** (0.0–1.0) from distance, time, noise, etc.
2. It uses that value as a U coordinate to sample the **gradient texture**
3. The sampled color replaces or modulates the output pixel color
4. This gives smooth, artist-controlled color transitions without changing shader code

Example: A trail shader computes heat (bright at leading edge, fading at tail). It samples a La Campanella gradient to get: deep black smoke → dark red → orange flame → bright gold at the hottest point.

## Theme Palette Guidelines

Each musical theme should have its own gradient strip. Refer to the mod's theme identity:

| Theme | Gradient Progression (left → right, cold → hot) |
|-------|--------------------------------------------------|
| **La Campanella** | Deep black → dark ember → orange flame → bright gold |
| **Eroica** | Dark burgundy → scarlet → crimson → gold highlight |
| **Swan Lake** | Black void → cool gray → pure white → prismatic rainbow edge |
| **Moonlight Sonata** | Deep void → dark purple → violet → vibrant ice blue |
| **Enigma Variations** | Black void → deep purple → eerie green → ghostly white |
| **Fate** | Black void → dark pink → bright crimson → celestial white |

## Current Inventory

*This folder is currently empty — awaiting new gradient and LUT textures.*

### Needed Asset Types
- Per-theme 1D color ramp (256×1 each, one per musical theme)
- Rainbow LUT (256×1, full spectrum)
- Linear grayscale gradient (256×1, white to black)
- Radial grayscale gradient (128×128, white center to black edge)
