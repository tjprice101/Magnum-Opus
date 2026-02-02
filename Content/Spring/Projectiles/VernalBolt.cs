using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// Vernal Bolt - Primary projectile for Vernal Scepter
    /// Splits into 4 homing petals after 30 frames of flight.
    /// Critical hits spawn healing particles.
    /// </summary>
    public class VernalBolt : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringLavender = new Color(200, 180, 220);

        private bool hasSplit = false;
        private float orbitAngle = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 240;
            Projectile.light = 0.5f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/StarBurst2";

        public override void AI()
        {
            orbitAngle += 0.12f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // === VFX VARIATION #6: FLOWING SINE-WAVE TRAIL ===
            // Particles follow a sinusoidal wave pattern behind the projectile
            if (Main.rand.NextBool(2))
            {
                float sineOffset = (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 8f;
                Vector2 perpendicular = Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2);
                Vector2 trailPos = Projectile.Center + perpendicular * sineOffset + Main.rand.NextVector2Circular(3f, 3f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + perpendicular * (float)Math.Cos(Main.GameUpdateCount * 0.2f) * 1.5f;
                Color trailColor = Color.Lerp(SpringLavender, SpringPink, Main.rand.NextFloat()) * 0.75f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.34f, 24, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Sparkle accents for magical shimmer
                var sparkle = new SparkleParticle(trailPos, trailVel * 1.2f, SpringWhite * 0.65f, 0.27f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // === VFX VARIATION #7: HELIX DOUBLE-TRAIL ===
            // Two intertwined particle streams spiral around the bolt
            if (Main.GameUpdateCount % 3 == 0)
            {
                for (int h = 0; h < 2; h++)
                {
                    float helixAngle = orbitAngle * 1.5f + MathHelper.Pi * h;
                    float helixRadius = 10f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 3f;
                    Vector2 helixPos = Projectile.Center + helixAngle.ToRotationVector2() * helixRadius;
                    Color helixColor = h == 0 ? SpringPink * 0.7f : SpringGreen * 0.7f;
                    var helix = new GenericGlowParticle(helixPos, -Projectile.velocity * 0.1f, helixColor, 0.2f, 18, true);
                    MagnumParticleHandler.SpawnParticle(helix);
                }
            }

            // === VFX VARIATION #8: STARDUST COMET TAIL ===
            // Sparkling dust particles stream behind like a comet
            if (Main.rand.NextBool(3))
            {
                Vector2 dustOffset = Main.rand.NextVector2Circular(8f, 8f);
                Vector2 dustVel = -Projectile.velocity * Main.rand.NextFloat(0.15f, 0.3f) + Main.rand.NextVector2Circular(1f, 1f);
                Color dustColor = Color.Lerp(SpringWhite, SpringLavender, Main.rand.NextFloat()) * 0.55f;
                var dust = new GenericGlowParticle(Projectile.Center + dustOffset, dustVel, dustColor, 0.16f, 28, true);
                MagnumParticleHandler.SpawnParticle(dust);
                
                // Tiny sparkle dust
                var sparkDust = new SparkleParticle(Projectile.Center + dustOffset * 0.5f, dustVel * 0.8f, SpringWhite * 0.5f, 0.18f, 16);
                MagnumParticleHandler.SpawnParticle(sparkDust);
            }

            // Orbiting spark points - enhanced with glow
            if (Projectile.timeLeft % 5 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float sparkAngle = orbitAngle + MathHelper.TwoPi * i / 3f;
                    Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 14f;
                    Color sparkColor = Color.Lerp(SpringPink, SpringGreen, (float)i / 3f);
                    CustomParticles.GenericFlare(sparkPos, sparkColor * 0.65f, 0.24f, 8);
                }
            }

            // Vanilla dust for density
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, -Projectile.velocity * 0.1f, 0, SpringPink, 0.95f);
                dust.noGravity = true;
            }

            // === VFX VARIATION #9: ORBITING MUSIC NOTE RING ===
            // Music notes orbit in a steady ring formation
            if (Main.GameUpdateCount % 8 == 0)
            {
                float noteRingAngle = Main.GameUpdateCount * 0.08f;
                for (int n = 0; n < 2; n++)
                {
                    float noteAngle = noteRingAngle + MathHelper.Pi * n;
                    Vector2 notePos = Projectile.Center + noteAngle.ToRotationVector2() * 16f;
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.2f, 0.2f), Main.rand.NextFloat(-0.8f, -0.3f));
                    Color noteColor = Color.Lerp(SpringPink, SpringLavender, (float)n / 2f);
                    ThemedParticles.MusicNote(notePos, noteVel, noteColor, 0.75f, 32);
                }
            }

            // Split after 30 frames
            if (!hasSplit && Projectile.timeLeft <= 210)
            {
                hasSplit = true;
                SplitIntoPetals();
            }

            // Dynamic lighting
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.15f + 0.85f;
            Lighting.AddLight(Projectile.Center, SpringLavender.ToVector3() * pulse * 0.65f);
        }

        private void SplitIntoPetals()
        {
            // Split VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(Projectile.Center, SpringLavender, 0.65f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SpringPink * 0.7f, 0.45f, 15);
            
            // Petal sparkle ring
            for (int s = 0; s < 4; s++)
            {
                float sparkAngle = MathHelper.TwoPi * s / 4f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 15f;
                CustomParticles.GenericFlare(sparkPos, SpringPink * 0.8f, 0.2f, 12);
            }

            // Spawn 4 homing petals in X pattern
            if (Main.myPlayer == Projectile.owner)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = Projectile.velocity.ToRotation() + MathHelper.PiOver4 + MathHelper.PiOver2 * i;
                    Vector2 petalVel = angle.ToRotationVector2() * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, petalVel, 
                        ModContent.ProjectileType<HomingSpringPetal>(), Projectile.damage * 2 / 3, Projectile.knockBack * 0.5f, Projectile.owner);
                }
            }

            // Sparkle burst
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 sparkleVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                Color sparkleColor = Color.Lerp(SpringPink, SpringGreen, (float)i / 8f);
                var sparkle = new GenericGlowParticle(Projectile.Center, sparkleVel, sparkleColor * 0.7f, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - VISIBLE notes burst! (scale 0.75f)
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 noteVel = angle.ToRotationVector2() * 3f;
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, i / 6f);
                ThemedParticles.MusicNote(target.Center, noteVel, noteColor, 0.75f, 35);
            }
            
            // Central flash
            CustomParticles.GenericFlare(target.Center, SpringWhite, 0.6f, 18);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.5f, 15);
            
            // Sparkle ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * 3.5f;
                var sparkle = new SparkleParticle(target.Center, sparkVel, SpringWhite * 0.8f, 0.35f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Bloom Burst: Critical hits spawn healing particles
            if (hit.Crit)
            {
                CustomParticles.GenericFlare(target.Center, SpringGreen, 0.75f, 22);
                
                // Spawn healing flower pickup
                if (Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                        ModContent.ProjectileType<HealingFlower>(), 0, 0f, Projectile.owner, 5); // Heal 5 HP
                }

                // Green healing sparkles
                for (int i = 0; i < 6; i++)
                {
                    Vector2 sparklePos = target.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 sparkleVel = new Vector2(0, -Main.rand.NextFloat(1f, 3f));
                    var sparkle = new GenericGlowParticle(sparklePos, sparkleVel, SpringGreen * 0.8f, 0.35f, 30, true);
                    MagnumParticleHandler.SpawnParticle(sparkle);
                }
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (hasSplit) return; // Don't do kill VFX if we split

            // ☁EMUSICAL FINALE - VISIBLE notes scatter! (scale 0.8f)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * 4f;
                Color noteColor = Color.Lerp(SpringPink, SpringLavender, i / 8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.8f, 40);
            }

            // Bloom VFX on death - layered flares
            CustomParticles.GenericFlare(Projectile.Center, SpringWhite, 0.65f, 20);
            CustomParticles.GenericFlare(Projectile.Center, SpringLavender, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SpringPink * 0.6f, 0.38f, 14);
            
            // Sparkle ring
            for (int s = 0; s < 6; s++)
            {
                float sparkAngle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 15f;
                var sparkle = new SparkleParticle(sparkPos, sparkAngle.ToRotationVector2() * 2f, 
                    SpringWhite * 0.7f, 0.3f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(3f, 7f);
                Color burstColor = Color.Lerp(SpringLavender, SpringGreen, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.7f, 0.3f, 22, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 1f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                float trailScale = (1f - progress * 0.5f) * 0.8f;
                Color trailColor = Color.Lerp(SpringLavender, SpringPink, progress) * trailAlpha * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale * pulse, SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Multi-layer bloom
            spriteBatch.Draw(texture, drawPos, null, SpringLavender * 0.3f, Projectile.rotation, origin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.4f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.5f, Projectile.rotation, origin, 0.25f * pulse, SpriteEffects.None, 0f);

            // Orbiting spark renders
            for (int i = 0; i < 2; i++)
            {
                float sparkAngle = orbitAngle + MathHelper.Pi * i;
                Vector2 sparkOffset = sparkAngle.ToRotationVector2() * 10f;
                Color sparkColor = i == 0 ? SpringPink : SpringGreen;
                spriteBatch.Draw(texture, drawPos + sparkOffset, null, sparkColor * 0.5f, 0f, origin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Homing Spring Petal - Split projectile from Vernal Bolt
    /// </summary>
    public class HomingSpringPetal : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        private float rotationSpeed;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.light = 0.3f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle1";

        public override void AI()
        {
            if (rotationSpeed == 0)
                rotationSpeed = Main.rand.NextFloat(0.15f, 0.25f) * (Main.rand.NextBool() ? 1 : -1);

            Projectile.rotation += rotationSpeed;

            // Gentle homing
            NPC target = FindClosestNPC(350f);
            if (target != null)
            {
                Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dirToTarget * 12f, 0.06f);
            }

            // Trail particles
            if (Main.rand.NextBool(3))
            {
                Vector2 trailPos = Projectile.Center + Main.rand.NextVector2Circular(5f, 5f);
                Color trailColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.7f;
                var trail = new GenericGlowParticle(trailPos, -Projectile.velocity * 0.1f, trailColor, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Sparkle dust
            if (Main.rand.NextBool(5))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, Vector2.Zero, 0, SpringPink, 0.7f);
                dust.noGravity = true;
            }

            // ☁EMUSICAL NOTATION - Notes trail behind petals! - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = -Projectile.velocity * 0.04f + new Vector2(0, Main.rand.NextFloat(-0.6f, -0.2f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.9f, 0.7f, 35);
                
                // Spring sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, SpringGreen * 0.4f, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.35f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.45f, 15);
            
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SpringGreen * 0.65f, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                Color trailColor = Color.Lerp(SpringPink, SpringGreen, progress) * trailAlpha * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.6f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.4f, Projectile.rotation, origin, 0.45f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.5f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Homing Flower Bolt - Projectile from Nature's Blessing ability
    /// </summary>
    public class HomingFlowerBolt : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringLavender = new Color(200, 180, 220);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
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
            Projectile.timeLeft = 180;
            Projectile.light = 0.4f;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle3";

        public override void AI()
        {
            Projectile.rotation += 0.18f;

            // Strong homing
            NPC target = FindClosestNPC(500f);
            if (target != null)
            {
                Vector2 dirToTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, dirToTarget * 14f, 0.08f);
            }

            // Flower trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(SpringPink, SpringLavender, Main.rand.NextFloat()) * 0.65f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f, trailColor, 0.28f, 20, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // Sparkle dust
            if (Main.rand.NextBool(4))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, Vector2.Zero, 0, SpringPink, 0.8f);
                dust.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, SpringLavender.ToVector3() * 0.4f);
        }

        private NPC FindClosestNPC(float range)
        {
            NPC closest = null;
            float closestDist = range;

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.friendly || npc.dontTakeDamage) continue;
                float dist = Vector2.Distance(Projectile.Center, npc.Center);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = npc;
                }
            }
            return closest;
        }

        public override void OnKill(int timeLeft)
        {
            // Death VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(Projectile.Center, SpringLavender, 0.55f, 18);
            CustomParticles.GenericFlare(Projectile.Center, SpringGreen * 0.5f, 0.35f, 14);
            
            // Petal sparkle burst
            for (int s = 0; s < 4; s++)
            {
                float sparkAngle = MathHelper.TwoPi * s / 4f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 10f;
                CustomParticles.GenericFlare(sparkPos, SpringGreen * 0.6f, 0.15f, 10);
            }

            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                Color burstColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, burstColor * 0.6f, 0.26f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                float trailAlpha = 1f - progress;
                Color trailColor = Color.Lerp(SpringLavender, SpringGreen, progress) * trailAlpha * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.7f * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(texture, drawPos, null, SpringLavender * 0.35f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.45f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.55f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
