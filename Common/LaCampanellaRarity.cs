using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Custom rarity for La Campanella items - black that fades to dark orange.
    /// Evokes the fiery, bell-tolling theme of Liszt's La Campanella.
    /// </summary>
    public class LaCampanellaRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Slow transition speed for smooth gradient
                float time = (float)Main.timeForVisualEffects * 0.018f;
                
                // Black base: RGB(20, 18, 25) - very dark, almost black
                // Dark orange target: RGB(255, 100, 0) - fiery dark orange
                
                // Smooth sine wave for slow fade (0 to 1)
                float blend = (float)(Math.Sin(time) + 1f) / 2f;
                
                // Black base color
                Color black = new Color(20, 18, 25);
                // Dark orange color (fire/bell flames)
                Color darkOrange = new Color(255, 100, 0);
                
                // Smooth transition from black to dark orange
                return Color.Lerp(black, darkOrange, blend);
            }
        }
    }

    /// <summary>
    /// Rainbow rarity for special La Campanella items like boss drops.
    /// Features fiery orange to golden bell-fire effect.
    /// </summary>
    public class LaCampanellaRainbowRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Fire-themed rainbow cycle
                float time = (float)Main.timeForVisualEffects * 0.025f;
                
                // Cycle through fire hues (orange, red, yellow, black accent)
                float phase = (time % 4f) / 4f;
                
                if (phase < 0.25f)
                {
                    // Black to dark orange
                    return Color.Lerp(new Color(30, 20, 15), new Color(255, 100, 0), phase * 4f);
                }
                else if (phase < 0.5f)
                {
                    // Dark orange to bright orange-yellow
                    return Color.Lerp(new Color(255, 100, 0), new Color(255, 180, 50), (phase - 0.25f) * 4f);
                }
                else if (phase < 0.75f)
                {
                    // Bright to red-orange
                    return Color.Lerp(new Color(255, 180, 50), new Color(255, 60, 30), (phase - 0.5f) * 4f);
                }
                else
                {
                    // Red-orange back to black
                    return Color.Lerp(new Color(255, 60, 30), new Color(30, 20, 15), (phase - 0.75f) * 4f);
                }
            }
        }
    }
}
