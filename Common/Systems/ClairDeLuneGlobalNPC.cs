using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Per-NPC tracking for Clair de Lune weapon effects.
    /// Tracks debuffs, damage stacking, and special state per NPC.
    /// </summary>
    public class ClairDeLuneGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // Starfall Whisper: Track if this NPC was hit by a Starfall Whisper orb
        public bool IsStarfallWhisperTarget { get; set; } = false;

        // Requiem of Time: Track zone effects on this NPC
        public bool InForwardZone { get; set; } = false;
        public bool InReverseZone { get; set; } = false;

        // Gear-Driven Arbiter: Track verdict stacks on this NPC
        public int ArbiterVerdictStacks { get; set; } = 0;
        public int VerdictStackTimer { get; set; } = 0;

        // Automaton's Tuning Fork: Track active frequency effects
        public int AutomatonActiveFrequencies { get; set; } = 0; // Bitfield for active frequencies

        public override void ResetEffects(NPC npc)
        {
            IsStarfallWhisperTarget = false;
            InForwardZone = false;
            InReverseZone = false;

            // Decay verdict stacks
            if (VerdictStackTimer > 0)
                VerdictStackTimer--;
            else if (ArbiterVerdictStacks > 0)
                ArbiterVerdictStacks--;

            AutomatonActiveFrequencies = 0;
        }
    }
}
