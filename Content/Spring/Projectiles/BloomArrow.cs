using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// BloomArrow - Arrow that splits into homing petals on hit or after traveling a distance
    /// </summary>
    public class BloomArrow : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringLavender = new Color(200, 162, 200);

        private bool hasSplit = false;
        private float[] orbitAngles = new float[5];
        private float arrowRotation;

        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField3";

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.alpha = 0;
            Projectile.light = 0.4f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = true;
            Projectile.arrow = true;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            arrowRotation += 0.05f;
            
            // Update orbiting petal angles
            for (int i = 0; i < orbitAngles.Length; i++)
            {
                orbitAngles[i] += 0.08f + i * 0.015f;
            }

            // === VFX VARIATION #1: ORBITING MUSIC NOTE CONSTELLATION ===
            // Music notes orbit the arrow in a beautiful spiral pattern
            if (Main.GameUpdateCount % 6 == 0)
            {
                for (int n = 0; n < 3; n++)
                {
                    float noteOrbitAngle = arrowRotation * 2f + MathHelper.TwoPi * n / 3f;
                    float noteOrbitRadius = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.1f + n) * 4f;
                    Vector2 noteOrbitPos = Projectile.Center + noteOrbitAngle.ToRotationVector2() * noteOrbitRadius;
                    Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.3f));
                    Color noteColor = Color.Lerp(SpringPink, SpringLavender, (float)n / 3f);
                    ThemedParticles.MusicNote(noteOrbitPos, noteVel, noteColor, 0.78f, 28);
                }
            }

            // === VFX VARIATION #2: CAMERA GLINT/LENS FLARE ===
            // Bright sparkle flashes that catch the eye
            if (Main.rand.NextBool(8))
            {
                float glintAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                Vector2 glintPos = Projectile.Center + glintAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 10f);
                CustomParticles.GenericFlare(glintPos, Color.White, 0.55f, 6);
                CustomParticles.GenericFlare(glintPos, SpringPink, 0.4f, 8);
            }

            // === VFX VARIATION #3: SPIRAL PARTICLE TRAIL ===
            // Particles spiral outward from the arrow path
            if (Main.rand.NextBool(2))
            {
                float spiralAngle = Main.GameUpdateCount * 0.25f;
                Vector2 spiralOffset = spiralAngle.ToRotationVector2() * 8f;
                Vector2 trailPos = Projectile.Center + spiralOffset + Main.rand.NextVector2Circular(2f, 2f);
                Vector2 trailVel = -Projectile.velocity * 0.08f + spiralAngle.ToRotationVector2() * 1.5f;
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.75f;
                var trail = new GenericGlowParticle(trailPos, trailVel, trailColor, 0.32f, 22, true);
                MagnumParticleHandler.SpawnParticle(trail);
                
                // Sparkle accent in spiral
                var sparkle = new SparkleParticle(trailPos, trailVel * 0.5f, SpringWhite * 0.7f, 0.28f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // === VFX VARIATION #4: AMBIENT PETAL MOTES ===
            // Tiny floating particles surround the projectile
            if (Main.rand.NextBool(4))
            {
                float moteAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float moteRadius = Main.rand.NextFloat(12f, 22f);
                Vector2 motePos = Projectile.Center + moteAngle.ToRotationVector2() * moteRadius;
                Vector2 moteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1f, 0.5f));
                Color moteColor = Color.Lerp(SpringGreen, SpringPink, Main.rand.NextFloat()) * 0.5f;
                var mote = new GenericGlowParticle(motePos, moteVel, moteColor, 0.15f, 25, true);
                MagnumParticleHandler.SpawnParticle(mote);
            }
            
            // Vanilla dust for density
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, -Projectile.velocity * 0.15f, 80, SpringPink, 0.85f);
                dust.noGravity = true;
            }

            // === VFX VARIATION #5: RHYTHMIC PULSE PARTICLES ===
            // Particles emit in waves tied to visual rhythm
            if (Main.GameUpdateCount % 12 == 0)
            {
                for (int p = 0; p < 4; p++)
                {
                    float pulseAngle = MathHelper.TwoPi * p / 4f + Main.GameUpdateCount * 0.05f;
                    Vector2 pulseVel = pulseAngle.ToRotationVector2() * 2.5f;
                    Color pulseColor = Color.Lerp(SpringPink, SpringLavender, (float)p / 4f) * 0.6f;
                    var pulseParticle = new GenericGlowParticle(Projectile.Center, pulseVel, pulseColor, 0.22f, 18, true);
                    MagnumParticleHandler.SpawnParticle(pulseParticle);
                }
            }

            // Split into petals after 40 frames of flight if not hit anything
            if (!hasSplit && Projectile.timeLeft <= 140)
            {
                SplitIntoPetals();
            }

            // Pulsing glow
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.15f + 0.5f;
            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * pulse);
        }

        private void SplitIntoPetals()
        {
            hasSplit = true;
            
            // ☁EMUSICAL BURST - VISIBLE notes scatter as petals bloom! (scale 0.75f)
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                Vector2 noteVel = angle.ToRotationVector2() * 3.5f;
                Color noteColor = Color.Lerp(SpringPink, SpringGreen, i / 8f);
                ThemedParticles.MusicNote(Projectile.Center, noteVel, noteColor, 0.75f, 40);
            }
            
            // Spawn 3 homing petals
            for (int i = 0; i < 3; i++)
            {
                float angle = Projectile.velocity.ToRotation() + MathHelper.ToRadians(-30 + i * 30);
                Vector2 petalVel = angle.ToRotationVector2() * Projectile.velocity.Length() * 0.8f;
                
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    Projectile.Center,
                    petalVel,
                    ModContent.ProjectileType<HomingPetal>(),
                    Projectile.damage / 2,
                    Projectile.knockBack * 0.5f,
                    Projectile.owner
                );
            }

            // Split VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.6f, 15);
            CustomParticles.GenericFlare(Projectile.Center, SpringWhite * 0.6f, 0.4f, 12);
            
            // Petal sparkle burst
            for (int s = 0; s < 4; s++)
            {
                float sparkAngle = MathHelper.TwoPi * s / 4f;
                Vector2 sparkPos = Projectile.Center + sparkAngle.ToRotationVector2() * 12f;
                CustomParticles.GenericFlare(sparkPos, SpringWhite * 0.7f, 0.18f, 10);
            }
            
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SpringPink * 0.8f, 0.25f, 20, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (!hasSplit)
            {
                SplitIntoPetals();
            }

            // Pollination: 15% chance to spawn healing flower
            if (Main.rand.NextFloat() < 0.15f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<HealingFlower>(),
                    0,
                    0,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D coreTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField3").Value;
            Texture2D petalTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle1").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 coreOrigin = coreTex.Size() / 2f;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.15f) * 0.1f + 1f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ORBITING PETALS - 5 small flower petals spin around the arrow
            for (int i = 0; i < orbitAngles.Length; i++)
            {
                float orbitRadius = 14f + (float)Math.Sin(arrowRotation + i * 1.2f) * 3f;
                Vector2 petalPos = drawPos + orbitAngles[i].ToRotationVector2() * orbitRadius;
                float petalProgress = (float)i / orbitAngles.Length;
                Color petalColor = Color.Lerp(SpringPink, SpringWhite, petalProgress);
                sb.Draw(petalTex, petalPos, null, (petalColor * 0.7f) with { A = 0 }, orbitAngles[i], petalOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            // ARROW GLOW - Pink aura around arrow
            sb.Draw(glowTex, drawPos, null, (SpringPink * 0.4f) with { A = 0 }, 0f, glowOrigin, 0.35f * pulse, SpriteEffects.None, 0f);
            
            // Arrow-shaped core elongated in direction of travel
            sb.Draw(coreTex, drawPos, null, (SpringPink * 0.7f) with { A = 0 }, Projectile.rotation, coreOrigin, new Vector2(0.25f, 0.45f) * pulse, SpriteEffects.None, 0f);
            sb.Draw(coreTex, drawPos, null, (SpringWhite * 0.5f) with { A = 0 }, Projectile.rotation, coreOrigin, new Vector2(0.15f, 0.3f) * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // ☁ELAYERED DEATH - Multiple particle types!
            
            // Central flash
            CustomParticles.GenericFlare(Projectile.Center, SpringWhite, 0.5f, 15);
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.4f, 12);
            
            // Sparkle ring
            for (int i = 0; i < 6; i++)
            {
                float angle = MathHelper.TwoPi * i / 6f;
                Vector2 sparkVel = angle.ToRotationVector2() * 2f;
                var sparkle = new SparkleParticle(Projectile.Center, sparkVel, 
                    Color.Lerp(SpringPink, SpringWhite, i / 6f) * 0.8f, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
            
            // Dust for density
            for (int i = 0; i < 6; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 100, SpringPink, 0.9f);
                dust.noGravity = true;
            }
            
            // VISIBLE farewell note (scale 0.75f)
            ThemedParticles.MusicNote(Projectile.Center, new Vector2(0, -1f), SpringPink, 0.75f, 35);
        }
    }

    /// <summary>
    /// HomingPetal - Seeks nearby enemies and damages them
    /// </summary>
    public class HomingPetal : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private float petalSpin;

        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle5";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.alpha = 0;
            Projectile.light = 0.3f;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            petalSpin += 0.12f;
            
            // Homing behavior
            NPC target = FindClosestNPC(400f);
            if (target != null)
            {
                Vector2 toTarget = target.Center - Projectile.Center;
                toTarget.Normalize();
                float homingStrength = 0.12f;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toTarget * 10f, homingStrength);
            }

            // Gentle floating motion
            Projectile.velocity.Y += (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.05f;

            // Spin
            Projectile.rotation += 0.15f;

            // Trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(SpringPink, SpringWhite, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.15f, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // MUSICAL NOTATION - VISIBLE notes float from petals! (scale 0.7f)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = -Projectile.velocity * 0.03f + new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.8f, -0.2f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.85f, 0.7f, 38);
            }
            
            // Sparkle accents for magical feel
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(5f, 5f), 
                    -Projectile.velocity * 0.1f, SpringWhite * 0.6f, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.3f);
        }

        private NPC FindClosestNPC(float maxDistance)
        {
            NPC closest = null;
            float closestDist = maxDistance;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.CanBeChasedBy())
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closest = npc;
                    }
                }
            }

            return closest;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - VISIBLE notes burst on hit! (scale 0.75f)
            for (int i = 0; i < 5; i++)
            {
                float angle = MathHelper.TwoPi * i / 5f;
                Vector2 noteVel = angle.ToRotationVector2() * 2.5f;
                ThemedParticles.MusicNote(target.Center, noteVel, SpringPink, 0.75f, 30);
            }
            
            // Life leech - heal player on kill
            if (target.life <= 0)
            {
                Main.player[Projectile.owner].Heal(3);
                
                // Healing VFX
                CustomParticles.GenericFlare(Main.player[Projectile.owner].Center, SpringGreen, 0.5f, 15);
            }

            // Hit VFX - layered sparkles
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(target.Center, burstVel, SpringPink, 0.3f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
                
                var sparkle = new SparkleParticle(target.Center, burstVel * 1.2f, SpringWhite * 0.7f, 0.25f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Pollination check
            if (Main.rand.NextFloat() < 0.15f)
            {
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<HealingFlower>(),
                    0,
                    0,
                    Projectile.owner
                );
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Texture2D petalTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle5").Value;
            
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.12f) * 0.15f + 1f;
            Color drawColor = Color.Lerp(SpringPink, SpringWhite, (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.5f + 0.5f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // PETAL SHIMMER TRAIL
            for (int i = 1; i < 4; i++)
            {
                float trailProgress = i / 4f;
                Vector2 trailPos = drawPos - Projectile.velocity.SafeNormalize(Vector2.Zero) * i * 6f;
                Color trailColor = Color.Lerp(SpringPink, SpringGreen, trailProgress) * (1f - trailProgress) * 0.5f;
                sb.Draw(glowTex, trailPos, null, trailColor with { A = 0 }, 0f, glowOrigin, 0.15f * (1f - trailProgress * 0.5f), SpriteEffects.None, 0f);
            }

            // OUTER GLOW
            sb.Draw(glowTex, drawPos, null, (drawColor * 0.4f) with { A = 0 }, 0f, glowOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // SPINNING PETAL LAYERS - Creates flower-like rotation
            for (int i = 0; i < 3; i++)
            {
                float layerRot = petalSpin + MathHelper.TwoPi * i / 3f;
                Color layerColor = Color.Lerp(SpringPink, SpringWhite, i / 3f);
                sb.Draw(petalTex, drawPos, null, (layerColor * 0.65f) with { A = 0 }, layerRot, petalOrigin, 0.22f * pulse, SpriteEffects.None, 0f);
            }
            
            // WHITE CORE
            sb.Draw(petalTex, drawPos, null, (SpringWhite * 0.5f) with { A = 0 }, -petalSpin * 0.5f, petalOrigin, 0.12f, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(2f, 2f);
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 80, SpringPink, 0.8f);
                dust.noGravity = true;
            }
        }
    }

    /// <summary>
    /// HealingFlower - Stationary flower that heals player when touched
    /// </summary>
    public class HealingFlower : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private float flowerRotation;
        private float[] petalAngles = new float[6];

        public override string Texture => "MagnumOpus/Assets/Particles/GlowingHalo4";

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = false;
            Projectile.hostile = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 300; // 5 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            flowerRotation += 0.02f;
            
            // Update petal angles
            for (int i = 0; i < petalAngles.Length; i++)
            {
                petalAngles[i] += 0.03f + i * 0.005f;
            }
            
            Player owner = Main.player[Projectile.owner];

            // Check if player is nearby
            if (Vector2.Distance(owner.Center, Projectile.Center) < 40f)
            {
                owner.Heal(5);
                
                // Heal VFX
                CustomParticles.GenericFlare(owner.Center, SpringGreen, 0.7f, 20);
                for (int i = 0; i < 8; i++)
                {
                    Vector2 healVel = new Vector2(Main.rand.NextFloat(-1f, 1f), -Main.rand.NextFloat(2f, 4f));
                    var heal = new GenericGlowParticle(owner.Center, healVel, SpringGreen, 0.35f, 25, true);
                    MagnumParticleHandler.SpawnParticle(heal);
                }
                
                Projectile.Kill();
                return;
            }

            // Ambient flower particles
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Vector2 petalVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), -Main.rand.NextFloat(0.5f, 1f));
                Color petalColor = Main.rand.NextBool() ? SpringPink : SpringGreen;
                var petal = new GenericGlowParticle(petalPos, petalVel, petalColor * 0.7f, 0.25f, 30, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }

            // Pulsing glow
            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.08f) * 0.2f + 0.6f;
            Lighting.AddLight(Projectile.Center, SpringGreen.ToVector3() * pulse);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch sb = Main.spriteBatch;
            
            Texture2D haloTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo4").Value;
            Texture2D petalTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle3").Value;
            Texture2D glowTex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            
            Vector2 haloOrigin = haloTex.Size() / 2f;
            Vector2 petalOrigin = petalTex.Size() / 2f;
            Vector2 glowOrigin = glowTex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Projectile.timeLeft * 0.1f) * 0.15f + 1f;
            float alpha = Projectile.timeLeft > 60 ? 1f : Projectile.timeLeft / 60f;

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // ORBITING PETALS - 6 petals form a flower shape
            for (int i = 0; i < petalAngles.Length; i++)
            {
                float orbitRadius = 18f + (float)Math.Sin(flowerRotation * 2f + i * 0.8f) * 4f;
                Vector2 petalPos = drawPos + petalAngles[i].ToRotationVector2() * orbitRadius;
                float petalProgress = (float)i / petalAngles.Length;
                Color petalColor = Color.Lerp(SpringPink, SpringGreen, petalProgress);
                sb.Draw(petalTex, petalPos, null, (petalColor * 0.6f * alpha) with { A = 0 }, petalAngles[i], petalOrigin, 0.15f * pulse, SpriteEffects.None, 0f);
            }

            // GREEN HEALING HALO - Outer ring
            sb.Draw(haloTex, drawPos, null, (SpringGreen * 0.35f * alpha) with { A = 0 }, flowerRotation, haloOrigin, 0.4f * pulse, SpriteEffects.None, 0f);
            
            // PINK FLOWER CENTER - Core halo
            sb.Draw(haloTex, drawPos, null, (SpringPink * 0.5f * alpha) with { A = 0 }, -flowerRotation * 0.5f, haloOrigin, 0.25f * pulse, SpriteEffects.None, 0f);
            
            // WHITE HEALING CORE
            sb.Draw(glowTex, drawPos, null, (Color.White * 0.4f * alpha) with { A = 0 }, 0f, glowOrigin, 0.18f * pulse, SpriteEffects.None, 0f);

            sb.End();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // Flower dissipation
            for (int i = 0; i < 10; i++)
            {
                Vector2 dustVel = Main.rand.NextVector2Circular(3f, 3f);
                Color dustColor = Main.rand.NextBool() ? SpringPink : SpringGreen;
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, dustVel, 80, dustColor, 0.9f);
                dust.noGravity = true;
            }
        }
    }
}
