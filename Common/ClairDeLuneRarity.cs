using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Clair de Lune rarity - FINAL BOSS TIER
    /// Dark gray clockwork fading through crimson energy to crystal brilliance and brass accents.
    /// Used for all Clair de Lune theme items (post-Ode to Joy content).
    /// Theme: Shattered Time, Clockwork Mechanisms, Temporal Crystals, Crimson Energy
    /// </summary>
    public class ClairDeLuneRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate through temporal clockwork gradient: dark steel -> crimson -> crystal -> brass
                float time = (float)Main.timeForVisualEffects * 0.018f;
                
                // Clair de Lune colors - clockwork temporal palette
                Color darkGray = new Color(58, 58, 58);            // #3A3A3A - Clockwork Steel
                Color crimson = new Color(220, 20, 60);            // #DC143C - Temporal Energy
                Color crystal = new Color(224, 224, 224);          // #E0E0E0 - Shattered Time Crystal
                Color brass = new Color(205, 127, 50);             // #CD7F32 - Clockwork Brass
                Color moonlightSilver = new Color(192, 192, 220);  // Lunar Reflection
                
                // Five-phase temporal gradient with clockwork pulse
                float phase = (time % 5f);
                float mechanicalPulse = 0.85f + 0.15f * (float)Math.Sin(time * 4f); // Mechanical gear shimmer
                
                Color baseColor;
                if (phase < 1f)
                {
                    // Dark steel to crimson
                    baseColor = Color.Lerp(darkGray, crimson, phase);
                }
                else if (phase < 2f)
                {
                    // Crimson to crystal
                    baseColor = Color.Lerp(crimson, crystal, phase - 1f);
                }
                else if (phase < 3f)
                {
                    // Crystal to moonlight silver
                    baseColor = Color.Lerp(crystal, moonlightSilver, phase - 2f);
                }
                else if (phase < 4f)
                {
                    // Moonlight silver to brass
                    baseColor = Color.Lerp(moonlightSilver, brass, phase - 3f);
                }
                else
                {
                    // Brass back to dark steel
                    baseColor = Color.Lerp(brass, darkGray, phase - 4f);
                }
                
                // Apply mechanical pulse for that clockwork shimmer
                return baseColor * mechanicalPulse;
            }
        }
    }
}
