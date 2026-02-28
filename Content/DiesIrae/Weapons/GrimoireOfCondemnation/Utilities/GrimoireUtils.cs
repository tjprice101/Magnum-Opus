using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.GrimoireOfCondemnation.Utilities
{
    public static class GrimoireUtils
    {
        public static readonly Color VoidInk = new Color(20, 8, 15);
        public static readonly Color CurseRed = new Color(160, 25, 20);
        public static readonly Color CondemnOrange = new Color(240, 90, 20);
        public static readonly Color AccursedGold = new Color(255, 170, 40);
        public static readonly Color ParchmentWhite = new Color(255, 240, 210);

        public static readonly Color[] GrimoirePalette = { VoidInk, CurseRed, CondemnOrange, AccursedGold, ParchmentWhite };

        public static Color GetGrimoireColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (GrimoirePalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, GrimoirePalette.Length - 1);
            return Color.Lerp(GrimoirePalette[low], GrimoirePalette[high], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
