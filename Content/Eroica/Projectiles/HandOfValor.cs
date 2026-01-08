using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Hand of Valor projectile thrown by Eroica boss in Phase 2.
    /// Sprite points down by default, orients to direction of travel.
    /// Thrown in sets of 3 in a 120 degree cone.
    /// </summary>
    public class HandOfValor : ModProjectile
    {
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.5f;
            Projectile.scale = 0.3f; // 70% size reduction
        }

        public override void AI()
        {
            // Orient to direction of travel
            // Sprite points UP by default, so we subtract 90 degrees (Pi/2) to make it point in velocity direction
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            
            // Red and gold particle trail
            if (Main.rand.NextBool(3))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            
            // Occasional sparkle
            if (Main.rand.NextBool(6))
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldCoin, 0f, 0f, 0, default, 1f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 0.9f, 0.5f, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            // Red and gold explosion
            for (int i = 0; i < 20; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0.2f }, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Small knockback burst effect
            for (int i = 0; i < 10; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Draw trail
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                
                // Red-gold gradient trail
                Color trailColor = Color.Lerp(new Color(200, 50, 30, 80), new Color(255, 200, 100, 120), progress) * progress;
                float scale = Projectile.scale * (0.6f + progress * 0.4f);
                
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw glow
            Color glowColor = new Color(255, 180, 80, 0) * 0.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(2.5f, 0).RotatedBy(MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
            }
            
            // Draw main projectile
            Color mainColor = new Color(255, 240, 220, 255);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 240, 200, 220);
        }
    }
}
