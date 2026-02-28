using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Configuration for how a trail strip is built (width curve, color curve).
    /// Provides preset configurations for spiral blades and fragments.
    /// </summary>
    public struct SymphonyTrailSettings
    {
        /// <summary>Width at a given 0→1 progress along the trail (0 = head, 1 = tail).</summary>
        public Func<float, float> WidthFunction;

        /// <summary>Color at a given 0→1 progress along the trail.</summary>
        public Func<float, Color> ColorFunction;

        /// <summary>Whether to average normals between adjacent segments for smoother curves.</summary>
        public bool SmoothNormals;

        // ─── Presets ──────────────────────────────────────────────

        /// <summary>Main spiral blade trail — wide head, tapers to nothing.</summary>
        public static SymphonyTrailSettings SpiralBlade => new SymphonyTrailSettings
        {
            WidthFunction = p => (1f - p) * 18f,
            ColorFunction = p =>
            {
                Color c = SymphonyUtils.GetSymphonyGradient(p);
                return c * (1f - p * 0.6f);
            },
            SmoothNormals = true
        };

        /// <summary>Fragment trail — thinner, redder, decays fast.</summary>
        public static SymphonyTrailSettings Fragment => new SymphonyTrailSettings
        {
            WidthFunction = p => (1f - p) * 9f,
            ColorFunction = p =>
            {
                Color c = Color.Lerp(SymphonyUtils.DiscordRed, SymphonyUtils.SymphonyPink, p);
                return c * (1f - p * 0.7f);
            },
            SmoothNormals = false
        };
    }
}
