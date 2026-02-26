# Noise Textures

Procedural noise maps sampled by shaders at runtime. **These textures are never drawn directly on screen.** They are data textures — shaders read their grayscale values to control distortion, dissolve, scrolling, fog density, and other procedural effects.

## What Belongs Here

- **Perlin / Simplex noise** — Smooth, organic cloudy patterns for fog, dissolve transitions, and soft distortion
- **Voronoi / Cellular noise** — Geometric cell patterns for crystalline, cracked, or magical energy effects
- **FBM (Fractal Brownian Motion) noise** — Multi-octave turbulent noise for complex fog, nebula, and fire
- **Marble / Flow noise** — Flowing vein-like patterns for liquid, lava, or organic energy
- **Smoke / Caustics** — Soft billowing patterns for atmospheric haze and underwater caustic light
- **Energy gradients** — Horizontal or radial energy ramps used as UV lookup textures for beams, trails, and lens flares
- **Specialty patterns** — Star fields, cosmic vortex, musical wave, reality crack, and other unique noise types

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| **Color space** | Grayscale (R=G=B). Color is applied at runtime via shader tinting |
| **Dimensions** | Power-of-two: 64×64, 128×128, 256×256, or 512×512 |
| **Tiling** | Must tile seamlessly in both axes (for scrolling/rotation effects) |
| **Alpha** | No transparency — grayscale value IS the intensity. Use solid backgrounds |
| **Format** | PNG, 8-bit per channel |

## How These Are Used

Shaders sample these textures to:
- Control **fog dissipation** density and shape (FBM, nebula noise)
- Create **alpha erosion / dissolve** transitions (perlin, simplex)
- **Distort screen space** for heat haze, shockwaves, and ripple effects (voronoi, caustics)
- **Animate energy** along trails and beams via scrolling UV coordinates (energy gradients)
- Generate **organic movement** without discrete CPU-side particles (marble, smoke)
- Drive **procedural color mapping** when combined with gradient LUTs

## Current Inventory

| File | Type | Description |
|------|------|-------------|
| `PerlinNoise.png` | Perlin | Classic smooth perlin noise, general purpose |
| `SimplexNoise.png` | Simplex | Simplex noise variant, slightly sharper than perlin |
| `VoronoiNoise.png` | Voronoi | Cellular/geometric noise for crystalline effects |
| `TileableFBMNoise.png` | FBM | Multi-octave fractional Brownian motion, complex turbulence |
| `TileableMarbleNoise.png` | Marble | Flowing vein patterns for liquid/organic energy |
| `NoiseSmoke.png` | Smoke | Soft billowing smoke pattern |
| `SoftCircularCaustics.png` | Caustics | Underwater-style caustic light dappling |
| `NebulaWispNoise.png` | Nebula | Wispy fractal noise for cosmic fog/nebula |
| `CosmicEnergyVortex.png` | Specialty | Swirling cosmic vortex energy pattern |
| `CosmicNebulaClouds.png` | Specialty | Dense nebula cloud formations |
| `StarFieldScatter.png` | Specialty | Scattered star/point light pattern |
| `MusicalWavePattern.png` | Specialty | Wave pattern evoking musical frequency/harmonic oscillation |
| `DestinyThreadPattern.png` | Specialty | Intertwined thread/fate-line pattern |
| `RealityCrackPattern.png` | Specialty | Fractured/cracked reality pattern |
| `SparklyNoiseTexture.png` | Specialty | High-frequency sparkle/glint noise |
| `HorizontalEnergyGradient.png` | Gradient | Horizontal energy ramp for trail/beam UV lookup |
| `HorizontalBlackCoreCenterEnergyGradient.png` | Gradient | Anamorphic lens flare gradient with dark center core |
| `UniversalRadialFlowNoise.png` | Flow | Radial outward-flowing noise for aura/explosion effects |
