using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.SymphonysEnd
{
    /// <summary>
    /// Per-player state for Symphony's End.
    /// Tracks firing cadence, active blade count, and wand crackle intensity.
    /// </summary>
    public class SymphonyPlayer : ModPlayer
    {
        /// <summary>Number of SymphonySpiralBlade projectiles currently alive.</summary>
        public int ActiveBladeCount;

        /// <summary>Frames elapsed since last blade was fired.</summary>
        public int FramesSinceLastFire;

        /// <summary>True while the wand is held (enables crackle VFX).</summary>
        public bool IsHoldingWand;

        /// <summary>0-1 intensity that ramps up with rapid fire.</summary>
        public float FireIntensity;

        public override void ResetEffects()
        {
            IsHoldingWand = false;
            FramesSinceLastFire++;

            // Decay fire intensity when not actively firing
            if (FramesSinceLastFire > 15)
                FireIntensity = MathHelper.Lerp(FireIntensity, 0f, 0.05f);
        }

        /// <summary>Called by the item each time a blade is fired.</summary>
        public void OnFire()
        {
            FramesSinceLastFire = 0;
            FireIntensity = MathHelper.Clamp(FireIntensity + 0.15f, 0f, 1f);
        }
    }

    /// <summary>
    /// Extension: <c>player.Symphony()</c> returns the <see cref="SymphonyPlayer"/> instance.
    /// </summary>
    public static class SymphonyPlayerExtensions
    {
        public static SymphonyPlayer Symphony(this Player player)
            => player.GetModPlayer<SymphonyPlayer>();
    }
}
