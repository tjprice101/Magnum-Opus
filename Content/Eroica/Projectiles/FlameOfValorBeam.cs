using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Beam projectile fired by Flames of Valor.
    /// Red and gold energy beam similar to Heroic Sigil effect.
    /// </summary>
    public class FlameOfValorBeam : ModProjectile
    {
        // Use vanilla texture - solar flare projectile
        public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.SolarWhipSwordExplosion;
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.light = 0.8f;
            Projectile.extraUpdates = 1;
            Projectile.scale = 0.65f; // 35% size reduction
        }

        public override void AI()
        {
            // Face direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Enhanced trail using ThemedParticles
            ThemedParticles.EroicaTrail(Projectile.Center, Projectile.velocity);
            
            // Red and gold particle trail (reduced count)
            if (Main.rand.NextBool(4))
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, 0f, 100, default, 1.8f);
                dust.noGravity = true;
                dust.velocity = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(2f, 2f);
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 1f, 0.6f, 0.2f);
        }

        public override void OnKill(int timeLeft)
        {
            // Enhanced burst with ThemedParticles
            ThemedParticles.EroicaBloomBurst(Projectile.Center, 0.8f);
            ThemedParticles.EroicaSparks(Projectile.Center, Projectile.velocity);
            
            // Burst effect on death (reduced count)
            for (int i = 0; i < 8; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
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
                
                // Gradient from gold to red
                Color trailColor = Color.Lerp(new Color(255, 100, 50, 100), new Color(255, 220, 100, 150), progress) * progress;
                float scale = Projectile.scale * (0.5f + progress * 0.5f);
                
                spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }
            
            // Draw glow
            Color glowColor = new Color(255, 200, 100, 0) * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0).RotatedBy(MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, null, glowColor, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.2f, SpriteEffects.None, 0f);
            }
            
            // Draw main projectile
            Color mainColor = new Color(255, 220, 180, 200);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, mainColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 220, 150, 150);
        }
    }
}
