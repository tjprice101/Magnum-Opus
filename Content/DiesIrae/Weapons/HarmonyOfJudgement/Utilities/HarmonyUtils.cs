using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Utilities
{
    public static class HarmonyUtils
    {
        public static readonly Color VoidBlack = new Color(10, 5, 5);
        public static readonly Color HarmonyRed = new Color(170, 25, 15);
        public static readonly Color JudgmentEmber = new Color(255, 90, 20);
        public static readonly Color SigilGold = new Color(255, 180, 50);
        public static readonly Color RayWhite = new Color(255, 245, 230);

        public static readonly Color[] HarmonyPalette = { VoidBlack, HarmonyRed, JudgmentEmber, SigilGold, RayWhite };

        public static Color GetHarmonyColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (HarmonyPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, HarmonyPalette.Length - 1);
            return Color.Lerp(HarmonyPalette[low], HarmonyPalette[high], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
