---
applyTo: "Common/Systems/VFX/Bloom/**,Common/Systems/VFX/Screen/**,Common/Systems/VFX/Effects/**,Common/Systems/VFX/Weapon/**"
---

# Lighting & Atmosphere Conventions

## Dynamic Lighting

### Lighting.AddLight Requirements

Every glowing projectile, weapon swing, and particle burst should call `Lighting.AddLight()`:
```csharp
// Scale light intensity by visual brightness — don't over-light
Lighting.AddLight(position, colorVector * intensity);
```

- Light radius should match the visual glow size
- Animate light intensity to match VFX pulses (don't use static values)
- Boss attacks should use strong lighting; ambient effects should be subtle

### Pulsing Patterns

Use sinusoidal pulsing for living, breathing light:
```csharp
float pulse = 0.7f + 0.3f * MathF.Sin(Main.GlobalTimeWrappedHourly * speed);
```

- Idle glow: slow pulse (speed 2-4)
- Attack charge: accelerating pulse
- Impact flash: sharp spike then decay

## Bloom & Glow Stacking

Multi-scale bloom creates richer glow than a single bloom layer:

| Layer | Scale | Opacity | Purpose |
|-------|-------|---------|---------|
| Core | 0.3-0.5x | 80-100% | Hot bright center |
| Inner | 1.0x | 40-60% | Main glow body |
| Outer | 2.0-3.0x | 15-30% | Soft ambient spread |

Always draw bloom layers with `BlendState.Additive`. Never draw bloom with `AlphaBlend` — it looks flat.

## Aura Rendering

For weapon/player auras:
- Draw BEHIND the entity (use `PreDraw` or appropriate draw layer)
- Rotate slowly for visual interest
- Use noise texture masking for organic edges (not hard circles)
- Pulse scale slightly (±5-10%) for breathing effect

## Screen Tinting

- Use sparingly — only for boss phases, ultimate abilities, or environmental effects
- Always lerp in/out gradually (never snap)
- Duration: 0.3-0.5s for impacts, 2-5s for phase transitions
- Maximum opacity: 30% for combat tints, 60% for cinematic moments
- Per-theme tint colors must match theme palette

## God Rays & Light Rays

- Reserve for dramatic moments (boss spawns, phase transitions, weapon ultimates)
- Emanate from a logical light source (weapon core, boss eye, explosion center)
- Fade within 1-2 seconds — they lose impact if persistent
- Combine with screen shake for maximum drama

## Fog & Mist

- Use for boss arenas and atmospheric weapon effects
- Scroll slowly using UV offset (not particle systems — too expensive)
- Layer 2-3 fog planes at different scroll speeds for parallax depth
- Keep opacity low (10-25%) to avoid obscuring gameplay

## Performance Rules

- Maximum 3 active bloom layers per entity at any time
- God rays: maximum 1 active effect at a time (they're expensive)
- Screen tint: maximum 1 active tint (blend if transitioning between two)
- Fog: maximum 2 scrolling layers per screen region
- All atmospheric effects must have graceful fallback when many entities are active
