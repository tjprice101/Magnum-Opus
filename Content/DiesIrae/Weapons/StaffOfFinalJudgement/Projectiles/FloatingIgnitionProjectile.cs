using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.StaffOfFinalJudgement.Projectiles
{
    public class FloatingIgnitionProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 240);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}