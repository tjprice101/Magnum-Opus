using System;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Utilities
{
    /// <summary>
    /// Tracks Standing Ovation state per player:
    /// - Ovation Meter (builds on kills, triggers Standing Ovation Event)
    /// - Encore window (re-summon within 5s of event = bonus minion)
    /// - Cross-summon sync with Triumphant Chorus
    /// </summary>
    public class OvationPlayer : ModPlayer
    {
        /// <summary>Ovation meter (0-100). Fills on minion kills.</summary>
        public float OvationMeter;

        /// <summary>Frames remaining for Standing Ovation Event (shockwave + rose rain).</summary>
        public int EventTimer;

        /// <summary>Frames remaining for Encore window (5s after event).</summary>
        public int EncoreTimer;

        /// <summary>Whether player is currently in Encore window.</summary>
        public bool EncoreReady => EncoreTimer > 0;

        /// <summary>Crowd size bonus: +5% per additional minion beyond 1.</summary>
        public int CrowdSize;

        public override void ResetEffects()
        {
            // Count active ovation minions
            int count = 0;
            int minionType = ModContent.ProjectileType<Projectiles.StandingOvationMinion>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == minionType)
                    count++;
            }
            CrowdSize = count;
        }

        public override void PostUpdate()
        {
            if (EventTimer > 0)
                EventTimer--;

            if (EncoreTimer > 0)
                EncoreTimer--;

            // Decay meter slowly when no minions active
            if (CrowdSize <= 0 && OvationMeter > 0)
                OvationMeter = Math.Max(0f, OvationMeter - 0.05f);
        }

        /// <summary>
        /// Registers a kill from ovation minion. Returns true if event triggers.
        /// </summary>
        public bool RegisterKill()
        {
            float gain = 15f + CrowdSize * 3f;
            OvationMeter = Math.Min(100f, OvationMeter + gain);

            if (OvationMeter >= 100f)
            {
                OvationMeter = 0f;
                EventTimer = 180; // 3 seconds
                EncoreTimer = 300; // 5 second window after event ends (set again when event finishes)
                return true;
            }
            return false;
        }

        /// <summary>Crowd damage multiplier: base 1.0 + 0.05 per extra minion.</summary>
        public float GetCrowdMultiplier()
        {
            return 1f + Math.Max(0, CrowdSize - 1) * 0.05f;
        }

        /// <summary>Check if Triumphant Chorus is also active for cross-summon sync (+15%).</summary>
        public bool HasChorusSync()
        {
            int chorusType = ModContent.ProjectileType<TriumphantChorus.Projectiles.TriumphantChorusMinion>();
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile p = Main.projectile[i];
                if (p.active && p.owner == Player.whoAmI && p.type == chorusType)
                    return true;
            }
            return false;
        }
    }
}
