using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ArbitersSentence.Utilities
{
    public static class ArbiterUtils
    {
        public static readonly Color JudgmentCrimson = new Color(180, 20, 10);
        public static readonly Color HellflameOrange = new Color(255, 100, 15);
        public static readonly Color PurgatoryGold = new Color(255, 180, 40);
        public static readonly Color SentenceWhite = new Color(255, 240, 220);
        public static readonly Color AshBlack = new Color(20, 10, 8);

        public static readonly Color[] FlamePalette = { AshBlack, JudgmentCrimson, HellflameOrange, PurgatoryGold, SentenceWhite };

        public static Color GetFlameColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (FlamePalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, FlamePalette.Length - 1);
            return Color.Lerp(FlamePalette[low], FlamePalette[high], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
