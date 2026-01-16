# MagnumOpus Midjourney Prompts Master Library

> **Purpose**: Consolidated Midjourney prompts for generating white/grayscale particle assets that are tinted at runtime.

---

## ⚙️ UNIVERSAL RULES

### Asset Requirements
- **Pure white or grayscale** - No color information
- **Transparent background** - PNG with alpha channel
- **Small resolution** - 8x8 to 64x64 pixels (upscale for detail when generating)
- **Centered composition** - For proper rotation
- **Clean anti-aliased edges** - Blend well when scaled

### Universal Suffix (append to all prompts)
```
, white on pure black background, game asset, transparent PNG, 2D sprite, no text, no watermark --s 250 --style raw
```

### Post-Processing Steps
1. Remove background → pure transparent
2. Convert to grayscale → remove color cast
3. Adjust levels → full white (255) in brightest areas
4. Downscale to target resolution
5. Split sprite sheets into individual files
6. Test tinting in Terraria with color parameter

---

## 📦 CORE PARTICLE EFFECTS

### 1. Soft Glow (Most Versatile)
```
white particle effect sprite sheet, 8 variations in 2x4 grid layout, pure white soft circular glow particles on transparent background, each particle is a different softness gradient from hard-edged circle to extremely soft gaussian blur falloff, smooth anti-aliased edges, radial gradient from bright white center fading to transparent edges, professional game asset quality, pixel-perfect clean design, suitable for 2D game particle systems, 32x32 pixel sprites upscaled 8x for detail, PNG with alpha transparency, no color only luminosity values, each particle variation shows different falloff curves: linear, quadratic, exponential, inverse square, soft gaussian, hard rim, medium blend, feathered edge, flat orthographic view, centered composition, isolated on pure black background for easy extraction --v 6.1 --ar 2:1 --style raw --s 50
```

### 2. Energy Spark/Flare
```
white energy spark particle sprite sheet, 12 variations in 3x4 grid, pure white and grayscale only, transparent background, includes: sharp 4-pointed stars, 6-pointed stars, 8-pointed lens flares, soft diamond sparkles, elongated streak sparks, round soft glows with bright cores, small pinpoint highlights, medium soft orbs, large diffuse glows, asymmetric organic sparks, electric arc fragments, plasma wisps, all with smooth anti-aliased edges, radial symmetry where appropriate, professional 2D game particle assets, 32x32 pixel base resolution upscaled, bright white cores with soft falloff to transparent, game-ready sprite sheet format, black background for extraction --v 6.1 --ar 3:2 --style raw --s 75
```

### 3. Smoke/Cloud/Vapor
```
white smoke cloud particle sprite sheet, 16 variations in 4x4 grid layout, pure white and grayscale smoke puffs on transparent background, organic natural cloud shapes with wispy edges, includes: small tight smoke puffs, large billowing clouds, thin wispy tendrils, dense fog patches, dissipating vapor trails, cotton-like soft clouds, sharp edged stylized smoke, rounded cumulus shapes, stretched motion blur smoke, spiral smoke wisps, layered depth clouds, ethereal mist patches, each particle has soft semi-transparent edges that blend naturally, 64x64 pixel sprites upscaled, professional game asset quality, varied opacity gradients, no hard edges only soft blended boundaries, centered composition for rotation, black background --v 6.1 --ar 1:1 --style raw --s 100
```

### 4. Geometric Magic Symbols
```
white magic symbol particle sprite sheet, 20 variations in 5x4 grid, pure white geometric shapes on transparent background, includes: simple circles, double circles, triple concentric rings, pentagrams, hexagrams, octagons, runic circles, sacred geometry patterns, spiral symbols, crescent moons, star shapes of varying points, diamond rhombus shapes, cross patterns, celtic knot fragments, mandala segments, arcane sigils, alchemical symbols simplified, each symbol has clean sharp edges with subtle soft glow aura, professional vector quality, 32x32 pixel base upscaled, varies line thickness thin medium bold, black background --v 6.1 --ar 5:4 --style raw --s 50
```

### 5. Impact/Explosion Burst
```
white explosion burst particle sprite sheet, 12 variations in 4x3 grid, pure white and grayscale impact effects on transparent background, includes: radial starburst explosions, circular shockwave rings, expanding impact circles, debris scatter patterns, directional cone blasts, omnidirectional burst rays, soft bloom explosions, hard-edged flash bursts, layered ring explosions, asymmetric organic explosions, speed line impacts, energy nova effects, each with bright white center fading outward, clean anti-aliased edges, 64x64 pixel base resolution upscaled, centered composition, black background for extraction --v 6.1 --ar 4:3 --style raw --s 75
```

### 6. Ring/Halo/Aura
```
white ring halo particle sprite sheet, 12 variations in 4x3 grid, pure white and grayscale circular ring effects on transparent background, includes: thin sharp rings, thick soft rings, double concentric rings, triple nested rings, broken/dashed rings, glowing aura circles, soft halo effects, ring with inner glow, ring with outer glow, fading gradient rings, pulsing ring effect frames, expanding shockwave rings, each ring centered with transparent center hole, smooth anti-aliased curves, 64x64 pixel sprites upscaled, varying ring thickness from hairline to bold, soft falloff on edges, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### 7. Trail/Streak/Motion
```
white motion trail particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale streak effects on transparent background, includes: tapered comet tails, soft gradient trails, sharp speed lines, curved arc trails, spiral motion paths, elongated stretched orbs, directional blur streaks, fading ghost trails, ribbon-like flowing trails, particle stream paths, energy beam segments, lightning bolt fragments, afterimage echoes, motion smear effects, acceleration trails, deceleration fade trails, natural falloff from bright leading edge to transparent trailing edge, 64x16 pixel elongated sprites upscaled, horizontally oriented for easy rotation, black background --v 6.1 --ar 2:1 --style raw --s 75
```

---

## 🎵 MUSICAL PARTICLES

### Musical Notes
```
white musical notation particle sprite sheet, 24 variations in 6x4 grid, pure white and grayscale musical symbols on transparent background, includes: quarter notes, eighth notes, sixteenth notes, half notes, whole notes, treble clef, bass clef, sharp symbols, flat symbols, natural symbols, rest symbols quarter eighth whole, beamed note pairs, beamed note triplets, musical staff fragments, crescendo decrescendo marks, fermata, accent marks, staccato dots, tied notes, chord clusters, arpeggiated notes, grace notes, each symbol clean sharp vector quality with subtle soft glow aura, 32x32 pixel base upscaled, centered composition, black background --v 6.1 --ar 3:2 --style raw --s 50
```

### Piano Key Impact
```
white piano key impact sprite sheet, 16 variations in 4x4 grid, pure white and grayscale keyboard-inspired effects on transparent background, includes: single key press ripple, octave span wave burst, chord cluster multi-key impact, ascending scale staircase trail, descending scale falling notes, glissando sliding blur trail, key hammer strike impact, string resonance vibration lines, damper pedal sustain glow, soft pedal muted halo, grand piano soundboard wave, upright piano vertical burst, ivory key smooth flash, ebony key sharp flash, broken chord scattered impact, rolled chord spiral effect, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

### Sound Wave Patterns
```
white musical sound wave particle sprite sheet, 20 variations in 5x4 grid, pure white and grayscale audio visualization on transparent background, includes: sine wave smooth oscillation, aggressive sawtooth wave pattern, stacked harmonic overtone waves, musical staff with flowing notes, bass clef emanating sound rings, treble clef radiating energy, piano key ripple wave, violin bow stroke trail, crescendo building wave intensity, decrescendo fading wave, staccato sharp burst pulses, legato smooth connected waves, vibrato oscillating shimmer, resonance sympathetic wave echo, dissonance chaotic wave interference, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 5:4 --style raw --s 75
```

---

## 🪶 ORGANIC PARTICLES

### Feathers/Petals/Natural
```
white organic particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale natural shapes on transparent background, includes: simple feather shapes, detailed feather with barbs, cherry blossom petals 5-petal, rose petals curved, maple leaf shapes, simple leaves, floating seeds, dandelion fluff, snowflake crystals, water droplets, flower buds, grass blade fragments, vine tendrils, organic curved wisps, butterfly wing fragments, scale/shell fragments, natural organic curves and soft edges, 32x32 pixel sprites upscaled, varied orientations for natural scatter, black background --v 6.1 --ar 1:1 --style raw --s 100
```

### Swan Feathers (Swan Lake Theme)
```
white swan feather sprite sheet, 12 variations in 4x3 grid, pure white and grayscale elegant feathers on transparent background, includes: long primary feather graceful, short fluffy down feather, curved flight feather, feather tip fragment, feather mid-section, feather base quill visible, feather floating pose, feather falling angle, feather with subtle motion blur, pristine clean feather, slightly ruffled feather, feather dissolving into particles, each feather has elegant swan-like quality, soft barbs visible, graceful curves, suitable for swan lake ethereal effects, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

### Sakura Petals (Eroica Theme)
```
white sakura petal sprite sheet, 12 variations in 4x3 grid, pure white and grayscale cherry blossom petals on transparent background, includes: single petal various angles, petal pairs, petal cluster, petal floating horizontally, petal falling vertically, petal spinning, petal with subtle crease, petal edge curl, pristine fresh petal, slightly wilted petal, petal fragment, petal with motion trail, each petal has delicate cherry blossom shape, organic curves, light translucent quality, suitable for heroic eroica effects, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

---

## ⚔️ WEAPON EFFECTS

### Sword Smear/Trail
```
white sword smear slash trail sprite sheet, 12 variations in 4x3 grid, pure white and grayscale weapon trails on transparent background, includes: simple arc slash curve, full circle spin slash, diagonal slash from top right, diagonal slash from top left, horizontal slash left to right, vertical slash downward, figure-8 double slash, spiral slash trail, heavy impact slash with debris, light fast slash thin, charged slash thick with energy, interrupted slash broken, each slash shows motion through thickness variation, bright at leading edge fading to trail, dynamic flowing curves, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### Anime Slash Lines
```
white anime slash effect sprite sheet, 16 variations in 4x4 grid, pure white and grayscale speed line impacts on transparent background, includes: single slash line diagonal, crossed X slash, triple parallel slash, radial burst slash lines, curved arc slash, lightning bolt slash, crescent moon slash, star pattern multi-slash, focused point slash, scattered chaos slash, building momentum lines, impact point radial, speed line tunnel, motion blur streak, afterimage echo slash, finisher grand slash, each effect has dynamic anime-style energy, sharp clean lines with soft glow edges, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## 🔥 THEME: LA CAMPANELLA (Fire/Bell)

### Bell Chime Effects
```
white bell chime effect sprite sheet, 12 variations in 4x3 grid, pure white and grayscale bell-inspired effects on transparent background, includes: bell shape silhouette, bell clapper impact burst, bell resonance ring expanding, bell vibration lines, bell cross-section with sound waves, bell tower emanating energy, cracked bell with energy escaping, bell rim glow ring, harmonic bell overtone rings multiple, muted bell soft glow, struck bell sharp burst, bell fade echo trail, each effect captures musical bell quality, suitable for La Campanella fire bell theme, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### Infernal Flame
```
white flame fire sprite sheet, 16 variations in 4x4 grid, pure white and grayscale fire effects on transparent background, includes: small flame flicker, large billowing flame, sharp aggressive flame, soft gentle flame, fire tendril wisp, flame burst explosion, flame trail streak, flame spiral tornado, ember particles scattered, fire lick tongue shape, inferno intense flame, dying ember flame, flame with smoke wisps, flame ring circle, dual flame pair, chaotic wild fire, each flame has organic flowing movement implied, suitable for infernal fire effects, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 100
```

---

## 🌙 THEME: MOONLIGHT SONATA (Lunar/Ethereal)

### Lunar Effects
```
white lunar effect sprite sheet, 12 variations in 4x3 grid, pure white and grayscale moon-inspired effects on transparent background, includes: crescent moon shape, full moon with crater hints, moon phase sequence, lunar halo ring, moonbeam ray shaft, moonlight sparkle, moon behind cloud, eclipse partial shadow, lunar glow soft bloom, moon fragment shard, tidal wave moon influence, lunar dust particles, each effect has ethereal mystical lunar quality, soft glows and gentle gradients, suitable for Moonlight Sonata theme, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### Mist and Fog
```
white ethereal mist sprite sheet, 12 variations in 4x3 grid, pure white and grayscale fog effects on transparent background, includes: thin wispy mist strand, thick fog bank, swirling mist vortex, mist tendril reaching, fog patch hovering, mist dissipating edges, layered mist depth, rolling mist wave, mist around silhouette, ghostly mist face-like, mist coalescing forming, mist barrier wall, each mist has supernatural ethereal quality, very soft edges, variable transparency, suitable for mystical moon effects, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

---

## 👁️ THEME: ENIGMA VARIATIONS (Mystery/Arcane)

### Question Mark Particles
```
white question mark particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale mysterious question marks on transparent background, includes: simple clean question mark, stylized ornate question mark with serifs, question mark dissolving into particles, question mark made of swirling smoke, inverted question mark, question mark with glowing dot, double question mark intertwined, question mark emerging from void, question mark shattering into fragments, question mark with eye as the dot, question mark with spiral tail, question mark dripping like liquid, ghostly transparent question mark, bold thick question mark, thin elegant calligraphic question mark, question mark with arcane symbols, each symbol clean with soft glow aura, 32x32 pixel base upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

### Mysterious Eye Symbols
```
white mysterious eye symbol sprite sheet, 12 variations in 4x3 grid, pure white and grayscale arcane eye designs on transparent background, includes: single all-seeing eye simple, eye within triangle pyramid, eye with radiating rays, closed eye with lashes, eye made of swirling void, eye with spiral iris, multiple overlapping eyes, eye dissolving into mist, eye with question mark pupil, third eye forehead symbol, crying eye with tear drops, eye emerging from darkness, each eye has mystical otherworldly quality, subtle glow around edges, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### Arcane Glyphs
```
white arcane glyph sprite sheet, 16 variations in 4x4 grid, pure white and grayscale mysterious symbols on transparent background, includes: unknown language character, runic inscription fragment, alchemical circle simplified, occult symbol abstracted, geometric impossible pattern, recursive fractal glyph, cipher encrypted symbol, sacred geometry fragment, dimensional portal glyph, binding seal circle, warding symbol protective, curse mark ominous, prophecy inscription, ancient artifact marking, forbidden knowledge symbol, paradox symbol impossible, each glyph indecipherable yet meaningful, arcane mysterious quality, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

### Void Swirl Effects
```
white void swirl vortex sprite sheet, 12 variations in 4x3 grid, pure white and grayscale spiraling void effects on transparent background, includes: tight spiral inward, loose spiral outward, double helix spiral, fragmenting spiral dissolving, spiral with particle trail, clockwise and counter-clockwise variations, spiral emerging from point, spiral collapsing to point, spiral with eye at center, chaotic turbulent spiral, geometric angular spiral, organic flowing spiral, each vortex suggests pulling or mysterious energy, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

---

## 🌌 THEME: FATE (Cosmic/Endgame)

### Planets
```
white planet particle sprite sheet, 16 variations in 4x4 grid, pure white and grayscale celestial planet designs on transparent background, includes: small rocky planet, gas giant with bands, ringed planet saturn-like, planet with visible atmosphere halo, crescent planet partially lit, planet with moon orbiting, ice planet crystalline surface, volcanic planet with cracks, ocean planet reflective, planet with ring debris, planet silhouette eclipse, planet with aurora effect, planet crumbling breaking apart, planet emerging from void, dwarf planet simple sphere, binary planet pair orbiting, subtle glow aura around edges, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 100
```

### Stars and Supernovas
```
white star particle sprite sheet, 20 variations in 5x4 grid, pure white and grayscale stellar bodies on transparent background, includes: simple 4-point star clean, 6-point star elegant, 8-point star complex, starburst with rays, twinkling star with sparkle, dying star collapsing, supernova explosion burst, red giant bloated star, white dwarf compact star, binary stars orbiting pair, star with corona flares, neutron star intense core, pulsing variable star, star behind cosmic dust, newborn star in nebula, falling shooting star trail, star cluster group, star with lens flare effect, star going nova expanding, subtle distant star point, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 5:4 --style raw --s 75
```

### Constellation Patterns
```
white constellation pattern sprite sheet, 12 variations in 4x3 grid, pure white and grayscale connected star patterns on transparent background, includes: simple three-star triangle constellation, five-star pentagon arrangement, archer bow constellation fragment, scales of justice constellation, crown constellation arc, serpent snake constellation curve, warrior sword constellation, cup vessel constellation, wing constellation pair, cross constellation simple, spiral constellation arrangement, abstract geometric constellation, each constellation has bright star nodes connected by faint lines, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

### Galaxy Spirals
```
white galaxy spiral sprite sheet, 8 variations in 4x2 grid, pure white and grayscale spiral galaxy effects on transparent background, includes: classic spiral galaxy two arms, barred spiral galaxy, tight spiral with bright core, loose spiral with scattered stars, edge-on galaxy disk view, galaxy collision merging pair, elliptical galaxy blob, irregular galaxy chaotic, each galaxy has luminous core fading to spiral arms, tiny star points throughout, sense of immense scale and rotation, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 2:1 --style raw --s 100
```

### Black Hole/Singularity
```
white black hole singularity sprite sheet, 8 variations in 4x2 grid, pure white and grayscale event horizon effects on transparent background, includes: black hole with accretion disk, gravitational lensing distortion ring, singularity point with warped space, black hole consuming star streamer, wormhole tunnel entrance, void sphere with light bending, mini black hole unstable, black hole with jets emanating, dark center with bright ring of infalling material, extreme contrast between void and light, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 2:1 --style raw --s 100
```

### Reality Tears
```
white reality tear crack sprite sheet, 12 variations in 4x3 grid, pure white and grayscale dimensional rift effects on transparent background, includes: vertical reality crack jagged, horizontal dimensional tear, branching crack like shattered glass, reality fissure with void beyond, space-time tear with warping, crack with light bleeding through, crack closing healing, crack opening widening, spiral tear rotating, reality shatter point with radiating cracks, dimensional wound glowing edges, subtle crack beginning to form, sharp edges with glow at borders, 64x32 pixel sprites upscaled, horizontal orientation, black background --v 6.1 --ar 2:1 --style raw --s 75
```

---

## 💀 THEME: DIES IRAE (Hellfire/Chains)

### Hellfire Flames
```
white hellfire flame sprite sheet, 12 variations in 4x3 grid, pure white and grayscale infernal fire on transparent background, includes: aggressive demon flame, skull-shaped flame silhouette, flame with screaming souls implied, corrupt fire crackling, fire erupting from ground, hellfire spiral tornado, brimstone ember particles, flame with chain wisps, dark fire inverted glow, fire consuming effect, hellfire burst explosion, smoldering hellcoal, each flame has malevolent supernatural quality, more aggressive than normal fire, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

### Chain Effects
```
white chain link sprite sheet, 16 variations in 4x4 grid, pure white and grayscale metal chain effects on transparent background, includes: single chain link, chain segment 3-5 links, chain corner turning, chain breaking snapping, chain emerging from void, chain wrapped coiling, chain with hook end, chain with weight attached, ancient rusted chain texture, burning chain on fire, chain pulling taut, chain slack hanging, chain pile cluster, chain whip motion trail, chain binding circle, shattered chain fragments, each chain has heavy metal quality, suitable for binding infernal effects, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## ⚙️ THEME: CLAIR DE LUNE (Clockwork/Mechanical)

### Gears and Clockwork
```
white clockwork gear sprite sheet, 16 variations in 4x4 grid, pure white and grayscale mechanical parts on transparent background, includes: simple small gear, complex large gear many teeth, interlocking gear pair, gear train sequence, broken gear missing teeth, spinning gear motion blur, gear with decorative center, clock hand minute, clock hand hour, clock hand second thin, clock face numbers, pendulum swing, spring coil tension, escapement mechanism, balance wheel, cog wheel industrial, each piece has precision mechanical quality, suitable for clockwork time effects, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 50
```

### Time Effects
```
white time effect sprite sheet, 12 variations in 4x3 grid, pure white and grayscale temporal effects on transparent background, includes: clock face dissolving, time spiral inward, time wave expanding, frozen moment shatter, rewind arrow curve, fast forward streaks, hourglass sand falling, temporal echo afterimage, time stop ripple, aging effect progression, rejuvenation reverse glow, temporal rift crack, each effect suggests time manipulation, flowing or frozen quality, 64x64 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## 🌹 THEME: ODE TO JOY (Botanical/Vines)

### Rose Petals
```
white rose petal sprite sheet, 12 variations in 4x3 grid, pure white and grayscale flower petals on transparent background, includes: single rose petal curved, petal unfurling opening, petal falling gently, petal cluster grouping, petal with dewdrop, wilting petal curled edge, pristine fresh petal, petal with subtle veins, petal fragment torn, petal spiral arrangement, petal floating horizontal, petal wind-blown angle, each petal has romantic elegant quality, organic curves, soft edges, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 100
```

### Vines and Thorns
```
white vine thorn sprite sheet, 16 variations in 4x4 grid, pure white and grayscale botanical elements on transparent background, includes: simple vine tendril, vine with thorns, vine curl spiral, vine reaching growth, thorny branch segment, rose stem with thorns, vine leaf attached, vine tip growing, twisted vine gnarled, vine wrapping coiling, thorn cluster sharp, vine breaking through, withered vine dying, vine bloom spot, intertwined vines pair, vine root emerging, each vine has natural organic growth quality, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

---

## ✨ THEME: NACHTMUSIK (Night Sky/Stars)

### Night Sky Stars
```
white night sky star sprite sheet, 20 variations in 5x4 grid, pure white and grayscale nocturnal star effects on transparent background, includes: twinkling star animated frames, shooting star with trail, star cluster grouping, constellation fragment connected, bright star prominent, dim star subtle, star behind wispy cloud, starfield dense pattern, star with cross flare, star with soft glow, binary star pair, variable star pulsing frames, star emerging from darkness, star fading dimming, northern star bright point, star reflected in water shimmer, star through atmosphere shimmer, star with aurora nearby, falling star meteor, wishing star extra bright, 16x16 to 32x32 pixel sprites upscaled, black background --v 6.1 --ar 5:4 --style raw --s 75
```

---

## 🧊 THEME: WINTER (Ice/Frost)

### Ice Crystals
```
white ice crystal sprite sheet, 16 variations in 4x4 grid, pure white and grayscale frozen crystal effects on transparent background, includes: simple ice shard, complex snowflake 6-point, frost crystal branching, icicle hanging, ice fragment broken, frost pattern surface, ice spike aggressive, frozen droplet, ice formation cluster, cracking ice pattern, melting ice edge, pure ice clear, frosted ice opaque, ice with trapped bubble, crystalline structure geometric, ice dust particles, each ice has cold crystalline quality, sharp faceted edges, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 1:1 --style raw --s 75
```

### Snowflakes
```
white snowflake sprite sheet, 12 variations in 4x3 grid, pure white and grayscale snow crystal designs on transparent background, includes: classic 6-point snowflake, dendritic snowflake branching, plate snowflake simple, stellar snowflake elaborate, irregular snowflake natural, tiny snow particle, falling snowflake angle, snowflake with motion blur, melting snowflake edges soft, pristine snowflake sharp, clustered snowflakes overlapping, snowflake dissolving, each snowflake unique yet recognizable, delicate crystalline structure, 32x32 pixel sprites upscaled, black background --v 6.1 --ar 4:3 --style raw --s 75
```

---

## 🐉 BOSS DESIGN PROMPTS

### Worm/Serpent Boss Concepts
```
pixel art boss sprite sheet, serpent dragon worm creature, white silhouette on black background, includes: head segment with open mouth, head segment closed, body segment straight, body segment curved, body segment damaged, tail segment pointed, tail segment finned, transitional segments head-to-body, transitional segments body-to-tail, each segment designed to connect seamlessly, organic yet armored appearance, 64x64 pixel segments upscaled, suitable for modular worm boss construction, professional game boss quality, side view perspective --v 6.1 --ar 2:1 --style raw --s 100
```

### Guardian Boss Concepts
```
pixel art boss sprite sheet, ethereal guardian creature, white silhouette on black background, includes: humanoid torso upper body, floating lower body ethereal, arm raised attack pose, arm casting magic pose, head with crown or halo, wing pair spread, wing pair folded, shield defensive pose, weapon held various, damage state cracked, death dissolving particles, idle floating pose, charge attack windup, each piece modular for animation, 128x128 pixel sprites upscaled, professional boss design, front-facing perspective --v 6.1 --ar 1:1 --style raw --s 100
```

---

## 📦 ITEM DESIGN PROMPTS

### Treasure Bag
```
pixel art treasure bag sprite, single centered design, ornate fabric pouch with musical theme, tied with ribbon or string at top, subtle musical symbols embroidered, bulging with treasures implied, professional item icon quality, centered composition, white and grayscale on black background for color tinting, 32x32 pixel base upscaled 4x, suitable for terraria mod item icon --v 6.1 --ar 1:1 --style raw --s 50
```

### Sheet Music Item
```
pixel art sheet music sprite, single centered design, rolled or flat parchment with musical notation visible, elegant aged paper quality, staff lines with notes suggested, suitable for celestial upgrade material, professional item icon quality, centered composition, white and grayscale on black background, 32x32 pixel base upscaled 4x --v 6.1 --ar 1:1 --style raw --s 50
```

---

## 🏔️ PROFANED RAY BEAM SYSTEM

Three-part beam textures: Start (origin) + Mid (tiling body) + End (terminus)

### Lava Flow Style
**Start:**
```
holy fire beam origin point, intense bright core erupting outward, lava-like energy gathering, molten light source, radiant energy burst expanding rightward, circular glow transitioning to beam shape, white on pure black background, game asset, transparent PNG, 2D sprite --ar 1:1 --s 250 --style raw
```

**Mid (Seamless Tile):**
```
seamless tileable holy fire beam body, flowing lava energy stream, molten light current, undulating heat waves, organic flowing fire texture, horizontal energy flow, edges fade to transparent, seamless horizontal tile, white on pure black background, game asset, transparent PNG, 2D sprite --ar 4:1 --s 250 --style raw
```

**End:**
```
holy fire beam terminus, energy dissipating into particles, lava droplets scattering, molten light fading, beam tapering to point with dispersing embers, white on pure black background, game asset, transparent PNG, 2D sprite --ar 1:1 --s 250 --style raw
```

### Crystal Holy Style
**Start:**
```
crystalline holy beam origin, faceted light source, geometric energy gathering point, prismatic core radiating outward, sacred geometry burst, angular light formation expanding rightward, white on pure black background, game asset, transparent PNG, 2D sprite --ar 1:1 --s 250 --style raw
```

**Mid:**
```
seamless tileable crystalline beam body, faceted light stream, geometric energy flow, angular shard patterns, prismatic refraction lines, hard edges with soft glow, seamless horizontal tile, white on pure black background, game asset, transparent PNG, 2D sprite --ar 4:1 --s 250 --style raw
```

**End:**
```
crystalline beam terminus, shattering into geometric fragments, prismatic shards dispersing, faceted light dissipating, angular energy breaking apart, white on pure black background, game asset, transparent PNG, 2D sprite --ar 1:1 --s 250 --style raw
```

---

## 📐 RECOMMENDED RESOLUTIONS

| Asset Type | Base Resolution | Upscale for Generation |
|------------|-----------------|------------------------|
| Small particles | 8x8 to 16x16 | 4-8x |
| Standard particles | 32x32 | 4-8x |
| Large effects | 64x64 | 4x |
| Beam segments (Start/End) | 128x128 or 256x256 | 2x |
| Beam body (Mid) | 512x128 or 1024x256 | 1-2x |
| Boss segments | 64x64 to 128x128 | 2-4x |
| Item icons | 32x32 | 4x |

---

## 🎨 THEME COLOR PALETTES (For Reference)

Apply these colors when tinting white assets in code:

| Theme | Primary | Secondary | Accent |
|-------|---------|-----------|--------|
| La Campanella | (20,15,20) Black | (255,100,0) Orange | (218,165,32) Gold |
| Eroica | (139,0,0) Scarlet | (255,215,0) Gold | (255,150,180) Sakura |
| Swan Lake | (255,255,255) White | (20,20,30) Black | Rainbow via HSL |
| Moonlight Sonata | (75,0,130) Purple | (135,206,250) Blue | (220,220,235) Silver |
| Enigma Variations | (15,10,20) Black | (140,60,200) Purple | (50,220,100) Green |
| Fate | (15,5,20) Black | (180,50,100) Pink | (255,60,80) Red |
| Dies Irae | (139,0,0) Red | (30,10,10) Black | (255,140,0) Flame |
| Clair de Lune | (100,120,160) Mist | (240,240,250) Pearl | (180,200,230) Blue |
| Ode to Joy | (180,30,60) Rose | (60,120,60) Green | (255,200,180) Petal |
| Nachtmusik | (20,20,40) Night | (200,200,255) Star | (100,100,180) Sky |
| Winter | (200,230,255) Ice | (255,255,255) Snow | (150,200,255) Frost |
