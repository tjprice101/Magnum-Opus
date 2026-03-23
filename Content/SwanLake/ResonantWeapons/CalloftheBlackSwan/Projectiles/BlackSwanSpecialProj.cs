using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.SwanLake.ResonantWeapons.CalloftheBlackSwan.Projectiles
{
    /// <summary>Call of the Black Swan special: prismatic sparkle buff aura. Placeholder.</summary>
    public class BlackSwanSpecialProj : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/VFX Asset Library/GlowAndBloom/SoftGlow";
        public override void SetDefaults()
        {
            Projectile.width = 30; Projectile.height = 30;
            Projectile.friendly = false; Projectile.penetrate = -1;
            Projectile.tileCollide = false; Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
        }
        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!owner.active || owner.dead) { Projectile.Kill(); return; }
            Projectile.Center = owner.Center;
            // Rainbow sparkle buff: +movement speed for 10s
            owner.moveSpeed += 0.15f;
            Lighting.AddLight(Projectile.Center, 0.4f, 0.4f, 0.5f);
            if (Main.rand.NextBool(3))
            {
                Color c = Main.hslToRgb(Main.rand.NextFloat(), 0.6f, 0.8f);
                Dust d = Dust.NewDustPerfect(owner.Center + Main.rand.NextVector2Circular(30f, 30f),
                    DustID.RainbowTorch, Main.rand.NextVector2Circular(1f, 1f), 0, c, 0.6f);
                d.noGravity = true;
            }
        }
    }
}
