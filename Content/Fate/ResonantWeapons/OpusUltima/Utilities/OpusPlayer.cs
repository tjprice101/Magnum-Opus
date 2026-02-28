using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.OpusUltima.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking combo state for Opus Ultima.
    /// 3-movement combo cycle: Exposition → Development → Recapitulation.
    /// Extension method: player.Opus()
    /// </summary>
    public class OpusPlayer : ModPlayer
    {
        /// <summary>Current swing count for combo tracking (0-2, cycles through 3 movements).</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 120 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 120;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Total attacks performed (for escalating effects).</summary>
        public int TotalAttacks;

        /// <summary>Current musical movement (0=Exposition, 1=Development, 2=Recapitulation).</summary>
        public int CurrentMovement => SwingCounter % 3;

        /// <summary>Whether the current swing just triggered the Recapitulation (Movement III).</summary>
        public bool JustTriggeredRecap;

        /// <summary>Whether energy balls have been fired this swing.</summary>
        public bool EnergyBallsFired;

        public override void ResetEffects()
        {
            JustTriggeredRecap = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    SwingCounter = 0;
                    ComboIntensity = 0f;
                }
            }

            // Decay combo intensity slowly
            ComboIntensity *= 0.995f;
        }

        /// <summary>
        /// Called on each swing. Returns the movement index (0-2) for this swing.
        /// Also sets JustTriggeredRecap if this is Movement III.
        /// </summary>
        public int OnSwing()
        {
            int movement = CurrentMovement;
            SwingCounter++;
            TotalAttacks++;
            ComboResetTimer = ComboResetDelay;
            EnergyBallsFired = false;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.33f, 0f, 1f);

            if (movement == 2)
            {
                JustTriggeredRecap = true;
                ComboIntensity = 1f; // Max intensity on Recapitulation
            }

            return movement;
        }
    }

    public static class OpusPlayerExtensions
    {
        public static OpusPlayer Opus(this Player player)
            => player.GetModPlayer<OpusPlayer>();
    }
}
