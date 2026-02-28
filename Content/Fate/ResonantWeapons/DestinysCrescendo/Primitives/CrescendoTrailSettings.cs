using Microsoft.Xna.Framework;
using System;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Configuration for how a trail strip is built (width curve, color curve).
    /// Provides preset configurations for the cosmic deity beam trails.
    /// </summary>
    public struct CrescendoTrailSettings
    {
        /// <summary>Width at a given 0→1 progress along the trail (0 = head, 1 = tail).</summary>
        public Func<float, float> WidthFunction;

        /// <summary>Color at a given 0→1 progress along the trail.</summary>
        public Func<float, Color> ColorFunction;

        /// <summary>Whether to average normals between adjacent segments for smoother curves.</summary>
        public bool SmoothNormals;

        // ─── Presets ──────────────────────────────────────────────

        /// <summary>Cosmic beam trail — moderate width, tapering to nothing with divine gradient.</summary>
        public static CrescendoTrailSettings BeamTrail => new CrescendoTrailSettings
        {
            WidthFunction = p => (1f - p) * 14f,
            ColorFunction = p =>
            {
                Color c = CrescendoUtils.GetCrescendoGradient(0.2f + p * 0.6f);
                float fade = 1f - MathF.Pow(p, 1.4f);
                return new Color((int)(c.R * fade), (int)(c.G * fade), (int)(c.B * fade), 0);
            },
            SmoothNormals = true
        };

        /// <summary>Deity aura trail — wide, soft, slowly fading for ambient presence.</summary>
        public static CrescendoTrailSettings AuraTrail => new CrescendoTrailSettings
        {
            WidthFunction = p => (1f - p * 0.7f) * 20f,
            ColorFunction = p =>
            {
                Color c = Color.Lerp(CrescendoUtils.DeityPurple, CrescendoUtils.CrescendoPink, p);
                float fade = (1f - p) * 0.5f;
                return new Color((int)(c.R * fade), (int)(c.G * fade), (int)(c.B * fade), 0);
            },
            SmoothNormals = true
        };

        /// <summary>Slash arc trail — sharp head, fast decay for melee impact.</summary>
        public static CrescendoTrailSettings SlashTrail => new CrescendoTrailSettings
        {
            WidthFunction = p => (1f - p) * 10f,
            ColorFunction = p =>
            {
                Color c = Color.Lerp(CrescendoUtils.DivineCrimson, CrescendoUtils.StarGold, p);
                return c * (1f - p * 0.8f);
            },
            SmoothNormals = false
        };
    }
}
