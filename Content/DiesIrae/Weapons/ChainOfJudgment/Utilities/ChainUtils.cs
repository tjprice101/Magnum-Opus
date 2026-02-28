using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ChainOfJudgment.Utilities
{
    /// <summary>
    /// Self-contained utility class for Chain of Judgment — the hellfire chain whip.
    /// Palette: molten iron blacks → chain-link reds → hellfire orange → white-hot tips.
    /// </summary>
    public static class ChainUtils
    {
        // ─── Chain Palette (6-stop, dark to bright) ───
        public static readonly Color IronBlack = new Color(25, 10, 10);
        public static readonly Color ChainCrimson = new Color(120, 15, 15);
        public static readonly Color MoltenLink = new Color(200, 45, 15);
        public static readonly Color HellfireChain = new Color(255, 120, 20);
        public static readonly Color WhiteHot = new Color(255, 200, 120);
        public static readonly Color AshWhite = new Color(255, 240, 220);
        public static readonly Color DarkSmoke = new Color(30, 15, 12);

        public static readonly Color[] ChainPalette = { IronBlack, ChainCrimson, MoltenLink, HellfireChain, WhiteHot, AshWhite };

        /// <summary>
        /// Gets a color from the chain palette based on a 0-1 progress value.
        /// </summary>
        public static Color GetChainColor(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (ChainPalette.Length - 1);
            int low = (int)scaled;
            int high = Math.Min(low + 1, ChainPalette.Length - 1);
            return Color.Lerp(ChainPalette[low], ChainPalette[high], scaled - low);
        }

        /// <summary>
        /// Chain bounce easing — sharp initial acceleration then snap deceleration at end.
        /// Simulates the whip-crack energy transfer.
        /// </summary>
        public static float ChainSnap(float t)
        {
            if (t < 0.2f) return t / 0.2f * 0.8f; // Fast start
            if (t < 0.7f) return 0.8f + (t - 0.2f) / 0.5f * 0.15f; // Coast
            return 0.95f + (t - 0.7f) / 0.3f * 0.05f; // Snap settle
        }

        /// <summary>
        /// Multicolor lerp helper for random coloring within chain palette.
        /// </summary>
        public static Color MulticolorLerp(float t, params Color[] colors)
        {
            if (colors.Length == 0) return Color.White;
            if (colors.Length == 1) return colors[0];
            float scaled = t * (colors.Length - 1);
            int low = Math.Min((int)scaled, colors.Length - 2);
            return Color.Lerp(colors[low], colors[low + 1], scaled - low);
        }

        /// <summary>
        /// Creates an additive color with the given alpha multiplier.
        /// </summary>
        public static Color Additive(Color c, float alpha)
        {
            return new Color(c.R, c.G, c.B, 0) * alpha;
        }

        /// <summary>
        /// Begin SpriteBatch in additive blend mode.
        /// </summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Return SpriteBatch to default blend mode.
        /// </summary>
        public static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
