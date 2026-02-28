using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities
{
    /// <summary>
    /// Tracks per-player state for Iridescent Wingspan.
    /// Handles Ethereal Flight charge buildup.
    /// </summary>
    public class WingspanPlayer : ModPlayer
    {
        /// <summary>Wing charge (0-100). At 100 → next cast fires empowered wing blast.</summary>
        public int WingCharge;
        public bool IsFullyCharged => WingCharge >= 100;

        /// <summary>Visual wing display timer — shows ethereal wings while holding.</summary>
        public int WingDisplayTimer;
        public bool ShowWings => WingDisplayTimer > 0;

        public void RegisterHit()
        {
            WingCharge = Math.Min(WingCharge + 8, 100);
        }

        public void ConsumeCharge()
        {
            WingCharge = 0;
        }

        public override void PostUpdate()
        {
            WingDisplayTimer = Math.Max(0, WingDisplayTimer - 1);
        }

        public override void OnRespawn() { WingCharge = 0; }
    }

    public static class WingspanPlayerExt
    {
        public static WingspanPlayer Wingspan(this Player player) => player.GetModPlayer<WingspanPlayer>();
    }
}
