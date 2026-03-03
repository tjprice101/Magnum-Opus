using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Content.MoonlightSonata.Minions;

namespace MagnumOpus.Content.MoonlightSonata.Weapons.StaffOfTheLunarPhases.Utilities
{
    /// <summary>
    /// Per-player state tracker for Staff of the Lunar Phases — "The Conductor's Baton".
    /// Tracks Conductor Mode state, beam targeting, summon circle animation, and Lunar Phase cycling.
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
        // LUNAR PHASE SYSTEM
        // =================================================================

        /// <summary>
        /// Current lunar phase mode (0-3).
        /// 0 = New Moon (stealth/crit), 1 = Waxing (damage building),
        /// 2 = Full Moon (AoE/power), 3 = Waning (healing/regen).
        /// </summary>
        public int LunarPhaseMode;

        /// <summary>Timer for phase cycling — advances phase when full.</summary>
        public int PhaseCycleTimer;

        /// <summary>Ticks per lunar phase — 10 seconds per phase.</summary>
        public const int PhaseTickDuration = 600;

        /// <summary>Phase names for tooltip display.</summary>
        public static readonly string[] LunarPhaseNames =
            { "New Moon", "Waxing Crescent", "Full Moon", "Waning Gibbous" };

        /// <summary>Phase colors for visual feedback.</summary>
        public static readonly Color[] LunarPhaseColors =
        {
            new Color(20, 8, 40),      // New Moon — void black
            new Color(120, 80, 200),    // Waxing — growing purple
            new Color(200, 210, 255),   // Full Moon — brilliant white-blue
            new Color(160, 130, 220)    // Waning — fading lavender
        };

        /// <summary>Beam damage multiplier per phase.</summary>
        public static readonly float[] PhaseDamageMultiplier = { 1.0f, 1.15f, 1.4f, 0.8f };

        /// <summary>Beam speed multiplier per phase.</summary>
        public static readonly float[] PhaseBeamSpeedMultiplier = { 1.1f, 1.0f, 0.9f, 1.0f };

        /// <summary>Healing per beam hit per phase (Waning = high heal).</summary>
        public static readonly int[] PhaseHealAmount = { 5, 8, 10, 18 };

        /// <summary>Helper: Is it Full Moon phase?</summary>
        public bool IsFullMoon => LunarPhaseMode == 2;

        /// <summary>Helper: Is it Waning phase?</summary>
        public bool IsWaning => LunarPhaseMode == 3;

        /// <summary>Helper: Current phase color.</summary>
        public Color CurrentPhaseColor => LunarPhaseColors[LunarPhaseMode];

        /// <summary>Helper: Current phase name.</summary>
        public string CurrentPhaseName => LunarPhaseNames[LunarPhaseMode];

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

            // Advance lunar phase cycle
            UpdateLunarPhase();
        }

        private void UpdateLunarPhase()
        {
            // Only cycle if the Goliath is active (player has buff)
            bool hasGoliath = false;
            foreach (var proj in Main.ActiveProjectiles)
            {
                if (proj.owner == Player.whoAmI && proj.active && proj.minion &&
                    proj.type == ModContent.ProjectileType<Minions.GoliathOfMoonlight>())
                {
                    hasGoliath = true;
                    break;
                }
            }

            if (!hasGoliath)
            {
                PhaseCycleTimer = 0;
                return;
            }

            PhaseCycleTimer++;
            if (PhaseCycleTimer >= PhaseTickDuration)
            {
                PhaseCycleTimer = 0;
                AdvanceLunarPhase();
            }
        }

        /// <summary>Advance to the next lunar phase.</summary>
        public void AdvanceLunarPhase()
        {
            LunarPhaseMode = (LunarPhaseMode + 1) % 4;
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

        /// <summary>Increment beam counter and advance phase on every 4th beam.</summary>
        public void OnBeamFired()
        {
            BeamCounter++;

            // Advance lunar phase every 4th beam for more dynamic cycling
            if (BeamCounter % 4 == 0)
                AdvanceLunarPhase();
        }
    }

    /// <summary>Extension method for convenient access.</summary>
    public static class GoliathPlayerExtensions
    {
        public static GoliathPlayer Goliath(this Player player)
            => player.GetModPlayer<GoliathPlayer>();
    }
}
