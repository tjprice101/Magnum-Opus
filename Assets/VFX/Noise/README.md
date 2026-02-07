# Noise Textures for Shader-Based VFX

This folder contains **data textures** used by shaders and the CinematicVFX system to create smooth, organic visual effects.

---

## ✅ CUSTOM NOISE TEXTURES (Currently Available)

These textures are used by `CinematicVFX.cs` and `LayeredNebulaFog.cs`:

| File | Purpose | Used By |
|------|---------|---------|
| `HorizontalEnergyGradient.png` | Energy streak/trail texture | `CinematicVFX.SpawnEnergyStreak()` |
| `HorizontalBlackCoreCenterEnergyGradient.png` | Anamorphic lens flare with dark center | `CinematicVFX.SpawnLensFlare()` with `useBlackCore: true` |
| `NebulaWispNoise.png` | Wispy fractal noise for fog | `LayeredNebulaFog` background layer |
| `SparklyNoiseTexture.png` | Sparkle/glint pattern | `CinematicVFX.SpawnImpactGlint()` with `sparkly: true` |
| `TileableFBMNoise.png` | Complex turbulent FBM noise | `LayeredNebulaFog` midground, `CinematicVFX.SpawnEnhancedNebula()` |
| `TileableMarbleNoise.png` | Flowing organic marble noise | `CinematicVFX.SpawnEnhancedNebula()` with `useMarble: true` |

### Texture Requirements
- **Power-of-two dimensions** (64, 128, 256, 512)
- **Grayscale** - Textures are tinted with `Color` multiplication at runtime
- **Seamlessly tiling** (FBM, Marble, Nebula) for scrolling/rotation effects
- **No alpha transparency** in noise textures - use grayscale value as alpha

---

## ⚠️ IMPORTANT: These Are NOT Visual Textures

Unlike particle sprites, these textures are **never drawn directly as-is**. They are sampled by shaders/renderers to:
- Control fog dissipation patterns
- Create smooth alpha erosion (dissolve effects)
- Distort screen space (heat haze, shockwaves)
- Animate energy trails with scrolling UV
- Generate organic movement without discrete particles

## Texture Types

### 1. Perlin/Simplex Noise (`noise_perlin_*.png`)
Smooth, cloudy patterns for organic transitions.
- **Fine**: Small detail, rapid variation
- **Medium**: Balanced for general fog
- **Coarse**: Large blobs for slow morphing

### 2. Voronoi/Cellular Noise (`noise_voronoi_*.png`)
Geometric cell patterns for magical effects.
- **Soft**: Rounded cells for organic energy
- **Sharp**: Cracked glass for crystalline effects

### 3. Fractal Noise (`noise_fractal_*.png`)
Multi-scale complexity for smoke and nebulae.
- **Smoke**: Turbulent, chaotic patterns
- **Nebula**: High-detail cosmic gas

### 4. Trail Textures (`trail_*.png`)
Horizontal strips for triangle-strip mesh trails.
- **Energy Gradient**: Bright center, dark edges
- **Flow**: Streaming fiber patterns
- **Sparkle**: Random glitter points

### 5. Distortion Textures (`distort_*.png`)
For screen-space warping effects.
- **Normal**: RGB-encoded surface normals
- **Ripple**: Radial shockwave pattern
- **Haze**: Heat shimmer waves

### 6. Dissolve Textures (`dissolve_*.png`)
For alpha erosion death/spawn effects.
- **Organic**: Burning paper edges
- **Crystal**: Dendritic growth pattern

### 7. Utility Ramps (`ramp_*.png`)
Gradient lookup textures.
- **Linear**: Horizontal gradient for color ramps
- **Radial**: Circular falloff for particles

## Technical Requirements

| Property | Requirement |
|----------|-------------|
| Format | PNG (no compression artifacts) |
| Color | **Grayscale only** (except normal maps) |
| Size | 256x256, 512x512, or 1024x1024 |
| Tiling | **Must be seamless** in all directions |
| Range | Full 0-255 spectrum utilized |

## Usage in Code

```csharp
// Load noise texture
Texture2D noiseTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/VFX/Noise/noise_perlin_medium").Value;

// In shader (HLSL):
float noise = tex2D(noiseSampler, uv + time * 0.1).r;
float alpha = smoothstep(threshold - 0.05, threshold + 0.05, noise);
```

## Generating These Textures

See `Midjourney Prompts/14_Noise_Textures_And_Meshes.txt` for AI generation prompts.

Alternatively, use free procedural generators:
- [FilterForge](https://www.filterforge.com/)
- [Substance Designer](https://www.adobe.com/products/substance3d-designer.html)
- [NormalMap Online](https://cpetry.github.io/NormalMap-Online/)
- [Noise Texture Generator](https://www.noisetexturegenerator.com/)
