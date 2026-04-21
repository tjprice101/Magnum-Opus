using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Buffs;

namespace MagnumOpus.Content.DiesIrae.Systems
{
    /// <summary>
    /// GlobalProjectile that automatically applies Tolled stacks
    /// when Dies Irae-themed GenericHomingOrbChild or GenericDamageZone projectiles hit enemies.
    /// </summary>
    public class DiesIraeOrbHitSystem : GlobalProjectile
    {
        public override bool InstancePerEntity => false;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.active || target.friendly || target.dontTakeDamage)
                return;

            bool isDiesIraeOrb = projectile.type == ModContent.ProjectileType<GenericHomingOrbChild>()
                && (int)projectile.localAI[0] == GenericHomingOrbChild.THEME_DIESIRAE;

            bool isDiesIraeZone = projectile.type == ModContent.ProjectileType<GenericDamageZone>()
                && (int)projectile.localAI[0] == GenericHomingOrbChild.THEME_DIESIRAE;

            if (isDiesIraeOrb || isDiesIraeZone)
            {
                target.GetGlobalNPC<DeathTollingBellGlobalNPC>().IncrementTolledStack(target);
            }
        }
    }
}
