# Nachtmusik Arsenal Style Guide — LOCKED
<!-- Generated via 5-round interactive design dialog, March 2026 -->
<!-- Status: LOCKED — do not modify without repeating the design dialog -->

> *"A Little Night Music — every weapon is a serenade to the infinite dark, every star a note in the cosmos's final symphony."*

## Key

| Marker | Meaning |
|--------|---------|
| 🔒 | **LOCKED** — non-negotiable rule from the design dialog; must not be violated |
| ⭐ | **RECOMMENDED** — strong design guidance; only deviate consciously |
| 🔓 | **FREE** — no constraint; designer's choice |

---

## Provenance

Derived from a 5-round interactive design dialog plus direct code analysis of:

- [Content/Nachtmusik/NachtmusikPalette.cs](../Content/Nachtmusik/NachtmusikPalette.cs)
- [Content/Nachtmusik/NachtmusikVFXLibrary.cs](../Content/Nachtmusik/NachtmusikVFXLibrary.cs)
- [Content/Nachtmusik/NachtmusikShaderManager.cs](../Content/Nachtmusik/NachtmusikShaderManager.cs)
- [Effects/Nachtmusik/NachtmusikStarTrail.fx](../Effects/Nachtmusik/NachtmusikStarTrail.fx)
- [Effects/Nachtmusik/NachtmusikSerenade.fx](../Effects/Nachtmusik/NachtmusikSerenade.fx)
- [Content/Nachtmusik/Weapons/TwilightSeverance/TwilightSeverance.cs](../Content/Nachtmusik/Weapons/TwilightSeverance/TwilightSeverance.cs)
- [Content/Nachtmusik/Weapons/GalacticOverture/GalacticOverture.cs](../Content/Nachtmusik/Weapons/GalacticOverture/GalacticOverture.cs)
- [Documentation/Resonance Weapons Planning/09_Nachtmusik.md](Resonance%20Weapons%20Planning/09_Nachtmusik.md)

Arsenal scope: 11 weapons — 3 Melee, 3 Ranged, 2 Magic, 3 Summon.

---

## 1. Emotional Core

### 🔒 Primary Feeling: Profound Cosmic Solemnity

The Nachtmusik arsenal is **not** playful or whimsical. It is the quiet reverence of standing beneath an infinite night sky — awed, small, present. Effects should make the player feel that combat is taking place inside something vast and ancient. Avoid flourishes that feel frivolous, hurried, or decorative.

### 🔒 Solemnity Scale Rule

Solemnity is earned by magnitude. It must not be forced into every frame:

| State | Solemnity Level | What That Means in Practice |
|-------|----------------|------------------------------|
| **Ambient / Idle** | Subtle | Barely-there starlit shimmer. A hint of glow. Solemnity implied, not stated. |
| **Active Combat** (swings, shots, casts) | Dignified | Graceful trails, visible-but-restrained bloom, music notes on hit. Weight without bombast. |
| **Finishers / Ultimates / Kills** | Awe-inspiring | The full palette fires. Void bloom expands. Stars scatter. The night sky makes itself known. |

### 🔒 The Night Sky Metaphor (Identity Anchor)

Every visual effect — every particle system, shader choice, bloom layer, trail arc — must pass this single test:

> **"Does this effect make the player feel like they are looking at, or wielding, the night sky itself?"**

If an effect is technically impressive but doesn't evoke a dark vast canopy of stars — silence, cosmic depth, the weight of space — it does not belong in Nachtmusik. This test supersedes all other guidance. When in doubt, ask this question first.

---

## 2. The Locked Palette

### 🔒 The Six-Step Core Scale

These six colors form the backbone of every Nachtmusik effect. All six must be present somewhere across each weapon's full lifecycle (subtle dark layers to bright finisher flashes).

```
[0] NachtmusikPalette.MidnightBlue      (15, 15, 45)      Pianissimo — Shadow / dark outer glow
[1] NachtmusikPalette.DeepBlue          (30, 50, 120)     Piano — Dark nocturnal body
[2] NachtmusikPalette.StarlitBlue       (80, 120, 200)    Mezzo — Primary trail / swing color (HEARTBEAT)
[3] NachtmusikPalette.StarWhite         (200, 210, 240)   Forte — Star sparkle / bright accent
[4] NachtmusikPalette.MoonlitSilver     (230, 235, 245)   Fortissimo — Bloom mid-layer
[5] NachtmusikPalette.TwinklingWhite    (248, 250, 255)   Sforzando — Core flash / finisher peak
```

### 🔒 Gold Accent Rule

- `RadianceGold` (255, 215, 0) and `StarGold` (255, 230, 150) are permitted as **sparkle tip-highlights ONLY**.
- Gold is **always paired with cool indigo/silver** — it never appears standalone.
- Gold's primary application: the warm bright tip of a **music note particle** (see §5), distinguishing notes from standard star sparkles.
- Gold must **never** be dominant in trails, auras, bloom bodies, or ambient effects.
- If any effect looks warm or fire-adjacent, it violates this rule.

### ⭐ Extended Palette Usage

The full extended palette in `NachtmusikPalette.cs` — `CosmicPurple`, `Violet`, `NebulaPink`, `DuskViolet`, `ConstellationBlue`, `NightSkyBlue`, etc. — is available for weapon-specific variation. All extended colors remain within the cool (blue/indigo/purple/silver) range.

### 🔓 Adjacent Theme Borrowing

Nachtmusik may borrow visual elements from adjacent themes (dark smoke, purple mist, etc.) at its creative borders. Nothing is explicitly forbidden. However, any borrowed element must pass the Night Sky Metaphor test: a wisp of dark smoke is only acceptable if it reads as cosmic void, not as fire.

---

## 3. Motion Language — FLOW

### 🔒 The Single Word is FLOW

Every trail, particle arc, bloom expansion, projectile path, and swing arc must **bend like a melody**. Even the fastest weapon must leave a trail that curves and flows.

> **Speed does not break the flow rule. A fast arc moves through flowing space and still leaves a curved ribbon. Fast ≠ sharp.**

### 🔒 Flow Rules by Element

| Element | Rule |
|---------|------|
| **Trail geometry** | Curved arcs always. `CalamityStyleTrailRenderer.TrailStyle.Cosmic` is the canonical style (see `TwilightSeveranceSwing.cs`). No axis-aligned rectangular strips. |
| **Particle motion** | Drift, arc, spiral, float. Low gravity influence (0.0–0.15 for most effects). No abrupt stop-start behavior. |
| **Bloom expansion** | Soft easing expansion. The signature Nachtmusik bloom is a ring that expands outward and fades at its edge — never a hard pop or flat circle. |
| **Projectile paths** | Curved preferred. Even "straight-firing" projectiles should have gentle drift or sway. |
| **Swing arc geometry** | Follow melody curves: slow windup → fast acceleration → gentle follow-through overshoot. The swing itself is a musical phrase. |

---

## 4. Musical Motifs — Equal Coexistence

### 🔒 The Three Motifs

Stars, music notes, and constellation geometry are all valid Nachtmusik motifs. None has global hierarchy over the others.

| Motif | Canonical System | When It Appears |
|-------|-----------------|-----------------|
| **Twinkling Stars** | `NachtmusikVFXLibrary.SpawnTwinklingStars()` / `NachtmusikStarFlow` shader | Active attacks only (trails, impacts). Never idle. |
| **Music Notes** | `NachtmusikVFXLibrary.SpawnMusicNotes()` | On any hit or impact. Upward drift. |
| **Constellation Geometry** | `NachtmusikVFXLibrary.SpawnConstellationCircle()` / line primitives | Finishers, ultimates, charge-activation specials. |

### 🔒 Three-Motif Coverage Rule

**No weapon may lack all three.** Every weapon must incorporate all three motifs somewhere in its full lifecycle — trail, impact, or special mechanic. The question is emphasis, not exclusion.

### 🔒 Per-Weapon Motif Emphasis

Each weapon **leans into one motif** as its primary visual signature. The other two appear as support. This per-weapon emphasis is the primary axis of uniqueness within a weapon class.

When designing a new Nachtmusik weapon, identifying its primary motif is the first creative decision to lock.

### 🔒 Twinkling: Active Combat Only

The procedural twinkling from `NachtmusikStarTrail.fx` — the rhythmic bright pinpoints along the trail — is **active-attacks-only**. Weapons must NOT show twinkling sparkles in idle or ambient hold state. The night sky is still when you are still.

---

## 5. The Music Note — Canonical Properties

Music notes are the most visible per-hit indicator across the entire arsenal. Their properties are locked:

| Property | Value | Notes |
|----------|-------|-------|
| **Hue band** | HSL 0.55–0.75 (blue → indigo → violet) | Matches `NachtmusikVFXLibrary.HueMin`/`HueMax` |
| **Gold tip** | Warm bright center (additive blend) | This gold tip is what distinguishes notes from star sparkles |
| **Spawn location** | At or near hit point, small random offset | Never far from the impact |
| **Movement** | Upward drift (`vel.Y` = −1.5f to −3f) + gentle X sway | Never downward, never stationary |
| **Scale** | 0.7f–1.0f minimum | Must always be clearly visible — never tiny |
| **Lifetime** | 25–40 frames | Long enough to register; short enough to stay clean |
| **Spawn call** | `NachtmusikVFXLibrary.SpawnMusicNotes(pos, count, spread, minScale, maxScale, lifetime)` | |

---

## 6. The Three Mandatory Visual Markers

These three elements **must appear** in every Nachtmusik weapon. A weapon missing any one of them is not identifiably Nachtmusik regardless of palette.

### 🔒 Marker 1 — NachtmusikStarFlow Shader on the Trail

Every Nachtmusik weapon's primary trail must use the `NachtmusikStarFlow` technique from `Effects/Nachtmusik/NachtmusikStarTrail.fx`, or a weapon-specific shader that is thematically compatible (deep indigo → silver gradient with procedural twinkling constellation points).

The canonical entry point is `NachtmusikShaderManager.ApplyStarTrail(time, primary, secondary)` with:
- Primary: `NachtmusikPalette.DeepBlue` or `MidnightBlue`
- Secondary: `NachtmusikPalette.StarlitBlue` or `StarWhite`

Weapon-specific shaders (`ExecutionDecree`, `CrescendoRise`, `DimensionalRift`, etc.) are valid as the primary trail shader. They must still express the cosmic drift + twinkling star-point language.

### 🔒 Marker 2 — SpawnTwinklingStars on Every Impact

Every hit, every impact, must call:
```csharp
NachtmusikVFXLibrary.SpawnTwinklingStars(pos, count, radius);
```
Minimum: 1–2 stars at scale 1. Scale count and radius to combo step or weapon power. This is the tactile confirmation that the night sky has been disturbed.

### 🔒 Marker 3 — Night Void Dark Layer in Every Bloom Stack

Every bloom stack must include at least one **dark base layer** using `NachtmusikPalette.CosmicVoid` (10, 10, 30) or `MidnightBlue` (15, 15, 45) at low opacity (0.3–0.5) in additive blend.

This dark-to-bright gradient is what makes Nachtmusik bloom feel like it is expanding from within the darkness of space rather than simply flashing light. A bloom stack that begins with a bright color feels wrong — it must emerge from void.

Reference implementations:
- `NachtmusikPalette.DrawItemBloom()` — item glow in world/inventory
- `NachtmusikVFXLibrary.DrawNachtmusikBloomStack()` — runtime bloom layers

---

## 7. The Nachtmusik Signature Moment

### 🔒 The Single Image

> **A deep void bloom expands — dark at its origin, star-silver at its edge, then fades to silence. The weight of the night made visible.**

This is the image that must be recognizable and consistent across all 11 weapons. It occurs at:
- Every kill
- Every finisher / maximum-charge activation
- Every Phase 3 / ultimate special

**Technical implementation:**
```csharp
// Signature moment: void bloom → star-silver → silence
NachtmusikVFXLibrary.DrawBloom(pos, scale: 0.8f-1.2f);         // Night Void → Stellar White
NachtmusikVFXLibrary.SpawnStarBurst(pos, count, particleScale); // Star scatter at rim
NachtmusikVFXLibrary.SpawnTwinklingStars(pos, 3-5, radius);     // Twinkling confirmation
NachtmusikVFXLibrary.SpawnMusicNotes(pos, 3-5, spread);         // Gold-tipped notes drift up
```

**What this is NOT:** An explosion. Not a burst of light. Not a flash. It is a slow expansion — like a shockwave of meaning moving through space — that dims back to darkness. The silence after is part of the effect.

---

## 8. Melee Arsenal — Weight Axis

### 🔒 Primary Differentiation: Weight

Three melee weapons share the cosmic trail style. Their single differentiating axis is **Weight** — how much the weapon impresses itself onto space:

| Weapon | Weight Identity | Trail Width / Density | Swing Arc | Shader |
|--------|----------------|----------------------|-----------|--------|
| **Nocturnal Executioner** | **Crushing** — the finality of midnight | Wide, dense. High particle count. Nebula cloud distortion. | Wide sweeping arcs. 3-layer additive rendering. | `ExecutionDecree` (void-rip slash, `distortionAmt: 0.12f`) |
| **Midnight's Crescendo** | **Building Tension** — momentum accumulating | Trail width and brightness **scale with crescendo stacks** (0.6× at 0 → 1.8× at max) | Medium arcs that visibly widen with stacks | `CrescendoRise` (intensity-building trail) |
| **Twilight Severance** | **Cutting Silence** — absence of light as weapon | Ultra-thin. Near-zero particle density. Speed prioritized. | Extremely tight arcs. ThinSlash geometry. | `DimensionalRift` (dimensional rift tear, `distortionAmt: 0.04f`) |

### Melee Motif Emphasis

| Weapon | Primary Motif | Notes |
|--------|--------------|-------|
| Nocturnal Executioner | Constellation geometry | Execution fan creates star-convergence geometry; cosmic presence aura forms a constellation circle |
| Midnight's Crescendo | Twinkling stars | Stars multiply and intensify with stacks — the crescendo is visible in star density |
| Twilight Severance | Music notes | Dimension Sever releases a cascade of notes; the X-slash itself is the negative-space geometry |

### ⭐ Melee-Specific Guidance

- All three must use `CalamityStyleTrailRenderer.TrailStyle.Cosmic` (or equivalent flowing arc geometry — no rigid rectangles).
- All three use smear arcs with cosmic noise (`CosmicNebula` or `TileableFBMNoise`) for distortion — no crisp geometric smears.
- The **Executioner has the most**, the **Severance has the least**. Crescendo sits between and moves toward Executioner's density as stacks build.

---

## 9. Ranged Arsenal — Interaction Axis

### 🔒 Primary Differentiation: Relationship with Space

Three ranged weapons all fire "star projectiles." Their differentiating axis is **Interaction** — how the projectile relates to the space it moves through:

| Weapon | Interaction Identity | Primary Trail Visual | On-Impact Visual |
|--------|---------------------|---------------------|-----------------|
| **Constellation Piercer** | **Pierces / Connects** — the shot draws a geometric line through space; star points appear at regular intervals connecting source to target | Rigid geometric constellation line; star-point particles at intervals | Constellation geometry radiates from impact; `StarChainBeam` shader |
| **Nebula's Whisper** | **Residue / Lingers** — shot disperses into an expanding nebula cloud that persists in space after impact; fields accumulate | Soft expanding smoke-cloud particles; no hard edge; `NebulaScatter` shader | Nebula residue field lingers 2–4 seconds; Whisper Storm convergence pulls fields inward |
| **Serenade of Distant Stars** | **Seeks / Follows** — projectiles are homing stars that arc toward targets; the trail emphasizes the devotion and journey of each individual star | Graceful curved ribbon arc; `StarHomingTrail` shader; no geometric points | Warm starlight burst on arrival; `StarHomingGlow` bloom |

### ⭐ Ranged Guidance

- **Piercer**: Visual language is geometric and spatial. Think of it as drawing constellation lines across the battlefield.
- **Whisper**: Visual language is atmospheric and accumulative. The longer the fight, the more nebula residue fills the area.
- **Serenade**: Visual language is sentimental and individual. Each star is precious. Each arc has a story.

### Ranged Motif Emphasis

| Weapon | Primary Motif |
|--------|--------------|
| Constellation Piercer | Constellation geometry |
| Nebula's Whisper | Twinkling stars (embedded in the nebula cloud) |
| Serenade of Distant Stars | Music notes (notes trail each homing star) |

---

## 10. Magic Arsenal — Scale Axis

### 🔒 Primary Differentiation: Physical Scale

Two magic weapons share the Nachtmusik palette. Their differentiating axis is **Scale** — the size of space they occupy and the weight of their presence:

| Weapon | Scale Identity | Visual Language |
|--------|---------------|----------------|
| **Starweaver's Grimoire** | **Low-weight, intricate thread/web** — precise, delicate, complex | Thin star-thread ribbons connecting orb nodes; fine constellation weave geometry; the complexity lives in the detail, not the size. Fits comfortably in a tight hallway. |
| **Requiem of the Cosmos** | **Massive, crushing, singularity-scale** — vast, austere, devastating | Enormous bloom radius; `CosmicVortex` noise in orb interior; Stellar Collapse creates a singularity event; at full charge the bloom occupies a significant portion of the screen. Reshapes the player's sense of scale. |

### Magic Motif Emphasis

| Weapon | Primary Motif |
|--------|--------------|
| Starweaver's Grimoire | Constellation geometry (the weave itself is the constellation pattern) |
| Requiem of the Cosmos | Twinkling stars (embedded in the vast cosmic body; star-scatter on Stellar Collapse) |

Both weapons include music notes on impact per the three-motif coverage rule.

---

## 11. Summoner Arsenal — Entity Grammar

### 🔒 The Shared Entity Visual Class

All three Nachtmusik summoner weapons deploy entities that share the same visual grammar. They are **clearly cosmic beings, visible but calm in their idle state**. Not theatrical. Not dramatic at rest.

#### Idle State Rules (all three summoners)

| Element | Rule |
|---------|------|
| Body glow | Soft `StarlitBlue` or `DeepBlue` halo around the entity body — clearly a cosmic being |
| Ambient stars | 1 twinkling star every 2–3 seconds near the body (ambient; calm) |
| Music notes | None while idle |
| Constellation outlines | None while idle |
| Movement | Gentle float, slow sway, or leisurely orbit. Never fast or agitated at rest. |

### 🔒 Attack Behavior Is the Differentiator

The visual distinction between the three summoners comes entirely from their **attack patterns**, not their idle appearance:

| Weapon | Attack Pattern Visual Identity |
|--------|-------------------------------|
| **Celestial Chorus Baton** | Choral singing beams: multiple entity "singers" converge harmonic thin laser beams on the same target simultaneously. Musical, synchronized, choral. |
| **Galactic Overture** | The grandest individual scale of the three: the Celestial Muse carries an evolving spiral galaxy disc aesthetic; attacks fire cosmic jet beams. The most visually distinct of the three entities. |
| **Conductor of Constellations** | 4-instrument variety: Strings (thin sweeping beams), Percussion (AoE slam ring), Winds (wide wave arc), Brass (heavy burst). The Orchestral Sync is the signature — all 4 fire simultaneously. |

### ⭐ Summoner Guidance

The three summoners are treated as **one entity class with differentiated behavior** — not three distinct visual styles. A player who encounters all three should think "these are all Nachtmusik summons" from their appearance, and only distinguish them by how they attack.

The Galactic Overture's galaxy disc is an intentional exception — it carries a more distinct visual because it is designed to feel like the "grandest opening" (an overture). Even then, its idle glow and ambient behavior still follow the shared entity grammar.

---

## 12. Impact Structure — Designer's Choice

### 🔓 No Shared Impact Template

Each weapon designs its impacts from scratch. There is no mandatory sequence or layer structure shared across the arsenal. The only constraints on impacts are the three mandatory markers:

1. `SpawnTwinklingStars` — must be present
2. Music notes — must be present somewhere in the hit lifecycle
3. Night Void dark layer in any bloom rendered at the impact — must be present

Everything else (flash shape, ring count, particle type, screen shake decision, color emphasis, layering order) is per-weapon creative freedom.

---

## 13. What Stays Constant Across All 11 Weapons

| Element | 🔒 Rule |
|---------|---------|
| Palette dominant | Cool blue-silver range. Warm gold only as sparkle tip-highlights on music notes. |
| Trail shader | `NachtmusikStarFlow` technique or a compatible weapon-specific variant. Must express cosmic drift + twinkling. |
| Impact minimum | `SpawnTwinklingStars` + 1+ music note + Night Void dark bloom layer. |
| Motion metaphor | FLOW curves. No rigid straight-line geometry in trails or swing arcs. |
| Twinkling activation | Active attacks only. Never ambient / idle state. |
| Music note hue | HSL 0.55–0.75 (blue-indigo-violet), gold-tipped, scale 0.7+, upward drift. |
| Three-motif coverage | Every weapon includes all three motifs (stars, notes, constellations) across its lifecycle. |
| Identity test | The Night Sky Metaphor. Every effect must evoke the night sky. |
| Bloom structure | Must emerge from a dark void base layer (not bright-from-birth). |
| Signature moment | Deep void bloom → star-silver rim → silence. Present at kills and finishers. |

---

## 14. What Varies Freely

| Element | 🔓 Freedom |
|---------|-----------|
| Impact structure | Completely free per weapon — no shared template required. |
| Particle types | Any particle from the VFX library. Adjacent theme borrowing is permitted if it passes the Night Sky test. |
| Trail secondary color | Any sub-range of the extended Nachtmusik palette. |
| Weapon-specific shaders | Encouraged. Each weapon having its own shader that wraps the Star Trail language is ideal. |
| Class-level grammar | No per-class VFX requirement. Each weapon is its own unit. |
| Sound design | No constraint from this guide. |
| Uniqueness mandate | Aspirational, not mandatory. Avoid exact duplication, but shared systems are permitted. |
| Combo step escalation | Escalation intensity, bloom scale, star count per step — all free. |

---

## 15. Lore & Tooltip Guidelines

### 🔒 Lore Color

All Nachtmusik lore lines use:
```csharp
OverrideColor = new Color(100, 120, 200) // Starlight Indigo
```

### ⭐ Lore Vocabulary

Lore lines should draw from the night sky domain. Words that belong:

> night, stars, twilight, constellation, cosmic, serenade, midnight, nebula, stellar, nocturnal, celestial, silence, void, darkness, infinite, vast, distant, echo, sorrow

Words that do NOT belong in Nachtmusik lore:
- Space, cosmos, galaxy (too sci-fi; evoke industrial space fantasy rather than nocturnal serenade)
- Fire, heat, warmth, sunrise, dawn (temporal wrong-direction; Nachtmusik is night, not transition)
- Clanging, bells, percussion sounds (La Campanella's domain)
- Silver tidal undulation, moonbeams (Moonlight Sonata's specific language)

### ⭐ Lore Tone

Lore lines should feel like whispered poetry under an open sky. Not triumphant. Not fearful. Solemn, observational, slightly mournful at the weight of something infinite.

**Examples from existing weapons:**
- *"Between light and dark, the blade finds every truth."* (Twilight Severance)
- *"At midnight, the executioner does not knock. The stars simply go dark."* (Nocturnal Executioner)
- *"All the stars are instruments. The night sky is the concert hall. You are the conductor."* (Conductor of Constellations)

---

## 16. Quick-Check for New Nachtmusik Weapons

Before approving any new weapon, verify all boxes:

```
MANDATORY (🔒)
[ ] Trail uses NachtmusikStarFlow or a compatible cosmic-drift shader
[ ] Every impact calls SpawnTwinklingStars (min 1–2 stars)
[ ] Every bloom stack includes a Night Void dark base layer
[ ] All three motifs appear somewhere in the weapon lifecycle
[ ] Motion geometry uses flowing curves (no rigid straight trails)
[ ] Twinkling only fires during active attacks, not idle
[ ] Music notes: gold-tipped, hue 0.55–0.75, scale 0.7+, upward drift
[ ] Palette stays cool (no warm tones except gold-tip highlights)
[ ] Night Sky Metaphor test passed: "does this evoke the night sky?"
[ ] Signature moment (void bloom → silver → silence) present at kills/finishers

UNIQUENESS CHECK (⭐)
[ ] Does this weapon have a primary motif emphasis that differs from same-class peers?
[ ] Is the weight/interaction/scale identity clearly distinct from same-class peers?
[ ] Would a player be able to identify this specific weapon from its VFX alone?

LORE CHECK (🔒)
[ ] Lore color: new Color(100, 120, 200)
[ ] Lore uses night-sky vocabulary (no space/sci-fi terminology)
[ ] Tone is solemn, observational, poetic
```

---

*Last updated: March 2026. Repeat the 5-round design dialog before making changes to any 🔒 locked rule.*
