using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Utilities
{
    /// <summary>
    /// Per-player state for Wrath's Cleaver combo system.
    /// Tracks swing phase, combo counter, and wrath meter.
    /// </summary>
    public class WrathsCleaverPlayer : ModPlayer
    {
        /// <summary>Current combo step (0-4). Resets after timeout.</summary>
        public int ComboStep;

        /// <summary>Frames since last swing. Combo resets after 90 ticks.</summary>
        public int ComboResetTimer;

        /// <summary>Wrath meter: builds with hits, triggers infernal eruption at max.</summary>
        public float WrathMeter;

        /// <summary>Max wrath before eruption trigger.</summary>
        public const float MaxWrath = 100f;

        /// <summary>Whether the player is currently in a dash-lunge state.</summary>
        public bool IsLunging;

        /// <summary>Direction of lunge for rendering.</summary>
        public Microsoft.Xna.Framework.Vector2 LungeDirection;

        public override void ResetEffects()
        {
            ComboResetTimer++;
            if (ComboResetTimer > 90)
            {
                ComboStep = 0;
                ComboResetTimer = 0;
            }

            // Wrath decays slowly when not attacking
            if (WrathMeter > 0f && ComboResetTimer > 30)
                WrathMeter -= 0.3f;
            if (WrathMeter < 0f) WrathMeter = 0f;

            if (IsLunging && ComboResetTimer > 15)
                IsLunging = false;
        }

        /// <summary>
        /// Advance the combo and reset the timer.
        /// Returns the new combo step.
        /// </summary>
        public int AdvanceCombo()
        {
            ComboStep = (ComboStep + 1) % 5;
            ComboResetTimer = 0;
            return ComboStep;
        }

        /// <summary>
        /// Add wrath from a hit. Returns true if eruption threshold reached.
        /// </summary>
        public bool AddWrath(float amount)
        {
            WrathMeter += amount;
            if (WrathMeter >= MaxWrath)
            {
                WrathMeter = 0f;
                return true;
            }
            return false;
        }
    }

    public static class WrathsCleaverPlayerExtension
    {
        public static WrathsCleaverPlayer WrathsCleaver(this Player player)
            => player.GetModPlayer<WrathsCleaverPlayer>();
    }
}
