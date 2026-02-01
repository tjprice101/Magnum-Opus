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
using MagnumOpus.Content.Winter.Weapons;

namespace MagnumOpus.Content.Winter.Projectiles
{
    /// <summary>
    /// Frost Sentinel Minion - Orbiting ice elemental summoned by Frozen Heart
    /// Features Cryo Synchrony aura, Shatter Strike crits, and Permafrost Bond damage bonus
    /// </summary>
    public class FrostSentinelMinion : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField10";
        
        private static readonly Color IceBlue = new Color(150, 220, 255);
        private static readonly Color FrostWhite = new Color(240, 250, 255);
        private static readonly Color DeepBlue = new Color(60, 100, 180);
        private static readonly Color CrystalCyan = new Color(100, 255, 255);
        private static readonly Color GlacialPurple = new Color(120, 130, 200);

        private float orbitAngle = 0f;
        private float attackCooldown = 0f;
        private const float CryoAuraRadius = 200f;
        private const float AttackRange = 450f;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18000;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minionSlots = 1f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Check buff
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<FrostSentinelBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<FrostSentinelBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // Count sentinels and frozen enemies
            int sentinelCount = owner.ownedProjectileCounts[Projectile.type];
            int frozenCount = CountFrozenEnemies();

            // Permafrost Bond - bonus damage per frozen enemy
            float damageBonus = 1f + (frozenCount * 0.15f);

            // Find and track target
            NPC target = FindTarget(owner);

            if (target != null)
            {
                // Attack behavior
                Vector2 toTarget = target.Center - Projectile.Center;
                float targetDist = toTarget.Length();

                if (targetDist > 60f)
                {
                    // Move toward target
                    Vector2 targetVel = toTarget.SafeNormalize(Vector2.Zero) * 14f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetVel, 0.08f);
                }
                else
                {
                    // Circle around target
                    orbitAngle += 0.08f;
                    Vector2 orbitPos = target.Center + orbitAngle.ToRotationVector2() * 50f;
                    Vector2 orbitVel = (orbitPos - Projectile.Center).SafeNormalize(Vector2.Zero) * 8f;
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, orbitVel, 0.1f);
                }

                // Fire frost projectiles
                attackCooldown--;
                if (attackCooldown <= 0f && targetDist < AttackRange)
                {
                    attackCooldown = 35f;

                    if (Main.myPlayer == Projectile.owner)
                    {
                        Vector2 shootVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f;
                        int damage = (int)(Projectile.damage * damageBonus);
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, shootVel,
                            ModContent.ProjectileType<SentinelFrostBolt>(), damage, Projectile.knockBack * 0.5f, Projectile.owner);
                    }

                    // Attack VFX
                    CustomParticles.GenericFlare(Projectile.Center, CrystalCyan, 0.45f, 14);
                }
            }
            else
            {
                // Idle - orbit around player
                orbitAngle += 0.03f;
                float orbitRadius = 80f + GetSentinelIndex() * 25f;
                Vector2 idealPos = owner.Center + orbitAngle.ToRotationVector2() * orbitRadius;
                Vector2 toIdeal = idealPos - Projectile.Center;
                Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.12f, 0.1f);

                attackCooldown = Math.Max(0, attackCooldown - 1);
            }

            // Cryo Synchrony - 3+ sentinels create freezing aura
            if (sentinelCount >= 3)
            {
                ApplyCryoAura(owner);
            }

            // Ambient particles
            if (Main.rand.NextBool(5))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(18f, 18f);
                Vector2 particleVel = Main.rand.NextVector2Circular(2f, 2f);
                Color particleColor = Color.Lerp(IceBlue, CrystalCyan, Main.rand.NextFloat()) * 0.5f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Sentinel frost hymn - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, CrystalCyan * 0.6f, 0.72f, 38);
                
                // Frost sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel * 0.5f, IceBlue * 0.5f, 0.28f, 22);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Frost trail
            if (Projectile.velocity.Length() > 2f && Main.rand.NextBool(3))
            {
                Vector2 trailPos = Projectile.Center - Projectile.velocity * 0.3f;
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                var trail = new GenericGlowParticle(trailPos, trailVel, IceBlue * 0.4f, 0.18f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            Projectile.rotation = Projectile.velocity.X * 0.02f;
            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.5f);
        }

        private NPC FindTarget(Player owner)
        {
            NPC target = null;
            float closestDist = AttackRange;

            // Check for player-targeted enemy
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC targetNPC = Main.npc[owner.MinionAttackTargetNPC];
                if (targetNPC.active && !targetNPC.friendly && targetNPC.lifeMax > 5)
                {
                    return targetNPC;
                }
            }

            // Find closest enemy
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

            return target;
        }

        private int GetSentinelIndex()
        {
            int index = 0;
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.type == Projectile.type && proj.owner == Projectile.owner)
                {
                    if (proj.whoAmI == Projectile.whoAmI)
                        return index;
                    index++;
                }
            }
            return 0;
        }

        private int CountFrozenEnemies()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.HasBuff(BuffID.Frozen))
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < 500f)
                        count++;
                }
            }
            return count;
        }

        private void ApplyCryoAura(Player owner)
        {
            // Cryo aura particles (only first sentinel handles this)
            if (GetSentinelIndex() == 0 && Main.rand.NextBool(6))
            {
                float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                float radius = Main.rand.NextFloat(CryoAuraRadius * 0.7f, CryoAuraRadius);
                Vector2 auraPos = owner.Center + angle.ToRotationVector2() * radius;
                Vector2 auraVel = (owner.Center - auraPos).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(1f, 2f);
                var aura = new GenericGlowParticle(auraPos, auraVel, DeepBlue * 0.35f, 0.22f, 25, true);
                MagnumParticleHandler.SpawnParticle(aura);
            }

            // Apply slow/freeze to enemies in range
            if (Main.GameUpdateCount % 30 == 0)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(owner.Center, npc.Center);
                        if (dist < CryoAuraRadius)
                        {
                            npc.AddBuff(BuffID.Slow, 60);
                            npc.AddBuff(BuffID.Frostburn2, 60);

                            // Occasional freeze
                            if (Main.rand.NextFloat() < 0.08f)
                            {
                                npc.AddBuff(BuffID.Frozen, 45);
                                CustomParticles.GenericFlare(npc.Center, CrystalCyan, 0.5f, 16);
                            }
                        }
                    }
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 180);
            target.AddBuff(BuffID.Slow, 120);

            // Shatter Strike on crit
            if (hit.Crit)
            {
                // Shatter damage to nearby enemies
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && npc.whoAmI != target.whoAmI && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(target.Center, npc.Center);
                        if (dist < 120f)
                        {
                            int shatterDamage = (int)(damageDone * 0.4f);
                            npc.SimpleStrikeNPC(shatterDamage, hit.HitDirection, false, hit.Knockback * 0.5f, DamageClass.Summon);
                            npc.AddBuff(BuffID.Frostburn2, 120);

                            // Shatter VFX
                            for (int j = 0; j < 4; j++)
                            {
                                Vector2 shardVel = (npc.Center - target.Center).SafeNormalize(Vector2.Zero) * Main.rand.NextFloat(3f, 6f);
                                shardVel += Main.rand.NextVector2Circular(2f, 2f);
                                var shard = new GenericGlowParticle(target.Center, shardVel, CrystalCyan * 0.6f, 0.22f, 16, true);
                                MagnumParticleHandler.SpawnParticle(shard);
                            }
                        }
                    }
                }

                // Main shatter VFX
                CustomParticles.GenericFlare(target.Center, FrostWhite, 0.7f, 20);
                // Frost sparkle burst 
                var frostSparkle = new SparkleParticle(target.Center, Vector2.Zero, IceBlue * 0.6f, 0.5f * 0.6f, 18);
                MagnumParticleHandler.SpawnParticle(frostSparkle);

                // ☁EMUSICAL CRIT - Shatter Strike crystal chord
                ThemedParticles.MusicNoteBurst(target.Center, CrystalCyan * 0.8f, 8, 5f);
                ThemedParticles.MusicNoteRing(target.Center, IceBlue * 0.65f, 40f, 6);

                for (int i = 0; i < 10; i++)
                {
                    float angle = MathHelper.TwoPi * i / 10f;
                    Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                    Color burstColor = Color.Lerp(IceBlue, FrostWhite, (float)i / 10f) * 0.55f;
                    var burst = new GenericGlowParticle(target.Center, burstVel, burstColor, 0.25f, 18, true);
                    MagnumParticleHandler.SpawnParticle(burst);
                }
            }

            // Standard hit VFX
            CustomParticles.GenericFlare(target.Center, IceBlue, 0.45f, 14);

            // ☁EMUSICAL IMPACT - Sentinel strike note
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.65f, 4, 3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField10").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f + Projectile.whoAmI) * 0.12f + 1f;

            // Sentinel glow layers
            spriteBatch.Draw(texture, drawPos, null, DeepBlue * 0.3f, Projectile.rotation, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.5f, Projectile.rotation, origin, 0.48f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, CrystalCyan * 0.65f, Projectile.rotation, origin, 0.35f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.8f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            // Orbiting frost points
            float orbitTime = Main.GameUpdateCount * 0.06f + Projectile.whoAmI * 0.5f;
            for (int i = 0; i < 3; i++)
            {
                float pointAngle = orbitTime + MathHelper.TwoPi * i / 3f;
                Vector2 pointPos = drawPos + pointAngle.ToRotationVector2() * 18f;
                spriteBatch.Draw(texture, pointPos, null, CrystalCyan * 0.6f, 0f, origin, 0.12f * pulse, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, FrostWhite, 0.7f, 22);
            // Frost sparkle burst 
            var frostSparkle = new SparkleParticle(Projectile.Center, Vector2.Zero, IceBlue * 0.6f, 0.45f * 0.6f, 18);
            MagnumParticleHandler.SpawnParticle(frostSparkle);

            // ☁EMUSICAL FINALE - Sentinel farewell melody
            ThemedParticles.MusicNoteBurst(Projectile.Center, CrystalCyan * 0.7f, 8, 4.5f);
            ThemedParticles.MusicNoteRing(Projectile.Center, IceBlue * 0.6f, 45f, 6);

            for (int i = 0; i < 10; i++)
            {
                float angle = MathHelper.TwoPi * i / 10f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 7f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }

    /// <summary>
    /// Sentinel Frost Bolt - Ranged attack from Frost Sentinels
    /// </summary>
    public class SentinelFrostBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle11";
        
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
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
            Projectile.extraUpdates = 1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            // Light homing
            float homingRange = 250f;
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

            // Trail
            if (Main.rand.NextBool(2))
            {
                Vector2 trailVel = -Projectile.velocity * 0.1f;
                var trail = new GenericGlowParticle(Projectile.Center, trailVel, IceBlue * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Frost bolt whisper - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.8f, 0.8f), Main.rand.NextFloat(-1f, -0.3f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, IceBlue * 0.55f, 0.68f, 32);
                
                // Tiny frost sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.4f, CrystalCyan * 0.4f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, IceBlue.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 120);
            target.AddBuff(BuffID.Slow, 90);

            if (Main.rand.NextFloat() < 0.12f)
            {
                target.AddBuff(BuffID.Frozen, 45);
            }

            CustomParticles.GenericFlare(target.Center, CrystalCyan, 0.4f, 14);

            // ☁EMUSICAL IMPACT - Frost bolt impact note
            ThemedParticles.MusicNoteBurst(target.Center, IceBlue * 0.6f, 4, 3f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle11").Value;
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
                spriteBatch.Draw(texture, trailPos, null, IceBlue * alpha, Projectile.oldRot[i], origin, 0.2f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }

            // Main
            spriteBatch.Draw(texture, drawPos, null, IceBlue * 0.6f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, FrostWhite * 0.8f, Projectile.rotation, origin, 0.16f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Frost bolt fade
            ThemedParticles.MusicNoteBurst(Projectile.Center, IceBlue * 0.5f, 4, 2.5f);

            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, IceBlue * 0.5f, 0.16f, 12, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }
    }
}
