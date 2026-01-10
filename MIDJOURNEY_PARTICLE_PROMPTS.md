# Midjourney Particle Effect Template Prompts

This document contains highly detailed, optimized prompts for generating white particle effect sprite sheets suitable for Terraria modding, similar to Calamity's particle system approach.

---

## Understanding the Goal

Calamity uses **white/grayscale particle textures** that are then **tinted in code** to any color. This approach allows a single particle sprite to be reused across hundreds of effects by simply changing the `Color` parameter when drawing. The particles should be:

- **Pure white or grayscale** (no color information)
- **Transparent backgrounds** (PNG with alpha channel)
- **Small resolution** (typically 8x8 to 64x64 pixels)
- **Clean, anti-aliased edges** that blend well when scaled
- **Centered** in the image for proper rotation
- **Varied shapes** for different effect types

---

## MASTER PROMPT: Soft Glow Particles (Most Versatile)

```
white particle effect sprite sheet, 8 variations in 2x4 grid layout, pure white soft circular glow particles on transparent background, each particle is a different softness gradient from hard-edged circle to extremely soft gaussian blur falloff, smooth anti-aliased edges, radial gradient from bright white center fading to transparent edges, professional game asset quality, pixel-perfect clean design, suitable for 2D game particle systems, 32x32 pixel sprites upscaled 8x for detail, PNG with alpha transparency, no color only luminosity values, studio lighting reference for glow falloff, each particle variation shows different falloff curves: linear, quadratic, exponential, inverse square, soft gaussian, hard rim, medium blend, feathered edge, flat orthographic view, centered composition, isolated on pure black background for easy extraction, sprite sheet format --v 6.1 --ar 2:1 --style raw --s 50
```

---

## PROMPT 2: Energy Spark/Flare Particles

```
white energy spark particle sprite sheet, 12 variations in 3x4 grid, pure white and grayscale only, transparent background, includes: sharp 4-pointed stars, 6-pointed stars, 8-pointed lens flares, soft diamond sparkles, elongated streak sparks, round soft glows with bright cores, small pinpoint highlights, medium soft orbs, large diffuse glows, asymmetric organic sparks, electric arc fragments, plasma wisps, all with smooth anti-aliased edges, radial symmetry where appropriate, professional 2D game particle assets, suitable for magic effects fire sparks lightning electricity, each sprite isolated and centered, 32x32 pixel base resolution upscaled for detail, bright white cores with soft falloff to transparent, no color information pure luminosity only, clean vector-like quality with smooth gradients, game-ready sprite sheet format, black background for extraction --v 6.1 --ar 3:2 --style raw --s 75
```

---

## PROMPT 3: Smoke/Cloud/Vapor Particles

```
white smoke cloud particle sprite sheet, 16 variations in 4x4 grid layout, pure white and grayscale smoke puffs on transparent background, organic natural cloud shapes with wispy edges, includes: small tight smoke puffs, large billowing clouds, thin wispy tendrils, dense fog patches, dissipating vapor trails, cotton-like soft clouds, sharp edged stylized smoke, rounded cumulus shapes, stretched motion blur smoke, spiral smoke wisps, layered depth clouds, ethereal mist patches, each particle has soft semi-transparent edges that blend naturally, suitable for 2D game particle systems, 64x64 pixel sprites upscaled, professional game asset quality, varied opacity gradients within each sprite, no hard edges only soft blended boundaries, centered composition for rotation, black background for easy extraction, PNG alpha transparency ready --v 6.1 --ar 1:1 --style raw --s 100
```

---

## PROMPT 4: Geometric Magic Symbols

```
white magic symbol particle sprite sheet, 20 variations in 5x4 grid, pure white geometric shapes on transparent background, includes: simple circles, double circles, triple concentric rings, pentagrams, hexagrams, octagons, runic circles, sacred geometry patterns, spiral symbols, crescent moons, star shapes of varying points 4 5 6 7 8, diamond rhombus shapes, cross patterns, celtic knot fragments, mandala segments, arcane sigils, alchemical symbols simplified, each symbol has clean sharp edges with subtle soft glow aura, professional vector quality, suitable for magic spell effects summoning circles buff indicators, 32x32 pixel base upscaled, no color pure white only, game-ready 2D sprite sheet, each symbol centered and isolated, varies line thickness thin medium bold, black background --v 6.1 --ar 5:4 --style raw --s 50
```

---

## PROMPT 5: Impact/Explosion Burst Particles

```
white explosion burst particle sprite sheet, 12 variations in 4x3 grid, pure white and grayscale impact effects on transparent background, includes: radial starburst explosions, circular shockwave rings, expanding impact circles, debris scatter patterns, directional cone blasts, omnidirectional burst rays, soft bloom explosions, hard-edged flash bursts, layered ring explosions, asymmetric organic explosions, speed line impacts, energy nova effects, each with bright white center fading outward, clean anti-aliased edges suitable for scaling, professional game particle assets, motion-implying design with radial symmetry, 64x64 pixel base resolution upscaled, suitable for hit effects explosions spell impacts, no color pure luminosity, centered composition, black background for extraction, PNG alpha ready --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 6: Trail/Streak/Motion Particles

```
white motion trail particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale streak effects on transparent background, includes: tapered comet tails, soft gradient trails, sharp speed lines, curved arc trails, spiral motion paths, elongated stretched orbs, directional blur streaks, fading ghost trails, ribbon-like flowing trails, particle stream paths, energy beam segments, lightning bolt fragments, afterimage echoes, motion smear effects, acceleration trails, deceleration fade trails, each has natural falloff from bright leading edge to transparent trailing edge, anti-aliased smooth edges, suitable for projectile trails movement effects speed particles, 64x16 pixel elongated sprites upscaled, professional game asset quality, horizontally oriented for easy rotation, black background, PNG alpha transparency --v 6.1 --ar 2:1 --style raw --s 75
```

---

## PROMPT 7: Dust/Debris/Small Particle Clusters

```
white micro particle cluster sprite sheet, 32 variations in 8x4 grid, pure white and grayscale tiny particles on transparent background, includes: single dot particles of varying sizes 1-4 pixels, small 2-3 particle clusters, medium 4-6 particle groups, scattered debris patterns, aligned particle rows, circular particle arrangements, random scatter patterns, dense particle clouds, sparse floating specks, directional particle sprays, settled dust patterns, floating ambient particles, each cluster shows different density and arrangement, very small scale 8x8 to 16x16 pixel sprites upscaled, soft anti-aliased edges even on smallest particles, suitable for ambient dust debris sparkle effects, no color pure white only, professional game particle assets, centered in frame, black background for extraction --v 6.1 --ar 2:1 --style raw --s 50
```

---

## PROMPT 8: Ring/Halo/Aura Effects

```
white ring halo particle sprite sheet, 12 variations in 4x3 grid, pure white and grayscale circular ring effects on transparent background, includes: thin sharp rings, thick soft rings, double concentric rings, triple nested rings, broken/dashed rings, glowing aura circles, soft halo effects, ring with inner glow, ring with outer glow, fading gradient rings, pulsing ring effect frames, expanding shockwave rings, each ring centered with transparent center hole, smooth anti-aliased curves, suitable for buff indicators shields auras magic circles, 64x64 pixel sprites upscaled, professional game asset quality, varying ring thickness from hairline to bold, soft falloff on edges, no color pure luminosity only, black background for easy extraction --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 9: Musical Note Particles (Theme-Specific)

```
white musical notation particle sprite sheet, 24 variations in 6x4 grid, pure white and grayscale musical symbols on transparent background, includes: quarter notes, eighth notes, sixteenth notes, half notes, whole notes, treble clef, bass clef, sharp symbols, flat symbols, natural symbols, rest symbols quarter eighth whole, beamed note pairs, beamed note triplets, musical staff fragments, crescendo decrescendo marks, fermata, accent marks, staccato dots, tied notes, chord clusters, arpeggiated notes, grace notes, each symbol clean sharp vector quality with subtle soft glow aura, professional typography reference, suitable for music-themed game effects, 32x32 pixel base upscaled, no color pure white only, centered composition, black background, PNG alpha ready --v 6.1 --ar 3:2 --style raw --s 50
```

---

## PROMPT 10: Feather/Petal/Organic Particles

```
white organic particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale natural shapes on transparent background, includes: simple feather shapes, detailed feather with barbs, cherry blossom petals 5-petal, rose petals curved, maple leaf shapes, simple leaves, floating seeds, dandelion fluff, snowflake crystals, water droplets, flower buds, grass blade fragments, vine tendrils, organic curved wisps, butterfly wing fragments, scale/shell fragments, each shape has natural organic curves and soft edges, suitable for nature magic effects elemental particles, 32x32 pixel sprites upscaled, professional game asset quality, no color pure white luminosity only, varied orientations for natural scatter, black background for extraction, PNG alpha transparency --v 6.1 --ar 1:1 --style raw --s 100
```

---

## POST-PROCESSING TIPS

After generating with Midjourney:

1. **Remove Background**: Use Photoshop/GIMP to ensure pure transparent background
2. **Convert to Grayscale**: Remove any color cast, keep only luminosity
3. **Adjust Levels**: Ensure full white (255) in brightest areas, pure transparent in darkest
4. **Downscale**: Reduce to target resolution (8x8, 16x16, 32x32, 64x64)
5. **Split Sprites**: Cut sprite sheet into individual files
6. **Test Tinting**: Load in Terraria and test with `spriteBatch.Draw()` color parameter
7. **Optimize**: Use PNG compression, ensure 32-bit RGBA format

---

## IMPLEMENTATION IN TERRARIA

```csharp
// Example: Using white particle with color tinting
Texture2D whiteParticle = ModContent.Request<Texture2D>("YourMod/Particles/SoftGlow").Value;

// Tint to any color at draw time
Color eroicaRed = new Color(255, 80, 60);
Color moonlightPurple = new Color(180, 100, 255);

spriteBatch.Draw(whiteParticle, position, null, eroicaRed * 0.8f, 
    rotation, origin, scale, SpriteEffects.None, 0f);
```

This approach allows ONE particle texture to serve UNLIMITED color variations!

---

---

## PROMPT 11: Crystallized Fractal Effects

```
white crystalline fractal particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale crystal formations on transparent background, includes: sharp geometric crystal clusters, hexagonal ice crystal structures, branching frost fractals, angular quartz-like shards, symmetric snowflake crystal patterns, asymmetric organic crystal growth, faceted gem surfaces with light refraction lines, crystalline dendrite formations, sacred geometry crystal lattices, shattered crystal fragment sprays, growing crystal branch patterns, frozen geometric spires, mineral vein formations, prismatic crystal faces with edge highlights, recursive fractal crystal patterns, geode-like circular crystal arrangements, each crystal has sharp defined edges with subtle inner glow suggesting light passing through, professional game asset quality, suitable for ice magic gem effects mineralization, 64x64 pixel base upscaled, clean vector-like faceted surfaces, bright white highlights on crystal edges, black background for extraction --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 12: Advanced Crystal Shard Particles

```
white crystal shard debris particle sprite sheet, 24 variations in 6x4 grid, pure white and grayscale shattered crystal pieces on transparent background, includes: single sharp crystal spikes, double-pointed crystal shards, clustered mini crystal formations, broken faceted fragments, elongated crystal needles, thick crystal chunks, thin crystal slivers, diamond-shaped crystal pieces, triangular crystal shards, irregular shattered pieces, scattered crystal dust particles, crystalline powder clusters, polished gem fragments, raw uncut crystal bits, translucent crystal layers stacked, crystal with internal fracture lines, each shard has hard geometric edges with soft luminous glow, suggests refracted light within crystal body, 32x32 pixel sprites upscaled, suitable for shatter effects crystal explosions gem breaking, no color pure luminosity, centered for rotation, black background PNG alpha --v 6.1 --ar 3:2 --style raw --s 50
```

---

## PROMPT 13: Fractal Energy Formations

```
white fractal energy pattern sprite sheet, 12 variations in 4x3 grid, pure white and grayscale mathematical fractal shapes on transparent background, includes: Mandelbrot-inspired spiral formations, Julia set branching patterns, Sierpinski triangle fragments, Koch snowflake recursive edges, fractal fern-like growth patterns, lightning-bolt fractal branches, spiral golden ratio formations, crystalline Voronoi cell patterns, recursive tree branching, infinite zoom spiral effects, sacred geometry fractal mandalas, organic fractal coral-like structures, each pattern shows self-similar recursive detail at multiple scales, clean sharp edges with subtle energy glow aura, professional game particle assets, 64x64 pixel base upscaled 8x, suitable for cosmic magic reality-warping dimensional effects, no color pure white luminosity, black background for extraction --v 6.1 --ar 4:3 --style raw --s 100
```

---

## PROMPT 14: Explosive Burst Effects

```
white explosion effect sprite sheet, 16 variations in 4x4 grid, pure white and grayscale explosive bursts on transparent background, includes: classic starburst explosion with debris, circular shockwave with particle scatter, mushroom cloud silhouette burst, radial speed-line explosion, asymmetric chaotic blast, layered ring explosion sequence, directional cone blast with fragments, spherical nova expansion, ground-impact upward burst, firework-style scatter explosion, imploding then exploding pattern, smoke-edged blast wave, debris-heavy shrapnel burst, clean energy nova flash, turbulent roiling explosion cloud, geometric crystalline explosion, each burst has bright white hot center fading to cooler edges, motion blur implied through design, professional game VFX quality, 64x64 pixel base upscaled, suitable for weapon impacts spell explosions death effects, no color pure luminosity only, centered radial composition, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 15: Sequential Explosion Frames

```
white explosion animation sequence sprite sheet, 8 frames in 2x4 grid showing explosion lifecycle, pure white and grayscale on transparent background, frame 1: initial flash point bright core, frame 2: rapid expansion with emerging debris, frame 3: full bloom explosion maximum size, frame 4: beginning dissipation outer edges fading, frame 5: smoke formation edges softening, frame 6: debris scatter continuing outward, frame 7: dissipating smoke and fading glow, frame 8: final wisps and lingering particles, each frame 64x64 pixels upscaled, consistent center point for animation alignment, smooth transition between frames, professional game animation quality, suitable for animated explosion VFX impact effects, no color pure white luminosity, soft edges that blend when animated, black background for extraction --v 6.1 --ar 2:1 --style raw --s 50
```

---

## PROMPT 16: Shockwave Ring Explosions

```
white shockwave ring explosion sprite sheet, 12 variations in 4x3 grid, pure white and grayscale expanding ring effects on transparent background, includes: thin sharp expanding ring, thick soft pressure wave, double concentric shockwaves, triple nested expanding rings, broken/fragmenting shockwave, distortion ripple ring, ring with trailing particles, ring with central flash, fade-gradient expanding ring, ring with speed lines, hemisphere dome shockwave, ground-level horizontal shockwave, each ring shows moment of expansion with motion implied, bright leading edge fading trailing edge, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for impact waves explosions super attacks, no color pure luminosity, centered perfect circular composition, black background PNG alpha --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 17: Light Beam Effects

```
white light beam particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale beam effects on transparent background, includes: straight laser beam segment, tapered spotlight cone, soft volumetric light shaft, hard-edged geometric beam, beam with lens flare hotspots, beam with dust motes particles, converging focused beam, diverging spreading beam, beam with soft feathered edges, beam with hard core soft edge, pulsing intensity beam segment, beam with ray god-ray streaks, cylindrical column of light, flat ribbon light band, beam with atmospheric scatter, beam endpoint impact glow, each beam shows realistic light falloff and atmospheric interaction, professional game VFX quality, 64x32 elongated sprites upscaled, suitable for laser attacks divine light spotlight effects, no color pure white luminosity, horizontally oriented, black background --v 6.1 --ar 2:1 --style raw --s 75
```

---

## PROMPT 18: Divine Ray/God Ray Effects

```
white god ray light shaft sprite sheet, 12 variations in 4x3 grid, pure white and grayscale divine light beams on transparent background, includes: single dramatic light shaft, multiple parallel light rays, converging rays to focal point, diverging rays from source, rays with dust particles floating within, rays with soft atmospheric haze, rays breaking through clouds silhouette, rays with lens flare accents, volumetric scattered light beams, rays with bright source point, rays fading into distance, rays with edge diffraction effects, each ray has realistic falloff with brighter source fading outward, soft edges suggesting atmospheric scatter, professional cinematic VFX quality, 128x64 wide sprites upscaled, suitable for divine magic celestial effects dramatic lighting, no color pure white luminosity, vertical or diagonal orientation, black background for extraction --v 6.1 --ar 2:1 --style raw --s 100
```

---

## PROMPT 19: Concentrated Energy Beam Core

```
white energy beam core sprite sheet, 16 variations in 4x4 grid, pure white and grayscale beam center effects on transparent background, includes: intense bright beam core, beam core with electric crackling edges, beam core with spiral energy wrapping, beam core with particle stream, smooth gradient beam falloff, beam with sharp bright center line, beam with multiple internal strands, beam with pulsating brightness variation, beam core with outer halo glow, beam with trailing energy wisps, beam intersection crosspoint glow, beam with impact flare terminus, beam startup charging glow, beam sustained middle section, beam endpoint dissipation, beam with refractive edge distortion, each shows intense energy concentration, professional game VFX quality, 48x48 pixel sprites upscaled, suitable for laser weapons energy attacks beam spells, no color pure luminosity only, centered composition, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 20: Sword Smear/Slash Effects

```
white sword slash smear sprite sheet, 16 variations in 4x4 grid, pure white and grayscale melee weapon trails on transparent background, includes: horizontal straight slash, diagonal downward slash, diagonal upward slash, vertical overhead chop, curved crescent moon slash, full circular spin slash, figure-eight flourish trail, wide sweeping arc slash, tight quick jab trail, heavy two-handed cleave, dual blade X-cross slash, rising uppercut slash, descending hammer strike, thrust stab linear trail, curved katana draw slash, chaotic multi-hit combo trail, each slash shows motion blur with bright leading edge fading to transparent trail, sharp clean vector curves, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for melee combat sword attacks weapon trails, no color pure white luminosity, centered for positioning, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 21: Advanced Weapon Trail Smears

```
white weapon trail smear sprite sheet, 20 variations in 5x4 grid, pure white and grayscale motion trails on transparent background, includes: thin rapier precise slash, thick broadsword heavy swing, curved scimitar flowing arc, angular axe chopping trail, blunt hammer impact smear, spear thrust linear pierce, whip crack curving trail, chain weapon spiraling path, claw triple-line slash, fist punch blur trail, kick sweep arc, shield bash impact smear, bow draw energy trail, magical staff swing trail, scythe sweeping harvest arc, dagger quick stab trails, paired weapon synchronized trails, weapon spin 360 blur, charge attack elongated trail, combo finisher elaborate flourish, each trail has appropriate weight and speed implied through thickness and fade, professional game VFX, 64x48 sprites upscaled, no color pure luminosity, black background --v 6.1 --ar 5:4 --style raw --s 50
```

---

## PROMPT 22: Anime-Style Slash Effects

```
white anime slash effect sprite sheet, 12 variations in 4x3 grid, pure white and grayscale stylized sword cuts on transparent background, includes: classic anime single slash line, double parallel slash, triple claw slash marks, X-cross slash impact, crescent moon blade arc, circular omnislash ring, speed line burst slash, impact star slash, energy-charged glowing slash, afterimage echo slash trail, stylized Japanese calligraphy slash, comic book action line slash, each slash has characteristic anime styling with sharp attack edge and dramatic trailing fade, speed lines incorporated into design, professional manga-inspired game VFX, 64x64 pixel sprites upscaled, suitable for action games anime-style combat, no color pure white luminosity, dynamic diagonal compositions, black background for extraction --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 23: Prismatic Sparkle Effects

```
white prismatic sparkle particle sprite sheet, 24 variations in 6x4 grid, pure white and grayscale diamond sparkles on transparent background, includes: classic 4-point diamond sparkle, 6-point star sparkle, 8-point complex sparkle, soft round twinkle, hard-edged gem flash, lens flare sparkle, asymmetric organic sparkle, clustered mini sparkles, large dramatic sparkle, tiny subtle twinkle, sparkle with radiating rays, sparkle with soft halo, sparkle with inner star pattern, sparkle with outer ring, elongated horizontal sparkle, elongated vertical sparkle, rotating sparkle frame, pulsing sparkle intensity, scattered sparkle cluster, rainbow-positioned sparkle arc, sparkle burst spray pattern, fading sparkle trail, emerging sparkle birth, disappearing sparkle death, each sparkle designed for prismatic color tinting with bright white core, professional game VFX, 32x32 sprites upscaled, suitable for magic gems treasure highlights, no color pure luminosity, centered, black background --v 6.1 --ar 3:2 --style raw --s 50
```

---

## PROMPT 24: Gem Glitter and Shine Effects

```
white gem glitter shine sprite sheet, 20 variations in 5x4 grid, pure white and grayscale jewelry sparkle effects on transparent background, includes: single brilliant-cut gem flash, faceted surface reflection line, multiple facet sparkle cluster, gem edge highlight gleam, internal gem fire sparkle, surface polish shine, moving light across gem animation, gem catching light flash, subtle ambient gem glow, dramatic gem spotlight shine, gem with surrounding smaller sparkles, elongated gem surface reflection, circular gem crown flash, teardrop gem pendant sparkle, square-cut gem flash, emerald-cut rectangular shine, round brilliant maximum sparkle, gem casting light rays, gem prismatic rainbow edge hint, gem with floating magic particles, each designed to overlay on gem items for magical enhancement, professional game VFX quality, 32x32 sprites upscaled, no color for tinting flexibility, black background --v 6.1 --ar 5:4 --style raw --s 75
```

---

## PROMPT 25: Animated Sparkle Sequence

```
white sparkle animation sequence sprite sheet, 8 frames in 2x4 grid showing sparkle lifecycle, pure white and grayscale on transparent background, frame 1: tiny point initial appearance, frame 2: rapid expansion 4-point star forming, frame 3: maximum brightness full sparkle bloom, frame 4: secondary rays extending, frame 5: peak complexity with halo, frame 6: beginning fade outer rays retracting, frame 7: shrinking core remaining, frame 8: final tiny point fade out, each frame 32x32 pixels upscaled, consistent center point for animation playback, smooth professional transition between frames, suitable for looping sparkle effects or one-shot twinkle, game animation VFX quality, no color pure white luminosity for tinting, black background for extraction PNG alpha --v 6.1 --ar 2:1 --style raw --s 50
```

---

## PROMPT 26: Magical Aura Sparkle Fields

```
white magical sparkle field sprite sheet, 12 variations in 4x3 grid, pure white and grayscale distributed sparkle patterns on transparent background, includes: circular sparkle ring arrangement, spherical sparkle aura cloud, spiral sparkle galaxy pattern, random scattered ambient sparkles, vertical rising sparkle column, horizontal sparkle band, heart-shaped sparkle outline, star-shaped sparkle constellation, dense inner sparse outer sparkle gradient, sparse inner dense outer sparkle ring, sparkle trail comet shape, sparkle burst radial explosion, each pattern shows multiple sparkles at varying sizes and intensities creating cohesive magical aura effect, professional game VFX for buff indicators magic effects enchantments, 64x64 pixel sprites upscaled, no color pure luminosity for versatile tinting, black background for extraction --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 27: Arcing Sword Projectile Slash Effects

```
white arcing projectile slash sprite sheet, 16 variations in 4x4 grid, pure white and grayscale traveling blade waves on transparent background, includes: crescent moon wave projectile, curved sonic boom arc, spiraling blade vortex projectile, double helix intertwined slashes, boomerang returning arc trail, expanding crescent growing as it travels, contracting crescent shrinking projectile, wavy serpentine blade path, sharp angular zigzag slash, wide sweeping energy scythe, thin precise rapier wave, heavy cleaving greataxe arc, paired twin crescent projectiles, triple layered arc stack, fragmented breaking slash projectile, ethereal ghost slash afterimage, each designed as mid-flight projectile with motion blur trailing edge and sharp leading cutting edge, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for ranged sword attacks energy wave projectiles, no color pure luminosity for tinting, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 28: Musical Staff & Sound Wave Effects

```
white musical sound wave particle sprite sheet, 20 variations in 5x4 grid, pure white and grayscale audio visualization on transparent background, includes: sine wave smooth oscillation, aggressive sawtooth wave pattern, stacked harmonic overtone waves, musical staff with flowing notes, bass clef emanating sound rings, treble clef radiating energy, piano key ripple wave, violin bow stroke trail, crescendo building wave intensity, decrescendo fading wave, staccato sharp burst pulses, legato smooth connected waves, vibrato oscillating shimmer, musical rest pause void, chord stack vertical wave layers, arpeggio cascading wave steps, fermata sustained glow ring, tempo pulse beat markers, resonance sympathetic wave echo, dissonance chaotic wave interference, professional game VFX for music-themed abilities, 64x64 pixel sprites upscaled, no color pure white luminosity, black background --v 6.1 --ar 5:4 --style raw --s 75
```

---

## PROMPT 29: Piano Key Impact Effects

```
white piano key impact sprite sheet, 16 variations in 4x4 grid, pure white and grayscale keyboard-inspired effects on transparent background, includes: single key press ripple, octave span wave burst, chord cluster multi-key impact, ascending scale staircase trail, descending scale falling notes, glissando sliding blur trail, key hammer strike impact, string resonance vibration lines, damper pedal sustain glow, soft pedal muted halo, grand piano soundboard wave, upright piano vertical burst, ivory key smooth flash, ebony key sharp flash, broken chord scattered impact, rolled chord spiral effect, each effect captures piano playing dynamics, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for piano-themed weapons music magic, no color pure luminosity for tinting, black background for extraction --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 30: Swan Feather Projectile Trails

```
white feather projectile trail sprite sheet, 16 variations in 4x4 grid, pure white and grayscale elegant feather effects on transparent background, includes: single floating feather drift, spinning feather spiral descent, feather burst scatter explosion, feather trail ribbon stream, paired swan feathers intertwined, feather quill writing trail, downy soft feather cloud, sharp flight feather projectile, feather dissipating into particles, crystallized ice feather shard, feather with sparkle accents, feather transforming to energy, barb separation scatter effect, rachis spine energy beam, feather vane cutting edge blade, calamus base impact burst, each feather has elegant flowing curves with soft edges, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for swan-themed magic feather projectiles, no color pure white luminosity, black background --v 6.1 --ar 1:1 --style raw --s 100
```

---

## PROMPT 31: Orchestral Conductor Baton Trails

```
white conductor baton trail sprite sheet, 12 variations in 4x3 grid, pure white and grayscale orchestral command effects on transparent background, includes: downbeat strong vertical strike, upbeat lifting arc, lateral sweeping cue gesture, circular tempo pattern, figure-eight flowing pattern, sharp cutoff stop slash, gentle fadeout diminishing trail, accent sforzando burst, preparatory breath arc, fermata hold sustained glow, crescendo building intensity trail, subito sudden flash impact, each trail captures expressive conducting gestures with elegant flowing motion, bright attack fading to soft release, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for music-themed magic command abilities summoning, no color pure luminosity, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 32: Resonance & Harmonic Wave Effects

```
white harmonic resonance sprite sheet, 16 variations in 4x4 grid, pure white and grayscale standing wave patterns on transparent background, includes: fundamental frequency single wave, second harmonic double node, third harmonic triple pattern, overtone series stacked waves, sympathetic resonance echo rings, interference pattern constructive peaks, interference pattern destructive nulls, beating frequency pulsing pattern, resonant cavity standing wave, tuning fork dual prong vibration, string harmonic nodes glowing, air column resonance tube, Chladni plate geometric pattern, cymatics circular mandala, golden ratio spiral harmonic, Fibonacci sequence wave growth, each pattern shows physics-accurate wave behavior with musical beauty, professional game VFX, 64x64 pixel sprites upscaled, suitable for resonance-themed abilities harmonic cores, no color pure luminosity, black background --v 6.1 --ar 1:1 --style raw --s 100
```

---

## PROMPT 33: Dramatic Heroic Slash Impacts (Eroica Theme)

```
white heroic slash impact sprite sheet, 12 variations in 4x3 grid, pure white and grayscale dramatic attack effects on transparent background, includes: triumphant upward rising slash, defiant cross-guard parry flash, valor charge forward thrust trail, conquest cleaving overhead arc, glory radiant burst impact, destiny spiral ascending blade, heroic last stand desperate slash, victory fanfare burst explosion, march rhythm repeated strike marks, battlefield sweeping wide arc, commander rallying slash wave, legendary finishing blow impact, each slash conveys heroic weight and dramatic intensity with bold thick strokes and brilliant impact flashes, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for heroic warrior attacks boss abilities, no color pure luminosity, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 34: Moonlight/Ethereal Glow Effects (Moonlight Sonata Theme)

```
white moonlight ethereal glow sprite sheet, 16 variations in 4x4 grid, pure white and grayscale lunar luminescence on transparent background, includes: soft lunar halo ring, crescent moon silver arc, full moon radiant sphere, moonbeam descending shaft, lunar eclipse corona, moonlit mist wispy tendrils, silver starlight twinkle, nocturne gentle pulse, adagio slow fade glow, melancholic tear drop trail, reflection rippling water moon, moon phase transition sequence, selenite crystal moon shard, tidal pull energy wave, dreams floating orb drift, midnight bloom flower unfold, each effect captures serene melancholic lunar beauty with soft gradients and gentle light falloff, professional game VFX, 64x64 pixel sprites upscaled, suitable for moon-themed magic night abilities, no color pure luminosity, black background --v 6.1 --ar 1:1 --style raw --s 100
```

---

## PROMPT 35: Vinyl Record / Disc Projectile Effects

```
white spinning disc projectile sprite sheet, 12 variations in 4x3 grid, pure white and grayscale circular blade effects on transparent background, includes: vinyl record spinning with groove trails, CD disc prismatic edge flash, chakram circular blade ring, disc with motion blur rotation, wobbling unstable disc flight, perfectly stable gyroscopic disc, disc ricocheting angle change, disc returning boomerang arc, shattered disc fragment spray, disc cutting through air waves, stacked multiple disc volley, disc with sonic boom ring, each disc shows rotation motion with sharp cutting edges and satisfying spin dynamics, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for music-themed ranged weapons disc projectiles, no color pure luminosity for tinting, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 36: Violin String Vibration Effects

```
white violin string vibration sprite sheet, 16 variations in 4x4 grid, pure white and grayscale string instrument effects on transparent background, includes: single string pluck vibration, bow stroke sustained resonance, pizzicato sharp pluck burst, tremolo rapid oscillation, harmonic node point glow, string snap break effect, double stop two string wave, chord four string harmony, glissando sliding pitch trail, vibrato wobbling shimmer, sul ponticello harsh near-bridge, sul tasto soft fingerboard glow, col legno wood tap impact, spiccato bouncing bow dots, ricochet multiple bounce trail, string resonance sympathetic echo, each effect captures expressive violin technique with elegant flowing lines, professional game VFX, 48x64 elongated sprites upscaled, suitable for string instrument themed weapons, no color pure luminosity, black background --v 6.1 --ar 3:4 --style raw --s 75
```

---

## ADDITIONAL PARAMETERS TO EXPERIMENT WITH

- `--chaos 10-30` for more variation between grid items
- `--weird 50-200` for more unique/unusual particle shapes
- `--tile` for seamless patterns (useful for noise textures)
- `--no color saturation hue` to reinforce grayscale output
- Add `isolated on #000000 pure black` for easier extraction
- Add `sprite sheet game asset unity unreal godot` for game-relevant results

---

---

# BOSS SPRITE PROMPTS

## Swan Lake, The Monochromatic Fractal - Boss Concept Art

*A celestial deity boss with six massive swan wings, detached ethereal limbs, and an elegant yet foreboding cosmic presence. Main color palette: pure white, dark gray, black with pale pearlescent and chromatic accents.*

---

### PROMPT A: Full Body Front View (Primary Reference)

```
massive celestial deity boss character concept art, elegant cosmic feminine entity with six enormous swan wings arranged symmetrically behind her three on each side, arms and legs completely detached from torso floating separately held in place by rainbow pearlescent ethereal flames, outfit entirely black and white flowing cloth robes with pale pearlescent metallic accents and trim, cloth fabric interwoven with delicate swan feathers throughout the design, wearing a long dramatic flowing cloak billowing with cosmic energy, ornate swan crown floating above her head like a halo, eyes glowing with soft otherworldly light, expression serene yet intimidating foreboding presence, clean elegant design not chaotic or busy, front view perfectly centered symmetrical pose, arms extended outward welcoming yet threatening, highly detailed intricate pixel art style with very small fine pixels no large chunky pixels, massive scale imposing figure filling the frame, monochromatic color scheme pure white dark gray deep black with subtle chromatic iridescent light rays emanating from her form, pale pearlescent rainbow shimmer on metallic surfaces, cosmic starfield energy wisps surrounding her, professional game boss sprite quality, idle standing pose, transparent background for game asset extraction --v 6.1 --ar 1:1 --s 250 --style raw
```

---

### PROMPT B: Emphasis on Wings and Scale

```
towering celestial swan goddess boss sprite, six massive detailed swan wings unfurled behind elegant feminine deity figure, three wings each side arranged in majestic spread formation, each wing highly detailed with individual feathers visible, wings white fading to gray at tips with pearlescent sheen, body composed of flowing black and white ceremonial robes with feather-woven cloth texture, arms disconnected floating held by iridescent rainbow flame energy tendrils, legs separated from body suspended by chromatic pearlescent fire wisps, ornate silver-white metallic armor accents with pale rainbow iridescence, dramatic hooded cloak flowing behind mixing with wing silhouettes, swan crown ornament hovering above head radiating soft light, cosmic fractal patterns subtly embedded in cloth and wings, intimidating scale dwarfing surroundings, serene elegant face with knowing ancient eyes, perfectly symmetrical front-facing composition, very fine detailed pixel work no blocky pixels, monochrome black white gray palette with chromatic light accents only, foreboding divine presence, game boss character design, transparent background --v 6.1 --ar 3:4 --s 200 --style raw
```

---

### PROMPT C: Focus on Detached Limbs and Flames

```
ethereal cosmic deity boss character, elegant feminine figure with completely detached floating limbs, arms separated at shoulders hovering beside body held by rainbow pearlescent ghostly flames, legs disconnected at hips suspended by iridescent chromatic fire wisps, torso wrapped in intricate black and white layered cloth robes with feather details woven into fabric, six enormous swan wings spread behind her in symmetric arrangement, pale metallic pearlescent ornamental armor pieces on shoulders chest and waist, long flowing cloak with cosmic void pattern interior, floating swan crown above head serving as divine symbol, the disconnected limbs trail pearlescent flame ribbons connecting back to main body, each flame has subtle rainbow chromatic shift, highly detailed small pixel art style for game sprite, front view centered symmetrical idle pose, arms outstretched palms open, imposing massive scale, clean professional design elegant not chaotic, monochromatic white gray black with iridescent accents, transparent background for extraction --v 6.1 --ar 1:1 --s 175 --style raw
```

---

### PROMPT D: Cloth and Feather Detail Focus

```
intricate celestial boss character design, elegant deity figure wearing elaborate black and white ceremonial vestments, robes composed of flowing cloth layers with swan feathers seamlessly interwoven throughout the fabric, feathers transition from pure white near face to deep black at hem, pearlescent metallic thread embroidery creating fractal geometric patterns, six majestic swan wings behind figure detailed feather rendering, floating detached arms and legs held by soft rainbow iridescent flame energy, dramatic floor-length cloak with hood partially raised, swan crown floating above casting gentle light halo, eyes reflecting cosmic starlight serene yet powerful gaze, pale chromatic light rays emanating from chest and crown, highly refined pixel art with fine small pixels for maximum detail, perfectly symmetrical front view, monochrome palette pure white to deep black with subtle pearlescent rainbow shimmer on metals and flames only, imposing divine scale, professional game boss sprite, transparent background --v 6.1 --ar 4:5 --s 225 --style raw
```

---

### PROMPT E: Cosmic and Fractal Elements

```
monochromatic fractal deity boss sprite, celestial feminine entity embodying cosmic order and chaos, six vast swan wings with fractal feather patterns at edges dissolving into geometric shapes, body adorned in black white and gray robes with mathematical fractal motifs woven into cloth alongside delicate feathers, arms and legs severed and floating held by prismatic rainbow pearlescent flame tendrils forming sacred geometry connections, torso armor pieces pale iridescent metal with engraved cosmic symbols, billowing cloak interior reveals fractal void starscape, ornate swan crown hovering above emanating chromatic light pillars, face elegant and timeless with eyes containing swirling galaxies, overall presence is foreboding inevitable like a force of nature, front facing perfectly centered symmetrical, idle pose arms spread as if conducting the cosmos, extremely detailed fine pixel work no chunky pixels, monochromatic foundation with chromatic iridescent accents on flames metals and energy effects, massive intimidating scale, professional game boss character, transparent background --v 6.1 --ar 1:1 --s 250 --style raw
```

---

### PROMPT F: Intimidating Divine Presence

```
imposing divine boss entity concept art, massive feminine celestial being with overwhelming presence, six swan wings spanning enormous width each wing pristine white with gray shading and pearlescent gleam, elegant figure in stark black and white flowing vestments cloth texture with embedded feather details, detached limbs floating in fixed positions held by streams of rainbow chromatic ethereal fire, pale pearlescent metal armor accents on collar shoulders gauntlets greaves, sweeping dramatic cloak pooling at invisible floor with cosmic particle effects at edges, swan crown floating serenely above head like orbital halo glowing softly, expression calm and knowing but deeply intimidating ancient power behind serene facade, foreboding atmosphere of inevitable judgment, perfectly symmetrical frontal view idle standing pose arms outstretched, extremely fine detailed pixel rendering small precise pixels throughout, strictly monochromatic black white gray color scheme with only chromatic light from flames and subtle pearlescent sheen on metals, professional boss sprite quality game asset, transparent background for clean extraction --v 6.1 --ar 3:4 --s 200 --style raw
```

---

### PROMPT G: Maximum Detail Variant

```
ultra detailed celestial swan deity boss sprite maximum resolution, elegant cosmic feminine figure designed as final boss encounter, six individually detailed swan wings with thousands of rendered feathers in symmetric spread formation, complete black white and gray outfit consisting of layered flowing robes with real cloth physics appearance feathers interlaced into weave pattern visible at close inspection, detached floating arms held by intricate rainbow pearlescent flame ribbons with subtle chromatic color shifting, detached floating legs suspended by matching iridescent fire streams forming elegant curves, ornate pale pearlescent metal decorative armor with micro-detailed engravings, massive dramatic cloak with hood featuring cosmic void lining with tiny stars, elaborate swan crown hovering above head with multiple tiers radiating soft divine light, face rendered with careful attention serene beautiful but unmistakably powerful and ancient, frontal symmetrical centered composition idle pose arms welcoming yet threatening, absolute finest pixel detail work extremely small pixels for smooth curves and intricate patterns, monochromatic palette with chromatic accents strictly limited to flames and metallic iridescence, overwhelming sense of scale and divine presence, transparent background professional game asset --v 6.1 --ar 1:1 --s 300 --style raw
```

---

### PROMPT H: Terraria-Style Pixel Art Adaptation

```
terraria style boss sprite swan deity, elegant celestial feminine boss character in pixel art style matching terraria aesthetic, six large swan wings behind figure with clear pixel feather definition, body in black white gray flowing pixel robes with visible feather patterns in cloth, detached floating arms and legs held by animated rainbow pearlescent flame pixels, pale iridescent metal armor accents rendered in pixel art style, dramatic pixel cloak billowing, swan crown pixel ornament floating above, serene intimidating expression in pixel face detail, front view symmetrical idle animation frame, color palette strictly monochromatic with chromatic pixel accents on flames only, large imposing boss scale appropriate for terraria proportions, clean readable silhouette, professional quality matching calamity mod boss standards, transparent background for game implementation --v 6.1 --ar 1:1 --s 150 --style raw
```

---

### BOSS PROMPT PARAMETERS NOTES

**Recommended settings to try:**
- `--s 150-300` (higher stylize for more artistic interpretation)
- `--ar 1:1` for centered symmetric sprites
- `--ar 3:4` or `--ar 4:5` for taller imposing figures
- `--chaos 5-15` for slight variations while maintaining concept
- `--no background color saturation warm colors` to enforce monochrome

**Key terms that worked well:**
- "detached floating limbs held by [flame type]"
- "rainbow pearlescent" / "chromatic iridescent" for the flame colors
- "feathers interwoven into cloth/fabric"
- "swan crown floating/hovering above head"
- "monochromatic black white gray with chromatic accents"
- "perfectly symmetrical front view centered"
- "fine small pixels no chunky/blocky pixels"
- "foreboding divine presence"

**Post-processing for Terraria:**
1. Downscale to appropriate boss size (typically 150-400 pixels tall)
2. Ensure transparency is clean around all edges
3. May need to simplify some details for readability at game scale
4. Consider creating separate wing/limb sprites for animation
5. The detached limbs concept works perfectly for multi-part boss in tModLoader

---

---

# THEMED ITEM SPRITE PROMPTS

## Swan Lake Treasure Bag

*A pixel art treasure bag containing loot from the Swan Lake boss. Design features monochromatic elegance with pearlescent rainbow accents, swan feathers, and dual black/white swan symbolism.*

---

### PROMPT: Swan Lake Treasure Bag

```
pixel art treasure bag sprite terraria style, elegant monochromatic design themed around swan lake duality, bag shaped like ornate pouch or satchel with drawstring top, exterior fabric appears as interwoven black and white cloth in checkerboard or gradient pattern, delicate white and black swan feathers decorating the bag's surface some floating around it, pale pearlescent rainbow shimmer accents on metallic clasp and trim, subtle chromatic iridescent glow emanating from bag opening suggesting magical contents, bag features embroidered swan silhouettes one white swan one black swan in yin-yang arrangement, fractal geometric patterns etched into fabric borders, soft ethereal particles rising from bag opening white feathers and prismatic sparkles, professional terraria calamity mod treasure bag quality, clean readable pixel art style 64x64 base resolution, monochromatic palette pure white dark gray deep black with rainbow pearlescent accents only on metals and magical glow, elegant sophisticated design not chaotic, transparent background PNG, centered composition, item sprite game asset --v 6.1 --ar 1:1 --style raw --s 150
```

---

### PROMPT VARIANT: Swan Lake Treasure Bag (Ornate)

```
terraria pixel art treasure bag boss loot container, swan lake themed elegant design, bag shaped as regal drawstring pouch with cosmic void interior, exterior features monochromatic swan feather pattern alternating pure white and deep black feathers overlapping like scales, pearlescent metal clasp shaped like intertwined swan necks forming heart shape, rainbow iridescent shimmer on metallic elements, bag opening glows with soft chromatic light, floating swan down feathers and prismatic sparkles surrounding bag, fractal lace trim pattern in silver-white, duality symbolism black swan on left white swan on right embroidered on bag face, professional pixel art matching terraria calamity treasure bags, 64x64 pixel sprite clean sharp edges, color scheme strictly monochromatic with pale rainbow pearlescent shine limited to metals and magic effects, sophisticated clean design, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 175
```

---

---

## Eroica Celestial Sheet Music

*Pixel art sheet music themed around Eroica - heroic, triumphant, bold. Features crimson red, gold, and dramatic musical notation with heroic energy.*

---

### PROMPT: Eroica Celestial Sheet Music

```
pixel art celestial sheet music sprite terraria style, eroica heroic theme, ornate floating sheet music pages glowing with divine energy, musical staff lines rendered in brilliant gold with crimson red accents, bold dramatic quarter notes half notes and eighth notes arranged in triumphant heroic melody pattern, treble clef at beginning ornately decorated with conquest motifs, pages appear ancient parchment with golden illuminated borders, heroic radiating energy aura crimson and gold light rays, musical notation pulses with power, staff lines curve dynamically suggesting movement and drama, sharp symbols and crescendo markings emphasized, corner flourishes featuring crossed swords laurel wreaths victory symbols, professional terraria calamity mod item sprite quality, clean pixel art 64x64 base resolution, color palette dominated by crimson red bright gold brass and white with dark red shadows, dramatic lighting bright gold highlights, transparent background PNG, centered composition showing 2-3 overlapping sheet music pages, game asset item sprite --v 6.1 --ar 1:1 --style raw --s 175
```

---

### PROMPT VARIANT: Eroica Sheet Music (Triumphant)

```
terraria pixel art floating sheet music item sprite, heroic eroica symphony theme, 2-3 overlapping pages of musical notation suspended in air glowing with triumphant energy, staff lines in shining gold musical notes in bold crimson red, treble clef and bass clef elaborately decorated with heroic emblems, notes arranged to suggest powerful heroic fanfare melody, pages edged with ornate golden filigree borders featuring laurel leaves and victory stars, dramatic crimson aura radiating from pages, gold sparkles and red energy wisps flowing around music, crescendo and forte markings emphasized with glowing effects, parchment texture with battle-worn aged appearance but still majestic, professional pixel art matching terraria item quality, 64x64 pixel sprite resolution clean readable, color scheme crimson red bright gold metallic brass warm orange highlights deep red shadows, dramatic heroic presence, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
```

---

---

## Swan Lake Celestial Sheet Music

*Pixel art sheet music themed around Swan Lake - elegant, monochromatic, ethereal. Features black/white/silver with pearlescent rainbow accents and swan imagery.*

---

### PROMPT: Swan Lake Celestial Sheet Music

```
pixel art celestial sheet music sprite terraria style, swan lake elegant theme, graceful floating sheet music pages with ethereal glow, musical staff lines rendered in pristine silver-white, notes in elegant black and white alternating pattern, treble clef decorated with delicate swan silhouette, pages appear as fine parchment in monochromatic gradient from pure white to soft gray, swan feathers gently floating around and resting on pages, pale pearlescent rainbow shimmer on page edges, musical notation arranged in flowing elegant melody pattern, soft chromatic iridescent light emanating from pages, staff lines curve gracefully like swan necks, fractal geometric patterns in margins, corner embellishments featuring swan crowns and feather motifs, professional terraria calamity mod item sprite quality, clean pixel art 64x64 base resolution, strictly monochromatic color palette pure white silver gray deep black with subtle rainbow pearlescent glow accents only, serene elegant presence, transparent background PNG, centered composition showing 2-3 overlapping pages, game asset item sprite --v 6.1 --ar 1:1 --style raw --s 175
```

---

### PROMPT VARIANT: Swan Lake Sheet Music (Moonlit)

```
terraria pixel art floating sheet music item sprite, swan lake monochromatic theme, 2-3 overlapping pages of musical notation suspended ethereally glowing with soft moonlight, staff lines in luminous silver musical notes alternating pure white and deep black, treble clef adorned with elegant swan crown ornament, pages feature fractal lace borders and geometric patterns, delicate black and white swan feathers floating around pages some resting on staff lines, pale rainbow pearlescent shimmer exclusively on metallic highlights and page edges, musical notation suggests serene melancholic melody, cosmic starfield visible through semi-transparent pages, silvery moonbeam light rays, notes cast soft shadows creating depth, parchment has aged elegant appearance, professional pixel art matching terraria item quality, 64x64 pixel sprite resolution clean sharp, color scheme monochromatic pure white to deep black with silver tones and minimal chromatic pearlescent accents, elegant sophisticated duality theme black swan white swan symbolism, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
```

---

### BOSS TREASURE BAG & SHEET MUSIC NOTES

**Treasure Bag Design Philosophy:**
- Must be instantly recognizable as containing boss loot
- Should visually represent the boss's theme/aesthetic
- Typically 64x64 pixels for Terraria items
- Glowing/particle effects suggest magical contents
- Clean silhouette readable at small scale

**Sheet Music Design Philosophy:**
- Floating/suspended appearance suggests magical item
- Musical notation should be readable but stylized
- Staff lines and notes are core visual elements
- Theme colors should match the corresponding content tier
- Page curl/flutter suggests ethereal nature

**Implementation Tips:**
1. Treasure bags often have idle animation (gentle float, particles)
2. Sheet music could rotate slowly or pages could flutter
3. Both items work well with additive blending for glow effects
4. Consider creating multiple frames for subtle animation
5. Particle effects around items enhance magical feel
