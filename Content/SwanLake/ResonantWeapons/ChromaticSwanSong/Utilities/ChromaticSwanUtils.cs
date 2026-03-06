using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using MagnumOpus.Content.SwanLake;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities
{
    /// <summary>
    /// Self-contained utilities for Chromatic Swan Song.
    /// Chromatic = full spectrum shifting. Pure white body + rainbow edge shimmer.
    /// </summary>
    public static class ChromaticSwanUtils
    {
        // Core palette
        public static readonly Color PureWhite = new Color(255, 255, 255);
        public static readonly Color SilverBase = new Color(200, 205, 215);
        public static readonly Color DarkContrast = new Color(20, 18, 30);
        public static readonly Color LoreColor = new Color(240, 240, 255);

        // Chromatic spectrum — desaturated pastel rainbow for prismatic edge effects
        public static readonly Color[] SpectrumPalette = new Color[]
        {
            new Color(245, 195, 205), // Pastel Rose
            new Color(245, 225, 180), // Pastel Amber
            new Color(245, 245, 200), // Pastel Lemon
            new Color(200, 245, 215), // Pastel Mint
            new Color(195, 220, 245), // Pastel Azure
            new Color(220, 200, 245), // Pastel Violet
        };

        public static Color GetSpectrumColor(float t)
        {
            t = ((t % 1f) + 1f) % 1f;
            float scaled = t * (SpectrumPalette.Length - 1);
            int lo = (int)scaled;
            int hi = Math.Min(lo + 1, SpectrumPalette.Length - 1);
            return Color.Lerp(SpectrumPalette[lo], SpectrumPalette[hi], scaled - lo);
        }

        public static Color GetChromatic(float progress)
        {
            return GetSpectrumColor(progress + (float)Main.GameUpdateCount * 0.015f);
        }

        // SpriteBatch helpers
        public static void EnterShaderRegion(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
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
