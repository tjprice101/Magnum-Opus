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
    /// Fist of Eroica - Spawns at screen edges, pauses briefly, then blasts across to the other side.
    /// Part of Eroica boss Phase 2 attack pattern.
    /// Uses HandOfValor.png texture.
    /// ai[0] = timer, ai[1] = charge direction encoded (0=down, 1=up, 2=right, 3=left)
    /// </summary>
    public class FistOfEroica : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Projectiles/HandOfValor";
        
        private const int WindupTime = 30; // 0.5 seconds before charging
        private bool hasCharged = false;
        private Vector2 chargeDirection = Vector2.Zero;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1; // Goes through everything
            Projectile.timeLeft = 180; // Long enough for windup + crossing screen
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
            Projectile.light = 0.8f;
            Projectile.scale = 0.272f; // 15% smaller than 0.32
        }

        public override void AI()
        {
            Projectile.ai[0]++;
            
            // Decode charge direction from ai[1]
            if (chargeDirection == Vector2.Zero)
            {
                switch ((int)Projectile.ai[1])
                {
                    case 0: chargeDirection = new Vector2(0, 1); break;   // Down
                    case 1: chargeDirection = new Vector2(0, -1); break;  // Up
                    case 2: chargeDirection = new Vector2(1, 0); break;   // Right
                    case 3: chargeDirection = new Vector2(-1, 0); break;  // Left
                    default: chargeDirection = new Vector2(0, 1); break;
                }
            }
            
            // Windup phase - hover in place with warning effects
            if (Projectile.ai[0] < WindupTime)
            {
                Projectile.velocity = Vector2.Zero;
                
                // Orient toward charge direction
                Projectile.rotation = chargeDirection.ToRotation() - MathHelper.PiOver2;
                
                // Pulsing warning glow effect
                float pulse = (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.5f + 0.5f;
                
                // Gold and red sparkle warning
                if (Main.rand.NextBool(2))
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldCoin : DustID.CrimsonTorch;
                    Vector2 offset = Main.rand.NextVector2Circular(20f, 20f);
                    Dust warning = Dust.NewDustPerfect(Projectile.Center + offset, dustType, -offset * 0.1f, 100, default, 1.5f + pulse);
                    warning.noGravity = true;
                }
                
                // Gathering particles
                if (Projectile.ai[0] % 3 == 0)
                {
                    Vector2 gatherOffset = Main.rand.NextVector2Circular(60f, 60f);
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust gather = Dust.NewDustPerfect(Projectile.Center + gatherOffset, dustType, -gatherOffset * 0.08f, 100, default, 1.8f);
                    gather.noGravity = true;
                }
                
                return;
            }
            
            // Start charging!
            if (!hasCharged)
            {
                hasCharged = true;
                Projectile.velocity = chargeDirection * 60f; // Very fast charge!
                SoundEngine.PlaySound(SoundID.Item117 with { Pitch = 0.3f, Volume = 0.7f }, Projectile.Center);
                
                // Burst effect on charge start
                for (int i = 0; i < 15; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust burst = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                    burst.noGravity = true;
                    burst.velocity = Main.rand.NextVector2Circular(8f, 8f);
                }
            }
            
            // Orient to direction of travel
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            
            // Gold and red sparkle trail while moving
            for (int i = 0; i < 3; i++)
            {
                // Gold sparkles
                if (Main.rand.NextBool(2))
                {
                    Dust goldSparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldCoin, 0f, 0f, 0, default, 1.2f);
                    goldSparkle.noGravity = true;
                    goldSparkle.velocity = -Projectile.velocity * 0.15f + Main.rand.NextVector2Circular(3f, 3f);
                    goldSparkle.fadeIn = 1.1f;
                }
                
                // Red sparkles
                if (Main.rand.NextBool(2))
                {
                    Dust redSparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.CrimsonTorch, 0f, 0f, 100, default, 1.4f);
                    redSparkle.noGravity = true;
                    redSparkle.velocity = -Projectile.velocity * 0.12f + Main.rand.NextVector2Circular(2f, 2f);
                }
            }
            
            // Additional flame trail
            if (Main.rand.NextBool(2))
            {
                Dust fire = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.GoldFlame, 0f, 0f, 100, default, 1.5f);
                fire.noGravity = true;
                fire.velocity = -Projectile.velocity * 0.08f;
            }
            
            // Strong lighting
            Lighting.AddLight(Projectile.Center, 1.2f, 0.6f, 0.3f);
            
            // Screen shake while near player
            Player nearestPlayer = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
            float distToPlayer = Vector2.Distance(Projectile.Center, nearestPlayer.Center);
            if (distToPlayer < 300f)
            {
                EroicaScreenShake.SmallShake(Projectile.Center);
            }
        }

        public override void OnKill(int timeLeft)
        {
            // Gold and red explosion
            for (int i = 0; i < 25; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 2f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(10f, 10f);
            }
            
            // Gold sparkle burst
            for (int i = 0; i < 12; i++)
            {
                Dust sparkle = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.GoldCoin, 0f, 0f, 0, default, 1.5f);
                sparkle.noGravity = true;
                sparkle.velocity = Main.rand.NextVector2Circular(8f, 8f);
            }
            
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f, Pitch = 0f }, Projectile.Center);
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // Impact burst
            for (int i = 0; i < 15; i++)
            {
                int dustType = Main.rand.NextBool() ? DustID.GoldCoin : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustDirect(target.Center, 1, 1, dustType, 0f, 0f, 100, default, 1.5f);
                dust.noGravity = true;
                dust.velocity = Main.rand.NextVector2Circular(6f, 6f);
            }
            
            EroicaScreenShake.SmallShake(target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawOrigin = new Vector2(texture.Width / 2, texture.Height / 2);
            
            // Only draw trail if charging
            if (hasCharged)
            {
                for (int k = 0; k < Projectile.oldPos.Length; k++)
                {
                    Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + new Vector2(Projectile.width / 2, Projectile.height / 2);
                    float trailAlpha = (float)(Projectile.oldPos.Length - k) / Projectile.oldPos.Length;
                    Color trailColor = new Color(255, 180, 50) * trailAlpha * 0.5f;
                    float trailScale = Projectile.scale * (1f - k * 0.05f);
                    
                    spriteBatch.Draw(texture, drawPos, null, trailColor, Projectile.oldRot[k], drawOrigin, trailScale, SpriteEffects.None, 0f);
                }
            }
            
            // Draw main projectile with glow
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Pulsing glow during windup
            float glowScale = hasCharged ? 1.2f : 1f + (float)Math.Sin(Projectile.ai[0] * 0.3f) * 0.3f;
            
            // Outer glow
            spriteBatch.Draw(texture, mainPos, null, new Color(255, 200, 100) * 0.4f, Projectile.rotation, drawOrigin, Projectile.scale * glowScale, SpriteEffects.None, 0f);
            
            // Inner bright core
            spriteBatch.Draw(texture, mainPos, null, new Color(255, 220, 180), Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
