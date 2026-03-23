using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Nachtmusik.Weapons.MidnightsCrescendo.Projectiles
{
    /// <summary>Midnight's Crescendo special: falling star projectile. Placeholder.</summary>
    public class MidnightsCrescendoSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 20; Projectile.height = 20;
            Projectile.friendly = true; Projectile.penetrate = 1;
            Projectile.tileCollide = true; Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, 0.2f, 0.25f, 0.4f);
            if (Main.rand.NextBool(2))
            {
                Dust d = Dust.NewDustPerfect(Projectile.Center, DustID.SilverCoin, -Projectile.velocity * 0.1f);
                d.noGravity = true; d.scale = 0.6f;
            }
        }
    }
}
