using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Custom rarity that creates a flashing deep purple and blue hue effect
    /// for all Moonlight Sonata items.
    /// </summary>
    public class MoonlightSonataRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Create a smooth oscillation between deep purple and blue
                float time = (float)Main.timeForVisualEffects * 0.05f;
                
                // Deep purple: RGB(138, 43, 226) - BlueViolet
                // Deep blue: RGB(65, 105, 225) - RoyalBlue
                
                // Use sine wave for smooth transition
                float blend = (float)(Math.Sin(time) + 1f) / 2f; // 0 to 1
                
                // Interpolate between deep purple and deep blue
                Color deepPurple = new Color(138, 43, 226);
                Color deepBlue = new Color(65, 105, 225);
                
                return Color.Lerp(deepPurple, deepBlue, blend);
            }
        }
    }
}
