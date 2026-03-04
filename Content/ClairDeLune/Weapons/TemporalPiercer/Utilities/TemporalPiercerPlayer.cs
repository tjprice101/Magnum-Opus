using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Weapons.TemporalPiercer.Utilities
{
    /// <summary>
    /// Tracks Temporal Puncture marks per NPC for the Temporal Piercer weapon.
    /// Max 5 stacks on any NPC, 8 seconds per stack. At 5 → Frozen Moment triggers.
    /// </summary>
    public class TemporalPiercerPlayer : ModPlayer
    {
        /// <summary>
        /// NPC index → (stack count, remaining frames per stack)
        /// </summary>
        private readonly Dictionary<int, PunctureData> _punctureMarks = new();

        private const int MaxStacks = 5;
        private const int StackDurationFrames = 480; // 8 seconds

        public struct PunctureData
        {
            public int Stacks;
            public int[] FramesRemaining; // Per-stack countdown
            public float AccumulatedDamage; // Total damage dealt for Frozen Moment calculation

            public PunctureData(int stacks, int duration, float damage)
            {
                Stacks = stacks;
                FramesRemaining = new int[MaxStacks];
                for (int i = 0; i < stacks; i++)
                    FramesRemaining[i] = duration;
                AccumulatedDamage = damage;
            }
        }

        /// <summary>
        /// Add a puncture mark to an NPC. Returns current stack count.
        /// </summary>
        public int AddPunctureMark(int npcIndex, float damage)
        {
            if (!_punctureMarks.TryGetValue(npcIndex, out var data))
            {
                data = new PunctureData(0, 0, 0f);
                data.FramesRemaining = new int[MaxStacks];
            }

            if (data.Stacks < MaxStacks)
            {
                data.FramesRemaining[data.Stacks] = StackDurationFrames;
                data.Stacks++;
            }
            else
            {
                // Refresh oldest stack
                data.FramesRemaining[0] = StackDurationFrames;
            }

            data.AccumulatedDamage += damage;
            _punctureMarks[npcIndex] = data;
            return data.Stacks;
        }

        /// <summary>
        /// Get current puncture stacks on an NPC.
        /// </summary>
        public int GetStacks(int npcIndex)
        {
            return _punctureMarks.TryGetValue(npcIndex, out var data) ? data.Stacks : 0;
        }

        /// <summary>
        /// Get accumulated damage for Frozen Moment calculation.
        /// </summary>
        public float GetAccumulatedDamage(int npcIndex)
        {
            return _punctureMarks.TryGetValue(npcIndex, out var data) ? data.AccumulatedDamage : 0f;
        }

        /// <summary>
        /// Consume all marks on an NPC (triggers Frozen Moment).
        /// Returns accumulated damage × 2.
        /// </summary>
        public float ConsumeAllMarks(int npcIndex)
        {
            if (!_punctureMarks.TryGetValue(npcIndex, out var data))
                return 0f;

            float burstDamage = data.AccumulatedDamage * 2f;
            _punctureMarks.Remove(npcIndex);
            return burstDamage;
        }

        public override void PostUpdate()
        {
            // Decay per-stack timers
            var expired = new List<int>();
            var keys = new List<int>(_punctureMarks.Keys);

            foreach (int npcIndex in keys)
            {
                var data = _punctureMarks[npcIndex];

                // Remove stacks whose timer expired (from highest down)
                for (int i = data.Stacks - 1; i >= 0; i--)
                {
                    data.FramesRemaining[i]--;
                    if (data.FramesRemaining[i] <= 0)
                    {
                        // Shift down
                        for (int j = i; j < data.Stacks - 1; j++)
                            data.FramesRemaining[j] = data.FramesRemaining[j + 1];
                        data.Stacks--;
                    }
                }

                if (data.Stacks <= 0)
                    expired.Add(npcIndex);
                else
                    _punctureMarks[npcIndex] = data;
            }

            foreach (int key in expired)
                _punctureMarks.Remove(key);
        }
    }
}
