using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.DiesIrae.Weapons.WrathsCleaver.Projectiles
{
    /// <summary>Wrath's Cleaver special: quick 4 attacks sending projectile barrages. Placeholder.</summary>
    public class WrathsCleaverSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.penetrate = 3;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 120;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Projectile.velocity *= 0.98f;
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.1f);
            if (Main.rand.NextBool(3))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, Main.rand.NextVector2Circular(2f, 2f));
                d.noGravity = true;
            }
        }
    }
}
