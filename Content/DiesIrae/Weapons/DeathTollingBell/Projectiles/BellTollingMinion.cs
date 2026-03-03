using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;

namespace MagnumOpus.Content.DiesIrae.Weapons.DeathTollingBell.Projectiles
{
    public class BellTollingMinion : ModProjectile
    {
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 1f;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 18000;
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
}