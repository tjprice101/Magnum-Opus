using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities
{
    /// <summary>
    /// Self-contained utilities for Iridescent Wingspan.
    /// Ethereal wings theme — pure white with prismatic edge glow, graceful and spectral.
    /// </summary>
    public static class WingspanUtils
    {
        public static readonly Color EtherealWhite = new Color(240, 240, 255);
        public static readonly Color WingPrismatic = new Color(245, 245, 255);
        public static readonly Color SpectralBlue = new Color(200, 210, 230);
        public static readonly Color DeepVoid = new Color(10, 8, 20);
        public static readonly Color LoreColor = new Color(240, 240, 255);

        /// <summary>Wing palette — neutral white gradient with subtle cool shimmer.</summary>
        public static readonly Color[] WingPalette = new Color[]
        {
            new Color(255, 255, 255),
            new Color(240, 242, 250),
            new Color(220, 225, 240),
            new Color(200, 210, 230),
            new Color(225, 230, 245),
            new Color(248, 248, 255),
        };

        public static Color GetWingGradient(float t)
        {
            t = ((t % 1f) + 1f) % 1f;
            float scaled = t * (WingPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, WingPalette.Length - 1);
            return Color.Lerp(WingPalette[lo], WingPalette[hi], scaled - lo);
        }

        public static Color GetPrismaticEdge(float angle)
        {
            float hue = ((angle / MathHelper.TwoPi) + (float)Main.GameUpdateCount * 0.005f) % 1f;
            // Desaturated pastel rainbow over white base — Swan Lake prismatic edge
            return Main.hslToRgb(hue, 0.35f, 0.9f);
        }

        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, MagnumBlendStates.TrueAdditive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        // ─────────── THEME TEXTURE ACCENTS ───────────

        public static void DrawThemeAccents(SpriteBatch sb, Vector2 worldPos, float scale, float intensity = 1f)
        {
            SwanLakeVFXLibrary.DrawThemeCrystalAccent(sb, worldPos, scale, intensity * 0.5f);
            float rot = (float)Main.GameUpdateCount * 0.02f;
            SwanLakeVFXLibrary.DrawThemeImpactRing(sb, worldPos, scale, intensity * 0.4f, rot);
        }
    }
}
