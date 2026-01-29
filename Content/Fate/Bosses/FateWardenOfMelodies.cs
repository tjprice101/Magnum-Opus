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
using Terraria.Graphics.Effects;
using MagnumOpus.Content.Fate.ResonanceEnergies;
using MagnumOpus.Content.Fate.Debuffs;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.Fate.Bosses
{
    /// <summary>
    /// FATE, THE WARDEN OF UNIVERSAL MELODIES - ENDGAME CELESTIAL BOSS
    /// 
    /// Design Philosophy:
    /// - High mechanical skill ceiling with tight timing windows
    /// - Destructive but precise attacks (not screen-filling)
    /// - Celestial cosmic theme with glyphs, stars, and cosmic clouds
    /// - Uses Duke Fishron sprite and animations (red-shaded)
    /// - Confrontational deity who holds back the score of time
    /// 
    /// Attack Philosophy:
    /// - Quick, deadly attacks that require precise positioning
    /// - Pattern recognition rewards skilled players
    /// - Safe zones exist but require finding them
    /// - Combos that chain attacks for pressure
    /// </summary>
    public class FateWardenOfMelodies : ModNPC
    {
        // Custom boss sprite - celestial deity with multiple arms
        public override string Texture => "MagnumOpus/Content/Fate/Bosses/FateWardenOfMelodies";
        
        #region Theme Colors - Dark Prismatic Celestial
        private static readonly Color FateBlack = new Color(15, 5, 20);
        private static readonly Color FateDarkPink = new Color(180, 50, 100);
        private static readonly Color FateBrightRed = new Color(255, 60, 80);
        private static readonly Color FatePurple = new Color(120, 30, 140);
        private static readonly Color FateWhite = new Color(255, 255, 255);
        private static readonly Color FateStarGold = new Color(255, 230, 180);
        #endregion
        
        #region Constants - Tight Timing Windows
        private const float BaseSpeed = 22f;           // Very fast base speed
        private const int BaseDamage = 210;            // ENDGAME damage - higher than Swan Lake (170)
        private const float EnrageDistance = 900f;     // Tight arena
        private const float TeleportDistance = 1400f;
        private const int AttackWindowFrames = 35;     // Very short windows
        
        // Projectile speeds - calibrated for high skill players
        private const float FastProjectileSpeed = 24f;
        private const float MediumProjectileSpeed = 18f;
        private const float HomingSpeed = 10f;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Idle,
            Attack,
            Reposition,
            Enraged,
            CosmicWrath,    // Ultimate phase below 20%
            Dying,
            Awakening       // TRUE FORM - Fate refuses to fall
        }
        
        private enum AttackPattern
        {
            // Core Attacks (Always available)
            CosmicDash,         // Teleport + instant dash
            StarfallBarrage,    // Rapid star projectiles
            GlyphCircle,        // Orbiting glyph ring
            
            // Phase 2 (Below 70%)
            DestinyChain,       // Multi-teleport combo
            ConstellationStrike,// Connecting star pattern
            TimeSlice,          // Screen-crossing slashes
            
            // Phase 3 (Below 40%)
            UniversalJudgment,  // Safe-arc radial burst
            CosmicVortex,       // Pulling gravity zone
            FinalMelody         // Ultimate multi-phase attack
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
        private AttackPattern lastAttack = AttackPattern.CosmicDash;
        
        private int dashCount = 0;
        private Vector2 dashTarget;
        private Vector2 dashDirection;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 2400; // 40 seconds - faster aggression
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        
        // TRUE FORM AWAKENING - On first "death", Fate awakens to full power
        private bool hasAwakened = false;
        private int awakeningTimer = 0;
        private const int AwakeningDuration = 300; // 5 seconds of awakening animation
        
        // Animation
        private int frameCounter = 0;
        private int currentFrame = 0;
        
        // Glyph orbit tracking
        private float glyphOrbitAngle = 0f;
        private List<Vector2> storedStarPositions = new List<Vector2>();
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1; // Single frame sprite
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 15;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 180;
            NPC.height = 200;
            NPC.damage = BaseDamage;
            NPC.defense = 140;      // ENDGAME defense - higher than Swan Lake (110)
            NPC.lifeMax = 3000000;   // ENDGAME BOSS - 3 million HP (must defeat TWICE due to True Form)
            NPC.HitSound = SoundID.NPCHit14;
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
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/DynastyOfTheResonantFate");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Fate, The Warden of Universal Melodies - " +
                    "A celestial deity who holds back the score of time itself. " +
                    "The planets spin to his rhythm. Without his melody, the world would perish.")
            });
        }

        #region Color Helpers
        private Color GetCosmicGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(FateBlack, FateDarkPink, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(FateDarkPink, FateBrightRed, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(FateBrightRed, FateWhite, (progress - 0.66f) * 3f);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.4f;
        private float GetAggressionRateMult() => 1f - aggressionLevel * 0.25f;
        #endregion

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Fate);
                hasRegisteredHealthBar = true;
            }
            
            // Handle special states that bypass normal AI
            if (State == BossPhase.Dying)
            {
                UpdateDeathAnimation();
                return;
            }
            
            if (State == BossPhase.Awakening)
            {
                UpdateAwakening();
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
            UpdateGlyphOrbit();
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
                case BossPhase.CosmicWrath:
                    AI_CosmicWrath(target);
                    break;
            }
            
            Timer++;
            fightTimer++;
            UpdateAnimation();
            SpawnAmbientParticles();
            
            // Boss dialogue now only triggers at HP thresholds via AnnounceDifficultyChange
            
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            
            float lightIntensity = isEnraged ? 1.4f : 1.0f;
            Lighting.AddLight(NPC.Center, FateBrightRed.ToVector3() * lightIntensity * 0.6f);
        }

        #region Core AI Methods
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : (hpPercent > 0.2f ? 2 : 3));
            
            if (newTier != difficultyTier)
            {
                difficultyTier = newTier;
                AnnounceDifficultyChange();
            }
        }
        
        private void AnnounceDifficultyChange()
        {
            MagnumScreenEffects.AddScreenShake(difficultyTier >= 2 ? 20f : 14f);
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = difficultyTier * 0.15f - 0.3f }, NPC.Center);
            
            // Cosmic burst
            CustomParticles.GenericFlare(NPC.Center, FateWhite, 1.3f, 25);
            for (int i = 0; i < 10 + difficultyTier * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (10 + difficultyTier * 4);
                Color burstColor = GetCosmicGradient((float)i / (10 + difficultyTier * 4));
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 70f, burstColor, 0.6f, 18);
            }
            
            // Glyph burst
            CustomParticles.GlyphBurst(NPC.Center, FateDarkPink, 8, 5f);
            
            // Use dialogue system for phase transitions
            if (difficultyTier == 1)
                BossDialogueSystem.Fate.OnPhase2(NPC.whoAmI);
            else if (difficultyTier == 2)
                BossDialogueSystem.Fate.OnPhase3(NPC.whoAmI);
            else if (difficultyTier >= 3)
                BossDialogueSystem.Fate.OnCosmicWrath(NPC.whoAmI);
            
            // Enter cosmic wrath at tier 3
            if (difficultyTier >= 3)
            {
                State = BossPhase.CosmicWrath;
                Timer = 0;
            }
        }
        
        private void UpdateAggression()
        {
            aggressionLevel = Math.Min(1f, (float)fightTimer / MaxAggressionTime);
        }
        
        private void UpdateGlyphOrbit()
        {
            glyphOrbitAngle += 0.02f;
            if (glyphOrbitAngle > MathHelper.TwoPi)
                glyphOrbitAngle -= MathHelper.TwoPi;
        }
        
        private void CheckEnrage(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            
            if (distance > TeleportDistance)
            {
                TeleportToPlayer(target);
                return;
            }
            
            if (distance > EnrageDistance)
            {
                enrageTimer++;
                if (enrageTimer > 60 && !isEnraged)
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    BossDialogueSystem.Fate.OnEnrage();
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.6f }, NPC.Center);
                }
            }
            else
            {
                enrageTimer = Math.Max(0, enrageTimer - 4);
                if (isEnraged && enrageTimer == 0)
                {
                    isEnraged = false;
                    if (State == BossPhase.Enraged)
                    {
                        State = BossPhase.Idle;
                        Timer = 0;
                    }
                }
            }
        }
        
        private void TeleportToPlayer(Player target)
        {
            // Departure VFX
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 1.3f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, FateWhite, 1.2f, 20);
            CustomParticles.GlyphBurst(NPC.Center, FatePurple, 6, 4f);
            
            // Cosmic cloud at departure
            for (int i = 0; i < 8; i++)
            {
                Vector2 cloudVel = Main.rand.NextVector2Circular(4f, 4f);
                var cloud = new GenericGlowParticle(NPC.Center, cloudVel, FatePurple * 0.6f, 0.4f, 25, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
            
            Vector2 teleportOffset = Main.rand.NextVector2CircularEdge(300f, 300f);
            NPC.Center = target.Center + teleportOffset;
            NPC.velocity = Vector2.Zero;
            
            // Arrival VFX
            SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, FateWhite, 1.5f, 28);
            for (int i = 0; i < 6; i++)
            {
                CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 6f), 0.3f + i * 0.12f, 15 + i * 3);
            }
            CustomParticles.GlyphBurst(NPC.Center, FateDarkPink, 8, 6f);
        }
        
        #endregion

        #region Phase AI
        
        private void AI_Spawning(Player target)
        {
            NPC.velocity *= 0.95f;
            
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                Main.NewText("The Warden of Universal Melodies descends...", FateDarkPink);
            }
            
            if (Timer < 90 && Timer % 5 == 0)
            {
                float progress = Timer / 90f;
                // Converging cosmic particles
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f + Timer * 0.04f;
                    float radius = 200f * (1f - progress * 0.6f);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, GetCosmicGradient(progress), 0.4f + progress * 0.3f, 18);
                }
                
                // Glyphs spiral inward
                CustomParticles.Glyph(NPC.Center + Main.rand.NextVector2Circular(100f * (1f - progress), 100f * (1f - progress)), 
                    FateDarkPink, 0.4f, -1);
            }
            
            if (Timer == 90)
            {
                // Grand entrance explosion
                MagnumScreenEffects.AddScreenShake(25f);
                CustomParticles.GenericFlare(NPC.Center, FateWhite, 2f, 30);
                for (int i = 0; i < 12; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 12f), 0.25f + i * 0.2f, 18 + i * 4);
                }
                CustomParticles.GlyphBurst(NPC.Center, FateBrightRed, 12, 8f);
            }
            
            if (Timer >= 84)
            {
                // Activate the cosmic Fate sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:FateSky"] != null)
                {
                    SkyManager.Instance.Activate("MagnumOpus:FateSky");
                }
                
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames;
            }
        }
        
        private void AI_Idle(Player target)
        {
            // Hover near player
            float hoverHeight = -280f - (float)Math.Sin(Timer * 0.03f) * 30f;
            float waveX = (float)Math.Sin(Timer * 0.025f) * 60f;
            
            Vector2 hoverPos = target.Center + new Vector2(waveX, hoverHeight);
            Vector2 toHover = hoverPos - NPC.Center;
            float hoverSpeed = 12f * GetAggressionSpeedMult();
            
            if (toHover.Length() > 40f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * hoverSpeed, 0.06f);
            }
            else
            {
                NPC.velocity *= 0.94f;
            }
            
            // Start attack when cooldown expires
            if (attackCooldown <= 0)
            {
                SelectNextAttack(target);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.CosmicDash,
                AttackPattern.StarfallBarrage,
                AttackPattern.GlyphCircle
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.DestinyChain);
                pool.Add(AttackPattern.ConstellationStrike);
                pool.Add(AttackPattern.TimeSlice);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.UniversalJudgment);
                pool.Add(AttackPattern.CosmicVortex);
                pool.Add(AttackPattern.FinalMelody);
            }
            
            // Remove last attack to prevent repetition
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Attack;
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.CosmicDash:
                    Attack_CosmicDash(target);
                    break;
                case AttackPattern.StarfallBarrage:
                    Attack_StarfallBarrage(target);
                    break;
                case AttackPattern.GlyphCircle:
                    Attack_GlyphCircle(target);
                    break;
                case AttackPattern.DestinyChain:
                    Attack_DestinyChain(target);
                    break;
                case AttackPattern.ConstellationStrike:
                    Attack_ConstellationStrike(target);
                    break;
                case AttackPattern.TimeSlice:
                    Attack_TimeSlice(target);
                    break;
                case AttackPattern.UniversalJudgment:
                    Attack_UniversalJudgment(target);
                    break;
                case AttackPattern.CosmicVortex:
                    Attack_CosmicVortex(target);
                    break;
                case AttackPattern.FinalMelody:
                    Attack_FinalMelody(target);
                    break;
            }
        }
        
        private void AI_Reposition(Player target)
        {
            Vector2 idealPos = target.Center + new Vector2(NPC.direction * -350f, -250f);
            Vector2 toIdeal = idealPos - NPC.Center;
            float speed = 15f * GetAggressionSpeedMult();
            
            if (toIdeal.Length() > 50f)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * speed, 0.08f);
            }
            else
            {
                NPC.velocity *= 0.92f;
            }
            
            if (Timer >= 28)
            {
                State = BossPhase.Idle;
                Timer = 0;
                attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Rapid dashes toward player
            float dashSpeed = 28f * GetAggressionSpeedMult();
            Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
            
            NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * dashSpeed, 0.12f);
            
            // Spawn projectiles while dashing
            if (Timer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float angle = Main.rand.NextFloat(-0.3f, 0.3f);
                Vector2 projVel = (NPC.velocity.SafeNormalize(Vector2.UnitY) * 12f).RotatedBy(angle);
                BossProjectileHelper.SpawnHostileOrb(NPC.Center, projVel, 80, FateBrightRed, HomingSpeed * 0.002f);
            }
            
            // Trail particles
            if (Timer % 3 == 0)
            {
                CustomParticles.GenericFlare(NPC.Center, FateBrightRed, 0.5f, 12);
            }
        }
        
        private void AI_CosmicWrath(Player target)
        {
            // Final 20% phase - relentless assault
            aggressionLevel = 1f;
            
            if (attackCooldown <= 0)
            {
                // Chain attacks rapidly
                List<AttackPattern> wrathPool = new List<AttackPattern>
                {
                    AttackPattern.CosmicDash,
                    AttackPattern.TimeSlice,
                    AttackPattern.UniversalJudgment,
                    AttackPattern.StarfallBarrage
                };
                wrathPool.Remove(lastAttack);
                
                CurrentAttack = wrathPool[Main.rand.Next(wrathPool.Count)];
                lastAttack = CurrentAttack;
                Timer = 0;
                SubPhase = 0;
                State = BossPhase.Attack;
            }
            else
            {
                // Aggressive hover
                Vector2 toPlayer = (target.Center - NPC.Center);
                float distance = toPlayer.Length();
                toPlayer.Normalize();
                
                float speed = MathHelper.Lerp(8f, 20f, 1f - distance / 500f);
                NPC.velocity = Vector2.Lerp(NPC.velocity, toPlayer * speed, 0.1f);
            }
        }
        
        private void EndAttack()
        {
            Timer = 0;
            SubPhase = 0;
            
            if (difficultyTier >= 3)
            {
                State = BossPhase.CosmicWrath;
                attackCooldown = (int)(14 * GetAggressionRateMult()); // Very short cooldown
            }
            else
            {
                State = BossPhase.Reposition;
                attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
            }
        }
        
        #endregion

        #region Attacks
        
        /// <summary>
        /// COSMIC DASH - Teleport to offset position, instant dash through player
        /// Tight timing window, must dash perpendicular to avoid
        /// </summary>
        private void Attack_CosmicDash(Player target)
        {
            int dashCount = 2 + difficultyTier;
            int telegraphTime = (int)((18 - difficultyTier * 2) * GetAggressionRateMult());
            float dashSpeed = (50f + difficultyTier * 8f) * GetAggressionSpeedMult();
            
            if (SubPhase < dashCount * 2) // Each dash has telegraph + execute
            {
                bool isTelegraph = SubPhase % 2 == 0;
                
                if (isTelegraph)
                {
                    // Telegraph phase
                    if (Timer == 1)
                    {
                        // Teleport to offset position
                        float offsetAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        Vector2 offset = offsetAngle.ToRotationVector2() * 400f;
                        
                        // Departure VFX
                        CustomParticles.GlyphBurst(NPC.Center, FatePurple, 4, 3f);
                        
                        NPC.Center = target.Center + offset;
                        NPC.velocity = Vector2.Zero;
                        dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                        
                        // Arrival flash
                        CustomParticles.GenericFlare(NPC.Center, FateWhite, 0.8f, 15);
                        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.3f }, NPC.Center);
                    }
                    
                    // Warning line
                    float progress = (float)Timer / telegraphTime;
                    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 500f, 12, WarningType.Danger);
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 50f, progress, FateBrightRed, 6);
                    
                    if (Timer >= telegraphTime)
                    {
                        Timer = 0;
                        SubPhase++;
                    }
                }
                else
                {
                    // Execute dash
                    if (Timer == 1)
                    {
                        NPC.velocity = dashDirection * dashSpeed;
                        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.8f }, NPC.Center);
                        BossVFXOptimizer.AttackReleaseBurst(NPC.Center, FateBrightRed, FateDarkPink, 0.9f);
                    }
                    
                    // Trail particles
                    if (Timer % 2 == 0)
                    {
                        CustomParticles.GenericFlare(NPC.Center, FateBrightRed * 0.7f, 0.4f, 10);
                        var trail = new GenericGlowParticle(NPC.Center, -NPC.velocity * 0.1f, FateDarkPink, 0.35f, 15, true);
                        MagnumParticleHandler.SpawnParticle(trail);
                    }
                    
                    // Friction
                    NPC.velocity *= 0.95f;
                    
                    if (Timer >= 11)
                    {
                        Timer = 0;
                        SubPhase++;
                    }
                }
            }
            else
            {
                if (Timer >= 14) EndAttack();
            }
        }
        
        /// <summary>
        /// STARFALL BARRAGE - Rapid star projectiles from above with tracking
        /// Creates pressure, must keep moving
        /// </summary>
        private void Attack_StarfallBarrage(Player target)
        {
            int duration = (int)((56 + difficultyTier * 20) * GetAggressionRateMult());
            int fireInterval = Math.Max(4, (int)((7 - difficultyTier * 2) * GetAggressionRateMult()));
            
            // Hover above target
            Vector2 hoverPos = target.Center + new Vector2(0, -400f);
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 50f)
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover.SafeNormalize(Vector2.Zero) * 12f, 0.08f);
            }
            
            // Warning flares where projectiles will spawn
            if (Timer > 20 && Timer % 15 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float xOffset = Main.rand.NextFloat(-200f, 200f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -500f);
                    BossVFXOptimizer.WarningFlare(spawnPos, 1f, WarningType.Caution);
                }
            }
            
            // Fire projectiles
            if (Timer > 21 && Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 2 + difficultyTier;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-250f, 250f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -500f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), FastProjectileSpeed);
                    
                    // Mix of accelerating and homing
                    if (i % 2 == 0)
                        BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.7f, 85, FateBrightRed, 15f);
                    else
                        BossProjectileHelper.SpawnHostileOrb(spawnPos, vel * 0.8f, 85, FateStarGold, 0.015f);
                    
                    CustomParticles.GenericFlare(spawnPos, FateWhite, 0.4f, 10);
                }
            }
            
            if (Timer >= duration) EndAttack();
        }
        
        /// <summary>
        /// GLYPH CIRCLE - Spawns orbiting glyphs that shoot inward
        /// Creates a closing ring pattern
        /// </summary>
        private void Attack_GlyphCircle(Player target)
        {
            int glyphCount = 8 + difficultyTier * 2;
            int chargeTime = (int)((28 - difficultyTier * 5) * GetAggressionRateMult());
            
            if (SubPhase == 0)
            {
                // Slow down and prepare
                NPC.velocity *= 0.9f;
                
                // Spawn glyph indicators in circle around player
                if (Timer % 8 == 0 && Timer < chargeTime)
                {
                    float progress = (float)Timer / chargeTime;
                    float radius = 300f - progress * 50f;
                    
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount + Timer * 0.02f;
                        Vector2 glyphPos = target.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(glyphPos, Color.Lerp(FatePurple, FateDarkPink, progress), 0.4f, -1);
                    }
                }
                
                // Sound buildup
                if (Timer == 1)
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 0.5f }, NPC.Center);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Fire all glyphs inward
                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item125 with { Volume = 1f }, target.Center);
                    
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount;
                        float radius = 250f;
                        Vector2 spawnPos = target.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * MediumProjectileSpeed;
                        
                        BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 80, FateDarkPink, 0f);
                        CustomParticles.GenericFlare(spawnPos, FateDarkPink, 0.5f, 12);
                    }
                    
                    // Central warning
                    CustomParticles.HaloRing(target.Center, FateBrightRed * 0.6f, 0.4f, 15);
                }
                
                if (Timer >= 21) EndAttack();
            }
        }
        
        /// <summary>
        /// DESTINY CHAIN - Multiple teleport+attack combo
        /// High pressure, tests reaction speed
        /// </summary>
        private void Attack_DestinyChain(Player target)
        {
            int chainCount = 3 + difficultyTier;
            int teleportDelay = Math.Max(12, (int)((15 - difficultyTier * 3) * GetAggressionRateMult()));
            
            if (SubPhase < chainCount)
            {
                if (Timer == 1)
                {
                    // Teleport near player from random direction
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    Vector2 offset = angle.ToRotationVector2() * 200f;
                    
                    // Departure glyph
                    CustomParticles.Glyph(NPC.Center, FatePurple, 0.5f, -1);
                    
                    NPC.Center = target.Center + offset;
                    NPC.velocity = Vector2.Zero;
                    
                    // Arrival burst
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 0.7f, 12);
                }
                
                // Quick attack toward player
                if (Timer == 5 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitY);
                    for (int i = -1; i <= 1; i++)
                    {
                        Vector2 vel = toPlayer.RotatedBy(i * 0.15f) * FastProjectileSpeed;
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 75, FateBrightRed, 12f);
                    }
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, FateBrightRed, FatePurple, 0.6f);
                }
                
                if (Timer >= teleportDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 18) EndAttack();
            }
        }
        
        /// <summary>
        /// CONSTELLATION STRIKE - Places stars, then connects them with deadly beams
        /// Pattern recognition, avoid the lines
        /// </summary>
        private void Attack_ConstellationStrike(Player target)
        {
            int starCount = 5 + difficultyTier;
            int placeTime = (int)((50 - difficultyTier * 5) * GetAggressionRateMult());
            
            if (SubPhase == 0)
            {
                // Place stars around arena
                storedStarPositions.Clear();
                NPC.velocity *= 0.95f;
                
                int starsToPlace = (int)(Timer / (placeTime / (float)starCount));
                starsToPlace = Math.Min(starsToPlace, starCount);
                
                // Place stars progressively
                while (storedStarPositions.Count < starsToPlace)
                {
                    float angle = MathHelper.TwoPi * storedStarPositions.Count / starCount;
                    float radius = 200f + Main.rand.NextFloat(-50f, 50f);
                    Vector2 starPos = target.Center + angle.ToRotationVector2() * radius;
                    storedStarPositions.Add(starPos);
                    
                    // Star placement VFX
                    CustomParticles.GenericFlare(starPos, FateWhite, 0.6f, 20);
                    CustomParticles.GenericFlare(starPos, FateStarGold, 0.4f, 25);
                    SoundEngine.PlaySound(SoundID.Item9 with { Pitch = storedStarPositions.Count * 0.1f, Volume = 0.4f }, starPos);
                }
                
                // Pulse existing stars
                if (Timer % 10 == 0)
                {
                    foreach (var starPos in storedStarPositions)
                    {
                        CustomParticles.GenericFlare(starPos, FateStarGold * 0.5f, 0.3f, 8);
                    }
                }
                
                if (Timer >= placeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Warning phase - show connection lines
                if (Timer % 5 == 0 && Timer < 30)
                {
                    for (int i = 0; i < storedStarPositions.Count; i++)
                    {
                        int next = (i + 1) % storedStarPositions.Count;
                        Vector2 dir = (storedStarPositions[next] - storedStarPositions[i]).SafeNormalize(Vector2.UnitX);
                        float length = Vector2.Distance(storedStarPositions[i], storedStarPositions[next]);
                        BossVFXOptimizer.WarningLine(storedStarPositions[i], dir, length, 8, WarningType.Danger);
                    }
                }
                
                if (Timer >= 21)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                // Fire beams along constellation lines
                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SoundEngine.PlaySound(SoundID.Item125 with { Volume = 1.2f }, NPC.Center);
                    
                    for (int i = 0; i < storedStarPositions.Count; i++)
                    {
                        int next = (i + 1) % storedStarPositions.Count;
                        Vector2 start = storedStarPositions[i];
                        Vector2 end = storedStarPositions[next];
                        Vector2 dir = (end - start).SafeNormalize(Vector2.UnitX);
                        float length = Vector2.Distance(start, end);
                        
                        // Spawn projectiles along line
                        int projCount = (int)(length / 40f);
                        for (int p = 0; p < projCount; p++)
                        {
                            float t = (float)p / projCount;
                            Vector2 pos = Vector2.Lerp(start, end, t);
                            Vector2 vel = dir.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-2f, 2f);
                            BossProjectileHelper.SpawnHostileOrb(pos, vel + dir * 5f, 70, FateStarGold, 0f);
                        }
                        
                        // Beam VFX
                        for (int seg = 0; seg < (int)(length / 20f); seg++)
                        {
                            Vector2 pos = Vector2.Lerp(start, end, seg / (length / 20f));
                            CustomParticles.GenericFlare(pos, FateWhite, 0.5f, 15);
                        }
                    }
                    
                    MagnumScreenEffects.AddScreenShake(10f);
                }
                
                if (Timer >= 28)
                {
                    storedStarPositions.Clear();
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// TIME SLICE - Screen-crossing slash attacks
        /// Must position between the slashes
        /// </summary>
        private void Attack_TimeSlice(Player target)
        {
            int sliceCount = 2 + difficultyTier;
            int sliceDelay = Math.Max(20, (int)((25 - difficultyTier * 4) * GetAggressionRateMult()));
            
            if (SubPhase < sliceCount)
            {
                if (Timer == 1)
                {
                    // Teleport to side of arena
                    float side = SubPhase % 2 == 0 ? -1f : 1f;
                    NPC.Center = target.Center + new Vector2(side * 600f, Main.rand.NextFloat(-200f, 200f));
                    NPC.velocity = Vector2.Zero;
                    
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.6f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 0.9f, 15);
                }
                
                // Warning line across screen
                if (Timer < sliceDelay - 5)
                {
                    float progress = (float)Timer / (sliceDelay - 5);
                    Vector2 sliceDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    BossVFXOptimizer.WarningLine(NPC.Center, sliceDir, 1200f, 20, WarningType.Imminent);
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 80f, progress, FateBrightRed, 8);
                }
                
                // Execute slice
                if (Timer == sliceDelay && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 sliceDir = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    float sliceSpeed = 35f + difficultyTier * 5f;
                    
                    // Line of fast projectiles
                    for (int i = 0; i < 15; i++)
                    {
                        float offset = (i - 7) * 8f;
                        Vector2 spawnPos = NPC.Center + sliceDir.RotatedBy(MathHelper.PiOver2) * offset;
                        Vector2 vel = sliceDir * sliceSpeed;
                        BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 85, FateBrightRed, 5f);
                    }
                    
                    // Slice VFX
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.2f }, NPC.Center);
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, FateBrightRed, FateWhite, 1.2f);
                    MagnumScreenEffects.AddScreenShake(12f);
                }
                
                if (Timer >= sliceDelay + 11)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 14) EndAttack();
            }
        }
        
        /// <summary>
        /// UNIVERSAL JUDGMENT - Hero's Judgment style with safe arc
        /// Signature spectacle attack
        /// </summary>
        private void Attack_UniversalJudgment(Player target)
        {
            int chargeTime = (int)((42 - difficultyTier * 8) * GetAggressionRateMult());
            int waveCount = 2 + difficultyTier;
            
            if (SubPhase == 0)
            {
                // Charge phase
                NPC.velocity *= 0.93f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f }, NPC.Center);
                    Main.NewText("Witness Universal Judgment!", FateBrightRed);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // Converging cosmic particles
                if (Timer % 4 == 0)
                {
                    int particleCount = (int)(8 + progress * 12);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.06f;
                        float radius = 250f * (1f - progress * 0.6f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color color = GetCosmicGradient(progress);
                        CustomParticles.GenericFlare(pos, color, 0.3f + progress * 0.4f, 12);
                    }
                }
                
                // Glyphs orbit
                if (Timer % 8 == 0)
                {
                    CustomParticles.GlyphCircle(NPC.Center, FateDarkPink, 6, 100f * (1f - progress * 0.3f), 0.03f);
                }
                
                // Safe zone indicators
                if (Timer > chargeTime * 0.5f)
                {
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(28f - difficultyTier * 3f), 200f, 8);
                }
                
                if (Timer > chargeTime * 0.8f)
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
                // Fire waves with safe arc
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(18f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 1.8f, 28);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 36 + difficultyTier * 8;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(25f - difficultyTier * 3f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // Safe arc exemption
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = (14f + difficultyTier * 2f + SubPhase * 1.5f) * GetAggressionSpeedMult();
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 85, GetCosmicGradient((float)i / projectileCount), 10f);
                        }
                    }
                    
                    // Cascading halos
                    for (int i = 0; i < 8; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 8f), 0.35f + i * 0.15f, 16 + i * 4);
                    }
                    
                    // Glyph explosion
                    CustomParticles.GlyphBurst(NPC.Center, FateBrightRed, 10, 7f);
                }
                
                if (Timer >= 25)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 28) EndAttack();
            }
        }
        
        /// <summary>
        /// COSMIC VORTEX - Creates gravity well that pulls player
        /// Must fight against pull while avoiding projectiles
        /// </summary>
        private void Attack_CosmicVortex(Player target)
        {
            int duration = (int)((100 + difficultyTier * 20) * GetAggressionRateMult());
            
            // Create vortex at player's position at start
            if (Timer == 1)
            {
                storedStarPositions.Clear();
                storedStarPositions.Add(target.Center); // Store vortex center
                SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.8f, Volume = 1.2f }, target.Center);
            }
            
            if (storedStarPositions.Count == 0) { EndAttack(); return; }
            Vector2 vortexCenter = storedStarPositions[0];
            
            // Pull player toward vortex (handled via debuff or direct velocity in real implementation)
            float pullStrength = 0.3f + difficultyTier * 0.1f;
            Vector2 pullDir = (vortexCenter - target.Center).SafeNormalize(Vector2.Zero);
            target.velocity += pullDir * pullStrength;
            
            // Vortex VFX
            if (Timer % 3 == 0)
            {
                float vortexAngle = Timer * 0.1f;
                for (int i = 0; i < 4; i++)
                {
                    float angle = vortexAngle + MathHelper.TwoPi * i / 4f;
                    float radius = 60f + (float)Math.Sin(Timer * 0.05f) * 20f;
                    Vector2 pos = vortexCenter + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, FatePurple * 0.7f, 0.35f, 12);
                }
            }
            
            // Central glyph pulses
            if (Timer % 12 == 0)
            {
                CustomParticles.Glyph(vortexCenter, FateBrightRed, 0.6f, -1);
                CustomParticles.HaloRing(vortexCenter, FateDarkPink * 0.5f, 0.4f, 15);
            }
            
            // Spawn projectiles from vortex edge
            if (Timer % 15 == 0 && Timer > 20 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int projCount = 4 + difficultyTier;
                for (int i = 0; i < projCount; i++)
                {
                    float angle = MathHelper.TwoPi * i / projCount + Timer * 0.02f;
                    Vector2 spawnPos = vortexCenter + angle.ToRotationVector2() * 80f;
                    Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed;
                    BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 80, FateBrightRed, 0.01f);
                }
            }
            
            // Boss hovers opposite to vortex from player
            Vector2 idealPos = target.Center + (target.Center - vortexCenter).SafeNormalize(Vector2.UnitY) * 300f;
            idealPos.Y = Math.Min(idealPos.Y, target.Center.Y - 200f);
            Vector2 toIdeal = idealPos - NPC.Center;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal.SafeNormalize(Vector2.Zero) * 10f, 0.05f);
            
            if (Timer >= duration)
            {
                storedStarPositions.Clear();
                EndAttack();
            }
        }
        
        /// <summary>
        /// FINAL MELODY - Multi-phase ultimate attack
        /// The crescendo of the fight
        /// </summary>
        private void Attack_FinalMelody(Player target)
        {
            int phase1Time = 28;
            int phase2Time = 42;
            int phase3Time = 35;
            
            if (SubPhase == 0)
            {
                // Phase 1: Center and charge
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    Main.NewText("THE FINAL MELODY BEGINS!", FateBrightRed);
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f, Volume = 1.5f }, NPC.Center);
                }
                
                float progress = Timer / (float)phase1Time;
                
                // Massive glyph circle forms
                if (Timer % 6 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f + Timer * 0.03f;
                        float radius = 200f - progress * 50f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.Glyph(pos, GetCosmicGradient(progress), 0.5f, -1);
                    }
                }
                
                if (Timer >= phase1Time)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Phase 2: Rapid multi-directional bursts
                int burstInterval = Math.Max(8, 15 - difficultyTier * 2);
                
                if (Timer % burstInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = Timer * 0.1f;
                    int projPerBurst = 6 + difficultyTier;
                    
                    for (int i = 0; i < projPerBurst; i++)
                    {
                        float angle = baseAngle + MathHelper.TwoPi * i / projPerBurst;
                        Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed;
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 80, GetCosmicGradient((float)i / projPerBurst), 8f);
                    }
                    
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, FateBrightRed, FateDarkPink, 0.7f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 0.7f }, NPC.Center);
                }
                
                if (Timer >= phase2Time)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                // Phase 3: Grand finale - safe arc judgment
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(25f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 2f, Pitch = -0.2f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 2.5f, 35);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 48 + difficultyTier * 10;
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(22f - difficultyTier * 2f);
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            float speed = 16f + difficultyTier * 3f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 90, FateBrightRed, 12f);
                        }
                    }
                    
                    // Epic cascading halos
                    for (int i = 0; i < 12; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 12f), 0.3f + i * 0.2f, 18 + i * 5);
                    }
                    
                    CustomParticles.GlyphBurst(NPC.Center, FateBrightRed, 16, 10f);
                }
                
                if (Timer >= phase3Time)
                {
                    EndAttack();
                }
            }
        }
        
        #endregion

        #region Animation & Visuals
        
        private void UpdateAnimation()
        {
            frameCounter++;
            if (frameCounter >= 5)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= Main.npcFrameCount[Type])
                    currentFrame = 0;
            }
        }
        
        public override void FindFrame(int frameHeight)
        {
            NPC.frame.Y = currentFrame * frameHeight;
        }
        
        private void SpawnAmbientParticles()
        {
            // Performance gate - skip under critical load
            if (BossVFXOptimizer.IsCriticalLoad) return;
            
            // Orbiting glyphs
            if (Main.GameUpdateCount % 20 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = glyphOrbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 70f + (float)Math.Sin(Main.GameUpdateCount * 0.03f + i) * 15f;
                    Vector2 glyphPos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.Glyph(glyphPos, FateDarkPink * 0.6f, 0.35f, -1);
                }
            }
            
            // Star sparkles - reduce under high load
            int sparkleChance = BossVFXOptimizer.IsHighLoad ? 16 : 8;
            if (Main.rand.NextBool(sparkleChance))
            {
                Vector2 starOffset = Main.rand.NextVector2Circular(100f, 100f);
                CustomParticles.GenericFlare(NPC.Center + starOffset, FateWhite * 0.4f, 0.2f, 12);
            }
            
            // Cosmic cloud trail - reduce under high load
            int cloudChance = BossVFXOptimizer.IsHighLoad ? 6 : 3;
            if (NPC.velocity.Length() > 5f && Main.rand.NextBool(cloudChance))
            {
                Vector2 cloudOffset = Main.rand.NextVector2Circular(20f, 20f);
                var cloud = new GenericGlowParticle(NPC.Center + cloudOffset, -NPC.velocity * 0.1f, FatePurple * 0.4f, 0.3f, 20, true);
                MagnumParticleHandler.SpawnParticle(cloud);
            }
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            
            int frameHeight = texture.Height / Main.npcFrameCount[Type];
            Rectangle frame = new Rectangle(0, currentFrame * frameHeight, texture.Width, frameHeight);
            
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = new Vector2(texture.Width / 2f, frameHeight / 2f);
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
            
            // Afterimage trail
            for (int i = NPC.oldPos.Length - 1; i >= 0; i--)
            {
                float progress = (float)i / NPC.oldPos.Length;
                float trailAlpha = (1f - progress) * 0.4f;
                Color trailColor = GetCosmicGradient(progress) * trailAlpha;
                trailColor.A = 0;
                
                Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                spriteBatch.Draw(texture, trailPos, frame, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.2f), effects, 0f);
            }
            
            // Glow underlayer
            Color glowColor = FateBrightRed * 0.3f;
            glowColor.A = 0;
            spriteBatch.Draw(texture, drawPos, frame, glowColor, NPC.rotation, origin, NPC.scale * 1.1f, effects, 0f);
            
            // Main draw with natural colors (custom sprite already has correct colors)
            spriteBatch.Draw(texture, drawPos, frame, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion

        #region Death Animation & True Form Awakening
        
        /// <summary>
        /// TRUE FORM AWAKENING - When Fate is "killed" for the first time,
        /// he refuses to fall and awakens his true cosmic power.
        /// </summary>
        private void UpdateAwakening()
        {
            awakeningTimer++;
            NPC.velocity *= 0.92f;
            NPC.dontTakeDamage = true;
            
            float progress = (float)awakeningTimer / AwakeningDuration;
            
            // Phase 1: Collapse and despair (0-90 frames / 1.5 sec)
            if (awakeningTimer <= 90)
            {
                float collapseProgress = awakeningTimer / 90f;
                
                // Flickering, failing cosmic energy
                if (awakeningTimer % 8 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f + awakeningTimer * 0.02f;
                        float radius = 100f + (float)Math.Sin(awakeningTimer * 0.3f) * 30f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color flickerColor = GetCosmicGradient(Main.rand.NextFloat()) * (0.3f + Main.rand.NextFloat() * 0.4f);
                        CustomParticles.GenericFlare(pos, flickerColor, 0.3f, 12);
                    }
                }
                
                // Dialogue - the struggle
                if (awakeningTimer == 30)
                    Main.NewText("No... this cannot be...", FateBrightRed);
                    
                if (awakeningTimer == 70)
                    Main.NewText("I will NOT fall here!", FateDarkPink);
            }
            // Phase 2: Rising defiance (90-150 frames / 1 sec)
            else if (awakeningTimer <= 150)
            {
                float risingProgress = (awakeningTimer - 90) / 60f;
                
                // Energy starts gathering intensely
                if (awakeningTimer % 4 == 0)
                {
                    int particleCount = (int)(8 + risingProgress * 20);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + awakeningTimer * 0.08f;
                        float radius = 200f * (1f - risingProgress * 0.6f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.GenericFlare(pos, Color.Lerp(FateDarkPink, FateWhite, risingProgress), 0.4f + risingProgress * 0.5f, 20);
                    }
                    
                    // Glyphs converging
                    CustomParticles.GlyphCircle(NPC.Center, FateBrightRed * (0.5f + risingProgress * 0.5f), 6, 80f * (1f - risingProgress * 0.5f), 0.06f);
                }
                
                MagnumScreenEffects.AddScreenShake(risingProgress * 15f);
                
                // Epic dialogue - the resolve
                if (awakeningTimer == 100)
                    Main.NewText("You think you can UNSHACKLE those melodies?!", FateWhite);
                    
                if (awakeningTimer == 130)
                    Main.NewText("I am the WARDEN of this universe's symphony!", Color.Lerp(FateBrightRed, FateWhite, 0.5f));
            }
            // Phase 3: True Form Emergence (150-240 frames / 1.5 sec)
            else if (awakeningTimer <= 240)
            {
                float emergenceProgress = (awakeningTimer - 150) / 90f;
                
                // Intense cosmic energy spiral
                if (awakeningTimer % 3 == 0)
                {
                    int spiralCount = 3;
                    for (int arm = 0; arm < spiralCount; arm++)
                    {
                        float baseAngle = awakeningTimer * 0.12f + MathHelper.TwoPi * arm / spiralCount;
                        for (int point = 0; point < 8; point++)
                        {
                            float spiralAngle = baseAngle + point * 0.3f;
                            float spiralRadius = 30f + point * 20f;
                            Vector2 spiralPos = NPC.Center + spiralAngle.ToRotationVector2() * spiralRadius;
                            Color spiralColor = GetCosmicGradient((float)point / 8f + emergenceProgress);
                            CustomParticles.GenericFlare(spiralPos, spiralColor, 0.5f, 15);
                        }
                    }
                }
                
                // Star particles exploding outward
                if (awakeningTimer % 5 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 starVel = Main.rand.NextVector2Unit() * Main.rand.NextFloat(6f, 14f);
                        var star = new GenericGlowParticle(NPC.Center, starVel, FateWhite * 0.8f, 0.35f, 25, true);
                        MagnumParticleHandler.SpawnParticle(star);
                    }
                }
                
                // Cascading halo rings
                if (awakeningTimer % 10 == 0)
                {
                    int ringIndex = (awakeningTimer - 150) / 10;
                    CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(ringIndex / 9f), 0.4f + ringIndex * 0.15f, 25);
                }
                
                MagnumScreenEffects.AddScreenShake(12f + emergenceProgress * 20f);
                
                // The climactic declaration
                if (awakeningTimer == 180)
                {
                    Main.NewText("I WILL NOT LET YOU CONDUCT THE FINAL MOVEMENT!", FateWhite);
                    SoundEngine.PlaySound(SoundID.Roar with { Volume = 1.8f, Pitch = -0.5f }, NPC.Center);
                }
                
                if (awakeningTimer == 220)
                    Main.NewText("WITNESS MY TRUE FORM!", Color.Lerp(FateBrightRed, Color.White, 0.7f));
            }
            // Phase 4: Rebirth Explosion (240-300 frames / 1 sec)
            else
            {
                float finalProgress = (awakeningTimer - 240) / 60f;
                
                // The great rebirth burst at start of phase
                if (awakeningTimer == 241)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 2.5f, Pitch = 0.3f }, NPC.Center);
                    
                    // Massive white flash
                    CustomParticles.GenericFlare(NPC.Center, FateWhite, 4f, 45);
                    CustomParticles.GenericFlare(NPC.Center, FateBrightRed, 3f, 40);
                    
                    // 20 cascading halos
                    for (int i = 0; i < 20; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 20f), 0.4f + i * 0.2f, 25 + i * 4);
                    }
                    
                    // Massive glyph explosion
                    CustomParticles.GlyphBurst(NPC.Center, FateWhite, 30, 18f);
                    
                    // Radial cosmic storm
                    for (int i = 0; i < 50; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 50f;
                        Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(12f, 24f);
                        var cosmicSpark = new GenericGlowParticle(NPC.Center, vel, GetCosmicGradient((float)i / 50f), 0.6f, 40, true);
                        MagnumParticleHandler.SpawnParticle(cosmicSpark);
                    }
                }
                
                // Continue particle effects during recovery
                if (awakeningTimer % 6 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + awakeningTimer * 0.05f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * 60f;
                        CustomParticles.GenericFlare(pos, FateWhite * 0.6f, 0.4f, 15);
                    }
                }
                
                // Final motivational declaration
                if (awakeningTimer == 270)
                    Main.NewText("Now... you face the UNSHACKLED WARDEN!", new Color(255, 200, 220));
            }
            
            // AWAKENING COMPLETE - Restore to full power with enhanced stats
            if (awakeningTimer >= AwakeningDuration)
            {
                hasAwakened = true;
                
                // RESTORE FULL HEALTH
                NPC.life = NPC.lifeMax;
                
                // ENHANCED STATS for True Form
                NPC.damage = (int)(BaseDamage * 1.4f);  // 40% more damage
                NPC.defense = 110;                       // More defense
                
                // Reset difficulty tier tracking (will immediately update to tier 0)
                difficultyTier = -1;
                
                // Reset fight state
                State = BossPhase.Idle;
                Timer = 0;
                SubPhase = 0;
                awakeningTimer = 0;
                deathTimer = 0;
                NPC.dontTakeDamage = false;
                
                // Aggression starts high in true form
                aggressionLevel = 0.5f;
                
                // True form announcement
                Main.NewText("「 THE TRUE FATE AWAKENS 」", new Color(255, 180, 200));
                
                // Intense ambient burst
                for (int i = 0; i < 12; i++)
                {
                    float angle = MathHelper.TwoPi * i / 12f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * 80f;
                    CustomParticles.Glyph(pos, FateWhite * 0.8f, 0.5f, -1);
                }
            }
        }
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            NPC.dontTakeDamage = true;
            
            float progress = (float)deathTimer / 180f;
            
            // Building cosmic energy
            if (deathTimer < 120)
            {
                if (deathTimer % 5 == 0)
                {
                    int particleCount = (int)(8 + progress * 16);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + deathTimer * 0.05f;
                        float radius = 150f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.GenericFlare(pos, GetCosmicGradient(progress), 0.4f + progress * 0.4f, 18);
                    }
                }
                
                if (deathTimer % 10 == 0)
                {
                    CustomParticles.GlyphCircle(NPC.Center, FateDarkPink, 8, 100f * (1f - progress * 0.4f), 0.04f);
                }
                
                MagnumScreenEffects.AddScreenShake(progress * 8f);
            }
            // Climax
            else if (deathTimer == 150)
            {
                MagnumScreenEffects.AddScreenShake(30f);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 2f, Pitch = -0.3f }, NPC.Center);
                
                // Massive cosmic explosion
                CustomParticles.GenericFlare(NPC.Center, FateWhite, 3f, 40);
                
                for (int i = 0; i < 16; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, GetCosmicGradient(i / 16f), 0.3f + i * 0.25f, 20 + i * 5);
                }
                
                CustomParticles.GlyphBurst(NPC.Center, FateBrightRed, 20, 12f);
                
                // Radial spark storm
                for (int i = 0; i < 30; i++)
                {
                    float angle = MathHelper.TwoPi * i / 30f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 16f);
                    var spark = new GenericGlowParticle(NPC.Center, vel, GetCosmicGradient((float)i / 30f), 0.5f, 35, true);
                    MagnumParticleHandler.SpawnParticle(spark);
                }
            }
            
            if (deathTimer >= 180)
            {
                // TRUE death dialogue (after awakening) - pass whether this is the first kill
                bool isFirstKill = !MoonlightSonataSystem.FateBossKilledOnce;
                BossDialogueSystem.Fate.OnDeath(isFirstKill);
                BossDialogueSystem.CleanupDialogue(NPC.whoAmI);
                
                // Deactivate the cosmic Fate sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:FateSky"] != null)
                {
                    SkyManager.Instance.Deactivate("MagnumOpus:FateSky");
                }
                
                NPC.life = 0;
                NPC.HitEffect();
                NPC.checkDead();
            }
        }
        
        public override bool CheckDead()
        {
            // FIRST "DEATH" - Trigger True Form Awakening
            if (!hasAwakened)
            {
                State = BossPhase.Awakening;
                Timer = 0;
                awakeningTimer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                return false;
            }
            
            // SECOND DEATH - Actual death after awakening
            if (deathTimer < 180)
            {
                State = BossPhase.Dying;
                Timer = 0;
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                return false;
            }
            
            BossHealthBarUI.UnregisterBoss(NPC.whoAmI);
            
            return true;
        }
        
        #endregion

        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Expert/Master mode: Treasure Bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<FateTreasureBag>()));
            
            // Non-Expert drops (only when not in Expert mode)
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            
            // Drop Fate crafting materials in normal mode
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<FateResonantEnergy>(), 1, 20, 35));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfTheGalaxysHarmony>(), 1, 25, 35));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShardOfFatesTempo>(), 1, 10, 18));
            
            // Random weapon drop in normal mode (1 weapon)
            notExpert.OnSuccess(ItemDropRule.OneFromOptions(1, 
                ModContent.ItemType<Content.Fate.ResonantWeapons.CodaOfAnnihilation>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.DestinysCrescendo>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.FractalOfTheStars>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.LightOfTheFuture>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.OpusUltima>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.RequiemOfReality>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.ResonanceOfABygoneReality>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.SymphonysEnd>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.TheConductorsLastConstellation>(),
                ModContent.ItemType<Content.Fate.ResonantWeapons.TheFinalFermata>()
            ));
            
            npcLoot.Add(notExpert);
            
            // Seed of Universal Melodies - legendary crafting material (always drops 1-2)
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<Content.Items.SeedOfUniversalMelodies>(), 1, 1, 2));
            
            // Trophy and mask (commented until items exist)
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateTrophy>(), 10));
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FateMask>(), 7));
        }
        
        #endregion
    }
}
