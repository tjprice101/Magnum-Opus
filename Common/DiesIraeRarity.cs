using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Common
{
    /// <summary>
    /// Dies Irae rarity - Blood red fading through charred black to fiery ember orange.
    /// Used for all Dies Irae theme items (post-Nachtmusik content).
    /// Theme: Day of Wrath, Final Judgment, Hellfire and Damnation
    /// </summary>
    public class DiesIraeRarity : ModRarity
    {
        public override Color RarityColor
        {
            get
            {
                // Animate through infernal judgment gradient: blood red -> charred black -> ember orange -> crimson
                float time = (float)Main.timeForVisualEffects * 0.015f;
                
                // Dies Irae colors - infernal hellfire palette
                Color bloodRed = new Color(139, 0, 0);         // #8B0000 - Blood Red (Wrath)
                Color charredBlack = new Color(25, 10, 10);    // #190A0A - Charred Black (Damnation)
                Color emberOrange = new Color(255, 100, 0);    // #FF6400 - Ember Orange (Hellfire)
                Color crimson = new Color(220, 20, 60);        // #DC143C - Crimson (Judgment)
                
                // Four-phase infernal gradient with ominous flicker
                float phase = (time % 4f);
                float flicker = 0.85f + 0.15f * (float)Math.Sin(time * 5f); // Ominous fire flicker
                
                Color baseColor;
                if (phase < 1f)
                    baseColor = Color.Lerp(bloodRed, charredBlack, phase);
                else if (phase < 2f)
                    baseColor = Color.Lerp(charredBlack, emberOrange, phase - 1f);
                else if (phase < 3f)
                    baseColor = Color.Lerp(emberOrange, crimson, phase - 2f);
                else
                    baseColor = Color.Lerp(crimson, bloodRed, phase - 3f);
                
                // Apply flicker effect
                return baseColor * flicker;
            }
        }
    }
}
