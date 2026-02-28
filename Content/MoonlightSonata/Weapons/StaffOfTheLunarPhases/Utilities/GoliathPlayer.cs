using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities
{
    /// <summary>
    /// Per-player state tracker for Staff of the Lunar Phases — "The Conductor's Baton".
    /// Tracks Conductor Mode state, beam targeting, and summon circle animation phase.
    /// </summary>
    public sealed class GoliathPlayer : ModPlayer
    {
        // =================================================================
        // CONDUCTOR MODE
        // =================================================================

        /// <summary>Whether Conductor Mode is active (right-click toggle).</summary>
        public bool ConductorMode;

        /// <summary>World-space cursor position when in Conductor Mode.</summary>
        public Vector2 ConductorTarget;

        /// <summary>Visual pulse timer for Conductor Mode indicator (0..1 cycling).</summary>
        public float ConductorPulse;

        // =================================================================
        // SUMMON CIRCLE ANIMATION
        // =================================================================

        /// <summary>Ritual phase for summon circle shader (0=dormant, 1=fully active). Decays over time.</summary>
        public float RitualPhase;

        /// <summary>Timer tracking how long the summon circle has been active.</summary>
        public int RitualTimer;

        /// <summary>Duration of summon circle animation in ticks.</summary>
        public const int RitualDuration = 40;

        // =================================================================
        // BEAM TRACKING
        // =================================================================

        /// <summary>Counter for total beams fired — every 3rd beam in Conductor Mode is devastating.</summary>
        public int BeamCounter;

        /// <summary>Whether the next Conductor Mode beam should be devastating.</summary>
        public bool NextBeamIsDevastating => ConductorMode && (BeamCounter % 3 == 2);

        // =================================================================
        // LIFECYCLE
        // =================================================================

        public override void ResetEffects()
        {
            // Conductor mode persists until toggled off — no reset needed
        }

        public override void PostUpdate()
        {
            // Update conductor mode pulse
            if (ConductorMode)
            {
                ConductorPulse += 0.03f;
                if (ConductorPulse > 1f)
                    ConductorPulse -= 1f;

                ConductorTarget = Main.MouseWorld;
            }
            else
            {
                ConductorPulse = 0f;
            }

            // Decay ritual phase
            if (RitualTimer > 0)
            {
                RitualTimer--;
                RitualPhase = RitualTimer / (float)RitualDuration;
            }
            else
            {
                RitualPhase = 0f;
            }
        }

        /// <summary>Toggle Conductor Mode on/off.</summary>
        public void ToggleConductorMode()
        {
            ConductorMode = !ConductorMode;
            if (ConductorMode)
                ConductorTarget = Main.MouseWorld;
        }

        /// <summary>Trigger summon circle animation (called when summoning the Goliath).</summary>
        public void TriggerRitual()
        {
            RitualTimer = RitualDuration;
            RitualPhase = 1f;
        }

        /// <summary>Increment beam counter (called when Goliath fires a beam).</summary>
        public void OnBeamFired()
        {
            BeamCounter++;
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class GoliathPlayerExtensions
    {
        public static GoliathPlayer Goliath(this Player player)
            => player.GetModPlayer<GoliathPlayer>();
    }
}
