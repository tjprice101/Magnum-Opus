using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.LaCampanella.ResonantWeapons.InfernalChimesCalling.Utilities
{
    /// <summary>
    /// Per-player tracking for InfernalChimesCalling summoner weapon.
    /// - Infernal Crescendo timer: every 12s, all bells fire synchronized barrage
    /// - Bell Sacrifice cooldown: 15s respawn after sacrifice
    /// - Sequential attack stagger tracking
    /// </summary>
    public class InfernalChimesCallingPlayer : ModPlayer
    {
        /// <summary>Counts up to CrescendoInterval, then triggers Infernal Crescendo.</summary>
        public int CrescendoTimer;
        public const int CrescendoInterval = 720; // 12 seconds
        public const int CrescendoChargeTime = 120; // 2 seconds charge before firing
        public bool CrescendoCharging;
        public int CrescendoChargeTimer;

        /// <summary>Cooldown remaining until a sacrificed bell respawns.</summary>
        public int SacrificeCooldown;
        public const int SacrificeRespawnTime = 900; // 15 seconds

        /// <summary>Which bell index fires next in the sequential pattern.</summary>
        public int NextBellToFire;
        public int StaggerTimer;
        public const int StaggerDelay = 18; // 0.3 seconds between bells

        /// <summary>Tracks if Harmonic Convergence was achieved (all bells fired within 1s).</summary>
        public int RecentShockwaveCount;
        public int ConvergenceWindow;
        public const int ConvergenceWindowMax = 60; // 1 second window

        public override void PostUpdate()
        {
            if (!Player.active || Player.dead) return;

            // Count active choir bells
            int bellCount = Player.ownedProjectileCounts[ModContent.ProjectileType<Projectiles.CampanellaChoirMinion>()];
            if (bellCount <= 0)
            {
                CrescendoTimer = 0;
                CrescendoCharging = false;
                return;
            }

            // Infernal Crescendo timer
            if (!CrescendoCharging)
            {
                CrescendoTimer++;
                if (CrescendoTimer >= CrescendoInterval)
                {
                    CrescendoCharging = true;
                    CrescendoChargeTimer = CrescendoChargeTime;
                }
            }
            else
            {
                CrescendoChargeTimer--;
                if (CrescendoChargeTimer <= 0)
                {
                    // Crescendo fires! Handled in minion AI.
                    CrescendoCharging = false;
                    CrescendoTimer = 0;
                }
            }

            // Stagger timer for sequential attacks
            if (StaggerTimer > 0)
                StaggerTimer--;

            // Bell Sacrifice respawn
            if (SacrificeCooldown > 0)
                SacrificeCooldown--;

            // Harmonic Convergence window
            if (ConvergenceWindow > 0)
            {
                ConvergenceWindow--;
                if (ConvergenceWindow <= 0)
                    RecentShockwaveCount = 0;
            }
        }

        /// <summary>Register a shockwave fired. Returns true if Harmonic Convergence is achieved.</summary>
        public bool RegisterShockwave(int totalBells)
        {
            RecentShockwaveCount++;
            if (ConvergenceWindow <= 0)
                ConvergenceWindow = ConvergenceWindowMax;

            // Convergence: all bells fired within the window
            if (RecentShockwaveCount >= totalBells && totalBells >= 2)
            {
                RecentShockwaveCount = 0;
                ConvergenceWindow = 0;
                return true; // Harmonic Convergence!
            }
            return false;
        }

        public override void OnRespawn()
        {
            CrescendoTimer = 0;
            CrescendoCharging = false;
            SacrificeCooldown = 0;
            NextBellToFire = 0;
            StaggerTimer = 0;
            RecentShockwaveCount = 0;
            ConvergenceWindow = 0;
        }
    }

    public static class InfernalChimesCallingPlayerExtensions
    {
        public static InfernalChimesCallingPlayer InfernalChimesCalling(this Player player) =>
            player.GetModPlayer<InfernalChimesCallingPlayer>();
    }
}
