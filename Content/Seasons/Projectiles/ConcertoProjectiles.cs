using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Seasons.Projectiles
{
    /// <summary>
    /// Spring Verse - UNIQUE CHERRY BLOSSOM STORM AESTHETIC
    /// A swirling vortex of sakura petals with orbiting flower sprites
    /// and a gentle floral trail that leaves the fragrance of spring.
    /// </summary>
    public class SpringVerseProjectile : ModProjectile
    {
        // Use MagicSparklField for unique floral core
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField3";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringPetalCore = new Color(255, 220, 230);
        private static readonly Color SpringLeaf = new Color(120, 200, 100);
        
        private float blossomRotation = 0f;
        private float[] petalAngles = new float[6];
        private float[] petalDistances = new float[6];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            
            // Initialize orbiting petal elements
            for (int i = 0; i < petalAngles.Length; i++)
            {
                petalAngles[i] = MathHelper.TwoPi * i / petalAngles.Length;
                petalDistances[i] = 10f + Main.rand.NextFloat(5f);
            }
        }

        public override void AI()
        {
            blossomRotation += 0.12f;
            
            // Update orbiting petals - gentle spiral motion
            for (int i = 0; i < petalAngles.Length; i++)
            {
                petalAngles[i] += 0.08f + i * 0.01f;
                petalDistances[i] = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 0.8f) * 4f;
            }

            // Gentle homing - spring breeze guiding
            float homingRange = 300f;
            float homingStrength = 0.025f;

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
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * Projectile.velocity.Length(), homingStrength);
            }

            // SAKURA PETAL DRIFT - Floating petals behind
            if (Main.rand.NextBool(2))
            {
                Vector2 petalOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 petalVel = -Projectile.velocity * 0.08f + new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(0.3f, 1.2f));
                Color petalColor = Color.Lerp(SpringPink, SpringPetalCore, Main.rand.NextFloat());
                var petal = new GenericGlowParticle(Projectile.Center + petalOffset, petalVel, petalColor * 0.55f, 0.22f, 28, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // GREEN LEAF ACCENTS - Occasional leaves mixed in
            if (Main.rand.NextBool(8))
            {
                Vector2 leafVel = -Projectile.velocity * 0.06f + new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(0.5f, 1.5f));
                var leaf = new GenericGlowParticle(Projectile.Center, leafVel, SpringLeaf * 0.45f, 0.18f, 30, true);
                MagnumParticleHandler.SpawnParticle(leaf);
            }
            
            // ORBITING PETAL SPARKLE TRAIL
            if (Projectile.timeLeft % 3 == 0)
            {
                int petalIndex = Projectile.timeLeft % petalAngles.Length;
                Vector2 petalPos = Projectile.Center + petalAngles[petalIndex].ToRotationVector2() * petalDistances[petalIndex];
                var sparkle = new SparkleParticle(petalPos, -Projectile.velocity * 0.04f, SpringPetalCore * 0.6f, 0.15f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // MUSIC NOTES - Spring's gentle melody
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat(0.4f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor * 0.85f, 0.75f, 35);
            }
            
            // SPARKLE SHIMMER - Dewdrops on petals
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Main.rand.NextVector2Circular(0.8f, 0.8f), SpringWhite * 0.7f, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Spawn secondary petal projectiles periodically
            if (Projectile.timeLeft % 20 == 0 && Main.myPlayer == Projectile.owner)
            {
                float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                Vector2 petalVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 5f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, petalVel,
                    ModContent.ProjectileType<VersePetalProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
            }

            // Warm spring light
            float lightPulse = 0.4f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 240);

            // BLOSSOM IMPACT - Petal explosion
            CustomParticles.GenericFlare(target.Center, SpringWhite, 0.8f, 20);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.65f, 18);
            
            // Cascading petal halos
            for (int i = 0; i < 4; i++)
            {
                Color haloColor = Color.Lerp(SpringPink, SpringGreen, i / 4f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.5f - i * 0.1f), 0.3f + i * 0.1f, 14 + i * 2);
            }

            // Massive petal burst
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = i % 2 == 0 ? SpringPink : SpringPetalCore;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor * 0.5f, 0.28f, 24, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // Music note chord
            ThemedParticles.MusicNoteBurst(target.Center, SpringPink * 0.75f, 5, 3.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField3").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle2").Value;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;

            // BLOSSOM TRAIL - Petals drifting behind
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Pink petal glow
                Color trailColor = Color.Lerp(SpringPink, SpringPetalCore, progress) * (1f - progress) * 0.45f;
                float trailScale = 0.35f * (1f - progress * 0.6f);
                sb.Draw(glowTex, trailPos, null, trailColor with { A = 0 }, blossomRotation + i * 0.25f, glowOrigin, trailScale, SpriteEffects.None, 0f);
                
                // Occasional leaf in trail
                if (i % 4 == 0)
                {
                    Color leafColor = SpringLeaf * (1f - progress) * 0.3f;
                    sb.Draw(sparkleTex, trailPos + new Vector2(3f, 0f), null, leafColor with { A = 0 }, i * 0.5f, sparkleOrigin, 0.1f * (1f - progress), SpriteEffects.None, 0f);
                }
            }
            
            // OUTER BLOSSOM AURA - Soft pink halo
            sb.Draw(glowTex, drawPos, null, (SpringPink * 0.3f) with { A = 0 }, blossomRotation, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (SpringGreen * 0.25f) with { A = 0 }, -blossomRotation * 0.7f, glowOrigin, 0.45f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING PETALS - Dancing sakura
            for (int i = 0; i < petalAngles.Length; i++)
            {
                Vector2 petalOffset = petalAngles[i].ToRotationVector2() * petalDistances[i];
                Vector2 petalPos = drawPos + petalOffset;
                float petalScale = 0.12f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.6f) * 0.03f;
                Color petalColor = (i % 2 == 0 ? SpringPink : SpringPetalCore) * 0.7f;
                sb.Draw(sparkleTex, petalPos, null, petalColor with { A = 0 }, petalAngles[i], sparkleOrigin, petalScale, SpriteEffects.None, 0f);
            }
            
            // BLOSSOM CORE - Flower center
            sb.Draw(coreTex, drawPos, null, (SpringGreen * 0.4f) with { A = 0 }, blossomRotation, coreOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (SpringPink * 0.6f) with { A = 0 }, -blossomRotation * 1.3f, coreOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (SpringWhite * 0.85f) with { A = 0 }, blossomRotation * 0.5f, coreOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // BLOSSOM SCATTER - Petals fly everywhere
            CustomParticles.GenericFlare(Projectile.Center, SpringWhite, 0.7f, 18);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                // Alternate pink petals and green leaves
                Color burstColor = i % 3 == 0 ? SpringLeaf : (i % 2 == 0 ? SpringPink : SpringPetalCore);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.45f, 0.22f, 26, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Star sparkle accents
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), Main.rand.NextVector2Circular(2f, 2f), SpringWhite * 0.6f, 0.25f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            ThemedParticles.MusicNoteBurst(Projectile.Center, SpringPink * 0.7f, 6, 3.5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, SpringGreen * 0.6f, 45f, 6);
        }
    }

    /// <summary>
    /// Verse Petal - UNIQUE FALLING CHERRY BLOSSOM
    /// Individual sakura petal that flutters down gently
    /// </summary>
    public class VersePetalProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle1";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringPetalCore = new Color(255, 220, 230);
        
        private float flutterPhase = 0f;

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;
            Projectile.velocity *= 0.97f;

            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, Vector2.Zero, SpringPink * 0.35f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Petal drift melody (VISIBLE SCALE 0.7f+)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.8f, 0.7f, 30);
            }
            
            // ☁ESPARKLE ACCENT - Petal twinkle
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, SpringPink, 0.2f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.35f, 12);

            // ☁EMUSICAL IMPACT - Petal whisper
            ThemedParticles.MusicNoteBurst(target.Center, SpringPink * 0.65f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle1").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.6f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Summer Movement - UNIQUE BLAZING SUN ORB AESTHETIC
    /// A fierce solar flare with orbiting solar prominences 
    /// that leaves scorching heat waves and fire trails.
    /// </summary>
    public class SummerMovementProjectile : ModProjectile
    {
        // Use StarBurst for unique solar flare look
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst1";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerWhite = new Color(255, 255, 240);
        private static readonly Color SummerRed = new Color(255, 80, 30);
        private static readonly Color SummerYellow = new Color(255, 255, 100);
        
        private float solarRotation = 0f;
        private float[] prominenceAngles = new float[4];
        private float[] prominenceExtensions = new float[4];

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;
            
            // Initialize orbiting solar prominences
            for (int i = 0; i < prominenceAngles.Length; i++)
            {
                prominenceAngles[i] = MathHelper.TwoPi * i / prominenceAngles.Length;
                prominenceExtensions[i] = 8f + Main.rand.NextFloat(4f);
            }
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            solarRotation += 0.14f;
            
            // Update orbiting prominences - dynamic extension
            for (int i = 0; i < prominenceAngles.Length; i++)
            {
                prominenceAngles[i] += 0.1f + i * 0.02f;
                prominenceExtensions[i] = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i * 0.9f) * 5f;
            }

            // SOLAR FLARE TRAIL - Intense heat waves
            if (Main.rand.NextBool(2))
            {
                for (int layer = 0; layer < 2; layer++)
                {
                    Vector2 flareOffset = Main.rand.NextVector2Circular(6f + layer * 3f, 6f + layer * 3f);
                    Vector2 flareVel = -Projectile.velocity * (0.08f + layer * 0.03f) + Main.rand.NextVector2Circular(1.2f, 1.2f);
                    Color flareColor = Color.Lerp(SummerYellow, SummerRed, layer * 0.5f + Main.rand.NextFloat(0.3f));
                    var flare = new GenericGlowParticle(Projectile.Center + flareOffset, flareVel, flareColor * (0.5f - layer * 0.15f), 0.28f - layer * 0.06f, 18 + layer * 4, true);
                    MagnumParticleHandler.SpawnParticle(flare);
                }
            }
            
            // PROMINENCE SPARK TRAIL - Orbiting flares shed sparks
            if (Projectile.timeLeft % 2 == 0)
            {
                int promIndex = Projectile.timeLeft % prominenceAngles.Length;
                Vector2 promPos = Projectile.Center + prominenceAngles[promIndex].ToRotationVector2() * prominenceExtensions[promIndex];
                var spark = new SparkleParticle(promPos, -Projectile.velocity * 0.05f, SummerYellow * 0.7f, 0.18f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // MUSIC NOTES - Summer's blazing symphony
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1.8f, -0.6f));
                Color noteColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat(0.5f));
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), noteVel, noteColor * 0.85f, 0.75f, 35);
            }
            
            // SPARKLE SHIMMER - Sunlight gleam
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), -Projectile.velocity * 0.05f, SummerWhite * 0.8f, 0.28f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Leave burning pillars periodically
            if (Projectile.timeLeft % 25 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<SolarPillarProjectile>(), Projectile.damage / 2, 0f, Projectile.owner);
            }

            // Intense solar lighting
            float lightPulse = 0.6f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * lightPulse);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 300);
            target.AddBuff(BuffID.Daybreak, 180);

            // SOLAR IMPACT - Supernova burst
            CustomParticles.GenericFlare(target.Center, SummerWhite, 0.9f, 22);
            CustomParticles.GenericFlare(target.Center, SummerGold, 0.7f, 20);
            
            // Cascading heat halos
            for (int i = 0; i < 5; i++)
            {
                Color haloColor = Color.Lerp(SummerYellow, SummerRed, i / 5f);
                CustomParticles.HaloRing(target.Center, haloColor * (0.5f - i * 0.08f), 0.3f + i * 0.12f, 14 + i * 2);
            }

            // Radial solar burst
            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 10f);
                Color burstColor = i % 2 == 0 ? SummerGold : SummerOrange;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor * 0.5f, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            ThemedParticles.MusicNoteBurst(target.Center, SummerGold * 0.75f, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst1").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D sparkleTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle6").Value;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 sparkleOrigin = sparkleTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.1f + 1f;

            // HEAT WAVE TRAIL - Blazing fire trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Outer heat glow
                Color heatColor = Color.Lerp(SummerRed, SummerOrange, progress) * (1f - progress) * 0.4f;
                float heatScale = 0.45f * (1f - progress * 0.5f);
                sb.Draw(glowTex, trailPos, null, heatColor with { A = 0 }, solarRotation + i * 0.2f, glowOrigin, heatScale, SpriteEffects.None, 0f);
                
                // Inner gold core trail
                Color coreColor = Color.Lerp(SummerGold, SummerYellow, progress) * (1f - progress) * 0.5f;
                float coreScale = 0.3f * (1f - progress * 0.6f);
                sb.Draw(coreTex, trailPos, null, coreColor with { A = 0 }, Projectile.oldRot[i], coreOrigin, coreScale, SpriteEffects.None, 0f);
            }
            
            // OUTER CORONA - Large diffuse solar halo
            sb.Draw(glowTex, drawPos, null, (SummerRed * 0.3f) with { A = 0 }, solarRotation, glowOrigin, 0.65f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (SummerOrange * 0.4f) with { A = 0 }, -solarRotation * 0.6f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING PROMINENCES - Solar flare arcs
            for (int i = 0; i < prominenceAngles.Length; i++)
            {
                Vector2 promOffset = prominenceAngles[i].ToRotationVector2() * prominenceExtensions[i];
                Vector2 promPos = drawPos + promOffset;
                float promScale = 0.14f + (float)Math.Sin(Main.GameUpdateCount * 0.18f + i * 0.7f) * 0.04f;
                Color promColor = (i % 2 == 0 ? SummerOrange : SummerYellow) * 0.8f;
                sb.Draw(sparkleTex, promPos, null, promColor with { A = 0 }, prominenceAngles[i], sparkleOrigin, promScale, SpriteEffects.None, 0f);
            }
            
            // SOLAR CORE - Blazing sun center
            sb.Draw(coreTex, drawPos, null, (SummerOrange * 0.5f) with { A = 0 }, solarRotation, coreOrigin, 0.38f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (SummerGold * 0.7f) with { A = 0 }, -solarRotation * 1.2f, coreOrigin, 0.28f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (SummerWhite * 0.9f) with { A = 0 }, solarRotation * 0.5f, coreOrigin, 0.16f * pulse, SpriteEffects.None, 0f);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // SOLAR EXPLOSION - Mini supernova
            CustomParticles.GenericFlare(Projectile.Center, SummerWhite, 0.8f, 20);

            for (int i = 0; i < 16; i++)
            {
                float angle = MathHelper.TwoPi * i / 16f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 9f);
                Color burstColor = i % 3 == 0 ? SummerRed : (i % 2 == 0 ? SummerGold : SummerOrange);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.5f, 0.28f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
            
            // Star sparkle accents
            for (int i = 0; i < 6; i++)
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), Main.rand.NextVector2Circular(2.5f, 2.5f), SummerYellow * 0.7f, 0.28f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            ThemedParticles.MusicNoteBurst(Projectile.Center, SummerGold * 0.7f, 7, 4f);
            ThemedParticles.MusicNoteRing(Projectile.Center, SummerOrange * 0.6f, 50f, 6);
        }
    }

    /// <summary>
    /// Solar Pillar - UNIQUE BLAZING FIRE PILLAR
    /// A towering column of solar flame with rising embers
    /// </summary>
    public class SolarPillarProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerYellow = new Color(255, 255, 100);

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 60;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.velocity = Vector2.Zero;

            // Rising flames
            if (Main.rand.NextBool(2))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(15f, 25f);
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-4f, -2f));
                Color particleColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat()) * 0.5f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Pillar flame harmony (VISIBLE SCALE 0.72f+)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-2f, -1f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SummerGold * 0.8f, 0.72f, 32);
            }
            
            // ☁ESPARKLE ACCENT - Flame sparkle
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 20f), new Vector2(0, -1f), SummerOrange, 0.22f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.5f * (Projectile.timeLeft / 120f));
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);

            // ☁EMUSICAL IMPACT - Pillar chime
            ThemedParticles.MusicNoteBurst(target.Center, SummerOrange * 0.6f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo4").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = Projectile.timeLeft / 120f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.15f + 1f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Vertical pillar (stretch effect)
            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.35f * lifeProgress, 0f, origin, new Vector2(0.4f, 0.9f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.5f * lifeProgress, 0f, origin, new Vector2(0.25f, 0.7f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Autumn Passage - Decaying orbs that drain life
    /// Unique visual: Swirling amber orb with orbiting falling leaves
    /// </summary>
    public class AutumnPassageProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(180, 50, 30);
        private static readonly Color AutumnGold = new Color(218, 165, 32);
        private static readonly Color AutumnDecay = new Color(80, 60, 30);

        // Orbiting leaf particles - 5 leaves spinning
        private float[] leafAngles = new float[5];
        private float[] leafDistances = new float[5];
        private float decayRotation;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 22;
            Projectile.height = 22;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 4;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;

            // Initialize orbiting leaves
            for (int i = 0; i < leafAngles.Length; i++)
            {
                leafAngles[i] = MathHelper.TwoPi * i / leafAngles.Length;
                leafDistances[i] = 10f + Main.rand.NextFloat(4f);
            }
        }

        public override void AI()
        {
            decayRotation += 0.04f;
            Projectile.rotation += 0.08f;
            Projectile.velocity *= 0.99f;

            // Update orbiting leaves with flutter
            for (int i = 0; i < leafAngles.Length; i++)
            {
                leafAngles[i] += 0.06f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 0.02f;
                leafDistances[i] = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i * 0.8f) * 4f;
            }

            // Falling leaf particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(8f, 8f);
                // Fall down and sideways like real leaves
                Vector2 trailVel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.5f, 2f));
                Color trailColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.55f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.22f, 25, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // MUSICAL NOTATION - Autumn decay whisper (VISIBLE SCALE 0.75f+)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, AutumnGold * 0.85f, 0.75f, 35);
            }
            
            // GLYPH ACCENT - Autumn decay runes
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), AutumnBrown * 0.7f, 0.28f, -1);
            }

            // Leave decay fields
            if (Projectile.timeLeft % 30 == 0 && Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, Vector2.Zero,
                    ModContent.ProjectileType<DecayFieldProjectile>(), Projectile.damage / 4, 0f, Projectile.owner);
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 240);
            target.AddBuff(BuffID.ShadowFlame, 180);

            // Life steal
            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 15));

            CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.6f, 20);
            CustomParticles.HaloRing(target.Center, AutumnBrown * 0.5f, 0.45f, 16);

            // Decay burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(AutumnOrange, AutumnGold, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // MUSICAL IMPACT - Autumn harvest chord
            ThemedParticles.MusicNoteBurst(target.Center, AutumnOrange * 0.75f, 5, 3.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            // Load textures - using unique autumn textures
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow3").Value;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo4").Value;
            Texture2D leafTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle5").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 leafOrigin = leafTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // FALLING LEAF TRAIL - Leaves drifting behind
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Amber glow trail
                Color trailColor = Color.Lerp(AutumnGold, AutumnDecay, progress) * (1f - progress) * 0.4f;
                float trailScale = 0.4f * (1f - progress * 0.5f);
                sb.Draw(glowTex, trailPos, null, trailColor with { A = 0 }, decayRotation + i * 0.3f, glowOrigin, trailScale, SpriteEffects.None, 0f);
                
                // Occasional leaf silhouette in trail
                if (i % 3 == 0)
                {
                    Color leafColor = Color.Lerp(AutumnOrange, AutumnBrown, progress) * (1f - progress) * 0.35f;
                    float leafAngle = i * 0.7f + Main.GameUpdateCount * 0.05f;
                    sb.Draw(leafTex, trailPos + new Vector2(0, i * 0.5f), null, leafColor with { A = 0 }, leafAngle, leafOrigin, 0.12f * (1f - progress), SpriteEffects.None, 0f);
                }
            }
            
            // OUTER DECAY AURA - Warm amber haze
            sb.Draw(glowTex, drawPos, null, (AutumnDecay * 0.3f) with { A = 0 }, decayRotation, glowOrigin, 0.65f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (AutumnBrown * 0.4f) with { A = 0 }, -decayRotation * 0.7f, glowOrigin, 0.5f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING LEAVES - Dancing falling leaves
            for (int i = 0; i < leafAngles.Length; i++)
            {
                Vector2 leafOffset = leafAngles[i].ToRotationVector2() * leafDistances[i];
                Vector2 leafPos = drawPos + leafOffset;
                float leafScale = 0.11f + (float)Math.Sin(Main.GameUpdateCount * 0.12f + i * 0.6f) * 0.025f;
                // Alternate colors: orange, gold, brown, red
                Color[] leafColors = { AutumnOrange, AutumnGold, AutumnBrown, AutumnRed, AutumnOrange };
                Color leafColor = leafColors[i % 5] * 0.75f;
                float leafRot = leafAngles[i] + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i) * 0.5f; // Flutter
                sb.Draw(leafTex, leafPos, null, leafColor with { A = 0 }, leafRot, leafOrigin, leafScale, SpriteEffects.None, 0f);
            }
            
            // AUTUMN ORB CORE - Layered warm glow
            sb.Draw(coreTex, drawPos, null, (AutumnBrown * 0.5f) with { A = 0 }, decayRotation, coreOrigin, 0.38f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (AutumnOrange * 0.7f) with { A = 0 }, -decayRotation * 1.2f, coreOrigin, 0.28f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (AutumnGold * 0.9f) with { A = 0 }, decayRotation * 0.5f, coreOrigin, 0.16f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // AUTUMN SCATTER - Leaves explode outward
            CustomParticles.GenericFlare(Projectile.Center, AutumnGold, 0.7f, 18);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 6f);
                // Autumn leaf colors
                Color[] burstColors = { AutumnOrange, AutumnGold, AutumnBrown, AutumnRed };
                Color burstColor = burstColors[i % 4] * 0.55f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // MUSICAL FINALE - Autumn passage farewell
            ThemedParticles.MusicNoteBurst(Projectile.Center, AutumnGold * 0.7f, 6, 3.5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, AutumnBrown * 0.6f, 45f, 6);
        }
    }

    /// <summary>
    /// Decay Field - Area left by Autumn Passage
    /// Unique visual: Withered leaf circle with rising amber wisps
    /// </summary>
    public class DecayFieldProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo2";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnDecay = new Color(80, 60, 30);
        private static readonly Color AutumnGold = new Color(218, 165, 32);

        private float fieldRotation;

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 80;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
        }

        public override void AI()
        {
            fieldRotation += 0.015f;
            Projectile.velocity = Vector2.Zero;
            Projectile.alpha = (int)(80 + (175 * (1f - Projectile.timeLeft / 90f)));

            float lifeProgress = Projectile.timeLeft / 90f;

            // Rising decay wisps
            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(22f, 22f);
                Vector2 particleVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-2.5f, -1f));
                Color particleColor = Color.Lerp(AutumnGold, AutumnDecay, Main.rand.NextFloat()) * 0.45f * lifeProgress;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // MUSICAL NOTATION - Decay field dirge (VISIBLE SCALE 0.7f+)
            if (Main.rand.NextBool(10))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, AutumnGold * 0.75f * lifeProgress, 0.7f, 30);
            }
            
            // GLYPH ACCENT - Field decay runes
            if (Main.rand.NextBool(15))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(18f, 18f), AutumnOrange * 0.5f * lifeProgress, 0.22f, -1);
            }

            Lighting.AddLight(Projectile.Center, AutumnGold.ToVector3() * 0.25f * lifeProgress);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.ShadowFlame, 60);

            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 25));

            // MUSICAL IMPACT - Decay siphon note
            ThemedParticles.MusicNoteBurst(target.Center, AutumnGold * 0.55f, 2, 2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            // Load textures
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D haloTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo2").Value;
            Texture2D leafTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle5").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 haloOrigin = haloTex.Size() / 2f;
            Vector2 leafOrigin = leafTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float lifeProgress = Projectile.timeLeft / 90f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // DECAY FIELD HALO - Ring of withering
            sb.Draw(haloTex, drawPos, null, (AutumnDecay * 0.3f * lifeProgress) with { A = 0 }, fieldRotation, haloOrigin, 0.5f, SpriteEffects.None, 0f);
            sb.Draw(haloTex, drawPos, null, (AutumnBrown * 0.4f * lifeProgress) with { A = 0 }, -fieldRotation * 0.7f, haloOrigin, 0.4f, SpriteEffects.None, 0f);
            
            // ORBITING DECAY LEAVES - Slow spin of withered leaves
            int leafCount = 6;
            for (int i = 0; i < leafCount; i++)
            {
                float leafAngle = fieldRotation * 0.5f + MathHelper.TwoPi * i / leafCount;
                float leafRadius = 20f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 3f;
                Vector2 leafPos = drawPos + leafAngle.ToRotationVector2() * leafRadius;
                Color[] leafColors = { AutumnOrange, AutumnBrown, AutumnGold, AutumnDecay };
                Color leafColor = leafColors[i % 4] * 0.5f * lifeProgress;
                sb.Draw(leafTex, leafPos, null, leafColor with { A = 0 }, leafAngle + i, leafOrigin, 0.1f, SpriteEffects.None, 0f);
            }
            
            // CENTER GLOW - Fading amber heart
            sb.Draw(glowTex, drawPos, null, (AutumnGold * 0.35f * lifeProgress) with { A = 0 }, 0f, glowOrigin, 0.35f, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (AutumnOrange * 0.25f * lifeProgress) with { A = 0 }, 0f, glowOrigin, 0.25f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Winter Finale - Blizzard burst with freeze chance
    /// Unique visual: Swirling snowflake/ice crystal orb with orbiting ice shards
    /// </summary>
    public class WinterFinaleProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst2";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);
        private static readonly Color WinterPurple = new Color(180, 160, 255);
        private static readonly Color WinterCyan = new Color(180, 240, 255);
        private static readonly Color WinterFrost = new Color(200, 220, 240);

        // Orbiting ice crystals - 4 shards spinning
        private float[] shardAngles = new float[4];
        private float[] shardDistances = new float[4];
        private float frostRotation;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 30;
            Projectile.extraUpdates = 1;

            // Initialize orbiting ice shards
            for (int i = 0; i < shardAngles.Length; i++)
            {
                shardAngles[i] = MathHelper.TwoPi * i / shardAngles.Length;
                shardDistances[i] = 8f + Main.rand.NextFloat(3f);
            }
        }

        public override void AI()
        {
            frostRotation += 0.05f;
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Update orbiting ice shards
            for (int i = 0; i < shardAngles.Length; i++)
            {
                shardAngles[i] += 0.07f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i) * 0.01f;
                shardDistances[i] = 8f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + i * 0.7f) * 3f;
            }

            // Snowflake particles
            if (Main.rand.NextBool(2))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(6f, 6f);
                Vector2 trailVel = -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(1f, 1f);
                Color trailColor = Color.Lerp(WinterCyan, WinterWhite, Main.rand.NextFloat()) * 0.5f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Winter frost chime (VISIBLE SCALE 0.75f+)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, WinterBlue * 0.85f, 0.75f, 35);
            }
            
            // ☁ESPARKLE ACCENT - Frost crystal shimmer
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), -Projectile.velocity * 0.06f, WinterWhite, 0.28f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, WinterBlue.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 300);

            // 35% freeze chance
            if (Main.rand.NextFloat() < 0.35f)
            {
                target.AddBuff(BuffID.Frozen, 90);

                // Shatter damage to nearby enemies
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist < 80f)
                        {
                            npc.SimpleStrikeNPC(Projectile.damage / 4, hit.HitDirection, false, Projectile.knockBack * 0.3f, DamageClass.Magic);
                            npc.AddBuff(BuffID.Frostburn2, 120);
                        }
                    }
                }

                // Shatter VFX
                CustomParticles.GenericFlare(target.Center, WinterWhite, 0.8f, 22);
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    var shard = new GenericGlowParticle(target.Center, shardVel, WinterBlue * 0.6f, 0.22f, 18, true);
                    MagnumParticleHandler.SpawnParticle(shard);
                }
            }

            CustomParticles.GenericFlare(target.Center, WinterBlue, 0.65f, 20);
            CustomParticles.HaloRing(target.Center, WinterWhite * 0.5f, 0.45f, 16);

            // Ice burst
            for (int i = 0; i < 10; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(6f, 6f);
                Color burstColor = Color.Lerp(WinterBlue, WinterPurple, Main.rand.NextFloat()) * 0.5f;
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // MUSICAL IMPACT - Winter shatter symphony
            ThemedParticles.MusicNoteBurst(target.Center, WinterCyan * 0.75f, 6, 4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            // Load textures - unique ice/frost textures
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow4").Value;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst2").Value;
            Texture2D shardTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle3").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 shardOrigin = shardTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // FROST TRAIL - Icy mist behind
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                
                // Icy mist trail
                Color mistColor = Color.Lerp(WinterCyan, WinterPurple, progress) * (1f - progress) * 0.45f;
                float mistScale = 0.4f * (1f - progress * 0.5f);
                sb.Draw(glowTex, trailPos, null, mistColor with { A = 0 }, frostRotation + i * 0.2f, glowOrigin, mistScale, SpriteEffects.None, 0f);
                
                // Occasional ice crystal in trail
                if (i % 3 == 0)
                {
                    Color crystalColor = WinterWhite * (1f - progress) * 0.4f;
                    sb.Draw(shardTex, trailPos, null, crystalColor with { A = 0 }, frostRotation * 2f + i, shardOrigin, 0.1f * (1f - progress), SpriteEffects.None, 0f);
                }
            }
            
            // OUTER FROST AURA - Icy blue haze
            sb.Draw(glowTex, drawPos, null, (WinterPurple * 0.3f) with { A = 0 }, frostRotation, glowOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
            sb.Draw(glowTex, drawPos, null, (WinterCyan * 0.4f) with { A = 0 }, -frostRotation * 0.7f, glowOrigin, 0.42f * pulse, SpriteEffects.None, 0f);
            
            // ORBITING ICE SHARDS - Spinning crystals
            for (int i = 0; i < shardAngles.Length; i++)
            {
                Vector2 shardOffset = shardAngles[i].ToRotationVector2() * shardDistances[i];
                Vector2 shardPos = drawPos + shardOffset;
                float shardScale = 0.12f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i * 0.7f) * 0.03f;
                Color[] shardColors = { WinterWhite, WinterCyan, WinterBlue, WinterPurple };
                Color shardColor = shardColors[i % 4] * 0.8f;
                sb.Draw(shardTex, shardPos, null, shardColor with { A = 0 }, shardAngles[i] * 1.5f, shardOrigin, shardScale, SpriteEffects.None, 0f);
            }
            
            // SNOWFLAKE CORE - Layered ice crystal center
            sb.Draw(coreTex, drawPos, null, (WinterPurple * 0.45f) with { A = 0 }, frostRotation, coreOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (WinterCyan * 0.65f) with { A = 0 }, -frostRotation * 1.2f, coreOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (WinterWhite * 0.85f) with { A = 0 }, frostRotation * 0.5f, coreOrigin, 0.15f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // BLIZZARD SCATTER - Ice explodes outward
            CustomParticles.GenericFlare(Projectile.Center, WinterWhite, 0.75f, 20);
            CustomParticles.HaloRing(Projectile.Center, WinterCyan * 0.6f, 0.5f, 16);

            // Spawn ice shards on death
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f;
                    Vector2 shardVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 8f);
                    Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, shardVel,
                        ModContent.ProjectileType<FinaleIceShardProjectile>(), Projectile.damage / 3, Projectile.knockBack * 0.3f, Projectile.owner);
                }
            }

            for (int i = 0; i < 14; i++)
            {
                float angle = MathHelper.TwoPi * i / 14f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                Color[] burstColors = { WinterCyan, WinterBlue, WinterPurple, WinterWhite };
                Color burstColor = burstColors[i % 4] * 0.55f;
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // MUSICAL FINALE - Winter finale grand conclusion
            ThemedParticles.MusicNoteBurst(Projectile.Center, WinterCyan * 0.7f, 8, 4f);
            ThemedParticles.MusicNoteRing(Projectile.Center, WinterWhite * 0.6f, 55f, 8);
        }
    }

    /// <summary>
    /// Finale Ice Shard - Secondary projectile from Winter Finale
    /// Unique visual: Small spinning ice crystal with frost trail
    /// </summary>
    public class FinaleIceShardProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle3";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterCyan = new Color(180, 240, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);

        private float spinRotation;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
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
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            spinRotation += 0.2f;
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.velocity.Y += 0.08f;

            // Frost mist trail
            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f + Main.rand.NextVector2Circular(0.5f, 0.5f), WinterCyan * 0.4f, 0.12f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // MUSICAL NOTATION - Ice shard tinkle (VISIBLE SCALE 0.68f+)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, WinterCyan * 0.8f, 0.68f, 28);
            }
            
            // SPARKLE ACCENT - Ice crystal twinkle
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.08f, WinterWhite, 0.18f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, WinterCyan.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
            CustomParticles.GenericFlare(target.Center, WinterCyan, 0.4f, 14);

            // MUSICAL IMPACT - Ice shard ping
            ThemedParticles.MusicNoteBurst(target.Center, WinterCyan * 0.6f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D shardTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle3").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 shardOrigin = shardTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // FROST TRAIL - Short icy mist
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                Color trailColor = Color.Lerp(WinterCyan, WinterWhite, progress) * (1f - progress) * 0.4f;
                sb.Draw(glowTex, trailPos, null, trailColor with { A = 0 }, 0f, glowOrigin, 0.15f * (1f - progress), SpriteEffects.None, 0f);
            }

            // ICE SHARD - Spinning crystal
            sb.Draw(glowTex, drawPos, null, (WinterCyan * 0.35f) with { A = 0 }, 0f, glowOrigin, 0.2f, SpriteEffects.None, 0f);
            sb.Draw(shardTex, drawPos, null, (WinterWhite * 0.8f) with { A = 0 }, spinRotation, shardOrigin, 0.18f, SpriteEffects.None, 0f);
            sb.Draw(shardTex, drawPos, null, (WinterCyan * 0.6f) with { A = 0 }, -spinRotation * 0.7f, shardOrigin, 0.12f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
