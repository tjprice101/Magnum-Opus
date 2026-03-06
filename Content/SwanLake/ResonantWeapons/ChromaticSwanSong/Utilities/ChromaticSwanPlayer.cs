using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities
{
    /// <summary>
    /// Per-player state tracking for Chromatic Swan Song.
    /// 
    /// SYSTEMS:
    /// • Chromatic Scale — consecutive casts cycle through C-D-E-F-G-A-B (7 notes)
    /// • Opus Detonation — completing full octave (7 casts) triggers massive prismatic explosion
    /// • Harmonic Stack — hitting different enemies builds energy for enhanced arias
    /// • Dying Breath — below 30% HP: 2x bolt speed, +50% aria radius, black feathers
    /// </summary>
    public class ChromaticSwanPlayer : ModPlayer
    {
        // ── Chromatic Scale System ──
        /// <summary>Current position in the chromatic scale (0-6 = C-D-E-F-G-A-B).</summary>
        public int ChromaticScalePosition;

        /// <summary>How many consecutive casts toward completing the octave.</summary>
        public int ConsecutiveCasts;

        /// <summary>Decay timer for consecutive casts.</summary>
        public int CastDecayTimer;

        /// <summary>True when 7 consecutive casts complete the octave — next bolt triggers Opus.</summary>
        public bool OpusReady;

        // ── Harmonic Stack System (existing) ──
        /// <summary>How many unique enemies hit since last harmonic release.</summary>
        public int HarmonicStack;

        /// <summary>Timer to decay harmonic stack if not attacking.</summary>
        public int StackDecayTimer;

        /// <summary>Number of consecutive hits on the same target.</summary>
        public int ConsecutiveHits;

        /// <summary>Who we last hit.</summary>
        public int LastTargetWhoAmI = -1;

        /// <summary>Aria detonation is ready when ConsecutiveHits reaches 3 on same target.</summary>
        public bool AriaReady => ConsecutiveHits >= 3;

        /// <summary>Returns true if player is below 30% HP — activates Dying Breath.</summary>
        public bool DyingBreathActive => Player.statLife < Player.statLifeMax2 * 0.3f;

        /// <summary>
        /// Maps chromatic scale positions (0-6) to evenly-spaced hues across the spectrum.
        /// C=Red, D=Orange, E=Yellow, F=Green, G=Cyan, A=Blue, B=Purple
        /// </summary>
        public static float GetScaleHue(int position) => (position % 7) / 7f;

        /// <summary>Returns the color for a given chromatic scale position.
        /// Desaturated pastel rainbow over white base — Swan Lake prismatic identity.</summary>
        public static Color GetScaleColor(int position) => Main.hslToRgb(GetScaleHue(position), 0.5f, 0.82f);

        /// <summary>Returns the complementary color for a given scale position.</summary>
        public static Color GetComplementaryColor(int position) => Main.hslToRgb((GetScaleHue(position) + 0.5f) % 1f, 0.5f, 0.82f);

        /// <summary>Scale note names for display.</summary>
        public static readonly string[] NoteNames = { "C", "D", "E", "F", "G", "A", "B" };

        public override void PostUpdate()
        {
            // Decay harmonic stack
            StackDecayTimer++;
            if (StackDecayTimer > 120) // 2 seconds of inactivity
            {
                HarmonicStack = Math.Max(0, HarmonicStack - 1);
                StackDecayTimer = 90;
            }

            // Decay cast streak
            CastDecayTimer++;
            if (CastDecayTimer > 90) // 1.5 seconds of inactivity resets octave progress
            {
                ConsecutiveCasts = 0;
                OpusReady = false;
            }
        }

        /// <summary>Register a new cast — advances the chromatic scale.</summary>
        public void RegisterCast()
        {
            CastDecayTimer = 0;
            ConsecutiveCasts++;
            ChromaticScalePosition = (ChromaticScalePosition + 1) % 7;

            if (ConsecutiveCasts >= 7)
            {
                OpusReady = true;
                ConsecutiveCasts = 0;
            }
        }

        /// <summary>Consume the Opus Detonation state after firing the opus bolt.</summary>
        public void ConsumeOpus()
        {
            OpusReady = false;
            ConsecutiveCasts = 0;
        }

        public void RegisterHit(int targetWhoAmI)
        {
            StackDecayTimer = 0;

            if (targetWhoAmI == LastTargetWhoAmI)
            {
                ConsecutiveHits++;
            }
            else
            {
                // New target — add to harmonic stack
                HarmonicStack++;
                ConsecutiveHits = 1;
                LastTargetWhoAmI = targetWhoAmI;
            }
        }

        public void ConsumeAria()
        {
            ConsecutiveHits = 0;
        }

        public void ConsumeHarmonicStack()
        {
            HarmonicStack = 0;
        }

        public override void OnRespawn()
        {
            HarmonicStack = 0;
            ConsecutiveHits = 0;
            LastTargetWhoAmI = -1;
            ChromaticScalePosition = 0;
            ConsecutiveCasts = 0;
            OpusReady = false;
        }
    }

    public static class ChromaticSwanPlayerExt
    {
        public static ChromaticSwanPlayer ChromaticSwan(this Player player)
            => player.GetModPlayer<ChromaticSwanPlayer>();
    }
}
