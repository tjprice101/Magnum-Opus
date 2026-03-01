using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.TheConductorsLastConstellation.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking 3-phase combo state (3 orchestral movements),
    /// swing counter, convergence cooldown, and beam tracking for
    /// The Conductor's Last Constellation.
    ///
    /// Phase 0 = Downbeat (descending sweep)
    /// Phase 1 = Crescendo (rising sweep)
    /// Phase 2 = Forte (wide horizontal + convergence)
    /// </summary>
    public class ConstellationConductorPlayer : ModPlayer
    {
        /// <summary>Current combo phase (0=Downbeat, 1=Crescendo, 2=Forte).</summary>
        public int ComboPhase;

        /// <summary>Total swing counter for visual escalation.</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 90 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 90;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether this swing triggered the Convergence (3rd combo hit).</summary>
        public bool JustTriggeredConvergence;

        /// <summary>Cooldown before the next Convergence can trigger.</summary>
        public int ConvergenceCooldown;

        /// <summary>Number of active conductor beams currently alive.</summary>
        public int ActiveBeamCount;

        /// <summary>Direction alternator for swing variety.</summary>
        public int SwingDirection => SwingCounter % 2 == 0 ? 1 : -1;

        public override void ResetEffects()
        {
            JustTriggeredConvergence = false;
        }

        public override void PostUpdate()
        {
            if (ComboResetTimer > 0)
            {
                ComboResetTimer--;
                if (ComboResetTimer <= 0)
                {
                    ComboPhase = 0;
                    ComboIntensity = 0f;
                }
            }

            if (ConvergenceCooldown > 0)
                ConvergenceCooldown--;

            // Decay combo intensity slowly
            ComboIntensity *= 0.993f;
        }

        /// <summary>
        /// Called on each swing. Returns the current combo phase before advancing.
        /// Triggers Convergence on phase 2 (Forte).
        /// </summary>
        public int OnSwing()
        {
            int currentPhase = ComboPhase;
            SwingCounter++;
            ComboResetTimer = ComboResetDelay;

            // Build intensity with each swing
            ComboIntensity = MathHelper.Clamp(ComboIntensity + 0.3f, 0f, 1f);

            // Advance combo phase
            ComboPhase++;
            if (ComboPhase >= 3)
            {
                ComboPhase = 0;

                // Convergence triggers on the 3rd hit completion
                if (ConvergenceCooldown <= 0)
                {
                    JustTriggeredConvergence = true;
                    ConvergenceCooldown = 45; // 0.75 second cooldown
                    ComboIntensity = 1f;
                }
            }

            return currentPhase;
        }
    }

    public static class ConstellationConductorPlayerExtensions
    {
        public static ConstellationConductorPlayer Conductor(this Player player)
            => player.GetModPlayer<ConstellationConductorPlayer>();
    }
}
