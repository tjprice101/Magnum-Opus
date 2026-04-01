using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace MagnumOpus.Content.ClairDeLune.Accessories
{
    /// <summary>
    /// Global NPC that handles Clair de Lune accessory debuff effects on enemies.
    /// Tracks Bullet Time (Chronodisruptor) and Time Fracture zones (Fractured Hourglass).
    /// </summary>
    public class ClairDeLuneAccessoryGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        // --- Bullet Time (Chronodisruptor of Harmony) ---
        public bool bulletTimeActive;
        public int bulletTimeTimer;

        // --- Time Fracture zone damage bonus (Fractured Hourglass Pendant) ---
        // Tracked via player-side zone positions; NPC checks proximity

        public override void ResetEffects(NPC npc)
        {
            // Bullet Time timer decay
            if (bulletTimeTimer > 0)
            {
                bulletTimeTimer--;
                if (bulletTimeTimer <= 0)
                    bulletTimeActive = false;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
        {
            ApplyTimeFractureBonusIfInZone(npc, ref modifiers);
        }

        public override void ModifyHitByItem(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
        {
            ApplyTimeFractureBonusIfInZone(npc, ref modifiers);
        }

        /// <summary>
        /// Applies Bullet Time damage reduction: enemies deal 15% less damage.
        /// Called via vanilla buff system — we reduce outgoing damage from this NPC.
        /// </summary>
        public override void ModifyHitPlayer(NPC npc, Player target, ref Player.HurtModifiers modifiers)
        {
            if (bulletTimeActive)
                modifiers.FinalDamage *= 0.85f;
        }

        private void ApplyTimeFractureBonusIfInZone(NPC npc, ref NPC.HitModifiers modifiers)
        {
            // Check if any player has active time fracture zones near this NPC
            for (int i = 0; i < Main.maxPlayers; i++)
            {
                Player player = Main.player[i];
                if (!player.active || player.dead) continue;
                var hourglassPlayer = player.GetModPlayer<FracturedHourglassPlayer>();
                if (!hourglassPlayer.hourglassActive) continue;

                for (int z = 0; z < 2; z++)
                {
                    if (hourglassPlayer.zoneTimers[z] > 0 &&
                        Vector2.Distance(npc.Center, hourglassPlayer.zonePositions[z]) < 120f)
                    {
                        modifiers.FinalDamage += 0.10f;
                        return; // Don't stack from multiple zones
                    }
                }
            }
        }

        /// <summary>
        /// Apply Bullet Time: slow + 15% reduced damage output for 2s.
        /// </summary>
        public void ApplyBulletTime(NPC npc)
        {
            bulletTimeActive = true;
            bulletTimeTimer = 120; // 2 seconds
        }
    }
}
