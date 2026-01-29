using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Winter.Projectiles
{
    /// <summary>
    /// Permafrost Bolt - Main projectile for Permafrost Codex
    /// Frost magic bolt that applies stacking frostbite
    /// </summary>
    public class PermafrostBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Magic frost trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1.5f, 1.5f);
                Color trailColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Arcane rune sparkles
            if (Main.rand.NextBool(6))
            {
                CustomParticles.GenericFlare(Projectile.Center, GlacialPurple * 0.5f, 0.18f, 10);
            }

            Lighting.AddLight(Projectile.Center, GlacialPurple.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 240);
            target.AddBuff(BuffID.Slow, 180);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, GlacialPurple, 0.45f, 16);

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(GlacialPurple, IceBlue, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.45f;
                float trailScale = 0.35f * (1f - progress * 0.5f);
                Color trailColor = Color.Lerp(IceBlue, GlacialPurple, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, GlacialPurple * 0.5f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.32f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.75f, Projectile.rotation, origin, 0.18f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, GlacialPurple, 0.45f, 16);

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Ice Storm Projectile - Charged attack for Permafrost Codex
    /// Creates a devastating blizzard that damages all enemies in range
    /// </summary>
    public class IceStormProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        private const float StormRadius = 180f;

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 100;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            // Slow movement
            Projectile.velocity *= 0.97f;

            // Growing blizzard
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            float currentRadius = StormRadius * (0.5f + lifeProgress * 0.5f);
            
            Projectile.scale = 1f + lifeProgress * 0.3f;

            // Blizzard particle storm
            int particleCount = 4 + (int)(lifeProgress * 6);
            for (int i = 0; i < particleCount; i++)
            {
                // Swirling snow and ice
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float dist = Main.rand.NextFloat(0.3f, 1f) * currentRadius;
                Vector2 particlePos = Projectile.Center + angle.ToRotationVector2() * dist;
                
                // Spiral velocity
                float spiralAngle = angle + MathHelper.PiOver2;
                Vector2 particleVel = spiralAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 8f) + new Vector2(0, Main.rand.NextFloat(-2f, 0.5f));
                
                Color particleColor = Main.rand.NextBool(3) ? FrostWhite : Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat());
                particleColor *= Main.rand.NextFloat(0.4f, 0.7f);
                
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, Main.rand.NextFloat(0.15f, 0.35f), Main.rand.Next(15, 30), true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // Icicle spawns
            if (Projectile.timeLeft % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 spawnPos = Projectile.Center + angle.ToRotationVector2() * Main.rand.NextFloat(30f, currentRadius * 0.8f);
                    Vector2 icicleVel = new Vector2(Main.rand.NextFloat(-3f, 3f), Main.rand.NextFloat(8f, 14f));
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), spawnPos, icicleVel,
                        ModContent.ProjectileType<StormIcicle>(), Projectile.damage / 4, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }

            // Central vortex flare
            if (Projectile.timeLeft % 8 == 0)
            {
                CustomParticles.GenericFlare(Projectile.Center, GlacialPurple * 0.6f, 0.4f + lifeProgress * 0.3f, 12);
            }

            // Frost sparkles (replacing banned HaloRing)
            if (Projectile.timeLeft % 20 == 0)
            {
                var frostSparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.4f, (currentRadius / 200f) * 0.6f, 25);
                MagnumParticleHandler.SpawnParticle(frostSparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * (0.8f + lifeProgress * 0.4f));
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float lifeProgress = 1f - Projectile.timeLeft / 180f;
            float currentRadius = StormRadius * (0.5f + lifeProgress * 0.5f);
            
            Vector2 targetCenter = targetHitbox.Center.ToVector2();
            return Vector2.Distance(Projectile.Center, targetCenter) < currentRadius;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frozen, 60);
            target.AddBuff(BuffID.Frostburn2, 300);
            target.AddBuff(BuffID.Slow, 240);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.5f, 16);

            for (int i = 0; i < 4; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(5f, 5f);
                var spark = new GenericGlowParticle(target.Center, sparkVel, IceBlue * 0.5f, 0.22f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = 1f - Projectile.timeLeft / 180f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow layers
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 1f;
            float currentScale = Projectile.scale * pulse;

            spriteBatch.Draw(texture, drawPos, null, DeepBlue * 0.25f, 0f, origin, currentScale * 2.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.35f, 0f, origin, currentScale * 1.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, GlacialPurple * 0.45f, 0f, origin, currentScale * 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, CrystalCyan * 0.55f, 0f, origin, currentScale * 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.7f, 0f, origin, currentScale * 0.3f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Final explosion
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 1.0f, 30);
            // Frost sparkle burst (replacing banned HaloRing)
            var frostSparkle1 = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.7f, 0.8f * 0.6f, 25);
            MagnumParticleHandler.SpawnParticle(frostSparkle1);
            var frostSparkle2 = new SparkleParticle(Projectile.Center, Vector2.Zero, CrystalCyan * 0.5f, 0.6f * 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(frostSparkle2);
            var frostSparkle3 = new SparkleParticle(Projectile.Center, Vector2.Zero, GlacialPurple * 0.4f, 0.4f * 0.6f, 18);
            MagnumParticleHandler.SpawnParticle(frostSparkle3);

            for (int i = 0; i < 20; i++)
            {
                float angle = MathHelper.TwoPi * i / 20f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color burstColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Storm Icicle - Secondary projectile spawned by Ice Storm
    /// Falls down and shatters on impact
    /// </summary>
    public class StormIcicle : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.25f; // Gravity

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, IceBlue * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.35f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.35f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, IceBlue * alpha, Projectile.oldRot[i], origin, 0.2f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }

            // Main
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.55f, Projectile.rotation, origin, 0.28f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.75f, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.45f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }
}
