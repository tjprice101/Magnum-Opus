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
using MagnumOpus.Content.Nachtmusik.ResonanceEnergies;
using MagnumOpus.Content.Nachtmusik.HarmonicCores;
using MagnumOpus.Content.Nachtmusik.ResonantWeapons;
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
        // POST-FATE ULTIMATE BOSS - BEYOND FATE TIER - INSANE DIFFICULTY
        private const float BaseSpeed = 45f; // BLAZING FAST - 2.5x base movement
        private const int BaseDamage = 350;  // DEVASTATING damage output
        private const float EnrageDistance = 700f; // Very aggressive enrage threshold
        private const float TeleportDistance = 1600f;
        private const int AttackWindowFrames = 15; // EXTREMELY short attack windows - relentless
        
        // Projectile speeds - INSANELY FAST
        private const float FastProjectileSpeed = 38f; // Lightning fast projectiles
        private const float MediumProjectileSpeed = 28f; // Standard attacks still deadly fast
        private const float HomingSpeed = 18f; // Aggressive homing
        
        // Phase 2 stats multipliers - ABSOLUTE NIGHTMARE
        private const float Phase2DamageMult = 1.9f; // Nearly double damage in Phase 2
        private const float Phase2SpeedMult = 2.2f; // Over double speed in Phase 2 - CRAZY FAST
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
            AuroraVeil,           // NEW: Protective shield that reflects projectiles back
            
            // Phase 2 - Celestial Fury Attacks  
            CosmicTempest,        // Rapid chaotic stars
            NebulaBurst,          // Expanding star explosions
            GalacticJudgment,     // Safe-arc radial burst
            StarfallApocalypse,   // Raining star barrage
            EternalNightmare,     // Multi-phase ultimate
            CelestialCharge,      // High-speed dashes
            SupernovaCollapse,    // NEW: Massive implosion then explosion
            TwilightReversal,     // NEW: Time-reversal attack that rewinds projectiles
            QuantumBlink          // NEW: Rapid multi-teleport assault
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
            // Hitbox = 80% of sprite size (270x270)
            NPC.width = 216;
            NPC.height = 216;
            NPC.damage = BaseDamage;
            NPC.defense = 180; // POST-FATE ULTIMATE defense - 29% above Fate (140)
            NPC.lifeMax = 4000000; // 4 million HP per phase (8 million total - must defeat twice)
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

        // Sky effect tracking
        private bool skyEffectActivated = false;

        public override void AI()
        {
            // Activate cosmic celestial sky effect when boss spawns
            if (!skyEffectActivated && !Main.dedServ)
            {
                if (SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] != null)
                {
                    SkyManager.Instance.Activate("MagnumOpus:NachtmusikCelestialSky", NPC.Center);
                }
                skyEffectActivated = true;
            }
            
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
                
                // ACTIVATE PHASE 2 ENHANCED CELESTIAL SKY - Fading color effects!
                if (SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] is NachtmusikCelestialSky celestialSky)
                {
                    celestialSky.ActivatePhase2();
                    celestialSky.TriggerFlash(1.5f, Gold); // Golden flash on awakening
                }
            }
            
            // Increase stats for Phase 2 - DEVASTATING
            NPC.damage = (int)(BaseDamage * Phase2DamageMult);
            NPC.defense = 220; // Massively increased defense in Phase 2
            
            if (Main.netMode != NetmodeID.Server)
            {
                Main.NewText("THE STARS REFUSE TO DIM!", Gold);
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
                    
                    // Dramatic sky flash on Phase 2 dramatic reveal
                    if (!Main.dedServ && SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] is NachtmusikCelestialSky celestialSky)
                    {
                        celestialSky.TriggerFlash(2f, StarWhite);
                    }
                }
                
                // Expanding gold starbursts
                if (Timer % 5 == 0)
                {
                    var goldBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, Gold, 0.35f + progress * 0.4f, 25);
                    MagnumParticleHandler.SpawnParticle(goldBurst);
                    var violetBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, Violet, 0.25f + progress * 0.35f, 22, 1);
                    MagnumParticleHandler.SpawnParticle(violetBurst);
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
                    // Ultimate reveal flash with star bursts and shattered starlight
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 2.0f, 30);
                    
                    // Star burst explosion at center
                    NachtmusikCosmicVFX.SpawnStarBurstImpact(NPC.Center, 1.5f, 6);
                    
                    // Massive shattered starlight explosion
                    NachtmusikCosmicVFX.SpawnShatteredStarlightBurst(NPC.Center, 30, 12f, 1.0f, true);
                    
                    for (int i = 0; i < 16; i++)
                    {
                        float scale = 0.25f + i * 0.08f;
                        Color color = Color.Lerp(Gold, Violet, i / 16f);
                        var revealBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, color, scale, 20 + i * 2, i % 2);
                        MagnumParticleHandler.SpawnParticle(revealBurst);
                        
                        // Shattered starlight accents
                        if (i % 3 == 0)
                        {
                            float angle = MathHelper.TwoPi * i / 16f;
                            Vector2 fragVel = angle.ToRotationVector2() * 8f;
                            var fragment = new ShatteredStarlightParticle(NPC.Center, fragVel, color, 0.3f, 25, true, 0.08f);
                            MagnumParticleHandler.SpawnParticle(fragment);
                        }
                    }
                    
                    // Glyphs and music notes
                    NachtmusikCosmicVFX.SpawnGlyphBurst(NPC.Center, 12, 8f, 0.5f);
                    NachtmusikCosmicVFX.SpawnMusicNoteBurst(NPC.Center, 16, 8f);
                    
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
                    VFXIntegration.OnBossEnrage("Nachtmusik", NPC.Center);
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
            var teleportBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, Violet, 0.45f, 18);
            MagnumParticleHandler.SpawnParticle(teleportBurst);
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
                VFXIntegration.OnBossSpawn("Nachtmusik", NPC.Center);
                State = BossPhase.Phase1_Idle;
                Timer = 0;
                attackCooldown = AttackWindowFrames;
                
                CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.2f, 25);
                for (int i = 0; i < 10; i++)
                {
                    Color burstColor = Color.Lerp(DeepPurple, Gold, i / 10f);
                    var spawnBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, burstColor, 0.3f + i * 0.08f, 18 + i * 2, i % 2);
                    MagnumParticleHandler.SpawnParticle(spawnBurst);
                }
            }
        }
        
        private void AI_Phase1_Idle(Player target)
        {
            // GRACEFUL CELESTIAL DANCE - Figure-8 flowing movement
            float hoverHeight = -280f;
            
            // Figure-8 pattern for elegant flow
            float figure8X = (float)Math.Sin(Timer * 0.012f) * 120f;
            float figure8Y = (float)Math.Sin(Timer * 0.024f) * 40f; // Double frequency for figure-8
            
            // Add gentle vertical bob
            float breathY = (float)Math.Sin(Timer * 0.03f) * 15f;
            
            Vector2 hoverPos = target.Center + new Vector2(figure8X, hoverHeight + figure8Y + breathY);
            Vector2 toHover = hoverPos - NPC.Center;
            
            // Smooth, graceful interpolation
            if (toHover.Length() > 15f)
            {
                toHover.Normalize();
                float speed = BaseSpeed * 0.35f; // Slower, more elegant
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * speed, 0.025f); // Smoother lerp
            }
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.03f);
            }
            
            // Gentle rotation following movement direction
            if (NPC.velocity.Length() > 1f)
            {
                float targetRotation = NPC.velocity.X * 0.015f; // Subtle tilt
                NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.02f);
            }
            else
            {
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.02f);
            }
            
            // === PHASE 10 MUSICAL VFX: Beat Synced Rhythm - Celestial Waltz ===
            if (Timer % 40 == 0)
            {
                Phase10Integration.Universal.BeatSyncedRhythm(NPC.Center, Violet, 90f, Timer);
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
                pool.Add(AttackPattern.AuroraVeil);
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
                case AttackPattern.AuroraVeil:
                    Attack_AuroraVeil(target);
                    break;
            }
        }
        
        #endregion

        #region Phase 2 AI
        
        private void AI_Phase2_Idle(Player target)
        {
            // CELESTIAL FURY DANCE - More intense but still flowing
            float hoverHeight = -220f;
            
            // Spiraling pattern - more aggressive figure-8 with rotation
            float spiralAngle = Timer * 0.02f;
            float spiralRadius = 80f + (float)Math.Sin(Timer * 0.015f) * 40f;
            float spiralX = (float)Math.Cos(spiralAngle) * spiralRadius;
            float spiralY = (float)Math.Sin(spiralAngle * 2f) * 35f; // Creates infinity pattern
            
            // Pulsing approach - gets closer during certain beats
            float pulseDistance = (float)Math.Sin(Timer * 0.008f) * 60f;
            hoverHeight += pulseDistance;
            
            Vector2 hoverPos = target.Center + new Vector2(spiralX, hoverHeight + spiralY);
            Vector2 toHover = hoverPos - NPC.Center;
            
            // More responsive but still smooth
            if (toHover.Length() > 20f)
            {
                toHover.Normalize();
                float speed = BaseSpeed * Phase2SpeedMult * 0.4f; // Controlled intensity
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * speed, 0.035f);
            }
            else
            {
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.04f);
            }
            
            // Dynamic rotation - follows movement with energy
            if (NPC.velocity.Length() > 2f)
            {
                float targetRotation = NPC.velocity.X * 0.02f; // More pronounced tilt in Phase 2
                NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.03f);
            }
            else
            {
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.025f);
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
                pool.Add(AttackPattern.QuantumBlink);
                pool.Add(AttackPattern.TwilightReversal);
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.EternalNightmare);
                pool.Add(AttackPattern.SupernovaCollapse);
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
                case AttackPattern.SupernovaCollapse:
                    Attack_SupernovaCollapse(target);
                    break;
                case AttackPattern.TwilightReversal:
                    Attack_TwilightReversal(target);
                    break;
                case AttackPattern.QuantumBlink:
                    Attack_QuantumBlink(target);
                    break;
            }
        }
        
        #endregion

        #region Shared AI
        
        private void AI_Reposition(Player target)
        {
            // GRACEFUL ARC REPOSITION - Curves elegantly to new position
            if (Timer == 1)
            {
                // Pick a position in an arc around the player
                float angle = Main.rand.NextFloat(-MathHelper.PiOver4, MathHelper.PiOver4) - MathHelper.PiOver2;
                float distance = Main.rand.NextFloat(300f, 400f);
                dashTarget = target.Center + angle.ToRotationVector2() * distance;
            }
            
            Vector2 toIdeal = dashTarget - NPC.Center;
            float distanceToTarget = toIdeal.Length();
            
            // Calculate curved path - arc toward target
            if (distanceToTarget > 40f)
            {
                // Speed curve using smooth easing - accelerate then decelerate
                float progress = Math.Min(Timer / 60f, 1f);
                float speedCurve = BossAIUtilities.Easing.EaseOutQuad(progress) * BossAIUtilities.Easing.EaseInQuad(1f - progress) * 4f;
                float baseSpeed = BaseSpeed * GetPhaseSpeedMult() * 0.6f;
                float speed = baseSpeed * (0.3f + speedCurve * 0.7f);
                
                toIdeal.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toIdeal * speed, 0.04f); // Smooth transition
                
                // Rotation follows movement gracefully
                float targetRotation = NPC.velocity.X * 0.012f;
                NPC.rotation = MathHelper.Lerp(NPC.rotation, targetRotation, 0.02f);
                
                // Recovery shimmer during reposition to show vulnerability
                if (Timer % 5 == 0)
                {
                    float shimmerProgress = Timer / 70f;
                    BossVFXOptimizer.RecoveryShimmer(NPC.Center, isPhase2 ? Gold : Violet, 60f, shimmerProgress);
                }
                
                // Flowing trail during reposition
                if (Timer % 6 == 0 && NPC.velocity.Length() > 5f)
                {
                    CustomParticles.GenericFlare(NPC.Center - NPC.velocity.SafeNormalize(Vector2.Zero) * 20f, 
                        isPhase2 ? Gold : Violet, 0.25f, 12);
                }
            }
            else
            {
                // Graceful arrival with smooth easing
                float arrivalProgress = Math.Max(0, 1f - distanceToTarget / 40f);
                float arrivalEase = BossAIUtilities.Easing.EaseOutCubic(arrivalProgress);
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.05f + arrivalEase * 0.1f);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.03f);
                
                if (NPC.velocity.Length() < 2f)
                {
                    // Ready to attack cue
                    BossVFXOptimizer.ReadyToAttackCue(NPC.Center, isPhase2 ? Gold : Violet, 0.5f);
                    
                    State = isPhase2 ? BossPhase.Phase2_Idle : BossPhase.Phase1_Idle;
                    Timer = 0;
                    NPC.rotation = 0f;
                    attackCooldown = (int)(AttackWindowFrames * GetAggressionRateMult());
                }
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
            // Nachtmusik theme attack end cue - celestial gold exhale
            BossVFXOptimizer.AttackEndCue(NPC.Center, Gold, Violet, 0.7f);
            
            State = isPhase2 ? BossPhase.Phase2_Reposition : BossPhase.Phase1_Reposition;
            Timer = 0;
            SubPhase = 0;
        }
        
        #endregion

        #region Phase 1 Attacks
        
        /// <summary>
        /// Starlight Waltz - Sweeping arc of star projectiles - AGGRESSIVE VERSION
        /// </summary>
        private void Attack_StarlightWaltz(Player target)
        {
            int chargeTime = (int)((35 - difficultyTier * 8) * GetAggressionRateMult()); // Faster charge
            int fireTime = 45; // Shorter but more intense
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.96f;
                float progress = Timer / (float)chargeTime;
                
                // Warning arc indicator - LARGER
                if (Timer % 3 == 0)
                {
                    float arcStart = (target.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;
                    for (int i = 0; i < 12; i++) // More warning particles
                    {
                        float angle = arcStart + MathHelper.Pi * i / 12f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (150f + progress * 80f);
                        CustomParticles.GenericFlare(pos, Violet * 0.6f, 0.3f, 8);
                    }
                }
                
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 100f, progress, Violet, 8);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1) // Fire - MORE PROJECTILES
            {
                if (Timer % 3 == 0 && Main.netMode != NetmodeID.MultiplayerClient) // Fire twice as fast
                {
                    float baseAngle = (target.Center - NPC.Center).ToRotation() - MathHelper.PiOver2;
                    float sweepAngle = baseAngle + MathHelper.Pi * ((float)Timer / fireTime);
                    
                    // Fire 2 projectiles per burst (inner and outer)
                    for (int layer = 0; layer < 2; layer++)
                    {
                        Vector2 vel = sweepAngle.ToRotationVector2() * (MediumProjectileSpeed + layer * 4f);
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, layer == 0 ? Gold : Violet, 0.02f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Gold, 0.5f, 10);
                }
                
                if (Timer >= fireTime)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Constellation Dance - Star pattern that forms geometric shapes - AGGRESSIVE VERSION
        /// </summary>
        private void Attack_ConstellationDance(Player target)
        {
            int chargeTime = 30; // Faster charge
            int formationTime = 20;
            
            if (SubPhase == 0) // Telegraph
            {
                NPC.velocity *= 0.95f;
                float progress = Timer / (float)chargeTime;
                
                // Show constellation points - MORE POINTS
                if (Timer % 3 == 0)
                {
                    int points = 10 + difficultyTier * 4; // More points
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points + Timer * 0.03f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * 180f;
                        CustomParticles.GenericFlare(pos, StarWhite * progress, 0.35f, 10);
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
                    int points = 10 + difficultyTier * 4; // More points
                    for (int i = 0; i < points; i++)
                    {
                        float angle = MathHelper.TwoPi * i / points;
                        Vector2 spawnPos = NPC.Center + angle.ToRotationVector2() * 180f;
                        Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * MediumProjectileSpeed;
                        
                        // Alternate between homing orbs and wave projectiles
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 70, Gold, 0.025f);
                        else
                            BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel, 70, Violet, 3.5f);
                        
                        CustomParticles.GenericFlare(spawnPos, StarWhite, 0.65f, 15);
                    }
                    
                    // Connecting constellation effect - BIGGER
                    for (int i = 0; i < 12; i++)
                    {
                        float constAngle = MathHelper.TwoPi * i / 12f;
                        Vector2 constPos = NPC.Center + constAngle.ToRotationVector2() * (30f + i * 10f);
                        var constBurst = new StarBurstParticle(constPos, Vector2.Zero, Violet, 0.25f + i * 0.05f, 18, i % 2);
                        MagnumParticleHandler.SpawnParticle(constBurst);
                    }
                }
                
                if (Timer >= formationTime)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Moonbeam Cascade - Vertical rays of light - AGGRESSIVE VERSION
        /// </summary>
        private void Attack_MoonbeamCascade(Player target)
        {
            int waves = 5 + difficultyTier * 2; // More waves
            int waveDelay = (int)((22 - difficultyTier * 4) * GetAggressionRateMult()); // Faster waves
            
            if (SubPhase < waves)
            {
                // Warning phase for each wave
                if (Timer < 12) // Shorter warning
                {
                    if (Timer % 2 == 0)
                    {
                        float xOffset = (SubPhase - waves / 2f) * 100f; // Tighter spacing
                        Vector2 warningPos = new Vector2(target.Center.X + xOffset, target.Center.Y - 400f);
                        BossVFXOptimizer.WarningLine(warningPos, Vector2.UnitY, 800f, 10, WarningType.Danger);
                        
                        // Additional warning beam on sides
                        Vector2 warningPos2 = new Vector2(target.Center.X + xOffset + 50f, target.Center.Y - 400f);
                        BossVFXOptimizer.WarningLine(warningPos2, Vector2.UnitY, 800f, 8, WarningType.Caution);
                    }
                }
                // Fire phase - MULTIPLE BEAMS PER WAVE
                else if (Timer == 12 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float xOffset = (SubPhase - waves / 2f) * 100f;
                    
                    // Main beam
                    Vector2 spawnPos = new Vector2(target.Center.X + xOffset, target.Center.Y - 500f);
                    Vector2 vel = Vector2.UnitY * FastProjectileSpeed;
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 80, StarWhite, 12f);
                    
                    // Side beams
                    Vector2 spawnPos2 = new Vector2(target.Center.X + xOffset + 50f, target.Center.Y - 500f);
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos2, vel * 0.9f, 75, Gold, 10f);
                    
                    CustomParticles.GenericFlare(spawnPos, StarWhite, 0.8f, 15);
                    CustomParticles.GenericFlare(spawnPos2, Gold, 0.6f, 12);
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
                if (Timer >= 20)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Nocturnal Serenade - Homing musical note projectiles - AGGRESSIVE VERSION
        /// </summary>
        private void Attack_NocturnalSerenade(Player target)
        {
            int chargeTime = 25; // Faster charge
            int noteCount = 16 + difficultyTier * 8; // Many more notes
            
            if (SubPhase == 0) // Charge
            {
                NPC.velocity *= 0.95f;
                float progress = Timer / (float)chargeTime;
                
                // Musical note particles converging - MORE
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (180f - progress * 130f);
                        CustomParticles.GenericFlare(pos, i == 0 ? Violet : Gold, 0.4f, 12);
                    }
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item4 with { Pitch = 0.3f }, NPC.Center);
                }
            }
            else if (SubPhase == 1) // Fire notes in RAPID bursts
            {
                if (Timer % 4 == 0 && Timer <= noteCount * 4 && Main.netMode != NetmodeID.MultiplayerClient) // Faster burst
                {
                    // Fire 2-3 notes per burst
                    int burstCount = 2 + difficultyTier;
                    for (int i = 0; i < burstCount; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        Vector2 vel = angle.ToRotationVector2() * HomingSpeed * 1.3f; // Faster base speed
                        BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 65, i == 0 ? Gold : Violet, 0.04f); // Stronger homing
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Gold, 0.5f, 10);
                }
                
                if (Timer >= noteCount * 4 + 20)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Crescent Slash - Arc-shaped projectiles - AGGRESSIVE VERSION
        /// </summary>
        private void Attack_CrescentSlash(Player target)
        {
            int slashes = 4 + difficultyTier * 2; // Many more slashes
            int slashDelay = 25; // Faster slashes
            
            if (SubPhase < slashes)
            {
                if (Timer < 15) // Shorter telegraph
                {
                    float progress = Timer / 15f;
                    float slashAngle = (target.Center - NPC.Center).ToRotation();
                    
                    // Arc warning - WIDER
                    for (int i = -5; i <= 5; i++)
                    {
                        float angle = slashAngle + i * 0.12f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * (80f + progress * 80f);
                        CustomParticles.GenericFlare(pos, DeepPurple * progress, 0.25f, 5);
                    }
                }
                else if (Timer == 15 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float baseAngle = (target.Center - NPC.Center).ToRotation();
                    
                    // MORE PROJECTILES PER SLASH
                    for (int i = -4; i <= 4; i++)
                    {
                        float angle = baseAngle + i * 0.15f;
                        Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed * 1.2f;
                        BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 75, i % 2 == 0 ? Violet : Gold, 3f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Violet, 0.8f, 15);
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
        /// Cosmic Tempest - Rapid chaotic star projectiles - MAXIMUM AGGRESSION
        /// </summary>
        private void Attack_CosmicTempest(Player target)
        {
            int duration = (int)((70 + difficultyTier * 15) * GetAggressionRateMult());
            int fireInterval = Math.Max(2, 5 - difficultyTier); // Fire much faster
            
            // AGGRESSIVE pursuit during attack - chase relentlessly
            Vector2 toTarget = target.Center - NPC.Center;
            if (toTarget.Length() > 150f)
            {
                toTarget.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * Phase2SpeedMult, 0.1f); // Faster pursuit
            }
            
            if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                // DOUBLE the projectiles for maximum chaos
                for (int i = 0; i < 4 + difficultyTier * 2; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    float speed = MediumProjectileSpeed + Main.rand.NextFloat(-2f, 6f);
                    Vector2 vel = angle.ToRotationVector2() * speed;
                    
                    Color color = Main.rand.NextBool() ? Gold : Violet;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, (int)(75 * Phase2DamageMult), color, 0.02f); // Stronger homing
                }
                
                CustomParticles.GenericFlare(NPC.Center, Gold, 0.4f, 6);
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
                        Color radiantColor = Color.Lerp(Gold, Violet, i / 10f);
                        var radiantBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, radiantColor, 0.25f + i * 0.08f, 15 + i, i % 2);
                        MagnumParticleHandler.SpawnParticle(radiantBurst);
                        
                        // Sparkle spray
                        float sparkAngle = MathHelper.TwoPi * i / 10f;
                        Vector2 sparkVel = sparkAngle.ToRotationVector2() * 5f;
                        var spark = new GlowSparkParticle(NPC.Center, sparkVel, radiantColor, 0.2f, 18);
                        MagnumParticleHandler.SpawnParticle(spark);
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
                TelegraphSystem.ConvergingRing(NPC.Center, 300f, chargeTime, Violet);
                
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
                    
                    // Cascading starbursts
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.5f, 25);
                    for (int i = 0; i < 12; i++)
                    {
                        Color color = Color.Lerp(DeepPurple, Gold, i / 12f);
                        var cascadeBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, color, 0.35f + i * 0.1f, 18 + i * 2, i % 2);
                        MagnumParticleHandler.SpawnParticle(cascadeBurst);
                        
                        // Shattered starlight fragments
                        float fragAngle = MathHelper.TwoPi * i / 12f;
                        Vector2 fragVel = fragAngle.ToRotationVector2() * 7f;
                        var fragment = new ShatteredStarlightParticle(NPC.Center, fragVel, color, 0.25f, 22, true, 0.08f);
                        MagnumParticleHandler.SpawnParticle(fragment);
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
        /// Starfall Apocalypse - Raining stars from above - APOCALYPTIC INTENSITY
        /// </summary>
        private void Attack_StarfallApocalypse(Player target)
        {
            int duration = (int)((80 + difficultyTier * 20) * GetAggressionRateMult());
            int fireInterval = Math.Max(2, 6 - difficultyTier * 2); // MUCH faster
            
            // Hover above - but closer for pressure
            Vector2 hoverPos = target.Center + new Vector2(0, -320f);
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 40f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * BaseSpeed, 0.08f);
            }
            
            // Warning flares - more of them
            if (Timer % 10 == 0)
            {
                for (int i = 0; i < 5 + difficultyTier * 2; i++)
                {
                    float xOffset = Main.rand.NextFloat(-450f, 450f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -600f);
                    CustomParticles.GenericFlare(warningPos, Gold * 0.5f, 0.35f, 10);
                }
            }
            
            // Fire stars - MUCH more intense
            if (Timer % fireInterval == 0 && Timer > 15 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 4 + difficultyTier * 2; // MORE STARS
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-500f, 500f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -650f);
                    float ySpeed = FastProjectileSpeed + difficultyTier * 3f;
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-3f, 3f), ySpeed);
                    
                    Color color = Main.rand.NextBool(3) ? StarWhite : Gold;
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.8f, (int)(80 * Phase2DamageMult), color, 15f); // Faster acceleration
                    
                    CustomParticles.GenericFlare(spawnPos, color, 0.35f, 8);
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
                
                // === PHASE 10 MUSICAL VFX: Crescendo Charge Up - Eternal Night Building ===
                Phase10Integration.Universal.CrescendoChargeUp(NPC.Center, Gold, progress);
                
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
                        Color finaleColor = Color.Lerp(DeepPurple, Gold, i / 10f);
                        var finaleBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, finaleColor, 0.35f + i * 0.08f, 18, i % 2);
                        MagnumParticleHandler.SpawnParticle(finaleBurst);
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
        /// Celestial Charge - Graceful sweeping dash attacks - FLUID DANCE OF LIGHT
        /// </summary>
        private void Attack_CelestialCharge(Player target)
        {
            int dashes = 4 + difficultyTier; // Fewer but more deliberate dashes
            int dashTime = 35; // Longer, more visible dashes
            int pauseTime = 40; // Longer pause for readability
            
            if (SubPhase < dashes * 2) // Alternating telegraph and dash
            {
                bool isDashing = SubPhase % 2 == 1;
                
                if (!isDashing) // Telegraph - GRACEFUL WINDUP
                {
                    if (Timer == 1)
                    {
                        // Predict where player will be and arc around them
                        Vector2 predictedPos = target.Center + target.velocity * 15f;
                        float angleToTarget = (predictedPos - NPC.Center).ToRotation();
                        
                        // Sweep in an arc, not straight line
                        float sweepOffset = (SubPhase / 2 % 2 == 0 ? 1 : -1) * MathHelper.PiOver4;
                        dashDirection = (angleToTarget + sweepOffset).ToRotationVector2();
                        dashTarget = target.Center + dashDirection * 400f;
                    }
                    
                    // Graceful deceleration
                    NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.08f);
                    
                    // Elegant warning arc instead of harsh line
                    float progress = Timer / (float)pauseTime;
                    for (int i = 0; i < 5; i++)
                    {
                        float arcProgress = i / 5f;
                        Vector2 arcPoint = NPC.Center + dashDirection * (100f + arcProgress * 400f) * progress;
                        Color arcColor = Color.Lerp(Violet, Gold, arcProgress) * (0.3f + progress * 0.4f);
                        if (Timer % 3 == 0)
                            CustomParticles.GenericFlare(arcPoint, arcColor, 0.25f + progress * 0.15f, 10);
                    }
                    
                    // Converging particles around boss - building energy
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 60f, progress, Gold, 6);
                    
                    // Rotate to face dash direction gracefully
                    NPC.rotation = MathHelper.Lerp(NPC.rotation, dashDirection.ToRotation(), 0.05f);
                    
                    if (Timer >= pauseTime)
                    {
                        Timer = 0;
                        SubPhase++;
                        SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 0.8f }, NPC.Center);
                    }
                }
                else // Dash - GRACEFUL SWEEP
                {
                    // Smooth acceleration curve instead of instant max speed
                    float dashProgress = Timer / (float)dashTime;
                    float speedCurve = (float)Math.Sin(dashProgress * MathHelper.Pi); // Smooth bell curve
                    float currentSpeed = BaseSpeed * Phase2SpeedMult * 1.8f * speedCurve;
                    
                    NPC.velocity = dashDirection * currentSpeed;
                    
                    // Flowing trail particles - elegant ribbons
                    if (Timer % 2 == 0)
                    {
                        // Main trail
                        CustomParticles.GenericFlare(NPC.Center, Gold * 0.8f, 0.45f, 15);
                        
                        // Side ribbons
                        Vector2 perpendicular = dashDirection.RotatedBy(MathHelper.PiOver2);
                        float ribbonOffset = (float)Math.Sin(Timer * 0.3f) * 30f;
                        CustomParticles.GenericFlare(NPC.Center + perpendicular * ribbonOffset, Violet * 0.6f, 0.3f, 12);
                        CustomParticles.GenericFlare(NPC.Center - perpendicular * ribbonOffset, Violet * 0.6f, 0.3f, 12);
                        
                        // Starlight trail
                        CustomParticles.GenericFlare(NPC.Center - dashDirection * 30f, StarWhite * 0.5f, 0.25f, 10);
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
                // Graceful recovery - slow drift to stop
                NPC.velocity = Vector2.Lerp(NPC.velocity, Vector2.Zero, 0.06f);
                NPC.rotation = MathHelper.Lerp(NPC.rotation, 0f, 0.03f);
                
                if (Timer >= 45)
                {
                    dashCount = 0;
                    NPC.rotation = 0f;
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Aurora Veil - Creates a protective barrier of light that reflects enemy attacks
        /// UNIQUE: Boss summons orbiting aurora shields that also fire projectiles
        /// </summary>
        private void Attack_AuroraVeil(Player target)
        {
            int chargeTime = 30;
            int activeTime = 180 + difficultyTier * 60; // Long-lasting veil
            int shieldCount = 6 + difficultyTier * 2;
            
            if (SubPhase == 0) // Summon veil
            {
                NPC.velocity *= 0.9f;
                
                float progress = Timer / (float)chargeTime;
                BossVFXOptimizer.ConvergingWarning(NPC.Center, 120f, progress, Violet, shieldCount);
                
                if (Timer % 3 == 0)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 80f, Violet * 0.7f, 0.4f, 15);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item29 with { Pitch = 0.4f }, NPC.Center);
                    
                    // Spawn shield burst
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 1.2f, 20);
                    for (int i = 0; i < shieldCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / shieldCount;
                        CustomParticles.HaloRing(NPC.Center + angle.ToRotationVector2() * 60f, Violet, 0.4f, 15);
                    }
                }
            }
            else if (SubPhase == 1) // Active - shields orbit and fire
            {
                // Slow follow
                Vector2 toTarget = target.Center - NPC.Center;
                if (toTarget.Length() > 200f)
                {
                    toTarget.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 0.4f, 0.03f);
                }
                else
                {
                    NPC.velocity *= 0.95f;
                }
                
                // Orbiting shields visual
                float orbitAngle = Timer * 0.05f;
                for (int i = 0; i < shieldCount; i++)
                {
                    float angle = orbitAngle + MathHelper.TwoPi * i / shieldCount;
                    float radius = 100f + (float)Math.Sin(Timer * 0.1f + i) * 20f;
                    Vector2 shieldPos = NPC.Center + angle.ToRotationVector2() * radius;
                    
                    Color shieldColor = Color.Lerp(Violet, Gold, (float)i / shieldCount);
                    CustomParticles.GenericFlare(shieldPos, shieldColor, 0.35f, 5);
                    
                    // Shields fire projectiles periodically
                    if (Timer % (30 - difficultyTier * 5) == i * 3 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        Vector2 fireDir = (target.Center - shieldPos).SafeNormalize(Vector2.UnitX);
                        BossProjectileHelper.SpawnHostileOrb(shieldPos, fireDir * MediumProjectileSpeed, (int)(60 * GetPhaseDamageMult()), shieldColor, 0.01f);
                    }
                }
                
                if (Timer >= activeTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                    
                    // Veil disperses in explosion
                    SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 80f, Violet, 0.6f, 18);
                    }
                    CustomParticles.HaloRing(NPC.Center, Gold, 0.8f, 20);
                }
            }
            else
            {
                if (Timer >= 30)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Supernova Collapse - Massive gravity well pulls everything in, then EXPLODES
        /// UNIQUE: Creates a black hole effect with devastating radial explosion
        /// </summary>
        private void Attack_SupernovaCollapse(Player target)
        {
            int collapseTime = 90 + difficultyTier * 20;
            int explosionDelay = 40;
            float collapseRadius = 350f;
            
            if (SubPhase == 0) // Collapse phase - everything gets pulled in
            {
                NPC.velocity *= 0.95f;
                
                float progress = Timer / (float)collapseTime;
                float currentRadius = collapseRadius * (1f - progress * 0.7f);
                
                // Warning zone
                BossVFXOptimizer.DangerZoneRing(NPC.Center, currentRadius, (int)(12 + progress * 8));
                
                // Swirling particles getting pulled in
                if (Timer % 2 == 0)
                {
                    int spiralCount = 8 + (int)(progress * 12);
                    for (int i = 0; i < spiralCount; i++)
                    {
                        float angle = Timer * 0.1f + MathHelper.TwoPi * i / spiralCount;
                        float dist = currentRadius * (1f - (Timer % 30) / 30f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * dist;
                        Color color = Color.Lerp(DeepPurple, Gold, progress);
                        CustomParticles.GenericFlare(pos, color, 0.25f + progress * 0.3f, 10);
                    }
                }
                
                // Growing core
                if (Timer % 5 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, Color.Lerp(DeepPurple, StarWhite, progress), 0.4f + progress * 0.8f, 15);
                }
                
                // Announce
                if (Timer == 1 && Main.netMode != NetmodeID.Server)
                    Main.NewText("Gravity bends to my will!", Violet);
                
                // Screen shake builds
                if (Timer > collapseTime * 0.5f)
                    MagnumScreenEffects.AddScreenShake(progress * 8f);
                
                if (Timer >= collapseTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item162 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                }
            }
            else if (SubPhase == 1) // Brief pause before explosion
            {
                // Everything goes dark momentarily
                float pauseProgress = Timer / (float)explosionDelay;
                
                // Pulsing core
                if (Timer % 4 == 0)
                {
                    float pulse = 1f + (float)Math.Sin(Timer * 0.5f) * 0.3f;
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 0.8f * pulse, 8);
                    CustomParticles.GenericFlare(NPC.Center, DeepPurple, 1.2f * pulse, 10);
                }
                
                if (Timer >= explosionDelay)
                {
                    Timer = 0;
                    SubPhase = 2;
                    
                    // MASSIVE EXPLOSION
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = -0.8f, Volume = 2f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(25f);
                    
                    // Core flash
                    CustomParticles.GenericFlare(NPC.Center, StarWhite, 2.5f, 30);
                    CustomParticles.GenericFlare(NPC.Center, Gold, 2.0f, 28);
                    CustomParticles.GenericFlare(NPC.Center, Violet, 1.8f, 25);
                    
                    // Cascading halos
                    for (int i = 0; i < 8; i++)
                    {
                        Color ringColor = Color.Lerp(Gold, Violet, i / 8f);
                        CustomParticles.HaloRing(NPC.Center, ringColor, 0.5f + i * 0.15f, 20 + i * 3);
                    }
                    
                    // Radial projectile explosion
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projectileCount = 24 + difficultyTier * 8;
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            float speed = FastProjectileSpeed * 1.2f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            Color projColor = i % 2 == 0 ? Gold : Violet;
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(90 * GetPhaseDamageMult()), projColor, 8f);
                        }
                        
                        // Secondary wave - slower homing
                        for (int i = 0; i < projectileCount / 2; i++)
                        {
                            float angle = MathHelper.TwoPi * i / (projectileCount / 2) + MathHelper.PiOver4;
                            Vector2 vel = angle.ToRotationVector2() * MediumProjectileSpeed * 0.6f;
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, (int)(70 * GetPhaseDamageMult()), StarWhite, 0.03f);
                        }
                    }
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 60)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Twilight Reversal - Spawns projectiles that REVERSE direction mid-flight
        /// UNIQUE: Projectiles fly outward, pause, then return toward player
        /// </summary>
        private void Attack_TwilightReversal(Player target)
        {
            int waves = 3 + difficultyTier;
            int waveDelay = 50 - difficultyTier * 8;
            int projectilesPerWave = 12 + difficultyTier * 4;
            
            if (SubPhase < waves)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.3f }, NPC.Center);
                    
                    // Spawn outgoing wave
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < projectilesPerWave; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectilesPerWave + SubPhase * 0.3f;
                            float speed = MediumProjectileSpeed * 0.8f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            // Spawn reversing projectile (uses wave projectile with special behavior)
                            BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, (int)(65 * GetPhaseDamageMult()), 
                                Color.Lerp(Violet, Gold, (float)i / projectilesPerWave), 6f);
                        }
                    }
                    
                    // VFX burst
                    CustomParticles.GenericFlare(NPC.Center, Violet, 0.9f, 20);
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f;
                        CustomParticles.HaloRing(NPC.Center + angle.ToRotationVector2() * 30f, Gold, 0.3f, 12);
                    }
                    
                    if (SubPhase == 0 && Main.netMode != NetmodeID.Server)
                        Main.NewText("Time flows... backward!", Gold);
                }
                
                // Slow hover
                Vector2 toTarget = target.Center - NPC.Center;
                if (toTarget.Length() > 300f)
                {
                    toTarget.Normalize();
                    NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * BaseSpeed * 0.3f, 0.02f);
                }
                else
                {
                    NPC.velocity *= 0.95f;
                }
                
                // Ambient particles
                if (Timer % 6 == 0)
                {
                    float angle = Main.rand.NextFloat() * MathHelper.TwoPi;
                    CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 50f, Violet * 0.6f, 0.3f, 12);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 40)
                    EndAttack();
            }
        }
        
        /// <summary>
        /// Quantum Blink - RAPID teleportation assault with projectile bursts at each location
        /// UNIQUE: Boss teleports 8-12 times in rapid succession, firing at each spot
        /// </summary>
        private void Attack_QuantumBlink(Player target)
        {
            int blinks = 8 + difficultyTier * 3;
            int teleportTime = 15; // Very fast
            int attackTime = 10;
            
            if (SubPhase < blinks * 2) // Alternating teleport and attack
            {
                bool isTeleporting = SubPhase % 2 == 0;
                
                if (isTeleporting)
                {
                    if (Timer == 1)
                    {
                        // Departure VFX
                        CustomParticles.GenericFlare(NPC.Center, Violet, 0.7f, 12);
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f;
                            CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 30f, Gold, 0.3f, 10);
                        }
                        
                        // Calculate teleport position - random around player
                        float teleportAngle = Main.rand.NextFloat() * MathHelper.TwoPi;
                        float teleportDist = Main.rand.NextFloat(150f, 350f);
                        Vector2 newPos = target.Center + teleportAngle.ToRotationVector2() * teleportDist;
                        
                        NPC.Center = newPos;
                        NPC.velocity = Vector2.Zero;
                        
                        SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.5f, Volume = 0.7f }, NPC.Center);
                    }
                    
                    // Fade in effect
                    float fadeProgress = Timer / (float)teleportTime;
                    if (Timer % 2 == 0)
                    {
                        CustomParticles.GenericFlare(NPC.Center, StarWhite * fadeProgress, 0.4f, 6);
                    }
                    
                    if (Timer >= teleportTime)
                    {
                        Timer = 0;
                        SubPhase++;
                    }
                }
                else // Attack burst
                {
                    if (Timer == 1)
                    {
                        // Fire burst of projectiles toward player
                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                            int burstCount = 3 + difficultyTier;
                            
                            for (int i = 0; i < burstCount; i++)
                            {
                                float spread = (i - burstCount / 2f) * 0.15f;
                                Vector2 vel = toTarget.RotatedBy(spread) * FastProjectileSpeed;
                                Color projColor = i % 2 == 0 ? Gold : Violet;
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, (int)(70 * GetPhaseDamageMult()), projColor, 12f);
                            }
                        }
                        
                        // Attack VFX
                        CustomParticles.GenericFlare(NPC.Center, Gold, 0.6f, 10);
                        SoundEngine.PlaySound(SoundID.Item12 with { Pitch = 0.3f, Volume = 0.6f }, NPC.Center);
                    }
                    
                    if (Timer >= attackTime)
                    {
                        Timer = 0;
                        SubPhase++;
                    }
                }
            }
            else
            {
                // Final position - brief pause
                NPC.velocity *= 0.9f;
                if (Timer >= 30)
                    EndAttack();
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
                
                // === PHASE 10 MUSICAL VFX: Death Finale - Celestial Queen's Final Note ===
                Phase10Integration.Universal.DeathFinale(NPC.Center, StarWhite, Gold);
                VFXIntegration.OnBossDeath("Nachtmusik", NPC.Center);
                
                for (int ring = 0; ring < 20; ring++)
                {
                    float scale = 0.35f + ring * 0.12f;
                    Color color = GetNachtmusikGradient(ring / 20f);
                    var deathBurst = new StarBurstParticle(NPC.Center, Vector2.Zero, color, scale, 25 + ring * 2, ring % 2);
                    MagnumParticleHandler.SpawnParticle(deathBurst);
                    
                    // Massive shattered starlight spray
                    float fragAngle = MathHelper.TwoPi * ring / 20f;
                    Vector2 fragVel = fragAngle.ToRotationVector2() * (10f + ring * 0.5f);
                    var fragment = new ShatteredStarlightParticle(NPC.Center, fragVel, color, 0.4f, 30, true, 0.06f);
                    MagnumParticleHandler.SpawnParticle(fragment);
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
                // Deactivate sky effect before NPC becomes inactive
                if (!Main.dedServ)
                {
                    if (SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] != null)
                    {
                        SkyManager.Instance.Deactivate("MagnumOpus:NachtmusikCelestialSky");
                    }
                }
                
                NPC.life = 0;
                NPC.HitEffect();
                NPC.active = false;
            }
        }
        
        #endregion

        #region Visual Effects
        
        private void SpawnAmbientParticles()
        {
            // Orbiting stars with star burst accents
            if (Main.GameUpdateCount % 8 == 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = starOrbitAngle + MathHelper.TwoPi * i / 3f;
                    float radius = 80f + (float)Math.Sin(Main.GameUpdateCount * 0.03f + i) * 20f;
                    Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                    Color color = isPhase2 ? Gold : Violet;
                    CustomParticles.GenericFlare(pos, color * 0.6f, 0.25f, 12);
                    
                    // Occasional star burst on orbit points in Phase 2
                    if (isPhase2 && Main.rand.NextBool(8))
                    {
                        var starBurst = new StarBurstParticle(pos, Vector2.Zero, color, 0.2f, 12);
                        MagnumParticleHandler.SpawnParticle(starBurst);
                    }
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
            
            // Golden shimmer in Phase 2 with occasional shattered starlight
            if (isPhase2 && Main.rand.NextBool(8))
            {
                Vector2 shimmerPos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.8f, NPC.height * 0.8f);
                Dust gold = Dust.NewDustDirect(shimmerPos, 1, 1, DustID.GoldFlame, 0, -0.5f, 0, default, 0.9f);
                gold.noGravity = true;
            }
            
            // Phase 2: Periodic shattered starlight fragments trailing from wings
            if (isPhase2 && Main.rand.NextBool(12))
            {
                Vector2 wingOffset = new Vector2(NPC.spriteDirection * 50f, -10f);
                Color fragmentColor = GetNachtmusikGradient(Main.rand.NextFloat());
                var fragment = new ShatteredStarlightParticle(NPC.Center + wingOffset, new Vector2(0, 1.5f) + Main.rand.NextVector2Circular(0.8f, 0.3f), fragmentColor, 0.25f, 25, true, 0.08f);
                MagnumParticleHandler.SpawnParticle(fragment);
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
            // Expert/Master mode: Treasure Bag
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<NachtmusikTreasureBag>()));
            
            // Non-Expert drops (only when not in Expert mode)
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            
            // Resonant Energy (15-25)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantEnergy>(), 1, 15, 25));
            
            // Resonant Core (8-12)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<NachtmusikResonantCore>(), 1, 8, 12));
            
            // Harmonic Core (guaranteed)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfNachtmusik>(), 1, 1, 1));
            
            // Remnants (30-50)
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<RemnantOfNachtmusiksHarmony>(), 1, 30, 50));
            
            // Shard of Nachtmusik's Tempo (12-20) - key crafting material
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<ShardOfNachtmusiksTempo>(), 1, 12, 20));
            
            // Random weapon drop in normal mode (1 weapon from pool)
            notExpert.OnSuccess(ItemDropRule.OneFromOptions(1, 
                ModContent.ItemType<NocturnalExecutioner>(),
                ModContent.ItemType<MidnightsCrescendo>(),
                ModContent.ItemType<TwilightSeverance>(),
                ModContent.ItemType<ConstellationPiercer>(),
                ModContent.ItemType<NebulasWhisper>(),
                ModContent.ItemType<SerenadeOfDistantStars>(),
                ModContent.ItemType<StarweaversGrimoire>(),
                ModContent.ItemType<RequiemOfTheCosmos>(),
                ModContent.ItemType<CelestialChorusBaton>(),
                ModContent.ItemType<GalacticOverture>(),
                ModContent.ItemType<ConductorOfConstellations>()
            ));
            
            npcLoot.Add(notExpert);
            
            // Trophy and mask (drop in all modes)
            // TODO: Create trophy and mask items
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikTrophy>(), 10));
            // npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<NachtmusikMask>(), 7));
        }
        
        public override void OnKill()
        {
            // Mark boss as defeated
            MoonlightSonataSystem.DownedNachtmusik = true;
            
            // Deactivate the celestial sky effect
            if (!Main.dedServ)
            {
                if (SkyManager.Instance["MagnumOpus:NachtmusikCelestialSky"] != null)
                {
                    SkyManager.Instance.Deactivate("MagnumOpus:NachtmusikCelestialSky");
                }
            }
            
            if (Main.netMode == NetmodeID.Server)
            {
                NetMessage.SendData(MessageID.WorldData);
            }
        }
        
        #endregion
    }
}
