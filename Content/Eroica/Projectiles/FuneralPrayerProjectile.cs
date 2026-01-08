using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// Funeral Prayer projectile - large flaming bolt with red/gold flames using 6x6 sprite sheet.
    /// </summary>
    public class FuneralPrayerProjectile : ModProjectile
    {
        private const int FrameCount = 36;
        private const int FramesPerRow = 6;
        private const int FrameRows = 6;
        private const int AnimationSpeed = 2; // Ticks per frame
        
        private int frameCounter = 0;
        private int currentFrame = 0;
        
        // Trail positions for afterimage effect
        private Vector2[] trailPositions = new Vector2[10];
        private float[] trailRotations = new float[10];
        private int trailIndex = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 10;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 48;
            Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.9f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Update trail
            trailPositions[trailIndex] = Projectile.Center;
            trailRotations[trailIndex] = Projectile.rotation;
            trailIndex = (trailIndex + 1) % trailPositions.Length;
            
            // Animate through 6x6 sprite sheet
            frameCounter++;
            if (frameCounter >= AnimationSpeed)
            {
                frameCounter = 0;
                currentFrame = (currentFrame + 1) % FrameCount;
            }
            
            // Rotation follows velocity
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Red/gold lighting
            Lighting.AddLight(Projectile.Center, 1.0f, 0.5f, 0.1f);
            
            // Red flames
            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 1.8f);
                flame.noGravity = true;
                flame.velocity = Projectile.velocity * -0.2f + Main.rand.NextVector2Circular(1f, 1f);
            }
            
            // Gold particles
            if (Main.rand.NextBool(2))
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
                gold.noGravity = true;
                gold.velocity = Projectile.velocity * -0.3f;
            }
            
            // Ember sparks
            if (Main.rand.NextBool(3))
            {
                Dust ember = Dust.NewDustDirect(Projectile.Center, 1, 1,
                    DustID.Torch, 0f, 0f, 100, default, 1.0f);
                ember.noGravity = true;
                ember.velocity = Main.rand.NextVector2Circular(3f, 3f);
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            
            int frameWidth = texture.Width / FramesPerRow;
            int frameHeight = texture.Height / FrameRows;
            
            int frameX = currentFrame % FramesPerRow;
            int frameY = currentFrame / FramesPerRow;
            
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2, frameHeight / 2);
            
            // Draw trail afterimages
            for (int i = 0; i < trailPositions.Length; i++)
            {
                if (trailPositions[i] == Vector2.Zero) continue;
                
                int trailFrameOffset = (trailPositions.Length - i);
                int trailFrame = (currentFrame - trailFrameOffset + FrameCount) % FrameCount;
                int trailFrameX = trailFrame % FramesPerRow;
                int trailFrameY = trailFrame / FramesPerRow;
                Rectangle trailSourceRect = new Rectangle(trailFrameX * frameWidth, trailFrameY * frameHeight, frameWidth, frameHeight);
                
                float trailAlpha = (float)i / trailPositions.Length * 0.4f;
                Color trailColor = Color.OrangeRed * trailAlpha;
                
                Main.EntitySpriteDraw(texture, trailPositions[i] - Main.screenPosition,
                    trailSourceRect, trailColor, trailRotations[i], origin, 0.8f, SpriteEffects.None, 0);
            }
            
            // Draw glow
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = new Vector2(4, 0).RotatedBy(MathHelper.PiOver2 * i);
                Color glowColor = new Color(255, 100, 50) * 0.3f;
                Main.EntitySpriteDraw(texture, Projectile.Center + offset - Main.screenPosition,
                    sourceRect, glowColor, Projectile.rotation, origin, Projectile.scale * 1.1f, SpriteEffects.None, 0);
            }
            
            // Draw main sprite
            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition,
                sourceRect, Color.White, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Large fiery explosion
            for (int i = 0; i < 40; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.RedTorch, 0f, 0f, 100, default, 2.5f);
                flame.noGravity = true;
                flame.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }

            // Gold sparks
            for (int i = 0; i < 25; i++)
            {
                Dust gold = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.GoldFlame, 0f, 0f, 100, default, 2.0f);
                gold.noGravity = true;
                gold.velocity = Main.rand.NextVector2Circular(5f, 5f);
            }
            
            // Embers
            for (int i = 0; i < 15; i++)
            {
                Dust ember = Dust.NewDustDirect(Projectile.Center, 1, 1,
                    DustID.Torch, 0f, 0f, 100, default, 1.5f);
                ember.noGravity = false;
                ember.velocity = Main.rand.NextVector2Circular(4f, 4f) + new Vector2(0, -2f);
            }
        }
    }
}
