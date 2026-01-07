using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Custom rarity for Eroica items - deep scarlet that slowly fades to light pink.
    /// Evokes the heroic, passionate theme of Beethoven's Eroica Symphony.
    /// </summary>
    public class EroicaRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Slow transition speed for smooth gradient
                float time = (float)Main.timeForVisualEffects * 0.02f;
                
                // Deep scarlet base: RGB(139, 0, 40) - rich, dark red
                // Very light pink target: RGB(255, 200, 220) - soft, light pink
                
                // Smooth sine wave for slow fade (0 to 1)
                float blend = (float)(Math.Sin(time) + 1f) / 2f;
                
                // Deep scarlet base color
                Color deepScarlet = new Color(139, 0, 40);
                // Very light pink color  
                Color lightPink = new Color(255, 200, 220);
                
                // Smooth transition from deep scarlet to light pink
                return Color.Lerp(deepScarlet, lightPink, blend);
            }
        }
    }

    /// <summary>
    /// Rainbow rarity for special Eroica items like the Score and Harmonic Energies.
    /// Cycles through all colors of the rainbow for a magical effect.
    /// </summary>
    public class EroicaRainbowRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Smooth rainbow cycle using HSL-like approach
                float time = (float)Main.timeForVisualEffects * 0.02f;
                
                // Cycle through hue values (0-1 = full rainbow)
                float hue = (time % 6f) / 6f;
                
                // Convert hue to RGB (simplified HSV to RGB with full saturation and value)
                return HueToRGB(hue);
            }
        }

        private static Color HueToRGB(float hue)
        {
            // Ensure hue is in 0-1 range
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
            
            // Boost brightness slightly for better visibility
            return new Color((int)(r * 255), (int)(g * 255), (int)(b * 255));
        }
    }
}
