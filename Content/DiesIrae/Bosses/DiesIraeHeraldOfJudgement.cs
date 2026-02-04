using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Audio;
using MagnumOpus.Content.DiesIrae.ResonanceEnergies;
using MagnumOpus.Content.DiesIrae.HarmonicCores;
using MagnumOpus.Content.DiesIrae.ResonantWeapons;
using MagnumOpus.Content.DiesIrae.Accessories;
using MagnumOpus.Content.DiesIrae.Projectiles;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.DiesIrae.Bosses
{
    /// <summary>
    /// DIES IRAE, HERALD OF JUDGMENT - POST-NACHTMUSIK BOSS
    /// 
    /// Design Philosophy:
    /// - Infernal judge who passes ultimate judgment on all souls
    /// - Single-phase boss with escalating intensity
    /// - Fire and brimstone aesthetic with judgment/execution themes
    /// 
    /// Musical Theme:
    /// - "Dies Irae" - Day of Wrath, the final judgment
    /// </summary>
    public class DiesIraeHeraldOfJudgement : ModNPC
    {
        #region Theme Colors - Dies Irae Palette
        private static readonly Color BloodRed = new Color(139, 0, 0);        // #8B0000
        private static readonly Color EmberOrange = new Color(255, 69, 0);    // #FF4500
        private static readonly Color CharredBlack = new Color(25, 20, 15);   // #19140F
        private static readonly Color Crimson = new Color(200, 30, 30);       // #C81E1E
        private static readonly Color HellfireGold = new Color(255, 180, 50); // Golden flames
        #endregion
        
        #region Constants
        // POST-NACHTMUSIK BOSS - Ultimate difficulty tier (Nachtmusik BaseDamage: 350)
        private const float BaseSpeed = 40f;
        private const int BaseDamage = 500;
        private const float EnrageDistance = 800f;
        private const float TeleportDistance = 1500f;
        private const int AttackWindowFrames = 20;
        
        // Projectile speeds (faster than Nachtmusik)
        private const float FastProjectileSpeed = 38f;
        private const float MediumProjectileSpeed = 26f;
        private const float HomingSpeed = 16f;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Idle,
            Attack,
            Reposition,
            Enraged,
            Death
        }
        
        private enum AttackPattern
        {
            // Core Attacks
            HellfireBarrage,      // Rapid fire projectiles
            JudgmentRay,          // Sweeping beam attack
            InfernalRing,         // Expanding ring of fire
            CondemnationStrike,   // Targeted slam attack
            SoulHarvest,          // Homing soul projectiles
            
            // Advanced Attacks (Phase 2+)
            WrathfulDescent,      // Dive bomb with fire trail
            ChainOfDamnation,     // Multi-target chain attack
            ApocalypseRain,       // Rain of fire from above
            FinalJudgment,        // Safe-arc radial burst
            DivinePunishment      // Ultimate attack
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
        private AttackPattern lastAttack = AttackPattern.HellfireBarrage;
        
        private Vector2 dashTarget;
        private Vector2 dashDirection;
        private int dashCount = 0;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 3600; // 60 seconds
        
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
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox
            NPC.width = 200;
            NPC.height = 200;
            NPC.damage = BaseDamage;
            NPC.defense = 280; // POST-NACHTMUSIK ULTIMATE defense (Nachtmusik: 180)
            NPC.lifeMax = 10000000; // 10 million HP - POST-NACHTMUSIK (Nachtmusik: 8M total)
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 50);
            NPC.boss = true;
            NPC.npcSlots = 20f;
            NPC.aiStyle = -1;
            NPC.scale = 1.0f;
            
            if (!Main.dedServ)
            {
                Music = MusicID.Boss2; // Placeholder - add custom music later
            }
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement(
                    "Dies Irae, Herald of Judgment - The final arbiter of all souls. " +
                    "Its flames judge the worthy and condemn the wicked. " +
                    "None escape its verdict.")
            });
        }

        #region Color Helpers
        private Color GetDiesIraeGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(CharredBlack, BloodRed, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(BloodRed, EmberOrange, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(EmberOrange, HellfireGold, (progress - 0.66f) * 3f);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.3f;
        private float GetAggressionRateMult() => 1f - aggressionLevel * 0.15f;
        #endregion

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.DiesIrae);
                hasRegisteredHealthBar = true;
            }
            
            if (State == BossPhase.Death)
            {
                UpdateDeathAnimation();
                return;
            }
            
            NPC.TargetClosest(true);
            Player target = Main.player[NPC.target];
            
            if (!target.active || target.dead)
            {
                NPC.velocity.Y -= 0.5f;
                NPC.EncourageDespawn(60);
                return;
            }
            
            UpdateDifficultyTier();
            UpdateAggression();
            if (attackCooldown > 0) attackCooldown--;
            CheckEnrage(target);
            
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
            }
            
            Timer++;
            fightTimer++;
            
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            
            float lightIntensity = isEnraged ? 1.5f : 1.0f;
            Lighting.AddLight(NPC.Center, EmberOrange.ToVector3() * lightIntensity);
        }

        public override bool CheckDead()
        {
            if (State != BossPhase.Death)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                State = BossPhase.Death;
                Timer = 0;
                return false;
            }
            
            return State == BossPhase.Death && deathTimer >= 180;
        }

        #region Core AI Methods
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : 2);
            
            if (newTier > difficultyTier)
            {
                difficultyTier = newTier;
                OnPhaseChange(newTier);
            }
        }
        
        private void OnPhaseChange(int newTier)
        {
            if (Main.netMode != NetmodeID.Server)
            {
                string message = newTier switch
                {
                    1 => "YOUR SINS CANNOT BE HIDDEN!",
                    2 => "FACE THE FINAL JUDGMENT!",
                    _ => ""
                };
                
                if (!string.IsNullOrEmpty(message))
                    Main.NewText(message, EmberOrange);
            }
            
            MagnumScreenEffects.AddScreenShake(10f + newTier * 5f);
        }
        
        private void UpdateAggression()
        {
            aggressionLevel = Math.Min(1f, fightTimer / (float)MaxAggressionTime);
        }
        
        private void CheckEnrage(Player target)
        {
            float distanceFromTarget = Vector2.Distance(NPC.Center, target.Center);
            
            if (distanceFromTarget > EnrageDistance && State != BossPhase.Enraged)
            {
                enrageTimer++;
                if (enrageTimer > 300) // 5 seconds to return
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("THERE IS NO ESCAPE FROM JUDGMENT!", Crimson);
                }
            }
            else
            {
                enrageTimer = Math.Max(0, enrageTimer - 2);
            }
        }
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar, NPC.Center);
                
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("DIES IRAE - THE DAY OF WRATH HAS COME!", BloodRed);
            }
            
            NPC.velocity *= 0.95f;
            
            if (Timer >= 90)
            {
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = 60;
            }
        }
        
        private void AI_Idle(Player target)
        {
            Vector2 hoverTarget = target.Center + new Vector2(0, -350f);
            Vector2 toTarget = hoverTarget - NPC.Center;
            
            float speed = BaseSpeed * 0.15f * GetAggressionSpeedMult();
            if (toTarget.Length() > 50f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * speed, 0.08f);
            }
            else
            {
                NPC.velocity *= 0.92f;
            }
            
            // === PHASE 10 MUSICAL VFX: Beat Synced Rhythm - Herald's Wrath ===
            if (Timer % 32 == 0)
            {
                Phase10Integration.Universal.BeatSyncedRhythm(NPC.Center, EmberOrange, 110f, Timer);
            }
            
            if (attackCooldown <= 0 && Timer >= AttackWindowFrames)
            {
                SelectNextAttack(target);
            }
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.HellfireBarrage:
                    Attack_HellfireBarrage(target);
                    break;
                case AttackPattern.JudgmentRay:
                    Attack_JudgmentRay(target);
                    break;
                case AttackPattern.InfernalRing:
                    Attack_InfernalRing(target);
                    break;
                case AttackPattern.CondemnationStrike:
                    Attack_CondemnationStrike(target);
                    break;
                case AttackPattern.SoulHarvest:
                    Attack_SoulHarvest(target);
                    break;
                case AttackPattern.WrathfulDescent:
                    Attack_WrathfulDescent(target);
                    break;
                case AttackPattern.ChainOfDamnation:
                    Attack_ChainOfDamnation(target);
                    break;
                case AttackPattern.ApocalypseRain:
                    Attack_ApocalypseRain(target);
                    break;
                case AttackPattern.FinalJudgment:
                    Attack_FinalJudgment(target);
                    break;
                case AttackPattern.DivinePunishment:
                    Attack_DivinePunishment(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            float duration = 60f;
            float progress = Timer / duration;
            
            if (Timer < 40)
            {
                Vector2 repositionTarget = target.Center + Main.rand.NextVector2CircularEdge(400f, 400f);
                Vector2 toReposition = repositionTarget - NPC.Center;
                
                // Smooth bell curve movement: accelerate then decelerate
                float speedCurve = BossAIUtilities.Easing.EaseOutQuad(progress) * BossAIUtilities.Easing.EaseInQuad(1f - progress) * 4f;
                float speed = BaseSpeed * 0.5f * GetAggressionSpeedMult() * Math.Max(0.3f, speedCurve);
                
                if (toReposition.Length() > 80f)
                {
                    toReposition.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toReposition * speed, 0.12f);
                }
                
                // Trail particles while moving
                if (NPC.velocity.Length() > 2f)
                {
                    float trailProgress = Timer / 40f;
                    BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, EmberOrange, trailProgress);
                }
            }
            else
            {
                // Smooth deceleration using easing
                float decelProgress = (Timer - 40f) / 20f;
                float decelMult = 1f - BossAIUtilities.Easing.EaseOutCubic(Math.Min(1f, decelProgress));
                NPC.velocity *= 0.85f + 0.1f * decelMult;
            }
            
            // Recovery shimmer effect - vulnerability indicator
            if (Timer % 4 == 0)
            {
                float shimmerProgress = Timer / 60f;
                BossVFXOptimizer.RecoveryShimmer(NPC.Center, Crimson, 55f, shimmerProgress);
            }
            
            if (Timer >= 60)
            {
                // Ready to attack again - visual warning
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, BloodRed, 0.7f);
                
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = (int)(30 * GetAggressionRateMult());
            }
        }
        
        private void AI_Enraged(Player target)
        {
            Vector2 toTarget = target.Center - NPC.Center;
            float distance = toTarget.Length();
            
            // Aggressive pursuit
            if (distance > 100f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 1.5f, 0.15f);
            }
            
            // Fire rapid projectiles
            if (Timer % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 fireDir = toTarget.SafeNormalize(Vector2.UnitX);
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 vel = fireDir.RotatedBy(MathHelper.ToRadians(15f * i)) * FastProjectileSpeed;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel, 
                        ProjectileID.InfernoHostileBolt, (int)(BaseDamage * 0.5f), 2f);
                }
            }
            
            // Exit enrage if player returns
            if (distance < EnrageDistance * 0.7f)
            {
                isEnraged = false;
                enrageTimer = 0;
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = 30;
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.HellfireBarrage,
                AttackPattern.JudgmentRay,
                AttackPattern.InfernalRing,
                AttackPattern.CondemnationStrike,
                AttackPattern.SoulHarvest
            };
            
            // Add advanced attacks based on difficulty
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.WrathfulDescent);
                pool.Add(AttackPattern.ChainOfDamnation);
                pool.Add(AttackPattern.ApocalypseRain);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.FinalJudgment);
                pool.Add(AttackPattern.DivinePunishment);
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Attack;
        }
        
        private void EndAttack()
        {
            // Visual cue: Attack ending - player has a window
            BossVFXOptimizer.AttackEndCue(NPC.Center, EmberOrange, HellfireGold, 0.9f);
            
            State = BossPhase.Reposition;
            Timer = 0;
            SubPhase = 0;
            attackCooldown = (int)(45 * GetAggressionRateMult());
        }
        
        #endregion
        
        #region Attack Implementations
        
        private void Attack_HellfireBarrage(Player target)
        {
            int burstCount = 4 + difficultyTier;
            int burstDelay = Math.Max(15, 30 - difficultyTier * 5);
            
            NPC.velocity *= 0.95f;
            
            // === ENHANCED TELEGRAPH VFX ===
            float chargeProgress = Math.Min(1f, Timer / 30f);
            DiesIraeVFX.ChargeUp(NPC.Center, chargeProgress, 1.2f);
            
            if (SubPhase < burstCount)
            {
                if (Timer == 5)
                {
                    SoundEngine.PlaySound(SoundID.Item20 with { Pitch = -0.3f, Volume = 1.3f }, NPC.Center);
                    
                    // === MASSIVE MUZZLE FLASH ===
                    DiesIraeVFX.FireImpact(NPC.Center, 0.9f);
                    MagnumScreenEffects.AddScreenShake(4f + difficultyTier * 2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        int projectileCount = 7 + difficultyTier * 3;
                        float spreadAngle = MathHelper.ToRadians(50f + difficultyTier * 12f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.Lerp(-spreadAngle / 2, spreadAngle / 2, (float)i / (projectileCount - 1));
                            Vector2 vel = toTarget.RotatedBy(angle) * (MediumProjectileSpeed + difficultyTier * 4f);
                            
                            // Alternate between inferno bolts and fireballs
                            int projType = i % 3 == 0 ? ProjectileID.CultistBossFireBall : ProjectileID.InfernoHostileBolt;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                projType, (int)(BaseDamage * 0.4f), 2f);
                        }
                    }
                }
                
                if (Timer >= burstDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 25)
                    EndAttack();
            }
        }
        
        private void Attack_JudgmentRay(Player target)
        {
            int sweepTime = 80 - difficultyTier * 10;
            
            if (SubPhase == 0) // Telegraph - DRAMATIC CHARGE UP
            {
                NPC.velocity *= 0.9f;
                
                // === INTENSE CHARGE VFX ===
                float progress = Timer / 50f;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 1.5f);
                
                // Warning line toward player
                if (Timer % 5 == 0)
                {
                    Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 warningPos = NPC.Center + toTarget * (50f + i * 40f);
                        DiesIraeVFX.WarningFlare(warningPos, 0.4f);
                    }
                }
                
                if (Timer >= 50)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(10f);
                    DiesIraeVFX.FireImpact(NPC.Center, 1.3f);
                }
            }
            else if (SubPhase == 1) // Sweep - MASSIVE BEAM EFFECT
            {
                // Continuous beam VFX
                float sweepProgress = (float)Timer / sweepTime;
                float sweepAngle = MathHelper.Lerp(-MathHelper.PiOver2, MathHelper.PiOver2, sweepProgress);
                Vector2 baseDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                Vector2 beamDir = baseDir.RotatedBy(sweepAngle);
                
                // Beam visual
                for (int i = 0; i < 30; i++)
                {
                    Vector2 beamPos = NPC.Center + beamDir * (50f + i * 25f);
                    Color beamColor = Color.Lerp(Color.White, DiesIraeColors.Crimson, i / 30f);
                    beamColor.A = 0;
                    
                    var beam = new BloomParticle(beamPos, Vector2.Zero, beamColor * 0.8f, 0.4f, 5);
                    MagnumParticleHandler.SpawnParticle(beam);
                }
                
                if (Main.netMode != NetmodeID.MultiplayerClient && Timer % 4 == 0)
                {
                    Vector2 vel = beamDir * FastProjectileSpeed * 1.2f;
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                        ProjectileID.CultistBossFireBall, (int)(BaseDamage * 0.35f), 2f);
                }
                
                if (Timer >= sweepTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else
            {
                if (Timer >= 20)
                    EndAttack();
            }
        }
        
        private void Attack_InfernalRing(Player target)
        {
            int rings = 3 + difficultyTier;
            int ringDelay = 35 - difficultyTier * 5;
            
            NPC.velocity *= 0.95f;
            
            // Charge up between rings
            if (SubPhase < rings && Timer < 10)
            {
                float progress = Timer / 10f + SubPhase * 0.2f;
                DiesIraeVFX.ChargeUp(NPC.Center, Math.Min(1f, progress), 1f);
            }
            
            if (SubPhase < rings)
            {
                if (Timer == 10)
                {
                    SoundEngine.PlaySound(SoundID.Item45 with { Pitch = -0.2f + SubPhase * 0.1f, Volume = 1.4f }, NPC.Center);
                    
                    // === MASSIVE RING EXPLOSION VFX ===
                    DiesIraeVFX.FireImpact(NPC.Center, 1.2f + SubPhase * 0.2f);
                    MagnumScreenEffects.AddScreenShake(5f + SubPhase * 2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 16 + difficultyTier * 6 + SubPhase * 2;
                        float speed = MediumProjectileSpeed + SubPhase * 4f;
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                ProjectileID.Fireball, (int)(BaseDamage * 0.3f), 2f);
                        }
                    }
                }
                
                if (Timer >= ringDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 25)
                    EndAttack();
            }
        }
        
        private void Attack_CondemnationStrike(Player target)
        {
            int strikeCount = 3 + difficultyTier;
            
            if (SubPhase == 0) // Telegraph - DRAMATIC WARNING
            {
                dashTarget = target.Center;
                NPC.velocity *= 0.9f;
                
                // Warning line to target
                float progress = Timer / 40f;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 1.3f);
                
                if (Timer % 4 == 0)
                {
                    Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 warningPos = NPC.Center + toTarget * (i * 50f);
                        DiesIraeVFX.WarningFlare(warningPos, 0.5f);
                    }
                }
                
                if (Timer >= 40)
                {
                    Timer = 0;
                    SubPhase = 1;
                    dashCount = 0;
                }
            }
            else if (SubPhase == 1) // Strikes - DEVASTATING DASHES
            {
                if (dashCount < strikeCount)
                {
                    if (Timer == 1)
                    {
                        dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        NPC.velocity = dashDirection * BaseSpeed * 1.8f;
                        SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.2f, Volume = 1.3f }, NPC.Center);
                        DiesIraeVFX.FireImpact(NPC.Center, 1f);
                        MagnumScreenEffects.AddScreenShake(8f);
                    }
                    
                    // === FIRE TRAIL DURING DASH ===
                    DiesIraeVFX.FireTrail(NPC.Center, NPC.velocity, 1.5f);
                    DiesIraeVFX.FireTrail(NPC.Center + Main.rand.NextVector2Circular(20f, 20f), NPC.velocity, 1f);
                    
                    // Fire projectiles during dash
                    if (Timer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 perpendicular = new Vector2(-dashDirection.Y, dashDirection.X);
                        for (int side = -1; side <= 1; side += 2)
                        {
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, perpendicular * side * 10f,
                                ProjectileID.InfernoHostileBolt, (int)(BaseDamage * 0.25f), 1f);
                        }
                    }
                    
                    if (Timer >= 22)
                    {
                        Timer = 0;
                        dashCount++;
                        NPC.velocity *= 0.25f;
                        DiesIraeVFX.FireImpact(NPC.Center, 0.8f);
                    }
                }
                else
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 25)
                    EndAttack();
            }
        }
        
        private void Attack_SoulHarvest(Player target)
        {
            int soulCount = 8 + difficultyTier * 4;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0) // Spawn souls - DRAMATIC SUMMONING
            {
                float progress = Timer / 25f;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 1.2f);
                
                if (Timer == 25)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath6 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                    DiesIraeVFX.FireImpact(NPC.Center, 1.5f);
                    MagnumScreenEffects.AddScreenShake(12f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < soulCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / soulCount;
                            Vector2 spawnOffset = angle.ToRotationVector2() * 180f;
                            Vector2 vel = Main.rand.NextVector2Circular(3f, 3f);
                            
                            int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center + spawnOffset, vel,
                                ProjectileID.LostSoulHostile, (int)(BaseDamage * 0.35f), 1f);
                            
                            // Spawn VFX at each soul location
                            DiesIraeVFX.FireImpact(NPC.Center + spawnOffset, 0.5f);
                        }
                    }
                }
                
                if (Timer >= 65)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else
            {
                if (Timer >= 35)
                    EndAttack();
            }
        }
        
        private void Attack_WrathfulDescent(Player target)
        {
            if (SubPhase == 0) // Rise up - OMINOUS ASCENT
            {
                Vector2 riseTarget = target.Center + new Vector2(0, -650f);
                Vector2 toRise = riseTarget - NPC.Center;
                
                // Fire trail while rising
                DiesIraeVFX.FireTrail(NPC.Center, NPC.velocity, 1.2f);
                
                if (toRise.Length() > 50f)
                {
                    toRise.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toRise * BaseSpeed * 1.2f, 0.12f);
                }
                
                if (Timer >= 50 || Vector2.Distance(NPC.Center, riseTarget) < 100f)
                {
                    Timer = 0;
                    SubPhase = 1;
                    dashTarget = target.Center;
                }
            }
            else if (SubPhase == 1) // Telegraph - DRAMATIC PAUSE
            {
                NPC.velocity *= 0.8f;
                
                // Charge up above player
                float progress = Timer / 30f;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 1.5f);
                
                // Ground impact warning
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        Vector2 warningPos = dashTarget + angle.ToRotationVector2() * 100f;
                        DiesIraeVFX.WarningFlare(warningPos, 0.6f);
                    }
                }
                
                if (Timer >= 30)
                {
                    Timer = 0;
                    SubPhase = 2;
                    dashDirection = (dashTarget - NPC.Center).SafeNormalize(Vector2.UnitY);
                    NPC.velocity = dashDirection * BaseSpeed * 2.5f;
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 1.5f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(15f);
                }
            }
            else if (SubPhase == 2) // Descent - DEVASTATING DIVE
            {
                // INTENSE fire trail during descent
                for (int i = 0; i < 3; i++)
                {
                    DiesIraeVFX.FireTrail(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), NPC.velocity, 1.5f);
                }
                
                if (Timer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 trailVel = Main.rand.NextVector2Circular(5f, 5f);
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, trailVel,
                        ProjectileID.Fireball, (int)(BaseDamage * 0.2f), 1f);
                }
                
                if (Timer >= 35 || NPC.Center.Y > dashTarget.Y + 150f)
                {
                    Timer = 0;
                    SubPhase = 3;
                    NPC.velocity *= 0.15f;
                    
                    // === MASSIVE IMPACT EXPLOSION ===
                    DiesIraeVFX.FireImpact(NPC.Center, 2f);
                    MagnumScreenEffects.AddScreenShake(20f);
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 1.5f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 24; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 24f;
                            Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed * 1.3f;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                ProjectileID.InfernoHostileBolt, (int)(BaseDamage * 0.35f), 2f);
                        }
                    }
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 35)
                    EndAttack();
            }
        }
        
        private void Attack_ChainOfDamnation(Player target)
        {
            int chainLength = 5 + difficultyTier;
            
            NPC.velocity *= 0.95f;
            
            // Charge up
            if (SubPhase < chainLength && Timer < 10)
            {
                DiesIraeVFX.ChargeUp(NPC.Center, Timer / 10f, 0.8f);
            }
            
            if (SubPhase < chainLength)
            {
                if (Timer == 10)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f + SubPhase * 0.1f }, NPC.Center);
                    DiesIraeVFX.FireImpact(NPC.Center, 0.7f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        float offset = MathHelper.ToRadians(35f * (SubPhase % 2 == 0 ? 1 : -1));
                        Vector2 vel = toTarget.RotatedBy(offset) * FastProjectileSpeed * 1.1f;
                        
                        // Create a chain of linked projectiles
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 spawnPos = NPC.Center + toTarget * (i * 35f);
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, vel,
                                ProjectileID.CursedFlameHostile, (int)(BaseDamage * 0.3f), 2f);
                        }
                    }
                }
                
                if (Timer >= 18)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 25)
                    EndAttack();
            }
        }
        
        private void Attack_ApocalypseRain(Player target)
        {
            int duration = 140 + difficultyTier * 40;
            int fireRate = Math.Max(4, 10 - difficultyTier * 2);
            
            // Hover above player
            Vector2 hoverTarget = target.Center + new Vector2(0, -550f);
            Vector2 toHover = hoverTarget - NPC.Center;
            if (toHover.Length() > 30f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed * 0.6f, 0.1f);
            }
            
            // Ambient fire aura while hovering
            DiesIraeVFX.FireTrail(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), Vector2.Zero, 0.8f);
            
            // Rain fire
            if (Timer % fireRate == 0 && Timer > 20 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projectilesPerWave = 2 + difficultyTier;
                for (int p = 0; p < projectilesPerWave; p++)
                {
                    float xOffset = Main.rand.NextFloat(-450f, 450f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -650f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), MediumProjectileSpeed * 1.1f);
                    
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPos, vel,
                        ProjectileID.InfernoHostileBolt, (int)(BaseDamage * 0.35f), 2f);
                    
                    // Spawn warning at ground level
                    DiesIraeVFX.WarningFlare(new Vector2(spawnPos.X, target.Center.Y), 0.4f);
                }
            }
            
            if (Timer >= duration)
                EndAttack();
        }
        
        private void Attack_FinalJudgment(Player target)
        {
            int chargeTime = 70 - difficultyTier * 10;
            int waveCount = 4 + difficultyTier;
            
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0) // Charge - APOCALYPTIC BUILDUP
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("JUDGMENT IS UPON YOU!", EmberOrange);
                }
                
                // === MASSIVE CHARGE UP VFX ===
                float progress = (float)Timer / chargeTime;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 2f);
                
                // === PHASE 10 MUSICAL VFX: Crescendo Charge Up - Final Judgment Building ===
                Phase10Integration.Universal.CrescendoChargeUp(NPC.Center, HellfireGold, progress);
                
                // Safe zone indicator
                if (Timer > chargeTime / 2 && Timer % 4 == 0)
                {
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    float safeArc = MathHelper.ToRadians(26f - difficultyTier * 2f);
                    
                    // Show safe arc with cyan particles
                    for (int i = 0; i < 8; i++)
                    {
                        float t = (float)i / 8f - 0.5f;
                        float angle = safeAngle + t * safeArc * 2f;
                        Vector2 safePos = NPC.Center + angle.ToRotationVector2() * 150f;
                        
                        // Cyan safe indicator
                        var safe = new BloomParticle(safePos, Vector2.Zero, Color.Cyan * 0.7f, 0.3f, 8);
                        safe.Color = safe.Color with { A = 0 };
                        MagnumParticleHandler.SpawnParticle(safe);
                    }
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    MagnumScreenEffects.AddScreenShake(12f);
                    DiesIraeVFX.FireImpact(NPC.Center, 1.5f);
                }
            }
            else if (SubPhase <= waveCount) // Radial bursts with safe arc - DEVASTATING WAVES
            {
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(18f + SubPhase * 2f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.6f, Pitch = -0.2f + SubPhase * 0.1f }, NPC.Center);
                    
                    // === MASSIVE WAVE EXPLOSION VFX ===
                    DiesIraeVFX.FireImpact(NPC.Center, 1.3f + SubPhase * 0.15f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 28 + difficultyTier * 10 + SubPhase * 2;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(26f - difficultyTier * 2f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = FastProjectileSpeed + SubPhase * 3f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            // Alternate projectile types
                            int projType = i % 4 == 0 ? ProjectileID.CultistBossFireBall : ProjectileID.InfernoHostileBolt;
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                projType, (int)(BaseDamage * 0.4f), 2f);
                        }
                    }
                }
                
                if (Timer >= 30)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 35)
                    EndAttack();
            }
        }
        
        private void Attack_DivinePunishment(Player target)
        {
            // Multi-phase ultimate attack - THE ULTIMATE SPECTACLE
            NPC.velocity *= 0.95f;
            
            if (SubPhase == 0) // Dramatic charge - APOCALYPTIC BUILDUP
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.5f, Volume = 1.8f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("RECEIVE DIVINE PUNISHMENT!", BloodRed);
                }
                
                // === ULTIMATE CHARGE VFX ===
                float progress = Timer / 80f;
                DiesIraeVFX.ChargeUp(NPC.Center, progress, 2.5f);
                
                // Fire aura during charge
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.05f;
                        Vector2 auraPos = NPC.Center + angle.ToRotationVector2() * (80f + progress * 40f);
                        DiesIraeVFX.FireImpact(auraPos, 0.4f);
                    }
                }
                
                if (Timer >= 80)
                {
                    Timer = 0;
                    SubPhase = 1;
                    DiesIraeVFX.FireImpact(NPC.Center, 2f);
                    MagnumScreenEffects.AddScreenShake(20f);
                }
            }
            else if (SubPhase == 1) // Rapid dashes - DEVASTATING ASSAULT
            {
                dashCount = 0;
                Timer = 0;
                SubPhase = 2;
            }
            else if (SubPhase == 2) // Execute dashes
            {
                if (dashCount < 6)
                {
                    if (Timer == 1)
                    {
                        dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        NPC.velocity = dashDirection * BaseSpeed * 2.2f;
                        SoundEngine.PlaySound(SoundID.Item73 with { Pitch = 0.3f, Volume = 1.3f }, NPC.Center);
                        DiesIraeVFX.FireImpact(NPC.Center, 0.9f);
                        MagnumScreenEffects.AddScreenShake(8f);
                    }
                    
                    // INTENSE dash trail
                    for (int i = 0; i < 2; i++)
                    {
                        DiesIraeVFX.FireTrail(NPC.Center + Main.rand.NextVector2Circular(25f, 25f), NPC.velocity, 1.3f);
                    }
                    
                    if (Timer % 2 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                            ProjectileID.Fireball, (int)(BaseDamage * 0.25f), 1f);
                    }
                    
                    if (Timer >= 18)
                    {
                        Timer = 0;
                        dashCount++;
                        NPC.velocity *= 0.25f;
                    }
                }
                else
                {
                    Timer = 0;
                    SubPhase = 3;
                }
            }
            else if (SubPhase == 3) // Final explosion - CATACLYSMIC FINALE
            {
                // Charge up for finale
                float progress = Timer / 25f;
                if (Timer < 25)
                {
                    DiesIraeVFX.ChargeUp(NPC.Center, progress, 2f);
                }
                
                if (Timer == 25)
                {
                    MagnumScreenEffects.AddScreenShake(25f);
                    SoundEngine.PlaySound(SoundID.Item14 with { Volume = 2f, Pitch = -0.3f }, NPC.Center);
                    
                    // === ULTIMATE EXPLOSION VFX ===
                    DiesIraeVFX.DeathExplosion(NPC.Center, 1.2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Massive radial burst - 3 waves
                        for (int wave = 0; wave < 4; wave++)
                        {
                            int count = 24 + wave * 6;
                            float speed = MediumProjectileSpeed + wave * 5f;
                            
                            for (int i = 0; i < count; i++)
                            {
                                float angle = MathHelper.TwoPi * i / count + wave * 0.15f;
                                Vector2 vel = angle.ToRotationVector2() * speed;
                                
                                int projType = wave % 2 == 0 ? ProjectileID.InfernoHostileBolt : ProjectileID.CultistBossFireBall;
                                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vel,
                                    projType, (int)(BaseDamage * 0.35f), 2f);
                            }
                        }
                    }
                }
                
                if (Timer >= 65)
                {
                    Timer = 0;
                    SubPhase = 4;
                }
            }
            else
            {
                if (Timer >= 45)
                    EndAttack();
            }
        }
        
        #endregion
        
        #region Death Animation
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            if (deathTimer == 1)
            {
                SoundEngine.PlaySound(SoundID.NPCDeath14 with { Volume = 1.8f, Pitch = -0.5f }, NPC.Center);
                
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("THE JUDGMENT... IS COMPLETE...", BloodRed);
            }
            
            // === ESCALATING DEATH VFX ===
            float deathProgress = deathTimer / 180f;
            
            // Continuous fire eruptions
            if (deathTimer % 8 == 0)
            {
                DiesIraeVFX.FireImpact(NPC.Center + Main.rand.NextVector2Circular(50f * deathProgress, 50f * deathProgress), 0.6f + deathProgress * 0.8f);
            }
            
            // Fire trail from body
            if (deathTimer % 3 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    DiesIraeVFX.FireTrail(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), Main.rand.NextVector2Circular(3f, 3f), 1f + deathProgress);
                }
            }
            
            // Music notes releasing
            if (deathTimer % 12 == 0)
            {
                Vector2 noteVel = Main.rand.NextVector2CircularEdge(5f, 5f);
                DiesIraeVFX.SpawnMusicNote(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), noteVel, Color.White, 1f);
            }
            
            // Shake during death
            if (deathTimer < 160)
            {
                MagnumScreenEffects.AddScreenShake(deathTimer / 15f);
            }
            
            // === FINAL EXPLOSION ===
            if (deathTimer == 160)
            {
                SoundEngine.PlaySound(SoundID.Item14 with { Volume = 2.5f, Pitch = -0.3f }, NPC.Center);
                MagnumScreenEffects.AddScreenShake(30f);
                
                // MASSIVE death explosion
                DiesIraeVFX.DeathExplosion(NPC.Center, 2f);
                
                // === PHASE 10 MUSICAL VFX: Death Finale - The Herald Falls ===
                Phase10Integration.Universal.DeathFinale(NPC.Center, BloodRed, EmberOrange);
            }
            
            if (deathTimer >= 180)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Drops
        
        public override void OnKill()
        {
            // Set downed flag
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                MoonlightSonataSystem.DownedDiesIrae = true;
                
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
            }
        }
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Treasure bag (Expert+)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DiesIraeTreasureBag>()));
            
            // Non-Expert drops (only when not in Expert mode)
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            
            // Materials - always drop
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfDiesIrae>(), 1, 25, 35));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DiesIraeResonantEnergy>(), 1, 15, 25));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfDiesIrae>(), 1, 3, 5));
            
            // Weapons - one random drop
            int[] weapons = new int[]
            {
                ModContent.ItemType<WrathsCleaver>(),
                ModContent.ItemType<ChainOfJudgment>(),
                ModContent.ItemType<ExecutionersVerdict>(),
                ModContent.ItemType<SinCollector>(),
                ModContent.ItemType<DamnationsCannon>(),
                ModContent.ItemType<ArbitersSentence>(),
                ModContent.ItemType<StaffOfFinalJudgement>(),
                ModContent.ItemType<EclipseOfWrath>(),
                ModContent.ItemType<GrimoireOfCondemnation>()
            };
            notExpert.OnSuccess(ItemDropRule.OneFromOptions(1, weapons));
            
            npcLoot.Add(notExpert);
            
            // Trophy (placeholder)
            npcLoot.Add(ItemDropRule.Common(ItemID.GoldCoin, 1, 10, 15));
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // === AMBIENT FIRE AURA - Constant burning presence ===
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.12f) * 0.15f;
            float pulseWave2 = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.08f + 1f) * 0.1f;
            
            // Spawn ambient fire particles
            if (Main.rand.NextBool(4) && State != BossPhase.Death)
            {
                Vector2 firePos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.5f);
                DiesIraeVFX.FireTrail(firePos, Main.rand.NextVector2Circular(2f, 2f), 0.4f);
            }
            
            // Draw trails during dash attacks - ENHANCED
            if (State == BossPhase.Attack && 
                (CurrentAttack == AttackPattern.CondemnationStrike || 
                 CurrentAttack == AttackPattern.WrathfulDescent ||
                 CurrentAttack == AttackPattern.DivinePunishment))
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                    float progress = (float)i / NPC.oldPos.Length;
                    
                    // Multi-layer trail - White core fading to red to black
                    Color whiteLayer = Color.White * (1f - progress) * 0.4f;
                    Color redLayer = BloodRed * (1f - progress) * 0.6f;
                    Color blackLayer = CharredBlack * (1f - progress) * 0.3f;
                    
                    float trailScale = NPC.scale * (1f - progress * 0.4f);
                    
                    // Black outer
                    spriteBatch.Draw(texture, trailPos, null, blackLayer with { A = 0 }, NPC.rotation, origin, trailScale * 1.1f, SpriteEffects.None, 0f);
                    // Red middle
                    spriteBatch.Draw(texture, trailPos, null, redLayer with { A = 0 }, NPC.rotation, origin, trailScale, SpriteEffects.None, 0f);
                    // White core
                    if (i < NPC.oldPos.Length / 2)
                        spriteBatch.Draw(texture, trailPos, null, whiteLayer with { A = 0 }, NPC.rotation, origin, trailScale * 0.9f, SpriteEffects.None, 0f);
                }
            }
            
            // === BLOOM LAYERS - Multi-layer glow ===
            Color baseGlow = isEnraged ? Crimson : EmberOrange;
            
            // Layer 1: Outer dark red bloom (largest, faintest)
            for (int i = 0; i < 6; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 6f + Main.GameUpdateCount * 0.02f).ToRotationVector2() * (12f * pulse);
                spriteBatch.Draw(texture, drawPos + offset, null, (BloodRed * 0.15f) with { A = 0 }, NPC.rotation, origin, NPC.scale * 1.15f, SpriteEffects.None, 0f);
            }
            
            // Layer 2: Middle orange/crimson bloom
            for (int i = 0; i < 5; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 5f - Main.GameUpdateCount * 0.015f).ToRotationVector2() * (7f * pulseWave2);
                spriteBatch.Draw(texture, drawPos + offset, null, (baseGlow * 0.25f) with { A = 0 }, NPC.rotation, origin, NPC.scale * 1.08f, SpriteEffects.None, 0f);
            }
            
            // Layer 3: Inner white-hot bloom during enrage or attacks
            if (isEnraged || State == BossPhase.Attack)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * (4f * pulse);
                    Color whiteGlow = (Color.White * 0.2f) with { A = 0 };
                    spriteBatch.Draw(texture, drawPos + offset, null, whiteGlow, NPC.rotation, origin, NPC.scale * 1.02f, SpriteEffects.None, 0f);
                }
            }
            
            // Core glow layer
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.TwoPi * i / 4f).ToRotationVector2() * (3f * pulse);
                spriteBatch.Draw(texture, drawPos + offset, null, (baseGlow * 0.35f) with { A = 0 }, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            }
            
            // Main sprite
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            spriteBatch.Draw(texture, drawPos, null, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            // === FIRE OVERLAY during attacks ===
            if (State == BossPhase.Attack && Main.rand.NextBool(6))
            {
                float fireScale = 0.08f + Main.rand.NextFloat(0.04f);
                Vector2 fireOffset = Main.rand.NextVector2Circular(NPC.width * 0.3f, NPC.height * 0.3f);
                Color fireColor = Main.rand.NextBool() ? EmberOrange : Crimson;
                spriteBatch.Draw(texture, drawPos + fireOffset, null, (fireColor * 0.4f) with { A = 0 }, 
                    Main.rand.NextFloat(MathHelper.TwoPi), origin, fireScale, SpriteEffects.None, 0f);
            }
            
            return false;
        }
        
        #endregion
    }
}
