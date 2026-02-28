using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.DamnationsCannon.Utilities
{
    public static class DamnationUtils
    {
        public static readonly Color VoidBlack = new Color(15, 8, 8);
        public static readonly Color DamnationRed = new Color(160, 20, 10);
        public static readonly Color WrathOrange = new Color(240, 80, 15);
        public static readonly Color ExplosionGold = new Color(255, 160, 30);
        public static readonly Color DetonationWhite = new Color(255, 230, 200);
        public static readonly Color DarkSmoke = new Color(30, 15, 10);

        public static readonly Color[] DamnationPalette = { VoidBlack, DamnationRed, WrathOrange, ExplosionGold, DetonationWhite };

        public static Color GetDamnationColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (DamnationPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, DamnationPalette.Length - 1);
            return Color.Lerp(DamnationPalette[low], DamnationPalette[high], scaled - low);
        }

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            if (colors.Length <= 1) return colors.Length == 1 ? colors[0] : Color.White;
            float scaled = t * (colors.Length - 1);
            int low = Math.Min((int)scaled, colors.Length - 2);
            return Color.Lerp(colors[low], colors[low + 1], scaled - low);
        }

        public static Color Additive(Color c, float alpha) => new Color(c.R, c.G, c.B, 0) * alpha;
    }
}
