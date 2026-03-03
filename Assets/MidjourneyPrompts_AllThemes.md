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
> **Visual Motifs:** Bell shapes, bell curves, ringing vibration waves, cascading flame tongues, candle-like flickers, hammered piano strings, molten dripping metal, forge heat shimmer
> **Emotional Core:** Passion, intensity, burning brilliance, virtuosic speed
> **Colors (applied at runtime):** Black smoke, orange flames, gold highlights

---

## La Campanella — Beam Textures

### LC Braided Energy Helix Beam
```
White intertwined double-helix beam segment on solid black background, two spiraling strands of molten energy twisted around each other like bell-rope fibers, bright white glowing core with softer gray outer wisps, hammered metal texture along the strand surfaces, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Cracking Energy Fissure Beam
```
White cracking fissure beam segment on solid black background, horizontal energy beam with fracture lines radiating outward like a cracked church bell, jagged bright cracks forming mosaic pattern through the beam body, hot white core with gray stress fractures branching to edges, molten seams between fragments, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Energy Motion Beam
```
White flowing energy beam segment on solid black background, horizontal stream of cascading energy droplets falling like rapid piano arpeggios, each droplet elongated and flame-shaped, flowing left to right in rhythmic cascading pattern, bright core drops with soft gray motion trails, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Energy Surge Beam
```
White surging energy beam on solid black background, powerful horizontal energy surge with bell-curve intensity peaks repeating along its length, each peak like a struck bell resonance, bright white crescendo peaks fading to gray troughs between pulses, heat shimmer distortion lines along edges, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Horizontal Beam Streak
```
White horizontal beam streak on solid black background, clean linear energy column with flickering candle-flame edges dancing along both sides, the central beam razor-sharp and intensely bright, edge flames asymmetric and wild like forge fire, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### LC Infernal Beam Ring
```
White ring-shaped beam cross-section on solid black background, circular beam rendered as a ring of molten bell-metal with dripping heated rivulets trailing downward, ring surface textured with hammered metal dimples, bright white hot spots where the metal is freshest, darker gray cooling sections, game VFX texture, 128x128px --ar 1:1 --style raw
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
White turbulent energy core on solid black background, horizontal beam core with roiling forge-fire turbulence, churning molten metal texture with bright white hot spots surrounded by darker swirling currents, small bubbling eruptions along the surface like molten gold in a crucible, intense and chaotic internal motion, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### LC Thin Linear Glow
```
White thin linear glow on solid black background, extremely fine horizontal line of intense brightness like a single vibrating piano wire caught in light, razor-thin bright core with delicate soft bloom extending above and below, subtle standing wave amplitude modulation along the wire length, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

### LC Beam Lens Flare Explosion Impact
```
White radial burst on solid black background, explosive bell-strike impact flare with energy radiating outward in concentric rings like sound waves from a struck bell, bright white center with radiating streaks of decreasing intensity, ring-shaped pressure waves expanding outward, sparks flying in spiral patterns, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Burst Fade Streaks Lens Flare
```
White radial streaked lens flare on solid black background, central bright point with long fading streaks radiating outward like hammered sparks flying from an anvil, streaks vary in length and intensity creating asymmetric forge-spark burst, soft gray glow envelope around the cluster, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Burst Heavy Streaks Lens Flare
```
White heavy radial burst on solid black background, massive forge-hammer impact flare with thick bold streaks radiating from center like molten metal spattering from a heavy blow, each streak tapers from thick bright base to thin gray tip, uneven distribution suggesting violent directional force, bright hot core with intense radial energy, game VFX texture, 128x128px --ar 1:1 --style raw
```

### LC 8-Point Starburst Flare (Projectile)
```
White 8-pointed starburst on solid black background, brilliant flare shaped like a ringing bell's acoustic radiation pattern, eight sharp points radiating symmetrically with bright tips, each point slightly flame-shaped with flickering edges, narrow bright core with larger soft glow between the points, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## La Campanella — Glow and Bloom

### LC Glow Orb
```
White soft circular glow on solid black background, perfectly round orb of light with forge-like heat falloff, extremely bright pinpoint center rapidly falling off to medium gray then black, the falloff curve shaped like a bell curve with a sharp bright peak, subtle heat shimmer texture in the mid-range, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Lens Flare
```
White complex lens flare on solid black background, multi-element flare arrangement resembling a bell seen from above, central bright disc with a ring of smaller flare elements arranged in a circle around it, thin anamorphic streak cutting horizontally through center, each element has soft feathered edges, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Point Bloom
```
White pinpoint bloom on solid black background, tiny intensely bright center point with wide soft Gaussian bloom extending outward, the bloom envelope shaped like a flame tip — slightly taller than wide, elongated upward like a candle flame, gentle flickering edge irregularities, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### LC Soft Glow
```
White soft glow on solid black background, large diffuse luminous cloud with gentle Gaussian falloff, slightly uneven edges suggesting heat haze or warm air convection, the glow center offset slightly upward like rising heat, extremely smooth and atmospheric, game VFX bloom texture, 256x256px --ar 1:1 --style raw
```

### LC Soft Glow Bright Center
```
White bloom with bright center on solid black background, large soft glow with an extremely intense white hotspot in the center like molten metal at forging temperature, the bright core takes up about 20% of the radius then falls off rapidly to a wide soft gray halo, game VFX bloom texture, 256x256px --ar 1:1 --style raw
```

### LC Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle concentric ring structure like acoustic resonance rings expanding from a bell strike, each ring slightly brighter than the smooth falloff between them, creating a gentle rippled bloom effect rather than smooth Gaussian, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### LC Star Flare
```
White star-shaped flare on solid black background, four-pointed star flare with elongated vertical and horizontal spikes like forge sparks, each spike has a bright base that tapers to a fine point, the horizontal spikes slightly longer than vertical suggesting hammer-strike direction, bright center core where all spikes meet, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## La Campanella — Impact Effects

### LC Harmonic Resonance Wave Impact
```
White resonance wave impact on solid black background, circular expanding shockwave made of multiple concentric bell-vibration rings, each ring thin and bright at its leading edge fading to gray behind, rings spaced at harmonic intervals — closer together near center, wider apart at edges, interference patterns where waves overlap create brighter nodes, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Crumbling Shatter Burst
```
White shatter burst on solid black background, explosive fragmentation pattern like a shattered bell, large angular shards radiating outward from center, each shard bright at its inner edge and darker at outer edge, cracks and fracture lines between shards, some smaller debris particles scattered between the major fragments, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Impact Ellipse
```
White impact ellipse on solid black background, horizontally elongated elliptical impact burst like a forge hammer striking an anvil, the ellipse brighter and thicker at center thinning toward the pointed ends, splash-like ejection streaks along the top edge suggesting molten metal spray, game VFX impact texture, 128x64px --ar 2:1 --style raw
```

### LC Power Effect Ring
```
White power ring on solid black background, bold expanding ring of energy with flame-tongue protrusions along the outer edge, the ring itself thick and bright, outer edge decorated with small upward-licking flame shapes evenly spaced, inner edge smooth, ring slightly thicker at cardinal points suggesting directional force, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Radial Slash Star Impact
```
White radial slash star on solid black background, six-pointed impact star with each point shaped like a curved flame, points alternate between long and short creating dynamic asymmetry, bright white center hub with streaks extending to each flame-point tip, overall shape suggests a spinning fire wheel, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC X-Shaped Impact Cross
```
White X-shaped cross impact on solid black background, two crossing diagonal slash marks forming an X, each slash line tapers from thick bright center to thin gray tips, the intersection point extremely bright with a small explosion burst, edges of each slash have flickering flame-like irregularities, small forge sparks scattered around the intersection, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### LC Beam Lens Flare Explosion Impact
```
White explosion impact on solid black background, massive radial detonation burst like a great bell being struck with overwhelming force, central blindingly bright core with thick radiating energy bands, concentric pressure waves visible as rings within the burst, outermost ring has flame-like tongues licking outward, debris sparks flying in curved trajectories, game VFX impact texture, 256x256px --ar 1:1 --style raw
```

---

## La Campanella — Projectiles

### LC 4-Point Star Shining Projectile
```
White four-pointed star projectile on solid black background, compact shining star with four sharp points, each point shaped like a candle flame — wider at base tapering to sharp tip, bright core where points meet with softer glow envelope, subtle heat shimmer rings around the star, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Bright Star Projectile 1
```
White bright star projectile on solid black background, intense burning star shape with irregular flame-edge points, like a chunk of molten bell-metal flying through air, bright white-hot core with ragged glowing edges, small trailing drip of molten material hanging below, surrounded by heat distortion ring, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Bright Star Projectile 2
```
White blazing projectile on solid black background, rapidly spinning flame wheel projectile, four curved flame arms spiraling from center creating pinwheel rotation, each arm wider at outer tip and narrow at center junction, overall shape suggests a spinning Catherine wheel firework, bright center with gradient gray arms, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Gyratory Orb
```
White spinning orb projectile on solid black background, spherical orb with visible internal rotation — swirling molten patterns visible through a translucent shell, the surface has hammered-metal dimple texture, bright equatorial band where spin is fastest, slightly compressed poles, a small ring orbiting the equator like a bell's acoustic vibration ring, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### LC Orbiting Energy Sphere
```
White energy sphere on solid black background, large central orb with three smaller orbiting flame wisps circling it, the main sphere has a forge-fire core with softer outer glow, each orbiting wisp is teardrop-shaped and trails a small gray wake, orbiting wisps spaced at even 120-degree intervals, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### LC Pulsating Music Note Orb
```
White music-themed orb on solid black background, circular orb containing a visible musical note shape within — a quarter note silhouette burning bright inside a sphere of flame, the note appears to be made of molten metal, sphere edges flicker with candle-flame irregularity, small resonance ripples emanating from the orb surface, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## La Campanella — Slash Arcs

### LC Flaming Sword Arc Smear
```
White sword arc smear on solid black background, sweeping 120-degree crescent blade arc with aggressive flame-tongue edges, the arc is thick and bright at the leading edge thinning toward the trailing edge, flame licks extend outward from the outer curve irregularly, inner curve smooth and precise, hot spots brightest at the arc's midpoint, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Flaming Sword Arc Smear 2
```
White sword arc smear variant on solid black background, wider 150-degree sweeping arc with double-layered structure, outer layer thin and wispy with flame filaments, inner layer thick bright concentrated energy core, the two layers separated by a narrow gap of darkness creating depth, flame sparks scattering from the leading edge, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Full Circle Sword Arc Slash
```
White full 360-degree sword arc on solid black background, complete circular slash ring with forge-hammer intensity, ring varies in thickness — thickest and brightest at the 3 o'clock position (strike point) thinning toward the opposite side, flame eruptions at the thickest point, hammered-metal texture along the ring body, small forge sparks ejecting outward from the strike point, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### LC Sword Arc Smear
```
White clean sword arc on solid black background, precise 90-degree arc slash with bell-curve brightness profile — gradually brightening from start, peaking at center, and fading toward the end, the arc edge has subtle vibration wobble like a struck bell resonating, thin bright core with moderate soft glow, overall clean and sharp but with that characteristic ring, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## La Campanella — Trails and Ribbons

### LC Basic Trail
```
White basic trail strip on solid black background, horizontal gradient strip bright on left fading to black on right, the bright end has flickering candle-flame edge irregularity, the fade-out is not smooth but has ember-like bright speckles scattered in the darker region like dying forge embers, overall a flame-trail character, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### LC Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with visible standing wave pattern — fixed bright nodes at regular intervals connected by oscillating wave curves, each node point extra bright like a struck string's vibration node, the wave amplitude larger between nodes with bell-curve falloff, subtle harmonic overtone waves visible as fainter secondary oscillation, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### LC Spiraling Vortex Energy Strip
```
White spiraling energy strip on solid black background, horizontal strip with internal helical spiral pattern that twists along the strip length, the spiral made of flame-tongue shapes wrapping around a central axis, bright spiral crests with darker troughs, overall the strip tapers from bright/wide on left to narrow/dark on right, forge-fire intensity, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## La Campanella — Lightning

### LC Lightning Surge
```
White jagged lightning bolt on solid black background, horizontal electrical arc with forge-spark character, the main bolt path thick and angular with sharp 90-degree direction changes, bright white core with thinner secondary forking branches, each branch terminates in a tiny bright spark point, the bolt shape suggests hammered metal shattering — angular and aggressive rather than smooth, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## La Campanella — Noise Textures

### LC Musical Wave Pattern
```
Grayscale seamless tileable noise texture on solid black background, wave pattern composed of rapid high-frequency oscillations like vibrating piano strings at fortissimo, dense tightly-packed waves with occasional resonance peaks where waves constructively interfere, the pattern suggests virtuosic speed and intensity, sharp wave crests with rapid dropoffs, 256x256px seamless tile --ar 1:1 --style raw
```

### LC Cosmic Energy Vortex
```
Grayscale seamless tileable noise texture on solid black background, swirling vortex pattern with forge-fire character, molten metal currents spiraling inward toward a bright center, the spiral arms have rough hammered texture rather than smooth curves, turbulent eddies along the spiral edges, intense bright concentration at the center suggesting extreme heat, 256x256px seamless tile --ar 1:1 --style raw
```

### LC Unique Theme Noise — Bell Resonance Pattern
```
Grayscale seamless tileable noise texture on solid black background, concentric expanding circles like acoustic waves radiating from a struck bell, circles vary in brightness with harmonic relationships, some circles wider and brighter (fundamental frequency) some thinner and fainter (overtones), interference patterns where different resonance circles overlap create complex moire-like zones, 256x256px seamless tile --ar 1:1 --style raw
```

---

## La Campanella — Smoke and Atmospheric

### LC Smoke Puff Cloud
```
White smoke puff on solid black background, billowing cloud of forge smoke with heavy dense character, thick opaque center with wispy trailing tendrils, small bright ember points scattered within the smoke body, the cloud shape slightly elongated vertically suggesting rising heat convection, textured with soot-heavy density variations, game VFX texture, 128x128px --ar 1:1 --style raw
```

---

## La Campanella — Particles (Theme-Specific Versions)

### LC Music Note
```
White music note symbol on solid black background, standard eighth note shape rendered as if forged from molten metal, the note head round and bright like a bell shape, the stem has subtle hammered texture, a tiny flame wisp curling from the flag, sharp clean edges with very subtle heat glow bloom around the silhouette, game particle texture, 32x32px --ar 1:1 --style raw
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
White star particle on solid black background, four-pointed star with each point shaped like a small candle flame, sharp elongated points with bright tips, the center where points meet is the brightest, subtle asymmetry in point lengths suggesting flickering, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Glyph
```
White arcane glyph on solid black background, circular symbol incorporating a bell silhouette at its center surrounded by musical notation fragments, small flame decorations at the cardinal points, thin precise line work, the overall shape suggests a musical protection ward or forge sigil, game particle texture, 32x32px --ar 1:1 --style raw
```

### LC Energy Flare
```
White energy flare burst on solid black background, radial burst of forge sparks exploding outward from center, bright intensely white core with elongated spark streaks radiating at varied angles, some sparks curve slightly suggesting they were thrown from a spinning wheel, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Sword Arc Particle
```
White sword arc particle on solid black background, 90-degree crescent arc with flame-tongue edges on the outer curve, the arc bright and thick transitioning from maximum brightness at center to fading at both ends, inner edge clean and sharp, outer edge wild with fire licks, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Halo Ring
```
White glowing halo on solid black background, circular ring of light with flame-like flickering brightness variations around its circumference, the ring width varies — thicker at top and bottom (bell vibration nodes), thinner at sides, soft bloom around the ring, game particle texture, 64x64px --ar 1:1 --style raw
```

### LC Flame Impact Explosion
```
White flame explosion on solid black background, violent radial explosion with thick flame tongues shooting outward in all directions, bright white-hot center with dense overlapping flame shapes, outer edges have individual flame wisps breaking free, overall shape slightly asymmetric suggesting directional force, game particle texture, 64x64px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 2: EROICA — The Hero's Symphony
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Third Symphony — the hero's journey, triumph through adversity, noble sacrifice
> **Visual Motifs:** Sakura petals, rising embers ascending to heaven, golden laurel wreaths, sword-cross memorial marks, fluttering war banners, phoenix feather wisps, heroic shield shapes, broken chains
> **Emotional Core:** Courage, sacrifice, triumphant glory, noble spirit
> **Colors (applied at runtime):** Scarlet, crimson, gold, sakura pink

---

## Eroica — Beam Textures

### ER Braided Energy Helix Beam
```
White intertwined double-helix beam on solid black background, two spiraling strands wrapped around each other like intertwined laurel branches, each strand composed of small leaf-shaped segments connected in a chain, bright core strand with softer outer strand, elegant classical proportions, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Cracking Energy Fissure Beam
```
White cracking fissure beam on solid black background, horizontal beam with noble fracture patterns resembling cracked marble or broken classical columns, the cracks form angular geometric patterns like shattered stained glass from a cathedral, bright fragments with darker dividing lines, the fragmentation beautiful rather than chaotic — ordered heroic destruction, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of rising energy shapes like sakura petals carried on a heroic wind, each petal-shape swirls gently while flowing left to right, some petals bright and close some gray and distant creating depth, graceful and noble motion rather than aggressive, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Energy Surge Beam
```
White energy surge beam on solid black background, powerful horizontal beam with triumphant crescendo intensity, the energy builds from left to right in rising waves like a symphony reaching its climax, each wave crest higher and brighter than the last, the peaks crowned with small fleur-de-lis or laurel-leaf shapes, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Horizontal Beam Streak
```
White horizontal beam streak on solid black background, clean powerful beam with subtle banner-ripple edges flowing along both sides like a war banner unfurling, the central beam steady and strong, edge ripples suggest fabric-like flowing motion, noble and directed, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### ER Infernal Beam Ring
```
White ring-shaped beam element on solid black background, heroic halo ring with evenly spaced laurel leaf decorations around its circumference, the ring bold and thick with classical proportions, each leaf bright and clearly defined, gaps between leaves softly glowing, the overall ring majestic like a victor's crown, game VFX texture, 128x128px --ar 1:1 --style raw
```

### ER Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of marching wave forms like a military hymn's rhythm — steady consistent peaks with heroic regularity, each wave crest adorned with a tiny rising ember shape, the wave pattern strong and disciplined rather than chaotic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of concentric arcing waves flowing left to right like a battle cry carrying across a field, each wave arc bold and broad, waves overlap creating interference patterns that form subtle cross/shield shapes at intersections, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Turbulent Plasma Core Segment
```
White turbulent core on solid black background, horizontal beam core with roiling energy that suggests rising embers and ascending spirit, upward-flowing bright streams within the beam body, scattered bright points like embers rising from a funeral pyre, noble and reverent in character rather than chaotic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### ER Thin Linear Glow
```
White thin linear glow on solid black background, fine horizontal line like a drawn sword blade catching light, razor-sharp central edge with elegant soft bloom extending symmetrically above and below, the bloom slightly warmer/wider at center suggesting noble brilliance, absolutely straight and unwavering — heroic resolve, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

### ER Beam Lens Flare Explosion Impact
```
White radial burst on solid black background, heroic detonation burst with six major radiating beams arranged like a star/compass rose, the vertical beam longest (reaching toward heaven), each beam tapers perfectly, concentric rings between the beams like sonar waves, rising ember particles scattered throughout, game VFX texture, 128x128px --ar 1:1 --style raw
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
White radial bloom on solid black background, circular bloom with very subtle laurel-wreath patterning at the mid-radius — barely visible brighter arcs suggesting leaf shapes arranged in a circle within the overall soft glow, creating an almost subliminal crown of light, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### ER Star Flare
```
White star flare on solid black background, four-pointed star with perfectly symmetrical spikes suggesting a compass rose or heraldic star, points sharp and clean, each spike same length representing balanced heroic virtue, bright center core with soft gray between points, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Eroica — Impact Effects

### ER Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding shockwave visualized as a heroic battle cry, the main wave ring thick and bold, followed by secondary thinner rings at harmonic intervals, rising ember-like particles lifting from the ring's passage, the rings suggest outward-expanding courage inspiring nearby allies, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Crumbling Shatter Burst
```
White shatter burst on solid black background, noble destruction — large geometric fragments breaking outward like shattered classical masonry, each fragment has clean angular edges, fragments larger near center becoming smaller toward edges, between fragments a bright lattice of breaking energy, suggests a shield breaking under final sacrifice, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Power Effect Ring
```
White power ring on solid black background, majestic expanding ring with evenly spaced small emblem-like protrusions along the outer edge, each protrusion a tiny shield or laurel shape, the ring itself bold and unwavering, inner glow slightly brighter creating depth, a ring of heroic power expanding from the strike point, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER Radial Slash Star Impact
```
White radial slash star on solid black background, six-pointed impact star where each point resembles an upward-reaching sword blade, points alternate in length, bright center is a hexagonal core, the overall shape suggests a memorial star or hero's sigil left at the point of impact, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### ER X-Shaped Impact Cross
```
White X cross impact on solid black background, two crossing slashes forming a noble X — each slash clean and precise like sword strikes, the intersection forms a bright diamond shape, each slash tapers elegantly to fine points, subtle sakura petal shapes scattered near the crossing point, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Eroica — Projectiles

### ER Gyratory Orb
```
White spinning orb on solid black background, spherical projectile with visible internal rotation of noble energy, the surface has subtle shield-like faceted panels, bright equatorial band, a faint ring of small rising ember points orbiting the sphere's circumference, dignified and powerful, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

### ER Orbiting Energy Sphere
```
White energy sphere on solid black background, central bright sphere with three small sakura-petal-shaped energy wisps orbiting around it in a tilted ring, the main sphere has a warm heroic glow, each orbiting wisp leaves a very faint gray trail, elegant orbital paths, game VFX projectile sprite, 128x128px --ar 1:1 --style raw
```

### ER Pulsating Music Note Orb
```
White music orb on solid black background, glowing sphere containing a visible musical note silhouette inside, the note shape elegant and refined like calligraphic script, the sphere surface has a faint shield-pattern lattice, subtle rising embers float upward from the sphere's top, game VFX projectile sprite, 64x64px --ar 1:1 --style raw
```

---

## Eroica — Slash Arcs

### ER Flaming Sword Arc Smear
```
White sword arc smear on solid black background, noble 120-degree sweeping crescent with clean powerful edges, the arc thick and decisive, outer edge has small sakura petal shapes breaking away and trailing behind, inner edge razor-sharp like a hero's blade, brightness peaks at the leading edge and fades toward the trailing end, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### ER Full Circle Sword Arc Slash
```
White full 360-degree sword arc on solid black background, complete spinning slash circle with heroic intensity, the ring uniformly thick and decisive like a whirlwind of blades, evenly spaced brighter nodes around the circumference where the slash is most concentrated, tiny rising embers scattered inside the circle ascending upward, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### ER Sword Arc Smear
```
White sword arc on solid black background, clean precise 90-degree arc with the grace of a master swordsman's stroke, the arc perfectly even in width from start to finish suggesting practiced technique, subtle leading-edge brightening, extremely smooth and controlled, slight trailing sakura wisps at the arc's end, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Eroica — Trails and Ribbons

### ER Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading to right, the bright end decorated with subtle banner-like ripple edges, the fade-out has small rising ember sparkle points ascending upward from the trail body, the overall trail noble and warm, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### ER Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with visible wave pattern — the wave peaks are decorated with tiny laurel-leaf shapes like a victory garland strung along the ribbon, steady and rhythmic — a march tempo rather than wild oscillation, soft bloom at each wave node, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### ER Spiraling Vortex Energy Strip
```
White spiraling strip on solid black background, horizontal strip with internal helical pattern of rising energy, the spiral arms shaped like phoenix feathers — elongated and elegant, twisting upward as they progress along the strip, bright feather crests with soft gray valleys, tapering from wide/bright left to narrow/faint right, game VFX trail texture, 256x64px --ar 1:1 --style raw
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
White laurel leaf on solid black background, single classical laurel leaf shape, pointed oval with visible central rib and branching veins, bright and sharply defined, the leaf curves slightly as if on a wreath, noble botanical accuracy, game particle texture, 32x16px --ar 2:1 --style raw
```

### ER Glyph
```
White heroic glyph on solid black background, circular symbol incorporating a sword silhouette crossed with a laurel branch, surrounded by a thin precise ring, small ember dots at the cardinal points, classical heraldic styling, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 3: SWAN LAKE — Grace in Monochrome
# ═══════════════════════════════════════════

> **Musical Soul:** Tchaikovsky's ballet — dying grace, ethereal beauty, elegance even in destruction
> **Visual Motifs:** Swan feathers drifting, ballet tutu fabric wisps, rippling water reflections, prismatic rainbow shimmer at edges, graceful arching curves, crescent moon shapes, lake surface distortions, crystal ice
> **Emotional Core:** Elegance, tragedy, ethereal beauty, graceful departure
> **Colors (applied at runtime):** Pure white, black contrast, prismatic rainbow edges

---

## Swan Lake — Beam Textures

### SL Braided Energy Helix Beam
```
White intertwined double-helix beam on solid black background, two spiraling ribbon-like strands wrapped around each other like ballet ribbons intertwining, each strand thin and graceful with flowing fabric-like edges, the twist is gentle and elegant rather than tight, soft bloom along the ribbon surfaces, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with delicate crystalline fracture patterns like cracking ice on a frozen lake, thin hairline fractures spreading in organic branching patterns, bright ice-crystal shards along the cracks, the beauty of something frozen and fragile breaking apart, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting feather-light shapes like down feathers carried on a gentle breeze, each feather shape soft and luminous, tumbling and floating left to right, some overlapping creating depth layers, graceful and unhurried, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with graceful crescendo intensity like a ballet dancer's grand jeté — energy builds in a soaring arc, peaks with ethereal brilliance, then descends gracefully, the peak has a spray of tiny scattered points like stage light catching crystalline dust, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Horizontal Beam Streak
```
White horizontal beam on solid black background, clean linear beam with perfectly smooth edges that have subtle ripple distortion like viewing the beam through water, the central line pure and bright, the ripple edges create a reflective shimmering effect, elegant and serene, tileable horizontal strip, game VFX texture, 256x32px --ar 8:1 --style raw
```

### SL Infernal Beam Ring
```
White ring beam element on solid black background, delicate circular ring like a ripple on a perfectly still lake, the ring thin and precise, subtle secondary and tertiary concentric ripple rings spaced inside and outside, the water-ripple quality is ethereal and pristine, game VFX texture, 128x128px --ar 1:1 --style raw
```

### SL Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of flowing sine waves like the gentle undulation of a swan's wake on water, smooth and regular with long wavelengths, the wave peaks catch light like water crests, delicate and hypnotic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of expanding arcs flowing left to right like ripples from a swan touching water, each arc thin and delicate, arcs overlap creating moiré interference patterns of exquisite mathematical beauty, ethereal and translucent in character, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with gentle rolling turbulence like mist rolling across a lake surface at dawn, soft billowing forms that flow and merge, no harsh elements — everything diffuse and dreamlike, bright wisps within the mist body, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### SL Thin Linear Glow
```
White thin linear glow on solid black background, extremely fine horizontal line like a strand of spider silk catching moonlight, delicate and barely visible with soft bloom, the thinness itself is the beauty — fragile, precise, heartbreakingly delicate, tileable horizontal strip, game VFX texture, 256x16px --ar 16:1 --style raw
```

---

## Swan Lake — Glow and Bloom

### SL Glow Orb
```
White soft circular glow on solid black background, ethereal orb with wide gentle Gaussian falloff, the center only moderately bright compared to the expansive soft glow — creating a diffuse misty quality rather than a hot center, like moonlight through fog, extremely soft and dreamlike, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### SL Lens Flare
```
White lens flare on solid black background, multi-element flare with elongated horizontal anamorphic streak dominant, delicate rainbow iris-diffraction rings ghosting above and below the streak, the flare feels prismatic and crystal-like, each element razor-fine and ethereal, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### SL Point Bloom
```
White pinpoint bloom on solid black background, tiny bright center with extensive wide soft bloom that creates a stage-spotlight quality, the bloom perfectly circular and even, suggesting a single perfect spotlight illuminating a dancer on a dark stage, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### SL Star Flare
```
White star flare on solid black background, six-pointed star with long impossibly thin spikes like ice crystal needles, each spike hair-fine and perfectly straight, the hexagonal symmetry of a snowflake, bright center point, the spikes seem to shimmer and vibrate with crystalline fragility, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Swan Lake — Impact Effects

### SL Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding ripple impact like a stone dropped in a perfectly still lake, concentric circles expanding outward with mathematical precision, each ring the same thin delicate weight, the gaps between rings perfectly even, pristine and ordered, drifting feather-like particles scattered between the rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL Crumbling Shatter Burst
```
White shatter burst on solid black background, crystalline fragmentation like a frozen lake surface cracking, irregular polygon shards of ice breaking apart with beautiful geometric precision, each shard has smooth faces and sharp edges, tiny prismatic sparkle points at shard corners where light would refract, elegant destruction, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL Power Effect Ring
```
White power ring on solid black background, thin elegant expanding ring with small swan-feather shapes trailing from the outer edge like feathers shed from a spinning dancer's costume, the ring itself delicate and precise, feather shapes drift outward and downward gracefully, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### SL X-Shaped Impact Cross
```
White X cross on solid black background, two crossing lines forming a delicate X like crossed ballet ribbons, each line thin and flowing with slight fabric-like curves rather than harsh straight slashes, the intersection has a small bright bloom, ribbon ends taper to fine points, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Swan Lake — Slash Arcs

### SL Sword Arc Smear
```
White sword arc on solid black background, graceful 120-degree sweeping arc like a dancer's arm tracing through stage light, the arc wide and flowing with soft edges like silk fabric in motion, brightness gradient flows from leading to trailing edge, inner curve decorated with tiny feather-light wisps breaking away, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### SL Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete pirouette circle — the ring varies in opacity like a spinning dancer becoming a blur, thicker sections suggest the body and thinner sections the limbs, tiny scattered points within the circle like stage dust caught in a spotlight, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Swan Lake — Trails and Ribbons

### SL Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading right, the bright end has soft feathery edges like down, the fade-out dissolves into individual small floating feather particles rather than a smooth gradient, graceful dissolution, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### SL Harmonic Standing Wave Ribbon
```
White standing wave ribbon on solid black background, horizontal strip with gentle flowing sine wave — the wave pattern smooth and balletic with long wavelengths, each wave crest catches light like a satin ribbon rippling in gentle wind, incredibly smooth and graceful, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Swan Lake — Particles

### SL Music Note
```
White music note on solid black background, flowing cursive music note with balletic grace, the note head opens into a tiny crescent shape, the flag extends into a flowing ribbon-like tail that curls elegantly, extremely refined calligraphy, game particle texture, 32x32px --ar 1:1 --style raw
```

### SL Swan Feather (Already exists — enhance)
```
White swan feather on solid black background, single large pristine feather with visible rachis and detailed barb structure, the feather slightly curved as if drifting through air, some barbs slightly separated at the tip suggesting gentle wear, luminous and soft with delicate edge translucency, game particle texture, 48x24px --ar 2:1 --style raw
```

### SL Crystal Shard
```
White crystal shard on solid black background, elongated hexagonal ice crystal with faceted surfaces, each facet has a different brightness suggesting light refraction, the shard slightly asymmetric and natural-looking, tiny bright points at the crystal's sharpest vertices, game particle texture, 32x16px --ar 2:1 --style raw
```

### SL Water Ripple
```
White water ripple on solid black background, single expanding concentric circle ripple like one perfect circular wave on a still lake surface, the ripple ring thin and precise, with a smaller secondary ring inside it, extremely clean and minimal, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 4: MOONLIGHT SONATA — The Moon's Quiet Sorrow
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Adagio sostenuto — quiet sorrow, moonlit peace, gentle nocturnal melancholy
> **Visual Motifs:** Crescent moons, tidal waves (gentle), moonbeam shafts, silvery mist, lunar phases, soft fog rolling, gentle water ripples in moonlight, willow branch silhouettes, moth wings, tide pools
> **Emotional Core:** Melancholy, peace, mystical stillness, quiet sorrow
> **Colors (applied at runtime):** Deep dark purples, vibrant light blues, violet, ice blue

---

## Moonlight Sonata — Beam Textures

### MS Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands of lunar energy winding around each other like moonlit tidal currents intertwining, the strands flow like slow water — thick and languid, soft edges suggest mist-shrouded forms, gentle and hypnotic rather than energetic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with hairline cracks like the surface of the moon — a network of fine gentle craters and rilles, the cracks are not violent but ancient and weathered, bright faces of lunar terrain catch light while shadows pool in the cracks, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting mist forms like fog rolling across a moonlit moor, each mist shape soft-edged and billowing, some forms resemble small crescent shapes carried on the breeze, the motion is slow and meditative, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Energy Surge Beam
```
White tidal surge beam on solid black background, horizontal beam with gentle tidal wave energy — the intensity rises and falls like ocean tides under the moon's pull, broad gentle swells rather than sharp peaks, the brightest points are smooth rounded crests, a sense of gravitational pull and lunar influence, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Oscillating Frequency Wave Beam
```
White oscillating wave beam on solid black background, horizontal beam composed of slow deep oscillations like the adagio tempo of the sonata, long wavelengths and deep amplitude suggesting profound emotional weight, each wave crest has a small crescent-shaped highlight like a tiny moon reflection, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of overlapping soft arcs flowing left to right like moonbeam shafts through clouds, each arc broad and gentle, the arcs create soft interference patterns like moonlight dappling through a forest canopy, quiet and serene, game VFX texture, 256x64px --ar 1:1 --style raw
```

### MS Turbulent Plasma Core
```
White turbulent core on solid black background, horizontal beam core with slow rolling fog-like turbulence, thick mist banks that drift and eddy like ground fog in moonlight, the turbulence is gentle and meditative — soft billowing forms merging and separating unhurriedly, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Glow and Bloom

### MS Glow Orb
```
White soft circular glow on solid black background, gentle lunar orb with wide ethereal falloff, the center moderately bright with an extremely gradual Gaussian fade creating maximum atmospheric haze, the glow feels like looking at the moon through a thin veil of clouds, soft and sorrowful, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### MS Lens Flare
```
White lens flare on solid black background, crescent-shaped primary flare element — a soft bright crescent like the moon in its waning phase, accompanied by two or three small circular ghost elements along a diagonal line, quiet and understated rather than dramatic, the crescent shape is the dominant element, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### MS Star Flare
```
White star flare on solid black background, four-pointed star with elongated VERTICAL spike suggesting a moonbeam descending from above, the vertical spike three times longer than the horizontal spike, creating an asymmetric shape like moonlight streaming down, soft and gentle, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

### MS Soft Radial Bloom
```
White radial bloom on solid black background, circular bloom with subtle crescent-shaped brighter region on one side, as if the bloom itself has lunar phases — one half slightly brighter than the other, creating an asymmetric moon-like quality, extremely soft edges, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Impact Effects

### MS Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, tidal ripple impact — soft concentric rings expanding outward like moonlit tide waves on a shore, each ring broad and gentle with wide spacing, the rings fade gradually rather than sharply, moth-wing-shaped particles drifting between the rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### MS Power Effect Ring
```
White power ring on solid black background, thin crescent-decorated ring — the ring itself slender and elegant, with tiny crescent moon shapes replacing the expected uniform thickness, crescents face different directions creating a lunar phase sequence around the circumference, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### MS Radial Slash Star Impact
```
White radial star impact on solid black background, four-pointed impact star with softened rounded points like moonbeams, each point tapers gradually with wide soft edges, the spaces between points have very faint fog-like fill rather than sharp darkness, gentle and melancholy, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Slash Arcs

### MS Sword Arc Smear
```
White sword arc on solid black background, gentle 120-degree sweeping arc like a moonbeam slowly drawn across the sky, the arc soft and wide with misty edges that blend smoothly into darkness, no harsh boundaries, brightness fades from a gentle peak at center toward both ends, ethereal and sorrowful like a sigh made visible, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### MS Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete circular arc resembling a lunar halo — the bright ring around the moon on a misty night, the ring even and gentle with wide soft edges that fade to nothing, a single brighter crescent segment suggesting a momentary brightening, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Trails and Ribbons

### MS Basic Trail
```
White basic trail strip on solid black background, horizontal gradient bright on left fading right, the bright end diffuse and misty like fog, the fade-out is gradual and organic with wisps of mist extending beyond the main body, the overall trail feels like moonlit fog slowly dissipating, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### MS Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with gentle tidal undulation — very long wavelength low amplitude waves like the surface of a calm moonlit sea, the wave crests have a faint crescent highlight, incredibly gentle and meditative in rhythm, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Moonlight Sonata — Particles

### MS Music Note
```
White music note on solid black background, gentle half note rendered in thin delicate lines, the note head open (hollow) like a crescent moon, the stem thin and slightly curved like a willow branch, surrounded by a very faint misty glow halo, melancholic and quiet, game particle texture, 32x32px --ar 1:1 --style raw
```

### MS Crescent Moon
```
White crescent moon on solid black background, thin waning crescent moon shape with smooth curved inner edge and soft outer edge, the crescent tapers to fine points at both tips, faint shadow visible on the dark side suggesting the full sphere, game particle texture, 32x32px --ar 1:1 --style raw
```

### MS Moth Wing
```
White moth wing on solid black background, single moth wing with visible wing-scale texture and subtle eyespot pattern, the wing broad and slightly triangular, edges feathered and soft like a real moth, ethereal and nocturnal, game particle texture, 32x24px --ar 4:3 --style raw
```

### MS Tidal Mist Wisp
```
White mist wisp on solid black background, small elongated wisp of fog, softly feathered at all edges, slightly curved as if drifting on a gentle breeze, thicker at one end thinning to nothing at the other, the softest most ethereal particle shape possible, game particle texture, 32x16px --ar 2:1 --style raw
```

### MS Glyph
```
White glyph on solid black background, circular symbol with a crescent moon at center flanked by small tidal wave curves, surrounded by a thin ring, tiny dot-stars at cardinal points, the linework thin and delicate, suggests a nocturnal ward or lunar seal, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 5: ENIGMA VARIATIONS — The Unknowable Mystery
# ═══════════════════════════════════════════

> **Musical Soul:** Elgar's enigma — mystery, hidden meanings, what lurks behind the veil, unknowable truths
> **Visual Motifs:** Watching eyes, void tendrils, occult sigils, impossible geometries, warping spaces, cryptic runes, eldritch tentacles, fractal spirals, Penrose triangles, dissolving reality
> **Emotional Core:** Mystery, dread, arcane secrets, unknowable horror
> **Colors (applied at runtime):** Void black, deep purple, eerie green flame

---

## Enigma Variations — Beam Textures

### EN Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands like eldritch tentacles coiling around each other, the surfaces textured with tiny sucker-like bumps, the coil tightens and loosens irregularly as if the tendrils are alive and squirming, disturbing organic quality, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Cracking Energy Fissure Beam
```
White cracking beam on solid black background, horizontal beam with reality-fracture cracks, the cracks don't follow material stress patterns but instead form impossible geometric angles that shouldn't exist, some cracks seem to open into deeper darkness beyond the beam, Escher-like paradox geometry, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### EN Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting occult sigil fragments, broken rune shapes and arcane letterforms flowing left to right as if torn from a forbidden manuscript, some fragments rotate or overlap creating momentary recognizable (yet meaningless) words, unsettling, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
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
White turbulent core on solid black background, horizontal beam core with writhing organic turbulence, the churning patterns form momentary eye shapes and tendril silhouettes before dissolving back into chaos, something watches from within the energy, alive and aware, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
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
White resonance impact on solid black background, expanding wave rings that warp and distort reality, the rings are not perfect circles but wobble and buckle as if space resists their expansion, sections of rings fade to nothing mid-arc then reappear, eye-shaped distortion at center, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN Crumbling Shatter Burst
```
White shatter burst on solid black background, reality fragmenting into impossible Escher-like shards, each fragment shows a different perspective angle despite being flat, the gaps between fragments show void, some shards overlap in ways that violate geometry, deeply unsettling fragmentation, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN Power Effect Ring
```
White power ring on solid black background, expanding ring decorated with arcane sigils and eye motifs spaced around its circumference, the ring itself slightly irregular as if hand-drawn by a mad scholar, the sigils are legible but meaningless — occult power symbols from no real tradition, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### EN X-Shaped Impact Cross
```
White X cross on solid black background, two crossing slashes that don't quite intersect correctly — they slip past each other in impossible perspective, each slash has tentacle-like wisps trailing from it, the near-intersection point has a bright disturbing eye-shaped flash, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Enigma Variations — Slash Arcs

### EN Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc whose edges writhe with tiny tendril protrusions, the arc body shows faint eye-spot patterns at irregular intervals, brightness is uneven — sections flicker between bright and dim unpredictably, the whole arc has an organic alive quality, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### EN Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete circle that isn't quite a circle — it spirals inward very slightly creating a subtle vortex, the ring surface textured with tiny occult symbols, bright sections alternate asymmetrically with dimmer sections, the overall shape is disorienting and wrong, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Enigma Variations — Trails and Ribbons

### EN Basic Trail
```
White trail strip on solid black background, horizontal gradient that doesn't fade smoothly but dissolves in patches — sections of the trail vanish revealing the void behind while other sections remain bright, as if the trail exists partially in another dimension, disturbing intermittent visibility, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### EN Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with a wave pattern that reverses direction partway through, the wave phase shifts impossibly in the middle creating a visual paradox, standing wave nodes blink like eyes opening and closing, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Lightning

### EN Lightning Surge
```
White lightning bolt on solid black background, electrical arc that branches into tentacle-like organic tendrils rather than jagged angles, the branches reach and grasp like fingers, some branches curl back on themselves, the bolt seems directed by malicious intelligence rather than physics, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Enigma Variations — Particles

### EN Music Note
```
White music note on solid black background, a music note where the note head is replaced by a tiny eye shape, the stem and flag rendered in occult spidery linework, the overall shape barely recognizable as a note — corrupted and wrong, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Enigma Eye (Already exists — enhance variants)
```
White watching eye on solid black background, single detailed eye with visible iris ring and bright pupil center, the eye is slightly inhuman — too round, the iris too detailed, small eldritch tendrils extend from the corners, the eye radiates an active malevolent awareness, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Void Tendril
```
White tendril on solid black background, single curving organic tentacle shape tapering from thick base to thin tip, the surface textured with tiny sucker marks, the tendril curves in an S-shape as if reaching for something, slightly transparent at the thinnest end, game particle texture, 48x16px --ar 3:1 --style raw
```

### EN Impossible Geometry Shard
```
White geometric shard on solid black background, small Penrose-triangle-like impossible shape, each face has a different brightness suggesting an object that cannot exist in 3D space, clean lines and precise angles, mathematically disturbing, game particle texture, 32x32px --ar 1:1 --style raw
```

### EN Glyph (Occult)
```
White glyph on solid black background, complex arcane sigil with concentric circles and intersecting lines forming an occult diagram, a watching eye at the very center, tiny illegible rune-like symbols along the circle borders, unsettling and unholy, game particle texture, 32x32px --ar 1:1 --style raw
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
> **Visual Motifs:** Water lily pads, gentle ripples, impressionist brush strokes, soft-focus moonlight, dew drops, gentle mist, floating dust motes in moonbeams, fireflies, glass reflections, puddle reflections
> **Emotional Core:** Dreamlike calm, gentle luminescence, tender nostalgia, reverie
> **Colors (applied at runtime):** Night mist blue, soft blue, pearl white

---

## Clair de Lune — Beam Textures

### CL Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two gently twisting streams like moonlight reflecting off two intertwined streams of water, soft-focus impressionistic quality, the edges slightly blurred as if painted with a soft brush, each strand wider and more diffuse than sharp, dreamlike, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of drifting shapes like fireflies and dew drops floating on a gentle current, each shape small and perfectly round with a soft halo, scattered at varying distances creating bokeh-like depth, peaceful and hypnotic, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam with gentle impressionistic swells like music notes rising and falling in a reverie, each swell soft-edged and rounded, the intensity is never harsh — even the brightest points have diffuse soft-focus quality, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam of gentle ripples like moonlight playing across a shallow pool, the ripples are wide gentle curves with soft reflection highlights at their peaks, the pattern dreamlike and soothing, almost hypnotic in its gentle regularity, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### CL Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of overlapping soft circles like water lily pad reflections in moonlit water, each circle soft-edged and slightly overlapping the next, creating a chain of gentle luminous forms, impressionistic and peaceful, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Glow and Bloom

### CL Glow Orb
```
White glow on solid black background, extremely soft and diffuse circular glow, the center barely brighter than the surrounding bloom — everything is soft-focus, like looking at a distant light through dewy glass, the widest softest falloff possible, pure gentle luminescence, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### CL Lens Flare
```
White lens flare on solid black background, impressionistic bokeh-style flare — multiple soft hexagonal shapes of varying sizes arranged loosely around a central soft disc, each bokeh shape slightly different in size, like light through a rain-spotted window, dreamy and unfocused, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### CL Star Flare
```
White star flare on solid black background, soft diffuse four-pointed star with extremely wide rounded points — more of a soft luminous plus-shape than sharp star, the edges completely faded and blurred, like starlight seen through tears or gentle mist, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Impact Effects

### CL Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, gentle expanding ripple circles like a petal falling onto moonlit water, the rings wide-spaced and incredibly soft, each ring barely brighter than the next, no harshness — everything fades with impressionistic softness, tiny firefly-like points drifting between rings, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### CL Power Effect Ring
```
White power ring on solid black background, gossamer-thin ring that is more impression than solid line, the ring formed by many tiny soft dots arranged in a circle like a necklace of dew drops catching moonlight, each dot has a miniature bloom, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Clair de Lune — Slash Arcs

### CL Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc rendered in impressionistic brush-stroke style, the arc wide and soft like a watercolor wash, edges completely blurred and organic, no sharp lines — the slash is a gentle sweeping reflection of light on water, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Clair de Lune — Trails and Ribbons

### CL Basic Trail
```
White trail on solid black background, horizontal gradient that dissolves into scattered soft dots like firefly lights gradually spacing farther apart and dimming, the bright end is a diffuse soft glow, the fade-out is a constellation of dimming gentle points, dreaming and wistful, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### CL Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with the gentlest possible undulation — almost flat with barely perceptible sine wave, the wave crests have soft impressionistic highlights like moonlight on tiny ripples, extremely calm and meditative, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Clair de Lune — Particles

### CL Music Note
```
White music note on solid black background, whole note rendered as a soft luminous oval with an impressionistic quality — edges blurred, the interior slightly textured like light on water, surrounded by a gentle misty glow, the note seems to float and drift, game particle texture, 32x32px --ar 1:1 --style raw
```

### CL Firefly
```
White firefly particle on solid black background, tiny bright oval point with very wide soft bloom extending around it, the bloom slightly elongated suggesting movement, like a single firefly captured mid-flight on a summer night, minimal and magical, game particle texture, 16x16px --ar 1:1 --style raw
```

### CL Dew Drop
```
White dew drop on solid black background, single perfect sphere of water catching moonlight, a bright highlight crescent on upper left, softer fill glow on lower right, extremely tiny and precious, like a single drop of moonlight condensed, game particle texture, 16x16px --ar 1:1 --style raw
```

### CL Impressionist Brush Stroke
```
White brush stroke on solid black background, single short impressionist paint dab — wider in the middle tapering at both ends, slightly textured with visible brush-hair lines, soft-edged, like a Monet brush stroke made of light, game particle texture, 32x16px --ar 2:1 --style raw
```

### CL Glyph
```
White glyph on solid black background, circular symbol with a water lily silhouette at center, surrounded by a ring of tiny dew-drop dots, the linework soft and slightly imprecise — impressionistic rather than architectural, gentle and dreamlike, game particle texture, 32x32px --ar 1:1 --style raw
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
---

# ═══════════════════════════════════════════
# THEME 9: NACHTMUSIK — A Little Night Music
# ═══════════════════════════════════════════

> **Musical Soul:** Mozart's Eine Kleine Nachtmusik — playful nocturnal elegance, stellar beauty, the joy of stargazing, sophisticated night revelry
> **Visual Motifs:** Constellations, telescope star fields, elegant night sky, precise star points, comet trails, crescent moon with stars, nocturnal flowers (evening primrose, moonflower), astronomical instruments, crescendo wave lines, dance-floor sparkle
> **Emotional Core:** Nocturnal wonder, stellar beauty, sophisticated joy, cosmic elegance
> **Colors (applied at runtime):** Deep indigo, starlight silver, cosmic blue

---

## Nachtmusik — Beam Textures

### NK Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling star-trails wrapping around each other like dual comet paths in the night sky, each strand dotted with tiny precise star points along its length, the strands are clean and precise — astronomical accuracy, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### NK Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of tiny star points drifting like a meteor shower, each point precise and sharp, some stars brighter and larger (different magnitudes), occasional streak shapes like shooting stars, elegant nighttime astronomy captured in a beam, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
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
White glow on solid black background, precise stellar-magnitude glow — a bright star with controlled clean Gaussian falloff, the center pointlike and brilliant, falloff even and calculated like a real star's apparent brightness profile, surrounded at far distance by scattered faint point companions, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### NK Lens Flare
```
White lens flare on solid black background, telescope-optics flare — central bright disc with precise thin diffraction spikes from a telescope's secondary mirror support vanes, subtle Airy disc rings around the central point, realistic astronomical optics quality, elegant and scientific, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### NK Star Flare
```
White star flare on solid black background, precise four-pointed star with razor-thin spikes of equal length, perfect symmetry like a calibration star in a telescope eyepiece, tiny Airy disc ring around the center, clean and scientific in quality, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Nachtmusik — Impact Effects

### NK Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding rings like sonar/radar display, precise circular rings at mathematically even intervals, each ring thin and clean, tiny star points scattered in the spaces between rings like a sky chart expanding, elegant ordered expansion, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### NK Power Effect Ring
```
White power ring on solid black background, clean precise ring with constellation-point decorations at regular intervals around its circumference, thin constellation-line segments connecting some points, the ring is a star chart in circular form, astronomical and elegant, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Nachtmusik — Slash Arcs

### NK Sword Arc Smear
```
White sword arc on solid black background, 120-degree sweeping arc rendered as a band of starlit sky, the arc body contains scattered tiny star points creating a Milky Way-arc effect, bright central band with fainter scattered stars toward the edges, elegant cosmic sweep, game VFX slash texture, 256x256px --ar 1:1 --style raw
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
White glyph on solid black background, circular symbol incorporating an astronomical sextant or compass rose design, star-point decorations at key angles, tiny musical notation fragments integrated into the geometric framework, precise and elegant — the intersection of music and astronomy, game particle texture, 32x32px --ar 1:1 --style raw
```

---
---

# ═══════════════════════════════════════════
# THEME 10: ODE TO JOY — Universal Brotherhood
# ═══════════════════════════════════════════

> **Musical Soul:** Beethoven's Ninth — the triumph of the human spirit, universal joy, celebration, the brotherhood of all humanity
> **Visual Motifs:** Fireworks, jubilant confetti, radiant sunbursts, champagne bubble sparkle, festival streamers, golden laurels, rays of dawn, cathedral light shafts, raised hands, celebration bells, torch flames
> **Emotional Core:** Joy, celebration, triumph of spirit, radiant warmth
> **Colors (applied at runtime):** Warm gold, radiant amber, jubilant light

---

## Ode to Joy — Beam Textures

### OJ Braided Energy Helix Beam
```
White intertwined helix beam on solid black background, two spiraling strands like celebratory streamers twisting together in joyful spirals, the strands are wide and flowing like festival ribbons, each twist is open and exuberant, tiny sparkle points scattered along the streamer edges, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Energy Motion Beam
```
White flowing energy beam on solid black background, horizontal stream of rising celebration shapes — confetti pieces, tiny sparkle bursts, and small star shapes tumbling joyfully left to right, the shapes are varied and numerous creating a festival procession feeling, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Energy Surge Beam
```
White energy surge beam on solid black background, horizontal beam building to a glorious fortissimo climax, the energy rises in triumphant ascending waves, each wave crest larger and more brilliant than the last, the final peak explodes with radiating streamers and sparkles, the joy is OVERWHELMING, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Sound Wave Beam
```
White sound wave beam on solid black background, horizontal beam of jubilant waves flowing left to right like a crowd's cheer visualized in sound waves, the arcs are full and round suggesting rich choral harmony, waves overlap and create bright constructive interference where voices join together, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

### OJ Oscillating Frequency Wave Beam
```
White wave beam on solid black background, horizontal beam of triumphant chorale harmonics — multiple wave frequencies layered together to create rich complex waveform, the combined pattern has a warm rounded character, occasional bright peaks where all harmonics align in glorious unison, tileable horizontal strip, game VFX texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Glow and Bloom

### OJ Glow Orb
```
White glow on solid black background, warm expansive bloom with generous Gaussian falloff, the center bright and inviting, the falloff wide and embracing — this light welcomes rather than blinds, the warmth feels like golden sunlight on skin, generous and giving, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### OJ Lens Flare
```
White lens flare on solid black background, jubilant multi-element flare with a central bright disc, multiple warm concentric ring elements, and thin streaks that fan outward in a sunburst pattern, the flare feels like dawn breaking — rays of first light, celebration and new beginning, game VFX bloom texture, 128x128px --ar 1:1 --style raw
```

### OJ Star Flare
```
White star flare on solid black background, six-pointed star with wide generous spikes that radiate warmth, the points broader than sharp, the center large and inviting, between the major spikes smaller secondary spikes create a full sunburst pattern, joyful and radiant, game VFX bloom texture, 64x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Impact Effects

### OJ Harmonic Resonance Wave Impact
```
White resonance impact on solid black background, expanding firework-burst rings, each ring erupts with small sparkle points like a pyrotechnic display, the rings expand with celebration energy, between rings tiny confetti-like particle shapes scatter, the overall impression is a triumphant firework detonation, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ Crumbling Shatter Burst
```
White burst on solid black background, not destruction but ERUPTION of celebration — fragments burst outward like confetti cannon, each fragment a different small geometric shape (triangles, circles, stars, rectangles), the fragments scatter joyfully rather than violently, bright and festive, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ Power Effect Ring
```
White power ring on solid black background, expanding ring wreathed in tiny celebration elements — small flags, streamers, and musical note shapes decorate the ring's circumference, the ring itself golden-warm in brightness, a victory lap of light, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

### OJ X-Shaped Impact Cross
```
White X cross on solid black background, two crossing streaks that burst with sparkle at their intersection — the cross is a firework pattern, star-burst decoration at the center where the streaks meet, each streak tapers but is adorned with small point-sparkles along its length, triumphant, game VFX impact texture, 128x128px --ar 1:1 --style raw
```

---

## Ode to Joy — Slash Arcs

### OJ Sword Arc Smear
```
White sword arc on solid black background, jubilant 120-degree sweeping arc with celebration energy, the arc body sparkles with tiny embedded point lights, the leading edge erupts with small confetti-like shapes, the trailing edge dissolves into sparkle dust, the swing is triumphant and exuberant, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

### OJ Full Circle Sword Arc
```
White full 360-degree arc on solid black background, complete triumphant spin with firework-ring quality, the circle is a bright bold ring with sparkle-burst bright points evenly spaced like embedded fireworks detonating along the ring's path, tiny celebratory particles fill the interior, game VFX slash texture, 256x256px --ar 1:1 --style raw
```

---

## Ode to Joy — Trails and Ribbons

### OJ Basic Trail
```
White trail on solid black background, horizontal gradient that dissolves into festival sparkle, the bright end overflows with tiny celebration points, the fade-out scatters into confetti-like shapes and diminishing sparkle points, the overall feel is a celebration parade trail, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

### OJ Harmonic Standing Wave Ribbon
```
White wave ribbon on solid black background, horizontal strip with joyful chorale-wave pattern, each wave crest adorned with a tiny sunburst shape, the rhythm is allegro — quick and energetic, the wave pattern rises and leaps with the exuberance of Beethoven's finale, game VFX trail texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Lightning

### OJ Lightning Surge
```
White lightning bolt on solid black background, bright triumphant bolt splitting into MANY branches — not threatening but celebratory, like a firework trail branching into multiple streamers, each branch ends in a small bright star-burst, the bolt travels upward suggesting ascension and joy rather than destructive strike, game VFX lightning texture, 256x64px --ar 1:1 --style raw
```

---

## Ode to Joy — Particles

### OJ Music Note
```
White music note on solid black background, joyful eighth note rendered with bold confident strokes, the note head round and full, the flag upturned and enthusiastic, the entire note slightly tilted as if dancing, surrounded by a scatter of tiny sparkle dots like celebration confetti, game particle texture, 32x32px --ar 1:1 --style raw
```

### OJ Confetti
```
White confetti piece on solid black background, small rectangular strip slightly curled and twisted like thrown confetti, one face bright and one face slightly darker suggesting dimension, the curl creates a dynamic shape suggesting mid-air tumbling, game particle texture, 16x8px --ar 2:1 --style raw
```

### OJ Firework Sparkle
```
White firework sparkle on solid black background, bright central point with four short radiating lines and four tiny dots between them creating an 8-element sparkle burst, like a single moment from a firework captured in freeze frame, crisp and celebratory, game particle texture, 16x16px --ar 1:1 --style raw
```

### OJ Festival Streamer
```
White streamer on solid black background, short curling ribbon of celebration streamer, the ribbon twists and curls in space showing both bright face and shadowed curve, wider at one end where it was released, tapering at the other where it curls, dynamic and festive, game particle texture, 32x16px --ar 2:1 --style raw
```

### OJ Sunburst Fragment
```
White sunburst fragment on solid black background, single radiating ray-shape like one spoke of a sunburst, wider at the base tapering to a fine point, the base has a tiny arc-shape connecting to where other rays would be, warm and radiant, game particle texture, 32x8px --ar 1:1 --style raw
```

### OJ Glyph
```
White glyph on solid black background, circular symbol with a radiant sun design at center surrounded by musical notation fragments, the sun has triangular rays, tiny laurel leaves flank the sides, the ring decorated with small celebration-dot patterns, joyful and triumphant, game particle texture, 32x32px --ar 1:1 --style raw
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
| Beam Textures | 14 | 13 | 11 | 7 | 7 | 5 | 5 | 5 | 4 | 5 |
| Glow/Bloom | 7 | 6 | 4 | 4 | 3 | 3 | 3 | 3 | 3 | 3 |
| Impact Effects | 7 | 5 | 4 | 3 | 4 | 3 | 2 | 4 | 2 | 4 |
| Projectiles | 6 | 3 | — | — | — | — | — | — | — | — |
| Slash Arcs | 4 | 3 | 2 | 2 | 2 | 2 | 1 | 2 | 1 | 2 |
| Trails/Ribbons | 3 | 3 | 2 | 2 | 2 | 2 | 2 | 2 | 2 | 2 |
| Lightning | 1 | 1 | — | — | 1 | — | — | 1 | — | 1 |
| Noise (themed) | 3 | — | — | — | — | — | — | — | — | — |
| Smoke/Atmo | 1 | — | — | — | — | — | — | — | — | — |
| Particles | 9 | 5 | 4 | 5 | 5 | 4 | 5 | 5 | 5 | 6 |
| **TOTAL** | **55** | **39** | **27** | **23** | **24** | **19** | **21** | **27** | **17** | **28** |

> **Grand Total: ~280 unique themed asset prompts across 10 themes**

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
