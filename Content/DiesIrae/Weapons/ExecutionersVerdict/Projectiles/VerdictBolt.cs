using MagnumOpus.Common;
using MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Buffs;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    public class VerdictBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.velocity = Projectile.velocity.RotatedBy(0.08f * (Projectile.whoAmI % 2 == 0 ? 1 : -1));
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 180);
            target.AddBuff(ModContent.BuffType<PyreImmolation>(), 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class SpectralVerdictSlash : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 40;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 100;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            Projectile.rotation += 0.3f * Projectile.direction;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ExecutionBrand>(), 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}