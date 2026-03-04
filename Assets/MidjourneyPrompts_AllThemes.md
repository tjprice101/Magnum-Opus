# MagnumOpus — Complete Theme-Specific Asset Midjourney Prompts

> **Master document containing Midjourney prompts for every visual asset, with unique versions tailored to each of the 10 musical score themes.**

## How to Use This Document

1. Find the **theme** section you're generating assets for
2. Each asset has a ready-to-paste Midjourney prompt with `--ar` and `--style raw`
3. **All VFX textures are white/grayscale on solid black background** — shaders tint them at runtime
4. After generating, place the PNG in the correct folder per the copilot-instructions asset placement guide
5. File naming convention: `<ThemeName><AssetName>.png` (e.g., `LaCampanellaBraidedEnergyHelixBeam.png`)

## Technical Constraints (Apply to ALL prompts)

- **Color**: White and grayscale values only, on pure black (#000000) background
- **Format**: PNG, power-of-two dimensions
- **No baked color**: All color tinting happens in shaders at runtime
- **Additive blending**: Black = invisible. Only the white/bright parts show

---
---

# ═══════════════════════════════════════════
# THEME 1: LA CAMPANELLA — The Flaming Bell
# ═══════════════════════════════════════════

> **Musical Soul:** Liszt's virtuosic fire — ringing bells, cascading arpeggios, passionate intensity
> **Visual Motifs:** Bell silhouettes, bell curves, ringing vibration waves, chime clusters, resonating bronze, cascading arpeggios, tubular chime shapes, bell clapper strikes, carillon towers, concentric resonance rings, hand-bell outlines, wind chime strands
> **Emotional Core:** Passion, intensity, burning brilliance, virtuosic speed
> **Colors (applied at runtime):** Black smoke, orange flames, gold highlights

---

## La Campanella — Beam Textures

### LC Braided Energy Helix Beam
```
White intertwined double-helix beam segment on solid black background, two spiraling strands of resonant energy twisted around each other like bell-rope fibers, bright white glowing core with softer gray outer wisps, concentric vibration rings emanating from each strand, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Cracking Energy Fissure Beam
```
White cracking fissure beam segment on solid black background, horizontal energy beam with fracture lines radiating outward like a cracked church bell, jagged bright cracks forming mosaic pattern through the beam body, hot white core with gray stress fractures branching to edges, resonance cracks between fragments like a bell struck too hard, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Energy Motion Beam
```
White flowing energy beam segment on solid black background, horizontal stream of cascading energy droplets falling like rapid piano arpeggios, each droplet elongated and chime-shaped, flowing left to right in rhythmic cascading pattern, bright core drops with soft gray motion trails, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Energy Surge Beam
```
White surging energy beam on solid black background, powerful horizontal energy surge with bell-curve intensity peaks repeating along its length, each peak like a struck bell resonance, bright white crescendo peaks fading to gray troughs between pulses, heat shimmer distortion lines along edges, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Horizontal Beam Streak
```
White horizontal beam streak on solid black background, clean linear energy column with flickering bell-resonance edges dancing along both sides, the central beam razor-sharp and intensely bright, edge waves asymmetric and wild like rapidly struck chimes, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### LC Infernal Beam Ring
```
White ring-shaped beam cross-section on solid black background, circular beam rendered as a bell mouth seen from below, the rim vibrating with concentric resonance rings expanding outward, bell surface textured with gentle bronze patina dimples, bright white spots where the bell resonates strongest, darker gray dampened sections, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of rapid frequency oscillations like vibrating piano strings struck with tremendous force, high-frequency tight waves in the bright core expanding to lower-frequency loose waves at gray edges, standing wave nodes visible as bright concentration points, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Sound Wave Beam
```
White acoustic wave beam on solid black background, horizontal beam made of concentric sound wave arcs emanating from left side like a bell being struck, each arc thin and bright in the center fading to soft gray at extremities, waves compress tighter near the source and expand toward the right, vibration interference patterns where waves overlap, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Turbulent Plasma Core Segment
```
White turbulent energy core on solid black background, horizontal beam core with roiling bell-resonance turbulence, churning vibration waves with bright white hotspots surrounded by darker swirling overtone currents, small chime-burst eruptions along the surface where harmonic frequencies collide, intense and chaotic internal motion, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Thin Linear Glow
```
White thin linear glow on solid black background, extremely fine horizontal line of intense brightness like a single vibrating piano wire caught in light, razor-thin bright core with delicate soft bloom extending above and below, subtle standing wave amplitude modulation along the wire length, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

### LC Beam Lens Flare Explosion Impact
```
White radial burst on solid black background, explosive bell-strike impact flare with energy radiating outward in concentric rings like sound waves from a struck bell, bright white center with radiating streaks of decreasing intensity, ring-shaped resonance waves expanding outward, tiny chime fragments flying in spiral patterns, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Burst Fade Streaks Lens Flare
```
White radial streaked lens flare on solid black background, central bright point with long fading streaks radiating outward like resonance waves from a struck chime, streaks vary in length and intensity creating asymmetric bell-ring burst, soft gray glow envelope around the cluster, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Burst Heavy Streaks Lens Flare
```
White heavy radial burst on solid black background, massive bell-clapper impact flare with thick bold streaks radiating from center like resonance waves from a great bell's toll, each streak tapers from thick bright base to thin gray tip, uneven distribution suggesting powerful directional force, bright hot core with intense radial chime energy, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC 8-Point Starburst Flare (Projectile)
```
White 8-pointed starburst on solid black background, brilliant flare shaped like a ringing bell's acoustic radiation pattern, eight sharp points radiating symmetrically with bright tips, each point slightly chime-shaped with vibrating edges, narrow bright core with larger soft glow between the points, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## La Campanella — Glow and Bloom

### LC Glow Orb
```
White soft circular glow on solid black background, perfectly round orb of light with resonant bell-curve falloff, extremely bright pinpoint center rapidly falling off to medium gray then black, the falloff curve shaped like a bell curve with a sharp bright peak, subtle concentric chime-ring texture in the mid-range, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Lens Flare
```
White complex lens flare on solid black background, multi-element flare arrangement resembling a bell seen from above, central bright disc with a ring of smaller flare elements arranged in a circle around it, thin anamorphic streak cutting horizontally through center, each element has soft feathered edges, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Point Bloom
```
White pinpoint bloom on solid black background, tiny intensely bright center point with wide soft Gaussian bloom extending outward, the bloom envelope shaped like a bell tip — slightly taller than wide, elongated upward like a chime resonance, gentle vibration edge irregularities, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### LC Soft Glow
```
White soft glow on solid black background, large diffuse luminous cloud with gentle Gaussian falloff, slightly uneven edges suggesting bell-wave resonance or vibrating air, the glow center offset slightly upward like rising resonance, extremely smooth and atmospheric, game VFX bloom texture, 256x256px --ar 1:1 --style raw
```

### LC Soft Glow Bright Center
```
White bloom with bright center on solid black background, large soft glow with an extremely intense white hotspot in the center like a bell struck at its resonant frequency, the bright core takes up about 20% of the radius then falls off rapidly to a wide soft gray halo, game VFX bloom texture, 256x256px --ar 1:1 --style raw
```

### LC Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle concentric ring structure like acoustic resonance rings expanding from a bell strike, each ring slightly brighter than the smooth falloff between them, creating a gentle rippled bloom effect rather than smooth Gaussian, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Star Flare
```
White star-shaped flare on solid black background, four-pointed star flare with elongated vertical and horizontal spikes like resonating chime prongs, each spike has a bright base that tapers to a fine point, the horizontal spikes slightly longer than vertical suggesting bell-clapper strike direction, bright center core where all spikes meet, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## La Campanella — Impact Effects

### LC Harmonic Resonance Wave Impact
```
White resonance wave impact on solid black background, circular expanding shockwave made of multiple concentric bell-vibration rings, each ring thin and bright at its leading edge fading to gray behind, rings spaced at harmonic intervals — closer together near center, wider apart at edges, interference patterns where waves overlap create brighter nodes, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Crumbling Shatter Burst
```
White shatter burst on solid black background, explosive fragmentation pattern like a shattered bell, large angular bell-metal shards radiating outward from center, each shard bright at its inner edge and darker at outer edge, resonance cracks and fracture lines between shards, some smaller chime-fragment debris scattered between the major fragments, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Impact Ellipse
```
White impact ellipse on solid black background, horizontally elongated elliptical impact burst like a bell clapper striking the bell's inner wall, the ellipse brighter and thicker at center thinning toward the pointed ends, concentric resonance ring streaks along the top edge suggesting acoustic wave spray, game VFX impact texture, 128x64px --ar 2:1 --style raw
```

### LC Power Effect Ring
```
White power ring on solid black background, bold expanding ring of energy with chime-prong protrusions along the outer edge, the ring itself thick and bright, outer edge decorated with small upward-reaching bell-curve shapes evenly spaced, inner edge smooth, ring slightly thicker at cardinal points suggesting directional resonance force, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Slash Star Impact
```
White radial slash star on solid black background, six-pointed impact star with each point shaped like a curved chime prong, points alternate between long and short creating dynamic asymmetry, bright white center hub with streaks extending to each chime-point tip, overall shape suggests a spinning bell-resonance wheel, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC X-Shaped Impact Cross
```
White X-shaped cross impact on solid black background, two crossing diagonal slash marks forming an X, each slash line tapers from thick bright center to thin gray tips, the intersection point extremely bright with a small chime-burst explosion, edges of each slash have flickering resonance-wave irregularities, small bell-fragment sparks scattered around the intersection, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Beam Lens Flare Explosion Impact
```
White explosion impact on solid black background, massive radial detonation burst like a great bell being struck with overwhelming force, central blindingly bright core with thick radiating energy bands, concentric resonance waves visible as rings within the burst, outermost ring has chime-vibration tongues ringing outward, bell-fragment debris flying in curved trajectories, game VFX impact texture, 256x256px --ar 1:1 --style raw
```

---

## La Campanella — Projectiles

### LC 4-Point Star Shining Projectile
```
White four-pointed star projectile on solid black background, compact shining star with four sharp points, each point shaped like a bell chime prong — wider at base tapering to sharp tip, bright core where points meet with softer glow envelope, subtle resonance vibration rings around the star, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Bright Star Projectile 1
```
White bright star projectile on solid black background, intense ringing star shape with irregular chime-edge points, like a fragment of resonant bell-metal flying through air, bright white-hot core with ragged glowing edges, small trailing vibration wake hanging below, surrounded by concentric resonance ring, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Bright Star Projectile 2
```
White blazing projectile on solid black background, rapidly spinning chime-wheel projectile, four curved bell-curve arms spiraling from center creating pinwheel rotation, each arm wider at outer tip and narrow at center junction, overall shape suggests a spinning wind-chime array caught in motion, bright center with gradient gray arms, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Gyratory Orb
```
White spinning orb projectile on solid black background, spherical orb with visible internal rotation — swirling resonance patterns visible through a translucent bell-bronze shell, the surface has subtle bell-curve dimple texture, bright equatorial band where spin is fastest, slightly compressed poles, a small ring orbiting the equator like a bell's acoustic vibration ring, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Orbiting Energy Sphere
```
White energy sphere on solid black background, large central orb with three smaller orbiting chime wisps circling it, the main sphere has a resonant bell core with softer outer glow, each orbiting wisp is teardrop-shaped and trails a small gray resonance wake, orbiting wisps spaced at even 120-degree intervals, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### LC Pulsating Music Note Orb
```
White music-themed orb on solid black background, circular orb containing a visible musical note shape within — a quarter note silhouette ringing bright inside a sphere of bell-bronze energy, the note appears to be cast from resonant chime metal, sphere edges shimmer with vibration wave irregularity, small resonance ripples emanating from the orb surface, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## La Campanella — Slash Arcs

### LC Flaming Sword Arc Smear
```
White sword arc smear on solid black background, sweeping 120-degree crescent blade arc with aggressive bell-chime resonance edges, the arc is thick and bright at the leading edge thinning toward the trailing edge, vibration wave rings extend outward from the outer curve irregularly, inner curve smooth and precise like a bell's rim, hot spots brightest at the arc's midpoint, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Flaming Sword Arc Smear 2
```
White sword arc smear variant on solid black background, wider 150-degree sweeping arc with double-layered structure, outer layer thin and wispy with resonance wave filaments, inner layer thick bright concentrated energy core, the two layers separated by a narrow gap of darkness creating depth, bell-fragment sparks scattering from the leading edge, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Full Circle Sword Arc Slash
```
White full 360-degree sword arc on solid black background, complete circular slash ring with bell-toll intensity, ring varies in thickness — thickest and brightest at the 3 o'clock position (strike point) thinning toward the opposite side, resonance wave eruptions at the thickest point, bell-bronze texture along the ring body, small chime fragments ejecting outward from the strike point, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Sword Arc Smear
```
White clean sword arc on solid black background, precise 90-degree arc slash with bell-curve brightness profile — gradually brightening from start, peaking at center, and fading toward the end, the arc edge has subtle vibration wobble like a struck bell resonating, thin bright core with moderate soft glow, overall clean and sharp but with that characteristic ring, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## La Campanella — Trails and Ribbons

### LC Basic Trail
```
White basic trail strip on solid black background, horizontal gradient strip bright on left fading to black on right, the bright end has flickering chime-ring edge irregularity, the fade-out is not smooth but has resonance-pulse bright speckles scattered in the darker region like fading bell echoes, overall a chime-trail character, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### LC Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with visible standing wave pattern — fixed bright nodes at regular intervals connected by oscillating wave curves, each node point extra bright like a struck string's vibration node, the wave amplitude larger between nodes with bell-curve falloff, subtle harmonic overtone waves visible as fainter secondary oscillation, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### LC Spiraling Vortex Energy Strip
```
White spiraling energy strip on solid black background, horizontal strip with internal helical spiral pattern that twists along the strip length, the spiral made of bell-curve wave shapes wrapping around a central axis, bright spiral crests with darker troughs, overall the strip tapers from bright/wide on left to narrow/dark on right, ringing chime intensity, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## La Campanella — Lightning

### LC Lightning Surge
```
White jagged lightning bolt on solid black background, horizontal electrical arc with bell-strike character, the main bolt path thick and angular with sharp 90-degree direction changes, bright white core with thinner secondary forking branches, each branch terminates in a tiny bright chime point, the bolt shape suggests a bell rung with overwhelming force — angular resonance cracks radiating outward, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## La Campanella — Noise Textures

### LC Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, wave pattern composed of rapid high-frequency oscillations like vibrating piano strings at fortissimo, dense tightly-packed waves with occasional resonance peaks where waves constructively interfere, the pattern suggests virtuosic speed and intensity, sharp wave crests with rapid dropoffs, 256x256px seamless tile --ar 1:1 --style raw
```

### LC Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, swirling vortex pattern with bell-resonance character, resonant energy currents spiraling inward toward a bright center, the spiral arms have vibrating chime texture rather than smooth curves, turbulent eddies along the spiral edges, intense bright concentration at the center suggesting extreme acoustic resonance, 256x256px seamless tile --ar 1:1 --style raw
```

### LC Unique Theme Noise — Bell Resonance Pattern
```
Grayscale seamless tileable noise texture on solid black background, concentric expanding circles like acoustic waves radiating from a struck bell, circles vary in brightness with harmonic relationships, some circles wider and brighter (fundamental frequency) some thinner and fainter (overtones), interference patterns where different resonance circles overlap create complex moire-like zones, 256x256px seamless tile --ar 1:1 --style raw
```

---

## La Campanella — Smoke and Atmospheric

### LC Smoke Puff Cloud
```
White smoke puff on solid black background, billowing cloud of bell-resonance haze with heavy dense character, thick opaque center with wispy trailing tendrils, small bright chime-spark points scattered within the smoke body, the cloud shape slightly elongated vertically suggesting rising vibration energy, textured with concentric ring density variations, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## La Campanella — Particles (Theme-Specific Versions)

### LC Music Note
```
White music note symbol on solid black background, standard eighth note shape rendered as if cast from resonant bell bronze, the note head round and bright like a bell shape, the stem has subtle chime-metal texture, tiny concentric resonance rings emanating from the note head, sharp clean edges with very subtle vibration glow bloom around the silhouette, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Quarter Note
```
White quarter note on solid black background, filled note head with straight stem, the note head shaped like a miniature bell seen from the side, the stem has a subtle taper like a bell's clapper, surrounded by extremely faint concentric vibration rings emanating from the note head, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Whole Note
```
White whole note on solid black background, open oval ring shape like a bell mouth seen from above, the ring thick and bright with hollow dark center, subtle radial vibration lines extending outward from the ring, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Star Particle
```
White star particle on solid black background, four-pointed star with each point shaped like a small chime prong, sharp elongated points with bright tips, the center where points meet is the brightest with concentric resonance rings, subtle asymmetry in point lengths suggesting vibration, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Glyph
```
White arcane glyph on solid black background, circular symbol incorporating a bell silhouette at its center surrounded by musical notation fragments, small chime decorations at the cardinal points, thin precise line work, the overall shape suggests a musical resonance ward or carillon sigil, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Energy Flare
```
White energy flare burst on solid black background, radial burst of chime resonance exploding outward from center, bright intensely white core with elongated vibration streaks radiating at varied angles, some streaks curve slightly suggesting concentric bell-ring waves, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Sword Arc Particle
```
White sword arc particle on solid black background, 90-degree crescent arc with bell-resonance wave edges on the outer curve, the arc bright and thick transitioning from maximum brightness at center to fading at both ends, inner edge clean and sharp, outer edge rippling with chime vibrations, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Halo Ring
```
White glowing halo on solid black background, circular ring of light with chime-like ringing brightness variations around its circumference, the ring width varies — thicker at top and bottom (bell vibration nodes), thinner at sides, soft resonance bloom around the ring, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Flame Impact Explosion
```
White bell-strike explosion on solid black background, violent radial explosion with thick resonance waves shooting outward in all directions, bright white-hot center with dense overlapping chime-ring shapes, outer edges have individual bell-fragment wisps breaking free, overall shape slightly asymmetric suggesting directional force, game particle texture, 64x64px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 2: EROICA — The Hero's Symphony
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Third Symphony — the hero's journey, triumph through adversity, noble sacrifice
> **Visual Motifs:** Sakura blossoms, falling cherry blossom petals, sakura branches in bloom, petal cascades, sakura petal storms, blossom-strewn paths, pink petal whorls, sakura canopies, cherry blossom drifts, floating petal clouds, blooming branch silhouettes
> **Emotional Core:** Courage, sacrifice, triumphant glory, noble spirit
> **Colors (applied at runtime):** Scarlet, crimson, gold, sakura pink

---

## Eroica — Beam Textures

### ER Braided Energy Helix Beam
```
White intertwined double-helix beam on solid black background, two spiraling strands wrapped around each other like intertwined sakura branches, each strand composed of small petal-shaped segments connected in a flowing chain, bright core strand with softer outer strand, elegant natural proportions, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Cracking Energy Fissure Beam
```
White cracking fissure beam on solid black background, horizontal beam with noble fracture patterns resembling cracked sakura wood or splitting cherry bark, the cracks form organic branching patterns like sakura tree branches, bright fragments with darker dividing lines, the fragmentation beautiful rather than chaotic — petals revealed in the cracks, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of rising energy shapes like sakura petals carried on a heroic wind, each petal-shape swirls gently while flowing left to right, some petals bright and close some gray and distant creating depth, graceful and noble motion rather than aggressive, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Energy Surge Beam
```
White energy surge beam on solid black background, powerful horizontal beam with sakura-blossom crescendo intensity, the energy builds from left to right in rising waves like petals caught in a spring wind reaching its climax, each wave crest crowned with small cherry blossom shapes, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Horizontal Beam Streak
```
White horizontal beam streak on solid black background, clean powerful beam with subtle petal-drift edges flowing along both sides like sakura petals falling past the beam, the central beam steady and strong, edge ripples suggest drifting blossom-like flowing motion, noble and directed, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### ER Infernal Beam Ring
```
White ring-shaped beam element on solid black background, heroic sakura wreath ring with evenly spaced cherry blossom decorations around its circumference, the ring bold and thick with natural proportions, each blossom bright and clearly defined, gaps between blossoms softly glowing, the overall ring majestic like a victor's sakura crown, game VFX texture, 128x128px --ar 1:1 --style raw
```

### ER Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of marching wave forms like a heroic hymn's rhythm — steady consistent peaks with noble regularity, each wave crest adorned with a tiny rising sakura petal shape, the wave pattern strong and graceful, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of concentric arcing waves flowing left to right like a battle cry carrying across a field, each wave arc bold and broad, waves overlap creating interference patterns that form subtle cross/shield shapes at intersections, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Turbulent Plasma Core Segment
```
White turbulent core on solid black background, horizontal beam core with roiling energy that suggests sakura petals rising on warm spring updrafts, upward-flowing bright streams within the beam body, scattered bright points like cherry blossom petals rising from a blooming grove, noble and reverent in character rather than chaotic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Thin Linear Glow
```
White thin linear glow on solid black background, fine horizontal line like a drawn sword blade catching light, razor-sharp central edge with elegant soft bloom extending symmetrically above and below, the bloom slightly warmer/wider at center suggesting noble brilliance, absolutely straight and unwavering — heroic resolve, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

### ER Beam Lens Flare Explosion Impact
```
White radial burst on solid black background, heroic detonation burst with six major radiating beams arranged like a star/compass rose, the vertical beam longest (reaching toward heaven), each beam tapers perfectly, concentric rings between the beams like petal-scatter waves, rising sakura petal shapes scattered throughout, game VFX texture, 128x128px --ar 1:1 --style raw
```

### ER Radial Burst Fade Streaks
```
White radial burst with fading streaks on solid black background, central bright point with streaks radiating outward like rays of dawn breaking, streaks elegant and evenly distributed, each fading from bright base to fine gray tip, creating a sunrise/glory burst effect, the spaces between streaks softly illuminated, game VFX texture, 128x128px --ar 1:1 --style raw
```

### ER Radial Burst Heavy Streaks
```
White heavy radial burst on solid black background, powerful glory burst with thick bold streaks like a sunburst behind a hero's silhouette, streaks wide at base and dramatically tapering, slight sakura-petal shapes visible between the main streaks as smaller accent elements, commanding and triumphant, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## Eroica — Glow and Bloom

### ER Glow Orb
```
White soft circular glow on solid black background, noble radiant orb with warm smooth Gaussian falloff, the center bright and pure, the falloff even and symmetrical like a hero's aura, subtle suggestion of upward energy — the top hemisphere very slightly brighter than the bottom, creating a rising-spirit feeling, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### ER Lens Flare
```
White lens flare on solid black background, complex multi-element flare with a strong central disc, six delicate ray spikes extending outward, and a subtle hexagonal bokeh ghosting effect, classical and clean like light reflecting off polished armor, each element crisp and intentional, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### ER Point Bloom
```
White pinpoint bloom on solid black background, small intensely bright center with extensive soft bloom, the bloom envelope perfectly circular and symmetrical, suggesting a steady unwavering flame of heroic will — no flicker, no distortion, pure concentrated light, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### ER Soft Glow
```
White soft glow on solid black background, large diffuse luminous cloud with noble warmth, even gentle falloff in all directions, the glow has faint texture suggesting rising heat from a sacred flame, overall warm and enveloping like a protective aura, game VFX bloom texture, 256x256px --ar 1:1 --style raw
```

### ER Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with very subtle sakura-wreath patterning at the mid-radius — barely visible brighter arcs suggesting petal shapes arranged in a circle within the overall soft glow, creating an almost subliminal crown of cherry blossoms, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### ER Star Flare
```
White star flare on solid black background, four-pointed star with perfectly symmetrical spikes suggesting a compass rose or heraldic star, points sharp and clean, each spike same length representing balanced heroic virtue, bright center core with soft gray between points, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Eroica — Impact Effects

### ER Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding shockwave visualized as a heroic petal burst, the main wave ring thick and bold, followed by secondary thinner rings at harmonic intervals, rising sakura-petal particles lifting from the ring's passage, the rings suggest outward-expanding cherry blossom storm, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Crumbling Shatter Burst
```
White shatter burst on solid black background, noble destruction — large petal-shaped fragments breaking outward like cherry blossoms torn from a branch by a powerful wind, each fragment has organic curved edges, fragments larger near center becoming smaller toward edges, between fragments a bright lattice of breaking energy, suggests sakura branches shattering under the weight of bloom, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Power Effect Ring
```
White power ring on solid black background, majestic expanding ring with evenly spaced small sakura-blossom protrusions along the outer edge, each protrusion a tiny cherry blossom five-petal shape, the ring itself bold and unwavering, inner glow slightly brighter creating depth, a ring of blossoming power expanding from the strike point, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Radial Slash Star Impact
```
White radial slash star on solid black background, six-pointed impact star where each point resembles a sakura petal elongated into a blade shape, points alternate in length, bright center is a hexagonal core shaped like a cherry blossom viewed from above, the overall shape suggests a memorial blossom sigil left at the point of impact, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER X-Shaped Impact Cross
```
White X cross impact on solid black background, two crossing slashes forming a noble X — each slash clean and precise like sword strikes, the intersection forms a bright diamond shape, each slash tapers elegantly to fine points, subtle sakura petal shapes scattered near the crossing point, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Eroica — Projectiles

### ER Gyratory Orb
```
White spinning orb on solid black background, spherical projectile with visible internal rotation of noble energy, the surface has subtle five-petal sakura-faceted panels, bright equatorial band, a faint ring of small rising petal points orbiting the sphere's circumference, dignified and powerful, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### ER Orbiting Energy Sphere
```
White energy sphere on solid black background, central bright sphere with three small sakura-petal-shaped energy wisps orbiting around it in a tilted ring, the main sphere has a warm heroic glow, each orbiting wisp leaves a very faint gray trail, elegant orbital paths, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### ER Pulsating Music Note Orb
```
White music orb on solid black background, glowing sphere containing a visible musical note silhouette inside, the note shape elegant and refined like calligraphic script, the sphere surface has a faint sakura-petal lattice, subtle cherry blossom wisps float upward from the sphere's top, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Eroica — Slash Arcs

### ER Flaming Sword Arc Smear
```
White sword arc smear on solid black background, noble 120-degree sweeping crescent with clean powerful edges, the arc thick and decisive, outer edge has small sakura petal shapes breaking away and trailing behind, inner edge razor-sharp like a hero's blade, brightness peaks at the leading edge and fades toward the trailing end, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### ER Full Circle Sword Arc Slash
```
White full 360-degree sword arc on solid black background, complete spinning slash circle with heroic intensity, the ring uniformly thick and decisive like a whirlwind of blades, evenly spaced brighter nodes around the circumference where the slash is most concentrated, tiny rising sakura petals scattered inside the circle drifting upward, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### ER Sword Arc Smear
```
White sword arc on solid black background, clean precise 90-degree arc with the grace of a master swordsman's stroke, the arc perfectly even in width from start to finish suggesting practiced technique, subtle leading-edge brightening, extremely smooth and controlled, slight trailing sakura wisps at the arc's end, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Eroica — Trails and Ribbons

### ER Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading to right, the bright end decorated with subtle petal-drift ripple edges, the fade-out has small rising sakura petal sparkle points drifting upward from the trail body, the overall trail noble and warm, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### ER Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with visible wave pattern — the wave peaks are decorated with tiny sakura-petal shapes like cherry blossoms strung along the ribbon, steady and rhythmic — a graceful waltz tempo rather than wild oscillation, soft bloom at each wave node, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### ER Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with internal helical pattern of rising energy, the spiral arms shaped like sakura petals caught in an updraft — elongated and elegant, twisting upward as they progress along the strip, bright petal crests with soft gray valleys, tapering from wide/bright left to narrow/faint right, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Eroica — Lightning

### ER Lightning Surge
```
White lightning bolt on solid black background, horizontal electrical arc with heroic character — the bolt follows a bold decisive path with strong directional momentum, branches are clean and angular like sword slashes, each branch terminates in a bright point, the bolt has more order than chaos — a directed strike rather than wild discharge, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Eroica — Particles

### ER Music Note
```
White music note on solid black background, elegant eighth note with calligraphic serif styling, the note head crisp and round, the flag flowing like a tiny banner, thin clean lines with subtle soft bloom halo, refined and heroic rather than playful, game particle texture, 32x32px --ar 1:1 --style raw
```

### ER Sakura Petal (Unique to Eroica)
```
White sakura petal on solid black background, single cherry blossom petal with realistic organic shape — slightly cupped, asymmetric, with a visible central vein, the petal bright and softly luminous, edges slightly translucent and thinner, the shape carries grace and gentle movement, game particle texture, 32x32px --ar 1:1 --style raw
```

### ER Rising Ember
```
White rising ember particle on solid black background, tiny elongated bright point like an ember floating upward from a sacred fire, slight teardrop shape wider at bottom narrowing at top, very small and delicate with a faint soft glow halo, game particle texture, 16x16px --ar 1:1 --style raw
```

### ER Laurel Leaf
```
White sakura petal on solid black background, single cherry blossom petal in gentle curved shape, pointed oval with visible central vein and delicate texture, bright and sharply defined, the petal curves slightly as if drifting on wind, noble botanical accuracy, game particle texture, 32x16px --ar 2:1 --style raw
```

### ER Glyph
```
White heroic glyph on solid black background, circular symbol incorporating a sakura blossom silhouette crossed with a cherry branch, surrounded by a thin precise ring, small petal dots at the cardinal points, elegant botanical styling, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 3: SWAN LAKE — Grace in Monochrome
# ═══════════════════════════════════════════

> **Musical Soul:** Tchaikovsky's ballet — dying grace, ethereal beauty, elegance even in destruction
> **Visual Motifs:** Pearlescent nacre sheen, chaotic swan wake turbulence, iridescent water disturbance, churning wake foam, pearl shell fragments, shattered pearl surfaces, swirling opalescent eddies, prismatic rainbow shimmer in churned water, roiling white water, nacre-coated debris, mother-of-pearl fracture patterns
> **Emotional Core:** Elegance, tragedy, ethereal beauty, graceful departure
> **Colors (applied at runtime):** Pure white, black contrast, prismatic rainbow edges

---

## Swan Lake — Beam Textures

### SL Braided Energy Helix Beam
```
White intertwined double-helix beam on solid black background, two spiraling ribbon-like strands wrapped around each other like churning wake currents intertwining, each strand flowing with pearlescent turbulence and nacre-sheen surface, the twist is chaotic and powerful yet beautiful, iridescent shimmer highlights along the strand surfaces, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with pearlescent fracture patterns like cracking mother-of-pearl shell, thin iridescent fractures spreading in chaotic branching patterns, bright nacre shards along the cracks revealing opalescent layers beneath, the beauty of something precious shattering into prismatic chaos, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of churning wake-foam shapes like the chaotic turbulence behind a swan's powerful stroke, each shape roiling and tumbling with pearlescent sheen, scattered pearl fragments drift left to right creating iridescent depth layers, graceful chaos, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with chaotic crescendo intensity like a swan's powerful takeoff churning the lake — energy builds in a violent surge, peaks with pearlescent brilliance, then crashes back, the peak has a spray of nacre-sheen fragments scattered like roiling wake foam, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Horizontal Beam Streak
```
White horizontal beam on solid black background, clean linear beam with chaotic wake-ripple distortion along both edges like viewing the beam through churning pearlescent water, the central line pure and bright, the ripple edges create a turbulent iridescent shimmering effect, powerful and ethereal, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### SL Infernal Beam Ring
```
White ring beam element on solid black background, circular ring like a swan's wake spreading in a chaotic circle on disturbed water, the ring thick and turbulent with pearlescent nacre texture, secondary and tertiary concentric wake-rings spaced inside and outside, the water-turbulence quality is wild yet beautiful, game VFX texture, 128x128px --ar 1:1 --style raw
```

### SL Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of chaotic undulations like the turbulent wake of a swan's powerful wing-beats on water, waves irregular and overlapping with pearlescent shimmer at their crests, the pattern wild yet hypnotic, nacre highlights catching light, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of expanding arcs flowing left to right like chaotic wake-waves from a swan's passage, each arc churning with pearlescent turbulence, arcs collide and create moiré interference patterns of chaotic iridescent beauty, powerful and overwhelming, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with violent pearlescent turbulence like a swan churning lake water into foam, chaotic roiling forms that crash and coalesce with nacre-sheen highlights, bright churning wake-foam within the beam body, wild yet beautiful internal chaos, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Thin Linear Glow
```
White thin linear glow on solid black background, extremely fine horizontal line like a strand of pearlescent wake-light cutting through dark water, iridescent and barely visible with soft prismatic bloom, the thinness itself is the beauty — fragile, precise, a single thread of nacre in darkness, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

---

## Swan Lake — Glow and Bloom

### SL Glow Orb
```
White soft circular glow on solid black background, ethereal orb with wide pearlescent Gaussian falloff, the center radiating with nacre-sheen intensity creating a diffuse iridescent quality, like light refracting through a shattered pearl, extremely soft and dreamlike, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### SL Lens Flare
```
White lens flare on solid black background, multi-element flare with elongated horizontal anamorphic streak dominant, iridescent nacre-rainbow diffraction rings ghosting above and below the streak, the flare feels prismatic and pearl-like, each element razor-fine and pearlescent, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### SL Point Bloom
```
White pinpoint bloom on solid black background, tiny bright center with extensive wide soft bloom that creates a pearlescent spotlight quality, the bloom iridescent and shimmering, suggesting a single precious pearl illuminating from within on a dark stage, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### SL Star Flare
```
White star flare on solid black background, six-pointed star with long impossibly thin spikes like fractured nacre crystal needles, each spike hair-fine and slightly iridescent, the hexagonal symmetry of a shattered pearl, bright center point, the spikes seem to shimmer with pearlescent fragility, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Swan Lake — Impact Effects

### SL Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding wake-ripple impact like a swan crashing onto turbulent water, concentric circles expanding outward with chaotic energy, each ring churning with pearlescent turbulence, the gaps between rings filled with nacre debris and foam, scattered iridescent shell-fragment particles between the rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL Crumbling Shatter Burst
```
White shatter burst on solid black background, pearlescent fragmentation like a mother-of-pearl shell shattering, irregular polygon shards of nacre breaking apart with beautiful chaotic energy, each shard has smooth iridescent faces and sharp edges, tiny prismatic sparkle points at shard corners where light refracts through the pearl layers, elegant destruction, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL Power Effect Ring
```
White power ring on solid black background, thin chaotic expanding ring with small pearl-fragment shapes trailing from the outer edge like nacre debris shed from a churning wake, the ring itself pearlescent and turbulent, fragments drift outward and scatter chaotically, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL X-Shaped Impact Cross
```
White X cross on solid black background, two crossing lines forming a chaotic X like intersecting swan wake-paths on disturbed water, each line turbulent and flowing with pearlescent wave-like curves, the intersection has a bright iridescent bloom, wake-froth edges taper to fine dissolving points, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Swan Lake — Slash Arcs

### SL Sword Arc Smear
```
White sword arc on solid black background, chaotic 120-degree sweeping arc like a swan's wing slashing through water, the arc wide and churning with pearlescent wake-foam edges, brightness gradient flows from leading to trailing edge, inner curve decorated with tiny nacre-fragment wisps breaking away in turbulent scatter, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### SL Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete chaotic wake-circle — the ring churns with pearlescent turbulence like a swan spinning on the water's surface, thicker sections of roiling wake-foam and thinner sections of iridescent spray, tiny scattered pearl-fragment points within the circle like nacre debris caught in a whirlpool, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Swan Lake — Trails and Ribbons

### SL Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading right, the bright end has churning wake-foam edges with pearlescent sheen, the fade-out dissolves into individual small floating nacre-shard particles rather than a smooth gradient, chaotic beautiful dissolution, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### SL Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with chaotic flowing wave — the wave pattern turbulent and powerful like a swan's wake on disturbed water, each wave crest catches pearlescent light like nacre-sheened foam, wild and beautiful in its irregularity, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Swan Lake — Particles

### SL Music Note
```
White music note on solid black background, flowing music note with pearlescent nacre surface, the note head opens into a tiny crescent shape with iridescent sheen, the flag extends into a flowing wake-trail tail that churns elegantly, mother-of-pearl texture throughout, game particle texture, 32x32px --ar 1:1 --style raw
```

### SL Swan Feather (Already exists — enhance)
```
White swan feather on solid black background, single large feather with visible rachis and barb structure, the feather surface coated in pearlescent nacre sheen with iridescent highlights, some barbs scattered chaotically at the tip as if torn by turbulent wake, luminous and opalescent with prismatic edge translucency, game particle texture, 48x24px --ar 2:1 --style raw
```

### SL Crystal Shard
```
White pearl shard on solid black background, elongated mother-of-pearl fragment with layered nacre surfaces, each layer has a different iridescent brightness suggesting light refracting through pearlescent material, the shard slightly asymmetric and chaotic-looking, tiny bright prismatic points at the shard's sharpest vertices, game particle texture, 32x16px --ar 2:1 --style raw
```

### SL Water Ripple
```
White wake-ripple on solid black background, single expanding chaotic circular wave like turbulent wake-rings spreading from a swan's powerful landing, the ripple ring thick and churning with pearlescent foam-sheen, with a smaller more violent secondary ring inside it, iridescent and dynamic, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 4: MOONLIGHT SONATA — The Moon's Quiet Sorrow
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Adagio sostenuto — quiet sorrow, moonlit peace, gentle nocturnal melancholy
> **Visual Motifs:** Crescent moons, twinkling star fields, moonbeam shafts laced with starlight, silvery mist, lunar phases, scattered star clusters, gentle star-point sparkles, celestial moon-and-star arrangements, moon haloes with star companions, starlit night sky fragments, soft fog rolling under stars
> **Emotional Core:** Melancholy, peace, mystical stillness, quiet sorrow
> **Colors (applied at runtime):** Deep dark purples, vibrant light blues, violet, ice blue

---

## Moonlight Sonata — Beam Textures

### MS Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands of lunar energy winding around each other like moonlit currents intertwining, the strands flow like slow starlight — thick and languid, soft edges suggest mist-shrouded forms with tiny embedded star points twinkling along the strands, gentle and hypnotic rather than energetic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with hairline cracks like the surface of the moon — a network of fine gentle craters and rilles, the cracks are not violent but ancient and weathered, bright faces of lunar terrain catch light while shadows pool in the cracks, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting starlit mist forms like fog rolling across a moonlit sky, each mist shape soft-edged with tiny star-point sparkles embedded within, some forms resemble small crescent shapes with companion stars drifting alongside, the motion is slow and meditative, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Energy Surge Beam
```
White tidal surge beam on solid black background, horizontal beam with gentle lunar wave energy — the intensity rises and falls like the moon's pull on gentle tides, broad gentle swells rather than sharp peaks, the brightest points are smooth rounded crests adorned with small twinkling star points, a sense of gravitational pull and moonlit starfield influence, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of slow deep oscillations like the adagio tempo of the sonata, long wavelengths and deep amplitude suggesting profound emotional weight, each wave crest has a small crescent-shaped highlight with tiny companion star points flanking it like a moon with stars, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of overlapping soft arcs flowing left to right like moonbeam shafts laced with starlight through clouds, each arc broad and gentle, the arcs create soft interference patterns like moonlight and stars dappling through a night canopy, quiet and serene, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with slow rolling fog-like turbulence, thick mist banks that drift and eddy like ground fog in moonlight, the turbulence is gentle and meditative — soft billowing forms merging and separating unhurriedly, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Glow and Bloom

### MS Glow Orb
```
White soft circular glow on solid black background, gentle lunar orb with wide ethereal falloff, the center moderately bright with an extremely gradual Gaussian fade creating maximum atmospheric haze, the glow feels like looking at the moon through a thin veil of clouds with a few tiny star points scattered in the haze, soft and sorrowful, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### MS Lens Flare
```
White lens flare on solid black background, crescent-shaped primary flare element — a soft bright crescent like the moon in its waning phase, accompanied by two or three small star-point ghost elements along a diagonal line like companion stars, quiet and understated rather than dramatic, the crescent-and-stars arrangement is the dominant element, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### MS Star Flare
```
White star flare on solid black background, four-pointed star with elongated VERTICAL spike suggesting a moonbeam descending from above with twinkling star companions, the vertical spike three times longer than the horizontal spike, creating an asymmetric shape like starlit moonlight streaming down, soft and gentle, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### MS Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle crescent-shaped brighter region on one side, as if the bloom itself has lunar phases — one half slightly brighter than the other, creating an asymmetric moon-like quality, extremely soft edges, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Impact Effects

### MS Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, lunar ripple impact — soft concentric rings expanding outward like moonlit waves on a shore beneath a starry sky, each ring broad and gentle with wide spacing, the rings fade gradually rather than sharply, tiny twinkling star-point particles drifting between the rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### MS Power Effect Ring
```
White power ring on solid black background, thin crescent-decorated ring — the ring itself slender and elegant, with tiny crescent moon shapes replacing the expected uniform thickness, crescents face different directions creating a lunar phase sequence around the circumference, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### MS Radial Slash Star Impact
```
White radial star impact on solid black background, four-pointed impact star with softened rounded points like moonbeams crowned with tiny star sparkles, each point tapers gradually with wide soft edges, the spaces between points have very faint starlit fog-like fill rather than sharp darkness, gentle and melancholy, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Slash Arcs

### MS Sword Arc Smear
```
White sword arc on solid black background, gentle 120-degree sweeping arc like a moonbeam slowly drawn across a starlit sky, the arc soft and wide with misty starlit edges that blend smoothly into darkness, no harsh boundaries, scattered tiny star points embedded in the arc body, brightness fades from a gentle peak at center toward both ends, ethereal and sorrowful like a sigh made visible, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### MS Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete circular arc resembling a lunar halo — the bright ring around the moon on a starry misty night, the ring even and gentle with wide soft edges that fade to nothing, embedded tiny star points twinkling along the halo, a single brighter crescent segment suggesting a momentary brightening, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Trails and Ribbons

### MS Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading right, the bright end diffuse and misty like a moonbeam with tiny star sparkles, the fade-out is gradual and organic with wisps of starlit mist extending beyond the main body, the overall trail feels like moonlight and stars slowly dissipating, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### MS Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with gentle lunar undulation — very long wavelength low amplitude waves like the surface of a calm moonlit sea beneath stars, the wave crests have a faint crescent highlight with tiny star companions, incredibly gentle and meditative in rhythm, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Particles

### MS Music Note
```
White music note on solid black background, gentle half note rendered in thin delicate lines, the note head open (hollow) like a crescent moon, the stem thin and slightly curved like a sliver of starlight, surrounded by a scatter of very faint twinkling star-point dots, melancholic and quiet, game particle texture, 32x32px --ar 1:1 --style raw
```

### MS Crescent Moon
```
White crescent moon on solid black background, thin waning crescent moon shape with smooth curved inner edge and soft outer edge, the crescent tapers to fine points at both tips, faint shadow visible on the dark side suggesting the full sphere, game particle texture, 32x32px --ar 1:1 --style raw
```

### MS Moth Wing
```
White twinkling star cluster on solid black background, small cluster of three to five star points of varying brightness arranged in a gentle arc, each star has a tiny four-pointed sparkle, the cluster curves softly like a fragment of a constellation, ethereal and nocturnal, game particle texture, 32x24px --ar 4:3 --style raw
```

### MS Tidal Mist Wisp
```
White starlit mist wisp on solid black background, small elongated wisp of moonlit fog with tiny embedded star-point sparkles, softly feathered at all edges, slightly curved as if drifting beneath a starry sky, thicker at one end thinning to nothing at the other, the softest most ethereal particle with twinkling stars woven through, game particle texture, 32x16px --ar 2:1 --style raw
```

### MS Glyph
```
White glyph on solid black background, circular symbol with a crescent moon at center flanked by small twinkling star-point clusters, surrounded by a thin ring, tiny dot-stars at cardinal points and additional scattered star companions, the linework thin and delicate, suggests a lunar-stellar ward or moon-and-stars seal, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 5: ENIGMA VARIATIONS — The Unknowable Mystery
# ═══════════════════════════════════════════

> **Musical Soul:** Elgar's enigma — mystery, hidden meanings, what lurks behind the veil, unknowable truths
> **Visual Motifs:** Watching eyes, question mark silhouettes, cryptic glyphs, riddle-inscribed circles, puzzle-piece fragments, cipher text scrolls, mysterious keyholes, unsolved sigils, labyrinth patterns, mirrored reflections, hidden messages, arcane question symbols
> **Emotional Core:** Mystery, dread, arcane secrets, unknowable enigma
> **Colors (applied at runtime):** Void black, deep purple, eerie green flame

---

## Enigma Variations — Beam Textures

### EN Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands like enigmatic cipher scripts coiling around each other, the surfaces textured with tiny glyph-like markings and question mark fragments, the coil tightens and loosens irregularly as if encoding and decoding a mysterious message, cryptic and unsettling quality, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with reality-fracture cracks, the cracks don't follow material stress patterns but instead form question-mark-like curves and impossible puzzle-piece angles that shouldn't exist, some cracks seem to open into deeper mystery beyond the beam, Escher-like paradox geometry, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting cryptic glyph fragments, broken cipher symbols and arcane question-mark letterforms flowing left to right as if torn from a forbidden manuscript of riddles, some fragments rotate or overlap creating momentary recognizable (yet meaningless) words, unsettling, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with irregular chaotic intensity pulses, no discernible rhythm or pattern — the energy surges unpredictably, some sections intense and bright others dimly lit, eye-like shapes occasionally form in the brightness patterns then dissolve, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam where the wave pattern constantly shifts between different frequencies simultaneously, multiple incompatible wave patterns overlaid creating visual discord and confusion, standing wave nodes appear and vanish unpredictably, the mathematical chaos of unsolvable equations, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of warped and distorted arcs — the arcs bend and twist as if space itself is warping around the beam, some arcs curve backward, some spiral inward, the geometry is wrong and disorienting, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with writhing mysterious turbulence, the churning patterns form momentary eye shapes and question mark silhouettes before dissolving back into chaos, something watches from within the energy — an unanswered riddle given form, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Glow and Bloom

### EN Glow Orb
```
White glow on solid black background, circular glow that isn't quite circular — the edges warp and distort as if the light itself is being pulled through a spatial anomaly, the center slightly off-center creating an unsettling asymmetry, the falloff uneven and wrong, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### EN Lens Flare
```
White lens flare on solid black background, a flare arrangement that forms a subtle eye shape — the central disc is the iris, horizontal streaks form the eye opening, faint circular elements above and below suggest eyelids, disturbing and watchful, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### EN Star Flare
```
White star flare on solid black background, irregular star with an inconsistent number of points — some points longer, some missing, some pointing at wrong angles, creating an asymmetric disturbing shape like a corrupted geometry, bright center, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Impact Effects

### EN Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding wave rings that warp and distort reality, the rings are not perfect circles but wobble and buckle as if reality resists their expansion, sections of rings fade to nothing mid-arc then reappear like vanishing riddles, eye-shaped distortion at center watching, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN Crumbling Shatter Burst
```
White shatter burst on solid black background, reality fragmenting into impossible Escher-like shards, each fragment shows a different perspective angle despite being flat, the gaps between fragments show void, some shards overlap in ways that violate geometry, deeply unsettling fragmentation, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN Power Effect Ring
```
White power ring on solid black background, expanding ring decorated with arcane glyphs and eye motifs spaced around its circumference, the ring itself slightly irregular as if inscribed by a mysterious hand, the glyphs are legible but meaningless — unsolvable riddle symbols from no known language, question-mark fragments between the glyphs, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN X-Shaped Impact Cross
```
White X cross on solid black background, two crossing slashes that don't quite intersect correctly — they slip past each other in impossible perspective, each slash has mysterious glyph-like wisps trailing from it, the near-intersection point has a bright disturbing eye-shaped flash that seems to ask a question, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Enigma Variations — Slash Arcs

### EN Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc whose edges ripple with tiny glyph and question-mark protrusions, the arc body shows faint eye-spot patterns at irregular intervals, brightness is uneven — sections flicker between bright and dim unpredictably, the whole arc has a cryptic mysterious quality, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### EN Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete circle that isn't quite a circle — it spirals inward very slightly creating a subtle vortex, the ring surface textured with tiny cryptic glyph symbols and question marks, bright sections alternate asymmetrically with dimmer sections, the overall shape is disorienting and riddling, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Enigma Variations — Trails and Ribbons

### EN Basic Trail
```
White trail strip on solid black background, horizontal gradient that doesn't fade smoothly but dissolves in patches — sections of the trail vanish revealing the void behind while other sections remain bright, as if the trail exists partially as an unsolvable riddle, disturbing intermittent visibility like a mystery that reveals and conceals, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### EN Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with a wave pattern that reverses direction partway through, the wave phase shifts impossibly in the middle creating a visual paradox, standing wave nodes blink like eyes opening and closing, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Lightning

### EN Lightning Surge
```
White lightning bolt on solid black background, electrical arc that branches into cryptic glyph-like shapes rather than jagged angles, the branches form partial question marks and mysterious symbols, some branches curl back on themselves forming eye-like loops, the bolt seems directed by unknowable intelligence rather than physics, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Particles

### EN Music Note
```
White music note on solid black background, a music note where the note head is replaced by a tiny eye shape, the stem and flag rendered in mysterious cipher-like linework, the overall shape barely recognizable as a note — a riddle encoded in musical form, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Enigma Eye (Already exists — enhance variants)
```
White watching eye on solid black background, single detailed eye with visible iris ring and bright pupil center, the eye is slightly inhuman — too round, the iris too detailed, small question-mark-shaped wisps extend from the corners, the eye radiates an active searching awareness as if seeking an answer, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Void Tendril
```
White question mark wisp on solid black background, single curving abstract shape tapering from thick base to thin tip forming a question-mark-like silhouette, the surface textured with tiny glyph marks, the shape curves in an S-shape as if asking a question, slightly transparent at the thinnest end, game particle texture, 48x16px --ar 3:1 --style raw
```

### EN Impossible Geometry Shard
```
White puzzle shard on solid black background, small impossible-geometry puzzle piece, each face has a different brightness suggesting an object that cannot exist in 3D space, clean lines and precise angles forming a riddle in geometric form, mathematically mysterious, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Glyph (Occult)
```
White glyph on solid black background, complex arcane glyph with concentric circles and intersecting lines forming a mysterious cipher diagram, a watching eye at the very center, tiny illegible question-mark-like symbols along the circle borders, cryptic and unsolvable, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 6: FATE — The Celestial Symphony of Destiny
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Fifth — cosmic inevitability, destiny knocking, the weight of the stars themselves
> **Visual Motifs:** Ancient celestial glyphs, star maps, cosmic nebula swirls, event-horizon rings, light-bending gravity, supernova detonation, constellation patterns, infinite void, cosmic strings, gravitational lensing
> **Emotional Core:** Cosmic inevitability, endgame awe, celestial power beyond comprehension
> **Colors (applied at runtime):** Black void, dark pink, bright crimson, celestial white

---

## Fate — Beam Textures

### FA Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands of cosmic energy wrapping around each other like binary star orbits, gravitational distortion visible as warped bright points along the strands, each strand shimmers with embedded tiny star-point lights, the cosmic scale feels immense, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### FA Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with spacetime-fracture cracks, the cracks look like they go infinitely deep — darker than the surrounding black, reality tearing to reveal the void between dimensions, gravitational lensing warps the edges of each crack, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### FA Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with supernova-pulse intensity, the energy periodically surges to overwhelming brightness then contracts, each surge leaves a brief afterimage ring expanding outward (like a supernova shockwave), the scale is astronomical, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### FA Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam composed of gravitational wave oscillations — deep spacetime ripples that compress and stretch space itself, the wave pattern subtly distorts the beam's own edges (gravitational lensing), embedded tiny constellation-like point clusters at wave nodes, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### FA Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with supernova-remnant turbulence, dense bright filaments and shock fronts colliding and interacting, nebula-like wisps of varying brightness, tiny embedded star points throughout the turbulence, the chaos of a stellar explosion, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Fate — Glow and Bloom

### FA Glow Orb
```
White glow on solid black background, circular glow with gravitational lensing distortion — the center slightly brighter ring around a less-bright core (like an Einstein ring), creating a halo-within-halo effect, the entire structure has immense cosmic weight, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### FA Lens Flare
```
White lens flare on solid black background, complex multi-element flare suggesting a celestial event, central starburst with multiple concentric rings creating a target-like pattern, thin diffraction spikes radiating outward, smaller ghost orbs in a line suggesting gravitational lensing of background stars, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### FA Star Flare
```
White star flare on solid black background, eight-pointed star with long thin spikes suggesting a supernova detonation, each spike razor-sharp and impossibly long, the center has a tiny dark point (the collapsed star core) surrounded by a bright ring, cosmic annihilation beauty, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Fate — Impact Effects

### FA Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding gravitational wave shockwave, the rings compress and distort space — each ring has a bright leading edge and a dark trough behind it where space is stretched, the rings expand at cosmological speed, tiny star points scatter when the wave passes, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### FA Crumbling Shatter Burst
```
White shatter burst on solid black background, spacetime itself fragmenting, each shard shows a different region of space (some with tiny constellation patterns, some pure void), the gaps between shards impossibly dark, gravitational lensing bends the edges of each fragment, reality-ending fragmentation, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### FA Power Effect Ring
```
White power ring on solid black background, event horizon ring — a bold thick ring that bends light around itself, the inner region is slightly dimmer (the gravity well), the ring itself radiates with ancient celestial glyph marks at cardinal points, accretion-disk quality, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Fate — Slash Arcs

### FA Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc that tears through spacetime, the arc edge has gravitational distortion — warped light along its boundary, tiny embedded star points within the arc body, the slash cuts through reality itself revealing an even brighter energy beyond, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### FA Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete event-horizon ring — thick and bold with gravitational lensing creating a secondary ring slightly offset inside the main ring, the inner space subtly dimmer suggesting objects caught in the gravity well, celestial glyph marks along the ring, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Fate — Trails and Ribbons

### FA Basic Trail
```
White trail on solid black background, horizontal gradient with embedded tiny star-point lights throughout the trail body like a section of the Milky Way, the bright end has a nebula-like dense bright cluster, the fade-out dissolves into scattered individual stars that dim and disappear, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### FA Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip where the standing wave nodes are marked by small constellation patterns, the wave itself deep and heavy — low frequency, high amplitude, suggesting gravitational wave oscillation, the ribbon feels immensely massive and unstoppable, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Fate — Particles

### FA Music Note
```
White music note on solid black background, music note silhouette composed of tiny star points arranged in the shape of the note, like a constellation forming a musical symbol, the note head is a cluster of bright stars, the stem a line of stars diminishing in brightness, game particle texture, 32x32px --ar 1:1 --style raw
```

### FA Celestial Glyph
```
White celestial glyph on solid black background, ancient astronomical symbol with concentric circles and radiating lines like an astrolabe diagram, celestial coordinate marks, tiny star points at key intersections, the geometry precise and mathematical — the language of the cosmos encoded in a symbol, game particle texture, 32x32px --ar 1:1 --style raw
```

### FA Cosmic String Fragment
```
White cosmic string on solid black background, thin curving line of incredibly intense brightness — a one-dimensional cosmic string left behind from the beginning of the universe, the string curves in a smooth arc, space around it appears warped (slight distortion of nearby elements), game particle texture, 32x32px --ar 1:1 --style raw
```

### FA Supernova Core
```
White supernova core on solid black background, tiny incredibly bright point surrounded by multiple expanding ring layers — the remnant of a stellar explosion, bright inner point, intermediate brightness ring, faint outer ring, the structure suggests immense energy compressed to a tiny size, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 7: CLAIR DE LUNE — Moonlit Reverie
# ═══════════════════════════════════════════

> **Musical Soul:** Debussy's Clair de Lune — dreamy, impressionistic, the beauty of a moonlit evening, gentle luminescence
> **Visual Motifs:** Shattered clock faces, broken clock hands, fractured hourglasses, scattered gear fragments, flowing sand from broken timepieces, pendulum arcs, clock number fragments, cracked glass watch faces, stopped clock towers, ticking echoes visualized, melting clock edges, time-frozen debris
> **Emotional Core:** Dreamlike calm, gentle luminescence, tender nostalgia, reverie
> **Colors (applied at runtime):** Night mist blue, soft blue, pearl white

---

## Clair de Lune — Beam Textures

### CL Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two gently twisting streams like the twin hands of a shattered clock spiraling through time's wake, each strand trailing broken gear-tooth edges and fractured numeral fragments, soft-focus quality as if time itself is blurring, dreamlike, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting clock fragments and gear pieces floating on a gentle current of time's wake, each shape small and precise — tiny gear teeth, clock hand fragments, numeral shards — with a soft halo, scattered at varying distances creating depth, peaceful yet melancholy, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with gentle clock-tick swells like seconds passing and accumulating, each swell soft-edged and rounded like a pendulum's arc, the intensity is never harsh — even the brightest points have soft-focus quality of time viewed through frosted glass, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam of gentle oscillations like a pendulum swinging back and forth through time, the ripples mark each tick with a soft clock-hand arc highlight at their peaks, the pattern dreamlike and soothing — the steady rhythm of time passing, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of overlapping soft circles like shattered clock faces viewed through time's distortion, each circle soft-edged with numeral fragments barely visible at the edges, creating a chain of gentle broken timepiece forms, dreamlike and melancholy, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Glow and Bloom

### CL Glow Orb
```
White glow on solid black background, extremely soft and diffuse circular glow, the center barely brighter than the surrounding bloom — everything is soft-focus, like looking at a distant light through dewy glass, the widest softest falloff possible, pure gentle luminescence, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### CL Lens Flare
```
White lens flare on solid black background, clock-face-style flare — multiple soft circular shapes of varying sizes arranged around a central soft disc like hour markers on a clock face, each element at a different radial position, like time fragments scattered through light, dreamy and unfocused, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### CL Star Flare
```
White star flare on solid black background, soft diffuse four-pointed star with clock-hand-shaped points — two longer points (hour and minute hands) and two shorter, the edges completely faded and blurred, like a clock face dissolving into time's wake, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Impact Effects

### CL Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, gentle expanding ripple circles like the aftershock of a clock shattering, the rings wide-spaced and soft like time ripples spreading outward, each ring barely brighter than the next, no harshness — everything fades with dreamlike softness, tiny clock-gear-fragment points drifting between rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### CL Power Effect Ring
```
White power ring on solid black background, gossamer-thin ring that is more impression than solid line, the ring formed by many tiny soft dots arranged in a circle like clock-face hour markers, each dot has a miniature bloom like the glow of numerals on a phosphorescent watch face, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Clair de Lune — Slash Arcs

### CL Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc rendered like a clock hand sweeping through time, the arc wide and soft like a timepiece's motion blur, edges completely blurred and organic, no sharp lines — the slash is a gentle sweeping arc of a clock hand dissolving into time's wake, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Clair de Lune — Trails and Ribbons

### CL Basic Trail
```
White trail on solid black background, horizontal gradient that dissolves into scattered soft clock-fragment dots like broken timepiece elements gradually spacing farther apart and dimming, the bright end is a diffuse soft glow of concentrated time, the fade-out is a constellation of dimming clock shards, dreamlike and wistful, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### CL Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with the gentlest possible undulation — almost flat with barely perceptible pendulum-swing wave, the wave crests have soft highlights like the gentle tick of a clock's second hand, extremely calm and meditative — the quiet passage of time, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Particles

### CL Music Note
```
White music note on solid black background, whole note rendered as a soft luminous oval with a clock-face quality — edges blurred, the interior slightly textured like the worn surface of an antique timepiece, surrounded by a gentle misty glow of fading time, the note seems to float and drift like a moment suspended in time's wake, game particle texture, 32x32px --ar 1:1 --style raw
```

### CL Clock Gear Fragment
```
White clock gear fragment on solid black background, tiny bright gear-tooth shape with very wide soft bloom extending around it, the bloom slightly elongated suggesting the fragment is drifting through time's wake, like a single piece of a shattered clock suspended in eternal freefall, minimal and melancholy, game particle texture, 16x16px --ar 1:1 --style raw
```

### CL Hourglass Sand Grain
```
White hourglass sand grain on solid black background, single perfect sphere of luminous sand catching light, a bright highlight crescent on upper left, softer fill glow on lower right, extremely tiny and precious, like a single grain of time freed from a shattered hourglass, game particle texture, 16x16px --ar 1:1 --style raw
```

### CL Clock Hand Fragment
```
White clock hand fragment on solid black background, single narrow tapered shard — wider at one end narrowing to a point like a broken clock hand, slightly textured with fine engraved lines, soft-edged, like a minute hand snapped from a shattered timepiece and dissolving into light, game particle texture, 32x16px --ar 2:1 --style raw
```

### CL Glyph
```
White glyph on solid black background, circular symbol with a clock face silhouette at center — Roman numerals faintly visible around the rim, surrounded by a ring of tiny gear-tooth dots, the linework soft and slightly imprecise — the clock is cracked and fading, gentle and dreamlike, game particle texture, 32x32px --ar 1:1 --style raw
```

---

## Clair de Lune — Projectiles

### CL Gyratory Orb
```
White spinning orb on solid black background, spherical projectile with gentle internal swirling like clock hands spinning inside a glass sphere, the surface has soft-focus temporal quality, rotation suggested by subtle directional gear-tooth streaks within the sphere, surrounded by a wide tender bloom of fading time, dreamy and precious, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### CL Orbiting Energy Sphere
```
White energy sphere on solid black background, central soft glow orb with three tiny clock-gear-like points orbiting gently around it, each orbiting point has a miniature bloom trail, the main sphere has soft-focus temporal quality, the orbiters drift lazily like gear fragments shed from a dissolving clock, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### CL Pulsating Music Note Orb
```
White music orb on solid black background, diffuse luminous sphere containing a gentle whole-note shape visible within, the note rendered in soft clock-hand strokes, the sphere surface has the quality of light through a cracked timepiece — shifting and dreamy, surrounded by a wide soft halo of scattered time, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Beam Textures

### CL Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with delicate crack patterns like a clock face shattering in slow motion, the cracks form soft organic branching shapes like fracture lines spreading across a timepiece's crystal, bright fracture lines fading to gossamer gray edges, the breaking is beautiful and inevitable — time itself fragmenting, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with gentle temporal turbulence, soft billowing forms like sand from a shattered hourglass drifting through time's wake, the movement is unhurried and meditative — luminous shapes merge and separate like the overlapping shadows of clock hands, bright wisps with diffuse soft edges, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Glow and Bloom

### CL Point Bloom
```
White pinpoint bloom on solid black background, tiny bright center with enormously wide soft bloom creating a diffuse temporal glow, the bloom so wide and gentle it resembles a softly glowing clock numeral dissolving rather than a point of light, like the last tick of a dying clock seen through tear-blurred eyes, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### CL Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with gentle concentric ripple structure like the face of a clock with its numerals dissolving outward in rings, the ripples barely perceptible — more feeling than form, everything soft-focus and time-worn, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Impact Effects

### CL Crumbling Shatter Burst
```
White shatter burst on solid black background, gentle fragmentation like a clock face bursting in slow motion, thin curved shards of timepiece glass drifting apart with soft dreamy trajectories, each shard luminous and translucent with faint numeral traces, tiny gear-tooth sparkle points at the fracture edges, the destruction impossibly gentle and beautiful, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### CL Radial Slash Star Impact
```
White radial star impact on solid black background, soft four-pointed impact star with wide clock-hand-shaped points, each point a broad diffuse wash of light like a clock hand dissolving rather than a sharp spike, the spaces between points filled with gentle hourglass sand mist, a dreamy impact that suggests the shattering of a moment, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### CL X-Shaped Impact Cross
```
White X cross impact on solid black background, two crossing arcs like clock hands crossing at midnight, each arc soft-edged and time-worn, the intersection has a warm diffuse bloom like the last light of a dying timepiece, temporal and tender, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Slash Arcs

### CL Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete gentle circle like a clock face viewed from above, the ring soft and time-worn with fading numeral-trace edges, opacity varies subtly around the circumference like the fading glow of each hour marker, tiny scattered glow points like gear fragments caught in a spin, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### CL Impressionist Sweep Arc Smear
```
White sword arc smear on solid black background, 90-degree gentle sweep rendered as a series of overlapping soft dabs — like a sequence of clock positions arranged in an arc, each position slightly different in size and brightness, the overall arc soft and luminous like a clock hand tracing through the hours in time-lapse, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Trails and Ribbons

### CL Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with gentle internal spiral of soft luminous forms like clock gears and numeral fragments caught in a lazy temporal whirlpool, the spiral arms are wide and diffuse — clock hand silhouettes twisting along the strip, bright soft center tapering to faint scattered gear-tooth points on the right, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Lightning

### CL Lightning Surge
```
White lightning bolt on solid black background, the gentlest possible electrical arc — a single soft branching path like cracks spreading across a clock's crystal face or the fracture lines of a shattered hourglass, the branches are smooth and organic rather than jagged, each branch fades to a soft point like the last grain of sand falling, luminous and dreamlike rather than violent, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Noise Textures

### CL Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, gentle overlapping pendulum-arc ripples like the rhythm of time passing across broken clock faces, the waves have long wavelengths and low amplitude creating soft luminous undulations, dreamy and meditative like the ticking of eternity, 256x256px seamless tile --ar 1:1 --style raw
```

### CL Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, soft swirling vortex of luminous temporal forms, the spiral arms wide and diffuse like the unwinding mainspring of a broken clock slowly releasing its stored time, gentle and unhurried rotation with soft-focus edges throughout, dreamy atmospheric quality, 256x256px seamless tile --ar 1:1 --style raw
```

### CL Unique Theme Noise — Shattered Timepiece Dapple Pattern
```
Grayscale seamless tileable noise texture on solid black background, pattern of scattered soft round and gear-shaped forms at varying sizes and brightnesses, like fragments of shattered clock faces and gears drifting through darkness, each fragment soft-edged and luminous with traces of clock numerals, the overall pattern gentle and dreamlike with no harsh elements, 256x256px seamless tile --ar 1:1 --style raw
```

---

## Clair de Lune — Smoke and Atmospheric

### CL Smoke Puff Cloud
```
White smoke puff on solid black background, impossibly soft diffuse cloud of luminous temporal mist, no hard edges anywhere — the entire form dissolves gently into the surrounding darkness, like the exhaled sigh of a clock winding down forever, the cloud shape slightly elongated horizontally suggesting the passage of a final moment, dreaming and ephemeral, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## Clair de Lune — Additional Particles

### CL Clock Face Shard
```
White clock face shard on solid black background, single broad curved fragment of a shattered clock face with gentle luminous quality, the shard wider at base narrowing to a cracked edge, surface has subtle impression of Roman numerals and hour marks — faintly embossed, edges translucent and fading gently, drifting through time's wake, game particle texture, 32x24px --ar 4:3 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 8: DIES IRAE — Day of Wrath
# ═══════════════════════════════════════════

> **Musical Soul:** The ancient hymn of judgment — apocalyptic fury, divine wrath, the end of all things
> **Visual Motifs:** Cracked earth, lava fissures, judgment scales, falling pillars, raining fire, shattered halos, broken angel wings, infernal gates, chains of damnation, scorched ground, ash fall
> **Emotional Core:** Fury, judgment, apocalyptic power, divine retribution
> **Colors (applied at runtime):** Blood red, dark crimson, ember orange

---

## Dies Irae — Beam Textures

### DI Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands like chains of damnation wrapping around each other, each strand composed of thick linked chain segments, the chains pull taut and strain against each other with visible tension, heavy and oppressive, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### DI Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with deep lava-fissure cracks, the cracks reveal blindingly bright molten core beneath a cooling dark crust, crack edges jagged and violent, lava oozes from the widest fissures, the destruction is geological and apocalyptic in scale, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### DI Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with apocalyptic judgment-day intensity, the energy builds to overwhelming destructive peak then briefly dims only to build again — relentless and wrathful, each peak crowned with a burst of ash-particle shapes, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### DI Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with volcanic eruption turbulence, thick heavy magma flows churning violently, explosive bursts punching through the surface, ash clouds billowing from eruption points, the violence of the earth's fury, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### DI Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of devastating pressure waves — each wave front thick and violent like a blast wave from an explosion, the waves compress the space ahead of them (visible compression lines) and leave destruction behind, Biblical trumpet blast quality, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Dies Irae — Glow and Bloom

### DI Glow Orb
```
White glow on solid black background, angry intense glow orb with a hard bright core that drops off steeply then has a wider smoldering ember-like outer glow, the center is almost blindingly bright suggesting extreme heat, the outer region has subtle flickering ember-speckle texture, wrathful, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### DI Lens Flare
```
White lens flare on solid black background, aggressive multi-element flare with a hard central disc, thick bold diffraction spikes radiating outward like burning sword blades, the elements are large and imposing rather than delicate, the flare commands and overwhelms, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### DI Star Flare
```
White star flare on solid black background, four-pointed star with thick heavy spikes that widen rather than taper — each spike a blunt burning wedge of pure destructive energy, the center is a hard bright square shape suggesting the geometry of judgment, wrathful and imposing, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Dies Irae — Impact Effects

### DI Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, seismic shockwave expanding outward, the rings thick and heavy like earthquake ground-cracks spreading concentrically, the ground between rings buckles and fragments, falling debris particles scattered between the wave fronts, apocalyptic ground pound, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### DI Crumbling Shatter Burst
```
White shatter burst on solid black background, cataclysmic destruction — massive irregular chunks of earth and stone blasting outward from ground zero, each chunk thick and heavy, some chunks crack with internal lava fissures, dense debris cloud of smaller fragments, ash particles filling the spaces, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### DI Power Effect Ring
```
White power ring on solid black background, thick heavy expanding ring with cracked-earth texture along its body, chunks of the ring crumble and break off as it expands, small lava drops drip from the breaking edges, the ring is destroying itself as it grows — self-immolating judgment, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### DI X-Shaped Impact Cross
```
White X cross on solid black background, two massive crossing slash marks carved deep into stone, each slash wide and violent with crumbling debris edges, the intersection creates a deep crater depression, lava-bright fissures visible at the deepest point, destructive and final, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Dies Irae — Slash Arcs

### DI Sword Arc Smear
```
White sword arc smear on solid black background, brutal 120-degree sweeping arc with jagged violent edges, the arc thick and heavy like a judgment blade, outer edge has crumbling stone-like debris breaking away, inner edge ragged and torn, the brightness HARSH and unforgiving, no soft edges — pure destructive force, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### DI Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete devastating spin slash like a ring of apocalyptic fire, the ring thick and uneven with heavy sections and cracked sections, large chunks break away from the ring's outer edge, internal fissure-cracks line the ring body, overwhelming and wrathful, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Dies Irae — Trails and Ribbons

### DI Basic Trail
```
White trail on solid black background, horizontal gradient with aggressive character, the bright end is blindingly intense, the fade-out happens through chunks breaking away and crumbling — not smooth fade but jagged dissolution, falling ash particles and ember points in the dissolving region, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### DI Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with aggressive seismic wave pattern, sharp peaked waves with steep slopes suggesting earthquake p-waves, each peak looks like it could crack stone, the wave is violent and relentless with no soft moments, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Dies Irae — Lightning

### DI Lightning Surge
```
White lightning bolt on solid black background, massive thick lightning bolt like divine judgment striking from above, the main bolt extremely wide and powerful with very few branches — focused wrathful strike rather than scattered discharge, the bolt terminates in a blast crater shape, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Dies Irae — Particles

### DI Music Note
```
White music note on solid black background, aggressive fortissimo quarter note rendered in bold heavy strokes, the note head large and dark like a branding mark, the stem thick as a pillar, small cracks visible in the note's body as if carved from stone under immense pressure, game particle texture, 32x32px --ar 1:1 --style raw
```

### DI Broken Halo Fragment
```
White broken halo on solid black background, curved fragment of a shattered holy halo, one end jagged where it broke, the other end tapers, the surface has ornate decorative patterning now cracked and damaged, a fallen angel's broken crown, game particle texture, 32x16px --ar 2:1 --style raw
```

### DI Ash Flake
```
White ash particle on solid black background, irregular thin flake of ash drifting downward, the flake curled and crumpled like a burned paper fragment, edges bright from residual heat, center darker and cooled, extremely lightweight and drifting, game particle texture, 16x16px --ar 1:1 --style raw
```

### DI Judgment Chain Link
```
White chain link on solid black background, single thick chain link — heavy wrought iron proportions, slightly open as if recently broken, the metal surface rough and pitted, one end shows the bright fracture point where it broke free, game particle texture, 24x16px --ar 3:2 --style raw
```

### DI Glyph
```
White glyph on solid black background, circular symbol with judgment scales at center, surrounded by a cracked and breaking ring, flame-tongue shapes at the cardinal points, the linework heavy and carved rather than drawn, suggests an ancient judgment seal, game particle texture, 32x32px --ar 1:1 --style raw
```

---

## Dies Irae — Projectiles

### DI Gyratory Orb
```
White spinning orb on solid black background, heavy dense sphere of compressed destruction, the surface cracked with lava-bright fissures revealing a molten interior, the sphere spins with grinding force — visible rotation through asymmetric crack patterns, chunks of cooled crust breaking away from the spinning surface, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### DI Orbiting Energy Sphere
```
White energy sphere on solid black background, central dense orb of apocalyptic energy with three orbiting chain-link fragments circling it like shrapnel caught in a gravity well, the main sphere smolders with internal fissures, each orbiting fragment trails ash and ember, heavy and threatening, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### DI Pulsating Music Note Orb
```
White music orb on solid black background, dense smoldering sphere containing a music note branded into its surface — the note burned and scarred into the sphere's cooling crust, the note glows with internal lava-bright heat, the sphere surface cracked and ashen around the brand, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Beam Textures

### DI Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of falling ash and burning debris carried on scorching winds, each particle a thick angular chunk trailing embers, heavy fragments tumble left to right with violent momentum, dense and oppressive — the air itself burns, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### DI Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam of violent seismic oscillations like a magnitude-ten earthquake captured in waveform, the peaks are sharp and jagged with steep violent slopes, each crest cracks at its tip like breaking stone, relentless destructive rhythm with no soft moments, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Glow and Bloom

### DI Point Bloom
```
White pinpoint bloom on solid black background, harsh intensely bright center point with aggressive steep falloff, the bloom is compact and angry — concentrated destructive light, the edges have a subtle flickering ember-speckle quality suggesting the light itself is burning, wrathful and focused, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### DI Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with cracked-earth texture radiating from center, the cracks form a web of fissures through the glow as if the light itself is damaging the surface it rests on, ember-bright nodes where major fissures intersect, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Impact Effects

### DI Radial Slash Star Impact
```
White radial star impact on solid black background, six-pointed impact star with thick brutal points like shattered stone pillars radiating from a cataclysmic strike, each point jagged and crumbling at its edges, deep lava-bright fissures running down the center of each point, the geometry of divine punishment, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Slash Arcs

### DI Flaming Sword Arc Smear
```
White sword arc smear on solid black background, wide violent 150-degree arc of pure destruction, the arc body thick and heavy with crumbling stone-like texture, the outer edge erupts with molten debris and heavy lava droplets, the inner edge is a raw bright wound of exposed energy, each section of the arc cracks and breaks as if carved through bedrock, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Trails and Ribbons

### DI Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with violent internal tornado of ash and ember, the spiral arms are thick churning columns of destruction wrapping around the strip axis, lava-bright fissures run along the spiral crests, the strip tapers from maximum devastation on left to crumbling ash dissolution on right, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Dies Irae — Noise Textures

### DI Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, violent seismic waveform pattern with sharp aggressive peaks and deep troughs, the waves crack at their crests revealing brighter sub-layers, the pattern suggests relentless destructive tremors — earthquake oscillograph recording the end of the world, 256x256px seamless tile --ar 1:1 --style raw
```

### DI Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, churning maelstrom of volcanic turbulence, thick heavy currents of magma-like energy colliding and grinding against each other, the vortex center is a white-hot point of maximum destruction, cracks and fissures radiate through the swirling pattern, 256x256px seamless tile --ar 1:1 --style raw
```

### DI Unique Theme Noise — Cracked Earth Pattern
```
Grayscale seamless tileable noise texture on solid black background, network of deep lava fissures cracking through scorched earth, the cracks form an interconnected web of bright hot lines against darker cooled surfaces, crack intersections are brighter where magma pools, the pattern suggests ground shattered by divine judgment, 256x256px seamless tile --ar 1:1 --style raw
```

---

## Dies Irae — Smoke and Atmospheric

### DI Smoke Puff Cloud
```
White smoke puff on solid black background, thick heavy plume of volcanic ash and choking smoke, extremely dense and opaque in the center, the cloud edges roil with violent convection currents, bright ember points embedded throughout like burning fragments carried skyward, the smoke is suffocating and wrathful — a pyroclastic cloud of judgment, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## Dies Irae — Additional Particles

### DI Falling Ember
```
White ember particle on solid black background, large glowing fragment of scorched material tumbling through air, one face bright and molten, the edges cooling to gray, the shape irregular and angular like a chunk of destroyed structure, tiny ash wisps trailing from the cooler edges, game particle texture, 24x24px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 9: NACHTMUSIK — A Little Night Music
# ═══════════════════════════════════════════

> **Musical Soul:** Mozart's Eine Kleine Nachtmusik — playful nocturnal elegance, twilight beauty, the joy of stargazing under moonlit skies, sophisticated night revelry
> **Visual Motifs:** Twilight skies, moonlit clouds, starry evenings, soft star points, comet trails, crescent moon with stars, nocturnal flowers (evening primrose, moonflower), dusk gradients, twilight silhouettes, moonbeams through clouds, evening mist
> **Emotional Core:** Nocturnal wonder, stellar beauty, sophisticated joy, twilight elegance
> **Colors (applied at runtime):** Deep indigo, starlight silver, cosmic blue

---

## Nachtmusik — Beam Textures

### NK Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling star-trails wrapping around each other like dual comet paths across a twilight sky, each strand dotted with tiny soft star points along its length, the strands are graceful and flowing — moonlit elegance, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of tiny star points drifting like a meteor shower across a twilight sky, each point soft and twinkling, some stars brighter and larger, occasional streak shapes like shooting stars, elegant nighttime wonder captured in a beam, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with elegant crescendo-decrescendo dynamic markings, the energy swells and recedes in precise musical-phrase patterning, peak moments adorned with small constellation-like point clusters, the rhythm is Mozart — structured graceful precise, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam of precise orchestral wave forms, clean measured oscillations with mathematically perfect spacing, each wave decorated with tiny star points at the crests and troughs, the wave pattern is elegant and structured — serenade tempo, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Glow and Bloom

### NK Glow Orb
```
White glow on solid black background, warm twilight-star glow — a bright star with soft clean Gaussian falloff, the center bright and twinkling, falloff wide and gentle like a star seen through evening mist, surrounded at far distance by scattered faint point companions, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### NK Lens Flare
```
White lens flare on solid black background, twilight-star flare — central bright disc with elegant thin diffraction spikes like starlight through evening atmosphere, subtle soft rings around the central point, the quality of a brilliant star seen on a clear moonlit night, beautiful and nocturnal, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### NK Star Flare
```
White star flare on solid black background, elegant four-pointed star with thin spikes of equal length, perfect symmetry like the brightest star on a moonlit evening, tiny soft halo ring around the center, clean and beautiful in quality, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Impact Effects

### NK Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding rings like twilight ripples spreading across a moonlit sky, graceful circular rings at even intervals, each ring thin and elegant, tiny star points scattered in the spaces between rings like a starry evening expanding, elegant ordered expansion, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### NK Power Effect Ring
```
White power ring on solid black background, clean elegant ring with constellation-point decorations at regular intervals around its circumference, thin constellation-line segments connecting some points, the ring is a starry sky in circular form, nocturnal and elegant, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Nachtmusik — Slash Arcs

### NK Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc rendered as a band of twilight starlit sky, the arc body contains scattered tiny star points creating a Milky Way-arc effect, bright central band with fainter scattered stars toward the edges, elegant moonlit sweep, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Nachtmusik — Trails and Ribbons

### NK Basic Trail
```
White trail on solid black background, horizontal gradient that dissolves into individual scattered star points, the bright end is a dense stellar cluster, the fade-out sees stars spacing farther apart like moving from a dense nebula to open space, comet-like quality, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### NK Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with elegant serenade-tempo wave, precise and measured oscillation with small constellation-cluster points used as decorative accents at wave peaks, sophisticated and musical, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Particles

### NK Music Note
```
White music note on solid black background, elegant eighth note rendered with refined serif-like styling as if from a classical sheet music engraving, precise and beautiful typography, the note surrounded by a scatter of two to three tiny star points like a mini constellation, game particle texture, 32x32px --ar 1:1 --style raw
```

### NK Constellation Fragment
```
White constellation on solid black background, three to five bright star points connected by thin straight lines forming a small constellation pattern like a fragment of Orion or Lyra, the star points vary in brightness (star magnitudes), lines extremely thin, game particle texture, 32x32px --ar 1:1 --style raw
```

### NK Comet
```
White comet particle on solid black background, small bright comet head with a fine trailing tail curving gently behind it, the head is a concentrated bright point, the tail fans out in a gentle arc getting dimmer toward the end, elegant and astronomical, game particle texture, 32x16px --ar 2:1 --style raw
```

### NK Nocturnal Flower
```
White flower on solid black background, small moonflower blossom with five petals open in a star-like arrangement, the petals luminous and softly glowing, the center has a small cluster of bright stamen points, a flower that only blooms at night, game particle texture, 32x32px --ar 1:1 --style raw
```

### NK Glyph
```
White glyph on solid black background, circular symbol incorporating a crescent moon and star design, star-point decorations at key angles, tiny musical notation fragments integrated into the nocturnal framework, elegant and beautiful — the intersection of music and moonlit night, game particle texture, 32x32px --ar 1:1 --style raw
```

---

## Nachtmusik — Projectiles

### NK Gyratory Orb
```
White spinning orb on solid black background, spherical projectile resembling a miniature twilight sky captured in a sphere, the surface shows soft starlit patterns like a moonlit celestial globe, bright equatorial band from rotation, tiny star points embedded in the surface like mapped constellations, elegant and nocturnal, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### NK Orbiting Energy Sphere
```
White energy sphere on solid black background, central bright star with three smaller companion stars orbiting in graceful elliptical paths like a multiple star system seen on a clear twilight evening, the main star dominant and bright, each orbiter smaller and at a different distance, thin soft orbital path traces barely visible, moonlit elegance, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### NK Pulsating Music Note Orb
```
White music orb on solid black background, glowing sphere containing a graceful music note shape rendered as a constellation — the note formed by bright star points connected with thin lines, the sphere has an elegant twilight quality like a musical note mapped onto the moonlit evening sky, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Beam Textures

### NK Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with graceful fracture patterns resembling the clean splitting of moonlit crystal, each crack forms along elegant planes, bright faceted surfaces at different angles catch starlight, the fracturing reveals deeper layers of embedded twilight-star lights, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of precise acoustic arcs flowing left to right like serenade melody lines visualized through an oscilloscope, each arc clean and mathematically defined, arcs overlap at musical intervals suggesting harmonic relationships, tiny star sparkle points at each wave intersection, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with elegant twilight-nebula turbulence, soft bright wisps folding and swirling like moonlit evening clouds, embedded soft star points of varying brightnesses scattered throughout, the turbulence suggests a starry twilight sky coming alive, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Glow and Bloom

### NK Point Bloom
```
White pinpoint bloom on solid black background, elegant twilight-star point with soft Gaussian bloom, the center bright and twinkling like the evening star at dusk, the falloff gentle and atmospheric, a soft halo suggestion at the bloom boundary — the quality of starlight through evening air, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### NK Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle embedded constellation patterns — tiny star points arranged in graceful figures within the overall soft glow, like a star field seen through moonlit evening mist, the bloom provides warmth while the star points add twinkling structure, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Impact Effects

### NK Crumbling Shatter Burst
```
White shatter burst on solid black background, elegant fragmentation like a moonlit crystal dome shattering to reveal the starry twilight beyond, each shard has softly polished surfaces catching starlight at different angles, the shards separate cleanly and gracefully, tiny star-point sparkles at fracture edges, sophisticated destruction, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### NK Radial Slash Star Impact
```
White radial star impact on solid black background, elegant eight-pointed star where each point aligns gracefully outward, the points taper to fine needle-sharp tips like starlight diffraction spikes, tiny constellation-dot clusters decorate the spaces between points, the geometry of a brilliant twilight star impact, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### NK X-Shaped Impact Cross
```
White X cross impact on solid black background, two gracefully crossing lines forming an X like moonbeams intersecting through twilight clouds, each line thin and elegant with soft brightness, the intersection has a bright twinkling star point, small star-dot markers along each line at regular intervals, nocturnal and elegant, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Slash Arcs

### NK Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete circular slash rendered as a ring of starlight, the ring body contains embedded star points of varying magnitudes like a band of the Milky Way bent into a circle, brighter concentration at one section suggesting the strike point, elegant and cosmic, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### NK Comet Trail Sword Arc Smear
```
White sword arc smear on solid black background, elegant 120-degree arc with the clean precision of a comet tail, the arc body bright and dense at the leading edge dissolving into scattered individual star points at the trailing end, tiny constellation-like clusters embedded in the arc body, sophisticated celestial sweep, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Trails and Ribbons

### NK Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with elegant internal spiral of twilight starlit streams, the spiral arms are graceful and flowing — moonlit stellar jets wrapping around a nocturnal axis, bright star points concentrated along the spiral crests, the strip tapers from dense bright star cluster on left to scattered distant twilight stars on right, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Lightning

### NK Lightning Surge
```
White lightning bolt on solid black background, elegant electrical arc with the graceful precision of a constellation line connecting stars across a twilight sky, the main bolt path clean and angular with deliberate direction changes, each direction change marked by a bright star-point node, branches thin and delicate like secondary constellation connections, nocturnal and beautiful, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Noise Textures

### NK Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, precise wave pattern composed of clean serenade-tempo oscillations with mathematical regularity, the waves are elegant and structured like sheet music notation spaced on a staff, bright wave crests with exact periodicity and controlled amplitude, sophisticated and orderly, 256x256px seamless tile --ar 1:1 --style raw
```

### NK Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, elegant twilight spiral pattern seen from above, soft spiral arms studded with twinkling star concentrations, the spiral geometry flowing and graceful, dark wisps between bright arms like moonlit evening clouds parting to reveal stars, nocturnal and magnificent, 256x256px seamless tile --ar 1:1 --style raw
```

### NK Unique Theme Noise — Twilight Star Field Pattern
```
Grayscale seamless tileable noise texture on solid black background, atmospheric star field with points of varying brightnesses distributed across the tile like a twilight sky, brighter stars have subtle four-point diffraction spikes, dimmer stars are soft glowing dots, occasional denser cluster regions suggesting star clusters seen through evening mist, the warmth of a moonlit sky, 256x256px seamless tile --ar 1:1 --style raw
```

---

## Nachtmusik — Smoke and Atmospheric

### NK Smoke Puff Cloud
```
White smoke puff on solid black background, elegant wisp of moonlit evening mist, the cloud has delicate internal structure with soft filaments and density variations like a twilight cloud catching the last light of dusk, subtle bright star-point nodes within the cloud, the cloud edges dissolve into faint wisps of evening atmosphere, nocturnal and serene rather than mundane, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## Nachtmusik — Additional Particles

### NK Dancing Sparkle
```
White sparkle particle on solid black background, small elegant starburst with six thin rays of equal length surrounding a bright center point, like a single star twinkling on a moonlit evening or a moment of twilight sparkle, clean and radiant, the rays thin and elegant, game particle texture, 16x16px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 10: ODE TO JOY — Universal Brotherhood
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Ninth — the triumph of the human spirit, universal joy, the brotherhood of all humanity expressed through nature's beauty
> **Visual Motifs:** Blooming roses, scattered petals, unfurling flower buds, thorned vines, rose gardens in full bloom, pollen drifting on warm air, intertwined stems, fallen petals on the ground, flower crowns, pressed flowers, botanical illustrations, blossom cascades
> **Emotional Core:** Joy, celebration, triumph of spirit, radiant warmth
> **Colors (applied at runtime):** Warm gold, radiant amber, jubilant light

---

## Ode to Joy — Beam Textures

### OJ Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands like flowering vines twisting together in graceful spirals, the strands are organic and flowing like climbing rose stems, each twist adorned with tiny petal shapes unfurling along the edges, tiny pollen points scattered along the vine paths, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting botanical forms — rose petals, tiny flower buds, and small leaf shapes tumbling gracefully left to right, the shapes are varied and delicate creating a gentle petal procession feeling, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam building to a glorious full-bloom climax, the energy rises in ascending waves like flowers opening one by one, each wave crest larger and more brilliant than the last — buds becoming full roses, the final peak bursts into radiating petals and pollen, the bloom is OVERWHELMING, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of flourishing waves flowing left to right like a garden in full bloom visualized as sound waves, the arcs are full and round suggesting lush petal curves, waves overlap and create bright constructive interference where blossoms converge together, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam of layered botanical harmonics — multiple wave frequencies layered together to create rich organic waveform, the combined pattern has a warm rounded character like overlapping rose petals viewed in profile, occasional bright peaks where all harmonics align in glorious full bloom, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Glow and Bloom

### OJ Glow Orb
```
White glow on solid black background, warm expansive bloom with generous Gaussian falloff, the center bright and inviting like the heart of a rose, the falloff wide and embracing like petals opening outward — this light welcomes rather than blinds, the warmth feels like golden sunlight on a garden in full bloom, generous and giving, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### OJ Lens Flare
```
White lens flare on solid black background, botanical multi-element flare with a central bright disc like a flower's center, petals of soft glow arranged around it, and thin stamens that radiate outward in a rose-bloom pattern, the flare feels like a flower opening at dawn — petals of first light, blooming and new, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### OJ Star Flare
```
White star flare on solid black background, six-pointed flare with wide generous petal-shaped spikes that radiate warmth, the points softer and rounder than sharp — like flower petals in a radial arrangement, the center large and inviting, between the major petals smaller secondary wisps create a full blossom pattern, joyful and radiant, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Impact Effects

### OJ Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding blossom-burst rings, each ring erupts with small petal shapes like a rose bursting into bloom in accelerated time, the rings expand with flowering energy, between rings tiny petal-like particle shapes scatter like pollen, the overall impression is a triumphant floral detonation, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ Crumbling Shatter Burst
```
White burst on solid black background, not destruction but ERUPTION of blossoming — fragments burst outward like a flower exploding into bloom, each fragment a different petal shape (broad rose petals, narrow lily petals, tiny bud fragments), the petals scatter joyfully rather than violently, bright and botanical, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ Power Effect Ring
```
White power ring on solid black background, expanding ring wreathed in tiny botanical elements — small rose petals, curling vine tendrils, and tiny bud shapes decorate the ring's circumference, the ring itself golden-warm in brightness, a flowering wreath of light, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ X-Shaped Impact Cross
```
White X cross on solid black background, two crossing vine stems that bloom with petals at their intersection — the cross is a floral pattern, rose-blossom decoration at the center where the stems meet, each stem is adorned with small thorn-point highlights along its length, triumphant and botanical, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Ode to Joy — Slash Arcs

### OJ Sword Arc Smear
```
White sword arc on solid black background, flourishing 120-degree sweeping arc with blooming energy, the arc body shimmers with tiny embedded petal shapes, the leading edge erupts with small rose-petal forms cascading outward, the trailing edge dissolves into scattered pollen dust, the swing is triumphant and lush, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### OJ Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete triumphant spin with flowering-wreath quality, the circle is a bright bold ring with blossom-bright points evenly spaced like roses blooming along the ring's path, tiny petals and pollen fill the interior, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Ode to Joy — Trails and Ribbons

### OJ Basic Trail
```
White trail on solid black background, horizontal gradient that dissolves into scattered petal drift, the bright end overflows with tiny rose-blossom points, the fade-out scatters into falling petal shapes and diminishing pollen points, the overall feel is a garden trail of scattered blossoms, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### OJ Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with joyful blooming-wave pattern, each wave crest adorned with a tiny rose-bud shape unfurling, the rhythm is allegro — quick and energetic, the wave pattern rises and leaps with the exuberance of flowers opening in time-lapse, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Lightning

### OJ Lightning Surge
```
White lightning bolt on solid black background, bright triumphant bolt splitting into MANY branches — not threatening but botanical, like a vine rapidly growing and branching into multiple flowering tendrils, each branch ends in a small bright blossom-burst, the bolt reaches upward suggesting growth and flourishing rather than destructive strike, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Particles

### OJ Music Note
```
White music note on solid black background, joyful eighth note rendered with bold confident strokes, the note head round and full like a rose bud, the flag upturned and enthusiastic like an unfurling petal, the entire note slightly tilted as if dancing, surrounded by a scatter of tiny pollen dots like drifting garden dust, game particle texture, 32x32px --ar 1:1 --style raw
```

### OJ Rose Petal
```
White rose petal on solid black background, single broad curved petal with gentle luminous quality, slightly curled and cupped like a freshly fallen rose petal, one face bright with delicate vein-line texture, the curl creates a dynamic shape suggesting mid-air drifting, soft edges, game particle texture, 16x8px --ar 2:1 --style raw
```

### OJ Blossom Sparkle
```
White blossom sparkle on solid black background, bright central point with five soft petal-shaped radiating forms and tiny pollen dots between them creating a flower-burst shape, like a single rose opening in fast-forward captured at peak bloom, warm and botanical, game particle texture, 16x16px --ar 1:1 --style raw
```

### OJ Vine Tendril
```
White vine tendril on solid black background, short curling vine stem, the tendril twists and curls gracefully in space showing organic growth, wider at one end where it joins the main stem, tapering to a delicate spiraling tip with a tiny bud, dynamic and botanical, game particle texture, 32x16px --ar 2:1 --style raw
```

### OJ Thorn Fragment
```
White thorn fragment on solid black background, single curved thorn shape like one thorn broken from a rose stem, wider at the base tapering to a sharp fine point, the base has a tiny arc-shape where it connected to the stem, warm and organic, game particle texture, 32x8px --ar 1:1 --style raw
```

### OJ Glyph
```
White glyph on solid black background, circular symbol with a blooming rose design at center surrounded by musical notation fragments, the rose has layered petals spiraling outward, tiny leaf shapes flank the sides, the ring decorated with small thorn-dot patterns, joyful and botanical, game particle texture, 32x32px --ar 1:1 --style raw
```

---

## Ode to Joy — Projectiles

### OJ Gyratory Orb
```
White spinning orb on solid black background, spherical projectile like a flower bud about to bloom, the surface decorated with tiny embossed botanical shapes — petals, leaves, and vine-curl swirls, the orb spins with flourishing energy, a wide warm bloom surrounds it like golden pollen haze, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### OJ Orbiting Energy Sphere
```
White energy sphere on solid black background, bright central orb of blooming energy with three orbiting petal-cluster wisps circling gracefully, each orbiting wisp shaped like a tiny flower mid-bloom, the main sphere radiates warm generous light, the orbiters trail scattered pollen-like particles, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### OJ Pulsating Music Note Orb
```
White music orb on solid black background, radiant sphere containing a proud joyful eighth note visible within, the note rendered in bold botanical strokes, the sphere itself shimmers with embedded tiny petal points like pollen dust, a warm flourishing glow suffuses the entire form, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Additional Beam Textures

### OJ Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with botanical fracture patterns like a flower bud’s shell cracking open to reveal the bloom within, the cracks form organic branching shapes like leaf veins, each fragment is a tiny petal or leaf piece, the breaking is joyful and generative — new life emerging, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with effervescent pollen-drift turbulence, countless tiny bright spheres of pollen rising and swirling within the beam body, the particles are joyful and organic, larger clusters burst into smaller petal fragments, the energy is fizzing with barely contained botanical vitality, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Additional Glow and Bloom

### OJ Point Bloom
```
White pinpoint bloom on solid black background, warm bright center with generous broad bloom, the falloff is wide and welcoming — this light invites and embraces, subtle petal-point texture embedded in the mid-range bloom like pollen caught in a shaft of garden sunlight, joyful and radiant, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### OJ Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle petal-ray patterning radiating from center, the rays are soft and wide creating a warm rose-opening quality, between the rays the glow is slightly less intense creating a gentle pulsing flower-like pattern, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Ode to Joy — Additional Impact Effects

### OJ Radial Slash Star Impact
```
White radial star impact on solid black background, flourishing six-pointed impact star where each point blooms with petal-dot decorations like a rose bursting open from the center, the points are wide and generous rather than sharp and threatening, tiny petal-like shapes scattered between and around the points, pure botanical joy, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Ode to Joy — Additional Slash Arcs

### OJ Flaming Sword Arc Smear
```
White sword arc smear on solid black background, triumphant 120-degree arc erupting with blooming energy, the arc body thick and confident with rose-petal decorations along both edges, tiny petal shapes and thorn fragments scatter from the leading edge, the trailing edge dissolves into a shower of diminishing pollen points, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Ode to Joy — Additional Trails and Ribbons

### OJ Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with joyful internal spiral of botanical elements, the spiral arms are made of cascading petal shapes and tiny bud forms twisting like a flowering vine along the strip, bright and dense on the left tapering to scattered drifting petals on the right, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Noise Textures

### OJ Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, flourishing wave pattern of overlapping botanical harmonics, multiple organic wave frequencies layered to create warm rich combined waveforms, the pattern has a rounded generous character with bright constructive-interference peaks where petal curves align in glorious full bloom, 256x256px seamless tile --ar 1:1 --style raw
```

### OJ Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, radiant rose-bloom vortex with warm generous spiral arms, the arms are wide and open like unfurling petals rather than tight and threatening, bright organic light quality radiates from the center, the pattern feels like a golden rose opening joyfully viewed from above, 256x256px seamless tile --ar 1:1 --style raw
```

### OJ Unique Theme Noise — Petal Scatter Pattern
```
Grayscale seamless tileable noise texture on solid black background, scattered botanical elements — rose petal shapes, tiny leaf silhouettes, and bud-like fragments distributed across the tile at varying sizes and brightnesses, the overall pattern suggests a garden floor covered in freshly fallen petals with light catching their curved surfaces, joyful and organic, 256x256px seamless tile --ar 1:1 --style raw
```

---

## Ode to Joy — Smoke and Atmospheric

### OJ Smoke Puff Cloud
```
White smoke puff on solid black background, light buoyant cloud of golden pollen haze like the aftermath of a rose bush shaking in a warm breeze, the cloud is airy and open rather than dense, tiny bright pollen points embedded throughout like floating garden dust, the cloud shape lifts upward with gentle botanical buoyancy, warm and organic, game VFX texture, 128x128px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# SHARED UTILITY ASSETS (Theme-Neutral)
# ═══════════════════════════════════════════

> The following assets are **generic utilities** that serve all themes equally. They do NOT need theme-specific versions as they provide pure data/shapes that shaders interpret with per-theme parameters.

## Masks and Shapes (No themed versions needed)
- `HardCircleMask.png` — Pure circular alpha mask, 1.0 inside circle, 0.0 outside
- `SmallHardCircleMask.png` — Smaller circular alpha mask
- `SoftCircle.png` — Circle with soft Gaussian edge falloff
- `SquareMask.png` — Square alpha mask
- `VerticalEllipse.png` — Vertically elongated elliptical mask
- `WideSoftEllipse.png` — Horizontally elongated soft elliptical mask

**Reasoning:** Masks define WHERE effects appear, not HOW they look. The same circle mask works for every theme — the shader applies the theme's visual character.

## Basic Noise Textures (No themed versions needed)
- `PerlinNoise.png` — General-purpose smooth noise
- `SimplexNoise.png` — Simplex noise variant
- `TileableFBMNoise.png` — Fractal Brownian motion turbulence
- `TileableMarbleNoise.png` — Flowing marble veins
- `NoiseSmoke.png` — Soft billowing smoke
- `SoftCircularCaustics.png` — Underwater caustic light
- `UVDistortionMap.png` — General UV distortion data
- `VoronoiNoise.png` / `VornoiEdgeNoise.png` / `VoronoiCellNoise.png` — Cellular noise variants

**Reasoning:** Noise textures are sampled by shaders as mathematical data. The same Perlin noise works for fire, water, cosmic, or any other energy — the shader context determines appearance.

## Color Gradients (Already per-theme — no new prompts needed)
All 10 theme gradient LUTs already exist:
- `ClairDeLuneGradientLUTandRAMP.png`
- `DiesIraeGradientLUTandRAMP.png`
- `EnigmaGradientLUTandRAMP.png`
- `EroicaGradientLUTandRAMP.png`
- `EroicaGradientPALELUTandRAMP.png`
- `FateGradientLUTandRAMP.png`
- `LaCampanellaGradientLUTandRAMP.png`
- `MoonlightSonataGradientLUTandRAMP.png`
- `NachtmusikGradientLUTandRAMP.png`
- `OdeToJoyGradientLUTandRAMP.png`
- `SwanLakeGradient.png`

## Solid Utility Textures (No themed versions needed)
- `SolidWhiteLine.png` — Pure white line for shader use

---
---

# ═══════════════════════════════════════════
# APPENDIX: COMPLETE ASSET COUNT BY THEME
# ═══════════════════════════════════════════

| Category | LC | ER | SL | MS | EN | FA | CL | DI | NK | OJ |
|----------|----|----|----|----|----|----|----|----|----|----|
| Beam Textures | 14 | 13 | 11 | 7 | 7 | 5 | 7 | 7 | 7 | 7 |
| Glow/Bloom | 7 | 6 | 4 | 4 | 3 | 3 | 5 | 5 | 5 | 5 |
| Impact Effects | 7 | 5 | 4 | 3 | 4 | 3 | 5 | 5 | 5 | 5 |
| Projectiles | 6 | 3 | — | — | — | — | 3 | 3 | 3 | 3 |
| Slash Arcs | 4 | 3 | 2 | 2 | 2 | 2 | 3 | 3 | 3 | 3 |
| Trails/Ribbons | 3 | 3 | 2 | 2 | 2 | 2 | 3 | 3 | 3 | 3 |
| Lightning | 1 | 1 | — | — | 1 | — | 1 | 1 | 1 | 1 |
| Noise (themed) | 3 | — | — | — | — | — | 3 | 3 | 3 | 3 |
| Smoke/Atmo | 1 | — | — | — | — | — | 1 | 1 | 1 | 1 |
| Particles | 9 | 5 | 4 | 5 | 5 | 4 | 6 | 6 | 6 | 6 |
| **TOTAL** | **55** | **39** | **27** | **23** | **24** | **19** | **37** | **37** | **37** | **37** |

> **Grand Total: ~348 unique themed asset prompts across 10 themes**

---

# ═══════════════════════════════════════════
# GENERATION TIPS
# ═══════════════════════════════════════════

1. **Always use `--style raw`** for VFX textures. This prevents Midjourney from adding artistic interpretation that breaks shader compatibility.

2. **Verify solid black background.** After generation, check that the background is truly #000000. Use a photo editor to clean up if needed.

3. **Grayscale check.** Generated images should be desaturated to pure grayscale before use. Any color artifacts from generation should be removed.

4. **Power-of-two resize.** Midjourney may not output exact power-of-two dimensions. Resize to the target dimension (64, 128, 256, 512) in a photo editor.

5. **Tileability for beams/trails.** After generation, check that horizontal beam and trail strips tile seamlessly by placing two copies side by side. Fix seams manually if needed.

6. **Batch generation.** For efficiency, generate all assets for one theme at a time, then move to the next theme.

7. **File naming:** Use `<ThemeAbbreviation>_<AssetCategory>_<AssetName>.png` format. Example: `LC_Beam_BraidedEnergyHelix.png`, `ER_Particle_SakuraPetal.png`

8. **Placement:** After generation, place files per the copilot-instructions asset guide:
   - VFX textures → `Assets/VFX Asset Library/<Category>/`
   - Particles → `Assets/Particles Asset Library/`
   - Or in per-weapon folders: `Assets/<Theme>/<Weapon>/<Category>/`
