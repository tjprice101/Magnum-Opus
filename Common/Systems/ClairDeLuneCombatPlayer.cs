using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MagnumOpus.Common.Systems
{
    /// <summary>
    /// Combat state tracking for Clair de Lune weapons.
    /// Tracks combo phases, stationary zones, frequency modes, and resonance effects.
    /// </summary>
    public class ClairDeLuneCombatPlayer : ModPlayer
    {
        // Chronologicality: Track current combo phase (1-4)
        public int ChronologicalityComboPhase { get; set; } = 1;
        public int ChronologicalityComboTimer { get; set; } = 0;

        // Starfall Whisper: Track fracture points (position + time created)
        public List<(Vector2 position, int generationCount, int spawnTime)> FracturePoints { get; set; }
            = new();

        // Midnight Mechanism: Track fire rate phase (1-5)
        public int MidnightMechanismPhase { get; set; } = 1;
        public int MidnightMechanismTimer { get; set; } = 0;
        public int MidnightMechanismBulletCount { get; set; } = 0;

        // Requiem of Time: Track active zones (Forward and/or Reverse)
        public bool HasForwardZone { get; set; } = false;
        public bool HasReverseZone { get; set; } = false;
        public Vector2 ForwardZoneCenter { get; set; } = Vector2.Zero;
        public Vector2 ReverseZoneCenter { get; set; } = Vector2.Zero;
        public int ForwardZoneTimer { get; set; } = 0;
        public int ReverseZoneTimer { get; set; } = 0;

        // Orrery of Dreams: Track when all 3 orbs align (convergence timer)
        public int OrreryConvergenceTimer { get; set; } = 0;

        // Automaton's Tuning Fork: Track current frequency and resonance timer
        public int AutomatonFrequency { get; set; } = 0; // 0=A, 1=C, 2=E, 3=G
        public int AutomatonFrequencyChangeTime { get; set; } = 0;
        public int AutomatonPerfectResonanceTimer { get; set; } = 0;

        // Gear-Driven Arbiter: Track verdict stacks per NPC (indexed by NPC index)
        public Dictionary<int, int> ArbiterVerdictStacks { get; set; } = new();

        public override void ResetEffects()
        {
            // Decay timers
            if (ChronologicalityComboTimer > 0)
                ChronologicalityComboTimer--;
            else
                ChronologicalityComboPhase = 1;

            if (MidnightMechanismTimer > 0)
                MidnightMechanismTimer--;

            if (ForwardZoneTimer > 0)
                ForwardZoneTimer--;
            else
                HasForwardZone = false;

            if (ReverseZoneTimer > 0)
                ReverseZoneTimer--;
            else
                HasReverseZone = false;

            if (OrreryConvergenceTimer > 0)
                OrreryConvergenceTimer--;

            if (AutomatonPerfectResonanceTimer > 0)
                AutomatonPerfectResonanceTimer--;

            // Clean up old fracture points (>1.5s old = 90 frames)
            for (int i = FracturePoints.Count - 1; i >= 0; i--)
            {
                if (Main.GameUpdateCount - FracturePoints[i].spawnTime > 90)
                    FracturePoints.RemoveAt(i);
            }

            // Clean up verdict stacks for dead NPCs
            var deadKeys = new List<int>();
            foreach (var kvp in ArbiterVerdictStacks)
            {
                if (kvp.Key < 0 || kvp.Key >= Main.maxNPCs || !Main.npc[kvp.Key].active)
                    deadKeys.Add(kvp.Key);
            }
            foreach (int key in deadKeys)
                ArbiterVerdictStacks.Remove(key);
        }

        public void ResetAll()
        {
            ChronologicalityComboPhase = 1;
            ChronologicalityComboTimer = 0;
            FracturePoints.Clear();
            MidnightMechanismPhase = 1;
            MidnightMechanismTimer = 0;
            MidnightMechanismBulletCount = 0;
            HasForwardZone = false;
            HasReverseZone = false;
            ForwardZoneTimer = 0;
            ReverseZoneTimer = 0;
            OrreryConvergenceTimer = 0;
            AutomatonFrequency = 0;
            AutomatonFrequencyChangeTime = 0;
            AutomatonPerfectResonanceTimer = 0;
            ArbiterVerdictStacks.Clear();
        }
    }
}
