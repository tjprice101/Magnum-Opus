using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Enigma Variations rarity - Mysterious green/purple gradient with eerie sparkles.
    /// Used for endgame Enigma theme weapons.
    /// </summary>
    public class EnigmaRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate between eerie green and deep purple
                float time = (float)Main.timeForVisualEffects * 0.02f;
                float pulse = (float)Math.Sin(time) * 0.5f + 0.5f;
                
                // Eerie green: (50, 220, 100) -> Deep purple: (120, 40, 180)
                Color green = new Color(50, 220, 100);
                Color purple = new Color(120, 40, 180);
                Color black = new Color(20, 15, 25);
                
                // Triple-phase gradient: green -> purple -> black -> green
                float phase = (time * 0.5f) % 3f;
                if (phase < 1f)
                    return Color.Lerp(green, purple, phase);
                else if (phase < 2f)
                    return Color.Lerp(purple, black, phase - 1f);
                else
                    return Color.Lerp(black, green, phase - 2f);
            }
        }
    }
    
    /// <summary>
    /// Enigma rainbow cycling rarity for special items.
    /// </summary>
    public class EnigmaRainbowRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float hue = ((float)Main.timeForVisualEffects * 0.008f) % 1f;
                // Limit hue to green-purple range (0.25 - 0.85)
                hue = 0.25f + hue * 0.6f;
                return Main.hslToRgb(hue, 0.9f, 0.6f);
            }
        }
    }
}
