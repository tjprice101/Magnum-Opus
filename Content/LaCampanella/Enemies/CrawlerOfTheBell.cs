using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using MagnumOpus.Content.LaCampanella.ResonanceEnergies;
using MagnumOpus.Content.LaCampanella.HarmonicCores;
using MagnumOpus.Common.Systems;
using MagnumOpus.Common.Systems.Particles;

namespace MagnumOpus.Content.LaCampanella.Enemies
{
    /// <summary>
    /// Crawler of the Bell - A fiery underground desert mini-boss from the La Campanella theme.
    /// Spawns in underground deserts after Moon Lord is defeated (5% chance).
    /// 
    /// 5 FLAMING ATTACKS:
    /// 1. Bell Toll Flames - Rings of fire that expand outward like sound waves
    /// 2. Infernal Crawl - Leaves fire trail while charging at player
    /// 3. Smoke Burst - Releases thick smoke clouds with embedded fireballs
    /// 4. Chime Pillars - Summons pillars of flame from below
    /// 5. Crescendo Inferno - Ultimate fire explosion with bell chime
    /// </summary>
    public class CrawlerOfTheBell : ModNPC
    {
        private enum AIState
        {
            Idle,
            Crawling,
            BellTollFlames,     // Attack 1 - expanding fire rings
            InfernalCrawl,      // Attack 2 - charging with fire trail
            SmokeBurst,         // Attack 3 - smoke clouds with fireballs
            ChimePillars,       // Attack 4 - fire pillars from ground
            CrescendoInferno    // Attack 5 - ultimate explosion
        }

        private AIState CurrentState
        {
            get => (AIState)NPC.ai[0];
            set => NPC.ai[0] = (float)value;
        }

        private float StateTimer
        {
            get => NPC.ai[1];
            set => NPC.ai[1] = value;
        }

        private float AttackCooldown
        {
            get => NPC.ai[2];
            set => NPC.ai[2] = value;
        }

        private int AttackCounter
        {
            get => (int)NPC.ai[3];
            set => NPC.ai[3] = value;
        }

        // Animation - 6x6 sprite sheet (36 frames)
        private int frameCounter = 0;
        private int currentFrame = 0;
        private const int FrameTime = 5;
        private const int FrameColumns = 6;
        private const int FrameRows = 6;
        private const int TotalFrames = 36;

        // Movement tracking
        private int lastSpriteDirection = -1;
        private bool isMoving = false;

        // La Campanella Colors - Black to Orange with smoky effects
        private static readonly Color CampanellaBlack = new Color(20, 15, 20);
        private static readonly Color CampanellaOrange = new Color(255, 100, 0);
        private static readonly Color CampanellaYellow = new Color(255, 200, 50);
        private static readonly Color CampanellaGold = new Color(218, 165, 32);

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = TotalFrames;

            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Poisoned] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Confused] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.OnFire] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.CursedInferno] = true;
            NPCID.Sets.SpecificDebuffImmunity[Type][BuffID.Burning] = true;

            NPCID.Sets.DangerDetectRange[Type] = 700;
            
            // Boss map icon
            NPCID.Sets.BossBestiaryPriority.Add(Type);
        }

        public override void SetDefaults()
        {
            // MINI-BOSS STATS - Reduced size by 50%
            NPC.width = 90;  // Was 180
            NPC.height = 60; // Was 120
            NPC.damage = 150;
            NPC.defense = 70;
            NPC.lifeMax = 55000;
            NPC.HitSound = SoundID.NPCHit41;
            NPC.DeathSound = SoundID.NPCDeath43;
            NPC.knockBackResist = 0f;
            NPC.value = Item.buyPrice(gold: 35);
            NPC.aiStyle = -1;
            NPC.npcSlots = 5f;
            NPC.scale = 0.5f; // 50% scale

            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.lavaImmune = true;

            NPC.boss = false; // Mini-boss, not full boss
            
            // DrawOffsetY positions the sprite relative to the hitbox
            // Positive values move sprite UP (useful when sprite's visual bottom is lower than hitbox bottom)
            // The sprite is 6x6 frames at scale 0.5, so we need to ensure sprite bottom aligns with hitbox bottom
            DrawOffsetY = 0f; // Reset - sprite bottom should match hitbox bottom
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                BestiaryDatabaseNPCsPopulator.CommonTags.SpawnConditions.Biomes.UndergroundDesert,
                new FlavorTextBestiaryInfoElement("Crawler of the Bell - " +
                    "A blazing creature that emerged from the deepest sands, " +
                    "its body resonating with the infernal chimes of La Campanella. " +
                    "Where it crawls, flame follows like a funeral procession.")
            });
        }

        public override void ApplyDifficultyAndPlayerScaling(int numPlayers, float balance, float bossAdjustment)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.8f * balance);
            NPC.damage = (int)(NPC.damage * 0.7f);
        }

        public override void AI()
        {
            Player target = Main.player[NPC.target];

            // Check if moving
            float movementThreshold = 0.5f;
            isMoving = Math.Abs(NPC.velocity.X) > movementThreshold;

            if (isMoving && target.active)
            {
                lastSpriteDirection = target.Center.X > NPC.Center.X ? 1 : -1;
            }
            NPC.spriteDirection = lastSpriteDirection;

            // Intense fiery glow
            Lighting.AddLight(NPC.Center, CampanellaOrange.ToVector3() * 0.8f);

            // Ambient particles - smoky flames
            SpawnAmbientParticles();

            // Retarget
            NPC.TargetClosest(true);
            target = Main.player[NPC.target];

            // Despawn check
            if (!target.active || target.dead)
            {
                NPC.velocity.Y += 0.1f;
                if (NPC.timeLeft > 60)
                    NPC.timeLeft = 60;
                return;
            }

            float distanceToTarget = Vector2.Distance(NPC.Center, target.Center);

            // Update timers
            StateTimer++;
            if (AttackCooldown > 0f)
                AttackCooldown--;

            // Select attack when cooldown done
            if (AttackCooldown <= 0f && distanceToTarget < 600f &&
                CurrentState == AIState.Idle || CurrentState == AIState.Crawling)
            {
                SelectNextAttack(target, distanceToTarget);
            }

            switch (CurrentState)
            {
                case AIState.Idle:
                case AIState.Crawling:
                    HandleCrawling(target, distanceToTarget);
                    break;
                case AIState.BellTollFlames:
                    HandleBellTollFlames(target);
                    break;
                case AIState.InfernalCrawl:
                    HandleInfernalCrawl(target);
                    break;
                case AIState.SmokeBurst:
                    HandleSmokeBurst(target);
                    break;
                case AIState.ChimePillars:
                    HandleChimePillars(target);
                    break;
                case AIState.CrescendoInferno:
                    HandleCrescendoInferno(target);
                    break;
            }

            // Animation
            UpdateAnimation();
        }

        private void SpawnAmbientParticles()
        {
            // Smoky trail
            if (Main.rand.NextBool(4))
            {
                Dust smoke = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, -1f, 150, CampanellaBlack, 1.5f);
                smoke.noGravity = true;
                smoke.velocity *= 0.5f;
            }

            // Orange embers
            if (Main.rand.NextBool(6))
            {
                Dust ember = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 
                    Main.rand.NextFloat(-1f, 1f), -2f, 100, CampanellaOrange, 1.8f);
                ember.noGravity = true;
            }

            // Themed particles
            if (Main.rand.NextBool(10))
            {
                ThemedParticles.LaCampanellaAura(NPC.Center, NPC.width * 0.4f);
            }
        }

        private void SelectNextAttack(Player target, float distance)
        {
            AttackCounter++;
            int attackChoice = AttackCounter % 5;

            // Play bell chime sound for attack telegraphing
            SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.3f, Volume = 0.8f }, NPC.Center);

            switch (attackChoice)
            {
                case 0: // Bell Toll Flames
                    CurrentState = AIState.BellTollFlames;
                    StateTimer = 0f;
                    break;

                case 1: // Infernal Crawl
                    CurrentState = AIState.InfernalCrawl;
                    StateTimer = 0f;
                    break;

                case 2: // Smoke Burst
                    CurrentState = AIState.SmokeBurst;
                    StateTimer = 0f;
                    break;

                case 3: // Chime Pillars
                    CurrentState = AIState.ChimePillars;
                    StateTimer = 0f;
                    break;

                case 4: // Crescendo Inferno - more likely when low health
                    if (NPC.life < NPC.lifeMax * 0.4f || Main.rand.NextBool(3))
                    {
                        CurrentState = AIState.CrescendoInferno;
                        StateTimer = 0f;
                        SoundEngine.PlaySound(SoundID.Item119, NPC.Center);
                    }
                    else
                    {
                        CurrentState = AIState.BellTollFlames;
                        StateTimer = 0f;
                    }
                    break;
            }
        }

        private void HandleCrawling(Player target, float distance)
        {
            float moveSpeed = 4f;
            float accel = 0.15f;

            if (distance > 100f)
            {
                if (target.Center.X > NPC.Center.X)
                    NPC.velocity.X = Math.Min(NPC.velocity.X + accel, moveSpeed);
                else
                    NPC.velocity.X = Math.Max(NPC.velocity.X - accel, -moveSpeed);
            }
            else
            {
                NPC.velocity.X *= 0.9f;
            }

            // Jump if blocked
            if (NPC.collideX && NPC.velocity.Y == 0f)
            {
                NPC.velocity.Y = -10f;
                CreateJumpEffect();
            }

            CurrentState = AIState.Crawling;
        }

        #region Attack 1: Bell Toll Flames
        private void HandleBellTollFlames(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Fire a spread of flames aimed at the player in waves
            if (StateTimer < 90f)
            {
                // Every 18 frames, fire a burst of flames toward the player
                if (StateTimer % 18 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 toPlayer = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                    float baseAngle = toPlayer.ToRotation();
                    float spreadAngle = MathHelper.ToRadians(45f); // 45 degree spread
                    int flameCount = 5; // 5 flames per burst
                    float flameSpeed = 8f + StateTimer * 0.03f;
                    
                    for (int i = 0; i < flameCount; i++)
                    {
                        // Fan spread aimed at player
                        float angleOffset = MathHelper.Lerp(-spreadAngle, spreadAngle, (float)i / (flameCount - 1));
                        Vector2 velocity = (baseAngle + angleOffset).ToRotationVector2() * flameSpeed;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<BellTollFlame>(), NPC.damage / 3, 1f, Main.myPlayer);
                    }

                    // Bell toll VFX - impact at spawn point
                    ThemedParticles.LaCampanellaImpact(NPC.Center, 0.8f);
                    
                    // Fire embers burst outward
                    for (int i = 0; i < 8; i++)
                    {
                        float emberAngle = baseAngle + MathHelper.Lerp(-MathHelper.PiOver4, MathHelper.PiOver4, Main.rand.NextFloat());
                        Vector2 emberVel = emberAngle.ToRotationVector2() * Main.rand.NextFloat(3f, 6f);
                        var ember = new GenericGlowParticle(NPC.Center, emberVel, CampanellaOrange, 0.35f, 20, true);
                        MagnumParticleHandler.SpawnParticle(ember);
                    }
                    
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = -0.5f + StateTimer * 0.005f }, NPC.Center);
                }
            }
            else
            {
                EndAttack(90f);
            }
        }
        #endregion

        #region Attack 2: Infernal Crawl
        private void HandleInfernalCrawl(Player target)
        {
            // Charge at player leaving fire trail
            if (StateTimer < 10f)
            {
                // Windup
                NPC.velocity *= 0.9f;
                
                // Charging VFX
                float progress = StateTimer / 10f;
                CustomParticles.GenericFlare(NPC.Center, CampanellaOrange, 0.3f + progress * 0.5f, 15);
            }
            else if (StateTimer < 70f)
            {
                // Charge!
                Vector2 toTarget = (target.Center - NPC.Center).SafeNormalize(Vector2.UnitX);
                NPC.velocity = toTarget * 12f;
                NPC.noTileCollide = true;

                // Fire trail
                if (StateTimer % 3 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero,
                        ModContent.ProjectileType<InfernalCrawlTrail>(), NPC.damage / 4, 0f, Main.myPlayer);
                }

                // Charge particles
                for (int i = 0; i < 3; i++)
                {
                    Dust flame = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch, 
                        -NPC.velocity.X * 0.2f, -NPC.velocity.Y * 0.2f, 100, CampanellaOrange, 2f);
                    flame.noGravity = true;
                }
            }
            else
            {
                NPC.noTileCollide = false;
                EndAttack(60f);
            }
        }
        #endregion

        #region Attack 3: Smoke Burst
        private void HandleSmokeBurst(Player target)
        {
            NPC.velocity.X *= 0.85f;

            // Release smoke clouds with embedded fireballs
            if (StateTimer == 20f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Spawn smoke clouds
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 8f + Main.rand.NextFloat(-0.2f, 0.2f);
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * Main.rand.NextFloat(3f, 6f);
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<SmokeCloudProjectile>(), NPC.damage / 4, 1f, Main.myPlayer);
                    }
                }

                // Explosion VFX
                ThemedParticles.LaCampanellaImpact(NPC.Center, 1.5f);
                
                // Lots of smoke dust
                for (int i = 0; i < 30; i++)
                {
                    Dust smoke = Dust.NewDustDirect(NPC.Center, 0, 0, DustID.Smoke, 
                        Main.rand.NextFloat(-8f, 8f), Main.rand.NextFloat(-8f, 8f), 200, CampanellaBlack, 2.5f);
                    smoke.noGravity = true;
                }
                
                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            }

            if (StateTimer >= 50f)
            {
                EndAttack(80f);
            }
        }
        #endregion

        #region Attack 4: Chime Pillars
        private void HandleChimePillars(Player target)
        {
            NPC.velocity.X *= 0.9f;

            // Summon fire pillars from below player
            if (StateTimer < 90f)
            {
                if (StateTimer % 18 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Telegraph on ground below player
                    Vector2 pillarPos = new Vector2(target.Center.X + Main.rand.NextFloat(-150f, 150f), target.position.Y + target.height);
                    
                    // Find ground
                    for (int y = 0; y < 20; y++)
                    {
                        Point tilePos = new Vector2(pillarPos.X, pillarPos.Y + y * 16).ToTileCoordinates();
                        if (WorldGen.SolidTile(tilePos.X, tilePos.Y))
                        {
                            pillarPos.Y = tilePos.Y * 16 - 8;
                            break;
                        }
                    }

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), pillarPos, Vector2.Zero,
                        ModContent.ProjectileType<ChimePillar>(), NPC.damage / 3, 2f, Main.myPlayer);

                    // Chime sound
                    SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.2f }, pillarPos);
                }
            }
            else
            {
                EndAttack(100f);
            }
        }
        #endregion

        #region Attack 5: Crescendo Inferno
        private void HandleCrescendoInferno(Player target)
        {
            NPC.velocity *= 0.95f;

            if (StateTimer < 60f)
            {
                // Buildup phase - gathering fire
                float progress = StateTimer / 60f;
                
                // Inward pulling particles
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = Main.rand.NextVector2CircularEdge(100f + (1f - progress) * 100f, 100f + (1f - progress) * 100f);
                    Vector2 particlePos = NPC.Center + offset;
                    Vector2 velocity = (NPC.Center - particlePos).SafeNormalize(Vector2.Zero) * 4f;
                    
                    Dust pull = Dust.NewDustDirect(particlePos, 0, 0, DustID.Torch, velocity.X, velocity.Y, 100, CampanellaOrange, 1.5f);
                    pull.noGravity = true;
                }

                // Growing core
                CustomParticles.GenericFlare(NPC.Center, CampanellaOrange, 0.3f + progress * 0.8f, 20);
                ThemedParticles.LaCampanellaSparkles(NPC.Center, (int)(progress * 5) + 1, 50f);
            }
            else if (StateTimer == 60f)
            {
                // EXPLOSION!
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    // Massive fireball barrage
                    for (int i = 0; i < 24; i++)
                    {
                        float angle = MathHelper.TwoPi * i / 24f;
                        float speed = 8f + Main.rand.NextFloat(4f);
                        Vector2 velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * speed;
                        
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity,
                            ModContent.ProjectileType<CrescendoFireball>(), NPC.damage / 3, 2f, Main.myPlayer);
                    }
                }

                // Massive VFX
                UnifiedVFX.LaCampanella.Explosion(NPC.Center, 2f);
                CustomParticles.ExplosionBurst(NPC.Center, CampanellaOrange, 30, 15f);
                CustomParticles.ExplosionBurst(NPC.Center, CampanellaYellow, 20, 12f);
                
                // Halo rings
                for (int i = 0; i < 5; i++)
                {
                    CustomParticles.HaloRing(NPC.Center, Color.Lerp(CampanellaOrange, CampanellaYellow, i / 5f), 0.5f + i * 0.2f, 20 + i * 5);
                }

                SoundEngine.PlaySound(SoundID.Item119 with { Pitch = -0.2f }, NPC.Center);
                SoundEngine.PlaySound(SoundID.Item35 with { Pitch = 0.5f, Volume = 1.2f }, NPC.Center);
            }

            if (StateTimer >= 90f)
            {
                EndAttack(150f);
            }
        }
        #endregion

        private void EndAttack(float cooldown)
        {
            CurrentState = AIState.Crawling;
            StateTimer = 0f;
            AttackCooldown = cooldown;
        }

        private void CreateJumpEffect()
        {
            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustDirect(NPC.Bottom - new Vector2(20, 0), 40, 8, DustID.Torch,
                    Main.rand.NextFloat(-3f, 3f), -2f, 100, CampanellaOrange, 1.5f);
                fire.noGravity = true;
            }
            ThemedParticles.LaCampanellaImpact(NPC.Bottom, 0.6f);
        }

        private void UpdateAnimation()
        {
            int speed = isMoving ? 4 : 8;
            
            frameCounter++;
            if (frameCounter >= speed)
            {
                frameCounter = 0;
                currentFrame++;
                if (currentFrame >= TotalFrames)
                    currentFrame = 0;
            }
        }

        public override void FindFrame(int frameHeight)
        {
            // For 6x6 sprite sheets, we handle frame selection manually in PreDraw
            // Set a placeholder frame - actual frame calculation happens in PreDraw
            // frameHeight here is the full texture height, not per-frame height
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            // Hit particles
            for (int i = 0; i < 8; i++)
            {
                Dust fire = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch,
                    hit.HitDirection * 2f, -2f, 100, CampanellaOrange, 1.8f);
                fire.noGravity = true;
            }

            // Death effects
            if (NPC.life <= 0)
            {
                // Massive death explosion
                UnifiedVFX.LaCampanella.Explosion(NPC.Center, 2.5f);
                
                for (int i = 0; i < 50; i++)
                {
                    Dust death = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Torch,
                        Main.rand.NextFloat(-10f, 10f), Main.rand.NextFloat(-10f, 10f), 100, CampanellaOrange, 2.5f);
                    death.noGravity = true;
                }

                for (int i = 0; i < 30; i++)
                {
                    Dust smoke = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Smoke,
                        Main.rand.NextFloat(-6f, 6f), Main.rand.NextFloat(-6f, 6f), 200, CampanellaBlack, 3f);
                    smoke.noGravity = true;
                }

                SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
                SoundEngine.PlaySound(SoundID.NPCDeath43, NPC.Center);
            }
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            // Mini-boss tier drops - matching other themes
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LaCampanellaResonantEnergy>(), 1, 8, 15));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<ResonantCoreOfLaCampanella>(), 1, 3, 6));
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            // Mini-boss - 5% spawn rate in underground desert after Moon Lord
            if (!NPC.downedMoonlord)
                return 0f;

            if (spawnInfo.Player.ZoneUndergroundDesert && !spawnInfo.PlayerSafe)
            {
                return 0.05f;
            }

            return 0f;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            
            // Calculate frame from 6x6 sprite sheet
            int frameWidth = texture.Width / FrameColumns;
            int frameHeight = texture.Height / FrameRows;
            int frameX = currentFrame % FrameColumns;
            int frameY = currentFrame / FrameColumns;
            Rectangle sourceRect = new Rectangle(frameX * frameWidth, frameY * frameHeight, frameWidth, frameHeight);
            
            // For ground-based crawlers, the sprite's bottom should align with the hitbox bottom
            // Use NPC.Bottom for positioning and set origin at bottom-center of frame
            Vector2 drawPos = NPC.Bottom - screenPos;
            Vector2 origin = new Vector2(frameWidth / 2, frameHeight); // Bottom-center origin
            SpriteEffects effects = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            // Fiery glow underlay
            Color glowColor = CampanellaOrange * 0.3f;
            spriteBatch.Draw(texture, drawPos, sourceRect, glowColor, NPC.rotation, origin, NPC.scale * 1.05f, effects, 0f);

            // Main draw
            spriteBatch.Draw(texture, drawPos, sourceRect, drawColor, NPC.rotation, origin, NPC.scale, effects, 0f);

            return false;
        }
    }

    #region Projectiles

    public class BellTollFlame : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private static readonly Color CampanellaOrange = new Color(255, 100, 0);

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 3;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = false;
            Projectile.alpha = 50;
        }

        public override void AI()
        {
            // Slow down over time
            Projectile.velocity *= 0.98f;

            // Trail
            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, 
                    DustID.Torch, 0f, 0f, 100, CampanellaOrange, 1.5f);
                flame.noGravity = true;
                flame.velocity *= 0.3f;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.5f);

            // Fade out
            if (Projectile.timeLeft < 30)
                Projectile.alpha += 8;
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange, 0.4f, 15);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;

            Main.spriteBatch.Draw(glow, pos, null, CampanellaOrange * alpha, 0f, glow.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, Color.White * alpha * 0.5f, 0f, glow.Size() / 2, 0.25f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class InfernalCrawlTrail : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private static readonly Color CampanellaOrange = new Color(255, 100, 0);

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            if (Main.rand.NextBool(3))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, -1f, 100, CampanellaOrange, 2f);
                flame.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.4f);

            // Fade
            Projectile.alpha += 4;
            if (Projectile.alpha >= 255)
                Projectile.Kill();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;

            Main.spriteBatch.Draw(glow, pos, null, CampanellaOrange * alpha * 0.6f, 0f, glow.Size() / 2, 0.8f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class SmokeCloudProjectile : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private static readonly Color CampanellaBlack = new Color(20, 15, 20);
        private static readonly Color CampanellaOrange = new Color(255, 100, 0);

        public override void SetDefaults()
        {
            Projectile.width = 50;
            Projectile.height = 50;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.96f;

            // Smoke particles
            if (Main.rand.NextBool(3))
            {
                Dust smoke = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Smoke, Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f), 200, CampanellaBlack, 2f);
                smoke.noGravity = true;
            }

            // Occasional fireball burst
            if (Projectile.timeLeft % 30 == 0 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 fireVel = Main.rand.NextVector2Circular(4f, 4f);
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center, fireVel,
                    ModContent.ProjectileType<SmokeFireball>(), Projectile.damage / 2, 1f, Main.myPlayer);
            }

            // Fade
            if (Projectile.timeLeft < 60)
                Projectile.alpha += 4;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;
            float alpha = 1f - Projectile.alpha / 255f;

            Main.spriteBatch.Draw(glow, pos, null, CampanellaBlack * alpha * 0.8f, 0f, glow.Size() / 2, 1.2f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class SmokeFireball : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        private static readonly Color CampanellaOrange = new Color(255, 100, 0);

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.1f;

            if (Main.rand.NextBool(2))
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, 0f, 0f, 100, CampanellaOrange, 1.2f);
                flame.noGravity = true;
            }

            Projectile.rotation += 0.2f;
            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.3f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange, 0.3f, 12);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(glow, pos, null, CampanellaOrange, Projectile.rotation, glow.Size() / 2, 0.3f, SpriteEffects.None, 0f);

            return false;
        }
    }

    public class ChimePillar : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/SoftGlow";

        private static readonly Color CampanellaOrange = new Color(255, 100, 0);
        private static readonly Color CampanellaYellow = new Color(255, 200, 50);

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 200;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 90;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // Telegraph phase
            if (Projectile.ai[0] < 30f)
            {
                Projectile.ai[0]++;
                
                // Warning particles
                if (Projectile.ai[0] % 5 == 0)
                {
                    Dust warn = Dust.NewDustDirect(new Vector2(Projectile.Center.X - 20, Projectile.position.Y), 40, 10,
                        DustID.Torch, 0f, -2f, 100, CampanellaOrange, 1.5f);
                    warn.noGravity = true;
                }

                return;
            }

            // Eruption!
            if (Projectile.ai[0] == 30f)
            {
                Projectile.alpha = 0;
                SoundEngine.PlaySound(SoundID.Item74 with { Pitch = 0.3f }, Projectile.Center);
            }

            Projectile.ai[0]++;

            // Pillar particles
            for (int i = 0; i < 5; i++)
            {
                Vector2 dustPos = new Vector2(Projectile.Center.X + Main.rand.NextFloat(-20f, 20f), 
                    Projectile.position.Y + Main.rand.NextFloat(Projectile.height));
                Dust pillar = Dust.NewDustDirect(dustPos, 0, 0, DustID.Torch, 0f, -3f, 100, CampanellaOrange, 2f);
                pillar.noGravity = true;
            }

            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.6f);

            // Fade out
            if (Projectile.timeLeft < 20)
                Projectile.alpha += 12;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.alpha >= 255) return false;

            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/SoftGlow").Value;
            float alpha = 1f - Projectile.alpha / 255f;

            // Draw pillar as stretched glow
            for (int i = 0; i < 10; i++)
            {
                Vector2 pos = new Vector2(Projectile.Center.X, Projectile.position.Y + i * 20) - Main.screenPosition;
                Color col = Color.Lerp(CampanellaOrange, CampanellaYellow, i / 10f) * alpha;
                Main.spriteBatch.Draw(glow, pos, null, col * 0.8f, 0f, glow.Size() / 2, new Vector2(0.6f, 0.4f), SpriteEffects.None, 0f);
            }

            return false;
        }
    }

    public class CrescendoFireball : ModProjectile
    {
        public override string Texture => "MagnumOpus/Assets/Particles/EnergyFlare";

        private static readonly Color CampanellaOrange = new Color(255, 100, 0);
        private static readonly Color CampanellaYellow = new Color(255, 200, 50);

        public override void SetDefaults()
        {
            Projectile.width = 18;
            Projectile.height = 18;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = true;
        }

        public override void AI()
        {
            // Trail
            for (int i = 0; i < 2; i++)
            {
                Dust flame = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, -Projectile.velocity.X * 0.2f, -Projectile.velocity.Y * 0.2f, 100, CampanellaOrange, 1.8f);
                flame.noGravity = true;
            }

            Projectile.rotation = Projectile.velocity.ToRotation();
            Lighting.AddLight(Projectile.Center, CampanellaOrange.ToVector3() * 0.5f);
        }

        public override void OnKill(int timeLeft)
        {
            CustomParticles.GenericFlare(Projectile.Center, CampanellaOrange, 0.5f, 18);
            CustomParticles.GenericFlare(Projectile.Center, CampanellaYellow, 0.3f, 15);
            SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.5f }, Projectile.Center);

            for (int i = 0; i < 10; i++)
            {
                Dust explode = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height,
                    DustID.Torch, Main.rand.NextFloat(-4f, 4f), Main.rand.NextFloat(-4f, 4f), 100, CampanellaOrange, 1.5f);
                explode.noGravity = true;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D glow = ModContent.Request<Texture2D>("MagnumOpus/Assets/Particles/EnergyFlare").Value;
            Vector2 pos = Projectile.Center - Main.screenPosition;

            Main.spriteBatch.Draw(glow, pos, null, CampanellaOrange, Projectile.rotation, glow.Size() / 2, 0.5f, SpriteEffects.None, 0f);
            Main.spriteBatch.Draw(glow, pos, null, CampanellaYellow * 0.5f, Projectile.rotation, glow.Size() / 2, 0.3f, SpriteEffects.None, 0f);

            return false;
        }
    }

    #endregion
}
