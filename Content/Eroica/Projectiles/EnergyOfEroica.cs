using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Homing energy projectile spawned by Movement I.
    /// Chases the player dealing significant damage.
    /// </summary>
    public class EnergyOfEroica : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150; // 2.5 seconds - shorter duration
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.5f;
        }

        public override void AI()
        {
            // Pink lighting
            Lighting.AddLight(Projectile.Center, 0.9f, 0.4f, 0.6f);

            // Find target player
            Player target = Main.player[(int)Projectile.ai[0]];
            
            if (target.active && !target.dead)
            {
                // Moderate homing - dodgeable with movement
                Vector2 direction = target.Center - Projectile.Center;
                float distance = direction.Length();
                
                if (distance > 0)
                {
                    direction.Normalize();
                    
                    // Slower speed and gentler turns - requires steady movement to escape
                    float homingSpeed = 10f;
                    float turnSpeed = 0.06f; // Turns more gradually
                    
                    // Slightly faster when close, but still manageable
                    if (distance < 300f)
                    {
                        homingSpeed = 12f;
                        turnSpeed = 0.08f;
                    }
                    
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, direction * homingSpeed, turnSpeed);
                }
            }

            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Particle trail
            if (Main.rand.NextBool(2))
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f;
            }

            // Shimmer effect
            if (Main.rand.NextBool(5))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 0, default, 1.2f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Burst of pink particles on impact/expiration
            for (int i = 0; i < 15; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }

            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item110, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw trail
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, texture.Height * 0.5f);

            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color trailColor = new Color(255, 150, 200, 100) * ((float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length);
                float scale = Projectile.scale * (1f - k * 0.1f);
                Main.EntitySpriteDraw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 150, 200, 150);
        }
    }
}
