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

## PROMPT 37: Chain Link & Iron Chain Effects (La Campanella Theme)

```
white iron chain link sprite sheet, 16 variations in 4x4 grid, pure white and grayscale metal chain effects on transparent background, includes: single heavy iron chain link isolated, chain segment three links curved, chain arc swooping in motion, broken chain link shattered burst, chain whip snapping crack effect, chain coil spiral wrapping, taut chain straight tension line, slack chain gentle drape curve, chain impact ground pound radial, chain connecting two anchor points, chain spinning centrifugal orbit, chain with attached bell weight swinging, chain links separating break apart, chain lightning crackling link pattern, chain being forged molten glow, anchor chain thick nautical links, each chain shows weight and metal texture with satisfying interconnected link detail, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for chain weapons kusarigama flails La Campanella themed projectiles, no color pure luminosity for tinting, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## PROMPT 38: Blazing Chain & Flaming Link Effects

```
white blazing chain on fire sprite sheet, 12 variations in 4x3 grid, pure white and grayscale burning chain effects on transparent background, includes: chain links wreathed in flames, fire trailing behind swinging chain, ember sparks flying from chain impact, chain glowing red-hot molten, flaming chain orbit circular trail, chain dragging fire streak ground, chain explosion links scattering with fire burst, ghostly spirit chain ethereal glow, chain emerging from flames summoning, inferno chain whirlwind vortex, chain binding target constricting with heat waves, blazing chain meteor attached flaming weight, each chain combines heavy metal weight with dynamic fire particle effects, flames should wrap around links realistically, professional game VFX quality, 64x64 pixel sprites upscaled, suitable for fire chain weapons La Campanella themed hell-themed attacks, no color pure luminosity for tinting, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## PROMPT 39: Chain Connection & Attachment Points

```
white chain attachment point sprite sheet, 9 variations in 3x3 grid, pure white and grayscale chain anchor effects on transparent background, includes: chain hook latching onto target, chain ring mount circular anchor, chain shackle cuff binding, chain link welding spark connection, chain pivot point swinging joint, chain clasp open and closed states, chain emerging from portal summoning ring, chain piercing through impact penetration, chain wrapping around securing grip, each attachment shows the mechanical connection point where chains meet objects or weapons with satisfying metallic detail, professional game VFX quality, 48x48 pixel sprites upscaled, suitable for chain weapon attachment points grappling hook effects constraint visuals, no color pure luminosity for tinting, black background --v 6.1 --ar 1:1 --style raw --s 50
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

---

# PHASE 9 POST-FATE BOSS TREASURE BAGS

*Treasure bags for the four post-Fate bosses: Nachtmusik, Dies Irae, Ode to Joy, and Clair de Lune.*

---

## Nachtmusik Treasure Bag

*A celestial treasure bag for Nachtmusik, Queen of Radiance. Features deep purple/violet with golden starlight accents, radiating ethereal celestial energy.*

### PROMPT: Nachtmusik Treasure Bag

```
pixel art treasure bag sprite terraria style, celestial queen of radiance theme, bag shaped like ornate cosmic pouch with drawstring top made of woven starlight, exterior fabric appears as deep purple violet gradient with embedded constellation patterns, golden star decorations scattered across surface, radiating ethereal white and gold celestial glow from opening, metallic clasp shaped like crescent moon with golden filigree, floating golden music notes and twinkling star particles surrounding bag, delicate embroidered crown of stars on bag face, cosmic nebula swirl patterns in violet and deep purple, pale stardust particles rising from bag, bag rim decorated with tiny golden bells, soft ethereal moonlight illumination, professional terraria calamity mod treasure bag quality, clean pixel art 64x64 base resolution, color palette deep purple midnight violet golden yellow warm white starlight, transparent background PNG, centered composition, item sprite game asset --v 6.1 --ar 1:1 --style raw --s 175
```

### PROMPT VARIANT: Nachtmusik Treasure Bag (Radiant)

```
terraria pixel art treasure bag boss loot container, nachtmusik queen of radiance themed, regal drawstring pouch with celestial void interior visible at opening, exterior features midnight purple fabric with woven golden thread forming constellation patterns and music staff lines, golden metal trim and clasp decorated with tiny dangling star charms, radiant white-gold light emanating from bag opening suggesting divine treasures, floating quarter notes and treble clefs in golden sparkles around bag, embroidered celestial queen silhouette on bag face, violet aurora wisps trailing from bag, ornate crown motif at drawstring top, stardust and moonbeam particles, professional pixel art matching terraria calamity treasure bags, 64x64 pixel sprite clean edges, color scheme deep purple violet indigo with rich gold and pure white accents, elegant celestial presence, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
```

---

## Dies Irae Treasure Bag

*A dark and ominous treasure bag for the Dies Irae boss. Features deep blood red, black, and smoldering ember accents, evoking the Day of Wrath.*

### PROMPT: Dies Irae Treasure Bag

```
pixel art treasure bag sprite terraria style, dies irae day of wrath theme, bag shaped like ancient cursed satchel with heavy iron chains, exterior fabric appears as charred black leather with cracked ember lines glowing blood red, skull motif clasp with red gem eyes, smoldering dark flames licking at bag edges, floating ember particles and ash wisps surrounding bag, ominous crimson glow emanating from opening, gothic cross and judgment scale symbols embroidered in dark red, heavy chains wrapped around bag base trailing into darkness, cracked molten fractures revealing hellfire within, apocalyptic runes etched into leather, brimstone smoke rising, professional terraria calamity mod treasure bag quality, clean pixel art 64x64 base resolution, color palette deep black charcoal blood red dark crimson orange ember accents, dark foreboding presence, transparent background PNG, centered composition, item sprite game asset --v 6.1 --ar 1:1 --style raw --s 175
```

### PROMPT VARIANT: Dies Irae Treasure Bag (Infernal)

```
terraria pixel art treasure bag boss loot container, dies irae judgment day themed, ominous bag shaped as ancient iron-bound satchel exuding dread, exterior features cracked obsidian leather with molten ember veins pulsing, heavy rusted chains and iron bands wrapped around bag, skull-shaped clasp with glowing red eye sockets, dark crimson hellfire visible through bag opening, floating embers ash particles and brimstone smoke, apocalyptic symbols swords scales trumpets embossed in burnt orange, charred edges suggesting fire damage, gothic metal spikes and studs, ominous blood-red aura radiating outward, crumbling ash falling from bag, professional pixel art matching terraria calamity treasure bags, 64x64 pixel sprite dark and foreboding, color scheme pitch black charcoal deep blood red bright ember orange white-hot accents, menacing infernal presence, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
```

---

## Ode to Joy Treasure Bag

*A radiant and triumphant treasure bag for the Ode to Joy boss. Features brilliant gold, pure white, and celebratory divine light, evoking universal joy and brotherhood.*

### PROMPT: Ode to Joy Treasure Bag

```
pixel art treasure bag sprite terraria style, ode to joy triumphant celebration theme, bag shaped like ornate golden chalice-pouch overflowing with light, exterior fabric appears as pristine white silk with golden embroidered laurel wreaths, brilliant divine golden light bursting from opening, metallic gold clasp shaped like angel wings, floating golden music notes and joyful sparkle particles, radiant sunburst pattern embroidered on bag face, celebratory ribbon streamers flowing from top, choir of tiny golden bells decorating rim, olympian victory motifs and triumphant symbols, holy divine glow surrounding entire bag, pure white feathers and golden stars floating around, professional terraria calamity mod treasure bag quality, clean pixel art 64x64 base resolution, color palette pure white brilliant gold warm ivory divine yellow, joyful triumphant radiant presence, transparent background PNG, centered composition, item sprite game asset --v 6.1 --ar 1:1 --style raw --s 175
```

### PROMPT VARIANT: Ode to Joy Treasure Bag (Divine)

```
terraria pixel art treasure bag boss loot container, ode to joy universal brotherhood themed, celestial bag shaped as divine chalice-pouch radiating pure joy, exterior features pristine white fabric with intricate gold thread laurel leaf patterns and ninth symphony motifs, golden metallic trim glowing with inner divine light, brilliant white-gold radiance streaming from bag opening suggesting heavenly treasures, floating golden treble clefs eighth notes and celebration sparkles, embroidered multi-pointed star of unity on bag face, victory laurel crown decorating drawstring top, ethereal choir light rays, heavenly feathers and stardust, golden ribbon banner accents, professional pixel art matching terraria calamity treasure bags, 64x64 pixel sprite clean and bright, color scheme pure brilliant white rich gold warm yellow ivory divine glow, triumphant joyful celestial presence, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
```

---

## Clair de Lune Treasure Bag

*A serene and ethereal treasure bag for the Clair de Lune boss. Features soft moonlit blues, silver, and gentle moonbeam glow, evoking peaceful nocturnal beauty.*

### PROMPT: Clair de Lune Treasure Bag

```
pixel art treasure bag sprite terraria style, clair de lune moonlight theme, bag shaped like elegant velvet pouch bathed in soft moonlight, exterior fabric appears as deep midnight blue with silver thread moonbeam patterns, crescent moon silver clasp with pearl inlay, gentle pale blue-white moonlight emanating from opening, floating silver dust motes and soft moonbeam rays surrounding bag, embroidered full moon reflecting on water imagery, delicate ripple wave patterns in silver, tiny silver stars decorating rim, nocturnal flower motifs lilies and moonflowers, soft dreamy mist wisps trailing from bag, serene peaceful glow, professional terraria calamity mod treasure bag quality, clean pixel art 64x64 base resolution, color palette deep blue midnight navy soft silver pale moonlight white gentle cyan, peaceful serene dreamy presence, transparent background PNG, centered composition, item sprite game asset --v 6.1 --ar 1:1 --style raw --s 175
```

### PROMPT VARIANT: Clair de Lune Treasure Bag (Ethereal)

```
terraria pixel art treasure bag boss loot container, clair de lune peaceful moonlight themed, dreamy bag shaped as silk pouch floating in gentle moonbeam, exterior features deep midnight blue velvet with silver thread piano key patterns and moonlit ripple effects, ornate silver crescent moon clasp with dangling star charm, soft pale blue-white ethereal glow from bag opening suggesting moonlit treasures, floating silver particles soft moonbeam wisps and gentle starlight, embroidered reflection of moon on tranquil water on bag face, delicate lace trim in silver-white, nocturnal flowers and sleeping nightingale motifs, peaceful serene dreamy atmosphere, professional pixel art matching terraria calamity treasure bags, 64x64 pixel sprite soft and elegant, color scheme midnight blue navy silver pale blue moonlight white gentle lavender hints, tranquil ethereal nocturnal presence, transparent background for game implementation --v 6.1 --ar 1:1 --style raw --s 200
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
---

---

# LA CAMPANELLA UNIQUE WEAPON PROMPTS

## Design Philosophy

La Campanella (Liszt) features a bell-focused, virtuosic aesthetic with:
- **Base Metal**: Sleek black steel/iron
- **Accent Metal**: Orange copper/bronze
- **Fire Effects**: Yellow and red flames with ember particles
- **Highlights**: White/silver/ivory accents
- **Signature Element**: Dangling bells on chains integrated throughout
- **Musical Elements**: Music notes, staff lines, treble clefs, piano keys, clockwork mechanisms

These prompts go beyond standard swords and guns to create anime-inspired and exotic weapon types.

---

## PROMPT LC-1: Twin Scissor Blades (Kill la Kill style)

```
Concept art for a side-view idle pixel art sprite of a massive twin scissor blade weapon themed around La Campanella bells and music, crafted from sleek black and orange metal with white silver accents, created by music itself in the style of Terraria, rotated 45 degrees clockwise with blades oriented toward top-right corner, the scissor handles form ornate bell shapes that ring when weapon opens, bell pendants and chains dangling from the pivot mechanism swaying with motion, blazing chains wreathed in fire orbiting the weapon, heavy iron chains connecting the blade tips trailing flames, ignited in red and yellow flames with ember particles dancing between blades, musical staff lines engraved along blade edges, gears and clockwork mechanisms visible at joint, treble clef shaped handle guards, glowing orange cracks along black metal surfaces, highly detailed steampunk musical engravings, full weapon visible in frame --ar 16:9 --v 7.0
```

---

## PROMPT LC-2: Chainsaw Halberd (Bloodborne/God Eater style)

```
Concept art for a side-view idle pixel art sprite of an elegant chainsaw-halberd hybrid weapon themed around La Campanella orchestral bells, crafted from sleek black steel with orange copper detailing and white porcelain accents, created by music itself in the style of Terraria, rotated 45 degrees clockwise pointing toward top-right, the chainsaw teeth are shaped like tiny bells that chime as they spin, ornate bell shapes integrated into the halberd head, blazing chains wrapped around the shaft wreathed in fire, chains of dangling bells attached to the weapon body, heavy iron chain links orbiting the blade trailing flames, ignited in intense yellow and red flames with musical ember particles, the chain blade glows orange-hot with fire, piano key patterns along the handle grip, clockwork gears visible in mechanism housing, swirling music notes and staff lines orbiting the weapon, professional pixel art game asset --ar 16:9 --v 7.0
```

---

## PROMPT LC-3: Whip-Sword / Urumi (Snake Sword)

```
Concept art for a side-view idle pixel art sprite of a segmented whip-sword weapon themed around La Campanella bells, sleek black and orange metal segments connected by glowing chains, blazing chain links wreathed in fire between each blade segment, each segment has a small bell attached that creates melody when swinging, white silver engravings on each blade segment, rotated 45 degrees clockwise in Terraria style, the blade unfurls in an elegant S-curve shape, dangling bell ornaments hanging from each joint on flaming chains, heavy iron chains orbiting the weapon trailing fire and embers, ignited in red and yellow flames with ember trails following the blade curves, musical notation etched into each segment, treble clef pommel, ornate steampunk handle with visible gears, music notes floating around the weapon --ar 16:9 --v 7.0
```

---

## PROMPT LC-4: Gunlance (Monster Hunter style)

```
Concept art for a side-view idle pixel art sprite of an ornate gunlance weapon themed around La Campanella bells and orchestral music, massive sleek black barrel with orange copper heat vents and white ivory accents, created by music itself in the style of Terraria, rotated 45 degrees clockwise with lance tip toward top-right, the barrel is shaped like an enormous bell that fires explosive musical notes, multiple smaller bells dangling from chains on the weapon body, ignited in red yellow flames erupting from vents, shell magazine decorated with piano key pattern, clockwork firing mechanism with visible gears, musical staff lines running along the lance blade, ember particles and music notes swirling around weapon --ar 16:9 --v 7.0
```

---

## PROMPT LC-5: Giant Tuning Fork Blade

```
Concept art for a side-view idle pixel art sprite of a massive tuning fork transformed into a greatsword themed around La Campanella, the prongs form the double-edged blade in sleek black metal with orange resonance lines and white crystal accents, created by music itself in the style of Terraria, rotated 45 degrees clockwise, vibration waves visible emanating from the blade, ornate bells dangling from the fork's base swaying in resonance, ignited in yellow and red flames at the blade tips, the weapon hums with visible sound waves, clockwork tuning mechanisms in the handle, musical notation spiraling around the weapon, ember particles dancing to the vibration frequency --ar 16:9 --v 7.0
```

---

## PROMPT LC-6: Bell-Hammer Warhammer

```
Concept art for a side-view idle pixel art sprite of an enormous bell-shaped warhammer themed around La Campanella, the hammerhead IS a giant ornate bell in sleek black and orange metal with white silver filigree, created by music itself in the style of Terraria, rotated 45 degrees clockwise, smaller bells dangling from blazing chains attached to the hammer head, heavy iron chains wreathed in fire orbiting the weapon, chains connecting the clapper to the hammer shaft, clapper visible inside the main bell glowing hot, ignited in red and yellow flames exploding outward from impact point, handle wrapped in piano wire pattern with music notation, clockwork mechanisms and gears embedded in shaft, treble clef counterweight at pommel end with chains dangling, musical shockwave rings emanating from bell, highly detailed orchestral engravings --ar 16:9 --v 7.0
```

---

## PROMPT LC-7: Music Box Cannon (Steampunk Artillery)

```
Concept art for a side-view idle pixel art sprite of an ornate music box cannon weapon themed around La Campanella, sleek black metal housing with orange copper pipes and white porcelain keys, created by music itself in the style of Terraria, rotated 45 degrees clockwise, the barrel emerges from an elaborate music box mechanism with visible cylinder and pins, multiple bells arranged like organ pipes on top, dangling bell ornaments on chains, ignited in red and yellow flames shooting from barrel, clockwork gears and springs visible through cutaway panels, piano keys along the trigger mechanism, musical notes launching as projectiles, ember particles and staff lines swirling --ar 16:9 --v 7.0
```

---

## PROMPT LC-8: Chakram Rings (Xena style)

```
Concept art for a side-view idle pixel art sprite of ornate dual chakram throwing rings themed around La Campanella bells, circular blade rings in sleek black and orange metal with white silver cutting edges, created by music itself in the style of Terraria, the rings shaped like ornate bells viewed from above with bladed outer edges, small bells dangling from the inner grip area creating melody when thrown, ignited in red and yellow flames trailing as halos, visible gears and clockwork in the grip mechanism, musical notation engraved around the circumference, treble clef patterns in the blade serrations, twin weapons shown overlapping --ar 16:9 --v 7.0
```

---

## PROMPT LC-9: Kusarigama with Bell-Weight

```
Concept art for a side-view idle pixel art sprite of an elegant kusarigama chain-sickle themed around La Campanella bells, sleek black sickle blade with orange copper engravings and white crystal edge, created by music itself in the style of Terraria, rotated 45 degrees clockwise, the weighted end IS an ornate ringing bell in black and orange metal, chain links shaped like miniature bells chiming as they swing, blazing chains wreathed in fire connecting sickle to bell, additional flaming chains orbiting the weapon trailing embers, heavy iron chain links glowing orange-hot, ignited in red and yellow flames along blade and chain, clockwork mechanisms in sickle handle, musical staff lines spiraling around the chain, dangling bell ornaments on secondary chains, ember particles following the chain's arc --ar 16:9 --v 7.0
```

---

## PROMPT LC-10: Giant Metronome Lance

```
Concept art for a side-view idle pixel art sprite of an enormous metronome transformed into a jousting lance themed around La Campanella, sleek black and orange metal with white ivory accents and visible pendulum mechanism, created by music itself in the style of Terraria, rotated 45 degrees clockwise with point toward top-right, the metronome's pendulum swings within a glass housing at the lance head, ornate bells dangling from the tempo markings, ignited in red and yellow flames pulsing in rhythm, clockwork gears and springs visible, musical notation etched along shaft matching tempo markings, conductor's baton integrated into design --ar 16:9 --v 7.0
```

---

## PROMPT LC-11: Pipe Organ Gatling Gun

```
Concept art for a side-view idle pixel art sprite of a rotating pipe organ gatling gun themed around La Campanella, multiple sleek black and orange organ pipes arranged in rotating barrel formation, created by music itself in the style of Terraria, rotated 45 degrees clockwise, each pipe is a different size creating different tones when fired, ornate bells dangling from the weapon frame, ignited in red and yellow flames erupting from pipe ends, white ivory keys along the grip and trigger mechanism, visible clockwork rotation mechanism with gears, musical notes exploding outward as projectiles, steam and ember particles, bellows mechanism visible on side --ar 16:9 --v 7.0
```

---

## PROMPT LC-12: Conductor's Baton Rapier

```
Concept art for a side-view idle pixel art sprite of an elegant conductor's baton transformed into a rapier themed around La Campanella, sleek black blade with orange copper fuller and white silver guard, created by music itself in the style of Terraria, rotated 45 degrees clockwise with tip toward top-right, the handle IS an ornate conductor's baton with cork grip, bell-shaped guard with dangling bell ornaments, ignited in subtle yellow and red flames along the blade edge, musical staff lines trailing from blade tip as you swing, clockwork metronome in pommel, visible gears in guard mechanism, music notes orbiting the weapon, elegant virtuoso aesthetic --ar 16:9 --v 7.0
```

---

## PROMPT LC-13: Grand Piano Greataxe

```
Concept art for a side-view idle pixel art sprite of a grand piano lid transformed into a massive greataxe themed around La Campanella, sleek black lacquer axe head with orange copper edge and white ivory inlays shaped like piano keys, created by music itself in the style of Terraria, rotated 45 degrees clockwise, piano strings visible inside the axe head that resonate on impact, ornate bells dangling from blazing chains along the shaft, heavy iron chains wreathed in fire orbiting the weapon head, flaming chain links connecting bells to the axe blade, ignited in red and yellow flames with musical ember particles, the cutting edge follows the curve of a piano lid, clockwork hammers visible inside, musical notation engraved on blade surface, golden filigree matching grand piano legs on handle --ar 16:9 --v 7.0
```

---

## PROMPT LC-14: Carillon Tower Staff

```
Concept art for a side-view idle pixel art sprite of a miniature carillon bell tower transformed into a mage staff themed around La Campanella, sleek black metal tower structure with orange copper bells at multiple levels and white silver architectural details, created by music itself in the style of Terraria, rotated 45 degrees clockwise, multiple bells of different sizes arranged in tower formation at staff head, all bells dangling and ringing with motion, ignited in yellow and red flames emanating from bell mouths, clockwork mechanism visible rotating the bells, musical waves radiating outward, gothic spire design with music notation engravings, floating ember particles --ar 16:9 --v 7.0
```

---

## PROMPT LC-15: Violin Bow Glaive

```
Concept art for a side-view idle pixel art sprite of an enormous violin bow transformed into a glaive polearm themed around La Campanella, sleek black shaft with orange copper fittings and white horsehair blade edge, created by music itself in the style of Terraria, rotated 45 degrees clockwise with tip toward top-right, the bow hair forms a razor-sharp curved blade that sings when swung, frog mechanism at base with dangling bell ornaments, ignited in red and yellow flames along the hair-blade, rosin particles floating as embers, clockwork tension adjustment in shaft, musical vibration waves visible around blade, ornate scroll at blade tip --ar 16:9 --v 7.0
```

---

## LA CAMPANELLA PROMPT MODIFIERS

Add these phrases to any prompt for variations:

**Style Modifiers:**
- `asymmetric brutal design` - more aggressive/intimidating
- `art nouveau flowing curves` - elegant swooping organic lines
- `brutalist industrial aesthetic` - chunky powerful mechanical
- `crystalline energy formations` - magical glowing variants
- `baroque ornamental excess` - maximum decoration

**Chain Modifiers (SIGNATURE ELEMENT):**
- `blazing chains wreathed in fire orbiting the weapon` - flaming orbital chains
- `heavy iron chains attached trailing flames` - weighted burning chains
- `chain links glowing orange-hot molten` - heat-emphasized chains
- `chains connecting bells swaying with motion` - musical chain integration
- `multiple chains spiraling around the weapon` - chain abundance
- `broken chain links scattering as embers` - destruction effects
- `chains emerging from flames` - summoning/hellfire aesthetic

**Element Additions:**
- `pipe organ integration` - more orchestral pipes
- `conductor's baton motif` - elegant authority symbols
- `sheet music ribbons flowing` - dynamic paper trails
- `metronome pendulum element` - rhythmic motion
- `piano hammer mechanisms` - percussive details

**Animation Suggestions:**
- `bells swaying with implied motion`
- `flames flickering dynamically`
- `gears rotating visible`
- `music notes spiraling outward`
- `pendulum mid-swing`
- `chains orbiting with centrifugal motion`
- `chain links rattling with kinetic energy`

---

## LA CAMPANELLA COLOR REFERENCE

```
PRIMARY PALETTE:
- Black Metal:     #1a1a1a, #2d2d2d, #0d0d0d
- Orange Copper:   #cd7f32, #b87333, #da8a67
- Orange Glow:     #ff6600, #ff8c00, #ffa500

FIRE PALETTE:
- Yellow Fire:     #ffdd00, #ffcc00, #fff200
- Red Fire:        #ff4500, #ff6347, #dc143c
- Ember Orange:    #ff7f00, #ff8800, #ffa000

ACCENT PALETTE:
- White Silver:    #c0c0c0, #d3d3d3, #ffffff
- Ivory:           #fffff0, #faebd7, #f5f5dc
- Cream:           #fffdd0, #faf0e6, #fff8dc
```

---

# CELESTIAL MUSICAL SEEDS - Mechanical & Voltaic/Fire Essence

## Design Philosophy

These seeds represent the primordial origin of all melodies in the universe - cosmic artifacts that contain the essence of creation through sound. They combine:
- **Mechanical clockwork precision** - gears, cogs, intricate machinery
- **Voltaic/electrical energy** - lightning, plasma, crackling arcs
- **Fire essence** - flames, embers, heat distortion
- **Musical notation** - staffs, notes, clefs, crescendos woven into design
- **Chromatic + Black/White duality** - rainbow prismatic effects alongside stark monochrome

---

## PROMPT SEED-1: Celestial Seed of Mechanical Harmony (Primary - Chromatic)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical seed of the universe, cosmic seed pod containing all melodies of creation, intricate mechanical clockwork visible through crystalline shell showing hundreds of tiny rotating gears and cogs in brass and silver, musical notation circuits etched into every surface with staffs clefs notes forming circuit-board patterns, chromatic prismatic rainbow energy pulsing through the mechanical veins, the seed pulses with voltaic lightning arcs of electric blue and purple crackling between gear teeth, flames of orange and red emanate from core exhaust vents like a cosmic engine, surrounded by atmospheric glow halo of shifting rainbow light, ornate baroque filigree decorations frame the mechanical internals, floating musical note particles orbit the seed, each gear has tiny musical symbols engraved, the seed appears to be humming with contained symphonic power, highly detailed every pixel meaningful, Terraria modding style with enhanced detail, transparent background, centered composition, the seed of the universe of melodies incarnate --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SEED-2: Celestial Seed of Voltaic Thunder (Electrical Dominant - Chromatic)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical thunder seed, cosmic seed of electrical symphonies, the shell is made of interlocking black iron and white silver metallic segments forming a pod shape, massive voltaic energy arcs of electric blue cyan and purple lightning constantly discharge between shell segments, inside visible through cracks are golden brass clockwork mechanisms spinning at impossible speeds, tesla coil structures rise from the seed surface crackling with contained plasma, musical treble and bass clefs are formed BY the lightning arcs themselves as they discharge, chromatic rainbow energy ripples through the electrical field creating prismatic interference patterns, tiny mechanical hammers inside strike tuning forks creating visible sound wave rings, atmospheric corona glow surrounds entire seed pulsing with the beat of cosmic music, ember sparks scatter from electrical contacts, extremely detailed mechanical internals each gear connected by lightning chains, the essence of thunderous orchestral crescendos made physical, pixel art Terraria style maximum detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SEED-3: Celestial Seed of Infernal Requiem (Fire Dominant - Chromatic)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical fire seed, cosmic seed containing the flames of all passion in music, outer shell of black obsidian and white porcelain cracked to reveal interior inferno, inside roars a furnace of orange red and yellow flames that form shapes of musical instruments violins harps pianos, clockwork bellows mechanisms pump rhythmically feeding the flames, gears of heat-resistant black iron rotate through the fire their teeth dripping molten brass, musical notation appears written IN the flames themselves sheet music burning eternally, chromatic rainbow fire at the hottest core where all colors of flame exist simultaneously, heat distortion waves ripple outward carrying visible sound vibrations, ember particles float upward transforming into musical notes as they cool, forge hammers inside beat against anvils creating sparks in rhythm, decorative skull motifs with musical instrument features, phoenix feather details curl around the shell, the requiem of creation's fire made manifest, atmospheric smoke and heat glow surround, Terraria pixel art style hyper-detailed, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SEED-4: Celestial Seed of Monochrome Symphony (Black & White Dominant)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical seed in pure black and white, cosmic seed of duality where shadow and light create music, the shell alternates between jet black void segments and brilliant white radiant segments in perfect balance, inside the transparent crystalline windows show clockwork mechanisms in silver and gunmetal, some gears spin clockwise others counter-clockwise creating harmonic resonance, piano key patterns form the seed's surface black and white keys in spiral arrangements, monochrome lightning arcs of white electricity against black void spaces, grayscale flames of silver and ash burn at the poles, musical notation in inverted colors black notes on white sections white notes on black, the duality creates interference patterns that shimmer with subtle hints of chromatic iridescence at the borders, yin-yang inspired design with musical clefs, stark dramatic contrast maximum black maximum white minimal gray, atmospheric halo of alternating black and white energy rings, Terraria pixel art style extremely detailed, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SEED-5: Celestial Seed of Prismatic Genesis (Maximum Rainbow)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical genesis seed, the original seed from which all cosmic music was born, shell is transparent crystal allowing full view of impossible complexity within, every visible surface covered in chromatic prismatic rainbow energy that shifts and flows, mechanical clockwork internals include hundreds of gears each a different color of the spectrum rotating in perfect synchronization, voltaic arcs of rainbow lightning connect gear systems crackling with all colors simultaneously, flames at the core burn in impossible prismatic fire cycling through every hue, musical notation floats within the crystal inscribed in light itself, the seed contains miniature galaxies of musical energy spiral arms made of sheet music and sound waves, atmospheric glow creates aurora borealis effects surrounding the seed, lens flare prismatic artifacts emanate from brightest points, sound itself is visible as colorful rippling waves, black and white accents provide contrast at key structural points, the alpha and omega of melodic creation pixel perfect detail, Terraria mod sprite style, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SEED-6: Celestial Seed of Silent Void (Dark Variant - Black & White)

```
Concept art for an extremely large and detailed pixel art sprite of a celestial musical void seed, cosmic seed of the silence between notes, primarily black with white accent details creating negative space art, the shell is matte black absorbing light with white silver filigree tracing musical patterns, inside visible through small windows are ghost-white clockwork mechanisms that move without sound, pale white voltaic energy flickers weakly like dying stars, cold white flames burn without heat at the core, inverted musical notation white symbols on absolute black, the seed represents the rests and pauses that give music meaning, atmospheric void halo of darkness with faint white particle ring, mechanical elements appear skeletal and minimal, decorative elements include moon phases and star maps, subtle iridescent sheen hints at hidden chromatic potential waiting to be awakened, silence made tangible, the pause before the crescendo, Terraria pixel art style high detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## CELESTIAL SEED MODIFIER PHRASES

Add these to customize any seed prompt:

**Mechanical Modifiers:**
- `with exposed orrery planetary gear systems`
- `music box cylinder with pins playing eternally`
- `clockwork heart beating at 120 BPM visible through chest cavity`
- `steam vents releasing musical note-shaped vapor`
- `pendulum escapement mechanism marking cosmic time`

**Voltaic Modifiers:**
- `jacob's ladder electrical arcs climbing the surface`
- `tesla coil crown crackling with potential`
- `capacitor banks storing musical energy`
- `plasma conduits running like veins`
- `electromagnetic field lines visible as aurora`

**Fire Modifiers:**
- `volcanic core with magma gears`
- `phoenix flame rebirth cycle visible`
- `forge bellows breathing life into flames`
- `ember constellations forming musical patterns`
- `heat mirage distortion field surrounding`

**Musical Integration:**
- `every gear tooth is a musical note`
- `the rotation speed determines pitch`
- `harmonic resonance creates visible standing waves`
- `the seed hums at concert A 440Hz`
- `crescendo and decrescendo markings pulse with energy`

**Atmospheric Effects:**
- `god rays emanating from core`
- `particle field of floating quarter notes`
- `rippling sound wave halos`
- `gravitational lensing of light around seed`
- `cosmic dust clouds of powdered sheet music`

---

## CELESTIAL SEED COLOR PALETTE

```
CHROMATIC RAINBOW PALETTE:
- Red Energy:      #ff0000, #ff3333, #cc0000
- Orange Energy:   #ff6600, #ff9933, #cc5500
- Yellow Energy:   #ffff00, #ffff66, #cccc00
- Green Energy:    #00ff00, #66ff66, #00cc00
- Cyan Energy:     #00ffff, #66ffff, #00cccc
- Blue Energy:     #0066ff, #3399ff, #0044cc
- Purple Energy:   #9900ff, #cc66ff, #7700cc

MONOCHROME PALETTE:
- Pure Black:      #000000, #0a0a0a, #111111
- Dark Gray:       #333333, #444444, #555555
- Silver:          #c0c0c0, #aaaaaa, #999999
- Pure White:      #ffffff, #f5f5f5, #eeeeee

MECHANICAL METALS:
- Brass:           #b5a642, #d4af37, #c5b358
- Copper:          #b87333, #cd7f32, #da8a67
- Iron:            #434343, #52514f, #3d3d3d
- Gunmetal:        #2c3539, #4a5459, #363d3f

VOLTAIC ELECTRICITY:
- Electric Blue:   #00bfff, #1e90ff, #00ffff
- Plasma Purple:   #9370db, #8a2be2, #9400d3
- Arc White:       #f0ffff, #e0ffff, #ffffff

FIRE ESSENCE:
- Core Yellow:     #fff44f, #ffdf00, #ffd700
- Mid Orange:      #ff8c00, #ff7f00, #ff6600
- Outer Red:       #ff4500, #ff0000, #dc143c
- Ember Glow:      #ff4500, #ff6347, #fa8072
```

---

---

# SEED OF UNIVERSAL MELODIES - Enhanced Music-Themed Prompts

## Design Philosophy

The Seed of Universal Melodies represents the primordial source of ALL music in existence. These enhanced prompts heavily emphasize musical themes:
- **Musical Notation as DNA** - staffs, notes, clefs form the genetic code of the seed
- **Instruments as Organs** - miniature harps, pianos, violins function as internal organs
- **Sound Waves as Lifeblood** - visible sonic vibrations pulse through the structure
- **Conductor's Will** - baton motifs, tempo markings, dynamic symbols guide the energy
- **Symphonic Machinery** - clockwork powered by harmonic resonance

---

## PROMPT SUM-1: Seed of Universal Melodies (Primary Design)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies, the primordial cosmic seed from which all music in existence was born, outer crystalline shell is shaped like a giant musical note with a glowing treble clef at its core, visible through transparent sections are miniature orchestral instruments made of golden clockwork - tiny violins with spinning bows, microscopic grand pianos with hammering keys, minuscule harps with vibrating strings all playing in eternal harmony, musical staff lines spiral around the seed like DNA double helix strands with quarter notes and eighth notes as the genetic code, the seed pulses with chromatic energy in rhythm like a cosmic heartbeat at 60 BPM, voltaic lightning arcs form bass clef and treble clef shapes as they discharge, flames at the core burn in the shape of crescendo and decrescendo symbols, conductor's baton mechanisms orchestrate the internal movements, visible sound wave rings emanate outward carrying actual musical notation that can be read, tempo marking "Eternally" engraved on shell, dynamic markings fff to ppp cycle through the energy levels, atmospheric glow creates aurora of floating musical notes quarter rests whole notes fermatas, highly detailed Terraria pixel art style, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SUM-2: Seed of Universal Melodies (Orchestral Variant)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies orchestral variant, cosmic seed containing a complete symphony orchestra frozen in eternal performance, the shell is shaped like a concert hall dome with crystalline acoustic panels, inside visible are impossibly detailed microscopic musicians - string section violins violas cellos basses with bows moving, brass section trumpets french horns trombones with visible air flow, woodwind section flutes clarinets oboes with keys dancing, percussion section timpani cymbals gongs with mallets striking, a tiny conductor on a podium raises a glowing baton controlling all movements, musical scores float around each section with their parts illuminated, sound waves from each instrument section create visible interference patterns of different colors, the seed resonates at concert pitch A440 with tuning fork vibrations visible, mechanical clockwork keeps perfect tempo with metronome pendulums, chromatic rainbow energy represents the full spectrum of musical expression from pianissimo whispers to fortissimo explosions, atmospheric particles are floating bow rosin dust and sheet music confetti, Terraria modding style extreme detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SUM-3: Seed of Universal Melodies (Piano Variant)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies piano variant, cosmic seed shaped like a crystalline grand piano viewed from above showing all 88 keys in spiral arrangement, the keys alternate between black obsidian and white ivory in the seed's shell pattern, visible through the top are golden piano strings vibrating in complex harmonic patterns creating visible standing waves, tiny hammers connected to clockwork mechanisms strike the strings in impossible rapid sequences playing all of music simultaneously, the soundboard interior glows with chromatic fire representing the resonance of pure musical emotion, damper mechanisms controlled by miniature pedal systems, music notation spirals up from the sound holes like smoke made of quarter notes and rests, voltaic energy arcs between strings creating electrical harmonics, the piano action mechanism is visible as incredibly detailed clockwork with each hammer a different color of the rainbow, sustain pedal engaged creates infinite reverb glow effect, atmospheric halo of floating piano key sprites and hammer felt particles, Terraria pixel art style maximum detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SUM-4: Seed of Universal Melodies (Violin Variant)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies violin variant, cosmic seed shaped like a violin f-hole viewed straight on with the entire instrument visible within, the outer shell curves elegantly like a Stradivarius body in crystalline amber material, visible inside are four cosmic strings each a different element - fire string burns with passion, water string flows with emotion, earth string resonates with foundation, air string sings with freedom, a ghostly bow made of pure light draws across all strings simultaneously creating visible sound waves in chromatic rainbow colors, the scroll at top contains a miniature galaxy of musical energy spinning, chin rest and tailpiece are mechanical clockwork with visible gears marking tempo, rosin dust particles float as golden ember sparks, the bridge transmits vibrations as visible lightning arcs to the soundpost inside, vibrato wobble effect visible as the entire seed gently oscillates, purfling decoration is made of continuous musical notation forming protective spell, atmospheric glow of flowing horsehair bow strands and floating pizzicato note bursts, Terraria modding style extreme detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SUM-5: Seed of Universal Melodies (Conductor Variant)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies conductor variant, cosmic seed shaped like an ornate conductor's baton floating vertically with its tip pointing up, the shaft is made of crystalline material showing internal clockwork metronome mechanisms in gold and silver, at the tip a brilliant star of pure white light represents the downbeat of creation itself, energy trails from previous conducting gestures remain visible as ghostly arcs showing patterns for 4/4 time 3/4 waltz 6/8 compound, dynamic markings float around the seed responding to gesture - ff when baton raises pp when it lowers, tempo markings orbit like moons - Allegro Adagio Andante Presto, the handle grip shows fingerprints of countless cosmic conductors who wielded it before, sheet music ribbons flow from the baton tip carrying the score of the universe itself, fermata symbols appear as halos when the baton holds still, sforzando explosions of chromatic energy burst from sudden movements, cue gestures point to different directions summoning instrument sounds, crescendo builds visible as rising tide of rainbow light, atmospheric particles are floating tempo and dynamic marking sprites, Terraria pixel art style maximum orchestral detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## PROMPT SUM-6: Seed of Universal Melodies (Sheet Music Variant)

```
Concept art for an extremely large and detailed pixel art sprite of the Seed of Universal Melodies sheet music variant, cosmic seed shaped like a rolled scroll of infinite musical notation partially unfurled, the paper material is golden parchment that glows from within with contained melodic energy, visible notation includes every musical symbol ever conceived - notes from whole to sixty-fourth, all clefs treble bass alto tenor, every time signature from 4/4 to 7/8 to 13/16, key signatures from seven flats to seven sharps, the notation is not static but actively playing - note heads light up in sequence as the eternal symphony progresses, bar lines are made of voltaic lightning creating rhythmic structure, accidentals sharps flats naturals float around adding chromatic complexity, repeat signs create visual loops of recursive melody, coda and segno signs glow as navigation beacons, dynamics from ppp to fff pulse with intensity, articulation marks staccato legato accent create visible attack patterns, tempo markings accelerando ritardando affect the scroll unfurling speed, the ink is chromatic rainbow that shifts with harmonic content, atmospheric particles are floating individual note heads and rests like musical snow, Terraria modding style extreme calligraphic detail, transparent background --ar 1:1 --v 7.0 --s 250
```

---

## SEED OF UNIVERSAL MELODIES COLOR PALETTE

```
MUSICAL NOTATION:
- Staff Lines:     #1a1a1a, #333333, #000000
- Note Heads:      #0d0d0d, #1a1a1a, #000000  
- Golden Ink:      #ffd700, #ffcc00, #daa520
- Silver Ink:      #c0c0c0, #d3d3d3, #a9a9a9

INSTRUMENT MATERIALS:
- Violin Amber:    #ffbf00, #cc9900, #996600
- Piano Ivory:     #fffff0, #faebd7, #f5f5dc
- Piano Ebony:     #0d0d0d, #1a1a1a, #2d2d2d
- Brass:           #b5a642, #d4af37, #c5b358
- Silver Metal:    #c0c0c0, #aaaaaa, #999999

ENERGY PALETTE:
- Treble Clef:     #ffd700, #ffcc00, #ffaa00
- Bass Clef:       #4169e1, #6495ed, #4682b4
- Sound Waves:     #00ffff, #00cccc, #009999
- Harmonic Glow:   #ff69b4, #ff1493, #db7093

CHROMATIC MUSICAL:
- C Note Red:      #ff0000
- D Note Orange:   #ff7f00
- E Note Yellow:   #ffff00
- F Note Green:    #00ff00
- G Note Cyan:     #00ffff
- A Note Blue:     #0000ff
- B Note Purple:   #8b00ff
```

---

---

# LA CAMPANELLA WEAPONS - BASE & CELESTIALLY ENHANCED VARIANTS

## Design Philosophy

Each weapon has TWO versions:
1. **BASE VERSION** - Standard La Campanella aesthetic: black steel, orange copper, bells, chains, flames
2. **CELESTIALLY ENHANCED VERSION** - Infused with Seed of Universal Melodies: adds musical notation, sound waves, chromatic rainbow energy, floating instruments, conductor motifs

The enhanced version should clearly show the weapon has been transformed by cosmic musical power.

---

## LC-BASE-1: Campanella Twin Scissors (Base)

```
Concept art for a side-view idle pixel art sprite of twin scissor blade weapon base version La Campanella theme, massive scissors crafted from sleek black steel with orange copper accents and white silver highlights, bell shapes form the scissor handles that ring when opened, iron chains connect the blade tips with small bells dangling, chains wreathed in orange and red flames with ember particles, the pivot mechanism has visible clockwork gears, metal appears forged and practical not yet infused with cosmic power, steampunk aesthetic with rivets and bolts visible, the weapon is impressive but clearly mortal craftsmanship, rotated 45 degrees clockwise in Terraria style, professional pixel art game asset, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-1: Campanella Twin Scissors (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of twin scissor blade weapon celestially enhanced La Campanella theme transformed by the Seed of Universal Melodies, massive scissors now radiant with divine musical power, the black steel transformed to brilliant white platinum metal with blazing golden cracks running through it pulsing with harmonic energy, musical notation engraved along both blades that GLOWS AND SHIFTS as if playing a melody in brilliant white and gold, the bell handles are now magnificent crystalline bells that ring with visible sound wave halos and brilliant flaring light bursts, ornate bells with impossibly detailed engravings of musical scenes cover the pivot mechanism each bell unique and breathtaking, tiny ghostly violins and harps orbit the scissor pivot point playing eternally surrounded by radiant white auras, flames have transformed to brilliant white-gold celestial fire with intense lens flare effects, a conductor's baton motif appears in the handle design wreathed in divine light, sheet music ribbons of pure light flow from the blade tips leaving afterimage trails, the pivot gear mechanism now shows a miniature orchestra inside playing in sync with blade movements all glowing white-gold, tempo markings float around weapon Prestissimo for attack speed in elegant calligraphy, the weapon radiates contained symphonic power with constant subtle light pulses, bells toll with visible shockwave rings on every movement, dramatic divine upgrade visible at a glance, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-2: Campanella Chainsaw Halberd (Base)

```
Concept art for a side-view idle pixel art sprite of chainsaw halberd weapon base version La Campanella theme, elegant hybrid weapon with chainsaw teeth shaped like tiny bells, crafted from black steel with orange copper detailing, chains wrapped around shaft with dangling bell ornaments, visible clockwork mechanism powers the chain blade, orange and red flames trail from the chainsaw teeth, piano key patterns on handle grip, the weapon is formidable mortal craftsmanship, steampunk bells and gears aesthetic, not yet infused with cosmic musical power, rotated 45 degrees clockwise Terraria style, professional pixel art, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-2: Campanella Chainsaw Halberd (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of chainsaw halberd weapon celestially enhanced by Seed of Universal Melodies, the chainsaw teeth are now crystalline bells that chime divine notes as they spin creating visible harmonic frequencies with brilliant white flares, black steel transformed to radiant white platinum metal with golden veins of pure musical energy pulsing in rhythm, the blade creates a continuous musical scale as it rotates with each bell-tooth a masterwork of intricate engravings depicting musical legends, magnificent ornate bells arranged along the halberd head each unique with impossibly detailed filigree and gemstone inlays, sheet music physically wraps around the shaft as decoration GLOWING with brilliant white-gold celestial light, tiny brass instruments orbit the halberd head surrounded by lens flare halos - trumpets french horns playing fanfares, flames transformed to brilliant white-gold divine fire with intense radiant flaring, the clockwork mechanism now powered by a miniature pipe organ bellows visible through crystal panels glowing from within, musical staff lines trail behind weapon movement as ribbons of pure light, conductor baton integrated into handle wreathed in divine aura, forte and fortissimo dynamic symbols explode on impact with blinding flash effects, bells ring with visible expanding shockwave halos, celestial transformation unmistakable, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-3: Campanella Whip-Sword (Base)

```
Concept art for a side-view idle pixel art sprite of segmented whip sword weapon base version La Campanella theme, snake sword with black and orange metal segments connected by chain links, each segment has a small bell attached, flames wreath the chain connections with ember trails, musical notation etched into segments, treble clef pommel, elegant S-curve shape, steampunk handle with visible gears, impressive mortal craftsmanship not yet celestially enhanced, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-3: Campanella Whip-Sword (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of segmented whip sword celestially enhanced by Seed of Universal Melodies, the blade segments are now radiant white platinum metal containing captured musical phrases visible inside each crystalline window, segments connected by pure solidified sound waves glowing brilliant white-gold, each segment features an ornate bell of unique design with breathtaking engravings of musical mythology ringing with visible circular sound wave emanations creating divine harmony, the S-curve shape leaves a trail of glowing musical notation in brilliant white light that persists in the air with lens flare afterimages, miniature string quartet orbits the blade surrounded by radiant halos - violins violas cellos playing, flames now brilliant white-gold celestial fire with intense flaring light bursts, each segment plays a different note of a scale when struck creating melody with visible light pulses, crescendo symbols appear as blinding energy builds along the blade length, the pommel is now a magnificent bell that serves as conducting sphere controlling the blade like an orchestra, the bells throughout the weapon are masterworks of impossible detail with microscopic musical scenes engraved, legato marking makes segments flow with trailing light staccato makes them snap with flash bursts, divine musical transformation complete, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-4: Campanella Bell Hammer (Base)

```
Concept art for a side-view idle pixel art sprite of enormous bell-shaped warhammer base version La Campanella theme, the hammerhead IS a giant ornate bell in black and orange metal with white filigree, smaller bells dangle from chains on the hammer head, visible clapper inside glows orange-hot, iron chains wreathed in flame orbit the weapon, red and yellow flames explode from impact point, piano wire wrapped handle, clockwork mechanisms in shaft, treble clef counterweight at pommel, impressive forged weapon not yet cosmic, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-4: Campanella Bell Hammer (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of bell warhammer celestially enhanced by Seed of Universal Melodies, the main bell head now a magnificent crystalline masterpiece of white platinum and gold containing a visible miniature carillon of dozens of ornate bells inside all ringing in divine harmony each bell a unique work of art with impossibly detailed engravings, the clapper is now a conductor's baton of pure light that strikes creating visible shockwave rings of musical notation with blinding flash effects, black metal transformed to radiant white platinum with golden energy cracks pulsing like heartbeat, smaller orbiting bells now sing in perfect harmony each an exquisite unique design with intricate mythological musical scenes engraved, streams of solidified sound waves flow around the weapon like liquid light, on impact the weapon creates a visible chord with overlapping brilliant white-gold rings expanding outward with lens flare bursts, timpani drum energy surrounds the hammer head with visible drum skin vibrations glowing from within, the handle has become a pipe organ pipe of white metal that resonates with brilliant light pulses on each swing, fortississimo fff marking appears on maximum power strikes as blinding explosion, the bell tolls with visible shockwave text "DONG" in stylized musical font radiating outward wreathed in divine light, the bells are breathtakingly detailed with gemstone inlays and golden filigree, divine symphonic transformation complete, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-5: Campanella Kusarigama (Base)

```
Concept art for a side-view idle pixel art sprite of kusarigama chain sickle base version La Campanella theme, black sickle blade with orange copper engravings, the weighted end IS an ornate bell, chain links shaped like miniature bells, chains wreathed in fire connecting sickle to bell weight, additional flaming chains orbit weapon, clockwork in handle, musical staff lines on the chain, ember particles following chain arc, impressive mortal weapon not yet enhanced, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-5: Campanella Kusarigama (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of kusarigama celestially enhanced by Seed of Universal Melodies, the sickle blade now radiant white platinum metal with crystalline edge containing a frozen violin bow stroke visible inside glowing with inner light, the bell weight transformed to the most magnificent ornate bell imaginable - a physical manifestation of a perfect A440 tuning note with impossibly detailed engravings depicting the birth of music itself with gemstone inlays and golden filigree, connected by solidified sound waves glowing brilliant white-gold creating a musical phrase when swung with trailing light ribbons, the blade edge is a continuous treble clef shape in radiant white metal, tiny woodwind instruments orbit the sickle surrounded by brilliant halos - flutes clarinets playing, brilliant white-gold celestial flames with intense lens flare effects replace orange fire, the kusarigama creates visible sound wave spirals of pure light when spun, tempo markings appear in elegant golden calligraphy showing attack speed Vivace Presto Prestissimo, the handle contains a miniature music box of white platinum that plays divine melodies when weapon moves, glissando effect visible as brilliant light trails, ornate bells along the weapon ring with visible expanding shockwave halos, every bell surface covered in microscopic musical mythology scenes, divine musical mastery achieved, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-6: Campanella Gunlance (Base)

```
Concept art for a side-view idle pixel art sprite of gunlance weapon base version La Campanella theme Monster Hunter style, massive black barrel shaped like bell with orange copper vents and white ivory accents, multiple smaller bells dangling from chains, red yellow flames erupt from vents, shell magazine with piano key pattern, clockwork firing mechanism, musical staff along lance blade, ember particles, impressive mortal artillery not yet celestial, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-6: Campanella Gunlance (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of gunlance celestially enhanced by Seed of Universal Melodies, the bell barrel is now a magnificent masterwork of white platinum shaped like the most ornate cathedral bell ever conceived with impossibly intricate engravings and gemstone inlays firing concentrated blasts of pure musical energy - visible as projectiles of brilliant white-gold light complete with tiny instruments inside each shot, the barrel interior visible through crystal panels shows a miniature concert hall glowing with divine light where an orchestra plays creating the ammunition, shell magazine transformed to white platinum containing solidified symphony movements each shell a different piece Allegro Andante Adagio, brilliant white-gold celestial flames with intense lens flare bursts, the lance blade edge is continuous musical notation in radiant gold that can be read as actual playable music, percussion instruments orbit the weapon surrounded by brilliant halos - snare drums cymbals triangle playing impact rhythms, each shot creates visible sound wave expansion with blinding flash effects and musical notation rippling outward in pure light, dynamic markings on ammunition selector in golden calligraphy - pp shot to fff explosive cannon, the firing mechanism is now a grand piano action of white platinum each trigger pull like striking a divine key, sforzando accent marks appear on critical hits as blinding explosions, magnificent ornate bells decorate the weapon each unique with mythological musical scenes engraved, divine artillery symphony achieved, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-7: Campanella Grand Piano Greataxe (Base)

```
Concept art for a side-view idle pixel art sprite of grand piano greataxe base version La Campanella theme, piano lid transformed to massive axe head in black lacquer with orange copper edge and white ivory key inlays, piano strings visible inside that resonate on impact, bells dangle from chains along shaft, flames wreath the weapon head, chain links connect bells to blade, clockwork hammers visible inside, musical notation engraved on blade, golden filigree on handle, impressive mortal weapon, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-7: Campanella Grand Piano Greataxe (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of grand piano greataxe celestially enhanced by Seed of Universal Melodies, the piano lid axe head now radiant white platinum crystalline revealing a complete miniature grand piano inside that plays itself with ghostly hands of pure light on the keys, each of the 88 keys visible as the axe edge serrations alternating brilliant white ivory and polished obsidian each glowing with inner light, the internal strings are now made of pure concentrated light that create visible harmonic vibrations on impact with blinding flash effects, the surface transformed to radiant white platinum with golden cracks pulsing with contained concerto power, sheet music physically manifests and flows from the blade like ribbons of pure divine light, tiny keyboard instruments orbit the axe surrounded by brilliant halos - celestas harpsichords glockenspiels playing, brilliant white-gold celestial flames with intense lens flare bursts, each swing plays a chord that appears as visible stacked notes in radiant white-gold with expanding shockwave rings, sustain pedal effect creates lingering brilliant light glow after attacks, magnificent ornate bells decorate the axe head each a unique masterwork with impossibly detailed musical mythology engravings, the handle is now a conductor podium of white platinum with baton grip wreathed in divine aura, Romantic era musical terminology floats around weapon Con fuoco Appassionato Brillante in golden calligraphy, the greatest piano piece made divine weapon, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-8: Campanella Pipe Organ Cannon (Base)

```
Concept art for a side-view idle pixel art sprite of pipe organ cannon weapon base version La Campanella theme, multiple black and orange organ pipes in gatling arrangement, white ivory keys on grip, bells dangle from frame, red yellow flames erupt from pipe ends, clockwork rotation mechanism with gears, bellows mechanism on side, musical notes as projectiles, ember particles, impressive mortal instrument weapon, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-8: Campanella Pipe Organ Cannon (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of pipe organ cannon celestially enhanced by Seed of Universal Melodies, the organ pipes now radiant white platinum crystalline containing visible compressed symphonies ready to fire glowing from within, each pipe fires different pitch projectiles of brilliant white-gold light that harmonize in flight creating chord patterns with trailing light ribbons, the bellows mechanism transformed to cosmic lung of white platinum breathing pure musical energy visible as radiant streams, visible inside through crystal panels is a miniature cathedral glowing with divine light with full pipe organ being played by a ghostly organist of pure light, the ivory keys now play themselves responding to combat needs surrounded by brilliant halos, brilliant white-gold celestial flames with intense lens flare effects, ammunition is now physical manifestations of famous musical pieces in pure concentrated light - fire a burst of Toccata and Fugue watch gothic organ notes spiral outward wreathed in divine radiance, the rotation mechanism powered by the music itself with bells ringing at each rotation, magnificent ornate bells arranged among the pipes each a breathtaking unique design with impossibly detailed engravings of musical legends, tremolo and vibrato effects visible on sustained fire as brilliant light pulses, registration stops allow switching between different instrument sounds - flute strings brass, the bells toll with visible expanding shockwave halos, the ultimate divine musical artillery transformation complete, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-9: Campanella Carillon Staff (Base)

```
Concept art for a side-view idle pixel art sprite of carillon bell tower staff base version La Campanella theme, miniature bell tower structure as staff head in black metal with orange copper bells at multiple levels and white silver architectural details, multiple bells of different sizes arranged in tower formation, bells dangle and ring with motion, yellow red flames from bell mouths, clockwork rotation mechanism, gothic spire design with musical engravings, floating ember particles, impressive mortal mage weapon, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-9: Campanella Carillon Staff (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of carillon staff celestially enhanced by Seed of Universal Melodies, the bell tower now radiant white platinum crystalline containing a visible miniature cosmic carillon with hundreds of magnificent ornate bells playing the music of the spheres each bell a unique masterwork of impossible beauty with mythological musical scenes engraved in microscopic detail, the tower structure transformed to white platinum with golden energy cracks pulsing with harmonic resonance, visible inside through crystal is a tiny carillonneur of pure light playing the bells with cosmic hands, sound wave rings emanate from each bell tier as brilliant white-gold expanding halos creating layered harmonic visual with lens flare effects, brilliant white-gold celestial flames flow up the tower like divine aurora, the gothic architecture now white platinum includes musical notation as structural elements - staff lines as flying buttresses note heads as gargoyles all glowing with inner light, sheet music physically spirals around the tower like ribbons of pure radiance, the bells play actual recognizable melodies on cast creating musical projectiles of concentrated light, campanology terms float around in elegant golden calligraphy Treble Tenor Bob Major Grandsire, the spire tip is now a conductor star of blinding brilliance that coordinates all bell rings, every bell surface covered in gemstone inlays and golden filigree of breathtaking craftsmanship, ultimate divine carillon achieved, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## LC-BASE-10: Campanella Conductor Rapier (Base)

```
Concept art for a side-view idle pixel art sprite of conductor baton rapier base version La Campanella theme, elegant baton transformed to rapier with black blade orange copper fuller and white silver guard, handle IS conductor baton with cork grip, bell-shaped guard with dangling bell ornaments, subtle flames along blade edge, musical staff trailing from blade tip, clockwork metronome in pommel, visible gears in guard, music notes orbiting, elegant virtuoso mortal weapon, rotated 45 degrees clockwise Terraria style, transparent background --ar 16:9 --v 7.0
```

## LC-CELESTIAL-10: Campanella Conductor Rapier (Celestially Enhanced)

```
Concept art for a side-view idle pixel art sprite of conductor rapier celestially enhanced by Seed of Universal Melodies, the blade now radiant white platinum crystalline containing frozen conducting gestures visible inside like motion capture of a master conductor glowing with inner divine light, the cork grip transformed to cosmic white material wreathed in brilliant aura that responds to wielder intent, the blade edge IS a continuous timeline of musical history from ancient chant to modern symphony readable along its length in golden notation, bell guard is now a magnificent ornate bell of impossible beauty with breathtaking engravings of musical mythology containing a miniature orchestra that follows every blade movement as if conducted, brilliant white-gold celestial energy flows along blade leaving trailing light ribbons, every thrust parry and riposte creates visible conducting patterns in the air as pure light - downbeat upbeat cue fermata with lens flare effects, the blade tip leaves musical notation of radiant gold in its wake that persists and can be read, tiny solo instruments orbit the rapier surrounded by brilliant halos responding to its commands - first violin concertmaster oboe horn, tempo of combat literally controlled by blade movements in elegant golden calligraphy Accelerando speeds up Ritardando slows down, dynamic range visible with flash bursts - small movements create pp attacks wide sweeps create ff strikes with blinding flare, the pommel is an exquisite bell metronome that beats with cosmic time keeping the rhythm of the universe itself ringing with visible shockwave halos, ultimate divine maestro weapon achieved, Terraria style maximum detail, transparent background --ar 16:9 --v 7.0 --s 200
```

---

## BASE VS CELESTIAL COMPARISON NOTES

**Visual Differences at a Glance:**

| Element | BASE Version | CELESTIALLY ENHANCED Version |
|---------|--------------|------------------------------|
| Main Metal | Solid black/orange | Radiant white platinum with golden cracks |
| Flames | Orange/Red/Yellow | Brilliant white-gold celestial fire |
| Bells | Metal ringing | Magnificent ornate bells with impossible detail |
| Connections | Iron chains with fire | Solidified sound waves of pure light |
| Decorations | Engraved notation | GLOWING animated notation in gold |
| Orbitals | Ember particles | Tiny instruments with brilliant halos |
| Energy | Fire/ember based | Divine light with lens flare effects |
| Aura | Flame glow | Sheet music ribbons of pure radiance |
| Impact FX | Fire explosion | Blinding flash with shockwave halos |
| Bell Details | Simple ornate | Mythological scenes, gemstone inlays, golden filigree |

**Key Upgrade Indicators:**
1. Black metal transforms to radiant white platinum
2. Brilliant white-gold celestial fire replaces orange flames
3. Orbiting miniature instruments with glowing halos appear
4. Musical notation becomes animated gold light
5. Sound waves visible as brilliant white-gold expanding rings
6. Sheet music ribbons flow as pure divine light
7. Conductor/orchestral motifs wreathed in divine aura
8. Musical terminology appears as elegant golden calligraphy
9. Internal mechanisms glow with inner divine light
10. Bells become masterworks with impossibly detailed engravings

**Bell Enhancement Details:**
- Each bell unique with mythological musical scenes engraved
- Microscopic detail showing musical legends and history
- Gemstone inlays and golden filigree throughout
- Visible shockwave halos when bells ring
- Bells glow from within with divine light

---

## CELESTIALLY ENHANCED COLOR PALETTE

```
WHITE PLATINUM METALS:
- Pure Platinum:   #e5e4e2, #d4d4d4, #c0c0c0
- Bright White:    #ffffff, #fafafa, #f5f5f5
- Luminous Silver: #e8e8e8, #dcdcdc, #d3d3d3
- Divine White:    #fffef0, #fffff0, #fefefe

GOLDEN DIVINE ACCENTS:
- Bright Gold:     #ffd700, #ffcc00, #f4c430
- Divine Gold:     #ffdf00, #ffe135, #ffc125
- Warm Gold:       #daa520, #cd9b1d, #b8860b
- Light Gold:      #fff8dc, #ffecb3, #ffe4b5

CELESTIAL FIRE:
- White Fire Core: #ffffff, #fffafa, #fff8f0
- White-Gold Blend:#fff5e1, #ffedcc, #ffe4b8
- Divine Flame:    #fffacd, #fff8dc, #ffefd5
- Radiant Glow:    #fff0db, #ffecd2, #ffe4c9

BELL DETAIL ACCENTS:
- Gemstone Ruby:   #e0115f, #9b111e, #722f37
- Gemstone Sapphire: #0f52ba, #0067a5, #0047ab
- Gemstone Emerald: #50c878, #046307, #287233
- Gemstone Amber:  #ffbf00, #ff8c00, #ff7700

FLARE & LENS EFFECTS:
- Core Flare:      #ffffff, #fffef8, #fffdf5
- Soft Halo:       #fffff0, #fefefa, #fdfdf8
- Bright Burst:    #ffffc0, #ffff80, #ffff40
- Divine Radiance: #fff8e7, #fff5d4, #fff2c1
```