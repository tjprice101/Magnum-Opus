using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Ode to Joy rarity - Verdant green fading through rose pink to golden pollen.
    /// Used for all Ode to Joy theme items (post-Dies Irae content).
    /// Theme: Celebration, Nature's Triumph, Joyous Blossoms
    /// </summary>
    public class OdeToJoyRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate through joyous garden gradient: verdant green -> rose pink -> golden pollen -> white bloom
                float time = (float)Main.timeForVisualEffects * 0.015f;
                
                // Ode to Joy colors - celebratory garden palette
                Color verdantGreen = new Color(76, 175, 80);       // #4CAF50 - Verdant Green (Growth)
                Color rosePink = new Color(255, 182, 193);         // #FFB6C1 - Rose Pink (Blossoms)
                Color goldenPollen = new Color(255, 215, 0);       // #FFD700 - Golden Pollen (Joy)
                Color whiteBloom = new Color(255, 255, 255);       // #FFFFFF - White Bloom (Triumph)
                
                // Four-phase joyous gradient with gentle shimmer
                float phase = (time % 4f);
                float shimmer = 0.9f + 0.1f * (float)Math.Sin(time * 3f); // Gentle flower shimmer
                
                Color baseColor;
                if (phase < 1f)
                    baseColor = Color.Lerp(verdantGreen, rosePink, phase);
                else if (phase < 2f)
                    baseColor = Color.Lerp(rosePink, goldenPollen, phase - 1f);
                else if (phase < 3f)
                    baseColor = Color.Lerp(goldenPollen, whiteBloom, phase - 2f);
                else
                    baseColor = Color.Lerp(whiteBloom, verdantGreen, phase - 3f);
                
                // Apply shimmer effect
                return baseColor * shimmer;
            }
        }
    }
}
