using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Fate rarity - Cosmic white/pink/purple/crimson gradient with reality-bending shimmer.
    /// Used for ultimate endgame Fate theme weapons.
    /// </summary>
    public class FateRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate through cosmic gradient: white -> pink -> purple -> crimson
                float time = (float)Main.timeForVisualEffects * 0.015f;
                
                Color white = new Color(255, 255, 255);
                Color darkPink = new Color(200, 80, 120);
                Color purple = new Color(140, 50, 160);
                Color crimson = new Color(180, 30, 60);
                
                // Four-phase cosmic gradient with smooth cycling
                float phase = (time % 4f);
                if (phase < 1f)
                    return Color.Lerp(white, darkPink, phase);
                else if (phase < 2f)
                    return Color.Lerp(darkPink, purple, phase - 1f);
                else if (phase < 3f)
                    return Color.Lerp(purple, crimson, phase - 2f);
                else
                    return Color.Lerp(crimson, white, phase - 3f);
            }
        }
    }
    
    /// <summary>
    /// Fate cosmic rainbow rarity for the most powerful items.
    /// Chromatic aberration-style color shifting.
    /// </summary>
    public class FateCosmicRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                float time = (float)Main.timeForVisualEffects * 0.02f;
                
                // Chromatic aberration effect - rapid RGB shifting
                float r = (float)Math.Sin(time * 1.0f) * 0.5f + 0.5f;
                float g = (float)Math.Sin(time * 1.3f + 2f) * 0.5f + 0.5f;
                float b = (float)Math.Sin(time * 1.7f + 4f) * 0.5f + 0.5f;
                
                // Bias toward pink/purple/white cosmic palette
                r = 0.6f + r * 0.4f;
                g = 0.2f + g * 0.4f;
                b = 0.5f + b * 0.5f;
                
                return new Color(r, g, b);
            }
        }
    }
}
