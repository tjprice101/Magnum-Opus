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
    /// Icicle Bolt - Main projectile for Frostbite Repeater
    /// Piercing ice bolt that inflicts Hypothermia stacking debuff
    /// </summary>
    public class IcicleBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle4";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        private float iceOrbitAngle = 0f;

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            iceOrbitAngle += 0.1f;

            // === VFX: CRYSTALLINE SHARD ORBIT ===
            // Small ice crystals orbit the bolt
            if (Main.GameUpdateCount % 4 == 0)
            {
                for (int c = 0; c < 3; c++)
                {
                    float crystalAngle = iceOrbitAngle + MathHelper.TwoPi * c / 3f;
                    float crystalRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + c) * 3f;
                    Vector2 crystalPos = Projectile.Center + crystalAngle.ToRotationVector2() * crystalRadius;
                    Color crystalColor = Color.Lerp(IceBlue, CrystalCyan, (float)c / 3f) * 0.55f;
                    CustomParticles.GenericFlare(crystalPos, crystalColor, 0.16f, 8);
                }
            }

            // === VFX: SNOWFLAKE PARTICLE STREAM ===
            // Snowflakes drift off the icicle
            if (Main.rand.NextBool(3))
            {
                Vector2 snowOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 snowVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
                Color snowColor = FrostWhite * 0.5f;
                var snow = new GenericGlowParticle(Projectile.Center + snowOffset, snowVel, snowColor, 0.14f, 30, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            // Ice crystal trail - enhanced
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(7f, 7f);
                Vector2 trailVel = -Projectile.velocity * 0.09f + Main.rand.NextVector2Circular(1.2f, 1.2f);
                Color trailColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.24f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // === VFX: FROST GLINT SPARKLES ===
            // Bright sparkles like ice catching light
            if (Main.rand.NextBool(5))
            {
                Vector2 glintPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                CustomParticles.GenericFlare(glintPos, Color.White, 0.45f, 5);
                CustomParticles.GenericFlare(glintPos, CrystalCyan, 0.32f, 7);
            }

            // Crystal ice melody - VISIBLE (scale 0.75f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, CrystalCyan * 0.7f, 0.75f, 40);
            }
            
            // Prismatic sparkle for ice crystal shimmer
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(6f, 6f),
                    -Projectile.velocity * 0.12f, FrostWhite * 0.65f, 0.27f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.45f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia debuff
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 180);

            // Check for freeze at 5 stacks (handled in buff)
            
            // Impact VFX
            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.45f, 15);

            // ☁EMUSICAL IMPACT - VISIBLE frost pierce chord (scale 0.75f)
            for (int n = 0; n < 5; n++)
            {
                float angle = MathHelper.TwoPi * n / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(target.Center, noteVel, IceBlue * 0.75f, 0.75f, 35);
            }
            
            // Sparkle burst for icy impact
            for (int s = 0; s < 5; s++)
            {
                float sAngle = MathHelper.TwoPi * s / 5f;
                var sparkle = new SparkleParticle(target.Center, sAngle.ToRotationVector2() * 3f, FrostWhite * 0.7f, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            for (int i = 0; i < 5; i++)
            {
                Vector2 sparkVel = Main.rand.NextVector2Circular(4f, 4f);
                Color sparkColor = Color.Lerp(IceBlue, FrostWhite, Main.rand.NextFloat()) * 0.5f;
                var spark = new GenericGlowParticle(target.Center, sparkVel, sparkColor, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(spark);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle4").Value;
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
                Color trailColor = Color.Lerp(CrystalCyan, IceBlue, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.1f + 1f;
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.45f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.8f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, IceBlue, 0.45f, 16);

            // ☁EMUSICAL FINALE - VISIBLE ice bolt final note (scale 0.8f)
            for (int n = 0; n < 6; n++)
            {
                float angle = MathHelper.TwoPi * n / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, CrystalCyan * 0.65f, 0.8f, 38);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.2f, 14, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Blizzard Shard - Homing projectile for Frostbite Repeater's right-click
    /// </summary>
    public class BlizzardShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle14";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);

        private bool hasTarget = false;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 12;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 20;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Homing after initial delay
            if (Projectile.timeLeft < 135)
            {
                float homingRange = 400f;
                float homingStrength = 0.08f;

                NPC target = null;
                float closestDist = homingRange;

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < closestDist)
                        {
                            closestDist = dist;
                            target = npc;
                        }
                    }
                }

                if (target != null)
                {
                    hasTarget = true;
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
                }
            }

            // Blizzard particle trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(2f, 2f);
                Color trailColor = Color.Lerp(IceBlue, hasTarget ? CrystalCyan : DeepBlue, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Snowflake particles
            if (Main.rand.NextBool(4))
            {
                Vector2 snowVel = -Projectile.velocity * 0.2f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                var snow = new GenericGlowParticle(Projectile.Center, snowVel, FrostWhite * 0.4f, 0.15f, 20, true);
                MagnumParticleHandler.SpawnParticle(snow);
            }

            // ☁EMUSICAL NOTATION - Blizzard shard whistle - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.2f, -0.4f));
                Color noteColor = hasTarget ? CrystalCyan * 0.7f : IceBlue * 0.6f;
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.72f, 35);
                
                // Frost sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, FrostWhite * 0.4f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, CrystalCyan.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Apply Hypothermia x2
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(ModContent.BuffType<HypothermiaBuff>(), 300);
            target.AddBuff(BuffID.Frostburn2, 240);

            // Impact VFX
            CustomParticles.GenericFlare(target.Center, FrostWhite, 0.65f, 20);
            // Frost sparkle burst 
            var frostSparkle = new SparkleParticle(target.Center, Vector2.Zero, IceBlue * 0.5f, 0.4f * 0.6f, 16);
            MagnumParticleHandler.SpawnParticle(frostSparkle);

            // ☁EMUSICAL IMPACT - Homing frost resonance
            ThemedParticles.MusicNoteBurst(target.Center, CrystalCyan * 0.8f, 6, 4f);

            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.55f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle14").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float progress = (float)i / Projectile.oldPos.Length;
                float alpha = (1f - progress) * 0.5f;
                float trailScale = 0.4f * (1f - progress * 0.6f);
                Color trailColor = Color.Lerp(CrystalCyan, DeepBlue, progress) * alpha;

                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            // Main glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.12f + 1f;
            spriteBatch.Draw(texture, drawPos, null, DeepBlue * 0.4f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, CrystalCyan * 0.7f, Projectile.rotation, origin, 0.28f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.85f, Projectile.rotation, origin, 0.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 0.6f, 18);
            // Frost sparkle burst 
            var frostSparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.5f, 0.35f * 0.6f, 14);
            MagnumParticleHandler.SpawnParticle(frostSparkle);

            // ☁EMUSICAL FINALE - Blizzard dispersal melody
            ThemedParticles.MusicNoteBurst(Projectile.Center, IceBlue * 0.7f, 7, 4f);
            ThemedParticles.MusicNoteRing(Projectile.Center, CrystalCyan * 0.5f, 45f, 6);

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Hypothermia Debuff - Stacking slow, at 5 stacks freezes enemy
    /// </summary>
    public class HypothermiaBuff : ModBuff
    {
        public override string Texture => "Terraria/Images/Buff_" + BuffID.Frostburn;
        
        public override void SetStaticDefaults()
        {
            Main.debuff[Type] = true;
            Main.pvpBuff[Type] = true;
            Main.buffNoSave[Type] = true;
        }

        public override void Update(NPC npc, ref int buffIndex)
        {
            // Count stacks (multiple instances of this buff)
            int stacks = 0;
            for (int i = 0; i < npc.buffType.Length; i++)
            {
                if (npc.buffType[i] == Type && npc.buffTime[i] > 0)
                    stacks++;
            }

            // Apply increasing slow
            float slowMult = 1f - (stacks * 0.1f); // 10% slow per stack
            slowMult = Math.Max(0.3f, slowMult);

            // At 5 stacks, freeze
            if (stacks >= 5)
            {
                npc.AddBuff(BuffID.Frozen, 90);
                
                // Clear Hypothermia stacks
                for (int i = 0; i < npc.buffType.Length; i++)
                {
                    if (npc.buffType[i] == Type)
                        npc.buffTime[i] = 0;
                }

                // Freeze VFX
                CustomParticles.GenericFlare(npc.Center, new Color(100, 255, 255), 0.8f, 25);
                // Frost sparkle burst 
                var frostSparkle = new SparkleParticle(npc.Center, Vector2.Zero, new Color(150, 220, 255), 0.6f * 0.6f, 20);
                MagnumParticleHandler.SpawnParticle(frostSparkle);
            }

            // Visual frost particles on debuffed enemies
            if (Main.rand.NextBool(12))
            {
                Vector2 frostPos = npc.Center + Main.rand.NextVector2Circular(npc.width / 2f, npc.height / 2f);
                Vector2 frostVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 0.5f));
                var frost = new GenericGlowParticle(frostPos, frostVel, new Color(150, 220, 255) * 0.4f, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(frost);
            }
        }
    }
}
