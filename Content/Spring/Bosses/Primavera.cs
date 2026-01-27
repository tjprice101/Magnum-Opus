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
using MagnumOpus.Content.Spring.Materials;

namespace MagnumOpus.Content.Spring.Bosses
{
    /// <summary>
    /// PRIMAVERA, HERALD OF BLOOM - POST-WALL OF FLESH BOSS
    /// 
    /// Design Philosophy:
    /// - Graceful, flowing movements like spring winds
    /// - Petal and floral projectile patterns
    /// - Life/growth themed attacks (healing denial, regeneration)
    /// - Musical connection: Vivaldi's Spring from The Four Seasons
    /// 
    /// Theme Colors: White (#FFFFFF), Pink (#FFB7C5), Light Blue (#ADD8E6)
    /// </summary>
    public class Primavera : ModNPC
    {
        #region Theme Colors
        private static readonly Color SpringWhite = new Color(255, 255, 255);
        private static readonly Color SpringPink = new Color(255, 183, 197);
        private static readonly Color SpringBlue = new Color(173, 216, 230);
        private static readonly Color PetalPink = new Color(255, 192, 203);
        private static readonly Color BlossomGold = new Color(255, 223, 186);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 10f;
        private const int BaseDamage = 35; // Post-Eye of Cthulhu tier
        private const float EnrageDistance = 2000f;
        private const int AttackWindowFrames = 60;
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
            PetalStorm,        // Radial petal projectile burst
            BlossomBreeze,     // Line of wind-carried petals
            SpringShower,      // Rain of floral projectiles from above
            
            // Phase 2 (60-30% HP)
            VernalVortex,      // Spiraling petal tornado
            GrowthSurge,       // Healing zones that buff the boss if player doesn't destroy
            FloralBarrage,     // Rapid-fire targeted shots
            
            // Phase 3 (30-0% HP)
            BloomingJudgment,  // Signature spectacle attack
            RebornSpring,      // Desperation healing + attack combo
            AprilShowers       // Massive area denial rain
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
        private AttackPattern lastAttack = AttackPattern.PetalStorm;
        private int consecutiveAttacks = 0;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 1800;
        
        private int frameCounter = 0;
        private int currentFrame = 0;
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        
        // Ground-based movement
        private int jumpCooldown = 0;
        private bool isGrounded = false;
        private const float JumpVelocity = -14f;
        private const float HighJumpVelocity = -18f;
        private const float MoveSpeed = 8f;
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; // Single frame for now
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 8;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 100;
            NPC.height = 120;
            NPC.damage = BaseDamage;
            NPC.defense = 15; // Post-Eye of Cthulhu tier
            NPC.lifeMax = 8000; // Post-Eye of Cthulhu tier (comparable to Skeletron 4.4k in Classic)
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false;  // Ground-based boss
            NPC.noTileCollide = false;  // Respects terrain
            NPC.value = Item.buyPrice(gold: 8);
            NPC.boss = true;
            NPC.npcSlots = 10f;
            NPC.aiStyle = -1;
            
            if (!Main.dedServ)
            {
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/DawnOfTheGroveColossus");
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement("Primavera, Herald of Bloom - the eternal spirit of spring's awakening, bringing life and renewal in a dance of petals.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Spring);
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
            
            // Enrage check
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
            
            // Update facing direction
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
                // Phase transition VFX
                difficultyTier = newTier;
                PhaseTransitionVFX();
            }
        }
        
        private void PhaseTransitionVFX()
        {
            SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
            
            // Cascading petal halos
            for (int i = 0; i < 8; i++)
            {
                float progress = i / 8f;
                Color haloColor = Color.Lerp(SpringPink, SpringBlue, progress);
                CustomParticles.HaloRing(NPC.Center, haloColor, 0.4f + i * 0.12f, 18 + i * 3);
            }
            
            // Central bloom burst
            CustomParticles.GenericFlare(NPC.Center, SpringWhite, 1.5f, 30);
            CustomParticles.GenericFlare(NPC.Center, SpringPink, 1.2f, 25);
            
            // Petal explosion
            SpawnPetalBurst(NPC.Center, 20, 8f);
            
            MagnumScreenEffects.AddScreenShake(10f);
        }
        
        private void UpdateAggression()
        {
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.4f + difficultyTier * 0.1f;
        private float GetAggressionRateMult() => Math.Max(0.6f, 1f - aggressionLevel * 0.3f - difficultyTier * 0.08f);
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                // Spawn VFX
                SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.3f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, SpringWhite, 2f, 40);
                
                for (int i = 0; i < 12; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(SpringPink, SpringBlue, i / 12f), 0.3f + i * 0.1f, 15 + i * 2);
                }
                
                SpawnPetalBurst(NPC.Center, 30, 10f);
            }
            
            if (Timer >= 90)
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
            float idealDist = 200f - difficultyTier * 20f;
            float distX = target.Center.X - NPC.Center.X;
            float absDistX = Math.Abs(distX);
            
            if (absDistX > idealDist)
            {
                // Move toward player
                float dir = Math.Sign(distX);
                float speed = MoveSpeed * GetAggressionSpeedMult();
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * speed, 0.08f);
            }
            else if (absDistX < idealDist * 0.5f)
            {
                // Too close, back away
                float dir = -Math.Sign(distX);
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * MoveSpeed * 0.5f, 0.06f);
            }
            else
            {
                // Ideal range, slow down
                NPC.velocity.X *= 0.9f;
            }
            
            // Jumping logic - jump to reach player or over obstacles
            if (isGrounded && jumpCooldown <= 0)
            {
                bool shouldJump = false;
                bool highJump = false;
                
                // Jump if player is significantly above
                float yDiff = NPC.Center.Y - target.Center.Y;
                if (yDiff > 100f)
                {
                    shouldJump = true;
                    highJump = yDiff > 250f;
                }
                
                // Jump over obstacles (simple check)
                if (Math.Abs(NPC.velocity.X) < 1f && absDistX > 60f)
                {
                    // We're trying to move but stuck
                    shouldJump = true;
                }
                
                // Random hop for variety
                if (!shouldJump && Main.rand.NextBool(120))
                {
                    shouldJump = true;
                }
                
                if (shouldJump)
                {
                    NPC.velocity.Y = highJump ? HighJumpVelocity : JumpVelocity;
                    jumpCooldown = 40 + Main.rand.Next(20);
                    
                    // Jump VFX
                    SpawnPetalBurst(NPC.Bottom, 6, 4f);
                    SoundEngine.PlaySound(SoundID.Item24 with { Pitch = 0.4f, Volume = 0.7f }, NPC.Center);
                }
            }
            
            int effectiveCooldown = (int)(attackCooldown * GetAggressionRateMult());
            if (effectiveCooldown <= 0 && Timer > (int)(40 * GetAggressionRateMult()))
            {
                SelectNextAttack(target);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.PetalStorm,
                AttackPattern.BlossomBreeze,
                AttackPattern.SpringShower
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.VernalVortex);
                pool.Add(AttackPattern.GrowthSurge);
                pool.Add(AttackPattern.FloralBarrage);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.BloomingJudgment);
                pool.Add(AttackPattern.RebornSpring);
                
                if (consecutiveAttacks >= 5)
                    pool.Add(AttackPattern.AprilShowers);
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Attack;
            consecutiveAttacks++;
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.PetalStorm:
                    Attack_PetalStorm(target);
                    break;
                case AttackPattern.BlossomBreeze:
                    Attack_BlossomBreeze(target);
                    break;
                case AttackPattern.SpringShower:
                    Attack_SpringShower(target);
                    break;
                case AttackPattern.VernalVortex:
                    Attack_VernalVortex(target);
                    break;
                case AttackPattern.GrowthSurge:
                    Attack_GrowthSurge(target);
                    break;
                case AttackPattern.FloralBarrage:
                    Attack_FloralBarrage(target);
                    break;
                case AttackPattern.BloomingJudgment:
                    Attack_BloomingJudgment(target);
                    break;
                case AttackPattern.RebornSpring:
                    Attack_RebornSpring(target);
                    break;
                case AttackPattern.AprilShowers:
                    Attack_AprilShowers(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            // Ground-based repositioning
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            float idealDist = 250f;
            float distX = target.Center.X - NPC.Center.X;
            float absDistX = Math.Abs(distX);
            
            if (Math.Abs(absDistX - idealDist) < 100f && Timer > 25)
            {
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames / 2;
                return;
            }
            
            // Move toward or away from player
            float dir = absDistX > idealDist ? Math.Sign(distX) : -Math.Sign(distX);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * MoveSpeed * 1.2f, 0.1f);
            
            // Jump if needed
            if (isGrounded && jumpCooldown <= 0 && (target.Center.Y < NPC.Center.Y - 80f || Math.Abs(NPC.velocity.X) < 1f))
            {
                NPC.velocity.Y = JumpVelocity;
                jumpCooldown = 30;
                SpawnPetalBurst(NPC.Bottom, 4, 3f);
            }
            
            if (Timer > 70)
            {
                State = BossPhase.Idle;
                Timer = 0;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Aggressive ground pursuit
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            float enrageSpeed = MoveSpeed * 1.8f;
            float dir = Math.Sign(target.Center.X - NPC.Center.X);
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * enrageSpeed, 0.12f);
            
            // Aggressive jumping
            if (isGrounded && jumpCooldown <= 0)
            {
                NPC.velocity.Y = HighJumpVelocity;
                jumpCooldown = 25;
                SpawnPetalBurst(NPC.Bottom, 8, 6f);
            }
            
            // Angry petal storm while chasing
            if (Timer % 12 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 5; i++)
                {
                    float angle = MathHelper.TwoPi * i / 5f + Timer * 0.08f;
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    SpawnPetalProjectile(NPC.Center, vel, 40);
                }
            }
            
            if (Timer % 4 == 0)
            {
                SpawnPetalParticle(NPC.Center + Main.rand.NextVector2Circular(50f, 50f));
            }
        }
        
        private void AI_Dying(Player target)
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            // Spectacular death animation
            if (deathTimer < 120)
            {
                float intensity = (float)deathTimer / 120f;
                
                if (deathTimer % 5 == 0)
                {
                    int petalCount = (int)(8 + intensity * 12);
                    SpawnPetalBurst(NPC.Center, petalCount, 5f + intensity * 5f);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + deathTimer * 0.04f;
                        Vector2 offset = angle.ToRotationVector2() * (30f + intensity * 50f);
                        Color flareColor = Color.Lerp(SpringPink, SpringBlue, (float)i / 6f);
                        CustomParticles.GenericFlare(NPC.Center + offset, flareColor, 0.4f + intensity * 0.3f, 15);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(intensity * 5f);
            }
            else if (deathTimer == 120)
            {
                // Final explosion
                CustomParticles.GenericFlare(NPC.Center, SpringWhite, 2.5f, 40);
                CustomParticles.GenericFlare(NPC.Center, SpringPink, 2f, 35);
                CustomParticles.GenericFlare(NPC.Center, SpringBlue, 1.5f, 30);
                
                for (int i = 0; i < 15; i++)
                {
                    Color ringColor = Color.Lerp(SpringPink, SpringBlue, i / 15f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, 0.4f + i * 0.15f, 20 + i * 3);
                }
                
                SpawnPetalBurst(NPC.Center, 50, 15f);
                MagnumScreenEffects.AddScreenShake(20f);
                
                NPC.life = 0;
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_PetalStorm(Player target)
        {
            int chargeTime = 50 - difficultyTier * 8;
            int projectileCount = 12 + difficultyTier * 4;
            
            NPC.velocity *= 0.96f;
            
            if (SubPhase == 0) // Charge
            {
                float progress = (float)Timer / chargeTime;
                
                // Converging petals
                if (Timer % 3 == 0)
                {
                    int count = (int)(4 + progress * 6);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + Timer * 0.04f;
                        float radius = 120f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnPetalParticle(pos);
                    }
                }
                
                // Warning ring
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, SpringPink, 8);
                
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
                    SoundEngine.PlaySound(SoundID.Item29, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, SpringPink, SpringBlue, 1f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float speed = 9f + difficultyTier * 2f;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnPetalProjectile(NPC.Center, vel, 45);
                        }
                    }
                    
                    SpawnPetalBurst(NPC.Center, 15, 8f);
                }
                
                if (Timer >= 40)
                    EndAttack();
            }
        }
        
        private void Attack_BlossomBreeze(Player target)
        {
            int waves = 3 + difficultyTier;
            int waveDelay = 25 - difficultyTier * 4;
            
            if (SubPhase < waves)
            {
                // Slow drift toward player
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 4f, 0.03f);
                
                if (Timer == 10) // Telegraph
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 400f, 8, WarningType.Danger);
                }
                
                if (Timer == 20 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item66, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 5 + difficultyTier * 2;
                    float spread = MathHelper.ToRadians(40f);
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offsetAngle = MathHelper.Lerp(-spread, spread, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offsetAngle) * (10f + difficultyTier * 2f);
                        SpawnPetalProjectile(NPC.Center, vel, 45, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, SpringWhite, 0.8f, 18);
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
        
        private void Attack_SpringShower(Player target)
        {
            int duration = 120 + difficultyTier * 30;
            int fireInterval = 8 - difficultyTier;
            
            // Ground-based: stand still and summon rain from above
            NPC.velocity.X *= 0.9f;
            
            // Warning flares
            if (Timer % 15 == 0)
            {
                for (int i = 0; i < 3 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-250f, 250f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -450f);
                    CustomParticles.GenericFlare(warningPos, SpringPink * 0.5f, 0.3f, 12);
                }
            }
            
            // Spawn projectiles from the sky
            if (Timer % fireInterval == 0 && Timer > 30 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 2 + difficultyTier;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-300f, 300f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -500f);
                    float ySpeed = 8f + difficultyTier * 2f;
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-1.5f, 1.5f), ySpeed);
                    
                    SpawnPetalProjectile(spawnPos, vel, 40);
                    CustomParticles.GenericFlare(spawnPos, SpringBlue, 0.3f, 8);
                }
            }
            
            // Periodic jump to stay active
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            if (isGrounded && Timer % 50 == 0 && Timer > 0)
            {
                NPC.velocity.Y = JumpVelocity * 0.7f;
                SpawnPetalBurst(NPC.Bottom, 4, 3f);
            }
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_VernalVortex(Player target)
        {
            int duration = 150 + difficultyTier * 20;
            
            // Ground-based: charge back and forth while creating vortex of petals
            isGrounded = NPC.velocity.Y == 0f || NPC.collideY;
            jumpCooldown = Math.Max(0, jumpCooldown - 1);
            
            // Charge toward player with periodic direction changes
            float dir = Math.Sign(target.Center.X - NPC.Center.X);
            float speed = (MoveSpeed + difficultyTier * 2f) * GetAggressionSpeedMult();
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * speed, 0.1f);
            
            // Jump periodically or to follow player upward
            if (isGrounded && jumpCooldown <= 0)
            {
                bool shouldJump = Timer % 40 == 0 || target.Center.Y < NPC.Center.Y - 100f;
                if (shouldJump)
                {
                    NPC.velocity.Y = target.Center.Y < NPC.Center.Y - 200f ? HighJumpVelocity : JumpVelocity;
                    jumpCooldown = 30;
                    SpawnPetalBurst(NPC.Bottom, 5, 4f);
                }
            }
            
            // Spiral projectiles emanate from boss
            int fireInterval = Math.Max(4, 10 - difficultyTier * 2);
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float spiralAngle = Timer * 0.12f;
                int arms = 3 + difficultyTier;
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                    float projSpeed = 8f + difficultyTier * 2f;
                    Vector2 vel = armAngle.ToRotationVector2() * projSpeed;
                    SpawnPetalProjectile(NPC.Center, vel, 40, arm % 2 == 0);
                }
                
                CustomParticles.GenericFlare(NPC.Center, SpringPink, 0.35f, 10);
            }
            
            // Ambient petals
            if (Timer % 6 == 0)
                SpawnPetalParticle(NPC.Center + Main.rand.NextVector2Circular(35f, 35f));
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_GrowthSurge(Player target)
        {
            int chargeTime = 60 - difficultyTier * 10;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0) // Charge
            {
                float progress = (float)Timer / chargeTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 80f, progress, PetalPink, 6);
                
                // Growing glow
                if (Timer % 5 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, Color.Lerp(SpringPink, BlossomGold, progress), 0.4f + progress * 0.4f, 15);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Spawn healing zones + attack
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.5f }, NPC.Center);
                    
                    // Heal boss slightly
                    int healAmount = (int)(NPC.lifeMax * 0.02f);
                    NPC.life = Math.Min(NPC.lifeMax, NPC.life + healAmount);
                    NPC.HealEffect(healAmount, true);
                    
                    // VFX
                    CustomParticles.GenericFlare(NPC.Center, BlossomGold, 1.2f, 25);
                    for (int i = 0; i < 6; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(SpringPink, BlossomGold, i / 6f), 0.4f + i * 0.1f, 15 + i * 2);
                    }
                    SpawnPetalBurst(NPC.Center, 12, 6f);
                    
                    // Projectile burst
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int count = 10 + difficultyTier * 4;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            Vector2 vel = angle.ToRotationVector2() * (7f + difficultyTier * 2f);
                            SpawnPetalProjectile(NPC.Center, vel, 40);
                        }
                    }
                }
                
                if (Timer >= 50)
                    EndAttack();
            }
        }
        
        private void Attack_FloralBarrage(Player target)
        {
            int burstCount = 4 + difficultyTier * 2;
            int burstDelay = 18 - difficultyTier * 3;
            
            // Track player
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 6f, 0.04f);
            
            if (SubPhase < burstCount)
            {
                if (Timer == 8) // Telegraph
                {
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, direction, 350f, 6, WarningType.Caution);
                }
                
                if (Timer == burstDelay && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
                    
                    Vector2 direction = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    int count = 3 + difficultyTier;
                    
                    for (int i = 0; i < count; i++)
                    {
                        float offset = MathHelper.Lerp(-0.15f, 0.15f, (float)i / (count - 1));
                        Vector2 vel = direction.RotatedBy(offset) * (12f + difficultyTier * 3f);
                        SpawnPetalProjectile(NPC.Center, vel, 45, true);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, SpringPink, 0.6f, 12);
                }
                
                if (Timer >= burstDelay + 5)
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
        
        private void Attack_BloomingJudgment(Player target)
        {
            // SIGNATURE SPECTACLE ATTACK
            int chargeTime = 80 - difficultyTier * 10;
            int waveCount = 3 + difficultyTier;
            
            if (SubPhase == 0) // Charge with safe zone indicator
            {
                NPC.velocity *= 0.95f;
                
                float progress = (float)Timer / chargeTime;
                
                // Converging petal ring
                if (Timer % 4 == 0)
                {
                    int particleCount = (int)(8 + progress * 12);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.05f;
                        float radius = 180f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        SpawnPetalParticle(pos);
                    }
                }
                
                // Safe zone indicator
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 90f, 12);
                    
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(50f), 150f, 6);
                }
                
                if (Timer > chargeTime * 0.7f)
                    MagnumScreenEffects.AddScreenShake(progress * 4f);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount) // Multi-wave radial burst with safe arc
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(12f);
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, SpringWhite, SpringPink, 1.2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 24 + difficultyTier * 6;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(25f - difficultyTier * 3f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 10f + difficultyTier * 2f + SubPhase;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            SpawnPetalProjectile(NPC.Center, vel, 50, i % 3 == 0);
                        }
                    }
                    
                    // Cascading halos
                    for (int i = 0; i < 10; i++)
                    {
                        Color ringColor = Color.Lerp(SpringPink, SpringBlue, i / 10f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.4f + i * 0.12f, 18 + i * 3);
                    }
                    
                    SpawnPetalBurst(NPC.Center, 20, 10f);
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
        
        private void Attack_RebornSpring(Player target)
        {
            // Desperation healing + attack
            int healTime = 60;
            
            if (SubPhase == 0) // Healing phase
            {
                NPC.velocity *= 0.95f;
                
                float progress = (float)Timer / healTime;
                
                // Rising energy
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 particlePos = NPC.Center + new Vector2(Main.rand.NextFloat(-40f, 40f), 50f);
                        Vector2 particleVel = new Vector2(0, -3f - progress * 2f);
                        CustomParticles.GenericGlow(particlePos, particleVel, BlossomGold, 0.3f + progress * 0.2f, 20, true);
                    }
                }
                
                // Warning
                BossVFXOptimizer.DangerZoneRing(NPC.Center, 250f, 16);
                
                if (Timer >= healTime)
                {
                    // Heal
                    int healAmount = (int)(NPC.lifeMax * 0.05f);
                    NPC.life = Math.Min(NPC.lifeMax, NPC.life + healAmount);
                    NPC.HealEffect(healAmount, true);
                    
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Attack phase
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Center, BlossomGold, 1.5f, 30);
                    CustomParticles.GenericFlare(NPC.Center, SpringWhite, 1.2f, 25);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Big expanding ring
                        int count = 20 + difficultyTier * 6;
                        for (int i = 0; i < count; i++)
                        {
                            float angle = MathHelper.TwoPi * i / count;
                            Vector2 vel = angle.ToRotationVector2() * (8f + difficultyTier * 2f);
                            SpawnPetalProjectile(NPC.Center, vel, 45);
                        }
                    }
                    
                    for (int i = 0; i < 8; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(BlossomGold, SpringPink, i / 8f), 0.5f + i * 0.15f, 18 + i * 3);
                    }
                    
                    SpawnPetalBurst(NPC.Center, 25, 12f);
                    MagnumScreenEffects.AddScreenShake(15f);
                }
                
                if (Timer >= 50)
                    EndAttack();
            }
        }
        
        private void Attack_AprilShowers(Player target)
        {
            // Massive area denial
            int duration = 180 + difficultyTier * 30;
            int fireInterval = Math.Max(3, 6 - difficultyTier);
            
            // Slow drift
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * 3f, 0.02f);
            
            // Dense rain
            if (Timer % fireInterval == 0 && Timer > 30 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 4 + difficultyTier * 2;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -550f);
                    float ySpeed = 10f + difficultyTier * 3f + Main.rand.NextFloat(0, 3f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), ySpeed);
                    
                    SpawnPetalProjectile(spawnPos, vel, 40, i % 2 == 0);
                    CustomParticles.GenericFlare(spawnPos, SpringBlue * 0.5f, 0.25f, 6);
                }
            }
            
            // Ambient particles
            if (Timer % 8 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                    SpawnPetalParticle(pos);
                }
            }
            
            if (Timer >= duration)
                EndAttack();
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
            // Orbiting petals
            if (Timer % 8 == 0)
            {
                float baseAngle = Timer * 0.02f;
                for (int i = 0; i < 3; i++)
                {
                    float angle = baseAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 50f + (float)Math.Sin(Timer * 0.05f + i) * 15f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    SpawnPetalParticle(pos);
                }
            }
            
            // Gentle glow
            if (Timer % 12 == 0)
            {
                Color glowColor = Color.Lerp(SpringPink, SpringBlue, (float)Math.Sin(Timer * 0.02f) * 0.5f + 0.5f);
                CustomParticles.GenericFlare(NPC.Center, glowColor * 0.5f, 0.3f, 15);
            }
            
            Lighting.AddLight(NPC.Center, SpringPink.ToVector3() * 0.5f);
        }
        
        private void SpawnPetalParticle(Vector2 position)
        {
            Color petalColor = Main.rand.NextBool() ? SpringPink : PetalPink;
            if (Main.rand.NextBool(4))
                petalColor = SpringWhite;
            
            Vector2 vel = Main.rand.NextVector2Circular(2f, 2f) + new Vector2(0, -0.5f);
            CustomParticles.GenericGlow(position, vel, petalColor, 0.25f, 25, true);
        }
        
        private void SpawnPetalBurst(Vector2 position, int count, float speed)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + Main.rand.NextFloat(-0.2f, 0.2f);
                Vector2 vel = angle.ToRotationVector2() * (speed * Main.rand.NextFloat(0.8f, 1.2f));
                
                Color petalColor = Color.Lerp(SpringPink, PetalPink, Main.rand.NextFloat());
                if (Main.rand.NextBool(4))
                    petalColor = SpringWhite;
                
                CustomParticles.GenericGlow(position, vel, petalColor, 0.3f, 30, true);
            }
        }
        
        private void SpawnPetalProjectile(Vector2 position, Vector2 velocity, int damage, bool homing = false)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient) return;
            
            float homingStrength = homing ? 0.015f : 0f;
            Color projColor = Main.rand.NextBool() ? SpringPink : PetalPink;
            
            BossProjectileHelper.SpawnHostileOrb(position, velocity, damage, projColor, homingStrength);
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // Trail
            for (int i = 0; i < NPC.oldPos.Length - 1; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                Color trailColor = Color.Lerp(SpringPink, SpringBlue, progress) * (1f - progress) * 0.5f;
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                float trailScale = NPC.scale * (1f - progress * 0.3f);
                
                spriteBatch.Draw(texture, trailPos, null, trailColor, NPC.rotation, origin, trailScale, SpriteEffects.None, 0f);
            }
            
            // Glow layers
            float pulse = (float)Math.Sin(Timer * 0.08f) * 0.1f + 1f;
            
            Color outerGlow = SpringPink * 0.3f;
            outerGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, outerGlow, NPC.rotation, origin, NPC.scale * pulse * 1.15f, SpriteEffects.None, 0f);
            
            Color midGlow = SpringBlue * 0.4f;
            midGlow.A = 0;
            spriteBatch.Draw(texture, drawPos, null, midGlow, NPC.rotation, origin, NPC.scale * pulse * 1.08f, SpriteEffects.None, 0f);
            
            // Main sprite
            SpriteEffects effects = NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            spriteBatch.Draw(texture, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot & Drops
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Spring Resonant Energy (100%)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<SpringResonantEnergy>(), 1, 3, 5));
            
            // Vernal Bar materials
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PetalOfRebirth>(), 1, 15, 25));
            
            // Dormant Spring Core (for summoning again)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DormantSpringCore>(), 3));
        }
        
        public override void OnKill()
        {
            // Final VFX
            CustomParticles.GenericFlare(NPC.Center, SpringWhite, 2f, 40);
            for (int i = 0; i < 10; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(SpringPink, SpringBlue, i / 10f), 0.5f + i * 0.15f, 20 + i * 3);
            }
            SpawnPetalBurst(NPC.Center, 40, 12f);
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
