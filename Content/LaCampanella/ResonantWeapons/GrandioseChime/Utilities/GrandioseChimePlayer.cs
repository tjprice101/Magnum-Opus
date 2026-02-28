using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.GrandioseChime.Utilities
{
    /// <summary>
    /// Per-player tracking for GrandioseChime:
    /// - ShotCounter: tracks consecutive shots for barrage (every 3rd) and mines (every 4th)
    /// - KillEchoTracker: enables kill echoes (re-fire burst on kill)
    /// </summary>
    public class GrandioseChimePlayer : ModPlayer
    {
        public int ShotCounter;
        public int KillEchoTimer; // When > 0, dying enemies spawn echo projectiles
        private const int KillEchoDuration = 300; // 5 seconds

        /// <summary>
        /// Call when GrandioseChime fires a shot. Returns:
        /// 0 = normal shot, 1 = bellfire barrage (3rd), 2 = note mines (4th), 3 = both (12th)
        /// </summary>
        public int RegisterShot()
        {
            ShotCounter++;
            KillEchoTimer = KillEchoDuration;

            bool isBarrage = ShotCounter % 3 == 0;
            bool isMines = ShotCounter % 4 == 0;

            if (ShotCounter >= 12) ShotCounter = 0; // LCM reset

            if (isBarrage && isMines) return 3;
            if (isBarrage) return 1;
            if (isMines) return 2;
            return 0;
        }

        public override void PostUpdate()
        {
            if (KillEchoTimer > 0)
                KillEchoTimer--;
        }

        public bool HasKillEcho => KillEchoTimer > 0;
    }
}
