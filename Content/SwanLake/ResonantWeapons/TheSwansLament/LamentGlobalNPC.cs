using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Projectiles;
using MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament.Utilities;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.TheSwansLament
{
    /// <summary>
    /// Tracks Lament-related kills:
    /// - Registers kills for the Lament's Echo fire-rate buff
    /// - Spawns Destruction Halo projectiles on enemy death when killed by Lament weapons
    /// </summary>
    public class LamentGlobalNPC : GlobalNPC
    {
        public override bool InstancePerEntity => true;

        /// <summary>Whether this NPC was most recently hit by a Swan's Lament projectile.</summary>
        public bool HitByLament { get; set; }
        public int LamentOwner { get; set; } = -1;

        public override void OnHitByProjectile(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
        {
            if (projectile.type == ModContent.ProjectileType<LamentBulletProj>() ||
                projectile.type == ModContent.ProjectileType<DestructionHaloProj>())
            {
                HitByLament = true;
                LamentOwner = projectile.owner;
            }
        }

        public override void OnKill(NPC npc)
        {
            if (!HitByLament || LamentOwner < 0 || LamentOwner >= Main.maxPlayers) return;

            Player owner = Main.player[LamentOwner];
            if (!owner.active || owner.dead) return;

            // Register kill for Lament's Echo
            var echoPlayer = owner.GetModPlayer<LamentPlayer>();
            echoPlayer.RegisterKill();

            // Spawn Destruction Halo at the death location
            if (Main.myPlayer == LamentOwner)
            {
                int haloDamage = (int)(owner.GetTotalDamage(DamageClass.Ranged).ApplyTo(90));
                Projectile.NewProjectile(
                    owner.GetSource_FromThis(),
                    npc.Center,
                    Microsoft.Xna.Framework.Vector2.Zero,
                    ModContent.ProjectileType<DestructionHaloProj>(),
                    haloDamage, 4f, LamentOwner);
            }
        }
    }
}
