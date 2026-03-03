using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using System;

namespace MagnumOpus.Content.OdeToJoy.Weapons.TheStandingOvation.Projectiles
{
    public class StandingOvationMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
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

    public class JoyWaveProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.scale = 1f + (float)Math.Sin((120 - Projectile.timeLeft) * 0.1f) * 0.3f;
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}