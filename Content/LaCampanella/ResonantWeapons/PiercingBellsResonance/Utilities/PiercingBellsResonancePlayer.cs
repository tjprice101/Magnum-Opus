using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.PiercingBellsResonance.Utilities
{
    /// <summary>
    /// Per-player tracking for PiercingBellsResonance:
    /// - ShotCounter: every 4th shot fires a Seeking Crystal alongside the staccato bullet
    /// - Resonant Detonation via alt-fire (detonates all markers on enemies with 3+)
    /// </summary>
    public class PiercingBellsResonancePlayer : ModPlayer
    {
        public int ShotCounter;
        public const int SeekingCrystalInterval = 4;

        /// <summary>
        /// Called when PiercingBellsResonance fires a shot.
        /// Returns true if this is the 4th shot (Seeking Crystal fires too).
        /// </summary>
        public bool RegisterShot()
        {
            ShotCounter++;
            if (ShotCounter >= SeekingCrystalInterval)
            {
                ShotCounter = 0;
                return true;
            }
            return false;
        }

        public override void OnRespawn()
        {
            ShotCounter = 0;
        }
    }
}
