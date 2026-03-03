using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.OdeToJoy.Weapons.FountainOfJoyousHarmony.Projectiles
{
    public class JoyousFountainMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            if (!player.active || player.dead) { Projectile.Kill(); return; }
            Projectile.timeLeft = 2;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;
    }

    public class FountainWaterBolt : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 60);
            target.AddBuff(BuffID.Wet, 300);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }

    public class PetalSplashProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 30);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}