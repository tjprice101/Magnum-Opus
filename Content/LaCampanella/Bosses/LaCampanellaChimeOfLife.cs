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
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.ResonantWeapons;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.LaCampanella.Bosses
{
    /// <summary>
    /// LA CAMPANELLA, CHIME OF LIFE - POST-MOON LORD BOSS
    /// 
    /// Design Philosophy (Vanilla-Inspired):
    /// - A massive infernal bell that combines ground-based pressure with aerial denial
    /// - Inspired by: Golem's predictable slam patterns, Wall of Flesh's advancing pressure,
    ///   Queen Bee's charge patterns, Moon Lord's celestial telegraphs
    /// 
    /// Key Vanilla Principles Applied:
    /// - Clear audio/visual cues before each attack (bell tolls, fire gathering)
    /// - Ground shockwaves have visible travel time
    /// - Fire walls telegraph their direction clearly
    /// - Attack windows between slams for damage dealing
    /// - Slam patterns are learnable and dodgeable
    /// 
    /// Combat Identity:
    /// - Ground-based pressure boss with arena control
    /// - Fire rain creates vertical danger
    /// - Slams create horizontal shockwaves
    /// - Player must balance vertical and horizontal dodging
    /// </summary>
    public class LaCampanellaChimeOfLife : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/LaCampanella/Bosses/LaCampanellaChimeOfLife";
        
        #region Theme Colors
        private static readonly Color CampanellaOrange = new Color(255, 140, 40);
        private static readonly Color CampanellaGold = new Color(255, 200, 80);
        private static readonly Color CampanellaBlack = new Color(30, 20, 25);
        private static readonly Color CampanellaCrimson = new Color(200, 50, 30);
        private static readonly Color CampanellaWhite = new Color(255, 240, 220);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 5f;
        private const int BaseDamage = 110;
        private const float ChaseRange = 800f;
        private const int AttackWindowFrames = 75;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Grounded,
            Attack,
            Slam,
            Recovery,
            Enraged,
            Dying
        }
        
        private enum AttackPattern
        {
            // Core Attacks (Always available)
            BellSlam,
            TollWave,
            EmberShower,
            
            // Phase 2 (Below 65% HP)
            FireWallSweep,
            ChimeRings,
            InfernoCircle,
            RhythmicToll,           // Calamity-style bullet hell phase
            InfernalJudgment,       // NEW: Hero's Judgment style spectacle attack
            BellLaserGrid,          // NEW: Crossing laser beams
            
            // Phase 3 (Below 35% HP)
            TripleSlam,
            InfernalTorrent,
            InfernoCage,            // Environment trap attack
            ResonantShock,          // NEW: Electrical pulse wave
            GrandFinale
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
        private AttackPattern lastAttack = AttackPattern.BellSlam;
        private int consecutiveAttacks = 0;
        
        private int slamCount = 0;
        private Vector2 slamTarget;
        private bool isAirborne = false;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
        private float groundLevel = 0f;
        private bool onGround = false;
        
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
            NPCID.Sets.TrailCacheLength[Type] = 10;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire3] = true;
        }

        public override void SetDefaults()
        {
            // Hitbox = 80% of frame size (678x454 frame)
            NPC.width = 542;
            NPC.height = 363;
            NPC.damage = BaseDamage;
            NPC.defense = 75;
            NPC.lifeMax = 400000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            NPC.value = Item.buyPrice(gold: 18);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            NPC.scale = 1f;
            
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/TheChimeOfLife");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.TheUnderworld,
                new FlavorTextBestiaryInfoElement("La Campanella, the Chime of Life - an ancient bell forged in infernal fires, its toll heralds destruction.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.LaCampanella);
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
                NPC.velocity.Y += 0.3f;
                NPC.EncourageDespawn(60);
                return;
            }
            
            UpdateDifficultyTier();
            UpdateGroundCheck();
            if (attackCooldown > 0) attackCooldown--;
            CheckEnrage(target);
            
            switch (State)
            {
                case BossPhase.Spawning:
                    AI_Spawning(target);
                    break;
                case BossPhase.Grounded:
                    AI_Grounded(target);
                    break;
                case BossPhase.Attack:
                    AI_Attack(target);
                    break;
                case BossPhase.Slam:
                    AI_Slam(target);
                    break;
                case BossPhase.Recovery:
                    AI_Recovery(target);
                    break;
                case BossPhase.Enraged:
                    AI_Enraged(target);
                    break;
            }
            
            Timer++;
            UpdateAnimation();
            SpawnAmbientParticles();
            
            // Dialogue triggers at HP thresholds only
            
            float lightIntensity = isEnraged ? 1.5f : 1.0f;
            Lighting.AddLight(NPC.Center, CampanellaOrange.ToVector3() * lightIntensity);
        }
        
        #region Ground & Phase Management
        
        private void UpdateGroundCheck()
        {
            Vector2 bottomCenter = NPC.Bottom;
            int tileX = (int)(bottomCenter.X / 16);
            int tileY = (int)(bottomCenter.Y / 16);
            
            onGround = false;
            groundLevel = 0f;
            
            // Check below for ground
            for (int y = tileY; y < tileY + 10; y++)
            {
                Tile tile = Framing.GetTileSafely(tileX, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    groundLevel = y * 16f;
                    onGround = NPC.Bottom.Y >= groundLevel - 25f;
                    break;
                }
            }
            
            // CRITICAL: Prevent boss from going INTO the ground
            // If the boss center is inside a solid tile, push it up
            int centerTileY = (int)(NPC.Center.Y / 16);
            Tile centerTile = Framing.GetTileSafely(tileX, centerTileY);
            if (centerTile.HasTile && Main.tileSolid[centerTile.TileType])
            {
                // Boss is clipping into ground - push up to surface
                for (int y = centerTileY; y >= centerTileY - 20; y--)
                {
                    Tile checkTile = Framing.GetTileSafely(tileX, y);
                    if (!checkTile.HasTile || !Main.tileSolid[checkTile.TileType])
                    {
                        // Found air - place boss bottom at this tile's bottom
                        NPC.position.Y = (y + 1) * 16f - NPC.height;
                        NPC.velocity.Y = 0;
                        groundLevel = (y + 1) * 16f;
                        onGround = true;
                        break;
                    }
                }
            }
            
            // Also check if boss bottom is inside ground and push up
            if (groundLevel > 0 && NPC.Bottom.Y > groundLevel + 5f)
            {
                NPC.position.Y = groundLevel - NPC.height;
                NPC.velocity.Y = 0;
                onGround = true;
            }
        }
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.65f ? 0 : (hpPercent > 0.35f ? 1 : 2);
            
            if (newTier != difficultyTier)
            {
                difficultyTier = newTier;
                AnnounceDifficultyChange();
            }
        }
        
        private void AnnounceDifficultyChange()
        {
            if (State == BossPhase.Spawning) return;
            
            MagnumScreenEffects.AddScreenShake(difficultyTier == 2 ? 20f : 14f);
            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = -0.5f + difficultyTier * 0.2f, Volume = 1.5f }, NPC.Center);
            
            CustomParticles.GenericFlare(NPC.Center, Color.White, 1.3f, 25);
            for (int i = 0; i < 12 + difficultyTier * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (12 + difficultyTier * 4);
                Color flareColor = Color.Lerp(CampanellaOrange, CampanellaCrimson, difficultyTier / 2f);
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 80f, flareColor, 0.6f, 18);
            }
            
            for (int r = 0; r < 8; r++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(CampanellaGold, CampanellaCrimson, r / 8f), 0.3f + r * 0.15f, 18 + r * 3);
            }
            
            for (int i = 0; i < 15; i++)
            {
                Vector2 vel = Main.rand.NextVector2Circular(8f, 6f);
                var smoke = new HeavySmokeParticle(NPC.Center + Main.rand.NextVector2Circular(60f, 60f), vel, CampanellaBlack, Main.rand.Next(40, 60), 0.5f, 1.0f, 0.015f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            string message = difficultyTier == 2 ? "PRESTO INFERNALE!" : "VIVACE!";
            Color textColor = difficultyTier == 2 ? CampanellaCrimson : CampanellaOrange;
            CombatText.NewText(NPC.Hitbox, textColor, message, true);
            
            // Use dialogue system for phase transitions
            if (difficultyTier == 1)
                BossDialogueSystem.LaCampanella.OnPhase2(NPC.whoAmI);
            else if (difficultyTier == 2)
                BossDialogueSystem.LaCampanella.OnPhase3(NPC.whoAmI);
        }
        
        private void CheckEnrage(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            
            if (distance > ChaseRange * 1.5f)
            {
                enrageTimer++;
                if (enrageTimer > 120 && !isEnraged)
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    
                    BossDialogueSystem.LaCampanella.OnEnrage();
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
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
                        State = BossPhase.Grounded;
                        Timer = 0;
                    }
                }
            }
        }
        
        #endregion
        
        #region AI States
        
        private void AI_Spawning(Player target)
        {
            if (Timer == 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_BetsyFireballShot with { Pitch = -0.6f, Volume = 1.3f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, CampanellaOrange, 1.0f, 20);
            }
            
            NPC.velocity.Y += 0.35f;
            if (NPC.velocity.Y > 12f) NPC.velocity.Y = 12f;
            
            if (Timer % 4 == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(pos, CampanellaOrange, 0.4f, 12);
            }
            
            if (onGround || Timer > 150)
            {
                NPC.velocity = Vector2.Zero;
                
                MagnumScreenEffects.AddScreenShake(18f);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.4f, Volume = 1.5f }, NPC.Center);
                
                for (int i = 0; i < 8; i++)
                {
                    CustomParticles.HaloRing(NPC.Bottom, Color.Lerp(CampanellaOrange, CampanellaCrimson, i / 8f), 0.25f + i * 0.1f, 15 + i * 2);
                }
                
                for (int i = 0; i < 12; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 3f);
                    var smoke = new HeavySmokeParticle(NPC.Bottom + Main.rand.NextVector2Circular(80f, 20f), vel, CampanellaBlack, Main.rand.Next(35, 50), 0.4f, 0.8f, 0.012f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                BossDialogueSystem.LaCampanella.OnSpawn(NPC.whoAmI);
                
                // Activate the infernal sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] != null)
                {
                    SkyManager.Instance.Activate("MagnumOpus:LaCampanellaSky");
                }
                
                Timer = 0;
                State = BossPhase.Grounded;
                attackCooldown = AttackWindowFrames;
            }
        }
        
        private void AI_Grounded(Player target)
        {
            float targetX = target.Center.X;
            float moveSpeed = BaseSpeed * (1f + difficultyTier * 0.2f);
            
            if (Math.Abs(NPC.Center.X - targetX) > 150f)
            {
                float dir = Math.Sign(targetX - NPC.Center.X);
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dir * moveSpeed, 0.06f);
            }
            else
            {
                NPC.velocity.X *= 0.9f;
            }
            
            if (!onGround && !isAirborne)
            {
                NPC.velocity.Y += 0.5f;
                if (NPC.velocity.Y > 18f) NPC.velocity.Y = 18f;
            }
            else if (onGround)
            {
                NPC.velocity.Y = 0;
            }
            
            if (attackCooldown <= 0 && Timer > 40)
            {
                SelectNextAttack(target);
            }
            
            int fireRate = 40 - difficultyTier * 8; // Slower rate, fewer projectiles
            if (Timer % fireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient && Timer > 30)
            {
                float angle = MathHelper.ToRadians(-90 + Main.rand.NextFloat(-25f, 25f));
                Vector2 vel = angle.ToRotationVector2() * (9f + difficultyTier * 2f); // Faster projectiles
                // Alternate between orb and bolt for variety
                if (Main.rand.NextBool())
                    BossProjectileHelper.SpawnHostileOrb(NPC.Top - new Vector2(0, 20), vel, 65 + difficultyTier * 5, CampanellaOrange, 0.015f);
                else
                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Top - new Vector2(0, 20), vel * 0.8f, 65 + difficultyTier * 5, CampanellaGold, 14f);
                CustomParticles.GenericFlare(NPC.Top, CampanellaGold, 0.4f, 10);
            }
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.BellSlam,
                AttackPattern.TollWave,
                AttackPattern.EmberShower
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.FireWallSweep);
                pool.Add(AttackPattern.ChimeRings);
                pool.Add(AttackPattern.InfernoCircle);
                pool.Add(AttackPattern.RhythmicToll);
                pool.Add(AttackPattern.InfernalJudgment);  // Hero's Judgment style
                pool.Add(AttackPattern.BellLaserGrid);     // Laser grid attack
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.TripleSlam);
                pool.Add(AttackPattern.InfernalTorrent);
                pool.Add(AttackPattern.InfernoCage);
                pool.Add(AttackPattern.ResonantShock);     // Electrical shock wave
                
                if (consecutiveAttacks >= 5 && Main.rand.NextBool(3))
                {
                    pool.Add(AttackPattern.GrandFinale);
                }
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            consecutiveAttacks++;
            
            if (CurrentAttack == AttackPattern.BellSlam || CurrentAttack == AttackPattern.TripleSlam)
            {
                State = BossPhase.Slam;
                slamCount = CurrentAttack == AttackPattern.TripleSlam ? 3 : 1;
            }
            else
            {
                State = BossPhase.Attack;
            }
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.TollWave:
                    Attack_TollWave(target);
                    break;
                case AttackPattern.EmberShower:
                    Attack_EmberShower(target);
                    break;
                case AttackPattern.FireWallSweep:
                    Attack_FireWallSweep(target);
                    break;
                case AttackPattern.ChimeRings:
                    Attack_ChimeRings(target);
                    break;
                case AttackPattern.InfernoCircle:
                    Attack_InfernoCircle(target);
                    break;
                case AttackPattern.RhythmicToll:
                    Attack_RhythmicToll(target);
                    break;
                case AttackPattern.InfernalJudgment:
                    Attack_InfernalJudgment(target);
                    break;
                case AttackPattern.BellLaserGrid:
                    Attack_BellLaserGrid(target);
                    break;
                case AttackPattern.InfernalTorrent:
                    Attack_InfernalTorrent(target);
                    break;
                case AttackPattern.InfernoCage:
                    Attack_InfernoCage(target);
                    break;
                case AttackPattern.ResonantShock:
                    Attack_ResonantShock(target);
                    break;
                case AttackPattern.GrandFinale:
                    Attack_GrandFinale(target);
                    break;
            }
        }
        
        private void AI_Slam(Player target)
        {
            int windupTime = 40 - difficultyTier * 8;
            int hangTime = 15;
            int descendSpeed = 25 + difficultyTier * 5;
            
            if (SubPhase == 0)
            {
                NPC.velocity.X *= 0.9f;
                
                if (Timer == 1)
                {
                    slamTarget = target.Center;
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = -0.2f }, NPC.Center);
                }
                
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2CircularEdge(70f, 70f);
                        CustomParticles.GenericFlare(NPC.Center + offset, CampanellaOrange, 0.35f, 10);
                    }
                }
                
                if (Timer > windupTime / 2)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        float xOffset = slamTarget.X - 300f + i * 60f;
                        CustomParticles.GenericFlare(new Vector2(xOffset, slamTarget.Y), CampanellaCrimson * 0.3f, 0.15f, 3);
                    }
                }
                
                if (Timer >= windupTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    isAirborne = true;
                    
                    float horizontalDist = slamTarget.X - NPC.Center.X;
                    NPC.velocity.Y = -18f - difficultyTier * 2f;
                    NPC.velocity.X = horizontalDist / 35f;
                    
                    SoundEngine.PlaySound(SoundID.Item14 with { Pitch = 0.1f, Volume = 1.2f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Bottom, CampanellaGold, 0.7f, 15);
                }
            }
            else if (SubPhase == 1)
            {
                NPC.velocity.Y += 0.3f;
                
                if (Timer % 3 == 0)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(35f, 35f);
                    CustomParticles.GenericFlare(pos, CampanellaOrange, 0.4f, 10);
                }
                
                if (NPC.velocity.Y >= 0)
                {
                    Timer = 0;
                    SubPhase = 2;
                    NPC.velocity.X *= 0.3f;
                }
            }
            else if (SubPhase == 2)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.3f, Volume = 1.3f }, NPC.Center);
                }
                
                if (Timer % 2 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Bottom, CampanellaCrimson, 0.5f, 8);
                }
                
                if (Timer >= hangTime)
                {
                    Timer = 0;
                    SubPhase = 3;
                }
            }
            else if (SubPhase == 3)
            {
                NPC.velocity.Y += 1.2f;
                if (NPC.velocity.Y > descendSpeed) NPC.velocity.Y = descendSpeed;
                
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(40f, 40f);
                        Color trailColor = Color.Lerp(CampanellaOrange, CampanellaCrimson, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(NPC.Center + offset - NPC.velocity * 0.2f, trailColor, 0.45f, 10);
                    }
                }
                
                if (onGround || Timer > 80)
                {
                    Timer = 0;
                    SubPhase = 4;
                }
            }
            else if (SubPhase == 4)
            {
                NPC.velocity = Vector2.Zero;
                isAirborne = false;
                
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(16f + difficultyTier * 4f);
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Bottom, Color.White, 1.2f, 20);
                    for (int r = 0; r < 8; r++)
                    {
                        CustomParticles.HaloRing(NPC.Bottom, Color.Lerp(CampanellaGold, CampanellaCrimson, r / 8f), 0.25f + r * 0.12f, 16 + r * 2);
                    }
                    
                    for (int i = 0; i < 12; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 12f;
                        CustomParticles.GenericFlare(NPC.Bottom + angle.ToRotationVector2() * 80f, CampanellaOrange, 0.5f, 15);
                    }
                    
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(8f, 4f);
                        var smoke = new HeavySmokeParticle(NPC.Bottom + Main.rand.NextVector2Circular(60f, 15f), vel, CampanellaBlack, Main.rand.Next(30, 45), 0.5f, 0.9f, 0.012f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projCount = 6 + difficultyTier * 2; // Reduced from 10+3
                        for (int i = 0; i < projCount; i++)
                        {
                            float angle = MathHelper.Pi + MathHelper.Pi * i / (projCount - 1);
                            float speed = 13f + difficultyTier * 2.5f; // Faster
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            // Mix types for variety
                            if (i % 2 == 0)
                                BossProjectileHelper.SpawnHostileOrb(NPC.Bottom, vel, 75 + difficultyTier * 5, CampanellaCrimson, 0f);
                            else
                                BossProjectileHelper.SpawnWaveProjectile(NPC.Bottom, vel, 75 + difficultyTier * 5, CampanellaOrange, 4.5f);
                        }
                        
                        // Fewer upward bolts
                        for (int i = 0; i < 3 + difficultyTier; i++) // Reduced from 6+2
                        {
                            float angle = MathHelper.ToRadians(-135 + i * 90f / (2 + difficultyTier));
                            Vector2 vel = angle.ToRotationVector2() * (10f + difficultyTier * 1.5f);
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Top, vel, 70, CampanellaOrange, 14f);
                        }
                    }
                }
                
                if (Timer >= 18)
                {
                    slamCount--;
                    if (slamCount > 0)
                    {
                        Timer = 0;
                        SubPhase = 0;
                        slamTarget = Main.player[NPC.target].Center + Main.player[NPC.target].velocity * 15f;
                    }
                    else
                    {
                        EndAttack();
                    }
                }
            }
        }
        
        private void AI_Recovery(Player target)
        {
            // Smooth deceleration using easing curve
            float recoveryDuration = 21f;
            float progress = Timer / recoveryDuration;
            float easedProgress = BossAIUtilities.Easing.EaseOutCubic(progress);
            
            // Smooth velocity damping instead of harsh multiplier
            float dampFactor = MathHelper.Lerp(0.92f, 0.98f, easedProgress);
            NPC.velocity.X *= dampFactor;
            
            if (!onGround)
            {
                NPC.velocity.Y += 0.4f;
            }
            else
            {
                NPC.velocity.Y = 0;
            }
            
            // Recovery shimmer to show vulnerability window
            if (Timer % 4 == 0 && Timer < 18)
            {
                float shimmerProgress = Timer / 21f;
                BossVFXOptimizer.RecoveryShimmer(NPC.Center, CampanellaGold, 55f, shimmerProgress);
            }
            
            // Deceleration trail while slowing down
            if (Timer < 12 && Math.Abs(NPC.velocity.X) > 1f)
            {
                float trailProgress = Timer / 21f;
                BossVFXOptimizer.DecelerationTrail(NPC.Center, NPC.velocity, CampanellaOrange, trailProgress);
            }
            
            if (Timer >= 21)
            {
                // Ready to attack cue when recovery ends
                BossVFXOptimizer.ReadyToAttackCue(NPC.Center, CampanellaOrange, 0.6f);
                
                Timer = 0;
                State = BossPhase.Grounded;
                attackCooldown = AttackWindowFrames / 2;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
            float enrageSpeed = BaseSpeed * 2.5f;
            NPC.velocity = Vector2.Lerp(NPC.velocity, toTarget * enrageSpeed, 0.1f);
            
            int fireRate = 10 - difficultyTier * 2;
            if (Timer % fireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = MathHelper.TwoPi * i / 4f + Timer * 0.08f;
                    Vector2 vel = angle.ToRotationVector2() * 9f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 80, CampanellaCrimson, 0f);
                }
                CustomParticles.GenericFlare(NPC.Center, CampanellaCrimson, 0.5f, 10);
            }
            
            if (Timer % 3 == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(pos, CampanellaCrimson, 0.4f, 10);
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_TollWave(Player target)
        {
            int totalTolls = 3 + difficultyTier;
            int tollDelay = 35 - difficultyTier * 5;
            
            NPC.velocity.X *= 0.92f;
            
            if (SubPhase < totalTolls)
            {
                if (Timer < 20)
                {
                    if (Timer % 3 == 0)
                    {
                        float progress = Timer / 20f;
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f + Timer * 0.08f;
                            Vector2 pos = NPC.Center + angle.ToRotationVector2() * (60f - progress * 30f);
                            CustomParticles.GenericFlare(pos, CampanellaGold, 0.3f + progress * 0.2f, 10);
                        }
                    }
                }
                
                if (Timer == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.1f + SubPhase * 0.08f, Volume = 1.2f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projCount = 8 + difficultyTier * 2; // Reduced from 12+4
                        for (int i = 0; i < projCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projCount;
                            float speed = 11f + difficultyTier * 2f + SubPhase * 0.8f; // Faster
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            // Alternate between Wave and AcceleratingBolt for variety
                            if (i % 2 == 0)
                                BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, CampanellaGold, 4f);
                            else
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.9f, 70, CampanellaOrange, 15f);
                        }
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.8f, 18);
                    for (int r = 0; r < 5; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, CampanellaOrange * (1f - r * 0.15f), 0.3f + r * 0.1f, 14 + r * 2);
                    }
                }
                
                if (Timer >= tollDelay)
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
        
        private void Attack_EmberShower(Player target)
        {
            int duration = 70 + difficultyTier * 21; // Shorter duration
            int fireInterval = 14 - difficultyTier * 2; // Slower fire rate = fewer projectiles
            
            Vector2 hoverPos = target.Center + new Vector2(0, -350f);
            Vector2 toHover = hoverPos - NPC.Center;
            if (toHover.Length() > 60f)
            {
                toHover.Normalize();
                NPC.velocity = Vector2.Lerp(NPC.velocity, toHover * 8f, 0.05f);
            }
            else
            {
                NPC.velocity *= 0.9f;
            }
            
            if (Timer % 25 == 0)
            {
                for (int i = 0; i < 4 + difficultyTier; i++)
                {
                    float xOffset = Main.rand.NextFloat(-350f, 350f);
                    Vector2 warningPos = target.Center + new Vector2(xOffset, -450f);
                    CustomParticles.GenericFlare(warningPos, CampanellaOrange * 0.4f, 0.3f, 18);
                }
            }
            
            if (Timer % fireInterval == 0 && Timer > 25 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int count = 2 + difficultyTier;
                for (int i = 0; i < count; i++)
                {
                    float xOffset = Main.rand.NextFloat(-400f, 400f);
                    Vector2 spawnPos = target.Center + new Vector2(xOffset, -500f);
                    Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), 7f + difficultyTier * 2f);
                    BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 75 + difficultyTier * 5, CampanellaCrimson, 18f);
                    
                    CustomParticles.GenericFlare(spawnPos, CampanellaOrange, 0.45f, 12);
                }
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        private void Attack_FireWallSweep(Player target)
        {
            int wallCount = 2 + difficultyTier;
            int wallDelay = 39 - difficultyTier * 6;
            
            NPC.velocity.X *= 0.92f;
            
            if (SubPhase < wallCount)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.1f + SubPhase * 0.1f }, NPC.Center);
                }
                
                if (Timer >= 7 && Timer < 21)
                {
                    int side = (SubPhase % 2 == 0) ? -1 : 1;
                    Vector2 wallStart = target.Center + new Vector2(side * 650f, -350f);
                    
                    for (int i = 0; i < 20; i++)
                    {
                        Vector2 warningPos = wallStart + new Vector2(0, i * 40f);
                        Color warningColor = CampanellaOrange * (0.3f + (Timer - 7) / 14f * 0.3f);
                        CustomParticles.GenericFlare(warningPos, warningColor, 0.2f, 3);
                    }
                }
                
                if (Timer == 21 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int side = (SubPhase % 2 == 0) ? -1 : 1;
                    Vector2 wallStart = target.Center + new Vector2(side * 650f, -350f);
                    Vector2 wallDir = new Vector2(-side, 0);
                    
                    int projCount = 10 + difficultyTier * 2; // Reduced from 18+4
                    for (int i = 0; i < projCount; i++)
                    {
                        Vector2 spawnPos = wallStart + new Vector2(0, i * 70f); // More spacing
                        float speed = 14f + difficultyTier * 3f; // Faster
                        Vector2 vel = wallDir * speed;
                        // Alternate types for variety
                        if (i % 3 == 0)
                            BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel, 80, CampanellaOrange, 4.5f);
                        else
                            BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 80, CampanellaCrimson, 0f);
                    }
                    
                    MagnumScreenEffects.AddScreenShake(8f);
                }
                
                if (Timer >= wallDelay)
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
        
        private void Attack_ChimeRings(Player target)
        {
            int ringCount = 5 + difficultyTier * 2;
            int ringDelay = 14 - difficultyTier * 2;
            
            NPC.velocity.X *= 0.9f;
            
            if (SubPhase < ringCount)
            {
                if (Timer == 8)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.3f + SubPhase * 0.03f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projPerRing = 6 + difficultyTier; // Reduced from 10+2
                        float baseAngle = MathHelper.TwoPi * SubPhase / ringCount;
                        
                        for (int i = 0; i < projPerRing; i++)
                        {
                            float angle = baseAngle + MathHelper.TwoPi * i / projPerRing;
                            float speed = 10f + difficultyTier * 2f; // Faster
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            Color orbColor = (SubPhase % 2 == 0) ? CampanellaGold : CampanellaOrange;
                            // Mix of wave and orb
                            if (i % 2 == 0)
                                BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, orbColor, 4f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 1.1f, 70, orbColor, 0.01f);
                        }
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.65f, 14);
                    CustomParticles.HaloRing(NPC.Center, CampanellaGold, 0.35f, 12);
                }
                
                if (Timer >= ringDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 18)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_InfernoCircle(Player target)
        {
            int duration = 70 + difficultyTier * 21;
            int arms = 4 + difficultyTier;
            float spinSpeed = 0.025f + difficultyTier * 0.008f;
            
            NPC.velocity.X *= 0.9f;
            
            if (Timer < 21)
            {
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < arms; i++)
                    {
                        float angle = MathHelper.TwoPi * i / arms + Timer * spinSpeed * 3f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * 50f;
                        CustomParticles.GenericFlare(pos, CampanellaOrange, 0.35f, 10);
                    }
                }
            }
            else
            {
                int fireInterval = 6 - difficultyTier; // Slightly slower = fewer projectiles
                if (Timer % fireInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    float spiralAngle = Timer * spinSpeed;
                    
                    for (int arm = 0; arm < arms; arm++)
                    {
                        float angle = spiralAngle + MathHelper.TwoPi * arm / arms;
                        float speed = 12f + difficultyTier * 2.5f; // Faster
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        // Alternate types
                        if (arm % 2 == 0)
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70, CampanellaOrange, 0f);
                        else
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.85f, 70, CampanellaGold, 16f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, CampanellaGold, 0.35f, 8);
                }
            }
            
            if (Timer >= duration)
            {
                EndAttack();
            }
        }
        
        private void Attack_InfernalTorrent(Player target)
        {
            int chargeTime = 35 - difficultyTier * 6;
            int barrageTime = 56 + difficultyTier * 14;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase == 0)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = -0.4f }, NPC.Center);
                }
                
                float progress = Timer / (float)chargeTime;
                
                if (Timer % 3 == 0)
                {
                    int particleCount = (int)(5 + progress * 10);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.06f;
                        float radius = 150f * (1f - progress * 0.6f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.GenericFlare(pos, Color.Lerp(CampanellaOrange, CampanellaCrimson, progress), 0.3f + progress * 0.3f, 10);
                    }
                }
                
                if (Timer > chargeTime / 2)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 4f);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.1f, Volume = 1.3f }, NPC.Center);
                }
            }
            else if (SubPhase == 1)
            {
                int interval = 8 - difficultyTier; // Slower fire rate = fewer projectiles
                if (Timer % interval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    float baseAngle = toPlayer.ToRotation();
                    
                    int projCount = 2 + difficultyTier; // Reduced from 3+tier
                    float spread = MathHelper.ToRadians(30 + difficultyTier * 8); // Tighter spread
                    
                    for (int i = 0; i < projCount; i++)
                    {
                        float angle = baseAngle - spread / 2f + spread * i / (projCount - 1);
                        angle += Main.rand.NextFloat(-0.08f, 0.08f);
                        float speed = 15f + difficultyTier * 2.5f + Main.rand.NextFloat(-1f, 1f); // Faster
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        Color color = Main.rand.NextBool() ? CampanellaCrimson : CampanellaOrange;
                        // Mix of bolt and orb
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 75, color, 12f);
                        else
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, color, 0.02f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center + toPlayer * 40f, CampanellaGold, 0.4f, 8);
                }
                
                if (Timer >= barrageTime)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_GrandFinale(Player target)
        {
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    Main.NewText("HEAR THE FINAL TOLL!", CampanellaCrimson);
                    SoundEngine.PlaySound(SoundID.Roar with { Pitch = -0.2f, Volume = 1.5f }, NPC.Center);
                }
                
                if (Timer % 4 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, CampanellaCrimson, 0.4f + Timer * 0.01f, 12);
                    
                    for (int i = 0; i < 15; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(5f, 3f);
                        var smoke = new HeavySmokeParticle(NPC.Center + Main.rand.NextVector2Circular(70f, 70f), vel, CampanellaBlack, Main.rand.Next(30, 50), 0.4f, 0.8f, 0.01f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(Timer * 0.15f);
                
                if (Timer >= 49)
                {
                    Timer = 0;
                    SubPhase = 1;
                    slamCount = 0;
                }
            }
            else if (SubPhase <= 3)
            {
                int windupTime = 20 - SubPhase * 3;
                int impactTime = 25;
                
                if (Timer < windupTime)
                {
                    if (Timer == 1)
                    {
                        slamTarget = target.Center + target.velocity * 12f;
                        SoundEngine.PlaySound(SoundID.DD2_BetsyFlameBreath with { Pitch = 0.1f * SubPhase }, NPC.Center);
                    }
                    
                    NPC.velocity.X *= 0.9f;
                }
                else if (Timer == windupTime)
                {
                    float horizontalDist = slamTarget.X - NPC.Center.X;
                    NPC.velocity.Y = -22f;
                    NPC.velocity.X = horizontalDist / 30f;
                    isAirborne = true;
                }
                else if (Timer < windupTime + 15)
                {
                    NPC.velocity.Y += 0.8f;
                }
                else if (Timer == windupTime + 15)
                {
                    NPC.velocity.Y = 30f;
                    NPC.velocity.X *= 0.2f;
                }
                else if (Timer < windupTime + 15 + impactTime)
                {
                    NPC.velocity.Y += 0.5f;
                    if (NPC.velocity.Y > 35f) NPC.velocity.Y = 35f;
                    
                    if (onGround)
                    {
                        Timer = windupTime + 15 + impactTime - 1;
                    }
                }
                else if (Timer == windupTime + 15 + impactTime)
                {
                    NPC.velocity = Vector2.Zero;
                    isAirborne = false;
                    
                    MagnumScreenEffects.AddScreenShake(22f);
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.2f, Volume = 1.6f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Bottom, Color.White, 1.4f, 22);
                    
                    for (int r = 0; r < 10; r++)
                    {
                        CustomParticles.HaloRing(NPC.Bottom, Color.Lerp(CampanellaGold, CampanellaCrimson, r / 10f), 0.3f + r * 0.15f, 18 + r * 3);
                    }
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projCount = 16 + SubPhase * 4;
                        for (int i = 0; i < projCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projCount;
                            float speed = 12f + SubPhase * 2f;
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            BossProjectileHelper.SpawnHostileOrb(NPC.Bottom, vel, 85, CampanellaCrimson, 0f);
                        }
                    }
                    
                    for (int i = 0; i < 18; i++)
                    {
                        Vector2 vel = Main.rand.NextVector2Circular(10f, 5f);
                        var smoke = new HeavySmokeParticle(NPC.Bottom + Main.rand.NextVector2Circular(80f, 20f), vel, CampanellaBlack, Main.rand.Next(40, 60), 0.6f, 1.1f, 0.012f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                }
                
                if (Timer >= windupTime + 15 + impactTime + 15)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (SubPhase == 4)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 20)
                {
                    MagnumScreenEffects.AddScreenShake(25f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.5f, Volume = 1.8f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 1.8f, 30);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int arm = 0; arm < 8; arm++)
                        {
                            float armAngle = MathHelper.PiOver4 * arm;
                            for (int p = 0; p < 6; p++)
                            {
                                float speed = 8f + p * 2.5f;
                                Vector2 vel = armAngle.ToRotationVector2() * speed;
                                Color color = arm % 2 == 0 ? CampanellaGold : CampanellaCrimson;
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 80, color, 0f);
                            }
                        }
                        
                        for (int i = 0; i < 24; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 24f;
                            Vector2 vel = angle.ToRotationVector2() * 14f;
                            BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 75, CampanellaOrange, 4f);
                        }
                    }
                    
                    for (int r = 0; r < 12; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(CampanellaGold, CampanellaCrimson, r / 12f), 0.35f + r * 0.18f, 20 + r * 4);
                    }
                }
                
                if (Timer >= 42)
                {
                    consecutiveAttacks = 0;
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Rhythmic Toll - Symmetric bullet-hell pattern attack.
        /// Creates precise, readable patterns that pulse outward in musical rhythm.
        /// Players must weave through geometric formations.
        /// </summary>
        private void Attack_RhythmicToll(Player target)
        {
            // 4 phases of symmetric patterns, each with increasing complexity
            int phaseDuration = 35 - difficultyTier * 4;
            int totalPhases = 4 + difficultyTier;
            
            NPC.velocity *= 0.92f;
            
            int currentPhase = SubPhase;
            int phaseTimer = Timer;
            
            if (currentPhase < totalPhases)
            {
                // Warning buildup - geometric pattern preview
                if (phaseTimer < 20)
                {
                    float progress = phaseTimer / 20f;
                    int previewCount = 8 + currentPhase * 2;
                    
                    for (int i = 0; i < previewCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / previewCount + currentPhase * 0.3f;
                        float radius = 60f + progress * 40f;
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color warningColor = CampanellaGold * (0.3f + progress * 0.4f);
                        CustomParticles.GenericFlare(pos, warningColor, 0.15f + progress * 0.15f, 4);
                    }
                }
                
                // The toll - spawn symmetric pattern
                if (phaseTimer == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.1f + currentPhase * 0.1f, Volume = 1.1f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Pattern varies by phase for variety
                        int pattern = currentPhase % 4;
                        
                        switch (pattern)
                        {
                            case 0: // Expanding ring - simple symmetric circle
                                SpawnSymmetricRing(NPC.Center, 12 + difficultyTier * 2, 11f + difficultyTier * 2f, 0f, CampanellaGold);
                                break;
                                
                            case 1: // Dual offset rings - two rings at different speeds
                                SpawnSymmetricRing(NPC.Center, 8 + difficultyTier, 9f + difficultyTier, MathHelper.Pi / 8f, CampanellaOrange);
                                SpawnSymmetricRing(NPC.Center, 8 + difficultyTier, 13f + difficultyTier * 1.5f, 0f, CampanellaCrimson);
                                break;
                                
                            case 2: // Star burst - projectiles in star points with gaps
                                int arms = 5 + difficultyTier;
                                for (int arm = 0; arm < arms; arm++)
                                {
                                    float armAngle = MathHelper.TwoPi * arm / arms;
                                    // Inner and outer projectile per arm
                                    Vector2 innerVel = armAngle.ToRotationVector2() * (8f + difficultyTier);
                                    Vector2 outerVel = armAngle.ToRotationVector2() * (14f + difficultyTier * 2f);
                                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, innerVel, 70, CampanellaGold, 0f);
                                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, outerVel, 70, CampanellaOrange, 10f);
                                }
                                break;
                                
                            case 3: // Spiral wave - rotating pattern
                                float spiralOffset = currentPhase * 0.4f;
                                for (int i = 0; i < 16 + difficultyTier * 2; i++)
                                {
                                    float angle = MathHelper.TwoPi * i / (16 + difficultyTier * 2) + spiralOffset;
                                    float speedVariation = 10f + (i % 4) * 1.5f + difficultyTier * 1.5f;
                                    Vector2 vel = angle.ToRotationVector2() * speedVariation;
                                    Color color = (i % 2 == 0) ? CampanellaGold : CampanellaOrange;
                                    BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, color, 4f);
                                }
                                break;
                        }
                    }
                    
                    // VFX - symmetric halo burst
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.7f, 16);
                    for (int r = 0; r < 4; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(CampanellaGold, CampanellaOrange, r / 4f), 0.25f + r * 0.1f, 12 + r * 2);
                    }
                }
                
                if (phaseTimer >= phaseDuration)
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
        
        /// <summary>
        /// Helper to spawn a symmetric ring of projectiles.
        /// </summary>
        private void SpawnSymmetricRing(Vector2 center, int count, float speed, float angleOffset, Color color)
        {
            for (int i = 0; i < count; i++)
            {
                float angle = MathHelper.TwoPi * i / count + angleOffset;
                Vector2 vel = angle.ToRotationVector2() * speed;
                BossProjectileHelper.SpawnHostileOrb(center, vel, 70, color, 0f);
            }
        }
        
        /// <summary>
        /// Inferno Cage - Environment trap attack.
        /// Creates closing fire walls that force player into shrinking safe zone.
        /// Must dodge through gaps in the closing cage.
        /// </summary>
        private void Attack_InfernoCage(Player target)
        {
            int warningTime = 28 - difficultyTier * 4;
            int cageCloseTime = 56 + difficultyTier * 14;
            float maxCageSize = 450f - difficultyTier * 50f;
            float minCageSize = 120f + difficultyTier * 20f;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase == 0) // Warning phase - show cage outline
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item73 with { Pitch = -0.3f, Volume = 1.2f }, NPC.Center);
                    cageCenter = target.Center; // Lock cage to player position at start
                }
                
                // Draw cage warning outline
                float progress = Timer / (float)warningTime;
                int sides = 4;
                for (int side = 0; side < sides; side++)
                {
                    float sideAngle = MathHelper.PiOver2 * side;
                    Vector2 sideDir = sideAngle.ToRotationVector2();
                    Vector2 sideCenter = cageCenter + sideDir * maxCageSize;
                    
                    // Draw warning particles along each wall
                    for (int p = 0; p < 10; p++)
                    {
                        float offset = (p - 4.5f) * 60f;
                        Vector2 perpDir = sideDir.RotatedBy(MathHelper.PiOver2);
                        Vector2 warningPos = sideCenter + perpDir * offset;
                        Color warningColor = CampanellaOrange * (0.2f + progress * 0.5f);
                        CustomParticles.GenericFlare(warningPos, warningColor, 0.15f + progress * 0.1f, 3);
                    }
                }
                
                if (Timer >= warningTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.3f }, cageCenter);
                }
            }
            else if (SubPhase == 1) // Cage closing phase
            {
                float progress = Timer / (float)cageCloseTime;
                float currentSize = MathHelper.Lerp(maxCageSize, minCageSize, progress);
                
                // Spawn cage walls as projectiles - only spawn periodically to avoid spam
                int spawnInterval = 8 - difficultyTier;
                if (Timer % spawnInterval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn projectiles moving inward on each side
                    // Leave gaps for player to dodge through
                    int projPerSide = 3 + difficultyTier;
                    float gapIndex = Timer / spawnInterval % projPerSide; // Rotating gap
                    
                    for (int side = 0; side < 4; side++)
                    {
                        float sideAngle = MathHelper.PiOver2 * side;
                        Vector2 sideDir = sideAngle.ToRotationVector2();
                        Vector2 perpDir = sideDir.RotatedBy(MathHelper.PiOver2);
                        
                        for (int p = 0; p < projPerSide; p++)
                        {
                            // Skip gap position to give player escape route
                            if (p == (int)((gapIndex + side) % projPerSide))
                                continue;
                                
                            float offset = (p - (projPerSide - 1) / 2f) * (currentSize * 0.4f);
                            Vector2 spawnPos = cageCenter + sideDir * currentSize + perpDir * offset;
                            Vector2 vel = -sideDir * (3f + difficultyTier * 0.5f);
                            
                            Color projColor = (p + side) % 2 == 0 ? CampanellaOrange : CampanellaCrimson;
                            BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 60, projColor, 0f);
                        }
                    }
                }
                
                // Visual cage walls
                if (Timer % 4 == 0)
                {
                    for (int side = 0; side < 4; side++)
                    {
                        float sideAngle = MathHelper.PiOver2 * side;
                        Vector2 sideDir = sideAngle.ToRotationVector2();
                        Vector2 sideCenter = cageCenter + sideDir * currentSize;
                        
                        for (int p = 0; p < 8; p++)
                        {
                            float offset = (p - 3.5f) * (currentSize * 0.15f);
                            Vector2 perpDir = sideDir.RotatedBy(MathHelper.PiOver2);
                            Vector2 flamePos = sideCenter + perpDir * offset;
                            Color flameColor = Color.Lerp(CampanellaOrange, CampanellaCrimson, progress);
                            CustomParticles.GenericFlare(flamePos, flameColor, 0.25f, 8);
                        }
                    }
                }
                
                // Corner danger indicators
                if (Timer % 6 == 0)
                {
                    for (int corner = 0; corner < 4; corner++)
                    {
                        float cornerAngle = MathHelper.PiOver4 + MathHelper.PiOver2 * corner;
                        Vector2 cornerPos = cageCenter + cornerAngle.ToRotationVector2() * currentSize * 1.3f;
                        CustomParticles.GenericFlare(cornerPos, CampanellaCrimson, 0.3f, 10);
                    }
                }
                
                // Screen tension effect as cage closes
                if (progress > 0.5f)
                {
                    MagnumScreenEffects.AddScreenShake((progress - 0.5f) * 3f);
                }
                
                if (Timer >= cageCloseTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                    
                    // Final explosion when cage closes
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.1f, Volume = 1.4f }, cageCenter);
                    MagnumScreenEffects.AddScreenShake(12f);
                    
                    CustomParticles.GenericFlare(cageCenter, Color.White, 1.0f, 20);
                    for (int r = 0; r < 6; r++)
                    {
                        CustomParticles.HaloRing(cageCenter, Color.Lerp(CampanellaGold, CampanellaCrimson, r / 6f), 0.3f + r * 0.15f, 14 + r * 3);
                    }
                    
                    // Final burst of projectiles outward
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 12 + difficultyTier * 2; i++)
                        {
                            float angle = MathHelper.TwoPi * i / (12 + difficultyTier * 2);
                            Vector2 vel = angle.ToRotationVector2() * (10f + difficultyTier * 2f);
                            BossProjectileHelper.SpawnWaveProjectile(cageCenter, vel, 70, CampanellaOrange, 4f);
                        }
                    }
                }
            }
            else // Recovery phase
            {
                if (Timer >= 28)
                {
                    EndAttack();
                }
            }
        }
        
        private Vector2 cageCenter; // Field to track cage center for InfernoCage attack
        private float[] laserStartAngles; // Field for laser grid attack
        
        /// <summary>
        /// INFERNAL JUDGMENT - Hero's Judgment style spectacle attack for La Campanella
        /// Features: Charge with converging flames, safe zone indicators, multi-wave bell tolls with safe arc
        /// 150 BPM timing: 24 frames per beat
        /// </summary>
        private void Attack_InfernalJudgment(Player target)
        {
            // DIFFICULTY: Shorter charge, more waves, faster projectiles, tighter safe arc
            int chargeTime = 38 - difficultyTier * 4; // Shorter (was 72-8)
            int waveCount = 4 + difficultyTier; // More waves (was 3+)
            
            if (SubPhase == 0)
            {
                // === CHARGE PHASE: Converging flames ===
                NPC.velocity *= 0.95f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = -0.6f, Volume = 1.3f }, NPC.Center);
                    Main.NewText("THE BELL TOLLS FOR THEE!", CampanellaGold);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // OPTIMIZED: Converging flame ring with reduced particles
                if (Timer % 5 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 250f, progress, CampanellaOrange, 6);
                    
                    // Occasional smoke for theme (reduced frequency)
                    if (Timer % 10 == 0)
                    {
                        Vector2 smokePos = NPC.Center + Main.rand.NextVector2CircularEdge(150f * (1f - progress * 0.5f), 150f * (1f - progress * 0.5f));
                        var smoke = new HeavySmokeParticle(smokePos, (NPC.Center - smokePos).SafeNormalize(Vector2.Zero) * 2f,
                            CampanellaBlack, 20, 0.2f, 0.35f, 0.012f, false);
                        MagnumParticleHandler.SpawnParticle(smoke);
                    }
                }
                
                // SAFE ZONE INDICATOR - earlier and clearer
                if (Timer > chargeTime / 3)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 100f, 10);
                    
                    // Also show safe arc from boss
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(28f), 150f, 6);
                }
                
                // Screen shake builds at 60%+ charge
                if (Timer > chargeTime * 0.6f)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 5f);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= waveCount)
            {
                // === RELEASE PHASE: Multi-wave radial burst with safe arc ===
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(18f);
                    SoundEngine.PlaySound(SoundID.Item28 with { Pitch = 0.1f + SubPhase * 0.12f, Volume = 1.5f }, NPC.Center);
                    
                    // OPTIMIZED: Use BossVFXOptimizer for release burst
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, CampanellaGold, CampanellaOrange, 1.2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster, tighter safe arc
                        int projectileCount = 36 + difficultyTier * 8; // More (was 28+6)
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(25f - difficultyTier * 2f); // Tighter (was 35f)
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // SAFE ARC EXEMPTION
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            // DIFFICULTY: Faster projectiles
                            float speed = 15f + difficultyTier * 3f + SubPhase * 2f; // Much faster (was 11f+2.5f+1.5f)
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            if (i % 3 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 85, CampanellaGold, 18f);
                            else
                                BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel * 0.85f, 85, CampanellaOrange, 4f);
                        }
                    }
                    
                    // OPTIMIZED: Cascading halos
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, CampanellaOrange, CampanellaGold, 6, 0.35f, 14);
                }
                
                // DIFFICULTY: Faster waves (36 frames = 1.5 beats instead of 48)
                if (Timer >= 25)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 25)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// BELL LASER GRID - Crossing laser beams that sweep the arena
        /// Features: Warning indicators, sweeping laser beams, safe spots
        /// </summary>
        private void Attack_BellLaserGrid(Player target)
        {
            // DIFFICULTY: More lasers, faster sweep
            int laserCount = 5 + difficultyTier; // More lasers (was 4+)
            int sweepDuration = 50 - difficultyTier * 7; // Faster (was 96-12)
            
            if (SubPhase == 0)
            {
                // === TELEGRAPH PHASE: Show laser paths ===
                NPC.velocity *= 0.92f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.3f }, NPC.Center);
                    laserStartAngles = new float[laserCount];
                    for (int i = 0; i < laserCount; i++)
                    {
                        laserStartAngles[i] = MathHelper.TwoPi * i / laserCount;
                    }
                }
                
                // OPTIMIZED: Use LaserBeamWarning for clear telegraph
                if (Timer % 4 == 0 && Timer < 25) // Shorter telegraph (was 48)
                {
                    float progress = Timer / 25f;
                    for (int i = 0; i < laserCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / laserCount + Timer * 0.02f;
                        BossVFXOptimizer.LaserBeamWarning(NPC.Center, angle, 1200f, progress);
                    }
                }
                
                // OPTIMIZED: Charge-up VFX - reduced frequency
                if (Timer % 8 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 80f, Timer / 25f, CampanellaOrange, 4);
                }
                
                if (Timer >= 25) // Shorter telegraph (was 48)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // === LASER SWEEP PHASE ===
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f, Volume = 1.4f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(12f);
                }
                
                float sweepProgress = Timer / (float)sweepDuration;
                float sweepAngle = sweepProgress * MathHelper.Pi * 1.2f; // Wider sweep (was Pi)
                
                // OPTIMIZED: Reduced visual beam particle count but maintain threat
                for (int i = 0; i < laserCount; i++)
                {
                    float currentAngle = laserStartAngles[i] + sweepAngle * (i % 2 == 0 ? 1 : -1);
                    
                    // Visual laser beam - fewer particles per frame
                    if (Timer % 2 == 0)
                    {
                        for (int j = 0; j < 15; j++) // Reduced (was 30)
                        {
                            float dist = 50f + j * 100f;
                            Vector2 beamPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                            Color beamColor = Color.Lerp(CampanellaOrange, CampanellaGold, (float)j / 15f);
                            BossVFXOptimizer.OptimizedFlare(beamPos, beamColor, 0.35f, 4);
                        }
                    }
                    
                    // OPTIMIZED: Occasional sparks only
                    if (Main.rand.NextBool(12))
                    {
                        float dist = Main.rand.NextFloat(100f, 1000f);
                        Vector2 sparkPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                        var spark = new GlowSparkParticle(sparkPos, Main.rand.NextVector2Circular(2f, 2f),
                            false, 10, 0.25f, CampanellaGold, new Vector2(0.4f, 1.2f), false, true);
                        MagnumParticleHandler.SpawnParticle(spark);
                    }
                    
                    // DIFFICULTY: More frequent projectiles (8 frames = third of beat)
                    if (Timer % 8 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int p = 0; p < 4 + difficultyTier; p++) // More projectiles per pulse
                        {
                            float dist = 120f + p * 180f;
                            Vector2 projPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                            float projSpeed = 5f + difficultyTier * 2f; // Faster
                            Vector2 projVel = currentAngle.ToRotationVector2() * projSpeed;
                            BossProjectileHelper.SpawnAcceleratingBolt(projPos, projVel, 80 + difficultyTier * 5, CampanellaOrange, 12f);
                        }
                    }
                }
                
                if (Timer >= sweepDuration)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else
            {
                if (Timer >= 20) // Shorter recovery (was 30)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// RESONANT SHOCK - Electrical pulse wave that expands outward
        /// Features: Building electrical charge, expanding shock rings, chain lightning
        /// </summary>
        private void Attack_ResonantShock(Player target)
        {
            // DIFFICULTY: More waves, faster
            int shockWaves = 5 + difficultyTier; // More waves (was 4+)
            int chargeTime = 36; // Shorter charge (was 48)
            int waveDelay = 18; // Faster waves (was 24)
            
            if (SubPhase == 0)
            {
                // === ELECTRICAL CHARGE BUILDUP ===
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item93 with { Pitch = -0.3f }, NPC.Center);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // OPTIMIZED: Use ElectricalBuildupWarning instead of custom loops
                if (Timer % 5 == 0)
                {
                    BossVFXOptimizer.ElectricalBuildupWarning(NPC.Center, CampanellaGold, 180f * (1f - progress * 0.5f), progress);
                }
                
                // OPTIMIZED: Reduced charge VFX frequency
                if (Timer % 6 == 0)
                {
                    BossVFXOptimizer.OptimizedRadialFlares(NPC.Center, Color.Lerp(CampanellaOrange, Color.White, progress),
                        4, 50f - progress * 25f, 0.3f + progress * 0.3f, 8);
                }
                
                // Show danger zone expanding
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.DangerZoneRing(NPC.Center, 80f + progress * 200f, 10);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= shockWaves)
            {
                // === SHOCK WAVE RELEASE ===
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f + SubPhase * 0.1f, Volume = 1.3f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(10f + SubPhase * 2f);
                    
                    // OPTIMIZED: Use BossVFXOptimizer burst
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, CampanellaGold, CampanellaOrange, 1.0f);
                    
                    // Spawn expanding shock ring projectiles
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster
                        int ringCount = 20 + difficultyTier * 5; // More (was 16+4)
                        float baseSpeed = 12f + SubPhase * 3f + difficultyTier * 2f; // Much faster (was 8f+2f+1.5f)
                        
                        for (int i = 0; i < ringCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / ringCount;
                            Vector2 vel = angle.ToRotationVector2() * baseSpeed;
                            
                            // Alternate between accelerating bolts and tracking orbs
                            if (i % 3 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 85 + difficultyTier * 5, CampanellaGold, 15f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 85 + difficultyTier * 5, CampanellaOrange, 0.015f);
                        }
                    }
                    
                    // OPTIMIZED: Chain lightning with fewer iterations
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f + Main.rand.NextFloat(0.3f);
                        Vector2 lightningEnd = NPC.Center + angle.ToRotationVector2() * (180f + SubPhase * 40f);
                        MagnumVFX.DrawLaCampanellaLightning(NPC.Center, lightningEnd, 8, 30f, 3, 0.7f);
                    }
                    
                    // OPTIMIZED: Cascading halos
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, CampanellaOrange, CampanellaGold, 5, 0.4f, 14);
                }
                
                if (Timer >= waveDelay)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 24) // Shorter recovery (was 36)
                {
                    EndAttack();
                }
            }
        }
        
        private void EndAttack()
        {
            // Spawn attack ending visual cue - infernal exhale effect
            BossVFXOptimizer.AttackEndCue(NPC.Center, CampanellaOrange, CampanellaGold, 0.8f);
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Recovery;
            attackCooldown = AttackWindowFrames - difficultyTier * 12;
        }
        
        #endregion
        
        #region Visuals
        
        private void UpdateAnimation()
        {
            int frameSpeed = State == BossPhase.Slam ? 2 : 4;
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
            if (State == BossPhase.Spawning) return;
            
            // Performance gate - skip under critical load
            if (BossVFXOptimizer.IsCriticalLoad) return;
            bool isHighLoad = BossVFXOptimizer.IsHighLoad;
            
            // Flame particles - reduce under load
            int flameChance = isHighLoad ? (6 - difficultyTier) : (4 - difficultyTier);
            if (Main.rand.NextBool(flameChance))
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.4f, NPC.height * 0.4f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-3f, -1f));
                Color color = Color.Lerp(CampanellaOrange, CampanellaGold, Main.rand.NextFloat());
                CustomParticles.GenericFlare(pos, color, 0.3f, Main.rand.Next(15, 25));
            }
            
            // Smoke particles - reduce under load
            int smokeChance = isHighLoad ? (14 - difficultyTier * 2) : (10 - difficultyTier * 2);
            if (Main.rand.NextBool(smokeChance))
            {
                Vector2 pos = NPC.Bottom + Main.rand.NextVector2Circular(50f, 10f);
                Vector2 vel = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-1f, 0.5f));
                var smoke = new HeavySmokeParticle(pos, vel, CampanellaBlack, Main.rand.Next(30, 50), 0.3f, 0.6f, 0.01f, false);
                MagnumParticleHandler.SpawnParticle(smoke);
            }
            
            // Enrage particles - reduce under load
            if (isEnraged && Timer % (isHighLoad ? 4 : 2) == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(70f, 70f);
                CustomParticles.GenericFlare(pos, CampanellaCrimson, 0.4f, 8);
            }
        }
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            if (deathTimer % 10 == 0)
            {
                float progress = deathTimer / 200f;
                MagnumScreenEffects.AddScreenShake(5f + progress * 20f);
                
                Vector2 burstPos = NPC.Center + Main.rand.NextVector2Circular(60f * (1f - progress * 0.4f), 60f * (1f - progress * 0.4f));
                CustomParticles.GenericFlare(burstPos, Color.Lerp(CampanellaOrange, Color.White, progress), 0.5f + progress * 0.6f, 18);
                CustomParticles.HaloRing(burstPos, CampanellaCrimson, 0.3f + progress * 0.4f, 14);
                
                for (int i = 0; i < 5; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(6f, 4f);
                    var smoke = new HeavySmokeParticle(burstPos + Main.rand.NextVector2Circular(30f, 30f), vel, CampanellaBlack, Main.rand.Next(35, 50), 0.4f, 0.8f, 0.012f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
            }
            
            if (deathTimer >= 200)
            {
                MagnumScreenEffects.AddScreenShake(35f);
                SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = -0.5f, Volume = 2f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 2.2f, 35);
                
                for (int i = 0; i < 15; i++)
                {
                    float scale = 0.35f + i * 0.18f;
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(CampanellaGold, CampanellaCrimson, i / 15f), scale, 25 + i * 4);
                }
                
                for (int i = 0; i < 24; i++)
                {
                    float angle = MathHelper.TwoPi * i / 24f;
                    CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 120f, CampanellaOrange, 0.8f, 30);
                }
                
                for (int i = 0; i < 40; i++)
                {
                    Vector2 vel = Main.rand.NextVector2Circular(15f, 10f);
                    var smoke = new HeavySmokeParticle(NPC.Center + Main.rand.NextVector2Circular(100f, 100f), vel, CampanellaBlack, Main.rand.Next(50, 80), 0.7f, 1.4f, 0.012f, false);
                    MagnumParticleHandler.SpawnParticle(smoke);
                }
                
                // Death dialogue
                BossDialogueSystem.LaCampanella.OnDeath();
                BossDialogueSystem.CleanupDialogue(NPC.whoAmI);
                
                // Deactivate the infernal sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:LaCampanellaSky"] != null)
                {
                    SkyManager.Instance.Deactivate("MagnumOpus:LaCampanellaSky");
                }
                
                // Set boss downed flag for miniboss essence drops
                MoonlightSonataSystem.DownedLaCampanella = true;
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
            
            if ((State == BossPhase.Slam && SubPhase >= 3) || isEnraged)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    float progress = (float)i / NPC.oldPos.Length;
                    Color trailColor = Color.Lerp(CampanellaOrange, CampanellaCrimson, progress) * (1f - progress) * 0.4f;
                    Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                    spriteBatch.Draw(tex, trailPos, sourceRect, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.1f), SpriteEffects.None, 0f);
                }
            }
            
            float pulse = (float)Math.Sin(Timer * 0.07f) * 0.3f + 0.7f;
            Color glowColor = isEnraged ? CampanellaCrimson : Color.Lerp(CampanellaOrange, CampanellaGold, pulse);
            glowColor.A = 0;
            spriteBatch.Draw(tex, drawPos, sourceRect, glowColor * 0.4f, NPC.rotation, origin, NPC.scale * 1.08f, SpriteEffects.None, 0f);
            
            Color mainColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16));
            mainColor = Color.Lerp(mainColor, Color.White, 0.4f);
            spriteBatch.Draw(tex, drawPos, sourceRect, mainColor, NPC.rotation, origin, NPC.scale, SpriteEffects.None, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<LaCampanellaTreasureBag>()));
            
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1, 25, 35));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<IgnitionOfTheBell>(), 3));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<InfernalChimesCalling>(), 3));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<FangOfTheInfiniteBell>(), 3));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<DualFatedChime>(), 3));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfLaCampanella>(), 1));
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
