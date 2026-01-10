# Custom Particle Textures

This folder contains the 3 core particle textures used throughout MagnumOpus:

## Texture Files

| File | Purpose | Usage |
|------|---------|-------|
| `EnergyFlare.png` | Intense, bright burst | Impacts, explosions, boss attacks, dramatic moments |
| `SoftGlow.png` | Subtle ambient glow | Trails, auras, ambient effects, soft lighting |
| `MusicNote.png` | Musical note | Thematic musical effects - perfect for this music-themed mod! |

## How They Work

All textures are **white/grayscale** and get tinted at runtime to any color:
- **Eroica theme**: Scarlet, Crimson, Gold
- **Moonlight Sonata**: Deep Purple, Violet, Lavender, Silver
- **Swan Lake**: Pure White, Icy Blue, Pale Cyan
- **Dies Irae**: Blood Red, Dark Crimson, Ember
- **Clair de Lune**: Soft Blue, Moonbeam, Pearl

## Code Examples

```csharp
// Eroica weapon hit
CustomParticles.EroicaImpactBurst(position, 8);

// Moonlight trail
CustomParticles.MoonlightFlare(position, 0.5f);

// Floating music notes
CustomParticles.EroicaMusicNotes(position, 5, 30f);

// Generic with custom color
CustomParticles.GenericFlare(position, Color.Cyan, 0.6f, 25);
CustomParticles.GenericMusicNotes(position, Color.Gold, 4, 25f);
```
