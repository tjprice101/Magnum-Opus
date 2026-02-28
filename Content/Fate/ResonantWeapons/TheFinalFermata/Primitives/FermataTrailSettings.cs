using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheFinalFermata.Primitives
{
    /// <summary>
    /// Configuration for a Fermata trail strip.
    /// Defines width curve, color gradient, and rendering parameters.
    /// </summary>
    public class FermataTrailSettings
    {
        /// <summary>Maximum trail width in pixels.</summary>
        public float MaxWidth { get; set; } = 16f;

        /// <summary>Number of stored trail positions.</summary>
        public int TrailLength { get; set; } = 20;

        /// <summary>Width multiplier curve: progress (0=tip, 1=tail) -> width factor.</summary>
        public Func<float, float> WidthCurve { get; set; } = DefaultWidthCurve;

        /// <summary>Color gradient: progress (0=tip, 1=tail) -> color.</summary>
        public Func<float, Color> ColorGradient { get; set; } = DefaultColorGradient;

        /// <summary>Overall opacity multiplier.</summary>
        public float Opacity { get; set; } = 1f;

        /// <summary>Whether to use additive blending.</summary>
        public bool Additive { get; set; } = true;

        /// <summary>Default width curve: full at tip, tapers to 0 at tail.</summary>
        private static float DefaultWidthCurve(float t)
        {
            // Smooth taper: wide at front, thin at back
            return 1f - t * t;
        }

        /// <summary>Default color gradient using Fermata palette.</summary>
        private static Color DefaultColorGradient(float t)
        {
            return Utilities.FermataUtils.PaletteLerp(t);
        }

        /// <summary>Creates settings for the spectral sword orbit trail.</summary>
        public static FermataTrailSettings SwordOrbitTrail()
        {
            return new FermataTrailSettings
            {
                MaxWidth = 12f,
                TrailLength = 16,
                WidthCurve = t => (1f - t) * (1f - t),
                ColorGradient = t => Color.Lerp(
                    Utilities.FermataUtils.FermataPurple,
                    Utilities.FermataUtils.TemporalVoid,
                    t) * (1f - t * 0.6f),
                Opacity = 0.7f,
                Additive = true
            };
        }

        /// <summary>Creates settings for the sync slash trail.</summary>
        public static FermataTrailSettings SlashTrail()
        {
            return new FermataTrailSettings
            {
                MaxWidth = 20f,
                TrailLength = 12,
                WidthCurve = t => MathF.Sqrt(1f - t),
                ColorGradient = t => Color.Lerp(
                    Utilities.FermataUtils.FlashWhite,
                    Utilities.FermataUtils.FermataCrimson,
                    t * t),
                Opacity = 0.9f,
                Additive = true
            };
        }
    }
}
