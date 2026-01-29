using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Nachtmusik rarity - Dark midnight purple fading into shimmering gold.
    /// Used for all Nachtmusik theme items (post-Fate content).
    /// </summary>
    public class NachtmusikRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate through nocturnal cosmic gradient: deep purple -> violet -> gold -> star white
                float time = (float)Main.timeForVisualEffects * 0.012f;
                
                // Nachtmusik colors from the design doc
                Color deepPurple = new Color(45, 27, 78);     // #2D1B4E - Deep Purple (Night)
                Color violet = new Color(123, 104, 238);       // #7B68EE - Violet
                Color gold = new Color(255, 215, 0);           // #FFD700 - Gold (Stars)
                Color starWhite = new Color(255, 255, 255);    // #FFFFFF - Star White
                
                // Four-phase nocturnal gradient with smooth cycling and shimmer
                float phase = (time % 4f);
                float shimmer = 0.9f + 0.1f * (float)Math.Sin(time * 3f); // Subtle star shimmer
                
                Color baseColor;
                if (phase < 1f)
                    baseColor = Color.Lerp(deepPurple, violet, phase);
                else if (phase < 2f)
                    baseColor = Color.Lerp(violet, gold, phase - 1f);
                else if (phase < 3f)
                    baseColor = Color.Lerp(gold, starWhite, phase - 2f);
                else
                    baseColor = Color.Lerp(starWhite, deepPurple, phase - 3f);
                
                // Apply shimmer effect
                return baseColor * shimmer;
            }
        }
    }
}
