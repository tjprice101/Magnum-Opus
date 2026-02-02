using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Content.Spring.Weapons;
using static MagnumOpus.Common.Systems.DynamicParticleEffects;

namespace MagnumOpus.Content.Spring.Projectiles
{
    /// <summary>
    /// Flower Sprite Minion - Spring summon that orbits and attacks
    /// - Spring Harmony: Multiple sprites attack faster
    /// - Renewal Bond: Heals player every 5 seconds
    /// - Pollen Cloud: Attacks spawn lingering damage zones
    /// - Bloom Formation: 3+ sprites do sync attacks
    /// </summary>
    public class FlowerSpriteMinion : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringWhite = new Color(255, 250, 250);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringYellow = new Color(255, 255, 180);

        private float orbitAngle;
        private float wingFlap;
        private int healTimer = 0;
        private int attackCooldown = 0;
        private int syncAttackTimer = 0;
        private int spriteIndex = 0;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.minion = true;
            Projectile.minionSlots = 1f;
            Projectile.netImportant = true;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow4";

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];
            if (!CheckActive(owner)) return;

            // Count all flower sprites for synergy
            int spriteCount = CountFlowerSprites(owner);
            UpdateSpriteIndex(owner);

            // Update timers
            healTimer++;
            attackCooldown = Math.Max(0, attackCooldown - 1);
            syncAttackTimer++;

            // Spring Harmony: Attack speed scales with sprite count
            float attackSpeedMult = 1f + (spriteCount - 1) * 0.15f; // +15% speed per extra sprite

            // Renewal Bond: Heal every 5 seconds
            if (healTimer >= 300 && Projectile.owner == Main.myPlayer)
            {
                healTimer = 0;
                owner.Heal(3);
                
                // Heal VFX
                CustomParticles.GenericFlare(owner.Center, SpringGreen, 0.5f, 18);
                for (int i = 0; i < 5; i++)
                {
                    Vector2 healPos = owner.Center + Main.rand.NextVector2Circular(20f, 20f);
                    Vector2 healVel = new Vector2(0, -Main.rand.NextFloat(1f, 2.5f));
                    var heal = new GenericGlowParticle(healPos, healVel, SpringGreen * 0.8f, 0.3f, 25, true);
                    MagnumParticleHandler.SpawnParticle(heal);
                }
            }

            // Find target
            NPC target = FindTarget(700f);

            if (target != null)
            {
                // Attack behavior
                AttackTarget(target, attackSpeedMult, spriteCount);
            }
            else
            {
                // Idle orbit around player
                IdleOrbit(owner, spriteCount);
            }

            // Ambient effects
            UpdateAmbientEffects();

            // Wing flapping animation
            wingFlap += 0.3f;
        }

        private bool CheckActive(Player owner)
        {
            if (!owner.active || owner.dead)
            {
                owner.ClearBuff(ModContent.BuffType<FlowerSpriteBuff>());
                return false;
            }

            if (owner.HasBuff(ModContent.BuffType<FlowerSpriteBuff>()))
            {
                Projectile.timeLeft = 2;
                return true;
            }
            return false;
        }

        private int CountFlowerSprites(Player owner)
        {
            int count = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == owner.whoAmI && proj.type == Projectile.type)
                    count++;
            }
            return count;
        }

        private void UpdateSpriteIndex(Player owner)
        {
            int index = 0;
            foreach (Projectile proj in Main.projectile)
            {
                if (proj.active && proj.owner == owner.whoAmI && proj.type == Projectile.type)
                {
                    if (proj.whoAmI == Projectile.whoAmI)
                    {
                        spriteIndex = index;
                        return;
                    }
                    index++;
                }
            }
        }

        private NPC FindTarget(float range)
        {
            NPC closest = null;
            float closestDist = range;

            // Check for manually targeted NPC first
            if (Main.player[Projectile.owner].HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[Main.player[Projectile.owner].MinionAttackTargetNPC];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, target.Center);
                    if (dist < range * 1.5f)
                        return target;
                }
            }

            // Otherwise find closest enemy
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

        private void AttackTarget(NPC target, float attackSpeedMult, int spriteCount)
        {
            Vector2 toTarget = target.Center - Projectile.Center;
            float distToTarget = toTarget.Length();

            // Orbit around target at attack range
            float attackRange = 150f;
            float orbitSpeed = 0.05f + spriteCount * 0.01f;
            orbitAngle += orbitSpeed;

            Vector2 orbitOffset = orbitAngle.ToRotationVector2() * attackRange;
            Vector2 idealPos = target.Center + orbitOffset;

            Vector2 toIdeal = idealPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.1f, 0.15f);
            Projectile.velocity = Vector2.Clamp(Projectile.velocity, new Vector2(-12f, -12f), new Vector2(12f, 12f));

            // Face target
            Projectile.spriteDirection = target.Center.X > Projectile.Center.X ? 1 : -1;

            // Attack
            int baseAttackRate = 60; // 1 attack per second base
            int attackRate = (int)(baseAttackRate / attackSpeedMult);
            
            if (attackCooldown <= 0 && Main.myPlayer == Projectile.owner)
            {
                attackCooldown = attackRate;
                
                // Fire pollen projectile
                Vector2 shootDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                Vector2 shootVel = shootDir * 12f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, shootVel,
                    ModContent.ProjectileType<PollenBolt>(), Projectile.damage, Projectile.knockBack, Projectile.owner);

                // Attack VFX
                CustomParticles.GenericFlare(Projectile.Center, SpringYellow, 0.45f, 15);
            }

            // Bloom Formation: Sync attack every 3 seconds with 3+ sprites
            if (spriteCount >= 3 && syncAttackTimer >= 180)
            {
                syncAttackTimer = 0;
                
                // Only trigger from sprite index 0 to avoid multiple triggers
                if (spriteIndex == 0 && Main.myPlayer == Projectile.owner)
                {
                    SyncBurstAttack(target, spriteCount);
                }
            }
        }

        private void SyncBurstAttack(NPC target, int spriteCount)
        {
            // ☁EMUSICAL BURST - Grand symphony of notes on sync attack!
            ThemedParticles.MusicNoteBurst(target.Center, SpringYellow, 12, 5f);
            ThemedParticles.MusicNoteRing(target.Center, SpringPink, 80f, 8);
            
            // Big VFX - layered bloom instead of halo
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.9f, 25);
            CustomParticles.GenericFlare(target.Center, SpringYellow * 0.7f, 0.65f, 20);
            CustomParticles.GenericFlare(target.Center, SpringYellow * 0.5f, 0.45f, 16);
            
            // Flower sparkle ring
            for (int s = 0; s < 6; s++)
            {
                float sparkAngle = MathHelper.TwoPi * s / 6f;
                Vector2 sparkPos = target.Center + sparkAngle.ToRotationVector2() * 25f;
                CustomParticles.GenericFlare(sparkPos, SpringYellow * 0.8f, 0.25f, 14);
            }

            // Ring of petals around target
            int petalCount = 8 + spriteCount * 2;
            for (int i = 0; i < petalCount; i++)
            {
                float angle = MathHelper.TwoPi * i / petalCount;
                Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 80f;
                Vector2 petalVel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * 10f;
                
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), spawnPos, petalVel,
                    ModContent.ProjectileType<SyncPetal>(), Projectile.damage * 3 / 2, Projectile.knockBack * 0.5f, Projectile.owner);
            }

            // Burst particles
            for (int i = 0; i < 12; i++)
            {
                float angle = MathHelper.TwoPi * i / 12f;
                Vector2 burstVel = angle.ToRotationVector2() * Main.rand.NextFloat(4f, 8f);
                Color burstColor = Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat());
                var burst = new GenericGlowParticle(target.Center, burstVel, burstColor * 0.7f, 0.35f, 25, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        private void IdleOrbit(Player owner, int spriteCount)
        {
            // Calculate orbit offset based on sprite index
            float angleOffset = MathHelper.TwoPi * spriteIndex / Math.Max(1, spriteCount);
            orbitAngle += 0.03f;
            float radius = 60f + (float)Math.Sin(Main.GameUpdateCount * 0.03f) * 10f;

            Vector2 orbitPos = owner.Center + (orbitAngle + angleOffset).ToRotationVector2() * radius;
            orbitPos.Y -= 30f; // Hover above

            Vector2 toIdeal = orbitPos - Projectile.Center;
            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal * 0.08f, 0.1f);
            Projectile.velocity = Vector2.Clamp(Projectile.velocity, new Vector2(-8f, -8f), new Vector2(8f, 8f));

            // Face movement direction
            if (Math.Abs(Projectile.velocity.X) > 0.5f)
                Projectile.spriteDirection = Projectile.velocity.X > 0 ? 1 : -1;
        }

        private void UpdateAmbientEffects()
        {
            // ☁EMUSICAL NOTATION - Notes float around the sprite! - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.5f, 0.5f), Main.rand.NextFloat(-1.5f, -0.5f));
                Color noteColor = Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat());
                ThemedParticles.MusicNote(Projectile.Center + Main.rand.NextVector2Circular(15f, 15f), noteVel, noteColor, 0.72f, 40);
                
                // Spring sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(12f, 12f), noteVel * 0.5f, SpringYellow * 0.5f, 0.25f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Sparkle trail
            if (Main.rand.NextBool(5))
            {
                Vector2 sparklePos = Projectile.Center + Main.rand.NextVector2Circular(10f, 10f);
                Color sparkleColor = Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat()) * 0.6f;
                var sparkle = new GenericGlowParticle(sparklePos, Projectile.velocity * -0.1f, sparkleColor, 0.22f, 20, true);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            // Occasional petal
            if (Main.rand.NextBool(20))
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PinkFairy, Main.rand.NextVector2Circular(2f, 2f), 0, SpringPink, 0.8f);
                dust.noGravity = true;
            }

            // Dynamic light
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f + spriteIndex) * 0.15f + 0.6f;
            Lighting.AddLight(Projectile.Center, SpringYellow.ToVector3() * pulse * 0.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteBatch spriteBatch = Main.spriteBatch;

            // Wing flap offset
            float flapOffset = (float)Math.Sin(wingFlap) * 4f;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Ethereal glow
            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f + spriteIndex) * 0.12f + 1f;
            spriteBatch.Draw(texture, drawPos, null, SpringYellow * 0.35f, 0f, origin, 0.65f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.4f, 0f, origin, 0.5f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringWhite * 0.5f, 0f, origin, 0.35f * pulse, SpriteEffects.None, 0f);

            // "Wings" as side flares
            for (int side = -1; side <= 1; side += 2)
            {
                Vector2 wingOffset = new Vector2(side * 12f, flapOffset * side);
                spriteBatch.Draw(texture, drawPos + wingOffset, null, SpringPink * 0.25f, MathHelper.PiOver4 * side, origin, 0.3f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Pollen Bolt - Attack projectile from Flower Sprite
    /// Creates pollen cloud on hit
    /// </summary>
    public class PollenBolt : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringYellow = new Color(255, 255, 180);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

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
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.light = 0.3f;
            Projectile.tileCollide = true;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow2";

        public override void AI()
        {
            Projectile.rotation += 0.2f;

            if (Main.rand.NextBool(3))
            {
                Color trailColor = Color.Lerp(SpringYellow, SpringGreen, Main.rand.NextFloat()) * 0.6f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.08f, trailColor, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Notes scatter from pollen! - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-0.4f, 0.4f), Main.rand.NextFloat(-0.8f, -0.2f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringYellow * 0.9f, 0.68f, 35);
                
                // Pollen sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, SpringGreen * 0.4f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringYellow.ToVector3() * 0.3f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // ☁EMUSICAL IMPACT - Notes sing on pollen hit!
            ThemedParticles.MusicNoteBurst(target.Center, SpringYellow, 5, 2.5f);
            
            // Dynamic spring bloom impact
            SpringImpact(target.Center, 0.85f);
            DramaticImpact(target.Center, SpringPink, SpringGreen, 0.4f, 18);
            
            // Spawn pollen cloud
            if (Main.myPlayer == Projectile.owner)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero,
                    ModContent.ProjectileType<PollenCloud>(), Projectile.damage / 3, 0f, Projectile.owner);
            }
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, SpringYellow, 0.4f, 12);
            
            for (int i = 0; i < 4; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(3f, 3f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SpringGreen * 0.6f, 0.2f, 15, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(SpringYellow, SpringGreen, progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.4f * (1f - progress * 0.5f), SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, drawPos, null, SpringYellow * 0.4f, Projectile.rotation, origin, 0.4f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.5f, Projectile.rotation, origin, 0.25f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Pollen Cloud - Lingering damage zone
    /// </summary>
    public class PollenCloud : ModProjectile
    {
        private static readonly Color SpringYellow = new Color(255, 255, 180);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.light = 0.2f;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 20;
            Projectile.alpha = 50;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow3";

        public override void AI()
        {
            Projectile.velocity *= 0.95f;
            Projectile.alpha = (int)MathHelper.Lerp(50, 200, 1f - (float)Projectile.timeLeft / 90f);
            
            // Pollen particles
            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(20f, 20f);
                Vector2 particleVel = Main.rand.NextVector2Circular(1f, 1f) + new Vector2(0, -0.5f);
                Color particleColor = Color.Lerp(SpringYellow, SpringGreen, Main.rand.NextFloat()) * 0.5f;
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.18f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            float alpha = 1f - (float)Projectile.alpha / 255f;
            Lighting.AddLight(Projectile.Center, SpringYellow.ToVector3() * 0.25f * alpha);
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

            spriteBatch.Draw(texture, drawPos, null, SpringYellow * 0.3f * alpha, 0f, origin, 0.6f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SpringGreen * 0.25f * alpha, 0f, origin, 0.45f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Sync Petal - Bloom Formation attack petal
    /// </summary>
    public class SyncPetal : ModProjectile
    {
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringYellow = new Color(255, 255, 180);
        private static readonly Color SpringGreen = new Color(144, 238, 144);

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 60;
            Projectile.light = 0.4f;
            Projectile.tileCollide = false;
            Projectile.alpha = 0;
        }

        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle3";

        public override void AI()
        {
            Projectile.rotation += 0.25f;

            if (Main.rand.NextBool(2))
            {
                Color trailColor = Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat()) * 0.65f;
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, trailColor, 0.25f, 18, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Notes trail behind sync petals! - VISIBLE SCALE 0.7f+
            if (Main.rand.NextBool(4))
            {
                Vector2 noteVel = -Projectile.velocity * 0.03f + new Vector2(Main.rand.NextFloat(-0.3f, 0.3f), Main.rand.NextFloat(-0.6f, -0.1f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, Color.Lerp(SpringPink, SpringYellow, Main.rand.NextFloat()), 0.7f, 35);
                
                // Spring sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.5f, SpringGreen * 0.4f, 0.2f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.4f);
        }

        public override void OnKill(int timeLeft)
        {
            // ☁EMUSICAL FINALE - Notes scatter on petal death!
            ThemedParticles.MusicNoteBurst(Projectile.Center, SpringPink, 6, 3f);
            
            CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.5f, 15);
            
            for (int i = 0; i < 5; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(Projectile.Center, burstVel, SpringYellow * 0.6f, 0.22f, 18, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            SpriteBatch spriteBatch = Main.spriteBatch;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Trail
            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                if (Projectile.oldPos[i] == Vector2.Zero) continue;
                float progress = (float)i / Projectile.oldPos.Length;
                Color trailColor = Color.Lerp(SpringPink, SpringYellow, progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = Projectile.oldPos[i] + Projectile.Size / 2f - Main.screenPosition;
                spriteBatch.Draw(texture, trailPos, null, trailColor, Projectile.oldRot[i], origin, 0.5f * (1f - progress * 0.4f), SpriteEffects.None, 0f);
            }

            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.4f, Projectile.rotation, origin, 0.55f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, Color.White * 0.55f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
