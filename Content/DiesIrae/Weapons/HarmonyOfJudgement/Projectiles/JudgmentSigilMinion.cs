using MagnumOpus.Common;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.HarmonyOfJudgement.Projectiles
{
    public class JudgmentSigilMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 36;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.netImportant = true;
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

    public class JudgmentRayProjectile : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
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
            target.AddBuff(BuffID.OnFire3, 120);
        }

        public override bool PreDraw(ref Color lightColor) => false;
    }
}