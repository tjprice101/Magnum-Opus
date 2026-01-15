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

    /// <summary>
    /// Rainbow rarity for special Moonlight Sonata items like wings.
    /// Cycles through all colors of the rainbow for a magical effect.
    /// </summary>
    public class MoonlightSonataRainbowRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float time = (float)Main.timeForVisualEffects * 0.02f;
                float hue = (time % 6f) / 6f;
                return HueToRGB(hue);
            }
        }

        private static Color HueToRGB(float hue)
        {
            hue = hue - (float)Math.Floor(hue);
            float r, g, b;
            float h = hue * 6f;
            int i = (int)Math.Floor(h);
            float f = h - i;
            
            switch (i % 6)
            {
                case 0: r = 1; g = f; b = 0; break;
                case 1: r = 1 - f; g = 1; b = 0; break;
                case 2: r = 0; g = 1; b = f; break;
                case 3: r = 0; g = 1 - f; b = 1; break;
                case 4: r = f; g = 0; b = 1; break;
                default: r = 1; g = 0; b = 1 - f; break;
            }
            
            return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
