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
    /// Special projectile fired every 10th shot from Piercing Light of the Sakura.
    /// On impact, summons black, gold, and red lightning strikes.
    /// Uses 6x6 sprite sheet animation.
    /// </summary>
    public class PiercingLightOfTheSakuraProjectile : ModProjectile
    {
        // Animation - 6x6 sprite sheet
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;
        private const int FrameTime = 2;
        
        private int frameCounter = 0;
        private int currentFrame = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.6f;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            // Face direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Update animation
            frameCounter++;
            if (frameCounter >= FrameTime)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
            
            // Custom particle trail
            CustomParticles.EroicaTrail(Projectile.Center, Projectile.velocity, 0.35f);
            
            // Trail particles - black, gold, red
            if (Main.rand.NextBool(2))
            {
                int dustType;
                int rand = Main.rand.Next(3);
                if (rand == 0) dustType = DustID.Shadowflame;
                else if (rand == 1) dustType = DustID.GoldFlame;
                else dustType = DustID.CrimsonTorch;
                
                Dust trail = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    dustType, 0f, 0f, 100, default, 1.4f);
                trail.noGravity = true;
                trail.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(1.5f, 1.5f);
            }
            
            // Lighting
            Lighting.AddLight(Projectile.Center, 1f, 0.7f, 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // === SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.Eroica.Scarlet, UnifiedVFX.Eroica.Gold, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // Music notes on hit
            ThemedParticles.EroicaMusicNotes(target.Center, 3, 25f);
            
            SpawnLightning(target.Center);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            SpawnLightning(Projectile.Center);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            // Impact burst
            for (int i = 0; i < 20; i++)
            {
                int dustType;
                int rand = Main.rand.Next(3);
                if (rand == 0) dustType = DustID.Shadowflame;
                else if (rand == 1) dustType = DustID.GoldFlame;
                else dustType = DustID.CrimsonTorch;
                
                Dust burst = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                burst.noGravity = true;
                burst.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item62 with { Volume = 0.6f }, Projectile.Center);
        }

        private void SpawnLightning(Vector2 position)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            // Sakura lightning flash using EnergyFlares[0] (main flash) and EnergyFlares[5] (spark)
            var pinkFlash = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[0], position, Vector2.Zero,
                new Color(255, 180, 200), 1.4f, 25, 0.02f, true, true);
            CustomParticleSystem.SpawnParticle(pinkFlash);
            var whiteFlash = CustomParticleSystem.GetParticle().Setup(CustomParticleSystem.EnergyFlares[5], position, Vector2.Zero,
                new Color(255, 255, 220), 0.9f, 15, 0.015f, true, true);
            CustomParticleSystem.SpawnParticle(whiteFlash);
            CustomParticles.MoonlightHalo(position, 0.8f); // Ethereal silver undertone
            CustomParticles.ExplosionBurst(position, new Color(255, 150, 180), 10, 5f);
            
            // Spawn 3 explosion effects at impact position
            for (int i = 0; i < 3; i++)
            {
                Vector2 offset = new Vector2(Main.rand.NextFloat(-50f, 50f), Main.rand.NextFloat(-30f, 30f));
                Vector2 explosionPos = position + offset;
                
                // Spawn at impact position, not from above
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), explosionPos, Vector2.Zero,
                    ModContent.ProjectileType<SakuraLightning>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            
            // Calculate frame from 6x6 grid
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int column = currentFrame % FrameColumns;
            int row = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 drawOrigin = new Vector2(frameWidth / 2, frameHeight / 2);
            
            // Draw trail
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                float progress = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                
                // Gradient trail colors
                Color trailColor = Color.Lerp(new Color(100, 50, 50, 80), new Color(255, 200, 100, 120), progress) * progress;
                float scale = Projectile.scale * (0.5f + progress * 0.5f);
                
                spriteBatch.Draw(texture, drawPos, sourceRect, trailColor, Projectile.oldRot[k], drawOrigin, scale, SpriteEffects.None, 0f);
            }
            
            // Glow effect
            Color glowColor = new Color(255, 200, 100, 0) * 0.4f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(3f, 0).RotatedBy(MathHelper.PiOver2 * i);
                spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + offset, sourceRect, glowColor, 
                    Projectile.rotation, drawOrigin, Projectile.scale * 1.15f, SpriteEffects.None, 0f);
            }
            
            // Draw main projectile
            Color mainColor = new Color(255, 240, 220, 230);
            spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, sourceRect, mainColor, 
                Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return new Color(255, 240, 200, 200);
        }
    }
}
