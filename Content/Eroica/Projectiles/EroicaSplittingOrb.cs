using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Eroica.Projectiles
{
    /// <summary>
    /// RecursiveSplitExplosion projectile - Splits into smaller projectiles on death.
    /// ai[0] = recursionDepth (how many more times it can split)
    /// </summary>
    public class EroicaSplittingOrb : ModProjectile
    {
        // Use placeholder texture - projectile is rendered entirely via particles/PreDraw
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        // Colors
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        
        private int RecursionDepth
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = value;
        }
        
        private float SplitTimer
        {
            get => Projectile.ai[1];
            set => Projectile.ai[1] = value;
        }
        
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds max
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }
        
        public override void AI()
        {
            SplitTimer++;
            
            // Scale based on recursion depth (bigger = more splits remaining)
            float baseScale = 0.4f + RecursionDepth * 0.3f;
            Projectile.scale = baseScale;
            
            // Adjust hitbox to match scale
            int size = (int)(20 * baseScale);
            if (Projectile.width != size)
            {
                Projectile.width = size;
                Projectile.height = size;
            }
            
            // Lighting - brighter for larger orbs
            float lightIntensity = 0.5f + RecursionDepth * 0.2f;
            Lighting.AddLight(Projectile.Center, EroicaGold.ToVector3() * lightIntensity);
            
            // Rotation
            Projectile.rotation += Projectile.velocity.Length() * 0.03f;
            
            // Pulsing glow intensity
            float pulse = (float)Math.Sin(SplitTimer * 0.15f) * 0.2f + 0.8f;
            
            // Trail particles - more intense for larger orbs
            if (SplitTimer % (4 - RecursionDepth) == 0)
            {
                Color trailColor = Color.Lerp(EroicaScarlet, EroicaGold, Main.rand.NextFloat());
                CustomParticles.GenericFlare(Projectile.Center, trailColor * pulse, 0.2f + RecursionDepth * 0.08f, 12);
                
                // Dust trail
                int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, dustType, -Projectile.velocity * 0.15f, 100, default, 1.2f + RecursionDepth * 0.3f);
                dust.noGravity = true;
            }
            
            // Slight homing toward player for larger orbs
            if (RecursionDepth >= 1)
            {
                Player target = Main.player[Player.FindClosest(Projectile.Center, 1, 1)];
                if (target != null && target.active && !target.dead)
                {
                    Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    float homingStrength = 0.01f + RecursionDepth * 0.005f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * Projectile.velocity.Length(), homingStrength);
                }
            }
            
            // Auto-split after delay (not just on death)
            int splitDelay = 45 + (3 - RecursionDepth) * 15; // Larger orbs split faster
            if (SplitTimer >= splitDelay && RecursionDepth > 0)
            {
                SplitIntoChildren();
                Projectile.Kill();
            }
        }
        
        private void SplitIntoChildren()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
                return;
                
            int splitCount = 3 + RecursionDepth; // More children from larger orbs
            float sizeReduction = 0.7f; // Each generation is 70% the size
            
            // Sound - higher pitch for smaller splits
            float pitch = 0.2f + (3 - RecursionDepth) * 0.25f;
            SoundEngine.PlaySound(SoundID.Item14 with { Pitch = pitch, Volume = 0.6f }, Projectile.Center);
            
            // Visual burst
            CustomParticles.GenericFlare(Projectile.Center, Color.White, 0.5f + RecursionDepth * 0.2f, 15);
            CustomParticles.HaloRing(Projectile.Center, EroicaGold, 0.25f + RecursionDepth * 0.1f, 12);
            
            // Spawn children
            for (int i = 0; i < splitCount; i++)
            {
                float angle = MathHelper.TwoPi * i / splitCount + Main.rand.NextFloat(-0.2f, 0.2f);
                float speed = Projectile.velocity.Length() * 1.2f + Main.rand.NextFloat(2f, 4f);
                Vector2 childVel = angle.ToRotationVector2() * speed;
                
                // Children have one less recursion depth
                int childDepth = RecursionDepth - 1;
                
                // Spawn child orb
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    childVel,
                    ModContent.ProjectileType<EroicaSplittingOrb>(),
                    (int)(Projectile.damage * 0.8f), // Reduced damage per generation
                    Projectile.knockBack,
                    Main.myPlayer,
                    ai0: childDepth
                );
                
                // Spawn particle for each child direction
                Color burstColor = i % 2 == 0 ? EroicaGold : EroicaScarlet;
                CustomParticles.GenericFlare(Projectile.Center + childVel.SafeNormalize(Vector2.Zero) * 10f, burstColor, 0.3f, 10);
            }
            
            // Extra particles for visual flair
            ThemedParticles.SakuraPetals(Projectile.Center, 3 + RecursionDepth, 30f);
        }
        
        public override void OnKill(int timeLeft)
        {
            // If killed early (by timeout or other means) and has depth remaining, still split
            if (RecursionDepth > 0 && SplitTimer < 45)
            {
                SplitIntoChildren();
            }
            else
            {
                // Final death burst (no more splits)
                Color burstColor = RecursionDepth == 0 ? EroicaScarlet : EroicaGold;
                CustomParticles.GenericFlare(Projectile.Center, burstColor, 0.4f, 12);
                CustomParticles.HaloRing(Projectile.Center, burstColor * 0.7f, 0.2f, 10);
                
                // Dust burst
                for (int i = 0; i < 6; i++)
                {
                    int dustType = Main.rand.NextBool() ? DustID.GoldFlame : DustID.CrimsonTorch;
                    Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, dustType, 0f, 0f, 100, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity = Main.rand.NextVector2Circular(5f, 5f);
                }
            }
        }
        
        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = (float)Math.Sin(SplitTimer * 0.15f) * 0.15f + 0.85f;
            float baseScale = Projectile.scale * 0.8f;
            
            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(EroicaGold, EroicaScarlet, progress) * (1f - progress) * 0.5f;
                trailColor.A = 0;
                float trailScale = baseScale * (1f - progress * 0.4f);
                spriteBatch.Draw(tex, trailPos, null, trailColor, 0f, origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Outer glow layer
            Color outerGlow = EroicaGold * 0.3f;
            outerGlow.A = 0;
            spriteBatch.Draw(tex, drawPos, null, outerGlow, 0f, origin, baseScale * pulse * 1.6f, SpriteEffects.None, 0f);
            
            // Middle layer
            Color midGlow = Color.Lerp(EroicaGold, EroicaScarlet, 0.3f) * 0.5f;
            midGlow.A = 0;
            spriteBatch.Draw(tex, drawPos, null, midGlow, 0f, origin, baseScale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            // Core
            Color coreGlow = Color.Lerp(EroicaScarlet, Color.White, 0.3f) * 0.8f;
            coreGlow.A = 0;
            spriteBatch.Draw(tex, drawPos, null, coreGlow, 0f, origin, baseScale * pulse * 0.7f, SpriteEffects.None, 0f);
            
            // Hot center
            Color hotCore = Color.White * 0.9f;
            hotCore.A = 0;
            spriteBatch.Draw(tex, drawPos, null, hotCore, 0f, origin, baseScale * pulse * 0.3f, SpriteEffects.None, 0f);
            
            return false;
        }
    }
}
