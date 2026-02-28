using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.EnigmaVariations.ResonantWeapons.TheWatchingRefrain.Utilities
{
    /// <summary>
    /// Per-player state tracking for TheWatchingRefrain.
    /// Tracks phantom phase, mystery zone count, watcher intensity, and phase-shift events.
    /// </summary>
    public sealed class WatchingPlayer : ModPlayer
    {
        /// <summary>Current minion phase index — affects opacity, attack pattern, and VFX.</summary>
        public int PhantomPhase;

        /// <summary>Number of active mystery zones spawned by the phantom.</summary>
        public int MysteryZoneCount;

        /// <summary>Visual buildup intensity (0-1) — ramps up during sustained combat.</summary>
        public float WatcherIntensity;

        /// <summary>Whether the phantom shifted phase this frame (triggers VFX burst).</summary>
        public bool PhaseShiftThisFrame;

        public override void ResetEffects()
        {
            PhaseShiftThisFrame = false;
        }

        public override void PostUpdate()
        {
            // Watcher intensity decays toward 0 when not actively reinforced
            if (WatcherIntensity > 0f)
                WatcherIntensity = MathHelper.Clamp(WatcherIntensity - 0.005f, 0f, 1f);

            // Mystery zone count is reset externally by the minion projectile each frame
        }
    }

    public static class WatchingPlayerExtensions
    {
        public static WatchingPlayer Watching(this Player player) => player.GetModPlayer<WatchingPlayer>();
    }
}
