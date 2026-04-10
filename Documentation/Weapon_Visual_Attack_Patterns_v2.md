# Weapon Attack Patterns — Orb Behavior Design Document

> **Constraint:** No new visuals. Every weapon fires the same **orb projectile** (a homing energy ball). What makes each weapon *feel* unique is how the orb **moves, splits, orbits, bounces, decelerates, accelerates, and interacts with enemies**. The orb template will eventually receive polished art — these patterns define the behavior that art will inhabit.
>
> **Existing movement vocabulary** (proven in codebase):
> - **Homing** — turn toward nearest enemy (configurable range/strength)
> - **Straight** — no tracking, raw velocity
> - **Arcing** — gravity-affected parabola
> - **Bouncing** — ricochet off tiles/enemies (configurable count)
> - **Orbiting** — circle a point (player, enemy, or another projectile)
> - **Spiral/Helix** — corkscrew path toward target
> - **Decelerating** — slow down over time (hover, mine behavior)
> - **Accelerating** — speed up over time (ramping momentum)
> - **Multi-phase** — switch behavior at threshold (time, distance, hit count)
> - **Splitting** — spawn child orbs on hit, timer, or distance
> - **Stationary zone** — stop moving, become persistent AoE
> - **Distance-scaling** — stats change based on distance traveled
> - **Sine-wave** — oscillate perpendicular to travel direction

---

## MOONLIGHT SONATA — *The Moon's Quiet Sorrow*
> Palette: Deep purple, vibrant blue, violet, ice blue, silver

### Incisor of Moonlight — Melee
**Current:** Fires 3 homing orbs per swing.  
**Pattern: Tidal Pendulum**  
Orbs fire in a tight 3-shot spread but use **sine-wave** flight (oscillating left-right as they travel forward). Each consecutive swing increases the sine amplitude — first swing is nearly straight, second swings wider, third swing orbs weave dramatically. On the 4th swing, all 3 orbs lose their homing and instead **reverse direction** (fly back toward the player) before re-acquiring the nearest enemy. Creates a visual push-pull rhythm — tidal.

### Eternal Moon — Melee
**Current:** 5-phase combo with crescent wave slash + homing orbs.  
**Pattern: Waxing Momentum**  
Phase 1-2 fire 1 orb each — slow speed (8f), gentle homing (0.04). Phase 3 fires 2 orbs — medium speed (12f), standard homing (0.08). Phase 4 fires 3 orbs — fast (16f), aggressive homing (0.12). Phase 5 fires 1 single orb at max speed (24f) with **no homing** — pure straight shot. The escalation from lazy drifting orbs to a committed straight missile mirrors waxing confidence. Additionally, Phase 5's orb **splits into 5 child orbs on hit**, each with gentle homing — the full moon shattering into fragments.

### Moonlight's Calling — Magic
**Current:** Channeled beam + bouncing light orbs.  
**Pattern: Prismatic Refraction**  
Fire 1 orb that travels straight until it hits a tile surface, then **splits into 3 child orbs at 120-degree angles** (each inheriting half the remaining timeLeft). Each child orb bounces once more and splits into 2 grandchild orbs. The result is a branching tree of progressively weaker orbs filling an area. Channeled mode: orb fires with **deceleration** (0.97x per frame), hovering near the target and dealing rapid contact damage — acts as a slow, sticky, persistent threat rather than a one-and-done.

### Resurrection of the Moon — Ranged
**Current:** 3 ammo types cycling (ricochet, pierce, artillery).  
**Pattern: Three Trajectories, One Orb**  
All 3 modes fire the same orb — behavior changes entirely:
- **Ricochet mode:** Orb bounces off tiles (3 bounces max), gaining +10% speed per bounce. Each bounce point leaves a **stationary zone** (0.5s, small radius) — lingering moonlight.
- **Comet mode:** Orb has **pierce 3** and **accelerates** (starting at 8f, capping at 28f). No homing. Pure momentum.
- **Artillery mode:** Orb fires with **heavy arc** (gravity 0.15f), high initial velocity upward. On landing, splits into **4 child orbs** that scatter outward with mild homing.

### Staff of the Lunar Phases — Summoner
**Current:** Goliath minion fires homing beams.  
**Pattern: Conductor's Arpeggio**  
Goliath fires orbs in 3-shot bursts with 5-frame gaps. The first orb homes normally. The second orb homes with a **delayed start** (15 frames straight, then homing kicks in). The third fires with an **initial offset angle** (30 degrees off-target) then homes. Result: the 3 orbs approach from 3 different angles, converging on the target at slightly different times — a musical arpeggio. If all 3 hit the same enemy within 20 frames, grant a stacking damage buff to subsequent bursts.

---

## EROICA — *The Hero's Symphony*
> Palette: Scarlet, crimson, gold, sakura pink

### Celestial Valor — Melee
**Current:** 4-phase combo fires homing orbs + Valor Charge flying blade.  
**Pattern: Heroic Crescendo**  
Phase 1: 1 orb, straight shot, no homing. Phase 2: 2 orbs, gentle homing (0.05). Phase 3: 3 orbs, standard homing (0.08), pierce 1 each. Phase 4: 1 large orb (1.5x scale) with **aggressive homing (0.14)** and **splitting on hit** — breaks into 3 child orbs that scatter then re-home. Each phase is a musical step up — direct, guided, persistent, explosive. Valor Charge fires a single orb with **spiral/helix** flight toward cursor position.

### Sakura's Blossom — Melee
**Current:** 3-phase petal dance, charge spawns 4 spectral turrets.  
**Pattern: Falling Petals**  
Each swing fires 3-5 orbs upward (positive Y velocity, slight random X spread) with **gravity applied** (0.08f). The orbs arc up, peak, then fall back down — like tossed petals. Enemies in the fall zone take hits. On the **ground contact**, each orb becomes a brief **stationary zone** (0.8s) that damages enemies walking through. Spectral turret charge: 4 orbs spawn at fixed positions around the player and sit as **stationary zones** for 5 seconds, periodically firing tiny straight child orbs at nearest enemies (every 40 frames).

### Triumphant Fractal — Magic
**Current:** Fractal recursive splitting (2 gen), 64-fragment finale.  
**Pattern: Recursive Division**  
Unchanged — this weapon already has excellent orb behavior architecture. Fire 1 orb with gentle homing. On hit, splits into 2 orbs at ±45 degrees with same homing. Each child splits into 2 grandchildren on hit. The 10-kill finale fires 1 massive orb that on hit splits into 8, each splitting into 8 = 64 fragments. Behavior is already the gold standard of orb splitting. **Keep as-is.**

### Piercing Light of the Sakura — Ranged
**Current:** Fast straight shots, every 8th shot is a stronger Culmination bullet.  
**Pattern: Staccato Burst**  
Normal shots: straight orbs, no homing, **extraUpdates=1** (double speed). Tight grouping. Every 8th shot: same orb but **pierce 3, 150% damage, and on each pierce, spawns 1 orbiting child orb** that circles the pierced enemy for 1 second before dissipating. If the 8th shot pierces 3 enemies, 3 orbiting child orbs exist simultaneously — visible marking of the shot's path.

### Funeral Prayer — Magic Beam
**Current:** Channeled sustained beam tracking cursor.  
**Pattern: Sustained Stream**  
Instead of a single beam, fires a rapid stream of tiny orbs (useTime 3) along the cursor direction. Each orb has a very short timeLeft (30 frames), slight random spread (±3 degrees), and **deceleration (0.96x)**. The stream creates a cone of decelerating orbs that pile up at medium range — dense damage at mid-range, sparse at long range. Holding on a single target causes orbs to accumulate in the area. Moving the cursor drags the stream, leaving floating clusters behind.

### Blossom of the Sakura — Ranged
**Current:** Ultra-fast fire (useTime 4), barrel heat system.  
**Pattern: Overheating Spray**  
Orbs fire straight at low heat. As barrel heat rises (ai[0]), each shot gains increasing **random angular offset** (±1° at cold → ±12° at max heat). At max heat, orbs additionally gain **acceleration** (start slow, speed up) — creating a "bullets catching up to each other" effect at distance. The Tracer Blossom (every 5th shot): one orb with **aggressive homing (0.12)** and a longer timeLeft (180 frames) — it chases persistently while the spray continues around it.

### Finality of the Sakura — Summoner
**Current:** Sakura spirit firing petal volleys, shield, Final Bloom supernova.  
**Pattern: Ring Volley**  
Spirit fires 6 orbs in a ring pattern (evenly spaced 60° apart) simultaneously — all with **gentle inward homing** (0.04) toward the nearest enemy. Creates a converging ring that tightens on the target. Shield mode: 5 orbs enter **orbiting** state around the player (150px radius, 2 second duration). Orbs in orbit have collision — they intercept enemy projectiles. On contact or expiry, orbiting orbs convert to **homing** and seek nearest enemy. Final Bloom: spawns 3 waves of 8 orbs each (24 total) in expanding outward rings, each wave 10 frames apart — a triple-pulse supernova using only orb spawn timing.

---

## LA CAMPANELLA — *The Ringing Bell, Virtuosic Fire*
> Palette: Black smoke, orange flames, gold highlights

### Dual Fated Chime — Melee
**Current:** 5-phase alternating L/R swings, Resonance Ring detonation at 5 stacks.  
**Pattern: Bell Pendulum**  
L-swing fires 1 orb angled 30° left of facing. R-swing fires 1 orb angled 30° right. Both use **deceleration (0.95x)** — they slow down and hover. When 2 orbs from opposite swings hover within 4 tiles of each other, they **attract and collide** — detonating as a combined AoE (1.5x damage, flame dust burst). This rewards rhythmic alternating attacks — the pendulum of L/R swings creating convergence points. At 5 stacks, all existing hovering orbs simultaneously home toward the nearest enemy at max speed.

### Ignition of the Bell — Melee
**Current:** 3-phase thrust (Ignition Strike, Tolling Frenzy, Chime Cyclone).  
**Pattern: Three Tolls**  
Phase 1: 1 orb fires straight upward (positive Y velocity) then falls with **gravity** (0.12f) — a vertical arc that slams down. Phase 2: 3 orbs fire in rapid succession in a tight horizontal spread — **straight, no homing, extraUpdates=1** (fast, punchy). Phase 3: 1 orb fires that immediately enters **deceleration to hover** (0.90x), then after 30 frames becomes a **stationary zone** with a 120px pull radius — enemies are dragged toward it for 2 seconds before it detonates.

### Fang of the Infinite Bell — Magic
**Current:** Bouncing bell orbs (2 or 10 bounces), stacking damage, lightning at 10+.  
**Pattern: Chain Bounce Amplification**  
Already well-designed. The orb bounces 2 times (normal) or 10 times (empowered). Each bounce spawns 1 echo child orb that homes toward the nearest enemy. At 10+ stacks, the primary orb gains **accelerating** behavior — each bounce adds +15% speed. The echo orbs also gain acceleration on their homing, making the late-bounce echos noticeably faster and more aggressive. **Keep existing architecture, only tune echo orb acceleration.**

### Piercing Bell's Resonance — Ranged
**Current:** Straight bullets, every 4th shot seeking crystal, 3+ markers = detonation.  
**Pattern: Mark-and-Detonate**  
Normal shots: straight orbs, no homing. The interesting part is the **every-4th-shot seeking crystal** — this orb uses **aggressive homing (0.12)**, targeting specifically MarkerTagged enemies (not nearest). The seeking crystal should gain **pierce 2** and **on-pierce, spawn 1 stationary zone** at the pierce point (0.5s duration, small radius). This makes detonation more powerful — the crystal's flight path leaves damaging markers in its wake. At 3+ actual markers on a single target, all stationary zones in a 300px radius detonate simultaneously.

### Grandiose Chime — Ranged
**Current:** Wide golden beam, Kill Echo chains, note mine placement.  
**Pattern: Beam + Sentries**  
The beam is a special case (not orb-based). The orb component is the **note mines**: orbs that fire with standard velocity, then **decelerate to hover** (0.93x) and become **proximity-detonation stationary zones** (arm time: 20 frames, detection: 120px). On detonation, each mine fires 7 child orbs in a spread (the existing note-spread pattern). Kill Echo chains: on-kill, the weapon fires 1 orb at the nearest un-killed enemy with **aggressive homing** — if that kills too, chain continues (max 3 chains). Simple but satisfying cascade.

### Symphonic Bellfire Annihilator — Ranged Launcher
**Current:** Crescendo waves + rockets, dual buff stacking.  
**Pattern: Arcing Ordnance**  
Rockets fire as orbs with **heavy arc** (gravity 0.12f), high initial upward velocity. On impact/proximity: splits into **4 child orbs** scattering outward with brief homing (60 frame timeLeft, 0.06 turn). Crescendo mode modifies the orbs: same arc trajectory but **no splitting** — instead, the orb explodes in a larger AoE (80px radius) and applies stacking debuff. The dual-buff mechanic comes from alternating between splitting (Bellfire) and big-AoE (Crescendo) modes. Symphonic Overture: fires both simultaneously — one arcing orb that splits AND one that detonates big.

### Infernal Chimes' Calling — Summoner
**Current:** 1-5 spectral bells in arc, sequential attacks, Harmonic Convergence.  
**Pattern: Staggered Barrage**  
Each spectral bell is a **stationary zone** that periodically fires 1 homing orb (every 45 frames, staggered across bells). With 5 bells, that's an orb every 9 frames — a constant stream from different positions. Bells are placed in an arc around the player. Harmonic Convergence: all bells fire simultaneously toward the same target — 5 orbs arriving from different angles within a 5-frame window. The damage bonus is for all 5 hitting the same enemy. Infernal Crescendo: each bell fires 3 rapid orbs instead of 1, turning the staggered stream into a burst flood.

---

## ENIGMA VARIATIONS — *The Unknowable Mystery*
> Palette: Void black, deep purple, eerie green flame

### Cipher Nocturne — Magic
**Current:** Scaffold orb (homing 0.08, 350f range).  
**Pattern: Tethered Snap-Back**  
Orb fires with homing and a **tether to the player** (invisible, mechanical — tracked via ai[]). On release of the fire button (or after 90 frames), the orb **reverses direction** and snaps back toward the player at 1.5x its outbound speed. Enemies hit on both the outbound AND return trip take bonus damage. The longer the orb travels outbound, the more damage the snap-back deals (distance-scaling). Player can fire multiple orbs that all snap back simultaneously on release — creating a converging wave from multiple angles.

### Dissonance of Secrets — Magic
**Current:** Scaffold orb.  
**Pattern: Growing Orbit**  
Orb fires with standard homing but **grows in scale** over its lifetime (0.5x at spawn → 2.0x at 120 frames). Its hitbox grows proportionally. Additionally, as it grows, it spawns 1 **orbiting child orb** every 40 frames (max 3 satellites). The satellites orbit the parent and damage enemies independently. On the parent's death (hit or timeout), all satellites release with **aggressive homing** toward nearest enemies. A single shot becomes a mini solar system.

### Fugue of the Unknown — Magic
**Current:** Scaffold orb.  
**Pattern: Voice Accumulator**  
Left-click fires an orb that enters **orbiting** state around the player (150px radius), max 5 orbiting. Each orbiting orb slowly increases its orbit speed. Right-click releases ALL orbiting orbs simultaneously — they switch to **spiral/helix homing** toward the cursor target, each with slightly different spiral width (based on how long they orbited). The stagger creates a visual spiral wave converging from different distances. Enemies hit by 3+ orbs from a single release take bonus damage — rewarding patience in accumulating before releasing.

### Tacet's Enigma — Ranged
**Current:** Scaffold orb.  
**Pattern: Delayed Echo**  
Orb fires straight (no homing). On enemy hit, the orb vanishes — but after a **1-second delay**, a **ghost copy** spawns at the impact point traveling in the exact same direction the original was going, with **pierce 2**. If the ghost hits another enemy, another ghost spawns after 1 second. Max 3 echoes. Creates a temporal chain where one shot can tag multiple enemies in sequence with eerie delayed repetition. The Paradox Bolt (every 4th) has a 0.5-second delay instead and spawns 2 ghost copies diverging at ±15°.

### The Silent Measure — Ranged Bow
**Current:** Scaffold orb (some unique splitting/homing already in code).  
**Pattern: Split on First Hit**  
Orb fires straight (fast, no homing). On first enemy contact, splits into **3 homing child orbs** (0.08 turn, 300f range) that seek different targets (each picks a unique NPC if available). Every 5th shot fires a Paradox Piercing orb with **pierce 4** and no splitting — but applies a stacking debuff that makes the target take +8% more damage per stack from the split orbs.

### The Unresolved Cadence — Melee
**Current:** Has VoidSpecialProj (decelerating expanding wave) and CadenceSpecialProj (stationary detonation).  
**Pattern: Expanding + Collapsing**  
Already somewhat unique. Reinforce: swing fires a normal homing orb. Right-click fires the expanding wave orb (decelerates from 16f to 0, scale grows 0.5→3.0 in 60 frames) — this is the "cadence" that resolves. Add interaction: if a normal homing orb flies *through* the expanding wave, it gains **+50% damage and +50% speed** — the wave is an amplifier zone. This rewards firing homing orbs through your own wave.

### The Watching Refrain — Summoner
**Current:** Scaffold orb for minion attacks.  
**Pattern: Teleporting Strikes**  
The minion doesn't fire orbs conventionally. Instead, the orb **spawns directly at the target's position** (offset by random ±20px) with 0 initial velocity and a very short timeLeft (15 frames). Appears, hits, vanishes. No flight time, no homing needed. The damage happens as sudden manifestations around the target. Every 60 frames, the minion fires a conventional homing orb as well — but this orb leaves **stationary zone child orbs** (small, 1s duration) along its flight path every 20 frames, creating a trail of lingering damage zones.

### Variations of the Void — Magic
**Current:** Scaffold orb (supposed to be 3 converging beams).  
**Pattern: Triple Convergence**  
Fires 3 orbs simultaneously — one straight, one offset +20° with **gentle left-curving** homing bias, one offset -20° with **gentle right-curving** homing bias. All three converge on the cursor position. If all 3 orbs are alive and within 3 tiles of each other, they detonate together in a combined AoE (3x single orb damage, 150px radius). If scattered (enemies blocked some), each detonates individually for normal damage. Rewards clean convergence on a single point.

---

## SWAN LAKE — *Grace Dying Beautifully*
> Palette: Pure white, black contrast, prismatic rainbow edges

### Call of the Black Swan — Melee
**Current:** Already well-developed — dual-polarity orbs, 3 modes, GPU trail.  
**Pattern: Already Unique — Keep As-Is**  
This weapon already has the best orb implementation in the mod. Black/white polarity, normal/empowered/grand jeté modes, primitive trail — it's the reference standard. **No changes needed.**

### Call of the Pearlescent Lake — Ranged
**Current:** Sine-wave wobble rockets with variant homing, splash zones on kill.  
**Pattern: Already Unique — Keep As-Is**  
Already has sine-wave flight, two behavioral modes (tidal/still waters), and spawns stationary splash zones on kill. **No changes needed.**

### Chromatic Swan Song — Magic
**Current:** Already unique — spiral wobble, rainbow-shifting, spawns AriaDetonation on every hit.  
**Pattern: Already Unique — Keep As-Is**  
Color-cycling, musical scale coloring (C-D-E-F-G-A-B), AriaDetonation spawning. **No changes needed.**

### Feather of the Iridescent Flock — Summoner
**Current:** Already unique — V-formation, 4-state (Formation→ShardVolley→DiveAttack→Return).  
**Pattern: Already Unique — Keep As-Is**  
Complex multi-phase minion with crystal shard sub-projectiles. **No changes needed.**

### Iridescent Wingspan — Magic
**Current:** 5-bolt fan with wing charge mechanic and empowered mode.  
**Pattern: Already Unique — Keep As-Is**  
Fan-spread, cursor-curving, empowered scale-up, Prismatic Convergence. **No changes needed.**

### The Swan's Lament — Ranged
**Current:** Already unique — feather shrapnel, Destruction Halo expanding ring, Lamentation stacks.  
**Pattern: Already Unique — Keep As-Is**  
8-way shrapnel, expanding halo ring, stack mechanic. **No changes needed.**

---

## FOUR SEASONS — *The Eternal Cycle*
> Palette: Cycling through all 4 season palettes

### Four Seasons Blade — Melee
**Current:** 4-season swing cycling with themed slash waves.  
**Pattern: Seasonal Orb Modifier**  
Each season modifies how the orb behaves (same orb, 4 behavior modes):
- **Spring:** Orb has **gentle homing** (0.04) and **splits into 2** on hit.
- **Summer:** Orb fires **straight with acceleration** (10f → 22f) — fast, punchy.
- **Autumn:** Orb has **gravity** (0.06f) and **high arc** — lobs over obstacles, hits from above.
- **Winter:** Orb fires with **deceleration** (0.94x) and becomes a **stationary zone** for 1.5s at hover point — area denial.

Crescendo (every 4 full cycles): fires all 4 orb types simultaneously — splitting, accelerating, arcing, and zone-creating all at once.

### Concerto of Seasons — Magic
**Current:** 4 seasonal spells cycling.  
**Pattern: Four Verses**  
Each verse fires its orb differently:
- **Spring Verse:** 1 orb with **orbiting child** (the existing 6-petal system). Child orbs shed from the parent every 20 frames. Parent + children all home gently.
- **Summer Movement:** 3 orbs in rapid burst (5-frame gaps), straight shots, **extraUpdates=1** — staccato fast damage.
- **Autumn Passage:** 1 orb with **distance-scaling** — gains +2% damage per tile traveled, trail darkens with distance.
- **Winter Finale:** 1 orb that on hit **splits into 6 child orbs** radiating outward. Each child orb has brief 0.5s timeLeft and deceleration — a brief frost explosion.
- **Grand Crescendo:** Fires all four simultaneously fused into 1 large orb with homing + splitting + speed + distance-scaling.

### Seasonal Bow — Ranged
**Current:** 4 cycling arrow types.  
**Pattern: Arrow Behavior Swap**  
Same orb/arrow, behavior swaps per season:
- **Spring Arrow:** **Splits into 3** on first hit (each child has gentle homing).
- **Summer Arrow:** **Pierces 3** enemies, gains +10% speed on each pierce.
- **Autumn Arrow:** On-kill, spawns **4 arcing child orbs** that rain down in the area (gravity 0.10f).
- **Winter Arrow:** On hit, spawns **1 stationary zone** lasting 2s (frost patch — slows enemies walking through).
- **Harmonized Volley (right-click):** Fires 1 of each season simultaneously in a tight spread — 4 orbs with 4 different behaviors. The target receives splitting, piercing, raining, and zone effects all at once.

### Vivaldi's Baton — Summoner
**Current:** 4 seasonal spirit minions, Symphony Coordination.  
**Pattern: Seasonal Minion Orbs**  
Each spirit fires orbs with its season's behavior:
- **Spring Spirit:** Fires homing orb that **spawns 1 child orb** on hit.
- **Summer Spirit:** Fires **accelerating** straight orb (8f → 18f).  
- **Autumn Spirit:** Fires orb with **gravity arc** — lob attack.
- **Winter Spirit:** Fires orb with **deceleration to hover** — becomes zone.
- **Symphony Coordination:** All 4 spirits fire at the same target simultaneously — the convergence of 4 different flight paths looks naturally varied because each orb behaves differently.

---

## SPRING — *Nature's Breath*
> Palette: Cherry blossom pink, spring green, pale bloom

### Blossom's Edge — Melee
**Current:** 3-phase petal combo, seeking crystals on crit.  
**Pattern: Budding Bloom**  
Phase 1: 1 orb, gentle homing (0.05). Phase 2: 2 orbs, gentle homing. Phase 3: 3 orbs, gentle homing. Each orb that hits spawns **1 stationary zone** (0.8s, small) — a bloom patch. If the player walks through a bloom patch, they receive a small heal (5 HP). Crits: the hitting orb doesn't die — it converts to a **SeekingCrystal** (aggressive homing 0.12, 600f range, 3s duration) that chases another enemy.

### Vernal Scepter — Magic
**Current:** Already has VernalBolt — splits into 4 homing petals after 30 frames.  
**Pattern: Already Unique — Enhance Timing**  
Keep the split-at-30-frames mechanic. Enhancement: the 4 child orbs should spread out in cardinal directions first (15 frames of outward drift, no homing) then engage homing. This creates a visible bloom pattern — orbs expand outward then converge on targets. Every 6th cast fires an empowered orb that splits into **8** instead of 4 and child orbs have +50% homing strength.

### Petal Storm Bow — Ranged
**Current:** Bloom arrows splitting into 3 homing petals.  
**Pattern: Split-on-Hit Volley**  
Arrow (orb) fires straight. On first enemy hit, splits into **3 homing child orbs** (gentle homing 0.06, 400f range). 15% chance the split produces a **healing flower** — a stationary zone that lasts 3s and heals the player 3 HP/s when standing in it. Every 8th shot fires a Spring Showers orb: fires upward first (high Y velocity) then arcs down — at apex, splits into **5 child orbs** that rain down with gravity (0.08f) in a spread area beneath.

### Primavera's Bloom — Summoner
**Current:** Flower sprite minions.  
**Pattern: Garden Sentries**  
Flower sprites hover in fixed positions relative to the player (not randomly orbiting). Each fires 1 homing orb every 50 frames. The orbs have **gentle homing** and on-hit leave a **stationary zone** (1s, tiny) — a pollen cloud that deals ambient damage. With 3+ sprites, the sprites link: if an orb from sprite A passes within 3 tiles of sprite B, the orb gains **+20% speed** (relay boost). This rewards smart sprite positioning.

---

## SUMMER — *Solar Fury*
> Palette: Sun gold, blazing orange, white-hot

### Zenith Cleaver — Melee
**Current:** Solar energy waves, Sunstroke debuff.  
**Pattern: Solar Prominence**  
Each swing fires 1 orb with **high initial upward velocity** and **gravity** (0.10f) — it arcs up then crashes down. The arc creates a natural lobbing attack that clears obstacles. Where it lands, it creates a **stationary zone** (1.5s, 80px radius) — a sunspot. Enemies standing on sunspots take continuous Sunstroke damage. Every 3rd finisher: the orb is 2x scale, arcs higher, and on landing **splits into 3 arcing child orbs** that fly outward and create their own smaller sunspots.

### Solstice Tome — Magic
**Current:** Already has SolarOrbProjectile — rapid fire, 5-orb corona orbit.  
**Pattern: Already Has Orbiting — Enhance**  
Keep the 5-orb corona orbit. Enhancement: the **Sunbeam Charge** (right-click) should collect all existing orbiting child orbs and fire them as a concentrated burst (up to 5 orbs in a tight stream, each with **acceleration**). The more orbs collected, the stronger the beam. Normal left-click rebuilds the corona. Creates a build-then-spend rhythm.

### Solar Scorcher — Ranged
**Current:** Continuous flamethrower stream.  
**Pattern: Rapid Stream**  
Fires orbs at useTime 3 (rapid stream) with **short timeLeft (40 frames)**, slight random angular spread (±4°), and **gentle deceleration (0.97x)**. Creates a cone of dissipating orbs. Heatwave pulse (every 2s): fires 1 orb with **no deceleration, pierce 5, and 2x size** that punches through the stream. Solar Buildup mechanic: after 3s sustained fire, all orbs gain **+30% speed** and **the spread tightens to ±1°** — the stream focuses.

### Solar Crest — Summoner
**Current:** Sun spirit minions with periodic flares.  
**Pattern: Orbiting Emitters**  
Spirits orbit the player. Each spirit fires 1 straight orb along its current movement tangent (the direction it's currently traveling in its orbit). This creates a natural rotating shooter pattern — orbs spray outward in a wheel as spirits orbit. The periodic solar flare is a special emission: all spirits fire simultaneously toward the cursor target with **aggressive homing**. With 3+ spirits, flare orbs **converge** — arriving near-simultaneously from different orbital positions.

---

## AUTUMN — *The Harvest's Weight*
> Palette: Harvest orange, walnut brown, golden canopy, decay purple

### Harvest Reaper — Melee
**Current:** 4-phase scythe combo, decay bolts, crescent waves.  
**Pattern: Reaping Arcs**  
Phase 1: 1 orb, straight. Phase 2: 4 orbs in a **wide fan** (±30° spread from swing direction), all with **deceleration** (0.95x) — they spread then slow. Phase 3: 1 orb with **pierce 3** and **distance-scaling** (+3% damage per tile traveled) — rewards letting it fly far. On-kill: the killing orb spawns a soul wisp child orb with **orbit behavior** around the player (120px, 3s) that gradually spirals inward and is absorbed for a small heal.

### Withering Grimoire — Magic
**Current:** Already has DecayBoltProjectile — entropic fields on impact.  
**Pattern: Entropic Fields — Keep and Layer**  
Keep the on-hit stationary zone behavior. Enhancement: orbs should use **deceleration** flight (0.96x) — they slow as they travel, as if decaying. The **Withering Wave** (charge) fires 1 orb straight with **pierce all** (penetrate = -1) and **distance-scaling** (gains +5% more damage the further it travels but loses 1% speed per frame). It eventually stops mid-air and becomes a **large stationary zone** wherever it stalls. Life drain: on-hit, 5% of damage dealt heals player (code-only — no new visual).

### Twilight Arbalest — Ranged
**Current:** Already has TwilightBolt — distance-scaling damage + gradient color shift.  
**Pattern: Already Unique — Keep As-Is**  
Distance-scaled damage is already one of the most unique orb behaviors in the codebase. The orb gains damage the further it flies, with a visual gradient shift from purple to orange. **No changes needed.** Enhancement option: every 6th shot fires a **Harvest Moon orb** — 2x scale, arcing (gravity 0.08f), and on impact **splits into 4 homing child orbs**.

### Decay Bell — Summoner
**Current:** Harvest wraith minions.  
**Pattern: Tolling Barrage**  
Wraith minions fire 1 orb every 50 frames (staggered). Each orb has **deceleration** and on timer-death (not hit) becomes a **stationary zone** for 1s — an entropic field. Orbs that hit enemies normally deal damage and vanish. This creates a dual behavior: missed shots become area denial, hits deal damage. With 3+ wraiths, periodic toll: all wraiths fire simultaneously toward the same target with **aggressive homing** — a convergent barrage. On-kill, wraith orbs gain +15% damage for 5 seconds.

---

## WINTER — *The Frozen Embrace*
> Palette: Ice blue, frost white, crystal cyan, glacial purple

### Glacial Executioner — Melee
**Current:** Already has IcicleBolt and AvalancheWave.  
**Pattern: Freeze and Shatter**  
Phase 1-2: fire homing orbs (0.06 turn) that apply **stacking Frostbite debuff**. Phase 3 fires a large AvalancheWave (decelerating, growing-scale slash). At 5 Frostbite stacks, the next orb hit causes **Freeze** (1s stun). Frozen enemies hit by another orb take **Shatter** damage (2x). The reward loop: stack debuffs with homing orbs → freeze → exploit with the next orb. Phase 4 Finisher fires 3 orbs simultaneously — one straight, one homing, one arcing — different angles converging.

### Permafrost Codex — Magic
**Current:** Already has PermafrostBolt — stacking frostbite, aurora trail, orbiting runes.  
**Pattern: Already Unique — Enhance Zone**  
Keep orbiting frost runes and aurora trail. Enhancement: the **Ice Storm** charge fires 5 orbs simultaneously in a tight spread upward — all have **heavy gravity** (0.15f) and rain down in a cluster. Each landing point becomes a **stationary zone** (2s, frost patch) that slows enemies. Overlapping zones stack the slow effect. The **Permafrost Barrier** (right-click): spawns 8 orbs in a tight ring around the player in **orbit mode** (100px radius). They orbit for 5 seconds, intercepting anything that crosses the ring.

### Frostbite Repeater — Ranged
**Current:** Already has IcicleBolt — pierce 3, Hypothermia stacking.  
**Pattern: Already Unique — Keep Stacking**  
Pierce 3 + Hypothermia stacking is already a good behavioral identity. Enhancement: the **Blizzard Barrage** (right-click) fires 7 orbs with slight random velocity randomization (±10% speed, ±8° angle). Each orb has **standard homing** but the randomization makes them approach from slightly different trajectories — a chaotic swarm rather than a neat cluster. At 5 Hypothermia stacks = Freeze, making the Blizzard Barrage naturally build toward a freeze.

### Frozen Heart — Summoner
**Current:** Frost sentinel minions.  
**Pattern: Sentinel Orbit + Frost Corridors**  
Sentinels orbit the player in a **figure-8 pattern** (lemniscate). Each sentinel fires homing orbs normally. Enhancement: orbs leave a **frost trail** — not a new visual, just a mechanical corridor tracked via position history. Subsequent orbs that pass through a previous orb's trail gain **+30% velocity** (slipstreaming through frozen corridors). Crits cause the orb to spawn a **stationary zone** at the impact point (1s, frost patch). With 3+ sentinels, synchronized attack: all sentinels fire toward the cursor target simultaneously.

---

## FATE — *The Celestial Symphony of Destiny*
> Palette: Black void, dark pink, bright crimson, celestial white

### The Conductor's Last Constellation — Melee
**Current:** 3-phase combo, fires 3 homing orbs per swing.  
**Pattern: Measure Markers**  
Phase 1 (Downbeat): 3 orbs fire outward with **deceleration to hover** (0.92x). They slow down and hang in the air for 2 seconds. Phase 2 (Crescendo): 3 new orbs fire — these home toward the *hovering Phase 1 orbs* (not enemies). On contact with a hovering orb, both detonate as AoE. Phase 3 (Forte): 3 orbs fire with **aggressive homing (0.14)** at enemies — anything left standing after the detonations. Creates a 3-phase: place → detonate → clean up.

### Opus Ultima — Melee
**Current:** Already has OpusEnergyBallProjectile — 3 modes (EnergyBall splits into 5 Seekers).  
**Pattern: Already Unique — Keep As-Is**  
3-mode orb (parent → 5 seekers → crystal shards on hit), movement-based combo identity, +5% damage stacking per cycle. **No changes needed.** This is already one of the strongest orb-behavior weapons.

### Requiem of Reality — Melee
**Current:** 4-movement combo, seeking music notes, spectral blades.  
**Pattern: Movement Dynamics**  
Each of the 4 movements fires orbs with different **speed profiles**:
- **Adagio (slow):** 1 orb at speed 6, pierce 2 — lingers in the fight space, touching multiple enemies.
- **Allegro (fast):** 3 orbs at speed 20, no pierce — rapid fire, each hits one target hard.
- **Scherzo (playful):** 2 orbs with **bouncing** (3 bounces) — unpredictable, filling the space.
- **Finale:** 1 orb that combines behaviors — speed 12, pierce 1, and on second hit **splits into 4 child orbs** (1 slow, 1 fast, 2 bouncing) — a reprise of all previous movements.

### Fractal of the Stars — Melee
**Current:** Already has FractalOrbitBlade — orbits player, fires beams, 10s duration.  
**Pattern: Already Unique — Keep As-Is**  
Orbiting blades that fire sub-projectiles. Star Fracture recursive explosion. **No changes needed.**

### Symphony's End — Magic
**Current:** Already has SymphonySpiralBlade — helix toward cursor, shatters into 4 fragments.  
**Pattern: Already Unique — Keep As-Is**  
Helix/corkscrew flight + fragment splitting. **No changes needed.**

### The Final Fermata — Magic
**Current:** Scaffold orb disguised as orbiting swords.  
**Pattern: Fermata Hold**  
Orbs fire with standard homing but on hit, instead of dying, they enter **orbit mode** around the struck enemy (80px radius, 3s). Orbiting orbs deal periodic contact damage. Max 6 orbs orbiting simultaneously. Every 90 frames, all orbiting orbs **release** — they fly outward in radial directions, then re-home to the nearest enemy. Sustained channeling increases orb damage over time (+10%/second, max 5x). The fermata: hold, sustain, crescendo.

### Resonance of a Bygone Reality — Ranged
**Current:** Already has unique bullet + spectral blade every 5th hit.  
**Pattern: Echo Shots**  
Orbs fire straight (fast, useTime 6). Every 5th hit spawns a **larger orb** with aggressive homing that targets a *different enemy* than the one hit. This is the "spectral blade" — but mechanically it's just a bigger, meaner orb with retargeting logic. The Bygone Resonance combo: if the echo orb kills its target, it spawns *another* echo orb — a chain of the past repeating. Max 3 chains. Every 10th combo: brief invulnerability frame (existing mechanic).

### Light of the Future — Ranged
**Current:** Already has LightAcceleratingBullet — 6→42 speed, distance-scaling damage.  
**Pattern: Already Unique — Keep As-Is**  
Accelerating from 6 to 42 speed with damage scaling is one of the most distinctive orb behaviors. Every 3rd shot fires 3 homing rockets (LightCosmicRocket with spiral motion). **No changes needed.**

### Destiny's Crescendo — Summoner
**Current:** Cosmic deity minion with 4-phase escalation.  
**Pattern: Escalating Fire Rate**  
The minion fires orbs at increasing rates per phase:
- **Pianissimo:** 1 orb every 90 frames, gentle homing (0.04).
- **Piano:** 1 orb every 60 frames, standard homing (0.08).
- **Forte:** 2 orbs every 50 frames, standard homing.
- **Fortissimo:** 3 orbs every 40 frames, aggressive homing (0.12).

Phase transitions happen at damage thresholds. The visual escalation is purely in quantity and aggression — same orb, more of them, more relentless.

### Coda of Annihilation — Melee (Zenith-class)
**Current:** Throws spectral weapon copies with homing.  
**Pattern: Multi-Weapon Barrage**  
Each swing summons 2-3 orbs that home toward enemies. What makes it unique: each orb **inherits a random behavior from a different Fate weapon's pattern** — one might accelerate (Light of the Future), one might spiral (Symphony's End), one might orbit-then-release (Final Fermata), one might split (Opus Ultima). The randomization creates unpredictable but consistently devastating volleys. At full power, the screen fills with orbs behaving in 5+ different ways simultaneously — chaos unified by a single crimson palette.

---

## NACHTMUSIK — *Golden Starlit Melodies*
> Palette: Golden, dark purple

### Twilight Severance — Melee
**Current:** Has NocturnalBladeProjectile (gentle homing) + VoidRiftProjectile (stationary zone).  
**Pattern: Slash + Rift Combo**  
Normal swing: fires 1 homing orb (existing NocturnalBlade — 400f range, 0.04 turn, pen 2). Right-click dash: fires 1 orb that becomes a **stationary zone** lasting 1.5s and 80px radius (existing VoidRift behavior). The combo: dash through enemies to place rifts, then slash to fire homing orbs that pass through the rifts. Orbs that pass through a VoidRift gain **+40% damage and +50% speed** — the rift amplifies them. Rewards dashing first to set up amplifier zones, then slashing to exploit them.

### Starweaver's Grimoire — Magic
**Current:** Scaffold orb.  
**Pattern: Constellation Web**  
Orb fires with standard homing but on hit (or at max range), converts to a **stationary zone** (node, 3s duration). All active nodes within 300px are **tethered** (tracked in ai[] pairs). When an enemy crosses a tether line, it takes damage. Max 8 nodes. Right-click: all active nodes fire 1 homing orb each simultaneously toward the cursor target — up to 8 converging orbs from different positions. Creates a place-nodes-then-activate rhythm.

### Serenade of Distant Stars — Ranged
**Current:** Scaffold orb.  
**Pattern: Rhythm Stacking**  
Orb fires with standard homing. Each consecutive hit within 2 seconds increases a **Rhythm counter** (max 5). At each stack level, the orb gains behavioral upgrades:
- Stack 1: +10% homing strength.
- Stack 2: +15% speed.
- Stack 3: pierce 1.
- Stack 4: on-hit spawns 1 child orb with half stats.
- Stack 5: all orbs gain **aggressive homing (0.12)** and 1.3x scale.

Missing for 2 seconds resets to 0. Rewards sustained accurate fire.

### Requiem of the Cosmos — Magic
**Current:** Scaffold orb.  
**Pattern: Gravity Well Shots**  
Normal orbs fire with homing. Every 3rd shot fires a **gravity well orb** — same orb but with **deceleration to hover** (0.90x). Once hovering, it pulls nearby enemy projectiles and NPCs toward it gently for 2s, then detonates as AoE. The gravity well orb also pulls the player's own subsequent regular orbs, causing them to curve toward the well — natural convergence. Every 10th shot: the gravity well is 2x radius (Event Horizon) and lasts 3s.

### Nocturnal Executioner — Melee
**Current:** Already has NocturnalBladeProjectile (gentle homing) + VoidRiftProjectile.  
**Pattern: Day/Night Behavior Shift**  
Same orb, different behavior based on time of day:
- **Daytime:** Fires 5 orbs in a spread, each with **gentle homing** and short timeLeft (60 frames) — a quick bright burst.
- **Nighttime:** Fires 1 orb with **aggressive homing (0.14)**, long timeLeft (240 frames), and **on-hit spawns a stationary zone** (damage marker) lasting 3s — a relentless hunter that leaves marks.

The day version is quantity/burst, the night version is quality/sustained. Right-click dash fires a VoidRift stationary zone regardless of time.

---

## DIES IRAE — *Hell's Retribution Flames*
> Palette: White, black, crimson

### Wrath's Cleaver — Melee
**Current:** Scaffold orb (IgnitedWrathBall).  
**Pattern: Wrath Escalation**  
4-phase combo, each phase escalates the orb:
- Phase 1: 1 orb, straight, no homing — the warning.
- Phase 2: 2 orbs, mild homing (0.06) — judgment approaches.
- Phase 3: 3 orbs, standard homing (0.08), pierce 1 — judgment weighs.
- Phase 4: 1 orb, **aggressive homing (0.14)**, on-hit **splits into 8 child orbs** radiating outward — judgment rendered.

Right-click dash: fires 1 orb straight ahead at 2x speed with **pierce all** (penetrate = -1) and short timeLeft (40 frames) — a quick decisive strike through everything.

### Staff of Final Judgment — Magic
**Current:** Scaffold orb (FloatingIgnitionProjectile, BlazingShardProjectile).  
**Pattern: Sentencing Mines**  
Orb fires with standard velocity then **decelerates to hover** (0.93x), becoming a proximity mine. Arms after 20 frames. Detection range: 120px. On trigger: detonates as AoE and fires **3 homing child orbs** toward nearby enemies. Max 5 mines active. If 3+ mines detonate within 1 second of each other, trigger **Judgment Storm**: each detonation fires 5 child orbs instead of 3 (chain reaction amplification). Right-click: fires 1 orb straight with no deceleration as a direct damage shot.

### Sin Collector — Ranged
**Current:** Scaffold orb (SinBulletProjectile).  
**Pattern: Sin Accumulation Economy**  
Orbs fire straight (fast, no homing) — useTime 5 rapid fire. Each hit adds 1 Sin stack (tracked on player). Expenditure tiers grant one powerful modified orb:
- **Penance (10 sins):** 1 orb with pierce 3, 1.5x damage, slight homing.
- **Absolution (20 sins):** 1 orb with pierce all, 2x damage, **accelerating** (8f → 30f).
- **Damnation (30 sins):** 1 orb at 3x scale, 3x damage, **aggressive homing (0.14)**, on-hit spawns **stationary zone** (3s, 150px, continuous damage) — a lingering pyre.

Normal fire builds, expenditure rewards patience with escalating power.

### Wrathful Contract — Summoner
**Current:** Scaffold orb (JudgmentFlameProjectile).  
**Pattern: Blood-Bound Minion**  
Demon minion fires homing orbs (standard). The Blood Contract mechanic (1 HP/s drain, 5% heal on hit) is code-only, no visual change needed. Differentiation: the demon fires orbs with **ramping fire rate** — starts at 1 orb per 60 frames, accelerates to 1 per 30 frames over 10 seconds. After 3 kills: enters **Frenzy** — fire rate doubles for 5 seconds, orbs gain **pierce 1**. Below 10% player HP: **Breach** — demon fires 1 massive orb (2x scale) with **aggressive homing** every 2 seconds + standard fire.

### Harmony of Judgment — Summoner
**Current:** Scaffold orb (EclipseOrbProjectile).  
**Pattern: Autonomous Triad**  
Summons 3 judgment sigils (stationary). Each sigil cycles through 3 phases autonomously:
- **Scan:** Sigil targets nearest enemy, fires 1 homing orb (standard) every 60 frames.
- **Judge:** Fires 2 orbs per 60 frames, both homing toward the same target. After 3 hits on same enemy, transitions to Execute.
- **Execute:** Fires 1 large orb (1.5x scale) with **aggressive homing (0.14)** and **on-hit AoE** (100px). After execution, returns to Scan.

**Collective Judgment:** If 2+ sigils are in Execute phase on the same target, their execute orbs deal 2x damage each. **Harmonized Verdict:** If all 3 sigils execute simultaneously, the combined orb is 2x scale with 5x damage.

---

## CLAIR DE LUNE — *Shattered Time, Blazing Clocks*
> Palette: Dark red, vibrant gray, white

### Temporal Piercer — Melee
**Current:** Scaffold orb (DriveGearProjectile).  
**Pattern: Temporal Echo**  
Fires 1 orb with standard homing. On enemy hit, the orb dies — but after a **0.5-second delay**, a ghost copy spawns at the hit position traveling in the *opposite direction* (back toward the player) with homing toward the nearest enemy it passes. The return-trip orb deals 60% damage. Right-click charge: fires 1 orb with **aggressive homing (0.14)** and **on-hit creates a stationary zone** (1.5s, 100px radius) — a temporal rift at the strike point.

### Starfall Whisper — Ranged
**Current:** Scaffold orb (TemporalArrowProjectile).  
**Pattern: Delayed Replay**  
Orb fires straight (fast). On enemy hit, creates a **Temporal Fracture** at the impact point — a marker (tracked in GlobalNPC). After 1.5 seconds, the fracture replays: spawns a copy of the orb at the fracture point, traveling in the same direction as the original, dealing 75% damage. If the replay orb hits a different enemy, it creates its *own* fracture — chaining replays. Max 3 replay generations. Right-click: fires 5 orbs in a spread simultaneously — each can create its own fracture chain.

### Requiem of Time — Magic
**Current:** Scaffold orb (SecondBoltProjectile).  
**Pattern: Dual-Zone Caster**  
Left-click: fires 1 orb with standard homing. On hit (or max range), spawns a **Forward Zone** — stationary AoE (2s) that accelerates allies and increases all projectile speeds by +30% within. Right-click: fires 1 orb identical in behavior. On hit (or max range), spawns a **Reverse Zone** — stationary AoE (2s) that slows enemy movement by -40%. If a Forward and Reverse zone overlap, the overlap area deals **2x damage** per tick (Temporal Paradox). Weapons fire builds a tactical zone map.

### Orrery of Dreams — Magic
**Current:** Scaffold orb (DreamSphereProjectile).  
**Pattern: Triple Orbit Engine**  
3 orbs spawn and enter **orbit mode** around the player at 3 different radii: Inner (60px, fast orbit), Middle (120px, medium), Outer (180px, slow). Each orbiting orb fires 1 homing child orb at different rates: Inner every 30 frames (fast/small), Middle every 60 frames (medium), Outer every 90 frames (slow/large 1.3x scale). Right-click: all 3 orbs release from orbit toward the cursor target with **aggressive homing** — 3 converging orbs from different distances. Dream Alignment (every 12s): all 3 align on the same angle and fire simultaneously toward one target — triple-hit convergence.

### Midnight Mechanism — Ranged
**Current:** Scaffold orb (MechanismBulletProjectile).  
**Pattern: Spin-Up Gatling**  
5-phase escalating fire rate using the same orb:
- Phase 1: 1 orb every 20 frames. Straight, no homing.
- Phase 2: 1 orb every 12 frames. Straight, mild spread (±3°).
- Phase 3: 1 orb every 8 frames. Homing (0.04), spread (±5°).
- Phase 4: 1 orb every 5 frames. Homing (0.06), spread (±7°).
- Phase 5: 1 orb every 3 frames. Homing (0.08), spread (±10°).

Each phase activates after sustained fire threshold. Stopping resets to Phase 1 after 1 second. At Phase 5, the sheer volume of homing orbs saturates the screen. **Every 12th orb** in Phase 5 is 2x scale with 10x damage (**Midnight Strike**).

### Lunar Phylactery — Summoner
**Current:** Scaffold orb.  
**Pattern: Soul-Linked Sentinel**  
Crystal sentinel minion orbits the player and fires standard homing orbs. Unique mechanic: **Soul-Link** — as the player takes damage, the sentinel's orb damage increases proportionally (lower player HP = more orb damage, up to 2x at 20% HP). The orb's homing strength also scales: gentle at full HP (0.04), aggressive at low HP (0.14). Creates natural risk-reward. The sentinel's beam attack (every 60 frames): fires 3 orbs in rapid burst (5-frame gaps) all targeting the same enemy — a focused punishment volley.

### Automaton's Tuning Fork — Summoner
**Current:** Scaffold orb.  
**Pattern: Frequency Modes**  
Automaton fires homing orbs. Right-click cycles through 4 frequencies, each modifying the orb behavior:
- **Frequency A:** Orbs gain **pierce 2**, lose 20% speed.
- **Frequency C:** Orbs gain **+40% speed**, lose homing (straight shots).
- **Frequency E:** Orbs gain **splitting** (on-hit spawns 2 child orbs), deal 60% damage.
- **Frequency G:** Orbs become **slow + decelerating**, becoming stationary zones on expiry (1.5s).

**Perfect Resonance:** cycling through all 4 frequencies within 10 seconds grants a 5-second buff where orbs have ALL 4 properties simultaneously.

### Gear-Driven Arbiter — Summoner
**Current:** Scaffold orb.  
**Pattern: Verdict Stacking**  
Construct minion fires homing orbs. Each hit on the same enemy adds 1 Verdict stack (tracked per NPC, max 8). At each stack threshold, the orb's behavior against that target upgrades:
- 2 stacks: orbs home more aggressively (0.06 → 0.10).
- 4 stacks: orbs gain pierce 1 (pass through the stacked target to hit behind).
- 6 stacks: orbs gain +30% speed against the stacked target.
- 8 stacks: next hit is **Arbiter's Verdict** — 5x damage, resets stacks.

Stacks decay 1 per 3 seconds if not refreshed. Focus-fire is rewarded.

### Cog and Hammer — Ranged Launcher
**Current:** Scaffold orb (ClockworkBombProjectile).  
**Pattern: Ticking Bombs**  
Orb fires with **arc/gravity** (0.10f). On landing (tile contact), orb doesn't die — it becomes a **proximity mine** (arm time 15 frames, detection 100px, 3-tick countdown). Each tick makes the orb pulse larger (1.0x → 1.2x → 1.5x scale). On third tick: detonation AoE (120px). If no enemy triggers it, self-detonates after 5 seconds. Right-click: fires a sticky orb that attaches to the first enemy hit and ticks on them (guaranteed detonation). **Master Mechanism** (charge): fires 1 orb that on landing splits into **4 sub-mines** scattering in cardinal directions.

### Clockwork Harmony — Melee
**Current:** Scaffold orb (DriveGearProjectile).  
**Pattern: Temporal Dilation Blade**  
Swing fires 1 orb with standard homing. What's unique: the orb's **speed oscillates** — it cycles between fast (20f) and slow (6f) every 20 frames, like a ticking clock's second hand. The speed oscillation makes it weave unpredictably toward the target. Right-click charge: fires 1 orb that creates a **200px stationary zone** lasting 2s — all enemies inside move at half speed (temporal distortion). Alternatively, fires **12 orbs in a radial burst** (one at each clock position) — each orbits outward from center, bounces once off the first tile hit, then returns inward.

---

## ODE TO JOY — *The Eternal Symphony Garden*
> Palette: Monochromatic black, white, prismatic chromatic

### Thornbound Reckoning — Melee
**Current:** Scaffold orb.  
**Pattern: Bouncing Thorns**  
Swing fires 1 orb with **3 bounces** (off tiles/enemies). Each bounce increases damage by +20% (compounding). Right-click charge: fires 3 orbs simultaneously, each with **2 bounces** and slightly different initial angles (±10°, center, -10°). The 3 bouncing orbs fill the space unpredictably. If 2+ orbs hit the same enemy within 10 frames, bonus damage (thorn convergence).

### Rose Thorn Chainsaw — Melee
**Current:** Scaffold orb.  
**Pattern: Rapid Stream Saw**  
Swing fires 3 orbs in rapid burst (3-frame gaps) in a tight cone (±5°). Each orb has short timeLeft (40 frames) and **no homing** — pure directional. Right-click empowerment aura: for 5 seconds, all orbs gain **pierce 2** and **slight outward spread** (±8°) — the "saw" widens. At point-blank range, all 3 orbs hit the same enemy — concentrated. At mid-range, they spread to cover width. Simple, brutal, positional.

### The Gardener's Fury — Melee Axe
**Current:** This is actually a swing weapon (ExobladeStyleSwing), not orb-based.  
**Pattern: Seed Planting**  
Right-click fires 5 orbs straight **downward** (high gravity 0.20f) at target positions. Each orb, on tile contact, becomes a **stationary zone** lasting 3s (a planted seed — deals damage in small radius). After 1.5s, each zone fires 1 **homing child orb** upward toward the nearest enemy — the seed blooms. If an enemy dies over a zone, the zone's child orb is 2x scale.

### Elysian Verdict — Magic
**Current:** Scaffold orb (AnthemGloryProjectile, in practice).  
**Pattern: Mark and Detonate**  
Orb fires with gentle homing (0.06). On hit, applies **Elysian Mark** (tracked per NPC, max 3 tiers). Tier 1: target takes +10% orb damage. Tier 2: +20% and orbs home on this target from 50% further range. Tier 3: +30% and next orb hit **detonates** all marks — AoE explosion dealing stored mark damage. Below 25% player HP (**Paradise Lost**): orb gains 2x scale, homing aggressive (0.14), and each hit applies 2 mark tiers instead of 1.

### Hymn of the Victorious — Magic
**Current:** Scaffold orb.  
**Pattern: Four-Verse Cycle**  
Cycles through 4 verse types, each modifying the orb:
- **Exordium (Opening):** 1 orb, slow (8f), gentle homing — the dignified start.
- **Rising:** 2 orbs, medium speed (14f), standard homing — building.
- **Apex:** 3 orbs, fast (18f), aggressive homing (0.12) — the peak.
- **Gloria:** 1 orb, **pierce all**, **accelerating** (8f → 24f), **2x scale** — the triumph.

At 3+ Hymn Resonance stacks (from completing cycles), all verses gain +1 orb count (2, 3, 4, and 2 respectively).

### Anthem of Glory — Magic Beam
**Current:** Scaffold orb.  
**Pattern: Crescendo Stream**  
Channeled rapid-fire orbs (useTime 4). Damage and size scale with channel duration: 1x at start → 2x after 5 seconds. Orb speed also increases (10f → 18f). Every 2 seconds of channeling, fire 1 **homing child orb** (Glory Note) that seeks a different target than the main stream — spreading collateral damage. At 3 kills during channeling, trigger **Victory Fanfare**: all active Glory Notes detonate simultaneously as AoE, and channel damage resets to 1.5x (partial reset — stronger restart).

### Thorn Spray Repeater — Ranged
**Current:** Scaffold orb (ThornSprayProjectile).  
**Pattern: Accumulation Burst**  
Rapid-fire orbs (useTime 5), straight, no homing. Each hit embeds a thorn (tracked per NPC, max 25). Normal orb behavior — just straight fast shots. At 25 stacks on any single enemy, all thorns **detonate simultaneously** — deals stored damage as AoE (affects nearby enemies too). Max DPS comes from focusing one target to 25, then the detonation splashes to clean up. **Bloom Reload** every 36 shots: next 6 orbs gain **+50% damage**, **homing (0.08)**, and 1.3x scale.

### Petal Storm Cannon — Ranged Launcher
**Current:** Scaffold orb (PetalBombProjectile).  
**Pattern: Cluster Bombs**  
Orb fires with **arc/gravity** (0.10f). On impact: splits into **3 child orbs** scattering outward with brief homing (0.04, 45-frame timeLeft). Every 3rd shot: **Hurricane Shot** — fires 1 orb that on impact creates a **stationary zone** (3s, 150px radius, pulling nearby enemies inward gently). Child orbs from subsequent cluster shots that land inside the Hurricane zone get +50% damage.

### The Pollinator — Ranged
**Current:** Scaffold orb (PollinatorProjectile).  
**Pattern: Spreading Contagion**  
Orb fires with gentle homing (0.06). On hit, applies **Pollinated** DoT (1% HP/s, 5s). If a Pollinated enemy is within 100px of a non-Pollinated enemy, the DoT **spreads** (code transfer, no new orb visual). On-kill of a Pollinated enemy: **Mass Bloom** — fires 3 homing child orbs toward other Pollinated enemies. At 5 Bloom kills, **Harvest Season**: next 10 orbs gain 2x damage, 1.5x scale, and aggressive homing (0.12).

### Triumphant Chorus — Summoner
**Current:** Scaffold orb.  
**Pattern: Four-Voice Ensemble**  
Summons 4 minion types. Each fires orbs with a unique behavior:
- **Soprano:** Orb fires at **high arc** (gravity 0.05f, high Y velocity) — drops onto enemies from above.
- **Alto:** Orb fires with **sine-wave** wobble — oscillating side-to-side as it travels.
- **Tenor:** Orb fires **straight and fast** (18f, no homing).
- **Bass:** Orb fires **slow (8f) with strong homing (0.12)** — heavy and deliberate.

With all 4 active, **Harmony Bonus:** all orbs gain +15% shared damage. Synchronized attack: all 4 fire simultaneously toward the same target — 4 orbs approaching from 4 different flight patterns.

### Fountain of Joyous Harmony — Summoner
**Current:** Scaffold orb (FountainProjectile, timeLeft=60).  
**Pattern: Healing Artillery**  
Stationary fountain fires homing orbs (1 per 50 frames) with **arcing** trajectory (gravity 0.08f). Orbs lob upward and arc down onto enemies. On enemy hit: deals damage. On passing near the player (within 60px during arc): heals 3 HP. Tier upgrades increase fire rate (50 → 40 → 30 frames). Every 15s: **Geyser** — fires 5 orbs simultaneously straight upward, they spread at apex and rain down with gravity in a wide area. Overlapping fountain ranges: orbs that pass through another fountain's zone gain +30% damage.

### The Standing Ovation — Summoner
**Current:** Scaffold orb.  
**Pattern: Audience Meter**  
Phantom spectator minions fire standard homing orbs. Special: **Ovation Meter** fills with combat (kills, crits, multi-hits). As meter fills, spectators increase behavior:
- 25%: Fire rate increases (60 → 45 frame interval).
- 50%: Orbs gain pierce 1.
- 75%: Orbs gain +30% speed and homing (0.06 → 0.10).
- 100% (Standing Ovation): All spectators fire **3 orbs each** simultaneously toward the cursor target + trigger a 5-second **Encore** where fire rate is doubled.

Meter drains slowly out of combat. Rewards sustained aggressive play.

---

## Design Pattern Summary

### Behavior Differentiators Used (No New Visuals)

| Behavior | Weapons Using It |
|----------|-----------------|
| **Decelerate-to-hover/mine** | Dual Fated Chime, Ignition Bell, Grandiose Chime, Conductor's Constellation, Starweaver, Requiem of Cosmos, Staff of Judgment, Decay Bell, Cog and Hammer, Gardener's Fury, Clockwork Harmony, Clair de Lune zones, Automaton (G freq) |
| **Split on hit** | Eternal Moon, Moonlight's Calling, Triumphant Fractal, Celestial Valor, Vernal Scepter, Petal Storm (Bow+Cannon), Concerto (Winter), Wrath's Cleaver, Cog and Hammer |
| **Orbiting (player/enemy)** | Dissonance of Secrets, Fugue of Unknown, Orrery of Dreams, Finality of Sakura, Frozen Heart, Final Fermata, Permafrost Codex, Solar Crest |
| **Accelerating** | Eternal Moon (per phase), Blossom of Sakura, Light of the Future, Solar Scorcher (buildup), Sin Collector (Absolution), Hymn (Gloria), Anthem of Glory |
| **Bouncing** | Fang of Infinite Bell, Requiem of Reality (Scherzo), Thornbound Reckoning, Clockwork Harmony (radial) |
| **Multi-phase behavior** | Eternal Moon (5 phases), Wrath's Cleaver (4 phases), Midnight Mechanism (5 phases), Hymn of Victorious (4 verses), Requiem of Reality (4 movements) |
| **Gravity/arcing** | Ignition Bell, Sakura's Blossom, Zenith Cleaver, Bellfire Annihilator, Resurrection (artillery), Gardener's Fury, Petal Storm Cannon, Chorus (Soprano), Fountain, Cog and Hammer |
| **Stacking/accumulation** | Sin Collector (sins), Thorn Spray (25 embed), Gear-Driven Arbiter (8 verdict), Serenade (5 rhythm), Elysian Verdict (3 tier), Standing Ovation (meter) |
| **Return/echo/replay** | Cipher Nocturne (snap-back), Tacet's Enigma (delayed echo), Temporal Piercer (reverse echo), Starfall Whisper (fracture replay), Resonance of Bygone (chain echo) |
| **Stream/rapid-fire** | Funeral Prayer, Solar Scorcher, Blossom of Sakura, Midnight Mechanism, Thorn Spray, Rose Thorn Chainsaw, Anthem of Glory |
| **Zone synergy** | Twilight Severance (rift amplifier), Unresolved Cadence (wave amplifier), Requiem of Time (Forward/Reverse overlap), Hurricane zone (Petal Storm Cannon) |
| **Convergence reward** | Dual Fated Chime (L/R collision), Variations of Void (triple meet), Conductor's Constellation (hover+home), Starweaver (right-click activate) |

### Uniqueness Grid — Same Orb, Different Weapon Identity

Every weapon achieves uniqueness through **one primary behavior** from the vocabulary above, combined with **one secondary behavior** and a **special trigger mechanic**:

| Weapon | Primary | Secondary | Special |
|--------|---------|-----------|---------|
| Incisor of Moonlight | Sine-wave | Reverse direction | 4th swing reversal |
| Eternal Moon | Phase escalation | Split on hit | Phase 5 burst |
| Cipher Nocturne | Snap-back return | Distance-scaling | Release-all |
| Dissonance of Secrets | Growing scale | Orbiting children | Death-release |
| Midnight Mechanism | 5-phase fire rate | Homing escalation | 12th = Midnight Strike |
| Sin Collector | Rapid straight | Accumulation tiers | 10/20/30 expenditure |
| Orrery of Dreams | Triple orbit | Varied fire rates | Alignment convergence |
