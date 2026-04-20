using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.ConstellationPiercer.Projectiles
{
    public class ConstellationBoltProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Weapons/ConstellationPiercer/ConstellationPiercer";

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    -Projectile.velocity * 0.1f, 0, default, 0.7f);
                d.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, 0.1f, 0.12f, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.BlueTorch,
                    Main.rand.NextVector2CircularEdge(3f, 3f), 0, default, 0.5f);
                d.noGravity = true;
            }
        }
    }
}
