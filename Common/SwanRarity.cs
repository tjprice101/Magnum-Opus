using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Custom rarity for Swan Lake items - elegant black and white that fades in and out.
    /// Evokes the duality of Odette (White Swan) and Odile (Black Swan) from Tchaikovsky's ballet.
    /// </summary>
    public class SwanRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Slow, elegant transition between black and white
                float time = (float)Main.timeForVisualEffects * 0.025f;
                
                // Create a smooth oscillation using sine wave
                // Goes from 0 (black) to 1 (white) and back
                float blend = (float)(Math.Sin(time) + 1f) / 2f;
                
                // Pure black with slight warmth
                Color blackSwan = new Color(15, 12, 20);
                // Pure white with slight coolness (pearlescent)
                Color whiteSwan = new Color(250, 248, 255);
                
                // Add a subtle pearlescent shimmer at the midpoint
                float shimmerIntensity = 1f - Math.Abs(blend - 0.5f) * 2f; // Peaks at 0.5
                
                // Base color transition
                Color baseColor = Color.Lerp(blackSwan, whiteSwan, blend);
                
                // Add subtle pearlescent tint at midpoint (pink/blue shimmer)
                if (shimmerIntensity > 0.3f)
                {
                    float shimmerPhase = (float)Main.timeForVisualEffects * 0.08f;
                    Color pearlTint = Color.Lerp(
                        new Color(255, 240, 245), // Light pink
                        new Color(240, 245, 255), // Light blue
                        (float)(Math.Sin(shimmerPhase) + 1f) / 2f
                    );
                    baseColor = Color.Lerp(baseColor, pearlTint, shimmerIntensity * 0.2f);
                }
                
                return baseColor;
            }
        }
    }
}
