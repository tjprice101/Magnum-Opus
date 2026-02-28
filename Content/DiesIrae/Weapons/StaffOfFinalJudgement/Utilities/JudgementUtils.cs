using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Utilities
{
    public static class JudgementUtils
    {
        public static readonly Color InfernalBlack = new Color(15, 5, 5);
        public static readonly Color WrathCrimson = new Color(180, 20, 10);
        public static readonly Color JudgmentFlame = new Color(255, 80, 15);
        public static readonly Color DetonationGold = new Color(255, 190, 40);
        public static readonly Color DivineWhite = new Color(255, 245, 225);

        public static readonly Color[] JudgmentPalette = { InfernalBlack, WrathCrimson, JudgmentFlame, DetonationGold, DivineWhite };

        public static Color GetJudgmentColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (JudgmentPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, JudgmentPalette.Length - 1);
            return Color.Lerp(JudgmentPalette[low], JudgmentPalette[high], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
