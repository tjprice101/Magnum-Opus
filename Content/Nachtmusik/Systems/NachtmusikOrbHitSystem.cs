using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Nachtmusik.Debuffs;

namespace MagnumOpus.Content.Nachtmusik.Systems
{
    /// <summary>
    /// GlobalProjectile that automatically applies CelestialHarmony stacks
    /// when Nachtmusik-themed GenericHomingOrbChild or GenericDamageZone projectiles hit enemies.
    /// </summary>
    public class NachtmusikOrbHitSystem : GlobalProjectile
    {
        public override bool InstancePerEntity => false;

        public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!target.active || target.friendly || target.dontTakeDamage)
                return;

            bool isNachtmusikOrb = projectile.type == ModContent.ProjectileType<GenericHomingOrbChild>()
                && (int)projectile.localAI[0] == GenericHomingOrbChild.THEME_NACHTMUSIK;

            bool isNachtmusikZone = projectile.type == ModContent.ProjectileType<GenericDamageZone>()
                && (int)projectile.localAI[0] == GenericHomingOrbChild.THEME_NACHTMUSIK;

            if (isNachtmusikOrb || isNachtmusikZone)
            {
                target.AddBuff(ModContent.BuffType<CelestialHarmony>(), 600);
                target.GetGlobalNPC<CelestialHarmonyNPC>().AddStack(target, 1);
            }
        }
    }
}
