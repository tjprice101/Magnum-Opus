using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Utilities
{
    /// <summary>
    /// Per-player state tracking for Call of the Black Swan.
    /// Tracks empowerment, combo state, and cooldowns.
    /// </summary>
    public class BlackSwanPlayer : ModPlayer
    {
        #region Empowerment System

        /// <summary>Number of flare hits accumulated toward empowerment.</summary>
        public int FlareHitCount;

        /// <summary>Whether the next swing is empowered.</summary>
        public bool IsEmpowered;

        /// <summary>Remaining ticks of empowerment window.</summary>
        public int EmpowermentTimer;

        /// <summary>Flares needed to trigger empowerment.</summary>
        public const int FlaresNeeded = 3;

        /// <summary>Duration of empowerment window in ticks (5 seconds).</summary>
        public const int EmpowermentDuration = 300;

        #endregion

        #region Combo Tracking

        /// <summary>Current combo step (0, 1, or 2).</summary>
        public int ComboStep;

        /// <summary>Ticks since last swing, for combo reset.</summary>
        public int ComboResetTimer;

        /// <summary>Frames of inactivity before combo resets.</summary>
        public const int ComboResetDelay = 45;

        #endregion

        public override void ResetEffects()
        {
            // Tick down empowerment
            if (EmpowermentTimer > 0)
            {
                EmpowermentTimer--;
                if (EmpowermentTimer <= 0)
                {
                    IsEmpowered = false;
                    FlareHitCount = 0;
                }
            }

            // Tick down combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }
        }

        /// <summary>Register a flare hit. Triggers empowerment at threshold.</summary>
        public void RegisterFlareHit()
        {
            FlareHitCount++;
            if (FlareHitCount >= FlaresNeeded)
            {
                IsEmpowered = true;
                EmpowermentTimer = EmpowermentDuration;
                FlareHitCount = 0;
            }
        }

        /// <summary>Consume the empowerment for an empowered swing.</summary>
        public void ConsumeEmpowerment()
        {
            IsEmpowered = false;
            EmpowermentTimer = 0;
        }

        /// <summary>Advance the combo step and reset the timer.</summary>
        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 3;
            ComboResetTimer = ComboResetDelay;
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class BlackSwanPlayerExtensions
    {
        public static BlackSwanPlayer BlackSwan(this Player player)
            => player.GetModPlayer<BlackSwanPlayer>();
    }
}
