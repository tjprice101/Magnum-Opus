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
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Common;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;

namespace MagnumOpus.Content.Nachtmusik.Bosses
{
    /// <summary>
    /// NACHTMUSIK, QUEEN OF RADIANCE - POST-FATE BOSS
    /// 
    /// Design Philosophy:
    /// - Elegant celestial goddess who conducts the night sky
    /// - Two-phase boss with dramatic "fake death" transition
    /// - Phase 1: Graceful, flowing attacks with starlight patterns
    /// - Phase 2: Celestial Fury form - aggressive cosmic assault
    /// 
    /// Unique Mechanic:
    /// - When HP reaches 0 in Phase 1, the music fades out
    /// - Boss "dies" dramatically, then rises as Celestial Fury (Phase 2)
    /// - New music track begins: "...as the Stars Sing Into Your Heart"
    /// - Full HP restored with new attacks and intensified patterns
    /// 
    /// Musical Theme:
    /// - Phase 1: "Moonlit Resonance Overflows...." (ethereal, graceful)
    /// - Phase 2: "...as the Stars Sing Into Your Heart" (intense, cosmic)
    /// </summary>
    public class NachtmusikQueenOfRadiance : ModNPC
    {
        // Phase 1 sprite - Graceful Queen
        public override string Texture => "MagnumOpus/Content/Nachtmusik/Bosses/NachtmusikQueenOfRadiance";
        
        // Phase 2 sprite path
        private const string Phase2Texture = "MagnumOpus/Content/Nachtmusik/Bosses/NachtmusikCelestialFury";
        
        #region Theme Colors - Nachtmusik Palette
        private static readonly Color DeepPurple = new Color(45, 27, 78);       // #2D1B4E
        private static readonly Color Gold = new Color(255, 215, 0);             // #FFD700
        private static readonly Color Violet = new Color(123, 104, 238);         // #7B68EE
        private static readonly Color StarWhite = new Color(255, 255, 255);      // #FFFFFF
        private static readonly Color NightBlue = new Color(25, 25, 112);        // Midnight Blue accent
        #endregion
        
        #region Constants
        private const float BaseSpeed = 18f;
        private const int BaseDamage = 180;
        private const float EnrageDistance = 1100f;
        private const float TeleportDistance = 1600f;
        private const int AttackWindowFrames = 45;
        
        // Projectile speeds
        private const float FastProjectileSpeed = 20f;
        private const float MediumProjectileSpeed = 14f;
        private const float HomingSpeed = 8f;
        
        // Phase 2 stats multipliers
        private const float Phase2DamageMult = 1.25f;
        private const float Phase2SpeedMult = 1.3f;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Phase1_Idle,
            Phase1_Attack,
            Phase1_Reposition,
            FakeDeath,          // Dramatic "death" transition
            Phase2_Awakening,   // Rising as Celestial Fury
            Phase2_Idle,
            Phase2_Attack,
            Phase2_Reposition,
            Enraged,
            TrueDeath
        }
        
        private enum AttackPattern
        {
            // Phase 1 - Graceful Attacks
            StarlightWaltz,       // Sweeping projectile arcs
            ConstellationDance,   // Star pattern formation
            MoonbeamCascade,      // Vertical light rays
            NocturnalSerenade,    // Homing musical notes
            CrescentSlash,        // Crescent-shaped projectiles
            
            // Phase 2 - Celestial Fury Attacks  
            CosmicTempest,        // Rapid chaotic stars
            NebulaBurst,          // Expanding star explosions
            GalacticJudgment,     // Safe-arc radial burst
            StarfallApocalypse,   // Raining star barrage
            EternalNightmare,     // Multi-phase ultimate
            CelestialCharge       // High-speed dashes
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
        private AttackPattern lastAttack = AttackPattern.StarlightWaltz;
        
        private Vector2 dashTarget;
        private Vector2 dashDirection;
        private int dashCount = 0;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        private int fightTimer = 0;
        private float aggressionLevel = 0f;
        private const int MaxAggressionTime = 2700; // 45 seconds
        
        private bool hasRegisteredHealthBar = false;
        private int deathTimer = 0;
        
        // Phase tracking
        private bool isPhase2 = false;
        private bool fakeDeathTriggered = false;
        private int phase1MaxLife = 0;
        
        // Orbit tracking for ambient effects
        private float starOrbitAngle = 0f;
        private float crescentOrbitAngle = 0f;
        
        // Music tracking
        private int phase1MusicSlot = -1;
        private int phase2MusicSlot = -1;
        private float musicFadeProgress = 0f;
        #endregion

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 1;
            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frostburn] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Frozen] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 120;
            NPC.height = 140;
            NPC.damage = BaseDamage;
            NPC.defense = 120;
            NPC.lifeMax = 2500000; // 2.5 million HP (must defeat twice)
            NPC.HitSound = SoundID.NPCHit5;
            NPC.DeathSound = SoundID.NPCDeath7;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 40);
            NPC.boss = true;
            NPC.npcSlots = 18f;
            NPC.aiStyle = -1;
            NPC.scale = 1.0f;
            
            if (!Main.dedServ)
            {
                phase1MusicSlot = MusicLoader.GetMusicSlot(Mod, "Assets/Music/Moonlit Resonance Overflows...");
                phase2MusicSlot = MusicLoader.GetMusicSlot(Mod, "Assets/Music/...as the Stars Sing Into Your Heart");
                
                // Fallback to vanilla boss music if custom tracks don't exist
                if (phase1MusicSlot == -1)
                    phase1MusicSlot = MusicID.Boss3; // Moonlord-style as fallback
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
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement(
                    "Nachtmusik, Queen of Radiance - The divine conductor of the celestial symphony. " +
                    "Her graceful presence brings starlight to the darkness, but challenge her " +
                    "and witness the fury of a thousand burning suns.")
            });
        }

        #region Color Helpers
        private Color GetNachtmusikGradient(float progress)
        {
            if (progress < 0.33f)
                return Color.Lerp(DeepPurple, Violet, progress * 3f);
            else if (progress < 0.66f)
                return Color.Lerp(Violet, Gold, (progress - 0.33f) * 3f);
            else
                return Color.Lerp(Gold, StarWhite, (progress - 0.66f) * 3f);
        }
        
        private float GetAggressionSpeedMult() => 1f + aggressionLevel * 0.35f;
        private float GetAggressionRateMult() => 1f - aggressionLevel * 0.2f;
        
        private float GetPhaseSpeedMult() => isPhase2 ? Phase2SpeedMult : 1f;
        private float GetPhaseDamageMult() => isPhase2 ? Phase2DamageMult : 1f;
        #endregion

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Nachtmusik);
                hasRegisteredHealthBar = true;
            }
            
            // Handle death states
            if (State == BossPhase.TrueDeath)
            {
                UpdateTrueDeathAnimation();
                return;
            }
            
            if (State == BossPhase.FakeDeath)
            {
                UpdateFakeDeathTransition();
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
            
            // Check for Phase 1 "death" - trigger fake death
            if (!isPhase2 && !fakeDeathTriggered && NPC.life <= 1)
            {
                TriggerFakeDeath();
                return;
            }
            
            UpdateDifficultyTier();
            UpdateAggression();
            UpdateOrbits();
            if (attackCooldown > 0) attackCooldown--;
            CheckEnrage(target);
            
            switch (State)
            {
                case BossPhase.Spawning:
                    AI_Spawning(target);
                    break;
                case BossPhase.Phase1_Idle:
                    AI_Phase1_Idle(target);
                    break;
                case BossPhase.Phase1_Attack:
                    AI_Phase1_Attack(target);
                    break;
                case BossPhase.Phase1_Reposition:
                    AI_Reposition(target);
                    break;
                case BossPhase.Phase2_Idle:
                    AI_Phase2_Idle(target);
                    break;
                case BossPhase.Phase2_Attack:
                    AI_Phase2_Attack(target);
                    break;
                case BossPhase.Phase2_Reposition:
                    AI_Reposition(target);
                    break;
                case BossPhase.Enraged:
                    AI_Enraged(target);
                    break;
            }
            
            Timer++;
            fightTimer++;
            SpawnAmbientParticles();
            
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            
            float lightIntensity = isEnraged ? 1.3f : (isPhase2 ? 1.1f : 0.9f);
            Color lightColor = isPhase2 ? Gold : Violet;
            Lighting.AddLight(NPC.Center, lightColor.ToVector3() * lightIntensity);
        }

        public override bool CheckDead()
        {
            // In Phase 1, prevent actual death - trigger fake death instead
            if (!isPhase2 && !fakeDeathTriggered)
            {
                NPC.life = 1;
                TriggerFakeDeath();
                return false;
            }
            
            // In Phase 2, allow real death but with animation
            if (isPhase2 && State != BossPhase.TrueDeath)
            {
                NPC.life = 1;
                NPC.dontTakeDamage = true;
                State = BossPhase.TrueDeath;
                Timer = 0;
                
                // Note: DownedNachtmusik flag is set in OnKill()
                
                return false;
            }
            
            return State == BossPhase.TrueDeath && deathTimer >= 180;
        }

        #region Phase Transitions
        
        private void TriggerFakeDeath()
        {
            fakeDeathTriggered = true;
            NPC.dontTakeDamage = true;
            NPC.velocity = Vector2.Zero;
            State = BossPhase.FakeDeath;
            Timer = 0;
            musicFadeProgress = 0f;
            
            SoundEngine.PlaySound(SoundID.NPCDeath7 with { Volume = 1.5f, Pitch = -0.3f }, NPC.Center);
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("The Queen's light... fades...", Violet);
            }
        }
        
        private void UpdateFakeDeathTransition()
        {
            Timer++;
            NPC.velocity *= 0.95f;
            
            // Music fade out (0-90 frames)
            if (Timer <= 90)
            {
                musicFadeProgress = Timer / 90f;
                
                // Dramatic death particles
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.02f;
                        Vector2 offset = angle.ToRotationVector2() * (50f + Timer);
                        Color color = Color.Lerp(Violet, DeepPurple, musicFadeProgress);
                        CustomParticles.GenericFlare(NPC.Center + offset, color * (1f - musicFadeProgress), 0.5f, 15);
                    }
                }
                
                // Stars falling away
                if (Timer % 8 == 0)
                {
                    Vector2 starPos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                    Vector2 starVel = new Vector2(0, 3f) + Main.rand.NextVector2Circular(1f, 0.5f);
                    CustomParticles.GenericFlare(starPos, Gold * (1f - musicFadeProgress * 0.7f), 0.4f, 30);
                }
            }
            // Silence and darkness (90-150 frames)
            else if (Timer <= 150)
            {
                if (Timer == 91)
                {
                    // Complete darkness pulse
                    MagnumScreenEffects.AddScreenShake(8f);
                }
                
                // Brief darkness
                NPC.alpha = Math.Min(255, NPC.alpha + 5);
            }
            // Phase 2 awakening begins (150+)
            else if (Timer == 151)
            {
                BeginPhase2Awakening();
            }
        }
        
        private void BeginPhase2Awakening()
        {
            isPhase2 = true;
            State = BossPhase.Phase2_Awakening;
            Timer = 0;
            
            // Restore HP for Phase 2
            NPC.life = NPC.lifeMax;
            NPC.dontTakeDamage = true;
            NPC.alpha = 255;
            
            // Switch music
            if (!Main.dedServ)
            {
                Music = phase2MusicSlot;
            }
            
            // Increase stats for Phase 2
            NPC.damage = (int)(BaseDamage * Phase2DamageMult);
            NPC.defense = 140;
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("...but the stars refuse to dim.", Gold);
            }
        }
        
        private void UpdatePhase2Awakening()
        {
            Timer++;
            
            // Gradual appearance (0-60 frames)
            if (Timer <= 60)
            {
                NPC.alpha = Math.Max(0, 255 - (int)(Timer * 4.25f));
                
                // Rising cosmic particles
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 6f;
                        Vector2 offset = angle.ToRotationVector2() * (200f - Timer * 2.5f);
                        Vector2 vel = -offset.SafeNormalize(Vector2.Zero) * 4f;
                        Color color = GetNachtmusikGradient((float)i / 6f);
                        CustomParticles.GenericFlare(NPC.Center + offset, color, 0.4f + Timer / 120f, 20);
                    }
                }
            }
            // Dramatic reveal (60-90 frames)
            else if (Timer <= 90)
            {
                float progress = (Timer - 60) / 30f;
                
                if (Timer == 61)
                {
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.5f, Volume = 1.5f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(15f);
                }
                
                // Expanding gold halos
                if (Timer % 5 == 0)
                {
                    CustomParticles.HaloRing(NPC.Center, Gold, 0.3f + progress * 0.5f, 25);
                    CustomParticles.HaloRing(NPC.Center, Violet, 0.2f + progress * 0.4f, 22);
                }
                
                // Star burst
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f + Timer * 0.05f;
                        Vector2 vel = angle.ToRotationVector2() * (5f + progress * 8f);
                        CustomParticles.GenericFlare(NPC.Center, Gold, 0.5f, 18);
                    }
                }
            }
            // Ready for battle (90-120 frames)
            else if (Timer <= 120)
            {
                if (Timer == 91)
                {
                    // Ultimate reveal flash
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 2.0f, 30);
                    for (int i = 0; i < 16; i++)
                    {
                        float scale = 0.2f + i * 0.1f;
                        Color color = Color.Lerp(Gold, Violet, i / 16f);
                        CustomParticles.HaloRing(NPC.Center, color, scale, 20 + i * 2);
                    }
                    
                    Main.NewText("NACHTMUSIK, CELESTIAL FURY awakens!", Gold);
                }
            }
            // Begin Phase 2
            else
            {
                NPC.dontTakeDamage = false;
                State = BossPhase.Phase2_Idle;
                Timer = 0;
                attackCooldown = 60;
                difficultyTier = 0; // Reset difficulty tier for Phase 2 progression
            }
        }
        
        #endregion

        #region Core AI Methods
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.7f ? 0 : (hpPercent > 0.4f ? 1 : 2);
            
            if (newTier != difficultyTier)
            {
                int oldTier = difficultyTier;
                difficultyTier = newTier;
                AnnounceDifficultyChange(oldTier);
            }
        }
        
        private void AnnounceDifficultyChange(int oldTier)
        {
            if (!isPhase2 && oldTier < difficultyTier) return; // Phase 1 doesn't announce
            
            MagnumScreenEffects.AddScreenShake(difficultyTier == 2 ? 15f : 10f);
            SoundEngine.PlaySound(SoundID.Roar with { Pitch = difficultyTier * 0.15f }, NPC.Center);
            
            // VFX burst
            CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.0f, 20);
            for (int i = 0; i < 10 + difficultyTier * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (10 + difficultyTier * 4);
                Color color = GetNachtmusikGradient((float)i / (10 + difficultyTier * 4));
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 50f, color, 0.5f, 15);
            }
            
            if (isPhase2)
            {
                string[] messages = {
                    "The cosmos trembles...",
                    "Stars align for destruction!",
                    "WITNESS ETERNAL NIGHT!"
                };
                if (Main.netMode != NetmodeID.Server && difficultyTier < messages.Length)
                {
                    Main.NewText(messages[difficultyTier], difficultyTier == 2 ? Gold : Violet);
                }
            }
        }
        
        private void UpdateAggression()
        {
            if (fightTimer < MaxAggressionTime)
            {
                aggressionLevel = (float)fightTimer / MaxAggressionTime;
            }
            else
            {
                aggressionLevel = 1f;
            }
        }
        
        private void UpdateOrbits()
        {
            starOrbitAngle += 0.02f + aggressionLevel * 0.01f;
            crescentOrbitAngle -= 0.015f - aggressionLevel * 0.005f;
            
            if (starOrbitAngle > MathHelper.TwoPi) starOrbitAngle -= MathHelper.TwoPi;
            if (crescentOrbitAngle < 0) crescentOrbitAngle += MathHelper.TwoPi;
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
                if (enrageTimer > 90 && !isEnraged)
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("You cannot escape the night!", Gold);
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.3f }, NPC.Center);
                }
            }
            else
            {
                enrageTimer = Math.Max(0, enrageTimer - 3);
                if (isEnraged && enrageTimer == 0)
                {
                    isEnraged = false;
                    State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
                    Timer = 0;
                }
            }
        }
        
        private void TeleportToPlayer(Player target)
        {
            // Departure VFX
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, Violet, 0.8f, 15);
            for (int i = 0; i < 8; i++)
            {
                float angle = MathHelper.TwoPi * i / 8f;
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 40f, Gold, 0.4f, 12);
            }
            
            // Teleport
            Vector2 offset = Main.rand.NextVector2CircularEdge(400f, 400f);
            NPC.Center = target.Center + offset;
            
            // Arrival VFX
            SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.4f }, NPC.Center);
            CustomParticles.GenericFlare(NPC.Center, Gold, 0.8f, 15);
            CustomParticles.HaloRing(NPC.Center, Violet, 0.5f, 18);
        }
        
        #endregion

        #region Phase 1 AI
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f }, NPC.Center);
                if (Main.netMode != NetmodeID.Server)
                    Main.NewText("The Queen of Radiance descends...", Violet);
            }
            
            // Graceful descent
            NPC.velocity.Y = -3f + Timer * 0.05f;
            if (NPC.velocity.Y > 0) NPC.velocity.Y = 0;
            
            // Spawning particles
            if (Timer % 5 == 0)
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle = MathHelper.TwoPi * i / 6f + Timer * 0.03f;
                    Vector2 offset = angle.ToRotationVector2() * (150f - Timer);
                    Color color = GetNachtmusikGradient((float)i / 6f);
                    CustomParticles.GenericFlare(NPC.Center + offset, color, 0.4f, 15);
                }
            }
            
            if (Timer >= 90)
            {
                State = BossPhase.Phase1_Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames;
                
                CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.2f, 25);
                for (int i = 0; i < 10; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(DeepPurple, Gold, i / 10f), 0.3f + i * 0.1f, 18 + i * 2);
                }
            }
        }
        
        private void AI_Phase1_Idle(Player target)
        {
            // Graceful hovering
            float hoverHeight = -250f;
            float waveX = (float)Math.Sin(Timer * 0.015f) * 60f;
            float waveY = (float)Math.Sin(Timer * 0.02f) * 30f;
            
            Vector2 hoverPos = target.Center + new Vector2(waveX, hoverHeight + waveY);
            Vector2 toHover = hoverPos - NPC.Center;
            
            if (toHover.Length() > 20f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed * 0.5f, 0.04f);
            }
            else
            {
                NPC.velocity *= 0.95f;
            }
            
            if (attackCooldown <= 0)
            {
                SelectPhase1Attack(target);
            }
        }
        
        private void SelectPhase1Attack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.StarlightWaltz,
                AttackPattern.ConstellationDance,
                AttackPattern.MoonbeamCascade
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.NocturnalSerenade);
                pool.Add(AttackPattern.CrescentSlash);
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            State = BossPhase.Phase1_Attack;
            Timer = 0;
            SubPhase = 0;
        }
        
        private void AI_Phase1_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.StarlightWaltz:
                    Attack_StarlightWaltz(target);
                    break;
                case AttackPattern.ConstellationDance:
                    Attack_ConstellationDance(target);
                    break;
                case AttackPattern.MoonbeamCascade:
                    Attack_MoonbeamCascade(target);
                    break;
                case AttackPattern.NocturnalSerenade:
                    Attack_NocturnalSerenade(target);
                    break;
                case AttackPattern.CrescentSlash:
                    Attack_CrescentSlash(target);
                    break;
            }
        }
        
        #endregion

        #region Phase 2 AI
        
        private void AI_Phase2_Idle(Player target)
        {
            // More aggressive movement in Phase 2
            float hoverHeight = -200f;
            float waveX = (float)Math.Sin(Timer * 0.025f) * 100f;
            float waveY = (float)Math.Sin(Timer * 0.03f) * 50f;
            
            Vector2 hoverPos = target.Center + new Vector2(waveX, hoverHeight + waveY);
            Vector2 toHover = hoverPos - NPC.Center;
            
            if (toHover.Length() > 30f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed * Phase2SpeedMult * 0.6f, 0.05f);
            }
            else
            {
                NPC.velocity *= 0.93f;
            }
            
            if (attackCooldown <= 0)
            {
                SelectPhase2Attack(target);
            }
        }
        
        private void SelectPhase2Attack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.CosmicTempest,
                AttackPattern.NebulaBurst,
                AttackPattern.CelestialCharge
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.GalacticJudgment);
                pool.Add(AttackPattern.StarfallApocalypse);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.EternalNightmare);
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            State = BossPhase.Phase2_Attack;
            Timer = 0;
            SubPhase = 0;
        }
        
        private void AI_Phase2_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.CosmicTempest:
                    Attack_CosmicTempest(target);
                    break;
                case AttackPattern.NebulaBurst:
                    Attack_NebulaBurst(target);
                    break;
                case AttackPattern.GalacticJudgment:
                    Attack_GalacticJudgment(target);
                    break;
                case AttackPattern.StarfallApocalypse:
                    Attack_StarfallApocalypse(target);
                    break;
                case AttackPattern.EternalNightmare:
                    Attack_EternalNightmare(target);
                    break;
                case AttackPattern.CelestialCharge:
                    Attack_CelestialCharge(target);
                    break;
            }
        }
        
        #endregion

        #region Shared AI
        
        private void AI_Reposition(Player target)
        {
            Vector2 idealPos = target.Center + new Vector2(
                (Main.rand.NextBool() ? -1 : 1) * 350f,
                -280f
            );
            
            Vector2 toIdeal = idealPos - NPC.Center;
            float speed = BaseSpeed * GetPhaseSpeedMult() * GetAggressionSpeedMult();
            
            if (toIdeal.Length() > 50f)
            {
                toIdeal.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * speed, 0.08f);
            }
            else
            {
                State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
                Timer = 0;
                attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Rapid aggressive pursuit
            Vector2 toTarget = target.Center - NPC.Center;
            if (toTarget.Length() > 100f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 1.8f * GetPhaseSpeedMult(), 0.1f);
            }
            
            // Rapid projectile fire
            if (Timer % 15 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                float angle = (target.Center - NPC.Center).ToRotation();
                for (int i = -1; i <= 1; i++)
                {
                    Vector2 vel = (angle + i * 0.2f).ToRotationVector2() * FastProjectileSpeed;
                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(80 * GetPhaseDamageMult()), Gold, 5f);
                }
                CustomParticles.GenericFlare(NPC.Center, Gold, 0.5f, 12);
            }
        }
        
        private void EndAttack()
        {
            State = isPhase2 ? BossPhase.Phase2_Reposition : BossPhase.Phase1_Reposition;
            Timer = 0;
            SubPhase = 0;
        }
        
        #endregion

        #region Phase 1 Attacks
        
        /// <summary>
        /// Starlight Waltz - Sweeping arc of star projectiles
        /// </summary>
        private void Attack_StarlightWaltz(Player target)
        {
            int chargeTime = (int)((50 - difficultyTier * 8) * GetAggressionRateMult());
            int fireTime = 60;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.96f;
                float progress = Timer / (float)chargeTime;
                
                // Warning arc indicator
                if (Timer % 4 == 0)
                {
                    float arcStart = (target.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = arcStart + MathHelper.Pi * i / 8f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (150f + progress * 50f);
                        CustomParticles.GenericFlare(pos, Violet * 0.5f, 0.25f, 8);
                    }
                }
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 80f, progress, Violet, 6);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Fire
            {
                if (Timer % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = (target.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;
                    float sweepAngle = baseAngle + MathHelper.Pi * ((float)Timer / fireTime);
                    
                    Vector2 vel = sweepAngle.ToRotationVector2() * MediumProjectileSpeed;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, Gold, 0.01f);
                    
                    CustomParticles.GenericFlare(NPC.Center, Gold, 0.4f, 10);
                }
                
                if (Timer >= fireTime)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Constellation Dance - Star pattern that forms geometric shapes
        /// </summary>
        private void Attack_ConstellationDance(Player target)
        {
            int chargeTime = 45;
            int formationTime = 30;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.95f;
                float progress = Timer / (float)chargeTime;
                
                // Show constellation points
                if (Timer % 5 == 0)
                {
                    int points = 6 + difficultyTier * 2;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points + Timer * 0.02f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * 200f;
                        CustomParticles.GenericFlare(pos, StarWhite * progress, 0.3f, 10);
                    }
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                }
            }
            else if (SubPhase == 1) // Spawn formation
            {
                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int points = 6 + difficultyTier * 2;
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points;
                        Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 200f;
                        Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * MediumProjectileSpeed * 0.8f;
                        BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel, 70, Violet, 3f);
                        
                        CustomParticles.GenericFlare(spawnPos, StarWhite, 0.6f, 15);
                    }
                    
                    // Connecting lines effect
                    for (int i = 0; i < 8; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Violet, 0.3f + i * 0.08f, 18);
                    }
                }
                
                if (Timer >= formationTime)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Moonbeam Cascade - Vertical rays of light
        /// </summary>
        private void Attack_MoonbeamCascade(Player target)
        {
            int waves = 3 + difficultyTier;
            int waveDelay = (int)((35 - difficultyTier * 5) * GetAggressionRateMult());
            
            if (SubPhase < waves)
            {
                // Warning phase for each wave
                if (Timer < 20)
                {
                    if (Timer % 4 == 0)
                    {
                        float xOffset = (SubPhase - waves / 2f) * 120f;
                        Vector2 warningPos = new Vector2(target.Center.X + xOffset, target.Center.Y - 400f);
                        BossVFXOptimizer.WarningLine(warningPos, Vector2.UnitY, 800f, 10, WarningType.Danger);
                    }
                }
                // Fire phase
                else if (Timer == 20 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float xOffset = (SubPhase - waves / 2f) * 120f;
                    Vector2 spawnPos = new Vector2(target.Center.X + xOffset, target.Center.Y - 500f);
                    Vector2 vel = Vector2.UnitY * FastProjectileSpeed;
                    
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 80, StarWhite, 8f);
                    
                    CustomParticles.GenericFlare(spawnPos, StarWhite, 0.7f, 15);
                    SoundEngine.PlaySound(SoundID.Item12, spawnPos);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 30)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Nocturnal Serenade - Homing musical note projectiles
        /// </summary>
        private void Attack_NocturnalSerenade(Player target)
        {
            int chargeTime = 40;
            int noteCount = 8 + difficultyTier * 4;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.95f;
                float progress = Timer / (float)chargeTime;
                
                // Musical note particles converging
                if (Timer % 3 == 0)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * (150f - progress * 100f);
                    CustomParticles.GenericFlare(pos, Violet, 0.35f, 12);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f }, NPC.Center);
                }
            }
            else if (SubPhase == 1) // Fire notes in bursts
            {
                if (Timer % 8 == 0 && Timer <= noteCount * 8 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = angle.ToRotationVector2() * HomingSpeed;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 65, Gold, 0.025f);
                    
                    CustomParticles.GenericFlare(NPC.Center, Gold, 0.4f, 10);
                }
                
                if (Timer >= noteCount * 8 + 30)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Crescent Slash - Arc-shaped projectiles
        /// </summary>
        private void Attack_CrescentSlash(Player target)
        {
            int slashes = 2 + difficultyTier;
            int slashDelay = 40;
            
            if (SubPhase < slashes)
            {
                if (Timer < 25)
                {
                    float progress = Timer / 25f;
                    float slashAngle = (target.Center - NPC.Center).ToRotation();
                    
                    // Arc warning
                    for (int i = -3; i <= 3; i++)
                    {
                        float angle = slashAngle + i * 0.15f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (80f + progress * 60f);
                        CustomParticles.GenericFlare(pos, DeepPurple * progress, 0.2f, 5);
                    }
                }
                else if (Timer == 25 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = (target.Center - NPC.Center).ToRotation();
                    
                    for (int i = -2; i <= 2; i++)
                    {
                        float angle = baseAngle + i * 0.2f;
                        Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed;
                        BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 75, Violet, 2.5f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Violet, 0.7f, 15);
                    SoundEngine.PlaySound(SoundID.Item71, NPC.Center);
                }
                
                if (Timer >= slashDelay)
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
        
        #endregion

        #region Phase 2 Attacks
        
        /// <summary>
        /// Cosmic Tempest - Rapid chaotic star projectiles
        /// </summary>
        private void Attack_CosmicTempest(Player target)
        {
            int duration = (int)((90 + difficultyTier * 20) * GetAggressionRateMult());
            int fireInterval = Math.Max(3, 8 - difficultyTier * 2);
            
            // Aggressive pursuit during attack
            Vector2 toTarget = target.Center - NPC.Center;
            if (toTarget.Length() > 200f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 0.7f * Phase2SpeedMult, 0.06f);
            }
            
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // Chaotic spread
                for (int i = 0; i < 2 + difficultyTier; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = MediumProjectileSpeed + Main.rand.NextFloat(-3f, 5f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    
                    Color color = Main.rand.NextBool() ? Gold : Violet;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, (int)(70 * Phase2DamageMult), color, 0.015f);
                }
                
                CustomParticles.GenericFlare(NPC.Center, Gold, 0.35f, 8);
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        /// <summary>
        /// Nebula Burst - Expanding star explosions
        /// </summary>
        private void Attack_NebulaBurst(Player target)
        {
            int bursts = 3 + difficultyTier;
            int burstDelay = 45;
            
            if (SubPhase < bursts)
            {
                NPC.velocity *= 0.95f;
                
                if (Timer < 30)
                {
                    float progress = Timer / 30f;
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, Gold, 8);
                }
                else if (Timer == 30 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Expanding ring of projectiles
                    int count = 12 + difficultyTier * 4;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count;
                        Vector2 vel = angle.ToRotationVector2() * FastProjectileSpeed * 0.9f;
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(80 * Phase2DamageMult), StarWhite, 6f);
                    }
                    
                    MagnumScreenEffects.AddScreenShake(8f);
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.2f, 20);
                    for (int i = 0; i < 10; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(Gold, Violet, i / 10f), 0.2f + i * 0.1f, 15 + i);
                    }
                    SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                }
                
                if (Timer >= burstDelay)
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
        
        /// <summary>
        /// Galactic Judgment - Safe-arc radial burst (signature attack)
        /// </summary>
        private void Attack_GalacticJudgment(Player target)
        {
            int chargeTime = (int)((70 - difficultyTier * 10) * GetAggressionRateMult());
            int waves = 3 + difficultyTier;
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.96f;
                float progress = Timer / (float)chargeTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f }, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("Witness galactic judgment!", Gold);
                }
                
                // Converging cosmic energy
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 180f, progress, Gold, 10);
                
                // Safe zone indicator around player
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 100f, 10);
                }
                
                // Screen shake buildup
                if (Timer > chargeTime * 0.7f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 4f);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waves) // Fire waves with safe arc
            {
                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    MagnumScreenEffects.AddScreenShake(12f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.3f }, NPC.Center);
                    
                    int projectileCount = 28 + difficultyTier * 6;
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    float safeArc = MathHelper.ToRadians(28f - difficultyTier * 3f);
                    
                    for (int i = 0; i < projectileCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / projectileCount;
                        float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                        
                        if (Math.Abs(angleDiff) < safeArc) continue;
                        
                        float speed = FastProjectileSpeed + SubPhase * 2f;
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(85 * Phase2DamageMult), Gold, 8f);
                    }
                    
                    // Cascading halos
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.5f, 25);
                    for (int i = 0; i < 12; i++)
                    {
                        Color color = Color.Lerp(DeepPurple, Gold, i / 12f);
                        CustomParticles.HaloRing(NPC.Center, color, 0.3f + i * 0.12f, 18 + i * 2);
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
                if (Timer >= 40)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Starfall Apocalypse - Raining stars from above
        /// </summary>
        private void Attack_StarfallApocalypse(Player target)
        {
            int duration = (int)((100 + difficultyTier * 30) * GetAggressionRateMult());
            int fireInterval = Math.Max(4, 10 - difficultyTier * 2);
            
            // Hover above
            Vector2 hoverPos = target.Center + new Vector2(0, -400f);
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 50f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed * 0.8f, 0.06f);
            }
            
            // Warning flares
            if (Timer % 15 == 0)
            {
                for (int i = 0; i < 3 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-350f, 350f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -550f);
                    CustomParticles.GenericFlare(warningPos, Gold * 0.4f, 0.3f, 12);
                }
            }
            
            // Fire stars
            if (Timer % fireInterval == 0 && Timer > 20 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 2 + difficultyTier;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -600f);
                    float ySpeed = FastProjectileSpeed * 0.9f + difficultyTier * 2f;
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), ySpeed);
                    
                    Color color = Main.rand.NextBool(3) ? StarWhite : Gold;
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.7f, (int)(75 * Phase2DamageMult), color, 12f);
                    
                    CustomParticles.GenericFlare(spawnPos, color, 0.4f, 10);
                }
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        /// <summary>
        /// Eternal Nightmare - Multi-phase ultimate attack
        /// </summary>
        private void Attack_EternalNightmare(Player target)
        {
            // Phase 0: Charge announcement
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.95f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = 0.2f, Volume = 1.5f }, NPC.Center);
                    if (Main.netMode != NetmodeID.Server)
                        Main.NewText("EMBRACE ETERNAL NIGHT!", Gold);
                }
                
                float progress = Timer / 60f;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 200f, progress, DeepPurple, 12);
                
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Timer * 0.03f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (200f - progress * 150f);
                        CustomParticles.GenericFlare(pos, Violet, 0.4f + progress * 0.3f, 12);
                    }
                }
                
                if (Timer >= 60)
                {
                    Timer = 0;
                    SubPhase = 1;
                    MagnumScreenEffects.AddScreenShake(15f);
                }
            }
            // Phase 1: Spiral burst
            else if (SubPhase == 1)
            {
                if (Timer % 4 == 0 && Timer <= 60 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float spiralAngle = Timer * 0.15f;
                    for (int arm = 0; arm < 4; arm++)
                    {
                        float angle = spiralAngle + MathHelper.PiOver2 * arm;
                        Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed;
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, (int)(70 * Phase2DamageMult), Violet, 0.01f);
                    }
                    CustomParticles.GenericFlare(NPC.Center, Violet, 0.4f, 8);
                }
                
                if (Timer >= 70)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            // Phase 2: Radial burst
            else if (SubPhase == 2)
            {
                if (Timer == 1 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int count = 24;
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count;
                        Vector2 vel = angle.ToRotationVector2() * FastProjectileSpeed;
                        BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(80 * Phase2DamageMult), Gold, 6f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.5f, 25);
                    for (int i = 0; i < 10; i++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(DeepPurple, Gold, i / 10f), 0.3f + i * 0.1f, 18);
                    }
                    MagnumScreenEffects.AddScreenShake(12f);
                }
                
                if (Timer >= 50)
                {
                    Timer = 0;
                    SubPhase = 3;
                }
            }
            // Phase 3: Homing finale
            else if (SubPhase == 3)
            {
                if (Timer % 10 == 0 && Timer <= 50 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = angle.ToRotationVector2() * HomingSpeed * 1.2f;
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, (int)(75 * Phase2DamageMult), StarWhite, 0.03f);
                    }
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 0.5f, 12);
                }
                
                if (Timer >= 80)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Celestial Charge - High-speed dash attacks
        /// </summary>
        private void Attack_CelestialCharge(Player target)
        {
            int dashes = 3 + difficultyTier;
            int dashTime = 25;
            int pauseTime = 20;
            
            if (SubPhase < dashes * 2) // Alternating telegraph and dash
            {
                bool isDashing = SubPhase % 2 == 1;
                
                if (!isDashing) // Telegraph
                {
                    if (Timer == 1)
                    {
                        dashDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                        dashTarget = target.Center + dashDirection * 200f;
                    }
                    
                    NPC.velocity *= 0.9f;
                    
                    // Warning line
                    BossVFXOptimizer.WarningLine(NPC.Center, dashDirection, 500f, 12, WarningType.Danger);
                    
                    float progress = Timer / (float)pauseTime;
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 50f, progress, Gold, 4);
                    
                    if (Timer >= pauseTime)
                    {
                        Timer = 0;
                        SubPhase++;
                        SoundEngine.PlaySound(SoundID.Item122, NPC.Center);
                    }
                }
                else // Dash
                {
                    NPC.velocity = dashDirection * BaseSpeed * Phase2SpeedMult * 2.5f;
                    
                    // Trail particles
                    if (Timer % 2 == 0)
                    {
                        CustomParticles.GenericFlare(NPC.Center, Gold, 0.5f, 12);
                        CustomParticles.GenericFlare(NPC.Center + Main.rand.NextVector2Circular(30f, 30f), Violet, 0.3f, 10);
                    }
                    
                    if (Timer >= dashTime)
                    {
                        Timer = 0;
                        SubPhase++;
                        dashCount++;
                    }
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 30)
                {
                    dashCount = 0;
                    EndAttack();
                }
            }
        }
        
        #endregion

        #region Death Animation
        
        private void UpdateTrueDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            // Building intensity
            if (deathTimer < 120)
            {
                float progress = deathTimer / 120f;
                
                if (deathTimer % 5 == 0)
                {
                    int count = (int)(6 + progress * 10);
                    for (int i = 0; i < count; i++)
                    {
                        float angle = MathHelper.TwoPi * i / count + deathTimer * 0.05f;
                        Vector2 offset = angle.ToRotationVector2() * (150f - progress * 100f);
                        Color color = GetNachtmusikGradient((float)i / count);
                        CustomParticles.GenericFlare(NPC.Center + offset, color, 0.4f + progress * 0.4f, 15);
                    }
                }
                
                if (deathTimer % 15 == 0)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 8f);
                }
            }
            // Final explosion
            else if (deathTimer == 120)
            {
                MagnumScreenEffects.AddScreenShake(25f);
                SoundEngine.PlaySound(SoundID.Item122 with { Volume = 1.5f }, NPC.Center);
                
                // Massive star explosion
                CustomParticles.GenericFlare(NPC.Center, StarWhite, 3.0f, 40);
                
                for (int ring = 0; ring < 20; ring++)
                {
                    float scale = 0.3f + ring * 0.15f;
                    Color color = GetNachtmusikGradient(ring / 20f);
                    CustomParticles.HaloRing(NPC.Center, color, scale, 25 + ring * 2);
                }
                
                // Radial star burst
                for (int i = 0; i < 40; i++)
                {
                    float angle = MathHelper.TwoPi * i / 40f;
                    Vector2 vel = angle.ToRotationVector2() * Main.rand.NextFloat(8f, 15f);
                    CustomParticles.GenericFlare(NPC.Center + vel * 3f, Gold, 0.6f, 30);
                }
                
                if (Main.netMode != NetmodeID.Server)
                {
                    Main.NewText("The Queen of Radiance returns to the stars...", Gold);
                }
            }
            // Fade out
            else if (deathTimer > 120 && deathTimer < 180)
            {
                NPC.alpha = Math.Min(255, NPC.alpha + 5);
                
                if (deathTimer % 8 == 0)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.GenericFlare(pos, StarWhite * ((180f - deathTimer) / 60f), 0.4f, 15);
                }
            }
            else if (deathTimer >= 180)
            {
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
            }
        }
        
        #endregion

        #region Visual Effects
        
        private void SpawnAmbientParticles()
        {
            // Orbiting stars
            if (Main.GameUpdateCount % 8 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = starOrbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 80f + (float)Math.Sin(Main.GameUpdateCount * 0.03f + i) * 20f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = isPhase2 ? Gold : Violet;
                    CustomParticles.GenericFlare(pos, color * 0.6f, 0.25f, 12);
                }
            }
            
            // Crescent moon particles
            if (Main.GameUpdateCount % 12 == 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    float angle = crescentOrbitAngle + MathHelper.Pi * i;
                    float radius = 60f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    CustomParticles.GenericFlare(pos, StarWhite * 0.5f, 0.2f, 10);
                }
            }
            
            // Ambient star dust
            if (Main.rand.NextBool(isPhase2 ? 6 : 10))
            {
                Vector2 dustPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width, NPC.height);
                Vector2 dustVel = new Vector2(0, -1f) + Main.rand.NextVector2Circular(0.5f, 0.3f);
                Dust dust = Dust.NewDustDirect(dustPos, 1, 1, DustID.PurpleTorch, dustVel.X, dustVel.Y, 150, default, 1.1f);
                dust.noGravity = true;
            }
            
            // Golden shimmer in Phase 2
            if (isPhase2 && Main.rand.NextBool(8))
            {
                Vector2 shimmerPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.8f, NPC.height * 0.8f);
                Dust gold = Dust.NewDustDirect(shimmerPos, 1, 1, DustID.GoldFlame, 0, -0.5f, 0, default, 0.9f);
                gold.noGravity = true;
            }
        }
        
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Phase 2 uses different texture
            Texture2D texture;
            if (isPhase2)
            {
                texture = ModContent.Request<Texture2D>(Phase2Texture).Value;
            }
            else
            {
                texture = TextureAssets.Npc[Type].Value;
            }
            
            Vector2 drawPos = NPC.Center - screenPos;
            Vector2 origin = texture.Size() / 2f;
            
            // Draw afterimages for dashing
            if (NPC.velocity.Length() > BaseSpeed)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    if (NPC.oldPos[i] == Vector2.Zero) continue;
                    
                    float progress = (float)i / NPC.oldPos.Length;
                    Color trailColor = GetNachtmusikGradient(progress) * (1f - progress) * 0.4f;
                    Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                    
                    spriteBatch.Draw(texture, trailPos, null, trailColor, NPC.rotation,
                        origin, NPC.scale * (1f - progress * 0.3f), 
                        NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                }
            }
            
            // Glow effect
            Color glowColor = (isPhase2 ? Gold : Violet) * 0.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector2 offset = (MathHelper.PiOver2 * i).ToRotationVector2() * 4f;
                spriteBatch.Draw(texture, drawPos + offset, null, glowColor, NPC.rotation,
                    origin, NPC.scale, 
                    NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            }
            
            // Main sprite
            spriteBatch.Draw(texture, drawPos, null, drawColor, NPC.rotation,
                origin, NPC.scale, 
                NPC.spriteDirection == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
            
            return false;
        }
        
        #endregion

        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Boss bag (Expert/Master)
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NachtmusikTreasureBag>()));
            
            // Normal mode drops
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            
            // Resonant Energy (15-25)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantEnergy>(), 1, 15, 25));
            
            // Resonant Core (8-12)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantCore>(), 1, 8, 12));
            
            // Harmonic Core (guaranteed)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 1, 1, 1));
            
            // Remnants (30-50)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 1, 30, 50));
            
            npcLoot.Add(notExpert);
        }
        
        public override void OnKill()
        {
            // Mark boss as defeated
            MoonlightSonataSystem.DownedNachtmusik = true;
            
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        
        #endregion
    }
}
