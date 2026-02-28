using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Utilities;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Utilities
{
    /// <summary>
    /// Self-contained color palette and math utilities for The Final Fermata.
    /// Zero shared system references.
    /// </summary>
    public static class FermataUtils
    {
        // === FERMATA COLOR PALETTE ===
        public static readonly Color TemporalVoid = new Color(5, 5, 15);
        public static readonly Color FermataPurple = new Color(160, 50, 200);
        public static readonly Color TimeGold = new Color(255, 210, 70);
        public static readonly Color GhostSilver = new Color(200, 210, 240);
        public static readonly Color FermataCrimson = new Color(220, 50, 90);
        public static readonly Color FlashWhite = new Color(250, 248, 255);

        /// <summary>
        /// Lerps across the Fermata palette (6 stops).
        /// t in [0..1]: TemporalVoid -> FermataPurple -> FermataCrimson -> TimeGold -> GhostSilver -> FlashWhite.
        /// </summary>
        public static Color PaletteLerp(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            Color[] stops = { TemporalVoid, FermataPurple, FermataCrimson, TimeGold, GhostSilver, FlashWhite };
            float segment = t * (stops.Length - 1);
            int index = (int)segment;
            if (index >= stops.Length - 1) return stops[stops.Length - 1];
            float local = segment - index;
            return Color.Lerp(stops[index], stops[index + 1], local);
        }

        /// <summary>
        /// Smooth ease-in-out (Hermite interpolation).
        /// </summary>
        public static float SmoothStep(float t)
        {
            t = MathHelper.Clamp(t, 0f, 1f);
            return t * t * (3f - 2f * t);
        }

        /// <summary>
        /// Sine pulse between 0 and 1 at the given frequency.
        /// </summary>
        public static float SinePulse(float time, float frequency)
        {
            return (MathF.Sin(time * frequency) + 1f) * 0.5f;
        }

        /// <summary>
        /// Compute orbit angle for a sword slot in an N-sword formation.
        /// </summary>
        public static float FormationAngle(int index, int total, float baseAngle)
        {
            if (total <= 0) total = 1;
            return baseAngle + MathHelper.TwoPi * index / total;
        }

        /// <summary>
        /// Oscillating radius for visual interest.
        /// </summary>
        public static float OscillatingRadius(float baseRadius, float time, float amplitude = 5f, float freq = 0.05f)
        {
            return baseRadius + MathF.Sin(time * freq) * amplitude;
        }

        /// <summary>
        /// Quick 2D rotation vector from angle.
        /// </summary>
        public static Vector2 AngleToVector(float angle)
        {
            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        /// <summary>
        /// Random float from -range to +range using Terraria's rand.
        /// </summary>
        public static float RandSpread(float range)
        {
            return (Terraria.Main.rand.NextFloat() * 2f - 1f) * range;
        }

        /// <summary>
        /// Lerp color with alpha preserved.
        /// </summary>
        public static Color LerpColor(Color a, Color b, float t)
        {
            return Color.Lerp(a, b, MathHelper.Clamp(t, 0f, 1f));
        }
    }
}
