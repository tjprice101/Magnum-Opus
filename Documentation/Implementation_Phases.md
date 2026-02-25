# Implementation Phases -- MagnumOpus Parallel Content Systems

> **This document defines independent content phases that run alongside the Enhancements.md progression.**
> These phases introduce **living-world systems** -- world events, new damage types, dynamic enemies, and parallel weapon trees -- that make the game feel alive, fluid, and constantly surprising.

---

## How This Differs from Enhancements.md

| | Enhancements.md Phase 1 | Implementation Phases -- Phase #1 |
|---|---|---|
| **Focus** | Foundation Materials -- crafting bases, pre-hardmode drops, seasonal bars, essences (41 items) | Living World Content -- world events, a new damage type, 23 weapons, 10 enemies, event bosses, dynamic gameplay systems |
| **Goal** | Establish the crafting backbone for all themed equipment | Make the world feel alive with dynamic events, a parallel weapon progression, and enemies that react to the player's advancement |
| **Scope** | Materials, bars, essences, cores | Polyphonic damage class, world events, full weapon trees, enemy factions, crafting chain, Midjourney asset prompts |
| **Theme** | Seasonal / foundational | Musical counterpoint & polyphony -- "many voices singing together" |
| **Dependency** | None (first in line) | Can start after Enhancements Phase 1 materials exist; uses some foundation bars in recipes |

---

# Phase #1: The Polyphonic Line

> *"Where every other theme is a single composition -- one voice telling one story -- the Polyphonic Line is many voices singing at once, weaving in and out of each other, building something no single voice could achieve alone."*

This phase introduces:
- A new **Polyphonic damage class** with its own debuff mechanic (Harmonic Sear)
- **5 world events** that trigger dynamically throughout progression
- **10 new enemies** + 1 event boss across all tiers
- **23 weapons** spanning pre-hardmode through post-Moon Lord
- **11 crafting materials** in a dedicated progression chain
- A complete **chromatic iridescent** visual identity distinct from every other MagnumOpus theme

---

## 1. The Polyphonic Damage Type

### 1.1 Core Identity

**Polyphonic** is a standalone damage class -- it does not inherit from melee, ranged, magic, or summon. It represents the concept of *musical counterpoint*: multiple independent melodic lines sounding simultaneously.

| Property | Value |
|---|---|
| Class Name | `PolyphonicDamageClass` |
| Internal Tag | `polyphonic` |
| Musical Concept | Counterpoint -- many independent voices creating harmony through opposition |
| Terminology | Drawn from counterpoint and fugue theory (subject, answer, stretto, cantus firmus, etc.) |

**Why a new class?** Polyphonic weapons don't belong to any single combat school. They represent the *intersection* of all styles -- a melee swing that spawns ranged echoes, a staff that fires self-harmonizing projectiles, a bow whose arrows sing in canon. The damage type is the unifying thread.

### 1.2 Core Mechanic: Harmonic Sear

Every Polyphonic weapon imprints **harmonic frequencies** on enemies it hits. These frequencies layer on top of each other, and when enough voices accumulate, they begin to destroy the target from within.

| Stack Count | Effect | Visual |
|---|---|---|
| 1 Harmonic | Mild burn (2 DPS) | Single faint chromatic flame -- indigo tint |
| 2 Harmonics | Moderate burn (5 DPS) | Two interweaving flames -- indigo/teal |
| 3 Harmonics | Intense searing (10 DPS) | Three-colored fire wreath -- indigo/teal/rose |
| 4 Harmonics | Overwhelming burn (18 DPS) | Four-voice flame spiral -- indigo/teal/rose/amber |
| 5 Harmonics (Max) | Chromatic Fire (30 DPS + 8% max HP/s) | Full chromatic iridescent blaze -- all Polyphonic palette colors cycling |

**Key distinction from Resonant Burn:** Harmonic Sear uses the *Polyphonic palette specifically* (indigo/teal/rose/amber chromatic iridescence) rather than the wide rainbow monochrome of Resonant Burn. The flames are **tonal**, not spectral.

### 1.3 Resonant Scar Interaction: Harmonic Overload

If a Polyphonic weapon rolls the **Resonant Scar** prefix (from the existing prefix system), enemies affected by *both* Harmonic Sear AND Resonant Burn enter a unique amplified state:

| State | Trigger | Effect |
|---|---|---|
| **Harmonic Overload** | Target has both Harmonic Sear (any stack) AND Resonant Burn active simultaneously | Both debuffs deal **+40% bonus damage**. Visual becomes a stunning collision of chromatic iridescent + rainbow monochrome flames -- two competing fire systems warring on the enemy's body |

This creates a meaningful cross-system interaction that rewards players who mix Polyphonic weapons with Resonant-prefixed gear.

### 1.4 Damage Scaling

Polyphonic damage scales with its own set of bonuses:

| Source | Example |
|---|---|
| Polyphonic accessories | +X% Polyphonic damage |
| Harmonic Sear stacks | Bonus DoT on target (does not scale with damage %) |
| Weapon-specific mechanics | Each weapon has a unique secondary effect |
| Set bonuses (future) | Polyphonic armor sets in later phases |

Polyphonic damage is **not** affected by melee speed, ranged crit, magic mana cost reduction, or summon tag damage. It has its own ecosystem.

---

## 2. Visual Identity & Palette

### 2.1 The Chromatic Iridescent Aesthetic

The Polyphonic theme occupies a unique visual space in MagnumOpus: **chromatic iridescence**. Where existing themes sit in narrow hue bands (La Campanella = orange/black, Moonlight Sonata = purple/blue, Enigma = purple/green), Polyphonic spans the *full color wheel* through a controlled iridescent metallic look.

**Design pillars:**
- **Sleek metallic surfaces** -- brushed chrome, polished titanium, liquid mercury
- **Iridescent color shifting** -- colors flow across surfaces like oil on water
- **Musical notation accents** -- staff lines, clefs, and note shapes etched into metal
- **Crystallized sound** -- geometric crystal formations that pulse with internal light
- **Chromatic fire** -- flames that cycle through the Polyphonic palette rather than standard orange/red

### 2.2 Core Palette (6 Colors)

| Index | Name | Hex | RGB | Usage |
|---|---|---|---|---|
| 0 | Graphite Void | `#1A1A2E` | (26, 26, 46) | Shadows, trail ends, deepest darks |
| 1 | Chromatic Indigo | `#4A3AFF` | (74, 58, 255) | Primary glow, outer bloom, base tone |
| 2 | Iridescent Teal | `#00D4AA` | (0, 212, 170) | Secondary glow, mid-bloom, flowing accent |
| 3 | Prismatic Rose | `#FF4D8A` | (255, 77, 138) | Warm accent, Harmonic Sear flame base |
| 4 | Harmonic Amber | `#FFB347` | (255, 179, 71) | Hot highlights, impact cores, flame tips |
| 5 | White Chrome Flash | `#F0F0FF` | (240, 240, 255) | Core flare, lens flare center, brightest point |

```csharp
// Implementation
private static readonly Color[] PolyphonicPalette = new Color[]
{
    new Color(26, 26, 46),     // [0] Graphite Void
    new Color(74, 58, 255),    // [1] Chromatic Indigo
    new Color(0, 212, 170),    // [2] Iridescent Teal
    new Color(255, 77, 138),   // [3] Prismatic Rose
    new Color(255, 179, 71),   // [4] Harmonic Amber
    new Color(240, 240, 255),  // [5] White Chrome Flash
};
```

### 2.3 Extended Palette (12 Colors -- Per-Weapon Variation)

Each weapon picks a subset of these for its unique identity while staying within the Polyphonic family:

| Index | Name | Hex | RGB | Weapon Usage |
|---|---|---|---|---|
| 0 | Deep Void | `#0D0D1A` | (13, 13, 26) | Darkest shadow, void weapons |
| 1 | Midnight Indigo | `#2E1F99` | (46, 31, 153) | Deep blue-violet base |
| 2 | Chromatic Indigo | `#4A3AFF` | (74, 58, 255) | Primary Polyphonic blue |
| 3 | Electric Cyan | `#00E5FF` | (0, 229, 255) | High-energy cyan accents |
| 4 | Iridescent Teal | `#00D4AA` | (0, 212, 170) | Standard teal |
| 5 | Harmonic Emerald | `#00CC66` | (0, 204, 102) | Green-teal transition |
| 6 | Prismatic Rose | `#FF4D8A` | (255, 77, 138) | Warm pink-red |
| 7 | Chromatic Magenta | `#CC33FF` | (204, 51, 255) | Purple-pink bridge |
| 8 | Harmonic Amber | `#FFB347` | (255, 179, 71) | Warm amber highlight |
| 9 | Molten Gold | `#FFD700` | (255, 215, 0) | Brightest warm tone |
| 10 | Iridescent Silver | `#C0C0D0` | (192, 192, 208) | Metallic neutral |
| 11 | White Chrome Flash | `#F0F0FF` | (240, 240, 255) | Maximum brightness |

### 2.4 Custom Rarity: PolyphonicRarity

Polyphonic items use a custom rarity with **4-phase color cycling** that smoothly transitions through the core palette:

```
Phase 1 (0.00 - 0.25): Chromatic Indigo -> Iridescent Teal
Phase 2 (0.25 - 0.50): Iridescent Teal -> Prismatic Rose
Phase 3 (0.50 - 0.75): Prismatic Rose -> Harmonic Amber
Phase 4 (0.75 - 1.00): Harmonic Amber -> Chromatic Indigo (loop)
```

Cycle duration: ~3 seconds per full loop. The effect is a smooth, mesmerizing color flow that immediately signals "this is Polyphonic gear."

### 2.5 Unique Particle Types

| Particle | Description | Usage |
|---|---|---|
| **Chromatic Mote** | Tiny iridescent spark that shifts color as it drifts. Palette-locked to Polyphonic colors. Scale 0.3f-0.5f. | Ambient trails, idle weapon aura, background sparkle |
| **Voice Note** | A music note shape (uses existing MusicNote variants 1-6) rendered in chromatic iridescent colors with a metallic sheen overlay. Scale 0.7f+ (visible!). | Weapon swings, projectile trails, impact bursts |
| **Cadence Burst** | A rapid-fire radial burst of thin lines (like a conductor's baton flick) that fade from indigo to amber. | Impact effects, combo finishers, event triggers |
| **Iridescent Trail** | A flowing ribbon trail that shifts through the full Polyphonic palette along its length. Width tapers from 20f to 0. | Projectile tails, swing arcs, enemy movement |

### 2.6 Visual Distinction from Other Themes

| Theme | Hue Band | Surface Feel | Polyphonic Contrast |
|---|---|---|---|
| La Campanella | Orange/Black | Smoky, heavy, infernal | Polyphonic is sleek, clean, iridescent |
| Eroica | Scarlet/Gold | Warm, triumphant, heroic | Polyphonic is cool-shifted, technical, precise |
| Swan Lake | White/Black | Elegant, monochrome, graceful | Polyphonic is colorful, dynamic, chromatic |
| Moonlight Sonata | Purple/Blue | Soft, misty, lunar | Polyphonic is sharp, metallic, crystalline |
| Enigma Variations | Purple/Green | Eerie, void, mysterious | Polyphonic is vibrant, visible, celebratory |
| Fate | Pink/Crimson/Black | Cosmic, celestial, dark | Polyphonic is multi-spectral, grounded, instrumental |

---

## 3. Crafting Materials (11 Items)

The Polyphonic crafting chain is a self-contained progression that mirrors the game's advancement tiers. Each material represents a deeper understanding of harmonic resonance.

### 3.1 Material Progression Chain

```
Pre-Hardmode:
  Voicestone Shard (world drop) -> Polyphonic Ore (mined) -> Polyphonic Bar (smelted)

Hardmode:
  Harmonic Voice Fragment (enemy drop) -> Chromatic Alloy (crafted from Bar + Fragment)

Post-Plantera:
  Concertmaster's Baton (mini-boss drop) -> Convergence Fragment (crafted)

Post-Golem:
  Chromatic Core (event reward) -> Enharmonic Alloy (crafted)

Post-Moon Lord:
  Fragment of the Unfinished (event boss drop) -> Opus Null (ultimate crafting material)
```

### 3.2 Material Details

| # | Material | Tier | Source | Description | Visual |
|---|---|---|---|---|---|
| 1 | **Voicestone Shard** | Pre-HM | Drops from stone-type enemies during Dissonance Surge event; also found in newly-spawned Polyphonic Ore veins | A jagged shard of stone that hums with a single sustained tone. Faint indigo veins pulse through its surface. | Small angular stone fragment, deep gray with glowing indigo cracks, faint teal mist at edges |
| 2 | **Polyphonic Ore** | Pre-HM | Spawns in the world after first Dissonance Surge event concludes. Found at cavern layer. Glows faintly. | Raw crystalline ore that resonates with ambient sound. Walking near it produces faint harmonic overtones. | Chunky ore block with iridescent crystal growths, shifts indigo-to-teal in light, embedded musical staff line patterns |
| 3 | **Polyphonic Bar** | Pre-HM | Smelted at Furnace: 4 Polyphonic Ore = 1 Bar | A refined ingot of polyphonic metal. Its surface shifts color when tilted, like oil on water. | Sleek rectangular bar with a brushed-chrome surface, iridescent color bands flowing across it (indigo/teal/rose), clean geometric shape |
| 4 | **Harmonic Voice Fragment** | HM | Drops from Fugal Invasion event enemies (First/Second/Third/Fourth Voice) | A crystallized fragment of a destroyed harmonic entity. It whispers melodic fragments when held. | Floating crystalline shard, translucent with internal chromatic light, shaped like an angular music note, trails of teal/rose mist |
| 5 | **Chromatic Alloy** | HM | Crafted at Mythril/Orichalcum Anvil: 3 Polyphonic Bar + 5 Harmonic Voice Fragment + 1 Soul of Light + 1 Soul of Night | An advanced alloy that contains all chromatic frequencies. Its surface displays every color in the Polyphonic spectrum simultaneously. | Wide bar with a mirror-polish surface, full palette cycling across its face, faint geometric etching (hexagonal lattice), slight glow at edges |
| 6 | **Concertmaster's Baton** | Post-Plantera | Drops from The Concertmaster mini-boss during Grand Rehearsal event (100% drop) | The broken conducting baton of a spectral orchestra leader. It still carries the authority to command harmonic forces. | Thin elegant wand/baton, white chrome body with indigo/rose/amber inlays spiraling up, broken at the tip with chromatic energy leaking out, musical staff lines etched along its length |
| 7 | **Convergence Fragment** | Post-Plantera | Crafted at Mythril/Orichalcum Anvil: 2 Chromatic Alloy + 1 Concertmaster's Baton + 10 Soul of Sight | A dense crystal formed by forcing all harmonic voices into a single point. The internal pressure creates a perpetual state of near-explosion. | Compact diamond-shaped crystal, intensely bright core (white chrome), surrounded by compressed layers of all Polyphonic colors, visible internal fracture lines that pulse with light |
| 8 | **Chromatic Core** | Post-Golem | Reward for completing the Chromatic Convergence event (all 12 waves). 1-3 drop per completion. | The living heart of chromatic energy, ripped from the convergence itself. It cycles through 12 semitones in perfect sequence. | Spherical core with 12 distinct color facets (chromatic scale), each facet glows in sequence like a heartbeat, surrounded by orbiting chromatic motes, metallic frame holding the sphere |
| 9 | **Enharmonic Alloy** | Post-Golem | Crafted at Ancient Manipulator: 2 Convergence Fragment + 3 Chromatic Core + 5 Luminite Bar | The ultimate Polyphonic metal. Its surface exists in a state of enharmonic ambiguity -- every color is simultaneously itself and its enharmonic equivalent. | Tall refined ingot, surface appears to shift between two entirely different color states (like a lenticular print), clean architectural lines, slight reality-distortion shimmer at its edges |
| 10 | **Fragment of the Unfinished** | Post-ML | Drops from The Incomplete Maestro during Unfinished Symphony event (100% drop, 3-5 per kill) | A piece of a symphony that was never completed. It aches with potential, yearning to become whole. Contains all harmonic knowledge but lacks resolution. | Torn page of sheet music rendered in metal, musical notation visible but incomplete (notes trail off into void), edges dissolve into chromatic particles, the "ink" is liquid iridescent metal that shifts and flows |
| 11 | **Opus Null** | Post-ML | Crafted at Ancient Manipulator: 5 Enharmonic Alloy + 3 Fragment of the Unfinished + 20 Luminite Bar + 1 of each Lunar Fragment | The crafting material that represents the silence between notes -- the void from which all music emerges. It is simultaneously nothing and everything. | A perfect sphere of absolute void surrounded by a thin shell of every Polyphonic color compressed into a luminous ring, the center appears to absorb light, the outer ring radiates all colors simultaneously, orbiting notation symbols dissolve as they approach the center |

### 3.3 Drop Rate Tables

| Material | Source | Drop Rate | Stack |
|---|---|---|---|
| Voicestone Shard | Stone enemies during Dissonance Surge | 33% | 1-3 |
| Harmonic Voice Fragment | Voice Sentinels (Fugal Invasion) | 50% | 1-2 |
| Harmonic Voice Fragment | Fourth Voice Apex (mini-boss) | 100% | 3-5 |
| Concertmaster's Baton | The Concertmaster (mini-boss) | 100% | 1 |
| Chromatic Core | Chromatic Convergence (event clear) | 100% | 1-3 |
| Fragment of the Unfinished | The Incomplete Maestro (event boss) | 100% | 3-5 |

---

## 4. World Events (5)

World events are the heartbeat of the Polyphonic Line. They trigger dynamically throughout progression, introducing Polyphonic enemies, spawning resources, and giving players access to new crafting materials. Each event is structured like a musical form -- not just waves of enemies, but compositions with internal logic.

### 4.1 Event Overview

| # | Event | Tier | Trigger | Duration | Structure | Key Reward |
|---|---|---|---|---|---|---|
| 1 | **Dissonance Surge** | Pre-HM (Post-Skeletron) | Random night (10% chance) | ~8 minutes | 3 escalating waves | Spawns Polyphonic Ore in world; Voicestone Shards |
| 2 | **Fugal Invasion** | HM (Post-any Mech Boss) | Random night (8% chance) | ~12 minutes | 4-wave fugue structure | Harmonic Voice Fragments; Voice enemy drops |
| 3 | **Grand Rehearsal** | Post-Plantera | Player-summoned (crafted item: Rehearsal Score) | ~15 minutes | Orchestra sections + mini-boss | Concertmaster's Baton; section-specific drops |
| 4 | **Chromatic Convergence** | Post-Golem | Automatic after 50 cumulative Polyphonic enemy kills | ~18 minutes | 12 mini-waves (chromatic scale) | Chromatic Cores; unique per-semitone drops |
| 5 | **Unfinished Symphony** | Post-Moon Lord | Player-summoned (crafted item: Incomplete Score) + Full Moon | ~25 minutes | Multi-phase event with final boss | Fragment of the Unfinished; Opus Null components |

---

### 4.2 Event 1: Dissonance Surge

> *"The world groans. Somewhere deep below, a single voice has been singing for millennia. Tonight, it finally cracks."*

**Tier:** Pre-Hardmode (requires Skeletron defeated)
**Trigger:** 10% chance each night after 8:00 PM. Announced with status message: *"A discordant hum rises from the earth..."*
**Duration:** ~8 minutes (3 waves)

#### Wave Structure

| Wave | Duration | Enemies | Intensity | Special |
|---|---|---|---|---|
| **Wave 1: Dissonant Murmur** | 2.5 min | Dissonant Wraiths (slow spawn) | Low | Polyphonic Ore veins begin generating underground. Faint indigo glow visible on surface. |
| **Wave 2: Rising Overtones** | 2.5 min | Dissonant Wraiths + Chromatic Slimes (faster spawn) | Medium | Sky tints slightly indigo. Background harmonic hum intensifies. More ore spawns. |
| **Wave 3: The Crack** | 3 min | All Pre-HM Polyphonic enemies, dense spawn | High | Sky fully tinted chromatic iridescent. Final ore vein generation burst. Event culminates in a visual "crack" across the sky that fades. |

#### Rewards
- **Polyphonic Ore** spawns permanently in the world (cavern layer, similar density to Gold/Platinum)
- **Voicestone Shards** drop from all enemies during the event
- First completion unlocks the Polyphonic crafting tree

#### Sky Effect
The sky gradually shifts from normal to a deep indigo wash with iridescent streaks. Chromatic aurora-like bands weave across the upper sky. Stars appear to vibrate. The moon gains a faint chromatic halo.

---

### 4.3 Event 2: Fugal Invasion

> *"They arrive in sequence -- first one voice, then another answering, then a third weaving between them, until the night itself becomes a fugue."*

**Tier:** Hardmode (requires any Mechanical Boss defeated)
**Trigger:** 8% chance each night. Announced with: *"Harmonic voices pierce the veil..."*
**Duration:** ~12 minutes (4 waves)

This event is structured like an actual **musical fugue** -- each wave introduces a new "voice" (enemy type), and subsequent waves layer the voices together in increasingly complex counterpoint.

#### Wave Structure

| Wave | Musical Term | Duration | New Enemy | Returning Enemies | Mechanic |
|---|---|---|---|---|---|
| **Wave 1: Subject** | The fugue's main theme | 2.5 min | First Voice Sentinel | None | Single enemy type, predictable patterns. Players learn the "melody." |
| **Wave 2: Answer** | The response in a different key | 2.5 min | Second Voice Echo | First Voice Sentinel (reduced) | Second Voice mimics First Voice's attack patterns but offset in timing and position -- actual counterpoint behavior. |
| **Wave 3: Countersubject** | A contrasting melody against the subject | 3 min | Third Voice Harmonic | First + Second Voice | Third Voice attacks in the gaps between First and Second's patterns, creating a weaving triple-voice texture. |
| **Wave 4: Stretto** | All voices overlapping in rapid succession | 4 min | Fourth Voice Apex (mini-boss) | All three prior voices | All four voice types active simultaneously. Fourth Voice Apex is a mini-boss (12K HP) that conducts the other three, synchronizing their attacks into devastating coordinated salvos. |

#### Fugal AI Behavior
The enemies genuinely interact with each other's attack timing:
- First Voice attacks on beats 1 and 3 of a hidden internal 4-beat cycle
- Second Voice attacks on beats 2 and 4
- Third Voice attacks on the "and" of each beat (offbeats)
- Fourth Voice Apex's attacks span full measures and override all other voices temporarily (fermata behavior)

This creates a rhythmic combat experience where attackers come in a predictable but complex pattern, rewarding players who learn the rhythm.

#### Rewards
- **Harmonic Voice Fragments** from all Voice enemies
- **Fourth Voice Apex** drops 3-5 Fragments guaranteed + a chance at the Contrary Motion weapon
- Completion flag advances toward Chromatic Convergence counter

#### Sky Effect
The sky splits into four horizontal bands, each tinted a different Polyphonic color (indigo/teal/rose/amber from bottom to top). As each wave progresses, the bands begin to weave and intertwine, creating a braided aurora effect. During Stretto, the bands collapse into a single unified chromatic pulse.

---

### 4.4 Event 3: Grand Rehearsal

> *"The orchestra assembles. Strings tune. Brass warms. Percussion sets tempo. And the Concertmaster raises the baton..."*

**Tier:** Post-Plantera
**Trigger:** Player uses the **Rehearsal Score** item (crafted: 5 Chromatic Alloy + 10 Soul of Fright + 10 Soul of Might + 10 Soul of Sight at Mythril/Orichalcum Anvil)
**Duration:** ~15 minutes (4 sections + mini-boss)

This event is structured like an **orchestra rehearsal** -- each section of the orchestra gets its own wave, and the mini-boss is the Concertmaster bringing them all together.

#### Section Structure

| Section | Orchestra Section | Duration | Enemies | Combat Style | Visual Theme |
|---|---|---|---|---|---|
| **Section 1: Strings** | Violins, Violas, Cellos | 3 min | Phantom Violinist x3, Spectral Cellist x2 | Fast, darting attacks with wide sweeping arcs. Violin enemies fire rapid thin projectiles in bowing patterns. Cellist enemies fire slower, heavier resonant blasts. | Indigo-dominant, flowing ribbon trails, staff-line projectile paths |
| **Section 2: Brass** | Trumpets, Horns, Trombones | 3 min | Brass Fanfare enemies (3 variants) | Powerful frontal cone attacks with long windups. Telegraphed but devastating. Sound-wave shockwave projectiles. | Amber-dominant, bold geometric blast shapes, concentric ring shockwaves |
| **Section 3: Woodwinds** | Flutes, Clarinets, Oboes | 3 min | Woodwind Whisper enemies (3 variants) | Evasive, mobile enemies that attack from unexpected angles. Curved, spiraling projectiles that home slightly. | Teal-dominant, swirling wind trail effects, leaf-like curved projectile paths |
| **Section 4: Percussion** | Timpani, Cymbals, Snare | 3 min | Percussion Strike enemies (3 variants) | Ground-pound and area-denial attacks. Seismic wave projectiles that travel along the ground. Cymbal crash creates radial bursts. | Rose-dominant, impact shockwave rings, ground-crack effects |
| **Finale: The Concertmaster** | The conductor | ~3 min | **The Concertmaster** (mini-boss, 45K HP) | Summons echoes of all four sections. Attacks involve conducting gestures that direct coordinated enemy salvos. Raises baton for charge attacks. | Full Polyphonic palette, baton-trail effects, all four section colors present |

#### The Concertmaster (Mini-Boss)

| Property | Value |
|---|---|
| HP | 45,000 |
| Defense | 40 |
| Damage | 80 (contact), 60-100 (projectiles) |
| AI | Hovers above player, never directly attacks -- instead "conducts" by summoning echoes of section enemies and synchronizing their attacks |
| Special | Every 20 seconds, raises baton and performs a "Grand Crescendo" -- a 3-second charge followed by ALL active echoes attacking simultaneously |

**Drops:**
- Concertmaster's Baton (100%)
- 15-25 Gold Coins
- Rehearsal Trophy (10%)

#### Sky Effect
The sky darkens to a concert-hall ambiance -- deep velvet black with spotlights of each section's color sweeping across the sky. During the Concertmaster fight, a faint orchestral pit layout appears in the sky as a constellation-like pattern.

---

### 4.5 Event 4: Chromatic Convergence

> *"Twelve tones. Twelve waves. Each one a semitone higher than the last. When the scale completes, the world itself resonates at every frequency simultaneously."*

**Tier:** Post-Golem
**Trigger:** Automatic after 50 cumulative Polyphonic enemy kills (tracked via world data). Announced: *"The chromatic scale begins to play itself..."*
**Duration:** ~18 minutes (12 mini-waves)

This event is structured like a **chromatic scale** -- 12 waves, each corresponding to one semitone (C through B), each with a unique color from the extended palette and a unique attack pattern.

#### Wave Structure

Each wave lasts ~90 seconds. The Chromatic Harbinger enemy (who cycles through all 12 semitone attacks) appears as a rare spawn starting from wave 7.

| Wave | Semitone | Color | Enemy Modifier | Special Attack |
|---|---|---|---|---|
| 1 | C (Unison) | Deep Void | Normal enemies, no modifier | None -- baseline |
| 2 | C# | Midnight Indigo | +10% speed | Enemies leave fading afterimages |
| 3 | D | Chromatic Indigo | +15% damage | Enemies fire indigo bolts on death |
| 4 | D# | Electric Cyan | +20% speed, erratic movement | Enemies teleport short distances |
| 5 | E | Iridescent Teal | +10% HP | Enemies regenerate 1% HP/s |
| 6 | F | Harmonic Emerald | +25% damage | Enemies explode into homing shards on death |
| 7 | F# | Prismatic Rose | +15% HP, +15% speed | Chromatic Harbinger first appears |
| 8 | G | Chromatic Magenta | +30% damage | Enemies inflict Harmonic Sear on the player |
| 9 | G# | Harmonic Amber | +20% HP, +20% speed | Enemies attack in synchronized bursts |
| 10 | A | Molten Gold | +25% HP, +25% damage | Enemies leave damaging ground pools |
| 11 | A# | Iridescent Silver | +30% all stats | Enemies phase in and out of visibility |
| 12 | B (Leading Tone) | White Chrome Flash | +40% all stats | All enemy types present, maximum intensity, screen-wide chromatic pulse on wave start |

#### The Chromatic Harbinger (Rare Spawn)

| Property | Value |
|---|---|
| HP | 25,000 |
| Type | Rare spawn (waves 7-12), guaranteed spawn wave 12 |
| Mechanic | Cycles through 12 attack types matching each semitone, spending 5 seconds in each "key." Current key is visible as its body color. |
| Danger | Attacks become stronger as it ascends the scale. By wave 12 (B), its attacks have +40% damage and speed. |

#### Rewards
- **Chromatic Cores** (1-3 per full completion)
- Per-semitone themed drops (consumables, vanity, etc.)
- Chromatic Harbinger has a 20% chance to drop the Cantus Firmus weapon

#### Sky Effect
Each wave tints the sky a different color from the extended palette. The sky cycles smoothly through the chromatic spectrum, creating a stunning 18-minute rainbow progression. During wave 12, the sky becomes pure white chrome brilliance for the final 90 seconds.

---

### 4.6 Event 5: Unfinished Symphony

> *"A symphony was begun but never completed. Its creator vanished before the final movement. Now the incomplete work tears at the fabric of the world, demanding resolution -- or annihilation."*

**Tier:** Post-Moon Lord
**Trigger:** Player uses the **Incomplete Score** item (crafted: 3 Enharmonic Alloy + 1 Celestial Sigil + 50 Luminite Bar at Ancient Manipulator) during a Full Moon night
**Duration:** ~25 minutes (4 movements + boss fight)

This is the capstone event of the Polyphonic Line -- a multi-phase ordeal structured like a symphony's four movements, culminating in a fight against **The Incomplete Maestro**, the spectral composer who started the Polyphonic Line and vanished before finishing their magnum opus.

#### Movement Structure

| Movement | Musical Form | Duration | Content | Difficulty |
|---|---|---|---|---|
| **I. Allegro** | Fast opening | 4 min | All Pre-HM and HM Polyphonic enemies at massively boosted stats. Dense, fast-paced combat. | High |
| **II. Adagio** | Slow, contemplative | 5 min | Fewer enemies, but each is a powerful elite with unique AI. The Spectral Cellist and Phantom Violinist return as empowered "Soloist" variants with 50K HP each. Between enemy spawns, the world falls eerily silent. | Very High |
| **III. Scherzo** | Playful but menacing | 5 min | Enemies come in trios that mirror the fugal invasion's counterpoint behavior but at post-ML power levels. False "calm" periods punctuated by sudden triple-enemy ambushes. | Very High |
| **IV. Finale -- The Incomplete Maestro** | The unfinished climax | ~11 min | The Incomplete Maestro descends. Multi-phase boss fight. | Extreme |

#### The Incomplete Maestro (Event Boss)

| Property | Value |
|---|---|
| HP | 350,000 |
| Defense | 60 |
| Contact Damage | 150 |
| AI Phases | 3 phases at 100-60%, 60-30%, 30-0% HP |

**Phase 1 (100-60%): The First Three Movements**
- Attacks drawn from the orchestral sections (strings, brass, woodwinds, percussion)
- Each attack is a "quotation" from a previous event's enemies, but dramatically amplified
- Conducts with a spectral baton -- conducting gestures telegraph attacks
- Summons pairs of Voice Sentinels as support

**Phase 2 (60-30%): The Missing Movement**
- The Maestro becomes frantic, attacking with incomplete, stuttering patterns
- Attacks start and stop mid-execution, changing direction unpredictably
- Visual glitching: the Maestro's sprite flickers between composed and dissolved states
- New attack: "Unresolved Cadence" -- fires a massive beam that stops just short of resolving, leaving a lingering damage zone where the resolution should have been
- Summons the Concertmaster as a support enemy

**Phase 3 (30-0%): Silence Before the End**
- The Maestro goes silent. All music stops. The world becomes achingly quiet.
- Then unleashes a continuous barrage of every Polyphonic attack type simultaneously
- "Chromatic Apocalypse" ultimate: 12 waves of semitone-coded projectiles fire in rapid succession
- On death, the Maestro does not explode -- it fades into a single sustained note that hangs in the air, then resolves into silence. The screen briefly displays a measure of sheet music with the final note written in.

**Drops:**
- Fragment of the Unfinished (100%, 3-5)
- The Unfinished Fugue weapon (10%)
- Omni Voce weapon (5%)
- Incomplete Maestro Trophy (10%)
- Incomplete Maestro Mask (14.29%)
- Maestro's Relic (100% in Master Mode)

#### Sky Effect
**Movement I:** Sky becomes a roiling storm of chromatic energy -- clouds of indigo and teal churn violently.
**Movement II:** Sky clears to an unnatural stillness -- pure void black with a single chromatic star overhead.
**Movement III:** Sky fractures into three competing color zones that clash and merge unpredictably.
**Movement IV:** Sky dissolves entirely into a field of musical notation -- visible staff lines, notes, rests, and dynamic markings floating in the void. As the Maestro takes damage, notes begin to erase themselves. At death, the sky shows one final whole note, then fades to peaceful starlight.

---

## 5. Enemies (10 + 1 Event Boss)

All Polyphonic enemies share the chromatic iridescent design language -- sleek metallic bodies, flowing iridescent color accents, and musical-notation visual motifs. Each enemy is designed to feel like a *sentient piece of music* -- a harmonic entity that exists as crystallized sound.

### 5.1 Enemy Roster Overview

| # | Enemy | Tier | HP | Damage | Defense | Event Appearance | Role |
|---|---|---|---|---|---|---|---|
| 1 | Dissonant Wraith | Pre-HM | 120 | 25 | 8 | Dissonance Surge | Basic melee floater |
| 2 | Chromatic Slime | Pre-HM | 90 | 20 | 6 | Dissonance Surge | Ranged color-shifting slime |
| 3 | First Voice Sentinel | HM | 800 | 50 | 20 | Fugal Invasion (Wave 1) | Rhythmic melee attacker |
| 4 | Second Voice Echo | HM | 650 | 45 | 16 | Fugal Invasion (Wave 2) | Mirror/delay attacker |
| 5 | Third Voice Harmonic | HM | 700 | 55 | 18 | Fugal Invasion (Wave 3) | Offbeat gap-filler |
| 6 | Fourth Voice Apex | HM (Mini-Boss) | 12,000 | 75 | 30 | Fugal Invasion (Wave 4) | Conductor mini-boss |
| 7 | Phantom Violinist | Post-Plantera | 3,500 | 70 | 28 | Grand Rehearsal (Strings) | Fast darting attacker |
| 8 | Spectral Cellist | Post-Plantera | 4,500 | 85 | 32 | Grand Rehearsal (Strings) | Heavy resonant blaster |
| 9 | The Concertmaster | Post-Plantera (Mini-Boss) | 45,000 | 80 | 40 | Grand Rehearsal (Finale) | Conducting mini-boss |
| 10 | Chromatic Harbinger | Post-Golem | 25,000 | 100 | 45 | Chromatic Convergence | 12-semitone cycling attacker |
| Boss | The Incomplete Maestro | Post-ML | 350,000 | 150 | 60 | Unfinished Symphony | Final event boss |

### 5.2 Pre-Hardmode Enemies

#### Dissonant Wraith

> *"A fragment of sound given form -- a single note that shattered and became alive."*

| Property | Value |
|---|---|
| HP | 120 |
| Damage | 25 |
| Defense | 8 |
| AI | Slow-hover, drifts toward player with slight sine-wave oscillation. Periodically phases through blocks for 2 seconds. |
| Attack | Contact damage only. Leaves a faint Harmonic Sear (1 stack, 3 second duration) on contact. |
| Drops | Voicestone Shard (33%, 1-3), Polyphonic Bar (5%, 1) |

**Visual Design:** A translucent, spectral humanoid torso (no legs -- trails off into mist below the waist). Body is dark graphite void metal with indigo veins pulsing along the surface. Head is a featureless chrome oval with a single glowing staff-line crack across it. Arms end in long, tapered claws that trail iridescent mist. The entire body shimmers between visible and nearly-transparent on a 2-second cycle. Chromatic mote particles drift off its surface constantly.

**Unique VFX:** Trails a ribbon of indigo-to-teal iridescent mist. When it phases through blocks, its body fractures into chromatic shards that reassemble on the other side (like a music note breaking apart and reforming). On death, collapses into a burst of scattered music notes that hang in the air for 2 seconds before dissolving.

---

#### Chromatic Slime

> *"The simplest harmonic entity -- a single resonant frequency contained in a gelatinous shell that refracts light into every color."*

| Property | Value |
|---|---|
| HP | 90 |
| Damage | 20 (contact), 15 (projectile) |
| Defense | 6 |
| AI | Standard slime hopping with a twist -- each hop shifts its color to the next in the Polyphonic extended palette (12 colors, cycling). |
| Attack | Every 3rd hop, fires a small chromatic bolt toward the player. Bolt color matches current body color. |
| Drops | Voicestone Shard (25%, 1-2), Gel (100%, 2-5) |

**Visual Design:** A standard slime shape but rendered in polished chrome metal rather than translucent gel. Surface is mirror-reflective, shifting through the 12-color extended palette with each hop. Internal core is visible as a bright white-chrome orb suspended in the center. Small musical notation symbols (quarter notes, rests) float inside the metallic surface like inclusions in glass. Eyes are two horizontal staff lines.

**Unique VFX:** Each hop produces a ring of the current color that expands outward along the ground. The chromatic bolt projectile leaves a thin trail of the current palette color. On death, shatters into 12 metallic shards (one per semitone color) that scatter radially, each leaving its own colored trail.

---

### 5.3 Hardmode Enemies

#### First Voice Sentinel

> *"The subject of the fugue -- the first melody to speak, setting the theme that all others must follow."*

| Property | Value |
|---|---|
| HP | 800 |
| Damage | 50 |
| Defense | 20 |
| AI | Hovers at medium range. Attacks on beats 1 and 3 of an internal 4-beat cycle (each beat = 30 frames = 0.5 seconds). Between attacks, slowly orbits the player. |
| Attack | Fires a pair of indigo energy bolts in a V-pattern toward the player. Bolts travel in straight lines at moderate speed. |
| Drops | Harmonic Voice Fragment (50%, 1-2), Polyphonic Bar (15%, 1-2) |

**Visual Design:** A tall, slender humanoid figure (~3x player height) made of polished dark chrome. Body is geometric and angular -- think a conductor's silhouette rendered in Art Deco metalwork. Head is a tall vertical rectangle with a single bass clef etched in glowing indigo. Arms are long, thin, and end in sharp blade-like hands. Torso has five horizontal staff lines running across it, with notes appearing and disappearing along them. Legs taper to points, hovering off the ground. Primary color: Chromatic Indigo with Graphite Void accents.

**Unique VFX:** Staff lines on its torso glow brighter 0.5 seconds before each attack (telegraph). Indigo energy bolts have a 3-pass trail with width tapering from 12f to 0. On hit, spawns a brief staff-line imprint at the impact point. On death, the staff lines on its body "unravel" -- peeling off as ribbons of light that dissolve into chromatic motes.

---

#### Second Voice Echo

> *"The answer to the subject -- the same melody, transposed, delayed, a reflection that speaks in a different key."*

| Property | Value |
|---|---|
| HP | 650 |
| Damage | 45 |
| Defense | 16 |
| AI | Follows the First Voice Sentinel's position with a 1-second delay (echo behavior). Attacks on beats 2 and 4 -- always offset from First Voice. If no First Voice is present, adopts independent AI with random-beat attacks. |
| Attack | Fires a single teal energy bolt that mirrors the last First Voice attack trajectory but inverted (if First Voice aimed up-right, Second Voice aims down-left). |
| Drops | Harmonic Voice Fragment (50%, 1-2) |

**Visual Design:** Similar geometric silhouette to First Voice but slightly shorter and with rounded edges instead of angular. Same dark chrome body but with Iridescent Teal as the primary accent instead of indigo. Head is a horizontal oval with a treble clef etched in teal light. Staff lines on torso, but the notes are offset by one beat from First Voice's notes (visible musical delay). Body has a slight translucency -- you can see a faint "afterimage" of the First Voice through it.

**Unique VFX:** A thin teal line connects it to the nearest First Voice Sentinel (visible harmonic thread). Its attacks produce a brief "echo" visual -- a ghostly duplicate of the projectile that appears 0.5 seconds after the real one along the same path (deals no damage, purely visual). On death, shatters into teal fragments that drift toward the nearest First Voice.

---

#### Third Voice Harmonic

> *"The countersubject -- a new melody that weaves between the subject and answer, filling their silences with its own voice."*

| Property | Value |
|---|---|
| HP | 700 |
| Damage | 55 |
| Defense | 18 |
| AI | Attacks on offbeats (the "and" between each beat of the 4-beat cycle). Positions itself in the gaps between First and Second Voice's positions. Moves quickly and erratically. |
| Attack | Fires rapid bursts of 3 small prismatic rose bolts in a tight spread. Each burst comes on an offbeat, filling the silences between First and Second Voice's attacks. |
| Drops | Harmonic Voice Fragment (50%, 1-2) |

**Visual Design:** Smaller than First and Second Voice (~1.5x player height). Body is sleek, curved chrome with Prismatic Rose as the primary accent. No legs -- instead, a flowing ribbon-tail of rose energy. Head is a diamond shape with a sharp flat sign etched in pink light. Body is covered in eighth-note and sixteenth-note notation -- the fast, dense notes between the slower subject and answer. Two thin arm-blades curve backward like scythe handles.

**Unique VFX:** Moves with a rapid, staccato quality -- sharp position changes with brief pauses between (visible as position-snap with a brief afterimage trail). Rose bolts are small but leave bright sparkle trails. On death, the ribbon-tail stays suspended in the air for 3 seconds as a damaging hazard before dissolving.

---

#### Fourth Voice Apex (Mini-Boss)

> *"The stretto -- all voices compressed into a single devastating point. It does not sing. It commands."*

| Property | Value |
|---|---|
| HP | 12,000 |
| Damage | 75 (contact), 60-90 (directed attacks) |
| Defense | 30 |
| AI | Hovers above the battlefield. Does not attack directly -- instead "conducts" the other three Voice types, synchronizing and amplifying their attacks. Every 15 seconds, performs a "Fermata" -- freezing all other Voices in place for 2 seconds, then releasing them in a simultaneous coordinated attack burst. |
| Special | While Fourth Voice is alive, all other Voice enemies deal +25% damage and gain +15% speed. Killing Fourth Voice causes remaining Voices to become "leaderless" -- their attack timing falls out of sync and they become erratic. |
| Drops | Harmonic Voice Fragment (100%, 3-5), Contrary Motion weapon (8%) |

**Visual Design:** The largest Voice entity (~4x player height). Body is a massive geometric conductor figure -- broad-shouldered, imposing, rendered in polished dark chrome with ALL four Polyphonic accent colors (indigo/teal/rose/amber) present as flowing veins across its surface. Head is a tall crown-like structure with four glowing points (one in each Voice color). Right arm holds a spectral conductor's baton made of white chrome light. Left arm is raised in a commanding gesture. Torso displays a full musical score with all four voice parts visible simultaneously. Hovers on a platform of interlocking chromatic rings.

**Unique VFX:** The conductor's baton leaves persistent light trails as it moves -- visible conducting patterns (downbeat, upbeat, cutoff gestures). When performing Fermata, a massive chromatic ring expands from it and "catches" all Voice enemies, holding them in place with visible energy tethers. The release burst produces a screen-flash in all four Voice colors simultaneously. On death, the baton falls and shatters on the ground, producing a cascade of four-colored sparks and a final sustained chord visual (four converging beams).

---

### 5.4 Post-Plantera Enemies

#### Phantom Violinist

> *"A ghost of the orchestra's first chair. Its phantom bow never stops moving."*

| Property | Value |
|---|---|
| HP | 3,500 |
| Damage | 70 (contact), 50 (projectile) |
| Defense | 28 |
| AI | Extremely mobile -- darts in fast diagonal lines, pausing briefly to "bow" (attack). Attack pattern is rapid: 3 fast lunges in different directions, then a 1-second pause to fire a volley of thin line-projectiles in a sweeping arc (like a bow stroke across strings). |
| Attack | Fires 5-7 thin indigo line-projectiles in a sweeping 120-degree arc. Lines travel fast and pierce one target each. |
| Drops | Convergence Fragment component (15%), Violin Bow vanity (5%) |

**Visual Design:** An elegant spectral figure in a formal concert dress/tuxedo rendered in sleek indigo-chrome metal. Holds a metallic phantom violin under its chin and a bow in its right hand. Face is a featureless chrome mask with two staff-line "eye slits." The violin itself is a crystalline construct with visible strings made of light. Legs are visible but translucent, trailing into mist below the knee. Body language is perpetually in mid-performance -- always moving, always bowing.

**Unique VFX:** The bow stroke is visible as a sweeping arc of indigo light (uses SemiCircularSmear). Each projectile "line" is actually a thin staff line with a note head at its leading edge. Movement trails leave brief afterimages that fade through the Polyphonic palette. On death, the violin breaks first (dramatic visual) and then the Violinist dissolves into a cascade of indigo notes.

---

#### Spectral Cellist

> *"Where the Violinist darts, the Cellist grounds. Its resonant blasts shake the earth."*

| Property | Value |
|---|---|
| HP | 4,500 |
| Damage | 85 (contact), 75 (projectile) |
| Defense | 32 |
| AI | Slow-moving, plants itself in one position and fires from range. Repositions every 8 seconds. Attacks are slow but devastating -- fires heavy resonant blasts that travel through blocks and explode on reaching the player's Y-level. |
| Attack | Fires a single large teal-amber resonant orb that travels slowly through terrain, exploding into a 6-tile radius damaging zone when it reaches the player's horizontal level. Zone persists for 3 seconds. |
| Drops | Convergence Fragment component (15%) |

**Visual Design:** A seated spectral figure holding a massive chromatic cello between its knees. Larger than the Violinist -- wide, grounded, stable. Body is teal-chrome with amber accent lines. The cello is a towering metallic instrument with visible sound waves rippling outward from its body constantly. Face is a broad chrome mask with a bass clef etched across it. Sits on an invisible chair -- legs are folded but trail into chromatic mist.

**Unique VFX:** The resonant orb projectile is a medium sphere with a 4-layer bloom stack (teal outer, amber mid, white core) that pulses with visible sound-wave rings expanding from it as it travels. When it moves through blocks, it leaves temporary "vibration cracks" in the terrain tiles (visual only). The explosion zone is a persistent chromatic circle with staff lines radiating outward from the center. On death, the cello strings snap one by one (4 sequential visual/sound effects) before the entity dissolves.

---

#### The Concertmaster (Mini-Boss)

*Detailed in Section 4.4 (Grand Rehearsal event). See Event 3 for full combat breakdown.*

| Property | Value |
|---|---|
| HP | 45,000 |
| Defense | 40 |
| Damage | 80 (contact), 60-100 (directed attacks) |

**Visual Design:** The tallest humanoid Polyphonic entity (~5x player height). A towering conductor figure in a formal white-chrome tailcoat over a graphite-void body. Face is a noble chrome mask with all five staff lines visible as glowing horizontal bands. Right hand holds the Concertmaster's Baton -- a slender white-chrome wand with a crystal tip that blazes with cycling Polyphonic colors. Left hand makes precise conducting gestures. A flowing cape of chromatic energy trails behind, displaying scrolling sheet music along its length. Four orbiting chromatic rings represent the four orchestra sections.

---

### 5.5 Post-Golem Enemy

#### Chromatic Harbinger

> *"It does not belong to any single key. It is all keys. It is the chromatic scale made flesh."*

| Property | Value |
|---|---|
| HP | 25,000 |
| Damage | 100 (contact), 80-120 (attacks, varies by semitone) |
| Defense | 45 |
| AI | Cycles through 12 attack modes, one per semitone (C through B). Spends 5 seconds in each key before advancing. Current key is displayed as its body color. Each key has a unique attack pattern. Full cycle takes 60 seconds. |
| Drops | Chromatic Core (20%), Cantus Firmus weapon (8%), Chromatic Alloy (50%, 2-4) |

**12 Semitone Attack Patterns:**

| Key | Color | Attack |
|---|---|---|
| C | Deep Void | Simple directional bolt (baseline) |
| C# | Midnight Indigo | Bolt that splits into 3 on impact |
| D | Chromatic Indigo | 4 bolts in a cross pattern |
| D# | Electric Cyan | Teleport-strike (appears at player, then fires) |
| E | Iridescent Teal | 8 bolts in a ring, slowly closing inward |
| F | Harmonic Emerald | Homing vine-like projectile (slow but persistent) |
| F# | Prismatic Rose | 3-burst volley with 0.5s spacing |
| G | Chromatic Magenta | Ground-wave that travels along floor |
| G# | Harmonic Amber | Rising pillar projectiles from below |
| A | Molten Gold | Rain of bolts from above, wide spread |
| A# | Iridescent Silver | Phase-shift attack (appears, fires, disappears, repeats 3x) |
| B | White Chrome Flash | ALL previous attacks fire simultaneously (reduced damage per bolt) |

**Visual Design:** A shifting, amorphous humanoid shape -- its body is constantly reconfiguring itself, flowing between 12 distinct geometric forms (one per semitone). Each form lasts 5 seconds. The body is a solid chrome shell with the current semitone's color blazing from every joint and seam. Head is a dodecahedron (12-faced polyhedron) where each face displays a different glyph. Height oscillates between 2x and 4x player height as it shifts keys. A chromatic scale notation spirals up its torso like a helix.

**Unique VFX:** Body color transitions are smooth (not instant) -- a 1-second chromatic morph between each key. Each attack type has its own unique VFX matching the semitone's color. On key-change, a brief chromatic ring expands outward. On death, all 12 colors fire simultaneously outward as 12 radial projectile beams, then the body collapses into a single white-chrome point before expanding into a final chromatic burst.

---

## 6. Weapons (23)

Every Polyphonic weapon is designed to look and feel like a *musical instrument merged with a weapon* -- sleek metallic forms with iridescent color dynamics, crystallized sound elements, and notation-based VFX. No two weapons play alike. Each has a unique secondary mechanic on top of the standard Harmonic Sear application.

**Cardinal Rule (inherited from Enhancements.md): EVERY WEAPON MUST BE UNIQUE.** No reskins. No stat swaps. Every weapon has its own attack pattern, VFX behavior, and secondary mechanic.

### 6.1 Weapon Master Table

| # | Weapon | Tier | Type | Damage | Use Time | Secondary Mechanic |
|---|---|---|---|---|---|---|
| 1 | Overtone Edge | Pre-HM | Sword | 22 | 28 | Swing spawns a delayed echo swing 0.5s later |
| 2 | Resonant Shortbow | Pre-HM | Bow | 18 | 24 | Arrows vibrate mid-flight, splitting into 2 at 50% distance |
| 3 | Dissonance Staff | Pre-HM | Staff | 24 | 32 | Fires a bolt that "detunes" on hit -- bounces between nearby enemies up to 3 times |
| 4 | Chromatic Whip | Pre-HM | Whip | 15 | 30 | Whip crack leaves a chromatic sigil on ground; walking over it buffs Polyphonic damage 10% for 5s |
| 5 | Contrary Motion | HM | Dual Swords | 42 | 18 | Left click swings right sword up, right click swings left sword down. Hitting with both in sequence creates a "contrary motion" shockwave |
| 6 | Augmented Longbow | HM | Bow | 38 | 22 | Charged shot fires an "augmented arrow" that ascends one semitone per enemy pierced (damage +8% per semitone) |
| 7 | Invertible Counterpoint | HM | Tome/Staff | 45 | 26 | Fires two beams simultaneously -- one up-right, one down-left (invertible). Beams swap direction every 4th cast |
| 8 | Suspension Chains | HM | Flail | 40 | 34 | On hit, "suspends" the enemy in harmonic stasis for 1.5s (frozen, takes +20% damage during stasis). Works on non-boss enemies only. |
| 9 | Pedal Tone Repeater | HM | Repeater | 30 | 10 | Every 5th shot fires a massive "pedal tone" blast (5x damage, huge projectile). Normal shots are small and fast. |
| 10 | Canon of Chromatic Fire | Post-Plantera | Launcher | 75 | 36 | Fires a slow orb that clones itself every 2 seconds (up to 4 clones). Each clone follows the original's path with a delay (musical canon). All detonate simultaneously. |
| 11 | Obligato Blade | Post-Plantera | Greatsword | 85 | 32 | Swing charges up based on Harmonic Sear stacks on nearby enemies. At 5+ stacks (any enemy in 30 tiles), swing releases a Full Chromatic Edge (screen-wide horizontal slash) |
| 12 | Ricercar | Post-Plantera | Staff | 70 | 28 | Fires a projectile that "searches" -- it spirals outward from the player in expanding circles until it finds a target, then homes aggressively. Musical term "ricercar" means "to search." |
| 13 | Pasacaglia's Descent | Post-Plantera | Spear | 80 | 24 | Downward thrust attack. Each consecutive thrust on the same enemy increases damage by 15% (stacks up to 5 times). Ground-based "ostinato" -- a repeated pattern that intensifies. |
| 14 | Toccata Gauntlets | Post-Golem | Fist Weapon | 95 | 8 | Extremely fast punch combo. Every 12th punch triggers a "toccata" -- a rapid-fire burst of 6 chromatic bolts in all directions. The speed and flashiness evoke keyboard virtuosity. |
| 15 | Cantus Firmus | Post-Golem | Broadsword | 110 | 30 | The "fixed song" -- a slow, powerful blade. While swinging, ALL other Polyphonic weapons held by allies in multiplayer deal +15% damage. In single player, gives a 15% damage buff to the player's next weapon switch for 3s. |
| 16 | Twelve-Tone Row | Post-Golem | Gun | 50 | 6 | Fires 12 shots in sequence, each a different color from the extended palette. The 12th shot (White Chrome Flash) deals 4x damage. After the 12th shot, brief reload. |
| 17 | Isorhythmic Motet | Post-Golem | Summon Staff | 55 (per hit) | 36 | Summons a "Motet Sentinel" -- a floating chromatic orb that attacks in a fixed rhythmic pattern (1 attack per 2 seconds, always). The rhythm never changes -- ISORHYTHMIC -- but each successive attack in the pattern deals escalating damage (1x, 1.2x, 1.4x, 1.6x, then resets). |
| 18 | Ground Bass Anchor | Post-ML | Hammer | 180 | 38 | Downward slam creates a "ground bass" -- a persistent bass frequency wave that travels along the ground for 10 tiles in each direction, dealing continuous damage to all enemies standing on the ground. Lasts 4 seconds. Each subsequent slam extends the wave's range by 3 tiles. |
| 19 | Stretto Convergence | Post-ML | Twin Blades | 140 | 14 | Dual-wield blades that attack progressively faster (stretto = voices entering closer and closer together). After 8 swings, the blades overlap completely and deal a single combined strike at 3x damage. Then the tempo resets. |
| 20 | Harmonic Series Bow | Post-ML | Bow | 130 | 20 | Charged shot fires 1/2/3/4/5 arrows (corresponding to harmonic overtones 1-5). Each additional arrow deals 50% less damage than the previous but homes increasingly aggressively. Full charge = 5 arrows in a widening harmonic fan. |
| 21 | Chromatic Fantasy | Post-ML | Magic Weapon | 160 | 24 | Fires a beam that cycles through all 12 extended palette colors over its lifetime. Each color applies a different minor debuff. Hitting with all 12 colors within 10 seconds triggers "Chromatic Saturation" -- a 5-second damage amp of +25% on the target. |
| 22 | The Unfinished Fugue | Post-ML (Rare) | Greatsword | 200 | 28 | Each swing is one "voice" of a fugue -- the first swing fires a projectile forward, the second swing fires a projectile backward (answer), the third swing fires projectiles in both directions simultaneously (stretto). After the 3rd swing, a "coda" blast fires in all 8 directions for massive damage. |
| 23 | Omni Voce | Post-ML (Ultimate) | Transforming | 250 | varies | The capstone. Cycles between 4 weapon forms (sword/bow/staff/summon) every 5 seconds automatically. Each form uses the Polyphonic damage class. On form change, releases a chromatic pulse that applies 2 Harmonic Sear stacks to all nearby enemies. After cycling through all 4 forms, enters "Tutti" mode for 10 seconds -- all 4 forms attack simultaneously. |

---

### 6.2 Pre-Hardmode Weapons (4)

#### Weapon 1: Overtone Edge

> *"A blade forged from Polyphonic Bar. When swung, it leaves behind an echo of itself -- a ghost swing that strikes a moment later, as if the blade's voice needed to finish its phrase."*

| Property | Value |
|---|---|
| Damage | 22 Polyphonic |
| Use Time | 28 (Average) |
| Knockback | 4 (Weak) |
| Autoswing | Yes |
| Recipe | 10 Polyphonic Bar + 5 Voicestone Shard @ Iron/Lead Anvil |
| Rarity | PolyphonicRarity (Tier 1) |

**Secondary Mechanic -- Echo Swing:** 0.5 seconds after each swing, a ghostly duplicate of the swing arc appears and deals 60% of the original damage. The echo is translucent and colored in Iridescent Teal (shifted from the original Chromatic Indigo swing). This means each swing effectively hits twice, but with a delay that enemies can escape if they're fast.

**Visual Design:** A sleek, angular longsword with a blade made of polished chrome-indigo metal. The blade surface has a subtle oil-slick iridescence that shifts between indigo and teal as the viewing angle changes. The guard is two overlapping tuning-fork prongs in teal chrome. The handle is wrapped in dark graphite-void material with a single staff line inlaid in amber. Pommel is a small chromatic crystal that glows with the Polyphonic palette. Overall shape is clean and geometric -- more Art Deco than medieval.

**VFX Breakdown:**
- **Swing arc:** Indigo SemiCircularSmear with 3-pass trail (indigo outer, teal mid, white core). Width tapers from 20f to 0.
- **Echo swing:** Same arc but in Iridescent Teal, 60% opacity, with a visible "delay line" (several faint copies of the arc between the real and echo position, like animation frames).
- **Impact:** Cadence Burst (thin radial lines, indigo-to-amber fade). Chromatic Mote particles on hit.
- **Idle:** Faint chromatic mote particles drift off the blade tip. The iridescent surface shimmer is visible even when stationary.

---

#### Weapon 2: Resonant Shortbow

> *"Its arrows don't just fly -- they sing. At the peak of their arc, each arrow's vibration reaches a critical frequency and the shaft splits into two harmonics."*

| Property | Value |
|---|---|
| Damage | 18 Polyphonic |
| Use Time | 24 (Fast) |
| Knockback | 2 (Very Weak) |
| Velocity | 8 |
| Recipe | 8 Polyphonic Bar + 3 Voicestone Shard + 15 Wood @ Iron/Lead Anvil |
| Ammo | Wooden Arrows (converts to Resonant Arrows) |
| Rarity | PolyphonicRarity (Tier 1) |

**Secondary Mechanic -- Harmonic Split:** Arrows convert to Resonant Arrows on fire. At 50% of their max travel distance, each arrow vibrates visually and splits into 2 arrows that diverge at 15-degree angles. Each split arrow deals 70% of original damage. Effectively doubles the hit chance at range while reducing per-hit damage.

**Visual Design:** A compact recurve bow made of layered chrome-teal metal strips that interweave like braided metal. The string is a visible line of teal light (not a physical string). The limb tips end in small tuning-fork shapes that vibrate when drawn. The grip is graphite-void metal with an indigo inlay. When drawn, the limbs flex and the tuning-fork tips glow brighter. Arrows fired from it convert into chrome-and-teal elongated projectiles with staff-line fletching.

**VFX Breakdown:**
- **Arrow flight:** Chrome body with teal glow trail. Trail is a thin iridescent ribbon (width 6f, tapers to 0).
- **Split moment:** At 50% distance, the arrow flashes white-chrome and a brief "tuning fork" visual appears -- two prongs of light diverging. The two new arrows have slightly different teal hue (one more green, one more blue).
- **Impact:** Small Cadence Burst at point of impact. Single Voice Note particle (teal).

---

#### Weapon 3: Dissonance Staff

> *"This staff channels the raw dissonance of Polyphonic energy, firing bolts that can't hold their frequency -- they 'detune' on impact and ricochet to nearby enemies, seeking new frequencies to latch onto."*

| Property | Value |
|---|---|
| Damage | 24 Polyphonic |
| Use Time | 32 (Slow) |
| Knockback | 3 (Very Weak) |
| Mana Cost | 8 |
| Recipe | 12 Polyphonic Bar + 3 Voicestone Shard + 5 Fallen Star @ Iron/Lead Anvil |
| Rarity | PolyphonicRarity (Tier 1) |

**Secondary Mechanic -- Detuning Bounce:** The bolt "detunes" on first impact -- it loses its original frequency and bounces toward the nearest enemy within 15 tiles. Each bounce shifts the bolt's color one step along the Polyphonic palette (indigo -> teal -> rose) and deals 80% of the previous hit's damage. Max 3 bounces.

**Visual Design:** A staff (not a wand -- full-length, held in both hands) made of spiraling chrome-indigo and chrome-rose metal strands twisted around each other like a double helix. The head of the staff is an open circle (like a whole note in music notation) made of white chrome, with a small chromatic crystal suspended in its center. The base has a sharp point of graphite-void metal. Staff lines wrap around the shaft like engraved vines.

**VFX Breakdown:**
- **Projectile:** A medium orb (scale 0.6f) in Chromatic Indigo with a 4-layer bloom (indigo outer, teal mid-outer, rose mid-inner, white core). Trails an iridescent ribbon.
- **Bounce:** On hit, the orb briefly expands (flash), shifts to the next palette color (visible color morph over 0.2 seconds), and launches toward the next target with a new trail color. Each bounce produces a Cadence Burst at the bounce point.
- **Final impact:** After 3 bounces (or if no target found), the bolt detonates into a burst of chromatic motes and Voice Note particles.

---

#### Weapon 4: Chromatic Whip

> *"A whip of crystallized chromatic light. Its crack leaves a sigil on the ground -- a ward of harmonic power that empowers those who walk across it."*

| Property | Value |
|---|---|
| Damage | 15 Polyphonic (tag damage adds +5 per hit from summons) |
| Use Time | 30 (Average) |
| Knockback | 2 (Very Weak) |
| Recipe | 6 Polyphonic Bar + 8 Voicestone Shard + 3 Cobweb @ Iron/Lead Anvil |
| Rarity | PolyphonicRarity (Tier 1) |

**Secondary Mechanic -- Chromatic Sigil:** Each whip crack leaves a glowing chromatic sigil on the ground at the point of maximum extension. Sigil is a circle (~3 tiles diameter) with staff-line patterns inside. Standing on the sigil grants +10% Polyphonic damage for 5 seconds. Sigils last 8 seconds. Max 3 sigils active at once (oldest despawns when 4th is placed).

**Visual Design:** A long, thin whip made of segmented chrome-amber plates linked by teal energy threads. The handle is a short graphite-void grip with an angular guard shaped like a sharp sign (#). The whip's segments get smaller toward the tip, ending in a tiny chromatic crystal that flashes on crack. When fully extended, the whip straightens momentarily into a line of pure chromatic light before retracting.

**VFX Breakdown:**
- **Whip motion:** Amber-to-teal gradient along the whip's length. Each segment leaves a brief afterimage trail in its palette color.
- **Crack:** White Chrome Flash at the tip, radial Cadence Burst. Audible "crack" should sync with visual.
- **Sigil on ground:** A circle of rotating staff lines in chromatic colors (indigo/teal/rose cycling slowly). The sigil glows brighter when a player stands on it. Small Voice Note particles drift upward from the sigil.
- **Buff application:** Brief full-body chromatic flash on the player when entering the sigil.

---

### 6.3 Hardmode Weapons (5)

#### Weapon 5: Contrary Motion

> *"Two blades that move in opposition -- one rises as the other falls. When their paths cross, the collision generates a shockwave that obliterates everything between them."*

| Property | Value |
|---|---|
| Damage | 42 Polyphonic (each blade) |
| Use Time | 18 (Very Fast) |
| Knockback | 5 (Average) |
| Autoswing | Yes |
| Source | Fourth Voice Apex drop (8%) OR crafted: 15 Chromatic Alloy + 10 Harmonic Voice Fragment @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 2) |

**Secondary Mechanic -- Contrary Motion Shockwave:** Left click swings the right blade in an upward arc (indigo). Right click swings the left blade in a downward arc (teal). If both blades hit the same enemy within 0.3 seconds of each other, they generate a "contrary motion" shockwave -- a horizontal energy wave that deals 2x combined damage and travels 20 tiles in both directions. The shockwave pierces indefinitely.

**Visual Design:** Twin short swords connected at the pommel by a chrome chain. Right blade is Chromatic Indigo chrome with ascending staff-line engravings (notes climbing upward). Left blade is Iridescent Teal chrome with descending staff-line engravings (notes falling downward). The chain connecting them pulses with alternating indigo-teal light. When held, them both extend outward in an X pattern. Guard on each is a half-circle that, when brought together, forms a complete circle of white chrome.

**VFX Breakdown:**
- **Right swing (up):** Indigo SemiCircularSmear, upward arc, 3-pass trail (indigo/amber/white).
- **Left swing (down):** Teal SemiCircularSmear, downward arc, 3-pass trail (teal/rose/white).
- **Contrary Motion Shockwave:** A horizontal beam-slash of combined indigo+teal energy, with a visible "wave collision" pattern (two conflicting wave forms). Width 8f, travels fast, leaves brief staff-line imprints along its path.
- **Combo trigger:** When both blades connect, a bright white-chrome flash and a circular Cadence Burst at the impact point. The chain between the swords crackles with energy for 1 second after.

---

#### Weapon 6: Augmented Longbow

> *"Each enemy this arrow pierces raises its pitch by one semitone. By the time it's passed through four foes, it's singing in a key that shatters armor."*

| Property | Value |
|---|---|
| Damage | 38 Polyphonic |
| Use Time | 22 (Fast) |
| Knockback | 3 (Weak) |
| Velocity | 10 |
| Recipe | 12 Chromatic Alloy + 8 Harmonic Voice Fragment + 1 Soul of Light @ Mythril/Orichalcum Anvil |
| Ammo | Any Arrow (converts to Augmented Arrows) |
| Rarity | PolyphonicRarity (Tier 2) |

**Secondary Mechanic -- Semitone Ascent:** Augmented Arrows pierce enemies. Each enemy pierced "raises" the arrow by one semitone -- its color shifts through the extended palette and its damage increases by 8% per pierce. After 12 pierces (a full chromatic octave), the arrow becomes a "Chromatic Arrow" that deals 2x base damage and applies 3 Harmonic Sear stacks. Max pierce: 12. Arrow's visual size increases slightly with each ascent.

**Visual Design:** A tall longbow (1.5x standard bow height) made of stacked chrome plates that fan out like organ pipes. Each "pipe" is a different color from the extended palette. The string is a dual-thread of indigo and teal light. The riser (grip section) is solid Enharmonic Alloy-style chrome with hexagonal lattice surface. Arrows convert into long, thin projectiles resembling tuning forks with a sharp arrowhead tip, cycling in color based on current semitone.

**VFX Breakdown:**
- **Arrow in flight:** Starts as Chromatic Indigo arrow with a thin trail. Each pierce shifts the color and the trail gets 2f wider. After 6 pierces, the arrow is visibly larger and blazing with multiple colors.
- **Per-pierce:** Brief flash + ascending pitch tone (visual: the arrow leaves a small vertical line at each pierce point, forming a visible ascending chromatic scale behind it).
- **12th pierce (Chromatic Arrow):** Arrow becomes a blazing white-chrome bolt with a full rainbow trail, trailing Voice Notes and Chromatic Motes.

---

#### Weapon 7: Invertible Counterpoint

> *"A tome that fires two beams in opposite directions simultaneously. Every fourth cast, the directions invert. The enemy never knows which way the attack will come."*

| Property | Value |
|---|---|
| Damage | 45 Polyphonic (per beam) |
| Use Time | 26 (Average) |
| Knockback | 4 (Weak) |
| Mana Cost | 14 |
| Recipe | 15 Chromatic Alloy + 5 Harmonic Voice Fragment + 1 Soul of Night + 1 Soul of Light @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 2) |

**Secondary Mechanic -- Inversion Cycle:** Fires two beams simultaneously: one toward the cursor (indigo) and one in the exact opposite direction (teal). Every 4th cast, the beams "invert" -- the indigo beam goes to the opposite direction and the teal beam goes toward the cursor. This creates an unpredictable (for enemies) but trackable (for the player, via a visible counter) attack pattern.

**Visual Design:** An open tome floating beside the player (held with one hand extended). The tome's cover is polished chrome with a treble clef on the front and an inverted treble clef on the back. Pages are sheets of chromatic metal that flutter with internal light. When casting, both covers open wide and the pages fan outward, emitting twin beams from the spine. A small "cast counter" appears near the player as 4 dots (filling one per cast, resetting after inversion).

**VFX Breakdown:**
- **Primary beam (toward cursor):** Chromatic Indigo column beam, medium width (10f), 3-layer bloom, length extends to 30 tiles or first contact.
- **Opposite beam:** Iridescent Teal column beam, same specs but traveling the other direction.
- **Inversion (every 4th cast):** Both beams flash white-chrome briefly, the cast counter circle explodes in a small Cadence Burst, and the colors swap.
- **Impact:** Each beam creates a staff-line imprint at termination point + Voice Note burst.

---

#### Weapon 8: Suspension Chains

> *"This flail doesn't just hit -- it suspends. It catches an enemy in a moment of harmonic stasis, frozen between resolutions, vulnerable to everything."*

| Property | Value |
|---|---|
| Damage | 40 Polyphonic |
| Use Time | 34 (Slow) |
| Knockback | 6 (Strong) |
| Recipe | 10 Chromatic Alloy + 12 Harmonic Voice Fragment + 15 Chain @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 2) |

**Secondary Mechanic -- Harmonic Suspension:** On contact with a non-boss enemy, the flail wraps chromatic chains around it, freezing it in "suspension" for 1.5 seconds. During suspension, the enemy takes +20% damage from ALL sources and gains a visible vibrating chromatic aura. After suspension ends, the enemy cannot be suspended again for 5 seconds (cooldown per enemy). Boss enemies are immune to the freeze but still take a 0.5-second slow (50% movement reduction).

**Visual Design:** A flail head shaped like a fermata symbol (the arc-over-a-dot that means "hold this note"). The head is polished rose-chrome metal with amber inlays. The chain is made of interlocking chromatic links, each a different Polyphonic palette color. The handle is a short graphite-void grip with staff-line engravings. When spinning, the chain links blur into a continuous chromatic spiral. The fermata head leaves a persistent glow trail.

**VFX Breakdown:**
- **Spin:** Chain links create a chromatic spiral trail (all 6 core palette colors in sequence along the spiral). The fermata head at the end has a 3-layer bloom (rose outer, amber mid, white core).
- **Impact:** White Chrome Flash burst + Cadence Burst. The fermata arc visual expands briefly over the enemy.
- **Suspension state:** Enemy is encased in visible chromatic chains (4 rings of alternating indigo/teal/rose/amber that orbit the frozen enemy). The enemy's sprite gains a vibrating/jittering overlay. Chromatic Motes drift downward from the chains.
- **Suspension break:** Chains shatter outward as chromatic shards + a brief sound-wave ring expands from the enemy's position.

---

#### Weapon 9: Pedal Tone Repeater

> *"Ninety-nine shots of crystallized overtones, rapid and precise. But every hundredth... no, every fifth -- a pedal tone. The fundamental. The bass note that shakes foundations."*

| Property | Value |
|---|---|
| Damage | 30 Polyphonic (normal shot), 150 Polyphonic (pedal tone, every 5th shot) |
| Use Time | 10 (Very Fast) |
| Knockback | 2 (Very Weak) normal, 8 (Very Strong) pedal tone |
| Velocity | 12 |
| Recipe | 12 Chromatic Alloy + 6 Harmonic Voice Fragment + 1 Illegal Gun Parts @ Mythril/Orichalcum Anvil |
| Ammo | Musket Balls (converts to Resonant Bullets) |
| Rarity | PolyphonicRarity (Tier 2) |

**Secondary Mechanic -- Pedal Tone:** Standard shots are small, fast chromatic bullets. Every 5th shot automatically becomes a "Pedal Tone" -- a massive bass-frequency blast that deals 5x damage, has a huge projectile sprite, applies 2 Harmonic Sear stacks, and creates a screen-shake effect. A visible shot counter (5 small dots near the crosshair) tracks progress toward the next Pedal Tone. The Pedal Tone has significant knockback and a wider hitbox.

**Visual Design:** A repeater crossbow made of layered chrome-teal plates, shaped like a compressed piano keyboard (the body has alternating indigo and white chrome "keys" along its length). The rail is a long teal-chrome tube. The stock is graphite-void with amber accent lines. A small mechanical counter (visible as 5 tiny chromatic dots) sits atop the weapon. When the Pedal Tone is ready, all 5 dots merge into a single large pulsing orb of amber light.

**VFX Breakdown:**
- **Normal shot:** Small teal bullet (scale 0.3f) with a minimal trail (width 3f, teal, tapers to 0). Fast and clean.
- **5th shot indicator:** The 5 dots near the crosshair fill one by one (indigo, teal, rose, amber, white). When all 5 are full, they pulse.
- **Pedal Tone shot:** Massive amber-core blast (scale 1.5f) with a 5-layer bloom (amber outer, rose, teal, indigo, white core). Trail is wide (20f) and leaves visible sound-wave rings along its path. Screen shake on fire. Impact creates a 4-tile radius Cadence Burst + ground crack visual.

---

### 6.4 Post-Plantera Weapons (4)

#### Weapon 10: Canon of Chromatic Fire

> *"A canon in music is one melody chasing itself. This launcher fires an orb that births copies of itself, each following the original's path with perfect delay -- a canon of destruction."*

| Property | Value |
|---|---|
| Damage | 75 Polyphonic (per orb detonation) |
| Use Time | 36 (Very Slow) |
| Knockback | 7 (Strong) |
| Recipe | 5 Convergence Fragment + 15 Chromatic Alloy + 1 Concertmaster's Baton @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 3) |

**Secondary Mechanic -- Musical Canon:** Fires a single slow-moving orb (velocity 4). Every 2 seconds, the orb spawns a clone of itself at its current position. The clone then follows the original's exact recorded path from the start, delayed by 2 seconds. Max 4 clones. When the original detonates (on enemy contact or max range), ALL clones detonate simultaneously regardless of position. 5 simultaneous explosions = devastating area damage. Total damage potential: 375 Polyphonic (5 x 75) if all orbs are in range.

**Visual Design:** A crystalline launcher shaped like a miniature pipe organ -- 5 chrome pipes of decreasing size fanned outward from a central stock. Each pipe is a different Polyphonic accent color (indigo/teal/rose/amber/white chrome). The stock is graphite-void with hexagonal-lattice chrome panels. A chromatic crystal charging chamber sits at the base of the pipes. When fired, the orb exits from the center pipe and each clone uses a different pipe.

**VFX Breakdown:**
- **Primary orb:** Large sphere (scale 1.0f) in Chromatic Indigo with 4-layer bloom. Slow-moving, trails a wide iridescent ribbon (width 12f).
- **Clone spawn:** Flash + Cadence Burst at spawn point. Each clone is a different accent color (teal for 1st copy, rose for 2nd, amber for 3rd, white for 4th).
- **Following path:** Clones follow the EXACT path the original took, visible as fading trail lines that the clones track along.
- **Simultaneous detonation:** All 5 orbs explode at once with 5 overlapping Cadence Bursts. The combined explosion creates a full-palette chromatic flower burst (5 colors radiating from 5 points). Intense bloom. Screen-shake.

---

#### Weapon 11: Obligato Blade

> *"An obligato is a part that cannot be omitted -- it is essential. This blade becomes essential the moment your enemies are burning with harmonic fire, because that is when it unleashes its true power."*

| Property | Value |
|---|---|
| Damage | 85 Polyphonic (normal swing), 340 Polyphonic (Full Chromatic Edge) |
| Use Time | 32 (Slow) |
| Knockback | 6 (Strong) |
| Autoswing | Yes |
| Recipe | 8 Convergence Fragment + 10 Chromatic Alloy + 5 Soul of Might @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 3) |

**Secondary Mechanic -- Full Chromatic Edge:** The blade monitors Harmonic Sear stacks on all enemies within 30 tiles. When the total stacks across all enemies reaches 5 or more, the next swing releases a "Full Chromatic Edge" -- a screen-wide horizontal energy slash that deals 4x damage, pierces infinitely, and applies 2 Harmonic Sear stacks to everything it touches. The stack total resets after triggering. A visible meter on the player shows current accumulated stacks (0-5+).

**Visual Design:** A massive greatsword (held two-handed, 2x player height). The blade is wide and flat, made of layered chrome in all Polyphonic accent colors -- visible as horizontal color bands (like a geological cross-section of chromatic metal, indigo at the base through white chrome at the edge). The flat of the blade has staff lines engraved lengthwise with notation that scrolls when the weapon charges. Guard is a wide horizontal bar of amber chrome. Grip is long (two-handed), wrapped in graphite-void with teal inlay spirals.

**VFX Breakdown:**
- **Normal swing:** Wide SemiCircularSmear in the blade's dominant color band (shifts each swing: indigo, teal, rose, amber cycle). 3-pass trail, width 24f.
- **Charge indicator:** The staff lines on the blade glow progressively brighter as nearby Harmonic Sear stacks accumulate. At 5 stacks, the entire blade blazes white-chrome and the notation speed doubles.
- **Full Chromatic Edge:** Screen-wide horizontal slash line (60+ tiles) in full Polyphonic palette -- a gradient beam from indigo (far left) through teal/rose/amber to white chrome (far right). Trail persists for 0.8 seconds. Every enemy hit produces a Cadence Burst. On release, a brief screen-flash of white chrome.

---

#### Weapon 12: Ricercar

> *"'Ricercar' -- to search. This projectile does not fly in a straight line. It spirals outward, seeking, searching for a target. And when it finds one, it does not miss."*

| Property | Value |
|---|---|
| Damage | 70 Polyphonic |
| Use Time | 28 (Average) |
| Knockback | 5 (Average) |
| Mana Cost | 18 |
| Recipe | 5 Convergence Fragment + 8 Chromatic Alloy + 10 Soul of Sight @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 3) |

**Secondary Mechanic -- Seeking Spiral:** The projectile does not fire toward the cursor. Instead, it launches from the player and spirals outward in expanding circles (search pattern). The spiral radius increases by 3 tiles per revolution. The projectile persists for up to 5 seconds or until it detects an enemy within 8 tiles of its current position. Upon detection, the projectile abandons its spiral and homes aggressively toward the target (tracking speed: 15 degrees/frame). If it kills the target, it resumes spiraling. Max 2 kills per projectile.

**Visual Design:** A tall staff (taller than the player) topped with a magnifying-glass-shaped head made of chrome and crystal. The "lens" of the magnifying glass is a floating chromatic crystal that spins. The shaft is twisted chrome-teal metal with rose accent rings at even intervals. The base is a sharp graphite-void point. When cast, the magnifying lens head glows and the crystal inside spins faster.

**VFX Breakdown:**
- **Spiral flight:** Projectile is a medium orb (rose/teal split coloring) trailing a thin iridescent spiral ribbon. The spiral trail is visible as the projectile's path, creating an expanding helix pattern on screen.
- **Search mode:** The orb pulses gently and emits small "sonar" rings (expanding circles, teal, every 0.5 seconds).
- **Target lock:** When a target is detected, the orb flashes white-chrome, the sonar rings collapse inward, and the projectile streaks toward the target with a bright aggressive trail (width increases to 10f, color shifts to amber).
- **Impact:** Cadence Burst + Voice Note explosion.

---

#### Weapon 13: Pasacaglia's Descent

> *"A pasacaglia is a musical form built on a repeating bass pattern -- an ostinato that grounds everything above it. This spear strikes the same point, over and over, building power with each repetition."*

| Property | Value |
|---|---|
| Damage | 80 Polyphonic (base), up to 160 Polyphonic at max stacks |
| Use Time | 24 (Fast) |
| Knockback | 4 (Weak) |
| Recipe | 6 Convergence Fragment + 12 Chromatic Alloy + 10 Soul of Fright @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 3) |

**Secondary Mechanic -- Ostinato Building:** This is a downward-thrust spear (not a standard poke). Each consecutive thrust on the *same enemy* increases damage by 15% (stacks up to 5x for +75% total). Switching targets resets the counter. The stacking embodies the pasacaglia's repeating bass pattern -- the same action, building intensity. At max stacks (5), the final thrust releases a ground-quake that damages all enemies within 8 tiles.

**Visual Design:** A heavy polearm designed for downward strikes -- the blade is a wide, downward-pointing chrome arrowhead in amber chrome with indigo edge accents. The shaft is long, straight, and made of chrome-graphite-void metal with evenly-spaced teal rings (like measure bars on a musical staff). The butt end has a small amber crystal counterweight. The overall silhouette is vertical and grounded -- this weapon is about stability and repetition, not flashy acrobatics.

**VFX Breakdown:**
- **Thrust:** Downward strike with an amber-core impact flash. Each hit on the same enemy adds a visible ring around the target (indigo for 1st, teal for 2nd, rose for 3rd, amber for 4th, white for 5th).
- **Stack building:** The spear's shaft rings glow progressively brighter with each same-target hit. The staff-line rings cycle from dim to blazing.
- **Max stack ground-quake:** At 5 stacks, the final thrust creates a ground-pound shockwave -- expanding amber-to-indigo ring along the ground (width 4f, travels 8 tiles) + vertical Cadence Burst pillars at the impact point. Screen shake. All 5 target rings on the enemy explode simultaneously.

---

### 6.5 Post-Golem Weapons (4)

#### Weapon 14: Toccata Gauntlets

> *"A toccata is a showpiece -- a composition designed to demonstrate the performer's technical brilliance. These gauntlets are the showpiece. Fast, flashy, and utterly devastating."*

| Property | Value |
|---|---|
| Damage | 95 Polyphonic |
| Use Time | 8 (Insanely Fast) |
| Knockback | 3 (Weak) |
| Recipe | 3 Enharmonic Alloy + 8 Chromatic Core + 15 Shroomite Bar @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 4) |

**Secondary Mechanic -- Toccata Burst:** The gauntlets punch at extreme speed (use time 8). Every 12th consecutive punch triggers a "Toccata" -- an instantaneous burst of 6 chromatic bolts fired in evenly-spaced directions (360/6 = 60 degrees apart). Each bolt deals 50% weapon damage and applies 1 Harmonic Sear stack. A visible combo counter (12 segments of a circle around the player) tracks progress. Missing more than 1 second between punches resets the counter. At max combo, the player briefly glows with a full-palette chromatic aura.

**Visual Design:** A pair of chrome gauntlets (worn on both hands) with a keyboard motif. Each finger plate resembles a piano key -- alternating white chrome and indigo chrome plates. The knuckle guards are raised chrome ridges with amber crystal insets. The wrist guards have staff-line engravings. The overall look is sleek, technical, and precise -- a musician's hands reimagined as weapons. When punching, the "keys" visually depress and light up in sequence.

**VFX Breakdown:**
- **Each punch:** Quick, sharp flash at point of impact -- a different Polyphonic color for each sequential punch in the combo (cycles through the 6 core palette colors, 2 punches per color). Minimal trail (width 4f) for speed feel.
- **Combo counter:** 12-segment circle around the player fills one bright segment per punch (colors cycle through the palette). Glows subtly.
- **Toccata Burst (12th punch):** All 12 counter segments flash simultaneously and collapse inward. 6 chromatic bolts fire outward with individual trails (one per accent color + white chrome). Each bolt has a thin ribbon trail. Brief full-body chromatic aura on the player (0.5 seconds).
- **Idle:** Faint Chromatic Motes drift off the knuckles. Keys glow softly.

---

#### Weapon 15: Cantus Firmus

> *"The cantus firmus -- the 'fixed song.' An unchanging foundation melody upon which all other voices build. This blade IS the foundation. While it sings, everything else hits harder."*

| Property | Value |
|---|---|
| Damage | 110 Polyphonic |
| Use Time | 30 (Average) |
| Knockback | 7 (Strong) |
| Autoswing | Yes |
| Source | Chromatic Harbinger drop (8%) OR crafted: 4 Enharmonic Alloy + 5 Chromatic Core + 10 Chlorophyte Bar @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 4) |

**Secondary Mechanic -- Foundation Buff:** While actively swinging, all OTHER Polyphonic weapons used by the player (on next weapon switch) or allies (in multiplayer, in real-time) deal +15% damage. This buff lasts 3 seconds after the player stops swinging. The Cantus Firmus itself is a solid, reliable weapon, but its true power is amplifying everything else. The buff is visible as a subtler chromatic aura on the buffed player(s) labeled "Cantus Firmus" in the buff bar.

**Visual Design:** A classical broadsword with a straight, wide blade. The blade is polished amber-chrome metal with a single, perfectly straight white chrome line running down the center (the cantus firmus -- one unwavering note). The flat of the blade has no ornamentation except for that single line. The guard is a wide, stable cross-guard in graphite-void chrome. The handle is amber-wrapped. The pommel is a single, uncut chromatic crystal -- raw, foundational, unprocessed. The overall design is deliberately understated compared to other Polyphonic weapons -- it is the foundation, not the ornament.

**VFX Breakdown:**
- **Swing arc:** Amber SemiCircularSmear with 3-pass trail (amber outer, white core). Clean, stable, not overly flashy.
- **Foundation line:** The white chrome center line of the blade glows brighter during the swing, leaving a persistent trace for 0.3 seconds.
- **Buff aura (on allies/self):** Subtle amber-white chromatic glow around the buffed player. Brief notification particle: a single whole note (staff notation) that appears and fades above the player's head when the buff activates.
- **Impact:** Solid Cadence Burst (amber) + single Voice Note (amber whole note shape).

---

#### Weapon 16: Twelve-Tone Row

> *"Arnold Schoenberg's twelve-tone technique requires that all 12 notes of the chromatic scale be used before any can be repeated. This gun fires 12 unique shots -- one for each semitone -- then reloads. The 12th shot is transcendent."*

| Property | Value |
|---|---|
| Damage | 50 Polyphonic (shots 1-11), 200 Polyphonic (shot 12) |
| Use Time | 6 (Insanely Fast) |
| Knockback | 2 (Very Weak) normal, 6 (Strong) shot 12 |
| Velocity | 14 |
| Recipe | 5 Enharmonic Alloy + 6 Chromatic Core + 1 Vortex Fragment (any quantity) @ Ancient Manipulator |
| Ammo | Any Bullet (converts to Twelve-Tone Rounds) |
| Rarity | PolyphonicRarity (Tier 4) |

**Secondary Mechanic -- Twelve-Tone Sequence:** The gun fires 12 bullets in sequence, each a different color from the extended palette (Deep Void through White Chrome Flash). Each bullet has a slightly different property (varying speed, slight homing, varied knockback). The 12th shot (White Chrome Flash) deals 4x base damage, has a large projectile, pierces 3 enemies, and produces a massive visual. After the 12th shot, a 1-second reload pause before the sequence repeats. A visible magazine indicator shows the current shot number (1-12) with the corresponding color.

**Visual Design:** A sleek, chrome pistol-rifle hybrid with a cylindrical magazine that has 12 chambers, each filled with a chromatic crystal of a different extended palette color. The barrel is long and thin (sniper-esque) made of iridescent chrome. The stock is graphite-void with staff-line engravings. As each shot fires, the corresponding chamber's crystal dims and the magazine rotates to the next. The gun's body shifts color to match the current chamber's color.

**VFX Breakdown:**
- **Shots 1-11:** Small bullet projectiles each in their corresponding extended palette color. Thin trail (width 3f). Each impact produces a small burst in that color. The gun itself shifts color to match.
- **Shot 12 (White Chrome Flash):** Magazine chambers all re-ignite simultaneously. The gun blazes white. The projectile is a large chrome-white bolt with a 5-layer bloom and a wide trail (width 15f). Impact creates a large Cadence Burst + Voice Note shower. Brief screen-flash.
- **Reload:** The 12 chambers recharge one by one (visual: tiny chromatic crystals flickering back to life in sequence over 1 second). Subtle "winding up" sound visual.

---

#### Weapon 17: Isorhythmic Motet

> *"An isorhythmic motet uses repeating rhythmic patterns that cycle independently of the melodic patterns. This summon attacks in a fixed rhythm -- always -- but its melody (damage) evolves with each repetition."*

| Property | Value |
|---|---|
| Damage | 55 Polyphonic (per hit, scaling: 1x/1.2x/1.4x/1.6x cycle) |
| Use Time | 36 (Very Slow) |
| Knockback | 3 (Weak) |
| Mana Cost | 25 |
| Recipe | 4 Enharmonic Alloy + 4 Chromatic Core + 10 Ectoplasm @ Mythril/Orichalcum Anvil |
| Rarity | PolyphonicRarity (Tier 4) |
| Summon Slots | 1 per Motet Sentinel |

**Secondary Mechanic -- Isorhythmic Pattern:** Summons a Motet Sentinel -- a floating chromatic orb that attacks every 2 seconds (fixed rhythm, never changes, cannot be sped up or slowed down). However, each attack in the cycle deals escalating damage: 1x, 1.2x, 1.4x, 1.6x, then resets to 1x. This 4-beat cycle repeats indefinitely. The fixed rhythm is the "isorhythm" -- the unchanging temporal structure. The changing damage is the "melody" -- the variable element. Multiple Motet Sentinels synchronize their rhythms but offset their melody cycles, creating staggered damage peaks.

**Visual Design:** The staff is a chrome conductor's baton (thinner and shorter than the Concertmaster's) with a chromatic orb at its tip. When used, the orb detaches and becomes the Motet Sentinel. The Sentinel is a floating sphere of layered chromatic chrome (12 color facets, like the Chromatic Core material), orbiting with 4 visible rhythm-marker dots that circle it at a constant speed (one full rotation = one 4-beat cycle). Each rhythm-marker dot is a different color (indigo/teal/rose/amber) and glows when its beat arrives.

**VFX Breakdown:**
- **Sentinel idle:** Orb floats near the player, rotating slowly. 4 rhythm-marker dots orbit it at a constant speed. Ambient Chromatic Motes.
- **Attack (beat):** One rhythm-marker dot flashes and the Sentinel fires a bolt at the nearest enemy. Bolt color matches the current marker's color. The bolt has a thin trail (width 5f) and impacts with a small Cadence Burst.
- **Damage escalation visual:** At 1x, the bolt is small (scale 0.4f). At 1.2x, slightly larger (0.5f). At 1.4x, medium (0.6f). At 1.6x, large (0.8f) with a 3-layer bloom. Then resets.
- **Cycle reset:** Brief pulse from the Sentinel as the cycle restarts (all 4 markers flash simultaneously).

---

### 6.6 Post-Moon Lord Weapons (6)

#### Weapon 18: Ground Bass Anchor

> *"The ground bass -- the deepest voice, the foundation of the entire harmonic structure. This anchor is driven into the earth, and the earth itself becomes a weapon."*

| Property | Value |
|---|---|
| Damage | 180 Polyphonic |
| Use Time | 38 (Very Slow) |
| Knockback | 9 (Very Strong) |
| Recipe | 8 Opus Null + 5 Enharmonic Alloy + 3 Fragment of the Unfinished + 30 Luminite Bar @ Ancient Manipulator |
| Rarity | PolyphonicRarity (Tier 5) |

**Secondary Mechanic -- Ground Bass Wave:** Each downward slam creates a persistent "ground bass" -- a visible frequency wave that travels along the ground in both directions for 10 tiles. The wave deals continuous damage (60 DPS) to all grounded enemies and lasts 4 seconds. Subsequent slams within the wave's lifetime extend its range by 3 tiles per slam. Max extension: 25 tiles per direction. The wave applies 1 Harmonic Sear stack per second to enemies standing on it. This weapon is devastating against grounded enemies and rewards positional play.

**Visual Design:** A massive warhammer/anchor hybrid. The head is a dense, downward-pointing anchor shape made of layered chrome-indigo and chrome-amber metal, with visible resonant tuning-fork prongs at the bottom (they vibrate on impact). The shaft is thick, wrapped in chromatic chains. The butt end has a counterbalance of graphite-void chrome. Musical bass clef engravings cover the anchor head. When held, the weapon drags slightly, conveying immense weight.

**VFX Breakdown:**
- **Slam:** Massive amber-core impact flash + ground crack visual (actual cracked-earth texture overlay, temporary). Cadence Burst from impact point, large scale.
- **Ground Bass Wave:** Visible as pulsing low-frequency wave lines traveling along ground tiles -- alternating indigo and amber horizontal bars that scroll outward. Enemies standing on the wave have visible vibration lines around their feet. The wave has a faint glow visible from a distance.
- **Extended wave:** Each additional slam adds a visible "pulse" that expands the wave further. The wave's color intensifies with each extension.
- **Idle:** The anchor vibrates subtly (slight position jitter, 1 pixel). A faint bass hum visual (very small concentric circles) emanates from the tuning-fork tips.

---

#### Weapon 19: Stretto Convergence

> *"Stretto -- the point in a fugue where the voices enter closer and closer together, overlapping, compressing, until they collide. These twin blades embody that compression."*

| Property | Value |
|---|---|
| Damage | 140 Polyphonic (per blade), 420 Polyphonic (convergence strike) |
| Use Time | 14 (Very Fast), accelerating to 6 over 8 swings |
| Knockback | 5 (Average) |
| Autoswing | Yes |
| Recipe | 6 Opus Null + 8 Enharmonic Alloy + 4 Fragment of the Unfinished @ Ancient Manipulator |
| Rarity | PolyphonicRarity (Tier 5) |

**Secondary Mechanic -- Stretto Acceleration:** Twin blades that start at use time 14 and accelerate by 1 per swing over 8 swings (14, 13, 12, 11, 10, 9, 8, 7). On the 8th swing, both blades overlap and the player performs a single combined "Convergence Strike" dealing 3x damage in a massive arc. Then the tempo resets to 14. The acceleration is visible -- the blade trails get progressively denser as the attacks speed up, and the sound pitch rises with each swing. Missing (no enemy hit for 2 seconds) resets the counter.

**Visual Design:** Twin katana-length blades, one in Chromatic Indigo chrome and one in Prismatic Rose chrome. Their designs are mirror images -- the indigo blade curves slightly left, the rose blade curves slightly right. When they overlap at swing 8, the combined shape forms a single wide blade of white chrome. Guards are interlocking half-circle shapes. Grips are graphite-void with teal wire wrapping. Staff lines etched along each blade run in opposite directions (ascending on indigo, descending on rose).

**VFX Breakdown:**
- **Swings 1-3 (slow):** Alternating Indigo and Rose SemiCircularSmears. Wide, clean trails (width 18f). Brief afterimage.
- **Swings 4-6 (medium):** Trails get thinner and more numerous (overlapping previous trails still visible). Colors start to bleed into each other. Width 14f.
- **Swings 7 (fast):** Both blades nearly simultaneous. Trails are dense, almost continuous. Indigo and rose blending into magenta. Width 10f.
- **Swing 8 (Convergence Strike):** Both blades unite. Single massive white-chrome SemiCircularSmear, width 30f, piercing, full-palette bloom. Screen-flash. Radial Cadence Burst. All accumulated afterimage trails flash and dissolve simultaneously.

---

#### Weapon 20: Harmonic Series Bow

> *"The harmonic series: the fundamental tone and its overtones, each one doubled in frequency. This bow fires them in sequence -- one arrow, then two, then three -- each softer, each seeking its target with greater precision."*

| Property | Value |
|---|---|
| Damage | 130 Polyphonic (fundamental), 65/32/16/8 for overtones 2-5 |
| Use Time | 20 (Fast) at no charge, 40 (Very Slow) at full charge |
| Knockback | 6 (Strong) |
| Charge Time | 0.5s per overtone level (2.5s for full 5-arrow volley) |
| Recipe | 5 Opus Null + 10 Enharmonic Alloy + 2 Fragment of the Unfinished + 15 Vortex Fragment @ Ancient Manipulator |
| Ammo | Any Arrow |
| Rarity | PolyphonicRarity (Tier 5) |

**Secondary Mechanic -- Harmonic Overtones:** Hold to charge. Each 0.5 seconds of charge adds one overtone arrow:

| Charge Level | Arrows | Damage per Arrow | Homing |
|---|---|---|---|
| 0 (tap fire) | 1 (fundamental) | 130 | None |
| 1 (0.5s) | 2 | 130 + 65 | Overtone 2 has mild homing |
| 2 (1.0s) | 3 | 130 + 65 + 32 | Overtone 3 has moderate homing |
| 3 (1.5s) | 4 | 130 + 65 + 32 + 16 | Overtone 4 has strong homing |
| 4 (2.0s) | 5 | 130 + 65 + 32 + 16 + 8 | Overtone 5 has aggressive homing |

Full volley total: 251 damage spread across 5 arrows. The overtone arrows fan outward in increasing angles. They're weaker individually but nearly impossible to dodge at high charge levels.

**Visual Design:** A grand bow (2x player height) made of 5 nested chrome arcs, each a different size and color (indigo innermost, teal, rose, amber, white chrome outermost). When charging, the arcs pull apart in sequence, each "ringing" with its overtone color. The string is a thick beam of white chrome light that intensifies with charge. Full charge: all 5 arcs are separated and vibrating, the string is blindingly bright, and chromatic motes swirl around the player.

**VFX Breakdown:**
- **Charge visual:** Arcs separate one by one with each charge level. Chromatic Motes intensify. A visible charge counter (5 concentric circles, filling from inside out) appears near the player.
- **Fundamental arrow:** Large indigo-chrome arrow with a wide trail (width 12f). Straight flight, no homing.
- **Overtone arrows:** Each successive overtone arrow is smaller, a different color, and fans outward from the fundamental's path. Each has increasingly aggressive homing trails (visible as curving flight paths). The 5th overtone (white chrome) leaves a spiral contrail as it seeks its target.
- **Full volley:** All 5 arrows fire simultaneously in a fan pattern. The combined trail creates a "harmonic series" visual -- 5 colored lines spreading from a single point like an acoustic spectrum diagram.

---

#### Weapon 21: Chromatic Fantasy

> *"A chromatic fantasy is an improvisatory, free-flowing composition through every key. This weapon channels that freedom -- a beam that paints the enemy with every color, every debuff, every frequency, until nothing can withstand the chromatic saturation."*

| Property | Value |
|---|---|
| Damage | 160 Polyphonic (continuous beam) |
| Use Time | 24 (continuous channel) |
| Knockback | 4 (Weak) |
| Mana Cost | 20/second (continuous drain) |
| Recipe | 4 Opus Null + 6 Enharmonic Alloy + 5 Fragment of the Unfinished + 15 Nebula Fragment @ Ancient Manipulator |
| Rarity | PolyphonicRarity (Tier 5) |

**Secondary Mechanic -- Chromatic Saturation:** The beam cycles through all 12 extended palette colors over a 10-second period. Each color applies a different minor debuff to enemies it hits:

| Color | Debuff |
|---|---|
| Deep Void | -5% defense |
| Midnight Indigo | -3% movement speed |
| Chromatic Indigo | -5% damage |
| Electric Cyan | Confused for 0.5s |
| Iridescent Teal | -2% HP regen |
| Harmonic Emerald | Poisoned (3 DPS, 2s) |
| Prismatic Rose | On-fire equivalent (5 DPS, 2s) |
| Chromatic Magenta | Slowed for 0.5s |
| Harmonic Amber | Ichor equivalent (-15 defense, 1s) |
| Molten Gold | Cursed Inferno equivalent (6 DPS, 1s) |
| Iridescent Silver | Shadow Flame equivalent (8 DPS, 1s) |
| White Chrome Flash | All above debuffs refreshed |

If all 12 colors hit the same enemy within one 10-second cycle, **Chromatic Saturation** triggers: +25% damage from all Polyphonic sources for 5 seconds + visual overload (enemy becomes a blazing chromatic silhouette).

**Visual Design:** A floating crystal prism (held in one open hand, suspended above the palm). The prism is a triangular crystal made of layered Polyphonic chrome colors. When channeling, a white beam enters one face and exits the other face as a color-cycling beam (prism dispersion effect). The prism rotates slowly, and the output beam's color shifts with the rotation. Staff-line engravings on the prism's faces. When Chromatic Saturation triggers, the prism shatters momentarily (visual only) and reforms, briefly emitting all 12 colors simultaneously.

**VFX Breakdown:**
- **Beam:** Continuous column beam (width 8f) that smoothly transitions through 12 colors over 10 seconds. 3-layer bloom with the current color dominant. Length extends to 40 tiles or first contact point.
- **Per-color hit:** Small burst of the current color at the hit point. A small icon of the current debuff appears briefly above the enemy.
- **Color counting:** A small 12-segment wheel near the player tracks which colors have been applied to the current target. Segments light up in sequence.
- **Chromatic Saturation trigger:** All 12 wheel segments flash simultaneously. The enemy's body becomes a blazing white-chrome silhouette edged in cycling colors. Massive Cadence Burst. The beam temporarily becomes pure white chrome (width 12f) for the 5-second duration.

---

#### Weapon 22: The Unfinished Fugue

> *"Dropped by The Incomplete Maestro -- an unfinished composition given form as a blade. Its attack pattern IS a fugue: subject, answer, stretto, coda. But the coda comes too early, as if the piece was never meant to end this way..."*

| Property | Value |
|---|---|
| Damage | 200 Polyphonic |
| Use Time | 28 (Average) |
| Knockback | 8 (Very Strong) |
| Source | The Incomplete Maestro drop (10%) |
| Rarity | PolyphonicRarity (Tier 5 - Ultimate) |

**Secondary Mechanic -- Fugue Sequence:** The weapon has a 4-swing cycle that mirrors a fugue's structure:

| Swing | Fugal Element | Action | Damage |
|---|---|---|---|
| 1st | Subject | Normal swing + fires a projectile forward (indigo) | 200 + 100 (projectile) |
| 2nd | Answer | Normal swing + fires a projectile backward (teal) | 200 + 100 (projectile) |
| 3rd | Stretto | Normal swing + fires projectiles BOTH forward and backward simultaneously | 200 + 200 (both projectiles) |
| 4th | Coda | Massive overhead slam + 8-directional projectile burst (all Polyphonic colors) | 400 + 50 x 8 (burst) |

After the Coda, the cycle resets. Missing the chain (not swinging for 3 seconds) resets to Subject. The projectiles pierce 2 enemies each. The Coda burst is devastating but has a 0.5-second windup (telegraphed).

**Visual Design:** A fractured greatsword -- it looks like a completed blade that was broken and partially reassembled. The blade is wide chrome with visible fracture lines running through it -- the fractures glow in cycling Polyphonic colors as if the blade is trying to hold itself together with harmonic energy. The tip of the blade is absent -- it simply ends in a ragged, glowing edge (unfinished). The guard is a segmented circle with missing pieces (incomplete). The grip is wrapped in what appears to be torn sheet music (chromatic metal with visible notation). On the Subject swing, the fractures glow indigo. On Answer, teal. On Stretto, both. On Coda, all colors simultaneously, and the missing tip temporarily manifests as a blade of pure white chrome energy.

**VFX Breakdown:**
- **Subject (swing 1):** Indigo SemiCircularSmear (3-pass trail, width 20f) + forward projectile (indigo beam with score-line trails).
- **Answer (swing 2):** Teal SemiCircularSmear (same specs) + backward projectile (teal beam, inverted).
- **Stretto (swing 3):** Simultaneous indigo+teal SemiCircularSmear (overlapping, creating a magenta-white interference pattern) + dual projectiles.
- **Coda (swing 4):** Extended windup (0.5s, blade raised, all fractures blazing) + massive white-chrome SemiCircularSmear (width 30f) + 8 chromatic projectile beams radiating outward (one per direction, each a different extended palette color). Brief screen-flash. Screen-shake. The temporary blade tip (white chrome energy) extends the reach of this swing by 50%.

---

#### Weapon 23: Omni Voce -- "All Voices"

> *"The ultimate Polyphonic weapon. It does not choose one voice -- it contains them all. Sword, bow, staff, summon -- cycling endlessly, building toward the moment when all voices sound at once: Tutti."*

| Property | Value |
|---|---|
| Damage | 250 Polyphonic (base for all forms) |
| Use Time | Varies by form (Sword: 20, Bow: 16, Staff: 22, Summon: 30) |
| Knockback | 6 (Strong) |
| Source | The Incomplete Maestro drop (5%) |
| Rarity | PolyphonicRarity (Tier 5 - Ultimate+) |

**Secondary Mechanic -- Polyphonic Cycling + Tutti:** The weapon automatically cycles between 4 forms every 5 seconds:

| Cycle Position | Form | Attack Style | Color Accent |
|---|---|---|---|
| 0-5s | **Sword (Voce Prima)** | Wide horizontal slashes with echo projections | Chromatic Indigo |
| 5-10s | **Bow (Voce Seconda)** | Rapid-fire harmonic arrows that fan outward | Iridescent Teal |
| 10-15s | **Staff (Voce Terza)** | Seeking orbs that spiral and home aggressively | Prismatic Rose |
| 15-20s | **Summon (Voce Quarta)** | Temporary autonomous chromatic sentinel (attacks independently) | Harmonic Amber |

On each form change, a chromatic pulse expands from the player (15-tile radius) applying 2 Harmonic Sear stacks to all enemies hit.

After all 4 forms have cycled once (20 seconds of continuous use), the weapon enters **Tutti Mode** for 10 seconds. In Tutti, ALL 4 forms attack simultaneously:
- Sword swings produce projectiles
- Bow arrows fire automatically at the nearest enemy
- Staff orbs spiral around the player
- The Summon sentinel orbits and attacks independently

Tutti damage: 4 x 250 = 1000 effective DPS potential. After Tutti, the weapon resets and begins the cycle again.

**Visual Design:** A weapon that physically transforms. In its resting state, it's a small chrome sphere (like a miniature Chromatic Core) floating in the player's hand. When drawn, it unfolds into the current form:

- **Voce Prima (Sword):** Sleek longsword with an indigo blade and chrome guard. Staff lines etched along the flat.
- **Voce Seconda (Bow):** Compact compound bow with teal chrome limbs and a light-string. Tuning-fork limb tips.
- **Voce Terza (Staff):** Tall staff with a rose crystal orb headpiece and chrome shaft. Double-helix twist.
- **Voce Quarta (Summon):** Conductor's baton with an amber crystal tip. Short, elegant, authoritative.
- **Tutti Mode:** The sphere expands into ALL four forms simultaneously -- a floating assemblage of sword/bow/staff/baton orbiting the player, each one autonomous. The player's hands glow with white chrome energy.

**VFX Breakdown:**
- **Form change:** The current weapon dissolves into chromatic particles, the sphere reforms briefly (0.3s), then unfolds into the new form. A chromatic pulse ring expands outward. The 2 Harmonic Sear stacks are applied with visible indigo flame bursts on enemies.
- **Each form's attacks:** Use the VFX principles of their weapon type (SemiCircularSmear for sword, ribbon trails for arrows, spiral trails for orbs, sentinel glow for summon) all rendered in the Polyphonic palette.
- **Tutti activation:** Screen-wide chromatic flash. All 4 weapon forms materialize around the player and begin operating independently. The player's body gains a blazing full-palette aura. Trailing Voice Notes, Cadence Bursts, and Chromatic Motes from all 4 weapons create a visual spectacle -- controlled chaos of chromatic iridescent energy.
- **Tutti end:** All 4 forms converge back into the sphere. A final chromatic pulse. Brief fade to normal.

---

## 7. Midjourney Prompts -- Weapons

All weapon prompts follow a consistent ornate format emphasizing the Polyphonic Line's chromatic iridescent metallic aesthetic. Each prompt is designed to produce a magnificent side-view pixel art sprite with maximum visual detail, flowing energy effects, and musical notation motifs rendered in the sleek chrome-and-crystal design language unique to this theme.

### 7.1 Pre-Hardmode Weapons

**1. Overtone Edge**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent angular longsword rotated 45 degrees with blade pointing top-right, pristine polished chrome-indigo metal with oil-slick iridescence forming clean elegant Art Deco blade outline shifting between deep indigo and shimmering teal across the entire surface, blade interior filled with flowing amorphous overtone frequencies and crystallized sound waves swirling like harmonic resonance given form, wavy iridescent energy flows through visible tuning-fork guard prongs in teal chrome down entire weapon creating visible harmonic currents, chrome-indigo metal surface decorated with flowing musical staff-line dynamics and notation engravings running lengthwise, orbiting chromatic mote fragments and crystallized sound sparks constantly drifting in graceful spiral around the blade edge, cycling indigo-teal-rose-amber light drifts majestically from small chromatic crystal pommel while dark graphite handle pulses with inner amber staff-line luminescence, multiple voice note formations float naturally creating organic overtone patterns, iridescent teal echo energy charges through pristine chrome-indigo framework, ghost swing afterimages and crystallized harmonic fragments cascade from blade tip, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic iridescent resonance radiating, epic powerful sprite art, full longsword composition, --ar 16:9 --v 7.0
```

**2. Resonant Shortbow**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent compact recurve bow rotated 45 degrees with limb tips pointing top-right, pristine layered chrome-teal metal strips braided and interwoven forming clean elegant recurve outline with harmonic vibration finish, bow interior filled with flowing amorphous resonant frequencies and teal light string energy swirling like crystallized sound made visible, wavy iridescent energy flows through visible tuning-fork limb tips vibrating with harmonic overtones down entire bow creating visible resonance currents, chrome-teal metal surface decorated with flowing musical staff-line fletching dynamics and harmonic notation running along each braided strip, orbiting resonant arrow fragments and chromatic mote sparks constantly splitting in graceful spiral around the bowstring, glowing teal light string pulses majestically while dark graphite-void grip radiates with inner indigo luminescence, multiple harmonic split formations diverge naturally creating organic arrow-echo patterns, iridescent indigo-teal energy charges through pristine chrome framework, split arrow afterimages and crystallized frequency fragments cascade from tuning-fork tips, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic resonant vibration radiating, epic powerful sprite art, full recurve bow composition, --ar 16:9 --v 7.0
```

**3. Dissonance Staff**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent full-length two-handed staff rotated 45 degrees with head pointing top-right, pristine spiraling chrome-indigo and chrome-rose metal strands twisted in double helix forming clean elegant staff outline with dissonant harmonic finish, staff head filled with flowing amorphous detuning frequencies inside an open whole-note circle of white chrome with small chromatic crystal suspended weightlessly in the center, wavy iridescent energy flows through visible double-helix spiral strands down entire staff shaft creating visible dissonance currents, chrome-indigo and chrome-rose metal surface decorated with flowing musical staff lines wrapping like engraved vines with bouncing notation dynamics, orbiting chromatic bolt fragments and detuning spark particles constantly ricocheting in graceful spiral around the whole-note head, cycling indigo-teal-rose light drifts majestically from central crystal while sharp graphite-void base point pulses with inner harmonic luminescence, multiple bounce-path formations arc naturally creating organic dissonance patterns, iridescent rose energy charges through pristine chrome-indigo double-helix framework, detuning bolt afterimages and chromatic mote fragments cascade from crystal center, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic dissonant energy radiating, epic powerful sprite art, full two-handed staff composition, --ar 16:9 --v 7.0
```

**4. Chromatic Whip**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent segmented whip rotated 45 degrees with tip cracking toward top-right, pristine segmented chrome-amber plates linked by glowing teal energy threads forming clean elegant whip outline with crystallized sound finish, whip body filled with flowing amorphous chromatic frequencies and amber-teal energy swirling through each progressively smaller segment like harmonic cascade, wavy iridescent energy flows through visible teal thread connections between chrome-amber segments down entire whip creating visible chromatic currents, chrome-amber plate surface decorated with flowing musical sharp-sign dynamics and staff-line notation running along each linked segment, orbiting chromatic sigil fragments and crystallized ground-ward sparks constantly cycling in graceful spiral around the whip tip, cycling indigo-teal-rose-amber light blazes majestically from tiny chromatic crystal tip while dark graphite grip with angular sharp-sign guard pulses with inner harmonic luminescence, multiple sigil formations radiate naturally creating organic chromatic empowerment patterns, iridescent teal thread energy charges through pristine chrome-amber segmented framework, chromatic sigil afterimages and Voice Note fragments cascade from cracking tip, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic whip-crack energy radiating, epic powerful sprite art, full segmented whip composition, --ar 16:9 --v 7.0
```

### 7.2 Hardmode Weapons

**5. Contrary Motion**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent twin short swords connected at pommel by glowing chrome chain rotated 45 degrees crossing in X pattern pointing top-right, pristine chromatic indigo chrome right blade with ascending musical staff-line engravings and iridescent teal chrome left blade with descending staff-line engravings forming clean elegant contrary motion outline with counterpoint finish, blade interiors filled with flowing amorphous opposing harmonic frequencies and indigo-teal energy swirling in contrary directions like musical voices moving against each other, wavy iridescent energy flows through visible glowing chain connection pulsing with alternating indigo-teal light down entire dual weapon creating visible counterpoint currents, chrome-indigo and chrome-teal metal surfaces decorated with flowing ascending and descending musical notation dynamics and contrary motion score markings, orbiting shockwave fragments and contrary-motion spark particles constantly colliding in graceful spiral around the crossing point, half-circle white chrome guards pulse majestically as if yearning to complete their circle while chain crackles with inner harmonic luminescence, multiple contrary shockwave formations ripple naturally creating organic opposing-force patterns, iridescent indigo and teal energy charges simultaneously through pristine chrome dual framework, contrary motion shockwave afterimages and crystallized counterpoint fragments cascade from both blade tips, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic contrary force radiating, epic powerful sprite art, full twin sword composition, --ar 16:9 --v 7.0
```

**6. Augmented Longbow**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent tall longbow one-and-a-half times standard height rotated 45 degrees with limb tips pointing top-right, pristine stacked chrome plates fanned outward like organ pipes each section a different iridescent color from the 12-color chromatic palette forming clean elegant augmented bow outline with semitone ascension finish, bow body filled with flowing amorphous ascending chromatic frequencies and 12-tone energy swirling through each organ-pipe plate section like pitch rising through the chromatic scale, wavy iridescent energy flows through visible dual-thread string of indigo and teal light connecting tuning-fork arrow tips down entire bow creating visible semitone currents, chrome organ-pipe plate surfaces decorated with flowing ascending chromatic scale dynamics and twelve-tone notation markings running along each stacked plate, orbiting augmented arrow fragments and semitone-ascending spark particles constantly climbing in graceful spiral around the bowstring, hexagonal lattice riser grip section glows majestically with solid chrome precision while each pipe section radiates with its own spectral luminescence, multiple semitone-pierced formations ascend naturally creating organic chromatic scale patterns, iridescent 12-color energy charges through pristine chrome organ-pipe framework, augmented arrow afterimages growing brighter with each pierce cascade from limb tips, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic ascending resonance radiating, epic powerful sprite art, full tall longbow composition, --ar 16:9 --v 7.0
```

**7. Invertible Counterpoint**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent open floating tome rotated 45 degrees with spine pointing top-right and twin beams emitting in opposite directions, pristine polished chrome covers with treble clef engraved on front and inverted treble clef on back forming clean elegant invertible tome outline with counterpoint harmonic finish, tome interior filled with flowing amorphous dual-beam frequencies and iridescent chromatic metal pages fluttering with internal indigo-teal-rose light swirling like inverted musical voices, wavy iridescent energy flows through visible twin beams of indigo and teal firing from the spine in exact opposite directions down entire composition creating visible inversion currents, chrome cover surface decorated with flowing invertible counterpoint dynamics and mirrored treble clef notation running across both covers, orbiting 4-dot cast counter fragments and beam-inversion spark particles constantly swapping in graceful spiral around the open pages, fluttering chromatic metal pages blaze majestically between the twin beams while small cast counter dots pulse with cycling indigo-teal-rose-amber luminescence, multiple inversion-swapped beam formations alternate naturally creating organic counterpoint patterns, iridescent indigo beam and teal beam energy charges simultaneously through pristine chrome tome framework, inverted beam afterimages and staff-line imprint fragments cascade from both beam termination points, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic invertible energy radiating, epic powerful sprite art, full floating tome composition, --ar 16:9 --v 7.0
```

**8. Suspension Chains**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent flail with fermata-shaped head rotated 45 degrees with chain arc sweeping toward top-right, pristine polished rose-chrome metal head shaped like an arc-over-dot fermata musical symbol with amber inlays forming clean elegant suspension flail outline with harmonic stasis finish, fermata head interior filled with flowing amorphous suspended harmonic frequencies and rose-amber energy swirling like a held note frozen in time, wavy iridescent energy flows through visible chain of interlocking chromatic links each a different palette color cycling indigo-teal-rose-amber-white down entire flail creating visible suspension currents, rose-chrome and amber metal surface decorated with flowing fermata dynamics and held-note notation markings running along each chain link, orbiting harmonic stasis fragments and frozen-vibration spark particles constantly crystallizing in graceful spiral around the fermata head, chromatic chain links blur into continuous iridescent spiral while dark graphite grip with staff-line engravings pulses with inner harmonic luminescence, multiple suspension-ring formations freeze naturally creating organic stasis patterns, iridescent rose-amber energy charges through pristine chromatic chain framework, harmonic stasis afterimages and chromatic chain-shatter fragments cascade from fermata head, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic suspended resonance radiating, epic powerful sprite art, full flail composition, --ar 16:9 --v 7.0
```

**9. Pedal Tone Repeater**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent repeater crossbow with compressed piano keyboard body rotated 45 degrees with barrel pointing top-right, pristine layered chrome-teal plates with alternating indigo and white chrome piano key shapes along its length forming clean elegant pedal tone crossbow outline with bass-frequency finish, crossbow body filled with flowing amorphous pedal tone frequencies and massive bass-energy swirling through the piano-keyboard frame like the fundamental note building to devastating release, wavy iridescent energy flows through visible long teal-chrome barrel rail and 5 tiny chromatic dot indicators atop the weapon in indigo-teal-rose-amber-white down entire repeater creating visible bass frequency currents, chrome-teal and piano-key surfaces decorated with flowing pedal point dynamics and bass notation markings running along the keyboard body, orbiting pedal tone blast fragments and crystallized bass-frequency sparks constantly building in graceful spiral around the barrel tip, five chromatic dot indicators merge into single pulsing amber orb when charged while dark graphite stock with amber accent lines radiates with inner bass luminescence, multiple pedal tone shockwave formations pulse naturally creating organic ground-shaking patterns, iridescent amber-core energy charges through pristine chrome-teal piano-keyboard framework, massive pedal tone blast afterimages and sound-wave ring fragments cascade from barrel, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic bass devastation radiating, epic powerful sprite art, full repeater crossbow composition, --ar 16:9 --v 7.0
```

### 7.3 Post-Plantera Weapons

**10. Canon of Chromatic Fire**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent crystalline pipe organ launcher rotated 45 degrees with pipe mouths pointing top-right, pristine chrome framework housing 5 chrome pipes of decreasing size fanned outward each a different chromatic color indigo-teal-rose-amber-white chrome forming clean elegant canon launcher outline with musical canon finish, launcher body filled with flowing amorphous canon-round frequencies and clone-orb energy swirling through each pipe like musical voices chasing each other in perfect delayed imitation, wavy iridescent energy flows through visible hexagonal-lattice chrome panels on graphite-void stock and chromatic crystal charging chamber at the base of the pipes down entire launcher creating visible canon-replication currents, chrome pipe surfaces decorated with flowing canon repetition dynamics and round notation markings running along each graduated pipe, orbiting clone-orb fragments and canon-echo spark particles constantly replicating in graceful spiral around the pipe mouths, chromatic crystal charging chamber blazes majestically with cycling colors while each pipe mouth radiates with its own delayed luminescence, multiple simultaneous-detonation formations burst naturally creating organic five-voice chromatic flower patterns, iridescent five-color energy charges through pristine chrome pipe-organ framework, canon-clone afterimages following recorded paths and chromatic fire fragments cascade from all five pipe mouths, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic canon-fire devastation radiating, epic powerful sprite art, full pipe organ launcher composition, --ar 16:9 --v 7.0
```

**11. Obligato Blade**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent massive two-handed greatsword twice player height rotated 45 degrees with blade edge pointing top-right, pristine wide flat blade made of layered chrome in all chromatic accent colors as visible horizontal geological cross-section bands from indigo at base through teal through rose through amber to blazing white chrome at cutting edge forming clean elegant obligato outline with full chromatic edge finish, blade interior filled with flowing amorphous harmonic sear frequencies and scrolling musical notation energy swirling through the layered color bands like accumulated voices building toward critical mass, wavy iridescent energy flows through visible staff lines engraved lengthwise along the flat of the blade with intensifying notation down entire greatsword creating visible obligato currents, layered chrome color band surfaces decorated with flowing obligato dynamics and sear-stack counting notation running through each geological stratum, orbiting harmonic sear fragments and full chromatic edge spark particles constantly accumulating in graceful spiral around the blade edge, wide horizontal amber chrome guard blazes majestically while long two-handed grip wrapped in dark graphite with teal inlay spirals pulses with inner stack-monitoring luminescence, multiple screen-wide slash formations charge naturally creating organic full chromatic patterns, iridescent five-stack energy charges through pristine layered chrome framework, full chromatic edge afterimages spanning the horizon and chromatic sear fragments cascade from white chrome cutting edge, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic obligato devastation radiating, epic powerful sprite art, full massive greatsword composition, --ar 16:9 --v 7.0
```

**12. Ricercar**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent tall seeking staff topped with magnifying-glass head rotated 45 degrees with lens pointing top-right, pristine twisted chrome-teal metal shaft with rose accent rings at even intervals and magnifying-glass-shaped chrome-and-crystal head containing a floating spinning chromatic crystal forming clean elegant ricercar outline with seeking spiral finish, staff head filled with flowing amorphous searching frequencies and sonar-pulse energy swirling inside the magnifying lens like a harmonic predator spiraling outward seeking its prey, wavy iridescent energy flows through visible rose accent rings and twisted chrome-teal shaft down entire staff creating visible ricercar search currents, chrome-teal and rose metal surface decorated with flowing spiral search dynamics and seeking notation markings running along the twisted shaft, orbiting sonar ring fragments and seeking-spiral spark particles constantly expanding in graceful spiral around the magnifying lens head, floating spinning chromatic crystal blazes majestically inside the lens while sharp graphite-void base point pulses with inner target-lock luminescence, multiple expanding spiral-search formations arc naturally creating organic ricercar patterns, iridescent rose-teal energy charges through pristine chrome twisted framework, seeking spiral afterimages and aggressive homing fragments cascade from the magnifying lens, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic seeking resonance radiating, epic powerful sprite art, full tall seeking staff composition, --ar 16:9 --v 7.0
```

**13. Pasacaglia's Descent**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent heavy downward-thrust polearm rotated 45 degrees with wide blade pointing bottom-right in descending strike position, pristine wide downward-pointing chrome arrowhead blade in amber chrome with indigo edge accents forming clean elegant pasacaglia spear outline with ostinato repetition finish, blade interior filled with flowing amorphous repeated bass frequencies and ground-quake energy swirling through the wide arrowhead like an ostinato pattern building relentless intensity with each repetition, wavy iridescent energy flows through visible evenly-spaced teal rings like measure bars on a musical staff down entire long straight shaft of dark chrome-graphite metal creating visible ostinato currents, amber chrome and indigo edge surfaces decorated with flowing pasacaglia dynamics and bass repetition notation markings running along each measure-bar ring, orbiting ostinato stack-ring fragments and ground-quake spark particles constantly building in graceful spiral around the wide arrowhead tip, small amber crystal counterweight blazes majestically at the butt end while teal measure-bar rings glow progressively brighter with inner escalating luminescence, multiple ground-pound shockwave formations pulse naturally creating organic pasacaglia bass patterns, iridescent amber-core energy charges through pristine chrome-graphite framework, descending thrust afterimages with stacking indicator rings and ground-quake fragments cascade from wide arrowhead blade, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic ostinato devastation radiating, epic powerful sprite art, full heavy polearm composition, --ar 16:9 --v 7.0
```

### 7.4 Post-Golem Weapons

**14. Toccata Gauntlets**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent pair of chrome gauntlets with piano keyboard motif rotated 45 degrees with knuckles pointing top-right, pristine sleek chrome framework with alternating white chrome and indigo chrome piano key finger plates and raised chrome ridge knuckle guards with amber crystal insets forming clean elegant toccata gauntlet outline with virtuoso performance finish, gauntlet body filled with flowing amorphous toccata frequencies and rapid-fire chromatic bolt energy swirling through the piano-key finger plates like a keyboard virtuoso's impossible speed given lethal form, wavy iridescent energy flows through visible piano key depressions lighting up in sequence and staff-line wrist guard engravings down entire gauntlet pair creating visible toccata currents, chrome piano-key and knuckle-guard surfaces decorated with flowing toccata dynamics and rapid-fire notation markings running along each finger plate, orbiting 12-segment combo counter fragments and toccata burst spark particles constantly cycling in graceful spiral around the knuckle ridges, alternating white chrome and indigo chrome piano keys blaze majestically with each sequential depression while amber crystal knuckle insets pulse with inner virtuoso luminescence, multiple six-directional burst formations radiate naturally creating organic toccata patterns, iridescent six-color bolt energy charges through pristine chrome piano-keyboard framework, rapid toccata burst afterimages and chromatic bolt fragments cascade from every glowing finger tip, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic virtuoso devastation radiating, epic powerful sprite art, full dual gauntlet composition, --ar 16:9 --v 7.0
```

**15. Cantus Firmus**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent classical broadsword with deliberately understated foundation design rotated 45 degrees with blade pointing top-right, pristine straight wide blade of polished amber-chrome metal with a single perfectly straight white chrome line running down the absolute center like one unwavering musical note forming clean elegant cantus firmus outline with foundational bedrock finish, blade interior filled with flowing amorphous foundation frequencies and steady unwavering amber energy maintaining perfect harmonic constancy like the fixed song upon which all other voices build, wavy iridescent energy flows through visible single white chrome center line that never wavers never bends never ornaments down entire blade creating visible cantus firmus currents, polished amber-chrome metal surface deliberately undecorated except for the center line with subtle foundation dynamics and whole-note notation radiating stability, orbiting foundation buff fragments and steady-pulse spark particles constantly grounding in graceful spiral around the blade flat, wide stable cross-guard in dark graphite chrome anchors majestically while amber-wrapped handle and single uncut raw chromatic crystal pommel pulse with inner foundational luminescence, multiple ally-buffing aura formations emanate naturally creating organic cantus firmus patterns, iridescent amber-white energy charges through pristine polished chrome framework, foundation buff afterimages and whole-note notation fragments cascade from the unwavering center line, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic foundational resonance radiating, epic powerful sprite art, full classical broadsword composition, --ar 16:9 --v 7.0
```

**16. Twelve-Tone Row**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent sleek pistol-rifle hybrid with visible 12-chamber cylindrical magazine rotated 45 degrees with barrel pointing top-right, pristine long thin iridescent chrome sniper barrel with cylindrical magazine housing 12 chambers each filled with a chromatic crystal of a different color spanning deep void through indigo through teal through rose through amber to blazing white forming clean elegant twelve-tone outline with serial composition finish, gun body filled with flowing amorphous twelve-tone frequencies and 12-semitone energy swirling through each crystal chamber like Schoenberg's tone row cycling through every pitch before any repeats, wavy iridescent energy flows through visible rotating magazine chambers each igniting in sequence and staff-line stock engravings down entire pistol-rifle creating visible serial currents, iridescent chrome barrel and magazine surfaces decorated with flowing twelve-tone row dynamics and semitone-sequence notation markings running along each chamber, orbiting twelve-tone round fragments and semitone-cycling spark particles constantly sequencing in graceful spiral around the rotating magazine, all 12 chromatic crystal chambers blaze majestically in sequence while gun body shifts color to match current chamber and dark graphite stock pulses with inner serial luminescence, multiple twelfth-shot transcendence formations devastate naturally creating organic chromatic completion patterns, iridescent white chrome flash energy charges through pristine twelve-chamber framework on the devastating final round, twelve-tone round afterimages in all 12 colors and white chrome flash fragments cascade from the sniper barrel, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic serial devastation radiating, epic powerful sprite art, full pistol-rifle hybrid composition, --ar 16:9 --v 7.0
```

**17. Isorhythmic Motet**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent thin conductor's baton with chromatic orb at tip rotated 45 degrees with orb pointing top-right, pristine thin chrome conductor's baton shaft with 12-faceted chromatic orb headpiece surrounded by 4 orbiting rhythm-marker dots in indigo-teal-rose-amber forming clean elegant isorhythmic outline with fixed-rhythm summoning finish, orb filled with flowing amorphous isorhythmic frequencies and escalating melodic energy swirling through the 12 visible color facets like layered chromatic chrome cycling in unchanging temporal patterns, wavy iridescent energy flows through visible 4 rhythm-marker dots orbiting at constant speed with each dot glowing on its designated beat down entire baton creating visible isorhythmic currents, chrome baton shaft surface decorated with flowing isorhythmic dynamics and repeating-rhythm notation markings running along the elegant conductor's form, orbiting motet sentinel fragments and escalating-damage spark particles constantly cycling 1x-1.2x-1.4x-1.6x in graceful spiral around the chromatic orb, 12-faceted chromatic chrome orb blazes majestically with inner heartbeat rhythm while 4 rhythm-marker dots pulse with synchronized indigo-teal-rose-amber luminescence, multiple fixed-rhythm attack formations fire naturally creating organic isorhythmic motet patterns, iridescent four-beat cycling energy charges through pristine chrome baton framework, motet sentinel afterimages and rhythm-marker fragments cascade from the 12-faceted orb, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic rhythmic resonance radiating, epic powerful sprite art, full conductor's baton composition, --ar 16:9 --v 7.0
```

### 7.5 Post-Moon Lord Weapons

**18. Ground Bass Anchor**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent massive warhammer anchor hybrid rotated 45 degrees with anchor head pointing bottom-right in devastating downward slam position, pristine dense downward-pointing anchor head made of layered chrome-indigo and chrome-amber metal with visible resonant tuning-fork prongs at the bottom that vibrate with seismic force forming clean elegant ground bass outline with earth-shattering frequency finish, anchor head interior filled with flowing amorphous deep bass frequencies and ground-wave energy swirling through the layered chrome like the deepest harmonic voice shaking foundations with unstoppable resonance, wavy iridescent energy flows through visible chromatic chains wrapping the thick shaft and tuning-fork prongs emanating concentric bass waves down entire anchor creating visible ground bass currents, chrome-indigo and chrome-amber metal surface decorated with flowing bass clef engravings and ground-frequency notation markings covering the entire anchor head, orbiting ground-wave fragments and seismic bass-frequency spark particles constantly radiating in graceful spiral around the tuning-fork prongs, tuning-fork prongs vibrate majestically with visible sound-wave emissions while dark graphite chrome counterbalance and chromatic chain shaft pulse with inner deep-frequency luminescence, multiple persistent ground-wave formations travel naturally along the earth creating organic bass-line patterns, iridescent amber-indigo bass energy charges through pristine layered chrome anchor framework, ground bass wave afterimages traveling along terrain and seismic crack fragments cascade from tuning-fork impact point, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic seismic devastation radiating, epic powerful sprite art, full massive anchor-hammer composition, --ar 16:9 --v 7.0
```

**19. Stretto Convergence**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent twin katana-length blades rotated 45 degrees crossing in accelerating convergence pattern pointing top-right, pristine chromatic indigo chrome left blade curving slightly left with ascending staff-line engravings and prismatic rose chrome right blade curving slightly right with descending staff-line engravings forming clean elegant stretto outline with accelerating convergence finish, blade interiors filled with flowing amorphous stretto frequencies and accelerating tempo energy swirling through both blades like fugal voices entering closer and closer together compressing toward inevitable collision, wavy iridescent energy flows through visible interlocking half-circle guards and progressive afterimage density increasing with each swing down entire dual weapon creating visible stretto compression currents, chrome-indigo and chrome-rose blade surfaces decorated with flowing stretto acceleration dynamics and progressively compressed notation markings running in opposite directions along each blade, orbiting convergence strike fragments and tempo-accelerating spark particles constantly compressing in graceful spiral around the overlapping intersection, interlocking half-circle guards pulse majestically yearning to unite into single white chrome blade while dark graphite grips with teal wire wrapping radiate with inner accelerando luminescence, multiple progressive afterimage formations densify naturally creating organic stretto accumulation patterns, iridescent indigo-rose blending into magenta-white interference energy charges through pristine chrome dual framework, convergence strike afterimages and accumulated afterimage trail fragments explode from the united white chrome blade form, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic stretto convergence radiating, epic powerful sprite art, full twin katana composition, --ar 16:9 --v 7.0
```

**20. Harmonic Series Bow**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent grand bow twice player height made of 5 nested chrome arcs rotated 45 degrees with arcs separating and pointing top-right, pristine 5 nested chrome arcs of different sizes with innermost indigo then teal then rose then amber with outermost white chrome each separating and vibrating independently forming clean elegant harmonic series outline with overtone cascade finish, bow body filled with flowing amorphous harmonic overtone frequencies and five-fold arrow energy swirling through each nested arc like the fundamental tone and its four overtones each progressively weaker but more aggressively seeking, wavy iridescent energy flows through visible thick white chrome light string connecting all 5 arcs and charging energy intensifying with each overtone level down entire grand bow creating visible harmonic series currents, chrome arc surfaces decorated with flowing harmonic series dynamics and overtone-ratio notation markings running along each nested arc, orbiting overtone arrow fragments and harmonic fan spark particles constantly diverging in graceful spiral around the separated vibrating arcs, all 5 chrome arcs blaze majestically as they separate revealing individual overtone colors while chromatic motes swirl around the player creating visible charge-level circles with inner harmonic luminescence, multiple five-arrow harmonic fan formations spread naturally creating organic acoustic spectrum diagram patterns, iridescent fundamental-through-fifth-overtone energy charges through pristine nested chrome arc framework, five-arrow volley afterimages fanning from single origin point and spiral contrail fragments cascade from each overtone arrow, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic harmonic resonance radiating, epic powerful sprite art, full grand bow composition, --ar 16:9 --v 7.0
```

**21. Chromatic Fantasy**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent floating crystal prism suspended above an open palm rotated 45 degrees with dispersed beam pointing top-right, pristine triangular crystal prism made of layered chromatic chrome colors with visible internal color separation and a white beam entering one face exiting as a rainbow-cycling dispersed beam from the opposite face forming clean elegant chromatic fantasy outline with prismatic dispersion finish, prism interior filled with flowing amorphous chromatic frequencies cycling through all 12 extended palette colors and each color applying its own unique debuff energy swirling through the triangular crystal like pure white light shattering into every frequency of destruction, wavy iridescent energy flows through visible prism rotation and 12-segment color wheel tracker near the player down entire channeled beam creating visible chromatic saturation currents, layered chromatic chrome prism surfaces decorated with flowing chromatic fantasy dynamics and staff-line engravings with all-12-debuffs notation markings running across each prism face, orbiting 12-color segment fragments and chromatic saturation spark particles constantly completing in graceful spiral around the dispersed beam, prism shatters and reforms majestically when chromatic saturation triggers while all 12 colors fire simultaneously and open palm glows with inner prismatic luminescence, multiple all-twelve debuff formations apply naturally creating organic chromatic saturation patterns, iridescent white beam entering and 12-color spectrum exiting energy charges through pristine layered chrome prism framework, chromatic saturation afterimages and blazing white-chrome silhouette fragments cascade from the saturated target, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic fantasy devastation radiating, epic powerful sprite art, full floating prism composition, --ar 16:9 --v 7.0
```

**22. The Unfinished Fugue**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent fractured greatsword that appears broken and partially reassembled rotated 45 degrees with ragged glowing edge where blade tip should be pointing top-right, pristine wide chrome blade with visible glowing fracture lines running through it in cycling indigo-teal-rose-amber colors as if harmonic energy alone holds the broken blade together and a missing blade tip that simply ends in a ragged haunting glowing edge forming clean elegant unfinished fugue outline with incomplete masterwork finish, blade interior filled with flowing amorphous fugue-subject frequencies and subject-answer-stretto-coda energy swirling through the fracture lines like a composition that yearns to reach its final cadence but was abandoned before completion, wavy iridescent energy flows through visible fracture-line veins cycling through voice colors and incomplete segmented circle guard with missing pieces down entire fractured blade creating visible unfinished fugue currents, cracked chrome blade surface decorated with flowing fugal subject-answer dynamics and incomplete notation markings running through each fracture line, orbiting 8-directional coda fragments and fugue-voice spark particles constantly cycling subject-answer-stretto in graceful spiral around the absent blade tip, fracture lines cycle majestically through indigo for subject and teal for answer and both for stretto and all colors for coda while grip wrapped in torn metallic sheet music with visible notation pulses with inner incomplete luminescence, multiple fugue-cycle formations fire naturally creating organic subject-answer-stretto-coda patterns, iridescent all-color coda energy charges through pristine fractured chrome framework and temporary white chrome blade tip manifests for the devastating coda swing, coda burst afterimages in 8 directions and unfinished notation fragments cascade from the ragged glowing edge, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic unfinished devastation radiating, epic powerful sprite art, full fractured greatsword composition, --ar 16:9 --v 7.0
```

**23. Omni Voce**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent small chrome sphere unfolding into 4 simultaneous weapon forms rotated 45 degrees with all forms orbiting a central white chrome energy point top-right, pristine central chrome sphere like a miniature crystalline core expanding to reveal indigo longsword and teal compound bow and rose crystal staff and amber conductor's baton all orbiting simultaneously forming clean elegant omni voce outline with tutti transformation finish, all four weapon forms filled with flowing amorphous polyphonic frequencies from every voice and cycling chromatic pulse energy swirling through each form like all voices of the orchestra sounding at once in magnificent tutti, wavy iridescent energy flows through visible form-transition particles and chromatic pulse rings expanding from the player on each 5-second transformation down entire multi-weapon assemblage creating visible omni voce currents, each weapon form surface decorated with its own flowing voice dynamics and cycling notation markings with indigo sword trails and teal arrow fans and rose spiral orbs and amber sentinel attacks, orbiting tutti-mode fragments and four-voice chromatic pulse spark particles constantly transforming in graceful spiral around the central chrome sphere, all four weapon forms blaze majestically in their individual colors during tutti mode while central sphere radiates with pure white chrome energy and player glows with full-palette aura luminescence, multiple simultaneous four-form attack formations devastate naturally creating organic tutti patterns, iridescent all-four-voice energy charges simultaneously through pristine chrome sphere-to-weapon framework, tutti mode afterimages from all four weapons simultaneously and chromatic pulse fragments cascade from the blazing central sphere, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic tutti devastation radiating, epic powerful sprite art, full transforming multi-weapon composition, --ar 16:9 --v 7.0
```

---

## 8. Midjourney Prompts -- Enemies, Materials & World Events

### 8.1 Enemy Prompts

**1. Dissonant Wraith**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent translucent spectral humanoid torso entity with no legs trailing off into indigo mist, pristine dark graphite void metallic body with glowing indigo veins pulsing rhythmically along the surface and featureless chrome oval head with single glowing staff-line crack forming clean elegant wraith outline with dissonant ghost finish, wraith body filled with flowing amorphous dissonant frequencies and spectral energy swirling through the translucent torso like a shattered musical note given sentient form, wavy iridescent energy flows through visible indigo vein network and long tapered claws trailing iridescent mist down entire spectral entity creating visible dissonance currents, dark graphite void metallic surface decorated with flowing spectral dynamics and fragmented notation markings pulsing along each vein, orbiting chromatic mote fragments and shattered note spark particles constantly drifting in graceful spiral around the featureless chrome head, entire body shimmers majestically between visible and transparent on a haunting cycle while staff-line crack glows with inner indigo luminescence, multiple phase-through formations dissolve naturally creating organic wraith-reformation patterns, iridescent indigo-teal mist energy charges through pristine graphite void framework, spectral dissolution afterimages and scattered music note fragments cascade from trailing mist below the waist, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic spectral unease radiating, epic powerful sprite art, full ghostly entity composition, --ar 16:9 --v 7.0
```

**2. Chromatic Slime**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent polished chrome mirror-reflective metallic slime entity, pristine standard slime shape rendered in polished chrome metal instead of translucent gel with mirror-reflective surface shifting through all 12 iridescent chromatic palette colors and bright white-chrome orb core visible inside forming clean elegant chromatic slime outline with 12-color cycling finish, slime body filled with flowing amorphous chromatic frequencies and metallic resonance energy swirling through the mirror-polish interior with small musical notation symbols like quarter notes and rests floating inside like inclusions in glass, wavy iridescent energy flows through visible 12-color surface cycling and two horizontal musical staff-line eyes down entire chrome slime creating visible chromatic hop currents, polished chrome metal surface decorated with flowing chromatic dynamics and embedded notation symbol markings cycling through each color with every position change, orbiting chromatic bolt fragments and 12-semitone shard spark particles constantly cycling in graceful spiral around the mirror-reflective body, white-chrome orb core blazes majestically inside the metallic shell while musical notation inclusions drift with inner chromatic luminescence, multiple 12-fragment death-shatter formations scatter naturally creating organic chromatic radial patterns, iridescent 12-color cycling energy charges through pristine polished chrome framework, expanding color-ring afterimages from each hop and chromatic bolt fragments cascade from the metallic surface, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic metallic beauty radiating, epic powerful sprite art, full chrome slime entity composition, --ar 16:9 --v 7.0
```

**3. First Voice Sentinel**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent tall slender geometric humanoid figure three times player height made of polished dark chrome, pristine angular Art Deco metalwork body with tall vertical rectangle head bearing a glowing indigo bass clef and long thin arms ending in sharp blade-like hands forming clean elegant sentinel outline with fugal subject finish, sentinel body filled with flowing amorphous subject-theme frequencies and rhythmic beat-1-and-3 energy swirling through visible five horizontal musical staff lines on the torso with notes appearing and disappearing like the fugue's main melody being performed, wavy iridescent energy flows through visible staff-line telegraph glow intensifying before each attack and legs tapering to hovering points down entire geometric figure creating visible first voice currents, polished dark chrome surface decorated with flowing subject theme dynamics and bass clef notation markings running along each angular Art Deco limb, orbiting indigo bolt fragments and V-pattern projectile spark particles constantly firing on beats 1 and 3 in graceful spiral around the rectangle head, staff lines on torso blaze majestically brighter telegraphing imminent attack while chromatic indigo and graphite void accents pulse with inner rhythmic luminescence, multiple V-pattern bolt formations fire naturally creating organic fugal subject patterns, iridescent chromatic indigo energy charges through pristine polished dark chrome framework, staff-line unraveling afterimages and chromatic mote dissolution fragments cascade from the geometric body on death, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic rhythmic authority radiating, epic powerful sprite art, full tall geometric sentinel composition, --ar 16:9 --v 7.0
```

**4. Second Voice Echo**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent geometric humanoid figure similar to the First Voice but shorter with rounded edges made of dark chrome, pristine rounded Art Deco metalwork body with horizontal oval head bearing a glowing teal treble clef and slight translucency showing faint afterimage through it forming clean elegant echo outline with fugal answer finish, echo body filled with flowing amorphous answer-theme frequencies and rhythmic beat-2-and-4 energy swirling through visible staff lines on torso with notes offset by one beat from the subject creating a visible musical delay, wavy iridescent energy flows through visible thin teal harmonic thread connecting to nearest First Voice Sentinel and mirrored trajectory attack patterns down entire rounded figure creating visible second voice currents, dark chrome surface with iridescent teal accent decorated with flowing answer theme dynamics and treble clef notation markings with offset timing along each rounded edge, orbiting teal bolt fragments and inverted-trajectory spark particles constantly mirroring on beats 2 and 4 in graceful spiral around the oval head, echo afterimage projectiles appear majestically 0.5 seconds after each real bolt while teal harmonic thread connection pulses with inner delayed luminescence, multiple mirrored bolt formations fire naturally creating organic fugal answer patterns, iridescent iridescent teal energy charges through pristine dark chrome rounded framework, teal fragment afterimages drifting toward nearest First Voice and echo-duplicate dissolution fragments cascade from the translucent body, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic delayed resonance radiating, epic powerful sprite art, full rounded geometric echo composition, --ar 16:9 --v 7.0
```

**5. Third Voice Harmonic**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent small sleek curved chrome figure one-and-a-half times player height with prismatic rose accent, pristine curved chrome body with no legs replaced by flowing ribbon-tail of rose energy and diamond-shaped head with sharp flat sign etched in pink light forming clean elegant harmonic outline with countersubject finish, harmonic body filled with flowing amorphous countersubject frequencies and offbeat gap-filling energy swirling through visible eighth-note and sixteenth-note dense notation covering the entire body like rapid passages between the subject and answer, wavy iridescent energy flows through visible staccato position-snap movements and rose ribbon-tail leaving persistent damage hazard down entire sleek figure creating visible third voice currents, curved chrome surface with prismatic rose accent decorated with flowing countersubject dynamics and rapid sixteenth-note notation markings along each scythe-curved arm-blade, orbiting rose bolt burst fragments and rapid 3-bolt spread spark particles constantly filling offbeat gaps in graceful spiral around the diamond head, ribbon-tail of rose energy blazes majestically behind the entity while two thin backward-curving arm-blades pulse with inner staccato luminescence, multiple rapid offbeat burst formations fire naturally creating organic countersubject gap-filling patterns, iridescent prismatic rose energy charges through pristine curved chrome framework, staccato position-snap afterimages and lingering rose ribbon-tail hazard fragments cascade from the ribbon tail, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic staccato ferocity radiating, epic powerful sprite art, full sleek curved entity composition, --ar 16:9 --v 7.0
```

**6. Fourth Voice Apex (Mini-Boss)**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent massive geometric conductor figure four times player height made of imposing dark chrome, pristine broad-shouldered imposing body with flowing veins of indigo teal rose and amber across the surface and tall crown-like head structure with four glowing points in each voice color and right arm holding spectral conductor baton of white chrome light forming clean elegant apex conductor outline with stretto command finish, conductor body filled with flowing amorphous stretto frequencies and all-four-voice commanding energy swirling through visible full musical score on torso with all four voice parts simultaneously active like an orchestral conductor demanding perfect unified performance, wavy iridescent energy flows through visible spectral conductor baton trailing persistent light patterns and platform of interlocking chromatic rings below the hovering figure down entire massive conductor creating visible fourth voice command currents, dark chrome surface with all four Polyphonic accent color veins decorated with flowing stretto dynamics and four-part score notation markings running across the broad torso, orbiting fermata freeze-ring fragments and coordinated salvo spark particles constantly synchronizing in graceful spiral around the crown-like head, spectral white chrome baton blazes majestically with conducting gesture trails while four chromatic ring platform pulses with inner commanding luminescence, multiple fermata freeze-and-release formations burst naturally creating organic four-voice stretto patterns, iridescent all-four-voice energy charges simultaneously through pristine dark chrome conductor framework, baton-shatter afterimages and four-colored converging beam fragments cascade from the conducting figure on death, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic commanding authority radiating, epic powerful sprite art, full massive conductor mini-boss composition, --ar 16:9 --v 7.0
```

**7. Phantom Violinist**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent elegant spectral figure in formal concert tuxedo rendered in sleek indigo-chrome metal, pristine formal performance attire body holding metallic phantom violin under chin and bow in right hand with featureless chrome mask face bearing two staff-line eye slits forming clean elegant violinist outline with virtuoso performance finish, violinist body filled with flowing amorphous violin frequencies and rapid darting bow-stroke energy swirling through the crystalline violin construct with visible strings made of pure light like an eternal mid-performance given lethal form, wavy iridescent energy flows through visible bow-stroke sweeping arcs and SemiCircularSmear attack patterns and translucent legs trailing into mist below the knee down entire spectral figure creating visible phantom performance currents, indigo-chrome metal surface decorated with flowing virtuoso dynamics and rapid bowing notation markings running along the formal attire, orbiting thin line-projectile fragments and sweeping bow-stroke spark particles constantly darting in graceful spiral around the chrome mask face, crystalline violin blazes majestically with visible light strings while phantom bow sweeps with inner indigo luminescence, multiple 120-degree sweeping arc formations fire naturally creating organic violin bow-stroke patterns, iridescent indigo energy charges through pristine sleek chrome formal framework, violin-breaking afterimages and cascading indigo note dissolution fragments cascade from the shattered crystalline instrument on death, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic virtuoso elegance radiating, epic powerful sprite art, full spectral violinist composition, --ar 16:9 --v 7.0
```

**8. Spectral Cellist**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent seated spectral figure holding massive chromatic cello between its knees in teal-chrome with amber accents, pristine wide grounded stable body seated on invisible chair with towering metallic cello instrument emanating visible sound wave ripples and broad chrome mask face with bass clef etched across it forming clean elegant cellist outline with deep resonant finish, cellist body filled with flowing amorphous deep cello frequencies and heavy resonant blast energy swirling through the massive metallic instrument with sound waves rippling outward constantly like an immovable anchor of devastating bass power, wavy iridescent energy flows through visible cello sound-wave ripples and terrain-phasing resonant orb projectile paths and legs folded trailing into chromatic mist down entire seated figure creating visible spectral resonance currents, teal-chrome and amber accent surfaces decorated with flowing deep resonance dynamics and bass clef notation markings running across the broad mask face, orbiting resonant orb fragments and terrain-vibration spark particles constantly emanating in graceful spiral around the massive cello body, towering metallic cello blazes majestically with teal-amber bloom while visible sound waves ripple outward with inner deep-frequency luminescence, multiple terrain-phasing resonant zone formations persist naturally creating organic cello ground-denial patterns, iridescent teal-amber energy charges through pristine chrome-cello framework, sequential string-snap afterimages and chromatic dissolution fragments cascade from the cello as each string breaks on death, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic deep resonance radiating, epic powerful sprite art, full seated cellist entity composition, --ar 16:9 --v 7.0
```

**9. The Concertmaster (Mini-Boss)**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent towering conductor figure five times player height in formal white-chrome tailcoat over graphite-void body, pristine noble chrome mask face with all five musical staff lines as glowing horizontal bands and right hand holding the Concertmaster Baton a slender white-chrome wand with blazing crystal tip cycling through indigo teal rose amber and left hand making precise conducting gestures and flowing cape of chromatic energy displaying scrolling sheet music forming clean elegant concertmaster outline with grand orchestral finish, conductor body filled with flowing amorphous orchestral frequencies and four-section commanding energy swirling through the cape of sheet music and four orbiting chromatic rings representing orchestra sections like the supreme commander of all harmonic forces marshaling strings brass woodwinds and percussion, wavy iridescent energy flows through visible four orbiting chromatic section rings and conducting gesture patterns and grand crescendo charge attacks down entire towering figure creating visible concertmaster command currents, white-chrome tailcoat and graphite-void body surfaces decorated with flowing orchestral dynamics and full-score section notation markings scrolling along the chromatic cape, orbiting section echo fragments and grand crescendo spark particles constantly coordinating in graceful spiral around the noble chrome mask, Concertmaster Baton blazes majestically with cycling crystal tip while four orbiting section rings pulse with inner orchestral luminescence, multiple section-coordinated attack formations burst naturally creating organic grand crescendo patterns, iridescent all-section cycling energy charges through pristine white-chrome tailcoat framework, grand crescendo afterimages and four-section simultaneous burst fragments cascade from the blazing baton tip, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic orchestral magnificence radiating, epic powerful sprite art, full towering concertmaster mini-boss composition, --ar 16:9 --v 7.0
```

**10. Chromatic Harbinger**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent shifting amorphous humanoid shape constantly reconfiguring between geometric forms with all 12 chromatic colors visible, pristine solid chrome shell body with current semitone color blazing from every joint and seam and dodecahedron 12-faced head where each face displays a different musical glyph forming clean elegant harbinger outline with 12-semitone cycling finish, harbinger body filled with flowing amorphous all-twelve-semitone frequencies and cycling attack-mode energy swirling through the chromatic scale helix notation spiraling up the torso like the entire chromatic scale made flesh and given devastating purpose, wavy iridescent energy flows through visible 12-form body reconfiguration and smooth 1-second chromatic morph transitions between each semitone key down entire shifting figure creating visible chromatic harbinger currents, chrome shell surface with cycling semitone color decorated with flowing 12-attack-mode dynamics and chromatic scale notation markings spiraling up the torso helix, orbiting 12-radial beam fragments and semitone-cycling spark particles constantly shifting between all 12 attack types in graceful spiral around the dodecahedron head, all 12 glyph faces blaze majestically in sequence while body oscillates between two and four times player height with inner chromatic luminescence cycling through every color, multiple 12-simultaneous-attack formations fire naturally on the devastating B-key creating organic all-semitone patterns, iridescent all-12-colors radiating energy charges through pristine chrome shifting framework, 12-radial beam afterimages firing simultaneously and white-chrome collapse-to-burst fragments cascade from every joint on death, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic 12-tone devastation radiating, epic powerful sprite art, full shifting harbinger entity composition, --ar 16:9 --v 7.0
```

**11. The Incomplete Maestro (Event Boss)**
```
Magnificent side-view pixel art sprite of an extraordinarily grand chromatic iridescent towering spectral composer figure in tattered formal attire made of dark chrome and void metal, pristine fractured and incomplete body with visible gaps where sections are missing held together only by chromatic energy bridges and cracked chrome mask face with one half elegant and composed and the other half dissolving into void and broken spectral baton trailing incomplete musical notation forming clean elegant incomplete maestro outline with magnificent unfinished masterwork finish, maestro body filled with flowing amorphous unfinished symphony frequencies and three-phase escalating energy swirling through a massive unfinished musical score floating behind it like wings with notes trailing off into nothingness and robes of sheet music metal fluttering with unresolved harmonies, wavy iridescent energy flows through visible chromatic energy bridges spanning body gaps and incomplete notation trails and phase-transition visual glitching down entire fractured figure creating visible incomplete maestro currents, dark chrome and void metal surfaces decorated with flowing unfinished dynamics and trailing-off notation markings dissolving into void along each fracture and energy bridge, orbiting unresolved cadence beam fragments and chromatic apocalypse spark particles constantly escalating through three phases in graceful spiral around the cracked mask, broken spectral baton blazes majestically with incomplete notation trails while massive unfinished score wings flutter with inner incomplete luminescence radiating magnificent unfinished glory, multiple chromatic apocalypse 12-wave formations devastate naturally creating organic unfinished symphony patterns, iridescent all-Polyphonic-color energy charges through pristine fractured chrome and void framework and the maestro does not explode on death but fades into a single sustained note that resolves into silence, unresolved cadence afterimages and final whole-note resolution fragments cascade from the dissolving figure, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic incomplete magnificence radiating, epic powerful sprite art, full towering event boss composition, --ar 16:9 --v 7.0
```

### 8.2 Material Prompts

**1. Voicestone Shard**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent small jagged angular stone fragment, pristine deep gray stone with glowing indigo cracks running through it like luminous veins and faint teal mist emanating from the fractured edges forming clean elegant voicestone outline with humming resonance finish, shard interior filled with flowing amorphous single-tone frequencies and faint harmonic energy swirling through the indigo crack network like a stone that has been singing for millennia, wavy iridescent indigo-teal energy flows through visible vein fractures pulsing rhythmically creating visible voicestone currents, deep gray rough natural texture contrasted with supernatural glowing fractures decorated with faint harmonic notation markings, orbiting faint indigo mote fragments drift naturally from the edges, indigo cracks pulse majestically with inner harmonic luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic harmonic beauty radiating, epic crafting material sprite art, full stone shard composition, --ar 16:9 --v 7.0
```

**2. Polyphonic Ore**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent chunky ore block with crystal growths, pristine deep stone base with brilliant iridescent crystal clusters that shift between indigo and teal in light and embedded musical staff line patterns visible in the crystal faces forming clean elegant polyphonic ore outline with harmonic mineral finish, ore body filled with flowing amorphous ambient harmonic frequencies and supernatural chromatic crystal energy swirling through the natural ore formation like solidified music emerging from the earth, wavy iridescent indigo-teal energy flows through visible crystal growths and staff-line patterns creating visible polyphonic resonance currents, deep stone base with brilliant iridescent crystal clusters decorated with flowing harmonic dynamics and natural staff-line notation markings etched into crystal faces, orbiting chromatic mote fragments drift naturally from crystal tips, iridescent crystal formations blaze majestically with inner harmonic luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic mineral beauty radiating, epic crafting material sprite art, full ore block composition, --ar 16:9 --v 7.0
```

**3. Polyphonic Bar**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent sleek rectangular refined ingot, pristine brushed-chrome surface with iridescent color bands of indigo teal and rose flowing across its face like oil on water in constant motion forming clean elegant polyphonic bar outline with refined harmonic metal finish, bar interior filled with flowing amorphous polished harmonic frequencies and refined metallic energy swirling through the brushed-chrome surface like liquid music frozen into perfect geometric form, wavy iridescent indigo-teal-rose energy flows through visible oil-on-water color bands creating visible polyphonic metal currents, brushed-chrome surface decorated with subtle flowing harmonic dynamics and faint geometric notation markings, orbiting faint iridescent mote fragments drift naturally from bar edges, clean geometric shape blazes majestically with inner refined luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic refined beauty radiating, epic crafting material sprite art, full ingot composition, --ar 16:9 --v 7.0
```

**4. Harmonic Voice Fragment**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent floating translucent crystalline shard shaped like an angular music note, pristine crystallized harmonic energy with internal chromatic light and trails of teal and rose mist emanating from the fractured edges forming clean elegant voice fragment outline with crystallized melody finish, fragment interior filled with flowing amorphous melodic frequencies and whispered harmonic energy swirling through the translucent crystal like a destroyed harmonic entity's last song frozen in crystalline form, wavy iridescent teal-rose energy flows through visible internal chromatic light and mist trails creating visible harmonic voice currents, translucent crystal surface decorated with flowing melodic dynamics and angular music note notation markings, orbiting teal and rose mist fragments drift naturally from edges, internal chromatic light blazes majestically with inner harmonic luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic crystallized melody radiating, epic crafting material sprite art, full floating crystal shard composition, --ar 16:9 --v 7.0
```

**5. Chromatic Alloy**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent wide refined bar with mirror-polish surface, pristine advanced alloy with full chromatic palette cycling across its face indigo teal rose amber white simultaneously and faint hexagonal lattice geometric etching with slight glow at edges forming clean elegant chromatic alloy outline with all-frequency metal finish, bar interior filled with flowing amorphous all-chromatic frequencies and advanced harmonic energy swirling through the mirror-polish surface like every color in the Polyphonic spectrum existing simultaneously in perfect metallic harmony, wavy iridescent full-palette energy flows through visible hexagonal lattice etching and cycling colors creating visible chromatic alloy currents, mirror-polish surface decorated with flowing all-spectrum dynamics and hexagonal geometric notation markings, orbiting chromatic cycling mote fragments drift naturally from glowing edges, full palette cycling blazes majestically with inner all-frequency luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic all-spectrum beauty radiating, epic crafting material sprite art, full wide alloy bar composition, --ar 16:9 --v 7.0
```

**6. Concertmaster's Baton**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent thin elegant conducting wand broken at the tip, pristine white chrome body with spiraling inlays of indigo rose and amber climbing up the length and chromatic energy leaking from the broken tip like escaped harmonic light and musical staff lines etched along the baton length forming clean elegant baton outline with broken authority finish, baton interior filled with flowing amorphous conducting frequencies and commanding harmonic energy leaking from the fractured tip like the authority of a spectral orchestra leader that persists even after breaking, wavy iridescent indigo-rose-amber energy flows through visible spiraling inlays and staff-line engravings and leaked chromatic energy from broken tip creating visible concertmaster currents, white chrome surface decorated with flowing conducting dynamics and spiraling notation markings climbing the baton length, orbiting chromatic energy leak fragments drift naturally from the broken tip, spiraling inlays blaze majestically with inner commanding luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic broken authority radiating, epic crafting material sprite art, full conducting baton composition, --ar 16:9 --v 7.0
```

**7. Convergence Fragment**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent compact diamond-shaped crystal with intensely bright core, pristine dense crystal with white chrome core surrounded by compressed layers of all polyphonic colors indigo teal rose amber and visible internal fracture lines pulsing with light forming clean elegant convergence outline with near-explosion compression finish, crystal interior filled with flowing amorphous converged frequencies and all harmonic voices forced into single point energy swirling under immense internal pressure like perpetual state of near-detonation crystallized into impossible stability, wavy iridescent all-Polyphonic-color energy flows through visible compressed layers and pulsing fracture lines creating visible convergence currents, compressed layer surfaces decorated with flowing convergence dynamics and fracture-line notation markings pulsing with light, orbiting compressed chromatic fragments drift naturally from fracture lines, intensely bright white chrome core blazes majestically with inner explosive luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic compressed power radiating, epic crafting material sprite art, full diamond crystal composition, --ar 16:9 --v 7.0
```

**8. Chromatic Core**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent spherical core with 12 distinct color facets, pristine living heart of chromatic energy with 12 facets arranged like a chromatic scale each glowing in sequence like a heartbeat and surrounded by orbiting chromatic mote particles with metallic frame holding the sphere together forming clean elegant chromatic core outline with 12-semitone heartbeat finish, core interior filled with flowing amorphous 12-semitone frequencies and cycling sequential energy swirling through each color facet in perfect chromatic scale order like the living pulse of pure chromatic power, wavy iridescent 12-color energy flows through visible sequential facet glow and orbiting mote particles creating visible chromatic core currents, metallic frame and 12-color facet surfaces decorated with flowing chromatic scale dynamics and sequential heartbeat notation markings, orbiting chromatic mote particles blaze majestically in synchronized orbit, each facet pulses in sequence with inner heartbeat luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic living heartbeat radiating, epic crafting material sprite art, full spherical core composition, --ar 16:9 --v 7.0
```

**9. Enharmonic Alloy**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent tall refined ingot that shifts between two color states, pristine ultimate Polyphonic metal with surface existing in enharmonic ambiguity shifting between two entirely different color states like a lenticular print and clean architectural lines with slight reality-distortion shimmer at edges forming clean elegant enharmonic alloy outline with dual-state metal finish, ingot interior filled with flowing amorphous enharmonic frequencies and dual-identity energy swirling through the lenticular surface like every color simultaneously being itself and its enharmonic equivalent in impossible superposition, wavy iridescent dual-color-state energy flows through visible lenticular shifting and reality-distortion edge shimmer creating visible enharmonic currents, dual-state surface decorated with flowing enharmonic dynamics and architectural-line notation markings, orbiting reality-shimmer fragments drift naturally from distorted edges, lenticular surface blazes majestically shifting between states with inner ambiguous luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic enharmonic ambiguity radiating, epic crafting material sprite art, full tall ingot composition, --ar 16:9 --v 7.0
```

**10. Fragment of the Unfinished**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent torn page of sheet music rendered in metal, pristine metallic sheet music with visible musical notation that trails off into void at the edges with edges dissolving into chromatic particles and the ink being liquid iridescent metal that shifts and flows forming clean elegant unfinished fragment outline with incomplete masterwork finish, fragment body filled with flowing amorphous yearning frequencies and aching potential energy swirling through the incomplete notation like a symphony that was never completed but contains all harmonic knowledge yet lacks resolution, wavy iridescent chromatic energy flows through visible liquid iridescent ink and dissolving particle edges creating visible unfinished currents, metallic sheet music surface decorated with flowing incomplete dynamics and trailing-off notation markings dissolving into void, orbiting chromatic particle dissolution fragments drift naturally from torn edges, liquid iridescent ink notation blazes majestically while flowing with inner yearning luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic incomplete yearning radiating, epic crafting material sprite art, full torn metallic sheet music composition, --ar 16:9 --v 7.0
```

**11. Opus Null**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent perfect sphere of absolute void surrounded by luminous color ring, pristine perfect sphere of absolute void darkness at center absorbing all light surrounded by thin shell of every Polyphonic color compressed into a single luminous iridescent ring and orbiting musical notation symbols dissolving as they approach the center forming clean elegant opus null outline with silence-before-music finish, sphere interior filled with flowing amorphous silence frequencies and infinite potential energy swirling as absolute nothing at the core like the void from which all music emerges simultaneously nothing and everything, wavy iridescent all-Polyphonic-color compressed ring energy flows through visible thin luminous shell radiating all colors simultaneously creating visible opus null currents, absolute void center absorbs all light while outer ring surface decorated with flowing void dynamics and dissolving notation markings approaching the center, orbiting notation symbol fragments dissolve naturally as they approach the void center, compressed all-color luminous ring blazes majestically around the absolute darkness with inner infinite-potential luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic void-and-everything radiating, epic ultimate crafting material sprite art, full void sphere composition, --ar 16:9 --v 7.0
```

### 8.3 World Event Sky / Background Prompts

**1. Dissonance Surge Sky**
```
Magnificent panoramic pixel art background of an extraordinarily grand chromatic iridescent Terraria night sky transitioning from normal starfield to deep indigo wash, pristine night sky with iridescent chromatic aurora-like streaks weaving across the upper atmosphere and stars that appear to vibrate and shimmer with harmonic unease and the moon gaining a faint chromatic halo of indigo and teal forming clean elegant dissonance surge sky with ominous beauty finish, sky filled with flowing amorphous dissonant atmospheric frequencies and chromatic aurora energy swirling through the indigo wash like the earth's single sustained voice finally cracking after millennia, wavy iridescent indigo-teal energy flows through visible aurora streaks and vibrating stars and a visible crack of chromatic light running across one portion of the sky creating visible dissonance surge atmospheric currents, deep indigo wash surface decorated with flowing aurora dynamics and chromatic halo notation markings around the trembling moon, orbiting chromatic aurora fragments and vibrating star spark particles constantly weaving across the upper sky, chromatic crack of light blazes majestically across sky while indigo aurora streaks pulse with inner ominous luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic dissonant atmosphere radiating, epic panoramic sky composition, --ar 16:9 --v 7.0
```

**2. Fugal Invasion Sky**
```
Magnificent panoramic pixel art background of an extraordinarily grand chromatic iridescent Terraria night sky split into four distinct horizontal bands, pristine four-banded sky with indigo at bottom then teal then rose then amber at top each band weaving and intertwining creating a braided aurora effect with colors bleeding into each other at boundaries forming clean elegant fugal invasion sky with four-voice counterpoint finish, sky filled with flowing amorphous four-voice harmonic frequencies and braided counterpoint energy swirling through each color band like four independent melodic lines sounding simultaneously in the night sky, wavy iridescent four-color energy flows through visible braided intertwining bands and secondary hues at boundaries with stars visible between the bands creating visible fugal atmospheric currents, four horizontal band surfaces decorated with flowing fugal counterpoint dynamics and intertwining voice notation markings where bands bleed into each other, orbiting four-voice aurora fragments and braided color spark particles constantly weaving between bands, all four bands blaze majestically while intertwining with inner counterpoint luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic fugal atmosphere radiating, epic panoramic sky composition, --ar 16:9 --v 7.0
```

**3. Grand Rehearsal Sky**
```
Magnificent panoramic pixel art background of an extraordinarily grand chromatic iridescent Terraria night sky darkened to deep velvet black concert hall ambiance, pristine velvet black sky with dramatic spotlights in indigo teal rose and amber sweeping across like stage lighting and a faint constellation-like pattern of dots forming an orchestral pit layout in the upper sky and subtle sheet music notation faintly visible in the clouds forming clean elegant grand rehearsal sky with theatrical performance finish, sky filled with flowing amorphous orchestral rehearsal frequencies and concert hall atmospheric energy swirling through the sweeping spotlights like an orchestra assembling in the sky itself tuning and warming for the grand performance, wavy iridescent four-section spotlight energy flows through visible sweeping stage lights and orchestral pit constellation and faint cloud notation creating visible rehearsal atmospheric currents, velvet black concert hall surface decorated with flowing theatrical dynamics and orchestral pit constellation notation markings, orbiting spotlight sweep fragments and sheet music cloud spark particles constantly performing across the sky, stage lighting spotlights blaze majestically in section colors while orchestral pit constellation pulses with inner performance luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic theatrical atmosphere radiating, epic panoramic sky composition, --ar 16:9 --v 7.0
```

**4. Chromatic Convergence Sky**
```
Magnificent panoramic pixel art background of an extraordinarily grand chromatic iridescent Terraria sky cycling through all 12 colors of a chromatic scale, pristine full-spectrum sky smoothly transitioning from deep void through indigo through teal through rose through amber to blazing white chrome brilliance with each color section flowing into the next creating a full chromatic gradient and stars pulsing in time with the color cycle forming clean elegant chromatic convergence sky with living chromatic scale finish, sky filled with flowing amorphous 12-semitone atmospheric frequencies and chromatic scale cycling energy swirling through each color transition like the entire sky becoming a living breathing chromatic scale played across 18 minutes, wavy iridescent 12-color sequential energy flows through visible smooth color transitions and pulsing synchronized stars creating visible chromatic convergence atmospheric currents, full spectrum gradient surface decorated with flowing chromatic scale dynamics and semitone transition notation markings, orbiting 12-color cycling fragments and synchronized star-pulse spark particles constantly ascending the chromatic scale, entire sky blazes majestically through the complete chromatic spectrum with inner 12-semitone luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic convergence atmosphere radiating, epic panoramic sky composition, --ar 16:9 --v 7.0
```

**5. Unfinished Symphony Sky -- Movement IV (Finale)**
```
Magnificent panoramic pixel art background of an extraordinarily grand chromatic iridescent Terraria sky that has dissolved entirely into a field of floating musical notation, pristine void black sky filled with visible staff lines stretching across the emptiness and floating notes rests and dynamic markings suspended in space as if the sky itself became a page of sheet music with some notes being erased fading to nothing and one final whole note glowing brilliantly at center forming clean elegant unfinished symphony sky with haunting incomplete finale finish, sky filled with flowing amorphous unfinished symphony frequencies and dissolving notation energy swirling through the staff-line void like the incomplete maestro's abandoned masterwork projected onto the fabric of reality itself, wavy iridescent chromatic notation energy flows through visible floating staff lines and erasing notes and the single brilliant whole note creating visible unfinished atmospheric currents, void black sky between notation decorated with flowing incomplete dynamics and dissolving musical marking notation fading to nothing, orbiting erasing note fragments and dissolving rest symbol spark particles constantly fading across the staff-lined void, final whole note blazes majestically at sky center while surrounding notation erases with inner incomplete luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, overwhelming chromatic incomplete haunting beauty radiating, epic panoramic sky composition, --ar 16:9 --v 7.0
```

### 8.4 Event Summoning Item Prompts

**Rehearsal Score (Grand Rehearsal Summon)**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent rolled sheet of metallic musical score paper, pristine chrome surface scroll with visible musical notation in indigo ink tied with a teal and rose chromatic ribbon and the scroll glowing faintly with inner harmonic light like a formal concert program radiating elegant orchestral authority forming clean elegant rehearsal score outline with grand performance invitation finish, scroll body filled with flowing amorphous orchestral tuning frequencies and anticipatory performance energy swirling through the visible notation like the written instructions for summoning an entire spectral orchestra into existence, wavy iridescent indigo-teal-rose energy flows through visible notation ink and chromatic ribbon and inner scroll glow creating visible rehearsal summoning currents, chrome scroll surface decorated with flowing orchestral performance dynamics and formal concert notation markings in indigo ink, orbiting chromatic ribbon fragments and faint notation spark particles drift naturally from the glowing scroll, chromatic ribbon blazes majestically while inner light pulses with orchestral luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic orchestral anticipation radiating, epic summoning item sprite art, full scroll composition, --ar 16:9 --v 7.0
```

**Incomplete Score (Unfinished Symphony Summon)**
```
Magnificent centered pixel art sprite of an extraordinarily grand chromatic iridescent tattered torn sheet of metallic musical score paper, pristine chrome surface score with visible musical notation that trails off into void at the torn edges with burn marks and fracture lines across the surface and notes dissolving into chromatic particles at each tear and dark void energy seeping from the gaps in the notation forming clean elegant incomplete score outline with haunting unfinished summoning finish, score body filled with flowing amorphous unresolved harmonic frequencies and desperate yearning energy swirling through the incomplete notation like a summoning ritual for something that was never meant to be finished but demands completion through sheer force of will, wavy iridescent chromatic void energy flows through visible trailing-off notation and dissolving particle edges and dark void seepage creating visible incomplete summoning currents, tattered chrome surface decorated with flowing unfinished dynamics and dissolving fracture-line notation markings, orbiting chromatic dissolution fragments and void-seep spark particles drift naturally from torn edges, dark void energy blazes majestically through notation gaps while dissolving notes pulse with inner incomplete luminescence, Terraria legendary pixel art aesthetic with maximum ornate flowing detail, chromatic haunting incompleteness radiating, epic summoning item sprite art, full tattered score composition, --ar 16:9 --v 7.0
```

---

## 9. Proposed File Structure

All Polyphonic Line content follows the existing MagnumOpus conventions: each item/enemy gets its own folder containing the `.cs` and `.png` files. Shared systems go in `Common/`.

```
Content/
  PolyphonicLine/
    PolyphonicPalette.cs                          -- 6-color core + 12-color extended palette
    PolyphonicShaderManager.cs                     -- Shader management for Polyphonic VFX
    PolyphonicVFXLibrary.cs                        -- Chromatic Mote, Voice Note, Cadence Burst, Iridescent Trail

    DamageClass/
      PolyphonicDamageClass.cs                     -- Custom DamageClass definition
      PolyphonicRarity.cs                          -- Custom rarity with 4-phase color cycling
      HarmonicSearDebuff.cs                        -- The Harmonic Sear stackable debuff
      HarmonicSearDebuff.png
      HarmonicOverloadDebuff.cs                    -- Cross-system interaction (Harmonic Sear + Resonant Burn)
      HarmonicOverloadDebuff.png
      PolyphonicDamagePlayer.cs                    -- ModPlayer for Polyphonic stat tracking

    Materials/
      VoicestoneShard/
        VoicestoneShard.cs
        VoicestoneShard.png
      PolyphonicOre/
        PolyphonicOre.cs                           -- Ore tile
        PolyphonicOre.png
      PolyphonicBar/
        PolyphonicBar.cs
        PolyphonicBar.png
      HarmonicVoiceFragment/
        HarmonicVoiceFragment.cs
        HarmonicVoiceFragment.png
      ChromaticAlloy/
        ChromaticAlloy.cs
        ChromaticAlloy.png
      ConcertmastersBaton/
        ConcertmastersBaton.cs                     -- Material version (also dropped by mini-boss)
        ConcertmastersBaton.png
      ConvergenceFragment/
        ConvergenceFragment.cs
        ConvergenceFragment.png
      ChromaticCore/
        ChromaticCore.cs
        ChromaticCore.png
      EnharmonicAlloy/
        EnharmonicAlloy.cs
        EnharmonicAlloy.png
      FragmentOfTheUnfinished/
        FragmentOfTheUnfinished.cs
        FragmentOfTheUnfinished.png
      OpusNull/
        OpusNull.cs
        OpusNull.png

    Weapons/
      PreHardmode/
        OvertoneEdge/
          OvertoneEdge.cs
          OvertoneEdge.png
          OvertoneEdgeVFX.cs
          OvertoneEdgeProjectile.cs                -- Echo swing projectile
        ResonantShortbow/
          ResonantShortbow.cs
          ResonantShortbow.png
          ResonantShortbowVFX.cs
          ResonantArrow.cs                         -- Converted arrow projectile
          ResonantArrow.png
        DissonanceStaff/
          DissonanceStaff.cs
          DissonanceStaff.png
          DissonanceStaffVFX.cs
          DissonanceBolt.cs                        -- Bouncing bolt projectile
          DissonanceBolt.png
        ChromaticWhip/
          ChromaticWhip.cs
          ChromaticWhip.png
          ChromaticWhipVFX.cs
          ChromaticSigil.cs                        -- Ground sigil effect

      Hardmode/
        ContraryMotion/
          ContraryMotion.cs
          ContraryMotion.png
          ContraryMotionVFX.cs
          ContraryMotionShockwave.cs               -- Shockwave projectile
        AugmentedLongbow/
          AugmentedLongbow.cs
          AugmentedLongbow.png
          AugmentedLongbowVFX.cs
          AugmentedArrow.cs                        -- Semitone-ascending arrow
          AugmentedArrow.png
        InvertibleCounterpoint/
          InvertibleCounterpoint.cs
          InvertibleCounterpoint.png
          InvertibleCounterpointVFX.cs
          CounterpointBeam.cs                      -- Dual beam projectile
        SuspensionChains/
          SuspensionChains.cs
          SuspensionChains.png
          SuspensionChainsVFX.cs
          HarmonicStasisDebuff.cs                  -- Suspension debuff
          HarmonicStasisDebuff.png
        PedalToneRepeater/
          PedalToneRepeater.cs
          PedalToneRepeater.png
          PedalToneRepeaterVFX.cs
          ResonantBullet.cs                        -- Normal shot
          PedalToneBlast.cs                        -- 5th shot massive blast
          PedalToneBlast.png

      PostPlantera/
        CanonOfChromaticFire/
          CanonOfChromaticFire.cs
          CanonOfChromaticFire.png
          CanonOfChromaticFireVFX.cs
          CanonOrb.cs                              -- Primary + clone orb
          CanonOrb.png
        ObligatoBlade/
          ObligatoBlade.cs
          ObligatoBlade.png
          ObligatoBladeVFX.cs
          ChromaticEdgeProjectile.cs               -- Full Chromatic Edge slash
        Ricercar/
          Ricercar.cs
          Ricercar.png
          RicercarVFX.cs
          RicercarProjectile.cs                    -- Seeking spiral orb
          RicercarProjectile.png
        PasacagliasDescent/
          PasacagliasDescent.cs
          PasacagliasDescent.png
          PasacagliasDescentVFX.cs

      PostGolem/
        ToccataGauntlets/
          ToccataGauntlets.cs
          ToccataGauntlets.png
          ToccataGauntletsVFX.cs
          ToccataBolt.cs                           -- Burst projectiles
          ToccataBolt.png
        CantusFirmus/
          CantusFirmus.cs
          CantusFirmus.png
          CantusFirmusVFX.cs
          CantusFirmusBuff.cs                      -- Foundation buff
          CantusFirmusBuff.png
        TwelveToneRow/
          TwelveToneRow.cs
          TwelveToneRow.png
          TwelveToneRowVFX.cs
          TwelveToneRound.cs                       -- Per-semitone bullets
          TwelveToneRound.png
        IsorhythmicMotet/
          IsorhythmicMotet.cs
          IsorhythmicMotet.png
          IsorhythmicMotetVFX.cs
          MotetSentinel.cs                         -- Summon entity
          MotetSentinel.png
          MotetSentinelBuff.cs
          MotetSentinelBuff.png

      PostMoonLord/
        GroundBassAnchor/
          GroundBassAnchor.cs
          GroundBassAnchor.png
          GroundBassAnchorVFX.cs
          GroundBassWave.cs                        -- Persistent ground wave
        StrettoConvergence/
          StrettoConvergence.cs
          StrettoConvergence.png
          StrettoConvergenceVFX.cs
        HarmonicSeriesBow/
          HarmonicSeriesBow.cs
          HarmonicSeriesBow.png
          HarmonicSeriesBowVFX.cs
          OvertoneArrow.cs                         -- Homing overtone arrows
          OvertoneArrow.png
        ChromaticFantasy/
          ChromaticFantasy.cs
          ChromaticFantasy.png
          ChromaticFantasyVFX.cs
          ChromaticBeam.cs                         -- Color-cycling beam
          ChromaticSaturationDebuff.cs
          ChromaticSaturationDebuff.png
        TheUnfinishedFugue/
          TheUnfinishedFugue.cs
          TheUnfinishedFugue.png
          TheUnfinishedFugueVFX.cs
          FugueProjectile.cs                       -- Subject/Answer/Stretto projectiles
          FugueProjectile.png
          CodaBurst.cs                             -- 8-directional coda
        OmniVoce/
          OmniVoce.cs
          OmniVoce.png
          OmniVoceVFX.cs
          VocePrimaSword.cs                        -- Sword form projectiles
          VoceSecondaBow.cs                        -- Bow form arrows
          VoceTerzaOrb.cs                          -- Staff form orbs
          VoceQuartaSentinel.cs                    -- Summon form entity
          VoceQuartaSentinel.png
          TuttiBuff.cs
          TuttiBuff.png

    Enemies/
      PreHardmode/
        DissonantWraith/
          DissonantWraith.cs
          DissonantWraith.png
          DissonantWraithVFX.cs
        ChromaticSlime/
          ChromaticSlime.cs
          ChromaticSlime.png
          ChromaticSlimeVFX.cs
          ChromaticBolt.cs                         -- Slime projectile
          ChromaticBolt.png

      Hardmode/
        FirstVoiceSentinel/
          FirstVoiceSentinel.cs
          FirstVoiceSentinel.png
          FirstVoiceSentinelVFX.cs
          IndigoBolt.cs
          IndigoBolt.png
        SecondVoiceEcho/
          SecondVoiceEcho.cs
          SecondVoiceEcho.png
          SecondVoiceEchoVFX.cs
          TealBolt.cs
          TealBolt.png
        ThirdVoiceHarmonic/
          ThirdVoiceHarmonic.cs
          ThirdVoiceHarmonic.png
          ThirdVoiceHarmonicVFX.cs
          RoseBolt.cs
          RoseBolt.png
        FourthVoiceApex/
          FourthVoiceApex.cs
          FourthVoiceApex.png
          FourthVoiceApexVFX.cs

      PostPlantera/
        PhantomViolinist/
          PhantomViolinist.cs
          PhantomViolinist.png
          PhantomViolinistVFX.cs
          ViolinBowProjectile.cs                   -- Sweeping line projectile
          ViolinBowProjectile.png
        SpectralCellist/
          SpectralCellist.cs
          SpectralCellist.png
          SpectralCellistVFX.cs
          ResonantOrb.cs                           -- Heavy blast projectile
          ResonantOrb.png
        TheConcertmaster/
          TheConcertmaster.cs
          TheConcertmaster.png
          TheConcertmasterVFX.cs

      PostGolem/
        ChromaticHarbinger/
          ChromaticHarbinger.cs
          ChromaticHarbinger.png
          ChromaticHarbingerVFX.cs
          SemitoneBolt.cs                          -- 12 variants handled in code
          SemitoneBolt.png

      PostMoonLord/
        TheIncompleteMaestro/
          TheIncompleteMaestro.cs
          TheIncompleteMaestro.png
          TheIncompleteMaestroVFX.cs
          MaestroProjectile.cs                     -- Various attack projectiles
          MaestroProjectile.png
          UnresolvedCadenceBeam.cs                 -- Phase 2 beam attack
          ChromaticApocalypse.cs                   -- Phase 3 ultimate
          IncompleteMaestroTrophy.cs
          IncompleteMaestroTrophy.png
          IncompleteMaestroMask.cs
          IncompleteMaestroMask.png
          MaestrosRelic.cs
          MaestrosRelic.png

    Events/
      DissonanceSurge/
        DissonanceSurgeEvent.cs                    -- Event controller + world data
        DissonanceSurgeSky.cs                      -- Custom sky overlay
      FugalInvasion/
        FugalInvasionEvent.cs
        FugalInvasionSky.cs
        FugalTimingSystem.cs                       -- Beat-cycle AI coordination
      GrandRehearsal/
        GrandRehearsalEvent.cs
        GrandRehearsalSky.cs
        RehearsalScore.cs                          -- Summoning item
        RehearsalScore.png
      ChromaticConvergence/
        ChromaticConvergenceEvent.cs
        ChromaticConvergenceSky.cs
        PolyphonicKillCounter.cs                   -- Tracks cumulative kills
      UnfinishedSymphony/
        UnfinishedSymphonyEvent.cs
        UnfinishedSymphonySky.cs
        IncompleteScore.cs                         -- Summoning item
        IncompleteScore.png

Common/
  Systems/
    PolyphonicOreGeneration.cs                     -- World gen for Polyphonic Ore veins
    PolyphonicEventSystem.cs                       -- Central event trigger/management
    HarmonicSearSystem.cs                          -- Stack tracking + Harmonic Overload logic
    PolyphonicMaterialDrops.cs                     -- Enemy drop assignments
```

---

## 10. Content Summary

| Category | Count | Status |
|---|---|---|
| New Damage Class | 1 (Polyphonic) | Planned |
| Crafting Materials | 11 | Planned |
| World Events | 5 | Planned |
| Enemies (standard) | 8 | Planned |
| Mini-Bosses | 2 (Fourth Voice Apex, The Concertmaster) | Planned |
| Event Boss | 1 (The Incomplete Maestro) | Planned |
| Weapons | 23 | Planned |
| Custom Debuffs | 3 (Harmonic Sear, Harmonic Overload, Chromatic Saturation) | Planned |
| Custom Rarity | 1 (PolyphonicRarity) | Planned |
| Event Summon Items | 2 (Rehearsal Score, Incomplete Score) | Planned |
| Midjourney Prompts | 52 (23 weapons + 11 enemies + 11 materials + 5 skies + 2 items) | Complete |
| **Total New Content Pieces** | **~60** | Planned |

---

*This document represents Phase #1 of the Implementation Phases system. Future phases will expand on the Polyphonic Line (armor sets, accessories, NPCs) and introduce additional parallel content systems.*
