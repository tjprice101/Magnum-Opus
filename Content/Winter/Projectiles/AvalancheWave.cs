using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Winter.Projectiles
{
    /// <summary>
    /// Avalanche Wave - A cascading wall of ice and frost
    /// Spawned by GlacialExecutioner's Avalanche Strike (every 6th swing)
    /// </summary>
    public class AvalancheWave : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SwordArc3";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 60;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 8;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
        }

        public override void AI()
        {
            // Rotation based on velocity
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Growing effect
            float lifeProgress = 1f - Projectile.timeLeft / 90f;
            Projectile.scale = 1f + lifeProgress * 0.5f;

            // Slow down over time
            Projectile.velocity *= 0.975f;

            // Ice shard trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(30f * Projectile.scale, 30f * Projectile.scale);
                Vector2 trailVel = -Projectile.velocity * 0.2f + Main.rand.NextVector2Circular(3f, 3f);
                Color trailColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.35f * Projectile.scale, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Snowflake particles
            if (Main.rand.NextBool(4))
            {
                Vector2 snowPos = Projectile.Center + Main.rand.NextVector2Circular(25f, 25f);
                Vector2 snowVel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 2f));
                var snow = new GenericGlowParticle(snowPos, snowVel, FrostWhite * 0.5f, 0.2f, 30, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            // ☁EMUSICAL NOTATION - Avalanche rumbling melody - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(4))
            {
                Vector2 notePos = Projectile.Center + Main.rand.NextVector2Circular(20f * Projectile.scale, 20f * Projectile.scale);
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(notePos, noteVel, CrystalCyan * 0.65f, 0.72f * Projectile.scale, 40);
                
                // Frost sparkle accent
                var sparkle = new SparkleParticle(notePos, noteVel * 0.5f, FrostWhite * 0.4f, 0.2f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Spawn secondary ice shards
            if (Projectile.timeLeft % 8 == 0 && Projectile.timeLeft > 20)
            {
                float perpAngle = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
                for (int i = -1; i <= 1; i += 2)
                {
                    Vector2 shardVel = perpAngle.ToRotationVector2() * i * Main.rand.NextFloat(3f, 6f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, shardVel, 
                        ModContent.ProjectileType<IceShardProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.7f * Projectile.scale);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // High freeze chance
            if (Main.rand.NextFloat() < 0.4f)
            {
                target.AddBuff(BuffID.Frozen, 90);
            }
            target.AddBuff(BuffID.Frostburn2, 300);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.65f, 20);

            // ☁EMUSICAL IMPACT - Crushing avalanche chord
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.8f, 7, 4.5f);
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.6f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SwordArc3").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail afterimages
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float trailProgress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = (1f - trailProgress) * 0.5f;
                float trailScale = Projectile.scale * (1f - trailProgress * 0.4f);
                Color trailColor = Color.Lerp(CrystalCyan, DeepBlue, trailProgress) * trailAlpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale * 0.6f, SpriteEffects.None, 0f);
            }

            // Main projectile glow layers
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;

            spriteBatch.Draw(texture, drawPos, null, DeepBlue * 0.35f, Projectile.rotation, origin, Projectile.scale * pulse * 1.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.45f, Projectile.rotation, origin, Projectile.scale * 1.0f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, CrystalCyan * 0.55f, Projectile.rotation, origin, Projectile.scale * 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.7f, Projectile.rotation, origin, Projectile.scale * 0.4f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Death explosion VFX
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 0.9f, 25);

            // ☁EMUSICAL FINALE - Avalanche final crash symphony
            ThemedParticles.MusicNoteBurst(Projectile.Center, CrystalCyan * 0.8f, 10, 5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, IceBlue * 0.7f, 50f * Projectile.scale, 8);

            // Frost sparkle burst 
            var frostSparkle1 = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.6f, 0.6f * Projectile.scale * 0.6f, 20);
            MagnumParticleHandler.SpawnParticle(frostSparkle1);
            var frostSparkle2 = new SparkleParticle(Projectile.Center, Vector2.Zero, CrystalCyan * 0.4f, 0.4f * Projectile.scale * 0.6f, 16);
            MagnumParticleHandler.SpawnParticle(frostSparkle2);

            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = Color.Lerp(IceBlue, FrostWhite, (float)i / 12f) * 0.6f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.35f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Ice Shard - Secondary projectile spawned by Avalanche Wave
    /// </summary>
    public class IceShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle7";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

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
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 45;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Projectile.velocity.Y += 0.12f; // Slight gravity

            // Trail
            if (Main.rand.NextBool(3))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                Color trailColor = IceBlue * 0.4f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, trailColor, 0.18f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Ice shard chime - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(7))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1f, -0.3f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, IceBlue * 0.5f, 0.68f, 28);
                
                // Ice sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.4f, CrystalCyan * 0.45f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Main.rand.NextFloat() < 0.15f)
            {
                target.AddBuff(BuffID.Frozen, 45);
            }
            target.AddBuff(BuffID.Frostburn2, 120);

            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.35f, 12);

            // ☁EMUSICAL IMPACT - Shard pierce note
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.55f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle7").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, IceBlue * alpha, Projectile.oldRot[i], origin, 0.25f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }

            // Main glow
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.8f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Shard shatter note
            ThemedParticles.MusicNoteBurst(Projectile.Center, IceBlue * 0.5f, 3, 2f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }
}
