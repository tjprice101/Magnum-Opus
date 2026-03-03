using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.DualFatedChime.Utilities
{
    /// <summary>
    /// Per-player state tracking for Dual Fated Chime.
    /// Tracks 5-phase inferno waltz combo and Bell Resonance state.
    /// </summary>
    public class DualFatedChimePlayer : ModPlayer
    {
        #region Combo Tracking (5-Phase Inferno Waltz)

        /// <summary>Current combo step:
        /// 0 = Opening Peal (right horizontal)
        /// 1 = Answer (left diagonal, faster) 
        /// 2 = Escalation (right upward arc + flame wave)
        /// 3 = Resonance (left downward slam + double shockwave + ground fire)
        /// 4 = Grand Toll (cross-slash + 12 directional flame waves)
        /// </summary>
        public int ComboStep;

        /// <summary>Number of combo phases.</summary>
        public const int ComboPhaseCount = 5;

        /// <summary>Ticks since last swing, for combo reset.</summary>
        public int ComboResetTimer;

        /// <summary>Frames of inactivity before combo resets.</summary>
        public const int ComboResetDelay = 60;

        /// <summary>Flame Waltz Dodge: i-frames granted after completing all 5 combo phases.</summary>
        public int WaltzBuffTimer;

        #endregion

        public override void ResetEffects()
        {
            // Tick combo reset
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                    ComboStep = 0;
            }

            // Flame Waltz Dodge: brief invulnerability
            if (WaltzBuffTimer > 0)
            {
                WaltzBuffTimer--;
                Player.immune = true;
                Player.immuneTime = 2;
            }
        }

        /// <summary>Advance the combo step (0-4) and reset the timer.</summary>
        public void AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % ComboPhaseCount;
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
