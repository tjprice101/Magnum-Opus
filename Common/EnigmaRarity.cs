using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    // NOTE: EnigmaRarity class has been removed. Use EnigmaVariationsRarity instead.
    // EnigmaRainbowRarity is kept here for special/rainbow-tier Enigma items.
    
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
