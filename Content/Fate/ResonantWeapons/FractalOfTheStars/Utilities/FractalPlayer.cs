using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.FractalOfTheStars.Utilities
{
    /// <summary>
    /// Per-weapon ModPlayer tracking 3-phase combo state, swing counter,
    /// star fracture cooldown, and orbit blade tracking for Fractal of the Stars.
    /// </summary>
    public class FractalPlayer : ModPlayer
    {
        /// <summary>Current combo phase (0 = Horizontal Sweep, 1 = Rising Uppercut, 2 = Gravity Slam).</summary>
        public int ComboPhase;

        /// <summary>Total swing counter for visual escalation.</summary>
        public int SwingCounter;

        /// <summary>Ticks since last swing. Combo resets after 90 ticks of inactivity.</summary>
        public int ComboResetTimer;
        private const int ComboResetDelay = 90;

        /// <summary>Current combo intensity (0..1). Grows with each swing, used for VFX scaling.</summary>
        public float ComboIntensity;

        /// <summary>Whether this swing triggered the Star Fracture (3rd combo hit).</summary>
        public bool JustTriggeredStarFracture;

        /// <summary>Cooldown before the next Star Fracture can trigger.</summary>
        public int StarFractureCooldown;

        /// <summary>Number of orbiting blades currently active.</summary>
        public int OrbitBladeCount;

        /// <summary>Direction alternator for swing variety.</summary>
        public int SwingDirection => SwingCounter % 2 == 0 ? 1 : -1;

        public override void ResetEffects()
        {
            JustTriggeredStarFracture = false;
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

            if (StarFractureCooldown > 0)
                StarFractureCooldown--;

            // Decay combo intensity slowly
            ComboIntensity *= 0.993f;
        }

        /// <summary>
        /// Called on each swing. Returns the current combo phase before advancing.
        /// Triggers Star Fracture on phase 2 (Gravity Slam).
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

                // Star Fracture triggers on the 3rd hit completion
                if (StarFractureCooldown <= 0)
                {
                    JustTriggeredStarFracture = true;
                    StarFractureCooldown = 45; // 0.75 second cooldown
                    ComboIntensity = 1f;
                }
            }

            return currentPhase;
        }
    }

    public static class FractalPlayerExtensions
    {
        public static FractalPlayer Fractal(this Player player)
            => player.GetModPlayer<FractalPlayer>();
    }
}
