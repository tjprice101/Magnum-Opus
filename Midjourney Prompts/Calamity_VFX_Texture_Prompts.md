# Calamity VFX Texture Prompts - Ribbon Meshes and Gradients

> **PURPOSE:** These prompts generate textures for the new Calamity-style VFX system.
> These textures are used by:
> - CalamityFireShader.fx (dual-scroll advection, erosion)
> - RibbonTrailSystem.cs (triangle strip meshes)
> - SpectralLUTController.cs (color gradients)
> - ScreenHeatDistortionSystem.cs (heat haze)

---

## 📁 OUTPUT REQUIREMENTS

**Resolution:** 512x512 (tileable noise) or 256x32 (gradients)
**Format:** PNG with transparency where noted
**Color Mode:** Grayscale for noise/masks, RGB for gradients

---

## 🔥 FIRE/FLAME NOISE TEXTURES

### 1. Dual-Scroll Fire Noise
**Purpose:** Primary noise for dual-scroll UV advection in CalamityFireShader

```
seamless tileable perlin noise texture, organic fire-like turbulence, 
wisps and tendrils of smoke pattern, high contrast black to white,
soft edges blending into dark background, fractal detail at multiple scales,
game asset texture, 512x512, grayscale, isolated on pure black background
--ar 1:1 --tile --s 250 --q 2
```

### 2. Worley Cell Noise (Heat Distortion)
**Purpose:** Secondary noise for UV distortion warping

```
seamless tileable worley noise voronoi cells, organic cellular pattern,
rounded bubbles and organic shapes, soft edges, high contrast grayscale,
resembles boiling liquid surface tension, game asset texture,
512x512, isolated on pure black background
--ar 1:1 --tile --s 250 --q 2
```

### 3. Erosion Threshold Mask
**Purpose:** Mask for smoothstep erosion edge effects

```
seamless tileable gradient noise texture, directional vertical flow,
bottom bright fading to top dark, wispy flame-like erosion edges,
organic ragged torn paper edges, high contrast black and white,
game asset texture, 512x512, grayscale
--ar 1:1 --tile --s 250 --q 2
```

---

## 🌈 RIBBON STREAK TEXTURES

### 4. Soft Energy Ribbon
**Purpose:** Base texture for RibbonTrailSystem triangle strips

```
horizontal energy beam streak texture, soft glowing plasma ribbon,
white hot core fading to transparent edges, gaussian falloff,
perfectly straight horizontal beam, clean minimal design,
game asset texture for weapon trails, 512x64 aspect ratio,
isolated on pure black background with transparency
--ar 8:1 --s 250 --q 2
```

### 5. Flame Ribbon with Erosion
**Purpose:** Fire-effect ribbon with pre-baked erosion edges

```
horizontal flame streak texture, fiery energy ribbon,
bright yellow core fading through orange to red edges,
irregular flame-like erosion along edges, wisps and tendrils,
horizontal orientation flowing left to right,
game asset texture, 512x64 aspect ratio,
isolated on pure black background with transparency
--ar 8:1 --s 250 --q 2
```

### 6. Cosmic Nebula Ribbon
**Purpose:** Deep space ribbon for Fate theme

```
horizontal cosmic nebula streak, ethereal space cloud ribbon,
purple and pink cosmic dust, tiny star sparkles embedded,
soft gradient edges fading to transparency, mystical energy,
horizontal orientation, game asset texture, 512x64 aspect ratio,
isolated on pure black background
--ar 8:1 --s 250 --q 2
```

### 7. Electric Arc Ribbon
**Purpose:** Lightning/electrical ribbon texture

```
horizontal lightning bolt streak texture, branching electric arc,
bright white core with blue electrical glow, jagged forked edges,
high energy plasma discharge, horizontal orientation,
game asset texture, 512x64 aspect ratio,
isolated on pure black background with transparency
--ar 8:1 --s 250 --q 2
```

---

## 📊 GRADIENT LUT TEXTURES

### 8. Fire Temperature Gradient (Horizontal LUT)
**Purpose:** Color ramp for fire shader (black -> red -> orange -> yellow -> white)

```
horizontal gradient bar texture, fire temperature colormap,
left side pure black, transitioning through dark red, bright orange,
yellow-white, to pure white on right side, smooth gradient,
blackbody radiation colors, game asset color lookup texture,
256x32 pixels, no transparency
--ar 8:1 --s 200 --q 2
```

### 9. Rainbow Spectrum Gradient (Horizontal LUT)
**Purpose:** Visible light spectrum for chromatic effects

```
horizontal rainbow spectrum gradient bar, visible light wavelengths,
left violet transitioning through blue, cyan, green, yellow, orange,
to red on right side, physically accurate color spectrum,
smooth continuous gradient, game asset color lookup texture,
256x32 pixels, no transparency
--ar 8:1 --s 200 --q 2
```

### 10. Fraunhofer Spectrum (with Absorption Lines)
**Purpose:** Spectrum with dark absorption bands for realistic optical effects

```
horizontal rainbow spectrum with dark absorption lines,
Fraunhofer lines visible as dark vertical bands in spectrum,
violet to red rainbow with periodic black gaps,
solar spectrum appearance, scientific accuracy,
game asset color lookup texture, 256x32 pixels
--ar 8:1 --s 200 --q 2
```

### 11. Eroica Theme Gradient
**Purpose:** Scarlet-to-gold gradient for Eroica theme

```
horizontal gradient bar texture, heroic fire colormap,
left side deep scarlet red #8B0000, through crimson #DC143C,
orange flame #FF6432, to brilliant gold #FFD700 on right,
smooth gradient, game asset color lookup texture, 256x32 pixels
--ar 8:1 --s 200 --q 2
```

### 12. Fate Theme Gradient
**Purpose:** Cosmic void gradient for Fate theme

```
horizontal gradient bar texture, cosmic void colormap,
left side pure black #0F0514, through deep purple #780A8C,
dark pink #B43264, bright red #FF3C50, to star white on right,
smooth gradient, game asset color lookup texture, 256x32 pixels
--ar 8:1 --s 200 --q 2
```

### 13. La Campanella Theme Gradient
**Purpose:** Infernal black-to-orange gradient

```
horizontal gradient bar texture, infernal fire colormap,
left side smoky black #141014, through ember #642814,
infernal orange #FF6400, flame yellow #FFB432, to golden #FFDC64,
smooth gradient, game asset color lookup texture, 256x32 pixels
--ar 8:1 --s 200 --q 2
```

---

## 🎭 MASK TEXTURES

### 14. Radial Falloff Mask
**Purpose:** Circular falloff for point-source effects

```
radial gradient circle mask, white center fading to black edges,
perfectly circular, smooth gaussian falloff, centered in frame,
game asset alpha mask texture, 256x256 pixels, grayscale,
isolated on pure black background
--ar 1:1 --s 200 --q 2
```

### 15. Ring Mask
**Purpose:** Hollow ring for shockwave/halo effects

```
ring shaped gradient mask, hollow circle donut shape,
bright white ring band with black center and black edges,
smooth gradient falloff both directions, game asset texture,
256x256 pixels, grayscale, isolated on pure black background
--ar 1:1 --s 200 --q 2
```

### 16. Directional Streak Mask
**Purpose:** Elongated falloff for motion blur effects

```
horizontal elongated ellipse gradient mask, capsule shape,
bright white center fading to black at horizontal edges,
stretched oval shape, smooth falloff, game asset texture,
256x64 aspect ratio, grayscale
--ar 4:1 --s 200 --q 2
```

---

## 🌟 SPECIAL EFFECT TEXTURES

### 17. Noise with Horizontal Flow
**Purpose:** Pre-distorted noise that flows horizontally (for fire that "moves up")

```
seamless tileable noise texture with directional flow,
organic swirling pattern with horizontal bias,
wisps and tendrils flowing left to right, fire-like turbulence,
high contrast black to white, game asset texture, 512x512
--ar 1:1 --tile --s 250 --q 2
```

### 18. Star Sparkle Field
**Purpose:** Overlay texture for cosmic sparkle effects

```
scattered star sparkle field texture, random bright dots,
various sizes of glowing star points on black background,
some with 4-point star flares, some simple dots,
sparse distribution, game asset overlay texture, 256x256
--ar 1:1 --s 200 --q 2
```

### 19. Chromatic Dispersion Edge
**Purpose:** Pre-baked RGB channel separation for chromatic aberration

```
horizontal chromatic aberration edge texture,
red shifted left, green center, blue shifted right,
rainbow edge dispersion effect, prism light separation,
game asset texture for edge effects, 256x64
--ar 4:1 --s 200 --q 2
```

---

## 📝 TEXTURE PLACEMENT GUIDE

After generating, place textures in these locations:

| Texture | File Name | Location |
|---------|-----------|----------|
| Fire Noise | `FireNoise512.png` | `Assets/VFX/Noise/` |
| Worley Noise | `WorleyNoise512.png` | `Assets/VFX/Noise/` |
| Erosion Mask | `ErosionMask512.png` | `Assets/VFX/Masks/` |
| Soft Ribbon | `RibbonSoft512.png` | `Assets/VFX/Ribbons/` |
| Flame Ribbon | `RibbonFlame512.png` | `Assets/VFX/Ribbons/` |
| Cosmic Ribbon | `RibbonCosmic512.png` | `Assets/VFX/Ribbons/` |
| Electric Ribbon | `RibbonElectric512.png` | `Assets/VFX/Ribbons/` |
| Fire LUT | `LUT_Fire.png` | `Assets/VFX/LUT/` |
| Rainbow LUT | `LUT_Rainbow.png` | `Assets/VFX/LUT/` |
| Fraunhofer LUT | `LUT_Fraunhofer.png` | `Assets/VFX/LUT/` |
| Eroica LUT | `LUT_Eroica.png` | `Assets/VFX/LUT/` |
| Fate LUT | `LUT_Fate.png` | `Assets/VFX/LUT/` |
| Campanella LUT | `LUT_LaCampanella.png` | `Assets/VFX/LUT/` |
| Radial Mask | `MaskRadial256.png` | `Assets/VFX/Masks/` |
| Ring Mask | `MaskRing256.png` | `Assets/VFX/Masks/` |
| Streak Mask | `MaskStreak256.png` | `Assets/VFX/Masks/` |
| Flow Noise | `FlowNoise512.png` | `Assets/VFX/Noise/` |
| Star Field | `StarField256.png` | `Assets/VFX/Overlays/` |
| Chromatic Edge | `ChromaticEdge256.png` | `Assets/VFX/Overlays/` |

---

## 🔧 SHADER PARAMETER GUIDE

These textures are consumed by shaders with these sampler slots:

| Shader | s0 (uImage0) | s1 (uNoiseTex) | s2 (uPaletteLUT) | s3 (uMaskTex) |
|--------|--------------|----------------|------------------|---------------|
| CalamityFireShader | Base sprite | Fire/Worley noise | Theme LUT | Erosion mask |
| AdvancedDistortionShader | Screen capture | Worley noise | - | Ring mask |
| ProceduralTrailShader | - | Fire noise | Theme LUT | Streak mask |

---

## 🎨 COLOR REFERENCE

### Theme Color Hexcodes (for LUT generation)

**Eroica (Scarlet → Gold):**
- `#8B0000` Deep Scarlet
- `#DC143C` Crimson
- `#FF6432` Flame
- `#FFD700` Gold
- `#FFFFFF` White highlight

**Fate (Void → Cosmic):**
- `#0F0514` Void Black
- `#780A8C` Deep Purple
- `#B43264` Dark Pink
- `#FF3C50` Bright Red
- `#FFFFFF` Star White

**La Campanella (Infernal):**
- `#141014` Smoky Black
- `#642814` Ember
- `#FF6400` Orange
- `#FFB432` Flame Yellow
- `#FFDC64` Golden

**Swan Lake (Monochrome):**
- `#FFFFFF` Pure White
- `#DCE1EB` Silver
- `#B4B4C8` Pale Gray
- `#505064` Dark Gray
- `#14141E` Near Black

**Moonlight Sonata (Lunar):**
- `#4B0082` Indigo
- `#8A2BE2` Violet
- `#9664DC` Medium Purple
- `#87CEFA` Light Blue
- `#DCDCEB` Silver

**Enigma Variations (Void):**
- `#0F0A14` Void Black
- `#501478` Deep Purple
- `#8C3CC8` Purple
- `#32DC64` Green Flame
- `#1E6432` Dark Green
