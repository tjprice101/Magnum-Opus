# 🦢 Swan Lake — Resonance Weapons Planning

> *"Grace dying beautifully — the swan's final dance."*

## Theme Identity

| Attribute | Value |
|-----------|-------|
| **Musical Soul** | Tchaikovsky's Swan Lake — beauty in tragedy, grace over despair |
| **Emotional Core** | Elegance, tragedy, ethereal beauty |
| **Color Palette** | Pure white, black contrast, prismatic rainbow edges |
| **Palette Hex** | Obsidian Black `(10, 10, 15)` → Swan White `(245, 245, 255)` → Pearl Shimmer `(230, 220, 245)` → Prismatic Edge `(200, 180, 255)` → Iridescent Rainbow (shifts through full spectrum at edges) |
| **Lore Color** | `new Color(240, 240, 255)` — Pure White |
| **Lore Keywords** | Swan, feather, grace, lake, reflection, black & white, iridescence, elegance, tragedy, death dance |
| **VFX Language** | Floating feathers, liquid reflections, graceful arcs, prismatic refractions, pearlescent trails, monochrome elegance with rainbow prismatic accents at edges |

---

## Weapons Overview

| # | Weapon | Class | Key Mechanic |
|---|--------|-------|-------------|
| 1 | Call of the Black Swan | Melee | Black swan arcs with dark feather storms |
| 2 | The Swan's Lament | Ranged | Lamenting shots with destruction halos |
| 3 | Call of the Pearlescent Lake | Ranged | Pearlescent rockets with prismatic splash |
| 4 | Iridescent Wingspan | Magic | Wingspan bolts with rainbow shimmer |
| 5 | Chromatic Swan Song | Magic | Chromatic bolts with aria detonations |
| 6 | Feather of the Iridescent Flock | Summon | Iridescent crystal minion flock |
| 7 | Feather's Call | Special | Boss transformation item |

---

## 1. Call of the Black Swan (Melee)

### Identity & Musical Soul
The Black Swan is the dark mirror — Odile, the deceptive enchantress. Where white swans are graceful and mournful, the Black Swan is **fierce elegance**. Every swing of this blade is a violent dance move — pirouettes of dark feathers and arcs of obsidian energy. The tragedy is that the Black Swan's beauty is as real as the White Swan's, but driven by darker purpose.

### Lore Line
*"She danced not for love, but for the ruin of those who watch."*

### Combat Mechanics
- **3-Phase Dance Combo** (extends MeleeSwingItemBase):
  - **Phase 1 — Entrechat**: Quick diagonal slash. Spawns 3 black feather projectiles in a fan arc. Feathers drift down gracefully while dealing damage.
  - **Phase 2 — Fouetté**: Spinning horizontal slash (wider arc). Spawns a BlackSwanFlareProj — a dark radial flare that damages enemies in a circle. Feather trail lingers.
  - **Phase 3 — Grand Jeté**: Leaping overhead slam (player gets small upward impulse). Spawns a swan silhouette shockwave (expanding crescent) + 5 feathers rain down from above.
- **Swan's Grace**: Successive hits without getting hit build Grace stacks (max 5). Each stack: +8% swing speed, trail becomes more prismatic. At max Grace, next swing releases Prismatic Swan — a spectral white-rainbow swan that charges forward through enemies.
- **Black Mirror**: If the player takes damage while swinging, Grace stacks convert to Dark Mirror stacks. Dark Mirror: +15% damage but -5% speed per stack.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `BlackSwanArc.fx` | Swing trail with feather-edge texture | UV-scrolled trail strip. Core: pure black. Edges: white-to-prismatic shimmer (rainbow at very edge tips). Feather-shaped alpha mask along edges (jagged feather silhouettes). |
| `SwanGraceAura.fx` | Grace stack indicator aura around player | Soft SDF circle. At 0 stacks: invisible. Each stack adds a faint white ring. At 5 stacks: full prismatic rainbow shimmer. If Dark Mirror: inverts to obsidian aura with dark purple edges. |
| `PrismaticSwanCharge.fx` | Spectral swan projectile on max Grace release | Swan silhouette body with internal prismatic color scroll (rainbow UV flowing through). White core → prismatic edges → rainbow trail. Motion blur/afterimage stretch. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| BlackFeatherParticle | Drifts down slowly, tumbling rotation | Black feather sprite with subtle prismatic edge highlight, 40-60 frame life |
| WhiteFeatherParticle | Lighter, rises slightly before drifting | White feather with iridescent sheen, 50-70 frame life |
| PrismaticSparkParticle | Burst at prismatic effect moments | Rainbow color that shifts through spectrum, tiny 2-3px, 5-8 frame burst |
| GraceStackGlintParticle | Orbits player at arm's reach per Grace stack | Tiny white sparkle, slow orbit, 1 per stack |

#### Bloom Layers
1. **Blade edge**: Thin white glow at cutting edge during swing (tight, elegant)
2. **Feather scatter**: Subtle white micro-bloom on each feather projectile
3. **Grace aura**: Growing white-prismatic glow around player (scales with stacks)
4. **Prismatic Swan**: 3-layer bloom (white core → rainbow mid → wide soft white outer)

#### Trail Rendering
- BlackSwanPrimitiveRenderer (existing): Verify supports feather-edge alpha mask
- Trail width: Thin and elegant — narrow strip with graceful curve
- Trail color: Black body → white edge → prismatic tip fringe

#### Asset Requirements
| Asset | Path | Midjourney Prompt |
|-------|------|-------------------|
| Black feather texture | `Assets/SwanLake/CalloftheBlackSwan/Pixel/BlackFeather.png` | "Elegant black feather with subtle iridescent rainbow shimmer at edges, game particle sprite, on solid black background, 32x64px --ar 1:2 --style raw" |
| Swan silhouette | `Assets/SwanLake/CalloftheBlackSwan/Trails/SwanSilhouette.png` | "Graceful swan in flight silhouette, pure white, wings spread wide, elegant curved neck, on solid black background, 256x128px --ar 2:1 --style raw" |
| Prismatic trail texture | `Assets/SwanLake/CalloftheBlackSwan/Trails/PrismaticTrail.png` | "Rainbow prismatic energy trail texture, smooth gradient through full color spectrum, white center fading to rainbow edges, on solid black background, 512x64px seamless --ar 8:1 --style raw" |

#### Debuffs
| Debuff | Effect | Duration |
|--------|--------|----------|
| Swan's Mark | Marked enemies have -10 defense. Visual: black feather stuck to enemy. | 300 frames (5s) |

---

## 2. The Swan's Lament (Ranged)

### Identity & Musical Soul
A lament is a sorrowful song of mourning. This ranged weapon fires bullets infused with the dying swan's grief — each shot carries the weight of loss. The Destruction Halo it spawns is the final ring of light as the swan falls. Elegant in destruction, mournful in every sound.

### Lore Line
*"Each shot is a tear, and each tear is a farewell."*

### Combat Mechanics
- **Lament Bullet**: Primary fire — fast-moving white bullet that leaves a brief white streak trail. On hit, spawns a burst of 3 feather shrapnel in a cone behind the target.
- **Destruction Halo**: Every 6th shot fires a DestructionHaloProj — a large, slow-moving halo ring that expands as it travels (starts tight, reaches max radius at 2s). Enemies touching the halo rim take damage and are afflicted with Mournful Gaze (-15% movement speed).
- **Lamentation Stack**: Hitting the same enemy consecutively builds Lamentation. At 5 Lamentation on one target: the target begins weeping (cosmetic + -20% attack speed).
- **Finale Lament**: If a Destruction Halo kills an enemy, the halo detonates into a white flash nova — all enemies within nova radius receive Lamentation instantly at 5 stacks.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `DestructionHaloRing.fx` | Expanding halo ring projectile | SDF ring that expands over time (radius driven by uniform). Ring body: white with prismatic edge shimmer. Inner feathered edge → outer sharp dropoff. Subtle pulsing alpha. |
| `LamentBulletStreak.fx` | Clean elegant bullet streak trail | Ultra-thin trail strip, bright white center → transparent edge. No noise — perfectly clean line. Fades rapidly behind bullet. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| FeatherShrapnelParticle | Bursts in cone behind hit target, tumbles | White-pearl feathersprites, 3 per burst, 20-30 frame life with slow drift |
| HaloGlintParticle | Orbits along the halo ring circumference | Tiny prismatic sparkle, follows ring edge, 10 frame life |
| LamentationTeardropParticle | Falls from Lamentation-stacked target | Small white-blue teardrop, slow gravity fall, 30 frame life |
| NovaFeatherBurstParticle | Radial burst from Finale Lament nova | White feathers bursting radially, 15-20 per nova, 25 frame life |

---

## 3. Call of the Pearlescent Lake (Ranged)

### Identity & Musical Soul
The Pearlescent Lake is the world where swans exist — a shimmering body of water that catches moonlight and scatters it into a thousand prismatic reflections. This weapon fires rockets that carry the lake's pearlescent beauty, creating splash zones of iridescent liquid that persist and damage enemies stepping through them.

### Lore Line
*"The lake does not forgive those who disturb its surface."*

### Combat Mechanics
- **Pearlescent Rocket**: Primary fire — medium-speed rocket with iridescent pearlescent trail. On impact: splash zone (persistent AoE, 4 tile radius, lasts 5 seconds) of shimmering "lake water."
- **Lake Surface Zones**: Splash zones are persistent damage fields. Enemies in them take continuous damage + slow (25%). Zone color shifts through pearl/rainbow slowly. Multiple zones can overlap for stacking damage.
- **Ripple Effect**: Alternate fire — fires a "surface ripple" projectile that travels along the ground horizontally. Creates a line of small splash zones wherever it passes (think: a stone skipping across the lake).
- **Perfect Reflection**: If you fire a rocket directly downward into an existing splash zone, it "reflects" into an upward geyser — 3 prismatic pillars of lake water erupt, piercing upward through enemies with high damage.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `PearlescentSplashZone.fx` | Persistent lake water AoE field | Flat screen-space plane. Internal UV-scroll with water caustics pattern (use noise texture for caustics). Color: pearl white → prismatic shifting. Alpha pulsing gently. Edge feathered with SDF circle. |
| `PearlescentRocketTrail.fx` | Rocket trail with liquid pearl shimmer | Strip trail — pearlescent color (white that shifts to subtle pastel rainbow based on noise). Internal UV-scroll, slow speed. Elegant, liquid feel. |
| `PrismaticGeyser.fx` | Reflected upward geyser pillars | Vertical beam shader with rising UV-scroll. Prismatic rainbow with white highlights. Liquid distortion at base. Spray particles at top. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| PearlDropletParticle | Splashes out from rocket impact, arcs downward | Small pearlescent drops, gravity-affected arcs, 15 frame life |
| LakeSurfaceShimmerParticle | Rises from splash zone periodically | Subtle white motes, slow upward drift, 20 frame life |
| GeyserSprayParticle | Ejected from top of geyser pillars | Fast-moving prismatic drops, small arc, 10 frame life |
| RippleRingParticle | Expanding ring at each ripple bounce point | White expanding ring, very thin, 15 frame life |

---

## 4. Iridescent Wingspan (Magic)

### Identity & Musical Soul
Wingspan — the full spread of a swan's wings catching iridescent light. This magic weapon fires bolts that fan outward like spreading wings, then converge at the cursor. The rainbow iridescence shifts as the bolts travel, creating a mesmerizing display of light. Grace in motion, beauty in destruction.

### Lore Line
*"To witness the full wingspan is to know both the beauty and the death."*

### Combat Mechanics
- **Wingspan Bolt Fan**: Primary fire — fires 5 bolts in a wingspan pattern (2 flanking bolts arc outward then curve inward, 1 center bolt goes straight, 2 inner bolts arc gently). All 5 converge at cursor position. Each bolt shimmers with different spectrum color.
- **Iridescent Trail Persistence**: Bolt trails linger briefly (0.5s). Enemies that touch lingering trails take minor damage + receive Iridescent Burn (DoT that visual is rainbow flames).
- **Prismatic Convergence**: When all 5 bolts converge at the cursor, they create a prismatic burst — damage scales with how many bolts arrived (1 bolt: 1x, 5 bolts: 2.5x). Perfect 5-bolt convergence also spawns a brief prismatic orb that shoots 8 rainbow lasers in cardinal directions.
- **Wingspan Resonance**: If you cast within 1 second of the previous convergence, the next fan inherits the rainbow color cycle from where the last one ended — visual continuity that also grants +10% damage on sequential casts.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `WingspanBoltShimmer.fx` | Individual bolt body with color-cycling iridescence | SDF orb projectile body. Color: cycles through full spectrum based on bolt's lifetime + a per-bolt offset (so each of the 5 is at different hue). Internal glow pulse. Rainbow gradient around edge. |
| `PrismaticConvergenceBurst.fx` | 5-bolt convergence explosion | Expanding SDF circle with rainbow radial gradient (each sector a different color, like a color wheel). Center white. Edge prismatic. Cardinals highlighted for laser spawn. Brief (0.3s). |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| IridescentTrailSparkParticle | Shed from bolt trail as it travels | Tiny rainbow sparkle, color matched to bolt's current hue, 8 frame life |
| ConvergenceFlashParticle | Brief central burst at convergence point | White-rainbow flash, 4 frame burst |
| ConvergenceLaserSparkParticle | Shed from cardinal laser lines | Rainbow spark, directional, 6 frame life |
| WingspanResonanceGlintParticle | Brief glint on player when Resonance is active | White-to-rainbow glint near weapon hand, 3 frame |

---

## 5. Chromatic Swan Song (Magic)

### Identity & Musical Soul
The swan song — the final, most beautiful performance before death. This weapon channels **chromatic magic** — a full spectrum of power unleashed in bolts that detonate into **aria detonations**: structured, musical explosions where each ring of the blast corresponds to a different note and color. It's the most dramatic weapon in the Swan Lake arsenal — the last performance.

### Lore Line
*"The final song is always the most beautiful. It has to be."*

### Combat Mechanics
- **Chromatic Bolt**: Primary fire — moderately fast bolt that shifts through full rainbow spectrum as it travels. On impact, triggers Aria Detonation.
- **Aria Detonation**: Impact explosion structured as 3 concentric expanding rings, each a different color (inner: white, mid: random spectrum color, outer: complementary spectrum color). Each ring deals damage separately. Enemies hit by all 3 rings take bonus shatter damage.
- **Chromatic Scale**: Consecutive casts cycle the Aria Detonation through the chromatic scale (C-D-E-F-G-A-B). Each "note" changes the mid-ring color and subtly alters the detonation pattern (some rings are wider, some faster). Completing a full octave (7 consecutive casts) triggers Opus Detonation — all 7 colors detonate simultaneously in a massive prismatic explosion.
- **Dying Breath**: Below 30% HP, Chromatic Bolts gain double travel speed and Aria Detonations gain +50% radius. Visual: bolts gain black feather particles mixed into the rainbow.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `ChromaticBoltBody.fx` | Bolt with full-spectrum color cycling | SDF orb with color that cycles through HSV hue (0→360°) based on lifetime. Internal noise distortion for "living" feel. Bright center → prismatic edge. |
| `AriaDetonationRings.fx` | 3-ring structured explosion | Three expanding SDF rings, each with independent color uniform, independent expansion rate. White center flash. Rings have feathered edges. Uses additive blending. |
| `OpusDetonation.fx` | 7-color octave completion mega-explosion | All 7 chromatic scale colors detonating in overlapping rings simultaneously. Intense white center → rainbow bands. Screen shake. Brief chromatic aberration. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| ChromaticTrailParticle | Trail behind bolt, color-matched | Medium rainbow sparkle, 8-10 frame life, matches bolt's current hue |
| AriaRingGlintParticle | Spawns along each detonation ring circumference | Tiny glint matched to ring color, 4 frame life, fast radial motion |
| OpusFeatherBurstParticle | Massive burst from Opus Detonation | White feathers + rainbow sparkles intermixed, 30+ particles, 20 frame life |
| DyingBreathFeatherParticle | Black feathers mixed into low-HP bolts | Black feather despite rainbow bolt, sharp contrast, drifting, 25 frame life |

---

## 6. Feather of the Iridescent Flock (Summon)

### Identity & Musical Soul
Not one swan, but a flock — an iridescent crystalline minion that represents the corps de ballet. This summon conjures a crystal swan that attacks autonomously, launching shards and performing dive attacks. Each additional summon adds another swan to the flock, and they coordinate their attacks in formation, creating **visual formations** that echo ballet choreography.

### Lore Line
*"Alone, a swan is beautiful. Together, they are devastating."*

### Combat Mechanics
- **Iridescent Crystal Minion**: Summons a crystalline swan that floats near the player. Attack pattern cycles:
  - **Formation Flight**: 2s of passive circling (swans maintain elegant formation)
  - **Shard Volley**: Each swan launches 3 crystal shard projectiles at nearest enemy
  - **Dive Attack**: Swans take turns performing dive attacks (charges through enemy)
- **Flock Coordination**: Multiple swans fly in formation (V-formation with player at center). The more swans, the wider the V. Formation grants +5% damage per swan beyond the first.
- **Synchronized Dive**: When 3+ swans are summoned, they can all dive simultaneously (targeting different enemies). This creates a visual spectacle of coordinated attack.
- **Crystal Resonance**: When a swan is in formation (not attacking), it generates an iridescent aura. If 4+ swans are in formation, the aura becomes a Crystal Resonance field — allies in the field gain +3% crit chance.

### VFX Architecture Plan

#### Custom Shaders (2)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `IridescentCrystalBody.fx` | Crystal swan minion body shader | Pearlescent surface shader — white base with iridescent rainbow reflection based on angle/normal. Faceted crystal appearance with specular highlights. Subtle pulsing glow. |
| `FlockFormationLine.fx` | V-formation visual connection between swans | Thin line strip connecting swans in V-formation. White with prismatic shimmer. More visible when more swans present. Pulsing alpha. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| CrystalShardTrailParticle | Trail behind crystal shard projectiles | White-prismatic shard fragments, 3-4px, tumbling, 8 frame life |
| DiveAttackStreakParticle | Fast streak during dive attack | White streak with rainbow edge, 6 frame life, directional |
| FormationAuraGlintParticle | Ambient sparkles around formation swans | Tiny prismatic glints, slow orbit around each swan, 20 frame life |
| CrystalResonanceFieldParticle | Rising from resonance field area | Iridescent motes drifting upward, slow, 30 frame life |

---

## 7. Feather's Call (Special — Transformation Item)

### Identity & Musical Soul
The ultimate Swan Lake weapon — a rare 1% boss drop that transforms the player into a **mini Swan Lake boss**. This is the swan's final transformation — Odette becoming the swan itself. The player gains entirely unique attacks and movement while the transformation lasts. It's not a weapon you use casually — it's a spectacular moment.

### Lore Line
*"To truly call the feather is to become what you once sought to protect."*

### Combat Mechanics
- **Transformation**: Using the item begins continuous mana drain. Player transforms into a miniature Swan Lake boss with:
  - **Hover flight**: Free movement in all directions
  - **Feather Barrage** (auto): Continuously fires homing feather projectiles at nearby enemies
  - **Wing Gust** (left click): Directional wing gust that pushes enemies away + damages in cone
  - **Lake's Embrace** (right click): Creates a circular pearlescent lake zone at cursor. Enemies in zone are slowed 40%, allies heal 3 HP/s.
  - **Swan's Sacrifice** (at <20% mana): Final burst of prismatic energy expanding from player, dealing massive damage. Ends transformation.
- **Duration**: Lasts until mana depletes or player manually cancels. High mana cost/s.
- **Transformation VFX**: The player model is replaced with a custom sprite — large white-prismatic swan with iridescent wings. Feather particles constantly shed.

### VFX Architecture Plan

#### Custom Shaders (3)
| Shader | Purpose | Technique |
|--------|---------|-----------|
| `SwanTransformBody.fx` | Transformed player swan body rendering | Full body replacement shader. Pearlescent white body, iridescent wing surfaces that shift color with movement angle. Feathered edge alpha masking. Glowing core. |
| `LakeEmbraceZone.fx` | Healing/slowing lake zone AoE | Circular pearlescent surface (water caustics UV-scroll). Subtle prismatic shimmer. Healing pulse rings expanding from center. Very elegant. |
| `SwanSacrifice.fx` | Final burst on mana depletion | Expanding prismatic nova from player center. Intense white → rainbow → white ring. Screen flash. All remaining feather particles burst outward simultaneously. |

#### Particle System Plan
| Particle | Behavior | Visual |
|----------|----------|--------|
| TransformFeatherShedParticle | Constantly sheds from transformed swan | White-iridescent feathers, slow drift down/back, continuous spawning, 40 frame life |
| WingGustFeatherParticle | Directional cone burst from Wing Gust | Fast-moving white feather spray in cone, 15-20 per gust, 15 frame life |
| HomingFeatherProjectileParticle | Trail behind homing feather auto-attacks | Tiny white sparkle trail, 4 frame life |
| SacrificeFeatherExplosionParticle | Massive burst from Swan's Sacrifice | All available feather types in radial explosion, 50+ particles, 30 frame life |

---

## Cross-Theme Synergy Notes

### Swan Lake Theme Unity
All weapons share the white/black/prismatic palette with elegance and tragedy:
- **Call of the Black Swan**: Dark elegance — obsidian meeting prismatic fire
- **The Swan's Lament**: Sorrowful ranged precision with mournful halo detonations
- **Call of the Pearlescent Lake**: Liquid beauty — persistent water zones and geyser reflections
- **Iridescent Wingspan**: Fan convergence with full-spectrum iridescent wonder
- **Chromatic Swan Song**: The final aria — structured musical detonations in full chromatic scale
- **Feather of the Iridescent Flock**: Ballet corps — coordinated crystal swans in formation
- **Feather's Call**: Apotheosis — become the swan itself

### Visual Distinction Strategy
Despite sharing white-prismatic palette, each weapon has a distinct visual approach:
- **Black Swan** uses black-dominant with prismatic accents (dark elegance)
- **Lament** uses clean white shots with structured halo geometry
- **Pearlescent Lake** uses liquid caustic surfaces and water VFX
- **Wingspan** uses individual bolt color-cycling across the spectrum
- **Chromatic Swan Song** uses structured ring detonations with musical scale mapping
- **Iridescent Flock** uses crystalline/faceted aesthetics
- **Feather's Call** uses full-body transformation with constant feather shedding

### Musical Motifs
- **Feather physics**: Almost every weapon sheds feathers — black, white, or prismatic. Each weapon's feathers behave differently (drift, tumble, burst, home) to maintain uniqueness.
- **Formation/choreography**: Swan Lake is a ballet — weapons should feel choreographed. The Flock literally flies in formation. Chromatic Swan Song's detonations are structured like musical phrases.
- **Tragedy and sacrifice**: Dying Breath mechanic (Swan Song), Swan's Sacrifice (Feather's Call), Lament's mournful themes. Beauty born from loss.
- **Water/lake imagery**: Pearlescent Lake and Lake's Embrace tie back to the actual lake setting.
