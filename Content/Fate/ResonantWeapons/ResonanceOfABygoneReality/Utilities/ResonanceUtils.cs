using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Self-contained utilities for Resonance of a Bygone Reality.
    /// Color palette (6 colors), gradient helper, SpriteBatch mode toggles, math helpers.
    /// Zero references to global VFX systems.
    /// </summary>
    public static class ResonanceUtils
    {
        // === COLOR PALETTE ===
        public static readonly Color VoidBlack = new Color(15, 5, 20);
        public static readonly Color NebulaMist = new Color(140, 60, 120);
        public static readonly Color NebulaPurple = new Color(160, 80, 200);
        public static readonly Color CosmicRose = new Color(220, 80, 130);
        public static readonly Color StarGold = new Color(255, 230, 180);
        public static readonly Color ConstellationSilver = new Color(200, 210, 240);

        private static readonly Color[] Palette = { VoidBlack, NebulaMist, NebulaPurple, CosmicRose, StarGold, ConstellationSilver };

        /// <summary>
        /// Lerp through the 6-color palette. t in [0,1].
        /// </summary>
        public static Color GradientLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            float scaled = t * (Palette.Length - 1);
            int idx = Math.Min((int)scaled, Palette.Length - 2);
            float frac = scaled - idx;
            return Color.Lerp(Palette[idx], Palette[idx + 1], frac);
        }

        /// <summary>
        /// Begin additive SpriteBatch mode for bloom / glow drawing.
        /// </summary>
        public static void BeginAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Restore default alpha-blend SpriteBatch mode.
        /// </summary>
        public static void EndAdditive(SpriteBatch sb)
        {
            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);
        }

        /// <summary>
        /// Hermite smooth-step interpolation [0,1].
        /// </summary>
        public static float SmoothStep(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Oscillating pulse mapped to [0,1].
        /// </summary>
        public static float Pulse(float time, float frequency = 1f)
        {
            return (MathF.Sin(time * frequency) + 1f) * 0.5f;
        }

        /// <summary>
        /// Returns a random palette-sampled color.
        /// </summary>
        public static Color RandomPaletteColor()
        {
            return GradientLerp(Main.rand.NextFloat());
        }
    }
}
