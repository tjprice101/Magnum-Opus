using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

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

        // Chromatic spectrum — shifts through these for rainbow edge effects
        public static readonly Color[] SpectrumPalette = new Color[]
        {
            new Color(255, 100, 130), // Rose
            new Color(255, 180, 80),  // Amber
            new Color(255, 255, 120), // Lemon
            new Color(120, 255, 160), // Mint
            new Color(100, 180, 255), // Azure
            new Color(180, 120, 255), // Violet
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
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        public static void RestoreSpriteBatch(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }
    }
}
