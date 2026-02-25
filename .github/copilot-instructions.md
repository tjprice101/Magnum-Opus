# MagnumOpus Copilot Instructions

## Reference Repositories

When implementing VFX, shaders, trails, bloom, particles, melee swings, projectile rendering, or any visual effect, **analyze these repositories directly** and follow their patterns:

| Repository | Link | Use For |
|------------|------|---------|
| **CalamityMod** | [CalamityModPublic (1.4.4 branch)](https://github.com/CalamityTeam/CalamityModPublic/tree/1.4.4) | Melee swing architecture (Exoblade, Ark of the Cosmos, Galaxia), primitive trail shaders, bloom stacking, boss AI, projectile VFX, CurveSegment animation |
| **VFX+** | [VFXPlus](https://github.com/GreatFriend129/VFXPlus/tree/main) | Advanced VFX systems, shader implementations, trail rendering, visual polish techniques |
| **Coralite** | [Coralite-Mod](https://github.com/CoraIite/Coralite-Mod) | Shader techniques, particle systems, visual effect implementations |

**Do NOT invent VFX patterns from scratch.** Read how these mods implement the specific effect you need, then adapt their approach for MagnumOpus using MagnumOpus's existing utility classes where available.

---

## Asset Workflow

If you need a texture, particle sprite, or any visual asset that does not already exist in `Assets/`, **STOP implementation** and tell the user:

1. **What asset is needed** and where it would be used
2. **A detailed Midjourney prompt** to generate it, following this format:
   - Art style and medium
   - Subject description with specific visual details
   - Color palette (white/grayscale on solid black background for particles)
   - Dimensions and technical requirements
   - Example: *"White soft circular glow with gentle falloff on solid black background, game particle texture, 256x256px, seamless edges, no background detail --ar 1:1 --style raw"*
3. **Expected file location** in the project (e.g., `Assets/Particles/`, or same folder as the .cs file)

Do not use placeholder textures or skip VFX because an asset is missing. Ask for it.

---

## Mod Philosophy

MagnumOpus is a music-themed Terraria mod where each weapon, projectile, and boss implements its own unique VFX directly in its .cs file. There are no global VFX systems that automatically apply effects. Every weapon should have a distinct visual identity -- no two weapons in the same theme should share effects.

---

## Theme Identities

Each musical score has its own visual and emotional identity. Use the correct theme's colors and mood when implementing content for that theme.

| Theme | Musical Soul | Colors | Emotional Core |
|-------|-------------|--------|---------------|
| **La Campanella** | The ringing bell, virtuosic fire | Black smoke, orange flames, gold highlights | Passion, intensity, burning brilliance |
| **Eroica** | The hero's symphony | Scarlet, crimson, gold, sakura pink | Courage, sacrifice, triumphant glory |
| **Swan Lake** | Grace dying beautifully | Pure white, black contrast, prismatic rainbow edges | Elegance, tragedy, ethereal beauty |
| **Moonlight Sonata** | The moon's quiet sorrow | Deep purple, violet, lavender, silver | Melancholy, peace, mystical stillness |
| **Enigma Variations** | The unknowable mystery | Void black, deep purple, eerie green flame | Mystery, dread, arcane secrets |
| **Fate** | The celestial symphony of destiny | Black void, dark pink, bright crimson, celestial white | Cosmic inevitability, endgame awe |
| **Clair de Lune** | Moonlit reverie | Night mist blue, soft blue, pearl white | Dreamlike calm, gentle luminescence |
| **Dies Irae** | Day of wrath | Blood red, dark crimson, ember orange | Fury, judgment, apocalyptic power |
| **Nachtmusik** | A little night music | Deep indigo, starlight silver, cosmic blue | Nocturnal wonder, stellar beauty |
| **Ode to Joy** | Universal brotherhood | Warm gold, radiant amber, jubilant light | Joy, celebration, triumph of spirit |

---

## Item Tooltips

Every item must implement `ModifyTooltips` with effect descriptions and a themed lore line. Do not leave tooltips empty.

```csharp
public override void ModifyTooltips(List<TooltipLine> tooltips)
{
    tooltips.Add(new TooltipLine(Mod, "Effect1", "Effect description here"));
    tooltips.Add(new TooltipLine(Mod, "Lore", "'Themed flavor text'") { OverrideColor = themeColor });
}
```

---

## File Structure

- Melee weapons: `WeaponItem.cs`, `WeaponSwing.cs`, optional `WeaponVFX.cs` in their own folder
- Item/projectile textures: same folder as their .cs file
- Particle textures: `Assets/Particles/`
- Shader files: `Assets/Shaders/`
- Music: `Assets/Music/`
