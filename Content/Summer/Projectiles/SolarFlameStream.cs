using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Summer.Projectiles
{
    /// <summary>
    /// Solar Flame Stream - Main flame projectile from Solar Scorcher
    /// </summary>
    public class SolarFlameStream : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunWhite = new Color(255, 250, 240);
        private static readonly Color SunRed = new Color(255, 100, 50);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 45;
            Projectile.light = 0.4f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 100;
            Projectile.extraUpdates = 1;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/ParticleTrail3";

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            
            // Grow then fade
            float lifeProgress = 1f - (float)Projectile.timeLeft / 45f;
            Projectile.scale = 0.8f + lifeProgress * 0.4f - lifeProgress * lifeProgress * 0.5f;
            Projectile.alpha = (int)(100 + lifeProgress * 155);

            // === VFX VARIATION #10: BLAZING EMBER WAKE ===
            // Fire particles with dynamic ember sparks
            if (Main.rand.NextBool(2))
            {
                Vector2 firePos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Vector2 fireVel = -Projectile.velocity * 0.06f + Main.rand.NextVector2Circular(2.5f, 2.5f);
                fireVel.Y -= Main.rand.NextFloat(0.5f, 1.5f); // Embers rise
                Color fireColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.65f;
                var fire = new GenericGlowParticle(firePos, fireVel, fireColor, 0.32f, 18, true);
                MagnumParticleHandler.SpawnParticle(fire);
            }

            // === VFX VARIATION #11: HEAT SHIMMER DISTORTION ===
            // Subtle wavy particles that simulate heat distortion
            if (Main.rand.NextBool(4))
            {
                float shimmerAngle = Main.GameUpdateCount * 0.3f + Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 shimmerOffset = shimmerAngle.ToRotationVector2() * 6f;
                Vector2 shimmerPos = Projectile.Center + shimmerOffset;
                Vector2 shimmerVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -0.5f));
                Color shimmerColor = SunWhite * 0.35f;
                var shimmer = new GenericGlowParticle(shimmerPos, shimmerVel, shimmerColor, 0.18f, 22, true);
                MagnumParticleHandler.SpawnParticle(shimmer);
            }

            // Fire dust
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.Torch, -Projectile.velocity * 0.12f + new Vector2(0, -1f), 0, SunOrange, 1.3f);
                dust.noGravity = true;
            }

            // === VFX VARIATION #12: FLARE BURST ACCENTS ===
            // Periodic bright flare bursts along the flame path
            if (Main.GameUpdateCount % 8 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, SunGold, 0.4f, 8);
                // Radial mini-flares
                for (int f = 0; f < 3; f++)
                {
                    float flareAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 flarePos = Projectile.Center + flareAngle.ToRotationVector2() * 8f;
                    CustomParticles.GenericFlare(flarePos, SunOrange * 0.7f, 0.22f, 6);
                }
            }

            // Musical fire notes - VISIBLE (scale 0.75f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color noteColor = Color.Lerp(SunOrange, SunGold, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f, 32);
            }
            
            // Sparkle accents - enhanced
            if (Main.rand.NextBool(3))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                var sparkle = new SparkleParticle(sparklePos, Main.rand.NextVector2Circular(1.5f, 1.5f), SunWhite * 0.55f, 0.24f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Slow down slightly
            Projectile.velocity *= 0.97f;

            float alpha = 1f - (float)Projectile.alpha / 255f;
            Lighting.AddLight(Projectile.Center, SunOrange.ToVector3() * 0.55f * alpha);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - VISIBLE fire notes! (scale 0.75f)
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 3f;
                ThemedParticles.MusicNote(target.Center, noteVel, SunOrange, 0.75f, 30);
            }
            
            // Apply burning
            target.AddBuff(BuffID.OnFire3, 120);
            target.AddBuff(BuffID.Daybreak, 60);
        }

        public override void OnKill(int timeLeft)
        {
            // Small fire burst
            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                Color burstColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.22f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float alpha = 1f - (float)Projectile.alpha / 255f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - progress) * alpha * 0.5f;
                Color trailColor = Color.Lerp(SunOrange, SunRed, progress);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor * trailAlpha, Projectile.oldRot[i], origin, Projectile.scale * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }

            // Main flame
            spriteBatch.Draw(texture, drawPos, null, SunOrange * alpha * 0.5f, Projectile.rotation, origin, Projectile.scale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunGold * alpha * 0.6f, Projectile.rotation, origin, Projectile.scale * 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SunWhite * alpha * 0.7f, Projectile.rotation, origin, Projectile.scale * 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Heatwave Pulse - Expanding damage ring from Solar Scorcher
    /// </summary>
    public class HeatwavePulse : ModProjectile
    {
        private static readonly Color SunGold = new Color(255, 215, 0);
        private static readonly Color SunOrange = new Color(255, 140, 0);
        private static readonly Color SunRed = new Color(255, 100, 50);

        private float expandRadius = 20f;

        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.light = 0.6f;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo5";

        public override void AI()
        {
            // Expand outward
            expandRadius += 8f;
            Projectile.width = Projectile.height = (int)(expandRadius * 2);
            Projectile.Center = Main.player[Projectile.owner].Center;

            // Ring particles
            if (Projectile.timeLeft % 2 == 0)
            {
                int particleCount = (int)(expandRadius / 10f);
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + Main.rand.NextFloat(-0.2f, 0.2f);
                    Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * expandRadius;
                    Vector2 particleVel = angle.ToRotationVector2() * 2f;
                    Color particleColor = Color.Lerp(SunOrange, SunRed, Main.rand.NextFloat()) * 0.6f;
                    var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.3f, 18, true);
                    MagnumParticleHandler.SpawnParticle(particle);
                }
                
                // ☁EMUSICAL NOTATION - Notes ride the heatwave! - VISIBLE SCALE 0.72f+
                for (int n = 0; n < 3; n++)
                {
                    float noteAngle = MathHelper.TwoPi * n / 3f + Main.rand.NextFloat(MathHelper.PiOver4);
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * expandRadius;
                    Vector2 noteVel = noteAngle.ToRotationVector2() * 1.5f + new Vector2(0, -0.5f);
                    ThemedParticles.MusicNote(notePos, noteVel, SunGold, 0.72f, 35);
                    
                    // Heat sparkle
                    var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, SunOrange * 0.4f, 0.2f, 16);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }

            // Fading
            float progress = 1f - (float)Projectile.timeLeft / 30f;
            Projectile.alpha = (int)(progress * 200);

            Lighting.AddLight(Projectile.Center, SunOrange.ToVector3() * (1f - progress) * 0.8f);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // Ring collision - only hit enemies at the edge of the expanding ring
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            float distToCenter = Vector2.Distance(Projectile.Center, targetCenter);
            
            float ringThickness = 30f;
            return distToCenter >= expandRadius - ringThickness && distToCenter <= expandRadius + ringThickness;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            
            // Hit VFX
            CustomParticles.GenericFlare(target.Center, SunOrange, 0.5f, 15);
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                var spark = new GenericGlowParticle(target.Center, sparkVel, SunRed * 0.6f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Draw expanding ring
            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float alpha = 1f - (float)Projectile.alpha / 255f;

            // Draw ring as series of points
            int pointCount = (int)(expandRadius / 6f);
            Texture2D glowTex = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = glowTex.Size() / 2f;

            for (int i = 0; i < pointCount; i++)
            {
                float angle = MathHelper.TwoPi * i / pointCount;
                Vector2 pointPos = Projectile.Center + angle.ToRotationVector2() * expandRadius - Main.screenPosition;
                Color pointColor = Color.Lerp(SunGold, SunRed, (float)i / pointCount) * alpha * 0.4f;
                spriteBatch.Draw(glowTex, pointPos, null, pointColor, 0f, origin, 0.25f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
