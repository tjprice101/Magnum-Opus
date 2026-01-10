using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;

namespace MagnumOpus.Content.Eroica.Pets
{
    /// <summary>
    /// Triumphant Colossus - A pet that WALKS on the ground behind the player.
    /// Uses 6x6 sprite sheet animation (36 frames total).
    /// Sprite faces RIGHT by default - flip when moving left.
    /// </summary>
    public class TriumphantColossusPet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Pets/TriumphantColossus";

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 5; // Ticks per frame
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = TotalFrames;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 60;
            Projectile.aiStyle = -1; // Custom AI
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = true; // Walk on ground
            Projectile.ignoreWater = true;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => false;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Keep the projectile alive while buff is active
            if (!CheckActive(owner))
                return;

            // Apply gravity
            Projectile.velocity.Y += 0.4f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;

            // Calculate target position behind player
            float xOffset = owner.direction == 1 ? -80f : 80f;
            Vector2 targetPos = new Vector2(owner.Center.X + xOffset, Projectile.position.Y);
            
            float distanceX = targetPos.X - Projectile.Center.X;
            float absDistX = Math.Abs(distanceX);
            float distanceToOwner = Vector2.Distance(Projectile.Center, owner.Center);
            
            // Teleport if too far away
            if (distanceToOwner > 600f)
            {
                Projectile.Center = owner.Center + new Vector2(xOffset, -20f);
                Projectile.velocity = Vector2.Zero;
                return;
            }
            
            // Walking movement
            float walkSpeed = 4f;
            float runSpeed = 8f;
            float acceleration = 0.3f;
            
            bool isWalking = false;
            float currentSpeed = absDistX > 200f ? runSpeed : walkSpeed; // Run if far away
            
            if (absDistX > 30f)
            {
                isWalking = true;
                if (distanceX > 0)
                {
                    Projectile.velocity.X = Math.Min(Projectile.velocity.X + acceleration, currentSpeed);
                    Projectile.spriteDirection = 1;
                }
                else
                {
                    Projectile.velocity.X = Math.Max(Projectile.velocity.X - acceleration, -currentSpeed);
                    Projectile.spriteDirection = -1;
                }
            }
            else
            {
                // Slow down when close
                Projectile.velocity.X *= 0.85f;
                Projectile.spriteDirection = owner.direction;
            }
            
            // Check if on ground
            bool onGround = Projectile.velocity.Y == 0f || 
                           Collision.SolidCollision(new Vector2(Projectile.position.X + 4, Projectile.Bottom.Y + 8), Projectile.width - 8, 4);
            
            // Only jump if there's a wall blocking the path AND can't walk over it
            if (onGround && isWalking)
            {
                // Check for wall ahead at mid-height
                Vector2 wallCheckPos = new Vector2(
                    Projectile.Center.X + (Projectile.velocity.X > 0 ? 25 : -25),
                    Projectile.Bottom.Y - 20);
                
                // Check for ground ahead (can we walk there?)
                Vector2 groundCheckPos = new Vector2(
                    Projectile.Center.X + (Projectile.velocity.X > 0 ? 35 : -35),
                    Projectile.Bottom.Y + 8);
                    
                bool wallAhead = Collision.SolidCollision(wallCheckPos, 8, 30);
                bool groundAhead = Collision.SolidCollision(groundCheckPos, 8, 16);
                
                // Only jump if there's a wall we can't walk around AND there's ground to land on
                if (wallAhead && groundAhead)
                {
                    Projectile.velocity.Y = -8f;
                }
            }

            // ALWAYS animate through all 36 frames when walking
            if (isWalking || Math.Abs(Projectile.velocity.X) > 0.5f)
            {
                frameCounter++;
                if (frameCounter >= FrameTime)
                {
                    frameCounter = 0;
                    currentFrame++;
                    if (currentFrame >= TotalFrames)
                        currentFrame = 0;
                }
            }
            else
            {
                // Idle - still cycle but slower
                frameCounter++;
                if (frameCounter >= FrameTime * 2)
                {
                    frameCounter = 0;
                    currentFrame++;
                    if (currentFrame >= TotalFrames)
                        currentFrame = 0;
                }
            }

            // Scarlet red glow effect
            Lighting.AddLight(Projectile.Center, 0.4f, 0.1f, 0.1f);

            // Occasional particles when walking - enhanced with custom particles
            if (Main.rand.NextBool(25) && isWalking)
            {
                Dust glow = Dust.NewDustDirect(Projectile.BottomLeft, Projectile.width, 4, DustID.Torch, 0f, 0f, 100, Color.DarkRed, 0.8f);
                glow.noGravity = true;
                glow.velocity *= 0.3f;
            }
            
            // Themed particle heroic aura
            if (Main.rand.NextBool(15))
            {
                ThemedParticles.EroicaAura(Projectile.Center, 25f);
            }
            
            // Custom particle soft glow trail
            if (Main.rand.NextBool(18) && isWalking)
            {
                CustomParticles.EroicaTrailFlare(Projectile.Center, Projectile.velocity);
            }
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<TriumphantColossusBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<TriumphantColossusBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;

            // Calculate frame from 6x6 grid
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;

            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            // Origin at bottom-center so sprite stands on ground
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight);
            // Draw at bottom of hitbox, lowered by 8 pixels (0.5 blocks)
            Vector2 drawPos = Projectile.Bottom - Main.screenPosition + new Vector2(0, 8f);

            float drawScale = 0.45f;

            // Sprite faces RIGHT - flip when spriteDirection is -1
            SpriteEffects effects = Projectile.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Scarlet red glow effect
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.2f + 0.8f;
            Color glowColor = new Color(180, 40, 40, 0) * 0.4f * pulse;

            for (int i = 0; i < 4; i++)
            {
                Vector2 glowOffset = new Vector2(3f, 0f).RotatedBy(i * MathHelper.PiOver2);
                Main.EntitySpriteDraw(texture, drawPos + glowOffset, sourceRect, glowColor, Projectile.rotation,
                    origin, drawScale, effects, 0);
            }

            // Main sprite
            Main.EntitySpriteDraw(texture, drawPos, sourceRect, lightColor, Projectile.rotation,
                origin, drawScale, effects, 0);

            return false;
        }
    }
}
