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
using MagnumOpus.Content.Summer.Materials;

namespace MagnumOpus.Content.Summer.Bosses
{
    /// <summary>
    /// L'ESTATE, LORD OF THE ZENITH - POST-MECHANICAL BOSSES
    /// 
    /// Design Philosophy:
    /// - Blazing intensity, relentless heat
    /// - Solar flare and sunfire projectile patterns
    /// - Heat-based mechanics (damage over time zones)
    /// - Musical connection: Vivaldi's Summer from The Four Seasons
    /// 
    /// Theme Colors: Orange (#FF8C00), White (#FFFFFF)
    /// </summary>
    public class LEstate : ModNPC
    {
        #region Theme Colors
        private static readonly Color SummerOrange = new Color(255, 140, 0);
        private static readonly Color SummerWhite = new Color(255, 255, 255);
        private static readonly Color SolarGold = new Color(255, 200, 50);
        private static readonly Color FlameRed = new Color(255, 80, 30);
        private static readonly Color HeatYellow = new Color(255, 230, 100);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 14f;
        private const int BaseDamage = 75;
        private const float EnrageDistance = 1800f;
        private const int AttackWindowFrames = 50;
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
            SolarFlare,        // Radial sunfire burst
            HeatWave,          // Line wave attack
            SunshowerBombard,  // Rain of fire from above
            
            // Phase 2 (60-30% HP)
            ScorchingDash,     // Fast fire dash with trail
            ZenithBeam,        // Solar beam attack
            InfernoRing,       // Expanding fire ring
            
            // Phase 3 (30-0% HP)
            SummerSolstice,    // Signature spectacle attack
            SolarStorm,        // Chaotic multi-pattern assault
            Supernova          // Desperate explosion attack
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
        private AttackPattern lastAttack = AttackPattern.SolarFlare;
        private int consecutiveAttacks = 0;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 1500;
        
        private Vector2 dashDirection;
        private int dashCount = 0;
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        
        // Ground-based movement
        private int jumpCooldown = 0;
        private bool isGrounded = false;
        private const float JumpVelocity = -15f;
        private const float HighJumpVelocity = -20f;
        private const float MoveSpeed = 10f;
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Burning] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of sprite size (540x511)
            NPC.width = 432;
            NPC.height = 408;
            NPC.damage = BaseDamage;
            NPC.defense = 25; // Post-Skeletron tier
            NPC.lifeMax = 15000; // Post-Skeletron tier (comparable to Queen Bee 3.4k Classic, WoF 8k Classic)
            NPC.HitSound = SoundID.NPCHit3;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;  // Ground-based boss
            NPC.noTileCollide = false;  // Respects terrain
            NPC.value = Item.buyPrice(gold: 12);
            NPC.boss = true;
            NPC.npcSlots = 12f;
            NPC.aiStyle = -1;
            
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/ScorchOfTheFinalSun");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Desert,
                new FlavorTextBestiaryInfoElement("L'Estate, Lord of the Zenith - the blazing monarch of summer's peak, whose radiance scorches all who dare approach.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Summer);
                hasRegisteredHealthBar = true;
            }
            
            Player target = Main.player[NPC.target];
            if (!target.active || target.dead)
            {
                NPC.TargetClosest(true);
                target = Main.player[NPC.target];
                if (!target.active || target.dead)
                {
                    NPC.velocity.Y -= 0.5f;
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
            SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
            
            // Solar flare halos
            for (int i = 0; i < 10; i++)
            {
                float progress = i / 10f;
                Color haloColor = Color.Lerp(SummerOrange, FlameRed, progress);
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.5f + i * 0.15f, 18 + i * 3);
            }
            
            CustomParticles.GenericFlare(NPC.Center, SummerWhite, 2f, 35);
            CustomParticles.GenericFlare(NPC.Center, SolarGold, 1.6f, 30);
            
            SpawnSolarBurst(NPC.Center, 25, 10f);
            MagnumScreenEffects.AddScreenShake(12f);
        }
        
        private void UpdateAggression()
        {
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.5f + difficultyTier * 0.12f;
        private float GetAggressionRateMult() => Math.Max(0.55f, 1f - aggressionLevel * 0.35f - difficultyTier * 0.1f);
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, SummerWhite, 2.5f, 45);
                
                for (int i = 0; i < 15; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(SummerOrange, SolarGold, i / 15f), 0.4f + i * 0.12f, 15 + i * 2);
                }
                
                SpawnSolarBurst(NPC.Center, 35, 12f);
            }
            
            if (Timer >= 80)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Idle(Player target)
        {
            // Ground-based movement - check if grounded
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            // Horizontal movement toward player
            float idealDist = 180f - difficultyTier * 15f;
            float distX = target.Center.X - NPC.Center.X;
            float absDistX = Math.Abs(distX);
            
            if (absDistX > idealDist)
            {
                // Move toward player
                float dir = Math.Sign(distX);
                float speed = MoveSpeed * GetAggressionSpeedMult();
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * speed, 0.1f);
            }
            else if (absDistX < idealDist * 0.4f)
            {
                // Too close, back away
                float dir = -Math.Sign(distX);
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * MoveSpeed * 0.6f, 0.08f);
            }
            else
            {
                // Ideal range, slow down
                NPC.velocity.X *= 0.88f;
            }
            
            // Jumping logic - jump to reach player or over obstacles
            if (isGrounded && jumpCooldown <= 0)
            {
                bool shouldJump = false;
                bool highJump = false;
                
                // Jump if player is significantly above
                float yDiff = NPC.Center.Y - target.Center.Y;
                if (yDiff > 80f)
                {
                    shouldJump = true;
                    highJump = yDiff > 200f;
                }
                
                // Jump over obstacles (simple check)
                if (Math.Abs(NPC.velocity.X) < 1f && absDistX > 50f)
                {
                    shouldJump = true;
                }
                
                // Random hop for variety - Summer boss is more aggressive
                if (!shouldJump && Main.rand.NextBool(80))
                {
                    shouldJump = true;
                }
                
                if (shouldJump)
                {
                    NPC.velocity.Y = highJump ? HighJumpVelocity : JumpVelocity;
                    jumpCooldown = 35 + Main.rand.Next(15);
                    
                    // Jump VFX - solar flare
                    SpawnSolarBurst(NPC.Bottom, 8, 5f);
                    SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.2f, Volume = 0.6f }, NPC.Center);
                }
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
                AttackPattern.SolarFlare,
                AttackPattern.HeatWave,
                AttackPattern.SunshowerBombard
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.ScorchingDash);
                pool.Add(AttackPattern.ZenithBeam);
                pool.Add(AttackPattern.InfernoRing);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.SummerSolstice);
                pool.Add(AttackPattern.SolarStorm);
                
                if (consecutiveAttacks >= 5)
                    pool.Add(AttackPattern.Supernova);
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
                case AttackPattern.SolarFlare:
                    Attack_SolarFlare(target);
                    break;
                case AttackPattern.HeatWave:
                    Attack_HeatWave(target);
                    break;
                case AttackPattern.SunshowerBombard:
                    Attack_SunshowerBombard(target);
                    break;
                case AttackPattern.ScorchingDash:
                    Attack_ScorchingDash(target);
                    break;
                case AttackPattern.ZenithBeam:
                    Attack_ZenithBeam(target);
                    break;
                case AttackPattern.InfernoRing:
                    Attack_InfernoRing(target);
                    break;
                case AttackPattern.SummerSolstice:
                    Attack_SummerSolstice(target);
                    break;
                case AttackPattern.SolarStorm:
                    Attack_SolarStorm(target);
                    break;
                case AttackPattern.Supernova:
                    Attack_Supernova(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            // Ground-based repositioning
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            float duration = 65f;
            float progress = Timer / duration;
            
            float idealDist = 220f;
            float distX = target.Center.X - NPC.Center.X;
            float absDistX = Math.Abs(distX);
            
            if (Math.Abs(absDistX - idealDist) < 90f && Timer > 25)
            {
                // Ready to attack again
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, FlameRed, 0.7f);
                
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames / 2;
                return;
            }
            
            // Move toward or away from player with smooth easing
            float dir = absDistX > idealDist ? Math.Sign(distX) : -Math.Sign(distX);
            float speedCurve = BossAIUtilities.Easing.EaseInOutQuad(Math.Min(1f, progress * 2f));
            float speed = MoveSpeed * 1.3f * Math.Max(0.4f, speedCurve);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * speed, 0.12f);
            
            // Jump if needed
            if (isGrounded && jumpCooldown <= 0 && (target.Center.Y < NPC.Center.Y - 60f || Math.Abs(NPC.velocity.X) < 1f))
            {
                NPC.velocity.Y = JumpVelocity;
                jumpCooldown = 25;
                SpawnSolarBurst(NPC.Bottom, 6, 4f);
            }
            
            // Recovery shimmer - vulnerability indicator
            if (Timer % 4 == 0)
            {
                float shimmerProgress = Timer / 65f;
                BossVFXOptimizer.RecoveryShimmer(NPC.Center, SolarGold, 55f, shimmerProgress);
            }
            
            // Deceleration trail while moving (fiery)
            if (Math.Abs(NPC.velocity.X) > 2f)
            {
                float trailProgress = Timer / 65f;
                BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, SummerOrange, trailProgress);
            }
            
            if (Timer > 65)
            {
                // Ready to attack again
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, FlameRed, 0.7f);
                
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Aggressive ground pursuit
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            float enrageSpeed = MoveSpeed * 2f;
            float dir = Math.Sign(target.Center.X - NPC.Center.X);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * enrageSpeed, 0.15f);
            
            // Very aggressive jumping
            if (isGrounded && jumpCooldown <= 0)
            {
                NPC.velocity.Y = HighJumpVelocity;
                jumpCooldown = 20;
                SpawnSolarBurst(NPC.Bottom, 10, 8f);
            }
            
            // Solar flare storm while chasing
            if (Timer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.1f;
                    Vector2 vel = angle.ToRotationVector2() * 10f;
                    SpawnFireProjectile(NPC.Center, vel, 50);
                }
            }
            
            if (Timer % 3 == 0)
            {
                SpawnFlameParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f));
            }
            
            // === PHASE 10 MUSICAL VFX: Beat Synced Rhythm - Summer's Blazing Tempo ===
            if (Timer % 24 == 0)
            {
                Phase10Integration.Universal.BeatSyncedRhythm(NPC.Center, SummerOrange, 150f, Timer);
            }
        }
        
        private void AI_Dying(Player target)
        {
            deathTimer++;
            NPC.velocity *= 0.94f;
            
            if (deathTimer < 100)
            {
                float intensity = (float)deathTimer / 100f;
                
                if (deathTimer % 4 == 0)
                {
                    SpawnSolarBurst(NPC.Center, (int)(10 + intensity * 15), 6f + intensity * 6f);
                    
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + deathTimer * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * (40f + intensity * 60f);
                        Color flareColor = Color.Lerp(SummerOrange, FlameRed, (float)i / 8f);
                        CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.5f + intensity * 0.4f, 15);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(intensity * 6f);
            }
            else if (deathTimer == 100)
            {
                // Final explosion
                CustomParticles.GenericFlare(NPC.Center, SummerWhite, 3f, 50);
                CustomParticles.GenericFlare(NPC.Center, SolarGold, 2.5f, 45);
                CustomParticles.GenericFlare(NPC.Center, SummerOrange, 2f, 40);
                
                for (int i = 0; i < 18; i++)
                {
                    Color ringColor = Color.Lerp(SummerOrange, FlameRed, i / 18f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, 0.5f + i * 0.18f, 22 + i * 3);
                }
                
                SpawnSolarBurst(NPC.Center, 60, 18f);
                MagnumScreenEffects.AddScreenShake(25f);
                
                // === PHASE 10 MUSICAL VFX: Death Finale - Summer's Solar Symphony Ends ===
                Phase10Integration.Universal.DeathFinale(NPC.Center, SummerWhite, SolarGold);
                
                NPC.life = 0;
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_SolarFlare(Player target)
        {
            int chargeTime = 45 - difficultyTier * 7;
            int projectileCount = 14 + difficultyTier * 5;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0)
            {
                float progress = (float)Timer / chargeTime;
                
                if (Timer % 3 == 0)
                {
                    int count = (int)(5 + progress * 7);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.05f;
                        float radius = 100f * (1f - progress * 0.4f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnFlameParticle(pos);
                    }
                }
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 90f, progress, SummerOrange, 10);
                
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
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, SummerOrange, SolarGold, 1.1f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float speed = 10f + difficultyTier * 2f;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnFireProjectile(NPC.Center, vel, 50);
                        }
                    }
                    
                    SpawnSolarBurst(NPC.Center, 18, 9f);
                }
                
                if (Timer >= 35)
                    EndAttack();
            }
        }
        
        private void Attack_HeatWave(Player target)
        {
            int waves = 4 + difficultyTier;
            int waveDelay = 22 - difficultyTier * 3;
            
            if (SubPhase < waves)
            {
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 5f, 0.04f);
                
                if (Timer == 8)
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 450f, 10, WarningType.Danger);
                }
                
                if (Timer == 18 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item45, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 6 + difficultyTier * 2;
                    float spread = MathHelper.ToRadians(50f);
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offsetAngle = MathHelper.Lerp(-spread, spread, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offsetAngle) * (12f + difficultyTier * 2f);
                        SpawnFireProjectile(NPC.Center, vel, 50, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, SolarGold, 0.9f, 18);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 30)
            {
                EndAttack();
            }
        }
        
        private void Attack_SunshowerBombard(Player target)
        {
            int duration = 130 + difficultyTier * 35;
            int fireInterval = 6 - difficultyTier;
            
            // Ground-based: stand still and summon fire from above
            NPC.velocity.X *= 0.9f;
            
            // Check for grounded and occasional jump
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            if (isGrounded && Timer % 60 == 0 && Timer > 0)
            {
                NPC.velocity.Y = JumpVelocity * 0.6f;
                SpawnSolarBurst(NPC.Bottom, 5, 4f);
            }
            
            if (Timer % 12 == 0)
            {
                for (int i = 0; i < 4 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-300f, 300f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -500f);
                    CustomParticles.GenericFlare(warningPos, SummerOrange * 0.5f, 0.3f, 10);
                }
            }
            
            if (Timer % fireInterval == 0 && Timer > 25 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 3 + difficultyTier * 2;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-350f, 350f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -550f);
                    float ySpeed = 10f + difficultyTier * 2.5f;
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), ySpeed);
                    
                    SpawnFireProjectile(spawnPos, vel, 50);
                    CustomParticles.GenericFlare(spawnPos, SolarGold, 0.35f, 8);
                }
            }
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_ScorchingDash(Player target)
        {
            int maxDashes = 3 + difficultyTier;
            int telegraphTime = 20 - difficultyTier * 3;
            int dashDuration = 10;
            int recoveryTime = 12 - difficultyTier * 2;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.9f;
                
                dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 500f, 10, WarningType.Danger);
                
                float progress = (float)Timer / telegraphTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 50f, progress, FlameRed, 6);
                
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
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                    NPC.velocity = dashDirection * (35f + difficultyTier * 8f);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, FlameRed, SummerOrange, 0.9f);
                }
                
                // Fire trail
                if (Timer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 perpendicular = new Vector2(-dashDirection.Y, dashDirection.X);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 spawnPos = NPC.Center + perpendicular * i * 20f;
                        Vector2 vel = -dashDirection * 3f + perpendicular * i * 2f;
                        SpawnFireProjectile(spawnPos, vel, 45);
                    }
                }
                
                SpawnFlameParticle(NPC.Center);
                
                if (Timer >= dashDuration)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2) // Recovery
            {
                NPC.velocity *= 0.88f;
                
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
        
        private void Attack_ZenithBeam(Player target)
        {
            int chargeTime = 50 - difficultyTier * 8;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0) // Charge
            {
                float progress = (float)Timer / chargeTime;
                
                Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                BossVFXOptimizer.LaserBeamWarning(NPC.Center, direction.ToRotation(), 600f, progress);
                
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 particlePos = NPC.Center + direction * (50f + i * 30f);
                        CustomParticles.GenericFlare(particlePos, Color.Lerp(SolarGold, SummerWhite, progress), 0.3f + progress * 0.3f, 10);
                    }
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Fire
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Dense line of projectiles (simulates beam)
                        int count = 15 + difficultyTier * 5;
                        for (int i = 0; i < count; i++)
                        {
                            float speed = 15f + i * 1.5f;
                            Vector2 vel = direction * speed;
                            SpawnFireProjectile(NPC.Center, vel, 55, true);
                        }
                    }
                    
                    // Beam VFX
                    CustomParticles.GenericFlare(NPC.Center, SummerWhite, 1.5f, 25);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 beamPos = NPC.Center + direction * (80f + i * 60f);
                        CustomParticles.GenericFlare(beamPos, SolarGold, 0.6f - i * 0.05f, 15);
                    }
                    
                    MagnumScreenEffects.AddScreenShake(10f);
                }
                
                if (Timer >= 40)
                    EndAttack();
            }
        }
        
        private void Attack_InfernoRing(Player target)
        {
            int rings = 3 + difficultyTier;
            int ringDelay = 30 - difficultyTier * 4;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase < rings)
            {
                if (Timer == 10)
                {
                    BossVFXOptimizer.DangerZoneRing(NPC.Center, 150f + SubPhase * 80f, 16);
                }
                
                if (Timer == ringDelay && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item74, NPC.Center);
                    
                    int count = 16 + SubPhase * 4 + difficultyTier * 4;
                    float speed = 6f + SubPhase * 1.5f + difficultyTier * 1f;
                    
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + SubPhase * 0.3f;
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        SpawnFireProjectile(NPC.Center, vel, 50);
                    }
                    
                    CustomParticles.HaloRing(NPC.Center, SummerOrange, 0.6f + SubPhase * 0.2f, 20);
                    CustomParticles.GenericFlare(NPC.Center, SolarGold, 0.8f, 15);
                }
                
                if (Timer >= ringDelay + 5)
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
        
        private void Attack_SummerSolstice(Player target)
        {
            // SIGNATURE SPECTACLE ATTACK
            int chargeTime = 70 - difficultyTier * 10;
            int waveCount = 4 + difficultyTier;
            
            if (SubPhase == 0) // Charge with safe zone
            {
                NPC.velocity *= 0.94f;
                
                float progress = (float)Timer / chargeTime;
                
                // Converging solar flares
                if (Timer % 3 == 0)
                {
                    int particleCount = (int)(10 + progress * 15);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.06f;
                        float radius = 200f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnFlameParticle(pos);
                    }
                }
                
                // Safe zone indicator
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 80f, 12);
                    
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(45f), 160f, 8);
                }
                
                if (Timer > chargeTime * 0.7f)
                    MagnumScreenEffects.AddScreenShake(progress * 5f);
                
                // === PHASE 10 MUSICAL VFX: Crescendo Charge Up - Summer Solstice Building ===
                Phase10Integration.Universal.CrescendoChargeUp(NPC.Center, SolarGold, progress);
                
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
                    MagnumScreenEffects.AddScreenShake(15f);
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, SummerWhite, SummerOrange, 1.3f);
                    
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
                            SpawnFireProjectile(NPC.Center, vel, 55, i % 4 == 0);
                        }
                    }
                    
                    // Cascading solar halos
                    for (int i = 0; i < 12; i++)
                    {
                        Color ringColor = Color.Lerp(SummerOrange, FlameRed, i / 12f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.5f + i * 0.14f, 20 + i * 3);
                    }
                    
                    SpawnSolarBurst(NPC.Center, 25, 12f);
                }
                
                if (Timer >= 30)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (Timer >= 45)
            {
                EndAttack();
            }
        }
        
        private void Attack_SolarStorm(Player target)
        {
            int duration = 160 + difficultyTier * 25;
            
            // Chaotic movement
            float spinSpeed = (0.03f + difficultyTier * 0.01f) * GetAggressionSpeedMult();
            float radius = 250f - aggressionLevel * 50f;
            float angle = Timer * spinSpeed;
            Vector2 idealPos = target.Center + angle.ToRotationVector2() * radius;
            
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 16f, 0.09f);
            
            // Multi-pattern fire
            int fireInterval = Math.Max(3, 8 - difficultyTier * 2);
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Pattern alternates
                int pattern = (Timer / fireInterval) % 3;
                
                if (pattern == 0) // Spiral
                {
                    float spiralAngle = Timer * 0.15f;
                    int arms = 4 + difficultyTier;
                    for (int arm = 0; arm < arms; arm++)
                    {
                        float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                        Vector2 vel = armAngle.ToRotationVector2() * (9f + difficultyTier * 2f);
                        SpawnFireProjectile(NPC.Center, vel, 50);
                    }
                }
                else if (pattern == 1) // Targeted
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = direction.RotatedBy(i * 0.2f) * (13f + difficultyTier * 2f);
                        SpawnFireProjectile(NPC.Center, vel, 50, true);
                    }
                }
                else // Random burst
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float randomAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = randomAngle.ToRotationVector2() * Main.rand.NextFloat(7f, 12f);
                        SpawnFireProjectile(NPC.Center, vel, 45);
                    }
                }
                
                CustomParticles.GenericFlare(NPC.Center, SummerOrange, 0.4f, 10);
            }
            
            if (Timer % 5 == 0)
                SpawnFlameParticle(NPC.Center + Main.rand.NextVector2Circular(40f, 40f));
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_Supernova(Player target)
        {
            // Desperate explosion attack
            int chargeTime = 80 - difficultyTier * 10;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.94f;
                
                float progress = (float)Timer / chargeTime;
                
                // Intense converging energy
                if (Timer % 2 == 0)
                {
                    int count = (int)(12 + progress * 20);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.08f;
                        float radius = 250f * (1f - progress * 0.7f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color flareColor = Color.Lerp(SummerOrange, FlameRed, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(pos, flareColor, 0.3f + progress * 0.3f, 12);
                    }
                }
                
                // Growing glow
                CustomParticles.GenericFlare(NPC.Center, Color.Lerp(SolarGold, SummerWhite, progress), 0.5f + progress * 1.5f, 8);
                
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 300f, 20);
                
                if (Timer > chargeTime * 0.6f)
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
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f, Pitch = -0.3f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(20f);
                    
                    // Massive explosion VFX
                    CustomParticles.GenericFlare(NPC.Center, SummerWhite, 3f, 40);
                    CustomParticles.GenericFlare(NPC.Center, SolarGold, 2.5f, 35);
                    CustomParticles.GenericFlare(NPC.Center, SummerOrange, 2f, 30);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        Color ringColor = Color.Lerp(SolarGold, FlameRed, i / 20f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.6f + i * 0.2f, 22 + i * 3);
                    }
                    
                    SpawnSolarBurst(NPC.Center, 50, 18f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Massive projectile burst
                        int count = 40 + difficultyTier * 12;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            float speed = 8f + Main.rand.NextFloat(0, 6f);
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnFireProjectile(NPC.Center, vel, 55, i % 3 == 0);
                        }
                    }
                }
                
                if (Timer >= 60)
                    EndAttack();
            }
        }
        
        private void EndAttack()
        {
            // Visual cue: Attack ending - player has a window
            BossVFXOptimizer.AttackEndCue(NPC.Center, SummerOrange, SolarGold, 0.8f);
            
            State = BossPhase.Reposition;
            Timer = 0;
            SubPhase = 0;
            attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
        }
        
        #endregion
        
        #region VFX Helpers
        
        private void SpawnAmbientParticles()
        {
            // Solar corona
            if (Timer % 6 == 0)
            {
                float baseAngle = Timer * 0.025f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 55f + (float)Math.Sin(Timer * 0.06f + i) * 15f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    SpawnFlameParticle(pos);
                }
            }
            
            // Central glow
            if (Timer % 10 == 0)
            {
                Color glowColor = Color.Lerp(SummerOrange, SolarGold, (float)Math.Sin(Timer * 0.03f) * 0.5f + 0.5f);
                CustomParticles.GenericFlare(NPC.Center, glowColor * 0.6f, 0.4f, 15);
            }
            
            Lighting.AddLight(NPC.Center, SummerOrange.ToVector3() * 0.7f);
        }
        
        private void SpawnFlameParticle(Vector2 position)
        {
            Color flameColor = Main.rand.NextBool() ? SummerOrange : SolarGold;
            if (Main.rand.NextBool(3))
                flameColor = FlameRed;
            
            Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -1f);
            CustomParticles.GenericGlow(position, vel, flameColor, 0.28f, 22, true);
        }
        
        private void SpawnSolarBurst(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (speed * Main.rand.NextFloat(0.8f, 1.2f));
                
                Color flameColor = Color.Lerp(SummerOrange, SolarGold, Main.rand.NextFloat());
                if (Main.rand.NextBool(3))
                    flameColor = FlameRed;
                
                CustomParticles.GenericGlow(position, vel, flameColor, 0.35f, 28, true);
            }
        }
        
        private void SpawnFireProjectile(Vector2 position, Vector2 velocity, int damage, bool homing = false)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float homingStrength = homing ? 0.012f : 0f;
            Color projColor = Main.rand.NextBool() ? SummerOrange : SolarGold;
            
            BossProjectileHelper.SpawnHostileOrb(position, velocity, damage, projColor, homingStrength);
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // Heat trail
            for (int i = 0; i < NPC.oldPos.Length - 1; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(SummerOrange, FlameRed, progress) * (1f - progress) * 0.6f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                float trailScale = NPC.scale * (1f - progress * 0.25f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, NPC.rotation, origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers
            float pulse = (float)Math.Sin(Timer * 0.1f) * 0.12f + 1f;
            
            Color outerGlow = SummerOrange * 0.35f;
            outerGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, NPC.rotation, origin, NPC.scale * pulse * 1.2f, SpriteEffects.None, 0f);
            
            Color midGlow = SolarGold * 0.45f;
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
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SummerResonantEnergy>(), 1, 3, 5));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<EmberOfIntensity>(), 1, 18, 28));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DormantSummerCore>(), 3));
        }
        
        public override void OnKill()
        {
            CustomParticles.GenericFlare(NPC.Center, SummerWhite, 2.5f, 45);
            for (int i = 0; i < 12; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(SummerOrange, FlameRed, i / 12f), 0.5f + i * 0.15f, 20 + i * 3);
            }
            SpawnSolarBurst(NPC.Center, 50, 15f);
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
