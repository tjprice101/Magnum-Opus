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
using MagnumOpus.Content.EnigmaVariations.ResonanceEnergies;
using MagnumOpus.Content.EnigmaVariations.ResonantWeapons;
using MagnumOpus.Content.EnigmaVariations.HarmonicCores;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;
using MagnumOpus.Common.Systems.VFX;
using static MagnumOpus.Common.Systems.BossDialogueSystem;

namespace MagnumOpus.Content.EnigmaVariations.Bosses
{
    /// <summary>
    /// ENIGMA, THE HOLLOW MYSTERY - POST-MOON LORD BOSS
    /// 
    /// Design Philosophy (Vanilla-Inspired):
    /// - A nightmarish spider-like entity that embodies mystery and the unknown
    /// - Inspired by: Brain of Cthulhu's teleportation, Skeletron's spinning charges,
    ///   Lunatic Cultist's clone mechanics, Empress of Light's precise patterns
    /// 
    /// Key Vanilla Principles Applied:
    /// - Teleportation has clear visual/audio tells (void portals, gathering eyes)
    /// - Attacks have readable patterns that can be learned
    /// - Phase transitions have dramatic moments for player to prepare
    /// - Ultimate attack has a learnable safe strategy
    /// 
    /// Combat Identity:
    /// - Evasive boss that teleports and ambushes
    /// - Eyes and glyphs create area denial
    /// - Player must track and predict teleportation patterns
    /// - Mystery theme: attacks feel "unknowable" but ARE learnable
    /// </summary>
    public class EnigmaTheHollowMystery : ModNPC
    {
        public override string Texture => "MagnumOpus/Content/EnigmaVariations/Bosses/EnigmaTheHollowMystery";
        
        #region Theme Colors
        private static readonly Color EnigmaBlack = new Color(15, 10, 20);
        private static readonly Color EnigmaPurple = new Color(140, 60, 200);
        private static readonly Color EnigmaDeepPurple = new Color(80, 20, 120);
        private static readonly Color EnigmaGreen = new Color(50, 220, 100);
        private static readonly Color EnigmaVoid = new Color(30, 15, 40);
        #endregion
        
        #region Constants
        private const float BaseSpeed = 8f;
        private const int BaseDamage = 100;
        private const float TeleportRange = 700f;
        private const int AttackWindowFrames = 70;
        #endregion
        
        #region AI State
        private enum BossPhase
        {
            Spawning,
            Stalking,
            Attack,
            Teleport,
            Recovery,
            Enraged,
            Dying
        }
        
        private enum AttackPattern
        {
            // Core Attacks (Always available)
            VoidLunge,
            EyeVolley,
            ParadoxRing,
            
            // Phase 2 (Below 60% HP)
            ShadowDash,
            GlyphCircle,
            TendrilRise,
            ParadoxWeb,     // New bullet-hell spider web pattern
            
            // Phase 3 (Below 30% HP)
            RealityFracture,
            EyeOfTheVoid,
            RealityZones,   // New environment trap attack
            UltimateEnigma,
            
            // New Spectacle Attacks
            ParadoxJudgment,    // Hero's Judgment style with void energies
            VoidLaserWeb,       // Crossing laser beams in spider web pattern
            EntropicSurge,      // Electrical pulses of void energy
            
            // Unique Sigil/Formation Attacks
            SigilSnare,         // Glyphs spawn around player, converge inward trapping them
            VoidBeamPincer,     // Orbs spawn on player's sides with swirl animation, shoot beams
            WatchingGaze,       // Eyes spawn in formation, track player, then fire projectiles
            MysteryMaze,        // Create a maze of glyph walls player must navigate
            ParadoxMirror       // Clone illusions spawn with glyphs, only one is real danger
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
        private AttackPattern lastAttack = AttackPattern.VoidLunge;
        private int consecutiveAttacks = 0;
        
        private int lungeCount = 0;
        private Vector2 lungeDirection;
        
        private Vector2 teleportTarget;
        private bool isFading = false;
        
        private int enrageTimer = 0;
        private bool isEnraged = false;
        
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
            NPCID.Sets.TrailCacheLength[Type] = 12;
            NPCID.Sets.TrailingMode[Type] = 1;
            NPCID.Sets.MustAlwaysDraw[Type] = true;
            
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Darkness] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Blackout] = true;
        }

        public override void SetDefaults()
        {
            NPC.width = 540;  // 3x bigger (was 180)
            NPC.height = 360; // 3x bigger (was 120)
            NPC.damage = BaseDamage;
            NPC.defense = 65;
            NPC.lifeMax = 380000;
            NPC.HitSound = SoundID.NPCHit54;
            NPC.DeathSound = SoundID.NPCDeath52;
            NPC.knockBackResist = 0f;
            NPC.noGravity = false; // Ground-based boss - has gravity
            NPC.noTileCollide = true; // Still phases through tiles (teleport boss)
            NPC.value = Item.buyPrice(gold: 16);
            NPC.boss = true;
            NPC.npcSlots = 15f;
            NPC.aiStyle = -1;
            NPC.scale = 3f; // 3x bigger (was 1f)
            
            // Shift the sprite up so the hitbox isn't in the ground
            // Negative value shifts the sprite UP relative to hitbox
            DrawOffsetY = -120f;
            
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Assets/Music/EmbersOfTheRiddle");
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Times.NightTime,
                new FlavorTextBestiaryInfoElement("Enigma, the Hollow Mystery - an unknowable horror that watches from the void between realities.")
            });
        }

        public override void AI()
        {
            if (!hasRegisteredHealthBar)
            {
                BossHealthBarUI.RegisterBoss(NPC, BossColorTheme.Enigma);
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
                NPC.velocity.Y -= 0.3f;
                NPC.alpha = Math.Min(255, NPC.alpha + 5);
                if (NPC.alpha >= 255)
                    NPC.EncourageDespawn(10);
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
                case BossPhase.Stalking:
                    AI_Stalking(target);
                    break;
                case BossPhase.Attack:
                    AI_Attack(target);
                    break;
                case BossPhase.Teleport:
                    AI_Teleport(target);
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
            
            NPC.spriteDirection = NPC.direction = (target.Center.X > NPC.Center.X) ? 1 : -1;
            
            // Boss dialogue system - combat taunts and player HP checks
            if (State != BossPhase.Spawning)
            {
                // Dialogue triggers at HP thresholds only
                BossDialogueSystem.CheckPlayerLowHP(target, "Enigma");
            }
            
            float lightIntensity = isEnraged ? 1.2f : 0.8f;
            float pulse = (float)Math.Sin(Timer * 0.1f) * 0.2f + 0.8f;
            Lighting.AddLight(NPC.Center, EnigmaPurple.ToVector3() * lightIntensity * pulse);
        }
        
        #region Ground & Phase Management
        
        private void UpdateGroundCheck()
        {
            Vector2 bottomCenter = NPC.Bottom;
            int tileX = (int)(bottomCenter.X / 16);
            int tileY = (int)(bottomCenter.Y / 16);
            
            onGround = false;
            for (int y = tileY; y < tileY + 6; y++)
            {
                Tile tile = Framing.GetTileSafely(tileX, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    onGround = NPC.Bottom.Y >= y * 16f - 25f;
                    break;
                }
            }
        }
        
        private void UpdateDifficultyTier()
        {
            float hpPercent = (float)NPC.life / NPC.lifeMax;
            int newTier = hpPercent > 0.6f ? 0 : (hpPercent > 0.3f ? 1 : 2);
            
            if (newTier != difficultyTier)
            {
                difficultyTier = newTier;
                AnnounceDifficultyChange();
            }
        }
        
        private void AnnounceDifficultyChange()
        {
            if (State == BossPhase.Spawning) return;
            
            MagnumScreenEffects.AddScreenShake(difficultyTier == 2 ? 18f : 12f);
            SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.3f - difficultyTier * 0.1f, Volume = 1.3f }, NPC.Center);
            
            CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 1.2f, 25);
            for (int i = 0; i < 12 + difficultyTier * 4; i++)
            {
                float angle = MathHelper.TwoPi * i / (12 + difficultyTier * 4);
                Color color = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / (12 + difficultyTier * 4));
                CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 70f, color, 0.55f, 18);
            }
            
            for (int r = 0; r < 6; r++)
            {
                CustomParticles.HaloRing(NPC.Center, Color.Lerp(EnigmaPurple, EnigmaGreen, r / 6f), 0.3f + r * 0.15f, 16 + r * 3);
            }
            
            CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 8 + difficultyTier * 3, 6f);
            
            for (int i = 0; i < 5 + difficultyTier * 2; i++)
            {
                Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.5f, Main.player[NPC.target].Center);
            }
            
            string message = difficultyTier == 2 ? "V̴O̵I̷D̶ ̸R̶E̴V̵E̷L̴A̵T̵I̷O̵N̷" : "O̷B̶S̵E̶S̴S̶I̴O̶N̷";
            Color textColor = difficultyTier == 2 ? EnigmaGreen : EnigmaPurple;
            CombatText.NewText(NPC.Hitbox, textColor, message, true);
            
            // Use dialogue system for phase transitions
            if (difficultyTier == 1)
                BossDialogueSystem.Enigma.OnPhase2(NPC.whoAmI);
            else if (difficultyTier == 2)
                BossDialogueSystem.Enigma.OnPhase3(NPC.whoAmI);
        }
        
        private void CheckEnrage(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            
            if (distance > TeleportRange * 1.8f)
            {
                enrageTimer++;
                if (enrageTimer > 90 && !isEnraged)
                {
                    isEnraged = true;
                    State = BossPhase.Enraged;
                    Timer = 0;
                    
                    BossDialogueSystem.Enigma.OnEnrage();
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = 0.3f, Volume = 1.5f }, NPC.Center);
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
                        State = BossPhase.Stalking;
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
                SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.5f }, NPC.Center);
                CustomParticles.GenericFlare(NPC.Center, EnigmaPurple, 1.0f, 25);
                
                for (int i = 0; i < 8; i++)
                {
                    float angle = MathHelper.TwoPi * i / 8f;
                    Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * 150f;
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.6f, target.Center);
                }
            }
            
            NPC.alpha = (int)MathHelper.Lerp(255, 0, Timer / 49f);
            
            if (Timer % 5 == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.GenericFlare(pos, EnigmaPurple, 0.35f, 12);
            }
            
            if (Timer >= 49)
            {
                BossDialogueSystem.Enigma.OnSpawn(NPC.whoAmI);
                
                // Activate the Enigma mystery sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:EnigmaSky"] != null)
                {
                    SkyManager.Instance.Activate("MagnumOpus:EnigmaSky");
                }
                
                Timer = 0;
                State = BossPhase.Stalking;
                attackCooldown = AttackWindowFrames;
            }
        }
        
        private void AI_Stalking(Player target)
        {
            float distance = Vector2.Distance(NPC.Center, target.Center);
            float horizontalDist = Math.Abs(NPC.Center.X - target.Center.X);
            
            if (distance > TeleportRange)
            {
                InitiateTeleport(target);
                return;
            }
            
            // Ground-based horizontal movement only
            float moveSpeed = BaseSpeed * (1f + difficultyTier * 0.2f);
            float dirToTarget = Math.Sign(target.Center.X - NPC.Center.X);
            
            float idealDist = 300f - difficultyTier * 30f;
            if (horizontalDist > idealDist + 50f)
            {
                // Move toward player horizontally
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirToTarget * moveSpeed, 0.08f);
            }
            else if (horizontalDist < idealDist - 50f)
            {
                // Back away from player horizontally
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, -dirToTarget * moveSpeed * 0.5f, 0.06f);
            }
            else
            {
                // Pace back and forth when at ideal distance
                float paceDir = (float)Math.Sin(Timer * 0.03f);
                NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, paceDir * moveSpeed * 0.4f, 0.05f);
            }
            
            // Apply gravity - ground boss
            NPC.velocity.Y += 0.4f;
            if (NPC.velocity.Y > 12f) NPC.velocity.Y = 12f;
            
            // Simple ground collision
            ApplyGroundCollision();
            
            if (attackCooldown <= 0 && Timer > 50)
            {
                SelectNextAttack(target);
            }
            
            int fireRate = 45 - difficultyTier * 8; // Slower rate = fewer projectiles
            if (Timer % fireRate == 0 && Timer > 30 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                Vector2 vel = toPlayer * (10f + difficultyTier * 1.5f); // Faster
                // Alternate between orb types for variety
                if (Main.rand.NextBool())
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 65 + difficultyTier * 5, EnigmaPurple, 0.025f);
                else
                    BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.8f, 65 + difficultyTier * 5, EnigmaGreen, 14f);
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.4f, 10);
            }
        }
        
        /// <summary>
        /// Simple ground collision for the land-based boss
        /// </summary>
        private void ApplyGroundCollision()
        {
            // Check tiles below the NPC
            int tileX = (int)(NPC.Center.X / 16f);
            int tileYBottom = (int)((NPC.position.Y + NPC.height) / 16f);
            
            // Check a few tiles across the NPC width
            for (int x = -2; x <= 2; x++)
            {
                Tile tile = Framing.GetTileSafely(tileX + x, tileYBottom);
                if (tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType])
                {
                    float groundY = tileYBottom * 16f;
                    if (NPC.position.Y + NPC.height > groundY && NPC.velocity.Y > 0)
                    {
                        NPC.position.Y = groundY - NPC.height;
                        NPC.velocity.Y = 0;
                        onGround = true;
                        return;
                    }
                }
            }
            onGround = false;
        }
        
        private void InitiateTeleport(Player target)
        {
            State = BossPhase.Teleport;
            Timer = 0;
            isFading = true;
            
            // Find a ground position near the player (left or right)
            float xOffset = (Main.rand.NextBool() ? 1 : -1) * (250f + Main.rand.NextFloat(150f));
            Vector2 targetPos = new Vector2(target.Center.X + xOffset, target.Center.Y);
            
            // Find ground below target position
            teleportTarget = FindGroundPosition(targetPos);
        }
        
        /// <summary>
        /// Find a valid ground position for teleportation
        /// </summary>
        private Vector2 FindGroundPosition(Vector2 startPos)
        {
            int tileX = (int)(startPos.X / 16f);
            int startTileY = (int)(startPos.Y / 16f);
            
            // Search downward for ground
            for (int y = startTileY; y < startTileY + 50; y++)
            {
                Tile tile = Framing.GetTileSafely(tileX, y);
                if (tile.HasTile && Main.tileSolid[tile.TileType])
                {
                    // Found ground - position the NPC above it
                    return new Vector2(startPos.X, y * 16f - NPC.height);
                }
            }
            
            // Fallback - just use the start position
            return startPos;
        }
        
        private void SelectNextAttack(Player target)
        {
            List<AttackPattern> pool = new List<AttackPattern>
            {
                AttackPattern.VoidLunge,
                AttackPattern.EyeVolley,
                AttackPattern.ParadoxRing
            };
            
            if (difficultyTier >= 1)
            {
                pool.Add(AttackPattern.ShadowDash);
                pool.Add(AttackPattern.GlyphCircle);
                pool.Add(AttackPattern.TendrilRise);
                pool.Add(AttackPattern.ParadoxWeb); // Bullet-hell web pattern
                pool.Add(AttackPattern.ParadoxJudgment); // Hero's Judgment style
                pool.Add(AttackPattern.VoidLaserWeb); // Crossing laser beams
                
                // New unique attacks (Phase 2)
                pool.Add(AttackPattern.SigilSnare); // Glyphs converge on player
                pool.Add(AttackPattern.VoidBeamPincer); // Orbs on sides shoot beams
                pool.Add(AttackPattern.WatchingGaze); // Eyes track then fire
            }
            
            if (difficultyTier >= 2)
            {
                pool.Add(AttackPattern.RealityFracture);
                pool.Add(AttackPattern.EyeOfTheVoid);
                pool.Add(AttackPattern.RealityZones); // Environment trap
                pool.Add(AttackPattern.EntropicSurge); // Electrical pulses
                
                // New unique attacks (Phase 3)
                pool.Add(AttackPattern.MysteryMaze); // Glyph maze walls
                pool.Add(AttackPattern.ParadoxMirror); // Clone illusions
                
                if (consecutiveAttacks >= 5 && Main.rand.NextBool(3))
                {
                    pool.Add(AttackPattern.UltimateEnigma);
                }
            }
            
            pool.Remove(lastAttack);
            
            CurrentAttack = pool[Main.rand.Next(pool.Count)];
            lastAttack = CurrentAttack;
            
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Attack;
            consecutiveAttacks++;
            
            if (CurrentAttack == AttackPattern.VoidLunge || CurrentAttack == AttackPattern.ShadowDash)
            {
                lungeCount = 0;
            }
        }
        
        private void AI_Attack(Player target)
        {
            switch (CurrentAttack)
            {
                case AttackPattern.VoidLunge:
                    Attack_VoidLunge(target);
                    break;
                case AttackPattern.EyeVolley:
                    Attack_EyeVolley(target);
                    break;
                case AttackPattern.ParadoxRing:
                    Attack_ParadoxRing(target);
                    break;
                case AttackPattern.ShadowDash:
                    Attack_ShadowDash(target);
                    break;
                case AttackPattern.GlyphCircle:
                    Attack_GlyphCircle(target);
                    break;
                case AttackPattern.TendrilRise:
                    Attack_TendrilRise(target);
                    break;
                case AttackPattern.ParadoxWeb:
                    Attack_ParadoxWeb(target);
                    break;
                case AttackPattern.RealityFracture:
                    Attack_RealityFracture(target);
                    break;
                case AttackPattern.EyeOfTheVoid:
                    Attack_EyeOfTheVoid(target);
                    break;
                case AttackPattern.RealityZones:
                    Attack_RealityZones(target);
                    break;
                case AttackPattern.UltimateEnigma:
                    Attack_UltimateEnigma(target);
                    break;
                case AttackPattern.ParadoxJudgment:
                    Attack_ParadoxJudgment(target);
                    break;
                case AttackPattern.VoidLaserWeb:
                    Attack_VoidLaserWeb(target);
                    break;
                case AttackPattern.EntropicSurge:
                    Attack_EntropicSurge(target);
                    break;
                    
                // New unique attacks
                case AttackPattern.SigilSnare:
                    Attack_SigilSnare(target);
                    break;
                case AttackPattern.VoidBeamPincer:
                    Attack_VoidBeamPincer(target);
                    break;
                case AttackPattern.WatchingGaze:
                    Attack_WatchingGaze(target);
                    break;
                case AttackPattern.MysteryMaze:
                    Attack_MysteryMaze(target);
                    break;
                case AttackPattern.ParadoxMirror:
                    Attack_ParadoxMirror(target);
                    break;
            }
        }
        
        private void AI_Teleport(Player target)
        {
            if (isFading)
            {
                NPC.alpha = Math.Min(255, NPC.alpha + 12);
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.5f }, NPC.Center);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 4f);
                }
                
                if (Timer % 3 == 0)
                {
                    Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                    CustomParticles.GenericFlare(pos, EnigmaVoid, 0.4f, 12);
                }
                
                if (NPC.alpha >= 255)
                {
                    NPC.Center = teleportTarget;
                    isFading = false;
                    Timer = 0;
                }
            }
            else
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.2f, Volume = 1.1f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.8f, 18);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 5f);
                    
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 5f;
                        Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * 50f;
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.45f, target.Center);
                    }
                }
                
                NPC.alpha = Math.Max(0, NPC.alpha - 15);
                
                if (NPC.alpha <= 0)
                {
                    isFading = true;
                    Timer = 0;
                    State = BossPhase.Stalking;
                    attackCooldown = AttackWindowFrames / 2;
                }
            }
        }
        
        private void AI_Recovery(Player target)
        {
            NPC.velocity.X *= 0.92f;
            
            // Apply gravity during recovery
            NPC.velocity.Y += 0.4f;
            if (NPC.velocity.Y > 12f) NPC.velocity.Y = 12f;
            ApplyGroundCollision();
            
            if (Timer >= 18)
            {
                Timer = 0;
                State = BossPhase.Stalking;
                attackCooldown = AttackWindowFrames / 2;
            }
        }
        
        private void AI_Enraged(Player target)
        {
            // Ground-based enraged chase - horizontal movement only
            float dirToTarget = Math.Sign(target.Center.X - NPC.Center.X);
            float enrageSpeed = BaseSpeed * 2.2f;
            NPC.velocity.X = MathHelper.Lerp(NPC.velocity.X, dirToTarget * enrageSpeed, 0.12f);
            
            // Apply gravity
            NPC.velocity.Y += 0.4f;
            if (NPC.velocity.Y > 12f) NPC.velocity.Y = 12f;
            ApplyGroundCollision();
            
            int fireRate = 12 - difficultyTier * 2;
            if (Timer % fireRate == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                    Vector2 vel = angle.ToRotationVector2() * 8f;
                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, EnigmaGreen, 0.025f);
                }
                CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.45f, 10);
            }
            
            if (Timer % 3 == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(50f, 50f);
                CustomParticles.GenericFlare(pos, EnigmaGreen, 0.35f, 10);
            }
        }
        
        #endregion
        
        #region Attacks
        
        private void Attack_VoidLunge(Player target)
        {
            int maxLunges = 3 + difficultyTier;
            int telegraphTime = 21 - difficultyTier * 4;
            int lungeTime = 10;
            int recoveryTime = 8 - difficultyTier * 2;
            
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.88f;
                
                if (Timer == 1)
                {
                    lungeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    SoundEngine.PlaySound(SoundID.NPCHit54 with { Pitch = 0.2f }, NPC.Center);
                }
                
                if (Timer > 10)
                {
                    float lineLength = 350f + difficultyTier * 50f;
                    for (int i = 0; i < 15; i++)
                    {
                        float progress = i / 15f;
                        Vector2 linePos = NPC.Center + lungeDirection * lineLength * progress;
                        Color lineColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress) * 0.4f;
                        CustomParticles.GenericFlare(linePos, lineColor, 0.12f, 3);
                    }
                }
                
                if (Timer % 4 == 0)
                {
                    Vector2 chargeOffset = Main.rand.NextVector2CircularEdge(50f, 50f);
                    CustomParticles.GenericFlare(NPC.Center + chargeOffset, EnigmaPurple, 0.3f, 8);
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
                    float lungeSpeed = 30f + difficultyTier * 5f;
                    NPC.velocity = lungeDirection * lungeSpeed;
                    
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Pitch = 0.3f + lungeCount * 0.1f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.7f, 14);
                    CustomParticles.HaloRing(NPC.Center, EnigmaPurple, 0.4f, 10);
                }
                
                if (Timer % 2 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 offset = Main.rand.NextVector2Circular(18f, 18f);
                        Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                        CustomParticles.GenericFlare(NPC.Center + offset - NPC.velocity * 0.25f, trailColor, 0.35f, 10);
                    }
                    
                    CustomParticles.Glyph(NPC.Center - NPC.velocity * 0.3f, EnigmaPurple * 0.6f, 0.25f, -1);
                }
                
                if (Timer >= lungeTime)
                {
                    Timer = 0;
                    lungeCount++;
                    SubPhase = lungeCount >= maxLunges ? 3 : 2;
                }
            }
            else if (SubPhase == 2)
            {
                NPC.velocity *= 0.82f;
                
                if (Timer == 5)
                {
                    Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    float variation = MathHelper.ToRadians(Main.rand.NextFloat(-20f, 20f));
                    lungeDirection = toTarget.RotatedBy(variation);
                }
                
                if (Timer >= recoveryTime)
                {
                    Timer = 0;
                    SubPhase = 0;
                }
            }
            else
            {
                NPC.velocity *= 0.88f;
                if (Timer >= 13)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_EyeVolley(Player target)
        {
            int totalVolleys = 4 + difficultyTier;
            int volleyDelay = 18 - difficultyTier * 3;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase < totalVolleys)
            {
                if (Timer < 12)
                {
                    if (Timer % 3 == 0)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            Vector2 eyePos = NPC.Center + Main.rand.NextVector2CircularEdge(60f, 60f);
                            CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.6f, 0.35f, target.Center);
                        }
                    }
                }
                
                if (Timer == 12 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    float baseAngle = toPlayer.ToRotation();
                    
                    int projCount = 3 + difficultyTier; // Reduced from 5+2
                    float spread = MathHelper.ToRadians(40 + difficultyTier * 8); // Tighter spread
                    
                    for (int i = 0; i < projCount; i++)
                    {
                        float angle = baseAngle - spread / 2f + spread * i / (projCount - 1);
                        float speed = 12f + difficultyTier * 2f; // Faster
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        // Alternate colors and types
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70 + difficultyTier * 5, EnigmaPurple, 0.03f);
                        else
                            BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70 + difficultyTier * 5, EnigmaGreen, 4f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.6f, 15);
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = 0.1f + SubPhase * 0.05f }, NPC.Center);
                }
                
                if (Timer >= volleyDelay)
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
        
        private void Attack_ParadoxRing(Player target)
        {
            int totalRings = 3 + difficultyTier;
            int ringDelay = 30 - difficultyTier * 5;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase < totalRings)
            {
                if (Timer < 18)
                {
                    if (Timer % 3 == 0)
                    {
                        float progress = Timer / 18f;
                        for (int i = 0; i < 6; i++)
                        {
                            float angle = MathHelper.TwoPi * i / 6f + Timer * 0.1f;
                            Vector2 pos = NPC.Center + angle.ToRotationVector2() * (70f - progress * 40f);
                            CustomParticles.GenericFlare(pos, EnigmaPurple, 0.28f + progress * 0.15f, 10);
                        }
                    }
                }
                
                if (Timer == 18 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projCount = 8 + difficultyTier * 2; // Reduced from 12+4
                    float angleOffset = SubPhase * MathHelper.ToRadians(20);
                    
                    for (int i = 0; i < projCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / projCount + angleOffset;
                        float speed = 11f + difficultyTier * 2.5f + SubPhase; // Faster
                        Vector2 vel = angle.ToRotationVector2() * speed;
                        // Mix wave and orb types
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70 + difficultyTier * 5, EnigmaPurple, 4.5f);
                        else
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel * 1.1f, 70 + difficultyTier * 5, EnigmaGreen, 0.015f);
                    }
                    
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.75f, 18);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 5f);
                    for (int r = 0; r < 4; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, EnigmaPurple * (1f - r * 0.2f), 0.3f + r * 0.1f, 12 + r * 2);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.1f + SubPhase * 0.1f }, NPC.Center);
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
        
        private void Attack_ShadowDash(Player target)
        {
            int maxDashes = 3 + difficultyTier;
            int fadeTime = 13 - difficultyTier * 2;
            int dashTime = 11;
            
            if (SubPhase % 3 == 0)
            {
                NPC.alpha = Math.Min(255, NPC.alpha + 15);
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.3f }, NPC.Center);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 5, 4f);
                }
                
                if (Timer % 3 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), EnigmaVoid, 0.35f, 10);
                }
                
                if (NPC.alpha >= 255 || Timer >= fadeTime)
                {
                    Vector2 playerDir = target.velocity.Length() > 3f ? target.velocity.SafeNormalize(Vector2.Zero) : Main.rand.NextVector2Unit();
                    float dashAngle = Main.rand.NextFloat(-0.5f, 0.5f);
                    Vector2 behindPos = target.Center + playerDir.RotatedBy(MathHelper.Pi + dashAngle) * (180f + Main.rand.NextFloat(50f));
                    NPC.Center = behindPos;
                    
                    lungeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (SubPhase % 3 == 1)
            {
                if (Timer == 1)
                {
                    NPC.alpha = 0;
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Pitch = 0.4f, Volume = 1.2f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.85f, 16);
                    
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 4f;
                        Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * 45f;
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.45f, target.Center);
                    }
                    
                    float dashSpeed = 28f + difficultyTier * 5f;
                    NPC.velocity = lungeDirection * dashSpeed;
                }
                
                if (Timer % 2 == 0)
                {
                    Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, Main.rand.NextFloat());
                    CustomParticles.GenericFlare(NPC.Center - NPC.velocity * 0.2f + Main.rand.NextVector2Circular(15f, 15f), trailColor, 0.4f, 10);
                }
                
                if (Timer >= dashTime)
                {
                    Timer = 0;
                    lungeCount++;
                    SubPhase = lungeCount >= maxDashes ? 100 : SubPhase + 1;
                }
            }
            else if (SubPhase % 3 == 2)
            {
                NPC.velocity *= 0.85f;
                
                if (Timer >= 7 - difficultyTier * 1)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                NPC.velocity *= 0.9f;
                if (Timer >= 14)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_GlyphCircle(Player target)
        {
            int circleCount = 2 + difficultyTier;
            int circleDelay = 35 - difficultyTier * 6;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase < circleCount)
            {
                if (Timer < 25)
                {
                    if (Timer % 4 == 0)
                    {
                        int glyphCount = 8 + difficultyTier * 2;
                        float radius = 150f + SubPhase * 50f;
                        for (int i = 0; i < glyphCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / glyphCount + Timer * 0.02f;
                            Vector2 glyphPos = target.Center + angle.ToRotationVector2() * radius;
                            Color glyphColor = EnigmaPurple * (0.3f + Timer / 25f * 0.4f);
                            CustomParticles.Glyph(glyphPos, glyphColor, 0.35f, -1);
                        }
                    }
                }
                
                if (Timer == 25)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f + SubPhase * 0.1f }, target.Center);
                }
                
                if (Timer == 30 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int projCount = 6 + difficultyTier * 2; // Reduced from 10+3
                    float radius = 150f + SubPhase * 50f;
                    
                    for (int i = 0; i < projCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / projCount;
                        Vector2 spawnPos = target.Center + angle.ToRotationVector2() * radius;
                        Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * (13f + difficultyTier * 2.5f); // Faster
                        // Mix types for variety
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 75, EnigmaGreen, 0.025f);
                        else
                            BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.7f, 75, EnigmaPurple, 16f);
                        
                        CustomParticles.Glyph(spawnPos, EnigmaPurple, 0.4f, -1);
                    }
                    
                    CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 8, 6f);
                    MagnumScreenEffects.AddScreenShake(8f);
                }
                
                if (Timer >= circleDelay)
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
        
        private void Attack_TendrilRise(Player target)
        {
            int waveCount = 3 + difficultyTier;
            int waveDelay = 28 - difficultyTier * 4;
            
            NPC.velocity.X *= 0.9f;
            
            if (SubPhase < waveCount)
            {
                if (Timer < 20)
                {
                    if (Timer % 4 == 0)
                    {
                        int markerCount = 8 + difficultyTier * 2;
                        float spread = 500f + difficultyTier * 80f;
                        for (int i = 0; i < markerCount; i++)
                        {
                            float xOffset = -spread / 2f + spread * i / (markerCount - 1);
                            Vector2 groundPos = new Vector2(target.Center.X + xOffset, target.Center.Y + 180f);
                            CustomParticles.GenericFlare(groundPos, EnigmaGreen * (0.3f + Timer / 20f * 0.3f), 0.2f, 4);
                        }
                    }
                }
                
                if (Timer == 20)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, target.Center);
                }
                
                if (Timer == 25 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int tendrilCount = 6 + difficultyTier * 2; // Reduced from 10+3
                    float spread = 450f + difficultyTier * 60f; // Slightly narrower
                    
                    for (int i = 0; i < tendrilCount; i++)
                    {
                        float xOffset = -spread / 2f + spread * i / (tendrilCount - 1) + Main.rand.NextFloat(-20f, 20f);
                        Vector2 spawnPos = new Vector2(target.Center.X + xOffset, target.Center.Y + 220f);
                        Vector2 vel = new Vector2(Main.rand.NextFloat(-1f, 1f), -16f - difficultyTier * 2.5f); // Faster
                        // Alternate types
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel, 75, EnigmaPurple, -8f);
                        else
                            BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel * 0.9f, 75, EnigmaGreen, 5f);
                    }
                    
                    MagnumScreenEffects.AddScreenShake(7f);
                }
                
                if (Timer >= waveDelay)
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
        
        private void Attack_RealityFracture(Player target)
        {
            int chargeTime = 35 - difficultyTier * 6;
            int chaosTime = 84 + difficultyTier * 21;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase == 0)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.3f }, NPC.Center);
                }
                
                float progress = Timer / (float)chargeTime;
                
                if (Timer % 3 == 0)
                {
                    int particleCount = (int)(5 + progress * 10);
                    for (int i = 0; i < particleCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / particleCount + Timer * 0.08f;
                        float radius = 120f * (1f - progress * 0.5f);
                        Vector2 pos = NPC.Center + angle.ToRotationVector2() * radius;
                        Color color = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                        CustomParticles.GenericFlare(pos, color, 0.3f + progress * 0.3f, 10);
                    }
                    
                    CustomParticles.Glyph(NPC.Center + Main.rand.NextVector2Circular(40f, 40f), EnigmaPurple, 0.4f, -1);
                }
                
                if (Timer > chargeTime / 2)
                {
                    MagnumScreenEffects.AddScreenShake(progress * 5f);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    
                    MagnumScreenEffects.AddScreenShake(14f);
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.4f, Volume = 1.4f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 1.3f, 25);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 12, 8f);
                }
            }
            else if (SubPhase == 1)
            {
                int interval = 12 - difficultyTier; // Slower = fewer projectiles
                
                if (Timer % interval == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    int count = 2 + difficultyTier; // Reduced from 3+2
                    for (int i = 0; i < count; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = Main.rand.NextFloat(250f, 400f);
                        Vector2 spawnPos = target.Center + angle.ToRotationVector2() * dist;
                        Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * (12f + difficultyTier * 2.5f); // Faster
                        
                        Color color = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                        // Mix types
                        if (i % 2 == 0)
                            BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, vel * 0.35f, 75, color, 18f);
                        else
                            BossProjectileHelper.SpawnHostileOrb(spawnPos, vel * 0.6f, 75, color, 0.04f);
                        
                        CustomParticles.EnigmaEyeGaze(spawnPos, EnigmaPurple * 0.5f, 0.35f, target.Center);
                    }
                }
                
                if (Timer % 25 == 0 && Main.netMode != NetmodeID.MultiplayerClient) // Slower ring rate
                {
                    int ringCount = 6 + difficultyTier; // Reduced from 8+2
                    for (int i = 0; i < ringCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / ringCount + Timer * 0.02f;
                        Vector2 vel = angle.ToRotationVector2() * 10f; // Faster
                        BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, EnigmaPurple, 4.5f);
                    }
                }
                
                if (Timer >= chaosTime)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_EyeOfTheVoid(Player target)
        {
            int formationTime = 39;
            int fireTime = 49;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase == 0)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = 0.2f }, NPC.Center);
                }
                
                if (Timer % 5 == 0)
                {
                    int eyeCount = 10 + difficultyTier * 4;
                    for (int i = 0; i < eyeCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / eyeCount;
                        float radius = 300f + (float)Math.Sin(Timer * 0.1f + i) * 50f;
                        Vector2 eyePos = target.Center + angle.ToRotationVector2() * radius;
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.5f, target.Center);
                    }
                }
                
                if (Timer >= formationTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    
                    MagnumScreenEffects.AddScreenShake(12f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.3f }, target.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        int projCount = 10 + difficultyTier * 3; // Reduced from 16+6
                        for (int i = 0; i < projCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projCount;
                            Vector2 spawnPos = target.Center + angle.ToRotationVector2() * 320f;
                            Vector2 vel = (target.Center - spawnPos).SafeNormalize(Vector2.Zero) * (15f + difficultyTier * 2.5f); // Faster
                            // Mix types for variety
                            if (i % 2 == 0)
                                BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 80, EnigmaGreen, 0.025f);
                            else
                                BossProjectileHelper.SpawnWaveProjectile(spawnPos, vel, 80, EnigmaPurple, 5f);
                        }
                    }
                }
            }
            else
            {
                if (Timer >= fireTime)
                {
                    EndAttack();
                }
            }
        }
        
        private void Attack_UltimateEnigma(Player target)
        {
            if (SubPhase == 0)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    Main.NewText("T̷H̴E̵ ̴V̸O̷I̸D̵ ̴S̷P̷E̸A̶K̵S̴", EnigmaGreen);
                    SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.5f, Volume = 1.5f }, NPC.Center);
                }
                
                if (Timer % 4 == 0)
                {
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 0.4f + Timer * 0.008f, 12);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 4, 3f + Timer * 0.05f);
                    
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(80f, 80f);
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.5f, target.Center);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(Timer * 0.12f);
                
                if (Timer >= 49)
                {
                    Timer = 0;
                    SubPhase = 1;
                    lungeCount = 0;
                }
            }
            else if (SubPhase <= 4)
            {
                int fadeTime = 8;
                int attackTime = 14;
                
                if (Timer < fadeTime)
                {
                    NPC.alpha = Math.Min(255, (int)(Timer / (float)fadeTime * 255));
                    NPC.velocity *= 0.9f;
                    
                    if (Timer == 1)
                    {
                        CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 5f);
                    }
                }
                else if (Timer == fadeTime)
                {
                    float angle = MathHelper.TwoPi * SubPhase / 5f + Main.rand.NextFloat(-0.3f, 0.3f);
                    Vector2 newPos = target.Center + angle.ToRotationVector2() * (220f + Main.rand.NextFloat(50f));
                    NPC.Center = newPos;
                    
                    NPC.alpha = 0;
                    lungeDirection = (target.Center - NPC.Center).SafeNormalize(Vector2.Zero);
                    
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageHealImpact with { Pitch = 0.5f + SubPhase * 0.1f }, NPC.Center);
                    CustomParticles.GenericFlare(NPC.Center, EnigmaGreen, 1.0f, 18);
                    
                    for (int i = 0; i < 6; i++)
                    {
                        float eyeAngle = MathHelper.TwoPi * i / 6f;
                        Vector2 eyePos = NPC.Center + eyeAngle.ToRotationVector2() * 50f;
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.5f, target.Center);
                    }
                    
                    NPC.velocity = lungeDirection * (35f + SubPhase * 3f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            float projAngle = MathHelper.TwoPi * i / 8f;
                            Vector2 vel = projAngle.ToRotationVector2() * 7f;
                            BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 75, EnigmaPurple, 0.025f);
                        }
                    }
                }
                else if (Timer < fadeTime + attackTime)
                {
                    NPC.velocity *= 0.92f;
                    
                    if (Timer % 2 == 0)
                    {
                        CustomParticles.GenericFlare(NPC.Center - NPC.velocity * 0.15f, EnigmaGreen, 0.4f, 8);
                    }
                }
                else
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else if (SubPhase == 5)
            {
                NPC.velocity *= 0.9f;
                
                if (Timer == 20)
                {
                    MagnumScreenEffects.AddScreenShake(18f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.3f, Volume = 1.5f }, NPC.Center);
                    
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 1.6f, 28);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int ring = 0; ring < 3; ring++)
                        {
                            int projCount = 12 + ring * 4;
                            float speed = 8f + ring * 3f;
                            
                            for (int i = 0; i < projCount; i++)
                            {
                                float angle = MathHelper.TwoPi * i / projCount + ring * MathHelper.ToRadians(15);
                                Vector2 vel = angle.ToRotationVector2() * speed;
                                Color color = ring % 2 == 0 ? EnigmaPurple : EnigmaGreen;
                                BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 75, color, 4f);
                            }
                        }
                    }
                    
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaGreen, 16, 10f);
                    
                    for (int r = 0; r < 10; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(EnigmaPurple, EnigmaGreen, r / 10f), 0.35f + r * 0.15f, 18 + r * 3);
                    }
                }
                
                if (Timer >= 60)
                {
                    consecutiveAttacks = 0;
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// Paradox Web - Symmetric spider web bullet-hell pattern.
        /// Creates expanding strands of projectiles in a web formation.
        /// Players must weave through the gaps between strands.
        /// </summary>
        private void Attack_ParadoxWeb(Player target)
        {
            int strands = 6 + difficultyTier * 2; // Web strands
            int webWaves = 4 + difficultyTier;
            int waveDelay = 25 - difficultyTier * 3;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase < webWaves)
            {
                // Warning buildup - show web pattern
                if (Timer < 15)
                {
                    float progress = Timer / 15f;
                    for (int strand = 0; strand < strands; strand++)
                    {
                        float strandAngle = MathHelper.TwoPi * strand / strands + SubPhase * 0.2f;
                        for (int p = 0; p < 5; p++)
                        {
                            float dist = 30f + p * 25f * progress;
                            Vector2 pos = NPC.Center + strandAngle.ToRotationVector2() * dist;
                            Color warningColor = EnigmaPurple * (0.3f + progress * 0.4f);
                            CustomParticles.GenericFlare(pos, warningColor, 0.12f + progress * 0.1f, 3);
                        }
                    }
                }
                
                // Spawn web pattern
                if (Timer == 15)
                {
                    SoundEngine.PlaySound(SoundID.DD2_SkyDragonsFuryShot with { Pitch = -0.2f + SubPhase * 0.1f }, NPC.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Alternate between different web patterns
                        bool isAlternate = SubPhase % 2 == 1;
                        float angleOffset = isAlternate ? MathHelper.Pi / strands : 0f;
                        
                        // Main web strands
                        for (int strand = 0; strand < strands; strand++)
                        {
                            float strandAngle = MathHelper.TwoPi * strand / strands + angleOffset;
                            
                            // Multiple projectiles per strand for the "web" effect
                            for (int p = 0; p < 3 + difficultyTier; p++)
                            {
                                float speed = 8f + p * 2f + difficultyTier * 1.5f;
                                Vector2 vel = strandAngle.ToRotationVector2() * speed;
                                
                                // Alternate colors for visual variety
                                Color projColor = (strand + p) % 2 == 0 ? EnigmaPurple : EnigmaGreen;
                                
                                if (p % 2 == 0)
                                    BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 70, projColor, 0f);
                                else
                                    BossProjectileHelper.SpawnWaveProjectile(NPC.Center, vel, 70, projColor, 3.5f);
                            }
                        }
                        
                        // Cross-strands (connecting web threads) - fewer than main strands
                        if (SubPhase >= 2)
                        {
                            for (int i = 0; i < strands / 2; i++)
                            {
                                float crossAngle = MathHelper.TwoPi * i / (strands / 2) + angleOffset + MathHelper.Pi / strands;
                                Vector2 vel = crossAngle.ToRotationVector2() * (10f + difficultyTier * 2f);
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel, 65, EnigmaGreen, 12f);
                            }
                        }
                    }
                    
                    // VFX - web burst
                    CustomParticles.GenericFlare(NPC.Center, Color.White, 0.65f, 15);
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 6, 5f);
                    
                    for (int r = 0; r < 4; r++)
                    {
                        CustomParticles.HaloRing(NPC.Center, Color.Lerp(EnigmaPurple, EnigmaGreen, r / 4f), 0.2f + r * 0.08f, 10 + r * 2);
                    }
                    
                    // Eyes watching the player from the center
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(30f, 30f);
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.4f, target.Center);
                    }
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
        
        /// <summary>
        /// Reality Zones - Environment trap attack.
        /// Creates alternating safe/danger zones that shift over time.
        /// Players must recognize the pattern and position themselves correctly.
        /// </summary>
        private void Attack_RealityZones(Player target)
        {
            int warningTime = 25 - difficultyTier * 4;
            int activeTime = 70 + difficultyTier * 21;
            int zoneCycles = 4 + difficultyTier;
            int cycleTime = activeTime / zoneCycles;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase == 0) // Warning phase
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.DD2_EtherianPortalSpawnEnemy with { Pitch = -0.3f, Volume = 1.2f }, NPC.Center);
                    zoneCenter = target.Center; // Lock zone center to player at start
                }
                
                // Show zone pattern preview
                float progress = Timer / (float)warningTime;
                DrawZoneWarning(zoneCenter, 0, progress); // Show first zone pattern
                
                if (Timer >= warningTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    currentZonePattern = 0;
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.0f }, zoneCenter);
                }
            }
            else if (SubPhase == 1) // Active phase - zones shift
            {
                int cycleProgress = Timer % cycleTime;
                int currentCycle = Timer / cycleTime;
                
                // Shift zone pattern at start of each cycle
                if (cycleProgress == 0)
                {
                    currentZonePattern = (currentZonePattern + 1) % 4;
                    SoundEngine.PlaySound(SoundID.DD2_DarkMageCastHeal with { Pitch = 0.2f + currentCycle * 0.1f }, zoneCenter);
                    
                    // Flash effect on pattern shift
                    CustomParticles.GenericFlare(zoneCenter, EnigmaGreen, 0.7f, 15);
                    for (int i = 0; i < 4; i++)
                    {
                        CustomParticles.HaloRing(zoneCenter, EnigmaPurple, 0.3f + i * 0.1f, 10 + i * 2);
                    }
                }
                
                // Draw current danger zones
                DrawActiveZones(zoneCenter, currentZonePattern);
                
                // Spawn projectiles in danger zones periodically
                int spawnInterval = 12 - difficultyTier * 2;
                if (cycleProgress % spawnInterval == spawnInterval / 2 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    SpawnZoneProjectiles(zoneCenter, currentZonePattern, target);
                }
                
                // Eyes watching from zone edges
                if (Timer % 15 == 0)
                {
                    Vector2 eyePos = zoneCenter + Main.rand.NextVector2Circular(150f, 150f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.4f, target.Center);
                }
                
                if (Timer >= activeTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                    
                    // Final zone collapse
                    SoundEngine.PlaySound(SoundID.DD2_ExplosiveTrapExplode with { Pitch = 0.1f, Volume = 1.2f }, zoneCenter);
                    MagnumScreenEffects.AddScreenShake(10f);
                    
                    CustomParticles.GenericFlare(zoneCenter, Color.White, 1.0f, 20);
                    CustomParticles.GlyphBurst(zoneCenter, EnigmaGreen, 12, 8f);
                    
                    // Final projectile burst
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < 10 + difficultyTier * 2; i++)
                        {
                            float angle = MathHelper.TwoPi * i / (10 + difficultyTier * 2);
                            Vector2 vel = angle.ToRotationVector2() * (9f + difficultyTier * 2f);
                            BossProjectileHelper.SpawnHostileOrb(zoneCenter, vel, 70, EnigmaPurple, 0f);
                        }
                    }
                }
            }
            else // Recovery
            {
                if (Timer >= 25)
                {
                    EndAttack();
                }
            }
        }
        
        private Vector2 zoneCenter;
        private int currentZonePattern = 0;
        
        /// <summary>
        /// Draws warning indicator for upcoming zone pattern.
        /// </summary>
        private void DrawZoneWarning(Vector2 center, int pattern, float progress)
        {
            float radius = 200f;
            int segments = 8;
            
            for (int i = 0; i < segments; i++)
            {
                bool isDanger = IsDangerZone(pattern, i, segments);
                float segAngle = MathHelper.TwoPi * i / segments + MathHelper.Pi / segments;
                Vector2 segCenter = center + segAngle.ToRotationVector2() * radius * 0.6f;
                
                Color warningColor = isDanger ? EnigmaGreen * (0.2f + progress * 0.4f) : EnigmaPurple * (0.1f + progress * 0.2f);
                CustomParticles.GenericFlare(segCenter, warningColor, 0.15f + progress * 0.15f, 3);
            }
        }
        
        /// <summary>
        /// Draws active danger zones with particle effects.
        /// </summary>
        private void DrawActiveZones(Vector2 center, int pattern)
        {
            float radius = 200f;
            int segments = 8;
            
            for (int i = 0; i < segments; i++)
            {
                bool isDanger = IsDangerZone(pattern, i, segments);
                if (!isDanger) continue;
                
                float segAngle = MathHelper.TwoPi * i / segments + MathHelper.Pi / segments;
                
                // Draw danger zone particles
                if (Timer % 4 == i % 4)
                {
                    for (int p = 0; p < 3; p++)
                    {
                        float dist = radius * (0.3f + p * 0.25f);
                        Vector2 particlePos = center + segAngle.ToRotationVector2() * dist;
                        particlePos += Main.rand.NextVector2Circular(20f, 20f);
                        CustomParticles.GenericFlare(particlePos, EnigmaGreen, 0.2f, 8);
                    }
                }
            }
        }
        
        /// <summary>
        /// Spawns projectiles in the current danger zones.
        /// </summary>
        private void SpawnZoneProjectiles(Vector2 center, int pattern, Player target)
        {
            float radius = 200f;
            int segments = 8;
            
            for (int i = 0; i < segments; i++)
            {
                bool isDanger = IsDangerZone(pattern, i, segments);
                if (!isDanger) continue;
                
                float segAngle = MathHelper.TwoPi * i / segments + MathHelper.Pi / segments;
                float dist = radius * Main.rand.NextFloat(0.3f, 0.8f);
                Vector2 spawnPos = center + segAngle.ToRotationVector2() * dist;
                
                // Fire projectile toward player
                Vector2 toPlayer = (target.Center - spawnPos).SafeNormalize(Vector2.Zero);
                float speed = 7f + difficultyTier * 1.5f;
                
                Color projColor = i % 2 == 0 ? EnigmaGreen : EnigmaPurple;
                BossProjectileHelper.SpawnHostileOrb(spawnPos, toPlayer * speed, 65, projColor, 0.01f);
                
                CustomParticles.GenericFlare(spawnPos, projColor, 0.3f, 8);
            }
        }
        
        /// <summary>
        /// Determines if a segment is a danger zone based on pattern.
        /// Patterns create different arrangements:
        /// 0 = Alternating (checkerboard)
        /// 1 = Opposite quadrants
        /// 2 = Adjacent pairs
        /// 3 = Every third
        /// </summary>
        private bool IsDangerZone(int pattern, int segment, int totalSegments)
        {
            switch (pattern % 4)
            {
                case 0: // Alternating
                    return segment % 2 == 0;
                case 1: // Opposite quadrants
                    return segment < totalSegments / 2;
                case 2: // Adjacent pairs
                    return (segment / 2) % 2 == 0;
                case 3: // Every third (leaves gaps)
                    return segment % 3 != 0;
                default:
                    return false;
            }
        }
        
        private float[] voidLaserAngles; // For laser web attack
        
        /// <summary>
        /// PARADOX JUDGMENT - Hero's Judgment style spectacle attack for Enigma
        /// Features: Converging void energy, safe zone indicators, multi-wave burst with safe arc
        /// OPTIMIZED: BossVFXOptimizer warnings, reduced particles
        /// </summary>
        private void Attack_ParadoxJudgment(Player target)
        {
            // DIFFICULTY: Shorter charge, more waves, faster
            int chargeTime = 38 - difficultyTier * 4; // Shorter (was 72-8)
            int waveCount = 4 + difficultyTier; // More waves (was 3+)
            
            if (SubPhase == 0)
            {
                // === CHARGE PHASE: Converging void particles ===
                NPC.velocity *= 0.95f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item8 with { Pitch = -0.4f, Volume = 1.3f }, NPC.Center);
                    Main.NewText("WITNESS THE UNKNOWABLE!", EnigmaPurple);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // OPTIMIZED: Use BossVFXOptimizer for converging ring - reduced frequency
                if (Timer % 5 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 260f, progress, EnigmaPurple, 7);
                    
                    // Occasional glyphs for mystery theme (reduced)
                    if (Timer % 10 == 0)
                    {
                        CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple * 0.8f, 4, 180f * (1f - progress * 0.5f), 0.03f);
                    }
                }
                
                // OPTIMIZED: Watching eyes - less frequent
                if (Timer % 12 == 0)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        float angle = Main.rand.NextFloat(MathHelper.TwoPi);
                        float dist = 180f - progress * 80f;
                        Vector2 eyePos = NPC.Center + angle.ToRotationVector2() * dist;
                        CustomParticles.EnigmaEyeImpact(eyePos, NPC.Center, EnigmaPurple, 0.3f);
                    }
                }
                
                // SAFE ZONE INDICATOR - earlier, use optimizer
                if (Timer > chargeTime / 3)
                {
                    BossVFXOptimizer.SafeZoneRing(target.Center, 105f, 10);
                    
                    // Show safe arc from boss
                    float safeAngle = (target.Center - NPC.Center).ToRotation();
                    BossVFXOptimizer.SafeArcIndicator(NPC.Center, safeAngle, MathHelper.ToRadians(28f), 160f, 6);
                }
                
                // Screen shake builds at 60%+
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
                    MagnumScreenEffects.AddScreenShake(16f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.1f + SubPhase * 0.1f, Volume = 1.5f }, NPC.Center);
                    
                    // OPTIMIZED: Use BossVFXOptimizer for release burst
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EnigmaPurple, EnigmaGreen, 1.2f);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster, tighter safe arc
                        int projectileCount = 34 + difficultyTier * 8; // More (was 26+6)
                        float safeAngle = (target.Center - NPC.Center).ToRotation();
                        float safeArc = MathHelper.ToRadians(26f - difficultyTier * 2f); // Tighter (was 35f)
                        
                        for (int i = 0; i < projectileCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / projectileCount;
                            
                            // SAFE ARC EXEMPTION
                            float angleDiff = MathHelper.WrapAngle(angle - safeAngle);
                            if (Math.Abs(angleDiff) < safeArc) continue;
                            
                            // DIFFICULTY: Much faster projectiles
                            float speed = 14f + difficultyTier * 3f + SubPhase * 2f; // Faster (was 10f+2f+1.5f)
                            Vector2 vel = angle.ToRotationVector2() * speed;
                            
                            // Mix projectile types for difficulty
                            Color projColor = i % 2 == 0 ? EnigmaPurple : EnigmaGreen;
                            if (i % 4 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.7f, 95, projColor, 18f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 95, projColor, i % 3 == 0 ? 0.02f : 0f);
                        }
                    }
                    
                    // OPTIMIZED: Cascading halos
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, EnigmaPurple, EnigmaGreen, 6, 0.35f, 16);
                    
                    // Glyph burst (reduced)
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaPurple, 5, 5f);
                }
                
                // DIFFICULTY: Faster waves (36 frames instead of 48)
                if (Timer >= 25)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 20) // Shorter recovery (was 36)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// VOID LASER WEB - Spider web pattern of crossing laser beams
        /// Features: Warning indicators, sweeping void beams in web pattern
        /// OPTIMIZED: BossVFXOptimizer warnings, reduced particles
        /// </summary>
        private void Attack_VoidLaserWeb(Player target)
        {
            // DIFFICULTY: More lasers, faster sweep
            int laserCount = 7 + difficultyTier * 2; // More lasers (was 6+2)
            int sweepDuration = 50 - difficultyTier * 7; // Faster (was 96-12)
            
            if (SubPhase == 0)
            {
                // === TELEGRAPH PHASE ===
                NPC.velocity *= 0.92f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item15 with { Pitch = 0.1f }, NPC.Center);
                    voidLaserAngles = new float[laserCount];
                    for (int i = 0; i < laserCount; i++)
                    {
                        voidLaserAngles[i] = MathHelper.TwoPi * i / laserCount;
                    }
                }
                
                float progress = Timer / 25f; // Shorter telegraph (was 48)
                
                // OPTIMIZED: Use LaserBeamWarning for clear telegraph
                if (Timer % 4 == 0 && Timer < 25)
                {
                    for (int i = 0; i < laserCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / laserCount + Timer * 0.02f;
                        BossVFXOptimizer.LaserBeamWarning(NPC.Center, angle, 1500f, progress);
                    }
                }
                
                // OPTIMIZED: Gathering void energy - reduced frequency
                if (Timer % 8 == 0)
                {
                    BossVFXOptimizer.ConvergingWarning(NPC.Center, 90f, progress, EnigmaPurple, 5);
                }
                
                if (Timer >= 25) // Shorter (was 48)
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
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0f, Volume = 1.4f }, NPC.Center);
                    MagnumScreenEffects.AddScreenShake(12f);
                }
                
                float sweepProgress = Timer / (float)sweepDuration;
                float sweepAngle = sweepProgress * MathHelper.Pi * 1.2f; // Wider sweep (was Pi)
                
                // OPTIMIZED: Draw laser beams with reduced particles
                for (int i = 0; i < laserCount; i++)
                {
                    float currentAngle = voidLaserAngles[i] + sweepAngle * (i % 2 == 0 ? 1 : -1);
                    
                    // Visual laser beam - draw less frequently
                    if (Timer % 2 == 0)
                    {
                        Vector2 beamEnd = NPC.Center + currentAngle.ToRotationVector2() * 1800f;
                        MagnumVFX.DrawEnigmaLightning(NPC.Center, beamEnd, 12, 22f, 3, 0.6f);
                    }
                    
                    // OPTIMIZED: Reduced particle trail
                    if (Timer % 3 == 0)
                    {
                        for (int j = 0; j < 10; j++)
                        {
                            float dist = 100f + j * 160f;
                            Vector2 beamPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                            Color beamColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)j / 10f);
                            BossVFXOptimizer.OptimizedFlare(beamPos, beamColor, 0.25f, 4);
                        }
                    }
                    
                    // DIFFICULTY: More frequent projectiles, faster (6 frames)
                    if (Timer % 6 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int p = 0; p < 4 + difficultyTier; p++) // More projectiles
                        {
                            float dist = 150f + p * 200f;
                            Vector2 projPos = NPC.Center + currentAngle.ToRotationVector2() * dist;
                            float projSpeed = 6f + difficultyTier * 2f; // Faster
                            Vector2 projVel = currentAngle.ToRotationVector2() * projSpeed;
                            
                            if (p % 2 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(projPos, projVel, 90, EnigmaPurple, 14f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(projPos, projVel, 90, EnigmaGreen, 0.025f);
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
                if (Timer >= 14) // Shorter recovery (was 30)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// ENTROPIC SURGE - Void energy pulse waves expanding outward
        /// Features: Building entropy, expanding void rings, arcane lightning
        /// OPTIMIZED: BossVFXOptimizer, reduced particles
        /// </summary>
        private void Attack_EntropicSurge(Player target)
        {
            // DIFFICULTY: More waves, faster
            int surgeWaves = 6 + difficultyTier; // More waves (was 5+)
            int chargeTime = 25; // Shorter charge (was 48)
            int waveDelay = 11; // Much faster waves (was 24)
            
            if (SubPhase == 0)
            {
                // === ENTROPY BUILDUP ===
                NPC.velocity *= 0.9f;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item93 with { Pitch = -0.4f }, NPC.Center);
                }
                
                float progress = Timer / (float)chargeTime;
                
                // OPTIMIZED: Use ElectricalBuildupWarning
                if (Timer % 5 == 0)
                {
                    BossVFXOptimizer.ElectricalBuildupWarning(NPC.Center, EnigmaPurple, 200f * (1f - progress * 0.5f), progress);
                }
                
                // OPTIMIZED: Reduced charge VFX frequency
                if (Timer % 6 == 0)
                {
                    BossVFXOptimizer.OptimizedRadialFlares(NPC.Center, Color.Lerp(EnigmaPurple, EnigmaGreen, progress),
                        5, 55f - progress * 30f, 0.35f + progress * 0.3f, 10);
                    
                    // Glyph circle forming (less frequent)
                    if (Timer % 12 == 0)
                    {
                        CustomParticles.GlyphCircle(NPC.Center, EnigmaPurple, 4, 50f - progress * 20f, 0.03f);
                    }
                }
                
                // Show danger zone expanding
                if (Timer > chargeTime / 2)
                {
                    BossVFXOptimizer.DangerZoneRing(NPC.Center, 80f + progress * 220f, 10);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase <= surgeWaves)
            {
                // === SURGE WAVE RELEASE ===
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(10f + SubPhase * 2f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f + SubPhase * 0.08f, Volume = 1.3f }, NPC.Center);
                    
                    // OPTIMIZED: Use BossVFXOptimizer burst
                    BossVFXOptimizer.AttackReleaseBurst(NPC.Center, EnigmaPurple, EnigmaGreen, 1.0f);
                    
                    // Spawn expanding ring
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // DIFFICULTY: More projectiles, faster
                        int ringCount = 22 + difficultyTier * 5; // More (was 18+4)
                        float baseSpeed = 12f + SubPhase * 2.5f + difficultyTier * 2f; // Much faster (was 8f+2f+1.5f)
                        
                        for (int i = 0; i < ringCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / ringCount;
                            Vector2 vel = angle.ToRotationVector2() * baseSpeed;
                            
                            Color projColor = i % 2 == 0 ? EnigmaPurple : EnigmaGreen;
                            
                            // Mix projectile types for difficulty
                            if (i % 4 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(NPC.Center, vel * 0.6f, 90 + difficultyTier * 5, projColor, 16f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(NPC.Center, vel, 90 + difficultyTier * 5, projColor, i % 3 == 0 ? 0.018f : 0f);
                        }
                    }
                    
                    // OPTIMIZED: Fewer lightning arcs
                    for (int i = 0; i < 5; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 5f + Main.rand.NextFloat(0.25f);
                        Vector2 lightningEnd = NPC.Center + angle.ToRotationVector2() * (160f + SubPhase * 40f);
                        MagnumVFX.DrawEnigmaLightning(NPC.Center, lightningEnd, 8, 28f, 3, 0.7f);
                    }
                    
                    // OPTIMIZED: Cascading halos
                    BossVFXOptimizer.OptimizedCascadingHalos(NPC.Center, EnigmaPurple, EnigmaGreen, 5, 0.35f, 14);
                    
                    // Glyph burst (reduced)
                    CustomParticles.GlyphBurst(NPC.Center, EnigmaGreen, 4, 4f);
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
        
        /// <summary>
        /// SIGIL SNARE - Glyphs spawn in a ring around the player, then converge inward
        /// The player must escape before the glyphs close in and fire projectiles
        /// </summary>
        private void Attack_SigilSnare(Player target)
        {
            int chargeTime = 55 - difficultyTier * 8;
            int convergeTime = 40 - difficultyTier * 5;
            int glyphCount = 8 + difficultyTier * 2;
            float initialRadius = 280f - difficultyTier * 20f;
            float finalRadius = 50f;
            
            NPC.velocity *= 0.92f;
            
            if (SubPhase == 0)
            {
                // Phase 0: Spawn glyphs in ring around player (telegraph)
                float progress = Timer / (float)chargeTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item123 with { Pitch = 0.3f }, target.Center);
                    Main.NewText("The sigils converge...", EnigmaPurple);
                }
                
                // Spawn and intensify glyphs
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount + Timer * 0.01f;
                        Vector2 glyphPos = target.Center + angle.ToRotationVector2() * initialRadius;
                        Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)i / glyphCount) * (0.3f + progress * 0.5f);
                        CustomParticles.Glyph(glyphPos, glyphColor, 0.4f + progress * 0.2f, -1);
                        
                        // Eyes watching the player from glyph positions
                        if (Timer % 12 == 0)
                        {
                            CustomParticles.EnigmaEyeGaze(glyphPos + Main.rand.NextVector2Circular(15f, 15f), 
                                EnigmaGreen * 0.6f, 0.3f, target.Center);
                        }
                    }
                }
                
                // Warning particles converging slowly
                if (Timer % 6 == 0)
                {
                    BossVFXOptimizer.DangerZoneRing(target.Center, initialRadius * (1f - progress * 0.3f), glyphCount);
                    BossVFXOptimizer.SafeZoneRing(target.Center, finalRadius, 6);
                }
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Phase 1: Glyphs converge toward center (danger!)
                float progress = Timer / (float)convergeTime;
                float currentRadius = MathHelper.Lerp(initialRadius, finalRadius, progress);
                
                // Draw converging glyphs
                if (Timer % 3 == 0)
                {
                    for (int i = 0; i < glyphCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / glyphCount + Timer * 0.015f;
                        Vector2 glyphPos = target.Center + angle.ToRotationVector2() * currentRadius;
                        Color glyphColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                        CustomParticles.Glyph(glyphPos, glyphColor, 0.5f, -1);
                        
                        // Trailing particles as they move
                        CustomParticles.GenericFlare(glyphPos, EnigmaPurple * 0.5f, 0.25f, 8);
                    }
                    
                    // Shrinking danger ring
                    BossVFXOptimizer.DangerZoneRing(target.Center, currentRadius, glyphCount);
                }
                
                MagnumScreenEffects.AddScreenShake(2f + progress * 4f);
                
                if (Timer >= convergeTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                // Phase 2: Glyphs fire projectiles inward at player's last position
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(12f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 1.3f }, target.Center);
                    
                    // Big VFX burst
                    BossVFXOptimizer.AttackReleaseBurst(target.Center, EnigmaPurple, EnigmaGreen, 1.2f);
                    
                    // Spawn projectiles from glyph positions
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < glyphCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / glyphCount;
                            Vector2 spawnPos = target.Center + angle.ToRotationVector2() * finalRadius;
                            Vector2 toCenter = (target.Center - spawnPos).SafeNormalize(Vector2.Zero);
                            
                            // Fire toward center with varying speeds
                            float speed = 14f + difficultyTier * 3f + Main.rand.NextFloat(-2f, 2f);
                            BossProjectileHelper.SpawnAcceleratingBolt(spawnPos, toCenter * speed, 80 + difficultyTier * 5, EnigmaPurple, 12f);
                            
                            // Secondary homing projectile
                            if (i % 2 == 0)
                            {
                                BossProjectileHelper.SpawnHostileOrb(spawnPos, toCenter * (speed * 0.7f), 75, EnigmaGreen, 0.03f);
                            }
                        }
                    }
                    
                    // Cascading glyph burst
                    CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 12, 8f);
                    CustomParticles.GlyphBurst(target.Center, EnigmaGreen, 8, 5f);
                    
                    // Eyes scatter
                    for (int i = 0; i < 6; i++)
                    {
                        Vector2 eyePos = target.Center + Main.rand.NextVector2Circular(60f, 60f);
                        CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen, 0.4f, eyePos + Main.rand.NextVector2Unit() * 100f);
                    }
                }
                
                if (Timer >= 35)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// VOID BEAM PINCER - Orbs spawn on either side of player with swirl animation,
        /// then shoot beams that the player must dodge
        /// </summary>
        private void Attack_VoidBeamPincer(Player target)
        {
            int orbSpawnTime = 45 - difficultyTier * 6;
            int chargeTime = 35 - difficultyTier * 5;
            int beamDuration = 50 + difficultyTier * 10;
            int orbCount = 2 + (difficultyTier >= 2 ? 1 : 0); // 2 or 3 orbs at higher difficulty
            float orbDistance = 350f - difficultyTier * 30f;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase == 0)
            {
                // Phase 0: Orbs spawn with swirl animation on player's sides
                float spawnProgress = Timer / (float)orbSpawnTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.3f }, target.Center);
                }
                
                // Swirling spawn animation for each orb
                for (int orb = 0; orb < orbCount; orb++)
                {
                    float orbAngle;
                    if (orbCount == 2)
                    {
                        // Two orbs: left and right
                        orbAngle = orb == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                    }
                    else
                    {
                        // Three orbs: left, right, and above
                        orbAngle = orb == 0 ? MathHelper.PiOver2 : (orb == 1 ? -MathHelper.PiOver2 : MathHelper.Pi);
                    }
                    
                    // Swirl-in animation - particles spiral inward to form orb
                    if (Timer % 3 == 0)
                    {
                        Vector2 orbTargetPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                        float swirlRadius = 60f * (1f - spawnProgress);
                        
                        for (int p = 0; p < 4; p++)
                        {
                            float swirlAngle = Timer * 0.15f + MathHelper.TwoPi * p / 4f + orb * MathHelper.PiOver2;
                            Vector2 particlePos = orbTargetPos + swirlAngle.ToRotationVector2() * swirlRadius;
                            Vector2 toCenter = (orbTargetPos - particlePos).SafeNormalize(Vector2.Zero) * 3f;
                            
                            Color swirlColor = Color.Lerp(EnigmaPurple, EnigmaGreen, (float)p / 4f) * (0.4f + spawnProgress * 0.4f);
                            CustomParticles.GenericFlare(particlePos, swirlColor, 0.3f + spawnProgress * 0.2f, 10);
                            
                            // Glyphs forming at orb center
                            if (p == 0 && spawnProgress > 0.5f)
                            {
                                CustomParticles.Glyph(orbTargetPos + Main.rand.NextVector2Circular(15f, 15f), EnigmaPurple, 0.35f, -1);
                            }
                        }
                        
                        // Growing core at orb position
                        CustomParticles.GenericFlare(orbTargetPos, EnigmaGreen * spawnProgress, 0.2f + spawnProgress * 0.4f, 5);
                    }
                    
                    // Warning line showing beam direction
                    if (Timer > orbSpawnTime * 0.6f && Timer % 4 == 0)
                    {
                        Vector2 orbTargetPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                        BossVFXOptimizer.LaserBeamWarning(orbTargetPos, (target.Center - orbTargetPos).ToRotation(), orbDistance * 2f, spawnProgress);
                    }
                }
                
                if (Timer >= orbSpawnTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Phase 1: Orbs charge up (player has time to move)
                float chargeProgress = Timer / (float)chargeTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = -0.2f }, target.Center);
                }
                
                // Intensifying charge VFX at each orb
                for (int orb = 0; orb < orbCount; orb++)
                {
                    float orbAngle;
                    if (orbCount == 2)
                    {
                        orbAngle = orb == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                    }
                    else
                    {
                        orbAngle = orb == 0 ? MathHelper.PiOver2 : (orb == 1 ? -MathHelper.PiOver2 : MathHelper.Pi);
                    }
                    
                    Vector2 orbPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                    
                    if (Timer % 3 == 0)
                    {
                        // Converging charge particles
                        BossVFXOptimizer.ConvergingWarning(orbPos, 40f, chargeProgress, EnigmaPurple, 6);
                        
                        // Growing orb glow
                        CustomParticles.GenericFlare(orbPos, EnigmaPurple, 0.4f + chargeProgress * 0.4f, 8);
                        CustomParticles.GenericFlare(orbPos, EnigmaGreen, 0.25f + chargeProgress * 0.3f, 6);
                        
                        // Warning beam line intensifying
                        BossVFXOptimizer.LaserBeamWarning(orbPos, (target.Center - orbPos).ToRotation(), orbDistance * 2f, chargeProgress);
                    }
                    
                    // Eyes gathering around orbs
                    if (Timer % 8 == 0)
                    {
                        CustomParticles.EnigmaEyeGaze(orbPos + Main.rand.NextVector2Circular(25f, 25f), EnigmaGreen * 0.7f, 0.3f, target.Center);
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(chargeProgress * 3f);
                
                if (Timer >= chargeTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                // Phase 2: Beams fire! Player must not be in the beam path
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(15f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.5f, Volume = 1.5f }, target.Center);
                    
                    // Spawn beam projectiles from each orb
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int orb = 0; orb < orbCount; orb++)
                        {
                            float orbAngle;
                            if (orbCount == 2)
                            {
                                orbAngle = orb == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                            }
                            else
                            {
                                orbAngle = orb == 0 ? MathHelper.PiOver2 : (orb == 1 ? -MathHelper.PiOver2 : MathHelper.Pi);
                            }
                            
                            Vector2 orbPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                            Vector2 beamDir = (target.Center - orbPos).SafeNormalize(Vector2.Zero);
                            
                            // Spawn the beam projectile
                            int beamType = ModContent.ProjectileType<EnigmaVoidBeam>();
                            Projectile.NewProjectile(NPC.GetSource_FromAI(), orbPos, Vector2.Zero, beamType,
                                85 + difficultyTier * 8, 0f, Main.myPlayer, beamDir.ToRotation(), beamDuration);
                        }
                    }
                    
                    // VFX burst at each orb
                    for (int orb = 0; orb < orbCount; orb++)
                    {
                        float orbAngle;
                        if (orbCount == 2)
                        {
                            orbAngle = orb == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                        }
                        else
                        {
                            orbAngle = orb == 0 ? MathHelper.PiOver2 : (orb == 1 ? -MathHelper.PiOver2 : MathHelper.Pi);
                        }
                        
                        Vector2 orbPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                        BossVFXOptimizer.AttackReleaseBurst(orbPos, EnigmaPurple, EnigmaGreen, 1.0f);
                        CustomParticles.GlyphBurst(orbPos, EnigmaGreen, 6, 4f);
                    }
                }
                
                // Beam active VFX
                if (Timer % 4 == 0 && Timer < beamDuration)
                {
                    for (int orb = 0; orb < orbCount; orb++)
                    {
                        float orbAngle;
                        if (orbCount == 2)
                        {
                            orbAngle = orb == 0 ? MathHelper.PiOver2 : -MathHelper.PiOver2;
                        }
                        else
                        {
                            orbAngle = orb == 0 ? MathHelper.PiOver2 : (orb == 1 ? -MathHelper.PiOver2 : MathHelper.Pi);
                        }
                        
                        Vector2 orbPos = target.Center + new Vector2((float)Math.Cos(orbAngle), (float)Math.Sin(orbAngle)) * orbDistance;
                        CustomParticles.GenericFlare(orbPos, EnigmaGreen, 0.5f, 5);
                    }
                }
                
                if (Timer >= beamDuration + 20)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// WATCHING GAZE - Eyes spawn in formation around player, track for a moment, then fire
        /// </summary>
        private void Attack_WatchingGaze(Player target)
        {
            int eyeCount = 6 + difficultyTier * 2;
            int spawnTime = 30 - difficultyTier * 4;
            int trackTime = 50 - difficultyTier * 8;
            int waves = 2 + difficultyTier;
            float eyeRadius = 250f - difficultyTier * 20f;
            
            NPC.velocity *= 0.9f;
            
            if (SubPhase < waves)
            {
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item103 with { Pitch = -0.3f }, NPC.Center);
                    if (SubPhase == 0)
                    {
                        Main.NewText("The void watches...", EnigmaGreen);
                    }
                }
                
                if (Timer <= spawnTime)
                {
                    // Eyes spawn one by one with dramatic effect
                    float spawnProgress = Timer / (float)spawnTime;
                    int eyesToShow = (int)(eyeCount * spawnProgress);
                    
                    if (Timer % 4 == 0)
                    {
                        for (int i = 0; i < eyesToShow; i++)
                        {
                            float angle = MathHelper.TwoPi * i / eyeCount + SubPhase * MathHelper.PiOver4;
                            Vector2 eyePos = target.Center + angle.ToRotationVector2() * eyeRadius;
                            
                            // Eye spawn VFX
                            CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.45f, target.Center);
                            
                            // Purple void swirl around new eyes
                            if (i == eyesToShow - 1)
                            {
                                for (int s = 0; s < 4; s++)
                                {
                                    float swirlAngle = Timer * 0.2f + MathHelper.TwoPi * s / 4f;
                                    Vector2 swirlPos = eyePos + swirlAngle.ToRotationVector2() * 20f;
                                    CustomParticles.GenericFlare(swirlPos, EnigmaPurple * 0.6f, 0.2f, 8);
                                }
                            }
                        }
                    }
                }
                else if (Timer <= spawnTime + trackTime)
                {
                    // Eyes track player - draw them looking at player
                    float trackProgress = (Timer - spawnTime) / (float)trackTime;
                    
                    if (Timer % 3 == 0)
                    {
                        for (int i = 0; i < eyeCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / eyeCount + SubPhase * MathHelper.PiOver4;
                            Vector2 eyePos = target.Center + angle.ToRotationVector2() * eyeRadius;
                            
                            // Eyes intensify as they prepare to fire
                            CustomParticles.EnigmaEyeGaze(eyePos, Color.Lerp(EnigmaPurple, EnigmaGreen, trackProgress), 
                                0.4f + trackProgress * 0.15f, target.Center);
                            
                            // Warning line from eye to player
                            if (trackProgress > 0.5f)
                            {
                                BossVFXOptimizer.WarningLine(eyePos, (target.Center - eyePos).SafeNormalize(Vector2.Zero), 
                                    eyeRadius, 6, WarningType.Danger);
                            }
                        }
                    }
                    
                    // Tension building
                    if (trackProgress > 0.7f)
                    {
                        MagnumScreenEffects.AddScreenShake(trackProgress * 2f);
                    }
                }
                else if (Timer == spawnTime + trackTime + 1)
                {
                    // FIRE!
                    MagnumScreenEffects.AddScreenShake(10f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.3f, Volume = 1.2f }, target.Center);
                    
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        for (int i = 0; i < eyeCount; i++)
                        {
                            float angle = MathHelper.TwoPi * i / eyeCount + SubPhase * MathHelper.PiOver4;
                            Vector2 eyePos = target.Center + angle.ToRotationVector2() * eyeRadius;
                            Vector2 toTarget = (target.Center - eyePos).SafeNormalize(Vector2.Zero);
                            
                            float speed = 16f + difficultyTier * 3f;
                            
                            // Mix projectile types
                            if (i % 3 == 0)
                                BossProjectileHelper.SpawnAcceleratingBolt(eyePos, toTarget * speed * 0.7f, 75 + difficultyTier * 5, EnigmaGreen, 18f);
                            else
                                BossProjectileHelper.SpawnHostileOrb(eyePos, toTarget * speed, 75 + difficultyTier * 5, EnigmaPurple, 0.02f);
                            
                            // Eye death burst
                            CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen, 0.5f, toTarget);
                            CustomParticles.GenericFlare(eyePos, EnigmaPurple, 0.4f, 12);
                        }
                    }
                    
                    // Central burst
                    CustomParticles.GlyphBurst(target.Center, EnigmaPurple, 8, 5f);
                    BossVFXOptimizer.OptimizedCascadingHalos(target.Center, EnigmaPurple, EnigmaGreen, 4, 0.3f, 12);
                }
                
                if (Timer >= spawnTime + trackTime + 25)
                {
                    Timer = 0;
                    SubPhase++;
                }
            }
            else
            {
                if (Timer >= 30)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// MYSTERY MAZE - Creates a maze of glyph walls that the player must navigate
        /// Walls deal damage on contact
        /// </summary>
        private void Attack_MysteryMaze(Player target)
        {
            int wallCount = 4 + difficultyTier;
            int buildTime = 50 - difficultyTier * 8;
            int activeDuration = 180 + difficultyTier * 30;
            float mazeRadius = 400f;
            
            NPC.velocity *= 0.85f;
            
            if (SubPhase == 0)
            {
                // Phase 0: Build maze walls around player
                float buildProgress = Timer / (float)buildTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item123 with { Pitch = 0.1f }, target.Center);
                    Main.NewText("Navigate the mystery...", EnigmaPurple);
                }
                
                // Spawn wall projectiles progressively
                if (Timer % (buildTime / wallCount) == 0 && Timer > 0)
                {
                    int wallIndex = Timer / (buildTime / wallCount) - 1;
                    if (wallIndex < wallCount && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        // Create different wall configurations
                        float baseAngle = MathHelper.TwoPi * wallIndex / wallCount;
                        float offset = Main.rand.NextFloat(-0.2f, 0.2f);
                        
                        // Spawn a glyph wall segment
                        Vector2 wallStart = target.Center + (baseAngle + offset).ToRotationVector2() * (mazeRadius * 0.4f);
                        Vector2 wallEnd = target.Center + (baseAngle + offset).ToRotationVector2() * mazeRadius;
                        
                        int wallType = ModContent.ProjectileType<EnigmaMazeWall>();
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), wallStart, Vector2.Zero, wallType,
                            70 + difficultyTier * 5, 0f, Main.myPlayer, (wallEnd - wallStart).ToRotation(), activeDuration + buildTime - Timer);
                        
                        // Spawn VFX for wall creation
                        for (float t = 0; t <= 1f; t += 0.15f)
                        {
                            Vector2 pos = Vector2.Lerp(wallStart, wallEnd, t);
                            CustomParticles.Glyph(pos, EnigmaPurple, 0.4f, -1);
                            CustomParticles.GenericFlare(pos, EnigmaGreen * 0.5f, 0.25f, 10);
                        }
                        
                        SoundEngine.PlaySound(SoundID.Item100 with { Pitch = 0.2f + wallIndex * 0.1f, Volume = 0.6f }, wallStart);
                    }
                }
                
                // Ambient maze particles
                if (Timer % 5 == 0)
                {
                    BossVFXOptimizer.DangerZoneRing(target.Center, mazeRadius, 12);
                }
                
                if (Timer >= buildTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                }
            }
            else if (SubPhase == 1)
            {
                // Phase 1: Maze is active - spawn additional hazards inside
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.2f }, target.Center);
                }
                
                // Spawn wandering eye projectiles inside maze
                if (Timer % (45 - difficultyTier * 10) == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 spawnPos = target.Center + Main.rand.NextVector2Circular(mazeRadius * 0.6f, mazeRadius * 0.6f);
                    Vector2 vel = Main.rand.NextVector2Unit() * (8f + difficultyTier * 2f);
                    BossProjectileHelper.SpawnHostileOrb(spawnPos, vel, 70, EnigmaGreen, 0.015f);
                    
                    CustomParticles.EnigmaEyeGaze(spawnPos, EnigmaPurple, 0.35f, target.Center);
                }
                
                // Ambient glyph effects
                if (Timer % 8 == 0)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        Vector2 glyphPos = target.Center + Main.rand.NextVector2Circular(mazeRadius * 0.8f, mazeRadius * 0.8f);
                        CustomParticles.Glyph(glyphPos, EnigmaPurple * 0.4f, 0.25f, -1);
                    }
                }
                
                if (Timer >= activeDuration)
                {
                    EndAttack();
                }
            }
        }
        
        /// <summary>
        /// PARADOX MIRROR - Boss creates clone illusions with glyph circles
        /// Only one position is the "real" danger zone
        /// </summary>
        private void Attack_ParadoxMirror(Player target)
        {
            int cloneCount = 3 + difficultyTier;
            int setupTime = 40 - difficultyTier * 5;
            int fakeoutTime = 60 - difficultyTier * 10;
            int attackTime = 30;
            float cloneRadius = 350f;
            
            NPC.velocity *= 0.85f;
            
            if (SubPhase == 0)
            {
                // Phase 0: Boss teleports to center, clones appear around player
                float setupProgress = Timer / (float)setupTime;
                
                if (Timer == 1)
                {
                    SoundEngine.PlaySound(SoundID.Item117 with { Pitch = -0.2f }, target.Center);
                    Main.NewText("Which is real...?", EnigmaGreen);
                    
                    // Fade out for teleport
                    NPC.alpha = 200;
                }
                
                // Spawn clone illusions
                if (Timer % 5 == 0)
                {
                    for (int i = 0; i < cloneCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / cloneCount;
                        Vector2 clonePos = target.Center + angle.ToRotationVector2() * cloneRadius;
                        
                        // Swirl-in animation for each clone
                        float swirlRadius = 40f * (1f - setupProgress);
                        for (int s = 0; s < 3; s++)
                        {
                            float swirlAngle = Timer * 0.15f + MathHelper.TwoPi * s / 3f;
                            Vector2 swirlPos = clonePos + swirlAngle.ToRotationVector2() * swirlRadius;
                            CustomParticles.GenericFlare(swirlPos, EnigmaPurple * (0.4f + setupProgress * 0.3f), 0.25f, 8);
                        }
                        
                        // Growing clone silhouette
                        CustomParticles.GenericFlare(clonePos, EnigmaGreen * setupProgress, 0.3f + setupProgress * 0.3f, 6);
                        
                        // Glyph circles forming around each clone
                        if (setupProgress > 0.5f)
                        {
                            CustomParticles.Glyph(clonePos + Main.rand.NextVector2Circular(25f, 25f), EnigmaPurple, 0.3f, -1);
                        }
                    }
                }
                
                // Boss appears to be at all positions
                NPC.alpha = (int)(255 * (1f - setupProgress * 0.5f));
                
                if (Timer >= setupTime)
                {
                    Timer = 0;
                    SubPhase = 1;
                    NPC.alpha = 150; // Semi-transparent during fakeout
                }
            }
            else if (SubPhase == 1)
            {
                // Phase 1: All clones "charge" - one is real (chosen randomly)
                int realClone = (int)(NPC.whoAmI % cloneCount); // Deterministic based on NPC ID
                float fakeoutProgress = Timer / (float)fakeoutTime;
                
                if (Timer % 4 == 0)
                {
                    for (int i = 0; i < cloneCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / cloneCount;
                        Vector2 clonePos = target.Center + angle.ToRotationVector2() * cloneRadius;
                        
                        // All clones show charging VFX
                        BossVFXOptimizer.ConvergingWarning(clonePos, 50f, fakeoutProgress, EnigmaPurple, 6);
                        
                        // Glyph circles around all clones
                        int glyphCount = 6 + difficultyTier;
                        for (int g = 0; g < glyphCount; g++)
                        {
                            float glyphAngle = MathHelper.TwoPi * g / glyphCount + Timer * 0.02f;
                            Vector2 glyphPos = clonePos + glyphAngle.ToRotationVector2() * (40f - fakeoutProgress * 15f);
                            CustomParticles.Glyph(glyphPos, EnigmaPurple * (0.3f + fakeoutProgress * 0.3f), 0.3f, -1);
                        }
                        
                        // Eyes at each clone watching player
                        if (Timer % 12 == 0)
                        {
                            CustomParticles.EnigmaEyeGaze(clonePos + Main.rand.NextVector2Circular(20f, 20f), 
                                EnigmaGreen * 0.6f, 0.35f, target.Center);
                        }
                        
                        // The "real" clone gets slightly more intense near the end
                        if (i == realClone && fakeoutProgress > 0.7f)
                        {
                            CustomParticles.GenericFlare(clonePos, EnigmaGreen * 0.5f, 0.35f, 5);
                        }
                    }
                }
                
                MagnumScreenEffects.AddScreenShake(fakeoutProgress * 2f);
                
                if (Timer >= fakeoutTime)
                {
                    Timer = 0;
                    SubPhase = 2;
                }
            }
            else if (SubPhase == 2)
            {
                // Phase 2: Real clone attacks, fakes dissipate
                int realClone = (int)(NPC.whoAmI % cloneCount);
                
                if (Timer == 1)
                {
                    MagnumScreenEffects.AddScreenShake(12f);
                    SoundEngine.PlaySound(SoundID.Item122 with { Pitch = 0.4f, Volume = 1.3f }, target.Center);
                    NPC.alpha = 0; // Boss fully visible again
                    
                    // Fake clones dissipate with particles
                    for (int i = 0; i < cloneCount; i++)
                    {
                        float angle = MathHelper.TwoPi * i / cloneCount;
                        Vector2 clonePos = target.Center + angle.ToRotationVector2() * cloneRadius;
                        
                        if (i != realClone)
                        {
                            // Fake clone dissipation
                            CustomParticles.GenericFlare(clonePos, EnigmaPurple * 0.5f, 0.4f, 15);
                            CustomParticles.GlyphBurst(clonePos, EnigmaPurple, 4, 3f);
                        }
                        else
                        {
                            // Real clone attack burst
                            BossVFXOptimizer.AttackReleaseBurst(clonePos, EnigmaPurple, EnigmaGreen, 1.3f);
                            
                            // Fire projectiles from real clone position toward player
                            if (Main.netMode != NetmodeID.MultiplayerClient)
                            {
                                int projCount = 10 + difficultyTier * 3;
                                for (int p = 0; p < projCount; p++)
                                {
                                    float projAngle = MathHelper.TwoPi * p / projCount;
                                    Vector2 vel = projAngle.ToRotationVector2() * (12f + difficultyTier * 2f);
                                    
                                    // Mix types
                                    if (p % 3 == 0)
                                        BossProjectileHelper.SpawnAcceleratingBolt(clonePos, vel * 0.7f, 80 + difficultyTier * 5, EnigmaGreen, 14f);
                                    else
                                        BossProjectileHelper.SpawnHostileOrb(clonePos, vel, 80 + difficultyTier * 5, EnigmaPurple, 0.02f);
                                }
                            }
                            
                            // Glyph cascade
                            CustomParticles.GlyphBurst(clonePos, EnigmaGreen, 12, 6f);
                            
                            // Eyes scatter
                            for (int e = 0; e < 5; e++)
                            {
                                Vector2 eyePos = clonePos + Main.rand.NextVector2Circular(40f, 40f);
                                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaGreen, 0.45f, eyePos + Main.rand.NextVector2Unit() * 80f);
                            }
                        }
                    }
                }
                
                if (Timer >= attackTime)
                {
                    EndAttack();
                }
            }
        }
        
        private void EndAttack()
        {
            Timer = 0;
            SubPhase = 0;
            State = BossPhase.Recovery;
            attackCooldown = AttackWindowFrames - difficultyTier * 12;
        }
        
        #endregion
        
        #region Visuals
        
        private void UpdateAnimation()
        {
            int frameSpeed = difficultyTier == 2 ? 2 : 3;
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
            
            // Void particles - reduce under load
            int voidChance = isHighLoad ? (8 - difficultyTier) : (5 - difficultyTier);
            if (Main.rand.NextBool(voidChance))
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(NPC.width * 0.5f, NPC.height * 0.4f);
                Vector2 vel = Main.rand.NextVector2Circular(1f, 1f);
                Color color = Main.rand.NextBool() ? EnigmaPurple : EnigmaGreen;
                CustomParticles.GenericFlare(pos, color * 0.7f, 0.28f, Main.rand.Next(15, 25));
            }
            
            // Glyphs - reduce under load
            int glyphChance = isHighLoad ? (25 - difficultyTier * 3) : (15 - difficultyTier * 3);
            if (Main.rand.NextBool(glyphChance))
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.Glyph(pos, EnigmaPurple * 0.6f, 0.3f, -1);
            }
            
            // Eyes - skip under high load
            if (!isHighLoad && difficultyTier >= 1 && Main.rand.NextBool(25 - difficultyTier * 5))
            {
                Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.5f, 0.35f, Main.player[NPC.target].Center);
            }
            
            // Enrage particles - reduce under load
            if (isEnraged && Timer % (isHighLoad ? 6 : 3) == 0)
            {
                Vector2 pos = NPC.Center + Main.rand.NextVector2Circular(60f, 60f);
                CustomParticles.GenericFlare(pos, EnigmaGreen, 0.4f, 10);
            }
        }
        
        private void UpdateDeathAnimation()
        {
            deathTimer++;
            NPC.velocity *= 0.95f;
            
            if (deathTimer % 8 == 0)
            {
                float progress = deathTimer / 180f;
                MagnumScreenEffects.AddScreenShake(4f + progress * 18f);
                
                Vector2 burstPos = NPC.Center + Main.rand.NextVector2Circular(50f * (1f - progress * 0.4f), 50f * (1f - progress * 0.4f));
                Color burstColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress);
                CustomParticles.GenericFlare(burstPos, burstColor, 0.5f + progress * 0.5f, 16);
                CustomParticles.HaloRing(burstPos, EnigmaPurple, 0.3f + progress * 0.3f, 12);
                CustomParticles.GlyphBurst(burstPos, EnigmaPurple, (int)(3 + progress * 6), 4f);
                
                for (int i = 0; i < 2; i++)
                {
                    Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(70f, 70f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple * 0.6f, 0.4f, eyePos + Main.rand.NextVector2Unit() * 50f);
                }
            }
            
            if (deathTimer >= 180)
            {
                MagnumScreenEffects.AddScreenShake(30f);
                SoundEngine.PlaySound(SoundID.NPCDeath52 with { Pitch = -0.5f, Volume = 1.8f }, NPC.Center);
                
                CustomParticles.GenericFlare(NPC.Center, Color.White, 2f, 35);
                
                for (int i = 0; i < 12; i++)
                {
                    float scale = 0.35f + i * 0.18f;
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(EnigmaPurple, EnigmaGreen, i / 12f), scale, 25 + i * 4);
                }
                
                for (int i = 0; i < 20; i++)
                {
                    float angle = MathHelper.TwoPi * i / 20f;
                    CustomParticles.GenericFlare(NPC.Center + angle.ToRotationVector2() * 100f, EnigmaPurple, 0.75f, 30);
                }
                
                CustomParticles.GlyphBurst(NPC.Center, EnigmaGreen, 20, 12f);
                
                for (int i = 0; i < 16; i++)
                {
                    Vector2 eyePos = NPC.Center + Main.rand.NextVector2Circular(100f, 100f);
                    CustomParticles.EnigmaEyeGaze(eyePos, EnigmaPurple, 0.6f, eyePos + Main.rand.NextVector2Unit() * 150f);
                }
                
                // Death dialogue
                BossDialogueSystem.Enigma.OnDeath();
                BossDialogueSystem.CleanupDialogue(NPC.whoAmI);
                
                // Deactivate the Enigma mystery sky effect
                if (!Main.dedServ && SkyManager.Instance["MagnumOpus:EnigmaSky"] != null)
                {
                    SkyManager.Instance.Deactivate("MagnumOpus:EnigmaSky");
                }
                
                // Set boss downed flag for miniboss essence drops
                MoonlightSonataSystem.DownedEnigma = true;
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
            
            if (NPC.velocity.Length() > 10f || isEnraged)
            {
                for (int i = 0; i < NPC.oldPos.Length; i++)
                {
                    float progress = (float)i / NPC.oldPos.Length;
                    Color trailColor = Color.Lerp(EnigmaPurple, EnigmaGreen, progress) * (1f - progress) * 0.4f;
                    Vector2 trailPos = NPC.oldPos[i] + NPC.Size / 2f - screenPos;
                    spriteBatch.Draw(tex, trailPos, sourceRect, trailColor, NPC.rotation, origin, NPC.scale * (1f - progress * 0.12f), effects, 0f);
                }
            }
            
            float pulse = (float)Math.Sin(Timer * 0.08f) * 0.3f + 0.7f;
            Color glowColor = isEnraged ? EnigmaGreen : Color.Lerp(EnigmaPurple, EnigmaGreen, pulse);
            glowColor.A = 0;
            spriteBatch.Draw(tex, drawPos, sourceRect, glowColor * 0.35f, NPC.rotation, origin, NPC.scale * 1.1f, effects, 0f);
            
            Color mainColor = NPC.IsABestiaryIconDummy ? Color.White : Lighting.GetColor((int)(NPC.Center.X / 16), (int)(NPC.Center.Y / 16));
            mainColor = Color.Lerp(mainColor, Color.White, 0.3f);
            spriteBatch.Draw(tex, drawPos, sourceRect, mainColor * ((255 - NPC.alpha) / 255f), NPC.rotation, origin, NPC.scale, effects, 0f);
            
            return false;
        }
        
        #endregion
        
        #region Loot
        
        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<EnigmaTreasureBag>()));
            
            LeadingConditionRule notExpert = new LeadingConditionRule(new Conditions.NotExpert());
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<EnigmaResonantEnergy>(), 1, 15, 25));
            notExpert.OnSuccess(ItemDropRule.Common(ModContent.ItemType<HarmonicCoreOfEnigma>(), 1, 1, 1));
            
            notExpert.OnSuccess(ItemDropRule.OneFromOptions(1,
                ModContent.ItemType<VariationsOfTheVoid>(),
                ModContent.ItemType<TheUnresolvedCadence>(),
                ModContent.ItemType<DissonanceOfSecrets>(),
                ModContent.ItemType<CipherNocturne>(),
                ModContent.ItemType<FugueOfTheUnknown>()));
            
            notExpert.OnSuccess(ItemDropRule.OneFromOptions(2,
                ModContent.ItemType<TheWatchingRefrain>(),
                ModContent.ItemType<TheSilentMeasure>(),
                ModContent.ItemType<TacetsEnigma>()));
            
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
