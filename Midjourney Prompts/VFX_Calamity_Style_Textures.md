# Calamity-Style VFX Texture Prompts for MagnumOpus

> **Purpose:** Generate the missing textures needed for Ark of the Cosmos / Galaxia style visual effects.
> **Important:** All textures should be WHITE/GRAYSCALE on transparent/black background - they get tinted in-game.

---

## ⚡ CRITICAL: NOISE TEXTURES (For Procedural Trails)

> **These are THE essential textures for Calamity-style trails!**
> 
> Trails in Calamity are NOT pre-made PNG images - they're **rendered procedurally** as triangle strip meshes with HLSL shaders. The shaders sample these noise textures to create the nebula/fog effect.
> 
> **You only need 2-3 noise textures to power ALL trails in the entire mod!**

---

### 1. Perlin Noise Texture (Primary - MUST HAVE)

**What it is:** A seamlessly tileable Perlin noise texture. This is the PRIMARY texture sampled by trail shaders to create the flowing, organic nebula look. The shader scrolls UV coordinates across this texture to create animated fog.

**Prompt:**
```
seamless tileable perlin noise texture, organic flowing grayscale pattern, smooth gradient transitions between light and dark regions, cloud-like formations, no hard edges, continuous flowing shapes, medium frequency noise, professional VFX noise map, pure grayscale black to white, game shader texture asset --ar 1:1 --tile --v 6 --style raw
```

**Alternative (higher detail):**
```
seamless perlin noise pattern, tileable in all directions, organic marble-like swirls, smooth grayscale gradients, flowing cloud formations, procedural texture, no visible seams or repeating patterns, medium contrast, VFX shader noise map, black and white only, 2D texture asset --ar 1:1 --tile --v 6 --style raw
```

**Post-processing needed:**
- Verify it tiles seamlessly (edges match perfectly)
- Ensure full grayscale range (some pure black, some pure white)
- Final size: **256x256** or **512x512**

**Destination:** `Assets/VFX/Noise/PerlinNoise.png`

---

### 2. Simplex/Organic Noise Texture (Secondary)

**What it is:** A more organic, cellular-looking noise with softer, rounder shapes. Used for smoke/vapor effects and provides variation from standard Perlin.

**Prompt:**
```
seamless tileable simplex noise texture, soft organic cellular shapes, rounded cloud-like blobs, smooth grayscale gradients, flowing organic pattern, no sharp edges, vapor-like formations, tileable in all directions, VFX smoke noise map, pure black and white, game shader texture --ar 1:1 --tile --v 6 --style raw
```

**Alternative (more vapor-like):**
```
seamless tileable smoke noise pattern, soft billowing cloud shapes, organic vapor texture, smooth grayscale transitions, rounded blob formations, ethereal mist pattern, no hard edges, continuous flowing texture, VFX particle noise map, tileable seamless --ar 1:1 --tile --v 6 --style raw
```

**Post-processing needed:**
- Verify seamless tiling
- Softer contrast than Perlin (more mid-grays)
- Final size: **256x256**

**Destination:** `Assets/VFX/Noise/SimplexNoise.png`

---

### 3. Voronoi/Cellular Noise Texture (Optional - For Special Effects)

**What it is:** A cell-like pattern with clear regions separated by edges. Creates a crystalline/fractured look. Used for reality-crack effects, Fate theme cosmic shattering, and electrical patterns.

**Prompt:**
```
seamless tileable voronoi noise texture, cellular pattern with soft cell boundaries, organic cell shapes, smooth grayscale, cells lighter in center darker at edges, cracked earth pattern, crystal formation texture, tileable game shader asset, no color pure grayscale, VFX noise map --ar 1:1 --tile --v 6 --style raw
```

**Alternative (more crystalline):**
```
seamless tileable cellular noise pattern, crystal lattice structure, polygonal cell shapes, soft gradient within each cell, organic stained glass pattern, fractured ice texture, grayscale only, tileable in all directions, VFX shader texture, game asset --ar 1:1 --tile --v 6 --style raw
```

**Post-processing needed:**
- Verify seamless tiling
- Clear cell structure visible
- Final size: **256x256**

**Destination:** `Assets/VFX/Noise/VoronoiNoise.png`

---

### How These Noise Textures Are Used

```hlsl
// Example: Calamity-style trail shader samples noise like this
float2 scrollingUV = uv + float2(time * 0.3, 0); // Scroll over time
float noiseValue = tex2D(PerlinNoise, scrollingUV * 2.0).r; // Sample noise
float4 trailColor = lerp(innerColor, outerColor, noiseValue); // Blend colors
trailColor.a *= smoothstep(0.0, 0.2, uv.x) * smoothstep(1.0, 0.8, uv.x); // Edge fade
```

**The noise creates:**
- Flowing nebula clouds (by scrolling UV coordinates)
- Organic variation (breaks up uniform gradients)
- Animated fog (time-based UV offset)
- Cosmic dust (combined with color palettes)

---

## 🔴 HIGH PRIORITY (Supporting Textures)

---

### 1. HeavySmoke Spritesheet (7 Frames)

**What it is:** A vertical spritesheet of 7 different smoke puff variations. Used for the "flaming smoke" effect behind weapon swings. Each frame is a different smoke cloud shape.

**Prompt:**
```
sprite sheet, 7 frames arranged vertically, soft fluffy smoke puff cloud particles, each frame a different organic cloud shape, volumetric smoke, wispy edges, varying density, white smoke on pure black background, game asset, VFX particle texture, transparent edges fading to nothing, no hard edges, ethereal vapor, seamless alpha gradient, top-down 2D game art style --ar 1:7 --v 6 --style raw
```

**Alternative (more detailed):**
```
vertical sprite sheet with 7 rows, each row contains one unique smoke cloud shape, soft billowing smoke puffs, organic asymmetrical shapes, volumetric lighting, white and light gray tones only, pure black background, game particle effect asset, wispy dissolving edges, varying sizes from small puff to large cloud, no color just grayscale, high contrast, clean edges for alpha masking --ar 1:7 --v 6 --style raw
```

**Post-processing needed:**
- Crop to exactly 7 equal-height frames
- Ensure pure black background converts to transparency
- Final size: ~128x896 (128 wide × 7 frames of 128 tall)

---

### 2. CircularSmearSmokey (Nebula Arc)

**What it is:** A full 360° ring/donut shape with a smoky, nebula-like texture. This creates the purple cosmic arc behind Galaxia-style sword swings. Brightest in the middle band of the ring.

**Prompt:**
```
circular ring shape, donut gradient, nebula smoke texture inside ring band, brightest white in center of ring thickness, fading to transparent at inner and outer edges, wispy smoky tendrils, cosmic gas cloud texture, pure black background, VFX game asset, circular symmetry, ethereal glow, particle effect texture, no color only white and gray gradients --ar 1:1 --v 6 --style raw
```

**Alternative (more nebula-like):**
```
top-down view of circular nebula ring, cosmic gas cloud forming perfect ring shape, stellar dust, wispy smoke texture with subtle noise, white hot center band fading to transparent edges, astronomical photography style, black void background, game VFX texture, radial symmetry, soft ethereal glow, grayscale only --ar 1:1 --v 6 --style raw
```

**Post-processing needed:**
- Make background transparent
- Ensure ring is centered
- Final size: 256x256 or 512x512

---

### 3. CircularSmear (Clean Arc)

**What it is:** Same ring shape but with clean, smooth gradients instead of smoky texture. Used for cleaner energy-style swings.

**Prompt:**
```
perfect circular ring shape, smooth radial gradient, brightest white in middle band of ring, soft falloff to transparent at inner and outer edges, clean energy aura, no texture just smooth gradient, pure black background, game VFX asset, glowing halo effect, minimalist, vector-like smoothness --ar 1:1 --v 6 --style raw
```

**Post-processing needed:**
- Make background transparent
- Final size: 256x256

---

### 4. SemiCircularSmear (Half Arc)

**What it is:** A 180° half-ring for standard sword swing arcs. Same gradient pattern as full ring but only half.

**Prompt:**
```
semicircle arc shape, half ring, 180 degree arc, smooth radial gradient within arc band, brightest white at center of arc thickness, fading to transparent at edges, clean energy slash trail, pure black background, game VFX melee swing effect, crescent moon shape but thicker, no texture just gradient --ar 1:1 --v 6 --style raw
```

**Alternative (smokey version):**
```
semicircular arc shape, half donut, nebula smoke texture filling the arc band, wispy cosmic gas, 180 degree sweep, brightest in center fading to transparent edges, sword swing trail VFX, black background, game particle asset, ethereal energy slash --ar 1:1 --v 6 --style raw
```

**Post-processing needed:**
- Ensure arc is in upper half of square canvas
- Make background transparent
- Final size: 256x256

---

## 🟡 MEDIUM PRIORITY

---

### 5. TrailStreak (Horizontal Gradient)

**What it is:** A horizontal streak that fades from solid white on one end to transparent on the other. Used for projectile trails.

**Prompt:**
```
horizontal energy streak, left side bright white solid, gradually fading to transparent on right side, smooth gradient falloff, elongated lens flare shape, tapered ends, game VFX projectile trail texture, pure black background, clean energy beam, no texture just smooth gradient, slightly pointed ends --ar 4:1 --v 6 --style raw
```

**Post-processing needed:**
- Make background transparent
- Final size: 256x64 or 512x128

---

### 6. FlameTrail (Fiery Horizontal)

**What it is:** Similar to TrailStreak but with flame-like noise/wisps. For fire weapon trails.

**Prompt:**
```
horizontal flame trail texture, fire wisps and tendrils, left side intense white heat, fading to transparent smoke on right, organic flame shapes, flickering fire edge detail, game VFX fire projectile trail, black background, elongated flame streak, wispy burning edges, grayscale only --ar 4:1 --v 6 --style raw
```

**Post-processing needed:**
- Make background transparent
- Final size: 256x64

---

### 7. CosmicTrail (Nebula Horizontal)

**What it is:** A horizontal streak with star speckles and nebula clouds. For Fate/cosmic themed weapons.

**Prompt:**
```
horizontal cosmic nebula streak, star field with tiny bright points, nebula gas clouds, left side dense and bright, fading to sparse transparent on right, galaxy dust trail, astronomical, game VFX cosmic projectile trail, pure black background, ethereal space dust, grayscale with bright white star points --ar 4:1 --v 6 --style raw
```

**Post-processing needed:**
- Make background transparent
- Final size: 256x64

---

## 🟢 LOW PRIORITY (Masks)

---

### 8. CircularFalloff Mask

**What it is:** A simple circular gradient - white center fading to black/transparent edges. Used for bloom and aura effects.

**Prompt:**
```
perfect circle gradient, bright white center point, smooth radial falloff to pure black edges, soft glow orb, game VFX bloom mask texture, no texture just clean gradient, minimalist, circular symmetry, black background --ar 1:1 --v 6 --style raw
```

**Post-processing needed:**
- Final size: 128x128 or 256x256

---

### 9. RadialBurst Mask

**What it is:** A starburst/ray pattern radiating from center. For impact effects.

**Prompt:**
```
radial starburst pattern, light rays emanating from center point, 8 to 12 pointed star rays, bright white center, rays fading to black at edges, clean geometric, game VFX impact burst mask, symmetrical, no texture just gradient rays, black background --ar 1:1 --v 6 --style raw
```

**Post-processing needed:**
- Final size: 256x256

---

### 10. SoftEdge Mask (Horizontal)

**What it is:** A horizontal gradient for trail edge fading - useful for shader masks.

**Prompt:**
```
horizontal gradient bar, left edge pure white, smooth transition to pure black on right edge, linear falloff, clean mask texture, game VFX alpha mask, no noise just smooth gradient, simple rectangular shape --ar 4:1 --v 6 --style raw
```

**Post-processing needed:**
- Final size: 256x64

---

## 📐 Post-Processing Guide

### For ALL textures:

1. **Remove background:** Convert pure black (#000000) to transparent alpha
2. **Check edges:** Ensure no hard pixelated edges, should fade smoothly
3. **Save as PNG:** 32-bit with alpha channel
4. **Color:** Should be white/grayscale ONLY (game code tints them)

### For NOISE textures specifically:

1. **DO NOT remove background** - noise textures need the full grayscale range
2. **Verify seamless tiling** - edges must match perfectly (use Photoshop's Offset filter to check)
3. **Keep as grayscale** - no alpha channel needed for noise
4. **Check value range** - should have some pure black (0) and pure white (255) regions

### Specific sizes:

| Texture | Dimensions | Notes |
|---------|------------|-------|
| **PerlinNoise.png** | **256×256** | **CRITICAL - Seamless tileable** |
| **SimplexNoise.png** | **256×256** | **Seamless tileable** |
| **VoronoiNoise.png** | **256×256** | **Seamless tileable** |
| HeavySmoke.png | 128×896 | 7 frames, 128×128 each |
| CircularSmear.png | 256×256 | Centered ring |
| CircularSmearSmokey.png | 256×256 | Centered ring with noise |
| SemiCircularSmear.png | 256×256 | Arc in upper half |
| TrailStreak.png | 256×64 | Horizontal fade |
| FlameTrail.png | 256×64 | Horizontal with wisps |
| CosmicTrail.png | 256×64 | Horizontal with stars |
| CircularFalloff.png | 256×256 | Simple radial gradient |
| RadialBurst.png | 256×256 | Starburst rays |
| SoftEdge.png | 256×64 | Linear horizontal gradient |

### Photoshop/GIMP Quick Steps:

**For particle textures:**
1. Open generated image
2. Select → Color Range → Select black background
3. Delete selection (creates transparency)
4. Image → Mode → Grayscale (if not already)
5. Image → Mode → RGB (to allow tinting)
6. Resize to target dimensions
7. Export as PNG-24 with transparency

**For noise textures:**
1. Open generated image
2. Filter → Other → Offset (set to 50% width/height, Wrap Around)
3. Check for visible seams - if any, use Clone Stamp to blend
4. Filter → Other → Offset (reset to 0,0)
5. Image → Mode → Grayscale
6. Resize to 256×256
7. Export as PNG (no transparency needed)

---

## 📁 File Locations

Place the finished textures in:

### ⚡ CRITICAL - Noise Textures (For All Procedural Trails)

| Texture | Destination Path | Priority |
|---------|-----------------|----------|
| PerlinNoise.png | `Assets/VFX/Noise/PerlinNoise.png` | **MUST HAVE** |
| SimplexNoise.png | `Assets/VFX/Noise/SimplexNoise.png` | High |
| VoronoiNoise.png | `Assets/VFX/Noise/VoronoiNoise.png` | Optional |

### Other Textures

| Texture | Destination Path |
|---------|-----------------|
| HeavySmoke.png | `Assets/Particles/HeavySmoke.png` |
| CircularSmear.png | `Assets/Particles/Textures/CircularSmear.png` |
| CircularSmearSmokey.png | `Assets/Particles/Textures/CircularSmearSmokey.png` |
| SemiCircularSmear.png | `Assets/Particles/Textures/SemiCircularSmear.png` |
| TrailStreak.png | `Assets/VFX/Trails/TrailStreak.png` |
| FlameTrail.png | `Assets/VFX/Trails/FlameTrail.png` |
| CosmicTrail.png | `Assets/VFX/Trails/CosmicTrail.png` |
| CircularFalloff.png | `Assets/VFX/Masks/CircularFalloff.png` |
| RadialBurst.png | `Assets/VFX/Masks/RadialBurst.png` |
| SoftEdge.png | `Assets/VFX/Masks/SoftEdge.png` |

**Note:** Create the following folders if they don't exist:
- `Assets/VFX/Noise/` ← **NEW - Critical for procedural trails**
- `Assets/Particles/Textures/`
- `Assets/VFX/Trails/`
- `Assets/VFX/Masks/`

---

## 🎯 Quick Start: Minimum Viable VFX

**If you only generate 3 textures, make them:**

1. **PerlinNoise.png** - Powers ALL procedural trails (Ark of Cosmos style)
2. **HeavySmoke.png** - Smoke particles for La Campanella and general use
3. **CircularSmear.png** - Clean swing arcs for melee weapons

Everything else can be procedurally rendered or use existing `SoftGlow`/`EnergyFlare` assets!
