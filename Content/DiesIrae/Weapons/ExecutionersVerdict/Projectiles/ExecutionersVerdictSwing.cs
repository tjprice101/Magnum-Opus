using Microsoft.Xna.Framework;
using System;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.ExecutionersVerdict.Projectiles
{
    public class ExecutionersVerdictSwing : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 160;
            Projectile.height = 160;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 18;
            Projectile.extraUpdates = 0;
            Projectile.timeLeft = 200;
            Projectile.noEnchantmentVisuals = true;
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