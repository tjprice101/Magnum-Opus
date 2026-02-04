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
using MagnumOpus.Content.Eroica.ResonanceEnergies;
using MagnumOpus.Content.Eroica.Projectiles;
using MagnumOpus.Content.Eroica.ResonantWeapons;
using MagnumOpus.Content.Eroica.Pets;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.Eroica.Bosses
{
    /// <summary>
    /// EROICA, GOD OF VALOR - POST-MOON LORD BOSS
    /// 
    /// Design Philosophy (Vanilla-Inspired):
    /// - Phase 1: Kill the 3 Flames of Valor first (like Cultist ritual mechanic)
    /// - Phase 2: Main fight with clear attack telegraphs and windows
    /// - Attacks are READABLE but PUNISHING if not dodged
    /// - Inspired by: Moon Lord's overwhelming presence, Duke Fishron's aggression, 
    ///   Empress of Light's elegant projectile patterns
    /// 
    /// Key Vanilla Principles Applied:
    /// - Telegraphed attacks with clear visual cues
    /// - Attack cooldown windows for player to deal damage
    /// - Escalating difficulty as HP drops (faster/more projectiles)
    /// - Signature moves that define the boss identity
    /// - Enrage mechanic for running too far
    /// </summary>
    public class EroicasRetribution : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/Eroica/Bosses/EroicaGodOfValor";
        
        #region Theme Colors
        private static readonly Color EroicaGold = new Color(255, 200, 80);
        private static readonly Color EroicaScarlet = new Color(200, 50, 50);
        private static readonly Color EroicaCrimson = new Color(180, 30, 60);
        private static readonly Color EroicaWhite = new Color(255, 240, 220);
        private static readonly Color SakuraPink = new Color(255, 150, 180);
        #endregion
        
        #region Constants
        // DIFFICULTY TUNING: Players have wings with 0.5s dash cooldown - attacks must be AGGRESSIVE
        private const float BaseSpeed = 16f;  // Faster base movement (was 12f)
        private const int BaseDamage = 95;
        private const float EnrageDistance = 1000f;  // Tighter arena (was 1200f)
        private const float TeleportDistance = 1500f; // Teleport sooner (was 1800f)
        private const int AttackWindowFrames = 35;   // Less downtime (was 90)
        
        // Projectile speed constants - calibrated for wing dash players
        private const float FastProjectileSpeed = 18f;   // Fast enough players must react
        private const float MediumProjectileSpeed = 14f; // Standard threatening speed
        private const float SlowHomingSpeed = 8f;        // Slow but tracking
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Phase1_FlamesAlive,
            Phase1_Transition,
            Phase2_Idle,
            Phase2_Attack,
            Phase2_Reposition,
            Enraged,
            Dying
        }
        
        private enum AttackPattern
        {
            // Core Attacks
            SwordDash,
            HeroicBarrage,
            GoldenRain,
            
            // Phase 2B (Below 70% HP)
            ValorCross,
            SakuraStorm,
            TriumphantCharge,
            
            // Phase 2C (Below 40% HP)
            PhoenixDive,
            HeroesJudgment,
            UltimateValor
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
        private bool flamesSpawned = false;
        private bool phase2Started = false;
        private int difficultyTier = 0;
        
        private int attackCooldown = 0;
        private AttackPattern lastAttack = AttackPattern.SwordDash;
        private int consecutiveAttacks = 0;
        
        private int dashCount = 0;
        private Vector2 dashTarget;
        private Vector2 dashDirection;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        // Aggression system - boss gets more aggressive over time
        private int fightTimer = 0;
        private float aggressionLevel = 0f; // 0 to 1, increases over fight duration
        private const int MaxAggressionTime = 2520; // 60 seconds to reach max aggression
        
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int TotalFrames = 36;
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of visual size (269x174 frame × 0.65 scale)
            NPC.width = 140;
            NPC.height = 90;
            NPC.damage = BaseDamage;
            NPC.defense = 80;
            NPC.lifeMax = 450000;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 20);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            NPC.scale = 0.65f;
            NPC.dontTakeDamage = true;
            
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/CrownOfEroica");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Eroica, God of Valor - the triumphant deity of heroic sacrifice and eternal glory.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Eroica);
                hasRegisteredHealthBar = true;
            }
            
            if (State == BossPhase.Dying)
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
            
            if (!flamesSpawned && Main.netMode != NetmodeID.MultiplayerClient)
            {
                SpawnFlamesOfValor();
                flamesSpawned = true;
            }
            
            if (!phase2Started && flamesSpawned)
            {
                if (CountAliveFlames() == 0)
                {
                    State = BossPhase.Phase1_Transition;
                    Timer = 0;
                    phase2Started = true;
                    NPC.dontTakeDamage = false;
                }
            }
            
            UpdateDifficultyTier();
            UpdateAggression();
            if (attackCooldown > 0) attackCooldown--;
            CheckEnrage(target);
            
            switch (State)
            {
                case BossPhase.Phase1_FlamesAlive:
                    AI_Phase1_FlamesAlive(target);
                    break;
                case BossPhase.Phase1_Transition:
                    AI_Phase1_Transition(target);
                    break;
                case BossPhase.Phase2_Idle:
                    AI_Phase2_Idle(target);
                    break;
                case BossPhase.Phase2_Attack:
                    AI_Phase2_Attack(target);
                    break;
                case BossPhase.Phase2_Reposition:
                    AI_Phase2_Reposition(target);
                    break;
                case BossPhase.Enraged:
                    AI_Enraged(target);
                    break;
            }
            
            Timer++;
            UpdateAnimation();
            SpawnAmbientParticles();
            
            // Boss dialogue system - combat taunts and player HP checks
            if (phase2Started)
            {
                // Dialogue triggers at HP thresholds only
                BossDialogueSystem.CheckPlayerLowHP(target, "Eroica");
            }
            
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            
            float lightIntensity = isEnraged ? 1.2f : 0.8f;
            Lighting.AddLight(NPC.Center, EroicaGold.ToVector3() * lightIntensity);
        }
        
        #region Phase 1
        
        private void SpawnFlamesOfValor()
        {
            float[] offsets = { 0f, MathHelper.TwoPi / 3f, MathHelper.TwoPi * 2f / 3f };
            for (int i = 0; i < 3; i++)
            {
                int flameIndex = NPC.NewNPC(NPC.GetSource_FromAI(), (int)NPC.Center.X, (int)NPC.Center.Y,
                    ModContent.NPCType<FlamesOfValor>());
                if (flameIndex < Main.maxNPCs && Main.npc[flameIndex].ModNPC is FlamesOfValor flame)
                {
                    flame.SetOrbitOffset(offsets[i]);
                }
            }
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("The Flames of Valor ignite around Eroica...", EroicaGold);
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.2f }, NPC.Center);
            }
            
            for (int i = 0; i < 3; i++)
            {
                float angle = offsets[i];
                Vector2 flamePos = NPC.Center + angle.ToRotationVector2() * 180f;
                CustomParticles.GenericFlare(flamePos, EroicaScarlet, 0.8f, 20);
                ThemedParticles.SakuraPetals(flamePos, 8, 40f);
            }
        }
        
        private int CountAliveFlames()
        {
            int count = 0;
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<FlamesOfValor>())
                    count++;
            }
            return count;
        }
        
        private void AI_Phase1_FlamesAlive(Player target)
        {
            float hoverHeight = -300f;
            float waveX = (float)Math.Sin(Timer * 0.02f) * 80f;
            float waveY = (float)Math.Sin(Timer * 0.015f) * 40f;
            
            Vector2 hoverPos = target.Center + new Vector2(waveX, hoverHeight + waveY);
            Vector2 toHover = hoverPos - NPC.Center;
            
            if (toHover.Length() > 30f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 8f, 0.04f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }
            
            if (Timer % 180 == 0 && Timer > 60 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = (target.Center - NPC.Center).ToRotation() + MathHelper.ToRadians(-15 + i * 15);
                    Vector2 vel = angle.ToRotationVector2() * 6f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 60, EroicaGold, 0f);
                }
                
                CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.5f, 15);
                SoundEngine.PlaySound(SoundID.Item8, NPC.Center);
            }
        }
        
        private void AI_Phase1_Transition(Player target)
        {
            NPC.velocity *= 0.95f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
                MagnumScreenEffects.AddScreenShake(12f);
            }
            
            if (Timer < 90 && Timer % 5 == 0)
            {
                float progress = Timer / 90f;
                int particleCount = (int)(4 + progress * 8);
                
                for (int i = 0; i < particleCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.03f;
                    Vector2 offset = angle.ToRotationVector2() * (100f - progress * 60f);
                    Color color = Color.Lerp(EroicaScarlet, EroicaGold, progress);
                    CustomParticles.GenericFlare(NPC.Center + offset, color, 0.3f + progress * 0.3f, 15);
                }
                
                ThemedParticles.SakuraPetals(NPC.Center, (int)(3 + progress * 5), 50f + progress * 30f);
            }
            
            if (Timer == 60)
            {
                Main.NewText("All Flames extinguished... Eroica awakens!", EroicaCrimson);
            }
            
            if (Timer == 90)
            {
                MagnumScreenEffects.AddScreenShake(20f);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 1.5f, 25);
                for (int i = 0; i < 12; i++)
                {
                    float scale = 0.3f + i * 0.15f;
                    Color ringColor = Color.Lerp(EroicaScarlet, EroicaGold, i / 12f);
                    CustomParticles.HaloRing(NPC.Center, ringColor, scale, 20 + i * 3);
                }
                
                ThemedParticles.SakuraPetals(NPC.Center, 40, 150f);
            }
            
            if (Timer >= 120)
            {
                Timer = 0;
                State = BossPhase.Phase2_Idle;
                attackCooldown = AttackWindowFrames;
            }
        }
        
        #endregion
        
        #region Phase 2
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : 2);
            
            if (newTier != difficultyTier && phase2Started)
            {
                difficultyTier = newTier;
                AnnounceDifficultyChange();
            }
        }
        
        private void AnnounceDifficultyChange()
        {
            MagnumScreenEffects.AddScreenShake(difficultyTier == 2 ? 18f : 12f);
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = difficultyTier * 0.1f }, NPC.Center);
            
            CustomParticles.GenericFlare(NPC.Center, Color.White, 1.0f, 20);
            for (int i = 0; i < 8 + difficultyTier * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (8 + difficultyTier * 4);
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 60f, 
                    difficultyTier == 2 ? EroicaCrimson : EroicaGold, 0.6f, 18);
            }
            
            ThemedParticles.SakuraPetals(NPC.Center, 25 + difficultyTier * 10, 100f);
            
            // Use dialogue system for phase transitions
            if (difficultyTier == 1)
                BossDialogueSystem.Eroica.OnPhase2(NPC.whoAmI);
            else if (difficultyTier == 2)
                BossDialogueSystem.Eroica.OnPhase3(NPC.whoAmI);
        }
        
        private void CheckEnrage(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            
            // Teleport to player if way too far
            if (distance > TeleportDistance && phase2Started)
            {
                TeleportToPlayer(target);
                return;
            }
            
            if (distance > EnrageDistance)
            {
                enrageTimer++;
                if (enrageTimer > 90 && !isEnraged && phase2Started)
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    
                    BossDialogueSystem.Eroica.OnEnrage();
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 1.5f }, NPC.Center);
                }
            }
            else
            {
                enrageTimer = Math.Max(0, enrageTimer - 3);
                if (isEnraged && enrageTimer == 0)
                {
                    isEnraged = false;
                    if (State == BossPhase.Enraged)
                    {
                        State = BossPhase.Phase2_Idle;
                        Timer = 0;
                    }
                }
            }
        }
        
        private void TeleportToPlayer(Player target)
        {
            // VFX at departure
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f, Volume = 1.2f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, EroicaGold, 1.0f, 20);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 40f, EroicaScarlet, 0.5f, 15);
            }
            ThemedParticles.SakuraPetals(NPC.Center, 15, 60f);
            
            // Teleport to a position near the player
            Vector2 teleportOffset = Main.rand.NextVector2CircularEdge(350f, 350f);
            NPC.Center = target.Center + teleportOffset;
            NPC.velocity = Vector2.Zero;
            
            // VFX at arrival
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, Color.White, 1.2f, 25);
            CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.9f, 22);
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaScarlet, EroicaGold, i / 6f), 0.3f + i * 0.1f, 15 + i * 2);
            }
            ThemedParticles.SakuraPetals(NPC.Center, 20, 80f);
            
            // Brief invulnerability after teleport
            State = BossPhase.Phase2_Reposition;
            Timer = 0;
            attackCooldown = 30;
            
            MagnumScreenEffects.AddScreenShake(8f);
        }
        
        private void UpdateAggression()
        {
            if (!phase2Started) return;
            
            fightTimer++;
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        // Get speed multiplier based on aggression - MORE AGGRESSIVE scaling
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.6f + difficultyTier * 0.15f;
        
        // Get attack rate multiplier based on aggression - FASTER attacks over time
        private float GetAggressionRateMult() => Math.Max(0.5f, 1f - aggressionLevel * 0.4f - difficultyTier * 0.1f);
        
        private void AI_Phase2_Idle(Player target)
        {
            // Boss gets closer to player as aggression increases
            float baseDist = 350f - aggressionLevel * 100f; // 350 ↁE250 as aggression builds
            float hoverDist = baseDist + difficultyTier * 30f;
            Vector2 idealPos = target.Center + new Vector2(NPC.Center.X > target.Center.X ? hoverDist : -hoverDist, -100f);
            
            Vector2 toIdeal = idealPos - NPC.Center;
            if (toIdeal.Length() > 50f)
            {
                toIdeal.Normalize();
                float speed = BaseSpeed * (1f + difficultyTier * 0.15f) * GetAggressionSpeedMult();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * speed, 0.06f + aggressionLevel * 0.04f);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            
            // Attack cooldown scales with aggression
            int effectiveCooldown = (int)(attackCooldown * GetAggressionRateMult());
            if (effectiveCooldown <= 0 && Timer > (int)(30 * GetAggressionRateMult()))
            {
                SelectNextAttack(target);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.SwordDash,
                AttackPattern.HeroicBarrage,
                AttackPattern.GoldenRain
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.ValorCross);
                pool.Add(AttackPattern.SakuraStorm);
                pool.Add(AttackPattern.TriumphantCharge);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.PhoenixDive);
                pool.Add(AttackPattern.HeroesJudgment);
                
                if (consecutiveAttacks >= 4 && Main.rand.NextBool(3))
                {
                    pool.Add(AttackPattern.UltimateValor);
                }
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Phase2_Attack;
            consecutiveAttacks++;
            
            if (CurrentAttack == AttackPattern.SwordDash || CurrentAttack == AttackPattern.TriumphantCharge)
            {
                dashCount = 0;
            }
        }
        
        private void AI_Phase2_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.SwordDash:
                    Attack_SwordDash(target);
                    break;
                case AttackPattern.HeroicBarrage:
                    Attack_HeroicBarrage(target);
                    break;
                case AttackPattern.GoldenRain:
                    Attack_GoldenRain(target);
                    break;
                case AttackPattern.ValorCross:
                    Attack_ValorCross(target);
                    break;
                case AttackPattern.SakuraStorm:
                    Attack_SakuraStorm(target);
                    break;
                case AttackPattern.TriumphantCharge:
                    Attack_TriumphantCharge(target);
                    break;
                case AttackPattern.PhoenixDive:
                    Attack_PhoenixDive(target);
                    break;
                case AttackPattern.HeroesJudgment:
                    Attack_HeroesJudgment(target);
                    break;
                case AttackPattern.UltimateValor:
                    Attack_UltimateValor(target);
                    break;
            }
        }
        
        private void AI_Phase2_Reposition(Player target)
        {
            float idealDist = 400f;
            Vector2 toTarget = (target.Center - NPC.Center);
            float currentDist = toTarget.Length();
            
            if (Math.Abs(currentDist - idealDist) < 100f && Timer > 30)
            {
                // Spawn ready to attack cue when entering idle
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, EroicaGold, 0.7f);
                Timer = 0;
                State = BossPhase.Phase2_Idle;
                attackCooldown = AttackWindowFrames / 2;
                return;
            }
            
            // Use smooth easing for repositioning movement
            float repositionProgress = Timer / 90f;
            float easedSpeed = BossAIUtilities.Easing.Apply(repositionProgress, 15f, 8f, BossAIUtilities.Easing.EaseOutQuad);
            
            Vector2 idealDir = currentDist > idealDist ? -toTarget.SafeNormalize(Vector2.Zero) : toTarget.SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, idealDir * easedSpeed, 0.08f);
            
            // Recovery shimmer during reposition (signals vulnerability)
            BossVFXOptimizer.RecoveryShimmer(NPC.Center, EroicaGold, 50f, repositionProgress);
            
            if (Timer > 90)
            {
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, EroicaGold, 0.7f);
                Timer = 0;
                State = BossPhase.Phase2_Idle;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            float enrageSpeed = BaseSpeed * 2f;
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * enrageSpeed, 0.12f);
            
            int fireRate = 15 - difficultyTier * 3;
            if (Timer % fireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Timer * 0.1f;
                    Vector2 vel = angle.ToRotationVector2() * 10f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 80, EroicaCrimson, 0f);
                }
                
                CustomParticles.GenericFlare(NPC.Center, EroicaCrimson, 0.5f, 10);
            }
            
            if (Timer % 3 == 0)
            {
                Vector2 particlePos = NPC.Center + Main.rand.NextVector2Circular(40f, 40f);
                CustomParticles.GenericFlare(particlePos, EroicaCrimson, 0.4f, 12);
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_SwordDash(Player target)
        {
            // DIFFICULTY: More dashes, faster, less warning
            int maxDashes = 3 + difficultyTier; // More dashes (was 2+)
            int telegraphTime = 17 - difficultyTier * 3; // Less warning (was 35-)
            int dashDuration = 7; // Faster dash (was 12)
            int recoveryTime = 7 - difficultyTier * 1; // Less recovery (was 15-)
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.92f;
                
                if (Timer == 1)
                {
                    // PREDICT player position more aggressively
                    dashTarget = target.Center + target.velocity * 15f; // More prediction (was 10f)
                    dashDirection = (dashTarget - NPC.Center).SafeNormalize(Vector2.Zero);
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f }, NPC.Center);
                }
                
                // WARNING LINE - optimized, shows trajectory clearly
                if (Timer > 3 && Timer % 2 == 0)
                {
                    float lineLength = 600f + difficultyTier * 120f; // Longer warning
                    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, lineLength, 12, WarningType.Danger);
                }
                
                // Charging particles - optimized
                if (Timer % 4 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 60f, Timer / (float)telegraphTime, EroicaGold, 6);
                }
                
                if (Timer >= telegraphTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                if (Timer == 1)
                {
                    // DIFFICULTY: Much faster dash speed
                    float dashSpeed = 42f + difficultyTier * 8f; // Much faster (was 32f+6f)
                    NPC.velocity = dashDirection * dashSpeed;
                    
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.2f }, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 0.8f);
                    
                    // Spawn side projectiles on dash start for pressure
                    if (Main.netMode != NetmodeID.MultiplayerClient && difficultyTier >= 1)
                    {
                        Vector2 perp = dashDirection.RotatedBy(MathHelper.PiOver2);
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, perp * 6f, 65, EroicaScarlet, 0.02f);
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, -perp * 6f, 65, EroicaScarlet, 0.02f);
                    }
                }
                
                // Optimized trail particles
                if (Timer % 3 == 0)
                {
                    BossVFXOptimizer.ProjectileTrail(NPC.Center, NPC.velocity, EroicaGold);
                    BossVFXOptimizer.OptimizedThemedParticles(NPC.Center - NPC.velocity * 0.2f, "sakura", 2, 15f);
                }
                
                if (Timer >= dashDuration)
                {
                    Timer = 0;
                    dashCount++;
                    SubPhase = dashCount >= maxDashes ? 3 : 2;
                }
            }
            else if (SubPhase == 2)
            {
                NPC.velocity *= 0.85f;
                
                if (Timer == 5)
                {
                    dashTarget = target.Center + target.velocity * 8f;
                    dashDirection = (dashTarget - NPC.Center).SafeNormalize(Vector2.Zero);
                }
                
                if (Timer >= recoveryTime)
                {
                    Timer = 0;
                    SubPhase = 0;
                }
            }
            else
            {
                // Smooth deceleration with visual trail
                float decelProgress = Timer / 14f;
                float decelMult = 1f - BossAIUtilities.Easing.EaseOutCubic(decelProgress);
                NPC.velocity *= 0.9f + decelMult * 0.05f;
                
                // Deceleration trail particles
                BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, EroicaGold, decelProgress);
                
                if (Timer >= 14)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_HeroicBarrage(Player target)
        {
            // DIFFICULTY: More waves, faster, tighter timing
            int totalWaves = 4 + difficultyTier; // More waves (was 3+)
            int waveDelay = (int)((13 - difficultyTier * 2) * GetAggressionRateMult()); // Faster waves (was 25-4)
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase < totalWaves)
            {
                int chargeTime = 8; // Shorter charge
                if (Timer < chargeTime)
                {
                    // WARNING: Show spread pattern where projectiles will go
                    if (Timer % 3 == 0)
                    {
                        Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                        float baseAngle = toPlayer.ToRotation();
                        float totalSpread = MathHelper.ToRadians(80 + difficultyTier * 10);
                        
                        // Show warning lines for each projectile direction
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = baseAngle - totalSpread / 2f + totalSpread * i / 7f;
                            BossVFXOptimizer.WarningLine(NPC.Center, angle.ToRotationVector2(), 300f, 6, WarningType.Caution);
                        }
                    }
                    
                    // Optimized charge particles
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 50f, Timer / (float)chargeTime, EroicaScarlet, 5);
                }
                
                if (Timer == chargeTime && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    float baseAngle = toPlayer.ToRotation();
                    
                    // DIFFICULTY: More projectiles, faster, wider spread
                    int projectileCount = 9 + difficultyTier * 3; // More projectiles (was 7+2)
                    float totalSpread = MathHelper.ToRadians(80 + difficultyTier * 10); // Wider spread
                    
                    bool useHomingPattern = SubPhase % 2 == 1;
                    
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = baseAngle - totalSpread / 2f + totalSpread * i / (projectileCount - 1);
                        float baseSpeed = FastProjectileSpeed + difficultyTier * 3f + aggressionLevel * 5f; // MUCH faster
                        Vector2 vel = angle.ToRotationVector2() * baseSpeed;
                        Color orbColor = i % 2 == 0 ? EroicaScarlet : EroicaGold;
                        
                        if (useHomingPattern && i % 3 == 0)
                        {
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.6f, 70 + difficultyTier * 5, orbColor, 0.05f);
                        }
                        else
                        {
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 70 + difficultyTier * 5, orbColor, 15f);
                        }
                    }
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 0.7f);
                    SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.1f }, NPC.Center);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 21)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_GoldenRain(Player target)
        {
            // DIFFICULTY: Longer duration, faster fire rate, tracks player
            int duration = (int)((105 + difficultyTier * 35) * GetAggressionRateMult()); // Longer (was 120+40)
            int fireInterval = Math.Max(1, (int)((6 - difficultyTier * 1) * GetAggressionRateMult())); // Faster (was 12-2)
            
            // Boss tracks player horizontally while hovering
            Vector2 hoverPos = target.Center + new Vector2(0, -350f); // Closer (was -400f)
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 30f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 14f * GetAggressionSpeedMult(), 0.08f);
            }
            
            // WARNING: Show where rain will fall - CLEAR impact zones
            if (Timer % 12 == 0)
            {
                for (int i = 0; i < 4 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -100f);
                    BossVFXOptimizer.GroundImpactWarning(warningPos, 40f, 0.5f);
                    
                    // Also show spawn point
                    Vector2 spawnWarning = target.Center + new Vector2(xOffset, -550f);
                    BossVFXOptimizer.WarningFlare(spawnWarning, 0.6f, WarningType.Caution);
                }
            }
            
            if (Timer % fireInterval == 0 && Timer > 20 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // DIFFICULTY: More projectiles per volley
                int count = 3 + difficultyTier; // More (was 2+)
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f); // Wider spread
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -550f);
                    
                    // DIFFICULTY: Faster fall speed, some aim at player
                    Vector2 toPlayerDir = (target.Center - spawnPos).SafeNormalize(Vector2.UnitY);
                    float ySpeed = FastProjectileSpeed + difficultyTier * 4f + aggressionLevel * 5f; // MUCH faster
                    
                    // Some projectiles aim toward player, some fall straight
                    Vector2 vel;
                    if (i % 3 == 0)
                    {
                        vel = toPlayerDir * ySpeed * 0.9f; // Aimed at player
                    }
                    else
                    {
                        vel = new Vector2(Main.rand.NextFloat(-3f, 3f), ySpeed); // Straight down
                    }
                    
                    if (i % 2 == 0)
                    {
                        BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.7f, 75 + difficultyTier * 5, EroicaGold, 25f);
                    }
                    else
                    {
                        BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 75 + difficultyTier * 5, EroicaScarlet, 0.02f);
                    }
                    
                    BossVFXOptimizer.OptimizedFlare(spawnPos, EroicaGold, 0.35f, 8, 2);
                }
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        private void Attack_ValorCross(Player target)
        {
            // DIFFICULTY: More patterns, faster, tighter timing
            int patterns = 3 + difficultyTier; // More patterns (was 2+)
            int patternDelay = 25 - difficultyTier * 4; // Faster (was 50-8)
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase < patterns)
            {
                int telegraphTime = 13; // Shorter telegraph (was 25)
                if (Timer < telegraphTime)
                {
                    // WARNING: Show 8-arm pattern clearly
                    if (Timer % 2 == 0)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            float angle = MathHelper.PiOver4 * i + SubPhase * MathHelper.PiOver4 * 0.5f;
                            BossVFXOptimizer.WarningLine(NPC.Center, angle.ToRotationVector2(), 400f, 8, WarningType.Danger);
                        }
                    }
                }
                
                if (Timer == telegraphTime && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // DIFFICULTY: Faster projectiles, more per arm
                    float baseSpeed = FastProjectileSpeed + difficultyTier * 4f + aggressionLevel * 5f; // MUCH faster
                    int projectilesPerArm = 5 + difficultyTier; // More per arm (was 4+)
                    
                    for (int arm = 0; arm < 8; arm++)
                    {
                        float armAngle = MathHelper.PiOver4 * arm + SubPhase * MathHelper.PiOver4 * 0.5f;
                        
                        for (int p = 0; p < projectilesPerArm; p++)
                        {
                            float speed = baseSpeed + p * 2.5f;
                            Vector2 vel = armAngle.ToRotationVector2() * speed;
                            Color color = arm % 2 == 0 ? EroicaGold : EroicaScarlet;
                            
                            // More homing on outer projectiles
                            float homing = p >= projectilesPerArm - 2 ? 0.03f : 0f;
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 70, color, 12f + p * 2f);
                        }
                    }
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 0.9f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, NPC.Center);
                }
                
                if (Timer >= patternDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 21)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_SakuraStorm(Player target)
        {
            // DIFFICULTY: Longer, more arms, faster spin, tighter orbit
            int duration = (int)((91 + difficultyTier * 28) * GetAggressionRateMult()); // Longer (was 100+30)
            int arms = 4 + difficultyTier; // More arms (was 3+)
            
            // DIFFICULTY: Faster orbit, gets closer over time
            float spinSpeed = (0.028f + difficultyTier * 0.008f) * GetAggressionSpeedMult(); // Faster (was 0.02+0.005)
            Vector2 orbitCenter = target.Center;
            float baseRadius = 280f - aggressionLevel * 60f; // Tighter orbit (was 350f-50f)
            float angle = Timer * spinSpeed;
            
            Vector2 idealPos = orbitCenter + angle.ToRotationVector2() * baseRadius;
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 16f * GetAggressionSpeedMult(), 0.1f);
            
            // WARNING: Show next spiral arm direction
            if (Timer % 8 == 0)
            {
                float nextSpiralAngle = (Timer + 8) * 0.2f;
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = nextSpiralAngle + MathHelper.TwoPi * arm / arms;
                    BossVFXOptimizer.WarningFlare(NPC.Center + armAngle.ToRotationVector2() * 50f, 0.5f, WarningType.Caution);
                }
            }
            
            // DIFFICULTY: Faster fire rate, faster projectiles
            int fireInterval = Math.Max(2, (int)((4 - difficultyTier) * GetAggressionRateMult())); // Faster (was 6-)
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float spiralAngle = Timer * 0.2f; // Faster spiral (was 0.15f)
                
                for (int arm = 0; arm < arms; arm++)
                {
                    float armAngle = spiralAngle + MathHelper.TwoPi * arm / arms;
                    float speed = FastProjectileSpeed + difficultyTier * 4f + aggressionLevel * 5f; // MUCH faster
                    Vector2 vel = armAngle.ToRotationVector2() * speed;
                    
                    if (arm % 2 == 0)
                    {
                        BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 65 + difficultyTier * 5, SakuraPink, 4f);
                    }
                    else
                    {
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 65 + difficultyTier * 5, EroicaGold, 18f);
                    }
                }
                
                BossVFXOptimizer.OptimizedFlare(NPC.Center, SakuraPink, 0.35f, 8, 2);
            }
            
            // Optimized ambient petals
            if (Timer % 12 == 0)
            {
                BossVFXOptimizer.OptimizedThemedParticles(NPC.Center, "sakura", 2, 30f);
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        private void Attack_TriumphantCharge(Player target)
        {
            // DIFFICULTY: More dashes, faster, harder to predict
            int maxDashes = 4 + difficultyTier; // More dashes (was 3+)
            int windupTime = 13 - difficultyTier * 2; // Shorter warning (was 25-4)
            int dashTime = 8; // Slightly shorter (was 15)
            int recoveryTime = 6 - difficultyTier * 1; // Less recovery (was 12-2)
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.92f;
                
                if (Timer == 1)
                {
                    // PREDICT player velocity more aggressively
                    Vector2 predictedPos = target.Center + target.velocity * 18f; // More prediction (was 0f)
                    dashDirection = (predictedPos - NPC.Center).SafeNormalize(Vector2.Zero);
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.4f + dashCount * 0.1f }, NPC.Center);
                }
                
                // WARNING: Show dash trajectory
                if (Timer % 2 == 0)
                {
                    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 500f, 10, WarningType.Danger);
                    BossVFXOptimizer.OptimizedFlare(NPC.Center + Main.rand.NextVector2CircularEdge(50f, 50f), EroicaGold, 0.3f, 6, 2);
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                if (Timer == 1)
                {
                    // DIFFICULTY: Much faster dashes that escalate
                    float speed = 45f + difficultyTier * 8f + dashCount * 5f; // MUCH faster (was 35f+5f+3f)
                    NPC.velocity = dashDirection * speed;
                    
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.3f + dashCount * 0.15f }, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 0.8f);
                }
                
                // DIFFICULTY: Spawn projectiles during EVERY dash
                if (Timer % 3 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 perp = dashDirection.RotatedBy(MathHelper.PiOver2);
                    float sideSpeed = 5f + difficultyTier * 2f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center + perp * 25f, perp * sideSpeed, 65, EroicaGold, 0.02f);
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center - perp * 25f, -perp * sideSpeed, 65, EroicaGold, 0.02f);
                }
                
                // Optimized trail
                if (Timer % 3 == 0)
                {
                    BossVFXOptimizer.ProjectileTrail(NPC.Center, NPC.velocity, EroicaScarlet);
                    BossVFXOptimizer.OptimizedThemedParticles(NPC.Center - NPC.velocity * 0.2f, "sakura", 2, 12f);
                }
                
                if (Timer >= dashTime)
                {
                    Timer = 0;
                    dashCount++;
                    SubPhase = dashCount >= maxDashes ? 3 : 2;
                }
            }
            else if (SubPhase == 2)
            {
                NPC.velocity *= 0.8f;
                
                if (Timer == 3)
                {
                    // More aggressive prediction for next dash
                    Vector2 predictedPos = target.Center + target.velocity * 15f;
                    dashDirection = (predictedPos - NPC.Center).SafeNormalize(Vector2.Zero);
                }
                
                if (Timer >= recoveryTime)
                {
                    Timer = 0;
                    SubPhase = 0;
                }
            }
            else
            {
                NPC.velocity *= 0.85f;
                if (Timer >= 13) // Faster recovery (was 25)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_PhoenixDive(Player target)
        {
            if (SubPhase == 0)
            {
                NPC.alpha = Math.Min(255, NPC.alpha + 15); // Faster fade (was +12)
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f }, NPC.Center);
                    BossVFXOptimizer.OptimizedFlare(NPC.Center, EroicaGold, 0.7f, 12, 1);
                }
                
                // Optimized departure particles
                if (Timer % 4 == 0)
                {
                    BossVFXOptimizer.OptimizedFlare(NPC.Center + Main.rand.NextVector2Circular(35f, 35f), EroicaGold, 0.3f, 10, 2);
                }
                
                if (NPC.alpha >= 255)
                {
                    Timer = 0;
                    SubPhase = 1;
                    NPC.Center = target.Center + new Vector2(0, -500f); // Closer (was -600f)
                }
            }
            else if (SubPhase == 1)
            {
                if (Timer == 1)
                {
                    NPC.alpha = 0;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.2f }, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 1f);
                    
                    dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                }
                
                // WARNING: Clear dive trajectory and impact zone
                if (Timer > 2 && Timer < 13) // Shorter warning (was 5-25)
                {
                    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 600f, 12, WarningType.Imminent);
                    
                    // Show ground impact warning
                    Vector2 impactPos = NPC.Center + dashDirection * 500f;
                    BossVFXOptimizer.GroundImpactWarning(impactPos, 80f, Timer / 13f);
                }
                
                if (Timer >= 13) // Shorter telegraph (was 25)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                if (Timer == 1)
                {
                    // DIFFICULTY: MUCH faster dive
                    float diveSpeed = 55f + difficultyTier * 12f; // MUCH faster (was 45f+10f)
                    NPC.velocity = dashDirection * diveSpeed;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Volume = 1.5f }, NPC.Center);
                }
                
                // Optimized trail particles
                if (Timer % 3 == 0)
                {
                    BossVFXOptimizer.ProjectileTrail(NPC.Center, NPC.velocity, EroicaGold);
                    BossVFXOptimizer.OptimizedThemedParticles(NPC.Center - NPC.velocity * 0.15f, "sakura", 2, 20f);
                }
                
                // DIFFICULTY: More projectiles during dive
                if (Timer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 4 + difficultyTier; i++) // More projectiles (was 3)
                    {
                        float angle = MathHelper.TwoPi * i / (4 + difficultyTier) + Timer * 0.25f;
                        Vector2 vel = angle.ToRotationVector2() * (6f + difficultyTier * 2f);
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, EroicaCrimson, 0f);
                    }
                }
                
                if (NPC.Center.Y > target.Center.Y + 150f || Timer >= 25) // Faster (was 200f, 40)
                {
                    MagnumScreenEffects.AddScreenShake(15f);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 1.2f);
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, EroicaScarlet, EroicaGold, 5, 0.35f, 12);
                    
                    // GROUND EXPLOSION: Spawn projectiles in all directions
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int groundProjectiles = 8 + difficultyTier * 4;
                        for (int i = 0; i < groundProjectiles; i++)
                        {
                            float angle = MathHelper.TwoPi * i / groundProjectiles;
                            Vector2 vel = angle.ToRotationVector2() * (MediumProjectileSpeed + difficultyTier * 2f);
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 75, EroicaGold, 10f);
                        }
                    }
                    
                    Timer = 0;
                    SubPhase = 3;
                }
            }
            else
            {
                NPC.velocity *= 0.85f;
                if (Timer >= 18) // Shorter recovery (was 35)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_HeroesJudgment(Player target)
        {
            // DIFFICULTY: Shorter charge, more waves, faster projectiles, tighter safe arc
            int chargeTime = 42 - difficultyTier * 6; // Shorter (was 90-10)
            int waveCount = 3 + difficultyTier; // More waves (was 2+)
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.95f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, NPC.Center);
                    Main.NewText("Witness the Hero's Judgment!", EroicaGold);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // Optimized converging particles
                if (Timer % 4 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 200f, progress, EroicaGold, 8);
                }
                
                // SAFE ZONE INDICATOR - show player where to go (earlier and clearer)
                if (Timer > chargeTime / 3)
                {
                    float safeRadius = 90f; // Tighter (was 100f)
                    BossVFXOptimizer.SafeZoneRing(target.Center, safeRadius, 10);
                    
                    // Also show safe arc direction from boss
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(25f), 150f, 6);
                }
                
                if (Timer > chargeTime * 0.6f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 6f);
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
                    MagnumScreenEffects.AddScreenShake(18f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f }, NPC.Center);
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EroicaGold, EroicaScarlet, 1.2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, tighter safe arc, faster
                        int projectileCount = 40 + difficultyTier * 10; // More (was 32+8)
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(22f - difficultyTier * 2f); // MUCH tighter (was 30f)
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            // DIFFICULTY: Much faster projectiles
                            float speed = FastProjectileSpeed + difficultyTier * 3f + SubPhase * 2f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            // Mix accelerating bolts and homing orbs
                            if (i % 4 == 0)
                            {
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 0.7f, 80, EroicaScarlet, 0.04f);
                            }
                            else
                            {
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 80, EroicaGold, 15f);
                            }
                        }
                    }
                    
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, EroicaScarlet, EroicaGold, 6, 0.35f, 15);
                }
                
                // DIFFICULTY: Shorter delay between waves
                if (Timer >= 22) // Faster (was 45)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 20) // Shorter recovery (was 40)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_UltimateValor(Player target)
        {
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    Main.NewText("ULTIMATE VALOR!", EroicaGold);
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, Volume = 1.5f }, NPC.Center);
                }
                
                if (Timer % 3 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, EroicaGold, 0.4f + Timer * 0.01f, 10);
                    ThemedParticles.SakuraPetals(NPC.Center, 4, 50f);
                }
                
                MagnumScreenEffects.AddScreenShake(Timer * 0.2f);
                
                if (Timer >= 42)
                {
                    Timer = 0;
                    SubPhase = 1;
                    dashCount = 0;
                    dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                }
            }
            else if (SubPhase <= 5)
            {
                if (Timer == 1)
                {
                    NPC.velocity = dashDirection * 40f;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.4f + SubPhase * 0.1f }, NPC.Center);
                }
                
                if (Timer % 2 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center - NPC.velocity * 0.2f, EroicaGold, 0.5f, 10);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = angle.ToRotationVector2() * 6f;
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70, EroicaScarlet, 0.02f);
                    }
                }
                
                if (Timer >= 8)
                {
                    NPC.velocity *= 0.7f;
                    Timer = 0;
                    SubPhase++;
                    
                    if (SubPhase <= 5)
                    {
                        float spiralOffset = MathHelper.ToRadians(SubPhase * 30f);
                        dashDirection = ((target.Center - NPC.Center).SafeNormalize(Vector2.Zero)).RotatedBy(spiralOffset);
                    }
                }
            }
            else if (SubPhase == 6)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 15 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int arm = 0; arm < 8; arm++)
                    {
                        float armAngle = MathHelper.PiOver4 * arm;
                        for (int p = 0; p < 6; p++)
                        {
                            float speed = 10f + p * 2f;
                            Vector2 vel = armAngle.ToRotationVector2() * speed;
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, arm % 2 == 0 ? EroicaGold : EroicaScarlet, 0f);
                        }
                    }
                    
                    MagnumScreenEffects.AddScreenShake(15f);
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 1.2f, 20);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f }, NPC.Center);
                }
                
                if (Timer >= 28)
                {
                    Timer = 0;
                    SubPhase = 7;
                }
            }
            else if (SubPhase == 7)
            {
                if (Timer < 42 && Timer % 4 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float spiralAngle = Timer * 0.15f;
                    for (int arm = 0; arm < 5; arm++)
                    {
                        float angle = spiralAngle + MathHelper.TwoPi * arm / 5f;
                        Vector2 vel = angle.ToRotationVector2() * 10f;
                        BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, SakuraPink, 4f);
                    }
                    
                    ThemedParticles.SakuraPetals(NPC.Center, 5, 60f);
                }
                
                if (Timer >= 56)
                {
                    consecutiveAttacks = 0;
                    EndAttack();
                }
            }
        }
        
        private void EndAttack()
        {
            // Spawn attack ending visual cue - signals to player that attack is over
            BossVFXOptimizer.AttackEndCue(NPC.Center, EroicaGold, EroicaScarlet, 0.8f);
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Phase2_Reposition;
            attackCooldown = AttackWindowFrames - difficultyTier * 15;
        }
        
        #endregion
        
        #region Visuals
        
        private void UpdateAnimation()
        {
            int frameSpeed = phase2Started ? 3 : 5;
            frameCounter++;
            if (frameCounter >= frameSpeed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }
        
        private void SpawnAmbientParticles()
        {
            if (!phase2Started) return;
            
            // Performance gate - skip under critical load
            if (BossVFXOptimizer.IsCriticalLoad) return;
            bool isHighLoad = BossVFXOptimizer.IsHighLoad;
            
            // Orbiting embers - reduce count and frequency under load
            int orbitInterval = isHighLoad ? 10 : 6;
            if (Timer % orbitInterval == 0)
            {
                int orbitCount = isHighLoad ? (2 + difficultyTier) : (3 + difficultyTier * 2);
                for (int i = 0; i < orbitCount; i++)
                {
                    float angle = Timer * 0.03f + MathHelper.TwoPi * i / orbitCount;
                    float radius = 60f + (float)Math.Sin(Timer * 0.05f + i) * 15f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = Color.Lerp(EroicaScarlet, EroicaGold, (float)i / orbitCount);
                    CustomParticles.GenericFlare(pos, color, 0.28f, 10);
                }
            }
            
            // Sakura petals - reduce under load
            int petalChance = isHighLoad ? (10 - difficultyTier) : (6 - difficultyTier);
            if (Main.rand.NextBool(petalChance))
            {
                int petalCount = isHighLoad ? 1 : 2;
                ThemedParticles.SakuraPetals(NPC.Center + Main.rand.NextVector2Circular(50f, 50f), petalCount, 25f);
            }
            
            // Enrage particles - reduce under load  
            if (isEnraged && Timer % (isHighLoad ? 4 : 2) == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.GenericFlare(pos, EroicaCrimson, 0.4f, 8);
            }
        }
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            if (deathTimer % 8 == 0)
            {
                float progress = deathTimer / 180f;
                MagnumScreenEffects.AddScreenShake(4f + progress * 15f);
                
                Vector2 burstPos = NPC.Center + Main.rand.NextVector2Circular(50f * (1f - progress * 0.5f), 50f * (1f - progress * 0.5f));
                CustomParticles.GenericFlare(burstPos, Color.Lerp(EroicaGold, Color.White, progress), 0.5f + progress * 0.5f, 15);
                CustomParticles.HaloRing(burstPos, EroicaGold, 0.3f + progress * 0.3f, 12);
                ThemedParticles.SakuraPetals(burstPos, (int)(4 + progress * 12), 60f);
            }
            
            if (deathTimer >= 180)
            {
                MagnumScreenEffects.AddScreenShake(30f);
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 2f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 2f, 30);
                
                for (int i = 0; i < 12; i++)
                {
                    float scale = 0.4f + i * 0.2f;
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(EroicaScarlet, EroicaGold, i / 12f), scale, 25 + i * 3);
                }
                
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 100f, EroicaGold, 0.8f, 30);
                }
                
                ThemedParticles.SakuraPetals(NPC.Center, 60, 200f);
                
                // Death dialogue
                BossDialogueSystem.Eroica.OnDeath();
                BossDialogueSystem.CleanupDialogue(NPC.whoAmI);
                
                // Set boss downed flag for miniboss essence drops
                MoonlightSonataSystem.DownedEroica = true;
                if (Main.netMode == NetmodeID.Server)
                    NetMessage.SendData(MessageID.WorldData);
                
                NPC.life = 0;
                NPC.HitEffect();
                NPC.checkDead();
            }
        }
        
        #endregion
        
        #region Drawing
        
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = currentFrame * frameHeight;
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            int frameWidth = tex.Width / 6;
            int frameHeight = tex.Height / 6;
            int row = currentFrame / 6;
            int col = currentFrame % 6;
            Rectangle sourceRect = new Rectangle(col * frameWidth, row * frameHeight, frameWidth, frameHeight);
            Vector2 origin = new Vector2(frameWidth / 2f, frameHeight / 2f);
            Vector2 drawPos = NPC.Center - screenPos;
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            if (phase2Started && NPC.velocity.Length() > 8f)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    float progress = (float)i / NPC.oldPos.Length;
                    Color trailColor = Color.Lerp(EroicaScarlet, EroicaGold, progress) * (1f - progress) * 0.4f;
                    Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                    spriteBatch.Draw(tex, trailPos, sourceRect, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.15f), effects, 0f);
                }
            }
            
            Color glowColor = isEnraged ? EroicaCrimson : Color.Lerp(EroicaGold, EroicaScarlet, (float)Math.Sin(Timer * 0.05f) * 0.5f + 0.5f);
            glowColor.A = 0;
            spriteBatch.Draw(tex, drawPos, sourceRect, glowColor * 0.35f, NPC.rotation, origin, NPC.scale * 1.12f, effects, 0f);
            
            Color mainColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16));
            mainColor = Color.Lerp(mainColor, Color.White, 0.35f);
            spriteBatch.Draw(tex, drawPos, sourceRect, mainColor * ((255 - NPC.alpha) / 255f), NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<EroicaTreasureBag>()));
            
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EroicasResonantEnergy>(), 1, 10, 12));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfEroicasTriumph>(), 1, 15, 17));
            npcLoot.Add(notExpert);
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
