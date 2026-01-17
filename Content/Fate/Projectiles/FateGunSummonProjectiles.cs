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
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Fate.Debuffs;

namespace MagnumOpus.Content.Fate.Projectiles
{
    #region Fate1Gun Projectiles - Rapid Bullets with Spectral Blade spawns

    /// <summary>
    /// Rapid cosmic bullet - every 5th hit spawns spectral blade
    /// </summary>
    public class FateRapidBullet : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public static int HitCounter = 0; // Shared between all bullets from same player

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
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

            // Simple cosmic trail
            if (Main.rand.NextBool(2))
            {
                Color trailColor = FateCosmicVFX.GetCosmicGradient(Main.rand.NextFloat());
                var spark = new GlowSparkParticle(Projectile.Center, -Projectile.velocity * 0.05f, trailColor, 0.1f, 8);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            HitCounter++;

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

                // VFX
                FateCosmicVFX.SpawnStarSparkles(target.Center, 10, 40f, 0.3f);
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, target.Center);
            }

            // Small impact
            FateCosmicVFX.SpawnGlyphBurst(target.Center, 4, 3f, 0.2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.8f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.6f, Projectile.rotation, origin, 0.12f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Spectral blade that locks onto an enemy and slashes repeatedly for 3 seconds
    /// </summary>
    public class SpectralSlashingBlade : ModProjectile
    {
        public override string Texture => "Terraria/Images/Item_" + ItemID.Terragrim; // Uses Fate5Sword sprite style

        public ref float TargetNPC => ref Projectile.ai[0];

        private int slashCooldown = 0;
        private float orbitAngle = 0f;

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

            // Bright flare and magical effects
            if (Main.rand.NextBool(3))
            {
                Color flareColor = Main.rand.NextBool() ? FateCosmicVFX.FateWhite : FateCosmicVFX.FateDarkPink;
                var flare = new GenericGlowParticle(Projectile.Center, Main.rand.NextVector2Circular(2f, 2f), flareColor * 0.7f, 0.2f, 12, true);
                MagnumParticleHandler.SpawnParticle(flare);
            }

            // Cosmic energy emanation
            FateCosmicVFX.SpawnSpectralAura(Projectile.Center, 0.6f);

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateWhite.ToVector3() * 0.6f);
        }

        private void PerformSlash(NPC target)
        {
            // Deal damage directly
            target.SimpleStrikeNPC(Projectile.damage, Projectile.direction, false, 0f);
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 60);

            // Slash VFX
            Vector2 slashDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            for (int i = 0; i < 6; i++)
            {
                float angle = slashDir.ToRotation() + MathHelper.Lerp(-0.5f, 0.5f, (float)i / 5f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color sparkColor = FateCosmicVFX.GetCosmicGradient((float)i / 5f);
                var spark = new GlowSparkParticle(target.Center, sparkVel, sparkColor, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Bright flash
            var flash = new GenericGlowParticle(target.Center, Vector2.Zero, FateCosmicVFX.FateWhite, 0.4f, 8, true);
            MagnumParticleHandler.SpawnParticle(flash);

            SoundEngine.PlaySound(SoundID.Item1 with { Pitch = 0.5f, Volume = 0.5f }, target.Center);
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

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 origin = tex.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            // Glow effect
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.5f, Projectile.rotation, origin, 1.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * 0.4f, Projectile.rotation, origin, 1.6f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // White tinted sword
            spriteBatch.Draw(tex, drawPos, null, Color.White, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion

    #region Fate2Gun Projectiles - Accelerating Piercing Rounds

    /// <summary>
    /// Special round that starts slow and accelerates while piercing
    /// </summary>
    public class AcceleratingCosmicRound : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        private float currentSpeed = 4f;
        private const float MaxSpeed = 25f;
        private const float Acceleration = 0.8f;

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

            // Explosions in trail as it speeds up
            if (currentSpeed > 10f && Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnCosmicCloudBurst(Projectile.Center, 0.3f, 6);
                SoundEngine.PlaySound(SoundID.Item10 with { Volume = 0.3f, Pitch = 0.5f }, Projectile.Center);
            }

            // Heavy cosmic trail
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.6f + currentSpeed / MaxSpeed * 0.4f);

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateBrightRed.ToVector3() * (currentSpeed / MaxSpeed));
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

            // Big explosion on contact
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.8f);

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
            }

            SoundEngine.PlaySound(SoundID.Item14, target.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = FateCosmicVFX.GetCosmicGradient(progress) * (1f - progress) * 0.6f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (1f - progress) * 0.6f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float speedRatio = currentSpeed / MaxSpeed;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateBrightRed * (0.6f + speedRatio * 0.4f), Projectile.rotation, origin, 0.5f + speedRatio * 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * (0.4f + speedRatio * 0.4f), Projectile.rotation, origin, 0.25f + speedRatio * 0.15f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Cosmic rocket that homes and explodes
    /// </summary>
    public class CosmicRocket : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

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
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            // Light homing
            NPC target = FindNearestEnemy(400f);
            if (target != null)
            {
                Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 16f, 0.08f);
            }

            // Rocket trail
            FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.4f);
            
            if (Main.rand.NextBool(4))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 1, 10f, 0.2f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.5f);
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
            FateCosmicVFX.SpawnCosmicExplosion(Projectile.Center, 0.5f);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.6f }, Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FatePurple * 0.7f, Projectile.rotation, origin, 0.5f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.5f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

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
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

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
            // Slash VFX
            Vector2 slashDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.UnitX);
            
            for (int i = 0; i < 8; i++)
            {
                float angle = slashDir.ToRotation() + MathHelper.Lerp(-0.8f, 0.8f, (float)i / 7f);
                Vector2 sparkVel = angle.ToRotationVector2() * Main.rand.NextFloat(6f, 12f);
                Color sparkColor = FateCosmicVFX.GetCosmicGradient((float)i / 7f);
                var spark = new GlowSparkParticle(Projectile.Center + slashDir * 30f, sparkVel, sparkColor, 0.25f, 15);
                MagnumParticleHandler.SpawnParticle(spark);
            }

            // Flash
            var flash = new GenericGlowParticle(Projectile.Center + slashDir * 30f, Vector2.Zero, FateCosmicVFX.FateWhite, 0.5f, 8, true);
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

            // Beam VFX
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 6, 5f, 0.35f);
            FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 8, 30f, 0.3f);

            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, Projectile.Center);
        }

        private void SpawnDeityAura()
        {
            // Orbiting glyphs
            if (Main.GameUpdateCount % 6 == 0)
            {
                float orbitAngle = Main.GameUpdateCount * 0.05f;
                FateCosmicVFX.SpawnOrbitingGlyphs(Projectile.Center, 3, 35f, orbitAngle, 0.3f);
            }

            // Star sparkles
            if (Main.rand.NextBool(8))
            {
                FateCosmicVFX.SpawnStarSparkles(Projectile.Center, 2, 25f, 0.2f);
            }

            // Cosmic cloud wisps
            if (Main.rand.NextBool(6))
            {
                FateCosmicVFX.SpawnCosmicCloudTrail(Projectile.Center, Projectile.velocity, 0.4f);
            }

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateDarkPink.ToVector3() * 0.8f);
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
        public override string Texture => "Terraria/Images/Buff_" + BuffID.TwinEyesMinion;

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
    /// Beam fired by cosmic deity
    /// </summary>
    public class DeityCosmicBeam : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
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

            // Pure cosmic light trail
            FateCosmicVFX.SpawnCosmicBeamParticles(Projectile.Center, Projectile.Center - Projectile.velocity, 0.8f);

            Lighting.AddLight(Projectile.Center, FateCosmicVFX.FateWhite.ToVector3() * 1.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<DestinyCollapse>(), 180);
            FateCosmicVFX.SpawnCosmicExplosion(target.Center, 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            FateCosmicVFX.SpawnGlyphBurst(Projectile.Center, 4, 3f, 0.25f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D tex = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 origin = tex.Size() / 2f;

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(FateCosmicVFX.FateWhite, FateCosmicVFX.FateDarkPink, progress) * (1f - progress) * 0.7f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(tex, trailPos, null, trailColor, Projectile.oldRot[i], origin, (1f - progress * 0.5f) * 0.8f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            Vector2 drawPos = Projectile.Center - Main.screenPosition;
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateWhite * 0.9f, Projectile.rotation, origin, 0.7f, SpriteEffects.None, 0f);
            spriteBatch.Draw(tex, drawPos, null, FateCosmicVFX.FateDarkPink * 0.5f, Projectile.rotation, origin, 1f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp,
                DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    #endregion
}
