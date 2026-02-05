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
using Terraria.Graphics.Effects;
using MagnumOpus.Content.OdeToJoy.ResonanceEnergies;
using MagnumOpus.Content.OdeToJoy.Projectiles;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.OdeToJoy.Bosses
{
    /// <summary>
    /// ODE TO JOY - CHROMATIC ROSE CONDUCTOR
    /// 
    /// Design Philosophy:
    /// - The divine conductor of nature's symphony, controlling chromatic roses and harmonic growth
    /// - Two-phase boss with dramatic transition at 50% HP
    /// - Phase 1: Graceful, elegant botanical attacks - "On that hallowed ground..."
    /// - Phase 2: Intense chromatic fury - "dost thou melody sing"
    /// 
    /// Unique Mechanic:
    /// - HP locks at 50% for phase transition
    /// - Boss transforms into more aggressive "Garden of Eternal Roses" form
    /// - New music track, intensified attacks, chromatic VFX explosion
    /// 
    /// Musical Theme:
    /// - Phase 1: "On that hallowed ground..." (ethereal, botanical)
    /// - Phase 2: "dost thou melody sing" (intense, triumphant)
    /// </summary>
    public class OdeToJoyChromaticRoseConductor : ModNPC
    {
        // Phase 1 sprite - Chromatic Rose Conductor
        public override string Texture => "MagnumOpus/Content/OdeToJoy/Bosses/OdeToJoyChromaticRoseConductor";
        
        #region Theme Colors - Ode to Joy Palette
        private static readonly Color RosePink = new Color(255, 182, 193);       // Rose pink
        private static readonly Color PetalPink = new Color(255, 105, 180);      // Hot pink
        private static readonly Color GoldenPollen = new Color(255, 215, 0);     // Golden yellow
        private static readonly Color WhiteBloom = new Color(255, 255, 255);     // Pure white
        private static readonly Color LeafGreen = new Color(144, 238, 144);      // Light green
        private static readonly Color ChromaticShift = new Color(255, 200, 220); // Chromatic accent
        #endregion
        
        #region Constants
        // POST-PLANTERA TIER - Balanced difficulty
        private const float BaseSpeed = 14f;
        private const int BaseDamage = 75;
        private const float EnrageDistance = 1100f;
        private const float TeleportDistance = 1600f;
        private const int AttackWindowFrames = 45;
        
        // Projectile speeds
        private const float FastProjectileSpeed = 16f;
        private const float MediumProjectileSpeed = 12f;
        private const float SlowHomingSpeed = 7f;
        
        // Phase 2 multipliers - more intense
        private const float Phase2DamageMult = 1.4f;
        private const float Phase2SpeedMult = 1.6f;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Phase1_Idle,
            Phase1_Attack,
            Phase1_Reposition,
            PhaseTransition,    // HP locks at 50%, dramatic transformation
            Phase2_Awakening,   // Rising as Garden of Eternal Roses
            Phase2_Idle,
            Phase2_Attack,
            Phase2_Reposition,
            Enraged,
            Dying
        }
        
        private enum AttackPattern
        {
            // Phase 1 - Graceful Botanical Attacks
            PetalStorm,           // Sweeping petal projectile arcs
            VineWhip,             // Lashing vine attacks
            RoseBudVolley,        // Rose bud projectiles that bloom on impact
            PollenCloud,          // Area denial golden pollen
            HarmonicBloom,        // Radial flower burst
            
            // Phase 2 - Chromatic Fury
            ChromaticCascade,     // Intense multi-colored petal storm
            ThornyEmbrace,        // Converging vine cage attack
            GardenSymphony,       // Safe-arc radial burst (Hero's Judgment style)
            EternalBloom,         // Massive screen-wide floral explosion
            JubilantFinale,       // Ultimate: chromatic rose conductor's finale
            
            // Both Phases
            Reposition
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
        private bool phase2Started = false;
        private bool isPhase2 = false;
        private int difficultyTier = 0;
        
        private int attackCooldown = 0;
        private AttackPattern lastAttack = AttackPattern.PetalStorm;
        private int consecutiveAttacks = 0;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        // Aggression system
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 3600; // 60 seconds to max aggression
        
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 1; // Update when spritesheet is ready
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        
        // Two-phase music system
        private int phase1MusicSlot = -1;
        private int phase2MusicSlot = -1;
        
        // Phase transition variables
        private int phase1MaxLife;
        private const float PhaseTransitionThreshold = 0.5f; // 50% HP triggers transition
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 160;
            NPC.damage = BaseDamage;
            NPC.defense = 55;
            NPC.lifeMax = 320000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 15);
            NPC.boss = true;
            NPC.npcSlots = 12f;
            NPC.aiStyle = -1;
            NPC.scale = 1.0f;
            
            if (!Main.dedServ)
            {
                // Two-phase music system - "On that hallowed ground..." then "dost thou melody sing"
                phase1MusicSlot = MusicLoader.GetMusicSlot(Mod, "Assets/Music/OdeToJoyBossPhase1");
                phase2MusicSlot = MusicLoader.GetMusicSlot(Mod, "Assets/Music/OdeToJoyBossPhase2");
                
                // Fallback to vanilla boss music if custom tracks don't exist
                if (phase1MusicSlot == -1)
                    phase1MusicSlot = MusicID.Boss4; // Plantera-style as fallback
                if (phase2MusicSlot == -1)
                    phase2MusicSlot = MusicID.Boss2; // Intense boss music fallback
                    
                Music = phase1MusicSlot;
            }
            
            phase1MaxLife = NPC.lifeMax;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.Surface,
                new FlavorTextBestiaryInfoElement(
                    "The Chromatic Rose Conductor - A divine entity born from the harmony of nature's symphony. " +
                    "Her petals dance to an eternal melody, and those who hear her song are blessed with joy... " +
                    "or consumed by the garden's embrace.")
            });
        }

        #region Color Helpers
        private Color GetOdeToJoyGradient(float progress)
        {
            if (progress < 0.25f)
                return Color.Lerp(WhiteBloom, RosePink, progress * 4f);
            else if (progress < 0.5f)
                return Color.Lerp(RosePink, PetalPink, (progress - 0.25f) * 4f);
            else if (progress < 0.75f)
                return Color.Lerp(PetalPink, GoldenPollen, (progress - 0.5f) * 4f);
            else
                return Color.Lerp(GoldenPollen, LeafGreen, (progress - 0.75f) * 4f);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.25f;
        private float GetAggressionRateMult() => 1f - aggressionLevel * 0.15f;
        
        private float GetPhaseSpeedMult() => isPhase2 ? Phase2SpeedMult : 1f;
        private float GetPhaseDamageMult() => isPhase2 ? Phase2DamageMult : 1f;
        #endregion

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.OdeToJoy);
                hasRegisteredHealthBar = true;
            }
            
            if (State == BossPhase.Dying)
            {
                UpdateDeathAnimation();
                return;
            }
            
            if (State == BossPhase.PhaseTransition)
            {
                UpdatePhaseTransition();
                return;
            }
            
            if (State == BossPhase.Phase2_Awakening)
            {
                UpdatePhase2Awakening();
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
            
            // Check for phase transition at 50% HP
            if (!isPhase2 && !phase2Started && (float)NPC.life / NPC.lifeMax <= PhaseTransitionThreshold)
            {
                BeginPhaseTransition();
                return;
            }
            
            UpdateDifficultyTier();
            UpdateAggression();
            if (attackCooldown > 0) attackCooldown--;
            CheckEnrage(target);
            SpawnAmbientParticles();
            
            switch (State)
            {
                case BossPhase.Spawning:
                    AI_Spawning(target);
                    break;
                case BossPhase.Phase1_Idle:
                case BossPhase.Phase2_Idle:
                    AI_Idle(target);
                    break;
                case BossPhase.Phase1_Attack:
                case BossPhase.Phase2_Attack:
                    AI_Attack(target);
                    break;
                case BossPhase.Phase1_Reposition:
                case BossPhase.Phase2_Reposition:
                    AI_Reposition(target);
                    break;
                case BossPhase.Enraged:
                    AI_Enraged(target);
                    break;
            }
            
            Timer++;
            NPC.spriteDirection = NPC.Center.X < target.Center.X ? 1 : -1;
        }
        
        #region Phase Transition System
        
        private void BeginPhaseTransition()
        {
            State = BossPhase.PhaseTransition;
            Timer = 0;
            phase2Started = true;
            NPC.dontTakeDamage = true;
            
            // Lock HP at exactly 50%
            NPC.life = (int)(NPC.lifeMax * PhaseTransitionThreshold);
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("The garden... awakens...", RosePink);
            }
            
            // Dramatic screen shake
            MagnumScreenEffects.AddScreenShake(12f);
            
            // VFX burst
            OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 24, 12f, 2f, true);
        }
        
        private void UpdatePhaseTransition()
        {
            Timer++;
            NPC.velocity *= 0.95f;
            
            // Petal shedding effect (0-90 frames)
            if (Timer <= 90)
            {
                float transitionProgress = Timer / 90f;
                
                // Petals swirling around boss
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + Timer * 0.08f;
                        float radius = 80f + transitionProgress * 40f;
                        Vector2 petalPos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color petalColor = GetOdeToJoyGradient(i / 6f);
                        CustomParticles.GenericFlare(petalPos, petalColor * (1f - transitionProgress * 0.3f), 0.5f, 20);
                    }
                }
                
                // Music fades (handled by Terraria naturally when we switch)
                
                // Rose buds blooming around the boss
                if (Timer % 8 == 0)
                {
                    Vector2 rosePos = NPC.Center + Main.rand.NextVector2Circular(120f, 120f);
                    RoseBudParticle.SpawnBurst(rosePos, 3, 3f, RosePink, GoldenPollen, 0.4f, 30);
                }
            }
            // Silence and darkness (90-120 frames)
            else if (Timer <= 120)
            {
                if (Timer == 91)
                {
                    // Pulse effect
                    MagnumScreenEffects.AddScreenShake(8f);
                    OdeToJoyVFX.OdeToJoySignatureExplosion(NPC.Center, 1.5f);
                }
                
                // Brief fade
                NPC.alpha = Math.Min(200, NPC.alpha + 8);
            }
            // Phase 2 awakening begins (120+)
            else if (Timer == 121)
            {
                BeginPhase2Awakening();
            }
        }
        
        private void BeginPhase2Awakening()
        {
            isPhase2 = true;
            State = BossPhase.Phase2_Awakening;
            Timer = 0;
            
            // HP stays at 50% - this is the remaining fight
            NPC.dontTakeDamage = true;
            
            // Switch music to Phase 2: "dost thou melody sing"
            if (!Main.dedServ)
            {
                Music = phase2MusicSlot;
            }
            
            // Increase stats for Phase 2
            NPC.damage = (int)(BaseDamage * Phase2DamageMult);
            NPC.defense = 70; // Increased defense in Phase 2
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("DOST THOU MELODY SING!", GoldenPollen);
            }
        }
        
        private void UpdatePhase2Awakening()
        {
            Timer++;
            
            // Gradual appearance (0-60 frames)
            if (Timer <= 60)
            {
                float awakeningProgress = Timer / 60f;
                NPC.alpha = (int)(200 * (1f - awakeningProgress));
                
                // Chromatic energy surging
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.1f;
                        Vector2 energyPos = NPC.Center + angle.ToRotationVector2() * (60f * awakeningProgress);
                        
                        Color energyColor = GetOdeToJoyGradient((i / 8f + Timer * 0.02f) % 1f);
                        CustomParticles.GenericFlare(energyPos, energyColor, 0.4f + awakeningProgress * 0.3f, 15);
                    }
                }
                
                // Rose buds blooming everywhere
                if (Timer % 6 == 0)
                {
                    OdeToJoyVFX.RoseBudExplosion(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), 4, 5f, 0.6f);
                }
            }
            // Power surge (60-90 frames)
            else if (Timer <= 90)
            {
                if (Timer == 61)
                {
                    // MASSIVE chromatic explosion
                    OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 32, 14f, 2.5f, true);
                    MagnumScreenEffects.AddScreenShake(15f);
                }
                
                // Radiant glow
                NPC.alpha = 0;
                
                // Petal storm
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 petalPos = NPC.Center + Main.rand.NextVector2Circular(150f, 150f);
                        Vector2 petalVel = (NPC.Center - petalPos).SafeNormalize(Vector2.Zero) * 3f;
                        
                        var petal = new GenericGlowParticle(petalPos, petalVel, GetOdeToJoyGradient(Main.rand.NextFloat()), 0.5f, 30, true);
                        MagnumParticleHandler.SpawnParticle(petal);
                    }
                }
            }
            // Complete awakening
            else
            {
                NPC.dontTakeDamage = false;
                NPC.alpha = 0;
                State = BossPhase.Phase2_Idle;
                Timer = 0;
                attackCooldown = 30;
            }
        }
        
        #endregion
        
        #region Difficulty & Aggression
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            
            // Phase 1: 100-50% (tiers based on this range)
            // Phase 2: 50-0% (tiers based on this range)
            if (!isPhase2)
            {
                // Phase 1 tiers (100-50% HP range)
                float phase1Progress = (hpPercent - 0.5f) / 0.5f; // 1.0 at 100%, 0.0 at 50%
                difficultyTier = phase1Progress > 0.6f ? 0 : (phase1Progress > 0.3f ? 1 : 2);
            }
            else
            {
                // Phase 2 tiers (50-0% HP range)
                float phase2Progress = hpPercent / 0.5f; // 1.0 at 50%, 0.0 at 0%
                difficultyTier = phase2Progress > 0.6f ? 0 : (phase2Progress > 0.3f ? 1 : 2);
            }
        }
        
        private void UpdateAggression()
        {
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private void CheckEnrage(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            
            if (distance > EnrageDistance)
            {
                enrageTimer++;
                if (enrageTimer > 180 && !isEnraged)
                {
                    VFXIntegration.OnBossEnrage("OdeToJoy", NPC.Center);
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    
                    if (Main.netMode != NetmodeID.Server)
                    {
                        Main.NewText("The garden grows restless...", PetalPink);
                    }
                }
            }
            else
            {
                enrageTimer = Math.Max(0, enrageTimer - 2);
                if (isEnraged && enrageTimer == 0)
                {
                    isEnraged = false;
                    State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
                }
            }
            
            if (distance > TeleportDistance)
            {
                TeleportToPlayer(target);
            }
        }
        
        private void TeleportToPlayer(Player target)
        {
            Vector2 teleportPos = target.Center + Main.rand.NextVector2CircularEdge(300f, 300f);
            
            // Departure VFX
            OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 16, 8f, 1f, true);
            
            NPC.Center = teleportPos;
            NPC.velocity = Vector2.Zero;
            
            // Arrival VFX
            OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 16, 8f, 1f, true);
        }
        
        #endregion
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer < 60)
            {
                NPC.alpha = (int)(255 * (1f - Timer / 60f));
                
                // Spawn VFX
                if (Timer % 5 == 0)
                {
                    OdeToJoyVFX.RoseBudExplosion(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), 3, 4f, 0.5f);
                }
            }
            else
            {
                NPC.alpha = 0;
                VFXIntegration.OnBossSpawn("OdeToJoy", NPC.Center);
                State = BossPhase.Phase1_Idle;
                Timer = 0;
                
                if (Main.netMode != NetmodeID.Server)
                {
                    Main.NewText("On that hallowed ground, the roses bloom eternal...", RosePink);
                }
            }
        }
        
        private void AI_Idle(Player target)
        {
            // Gentle hover toward player
            Vector2 hoverTarget = target.Center + new Vector2(0, -250f);
            Vector2 toHover = hoverTarget - NPC.Center;
            
            if (toHover.Length() > 50f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed * 0.5f * GetPhaseSpeedMult(), 0.08f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }
            
            // === PHASE 10 MUSICAL VFX: Ambient Crescendo - Rose Garden's Rhythm ===
            if (Timer % 30 == 0)
            {
                Phase10Integration.Universal.CrescendoChargeUp(NPC.Center, RosePink, 0.4f);
            }
            
            if (attackCooldown <= 0)
            {
                SelectNextAttack(target);
            }
        }
        
        private void AI_Attack(Player target)
        {
            ExecuteCurrentAttack(target);
        }
        
        private void AI_Reposition(Player target)
        {
            Vector2 repositionTarget = target.Center + Main.rand.NextVector2CircularEdge(350f, 250f);
            Vector2 toTarget = repositionTarget - NPC.Center;
            float maxTime = 60f;
            float progress = Timer / maxTime;
            
            if (toTarget.Length() > 50f && Timer < maxTime)
            {
                // Bell curve speed: accelerate then decelerate smoothly
                float speedMult = BossAIUtilities.Easing.EaseOutQuad(progress) * BossAIUtilities.Easing.EaseInQuad(1f - progress) * 4f;
                speedMult = Math.Max(speedMult, 0.15f);
                
                toTarget.Normalize();
                float targetSpeed = BaseSpeed * GetPhaseSpeedMult() * speedMult;
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * targetSpeed, 0.12f);
                
                // VFX: Recovery shimmer - player can identify vulnerability
                if (Timer % 8 == 0)
                    BossVFXOptimizer.RecoveryShimmer(NPC.Center, ChromaticShift, 60f, progress);
                
                // VFX: Deceleration trail during slowdown phase
                if (progress > 0.5f && Timer % 4 == 0)
                    BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, RosePink, progress);
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (NPC.velocity.Length() < 1f || Timer >= maxTime)
                {
                    // VFX: Ready to attack cue - warns player aggression is returning
                    BossVFXOptimizer.ReadyToAttackCue(NPC.Center, PetalPink);
                    
                    State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
                    Timer = 0;
                    attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
                }
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Aggressive pursuit + constant petal projectiles
            Vector2 toTarget = target.Center - NPC.Center;
            toTarget.Normalize();
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 1.8f * GetPhaseSpeedMult(), 0.15f);
            
            // Constant petal barrage
            if (Timer % 10 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 projVel = toTarget * FastProjectileSpeed;
                // Spawn petal projectile (placeholder - implement OdeToJoyPetalProjectile)
                // Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, projVel, ModContent.ProjectileType<OdeToJoyPetalProjectile>(), GetAttackDamage(40), 0f);
            }
            
            // VFX rage effect
            if (Timer % 5 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center + Main.rand.NextVector2Circular(60f, 60f), PetalPink, 0.6f, 15);
            }
        }
        
        #endregion
        
        #region Attack Selection & Execution
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>();
            
            if (!isPhase2)
            {
                // Phase 1 attacks
                pool.Add(AttackPattern.PetalStorm);
                pool.Add(AttackPattern.VineWhip);
                pool.Add(AttackPattern.RoseBudVolley);
                
                if (difficultyTier >= 1)
                {
                    pool.Add(AttackPattern.PollenCloud);
                    pool.Add(AttackPattern.HarmonicBloom);
                }
            }
            else
            {
                // Phase 2 attacks
                pool.Add(AttackPattern.ChromaticCascade);
                pool.Add(AttackPattern.ThornyEmbrace);
                pool.Add(AttackPattern.RoseBudVolley);
                
                if (difficultyTier >= 1)
                {
                    pool.Add(AttackPattern.GardenSymphony);
                    pool.Add(AttackPattern.EternalBloom);
                }
                
                if (difficultyTier >= 2)
                {
                    pool.Add(AttackPattern.JubilantFinale);
                }
            }
            
            // Remove last attack to prevent repetition
            pool.Remove(lastAttack);
            
            if (pool.Count == 0)
            {
                pool.Add(isPhase2 ? AttackPattern.ChromaticCascade : AttackPattern.PetalStorm);
            }
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = isPhase2 ? BossPhase.Phase2_Attack : BossPhase.Phase1_Attack;
        }
        
        private void ExecuteCurrentAttack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.PetalStorm:
                    Attack_PetalStorm(target);
                    break;
                case AttackPattern.VineWhip:
                    Attack_VineWhip(target);
                    break;
                case AttackPattern.RoseBudVolley:
                    Attack_RoseBudVolley(target);
                    break;
                case AttackPattern.PollenCloud:
                    Attack_PollenCloud(target);
                    break;
                case AttackPattern.HarmonicBloom:
                    Attack_HarmonicBloom(target);
                    break;
                case AttackPattern.ChromaticCascade:
                    Attack_ChromaticCascade(target);
                    break;
                case AttackPattern.ThornyEmbrace:
                    Attack_ThornyEmbrace(target);
                    break;
                case AttackPattern.GardenSymphony:
                    Attack_GardenSymphony(target);
                    break;
                case AttackPattern.EternalBloom:
                    Attack_EternalBloom(target);
                    break;
                case AttackPattern.JubilantFinale:
                    Attack_JubilantFinale(target);
                    break;
            }
        }
        
        private void EndAttack()
        {
            consecutiveAttacks++;
            
            // VFX: Attack ending cue - exhale burst with safety ring
            BossVFXOptimizer.AttackEndCue(NPC.Center, RosePink, GoldenPollen);
            
            if (consecutiveAttacks >= 3)
            {
                State = isPhase2 ? BossPhase.Phase2_Reposition : BossPhase.Phase1_Reposition;
                consecutiveAttacks = 0;
            }
            else
            {
                State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
            }
            
            Timer = 0;
            SubPhase = 0;
            attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
        }
        
        private int GetAttackDamage(int baseDamage)
        {
            return (int)(baseDamage * GetPhaseDamageMult() * (1f + difficultyTier * 0.15f));
        }
        
        #endregion
        
        #region Attack Implementations (Placeholder)
        
        private void Attack_PetalStorm(Player target)
        {
            int chargeTime = 45 - difficultyTier * 8;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.95f;
                
                float progress = (float)Timer / chargeTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, RosePink, 8);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Execute
            {
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(6f);
                    OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 18, 10f, 1.2f, true);
                    
                    // TODO: Spawn petal projectiles in arc
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int count = 12 + difficultyTier * 4;
                        float spread = MathHelper.ToRadians(120f);
                        float baseAngle = (target.Center - NPC.Center).ToRotation();
                        
                        for (int i = 0; i < count; i++)
                        {
                            float angle = baseAngle - spread / 2f + spread * i / count;
                            Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed;
                            // Projectile.NewProjectile(...);
                        }
                    }
                }
                
                if (Timer >= 30)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_VineWhip(Player target)
        {
            // Placeholder - implement lashing vine attack
            if (Timer >= 60)
            {
                EndAttack();
            }
        }
        
        private void Attack_RoseBudVolley(Player target)
        {
            int chargeTime = 30 - difficultyTier * 5;
            
            if (SubPhase == 0)
            {
                float progress = (float)Timer / chargeTime;
                
                // Rose buds gathering
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        float angle = Timer * 0.1f + MathHelper.TwoPi * i / 3f;
                        Vector2 offset = angle.ToRotationVector2() * (80f - progress * 30f);
                        RoseBudParticle.SpawnBurst(NPC.Center + offset, 1, 0f, RosePink, WhiteBloom, 0.3f, 20);
                    }
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else
            {
                if (Timer == 1)
                {
                    OdeToJoyVFX.RoseBudExplosion(NPC.Center, 8, 8f, 1f);
                    
                    // TODO: Spawn rose bud projectiles
                }
                
                if (Timer >= 20)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_PollenCloud(Player target)
        {
            // Placeholder - implement area denial pollen cloud
            if (Timer >= 90)
            {
                EndAttack();
            }
        }
        
        private void Attack_HarmonicBloom(Player target)
        {
            // Placeholder - implement radial flower burst
            if (Timer >= 60)
            {
                EndAttack();
            }
        }
        
        private void Attack_ChromaticCascade(Player target)
        {
            // Phase 2 intense petal storm
            int chargeTime = 35 - difficultyTier * 5;
            int waveCount = 3 + difficultyTier;
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.93f;
                float progress = (float)Timer / chargeTime;
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 120f, progress, PetalPink, 12);
                
                // Chromatic energy building
                if (Timer % 3 == 0)
                {
                    Color chromaColor = GetOdeToJoyGradient((Timer * 0.03f) % 1f);
                    CustomParticles.GenericFlare(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), chromaColor, 0.5f, 15);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount)
            {
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(8f);
                    OdeToJoyVFX.ChromaticRosePetalBurst(NPC.Center, 22, 12f, 1.5f, true);
                    
                    // TODO: Spawn chromatic petal wave
                }
                
                if (Timer >= 25)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 20)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_ThornyEmbrace(Player target)
        {
            // Placeholder - implement converging vine cage
            if (Timer >= 120)
            {
                EndAttack();
            }
        }
        
        private void Attack_GardenSymphony(Player target)
        {
            // Hero's Judgment style safe-arc attack
            int chargeTime = 60 - difficultyTier * 10;
            int waveCount = 2 + difficultyTier;
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.95f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f }, NPC.Center);
                    Main.NewText("The garden sings its symphony!", GoldenPollen);
                }
                
                float progress = (float)Timer / chargeTime;
                
                // === PHASE 10 MUSICAL VFX: Crescendo Charge Up - Garden's Harmony Building ===
                Phase10Integration.Universal.CrescendoChargeUp(NPC.Center, GoldenPollen, progress);
                
                // Converging rose petals
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 150f, progress, RosePink, 10);
                TelegraphSystem.ConvergingRing(NPC.Center, 300f, chargeTime, GoldenPollen);
                
                // Safe zone indicator for player
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 80f, 10);
                }
                
                // Rose buds blooming in spiral
                if (Timer % 6 == 0)
                {
                    float spiralAngle = Timer * 0.15f;
                    Vector2 rosePos = NPC.Center + spiralAngle.ToRotationVector2() * (50f + progress * 80f);
                    RoseBudParticle.SpawnBurst(rosePos, 2, 2f, GetOdeToJoyGradient(progress), WhiteBloom, 0.4f, 25);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount)
            {
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(12f);
                    OdeToJoyVFX.OdeToJoySignatureExplosion(NPC.Center, 1.8f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 28 + difficultyTier * 6;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(28f - difficultyTier * 2f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // Safe arc exemption
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 12f + difficultyTier * 2f + SubPhase;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            // TODO: Spawn projectile
                        }
                    }
                    
                    // Cascading rose rings
                    for (int ring = 0; ring < 6; ring++)
                    {
                        Color ringColor = GetOdeToJoyGradient(ring / 6f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.4f + ring * 0.12f, 18 + ring * 3);
                    }
                }
                
                if (Timer >= 40)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 35)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_EternalBloom(Player target)
        {
            // Placeholder - massive screen-wide floral explosion
            if (Timer >= 150)
            {
                EndAttack();
            }
        }
        
        private void Attack_JubilantFinale(Player target)
        {
            // Ultimate attack - chromatic rose conductor's finale
            int chargeTime = 90;
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    Main.NewText("JUBILANT FINALE!", GoldenPollen);
                }
                
                float progress = (float)Timer / chargeTime;
                
                // Massive energy convergence
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 200f, progress, GoldenPollen, 16);
                
                // Rose buds everywhere
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 rosePos = NPC.Center + Main.rand.NextVector2Circular(150f, 150f);
                        OdeToJoyVFX.RoseBudExplosion(rosePos, 3, 4f, 0.5f);
                    }
                }
                
                // Screen shake building
                if (Timer > chargeTime * 0.7f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 8f);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else
            {
                if (Timer == 1)
                {
                    // MASSIVE EXPLOSION
                    MagnumScreenEffects.AddScreenShake(20f);
                    OdeToJoyVFX.DeathExplosion(NPC.Center, 2f);
                    
                    // TODO: Spawn massive projectile storm
                }
                
                if (Timer >= 60)
                {
                    EndAttack();
                }
            }
        }
        
        #endregion
        
        #region Ambient VFX
        
        private void SpawnAmbientParticles()
        {
            // Floating petals around boss
            if (Main.rand.NextBool(8))
            {
                Vector2 petalPos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                Vector2 petalVel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-0.5f, 0.5f));
                
                Color petalColor = GetOdeToJoyGradient(Main.rand.NextFloat());
                var petal = new GenericGlowParticle(petalPos, petalVel, petalColor * 0.7f, 0.35f, 40, true);
                MagnumParticleHandler.SpawnParticle(petal);
            }
            
            // Rose buds occasionally
            if (Main.rand.NextBool(30))
            {
                Vector2 rosePos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                var rose = RoseBudParticle.CreateRandom(rosePos, Main.rand.NextVector2Circular(1f, 1f), RosePink, GoldenPollen, 0.3f, 50, true);
                MagnumParticleHandler.SpawnParticle(rose);
            }
            
            // Phase 2 enhanced particles
            if (isPhase2 && Main.rand.NextBool(12))
            {
                Vector2 chromaPos = NPC.Center + Main.rand.NextVector2Circular(120f, 120f);
                Color chromaColor = GetOdeToJoyGradient((Main.GameUpdateCount * 0.02f) % 1f);
                CustomParticles.GenericFlare(chromaPos, chromaColor, 0.4f, 20);
            }
            
            // Lighting
            float intensity = isPhase2 ? 1.2f : 0.8f;
            Lighting.AddLight(NPC.Center, RosePink.ToVector3() * intensity);
        }
        
        #endregion
        
        #region Death Animation
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            NPC.dontTakeDamage = true;
            
            // Phase 1: Building intensity (0-120 frames)
            if (deathTimer <= 120)
            {
                float intensity = (float)deathTimer / 120f;
                
                // Petals swirling and gathering
                if (deathTimer % 3 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + deathTimer * 0.05f;
                        float radius = 150f * (1f - intensity * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        
                        Color petalColor = GetOdeToJoyGradient(i / 6f);
                        CustomParticles.GenericFlare(pos, petalColor, 0.4f + intensity * 0.4f, 15);
                    }
                }
                
                // Rose buds blooming in finale
                if (deathTimer % 8 == 0)
                {
                    OdeToJoyVFX.RoseBudExplosion(NPC.Center + Main.rand.NextVector2Circular(80f, 80f), 4, 5f, 0.6f);
                }
                
                MagnumScreenEffects.AddScreenShake(intensity * 4f);
            }
            // Phase 2: Climax (120-150 frames)
            else if (deathTimer == 140)
            {
                // FINAL EXPLOSION
                OdeToJoyVFX.DeathExplosion(NPC.Center, 2.5f);
                MagnumScreenEffects.AddScreenShake(25f);
                
                // === PHASE 10 MUSICAL VFX: Death Finale - The Garden's Final Song ===
                Phase10Integration.Universal.DeathFinale(NPC.Center, WhiteBloom, RosePink);
                VFXIntegration.OnBossDeath("OdeToJoy", NPC.Center);
                
                if (Main.netMode != NetmodeID.Server)
                {
                    Main.NewText("The eternal melody... fades...", GoldenPollen);
                }
            }
            // Phase 3: Fade out (150+ frames)
            else if (deathTimer > 150)
            {
                NPC.alpha += 5;
                
                // Final petal scatter
                if (deathTimer % 5 == 0 && NPC.alpha < 255)
                {
                    Vector2 scatterVel = Main.rand.NextVector2Circular(8f, 8f);
                    var petal = new GenericGlowParticle(NPC.Center, scatterVel, GetOdeToJoyGradient(Main.rand.NextFloat()), 0.5f, 40, true);
                    MagnumParticleHandler.SpawnParticle(petal);
                }
                
                if (NPC.alpha >= 255)
                {
                    NPC.life = 0;
                    NPC.HitEffect();
                    NPC.checkDead();
                }
            }
        }
        
        public override bool CheckDead()
        {
            if (State != BossPhase.Dying && deathTimer == 0)
            {
                State = BossPhase.Dying;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                return false;
            }
            
            return deathTimer > 180;
        }
        
        #endregion
        
        #region Drawing
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Trail afterimages
            for (int i = 0; i < NPC.oldPos.Length - 1; i++)
            {
                float progress = (float)i / NPC.oldPos.Length;
                float alpha = (1f - progress) * 0.3f;
                Color trailColor = Color.Lerp(RosePink, GoldenPollen, progress) * alpha;
                
                Vector2 drawPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                
                Texture2D texture = Terraria.GameContent.TextureAssets.Npc[Type].Value;
                Rectangle frame = NPC.frame;
                Vector2 origin = frame.Size() / 2f;
                
                spriteBatch.Draw(texture, drawPos, frame, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.2f), NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            
            return true;
        }
        
        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Bloom glow overlay
            Texture2D glowTex = Terraria.GameContent.TextureAssets.Npc[Type].Value;
            Vector2 drawPos = NPC.Center - screenPos;
            Rectangle frame = NPC.frame;
            Vector2 origin = frame.Size() / 2f;
            
            float pulse = 1f + (float)Math.Sin(Main.GameUpdateCount * 0.05f) * 0.1f;
            Color glowColor = (isPhase2 ? GoldenPollen : RosePink) * 0.3f;
            glowColor.A = 0;
            
            spriteBatch.Draw(glowTex, drawPos, frame, glowColor, NPC.rotation, origin, NPC.scale * pulse * 1.1f, NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
        }
        
        #endregion
        
        #region Animation
        
        public override void FindFrame(int frameHeight)
        {
            frameCounter++;
            if (frameCounter >= 8)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
            
            NPC.frame.Y = currentFrame * frameHeight;
        }
        
        #endregion
        
        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Resonance Energy
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OdeToJoyResonantEnergy>(), 1, 20, 30));
            
            // Treasure bag (Expert/Master)
            // npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<OdeToJoyTreasureBag>()));
            
            // TODO: Add weapon drops, accessories, etc.
        }
        
        public override void OnKill()
        {
            // Deactivate any sky effects
            // Set downed flag
            // etc.
        }
        
        #endregion
    }
}
