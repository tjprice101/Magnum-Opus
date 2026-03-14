Design multi-layered hit effects, crit flashes, death bursts, screen shake, and hit-stop — from the initial flash to the final particle settling.

Context: $ARGUMENTS

---

## When to Use
- Designing weapon hit effects (melee, ranged, magic, summon)
- Creating crit/super-crit escalation visuals
- Designing enemy death burst effects
- Adding screen shake and hit-stop to attacks
- Creating boss attack impact effects on the player

## Step 1: Interactive Context Dialog

Ask the user:
1. **What is hitting?** (Melee swing, projectile, beam endpoint, summon attack, boss attack, environmental hazard?)
2. **Theme/score?** (Constrains color palette and emotional tone)
3. **How should it FEEL?** (Heavy and earth-shaking? Quick and precise? Explosive and chaotic? Elegant and clean?)
4. **Damage tier:** Minor tick, standard hit, heavy, critical, killing blow?
5. **Special on-hit mechanic?** (Apply debuff, chain damage, mark enemy, spawn secondary projectile?)

## Step 2: Search Reference Mods

Search for similar impact patterns:
- **Calamity:** Search `Particles/` for CritSpark, CircularSmear, SemiCircularSmear, PulseRing
- **WotG:** Search for divine impact effects, screen-level hit responses
- **MagnumOpus:** Check `Content/FoundationWeapons/ImpactFoundation/` (3 shader modes) and `Content/FoundationWeapons/ExplosionParticlesFoundation/` (CPU physics)

Read 2-3 implementations. Note the layering pattern.

## Step 3: Design Impact Layer Stack

Every quality impact has multiple layers. Design which this impact uses:

| Layer | Description | When to Include |
|-------|------------|-----------------|
| 1. **Flash** | Brief bright overlay at hit point (1-3 frames) | Almost always |
| 2. **Ring/wave** | Expanding circle or shockwave from hit point | Medium+ hits |
| 3. **Particle burst** | Themed particles scattered from impact | Almost always |
| 4. **Screen shake** | Camera offset for weight | Medium+ hits |
| 5. **Hit-stop** | Brief freeze (1-3 frames) for heavy feel | Heavy hits, crits |
| 6. **Dust/debris** | Small physical particles (gravity-affected) | Grounded hits |
| 7. **Persistent mark** | Lingering glow/symbol at hit location | Special hits |
| 8. **Screen effect** | Flash, vignette pulse, chromatic aberration | Crits, kills |

### Screen Shake Vocabulary

| Feel | Intensity | Duration | Pattern |
|------|-----------|----------|---------|
| Light tap | 2-3px | 3 frames | Single offset + return |
| Solid hit | 4-6px | 5-8 frames | Decaying oscillation |
| Heavy slam | 8-12px | 10-15 frames | Sharp spike + rumble decay |
| Earth-shaking | 12-16px | 15-20 frames | Multi-axis rumble |
| Boss death | 16-20px | 30-45 frames | Long dramatic rumble |

```csharp
// Screen shake implementation pattern
float shakeIntensity = baseIntensity * (1f - progress); // Linear decay
// Or for rumble: add high-frequency oscillation
float rumble = MathF.Sin(progress * 40f) * intensity * (1f - progress);
```

### Hit-Stop / Freeze Frame

```csharp
// Brief freeze for impact weight (1-3 frames for normal, 3-5 for crits)
if (hitStopTimer > 0)
{
    hitStopTimer--;
    return; // Skip all update logic — everything freezes
}
```

Use sparingly — only for heavy hits and crits. Extended freeze feels like lag.

### Crit Flash Techniques

- **White flash:** Draw white overlay at hit position, 60-70% opacity, fade over 2-3 frames
- **Chromatic aberration pulse:** Split RGB channels for 3-5 frames, 4-6px offset
- **Zoom punch:** Brief 2-5% camera zoom toward hit point, snap back over 5 frames
- **Color inversion:** Invert colors in small radius for 1-2 frames (extreme — use for boss crits only)

## Step 4: Design Damage Tier Escalation

| Tier | Layers | Intensity | Duration |
|------|--------|-----------|----------|
| **Minor** | Flash + 3-5 particles | Low | 5-10 frames |
| **Standard** | Flash + ring + 8-12 particles + light shake | Medium | 10-15 frames |
| **Heavy** | Flash + ring + 15-20 particles + shake + dust | High | 15-20 frames |
| **Critical** | All layers + hit-stop + screen flash + chromatic | Maximum | 20-30 frames |
| **Kill** | All layers + death burst + persistent mark + screen pulse | Spectacular | 30-60 frames |

Present tier escalation plan to user for approval.

## Step 5: Design Themed Death Burst

Enemy death effects should reflect the weapon's theme:

| Theme | Death Burst |
|-------|------------|
| **La Campanella** | Ember explosion + smoke billow + bell chime visual ring |
| **Eroica** | Sakura petal scatter + golden light burst + ascending embers |
| **Swan Lake** | White feather drift + prismatic flash + elegant dissolve |
| **Moonlight Sonata** | Purple mist exhale + silver sparkle settle + moonbeam fade |
| **Enigma** | Void implosion + eerie green flame remnants + watching eye flash |
| **Fate** | Cosmic particle scatter + star field brief + chromatic shatter |
| **Clair de Lune** | Soft blue mist release + pearl sparkle + gentle dissipation |
| **Dies Irae** | Blood explosion + ember rain + ground scorch mark |
| **Nachtmusik** | Starlight fragment scatter + indigo mist + constellation brief |
| **Ode to Joy** | Golden light burst + warm amber particles + musical note cascade |

### Scale-Appropriate Deaths
- **Minor enemy:** 8-15 particles, no screen effects, 0.3s
- **Standard enemy:** 15-25 particles, brief flash, 0.5s
- **Mini-boss:** 25-40 particles, screen shake + flash, 0.8s
- **Boss:** 40-60+ particles, full screen response, 1-2s (see `/design-boss-phase` for boss death sequences)

## Step 6: Asset Check

Check existing assets for impact effects:
- `Assets/VFX Asset Library/ImpactEffects/` — 8 impact textures
- `Assets/Particles Asset Library/` — sparkles, explosions, lightning bursts, smoke
- `Assets/VFX Asset Library/GlowAndBloom/` — flash/flare sprites
- `Content/FoundationWeapons/ImpactFoundation/` — 3 shader types (ripple, damage zone, slash mark)
- `Content/FoundationWeapons/ExplosionParticlesFoundation/` — CPU physics burst

If missing: **STOP** and provide Midjourney prompt with details.

## Step 7: Final Impact Spec

Present complete specification:
- Layer stack with per-frame timing breakdown
- Particle types, counts, velocities, lifetimes, colors
- Screen shake pattern and intensity
- Hit-stop duration (if applicable)
- Screen effect details (if applicable)
- Death burst variant for the theme
- Color palette (from theme)
- Performance budget
- Which Foundation Weapons to reference for implementation

## Step 8: Implement

Write impact code directly into the weapon's projectile/swing class or VFX helper:
- On-hit effects in `OnHitNPC` / `ModifyHitNPC`
- Death burst in `OnKill` or via global NPC hook
- Screen effects via existing VFX systems in `Common/Systems/VFX/Effects/`
- Custom impact shaders if needed in `Effects/<Theme>/<Weapon>/`

Run `dotnet build` to verify compilation.
