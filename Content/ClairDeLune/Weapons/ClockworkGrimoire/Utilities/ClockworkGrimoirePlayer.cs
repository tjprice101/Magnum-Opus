using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.ClockworkGrimoire.Utilities
{
    /// <summary>
    /// Tracks Clockwork Grimoire's 4-mode cycle and Temporal Synergy.
    /// Modes: 0=Hour, 1=Minute, 2=Second, 3=Pendulum
    /// </summary>
    public class ClockworkGrimoirePlayer : ModPlayer
    {
        /// <summary>Current active mode (0-3).</summary>
        public int CurrentMode;

        /// <summary>Modes used in sequence for Temporal Synergy (tracks H→M→S→P).</summary>
        public bool[] SynergyProgress = new bool[4];

        /// <summary>Whether next cast is synergy-enhanced (50% boost).</summary>
        public bool SynergyActive;

        /// <summary>Cycle mode forward on alt-fire.</summary>
        public void CycleMode()
        {
            // Mark current mode as used for synergy
            SynergyProgress[CurrentMode] = true;

            CurrentMode = (CurrentMode + 1) % 4;

            // Check for full synergy sequence
            if (SynergyProgress[0] && SynergyProgress[1] && SynergyProgress[2] && SynergyProgress[3])
            {
                SynergyActive = true;
                SynergyProgress = new bool[4];
            }
        }

        /// <summary>Consume synergy boost if active.</summary>
        public bool ConsumeSynergy()
        {
            if (SynergyActive)
            {
                SynergyActive = false;
                return true;
            }
            return false;
        }

        public string GetModeName()
        {
            return CurrentMode switch
            {
                0 => "Hour",
                1 => "Minute",
                2 => "Second",
                3 => "Pendulum",
                _ => "Hour"
            };
        }

        public Color GetModeColor()
        {
            return CurrentMode switch
            {
                0 => ClairDeLunePalette.MoonbeamGold,     // Hour — gold sustained
                1 => ClairDeLunePalette.SoftBlue,         // Minute — scattered blue
                2 => ClairDeLunePalette.PearlFrost,       // Second — rapid frost
                3 => ClairDeLunePalette.ClockworkBrass,   // Pendulum — swinging brass
                _ => ClairDeLunePalette.SoftBlue
            };
        }

        public override void OnRespawn()
        {
            CurrentMode = 0;
            SynergyActive = false;
            SynergyProgress = new bool[4];
        }
    }
}
