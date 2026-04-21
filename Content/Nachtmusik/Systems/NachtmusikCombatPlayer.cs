using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Systems
{
    /// <summary>
    /// ModPlayer tracking weapon-specific combat state for Nachtmusik weapons.
    /// </summary>
    public class NachtmusikCombatPlayer : ModPlayer
    {
        // Serenade of Distant Stars — Rhythm Stacking
        public int SerenadeRhythmStacks;
        public int SerenadeRhythmTimer;
        public const int MaxRhythmStacks = 5;
        public const int RhythmDecayFrames = 120;

        // Requiem of the Cosmos — Shot Counter
        public int RequiemShotCounter;

        // Constellation Piercer — Star Mapping
        public Vector2[] ConstellationMarkers = new Vector2[3];
        public int ConstellationMarkerCount;

        // Midnight's Crescendo — Combo Phase
        public int MidnightCrescendoComboPhase;

        public override void ResetEffects()
        {
            // Rhythm stacking decay
            if (SerenadeRhythmTimer > 0)
            {
                SerenadeRhythmTimer--;
                if (SerenadeRhythmTimer <= 0)
                {
                    SerenadeRhythmStacks = 0;
                }
            }
        }

        /// <summary>
        /// Increments rhythm stacks and resets the decay timer.
        /// Called when a Serenade orb hits an enemy.
        /// </summary>
        public void IncrementRhythm()
        {
            if (SerenadeRhythmStacks < MaxRhythmStacks)
                SerenadeRhythmStacks++;
            SerenadeRhythmTimer = RhythmDecayFrames;
        }

        /// <summary>
        /// Adds a constellation marker position. Triggers detonation at 3 marks.
        /// Returns true if 3 markers are now filled.
        /// </summary>
        public bool AddConstellationMarker(Vector2 position)
        {
            if (ConstellationMarkerCount < 3)
            {
                ConstellationMarkers[ConstellationMarkerCount] = position;
                ConstellationMarkerCount++;
            }
            return ConstellationMarkerCount >= 3;
        }

        /// <summary>
        /// Resets constellation markers after detonation.
        /// </summary>
        public void ResetConstellationMarkers()
        {
            ConstellationMarkerCount = 0;
            for (int i = 0; i < 3; i++)
                ConstellationMarkers[i] = Vector2.Zero;
        }
    }
}
