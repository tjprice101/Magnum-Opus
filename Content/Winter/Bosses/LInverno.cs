using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using MagnumOpus.Content.Winter.Materials;

namespace MagnumOpus.Content.Winter.Bosses
{
    /// <summary>
    /// L'INVERNO, THE FROZEN SILENCE - POST-GOLEM BOSS
    /// 
    /// Design Philosophy:
    /// - Cold, stillness, silence before the storm
    /// - Ice crystals, frost patterns, freezing mechanics
    /// - Slowing debuffs, frost zones, icicle storms
    /// - Musical connection: Vivaldi's Winter from The Four Seasons
    /// 
    /// Theme Colors: White (#FFFFFF), Light Blue (#ADD8E6), Deep Blue (#1E90FF)
    /// </summary>
    public class LInverno : ModNPC
    {
        #region Theme Colors
        private static readonly Color WinterWhite = new Color(255, 255, 255);
        private static readonly Color WinterIce = new Color(173, 216, 230);
        private static readonly Color WinterDeepBlue = new Color(30, 144, 255);
        private static readonly Color FrostBlue = new Color(135, 206, 250);
        private static readonly Color CrystalCyan = new Color(0, 255, 255);
        private static readonly Color GlacialPurple = new Color(150, 130, 200);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 11f;
        private const int BaseDamage = 95;
        private const float EnrageDistance = 2000f;
        private const int AttackWindowFrames = 52;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Idle,
            Attack,
            Reposition,
            Enraged,
            Dying
        }
        
        private enum AttackPattern
        {
            // Phase 1 (100-60% HP)
            IcicleStorm,       // Radial icicle burst
            FrostBreath,       // Cone of freezing projectiles
            CrystalBarrage,    // Targeted crystal shards
            
            // Phase 2 (60-30% HP)
            GlacialCharge,     // Dash with frost trail
            BlizzardVortex,    // Spiraling ice storm
            FreezeRay,         // Beam attack that creates frost zones
            
            // Phase 3 (30-0% HP)
            WintersJudgment,   // Signature spectacle attack
            AbsoluteZero,      // Devastating frost explosion
            EternalFrost       // Ultimate freezing finale
        }
        
        private BossPhase State
        {
            get => (BossPhase)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }
        
        private int Timer
        {
            get => (int)NPC.ai[1];
            set => NPC.ai[1] = value;
        }
        
        private AttackPattern CurrentAttack
        {
            get => (AttackPattern)NPC.ai[2];
            set => NPC.ai[2] = (float)value;
        }
        
        private int SubPhase
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }
        #endregion
        
        #region Instance Variables
        private int difficultyTier = 0;
        private int attackCooldown = 0;
        private AttackPattern lastAttack = AttackPattern.IcicleStorm;
        private int consecutiveAttacks = 0;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 1550;
        
        private Vector2 dashDirection;
        private int dashCount = 0;
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Chilled] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 115;
            NPC.height = 130;
            NPC.damage = BaseDamage;
            NPC.defense = 55;
            NPC.lifeMax = 88000; // Post-Golem tier (between Empress 70k and Moon Lord)
            NPC.HitSound = SoundID.Tink;
            NPC.DeathSound = SoundID.Shatter;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 20);
            NPC.boss = true;
            NPC.npcSlots = 14f;
            NPC.aiStyle = -1;
            NPC.coldDamage = true;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Snow,
                new FlavorTextBestiaryInfoElement("L'Inverno, the Frozen Silence - the crystalline sovereign of eternal winter, whose frigid symphony brings all life to stillness.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Winter);
                hasRegisteredHealthBar = true;
            }
            
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y -= 0.35f;
                    NPC.EncourageDespawn(60);
                    return;
                }
            }
            
            Timer++;
            UpdateDifficultyTier();
            UpdateAggression();
            SpawnAmbientParticles();
            
            float distToTarget = Vector2.Distance(NPC.Center, target.Center);
            if (distToTarget > EnrageDistance && State != BossPhase.Enraged)
            {
                State = BossPhase.Enraged;
                Timer = 0;
            }
            else if (distToTarget <= EnrageDistance && State == BossPhase.Enraged)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
            
            switch (State)
            {
                case BossPhase.Spawning:
                    AI_Spawning(target);
                    break;
                case BossPhase.Idle:
                    AI_Idle(target);
                    break;
                case BossPhase.Attack:
                    AI_Attack(target);
                    break;
                case BossPhase.Reposition:
                    AI_Reposition(target);
                    break;
                case BossPhase.Enraged:
                    AI_Enraged(target);
                    break;
                case BossPhase.Dying:
                    AI_Dying(target);
                    break;
            }
            
            if (NPC.velocity.X != 0)
                NPC.spriteDirection = NPC.velocity.X > 0 ? 1 : -1;
                
            attackCooldown = Math.Max(0, attackCooldown - 1);
        }
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.6f ? 0 : (hpPercent > 0.3f ? 1 : 2);
            
            if (newTier > difficultyTier)
            {
                difficultyTier = newTier;
                PhaseTransitionVFX();
            }
        }
        
        private void PhaseTransitionVFX()
        {
            SoundEngine.PlaySound(SoundID.Shatter with { Pitch = 0.1f }, NPC.Center);
            
            // Crystalline halos
            for (int i = 0; i < 11; i++)
            {
                float progress = i / 11f;
                Color haloColor = Color.Lerp(WinterIce, CrystalCyan, progress);
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.48f + i * 0.15f, 19 + i * 3);
            }
            
            CustomParticles.GenericFlare(NPC.Center, WinterWhite, 2f, 35);
            CustomParticles.GenericFlare(NPC.Center, FrostBlue, 1.6f, 30);
            
            SpawnIceBurst(NPC.Center, 35, 11f);
            MagnumScreenEffects.AddScreenShake(12f);
        }
        
        private void UpdateAggression()
        {
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.4f + difficultyTier * 0.12f;
        private float GetAggressionRateMult() => Math.Max(0.5f, 1f - aggressionLevel * 0.38f - difficultyTier * 0.1f);
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, WinterWhite, 2.4f, 45);
                
                for (int i = 0; i < 15; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(WinterIce, CrystalCyan, i / 15f), 0.38f + i * 0.12f, 16 + i * 2);
                }
                
                SpawnIceBurst(NPC.Center, 45, 12f);
            }
            
            if (Timer >= 90)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Idle(Player target)
        {
            float hoverDist = 280f - difficultyTier * 25f;
            Vector2 idealPos = target.Center + new Vector2(NPC.Center.X > target.Center.X ? hoverDist : -hoverDist, -140f);
            
            // Slow, ominous drift like falling snow
            idealPos.Y += (float)Math.Sin(Timer * 0.02f) * 28f;
            idealPos.X += (float)Math.Cos(Timer * 0.015f) * 18f;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            if (toIdeal.Length() > 35f)
            {
                toIdeal.Normalize();
                float speed = BaseSpeed * GetAggressionSpeedMult();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * speed, 0.05f);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            
            int effectiveCooldown = (int)(attackCooldown * GetAggressionRateMult());
            if (effectiveCooldown <= 0 && Timer > (int)(35 * GetAggressionRateMult()))
            {
                SelectNextAttack(target);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.IcicleStorm,
                AttackPattern.FrostBreath,
                AttackPattern.CrystalBarrage
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.GlacialCharge);
                pool.Add(AttackPattern.BlizzardVortex);
                pool.Add(AttackPattern.FreezeRay);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.WintersJudgment);
                pool.Add(AttackPattern.AbsoluteZero);
                
                if (consecutiveAttacks >= 5)
                    pool.Add(AttackPattern.EternalFrost);
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Attack;
            consecutiveAttacks++;
            dashCount = 0;
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.IcicleStorm:
                    Attack_IcicleStorm(target);
                    break;
                case AttackPattern.FrostBreath:
                    Attack_FrostBreath(target);
                    break;
                case AttackPattern.CrystalBarrage:
                    Attack_CrystalBarrage(target);
                    break;
                case AttackPattern.GlacialCharge:
                    Attack_GlacialCharge(target);
                    break;
                case AttackPattern.BlizzardVortex:
                    Attack_BlizzardVortex(target);
                    break;
                case AttackPattern.FreezeRay:
                    Attack_FreezeRay(target);
                    break;
                case AttackPattern.WintersJudgment:
                    Attack_WintersJudgment(target);
                    break;
                case AttackPattern.AbsoluteZero:
                    Attack_AbsoluteZero(target);
                    break;
                case AttackPattern.EternalFrost:
                    Attack_EternalFrost(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            float idealDist = 320f;
            Vector2 toTarget = (target.Center - NPC.Center);
            float currentDist = toTarget.Length();
            
            if (Math.Abs(currentDist - idealDist) < 70f && Timer > 25)
            {
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames / 2;
                return;
            }
            
            Vector2 idealDir = currentDist > idealDist ? -toTarget.SafeNormalize(Vector2.Zero) : toTarget.SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, idealDir * 12f, 0.07f);
            
            if (Timer > 65)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            float enrageSpeed = BaseSpeed * 1.85f;
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * enrageSpeed, 0.1f);
            
            if (Timer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 7; i++)
                {
                    float angle = MathHelper.TwoPi * i / 7f + Timer * 0.08f;
                    Vector2 vel = angle.ToRotationVector2() * 10f;
                    SpawnIceProjectile(NPC.Center, vel, 60);
                }
            }
            
            if (Timer % 3 == 0)
            {
                SpawnFrostParticle(NPC.Center + Main.rand.NextVector2Circular(55f, 55f));
            }
        }
        
        private void AI_Dying(Player target)
        {
            deathTimer++;
            NPC.velocity *= 0.92f;
            
            if (deathTimer < 115)
            {
                float intensity = (float)deathTimer / 115f;
                
                if (deathTimer % 4 == 0)
                {
                    SpawnIceBurst(NPC.Center, (int)(14 + intensity * 20), 5.5f + intensity * 8f);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + deathTimer * 0.04f;
                        Vector2 offset = angle.ToRotationVector2() * (40f + intensity * 60f);
                        Color flareColor = Color.Lerp(WinterIce, CrystalCyan, (float)i / 8f);
                        CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.48f + intensity * 0.38f, 16);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(intensity * 6f);
            }
            else if (deathTimer == 115)
            {
                // Final crystalline shatter
                CustomParticles.GenericFlare(NPC.Center, WinterWhite, 3f, 50);
                CustomParticles.GenericFlare(NPC.Center, FrostBlue, 2.5f, 45);
                CustomParticles.GenericFlare(NPC.Center, CrystalCyan, 2f, 38);
                
                for (int i = 0; i < 18; i++)
                {
                    Color ringColor = Color.Lerp(WinterIce, CrystalCyan, i / 18f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, 0.55f + i * 0.18f, 22 + i * 3);
                }
                
                SpawnIceBurst(NPC.Center, 60, 18f);
                MagnumScreenEffects.AddScreenShake(24f);
                
                NPC.life = 0;
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_IcicleStorm(Player target)
        {
            int chargeTime = 50 - difficultyTier * 8;
            int projectileCount = 18 + difficultyTier * 6;
            
            NPC.velocity *= 0.94f;
            
            if (SubPhase == 0)
            {
                float progress = (float)Timer / chargeTime;
                
                if (Timer % 3 == 0)
                {
                    int count = (int)(6 + progress * 10);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.04f;
                        float radius = 120f * (1f - progress * 0.45f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnFrostParticle(pos);
                    }
                }
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, WinterIce, 10);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, WinterIce, CrystalCyan, 1.1f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float speed = 10f + difficultyTier * 2.5f;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnIceProjectile(NPC.Center, vel, 60);
                        }
                    }
                    
                    SpawnIceBurst(NPC.Center, 22, 10f);
                }
                
                if (Timer >= 40)
                    EndAttack();
            }
        }
        
        private void Attack_FrostBreath(Player target)
        {
            int breathDuration = 70 + difficultyTier * 18;
            
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 4f, 0.03f);
            
            if (Timer < 22) // Telegraph
            {
                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.WarningLine(NPC.Center, direction, 380f, 8, WarningType.Danger);
                return;
            }
            
            int fireInterval = 5 - difficultyTier;
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                int count = 5 + difficultyTier * 2;
                float spread = MathHelper.ToRadians(52f);
                
                for (int i = 0; i < count; i++)
                {
                    float offsetAngle = MathHelper.Lerp(-spread, spread, (float)i / (count - 1));
                    Vector2 vel = direction.RotatedBy(offsetAngle) * (9f + difficultyTier * 2f + Main.rand.NextFloat(0, 3f));
                    SpawnIceProjectile(NPC.Center, vel, 55);
                }
                
                CustomParticles.GenericFlare(NPC.Center, FrostBlue, 0.7f, 14);
            }
            
            if (Timer >= breathDuration)
                EndAttack();
        }
        
        private void Attack_CrystalBarrage(Player target)
        {
            int burstCount = 6 + difficultyTier * 2;
            int burstDelay = 16 - difficultyTier * 3;
            
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 6f, 0.04f);
            
            if (SubPhase < burstCount)
            {
                if (Timer == 8)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 320f, 6, WarningType.Caution);
                }
                
                if (Timer == burstDelay && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item28, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 3 + difficultyTier;
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offset = MathHelper.Lerp(-0.15f, 0.15f, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offset) * (14f + difficultyTier * 2.5f);
                        SpawnIceProjectile(NPC.Center, vel, 60, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, CrystalCyan, 0.65f, 14);
                }
                
                if (Timer >= burstDelay + 5)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 35)
            {
                EndAttack();
            }
        }
        
        private void Attack_GlacialCharge(Player target)
        {
            int maxDashes = 4 + difficultyTier;
            int telegraphTime = 20 - difficultyTier * 3;
            int dashDuration = 12;
            int recoveryTime = 13 - difficultyTier * 2;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.87f;
                
                dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 520f, 11, WarningType.Danger);
                
                float progress = (float)Timer / telegraphTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 50f, progress, WinterDeepBlue, 7);
                
                if (Timer >= telegraphTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Dash
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.2f }, NPC.Center);
                    NPC.velocity = dashDirection * (35f + difficultyTier * 8f);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, WinterDeepBlue, WinterIce, 1f);
                }
                
                // Frost trail
                if (Timer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 perpendicular = new Vector2(-dashDirection.Y, dashDirection.X);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 spawnPos = NPC.Center + perpendicular * i * 20f;
                        Vector2 vel = -dashDirection * 3f + perpendicular * i * 2f;
                        SpawnIceProjectile(spawnPos, vel, 55);
                    }
                }
                
                SpawnFrostParticle(NPC.Center);
                
                if (Timer >= dashDuration)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2) // Recovery
            {
                NPC.velocity *= 0.86f;
                
                if (Timer >= recoveryTime)
                {
                    dashCount++;
                    if (dashCount >= maxDashes)
                    {
                        EndAttack();
                    }
                    else
                    {
                        Timer = 0;
                        SubPhase = 0;
                    }
                }
            }
        }
        
        private void Attack_BlizzardVortex(Player target)
        {
            int duration = 160 + difficultyTier * 28;
            
            float spinSpeed = (0.03f + difficultyTier * 0.009f) * GetAggressionSpeedMult();
            float radius = 255f - aggressionLevel * 50f;
            float angle = Timer * spinSpeed;
            Vector2 idealPos = target.Center + angle.ToRotationVector2() * radius;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 16f, 0.09f);
            
            int fireInterval = Math.Max(4, 8 - difficultyTier * 2);
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float spiralAngle = Timer * 0.12f;
                int arms = 5 + difficultyTier;
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                    float speed = 9f + difficultyTier * 2f;
                    Vector2 vel = armAngle.ToRotationVector2() * speed;
                    SpawnIceProjectile(NPC.Center, vel, 60, arm % 2 == 0);
                }
                
                CustomParticles.GenericFlare(NPC.Center, FrostBlue, 0.4f, 12);
            }
            
            if (Timer % 6 == 0)
                SpawnFrostParticle(NPC.Center + Main.rand.NextVector2Circular(40f, 40f));
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_FreezeRay(Player target)
        {
            int beamCount = 3 + difficultyTier;
            int beamDelay = 38 - difficultyTier * 5;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase < beamCount)
            {
                if (Timer < 22) // Telegraph
                {
                    float progress = (float)Timer / 22f;
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.LaserBeamWarning(NPC.Center, direction.ToRotation(), 550f, progress);
                }
                
                if (Timer == 22 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    
                    // Fire line of projectiles to simulate beam
                    int segments = 12 + difficultyTier * 3;
                    for (int i = 0; i < segments; i++)
                    {
                        Vector2 spawnPos = NPC.Center + direction * i * 42f;
                        Vector2 vel = direction.RotatedBy(Main.rand.NextFloat(-0.1f, 0.1f)) * (4f + i * 0.4f);
                        SpawnIceProjectile(spawnPos, vel, 55);
                    }
                    
                    // Beam VFX
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 pos = NPC.Center + direction * i * 50f;
                        CustomParticles.GenericFlare(pos, CrystalCyan, 0.6f - i * 0.04f, 18);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, WinterWhite, 1.2f, 22);
                }
                
                if (Timer >= beamDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 40)
            {
                EndAttack();
            }
        }
        
        private void Attack_WintersJudgment(Player target)
        {
            // SIGNATURE SPECTACLE ATTACK
            int chargeTime = 80 - difficultyTier * 12;
            int waveCount = 4 + difficultyTier;
            
            if (SubPhase == 0) // Charge with safe zone
            {
                NPC.velocity *= 0.93f;
                
                float progress = (float)Timer / chargeTime;
                
                // Converging ice crystals
                if (Timer % 3 == 0)
                {
                    int particleCount = (int)(10 + progress * 16);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.05f;
                        float radius = 200f * (1f - progress * 0.55f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnFrostParticle(pos);
                    }
                }
                
                // Safe zone indicator
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 90f, 13);
                    
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(50f), 160f, 8);
                }
                
                if (Timer > chargeTime * 0.7f)
                    MagnumScreenEffects.AddScreenShake(progress * 5f);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount) // Multi-wave with safe arc
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.4f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(16f);
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, WinterWhite, WinterIce, 1.3f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 28 + difficultyTier * 8;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(22f - difficultyTier * 3f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 11f + difficultyTier * 2.5f + SubPhase;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnIceProjectile(NPC.Center, vel, 65, i % 4 == 0);
                        }
                    }
                    
                    // Cascading frost halos
                    for (int i = 0; i < 12; i++)
                    {
                        Color ringColor = Color.Lerp(WinterIce, CrystalCyan, i / 12f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.5f + i * 0.14f, 20 + i * 3);
                    }
                    
                    SpawnIceBurst(NPC.Center, 25, 12f);
                }
                
                if (Timer >= 35)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 50)
            {
                EndAttack();
            }
        }
        
        private void Attack_AbsoluteZero(Player target)
        {
            int chargeTime = 65 - difficultyTier * 10;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.94f;
                
                float progress = (float)Timer / chargeTime;
                
                // Intensifying frost energy
                if (Timer % 2 == 0)
                {
                    int count = (int)(12 + progress * 20);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.065f;
                        float radius = 220f * (1f - progress * 0.65f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color flareColor = Color.Lerp(WinterIce, CrystalCyan, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(pos, flareColor, 0.35f + progress * 0.35f, 13);
                    }
                }
                
                CustomParticles.GenericFlare(NPC.Center, Color.Lerp(FrostBlue, WinterWhite, progress), 0.6f + progress * 1.5f, 10);
                
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 300f, 20);
                
                if (Timer > chargeTime * 0.65f)
                    MagnumScreenEffects.AddScreenShake(progress * 7f);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Explode
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Shatter with { Volume = 1.5f, Pitch = -0.2f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(20f);
                    
                    // Massive frost explosion VFX
                    CustomParticles.GenericFlare(NPC.Center, WinterWhite, 3f, 45);
                    CustomParticles.GenericFlare(NPC.Center, FrostBlue, 2.6f, 40);
                    CustomParticles.GenericFlare(NPC.Center, CrystalCyan, 2.1f, 35);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        Color ringColor = Color.Lerp(FrostBlue, CrystalCyan, i / 20f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.6f + i * 0.2f, 23 + i * 3);
                    }
                    
                    SpawnIceBurst(NPC.Center, 52, 18f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Massive projectile burst
                        int count = 40 + difficultyTier * 12;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            float speed = 8.5f + Main.rand.NextFloat(0, 6f);
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnIceProjectile(NPC.Center, vel, 65, i % 3 == 0);
                        }
                    }
                }
                
                if (Timer >= 58)
                    EndAttack();
            }
        }
        
        private void Attack_EternalFrost(Player target)
        {
            // Ultimate freezing finale
            int chargeTime = 85 - difficultyTier * 12;
            int waveCount = 3;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.92f;
                
                float progress = (float)Timer / chargeTime;
                
                // Massive converging frost storm
                if (Timer % 2 == 0)
                {
                    int count = (int)(16 + progress * 28);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.08f;
                        float radius = 280f * (1f - progress * 0.75f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color flareColor = Color.Lerp(WinterIce, GlacialPurple, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(pos, flareColor, 0.38f + progress * 0.4f, 14);
                    }
                }
                
                CustomParticles.GenericFlare(NPC.Center, Color.Lerp(CrystalCyan, WinterWhite, progress), 0.7f + progress * 1.8f, 11);
                
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 350f, 22);
                
                if (Timer > chargeTime * 0.6f)
                    MagnumScreenEffects.AddScreenShake(progress * 8f);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount) // Multi-wave explosion
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.55f, Pitch = -0.3f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(22f);
                    
                    CustomParticles.GenericFlare(NPC.Center, WinterWhite, 2.8f + SubPhase * 0.3f, 40);
                    CustomParticles.GenericFlare(NPC.Center, CrystalCyan, 2.3f + SubPhase * 0.25f, 35);
                    
                    for (int i = 0; i < 16; i++)
                    {
                        Color ringColor = Color.Lerp(WinterIce, GlacialPurple, i / 16f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.55f + i * 0.17f + SubPhase * 0.08f, 22 + i * 3);
                    }
                    
                    SpawnIceBurst(NPC.Center, 38 + SubPhase * 5, 15f + SubPhase * 2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int count = 32 + difficultyTier * 10 + SubPhase * 5;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(20f - difficultyTier * 2f);
                        
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc && SubPhase < 3) continue;
                            
                            float speed = 9f + difficultyTier * 2f + SubPhase * 2f + Main.rand.NextFloat(0, 4f);
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnIceProjectile(NPC.Center, vel, 70, i % 3 == 0);
                        }
                    }
                }
                
                if (Timer >= 42)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 60)
            {
                EndAttack();
            }
        }
        
        private void EndAttack()
        {
            State = BossPhase.Reposition;
            Timer = 0;
            SubPhase = 0;
            attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
        }
        
        #endregion
        
        #region VFX Helpers
        
        private void SpawnAmbientParticles()
        {
            // Drifting frost crystals
            if (Timer % 6 == 0)
            {
                float baseAngle = Timer * 0.02f;
                for (int i = 0; i < 5; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 5f;
                    float radius = 55f + (float)Math.Sin(Timer * 0.05f + i) * 18f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    SpawnFrostParticle(pos);
                }
            }
            
            // Icy glow
            if (Timer % 10 == 0)
            {
                Color glowColor = Color.Lerp(WinterIce, CrystalCyan, (float)Math.Sin(Timer * 0.02f) * 0.5f + 0.5f);
                CustomParticles.GenericFlare(NPC.Center, glowColor * 0.5f, 0.4f, 16);
            }
            
            Lighting.AddLight(NPC.Center, WinterIce.ToVector3() * 0.6f);
        }
        
        private void SpawnFrostParticle(Vector2 position)
        {
            Color frostColor = Main.rand.NextBool() ? WinterIce : FrostBlue;
            if (Main.rand.NextBool(3))
                frostColor = CrystalCyan;
            if (Main.rand.NextBool(5))
                frostColor = WinterWhite;
            
            // Frost drifts slowly downward
            Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), Main.rand.NextFloat(0.3f, 1.5f));
            CustomParticles.GenericGlow(position, vel, frostColor, 0.28f, 30, true);
        }
        
        private void SpawnIceBurst(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (speed * Main.rand.NextFloat(0.8f, 1.2f));
                
                Color iceColor = Color.Lerp(WinterIce, CrystalCyan, Main.rand.NextFloat());
                if (Main.rand.NextBool(3))
                    iceColor = FrostBlue;
                if (Main.rand.NextBool(4))
                    iceColor = WinterWhite;
                
                CustomParticles.GenericGlow(position, vel, iceColor, 0.35f, 32, true);
            }
        }
        
        private void SpawnIceProjectile(Vector2 position, Vector2 velocity, int damage, bool homing = false)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float homingStrength = homing ? 0.015f : 0f;
            Color projColor = Main.rand.NextBool() ? WinterIce : CrystalCyan;
            
            BossProjectileHelper.SpawnHostileOrb(position, velocity, damage, projColor, homingStrength);
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // Frost trail
            for (int i = 0; i < NPC.oldPos.Length - 1; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(WinterIce, CrystalCyan, progress) * (1f - progress) * 0.6f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                float trailScale = NPC.scale * (1f - progress * 0.25f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, NPC.rotation, origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers
            float pulse = (float)Math.Sin(Timer * 0.08f) * 0.12f + 1f;
            
            Color outerGlow = WinterIce * 0.35f;
            outerGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, NPC.rotation, origin, NPC.scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            Color midGlow = CrystalCyan * 0.45f;
            midGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, midGlow, NPC.rotation, origin, NPC.scale * pulse * 1.1f, SpriteEffects.None, 0f);
            
            // Main sprite
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot & Drops
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<WinterResonantEnergy>(), 1, 3, 5));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostEssence>(), 1, 5, 8));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ShardOfStillness>(), 1, 18, 28));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DormantWinterCore>(), 3));
        }
        
        public override void OnKill()
        {
            CustomParticles.GenericFlare(NPC.Center, WinterWhite, 2.7f, 48);
            for (int i = 0; i < 14; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(WinterIce, CrystalCyan, i / 14f), 0.5f + i * 0.15f, 20 + i * 3);
            }
            SpawnIceBurst(NPC.Center, 55, 15f);
        }
        
        public override bool CheckDead()
        {
            if (State != BossPhase.Dying)
            {
                State = BossPhase.Dying;
                deathTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                return false;
            }
            return true;
        }
        
        #endregion
    }
}
