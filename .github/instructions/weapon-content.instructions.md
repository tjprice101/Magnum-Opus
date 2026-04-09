---
applyTo: "Content/**/Weapons/**,Content/**/Melee/**,Content/**/Magic/**,Content/**/Ranged/**,Content/**/Summoner/**"
---

# Weapon Content Conventions — MagnumOpus

## File Structure (SandboxLastPrism Pattern)

Every weapon follows this folder organization:

```
Content/<ThemeName>/<Category>/<WeaponName>/
├── <WeaponName>.cs           — Main ModItem class
├── <WeaponName>.png          — Item sprite (same folder as .cs)
├── <WeaponName>VFX.cs        — VFX static helper class (optional)
├── <WeaponName>Swing.cs      — Swing projectile (melee)
├── <WeaponName>Projectile.cs — Projectile (ranged/magic/summoner)
├── Dusts/
│   ├── <DustName>.cs         — Custom ModDust types
│   └── Textures/             — Dust sprite PNGs
└── Systems/                  — Weapon-specific systems
```

**VFX textures** (for shaders/trails/bloom): `Assets/<ThemeName>/<WeaponName>/`
**Custom shaders**: `Effects/<ThemeName>/<WeaponName>/`

## Required: ModifyTooltips

Every weapon item MUST implement `ModifyTooltips`. The localization file has empty `Tooltip: ""` placeholders — actual tooltips are defined in code:

```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    // Effect lines — describe what the item does
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Fires spiraling projectiles that converge on targets"));
    tooltips.Add(new TooltipLine(Mod, "Effect2", "Every 5th hit releases a harmonic pulse"));
    
    // Lore line — themed flavor text with colored text
    tooltips.Add(new TooltipLine(Mod, "Lore", "'The moonlight whispers of tides long forgotten'")
    {
        OverrideColor = new Color(140, 100, 200) // Theme-appropriate color
    });
}
```

### Theme Lore Colors

| Theme | Lore Color | Lore Themes |
|-------|-----------|-------------|
| Moonlight Sonata | `new Color(140, 100, 200)` | Moonlight, tides, silver, stillness, sorrow |
| Eroica | `new Color(200, 50, 50)` | Heroism, sacrifice, glory, triumph |
| La Campanella | `new Color(255, 140, 40)` | Fire, bells, passion, intensity |
| Swan Lake | `new Color(240, 240, 255)` | Grace, feathers, elegance, tragedy |
| Enigma Variations | `new Color(140, 60, 200)` | Mystery, dread, arcane secrets |
| Fate | `new Color(180, 40, 80)` | Destiny, cosmos, inevitability |
| Clair de Lune | `new Color(150, 200, 255)` | Shattered time, blazing clocks, temporal destruction |
| Dies Irae | `new Color(200, 50, 30)` | Hellfire retribution, divine judgment, heavenly banishment |
| Nachtmusik | `new Color(100, 120, 200)` | Golden twinkling, starlit melodies, sweet songs |
| Ode to Joy | `new Color(255, 200, 50)` | Chromatic glass roses, prismatic radiance, eternal symphony |

### Lore Guidelines
- Use sentence case, not ALL CAPS
- Wrap lore text in single quotes
- Match the theme's emotional core — never cross-pollinate themes
- Moonlight Sonata lore should NEVER reference cosmos/stars/space

## Tooltip Style
- Follow vanilla Terraria's clean, informative style
- Avoid capitalized emphasis (`MASSIVE` → `massive`)
- Effect lines describe mechanics. Lore lines are poetic.

## VFX Requirements

Every weapon should implement its own unique VFX directly in its files. There are no global VFX systems that auto-apply effects. Each weapon is responsible for:

1. **Trail/swing rendering** — In the projectile's `PreDraw`/`PostDraw` or via the VFX helper class
2. **Particle spawning** — In `AI()`, `OnHitNPC()`, or equivalent hooks
3. **Bloom/glow** — Multi-scale additive stacking in draw calls
4. **Impact effects** — In `OnHitNPC()` or `ModifyHitNPC()`

## Mechanic Diversity Requirements

### Uniqueness Mandate

Within a single theme, no two weapons of the same class may share the same attack pattern, trail style, impact effect, or special mechanic. Each weapon must be mechanically distinct:

| Dimension | Requirement |
|-----------|-------------|
| Attack pattern | Different swing arc, projectile behavior, or firing mode |
| Trail/swing VFX | Different trail type, color gradient, or shader |
| Impact effect | Different particle burst, screen response, or on-hit behavior |
| Special mechanic | Different resource system, combo escalation, or mode switch |

### Combo & Mechanic Types

Weapons should draw from diverse mechanic archetypes:
- **Escalating combos** — Each hit in a chain gets more powerful/visually intense
- **Charge mechanics** — Hold to charge, release for scaled effect
- **Resource buildup** — Accumulate energy/notes/stacks for a special release
- **Mode switching** — Toggle between two distinct attack modes
- **Musical triggers** — Every Nth hit triggers a harmonic burst, chord effect, or rhythm bonus
- **Environmental interaction** — Attacks leave lingering zones, marks, or summons

### Foundation Weapons Reference

Before implementing any weapon, review `Content/FoundationWeapons/` for reusable rendering patterns:
- AttackAnimationFoundation — CurveSegment-driven animated attacks
- SwordSmearFoundation — Multi-layer smear rendering
- RibbonFoundation — 10 trail rendering modes
- ImpactFoundation — Multi-layer impact bursts
- ExplosionParticlesFoundation — Physics-driven explosion particles

### Agent Routing

For weapon design questions, invoke `@weapon-mechanic` for mechanics and `@creative-director` for concept ideation. For VFX implementation, invoke `@vfx-composer` which orchestrates specialized sub-agents.

## Uniqueness Mandate

Within a single theme, weapons of the same class MUST be meaningfully different:
- Different trail types (ribbon vs afterimage vs primitive vs none)
- Different particle effects
- Different special mechanics
- Different shader approaches

If implementing a weapon that overlaps with an existing one in the same theme+class, restructure its VFX to be distinct.
