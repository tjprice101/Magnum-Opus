# VFX Enhancement Texture Prompts

> **Based on VFX+ mod comparison - textures to enhance MagnumOpus VFX systems**
> 
> **⚠️ CRITICAL: ALL textures must be PURE WHITE or GRAYSCALE on solid black background for runtime color tinting!**

---

## 🎨 UNIVERSAL PROMPT REQUIREMENTS

**Every prompt below produces:**
- **Pure white (#FFFFFF) or grayscale values only** - absolutely no colors
- **Solid black (#000000) background** - required for additive blending
- **Clean edges where specified** - no unwanted blur bleeding
- **Smooth gradients for glow falloff** - white center → gray → black edges
- **Game-ready sprite format** - isolated element, no environment

---

## 1. AFTERIMAGE / GHOST TEXTURES

### 1A. Afterimage Ghost Silhouette
```
Pure white humanoid silhouette sprite on solid black background, soft gaussian blur applied uniformly around all edges creating 20-pixel feathered falloff, bright white opaque center transitioning to semi-transparent gray at edges then fully transparent at boundary, no facial features or details just smooth glowing form, simple oval-shaped torso with suggested arm positions angled slightly outward, perfect bilateral symmetry on vertical center axis, completely flat 2D game sprite style with no lighting variation or shadows, 256x256 pixel dimensions, PNG transparency ready, pure white and gray values only absolutely no colors, designed for programmatic color tinting in game engine --ar 1:1 --v 6.1 --style raw --no color tint hue saturation face features details texture noise grain environment background elements
```

### 1B. Horizontal Motion Blur Streak
```
Horizontal white motion blur streak on solid black background, perfectly straight horizontal orientation with no curve or wobble, brightest pure white concentrated in center vertical band approximately 4 pixels wide, smooth gaussian gradient fade to medium gray then to black at left and right ends, streak height approximately 32 pixels with soft vertical edges that feather out, total length 512 pixels, resembles high-speed camera motion blur of fast-moving light source, seamless tileable on horizontal axis for repeating, no particles sparkles or additional elements just smooth continuous gradient, grayscale values only no color, 2D game VFX asset for projectile trails --ar 4:1 --v 6.1 --style raw --no color sparks particles texture vertical elements curves noise grain
```

### 1C. Chromatic Separation Base Layer
```
Three concentric soft circular rings on solid black background, outermost ring largest approximately 350 pixels diameter and most diffuse with heavy gaussian blur, middle ring approximately 200 pixels diameter with medium blur, innermost ring smallest approximately 100 pixels diameter and brightest with light blur, all three rings pure white and grayscale only absolutely no RGB colors, each ring has smooth gaussian blur falloff from bright center of ring stroke to transparent edges, rings slightly offset from common center by 10-15 pixels to suggest motion or displacement, spacing between ring centers approximately 30 pixels, total composition diameter 400 pixels, designed for RGB channel splitting post-process effect, 2D screen overlay texture --ar 1:1 --v 6.1 --style raw --no color RGB red green blue sharp edges solid shapes environment
```

---

## 2. SWING SMEAR TEXTURES

### 2A. Wide Crescent Arc Slash
```
Single curved crescent arc shape on solid black background, arc spans approximately 120 degrees of a circle suggesting wide horizontal sword swing, brightest pure white concentrated along the outer curved edge representing blade travel path, smooth gradient fade inward toward inner arc edge becoming medium gray then black, arc thickness 40 pixels at widest center point tapering gradually to approximately 15 pixels at both pointed ends, overall shape resembles curved sword swing motion trail, smooth anti-aliased edges on outer curve for clean appearance, sharp tapered points at both arc endpoints, absolutely no sword blade or handle visible just the pure motion trail, grayscale values only no color, designed for melee weapon VFX overlay --ar 2:1 --v 6.1 --style raw --no weapon blade handle color details particles character environment
```

### 2B. Vertical Overhead Slash Trail
```
Vertical curved arc on solid black background representing overhead sword swing motion, arc curves slightly in S-shape like natural overhead-to-ground slashing motion path, brightest pure white at top of arc where swing begins fading to medium gray then to transparent at bottom where swing ends, represents top-to-bottom slashing motion energy trail, arc width approximately 50 pixels at top origin point tapering to sharp point at bottom termination, total height 450 pixels, smooth continuous gradient along entire length simulating motion blur energy dissipation, slight natural S-curve suggesting realistic swing mechanics not perfectly straight, crisp clean outer edge with soft inner fade, grayscale only absolutely no colors, 2D game sprite --ar 1:2 --v 6.1 --style raw --no weapon sword character color horizontal elements environment
```

### 2C. Combo Finisher Impact Arc
```
Dramatic curved crescent arc with starburst emanating from impact point on solid black background, main crescent arc spans approximately 90 degrees, pure white brightest intensity concentrated at the point where arc terminates representing impact moment, 8 sharp pointed white rays emanating outward radially from impact point each ray 30-50 pixels long with varying lengths for organic feel, rays have gradient fade from bright white base to gray at pointed tips, main arc itself has standard gradient fade from outer edge inward, combined effect suggests powerful finishing blow with explosive energy discharge at point of impact, all elements grayscale values only no color, arc and starburst combined in single cohesive sprite composition --ar 3:2 --v 6.1 --style raw --no character weapon color environment multiple separate arcs blood
```

### 2D. Full Rotation Spiral Trail
```
Complete 360-degree spiral trail on solid black background representing spinning attack motion, spiral starts tight in center approximately 30 pixel diameter and expands outward smoothly, brightest pure white on outermost newest part of spiral representing current position, smooth continuous gradient fade toward center becoming progressively grayer representing older fading trail, exactly 3 complete spiral rotations visible from center to edge, consistent line thickness of 20 pixels throughout entire spiral, perfectly smooth continuous curved line with no breaks gaps or discontinuities, resembles spinning weapon attack motion trail, total outer diameter approximately 450 pixels, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no character weapon color particles center object environment
```

### 2E. Double-Layer Smear Effect
```
Wide curved arc slash on solid black background featuring TWO distinct visible layers for depth effect, outer layer is very soft diffuse glow approximately 80 pixels total thickness with heavy gaussian blur creating atmospheric haze effect in medium gray 50% brightness, inner layer is brighter sharper approximately 30 pixels thick with cleaner edges in bright white, both layers follow identical curved arc path with perfect alignment, outer diffuse layer creates sense of energy aura around inner concentrated slash, arc spans approximately 100 degrees, designed to create layered depth effect of energy slash with atmospheric glow, grayscale values only absolutely no color --ar 2:1 --v 6.1 --style raw --no color weapon character particles single layer only
```

---

## 3. ENERGY BEAM TEXTURES

### 3A. Beam Core Segment (Tileable)
```
Perfectly horizontal laser beam core segment on solid black background, specifically designed for seamless horizontal tiling, brightest pure white line exactly 8 pixels tall running through exact vertical center of composition, soft gaussian glow extending 20 pixels above and 20 pixels below center line fading from white to medium gray to black, total height 64 pixels, absolutely seamless on left and right edges with no visible seam when tiled horizontally, no variation along horizontal length maintaining perfectly uniform appearance, resembles concentrated laser beam energy center, grayscale only no color --ar 8:1 --v 6.1 --style raw --no color particles sparks variation end caps terminations
```

### 3B. Beam Outer Glow (Tileable)
```
Horizontal beam edge atmospheric glow segment on solid black background, specifically designed for seamless horizontal tiling, NO bright center core just soft diffuse ambient glow, widest brightest portion in exact vertical center approximately 30% gray intensity, smooth gaussian falloff above and below center fading to transparent black at top and bottom edges, total height 128 pixels, absolutely seamless on left and right edges for perfect tiling, designed to complement and overlay beam core texture as outer atmospheric energy layer, grayscale values only no color --ar 8:1 --v 6.1 --style raw --no bright center core color particles sharp edges
```

### 3C. Beam Muzzle Flare Origin
```
Circular starburst pattern on solid black background representing beam origin emission point, bright pure white point at exact center of composition, 12 sharp pointed rays extending outward from center evenly spaced exactly 30 degrees apart, rays have brightest white intensity at base near center fading to medium gray at pointed tips, ray lengths vary slightly between 60-80 pixels for organic natural feel rather than mechanical uniformity, soft circular glow halo behind rays approximately 40 pixels radius providing additional bloom, designed as beam starting point muzzle flash VFX, total composition diameter approximately 200 pixels, grayscale only no color --ar 1:1 --v 6.1 --style raw --no color weapon character beam line environment
```

### 3D. Beam Impact Splash
```
Radial splash dispersion pattern on solid black background representing beam hitting surface, small bright white circle at exact center approximately 20 pixels diameter representing impact point, 16 teardrop-shaped splash droplets radiating outward from center in even distribution, splashes are brightest white near center base fading to gray at pointed tips, splash lengths vary between 40-70 pixels for organic scattered appearance, even angular spacing between splashes approximately 22.5 degrees apart, suggests energy beam hitting solid surface and dispersing outward, additional 8-12 small scattered dot particles fading outward beyond main splashes, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color surface wall environment beam line projectile
```

### 3E. Beam End Cap Termination
```
Horizontal pointed beam termination cap on solid black background, left edge designed to connect seamlessly to tileable beam core segment, tapers smoothly to sharp point on right side, brightest pure white on left edge matching beam core intensity and height, smooth continuous gradient taper becoming progressively grayer toward pointed termination, total length 128 pixels from left edge to point, height matches beam core at 64 pixels on left edge, pointed tip narrows to approximately 3 pixels at termination, represents clean beam endpoint where energy dissipates, grayscale only no color --ar 2:1 --v 6.1 --style raw --no color particles glow extending beyond point
```

---

## 4. LIGHT RAY / GOD RAY TEXTURES

### 4A. Radial God Rays Full Circle
```
Radial volumetric light rays emanating from exact center point on solid black background, 24 individual rays evenly spaced exactly 15 degrees apart around full 360-degree circle, rays are triangular wedge shapes brightest pure white at center point origin fading to transparent gray at outer edges, each ray length approximately 230 pixels from center to tip, rays have soft feathered edges not sharp hard lines, gaps between rays fade smoothly to black, creates atmospheric volumetric light effect like sun through clouds, total composition diameter approximately 500 pixels, center point is brightest, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color sun light source environment center object solid shapes
```

### 4B. Single Directional Light Shaft
```
Single vertical volumetric light beam shaft on solid black background, perfectly vertical orientation with no tilt, width 80 pixels at top edge tapering slightly to 60 pixels at bottom edge, brightest pure white concentrated in center vertical axis, soft gaussian horizontal falloff to medium gray then black at left and right edges, subtle dust particle dots scattered randomly within beam area 20-30 small white points of varying sizes 2-6 pixels, total height 500 pixels, represents single atmospheric volumetric light ray like light through window, grayscale values only no color --ar 1:4 --v 6.1 --style raw --no color window light source environment horizontal elements ground floor
```

### 4C. Explosion Flash Burst
```
Intense radial starburst flash on solid black background representing explosion or impact flash, very bright pure white center circle approximately 40 pixels diameter at maximum intensity, 16 sharp pointed rays extending outward from center with hard bright edges on sides tapering to pointed tips, ray lengths vary between 80-120 pixels for dynamic organic appearance, additional 8 shorter secondary rays positioned between main rays at 45-degree offsets approximately 40-60 pixels long, creates intense explosive flash effect, outer edge of composition has soft glow falloff, total diameter approximately 300 pixels, grayscale values only with high contrast between white elements and black background --ar 1:1 --v 6.1 --style raw --no color smoke debris fire environment explosion cloud
```

### 4D. Concentrated Light Cone
```
Triangular light cone shape on solid black background representing focused directional light beam, apex point positioned at top center of composition, cone expands downward symmetrically at approximately 45-degree angle on each side from apex, brightest pure white concentrated along center vertical axis of cone, smooth gaussian gradient fade to medium gray toward left and right edges of cone, bottom edge of cone is soft diffuse and feathered not sharp or hard, height approximately 400 pixels from apex to bottom, width at bottom approximately 300 pixels, represents focused directional spotlight or flashlight beam, grayscale values only no color --ar 3:4 --v 6.1 --style raw --no color light source object fixture environment floor surface
```

---

## 5. PARTICLE TRAIL TEXTURES

### 5A. Comet Trail Gradient Fade
```
Horizontal comet tail trailing wake on solid black background, NO comet head or source object just the trailing tail portion, brightest pure white on left side representing most recent freshest part of trail, smooth continuous unbroken gradient fade from white to medium gray to transparent black toward right side representing older dissipating trail, slight vertical thickness variation with thicker section approximately 40 pixels on left tapering to approximately 10 pixels on right, total length 900 pixels for long dramatic trail, wispy tendrils at right end showing 3-4 thin strands separating and fading, designed for projectile wake trail effect, grayscale values only no color --ar 8:1 --v 6.1 --style raw --no comet head color particles sharp edges sphere projectile source
```

### 5B. Sparkle Particle Field
```
Scattered four-pointed star sparkle particles distributed across composition on solid black background, 40-50 individual 4-pointed star shapes with points at 0 90 180 270 degrees, stars vary in size from small 8 pixels to large 24 pixels diameter, larger stars have brighter pure white centers with visible points, smaller stars are grayer and more subtle, random organic distribution across canvas with slight concentration toward left side suggesting motion direction, each star has 4 sharply pointed rays with soft circular glow bloom around center point, minimum spacing between stars approximately 20 pixels to avoid overlap, grayscale values only no color, designed for magical sparkle trail overlay --ar 4:1 --v 6.1 --style raw --no color character wand continuous line 6-pointed stars circles
```

### 5C. Ember Particle Scatter
```
Scattered circular ember particles distributed across composition on solid black background, 60-80 individual perfectly round circular particles, particles vary in size from small 4 pixels to large 16 pixels diameter, all particles are soft circles with gaussian blur feathered edges not sharp hard circles, larger particles are brighter pure white smaller particles are grayer more faded, random organic distribution pattern suggesting upward drift motion with more particles concentrated on left side fewer scattered toward right, NO flames fire triangular shapes or pointed elements just round glowing soft circular particles, grayscale values only absolutely no orange red or fire colors --ar 4:1 --v 6.1 --style raw --no color flames fire sharp shapes triangles pointed elements orange red yellow
```

### 5D. Lightning Arc Branch Pattern
```
Branching lightning bolt electrical arc pattern on solid black background, main central trunk line starts on left side and extends toward right with 3-4 major sharp angular direction changes creating zigzag path, 6-8 smaller secondary branches fork off main trunk at various points along its length, all line segments are thin approximately 4-6 pixels wide with slight soft glow extending 2-3 pixels around edges, brightest pure white on main trunk with branches slightly grayer, all segments are sharp angular straight lines not curved, branch lines get progressively thinner as they extend further from main trunk, total length approximately 450 pixels from left to right, grayscale values only absolutely no blue purple or electrical colors --ar 4:1 --v 6.1 --style raw --no color blue electricity source character curves smooth lines
```

### 5E. Dissolving Particle Trail
```
Horizontal particle trail showing progressive dissolution and fade on solid black background, left side contains dense tightly clustered overlapping soft circular particles creating nearly solid mass, progressively fewer and more scattered particles toward right side showing dissolution, leftmost particles are bright pure white and larger approximately 20 pixels diameter, rightmost particles are small approximately 4 pixels and gray faded, demonstrates particle system fading and dispersing over distance traveled, minimum 100 individual particles total in composition, smooth continuous density gradient from dense to sparse, grayscale values only no color --ar 4:1 --v 6.1 --style raw --no color sharp shapes character source object
```

---

## 6. BLOOM / GLOW OVERLAYS

### 6A. Perfect Soft Circular Bloom
```
Single perfectly circular glow bloom on solid black background, exact geometric center is brightest pure white at maximum intensity, perfectly smooth radial gaussian gradient falloff from white center to medium gray to black at edges, absolutely NO visible hard edge boundary or ring just smooth continuous unbroken fade to transparent, center bright area approximately 60 pixels diameter at full intensity, total visible glow extends to approximately 200 pixels diameter before becoming imperceptible, mathematically perfect circle with no irregularities wobble or organic variation, designed for point light source glow overlay effect, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color texture noise hard edges ring boundary shape irregularity oval
```

### 6B. Hexagonal Bokeh Array
```
Horizontal row of 7 hexagonal bokeh lens flare shapes on solid black background, arranged in straight horizontal line, center hexagon is largest approximately 60 pixels diameter and brightest pure white, hexagons decrease in size and brightness symmetrically toward both ends with smallest approximately 20 pixels, each hexagon has soft glowing gaussian fill brightest in hexagon center fading toward edges, hexagons slightly overlap at edges creating connected lens flare appearance, spacing creates classic camera lens artifact pattern, all shapes are geometrically perfect 6-sided regular hexagons, grayscale values only no color --ar 2:1 --v 6.1 --style raw --no color circles irregular shapes light source sun environment
```

### 6C. Horizontal Anamorphic Streak
```
Long horizontal anamorphic lens flare streak on solid black background, brightest pure white thin line running through exact horizontal center approximately 2 pixels tall, soft vertical glow extending approximately 30 pixels above and 30 pixels below center line, glow intensity is strongest and widest at horizontal center of streak and tapers toward both ends, total length approximately 500 pixels total height approximately 80 pixels, resembles anamorphic widescreen camera lens artifact light streak, perfectly straight horizontal with no curves wobble or variation, grayscale values only no color --ar 4:1 --v 6.1 --style raw --no color vertical elements light source curves environment
```

### 6D. Multi-Ring Concentric Glow Stack
```
Four concentric circular glows sharing same center point on solid black background creating layered depth bloom effect, innermost circle approximately 30 pixels diameter brightest pure white at maximum intensity, second ring approximately 80 pixels diameter at 75% brightness medium-bright gray, third ring approximately 150 pixels diameter at 50% brightness medium gray, outermost ring approximately 250 pixels diameter at 25% brightness light gray, all rings have soft gaussian feathered edges blending smoothly, rings share exact same geometric center point with no offset, creates layered multi-level bloom depth effect for intense light sources, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color sharp edges solid fills hard boundaries texture
```

---

## 7. SCREEN EFFECT TEXTURES

### 7A. Radial Speed Lines Zoom Blur
```
Radial speed lines emanating from exact center point on solid black background creating zoom blur motion effect, 64 perfectly straight thin lines evenly distributed around full 360 degrees from center, lines are thin approximately 2 pixels wide, lines start 50 pixels away from center point and extend outward to edge of canvas, lines are brightest pure white intensity near center fading to gray at outer ends, creates intense zoom blur speed rush effect, center area within 100 pixels diameter remains completely empty black with no lines, lines maintain consistent even angular spacing, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color curves center object character spiral
```

### 7B. Vignette Overlay Mask
```
Rectangular vignette gradient mask for screen darkening overlay, center area is completely transparent showing as black in output, corners and edges transition to pure white, smooth elliptical radial gradient transition from transparent center to white corners and edges, gradient transition begins approximately 30% inward from edges, creates oval-shaped clear transparent center viewing area with darkened white corners for inverse masking, perfectly symmetrical on both horizontal and vertical axes, 16:9 widescreen aspect ratio, grayscale gradient values only no color patterns or textures --ar 16:9 --v 6.1 --style raw --no color texture pattern center content sharp edges
```

### 7C. Heat Distortion Noise Map
```
Seamless tileable organic perlin noise pattern in pure grayscale only, smooth organic blobby cloud-like shapes with no recognizable forms, mid-gray base tone approximately 50% gray overall average brightness, lighter white areas and darker black areas blend smoothly together with no sharp transitions or hard edges, noise scale produces medium-sized blobs approximately 30-50 pixels across, specifically designed for UV coordinate distortion shader displacement mapping, MUST tile seamlessly on all four edges with no visible seams, no recognizable shapes patterns or repeating elements just organic random noise, exactly 512x512 pixels square --ar 1:1 --v 6.1 --style raw --no color sharp edges recognizable shapes high contrast patterns regular grid
```

### 7D. Chromatic Ring Distortion Map
```
Concentric circular rings pattern on solid black background for radial chromatic aberration distortion mapping, 8-10 rings expanding outward from exact center point, rings alternate strictly between pure white and pure black creating high-contrast stripe pattern, innermost rings have tighter closer spacing outer rings have progressively wider spacing, all ring edges have slightly soft anti-aliased edges not perfectly hard, exact center point is pure white, designed for radial chromatic distortion UV displacement effect, grayscale values only with maximum contrast --ar 1:1 --v 6.1 --style raw --no color solid fill gradient blur soft only
```

---

## 8. IMPACT / HIT EFFECT TEXTURES

### 8A. X-Shaped Slash Impact Cross
```
X-shaped crossed slash impact marks on solid black background, two diagonal lines crossing at exact center of composition, each line approximately 200 pixels long and 15 pixels thick at center crossing point tapering to sharp points at all four ends, center intersection crossing point has bright white circular glow approximately 40 pixels diameter creating impact flash, lines are bright pure white with soft glow edges extending 3-4 pixels beyond main stroke, lines positioned precisely at 45-degree and 135-degree angles creating perfect X shape, represents melee weapon impact slash mark effect, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color blood character weapon curves environment
```

### 8B. Expanding Shockwave Ring
```
Single circular ring on solid black background representing expanding shockwave at single frozen moment, ring has thick stroke width approximately 25 pixels, ring outer diameter approximately 300 pixels, brightest pure white concentrated on outer edge of ring stroke, gradient fade to gray on inner edge of ring stroke creating dimensional thickness appearance, exact center interior of circle is completely empty black with no fill, ring represents single frame of expanding shockwave propagation, geometrically perfect circular with no distortion or wobble, slight soft glow extending outside outer edge approximately 5 pixels, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color filled circle multiple rings debris dust particles center object
```

### 8C. 4-Point Star Impact Burst
```
Four-pointed star burst impact shape on solid black background, bright pure white center circle approximately 30 pixels diameter at maximum intensity, 4 long primary pointed rays extending precisely up down left right at 0 90 180 270 degrees, each primary ray approximately 100 pixels long, rays are widest approximately 25 pixels at base where they connect to center tapering to sharp points approximately 3 pixels wide at tips, rays have bright white centers with soft glow feathered edges, 4 smaller secondary rays at 45-degree angles between primary rays approximately 50 pixels long, creates critical hit flash explosion effect, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color particles debris curves environment 6-point 8-point
```

### 8D. Concentric Impact Ripple Rings
```
Three concentric circular rings on solid black background representing expanding impact ripple effect, innermost ring approximately 80 pixels diameter brightest pure white at maximum intensity, middle ring approximately 160 pixels diameter at medium gray brightness, outermost ring approximately 260 pixels diameter at light gray brightness, all three rings have consistent 8 pixel stroke thickness, all rings share exact same geometric center point, space between rings is completely black with no fill, represents frozen moment of expanding impact ripple propagation, each ring has soft glow on outer edges, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color filled circles solid shapes particles debris
```

### 8E. Directional Hit Slash Mark
```
Single diagonal slash mark on solid black background, slash runs from upper left corner area toward lower right corner area, stroke width approximately 30 pixels at thickest center section tapering to sharp points at both upper-left and lower-right ends, total slash length approximately 250 pixels, bright pure white core with soft glow extending approximately 10 pixels beyond stroke edges, very slight subtle curve suggesting natural slashing motion arc not perfectly straight, represents directional hit damage indicator showing attack direction, positioned in center of composition with equal spacing to edges, grayscale values only no color --ar 1:1 --v 6.1 --style raw --no color blood character weapon X-shape environment cross multiple slashes
```

---

## 📋 USAGE NOTES

### Color Tinting System

All textures are **pure white/grayscale** specifically designed for runtime color multiplication in game engine:

```csharp
// In-game C# color tinting example:
Color finalColor = textureColor * themeColor;

// Examples:
// White (255,255,255) * Cyan (0,255,255) = Pure Cyan (0,255,255)
// Gray (128,128,128) * Cyan (0,255,255) = Dark Cyan (0,128,128)
// Black (0,0,0) * Any Color = Black (0,0,0) - remains transparent in additive

// Additive blending formula:
// Result = Source * SourceAlpha + Destination
// White textures become fully tinted, gray becomes darker tint, black adds nothing
```

### File Specifications

| Property | Requirement |
|----------|-------------|
| Format | PNG with alpha transparency channel |
| Color Mode | Grayscale or RGB (white values only, no colors) |
| Background | Pure solid black (#000000) for additive blend compatibility |
| Dimensions | Power of 2 preferred (256, 512, 1024 pixels) |
| Bit Depth | 8-bit standard or 16-bit for fine gradients |

### Asset Folder Structure

```
Assets/
├── Particles/           # Small individual particle sprites (32-128px)
├── VFX/
│   ├── Trails/         # Motion trails, smears, wakes (elongated aspect)
│   ├── Beams/          # Laser/beam segments (tileable horizontal)
│   ├── Blooms/         # Glow overlays, flares (centered circular)
│   ├── Impacts/        # Hit effects, slashes (centered)
│   └── Screen/         # Full-screen overlays (screen resolution)
```

### Naming Convention

```
[Category]_[Type]_[Variant].png

Examples:
Trail_CometTail_Long.png
Beam_Core_Tileable.png
Impact_StarBurst_4Point.png
Bloom_CircularSoft_256.png
Smear_CrescentArc_Wide.png
Screen_Vignette_16x9.png
```
