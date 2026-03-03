using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    public class WrathsCleaverSwing : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 120;
            Projectile.height = 120;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.ownerHitCheck = true;
            Projectile.timeLeft = 600;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Projectile.timeLeft = 2;
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            Projectile.rotation += 0.3f * Projectile.direction;
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}