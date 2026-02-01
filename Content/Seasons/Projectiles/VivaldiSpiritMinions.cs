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
    /// Spring Spirit Minion - Fast, supportive, heals on hit
    /// </summary>
    public class SpringSpiritMinion : ModProjectile
    {
        // Use the proper Spring minion sprite
        public override string Texture => "MagnumOpus/Content/Spring/Projectiles/FlowerSpriteMinion";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringGreen = new Color(144, 238, 144);
        private static readonly Color SpringWhite = new Color(255, 250, 250);

        private bool HasTarget => Projectile.ai[0] > 0;
        private int CoordinatedAttackTimer { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 0f; // No slots - all 4 spirits come together
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        /// <summary>
        /// Gets the damage multiplier based on player's total minion slots.
        /// More slots = stronger spirits.
        /// </summary>
        private float GetMinionSlotBonus(Player owner)
        {
            int slots = owner.maxMinions;
            // Base 1.0x at 1 slot, +15% per additional slot, caps at 3x
            return Math.Min(3f, 1f + (slots - 1) * 0.15f);
        }

        /// <summary>
        /// Gets the speed multiplier based on player's total minion slots.
        /// </summary>
        private float GetSpeedBonus(Player owner)
        {
            int slots = owner.maxMinions;
            // Base 1.0x at 1 slot, +8% per additional slot, caps at 2x
            return Math.Min(2f, 1f + (slots - 1) * 0.08f);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            // Keep minion alive
            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            // Get minion slot bonuses
            float damageBonus = GetMinionSlotBonus(owner);
            float speedBonus = GetSpeedBonus(owner);

            // Find target
            NPC target = FindTarget(owner);
            float targetDist = target != null ? Vector2.Distance(Projectile.Center, target.Center) : 999f;

            // Coordinated attack mode
            if (CoordinatedAttackTimer > 0)
            {
                CoordinatedAttackTimer--;

                if (target != null)
                {
                    // Aggressive homing during coordination
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 16f * speedBonus, 0.12f);

                    // Fire petal bolts rapidly
                    if (CoordinatedAttackTimer % 8 == 0)
                    {
                        Vector2 boltVel = targetDir * 14f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritPetalBolt>(), (int)(Projectile.damage * damageBonus), Projectile.knockBack * 0.5f, Projectile.owner);
                        CustomParticles.GenericFlare(Projectile.Center, SpringPink, 0.35f, 12);
                    }
                }
            }
            else
            {
                // Normal behavior - orbit and dart at enemies
                float baseAngle = Main.GameUpdateCount * 0.025f + Projectile.whoAmI * 0.5f;
                Vector2 idlePos = owner.Center + baseAngle.ToRotationVector2() * 80f + new Vector2(0, -30f);

                if (target != null && targetDist < 450f)
                {
                    // Dart toward enemy, then retreat
                    float dartPhase = (Main.GameUpdateCount % 90) / 90f;
                    if (dartPhase < 0.4f)
                    {
                        // Dart to target
                        Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 14f * speedBonus, 0.08f);
                    }
                    else
                    {
                        // Return to orbit
                        Vector2 toIdle = (idlePos - Projectile.Center);
                        if (toIdle.Length() > 30f)
                        {
                            Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * 10f, 0.06f);
                        }
                    }

                    // Fire petal bolt periodically - faster with more minion slots
                    int fireInterval = Math.Max(20, (int)(45 / speedBonus));
                    if (Main.GameUpdateCount % fireInterval == 0)
                    {
                        Vector2 boltVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 12f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritPetalBolt>(), (int)(Projectile.damage / 2 * damageBonus), Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                }
                else
                {
                    // Idle orbit
                    Vector2 toIdle = (idlePos - Projectile.Center);
                    if (toIdle.Length() > 15f)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * 8f, 0.05f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.9f;
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.05f;

            // Particles
            if (Main.rand.NextBool(4))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Color particleColor = Color.Lerp(SpringPink, SpringGreen, Main.rand.NextFloat()) * 0.4f;
                var particle = new GenericGlowParticle(particlePos, Main.rand.NextVector2Circular(1f, 1f), particleColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Spring spirit's gentle song (VISIBLE SCALE 0.72f+)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.85f, 0.72f, 38);
            }
            
            // ☁ESPARKLE ACCENT - Spirit petal shimmer
            if (Main.rand.NextBool(6))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, SpringGreen, 0.22f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Heal on hit
            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 25));

            target.AddBuff(BuffID.Poisoned, 120);

            CustomParticles.GenericFlare(target.Center, SpringPink, 0.5f, 16);
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(target.Center, burstVel, SpringGreen * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL IMPACT - Spring spirit healing harmony
            ThemedParticles.MusicNoteBurst(target.Center, SpringPink * 0.7f, 4, 3f);
        }

        private NPC FindTarget(Player owner)
        {
            float range = 600f;
            NPC closestTarget = null;
            float closestDist = range;

            // Check for player's targeted NPC first
            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                {
                    return target;
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
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            // Use the ACTUAL sprite texture for the minion
            Texture2D spriteTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Spring/Projectiles/FlowerSpriteMinion").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 spriteOrigin = spriteTexture.Size() / 2f;
            Vector2 glowOrigin = glowTexture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.08f) * 0.1f + 1f;
            float coordinatedBoost = CoordinatedAttackTimer > 0 ? 1.3f : 1f;
            
            // Draw additive glow BEHIND the sprite
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer glow layers
            Color springGlow = SpringPink with { A = 0 };
            spriteBatch.Draw(glowTexture, drawPos, null, springGlow * 0.3f * coordinatedBoost, 0f, glowOrigin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTexture, drawPos, null, SpringGreen with { A = 0 } * 0.2f * coordinatedBoost, 0f, glowOrigin, 0.8f * pulse, SpriteEffects.None, 0f);

            // Petal-like orbiting glow points
            for (int i = 0; i < 5; i++)
            {
                float petalAngle = Main.GameUpdateCount * 0.04f + MathHelper.TwoPi * i / 5f;
                Vector2 petalPos = drawPos + petalAngle.ToRotationVector2() * 20f;
                spriteBatch.Draw(glowTexture, petalPos, null, springGlow * 0.35f, 0f, glowOrigin, 0.15f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw the ACTUAL minion sprite
            float scale = 1f * pulse;
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color drawColor = Projectile.GetAlpha(lightColor) * coordinatedBoost;
            spriteBatch.Draw(spriteTexture, drawPos, null, drawColor, Projectile.rotation * 0.5f, spriteOrigin, scale, effects, 0f);

            return false;
        }
    }

    /// <summary>
    /// Spirit Petal Bolt - Spring spirit projectile
    /// </summary>
    public class SpiritPetalBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle3";
        
        private static readonly Color SpringPink = new Color(255, 183, 197);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation += 0.15f;

            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, SpringPink * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Petal bolt flutter (VISIBLE SCALE 0.68f+)
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SpringPink * 0.8f, 0.68f, 28);
            }
            
            // ☁ESPARKLE ACCENT - Petal twinkle
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.08f, SpringPink, 0.18f, 14);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SpringPink.ToVector3() * 0.2f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Poisoned, 90);
            CustomParticles.GenericFlare(target.Center, SpringPink, 0.35f, 12);

            // ☁EMUSICAL IMPACT - Petal bolt chime
            ThemedParticles.MusicNoteBurst(target.Center, SpringPink * 0.6f, 2, 2f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle3").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SpringPink * 0.55f, Projectile.rotation, origin, 0.22f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Summer Spirit Minion - Aggressive, fires solar bolts
    /// </summary>
    public class SummerSpiritMinion : ModProjectile
    {
        // Use Summer spirit sprite (uses SunfireCore as placeholder until dedicated sprite exists)
        public override string Texture => "MagnumOpus/Content/Summer/Materials/SunfireCore";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerWhite = new Color(255, 255, 240);

        private int CoordinatedAttackTimer { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;
            ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true;
            Main.projPet[Projectile.type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.minion = true;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.minionSlots = 0f; // No slots - all 4 spirits come together
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        private float GetMinionSlotBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(3f, 1f + (slots - 1) * 0.15f);
        }

        private float GetSpeedBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(2f, 1f + (slots - 1) * 0.08f);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            float damageBonus = GetMinionSlotBonus(owner);
            float speedBonus = GetSpeedBonus(owner);

            NPC target = FindTarget(owner);
            float targetDist = target != null ? Vector2.Distance(Projectile.Center, target.Center) : 999f;

            if (CoordinatedAttackTimer > 0)
            {
                CoordinatedAttackTimer--;

                if (target != null)
                {
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 18f * speedBonus, 0.1f);

                    if (CoordinatedAttackTimer % 6 == 0)
                    {
                        Vector2 boltVel = targetDir * 16f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritSolarBolt>(), (int)(Projectile.damage * damageBonus), Projectile.knockBack * 0.5f, Projectile.owner);
                        CustomParticles.GenericFlare(Projectile.Center, SummerGold, 0.4f, 12);
                    }
                }
            }
            else
            {
                float baseAngle = Main.GameUpdateCount * 0.03f + Projectile.whoAmI * 0.7f;
                Vector2 idlePos = owner.Center + baseAngle.ToRotationVector2() * 70f + new Vector2(0, -25f);

                if (target != null && targetDist < 500f)
                {
                    // Aggressive pursuit
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 12f * speedBonus, 0.06f);

                    int fireInterval = Math.Max(12, (int)(25 / speedBonus));
                    if (Main.GameUpdateCount % fireInterval == 0)
                    {
                        Vector2 boltVel = targetDir * 14f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritSolarBolt>(), (int)(Projectile.damage / 2 * damageBonus), Projectile.knockBack * 0.4f, Projectile.owner);
                        CustomParticles.GenericFlare(Projectile.Center, SummerOrange, 0.35f, 10);
                    }
                }
                else
                {
                    Vector2 toIdle = (idlePos - Projectile.Center);
                    if (toIdle.Length() > 15f)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * 9f, 0.05f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.9f;
                    }
                }
            }

            Projectile.rotation = Projectile.velocity.X * 0.04f;

            if (Main.rand.NextBool(3))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(12f, 12f);
                Color particleColor = Color.Lerp(SummerGold, SummerOrange, Main.rand.NextFloat()) * 0.45f;
                var particle = new GenericGlowParticle(particlePos, Main.rand.NextVector2Circular(1f, 1f), particleColor, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Summer spirit's blazing anthem (VISIBLE SCALE 0.72f+)
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SummerGold * 0.85f, 0.72f, 38);
            }
            
            // ☁ESPARKLE ACCENT - Solar spirit shimmer
            if (Main.rand.NextBool(5))
            {
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), Vector2.Zero, SummerOrange, 0.25f, 16);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.5f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 180);
            target.AddBuff(BuffID.Daybreak, 90);

            CustomParticles.GenericFlare(target.Center, SummerGold, 0.55f, 18);
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                var burst = new GenericGlowParticle(target.Center, burstVel, SummerOrange * 0.5f, 0.25f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL IMPACT - Summer spirit solar fanfare
            ThemedParticles.MusicNoteBurst(target.Center, SummerGold * 0.75f, 5, 3.5f);
        }

        private NPC FindTarget(Player owner)
        {
            float range = 600f;
            NPC closestTarget = null;
            float closestDist = range;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                    return target;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/StarBurst1").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.1f) * 0.12f + 1f;
            float coordinatedBoost = CoordinatedAttackTimer > 0 ? 1.4f : 1f;

            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.35f * coordinatedBoost, Projectile.rotation, origin, 0.6f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.55f * coordinatedBoost, Projectile.rotation, origin, 0.42f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerWhite * 0.75f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            // Solar flare points
            for (int i = 0; i < 6; i++)
            {
                float flareAngle = Main.GameUpdateCount * 0.05f + MathHelper.TwoPi * i / 6f;
                Vector2 flarePos = drawPos + flareAngle.ToRotationVector2() * (18f + (float)Math.Sin(Main.GameUpdateCount * 0.15f + i) * 4f);
                spriteBatch.Draw(texture, flarePos, null, SummerGold * 0.5f, 0f, origin, 0.1f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Spirit Solar Bolt - Summer spirit projectile
    /// </summary>
    public class SpiritSolarBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/MagicSparklField7";
        
        private static readonly Color SummerGold = new Color(255, 215, 0);
        private static readonly Color SummerOrange = new Color(255, 140, 0);

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 75;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 40;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(2))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, SummerOrange * 0.45f, 0.18f, 14, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Solar bolt sizzle (VISIBLE SCALE 0.68f+)
            if (Main.rand.NextBool(5))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, SummerGold * 0.85f, 0.68f, 28);
            }
            
            // ☁ESPARKLE ACCENT - Solar sparkle
            if (Main.rand.NextBool(4))
            {
                var sparkle = new SparkleParticle(Projectile.Center, -Projectile.velocity * 0.08f, SummerOrange, 0.2f, 12);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, SummerGold.ToVector3() * 0.35f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.OnFire3, 120);
            CustomParticles.GenericFlare(target.Center, SummerGold, 0.4f, 14);

            // ☁EMUSICAL IMPACT - Solar bolt flare
            ThemedParticles.MusicNoteBurst(target.Center, SummerGold * 0.65f, 3, 2.5f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/MagicSparklField7").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, SummerOrange * 0.45f, Projectile.rotation, origin, 0.3f, SpriteEffects.None, 0f);
            spriteBatch.Draw(texture, drawPos, null, SummerGold * 0.65f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }

    /// <summary>
    /// Autumn Spirit Minion - Area control, life drain aura
    /// </summary>
    public class AutumnSpiritMinion : ModProjectile
    {
        // Use the proper Autumn minion sprite
        public override string Texture => "MagnumOpus/Content/Autumn/Projectiles/HarvestWraithMinion";
        
        private static readonly Color AutumnOrange = new Color(255, 140, 50);
        private static readonly Color AutumnBrown = new Color(139, 90, 43);
        private static readonly Color AutumnRed = new Color(180, 50, 30);

        private int CoordinatedAttackTimer { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private int auraDamageTimer = 0;

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
            Projectile.minionSlots = 0f; // No slots - all 4 spirits come together
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        private float GetMinionSlotBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(3f, 1f + (slots - 1) * 0.15f);
        }

        private float GetSpeedBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(2f, 1f + (slots - 1) * 0.08f);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            float damageBonus = GetMinionSlotBonus(owner);
            float speedBonus = GetSpeedBonus(owner);

            NPC target = FindTarget(owner);

            // Life drain aura - scales with minion slots
            auraDamageTimer++;
            int auraInterval = Math.Max(15, (int)(30 / speedBonus));
            if (auraDamageTimer >= auraInterval)
            {
                auraDamageTimer = 0;
                float auraRange = (CoordinatedAttackTimer > 0 ? 150f : 100f) * (1f + (owner.maxMinions - 1) * 0.1f);
                int auraDamage = (int)((CoordinatedAttackTimer > 0 ? Projectile.damage / 2 : Projectile.damage / 4) * damageBonus);

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < auraRange)
                        {
                            npc.SimpleStrikeNPC(auraDamage, 0, false, 0f, DamageClass.Summon);
                            npc.AddBuff(BuffID.ShadowFlame, 60);
                            owner.Heal(Math.Max(1, auraDamage / 10));

                            // Drain particle
                            Vector2 drainDir = (Projectile.Center - npc.Center).SafeNormalize(Vector2.Zero);
                            var drain = new GenericGlowParticle(npc.Center, drainDir * 3f, AutumnRed * 0.5f, 0.2f, 20, true);
                            MagnumParticleHandler.SpawnParticle(drain);
                        }
                    }
                }

                // Aura pulse VFX
                CustomParticles.HaloRing(Projectile.Center, AutumnOrange * 0.3f, auraRange / 200f, 20);
            }

            if (CoordinatedAttackTimer > 0)
            {
                CoordinatedAttackTimer--;

                if (target != null)
                {
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 12f * speedBonus, 0.08f);
                }
            }
            else
            {
                float baseAngle = Main.GameUpdateCount * 0.02f + Projectile.whoAmI * 0.6f;
                Vector2 idlePos = owner.Center + baseAngle.ToRotationVector2() * 90f + new Vector2(0, -20f);

                if (target != null && Vector2.Distance(Projectile.Center, target.Center) < 400f)
                {
                    // Stay at medium range for aura
                    Vector2 idealPos = target.Center + (Projectile.Center - target.Center).SafeNormalize(Vector2.Zero) * 60f;
                    Vector2 toIdeal = (idealPos - Projectile.Center);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 8f * speedBonus, 0.05f);
                }
                else
                {
                    Vector2 toIdle = (idlePos - Projectile.Center);
                    if (toIdle.Length() > 15f)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * 7f, 0.04f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.92f;
                    }
                }
            }

            Projectile.rotation += 0.02f;

            if (Main.rand.NextBool(4))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(15f, 15f);
                Color particleColor = Color.Lerp(AutumnOrange, AutumnBrown, Main.rand.NextFloat()) * 0.4f;
                Vector2 particleVel = new Vector2(0, Main.rand.NextFloat(-1.5f, -0.5f));
                var particle = new GenericGlowParticle(particlePos, particleVel, particleColor, 0.2f, 20, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Autumn spirit's fading elegy - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, AutumnOrange * 0.6f, 0.72f, 38);
                
                // Autumn Glyph accent for arcane decay theme
                if (Main.rand.NextBool(3))
                    CustomParticles.Glyph(Projectile.Center + Main.rand.NextVector2Circular(8f, 8f), AutumnBrown * 0.5f, 0.25f, Main.rand.Next(1, 13));
            }

            Lighting.AddLight(Projectile.Center, AutumnOrange.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.CursedInferno, 150);
            target.AddBuff(BuffID.ShadowFlame, 120);

            Player owner = Main.player[Projectile.owner];
            owner.Heal(Math.Max(1, damageDone / 20));

            CustomParticles.GenericFlare(target.Center, AutumnOrange, 0.5f, 16);
            for (int i = 0; i < 6; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(4f, 4f);
                var burst = new GenericGlowParticle(target.Center, burstVel, AutumnRed * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL IMPACT - Autumn spirit harvest dirge - VISIBLE SCALE 0.7f+
            ThemedParticles.MusicNoteBurst(target.Center, AutumnOrange * 0.7f, 4, 3f);
            
            // Decay Glyph burst on impact
            CustomParticles.GlyphBurst(target.Center, AutumnRed * 0.5f, 3, 2.5f);
        }

        private NPC FindTarget(Player owner)
        {
            float range = 550f;
            NPC closestTarget = null;
            float closestDist = range;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                    return target;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            // Use the ACTUAL sprite texture for the minion
            Texture2D spriteTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Autumn/Projectiles/HarvestWraithMinion").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/GlowingHalo6").Value;
            Vector2 spriteOrigin = spriteTexture.Size() / 2f;
            Vector2 glowOrigin = glowTexture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.07f) * 0.1f + 1f;
            float coordinatedBoost = CoordinatedAttackTimer > 0 ? 1.35f : 1f;

            // Draw additive glow BEHIND the sprite
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Outer aura ring
            Color autumnGlow = AutumnOrange with { A = 0 };
            float auraScale = (CoordinatedAttackTimer > 0 ? 0.75f : 0.5f) * (0.95f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.05f);
            spriteBatch.Draw(glowTexture, drawPos, null, autumnGlow * 0.2f * coordinatedBoost, 0f, glowOrigin, auraScale, SpriteEffects.None, 0f);

            // Inner glows
            spriteBatch.Draw(glowTexture, drawPos, null, AutumnBrown with { A = 0 } * 0.3f * coordinatedBoost, 0f, glowOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTexture, drawPos, null, autumnGlow * 0.4f * coordinatedBoost, 0f, glowOrigin, 0.38f * pulse, SpriteEffects.None, 0f);

            // Falling leaf points
            for (int i = 0; i < 4; i++)
            {
                float leafAngle = Main.GameUpdateCount * 0.025f + MathHelper.PiOver2 * i;
                float leafDist = 18f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + i * 1.5f) * 6f;
                Vector2 leafPos = drawPos + leafAngle.ToRotationVector2() * leafDist;
                spriteBatch.Draw(glowTexture, leafPos, null, autumnGlow * 0.35f, 0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw the ACTUAL minion sprite
            float scale = 1f * pulse;
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color drawColor = Projectile.GetAlpha(lightColor) * coordinatedBoost;
            spriteBatch.Draw(spriteTexture, drawPos, null, drawColor, Projectile.rotation * 0.3f, spriteOrigin, scale, effects, 0f);

            return false;
        }
    }

    /// <summary>
    /// Winter Spirit Minion - Defensive, slows and freezes enemies
    /// </summary>
    public class WinterSpiritMinion : ModProjectile
    {
        // Use the proper Winter minion sprite
        public override string Texture => "MagnumOpus/Content/Winter/Projectiles/FrostSentinelMinion";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);
        private static readonly Color WinterWhite = new Color(240, 250, 255);
        private static readonly Color WinterPurple = new Color(180, 160, 255);

        private int CoordinatedAttackTimer { get => (int)Projectile.ai[1]; set => Projectile.ai[1] = value; }
        private int frostAuraTimer = 0;

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
            Projectile.minionSlots = 0f; // No slots - all 4 spirits come together
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 2;
        }

        public override bool? CanCutTiles() => false;
        public override bool MinionContactDamage() => true;

        private float GetMinionSlotBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(3f, 1f + (slots - 1) * 0.15f);
        }

        private float GetSpeedBonus(Player owner)
        {
            int slots = owner.maxMinions;
            return Math.Min(2f, 1f + (slots - 1) * 0.08f);
        }

        public override void AI()
        {
            Player owner = Main.player[Projectile.owner];

            if (owner.dead || !owner.active)
            {
                owner.ClearBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>());
                Projectile.Kill();
                return;
            }

            if (owner.HasBuff(ModContent.BuffType<Weapons.VivaldiConductorBuff>()))
            {
                Projectile.timeLeft = 2;
            }

            float damageBonus = GetMinionSlotBonus(owner);
            float speedBonus = GetSpeedBonus(owner);

            NPC target = FindTarget(owner);

            // Frost aura - slows nearby enemies, scales with minion slots
            frostAuraTimer++;
            int frostInterval = Math.Max(10, (int)(20 / speedBonus));
            if (frostAuraTimer >= frostInterval)
            {
                frostAuraTimer = 0;
                float auraRange = (CoordinatedAttackTimer > 0 ? 140f : 90f) * (1f + (owner.maxMinions - 1) * 0.1f);
                float freezeChance = 0.15f + (owner.maxMinions - 1) * 0.02f; // Better freeze chance with more slots

                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && !npc.friendly && !npc.dontTakeDamage)
                    {
                        float dist = Vector2.Distance(Projectile.Center, npc.Center);
                        if (dist < auraRange)
                        {
                            npc.AddBuff(BuffID.Frostburn2, 60);
                            npc.AddBuff(BuffID.Slow, 60);

                            // Freeze chance during coordination - better with more slots
                            if (CoordinatedAttackTimer > 0 && Main.rand.NextFloat() < freezeChance)
                            {
                                npc.AddBuff(BuffID.Frozen, 45);
                            }
                        }
                    }
                }
            }

            if (CoordinatedAttackTimer > 0)
            {
                CoordinatedAttackTimer--;

                if (target != null)
                {
                    Vector2 targetDir = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, targetDir * 14f * speedBonus, 0.09f);

                    if (CoordinatedAttackTimer % 10 == 0)
                    {
                        Vector2 boltVel = targetDir * 12f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritIceBolt>(), (int)(Projectile.damage / 2 * damageBonus), Projectile.knockBack * 0.5f, Projectile.owner);
                        CustomParticles.GenericFlare(Projectile.Center, WinterBlue, 0.35f, 12);
                    }
                }
            }
            else
            {
                float baseAngle = Main.GameUpdateCount * 0.022f + Projectile.whoAmI * 0.8f;
                Vector2 idlePos = owner.Center + baseAngle.ToRotationVector2() * 75f + new Vector2(0, -35f);

                if (target != null && Vector2.Distance(Projectile.Center, target.Center) < 450f)
                {
                    // Defensive positioning - stay between target and player
                    Vector2 defendPos = owner.Center + (target.Center - owner.Center).SafeNormalize(Vector2.Zero) * 60f;
                    Vector2 toDefend = (defendPos - Projectile.Center);
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, toDefend.SafeNormalize(Vector2.Zero) * 9f * speedBonus, 0.05f);

                    int fireInterval = Math.Max(18, (int)(35 / speedBonus));
                    if (Main.GameUpdateCount % fireInterval == 0)
                    {
                        Vector2 boltVel = (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f * speedBonus;
                        Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, boltVel,
                            ModContent.ProjectileType<SpiritIceBolt>(), (int)(Projectile.damage / 3 * damageBonus), Projectile.knockBack * 0.3f, Projectile.owner);
                    }
                }
                else
                {
                    Vector2 toIdle = (idlePos - Projectile.Center);
                    if (toIdle.Length() > 15f)
                    {
                        Projectile.velocity = Vector2.Lerp(Projectile.velocity, toIdle.SafeNormalize(Vector2.Zero) * 7f, 0.04f);
                    }
                    else
                    {
                        Projectile.velocity *= 0.9f;
                    }
                }
            }

            Projectile.rotation += 0.015f;

            if (Main.rand.NextBool(4))
            {
                Vector2 particlePos = Projectile.Center + Main.rand.NextVector2Circular(14f, 14f);
                Color particleColor = Color.Lerp(WinterBlue, WinterWhite, Main.rand.NextFloat()) * 0.4f;
                var particle = new GenericGlowParticle(particlePos, Main.rand.NextVector2Circular(1f, 1f), particleColor, 0.2f, 18, true);
                MagnumParticleHandler.SpawnParticle(particle);
            }

            // ☁EMUSICAL NOTATION - Winter spirit's frost carol - VISIBLE SCALE 0.72f+
            if (Main.rand.NextBool(8))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, WinterBlue * 0.6f, 0.72f, 38);
                
                // Frost sparkle accent
                var sparkle = new SparkleParticle(Projectile.Center + Main.rand.NextVector2Circular(10f, 10f), noteVel * 0.5f, WinterWhite * 0.6f, 0.35f, 25);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, WinterBlue.ToVector3() * 0.4f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 180);
            target.AddBuff(BuffID.Slow, 120);

            if (Main.rand.NextFloat() < 0.2f)
            {
                target.AddBuff(BuffID.Frozen, 60);
            }

            CustomParticles.GenericFlare(target.Center, WinterBlue, 0.55f, 18);
            for (int i = 0; i < 8; i++)
            {
                Vector2 burstVel = Main.rand.NextVector2Circular(5f, 5f);
                var burst = new GenericGlowParticle(target.Center, burstVel, WinterWhite * 0.5f, 0.22f, 16, true);
                MagnumParticleHandler.SpawnParticle(burst);
            }

            // ☁EMUSICAL IMPACT - Winter spirit crystalline chime - VISIBLE SCALE 0.72f+
            ThemedParticles.MusicNoteBurst(target.Center, WinterBlue * 0.75f, 5, 3.5f);
            
            // Frost sparkle burst on impact
            for (int j = 0; j < 4; j++)
            {
                Vector2 sparkleVel = Main.rand.NextVector2Circular(3f, 3f);
                var sparkle = new SparkleParticle(target.Center, sparkleVel, WinterWhite * 0.7f, 0.3f, 20);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }
        }

        private NPC FindTarget(Player owner)
        {
            float range = 500f;
            NPC closestTarget = null;
            float closestDist = range;

            if (owner.HasMinionAttackTargetNPC)
            {
                NPC target = Main.npc[owner.MinionAttackTargetNPC];
                if (target.active && !target.friendly && !target.dontTakeDamage)
                    return target;
            }

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5 && !npc.dontTakeDamage)
                {
                    float dist = Vector2.Distance(Projectile.Center, npc.Center);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        closestTarget = npc;
                    }
                }
            }

            return closestTarget;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            // Use the ACTUAL sprite texture for the minion
            Texture2D spriteTexture = ModContent.Request<Texture2D>("MagnumOpus/Content/Winter/Projectiles/FrostSentinelMinion").Value;
            Texture2D glowTexture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow2").Value;
            Vector2 spriteOrigin = spriteTexture.Size() / 2f;
            Vector2 glowOrigin = glowTexture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            float pulse = (float)Math.Sin(Main.GameUpdateCount * 0.06f) * 0.1f + 1f;
            float coordinatedBoost = CoordinatedAttackTimer > 0 ? 1.35f : 1f;

            // Draw additive glow BEHIND the sprite
            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Frost aura
            Color winterGlow = WinterBlue with { A = 0 };
            float auraScale = (CoordinatedAttackTimer > 0 ? 0.7f : 0.45f) * (0.95f + (float)Math.Sin(Main.GameUpdateCount * 0.04f) * 0.05f);
            spriteBatch.Draw(glowTexture, drawPos, null, winterGlow * 0.15f * coordinatedBoost, 0f, glowOrigin, auraScale, SpriteEffects.None, 0f);

            spriteBatch.Draw(glowTexture, drawPos, null, WinterPurple with { A = 0 } * 0.25f * coordinatedBoost, 0f, glowOrigin, 0.55f * pulse, SpriteEffects.None, 0f);
            spriteBatch.Draw(glowTexture, drawPos, null, winterGlow * 0.35f * coordinatedBoost, 0f, glowOrigin, 0.4f * pulse, SpriteEffects.None, 0f);

            // Ice crystal points
            for (int i = 0; i < 6; i++)
            {
                float crystalAngle = Main.GameUpdateCount * 0.03f + MathHelper.TwoPi * i / 6f;
                Vector2 crystalPos = drawPos + crystalAngle.ToRotationVector2() * 20f;
                spriteBatch.Draw(glowTexture, crystalPos, null, winterGlow * 0.4f, 0f, glowOrigin, 0.12f, SpriteEffects.None, 0f);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            // Draw the ACTUAL minion sprite
            float scale = 1f * pulse;
            SpriteEffects effects = Projectile.velocity.X < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            Color drawColor = Projectile.GetAlpha(lightColor) * coordinatedBoost;
            spriteBatch.Draw(spriteTexture, drawPos, null, drawColor, Projectile.rotation * 0.3f, spriteOrigin, scale, effects, 0f);

            return false;
        }
    }

    /// <summary>
    /// Spirit Ice Bolt - Winter spirit projectile
    /// </summary>
    public class SpiritIceBolt : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/PrismaticSparkle9";
        
        private static readonly Color WinterBlue = new Color(150, 220, 255);

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (Main.rand.NextBool(3))
            {
                var trail = new GenericGlowParticle(Projectile.Center, -Projectile.velocity * 0.1f, WinterBlue * 0.4f, 0.15f, 12, true);
                MagnumParticleHandler.SpawnParticle(trail);
            }

            // ☁EMUSICAL NOTATION - Ice bolt tinkle - VISIBLE SCALE 0.68f+
            if (Main.rand.NextBool(6))
            {
                Vector2 noteVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1.5f, -0.5f));
                ThemedParticles.MusicNote(Projectile.Center, noteVel, WinterBlue * 0.55f, 0.68f, 28);
                
                // Tiny frost sparkle
                var sparkle = new SparkleParticle(Projectile.Center, noteVel * 0.3f, WinterBlue * 0.5f, 0.2f, 18);
                MagnumParticleHandler.SpawnParticle(sparkle);
            }

            Lighting.AddLight(Projectile.Center, WinterBlue.ToVector3() * 0.25f);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(BuffID.Frostburn2, 90);
            target.AddBuff(BuffID.Slow, 60);
            CustomParticles.GenericFlare(target.Center, WinterBlue, 0.35f, 12);

            // ☁EMUSICAL IMPACT - Ice bolt ping - VISIBLE SCALE 0.68f+
            ThemedParticles.MusicNoteBurst(target.Center, WinterBlue * 0.6f, 2, 2f);
            
            // Frost sparkle on impact
            var sparkle = new SparkleParticle(target.Center, Main.rand.NextVector2Circular(2f, 2f), WinterBlue * 0.5f, 0.25f, 15);
            MagnumParticleHandler.SpawnParticle(sparkle);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteBatch spriteBatch = Main.spriteBatch;
            Texture2D texture = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/PrismaticSparkle9").Value;
            Vector2 origin = texture.Size() / 2f;
            Vector2 drawPos = Projectile.Center - Main.screenPosition;

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            spriteBatch.Draw(texture, drawPos, null, WinterBlue * 0.55f, Projectile.rotation, origin, 0.2f, SpriteEffects.None, 0f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone, null, Main.GameViewMatrix.TransformationMatrix);

            return false;
        }
    }
}
