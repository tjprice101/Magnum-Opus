using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.IridescentWingspan.Utilities
{
    /// <summary>
    /// Tracks per-player state for Iridescent Wingspan.
    /// Handles Ethereal Flight charge buildup, Prismatic Convergence, and Wingspan Resonance.
    /// </summary>
    public class WingspanPlayer : ModPlayer
    {
        /// <summary>Wing charge (0-100). At 100 → next cast fires empowered wing blast.</summary>
        public int WingCharge;
        public bool IsFullyCharged => WingCharge >= 100;

        /// <summary>Visual wing display timer — shows ethereal wings while holding.</summary>
        public int WingDisplayTimer;
        public bool ShowWings => WingDisplayTimer > 0;

        /// <summary>Wingspan Resonance: timer that counts down after convergence. 
        /// If player casts again while > 0, next fan gets +10% damage.</summary>
        public int ResonanceTimer;

        /// <summary>Active resonance damage bonus (0.0 - 0.10).</summary>
        public float ResonanceDamageBonus;

        /// <summary>Tracks how many bolts from the current fan have arrived at convergence point.</summary>
        public int ConvergenceCount;

        /// <summary>Total bolts fired in current fan (always 5 for normal, 1 for empowered).</summary>
        public int CurrentFanSize = 5;

        public void RegisterHit()
        {
            WingCharge = Math.Min(WingCharge + 8, 100);
        }

        public void ConsumeCharge()
        {
            WingCharge = 0;
        }

        /// <summary>Called when a bolt reaches the cursor convergence point.</summary>
        public void RegisterConvergence()
        {
            ConvergenceCount++;
        }

        /// <summary>Called when convergence burst fires. Resets counter, starts resonance timer.</summary>
        public void ConsumeConvergence()
        {
            ConvergenceCount = 0;
            ResonanceTimer = 60; // 1 second window for resonance bonus 
        }

        public override void PostUpdate()
        {
            WingDisplayTimer = Math.Max(0, WingDisplayTimer - 1);
            ResonanceTimer = Math.Max(0, ResonanceTimer - 1);
            if (ResonanceTimer <= 0)
                ResonanceDamageBonus = 0f;
        }

        public override void OnRespawn() { WingCharge = 0; ResonanceTimer = 0; ResonanceDamageBonus = 0f; ConvergenceCount = 0; }
    }

    public static class WingspanPlayerExt
    {
        public static WingspanPlayer Wingspan(this Player player) => player.GetModPlayer<WingspanPlayer>();
    }
}
