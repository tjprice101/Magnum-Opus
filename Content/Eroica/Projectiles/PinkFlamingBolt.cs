using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Pink flaming bolt shot by Movement III.
    /// Fast moving, straight trajectory with enhanced fractal lightning effects.
    /// </summary>
    public class PinkFlamingBolt : ModProjectile
    {
        public override string Texture => "Terraria/Images/Projectile_0";

        private float pulseTimer = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 15;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.light = 0.6f;
        }

        public override void AI()
        {
            pulseTimer += 0.15f;
            float pulse = 1f + (float)System.Math.Sin(pulseTimer) * 0.2f;
            
            // Pink flame lighting
            Lighting.AddLight(Projectile.Center, 1f * pulse, 0.3f * pulse, 0.5f * pulse);

            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Core pink flame particles with pulsing
            for (int i = 0; i < 3; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 2.2f * pulse);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(0.5f, 0.5f);
            }

            // Pink flame trail particles
            for (int i = 0; i < 2; i++)
            {
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.4f + Main.rand.NextVector2Circular(1f, 1f);
            }

            // Bright sparkle core
            if (Main.rand.NextBool(2))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldFlame, 0f, 0f, 0, default, 1.8f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }

            // Fire accent particles
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.Torch, 0f, 0f, 100, new Color(255, 100, 150), 1.5f);
                fire.noGravity = true;
                fire.velocity = -Projectile.velocity * 0.15f;
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Eroica-themed explosion burst
            MagnumVFX.CreateEroicaBurst(Projectile.Center, 2);
            
            // Sakura-themed fractal spark burst (pink cherry blossom effect)
            MagnumVFX.CreateSakuraSparkBurst(Projectile.Center, 8, 70f);

            // Burst of pink flames on impact
            for (int i = 0; i < 25; i++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.PinkTorch, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Sound effect
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item74, Projectile.position);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Switch to additive blending for glow
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw glowing trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(new Color(255, 150, 200), new Color(200, 50, 100), progress);
                trailColor *= (1f - progress);
                float width = MathHelper.Lerp(10f, 2f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                // Outer glow
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.5f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2f), SpriteEffects.None, 0f);
                // Core
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                // White center
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * (1f - progress) * 0.7f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.3f), SpriteEffects.None, 0f);
            }
            
            // Draw main projectile glow
            float pulse = MagnumVFX.GetPulse(0.15f, 0.9f, 1.1f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), new Color(255, 100, 150) * 0.5f,
                0f, new Vector2(0.5f, 0.5f), 20f * pulse, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), new Color(255, 180, 200) * 0.7f,
                0f, new Vector2(0.5f, 0.5f), 10f * pulse, SpriteEffects.None, 0f);
            // White center
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), Color.White * 0.9f,
                0f, new Vector2(0.5f, 0.5f), 4f * pulse, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            return false;
        }
    }
}
