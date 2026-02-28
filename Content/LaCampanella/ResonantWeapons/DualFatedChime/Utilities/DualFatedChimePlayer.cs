using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities
{
    /// <summary>
    /// Per-player state tracking for Dual Fated Chime.
    /// Tracks Inferno Waltz charge, combo state, and cooldowns.
    /// </summary>
    public class DualFatedChimePlayer : ModPlayer
    {
        #region Inferno Waltz Charge System

        /// <summary>Current charge bar value (0 → MaxCharge).</summary>
        public float ChargeBar;

        /// <summary>Maximum charge value.</summary>
        public const float MaxCharge = 100f;

        /// <summary>Charge gained per hit.</summary>
        public const float ChargePerHit = 8f;

        /// <summary>Whether charge bar is full and Inferno Waltz is ready.</summary>
        public bool IsWaltzReady => ChargeBar >= MaxCharge;

        /// <summary>Whether currently performing Inferno Waltz.</summary>
        public bool IsPerformingWaltz;

        /// <summary>Remaining ticks of Inferno Waltz buff.</summary>
        public int WaltzBuffTimer;

        /// <summary>Duration of Inferno Waltz movement buff in ticks (15 seconds).</summary>
        public const int WaltzBuffDuration = 900;

        #endregion

        #region Combo Tracking

        /// <summary>Current combo step (0=BellStrike, 1=TollSweep, 2=GrandToll).</summary>
        public int ComboStep;

        /// <summary>Ticks since last swing, for combo reset.</summary>
        public int ComboResetTimer;

        /// <summary>Frames of inactivity before combo resets.</summary>
        public const int ComboResetDelay = 50;

        #endregion

        public override void ResetEffects()
        {
            // Tick waltz buff
            if (WaltzBuffTimer > 0)
            {
                WaltzBuffTimer--;
                Player.moveSpeed += 0.35f;
                Player.maxRunSpeed += 3f;
                if (WaltzBuffTimer <= 0)
                    IsPerformingWaltz = false;
            }

            // Tick combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }
        }

        /// <summary>Add charge from hitting enemies.</summary>
        public void AddCharge(float amount)
        {
            ChargeBar = System.Math.Min(ChargeBar + amount, MaxCharge);
        }

        /// <summary>Consume charge to begin Inferno Waltz.</summary>
        public void ConsumeCharge()
        {
            ChargeBar = 0f;
            IsPerformingWaltz = true;
            WaltzBuffTimer = WaltzBuffDuration;
        }

        /// <summary>Advance the combo step and reset the timer.</summary>
        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 3;
            ComboResetTimer = ComboResetDelay;
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class DualFatedChimePlayerExtensions
    {
        public static DualFatedChimePlayer DualFatedChime(this Player player)
            => player.GetModPlayer<DualFatedChimePlayer>();
    }
}
