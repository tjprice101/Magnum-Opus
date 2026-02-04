using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.MoonlightSonata.Projectiles
{
    /// <summary>
    /// Purple energy wave projectile fired by the Eternal Moon sword.
    /// Features Calamity-inspired primitive trail rendering with multi-layer glow.
    /// </summary>
    public class EternalMoonWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc2"; // Particle-based rendering

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 18;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }

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
            Projectile.alpha = 255; // Fully transparent - we use particles for visuals
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // === ENHANCED MULTI-LAYER TRAIL WITH BLOOM ===
            // Enhanced trail using ThemedParticles
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Musical note trail - floating notes shed from the wave
            ThemedParticles.MoonlightMusicTrail(Projectile.Center, Projectile.velocity);
            
            // Core glowing particles with gradient using enhanced system
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(14f, 14f);
                float progress = Main.rand.NextFloat();
                Color trailColor = Color.Lerp(ThemedParticles.MoonlightDarkPurple, ThemedParticles.MoonlightLightBlue, progress);
                
                // Enhanced particle with bloom stacking
                var glow = EnhancedParticlePool.GetParticle()
                    .Setup(CustomParticleSystem.RandomGlow(), Projectile.Center + offset, -Projectile.velocity * 0.15f,
                        trailColor, 0.28f + progress * 0.12f, 16)
                    .WithBloom(3, 0.8f)
                    .WithDrag(0.95f);
                EnhancedParticlePool.SpawnParticle(glow);
            }
            
            // Vivid flare accents
            if (Main.rand.NextBool(3))
            {
                CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.MoonlightSonata.LightPurple, 0.35f, 14);
            }
            
            // Music notes in trail
            if (Main.rand.NextBool(5))
            {
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, Main.rand.NextFloat());
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(Projectile.Center, -Projectile.velocity * 0.1f, noteColor, 0.75f * shimmer, 25);
            }
            
            // Main wave particles
            for (int i = 0; i < 2; i++)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2Circular(16f, 16f);
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, 0f, 0f, 80, default, 2f);
                dust.noGravity = true;
                dust.velocity = Projectile.velocity * 0.08f;
                dust.fadeIn = 1.2f;
            }
            
            // Prismatic accents
            if (Main.rand.NextBool(3))
            {
                CustomParticles.PrismaticSparkle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), 
                    UnifiedVFX.MoonlightSonata.Silver, 0.22f);
            }

            // Wave pulsing effect
            Projectile.scale = 1f + (float)System.Math.Sin(Projectile.timeLeft * 0.3f) * 0.12f;
            
            // Enhanced Lighting
            Lighting.AddLight(Projectile.Center, UnifiedVFX.MoonlightSonata.LightBlue.ToVector3() * 0.7f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Musical Dissonance debuff
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // === ENHANCED IMPACT WITH MULTI-LAYER BLOOM ===
            // Central flash with proper bloom stacking
            EnhancedParticles.BloomFlare(target.Center, Color.White * 0.9f, 0.6f, 18, 4, 1.0f);
            EnhancedParticles.BloomFlare(target.Center, ThemedParticles.MoonlightLightBlue, 0.5f, 16, 3, 0.85f);
            
            // Enhanced themed impact with full bloom
            UnifiedVFXBloom.MoonlightSonata.ImpactEnhanced(target.Center, 0.8f);
        }

        public override void OnKill(int timeLeft)
        {
            // Simplified eternal wave death - soft sparkle effect
            DynamicParticleEffects.MoonlightDeathTwilightSparkle(Projectile.Center, 0.9f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // === PRIMITIVE TRAIL RENDERING ===
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // Draw glowing trail with gradient
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(UnifiedVFX.MoonlightSonata.LightBlue, UnifiedVFX.MoonlightSonata.DarkPurple, progress);
                trailColor *= (1f - progress);
                float width = MathHelper.Lerp(18f, 3f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                if (length < 1f) continue;
                float rotation = direction.ToRotation();
                
                // Outer glow layer
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.35f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 2.5f), SpriteEffects.None, 0f);
                // Middle layer
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor * 0.6f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 1.3f), SpriteEffects.None, 0f);
                // Core layer
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), trailColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width), SpriteEffects.None, 0f);
                // White center
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), Color.White * (1f - progress) * 0.7f,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, width * 0.35f), SpriteEffects.None, 0f);
            }
            
            // Draw main projectile glow
            float pulse = MagnumVFX.GetPulse(0.15f, 0.85f, 1.2f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer glow
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.DarkPurple * 0.4f,
                0f, new Vector2(0.5f, 0.5f), 30f * pulse, SpriteEffects.None, 0f);
            // Middle
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.MediumPurple * 0.6f,
                0f, new Vector2(0.5f, 0.5f), 18f * pulse, SpriteEffects.None, 0f);
            // Core
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.LightBlue * 0.8f,
                0f, new Vector2(0.5f, 0.5f), 10f * pulse, SpriteEffects.None, 0f);
            // White center
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), Color.White * 0.9f,
                0f, new Vector2(0.5f, 0.5f), 5f * pulse, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            return false;
        }
    }
}
