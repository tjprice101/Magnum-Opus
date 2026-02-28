using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheSilentMeasure.Utilities
{
    /// <summary>
    /// Per-player state tracking for TheSilentMeasure.
    /// Tracks active seekers, chain lightning targets, visual buildup, and burst state.
    /// </summary>
    public sealed class SilentPlayer : ModPlayer
    {
        /// <summary>Number of active seeker projectiles currently tracking enemies.</summary>
        public int SeekerCount;

        /// <summary>Number of enemies currently linked in a chain lightning arc.</summary>
        public int ChainTargets;

        /// <summary>Visual buildup intensity (0-1), ramps up with successive hits and seekers.</summary>
        public float MeasureIntensity;

        /// <summary>Whether a "?" burst explosion occurred this frame (for screen effects).</summary>
        public bool QuestionBurstThisFrame;

        public override void ResetEffects()
        {
            SeekerCount = 0;
            ChainTargets = 0;
            QuestionBurstThisFrame = false;
            MeasureIntensity *= 0.95f;
            if (MeasureIntensity < 0.01f)
                MeasureIntensity = 0f;
        }
    }

    public static class SilentPlayerExtensions
    {
        public static SilentPlayer Silent(this Player player) => player.GetModPlayer<SilentPlayer>();
    }
}
