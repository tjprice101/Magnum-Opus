using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Fate.ResonantWeapons.ResonanceOfABygoneReality
{
    /// <summary>
    /// Per-player data for Resonance of a Bygone Reality.
    /// Tracks hit counter for spectral blade spawning (every 5th hit).
    /// Also tracks Bygone Resonance (blade+bullet same target in 0.5s)
    /// and Reality Fade invulnerability (every 10th combined hit).
    /// PER PLAYER instance — NOT static!
    /// </summary>
    public class ResonancePlayer : ModPlayer
    {
        /// <summary>
        /// Hit counter for Resonance bullets. Every 5th hit spawns a spectral blade.
        /// Per-player, per-instance — never static.
        /// </summary>
        public int HitCounter;

        // === REALITY FADE (Doc mechanic: 10th combined hit = brief invuln) ===
        /// <summary>Combined hit counter (bullets + blades). Every 10th triggers Reality Fade.</summary>
        public int CombinedHitCounter;

        /// <summary>Reality Fade invulnerability timer (18 ticks = 0.3s).</summary>
        public int RealityFadeTimer;

        /// <summary>Whether Reality Fade is currently active.</summary>
        public bool IsRealityFading => RealityFadeTimer > 0;

        // === BYGONE RESONANCE TRACKING ===
        /// <summary>Last NPC index hit by a bullet (for Bygone Resonance trigger).</summary>
        public int LastBulletHitNPC = -1;

        /// <summary>Ticks since last bullet hit (resonance window is 30 ticks = 0.5s).</summary>
        public int BulletHitTimer;

        /// <summary>Last NPC index hit by a spectral blade.</summary>
        public int LastBladeHitNPC = -1;

        /// <summary>Ticks since last blade hit.</summary>
        public int BladeHitTimer;

        public override void ResetEffects()
        {
            // HitCounter persists across frames; only reset on death.
        }

        public override void PostUpdate()
        {
            // Reality Fade timer
            if (RealityFadeTimer > 0)
            {
                RealityFadeTimer--;
                // Brief invulnerability during Reality Fade
                Player.immune = true;
                Player.immuneTime = 2;
            }

            // Bullet hit timer for Bygone Resonance
            if (BulletHitTimer > 0)
            {
                BulletHitTimer--;
                if (BulletHitTimer <= 0)
                    LastBulletHitNPC = -1;
            }

            // Blade hit timer for Bygone Resonance
            if (BladeHitTimer > 0)
            {
                BladeHitTimer--;
                if (BladeHitTimer <= 0)
                    LastBladeHitNPC = -1;
            }
        }

        /// <summary>
        /// Called when a bullet hits an NPC. Tracks for Bygone Resonance and Reality Fade.
        /// Returns true if Bygone Resonance was triggered.
        /// </summary>
        public bool OnBulletHit(int npcIndex)
        {
            CombinedHitCounter++;
            LastBulletHitNPC = npcIndex;
            BulletHitTimer = 30; // 0.5s window

            // Check Reality Fade
            if (CombinedHitCounter >= 10)
            {
                CombinedHitCounter = 0;
                RealityFadeTimer = 18; // 0.3s invulnerability
            }

            // Check Bygone Resonance (blade hit same target within window)
            if (LastBladeHitNPC == npcIndex && BladeHitTimer > 0)
            {
                LastBladeHitNPC = -1;
                LastBulletHitNPC = -1;
                return true; // Bygone Resonance triggered!
            }

            return false;
        }

        /// <summary>
        /// Called when a spectral blade hits an NPC. Tracks for Bygone Resonance.
        /// Returns true if Bygone Resonance was triggered.
        /// </summary>
        public bool OnBladeHit(int npcIndex)
        {
            CombinedHitCounter++;
            LastBladeHitNPC = npcIndex;
            BladeHitTimer = 30;

            // Check Reality Fade
            if (CombinedHitCounter >= 10)
            {
                CombinedHitCounter = 0;
                RealityFadeTimer = 18;
            }

            // Check Bygone Resonance
            if (LastBulletHitNPC == npcIndex && BulletHitTimer > 0)
            {
                LastBulletHitNPC = -1;
                LastBladeHitNPC = -1;
                return true;
            }

            return false;
        }

        public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource)
        {
            HitCounter = 0;
            CombinedHitCounter = 0;
            RealityFadeTimer = 0;
        }

        public override void OnRespawn()
        {
            HitCounter = 0;
            CombinedHitCounter = 0;
        }
    }

    /// <summary>
    /// Extension method for convenient access to ResonancePlayer.
    /// Usage: player.Resonance().HitCounter
    /// </summary>
    public static class ResonancePlayerExtensions
    {
        public static ResonancePlayer Resonance(this Player player)
        {
            return player.GetModPlayer<ResonancePlayer>();
        }
    }
}
