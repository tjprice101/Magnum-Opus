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
    /// Moonlight beam projectile - heavy duty, bounces off surfaces, moves fast.
    /// Dark purple center with light purple gradient and sparkles.
    /// Enhanced with additive glow and fractal sparks on bounce.
    /// </summary>
    public class MoonlightBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3"; // Particle-based rendering

        private int bounceCount = 0;
        private const int MaxBounces = 5;
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
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.extraUpdates = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            bounceCount++;
            
            if (bounceCount >= MaxBounces)
            {
                // === ENHANCED FINALE EXPLOSION WITH MULTI-LAYER BLOOM ===
                // Central flash with proper bloom stacking
                EnhancedParticles.BloomFlare(Projectile.Center, Color.White, 0.9f, 22, 4, 1.2f);
                EnhancedParticles.BloomFlare(Projectile.Center, ThemedParticles.MoonlightLightBlue, 0.7f, 20, 3, 1.0f);
                
                // Enhanced themed explosion with bloom
                EnhancedThemedParticles.MoonlightBloomBurstEnhanced(Projectile.Center, 1.2f);
                
                // Fractal burst
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 flareOffset = angle.ToRotationVector2() * 35f;
                    float progress = (float)i / 8f;
                    Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                    CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.55f, 20);
                }
                
                // Halo cascade
                for (int ring = 0; ring < 5; ring++)
                {
                    float ringProgress = (float)ring / 5f;
                    Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, ringProgress);
                    CustomParticles.HaloRing(Projectile.Center, ringColor, 0.4f + ring * 0.15f, 16 + ring * 5);
                }
                
                // Spark spray
                for (int i = 0; i < 14; i++)
                {
                    float angle = MathHelper.TwoPi * i / 14f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                    float progress = (float)i / 14f;
                    Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                    
                    var spark = new GenericGlowParticle(Projectile.Center, sparkVel, sparkColor, 0.4f, 22, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
                
                // Music notes finale
                ThemedParticles.MoonlightMusicNotes(Projectile.Center, 8, 45f);
                
                // Lightning fractals
                for (int i = 0; i < 4; i++)
                {
                    float lightningAngle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 lightningEnd = Projectile.Center + lightningAngle.ToRotationVector2() * 70f;
                    MagnumVFX.DrawMoonlightLightning(Projectile.Center, lightningEnd, 5, 18f, 2, 0.35f);
                }
                
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10, Projectile.Center);
                return true;
            }

            // Bounce off walls
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            // === BOUNCE IMPACT VFX ===
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.5f }, Projectile.Center);
            
            // Central flash
            CustomParticles.GenericFlare(Projectile.Center, UnifiedVFX.MoonlightSonata.LightBlue * 0.9f, 0.45f, 14);
            
            // Themed impact
            ThemedParticles.MoonlightImpact(Projectile.Center, 0.45f);
            
            // Fractal burst on bounce
            for (int i = 0; i < 4; i++)
            {
                float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 flareOffset = angle.ToRotationVector2() * 20f;
                float progress = (float)i / 4f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(Projectile.Center + flareOffset, fractalColor, 0.35f, 14);
            }
            
            // Halo ring
            CustomParticles.HaloRing(Projectile.Center, UnifiedVFX.MoonlightSonata.MediumPurple, 0.3f, 14);
            
            // Music notes on bounce
            ThemedParticles.MoonlightMusicNotes(Projectile.Center, 3, 22f);
            
            // Mini lightning sparks
            for (int i = 0; i < 3; i++)
            {
                float sparkAngle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.4f, 0.4f);
                Vector2 sparkEnd = Projectile.Center + sparkAngle.ToRotationVector2() * 40f;
                MagnumVFX.DrawMoonlightLightning(Projectile.Center, sparkEnd, 3, 12f, 0, 0f);
            }

            return false;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulseTimer += 0.2f;
            
            float pulse = 1f + (float)System.Math.Sin(pulseTimer) * 0.3f;
            
            // === CALAMITY-INSPIRED TRAIL SYSTEM ===
            // Enhanced trail using new ThemedParticles system
            ThemedParticles.MoonlightTrail(Projectile.Center, Projectile.velocity);
            
            // Musical note trail - occasional floating notes
            ThemedParticles.MoonlightMusicTrail(Projectile.Center, Projectile.velocity);
            
            // === GRADIENT GLOW PARTICLES ===
            if (Projectile.timeLeft % 2 == 0)
            {
                float trailProgress = (float)(180 - Projectile.timeLeft) / 180f;
                Color trailColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, trailProgress);
                
                var glow = new GenericGlowParticle(
                    Projectile.Center + Main.rand.NextVector2Circular(4f, 4f),
                    -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f),
                    trailColor,
                    0.35f * pulse,
                    18,
                    true
                );
                MagnumParticleHandler.SpawnParticle(glow);
            }
            
            // === ORBITING STAR POINTS ===
            if (Projectile.timeLeft % 4 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = pulseTimer * 0.5f + MathHelper.TwoPi * i / 4f;
                    float radius = 8f + (float)System.Math.Sin(pulseTimer + i) * 3f;
                    Vector2 starPos = Projectile.Center + angle.ToRotationVector2() * radius;
                    float progress = (float)i / 4f;
                    Color starColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                    CustomParticles.GenericFlare(starPos, starColor, 0.2f, 10);
                }
            }
            
            // Dark purple core - bigger and pulsing
            Dust core = Dust.NewDustDirect(Projectile.Center - new Vector2(4, 4), 8, 8, DustID.PurpleTorch, 0f, 0f, 50, default, 2.2f * pulse);
            core.noGravity = true;
            core.velocity = Vector2.Zero;
            core.fadeIn = 1.8f;
            
            // Light purple outer glow
            for (int i = 0; i < 2; i++)
            {
                Vector2 offset = Main.rand.NextVector2Circular(10f, 10f);
                Dust glow = Dust.NewDustDirect(Projectile.Center + offset, 1, 1, DustID.PinkTorch, 0f, 0f, 100, default, 1.5f);
                glow.noGravity = true;
                glow.velocity = Projectile.velocity * 0.02f;
            }
            
            // Sparkle effect using ThemedParticles
            if (Main.rand.NextBool(4))
            {
                ThemedParticles.MoonlightSparkles(Projectile.Center, 3, 12f);
            }
            
            // === MUSIC NOTES IN TRAIL ===
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = -Projectile.velocity.SafeNormalize(Vector2.Zero) * 1.5f;
                noteVel = noteVel.RotatedByRandom(0.4f);
                Color noteColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, Main.rand.NextFloat());
                float shimmer = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f * shimmer, 25);
            }
            
            // Lighting with pulsing intensity
            float lightPulse = 0.6f + (float)System.Math.Sin(pulseTimer * 0.5f) * 0.2f;
            Lighting.AddLight(Projectile.Center, 0.6f * lightPulse, 0.25f * lightPulse, 0.9f * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Debuffs.MusicsDissonance>(), 180);
            
            // === PHASE 1: CENTRAL FLASH WITH MULTI-LAYER BLOOM ===
            EnhancedParticles.BloomFlare(target.Center, Color.White, 0.75f, 18, 4, 1.1f);
            EnhancedParticles.BloomFlare(target.Center, ThemedParticles.MoonlightLightBlue, 0.6f, 16, 3, 0.9f);
            
            // === PHASE 2: ENHANCED THEMED IMPACT ===
            UnifiedVFXBloom.MoonlightSonata.ImpactEnhanced(target.Center, 0.85f);
            
            // === PHASE 3: SIGNATURE FRACTAL FLARE BURST ===
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 flareOffset = angle.ToRotationVector2() * 30f;
                float progress = (float)i / 6f;
                Color fractalColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress);
                CustomParticles.GenericFlare(target.Center + flareOffset, fractalColor, 0.45f, 18);
            }
            
            // === PHASE 4: GRADIENT HALO RINGS ===
            for (int ring = 0; ring < 3; ring++)
            {
                float ringProgress = (float)ring / 3f;
                Color ringColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.Silver, ringProgress);
                CustomParticles.HaloRing(target.Center, ringColor, 0.25f + ring * 0.1f, 14 + ring * 4);
            }
            
            // === PHASE 5: SPARK SPRAY ===
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                float progress = (float)i / 8f;
                Color sparkColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.Silver, progress);
                
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.3f, 18, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // === PHASE 6: MUSIC NOTES ===
            ThemedParticles.MoonlightMusicNotes(target.Center, 4, 28f);
            ThemedParticles.MoonlightAccidentals(target.Center, 2, 18f);
        }

        public override void OnKill(int timeLeft)
        {
            // Simplified moonlight beam death - gentle ascending fade
            DynamicParticleEffects.MoonlightDeathMoonbeamFade(Projectile.Center, 1.0f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D pixel = TextureAssets.MagicPixel.Value;
            
            // Switch to additive blending
            MagnumVFX.BeginAdditiveBlend(spriteBatch);
            
            // === CALAMITY-INSPIRED MULTI-LAYER TRAIL ===
            // Layer 1: Outer purple glow trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color outerColor = Color.Lerp(UnifiedVFX.MoonlightSonata.DarkPurple, UnifiedVFX.MoonlightSonata.MediumPurple, progress) * (1f - progress) * 0.3f;
                float outerWidth = MathHelper.Lerp(18f, 4f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), outerColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, outerWidth), SpriteEffects.None, 0f);
            }
            
            // Layer 2: Mid gradient trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color midColor = Color.Lerp(UnifiedVFX.MoonlightSonata.MediumPurple, UnifiedVFX.MoonlightSonata.LightBlue, progress) * (1f - progress) * 0.5f;
                float midWidth = MathHelper.Lerp(12f, 2f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), midColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, midWidth), SpriteEffects.None, 0f);
            }
            
            // Layer 3: Core light blue trail
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color coreColor = Color.Lerp(UnifiedVFX.MoonlightSonata.LightBlue, UnifiedVFX.MoonlightSonata.Silver, progress) * (1f - progress) * 0.7f;
                float coreWidth = MathHelper.Lerp(6f, 1f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), coreColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, coreWidth), SpriteEffects.None, 0f);
            }
            
            // Layer 4: White hot center
            for (int i = 0; i < Projectile.oldPos.Length - 1; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero || Projectile.oldPos[i + 1] == Vector2.Zero) continue;
                
                float progress = (float)i / Projectile.oldPos.Length;
                Color whiteColor = Color.White * (1f - progress) * 0.85f;
                float whiteWidth = MathHelper.Lerp(2f, 0.5f, progress);
                
                Vector2 start = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 end = Projectile.oldPos[i + 1] + Projectile.Size / 2f - Main.screenPosition;
                Vector2 direction = end - start;
                float length = direction.Length();
                float rotation = direction.ToRotation();
                
                spriteBatch.Draw(pixel, start, new Rectangle(0, 0, 1, 1), whiteColor,
                    rotation, new Vector2(0, 0.5f), new Vector2(length, whiteWidth), SpriteEffects.None, 0f);
            }
            
            // === MAIN PROJECTILE GLOW ===
            float pulse = MagnumVFX.GetPulse(0.2f, 0.8f, 1.2f);
            Vector2 mainPos = Projectile.Center - Main.screenPosition;
            
            // Outer dark purple glow
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.DarkPurple * 0.4f,
                0f, new Vector2(0.5f, 0.5f), 30f * pulse, SpriteEffects.None, 0f);
            // Mid purple layer
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.MediumPurple * 0.5f,
                0f, new Vector2(0.5f, 0.5f), 20f * pulse, SpriteEffects.None, 0f);
            // Light blue layer
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), UnifiedVFX.MoonlightSonata.LightBlue * 0.65f,
                0f, new Vector2(0.5f, 0.5f), 12f * pulse, SpriteEffects.None, 0f);
            // White center
            spriteBatch.Draw(pixel, mainPos, new Rectangle(0, 0, 1, 1), Color.White * 0.9f,
                0f, new Vector2(0.5f, 0.5f), 5f * pulse, SpriteEffects.None, 0f);
            
            MagnumVFX.EndAdditiveBlend(spriteBatch);
            
            return false;
        }
    }
}
