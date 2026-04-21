using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Systems
{
    /// <summary>
    /// Tracks recent mine detonation timestamps for Staff of Final Judgment's Judgment Storm mechanic.
    /// If 3+ mines detonate within 60 frames, triggers Judgment Storm (5 children instead of 3).
    /// </summary>
    public class JudgmentStormTracker : ModSystem
    {
        private static readonly List<int> _recentDetonations = new();

        public override void PostUpdateProjectiles()
        {
            // Clean up old detonation timestamps
            int currentTime = (int)Main.GameUpdateCount;
            _recentDetonations.RemoveAll(t => currentTime - t > 60);
        }

        /// <summary>
        /// Records a mine detonation and returns whether Judgment Storm should trigger.
        /// </summary>
        public static bool RecordDetonation()
        {
            int currentTime = (int)Main.GameUpdateCount;
            _recentDetonations.Add(currentTime);

            // Count detonations within the last 60 frames
            int count = 0;
            for (int i = _recentDetonations.Count - 1; i >= 0; i--)
            {
                if (currentTime - _recentDetonations[i] <= 60)
                    count++;
            }

            return count >= 3;
        }

        public override void OnWorldUnload()
        {
            _recentDetonations.Clear();
        }
    }
}
