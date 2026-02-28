using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.SinCollector.Utilities
{
    /// <summary>
    /// Self-contained utilities for Sin Collector — the infernal sniper rifle.
    /// Palette: gunmetal → dark crimson → ember tracking → muzzle flash gold → white-hot core.
    /// </summary>
    public static class SinUtils
    {
        public static readonly Color GunmetalBlack = new Color(20, 15, 18);
        public static readonly Color SinCrimson = new Color(140, 20, 20);
        public static readonly Color TrackingEmber = new Color(220, 60, 15);
        public static readonly Color MuzzleGold = new Color(255, 180, 40);
        public static readonly Color WhiteFlash = new Color(255, 240, 220);
        public static readonly Color DarkSmoke = new Color(35, 20, 18);

        public static readonly Color[] SinPalette = { GunmetalBlack, SinCrimson, TrackingEmber, MuzzleGold, WhiteFlash };

        public static Color GetSinColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (SinPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, SinPalette.Length - 1);
            return Color.Lerp(SinPalette[low], SinPalette[high], scaled - low);
        }

        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            if (colors.Length <= 1) return colors.Length == 1 ? colors[0] : Color.White;
            float scaled = t * (colors.Length - 1);
            int low = Math.Min((int)scaled, colors.Length - 2);
            return Color.Lerp(colors[low], colors[low + 1], scaled - low);
        }

        public static Color Additive(Color c, float alpha)
        {
            return new Color(c.R, c.G, c.B, 0) * alpha;
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
