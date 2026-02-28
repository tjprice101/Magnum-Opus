using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.DestinysCrescendo
{
    /// <summary>
    /// Per-player state for Destiny's Crescendo.
    /// Tracks active cosmic deity minion count and summoning intensity.
    /// </summary>
    public class CrescendoPlayer : ModPlayer
    {
        /// <summary>Number of CrescendoDeityMinion projectiles currently alive.</summary>
        public int ActiveDeityCount;

        /// <summary>True while the staff is held (enables ambient VFX).</summary>
        public bool IsHoldingStaff;

        /// <summary>0-1 intensity that ramps up during active summoning.</summary>
        public float SummonIntensity;

        public override void ResetEffects()
        {
            IsHoldingStaff = false;

            // Decay summon intensity when not actively summoning
            if (ActiveDeityCount <= 0)
                SummonIntensity = MathHelper.Lerp(SummonIntensity, 0f, 0.05f);
        }

        /// <summary>Called by the item each time a deity is summoned.</summary>
        public void OnSummon()
        {
            SummonIntensity = MathHelper.Clamp(SummonIntensity + 0.25f, 0f, 1f);
        }
    }

    /// <summary>
    /// Extension: <c>player.Crescendo()</c> returns the <see cref="CrescendoPlayer"/> instance.
    /// </summary>
    public static class CrescendoPlayerExtensions
    {
        public static CrescendoPlayer Crescendo(this Player player)
            => player.GetModPlayer<CrescendoPlayer>();
    }
}
