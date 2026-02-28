using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Trail style presets with width/color curve functions.
    /// BulletTrail: narrow, bright, fast taper.
    /// BladeTrail: wide, ghostly, bell-curve width.
    /// </summary>
    public class ResonanceTrailSettings
    {
        /// <summary>Width at a given progress (0 = head, 1 = tail).</summary>
        public Func<float, float> WidthFunction;

        /// <summary>Color at a given progress (0 = head, 1 = tail).</summary>
        public Func<float, Color> ColorFunction;

        /// <summary>Maximum trail width in pixels.</summary>
        public float MaxWidth;

        /// <summary>
        /// Preset: bullet trails — narrow, bright, quadratic taper.
        /// </summary>
        public static ResonanceTrailSettings BulletTrail => new ResonanceTrailSettings
        {
            MaxWidth = 6f,
            WidthFunction = t => (1f - t) * (1f - t),
            ColorFunction = t =>
            {
                Color c = ResonanceUtils.GradientLerp(t * 0.8f + 0.2f);
                return c * (1f - t);
            }
        };

        /// <summary>
        /// Preset: blade slash trails — wide, ghostly, bell-curve shape.
        /// </summary>
        public static ResonanceTrailSettings BladeTrail => new ResonanceTrailSettings
        {
            MaxWidth = 24f,
            WidthFunction = t =>
            {
                float bell = MathF.Sin(t * MathHelper.Pi);
                return bell * (1f - t * 0.3f);
            },
            ColorFunction = t =>
            {
                Color c = Color.Lerp(ResonanceUtils.ConstellationSilver, ResonanceUtils.NebulaPurple, t);
                float alpha = 1f - t * t;
                return c * alpha;
            }
        };
    }
}
