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
using MagnumOpus.Content.Autumn.Materials;

namespace MagnumOpus.Content.Autumn.Bosses
{
    /// <summary>
    /// AUTUNNO, THE WITHERING MAESTRO - POST-PLANTERA BOSS
    /// 
    /// Design Philosophy:
    /// - Melancholic, withering elegance
    /// - Falling leaf and decay projectile patterns
    /// - Life-draining mechanics (debuffs, soul-stealing)
    /// - Musical connection: Vivaldi's Autumn from The Four Seasons
    /// 
    /// Theme Colors: White (#FFFFFF), Brown (#8B4513), Dark Orange (#FF4500)
    /// </summary>
    public class Autunno : ModNPC
    {
        #region Theme Colors
        private static readonly Color AutumnWhite = new Color(255, 255, 255);
        private static readonly Color AutumnBrown = new Color(139, 69, 19);
        private static readonly Color AutumnOrange = new Color(255, 69, 0);
        private static readonly Color FadingGold = new Color(218, 165, 32);
        private static readonly Color DecayPurple = new Color(128, 64, 96);
        private static readonly Color LeafRed = new Color(180, 50, 30);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 12f;
        private const int BaseDamage = 85;
        private const float EnrageDistance = 1900f;
        private const int AttackWindowFrames = 55;
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
            LeafStorm,         // Radial falling leaf burst
            WitheringWind,     // Wave of decay particles
            HarvestMoon,       // Overhead projectile rain
            
            // Phase 2 (60-30% HP)
            SoulReap,          // Dash attack with life drain effect
            DecayingVortex,    // Spiraling leaves with decay zones
            TwilightBarrage,   // Rapid targeted shots
            
            // Phase 3 (30-0% HP)
            AutumnalJudgment,  // Signature spectacle attack
            LastHarvest,       // Desperate multi-pattern assault
            WitheringFinale    // Ultimate decay explosion
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
        private AttackPattern lastAttack = AttackPattern.LeafStorm;
        private int consecutiveAttacks = 0;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 1600;
        
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
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Slow] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of sprite size (459x540)
            NPC.width = 367;
            NPC.height = 432;
            NPC.damage = BaseDamage;
            NPC.defense = 30; // Post-Wall of Flesh tier
            NPC.lifeMax = 32000; // Post-Wall of Flesh tier (comparable to Queen Slime 18k, pre-mech boss tier)
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath6;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.boss = true;
            NPC.npcSlots = 12f;
            NPC.aiStyle = -1;
            
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/TwilightOfTheFallingCrown");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Autunno, the Withering Maestro - the somber conductor of autumn's decline, whose melancholic symphony heralds the end of all things.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Autumn);
                hasRegisteredHealthBar = true;
            }
            
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y -= 0.4f;
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
            SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.2f }, NPC.Center);
            
            // Withering halos
            for (int i = 0; i < 10; i++)
            {
                float progress = i / 10f;
                Color haloColor = Color.Lerp(AutumnOrange, DecayPurple, progress);
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.45f + i * 0.14f, 18 + i * 3);
            }
            
            CustomParticles.GenericFlare(NPC.Center, AutumnWhite, 1.8f, 32);
            CustomParticles.GenericFlare(NPC.Center, FadingGold, 1.4f, 28);
            
            SpawnLeafBurst(NPC.Center, 30, 10f);
            MagnumScreenEffects.AddScreenShake(11f);
        }
        
        private void UpdateAggression()
        {
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.45f + difficultyTier * 0.11f;
        private float GetAggressionRateMult() => Math.Max(0.55f, 1f - aggressionLevel * 0.35f - difficultyTier * 0.09f);
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = 0.3f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, AutumnWhite, 2.2f, 42);
                
                for (int i = 0; i < 14; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(AutumnOrange, AutumnBrown, i / 14f), 0.35f + i * 0.11f, 15 + i * 2);
                }
                
                SpawnLeafBurst(NPC.Center, 40, 11f);
            }
            
            if (Timer >= 85)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Idle(Player target)
        {
            float hoverDist = 290f - difficultyTier * 28f;
            Vector2 idealPos = target.Center + new Vector2(NPC.Center.X > target.Center.X ? hoverDist : -hoverDist, -130f);
            
            // Gentle drifting motion like falling leaves
            idealPos.Y += (float)Math.Sin(Timer * 0.025f) * 35f;
            idealPos.X += (float)Math.Sin(Timer * 0.018f) * 20f;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            if (toIdeal.Length() > 38f)
            {
                toIdeal.Normalize();
                float speed = BaseSpeed * GetAggressionSpeedMult();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * speed, 0.055f);
            }
            else
            {
                NPC.velocity *= 0.91f;
            }
            
            int effectiveCooldown = (int)(attackCooldown * GetAggressionRateMult());
            if (effectiveCooldown <= 0 && Timer > (int)(38 * GetAggressionRateMult()))
            {
                SelectNextAttack(target);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.LeafStorm,
                AttackPattern.WitheringWind,
                AttackPattern.HarvestMoon
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.SoulReap);
                pool.Add(AttackPattern.DecayingVortex);
                pool.Add(AttackPattern.TwilightBarrage);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.AutumnalJudgment);
                pool.Add(AttackPattern.LastHarvest);
                
                if (consecutiveAttacks >= 5)
                    pool.Add(AttackPattern.WitheringFinale);
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
                case AttackPattern.LeafStorm:
                    Attack_LeafStorm(target);
                    break;
                case AttackPattern.WitheringWind:
                    Attack_WitheringWind(target);
                    break;
                case AttackPattern.HarvestMoon:
                    Attack_HarvestMoon(target);
                    break;
                case AttackPattern.SoulReap:
                    Attack_SoulReap(target);
                    break;
                case AttackPattern.DecayingVortex:
                    Attack_DecayingVortex(target);
                    break;
                case AttackPattern.TwilightBarrage:
                    Attack_TwilightBarrage(target);
                    break;
                case AttackPattern.AutumnalJudgment:
                    Attack_AutumnalJudgment(target);
                    break;
                case AttackPattern.LastHarvest:
                    Attack_LastHarvest(target);
                    break;
                case AttackPattern.WitheringFinale:
                    Attack_WitheringFinale(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            float idealDist = 330f;
            Vector2 toTarget = (target.Center - NPC.Center);
            float currentDist = toTarget.Length();
            float maxTime = 68f;
            float progress = Timer / maxTime;
            
            if (Math.Abs(currentDist - idealDist) < 75f && Timer > 26)
            {
                // VFX: Ready to attack cue - warns player aggression is returning
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, LeafRed);
                
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames / 2;
                return;
            }
            
            // Bell curve speed: accelerate then decelerate smoothly
            float speedMult = BossAIUtilities.Easing.EaseOutQuad(progress) * BossAIUtilities.Easing.EaseInQuad(1f - progress) * 4f;
            speedMult = Math.Max(speedMult, 0.15f);
            
            Vector2 idealDir = currentDist > idealDist ? -toTarget.SafeNormalize(Vector2.Zero) : toTarget.SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, idealDir * 13f * speedMult, 0.085f);
            
            // VFX: Recovery shimmer - player can identify vulnerability
            if (Timer % 8 == 0)
                BossVFXOptimizer.RecoveryShimmer(NPC.Center, FadingGold, 60f, progress);
            
            // VFX: Deceleration trail during slowdown phase
            if (progress > 0.5f && Timer % 4 == 0)
                BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, AutumnOrange, progress);
            
            if (Timer > maxTime)
            {
                // VFX: Ready to attack cue
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, LeafRed);
                
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            float enrageSpeed = BaseSpeed * 1.9f;
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * enrageSpeed, 0.11f);
            
            if (Timer % 11 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.09f;
                    Vector2 vel = angle.ToRotationVector2() * 9f;
                    SpawnLeafProjectile(NPC.Center, vel, 55);
                }
            }
            
            if (Timer % 4 == 0)
            {
                SpawnLeafParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f));
            }
        }
        
        private void AI_Dying(Player target)
        {
            deathTimer++;
            NPC.velocity *= 0.93f;
            
            if (deathTimer < 110)
            {
                float intensity = (float)deathTimer / 110f;
                
                if (deathTimer % 4 == 0)
                {
                    SpawnLeafBurst(NPC.Center, (int)(12 + intensity * 18), 5f + intensity * 7f);
                    
                    for (int i = 0; i < 7; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 7f + deathTimer * 0.045f;
                        Vector2 offset = angle.ToRotationVector2() * (35f + intensity * 55f);
                        Color flareColor = Color.Lerp(AutumnOrange, DecayPurple, (float)i / 7f);
                        CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.45f + intensity * 0.35f, 15);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(intensity * 5.5f);
            }
            else if (deathTimer == 110)
            {
                // Final withering explosion
                CustomParticles.GenericFlare(NPC.Center, AutumnWhite, 2.8f, 48);
                CustomParticles.GenericFlare(NPC.Center, FadingGold, 2.3f, 42);
                CustomParticles.GenericFlare(NPC.Center, DecayPurple, 1.8f, 36);
                
                for (int i = 0; i < 16; i++)
                {
                    Color ringColor = Color.Lerp(AutumnOrange, DecayPurple, i / 16f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, 0.5f + i * 0.17f, 21 + i * 3);
                }
                
                SpawnLeafBurst(NPC.Center, 55, 16f);
                MagnumScreenEffects.AddScreenShake(22f);
                
                NPC.life = 0;
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_LeafStorm(Player target)
        {
            int chargeTime = 48 - difficultyTier * 7;
            int projectileCount = 15 + difficultyTier * 5;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0)
            {
                float progress = (float)Timer / chargeTime;
                
                if (Timer % 3 == 0)
                {
                    int count = (int)(5 + progress * 8);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.045f;
                        float radius = 110f * (1f - progress * 0.4f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnLeafParticle(pos);
                    }
                }
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 95f, progress, AutumnOrange, 9);
                
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
                    SoundEngine.PlaySound(SoundID.Grass, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, AutumnOrange, FadingGold, 1.05f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float speed = 9f + difficultyTier * 2f;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnLeafProjectile(NPC.Center, vel, 55);
                        }
                    }
                    
                    SpawnLeafBurst(NPC.Center, 20, 9f);
                }
                
                if (Timer >= 38)
                    EndAttack();
            }
        }
        
        private void Attack_WitheringWind(Player target)
        {
            int waves = 4 + difficultyTier;
            int waveDelay = 23 - difficultyTier * 3;
            
            if (SubPhase < waves)
            {
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 5f, 0.04f);
                
                if (Timer == 9)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 420f, 9, WarningType.Danger);
                }
                
                if (Timer == 19 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Grass, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 6 + difficultyTier * 2;
                    float spread = MathHelper.ToRadians(48f);
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offsetAngle = MathHelper.Lerp(-spread, spread, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offsetAngle) * (11f + difficultyTier * 2f);
                        SpawnLeafProjectile(NPC.Center, vel, 55, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, FadingGold, 0.85f, 17);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 32)
            {
                EndAttack();
            }
        }
        
        private void Attack_HarvestMoon(Player target)
        {
            int duration = 125 + difficultyTier * 32;
            int fireInterval = 7 - difficultyTier;
            
            Vector2 hoverPos = target.Center + new Vector2(0, -420f);
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 50f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 11f * GetAggressionSpeedMult(), 0.055f);
            }
            
            if (Timer % 14 == 0)
            {
                for (int i = 0; i < 4 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-280f, 280f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -480f);
                    CustomParticles.GenericFlare(warningPos, AutumnOrange * 0.5f, 0.32f, 11);
                }
            }
            
            if (Timer % fireInterval == 0 && Timer > 28 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 3 + difficultyTier * 2;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-320f, 320f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -530f);
                    float ySpeed = 9f + difficultyTier * 2.5f;
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), ySpeed);
                    
                    SpawnLeafProjectile(spawnPos, vel, 55);
                    CustomParticles.GenericFlare(spawnPos, FadingGold, 0.32f, 9);
                }
            }
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_SoulReap(Player target)
        {
            int maxDashes = 3 + difficultyTier;
            int telegraphTime = 22 - difficultyTier * 3;
            int dashDuration = 11;
            int recoveryTime = 14 - difficultyTier * 2;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.88f;
                
                dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 480f, 10, WarningType.Danger);
                
                float progress = (float)Timer / telegraphTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 55f, progress, DecayPurple, 7);
                
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
                    SoundEngine.PlaySound(SoundID.NPCDeath6, NPC.Center);
                    NPC.velocity = dashDirection * (32f + difficultyTier * 7f);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, DecayPurple, AutumnOrange, 0.9f);
                }
                
                // Soul trail
                if (Timer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 perpendicular = new Vector2(-dashDirection.Y, dashDirection.X);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 spawnPos = NPC.Center + perpendicular * i * 18f;
                        Vector2 vel = -dashDirection * 2.5f + perpendicular * i * 1.5f;
                        SpawnLeafProjectile(spawnPos, vel, 50);
                    }
                }
                
                SpawnLeafParticle(NPC.Center);
                
                if (Timer >= dashDuration)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2) // Recovery
            {
                NPC.velocity *= 0.87f;
                
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
        
        private void Attack_DecayingVortex(Player target)
        {
            int duration = 145 + difficultyTier * 22;
            
            float spinSpeed = (0.028f + difficultyTier * 0.008f) * GetAggressionSpeedMult();
            float radius = 265f - aggressionLevel * 45f;
            float angle = Timer * spinSpeed;
            Vector2 idealPos = target.Center + angle.ToRotationVector2() * radius;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 15f, 0.085f);
            
            int fireInterval = Math.Max(4, 9 - difficultyTier * 2);
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float spiralAngle = Timer * 0.11f;
                int arms = 4 + difficultyTier;
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                    float speed = 8f + difficultyTier * 2f;
                    Vector2 vel = armAngle.ToRotationVector2() * speed;
                    SpawnLeafProjectile(NPC.Center, vel, 55, arm % 2 == 0);
                }
                
                CustomParticles.GenericFlare(NPC.Center, AutumnOrange, 0.38f, 11);
            }
            
            if (Timer % 7 == 0)
                SpawnLeafParticle(NPC.Center + Main.rand.NextVector2Circular(38f, 38f));
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_TwilightBarrage(Player target)
        {
            int burstCount = 5 + difficultyTier * 2;
            int burstDelay = 17 - difficultyTier * 3;
            
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 6f, 0.045f);
            
            if (SubPhase < burstCount)
            {
                if (Timer == 9)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 340f, 7, WarningType.Caution);
                }
                
                if (Timer == burstDelay && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 3 + difficultyTier;
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offset = MathHelper.Lerp(-0.18f, 0.18f, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offset) * (13f + difficultyTier * 2.5f);
                        SpawnLeafProjectile(NPC.Center, vel, 55, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, DecayPurple, 0.6f, 13);
                }
                
                if (Timer >= burstDelay + 4)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 32)
            {
                EndAttack();
            }
        }
        
        private void Attack_AutumnalJudgment(Player target)
        {
            // SIGNATURE SPECTACLE ATTACK
            int chargeTime = 75 - difficultyTier * 10;
            int waveCount = 4 + difficultyTier;
            
            if (SubPhase == 0) // Charge with safe zone
            {
                NPC.velocity *= 0.94f;
                
                float progress = (float)Timer / chargeTime;
                
                // Converging withered leaves
                if (Timer % 3 == 0)
                {
                    int particleCount = (int)(9 + progress * 14);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.055f;
                        float radius = 190f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnLeafParticle(pos);
                    }
                }
                
                // Safe zone indicator
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 85f, 12);
                    
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(48f), 155f, 7);
                }
                
                if (Timer > chargeTime * 0.7f)
                    MagnumScreenEffects.AddScreenShake(progress * 4.5f);
                
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
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.35f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(14f);
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, AutumnWhite, AutumnOrange, 1.25f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 26 + difficultyTier * 7;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(23f - difficultyTier * 3f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 10.5f + difficultyTier * 2.5f + SubPhase;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnLeafProjectile(NPC.Center, vel, 60, i % 4 == 0);
                        }
                    }
                    
                    // Cascading decay halos
                    for (int i = 0; i < 11; i++)
                    {
                        Color ringColor = Color.Lerp(AutumnOrange, DecayPurple, i / 11f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.48f + i * 0.13f, 19 + i * 3);
                    }
                    
                    SpawnLeafBurst(NPC.Center, 23, 11f);
                }
                
                if (Timer >= 32)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 48)
            {
                EndAttack();
            }
        }
        
        private void Attack_LastHarvest(Player target)
        {
            int duration = 155 + difficultyTier * 25;
            
            // Erratic movement
            float spinSpeed = (0.032f + difficultyTier * 0.01f) * GetAggressionSpeedMult();
            float radius = 240f - aggressionLevel * 50f;
            float angle = Timer * spinSpeed;
            Vector2 idealPos = target.Center + angle.ToRotationVector2() * radius;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 16f, 0.095f);
            
            int fireInterval = Math.Max(4, 8 - difficultyTier * 2);
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int pattern = (Timer / fireInterval) % 3;
                
                if (pattern == 0) // Spiral
                {
                    float spiralAngle = Timer * 0.14f;
                    int arms = 5 + difficultyTier;
                    for (int arm = 0; arm < arms; arm++)
                    {
                        float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                        Vector2 vel = armAngle.ToRotationVector2() * (9f + difficultyTier * 2f);
                        SpawnLeafProjectile(NPC.Center, vel, 55);
                    }
                }
                else if (pattern == 1) // Targeted
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = direction.RotatedBy(i * 0.18f) * (12f + difficultyTier * 2f);
                        SpawnLeafProjectile(NPC.Center, vel, 55, true);
                    }
                }
                else // Random scatter
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = randomAngle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                        SpawnLeafProjectile(NPC.Center, vel, 50);
                    }
                }
                
                CustomParticles.GenericFlare(NPC.Center, AutumnOrange, 0.42f, 11);
            }
            
            if (Timer % 6 == 0)
                SpawnLeafParticle(NPC.Center + Main.rand.NextVector2Circular(42f, 42f));
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_WitheringFinale(Player target)
        {
            // Ultimate decay explosion
            int chargeTime = 75 - difficultyTier * 10;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.93f;
                
                float progress = (float)Timer / chargeTime;
                
                // Intense converging decay energy
                if (Timer % 2 == 0)
                {
                    int count = (int)(14 + progress * 22);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.07f;
                        float radius = 240f * (1f - progress * 0.7f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color flareColor = Color.Lerp(AutumnOrange, DecayPurple, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(pos, flareColor, 0.32f + progress * 0.32f, 12);
                    }
                }
                
                // Growing glow
                CustomParticles.GenericFlare(NPC.Center, Color.Lerp(FadingGold, AutumnWhite, progress), 0.55f + progress * 1.4f, 9);
                
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 280f, 18);
                
                if (Timer > chargeTime * 0.6f)
                    MagnumScreenEffects.AddScreenShake(progress * 6.5f);
                
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
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.45f, Pitch = -0.25f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(18f);
                    
                    // Massive withering explosion VFX
                    CustomParticles.GenericFlare(NPC.Center, AutumnWhite, 2.8f, 42);
                    CustomParticles.GenericFlare(NPC.Center, FadingGold, 2.4f, 38);
                    CustomParticles.GenericFlare(NPC.Center, DecayPurple, 1.9f, 32);
                    
                    for (int i = 0; i < 18; i++)
                    {
                        Color ringColor = Color.Lerp(FadingGold, DecayPurple, i / 18f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.55f + i * 0.19f, 21 + i * 3);
                    }
                    
                    SpawnLeafBurst(NPC.Center, 48, 17f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Massive projectile burst
                        int count = 38 + difficultyTier * 11;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            float speed = 8f + Main.rand.NextFloat(0, 5.5f);
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnLeafProjectile(NPC.Center, vel, 60, i % 3 == 0);
                        }
                    }
                }
                
                if (Timer >= 55)
                    EndAttack();
            }
        }
        
        private void EndAttack()
        {
            // VFX: Attack ending cue - exhale burst with safety ring
            BossVFXOptimizer.AttackEndCue(NPC.Center, AutumnOrange, FadingGold);
            
            State = BossPhase.Reposition;
            Timer = 0;
            SubPhase = 0;
            attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
        }
        
        #endregion
        
        #region VFX Helpers
        
        private void SpawnAmbientParticles()
        {
            // Drifting leaves around boss
            if (Timer % 7 == 0)
            {
                float baseAngle = Timer * 0.022f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 52f + (float)Math.Sin(Timer * 0.055f + i) * 16f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    SpawnLeafParticle(pos);
                }
            }
            
            // Decay glow
            if (Timer % 11 == 0)
            {
                Color glowColor = Color.Lerp(AutumnOrange, FadingGold, (float)Math.Sin(Timer * 0.025f) * 0.5f + 0.5f);
                CustomParticles.GenericFlare(NPC.Center, glowColor * 0.55f, 0.38f, 15);
            }
            
            Lighting.AddLight(NPC.Center, AutumnOrange.ToVector3() * 0.55f);
        }
        
        private void SpawnLeafParticle(Vector2 position)
        {
            Color leafColor = Main.rand.NextBool() ? AutumnOrange : FadingGold;
            if (Main.rand.NextBool(3))
                leafColor = LeafRed;
            if (Main.rand.NextBool(5))
                leafColor = AutumnBrown;
            
            // Leaves drift down and sideways
            Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(0.5f, 2f));
            CustomParticles.GenericGlow(position, vel, leafColor, 0.27f, 28, true);
        }
        
        private void SpawnLeafBurst(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.22f, 0.22f);
                Vector2 vel = angle.ToRotationVector2() * (speed * Main.rand.NextFloat(0.8f, 1.2f));
                
                Color leafColor = Color.Lerp(AutumnOrange, FadingGold, Main.rand.NextFloat());
                if (Main.rand.NextBool(3))
                    leafColor = LeafRed;
                if (Main.rand.NextBool(4))
                    leafColor = AutumnBrown;
                
                CustomParticles.GenericGlow(position, vel, leafColor, 0.33f, 30, true);
            }
        }
        
        private void SpawnLeafProjectile(Vector2 position, Vector2 velocity, int damage, bool homing = false)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float homingStrength = homing ? 0.014f : 0f;
            Color projColor = Main.rand.NextBool() ? AutumnOrange : FadingGold;
            
            BossProjectileHelper.SpawnHostileOrb(position, velocity, damage, projColor, homingStrength);
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // Withering trail
            for (int i = 0; i < NPC.oldPos.Length - 1; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(AutumnOrange, DecayPurple, progress) * (1f - progress) * 0.55f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                float trailScale = NPC.scale * (1f - progress * 0.28f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, NPC.rotation, origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers
            float pulse = (float)Math.Sin(Timer * 0.085f) * 0.11f + 1f;
            
            Color outerGlow = AutumnOrange * 0.32f;
            outerGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, NPC.rotation, origin, NPC.scale * pulse * 1.18f, SpriteEffects.None, 0f);
            
            Color midGlow = FadingGold * 0.42f;
            midGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, midGlow, NPC.rotation, origin, NPC.scale * pulse * 1.09f, SpriteEffects.None, 0f);
            
            // Main sprite
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot & Drops
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<AutumnResonantEnergy>(), 1, 3, 5));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LeafOfEnding>(), 1, 18, 28));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DormantAutumnCore>(), 3));
        }
        
        public override void OnKill()
        {
            CustomParticles.GenericFlare(NPC.Center, AutumnWhite, 2.5f, 45);
            for (int i = 0; i < 13; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(AutumnOrange, DecayPurple, i / 13f), 0.48f + i * 0.14f, 19 + i * 3);
            }
            SpawnLeafBurst(NPC.Center, 52, 14f);
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
