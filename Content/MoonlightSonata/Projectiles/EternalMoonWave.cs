using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Purple energy wave projectile fired by the Eternal Moon sword.
    /// Uses particles and visual effects instead of a texture.
    /// </summary>
    public class EternalMoonWave : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0"; // Invisible base texture

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255; // Fully transparent - we use dust for visuals
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Create purple energy wave effect with particles
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Main wave particles
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-15, 15), Main.rand.NextFloat(-15, 15));
                Vector2 dustPos = Projectile.Center + offset;
                
                // Dark purple core
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.1f;
                dust.fadeIn = 1.2f;
                
                // Light purple outer glow
                if (Main.rand.NextBool(2))
                {
                    Dust glow = Dust.NewDustDirect(dustPos + offset * 0.5f, 1, 1, DustID.PinkTorch, 0f, 0f, 150, default, 1.5f);
                    glow.noGravity = true;
                    glow.velocity = Projectile.velocity * 0.05f;
                }
            }
            
            // Trailing particles
            for (int i = 0; i < 2; i++)
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * Main.rand.NextFloat(0.5f, 1f);
                Dust trail = Dust.NewDustDirect(trailPos, 1, 1, DustID.PurpleCrystalShard, 0f, 0f, 100, default, 1.2f);
                trail.noGravity = true;
                trail.velocity *= 0.3f;
            }

            // Wave pulsing effect
            Projectile.scale = 1f + (float)System.Math.Sin(Projectile.timeLeft * 0.3f) * 0.1f;
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.5f, 0.2f, 0.8f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // Hit particles
            for (int i = 0; i < 10; i++)
            {
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, DustID.PurpleTorch, 
                    Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(-3f, 3f), 100, default, 1.5f);
                dust.noGravity = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Dissipation effect
            for (int i = 0; i < 15; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Circular(4f, 4f);
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.PurpleTorch, velocity.X, velocity.Y, 100, default, 1.5f);
                dust.noGravity = true;
                dust.fadeIn = 0.8f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Don't draw the projectile sprite, only use particles
            return false;
        }
    }
}
