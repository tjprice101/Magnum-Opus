using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    #region Fate1Gun Projectiles - Rapid Bullets with Spectral Blade spawns

    /// <summary>
    /// Rapid cosmic bullet - every 5th hit spawns spectral blade
    /// Enhanced with spectacular celestial visual effects
    /// </summary>
    public class FateRapidBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public static int HitCounter = 0; // Shared between all bullets from same player
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.15f;

            // === ENHANCED COSMIC TRAIL ===
            // Heavy cosmic cloud trail
            if (Main.rand.NextBool(2))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.4f);
            }
            
            // Gradient spark trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.08f + Main.rand.NextVector2Circular(1f, 1f), 
                    trailColor, 0.15f, 10);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Star sparkles
            if (Main.rand.NextBool(5))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 10f, 0.15f);
            }
            
            // Occasional glyph in trail
            if (Main.rand.NextBool(12))
            {
                CustomParticles.Glyph(Projectile.Center, FateCosmicVFX.FatePurple, 0.15f, -1);
            }
            
            // Music notes for musical theme
            if (Main.rand.NextBool(15))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 8f, 0.15f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitCounter++;

            // === ENHANCED IMPACT VFX ===
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.4f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 4f, 0.2f);
            
            // Every 5th hit spawns spectral blade
            if (HitCounter >= 5)
            {
                HitCounter = 0;
                
                // Spawn spectral blade that attacks this enemy
                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    target.Center + Main.rand.NextVector2Circular(50f, 50f),
                    Vector2.Zero,
                    ModContent.ProjectileType<SpectralSlashingBlade>(),
                    (int)(Projectile.damage * 2f),
                    0f,
                    Projectile.owner,
                    target.whoAmI
                );

                // Major VFX for blade spawn
                FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.8f);
                FateCosmicVFX.SpawnStarSparkles(target.Center, 12, 45f, 0.35f);
                FateCosmicVFX.SpawnGlyphBurst(target.Center, 8, 6f, 0.4f);
                FateCosmicVFX.SpawnCosmicMusicNotes(target.Center, 4, 30f, 0.3f);
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, target.Center);
            }
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 3, 3f, 0.15f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 4, 15f, 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.15f;

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (0.2f - progress * 0.1f) * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Multi-layer bloom
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.3f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.7f, Projectile.rotation, origin, 0.28f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.8f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Spectral blade that locks onto an enemy and slashes repeatedly for 3 seconds
    /// Uses Coda of Annihilation texture with celestial bloom effects
    /// </summary>
    public class SpectralSlashingBlade : ModProjectile
    {
        public override string Texture => "MagnumOpus/Content/Fate/ResonantWeapons/CodaOfAnnihilation";

        public ref float TargetNPC => ref Projectile.ai[0];

        private int slashCooldown = 0;
        private float orbitAngle = 0f;
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180; // 3 seconds
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            NPC target = Main.npc[(int)TargetNPC];
            pulsePhase += 0.1f;

            if (!target.active || target.dontTakeDamage)
            {
                // Find new target or die
                NPC newTarget = FindNearestEnemy(400f);
                if (newTarget != null)
                {
                    TargetNPC = newTarget.whoAmI;
                }
                else
                {
                    Projectile.Kill();
                    return;
                }
            }

            // Orbit around target and slash
            orbitAngle += 0.2f;
            float orbitRadius = 60f;
            Vector2 targetPos = target.Center + orbitAngle.ToRotationVector2() * orbitRadius;
            
            Projectile.velocity = (targetPos - Projectile.Center) * 0.3f;
            
            // Point blade tip toward enemy
            Vector2 toTarget = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            Projectile.rotation = toTarget.ToRotation() + MathHelper.PiOver4;

            // Slash attack
            slashCooldown--;
            if (slashCooldown <= 0)
            {
                slashCooldown = 15;
                PerformSlash(target);
            }

            // === ENHANCED CELESTIAL TRAIL ===
            // Cosmic cloud trail
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.35f);
            }
            
            // Star sparkle aura
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 15f, 0.2f);
            }
            
            // Glyph particles
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), 
                    FateCosmicVFX.FatePurple, 0.2f, -1);
            }
            
            // Music notes for musical theme
            if (Main.rand.NextBool(10))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 12f, 0.18f);
            }
            
            // Cosmic energy emanation
            FateCosmicVFX.SpawnSpectralAura(Projectile.Center, 0.5f);

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateWhite.ToVector3() * 0.7f);
        }

        private void PerformSlash(NPC target)
        {
            // Deal damage directly
            target.SimpleStrikeNPC(Projectile.damage, Projectile.direction, false, 0f);
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 60);

            // === ENHANCED SLASH VFX ===
            Vector2 slashDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Cosmic spark burst
            for (int i = 0; i < 10; i++)
            {
                float angle = slashDir.ToRotation() + MathHelper.Lerp(-0.6f, 0.6f, (float)i / 9f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(5f, 10f);
                Color sparkColor = FateCosmicVFX.GetCosmicGradient((float)i / 9f);
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Glyph burst on slash
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 4f, 0.25f);
            
            // Star sparkles
            FateCosmicVFX.SpawnStarSparkles(target.Center, 6, 25f, 0.25f);
            
            // Cosmic flash
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.35f);

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.6f }, target.Center);
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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

        public override bool? CanDamage() => false; // Damage handled manually

        public override void OnKill(int timeLeft)
        {
            // Death explosion
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.6f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 10, 35f, 0.3f);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 6, 5f, 0.3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.1f;

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.4f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (1f - progress * 0.3f) * pulse, SpriteEffects.None, 0f);
            }

            // Multi-layer celestial bloom
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer purple nebula glow
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.25f, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            // Bright red energy layer
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.35f, Projectile.rotation, origin, 1.5f * pulse, SpriteEffects.None, 0f);
            // Dark pink field
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.5f, Projectile.rotation, origin, 1.25f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Core sword - white tinted
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, 1f * pulse, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion

    #region Fate2Gun Projectiles - Accelerating Piercing Rounds

    /// <summary>
    /// Special round that starts slow and accelerates while piercing
    /// Enhanced with spectacular celestial cosmic effects
    /// </summary>
    public class AcceleratingCosmicRound : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        private float currentSpeed = 4f;
        private const float MaxSpeed = 25f;
        private const float Acceleration = 0.8f;
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.12f + (currentSpeed / MaxSpeed) * 0.08f;

            // Accelerate toward nearest enemy
            NPC target = FindNearestEnemy(500f);
            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                
                // Accelerate!
                currentSpeed = Math.Min(currentSpeed + Acceleration, MaxSpeed);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity.SafeNormalize(Vector2.UnitX), targetDir, 0.15f) * currentSpeed;
            }
            else
            {
                currentSpeed = Math.Min(currentSpeed + Acceleration * 0.5f, MaxSpeed);
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * currentSpeed;
            }

            float speedRatio = currentSpeed / MaxSpeed;

            // === ENHANCED COSMIC TRAIL - scales with speed ===
            // Cosmic cloud trail - more intense at higher speeds
            if (Main.rand.NextBool(2))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.5f + speedRatio * 0.5f);
            }
            
            // Star sparkles - more frequent at higher speeds
            if (Main.rand.NextFloat() < 0.2f + speedRatio * 0.3f)
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 15f + speedRatio * 10f, 0.2f + speedRatio * 0.15f);
            }
            
            // Glyphs at higher speeds
            if (speedRatio > 0.3f && Main.rand.NextBool(6))
            {
                CustomParticles.Glyph(Projectile.Center, FateCosmicVFX.FatePurple, 0.2f + speedRatio * 0.15f, -1);
            }
            
            // Cosmic electricity at high speeds
            if (speedRatio > 0.5f && Main.rand.NextBool(5))
            {
                FateCosmicVFX.SpawnCosmicElectricity(Projectile.Center, 1, 25f, 0.2f);
            }

            // Explosions in trail as it speeds up
            if (currentSpeed > 10f && Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 0.35f, 8);
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.35f, Pitch = 0.5f + speedRatio * 0.3f }, Projectile.Center);
            }

            // Music notes for musical theme
            if (Main.rand.NextBool(10))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 12f, 0.2f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateBrightRed.ToVector3() * (0.5f + speedRatio * 0.6f));
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);

            // === ENHANCED IMPACT VFX ===
            float speedRatio = currentSpeed / MaxSpeed;
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.6f + speedRatio * 0.5f);
            FateCosmicVFX.SpawnStarSparkles(target.Center, 8 + (int)(speedRatio * 6), 30f + speedRatio * 20f, 0.3f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 5, 5f, 0.3f);

            // Spawn 3 cosmic rockets from around the player
            Player owner = Main.player[Projectile.owner];
            for (int i = 0; i < 3; i++)
            {
                float angle = MathHelper.TwoPi * i / 3f + Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 rocketSpawn = owner.Center + angle.ToRotationVector2() * 60f;
                Vector2 rocketDir = (target.Center - rocketSpawn).SafeNormalize(Vector2.UnitX);

                Projectile.NewProjectile(
                    Projectile.GetSource_FromThis(),
                    rocketSpawn,
                    rocketDir * 12f,
                    ModContent.ProjectileType<CosmicRocket>(),
                    (int)(Projectile.damage * 0.5f),
                    Projectile.knockBack,
                    Projectile.owner
                );
                
                // Rocket spawn VFX
                FateCosmicVFX.SpawnGlyphBurst(rocketSpawn, 3, 3f, 0.2f);
            }

            SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }
        
        public override void OnKill(int timeLeft)
        {
            // Death burst
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.5f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 6, 25f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            
            float speedRatio = currentSpeed / MaxSpeed;
            float pulse = 1f + (float)Math.Sin(pulsePhase) * (0.1f + speedRatio * 0.1f);

            // Enhanced trail with gradient
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * (0.5f + speedRatio * 0.3f);
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - progress) * (0.5f + speedRatio * 0.3f) * pulse;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Multi-layer bloom that scales with speed
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * (0.2f + speedRatio * 0.2f), Projectile.rotation, origin, (0.7f + speedRatio * 0.4f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * (0.5f + speedRatio * 0.4f), Projectile.rotation, origin, (0.5f + speedRatio * 0.3f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * (0.6f + speedRatio * 0.3f), Projectile.rotation, origin, (0.35f + speedRatio * 0.2f) * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * (0.5f + speedRatio * 0.4f), Projectile.rotation, origin, (0.2f + speedRatio * 0.15f) * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Cosmic rocket that homes and explodes
    /// Enhanced with celestial trail effects
    /// </summary>
    public class CosmicRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.15f;

            // Light homing
            NPC target = FindNearestEnemy(400f);
            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 16f, 0.08f);
            }

            // === ENHANCED ROCKET TRAIL ===
            // Cosmic cloud trail
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.45f);
            
            // Star sparkles
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 12f, 0.2f);
            }
            
            // Glyph accents
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center, FateCosmicVFX.FatePurple, 0.15f, -1);
            }
            
            // Gradient glow particles
            if (Main.rand.NextBool(3))
            {
                Color glowColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                var glow = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, glowColor * 0.6f, 0.15f, 10, true);
                MagnumParticleHandler.SpawnParticle(glow);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.6f);
        }

        private NPC FindNearestEnemy(float range)
        {
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            // Enhanced explosion
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.6f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 8, 25f, 0.25f);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 4f, 0.2f);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.12f;

            // Draw trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (0.4f - progress * 0.2f) * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Multi-layer bloom
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.4f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.5f, Projectile.rotation, origin, 0.4f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.6f, Projectile.rotation, origin, 0.28f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.6f, Projectile.rotation, origin, 0.15f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion

    #region Fate1Summon Projectiles - Cosmic Deity Minion

    /// <summary>
    /// Cosmic deity minion that slashes rapidly and fires beams
    /// </summary>
    public class CosmicDeityMinion : ModProjectile
    {
        private int attackCooldown = 0;
        private int beamCooldown = 0;
        private const int SlashCooldown = 12;
        private const int BeamCooldownMax = 120;

        public override void SetStaticDefaults()
        {
            Main.projPet[Projectile.type] = true;
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.minion = true;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 2f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (!CheckActive(owner))
                return;

            attackCooldown = Math.Max(0, attackCooldown - 1);
            beamCooldown = Math.Max(0, beamCooldown - 1);

            NPC target = FindTarget();

            if (target != null)
            {
                // Move toward target
                Vector2 desiredPos = target.Center - new Vector2(target.direction * 80f, 0);
                Vector2 toDesired = desiredPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toDesired * 0.15f, 0.1f);

                // Face target
                Projectile.rotation = (target.Center - Projectile.Center).ToRotation();

                // Rapid slash attacks
                if (attackCooldown <= 0 && Vector2.Distance(Projectile.Center, target.Center) < 150f)
                {
                    attackCooldown = SlashCooldown;
                    PerformSlash(target);
                }

                // Occasional beam attack
                if (beamCooldown <= 0)
                {
                    beamCooldown = BeamCooldownMax;
                    FireCosmicBeam(target);
                }
            }
            else
            {
                // Idle - float near player
                Vector2 idlePos = owner.Center + new Vector2(owner.direction * -60f, -40f);
                Projectile.velocity = (idlePos - Projectile.Center) * 0.05f;
                Projectile.rotation += 0.02f;
            }

            // Deity aura VFX
            SpawnDeityAura();
        }

        private bool CheckActive(Player owner)
        {
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<CosmicDeityBuff>());
                Projectile.Kill();
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<CosmicDeityBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            return true;
        }

        private NPC FindTarget()
        {
            // Check for manual target
            if (Main.player[Projectile.owner].HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[Main.player[Projectile.owner].MinionAttackTargetNPC];
                if (target.active && target.CanBeChasedBy(Projectile))
                    return target;
            }

            // Find closest
            float range = 800f;
            NPC closest = null;
            float closestDist = range;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && npc.CanBeChasedBy(Projectile))
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

        private void PerformSlash(NPC target)
        {
            // === ENHANCED SLASH VFX ===
            Vector2 slashDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            // Cosmic spark arc
            for (int i = 0; i < 12; i++)
            {
                float angle = slashDir.ToRotation() + MathHelper.Lerp(-0.9f, 0.9f, (float)i / 11f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(7f, 14f);
                Color sparkColor = FateCosmicVFX.GetCosmicGradient((float)i / 11f);
                var spark = new GlowSparkParticle(Projectile.Center + slashDir * 30f, sparkVel, sparkColor, 0.3f, 18);
                MagnumParticleHandler.SpawnParticle(spark);
            }
            
            // Glyph accents on slash
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center + slashDir * 25f, 3, 4f, 0.25f);
            
            // Star sparkles
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center + slashDir * 30f, 4, 20f, 0.2f);

            // Flash
            var flash = new GenericGlowParticle(Projectile.Center + slashDir * 30f, Vector2.Zero, FateCosmicVFX.FateWhite, 0.6f, 10, true);
            MagnumParticleHandler.SpawnParticle(flash);

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.3f, Volume = 0.6f }, Projectile.Center);
        }

        private void FireCosmicBeam(NPC target)
        {
            Vector2 direction = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);

            Projectile.NewProjectile(
                Projectile.GetSource_FromThis(),
                Projectile.Center,
                direction * 18f,
                ModContent.ProjectileType<DeityCosmicBeam>(),
                (int)(Projectile.damage * 1.5f),
                Projectile.knockBack,
                Projectile.owner
            );

            // === ENHANCED BEAM FIRE VFX ===
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 8, 6f, 0.4f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 12, 40f, 0.35f);
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.5f);
            FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 4, 25f, 0.25f);

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, Projectile.Center);
        }

        private void SpawnDeityAura()
        {
            // === ENHANCED DEITY AURA ===
            // Orbiting glyphs - more frequent and dramatic
            if (Main.GameUpdateCount % 4 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.06f;
                FateCosmicVFX.SpawnOrbitingGlyphs(Projectile.Center, 4, 40f, orbitAngle, 0.35f);
            }

            // Star sparkles - constant celestial shimmer
            if (Main.rand.NextBool(5))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 2, 30f, 0.22f);
            }

            // Cosmic cloud wisps
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.45f);
            }
            
            // Occasional cosmic electricity
            if (Main.rand.NextBool(12))
            {
                FateCosmicVFX.SpawnCosmicElectricity(Projectile.Center, 1, 35f, 0.2f);
            }
            
            // Music notes for musical theme
            if (Main.rand.NextBool(15))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 15f, 0.18f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.9f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 120);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.15f;
            
            // Cosmic deity form - layered glows
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.CosmicBlack * 0.5f, Projectile.rotation, origin, 2.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.6f, Projectile.rotation, origin, 1.8f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.7f, Projectile.rotation, origin, 1.2f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.8f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Buff for cosmic deity minion
    /// </summary>
    public class CosmicDeityBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }

        public override void Update(Player player, ref int buffIndex)
        {
            if (player.ownedProjectileCounts[ModContent.ProjectileType<CosmicDeityMinion>()] > 0)
            {
                player.buffTime[buffIndex] = 18000;
            }
            else
            {
                player.DelBuff(buffIndex);
                buffIndex--;
            }
        }
    }

    /// <summary>
    /// Beam fired by cosmic deity - enhanced with spectacular celestial effects
    /// </summary>
    public class DeityCosmicBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";
        
        private float pulsePhase = 0f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 15;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            pulsePhase += 0.18f;

            // === ENHANCED COSMIC BEAM TRAIL ===
            // Pure cosmic light trail
            FateCosmicVFX.SpawnCosmicBeamParticles(Projectile.Center, Projectile.Center - Projectile.velocity, 0.9f);
            
            // Star sparkles in trail
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 15f, 0.25f);
            }
            
            // Cosmic cloud wisps
            if (Main.rand.NextBool(3))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.35f);
            }
            
            // Glyph accents
            if (Main.rand.NextBool(8))
            {
                CustomParticles.Glyph(Projectile.Center, FateCosmicVFX.FatePurple, 0.2f, -1);
            }
            
            // Music notes
            if (Main.rand.NextBool(12))
            {
                FateCosmicVFX.SpawnCosmicMusicNotes(Projectile.Center, 1, 10f, 0.18f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateWhite.ToVector3() * 1.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            
            // Enhanced impact
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.5f);
            FateCosmicVFX.SpawnStarSparkles(target.Center, 6, 25f, 0.25f);
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 4f, 0.25f);
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.4f);
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 5, 4f, 0.25f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 8, 20f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(pulsePhase) * 0.15f;

            // Enhanced gradient trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                float trailScale = (1f - progress * 0.4f) * 0.7f * pulse;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, trailScale, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            
            // Multi-layer bloom
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.4f, Projectile.rotation, origin, 1.1f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.5f, Projectile.rotation, origin, 0.85f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.6f, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.85f, Projectile.rotation, origin, 0.35f * pulse, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion
}
