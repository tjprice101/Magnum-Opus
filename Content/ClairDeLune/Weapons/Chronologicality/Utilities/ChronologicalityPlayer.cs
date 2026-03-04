using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.Chronologicality.Utilities
{
    /// <summary>
    /// Per-player state tracking for Chronologicality's 3-phase clock combo,
    /// temporal echo system, and Clockwork Overflow mechanic.
    /// </summary>
    public class ChronologicalityPlayer : ModPlayer
    {
        // --- Combo State ---
        /// <summary>Current combo phase: 0=Hour, 1=Minute, 2=Second</summary>
        public int ComboPhase { get; set; }

        /// <summary>Whether the last swing completed its full arc (no interruption)</summary>
        public bool LastSwingCompleted { get; set; }

        /// <summary>Timer that resets combo if player waits too long between swings</summary>
        public int ComboResetTimer { get; set; }
        public const int ComboResetTime = 90; // 1.5 seconds

        /// <summary>Number of consecutive perfect 3-phase cycles completed</summary>
        public int PerfectCycles { get; set; }

        // --- Clockwork Overflow ---
        /// <summary>Whether the player can trigger Clockwork Overflow (completed a perfect H→M→S cycle)</summary>
        public bool CanTriggerOverflow => PerfectCycles > 0;

        /// <summary>Cooldown after Overflow triggers</summary>
        public int OverflowCooldown { get; set; }
        public const int OverflowCooldownTime = 180; // 3 seconds

        // --- Weapon Hold ---
        public bool HoldingWeapon { get; set; }

        public override void PostUpdate()
        {
            // Tick down combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    // Combo expired — reset to Hour Hand
                    ComboPhase = 0;
                    PerfectCycles = 0;
                }
            }

            // Tick down overflow cooldown
            if (OverflowCooldown > 0)
                OverflowCooldown--;

            HoldingWeapon = false;
        }

        /// <summary>
        /// Called when a swing completes its full arc. Advances the combo phase.
        /// </summary>
        public void AdvanceCombo()
        {
            LastSwingCompleted = true;
            ComboPhase = (ComboPhase + 1) % 3;
            ComboResetTimer = ComboResetTime;

            // If we just completed a Second Hand (phase went back to 0), count a perfect cycle
            if (ComboPhase == 0)
                PerfectCycles++;
        }

        /// <summary>
        /// Called when Clockwork Overflow is triggered. Resets combo state.
        /// </summary>
        public void ConsumeOverflow()
        {
            PerfectCycles = 0;
            ComboPhase = 0;
            OverflowCooldown = OverflowCooldownTime;
        }
    }

    public static class ChronologicalityPlayerExtensions
    {
        public static ChronologicalityPlayer ChronologicalityState(this Player player)
            => player.GetModPlayer<ChronologicalityPlayer>();
    }
}
