using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.ChromaticSwanSong.Utilities
{
    /// <summary>
    /// Per-player state tracking for Chromatic Swan Song.
    /// Tracks aria charge buildup and harmonic stack per-enemy.
    /// </summary>
    public class ChromaticSwanPlayer : ModPlayer
    {
        /// <summary>How many unique enemies hit since last aria release.</summary>
        public int HarmonicStack;

        /// <summary>Timer to decay harmonic stack if not attacking.</summary>
        public int StackDecayTimer;

        /// <summary>Number of consecutive hits on the same target.</summary>
        public int ConsecutiveHits;

        /// <summary>Who we last hit.</summary>
        public int LastTargetWhoAmI = -1;

        /// <summary>Aria detonation is ready when ConsecutiveHits reaches 3 on same target.</summary>
        public bool AriaReady => ConsecutiveHits >= 3;

        public override void PostUpdate()
        {
            // Decay harmonic stack
            StackDecayTimer++;
            if (StackDecayTimer > 120) // 2 seconds of inactivity
            {
                HarmonicStack = Math.Max(0, HarmonicStack - 1);
                StackDecayTimer = 90;
            }
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

        public override void OnRespawn() { HarmonicStack = 0; ConsecutiveHits = 0; LastTargetWhoAmI = -1; }
    }

    public static class ChromaticSwanPlayerExt
    {
        public static ChromaticSwanPlayer ChromaticSwan(this Player player)
            => player.GetModPlayer<ChromaticSwanPlayer>();
    }
}
